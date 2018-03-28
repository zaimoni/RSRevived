// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBodyArmor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemBodyArmor : Item
  {
    private static readonly GameItems.IDs[] BAD_POLICE_OUTFITS = new GameItems.IDs[]{
      GameItems.IDs.ARMOR_HELLS_SOULS_JACKET,
      GameItems.IDs.ARMOR_FREE_ANGELS_JACKET
    };
    private static readonly GameItems.IDs[] GOOD_POLICE_OUTFITS = new GameItems.IDs[]{
      GameItems.IDs.ARMOR_POLICE_JACKET,
      GameItems.IDs.ARMOR_POLICE_RIOT
    };

    private const int MIN_GANG_ARMOR_ID = (int)GameItems.IDs.ARMOR_HELLS_SOULS_JACKET;
    private const int MIN_GANG_ID = (int)GameGangs.IDs.NONE+1;

    new public ItemBodyArmorModel Model { get {return base.Model as ItemBodyArmorModel; } }
    public int Protection_Hit { get { return Model.Protection_Hit; } }
    public int Protection_Shot { get { return Model.Protection_Shot; } }
    public int Encumbrance { get { return Model.Encumbrance; } }
    public int Weight { get { return Model.Weight; } }
    public int Rating { get { return Model.Rating; } }

    public ItemBodyArmor(ItemBodyArmorModel model)
      : base(model)
    {
    }

    // these five functions are actually functions of the body armor model.
    // Callers should not assume the current overoptimization from C-level converisons will hold indefinitely.
    // March 28 2018: armors that are !IsNeutral are guaranteed non-neutral to all factions that pay attention
    // to such things.
    public bool IsHostileForCops()
    {
      return 0 <= Array.IndexOf<GameItems.IDs>(BAD_POLICE_OUTFITS, Model.ID);
    }

    public bool IsFriendlyForCops()
    {
      return 0 <= Array.IndexOf<GameItems.IDs>(GOOD_POLICE_OUTFITS, Model.ID);
    }

    // Validity for these is enforced in GameItems.  The ordering of the armor ids was adjusted to make these valid.
    // these never were valid for non-gang members
    public bool IsHostileForBiker(GameGangs.IDs gangID)
    {
      return !IsNeutral && !IsFriendlyForBiker(gangID);
    }

    public bool IsFriendlyForBiker(GameGangs.IDs gangID)
    {
      return (int)Model.ID == (((int)gangID - MIN_GANG_ID) + MIN_GANG_ARMOR_ID);
    }

    public bool IsNeutral {
      get {
        int armor_index = (int)Model.ID- MIN_GANG_ARMOR_ID;
        return -GOOD_POLICE_OUTFITS.Length > armor_index;
      }
    }
  }
}
