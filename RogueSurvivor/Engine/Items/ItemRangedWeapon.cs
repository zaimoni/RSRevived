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
    private int m_Ammo;
    private readonly AmmoType m_AmmoType;   // assuming this is a trade memory for speed issue

    public int Ammo {
      get {
        return m_Ammo;
      }
      set {
        m_Ammo = value;
      }
    }

    public AmmoType AmmoType {
      get {
        return m_AmmoType;
      }
    }

    public ItemRangedWeapon(ItemRangedWeaponModel model)
      : base(model)
    {
      m_Ammo = model.MaxAmmo;
      m_AmmoType = model.AmmoType;
    }
  }
}
