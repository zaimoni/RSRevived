// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Threading;
using Zaimoni.Data;
using djack.RogueSurvivor.Engine;

using Point = Zaimoni.Data.Vector2D<short>;
using System.Text.Json;
using Zaimoni.JSON;

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
  public class Item : INoun, IJsonStaged<Item>
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

    protected Item(JsonElement src, JsonSerializerOptions options) {
        src.RecordRef(this);
        ModelID = src.RequireEnum<Gameplay.Item_IDs>("Model");
        if (0 > (int)ModelID || (int)Gameplay.Item_IDs._COUNT <= (int)ModelID) throw new JsonException("not an item model: " + ModelID.ToString());
        m_Quantity = src.TryGetProperty("Qty", out var relay) ? relay.GetInt32() : 1;
        EquippedPart = src.TryGetProperty("Equipped", out _) ? src.RequireEnum<DollPart>("Equipped") : DollPart.NONE;
    }

    static Item IJsonStaged<Item>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    // Qty is implied (1) when not stackable; Equipped is implied when not equipped
    public virtual void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) {
        writer.WriteString("Model", ModelID.ToString());
        if (1 != m_Quantity || Model.IsStackable) writer.WriteNumber("Qty", m_Quantity);
        if (DollPart.NONE != EquippedPart) writer.WriteString("Equipped", EquippedPart.ToString());
    }

    /// <summary>JSON load: a subclass was paired with a model of the wrong kind</summary>
    protected void RequireModel<T>() where T : class {
        if (Model is not T) throw new JsonException(ModelID.ToString() + " is not a " + typeof(T).Name);
    }

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
      if (!actor.IsCarrying(this)) throw new ArgumentNullException("actor.Inventory?.Contains(this)");
#endif
      already?.UnequippedBy(actor);
      actor.Equip(this, true);
#if FAIL
      // postcondition: item is unequippable (but this breaks on merge)
      if (!Rules.CanActorUnequipItem(actor,this)) throw new ArgumentOutOfRangeException("equipped item cannot be unequipped","item type value: "+Model.ID.ToString());
#endif
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
