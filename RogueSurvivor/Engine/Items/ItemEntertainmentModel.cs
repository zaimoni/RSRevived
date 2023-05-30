// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemEntertainmentModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemEntertainmentModel : ItemModel
  {
    public readonly int Value;
    public readonly int BoreChance;

    public ItemEntertainmentModel(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int value, int boreChance, int stacking, string flavor)
      : base(_id, aName, theNames, imageID, flavor)
    {
      Value = value;
      BoreChance = boreChance;
      StackingLimit = stacking;
    }

    public override Item create() { return new ItemEntertainment(this); }
    public ItemEntertainment instantiate() { return new ItemEntertainment(this); }
  }
}
