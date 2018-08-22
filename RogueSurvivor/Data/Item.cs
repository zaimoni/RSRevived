// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Zaimoni.Data;
using Point = System.Drawing.Point;

namespace djack.RogueSurvivor.Data
{
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
        if (m_Quantity > 1 || model.IsPlural)
          return model.PluralName.PrefixDefinitePluralArticle();
        return model.SingleName.PrefixDefiniteSingularArticle();
      }
    }

    public string AName {
      get {
        ItemModel model = Model;
        if (model.IsProper) return model.SingleName;
        if (m_Quantity > 1 || model.IsPlural)
          return model.PluralName.PrefixIndefinitePluralArticle();
        return model.SingleName.PrefixIndefiniteSingularArticle();
      }
    }

    public int Quantity {
      get {
        return m_Quantity;
      }
      set {
#if DEBUG
        if (Model.StackingLimit < value ) throw new ArgumentOutOfRangeException(nameof(value),value,"exceeds "+Model.StackingLimit.ToString());
#endif
        m_Quantity = value;
        if (m_Quantity >= 0) return;
        m_Quantity = 0;
      }
    }

    public bool CanStackMore {
      get {
        ItemModel model = Model;
        if (model.IsStackable)
          return m_Quantity < model.StackingLimit;
        return false;
      }
    }

    public bool IsEquipped { get { return EquippedPart != DollPart.NONE; } }

    public void Equip()
    {   
      EquippedPart = Model.EquipmentPart;
    }

    public void Unequip()
    {   
      EquippedPart = DollPart.NONE;
    }

    public bool IsUnique { get { return Model.IsUnique; } }
#if DEAD_FUNC
    public bool IsForbiddenToAI { get { return Model.IsForbiddenToAI; } }
#endif
    public virtual bool IsUseless { get { return false; } }

    public Item(ItemModel model)
    {
#if DEBUG
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      m_ModelID = (int) model.ID;
      m_Quantity = 1;
      EquippedPart = DollPart.NONE;
    }

    public virtual ItemStruct Struct { get { return new ItemStruct(Model.ID, m_Quantity); } }

    public virtual void OptimizeBeforeSaving() { }  // alpha 10

    // thin wrappers
    public void DropAt(Map m, Point pos) {m.DropItemAt(this,pos);} // this guaranteed non-null so non-null precondition ok

    public override string ToString()
    {
      if (Model.IsStackable) return Model.ID.ToString()+" ("+Quantity.ToString()+")";
      return Model.ID.ToString();
    }
  }
}
