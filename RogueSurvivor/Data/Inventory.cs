// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Inventory
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
      Contract.Requires(0 <= maxCapacity);
      MaxCapacity = maxCapacity;
    }

    public bool AddAll(Item it)
    {
      Contract.Requires(null!=it);
      int stackedQuantity;
      List<Item> itemsStackableWith = GetItemsStackableWith(it, out stackedQuantity);
      if (stackedQuantity == it.Quantity) {
        int quantity = it.Quantity;
        foreach (Item to in itemsStackableWith) {
          int addThis = Math.Min(to.Model.StackingLimit - to.Quantity, quantity);
          AddToStack(it, addThis, to);
          quantity -= addThis;
          if (quantity <= 0) break;
        }
        return true;
      }
      if (IsFull) return false;
      m_Items.Add(it);
      return true;
    }

    public int AddAsMuchAsPossible(Item it)
    {
      Contract.Requires(null!=it);
      int quantity = it.Quantity;
      int quantityAdded = 0;
      int stackedQuantity;
      List<Item> itemsStackableWith = GetItemsStackableWith(it, out stackedQuantity);
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
      Contract.Requires(null!=it);
      if (!IsFull) return true;
      int stackedQuantity;
      return GetItemsStackableWith(it, out stackedQuantity) != null;
    }

    public void RemoveAllQuantity(Item it)
    {
      m_Items.Remove(it);
    }

    public void Consume(Item it)
    {
      Contract.Requires(null!=it);
      if (0 >= --it.Quantity) m_Items.Remove(it);
    }

    private int AddToStack(Item from, int addThis, Item to)
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
      Contract.Requires(null!=it);
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

    public Item GetBestDestackable(ItemModel it)
    {
      Contract.Requires(null!=it);
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
      Contract.Requires(null!=it);
      return GetBestDestackable(it.Model);
    }

    public bool Contains(Item it)
    {
      return m_Items.Contains(it);
    }

    public Item GetFirstByModel(ItemModel model)
    {
      foreach (Item mItem in m_Items) {
        if (mItem.Model == model) return mItem;
      }
      return null;
    }

    public bool Has<_T_>() where _T_ : Item
    {
      return null != GetFirst<_T_>();
    }

    public _T_ GetFirst<_T_>() where _T_ : Item
    {
      foreach (Item it in m_Items) {
        if (it is _T_) return it as _T_;
      }
      return null;
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
        if (mItem is _T_) (tList ?? (tList = new List<_T_>())).Add(mItem as _T_);
      }
      return tList;
    }

/*
    public Item GetFirstMatching(Predicate<Item> fn)
    {
      foreach (Item mItem in m_Items)
      {
        if (fn(mItem))
          return mItem;
      }
      return (Item) null;
    }
*/

    public _T_ GetFirstMatching<_T_>(Predicate<_T_> fn) where _T_ : Item
    {
      foreach (Item obj in m_Items) {
        _T_ tmp = obj as _T_;
        if (tmp != null && (fn == null || fn(tmp))) return tmp;
      }
      return null;
    }

    public override string ToString()
    {
      string ret = "inv/"+m_Items.Count.ToString();
      foreach (Item obj in m_Items) {
        ret += "\n"+obj.ToString();
      }
      return ret;
    }
  }
}
