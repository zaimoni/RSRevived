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

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal abstract class OrderableAI : BaseAI
  {
    private const int EMOTE_GRAB_ITEM_CHANCE = 30;

    protected Percept m_LastEnemySaw;
    protected Percept m_LastItemsSaw;
    protected Percept m_LastSoldierSaw;
    protected Percept m_LastRaidHeard;
    protected bool m_ReachedPatrolPoint;
    protected int m_ReportStage;

    public bool DontFollowLeader { get; set; }

    public override void SetOrder(ActorOrder newOrder)
    {
      base.SetOrder(newOrder);
      m_ReachedPatrolPoint = false;
      m_ReportStage = 0;
    }

    protected ActorAction ExecuteOrder(RogueGame game, ActorOrder order, List<Percept> percepts)
    {
      if (!m_Actor.HasLeader) return null;
      switch (order.Task)
      {
        case ActorTasks.BARRICADE_ONE:
          return ExecuteBarricading(game, order.Location, false);
        case ActorTasks.BARRICADE_MAX:
          return ExecuteBarricading(game, order.Location, true);
        case ActorTasks.GUARD:
          return ExecuteGuard(game, order.Location, percepts);  // cancelled by enamies sighted
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
          return ExecuteReportPosition(game);
        default:
          throw new NotImplementedException("order task not handled");
      }
    }

    private ActorAction ExecuteBarricading(RogueGame game, Location location, bool toTheMax)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      DoorWindow door = location.Map.GetMapObjectAt(location.Position) as DoorWindow;
      if (door == null) return null;
      if (!game.Rules.CanActorBarricadeDoor(m_Actor, door)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction= new ActionBarricadeDoor(m_Actor, game, door);
        if (!tmpAction.IsLegal()) return null;
        if (!toTheMax) SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(game, location.Position);
      if (null == tmpAction) return null;
      RunIfPossible(game.Rules);
      return tmpAction;
    }

    private ActorAction ExecuteBuildFortification(RogueGame game, Location location, bool isLarge)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      if (!game.Rules.CanActorBuildFortification(m_Actor, location.Position, isLarge)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction = new ActionBuildFortification(m_Actor, game, location.Position, isLarge);
        if (!tmpAction.IsLegal()) return null;
        SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(game, location.Position);
      if (null == tmpAction) return null;
      RunIfPossible(game.Rules);
      return tmpAction;
    }

    private ActorAction ExecuteGuard(RogueGame game, Location location, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(game, percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }

      if (m_Actor.Location.Position != location.Position)
      {
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(game, location.Position);
        if (actorAction3 != null)
        {
                    m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (m_Actor.IsHungry)
      {
        ActorAction actorAction3 = BehaviorEat(game);
        if (actorAction3 != null)
        {
                    m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      ActorAction actorAction4 = BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (actorAction4 != null)
      {
                m_Actor.Activity = Activity.IDLE;
        return actorAction4;
      }
            m_Actor.Activity = Activity.IDLE;
      return (ActorAction) new ActionWait(m_Actor, game);
    }

    private ActorAction ExecutePatrol(RogueGame game, Location location, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(game, percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      if (!m_ReachedPatrolPoint)
        m_ReachedPatrolPoint = m_Actor.Location.Position == location.Position;

      if (!m_ReachedPatrolPoint)
      {
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(game, location.Position);
        if (actorAction3 != null)
        {
                    m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (m_Actor.IsHungry)
      {
        ActorAction actorAction3 = BehaviorEat(game);
        if (actorAction3 != null)
        {
                    m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      ActorAction actorAction4 = BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (actorAction4 != null)
      {
                m_Actor.Activity = Activity.IDLE;
        return actorAction4;
      }
      List<Zone> patrolZones = location.Map.GetZonesAt(Order.Location.Position.X, Order.Location.Position.Y);
      return BehaviorWander(game, (Predicate<Location>) (loc =>
      {
        List<Zone> zonesAt = loc.Map.GetZonesAt(loc.Position.X, loc.Position.Y);
        if (zonesAt == null)
          return false;
        foreach (Zone zone1 in zonesAt)
        {
          foreach (Zone zone2 in patrolZones)
          {
            if (zone1 == zone2)
              return true;
          }
        }
        return false;
      }));
    }

    private ActorAction ExecuteDropAllItems(RogueGame game)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      Item it = m_Actor.Inventory[0];
      if (it.IsEquipped)
        return (ActorAction) new ActionUnequipItem(m_Actor, game, it);
      return (ActorAction) new ActionDropItem(m_Actor, game, it);
    }

    private ActorAction ExecuteReport(RogueGame game, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(game, percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      ActorAction actorAction = null;
      bool flag = false;
      switch (m_ReportStage)
      {
        case 0:
          actorAction = m_LastRaidHeard == null ? (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, "No raids heard.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastRaidHeard);
          ++m_ReportStage;
          break;
        case 1:
          actorAction = m_LastEnemySaw == null ? (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, "No enemies sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
          ++m_ReportStage;
          break;
        case 2:
          actorAction = m_LastItemsSaw == null ? (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, "No items sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
          ++m_ReportStage;
          break;
        case 3:
          actorAction = m_LastSoldierSaw == null ? (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, "No soldiers sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
          ++m_ReportStage;
          break;
        case 4:
          flag = true;
          actorAction = (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, "That's it.", RogueGame.Sayflags.NONE);
          break;
      }
      if (flag) SetOrder(null);
      return actorAction ?? (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, "Let me think...", RogueGame.Sayflags.NONE);
    }

    private ActorAction ExecuteSleepNow(RogueGame game, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(game, percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      string reason;
      if (game.Rules.CanActorSleep(m_Actor, out reason)) {
        if (m_Actor.Location.Map.LocalTime.TurnCounter % 2 == 0)
          return new ActionSleep(m_Actor, game);
        return new ActionWait(m_Actor, game);
      }
      SetOrder(null);
      game.DoEmote(m_Actor, string.Format("I can't sleep now : {0}.", (object) reason));
      return new ActionWait(m_Actor, game);
    }

    private ActorAction ExecuteToggleFollow(RogueGame game)
    {
      SetOrder(null);
      DontFollowLeader = !DontFollowLeader;
      game.DoEmote(m_Actor, DontFollowLeader ? "OK I'll do my stuff, see you soon!" : "I'm ready!");
      return (ActorAction) new ActionWait(m_Actor, game);
    }

    private ActorAction ExecuteReportPosition(RogueGame game)
    {
      SetOrder(null);
      string text = string.Format("I'm in {0} at {1},{2}.", (object)m_Actor.Location.Map.Name, (object)m_Actor.Location.Position.X, (object)m_Actor.Location.Position.Y);
      return (ActorAction) new ActionSay(m_Actor, game, m_Actor.Leader, text, RogueGame.Sayflags.NONE);
    }

    public void OnRaid(RaidType raid, Location location, int turn)
    {
      if (m_Actor.IsSleeping) return;
      string str;
      switch (raid)
      {
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
    private ItemBodyArmor GetBestBodyArmor(Predicate<Item> fn)
    {
      if (m_Actor.Inventory == null) return null;
      int num1 = 0;
      ItemBodyArmor itemBodyArmor1 = null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (null != fn && !fn(obj)) continue;
        ItemBodyArmor itemBodyArmor2 = obj as ItemBodyArmor;
        if (null == itemBodyArmor2) continue;
        int num2 = itemBodyArmor2.Protection_Hit + itemBodyArmor2.Protection_Shot;
        if (num2 > num1) {
          num1 = num2;
          itemBodyArmor1 = itemBodyArmor2;
        }
      }
      return itemBodyArmor1;
    }

    // but all body armors are equipped to the torso slot(?)
    private ItemBodyArmor GetEquippedBodyArmor()
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemBodyArmor) return obj as ItemBodyArmor;
      }
      return null;
    }

    protected ActorAction BehaviorEquipBodyArmor(RogueGame game)
    {
      ItemBodyArmor bestBodyArmor = GetBestBodyArmor((Predicate<Item>) (it => !IsItemTaboo(it)));
      if (bestBodyArmor == null) return null;
      ItemBodyArmor equippedBodyArmor = GetEquippedBodyArmor();
      if (equippedBodyArmor == bestBodyArmor) return null;
      return new ActionEquipItem(m_Actor, game, bestBodyArmor);
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
        int num4 = game.Rules.ActorItemNutritionValue(m_Actor,food.NutritionAt(turnCounter));
        if (num4 > need) continue; // more work needed
        int num3 = need-num4;
        if (num3 > rating) {
          obj1 = food;
          rating = num3;
        }
      }
      return obj1;
    }

    protected ActorAction BehaviorEat(RogueGame game)
    {
      ItemFood bestEdibleItem = GetBestEdibleItem();
      if (null == bestEdibleItem) return null;
      if (!game.Rules.CanActorUseItem(m_Actor, bestEdibleItem)) return null;
      return new ActionUseItem(m_Actor, game, bestEdibleItem);
    }

    protected ActorAction BehaviorEatProactively(RogueGame game)
    {
      Item bestEdibleItem = GetBestPerishableItem(game);
      if (null == bestEdibleItem) return null;
      if (!game.Rules.CanActorUseItem(m_Actor, bestEdibleItem)) return null;
      return new ActionUseItem(m_Actor, game, bestEdibleItem);
    }

    protected ItemLight GetEquippedLight()
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemLight) return obj as ItemLight;
      }
      return null;
    }

    protected ActorAction BehaviorEquipLight(RogueGame game)
    {
      ItemLight tmp = GetEquippedLight();
      if (null != tmp && !tmp.IsUseless) return null;
      tmp = m_Actor.GetFirstMatching<ItemLight>((Predicate<ItemLight>)(it =>
      {
          if (!it.IsUseless) return !IsItemTaboo(it);
          return false;
      }));
      if (tmp != null && game.Rules.CanActorEquipItem(m_Actor, tmp))
        return new ActionEquipItem(m_Actor, game, tmp);
      return null;
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

    protected ActorAction BehaviorEquipCellPhone(RogueGame game)
    {
      bool wantCellPhone = false;
      if (m_Actor.CountFollowers > 0) wantCellPhone = true;
      else if (m_Actor.HasLeader) {
        ItemTracker itemTracker = m_Actor.Leader.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
        wantCellPhone = (null != itemTracker && itemTracker.CanTrackFollowersOrLeader);
      }

      Item equippedCellPhone = GetEquippedCellPhone();
      if (equippedCellPhone != null) {
        if (wantCellPhone) return null;
        return new ActionUnequipItem(m_Actor, game, equippedCellPhone);
      }
      if (!wantCellPhone) return null;
      ItemTracker firstTracker = m_Actor.GetFirstMatching<ItemTracker>((Predicate<ItemTracker>) (it =>
      {
        if (it.CanTrackFollowersOrLeader && 0 < it.Batteries)
          return !IsItemTaboo(it);
        return false;
      }));
      if (firstTracker != null && game.Rules.CanActorEquipItem(m_Actor, firstTracker))
        return new ActionEquipItem(m_Actor, game, firstTracker);
      return null;
    }

    protected ActorAction BehaviorThrowGrenade(RogueGame game, HashSet<Point> fov, List<Percept> enemies)
    {
      if (enemies == null || enemies.Count == 0) return null;
      if (enemies.Count < 3) return null;
      ItemGrenade firstGrenade = m_Actor.GetFirstMatching<ItemGrenade>((Predicate<ItemGrenade>) (it => !IsItemTaboo(it)));
      if (firstGrenade == null) return null;
      ItemGrenadeModel itemGrenadeModel = firstGrenade.Model as ItemGrenadeModel;
      int maxRange = game.Rules.ActorMaxThrowRange(m_Actor, itemGrenadeModel.MaxThrowDistance);
      Point? nullable = new Point?();
      int num1 = 0;
      foreach (Point point in fov) {
        if (Rules.GridDistance(m_Actor.Location.Position, point) > itemGrenadeModel.BlastAttack.Radius && (Rules.GridDistance(m_Actor.Location.Position, point) <= maxRange && LOS.CanTraceThrowLine(m_Actor.Location, point, maxRange, (List<Point>) null))) {
          int num2 = 0;
          for (int x = point.X - itemGrenadeModel.BlastAttack.Radius; x <= point.X + itemGrenadeModel.BlastAttack.Radius; ++x) {
            for (int y = point.Y - itemGrenadeModel.BlastAttack.Radius; y <= point.Y + itemGrenadeModel.BlastAttack.Radius; ++y) {
              if (m_Actor.Location.Map.IsInBounds(x, y)) {
                Actor actorAt = m_Actor.Location.Map.GetActorAt(x, y);
                if (actorAt != null && actorAt != m_Actor) {
                  int distance = Rules.GridDistance(point, actorAt.Location.Position);
                  if (distance <= itemGrenadeModel.BlastAttack.Radius) {
                    if (game.Rules.IsEnemyOf(m_Actor, actorAt)) {
                      int num3 = game.Rules.BlastDamage(distance, itemGrenadeModel.BlastAttack) * actorAt.MaxHPs;
                      num2 += num3;
                    } else {
                      num2 = -1;
                      break;
                    }
                  }
                }
              }
            }
          }
          if (num2 > 0 && (!nullable.HasValue || num2 > num1)) {
            nullable = new Point?(point);
            num1 = num2;
          }
        }
      }
      if (!nullable.HasValue) return null;
      if (!firstGrenade.IsEquipped)
        return new ActionEquipItem(m_Actor, game, firstGrenade);
      ActorAction actorAction = new ActionThrowGrenade(m_Actor, game, nullable.Value);
      if (!actorAction.IsLegal()) return null;
      return actorAction;
    }

    private string MakeCentricLocationDirection(Location from, Location to)
    {
      if (from.Map != to.Map)
        return string.Format("in {0}", (object) to.Map.Name);
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
        return actorAt != null && !actorAt.IsSleeping && !game.Rules.IsEnemyOf(m_Actor, actorAt);
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
        int num = game.Rules.ActorFOV(actorAt1, map.LocalTime, game.Session.World.Weather);
        if (percept.Location.Map == actorAt1.Location.Map && (double) Rules.StdDistance(percept.Location.Position, actorAt1.Location.Position) <= (double) (2 + num))
          return null;
        text = string.Format("I saw {0} {1} {2}.", (object) it.AName, (object) str1, (object) str2);
      } else {
        if (!(percept.Percepted is string))
          throw new InvalidOperationException("unhandled percept.Percepted type");
        text = string.Format("I heard {0} {1} {2}!", (object) (percept.Percepted as string), (object) str1, (object) str2);
      }
      ActionSay actionSay = new ActionSay(m_Actor, game, actorAt1, text, RogueGame.Sayflags.NONE);
      if (actionSay.IsLegal()) return actionSay;
      return null;
    }

    protected ActorAction BehaviorFleeFromExplosives(RogueGame game, List<Percept> itemStacks)
    {
      if (itemStacks == null || itemStacks.Count == 0) return null;
      List<Percept> goals = Filter(game, itemStacks, (Predicate<Percept>) (p =>
      {
        Inventory inventory = p.Percepted as Inventory;
        if (inventory == null || inventory.IsEmpty) return false;
        foreach (Item obj in inventory.Items) {
          if (obj is ItemPrimedExplosive) return true;
        }
        return false;
      }));
      if (goals == null || goals.Count == 0) return null;
      ActorAction actorAction = BehaviorWalkAwayFrom(game, goals);
      if (actorAction == null) return null;
      RunIfPossible(game.Rules);
      return actorAction;
    }

    protected ActorAction BehaviorSecurePerimeter(RogueGame game, HashSet<Point> fov)
    {
      Map map = m_Actor.Location.Map;
      foreach (Point position in fov) {
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null) {
          DoorWindow door = mapObjectAt as DoorWindow;
          if (door != null) {
            if (door.IsOpen && game.Rules.IsClosableFor(m_Actor, door)) {
              if (Rules.IsAdjacent(door.Location.Position, m_Actor.Location.Position))
                return new ActionCloseDoor(m_Actor, game, door);
              return BehaviorIntelligentBumpToward(game, door.Location.Position);
            }
            if (door.IsWindow && !door.IsBarricaded && game.Rules.CanActorBarricadeDoor(m_Actor, door)) {
              if (Rules.IsAdjacent(door.Location.Position, m_Actor.Location.Position))
                return new ActionBarricadeDoor(m_Actor, game, door);
              return BehaviorIntelligentBumpToward(game, door.Location.Position);
            }
          }
        }
      }
      return null;
    }

    protected ActorAction BehaviorWarnFriends(RogueGame game, List<Percept> friends, Actor nearestEnemy)
    {
      if (Rules.IsAdjacent(m_Actor.Location, nearestEnemy.Location)) return null;
      if (m_Actor.HasLeader && m_Actor.Leader.IsSleeping) return new ActionShout(m_Actor, game);
      foreach (Percept friend in friends) {
        Actor actor = friend.Percepted as Actor;
        if (actor == null) throw new ArgumentException("percept not an actor");
        if (actor != m_Actor && (actor.IsSleeping && !game.Rules.IsEnemyOf(m_Actor, actor)) && game.Rules.IsEnemyOf(actor, nearestEnemy)) {
          string text = nearestEnemy == null ? string.Format("Wake up {0}!", (object) actor.Name) : string.Format("Wake up {0}! {1} sighted!", (object) actor.Name, (object) nearestEnemy.Name);
          return new ActionShout(m_Actor, game, text);
        }
      }
      return null;
    }

    protected ActorAction BehaviorDontLeaveFollowersBehind(RogueGame game, int distance, out Actor target)
    {
      target = null;
      int num1 = int.MinValue;
      Map map = m_Actor.Location.Map;
      Point position = m_Actor.Location.Position;
      int num2 = 0;
      int num3 = m_Actor.CountFollowers / 2;
      foreach (Actor follower in m_Actor.Followers) {
        if (follower.Location.Map == map) {
          if (Rules.GridDistance(follower.Location.Position, position) <= distance && ++num2 >= num3) return null;
          int num4 = Rules.GridDistance(follower.Location.Position, position);
          if (target == null || num4 > num1) {
            target = follower;
            num1 = num4;
          }
        }
      }
      if (target == null) return null;
      return BehaviorIntelligentBumpToward(game, target.Location.Position);
    }

    protected ActorAction BehaviorLeadActor(RogueGame game, Percept target)
    {
      Actor target1 = target.Percepted as Actor;
      if (!game.Rules.CanActorTakeLead(m_Actor, target1)) return null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, target1.Location.Position))
        return new ActionTakeLead(m_Actor, game, target1);
      return BehaviorIntelligentBumpToward(game, target1.Location.Position);
    }

    protected ActorAction BehaviorBuildLargeFortification(RogueGame game, int startLineChance)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (game.Rules.CountBarricadingMaterial(m_Actor) < game.Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, true))
        return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || (map.IsOnMapBorder(point.X, point.Y) || map.GetActorAt(point) != null) || (map.GetExitAt(point) != null || map.GetTileAt(point.X, point.Y).IsInside))
          return false;
        int num1 = map.CountAdjacentInMap(point, (Predicate<Point>) (ptAdj => !map.GetTileAt(ptAdj).Model.IsWalkable));
        int num2 = map.CountAdjacentInMap(point, (Predicate<Point>) (ptAdj =>
        {
          Fortification fortification = map.GetMapObjectAt(ptAdj) as Fortification;
          if (fortification != null)
            return !fortification.IsTransparent;
          return false;
        }));
        return num1 == 3 && num2 == 0 && game.Rules.RollChance(startLineChance) || num1 == 0 && num2 == 1;
      }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!game.Rules.CanActorBuildFortification(m_Actor, point1, true)) return null;
      return new ActionBuildFortification(m_Actor, game, point1, true);
    }

    protected bool IsDoorwayOrCorridor(RogueGame game, Map map, Point pos)
    {
      if (!map.GetTileAt(pos).Model.IsWalkable)
        return false;
      Point p1 = pos + Direction.N;
      bool flag1 = map.IsInBounds(p1) && !map.GetTileAt(p1).Model.IsWalkable;
      Point p2 = pos + Direction.S;
      bool flag2 = map.IsInBounds(p2) && !map.GetTileAt(p2).Model.IsWalkable;
      Point p3 = pos + Direction.E;
      bool flag3 = map.IsInBounds(p3) && !map.GetTileAt(p3).Model.IsWalkable;
      Point p4 = pos + Direction.W;
      bool flag4 = map.IsInBounds(p4) && !map.GetTileAt(p4).Model.IsWalkable;
      Point p5 = pos + Direction.NE;
      bool flag5 = map.IsInBounds(p5) && !map.GetTileAt(p5).Model.IsWalkable;
      Point p6 = pos + Direction.NW;
      bool flag6 = map.IsInBounds(p6) && !map.GetTileAt(p6).Model.IsWalkable;
      Point p7 = pos + Direction.SE;
      bool flag7 = map.IsInBounds(p7) && !map.GetTileAt(p7).Model.IsWalkable;
      Point p8 = pos + Direction.SW;
      bool flag8 = map.IsInBounds(p8) && !map.GetTileAt(p8).Model.IsWalkable;
      bool flag9 = !flag5 && !flag7 && !flag6 && !flag8;
      return flag9 && flag1 && (flag2 && !flag3) && !flag4 || flag9 && flag3 && (flag4 && !flag1) && !flag2;
    }

    protected ActorAction BehaviorBuildSmallFortification(RogueGame game)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (game.Rules.CountBarricadingMaterial(m_Actor) < game.Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, false))
        return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || (map.IsOnMapBorder(point.X, point.Y) || map.GetActorAt(point) != null) || map.GetExitAt(point) != null)
          return false;
        return IsDoorwayOrCorridor(game, map, point);
      }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!game.Rules.CanActorBuildFortification(m_Actor, point1, false)) return null;
      return new ActionBuildFortification(m_Actor, game, point1, false);
    }

    protected ActorAction BehaviorSleep(RogueGame game, HashSet<Point> FOV)
    {
      if (!game.Rules.CanActorSleep(m_Actor)) return null;
      Map map = m_Actor.Location.Map;
      if (map.HasAnyAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt => map.GetMapObjectAt(pt) is DoorWindow)))
      {
        ActorAction actorAction = BehaviorWander(game, (Predicate<Location>) (loc =>
        {
          if (!(map.GetMapObjectAt(loc.Position) is DoorWindow))
            return !map.HasAnyAdjacentInMap(loc.Position, (Predicate<Point>) (pt => loc.Map.GetMapObjectAt(pt) is DoorWindow));
          return false;
        }));
        if (actorAction != null) return actorAction;
      }
      if (m_Actor.IsOnCouch) return new ActionSleep(m_Actor, game);
      Point? nullable = new Point?();
      float num1 = float.MaxValue;
      foreach (Point point in FOV) {
        MapObject mapObjectAt = map.GetMapObjectAt(point);
        if (mapObjectAt != null && mapObjectAt.IsCouch && map.GetActorAt(point) == null) {
          float num2 = Rules.StdDistance(m_Actor.Location.Position, point);
          if ((double) num2 < (double) num1) {
            num1 = num2;
            nullable = new Point?(point);
          }
        }
      }
      if (nullable.HasValue) {
        ActorAction actorAction = BehaviorIntelligentBumpToward(game, nullable.Value);
        if (actorAction != null) return actorAction;
      }

      Item it = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);  // all battery powered items are left hand, currently
      if (game.Rules.IsItemBatteryPowered(it)) game.DoUnequipItem(m_Actor, it);
      return new ActionSleep(m_Actor, game);
    }

    protected ActorAction BehaviorRestIfTired(RogueGame game)
    {
      if (m_Actor.StaminaPoints >= Actor.STAMINA_MIN_FOR_ACTIVITY) return null;
      return new ActionWait(m_Actor, game);
    }

    protected ActorAction BehaviorDropUselessItem(RogueGame game)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item it in m_Actor.Inventory.Items) {
        if (it.IsUseless) return BehaviorDropItem(game, it);
      }
      return null;
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
      Item obj = null;
      foreach (Item it in stack.Items) {
        if (game.Rules.CanActorGetItem(m_Actor, it) && IsInterestingItem(it)) {
          obj = it;
          break;
        }
      }
      if (obj == null) return null;
      Item it1 = obj;
      if (game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) it1.AName));
      if (position == m_Actor.Location.Position)
        return new ActionTakeItem(m_Actor, game, position, it1);
      return BehaviorIntelligentBumpToward(game, position);
    }

    protected bool NeedsLight(RogueGame game)
    {
      switch (m_Actor.Location.Map.Lighting)
      {
        case Lighting.DARKNESS:
          return true;
        case Lighting.OUTSIDE:
          if (!m_Actor.Location.Map.LocalTime.IsNight)
            return false;
          if (game.Session.World.Weather != Weather.HEAVY_RAIN)
            return !m_Actor.IsInside;
          return true;
        case Lighting.LIT:
          return false;
        default:
          throw new ArgumentOutOfRangeException("unhandled lighting");
      }
    }

  }
}
