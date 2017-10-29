// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemEntertainmentModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemEntertainmentModel : ItemModel
  {
    public readonly int Value;
    public readonly int BoreChance;

    public ItemEntertainmentModel(string aName, string theNames, string imageID, int value, int boreChance, int stacking, string flavor)
      : base(aName, theNames, imageID, flavor)
    {
      Value = value;
      BoreChance = boreChance;
      StackingLimit = stacking;
    }
  }
}
