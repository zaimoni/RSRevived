// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Inventory
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

using ItemAmmo = djack.RogueSurvivor.Engine.Items.ItemAmmo;
using ItemAmmoModel = djack.RogueSurvivor.Engine.Items.ItemAmmoModel;
using ItemRangedWeapon = djack.RogueSurvivor.Engine.Items.ItemRangedWeapon;
using ItemRangedWeaponModel = djack.RogueSurvivor.Engine.Items.ItemRangedWeaponModel;
using ItemTrap = djack.RogueSurvivor.Engine.Items.ItemTrap;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Inventory
  {
    private readonly List<Item> m_Items = new List<Item>(1);
    public int MaxCapacity { get; set; }    // Actor requires a public setter

    public IEnumerable<Item> Items { get { return m_Items; } }
    public int CountItems { get { return m_Items.Count; } }

    public Item this[int index] {
      get {
        if (index < 0 || index >= m_Items.Count) return null;
        return m_Items[index];
      }
    }

    public bool IsEmpty { get { return m_Items.Count == 0; } }
    public bool IsFull { get { return m_Items.Count >= MaxCapacity; } }

    public Item TopItem {
      get {
        if (m_Items.Count == 0) return null;
        return m_Items[m_Items.Count - 1];
      }
    }

    public Item BottomItem {
      get {
        if (m_Items.Count == 0) return null;
        return m_Items[0];
      }
    }

    // for debugging
    public Item Slot0 { get { return this[0]; } }
    public Item Slot1 { get { return this[1]; } }
    public Item Slot2 { get { return this[2]; } }
    public Item Slot3 { get { return this[3]; } }
    public Item Slot4 { get { return this[4]; } }
    public Item Slot5 { get { return this[5]; } }
    public Item Slot6 { get { return this[6]; } }
    public Item Slot7 { get { return this[7]; } }
    public Item Slot8 { get { return this[8]; } }
    public Item Slot9 { get { return this[9]; } }

    public Inventory(int maxCapacity)
    {
#if DEBUG
      if (0 >= maxCapacity) throw new ArgumentOutOfRangeException(nameof(maxCapacity),maxCapacity,"must be positive");
#endif
      MaxCapacity = maxCapacity;
    }

    public bool AddAll(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      List<Item> itemsStackableWith = GetItemsStackableWith(it, out int stackedQuantity);
      if (0 < stackedQuantity) {
        int quantity = stackedQuantity;
        foreach (Item to in itemsStackableWith) {
          int addThis = Math.Min(to.Model.StackingLimit - to.Quantity, quantity);
          AddToStack(it, addThis, to);
          quantity -= addThis;
          it.Quantity -= addThis;
          if (quantity <= 0) break;
        }
        if (0 >= it.Quantity) return true;
      }
      if (IsFull) return false;
      m_Items.Add(it);
      return true;
    }

    public int AddAsMuchAsPossible(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      int quantity = it.Quantity;
      int quantityAdded = 0;
      List<Item> itemsStackableWith = GetItemsStackableWith(it, out int stackedQuantity);
      if (itemsStackableWith != null) {
        foreach (Item to in itemsStackableWith) {
          int stack = AddToStack(it, it.Quantity - quantityAdded, to);
          quantityAdded += stack;
        }
        if (quantityAdded < it.Quantity) {
          it.Quantity -= quantityAdded;
          if (!IsFull) {
            m_Items.Add(it);
            quantityAdded = quantity;
          }
        } else
          it.Quantity = 0;
        return quantityAdded;
      }
      if (IsFull) return 0;

      m_Items.Add(it);
      return it.Quantity;
    }

    public bool CanAddAtLeastOne(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (!IsFull) return true;
      return HasAtLeastOneStackableWith(it);
//    return GetItemsStackableWith(it, out int stackedQuantity) != null;
    }

    public void RemoveAllQuantity(Item it)
    {
      m_Items.Remove(it);
    }

    public void Consume(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (0 >= --it.Quantity) m_Items.Remove(it);
    }

    static private int AddToStack(Item from, int addThis, Item to)
    {
      int num = 0;
      for (; addThis > 0 && to.Quantity < to.Model.StackingLimit; --addThis) {
        ++to.Quantity;
        ++num;
      }
      return num;
    }

    public List<Item> GetItemsStackableWith(Item it, out int stackedQuantity)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      stackedQuantity = 0;
      if (!it.Model.IsStackable) return null;
      List<Item> objList = null;
      foreach (Item mItem in m_Items) {
        if (mItem.Model == it.Model && mItem.CanStackMore && !mItem.IsEquipped) {
          (objList ?? (objList = new List<Item>())).Add(mItem);
          stackedQuantity += Math.Min(it.Quantity - stackedQuantity, mItem.Model.StackingLimit - mItem.Quantity);
          if (stackedQuantity == it.Quantity) break;
        }
      }
      return objList;
    }

    bool HasAtLeastOneStackableWith(Item it) // alpha10
    {
      if (!it.Model.IsStackable) return false;

      foreach (Item other in m_Items) {
        if (other != it && other.Model == it.Model && other.CanStackMore && !other.IsEquipped) return true;
      }
      return false;
    }

    public _T_ GetSmallestStackOf<_T_>() where _T_ : Item   // alpha10 equivalent: GetSmallestStackByType
    {
      _T_ smallest = null;

      foreach (Item it in m_Items) {
        if (it is _T_ obj) {
          if (null == smallest || obj.Quantity < smallest.Quantity) {
            smallest = obj;
          }
        }
      }
      return smallest;
    }

    public Item GetBestDestackable(ItemModel it)    // alpha10 equivalent: GetSmallestStackByModel.  XXX \todo rename for legibility?
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      Item obj = null;
      foreach (Item mItem in m_Items) {
        if (mItem.Model == it && (obj == null || mItem.Quantity < obj.Quantity))
          obj = mItem;
      }
      return obj;
    }

    // XXX thin forwarder
    public Item GetBestDestackable(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      return GetBestDestackable(it.Model);
    }

    public ItemAmmo GetCompatibleAmmoItem(ItemRangedWeaponModel rw)  // XXX layering violation
    {
      return GetBestDestackable(Models.Items[(int)rw.AmmoType + (int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL)]) as ItemAmmo;
    }

    public ItemAmmo GetCompatibleAmmoItem(ItemRangedWeapon rw)  // XXX layering violation
    {
      return GetBestDestackable(Models.Items[(int)rw.AmmoType + (int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL)]) as ItemAmmo;
    }

    // we prefer to return weapons that need reloading.
    public ItemRangedWeapon GetCompatibleRangedWeapon(ItemAmmoModel am)
    {
#if DEBUG
      if (null == am) throw new ArgumentNullException(nameof(am));
#endif
      var rw = GetFirst<ItemRangedWeapon>(it => it.AmmoType == am.AmmoType && it.Ammo < it.Model.MaxAmmo);
      if (null != rw) return rw;
      return GetFirst<ItemRangedWeapon>(it => it.AmmoType == am.AmmoType);
    }

    public ItemRangedWeapon GetCompatibleRangedWeapon(ItemAmmo am)
    {
      return GetCompatibleRangedWeapon(am.Model);
    }

    public ItemRangedWeapon GetCompatibleRangedWeapon(Gameplay.GameItems.IDs am)
    {
      return GetCompatibleRangedWeapon(Models.Items[(int)am] as ItemAmmoModel);
    }

    public void UntriggerAllTraps() {
      foreach (Item obj in m_Items) {
        if (obj is ItemTrap trap && trap.IsTriggered) trap.IsTriggered = false;
      }
    }

    public bool Contains(Item it)
    {
      return m_Items.Contains(it);
    }

        // alpha10
        /// <summary>
        /// Defragment the inventory : consolidate smaller stacks into bigger ones. 
        /// Improves AI inventory management, not meant for the player as it can change the inventory and confuses him.
        /// TODO -- Maybe later add a special "defrag" command for the player to the interface, players would probably
        /// like to be able to merge stacks too.
        /// Don't abuse it, nice O^2...
        /// </summary>
        public void Defrag()
        {
            int countEmptyStacksToRemove = 0;

            // iterate over all stackable items to see if we can "steal" quantity from other stacks.
            // since we iterate from left to right and steal from the right, the leftmost stacks will always be
            // the biggest one.
            int n = m_Items.Count;
            for (int i = 0; i < n; i++) {
                Item mergeWith = m_Items[i];
                if (mergeWith.Quantity > 0 && mergeWith.CanStackMore) {
                    for (int j = i + 1; j < n && mergeWith.CanStackMore; j++) {
                        Item stealFrom = m_Items[j];
                        if (stealFrom.Model == mergeWith.Model && stealFrom.Quantity > 0) {
                            int steal = Math.Min(mergeWith.Model.StackingLimit - mergeWith.Quantity, stealFrom.Quantity);
                            mergeWith.Quantity += steal;
                            stealFrom.Quantity -= steal;
                            if (stealFrom.Quantity <= 0) countEmptyStacksToRemove++;
                        }
                    }
                }
            }

            // some smaller stacks might now be empty, delete them now.
            if (countEmptyStacksToRemove > 0) {
                int i = 0;
                do {
                    if (m_Items[i].Quantity <= 0) {
                        --countEmptyStacksToRemove;
                        m_Items.RemoveAt(i);
                    } else i++;
                }
                while (i < m_Items.Count && countEmptyStacksToRemove > 0);
            }
        }

    public void IncrementalDefrag(Item mergeWith) {
      if (0>=mergeWith.Quantity) return;    // arguably can just remove it but plausible callers were already doing so
      int i = m_Items.Count;
      while(0 < i-- && mergeWith.CanStackMore) {
        Item src = m_Items[i];
        if (0 >= src.Quantity) {    // failsafe for bugs elsewhere
          m_Items.RemoveAt(i);
          continue;
        }
        if (src == mergeWith) continue;
        if (src.Model != mergeWith.Model) continue;
        int realloc = Math.Min(mergeWith.Model.StackingLimit - mergeWith.Quantity, src.Quantity);
        mergeWith.Quantity += realloc;
        if (0 >= (src.Quantity -= realloc)) m_Items.RemoveAt(i);
      }
    }

    public bool HasModel(ItemModel model)
    {
      foreach (Item mItem in m_Items) {
        if (mItem.Model == model) return true;
      }
      return false;
    }

    public Item GetFirstByModel(ItemModel model)
    {
      foreach (Item mItem in m_Items) {
        if (mItem.Model == model) return mItem;
      }
      return null;
    }

    public Item GetFirstByModel<_T_>(ItemModel model, Predicate<_T_> ok) where _T_ : Item
    {
#if DEBUG
      if (null == ok) throw new ArgumentNullException(nameof(ok));
#endif
      foreach (Item mItem in m_Items) {
        if (mItem.Model == model && mItem is _T_ it && ok(it)) return mItem;
      }
      return null;
    }

    public int Count(ItemModel model)
    {
      int num = 0;
      foreach (Item mItem in m_Items) {
        if (mItem.IsUseless) continue;
        if (mItem.Model == model) num++;
      }
      return num;
    }

    public int CountQuantityOf(ItemModel model)
    {
      int num = 0;
      foreach (Item mItem in m_Items) {
        if (mItem.Model == model) num += mItem.Quantity;
      }
      return num;
    }

    public bool Has<_T_>() where _T_ : Item
    {
      return null != GetFirst<_T_>();
    }

    public _T_ GetFirst<_T_>() where _T_ : Item
    {
      foreach (Item it in m_Items) {
        if (it is _T_ obj) return obj;
      }
      return null;
    }

    public _T_ GetFirst<_T_>(Predicate<_T_> ok) where _T_ : Item
    {
      foreach (Item it in m_Items) {
        if (it is _T_ obj && ok(obj)) return obj;
      }
      return null;
    }

    public int CountType<_T_>() where _T_ : Item
    {
      int num = 0;
      foreach (Item it in m_Items) {
        if (it is _T_) ++num;
      }
      return num;
    }

    public int CountType<_T_>(Predicate<_T_> ok) where _T_ : Item
    {
      int num = 0;
      foreach (Item it in m_Items) {
        if (it is _T_ obj && ok(obj)) ++num;
      }
      return num;
    }


    public int CountQuantityOf<_T_>() where _T_ : Item
    {
      int num = 0;
      foreach (Item it in m_Items) {
        if (it is _T_) num += it.Quantity;
      }
      return num;
    }

    public bool Has(Gameplay.GameItems.IDs id)
    {
      return null != GetFirst(id);
    }

    public Item GetFirst(Gameplay.GameItems.IDs id)
    {
      foreach (Item it in m_Items) {
        if (id == it.Model.ID) return it;
      }
      return null;
    }

    public List<_T_> GetItemsByType<_T_>() where _T_ : Item
    {
      List<_T_> tList = null;
      foreach (Item mItem in m_Items) {
        if (mItem is _T_ it) (tList ?? (tList = new List<_T_>())).Add(it);
      }
      return tList;
    }

    public List<_T_> GetItemsByType<_T_>(Predicate<_T_> test) where _T_ : Item
    {
#if DEBUG
      if (null == test) throw new ArgumentNullException(nameof(test));
#endif
      List<_T_> tList = null;
      foreach (Item mItem in m_Items) {
        if (mItem is _T_ it && test(it)) (tList ?? (tList = new List<_T_>())).Add(it);
      }
      return tList;
    }

#if DEAD_FUNC
    public Item GetFirstMatching(Predicate<Item> fn)
    {
      foreach (Item mItem in m_Items)
      {
        if (fn(mItem))
          return mItem;
      }
      return (Item) null;
    }
#endif

    public _T_ GetFirstMatching<_T_>() where _T_ : Item
    {
      foreach (Item obj in m_Items) {
        if (obj is _T_ tmp) return tmp;
      }
      return null;
    }

    public _T_ GetFirstMatching<_T_>(Predicate<_T_> fn) where _T_ : Item
    {
      foreach (Item obj in m_Items) {
        if (obj is _T_ tmp && fn(tmp)) return tmp;
      }
      return null;
    }

    public _T_ GetBestMatching<_T_>(Predicate<_T_> ok, Func<_T_, _T_, bool> lt) where _T_ : Item
    {
      _T_ ret = null;
      foreach (Item obj in m_Items) {
        if (obj is _T_ tmp && ok(tmp)) {
          if (null == ret || lt(ret,tmp)) ret = tmp;
        };
      }
      return ret;
    }

    public void OptimizeBeforeSaving()  // alpha10
    {
      foreach (Item it in m_Items) it.OptimizeBeforeSaving();
    }

    public override string ToString()
    {
      var ret = new List<string>(1 + m_Items.Count) {
        "inv/"+m_Items.Count.ToString()
      };
      foreach (Item obj in m_Items) ret.Add(obj.ToString());
      return string.Join("\n",ret);
    }
  }
}
