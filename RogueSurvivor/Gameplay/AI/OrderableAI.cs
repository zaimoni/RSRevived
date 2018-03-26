﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
// #define TRACE_NAVIGATE
#define INTEGRITY_CHECK_ITEM_RETURN_CODE
// #define TRACE_BEHAVIORPATHTO

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
#if DEBUG
         if (null == who) throw new ArgumentNullException(nameof(who));
#endif
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
#if DEBUG
        if (null == intent) throw new ArgumentNullException(nameof(intent));
#endif
        Intent = intent;
      }

      // always execute.  Expire on execution
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (!Intent.IsLegal()) {
          _isExpired = true;
          return true;
        }
        // XXX need some sense of what a combat action is
        if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) return false;
       _isExpired = true;
        if (Intent.IsLegal()) ret = Intent;
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
#if DEBUG
        if (null == intent) throw new ArgumentNullException(nameof(intent));
#endif
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
//      if (m_Actor.Inventory.Has(Avoid)) return false; // checking whether this is actually needed
        _isExpired = true;  // but expire if the offending item is not in LOS or inventory
        return false;
      }
    }

    [Serializable]
    internal class Goal_BreakLineOfSight : Objective
    {
      readonly private HashSet<Location> _locs;

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
        tmp = tmp.Intersect(m_Actor.Controller.FOV);
        if (!tmp.Any()) return true;
        if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) return false;
        ret = (m_Actor.Controller as OrderableAI).BehaviorWalkAwayFrom(tmp,null);
        return true;
      }

      public override string ToString()
      {
        return "Breaking line of sight to "+_locs.to_s();
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

        if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) return false;
        ret = (m_Actor.Controller as OrderableAI).BehaviorPathTo(m => new HashSet<Point>(_locs.Where(loc => loc.Map==m).Select(loc => loc.Position)));
        if (null == ret) return false;
        if (!ret.IsLegal()) return false;
        return true;
      }
    }

    [Serializable]
    internal class Goal_HintPathToActor : Objective
    {
      private readonly Actor _dest;

      public Goal_HintPathToActor(int t0, Actor who, Actor dest)
      : base(t0,who)
      {
        _dest = dest;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (turn < Session.Get.WorldTime.TurnCounter) { // deadline up
          _isExpired = true;
          return true;
        }
        if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) return false;
        if (Rules.IsAdjacent(m_Actor.Location,_dest.Location)) {
          ret = new ActionWait(m_Actor);    // XXX should try to optimize ActionWait to any constructive non-movement action
          return true;
        }
        Location? test = m_Actor.Location.Map.Denormalize(_dest.Location);
        if (null!=test && m_Actor.Controller.FOV.Contains(test.Value.Position)) {
          var goals = new Dictionary<Point, int>();
          goals[test.Value.Position] = Rules.GridDistance(test.Value, m_Actor.Location);
          ActorAction tmp = (m_Actor.Controller as OrderableAI).BehaviorEfficientlyHeadFor(goals);
          if (tmp?.IsLegal() ?? false) {
            ret = tmp;
            return true;
          }
        }

        IEnumerable<Point> dest_pts = Direction.COMPASS.Select(dir => m_Actor.Location.Position + dir).Where(pt => m_Actor.Location.Map.IsWalkableFor(pt, m_Actor));
        ret = (m_Actor.Controller as OrderableAI).BehaviorPathTo(m => (m == m_Actor.Location.Map ? new HashSet<Point>(dest_pts) : new HashSet<Point>()));
        return ret?.IsLegal() ?? false;
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

    // doesn't include no-enemies check, inside check or any directives/orders
    // would like to trigger pathing to inside to enable sleeping
    // but have to distinguish between AI w/o item memory and AI w/item memory, etc.
    protected bool WantToSleepNow { get {
      return m_Actor.WouldLikeToSleep && m_Actor.CanSleep();
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
          return ExecuteBuildFortification(order.Location, false);
        case ActorTasks.BUILD_LARGE_FORTIFICATION:
          return ExecuteBuildFortification(order.Location, true);
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
      tmpAction = BehaviorIntelligentBumpToward(location);
      if (null == tmpAction) return null;
      RunIfPossible();
      return tmpAction;
    }

    private ActorAction ExecuteBuildFortification(Location location, bool isLarge)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      if (!m_Actor.CanBuildFortification(location.Position, isLarge)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction = new ActionBuildFortification(m_Actor, location.Position, isLarge);
        if (!tmpAction.IsLegal()) return null;
        SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(location);
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
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(location);
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
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(location);
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
    public ActorAction BehaviorEfficientlyHeadFor(Dictionary<Point,int> goals)
    {
      if (0>=goals.Count) return null;
      List<Point> legal_steps = m_Actor.LegalSteps;
      if (null == legal_steps) return null;
      if (2 <= legal_steps.Count) legal_steps = DecideMove_WaryOfTraps(legal_steps);
      if (2 <= legal_steps.Count) {
        int min_dist = goals.Values.Min();
        int near_scale = goals.Count+1;
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
#if DEBUG
      if (null == speaker) throw new ArgumentNullException(nameof(speaker));
      if (!speaker.Model.Abilities.CanTrade) throw new ArgumentOutOfRangeException(nameof(speaker),"both parties trading must be capable of it");
      if (!m_Actor.Model.Abilities.CanTrade) throw new ArgumentOutOfRangeException(nameof(speaker),"both parties trading must be capable of it");
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
      IEnumerable<ItemRangedWeapon> tmp_rw = ((!Directives.CanFireWeapons || m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) ? null : m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)));
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
        if (0 >= map.PowerGenerators.Get.Count) return null;
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
      if (a.Location.Map == m_Actor.Location.Map && a.Controller.CanSee(m_Actor.Location) && m_Actor.Controller.CanSee(a.Location)) return true;
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
#if DEBUG
        if (null == it) throw new ArgumentNullException(nameof(it));
        if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException(m_Actor.Name+" cannot trade");
#endif
        if (it is ItemBodyArmor) return !it.IsEquipped; // XXX best body armor should be equipped
        if (it is ItemFood food)
            {
            if (!m_Actor.Model.Abilities.HasToEat) return true;
            if (m_Actor.IsHungry) return false;
            // only should trade away food that doesn't drop below threshold
            if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food))
              return food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
            return true;
            }
        if (it is ItemRangedWeapon rw)
            {
            if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return true;
            if (0 < rw.Ammo) return false;
            if (null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return false;
            return true;    // more work needed
            }
        if (it is ItemAmmo am)
            {
            if (m_Actor.GetCompatibleRangedWeapon(am) == null) return true;
            return m_Actor.HasAtLeastFullStackOf(it, 2);
            }
        if (it is ItemMeleeWeapon)
            {
            Attack martial_arts = m_Actor.UnarmedMeleeAttack();
            if ((it.Model as ItemMeleeWeaponModel).Attack.Rating <= martial_arts.Rating) return true;
            if (2<=m_Actor.Inventory.Count(it.Model)) return true;  // trading away a spare is ok
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

    protected void BehaviorEquipBodyArmor()
    {
      ItemBodyArmor bestBodyArmor = m_Actor.GetBestBodyArmor();
      if (bestBodyArmor == null) return;
      if (GetEquippedBodyArmor() != bestBodyArmor) RogueForm.Game.DoEquipItem(m_Actor, bestBodyArmor);
    }

    protected ActorAction ManageMeleeRisk(List<Point> legal_steps, List<Point> retreat, List<Point> run_retreat, bool safe_run_retreat, List<ItemRangedWeapon> available_ranged_weapons, List<Percept> friends, List<Percept> enemies, List<Actor> slow_melee_threat)
    {
      ActorAction tmpAction = null;
      if ((null != retreat || null != run_retreat) && null != available_ranged_weapons && null!=enemies) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(retreat, enemies);
        MaximizeRangedTargets(run_retreat, enemies);
        IEnumerable<Actor> fast_enemies = enemies.Select(p => p.Percepted as Actor).Where(a => a.Speed >= 2 * m_Actor.Speed);   // typically rats.
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
	      tmpAction = DecideMove(retreat);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        // have slow enemies nearby
        if (null != slow_melee_threat) {
	      tmpAction = DecideMove(retreat);
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
        ItemAmmo ammo = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
        if (null != ammo) return new ActionUseItem(m_Actor, ammo);
      }
      return null;
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
    protected ActorAction BehaviorEquipWeapon(RogueGame game, List<Point> legal_steps, Dictionary<Point,int> damage_field, List<ItemRangedWeapon> available_ranged_weapons, List<Percept> enemies, HashSet<Actor> immediate_threat)
    {
#if DEBUG
      if ((null == available_ranged_weapons) != (null == GetBestRangedWeaponWithAmmo())) throw new InvalidOperationException("(null == available_ranged_weapons) != (null == GetBestRangedWeaponWithAmmo())");
#endif

      // migrated from CivilianAI::SelectAction
      ActorAction tmpAction = null;
      if (null != enemies) {
        if (1==Rules.GridDistance(enemies[0].Location,m_Actor.Location)) {
          // something adjacent...check for one-shotting
          ItemMeleeWeapon tmp_melee = m_Actor.GetBestMeleeWeapon();
          if (null!=tmp_melee) {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              tmpAction = BehaviorMeleeSnipe(en, m_Actor.MeleeWeaponAttack(tmp_melee.Model, en),null==immediate_threat || (1==immediate_threat.Count && immediate_threat.Contains(en)));
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
        foreach(ItemRangedWeapon rw in available_ranged_weapons) {
          if (rw.Model.MaxAmmo <= rw.Ammo) continue;
          ItemAmmo am = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
          if (null == am) continue;
          if (0 == rw.Ammo || (rw.Model.MaxAmmo - rw.Ammo) >= am.Quantity) {
            tmpAction = Equip(rw);
            if (null != tmpAction) return tmpAction;
            return new ActionUseItem(m_Actor,am);
          }
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
	        tmpAction = DecideMove(tmp);
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
    protected bool BehaviorEquipLight()
    {
      if (!NeedsLight()) return false;
      ItemLight tmp = GetEquippedLight();
      if (!tmp?.IsUseless ?? false) return true;
      tmp = m_Actor.Inventory.GetFirstMatching<ItemLight>(it => !it.IsUseless);
      if (tmp != null && m_Actor.CanEquip(tmp)) {
        RogueForm.Game.DoEquipItem(m_Actor, tmp);
        return true;
      }
      return false;
    }

    /// <returns>true if and only if a cell phone is required to be equipped</returns>
    protected bool BehaviorEquipCellPhone(RogueGame game)
    {
      bool wantCellPhone = m_Actor.NeedActiveCellPhone; // XXX could dial 911, at least while that desk is manned
      ItemTracker equippedCellPhone = m_Actor.GetEquippedCellPhone();
      if (equippedCellPhone != null) {
        if (wantCellPhone) return true;
        game.DoUnequipItem(m_Actor, equippedCellPhone);
      }
      if (!wantCellPhone) return false;
      ItemTracker firstTracker = m_Actor.Inventory.GetFirstMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless);
      if (firstTracker != null && m_Actor.CanEquip(firstTracker)) {
        game.DoEquipItem(m_Actor, firstTracker);
        return true;
      }
      return false;
    }

    /// <returns>null, or a legal ActionThrowGrenade</returns>
    protected ActorAction BehaviorThrowGrenade(RogueGame game, List<Percept> enemies)
    {
      if (3 > (enemies?.Count ?? 0)) return null;
      ItemGrenade firstGrenade = m_Actor.Inventory.GetFirstMatching<ItemGrenade>();
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
      Dictionary<Point,Actor> friends = map.FindAdjacent(m_Actor.Location.Position,(m,pt) => {
        Actor a = m.GetActorAtExt(pt);
        if (null == a) return null;
        if (a.IsSleeping) return null;
        return (m_Actor.IsEnemyOf(a) ? null : a);
      });
      if (0 >= friends.Count) return null;
      Actor actorAt1 = RogueForm.Game.Rules.Choose(friends).Value;
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
        if ((double) Rules.StdDistance(percept.Location, actorAt1.Location) <= (double) (2 + num)) return null;
        text = string.Format("I saw {0} {1} {2}.", it.AName, str1, str2);
      } else {
        if (!(percept.Percepted is string)) throw new InvalidOperationException("unhandled percept.Percepted type");
        text = string.Format("I heard {0} {1} {2}!", (object) (percept.Percepted as string), str1, str2);
      }
      return new ActionSay(m_Actor, actorAt1, text, RogueGame.Sayflags.NONE);
    }

    protected ActionCloseDoor BehaviorCloseDoorBehindMe(Location previousLocation)
    {
      DoorWindow door = previousLocation.Map.GetMapObjectAt(previousLocation.Position) as DoorWindow;
      if (null == door) return null;
      if (!m_Actor.CanClose(door)) return null;
      foreach(Direction dir in Direction.COMPASS) {
        Actor actor = previousLocation.Map.GetActorAtExt(previousLocation.Position+dir);
        if (null == actor) continue;
        if (actor.Controller is ObjectiveAI ai) {
          Dictionary<Point, int> tmp = ai.MovePlanIf(actor.Location.Position);
          if (tmp?.ContainsKey(previousLocation.Position) ?? false) return null;
        }
      }
      return new ActionCloseDoor(m_Actor, door, true);
    }

    private ActorAction BehaviorSecurePerimeter()
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
            ActionCloseDoor tmp = new ActionCloseDoor(m_Actor, door, m_Actor.Location == PrevLocation);
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
#if DEBUG
      if (null == nearestEnemy) throw new ArgumentNullException(nameof(nearestEnemy));
#endif
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
      // need an after-action "hint" to the target on where/who to go to
      if (!m_Actor.WillActAgainBefore(target1)) {
        int t0 = Session.Get.WorldTime.TurnCounter+m_Actor.HowManyTimesOtherActs(1,target1)-(m_Actor.IsBefore(target1) ? 1 : 0);
        (target1.Controller as OrderableAI)?.Objectives.Insert(0,new Goal_HintPathToActor(t0, target1, m_Actor));    // AI disallowed from leading player so fine
      }
      return BehaviorIntelligentBumpToward(target1.Location);
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
      ChoiceEval<ItemMedicine> choiceEval = Choose(inventory.GetItemsByType<ItemMedicine>(), it =>
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
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location)) {
          var tmp = m_Actor.BestMeleeAttack(actor);
#if DEBUG
          tmpAction = DoctrineRecoverSTA(Actor.STAMINA_MIN_FOR_ACTIVITY + Rules.STAMINA_COST_MELEE_ATTACK + tmp.StaminaPenalty);
          if (null == tmpAction) throw new ArgumentNullException(nameof(tmpAction));
          return tmpAction;
#else
          return DoctrineRecoverSTA(Actor.STAMINA_MIN_FOR_ACTIVITY + Rules.STAMINA_COST_MELEE_ATTACK + tmp.StaminaPenalty);
#endif
        }
        tmpAction = BehaviorHeadFor(target.Location);
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

    private HashSet<Point> AlliesNeedLoFvs(Actor enemy)
    {
      var friends = m_Actor.Controller.friends_in_FOV;
      if (null == friends) return null;
      var ret = new HashSet<Point>();
      var LoF = new List<Point>();
      foreach(var friend_where in friends) {
        var rw = (friend_where.Value.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo();
        if (null == rw) continue;
        if (!friend_where.Value.IsEnemyOf(enemy)) continue;
        // XXX not quite right (should use best reloadable weapon)
        if (friend_where.Value.CanFireAt(enemy,LoF)) {
          ret.UnionWith(LoF);
          continue;
        }
        int range = rw.Model.Attack.Range;
        // XXX not quite right (visibiblity range check omitted)
        if (LOS.CanTraceViewLine(new Location(m_Actor.Location.Map,friend_where.Key), enemy.Location, range, LoF)) {
          ret.UnionWith(LoF);
          continue;
        }
      }
      return (0 < ret.Count ? ret : null);
    }

    public ActorAction BehaviorWalkAwayFrom(IEnumerable<Point> goals, HashSet<Point> LoF_reserve)
    {
      Actor leader = m_Actor.LiveLeader;
      ItemRangedWeapon leader_rw = (null != leader ? leader.GetEquippedWeapon() as ItemRangedWeapon : null);
      Actor actor = (null != leader_rw ? GetNearestTargetFor(m_Actor.Leader) : null);
      bool checkLeaderLoF = actor != null && actor.Location.Map == m_Actor.Location.Map;
      List<Point> leaderLoF = null;
      if (checkLeaderLoF) {
        leaderLoF = new List<Point>(1);
        LOS.CanTraceFireLine(leader.Location, actor.Location, leader_rw.Model.Attack.Range, leaderLoF);
      }
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location location = m_Actor.Location + dir;
        if (!IsValidFleeingAction(Rules.IsBumpableFor(m_Actor, location))) return float.NaN;
        float num = SafetyFrom(location.Position, goals);
        if (LoF_reserve?.Contains(location.Position) ?? false) --num;
        if (null != leader) {
          num -= (float)Rules.StdDistance(location, leader.Location);
          if (leaderLoF?.Contains(location.Position) ?? false) --num;
        }
        return num;
      }, (a, b) => a > b);
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    private ActorAction BehaviorFlee(Actor enemy, Dictionary<Point, int> damage_field, HashSet<Point> LoF_reserve, bool doRun, string[] emotes)
    {
      var game = RogueForm.Game;
      ActorAction tmpAction = null;
      if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
        game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[0], (object) enemy.Name));
        // All OrderableAI instances currently can both use map objects, and barricade
        // there is an inventory check requirement on barricading as well
        // due to preconditions it is mutually exclusive that a door be closable or barricadable
        // however, we do not want to obstruct line of fire of allies
        {
        bool could_barricade = m_Actor.CouldBarricade();
        Dictionary<Point,DoorWindow> close_doors = new Dictionary<Point,DoorWindow>();
        Dictionary<Point,DoorWindow> barricade_doors = new Dictionary<Point,DoorWindow>();
        foreach(Point pt in Direction.COMPASS.Select(dir => m_Actor.Location.Position + dir)) {
          if (LoF_reserve?.Contains(pt) ?? false) continue;
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
              Objectives.Insert(0,new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, door.Location));
              return new ActionCloseDoor(m_Actor, door, m_Actor.Location == PrevLocation);
            }
          }
        } else if (0 < barricade_doors.Count) {
          int i = game.Rules.Roll(0, barricade_doors.Count);
          foreach(DoorWindow door in barricade_doors.Values) {
            if (0 >= i--) {
              Objectives.Insert(0,new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, door.Location));
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
        tmpAction = (null!= m_Actor.Controller.enemies_in_FOV ? BehaviorWalkAwayFrom(m_Actor.Controller.enemies_in_FOV.Keys, LoF_reserve) : null);
        if (null != tmpAction) {
          if (doRun) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(50))
            game.DoEmote(m_Actor, emotes[1]);
          return BehaviorMeleeAttack(enemy);
        }
        return null;
    }

    // sunk from BaseAI
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, Dictionary<Point, int> damage_field, ActorCourage courage, string[] emotes)
    {
      // this needs a serious rethinking; dashing into an ally's line of fire is immersion-breaking.
      Percept target = FilterNearest(enemies);
      List<Point> legal_steps = m_Actor.LegalSteps; // XXX should be passing this in instead
      Actor enemy = target.Percepted as Actor;

      bool doRun = false;	// only matters when fleeing
      bool decideToFlee = (null != legal_steps);
      if (decideToFlee) {
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
      }

      var LoF_reserve = AlliesNeedLoFvs(enemy);
      ActorAction tmpAction = null;
      if (decideToFlee) {
        tmpAction = BehaviorFlee(enemy, damage_field, LoF_reserve, doRun, emotes);
        if (null != tmpAction) return tmpAction;
      }

      // redo the pause check
      if (m_Actor.Speed > enemy.Speed && 2 == Rules.GridDistance(m_Actor.Location, target.Location)) {
          if (   !m_Actor.WillActAgainBefore(enemy)
              || !m_Actor.RunIsFreeMove)    // XXX assumes eneumy wants to close
            return new ActionWait(m_Actor);
          if (null != legal_steps) {
            // cannot close at normal speed safely; run-hit may be ok
            Dictionary<Point,ActorAction> dash_attack = new Dictionary<Point,ActorAction>();
            ReserveSTA(0,1,0,0);  // reserve stamina for 1 melee attack
            List<Point> attack_possible = legal_steps.Where(pt => Rules.IsAdjacent(pt,enemy.Location.Position)
              && !(LoF_reserve?.Contains(pt) ?? false)
              && (dash_attack[pt] = Rules.IsBumpableFor(m_Actor,new Location(m_Actor.Location.Map,pt))) is ActionMoveStep
              && RunIfAdvisable(pt)).ToList();
            ReserveSTA(0,0,0,0);  // baseline
            if (!attack_possible.Any()) return new ActionWait(m_Actor);
            // XXX could filter down attack_possible some more
            m_Actor.IsRunning = true;
            return dash_attack[attack_possible[game.Rules.Roll(0,attack_possible.Count)]];
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
#if TRACE_BEHAVIORPATHTO
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "in BehaviorPathTo; "+ m_Actor.Model.Abilities.AI_CanUseAIExits.ToString());
#endif
        HashSet<Map> exit_maps = a_map.PathTo(dest.Map, out HashSet<Exit> valid_exits);
#if TRACE_BEHAVIORPATHTO
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "valid_exits: "+ valid_exits.to_s());
#endif
        if (!m_Actor.Model.Abilities.AI_CanUseAIExits) {
          exit_maps.RemoveWhere(m=> m!=m.District.EntryMap);
          valid_exits.RemoveWhere(exit => !exit_maps.Contains(exit.ToMap));
        }
        if (m_Actor.Model.Abilities.AI_CanUseAIExits) {
          Exit exitAt = m_Actor.Location.Exit;
          if (exitAt != null && exit_maps.Contains(exitAt.ToMap))
            return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
        }

        var goals = a_map.ExitLocations(valid_exits);
#if TRACE_BEHAVIORPATHTO
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null==goals ? "null" : goals.ToString()));
#endif
        if (null == goals) return null;
	    navigate.GoalDistance(goals, m_Actor.Location.Position);
	  } else {
	    navigate.GoalDistance(dest.Position, m_Actor.Location.Position);
	  }
#if TRACE_BEHAVIORPATHTO
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "In domain: "+ navigate.Domain.Contains(m_Actor.Location.Position).ToString());
#endif
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
#if TRACE_BEHAVIORPATHTO
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "apparent range: "+ dist.ToString() + " <=> " + navigate.Cost(m_Actor.Location.Position).ToString());
#endif
      if (dist >= navigate.Cost(m_Actor.Location.Position)) return null;
      // XXX telepathy: do not block an exit which has a non-enemy at the other destination
      ActorAction tmp3 = DecideMove(PlanApproach(navigate));   // only called when no enemies in sight anyway
      if (null == tmp3) tmp3 = PlanApproachFailover(navigate);
      if (null == tmp3) return null;
      if (tmp3 is ActionMoveStep tmp2) {
        Exit exitAt = a_map.GetExitAt(tmp2.dest.Position);
        Actor actorAt = exitAt?.Location.Actor;
        if (null!=actorAt && !m_Actor.IsEnemyOf(actorAt)) return null;
      }
      return tmp3;
	}

    protected ActorAction BehaviorHangAroundActor(RogueGame game, Actor other, int minDist, int maxDist)
    {
      if (other == null || other.IsDead) return null;
      Point otherPosition = other.Location.Position;
      int num = 0;
      Location loc;
      do {
        Point p = otherPosition;
        p.X += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        p.Y += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        loc = new Location(other.Location.Map,p);
        if (100 < ++num) return null;
        if (loc == m_Actor.Location) return new ActionWait(m_Actor);    // XXX check what BehaviorIntelligentBumpToward does
        if (!loc.IsWalkableFor(m_Actor)) continue;
      }
      while(Rules.GridDistance(loc,other.Location) < minDist);

	  ActorAction actorAction = BehaviorPathTo(loc);
      if (!actorAction?.IsLegal() ?? true) return null;
      if (actorAction is ActionMoveStep tmp) {
        if (Rules.GridDistance(m_Actor.Location, tmp.dest) > maxDist)
           m_Actor.IsRunning = RunIfAdvisable(tmp.dest.Position);
	  }
      return actorAction;
    }

    protected override ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other == null || other.IsDead) return null;
	  if (other.Location.Map == m_Actor.Location.Map) {
        if (   CanSee(other.Location)
            && Rules.GridDistance(m_Actor.Location, other.Location) <= maxDist
            && null != m_Actor.MinStepPathTo(m_Actor.Location, other.Location))
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
#if DEBUG
      if (!m_Actor.Model.Abilities.IsLawEnforcer) throw new InvalidOperationException("!m_Actor.Model.Abilities.IsLawEnforcer");
#endif
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
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < m_Actor.BarricadingMaterialNeedForFortification(true)) return null;
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
      if (!m_Actor.CanBuildFortification(point1, true)) return null;
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
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < m_Actor.BarricadingMaterialNeedForFortification(false)) return null;
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
      if (!m_Actor.CanBuildFortification(point1, false)) return null;
      return new ActionBuildFortification(m_Actor, point1, false);
    }

    /// <returns>0 for disallowed, 1 for allowed, 2+ for "better".</returns>
    protected int SleepLocationRating(Location loc)
    { 
      // the legacy tests
      if (loc.Map.GetMapObjectAtExt(loc.Position) is DoorWindow) return -1;  // contextual; need to be aware of doors
      if (loc.Map.HasAnyAdjacentInMap(loc.Position, pt => loc.Map.GetMapObjectAtExt(pt) is DoorWindow)) return 0;
      if (loc.Map.HasExitAtExt(loc.Position)) return 0;    // both unsafe, and problematic for pathing in general
      if (m_Actor.Location!=loc && loc.Map.HasActorAt(loc.Position)) return 0;  // contextual

      // geometric code (walls, etc)
      if (!loc.Map.IsInsideAtExt(loc.Position)) return 0;
      if (!loc.Map.GetTileModelAtExt(loc.Position).IsWalkable) return 0;
      // we don't want to sleep next to anything that looks like an ex-door
      bool[] walls = Direction.COMPASS.Select(dir => loc.Map.GetTileModelAtExt(loc.Position+dir).IsWalkable).ToArray();

      // reference code...likely not optimal, but easy to verify
      if (walls[Direction.N.Index] && walls[Direction.S.Index]) return 0;
      if (walls[Direction.W.Index] && walls[Direction.E.Index]) return 0;
      if (walls[Direction.N.Index] && walls[Direction.NW.Index] != walls[Direction.NE.Index]) return 0;
      if (walls[Direction.S.Index] && walls[Direction.SW.Index] != walls[Direction.SE.Index]) return 0;
      if (walls[Direction.W.Index] && walls[Direction.NW.Index] != walls[Direction.SW.Index]) return 0;
      if (walls[Direction.E.Index] && walls[Direction.NE.Index] != walls[Direction.SE.Index]) return 0;

      // contextual code.  Context-free version would return int.MaxValue to request this.  static value would asusmed "passed"
      // XXX \todo if the LOS has a non-broken door then we're on the wrong side.  We should be pathing to it, but *not* securing the perimter.
      // XXX \todo treat already-sleeping as "somewhat like a wall"...want to be very sure it is possible to leave without waking anyone up
      // more geometric code (approval rather than veto)
      return 1;
    }

    private Dictionary<Point, int> GetSleepLocsInLOS(out Dictionary<Point, int> couches, out Dictionary<Point,int> doors)
    {
      couches = new Dictionary<Point,int>();
      doors = new Dictionary<Point, int>();
      var ret = new Dictionary<Point,int>();
      foreach(Point pt in m_Actor.Controller.FOV) {
        int rating = SleepLocationRating(new Location(m_Actor.Location.Map, pt));
        if (-1 == rating) {
          // use the same delta-encoding system that Map::Normalize does
          int delta = 0;
          if (m_Actor.Location.Position.X<pt.X) delta += 1;
          else if (m_Actor.Location.Position.X > pt.X) delta -= 1;
          if (m_Actor.Location.Position.Y < pt.Y) delta += 3;
          else if (m_Actor.Location.Position.Y > pt.Y) delta -= 3;
          doors[pt] = delta;
        }
        if ( 0 >= rating) continue;
        int dist = Rules.GridDistance(m_Actor.Location.Position, pt);
        ret[pt] = dist;
        if (m_Actor.Location.Map.GetMapObjectAt(pt)?.IsCouch ?? false) couches[pt] = dist;
      }
      return ret;
    }

    private ActorAction BehaviorSleep(Dictionary<Point,int> sleep_locs, Dictionary<Point,int> couches)
    {
#if DEBUG
      if (0 >= (sleep_locs?.Count ?? 0)) throw new ArgumentNullException(nameof(sleep_locs));
      if (null == couches) throw new ArgumentNullException(nameof(couches));
#endif
      if (!m_Actor.CanSleep()) return null;
      Map map = m_Actor.Location.Map;
      // Do not sleep next to a door/window
      if (0>=SleepLocationRating(m_Actor.Location)) {
        return BehaviorEfficientlyHeadFor(0<couches.Count ? couches : sleep_locs);  // null return ok here?
      }
      Item it = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
      if (m_Actor.IsOnCouch) {
        if (it is BatteryPowered) RogueForm.Game.DoUnequipItem(m_Actor, it);
        return new ActionSleep(m_Actor);
      }

      // head for a couch if in plain sight
      ActorAction tmpAction = BehaviorEfficientlyHeadFor(couches);
      if (null != tmpAction) return tmpAction;

      // all battery powered items other than the police radio are left hand, currently
      // the police radio is DollPart.HIP_HOLSTER, *but* it recharges on movement faster than it drains
      if (it is BatteryPowered) RogueForm.Game.DoUnequipItem(m_Actor, it);
      return new ActionSleep(m_Actor);
    }

    protected ActorAction BehaviorNavigateToSleep()
    {
      if (!m_Actor.IsInside) {
        // XXX this is stymied by closed, opaque doors which logically have inside squares near them; also ex-doorways
        // ignore barricaded doors on residences (they have lots of doors).  Do not respect those in shops, subways, or (vintage) the sewer maintenance.
        // \todo replace by more reasonable foreach loop
        IEnumerable<Location> see_inside = FOV.Where(pt => m_Actor.Location.Map.GetTileAtExt(pt).IsInside).Select(pt2 => new Location(m_Actor.Location.Map,pt2));
        return BehaviorHeadFor(see_inside);
      }

      ActorAction tmpAction = null;
      Dictionary<Point, int> sleep_locs = GetSleepLocsInLOS(out Dictionary<Point,int> couches, out Dictionary<Point,int> doors);
      if (0 >= sleep_locs.Count) {
         // \todo we probably should be using full pathing to the nearest valid location anyway
         return BehaviorWander(loc => loc.Map.IsInsideAtExt(loc.Position)); // XXX explore behavior would be better but that needs fixing
      }

      // \todo trigger secure perimeter if we have appropriate squares whose viewability is not blocked by doors
      if (0<doors.Count) {
        var beyond_door_sleep_locs = new Dictionary<Point,int>();
        var in_bounds = new HashSet<Point>(sleep_locs.Keys);

        foreach(var pos_type in doors) {
          foreach(Point pt in in_bounds.ToList()) {
            // beyond-X
            switch(pos_type.Value)
            {
            case -4:
            case -1:
            case  2:
               if (pos_type.Key.X>pt.X) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
                 continue;
               }
               break;
            case -2:
            case  1:
            case  4:
               if (pos_type.Key.X<pt.X) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
                 continue;
               }
              break;
            }
            // beyond-Y
            switch(pos_type.Value)
            {
            case -4:
            case -3:
            case -2:
               if (pos_type.Key.Y>pt.Y) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
               }
               break;
            case 2:
            case 3:
            case 4:
               if (pos_type.Key.Y<pt.Y) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
               }
               break;
            }
          }
//        if (0 >= in_bounds.Count) break;  // unclear whether micro-optimization or micro-pessimization
        }
        if (0 < beyond_door_sleep_locs.Count) {
          if (0 >= in_bounds.Count) return BehaviorEfficientlyHeadFor(beyond_door_sleep_locs);
          sleep_locs.OnlyIf(pt => in_bounds.Contains(pt));
        }
      }

      tmpAction = BehaviorSecurePerimeter();
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = BehaviorSleep(sleep_locs,couches);
      if (null != tmpAction) {
        if (tmpAction is ActionSleep) m_Actor.Activity = Activity.SLEEPING;
        return tmpAction;
      }
      return null;
    }

    protected ActorAction BehaviorRestIfTired()
    {
      if (m_Actor.StaminaPoints >= Actor.STAMINA_MIN_FOR_ACTIVITY) return null;
      return new ActionWait(m_Actor);
    }

    protected override ActorAction BehaviorExplore(ExplorationData exploration)
    {
      ActorCourage courage = Directives.Courage;
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position.X - PrevLocation.Position.X, m_Actor.Location.Position.Y - PrevLocation.Position.Y);
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location loc = m_Actor.Location + dir;
        if (!IsValidMoveTowardGoalAction(Rules.IsBumpableFor(m_Actor, loc))) return float.NaN;
        if (!loc.Map.IsInBounds(loc.Position)) {
          Location? test = loc.Map.Normalize(loc.Position);
          if (null == test) return float.NaN;
          loc = test.Value;
        }
        if (exploration.HasExplored(loc)) return float.NaN;
        Map map = loc.Map;
        Point position = loc.Position;
        if (m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous && map.TrapsMaxDamageAt(position) >= m_Actor.HitPoints)
          return float.NaN;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position))) num += 1000;
        /* if (!exploration.HasExplored(loc)) */ num += 500;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) num += 100;
        if (null != map.GetActivatedTrapAt(position)) num += -50;
        if (map.IsInsideAtExt(position)) {
          if (map.LocalTime.IsNight) num += 50;
        }
        else if (!map.LocalTime.IsNight) num += 50;
        if (dir == prevDirection) num += 25;
        return (float) (num + RogueForm.Game.Rules.Roll(0, 10));
      }, (a, b) => a > b);
      if (choiceEval != null) return new ActionBump(m_Actor, choiceEval.Choice);
      return null;
    }

    public bool HasAnyInterestingItem(IEnumerable<Item> Items)
    {
#if DEBUG
      if (0 >= (Items?.Count() ?? 0)) throw new ArgumentNullException(nameof(Items));
#endif
      return Items.Any(it => IsInterestingItem(it));
    }

    public bool HasAnyInterestingItem(Inventory inv)
    {
#if DEBUG
      if (inv?.IsEmpty ?? true) throw new ArgumentNullException(nameof(inv));
#endif
      return HasAnyInterestingItem(inv.Items);
    }

    public List<Item> GetTradeableItems()
    {
      Inventory inv = m_Actor.Inventory;
      if (inv == null) return null;
      IEnumerable<Item> ret = inv.Items.Where(it => IsTradeableItem(it));
      return ret.Any() ? ret.ToList() : null;
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
      ItemSprayScent firstStenchKiller = GetFirstStenchKiller(it => !it.IsUseless);
      if (firstStenchKiller != null && m_Actor.CanEquip(firstStenchKiller)) {
        game.DoEquipItem(m_Actor, firstStenchKiller);
        return true;
      }
      return false;
    }
#endregion

    protected ActorAction BehaviorGoReviveCorpse(RogueGame game, List<Percept> percepts)
    {
	  if (!Session.Get.HasCorpses) return null;
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) == 0) return null;
      if (!m_Actor.HasItemOfModel(GameItems.MEDIKIT)) return null;
      List<Percept> corpsePercepts = percepts.FilterT<List<Corpse>>().Filter(p =>
      {
        foreach (Corpse corpse in p.Percepted as List<Corpse>) {
          if (m_Actor.CanRevive(corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return true;
        }
        return false;
      });
      if (null == corpsePercepts) return null;
      Percept percept = FilterNearest(corpsePercepts);
	  if (m_Actor.Location.Position==percept.Location.Position) {
        foreach (Corpse corpse in (percept.Percepted as List<Corpse>)) {
          if (m_Actor.CanRevive(corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return new ActionReviveCorpse(m_Actor, corpse);
        }
	  }
      return BehaviorHeadFor(percept.Location);
    }

#region ground inventory stacks
    private List<Item> InterestingItems(IEnumerable<Item> Items)
    {
#if DEBUG
      if (null == Items) throw new ArgumentNullException(nameof(Items));
#endif
      HashSet<GameItems.IDs> exclude = new HashSet<GameItems.IDs>(Objectives.Where(o=>o is Goal_DoNotPickup).Select(o=>(o as Goal_DoNotPickup).Avoid));
      IEnumerable<Item> tmp = Items.Where(it => !exclude.Contains(it.Model.ID) && IsInterestingItem(it));
      return (tmp.Any() ? tmp.ToList() : null);
    }

    private List<Item> InterestingItems(Inventory inv)
    {
      return InterestingItems(inv.Items);
    }

    private Item MostInterestingItemInStack(Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      List<Item> interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      return obj;
    }

    protected ActorAction BehaviorWouldGrabFromStack(Location loc, Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.StackIsBlocked(loc, out MapObject mapObjectAt)) return null;

      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = !m_Actor.CanGet(obj) && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj) : null);
#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      if (cant_get && null == recover) {
        int obj_code = ItemRatingCode(obj);
        foreach(Item it in m_Actor.Inventory.Items) {
          int it_code = ItemRatingCode(it);
          if (obj_code > it_code) throw new InvalidOperationException("passing up more important item than what is in inventory");
        }
        return null;
      }
#else
      if (cant_get && null == recover) return null;
#endif

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = new ActionTakeItem(m_Actor, loc, obj);
      if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
        if (null == recover) return null;
        if (!recover.IsLegal()) return null;
        return recover;
      }
      return (tmp.IsLegal() ? tmp : null);    // in case this is the biker/trap pickup crash [cairo123]
    }

    protected ActorAction BehaviorGrabFromAccessibleStack(Location loc, Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = !m_Actor.CanGet(obj) && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, loc.Position) : null);
#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      if (cant_get && null == recover) {
        int obj_code = ItemRatingCode(obj);
        foreach(Item it in m_Actor.Inventory.Items) {
          int it_code = ItemRatingCode(it);
          if (obj_code > it_code) throw new InvalidOperationException("passing up more important item than what is in inventory");
        }
        return null;
      }
#else
      if (cant_get && null == recover) return null;
#endif

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = null;
      if (RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));

      tmp = new ActionTakeItem(m_Actor, loc, obj);
      if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
        if (null == recover) return null;
        if (!recover.IsLegal()) return null;
        if (recover is ActionDropItem drop) {
          if (obj.Model.ID == drop.Item.Model.ID) return null;
          Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, drop.Item.Model.ID));
        }
        Objectives.Insert(0,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter+1,m_Actor,tmp));
        return recover;
      }
      return (tmp.IsLegal() ? tmp : null);    // in case this is the biker/trap pickup crash [cairo123]
    }

    protected ActorAction BehaviorGrabFromStack(Location loc, Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.StackIsBlocked(loc, out MapObject mapObjectAt)) return null;

      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = !m_Actor.CanGet(obj) && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, loc.Position) : null);
#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      if (cant_get && null == recover) {
        int obj_code = ItemRatingCode(obj);
        foreach(Item it in m_Actor.Inventory.Items) {
          int it_code = ItemRatingCode(it);
          if (obj_code > it_code) throw new InvalidOperationException("passing up more important item than what is in inventory");
        }
        return null;
      }
#else
      if (cant_get && null == recover) return null;
#endif

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = null;
      if (RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
      bool may_take = (loc == m_Actor.Location);
      // XXX ActionGetFromContainer is obsolete.  Bypass BehaviorIntelligentBumpToward for containers.
      // currently all containers are not-walkable for UI reasons.
      if (mapObjectAt != null && mapObjectAt.IsContainer /* && !m_Actor.Location.Map.IsWalkableFor(position, m_Actor) */
          && 1==Rules.GridDistance(m_Actor.Location,loc))
        may_take = true;

      if (may_take) {
        tmp = new ActionTakeItem(m_Actor, loc, obj);
        if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
          if (null == recover) return null;
          if (!recover.IsLegal()) return null;
          if (recover is ActionDropItem drop) {
            if (obj.Model.ID == drop.Item.Model.ID) return null;
            Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, drop.Item.Model.ID));
          }
          Objectives.Insert(0,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter+1,m_Actor,tmp));
          return recover;
        }
        return (tmp.IsLegal() ? tmp : null);    // in case this is the biker/trap pickup crash [cairo123]
      }
      { // scoping brace
      List<Point> legal_steps = m_Actor.LegalSteps;
      if (null == legal_steps) return null;
      int current_distance = Rules.GridDistance(m_Actor.Location, loc);
      Location? denorm = m_Actor.Location.Map.Denormalize(loc);
      var costs = new Dictionary<Point,int>();
      var vis_costs = new Dictionary<Point,int>();
      if (legal_steps.Contains(denorm.Value.Position)) {
        Point pt = denorm.Value.Position;
        Location test = new Location(m_Actor.Location.Map,pt);
        costs[pt] = 1;
        // this particular heuristic breaks badly if it loses sight of its target
        if (LOS.ComputeFOVFor(m_Actor,test).Contains(denorm.Value.Position)) vis_costs[pt] = 1;
      } else {
        foreach(Point pt in legal_steps) {
          Location test = new Location(m_Actor.Location.Map,pt);
          int dist = Rules.GridDistance(test,loc);
          if (dist >= current_distance) continue;
          costs[pt] = dist;
          // this particular heuristic breaks badly if it loses sight of its target
          if (!LOS.ComputeFOVFor(m_Actor,test).Contains(denorm.Value.Position)) continue;
          vis_costs[pt] = dist;
        }
        // above fails if a direct diagonal path is blocked.
        if (0 >= costs.Count) {
          foreach(Point pt in legal_steps) {
            Location test = new Location(m_Actor.Location.Map,pt);
            int dist = Rules.GridDistance(test,loc);
            if (dist == current_distance) continue;
            costs[pt] = dist;
            // this particular heuristic breaks badly if it loses sight of its target
            if (!LOS.ComputeFOVFor(m_Actor,test).Contains(denorm.Value.Position)) continue;
            vis_costs[pt] = dist;
          }
        }
      }

      ActorAction tmpAction = DecideMove(vis_costs.Keys);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position);
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = DecideMove(costs.Keys);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position);
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      } // end scoping brace
      return null;
    }

    protected ActorAction BehaviorFindTrade(List<Percept> friends)
    {
#if DEBUG
        if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException("must want to trade");
#endif
        var TradeableItems = GetTradeableItems();
        if (0>=(TradeableItems?.Count ?? 0)) return null;
        Map map = m_Actor.Location.Map;

        List<Percept> percepts2 = friends.FilterOut(p => {
          if (p.Turn != map.LocalTime.TurnCounter) return true;
          Actor actor = p.Percepted as Actor;
          if (actor.IsPlayer) return true;
          if (IsActorTabooTrade(actor)) return true;
          if (!m_Actor.CanTradeWith(actor)) return true;
          if (null==m_Actor.MinStepPathTo(m_Actor.Location, p.Location)) return true;    // something wrong, e.g. iron gates in way.  Usual case is police visiting jail.
          if (1 == TradeableItems.Count) {
            List<Item> other_TradeableItems = (actor.Controller as OrderableAI).GetTradeableItems();
            if (null == other_TradeableItems) return true;
            if (1 == other_TradeableItems.Count && TradeableItems[0].Model.ID== other_TradeableItems[0].Model.ID) return true;
          }
          return !(actor.Controller as OrderableAI).HasAnyInterestingItem(TradeableItems);    // other half of m_Actor.GetInterestingTradeableItems(...)
        });
        if (percepts2 != null) {
          Actor actor = FilterNearest(percepts2).Percepted as Actor;
          if (Rules.IsAdjacent(m_Actor.Location, actor.Location)) {
            ActorAction tmpAction = new ActionTrade(m_Actor, actor);
            if (tmpAction.IsLegal()) {
              MarkActorAsRecentTrade(actor);
              RogueGame.DoSay(m_Actor, actor, string.Format("Hey {0}, let's make a deal!", (object) actor.Name), RogueGame.Sayflags.NONE);
              return tmpAction;
            }
          } else {
            ActorAction tmpAction = BehaviorIntelligentBumpToward(actor.Location);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.FOLLOWING;
              m_Actor.TargetActor = actor;
              // need an after-action "hint" to the target on where/who to go to
              if (!m_Actor.WillActAgainBefore(actor)) {
                int t0 = Session.Get.WorldTime.TurnCounter+m_Actor.HowManyTimesOtherActs(1, actor) -(m_Actor.IsBefore(actor) ? 1 : 0);
                (actor.Controller as OrderableAI)?.Objectives.Insert(0,new Goal_HintPathToActor(t0, actor, m_Actor));    // AI disallowed from initiating trades with player so fine
              }
              return tmpAction;
            }
         }
       }
      return null;
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
#if DEBUG
      if (0 >= (tainted?.Count() ?? 0)) throw new ArgumentNullException(nameof(tainted));
      if (tainted.Contains(m_Actor.Location.Position)) throw new InvalidOperationException("tainted.Contains(m_Actor.Location.Position)");
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
      if (null == ret) ret = PlanApproachFailover(navigate);
      if (null == ret) return null; // can happen due to postprocessing
      if (ret is ActionMoveStep test) {
        ReserveSTA(0,1,0,0);    // for now, assume we must reserve one melee attack of stamina (which is at least as much as one push/jump, typically)
        m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
        ReserveSTA(0,0,0,0);
      }
      return ret;
    }

    protected ActorAction BehaviorHastyNavigate(IEnumerable<Point> tainted)
    {
#if DEBUG
      if (0 >= tainted.Count()) throw new InvalidOperationException("0 >= tainted.Count()");
#endif
      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      navigate.GoalDistance(tainted, m_Actor.Location.Position);
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      ActorAction ret = DecideMove(PlanApproach(navigate));
      if (null == ret) ret = PlanApproachFailover(navigate);
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    protected ActorAction BehaviorHeadForExit(Dictionary<Point,Exit> valid_exits)
    {
#if DEBUG
      if (null == valid_exits) throw new ArgumentNullException(nameof(valid_exits));
#endif
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

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.AI_exits.Get;
      // XXX probably should exclude secret maps
      HashSet<Map> possible_destinations = new HashSet<Map>(m_Actor.Location.Map.destination_maps.Get);
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

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.AI_exits.Get;
      // XXX probably should exclude secret maps
      HashSet<Map> possible_destinations = new HashSet<Map>(m_Actor.Location.Map.destination_maps.Get);

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

      // we are not directly connected to an exit to the surface, and have more than one destination map.
      // reject maps that only have us as destination
      possible_destinations.RemoveWhere(m => 1==m.destination_maps.Get.Count);
      if (1 == possible_destinations.Count) {
        valid_exits.OnlyIf(e=>possible_destinations.Contains(e.ToMap));
        return BehaviorHeadForExit(valid_exits);
      }

      HashSet<Map> entry_destinations = new HashSet<Map>(m_Actor.Location.Map.District.EntryMap.destination_maps.Get);
      entry_destinations.IntersectWith(possible_destinations);
      if (0<entry_destinations.Count) {
        valid_exits.OnlyIf(e=>entry_destinations.Contains(e.ToMap));
        return BehaviorHeadForExit(valid_exits);
      }

      // all heuristics failed?
      if (m_Actor.Location.Map==Session.Get.UniqueMaps.Hospital_Patients.TheMap) {
        valid_exits.OnlyIf(e=>e.ToMap== Session.Get.UniqueMaps.Hospital_Offices.TheMap);
        return BehaviorHeadForExit(valid_exits);
      }
#if DEBUG
      throw new InvalidOperationException("need Map::PathTo to handle hospital, police, etc. maps");
#else
      return null;    // XXX should use Map::PathTo but it doesn't have the ordering knowledge required yet
#endif
    }

    private List<Location> Goals(Func<Map, HashSet<Point>> targets_at, Map dest, List<Map> already_seen = null, List<Location> goals = null)
    {
      if (null == goals) goals = new List<Location>();
      HashSet<Point> where_to_go = targets_at(dest);
      if (0 < where_to_go.Count) {
        foreach(Point pt in where_to_go) goals.Add(new Location(dest,pt));
      }

      if (null == already_seen) already_seen = new List<Map>{ dest };
      else already_seen.Add(dest);

      foreach(Map m in dest.destination_maps.Get) {
        if (already_seen.Contains(m)) continue;
        Goals(targets_at,m,already_seen,goals);
      }
      return goals;
    }

    protected FloodfillPathfinder<Location> PathfinderFor(Func<Map, HashSet<Point>> targets_at, Map dest)
    {
#if DEBUG
      if (null == targets_at) throw new ArgumentNullException(nameof(targets_at));
#endif
      var navigate = dest.PathfindLocSteps(m_Actor);
      var goals = Goals(targets_at, dest);

      navigate.GoalDistance(goals, m_Actor.Location);
      return navigate;
    }

    protected FloodfillPathfinder<Point> PathfinderFor(Func<Map, HashSet<Point>> targets_at)
    {
#if DEBUG
      if (null == targets_at) throw new ArgumentNullException(nameof(targets_at));
      if (!(this is CivilianAI)) throw new InvalidOperationException("unhandled OrderableAI subclass");
#endif
      FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      HashSet<Point> where_to_go = targets_at(m_Actor.Location.Map);
      if (0<where_to_go.Count) navigate.GoalDistance(where_to_go, m_Actor.Location.Position);
      // have to debar GangAI/SoldierAI/CHARGuardAI until this is fixed
      if (!m_Actor.Model.Abilities.AI_CanUseAIExits) {
        if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
        return navigate;
      }

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.AI_exits.Get;
      valid_exits.OnlyIf(exit => {  // simulate Exit::ReasonIsBlocked
#if DEBUG
        int blocked = exit.Location.IsBlockedForPathing;
        switch(blocked)
#else
        switch(exit.Location.IsBlockedForPathing)
#endif
        {
        case 2: return false;
        case 1: return m_Actor.CanJump;
#if DEBUG
        case 0: return true;    // not if someone is sleeping on a couch
        default: throw new InvalidOperationException("exit.Location.IsBlockedForPathing out of range: "+blocked.ToString());
#else
        default: return true;
#endif
        }
      });
      Dictionary<Map,HashSet<Point>> hazards = new Dictionary<Map, HashSet<Point>>();
      foreach(Map m in m_Actor.Location.Map.destination_maps.Get) {
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
          navigate.ReviseGoalDistance(tmp.Key, cost+1,m_Actor.Location.Position);   // works even for denormalized coordinates as long as they're adjacent to real coordinates
        }
      }
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      return navigate;
    }

    protected ActorAction BehaviorPathTo(FloodfillPathfinder<Location> navigate)
    {
      if (null == navigate) return null;
      if (!navigate.Domain.Contains(m_Actor.Location)) return null;
      ActorAction ret = DecideMove(navigate.Approach(m_Actor.Location));
      if (null == ret) ret = PlanApproachFailover(navigate);
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    protected ActorAction BehaviorPathTo(FloodfillPathfinder<Point> navigate)
    {
      if (null == navigate) return null;
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      if (m_Actor.Model.Abilities.AI_CanUseAIExits) {
        List<Point> legal_steps = m_Actor.OnePathRange(m_Actor.Location.Map,m_Actor.Location.Position);
        int current_cost = navigate.Cost(m_Actor.Location.Position);
        if (null==legal_steps || !legal_steps.Any(pt => navigate.Cost(pt)<=current_cost)) {
          return BehaviorUseExit(UseExitFlags.ATTACK_BLOCKING_ENEMIES | UseExitFlags.DONT_BACKTRACK);
        }
      }
      ActorAction ret = DecideMove(PlanApproach(navigate));
      if (null == ret) ret = PlanApproachFailover(navigate);
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
    {
#if DEBUG
      return BehaviorPathTo(PathfinderFor(targets_at,m_Actor.Location.Map));
#else
      return BehaviorPathTo(PathfinderFor(targets_at));
#endif
    }

    protected ActorAction BehaviorResupply(HashSet<GameItems.IDs> critical)
    {
      Dictionary<Point, Inventory> stacks = m_Actor.Location.Map.GetAccessibleInventories(m_Actor.Location.Position);
      if (0 < (stacks?.Count ?? 0)) {
        foreach(var x in stacks) {
          Location? loc = (m_Actor.Location.Map.IsInBounds(x.Key) ? new Location(m_Actor.Location.Map,x.Key) : m_Actor.Location.Map.Normalize(x.Key));
          if (null == loc) throw new ArgumentNullException(nameof(loc));
          ActorAction tmpAction = BehaviorGrabFromAccessibleStack(loc.Value, x.Value);
          if (null != tmpAction) return tmpAction;
        }
      }
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
          if (null != threats || null != sights_to_see) {
            int no_light_range = m_Actor.FOVrangeNoFlashlight(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
            HashSet<Point> no_light_FOV = LOS.ComputeFOVFor(m_Actor.Location, no_light_range);
            HashSet<Point> danger_point_FOV = LOS.ComputeFOVFor(m_Actor.Location, no_light_range+3);
            danger_point_FOV.ExceptWith(no_light_FOV);

            int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
            Rectangle view = new Rectangle(m_Actor.Location.Position.X - tmp_LOSrange, m_Actor.Location.Position.Y - tmp_LOSrange, 2*tmp_LOSrange+1,2*tmp_LOSrange+1);

            if (null!=threats) {
              HashSet<Point> tainted = threats.ThreatWhere(m_Actor.Location.Map, view);
              tainted.IntersectWith(danger_point_FOV);
              if (0<tainted.Count) return true;
            }
            if (null!=sights_to_see) {
              HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map, view);
              tainted.IntersectWith(danger_point_FOV);
              if (0<tainted.Count) return true;
            }
            if (null!=threats && null!=sights_to_see) return false;
          }

          // resume legacy implementation
          if (Session.Get.World.Weather != Weather.HEAVY_RAIN) return !m_Actor.IsInside;
          return true;
#if DEBUG
        default: throw new ArgumentOutOfRangeException("unhandled lighting");
#endif
      }
    }

    // taboos
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
      // actors ok to clear at midnight
      if (m_Actor.Location.Map.LocalTime.IsStrikeOfMidnight) m_TabooTrades = null;
    }
  }
}
