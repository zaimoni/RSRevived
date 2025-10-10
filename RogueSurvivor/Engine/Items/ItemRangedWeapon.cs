// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemRangedWeapon
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Item_s = djack.RogueSurvivor.Data.Item_s;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemRangedWeapon : ItemWeapon, Zaimoni.Serialization.ISerialize
    {
    new public ItemRangedWeaponModel Model { get {return (base.Model as ItemRangedWeaponModel)!; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }

    public int Ammo;

    public ItemRangedWeapon(ItemRangedWeaponModel model, int qty) : base(model) { Ammo = qty; }
    public ItemRangedWeapon(ItemRangedWeaponModel model) : this(model, model.MaxAmmo) {}
#region implement Zaimoni.Serialization.ISerialize
    protected ItemRangedWeapon(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref Ammo);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, Ammo);
    }
#endregion

    public override Item_s toStruct() { return new Item_s(ModelID, Ammo); }
    public override void toStruct(ref Item_s dest)
    {
        dest.ModelID = ModelID;
        dest.QtyLike = Ammo;
        dest.Flags = 0;
    }

    public override Gameplay.Item_IDs InventoryMemoryID { get {
        return 0 == Ammo ? base.InventoryMemoryID.UnloadedVersion() : base.InventoryMemoryID;
    } }

    // lambda function micro-optimizations for release-mode IL size.
    // Match the intended consuming type exactly, to avoid 7 release-mode IL byte overhead. 2020-09-29 zaimoni
    static public Predicate<ItemRangedWeapon> is_empty = rw => 0 >= rw.Ammo;
    static public Predicate<ItemRangedWeapon> is_not_empty = rw => 0 < rw.Ammo;

    static public ItemRangedWeapon make(Gameplay.Item_IDs x)
    {
      if (Gameplay.GameItems.From(x) is ItemRangedWeaponModel rw_model) return new ItemRangedWeapon(rw_model);
      throw new ArgumentOutOfRangeException(nameof(x), x, "not a ranged weapon");
    }

    public override string ToString()
    {
      return ModelID.ToString()+" ("+Ammo.ToString()+")";
    }
  }
}

namespace djack.RogueSurvivor {

  static public partial class RS_ext
  {
        static internal Gameplay.Item_IDs UnloadedVersion(this Gameplay.Item_IDs x)
        {
            switch (x)
            {
                case Gameplay.Item_IDs.RANGED_ARMY_PISTOL: return Gameplay.Item_IDs.UNLOADED_ARMY_PISTOL;
                case Gameplay.Item_IDs.RANGED_ARMY_RIFLE: return Gameplay.Item_IDs.UNLOADED_ARMY_RIFLE;
                case Gameplay.Item_IDs.RANGED_HUNTING_CROSSBOW: return Gameplay.Item_IDs.UNLOADED_HUNTING_CROSSBOW;
                case Gameplay.Item_IDs.RANGED_HUNTING_RIFLE: return Gameplay.Item_IDs.UNLOADED_HUNTING_RIFLE;
                case Gameplay.Item_IDs.RANGED_PISTOL: return Gameplay.Item_IDs.UNLOADED_PISTOL;
                case Gameplay.Item_IDs.RANGED_KOLT_REVOLVER: return Gameplay.Item_IDs.UNLOADED_KOLT_REVOLVER;
                case Gameplay.Item_IDs.RANGED_PRECISION_RIFLE: return Gameplay.Item_IDs.UNLOADED_PRECISION_RIFLE;
                case Gameplay.Item_IDs.RANGED_SHOTGUN: return Gameplay.Item_IDs.UNLOADED_SHOTGUN;
                case Gameplay.Item_IDs.UNIQUE_SANTAMAN_SHOTGUN: return Gameplay.Item_IDs.UNLOADED_SANTAMAN_SHOTGUN;
                case Gameplay.Item_IDs.UNIQUE_HANS_VON_HANZ_PISTOL: return Gameplay.Item_IDs.UNLOADED_HANS_VON_HANZ_PISTOL;
                default: throw new ArgumentOutOfRangeException(nameof(x), x.ToString());
            }
        }
    }

}