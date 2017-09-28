// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
// #define TRACE_NAVIGATE

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
    // OrderableAI subsumes all close-to-normal livings.  The Police Faction AI needs the following capabilities
    // * sweep district for threat
    // * explore
    // * pick up Y at X
    // * do Y at X (e.g., turn on generators)
    // * drop Y at X
    // * do Y at X at time T (e.g., sleep; be in position for invasion)

    // examples for actor objective stack, which is checked only when no enemies in sight:
    // Be at Location at time t
    // Have Location in view at time t (probably best to delegate this)
    // get item type at time t
    // sleep (on bed) at time t with food value >= and sanity value >=
    // * reversed-sense of these inequalities may also be of use
    // Engine.AI.Percept looks related, but reverse-engineering the required action is problematic
    [Serializable]
    internal abstract class Objective
    {
      protected int turn;   // turn count of WorldTime .. will need a more complex representation at some point.
      protected readonly Actor m_Actor;   // owning actor is likely important
      protected bool _isExpired;

      public int TurnCounter { get { return turn; } }
      public bool IsExpired { get { return _isExpired; } }
      public Actor Actor { get { return m_Actor; } }

      protected Objective(int t0, Actor who)
      {
         Contract.Requires(null != who);
         turn = t0;
         m_Actor = who;
      }

      // convention: return true to take action; ret null if self-cancellation indicated.
      public abstract bool UrgentAction(out ActorAction ret);

      public virtual List<Objective> Subobjectives() { return null; }
    }

    // workaround for breaking action loops
    [Serializable]
    internal class Goal_NextAction : Objective
    {
      public readonly ActorAction Intent;

      public Goal_NextAction(int t0, Actor who, ActorAction intent)
      : base(t0,who)
      {
        Contract.Requires(null != intent);
        Intent = intent;
      }

      // always execute.  Expire on execution
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (Intent.IsLegal()) ret = Intent;
        _isExpired = true;
        return true;
      }
    }

    [Serializable]
    internal class Goal_LastAction<T> : Objective
    {
      public readonly T Intent;

      public Goal_LastAction(int t0, Actor who, T intent)
      : base(t0,who)
      {
        Contract.Requires(null != intent);
        Intent = intent;
      }

      public T LastAction { get {return Intent; } }

      // expire when two turns old
      // we are an influence on behaviors so we don't actually execute
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (2 <= m_Actor.Location.Map.LocalTime.TurnCounter - TurnCounter) {
          _isExpired = true;
          return true;
        }
        return false;
      }
    }

    [Serializable]
    internal class Goal_DoNotPickup : Objective
    {
      public readonly GameItems.IDs Avoid;

      public Goal_DoNotPickup(int t0, Actor who, GameItems.IDs avoid)
      : base(t0,who)
      {
        Avoid = avoid;
      }

      // never execute, as we're an influence on another behavior.
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        // expire if the offending item is not in LoS
        if (m_Actor.Controller.FOV.Select(pt => m_Actor.Location.Map.GetItemsAt(pt)).Any(inv => null!=inv && inv.Has(Avoid))) return false;
        if (m_Actor.Inventory.Has(Avoid)) return false;
        _isExpired = true;  // but expire if the offending item is not in LOS or inventory
        return false;
      }
    }

    [Serializable]
    internal class Goal_BreakLineOfSight : Objective
    {
      private HashSet<Location> _locs;

      public Goal_BreakLineOfSight(int t0, Actor who, Location loc)
      : base(t0,who)
      {
        _locs = new HashSet<Location>{loc};
      }

      public Goal_BreakLineOfSight(int t0, Actor who, IEnumerable<Location> locs)
      : base(t0,who)
      {
        _locs = new HashSet<Location>(locs);
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        IEnumerable<Point> tmp = _locs.Where(loc => loc.Map==m_Actor.Location.Map).Select(loc => loc.Position);
        if (!tmp.Any()) return true;
        tmp = tmp.Except(m_Actor.Controller.FOV);
        if (!tmp.Any()) return true;
        ret = (m_Actor.Controller as BaseAI).BehaviorWalkAwayFrom(tmp);
        return true;
      }
    }

    [Serializable]
    internal class Goal_PathTo : Objective
    {
      private readonly HashSet<Location> _locs;

      public Goal_PathTo(int t0, Actor who, Location loc)
      : base(t0,who)
      {
        _locs = new HashSet<Location>{loc};
      }

      public Goal_PathTo(int t0, Actor who, IEnumerable<Location> locs)
      : base(t0,who)
      {
        _locs = new HashSet<Location>(locs);
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (_locs.Contains(m_Actor.Location)) {
          _isExpired = true;
          return true;
        }

        ret = (m_Actor.Controller as OrderableAI).BehaviorPathTo(m => new HashSet<Point>(_locs.Where(loc => loc.Map==m).Select(loc => loc.Position)));
        if (null == ret) return false;
        if (!ret.IsLegal()) return false;
        return true;
      }
    }

#if FAIL
    [Serializable]
    public Goal_Sleep : Objective
    {
    }

    [Serializable]
    // fulfilled by one of: sleep, stimulates, non-leader ally pushing/shoving
    public Goal_Wakeup : Objective
    {
    }

    [Serializable]
    public Goal_BeThereAt : Objective
    {
      private int _start_time;
      private Map _dest_map;
      private HashSet<Point> _dest_pts = new HashSet<Point>();
//    List<Objective> _sub_goals = new List<Objective>();

      public Goal_BeThereAt(Location dest, int t0)
      : base(t0)
      {
        _dest_map = dest.Map;
        _dest_pts.Add(dest.Position);
//      _bootstrap_subgoals();
//      _calc_subgoals();
      }

      // This is "be at X at t0"; it does not imply guarding.  Rather, this is for
      // pre-positioning for the midnight invasions.
      public Goal_BeThereAt(Location src, Map dest_map, IEnumerable<Point> dest_pts, int t0, int STA_buffer=0)
      : base(t0)
      {
        _dest_map = dest_map;
        _dest_pts.UnionWith(dest_pts);
//      _bootstrap_subgoals();
//      _calc_subgoals();
      }

      bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        // if no subgoals are urgent, check pathing
        int steps = 0;
        Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps();
        if (dest.Map == m_Actor.Location.Map) {
	      navigate.GoalDistance(dest.Position,int.MaxValue,m_Actor.Location.Position);
          steps = navigate.Cost();
        } else {
          // XXX need the costs for each map in the sequence
          HashSet<Exit> valid_exits;
          HashSet<Map> exit_maps = m_Actor.Location.Map.PathTo(dest.Map, out valid_exits);

	      Exit exitAt = m_Actor.Location.Exit;
          if (exitAt != null && exit_maps.Contains(exitAt.ToMap) && m_Actor.CanUseExit(m_Actor.Location.Position))
            return new ActionUseExit(m_Actor, m_Actor.Location.Position);   // would prefer return BehaviorUseExit(game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES)
	      navigate.GoalDistance(m_Actor.Location.Map.ExitLocations(valid_exits),int.MaxValue,m_Actor.Location.Position);
          steps = navigate.Cost();
	    }
        // XXX convert steps to actual turns
        if (TurnCounter-steps>m_Actor.Location.Map.LocalTime.TurnCounter) return false;  // not urgent
        // final pathing
	    Dictionary<Point, int> tmp = navigate.Approach(m_Actor.Location.Position);
        ret = DecideMove(tmp.Keys, null, null);
        return null != ret;
	    return DecideMove(tmp.Keys, null, null);	// only called when no enemies in sight anyway
      }

      public virtual List<Objective> Subobjectives() { return new List<Objective>(_sub_goals); }
    }
#endif

    [Serializable]
  internal abstract class OrderableAI : ObjectiveAI
    {
    private const int EMOTE_GRAB_ITEM_CHANCE = 30;
    private const int LAW_ENFORCE_CHANCE = 30;

    // taboos really belong here
    private Dictionary<Item, int> m_TabooItems;
    private List<Actor> m_TabooTrades;

    // these relate to PC orders for NPCs.  Alpha 9 had no support for AI orders to AI.
    private ActorDirective m_Directive;
    private ActorOrder m_Order;
    protected Percept m_LastEnemySaw;
    protected Percept m_LastItemsSaw;
    protected Percept m_LastSoldierSaw;
    protected Percept m_LastRaidHeard;
    protected bool m_ReachedPatrolPoint;
    protected int m_ReportStage;

    public bool DontFollowLeader { get; set; }

    protected OrderableAI()
    {
    }

    public ActorDirective Directives { get { return m_Directive ?? (m_Directive = new ActorDirective()); } }
    protected List<Actor> TabooTrades { get { return m_TabooTrades; } }
    public ActorOrder Order { get { return m_Order; } }

    // doesn't include no-enemies check or any directives/orders
    protected bool OkToSleepNow { get {
      return m_Actor.WouldLikeToSleep && m_Actor.IsInside && m_Actor.CanSleep();
    } }

    public void SetOrder(ActorOrder newOrder)
    {
      m_Order = newOrder;
      m_ReachedPatrolPoint = false;
      m_ReportStage = 0;
    }

    protected ActorAction ExecuteOrder(RogueGame game, ActorOrder order, List<Percept> percepts)
    {
      if (!m_Actor.HasLeader) return null;
      switch (order.Task) {
        case ActorTasks.BARRICADE_ONE:
          return ExecuteBarricading(order.Location, false);
        case ActorTasks.BARRICADE_MAX:
          return ExecuteBarricading(order.Location, true);
        case ActorTasks.GUARD:
          return ExecuteGuard(order.Location, percepts);  // cancelled by enamies sighted
        case ActorTasks.PATROL:
          return ExecutePatrol(order.Location, percepts);  // cancelled by enamies sighted
        case ActorTasks.DROP_ALL_ITEMS:
          return ExecuteDropAllItems(game);
        case ActorTasks.BUILD_SMALL_FORTIFICATION:
          return ExecuteBuildFortification(game, order.Location, false);
        case ActorTasks.BUILD_LARGE_FORTIFICATION:
          return ExecuteBuildFortification(game, order.Location, true);
        case ActorTasks.REPORT_EVENTS:
          return ExecuteReport(game, percepts);  // cancelled by enamies sighted
        case ActorTasks.SLEEP_NOW:
          return ExecuteSleepNow(game, percepts);  // cancelled by enamies sighted
        case ActorTasks.FOLLOW_TOGGLE:
          return ExecuteToggleFollow(game);
        case ActorTasks.WHERE_ARE_YOU:
          return ExecuteReportPosition();
        default:
          throw new NotImplementedException("order task not handled");
      }
    }

    private ActorAction ExecuteBarricading(Location location, bool toTheMax)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      DoorWindow door = location.Map.GetMapObjectAt(location.Position) as DoorWindow;
      if (door == null) return null;
      if (!m_Actor.CanBarricade(door)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction= new ActionBarricadeDoor(m_Actor, door);
        if (!toTheMax) SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(location.Position);
      if (null == tmpAction) return null;
      RunIfPossible();
      return tmpAction;
    }

    private ActorAction ExecuteBuildFortification(RogueGame game, Location location, bool isLarge)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      if (!game.Rules.CanActorBuildFortification(m_Actor, location.Position, isLarge)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction = new ActionBuildFortification(m_Actor, location.Position, isLarge);
        if (!tmpAction.IsLegal()) return null;
        SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(location.Position);
      if (null == tmpAction) return null;
      RunIfPossible();
      return tmpAction;
    }

    private ActorAction ExecuteGuard(Location location, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }

      if (m_Actor.Location.Position != location.Position) {
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(location.Position);
        if (actorAction3 != null) {
          m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (m_Actor.IsHungry) {
        ActorAction actorAction3 = BehaviorEat();
        if (actorAction3 != null) return actorAction3;
      }

      ActorAction actorAction4 = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (actorAction4 != null) return actorAction4;

      return new ActionWait(m_Actor);
    }

    private ActorAction ExecutePatrol(Location location, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }
      if (!m_ReachedPatrolPoint)
        m_ReachedPatrolPoint = m_Actor.Location.Position == location.Position;

      if (!m_ReachedPatrolPoint) {
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(location.Position);
        if (actorAction3 != null) {
          m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (m_Actor.IsHungry) {
        ActorAction actorAction3 = BehaviorEat();
        if (actorAction3 != null) return actorAction3;
      }

      ActorAction actorAction4 = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (actorAction4 != null) return actorAction4;

      List<Zone> patrolZones = location.Map.GetZonesAt(Order.Location.Position);
      return BehaviorWander(loc =>
      {
        List<Zone> zonesAt = loc.Map.GetZonesAt(loc.Position);
        if (zonesAt == null) return false;
        foreach (Zone zone1 in zonesAt) {
          foreach (Zone zone2 in patrolZones) {
            if (zone1 == zone2) return true;
          }
        }
        return false;
      });
    }

    private ActorAction ExecuteDropAllItems(RogueGame game)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      Item it = m_Actor.Inventory[0];
      if (it.IsEquipped) game.DoUnequipItem(m_Actor, it);
      return new ActionDropItem(m_Actor, it);
    }

    private ActorAction ExecuteReport(RogueGame game, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }
      ActorAction actorAction = null;
      bool flag = false;
      switch (m_ReportStage)
      {
        case 0:
          actorAction = m_LastRaidHeard == null ? new ActionSay(m_Actor, m_Actor.Leader, "No raids heard.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastRaidHeard);
          ++m_ReportStage;
          break;
        case 1:
          actorAction = m_LastEnemySaw == null ? new ActionSay(m_Actor, m_Actor.Leader, "No enemies sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
          ++m_ReportStage;
          break;
        case 2:
          actorAction = m_LastItemsSaw == null ? new ActionSay(m_Actor, m_Actor.Leader, "No items sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
          ++m_ReportStage;
          break;
        case 3:
          actorAction = m_LastSoldierSaw == null ? new ActionSay(m_Actor, m_Actor.Leader, "No soldiers sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
          ++m_ReportStage;
          break;
        case 4:
          flag = true;
          actorAction = new ActionSay(m_Actor, m_Actor.Leader, "That's it.", RogueGame.Sayflags.NONE);
          break;
      }
      if (flag) SetOrder(null);
      return actorAction ?? new ActionSay(m_Actor, m_Actor.Leader, "Let me think...", RogueGame.Sayflags.NONE);
    }

    private ActorAction ExecuteSleepNow(RogueGame game, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }
      if (m_Actor.CanSleep(out string reason)) {
        return (m_Actor.Location.Map.LocalTime.TurnCounter % 2 == 0 ? (ActorAction)(new ActionSleep(m_Actor)) : new ActionWait(m_Actor));
      }
      SetOrder(null);
      game.DoEmote(m_Actor, string.Format("I can't sleep now : {0}.", (object) reason));
      return new ActionWait(m_Actor);
    }

    private ActorAction ExecuteToggleFollow(RogueGame game)
    {
      SetOrder(null);
      DontFollowLeader = !DontFollowLeader;
      game.DoEmote(m_Actor, DontFollowLeader ? "OK I'll do my stuff, see you soon!" : "I'm ready!");
      return new ActionWait(m_Actor);
    }

    private ActorAction ExecuteReportPosition()
    {
      SetOrder(null);
      string text = string.Format("I'm in {0} at {1},{2}.", (object)m_Actor.Location.Map.Name, (object)m_Actor.Location.Position.X, (object)m_Actor.Location.Position.Y);
      return new ActionSay(m_Actor, m_Actor.Leader, text, RogueGame.Sayflags.NONE);
    }

    // this assumes conditions like "everything is in FOV" so that a floodfill pathfinding is not needed.
    // we also assume no enemies in sight.
    ActorAction BehaviorEfficientlyHeadFor(Dictionary<Point,int> goals)
    {
      if (0>=goals.Count) return null;
      List<Point> legal_steps = m_Actor.LegalSteps;
      if (null == legal_steps) return null;
      if (2 <= legal_steps.Count) legal_steps = DecideMove_WaryOfTraps(legal_steps);
      if (2 <= legal_steps.Count) {
        int min_dist = goals.Values.Min();
        int near_scale = goals.Count+1;
#if DEBUG
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, near_scale.ToString());
#endif
        Dictionary<Point,int> efficiency = new Dictionary<Point,int>();
        foreach(Point pt in legal_steps) {
          efficiency[pt] = 0;
          foreach(var pt_delta in goals) {
            // relies on FOV not being "too large"
            int delta = pt_delta.Value-Rules.GridDistance(pt, pt_delta.Key);
            if (min_dist == pt_delta.Value) {
              efficiency[pt] += near_scale*delta;
            } else {
              efficiency[pt] += delta;
            }
          }
#if DEBUG
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, pt.X.ToString()+","+pt.Y.ToString()+": "+efficiency[pt].ToString());
#endif
        }
        int fast_approach = efficiency.Values.Max();
        efficiency.OnlyIf(val=>fast_approach==val);
        legal_steps = new List<Point>(efficiency.Keys);
      }

	  ActorAction tmpAction = DecideMove(legal_steps);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position);
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      return null;
    }

    public bool IsRationalTradeItem(Actor speaker, Item offeredItem)    // Cf. ActorControllerAI::IsInterestingTradeItem
    {
      Contract.Requires(null!=speaker);
      Contract.Requires(speaker.Model.Abilities.CanTrade);
#if DEBUG
      Contract.Requires(Actor.Model.Abilities.CanTrade);
#endif
      return IsInterestingItem(offeredItem);
    }

    protected void MaximizeRangedTargets(List<Point> dests, List<Percept> enemies)
    {
      if (null == dests || 2<=dests.Count) return;

      Dictionary<Point,int> targets = new Dictionary<Point,int>();
      int max_range = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
      foreach(Point pt in dests) {
        targets[pt] = enemies.Count(p => LOS.CanTraceHypotheticalFireLine(new Location(m_Actor.Location.Map,pt), p.Location.Position, max_range, m_Actor));
      }
      int max_LoF = targets.Values.Max();
      dests.RemoveAll(pt => max_LoF>targets[pt]);
    }

    protected List<ItemRangedWeapon> GetAvailableRangedWeapons()
    {
      IEnumerable<ItemRangedWeapon> tmp_rw = ((!Directives.CanFireWeapons || m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) ? null : m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>()?.Where(rw => 0 < rw.Ammo || null != m_Actor.GetCompatibleAmmoItem(rw)));
      return (null!=tmp_rw && tmp_rw.Any() ? tmp_rw.ToList() : null);
    }

    protected bool HasBehaviorThatRecallsToSurface {
      get {
        if (null != m_Actor.Threats) return true;   // hunting threat
        if (null != m_Actor.InterestingLocs) return true;   // tourism
        if (m_Actor.HasLeader) {
          if (m_Actor.Leader.IsPlayer) return true; // typically true...staying in the subway forever isn't a good idea
          if ((m_Actor.Leader.Controller as OrderableAI)?.HasBehaviorThatRecallsToSurface ?? false) return true;   // if leader has recall-to-surface behavior, following him recalls to surface
        }
        return false;
      }
    }

    protected IEnumerable<Engine.MapObjects.PowerGenerator> GeneratorsToTurnOn {
      get {
        Map map = m_Actor.Location.Map;
        if (Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap == map) return null; // plot consequences until Prisoner That Should Not Be is dead, does not light level.
        if (0 >= map.PowerGenerators.Get.Length) return null;
        if (1.0 <= map.PowerRatio) return null;
        return m_Actor.Location.Map.PowerGenerators.Get.Where(obj => !obj.IsOn);
      }
    }

    protected bool WantToRecharge(ItemLight it)
    {
      int burn_time = 0;
      switch(it.Model.ID)
      {
      case GameItems.IDs.LIGHT_FLASHLIGHT: burn_time = m_Actor.Location.Map.LocalTime.SunsetToDawnDuration+2*WorldTime.TURNS_PER_HOUR;
        break;
      case GameItems.IDs.LIGHT_BIG_FLASHLIGHT: burn_time = m_Actor.Location.Map.LocalTime.MidnightToDawnDuration+WorldTime.TURNS_PER_HOUR;
        break;
#if DEBUG
      default: throw new InvalidOperationException("Unhandled light type " + it.Model.ID.ToString());
#else
      default: return false;
#endif
      }
      return it.Batteries<burn_time;
    }

    protected bool WantToRecharge() {
      return m_Actor.Inventory.GetItemsByType<ItemLight>()?.Any(it => WantToRecharge(it)) ?? false;
    }

    protected bool WantToRechargeAtDawn(ItemLight it)
    {
      int burn_time = 0;
      int reserve = 0;
      switch(it.Model.ID)
      {
      case GameItems.IDs.LIGHT_FLASHLIGHT: burn_time = m_Actor.Location.Map.LocalTime.SunsetToDawnDuration;
        reserve = 2*WorldTime.TURNS_PER_HOUR;
        break;
      case GameItems.IDs.LIGHT_BIG_FLASHLIGHT: burn_time = m_Actor.Location.Map.LocalTime.MidnightToDawnDuration;
        reserve = WorldTime.TURNS_PER_HOUR;
        break;
#if DEBUG
      default: throw new InvalidOperationException("Unhandled light type " + it.Model.ID.ToString());
#else
      default: return false;
#endif
      }
      return it.Batteries-burn_time<reserve;
    }

    protected bool WantToRechargeAtDawn() {
      return m_Actor.Inventory.GetItemsByType<ItemLight>()?.Any(it => WantToRechargeAtDawn(it)) ?? false;
    }

    public bool InCommunicationWith(Actor a)
    {
      if (m_Actor==a) return true;
      if (!(a.Controller is OrderableAI) && !(a.Controller is PlayerController)) return false;
      if (a.IsSleeping) return false;
      if (a.Location.Map == m_Actor.Location.Map && a.Controller.FOV.Contains(m_Actor.Location.Position) && m_Actor.Controller.FOV.Contains(a.Location.Position)) return true;
      if (a.HasActivePoliceRadio && m_Actor.HasActivePoliceRadio) return true;
      if (a.HasActiveArmyRadio && m_Actor.HasActiveArmyRadio) return true;
      if (null!=a.GetEquippedCellPhone() && null!=m_Actor.GetEquippedCellPhone()) return true;
      return false;
    }

    // XXX to implement
    // core inventory should be (but is not)
    // armor: 1 slot (done)
    // flashlight: 1 slot (currently very low priority)
    // melee weapon: 1 slot (done)
    // ranged weapon w/ammo: 1 slot
    // ammo clips: 1 slot high priority, 1 slot moderate priority (tradeable)
    // without Hauler levels, that is 5 non-tradeable slots when fully kitted
    // Also, has enough food checks should be based on wakeup time

    // Gun bunnies would:
    // * have a slot budget of MaxCapacity-3 or -4 for ranged weapons and ammo combined
    // * use no more than half of that slot budget for ranged weapons, rounded up
    // * strongly prefer one clip for each of two ranged weapons over 2 clips for a single ranged weapon

    // close to the inverse of IsInterestingItem
    public bool IsTradeableItem(Item it)
    {
		Contract.Requires(null != it);
#if DEBUG
        Contract.Requires(Actor.Model.Abilities.CanTrade);
#endif
        if (it is ItemBodyArmor) return !it.IsEquipped; // XXX best body armor should be equipped
        if (it is ItemFood)
            {
            if (!m_Actor.Model.Abilities.HasToEat) return true;
            if (m_Actor.IsHungry) return false;
            // only should trade away food that doesn't drop below threshold
            ItemFood food = it as ItemFood;
            if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2 + food.Nutrition))
              return food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
            return true;
            }
        if (it is ItemRangedWeapon)
            {
            if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return true;
            ItemRangedWeapon rw = it as ItemRangedWeapon;
            if (0 < rw.Ammo) return false;
            if (null != m_Actor.GetCompatibleAmmoItem(rw)) return false;
            return true;    // more work needed
            }
        if (it is ItemAmmo)
            {
            ItemAmmo am = it as ItemAmmo;
            if (m_Actor.GetCompatibleRangedWeapon(am) == null) return true;
            return m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
            }
        if (it is ItemMeleeWeapon)
            {
            Attack martial_arts = m_Actor.UnarmedMeleeAttack();
            if ((it.Model as ItemMeleeWeaponModel).Attack.Rating <= martial_arts.Rating) return true;
            // do not trade away the best melee weapon.  Others ok.
            return m_Actor.GetBestMeleeWeapon() != it;  // return value should not be null
            }
        if (it is ItemLight)
            {
            if (!m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2)) return false;
            // XXX more work needed
            return true;
            }
        // player should be able to trade for blue pills
/*
        if (it is ItemMedicine)
            {
            return HasAtLeastFullStackOfItemTypeOrModel(it, 2);
            }
*/
        return true;    // default to ok to trade away
    }

    // XXX *could* eliminate int turn by defining it as location.Map.LocalTime.TurnCounter
    public void OnRaid(RaidType raid, Location location, int turn)
    {
      if (m_Actor.IsSleeping) return;
      string str;
      switch (raid) {
        case RaidType.BIKERS:
          str = "motorcycles coming";
          break;
        case RaidType.GANGSTA:
          str = "cars coming";
          break;
        case RaidType.BLACKOPS:
          str = "a chopper hovering";
          break;
        case RaidType.SURVIVORS:
          str = "honking coming";
          break;
        case RaidType.NATGUARD:
          str = "the army coming";
          break;
        case RaidType.ARMY_SUPLLIES:
          str = "a chopper hovering";
          break;
        default:
          throw new ArgumentOutOfRangeException(string.Format("unhandled raidtype {0}", (object) raid.ToString()));
      }
      m_LastRaidHeard = new Percept((object) str, turn, location);
    }

    // Behaviors and support functions
    // but all body armors are equipped to the torso slot(?)
    private ItemBodyArmor GetEquippedBodyArmor()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemBodyArmor) return obj as ItemBodyArmor;
      }
      return null;
    }

    protected void BehaviorEquipBodyArmor(RogueGame game)
    {
      ItemBodyArmor bestBodyArmor = m_Actor.GetBestBodyArmor(it => !IsItemTaboo(it));
      if (bestBodyArmor == null) return;
      ItemBodyArmor equippedBodyArmor = GetEquippedBodyArmor();
      if (equippedBodyArmor == bestBodyArmor) return;
      game.DoEquipItem(m_Actor, bestBodyArmor);
    }

    protected ActorAction ManageMeleeRisk(List<Point> legal_steps, List<Point> retreat, List<Point> run_retreat, bool safe_run_retreat, List<ItemRangedWeapon> available_ranged_weapons, List<Percept> friends, List<Percept> enemies, List<Actor> slow_melee_threat)
    {
      ActorAction tmpAction = null;
      if ((null != retreat || null != run_retreat) && null != available_ranged_weapons && null!=enemies) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(retreat, enemies);
        MaximizeRangedTargets(run_retreat, enemies);
        IEnumerable<Actor> fast_enemies = enemies.Select(p => p.Percepted as Actor).Where(a => a.Speed <= 2 * m_Actor.Speed);   // typically rats.
        if (fast_enemies.Any()) return null;    // not practical to run from rats.
        // ranged weapon: fast retreat ok
        // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
        // XXX we also want to be close enough to fire at all
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat, enemies, friends) : ((null != retreat) ? DecideMove(retreat) : null));
        if (null != tmpAction) {
          if (tmpAction is ActionMoveStep test) {
            if (safe_run_retreat) RunIfPossible();
            else m_Actor.IsRunning = RunIfAdvisable(test.dest.Position);
          }
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }

      if (null != retreat) {
        // need stamina to melee: slow retreat ok
        if (WillTireAfterAttack(m_Actor)) {
	      tmpAction = DecideMove(retreat, enemies, friends);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        // have slow enemies nearby
        if (null != slow_melee_threat) {
	      tmpAction = DecideMove(retreat, enemies, friends);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
      }
      // end melee risk management check
      return null;
    }

    private void ETAToKill(Actor en, int dist, ItemRangedWeapon rw, Dictionary<Actor, int> best_weapon_ETAs, Dictionary<Actor, ItemRangedWeapon> best_weapons=null)
    {
      Attack tmp = m_Actor.HypotheticalRangedAttack(rw.Model.Attack, dist, en);
	  int a_dam = tmp.DamageValue - en.CurrentDefence.Protection_Shot;
      if (0 >= a_dam) return;   // do not update ineffective weapons
      int a_kill_b_in = ((8*en.HitPoints)/(5*a_dam))+2;	// assume bad luck when attacking.
      // Also, assume one fluky miss and compensate for overkill returning 0 rather than 1 attacks.
      if (a_kill_b_in > rw.Ammo) {  // account for reloading weapon
        int turns = a_kill_b_in-rw.Ammo;
        a_kill_b_in++;
        a_kill_b_in += turns/rw.Model.MaxAmmo;
      }
      if (null == best_weapons) {
        best_weapon_ETAs[en] = a_kill_b_in;
        return;
      } else if (!best_weapons.ContainsKey(en)) {
        best_weapons[en] = rw;
        best_weapon_ETAs[en] = a_kill_b_in;
        return;
      } else if (best_weapon_ETAs[en]>a_kill_b_in) {
        best_weapons[en] = rw;
        best_weapon_ETAs[en] = a_kill_b_in;
        return;
      } else if (2 == best_weapon_ETAs[en]) {
        Attack tmp2 = m_Actor.HypotheticalRangedAttack(best_weapons[en].Model.Attack, dist, en);
        if (tmp.DamageValue < tmp2.DamageValue) {   // lower damage for overkill is usually better
          best_weapons[en] = rw;
          best_weapon_ETAs[en] = a_kill_b_in;
        }
        return;
      }
    }

    private ActorAction Equip(ItemRangedWeapon rw) {
      if (!rw.IsEquipped && m_Actor.CanEquip(rw)) RogueForm.Game.DoEquipItem(m_Actor, rw);
      if (0 >= rw.Ammo) {
        ItemAmmo ammo = m_Actor.GetCompatibleAmmoItem(rw);
        if (null != ammo) return new ActionUseItem(m_Actor, ammo);
      }
      return null;
    }

    protected static int ScoreRangedWeapon(ItemRangedWeapon w)
    {
      Attack rw_attack = w.Model.Attack;
      return 1000 * rw_attack.Range + rw_attack.DamageValue;
    }

    [Pure]
    protected ItemRangedWeapon GetBestRangedWeaponWithAmmo(Predicate<Item> fn=null)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      ItemRangedWeapon obj1 = null;
      int num1 = 0;
      IEnumerable<ItemRangedWeapon> rws = m_Actor.Inventory.Items.Select(it=>it as ItemRangedWeapon).Where(w=>null!=w);
      if (null!=fn) rws = rws.Where(w=>fn(w));
      foreach (ItemRangedWeapon w in rws) {
        bool flag = false;
        if (w.Ammo > 0) flag = true;
        else {
          IEnumerable<ItemAmmo> ammos = m_Actor.Inventory.Items.Select(it=>it as ItemAmmo).Where(ammo=>null!=ammo && ammo.AmmoType==w.AmmoType);
          if (null!=fn) ammos = ammos.Where(ammo=>fn(ammo));
          flag = ammos.Any();
        }
        if (flag) {
          int num2 = ScoreRangedWeapon(w);
          if (num2 > num1) {
            obj1 = w;
            num1 = num2;
          }
        }
      }
      return obj1;
    }

    private ActorAction BehaviorMeleeSnipe(Actor en, Attack tmp_attack, bool one_on_one)
    {
      if (en.HitPoints>tmp_attack.DamageValue/2) return null;
      ActorAction tmpAction = null;
      // can one-shot
      if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {    // safe
        tmpAction = BehaviorMeleeAttack(en);
        if (null != tmpAction) return tmpAction;
      }
      if (one_on_one && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
        tmpAction = BehaviorMeleeAttack(en);
        if (null != tmpAction) return tmpAction;
      }
      return null;
    }

    private HashSet<Point> GetRangedAttackFromZone(List<Percept> enemies)
    {
      HashSet<Point> ret = new HashSet<Point>();
//    HashSet<Point> danger = new HashSet<Point>();
      int range = m_Actor.CurrentRangedAttack.Range;
      System.Collections.ObjectModel.ReadOnlyCollection<Point> optimal_FOV = LOS.OptimalFOV(range);
      foreach(Percept en in enemies) {
        foreach(Point pt in optimal_FOV.Select(p => new Point(p.X+en.Location.Position.X,p.Y+en.Location.Position.Y))) {
          if (ret.Contains(pt)) continue;
//        if (danger.Contains(pt)) continue;
          if (!m_Actor.Location.Map.IsValid(pt)) continue;
          List<Point> LoF = new List<Point>();  // XXX micro-optimization?: create once, clear N rather than create N
          if (LOS.CanTraceHypotheticalFireLine(new Location(en.Location.Map,pt), en.Location.Position, range, m_Actor, LoF)) ret.UnionWith(LoF);
          // if "safe" attack possible init danger in different/earlier loop
        }
      }
      return ret;
    }

    // forked from BaseAI::BehaviorEquipWeapon
    protected ActorAction BehaviorEquipWeapon(RogueGame game, List<Point> legal_steps, Dictionary<Point,int> damage_field, List<ItemRangedWeapon> available_ranged_weapons, List<Percept> enemies, List<Percept> friends, HashSet<Actor> immediate_threat)
    {
      Contract.Requires((null==available_ranged_weapons)==(null==GetBestRangedWeaponWithAmmo()));
#if DEBUG
      // == failed for traps
      Contract.Requires(null==immediate_threat || (null!=damage_field && damage_field.ContainsKey(Actor.Location.Position)));
#endif

      // migrated from CivilianAI::SelectAction
      ActorAction tmpAction = null;
      if (null != enemies) {
        if (1==Rules.GridDistance(enemies[0].Location,m_Actor.Location)) {
          // something adjacent...check for one-shotting
          ItemMeleeWeapon tmp_melee = m_Actor.GetBestMeleeWeapon(it => !IsItemTaboo(it));
          if (null!=tmp_melee) {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              tmpAction = BehaviorMeleeSnipe(en, m_Actor.HypotheticalMeleeAttack(tmp_melee.Model.BaseMeleeAttack(m_Actor.Sheet), en),null==immediate_threat || (1==immediate_threat.Count && immediate_threat.Contains(en)));
              if (null != tmpAction) {
                if (!tmp_melee.IsEquipped) game.DoEquipItem(m_Actor, tmp_melee);
                return tmpAction;
              }
            }
          } else { // also check for no-weapon one-shotting
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              tmpAction = BehaviorMeleeSnipe(en, m_Actor.UnarmedMeleeAttack(en), null == immediate_threat || (1 == immediate_threat.Count && immediate_threat.Contains(en)));
              if (null != tmpAction) {
                if (0 < m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS)) {
                  Item tmp_w = m_Actor.GetEquippedWeapon();
                  if (null != tmp_w) game.DoUnequipItem(m_Actor,tmp_w);
                }
                return tmpAction;
              }
            }
          }
        }
      }

      // if no ranged weapons, use BaseAI
      // OrderableAI::GetAvailableRangedWeapons knows about AI disabling of ranged weapons
      if (null == available_ranged_weapons) return base.BehaviorEquipWeapon(game);

      // if no enemies in sight, reload all ranged weapons and then equip longest-range weapon
      // XXX there may be more important objectives than this
      if (null == enemies) {
        IEnumerable<ItemRangedWeapon> reloadable = available_ranged_weapons.Where(rw2 => 0 >= rw2.Ammo);
        // XXX should not reload a precision rifle if also have an army rifle, but shouldn't have both in inventory anyway
        ItemRangedWeapon rw = reloadable.FirstOrDefault();
        if (null != rw) {
          tmpAction = Equip(reloadable.FirstOrDefault());
          if (null != tmpAction) return tmpAction;
        }
        return Equip(GetBestRangedWeaponWithAmmo());
      }
      // at this point, null != enemies, we have a ranged weapon available, and melee one-shot is not feasible
      // also, damage field should be non-null because enemies is non-null

      int best_range = available_ranged_weapons.Select(rw => rw.Model.Attack.Range).Max();
      List<Percept> en_in_range = FilterFireTargets(enemies,best_range);

      // if no enemies in range, or just one available ranged weapon, use the best one
      if (null == en_in_range || 1==available_ranged_weapons.Count) {
        tmpAction = Equip(GetBestRangedWeaponWithAmmo());
        if (null != tmpAction) return tmpAction;
      }

      if (null == en_in_range && null != legal_steps) {
        List<Percept> percepts2 = FilterPossibleFireTargets(enemies);
		if (null != percepts2) {
		  IEnumerable<Point> tmp = legal_steps.Where(p=>null!=FilterContrafactualFireTargets(percepts2,p));
		  if (tmp.Any()) {
	        tmpAction = DecideMove(tmp, enemies, friends);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.FIGHTING;
              if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position);
              return tmpAction;
            }
		  }
        }

        // XXX need to use floodfill pathfinder
        HashSet<Point> fire_from_here = GetRangedAttackFromZone(enemies);
        tmpAction = BehaviorNavigate(fire_from_here);
        if (null != tmpAction) return tmpAction;
      }

      if (null == en_in_range) return null; // no enemies in range, no constructive action: do somnething else

      // filter immediate threat by being in range
      HashSet<Actor> immediate_threat_in_range = (null!=immediate_threat ? new HashSet<Actor>(immediate_threat) : new HashSet<Actor>());
      if (null != immediate_threat) immediate_threat_in_range.IntersectWith(en_in_range.Select(p => p.Percepted as Actor));

      if (1 == available_ranged_weapons.Count) {
        if (1 == en_in_range.Count) {
          return BehaviorRangedAttack(en_in_range[0].Percepted as Actor);
        } else if (1 == immediate_threat_in_range.Count) {
          return BehaviorRangedAttack(immediate_threat_in_range.First());
        }
      }

      // Get ETA stats
      Dictionary<Actor,int> best_weapon_ETAs = new Dictionary<Actor,int>();
      Dictionary<Actor,ItemRangedWeapon> best_weapons = new Dictionary<Actor,ItemRangedWeapon>();
      if (1<available_ranged_weapons.Count) {
        foreach(ItemRangedWeapon rw in available_ranged_weapons) {
          foreach(Percept p in en_in_range) {
            Actor a = p.Percepted as Actor;
            ETAToKill(a,Rules.GridDistance(m_Actor.Location,p.Location),rw,best_weapon_ETAs, best_weapons);
          }
        }
      } else {
        foreach(Percept p in en_in_range) {
          Actor a = p.Percepted as Actor;
          ETAToKill(a,Rules.GridDistance(m_Actor.Location,p.Location), available_ranged_weapons[0], best_weapon_ETAs);
        }
      }

      // cf above: we got here because there were multiple ranged weapons to choose from in these cases
      if (1 == en_in_range.Count) {
        Actor a = en_in_range[0].Percepted as Actor;
        tmpAction = Equip(best_weapons[a]);
        if (null != tmpAction) return tmpAction;
        return BehaviorRangedAttack(en_in_range[0].Percepted as Actor);
      } else if (1 == immediate_threat_in_range.Count) {
        Actor a = immediate_threat_in_range.First();
        tmpAction = Equip(best_weapons[a]);
        if (null != tmpAction) return tmpAction;
        return BehaviorRangedAttack(a);
      }
      // at this point: there definitely is more than one enemy in range
      // if there are any immediate threat, there are at least two immediate threat
      if (2 <= immediate_threat_in_range.Count) {
        int ETA_min = immediate_threat_in_range.Select(a => best_weapon_ETAs[a]).Min();
        immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => best_weapon_ETAs[a] == ETA_min));
        if (2 <= immediate_threat_in_range.Count) {
          int HP_min = ((2 >= ETA_min) ? immediate_threat_in_range.Select(a => a.HitPoints).Max() : immediate_threat_in_range.Select(a => a.HitPoints).Min());
          immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => a.HitPoints == HP_min));
          if (2 <= immediate_threat_in_range.Count) {
           int dist_min = immediate_threat_in_range.Select(a => Rules.GridDistance(m_Actor.Location,a.Location)).Min();
           immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => Rules.GridDistance(m_Actor.Location, a.Location) == dist_min));
          }
        }
        Actor actor = immediate_threat_in_range.First();
        if (1 < available_ranged_weapons.Count) {
         tmpAction = Equip(best_weapons[actor]);
         if (null != tmpAction) return tmpAction;
        }
        return BehaviorRangedAttack(actor);
      }
      // at this point, no immediate threat in range
      {
        int ETA_min = en_in_range.Select(p => best_weapon_ETAs[p.Percepted as Actor]).Min();
        if (2==ETA_min) {
          // snipe something
          en_in_range = new List<Percept>(en_in_range.Where(p => ETA_min == best_weapon_ETAs[p.Percepted as Actor]));
          if (2<=en_in_range.Count) {
            int HP_max = en_in_range.Select(p => (p.Percepted as Actor).HitPoints).Max();
            en_in_range = new List<Percept>(en_in_range.Where(p => (p.Percepted as Actor).HitPoints == HP_max));
            if (2<=en_in_range.Count) {
             int dist_min = en_in_range.Select(p => Rules.GridDistance(m_Actor.Location,p.Location)).Min();
             en_in_range = new List<Percept>(en_in_range.Where(p => Rules.GridDistance(m_Actor.Location, p.Location) == dist_min));
            }
          }
          Actor actor = en_in_range.First().Percepted as Actor;
          if (1 < available_ranged_weapons.Count) {
            tmpAction = Equip(best_weapons[actor]);
            if (null != tmpAction) return tmpAction;
          }
          return BehaviorRangedAttack(actor);
        }
      }

      // just deal with something close
      {
        int dist_min = en_in_range.Select(p => Rules.GridDistance(m_Actor.Location,p.Location)).Min();
        en_in_range = new List<Percept>(en_in_range.Where(p => Rules.GridDistance(m_Actor.Location, p.Location) == dist_min));
        if (2<=en_in_range.Count) {
          int HP_min = en_in_range.Select(p => (p.Percepted as Actor).HitPoints).Min();
          en_in_range = new List<Percept>(en_in_range.Where(p => (p.Percepted as Actor).HitPoints == HP_min));
        }
        Actor actor = en_in_range.First().Percepted as Actor;
        if (1 < available_ranged_weapons.Count) {
          tmpAction = Equip(best_weapons[actor]);
          if (null != tmpAction) return tmpAction;
        }
        return BehaviorRangedAttack(actor);
      }
    }

    // This is only called when the actor is hungry.  It doesn't need to do food value corrections
    protected ItemFood GetBestEdibleItem()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      int need = m_Actor.MaxFood - m_Actor.FoodPoints;
      ItemFood obj1 = null;
      int rating = int.MinValue;
      foreach (Item obj2 in m_Actor.Inventory.Items) {
        ItemFood food = obj2 as ItemFood;
        if (null == food) continue;
        int num3 = 0;
        int num4 = food.NutritionAt(turnCounter);
        int num5 = num4 - need;
        if (num5 > 0) num3 -= num5;
        if (!food.IsPerishable) num3 -= num4;
        if (num3 > rating) {
          obj1 = food;
          rating = num3;
        }
      }
      return obj1;
    }

    // This is more pro-active.  We might want to flag whether
    // an AI uses the behavior based on this
    protected ItemFood GetBestPerishableItem()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      int need = m_Actor.MaxFood - m_Actor.FoodPoints;
      ItemFood obj1 = null;
      int rating = int.MinValue;
      foreach (Item obj2 in m_Actor.Inventory.Items) {
        ItemFood food = obj2 as ItemFood;
        if (null == food) continue;
        if (!food.IsPerishable) continue;
        if (food.IsSpoiledAt(turnCounter)) continue;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4 > need) continue; // more work needed
        int num3 = need-num4;
        if (num3 > rating) {
          obj1 = food;
          rating = num3;
        }
      }
      return obj1;
    }

    protected ActorAction BehaviorEat()
    {
      ItemFood bestEdibleItem = GetBestEdibleItem();
      if (null == bestEdibleItem) return null;
      return (m_Actor.CanUse(bestEdibleItem) ? new ActionUseItem(m_Actor, bestEdibleItem) : null);
    }

    protected ActorAction BehaviorEatProactively()
    {
      Item bestEdibleItem = GetBestPerishableItem();
      if (null == bestEdibleItem) return null;
      return (m_Actor.CanUse(bestEdibleItem) ? new ActionUseItem(m_Actor, bestEdibleItem) : null);
    }

    protected void BehaviorUnequipLeftItem(RogueGame game)
    {
      Item equippedItem = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
      if (null != equippedItem) game.DoUnequipItem(m_Actor, equippedItem);
    }

    protected ItemLight GetEquippedLight()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemLight) return obj as ItemLight;
      }
      return null;
    }

    /// <returns>true if and only if light should be equipped</returns>
    protected bool BehaviorEquipLight(RogueGame game)
    {
      if (!NeedsLight()) return false;
      ItemLight tmp = GetEquippedLight();
      if (null != tmp && !tmp.IsUseless) return true;
      tmp = m_Actor.GetFirstMatching<ItemLight>((Predicate<ItemLight>)(it =>
      {
          if (!it.IsUseless) return !IsItemTaboo(it);
          return false;
      }));
      if (tmp != null && m_Actor.CanEquip(tmp)) {
        game.DoEquipItem(m_Actor, tmp);
        return true;
      }
      return false;
    }

    /// <returns>true if and only if a cell phone is required to be equipped</returns>
    protected bool BehaviorEquipCellPhone(RogueGame game)
    {
      bool wantCellPhone = false;
      if (m_Actor.CountFollowers > 0) wantCellPhone = true;
      else if (m_Actor.HasLeader) {
        ItemTracker itemTracker = m_Actor.Leader.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
        wantCellPhone = (null != itemTracker && itemTracker.CanTrackFollowersOrLeader);
      }
      else return false; // XXX could dial 911, at least while that desk is manned

      ItemTracker equippedCellPhone = m_Actor.GetEquippedCellPhone();
      if (equippedCellPhone != null) {
        if (wantCellPhone) return true;
        game.DoUnequipItem(m_Actor, equippedCellPhone);
      }
      if (!wantCellPhone) return false;
      ItemTracker firstTracker = m_Actor.GetFirstMatching<ItemTracker>(it =>
      {
        if (it.CanTrackFollowersOrLeader && 0 < it.Batteries) return !IsItemTaboo(it);
        return false;
      });
      if (firstTracker != null && m_Actor.CanEquip(firstTracker)) {
        game.DoEquipItem(m_Actor, firstTracker);
        return true;
      }
      return false;
    }

    /// <returns>null, or a legal ActionThrowGrenade</returns>
    protected ActorAction BehaviorThrowGrenade(RogueGame game, List<Percept> enemies)
    {
      if (enemies == null || enemies.Count == 0) return null;
      if (enemies.Count < 3) return null;
      ItemGrenade firstGrenade = m_Actor.GetFirstMatching<ItemGrenade>((Predicate<ItemGrenade>) (it => !IsItemTaboo(it)));
      if (firstGrenade == null) return null;
      ItemGrenadeModel itemGrenadeModel = firstGrenade.Model;
      int maxRange = m_Actor.MaxThrowRange(itemGrenadeModel.MaxThrowDistance);
      Point? nullable = null;
      int num1 = 0;
      int my_dist = 0;
      foreach (Point point in m_Actor.Controller.FOV) {
        if (   (my_dist = Rules.GridDistance(m_Actor.Location.Position, point)) > itemGrenadeModel.BlastAttack.Radius
            && my_dist <= maxRange
            && LOS.CanTraceThrowLine(m_Actor.Location, point, maxRange)) {
          int num2 = 0;
          Rectangle blast_zone = new Rectangle(point.X-itemGrenadeModel.BlastAttack.Radius, point.Y-itemGrenadeModel.BlastAttack.Radius, 2*itemGrenadeModel.BlastAttack.Radius+1, 2*itemGrenadeModel.BlastAttack.Radius+1);
          // XXX \todo we want to evaluate the damage for where threat is *when the grenade explodes*
          if (   !blast_zone.Any(pt => {
                    if (!m_Actor.Location.Map.IsValid(pt)) return false;
                    Actor actorAt = m_Actor.Location.Map.GetActorAt(pt);
                    if (null == actorAt) return false;
//                  if (actorAt == m_Actor) throw new ArgumentOutOfRangeException("actorAt == m_Actor"); // probably an invariant failure
                    int distance = Rules.GridDistance(point, actorAt.Location.Position);
//                  if (distance > itemGrenadeModel.BlastAttack.Radius) throw new ArgumentOutOfRangeException("distance > itemGrenadeModel.BlastAttack.Radius"); // again, probably an invariant failure
                    if (m_Actor.IsEnemyOf(actorAt)) {
                      num2 += (itemGrenadeModel.BlastAttack.DamageAt(distance) * actorAt.MaxHPs);
                      return false;
                    }
//                  num2 = -1;
                    return true;
                 })
              &&  num2>num1) {
            nullable = point;
            num1 = num2;
          }
        }
      }
      if (null == nullable /* || !nullable.HasValue */) return null;  // 2nd test probably redundant
      if (!firstGrenade.IsEquipped) game.DoEquipItem(m_Actor, firstGrenade);
      ActorAction actorAction = new ActionThrowGrenade(m_Actor, nullable.Value);
      if (!actorAction.IsLegal()) throw new ArgumentOutOfRangeException("created illegal ActionThrowGrenade");  // invariant failure
      return actorAction;
    }

    private string MakeCentricLocationDirection(Location from, Location to)
    {
      if (from.Map != to.Map) return string.Format("in {0}", (object) to.Map.Name);
      Point position1 = from.Position;
      Point position2 = to.Position;
      Point v = new Point(position2.X - position1.X, position2.Y - position1.Y);
      return string.Format("{0} tiles to the {1}", (object) (int) Rules.StdDistance(v), (object) Direction.ApproximateFromVector(v));
    }

    private bool IsItemWorthTellingAbout(Item it)
    {
      return it != null && !(it is ItemBarricadeMaterial) && (m_Actor.Inventory.IsEmpty || !m_Actor.Inventory.Contains(it));
    }

    protected ActorAction BehaviorTellFriendAboutPercept(RogueGame game, Percept percept)
    {
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        Actor actorAt = map.GetActorAt(pt);
        return actorAt != null && !actorAt.IsSleeping && !m_Actor.IsEnemyOf(actorAt);
      }));
      if (pointList == null || pointList.Count == 0) return null;
      Actor actorAt1 = map.GetActorAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      string str1 = MakeCentricLocationDirection(m_Actor.Location, percept.Location);
      string str2 = string.Format("{0} ago", (object) WorldTime.MakeTimeDurationMessage(m_Actor.Location.Map.LocalTime.TurnCounter - percept.Turn));
      string text;
      if (percept.Percepted is Actor)
        text = string.Format("I saw {0} {1} {2}.", (object) (percept.Percepted as Actor).Name, (object) str1, (object) str2);
      else if (percept.Percepted is Inventory) {
        Inventory inventory = percept.Percepted as Inventory;
        if (inventory.IsEmpty) return null;
        Item it = inventory[game.Rules.Roll(0, inventory.CountItems)];
        if (!IsItemWorthTellingAbout(it)) return null;
        int num = actorAt1.FOVrange(map.LocalTime, Session.Get.World.Weather);
        if (percept.Location.Map == actorAt1.Location.Map && (double) Rules.StdDistance(percept.Location.Position, actorAt1.Location.Position) <= (double) (2 + num))
          return null;
        text = string.Format("I saw {0} {1} {2}.", (object) it.AName, (object) str1, (object) str2);
      } else {
        if (!(percept.Percepted is string)) throw new InvalidOperationException("unhandled percept.Percepted type");
        text = string.Format("I heard {0} {1} {2}!", (object) (percept.Percepted as string), (object) str1, (object) str2);
      }
      return new ActionSay(m_Actor, actorAt1, text, RogueGame.Sayflags.NONE);
    }

    protected ActorAction BehaviorSecurePerimeter()
    {
      Map map = m_Actor.Location.Map;
      Dictionary<Point,int> want_to_resolve = new Dictionary<Point,int>();
      foreach (Point position in m_Actor.Controller.FOV) {
        DoorWindow door = map.GetMapObjectAt(position) as DoorWindow;
        if (null == door) continue;
        if (door.IsOpen && m_Actor.CanClose(door)) {
          if (Rules.IsAdjacent(position, m_Actor.Location.Position)) {
            // this can trigger close-open loop with someone who is merely traveling
            // check for duplicating last action
            if (Objectives.Any(o => o is Goal_LastAction<ActionCloseDoor> && (o as Goal_LastAction<ActionCloseDoor>).LastAction.Door == door)) {
              // break action loop; plausibly the conflicting actor will be in the doorway next time
              return new ActionWait(m_Actor);
            }
            // proceed
            ActionCloseDoor tmp = new ActionCloseDoor(m_Actor, door);
            Objectives.Add(new Goal_LastAction<ActionCloseDoor>(m_Actor.Location.Map.LocalTime.TurnCounter,m_Actor,tmp));
            return tmp;
          }
          want_to_resolve[position] = Rules.GridDistance(position, m_Actor.Location.Position);
        }
        if (door.IsWindow && !door.IsBarricaded && m_Actor.CanBarricade(door)) {
          if (Rules.IsAdjacent(position, m_Actor.Location.Position))
            return new ActionBarricadeDoor(m_Actor, door);
          want_to_resolve[position] = Rules.GridDistance(position, m_Actor.Location.Position);
        }
      }
      // we could floodfill this, of course -- but everything is in LoS so try something else
      // we want to head for a nearest objective in such a way that the distance to all of the other objectives is minimized
      ActorAction tmpAction = BehaviorEfficientlyHeadFor(want_to_resolve);
      if (null != tmpAction) return tmpAction;
      return null;
    }

    protected ActionShout BehaviorWarnFriends(List<Percept> friends, Actor nearestEnemy)
    {
      Contract.Requires(null != nearestEnemy);
      if (Rules.IsAdjacent(m_Actor.Location, nearestEnemy.Location)) return null;
      if (m_Actor.HasLeader && m_Actor.Leader.IsSleeping) return new ActionShout(m_Actor);
      foreach (Percept friend in friends) {
        Actor actor = friend.Percepted as Actor;
        if (actor == null) throw new ArgumentException("percept not an actor");
        if (actor != m_Actor && (actor.IsSleeping && !m_Actor.IsEnemyOf(actor)) && actor.IsEnemyOf(nearestEnemy)) {
          string text = string.Format("Wake up {0}! {1} sighted!", actor.Name, nearestEnemy.Name);
          return new ActionShout(m_Actor, text);
        }
      }
      return null;
    }

    protected ActorAction BehaviorDontLeaveFollowersBehind(int distance, out Actor target)
    {
      target = null;

      // Scan the group:
      // - Find farthest member of the group.
      // - If at least half the group is close enough we consider the group cohesion to be good enough and do nothing.
      int halfGroup = m_Actor.CountFollowers / 2;
	  if (0 >= halfGroup) return null;	// automatic do nothing(!)
      int worstDist = Int32.MinValue;
      Map map = m_Actor.Location.Map;
      Point myPos = m_Actor.Location.Position;
      int closeCount = 0;

      foreach (Actor follower in m_Actor.Followers) {
        if (follower.Location.Map == map) {
          if (Rules.GridDistance(follower.Location.Position, myPos) <= distance && ++closeCount >= halfGroup) return null;
          int dist = Rules.GridDistance(follower.Location.Position, myPos);
          if (target == null || dist > worstDist) {
            target = follower;
            worstDist = dist;
          }
        } else {	// not even on map
          if (target == null || int.MaxValue > worstDist) {
            target = follower;
            worstDist = int.MaxValue;
          }
		}
      }
      if (target == null) return null;
      return BehaviorPathTo(target.Location);
//    return BehaviorIntelligentBumpToward(target.Location.Position);
    }

    protected ActorAction BehaviorLeadActor(Percept target)
    {
      Actor target1 = target.Percepted as Actor;
      if (!m_Actor.CanTakeLeadOf(target1)) return null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, target1.Location.Position))
        return new ActionTakeLead(m_Actor, target1);
      return BehaviorIntelligentBumpToward(target1.Location.Position);
    }

    protected ActionUseItem BehaviorUseMedecine(int factorHealing, int factorStamina, int factorSleep, int factorCure, int factorSan)
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory == null || inventory.IsEmpty) return null;
      bool needHP = m_Actor.HitPoints < m_Actor.MaxHPs;
      bool needSTA = m_Actor.IsTired;
      bool needSLP = m_Actor.WouldLikeToSleep;
      bool needCure = m_Actor.Infection > 0;
      bool needSan = m_Actor.Model.Abilities.HasSanity && m_Actor.Sanity < 3*m_Actor.MaxSanity/4;
      if (!needHP && !needSTA && (!needSLP && !needCure) && !needSan) return null;
      ChoiceEval<ItemMedicine> choiceEval = Choose(inventory.GetItemsByType<ItemMedicine>(), it => true, it =>
      {
        int num = 0;
        if (needHP) num += factorHealing * it.Healing;
        if (needSTA) num += factorStamina * it.StaminaBoost;
        if (needSLP) num += factorSleep * it.SleepBoost;
        if (needCure) num += factorCure * it.InfectionCure;
        if (needSan) num += factorSan * it.SanityCure;
        return (float) num;
      }, (a, b) => a > b);
      if (choiceEval == null || (double) choiceEval.Value <= 0.0) return null;
      return new ActionUseItem(m_Actor, choiceEval.Choice); // legal only for OrderableAI
    }

    protected override ActorAction BehaviorChargeEnemy(Percept target)
    {
      Actor actor = target.Percepted as Actor;
      ActorAction tmpAction = BehaviorMeleeAttack(actor);
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location))
          return (ActorAction)BehaviorUseMedecine(0, 1, 0, 0, 0) ?? new ActionWait(m_Actor);
        tmpAction = BehaviorHeadFor(target.Location.Position);
        if (null == tmpAction) return null;
        if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) RunIfPossible();
        return tmpAction;
      } catch(System.Exception) {
        throw;
      } finally {
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = actor;
        }
      }
    }

    // sunk from BaseAI
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, Dictionary<Point, int> damage_field, ActorCourage courage, string[] emotes)
    {
      Percept target = FilterNearest(enemies);
      bool doRun = false;	// only matters when fleeing
      Actor enemy = target.Percepted as Actor;
      bool decideToFlee;
      if (enemy.HasEquipedRangedWeapon()) decideToFlee = false;
      else if (m_Actor.Model.Abilities.IsLawEnforcer && enemy.MurdersCounter > 0)
        decideToFlee = false;
      else if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, enemy.Location))
        decideToFlee = true;
      else if (m_Actor.Leader != null && ActorCourage.COURAGEOUS == courage) {
	    decideToFlee = false;
      } else {
        switch (courage) {
          case ActorCourage.COWARD:
            decideToFlee = true;
            doRun = true;
            break;
          case ActorCourage.CAUTIOUS:
          case ActorCourage.COURAGEOUS:
            decideToFlee = WantToEvadeMelee(m_Actor, courage, enemy);
            doRun = !HasSpeedAdvantage(m_Actor, enemy);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled courage");
        }
      }
      if (!decideToFlee && WillTireAfterAttack(m_Actor)) {
        decideToFlee = true;    // but do not run as otherwise we won't build up stamina
      }

      ActorAction tmpAction = null;

      if (decideToFlee) {
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[0], (object) enemy.Name));
        // All OrderableAI instances currently can both use map objects, and barricade
        // there is an inventory check requirement on barricading as well
        // due to preconditions it is mutually exclusive that a door be closable or barricadable
        {
        bool could_barricade = m_Actor.CouldBarricade();
        Dictionary<Point,DoorWindow> close_doors = new Dictionary<Point,DoorWindow>();
        Dictionary<Point,DoorWindow> barricade_doors = new Dictionary<Point,DoorWindow>();
        foreach(Point pt in Direction.COMPASS.Select(dir => m_Actor.Location.Position + dir)) {
          DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(pt) as DoorWindow;
          if (null == door) continue;
          if (!IsBetween(m_Actor.Location.Position, pt, enemy.Location.Position)) continue;
          if (m_Actor.CanClose(door)) {
            if ((!Rules.IsAdjacent(pt, enemy.Location.Position) || !enemy.CanClose(door))) close_doors[pt] = door;
          } else if (could_barricade && door.CanBarricade()) {
            barricade_doors[pt] = door;
          }
        }
        if (0 < close_doors.Count) {
          int i = game.Rules.Roll(0, close_doors.Count);
          foreach(DoorWindow door in close_doors.Values) {
            if (0 >= i--) {
              Objectives.Add(new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, door.Location));
              return new ActionCloseDoor(m_Actor, door);
            }
          }
        } else if (0 < barricade_doors.Count) {
          int i = game.Rules.Roll(0, barricade_doors.Count);
          foreach(DoorWindow door in barricade_doors.Values) {
            if (0 >= i--) {
              Objectives.Add(new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, door.Location));
              return new ActionBarricadeDoor(m_Actor, door);
            }
          }
        }
        }   // enable automatic GC
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && (Lighting.DARKNESS== m_Actor.Location.Map.Lighting || game.Rules.RollChance(FLEE_THROUGH_EXIT_CHANCE))) {
          tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.NONE);
          if (null != tmpAction) {
            bool flag3 = true;
            if (m_Actor.HasLeader) {
              Exit exitAt = m_Actor.Location.Exit;
              if (exitAt != null) flag3 = m_Actor.Leader.Location.Map == exitAt.ToMap;
            }
            if (flag3) {
              m_Actor.Activity = Activity.FLEEING;
              return tmpAction;
            }
          }
        }
        // XXX we should run for the exit here ...
        if (null==damage_field || !damage_field.ContainsKey(m_Actor.Location.Position)) {
          tmpAction = BehaviorUseMedecine(2, 2, 1, 0, 0);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        // XXX or run for the exit here
        tmpAction = BehaviorWalkAwayFrom(enemies.Select(p => p.Location.Position));
        if (null != tmpAction) {
          if (doRun) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(50))
            game.DoEmote(m_Actor, emotes[1]);
          return BehaviorMeleeAttack(target.Percepted as Actor);
        }
        return null;
      } // if (decldeToFlee)

      // redo the pause check
      if (m_Actor.Speed > enemy.Speed) {
        int dist = Rules.GridDistance(m_Actor.Location,target.Location);
        if (2==dist) {
          if (   !m_Actor.WillActAgainBefore(enemy)
              || !m_Actor.RunIsFreeMove)
            return new ActionWait(m_Actor);
          List<Point> legal_steps = m_Actor.LegalSteps;
          if (null != legal_steps) {
            // cannot close at normal speed safely; run-hit may be ok
            Dictionary<Point,ActorAction> dash_attack = new Dictionary<Point,ActorAction>();
            ReserveSTA(0,1,0,0);  // reserve stamina for 1 melee attack
            List<Point> attack_possible = legal_steps.Where(pt => Rules.IsAdjacent(pt,enemy.Location.Position)
              && (dash_attack[pt] = Rules.IsBumpableFor(m_Actor,new Location(m_Actor.Location.Map,pt))) is ActionMoveStep
              && RunIfAdvisable(pt)).ToList();
            ReserveSTA(0,0,0,0);  // baseline
            if (!attack_possible.Any()) return new ActionWait(m_Actor);
            // XXX could filter down attack_possible some more
            m_Actor.IsRunning = true;
            return dash_attack[attack_possible[game.Rules.Roll(0,attack_possible.Count)]];
          }
        }
      }

      // charge
      tmpAction = BehaviorChargeEnemy(target);
      if (null != tmpAction) {
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[2], (object) enemy.Name));
        return tmpAction;
      }
      return null;
    }

	protected ActorAction BehaviorPathTo(Location dest,int dist=0)
	{
      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      Map a_map = m_Actor.Location.Map;
	  if (dest.Map != a_map) {
        if (!m_Actor.Model.Abilities.AI_CanUseAIExits) return null;
        HashSet<Map> exit_maps = a_map.PathTo(dest.Map, out HashSet<Exit> valid_exits);

	    Exit exitAt = m_Actor.Location.Exit;
        if (exitAt != null && exit_maps.Contains(exitAt.ToMap))
          return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
	    navigate.GoalDistance(a_map.ExitLocations(valid_exits), m_Actor.Location.Position);
	  } else {
	    navigate.GoalDistance(dest.Position, m_Actor.Location.Position);
	  }
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      if (dist >= navigate.Cost(m_Actor.Location.Position)) return null;
      // XXX telepathy: do not block an exit which has a non-enemy at the other destination
      ActorAction tmp3 = DecideMove(PlanApproach(navigate));   // only called when no enemies in sight anyway
      if (null == tmp3) return null;
      if (tmp3 is ActionMoveStep tmp2) {
        Exit exitAt = a_map.GetExitAt(tmp2.dest.Position);
        Actor actorAt = exitAt?.Location.Actor;
        if (null!=actorAt && !m_Actor.IsEnemyOf(actorAt)) return null;
      }
      return tmp3;
	}

    protected override ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other == null || other.IsDead) return null;
	  if (other.Location.Map == m_Actor.Location.Map) {
        if (   FOV.Contains(other.Location.Position)
            && Rules.GridDistance(m_Actor.Location.Position, other.Location.Position) <= maxDist
            && null != m_Actor.MinStepPathTo(m_Actor.Location.Map, m_Actor.Location.Position, other.Location.Position))
            return new ActionWait(m_Actor);
	  }
	  ActorAction actorAction = BehaviorPathTo(other.Location);
      if (actorAction == null || !actorAction.IsLegal()) return null;
      if (actorAction is ActionMoveStep tmp) {
        if (  Rules.GridDistance(m_Actor.Location.Position, tmp.dest.Position) > maxDist
           || other.Location.Map != m_Actor.Location.Map)
           m_Actor.IsRunning = RunIfAdvisable(tmp.dest.Position);
	  }
      return actorAction;
    }

    // belongs with CivilianAI, or possibly OrderableAI but NatGuard may not have access to the crime listings
    protected ActorAction BehaviorEnforceLaw(RogueGame game, List<Percept> percepts)
    {
      Contract.Requires(m_Actor.Model.Abilities.IsLawEnforcer);
      if (percepts == null) return null;
      List<Percept> percepts1 = percepts.FilterT<Actor>(a => 0< a.MurdersCounter && !m_Actor.IsEnemyOf(a));
      if (null == percepts1) return null;
      if (!game.Rules.RollChance(LAW_ENFORCE_CHANCE)) return null;
      // XXX V.0.10.0 this needs a rethinking (a well-armed murderer may be of more use killing z, a weak one should be assassinated)
      Percept percept = FilterNearest(percepts1);
      Actor target = percept.Percepted as Actor;
      if (game.Rules.RollChance(Rules.ActorUnsuspicousChance(m_Actor, target))) {
        game.DoEmote(target, string.Format("moves unnoticed by {0}.", (object)m_Actor.Name));
        return null;
      }
      game.DoEmote(m_Actor, string.Format("takes a closer look at {0}.", (object) target.Name));
      int chance = Rules.ActorSpotMurdererChance(m_Actor, target);
      if (!game.Rules.RollChance(chance)) return null;
      game.DoMakeAggression(m_Actor, target);
      m_Actor.TargetActor = target;
      // players are special: they get to react to this first
      return new ActionSay(m_Actor, target, string.Format("HEY! YOU ARE WANTED FOR {0}!", "murder".QtyDesc(target.MurdersCounter).ToUpper()), (target.IsPlayer ? RogueGame.Sayflags.IS_IMPORTANT : RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION));
    }

    protected ActorAction BehaviorBuildLargeFortification(RogueGame game, int startLineChance)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, true)) return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || map.IsOnMapBorder(point.X, point.Y) || map.HasActorAt(point) || (map.HasExitAt(point) || map.IsInsideAt(point)))
          return false;
        int num1 = map.CountAdjacentTo(point, ptAdj => !map.GetTileModelAt(ptAdj).IsWalkable); // allows IsInBounds above
        int num2 = map.CountAdjacentTo(point, ptAdj => {
          Fortification fortification = map.GetMapObjectAt(ptAdj) as Fortification;
          return fortification != null && !fortification.IsTransparent;
        });
        return (num1 == 3 && num2 == 0 && game.Rules.RollChance(startLineChance)) || (num1 == 0 && num2 == 1);
      }, dir => game.Rules.Roll(0, 666), (a, b) => a > b);
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!game.Rules.CanActorBuildFortification(m_Actor, point1, true)) return null;
      return new ActionBuildFortification(m_Actor, point1, true);
    }

    protected bool IsDoorwayOrCorridor(Map map, Point pos)
    { // all of these can use IsInBounds
      if (!map.GetTileModelAt(pos).IsWalkable) return false;
      Point p5 = pos + Direction.NE;
      bool flag_ne = map.IsInBounds(p5) && !map.GetTileModelAt(p5).IsWalkable;
      Point p6 = pos + Direction.NW;
      bool flag_nw = map.IsInBounds(p6) && !map.GetTileModelAt(p6).IsWalkable;
      Point p7 = pos + Direction.SE;
      bool flag_se = map.IsInBounds(p7) && !map.GetTileModelAt(p7).IsWalkable;
      Point p8 = pos + Direction.SW;
      bool flag_sw = map.IsInBounds(p8) && !map.GetTileModelAt(p8).IsWalkable;
      bool no_corner = !flag_ne && !flag_se && !flag_nw && !flag_sw;
      if (!no_corner) return false;

      Point p1 = pos + Direction.N;
      bool flag_n = map.IsInBounds(p1) && !map.GetTileModelAt(p1).IsWalkable;
      Point p2 = pos + Direction.S;
      bool flag_s = map.IsInBounds(p2) && !map.GetTileModelAt(p2).IsWalkable;
      Point p3 = pos + Direction.E;
      bool flag_e = map.IsInBounds(p3) && !map.GetTileModelAt(p3).IsWalkable;
      Point p4 = pos + Direction.W;
      bool flag_w = map.IsInBounds(p4) && !map.GetTileModelAt(p4).IsWalkable;
      return (flag_n && flag_s && !flag_e && !flag_w) || (flag_e && flag_w && !flag_n && !flag_s);
    }

    protected ActorAction BehaviorBuildSmallFortification(RogueGame game)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, false)) return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || map.IsOnMapBorder(point.X, point.Y) || map.HasActorAt(point) || map.HasExitAt(point))
          return false;
        return IsDoorwayOrCorridor(map, point); // this allows using IsInBounds rather than IsValid
      }, dir => game.Rules.Roll(0, 666), (a, b) => a > b);
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!game.Rules.CanActorBuildFortification(m_Actor, point1, false)) return null;
      return new ActionBuildFortification(m_Actor, point1, false);
    }

    protected ActorAction BehaviorSleep(RogueGame game)
    {
      if (!m_Actor.CanSleep()) return null;
      Map map = m_Actor.Location.Map;
      // Do not sleep next to a door/window
      if (map.HasAnyAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt => map.GetMapObjectAt(pt) is DoorWindow)))
      {
        ActorAction actorAction = BehaviorWander(loc =>
        {
          if (!(map.GetMapObjectAt(loc.Position) is DoorWindow))
            return !map.HasAnyAdjacentInMap(loc.Position, (Predicate<Point>) (pt => loc.Map.GetMapObjectAt(pt) is DoorWindow));
          return false;
        });
        if (actorAction != null) return actorAction;
      }
      Item it = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
      if (m_Actor.IsOnCouch) {
        if (it is BatteryPowered) game.DoUnequipItem(m_Actor, it);
        return new ActionSleep(m_Actor);
      }

      // head for a couch if in plain sight
      Dictionary<Point,int> couches = new Dictionary<Point,int>();
      foreach (Point pt2 in m_Actor.Controller.FOV) {
        if (map.HasAnyAdjacentInMap(pt2, (Predicate<Point>)(pt => map.GetMapObjectAt(pt) is DoorWindow))) continue;
        if (map.HasActorAt(pt2)) continue;
        if (map.GetMapObjectAt(pt2)?.IsCouch ?? false) {
          couches[pt2] = Rules.GridDistance(m_Actor.Location.Position, pt2);
        }
      }
      ActorAction tmpAction = BehaviorEfficientlyHeadFor(couches);
      if (null != tmpAction) return tmpAction;

      // all battery powered items other than the police radio are left hand, currently
      // the police radio is DollPart.HIP_HOLSTER, *but* it recharges on movement faster than it drains
      if (it is BatteryPowered) game.DoUnequipItem(m_Actor, it);
      return new ActionSleep(m_Actor);
    }

    protected ActorAction BehaviorRestIfTired()
    {
      if (m_Actor.StaminaPoints >= Actor.STAMINA_MIN_FOR_ACTIVITY) return null;
      return new ActionWait(m_Actor);
    }

    public override bool IsInterestingItem(Item it)
    {
        if (IsItemTaboo(it)) return false;
        return base.IsInterestingItem(it);
    }

    public bool HasAnyInterestingItem(IEnumerable<Item> Items)
    {
      if (Items == null) return false;
      return Items.Any(it => IsInterestingItem(it));
    }

    public bool HasAnyInterestingItem(Inventory inv)
    {
      if (inv == null) return false;
      return HasAnyInterestingItem(inv.Items);
    }

    protected bool RHSMoreInteresting(Item lhs, Item rhs)
    {
      Contract.Requires(null != lhs);
      Contract.Requires(null != rhs);
      Contract.Requires(IsInterestingItem(rhs));    // lhs may be from inventory
      if (IsItemTaboo(rhs)) return false;
      if (IsItemTaboo(lhs)) return true;
      if (lhs.Model.ID == rhs.Model.ID) {
        if (lhs.Quantity < rhs.Quantity) return true;
        if (lhs.Quantity > rhs.Quantity) return false;
        if (lhs is BatteryPowered)
          {
          return ((lhs as BatteryPowered).Batteries < (rhs as BatteryPowered).Batteries);
          }
        else if (lhs is ItemFood && (lhs as ItemFood).IsPerishable)
          { // complicated
          int need = m_Actor.MaxFood - m_Actor.FoodPoints;
          int lhs_nutrition = (lhs as ItemFood).NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter);
          int rhs_nutrition = (rhs as ItemFood).NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter);
          if (lhs_nutrition==rhs_nutrition) return false;
          if (need < lhs_nutrition && need >= rhs_nutrition) return true;
          if (need < rhs_nutrition && need >= lhs_nutrition) return false;
          return lhs_nutrition < rhs_nutrition;
          }
        else if (lhs is ItemRangedWeapon)
          {
          return ((lhs as ItemRangedWeapon).Ammo < (rhs as ItemRangedWeapon).Ammo);
          }
        return false;
      }

      // if food is interesting, it will dominate non-food
      if (rhs is ItemFood) return !(lhs is ItemFood);
      else if (lhs is ItemFood) return false;

      // ranged weapons
      if (rhs is ItemRangedWeapon) return !(lhs is ItemRangedWeapon);
      else if (lhs is ItemRangedWeapon) return false;

      if (rhs is ItemAmmo) return !(lhs is ItemAmmo);
      else if (lhs is ItemAmmo) return false;

      if (rhs is ItemMeleeWeapon)
        {
        if (!(lhs is ItemMeleeWeapon)) return false;
        return (lhs.Model as ItemMeleeWeaponModel).Attack.Rating < (rhs.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
      else if (lhs is ItemMeleeWeapon) return false;

      if (rhs is ItemBodyArmor)
        {
        if (!(lhs is ItemBodyArmor)) return false;
        return (lhs as ItemBodyArmor).Rating < (rhs as ItemBodyArmor).Rating;
        }
      else if (lhs is ItemBodyArmor) return false;

      if (rhs is ItemGrenade) return !(lhs is ItemGrenade);
      else if (lhs is ItemGrenade) return false;

      bool lhs_low_priority = (lhs is ItemLight) || (lhs is ItemTrap) || (lhs is ItemMedicine) || (lhs is ItemEntertainment) || (lhs is ItemBarricadeMaterial);
      if ((rhs is ItemLight) || (rhs is ItemTrap) || (rhs is ItemMedicine) || (rhs is ItemEntertainment) || (rhs is ItemBarricadeMaterial)) return !lhs_low_priority;
      else if (lhs_low_priority) return false;

      bool wantCellPhone = (m_Actor.CountFollowers > 0 || m_Actor.HasLeader);
      if (rhs is ItemTracker)
        {
        if (!(lhs is ItemTracker)) return false;
        if (wantCellPhone && (rhs as ItemTracker).CanTrackFollowersOrLeader) return true;
        return false;
        }
      else if (lhs is ItemTracker) return false;

      return false;
    }

    public bool HasAnyTradeableItem()
    {
      Inventory inv = m_Actor.Inventory;
      if (inv == null) return false;
      return inv.Items.Any(it => IsTradeableItem(it));
    }

    public List<Item> GetTradeableItems()
    {
      Inventory inv = m_Actor.Inventory;
      if (inv == null) return null;
      IEnumerable<Item> ret = inv.Items.Where(it => IsTradeableItem(it));
      return ret.Any() ? ret.ToList() : null;
    }

    protected ActorAction BehaviorDropItem(Item it)
    {
      if (it == null) return null;
      // use stimulants before dropping them
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        if (m_Actor.Inventory.GetBestDestackable(it) is ItemMedicine stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
          if (num4 <= need &&  m_Actor.CanUse(stim2)) return new ActionUseItem(m_Actor, stim2);
        }
      }

      if (m_Actor.CanUnequip(it)) RogueForm.Game.DoUnequipItem(m_Actor,it);
      MarkItemAsTaboo(it,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);    // XXX can be called from simulation thread

      List<Point> has_container = new List<Point>();
      foreach(Point pos in Direction.COMPASS.Select(dir => m_Actor.Location.Position+dir)) {
        if (!m_Actor.Location.Map.IsValid(pos)) continue;
        MapObject container = m_Actor.Location.Map.GetMapObjectAt(pos);
        if (null == container) continue;
        if (!container.IsContainer) continue;
        Inventory itemsAt = m_Actor.Location.Map.GetItemsAt(pos);
        if (null != itemsAt)
          {
          if (itemsAt.CountItems+1 >= itemsAt.MaxCapacity) continue; // practical consideration
#if DEBUG
          if (itemsAt.IsFull) throw new InvalidOperationException("illegal put into container attempted");
#endif
          }
#if DEBUG
        if (!RogueForm.Game.Rules.CanActorPutItemIntoContainer(m_Actor, pos)) throw new InvalidOperationException("illegal put into container attempted");
#endif
        has_container.Add(pos);
      }
      if (0 < has_container.Count) return new ActionPutInContainer(m_Actor, it, has_container[RogueForm.Game.Rules.Roll(0, has_container.Count)]);

      return (m_Actor.CanDrop(it) ? new ActionDropItem(m_Actor, it) : null);
    }

    protected ActorAction BehaviorDropUselessItem()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item it in m_Actor.Inventory.Items) {
        if (it.IsUseless) return BehaviorDropItem(it);
      }
      ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
      if (null != armor) return BehaviorDropItem(armor);

      ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
      if (null != weapon) {
        int martial_arts_rating = m_Actor.UnarmedMeleeAttack().Rating;
        int weapon_rating = m_Actor.MeleeWeaponAttack(weapon.Model).Rating;
        if (weapon_rating <= martial_arts_rating) return BehaviorDropItem(weapon);
      }
      return null;
    }

    protected ActorAction BehaviorDropBoringEntertainment()
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory.IsEmpty) return null;
      foreach (Item it in inventory.Items) {
        if (it is ItemEntertainment && m_Actor.IsBoredOf(it))
          return new ActionDropItem(m_Actor, it);
      }
      return null;
    }

    protected ActorAction BehaviorUseEntertainment()
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory.IsEmpty) return null;
      ItemEntertainment itemEntertainment = inventory.GetFirst<ItemEntertainment>();
      if (itemEntertainment == null) return null;
      return (m_Actor.CanUse(itemEntertainment) ? new ActionUseItem(m_Actor, itemEntertainment) : null);
    }

#region stench killer
    // stench killer support -- don't want to lock down to the only user, CivilianAI
    // actually, this particular heuristic is *bad* because it causes the z to lose tracking too close to shelter.
    private bool IsGoodStenchKillerSpot(Map map, Point pos)
    {
      if (map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, pos) > 0) return false;
      if (PrevLocation.Map.IsInsideAt(PrevLocation.Position) != map.IsInsideAt(pos)) return true;
      if (map.HasExitAt(pos)) return true;
      return null != map.GetMapObjectAt(pos) as DoorWindow;
    }

    protected ItemSprayScent GetEquippedStenchKiller()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemSprayScent && (obj as ItemSprayScent).Model.Odor == Odor.PERFUME_LIVING_SUPRESSOR)
          return obj as ItemSprayScent;
      }
      return null;
    }

    protected ItemSprayScent GetFirstStenchKiller(Predicate<ItemSprayScent> fn)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      return m_Actor.Inventory.GetFirstMatching<ItemSprayScent>(fn);
    }

    protected ActorAction BehaviorUseStenchKiller()
    {
      ItemSprayScent itemSprayScent = m_Actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayScent;
      if (itemSprayScent == null) return null;
      if (itemSprayScent.IsUseless) return null;
      if (itemSprayScent.Model.Odor != Odor.PERFUME_LIVING_SUPRESSOR) return null;
      if (!IsGoodStenchKillerSpot(m_Actor.Location.Map, m_Actor.Location.Position)) return null;
      return (m_Actor.CanUse(itemSprayScent) ? new ActionUseItem(m_Actor, itemSprayScent) : null);
    }

    protected bool BehaviorEquipStenchKiller(RogueGame game)
    {
      if (!IsGoodStenchKillerSpot(m_Actor.Location.Map, m_Actor.Location.Position)) return false;
      if (GetEquippedStenchKiller() != null) return true;
      ItemSprayScent firstStenchKiller = GetFirstStenchKiller((Predicate<ItemSprayScent>)(it =>
      {
          return !it.IsUseless && !IsItemTaboo(it);
      }));
      if (firstStenchKiller != null && m_Actor.CanEquip(firstStenchKiller)) {
        game.DoEquipItem(m_Actor, firstStenchKiller);
        return true;
      }
      return false;
    }
#endregion

#region ground inventory stacks
    private List<Item> InterestingItems(IEnumerable<Item> Items)
    {
      if (Items == null) return null;
      HashSet<GameItems.IDs> exclude = new HashSet<GameItems.IDs>(Objectives.Where(o=>o is Goal_DoNotPickup).Select(o=>(o as Goal_DoNotPickup).Avoid));
      IEnumerable<Item> tmp = Items.Where(it => !exclude.Contains(it.Model.ID) && IsInterestingItem(it));
      return (tmp.Any() ? tmp.ToList() : null);
    }

    public List<Item> InterestingItems(Inventory inv)
    {
      return InterestingItems(inv?.Items);
    }

    protected ActorAction BehaviorMakeRoomFor(Item it)
    {
      Contract.Requires(null != it);
      Contract.Requires(m_Actor.Inventory.IsFull);
      Contract.Requires(IsInterestingItem(it));

      Inventory inv = m_Actor.Inventory;
      if (it.Model.IsStackable && it.CanStackMore) {
         inv.GetItemsStackableWith(it, out int qty);
         if (qty>=it.Quantity) return null;
      }

      // not-best body armor can be dropped
      if (2<=m_Actor.CountItemQuantityOfType(typeof (ItemBodyArmor))) {
        ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
        if (null != armor) return BehaviorDropItem(armor);
      }

      { // not-best melee weapon can be dropped
        List<ItemMeleeWeapon> melee = inv.GetItemsByType<ItemMeleeWeapon>();
        if (null != melee) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          if (2<=melee.Count) return BehaviorDropItem(weapon);
          if (it is ItemMeleeWeapon && weapon.Model.Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating) return BehaviorDropItem(weapon);
        }
      }

      // another behavior is responsible for pre-emptively eating perishable food
      // canned food is normally eaten at the last minute
      if (GameItems.IDs.FOOD_CANNED_FOOD == it.Model.ID && m_Actor.Model.Abilities.HasToEat && inv.GetBestDestackable(it) is ItemFood food) {
        // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
        int need = m_Actor.MaxFood - m_Actor.FoodPoints;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4 <= need && m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
      }
      // it should be ok to devour stimulants in a glut
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID && inv.GetBestDestackable(it) is ItemMedicine stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4 <= need && m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
      }

      // see if we can eat our way to a free slot
      if (m_Actor.Model.Abilities.HasToEat && inv.GetBestDestackable(GameItems.CANNED_FOOD) is ItemFood food2) {
        // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
        int need = m_Actor.MaxFood - m_Actor.FoodPoints;
        int num4 = m_Actor.CurrentNutritionOf(food2);
        if (num4*food2.Quantity <= need && m_Actor.CanUse(food2)) return new ActionUseItem(m_Actor, food2);
      }

      // finisbing off stimulants to get a free slot is ok
      if (inv.GetBestDestackable(GameItems.PILLS_SLP) is ItemMedicine stim2) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
        if (num4*stim2.Quantity <= need && m_Actor.CanUse(stim2)) return new ActionUseItem(m_Actor, stim2);
      }

      // priority classes of incoming items are:
      // food
      // ranged weapon
      // ammo for a ranged weapon in inventory
      // melee weapon
      // body armor
      // grenades (soldiers and civilians, screened at the interesting item check)
      // light, traps, barricading, medical/entertainment, stench killer (civilians, screened at the interesting item check)
      // trackers (mainly because AI can't use properly), but cell phones are trackers

      // trackers (mainly because AI can't use properly), but cell phones are trackers
      bool wantCellPhone = (m_Actor.CountFollowers > 0 || m_Actor.HasLeader);
      if (it is ItemTracker) {
        bool tracker_ok = false;
        if (wantCellPhone && GameItems.IDs.TRACKER_CELL_PHONE == it.Model.ID) tracker_ok = true;
        if (!tracker_ok) return null;   // tracker normally not worth clearing a slot for
      }
      // ditch an unwanted tracker if possible
      ItemTracker tmpTracker = inv.GetFirstMatching<ItemTracker>(it2 => !wantCellPhone || GameItems.IDs.TRACKER_CELL_PHONE != it2.Model.ID);
      if (null != tmpTracker) return BehaviorDropItem(tmpTracker);

      // these lose to everything other than trackers.  Note that we should drop a light to get a more charged light -- if we're right on top of it.
      if (it is ItemLight) return null;
      if (it is ItemTrap) return null;
      if (it is ItemMedicine) return null;
      if (it is ItemEntertainment) return null;
      if (it is ItemBarricadeMaterial) return null;

      // dropping body armor to get a better one should be ok
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null != armor && armor.Rating < (it as ItemBodyArmor).Rating) {
          return BehaviorDropItem(armor);
        }
      }

      // ditch unimportant items
      ItemBarricadeMaterial tmpBarricade = inv.GetFirstMatching<ItemBarricadeMaterial>(null);
      if (null != tmpBarricade) return BehaviorDropItem(tmpBarricade);
      ItemTrap tmpTrap = inv.GetFirstMatching<ItemTrap>(null);
      if (null != tmpTrap) return BehaviorDropItem(tmpTrap);
      ItemEntertainment tmpEntertainment = inv.GetFirstMatching<ItemEntertainment>(null);
      if (null != tmpEntertainment) return BehaviorDropItem(tmpEntertainment);
      ItemMedicine tmpMedicine = inv.GetFirstMatching<ItemMedicine>(null);
      if (null != tmpMedicine) return BehaviorDropItem(tmpMedicine);

      // least charged flashlight goes
      List<ItemLight> lights = inv.GetItemsByType<ItemLight>();
      if (null != lights && 2<=lights.Count) {
        int min_batteries = lights.Select(obj => obj.Batteries).Min();
        ItemLight discard = lights.Find(obj => obj.Batteries==min_batteries);
        return BehaviorDropItem(discard);
      }

      // uninteresting ammo
      ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>((Predicate<ItemAmmo>) (ammo => !IsInterestingItem(ammo)));
      if (null != tmpAmmo) {
        ItemRangedWeapon tmpRw = m_Actor.GetCompatibleRangedWeapon(tmpAmmo);
        if (null != tmpRw) {
          tmpAmmo = inv.GetBestDestackable(tmpAmmo) as ItemAmmo;
          if (m_Actor.CanUse(tmpAmmo)) return new ActionUseItem(m_Actor, tmpAmmo);
        }
        return BehaviorDropItem(tmpAmmo);
      }

      // ranged weapon with zero ammo is ok to drop for something other than its own ammo
      ItemRangedWeapon tmpRw2 = inv.GetFirstMatching<ItemRangedWeapon>((Predicate<ItemRangedWeapon>) (rw => 0 >= rw.Ammo));
      if (null != tmpRw2)
      {
         bool reloadable = (it is ItemAmmo ? (it as ItemAmmo).AmmoType==tmpRw2.AmmoType : false);
         if (!reloadable) return BehaviorDropItem(tmpRw2);
      }

      // grenades next
      if (it is ItemGrenade) return null;
      ItemGrenade tmpGrenade = inv.GetFirstMatching<ItemGrenade>(null);
      if (null != tmpGrenade) return BehaviorDropItem(tmpGrenade);

      // do not pick up trackers if it means dropping body armor or higher priority
      if (it is ItemTracker) return null;

      // body armor
      if (it is ItemBodyArmor) return null;

      // give up
      return null;
    }

    protected ActorAction BehaviorWouldGrabFromStack(Point position, Inventory stack)
    {
      if (stack == null || stack.IsEmpty) return null;

      MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(position);    // XXX this check should affect BehaviorResupply
      if (mapObjectAt != null && !mapObjectAt.IsContainer && !m_Actor.Location.Map.IsWalkableFor(position, m_Actor)) {
        // Cf. Actor::CanOpen
        if (mapObjectAt is DoorWindow doorWindow && doorWindow.IsBarricaded) return null;
        // Cf. Actor::CanPush; closed door/window is not pushable but can be handled
        else if (!mapObjectAt.IsMovable) return null; // would have to handle OnFire if that could happen
      }

      List<Item> interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      ActorAction recover = (m_Actor.Inventory.IsFull ? BehaviorMakeRoomFor(obj) : null);
      if (m_Actor.Inventory.IsFull && null == recover && !obj.Model.IsStackable) return null;

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = new ActionTakeItem(m_Actor, position, obj);
      if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
        if (null == recover) return null;
        if (!recover.IsLegal()) return null;
        return recover;
      }
      return (tmp.IsLegal() ? tmp : null);    // in case this is the biker/trap pickup crash [cairo123]
    }

    protected ActorAction BehaviorGrabFromStack(RogueGame game, Point position, Inventory stack)
    {
      if (stack == null || stack.IsEmpty) return null;

      MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(position);    // XXX this check should affect BehaviorResupply
      if (mapObjectAt != null && !mapObjectAt.IsContainer && !m_Actor.Location.Map.IsWalkableFor(position, m_Actor)) {
        // Cf. Actor::CanOpen
        if (mapObjectAt is DoorWindow doorWindow && doorWindow.IsBarricaded) return null;
        // Cf. Actor::CanPush; closed door is not pushable but can be handled
        else if (!mapObjectAt.IsMovable) return null; // would have to handle OnFire if that could happen
      }

      List<Item> interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      ActorAction recover = (m_Actor.Inventory.IsFull ? BehaviorMakeRoomFor(obj) : null);
      if (m_Actor.Inventory.IsFull && null == recover && !obj.Model.IsStackable) return null;

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = null;
      if (game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
      bool may_take = (position == m_Actor.Location.Position);
      // XXX ActionGetFromContainer is obsolete.  Bypass BehaviorIntelligentBumpToward for containers.
      // currently all containers are not-walkable for UI reasons.
      if (mapObjectAt != null && mapObjectAt.IsContainer /* && !m_Actor.Location.Map.IsWalkableFor(position, m_Actor) */
          && 1==Rules.GridDistance(m_Actor.Location.Position,position))
        may_take = true;

      if (may_take) {
        tmp = new ActionTakeItem(m_Actor, position, obj);
        if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
          if (null == recover) return null;
          if (!recover.IsLegal()) return null;
          if (recover is ActionDropItem) {
            if (obj.Model.ID == (recover as ActionDropItem).Item.Model.ID) return null;
            Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, (recover as ActionDropItem).Item.Model.ID));
          }
          Objectives.Add(new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter+1,m_Actor,tmp));
          return recover;
        }
        return (tmp.IsLegal() ? tmp : null);    // in case this is the biker/trap pickup crash [cairo123]
      }

      return BehaviorIntelligentBumpToward(position);
    }
#endregion

#if FAIL
    // Gun bunny AI support
    // for efficiency purposes we probably want to return both the probability of hitting (current)
    // and some proxy for the "damage distribution"
    protected Dictionary<Actor,Dictionary<Item,float>> AttackEfficiency(List<Actor> enemies)
    {
      List<Item> tmpMelee = m_Actor.Inventory.Items.Where(it => it.Model.GetType()==typeof(ItemMeleeWeaponModel));
      List<Item> tmpRanged = m_Actor.Inventory.Items.Where(it => it.Model.GetType()==typeof(ItemRangedWeaponModel));
      Dictionary<Actor,Dictionary<Item,float>> ret = new Dictionary<Actor,Dictionary<Item,float>>(enemies.Count);
      foreach(Actor tmp in enemies) {
        CurrentDefence tmpDef = tmp.CurrentDefence;
        Dictionary<Item,float> prob = new Dictionary<Item,float>();
        if (0 < tmpMelee.Count && 1==Rules.GridDistance(m_Actor.Location.Position,tmp.Location.Position)) {
          // m_CurrentMeleeAttack = (it.Model as ItemMeleeWeaponModel).BaseMeleeAttack(Sheet);
          foreach(Item it in tmpMelee) {
            Attack tmpAtt = (it.Model as ItemMeleeWeaponModel).BaseMeleeAttack(m_Actor.Sheet);
          }
        }
        if (0 < tmpRanged.Count) {
          // m_CurrentRangedAttack = (it.Model as ItemRangedWeaponModel).Attack;   // value-copy due to struct Attack
          foreach(Item it in tmpRanged) {
            Attack tmpAtt = (it.Model as ItemRangedWeaponModel).Attack;
            if (Rules.GridDistance(m_Actor.Location.Position,tmp.Location.Position) > tmp2.Range) continue;
          }
        }
      }
      return ret;
    }
#endif

    // XXX arguably should be member function; doing this for code locality
    protected static bool IsLegalPathingAction(ActorAction act)
    {
      if (act is ActionMoveStep) return true;
      if (act is ActionSwitchPlace) return true;
      if (act is ActionOpenDoor) return true;
      if (act is ActionBashDoor) return true;
      if (act is ActionMoveStep) return true;
      if (act is ActionBreak) return true;
      if (act is ActionPush) return true;
      if (act is ActionShove) return true;
      return false;
    }

    // April 7 2017: This is called directly only by the same-map threat and tourism behaviors
    // These two behaviors both like a "spread out" where each non-follower ally heads for the targets nearer to them than
    // to the other non-follower allies
    protected ActorAction BehaviorNavigate(IEnumerable<Point> tainted)
    {
      Contract.Requires(0<tainted.Count());
#if DEBUG
      Contract.Requires(!tainted.Contains(m_Actor.Location.Position));  // propagated up from FloodfillPathfinder::GoalDistance
#else
      if (tainted.Contains(m_Actor.Location.Position)) return null;
#endif

      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      navigate.GoalDistance(tainted, m_Actor.Location.Position);
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && !navigate.Domain.Contains(m_Actor.Location.Position)) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": navigate destination unreachable from ("+m_Actor.Location.Position.X.ToString()+","+ m_Actor.Location.Position.Y.ToString() + ")");
        List<string> msg = new List<string>();
        foreach(Point pt in navigate.Domain) {
          msg.Add("(" + pt.X.ToString() + "," + pt.Y.ToString() + "): " + navigate.Cost(pt));
        }
        msg.Sort();
        foreach(string x in msg) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, x);
        }
        msg.Clear();
        foreach(Point pt in navigate.black_list) {
          msg.Add("(" + pt.X.ToString() + "," + pt.Y.ToString() + "): " + navigate.Cost(pt));
        }
        msg.Sort();
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "black list");
        foreach(string x in msg) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, x);
        }
      }
#endif
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;

      Dictionary<Point, int> dest = PlanApproach(navigate);
      Dictionary<Point, int> exposed = new Dictionary<Point,int>();

      foreach(Point pt in dest.Keys) {
#if TRACE_NAVIGATE
        string err = "";
        ActorAction tmp = Rules.IsPathableFor(m_Actor,new Location(m_Actor.Location.Map,pt), out err);
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": ("+pt.X.ToString()+","+pt.Y.ToString()+") "+(null==tmp ? "null ("+err+")" : tmp.ToString()));
#else
        ActorAction tmp = Rules.IsPathableFor(m_Actor,new Location(m_Actor.Location.Map,pt));
#endif
        if (null == tmp || !tmp.IsLegal() || !IsLegalPathingAction(tmp)) continue;
        HashSet<Point> los = LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt));
        los.IntersectWith(tainted);
        exposed[pt] = los.Count;
      }
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && 0 >= dest.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no possible moves for navigation");
      if (m_Actor.IsDebuggingTarget && 0 >= exposed.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no acceptable moves for navigation");
      if (m_Actor.IsDebuggingTarget && 0 < exposed.Count) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": consdiring navigation from ("+m_Actor.Location.Position.X.ToString()+","+ m_Actor.Location.Position.Y.ToString() + ")");
        List<string> msg = new List<string>();
        msg.Add("src:(" + m_Actor.Location.Position.X.ToString() + "," + m_Actor.Location.Position.Y.ToString() + "): " + navigate.Cost(m_Actor.Location.Position).ToString());
        foreach(Point pt in exposed.Keys) {
          msg.Add("(" + pt.X.ToString() + "," + pt.Y.ToString() + "): " + navigate.Cost(pt).ToString() + "," + exposed[pt].ToString());
        }
        msg.Sort();
        foreach(string x in msg) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, x);
        }
        msg.Clear();
      }
#endif
      if (0 >= exposed.Count) return null;

      int most_exposed = exposed.Values.Max();
      if (0<most_exposed) exposed.OnlyIf(val=>most_exposed<=val);
      Dictionary<Point, int> costs = new Dictionary<Point,int>();
      foreach(Point pt in exposed.Keys) {
        costs[pt] = navigate.Cost(pt);
      }
      ActorAction ret = DecideMove(costs);
#if DEBUG
      if (null == ret) throw new InvalidOperationException("DecideMove failed in no-fail situation");
#endif
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && null == ret) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": refused to choose move for navigation");
#endif
      if (ret is ActionMoveStep test) {
        ReserveSTA(0,1,0,0);    // for now, assume we must reserve one melee attack of stamina (which is at least as much as one push/jump, typically)
        m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
        ReserveSTA(0,0,0,0);
      }
      return ret;
    }

    protected ActorAction BehaviorHastyNavigate(IEnumerable<Point> tainted)
    {
      Contract.Requires(0<tainted.Count());

      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      navigate.GoalDistance(tainted, m_Actor.Location.Position);
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      ActorAction ret = DecideMove(PlanApproach(navigate));
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    protected ActorAction BehaviorHeadForExit(Dictionary<Point,Exit> valid_exits)
    {
      Contract.Requires(null!=valid_exits);
      if (0 >= valid_exits.Count) return null;
      if (valid_exits.ContainsKey(m_Actor.Location.Position)) {
        return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      return BehaviorHastyNavigate(valid_exits.Keys);
    }

    protected bool HaveThreatsInCurrentMap()
    {
      ThreatTracking threats = m_Actor.Threats;
      if (null == threats) return false;
      HashSet<Point> tainted = ((m_Actor.Location.Map!=m_Actor.Location.Map.District.SewersMap || !Session.Get.HasZombiesInSewers) ? threats.ThreatWhere(m_Actor.Location.Map) : new HashSet<Point>());
      return 0<tainted.Count;   // XXX could be more efficient?
    }

    protected ActorAction BehaviorHuntDownThreatCurrentMap()
    {
      ThreatTracking threats = m_Actor.Threats;
      if (null == threats) return null;
      // 1) clear the current map, unless it's non-vintage sewers
      HashSet<Point> tainted = ((m_Actor.Location.Map!=m_Actor.Location.Map.District.SewersMap || !Session.Get.HasZombiesInSewers) ? threats.ThreatWhere(m_Actor.Location.Map) : new HashSet<Point>());
#if FALSE_POSITIVE
      if (0<tainted.Count) {
        ActorAction ret = BehaviorNavigate(tainted);
        if (null == ret) {
          List<string> locs = new List<string>(tainted.Count);
          foreach(Point pt in tainted) {
            locs.Add("\n"+(new Location(m_Actor.Location.Map,pt)).ToString());
          }
          throw new InvalidOperationException("unreachable threat destinations" + string.Concat(locs.ToArray()));
        }
        return ret;
      }
#else
      if (0<tainted.Count) return BehaviorNavigate(tainted);
#endif
      return null;
    }

    protected bool HaveTourismInCurrentMap()
    {
      LocationSet sights_to_see = m_Actor.InterestingLocs;
      if (null == sights_to_see) return false;
      HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map);
      return 0<tainted.Count;   // XXX could be more efficient?
    }

    protected ActorAction BehaviorTourismCurrentMap()
    {
      LocationSet sights_to_see = m_Actor.InterestingLocs;
      if (null == sights_to_see) return null;
      // 1) clear the current map.  Sewers is ok for this as it shouldn't normally be interesting
      HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map);
#if FALSE_POSITIVE
      if (0<tainted.Count) {
        ActorAction ret = BehaviorNavigate(tainted);
        if (null == ret) {
          List<string> locs = new List<string>(tainted.Count);
          foreach(Point pt in tainted) {
            locs.Add("\n"+(new Location(m_Actor.Location.Map,pt)).ToString());
          }
          locs.Sort();
          throw new InvalidOperationException("unreachable tourism destinations"+string.Concat(locs.ToArray()));
        }
        return ret;
      }
#else
      if (0<tainted.Count) return BehaviorNavigate(tainted);
#endif
      return null;
    }

    // note that the return value is aliased to the incoming value if no change is made
    private HashSet<Map> IgnoreMapsCoveredByAllies(HashSet<Map> possible_destinations)
    {
      HashSet<Actor> allies = m_Actor.Allies;
#if TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
      if (m_Actor.IsDebuggingTarget && null==allies) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": null==allies");
#endif
      if (null==allies) return possible_destinations;
      allies.IntersectWith(allies.Where(a => !a.HasLeader));
      allies.IntersectWith(allies.Where(a => possible_destinations.Contains(a.Location.Map)));
#if TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
      if (m_Actor.IsDebuggingTarget && 0 >= allies.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no allies in target maps");
#endif
      if (0 >= allies.Count) return possible_destinations;
#if TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
      if (m_Actor.IsDebuggingTarget) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": allies");
        foreach(Actor a in allies) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, a.Name+" in "+a.Location.Map.ToString());
        }
      }
#endif
      HashSet<Map> ret = new HashSet<Map>(possible_destinations);
#if TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
      if (m_Actor.IsDebuggingTarget) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+possible_destinations.Count.ToString()+","+ret.Count.ToString());
        foreach(Map m in ret) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m.ToString());
        }
      }
#endif
      ret.ExceptWith(allies.Select(a => a.Location.Map));
#if TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
      if (m_Actor.IsDebuggingTarget) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+possible_destinations.Count.ToString()+","+ret.Count.ToString());
        foreach(Map m in ret) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m.ToString());
        }
      }
#endif
      return ret;
    }

    protected ActorAction BehaviorHuntDownThreatOtherMaps()
    {
      ThreatTracking threats = m_Actor.Threats;
      if (null == threats) return null;

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit=>exit.IsAnAIExit);
      // XXX probably should exclude secret maps
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap).Where(map => !map.IsSecret));
      // but ignore the sewers if we're not vintage
      if (Session.Get.HasZombiesInSewers) {
        possible_destinations.Remove(m_Actor.Location.Map.District.SewersMap);
      }
      if (0>=possible_destinations.Count) return null;
      valid_exits.OnlyIf(e=>possible_destinations.Contains(e.ToMap));

      if (1==possible_destinations.Count && possible_destinations.Contains(m_Actor.Location.Map.District.EntryMap))
        return BehaviorHeadForExit(valid_exits);    // done

      // try to pick something reasonable
      Dictionary<Map,HashSet<Point>> hazards = new Dictionary<Map, HashSet<Point>>();
      foreach(Map m in possible_destinations) {
        hazards[m] = threats.ThreatWhere(m);
      }
      hazards.OnlyIf(val=>0<val.Count);
      if (hazards.ContainsKey(m_Actor.Location.Map.District.EntryMap)) {
        // if the entry map has a problem, go for it
        valid_exits.OnlyIf(e=>e.ToMap==m_Actor.Location.Map.District.EntryMap);
        return BehaviorHeadForExit(valid_exits);
      }
      if (0 >= hazards.Count) return null;  // defer to tourism
      possible_destinations.IntersectWith(hazards.Keys);
      valid_exits.OnlyIf(e=>possible_destinations.Contains(e.ToMap));

      // Non-entry map destinations with non-follower allies are already handled
      HashSet<Map> unhandled = IgnoreMapsCoveredByAllies(possible_destinations);
      if (0 >= unhandled.Count) return null;    // defer to tourism

      valid_exits.OnlyIf(e=>unhandled.Contains(e.ToMap));
      return BehaviorHeadForExit(valid_exits);
    }

    // XXX sewers are not guaranteed to be fully connected, so we want reachable exits
    protected ActorAction BehaviorTourismOtherMaps()
    {
      LocationSet sights_to_see = m_Actor.InterestingLocs;
      if (null == sights_to_see) return null;

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit=>exit.IsAnAIExit);
      // XXX probably should exclude secret maps
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap).Where(map => !map.IsSecret));

      if (1==possible_destinations.Count && possible_destinations.Contains(m_Actor.Location.Map.District.EntryMap))
        return BehaviorHeadForExit(valid_exits);    // done

      HashSet<Actor> allies = m_Actor.Allies ?? new HashSet<Actor>();
      allies.IntersectWith(allies.Where(a => !a.HasLeader));
      HashSet<Map> covered = new HashSet<Map>(allies.Select(a => a.Location.Map));
      ActorAction tmp = BehaviorPathTo(m => covered.Contains(m) ? new HashSet<Point>() : sights_to_see.In(m));
      if (null!=tmp) return tmp;

      // done here
      if (m_Actor.Location.Map == m_Actor.Location.Map.District.EntryMap) return null;    // where we need to be
      if (possible_destinations.Contains(m_Actor.Location.Map.District.EntryMap)) {
        valid_exits.OnlyIf(e=>e.ToMap==m_Actor.Location.Map.District.EntryMap);
        return BehaviorHeadForExit(valid_exits);
      }
      if (1 == possible_destinations.Count) return BehaviorHeadForExit(valid_exits);
#if DEBUG
      throw new InvalidOperationException("need Map::PathTo to handle hospital, police, etc. maps");
#else
      return null;    // XXX should use Map::PathTo but it doesn't have the ordering knowledge required yet
#endif
    }

    protected FloodfillPathfinder<Point> PathfinderFor(Func<Map, HashSet<Point>> targets_at)
    {
#if DEBUG
      if (null == targets_at) throw new ArgumentNullException(nameof(targets_at));
#endif
      FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      HashSet<Point> where_to_go = targets_at(m_Actor.Location.Map);
      if (0<where_to_go.Count) navigate.GoalDistance(where_to_go, m_Actor.Location.Position);
      if (!m_Actor.Model.Abilities.AI_CanUseAIExits) {
        if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
        return navigate;
      }

      // currently, there are no cross-district AI exits.
      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit=>exit.IsAnAIExit);
      valid_exits.OnlyIf(exit => {  // simulate Exit::ReasonIsBlocked
        MapObject mapObjectAt = exit.Location.MapObject;
        if (null == mapObjectAt) return true;
        if (mapObjectAt.IsCouch) return true;   // XXX probably not if someone's sleeping on it
        if (!mapObjectAt.IsJumpable) return false;
        return m_Actor.CanJump;
      });
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap).Where(m=>!m.IsSecret));
      Dictionary<Map,HashSet<Point>> hazards = new Dictionary<Map, HashSet<Point>>();
      foreach(Map m in possible_destinations) {
        hazards[m] = targets_at(m);
      }
      hazards.OnlyIf(val=>0<val.Count);
      if (0 >= hazards.Count) {
        if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
        if (0 >= where_to_go.Count) return null;
        return navigate;
      }
//    veto_hazards(hazards);
      foreach(KeyValuePair<Map,HashSet<Point>> m_dests in hazards) {
        Dictionary<Point,Exit> exits_for_m = new Dictionary<Point,Exit>(valid_exits);
        exits_for_m.OnlyIf(exit => exit.ToMap == m_dests.Key);
        List<Point> remote_dests = new List<Point>(exits_for_m.Values.Select(exit => exit.Location.Position));
        FloodfillPathfinder<Point> remote_navigate = m_dests.Key.PathfindSteps(m_Actor);
        remote_navigate.GoalDistance(m_dests.Value, remote_dests);

        foreach(KeyValuePair<Point, Exit> tmp in exits_for_m) {
          if (!remote_navigate.Domain.Contains(tmp.Value.Location.Position)) return null;
          int cost = remote_navigate.Cost(tmp.Value.Location.Position);
          if (int.MaxValue == cost) continue;   // should be same as not in domain, but evidently not.
          navigate.ReviseGoalDistance(tmp.Key, cost+1,m_Actor.Location.Position);
        }
      }
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      return navigate;
    }

    protected ActorAction BehaviorPathTo(FloodfillPathfinder<Point> navigate)
    {
      if (null == navigate) return null;
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      if (m_Actor.Model.Abilities.AI_CanUseAIExits) {
        List<Point> legal_steps = m_Actor.OnePathRange(m_Actor.Location.Map,m_Actor.Location.Position);
        int current_cost = navigate.Cost(m_Actor.Location.Position);
        if (null==legal_steps || !legal_steps.Any(pt => navigate.Cost(pt)<current_cost)) {
          return BehaviorUseExit(UseExitFlags.ATTACK_BLOCKING_ENEMIES | UseExitFlags.DONT_BACKTRACK);
        }
      }

      ActorAction ret = DecideMove(PlanApproach(navigate));
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
    {
      return BehaviorPathTo(PathfinderFor(targets_at));
    }

    protected ActorAction BehaviorResupply(HashSet<GameItems.IDs> critical)
    {
      return BehaviorPathTo(m => WhereIs(critical, m));
    }

    protected bool NeedsLight()
    {
      switch (m_Actor.Location.Map.Lighting)
      {
        case Lighting.DARKNESS: return true;
        case Lighting.LIT: return false;
#if DEBUG
        case Lighting.OUTSIDE:
#else
        default:
#endif
          if (!m_Actor.Location.Map.LocalTime.IsNight) return false;

          // use threat tracking/tourism when available
          ThreatTracking threats = m_Actor.Threats;
          LocationSet sights_to_see = m_Actor.InterestingLocs;
          int no_light_range = m_Actor.FOVrangeNoFlashlight(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
          HashSet<Point> no_light_FOV = LOS.ComputeFOVFor(m_Actor.Location, no_light_range);
          HashSet<Point> danger_point_FOV = LOS.ComputeFOVFor(m_Actor.Location, no_light_range+3);
          danger_point_FOV.ExceptWith(no_light_FOV);
          if (null!=threats) {
            HashSet<Point> tainted = threats.ThreatWhere(m_Actor.Location.Map);
            tainted.IntersectWith(danger_point_FOV);
            if (0<tainted.Count) return true;
          }
          if (null!=sights_to_see) {
            HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map);
            tainted.IntersectWith(danger_point_FOV);
            if (0<tainted.Count) return true;
          }
          if (null!=threats && null!=sights_to_see) return false;

          // resume legacy implementation
          if (Session.Get.World.Weather != Weather.HEAVY_RAIN) return !m_Actor.IsInside;
          return true;
#if DEBUG
        default: throw new ArgumentOutOfRangeException("unhandled lighting");
#endif
      }
    }

    // taboos
    public bool IsItemTaboo(Item it)
    {
      if (m_TabooItems == null) return false;
      return m_TabooItems.ContainsKey(it);
    }

    protected void MarkItemAsTaboo(Item it, int expiresTurn)
    {
      if (m_TabooItems == null) m_TabooItems = new Dictionary<Item,int>(1);
      else if (m_TabooItems.ContainsKey(it)) return;
      m_TabooItems.Add(it, expiresTurn);
    }

#if DEAD_FUNC
    public void MarkItemAsTaboo(Item it, Item alt)
    {
      if (m_TabooItems == null) return;
      else if (!m_TabooItems.ContainsKey(it)) return;
      m_TabooItems.Add(alt, m_TabooItems[it]);
    }

    protected void UnmarkItemAsTaboo(Item it)
    {
      if (m_TabooItems == null) return;
      m_TabooItems.Remove(it);
      if (m_TabooItems.Count == 0) m_TabooItems = null;
    }
#endif

    protected void MarkActorAsRecentTrade(Actor other)
    {
      if (m_TabooTrades == null) m_TabooTrades = new List<Actor>(1);
      else if (m_TabooTrades.Contains(other)) return;
      m_TabooTrades.Add(other);
    }

    public bool IsActorTabooTrade(Actor other)
    {
      if (m_TabooTrades == null) return false;
      return m_TabooTrades.Contains(other);
    }

    protected void ExpireTaboos()
    {
      // maintain taboo information
      int time = m_Actor.LastActionTurn;
      if (null != m_TabooItems) {
        m_TabooItems.OnlyIf(val => val<=time);
        if (0 == m_TabooItems.Count) m_TabooItems = null;
      }
      // actors ok to clear at midnight
      if (m_Actor.Location.Map.LocalTime.IsStrikeOfMidnight)
        m_TabooTrades = null;
    }
  }
}
