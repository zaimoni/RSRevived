// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBarricadeMaterialModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemBarricadeMaterialModel : ItemModel
  {
    public readonly int BarricadingValue;

    public ItemBarricadeMaterialModel(string aName, string theNames, string imageID, int barricadingValue, int stackingLimit, string flavor)
      : base(aName, theNames, imageID)
    {
      BarricadingValue = barricadingValue;
      StackingLimit = stackingLimit;
      FlavorDescription = flavor;
    }
  }
}
