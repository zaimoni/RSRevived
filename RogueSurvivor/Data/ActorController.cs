// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorController
  {
    protected Actor m_Actor;

    public virtual void TakeControl(Actor actor)
    {
      m_Actor = actor;
    }

    public virtual void LeaveControl()
    {
      m_Actor = null;
    }

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

    public Dictionary<Point,int> VisibleMaximumDamage()
    {
      if (null == m_Actor) return null;
      if (null == m_Actor.Location.Map) return null;    // Duckman
      HashSet<Point> tmpFOV = FOV;  // virtual function call may be time-expensive so cache
      if (null == tmpFOV) return null;
      Dictionary<Point,int> ret = new Dictionary<Point,int>();
      foreach(Point tmp in tmpFOV) {
        if (tmp == m_Actor.Location.Position) continue;
        Actor a = m_Actor.Location.Map.GetActorAt(tmp);
        if (null == a) continue;
        if (!RogueForm.Game.Rules.IsEnemyOf(m_Actor,a)) continue;
        if (!a.CanActNextTurn) continue;
        HashSet<Point> aFOV = a.Controller.FOV;
        if (null == aFOV) continue;
        // maximum melee damage: a.CurrentMeleeAttack.DamageValue
        // maximum ranged damage: a.CurrentRangedAttack.DamageValue
        // we can do better than these
        if (RogueForm.Game.Rules.WillOtherActTwiceBefore(m_Actor, a)) {
          foreach(Point tmp2 in aFOV) {
            if (tmp2 == a.Location.Position) continue;
            int dist = Rules.GridDistance(tmp2,a.Location.Position);
            int max_dam = 0;
            if (1 == dist) {
               max_dam = 2*a.CurrentMeleeAttack.DamageValue;
            } else if (2 == dist) {
               max_dam = a.CurrentMeleeAttack.DamageValue;
            }
        
            if (dist <= a.CurrentRangedAttack.Range && max_dam < 2*a.CurrentRangedAttack.DamageValue) {
              max_dam = 2*a.CurrentRangedAttack.DamageValue;
            } else if (dist == a.CurrentRangedAttack.Range+1 && max_dam < a.CurrentRangedAttack.DamageValue) {
              max_dam = a.CurrentRangedAttack.DamageValue;
            }
            if (0 < max_dam) {
              if (ret.ContainsKey(tmp2)) ret[tmp2] += max_dam;
              else ret[tmp2] = max_dam;
            }
          }
        } else {
          foreach(Point tmp2 in aFOV) {
            if (tmp2 == a.Location.Position) continue;
            int dist = Rules.GridDistance(tmp2,a.Location.Position);
            int max_dam = 0;
            if (1 == dist) max_dam = a.CurrentMeleeAttack.DamageValue;
            if (dist <= a.CurrentRangedAttack.Range && max_dam < a.CurrentRangedAttack.DamageValue) {
              max_dam = a.CurrentRangedAttack.DamageValue;
            }
            if (0 < max_dam) {
              if (ret.ContainsKey(tmp2)) ret[tmp2] += max_dam;
              else ret[tmp2] = max_dam;
            }
          }
        }
      }
      if (0 == ret.Count) return null;
      return ret;
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
      if (it.IsForbiddenToAI || it is ItemSprayPaint || it is ItemTrap && (it as ItemTrap).IsActivated)
        return false;

      // only soldiers and civilians use grenades (CHAR guards are disallowed as a balance issue)
      if (Gameplay.GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.Model.ID && !(m_Actor.Controller is Gameplay.AI.CivilianAI) && !(m_Actor.Controller is Gameplay.AI.SoldierAI)) return false;

      // only civilians use stench killer
      if (Gameplay.GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.Model.ID && !(m_Actor.Controller is Gameplay.AI.CivilianAI)) return false;

      // police have implicit police trackers
      if (Gameplay.GameItems.IDs.TRACKER_POLICE_RADIO == it.Model.ID && (int)Gameplay.GameFactions.IDs.ThePolice == m_Actor.Faction.ID) return false;

      // note that CHAR guards and soldiers don't need to eat like civilians, so they would not be interested in food
      if (it is ItemFood)
      {
        if (!m_Actor.Model.Abilities.HasToEat) return false;
        if (m_Actor.IsHungry) return true;
        if (!HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
          return !(it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
        return false;
      }

      // XXX new dropping code should cope with food vs. full inventory
      // don't lose last inventory slot to non-food unless we have enough
//    if (m_Actor.Model.Abilities.HasToEat && m_Actor.Inventory.CountItems >= m_Actor.MaxInv-1 && !HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2)) return false;

      if (it is ItemRangedWeapon)
      {
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
        if (2<= m_Actor.CountItemQuantityOfType(typeof(ItemMeleeWeapon))) {
          ItemMeleeWeapon weapon = GetWorstMeleeWeapon();
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
