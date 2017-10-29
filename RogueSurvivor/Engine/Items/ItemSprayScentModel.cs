// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayScentModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemSprayScentModel : ItemModel
  {
    public readonly int MaxSprayQuantity;
    public readonly Odor Odor;
    public readonly int Strength;

    public ItemSprayScentModel(string aName, string theNames, string imageID, int sprayQuantity, Odor odor, int strength, string flavor)
      : base(aName, theNames, imageID, DollPart.LEFT_HAND)
    {
      MaxSprayQuantity = sprayQuantity;
      Odor = odor;
      Strength = strength;
      FlavorDescription = flavor;
    }
  }
}
