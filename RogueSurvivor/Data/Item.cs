// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Item
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Item
  {
    private int m_ModelID;
    private int m_Quantity;
    private DollPart m_EquipedPart;

    public ItemModel Model
    {
      get
      {
        return Models.Items[this.m_ModelID];
      }
    }

    public virtual string ImageID
    {
      get
      {
        return this.Model.ImageID;
      }
    }

    public string TheName
    {
      get
      {
        ItemModel model = this.Model;
        if (model.IsProper)
          return model.SingleName;
        if (this.m_Quantity > 1 || model.IsPlural)
          return "some " + model.PluralName;
        return "the " + model.SingleName;
      }
    }

    public string AName
    {
      get
      {
        ItemModel model = this.Model;
        if (model.IsProper)
          return model.SingleName;
        if (this.m_Quantity > 1 || model.IsPlural)
          return "some " + model.PluralName;
        if (model.IsAn)
          return "an " + model.SingleName;
        return "a " + model.SingleName;
      }
    }

    public int Quantity
    {
      get
      {
        return this.m_Quantity;
      }
      set
      {
        this.m_Quantity = value;
        if (this.m_Quantity >= 0)
          return;
        this.m_Quantity = 0;
      }
    }

    public bool CanStackMore
    {
      get
      {
        ItemModel model = this.Model;
        if (model.IsStackable)
          return this.m_Quantity < model.StackingLimit;
        return false;
      }
    }

    public DollPart EquippedPart
    {
      get
      {
        return this.m_EquipedPart;
      }
      set
      {
        this.m_EquipedPart = value;
      }
    }

    public bool IsEquipped
    {
      get
      {
        return this.m_EquipedPart != DollPart.NONE;
      }
    }

    public bool IsUnique { get; set; }

    public bool IsForbiddenToAI { get; set; }

    public Item(ItemModel model)
    {
      this.m_ModelID = model.ID;
      this.m_Quantity = 1;
      this.m_EquipedPart = DollPart.NONE;
    }
  }
}
