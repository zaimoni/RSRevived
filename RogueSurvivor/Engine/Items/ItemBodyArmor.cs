// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBodyArmor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemBodyArmor : Item
  {
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

    public bool IsHostileForCops()
    {
      return Array.IndexOf(GameFactions.BAD_POLICE_OUTFITS, Model.ID) >= 0;
    }

    public bool IsFriendlyForCops()
    {
      return Array.IndexOf(GameFactions.GOOD_POLICE_OUTFITS, Model.ID) >= 0;
    }

    public bool IsHostileForBiker(GameGangs.IDs gangID)
    {
      return Array.IndexOf(GameGangs.BAD_GANG_OUTFITS[(int) gangID], Model.ID) >= 0;
    }

    public bool IsFriendlyForBiker(GameGangs.IDs gangID)
    {
      return Array.IndexOf(GameGangs.GOOD_GANG_OUTFITS[(int) gangID], Model.ID) >= 0;
    }
  }
}
