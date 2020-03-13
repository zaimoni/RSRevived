// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemAmmo
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemAmmo : Item
  {
    new public ItemAmmoModel Model { get {return (base.Model as ItemAmmoModel)!; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }

    public ItemAmmo(ItemAmmoModel model)
      : base(model, model.MaxQuantity)
    {
    }

    static public ItemAmmo make(Gameplay.GameItems.IDs x)
    {
      ItemModel tmp = Models.Items[(int)x];
      if (tmp is ItemRangedWeaponModel rw_model) tmp = Models.Items[(int)(rw_model.AmmoType)+(int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL)];    // use the ammo of the ranged weapon instead
      if (tmp is ItemAmmoModel am_model) return new ItemAmmo(am_model);
      throw new ArgumentOutOfRangeException(nameof(x), x, "not ammunition or ranged weapon");
    }
  }
}
