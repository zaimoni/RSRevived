﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Threading;
using Zaimoni.Data;
using djack.RogueSurvivor.Engine;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    /// <summary>
    /// actor.Model.Abilities.CanUseItems is assumed
    /// for actual use, item is assumed to be in inv
    /// </summary>
    public interface UsableItem
    {
        bool CouldUse(); // i.e., Legal, item-only
        bool CouldUse(Actor actor); // i.e. Legal, actor-dependent
        bool CanUse(Actor actor);   // i.e. Performable
        void Use(Actor actor, Inventory inv);
        string ReasonCantUse(Actor actor);
        bool UseBeforeDrop(Actor a);
        bool FreeSlotByUse(Actor a);
    }

    [Serializable]
    public struct Item_s    // for item memmory
    {
        public Gameplay.Item_IDs ModelID;
        public int QtyLike; // meaning depends on ModelID
        public uint Flags; // meaning depends on ModelID

        public Item_s(Gameplay.Item_IDs id, int qty, uint flags = 0)
        {
            ModelID = id;
            QtyLike = qty;
            Flags = flags;
        }

        public Model.Item Model { get => Gameplay.GameItems.From(ModelID); }
        public bool Consume() => 0 >= Interlocked.Decrement(ref QtyLike);
    }

    [Serializable]
  public class Item : INoun, Zaimoni.Serialization.ISerialize
    {
    public readonly Gameplay.Item_IDs ModelID;
    private int m_Quantity;
    public DollPart EquippedPart { get; private set; }

    public Data.Model.Item Model { get => Gameplay.GameItems.From(ModelID); }
    public virtual string ImageID { get => Model.ImageID; }
    public virtual Gameplay.Item_IDs InventoryMemoryID { get => ModelID;  }
    public virtual Item_s toStruct() => new Item_s(ModelID, m_Quantity);
    public virtual void toStruct(ref Item_s dest) {
        dest.ModelID = ModelID;
        dest.QtyLike = m_Quantity;
        dest.Flags = 0;
    }

    public string TheName {
      get {
        var model = Model;
        if (model.IsProper) return model.SingleName;
        if (m_Quantity > 1 || model.IsPlural) return model.PluralName.PrefixDefinitePluralArticle();
        return model.SingleName.PrefixDefiniteSingularArticle();
      }
    }
    public string UnknownPronoun { get => "it"; }

    public string AName {
      get {
        var model = Model;
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

    public Item(Model.Item model, int qty = 1)
    {
#if DEBUG
      if (0 >= qty) throw new ArgumentOutOfRangeException(nameof(qty)); // reddit/Brasz 2020-10-23
#endif
      ModelID = model.ID;
      m_Quantity = qty;
      EquippedPart = DollPart.NONE;
    }

#region implement Zaimoni.Serialization.ISerialize
    protected Item(Zaimoni.Serialization.DecodeObjects decode) {
        int tmp_int = 0;
        byte tmp_byte = 0;
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref tmp_int);
        ModelID = (Gameplay.Item_IDs)(tmp_int);
        // we intentionally save/load quantity even if not model-stackable
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref m_Quantity);
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref tmp_byte);
        EquippedPart = (DollPart)tmp_byte;
    }

    protected void save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (int)(ModelID));
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, m_Quantity);
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)EquippedPart);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => save(encode);
#endregion

    public void UnequippedBy(Actor actor, bool canMessage=true)
    {
      // a destroyed item won't be in inventory at this point
      if (IsEquipped) {  // other half of actor.CanUnequip(it) [precondition part is above]
        Unequip();
        actor.OnUnequipItem(this);
        if (canMessage) {
          actor.PlayersInLOS()?.RedrawPlayScreen(RogueGame.MakePanopticMessage(actor, RogueGame.VERB_UNEQUIP.Conjugate(actor), this));
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
      actor.PlayersInLOS()?.RedrawPlayScreen(RogueGame.MakePanopticMessage(actor, RogueGame.VERB_EQUIP.Conjugate(actor), this));
    }

    // thin wrappers
    public void DropAt(Map m, in Point pos) {   // used only in map generation
      var obj = m.GetMapObjectAt(pos) as ShelfLike;
      if (null != obj) obj.PutItemIn(this);
      else m.DropItemAt(this, in pos);
    }

    public override string ToString()
    {
      if (Model.IsStackable) return ModelID.ToString()+" ("+Quantity.ToString()+")";
      return ModelID.ToString();
    }
  }
}
