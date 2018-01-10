#define INTEGRITY_CHECK_ITEM_RETURN_CODE

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
using ActionRechargeItemBattery = djack.RogueSurvivor.Engine.Actions.ActionRechargeItemBattery;
using ActionSwitchPowerGenerator = djack.RogueSurvivor.Engine.Actions.ActionSwitchPowerGenerator;
using ActionTake = djack.RogueSurvivor.Engine.Actions.ActionTake;
using ActionUseItem = djack.RogueSurvivor.Engine.Actions.ActionUseItem;
using ActionUse = djack.RogueSurvivor.Engine.Actions.ActionUse;
using ActionTradeWithContainer = djack.RogueSurvivor.Engine.Actions.ActionTradeWithContainer;
using ActionWait = djack.RogueSurvivor.Engine.Actions.ActionWait;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class Goal_RestRatherThanLoseturnWhenTired : Objective
  {
    public Goal_RestRatherThanLoseturnWhenTired(int t0, Actor who)
    : base(t0,who)
    {
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (!m_Actor.IsTired) {
        _isExpired = true;
        return true;
      }
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) return false;
      if (m_Actor.CanActNextTurn) return false;
      ret = new ActionWait(m_Actor);
      return true;
    }
  }

  [Serializable]
  internal class Goal_RecoverSTA : Objective
  {
    public readonly int targetSTA;

    public Goal_RecoverSTA(int t0, Actor who, int target)
    : base(t0,who)
    {
      targetSTA = target;
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
        _isExpired = true;
        return true;
      }
      ret = (m_Actor.Controller as ObjectiveAI).DoctrineRecoverSTA(targetSTA);
      if (null == ret) _isExpired = true;
      return true;
    }
  }

  [Serializable]
  internal class Goal_MedicateSLP : Objective
  {
    public Goal_MedicateSLP(int t0, Actor who)
    : base(t0,who)
    {
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
        _isExpired = true;
        return true;
      }
      ret = (m_Actor.Controller as ObjectiveAI).DoctrineMedicateSLP();
      if (null == ret) _isExpired = true;
      return true;
    }
  }

  [Serializable]
  internal class Goal_RechargeAll : Objective
  {
    public Goal_RechargeAll(int t0, Actor who)
    : base(t0,who)
    {
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
        _isExpired = true;
        return true;
      }
      {
      var lights = m_Actor?.Inventory.GetItemsByType<ItemLight>();
      if (0 < (lights?.Count ?? 0)) {
        foreach(var x in lights) {
          ret = (m_Actor.Controller as ObjectiveAI)?.DoctrineRechargeToFull(x);
          if (null != ret) return true;
        }
      }
      }
      {
      var trackers = m_Actor?.Inventory.GetItemsByType<ItemTracker>(it => GameItems.IDs.TRACKER_POLICE_RADIO!=it.Model.ID);
      if (0 < (trackers?.Count ?? 0)) {
        foreach(var x in trackers) {
          ret = (m_Actor.Controller as ObjectiveAI)?.DoctrineRechargeToFull(x);
          if (null != ret) return true;
        }
      }
      }

      _isExpired = true;
      return true;
    }
  }

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

    // navigate.Approach(pt) only checks for points in bounds.  For the extended maps we also need to be exit-aware
    protected Dictionary<Point, int> SimulateApproach(Zaimoni.Data.FloodfillPathfinder<Point> navigate)
    {
      List<Point> legal_steps = m_Actor.OnePathRange(m_Actor.Location.Map,m_Actor.Location.Position);
      var test = new Dictionary<Point,int>(8);
      if (null != legal_steps) {
        int current_cost = navigate.Cost(m_Actor.Location.Position);
        foreach(Point pt in legal_steps) {
          int new_cost = navigate.Cost(pt);
          if (new_cost >= current_cost) continue;
          test[pt] = new_cost;
        }
      }
      return test;
    }

    // these two return a value copy for correctness
    protected Dictionary<Point, int> PlanApproach(Zaimoni.Data.FloodfillPathfinder<Point> navigate)
    {
      PlannedMoves.Clear();
      Dictionary<Point, int> dest = SimulateApproach(navigate);
      if (0 >= dest.Count) return dest;
      PlannedMoves[m_Actor.Location.Position] = dest;
      foreach(Point pt in dest.Keys) {
        if (0>navigate.Cost(pt)) continue;
        PlannedMoves[pt] = navigate.Approach(pt);
      }
      return new Dictionary<Point,int>(PlannedMoves[m_Actor.Location.Position]);
    }

    private Dictionary<Point, int> DowngradeApproach(Dictionary<Location,int> src)
    {
      var ret = new Dictionary<Point,int>();
      foreach(var x in src) {
        Location? test = x.Key.Map.Denormalize(x.Key);
        if (null == test) continue;
        ret[test.Value.Position] = x.Value;
      }
      return ret;
    }

    protected Dictionary<Location, int> PlanApproach(Zaimoni.Data.FloodfillPathfinder<Location> navigate)
    {
      PlannedMoves.Clear();
      var dest = navigate.Approach(m_Actor.Location);
      if (0 < dest.Count) {
        var approach = DowngradeApproach(dest);
        if (0<approach.Count) PlannedMoves[m_Actor.Location.Position] = approach;
        foreach(var x in dest) {
          Location? test = x.Key.Map.Denormalize(x.Key);
          if (null == test) continue;
          var test2 = navigate.Approach(x.Key);
          approach = DowngradeApproach(test2);
          if (0<approach.Count) PlannedMoves[test.Value.Position] = approach;
        }
      }
      return dest;
    }

    protected ActorAction PlanApproachFailover(Zaimoni.Data.FloodfillPathfinder<Point> navigate)
    {
      List<Point> legal_steps = m_Actor.OnePathRange(m_Actor.Location.Map,m_Actor.Location.Position);
      if (null != legal_steps) {
        var costs = new Dictionary<Point,int>();
        foreach(Point pt in legal_steps) {
          costs[pt] = navigate.Cost(pt);
        }
        int min_cost = costs.Values.Min();
        if (int.MaxValue == min_cost) return null;
        costs.OnlyIf(val => val <= min_cost);
        if (0<costs.Count) {
          var dests = costs.Keys.ToList();
          return Rules.IsPathableFor(m_Actor,new Location(m_Actor.Location.Map,dests[RogueForm.Game.Rules.Roll(0,dests.Count)]));
        }
      }
      return null;
    }

    protected ActorAction PlanApproachFailover(Zaimoni.Data.FloodfillPathfinder<Location> navigate)
    {
      Dictionary<Location,ActorAction> legal_steps = m_Actor.OnePathRange(m_Actor.Location);
      if (null != legal_steps) {
        var costs = new Dictionary<Location,int>();
        foreach(var loc_action in legal_steps) {
          costs[loc_action.Key] = navigate.Cost(loc_action.Key);
        }
        int min_cost = costs.Values.Min();
        if (int.MaxValue == min_cost) return null;
        costs.OnlyIf(val => val <= min_cost);
        if (0<costs.Count) {
          var dests = costs.Keys.ToList();
          return legal_steps[RogueForm.Game.Rules.Choose(dests)];
        }
      }
      return null;
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
      var enemies = m_Actor.Controller.enemies_in_FOV;
      if (null == enemies) return;
      Map map = m_Actor.Location.Map;
      foreach(var where_enemy in enemies) {
        Actor a = where_enemy.Value;
        int a_turns = m_Actor.HowManyTimesOtherActs(1,a);
        int a_turns_bak = a_turns;
        if (0 >= a_turns) continue; // morally if (!a.CanActNextTurn) continue;
        if (0==a.CurrentRangedAttack.Range && 1 == Rules.GridDistance(m_Actor.Location.Position, where_enemy.Key) && m_Actor.Speed>a.Speed) slow_melee_threat.Add(a);
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

    private ActorAction _PrefilterDrop(Item it)
    {
      // use stimulants before dropping them
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        if (m_Actor.Inventory.GetBestDestackable(it) is ItemMedicine stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
          if (num4 <= need &&  m_Actor.CanUse(stim2)) return new ActionUseItem(m_Actor, stim2);
        }
      }

      // reload weapons before dropping ammo
      { // scoping brace
      if (it is ItemAmmo ammo) {
        foreach(Item obj in m_Actor.Inventory.Items) {
          if (   obj is ItemRangedWeapon rw 
              && rw.AmmoType==ammo.AmmoType 
              && rw.Ammo < rw.Model.MaxAmmo) {
            RogueForm.Game.DoEquipItem(m_Actor,rw);
            return new ActionUseItem(m_Actor, ammo);
          }
        }
      }
      } // end scoping brace
      return null;
    }

    protected ActorAction BehaviorDropItem(Item it)
    {
      if (it == null) return null;
      ActorAction tmp = _PrefilterDrop(it);
      if (null != tmp) return tmp;

      // use stimulants before dropping them
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        if (m_Actor.Inventory.GetBestDestackable(it) is ItemMedicine stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
          if (num4 <= need &&  m_Actor.CanUse(stim2)) return new ActionUseItem(m_Actor, stim2);
        }
      }

      // reload weapons before dropping ammo
      { // scoping brace
      if (it is ItemAmmo ammo) {
        foreach(Item obj in m_Actor.Inventory.Items) {
          if (   obj is ItemRangedWeapon rw 
              && rw.AmmoType==ammo.AmmoType 
              && rw.Ammo < rw.Model.MaxAmmo) {
            RogueForm.Game.DoEquipItem(m_Actor,rw);
            return new ActionUseItem(m_Actor, ammo);
          }
        }
      }
      } // end scoping brace


      if (m_Actor.CanUnequip(it)) RogueForm.Game.DoUnequipItem(m_Actor,it);

      List<Point> has_container = new List<Point>();
      foreach(Point pos in Direction.COMPASS.Select(dir => m_Actor.Location.Position+dir)) {
        MapObject container = m_Actor.Location.Map.GetMapObjectAtExt(pos);
        if (!container?.IsContainer ?? true) continue;
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

    // Would be the lowest priority level of an item, except that it conflates "useless to everyone" and "useless to me"
    public bool ItemIsUseless(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
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
      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) {    // Bikers
        if (it is ItemRangedWeapon || it is ItemAmmo) return true;
      }

      return false;
    }

    // XXX should be an enumeration
    // 0: useless
    // 1: insurance
    // 2: want
    // 3: need
    protected int ItemRatingCode(Item it, Location? loc=null)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (ItemIsUseless(it)) return 0;
      bool is_in_inventory = m_Actor.Inventory.Contains(it);

      {
      if (it is ItemTracker tracker) {
        List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
        if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
        if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);
        // AI does not yet use z-trackers or blackops trackers correctly; possible only threat-aware AIs use them
        if (is_in_inventory) return (ok_trackers.Contains(it.Model.ID) && null!=m_Actor.LiveLeader) ? 2 : 1;
        if (m_Actor.Inventory.Items.Any(obj => !obj.IsUseless && obj.Model == it.Model)) return 0;
        return (ok_trackers.Contains(it.Model.ID) && null != m_Actor.LiveLeader) ? 2 : 1;
      }

      if (it is ItemBarricadeMaterial) return 1;
      if (it is ItemTrap) return 1;
      if (it is ItemEntertainment) {
        if (!m_Actor.Model.Abilities.HasSanity) return 0;
        if (m_Actor.IsDisturbed) return 3;
        if (m_Actor.Sanity < 3 * m_Actor.MaxSanity / 4) return 2;   // gateway expression for using entertainment
        return 1;
      }
      {
      if (it is ItemLight light) {
        if (is_in_inventory) return 2;
        if (m_Actor.Inventory.Items.Any(obj => !obj.IsUseless && obj is ItemLight)) return 0;
        return 2;   // historically low priority but ideal kit has one
      }
      }
      // XXX note that sleep and stamina have special uses for sufficiently good AI
      if (it is ItemMedicine) {
        if (is_in_inventory) return 1;
        if (m_Actor.HasAtLeastFullStackOf(it, m_Actor.Inventory.IsFull ? 1 : 2)) return 0;
        return 1;
      }
      {
      if (it is ItemBodyArmor armor) {
        ItemBodyArmor best = m_Actor.GetBestBodyArmor();
        if (null == best) return 2; // want 3, but RHSMoreInteresting that says 2
        if (best == armor) return 3;
        return best.Rating < armor.Rating ? 2 : 0; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
      }
      }
      {
      if (it is ItemMeleeWeapon melee) {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if (m_Actor.MeleeWeaponAttack(melee.Model).Rating <= martial_arts.Rating) return 0;
        ItemMeleeWeapon best = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
        if (null == best) return 2;  // martial arts invalidates starting baton for police
        if (best.Model.Attack.Rating < melee.Model.Attack.Rating) return 2;
        if (best.Model.Attack.Rating > melee.Model.Attack.Rating) return 1;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
        if (is_in_inventory) return 1 == melee_count ? 2 : 1;
        if (2 <= melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return worst.Model.Attack.Rating < melee.Model.Attack.Rating ? 1 : 0;
        }
        return 1;
      }
      }
      {
      if (it is ItemFood food) {
//      if (!m_Actor.Model.Abilities.HasToEat) return false;    // redundant; for documentation
        if (m_Actor.IsHungry) return 3;
        if (food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter)) return 0;
        // XXX if the preemptive eat behavior would trigger, that is 3
        if (m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food)) return 1;
        return 2;
      }
      }

      { // similar to IsInterestingItem(ItemAmmo)
      if (it is ItemAmmo am) {
        ItemRangedWeapon rw = m_Actor.GetCompatibleRangedWeapon(am);
        if (null == rw) {
          if (is_in_inventory) return 1;
          return 0 < m_Actor.Inventory.Count(am.Model) ? 0 : 1;
        }
        if (is_in_inventory) return 2;
        if (rw.Ammo < rw.Model.MaxAmmo) return 2;
        if (m_Actor.HasAtLeastFullStackOf(am, 2)) return 0;
        if (null != m_Actor.Inventory.GetFirstByModel<ItemAmmo>(am.Model,am2=>am.Quantity<am.Model.MaxQuantity)) return 2;
        if (AmmoAtLimit) return 0;
        return 2;
      }
      }
      { // similar to IsInterestingItem(rw)
      if (it is ItemRangedWeapon rw) {
        if (is_in_inventory) return 0<rw.Ammo ? 3 : 1;
        if (null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, obj => 0 < obj.Ammo)) return 0;    // XXX
        if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(obj => obj.AmmoType==rw.AmmoType)) return 0; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
        if (0 < rw.Ammo && null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, obj => 0 == obj.Ammo)) return 3;  // this replacement is ok; implies not having ammo
        if (0 >= m_Actor.Inventory.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo) && null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return 3;
        // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
        // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
        if (m_Actor.Inventory.MaxCapacity-5 <= m_Actor.Inventory.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo)) return 0;
        if (m_Actor.Inventory.MaxCapacity-4 <= m_Actor.Inventory.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo)+ m_Actor.Inventory.CountType<ItemAmmo>()) return 0;
        if (0 >= rw.Ammo && null == m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return 0;
        if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(obj => 0<obj.Ammo)) return 2;
        return 3;
      }
      }
      {
      if (it is ItemGrenade grenade) {
        if (is_in_inventory) return 2;
        if (m_Actor.Inventory.IsFull) return 1;
        if (m_Actor.HasAtLeastFullStackOf(grenade, 1)) return 1;
        return 2;
      }
      }

      return 1;
    }
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

    private ActorAction _BehaviorDropOrExchange(Item give, Item take, Point? position)
    {
      ActorAction tmp = _PrefilterDrop(give);
      if (null != tmp) return tmp;
      if (null != position) return new ActionTradeWithContainer(m_Actor,give,take,position.Value);
      return BehaviorDropItem(give);
    }

    protected bool RHSMoreInteresting(Item lhs, Item rhs)
    {
#if DEBUG
      if (null == lhs) throw new ArgumentNullException(nameof(lhs));
      if (null == rhs) throw new ArgumentNullException(nameof(rhs));
      if (!m_Actor.Inventory.Contains(lhs) && !IsInterestingItem(lhs)) throw new InvalidOperationException(lhs.ToString()+" not interesting to "+m_Actor.Name);
      if (!m_Actor.Inventory.Contains(rhs) && !IsInterestingItem(rhs)) throw new InvalidOperationException(rhs.ToString()+" not interesting to "+m_Actor.Name);
#endif
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

      // Top-level prescreen.  Resolves following priority issues in smoke testing
      // * food/ranged weapons/ammo
      // * melee weapons/armor
      // * trackers, lights, and entertainment
      int lhs_code = ItemRatingCode(lhs);
      int rhs_code = ItemRatingCode(rhs);
      if (lhs_code>rhs_code) return false;
      if (lhs_code<rhs_code) return true;

      // if food is interesting, it will dominate non-food
      if (rhs is ItemFood) return !(lhs is ItemFood);
      else if (lhs is ItemFood) return false;

      // ranged weapons
      if (rhs is ItemRangedWeapon) return !(lhs is ItemRangedWeapon);
      else if (lhs is ItemRangedWeapon) return false;

      if (rhs is ItemAmmo) return !(lhs is ItemAmmo);
      else if (lhs is ItemAmmo) return false;

      if (rhs is ItemMeleeWeapon rhs_melee)
        {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if (m_Actor.MeleeWeaponAttack(rhs_melee.Model).Rating <= martial_arts.Rating) return false;
        ItemMeleeWeapon best = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
        if (null == best) return true;
        if (best.Model.Attack.Rating < rhs_melee.Model.Attack.Rating) return true;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
        if (2<=melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return worst.Model.Attack.Rating < rhs_melee.Model.Attack.Rating;
        }
        if (lhs is ItemMeleeWeapon lhs_melee) return lhs_melee.Model.Attack.Rating < rhs_melee.Model.Attack.Rating;
        return false;
        }
      else if (lhs is ItemMeleeWeapon lhs_melee) {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if (m_Actor.MeleeWeaponAttack(lhs_melee.Model).Rating <= martial_arts.Rating) return true;
        ItemMeleeWeapon best = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
        if (null == best) return false;
        if (best.Model.Attack.Rating < lhs_melee.Model.Attack.Rating) return false;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
        if (2<=melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return worst.Model.Attack.Rating >= lhs_melee.Model.Attack.Rating;
        }
        return true;
      }

      if (rhs is ItemBodyArmor)
        {
        if (!(lhs is ItemBodyArmor)) return false;
        return (lhs as ItemBodyArmor).Rating < (rhs as ItemBodyArmor).Rating;
        }
      else if (lhs is ItemBodyArmor) return false;

      if (rhs is ItemGrenade) return !(lhs is ItemGrenade);
      else if (lhs is ItemGrenade) return false;

      // light and entertainment have been revised to possibly higher priority (context-sensitive)
      // traps and barricade material are guaranteed insurance policy status
      // medicine currently is, but that's an AI flaw

      // XXX note that sleep and stamina have special uses for sufficiently good AI
      bool lhs_low_priority = (lhs is ItemLight) || (lhs is ItemTrap) || (lhs is ItemMedicine) || (lhs is ItemEntertainment) || (lhs is ItemBarricadeMaterial);
      if ((rhs is ItemLight) || (rhs is ItemTrap) || (rhs is ItemMedicine) || (rhs is ItemEntertainment) || (rhs is ItemBarricadeMaterial)) return !lhs_low_priority;
      else if (lhs_low_priority) return false;

      List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
      if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);

      if (rhs is ItemTracker)
        {
        if (!(lhs is ItemTracker)) return false;
        if (ok_trackers.Contains(lhs.Model.ID)) return false;
        return ok_trackers.Contains(rhs.Model.ID);
        }
      else if (lhs is ItemTracker) return false;

      return false;
    }

    protected T GetWorst<T>(IEnumerable<T> src) where T:Item
    {
#if DEBUG
      if (0 >= (src?.Count() ?? 0)) return null;
#endif
      T worst = null;
      foreach(T test in src) {
        if (null == worst) worst = test;
        else if (RHSMoreInteresting(test,worst)) worst = test;
      }
      return worst;
    }

    protected ActorAction BehaviorMakeRoomFor(Item it, Point? position=null)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!m_Actor.Inventory.IsFull) throw new InvalidOperationException("already have room for "+it.ToString());
      if (m_Actor.CanGet(it)) throw new InvalidOperationException("already could get "+it.ToString());
      // also should require IsInterestingItem(it), but that's infinite recursion for reasonable use cases
#endif
      Inventory inv = m_Actor.Inventory;
      { // drop useless item doesn't always happen in a timely fashion
      var useless = inv.Items.Where(obj => ItemIsUseless(obj)).ToList();
      if (0<useless.Count) return _BehaviorDropOrExchange(useless[0], it, position);
      }

      // not-best body armor can be dropped
      if (2<=m_Actor.CountQuantityOf<ItemBodyArmor>()) {
        ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
        if (null != armor) return _BehaviorDropOrExchange(armor,it,position);
      }

      { // not-best melee weapon can be dropped
        List<ItemMeleeWeapon> melee = inv.GetItemsByType<ItemMeleeWeapon>();
        if (null != melee) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          if (2<=melee.Count) return _BehaviorDropOrExchange(weapon, it, position);
          if (it is ItemMeleeWeapon && weapon.Model.Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating) return _BehaviorDropOrExchange(weapon, it, position);
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

      int it_rating = ItemRatingCode(it);
      if (1==it_rating && it is ItemMeleeWeapon) return null;   // break action loop here
      if (1<it_rating) {
        // generally, find a less-critical item to drop
        int i = 0;
        while(++i < it_rating) {
          Item worst = GetWorst(m_Actor.Inventory.Items.Where(obj => ItemRatingCode(obj) == i));
          if (null == worst) continue;
          return _BehaviorDropOrExchange(worst, it, position);
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
            if (null != position) {
              ActorAction tmp = _PrefilterDrop(drop);
              if (null != tmp) return tmp;

              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionTradeWithContainer(m_Actor,drop,it,position.Value));
            } else {
              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionDropItem(m_Actor,drop));
              // 3b) pick up ammo
              recover.Add(new ActionTake(m_Actor,it.Model.ID));
            }
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
            if (null != position) {
              ActorAction tmp = _PrefilterDrop(drop);
              if (null != tmp) return tmp;

              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionTradeWithContainer(m_Actor,drop,it,position.Value));
            } else {
              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionDropItem(m_Actor,drop));
              // 3b) pick up ammo
              recover.Add(new ActionTake(m_Actor,it.Model.ID));
            }
            // 3c) use ammo just picked up : arguably ActionUseItem; use ActionUse(Actor actor, Gameplay.GameItems.IDs it)
            recover.Add(new ActionUse(m_Actor, it.Model.ID));
            return new ActionChain(m_Actor,recover);
          }
          }
      }
      }

      {
      if (it is ItemFood food) {
          Item drop = inv.GetFirst<ItemFood>();
          if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
          if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
          if (null == drop) drop = inv.GetFirst<ItemSprayScent>();
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
          if (m_Actor.IsHungry) {
            if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
            if (null == drop) drop = inv.GetFirst<ItemGrenade>();
            if (null == drop) drop = inv.GetFirst<ItemAmmo>();
            if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
          }
          if (null != drop) {
            if (null != position) return _BehaviorDropOrExchange(drop,it,position.Value);
            List<ActorAction> recover = new List<ActorAction>(2);
            // 3a) drop target without triggering the no-pickup schema
            recover.Add(new ActionDropItem(m_Actor,drop));
            // 3b) pick up food
            recover.Add(new ActionTake(m_Actor,it.Model.ID));
            return new ActionChain(m_Actor,recover);
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
          return _BehaviorDropOrExchange(armor, it, position);
        }
      }

      // medicine glut ... drop it
      foreach(GameItems.IDs x in GameItems.medicine) {
        if (it.Model.ID == x) continue;
        ItemModel model = Models.Items[(int)x];
        if (2>m_Actor.Count(model)) continue;
        Item tmp = m_Actor.Inventory.GetBestDestackable(model);
        if (null != tmp) return _BehaviorDropOrExchange(tmp, it, position);

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
      if (null != tmpTracker) return _BehaviorDropOrExchange(tmpTracker, it, position);

      // ahem...food glut, of all things
      if (   !(it is ItemEntertainment)
          && !(it is ItemBarricadeMaterial)
          && !(it is ItemSprayScent)
          && it.Model != GameItems.PILLS_SAN
          && it.Model != GameItems.PILLS_ANTIVIRAL
          && it.Model != GameItems.PILLS_STA) {
        ItemFood tmpFood = inv.GetFirst<ItemFood>(f => !IsInterestingItem(f) && f.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter));
        if (null != tmpFood) return _BehaviorDropOrExchange(tmpFood, it, position);
        tmpFood = inv.GetFirst<ItemFood>(f => !IsInterestingItem(f) && f.IsExpiredAt(m_Actor.Location.Map.LocalTime.TurnCounter));
        if (null != tmpFood) return _BehaviorDropOrExchange(tmpFood, it, position);
        tmpFood = inv.GetFirst<ItemFood>(f => !IsInterestingItem(f));
        if (null != tmpFood) return _BehaviorDropOrExchange(tmpFood, it, position);
      }

      // these lose to everything other than trackers.  Note that we should drop a light to get a more charged light -- if we're right on top of it.
      if (it is ItemSprayScent) return null;
      ItemSprayScent tmpSprayScent = inv.GetFirstMatching<ItemSprayScent>();
      if (null != tmpSprayScent) return _BehaviorDropOrExchange(tmpSprayScent, it, position);

      if (it is ItemBarricadeMaterial) return null;
      ItemBarricadeMaterial tmpBarricade = inv.GetFirstMatching<ItemBarricadeMaterial>();
      if (null != tmpBarricade) return _BehaviorDropOrExchange(tmpBarricade, it, position);

      if (it is ItemEntertainment) return null;
      ItemEntertainment tmpEntertainment = inv.GetFirstMatching<ItemEntertainment>();
      if (null != tmpEntertainment) return _BehaviorDropOrExchange(tmpEntertainment, it, position);

      if (it is ItemTrap) return null;
      ItemTrap tmpTrap = inv.GetFirstMatching<ItemTrap>();
      if (null != tmpTrap) return _BehaviorDropOrExchange(tmpTrap, it, position);

      if (it is ItemLight) {
        if (1 >= it_rating) return null;
        Item worst = GetWorst(m_Actor.Inventory.Items.Where(obj => 1 >= ItemRatingCode(obj)));
        if (null == worst) return null;
        return _BehaviorDropOrExchange(worst, it, position);
      }

      if (it is ItemMedicine) return null;

      // ditch unimportant items
      ItemMedicine tmpMedicine = inv.GetFirstMatching<ItemMedicine>();
      if (null != tmpMedicine) return _BehaviorDropOrExchange(tmpMedicine, it, position);

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
      if (null != tmpAmmo) return _BehaviorDropOrExchange(tmpAmmo, it, position);

      // ranged weapon with zero ammo is ok to drop for something other than its own ammo
      ItemRangedWeapon tmpRw2 = inv.GetFirstMatching<ItemRangedWeapon>(rw => 0 >= rw.Ammo);
      if (null != tmpRw2) {
         bool reloadable = (it is ItemAmmo ? (it as ItemAmmo).AmmoType==tmpRw2.AmmoType : false);
         if (!reloadable) return _BehaviorDropOrExchange(tmpRw2, it, position);
      }

      // if we have 2 clips of an ammo type, trading one for a melee weapon or food is ok
      if (it is ItemMeleeWeapon || it is ItemFood) {
        foreach(GameItems.IDs x in GameItems.ammo) {
          ItemModel model = Models.Items[(int)x];
          if (2<=m_Actor.Count(model)) {
            ItemAmmo ammo = inv.GetBestDestackable(model) as ItemAmmo;
            return _BehaviorDropOrExchange(ammo, it, position);
          }
        }
        // if we have two clips of any type, trading the smaller one for a melee weapon or food is ok
        ItemAmmo test = null;
        foreach(GameItems.IDs x in GameItems.ammo) {
          if (inv.GetBestDestackable(Models.Items[(int)x]) is ItemAmmo ammo) {
             if (null == test || test.Quantity>ammo.Quantity) test = ammo;
          }
        }
        return _BehaviorDropOrExchange(test, it, position);
      }

      // if inventory is full and the problem is ammo at this point, ignore if we already have a full clip
      if (it is ItemAmmo && 1<=m_Actor.Count(it.Model)) return null;
      if (it is ItemAmmo && AmmoAtLimit) return null;

      // if inventory is full and the problem is ranged weapon at this point, ignore if we already have one
      if (it is ItemRangedWeapon && 1<= inv.CountType<ItemRangedWeapon>()) return null;

      // grenades next
      if (it is ItemGrenade) return null;
      ItemGrenade tmpGrenade = inv.GetFirstMatching<ItemGrenade>();
      if (null != tmpGrenade) return _BehaviorDropOrExchange(tmpGrenade, it, position);

      // important trackers go for ammo
      if (it is ItemAmmo) {
        ItemTracker discardTracker = inv.GetFirstMatching<ItemTracker>();
        if (null != discardTracker) return _BehaviorDropOrExchange(discardTracker, it, position);
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
        if (0 >= m_Actor.Inventory.CountType<ItemRangedWeapon>(it => 0 < it.Ammo) && null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return true;
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
      ItemRangedWeapon rw = m_Actor.GetCompatibleRangedWeapon(am);
      if (null == rw) {
        if (0 < m_Actor.Inventory.CountType<ItemRangedWeapon>()) return false;  // XXX
        if (0 < m_Actor.Inventory.Count(am.Model)) return false;    // only need one clip to prime AI to look for empty ranged weapons
      } else {
        if (rw.Model.MaxAmmo>rw.Ammo) return true;
        if (m_Actor.HasAtLeastFullStackOf(am, 2)) return false;
        if (null != m_Actor.Inventory.GetFirstByModel<ItemAmmo>(am.Model, it => it.Quantity < it.Model.MaxQuantity)) return true;   // topping off clip is ok
      }
      return _InterestingItemPostprocess(am);
    }

    public bool IsInterestingItem(ItemFood food)
    {
      return !m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food);
    }

    // so we can do post-condition testing cleanly
    private bool _IsInterestingItem(Item it)
    {
      // note that CHAR guards and soldiers don't need to eat like civilians, so they would not be interested in food
      if (it is ItemFood food) {
//      if (!m_Actor.Model.Abilities.HasToEat) return false;    // redundant; for documentation
        if (m_Actor.IsHungry) return true;
        if (m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food)) return false;
        return !food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
      }

      if (it is ItemRangedWeapon rw) {
//      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;    // redundant; for documentation
        return IsInterestingItem(rw);
      }
      if (it is ItemAmmo am) {
//      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;    // redundant; for documentation
        return IsInterestingItem(am);
      }
      if (it is ItemMeleeWeapon melee) {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if (m_Actor.MeleeWeaponAttack(it.Model as ItemMeleeWeaponModel).Rating <= martial_arts.Rating) return false;
        ItemMeleeWeapon best = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
        if (null == best) return true;
        if (best.Model.Attack.Rating < melee.Model.Attack.Rating) return true;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
#if DEBUG
        if (0 >= melee_count) throw new InvalidOperationException("inconstent return values");
#endif        
        if (2<= melee_count) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          return weapon.Model.Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        return true;
      }
      if (it is ItemMedicine) {
        // XXX easy to action-loop if inventory full
        // this plausibly should actually check inventory-clearing options
        if (!m_Actor.Inventory.IsFull) return !m_Actor.HasAtLeastFullStackOf(it, 2);
      }
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null == armor) return true;
        return armor.Rating < (it as ItemBodyArmor).Rating; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
      }

      // No specific heuristic.
      if (it is ItemTracker) {
        if (1<=m_Actor.Inventory.Count(it.Model)) return false;
      } else if (it is ItemLight) {
        if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1)) return false;
      } else {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
      }
      return _InterestingItemPostprocess(it);
    }

    public bool IsInterestingItem(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!m_Actor.Model.Abilities.HasInventory) throw new InvalidOperationException("inventory required");   // CHAR guards: wander action can get item from containers
      if (!m_Actor.Model.Abilities.CanUseMapObjects) throw new InvalidOperationException("using map objects required");
#endif
      if (ItemIsUseless(it)) return false;

#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      bool ret = _IsInterestingItem(it);
      int item_rating = ItemRatingCode(it);
      if (ret && 1>item_rating) throw new InvalidOperationException("interesting item thought to have no use");
      if (!ret && 1<item_rating) {
        // check inventory for less-interesting item.  Force high visibility in debugger.
        foreach(Item obj in m_Actor.Inventory.Items) {
          int test_rating = ItemRatingCode(obj);
          if (test_rating < item_rating) throw new InvalidOperationException("uninteresting item thought to have a clear use");
        }
      }
      return ret;
#else
      return _IsInterestingItem(it);
#endif
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

    private void _InterpretRangedWeapons(IEnumerable<ItemRangedWeapon> rws, Point pt, Dictionary<Point, ItemRangedWeapon[]> best_rw, Dictionary<Point, ItemRangedWeapon[]> reload_empty_rw, Dictionary<Point, ItemRangedWeapon[]> discard_empty_rw, Dictionary<Point, ItemRangedWeapon[]> reload_rw)
    {
        if (!rws?.Any() ?? true) return;

        best_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        reload_empty_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        discard_empty_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        reload_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        bool keep_empty = false;
        bool keep_reload = false;
        
        foreach(var rw in rws) {
          // note that "better" ranged weapons taking the same ammo have larger clips
          if (0==rw.Ammo) {
            if (null == reload_empty_rw[pt][(int)rw.AmmoType]) reload_empty_rw[pt][(int)rw.AmmoType] = rw;
            else if (reload_empty_rw[pt][(int)rw.AmmoType].Model.MaxAmmo < rw.Model.MaxAmmo) reload_empty_rw[pt][(int)rw.AmmoType] = rw;
            if (null == discard_empty_rw[pt][(int)rw.AmmoType]) discard_empty_rw[pt][(int)rw.AmmoType] = rw;
            else if (discard_empty_rw[pt][(int)rw.AmmoType].Model.MaxAmmo > rw.Model.MaxAmmo) discard_empty_rw[pt][(int)rw.AmmoType] = rw;
            keep_empty = true;
          }
          if (rw.Model.MaxAmmo > rw.Ammo) {
            if (null == reload_rw[pt][(int)rw.AmmoType]) reload_rw[pt][(int)rw.AmmoType] = rw;
            else if (reload_rw[pt][(int)rw.AmmoType].Model.MaxAmmo < rw.Model.MaxAmmo) reload_rw[pt][(int)rw.AmmoType] = rw;
            else if ((reload_rw[pt][(int)rw.AmmoType].Model.MaxAmmo - reload_rw[pt][(int)rw.AmmoType].Ammo) < (rw.Model.MaxAmmo-rw.Ammo)) reload_rw[pt][(int)rw.AmmoType] = rw;
            keep_reload = true;
          }
          if (null == best_rw[pt][(int)rw.AmmoType]) {
            best_rw[pt][(int)rw.AmmoType] = rw;
            continue;
          }
          if (best_rw[pt][(int)rw.AmmoType].Ammo < rw.Ammo) {
            best_rw[pt][(int)rw.AmmoType] = rw;
            continue;
          }
          if (best_rw[pt][(int)rw.AmmoType].Model.MaxAmmo < rw.Model.MaxAmmo) {
            best_rw[pt][(int)rw.AmmoType] = rw;
            continue;
          }
        }
        if (!keep_empty) {
          reload_empty_rw.Remove(pt);
          discard_empty_rw.Remove(pt);
        }
        if (!keep_reload) {
          reload_rw.Remove(pt);
        }
    }

    // we are having some problems with breaking an action loop that requires reloading a weapon to make ammo gettable, when already at ammo limit
    // the logic is there but it's not being reached.
    // issue w/recovery logic and ammo
    protected ActorAction InventoryStackTactics(Location loc)
    {
      if (m_Actor.Inventory.IsEmpty) return null;

      // The index case.
      var rws = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>();
      if (0 < (rws?.Count ?? 0)) {
        foreach(var rw in rws) {
          if (rw.Ammo < rw.Model.MaxAmmo) {
            // usually want to reload this even if we had to drop ammo as a recovery option
            int i = Objectives.Count;
            while(0<i) {
              if (Objectives[--i] is Goal_DoNotPickup dnp) {
                if (dnp.Avoid != (GameItems.IDs)((int)(rw.AmmoType)+(int)(GameItems.IDs.AMMO_LIGHT_PISTOL))) continue;
                Objectives.RemoveAt(i);
              }
            }
          }
        }
      }

      Dictionary<Point,Inventory> ground_inv = loc.Map.GetAccessibleInventories(loc.Position);
      if (0 >= ground_inv.Count) return null;

      // set up pattern-matching for ranged weapons
      Point viewpoint_inventory = new Point(int.MaxValue,int.MaxValue); // intentionally chosen to be impossible, as a flag
      var best_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var reload_empty_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var discard_empty_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var reload_rw = new Dictionary<Point, ItemRangedWeapon[]>();

      _InterpretRangedWeapons(rws, viewpoint_inventory, best_rw, reload_empty_rw, discard_empty_rw, reload_rw);

      if (reload_rw.ContainsKey(viewpoint_inventory)) {
        // prepare to analyze ranged weapon swaps.
        foreach(var x in ground_inv) {
          var ground_rws = x.Value.GetItemsByType<ItemRangedWeapon>();
          _InterpretRangedWeapons(ground_rws, x.Key, best_rw, reload_empty_rw, discard_empty_rw, reload_rw);
        }

        if (discard_empty_rw.ContainsKey(viewpoint_inventory)) {
          // we should not have been able to reload this i.e. no ammo.
          Point? dest = null;
          ItemRangedWeapon test = null;
          ItemRangedWeapon src = null;
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == discard_empty_rw[viewpoint_inventory][i]) continue;
            foreach(var where_inv in best_rw) {
              if (where_inv.Key == viewpoint_inventory) continue;
              if (null == where_inv.Value[i]) continue;
              if (0 >= where_inv.Value[i].Ammo) continue;
              if (null == test) {
                dest = where_inv.Key;
                src = discard_empty_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
              if (test.Ammo < where_inv.Value[i].Ammo && test.Model.MaxAmmo <= where_inv.Value[i].Model.MaxAmmo) {
                dest = where_inv.Key;
                src = discard_empty_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
            }
          }
          if (null != test) return new ActionTradeWithContainer(m_Actor,src,test,dest.Value);
        }

        // optimization
        {
          Point? dest = null;
          ItemRangedWeapon test = null;
          ItemRangedWeapon src = null;
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == reload_rw[viewpoint_inventory][i]) continue;
            foreach(var where_inv in best_rw) {
              if (where_inv.Key == viewpoint_inventory) continue;
              if (null == where_inv.Value[i]) continue;
              if (reload_rw[viewpoint_inventory][i].Ammo >= where_inv.Value[i].Ammo) continue;
              if (reload_rw[viewpoint_inventory][i].Model.MaxAmmo > where_inv.Value[i].Model.MaxAmmo) continue;
              if (null == test) {
                dest = where_inv.Key;
                src = reload_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
              if (test.Ammo < where_inv.Value[i].Ammo && test.Model.MaxAmmo <= where_inv.Value[i].Model.MaxAmmo) {
                dest = where_inv.Key;
                src = reload_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
            }
          }
          if (null != test) return new ActionTradeWithContainer(m_Actor,src,test,dest.Value);
        }
      }

      return null;
    }

    protected ActorAction InventoryStackTactics() { return InventoryStackTactics(m_Actor.Location); }

    /// <remark>Intentionally asymmetric.  Call this twice to get proper coverage.
    /// Will ultimately end up in ObjectiveAI when AI state needed.</remark>
    static public bool TradeVeto(Item mine, Item theirs)
    {
      // reject identity trades for now.  This will change once AI state is involved.
      if (mine.Model == theirs.Model) return true;

      switch(mine.Model.ID)
      {
      // two weapons for the ammo
      case GameItems.IDs.RANGED_PRECISION_RIFLE:
      case GameItems.IDs.RANGED_ARMY_RIFLE:
        if (GameItems.IDs.AMMO_HEAVY_RIFLE==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_PISTOL:
      case GameItems.IDs.RANGED_KOLT_REVOLVER:
        if (GameItems.IDs.AMMO_LIGHT_PISTOL==theirs.Model.ID) return true;
        break;
      // one weapon for the ammo
      case GameItems.IDs.RANGED_ARMY_PISTOL:
        if (GameItems.IDs.AMMO_HEAVY_PISTOL==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_HUNTING_CROSSBOW:
        if (GameItems.IDs.AMMO_BOLTS==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_HUNTING_RIFLE:
        if (GameItems.IDs.AMMO_LIGHT_RIFLE==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_SHOTGUN:
        if (GameItems.IDs.AMMO_SHOTGUN==theirs.Model.ID) return true;
        break;
      // flashlights.  larger radius and longer duration are independently better...do not trade if both are worse
      case GameItems.IDs.LIGHT_BIG_FLASHLIGHT:
        if (GameItems.IDs.LIGHT_FLASHLIGHT==theirs.Model.ID && (theirs as BatteryPowered).Batteries<(mine as BatteryPowered).Batteries) return true;
        if (GameItems.IDs.LIGHT_BIG_FLASHLIGHT==theirs.Model.ID && (theirs as BatteryPowered).Batteries<(mine as BatteryPowered).Batteries) return true;
        break;
      case GameItems.IDs.LIGHT_FLASHLIGHT:
        if (GameItems.IDs.LIGHT_FLASHLIGHT==theirs.Model.ID && (theirs as BatteryPowered).Batteries<(mine as BatteryPowered).Batteries) return true;
        break;
      }
      return false;
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
        if (null != tmp_rw) {
          if (!AmmoAtLimit) {
            foreach(ItemRangedWeapon rw in tmp_rw) {
              if (null == m_Actor.Inventory.GetCompatibleAmmoItem(rw)) {
                if (rw.Ammo < rw.Model.MaxAmmo || !AmmoAtLimit) {
                  ret.Add((GameItems.IDs)((int)rw.AmmoType + (int)GameItems.IDs.AMMO_LIGHT_PISTOL));    // Validity explicitly tested for in GameItems::CreateModels
                }
              }
            }
          }
        }
        List<ItemAmmo> tmp_ammo = m_Actor.Inventory.GetItemsByType<ItemAmmo>();
        if (null != tmp_ammo && (null == tmp_rw || !AmmoAtLimit)) {
          foreach(ItemAmmo am in tmp_ammo) {
            if (null == m_Actor.GetCompatibleRangedWeapon(am)) {
                switch(am.Model.ID)
                {
                case GameItems.IDs.AMMO_LIGHT_PISTOL:
                  ret.Add(GameItems.IDs.RANGED_PISTOL); // weakly dominates Kolt
                  ret.Add(GameItems.IDs.RANGED_KOLT_REVOLVER);
                  ret.Add(GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL);   // dominates both normal pistols
                  break;
                case GameItems.IDs.AMMO_HEAVY_PISTOL:
                  ret.Add(GameItems.IDs.RANGED_ARMY_PISTOL);
                  break;
                case GameItems.IDs.AMMO_LIGHT_RIFLE:
                  ret.Add(GameItems.IDs.RANGED_HUNTING_RIFLE);
                  break;
                case GameItems.IDs.AMMO_HEAVY_RIFLE:
                  ret.Add(GameItems.IDs.RANGED_ARMY_RIFLE);     // mostly dominates precision rifle
                  ret.Add(GameItems.IDs.RANGED_PRECISION_RIFLE);
                  break;
                case GameItems.IDs.AMMO_SHOTGUN:
                  ret.Add(GameItems.IDs.RANGED_SHOTGUN);
                  ret.Add(GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN);   // dominates shotgun
                  break;
                case GameItems.IDs.AMMO_BOLTS:
                  ret.Add(GameItems.IDs.RANGED_HUNTING_CROSSBOW);
                  break;
                }
            }
          }
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

    protected static int ScoreRangedWeapon(ItemRangedWeapon w)
    {
      Attack rw_attack = w.Model.Attack;
      return 1000 * rw_attack.Range + rw_attack.DamageValue;
    }

    // conceptual difference between "doctrine" and "behavior" is that doctrine doesn't have contextual validity checks
    // that is, a null action return is defined to mean the doctrine is invalid
    public ActorAction DoctrineRecoverSTA(int targetSTA)
    {
       if (m_Actor.MaxSTA < targetSTA) targetSTA = m_Actor.MaxSTA;
       if (m_Actor.StaminaPoints >= targetSTA) return null;
       if (   m_Actor.StaminaPoints < targetSTA - 4
           && m_Actor.CanActNextTurn) {
         Item stim = m_Actor?.Inventory.GetBestDestackable(Models.Items[(int)GameItems.IDs.MEDICINE_PILLS_STA]);
         if (null != stim) return new ActionUseItem(m_Actor,stim);
       }
       return new ActionWait(m_Actor);
    }

    public ActorAction DoctrineMedicateSLP()
    {
       ItemMedicine stim = (m_Actor?.Inventory.GetBestDestackable(Models.Items[(int)Gameplay.GameItems.IDs.MEDICINE_PILLS_SLP]) as ItemMedicine);
       if (null == stim) return null;
       int threshold = m_Actor.MaxSleep-(Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost));
       if (m_Actor.SleepPoints > threshold) return null;
       if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
       return new ActionUseItem(m_Actor,stim);
    }

    public ActorAction DoctrineRechargeToFull(Item it)
    {
      BatteryPowered obj = it as BatteryPowered;
#if DEBUG
      if (null == obj) throw new ArgumentNullException(nameof(obj));
#endif
      if (obj.MaxBatteries-1 <= obj.Batteries) return null;
      var generators = m_Actor.Location.Map.PowerGenerators.Get.Where(power => Rules.IsAdjacent(m_Actor.Location,power.Location)).ToList();
      if (0 >= generators.Count) return null;
      var generators_on = generators.Where(power => power.IsOn).ToList();
      if (0 >= generators_on.Count) return new ActionSwitchPowerGenerator(m_Actor,generators[0]);
      if (!it.IsEquipped) RogueForm.Game.DoEquipItem(m_Actor,it);
      if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
      return new ActionRechargeItemBattery(m_Actor,it);
    }

    // XXX should also have concept of hoardable item (suitable for transporting to a safehouse)
    public ItemRangedWeapon GetBestRangedWeaponWithAmmo()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      var rws = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>(rw => {
        if (0 < rw.Ammo) return true;
        var ammo = m_Actor.Inventory.GetItemsByType < ItemAmmo >(am => am.AmmoType==rw.AmmoType);
        return null != ammo;
      });
      if (null == rws) return null;
      if (1==rws.Count) return rws[0];
      ItemRangedWeapon obj1 = null;
      int num1 = 0;
      foreach (ItemRangedWeapon w in rws) {
        int num2 = ScoreRangedWeapon(w);
        if (num2 > num1) {
          obj1 = w;
          num1 = num2;
        }
      }
      return obj1;
    }

  }
}
