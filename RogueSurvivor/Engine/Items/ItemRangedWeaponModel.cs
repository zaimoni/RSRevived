// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemRangedWeaponModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemRangedWeaponModel : ItemWeaponModel
  {
    private readonly int m_MaxAmmo;
    private readonly AmmoType m_AmmoType;

    public bool IsFireArm {
      get {
        return Attack.Kind == AttackKind.FIREARM;
      }
    }

    public bool IsBow {
      get {
        return Attack.Kind == AttackKind.BOW;
      }
    }

    public int MaxAmmo {
      get {
        return m_MaxAmmo;
      }
    }

    public AmmoType AmmoType {
      get {
        return m_AmmoType;
      }
    }

    public ItemRangedWeaponModel(string aName, string theNames, string imageID, Attack attack, int maxAmmo, AmmoType ammoType)
      : base(aName, theNames, imageID, attack)
    {
      m_MaxAmmo = maxAmmo;
      m_AmmoType = ammoType;
    }
  }
}
