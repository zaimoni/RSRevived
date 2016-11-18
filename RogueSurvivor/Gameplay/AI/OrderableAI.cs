// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

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
          if (inv.HasItemMatching(it=>Avoid==it.Model.ID)) return false;
        }
        if (m_Actor.Inventory.HasItemMatching(it => Avoid == it.Model.ID)) return false;
        _isExpired = true;  // but expire if the offending item is not in LOS or inventory
        return false;
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
    protected List<Objective> Objectives = new List<Objective>();

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
      if (game.Rules.CanActorSleep(m_Actor, out reason)) {
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
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
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

    // This is only called when the actor is hungry.  It doesn't need to do food value corrections
    protected ItemFood GetBestEdibleItem()
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
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
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
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
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
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
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
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
      return it != null && !(it is ItemBarricadeMaterial) && (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty || !m_Actor.Inventory.Contains(it));
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
      foreach (Point position in m_Actor.Controller.FOV) {
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null) {
          DoorWindow door = mapObjectAt as DoorWindow;
          if (door != null) {
            if (door.IsOpen && m_Actor.CanClose(door)) {
              if (Rules.IsAdjacent(door.Location.Position, m_Actor.Location.Position))
                return new ActionCloseDoor(m_Actor, door);
              return BehaviorIntelligentBumpToward(door.Location.Position);
            }
            if (door.IsWindow && !door.IsBarricaded && m_Actor.CanBarricade(door)) {
              if (Rules.IsAdjacent(door.Location.Position, m_Actor.Location.Position))
                return new ActionBarricadeDoor(m_Actor, door);
              return BehaviorIntelligentBumpToward(door.Location.Position);
            }
          }
        }
      }
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
          return BehaviorUseExit(RogueForm.Game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
	    navigate.GoalDistance(a_map.ExitLocations(valid_exits),int.MaxValue,m_Actor.Location.Position);
	  } else {
	    navigate.GoalDistance(dest.Position,int.MaxValue,m_Actor.Location.Position);
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
	    int num = Rules.GridDistance(m_Actor.Location.Position, other.Location.Position);
        if (FOV.Contains(other.Location.Position) && num <= maxDist) return new ActionWait(m_Actor);
	  }
	  ActorAction actorAction = BehaviorPathTo(other.Location);
      if (actorAction == null || !actorAction.IsLegal()) return null;
	  ActionMoveStep tmp = actorAction as ActionMoveStep;
	  if (null != tmp) {
        if (other.IsRunning || other.Location.Map != m_Actor.Location.Map) RunIfAdvisable(tmp.dest.Position);
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
        int num1 = map.CountAdjacentInMap(point, (Predicate<Point>) (ptAdj => !map.GetTileAt(ptAdj).Model.IsWalkable));
        int num2 = map.CountAdjacentInMap(point, (Predicate<Point>) (ptAdj =>
        {
          Fortification fortification = map.GetMapObjectAt(ptAdj) as Fortification;
          return fortification != null && !fortification.IsTransparent;
        }));
        return num1 == 3 && num2 == 0 && game.Rules.RollChance(startLineChance) || num1 == 0 && num2 == 1;
      }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!game.Rules.CanActorBuildFortification(m_Actor, point1, true)) return null;
      return new ActionBuildFortification(m_Actor, point1, true);
    }

    protected bool IsDoorwayOrCorridor(Map map, Point pos)
    {
      if (!map.GetTileAt(pos).Model.IsWalkable) return false;
      Point p5 = pos + Direction.NE;
      bool flag_ne = map.IsInBounds(p5) && !map.GetTileAt(p5).Model.IsWalkable;
      Point p6 = pos + Direction.NW;
      bool flag_nw = map.IsInBounds(p6) && !map.GetTileAt(p6).Model.IsWalkable;
      Point p7 = pos + Direction.SE;
      bool flag_se = map.IsInBounds(p7) && !map.GetTileAt(p7).Model.IsWalkable;
      Point p8 = pos + Direction.SW;
      bool flag_sw = map.IsInBounds(p8) && !map.GetTileAt(p8).Model.IsWalkable;
      bool no_corner = !flag_ne && !flag_se && !flag_nw && !flag_sw;
      if (!no_corner) return false;

      Point p1 = pos + Direction.N;
      bool flag_n = map.IsInBounds(p1) && !map.GetTileAt(p1).Model.IsWalkable;
      Point p2 = pos + Direction.S;
      bool flag_s = map.IsInBounds(p2) && !map.GetTileAt(p2).Model.IsWalkable;
      Point p3 = pos + Direction.E;
      bool flag_e = map.IsInBounds(p3) && !map.GetTileAt(p3).Model.IsWalkable;
      Point p4 = pos + Direction.W;
      bool flag_w = map.IsInBounds(p4) && !map.GetTileAt(p4).Model.IsWalkable;
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
      if (!game.Rules.CanActorSleep(m_Actor)) return null;
      Map map = m_Actor.Location.Map;
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
      Point? nullable = null;
      float num1 = float.MaxValue;
      foreach (Point point in m_Actor.Controller.FOV) {
        MapObject mapObjectAt = map.GetMapObjectAt(point);
        if (mapObjectAt != null && mapObjectAt.IsCouch && map.GetActorAt(point) == null) {
          float num2 = Rules.StdDistance(m_Actor.Location.Position, point);
          if ((double) num2 < (double) num1) {
            num1 = num2;
            nullable = point;
          }
        }
      }
      if (nullable.HasValue) {
        ActorAction actorAction = BehaviorIntelligentBumpToward(nullable.Value);
        if (actorAction != null) return actorAction;
      }

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

    protected ActionDropItem BehaviorDropItem(Item it)
    {
      if (it == null) return null;
      if (m_Actor.CanUnequip(it)) RogueForm.Game.DoUnequipItem(m_Actor,it);
      MarkItemAsTaboo(it,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);
      return (m_Actor.CanDrop(it) ? new ActionDropItem(m_Actor, it) : null);
    }

    protected ActionDropItem BehaviorDropUselessItem()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item it in m_Actor.Inventory.Items) {
        if (it.IsUseless) return BehaviorDropItem(it);
      }
      ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
      if (null != armor) return BehaviorDropItem(armor); 
      return null;
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
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemSprayScent && ((obj as ItemSprayScent).Model as ItemSprayScentModel).Odor == Odor.PERFUME_LIVING_SUPRESSOR)
          return obj as ItemSprayScent;
      }
      return null;
    }

    protected ItemSprayScent GetFirstStenchKiller(Predicate<ItemSprayScent> fn)
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
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

      // not-best melee weapon can be dropped
      if (2<=m_Actor.CountItemQuantityOfType(typeof (ItemMeleeWeapon))) {
        ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
        // ok to drop if either the weapon won't become interesting, or is less interesting that the other item
        if (null != weapon && (m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon)) > 2 || (it is ItemMeleeWeapon && RHSMoreInteresting(weapon, it)))) return BehaviorDropItem(weapon);  
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

      // ditch unimportant items
      ItemBarricadeMaterial tmpBarricade = inv.GetFirstMatching<ItemBarricadeMaterial>(null);
      if (null != tmpBarricade) return BehaviorDropItem(tmpBarricade);
      ItemTrap tmpTrap = inv.GetFirstMatching<ItemTrap>(null);
      if (null != tmpTrap) return BehaviorDropItem(tmpTrap);
      ItemEntertainment tmpEntertainment = inv.GetFirstMatching<ItemEntertainment>(null);
      if (null != tmpEntertainment) return BehaviorDropItem(tmpEntertainment);
      ItemMedicine tmpMedicine = inv.GetFirstMatching<ItemMedicine>(null);
      if (null != tmpMedicine) return BehaviorDropItem(tmpMedicine);
      ItemLight tmpLight = inv.GetFirstMatching<ItemLight>(null);
      if (null != tmpLight) return BehaviorDropItem(tmpLight);

      // uninteresting ammo
      ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>((Predicate<ItemAmmo>) (ammo => !IsInterestingItem(ammo)));
      if (null != tmpAmmo) {
        ItemRangedWeapon tmpRw = GetCompatibleRangedWeapon(tmpAmmo);
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
      // XXX dropping body armor to get a better one should be ok
      if (it is ItemBodyArmor) return null;
      ItemBodyArmor tmpBodyArmor = inv.GetFirstMatching<ItemBodyArmor>(null);
      if (null != tmpBodyArmor) return BehaviorDropItem(tmpBodyArmor);

      // give up
      return null;
    }

    protected ActorAction BehaviorWouldGrabFromStack(RogueGame game, Point position, Inventory stack)
    {
      if (stack == null || stack.IsEmpty) return null;
      MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(position);
      if (mapObjectAt != null) {
        Fortification fortification = mapObjectAt as Fortification;
        if (fortification != null && !fortification.IsWalkable) return null;
        DoorWindow doorWindow = mapObjectAt as DoorWindow;
        if (doorWindow != null && doorWindow.IsBarricaded) return null;
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
      MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(position);
      if (mapObjectAt != null) {
        Fortification fortification = mapObjectAt as Fortification;
        if (fortification != null && !fortification.IsWalkable) return null;
        DoorWindow doorWindow = mapObjectAt as DoorWindow;
        if (doorWindow != null && doorWindow.IsBarricaded) return null;
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
      if (position == m_Actor.Location.Position) {
        tmp = new ActionTakeItem(m_Actor, position, obj);
        if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
          if (null == recover) return null;
          if (!recover.IsLegal()) return null;
          if (recover is ActionDropItem) {
            Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, (recover as ActionDropItem).Item.Model.ID));
            if (obj.Model.ID == (recover as ActionDropItem).Item.Model.ID) return null;
          }
          Objectives.Add(new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter+1,m_Actor,tmp));
          return recover;
        }
        return (tmp.IsLegal() ? tmp : null);    // in case this is the biker/trap pickup crash [cairo123]
      }
      // BehaviorIntelligentBumpToward will return null if a get item from container is invalid, so need to prevent that
      // best range depends on other factors
      if (m_Actor.Inventory.IsFull) { 
        if (null != recover && recover.IsLegal()) {
          if (recover is ActionDropItem) {
            Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, (recover as ActionDropItem).Item.Model.ID));
            if (obj.Model.ID == (recover as ActionDropItem).Item.Model.ID) return null;
          }
          return recover;
        }
      }
      tmp = BehaviorIntelligentBumpToward(position);
      ActionGetFromContainer tmp2 = (tmp as ActionGetFromContainer);
      if (null != tmp2 && tmp2.Item != obj) {
        // translate the desired action
        tmp = new ActionTakeItem(m_Actor, position, obj);
      }
      return tmp;
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

    protected ActorAction BehaviorHuntDownThreat()
    {
      ThreatTracking threats = m_Actor.Threats;
      if (null == threats) return null;
      // 1) clear the current map, unless it's non-vintage sewers
      HashSet<Point> tainted = ((m_Actor.Location.Map!=m_Actor.Location.Map.District.SewersMap || !Session.Get.HasZombiesInSewers) ? threats.ThreatWhere(m_Actor.Location.Map) : new HashSet<Point>());
      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      if (0<tainted.Count) {
        navigate.GoalDistance(tainted,int.MaxValue,m_Actor.Location.Position);
        if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
        Dictionary<Point, int> dest = new Dictionary<Point,int>(navigate.Approach(m_Actor.Location.Position));
        Dictionary<Point, int> exposed = new Dictionary<Point,int>();
        foreach(Point pt in dest.Keys) {
          HashSet<Point> los = LOS.ComputeFOVFor(m_Actor, m_Actor.Location.Map.LocalTime, Session.Get.World.Weather, new Location(m_Actor.Location.Map,pt));
          los.IntersectWith(tainted);
          exposed[pt] = los.Count;
        }
        int most_exposed = exposed.Values.Max();
        if (0<most_exposed) exposed.OnlyIf(val=>most_exposed<=val);
        ActorAction ret = DecideMove(exposed.Keys.ToList(), null, null);
        ActionMoveStep test = ret as ActionMoveStep;
        if (null != test) RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
        return ret;
      }

      if (!m_Actor.Model.Abilities.AI_CanUseAIExits) return null;
      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit=>exit.IsAnAIExit);
      HashSet<Map> possible_destinations = new HashSet<Map>(valid_exits.Values.Select(exit=>exit.ToMap));
      // but ignore the sewers if we're not vintage
      if (Session.Get.HasZombiesInSewers) possible_destinations.Remove(m_Actor.Location.Map.District.SewersMap);
      if (0>=possible_destinations.Count) return null;
        
      // try to pick something reasonable
      Dictionary<Map,HashSet<Point>> hazards = new Dictionary<Map, HashSet<Point>>();
      if (1<possible_destinations.Count) {
        foreach(Map m in possible_destinations) {
          hazards[m] = threats.ThreatWhere(m);
        }
        hazards.OnlyIf(val=>0<val.Count);
        if (0<hazards.Count) possible_destinations.IntersectWith(hazards.Keys);
      }
      // if the entry map is a destination, go there if it has a problem
      if (1<possible_destinations.Count && possible_destinations.Contains(m_Actor.Location.Map.District.EntryMap) && hazards.ContainsKey(m_Actor.Location.Map.District.EntryMap)) {
        possible_destinations = new HashSet<Map>();
        possible_destinations.Add(m_Actor.Location.Map.District.EntryMap);
      }
      valid_exits.OnlyIf(e=>possible_destinations.Contains(e.ToMap));
      if (valid_exits.ContainsKey(m_Actor.Location.Position)) {
        return BehaviorUseExit(RogueForm.Game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }

	  navigate.GoalDistance(valid_exits.Keys, int.MaxValue,m_Actor.Location.Position);
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
	  Dictionary<Point, int> tmp = navigate.Approach(m_Actor.Location.Position);	// only called when no enemies in sight anyway
      {
      ActorAction ret = DecideMove(tmp.Keys.ToList(), null, null);
      ActionMoveStep test = ret as ActionMoveStep;
      if (null != test) RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
      }
#if FAIL
        // general priorities
        // 2) clear the entry map
        if (m_Actor.Location.Map!=m_Actor.Location.Map.District.EntryMap) {
        // 3) clear basements and subway; ok to clear police station and first level of hospital.
        } else {
        }
#endif
    }

    protected bool NeedsLight()
    {
      switch (m_Actor.Location.Map.Lighting)
      {
        case Lighting.DARKNESS:
          return true;
        case Lighting.OUTSIDE:
          if (!m_Actor.Location.Map.LocalTime.IsNight) return false;
          if (Session.Get.World.Weather != Weather.HEAVY_RAIN) return !m_Actor.IsInside;
          return true;
        case Lighting.LIT:
          return false;
        default:
          throw new ArgumentOutOfRangeException("unhandled lighting");
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
