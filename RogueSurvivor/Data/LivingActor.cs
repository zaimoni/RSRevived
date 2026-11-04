using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Engine;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

using Color = System.Drawing.Color;
using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;
using Fortification = djack.RogueSurvivor.Engine.MapObjects.Fortification;
using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Data
{
  /// <summary>An actor whose model has an inventory (Abilities.HasInventory): the living, humans and dogs.  The undead,
  /// rats and the Sewers Thing are plain Actors.  Equipment and attack reads, and communications, are virtuals of
  /// Actor overridden here; everything else that needs the inventory lives only here.</summary>
  [Serializable]
  public sealed class LivingActor : Actor, IInventory, IJsonStaged<LivingActor>
  {
    private readonly Inventory m_Inventory; // hammerspace inventory
    private readonly InventorySlots m_InventorySlots; // Angband-ish inventory
    private List<LivingActor>? m_Followers; // leadership fields are AI-specific (ObjectiveAI and dogs).
    private int m_TrustInLeader;
    private Dictionary<LivingActor,int>? m_TrustDict;
    [NonSerialized] private LivingActor? m_Leader;              // Backpointer

    public Inventory Inventory => m_Inventory;
    public InventorySlots InventorySlots => m_InventorySlots;

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
      m_TrustInLeader = src.TryGetProperty("TrustInLeader", out relay) ? relay.GetInt32() : 0;
      // Cross-links, filled when the load finishes.  Our leader is not saved: the leader's Followers sets it.
      if (src.TryGetProperty("Followers", out relay)) {
        m_Followers = new();
        foreach (var x in relay.EnumerateArray()) x.ReadRef<LivingActor>(fo => { m_Followers.Add(fo); fo.m_Leader = this; });
      }
      if (src.TryGetProperty("Trust", out relay)) relay.ReadRefDictionary(m_TrustDict = new(), options);
      FinishLoad(src, options);
    }

    public static LivingActor fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    protected override void toJsonLiving(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WritePropertyName("Inventory");
      JsonSerializer.Serialize(writer, m_Inventory, options);
      if (!m_InventorySlots.IsEmpty) {
        writer.WritePropertyName("Slots");
        m_InventorySlots.toJson(writer, options);
      }
      if (0 != m_TrustInLeader) writer.WriteNumber("TrustInLeader", m_TrustInLeader);
      // cross-links: other actors, who are saved by whatever owns them
      if (m_Followers is not null && 0 < m_Followers.Count) writer.WriteRefs("Followers", m_Followers);
      if (m_TrustDict is not null) {
        // The dead are skipped unless they still have a corpse: the corpse saves them, and they may be revived.  Otherwise nothing owns them.
        var live = m_TrustDict.Where(x => !x.Key.IsDead || (x.Key.Location.Map?.HasCorpseOf(x.Key) ?? false)).ToList();
        if (0 < live.Count) writer.WriteRefDictionary("Trust", live, options);
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

    public override ItemMeleeWeapon? GetBestMeleeWeapon(MapObject toBreak)
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

    public int? GetBestMeleeWeaponRating()
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


    public (List<T> all, T chosen)? ItemsAndChoice<T>(Func<List<T>, T> choose) where T:Item
    {
      var all = m_Inventory.GetItemsByType<T>();
      if (all is null) return null;
      if (1 == all.Count) return (all, all[0]);

      return (all, choose(all));
    }

    private ItemMeleeWeapon _chooseWorst(List<ItemMeleeWeapon> all) {
      int rate(ItemMeleeWeapon w) => MeleeWeaponAttack(w.Model).Rating;
      return all.Where(Item.notEquipped).Minimize(rate) ?? all.Minimize(rate);
    }
    public (List<ItemMeleeWeapon> all, ItemMeleeWeapon worst)? MeleeWeaponsAndWorst() => ItemsAndChoice<ItemMeleeWeapon>(_chooseWorst);

    private static ItemLight _chooseWorst(List<ItemLight> all) => all.Minimize(static obj => obj.Batteries);
    public (List<ItemLight> all, ItemLight worst)? LightsAndWorst() => ItemsAndChoice<ItemLight>(_chooseWorst);

    // the best body armor should be in the inventory slot
    public ItemBodyArmor? GetBestBodyArmor() => m_InventorySlots[SLOT_H_TORSO] as ItemBodyArmor;
    // non-null return implies at least two body armors
    public ItemBodyArmor? GetWorstBodyArmor() => m_Inventory.Minimize<ItemBodyArmor, int>(Item.notEquipped, ItemBodyArmor.Rate);

    public List<ItemRangedWeapon>? GetAvailableRangedWeapons()
    {
      if (Model.Abilities.AI_NotInterestedInRangedWeapons) return null;
      return m_Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || m_Inventory.GetCompatibleAmmoItem(rw) is not null);
    }

    public List<ItemRangedWeapon>? ReadyRangedWeapons()
    {
      if (Model.Abilities.AI_NotInterestedInRangedWeapons) return null;
      return m_Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo);
    }

    public ItemAmmo? SurplusAmmo() {
      foreach (var (_, model) in GameItems.ammo) {
        if (2 <= Count(model) && m_Inventory.GetBestDestackable(model) is ItemAmmo ammo) return ammo;
      }

      ItemAmmo? smallest = null;
      foreach (var (_, model) in GameItems.ammo) {
        if (    m_Inventory.GetBestDestackable(model) is ItemAmmo ammo
            && (smallest is null || smallest.Quantity > ammo.Quantity)) smallest = ammo;
      }
      return smallest;
    }

    // equipped items
    public override Item? GetEquippedItem(DollPart part)
    {
      if (DollPart.TORSO == part) return GetEquippedArmor();
      return m_Inventory.GetFirst<Item>(obj => obj.EquippedPart == part);
    }

    public Item? GetEquippedItem(Gameplay.Item_IDs id) => m_Inventory.GetFirst<Item>(obj => obj.ModelID == id && DollPart.NONE != obj.EquippedPart);

    // considering these as change target
    public override ItemBodyArmor? GetEquippedArmor()
    {
      if (m_InventorySlots[SLOT_H_TORSO] is ItemBodyArmor armor) return armor;
      // legacy implementation
      return m_Inventory.GetFirst<ItemBodyArmor>(obj => obj.EquippedPart == DollPart.TORSO);
    }

    public ItemBodyArmor? UsingCopArmor()
    {
      var ret = GetEquippedArmor();
      if (ret is not null && ret.IsFriendlyForCops()) return ret;
      return null;
    }

#endregion

#region radios
    public override bool HasActiveCellPhone => m_Inventory.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader && !it.IsUseless) is not null;
    public bool HasCellPhone => m_Inventory.GetFirstMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless) is not null;
    public override bool HasActivePoliceRadio => base.HasActivePoliceRadio || m_Inventory.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackPolice) is not null;  // charges on walking so won't stay useless
    public override bool HasPoliceRadio => base.HasPoliceRadio || m_Inventory.GetFirstMatching<ItemTracker>(it => it.CanTrackPolice) is not null;  // charges on walking so won't stay useless
    public override ItemTracker? GetEquippedCellPhone() => m_Inventory.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader);
#endregion

#region taking, having, counting
    public bool CanTake(Item it) {
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

    public bool Take(Item it) {
      if (it is ItemBodyArmor armor) return Take(armor);   // assume OrderableAI i.e. humanoid inventory, for now
      if (it is ItemTrap trap) trap.Desactivate();
      return m_Inventory.AddAll(it);
    }

    public int TakeAsMuchAsPossible(Item it) {
      if (it is ItemBodyArmor armor) return Take(armor) ? 1 : 0;   // assume OrderableAI i.e. humanoid inventory, for now
      if (it is ItemTrap trap) trap.Desactivate();
      return m_Inventory.AddAsMuchAsPossible(it);
    }

    public bool CanGetItems() => Model.Abilities.CanUseMapObjects;

    private string ReasonCantGet(Item it)
    {
      if (!CanGetItems()) return "no inventory";
      if (!CanTake(it)) {
        if (it is ItemTrap trap && trap.IsTriggered) return "triggered trap";
        if (m_Inventory.IsFull && !m_Inventory.CanAddAtLeastOne(it)) return "inventory is full";
      }
      return "";
    }

    private string ReasonCouldntBarricade()
    {
      if (!Model.Abilities.CanBarricade) return "no ability to barricade";
      if (m_Inventory.IsEmpty) return "no items";
      if (!m_Inventory.Has<ItemBarricadeMaterial>()) return "no barricading material";
      return "";
    }

    public bool IsCarrying(Item it) => m_InventorySlots.Contains(it) || m_Inventory.Contains(it);
    public bool IsCarrying(Gameplay.Item_IDs id) => m_InventorySlots.Contains(id) || m_Inventory.Contains(id);

    public void Remove(Item it, bool canMessage=true)
    {
      if (m_InventorySlots.Remove(it)) {
        it.UnequippedBy(this, canMessage);
        return;
      }
      it.UnequippedBy(this, canMessage);
      m_Inventory.RemoveAllQuantity(it);
    }

    public Item? GetItem(Gameplay.Item_IDs id) => m_Inventory.GetFirst(id);
    public bool HasItemOfModel(Model.Item model) => m_Inventory.HasModel(model);
    public int Count(Model.Item model) => m_Inventory.Count(model);
    public int CountQuantityOf<_T_>() where _T_ : Item => m_Inventory.CountQuantityOf<_T_>();

    public int CountItemsOfSameType(Type tt)
    {
      if (m_Inventory.IsEmpty) return 0;
      int num = 0;
      foreach (Item obj in m_Inventory) {
        if (obj.IsUseless) continue;
        if (obj.GetType() == tt) ++num;
      }
      return num;
    }

    public int CountItems<_T_>() where _T_ : Item
    {
      if (m_Inventory.IsEmpty) return 0;
      return m_Inventory.Where(it=>it is _T_).Select(it => it.Quantity).Sum();
    }

    public bool Has<_T_>() where _T_ : Item => m_Inventory.Has<_T_>();

    public bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
    {
      if (m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable) return m_Inventory.CountQuantityOf(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
    }

    public bool HasAtLeastFullStackOf(Model.Item it, int n) => m_Inventory.HasAtLeastFullStackOf(it, n); // release mode IL 20h i.e. would inline
//  public bool HasAtLeastFullStackOf(Item it, int n) => HasAtLeastFullStackOf(it.Model, n); // manually inlined below
    public bool HasAtLeastFullStackOf(Item it, int n) => m_Inventory.HasAtLeastFullStackOf(it.Model, n);

    public bool HasEnoughFoodFor(int nutritionNeed, ItemFood? exclude=null)
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

#region leadership
    public LivingActor? Leader { get => m_Leader; }
    public LivingActor? LiveLeader { get => (m_Leader is not null && !m_Leader.IsDead) ? m_Leader : null; }
    public bool HasLeader { get => !(m_Leader?.IsDead ?? true); }
    // null != LiveLeader is equivalent to HasLeader

    public int TrustInLeader {
      get => m_TrustInLeader;
      set { m_TrustInLeader = value; }  // \todo 4 change targets to eliminate public setter; CPU cost may be excessive
    }

    public bool IsTrustingLeader {
      get => HasLeader && TRUST_TRUSTING_THRESHOLD <= m_TrustInLeader;
    }

    public bool HasBondWith(LivingActor target)
    {
      if (m_Leader == target)      return TRUST_BOND_THRESHOLD <= m_TrustInLeader;
      if (target.m_Leader == this) return TRUST_BOND_THRESHOLD <= target.m_TrustInLeader;
      return false;
    }

    public void ModifyTrustInLeader(int mod, bool addMessage)
    {
      m_TrustInLeader += mod;
      if (m_TrustInLeader > Rules.TRUST_MAX) m_TrustInLeader = Rules.TRUST_MAX;
      else if (m_TrustInLeader < Rules.TRUST_MIN) m_TrustInLeader = Rules.TRUST_MIN;
      if (addMessage && Leader?.Controller is PlayerController pc)
        pc.AddMessage(new(string.Format("({0} trust with {1})", mod, TheName), Session.Get.WorldTime.TurnCounter, Color.White));
    }

    public IEnumerable<LivingActor>? Followers { get { return m_Followers; } }

    public LivingActor? FirstFollower(Predicate<LivingActor> test) {
      if (m_Followers is not null) foreach(var fo in m_Followers) if (test(fo)) return fo;
      return null;
    }

    public void DoForAllFollowers(Action<LivingActor> op)
    {
      if (m_Followers is not null) foreach(var fo in m_Followers) op(fo);
    }

    public int CountFollowers { get { return m_Followers?.Count ?? 0; } }

    public int MaxFollowers {
      get {
        return SKILL_LEADERSHIP_FOLLOWER_BONUS * MySkills.GetSkillLevel(Skills.IDs.LEADERSHIP);
      }
    }

    public List<KeyValuePair<int, LivingActor>>? DistantFollowers(int max_dist, int want_nearby)
    {
        if (m_Followers is null) return null;
        if (0 >= want_nearby) return null;

        List<KeyValuePair<int, LivingActor>> ret = new();
        int closeCount = 0;
        foreach (var follower in m_Followers) {
            int dist = Rules.InteractionDistance(follower.Location, Location);
            if (dist <= max_dist && ++closeCount >= want_nearby) return null;
            ret.Add(new(dist, follower));
        }
        return ret;
    }

    public string ReasonCannotBeLed()
    {
      if (Model.Abilities.IsUndead) return "undead";
      if (IsSleeping) return "sleeping";
      if (HasLeader) return "already has a leader";
      if (CountFollowers > 0) return "is a leader";  // XXX organized force would have a chain of command
      if (Controller.InCombat && !IsPlayer) return Name+" in combat";
      return "";
    }

    public string ReasonCannotLead()
    {
      int num = MaxFollowers;
      if (num == 0) return "can't lead";
      if (CountFollowers >= num) return "too many followers";
      if (HasLeader) return "already has a leader";
      // this should need refinement (range 1 might be ok)
      if (!IsPlayer && Controller.InCombat) return "in combat";
      return "";
    }

    public string ReasonCannotLead(Actor target)
    {
#if DEBUG
      if (target is null) throw new ArgumentNullException(nameof(target));
#endif
      if (IsEnemyOf(target)) return "enemy";
      if (Faction != target.Faction && target.Faction.LeadOnlyBySameFaction) return string.Format("{0} can't lead {1}", Faction.Name, target.Faction.Name);
      // to support savefile hacking.  AI in charge of player is a problem.
      if (target.IsPlayer && !IsPlayer) return "is player";
      return "";
    }

    private string ReasonCantTakeLeadOf(LivingActor target)
    {
#if DEBUG
      if (target is null) throw new ArgumentNullException(nameof(target));
#endif
      var ret = target.ReasonCannotBeLed();
      if (!string.IsNullOrEmpty(ret)) return ret;
      ret = ReasonCannotLead();
      if (!string.IsNullOrEmpty(ret)) return ret;
      return ReasonCannotLead(target);
    }

    public bool CanTakeLeadOf(LivingActor target, out string reason)
    {
      reason = ReasonCantTakeLeadOf(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanTakeLeadOf(LivingActor target)
    {
      return string.IsNullOrEmpty(ReasonCantTakeLeadOf(target));
    }

    private string ReasonIllegalToBeLed()
    {
      if (Model.Abilities.IsUndead) return "undead";
      if (IsSleeping) return "sleeping";
      if (HasLeader) return "already has a leader";
      if (CountFollowers > 0) return "is a leader";  // XXX organized force would have a chain of command
      return "";
    }

    private string ReasonIllegalToLead()
    {
      int num = MaxFollowers;
      if (num == 0) return "can't lead";
      if (CountFollowers >= num) return "too many followers";
      // this should need refinement (range 1 might be ok)
      return "";
    }

    private string ReasonIllegalToLead(LivingActor target)
    {
#if DEBUG
      if (target is null) throw new ArgumentNullException(nameof(target));
#endif
      var ret = target.ReasonIllegalToBeLed();
      if (!string.IsNullOrEmpty(ret)) return ret;
      ret = ReasonIllegalToLead();
      if (!string.IsNullOrEmpty(ret)) return ret;
      return ReasonCannotLead(target);
    }

    public bool LegalToTakeLeadOf(LivingActor target, out string reason)
    {
      reason = ReasonIllegalToLead(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool LegalToTakeLeadOf(LivingActor target)
    {
      return string.IsNullOrEmpty(ReasonIllegalToLead(target));
    }

    private string ReasonCantCancelLead(LivingActor target)
    {
      if (target.Leader != this) return "not your follower";
      if (target.IsSleeping) return "sleeping";
      return "";
    }

    public bool CanCancelLead(LivingActor target, out string reason)
    {
      reason = ReasonCantCancelLead(target);
      return string.IsNullOrEmpty(reason);
    }

    // leadership/follower handling
    public void AddFollower(LivingActor other)
    {
      if (m_Followers is null) m_Followers = new List<LivingActor>(1);
      else if (m_Followers.Contains(other)) throw new ArgumentException("other is already a follower");
      m_Followers.Add(other);
      other.Leader?.RemoveFollower(other);
      other.m_Leader = this;
    }

    // 2020-01-26 public void AddLeader(Actor leader) didn't work out in release mode IL: operator ?. doesn't actually reduce bytecode for the caller

    // 2022-05-13 having problems with multi-threading or out-of-order instructions within Actor::RemoveAllFollowers.
    // handled by refactoring out Actor::_NoLongerFollower
    private void _NoLongerFollower()
    {
      Interlocked.Exchange(ref m_Leader, null);
      (Controller as Gameplay.AI.OrderableAI)?.LeaderWasSetNull_handler();
    }

    public void RemoveFollower(LivingActor other)
    {
      var followers = m_Followers;
      if (followers is null) throw new InvalidOperationException(Name+": no followers");
      other._NoLongerFollower();
      followers.Remove(other);
//    other._NoLongerFollower(); // multi-threaded race condition when here?
      if (0 == followers.Count) Interlocked.Exchange(ref m_Followers, null);
    }

    public void RemoveAllFollowers()
    {
      var followers = m_Followers;
      if (followers is not null) {
        foreach(var fo in followers) fo._NoLongerFollower();
        Interlocked.Exchange(ref m_Followers, null);
      }
    }

    public void SetTrustIn(LivingActor other, int trust) => (m_TrustDict ??= new())[other] = trust;

    public int GetTrustIn(LivingActor other)
    {
      if (m_TrustDict is null) return 0;
      if (m_TrustDict.TryGetValue(other,out int trust)) return trust;
      return 0;
    }

    public int TrustIncrease {
      get {
        return 1 + SKILL_CHARISMATIC_TRUST_BONUS * MySkills.GetSkillLevel(Skills.IDs.CHARISMATIC);
      }
    }

    // We do not handle the enemy relations here.
    public HashSet<LivingActor>? Allies {
      get {
        var ret = new HashSet<LivingActor>();
        // 1) police have all other police as allies.
        if (IsFaction(GameFactions.IDs.ThePolice)) ret = (World.Get.PoliceInRadioRange(Location) ?? ret);
        // 2) leader/follower cliques are allies.
        m_Followers?.AppendTo(ret);
        var leader = LiveLeader;
        if (leader is not null) { // 2019-08-14: currently mutually exclusive with above for NPCs
          ret.Add(leader);
          leader.m_Followers?.AppendTo(ret);
          ret.Remove(this);
        }
        return (0<ret.Count ? ret : null);
      }
    }

    // ignores faction alliances
    public HashSet<LivingActor>? ChainOfCommand {
      get {
        var ret = new HashSet<LivingActor>();
        m_Followers?.AppendTo(ret);
        var leader = LiveLeader;
        if (leader is not null) {
          ret.Add(leader);
          leader.m_Followers?.AppendTo(ret);
          ret.Remove(this);
        }
        return (0<ret.Count ? ret : null);
      }
    }

    // alpha10
    /// <summary>Is this other actor our leader, a follower or a mate.  Actor.IsInGroupWith is the entry point.</summary>
    internal bool InGroupWith(Actor other)
    {
      var leader = LiveLeader;
      if (leader is not null) {
        if (   other == leader   // my leader?
            || (other as LivingActor)?.Leader == leader)    // a mate?
          return true;
      }
      return other is LivingActor living && (m_Followers?.Contains(living) ?? false); // a follower?
    }

    // This prepares an actor for being a PC.  Note that hacking the player controller in
    // by hex-editing does work flawlessly at the Actor level.
    public override void PrepareForPlayerControl()
    {
      var leader = Leader;
      if (leader is not null && !leader.IsPlayer) leader.RemoveFollower(this);   // needed if leader is NPC
    }

    protected override void OptimizeBeforeSaving()
    {
      base.OptimizeBeforeSaving();
      if (m_Leader?.IsDead ?? false) m_Leader = null;
      // \todo match RS alpha 10's prune of trust entries in dead actors (m_TrustDict for us)
      // to avoid weirdness we want to drop only if no revivable corpse is in-game
      m_Followers = m_Followers?.Optimize();
    }
#endregion

#region trackers and radios
    public void Tracks(ref Span<bool> find_us)
    {
        find_us[(int)ItemTrackerModel.TrackingOffset.POLICE_FACTION] = IsFaction(GameFactions.IDs.ThePolice);

        // the cell phone
        GetEquippedItem(DollPart.LEFT_HAND)?.NotUseless<ItemTracker>()?.Model.Tracks(ref find_us);

        // the police radio
        GetEquippedItem(DollPart.HIP_HOLSTER)?.NotUseless<ItemTracker>()?.Model.Tracks(ref find_us);

        // Tracking leaders makes sense, but not in single PC mode
        if (0 >= CountFollowers && LiveLeader is not null) find_us[(int)ItemTrackerModel.TrackingOffset.FOLLOWER_AND_LEADER] = false;
    }

    public bool NeedActivePoliceRadio {
      get {
        if (IsFaction(GameFactions.IDs.ThePolice)) return false; // implicit
        // XXX disallow murderers under certain conditions, etc
        var leader = LiveLeader;
        if (leader is not null) return leader.HasActivePoliceRadio; // XXX \todo change target: deep chain of command
        if (m_Followers is not null) foreach(var fo in m_Followers) if (fo.HasPoliceRadio) return true;
        return false;
      }
    }

    public bool NeedActiveCellPhone {
      get {
        if (!WantCellPhone) return false;
        var leader = LiveLeader;
        if (leader is not null) return leader.HasActiveCellPhone; // XXX \todo change target: deep chain of command
        if (m_Followers is not null) foreach(var fo in m_Followers) if (fo.HasCellPhone) return true;
        return false;
      }
    }

    public bool WantPoliceRadio {
      get {
        if (IsFaction(GameFactions.IDs.ThePolice)) return false; // police have implicit police radios
        bool have_cellphone = HasCellPhone;
        bool have_army = HasArmyRadio;
        if (!have_cellphone && !have_army) return true;

        bool out_of_comm(LivingActor a) {
          if (have_cellphone && a.HasCellPhone) return false;
          if (have_army && a.HasArmyRadio) return false;
          return true;
        }

        var leader = LiveLeader;
        if (leader is not null) return out_of_comm(leader); // XXX \todo change target: deep chain of command
        if (m_Followers is not null) foreach(var fo in m_Followers) if (out_of_comm(fo)) return true;
        return false;
      }
    }

    public bool WantCellPhone {
      get {
        bool have_police = HasPoliceRadio;
        bool have_army = HasArmyRadio;
        if (!have_police && !have_army) return true;

        bool out_of_comm(LivingActor a) {
          if (have_police && a.HasPoliceRadio) return false;
          if (have_army && a.HasArmyRadio) return false;
          return true;
        }

        var leader = LiveLeader;
        if (leader is not null) return out_of_comm(leader); // XXX \todo change target: deep chain of command
        if (m_Followers is not null) foreach(var fo in m_Followers) if (out_of_comm(fo)) return true;
        return false;
      }
    }
#endregion


#region can we (inventory-dependent)
    public int ReviveChance(Corpse corpse)
    {
      if (!ActionReviveCorpse.Can(this, corpse)) return 0;
      return corpse.FreshnessPercent / 4 + MySkills.GetSkillLevel(Skills.IDs.MEDIC) * SKILL_MEDIC_REVIVE_BONUS;
    }

    public bool CouldBarricade() => string.IsNullOrEmpty(ReasonCouldntBarricade());

    private string ReasonCantBarricade(DoorWindow door)
    {
      if (!door.CanBarricade(out string reason)) return reason;
      return ReasonCouldntBarricade();
    }

    public bool CanBarricade(DoorWindow door, out string reason)
    {
      reason = ReasonCantBarricade(door);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBarricade(DoorWindow door)
    {
      return string.IsNullOrEmpty(ReasonCantBarricade(door));
    }

    


    private string ReasonCantBuildFortification(Point pos, bool isLarge)
    {
      if (0 >= MySkills.GetSkillLevel(Skills.IDs.CARPENTRY)) return "no skill in carpentry";

      Map map = Location.Map;
      if (!map.GetTileModelAtExt(pos)?.IsWalkable ?? true) return  "cannot build on walls";

      int num = BarricadingMaterialNeedForFortification(isLarge);
      if (CountItems<ItemBarricadeMaterial>() < num) return string.Format("not enough barricading material, need {0}.", (object) num);
      if (map.HasMapObjectAt(pos) || map.HasActorAt(in pos)) return "blocked";
      return "";
    }

    public bool CanBuildFortification(Point pos, bool isLarge, out string reason)
    {
      reason = ReasonCantBuildFortification(pos,isLarge);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBuildFortification(in Point pos, bool isLarge)
    {
      return string.IsNullOrEmpty(ReasonCantBuildFortification(pos,isLarge));
    }

    // keep the unused parameter -- would need it if alternate materials possible
    private string ReasonCantRepairFortification(Fortification fort)
    {
      if (str_CannotUseMapObjects is string msg) return msg;
      if (0 >= CountItems<ItemBarricadeMaterial>()) return "no barricading material";
      return "";
    }

    public bool CanRepairFortification(Fortification fort, out string reason)
    {
      reason = ReasonCantRepairFortification(fort);
      return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanRepairFortification(Fortification fort)
    {
      return string.IsNullOrEmpty(ReasonCantRepairFortification(fort));
    }
#endif


    // maybe this should be over on the Inventory object
    private string ReasonCantUseItem(Item it)
    {
      if (!Model.Abilities.CanUseItems) return "no ability to use items";
      if (it is ItemWeapon) return "to use a weapon, equip it";
      if (it is ItemBodyArmor) return "to use armor, wear it";
      if (it is ItemBarricadeMaterial) return "to use material, build a barricade";
      if (!(it is UsableItem obj)) return "not a usable item type"; // cannot throw due to insane actions
      var err = obj.ReasonCantUse(this);
      if (!string.IsNullOrEmpty(err)) return err;
      if (!IsCarrying(it)) return "not in inventory";
      return "";
    }

    public bool CanUse(Item it, out string reason)
    {
      reason = ReasonCantUseItem(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanUse(Item it)
    {
      return string.IsNullOrEmpty(ReasonCantUseItem(it));
    }


    public bool CanGet(Item it, out string reason)
    {
      reason = ReasonCantGet(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanGet(Item it)
    {
      return string.IsNullOrEmpty(ReasonCantGet(it));
    }


    private string ReasonCantGiveTo(LivingActor target, Item gift)
    {
      if (IsEnemyOf(target)) return "enemy";
      if (gift.IsEquipped) return "equipped";
      if (target.IsSleeping) return "sleeping";
      return target.ReasonCantGet(gift);
    }

    public bool CanGiveTo(LivingActor target, Item gift, out string reason)
    {
      reason = ReasonCantGiveTo(target,gift);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanGiveTo(LivingActor target, Item gift) => string.IsNullOrEmpty(ReasonCantGiveTo(target, gift));


    private string ReasonCantUnequip(Item it)
    {
      if (!it.IsEquipped) return "not equipped";
      if (!IsCarrying(it)) return "not in inventory";
      return "";
    }

    public bool CanUnequip(Item it, out string reason)
    {
      reason = ReasonCantUnequip(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanUnequip(Item it)
    {
      return string.IsNullOrEmpty(ReasonCantUnequip(it));
    }


    private string ReasonCantDrop(Item it)
    {
      if (it.IsEquipped && Controller is PlayerController) return "unequip first";  // AI doesn't need that UI safety
      if (!IsCarrying(it)) return "not in inventory";
      return "";
    }

    public bool CanDrop(Item it, out string reason)
    {
      reason = ReasonCantDrop(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanDrop(Item it) => string.IsNullOrEmpty(ReasonCantDrop(it));

#endregion

#region trading
    public List<Item>? GetInterestingTradeableItems(Actor buyer) // called from RogueGame::PickItemToTrade so forced to be public no matter where
    {
#if DEBUG
      if (!Model.Abilities.CanTrade) throw new InvalidOperationException("cannot trade");
      if (!buyer.Model.Abilities.CanTrade) throw new InvalidOperationException("cannot trade");
#endif

      if (buyer.IsPlayer && IsPlayer) return m_Inventory.ToList();

      var buyer_ai = buyer.RunBy<Gameplay.AI.ObjectiveAI>();
      var ai = this.RunBy<Gameplay.AI.ObjectiveAI>();

      // IsInterestingTradeItem includes a charisma check i.e. RNG invocation, so cannot use .Any() prescreen safely
      var objList = m_Inventory.Where(it=> buyer_ai.IsInterestingTradeItem(this, it) && ai.IsTradeableItem(it)).ToList();  // \todo upgrade ObjectiveAI::IsTradeableItem to virtual with a PlayerController override
      return 0<objList.Count ? objList : null;
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
