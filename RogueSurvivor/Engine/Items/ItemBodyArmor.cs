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
      GameItems.IDs.ARMOR_FREE_ANGELS_JACKET,
      GameItems.IDs.ARMOR_HELLS_SOULS_JACKET
    };
    private static readonly GameItems.IDs[] GOOD_POLICE_OUTFITS = new GameItems.IDs[]{
      GameItems.IDs.ARMOR_POLICE_JACKET,
      GameItems.IDs.ARMOR_POLICE_RIOT
    };

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

    // these four are actually functions of the body armor model.
    public bool IsHostileForCops()
    {
      return 0 <= Array.IndexOf<GameItems.IDs>(BAD_POLICE_OUTFITS, Model.ID);
    }

    public bool IsFriendlyForCops()
    {
      return 0 <= Array.IndexOf<GameItems.IDs>(GOOD_POLICE_OUTFITS, Model.ID);
    }

    public bool IsHostileForBiker(GameGangs.IDs gangID)
    {
      return 0 <= Array.IndexOf<GameItems.IDs>(GameGangs.BAD_GANG_OUTFITS[(int) gangID], Model.ID);
    }

    public bool IsFriendlyForBiker(GameGangs.IDs gangID)
    {
      return 0 <= Array.IndexOf<GameItems.IDs>(GameGangs.GOOD_GANG_OUTFITS[(int) gangID], Model.ID);
    }
  }
}
