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
    public int Protection_Hit { get { return (Model as ItemBodyArmorModel).Protection_Hit; } }
    public int Protection_Shot { get { return (Model as ItemBodyArmorModel).Protection_Shot; } }
    public int Encumbrance { get { return (Model as ItemBodyArmorModel).Encumbrance; } }
    public int Weight { get { return (Model as ItemBodyArmorModel).Weight; } }
    public int Rating { get { return (Model as ItemBodyArmorModel).Rating; } }

    public ItemBodyArmor(ItemBodyArmorModel model)
      : base(model)
    {
      Contract.Requires(null != model);
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
