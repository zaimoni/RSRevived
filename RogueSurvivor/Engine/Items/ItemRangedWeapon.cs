// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemRangedWeapon
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemRangedWeapon : ItemWeapon
  {
    new public ItemRangedWeaponModel Model { get {return base.Model as ItemRangedWeaponModel; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }

    private int m_Ammo;

    public int Ammo {
      get {
        return m_Ammo;
      }
      set {
        m_Ammo = value;
      }
    }

    public ItemRangedWeapon(ItemRangedWeaponModel model)
      : base(model)
    {
      m_Ammo = model.MaxAmmo;
    }

#if PROTOTYPE
    public override ItemStruct Struct { get { return new ItemStruct(Model.ID, m_Ammo); } }
#endif

    public override string ToString()
    {
      return Model.ID.ToString()+" ("+Ammo.ToString()+")";
    }
  }
}
