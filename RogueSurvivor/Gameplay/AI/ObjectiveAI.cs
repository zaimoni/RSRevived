using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using ActionUseItem = djack.RogueSurvivor.Engine.Actions.ActionUseItem;
using ActionDropItem = djack.RogueSurvivor.Engine.Actions.ActionDropItem;
using ActionPutInContainer = djack.RogueSurvivor.Engine.Actions.ActionPutInContainer;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal abstract class ObjectiveAI : BaseAI
  {
    readonly protected List<Objective> Objectives = new List<Objective>();
    readonly private Dictionary<Point,Dictionary<Point, int>> PlannedMoves = new Dictionary<Point, Dictionary<Point, int>>();
    private int _STA_reserve;
    int STA_reserve { get { return _STA_reserve; } }

    protected bool RunIfAdvisable(Point dest)
    {
      if (!m_Actor.CanRun()) return false;
      // we don't want preparing to push a car to block running at full stamina
      if (m_Actor.MaxSTA > m_Actor.StaminaPoints) {
        if (m_Actor.RunIsFreeMove) {
          if (m_Actor.WillTireAfter(STA_reserve + m_Actor.RunningStaminaCost(dest))) return false;
        } else {
          if (m_Actor.WillTireAfter(STA_reserve + 2*m_Actor.RunningStaminaCost(dest)- m_Actor.NightSTApenalty)) return false;
        }
      }
      return true;
    }

    protected void ReserveSTA(int jump, int melee, int push, int push_weight)   // currently jump and break have the same cost
    {
      int tmp = push_weight;
      tmp += jump*Rules.STAMINA_COST_JUMP;
      tmp += melee*(Rules.STAMINA_COST_MELEE_ATTACK+m_Actor.BestMeleeAttack().StaminaPenalty);

      _STA_reserve = tmp+m_Actor.NightSTApenalty*(jump+melee+push);
    }

    // these two return a value copy for correctness
    protected Dictionary<Point, int> PlanApproach(Zaimoni.Data.FloodfillPathfinder<Point> navigate)
    {
      PlannedMoves.Clear();
      Dictionary<Point, int> dest = navigate.Approach(m_Actor.Location.Position);
      PlannedMoves[m_Actor.Location.Position] = dest;
      foreach(Point pt in dest.Keys) {
        if (0>navigate.Cost(pt)) continue;
        PlannedMoves[pt] = navigate.Approach(pt);
      }
      return new Dictionary<Point,int>(PlannedMoves[m_Actor.Location.Position]);
    }

    protected void ClearMovePlan()
    {
      PlannedMoves.Clear();
    }

    public Dictionary<Point, int> MovePlanIf(Point pt)
    {
      if (!PlannedMoves.ContainsKey(pt)) return null;
      if (null==PlannedMoves[pt]) return null;  // XXX probably being used incorrectly
      return new Dictionary<Point,int>(PlannedMoves[pt]);
    }

#region damage field
    protected void VisibleMaximumDamage(Dictionary<Point, int> ret,List<Actor> slow_melee_threat, HashSet<Actor> immediate_threat)
    {
      if (null == m_Actor) return;
      if (null == m_Actor.Location.Map) return;    // Duckman
      HashSet<Point> tmpFOV = FOV;  // virtual function call may be time-expensive so cache
      if (null == tmpFOV) return;
      Map map = m_Actor.Location.Map;
      foreach(Point tmp in tmpFOV) {
        if (tmp == m_Actor.Location.Position) continue;
        Actor a = map.GetActorAt(tmp);
        if (null == a) continue;
        if (!m_Actor.IsEnemyOf(a)) continue;
        int a_turns = m_Actor.HowManyTimesOtherActs(1,a);
        int a_turns_bak = a_turns;
        if (0 >= a_turns) continue; // morally if (!a.CanActNextTurn) continue;
        if (0==a.CurrentRangedAttack.Range && 1 == Rules.GridDistance(m_Actor.Location, a.Location) && m_Actor.Speed>a.Speed) slow_melee_threat.Add(a);
        // calculate melee damage field now
        Dictionary<Point,int> melee_damage_field = new Dictionary<Point,int>();
        int a_max_dam = a.MeleeAttack(m_Actor).DamageValue;
        foreach(Point pt in Direction.COMPASS.Select(dir=>a.Location.Position+dir).Where(pt=>map.IsValid(pt) && map.GetTileModelAtExt(pt).IsWalkable)) {
          melee_damage_field[pt] = a_turns*a_max_dam;
        }
        while(1<a_turns) {
          HashSet<Point> sweep = new HashSet<Point>(melee_damage_field.Keys);
          a_turns--;
          foreach(Point pt2 in sweep) {
            foreach(Point pt in Direction.COMPASS.Select(dir=>pt2+dir).Where(pt=>map.IsValid(pt) && map.GetTileModelAtExt(pt).IsWalkable && !sweep.Contains(pt))) {
              melee_damage_field[pt] = a_turns*a_max_dam;
            }
          }
        }
        if (melee_damage_field.ContainsKey(m_Actor.Location.Position)) {
          immediate_threat.Add(a);
        }
        // we can do melee attack damage field without FOV
        // FOV doesn't matter without a ranged attack
        // XXX doesn't handle non-optimal ranged attacks
        if (0 >= a.CurrentRangedAttack.Range) {
          foreach(var pt_dam in melee_damage_field) {
            if (ret.ContainsKey(pt_dam.Key)) ret[pt_dam.Key] += pt_dam.Value;
            else ret[pt_dam.Key] = pt_dam.Value;
          }
          continue;
        }

        // just directly recalculate FOV if needed, to avoid problems with newly spawned actors
        HashSet<Point> aFOV = LOS.ComputeFOVFor(a);
        // maximum melee damage: a.MeleeAttack(m_Actor).DamageValue
        // maximum ranged damage: a.CurrentRangedAttack.DamageValue
        Dictionary<Point,int> ranged_damage_field = new Dictionary<Point,int>();
        a_turns = a_turns_bak;
        foreach(Point pt in aFOV) {
          if (pt == a.Location.Position) continue;
          int dist = Rules.GridDistance(pt, a.Location.Position);
          a_max_dam = a.RangedAttack(dist, m_Actor).DamageValue;
          if (dist <= a.CurrentRangedAttack.Range) {
            ranged_damage_field[pt] = a_turns*a_max_dam;
          }
        }
        if (1<a_turns) {
          HashSet<Point> already = new HashSet<Point>();
          HashSet<Point> now = new HashSet<Point>{ a.Location.Position };
          do {
            a_turns--;
            HashSet<Point> tmp2 = a.NextStepRange(a.Location.Map,already,now);
            if (null == tmp2) break;
            foreach(Point pt2 in tmp2) {
              aFOV = LOS.ComputeFOVFor(a,new Location(a.Location.Map,pt2));
              aFOV.ExceptWith(ranged_damage_field.Keys);
              foreach(Point pt in aFOV) {
                int dist = Rules.GridDistance(pt, a.Location.Position);
                a_max_dam = a.RangedAttack(dist, m_Actor).DamageValue;
                if (dist <= a.CurrentRangedAttack.Range) {
                  ranged_damage_field[pt] = a_turns*a_max_dam;
                }
              }
            }
            already.UnionWith(now);
            now = tmp2;
          } while(1<a_turns);
        }
        if (ranged_damage_field.ContainsKey(m_Actor.Location.Position)) {
          immediate_threat.Add(a);
        }
        // ranged damage field should be a strict superset of melee in typical cases (exception: basement without flashlight)
        foreach(var pt_dam in ranged_damage_field) {
          if (melee_damage_field.ContainsKey(pt_dam.Key)) {
            if (ret.ContainsKey(pt_dam.Key)) ret[pt_dam.Key] += Math.Max(pt_dam.Value, melee_damage_field[pt_dam.Key]);
            else ret[pt_dam.Key] = Math.Max(pt_dam.Value, melee_damage_field[pt_dam.Key]);
          } else {
            if (ret.ContainsKey(pt_dam.Key)) ret[pt_dam.Key] += pt_dam.Value;
            else ret[pt_dam.Key] = pt_dam.Value;
          }
        }
      }
    }

    public bool AddExplosivesToDamageField(Dictionary<Point, int> damage_field, List<Percept_<Inventory>> goals)
    {
      if (null == goals) return false;
      bool in_blast_field = false;
      IEnumerable<Percept_<ItemPrimedExplosive>> explosives = goals.Select(p => new Percept_<ItemPrimedExplosive>((p.Percepted as Inventory).GetFirst<ItemPrimedExplosive>(), p.Turn, p.Location));
      foreach (Percept_<ItemPrimedExplosive> exp in explosives) {
        BlastAttack tmp_blast = exp.Percepted.Model.BlastAttack;
        Point pt = exp.Location.Position;
        if (damage_field.ContainsKey(pt)) damage_field[pt] += tmp_blast.Damage[0];
        else damage_field[pt] = tmp_blast.Damage[0];
        // We would need a very different implementation for large blast radii.
        int r = 0;
        while (++r <= tmp_blast.Radius) {
          foreach (Point p in Enumerable.Range(0, 8 * r).Select(i => exp.Location.Position.RadarSweep(r, i))) {
            if (!exp.Location.Map.IsValid(p)) continue;
            if (!LOS.CanTraceFireLine(exp.Location, p, tmp_blast.Radius)) continue;
            if (damage_field.ContainsKey(p)) damage_field[p] += tmp_blast.Damage[r];
            else damage_field[p] = tmp_blast.Damage[r];
            if (p == m_Actor.Location.Position) in_blast_field = true;
          }
        }
      }
      return in_blast_field;
    }

    protected bool AddExplosivesToDamageField(Dictionary<Point, int> damage_field, List<Percept> percepts)
    {
      return AddExplosivesToDamageField(damage_field, percepts.FilterCast<Inventory>(inv => inv.Has<ItemPrimedExplosive>()));
    }

    static protected void AddTrapsToDamageField(Dictionary<Point,int> damage_field, List<Percept> percepts)
    {
      List<Percept> goals = percepts.FilterT<Inventory>(inv => inv.Has<ItemTrap>());
      if (null == goals) return;
      foreach(Percept p in goals) {
        List<ItemTrap> tmp = (p.Percepted as Inventory).GetItemsByType<ItemTrap>();
        if (null == tmp) continue;
        int damage = tmp.Sum(trap => (trap.IsActivated ? trap.Model.Damage : 0));   // XXX wrong for barbed wire
        if (0 >= damage) continue;
        if (damage_field.ContainsKey(p.Location.Position)) damage_field[p.Location.Position] += damage;
        else damage_field[p.Location.Position] = damage;
      }
    }
#endregion

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
//    MarkItemAsTaboo(it,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);    // XXX can be called from simulation thread

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

    protected ActorAction BehaviorMakeRoomFor(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!m_Actor.Inventory.IsFull) throw new InvalidOperationException("already have room for items");
      // also should require IsInterestingItem(it), but that's infinite recursion for reasonable use cases
#endif
      Inventory inv = m_Actor.Inventory;
      if (it.Model.IsStackable && it.CanStackMore) {
         inv.GetItemsStackableWith(it, out int qty);
         if (qty>=it.Quantity) return null;
      } // no action if the whole stack completely fits

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

      // dropping body armor to get a better one should be ok
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null != armor && armor.Rating < (it as ItemBodyArmor).Rating) {
          return BehaviorDropItem(armor);
        }
      }

#if FAIL
      foreach(GameItems.IDs x in GameItems.medicine) {
        if (it.Model.ID == x) continue;
      }
#endif

      // trackers (mainly because AI can't use properly), but cell phones are trackers
      // XXX this is triggering a coverage failure; we need to be more sophisticated about trackers
      List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
      if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);
      if (it is ItemTracker) {
        if (!ok_trackers.Contains(it.Model.ID)) return null;   // tracker normally not worth clearing a slot for
      }
      // ditch an unwanted tracker if possible
      ItemTracker tmpTracker = inv.GetFirstMatching<ItemTracker>(it2 => !ok_trackers.Contains(it2.Model.ID));
      if (null != tmpTracker) return BehaviorDropItem(tmpTracker);

      // these lose to everything other than trackers.  Note that we should drop a light to get a more charged light -- if we're right on top of it.
      if (it is ItemSprayScent) return null;
      ItemSprayScent tmpSprayScent = inv.GetFirstMatching<ItemSprayScent>();
      if (null != tmpSprayScent) return BehaviorDropItem(tmpSprayScent);

      if (it is ItemBarricadeMaterial) return null;
      ItemBarricadeMaterial tmpBarricade = inv.GetFirstMatching<ItemBarricadeMaterial>();
      if (null != tmpBarricade) return BehaviorDropItem(tmpBarricade);

      if (it is ItemEntertainment) return null;
      ItemEntertainment tmpEntertainment = inv.GetFirstMatching<ItemEntertainment>();
      if (null != tmpEntertainment) return BehaviorDropItem(tmpEntertainment);

      if (it is ItemTrap) return null;
      ItemTrap tmpTrap = inv.GetFirstMatching<ItemTrap>();
      if (null != tmpTrap) return BehaviorDropItem(tmpTrap);

      if (it is ItemLight) return null;
      if (it is ItemMedicine) return null;

      // ditch unimportant items
      ItemMedicine tmpMedicine = inv.GetFirstMatching<ItemMedicine>();
      if (null != tmpMedicine) return BehaviorDropItem(tmpMedicine);

      // least charged flashlight goes
      List<ItemLight> lights = inv.GetItemsByType<ItemLight>();
      if (null != lights && 2<=lights.Count) {
        int min_batteries = lights.Select(obj => obj.Batteries).Min();
        ItemLight discard = lights.Find(obj => obj.Batteries==min_batteries);
        return BehaviorDropItem(discard);
      }

      // uninteresting ammo
      ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => !IsInterestingItem(ammo));
      if (null != tmpAmmo) {
        ItemRangedWeapon tmpRw = m_Actor.GetCompatibleRangedWeapon(tmpAmmo);
        if (null != tmpRw) {
          tmpAmmo = inv.GetBestDestackable(tmpAmmo) as ItemAmmo;
          if (m_Actor.CanUse(tmpAmmo)) return new ActionUseItem(m_Actor, tmpAmmo);
        }
        return BehaviorDropItem(tmpAmmo);
      }

      // ranged weapon with zero ammo is ok to drop for something other than its own ammo
      ItemRangedWeapon tmpRw2 = inv.GetFirstMatching<ItemRangedWeapon>(rw => 0 >= rw.Ammo);
      if (null != tmpRw2)
      {
         bool reloadable = (it is ItemAmmo ? (it as ItemAmmo).AmmoType==tmpRw2.AmmoType : false);
         if (!reloadable) return BehaviorDropItem(tmpRw2);
      }

      // grenades next
      if (it is ItemGrenade) return null;
      ItemGrenade tmpGrenade = inv.GetFirstMatching<ItemGrenade>();
      if (null != tmpGrenade) return BehaviorDropItem(tmpGrenade);

#if DEBUG
      // do not pick up trackers if it means dropping body armor or higher priority
      if (it is ItemTracker) return null;

      // body armor
      if (it is ItemBodyArmor) return null;

      throw new InvalidOperationException("coverage hole of types in BehaviorMakeRoomFor");
#else
      // give up
      return null;
#endif
    }

    public bool IsInterestingItem(ItemAmmo am)
    {
      if (m_Actor.GetCompatibleRangedWeapon(am) == null) return false;
      return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(am, 2);
    }
      
    public virtual bool IsInterestingItem(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!Actor.Model.Abilities.HasInventory) throw new InvalidOperationException("inventory required");   // CHAR guards: wander action can get item from containers
      if (!Actor.Model.Abilities.CanUseMapObjects) throw new InvalidOperationException("using map objects required");
#endif
	  if (it.IsForbiddenToAI) return false;
	  if (it is ItemSprayPaint) return false;
	  if (it is ItemTrap && (it as ItemTrap).IsActivated) return false;
      if (it.IsUseless || it is ItemPrimedExplosive || m_Actor.IsBoredOf(it)) return false;

      // only soldiers and civilians use grenades (CHAR guards are disallowed as a balance issue)
      if (GameItems.IDs.EXPLOSIVE_GRENADE == it.Model.ID && !(m_Actor.Controller is CivilianAI) && !(m_Actor.Controller is SoldierAI)) return false;

      // only civilians use stench killer
      if (GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.Model.ID && !(m_Actor.Controller is CivilianAI)) return false;

      // police have implicit police trackers
      if (GameItems.IDs.TRACKER_POLICE_RADIO == it.Model.ID && !!m_Actor.WantPoliceRadio) return false;
      if (GameItems.IDs.TRACKER_CELL_PHONE == it.Model.ID && !m_Actor.WantCellPhone) return false;

      // note that CHAR guards and soldiers don't need to eat like civilians, so they would not be interested in food
      if (it is ItemFood) {
        if (!m_Actor.Model.Abilities.HasToEat) return false;
        if (m_Actor.IsHungry) return true;
        if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
          return !(it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
        return false;
      }

      if (it is ItemRangedWeapon) {
        if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;
        if (1 <= m_Actor.CountItems< ItemRangedWeapon >()) return false;  // XXX rules out AI gun bunnies
        if (!m_Actor.Inventory.Contains(it) && m_Actor.HasItemOfModel(it.Model)) return false;
        ItemRangedWeapon rw = it as ItemRangedWeapon;
        return rw.Ammo > 0 || m_Actor.GetCompatibleAmmoItem(rw) != null;
      }
      if (it is ItemAmmo am) return IsInterestingItem(am);
      if (it is ItemMeleeWeapon) {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if (m_Actor.MeleeWeaponAttack(it.Model as ItemMeleeWeaponModel).Rating <= martial_arts.Rating) return false;

        int melee_count = m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon)); // XXX possibly obsolete
        if (2<= melee_count) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          return weapon.Model.Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        if (1<= melee_count && 1>= m_Actor.Inventory.MaxCapacity- m_Actor.Inventory.CountItems) {
          ItemMeleeWeapon weapon = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
          if (null == weapon) return true;  // martial arts invalidates starting baton for police
          return weapon.Model.Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        return true;
      }
      if (it is ItemMedicine) {
        // XXX easy to action-loop if inventory full
        // this plausibly should actually check inventory-clearing options
        if (0 >= m_Actor.Inventory.MaxCapacity - m_Actor.Inventory.CountItems) return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1);
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      }
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null == armor) return true;
        return armor.Rating < (it as ItemBodyArmor).Rating;
      }

      // No specific heuristic.
      if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1)) return false;
      if (!m_Actor.CanGet(it) && null == BehaviorMakeRoomFor(it)) return false; // we already have many useful items
      return true;
    }

    public virtual bool IsInterestingTradeItem(Actor speaker, Item offeredItem) // Cf. OrderableAI::IsRationalTradeItem
    {
#if DEBUG
      if (null == speaker) throw new ArgumentNullException(nameof(speaker));
      if (!speaker.Model.Abilities.CanTrade) throw new InvalidOperationException(nameof(speaker)+" must be able to trade");
      if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException(nameof(m_Actor)+" must be able to trade");
#endif
      if (RogueForm.Game.Rules.RollChance(Rules.ActorCharismaticTradeChance(speaker))) return true;
      return IsInterestingItem(offeredItem);
    }

    // cf ActorController::IsTradeableItem
    // this must prevent CivilianAI from
    // 1) bashing barricades, etc. for food when hungry
    // 2) trying to search for z at low ammo when there is ammo available
    public HashSet<GameItems.IDs> WhatDoINeedNow()
    {
      HashSet<GameItems.IDs> ret = new HashSet<GameItems.IDs>();

      if (/* m_Actor.Model.Abilities.HasToEat && */ m_Actor.IsHungry) {
        ret.UnionWith(GameItems.food);
      }

      if (!m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) {
        List<ItemRangedWeapon> tmp_rw = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>();
        List<ItemAmmo> tmp_ammo = m_Actor.Inventory.GetItemsByType<ItemAmmo>();
        if (null != tmp_rw) {
          foreach(ItemRangedWeapon rw in tmp_rw) {
            if (null == m_Actor.GetCompatibleAmmoItem(rw)) {
              ret.Add((GameItems.IDs)((int)rw.AmmoType + (int)GameItems.IDs.AMMO_LIGHT_PISTOL));    // Validity explicitly tested for in GameItems::CreateModels
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

    // If an item would be IsInterestingItem(it), its ID should be in this set if not handled by WhatDoINeedNow().
    // items flagged here should "be more interesting" than what we have
    public HashSet<GameItems.IDs> WhatDoIWantNow()
    {
      HashSet<GameItems.IDs> ret = new HashSet<GameItems.IDs>();

      if (/* m_Actor.Model.Abilities.HasToEat && */ !m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2)) {
        ret.UnionWith(GameItems.food);
      }
#if FAIL
      // needs substantial work...in particular, weapons w/o ammo need to have their own id values to be ignored here
      if (   !m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons
          && 0 >= m_Actor.CountItemsOfSameType(typeof(ItemRangedWeapon))) // XXX rules out AI gun bunnies
        {
        ret.UnionWith(GameItems.ranged);
        }
#endif

      foreach (GameItems.IDs am in GameItems.ammo) {
        if (m_Actor.GetCompatibleRangedWeapon(am) == null) continue;
        if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(am, 2)) continue;
        ret.Add(am);
      }

      { // scoping brace
      Attack martial_arts = m_Actor.UnarmedMeleeAttack();
      foreach(GameItems.IDs melee in GameItems.melee) {
        ItemMeleeWeaponModel model = Models.Items[(int)melee] as ItemMeleeWeaponModel;
        if (m_Actor.MeleeWeaponAttack(model).Rating <= martial_arts.Rating) continue;
        if (2<=m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon))) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          if (weapon.Model.Attack.Rating < model.Attack.Rating) ret.Add(melee);
          continue;
        }
        if (1<= m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon)) && 1>= m_Actor.Inventory.MaxCapacity- m_Actor.Inventory.CountItems) {
          ItemMeleeWeapon weapon = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
          if (null == weapon) {  // martial arts invalidates starting baton for police
            ret.Add(melee);
            continue;
          };
          if (weapon.Model.Attack.Rating < model.Attack.Rating) ret.Add(melee);
          continue;
        }
        ret.Add(melee);
      }
      } // end scoping brace

      ItemBodyArmor curr_armor = m_Actor.GetBestBodyArmor();
      if (null == curr_armor) {
        ret.UnionWith(GameItems.armor);
      } else {
        int curr_rating = curr_armor.Rating;
        foreach (GameItems.IDs armor in GameItems.armor) {
          if (curr_rating >= (Models.Items[(int)armor] as ItemBodyArmorModel).Rating) continue;
          ret.Add(armor);
        }
      }
#if FAIL
      if (it is ItemMedicine)
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
#endif
#if FAIL
      if (it.IsUseless || it is ItemPrimedExplosive || m_Actor.IsBoredOf(it))
        return false;
      return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1);
#endif
#if FAIL
	  if (it is ItemTrap && (it as ItemTrap).IsActivated) return false;

      // only soldiers and civilians use grenades (CHAR guards are disallowed as a balance issue)
      if (Gameplay.GameItems.IDs.EXPLOSIVE_GRENADE == it.Model.ID && !(m_Actor.Controller is Gameplay.AI.CivilianAI) && !(m_Actor.Controller is Gameplay.AI.SoldierAI)) return false;

      // only civilians use stench killer
      if (Gameplay.GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.Model.ID && !(m_Actor.Controller is Gameplay.AI.CivilianAI)) return false;
#endif
            return ret;
    }

    // XXX should also have concept of hoardable item (suitable for transporting to a safehouse)
  }
}
