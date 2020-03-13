// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemRangedWeaponModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemRangedWeaponModel : ItemWeaponModel
  {
    public readonly int MaxAmmo;
    public readonly AmmoType AmmoType;
    private ItemRangedWeapon? _rw;

    // alpha10
    public int RapidFireHit1Value { get { return Attack.Hit2Value; } }
    public int RapidFireHit2Value { get { return Attack.Hit3Value; } }
    public bool IsFireArm { get { return Attack.Kind == AttackKind.FIREARM; } }
    public bool IsBow { get { return Attack.Kind == AttackKind.BOW; } }

    public ItemRangedWeaponModel(string aName, string theNames, string imageID, Attack attack, int maxAmmo, AmmoType ammoType, string flavor, bool is_artifact = false)
      : base(aName, theNames, imageID, attack, flavor, is_artifact)
    {
      MaxAmmo = maxAmmo;
      AmmoType = ammoType;
    }

    public override Item create() { return new ItemRangedWeapon(this); }
    public ItemRangedWeapon instantiate() { return new ItemRangedWeapon(this); }

    public ItemRangedWeapon Example {   // don't use this in real inventories; this object needs to be C++ const but C# doesn't have const correctness
      get {
        return (_rw ?? (_rw = instantiate()));
      }
    }

  }
}
