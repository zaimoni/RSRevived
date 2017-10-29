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
    public readonly int MaxAmmo;
    public readonly AmmoType AmmoType;

    public bool IsFireArm { get { return Attack.Kind == AttackKind.FIREARM; } }
    public bool IsBow { get { return Attack.Kind == AttackKind.BOW; } }

    public ItemRangedWeaponModel(string aName, string theNames, string imageID, Attack attack, int maxAmmo, AmmoType ammoType, string flavor)
      : base(aName, theNames, imageID, attack, flavor)
    {
      MaxAmmo = maxAmmo;
      AmmoType = ammoType;
    }
  }
}
