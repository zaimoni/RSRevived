// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Actor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine.Items;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;
using LOS = djack.RogueSurvivor.Engine.LOS;
using Rules = djack.RogueSurvivor.Engine.Rules;
using Skills = djack.RogueSurvivor.Gameplay.Skills;
using PowerGenerator = djack.RogueSurvivor.Engine.MapObjects.PowerGenerator;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Actor
  {
    public const int FOOD_HUNGRY_LEVEL = WorldTime.TURNS_PER_DAY;
    public const int ROT_HUNGRY_LEVEL = 2*WorldTime.TURNS_PER_DAY;
    public const int SLEEP_SLEEPY_LEVEL = 30*WorldTime.TURNS_PER_HOUR;
    private const int STAMINA_INFINITE = 99;
    public const int STAMINA_MIN_FOR_ACTIVITY = 10;
    private const int NIGHT_STA_PENALTY = 2;
    public const int STAMINA_REGEN_WAIT = 2;
    private const int LIVING_SCENT_DROP = OdorScent.MAX_STRENGTH;
    private const int UNDEAD_MASTER_SCENT_DROP = OdorScent.MAX_STRENGTH;
    private const int MINIMAL_FOV = 2;
    private const int FOV_PENALTY_SUNSET = 1;
    private const int FOV_PENALTY_EVENING = 2;
    private const int FOV_PENALTY_MIDNIGHT = 3;
    private const int FOV_PENALTY_DEEP_NIGHT = 4;
    private const int FOV_PENALTY_SUNRISE = 2;
    private const int FOV_PENALTY_RAIN = 1;
    private const int FOV_PENALTY_HEAVY_RAIN = 2;
    private const int FIRE_DISTANCE_VS_RANGE_MODIFIER = 2;
    private const float FIRING_WHEN_STA_TIRED = 0.75f;
    private const float FIRING_WHEN_STA_NOT_FULL = 0.9f;

    public static float SKILL_AWAKE_SLEEP_BONUS = 0.15f;    // XXX 0.17f makes this useful at L1
    public static int SKILL_HAULER_INV_BONUS = 1;
    public static int SKILL_HIGH_STAMINA_STA_BONUS = 5;
    public static int SKILL_LEADERSHIP_FOLLOWER_BONUS = 1;
    public static float SKILL_LIGHT_EATER_MAXFOOD_BONUS = 0.15f;
    public static int SKILL_NECROLOGY_UNDEAD_BONUS = 2;
    public static int SKILL_TOUGH_HP_BONUS = 3;
    public static float SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = 0.15f;
    public static int SKILL_ZTOUGH_HP_BONUS = 4;

    public static int SKILL_AGILE_ATK_BONUS = 2;
    public static int SKILL_BOWS_ATK_BONUS = 5;
    public static int SKILL_BOWS_DMG_BONUS = 2;
    public static int SKILL_FIREARMS_ATK_BONUS = 5;
    public static int SKILL_FIREARMS_DMG_BONUS = 2;
    public static int SKILL_MARTIAL_ARTS_ATK_BONUS = 3;
    public static int SKILL_MARTIAL_ARTS_DMG_BONUS = 1;
    public static int SKILL_STRONG_DMG_BONUS = 2;
    public static int SKILL_ZAGILE_ATK_BONUS = 1;
    public static int SKILL_ZSTRONG_DMG_BONUS = 2;

    private Actor.Flags m_Flags;
    private Gameplay.GameActors.IDs m_ModelID;
    private int m_FactionID;
    private Gameplay.GameGangs.IDs m_GangID;
    private string m_Name;
    private ActorController m_Controller;
    private ActorSheet m_Sheet;
    private readonly int m_SpawnTime;
    private Inventory m_Inventory;
    private Doll m_Doll;
    private int m_HitPoints;
    private int m_previousHitPoints;
    private int m_StaminaPoints;
    private int m_previousStamina;
    private int m_FoodPoints;
    private int m_previousFoodPoints;
    private int m_SleepPoints;
    private int m_previousSleepPoints;
    private int m_Sanity;
    private int m_previousSanity;
    private Location m_Location;
    private int m_ActionPoints;
    private int m_LastActionTurn;
    private Activity m_Activity;
    private Actor m_TargetActor;
    private int m_AudioRangeMod;
    private Attack m_CurrentMeleeAttack;
    private Attack m_CurrentRangedAttack;
    private Defence m_CurrentDefence;
    private Actor m_Leader;
    private List<Actor> m_Followers;
    private int m_TrustInLeader;
    private Dictionary<Actor,int> m_TrustDict;
    private int m_KillsCount;
    private List<Actor> m_AggressorOf;
    private List<Actor> m_SelfDefenceFrom;
    private int m_MurdersCounter;
    private int m_Infection;
    private Corpse m_DraggedCorpse;
    private List<Item> m_BoringItems;

    public ActorModel Model
    {
      get {
        Contract.Ensures(null!=Contract.Result<ActorModel>());
        return Models.Actors[(int)m_ModelID];
      }
      set { // this must be public due to undead evolution
	    Contract.Requires(null!=value);
        m_ModelID = value.ID;
        OnModelSet();
      }
    }

    public bool IsUnique
    {
      get {
        return GetFlag(Actor.Flags.IS_UNIQUE);
      }
      set {
        SetFlag(Actor.Flags.IS_UNIQUE, value);
      }
    }

    public Faction Faction
    {
      get {
        return Models.Factions[m_FactionID];
      }
      set {
        m_FactionID = value.ID;
      }
    }

    public string Name
    {
      get {
        if (!IsPlayer) return m_Name;
        return "(YOU) " + m_Name;
      }
      set {
        m_Name = value;
        if (value == null) return;
        m_Name.Replace("(YOU) ", "");
      }
    }

    public string UnmodifiedName
    {
      get {
        return m_Name;
      }
    }

    public bool IsProperName
    {
      get {
        return GetFlag(Actor.Flags.IS_PROPER_NAME);
      }
      set {
        SetFlag(Actor.Flags.IS_PROPER_NAME, value);
      }
    }

    public bool IsPluralName
    {
      get {
        return GetFlag(Actor.Flags.IS_PLURAL_NAME);
      }
      set {
        SetFlag(Actor.Flags.IS_PLURAL_NAME, value);
      }
    }

    public string TheName
    {
      get {
        if (!IsProperName && !IsPluralName)
          return "the " + m_Name;
        return Name;
      }
    }

    public ActorController Controller
    {
      get {
        return m_Controller;
      }
      set {
        if (m_Controller != null) m_Controller.LeaveControl();
        m_Controller = value;
        if (m_Controller != null) m_Controller.TakeControl(this);
        if (m_Location.Map != null) m_Location.Map.RecalcPlayers();
      }
    }

    public bool IsPlayer
    {
      get {
        if (m_Controller != null)
          return m_Controller is PlayerController;
        return false;
      }
    }

    public int SpawnTime {
      get {
        return m_SpawnTime;
      }
    }

    public Gameplay.GameGangs.IDs GangID {
      get {
        return m_GangID;
      }
      set {
        m_GangID = value;
      }
    }

    public bool IsInAGang {
      get {
        return m_GangID != 0;
      }
    }

    public Doll Doll {
      get {
        return m_Doll;
      }
    }

    public bool IsDead {
      get {
        return GetFlag(Actor.Flags.IS_DEAD);
      }
      set {
        SetFlag(Actor.Flags.IS_DEAD, value);
      }
    }

    public bool IsSleeping {
      get {
        return GetFlag(Actor.Flags.IS_SLEEPING);
      }
      set {
        SetFlag(Actor.Flags.IS_SLEEPING, value);
      }
    }

    public bool IsRunning {
      get {
        return GetFlag(Actor.Flags.IS_RUNNING);
      }
      set {
        SetFlag(Actor.Flags.IS_RUNNING, value);
      }
    }

    public Inventory Inventory {
      get {
        return m_Inventory;
      }
    }

    public int HitPoints {
      get {
        return m_HitPoints;
      }
      set {
        m_HitPoints = value;
      }
    }

    public int PreviousHitPoints {
      get {
        return m_previousHitPoints;
      }
    }

    public int StaminaPoints {
      get {
        return m_StaminaPoints;
      }
      set {
        m_StaminaPoints = value;
      }
    }

    public int PreviousStaminaPoints {
      get {
        return m_previousStamina;
      }
      set {
        m_previousStamina = value;
      }
    }

    public int FoodPoints {
      get {
        return m_FoodPoints;
      }
    }

    public int PreviousFoodPoints {
      get {
        return m_previousFoodPoints;
      }
    }

    public int SleepPoints {
      get {
        return m_SleepPoints;
      }
    }

    public int PreviousSleepPoints {
      get {
        return m_previousSleepPoints;
      }
    }

    public int Sanity {
      get {
        return m_Sanity;
      }
    }

    public int PreviousSanity {
      get {
        return m_previousSanity;
      }
    }

    public ActorSheet Sheet {
      get {
        Contract.Ensures(null!=Contract.Result<ActorSheet>());
        return m_Sheet;
      }
    }

    public int ActionPoints {
      get {
        return m_ActionPoints;
      }
      set {
        m_ActionPoints = value;
      }
    }

    public int LastActionTurn {
      get {
        return m_LastActionTurn;
      }
    }

    public Location Location {
      get {
        return m_Location;
      }
      set {
        m_Location = value;
      }
    }

    public Activity Activity {
      get {
        return m_Activity;
      }
      set {
        m_Activity = value;
      }
    }

    public Actor TargetActor {
      get {
        return m_TargetActor;
      }
      set {
        m_TargetActor = value;
      }
    }

    public int AudioRange {
      get {
        return m_Sheet.BaseAudioRange + m_AudioRangeMod;
      }
    }

    public int AudioRangeMod {
      get {
        return m_AudioRangeMod;
      }
    }

    public Attack CurrentMeleeAttack {
      get {
        return m_CurrentMeleeAttack;
      }
    }

    public Attack CurrentRangedAttack {
      get {
        return m_CurrentRangedAttack;
      }
    }

    public Defence CurrentDefence {
      get {
        return m_CurrentDefence;
      }
    }

    // Leadership
    public Actor Leader {
      get {
        return m_Leader;
      }
    }

    public bool HasLeader {
      get {
        if (m_Leader != null) return !m_Leader.IsDead;
        return false;
      }
    }

    public int TrustInLeader {
      get {
        return m_TrustInLeader;
      }
      set {
        m_TrustInLeader = value;
      }
    }

    public IEnumerable<Actor> Followers
    {
      get {
        return (IEnumerable<Actor>)m_Followers;
      }
    }

    public int CountFollowers {
      get {
        if (m_Followers == null) return 0;
        return m_Followers.Count;
      }
    }

    public int MaxFollowers {
      get {
        return SKILL_LEADERSHIP_FOLLOWER_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP);
      }
    }

    private string ReasonCantTakeLeadOf(Actor target)
    {
      Contract.Requires(null != target);
      if (target.Model.Abilities.IsUndead) return "undead";
      if (IsEnemyOf(target)) return "enemy";
      if (target.IsSleeping) return "sleeping";
      if (target.HasLeader) return "already has a leader";
      if (target.CountFollowers > 0) return "is a leader";  // XXX organized force would have a chain of command
      int num = MaxFollowers;
      if (num == 0) return "can't lead";
      if (CountFollowers >= num) return "too many followers";
      // to support savefile hacking.  AI in charge of player is a problem.
      if (target.IsPlayer && !IsPlayer) return "is player";
      if (Faction != target.Faction && target.Faction.LeadOnlyBySameFaction) return string.Format("{0} can't lead {1}", Faction.Name, target.Faction.Name);
      return "";
    }

    public bool CanTakeLeadOf(Actor target, out string reason)
    {
      reason = ReasonCantTakeLeadOf(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanTakeLeadOf(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantTakeLeadOf(target));
    }

	private string ReasonCantChatWith(Actor target)
	{
	  Contract.Requires(null != target);
      if (!Model.Abilities.CanTalk) return "can't talk";
      if (!target.Model.Abilities.CanTalk) return string.Format("{0} can't talk", target.TheName);
      if (IsSleeping) return "sleeping";
      if (target.IsSleeping) return string.Format("{0} is sleeping", target.TheName);
      return "";
	}

    public bool CanChatWith(Actor target, out string reason)
    {
	  reason = ReasonCantChatWith(target);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanChatWith(Actor target)
    {
	  return string.IsNullOrEmpty(ReasonCantChatWith(target));
    }

	private string ReasonCantSwitchPlaceWith(Actor target)
	{
	  Contract.Requires(null != target);
      if (target.Leader != this) return "not your follower";
      if (target.IsSleeping) return "sleeping";
      return "";
	}

    public bool CanSwitchPlaceWith(Actor target, out string reason)
    {
	  reason = ReasonCantSwitchPlaceWith(target);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanSwitchPlaceWith(Actor target)
    {
	  return string.IsNullOrEmpty(ReasonCantSwitchPlaceWith(target));
    }

    // aggression statistics, etc.
    public int KillsCount {
      get {
        return m_KillsCount;
      }
      set {
        m_KillsCount = value;
      }
    }

    public IEnumerable<Actor> AggressorOf {
      get {
        return (IEnumerable<Actor>)m_AggressorOf;
      }
    }

    public int CountAggressorOf {
      get {
        if (m_AggressorOf == null) return 0;
        return m_AggressorOf.Count;
      }
    }

    public IEnumerable<Actor> SelfDefenceFrom {
      get {
        return (IEnumerable<Actor>)m_SelfDefenceFrom;
      }
    }

    public int CountSelfDefenceFrom {
      get {
        if (m_SelfDefenceFrom == null) return 0;
        return m_SelfDefenceFrom.Count;
      }
    }

    public int MurdersCounter {
      get {
        return m_MurdersCounter;
      }
      set {
        m_MurdersCounter = value;
      }
    }

    public int Infection
    {
      get {
        return m_Infection;
      }
    }

    public Corpse DraggedCorpse
    {
      get {
        return m_DraggedCorpse;
      }
      set {
        m_DraggedCorpse = value;
      }
    }

    public Actor(ActorModel model, Faction faction, int spawnTime, string name="", bool isProperName=false, bool isPluralName=false)
    {
      Contract.Requires(null != model);
      Contract.Requires(null != faction);
      if (string.IsNullOrEmpty(name)) name = model.Name;
      m_ModelID = model.ID;
      m_FactionID = faction.ID;
      m_GangID = 0;
      m_Name = name;
      IsProperName = isProperName;
      IsPluralName = isPluralName;
      m_Location = new Location();
      m_SpawnTime = spawnTime;
      IsUnique = false;
      IsDead = false;
      OnModelSet();
    }

    private void OnModelSet()
    {
      ActorModel model = Model;
      m_Doll = new Doll(model.DollBody);
      m_Sheet = new ActorSheet(model.StartingSheet);
      m_ActionPoints = m_Doll.Body.Speed;
      m_HitPoints = m_previousHitPoints = m_Sheet.BaseHitPoints;
      m_StaminaPoints = m_previousStamina = m_Sheet.BaseStaminaPoints;
      m_FoodPoints = m_previousFoodPoints = m_Sheet.BaseFoodPoints;
      m_SleepPoints = m_previousSleepPoints = m_Sheet.BaseSleepPoints;
      m_Sanity = m_previousSanity = m_Sheet.BaseSanity;
      if (model.Abilities.HasInventory)
        m_Inventory = new Inventory(model.StartingSheet.BaseInventoryCapacity);
      else
        m_Inventory = null; // any previous inventory will be irrevocably destroyed
      m_CurrentMeleeAttack = model.StartingSheet.UnarmedAttack;
      m_CurrentDefence = model.StartingSheet.BaseDefence;
      m_CurrentRangedAttack = Attack.BLANK;
    }

	public void PrefixName(string prefix)
	{
	  m_Name = prefix+" "+m_Name;
	}

    public int DamageBonusVsUndeads {
      get {
        return Actor.SKILL_NECROLOGY_UNDEAD_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.NECROLOGY);
      }
    }

	// strictly speaking, 1 step is allowed but we do not check LoF here
    private string ReasonCouldntFireAt(Actor target)
    {
      Contract.Requires(null != target);
      ItemRangedWeapon itemRangedWeapon = GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon == null) return "no ranged weapon equipped";
      if (CurrentRangedAttack.Range+1 < Rules.GridDistance(Location.Position, target.Location.Position)) return "out of range";
      if (itemRangedWeapon.Ammo <= 0) return "no ammo left";
      if (target.IsDead) return "already dead!";
      return "";
    }

    public bool CouldFireAt(Actor target, out string reason)
    {
      reason = ReasonCouldntFireAt(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool CouldFireAt(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCouldntFireAt(target));
    }

    private string ReasonCantFireAt(Actor target, List<Point> LoF)
    {
      Contract.Requires(null != target);
      if (LoF != null) LoF.Clear();
      ItemRangedWeapon itemRangedWeapon = GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon == null) return "no ranged weapon equipped";
      if (CurrentRangedAttack.Range < Rules.GridDistance(Location.Position, target.Location.Position)) return "out of range";
      if (itemRangedWeapon.Ammo <= 0) return "no ammo left";
      if (!LOS.CanTraceFireLine(Location, target.Location.Position, CurrentRangedAttack.Range, LoF)) return "no line of fire";
      if (target.IsDead) return "already dead!";
      return "";
    }

    public bool CanFireAt(Actor target, List<Point> LoF, out string reason)
    {
      reason = ReasonCantFireAt(target,LoF);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanFireAt(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantFireAt(target, null));
    }

    private string ReasonCantContrafactualFireAt(Actor target, Point p)
    {
      Contract.Requires(null != target);
      if (CurrentRangedAttack.Range < Rules.GridDistance(p, target.Location.Position)) return "out of range";
      if (!LOS.CanTraceHypotheticalFireLine(new Location(Location.Map,p), target.Location.Position, CurrentRangedAttack.Range, this)) return "no line of fire";
      return "";
    }

	public bool CanContrafactualFireAt(Actor target, Point p, out string reason)
	{
	  reason = ReasonCantContrafactualFireAt(target,p);
	  return string.IsNullOrEmpty(reason);
	}

	public bool CanContrafactualFireAt(Actor target, Point p)
	{
	  return string.IsNullOrEmpty(ReasonCantContrafactualFireAt(target, p));
	}

    public string ReasonCantMeleeAttack(Actor target)
    {
      Contract.Requires(null != target);
      if (Location.Map == target.Location.Map) {
        if (!Rules.IsAdjacent(Location.Position, target.Location.Position)) return "not adjacent";
      } else {
        Exit exitAt = Location.Map.GetExitAt(Location.Position);
        if (exitAt == null) return "not reachable";
        if (target.Location.Map.GetExitAt(target.Location.Position) == null) return "not reachable";
        if (exitAt.Location != target.Location) return "not reachable";
      }
      if (StaminaPoints < STAMINA_MIN_FOR_ACTIVITY) return "not enough stamina to attack";
      if (target.IsDead) return "already dead!";
      return "";
    }

    public bool CanMeleeAttack(Actor target, out string reason)
    {
      reason = ReasonCantMeleeAttack(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanMeleeAttack(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantMeleeAttack(target));
    }

    public Attack HypotheticalMeleeAttack(Attack baseAttack, Actor target = null)
    {
      int num3 = Actor.SKILL_AGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.AGILE) + Actor.SKILL_ZAGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_AGILE);
      int num4 = Actor.SKILL_STRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG) + Actor.SKILL_ZSTRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_STRONG);
      if (GetEquippedWeapon() == null)
      {
        num3 += Actor.SKILL_MARTIAL_ARTS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
        num4 += Actor.SKILL_MARTIAL_ARTS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
      }
      if (target != null && target.Model.Abilities.IsUndead)
        num4 += DamageBonusVsUndeads;
      float num5 = (float)baseAttack.HitValue + (float) num3;
      if (IsExhausted) num5 /= 2f;
      else if (IsSleepy) num5 *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) num5, baseAttack.DamageValue + num4, baseAttack.StaminaPenalty);
    }

    // does not properly account for martial arts
    public ItemMeleeWeapon GetBestMeleeWeapon(Predicate<Item> fn=null)
    {
      if (Inventory == null) return null;
      List<ItemMeleeWeapon> tmp = Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (null == tmp) return null;
      int num1 = 0;
      ItemMeleeWeapon itemMeleeWeapon1 = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        if (fn == null || fn(obj)) {
          int num2 = (obj.Model as ItemMeleeWeaponModel).Attack.Rating;
          if (num2 > num1) {
            num1 = num2;
            itemMeleeWeapon1 = obj;
          }
        }
      }
      return itemMeleeWeapon1;
    }

    // ultimately these two will be thin wrappers, as CurrentMeleeAttack/CurrentRangedAttack are themselves mathematical functions
    // of the equipped weapon which OrderableAI *will* want to vary when choosing an appropriate weapon
    public Attack MeleeAttack(Actor target = null) { return HypotheticalMeleeAttack(CurrentMeleeAttack, target); }

    public Attack BestMeleeAttack(Actor target = null)
    {
      ItemMeleeWeapon tmp_melee = GetBestMeleeWeapon();
      Attack base_melee_attack = (null!=tmp_melee ? (tmp_melee.Model as ItemMeleeWeaponModel).BaseMeleeAttack(Sheet) : CurrentMeleeAttack);
      return HypotheticalMeleeAttack(base_melee_attack, target);
    }

    public Attack RangedAttack(int distance, Actor target = null)
    {
      Attack baseAttack = CurrentRangedAttack;
      int num1 = 0;
      int num2 = 0;
      switch (baseAttack.Kind)
      {
        case AttackKind.FIREARM:
          num1 = Actor.SKILL_FIREARMS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.FIREARMS);
          num2 = Actor.SKILL_FIREARMS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.FIREARMS);
          break;
        case AttackKind.BOW:
          num1 = Actor.SKILL_BOWS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.BOWS);
          num2 = Actor.SKILL_BOWS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.BOWS);
          break;
      }
      if (target != null && target.Model.Abilities.IsUndead)
        num2 += DamageBonusVsUndeads;
      int efficientRange = baseAttack.EfficientRange;
      if (distance != efficientRange) {
        num1 += (efficientRange - distance) * FIRE_DISTANCE_VS_RANGE_MODIFIER;
      }
      float num4 = (float) (baseAttack.HitValue + num1);
      if (IsExhausted) num4 /= 2f;
      else if (IsSleepy) num4 *= 0.75f;
      if (IsTired)
        num4 *= FIRING_WHEN_STA_TIRED;
      else if (StaminaPoints < MaxSTA)
        num4 *= FIRING_WHEN_STA_NOT_FULL;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) num4, baseAttack.DamageValue + num2, baseAttack.StaminaPenalty, baseAttack.Range);
    }

    // leadership/follower handling
    public void AddFollower(Actor other)
    {
      if (other == null) throw new ArgumentNullException("other");
      if (m_Followers != null && m_Followers.Contains(other)) throw new ArgumentException("other is already a follower");
      if (m_Followers == null) m_Followers = new List<Actor>(1);
      m_Followers.Add(other);
      if (other.Leader != null) other.Leader.RemoveFollower(other);
      other.m_Leader = this;
    }

    public void RemoveFollower(Actor other)
    {
      if (other == null) throw new ArgumentNullException("other");
      if (m_Followers == null) throw new InvalidOperationException("no followers");
      m_Followers.Remove(other);
      if (m_Followers.Count == 0) m_Followers = null;
      other.m_Leader = null;
      Gameplay.AI.OrderableAI aiController = other.Controller as Gameplay.AI.OrderableAI;
      if (aiController == null) return;
      aiController.Directives.Reset();
      aiController.SetOrder(null);
    }

    public void RemoveAllFollowers()
    {
      while (m_Followers != null && m_Followers.Count > 0)
        RemoveFollower(m_Followers[0]);
    }

    public void SetTrustIn(Actor other, int trust)
    {
	  Contract.Requires(null != other);
      if (null == m_TrustDict) m_TrustDict = new Dictionary<Actor,int>();
      m_TrustDict[other] = trust;
    }

    public void AddTrustIn(Actor other, int amount)
    {
      SetTrustIn(other, GetTrustIn(other) + amount);
    }

    public int GetTrustIn(Actor other)
    {
      if (null == m_TrustDict) return 0;
      int trust = 0;
      if (m_TrustDict.TryGetValue(other,out trust)) return trust;
      return 0;
    }

    public ThreatTracking Threats { 
      get {
        if ((int)Gameplay.GameFactions.IDs.ThePolice == Faction.ID) return Engine.Session.Get.PoliceThreatTracking;
        return null;
      }
    }

    public LocationSet InterestingLocs { 
      get {
        if ((int)Gameplay.GameFactions.IDs.ThePolice == Faction.ID) return Engine.Session.Get.PoliceInvestigate;
        return null;
      }
    }

    public void MarkAsAgressorOf(Actor other)
    {
      if (other == null || other.IsDead) return;
      if (m_AggressorOf == null) m_AggressorOf = new List<Actor>(1);
      else if (m_AggressorOf.Contains(other)) return;
      m_AggressorOf.Add(other);
      Threats?.RecordTaint(other, other.Location);
    }

    public void MarkAsSelfDefenceFrom(Actor other)
    {
      if (other == null || other.IsDead) return;
      if (m_SelfDefenceFrom == null) m_SelfDefenceFrom = new List<Actor>(1);
      else if (m_SelfDefenceFrom.Contains(other)) return;
      m_SelfDefenceFrom.Add(other);
      Threats?.RecordTaint(other, other.Location);
    }

    public bool IsAggressorOf(Actor other)
    {
      if (m_AggressorOf == null) return false;
      return m_AggressorOf.Contains(other);
    }

    public bool IsSelfDefenceFrom(Actor other)
    {
      if (m_SelfDefenceFrom == null) return false;
      return m_SelfDefenceFrom.Contains(other);
    }

    public void RemoveAggressorOf(Actor other)
    {
      if (m_AggressorOf == null) return;
      m_AggressorOf.Remove(other);
      if (m_AggressorOf.Count != 0) return;
      m_AggressorOf = null;
    }

    public void RemoveSelfDefenceFrom(Actor other)
    {
      if (m_SelfDefenceFrom == null) return;
      m_SelfDefenceFrom.Remove(other);
      if (m_SelfDefenceFrom.Count != 0) return;
      m_SelfDefenceFrom = null;
    }

    public void RemoveAllAgressorSelfDefenceRelations()
    {
      while (m_AggressorOf != null) {
        Actor other = m_AggressorOf[0];
        RemoveAggressorOf(other);
        other.RemoveSelfDefenceFrom(this);
      }
      while (m_SelfDefenceFrom != null) {
        Actor other = m_SelfDefenceFrom[0];
        RemoveSelfDefenceFrom(other);
        other.RemoveAggressorOf(this);
      }
    }

    public bool IsEnemyOf(Actor target)
    {
      return target != null && (Faction.IsEnemyOf(target.Faction) || (Faction == target.Faction && IsInAGang && target.IsInAGang && GangID != target.GangID) || AreDirectEnemies(target) || AreIndirectEnemies(target));
    }


    public bool AreDirectEnemies(Actor other)
    {
      return other != null && !other.IsDead && (m_AggressorOf != null && m_AggressorOf.Contains(other) || m_SelfDefenceFrom != null && m_SelfDefenceFrom.Contains(other) || (other.IsAggressorOf(this) || other.IsSelfDefenceFrom(this)));
    }

    public bool AreIndirectEnemies(Actor other)
    {
      if (other == null || other.IsDead) return false;
      if (HasLeader) {
        if (m_Leader.AreDirectEnemies(other)) return true;
        if (other.HasLeader && m_Leader.AreDirectEnemies(other.Leader)) return true;
        foreach (Actor follower in m_Leader.Followers) {
          if (follower != this && follower.AreDirectEnemies(other))
            return true;
        }
      }
      if (CountFollowers > 0) {
        foreach (Actor mFollower in m_Followers) {
          if (mFollower.AreDirectEnemies(other)) return true;
        }
      }
      if (other.HasLeader) {
        if (other.Leader.AreDirectEnemies(this)) return true;
        if (HasLeader && other.Leader.AreDirectEnemies(m_Leader)) return true;
        foreach (Actor follower in other.Leader.Followers) {
          if (follower != other && follower.AreDirectEnemies(this))
            return true;
        }
      }
      return false;
    }

    // map-related, loosely
    public bool WouldBeAdjacentToEnemy(Map map,Point p)
    {
      return map.HasAnyAdjacentInMap(p, (Predicate<Point>) (pt =>
      {
          Actor actorAt = map.GetActorAt(pt);
          return null!= actorAt && IsEnemyOf(actorAt);
      }));  
    }

    public bool IsAdjacentToEnemy {
      get {
        return WouldBeAdjacentToEnemy(Location.Map,Location.Position);
      }
    }

    public bool IsBefore(Actor other)
    {
      Map map = Location.Map;
      foreach (Actor actor1 in map.Actors) {
        if (actor1 == this) return true;
        if (actor1 == other) return false;
      }
      return true;
    }

    public bool IsOnCouch {
      get {
        MapObject mapObjectAt = Location.Map.GetMapObjectAt(Location.Position);
        if (mapObjectAt == null) return false;
        return mapObjectAt.IsCouch;
      }
    }

    public bool IsInside {
      get {
        return Location.Map.GetTileAt(Location.Position.X, Location.Position.Y).IsInside;
      }
    }

    public List<Point> OneStepRange(Map m,Point p) {
      IEnumerable<Point> tmp = Direction.COMPASS_LIST.Select(dir=>p+dir).Where(pt=>m.IsWalkableFor(pt,this));
      return tmp.Any() ? tmp.ToList() : null;
    }

    private string ReasonCantBreak(MapObject mapObj)
    { 
      Contract.Requires(null != mapObj);
      if (!Model.Abilities.CanBreakObjects) return "cannot break objects";
      if (IsTired) return "tired";
      DoorWindow doorWindow = mapObj as DoorWindow;
      bool flag = doorWindow != null && doorWindow.IsBarricaded;
      if (mapObj.BreakState != MapObject.Break.BREAKABLE && !flag) return "can't break this object";
      if (mapObj.Location.Actor != null) return "someone is there";
      return "";
    }

    public bool CanBreak(MapObject mapObj, out string reason)
    {
      reason = ReasonCantBreak(mapObj);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBreak(MapObject mapObj)
    {
      return string.IsNullOrEmpty(ReasonCantBreak(mapObj));
    }

    public bool AbleToPush {
      get {
        return Model.Abilities.CanPush || 0<Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG) || 0<Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_STRONG);
      }
    }

    private string ReasonCantPush(MapObject mapObj)
    {
      Contract.Requires(null != mapObj);
      if (!AbleToPush) return "cannot push objects";
      if (IsTired) return "tired";
      if (!mapObj.IsMovable) return "cannot be moved";
      if (mapObj.Location.Actor != null) return "someone is there";
      if (mapObj.IsOnFire) return "on fire";
      return "";
    }

    public bool CanPush(MapObject mapObj, out string reason)
    {
      reason = ReasonCantPush(mapObj);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanPush(MapObject mapObj)
    {
      return string.IsNullOrEmpty(ReasonCantPush(mapObj));
    }

    private string ReasonCantClose(DoorWindow door)
    {
      Contract.Requires(null != door);
      if (!Model.Abilities.CanUseMapObjects) return "can't use objects";
      if (!door.IsOpen) return "not open";
      if (door.Location.Actor != null) return "someone is there";
      return "";
    }

    public bool CanClose(DoorWindow door, out string reason)
    {
      reason = ReasonCantClose(door);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanClose(DoorWindow door)
    {
      return string.IsNullOrEmpty(ReasonCantClose(door));
    }

    private string ReasonCantBarricade(DoorWindow door)
    {
      Contract.Requires(null != door);
      if (!Model.Abilities.CanBarricade) return "no ability to barricade";
      if (!door.IsClosed && !door.IsBroken) return "not closed or broken";
      if (door.BarricadePoints >= Rules.BARRICADING_MAX) return "barricade limit reached";
      if (door.Location.Actor != null) return "someone is there";
      if (Inventory == null || Inventory.IsEmpty) return "no items";
      if (!Inventory.Has<ItemBarricadeMaterial>()) return "no barricading material";
      return "";
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

	private string ReasonCantBash(DoorWindow door)
	{
	  Contract.Requires(null != door);
      if (!Model.Abilities.CanBashDoors) return "can't bash doors";
      if (IsTired) return "tired";
      if (door.BreakState != MapObject.Break.BREAKABLE && !door.IsBarricaded) return "can't break this object";
      return "";
	}

    public bool CanBash(DoorWindow door, out string reason)
    {
	  reason = ReasonCantBash(door);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanBash(DoorWindow door)
    {
	  return string.IsNullOrEmpty(ReasonCantBash(door));
    }

	private string ReasonCantOpen(DoorWindow door)
	{
	  Contract.Requires(null != door);
      if (!Model.Abilities.CanUseMapObjects) return "no ability to open";
	  if (door.BarricadePoints > 0) return "is barricaded";
      if (!door.IsClosed) return "not closed";
      return "";
	}

    public bool CanOpen(DoorWindow door, out string reason)
    {
	  reason = ReasonCantOpen(door);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanOpen(DoorWindow door)
    {
	  return string.IsNullOrEmpty(ReasonCantOpen(door));
    }

	// leave the dead parameter in there, for now.
	// E.g., non-CHAR power generators might actually *need fuel*
	private string ReasonCantSwitch(PowerGenerator powGen)
	{
	  Contract.Requires(null != powGen);
      if (!Model.Abilities.CanUseMapObjects) return "cannot use map objects";
      if (IsSleeping) return "is sleeping";
      return "";
	}

    public bool CanSwitch(PowerGenerator powGen, out string reason)
    {
	  reason = ReasonCantSwitch(powGen);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanSwitch(PowerGenerator powGen)
    {
	  return string.IsNullOrEmpty(ReasonCantSwitch(powGen));
    }

	private string ReasonCantRecharge(Item it)
	{
	  Contract.Requires(null != it);
      if (!Model.Abilities.CanUseItems) return "no ability to use items";
      if (!it.IsEquipped || !Inventory.Contains(it)) return "item not equipped";
      if (!(it is BatteryPowered)) return "not a battery powered item";
      return "";
	}

    public bool CanRecharge(Item it, out string reason)
    {
	  reason = ReasonCantRecharge(it);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanRecharge(Item it)
    {
	  return string.IsNullOrEmpty(ReasonCantRecharge(it));
    }

    // event timing
    public void SpendActionPoints(int actionCost)
    {
      m_ActionPoints -= actionCost;
      m_LastActionTurn = Location.Map.LocalTime.TurnCounter;
    }

    public bool CanActThisTurn {
      get {
        return 0 < m_ActionPoints;
      }
    }

    public bool CanActNextTurn {
      get {
        return 0 < m_ActionPoints + Speed;
      }
    }

    public bool WillActAgainBefore(Actor other)
    {
      return other.ActionPoints <= 0 && (!other.CanActNextTurn || IsBefore(other));
    }

    public int Speed { 
      get {
        float num = Doll.Body.Speed;    // an exhausted, sleepy living dragging a corpse in heavy armor, below 36 here, will have a speed of zero
        if (IsTired) { num *= 2f; num /= 3f; }
        if (IsExhausted) num /= 2f;
        else if (IsSleepy) { num *= 2f; num /= 3f; }
        Engine.Items.ItemBodyArmor itemBodyArmor = GetEquippedItem(DollPart.TORSO) as Engine.Items.ItemBodyArmor;
        if (itemBodyArmor != null) num -= (float) itemBodyArmor.Weight;
        if (DraggedCorpse != null) num /= 2f;
        return Math.Max((int) num, 0);
      }
    }

    // infection
    public int InfectionHPs {
      get {
        return MaxHPs + MaxSTA;
      }
    }
    public void Infect(int i) { 
      if (!Engine.Session.Get.HasInfection) return;    // no-op if mode doesn't have infection
      m_Infection = Math.Min(InfectionHPs, m_Infection + i);
    }

    public void Cure(int i) { 
      m_Infection = Math.Max(0, m_Infection - i);
    }

    public int InfectionPercent {
      get {
        return 100 * m_Infection / InfectionHPs;
      }
    }

    // health
    public int MaxHPs {
      get {
        int num = SKILL_TOUGH_HP_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.TOUGH) + SKILL_ZTOUGH_HP_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_TOUGH);
        return Sheet.BaseHitPoints + num;
      }
    }

    public void RegenHitPoints(int hpRegen)
    {
      m_HitPoints = Math.Min(MaxHPs, m_HitPoints + hpRegen);
    }

    // stamina
    public int NightSTApenalty {
      get { 
        if (!Location.Map.LocalTime.IsNight) return 0;
        if (Model.Abilities.IsUndead) return 0;
        return NIGHT_STA_PENALTY;
      }
    }

    public bool WillTireAfter(int staminaCost)
    {
      if (!Model.Abilities.CanTire) return false;
      if (0 < staminaCost) staminaCost += NightSTApenalty;
      if (IsExhausted) staminaCost *= 2;
      return STAMINA_MIN_FOR_ACTIVITY > m_StaminaPoints-staminaCost;
    }

    public bool WillTireAfterRunning(Point dest)
    {
      MapObject mapObjectAt = Location.Map.GetMapObjectAt(dest);
      if (mapObjectAt != null && !mapObjectAt.IsWalkable && mapObjectAt.IsJumpable) {
        return WillTireAfter(Rules.STAMINA_COST_RUNNING+Rules.STAMINA_COST_JUMP+NightSTApenalty);
      }
      return WillTireAfter(Rules.STAMINA_COST_RUNNING);
    }

    public int MaxSTA {
      get {
        int num = SKILL_HIGH_STAMINA_STA_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.HIGH_STAMINA);
        return Sheet.BaseStaminaPoints + num;
      }
    }

    public bool IsTired {
      get {
        if (!Model.Abilities.CanTire) return false;
        return m_StaminaPoints < STAMINA_MIN_FOR_ACTIVITY;
      }
    }

    private string ReasonCantRun()
    {
      if (!Model.Abilities.CanRun) return "no ability to run";
      if (StaminaPoints < Actor.STAMINA_MIN_FOR_ACTIVITY) return "not enough stamina to run";
      return "";
    }

    public bool CanRun(out string reason)
    {
      reason = ReasonCantRun();
      return string.IsNullOrEmpty(reason);
    }

    public bool CanRun()
    {
      return string.IsNullOrEmpty(ReasonCantRun());
    }

	public bool RunIsFreeMove { get { return Rules.BASE_ACTION_COST/2 < m_ActionPoints; } }

    public bool CanJump {
      get {
       return Model.Abilities.CanJump
            || 0 < Sheet.SkillTable.GetSkillLevel(Skills.IDs.AGILE)
            || 0 < Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_AGILE);
      }
    }

    private string ReasonCantUseExit(Point exitPoint)
    {
      if (Location.Map.GetExitAt(exitPoint) == null) return "no exit there";
      if (!IsPlayer && !Model.Abilities.AI_CanUseAIExits) return "this AI can't use exits";
      if (IsSleeping) return "is sleeping";
      return "";
    }

    public bool CanUseExit(Point exitPoint)
    {
      return string.IsNullOrEmpty(ReasonCantUseExit(exitPoint));
    }

    public bool CanUseExit(Point exitPoint, out string reason)
    {
      reason = ReasonCantUseExit(exitPoint);
      return string.IsNullOrEmpty(reason);
    }

	// Ultimately, we do plan to allow the AI to cross district boundaries
	private string ReasonCantLeaveMap()
	{
      if (!IsPlayer) return "can't leave maps";
      return "";
	}

    public bool CanLeaveMap(out string reason)
    {
	  reason = ReasonCantLeaveMap();
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanLeaveMap()
    {
	  return string.IsNullOrEmpty(ReasonCantLeaveMap());
    }

    // we do not roll these into a setter as no change requires both sets of checks
    public void SpendStaminaPoints(int staminaCost)
    {
      if (Model.Abilities.CanTire) {
        if (Location.Map.LocalTime.IsNight && staminaCost > 0)
          staminaCost += Model.Abilities.IsUndead ? 0 : NIGHT_STA_PENALTY;
        if (IsExhausted) staminaCost *= 2;
        m_StaminaPoints -= staminaCost;
      }
      else
        m_StaminaPoints = STAMINA_INFINITE;
    }

    public void RegenStaminaPoints(int staminaRegen)
    {
      if (Model.Abilities.CanTire)
        m_StaminaPoints = Math.Min(MaxSTA, m_StaminaPoints + staminaRegen);
      else
        m_StaminaPoints = STAMINA_INFINITE;
    }

    // sanity
    public bool IsInsane {
      get {
        if (Model.Abilities.HasSanity) return m_Sanity <= 0;
        return false;
      }
    }

    public int MaxSanity {
      get {
        return Sheet.BaseSanity;
      }
    }

    public void SpendSanity(int sanCost)
    {
      if (!Model.Abilities.HasSanity) return;
      m_Sanity -= sanCost;
      if (m_Sanity < 0) m_Sanity = 0;
    }

    public void RegenSanity(int sanRegen)
    {
      if (!Model.Abilities.HasSanity) return;
      m_Sanity = Math.Min(MaxSanity, m_Sanity + sanRegen);
    }

    public bool IsDisturbed {
      get {
        return Model.Abilities.HasSanity && Sanity <= Engine.Rules.ActorDisturbedLevel(this);
      }
    }

    public int HoursUntilUnstable {
      get {
        int num = Sanity - Engine.Rules.ActorDisturbedLevel(this);
        if (num <= 0) return 0;
        return num / WorldTime.TURNS_PER_HOUR;
      }
    }

    // hunger
    public bool IsHungry {
      get {
        if (Model.Abilities.HasToEat) return FOOD_HUNGRY_LEVEL >= m_FoodPoints;
        return false;
      }
    }

    public bool IsStarving {
      get {
        if (Model.Abilities.HasToEat) return 0 >= m_FoodPoints;
        return false;
      }
    }

    public bool IsRotHungry {
      get {
        if (Model.Abilities.IsRotting) return ROT_HUNGRY_LEVEL >= m_FoodPoints;
        return false;
      }
    }

    public bool IsRotStarving {
      get {
        if (Model.Abilities.IsRotting) return 0 >= m_FoodPoints;
        return false;
      }
    }

    public int MaxFood {
      get {
        int num = (int) ((double) Sheet.BaseFoodPoints * (double) SKILL_LIGHT_EATER_MAXFOOD_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Skills.IDs.LIGHT_EATER));
        return Sheet.BaseFoodPoints + num;
      }
    }

    public int MaxRot {
      get {
        int num = (int) ((double) Sheet.BaseFoodPoints * (double) SKILL_ZLIGHT_EATER_MAXFOOD_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_LIGHT_EATER));
        return Sheet.BaseFoodPoints + num;
      }
    }

    public void Appetite(int f) {
      m_FoodPoints = Math.Max(0, m_FoodPoints - f);
    }

    public void LivingEat(int f) { 
      m_FoodPoints = Math.Min(m_FoodPoints + f, MaxFood);
    }

    public void RottingEat(int f) { 
      m_FoodPoints = Math.Min(m_FoodPoints + f, MaxRot);
    }

    public bool CanEatCorpse {
      get {
        return Model.Abilities.IsUndead || IsStarving || IsInsane;
      }
    }

    // sleep
    public int SleepToHoursUntilSleepy {
      get {
        int num = SleepPoints - SLEEP_SLEEPY_LEVEL;
        if (Location.Map.LocalTime.IsNight) num /= 2;
        if (num <= 0) return 0;
        return num / WorldTime.TURNS_PER_HOUR;
      }
    }

    public bool IsAlmostSleepy {
      get {
        if (!Model.Abilities.HasToSleep) return false;
        return 3 >= SleepToHoursUntilSleepy;
      }
    }

    public bool IsSleepy { 
      get {
        if (Model.Abilities.HasToSleep) return SLEEP_SLEEPY_LEVEL >= SleepPoints;
        return false;
      }
    }

    public bool IsExhausted { 
      get {
        if (Model.Abilities.HasToSleep) return 0 >= SleepPoints;
        return false;
      }
    }

    public bool WouldLikeToSleep {
      get {
        return IsAlmostSleepy /* || IsSleepy */;    // cf above partial ordering
      }
    }

    public int MaxSleep {
      get {
        int num = (int) ((double) Sheet.BaseSleepPoints * (double) SKILL_AWAKE_SLEEP_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Skills.IDs.AWAKE));
        return Sheet.BaseSleepPoints + num;
      }
    }

    public void Rest(int s) {
      m_SleepPoints = Math.Min(m_SleepPoints + s, MaxSleep);
    }

    public void Drowse(int s) {
      m_SleepPoints = Math.Max(0, m_SleepPoints - s);      
    }

    // boring items
    public void AddBoringItem(Item it)
    {
      if (null == m_BoringItems) m_BoringItems = new List<Item>(1);
      else if (m_BoringItems.Contains(it)) return;
      m_BoringItems.Add(it);
    }

    public bool IsBoredOf(Item it)
    {
      if (null == m_BoringItems) return false;
      return m_BoringItems.Contains(it);
    }

    // inventory stats

    // This is the authoritative source for a living actor's maximum inventory.
    // As C# has no analog to a C++ const method or const local variables, 
    // use this to prevent accidental overwriting of MaxCapacity by bugs.
    public int MaxInv {
      get {
        int num = SKILL_HAULER_INV_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.HAULER);
        return Sheet.BaseInventoryCapacity + num;
      }
    }

    public _T_ GetFirstMatching<_T_>(Predicate<_T_> fn) where _T_ : Item
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return null;
      return m_Inventory.GetFirstMatching<_T_>(fn);
    }

    public bool HasItemOfModel(ItemModel model)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.Model == model) return true;
      }
      return false;
    }

    public int CountItemsQuantityOfModel(ItemModel model)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
      int num = 0;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.Model == model) num += obj.Quantity;
      }
      return num;
    }

    public int CountItemQuantityOfType(Type tt)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
      int num = 0;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.GetType() == tt) num += obj.Quantity;
      }
      return num;
    }

    public int CountItemsOfSameType(Type tt)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
      int num = 0;
      foreach (object obj in m_Inventory.Items) {
        if (obj.GetType() == tt) ++num;
      }
      return num;
    }    

    public bool Has<_T_>() where _T_ : Item
    {
      if (Inventory == null || Inventory.IsEmpty) return false;
      return Inventory.Has<_T_>();
    }

    public bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable)
        return CountItemsQuantityOfModel(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
    }

    public ItemMeleeWeapon GetWorstMeleeWeapon()
    {
      if (null == Inventory) return null;
      return Inventory.Items.Select(it=>it as ItemMeleeWeapon).Where(w=>null!=w).Minimize(w=>(w.Model as ItemMeleeWeaponModel).Attack.Rating);
    }

    public ItemBodyArmor GetBestBodyArmor(Predicate<ItemBodyArmor> fn=null)
    {
      if (null == Inventory) return null;
      IEnumerable<ItemBodyArmor> armors = Inventory.Items.Select(it=>it as ItemBodyArmor).Where(armor=>null!=armor);
      if (null!=fn) armors = armors.Where(armor=>fn(armor));
      return armors.Maximize(armor=>armor.Rating);
    }

    public ItemBodyArmor GetWorstBodyArmor()
    {
      if (null == Inventory) return null;
      return Inventory.Items.Select(it=>it as ItemBodyArmor).Where(armor=>null!=armor && DollPart.NONE == armor.EquippedPart).Minimize(armor=>armor.Rating);
    }

    // we prefer to return weapons that need reloading.
    public ItemRangedWeapon GetCompatibleRangedWeapon(ItemAmmo am)
    {
      if (null == Inventory) return null;
      IEnumerable<ItemRangedWeapon> tmp = Inventory.Items.Select(it=>it as ItemRangedWeapon).Where(rw=> null!=rw && rw.AmmoType == am.AmmoType);
      if (!tmp.Any()) return null;
      IEnumerable<ItemRangedWeapon> tmp2 = tmp.Where(rw=> rw.Ammo<(rw.Model as ItemRangedWeaponModel).MaxAmmo);
      return tmp2.FirstOrDefault() ?? tmp.FirstOrDefault();
    }

    public ItemAmmo GetCompatibleAmmoItem(ItemRangedWeapon rw)
    {
      if (null == Inventory) return null;
      IEnumerable<ItemAmmo> tmp = Inventory.Items.Select(it=>it as ItemAmmo).Where(am => am != null && am.AmmoType == rw.AmmoType);
      return tmp.FirstOrDefault();
    }

    // equipped items
    public Item GetEquippedItem(DollPart part)
    {
      if (null == m_Inventory || DollPart.NONE == part) return null;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.EquippedPart == part) return obj;
      }
      return null;
    }

    public Item GetEquippedItem(Gameplay.GameItems.IDs id)
    {
      if (null == m_Inventory) return null;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.Model.ID == id && obj.EquippedPart != DollPart.NONE) return obj;
      }
      return null;
    }

    // this cannot be side-effecting (martial arts, grenades)
    public Item GetEquippedWeapon()
    {
      return GetEquippedItem(DollPart.RIGHT_HAND);
    }

    // maybe this should be over on the Inventory object
    public Item GetItem(Gameplay.GameItems.IDs id)
    {
      if (null == m_Inventory) return null;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.Model.ID == id) return obj;
      }
      return null;
    }

    private string ReasonCantUseItem(Item it)
    {
      Contract.Requires(null != it);
      if (!Model.Abilities.CanUseItems) return "no ability to use items";
      if (it is ItemWeapon) return "to use a weapon, equip it";
      if (it is ItemFood && !Model.Abilities.HasToEat) return "no ability to eat";
      if (it is ItemMedicine && Model.Abilities.IsUndead) return "undeads cannot use medecine";
      if (it is ItemBarricadeMaterial) return "to use material, build a barricade";
      if (it is ItemAmmo)
      {
        ItemAmmo itemAmmo = it as ItemAmmo;
        ItemRangedWeapon itemRangedWeapon = GetEquippedWeapon() as ItemRangedWeapon;
        if (itemRangedWeapon == null || itemRangedWeapon.AmmoType != itemAmmo.AmmoType) return "no compatible ranged weapon equipped";
        if (itemRangedWeapon.Ammo >= (itemRangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo) return "weapon already fully loaded";
      }
      else if (it is ItemSprayScent)
      {
        if (it.IsUseless) return "no spray left.";
      }
      else if (it is ItemTrap)
      {
        if (!(it as ItemTrap).TrapModel.UseToActivate) return "does not activate manually";
      }
      else if (it is ItemEntertainment)
      {
        if (!Model.Abilities.IsIntelligent) return "not intelligent";
        if (IsBoredOf(it)) return "bored by this";
      }
      Inventory inventory = Inventory;
      if (inventory == null || !inventory.Contains(it)) return "not in inventory";
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

    private string ReasonCantGet(Item it)
    {
      Contract.Requires(null != it); 
      if (!Model.Abilities.HasInventory || !Model.Abilities.CanUseMapObjects || Inventory == null) return "no inventory";
      if (Inventory.IsFull && !Inventory.CanAddAtLeastOne(it)) return "inventory is full";
      if (it is ItemTrap && (it as ItemTrap).IsTriggered) return "triggered trap";
      return "";
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

    private string ReasonCantGetFromContainer(Point position)
    {
      MapObject mapObjectAt = Location.Map.GetMapObjectAt(position);
      if (mapObjectAt == null || !mapObjectAt.IsContainer) return "object is not a container";
      Inventory itemsAt = Location.Map.GetItemsAt(position);
      if (itemsAt == null) return "nothing to take there";
	  // XXX should be "can't get any of the items in the container"
      if (!CanGet(itemsAt.TopItem)) return "cannot take an item";
      return "";
    }

	public bool CanGetFromContainer(Point position,out string reason)
	{
	  reason = ReasonCantGetFromContainer(position);
	  return string.IsNullOrEmpty(reason);
	}

	public bool CanGetFromContainer(Point position)
	{
	  return string.IsNullOrEmpty(ReasonCantGetFromContainer(position));
	}


    private string ReasonCantEquip(Item it)
    {
      Contract.Requires(null != it);
      if (!Model.Abilities.CanUseItems) return "no ability to use items";
      if (!it.Model.IsEquipable) return "this item cannot be equipped";
      return "";
    }

    public bool CanEquip(Item it, out string reason)
    {
      reason = ReasonCantEquip(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanEquip(Item it)
    {
      return string.IsNullOrEmpty(ReasonCantEquip(it));
    }

    private string ReasonCantUnequip(Item it)
    {
      Contract.Requires(null != it);
      if (!it.IsEquipped) return "not equipped";
      Inventory inventory = Inventory;
      if (inventory == null || !inventory.Contains(it)) return "not in inventory";
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
      Contract.Requires(null != it);
      if (it.IsEquipped) return "unequip first";
      Inventory inventory = Inventory;
      if (inventory == null || !inventory.Contains(it)) return "not in inventory";
      return "";
    }

    public bool CanDrop(Item it, out string reason)
    {
      reason = ReasonCantDrop(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanDrop(Item it)
    {
      return string.IsNullOrEmpty(ReasonCantDrop(it));
    }

    public Skill SkillUpgrade(Skills.IDs id)
    {
      Sheet.SkillTable.AddOrIncreaseSkill(id);
      Skill skill = Sheet.SkillTable.GetSkill(id);
      if (id == Skills.IDs.HAULER && Inventory != null)
        Inventory.MaxCapacity = MaxInv;
      return skill;
    }

    // flag handling
    private bool GetFlag(Actor.Flags f)
    {
      return (m_Flags & f) != Actor.Flags.NONE;
    }

    private void SetFlag(Actor.Flags f, bool value)
    {
      if (value)
        m_Flags |= f;
      else
        m_Flags &= ~f;
    }

    private void OneFlag(Actor.Flags f)
    {
      m_Flags |= f;
    }

    private void ZeroFlag(Actor.Flags f)
    {
      m_Flags &= ~f;
    }

    // vision
    public int DarknessFOV { 
      get {
        if (Model.Abilities.IsUndead) return Sheet.BaseViewRange;
        return MINIMAL_FOV;
      }
    }

    public int NightFovPenalty(WorldTime time)
    {
      if (Model.Abilities.IsUndead) return 0;
      switch (time.Phase)
      {
        case DayPhase.SUNSET: return FOV_PENALTY_SUNSET;
        case DayPhase.EVENING: return FOV_PENALTY_EVENING;
        case DayPhase.MIDNIGHT: return FOV_PENALTY_MIDNIGHT;
        case DayPhase.DEEP_NIGHT: return FOV_PENALTY_DEEP_NIGHT;
        case DayPhase.SUNRISE: return FOV_PENALTY_SUNRISE;
        default: return 0;
      }
    }

    public int WeatherFovPenalty(Weather weather)
    {
      if (Model.Abilities.IsUndead) return 0;
      switch (weather)
      {
        case Weather.RAIN: return FOV_PENALTY_RAIN;
        case Weather.HEAVY_RAIN: return FOV_PENALTY_HEAVY_RAIN;
        default: return 0;
      }
    }

    int LightBonus { 
      get {
        ItemLight itemLight = GetEquippedItem(DollPart.LEFT_HAND) as ItemLight;
        if (itemLight != null && itemLight.Batteries > 0) return itemLight.FovBonus;
        return 0;
      }
    }

    public int FOVrangeNoFlashlight(WorldTime time, Weather weather)
    {
      if (IsSleeping) return 0;
      int FOV = Sheet.BaseViewRange;
      switch (Location.Map.Lighting) {
        case Lighting.DARKNESS:
          FOV = DarknessFOV;
          break;
        case Lighting.OUTSIDE:
          FOV -= NightFovPenalty(time) + WeatherFovPenalty(weather);
          break;
      }
      if (IsExhausted) FOV -= 2;
      else if (IsSleepy) --FOV;
      MapObject mapObjectAt = Location.Map.GetMapObjectAt(Location.Position);
      if (mapObjectAt != null && mapObjectAt.StandOnFovBonus) ++FOV;
      return Math.Max(MINIMAL_FOV, FOV);
    }

    public int FOVrange(WorldTime time, Weather weather)
    {
      if (IsSleeping) return 0; // repeat this short-circuit here for correctness
      int FOV = FOVrangeNoFlashlight(time, weather);
      Lighting light = Location.Map.Lighting;
      if (light == Lighting.DARKNESS || (light == Lighting.OUTSIDE && time.IsNight)) {
        int lightBonus = LightBonus;
        if (lightBonus == 0) {
          Map map = Location.Map;
          if (map.HasAnyAdjacentInMap(Location.Position, (Predicate<System.Drawing.Point>) (pt =>
              {
                Actor actorAt = map.GetActorAt(pt);
                if (actorAt == null) return false;
                return 0 < actorAt.LightBonus;
              })))
            lightBonus = 1;
        }
        FOV += lightBonus;
      }
      return Math.Max(MINIMAL_FOV, FOV);
    }

    // event handlers
    public void OnEquipItem(Item it)
    {
      if (it.Model is ItemMeleeWeaponModel) {
        m_CurrentMeleeAttack = (it.Model as ItemMeleeWeaponModel).BaseMeleeAttack(Sheet);
        return;
      }
      if (it.Model is ItemRangedWeaponModel) {
        m_CurrentRangedAttack = (it.Model as ItemRangedWeaponModel).Attack;   // value-copy due to struct Attack
        return;
      }
      if (it.Model is ItemBodyArmorModel) {
        m_CurrentDefence += (it.Model as ItemBodyArmorModel).ToDefence();
        return;
      }
      if (it is BatteryPowered) { 
        --(it as BatteryPowered).Batteries;
        if (IsPlayer && it is ItemLight) Controller.UpdateSensors();
        return;
      }
    }

    public void OnUnequipItem(Item it)
    {
      if (it.Model is ItemMeleeWeaponModel) {
        m_CurrentMeleeAttack = Sheet.UnarmedAttack;
        return;
      }
      if (it.Model is ItemRangedWeaponModel) {
        m_CurrentRangedAttack = Attack.BLANK;
        return;
      }
      if (it.Model is ItemBodyArmorModel) {
        m_CurrentDefence -= (it.Model as ItemBodyArmorModel).ToDefence();
        return;
      }
    }

#region Event-based Say implementation
    public struct SayArgs
    {
      public readonly Actor _target;
      public readonly List<Data.Message> messages;
      public readonly bool _important;
      public bool shown;

      public SayArgs(Actor target, bool important)
      {
        _target = target;
        messages = new List<Data.Message>();
        _important = important;
        shown = false;
      }
    }

    public static event EventHandler<SayArgs> Says;

    // experimental...testing an event approach to this
    public void Say(Actor target, string text, Engine.RogueGame.Sayflags flags)
    {
      if ((flags & Engine.RogueGame.Sayflags.IS_FREE_ACTION) == Engine.RogueGame.Sayflags.NONE)
        SpendActionPoints(Engine.Rules.BASE_ACTION_COST);

      EventHandler<SayArgs> handler = Says; // work around non-atomic test, etc.
      if (null != handler) {
        SayArgs tmp = new SayArgs(target,target.IsPlayer || (flags & Engine.RogueGame.Sayflags.IS_IMPORTANT) != Engine.RogueGame.Sayflags.NONE);
        tmp.messages.Add(RogueForm.Game.MakeMessage(this, string.Format("to {0} : ", (object) target.TheName), RogueForm.Game.SAYOREMOTE_COLOR));
        tmp.messages.Add(RogueForm.Game.MakeMessage(this, string.Format("\"{0}\"", (object) text), RogueForm.Game.SAYOREMOTE_COLOR));
        handler(this,tmp);
      }
    }
#endregion
#region Event-based Dies implementation
    public struct DieArgs
    {
      public readonly Actor _deadGuy;
      public readonly Actor _killer;
      public readonly string _reason;

      public DieArgs(Actor deadGuy, Actor killer, string reason)
      {
        _deadGuy = deadGuy;
        _killer = killer;
        _reason = reason;
      }
    }

    public static event EventHandler<DieArgs> Dies;

    public void Killed(string reason, Actor killer=null) {
      EventHandler<DieArgs> handler = Dies; // work around non-atomic test, etc.
      if (null != handler) {
        DieArgs tmp = new DieArgs(this,killer,reason);
        handler(this,tmp);
      }
    }
#endregion

    public static event EventHandler Moving;
    public void Moved() {
      EventHandler handler = Moving; // work around non-atomic test, etc.
      if (null!=handler) handler(this,null);
    }

    // administrative functions whose presence here is not clearly advisable but they improve the access situation here
    public void StartingSkill(Skills.IDs skillID,int n=1)
    {
      while(0< n--) { 
        if (Sheet.SkillTable.GetSkillLevel(skillID) >= Skills.MaxSkillLevel(skillID)) return;
        Sheet.SkillTable.AddOrIncreaseSkill(skillID);
        RecomputeStartingStats();
      }
    }

    public void RecomputeStartingStats()
    {
      m_HitPoints = MaxHPs;
      m_StaminaPoints = MaxSTA;
      m_FoodPoints = MaxFood;
      m_SleepPoints = MaxSleep;
      m_Sanity = MaxSanity;
      if (m_Inventory == null) return;
      m_Inventory.MaxCapacity = MaxInv;
    }

    public void CreateCivilianDeductFoodSleep(Rules r) { 
      m_FoodPoints -= r.Roll(0, m_FoodPoints / 4);
      m_SleepPoints -= r.Roll(0, m_SleepPoints / 4);
    }

    public void AfterSpawn()
    {
      Controller.UpdateSensors();
      Engine.Session.Get.PoliceTrackingThroughExitSpawn(this); // XXX overprecise
    }

    public void AfterAction()
    {
      m_previousHitPoints = m_HitPoints;
      m_previousFoodPoints = m_FoodPoints;
      m_previousSleepPoints = m_SleepPoints;
      m_previousSanity = m_Sanity;
    }

    public void DropScent()
    {
      if (Model.Abilities.IsUndead)
      {
        if (!Model.Abilities.IsUndeadMaster) return;
        Location.Map.RefreshScentAt(Odor.UNDEAD_MASTER, UNDEAD_MASTER_SCENT_DROP, Location.Position);
      }
      else
        Location.Map.RefreshScentAt(Odor.LIVING, LIVING_SCENT_DROP, Location.Position);
    }

    public void PreTurnStart()
    {
       DropScent();
       if (!IsSleeping) m_ActionPoints += Speed;
       if (m_StaminaPoints < MaxSTA) RegenStaminaPoints(STAMINA_REGEN_WAIT);
    }

    // This prepares an actor for being a PC.  Note that hacking the player controller in
    // by hex-editing does work flawlessly at the Actor level.
    public void PrepareForPlayerControl()
    {
      if (Leader != null) Leader.RemoveFollower(this);   // needed if leader is NPC
    }

    // This is a backstop for bugs elsewhere.
    // Just optimize everything that's an Actor or contains an Actor.
    public void OptimizeBeforeSaving()
    {
      if (m_TargetActor != null && m_TargetActor.IsDead) m_TargetActor = null;
      if (m_Leader != null && m_Leader.IsDead) m_Leader = null;
      int i = 0;
      if (null != m_Followers) {
        i = m_Followers.Count;
        while(0 < i--) {
          if (m_Followers[i].IsDead) m_Followers.RemoveAt(i);
        }
        if (0 == m_Followers.Count) m_Followers = null;
      }
      if (null != m_AggressorOf) {
        i = m_AggressorOf.Count;
        while(0 < i--) {
          if (m_AggressorOf[i].IsDead) m_AggressorOf.RemoveAt(i);
        }
        if (0 == m_AggressorOf.Count) m_AggressorOf = null;
      }
      if (null != m_SelfDefenceFrom) {
        i = m_SelfDefenceFrom.Count;
        while(0 < i--) {
          if (m_SelfDefenceFrom[i].IsDead) m_SelfDefenceFrom.RemoveAt(i);
        }
        if (0 == m_SelfDefenceFrom.Count) m_SelfDefenceFrom = null;
      }

      if (null != m_Controller) m_Controller.OptimizeBeforeSaving();
      if (null != m_BoringItems) m_BoringItems.TrimExcess();
    }

	// C# docs indicate using Actor as a key wants these
    public bool Equals(Actor x)
    {
      return m_SpawnTime == x.m_SpawnTime && m_Name == x.m_Name;
    }

    public override bool Equals(object obj)
    {
      Actor tmp = obj as Actor;
      if (null == tmp) return false;
      return Equals(tmp);
    }

    public override int GetHashCode()
    {
      return m_SpawnTime ^ m_Name.GetHashCode();
    }

    [System.Flags]
    private enum Flags
    {
      NONE = 0,
      IS_UNIQUE = 1,
      IS_PROPER_NAME = 2,
      IS_PLURAL_NAME = 4,
      IS_DEAD = 8,
      IS_RUNNING = 16,
      IS_SLEEPING = 32,
    }
  }
}
