using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Data
{
  /// <summary>An actor whose model has an inventory (Abilities.HasInventory): the living, humans and dogs.  The undead,
  /// rats and the Sewers Thing are plain Actors.  Members marked override have a bridge on Actor, for callers that
  /// still hold an Actor; the bridge answers as an actor without an inventory.</summary>
  [Serializable]
  public sealed class LivingActor : Actor, IJsonStaged<LivingActor>
  {
    private readonly Inventory m_Inventory; // hammerspace inventory
    private readonly InventorySlots m_InventorySlots; // Angband-ish inventory

    public override Inventory Inventory => m_Inventory;
    public override InventorySlots InventorySlots => m_InventorySlots;

    public LivingActor(ActorModel model, Faction faction, int spawnTime, string name = "") : base(model, faction, spawnTime, name, true)
    {
#if DEBUG
      if (!model.Abilities.HasInventory) throw new ArgumentException(model.Name + " has no inventory: wants Actor", nameof(model));
#endif
      m_Inventory = new Inventory(model.BaseInventoryCapacity);
      m_InventorySlots = new InventorySlots(1);   // just human-torso slot, for now
      InstallDefaultController();
    }

    // Implied when omitted: an empty inventory of the model's capacity, empty slots.
    private LivingActor(JsonElement src, JsonSerializerOptions options) : base(src, options)
    {
      m_Inventory = src.TryGetProperty("Inventory", out var relay) ? (relay.Deserialize<Inventory>(options) ?? throw new JsonException("null Inventory"))
                  : new Inventory(Model.BaseInventoryCapacity);
      m_InventorySlots = src.TryGetProperty("Slots", out relay) ? new InventorySlots(relay, options) : new InventorySlots(1);
      FinishLoad(src, options);
    }

    public static LivingActor fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    protected override void toJsonInventory(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WritePropertyName("Inventory");
      JsonSerializer.Serialize(writer, m_Inventory, options);
      if (!m_InventorySlots.IsEmpty) {
        writer.WritePropertyName("Slots");
        m_InventorySlots.toJson(writer, options);
      }
    }

#region weapons and armor
    public override ItemMeleeWeapon? GetBestMeleeWeapon()
    {
      var tmp = m_Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (tmp is null) return null;
      int num1 = UnarmedMeleeAttack().Rating;
      ItemMeleeWeapon? itemMeleeWeapon1 = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        int num2 = MeleeWeaponAttack(obj.Model).Rating;
        if (num2 <= num1) continue;
        num1 = num2;
        itemMeleeWeapon1 = obj;
      }
      return itemMeleeWeapon1;
    }

    public override ItemMeleeWeapon? GetBestMeleeWeapon(MapObject toBreak)    // XXX don't actually use this parameter, just controls overload
    {
      var tmp = m_Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (tmp is null) return null;
      int num1 = UnarmedMeleeAttack().Rating;
      ItemMeleeWeapon? itemMeleeWeapon1 = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        int num2 = MeleeWeaponAttack(obj.Model, toBreak).Rating;
        if (num2 <= num1) continue;
        num1 = num2;
        itemMeleeWeapon1 = obj;
      }
      return itemMeleeWeapon1;
    }

    public override int? GetBestMeleeWeaponRating()
    {
      var tmp = m_Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (tmp is null) return null;
      int martial_arts_rating = UnarmedMeleeAttack().Rating;
      int? ret = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        int num2 = MeleeWeaponAttack(obj.Model).Rating;
        if (num2 <= martial_arts_rating) continue;
        if (ret is not null && num2 <= ret.Value) continue;
        ret = num2;
      }
      return ret;
    }

    public override ItemMeleeWeapon? GetWorstMeleeWeapon()
    {
      var melee = m_Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (melee is null) return null;
      if (1 == melee.Count) return melee[0];
      // some sort of invariant problem here
      // NOTE: martial arts influences the apparent rating considerably
      int rate(ItemMeleeWeapon w) { return MeleeWeaponAttack(w.Model).Rating; }

      return melee.Where(Item.notEquipped).Minimize(rate) ?? melee.Minimize(rate);
    }

    // the best body armor should be in the inventory slot
    public override ItemBodyArmor? GetBestBodyArmor() => m_InventorySlots[SLOT_H_TORSO] as ItemBodyArmor;
    // non-null return implies at least two body armors
    public override ItemBodyArmor? GetWorstBodyArmor() => m_Inventory.Minimize<ItemBodyArmor, int>(Item.notEquipped, ItemBodyArmor.Rate);

    public List<ItemRangedWeapon>? GetAvailableRangedWeapons()
    {
      if (Model.Abilities.AI_NotInterestedInRangedWeapons) return null;
      return m_Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || m_Inventory.GetCompatibleAmmoItem(rw) is not null);
    }

    public override List<ItemRangedWeapon>? ReadyRangedWeapons()
    {
      if (Model.Abilities.AI_NotInterestedInRangedWeapons) return null;
      return m_Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo);
    }

    // equipped items
    public override Item? GetEquippedItem(DollPart part)
    {
      if (DollPart.TORSO == part) return GetEquippedArmor();
      return m_Inventory.GetFirst<Item>(obj => obj.EquippedPart == part);
    }

    public override Item? GetEquippedItem(Gameplay.Item_IDs id) => m_Inventory.GetFirst<Item>(obj => obj.ModelID == id && DollPart.NONE != obj.EquippedPart);

    // considering these as change target
    public override ItemBodyArmor? GetEquippedArmor()
    {
      if (m_InventorySlots[SLOT_H_TORSO] is ItemBodyArmor armor) return armor;
      // legacy implementation
      return m_Inventory.GetFirst<ItemBodyArmor>(obj => obj.EquippedPart == DollPart.TORSO);
    }
#endregion

#region radios
    public override bool HasActiveCellPhone => m_Inventory.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader && !it.IsUseless) is not null;
    public override bool HasCellPhone => m_Inventory.GetFirstMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless) is not null;
    public override bool HasActivePoliceRadio => base.HasActivePoliceRadio || m_Inventory.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackPolice) is not null;  // charges on walking so won't stay useless
    public override bool HasPoliceRadio => base.HasPoliceRadio || m_Inventory.GetFirstMatching<ItemTracker>(it => it.CanTrackPolice) is not null;  // charges on walking so won't stay useless
    public override ItemTracker? GetEquippedCellPhone() => m_Inventory.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader);
#endregion

#region taking, having, counting
    public override bool CanTake(Item it) {
      if (it is ItemBodyArmor armor) {
        if (m_InventorySlots[SLOT_H_TORSO] is ItemBodyArmor worn) return worn.Rating < armor.Rating;
        return true;
      }
      if (it is ItemTrap trap && trap.IsTriggered) return false;
      return m_Inventory.CanAddAll(it);
    }

    private bool Take(ItemBodyArmor armor) {
      if (m_InventorySlots[SLOT_H_TORSO] is ItemBodyArmor worn) {
          // making player act like AI here, is not ideal.
          if (worn.Rating >= armor.Rating) goto final_exit;
          m_InventorySlots.RemoveAt(SLOT_H_TORSO);
          worn.UnequippedBy(this);
          if (!m_Inventory.AddAll(worn)) Location.Drop(worn);
      }
      if (m_InventorySlots.SetIfNull(SLOT_H_TORSO, armor)) {
          Equip(armor, Location.Map is not null);
          return true;
      }
final_exit:
      return m_Inventory.AddAll(armor);
    }

    public override bool Take(Item it) {
      if (it is ItemBodyArmor armor) return Take(armor);   // assume OrderableAI i.e. humanoid inventory, for now
      if (it is ItemTrap trap) trap.Desactivate();
      return m_Inventory.AddAll(it);
    }

    public override int TakeAsMuchAsPossible(Item it) {
      if (it is ItemBodyArmor armor) return Take(armor) ? 1 : 0;   // assume OrderableAI i.e. humanoid inventory, for now
      if (it is ItemTrap trap) trap.Desactivate();
      return m_Inventory.AddAsMuchAsPossible(it);
    }

    public override bool CanGetItems() => Model.Abilities.CanUseMapObjects;

    protected override string ReasonCantGet(Item it)
    {
      if (!CanGetItems()) return "no inventory";
      if (!CanTake(it)) {
        if (it is ItemTrap trap && trap.IsTriggered) return "triggered trap";
        if (m_Inventory.IsFull && !m_Inventory.CanAddAtLeastOne(it)) return "inventory is full";
      }
      return "";
    }

    protected override string ReasonCouldntBarricade()
    {
      if (!Model.Abilities.CanBarricade) return "no ability to barricade";
      if (m_Inventory.IsEmpty) return "no items";
      if (!m_Inventory.Has<ItemBarricadeMaterial>()) return "no barricading material";
      return "";
    }

    public override bool IsCarrying(Item it) => m_InventorySlots.Contains(it) || m_Inventory.Contains(it);
    public override bool IsCarrying(Gameplay.Item_IDs id) => m_InventorySlots.Contains(id) || m_Inventory.Contains(id);

    public override void Remove(Item it, bool canMessage=true)
    {
      if (m_InventorySlots.Remove(it)) {
        it.UnequippedBy(this, canMessage);
        return;
      }
      it.UnequippedBy(this, canMessage);
      m_Inventory.RemoveAllQuantity(it);
    }

    public override Item? GetItem(Gameplay.Item_IDs id) => m_Inventory.GetFirst(id);
    public bool HasItemOfModel(Model.Item model) => m_Inventory.HasModel(model);
    public override int Count(Model.Item model) => m_Inventory.Count(model);
    public override int CountQuantityOf<_T_>() => m_Inventory.CountQuantityOf<_T_>();

    public int CountItemsOfSameType(Type tt)
    {
      if (m_Inventory.IsEmpty) return 0;
      int num = 0;
      foreach (Item obj in m_Inventory) {
        if (obj.IsUseless) continue;
        if (obj.GetType() == tt) ++num;
      }
      foreach (Item obj in m_Inventory) {
        if (obj.IsUseless) continue;
        if (obj.GetType() == tt) ++num;
      }
      return num;
    }

    public override int CountItems<_T_>()
    {
      if (m_Inventory.IsEmpty) return 0;
      return m_Inventory.Where(it=>it is _T_).Select(it => it.Quantity).Sum();
    }

    public override bool Has<_T_>() => m_Inventory.Has<_T_>();

    public override bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
    {
      if (m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable) return m_Inventory.CountQuantityOf(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
    }

    public override bool HasAtLeastFullStackOf(Model.Item it, int n) => m_Inventory.HasAtLeastFullStackOf(it, n); // release mode IL 20h i.e. would inline
//  public bool HasAtLeastFullStackOf(Item it, int n) => HasAtLeastFullStackOf(it.Model, n); // manually inlined below
    public override bool HasAtLeastFullStackOf(Item it, int n) => m_Inventory.HasAtLeastFullStackOf(it.Model, n);

    public override bool HasEnoughFoodFor(int nutritionNeed, ItemFood? exclude=null)
    {
      if (!_has_to_eat) return true;
      if (m_Inventory.IsEmpty) return false;
      var tmp = m_Inventory.GetItemsByType<ItemFood>();
      if (tmp is null) return false;
      int turnCounter = Location.Map.LocalTime.TurnCounter;
//    int num = 0;
      int num = FoodPoints-FOOD_HUNGRY_LEVEL;
      if (num >= nutritionNeed) return true;
      foreach (ItemFood tmpFood in tmp) {
        if (exclude==tmpFood) continue;
        num += tmpFood.NutritionAt(turnCounter)*tmpFood.Quantity;
        if (num >= nutritionNeed) return true;
      }
      return false;
    }
#endregion

#region trading
    public override List<Item>? GetInterestingTradeableItems(Actor buyer) // called from RogueGame::PickItemToTrade so forced to be public no matter where
    {
#if DEBUG
      if (!Model.Abilities.CanTrade) throw new InvalidOperationException("cannot trade");
      if (!buyer.Model.Abilities.CanTrade) throw new InvalidOperationException("cannot trade");
#endif

      if (buyer.IsPlayer && IsPlayer) return m_Inventory.ToList();

      var buyer_ai = buyer.Controller as Gameplay.AI.ObjectiveAI;
      var ai = Controller as Gameplay.AI.ObjectiveAI;

      // IsInterestingTradeItem includes a charisma check i.e. RNG invocation, so cannot use .Any() prescreen safely
      var objList = m_Inventory.Where(it=> buyer_ai.IsInterestingTradeItem(this, it) && ai.IsTradeableItem(it)).ToList();  // \todo upgrade ObjectiveAI::IsTradeableItem to virtual with a PlayerController override
      return 0<objList.Count ? objList : null;
    }

    public override List<Item>? GetRationalTradeableItems(Gameplay.AI.OrderableAI buyer)    // only called from AI trading decision making
    {
#if DEBUG
      if (!Model.Abilities.CanTrade) throw new InvalidOperationException(Name+" cannot trade");
#endif

      var objList = m_Inventory.Where(it=> buyer.IsRationalTradeItem(it) && (Controller as Gameplay.AI.OrderableAI).IsTradeableItem(it));
      return objList.Any() ? objList.ToList() : null;
    }
#endregion

    public override void SkillUpgrade(Skills.IDs id)
    {
      base.SkillUpgrade(id);
      if (Skills.IDs.HAULER == id) m_Inventory.MaxCapacity = MaxInv;
    }

    public override void RecomputeStartingStats()
    {
      base.RecomputeStartingStats();
      m_Inventory.MaxCapacity = MaxInv;
    }
  }
}
