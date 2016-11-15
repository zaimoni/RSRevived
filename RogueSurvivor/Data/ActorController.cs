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

    public void VisibleMaximumDamage(Dictionary<Point, int> ret,List<Actor> slow_melee_threat)
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
        if (!a.CanActNextTurn) continue;
        if (0==a.CurrentRangedAttack.Range && 1 == Rules.GridDistance(m_Actor.Location.Position, a.Location.Position)) slow_melee_threat.Add(a);
        // calculate melee damage field now
        Dictionary<Point,int> melee_damage_field = new Dictionary<Point,int>();
        int a_max_dam = a.MeleeAttack(m_Actor).DamageValue;
        if (RogueForm.Game.Rules.WillOtherActTwiceBefore(m_Actor, a)) {
          HashSet<Point> radius2 = new HashSet<Point>();
          foreach(Point pt in Direction.COMPASS.Select(dir=>a.Location.Position+dir).Where(pt=>map.IsInBounds(pt) && map.GetTileAt(pt).Model.IsWalkable)) {
            melee_damage_field[pt] = 2*a_max_dam;
            radius2.UnionWith(Direction.COMPASS.Select(dir => pt + dir));
          }
          foreach(Point pt in Enumerable.Range(0,16).Select(i=> a.Location.Position.RadarSweep(2, i)).Where(pt => map.IsInBounds(pt) && map.GetTileAt(pt).Model.IsWalkable && radius2.Contains(pt))) {
            melee_damage_field[pt] = a_max_dam;
          }
        } else {
          foreach(Point pt in Direction.COMPASS.Select(dir=>a.Location.Position+dir).Where(pt => map.IsInBounds(pt) && map.GetTileAt(pt).Model.IsWalkable)) {
            melee_damage_field[pt] = a_max_dam;
          }
        }
        // we can do melee attack damage field without FOV
        // FOV doesn't matter without a ranged attack
        // so we can work around a newly spawned actor not yet having a non-null FOV
        HashSet<Point> aFOV = (0<a.CurrentRangedAttack.Range ? a.Controller.FOV : null);
        if (null == aFOV) {
          foreach(Point pt in melee_damage_field.Keys) {
            if (ret.ContainsKey(pt)) ret[pt] += melee_damage_field[pt];
            else ret[pt] = melee_damage_field[pt];
          }
          continue;
        };
        // maximum melee damage: a.MeleeAttack(m_Actor).DamageValue
        // maximum ranged damage: a.CurrentRangedAttack.DamageValue
        Dictionary<Point,int> ranged_damage_field = new Dictionary<Point,int>();
        // we can do better than these
        if (RogueForm.Game.Rules.WillOtherActTwiceBefore(m_Actor, a)) {
          foreach(Point pt in aFOV) {
            if (pt == a.Location.Position) continue;
            int dist = Rules.GridDistance(pt, a.Location.Position);
            a_max_dam = a.RangedAttack(dist, m_Actor).DamageValue;
            if (dist <= a.CurrentRangedAttack.Range + 1) { 
              if (dist <= a.CurrentRangedAttack.Range) {
                ranged_damage_field[pt] = 2*a_max_dam;
              } else if (dist == a.CurrentRangedAttack.Range+1) {
                ranged_damage_field[pt] = a_max_dam;
              }
            }
          }
        } else {
          foreach(Point pt in aFOV) {
            if (pt == a.Location.Position) continue;
            int dist = Rules.GridDistance(pt, a.Location.Position);
            if (dist <= a.CurrentRangedAttack.Range) {
              ranged_damage_field[pt] = a_max_dam;
            }
          }
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
            if (!LOS.CanTraceFireLine(exp.Location,pt,tmp_blast.Radius)) continue;
	        if (damage_field.ContainsKey(pt)) damage_field[pt]+=tmp_blast.Damage[r];
	        else damage_field[pt] = tmp_blast.Damage[r];
            if (pt == m_Actor.Location.Position) in_blast_field = true;
          }
	    }
	  }
      return in_blast_field;
	}

    public abstract ActorAction GetAction(RogueGame game);

    // savegame support
    public virtual void OptimizeBeforeSaving() { }  // override this if there are memorized sensors

    // trading support
    protected bool HasEnoughFoodFor(int nutritionNeed)
    {
      if (!m_Actor.Model.Abilities.HasToEat) return true;
      if (null == m_Actor.Inventory || m_Actor.Inventory.IsEmpty) return false;
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      int num = 0;
      foreach (Item obj in m_Actor.Inventory.Items) {
        ItemFood tmpFood = obj as ItemFood;
        if (null == tmpFood) continue;
        num += tmpFood.NutritionAt(turnCounter);
        if (num >= nutritionNeed) return true;
      }
      return false;
    }

    protected ItemAmmo GetCompatibleAmmoItem(ItemRangedWeapon rw)
    {
      if (m_Actor.Inventory == null) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        ItemAmmo itemAmmo = obj as ItemAmmo;
        if (itemAmmo != null && itemAmmo.AmmoType == rw.AmmoType) return itemAmmo;
      }
      return null;
    }

    protected ItemRangedWeapon GetCompatibleRangedWeapon(ItemAmmo am)
    {
      if (m_Actor.Inventory == null) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        ItemRangedWeapon itemRangedWeapon = obj as ItemRangedWeapon;
        if (itemRangedWeapon != null && itemRangedWeapon.AmmoType == am.AmmoType)
          return itemRangedWeapon;
      }
      return null;
    }

    // close to the inverse of IsInterestingItem
    public bool IsTradeableItem(Item it)
    {
		Contract.Requires(null != it);
        if (it is ItemFood)
            {
            if (!m_Actor.Model.Abilities.HasToEat) return true;
            if (m_Actor.IsHungry) return false; 
            if (!HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
              return (it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
            return true;
            }
        if (it is ItemRangedWeapon)
            {
            if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return true;
            ItemRangedWeapon rw = it as ItemRangedWeapon;
            if (0 < rw.Ammo) return false;
            if (null != GetCompatibleAmmoItem(rw)) return false;
            return true;    // more work needed
            }
        if (it is ItemAmmo)
            {
            ItemAmmo am = it as ItemAmmo;
            if (GetCompatibleRangedWeapon(am) == null) return true;
            return m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
            }
        if (it is ItemMeleeWeapon)
            {
            if (m_Actor.Sheet.SkillTable.GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs.MARTIAL_ARTS) > 0) return true;   // martial artists+melee weapons needs work
            return m_Actor.CountItemQuantityOfType(typeof (ItemMeleeWeapon)) >= 2;
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

    protected ItemBodyArmor GetBestBodyArmor(Predicate<Item> fn)
    {
      if (m_Actor.Inventory == null) return null;
      int num1 = 0;
      ItemBodyArmor itemBodyArmor1 = null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (null != fn && !fn(obj)) continue;
        ItemBodyArmor itemBodyArmor2 = obj as ItemBodyArmor;
        if (null == itemBodyArmor2) continue;
        int num2 = itemBodyArmor2.Rating;
        if (num2 > num1) {
          num1 = num2;
          itemBodyArmor1 = itemBodyArmor2;
        }
      }
      return itemBodyArmor1;
    }

    protected ItemMeleeWeapon GetWorstMeleeWeapon()
    {
      if (m_Actor.Inventory == null) return null;
      int num1 = int.MaxValue;
      ItemMeleeWeapon ret = null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        ItemMeleeWeapon tmp = obj as ItemMeleeWeapon;
        if (null == tmp) continue;
        int num2 = (tmp.Model as ItemMeleeWeaponModel).Attack.Rating;
        if (num2 < num1) {
          num1 = num2;
          ret = tmp;
        }
      }
      return ret;
    }

    public bool IsInterestingItem(Item it)
    {
	  Contract.Requires(null != it);
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
        if (!HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
          return !(it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
        return false;
      }

      // XXX new dropping code should cope with food vs. full inventory
      // don't lose last inventory slot to non-food unless we have enough
//    if (m_Actor.Model.Abilities.HasToEat && m_Actor.Inventory.CountItems >= m_Actor.MaxInv-1 && !HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2)) return false;

      if (it is ItemRangedWeapon) {
        if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;
        if (1 <= m_Actor.CountItemsOfSameType(typeof(ItemRangedWeapon))) return false;  // XXX rules out AI gun bunnies
        if (!m_Actor.Inventory.Contains(it) && m_Actor.HasItemOfModel(it.Model)) return false;
        ItemRangedWeapon rw = it as ItemRangedWeapon;
        return rw.Ammo > 0 || GetCompatibleAmmoItem(rw) != null;
      }
      if (it is ItemAmmo)
      {
        ItemAmmo am = it as ItemAmmo;
        if (GetCompatibleRangedWeapon(am) == null) return false;
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      }
      if (it is ItemMeleeWeapon)
      { // better handling of martial arts requires better attack juggling in general
        if (m_Actor.Sheet.SkillTable.GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs.MARTIAL_ARTS) > 0) return false;
        if (2<=m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon))) {
          ItemMeleeWeapon weapon = GetWorstMeleeWeapon();
          return (weapon.Model as ItemMeleeWeaponModel).Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        if (1<= m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon)) && 1>= m_Actor.Inventory.MaxCapacity- m_Actor.Inventory.CountItems) {
          ItemMeleeWeapon weapon = m_Actor.GetBestMeleeWeapon();    // rely on OrderableAI doing the right thing
          return (weapon.Model as ItemMeleeWeaponModel).Attack.Rating < (it.Model as ItemMeleeWeaponModel).Attack.Rating;
        }
        return true;
      }
      if (it is ItemMedicine)
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      if (it is ItemBodyArmor) { 
        ItemBodyArmor armor = GetBestBodyArmor(null);
        if (null == armor) return true;
        return armor.Rating < (it as ItemBodyArmor).Rating;
      }
      if (it.IsUseless || it is ItemPrimedExplosive || m_Actor.IsBoredOf(it))
        return false;
      return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1);
    }
  }
}
