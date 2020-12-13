// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Inventory
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Linq;

using ItemAmmo = djack.RogueSurvivor.Engine.Items.ItemAmmo;
using ItemAmmoModel = djack.RogueSurvivor.Engine.Items.ItemAmmoModel;
using ItemRangedWeapon = djack.RogueSurvivor.Engine.Items.ItemRangedWeapon;
using ItemRangedWeaponModel = djack.RogueSurvivor.Engine.Items.ItemRangedWeaponModel;
using ItemTrap = djack.RogueSurvivor.Engine.Items.ItemTrap;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Inventory
  {
    private readonly List<Item> m_Items = new List<Item>(1);
    public int MaxCapacity { get; set; }    // Actor requires a public setter

    public IEnumerable<Item> Items { get { return m_Items; } }
    public int CountItems { get { return m_Items.Count; } }

    public Item? this[int index] {
      get {
        if (index < 0 || index >= m_Items.Count) return null;
        return m_Items[index];
      }
    }

    public bool IsEmpty { get { return m_Items.Count == 0; } }
    public bool IsFull { get { return m_Items.Count >= MaxCapacity; } }

    public Item? TopItem {
      get {
        var ub = m_Items.Count;
        if (0 >= ub) return null;
        return m_Items[ub - 1];
      }
    }

    public Item? BottomItem {
      get {
        if (m_Items.Count == 0) return null;
        return m_Items[0];
      }
    }

    // for debugging
    public Item? Slot0 { get { return this[0]; } }
    public Item? Slot1 { get { return this[1]; } }
    public Item? Slot2 { get { return this[2]; } }
    public Item? Slot3 { get { return this[3]; } }
    public Item? Slot4 { get { return this[4]; } }
    public Item? Slot5 { get { return this[5]; } }
    public Item? Slot6 { get { return this[6]; } }
    public Item? Slot7 { get { return this[7]; } }
    public Item? Slot8 { get { return this[8]; } }
    public Item? Slot9 { get { return this[9]; } }

    public Inventory(int maxCapacity)
    {
#if DEBUG
      if (0 >= maxCapacity) throw new ArgumentOutOfRangeException(nameof(maxCapacity),maxCapacity,"must be positive");
#endif
      MaxCapacity = maxCapacity;
    }

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    { // backstop.  Any zero-quantity items are to be eliminated.
      RepairZeroQty();
    }

    public bool AddAll(Item it)
    {
#if DEBUG
      if (0 >= it.Quantity) throw new InvalidOperationException("already zero");
#endif
      RejectContains(it, "already had ");
      var itemsStackableWith = GetItemsStackableWith(it, out int stackedQuantity);
      if (null != itemsStackableWith) { // also have 0<stackedQuantity
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
      RepairZeroQty();
      return true;
    }

    public int AddAsMuchAsPossible(Item it)
    {
#if DEBUG
      if (0 >= it.Quantity) throw new InvalidOperationException("already zero");
#endif
      RepairContains(it, "already had ");
      int quantity = it.Quantity;
      int quantityAdded = 0;
      var itemsStackableWith = GetItemsStackableWith(it, out int stackedQuantity);
      if (null != itemsStackableWith) {
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
        RepairZeroQty();
        return quantityAdded;
      }
      if (IsFull) {
        RepairZeroQty();
        return 0;
      }

      m_Items.Add(it);
      RepairZeroQty();
      return it.Quantity;
    }

    public bool CanAddAtLeastOne(Item it)
    {
      return !IsFull || HasAtLeastOneStackableWith(it);
    }

    public void Clear() { m_Items.Clear(); }

    public void RemoveAllQuantity(Item it) {
#if DEBUG
      if (!m_Items.Contains(it)) throw new InvalidOperationException("tracing");
#endif
      m_Items.Remove(it);
      RepairZeroQty();
    }

    public void Consume(Item it) {
#if DEBUG
      if (!m_Items.Contains(it)) throw new InvalidOperationException("tracing");
#endif
      if (0 >= --it.Quantity) m_Items.Remove(it);
      RepairZeroQty();
    }

    /// <returns>true if and only if the source inventory is now empty</returns>
    public bool Transfer(Item it, Inventory dest) {
#if DEBUG
      if (!Contains(it)) throw new InvalidOperationException("item not here");
#endif
      dest.RepairContains(it, "was already there: ");
      int quantity = it.Quantity;   // need initial value
      if (quantity != dest.AddAsMuchAsPossible(it)) {
        // inventory cross-linking?
#if DEBUG
        if (Contains(it) && dest.Contains(it)) throw new InvalidProgramException("need to un-crosslink inventories");
#endif
        return false;
      }
      RemoveAllQuantity(it);
      RepairZeroQty();
      dest.RepairZeroQty();
      RejectCrossLink(dest);
      return IsEmpty;
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

    public List<Item>? GetItemsStackableWith(Item it, out int stackedQuantity)
    {
      stackedQuantity = 0;
      if (!it.Model.IsStackable) return null;
      List<Item>? objList = null;
      foreach(Item mItem in m_Items) {
        if (mItem.Model == it.Model && mItem.CanStackMore && !mItem.IsEquipped) {
          (objList ??= new List<Item>()).Add(mItem);
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
      _T_? smallest = null;

      foreach (Item it in m_Items) {
        if (it is _T_ obj) {
          if (null == smallest || obj.Quantity < smallest.Quantity) smallest = obj;
        }
      }
      if (null == smallest) throw new InvalidOperationException("not found");
      return smallest;
    }

    public Item? GetBestDestackable(ItemModel it)    // alpha10 equivalent: GetSmallestStackByModel.  XXX \todo rename for legibility?
    {
      Item? obj = null;
      foreach (Item mItem in m_Items) {
        if (mItem.Model == it && (obj == null || mItem.Quantity < obj.Quantity))
          obj = mItem;
      }
      return obj;
    }

    // XXX thin forwarder
    public Item? GetBestDestackable(Item it) { return GetBestDestackable(it.Model); }

    public ItemAmmo? GetCompatibleAmmoItem(ItemRangedWeaponModel rw)  // XXX layering violation
    {
      return GetBestDestackable(Gameplay.GameItems.From((int)rw.AmmoType + (int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL))) as ItemAmmo;
    }

    public ItemAmmo? GetCompatibleAmmoItem(ItemRangedWeapon rw)  // XXX layering violation
    {
      return GetBestDestackable(Gameplay.GameItems.From((int)rw.AmmoType + (int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL))) as ItemAmmo;
    }

    // we prefer to return weapons that need reloading.
    public ItemRangedWeapon? GetCompatibleRangedWeapon(ItemAmmoModel am)
    {
      return GetFirst<ItemRangedWeapon>(it => it.AmmoType == am.AmmoType && it.Ammo < it.Model.MaxAmmo)
          ?? GetFirst<ItemRangedWeapon>(it => it.AmmoType == am.AmmoType);
    }

    public ItemRangedWeapon? GetCompatibleRangedWeapon(ItemAmmo am)
    {
      return GetCompatibleRangedWeapon(am.Model);
    }

    public ItemRangedWeapon? GetCompatibleRangedWeapon(Gameplay.GameItems.IDs am)
    {
      return GetCompatibleRangedWeapon(Gameplay.GameItems.From(am) as ItemAmmoModel);
    }

    public void UntriggerAllTraps() {
      foreach (Item obj in m_Items) {
        if (obj is ItemTrap trap && trap.IsTriggered) trap.IsTriggered = false;
      }
    }

    public int TrapsMaxDamageFor(Actor a)
    {
      int num = 0;
      if (a.Controller.IsEngaged) {
        foreach (var obj in m_Items) {
          if (obj is ItemTrap trap && !trap.IsSafeFor(a)) num += trap.Model.Damage;
        }
      } else {
        foreach (var obj in m_Items) {
          if (obj is ItemTrap trap && !trap.IsSafeFor(a) && !trap.WouldLearnHowToBypass(a)) num += trap.Model.Damage;
        }
      }
      return num;
    }

    public int TrapTurnsFor(Actor a)
    {
      const int CIVILIAN_STARVE = 2 * Actor.FOOD_HUNGRY_LEVEL;

      double trap_threat(ItemTrap trap) {
          var triggered = trap.TriggerChanceFor(a);
          if (0 >= triggered) return 0;
          var damage = Math.Min(trap.Model.Damage, a.HitPoints);
          if (0 >= damage) return 0;
          // handwavium: how debilitating is the injury expectation?
          var annoyance = (double)(damage)/a.HitPoints;
          annoyance *= annoyance;
          // a bear trap should be significantly worse than 3 standard 4-turn moves, but not an outright deterrent
          annoyance *= 30;  // 30 hp thinks beartrap is move cost 15 on damage alone
          var escape = trap.EscapeChanceFor(a);
          if (0 >= escape) return CIVILIAN_STARVE; // don't want to risk arithmetic overflow.  Automatic starvation should be decisive enough.
          if (100 > escape) annoyance += 2*(100.0/(100-escape) - 1);  // overestimate how much time we wasted escaping from this trap
          return annoyance;
      }

      double cost = 0;
      if (a.Controller.IsEngaged) {
        foreach (var obj in m_Items) {
          if (obj is ItemTrap trap && !trap.IsSafeFor(a)) {
            cost += trap_threat(trap);
            if (CIVILIAN_STARVE <= cost) return CIVILIAN_STARVE;
          }
        }
      } else {
        foreach (var obj in m_Items) {
          if (obj is ItemTrap trap && !trap.IsSafeFor(a) && !trap.WouldLearnHowToBypass(a)) {
            cost += trap_threat(trap);
            if (CIVILIAN_STARVE <= cost) return CIVILIAN_STARVE;
          }
        }
      }
      return (int)cost;
    }

    public bool Contains(Item it) { return m_Items.Contains(it); }

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
            // iterate over all stackable items to see if we can "steal" quantity from other stacks.
            // since we steal from the right, the leftmost not-full stacks will always be the biggest one.
            int ub = m_Items.Count;
            while (0 <= --ub) {
              Item stealFrom = m_Items[ub];
              if (0 >= stealFrom.Quantity) {
                m_Items.Remove(stealFrom);
                continue;
              }
              if (!stealFrom.CanStackMore) continue;
              int i = ub;
              while(0 <= --i) {
                Item mergeWith = m_Items[i];
                if (0 >= mergeWith.Quantity) {
                  m_Items.Remove(mergeWith);
                  if (0 > --ub) break;
                  continue;
                }
#if DEBUG
                if (mergeWith == stealFrom) throw new InvalidOperationException("duplicate items found");
#endif
                if (stealFrom.Model == mergeWith.Model && mergeWith.CanStackMore) {
                  int steal = Math.Min(mergeWith.Model.StackingLimit - mergeWith.Quantity, stealFrom.Quantity);
                  mergeWith.Quantity += steal;
                  if (0 >= (stealFrom.Quantity -= steal)) {
                    m_Items.Remove(stealFrom);
                    break;
                  }
                }
              }
            }
            RepairZeroQty();
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
      RepairZeroQty();
    }

    public bool HasModel(ItemModel model)
    {
      foreach (Item mItem in m_Items) if (mItem.Model == model) return true;
      return false;
    }

    public Item? GetFirstByModel(ItemModel model)
    {
      foreach (Item mItem in m_Items) if (mItem.Model == model) return mItem;
      return null;
    }

    public Item? GetFirstByModel<_T_>(ItemModel model, Predicate<_T_> ok) where _T_ : Item
    {
      foreach (Item mItem in m_Items) if (mItem.Model == model && mItem is _T_ it && ok(it)) return mItem;
      return null;
    }

    public int Count(ItemModel model)
    {
      int num = 0;
      foreach (Item mItem in m_Items) if (!mItem.IsUseless && mItem.Model == model) num++;
      return num;
    }

    public int CountQuantityOf(ItemModel model)
    {
      int num = 0;
      foreach (Item mItem in m_Items) if (mItem.Model == model) num += mItem.Quantity;
      return num;
    }

    public bool HasAtLeastFullStackOf(ItemModel it, int n)
    {
      if (IsEmpty) return false;
      if (it.IsStackable) return CountQuantityOf(it) >= n * it.StackingLimit;
      return Count(it) >= n;
    }

    public bool HasAtLeastFullStackOf(Item it, int n) { return HasAtLeastFullStackOf(it.Model, n); }

    public bool Has<_T_>() where _T_ : Item
    {
      return null != GetFirst<_T_>();
    }

    public bool Has<_T_>(Predicate<_T_> ok) where _T_ : Item
    {
      return null != GetFirst<_T_>(ok);
    }

    public _T_? GetFirst<_T_>() where _T_ : Item
    {
      foreach (Item it in m_Items) if (it is _T_ obj) return obj;
      return null;
    }

    public _T_? GetFirst<_T_>(Predicate<_T_> ok) where _T_ : Item
    {
      foreach (Item it in m_Items) if (it is _T_ obj && ok(obj)) return obj;
      return null;
    }

    public int CountType<_T_>() where _T_ : Item
    {
      int num = 0;
      foreach (Item it in m_Items) if (it is _T_) ++num;
      return num;
    }

    public int CountType<_T_>(Predicate<_T_> ok) where _T_ : Item
    {
      int num = 0;
      foreach (Item it in m_Items) if (it is _T_ obj && ok(obj)) ++num;
      return num;
    }

    public int CountQuantityOf<_T_>() where _T_ : Item
    {
      int num = 0;
      foreach (Item it in m_Items) if (it is _T_) num += it.Quantity;
      return num;
    }

    public bool Has(Gameplay.GameItems.IDs id)
    {
      return null != GetFirst(id);
    }

    public Item? GetFirst(Gameplay.GameItems.IDs id)
    {
      foreach (Item it in m_Items) if (id == it.Model.ID) return it;
      return null;
    }

    public List<_T_>? GetItemsByType<_T_>() where _T_ : Item
    {
      List<_T_>? tList = null;
      foreach (Item mItem in m_Items) if (mItem is _T_ it) (tList ??= new List<_T_>()).Add(it);
      return tList;
    }

    public List<_T_>? GetItemsByType<_T_>(Predicate<_T_> test) where _T_ : Item
    {
      List<_T_>? tList = null;
      foreach (Item mItem in m_Items) {
        if (mItem is _T_ it && test(it)) (tList ??= new List<_T_>()).Add(it);
      }
      return tList;
    }

    public _T_? Maximize<_T_, R>(Func<_T_, R> metric) where _T_ : Item where R:IComparable<R>
    {
      R num1 = (R)typeof(R).GetField("MinValue").GetValue(default(R));
      _T_? ret = default;
      foreach(var it in m_Items) {
         if (!(it is _T_ test)) continue;
         R num2 = metric(test);
         if (0<num2.CompareTo(num1)) {
           ret = test;
           num1 = num2;
         }
      }
      return ret;
    }

    public _T_? Minimize<_T_, R>(Predicate<_T_> ok,Func<_T_, R> metric) where _T_ : Item where R : IComparable<R>
    {
      R num1 = (R)typeof(R).GetField("MaxValue").GetValue(default(R));
      _T_? ret = default;
      foreach(var it in m_Items) {
         if (!(it is _T_ test) || !ok(test)) continue;
         R num2 = metric(test);
         if (0>num2.CompareTo(num1)) {
           ret = test;
           num1 = num2;
         }
      }
      return ret;
    }

    public _T_? GetFirstMatching<_T_>() where _T_ : Item    // XXX cf GetFirst
    {
      foreach (Item obj in m_Items) if (obj is _T_ tmp) return tmp;
      return null;
    }

    public _T_? GetFirstMatching<_T_>(Predicate<_T_> fn) where _T_ : Item    // XXX cf GetFirst
    {
      foreach (Item obj in m_Items) if (obj is _T_ tmp && fn(tmp)) return tmp;
      return null;
    }

    public _T_? GetBestMatching<_T_>(Predicate<_T_> ok, Func<_T_, _T_, bool> lt) where _T_ : Item
    {
      _T_? ret = null;
      foreach (Item obj in m_Items) {
        if (obj is _T_ tmp && ok(tmp)) {
          if (null == ret || lt(ret,tmp)) ret = tmp;
        }
      }
      return ret;
    }

    public string? _HasZeroQuantityOrDuplicate()
    {
      int i = m_Items.Count;
      while (0 <= --i) {
        if (0 >= m_Items[i].Quantity) return "zero qty";
        int j = i;
        while (0 <= --j) if (m_Items[i]==m_Items[j]) return "duplicate "+ m_Items[i];
      }
      return null;
    }

    public string? _HasMultiEquippedItems()
    {
      Span<int> seen = stackalloc int[(int)DollPart.HIP_HOLSTER]; // +1 for strict ub, -1 for systematic bias
      int i = m_Items.Count;
      while (0 <= --i) {
        var part = m_Items[i].EquippedPart;
        if (DollPart.NONE == part) continue;
#if REPAIR_MULTIEQUIP
        if (0 < seen[(int)part - 1]) {
          m_Items[i].Unequip();
          continue;
        }
#else
        if (0 < seen[(int)part - 1]) return "multi-equipped "+part;
#endif
        seen[(int)part - 1] = i + 1;
      }
      return null;
    }

    [Conditional("DEBUG")]
    private void _RejectZeroQty()
    {
      int i = m_Items.Count;
      while (0 <= --i) {
        if (0 >= m_Items[i].Quantity) throw new InvalidOperationException("zero qty: "+this);
        int j = i;
        while (0 <= --j) if (m_Items[i]==m_Items[j]) throw new InvalidOperationException("duplicate item "+m_Items[i]+": "+this);
      }
    }

    [Conditional("RELEASE")]
    private void _RepairZeroQty()
    {
      int i = m_Items.Count;
      while (0 <= --i) {
        if (0 >= m_Items[i].Quantity) {
          m_Items.RemoveAt(i);
          continue;
        }
        int j = i;
        while (0 <= --j) {
          if (m_Items[i]==m_Items[j]) {
            m_Items.RemoveAt(i);
            break;
          }
        }
      }
    }

    public void RepairZeroQty()
    {
      _RejectZeroQty();
      _RepairZeroQty();
    }

    [Conditional("DEBUG")]
    public void RejectCrossLink(Inventory other)
    {
      int ub = m_Items.Count;
      if (0 >= ub) return;
      int i = other.m_Items.Count;
      while (0 <= --i) {
        var other_it = other.m_Items[i];
        int j = ub;
        while (0 <= --j) if (other_it == m_Items[j]) throw new InvalidOperationException("cross-linked item "+other_it+": "+this+"\n\n"+other);
      }
    }

    public string? _HasCrossLink(Inventory other)
    { // 2020-09-15 Release Mode IL size 169 (0xa9)
      int ub = m_Items.Count;
      if (0 >= ub) return null;
      int i = other.m_Items.Count;
      while (0 <= --i) {
        var other_it = other.m_Items[i];
        int j = ub;
        while (0 <= --j) if (other_it == m_Items[j]) return "cross-linked item "+other_it+": "+this+"\n\n"+other;
      }
      return null;
    }

    [Conditional("RELEASE")]
    private void _RepairCrossLink(Inventory master)
    {
      if (0 >= m_Items.Count) return;
      int i = master.m_Items.Count;
      while (0 <= --i) {
        var other_it = master.m_Items[i];
        int j = m_Items.Count;
        while (0 <= --j) if (other_it == m_Items[j]) m_Items.RemoveAt(j);
      }
    }

    public void RepairCrossLink(Inventory master)
    {
      RejectCrossLink(master);
      _RepairCrossLink(master);
    }

    [Conditional("DEBUG")]
    public void RejectContains(Item it, string prefix_msg)
    {
      if (Contains(it)) throw new InvalidOperationException(prefix_msg + it);
    }

    [Conditional("RELEASE")]
    private void _RepairContains(Item it)
    {
      int i = m_Items.Count;
      while (0 <= --i) if (m_Items[i] == it) m_Items.RemoveAt(i);
    }

    public void RepairContains(Item it, string prefix_msg)
    {
      RejectContains(it, prefix_msg);
      _RepairContains(it);
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
