// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define USE_ITEM_STRUCT

using System;
using System.Threading;
using Zaimoni.Data;
using djack.RogueSurvivor.Engine;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    /// <summary>
    /// actor.Model.Abilities.CanUseItems is assumed
    /// for actual use, item is assumed to be in inv
    /// </summary>
    internal interface UsableItem
    {
        bool CouldUse(); // i.e., Legal, item-only
        bool CouldUse(Actor actor); // i.e. Legal, actor-dependent
        bool CanUse(Actor actor);   // i.e. Performable
        void Use(Actor actor, Inventory inv);
        string ReasonCantUse(Actor actor);
        bool UseBeforeDrop(Actor a);
        bool FreeSlotByUse(Actor a);
    }

#if USE_ITEM_STRUCT
    [Serializable]
    internal struct Item_s    // for item memmory
    {
        public Gameplay.GameItems.IDs ModelID;
        public int QtyLike; // meaning depends on ModelID
        public uint Flags; // meaning depends on ModelID

        public Item_s(Gameplay.GameItems.IDs id, int qty, uint flags = 0)
        {
            ModelID = id;
            QtyLike = qty;
            Flags = flags;
        }
    }
#endif

    [Serializable]
  internal class Item
  {
    private readonly Gameplay.GameItems.IDs m_ModelID;
    private int m_Quantity;
    public DollPart EquippedPart { get; private set; }

    public ItemModel Model { get { return Gameplay.GameItems.From(m_ModelID); } }
    public virtual string ImageID { get { return Model.ImageID; } }
    public virtual Gameplay.GameItems.IDs InventoryMemoryID { get { return m_ModelID; }  }

    public string TheName {
      get {
        ItemModel model = Model;
        if (model.IsProper) return model.SingleName;
        if (m_Quantity > 1 || model.IsPlural) return model.PluralName.PrefixDefinitePluralArticle();
        return model.SingleName.PrefixDefiniteSingularArticle();
      }
    }

    public string AName {
      get {
        ItemModel model = Model;
        if (model.IsProper) return model.SingleName;
        if (m_Quantity > 1 || model.IsPlural) return model.PluralName.PrefixIndefinitePluralArticle();
        return model.SingleName.PrefixIndefiniteSingularArticle();
      }
    }

    public int Quantity {
      get { return m_Quantity; }
      set {
#if DEBUG
        if (Model.StackingLimit < value ) throw new ArgumentOutOfRangeException(nameof(value),value,"exceeds "+Model.StackingLimit.ToString());
#endif
        m_Quantity = (0 < value ? value : 0);
      }
    }

    public bool Consume() => 0 >= Interlocked.Decrement(ref m_Quantity);
    public bool Transfer(Item dest, int qty) {
        Interlocked.Add(ref dest.m_Quantity, qty);
        return 0 >= Interlocked.Add(ref m_Quantity, -qty);
    }

    public int TopOffStack { get { return Model.StackingLimit - m_Quantity; } }
    public bool CanStackMore { get { return Model.CanStackMore(m_Quantity); } }
    public bool IsEquipped { get { return EquippedPart != DollPart.NONE; } }
    public static bool notEquipped(Item it) { return DollPart.NONE == it.EquippedPart; }

    public void Equip() { EquippedPart = Model.EquipmentPart; }
    public void Unequip() { EquippedPart = DollPart.NONE; }
    public bool IsUnique { get { return Model.IsUnique; } }
    public virtual bool IsUseless { get { return false; } }

    public Item(ItemModel model, int qty = 1)
    {
#if DEBUG
      if (0 >= qty) throw new ArgumentOutOfRangeException(nameof(qty)); // reddit/Brasz 2020-10-23
#endif
      m_ModelID = model.ID;
      m_Quantity = qty;
      EquippedPart = DollPart.NONE;
    }

#if PROTOTYPE
    public virtual ItemStruct Struct { get { return new ItemStruct(Model.ID, m_Quantity); } }
#endif

    public void UnequippedBy(Actor actor, bool canMessage=true)
    {
#if CPU_HOG
      if (!actor.Inventory?.Contains(this) ?? true) throw new ArgumentNullException("actor.Inventory?.Contains(this)");
#endif
      if (IsEquipped) {  // other half of actor.CanUnequip(it) [precondition part is above]
        Unequip();
        actor.OnUnequipItem(this);
        if (canMessage && RogueGame.Game.ForceVisibleToPlayer(actor)) {
          RogueGame.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_UNEQUIP.Conjugate(actor), this));
        }
      }
    }

    public void EquippedBy(Actor actor)
    {
      var already = actor.GetEquippedItem(Model.EquipmentPart);
      if (already == this) return;
#if DEBUG
      if (!actor.Inventory?.Contains(this) ?? true) throw new ArgumentNullException("actor.Inventory?.Contains(this)");
#endif
      already?.UnequippedBy(actor);
      actor.Equip(this);
#if FAIL
      // postcondition: item is unequippable (but this breaks on merge)
      if (!Rules.CanActorUnequipItem(actor,this)) throw new ArgumentOutOfRangeException("equipped item cannot be unequipped","item type value: "+Model.ID.ToString());
#endif
      if (RogueGame.Game.ForceVisibleToPlayer(actor))
        RogueGame.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_EQUIP.Conjugate(actor), this));
    }

    // thin wrappers
    public void DropAt(Map m, in Point pos) {   // used only in map generation
      var obj = m.GetMapObjectAt(pos);
      if (null != obj && obj.IsContainer) obj.PutItemIn(this);
      else m.DropItemAt(this, in pos);
    }

    public override string ToString()
    {
      if (Model.IsStackable) return Model.ID.ToString()+" ("+Quantity.ToString()+")";
      return Model.ID.ToString();
    }
  }
}
