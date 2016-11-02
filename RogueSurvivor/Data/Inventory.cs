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
    private List<Item> m_Items;
    public int MaxCapacity { get; set; }    // Actor requires a public setter

    public IEnumerable<Item> Items {
      get {
        return m_Items;
      }
    }

    public int CountItems {
      get {
        return m_Items.Count;
      }
    }

    public Item this[int index]
    {
      get
      {
        if (index < 0 || index >= m_Items.Count) return null;
        return m_Items[index];
      }
    }

    public bool IsEmpty
    {
      get
      {
        return m_Items.Count == 0;
      }
    }

    public bool IsFull
    {
      get
      {
        return m_Items.Count >= MaxCapacity;
      }
    }

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

    public Inventory(int maxCapacity)
    {
      if (maxCapacity < 0) throw new ArgumentOutOfRangeException("maxCapacity < 0");
      MaxCapacity = maxCapacity;
      m_Items = new List<Item>(1);
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

    public bool AddAsMuchAsPossible(Item it, out int quantityAdded)
    {
      Contract.Requires(null!=it);
      int quantity = it.Quantity;
      int stackedQuantity;
      List<Item> itemsStackableWith = GetItemsStackableWith(it, out stackedQuantity);
      if (itemsStackableWith != null) {
        quantityAdded = 0;
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
        return true;
      }
      if (IsFull) {
        quantityAdded = 0;
        return false;
      }
      quantityAdded = it.Quantity;
      m_Items.Add(it);
      return true;
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
      if (--it.Quantity > 0) return;
      m_Items.Remove(it);
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
          if (objList == null) objList = new List<Item>(m_Items.Count);
          objList.Add(mItem);
          int val2 = mItem.Model.StackingLimit - mItem.Quantity;
          int num = Math.Min(it.Quantity - stackedQuantity, val2);
          stackedQuantity += num;
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

    public List<_T_> GetItemsByType<_T_>() where _T_ : Item
    {
      List<_T_> tList = (List<_T_>) null;
      Type type = typeof (_T_);
      foreach (Item mItem in m_Items)
      {
        if (mItem.GetType() == type)
        {
          if (tList == null)
            tList = new List<_T_>(m_Items.Count);
          tList.Add(mItem as _T_);
        }
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

    public int CountItemsMatching(Predicate<Item> fn)
    {
      int num = 0;
      foreach (Item mItem in m_Items)
      {
        if (fn(mItem))
          ++num;
      }
      return num;
    }

    public bool HasItemMatching(Predicate<Item> fn)
    {
      foreach (Item mItem in m_Items)
      {
        if (fn(mItem))
          return true;
      }
      return false;
    }
  }
}
