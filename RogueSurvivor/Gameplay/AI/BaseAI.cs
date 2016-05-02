// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.BaseAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

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

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal abstract class BaseAI : AIController
  {
    private const int FLEE_THROUGH_EXIT_CHANCE = 50;
    private const int EMOTE_GRAB_ITEM_CHANCE = 30;
    private const int EMOTE_FLEE_CHANCE = 30;
    private const int EMOTE_FLEE_TRAPPED_CHANCE = 50;
    private const int EMOTE_CHARGE_CHANCE = 30;
    private const float MOVE_DISTANCE_PENALTY = 0.42f;
    private const float LEADER_LOF_PENALTY = 1f;
    private ActorOrder m_Order;
    private ActorDirective m_Directive;
    private Location m_prevLocation;
    private List<Item> m_TabooItems;
    private List<Point> m_TabooTiles;
    private List<Actor> m_TabooTrades;

    public override ActorOrder Order
    {
      get
      {
        return this.m_Order;
      }
    }

    public override ActorDirective Directives
    {
      get
      {
        if (this.m_Directive == null)
          this.m_Directive = new ActorDirective();
        return this.m_Directive;
      }
      set
      {
        this.m_Directive = value;
      }
    }

    protected Location PrevLocation
    {
      get
      {
        return this.m_prevLocation;
      }
    }

    protected List<Item> TabooItems
    {
      get
      {
        return this.m_TabooItems;
      }
    }

    protected List<Point> TabooTiles
    {
      get
      {
        return this.m_TabooTiles;
      }
    }

    protected List<Actor> TabooTrades
    {
      get
      {
        return this.m_TabooTrades;
      }
    }

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      this.CreateSensors();
      this.m_TabooItems = (List<Item>) null;
      this.m_TabooTiles = (List<Point>) null;
      this.m_TabooTrades = (List<Actor>) null;
    }

    public override void SetOrder(ActorOrder newOrder)
    {
      this.m_Order = newOrder;
    }

    public override ActorAction GetAction(RogueGame game)
    {
      List<Percept> percepts = this.UpdateSensors(game);
      if (this.m_prevLocation.Map == null)
        this.m_prevLocation = this.m_Actor.Location;
      this.m_Actor.TargetActor = (Actor) null;
      ActorAction actorAction = this.SelectAction(game, percepts);
      this.m_prevLocation = this.m_Actor.Location;
      if (actorAction != null)
        return actorAction;
      this.m_Actor.Activity = Activity.IDLE;
      return (ActorAction) new ActionWait(this.m_Actor, game);
    }

    protected abstract void CreateSensors();

    protected abstract List<Percept> UpdateSensors(RogueGame game);

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
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      Map map = m_Actor.Location.Map;
      foreach (Percept percept in percepts) {
        if (percept.Location.Map == map) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    protected List<Percept> FilterEnemies(RogueGame game, List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      foreach (Percept percept in percepts) {
        Actor target = percept.Percepted as Actor;
        if (null != target && target != m_Actor && game.Rules.IsEnemyOf(m_Actor, target)) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    protected List<Percept> FilterNonEnemies(RogueGame game, List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      foreach (Percept percept in percepts) {
        Actor target = percept.Percepted as Actor;
        if (null != target && target != m_Actor && !game.Rules.IsEnemyOf(m_Actor, target)) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    protected List<Percept> FilterCurrent(List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      foreach (Percept percept in percepts) {
        if (percept.Turn == turnCounter) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

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

    protected List<Percept> FilterActors(List<Percept> percepts, Predicate<Actor> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      foreach (Percept percept in percepts) {
        Actor actor = percept.Percepted as Actor;
        if (null != actor && predicateFn(actor)) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    protected List<Percept> FilterFireTargets(RogueGame game, List<Percept> percepts)
    {
      return this.Filter(game, percepts, (Predicate<Percept>) (p =>
      {
        Actor target = p.Percepted as Actor;
        if (target == null)
          return false;
        return game.Rules.CanActorFireAt(this.m_Actor, target);
      }));
    }

    protected List<Percept> FilterStacks(RogueGame game, List<Percept> percepts)
    {
      return this.Filter(game, percepts, (Predicate<Percept>) (p => p.Percepted is Inventory));
    }

    protected List<Percept> FilterCorpses(RogueGame game, List<Percept> percepts)
    {
      return this.Filter(game, percepts, (Predicate<Percept>) (p => p.Percepted is List<Corpse>));
    }

    protected List<Percept> Filter(RogueGame game, List<Percept> percepts, Predicate<Percept> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      foreach (Percept percept in percepts) {
        if (predicateFn(percept)) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    protected Percept FilterFirst(RogueGame game, List<Percept> percepts, Predicate<Percept> predicateFn)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      foreach (Percept percept in percepts) {
        if (predicateFn(percept)) return percept;
      }
      return null;
    }

    protected List<Percept> FilterOut(RogueGame game, List<Percept> percepts, Predicate<Percept> rejectPredicateFn)
    {
      return this.Filter(game, percepts, (Predicate<Percept>) (p => !rejectPredicateFn(p)));
    }

    protected List<Percept> SortByDistance(List<Percept> percepts)  // dead function?
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

    protected List<Percept> SortByDate(RogueGame game, List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = new List<Percept>((IEnumerable<Percept>) percepts);
      perceptList.Sort((Comparison<Percept>) ((pA, pB) =>
      {
        if (pA.Turn < pB.Turn)
          return 1;
        return pA.Turn <= pB.Turn ? 0 : -1;
      }));
      return perceptList;
    }

    protected ActorAction BehaviorWander(RogueGame game, Predicate<Location> goodWanderLocFn)
    {
      BaseAI.ChoiceEval<Direction> choiceEval = this.Choose<Direction>(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Location location = this.m_Actor.Location + dir;
        if (goodWanderLocFn != null && !goodWanderLocFn(location))
          return false;
        return this.isValidWanderAction(game.Rules.IsBumpableFor(this.m_Actor, game, location));
      }), (Func<Direction, float>) (dir =>
      {
        int num = game.Rules.Roll(0, 666);
        if (this.m_Actor.Model.Abilities.IsIntelligent && BaseAI.IsAnyActivatedTrapThere(this.m_Actor.Location.Map, (this.m_Actor.Location + dir).Position))
          num -= 1000;
        return (float) num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval != null)
        return (ActorAction) new ActionBump(this.m_Actor, game, choiceEval.Choice);
      return (ActorAction) null;
    }

    protected ActorAction BehaviorWander(RogueGame game)
    {
      return this.BehaviorWander(game, (Predicate<Location>) null);
    }

    protected ActorAction BehaviorBumpToward(RogueGame game, Point goal, Func<Point, Point, float> distanceFn)
    {
      BaseAI.ChoiceEval<ActorAction> choiceEval = this.ChooseExtended<Direction, ActorAction>(game, Direction.COMPASS_LIST, (Func<Direction, ActorAction>) (dir =>
      {
        Location location = this.m_Actor.Location + dir;
        ActorAction a = game.Rules.IsBumpableFor(this.m_Actor, game, location);
        if (a == null)
        {
          if (this.m_Actor.Model.Abilities.IsUndead && game.Rules.HasActorPushAbility(this.m_Actor))
          {
            MapObject mapObjectAt = this.m_Actor.Location.Map.GetMapObjectAt(location.Position);
            if (mapObjectAt != null && game.Rules.CanActorPush(this.m_Actor, mapObjectAt))
            {
              Direction pushDir = game.Rules.RollDirection();
              if (game.Rules.CanPushObjectTo(mapObjectAt, mapObjectAt.Location.Position + pushDir))
                return (ActorAction) new ActionPush(this.m_Actor, game, mapObjectAt, pushDir);
            }
          }
          return (ActorAction) null;
        }
        if (location.Position == goal || this.IsValidMoveTowardGoalAction(a))
          return a;
        return (ActorAction) null;
      }), (Func<Direction, float>) (dir =>
      {
        Location location = this.m_Actor.Location + dir;
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
      return this.BehaviorBumpToward(game, goal, (Func<Point, Point, float>) ((ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = Rules.StdDistance(ptA, ptB);
        if (!game.Rules.IsWalkableFor(this.m_Actor, this.m_Actor.Location.Map, ptA.X, ptA.Y))
          num += 0.42f;
        return num;
      }));
    }

    protected ActorAction BehaviorIntelligentBumpToward(RogueGame game, Point goal)
    {
      float currentDistance = Rules.StdDistance(this.m_Actor.Location.Position, goal);
      bool imStarvingOrCourageous = m_Actor.IsStarving || Directives.Courage == ActorCourage.COURAGEOUS;
      return this.BehaviorBumpToward(game, goal, (Func<Point, Point, float>) ((ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = Rules.StdDistance(ptA, ptB);
        if ((double) num >= (double) currentDistance) return float.NaN;
        if (!imStarvingOrCourageous)
        {
          int trapsMaxDamage = this.ComputeTrapsMaxDamage(this.m_Actor.Location.Map, ptA);
          if (trapsMaxDamage > 0)
          {
            if (trapsMaxDamage >= this.m_Actor.HitPoints) return float.NaN;
            num += 0.42f;
          }
        }
        return num;
      }));
    }

    // A number of the melee enemy targeting sequences not only work on grid distance,
    // they need to return a coordinated action/target pair.
    protected ActorAction TargetGridMelee(RogueGame game, List<Percept> perceptList, out Percept target)
    {
      target = null;
      ActorAction ret = null;
      int num1 = int.MaxValue;
      foreach (Percept percept in perceptList)
      {
        int num2 = Rules.GridDistance(m_Actor.Location.Position, percept.Location.Position);
        if (num2 < num1)
        {
          ActorAction tmp = BehaviorStupidBumpToward(game, percept.Location.Position);
          if (null != tmp)
          {
            num1 = num2;
            target = percept;
            ret = tmp;
          }
        }
      }
      return ret;
    }

        protected ActorAction BehaviorWalkAwayFrom(RogueGame game, Percept goal)
    {
      return this.BehaviorWalkAwayFrom(game, new List<Percept>(1) { goal });
    }

    protected ActorAction BehaviorWalkAwayFrom(RogueGame game, List<Percept> goals)
    {
      Actor leader = this.m_Actor.Leader;
      bool flag = this.m_Actor.HasLeader && this.m_Actor.GetEquippedWeapon() is ItemRangedWeapon;
      Actor actor = (Actor) null;
      if (flag)
        actor = this.GetNearestTargetFor(game, this.m_Actor.Leader);
      bool checkLeaderLoF = actor != null && actor.Location.Map == this.m_Actor.Location.Map;
      List<Point> leaderLoF = (List<Point>) null;
      if (checkLeaderLoF)
      {
        leaderLoF = new List<Point>(1);
        ItemRangedWeapon itemRangedWeapon = this.m_Actor.GetEquippedWeapon() as ItemRangedWeapon;
        LOS.CanTraceFireLine(leader.Location, actor.Location.Position, (itemRangedWeapon.Model as ItemRangedWeaponModel).Attack.Range, leaderLoF);
      }
      BaseAI.ChoiceEval<Direction> choiceEval = this.Choose<Direction>(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir => this.IsValidFleeingAction(game.Rules.IsBumpableFor(this.m_Actor, game, this.m_Actor.Location + dir))), (Func<Direction, float>) (dir =>
      {
        Location location = this.m_Actor.Location + dir;
        float num = this.SafetyFrom(game.Rules, location.Position, goals);
        if (this.m_Actor.HasLeader)
        {
          num -= Rules.StdDistance(location.Position, this.m_Actor.Leader.Location.Position);
          if (checkLeaderLoF && leaderLoF.Contains(location.Position))
            --num;
        }
        return num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval != null)
        return (ActorAction) new ActionBump(this.m_Actor, game, choiceEval.Choice);
      return (ActorAction) null;
    }

    protected ActorAction BehaviorMeleeAttack(RogueGame game, Percept target)
    {
      Actor target1 = target.Percepted as Actor;
      if (target1 == null)
        throw new ArgumentException("percepted is not an actor");
      if (!game.Rules.CanActorMeleeAttack(this.m_Actor, target1))
        return (ActorAction) null;
      return (ActorAction) new ActionMeleeAttack(this.m_Actor, game, target1);
    }

    protected ActorAction BehaviorRangedAttack(RogueGame game, Percept target)
    {
      Actor target1 = target.Percepted as Actor;
      if (target1 == null)
        throw new ArgumentException("percepted is not an actor");
      if (!game.Rules.CanActorFireAt(this.m_Actor, target1))
        return (ActorAction) null;
      return (ActorAction) new ActionRangedAttack(this.m_Actor, game, target1);
    }

    protected ActorAction BehaviorEquipWeapon(RogueGame game)
    {
      Item equippedWeapon = this.GetEquippedWeapon();
      if (equippedWeapon != null && equippedWeapon is ItemRangedWeapon)
      {
        if (!this.Directives.CanFireWeapons)
          return (ActorAction) new ActionUnequipItem(this.m_Actor, game, equippedWeapon);
        ItemRangedWeapon rw = equippedWeapon as ItemRangedWeapon;
        if (rw.Ammo > 0) return null;
        ItemAmmo compatibleAmmoItem = GetCompatibleAmmoItem(rw);
        if (compatibleAmmoItem != null)
          return (ActorAction) new ActionUseItem(this.m_Actor, game, (Item) compatibleAmmoItem);
      }
      if (this.Directives.CanFireWeapons)
      {
        Item rangedWeaponWithAmmo = this.GetBestRangedWeaponWithAmmo((Predicate<Item>) (it => !this.IsItemTaboo(it)));
        if (rangedWeaponWithAmmo != null && game.Rules.CanActorEquipItem(this.m_Actor, rangedWeaponWithAmmo))
          return (ActorAction) new ActionEquipItem(this.m_Actor, game, rangedWeaponWithAmmo);
      }

      // ranged weapon non-option for some reason
      ItemMeleeWeapon bestMeleeWeapon = GetBestMeleeWeapon(game, (Predicate<Item>) (it => !this.IsItemTaboo(it)));
      if (bestMeleeWeapon == null) return null;
      if (equippedWeapon == bestMeleeWeapon) return null;
      return (ActorAction) new ActionEquipItem(m_Actor, game, (Item) bestMeleeWeapon);
    }

    protected ActorAction BehaviorUnequipCellPhoneIfLeaderHasNot(RogueGame game)
    {
      ItemTracker itemTracker1 = this.m_Actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
      if (itemTracker1 == null) return null;
      if (!itemTracker1.CanTrackFollowersOrLeader) return null;
      ItemTracker itemTracker2 = m_Actor.Leader.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
      if ((itemTracker2 == null || !itemTracker2.CanTrackFollowersOrLeader))
        return (ActorAction) new ActionUnequipItem(this.m_Actor, game, (Item) itemTracker1);
      return null;
    }

    protected ActorAction BehaviorEquipStenchKiller(RogueGame game)
    {
      if ((Item) this.GetEquippedStenchKiller() != null)
        return (ActorAction) null;
      ItemSprayScent firstStenchKiller = this.GetFirstStenchKiller((Predicate<ItemSprayScent>)(it =>
      {
          if (0 < it.SprayQuantity) return !this.IsItemTaboo((Item)it);
          return false;
      }));
      if (firstStenchKiller != null && game.Rules.CanActorEquipItem(this.m_Actor, (Item) firstStenchKiller))
        return (ActorAction) new ActionEquipItem(this.m_Actor, game, (Item) firstStenchKiller);
      return (ActorAction) null;
    }

    protected ActorAction BehaviorUnequipLeftItem(RogueGame game)
    {
      Item equippedItem = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
      if (equippedItem == null) return null;
      return (ActorAction) new ActionUnequipItem(m_Actor, game, equippedItem);
    }

    protected ActorAction BehaviorGrabFromStack(RogueGame game, Point position, Inventory stack)
    {
      if (stack == null || stack.IsEmpty) return null;
      MapObject mapObjectAt = this.m_Actor.Location.Map.GetMapObjectAt(position);
      if (mapObjectAt != null)
      {
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
        game.DoEmote(this.m_Actor, string.Format("{0}! Great!", (object) it1.AName));
      if (position == this.m_Actor.Location.Position)
        return (ActorAction) new ActionTakeItem(this.m_Actor, game, position, it1);
      return this.BehaviorIntelligentBumpToward(game, position);
    }

    protected ActorAction BehaviorDropItem(RogueGame game, Item it)
    {
      if (it == null) return null;
      if (Rules.CanActorUnequipItem(m_Actor, it))
      {
        MarkItemAsTaboo(it);
        return (ActorAction) new ActionUnequipItem(m_Actor, game, it);
      }
      if (!game.Rules.CanActorDropItem(this.m_Actor, it)) return null;
      UnmarkItemAsTaboo(it);
      return (ActorAction) new ActionDropItem(m_Actor, game, it);
    }

    protected ActorAction BehaviorDropUselessItem(RogueGame game)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item it in m_Actor.Inventory.Items) {
        if (it.IsUseless) return BehaviorDropItem(game, it);
      }
      return null;
    }

    protected ActorAction BehaviorRestIfTired(RogueGame game)
    {
      if (this.m_Actor.StaminaPoints >= Rules.STAMINA_MIN_FOR_ACTIVITY)
        return (ActorAction) null;
      return (ActorAction) new ActionWait(this.m_Actor, game);
    }

    protected int ComputeTrapsMaxDamage(Map map, Point pos)
    {
      Inventory itemsAt = map.GetItemsAt(pos);
      if (itemsAt == null)
        return 0;
      int num = 0;
      foreach (Item obj in itemsAt.Items)
      {
        ItemTrap itemTrap = obj as ItemTrap;
        if (itemTrap != null)
          num += itemTrap.TrapModel.Damage;
      }
      return num;
    }

    protected ActorAction BehaviorBuildTrap(RogueGame game)
    {
      ItemTrap itemTrap = this.m_Actor.Inventory.GetFirstByType(typeof (ItemTrap)) as ItemTrap;
      if (itemTrap == null)
        return (ActorAction) null;
      string reason;
      if (!this.IsGoodTrapSpot(game, this.m_Actor.Location.Map, this.m_Actor.Location.Position, out reason))
        return (ActorAction) null;
      if (!itemTrap.IsActivated && !itemTrap.TrapModel.ActivatesWhenDropped)
        return (ActorAction) new ActionUseItem(this.m_Actor, game, (Item) itemTrap);
      game.DoEmote(this.m_Actor, string.Format("{0} {1}!", (object) reason, (object) itemTrap.AName));
      return (ActorAction) new ActionDropItem(this.m_Actor, game, (Item) itemTrap);
    }

    protected bool IsGoodTrapSpot(RogueGame game, Map map, Point pos, out string reason)
    {
      reason = "";
      bool flag = false;
      bool isInside = map.GetTileAt(pos).IsInside;
      if (!isInside && map.GetCorpsesAt(pos) != null)
      {
        reason = "that corpse will serve as a bait for";
        flag = true;
      }
      else if (this.m_prevLocation.Map.GetTileAt(this.m_prevLocation.Position).IsInside != isInside)
      {
        reason = "protecting the building with";
        flag = true;
      }
      else
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pos);
        if (mapObjectAt != null && mapObjectAt is DoorWindow)
        {
          reason = "protecting the doorway with";
          flag = true;
        }
        else if (map.GetExitAt(pos) != null)
        {
          reason = "protecting the exit with";
          flag = true;
        }
      }
      if (!flag)
        return false;
      Inventory itemsAt = map.GetItemsAt(pos);
      return itemsAt == null || itemsAt.CountItemsMatching((Predicate<Item>) (it =>
      {
        ItemTrap itemTrap = it as ItemTrap;
        if (itemTrap == null)
          return false;
        return itemTrap.IsActivated;
      })) <= 3;
    }

    protected ActorAction BehaviorAttackBarricade(RogueGame game)
    {
      Map map = this.m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(this.m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        DoorWindow doorWindow = map.GetMapObjectAt(pt) as DoorWindow;
        if (doorWindow != null)
          return doorWindow.IsBarricaded;
        return false;
      }));
      if (pointList == null)
        return (ActorAction) null;
      DoorWindow doorWindow1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]) as DoorWindow;
      ActionBreak actionBreak = new ActionBreak(this.m_Actor, game, (MapObject) doorWindow1);
      if (actionBreak.IsLegal())
        return (ActorAction) actionBreak;
      return (ActorAction) null;
    }

    protected ActorAction BehaviorAssaultBreakables(RogueGame game, HashSet<Point> fov)
    {
      Map map = this.m_Actor.Location.Map;
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
      ActionBreak actionBreak = new ActionBreak(m_Actor, game, percept.Percepted as MapObject);
      if (actionBreak.IsLegal()) return actionBreak;
      return null;
    }

    protected ActorAction BehaviorPushNonWalkableObject(RogueGame game)
    {
      if (!game.Rules.HasActorPushAbility(m_Actor)) return null;
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || mapObjectAt.IsWalkable)
          return false;
        return game.Rules.CanActorPush(this.m_Actor, mapObjectAt);
      }));
      if (pointList == null) return null;
      MapObject mapObjectAt1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      ActionPush actionPush = new ActionPush(m_Actor, game, mapObjectAt1, game.Rules.RollDirection());
      if (actionPush.IsLegal()) return actionPush;
      return null;
    }

    protected ActorAction BehaviorPushNonWalkableObjectForFood(RogueGame game)
    {
      if (!game.Rules.HasActorPushAbility(m_Actor)) return null;
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        // Wrecked cars are very tiring to push, and are jumpable so they don't need to be pushed.
        if (mapObjectAt == null || mapObjectAt.IsWalkable || mapObjectAt.IsJumpable)
          return false;
        return game.Rules.CanActorPush(this.m_Actor, mapObjectAt);
      }));
      if (pointList == null) return null;
      MapObject mapObjectAt1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      ActionPush actionPush = new ActionPush(m_Actor, game, mapObjectAt1, game.Rules.RollDirection());
      if (actionPush.IsLegal()) return actionPush;
      return null;
    }

    protected ActorAction BehaviorUseMedecine(RogueGame game, int factorHealing, int factorStamina, int factorSleep, int factorCure, int factorSan)
    {
      Inventory inventory = this.m_Actor.Inventory;
      if (inventory == null || inventory.IsEmpty)
        return (ActorAction) null;
      bool needHP = this.m_Actor.HitPoints < game.Rules.ActorMaxHPs(this.m_Actor);
      bool needSTA = game.Rules.IsActorTired(this.m_Actor);
      bool needSLP = m_Actor.WouldLikeToSleep;
      bool needCure = this.m_Actor.Infection > 0;
      bool needSan = this.m_Actor.Model.Abilities.HasSanity && this.m_Actor.Sanity < (int) (0.75 * (double) game.Rules.ActorMaxSanity(this.m_Actor));
      if (!needHP && !needSTA && (!needSLP && !needCure) && !needSan)
        return (ActorAction) null;
      List<ItemMedicine> itemsByType = inventory.GetItemsByType<ItemMedicine>();
      if (itemsByType == null)
        return (ActorAction) null;
      BaseAI.ChoiceEval<ItemMedicine> choiceEval = this.Choose<ItemMedicine>(game, itemsByType, (Func<ItemMedicine, bool>) (it => true), (Func<ItemMedicine, float>) (it =>
      {
        int num = 0;
        if (needHP)
          num += factorHealing * it.Healing;
        if (needSTA)
          num += factorStamina * it.StaminaBoost;
        if (needSLP)
          num += factorSleep * it.SleepBoost;
        if (needCure)
          num += factorCure * it.InfectionCure;
        if (needSan)
          num += factorSan * it.SanityCure;
        return (float) num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      if (choiceEval == null || (double) choiceEval.Value <= 0.0)
        return (ActorAction) null;
      return (ActorAction) new ActionUseItem(this.m_Actor, game, (Item) choiceEval.Choice);
    }

    protected ActorAction BehaviorUseEntertainment(RogueGame game)
    {
      Inventory inventory = this.m_Actor.Inventory;
      if (inventory.IsEmpty)
        return (ActorAction) null;
      ItemEntertainment itemEntertainment = (ItemEntertainment) inventory.GetFirstByType(typeof (ItemEntertainment));
      if (itemEntertainment == null)
        return (ActorAction) null;
      if (!game.Rules.CanActorUseItem(this.m_Actor, (Item) itemEntertainment))
        return (ActorAction) null;
      return (ActorAction) new ActionUseItem(this.m_Actor, game, (Item) itemEntertainment);
    }

    protected ActorAction BehaviorDropBoringEntertainment(RogueGame game)
    {
      Inventory inventory = this.m_Actor.Inventory;
      if (inventory.IsEmpty)
        return (ActorAction) null;
      foreach (Item it in inventory.Items)
      {
        if (it is ItemEntertainment && this.m_Actor.IsBoredOf(it))
          return (ActorAction) new ActionDropItem(this.m_Actor, game, it);
      }
      return (ActorAction) null;
    }

    protected ActorAction BehaviorFollowActor(RogueGame game, Actor other, Point otherPosition, bool isVisible, int maxDist)
    {
      if (other == null || other.IsDead)
        return (ActorAction) null;
      int num = Rules.GridDistance(m_Actor.Location.Position, otherPosition);
      if (isVisible && num <= maxDist)
        return (ActorAction) new ActionWait(this.m_Actor, game);
      if (other.Location.Map != this.m_Actor.Location.Map)
      {
        Exit exitAt = this.m_Actor.Location.Map.GetExitAt(this.m_Actor.Location.Position);
        if (exitAt != null && exitAt.ToMap == other.Location.Map && game.Rules.CanActorUseExit(this.m_Actor, this.m_Actor.Location.Position))
          return (ActorAction) new ActionUseExit(this.m_Actor, this.m_Actor.Location.Position, game);
      }
      ActorAction actorAction = this.BehaviorIntelligentBumpToward(game, otherPosition);
      if (actorAction == null || !actorAction.IsLegal())
        return (ActorAction) null;
      if (other.IsRunning)
        this.RunIfPossible(game.Rules);
      return actorAction;
    }

    protected ActorAction BehaviorHangAroundActor(RogueGame game, Actor other, Point otherPosition, int minDist, int maxDist)
    {
      if (other == null || other.IsDead)
        return (ActorAction) null;
      int num = 0;
      Point p;
      do
      {
        p = otherPosition;
        p.X += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        p.Y += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        this.m_Actor.Location.Map.TrimToBounds(ref p);
      }
      while (Rules.GridDistance(p, otherPosition) < minDist && ++num < 100);
      ActorAction a = this.BehaviorIntelligentBumpToward(game, p);
      if (a == null || !this.IsValidMoveTowardGoalAction(a) || !a.IsLegal())
        return (ActorAction) null;
      if (other.IsRunning)
        this.RunIfPossible(game.Rules);
      return a;
    }

    protected ActorAction BehaviorTrackScent(RogueGame game, List<Percept> scents)
    {
      if (scents == null || scents.Count == 0)
        return (ActorAction) null;
      Percept percept = FilterStrongestScent(scents);
      Map map = this.m_Actor.Location.Map;
      if (!(this.m_Actor.Location.Position == percept.Location.Position))
        return this.BehaviorIntelligentBumpToward(game, percept.Location.Position) ?? (ActorAction) null;
      if (map.GetExitAt(this.m_Actor.Location.Position) != null && this.m_Actor.Model.Abilities.AI_CanUseAIExits)
        return this.BehaviorUseExit(game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      return (ActorAction) null;
    }

    protected ActorAction BehaviorChargeEnemy(RogueGame game, Percept target)
    {
      ActorAction tmpAction = BehaviorMeleeAttack(game, target);
      if (null != tmpAction) return tmpAction;
      Actor actor = target.Percepted as Actor;
      if (game.Rules.IsActorTired(this.m_Actor) && Rules.IsAdjacent(m_Actor.Location, target.Location))
        return BehaviorUseMedecine(game, 0, 1, 0, 0, 0) ?? new ActionWait(this.m_Actor, game);
      tmpAction = BehaviorIntelligentBumpToward(game, target.Location.Position);
      if (null == tmpAction) return null;
      if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) RunIfPossible(game.Rules);
      return tmpAction;
    }

    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, bool hasVisibleLeader, bool isLeaderFighting, ActorCourage courage, string[] emotes)
    {
      Percept target = FilterNearest(enemies);
      bool flag1 = false;
      Actor enemy = target.Percepted as Actor;
      bool flag2;
      if (this.HasEquipedRangedWeapon(enemy))
        flag2 = false;
      else if (this.m_Actor.Model.Abilities.IsLawEnforcer && enemy.MurdersCounter > 0)
        flag2 = false;
      else if (game.Rules.IsActorTired(m_Actor) && Rules.IsAdjacent(m_Actor.Location, enemy.Location))
        flag2 = true;
      else if (this.m_Actor.Leader != null)
      {
        switch (courage)
        {
          case ActorCourage.COWARD:
            flag2 = true;
            flag1 = true;
            break;
          case ActorCourage.CAUTIOUS:
            flag2 = this.WantToEvadeMelee(game, this.m_Actor, courage, enemy);
            flag1 = !this.HasSpeedAdvantage(game, this.m_Actor, enemy);
            break;
          case ActorCourage.COURAGEOUS:
            if (isLeaderFighting)
            {
              flag2 = false;
              break;
            }
            flag2 = this.WantToEvadeMelee(game, this.m_Actor, courage, enemy);
            flag1 = !this.HasSpeedAdvantage(game, this.m_Actor, enemy);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled courage");
        }
      }
      else
      {
        switch (courage)
        {
          case ActorCourage.COWARD:
            flag2 = true;
            flag1 = true;
            break;
          case ActorCourage.CAUTIOUS:
          case ActorCourage.COURAGEOUS:
            flag2 = this.WantToEvadeMelee(game, this.m_Actor, courage, enemy);
            flag1 = !this.HasSpeedAdvantage(game, this.m_Actor, enemy);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled courage");
        }
      }
      if (flag2)
      {
        if (this.m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
          game.DoEmote(this.m_Actor, string.Format("{0} {1}!", (object) emotes[0], (object) enemy.Name));
        if (this.m_Actor.Model.Abilities.CanUseMapObjects)
        {
          BaseAI.ChoiceEval<Direction> choiceEval = this.Choose<Direction>(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
          {
            Point point = this.m_Actor.Location.Position + dir;
            DoorWindow door = this.m_Actor.Location.Map.GetMapObjectAt(point) as DoorWindow;
            return door != null && (this.IsBetween(this.m_Actor.Location.Position, point, enemy.Location.Position) && game.Rules.IsClosableFor(this.m_Actor, door)) && (Rules.GridDistance(point, enemy.Location.Position) != 1 || !game.Rules.IsClosableFor(enemy, door));
          }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
          if (choiceEval != null)
            return (ActorAction) new ActionCloseDoor(this.m_Actor, game, this.m_Actor.Location.Map.GetMapObjectAt(this.m_Actor.Location.Position + choiceEval.Choice) as DoorWindow);
        }
        if (this.m_Actor.Model.Abilities.CanBarricade)
        {
          BaseAI.ChoiceEval<Direction> choiceEval = this.Choose<Direction>(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
          {
            Point point = this.m_Actor.Location.Position + dir;
            DoorWindow door = this.m_Actor.Location.Map.GetMapObjectAt(point) as DoorWindow;
            return door != null && (this.IsBetween(this.m_Actor.Location.Position, point, enemy.Location.Position) && game.Rules.CanActorBarricadeDoor(this.m_Actor, door));
          }), (Func<Direction, float>) (dir => (float) game.Rules.Roll(0, 666)), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
          if (choiceEval != null)
            return (ActorAction) new ActionBarricadeDoor(this.m_Actor, game, this.m_Actor.Location.Map.GetMapObjectAt(this.m_Actor.Location.Position + choiceEval.Choice) as DoorWindow);
        }
        if (this.m_Actor.Model.Abilities.AI_CanUseAIExits && game.Rules.RollChance(50))
        {
          ActorAction actorAction = this.BehaviorUseExit(game, BaseAI.UseExitFlags.NONE);
          if (actorAction != null)
          {
            bool flag3 = true;
            if (this.m_Actor.HasLeader)
            {
              Exit exitAt = this.m_Actor.Location.Map.GetExitAt(this.m_Actor.Location.Position);
              if (exitAt != null)
                flag3 = this.m_Actor.Leader.Location.Map == exitAt.ToMap;
            }
            if (flag3)
            {
              this.m_Actor.Activity = Activity.FLEEING;
              return actorAction;
            }
          }
        }
        if (!(enemy.GetEquippedWeapon() is ItemRangedWeapon) && !Rules.IsAdjacent(m_Actor.Location, enemy.Location))
        {
          ActorAction actorAction = this.BehaviorUseMedecine(game, 2, 2, 1, 0, 0);
          if (actorAction != null)
          {
            this.m_Actor.Activity = Activity.FLEEING;
            return actorAction;
          }
        }
        ActorAction actorAction1 = this.BehaviorWalkAwayFrom(game, enemies);
        if (actorAction1 != null)
        {
          if (flag1)
            this.RunIfPossible(game.Rules);
          this.m_Actor.Activity = Activity.FLEEING;
          return actorAction1;
        }
        if (actorAction1 == null && this.IsAdjacentToEnemy(game, enemy))
        {
          if (this.m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(50))
            game.DoEmote(this.m_Actor, emotes[1]);
          return this.BehaviorMeleeAttack(game, target);
        }
      }
      else
      {
        ActorAction actorAction = this.BehaviorChargeEnemy(game, target);
        if (actorAction != null)
        {
          if (this.m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
            game.DoEmote(this.m_Actor, string.Format("{0} {1}!", (object) emotes[2], (object) enemy.Name));
          this.m_Actor.Activity = Activity.FIGHTING;
          this.m_Actor.TargetActor = target.Percepted as Actor;
          return actorAction;
        }
      }
      return (ActorAction) null;
    }

    protected ActorAction BehaviorExplore(RogueGame game, ExplorationData exploration)
    {
      Direction prevDirection = Direction.FromVector(this.m_Actor.Location.Position.X - this.m_prevLocation.Position.X, this.m_Actor.Location.Position.Y - this.m_prevLocation.Position.Y);
      bool imStarvingOrCourageous = m_Actor.IsStarving || Directives.Courage == ActorCourage.COURAGEOUS;
      BaseAI.ChoiceEval<Direction> choiceEval = this.Choose<Direction>(game, Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Location location = this.m_Actor.Location + dir;
        if (exploration.HasExplored(location))
          return false;
        return this.IsValidMoveTowardGoalAction(game.Rules.IsBumpableFor(this.m_Actor, game, location));
      }), (Func<Direction, float>) (dir =>
      {
        Location loc = this.m_Actor.Location + dir;
        Map map = loc.Map;
        Point position = loc.Position;
        if (this.m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous && this.ComputeTrapsMaxDamage(map, position) >= this.m_Actor.HitPoints)
          return float.NaN;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position.X, position.Y)))
          num += 1000;
        if (!exploration.HasExplored(loc))
          num += 500;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow))
          num += 100;
        if (BaseAI.IsAnyActivatedTrapThere(map, position))
          num += -50;
        if (map.GetTileAt(position.X, position.Y).IsInside)
        {
          if (map.LocalTime.IsNight)
            num += 50;
        }
        else if (!map.LocalTime.IsNight)
          num += 50;
        if (dir == prevDirection)
          num += 25;
        return (float) (num + game.Rules.Roll(0, 10));
      }), (Func<float, float, bool>) ((a, b) =>
      {
        if (!float.IsNaN(a))
          return (double) a > (double) b;
        return false;
      }));
      if (choiceEval != null)
        return (ActorAction) new ActionBump(this.m_Actor, game, choiceEval.Choice);
      return (ActorAction) null;
    }

    protected ActorAction BehaviorCloseDoorBehindMe(RogueGame game, Location previousLocation)
    {
      DoorWindow door = previousLocation.Map.GetMapObjectAt(previousLocation.Position) as DoorWindow;
      if (door == null)
        return (ActorAction) null;
      if (game.Rules.IsClosableFor(this.m_Actor, door))
        return (ActorAction) new ActionCloseDoor(this.m_Actor, game, door);
      return (ActorAction) null;
    }

    protected ActorAction BehaviorUseExit(RogueGame game, BaseAI.UseExitFlags useFlags)
    {
      Exit exitAt = this.m_Actor.Location.Map.GetExitAt(this.m_Actor.Location.Position);
      if (exitAt == null)
        return (ActorAction) null;
      if (!exitAt.IsAnAIExit)
        return (ActorAction) null;
      if ((useFlags & BaseAI.UseExitFlags.DONT_BACKTRACK) != BaseAI.UseExitFlags.NONE && exitAt.ToMap == this.m_prevLocation.Map && exitAt.ToPosition == this.m_prevLocation.Position)
        return (ActorAction) null;
      if ((useFlags & BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES) != BaseAI.UseExitFlags.NONE)
      {
        Actor actorAt = exitAt.ToMap.GetActorAt(exitAt.ToPosition);
        if (actorAt != null && game.Rules.IsEnemyOf(this.m_Actor, actorAt) && game.Rules.CanActorMeleeAttack(this.m_Actor, actorAt))
          return (ActorAction) new ActionMeleeAttack(this.m_Actor, game, actorAt);
      }
      if ((useFlags & BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS) != BaseAI.UseExitFlags.NONE)
      {
        MapObject mapObjectAt = exitAt.ToMap.GetMapObjectAt(exitAt.ToPosition);
        if (mapObjectAt != null && game.Rules.IsBreakableFor(this.m_Actor, mapObjectAt))
          return (ActorAction) new ActionBreak(this.m_Actor, game, mapObjectAt);
      }
      if (!game.Rules.CanActorUseExit(this.m_Actor, this.m_Actor.Location.Position))
        return (ActorAction) null;
      return (ActorAction) new ActionUseExit(this.m_Actor, this.m_Actor.Location.Position, game);
    }

    protected ActorAction BehaviorMakeRoomForFood(RogueGame game, List<Percept> stacks)
    {
      if (stacks == null || stacks.Count == 0)
        return (ActorAction) null;
      if (this.m_Actor.Inventory.CountItems < Rules.ActorMaxInv(this.m_Actor))
        return (ActorAction) null;
      if (this.HasItemOfType(typeof (ItemFood)))
        return (ActorAction) null;
      bool flag = false;
      foreach (Percept stack in stacks)
      {
        Inventory inventory = stack.Percepted as Inventory;
        if (inventory != null && inventory.HasItemOfType(typeof (ItemFood)))
        {
          flag = true;
          break;
        }
      }
      if (!flag)
        return (ActorAction) null;
      Inventory inventory1 = this.m_Actor.Inventory;
      Item firstMatching1 = inventory1.GetFirstMatching((Predicate<Item>) (it => !IsInterestingItem(it)));
      if (firstMatching1 != null)
        return this.BehaviorDropItem(game, firstMatching1);
      Item firstMatching2 = inventory1.GetFirstMatching((Predicate<Item>) (it => it is ItemBarricadeMaterial));
      if (firstMatching2 != null)
        return this.BehaviorDropItem(game, firstMatching2);
      Item firstMatching3 = inventory1.GetFirstMatching((Predicate<Item>) (it => it is ItemLight));
      if (firstMatching3 != null)
        return this.BehaviorDropItem(game, firstMatching3);
      Item firstMatching4 = inventory1.GetFirstMatching((Predicate<Item>) (it => it is ItemSprayPaint));
      if (firstMatching4 != null)
        return this.BehaviorDropItem(game, firstMatching4);
      Item firstMatching5 = inventory1.GetFirstMatching((Predicate<Item>) (it => it is ItemSprayScent));
      if (firstMatching5 != null)
        return this.BehaviorDropItem(game, firstMatching5);
      Item firstMatching6 = inventory1.GetFirstMatching((Predicate<Item>) (it => it is ItemAmmo));
      if (firstMatching6 != null)
        return this.BehaviorDropItem(game, firstMatching6);
      Item firstMatching7 = inventory1.GetFirstMatching((Predicate<Item>) (it => it is ItemMedicine));
      if (firstMatching7 != null)
        return this.BehaviorDropItem(game, firstMatching7);
      Item it1 = inventory1[game.Rules.Roll(0, inventory1.CountItems)];
      return this.BehaviorDropItem(game, it1);
    }

    protected ActorAction BehaviorUseStenchKiller(RogueGame game)
    {
      ItemSprayScent itemSprayScent = this.m_Actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayScent;
      if (itemSprayScent == null)
        return (ActorAction) null;
      if (itemSprayScent.SprayQuantity <= 0)
        return (ActorAction) null;
      if ((itemSprayScent.Model as ItemSprayScentModel).Odor != Odor.PERFUME_LIVING_SUPRESSOR)
        return (ActorAction) null;
      if (!this.IsGoodStenchKillerSpot(game, this.m_Actor.Location.Map, this.m_Actor.Location.Position))
        return (ActorAction) null;
      ActionUseItem actionUseItem = new ActionUseItem(this.m_Actor, game, (Item) itemSprayScent);
      if (actionUseItem.IsLegal())
        return (ActorAction) actionUseItem;
      return (ActorAction) null;
    }

    protected bool IsGoodStenchKillerSpot(RogueGame game, Map map, Point pos)
    {
      if (map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, pos) > 0)
        return false;
      if (this.m_prevLocation.Map.GetTileAt(this.m_prevLocation.Position).IsInside != map.GetTileAt(pos).IsInside)
        return true;
      MapObject mapObjectAt = map.GetMapObjectAt(pos);
      return mapObjectAt != null && mapObjectAt is DoorWindow || map.GetExitAt(pos) != null;
    }

    protected ActorAction BehaviorEnforceLaw(RogueGame game, List<Percept> percepts, out Actor target)
    {
      target = (Actor) null;
      if (!this.m_Actor.Model.Abilities.IsLawEnforcer)
        return (ActorAction) null;
      if (percepts == null)
        return (ActorAction) null;
      List<Percept> percepts1 = FilterActors(percepts, (Predicate<Actor>) (a =>
      {
        if (a.MurdersCounter > 0)
          return !game.Rules.IsEnemyOf(this.m_Actor, a);
        return false;
      }));
      if (percepts1 == null || percepts1.Count == 0)
        return (ActorAction) null;
      Percept percept = FilterNearest(percepts1);
      target = percept.Percepted as Actor;
      if (game.Rules.RollChance(game.Rules.ActorUnsuspicousChance(this.m_Actor, target)))
      {
        game.DoEmote(target, string.Format("moves unnoticed by {0}.", (object) this.m_Actor.Name));
        return (ActorAction) null;
      }
      game.DoEmote(this.m_Actor, string.Format("takes a closer look at {0}.", (object) target.Name));
      int chance = game.Rules.ActorSpotMurdererChance(this.m_Actor, target);
      if (!game.Rules.RollChance(chance))
        return (ActorAction) null;
      game.DoMakeAggression(this.m_Actor, target);
      return (ActorAction) new ActionSay(this.m_Actor, game, target, string.Format("HEY! YOU ARE WANTED FOR {0} MURDER{1}!", (object) target.MurdersCounter, target.MurdersCounter > 1 ? (object) "s" : (object) ""), RogueGame.Sayflags.IS_IMPORTANT);
    }

    protected ActorAction BehaviorGoEatFoodOnGround(RogueGame game, List<Percept> stacksPercepts)
    {
      if (stacksPercepts == null)
        return (ActorAction) null;
      List<Percept> percepts = this.Filter(game, stacksPercepts, (Predicate<Percept>) (p => (p.Percepted as Inventory).HasItemOfType(typeof (ItemFood))));
      if (percepts == null)
        return (ActorAction) null;
      Inventory itemsAt = this.m_Actor.Location.Map.GetItemsAt(this.m_Actor.Location.Position);
      if (itemsAt != null && itemsAt.HasItemOfType(typeof (ItemFood)))
      {
        Item firstByType = itemsAt.GetFirstByType(typeof (ItemFood));
        return (ActorAction) new ActionEatFoodOnGround(this.m_Actor, game, firstByType);
      }
      Percept percept = FilterNearest(percepts);
      return BehaviorStupidBumpToward(game, percept.Location.Position);
    }

    protected ActorAction BehaviorGoEatCorpse(RogueGame game, List<Percept> corpsesPercepts)
    {
      if (corpsesPercepts == null)
        return (ActorAction) null;
      if (this.m_Actor.Model.Abilities.IsUndead && this.m_Actor.HitPoints >= game.Rules.ActorMaxHPs(this.m_Actor))
        return (ActorAction) null;
      List<Corpse> corpsesAt = this.m_Actor.Location.Map.GetCorpsesAt(this.m_Actor.Location.Position);
      if (corpsesAt != null)
      {
        Corpse corpse = corpsesAt[0];
        if (game.Rules.CanActorEatCorpse(this.m_Actor, corpse))
          return (ActorAction) new ActionEatCorpse(this.m_Actor, game, corpse);
      }
      Percept percept = FilterNearest(corpsesPercepts);
      if (!m_Actor.Model.Abilities.IsIntelligent)
        return BehaviorStupidBumpToward(game, percept.Location.Position);
      return BehaviorIntelligentBumpToward(game, percept.Location.Position);
    }

    protected ActorAction BehaviorGoReviveCorpse(RogueGame game, List<Percept> corpsesPercepts)
    {
      if (corpsesPercepts == null) return null;
      if (this.m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) == 0) return null;
      if (!m_Actor.HasItemOfModel((ItemModel) game.GameItems.MEDIKIT)) return null;
      List<Percept> percepts = this.Filter(game, corpsesPercepts, (Predicate<Percept>) (p =>
      {
        foreach (Corpse corpse in p.Percepted as List<Corpse>)
        {
          if (game.Rules.CanActorReviveCorpse(this.m_Actor, corpse) && !game.Rules.IsEnemyOf(this.m_Actor, corpse.DeadGuy))
            return true;
        }
        return false;
      }));
      if (percepts == null)
        return (ActorAction) null;
      List<Corpse> corpsesAt = this.m_Actor.Location.Map.GetCorpsesAt(this.m_Actor.Location.Position);
      if (corpsesAt != null)
      {
        foreach (Corpse corpse in corpsesAt)
        {
          if (game.Rules.CanActorReviveCorpse(this.m_Actor, corpse) && !game.Rules.IsEnemyOf(this.m_Actor, corpse.DeadGuy))
            return (ActorAction) new ActionReviveCorpse(this.m_Actor, game, corpse);
        }
      }
      Percept percept = FilterNearest(percepts);
      if (!this.m_Actor.Model.Abilities.IsIntelligent)
        return this.BehaviorStupidBumpToward(game, percept.Location.Position);
      return this.BehaviorIntelligentBumpToward(game, percept.Location.Position);
    }

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
      Item obj1 = (Item) null;
      int num1 = 0;
      foreach (Item obj2 in this.m_Actor.Inventory.Items)
      {
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
            foreach (Item obj3 in this.m_Actor.Inventory.Items)
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
            int num2 = this.ScoreRangedWeapon(w);
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
      foreach (Item obj in this.m_Actor.Inventory.Items) {
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

    protected ItemSprayScent GetEquippedStenchKiller()
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemSprayScent && ((obj as ItemSprayScent).Model as ItemSprayScentModel).Odor == Odor.PERFUME_LIVING_SUPRESSOR)
          return obj as ItemSprayScent;
      }
      return (ItemSprayScent) null;
    }

    protected ItemSprayScent GetFirstStenchKiller(Predicate<ItemSprayScent> fn)
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj is ItemSprayScent && (fn == null || fn(obj as ItemSprayScent)))
          return obj as ItemSprayScent;
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
        if (IsInterestingItem(it)) return true;
      }
      return false;
    }

    public bool HasAnyInterestingItem(List<Item> Items)
    {
      if (Items == null) return false;
      foreach (Item it in Items) {
        if (IsInterestingItem(it)) return true;
      }
      return false;
    }

    protected Item FirstInterestingItem(Inventory inv)
    {
      if (inv == null) return null;
      foreach (Item it in inv.Items) {
        if (IsInterestingItem(it)) return it;
      }
      return null;
    }

    protected bool HasItemOfType(Type tt)
    {
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return false;
      return m_Actor.Inventory.HasItemOfType(tt);
    }

    protected void RunIfPossible(Rules rules)
    {
      if (!rules.CanActorRun(this.m_Actor))
        return;
      this.m_Actor.IsRunning = true;
    }

    protected int GridDistancesSum(Point from, List<Percept> goals)
    {
      int num = 0;
      foreach (Percept goal in goals)
        num += Rules.GridDistance(from, goal.Location.Position);
      return num;
    }

    protected float SafetyFrom(Rules rules, Point from, List<Percept> dangers)
    {
      Map map = m_Actor.Location.Map;
      float num1 = (float) (GridDistancesSum(from, dangers) / (1 + dangers.Count));
      int num2 = 0;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point point = from + direction;
        if (point == this.m_Actor.Location.Position || rules.IsWalkableFor(this.m_Actor, map, point.X, point.Y))
          ++num2;
      }
      float num3 = (float) num2 * 0.1f;
      bool isInside = map.GetTileAt(from).IsInside;
      int num4 = 0;
      foreach (Percept danger in dangers)
      {
        if (map.GetTileAt(danger.Location.Position).IsInside)
          ++num4;
        else
          --num4;
      }
      float num5 = 0.0f;
      if (isInside)
      {
        if (num4 < 0)
          num5 = 1.25f;
      }
      else if (num4 > 0)
        num5 = 1.25f;
      float num6 = 0.0f;
      if (this.m_Actor.Model.Abilities.CanTire && this.m_Actor.Model.Abilities.CanJump)
      {
        MapObject mapObjectAt = map.GetMapObjectAt(from);
        if (mapObjectAt != null && mapObjectAt.IsJumpable)
          num6 = 0.1f;
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
      if (a is ActionGetFromContainer) return IsInterestingItem((a as ActionGetFromContainer).Item);
      return a is ActionBarricadeDoor;
    }

    protected bool IsValidMoveTowardGoalAction(ActorAction a)
    {
      if (a != null && !(a is ActionChat) && (!(a is ActionGetFromContainer) && !(a is ActionSwitchPowerGenerator)))
        return !(a is ActionRechargeItemBattery);
      return false;
    }

    protected bool HasNoFoodItems(Actor actor)
    {
      Inventory inventory = actor.Inventory;
      if (inventory == null || inventory.IsEmpty)
        return true;
      return !inventory.HasItemOfType(typeof (ItemFood));
    }

    protected bool IsSoldier(Actor actor)
    {
      if (actor != null)
        return actor.Controller is SoldierAI;
      return false;
    }

    protected bool IsOccupiedByOther(Map map, Point position)
    {
      Actor actorAt = map.GetActorAt(position);
      if (actorAt != null)
        return actorAt != this.m_Actor;
      return false;
    }

    protected bool IsAdjacentToEnemy(RogueGame game, Actor actor)
    {
      if (actor == null)
        return false;
      Map map = actor.Location.Map;
      return map.HasAnyAdjacentInMap(actor.Location.Position, (Predicate<Point>) (pt =>
      {
        Actor actorAt = map.GetActorAt(pt);
        if (actorAt == null)
          return false;
        return game.Rules.IsEnemyOf(actor, actorAt);
      }));
    }

    protected bool IsInside(Actor actor)
    {
      if (actor == null)
        return false;
      return actor.Location.Map.GetTileAt(actor.Location.Position.X, actor.Location.Position.Y).IsInside;
    }

    protected bool HasEquipedRangedWeapon(Actor actor)
    {
      return actor.GetEquippedWeapon() is ItemRangedWeapon;
    }

    protected ItemMeleeWeapon GetBestMeleeWeapon(RogueGame game, Predicate<Item> fn)
    {
      if (this.m_Actor.Inventory == null)
        return (ItemMeleeWeapon) null;
      int num1 = 0;
      ItemMeleeWeapon itemMeleeWeapon1 = (ItemMeleeWeapon) null;
      foreach (Item obj in this.m_Actor.Inventory.Items)
      {
        if (fn == null || fn(obj))
        {
          ItemMeleeWeapon itemMeleeWeapon2 = obj as ItemMeleeWeapon;
          if (itemMeleeWeapon2 != null)
          {
            ItemMeleeWeaponModel meleeWeaponModel = itemMeleeWeapon2.Model as ItemMeleeWeaponModel;
            int num2 = 10000 * meleeWeaponModel.Attack.DamageValue + 100 * meleeWeaponModel.Attack.HitValue + -meleeWeaponModel.Attack.StaminaPenalty;
            if (num2 > num1)
            {
              num1 = num2;
              itemMeleeWeapon1 = itemMeleeWeapon2;
            }
          }
        }
      }
      return itemMeleeWeapon1;
    }

    protected bool WantToEvadeMelee(RogueGame game, Actor actor, ActorCourage courage, Actor target)
    {
      if (this.WillTireAfterAttack(game, actor))
        return true;
      if (game.Rules.ActorSpeed(actor) > game.Rules.ActorSpeed(target))
      {
        if (game.Rules.WillActorActAgainBefore(actor, target))
          return false;
        if (target.TargetActor == actor)
          return true;
      }
      Actor weakerInMelee = this.FindWeakerInMelee(game, this.m_Actor, target);
      return weakerInMelee != target && (weakerInMelee == this.m_Actor || courage != ActorCourage.COURAGEOUS);
    }

    protected Actor FindWeakerInMelee(RogueGame game, Actor a, Actor b)
    {
      int num1 = a.HitPoints + a.CurrentMeleeAttack.DamageValue;
      int num2 = b.HitPoints + b.CurrentMeleeAttack.DamageValue;
      if (num1 < num2)
        return a;
      if (num1 <= num2)
        return (Actor) null;
      return b;
    }

    // XXX doesn't work at night or with substantial melee weapons
    protected bool WillTireAfterAttack(RogueGame game, Actor actor)
    {
      if (!actor.Model.Abilities.CanTire)
        return false;
      return actor.StaminaPoints - 8 < Rules.STAMINA_MIN_FOR_ACTIVITY;
    }

    // XXX doesn't work at night
    protected bool WillTireAfterRunning(RogueGame game, Actor actor)
    {
      if (!actor.Model.Abilities.CanTire)
        return false;
      return actor.StaminaPoints - 4 < Rules.STAMINA_MIN_FOR_ACTIVITY;
    }

    protected bool HasSpeedAdvantage(RogueGame game, Actor actor, Actor target)
    {
      int num1 = game.Rules.ActorSpeed(actor);
      int num2 = game.Rules.ActorSpeed(target);
      return num1 > num2 || game.Rules.CanActorRun(actor) && !game.Rules.CanActorRun(target) && (!this.WillTireAfterRunning(game, actor) && num1 * 2 > num2);
    }

    protected bool NeedsLight(RogueGame game)
    {
      switch (this.m_Actor.Location.Map.Lighting)
      {
        case Lighting._FIRST:
          return true;
        case Lighting.OUTSIDE:
          if (!this.m_Actor.Location.Map.LocalTime.IsNight)
            return false;
          if (game.Session.World.Weather != Weather.HEAVY_RAIN)
            return !this.m_Actor.Location.Map.GetTileAt(this.m_Actor.Location.Position.X, this.m_Actor.Location.Position.Y).IsInside;
          return true;
        case Lighting.LIT:
          return false;
        default:
          throw new ArgumentOutOfRangeException("unhandled lighting");
      }
    }

    protected bool IsBetween(Point A, Point between, Point B)
    {
      return (double) Rules.StdDistance(A, between) + (double) Rules.StdDistance(B, between) <= (double) Rules.StdDistance(A, B) + 0.25;
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

    protected bool IsFriendOf(RogueGame game, Actor other)
    {
      if (!game.Rules.IsEnemyOf(this.m_Actor, other))
        return this.m_Actor.Faction == other.Faction;
      return false;
    }

    protected Actor GetNearestTargetFor(RogueGame game, Actor actor)
    {
      Map map = actor.Location.Map;
      Actor actor1 = (Actor) null;
      int num1 = int.MaxValue;
      foreach (Actor actor2 in map.Actors)
      {
        if (!actor2.IsDead && actor2 != actor && game.Rules.IsEnemyOf(actor, actor2))
        {
          int num2 = Rules.GridDistance(actor2.Location.Position, actor.Location.Position);
          if (num2 < num1 && (num2 == 1 || LOS.CanTraceViewLine(actor.Location, actor2.Location.Position)))
          {
            num1 = num2;
            actor1 = actor2;
          }
        }
      }
      return actor1;
    }

    protected List<Exit> ListAdjacentExits(RogueGame game, Location fromLocation)
    {
      List<Exit> exitList = (List<Exit>) null;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point pos = fromLocation.Position + direction;
        Exit exitAt = fromLocation.Map.GetExitAt(pos);
        if (exitAt != null)
        {
          if (exitList == null)
            exitList = new List<Exit>(8);
          exitList.Add(exitAt);
        }
      }
      return exitList;
    }

    protected Exit PickAnyAdjacentExit(RogueGame game, Location fromLocation)
    {
      List<Exit> exitList = this.ListAdjacentExits(game, fromLocation);
      if (exitList == null)
        return (Exit) null;
      return exitList[game.Rules.Roll(0, exitList.Count)];
    }

    public static bool IsAnyActivatedTrapThere(Map map, Point pos)
    {
      Inventory itemsAt = map.GetItemsAt(pos);
      if (itemsAt == null || itemsAt.IsEmpty)
        return false;
      return itemsAt.GetFirstMatching((Predicate<Item>) (it =>
      {
        ItemTrap itemTrap = it as ItemTrap;
        if (itemTrap != null)
          return itemTrap.IsActivated;
        return false;
      })) != null;
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

    protected void MarkItemAsTaboo(Item it)
    {
      if (this.m_TabooItems == null)
        this.m_TabooItems = new List<Item>(1);
      else if (this.m_TabooItems.Contains(it))
        return;
      this.m_TabooItems.Add(it);
    }

    protected void UnmarkItemAsTaboo(Item it)
    {
      if (this.m_TabooItems == null)
        return;
      this.m_TabooItems.Remove(it);
      if (this.m_TabooItems.Count != 0)
        return;
      this.m_TabooItems = (List<Item>) null;
    }

    protected bool IsItemTaboo(Item it)
    {
      if (this.m_TabooItems == null)
        return false;
      return this.m_TabooItems.Contains(it);
    }

    protected void MarkTileAsTaboo(Point p)
    {
      if (this.m_TabooTiles == null)
        this.m_TabooTiles = new List<Point>(1);
      else if (this.m_TabooTiles.Contains(p))
        return;
      this.m_TabooTiles.Add(p);
    }

    protected bool IsTileTaboo(Point p)
    {
      if (this.m_TabooTiles == null)
        return false;
      return this.m_TabooTiles.Contains(p);
    }

    protected void ClearTabooTiles()
    {
      this.m_TabooTiles = (List<Point>) null;
    }

    protected void MarkActorAsRecentTrade(Actor other)
    {
      if (this.m_TabooTrades == null)
        this.m_TabooTrades = new List<Actor>(1);
      else if (this.m_TabooTrades.Contains(other))
        return;
      this.m_TabooTrades.Add(other);
    }

    protected bool IsActorTabooTrade(Actor other)
    {
      if (this.m_TabooTrades == null)
        return false;
      return this.m_TabooTrades.Contains(other);
    }

    protected void ClearTabooTrades()
    {
      this.m_TabooTrades = (List<Actor>) null;
    }

    protected class ChoiceEval<_T_>
    {
      public _T_ Choice { get; private set; }

      public float Value { get; private set; }

      public ChoiceEval(_T_ choice, float value)
      {
        this.Choice = choice;
        this.Value = value;
      }

      public override string ToString()
      {
        return string.Format("ChoiceEval({0}; {1:F})", (object) this.Choice == null ? (object) "NULL" : (object) this.Choice.ToString(), (object) this.Value);
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
