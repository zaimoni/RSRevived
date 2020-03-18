// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Zaimoni.Data;
using djack.RogueSurvivor.Engine;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Data
{
#if PROTOTYPE
  [Serializable]
  internal struct ItemStruct    // for item memmory
  {
    public readonly Gameplay.GameItems.IDs ModelID;
    public readonly int QtyLike; // meaning depends on ModelID

    public ItemStruct(Gameplay.GameItems.IDs id, int qty)
    {
      ModelID = id;
      QtyLike = qty;
    }
  }
#endif

  [Serializable]
  internal class Item
  {
    private readonly int m_ModelID;
    private int m_Quantity;
    public DollPart EquippedPart { get; private set; }

    public ItemModel Model { get { return Models.Items[m_ModelID]; } }
    public virtual string ImageID { get { return Model.ImageID; } }

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

    public bool CanStackMore {
      get {
        ItemModel model = Model;
        return model.IsStackable && m_Quantity < model.StackingLimit;
      }
    }

    public bool IsEquipped { get { return EquippedPart != DollPart.NONE; } }
    public static bool notEquipped(Item it) { return DollPart.NONE == it.EquippedPart; }

    public void Equip() { EquippedPart = Model.EquipmentPart; }
    public void Unequip() { EquippedPart = DollPart.NONE; }
    public bool IsUnique { get { return Model.IsUnique; } }
#if DEAD_FUNC
    public bool IsForbiddenToAI { get { return Model.IsForbiddenToAI; } }
#endif
    public virtual bool IsUseless { get { return false; } }

    public Item(ItemModel model, int qty = 1)
    {
      m_ModelID = (int) model.ID;
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
        if (canMessage) {
          var game = RogueForm.Game;
          if (game.ForceVisibleToPlayer(actor)) game.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_UNEQUIP.Conjugate(actor), this));
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
      var game = RogueForm.Game;
      if (game.ForceVisibleToPlayer(actor)) game.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_EQUIP.Conjugate(actor), this));
    }

    // thin wrappers
    public void DropAt(Map m, in Point pos) {m.DropItemAt(this, in pos);} // this guaranteed non-null so non-null precondition ok

    public override string ToString()
    {
      if (Model.IsStackable) return Model.ID.ToString()+" ("+Quantity.ToString()+")";
      return Model.ID.ToString();
    }
  }
}
