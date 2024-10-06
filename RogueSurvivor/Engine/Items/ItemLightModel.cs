// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemLightModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  public sealed class ItemLightModel : ItemModel
  {
    public readonly int MaxBatteries;
    public readonly short FovBonus;
    public readonly string OutOfBatteriesImageID;

    public ItemLightModel(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, short fovBonus, int maxBatteries, string outOfBatteriesImageID, string flavor)
      : base(_id, aName, theNames, imageID, flavor, DollPart.LEFT_HAND, true)
    {
#if DEBUG
      if (string.IsNullOrEmpty(outOfBatteriesImageID)) throw new ArgumentNullException(nameof(outOfBatteriesImageID));
#endif
      FovBonus = fovBonus;
      MaxBatteries = maxBatteries;
      OutOfBatteriesImageID = outOfBatteriesImageID;
    }

    public override Item create() { return new ItemLight(this); }
    public ItemLight instantiate() { return new ItemLight(this); }
  }
}
