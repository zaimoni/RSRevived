// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFood
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Xml.Linq;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal sealed class ItemFood : Item, UsableItem, Zaimoni.Serialization.ISerialize
    {
    public readonly WorldTime? BestBefore;

    new public ItemFoodModel Model { get {return (base.Model as ItemFoodModel)!; } }
    public bool IsPerishable { get { return Model.IsPerishable; } }
    public int Nutrition { get { return Model.Nutrition; } }

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

    public ItemFood(ItemFoodModel model, int qty=1) : base(model, qty)
    {
#if DEBUG
      if (model.IsPerishable) throw new InvalidOperationException("wrong constructor");
#endif
    }

    public ItemFood(int bestBefore, ItemFoodModel model) : base(model)
    {
#if DEBUG
      if (0 > bestBefore) throw new InvalidOperationException("expired in past");
      if (!model.IsPerishable) throw new InvalidOperationException("wrong constructor");
#endif
      BestBefore = new WorldTime(bestBefore);
    }

#region implement Zaimoni.Serialization.ISerialize
    protected ItemFood(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        if (IsPerishable) BestBefore = decode.LoadInline<WorldTime>();
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        if (IsPerishable) Zaimoni.Serialization.ISave.InlineSave(encode, in BestBefore);
    }
#endregion

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
      var witnesses = actor.PlayersInLOS();
      if (null != witnesses) RogueGame.Game.RedrawPlayScreen(witnesses.Value, RogueGame.MakePanopticMessage(actor, RogueGame.VERB_EAT.Conjugate(actor), this));
      if (!IsSpoiledAt(actor.Location.Map.LocalTime.TurnCounter) || !Rules.Get.RollChance(FOOD_EXPIRED_VOMIT_CHANCE)) return;
      actor.Vomit();
      if (null != witnesses) RogueGame.Game.RedrawPlayScreen(witnesses.Value, RogueGame.MakePanopticMessage(actor, string.Format("{0} from eating spoiled food!", RogueGame.VERB_VOMIT.Conjugate(actor))));
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
      return ModelID.ToString()+(IsPerishable ? " (" + BestBefore.ToString() + ")" : "");
    }
  }
}
