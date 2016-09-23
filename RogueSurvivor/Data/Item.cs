// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Item
  {
    private int m_ModelID;
    private int m_Quantity;
    public DollPart EquippedPart { get; private set; }

    public ItemModel Model {
      get {
        Contract.Requires(null!=Models.Items);
        Contract.Ensures(null!=Contract.Result<ItemModel>());
        return Models.Items[m_ModelID];
      }
    }

    public virtual string ImageID {
      get {
        return Model.ImageID;
      }
    }

    public string TheName {
      get {
        ItemModel model = Model;
        if (model.IsProper) return model.SingleName;
        if (m_Quantity > 1 || model.IsPlural)
          return "some " + model.PluralName;
        return "the " + model.SingleName;
      }
    }

    public string AName {
      get {
        ItemModel model = Model;
        if (model.IsProper) return model.SingleName;
        if (m_Quantity > 1 || model.IsPlural)
          return "some " + model.PluralName;
        if (model.IsAn)
          return "an " + model.SingleName;
        return "a " + model.SingleName;
      }
    }

    public int Quantity {
      get {
        return m_Quantity;
      }
      set {
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

    public bool IsEquipped {
      get {
        return EquippedPart != DollPart.NONE;
      }
    }

    public void Equip()
    {   
      EquippedPart = Model.EquipmentPart;
    }

    public void Unequip()
    {   
      EquippedPart = DollPart.NONE;
    }

    public bool IsUnique { get; set; }
    public bool IsForbiddenToAI { get; set; }

    public virtual bool IsUseless {
      get { return false; }
    }

    public Item(ItemModel model)
    {
      Contract.Requires(null!=model);
      m_ModelID = (int) model.ID;
      m_Quantity = 1;
      EquippedPart = DollPart.NONE;
    }
  }
}
