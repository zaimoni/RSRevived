// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayScentModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemSprayScentModel : ItemModel
  {
    public readonly int MaxSprayQuantity;
    public readonly Odor Odor;
    public readonly int Strength;

    public ItemSprayScentModel(Gameplay.GameItems.IDs _id, string aName, string theNames, string imageID, int sprayQuantity, Odor odor, int strength, string flavor)
      : base(_id, aName, theNames, imageID, flavor, DollPart.LEFT_HAND)
    {
      MaxSprayQuantity = sprayQuantity;
      Odor = odor;
      Strength = strength;
    }

    public override Item create() { return new ItemSprayScent(this); }
    public ItemSprayScent instantiate() { return new ItemSprayScent(this); }
  }
}
