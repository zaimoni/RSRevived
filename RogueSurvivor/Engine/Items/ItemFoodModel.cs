// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFoodModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal sealed class ItemFoodModel : ItemModel
  {
    public readonly int Nutrition;
    public readonly bool IsPerishable;
    public readonly int BestBeforeDays;

    public ItemFoodModel(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int nutrition, int bestBeforeDays, int stackingLimit, string flavor)
      : base(_id, aName, theNames, imageID, flavor)
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

    // while a case can be made for army rations to be permanent food, it's much easier to see cans not degrading quickly than the ration packaging.
    // RS 9 does not have a random component to their duration, unlike groceries
    public override Item create()
    {
      if (ID==Gameplay.Item_IDs.FOOD_ARMY_RATION) return new ItemFood(Session.Get.WorldTime.TurnCounter + WorldTime.TURNS_PER_DAY * BestBeforeDays, this);
      return new ItemFood(this);
    }

    public ItemFood instantiate()
    {
      if (ID==Gameplay.Item_IDs.FOOD_ARMY_RATION) return new ItemFood(Session.Get.WorldTime.TurnCounter + WorldTime.TURNS_PER_DAY * BestBeforeDays, this);
      return new ItemFood(this);
    }

    public ItemFood instantiate(int bestBefore) { return new ItemFood(bestBefore, this); }
  }
}
