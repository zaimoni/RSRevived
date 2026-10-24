// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFood
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemFood : Item, UsableItem, IJsonStaged<ItemFood>
    {
    /// <summary>A perishable food's expiry turn, and the rule for what follows it: expired (reduced nutrition) until twice that
    /// turn, spoiled after.  That rule is the legacy one, cf. the (?!) below; this is the place to replace it with a shelf life from
    /// the model.  Saved as the bare expiry turn, which is also how a WorldTime saves.</summary>
    public readonly record struct ShelfLife(int ExpiresAt)
    {
      public bool IsStillFreshAt(int turnCounter) => turnCounter < ExpiresAt;
      public bool IsExpiredAt(int turnCounter) => turnCounter >= ExpiresAt && turnCounter < 2 * ExpiresAt;
      public bool IsSpoiledAt(int turnCounter) => turnCounter >= 2 * ExpiresAt;
      public override string ToString() => new WorldTime(ExpiresAt).ToString();
    }

    public readonly ShelfLife? BestBefore;

    new public ItemFoodModel Model { get {return (base.Model as ItemFoodModel)!; } }
    public bool IsPerishable { get { return Model.IsPerishable; } }
    public int Nutrition { get { return Model.Nutrition; } }

    // if those groceries expire on day 100, they will not spoil until day 200(?!)
    public bool IsStillFreshAt(int turnCounter) => BestBefore?.IsStillFreshAt(turnCounter) ?? true;
    public bool IsExpiredAt(int turnCounter) => BestBefore?.IsExpiredAt(turnCounter) ?? false;
    public bool IsSpoiledAt(int turnCounter) => BestBefore?.IsSpoiledAt(turnCounter) ?? false;

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
      BestBefore = new(bestBefore);
    }

    private ItemFood(JsonElement src, JsonSerializerOptions options) : base(src, options) {
        RequireModel<ItemFoodModel>();
        if (src.TryGetProperty("BestBefore", out var relay)) BestBefore = new(relay.GetInt32());
        if (IsPerishable && BestBefore is null) throw new JsonException("perishable food without BestBefore: " + ModelID.ToString());
    }

    static ItemFood IJsonStaged<ItemFood>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) {
        base.toJsonFields(writer, options);
        if (BestBefore is { } life) writer.WriteNumber("BestBefore", life.ExpiresAt);
    }

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(LivingActor a) { return a.Model.Abilities.HasToEat; }
    public bool CanUse(LivingActor a) { return CouldUse(a); }
    // disallowing dogs from eating canned food should be done at their level
    public void Use(LivingActor actor, Inventory inv) {
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
      witnesses?.RedrawPlayScreen(RogueGame.MakePanopticMessage(actor, RogueGame.VERB_EAT.Conjugate(actor), this));
      if (!IsSpoiledAt(actor.Location.Map.LocalTime.TurnCounter) || !Rules.Get.RollChance(FOOD_EXPIRED_VOMIT_CHANCE)) return;
      actor.Vomit();
      witnesses?.RedrawPlayScreen(RogueGame.MakePanopticMessage(actor, string.Format("{0} from eating spoiled food!", RogueGame.VERB_VOMIT.Conjugate(actor))));
    }
    public string ReasonCantUse(LivingActor a) {
      if (!CouldUse(a)) return "no ability to eat";
      return "";
    }
    public bool UseBeforeDrop(LivingActor a) {
      // other behaviors handle pre-emptive eating of perishables
      if (IsPerishable) return false;
      if (!a.Inventory.Contains(this) && a.HasEnoughFoodFor(a.Model.BaseFoodPoints / 2, this)) return false;
      int need = a.MaxFood - a.FoodPoints;
      int num4 = a.CurrentNutritionOf(this);
      return num4 <= need;
    }
    public bool FreeSlotByUse(LivingActor a) {
      // other behaviors handle pre-emptive eating of perishables
      if (IsPerishable) return false;
      if (!a.Inventory.Contains(this) && a.HasEnoughFoodFor(a.Model.BaseFoodPoints / 2, this)) return false;
      int need = a.MaxFood - a.FoodPoints;
      int num4 = a.CurrentNutritionOf(this);
      return Quantity <= need/num4;
    }
#endregion

    public override Data.Item_s toStruct() {
      if (BestBefore is { } life) return new Data.Item_s(ModelID, life.ExpiresAt);
      return base.toStruct();
    }

    public override string ToString()
    {
      return ModelID.ToString()+(BestBefore is { } life ? " (" + life.ToString() + ")" : "");
    }
  }
}
