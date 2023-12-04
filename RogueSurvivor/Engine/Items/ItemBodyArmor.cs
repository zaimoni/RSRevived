// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBodyArmor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal sealed class ItemBodyArmor : Item, Zaimoni.Serialization.ISerialize
    {
    private static readonly Item_IDs[] BAD_POLICE_OUTFITS = new[]{
      Item_IDs.ARMOR_HELLS_SOULS_JACKET,
      Item_IDs.ARMOR_FREE_ANGELS_JACKET
    };
    private static readonly Item_IDs[] GOOD_POLICE_OUTFITS = new[]{
      Item_IDs.ARMOR_POLICE_JACKET,
      Item_IDs.ARMOR_POLICE_RIOT
    };

    private const int MIN_GANG_ARMOR_ID = (int)Item_IDs.ARMOR_HELLS_SOULS_JACKET;
    private const int MIN_GANG_ID = (int)GameGangs.IDs.NONE+1;

    new public ItemBodyArmorModel Model { get {return (base.Model as ItemBodyArmorModel)!; } }
    public int Protection_Hit { get { return Model.Protection_Hit; } }
    public int Protection_Shot { get { return Model.Protection_Shot; } }
    public int Encumbrance { get { return Model.Encumbrance; } }
    public int Weight { get { return Model.Weight; } }
    public int Rating { get { return Model.Rating; } }
    public static int Rate(ItemBodyArmor armor) { return armor.Rating; }

    public ItemBodyArmor(ItemBodyArmorModel model) : base(model) {}

#region implement Zaimoni.Serialization.ISerialize
    protected ItemBodyArmor(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion

    // these five functions are actually functions of the body armor model.
    // Callers should not assume the current overoptimization from C-level converisons will hold indefinitely.
    // March 28 2018: armors that are !IsNeutral are guaranteed non-neutral to all factions that pay attention
    // to such things.
    public bool IsHostileForCops() => 0 <= Array.IndexOf(BAD_POLICE_OUTFITS, ModelID);
    public bool IsFriendlyForCops() => 0 <= Array.IndexOf(GOOD_POLICE_OUTFITS, ModelID);

    // Validity for these is enforced in GameItems.  The ordering of the armor ids was adjusted to make these valid.
    // these never were valid for non-gang members
    public bool IsHostileForBiker(GameGangs.IDs gangID)
    {
      return !IsNeutral && !IsFriendlyForBiker(gangID);
    }

    public bool IsFriendlyForBiker(GameGangs.IDs gangID)
    {
      return (int)ModelID == (((int)gangID - MIN_GANG_ID) + MIN_GANG_ARMOR_ID);
    }

    public bool IsNeutral {
      get {
        int armor_index = (int)ModelID- MIN_GANG_ARMOR_ID;
        return -GOOD_POLICE_OUTFITS.Length > armor_index;
      }
    }

    static public ItemBodyArmor make(GameGangs.IDs gangId)
    {
      switch (gangId) {
        case GameGangs.IDs.BIKER_HELLS_SOULS: return new ItemBodyArmor(GameItems.HELLS_SOULS_JACKET);
        case GameGangs.IDs.BIKER_FREE_ANGELS: return new ItemBodyArmor(GameItems.FREE_ANGELS_JACKET);
        default: throw new ArgumentOutOfRangeException(nameof(gangId), gangId, "not really a biker gang");
      }
    }
  }
}
