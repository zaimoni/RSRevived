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
    public int Protection_Hit { get; private set; }

    public int Protection_Shot { get; private set; }

    public int Encumbrance { get; private set; }

    public int Weight { get; private set; }

    public ItemBodyArmor(ItemModel model)
      : base(model)
    {
      if (!(model is ItemBodyArmorModel))
        throw new ArgumentException("model is not a BodyArmorModel");
      ItemBodyArmorModel itemBodyArmorModel = model as ItemBodyArmorModel;
            Protection_Hit = itemBodyArmorModel.Protection_Hit;
            Protection_Shot = itemBodyArmorModel.Protection_Shot;
            Encumbrance = itemBodyArmorModel.Encumbrance;
            Weight = itemBodyArmorModel.Weight;
    }

    public bool IsHostileForCops()
    {
      return Array.IndexOf<GameItems.IDs>(GameFactions.BAD_POLICE_OUTFITS, (GameItems.IDs)Model.ID) >= 0;
    }

    public bool IsFriendlyForCops()
    {
      return Array.IndexOf<GameItems.IDs>(GameFactions.GOOD_POLICE_OUTFITS, (GameItems.IDs)Model.ID) >= 0;
    }

    public bool IsHostileForBiker(GameGangs.IDs gangID)
    {
      return Array.IndexOf<GameItems.IDs>(GameGangs.BAD_GANG_OUTFITS[(int) gangID], (GameItems.IDs)Model.ID) >= 0;
    }

    public bool IsFriendlyForBiker(GameGangs.IDs gangID)
    {
      return Array.IndexOf<GameItems.IDs>(GameGangs.GOOD_GANG_OUTFITS[(int) gangID], (GameItems.IDs)Model.ID) >= 0;
    }
  }
}
