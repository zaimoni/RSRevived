using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using ActionChain = djack.RogueSurvivor.Engine.Actions.ActionChain;
using ActionDropItem = djack.RogueSurvivor.Engine.Actions.ActionDropItem;
using ActionPutInContainer = djack.RogueSurvivor.Engine.Actions.ActionPutInContainer;
using ActionTake = djack.RogueSurvivor.Engine.Actions.ActionTake;
using ActionUseItem = djack.RogueSurvivor.Engine.Actions.ActionUseItem;
using ActionUse = djack.RogueSurvivor.Engine.Actions.ActionUse;

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
      if (null == dest) return new Dictionary<Point,int>();
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

    public void ExecuteActionChain(IEnumerable<ActorAction> actions)
    {
      int insertAt = -2;
      foreach(ActorAction action in actions) {
        insertAt++;
        if (0 > insertAt) {
          action.Perform();
          continue;
        }
        Objectives.Insert(insertAt,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, action));
       }
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

    public bool ItemIsUseless(Item it)
    {
	  if (it.IsForbiddenToAI) return true;
	  if (it is ItemSprayPaint) return true;
	  if (it is ItemTrap && (it as ItemTrap).IsActivated) return true;
      if (it.IsUseless || it is ItemPrimedExplosive || m_Actor.IsBoredOf(it)) return true;

      // only soldiers and civilians use grenades (CHAR guards are disallowed as a balance issue)
      if (GameItems.IDs.EXPLOSIVE_GRENADE == it.Model.ID && !(m_Actor.Controller is CivilianAI) && !(m_Actor.Controller is SoldierAI)) return true;

      // only civilians use stench killer
      if (GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.Model.ID && !(m_Actor.Controller is CivilianAI)) return true;

      // police have implicit police trackers
      if (GameItems.IDs.TRACKER_POLICE_RADIO == it.Model.ID && !m_Actor.WantPoliceRadio) return true;
      if (GameItems.IDs.TRACKER_CELL_PHONE == it.Model.ID && !m_Actor.WantCellPhone) return true;

      if (it is ItemFood && !m_Actor.Model.Abilities.HasToEat) return true; // Soldiers and CHAR guards.  There might be a serum for this.
      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) {    // Gangsters
        if (it is ItemRangedWeapon || it is ItemAmmo) return true;
      }

      return false;
    }

    protected ActorAction BehaviorDropUselessItem() // XXX would be convenient if this were fast-failing
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item it in m_Actor.Inventory.Items) {
        if (ItemIsUseless(it)) return BehaviorDropItem(it); // allows recovering cleanly from bugs and charismatic trades
      }

      // strict domination checks
      ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
      if (null != armor) return BehaviorDropItem(armor);

      ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
      if (null != weapon) {
        int martial_arts_rating = m_Actor.UnarmedMeleeAttack().Rating;
        int weapon_rating = m_Actor.MeleeWeaponAttack(weapon.Model).Rating;
        if (weapon_rating <= martial_arts_rating) return BehaviorDropItem(weapon);
      }

      ItemRangedWeapon rw = m_Actor.Inventory.GetFirstMatching<ItemRangedWeapon>(it => 0==it.Ammo && 2<=m_Actor.Count(it.Model));
      if (null != rw) return BehaviorDropItem(rw);

      if (m_Actor.Inventory.MaxCapacity-5 <= m_Actor.Inventory.CountType<ItemAmmo>()) {
        if (0 < m_Actor.Inventory.CountType<ItemRangedWeapon>()) {
          ItemAmmo am = m_Actor.Inventory.GetFirstMatching<ItemAmmo>(it => null == m_Actor.GetCompatibleRangedWeapon(it));
          if (null != am) return BehaviorDropItem(am);
        }
      }

      return null;
    }

    protected ActorAction BehaviorMakeRoomFor(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!m_Actor.Inventory.IsFull) throw new InvalidOperationException("already have room for "+it.ToString());
      if (m_Actor.CanGet(it)) throw new InvalidOperationException("already could get "+it.ToString());
      // also should require IsInterestingItem(it), but that's infinite recursion for reasonable use cases
#endif
      Inventory inv = m_Actor.Inventory;

      // not-best body armor can be dropped
      if (2<=m_Actor.CountQuantityOf<ItemBodyArmor>()) {
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
      {
      if (GameItems.IDs.FOOD_CANNED_FOOD == it.Model.ID && m_Actor.Model.Abilities.HasToEat && inv.GetBestDestackable(it) is ItemFood food) {
        // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
        int need = m_Actor.MaxFood - m_Actor.FoodPoints;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4 <= need && m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
      }
      }
      { // it should be ok to devour stimulants in a glut
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID && inv.GetBestDestackable(it) is ItemMedicine stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4 <= need && m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
      }
      }

      { // see if we can eat our way to a free slot
      if (m_Actor.Model.Abilities.HasToEat && inv.GetBestDestackable(GameItems.CANNED_FOOD) is ItemFood food) {
        // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
        int need = m_Actor.MaxFood - m_Actor.FoodPoints;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4*food.Quantity <= need && m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
      }
      }

      { // finisbing off stimulants to get a free slot is ok
      if (inv.GetBestDestackable(GameItems.PILLS_SLP) is ItemMedicine stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4*stim.Quantity <= need && m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
      }
      }

      if (it is ItemAmmo am) {
        ItemRangedWeapon rw = m_Actor.GetCompatibleRangedWeapon(am);
        if (null != rw && rw.Ammo < rw.Model.MaxAmmo) {
          // we really do need to reload this.
          // 1) we would re-pickup food.
          // 2) it would not be a big deal if something less important than ammo were not re-picked up.
          Item drop = inv.GetFirst<ItemFood>();
          if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
          if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.MEDIKIT);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.BANDAGE);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
          if (null == drop) drop = inv.GetFirst<ItemGrenade>();
          if (null == drop) drop = inv.GetFirst<ItemAmmo>();
          if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
          if (null != drop) {
            List<ActorAction> recover = new List<ActorAction>(3);
            // 3a) drop target without triggering the no-pickup schema
            recover.Add(new ActionDropItem(m_Actor,drop));
            // 3b) pick up ammo
            recover.Add(new ActionTake(m_Actor,it.Model.ID));
            // 3c) use ammo just picked up : arguably ActionUseItem; use ActionUse(Actor actor, Gameplay.GameItems.IDs it)
            recover.Add(new ActionUse(m_Actor, it.Model.ID));
            return new ActionChain(m_Actor,recover);
          }
        }
      }

      {
      int needHP = m_Actor.MaxHPs- m_Actor.HitPoints;
      if (0 < needHP) {
        if (   (GameItems.MEDIKIT == it.Model && needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.MEDIKIT.Healing))
            || (GameItems.BANDAGE == it.Model && needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.BANDAGE.Healing)))
          { // same idea as reloading, only hp instead of ammo
          Item drop = inv.GetFirst<ItemFood>();
          if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
          if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
          if (null == drop) drop = inv.GetFirst<ItemGrenade>();
          if (null == drop) drop = inv.GetFirst<ItemAmmo>();
          if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
          if (null != drop) {
            List<ActorAction> recover = new List<ActorAction>(3);
            // 3a) drop target without triggering the no-pickup schema
            recover.Add(new ActionDropItem(m_Actor,drop));
            // 3b) pick up ammo
            recover.Add(new ActionTake(m_Actor,it.Model.ID));
            // 3c) use ammo just picked up : arguably ActionUseItem; use ActionUse(Actor actor, Gameplay.GameItems.IDs it)
            recover.Add(new ActionUse(m_Actor, it.Model.ID));
            return new ActionChain(m_Actor,recover);
          }
          }
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

      // dropping body armor to get a better one should be ok
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null != armor && armor.Rating < (it as ItemBodyArmor).Rating) {
          return BehaviorDropItem(armor);
        }
      }

      // medicine glut ... drop it
      foreach(GameItems.IDs x in GameItems.medicine) {
        if (it.Model.ID == x) continue;
        ItemModel model = Models.Items[(int)x];
        if (2>m_Actor.Count(model)) continue;
        Item tmp = m_Actor.Inventory.GetBestDestackable(model);
        if (null != tmp) return BehaviorDropItem(tmp);
      }

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

      // ahem...food glut, of all things
      ItemFood tmpFood = inv.GetFirst<ItemFood>(f => !IsInterestingItem(f) && f.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter));
      if (null != tmpFood) return BehaviorDropItem(tmpFood);
      tmpFood = inv.GetFirst<ItemFood>(f => !IsInterestingItem(f) && f.IsExpiredAt(m_Actor.Location.Map.LocalTime.TurnCounter));
      if (null != tmpFood) return BehaviorDropItem(tmpFood);
      tmpFood = inv.GetFirst<ItemFood>(f => !IsInterestingItem(f));
      if (null != tmpFood) return BehaviorDropItem(tmpFood);

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
      ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => null == m_Actor.GetCompatibleRangedWeapon(ammo));  // not quite the full check here.  Problematic if no ranged weapons at all.
//    ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => !IsInterestingItem(ammo));  // full check, triggers infinite recursion
      if (null != tmpAmmo) {
        ItemRangedWeapon tmpRw = m_Actor.GetCompatibleRangedWeapon(tmpAmmo);
        if (null != tmpRw && tmpRw.Ammo >= tmpRw.Model.MaxAmmo) { // inline a key part of the CanUse check here; stack overflow otherwise
          tmpAmmo = inv.GetBestDestackable(tmpAmmo) as ItemAmmo;
          if (null != tmpAmmo) return new ActionUseItem(m_Actor, tmpAmmo);
        }
        return BehaviorDropItem(tmpAmmo);
      }

      // ranged weapon with zero ammo is ok to drop for something other than its own ammo
      ItemRangedWeapon tmpRw2 = inv.GetFirstMatching<ItemRangedWeapon>(rw => 0 >= rw.Ammo);
      if (null != tmpRw2) {
         bool reloadable = (it is ItemAmmo ? (it as ItemAmmo).AmmoType==tmpRw2.AmmoType : false);
         if (!reloadable) return BehaviorDropItem(tmpRw2);
      }


      // if we have 2 clips of an ammo type, trading one for a melee weapon or food is ok
      if (it is ItemMeleeWeapon || it is ItemFood) {
        foreach(GameItems.IDs x in GameItems.ammo) {
          ItemModel model = Models.Items[(int)x];
          if (2<=m_Actor.Count(model)) {
            ItemAmmo ammo = inv.GetBestDestackable(model) as ItemAmmo;
            if (m_Actor.CanUse(ammo)) return new ActionUseItem(m_Actor, ammo);    // completeness; should not trigger due to above
            return BehaviorDropItem(ammo);
          }
        }
        // if we have two clips of any type, trading the smaller one for a melee weapon or food is ok
        ItemAmmo test = null;
        foreach(GameItems.IDs x in GameItems.ammo) {
          ItemAmmo ammo = inv.GetBestDestackable(Models.Items[(int)x]) as ItemAmmo;
          if (null != ammo) {
             if (null == test || test.Quantity>ammo.Quantity) test = ammo;
          }
        }
        if (null != test) {
            if (m_Actor.CanUse(test)) return new ActionUseItem(m_Actor, test);    // completeness; should not trigger due to above
            return BehaviorDropItem(test);
        }
      }

      // if inventory is full and the problem is ammo at this point, ignore if we already have a full clip
      if (it is ItemAmmo && 1<=m_Actor.Count(it.Model)) return null;

      // if inventory is full and the problem is ranged weapon at this point, ignore if we already have one
      if (it is ItemRangedWeapon && 1<= inv.CountType<ItemRangedWeapon>()) return null;

      // grenades next
      if (it is ItemGrenade) return null;
      ItemGrenade tmpGrenade = inv.GetFirstMatching<ItemGrenade>();
      if (null != tmpGrenade) return BehaviorDropItem(tmpGrenade);

      // important trackers go for ammo
      if (it is ItemAmmo) { 
        ItemTracker discardTracker = inv.GetFirstMatching<ItemTracker>();
        if (null != discardTracker) return BehaviorDropItem(discardTracker);
      }

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

    private bool _InterestingItemPostprocess(Item it)
    {
      if (!m_Actor.CanGet(it)) {
        if (m_Actor.Inventory.IsFull) return null != BehaviorMakeRoomFor(it);
        return false;
      }
      return true;
    }

    public bool IsInterestingItem(ItemRangedWeapon rw)
    {
      if (!m_Actor.Inventory.Contains(rw)) {
        if (null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, it => 0 < it.Ammo)) return false;    // XXX
        if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(it => it.AmmoType==rw.AmmoType)) return false; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
        if (0 < rw.Ammo && null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, it => 0 == it.Ammo)) return true;  // this replacement is ok; implies not having ammo
      }
      // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
      // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
      if (m_Actor.Inventory.MaxCapacity-5 <= m_Actor.Inventory.CountType<ItemRangedWeapon>(it => 0 < it.Ammo)) return false;
      if (m_Actor.Inventory.MaxCapacity-4 <= m_Actor.Inventory.CountType<ItemRangedWeapon>(it => 0 < it.Ammo)+ m_Actor.Inventory.CountType<ItemAmmo>()) return false;
      if (0 >= rw.Ammo && null == m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return false;
      return _InterestingItemPostprocess(rw);
    }

    private bool AmmoAtLimit {
      get {
        // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
        // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
        int limit = m_Actor.Inventory.MaxCapacity;
        if (0< m_Actor.Inventory.CountType<ItemBodyArmor>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemLight>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemFood>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemExplosive>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemMeleeWeapon>()) limit--;

        if (limit <= m_Actor.Inventory.CountType<ItemAmmo>()) return true;
        if (limit <= m_Actor.Inventory.CountType<ItemRangedWeapon>(it => 0 < it.Ammo)+ m_Actor.Inventory.CountType<ItemAmmo>()) return true;
        return false;
      }
    }
      
    public bool IsInterestingItem(ItemAmmo am)
    {
      if (m_Actor.GetCompatibleRangedWeapon(am) == null) {
        if (0 < m_Actor.Inventory.CountType<ItemRangedWeapon>()) return false;  // XXX
        if (0 < m_Actor.Inventory.Count(am.Model)) return false;    // only need one clip to prime AI to look for empty ranged weapons
      } else {
        if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(am, 2)) return false;
        if (null != m_Actor.Inventory.GetFirstByModel<ItemAmmo>(am.Model, it => it.Quantity < it.Model.MaxQuantity)) return true;   // topping off clip is ok
      }
      if (AmmoAtLimit) return false;
      return _InterestingItemPostprocess(am);
    }

    public bool IsInterestingItem(ItemFood food)
    {
      if (m_Actor.Inventory.Contains(food)) return !m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2 + food.Nutrition*food.Quantity);
      return !m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2);
    }

    public bool IsInterestingItem(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!Actor.Model.Abilities.HasInventory) throw new InvalidOperationException("inventory required");   // CHAR guards: wander action can get item from containers
      if (!Actor.Model.Abilities.CanUseMapObjects) throw new InvalidOperationException("using map objects required");
#endif
      if (ItemIsUseless(it)) return false;

      // note that CHAR guards and soldiers don't need to eat like civilians, so they would not be interested in food
      if (it is ItemFood) {
//      if (!m_Actor.Model.Abilities.HasToEat) return false;    // redundant; for documentation
        if (m_Actor.IsHungry) return true;
        if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
          return !(it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
        return false;
      }

      if (it is ItemRangedWeapon rw) {
//      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;    // redundant; for documentation
        return IsInterestingItem(rw);
      }
      if (it is ItemAmmo am) {
//      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;    // redundant; for documentation
        return IsInterestingItem(am);
      }
      if (it is ItemMeleeWeapon) {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if (m_Actor.MeleeWeaponAttack(it.Model as ItemMeleeWeaponModel).Rating <= martial_arts.Rating) return false;

        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
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
        if (!m_Actor.Inventory.IsFull) return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      }
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null == armor) return true;
        return armor.Rating < (it as ItemBodyArmor).Rating; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
      }

      // No specific heuristic.
      if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1)) return false;
      return _InterestingItemPostprocess(it);
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
        if (null != tmp_rw && !AmmoAtLimit) {
          foreach(ItemRangedWeapon rw in tmp_rw) {
            if (null == m_Actor.Inventory.GetCompatibleAmmoItem(rw)) {
              if (rw.Ammo < rw.Model.MaxAmmo || !AmmoAtLimit) {
                ret.Add((GameItems.IDs)((int)rw.AmmoType + (int)GameItems.IDs.AMMO_LIGHT_PISTOL));    // Validity explicitly tested for in GameItems::CreateModels
              }
            }
          }
#if FAIL
        // XXX need to fix AI to be gun bunny capable
        } else if (null != tmp_ammo) {
#endif
        }
      }

      int needHP = m_Actor.MaxHPs- m_Actor.HitPoints;
      if (needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.MEDIKIT.Healing)) {
        // We need second aid.
        ret.Add(GameItems.IDs.MEDICINE_MEDIKIT);
        ret.Add(GameItems.IDs.MEDICINE_BANDAGES);
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

      if (!AmmoAtLimit) {
        foreach (GameItems.IDs am in GameItems.ammo) {
          ItemRangedWeapon rw = m_Actor.GetCompatibleRangedWeapon(am);
          if (null == rw) continue;
          if (m_Actor.HasAtLeastFullStackOf(am, 2)) continue;
          if (rw.Ammo < rw.Model.MaxAmmo || !AmmoAtLimit) ret.Add(am);
        }
      }

      int needHP = m_Actor.MaxHPs- m_Actor.HitPoints;
      if (needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.BANDAGE.Healing)) {
        // We need first aid.
        ret.Add(GameItems.IDs.MEDICINE_BANDAGES);
      }

      { // scoping brace
      Attack martial_arts = m_Actor.UnarmedMeleeAttack();
      foreach(GameItems.IDs melee in GameItems.melee) {
        ItemMeleeWeaponModel model = Models.Items[(int)melee] as ItemMeleeWeaponModel;
        if (m_Actor.MeleeWeaponAttack(model).Rating <= martial_arts.Rating) continue;
        if (2<=m_Actor.CountQuantityOf<ItemMeleeWeapon>()) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          if (weapon.Model.Attack.Rating < model.Attack.Rating) ret.Add(melee);
          continue;
        }
        if (1<= m_Actor.CountQuantityOf<ItemMeleeWeapon>() && 1>= m_Actor.Inventory.MaxCapacity- m_Actor.Inventory.CountItems) {
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
