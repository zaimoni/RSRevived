// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemAmmo
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemAmmo : Item
  {
    private AmmoType m_AmmoType;

    public AmmoType AmmoType
    {
      get
      {
        return this.m_AmmoType;
      }
    }

    public ItemAmmo(ItemModel model)
      : base(model)
    {
      if (!(model is ItemAmmoModel))
        throw new ArgumentException("model is not a AmmoModel");
      ItemAmmoModel itemAmmoModel = model as ItemAmmoModel;
      this.m_AmmoType = itemAmmoModel.AmmoType;
      this.Quantity = itemAmmoModel.MaxQuantity;
    }
  }
}
