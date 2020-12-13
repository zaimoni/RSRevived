// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemRangedWeapon
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemRangedWeapon : ItemWeapon
  {
    new public ItemRangedWeaponModel Model { get {return (base.Model as ItemRangedWeaponModel)!; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }

    public int Ammo;

    public ItemRangedWeapon(ItemRangedWeaponModel model) : base(model) { Ammo = model.MaxAmmo; }

#if PROTOTYPE
    public override Data.ItemStruct Struct { get { return new Data.ItemStruct(Model.ID, Ammo); } }
#endif

    // lambda function micro-optimizations for release-mode IL size.
    // Match the intended consuming type exactly, to avoid 7 release-mode IL byte overhead. 2020-09-29 zaimoni
    static public Predicate<ItemRangedWeapon> is_empty = rw => 0 >= rw.Ammo;
    static public Predicate<ItemRangedWeapon> is_not_empty = rw => 0 < rw.Ammo;

    static public ItemRangedWeapon make(Gameplay.GameItems.IDs x)
    {
      if (Gameplay.GameItems.From(x) is ItemRangedWeaponModel rw_model) return new ItemRangedWeapon(rw_model);
      throw new ArgumentOutOfRangeException(nameof(x), x, "not a ranged weapon");
    }

    public override string ToString()
    {
      return Model.ID.ToString()+" ("+Ammo.ToString()+")";
    }
  }
}
