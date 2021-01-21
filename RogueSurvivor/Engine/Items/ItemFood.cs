// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFood
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemFood : Item, UsableItem
    {
    public readonly int Nutrition;
    public readonly WorldTime? BestBefore;

    new public ItemFoodModel Model { get {return (base.Model as ItemFoodModel)!; } }
    public bool IsPerishable { get { return Model.IsPerishable; } }

    // if those groceries expire on day 100, they will not spoil until day 200(?!)
    public bool IsStillFreshAt(int turnCounter)
    {
      if (!IsPerishable) return true;
      return turnCounter < BestBefore.TurnCounter;
    }

    public bool IsExpiredAt(int turnCounter)
    {
      if (!IsPerishable) return false;
      int t0 = BestBefore.TurnCounter;
      return turnCounter >= t0 && turnCounter < 2*t0;
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

    public ItemFood(ItemFoodModel model, int qty=1) : base(model)
    {
#if DEBUG
      if (model.IsPerishable) throw new InvalidOperationException("wrong constructor");
#endif
      Nutrition = model.Nutrition;
      Quantity = qty;
    }

    public ItemFood(int bestBefore, ItemFoodModel model) : base(model)
    {
#if DEBUG
      if (0 > bestBefore) throw new InvalidOperationException("expired in past");
      if (!model.IsPerishable) throw new InvalidOperationException("wrong constructor");
#endif
      Nutrition = model.Nutrition;
      BestBefore = new WorldTime(bestBefore);
    }

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) { return a.Model.Abilities.HasToEat; }
    public bool CanUse(Actor a) { return CouldUse(a); }
    // disallowing dogs from eating canned food should be done at their level
    public void Use(Actor actor, Inventory inv) {
      const int FOOD_EXPIRED_VOMIT_CHANCE = 25;

      actor.SpendActionPoints();
      actor.LivingEat(actor.CurrentNutritionOf(this));
      inv.Consume(this); // does the "is in inventory check"
      if (Model == Gameplay.GameItems.CANNED_FOOD) {
        var emptyCan = new ItemTrap(Gameplay.GameItems.EMPTY_CAN);// alpha10 { IsActivated = true };
        emptyCan.Activate(actor);  // alpha10
        actor.Location.Drop(emptyCan);
      }
      bool player = RogueGame.Game.ForceVisibleToPlayer(actor);
      if (player) RogueGame.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_EAT.Conjugate(actor), this));
      if (!IsSpoiledAt(actor.Location.Map.LocalTime.TurnCounter) || !Rules.Get.RollChance(FOOD_EXPIRED_VOMIT_CHANCE)) return;
      actor.Vomit();
      if (player) RogueGame.AddMessage(RogueGame.MakeMessage(actor, string.Format("{0} from eating spoiled food!", RogueGame.VERB_VOMIT.Conjugate(actor))));
    }
    public string ReasonCantUse(Actor a) {
      if (!CouldUse(a)) return "no ability to eat";
      return "";
    }
    public bool UseBeforeDrop(Actor a) {
      // other behaviors handle pre-emptive eating of perishables
      if (IsPerishable) return false;
      if (!a.Inventory!.Contains(this) && a.HasEnoughFoodFor(a.Sheet.BaseFoodPoints / 2, this)) return false;
      int need = a.MaxFood - a.FoodPoints;
      int num4 = a.CurrentNutritionOf(this);
      return num4 <= need;
    }
    public bool FreeSlotByUse(Actor a) {
      // other behaviors handle pre-emptive eating of perishables
      if (IsPerishable) return false;
      if (!a.Inventory!.Contains(this) && a.HasEnoughFoodFor(a.Sheet.BaseFoodPoints / 2, this)) return false;
      int need = a.MaxFood - a.FoodPoints;
      int num4 = a.CurrentNutritionOf(this);
      return Quantity <= need/num4;
    }
#endregion

    public override string ToString()
    {
      return Model.ID.ToString()+(IsPerishable ? " (" + BestBefore.ToString() + ")" : "");
    }
  }
}
