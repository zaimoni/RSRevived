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
    private int m_MaxAmmo;
    private AmmoType m_AmmoType;

    public bool IsFireArm
    {
      get
      {
        return this.Attack.Kind == AttackKind.FIREARM;
      }
    }

    public bool IsBow
    {
      get
      {
        return this.Attack.Kind == AttackKind.BOW;
      }
    }

    public int MaxAmmo
    {
      get
      {
        return this.m_MaxAmmo;
      }
    }

    public AmmoType AmmoType
    {
      get
      {
        return this.m_AmmoType;
      }
    }

    public ItemRangedWeaponModel(string aName, string theNames, string imageID, Attack attack, int maxAmmo, AmmoType ammoType)
      : base(aName, theNames, imageID, attack)
    {
      this.m_MaxAmmo = maxAmmo;
      this.m_AmmoType = ammoType;
    }
  }
}
