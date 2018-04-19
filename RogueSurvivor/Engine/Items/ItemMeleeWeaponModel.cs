// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemMeleeWeaponModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemMeleeWeaponModel : ItemWeaponModel
  {
    public bool IsFragile { get; set; }

    public ItemMeleeWeaponModel(string aName, string imageID, Attack attack, string flavor, bool is_artifact = false)
      : base(aName, is_artifact ? aName : aName.Plural(true), imageID, attack, flavor, is_artifact)
    {
    }

    public Attack BaseMeleeAttack(ActorSheet Sheet) {
      return new Attack(Attack.Kind, Attack.Verb, Attack.HitValue + Sheet.UnarmedAttack.HitValue, Attack.DamageValue + Sheet.UnarmedAttack.DamageValue, Attack.StaminaPenalty);
    }

    public bool IsMartialArts {
      get {
        if (Gameplay.GameItems.IDs.UNIQUE_FATHER_TIME_SCYTHE==ID) return true;
        if (Gameplay.GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA==ID) return true;
        return false;
      }
    }

    public override Item create()
    {
      return new ItemMeleeWeapon(this);
    }

    public ItemMeleeWeapon instantiate()
    {
      return new ItemMeleeWeapon(this);
    }
  }
}
