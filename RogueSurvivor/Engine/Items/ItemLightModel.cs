﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemLightModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemLightModel : ItemModel
  {
    public readonly int MaxBatteries;
    public readonly short FovBonus;
    public readonly string OutOfBatteriesImageID;

    public ItemLightModel(string aName, string theNames, string imageID, short fovBonus, int maxBatteries, string outOfBatteriesImageID, string flavor)
      : base(aName, theNames, imageID, flavor, DollPart.LEFT_HAND)
    {
#if DEBUG
      if (string.IsNullOrEmpty(outOfBatteriesImageID)) throw new ArgumentNullException(nameof(outOfBatteriesImageID));
#endif
      FovBonus = fovBonus;
      MaxBatteries = maxBatteries;
      OutOfBatteriesImageID = outOfBatteriesImageID;
      DontAutoEquip = true;
    }

    public override Item create()
    {
      return new ItemLight(this);
    }

    public ItemLight instantiate()
    {
      return new ItemLight(this);
    }
  }
}
