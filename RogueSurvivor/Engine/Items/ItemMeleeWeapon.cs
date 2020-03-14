// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemMeleeWeapon
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemMeleeWeapon : ItemWeapon
  {
    new public ItemMeleeWeaponModel Model { get {return (base.Model as ItemMeleeWeaponModel)!; } }
    public bool IsFragile { get { return Model.IsFragile; } }

    public ItemMeleeWeapon(ItemMeleeWeaponModel model)
      : base(model)
    {
    }
  }
}
