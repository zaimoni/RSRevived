// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFoodModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemFoodModel : ItemModel
  {
    public readonly int Nutrition;
    public readonly bool IsPerishable;
    public readonly int BestBeforeDays;

    public ItemFoodModel(string aName, string theNames, string imageID, int nutrition, int bestBeforeDays, int stackingLimit, string flavor)
      : base(aName, theNames, imageID, flavor)
    {
      Nutrition = nutrition;
      if (bestBeforeDays < 0) {
        IsPerishable = false;
      } else {
        IsPerishable = true;
        BestBeforeDays = bestBeforeDays;
      }
      IsPlural = (aName==theNames);
      StackingLimit = stackingLimit;
    }

    public override Item create()
    {
      return new ItemFood(this);
    }

    public ItemFood instantiate()
    {
      return new ItemFood(this);
    }

    public ItemFood instantiate(int bestBefore)
    {
      return new ItemFood(this, bestBefore);
    }
  }
}
