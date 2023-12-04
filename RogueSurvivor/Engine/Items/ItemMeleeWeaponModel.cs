// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemMeleeWeaponModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal sealed class ItemMeleeWeaponModel : ItemWeaponModel
  {
    public readonly bool IsFragile;

    // alpha10
    public readonly int ToolBashDamageBonus;
    public readonly float ToolBuildBonus;
    public bool IsTool { get { return ToolBashDamageBonus != 0 || ToolBuildBonus != 0; } }

    public ItemMeleeWeaponModel(Gameplay.Item_IDs _id, string aName, string imageID, Attack attack, string flavor, int bash, float build, bool is_artifact = false)
      : base(_id, aName, is_artifact ? aName : aName.Plural(true), imageID, attack, flavor, is_artifact)
    {
      ToolBashDamageBonus = bash;
      ToolBuildBonus = build;
    }

    public ItemMeleeWeaponModel(Gameplay.Item_IDs _id, string aName, string imageID, Attack attack, string flavor, int bash, float build, int stackingLimit, bool fragile)
      : base(_id, aName, aName.Plural(true), imageID, attack, flavor, false)
    {
      StackingLimit = stackingLimit;
      IsFragile = fragile;
      ToolBashDamageBonus = bash;
      ToolBuildBonus = build;
    }

    public Attack BaseMeleeAttack(in ActorSheet Sheet) {
      return new Attack(Attack.Kind, Attack.Verb, Attack.HitValue + Sheet.UnarmedAttack.HitValue, Attack.DamageValue + Sheet.UnarmedAttack.DamageValue, Attack.StaminaPenalty);
    }

    public bool IsMartialArts {
      get {
        if (Gameplay.Item_IDs.UNIQUE_FATHER_TIME_SCYTHE==ID) return true;
        if (Gameplay.Item_IDs.UNIQUE_FAMU_FATARU_KATANA==ID) return true;
        return false;
      }
    }

    public override ItemMeleeWeapon create() { return new ItemMeleeWeapon(this); }
  }
}
