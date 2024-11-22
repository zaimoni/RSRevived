// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBarricadeMaterialModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal sealed class ItemBarricadeMaterialModel : Data.Model.Item
  {
    public readonly int BarricadingValue;

    public ItemBarricadeMaterialModel(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int barricadingValue, int stackingLimit, string flavor)
      : base(_id, aName, theNames, imageID, flavor)
    {
      BarricadingValue = barricadingValue;
      StackingLimit = stackingLimit;
    }

    public override ItemBarricadeMaterial create() { return new ItemBarricadeMaterial(this); }
    public ItemBarricadeMaterial instantiate(int qty=1) { return new ItemBarricadeMaterial(this, qty); }
  }
}
