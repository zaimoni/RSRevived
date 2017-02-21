// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.AI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorController
  {
    protected Actor m_Actor;

#if CONTRACTS_FULL
	public Actor Actor { get { return m_Actor; } }
#endif

    public virtual void TakeControl(Actor actor)
    {
      m_Actor = actor;
      if (null!=actor.Location.Map) UpdateSensors();
    }

    public virtual void LeaveControl()
    {
      m_Actor = null;
    }

    public virtual Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> ItemMemory {
       get { 
         if (null == m_Actor) return null;
         if ((int)Gameplay.GameFactions.IDs.ThePolice == m_Actor.Faction.ID) return Session.Get.PoliceItemMemory;
         return null;
       }
    }

    public bool LastSeen(Location x, out int turn) { turn = 0; return (null != ItemMemory ? ItemMemory.HaveEverSeen(x, out turn) : false); }

    public bool IsKnown(Location x) {
      int discard;
      return LastSeen(x, out discard);
    }

    public void ForceKnown(Point x) {   // for world creation
      ItemMemory?.Set(new Location(m_Actor.Location.Map, x), null, m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    public List<Gameplay.GameItems.IDs> WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int> WhereIs(Gameplay.GameItems.IDs x) { return ItemMemory?.WhereIs(x); }

    public HashSet<Point> WhereIs(IEnumerable<Gameplay.GameItems.IDs> src, Map map) {
      HashSet<Point> ret = new HashSet<Point>();
      foreach(Gameplay.GameItems.IDs it in src) {
        Dictionary<Location, int> tmp = WhereIs(it);
        tmp.OnlyIf(loc=>loc.Map == map);
        ret.UnionWith(tmp.Keys.Select(loc => loc.Position));
      }
      return ret;
    }

    public abstract List<Percept> UpdateSensors();

    // vision
    public abstract HashSet<Point> FOV { get; }

    public bool CanSee(Location x)
    {
      if (null == m_Actor) return false;
      if (null == x.Map) return false;    // convince Duckman to not superheroically crash many games on turn 0 
      if (x.Map != m_Actor.Location.Map) return false;  // revise these two when restricted district exits go away
      if (!x.Map.IsInBounds(x.Position)) return false;
      if (x.Position == m_Actor.Location.Position) return true; // for GUI purposes can see oneself even if sleeping.
      if (m_Actor.IsSleeping) return false;
      HashSet<Point> tmpFOV = FOV;  // virtual function call may be time-expensive so cache
      if (null == tmpFOV) return false;
      return tmpFOV.Contains(x.Position);
    }

    public void VisibleMaximumDamage(Dictionary<Point, int> ret,List<Actor> slow_melee_threat, HashSet<Actor> immediate_threat)
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
        if (0==a.CurrentRangedAttack.Range && 1 == Rules.GridDistance(m_Actor.Location.Position, a.Location.Position) && m_Actor.Speed>a.Speed) slow_melee_threat.Add(a);
        // calculate melee damage field now
        Dictionary<Point,int> melee_damage_field = new Dictionary<Point,int>();
        int a_max_dam = a.MeleeAttack(m_Actor).DamageValue;
        foreach(Point pt in Direction.COMPASS.Select(dir=>a.Location.Position+dir).Where(pt=>map.IsInBounds(pt) && map.GetTileModelAt(pt).IsWalkable)) {
          melee_damage_field[pt] = a_turns*a_max_dam;
        }
        while(1<a_turns) {
          HashSet<Point> sweep = new HashSet<Point>(melee_damage_field.Keys);
          a_turns--;
          foreach(Point pt2 in sweep) {
            foreach(Point pt in Direction.COMPASS.Select(dir=>pt2+dir).Where(pt=>map.IsInBounds(pt) && map.GetTileModelAt(pt).IsWalkable && !sweep.Contains(pt))) {
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
          foreach(Point pt in melee_damage_field.Keys) {
            if (ret.ContainsKey(pt)) ret[pt] += melee_damage_field[pt];
            else ret[pt] = melee_damage_field[pt];
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
          HashSet<Point> now = new HashSet<Point>();
          now.Add(a.Location.Position);
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
        foreach(Point pt in ranged_damage_field.Keys) {
          if (melee_damage_field.ContainsKey(pt)) {
            if (ret.ContainsKey(pt)) ret[pt] += Math.Max(ranged_damage_field[pt], melee_damage_field[pt]);
            else ret[pt] = Math.Max(ranged_damage_field[pt], melee_damage_field[pt]);
          } else {
            if (ret.ContainsKey(pt)) ret[pt] += ranged_damage_field[pt];
            else ret[pt] = ranged_damage_field[pt];
          }
        }
      }
    }

	public bool AddExplosivesToDamageField(Dictionary<Point,int> damage_field, List<Percept> percepts)
	{
      List<Percept> goals = percepts.FilterT<Inventory>(inv => inv.Has<ItemPrimedExplosive>());
      if (null == goals) return false;
      bool in_blast_field = false;
	  IEnumerable<Percept_<ItemPrimedExplosive>> explosives = goals.Select(p=>new Percept_<ItemPrimedExplosive>((p.Percepted as Inventory).GetFirst<ItemPrimedExplosive>(), p.Turn, p.Location));
	  foreach(Percept_<ItemPrimedExplosive> exp in explosives) {
	    BlastAttack tmp_blast = (exp.Percepted.Model as ItemExplosiveModel).BlastAttack;
		Point pt = exp.Location.Position;
	    if (damage_field.ContainsKey(pt)) damage_field[pt]+=tmp_blast.Damage[0];
	    else damage_field[pt] = tmp_blast.Damage[0];
	    // We would need a very different implementation for large blast radii.
        int r = 0;
	    while(++r <= tmp_blast.Radius) {
          foreach(Point p in Enumerable.Range(0,8*r).Select(i=>exp.Location.Position.RadarSweep(r,i))) {
            if (!exp.Location.Map.IsInBounds(p)) continue;
            if (!LOS.CanTraceFireLine(exp.Location,p,tmp_blast.Radius)) continue;
	        if (damage_field.ContainsKey(p)) damage_field[p]+=tmp_blast.Damage[r];
	        else damage_field[p] = tmp_blast.Damage[r];
            if (p == m_Actor.Location.Position) in_blast_field = true;
          }
	    }
	  }
      return in_blast_field;
	}

    public void AddTrapsToDamageField(Dictionary<Point,int> damage_field, List<Percept> percepts)
    {
      List<Percept> goals = percepts.FilterT<Inventory>(inv => inv.Has<ItemTrap>());
      if (null == goals) return;
      foreach(Percept p in goals) {
        List<ItemTrap> tmp = (p.Percepted as Inventory).GetItemsByType<ItemTrap>();
        if (null == tmp) continue;
        int damage = tmp.Sum(trap => (trap.IsActivated ? trap.TrapModel.Damage : 0));   // XXX wrong for barbed wire
        if (0 >= damage) continue;
        if (damage_field.ContainsKey(p.Location.Position)) damage_field[p.Location.Position] += damage;
        else damage_field[p.Location.Position] = damage;
      }
    }

    public abstract ActorAction GetAction(RogueGame game);

    // savegame support
    public virtual void OptimizeBeforeSaving() { }  // override this if there are memorized sensors

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
            if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
              return (it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
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

    public virtual bool IsInterestingItem(Item it)
    {
	  Contract.Requires(null != it);
#if DEBUG
      Contract.Requires(Actor.Model.Abilities.HasInventory);    // CHAR guards: wander action can trigger getting items from containers.
      Contract.Requires(Actor.Model.Abilities.CanUseMapObjects);
#endif
	  if (it.IsForbiddenToAI) return false;
	  if (it is ItemSprayPaint) return false;
	  if (it is ItemTrap && (it as ItemTrap).IsActivated) return false;

      // only soldiers and civilians use grenades (CHAR guards are disallowed as a balance issue)
      if (Gameplay.GameItems.IDs.EXPLOSIVE_GRENADE == it.Model.ID && !(m_Actor.Controller is Gameplay.AI.CivilianAI) && !(m_Actor.Controller is Gameplay.AI.SoldierAI)) return false;

      // only civilians use stench killer
      if (Gameplay.GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.Model.ID && !(m_Actor.Controller is Gameplay.AI.CivilianAI)) return false;

      // police have implicit police trackers
      if (Gameplay.GameItems.IDs.TRACKER_POLICE_RADIO == it.Model.ID && (int)Gameplay.GameFactions.IDs.ThePolice == m_Actor.Faction.ID) return false;

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
        if (1 <= m_Actor.CountItemsOfSameType(typeof(ItemRangedWeapon))) return false;  // XXX rules out AI gun bunnies
        if (!m_Actor.Inventory.Contains(it) && m_Actor.HasItemOfModel(it.Model)) return false;
        ItemRangedWeapon rw = it as ItemRangedWeapon;
        return rw.Ammo > 0 || m_Actor.GetCompatibleAmmoItem(rw) != null;
      }
      if (it is ItemAmmo) {
        ItemAmmo am = it as ItemAmmo;
        if (m_Actor.GetCompatibleRangedWeapon(am) == null) return false;
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      }
      if (it is ItemMeleeWeapon) {
        Attack martial_arts = m_Actor.UnarmedMeleeAttack();
        if ((it.Model as ItemMeleeWeaponModel).Attack.Rating <= martial_arts.Rating) return false;

        if (2<=m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon))) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          return (weapon.Model as ItemMeleeWeaponModel).Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        if (1<= m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon)) && 1>= m_Actor.Inventory.MaxCapacity- m_Actor.Inventory.CountItems) {
          ItemMeleeWeapon weapon = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
          if (null == weapon) return true;  // martial arts invalidates starting baton for police
          return (weapon.Model as ItemMeleeWeaponModel).Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        return true;
      }
      if (it is ItemMedicine)
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      if (it is ItemBodyArmor) { 
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null == armor) return true;
        return armor.Rating < (it as ItemBodyArmor).Rating;
      }
      if (it.IsUseless || it is ItemPrimedExplosive || m_Actor.IsBoredOf(it))
        return false;
      return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1);
    }

    public virtual bool IsInterestingTradeItem(Actor speaker, Item offeredItem) // Cf. OrderableAI::IsRationalTradeItem
    {
      Contract.Requires(null!=speaker);
      Contract.Requires(speaker.Model.Abilities.CanTrade);
#if DEBUG
      Contract.Requires(Actor.Model.Abilities.CanTrade);
#endif
      if (RogueForm.Game.Rules.RollChance(Rules.ActorCharismaticTradeChance(speaker))) return true;
      return IsInterestingItem(offeredItem);
    }
  }
}
