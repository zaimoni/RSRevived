// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_IGNORE_MAPS_COVERED_BY_ALLIES
// #define TRACE_NAVIGATE

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
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
      protected bool _isExpired = false;

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
        foreach(Point pt in m_Actor.Controller.FOV) {
          Inventory inv = m_Actor.Location.Map.GetItemsAt(pt);
          if (null == inv) continue;
          if (inv.Has(Avoid)) return false;
        }
        if (m_Actor.Inventory.Has(Avoid)) return false;
        _isExpired = true;  // but expire if the offending item is not in LOS or inventory
        return false;
      }
    }

#if FAIL
    [Serializable]
    public Goal_PickupAt : Objective
    {
      public readonly GameItems.IDs target;
      public readonly Location loc;

      public Goal_PickupAt(int t0, Actor who, GameItems.IDs _target, Location _loc)
      : base(t0,who)
      {
        target = _target;
        loc = _loc;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        
        // if the target location is in LoS, and does not contain the target, expire
        if (m_Actor.Location.Map==loc.Map && m_Actor.Controller.FOV.Contains(loc.Position)) {
          Inventory inv = loc.Map.GetItemsAt(loc.Position);
          if (null == inv) {
           _isExpired = true;  // invalid, expire
           return false;
          }
        }
        ////
        
        return false;
      }
    }
#endif

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

	      Exit exitAt = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
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
  internal abstract class OrderableAI : BaseAI
  {
    private const int EMOTE_GRAB_ITEM_CHANCE = 30;

    // build out CivilianAI first, then fix the other AIs
    readonly protected List<Objective> Objectives = new List<Objective>();

    // taboos really belong here
    private Dictionary<Item, int> m_TabooItems = null;
    private Dictionary<Point, int> m_TabooTiles = null;
    private List<Actor> m_TabooTrades = null;

    // these relate to PC orders for NPCs.  Alpha 9 had no support for AI orders to AI.
    private ActorDirective m_Directive = null;
    private ActorOrder m_Order = null;
    protected Percept m_LastEnemySaw = null;
    protected Percept m_LastItemsSaw = null;
    protected Percept m_LastSoldierSaw = null;
    protected Percept m_LastRaidHeard = null;
    protected bool m_ReachedPatrolPoint = false;
    protected int m_ReportStage = 0;

    public bool DontFollowLeader { get; set; }

    public OrderableAI()
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
          return ExecutePatrol(game, order.Location, percepts);  // cancelled by enamies sighted
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

    private ActorAction ExecutePatrol(RogueGame game, Location location, List<Percept> percepts)
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

      List<Zone> patrolZones = location.Map.GetZonesAt(Order.Location.Position.X, Order.Location.Position.Y);
      return BehaviorWander(loc =>
      {
        List<Zone> zonesAt = loc.Map.GetZonesAt(loc.Position.X, loc.Position.Y);
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
      string reason;
      if (m_Actor.CanSleep(out reason)) {
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
      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      if (null == legal_steps) return null;
      if (2 <= legal_steps.Count) legal_steps = DecideMove_WaryOfTraps(legal_steps);
      if (2 <= legal_steps.Count) {
        int min_dist = goals.Values.Min();
        int near_scale = goals.Count+1;
        Dictionary<Point,int> efficiency = new Dictionary<Point,int>();
        foreach(Point pt in legal_steps) {
          efficiency[pt] = 0;
          foreach(Point pt2 in goals.Keys) {
            // relies on FOV not being "too large"
            int delta = goals[pt2]-Rules.GridDistance(pt, pt2);
            if (min_dist == goals[pt2]) {
              efficiency[pt] += near_scale*delta;
            } else {
              efficiency[pt] += delta;
            }
          }
        }
        int fast_approach = efficiency.Values.Min();
        efficiency.OnlyIf(val=>fast_approach==val);
        legal_steps = new List<Point>(efficiency.Keys);
      }

	  ActorAction tmpAction = DecideMove(legal_steps, null, null);
      if (null != tmpAction) {
		ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
        if (null != tmpAction2) RunIfAdvisable(tmpAction2.dest.Position);
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      return null;
    }

    // cf ActorController::IsTradeableItem
    // this must prevent CivilianAI from
    // 1) bashing barricades, etc. for food when hungry
    // 2) trying to search for z at low ammo when there is ammo available
    public HashSet<GameItems.IDs> WhatDoINeedNow()
    {
      HashSet<GameItems.IDs> ret = new HashSet<GameItems.IDs>();

      if (m_Actor.IsHungry && m_Actor.Model.Abilities.HasToEat) {
        ret.Add(GameItems.IDs.FOOD_ARMY_RATION);
        ret.Add(GameItems.IDs.FOOD_GROCERIES);
        ret.Add(GameItems.IDs.FOOD_CANNED_FOOD);
      }

      if (!m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) {
        List<ItemRangedWeapon> tmp_rw = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>();
        List<ItemAmmo> tmp_ammo = m_Actor.Inventory.GetItemsByType<ItemAmmo>();
        if (null != tmp_rw) {
          foreach(ItemRangedWeapon rw in tmp_rw) {
            // V 0.10.0 : deal with misalignment of enum values
            if (null == m_Actor.GetCompatibleAmmoItem(rw)) {
              switch(rw.AmmoType) {
              case AmmoType.LIGHT_PISTOL:
                ret.Add(GameItems.IDs.AMMO_LIGHT_PISTOL);
                break;
              case AmmoType.HEAVY_PISTOL:
                ret.Add(GameItems.IDs.AMMO_HEAVY_PISTOL);
                break;
              case AmmoType.SHOTGUN:
                ret.Add(GameItems.IDs.AMMO_SHOTGUN);
                break;
              case AmmoType.LIGHT_RIFLE:
                ret.Add(GameItems.IDs.AMMO_LIGHT_RIFLE);
                break;
              case AmmoType.HEAVY_RIFLE:
                ret.Add(GameItems.IDs.AMMO_HEAVY_RIFLE);
                break;
              case AmmoType.BOLT:
                ret.Add(GameItems.IDs.AMMO_BOLTS);
                break;
              }
            }
          }
#if FAIL
        // XXX need to fix AI to be gun bunny capable
        } else if (null != tmp_ammo) {
#endif
        }
      }
      return ret;
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

    private void ETAToKill(Actor en, int dist, ItemRangedWeapon rw, Dictionary<Actor, int> best_weapon_ETAs, Dictionary<Actor, ItemRangedWeapon> best_weapons=null)
    {
      Attack tmp = m_Actor.HypotheticalRangedAttack((rw.Model as ItemRangedWeaponModel).Attack, dist, en);
	  int a_dam = tmp.DamageValue - en.CurrentDefence.Protection_Shot;
      if (0 >= a_dam) return;   // do not update ineffective weapons
      int a_kill_b_in = ((8*en.HitPoints)/(5*a_dam))+2;	// assume bad luck when attacking.
      // Also, assume one fluky miss and compensate for overkill returning 0 rather than 1 attacks.
      if (a_kill_b_in > rw.Ammo) {  // account for reloading weapon
        int turns = a_kill_b_in-rw.Ammo;
        a_kill_b_in++;
        a_kill_b_in += turns/(rw.Model as ItemRangedWeaponModel).MaxAmmo;        
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
        Attack tmp2 = m_Actor.HypotheticalRangedAttack((best_weapons[en].Model as ItemRangedWeaponModel).Attack, dist, en);
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
      ItemRangedWeaponModel rangedWeaponModel = w.Model as ItemRangedWeaponModel;
      return 1000 * rangedWeaponModel.Attack.Range + rangedWeaponModel.Attack.DamageValue;
    }

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
        if (1==Rules.GridDistance(enemies[0].Location.Position,m_Actor.Location.Position)) {
          // something adjacent...check for one-shotting
          ItemMeleeWeapon tmp_melee = m_Actor.GetBestMeleeWeapon(it => !IsItemTaboo(it));
          if (null!=tmp_melee) {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              Attack tmp_attack = m_Actor.HypotheticalMeleeAttack((tmp_melee.Model as ItemMeleeWeaponModel).BaseMeleeAttack(m_Actor.Sheet),en);
              if (en.HitPoints>tmp_attack.DamageValue/2) continue;
              // can one-shot
              if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {    // safe
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  if (!tmp_melee.IsEquipped) game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  if (!tmp_melee.IsEquipped) game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
            }
          } else { // also check for no-weapon one-shotting
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              Attack tmp_attack = m_Actor.UnarmedMeleeAttack(en);
              if (en.HitPoints>tmp_attack.DamageValue/2) continue;
              // can one-shot
              if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {    // safe
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  if (0 < m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS)) {
                    Item tmp_w = m_Actor.GetEquippedWeapon();
                    if (null != tmp_w) game.DoUnequipItem(m_Actor,tmp_w);
                  }
                  return tmpAction;
                }
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(en);
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
      }

      // if no ranged weapons, use BaseAI
      // OrderableAI::GetAvailableRangedWeapons knows about AI disabling of ranged weapons
      if (null == available_ranged_weapons) return BehaviorEquipWeapon(game);

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

      int best_range = available_ranged_weapons.Select(rw => (rw.Model as ItemRangedWeaponModel).Attack.Range).Max();
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
			  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
			  if (null != tmpAction2) RunIfAdvisable(tmpAction2.dest.Position);
              return tmpAction;
            }
		  }
        }
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
            ETAToKill(a,Rules.GridDistance(m_Actor.Location.Position,p.Location.Position),rw,best_weapon_ETAs, best_weapons);
          }
        }
      } else {
        foreach(Percept p in en_in_range) {
          Actor a = p.Percepted as Actor;
          ETAToKill(a,Rules.GridDistance(m_Actor.Location.Position,p.Location.Position), available_ranged_weapons[0], best_weapon_ETAs);
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
           int dist_min = immediate_threat_in_range.Select(a => Rules.GridDistance(m_Actor.Location.Position,a.Location.Position)).Min();
           immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => Rules.GridDistance(m_Actor.Location.Position, a.Location.Position) == dist_min));
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
             int dist_min = en_in_range.Select(p => Rules.GridDistance(m_Actor.Location.Position,p.Location.Position)).Min();
             en_in_range = new List<Percept>(en_in_range.Where(p => Rules.GridDistance(m_Actor.Location.Position, p.Location.Position) == dist_min));
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
        int dist_min = en_in_range.Select(p => Rules.GridDistance(m_Actor.Location.Position,p.Location.Position)).Min();
        en_in_range = new List<Percept>(en_in_range.Where(p => Rules.GridDistance(m_Actor.Location.Position, p.Location.Position) == dist_min));
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
    protected ItemFood GetBestPerishableItem(RogueGame game)
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
        int num4 = Rules.ActorItemNutritionValue(m_Actor,food.NutritionAt(turnCounter));
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

    protected ActorAction BehaviorEatProactively(RogueGame game)
    {
      Item bestEdibleItem = GetBestPerishableItem(game);
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

    protected Item GetEquippedCellPhone()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemTracker && (obj as ItemTracker).CanTrackFollowersOrLeader)
          return obj;
      }
      return null;
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

      Item equippedCellPhone = GetEquippedCellPhone();
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
      ItemGrenadeModel itemGrenadeModel = firstGrenade.Model as ItemGrenadeModel;
      int maxRange = Rules.ActorMaxThrowRange(m_Actor, itemGrenadeModel.MaxThrowDistance);
      Point? nullable = null;
      int num1 = 0;
      foreach (Point point in m_Actor.Controller.FOV) {
        if (Rules.GridDistance(m_Actor.Location.Position, point) > itemGrenadeModel.BlastAttack.Radius && (Rules.GridDistance(m_Actor.Location.Position, point) <= maxRange && LOS.CanTraceThrowLine(m_Actor.Location, point, maxRange, (List<Point>) null))) {
          int num2 = 0;
          for (int x = point.X - itemGrenadeModel.BlastAttack.Radius; x <= point.X + itemGrenadeModel.BlastAttack.Radius; ++x) {
            for (int y = point.Y - itemGrenadeModel.BlastAttack.Radius; y <= point.Y + itemGrenadeModel.BlastAttack.Radius; ++y) {
              if (!m_Actor.Location.Map.IsInBounds(x, y)) continue;
              Actor actorAt = m_Actor.Location.Map.GetActorAt(x, y);
              if (null == actorAt) continue;
              if (actorAt == m_Actor) throw new ArgumentOutOfRangeException("actorAt == m_Actor"); // probably an invariant failure
              int distance = Rules.GridDistance(point, actorAt.Location.Position);
              if (distance > itemGrenadeModel.BlastAttack.Radius) throw new ArgumentOutOfRangeException("distance > itemGrenadeModel.BlastAttack.Radius"); // again, probably an invariant failure
              if (m_Actor.IsEnemyOf(actorAt)) {
                num2 += (game.Rules.BlastDamage(distance, itemGrenadeModel.BlastAttack) * actorAt.MaxHPs);
              } else {
                num2 = -1;
                break;
              }
            }
          }
          if (num2 > num1) {
            nullable = point;
            num1 = num2;
          }
        }
      }
      if (null == nullable || !nullable.HasValue) return null;  // 2nd test probably redundant
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

    protected ActorAction BehaviorFleeFromExplosives(List<Percept> itemStacks)
    {
      List<Percept> goals = itemStacks.FilterT<Inventory>(inv => inv.Has<ItemPrimedExplosive>());
      if (null == goals) return null;
      ActorAction actorAction = BehaviorWalkAwayFrom(goals);
      if (actorAction == null) return null;
      RunIfPossible();
	  m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
      return actorAction;
    }

    protected ActorAction BehaviorSecurePerimeter()
    {
      Map map = m_Actor.Location.Map;
      Dictionary<Point,int> want_to_resolve = new Dictionary<Point,int>();
      foreach (Point position in m_Actor.Controller.FOV) {
        DoorWindow door = map.GetMapObjectAt(position) as DoorWindow;
        if (null == door) continue;
        if (door.IsOpen && m_Actor.CanClose(door)) {
          if (Rules.IsAdjacent(position, m_Actor.Location.Position))
            return new ActionCloseDoor(m_Actor, door);
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

    // sunk from BaseAI
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, Dictionary<Point, int> damage_field, bool hasVisibleLeader, bool isLeaderFighting, ActorCourage courage, string[] emotes)
    {
      Percept target = FilterNearest(enemies);
      bool doRun = false;	// only matters when fleeing
      Actor enemy = target.Percepted as Actor;
      bool decideToFlee;
      if (HasEquipedRangedWeapon(enemy))
        decideToFlee = false;
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
        if (m_Actor.Model.Abilities.CanUseMapObjects) {
          BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
          {
            Point point = m_Actor.Location.Position + dir;
            DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(point) as DoorWindow;
            return door != null && (IsBetween(m_Actor.Location.Position, point, enemy.Location.Position) && m_Actor.CanClose(door)) && (Rules.GridDistance(point, enemy.Location.Position) != 1 || !enemy.CanClose(door));
          }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
          if (choiceEval != null)
            return new ActionCloseDoor(m_Actor, m_Actor.Location.Map.GetMapObjectAt(m_Actor.Location.Position + choiceEval.Choice) as DoorWindow);
        }
        if (m_Actor.Model.Abilities.CanBarricade) {
          BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
          {
            Point point = m_Actor.Location.Position + dir;
            DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(point) as DoorWindow;
            return door != null && (IsBetween(m_Actor.Location.Position, point, enemy.Location.Position) && m_Actor.CanBarricade(door));
          }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
          if (choiceEval != null)
            return new ActionBarricadeDoor(m_Actor, m_Actor.Location.Map.GetMapObjectAt(m_Actor.Location.Position + choiceEval.Choice) as DoorWindow);
        }
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && (Lighting.DARKNESS== m_Actor.Location.Map.Lighting || game.Rules.RollChance(FLEE_THROUGH_EXIT_CHANCE))) {
          tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.NONE);
          if (null != tmpAction) {
            bool flag3 = true;
            if (m_Actor.HasLeader) {
              Exit exitAt = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
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
        tmpAction = BehaviorWalkAwayFrom(enemies);
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
        int dist = Rules.GridDistance(m_Actor.Location.Position,target.Location.Position);
        if (m_Actor.WillActAgainBefore(enemy) && 2==dist) {
          // Neither free hit, nor clearly safe to close.  Main options are charge-hit and wait
          // We could also reposition for tactical advantage i.e. ability to retreat
          return new ActionWait(m_Actor);   // default
        }
      }

      // charge
      tmpAction = BehaviorChargeEnemy(target);
      if (null != tmpAction) {
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[2], (object) enemy.Name));
        m_Actor.Activity = Activity.FIGHTING;
        m_Actor.TargetActor = target.Percepted as Actor;
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
        HashSet<Exit> valid_exits;
        HashSet<Map> exit_maps = a_map.PathTo(dest.Map, out valid_exits);

	    Exit exitAt = a_map.GetExitAt(m_Actor.Location.Position);
        if (exitAt != null && exit_maps.Contains(exitAt.ToMap))
          return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
	    navigate.GoalDistance(a_map.ExitLocations(valid_exits), m_Actor.Location.Position);
	  } else {
	    navigate.GoalDistance(dest.Position, m_Actor.Location.Position);
	  }
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      if (dist >= navigate.Cost(m_Actor.Location.Position)) return null;
	  Dictionary<Point, int> tmp = navigate.Approach(m_Actor.Location.Position);
      // XXX telepathy: do not block an exit which has a non-enemy at the other destination
      ActorAction tmp3 = DecideMove(tmp.Keys, null, null);   // only called when no enemies in sight anyway
      ActionMoveStep tmp2 = tmp3 as ActionMoveStep;
      if (null != tmp2) {
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
            && Rules.GridDistance(m_Actor.Location.Position, other.Location.Position) <= maxDist)
            return new ActionWait(m_Actor);
	  }
	  ActorAction actorAction = BehaviorPathTo(other.Location);
      if (actorAction == null || !actorAction.IsLegal()) return null;
	  ActionMoveStep tmp = actorAction as ActionMoveStep;
	  if (null != tmp) {
        if (  Rules.GridDistance(m_Actor.Location.Position, tmp.dest.Position) > maxDist
           || other.Location.Map != m_Actor.Location.Map)
           RunIfAdvisable(tmp.dest.Position);
	  }
      return actorAction;
    }

    protected ActorAction BehaviorBuildLargeFortification(RogueGame game, int startLineChance)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (game.Rules.CountBarricadingMaterial(m_Actor) < Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, true)) return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || (map.IsOnMapBorder(point.X, point.Y) || map.GetActorAt(point) != null) || (map.GetExitAt(point) != null || map.GetTileAt(point.X, point.Y).IsInside))
          return false;
        int num1 = map.CountAdjacentTo(point, (Predicate<Point>) (ptAdj => !map.GetTileAt(ptAdj).Model.IsWalkable));
        int num2 = map.CountAdjacentTo(point, (Predicate<Point>) (ptAdj =>
        {
          Fortification fortification = map.GetMapObjectAt(ptAdj) as Fortification;
          return fortification != null && !fortification.IsTransparent;
        }));
        return (num1 == 3 && num2 == 0 && game.Rules.RollChance(startLineChance)) || (num1 == 0 && num2 == 1);
      }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!game.Rules.CanActorBuildFortification(m_Actor, point1, true)) return null;
      return new ActionBuildFortification(m_Actor, point1, true);
    }

    protected bool IsDoorwayOrCorridor(Map map, Point pos)
    {
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
      if (game.Rules.CountBarricadingMaterial(m_Actor) < Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, false)) return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || (map.IsOnMapBorder(point.X, point.Y) || map.GetActorAt(point) != null) || map.GetExitAt(point) != null)
          return false;
        return IsDoorwayOrCorridor(map, point);
      }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
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
        MapObject mapObjectAt = map.GetMapObjectAt(pt2);
        if (mapObjectAt != null && mapObjectAt.IsCouch && map.GetActorAt(pt2) == null) {
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
      return Items.Where(it => IsInterestingItem(it)).Any();
    }

    public bool HasAnyInterestingItem(Inventory inv)
    {
      if (inv == null) return false;
      return HasAnyInterestingItem(inv.Items);
    }

    protected Item FirstInterestingItem(Inventory inv)
    {
      if (inv == null) return null;
      foreach (Item it in inv.Items) {
        if (IsInterestingItem(it)) return it;
      }
      return null;
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
      return inv.Items.Where(it=> IsTradeableItem(it)).Any();
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
      if (m_Actor.CanUnequip(it)) RogueForm.Game.DoUnequipItem(m_Actor,it);
      MarkItemAsTaboo(it,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);    // XXX can be called from simulation thread

      List<Point> has_container = new List<Point>();
      foreach(Point pos in Direction.COMPASS.Select(dir => m_Actor.Location.Position+dir)) {
        if (!m_Actor.Location.Map.IsInBounds(pos)) continue;
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

    // stench killer support -- don't want to lock down to the only user, CivilianAI
    // actually, this particular heuristic is *bad* because it causes the z to lose tracking too close to shelter.
    protected bool IsGoodStenchKillerSpot(Map map, Point pos)
    {
      if (map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, pos) > 0) return false;
      if (PrevLocation.Map.GetTileAt(PrevLocation.Position).IsInside != map.GetTileAt(pos).IsInside) return true;
      MapObject mapObjectAt = map.GetMapObjectAt(pos);
      return mapObjectAt != null && mapObjectAt is DoorWindow || map.GetExitAt(pos) != null;
    }

    protected ItemSprayScent GetEquippedStenchKiller()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemSprayScent && ((obj as ItemSprayScent).Model as ItemSprayScentModel).Odor == Odor.PERFUME_LIVING_SUPRESSOR)
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
      if ((itemSprayScent.Model as ItemSprayScentModel).Odor != Odor.PERFUME_LIVING_SUPRESSOR) return null;
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

    protected ActorAction BehaviorMakeRoomFor(RogueGame game, Item it)
    {
      Contract.Requires(null != it);
      Contract.Requires(m_Actor.Inventory.IsFull);
      Contract.Requires(IsInterestingItem(it));

      Inventory inv = m_Actor.Inventory;
      if (it.Model.IsStackable && it.CanStackMore)
         {
         int qty;
         List<Item> tmp = inv.GetItemsStackableWith(it, out qty);
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
          if (it is ItemMeleeWeapon && (weapon.Model as ItemMeleeWeaponModel).Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating) return BehaviorDropItem(weapon);
        }
      }

      // another behavior is responsible for pre-emptively eating perishable food
      // canned food is normally eaten at the last minute
      if (GameItems.IDs.FOOD_CANNED_FOOD == it.Model.ID && m_Actor.Model.Abilities.HasToEat)
        {
        ItemFood food = inv.GetBestDestackable(it) as ItemFood;
        if (null != food) {
          // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
          int need = m_Actor.MaxFood - m_Actor.FoodPoints;
          int num4 = Rules.ActorItemNutritionValue(m_Actor,food.NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter));
          if (num4 <= need) {
            if (m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
          }
        }
      }
      // it should be ok to devour stimulants in a glut
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        ItemMedicine stim2 = inv.GetBestDestackable(it) as ItemMedicine;
        if (null != stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
          if (num4 <= need) {
            if (m_Actor.CanUse(stim2)) return new ActionUseItem(m_Actor, stim2);
          }
        }
      }

      // see if we can eat our way to a free slot
      if (m_Actor.Model.Abilities.HasToEat)
        {
        ItemFood food = inv.GetBestDestackable(game.GameItems[GameItems.IDs.FOOD_CANNED_FOOD]) as ItemFood;
        if (null != food) {
          // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
          int need = m_Actor.MaxFood - m_Actor.FoodPoints;
          int num4 = Rules.ActorItemNutritionValue(m_Actor,food.NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter));
          if (num4*food.Quantity <= need) {
            if (m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
          }
        }
      }

      // finisbing off stimulants to get a free slot is ok
      ItemMedicine stim = inv.GetBestDestackable(game.GameItems[GameItems.IDs.MEDICINE_PILLS_SLP]) as ItemMedicine;
      if (null != stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4*stim.Quantity <= need) {
          if (m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
        }
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
      ItemTracker tmpTracker = inv.GetFirstMatching<ItemTracker>((Predicate<ItemTracker>) (it2 => !wantCellPhone || GameItems.IDs.TRACKER_CELL_PHONE != it2.Model.ID));
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

    protected ActorAction BehaviorWouldGrabFromStack(RogueGame game, Point position, Inventory stack)
    {
      if (stack == null || stack.IsEmpty) return null;

      MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(position);    // XXX this check should affect BehaviorResupply
      if (mapObjectAt != null && !mapObjectAt.IsContainer && !m_Actor.Location.Map.IsWalkableFor(position, m_Actor)) {
        // Cf. Actor::CanOpen
        DoorWindow doorWindow = mapObjectAt as DoorWindow;
        if (doorWindow != null) {
          if (doorWindow.IsBarricaded) return null;
        // Cf. Actor::CanPush; closed door/window is not pushable but can be handled
        } else if (!mapObjectAt.IsMovable) return null; // would have to handle OnFire if that could happen
      }

      List<Item> interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      ActorAction recover = (m_Actor.Inventory.IsFull ? BehaviorMakeRoomFor(game, obj) : null);
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
        DoorWindow doorWindow = mapObjectAt as DoorWindow;
        if (doorWindow != null) {
          if (doorWindow.IsBarricaded) return null;
        // Cf. Actor::CanPush; closed door is not pushable but can be handled
        } else if (!mapObjectAt.IsMovable) return null; // would have to handle OnFire if that could happen
      }

      List<Item> interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      ActorAction recover = (m_Actor.Inventory.IsFull ? BehaviorMakeRoomFor(game, obj) : null);
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
      return false;
    }

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
        foreach(Point pt in navigate.Domain) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, "("+pt.X.ToString()+","+pt.Y.ToString()+"): "+navigate.Cost(pt));
        }
      }
#endif
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;

      Dictionary<Point, int> dest = navigate.Approach(m_Actor.Location.Position);
      Dictionary<Point, int> exposed = new Dictionary<Point,int>();

      foreach(Point pt in dest.Keys) {
#if TRACE_NAVIGATE
        string err = "";
        ActorAction tmp = Rules.IsBumpableFor(m_Actor,new Location(m_Actor.Location.Map,pt), out err);
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": ("+pt.X.ToString()+","+pt.Y.ToString()+") "+(null==tmp ? "null ("+err+")" : tmp.ToString()));
#else
        ActorAction tmp = Rules.IsBumpableFor(m_Actor,new Location(m_Actor.Location.Map,pt));
#endif
        if (null == tmp || !tmp.IsLegal() || !IsLegalPathingAction(tmp)) continue;
        HashSet<Point> los = LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt));
        los.IntersectWith(tainted);
        exposed[pt] = los.Count;
      }
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && 0 >= dest.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no possible moves for navigation");
      if (m_Actor.IsDebuggingTarget && 0 >= exposed.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no acceptable moves for navigation");
#endif
      if (0 >= exposed.Count) return null;

      int most_exposed = exposed.Values.Max();
      if (0<most_exposed) exposed.OnlyIf(val=>most_exposed<=val);
      ActorAction ret = DecideMove(exposed.Keys.ToList(), null, null);
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && null == ret) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": refused to choose move for navigation");
#endif
      ActionMoveStep test = ret as ActionMoveStep;
      if (null != test) RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    protected ActorAction BehaviorHastyNavigate(IEnumerable<Point> tainted)
    {
      Contract.Requires(0<tainted.Count());

      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      navigate.GoalDistance(tainted, m_Actor.Location.Position);
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      Dictionary<Point, int> dest = new Dictionary<Point,int>(navigate.Approach(m_Actor.Location.Position));
      ActorAction ret = DecideMove(dest.Keys.ToList(), null, null);
      ActionMoveStep test = ret as ActionMoveStep;
      if (null != test) RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
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

    protected ActorAction BehaviorHuntDownThreatCurrentMap()
    {
      ThreatTracking threats = m_Actor.Threats;
      if (null == threats) return null;
      // 1) clear the current map, unless it's non-vintage sewers
      HashSet<Point> tainted = ((m_Actor.Location.Map!=m_Actor.Location.Map.District.SewersMap || !Session.Get.HasZombiesInSewers) ? threats.ThreatWhere(m_Actor.Location.Map) : new HashSet<Point>());
      if (0<tainted.Count) return BehaviorNavigate(tainted);
      return null;
    }

    protected ActorAction BehaviorTourismCurrentMap()
    {
      LocationSet sights_to_see = m_Actor.InterestingLocs;
      if (null == sights_to_see) return null;
      // 1) clear the current map.  Sewers is ok for this as it shouldn't normally be interesting
      HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map);
      if (0<tainted.Count) return BehaviorNavigate(tainted);
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
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap));
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

    protected ActorAction BehaviorTourismOtherMaps()
    {
      LocationSet sights_to_see = m_Actor.InterestingLocs;
      if (null == sights_to_see) return null;

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit=>exit.IsAnAIExit);
      // XXX probably should exclude secret maps
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap));

      if (1==possible_destinations.Count && possible_destinations.Contains(m_Actor.Location.Map.District.EntryMap))
        return BehaviorHeadForExit(valid_exits);    // done
        
      // try to pick something reasonable
      Dictionary<Map,HashSet<Point>> hazards = new Dictionary<Map, HashSet<Point>>();
      foreach(Map m in possible_destinations) {
        hazards[m] = sights_to_see.In(m);
      }
      hazards.OnlyIf(val=>0<val.Count);
      if (hazards.ContainsKey(m_Actor.Location.Map.District.EntryMap)) {
        // if the entry map has a problem, go for it
        valid_exits.OnlyIf(e=>e.ToMap==m_Actor.Location.Map.District.EntryMap);
        return BehaviorHeadForExit(valid_exits);
      }

      if (0 >= hazards.Count) {
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

      possible_destinations.IntersectWith(hazards.Keys);
      valid_exits.OnlyIf(e=>possible_destinations.Contains(e.ToMap));

      // Non-entry map destinations with non-follower allies are already handled
      HashSet<Map> unhandled = IgnoreMapsCoveredByAllies(possible_destinations);
      if (0 >= unhandled.Count) {
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
 
      valid_exits.OnlyIf(e=>unhandled.Contains(e.ToMap));
      return BehaviorHeadForExit(valid_exits);
    }

    protected ActorAction BehaviorResupply(HashSet<GameItems.IDs> critical)
    {
      // bootstrap the current map
      HashSet<Point> where_to_go = WhereIs(critical, m_Actor.Location.Map);
      if (!m_Actor.Model.Abilities.AI_CanUseAIExits) {
        if (0 >= where_to_go.Count) return null;
        return BehaviorHastyNavigate(where_to_go);
      }

      FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      if (0<where_to_go.Count) navigate.GoalDistance(where_to_go, m_Actor.Location.Position);

      // currently, there are no cross-district AI exits.
      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit=>exit.IsAnAIExit);
      valid_exits.OnlyIf(exit => {  // simulate Exit::ReasonIsBlocked
        MapObject mapObjectAt = exit.Location.MapObject;
        if (null == mapObjectAt) return true;
        if (mapObjectAt.IsCouch) return true;   // XXX probably not if someone's sleeping on it
        if (!mapObjectAt.IsJumpable) return false;
        return m_Actor.CanJump;
      });
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap));

      foreach(Map m in possible_destinations) {
        HashSet<Point> remote_where_to_go = WhereIs(critical, m);
        if (0 >= remote_where_to_go.Count) continue;
        Dictionary<Point,Exit> exits_for_m = new Dictionary<Point,Exit>(valid_exits);
        exits_for_m.OnlyIf(exit => exit.ToMap == m);
        List<Point> remote_dests = new List<Point>(exits_for_m.Values.Select(exit => exit.Location.Position));
        FloodfillPathfinder<Point> remote_navigate = m.PathfindSteps(m_Actor);
        if (1==remote_dests.Count) {
          remote_navigate.GoalDistance(remote_where_to_go, remote_dests[0]);
        } else {
          remote_navigate.GoalDistance(remote_where_to_go, remote_dests);
        }
        Dictionary<Point,int> remote_costs = new Dictionary<Point,int>();
        foreach(KeyValuePair<Point, Exit> tmp in exits_for_m) {
          int cost = remote_navigate.Cost(tmp.Value.Location.Position);
          if (int.MaxValue == cost) continue;   // not in domain
          remote_costs[tmp.Key] = cost;
        }
        while(0 < remote_costs.Count) {
          int r_cost = remote_costs.Values.Min();
          List<KeyValuePair<Point,int>> pts = remote_costs.Where(tmp => tmp.Value==r_cost).ToList();
          navigate.ReviseGoalDistance(pts[0].Key,r_cost+1,m_Actor.Location.Position);
        }
      }
      if (int.MaxValue==navigate.Cost(m_Actor.Location.Position)) return null;

      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      int current_cost = navigate.Cost(m_Actor.Location.Position);
      if (null==legal_steps || !legal_steps.Any(pt => navigate.Cost(pt)<current_cost)) {
        return BehaviorUseExit(BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }

      Dictionary<Point, int> dest = new Dictionary<Point,int>(navigate.Approach(m_Actor.Location.Position));
      ActorAction ret = DecideMove(dest.Keys.ToList(), null, null);
      ActionMoveStep test = ret as ActionMoveStep;
      if (null != test) RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
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
          // XXX should base lighting on threat tracking/tourism
#if FAIL
          ThreatTracking threats = m_Actor.Threats;
          LocationSet sights_to_see = m_Actor.InterestingLocs;
          int no_light_range = m_Actor.FOVrangeNoFlashlight(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
          HashSet<Point> no_light_FOV = ComputeFOVFor(m_Actor, m_Actor.Location.Map.LocalTime, Weather weather, Location a_loc, no_light_range);
          HashSet<Point> danger_point_FOV = ComputeFOVFor(Actor actor, WorldTime time, Weather weather, Location a_loc, no_light_range+3);
          if (null!=threats) {
          }
          if (null!=sights_to_see) {
          }
#endif
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

    protected void MarkTileAsTaboo(Point p, int expiresTurn)
    {
      if (m_TabooTiles == null) m_TabooTiles = new Dictionary<Point,int>(1);
      else if (m_TabooTiles.ContainsKey(p)) return;
      m_TabooTiles.Add(p, expiresTurn);
    }

    public bool IsTileTaboo(Point p)
    {
      if (m_TabooTiles == null) return false;
      return m_TabooTiles.ContainsKey(p);
    }

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
      if (null != m_TabooTiles) {
        m_TabooTiles.OnlyIf(val => val<=time);
        if (0 == m_TabooTiles.Count) m_TabooTiles = null;
      }
      // actors ok to clear at midnight
      if (m_Actor.Location.Map.LocalTime.IsStrikeOfMidnight)
        m_TabooTrades = null;
    }
  }
}
