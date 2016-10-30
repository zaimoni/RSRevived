// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.BaseAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define DATAFLOW_TRACE

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal abstract class BaseAI : ActorController
    {
    private const int FLEE_THROUGH_EXIT_CHANCE = 50;
    private const int EMOTE_FLEE_CHANCE = 30;
    private const int EMOTE_FLEE_TRAPPED_CHANCE = 50;
    private const int EMOTE_CHARGE_CHANCE = 30;
    private const float MOVE_DISTANCE_PENALTY = 0.42f;
    private const float LEADER_LOF_PENALTY = 1f;
    private ActorDirective m_Directive; // Should be in orderableAI but needed for movement AI here, and also FeralDogAI
    private Location m_prevLocation;
    private Dictionary<Item, int> m_TabooItems;
    private Dictionary<Point, int> m_TabooTiles;
    private List<Actor> m_TabooTrades;

    public BaseAI()
    {
      m_Directive = null;
      m_TabooItems = null;
      m_TabooTiles = null;
      m_TabooTrades = null;
    }

    // BaseAI does have to know about directives for the movement behaviors
    public ActorDirective Directives {
      get {
        if (m_Directive == null)
          m_Directive = new ActorDirective();
        return m_Directive;
      }
    }

    protected Location PrevLocation
    {
      get
      {
        return m_prevLocation;
      }
    }

    protected List<Actor> TabooTrades
    {
      get
      {
        return m_TabooTrades;
      }
    }

    public override ActorAction GetAction(RogueGame game)
    {
      Contract.Ensures(null != Contract.Result<ActorAction>());
      Contract.Ensures(Contract.Result<ActorAction>().IsLegal());
      List<Percept> percepts = UpdateSensors(game);
      if (m_prevLocation.Map == null) m_prevLocation = m_Actor.Location;
      m_Actor.TargetActor = null;
      ActorAction actorAction = SelectAction(game, percepts);
      m_prevLocation = m_Actor.Location;
      if (actorAction != null) return actorAction;
      return new ActionWait(m_Actor);
    }

    protected abstract ActorAction SelectAction(RogueGame game, List<Percept> percepts);

/*
    NOTE: List<Percept>, as a list data structure, takes O(n) time/RAM to reset its capacity down 
    to its real size.  Since C# is a garbage collected language, this would actually worsen
    the RAM loading until the next explicit GC.Collect() call (typically within a fraction of a second).
    The only realistic mitigation is to pro-rate the capacity request.
 */
    // April 22, 2016: testing indicates this does not need micro-optimization
    protected List<Percept> FilterSameMap(List<Percept> percepts)
    {
      Map map = m_Actor.Location.Map;
      return Filter(percepts,(Predicate<Percept>) (p => p.Location.Map == map));
    }

    protected List<Percept> FilterEnemies(List<Percept> percepts)
    {
      return FilterActors(percepts,(Predicate<Actor>) (target => target!=m_Actor && m_Actor.IsEnemyOf(target)));
    }

    protected List<Percept> FilterNonEnemies(List<Percept> percepts)
    {
      return FilterActors(percepts,(Predicate<Actor>) (target => target!=m_Actor && !m_Actor.IsEnemyOf(target)));
    }

    protected List<Percept> FilterCurrent(List<Percept> percepts)
    {
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      return Filter(percepts,(Predicate<Percept>) (p => p.Turn == turnCounter));
    }

    // GangAI's mugging target selection triggered a race condition 
    // that allowed a non-null non-empty percepts
    // to be seen as returning null from FilterNearest anyway, from
    // the outside (Contracts saw a non-null return)
    protected Percept FilterNearest(List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      double num1 = double.MaxValue;
      Percept percept1 = null;
      foreach(Percept percept2 in percepts) {
         float num2 = Rules.StdDistance(m_Actor.Location.Position, percept2.Location.Position);
         if (num2 < num1) {
           percept1 = percept2;
           num1 = num2;
         }
      }
      return percept1;
    }

    protected Percept FilterStrongestScent(List<Percept> scents)
    {
      if (scents == null || scents.Count == 0)
        return (Percept) null;
      Percept percept = (Percept) null;
      SmellSensor.AIScent aiScent1 = (SmellSensor.AIScent) null;
      foreach (Percept scent in scents)
      {
        SmellSensor.AIScent aiScent2 = scent.Percepted as SmellSensor.AIScent;
        if (aiScent2 == null)
          throw new InvalidOperationException("percept not an aiScent");
        if (percept == null || aiScent2.Strength > aiScent1.Strength)
        {
          aiScent1 = aiScent2;
          percept = scent;
        }
      }
      return percept;
    }

    // dead function?
    protected List<Percept> FilterActorsModel(List<Percept> percepts, ActorModel model)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      foreach (Percept percept in percepts) {
        Actor actor = percept.Percepted as Actor;
        if (null != actor && actor.Model == model) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    // XXX dead function?
    protected List<Percept> FilterActors(List<Percept> percepts)
    {
      return Filter(percepts, (Predicate<Percept>) (p => p.Percepted is Actor));
    }

    protected List<Percept> FilterActors(List<Percept> percepts, Predicate<Actor> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      IEnumerable<Percept> tmp = percepts.Where((Func<Percept,bool>) (p =>
      {
        Actor a = p.Percepted as Actor;
        return null != a && predicateFn(a);
      }));
      return tmp.Any() ? tmp.ToList() : null;
    }

    protected List<Percept> FilterFireTargets(RogueGame game, List<Percept> percepts)
    {
      return FilterActors(percepts, (Predicate<Actor>) (target => game.Rules.CanActorFireAt(m_Actor, target)));
    }

    protected List<Percept> FilterStacks(List<Percept> percepts)
    {
      return Filter(percepts, (Predicate<Percept>) (p => p.Percepted is Inventory));
    }

    protected List<Percept> FilterStacks(List<Percept> percepts, Predicate<Inventory> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      IEnumerable<Percept> tmp = percepts.Where((Func<Percept,bool>) (p =>
      {
        Inventory i = p.Percepted as Inventory;
        return null != i && predicateFn(i);
      }));
      return tmp.Any() ? tmp.ToList() : null;
    }

    protected List<Percept> FilterCorpses(List<Percept> percepts)
    {
      return Filter(percepts, (Predicate<Percept>) (p => p.Percepted is List<Corpse>));
    }

    // XXX dead function?
    protected List<Percept> FilterCorpses(List<Percept> percepts, Predicate<List<Corpse>> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      IEnumerable<Percept> tmp = percepts.Where((Func<Percept,bool>) (p =>
      {
        List<Corpse> i = p.Percepted as List<Corpse>;
        return null != i && predicateFn(i);
      }));
      return tmp.Any() ? tmp.ToList() : null;
    }

    protected List<Percept> Filter(List<Percept> percepts, Predicate<Percept> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      IEnumerable<Percept> tmp = percepts.Where(p=> predicateFn(p));
      return tmp.Any() ? tmp.ToList() : null;
    }

    protected Percept FilterFirst(List<Percept> percepts, Predicate<Percept> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      foreach (Percept percept in percepts) {
        if (predicateFn(percept)) return percept;
      }
      return null;
    }

    protected List<Percept> FilterOut(List<Percept> percepts, Predicate<Percept> rejectPredicateFn)
    {
      return Filter(percepts, (Predicate<Percept>) (p => !rejectPredicateFn(p)));
    }

    protected List<Percept> SortByDistance(List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      Point from = m_Actor.Location.Position;
      List<Percept> perceptList = new List<Percept>((IEnumerable<Percept>) percepts);
      perceptList.Sort((Comparison<Percept>) ((pA, pB) =>
      {
        float num1 = Rules.StdDistance(pA.Location.Position, from);
        float num2 = Rules.StdDistance(pB.Location.Position, from);
        if (num1 > num2) return 1;
        return num1 < num2 ? -1 : 0;
      }));
      return perceptList;
    }

    // firearms use grid i.e. L-infinity distance
    protected List<Percept> SortByGridDistance(List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      if (1==percepts.Count) return percepts;

      List<Percept> perceptList = new List<Percept>(percepts);
      Point from = m_Actor.Location.Position;
      Dictionary<Percept,int> dict = new Dictionary<Percept,int>(perceptList.Count);
      foreach(Percept p in perceptList) {
        dict.Add(p,Rules.GridDistance(p.Location.Position, from));
      }
      perceptList.Sort((Comparison<Percept>) ((pA, pB) =>
      {
        int num1 = dict[pA];
        int num2 = dict[pB];
        if (num1 > num2) return 1;
        return num1 < num2 ? -1 : 0;
      }));
      return perceptList;
    }

    protected List<Percept> SortByDate(RogueGame game, List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = new List<Percept>(percepts);
      perceptList.Sort((Comparison<Percept>) ((pA, pB) =>
      {
        if (pA.Turn < pB.Turn) return 1;
        return pA.Turn <= pB.Turn ? 0 : -1;
      }));
      return perceptList;
    }

    // policy change for behaviors: unless the action from a behavior is being used to decide whether to commit to the behavior,
    // a behavior should handle all free actions itself and return only non-free actions.

    protected ActorAction BehaviorWander(RogueGame game, Predicate<Location> goodWanderLocFn)
    {
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        if (goodWanderLocFn != null && !goodWanderLocFn(location)) return false;
        return isValidWanderAction(game.Rules.IsBumpableFor(m_Actor, location));
      }), (Func<Direction, float>) (dir =>
      {
        int num = game.Rules.Roll(0, 666);
        if (m_Actor.Model.Abilities.IsIntelligent && null != m_Actor.Location.Map.GetActivatedTrapAt((m_Actor.Location + dir).Position))
          num -= 1000;
        return (float) num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      return (choiceEval != null ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    protected ActorAction BehaviorWander(RogueGame game)
    {
      return BehaviorWander(game, (Predicate<Location>) null);
    }

    protected ActorAction BehaviorBumpToward(RogueGame game, Point goal, Func<Point, Point, float> distanceFn)
    {
      BaseAI.ChoiceEval<ActorAction> choiceEval = ChooseExtended(game, Direction.COMPASS_LIST, (Func<Direction, ActorAction>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        ActorAction a = game.Rules.IsBumpableFor(m_Actor, location);
        if (a == null) {
          if (m_Actor.Model.Abilities.IsUndead && game.Rules.HasActorPushAbility(m_Actor)) {
            MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(location.Position);
            if (mapObjectAt != null && game.Rules.CanActorPush(m_Actor, mapObjectAt)) {
              Direction pushDir = game.Rules.RollDirection();
              if (game.Rules.CanPushObjectTo(mapObjectAt, mapObjectAt.Location.Position + pushDir))
                return new ActionPush(m_Actor, mapObjectAt, pushDir);
            }
          }
          return null;
        }
        if (location.Position == goal || IsValidMoveTowardGoalAction(a)) return a;
        return null;
      }), (Func<Direction, float>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        if (distanceFn != null) return distanceFn(location.Position, goal);
        return Rules.StdDistance(location.Position, goal);
      }), (Func<float, float, bool>) ((a, b) =>
      {
        if (!float.IsNaN(a)) return (double) a < (double) b;
        return false;
      }));
      if (choiceEval != null) return choiceEval.Choice;
      return null;
    }

    protected ActorAction BehaviorStupidBumpToward(RogueGame game, Point goal)
    {
      return BehaviorBumpToward(game, goal, (Func<Point, Point, float>) ((ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = Rules.StdDistance(ptA, ptB);
        if (!m_Actor.Location.Map.IsWalkableFor(ptA, m_Actor)) num += 0.42f;
        return num;
      }));
    }

    protected ActorAction BehaviorIntelligentBumpToward(RogueGame game, Point goal)
    {
      float currentDistance = Rules.StdDistance(m_Actor.Location.Position, goal);
      bool imStarvingOrCourageous = m_Actor.IsStarving || Directives.Courage == ActorCourage.COURAGEOUS;
      return BehaviorBumpToward(game, goal, (Func<Point, Point, float>) ((ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = Rules.StdDistance(ptA, ptB);
        if ((double) num >= (double) currentDistance) return float.NaN;
        if (!imStarvingOrCourageous) {
          int trapsMaxDamage = ComputeTrapsMaxDamage(m_Actor.Location.Map, ptA);
          if (trapsMaxDamage > 0) {
            if (trapsMaxDamage >= m_Actor.HitPoints) return float.NaN;
            num += 0.42f;
          }
        }
        return num;
      }));
    }

    // A number of the melee enemy targeting sequences not only work on grid distance,
    // they need to return a coordinated action/target pair.
    protected ActorAction TargetGridMelee(RogueGame game, List<Percept> perceptList, out Actor target)
    {
      target = null;
      ActorAction ret = null;
      int num1 = int.MaxValue;
      foreach (Percept percept in perceptList) {
        int num2 = Rules.GridDistance(m_Actor.Location.Position, percept.Location.Position);
        if (num2 < num1) {
          ActorAction tmp = BehaviorStupidBumpToward(game, percept.Location.Position);
          if (null != tmp) {
            num1 = num2;
            target = percept.Percepted as Actor;
            ret = tmp;
          }
        }
      }
      return ret;
    }

    protected ActorAction BehaviorWalkAwayFrom(RogueGame game, Percept goal)
    {
      return BehaviorWalkAwayFrom(game, new List<Percept>(1) { goal });
    }

    protected ActorAction BehaviorWalkAwayFrom(RogueGame game, List<Percept> goals)
    {
      Actor leader = m_Actor.Leader;
      bool flag = m_Actor.HasLeader && m_Actor.GetEquippedWeapon() is ItemRangedWeapon;
      Actor actor = (flag ? GetNearestTargetFor(m_Actor.Leader) : null);
      bool checkLeaderLoF = actor != null && actor.Location.Map == m_Actor.Location.Map;
      List<Point> leaderLoF = null;
      if (checkLeaderLoF) {
        leaderLoF = new List<Point>(1);
        ItemRangedWeapon itemRangedWeapon = m_Actor.GetEquippedWeapon() as ItemRangedWeapon;
        LOS.CanTraceFireLine(leader.Location, actor.Location.Position, (itemRangedWeapon.Model as ItemRangedWeaponModel).Attack.Range, leaderLoF);
      }
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir => IsValidFleeingAction(game.Rules.IsBumpableFor(m_Actor, m_Actor.Location + dir))), (Func<Direction, float>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        float num = SafetyFrom(location.Position, goals);
        if (m_Actor.HasLeader) {
          num -= Rules.StdDistance(location.Position, m_Actor.Leader.Location.Position);
          if (checkLeaderLoF && leaderLoF.Contains(location.Position)) --num;
        }
        return num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    protected ActionMeleeAttack BehaviorMeleeAttack(Actor target)
    {
      Contract.Requires(null != target);
      ActionMeleeAttack tmp = new ActionMeleeAttack(m_Actor, target);
      return (tmp.IsLegal() ? tmp : null);
    }

    protected ActionRangedAttack BehaviorRangedAttack(Actor target)
    {
      Contract.Requires(null != target);
      ActionRangedAttack tmp = new ActionRangedAttack(m_Actor, target);
      return (tmp.IsLegal() ? tmp : null);
    }

    /// <returns>null, or a non-free action</returns>
    protected ActorAction BehaviorEquipWeapon(RogueGame game)
    {
      Item equippedWeapon = GetEquippedWeapon();
      if (equippedWeapon != null && equippedWeapon is ItemRangedWeapon && !Directives.CanFireWeapons) {
        game.DoUnequipItem(m_Actor, equippedWeapon);
        equippedWeapon = null;
      }
      if (equippedWeapon != null && equippedWeapon is ItemRangedWeapon)
      {
        ItemRangedWeapon rw = equippedWeapon as ItemRangedWeapon;
        if (rw.Ammo > 0) return null;
        ItemAmmo compatibleAmmoItem = GetCompatibleAmmoItem(rw);
        if (compatibleAmmoItem != null)
          return new ActionUseItem(m_Actor, compatibleAmmoItem);
        game.DoUnequipItem(m_Actor, equippedWeapon);
        equippedWeapon = null;
      }
      if (Directives.CanFireWeapons) {
        Item rangedWeaponWithAmmo = GetBestRangedWeaponWithAmmo(it => !IsItemTaboo(it));
        if (rangedWeaponWithAmmo != null && game.Rules.CanActorEquipItem(m_Actor, rangedWeaponWithAmmo)) {
          game.DoEquipItem(m_Actor, rangedWeaponWithAmmo);
          return null;
        }
      }

      // ranged weapon non-option for some reason
      ItemMeleeWeapon bestMeleeWeapon = GetBestMeleeWeapon(it => !IsItemTaboo(it));
      if (bestMeleeWeapon == null) return null;
      if (equippedWeapon == bestMeleeWeapon) return null;
      game.DoEquipItem(m_Actor, bestMeleeWeapon);
      return null;
    }

    protected ActionDropItem BehaviorDropItem(Item it)
    {
      if (it == null) return null;
      if (Rules.CanActorUnequipItem(m_Actor, it)) RogueForm.Game.DoUnequipItem(m_Actor,it);
      MarkItemAsTaboo(it,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);
      ActionDropItem tmp = new ActionDropItem(m_Actor, it);
      return (tmp.IsLegal() ? tmp : null);
    }

    protected int ComputeTrapsMaxDamage(Map map, Point pos)
    {
      Inventory itemsAt = map.GetItemsAt(pos);
      if (itemsAt == null) return 0;
      int num = 0;
      foreach (Item obj in itemsAt.Items) {
        ItemTrap itemTrap = obj as ItemTrap;
        if (itemTrap != null) num += itemTrap.TrapModel.Damage;
      }
      return num;
    }

    protected ActorAction BehaviorBuildTrap(RogueGame game)
    {
      ItemTrap itemTrap = m_Actor.Inventory.GetFirst<ItemTrap>();
      if (itemTrap == null) return null;
      string reason;
      if (!IsGoodTrapSpot(game, m_Actor.Location.Map, m_Actor.Location.Position, out reason)) return null;
      if (!itemTrap.IsActivated && !itemTrap.TrapModel.ActivatesWhenDropped)
        return new ActionUseItem(m_Actor, itemTrap);
      game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) reason, (object) itemTrap.AName));
      return new ActionDropItem(m_Actor, itemTrap);
    }

    protected bool IsGoodTrapSpot(RogueGame game, Map map, Point pos, out string reason)
    {
      reason = "";
      bool flag = false;
      bool isInside = map.GetTileAt(pos).IsInside;
      if (!isInside && map.GetCorpsesAt(pos) != null) {
        reason = "that corpse will serve as a bait for";
        flag = true;
      } else if (m_prevLocation.Map.GetTileAt(m_prevLocation.Position).IsInside != isInside) {
        reason = "protecting the building with";
        flag = true;
      } else {
        MapObject mapObjectAt = map.GetMapObjectAt(pos);
        if (mapObjectAt != null && mapObjectAt is DoorWindow) {
          reason = "protecting the doorway with";
          flag = true;
        } else if (map.GetExitAt(pos) != null) {
          reason = "protecting the exit with";
          flag = true;
        }
      }
      if (!flag) return false;
      Inventory itemsAt = map.GetItemsAt(pos);
      return itemsAt == null || itemsAt.CountItemsMatching((Predicate<Item>) (it =>
      {
        ItemTrap itemTrap = it as ItemTrap;
        if (itemTrap == null) return false;
        return itemTrap.IsActivated;
      })) <= 3;
    }

    protected ActorAction BehaviorAttackBarricade(RogueGame game)
    {
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        DoorWindow doorWindow = map.GetMapObjectAt(pt) as DoorWindow;
        if (doorWindow != null)
          return doorWindow.IsBarricaded;
        return false;
      }));
      if (pointList == null) return null;
      DoorWindow doorWindow1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]) as DoorWindow;
      ActionBreak actionBreak = new ActionBreak(m_Actor, doorWindow1);
      return (actionBreak.IsLegal() ? actionBreak : null);
    }

    // intentionally disabled in alpha 9; for ZM AI
    protected ActorAction BehaviorAssaultBreakables(RogueGame game, HashSet<Point> fov)
    {
      Map map = m_Actor.Location.Map;
      List<Percept> percepts = null;
      foreach (Point position in fov) {
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && mapObjectAt.IsBreakable) {
          if (percepts == null) percepts = new List<Percept>();
          percepts.Add(new Percept((object) mapObjectAt, map.LocalTime.TurnCounter, new Location(map, position)));
        }
      }
      if (percepts == null) return null;
      Percept percept = FilterNearest(percepts);
      if (!Rules.IsAdjacent(m_Actor.Location.Position, percept.Location.Position))
        return BehaviorIntelligentBumpToward(game, percept.Location.Position);
      ActionBreak actionBreak = new ActionBreak(m_Actor, percept.Percepted as MapObject);
      return (actionBreak.IsLegal() ? actionBreak : null);
    }

    protected ActionPush BehaviorPushNonWalkableObject(RogueGame game)
    {
      if (!game.Rules.HasActorPushAbility(m_Actor)) return null;
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || mapObjectAt.IsWalkable)
          return false;
        return game.Rules.CanActorPush(m_Actor, mapObjectAt);
      }));
      if (pointList == null) return null;
      MapObject mapObjectAt1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      ActionPush tmp = new ActionPush(m_Actor, mapObjectAt1, game.Rules.RollDirection());
      return (tmp.IsLegal() ? tmp : null);
    }

    protected ActionPush BehaviorPushNonWalkableObjectForFood(RogueGame game)
    {
      if (!game.Rules.HasActorPushAbility(m_Actor)) return null;
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        // Wrecked cars are very tiring to push, and are jumpable so they don't need to be pushed.
        if (mapObjectAt == null || mapObjectAt.IsWalkable || mapObjectAt.IsJumpable)
          return false;
        return game.Rules.CanActorPush(m_Actor, mapObjectAt);
      }));
      if (pointList == null) return null;
      MapObject mapObjectAt1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      ActionPush tmp = new ActionPush(m_Actor, mapObjectAt1, game.Rules.RollDirection());
      return (tmp.IsLegal() ? tmp : null);
    }

    protected ActorAction BehaviorUseMedecine(RogueGame game, int factorHealing, int factorStamina, int factorSleep, int factorCure, int factorSan)
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory == null || inventory.IsEmpty) return null;
      bool needHP = m_Actor.HitPoints < m_Actor.MaxHPs;
      bool needSTA = m_Actor.IsTired;
      bool needSLP = m_Actor.WouldLikeToSleep;
      bool needCure = m_Actor.Infection > 0;
      bool needSan = m_Actor.Model.Abilities.HasSanity && m_Actor.Sanity < 3*m_Actor.MaxSanity/4;
      if (!needHP && !needSTA && (!needSLP && !needCure) && !needSan) return null;
      List<ItemMedicine> itemsByType = inventory.GetItemsByType<ItemMedicine>();
      if (itemsByType == null) return null;
      BaseAI.ChoiceEval<ItemMedicine> choiceEval = Choose(game, itemsByType, (Func<ItemMedicine, bool>) (it => true), (Func<ItemMedicine, float>) (it =>
      {
        int num = 0;
        if (needHP) num += factorHealing * it.Healing;
        if (needSTA) num += factorStamina * it.StaminaBoost;
        if (needSLP) num += factorSleep * it.SleepBoost;
        if (needCure) num += factorCure * it.InfectionCure;
        if (needSan) num += factorSan * it.SanityCure;
        return (float) num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval == null || (double) choiceEval.Value <= 0.0) return null;
      return new ActionUseItem(m_Actor, choiceEval.Choice);
    }

    protected ActorAction BehaviorUseEntertainment()
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory.IsEmpty) return null;
      ItemEntertainment itemEntertainment = inventory.GetFirst<ItemEntertainment>();
      if (itemEntertainment == null) return null;
      ActionUseItem tmp = new ActionUseItem(m_Actor, itemEntertainment);
      return (tmp.IsLegal() ? tmp  : null);
    }

    protected ActorAction BehaviorDropBoringEntertainment(RogueGame game)
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory.IsEmpty) return null;
      foreach (Item it in inventory.Items) {
        if (it is ItemEntertainment && m_Actor.IsBoredOf(it))
          return new ActionDropItem(m_Actor, it);
      }
      return null;
    }

    protected ActorAction BehaviorFollowActor(RogueGame game, Actor other, Point otherPosition, bool isVisible, int maxDist)
    {
      if (other == null || other.IsDead) return null;
      int num = Rules.GridDistance(m_Actor.Location.Position, otherPosition);
      if (isVisible && num <= maxDist) return new ActionWait(m_Actor);
      if (other.Location.Map != m_Actor.Location.Map) {
        Exit exitAt = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
        if (exitAt != null && exitAt.ToMap == other.Location.Map && game.Rules.CanActorUseExit(m_Actor, m_Actor.Location.Position))
          return new ActionUseExit(m_Actor, m_Actor.Location.Position);
      }
      ActorAction actorAction = BehaviorIntelligentBumpToward(game, otherPosition);
      if (actorAction == null || !actorAction.IsLegal()) return null;
      if (other.IsRunning) RunIfPossible();
      return actorAction;
    }

    protected ActorAction BehaviorHangAroundActor(RogueGame game, Actor other, Point otherPosition, int minDist, int maxDist)
    {
      if (other == null || other.IsDead) return null;
      int num = 0;
      Point p;
      do {
        p = otherPosition;
        p.X += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        p.Y += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
                m_Actor.Location.Map.TrimToBounds(ref p);
      }
      while (Rules.GridDistance(p, otherPosition) < minDist && ++num < 100);
      ActorAction a = BehaviorIntelligentBumpToward(game, p);
      if (a == null || !IsValidMoveTowardGoalAction(a) || !a.IsLegal())
        return null;
      if (other.IsRunning) RunIfPossible();
      return a;
    }

    protected ActorAction BehaviorTrackScent(RogueGame game, List<Percept> scents)
    {
      if (scents == null || scents.Count == 0) return null;
      Percept percept = FilterStrongestScent(scents);
      Map map = m_Actor.Location.Map;
      if (!(m_Actor.Location.Position == percept.Location.Position))
        return BehaviorIntelligentBumpToward(game, percept.Location.Position);
      if (map.GetExitAt(m_Actor.Location.Position) != null && m_Actor.Model.Abilities.AI_CanUseAIExits)
        return BehaviorUseExit(game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      return null;
    }

    protected ActorAction BehaviorChargeEnemy(RogueGame game, Percept target)
    {
      Actor actor = target.Percepted as Actor;
      ActorAction tmpAction = BehaviorMeleeAttack(actor);
      if (null != tmpAction) return tmpAction;
      if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location))
        return BehaviorUseMedecine(game, 0, 1, 0, 0, 0) ?? new ActionWait(m_Actor);
      tmpAction = BehaviorIntelligentBumpToward(game, target.Location.Position);
      if (null == tmpAction) return null;
      if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) RunIfPossible();
      return tmpAction;
    }

    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, bool hasVisibleLeader, bool isLeaderFighting, ActorCourage courage, string[] emotes)
    {
      Percept target = FilterNearest(enemies);
      bool doRun = false;
      Actor enemy = target.Percepted as Actor;
      bool decideToFlee;
      if (HasEquipedRangedWeapon(enemy))
        decideToFlee = false;
      else if (m_Actor.Model.Abilities.IsLawEnforcer && enemy.MurdersCounter > 0)
        decideToFlee = false;
      else if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, enemy.Location))
        decideToFlee = true;
      else if (m_Actor.Leader != null)
      {
        switch (courage)
        {
          case ActorCourage.COWARD:
            decideToFlee = true;
            doRun = true;
            break;
          case ActorCourage.CAUTIOUS:
            decideToFlee = WantToEvadeMelee(game, m_Actor, courage, enemy);
            doRun = !HasSpeedAdvantage(m_Actor, enemy);
            break;
          case ActorCourage.COURAGEOUS:
            if (isLeaderFighting)
            {
              decideToFlee = false;
              break;
            }
            decideToFlee = WantToEvadeMelee(game, m_Actor, courage, enemy);
            doRun = !HasSpeedAdvantage(m_Actor, enemy);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled courage");
        }
      } else {
        switch (courage) {
          case ActorCourage.COWARD:
            decideToFlee = true;
            doRun = true;
            break;
          case ActorCourage.CAUTIOUS:
          case ActorCourage.COURAGEOUS:
            decideToFlee = WantToEvadeMelee(game, m_Actor, courage, enemy);
            doRun = !HasSpeedAdvantage(m_Actor, enemy);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled courage");
        }
      }
      if (decideToFlee)
      {
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[0], (object) enemy.Name));
        if (m_Actor.Model.Abilities.CanUseMapObjects) {
          BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
          {
            Point point = m_Actor.Location.Position + dir;
            DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(point) as DoorWindow;
            return door != null && (IsBetween(m_Actor.Location.Position, point, enemy.Location.Position) && game.Rules.IsClosableFor(m_Actor, door)) && (Rules.GridDistance(point, enemy.Location.Position) != 1 || !game.Rules.IsClosableFor(enemy, door));
          }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
          if (choiceEval != null)
            return new ActionCloseDoor(m_Actor, m_Actor.Location.Map.GetMapObjectAt(m_Actor.Location.Position + choiceEval.Choice) as DoorWindow);
        }
        if (m_Actor.Model.Abilities.CanBarricade) {
          BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
          {
            Point point = m_Actor.Location.Position + dir;
            DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(point) as DoorWindow;
            return door != null && (IsBetween(m_Actor.Location.Position, point, enemy.Location.Position) && game.Rules.CanActorBarricadeDoor(m_Actor, door));
          }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
          if (choiceEval != null)
            return new ActionBarricadeDoor(m_Actor, m_Actor.Location.Map.GetMapObjectAt(m_Actor.Location.Position + choiceEval.Choice) as DoorWindow);
        }
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && game.Rules.RollChance(FLEE_THROUGH_EXIT_CHANCE)) {
          ActorAction actorAction = BehaviorUseExit(game, BaseAI.UseExitFlags.NONE);
          if (actorAction != null) {
            bool flag3 = true;
            if (m_Actor.HasLeader) {
              Exit exitAt = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
              if (exitAt != null)
                flag3 = m_Actor.Leader.Location.Map == exitAt.ToMap;
            }
            if (flag3) {
              m_Actor.Activity = Activity.FLEEING;
              return actorAction;
            }
          }
        }
        if (!(enemy.GetEquippedWeapon() is ItemRangedWeapon) && !Rules.IsAdjacent(m_Actor.Location, enemy.Location)) {
          ActorAction actorAction = BehaviorUseMedecine(game, 2, 2, 1, 0, 0);
          if (actorAction != null) {
            m_Actor.Activity = Activity.FLEEING;
            return actorAction;
          }
        }
        ActorAction actorAction1 = BehaviorWalkAwayFrom(game, enemies);
        if (actorAction1 != null) {
          if (doRun) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING;
          return actorAction1;
        }
        if (actorAction1 == null && IsAdjacentToEnemy(enemy)) {
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(50))
            game.DoEmote(m_Actor, emotes[1]);
          return BehaviorMeleeAttack(target.Percepted as Actor);
        }
      } else {
        ActorAction actorAction = BehaviorChargeEnemy(game, target);
        if (actorAction != null) {
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
            game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[2], (object) enemy.Name));
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return actorAction;
        }
      }
      return (ActorAction) null;
    }

    protected ActorAction BehaviorExplore(RogueGame game, ExplorationData exploration)
    {
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position.X - m_prevLocation.Position.X, m_Actor.Location.Position.Y - m_prevLocation.Position.Y);
      bool imStarvingOrCourageous = m_Actor.IsStarving || Directives.Courage == ActorCourage.COURAGEOUS;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        if (exploration.HasExplored(location)) return false;
        return IsValidMoveTowardGoalAction(game.Rules.IsBumpableFor(m_Actor, location));
      }), (Func<Direction, float>) (dir =>
      {
        Location loc = m_Actor.Location + dir;
        Map map = loc.Map;
        Point position = loc.Position;
        if (m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous && ComputeTrapsMaxDamage(map, position) >= m_Actor.HitPoints)
          return float.NaN;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position.X, position.Y))) num += 1000;
        if (!exploration.HasExplored(loc)) num += 500;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) num += 100;
        if (null != map.GetActivatedTrapAt(position)) num += -50;
        if (map.GetTileAt(position.X, position.Y).IsInside) {
          if (map.LocalTime.IsNight) num += 50;
        }
        else if (!map.LocalTime.IsNight) num += 50;
        if (dir == prevDirection) num += 25;
        return (float) (num + game.Rules.Roll(0, 10));
      }), (Func<float, float, bool>) ((a, b) =>
      {
        if (!float.IsNaN(a)) return (double) a > (double) b;
        return false;
      }));
      if (choiceEval != null) return new ActionBump(m_Actor, choiceEval.Choice);
      return null;
    }

    protected ActionCloseDoor BehaviorCloseDoorBehindMe(RogueGame game, Location previousLocation)
    {
      DoorWindow door = previousLocation.Map.GetMapObjectAt(previousLocation.Position) as DoorWindow;
      if (door == null) return null;
      ActionCloseDoor tmp = new ActionCloseDoor(m_Actor, door);
      return (tmp.IsLegal() ? tmp : null);
    }

    protected ActorAction BehaviorUseExit(RogueGame game, BaseAI.UseExitFlags useFlags)
    {
      Exit exitAt = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
      if (exitAt == null) return null;
      if (!exitAt.IsAnAIExit) return null;
      if ((useFlags & BaseAI.UseExitFlags.DONT_BACKTRACK) != BaseAI.UseExitFlags.NONE && exitAt.Location == m_prevLocation) return null;
      if ((useFlags & BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES) != BaseAI.UseExitFlags.NONE) {
        Actor actorAt = exitAt.Location.Actor;
        if (actorAt != null && m_Actor.IsEnemyOf(actorAt) && game.Rules.CanActorMeleeAttack(m_Actor, actorAt))
          return new ActionMeleeAttack(m_Actor, actorAt);
      }
      if ((useFlags & BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS) != BaseAI.UseExitFlags.NONE) {
        MapObject mapObjectAt = exitAt.Location.MapObject;
        if (mapObjectAt != null && game.Rules.IsBreakableFor(m_Actor, mapObjectAt))
          return new ActionBreak(m_Actor, mapObjectAt);
      }
      return (game.Rules.CanActorUseExit(m_Actor, m_Actor.Location.Position) ? new ActionUseExit(m_Actor, m_Actor.Location.Position) : null);
    }

    protected ItemBodyArmor GetWorstBodyArmor()
    {
      if (m_Actor.Inventory == null) return null;
      int num1 = int.MaxValue;
      ItemBodyArmor ret = null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        ItemBodyArmor tmp = obj as ItemBodyArmor;
        if (null == tmp) continue;
        if (DollPart.NONE != tmp.EquippedPart) continue;
        int num2 = tmp.Rating;
        if (num2 < num1) {
          num1 = num2;
          ret = tmp;
        }
      }
      return ret;
    }

    protected bool RHSMoreInteresting(Item lhs, Item rhs)
    {
#if DEBUG
      if (null == lhs) throw new ArgumentNullException("lhs"); 
      if (null == rhs) throw new ArgumentNullException("rhs"); 
//    if (!IsInterestingItem(lhs)) throw new ArgumentOutOfRangeException("lhs","!IsInterestingItem");   // LHS may be from own inventory
      if (!IsInterestingItem(rhs)) throw new ArgumentOutOfRangeException("rhs","!IsInterestingItem");
#endif
      if (IsItemTaboo(rhs)) return false;
      if (IsItemTaboo(lhs)) return true;
      if (lhs.Model.ID == rhs.Model.ID) {
        if (lhs.Quantity < rhs.Quantity) return true;
        if (lhs.Quantity > rhs.Quantity) return false;
        if (lhs is ItemLight)
          {
          return ((lhs as ItemLight).Batteries < (rhs as ItemLight).Batteries);
          }
        else if (lhs is ItemTracker)
          {
          return ((lhs as ItemTracker).Batteries < (rhs as ItemTracker).Batteries);
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

    protected ActorAction BehaviorMakeRoomFor(RogueGame game, Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException("it"); 
      if (!m_Actor.Inventory.IsFull) throw new ArgumentOutOfRangeException("inventory not full",m_Actor.Name);
      if (!IsInterestingItem(it)) throw new ArgumentOutOfRangeException("do not need to make room for uninteresting items");
      if (IsItemTaboo(it)) throw new ArgumentOutOfRangeException("do not need to make room for taboo items");
#endif

      Inventory inv = m_Actor.Inventory;
      if (it.Model.IsStackable && it.CanStackMore)
         {
         int qty;
         List<Item> tmp = inv.GetItemsStackableWith(it, out qty);
         if (qty>=it.Quantity) return null;
         }

      // not-best body armor can be dropped
      if (2<=m_Actor.CountItemQuantityOfType(typeof (ItemBodyArmor))) {
        ItemBodyArmor armor = GetWorstBodyArmor();
        if (null != armor) return BehaviorDropItem(armor);  
      }

      // not-best melee weapon can be dropped
      if (2<=m_Actor.CountItemQuantityOfType(typeof (ItemMeleeWeapon))) {
        ItemMeleeWeapon weapon = GetWorstMeleeWeapon();
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
          int num4 = game.Rules.ActorItemNutritionValue(m_Actor,food.NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter));
          if (num4 <= need) {
            if (game.Rules.CanActorUseItem(m_Actor, food)) return new ActionUseItem(m_Actor, food);
          }
        }
      }
      // it should be ok to devour stimulants in a glut
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        ItemMedicine stim2 = inv.GetBestDestackable(it) as ItemMedicine;
        if (null != stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = game.Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
          if (num4 <= need) {
            if (game.Rules.CanActorUseItem(m_Actor, stim2)) return new ActionUseItem(m_Actor, stim2);
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
          int num4 = game.Rules.ActorItemNutritionValue(m_Actor,food.NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter));
          if (num4*food.Quantity <= need) {
            if (game.Rules.CanActorUseItem(m_Actor, food)) return new ActionUseItem(m_Actor, food);
          }
        }
      }

      // finisbing off stimulants to get a free slot is ok
      ItemMedicine stim = inv.GetBestDestackable(game.GameItems[GameItems.IDs.MEDICINE_PILLS_SLP]) as ItemMedicine;
      if (null != stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = game.Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4*stim.Quantity <= need) {
          if (game.Rules.CanActorUseItem(m_Actor, stim)) return new ActionUseItem(m_Actor, stim);
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
          if (game.Rules.CanActorUseItem(m_Actor, tmpAmmo)) return new ActionUseItem(m_Actor, tmpAmmo);
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

    protected ActorAction BehaviorEnforceLaw(RogueGame game, List<Percept> percepts, out Actor target)
    {
      target = null;
      if (!m_Actor.Model.Abilities.IsLawEnforcer) return null;
      if (percepts == null) return null;
      List<Percept> percepts1 = FilterActors(percepts, (Predicate<Actor>) (a =>
      {
        if (a.MurdersCounter > 0) return !m_Actor.IsEnemyOf(a);
        return false;
      }));
      if (percepts1 == null || percepts1.Count == 0) return null;
      Percept percept = FilterNearest(percepts1);
      target = percept.Percepted as Actor;
      if (game.Rules.RollChance(game.Rules.ActorUnsuspicousChance(m_Actor, target))) {
        game.DoEmote(target, string.Format("moves unnoticed by {0}.", (object)m_Actor.Name));
        return null;
      }
      game.DoEmote(m_Actor, string.Format("takes a closer look at {0}.", (object) target.Name));
      int chance = game.Rules.ActorSpotMurdererChance(m_Actor, target);
      if (!game.Rules.RollChance(chance)) return null;
      game.DoMakeAggression(m_Actor, target);
      return new ActionSay(m_Actor, target, string.Format("HEY! YOU ARE WANTED FOR {0} MURDER{1}!", (object) target.MurdersCounter, target.MurdersCounter > 1 ? (object) "s" : (object) ""), RogueGame.Sayflags.IS_IMPORTANT);
    }

    protected ActorAction BehaviorGoEatFoodOnGround(RogueGame game, List<Percept> stacksPercepts)
    {
      if (stacksPercepts == null) return null;
      List<Percept> percepts = Filter(stacksPercepts, (Predicate<Percept>) (p => (p.Percepted as Inventory).Has<ItemFood>()));
      if (percepts == null) return null;
      Inventory itemsAt = m_Actor.Location.Map.GetItemsAt(m_Actor.Location.Position);
      ItemFood firstByType = itemsAt?.GetFirst<ItemFood>();
      if (null != firstByType) return new ActionEatFoodOnGround(m_Actor, firstByType);
      Percept percept = FilterNearest(percepts);
      return BehaviorStupidBumpToward(game, percept.Location.Position);
    }

    protected ActorAction BehaviorGoEatCorpse(RogueGame game, List<Percept> corpsesPercepts)
    {
      if (corpsesPercepts == null) return null;
      if (!m_Actor.CanEatCorpse) return null;
      if (m_Actor.Model.Abilities.IsUndead && m_Actor.HitPoints >= m_Actor.MaxHPs) return null;
      List<Corpse> corpsesAt = m_Actor.Location.Map.GetCorpsesAt(m_Actor.Location.Position);
      if (corpsesAt != null) {
        Corpse corpse = corpsesAt[0];
        return new ActionEatCorpse(m_Actor, corpse);
      }
      Percept percept = FilterNearest(corpsesPercepts);
      if (!m_Actor.Model.Abilities.IsIntelligent)
        return BehaviorStupidBumpToward(game, percept.Location.Position);
      return BehaviorIntelligentBumpToward(game, percept.Location.Position);
    }

    protected ActorAction BehaviorGoReviveCorpse(RogueGame game, List<Percept> corpsesPercepts)
    {
      if (corpsesPercepts == null) return null;
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) == 0) return null;
      if (!m_Actor.HasItemOfModel((ItemModel) game.GameItems.MEDIKIT)) return null;
      List<Percept> percepts = Filter(corpsesPercepts, (Predicate<Percept>) (p =>
      {
        foreach (Corpse corpse in p.Percepted as List<Corpse>) {
          if (game.Rules.CanActorReviveCorpse(m_Actor, corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return true;
        }
        return false;
      }));
      if (percepts == null) return null;
      List<Corpse> corpsesAt = m_Actor.Location.Map.GetCorpsesAt(m_Actor.Location.Position);
      if (corpsesAt != null) {
        foreach (Corpse corpse in corpsesAt) {
          if (game.Rules.CanActorReviveCorpse(m_Actor, corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return new ActionReviveCorpse(m_Actor, corpse);
        }
      }
      Percept percept = FilterNearest(percepts);
      if (!m_Actor.Model.Abilities.IsIntelligent)
        return BehaviorStupidBumpToward(game, percept.Location.Position);
      return BehaviorIntelligentBumpToward(game, percept.Location.Position);
    }

    // XXX why not delegate to Actor ...
    protected Item GetEquippedWeapon()
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemWeapon) return obj;
      }
      return null;
    }

    protected Item GetBestRangedWeaponWithAmmo(Predicate<Item> fn)
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      Item obj1 = null;
      int num1 = 0;
      foreach (Item obj2 in m_Actor.Inventory.Items) {
        ItemRangedWeapon w = obj2 as ItemRangedWeapon;
        if (w != null && (fn == null || fn(obj2)))
        {
          bool flag = false;
          if (w.Ammo > 0)
          {
            flag = true;
          }
          else
          {
            foreach (Item obj3 in m_Actor.Inventory.Items)
            {
              if (obj3 is ItemAmmo && (fn == null || fn(obj3)) && (obj3 as ItemAmmo).AmmoType == w.AmmoType)
              {
                flag = true;
                break;
              }
            }
          }
          if (flag)
          {
            int num2 = ScoreRangedWeapon(w);
            if (obj1 == null || num2 > num1)
            {
              obj1 = (Item) w;
              num1 = num2;
            }
          }
        }
      }
      return obj1;
    }

    protected int ScoreRangedWeapon(ItemRangedWeapon w)
    {
      ItemRangedWeaponModel rangedWeaponModel = w.Model as ItemRangedWeaponModel;
      return 1000 * rangedWeaponModel.Attack.Range + rangedWeaponModel.Attack.DamageValue;
    }

    protected Item GetFirstMeleeWeapon(Predicate<Item> fn)
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj is ItemMeleeWeapon && (fn == null || fn(obj))) return obj;
      }
      return null;
    }

    protected Item GetFirstBodyArmor(Predicate<Item> fn)
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj is ItemBodyArmor && (fn == null || fn(obj))) return obj;
      }
      return null;
    }

    protected bool IsRangedWeaponOutOfAmmo(Item it)
    {
      ItemRangedWeapon itemRangedWeapon = it as ItemRangedWeapon;
      if (itemRangedWeapon == null)
        return false;
      return itemRangedWeapon.Ammo <= 0;
    }

    public List<Item> GetTradeableItems(Inventory inv)
    {
      if (inv == null) return null;
      List<Item> ret = null;
      foreach (Item it in inv.Items) {
         if (IsTradeableItem(it)) {
           if (null == ret) ret = new List<Item>(inv.CountItems);
           ret.Add(it);
         }
      }
      return ret;
    }

    public bool HasAnyTradeableItem(Inventory inv)
    {
      if (inv == null) return false;
      foreach (Item it in inv.Items) {
        if (IsTradeableItem(it)) return true;
      }
      return false;
    }

    public bool HasAnyInterestingItem(Inventory inv)
    {
      if (inv == null) return false;
      foreach (Item it in inv.Items) {
        if (!IsItemTaboo(it)  && IsInterestingItem(it)) return true;
      }
      return false;
    }

    public bool HasAnyInterestingItem(List<Item> Items)
    {
      if (Items == null) return false;
      foreach (Item it in Items) {
        if (!IsItemTaboo(it) && IsInterestingItem(it)) return true;
      }
      return false;
    }

    protected Item FirstInterestingItem(Inventory inv)
    {
      if (inv == null) return null;
      foreach (Item it in inv.Items) {
        if (!IsItemTaboo(it) && IsInterestingItem(it)) return it;
      }
      return null;
    }

    protected void RunIfPossible()
    {
      if (!m_Actor.CanRun()) return;
      m_Actor.IsRunning = true;
    }

    protected void RunIfAdvisable(Point dest)
    {
      if (!m_Actor.CanRun()) return;
      MapObject mapObjectAt = m_Actor.Location.Map.GetMapObjectAt(dest);
      if (mapObjectAt != null && !mapObjectAt.IsWalkable && mapObjectAt.IsJumpable) {
        if (m_Actor.WillTireAfter(Rules.STAMINA_COST_RUNNING+Rules.STAMINA_COST_JUMP)) return;
      }
      if (m_Actor.WillTireAfter(Rules.STAMINA_COST_RUNNING)) return;
      m_Actor.IsRunning = true;
    }

    protected int GridDistancesSum(Point from, List<Percept> goals)
    {
      int num = 0;
      foreach (Percept goal in goals)
        num += Rules.GridDistance(from, goal.Location.Position);
      return num;
    }

    protected float SafetyFrom(Point from, List<Percept> dangers)
    {
      Map map = m_Actor.Location.Map;
      float num1 = (float) (GridDistancesSum(from, dangers) / (1 + dangers.Count));
      int num2 = 0;
      foreach (Direction direction in Direction.COMPASS) {
        Point point = from + direction;
        if (point == m_Actor.Location.Position || map.IsWalkableFor(point, m_Actor))
          ++num2;
      }
      float num3 = (float) num2 * 0.1f;
      bool isInside = map.GetTileAt(from).IsInside;
      int num4 = 0;
      foreach (Percept danger in dangers) {
        if (map.GetTileAt(danger.Location.Position).IsInside)
          ++num4;
        else
          --num4;
      }
      float num5 = 0.0f;
      if (isInside) {
        if (num4 < 0) num5 = 1.25f;
      }
      else if (num4 > 0) num5 = 1.25f;
      float num6 = 0.0f;
      if (m_Actor.Model.Abilities.CanTire && m_Actor.Model.Abilities.CanJump) {
        MapObject mapObjectAt = map.GetMapObjectAt(from);
        if (mapObjectAt != null && mapObjectAt.IsJumpable) num6 = 0.1f;
      }
      float num7 = 1f + num3 + num5 - num6;
      return num1 * num7;
    }

    protected BaseAI.ChoiceEval<_T_> Choose<_T_>(RogueGame game, List<_T_> listOfChoices, Func<_T_, bool> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (listOfChoices.Count == 0) return null;
      bool flag = false;
      float num = 0.0f;
      List<BaseAI.ChoiceEval<_T_>> choiceEvalList1 = new List<BaseAI.ChoiceEval<_T_>>(listOfChoices.Count);
      foreach(_T_ tmp in listOfChoices) {
        if (isChoiceValidFn(tmp)) {
          float f = evalChoiceFn(tmp);
          if (float.IsNaN(f)) continue;
          choiceEvalList1.Add(new BaseAI.ChoiceEval<_T_>(tmp, f));
          if (!flag || isBetterEvalThanFn(f, num)) {
            flag = true;
            num = f;
          }
        }
      }
      if (choiceEvalList1.Count == 0) return null;
      if (choiceEvalList1.Count == 1) return choiceEvalList1[0];
      List<BaseAI.ChoiceEval<_T_>> choiceEvalList2 = new List<BaseAI.ChoiceEval<_T_>>(choiceEvalList1.Count);
      foreach(BaseAI.ChoiceEval<_T_> tmp in choiceEvalList1) {
        if (tmp.Value == num) choiceEvalList2.Add(tmp);
      }
      return choiceEvalList2[game.Rules.Roll(0, choiceEvalList2.Count)];
    }

    protected BaseAI.ChoiceEval<_DATA_> ChooseExtended<_T_, _DATA_>(RogueGame game, List<_T_> listOfChoices, Func<_T_, _DATA_> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (listOfChoices.Count == 0) return null;
      bool flag = false;
      float num = 0.0f;
      List<BaseAI.ChoiceEval<_DATA_>> choiceEvalList1 = new List<BaseAI.ChoiceEval<_DATA_>>(listOfChoices.Count);
      foreach(_T_ tmp in listOfChoices)
      {
        _DATA_ choice = isChoiceValidFn(tmp);
        if (null == choice) continue;
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;
        choiceEvalList1.Add(new BaseAI.ChoiceEval<_DATA_>(choice, f));
        if (!flag || isBetterEvalThanFn(f, num)) {
          flag = true;
          num = f;
        }
      }
      if (choiceEvalList1.Count == 0) return null;
      if (choiceEvalList1.Count == 1) return choiceEvalList1[0];
      List<BaseAI.ChoiceEval<_DATA_>> choiceEvalList2 = new List<BaseAI.ChoiceEval<_DATA_>>(choiceEvalList1.Count);
      foreach(BaseAI.ChoiceEval<_DATA_> tmp in choiceEvalList1) {
        if (tmp.Value == num) choiceEvalList2.Add(tmp);
      }
      if (choiceEvalList2.Count == 0) return null;
      return choiceEvalList2[game.Rules.Roll(0, choiceEvalList2.Count)];
    }

    protected bool IsValidFleeingAction(ActorAction a)
    {
      if (null == a) return false;
      if (!(a is ActionMoveStep) && !(a is ActionOpenDoor))
        return a is ActionSwitchPlace;
      return true;
    }

    protected bool isValidWanderAction(ActorAction a)
    {
      if (null == a) return false;
      if (a is ActionMoveStep) return true;
      if (a is ActionSwitchPlace) return true;
      if (a is ActionPush) return true;
      if (a is ActionOpenDoor) return true;
      if (a is ActionBashDoor) return true;
      if (a is ActionChat) {
        return Directives.CanTrade || (a as ActionChat).Target == m_Actor.Leader;
      }
      if (a is ActionGetFromContainer) {
        Item it = (a as ActionGetFromContainer).Item;
        return !IsItemTaboo(it) && IsInterestingItem(it);
      }
      return a is ActionBarricadeDoor;
    }

    protected bool IsValidMoveTowardGoalAction(ActorAction a)
    {
      if (a != null && !(a is ActionChat) && (!(a is ActionGetFromContainer) && !(a is ActionSwitchPowerGenerator)))
        return !(a is ActionRechargeItemBattery);
      return false;
    }

    protected bool IsSoldier(Actor actor)
    {
      if (actor != null) return actor.Controller is SoldierAI;
      return false;
    }

    protected bool IsOccupiedByOther(Map map, Point position)
    {
      Actor actorAt = map.GetActorAt(position);
      if (actorAt != null) return actorAt != m_Actor;
      return false;
    }

    protected bool IsAdjacentToEnemy(Actor actor)
    {
      if (actor == null) return false;
      Map map = actor.Location.Map;
      return map.HasAnyAdjacentInMap(actor.Location.Position, (Predicate<Point>) (pt =>
      {
        Actor actorAt = map.GetActorAt(pt);
        if (actorAt == null) return false;
        return actor.IsEnemyOf(actorAt);
      }));
    }

    protected bool HasEquipedRangedWeapon(Actor actor)
    {
      return actor.GetEquippedWeapon() is ItemRangedWeapon;
    }

    protected ItemMeleeWeapon GetBestMeleeWeapon(Predicate<Item> fn)
    {
      if (m_Actor.Inventory == null) return null;
      List<ItemMeleeWeapon> tmp = m_Actor.Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (null == tmp) return null;
      int num1 = 0;
      ItemMeleeWeapon itemMeleeWeapon1 = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        if (fn == null || fn(obj)) {
          int num2 = (obj.Model as ItemMeleeWeaponModel).Attack.Rating;
          if (num2 > num1) {
            num1 = num2;
            itemMeleeWeapon1 = obj;
          }
        }
      }
      return itemMeleeWeapon1;
    }

    protected bool WantToEvadeMelee(RogueGame game, Actor actor, ActorCourage courage, Actor target)
    {
      if (WillTireAfterAttack(actor)) return true;
      if (actor.Speed > target.Speed) {
        if (actor.WillActAgainBefore(target)) return false;
        if (target.TargetActor == actor) return true;
      }
      Actor weakerInMelee = FindWeakerInMelee(game, m_Actor, target);
      return weakerInMelee != target && (weakerInMelee == m_Actor || courage != ActorCourage.COURAGEOUS);
    }

    protected Actor FindWeakerInMelee(RogueGame game, Actor a, Actor b)
    {
      int num1 = a.HitPoints + a.MeleeAttack(b).DamageValue;
      int num2 = b.HitPoints + b.MeleeAttack(a).DamageValue;
      if (num1 < num2) return a;
      if (num1 > num2) return b;
      return null;
    }

    protected bool WillTireAfterAttack(Actor actor)
    {
      return actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK+ actor.CurrentMeleeAttack.StaminaPenalty);
    }

    // XXX doesn't work in the presence of jumping
    protected bool WillTireAfterRunning(Actor actor)
    {
      return actor.WillTireAfter(Rules.STAMINA_COST_RUNNING);
    }

    protected bool HasSpeedAdvantage(Actor actor, Actor target)
    {
      int num1 = actor.Speed;
      int num2 = target.Speed;
      return num1 > num2 || actor.CanRun() && !target.CanRun() && (!WillTireAfterRunning(actor) && num1 * 2 > num2);
    }

    protected bool IsBetween(Point A, Point between, Point B)
    {
      return (double) Rules.StdDistance(A, between) + (double) Rules.StdDistance(B, between) <= (double) Rules.StdDistance(A, B) + 0.25;
    }

    protected bool IsFriendOf(Actor other)
    {
      if (!m_Actor.IsEnemyOf(other)) return m_Actor.Faction == other.Faction;
      return false;
    }

    protected Actor GetNearestTargetFor(Actor actor)
    {
      Map map = actor.Location.Map;
      Actor actor1 = null;
      int num1 = int.MaxValue;
      foreach (Actor actor2 in map.Actors) {
        if (!actor2.IsDead && actor2 != actor && actor.IsEnemyOf(actor2)) {
          int num2 = Rules.GridDistance(actor2.Location.Position, actor.Location.Position);
          if (num2 < num1 && (num2 == 1 || LOS.CanTraceViewLine(actor.Location, actor2.Location.Position))) {
            num1 = num2;
            actor1 = actor2;
          }
        }
      }
      return actor1;
    }

    protected List<Exit> ListAdjacentExits(Location fromLocation)
    {
      List<Exit> exitList = null;
      foreach (Direction direction in Direction.COMPASS) {
        Point pos = fromLocation.Position + direction;
        Exit exitAt = fromLocation.Map.GetExitAt(pos);
        if (exitAt != null) {
          if (exitList == null) exitList = new List<Exit>(8);
          exitList.Add(exitAt);
        }
      }
      return exitList;
    }

    protected Exit PickAnyAdjacentExit(RogueGame game, Location fromLocation)
    {
      List<Exit> exitList = ListAdjacentExits(fromLocation);
      if (exitList == null) return null;
      return exitList[game.Rules.Roll(0, exitList.Count)];
    }

    public static bool IsZoneChange(Map map, Point pos)
    {
      List<Zone> zonesHere = map.GetZonesAt(pos.X, pos.Y);
      if (zonesHere == null)
        return false;
      return map.HasAnyAdjacentInMap(pos, (Predicate<Point>) (adj =>
      {
        List<Zone> zonesAt = map.GetZonesAt(adj.X, adj.Y);
        if (zonesAt == null)
          return false;
        if (zonesHere == null)
          return true;
        foreach (Zone zone in zonesAt)
        {
          if (!zonesHere.Contains(zone))
            return true;
        }
        return false;
      }));
    }

    protected Point RandomPositionNear(Rules rules, Map map, Point goal, int range)
    {
      int x = goal.X + rules.Roll(-range, range);
      int y = goal.Y + rules.Roll(-range, range);
      map.TrimToBounds(ref x, ref y);
      return new Point(x, y);
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

    public bool IsItemTaboo(Item it)
    {
      if (m_TabooItems == null) return false;
      return m_TabooItems.ContainsKey(it);
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

    protected void ClearTabooTrades()
    {
      m_TabooTrades = null;
    }

    protected void ExpireTaboos()
    {
      // maintain taboo information
      int time = m_Actor.LastActionTurn;
      if (null != m_TabooItems) {
        foreach (Item tmp in new List<Item>(m_TabooItems.Keys)) { 
          if (m_TabooItems[tmp] > time) continue;
          m_TabooItems.Remove(tmp);
        }
        if (0 == m_TabooItems.Count) m_TabooItems = null;
      }
      if (null != m_TabooTiles) {
        foreach (Point tmp in new List<Point>(m_TabooTiles.Keys)) {
        if (m_TabooTiles[tmp] > time) continue;
          m_TabooTiles.Remove(tmp);
        }
        if (0 == m_TabooTiles.Count) m_TabooTiles = null;
      }
      // actors ok to clear at midnight
      if (m_Actor.Location.Map.LocalTime.IsStrikeOfMidnight)
        ClearTabooTrades();
    }


    protected class ChoiceEval<_T_>
    {
      public _T_ Choice { get; private set; }

      public float Value { get; private set; }

      public ChoiceEval(_T_ choice, float value)
      {
                Choice = choice;
                Value = value;
      }

      public override string ToString()
      {
        return string.Format("ChoiceEval({0}; {1:F})", (object)Choice == null ? (object) "NULL" : (object)Choice.ToString(), (object)Value);
      }
    }

    [System.Flags]
    protected enum UseExitFlags
    {
      NONE = 0,
      BREAK_BLOCKING_OBJECTS = 1,
      ATTACK_BLOCKING_ENEMIES = 2,
      DONT_BACKTRACK = 4,
    }
  }
}
