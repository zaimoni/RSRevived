// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFood
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemFood : Item
  {
    public int Nutrition { get; private set; }
    public bool IsPerishable { get; private set; }
    public WorldTime BestBefore { get; private set; }

    // if those groceries expire on day 100, they will not spoil until day 200(?!)
    public bool IsStillFreshAt(int turnCounter)
    {
      if (!IsPerishable) return true;
      return turnCounter < BestBefore.TurnCounter;
    }

    public bool IsExpiredAt(int turnCounter)
    {
      if (IsPerishable && turnCounter >= BestBefore.TurnCounter)
        return turnCounter < 2 * BestBefore.TurnCounter;
      return false;
    }

    public bool IsSpoiledAt(int turnCounter)
    {
      if (!IsPerishable) return false;
      return turnCounter >= 2 * BestBefore.TurnCounter;
    }

    public int NutritionAt(int turnCounter)
    {
      if (IsStillFreshAt(turnCounter)) return Nutrition;
      if (!IsExpiredAt(turnCounter)) return Nutrition / 3;
      return (2*Nutrition)/3;
    }

    public ItemFood(ItemFoodModel model)
      : base(model)
    {
      Nutrition = model.Nutrition;
      IsPerishable = false;
    }

    public ItemFood(ItemFoodModel model, int bestBefore)
      : base(model)
    {
      Contract.Requires(0<=bestBefore);
      Nutrition = model.Nutrition;
      BestBefore = new WorldTime(bestBefore);
      IsPerishable = true;
    }
  }
}
