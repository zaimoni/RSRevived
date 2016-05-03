// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using System;

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
            m_Actor = (Actor) null;
    }

    public abstract ActorAction GetAction(RogueGame game);

    // savegame support
    public virtual void OptimizeBeforeSaving() { }  // override this if there are memorized sensors

    // trading support
    protected bool HasEnoughFoodFor(int nutritionNeed)
    {
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

    public bool IsInterestingItem(Item it)
    {
      if (it.IsForbiddenToAI || it is ItemSprayPaint || it is ItemTrap && (it as ItemTrap).IsActivated)
        return false;
      // note that CHAR guards and soldiers don't need to eat like civilians, so they would not be interested in food
      if (it is ItemFood)
      {
        if (m_Actor.IsHungry) return true;
        if (!HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2))
          return !(it as ItemFood).IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
        return false;
      }
      // don't lose last inventory slot to non-food unless we have enough
      if (m_Actor.Inventory.CountItems >= Rules.ActorMaxInv(m_Actor)-1 && !HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2)) return false;

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
      {
        if (m_Actor.Sheet.SkillTable.GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs.MARTIAL_ARTS) > 0) return false;
        return m_Actor.CountItemQuantityOfType(typeof (ItemMeleeWeapon)) < 2;
      }
      if (it is ItemMedicine)
        return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2);
      if (it.IsUseless || it is ItemPrimedExplosive || m_Actor.IsBoredOf(it))
        return false;
      return !m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1);
    }
  }
}
