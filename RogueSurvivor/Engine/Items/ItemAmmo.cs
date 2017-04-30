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
    new public ItemAmmoModel Model { get {return base.Model as ItemAmmoModel; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }

    public ItemAmmo(ItemAmmoModel model)
      : base(model)
    {
      Quantity = model.MaxQuantity;
    }
  }
}
