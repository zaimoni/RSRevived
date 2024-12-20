﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Actor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define B_MOVIE_MARTIAL_ARTS

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Zaimoni.Data;
using static Zaimoni.Data.Functor;

using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;

using Color = System.Drawing.Color;
using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;
using PowerGenerator = djack.RogueSurvivor.Engine.MapObjects.PowerGenerator;
using Fortification = djack.RogueSurvivor.Engine.MapObjects.Fortification;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public enum Activity    // .NET Core 3.0: conflicts with System.Diagnostics.Activity
    {
        IDLE,
        CHASING,
        FIGHTING,
        TRACKING,
        FLEEING,
        FOLLOWING,
        SLEEPING,
        FOLLOWING_ORDER,
        FLEEING_FROM_EXPLOSIVE,
    }

    [System.Flags]
    public enum Sayflags
    {
        IS_IMPORTANT = 1,
        IS_FREE_ACTION = 2,
        IS_DANGER = 4
    }

    [Serializable]
    public readonly record struct ActorTag(string Name, int SpawnTime): Zaimoni.Serialization.ISerialize
    {
        internal ActorTag(Actor src) : this(src.UnmodifiedName, src.SpawnTime) {}

 #region implement Zaimoni.Serialization.ISerialize
        /* protected */ public ActorTag(Zaimoni.Serialization.DecodeObjects decode) : this(decode.DeserializeString(), decode.DeserializeInt())
        {
        }

        void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode)
        {
            Zaimoni.Serialization.Formatter.Serialize(encode.dest, SpawnTime);
            Zaimoni.Serialization.Formatter.Serialize(encode.dest, Name);
        }
#endregion
    };

  [Serializable]
  public sealed class Actor : IEquatable<Actor>, IDeserializationCallback, ILocation, Zaimoni.Data.INoun
    {
    public const int BASE_ACTION_COST = 100;
    public const int FOOD_HUNGRY_LEVEL = WorldTime.TURNS_PER_DAY;
    public const int ROT_HUNGRY_LEVEL = 2*WorldTime.TURNS_PER_DAY;
    public const int SLEEP_SLEEPY_LEVEL = 30*WorldTime.TURNS_PER_HOUR;
    private const int STAMINA_INFINITE = 99; // Marker.  !Abilities.CanTire typically is checked, so value is almost immaterial.
    public const int STAMINA_MIN_FOR_ACTIVITY = 10; // would space-time scale if stamina itself space-time scaled
    private const int NIGHT_STA_PENALTY = 2;
    public const int STAMINA_REGEN_WAIT = 2;
    public const int TRUST_BOND_THRESHOLD = Rules.TRUST_MAX;
    public const int TRUST_TRUSTING_THRESHOLD = 0 /* RS: 12*WorldTime.TURNS_PER_HOUR */;

    // most/all of these FOV modifiers should space-time scale
    private const short MINIMAL_FOV = 2;
    private const int FOV_PENALTY_SUNSET = 1;
    private const int FOV_PENALTY_EVENING = 2;
    private const int FOV_PENALTY_MIDNIGHT = 3;
    private const int FOV_PENALTY_DEEP_NIGHT = 4;
    private const int FOV_PENALTY_SUNRISE = 2;
    private const int FOV_PENALTY_RAIN = 1;
    private const int FOV_PENALTY_HEAVY_RAIN = 2;
    private const int FOV_BONUS_STANDING_ON_OBJECT = 1;
    private const int MAX_LIGHT_FOV_BONUS = 2;   // XXX should be read from configuration files (lights)
    private const int MAX_BASE_VISION = 8;       // XXX should be read from configuration files (actors); same as rifle range
    public const int  MAX_VISION = MAX_BASE_VISION + FOV_BONUS_STANDING_ON_OBJECT - FOV_PENALTY_SUNSET + MAX_LIGHT_FOV_BONUS;   // should be calculated after configuration files read

    public static double SKILL_AWAKE_SLEEP_BONUS = 0.1;
    public static double SKILL_AWAKE_SLEEP_REGEN_BONUS = 0.17;    // XXX 0.17f makes this useful at L1
    public static double SKILL_CARPENTRY_BARRICADING_BONUS = 0.15;
    public static int SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = 1;
    public static int SKILL_CHARISMATIC_TRADE_BONUS = 10;
    public static int SKILL_CHARISMATIC_TRUST_BONUS = 2;
    public static int SKILL_HARDY_HEAL_CHANCE_BONUS = 1;
    public static int SKILL_HAULER_INV_BONUS = 1;
    public static int SKILL_HIGH_STAMINA_STA_BONUS = 8;
    public static int SKILL_LEADERSHIP_FOLLOWER_BONUS = 1;
    public static double SKILL_LIGHT_EATER_FOOD_BONUS = 0.15;
    public static double SKILL_LIGHT_EATER_MAXFOOD_BONUS = 0.1;
    public static int SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = 20; // alpha10
    public static double SKILL_MEDIC_BONUS = 0.15;
    public static int SKILL_MEDIC_REVIVE_BONUS = 10;
    public static int SKILL_NECROLOGY_CORPSE_BONUS = 4;
    public static int SKILL_NECROLOGY_UNDEAD_BONUS = 2;
    public static double SKILL_STRONG_PSYCHE_ENT_BONUS = 0.15;
    public static double SKILL_STRONG_PSYCHE_LEVEL_BONUS = 0.15;
    public static int SKILL_STRONG_THROW_BONUS = 1;
    public static int SKILL_TOUGH_HP_BONUS = 6;
    public static int SKILL_UNSUSPICIOUS_BONUS = 20;   // alpha10
    public static double SKILL_ZEATER_REGEN_BONUS = 0.2f;
    public static int SKILL_ZGRAB_CHANCE = 4;   // alpha10
    public static double SKILL_ZINFECTOR_BONUS = 0.15f;
    public static double SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = 0.15;
    public static int SKILL_ZTOUGH_HP_BONUS = 4;
    public static double SKILL_ZTRACKER_SMELL_BONUS = 0.1;

    public static int SKILL_AGILE_ATK_BONUS = 2;
    public static int SKILL_BOWS_ATK_BONUS = 10;
    public static int SKILL_BOWS_DMG_BONUS = 4;
    public static int SKILL_FIREARMS_ATK_BONUS = 19;
    public static int SKILL_FIREARMS_DMG_BONUS = 2;
    public static int SKILL_MARTIAL_ARTS_ATK_BONUS = 6;
    public static int SKILL_MARTIAL_ARTS_DMG_BONUS = 2;
    public static int SKILL_STRONG_DMG_BONUS = 2;
    public static int SKILL_ZAGILE_ATK_BONUS = 1;
    public static double SKILL_ZLIGHT_EATER_FOOD_BONUS = 0.1;
    public static int SKILL_ZSTRONG_DMG_BONUS = 2;

    // humanoid inventory slots
    private const int SLOT_H_TORSO = 0;

    private Flags m_Flags;
    private GameActors.IDs m_ModelID;
    private GameFactions.IDs m_FactionID;
#nullable enable
    private string m_Name;
    private ActorController m_Controller;   // use accessor rather than direct update; direct update causes null dereference crash in vision sensor
    private ActorController? m_Subconscious = null; // alternate
    private SkillTable m_Skills;
    public readonly int SpawnTime;
    private Inventory? m_Inventory; // hammerspace inventory
    private InventorySlots? m_InventorySlots; // Angband-ish inventory
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
    private Actor? m_TargetActor;
    private List<Actor>? m_Followers; // leadership fields are AI-specific (ObjectiveAI and dogs).
    private int m_TrustInLeader;
    private Dictionary<Actor,int>? m_TrustDict;
    private List<Actor>? m_AggressorOf;
    private List<Actor>? m_SelfDefenceFrom;
    private int m_Infection;
    private Corpse? m_DraggedCorpse;   // sparse field, correlated with Corpse::DraggedBy
    public int OdorSuppressorCounter;   // sparse field
    public readonly Engine.ActorScoring ActorScoring;
    public Activity Activity;

    [NonSerialized] private Actor? m_Leader;              // Backpointer
    [NonSerialized] private Attack m_CurrentMeleeAttack;    // dataflow candidate
    [NonSerialized] private Attack m_CurrentRangedAttack;    // dataflow candidate
    [NonSerialized] private Defence m_CurrentDefence;    // dataflow candidate
    [NonSerialized] private bool _has_to_eat;

    public ActorModel Model
    {
      get { return GameActors.From(m_ModelID); }
      set { // this must be public due to undead evolution
        m_ModelID = value.ID;
        OnModelSet();
      }
    }

    public bool IsUnique
    {
      get { return GetFlag(Flags.IS_UNIQUE); }
      set { SetFlag(Flags.IS_UNIQUE, value); }
    }
#nullable restore

    public Faction Faction
    {
      get { return GameFactions.From(m_FactionID); }
      set { m_FactionID = value.ID; }
    }

    public bool IsFaction(GameFactions.IDs f) { return m_FactionID == f; }
    public bool IsFaction(Actor other) { return m_FactionID == other.m_FactionID; }

#nullable enable
    public string Name
    {
      get {
        if (!IsPlayer) return m_Name;
        return "(YOU) " + m_Name;
      }
      set {
        m_Name = value.Replace("(YOU) ", "");
      }
    }

    public string UnmodifiedName { get { return m_Name; } }
#nullable restore

    public bool IsProperName
    {
      get { return GetFlag(Flags.IS_PROPER_NAME); }
      set { SetFlag(Flags.IS_PROPER_NAME, value); }
    }

    // while a general noun interface would have to allow detection of singular vs plural naming, this would matter only for e.g. a swarm of rats.
#if OBSOLETE
    public bool IsPluralName
    {
      get {
        return GetFlag(Actor.Flags.IS_PLURAL_NAME);
      }
      private set {
        SetFlag(Actor.Flags.IS_PLURAL_NAME, value);
      }
    }
#else
    public readonly bool IsPluralName;
#endif

#nullable enable
    public string TheName
    {
      get {
        if (!IsProperName && !IsPluralName)
          return "the " + m_Name;
        return Name;
      }
    }
    public string UnknownPronoun { get => "someone"; }

    public string HisOrHer {    // 3rd person possessive pronoun; translation target
      get { return Model.DollBody.IsMale ? "his" : "her"; }
    }

    public string HeOrShe {    // 3rd person subject pronoun; translation target
      get { return Model.DollBody.IsMale ? "he" : "she"; }
    }

    public string HimOrHer {    // 3rd person direct object pronoun; translation target
      get { return Model.DollBody.IsMale ? "him" : "her"; }
    }

    // alpha10
    public string HimselfOrHerself {    // 3rd person "emphatic" pronoun e.g. "he himself"; translation target
       get { return Model.DollBody.IsMale ? "himself" : "herself"; }
    }
#nullable restore

    public ActorController Controller
    {
      get {
        if (WantPConTop) {
          if (m_Controller is not PlayerController) {
            if (m_Subconscious is not PlayerController) m_Subconscious = new PlayerController(this);
            return m_Subconscious;
          }
        }
        return m_Controller;
      }
      private set {
        int playerDelta = 0;
        if (null != m_Controller) {
          if (m_Controller is PlayerController) playerDelta -= 1;

          m_Controller.LeaveControl();
        }
        m_Controller = value;
        if (null != m_Controller) {
          m_Controller.TakeControl();
          if (m_Controller is PlayerController) playerDelta += 1;
          if (null != m_Subconscious && m_Controller.GetType() == m_Subconscious.GetType()) m_Subconscious = null;
        }
        if (0 != playerDelta) m_Location.Map?.Players.Recalc();
      }
    }

    public void MakePC() {
        if (m_Controller is PlayerController) return; // XXX invariant violation
        m_Subconscious = null;
        m_Controller?.LeaveControl();
        m_Controller = new PlayerController(this);
        m_Controller.TakeControl();
        m_Location.Map?.Players.Recalc();
    }

    public void UpgradePC() {
        if (m_Controller is not PlayerController pc) return; // XXX invariant violation
        m_Subconscious = null;
        m_Controller?.LeaveControl();
        m_Controller = new PlayerController(this, pc);
        m_Controller.TakeControl();
    }

    public ActorController AsAI {
      get {
        if (m_Controller is PlayerController) {
          return m_Subconscious ??= Model.InstanciateController(this);
        }
        return m_Controller;
      }
    }


#nullable enable
    public bool IsPlayer { get => Controller is PlayerController; }

    public GameGangs.IDs GangID { get { return m_Controller.GangID; } }

    public bool IsInAGang { get => GameGangs.IDs.NONE != GangID; }
    public ref readonly Doll Doll { get { return ref m_Doll; } }

    public bool IsDead {
      get { return GetFlag(Flags.IS_DEAD); }
      set { SetFlag(Flags.IS_DEAD, value); }
    }
    public static bool IsDeceased(Actor a) { return a.IsDead; }

    public bool IsSleeping {
      get { return GetFlag(Flags.IS_SLEEPING); }
      set { SetFlag(Flags.IS_SLEEPING, value); }
    }
    static public bool isAwake(Actor a) { return !a.IsSleeping; }

    public bool IsRunning {
      get { return GetFlag(Flags.IS_RUNNING); }
      set { SetFlag(Flags.IS_RUNNING, value); }
    }
    public void Walk() { Clear(Flags.IS_RUNNING); }
    public void Run() {
      if (CanRun() && !IsRunning) {
        if (IsCrouching) StandUp();
        Set(Flags.IS_RUNNING);
      }
    }

    // 2023-04-22: new posture crouching
    // This is to allow using ground inventories without stepping over them (i.e., lower what arm height means)
    // This is NOT something we want to use keystrokes on, at least without good reason.
    // we might want to allow manual crouching if we got useful stealth gameplay (e.g., crouching behind a window might be
    // good for cover against ranged weapons, or creating asymmetric LoS)
    // for now, we don't worry about stamina costs (STA for standing up is defensible)
    // crouch-walk-run stance changes are "free" because our time resolution is so large (2 minutes/turn)
    public bool IsCrouching {
      get { return GetFlag(Flags.IS_CROUCHING); }
      set { SetFlag(Flags.IS_CROUCHING, value); }
    }
    public void StandUp() { Clear(Flags.IS_CROUCHING); }
    public void Crouch() {
      if (CanCrouch() && !IsCrouching) {
        if (IsRunning) Walk();
        Set(Flags.IS_CROUCHING);
      }
    }

    public Inventory? Inventory { get => m_Inventory; }
    public InventorySlots? InventorySlots { get => m_InventorySlots; }

    public int HitPoints { get => m_HitPoints; }
    public int PreviousHitPoints { get => m_previousHitPoints; }
    public int StaminaPoints { get => m_StaminaPoints; }

    public int PreviousStaminaPoints {
      get => m_previousStamina;
      set { m_previousStamina = value; }
    }

    public int FoodPoints { get => m_FoodPoints; }
    public int PreviousFoodPoints { get => m_previousFoodPoints; }
    public int SleepPoints { get => m_SleepPoints; }
    public int PreviousSleepPoints { get => m_previousSleepPoints; }
    public int Sanity { get => m_Sanity; }
    public int PreviousSanity { get => m_previousSanity; }
    public SkillTable MySkills { get => m_Skills; }

    public int ActionPoints { get => m_ActionPoints; }
    public void APreset() { m_ActionPoints = 0; }

    public Location Location { // \todo replace this with a field if setting up access controls is not worth it
      get => m_Location;
      set { m_Location = value; }
    }

    public bool IsAvailableToHelp { get => Activity.IDLE == Activity || Activity.FOLLOWING == Activity; }
    public bool IsEngaged { get => Activity.CHASING == Activity || Activity.FIGHTING == Activity; }

    public Actor? TargetActor { // \todo replace this with a field if setting up access controls is not worth it
      get => m_TargetActor;
      set { m_TargetActor = value; }
    }

    // might be useful for e.g. deafness from being too close to an explosion
    public int AudioRange { get => Model.BaseAudioRange; }

    public ref Attack CurrentMeleeAttack { get => ref m_CurrentMeleeAttack; }
    public ref Attack CurrentRangedAttack { get => ref m_CurrentRangedAttack; }
    public ref Defence CurrentDefence { get => ref m_CurrentDefence; }

    // Leadership
    public Actor? Leader { get => m_Leader; }
    public Actor? LiveLeader { get => (null != m_Leader && !m_Leader.IsDead) ? m_Leader : null; }
    public bool HasLeader { get => !(m_Leader?.IsDead ?? true); }
    // null != LiveLeader is equivalent to HasLeader

    public int TrustInLeader {
      get => m_TrustInLeader;
      set { m_TrustInLeader = value; }  // \todo 4 change targets to eliminate public setter; CPU cost may be excessive
    }

    public bool IsTrustingLeader {
      get => HasLeader && TRUST_TRUSTING_THRESHOLD <= m_TrustInLeader;
    }

    public bool HasBondWith(Actor target)
    {
      if (m_Leader == target)      return TRUST_BOND_THRESHOLD <= m_TrustInLeader;
      if (target.m_Leader == this) return TRUST_BOND_THRESHOLD <= target.m_TrustInLeader;
      return false;
    }

    public IEnumerable<Actor>? Followers { get { return m_Followers; } }

    public Actor? FirstFollower(Predicate<Actor> test) {
      if (null != m_Followers) foreach(var fo in m_Followers) if (test(fo)) return fo;
      return null;
    }

    public void DoForAllFollowers(Action<Actor> op)
    {
      if (null != m_Followers) foreach(var fo in m_Followers) op(fo);
    }

    public int CountFollowers { get { return m_Followers?.Count ?? 0; } }

    public int MaxFollowers {
      get {
        return SKILL_LEADERSHIP_FOLLOWER_BONUS * MySkills.GetSkillLevel(Skills.IDs.LEADERSHIP);
      }
    }

    public List<KeyValuePair<int, Actor>>? DistantFollowers(int max_dist, int want_nearby)
    {
        if (null == m_Followers) return null;
        if (0 >= want_nearby) return null;

        List<KeyValuePair<int, Actor>> ret = new();
        int closeCount = 0;
        foreach (Actor follower in m_Followers) {
            int dist = Rules.InteractionDistance(follower.Location, in m_Location);
            if (dist <= max_dist && ++closeCount >= want_nearby) return null;
            ret.Add(new(dist, follower));
        }
        return ret;
    }
#nullable restore

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
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      if (IsEnemyOf(target)) return "enemy";
      if (Faction != target.Faction && target.Faction.LeadOnlyBySameFaction) return string.Format("{0} can't lead {1}", Faction.Name, target.Faction.Name);
      // to support savefile hacking.  AI in charge of player is a problem.
      if (target.IsPlayer && !IsPlayer) return "is player";
      return "";
    }

    private string ReasonCantTakeLeadOf(Actor target)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      var ret = target.ReasonCannotBeLed();
      if (!string.IsNullOrEmpty(ret)) return ret;
      ret = ReasonCannotLead();
      if (!string.IsNullOrEmpty(ret)) return ret;
      return ReasonCannotLead(target);
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

    private string ReasonIllegalToLead(Actor target)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      var ret = target.ReasonIllegalToBeLed();
      if (!string.IsNullOrEmpty(ret)) return ret;
      ret = ReasonIllegalToLead();
      if (!string.IsNullOrEmpty(ret)) return ret;
      return ReasonCannotLead(target);
    }

    public bool LegalToTakeLeadOf(Actor target, out string reason)
    {
      reason = ReasonIllegalToLead(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool LegalToTakeLeadOf(Actor target)
    {
      return string.IsNullOrEmpty(ReasonIllegalToLead(target));
    }

#nullable enable
    private string ReasonCantCancelLead(Actor target)
    {
      if (target.Leader != this) return "not your follower";
      if (target.IsSleeping) return "sleeping";
      return "";
    }

    public bool CanCancelLead(Actor target, out string reason)
    {
      reason = ReasonCantCancelLead(target);
      return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanCancelLead(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantCancelLead(target));
    }
#endif

    private string ReasonCantShout()    // this is here because there could be equipment that blocks shouting
    {
      if (!Model.Abilities.CanTalk) return "can't talk";
      return "";
    }

    public bool CanShout(out string reason)
    {
	  reason = ReasonCantShout();
	  return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanShout()
    {
	  return string.IsNullOrEmpty(ReasonCantShout());
    }
#endif

	private string ReasonCantChatWith(Actor target)
	{
      if (!Model.Abilities.CanTalk) return "can't talk";
      if (!target.Model.Abilities.CanTalk) return string.Format("{0} can't talk", target.TheName);
      if (target.IsSleeping) return string.Format("{0} is sleeping", target.TheName);
      return "";
	}

    public bool CanChatWith(Actor target, out string reason)
    {
	  reason = ReasonCantChatWith(target);
	  return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanChatWith(Actor target)
    {
	  return string.IsNullOrEmpty(ReasonCantChatWith(target));
    }
#endif
#nullable restore

	private string ReasonCantSwitchPlaceWith(Actor target)
	{
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
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
    public int KillsCount { get { return ActorScoring.KillsCount; } }

#if DEAD_FUNC
    public IEnumerable<Actor> AggressorOf { get { return m_AggressorOf; } }
    public int CountAggressorOf { get { return m_AggressorOf?.Count ?? 0; } }
    public IEnumerable<Actor> SelfDefenceFrom { get { return m_SelfDefenceFrom; } }
    public int CountSelfDefenceFrom { get { return m_SelfDefenceFrom?.Count ?? 0; } }
#endif

#nullable enable
    public int MurdererSpottedByChance(Actor spotter)
    {
      const int MURDERER_SPOTTING_BASE_CHANCE = 5;
      const int MURDER_SPOTTING_MURDERCOUNTER_BONUS = 5;
      const int MURDERER_SPOTTING_DISTANCE_PENALTY = 1;
      return MURDERER_SPOTTING_BASE_CHANCE + MURDER_SPOTTING_MURDERCOUNTER_BONUS * Session.Get.Police.CountCapitalCrimes(this) - MURDERER_SPOTTING_DISTANCE_PENALTY * Rules.InteractionDistance(spotter.Location, Location);
    }

    public int MurdersInProgress {
      get {
        // Even if this were not an apocalypse, law enforcement should get some slack in interpreting intent, etc.
        int ret = 0;
        if (null != m_AggressorOf) {
          if (IsHungry && !Model.Abilities.IsLawEnforcer) ret += m_AggressorOf.Count(a => null != a?.m_Inventory?.GetItemsByType<ItemFood>());
          if (!Model.Abilities.IsLawEnforcer) ret += m_AggressorOf.Count(a => a.Model.Abilities.IsLawEnforcer);
        }
        return ret;
      }
    }

    public int UnsuspicousForChance(Actor observer)
    {
      const int UNSUSPICIOUS_BAD_OUTFIT_PENALTY = 75;   // these two are logically independent
      const int UNSUSPICIOUS_GOOD_OUTFIT_BONUS = 75;
      int baseChance = SKILL_UNSUSPICIOUS_BONUS * MySkills.GetSkillLevel(Skills.IDs.UNSUSPICIOUS);

      // retain general-purpose code within the cases
      var armor = GetEquippedArmor();
      if (null != armor && !armor.IsNeutral) {
        int bonus() {
          switch(observer.Faction.ID) {
            case GameFactions.IDs.ThePolice:
              if (armor.IsHostileForCops()) return UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
              else if (armor.IsFriendlyForCops()) return UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
              break;
            case GameFactions.IDs.TheBikers:
            case GameFactions.IDs.TheGangstas:
              if (armor.IsHostileForBiker(observer.GangID)) return UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
              else if (armor.IsFriendlyForBiker(observer.GangID)) return UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
            break;
          }
          return 0;
        }

        baseChance += bonus();
      }
      return baseChance;
    }

    public bool IsUnsuspiciousFor(Actor observer) { return Rules.Get.RollChance(UnsuspicousForChance(observer)); }
#nullable restore

    public bool IgnoredByPolice(Actor observer) {
      if (100 <= UnsuspicousForChance(observer)) return true;
      if (0 >= MurdererSpottedByChance(observer)) return true;
      return false;
    }

    public int MurdersOnRecord(Actor observer) {
      int circumstantial = MurdersInProgress;
      if (!observer.Faction.IsEnemyOf(Faction) && observer.Model.Abilities.IsLawEnforcer && IsEnemyOf(observer)) circumstantial += 1;
      if (IgnoredByPolice(observer)) return circumstantial;
      int murders = Session.Get.Police.CountCapitalCrimes(this); // not really, this aggregates murders and assaults
      return murders + circumstantial;
    }

    public bool IsFirstClassCitizen()
    {
        if (Model.Abilities.IsLawEnforcer) return true;
        if (Faction.ID.ExtortionIsAggression()) return true;
        return false;
    }

    public string? AIwillNotMurder { get {
            if (Model.Abilities.IsLawEnforcer) return "police do not murder";
            var leader = LiveLeader;
            if (null != leader && leader.Model.Abilities.IsLawEnforcer) return "deputies do not murder";
            if (Faction.ID.ExtortionIsAggression()) return "authorities do not murder";
            return null;
        }
    }

    public void HasMurdered(Actor victim)
    {
#if DEBUG
        if (!IsPlayer) { // oversimplify things.  Would not be true if the apocalypse ended.
          var err = AIwillNotMurder;
          if (!string.IsNullOrEmpty(err)) throw new InvalidOperationException(err);
        }
#endif
        ActorScoring.AddEvent(Engine.Session.Get.WorldTime.TurnCounter, string.Format("Murdered {0} a {1}!", victim.TheName, victim.Model.Name));
    }

    public int Infection { get { return m_Infection; } }
#nullable enable
    // \todo build out living AI for dragging corpses
    public Corpse? DraggedCorpse { get { return m_DraggedCorpse; } }

    public void StartDragging(Corpse c)
    {
      m_DraggedCorpse = c; // \todo? call StopDraggingCorpse to get that message (RS Alpha does not do this)
      this.PlayersInLOS()?.RedrawPlayScreen(RogueGame.MakePanopticMessage(this, string.Format("{0} dragging {1} corpse.", RogueGame.VERB_START.Conjugate(this), c.DeadGuy.Name)));
    }

    public void StopDraggingCorpse()
    {
      var dead_name = m_DraggedCorpse?.DeadGuy.Name;
      if (null != dead_name) {
        m_DraggedCorpse = null;
        this.PlayersInLOS()?.RedrawPlayScreen(RogueGame.MakePanopticMessage(this, string.Format("{0} dragging {1} corpse.", RogueGame.VERB_STOP.Conjugate(this), dead_name)));
      }
    }
#nullable restore

#if TRACER
    private bool WantPConTop {
      get {
        return false;
      }
    }
#else
    private const bool WantPConTop = false;
#endif

    // has to be accessor, due to usage outside of Actor
    public bool IsDebuggingTarget {
      get {
#if TRACER
        if (   Controller is Gameplay.AI.CivilianAI
            && !IsSleeping
            && Controller.InCombat) return true;
/*      if (Controller is Gameplay.AI.CivilianAI && Location.Map == Location.Map.District.EntryMap && new Rectangle(new Point(0,0),new Point(3,3)).Contains(Location.Map.District.WorldPosition)) return true;
        var denorm = RogueGame.Player.Location.Map.Denormalize(Location);
        if (   Controller is Gameplay.AI.CivilianAI
            && null != denorm
            && RogueGame.Player.Location.LocalView.Contains(denorm.Value.Position)) return true;
*/
#endif
        return false;
      }
    }

    // 2024-11-01: NPC viewpoint support removed as obsolescent for debugging
    public bool IsViewpoint { get => IsPlayer /* || IsDebuggingTarget */; } // i.e., is camera viewpoint
//  public bool IsNPCviewpoint { get { return IsDebuggingTarget; } }

    [NonSerialized] private string[]? _force_PC_names = null; // only used below
    public void CommandLinePlayer() // would prefer private
    {
      // command-line option override
      if (null == _force_PC_names) {
        if (Engine.Session.CommandLineOptions.ContainsKey("PC")) {
          _force_PC_names = Engine.Session.CommandLineOptions["PC"].Split('\0');
        } else {
          _force_PC_names = Array.Empty<string>();
        }
      }
      if (0<_force_PC_names.Length && _force_PC_names.Contains(UnmodifiedName) && !IsPlayer) {
        Controller = new PlayerController(this);
        Session.Get.Scoring.UseReincarnation(); // intentionally unconditional; this is the command line
      }
    }

#nullable enable
    public Actor(ActorModel model, Faction faction, int spawnTime, string name="")
    {
      m_ModelID = model.ID;
      m_FactionID = faction.ID;
      m_Name = string.IsNullOrEmpty(name) ? model.Name : name;
      IsProperName = false;
      IsPluralName = false;
      m_Location = new Location();
      SpawnTime = spawnTime;
      IsUnique = false;
      IsDead = false;
      ActorScoring = new ActorScoring(this);
      CommandLinePlayer();
      OnModelSet();
      Controller = Model.InstanciateController(this);
    }

    public void Retype(ActorModel model) { m_ModelID = model.ID; }

    private void OnModelSet()
    {
      ActorModel model = Model;
      _has_to_eat = model.Abilities.HasToEat;
      m_Doll = new Doll(model);
      m_Skills = new();
      m_ActionPoints = model.DollBody.Speed;
      m_HitPoints = m_previousHitPoints = model.BaseHitPoints;
      m_StaminaPoints = m_previousStamina = model.BaseStaminaPoints;
      m_FoodPoints = m_previousFoodPoints = model.BaseFoodPoints;
      m_SleepPoints = m_previousSleepPoints = model.BaseSleepPoints;
      m_Sanity = m_previousSanity = model.BaseSanity;
      m_Inventory = (model.Abilities.HasInventory ? new Inventory(model.BaseInventoryCapacity)
                                                  : null); // any previous inventory will be irrevocably destroyed
      // just human-torso slot, for now
      m_InventorySlots = (model.Abilities.HasInventory ? new InventorySlots(1)
                                                       : null); // any previous inventory will be irrevocably destroyed
      m_CurrentMeleeAttack = model.UnarmedAttack;
      m_CurrentDefence = Model.BaseDefence;
      m_CurrentRangedAttack = Attack.BLANK;
    }

    [OnDeserialized] private void OnDeserialized(StreamingContext context)
    {
      _has_to_eat = Model.Abilities.HasToEat;
      var w = GetEquippedWeapon();
      var rw = w as ItemRangedWeapon;
      m_CurrentRangedAttack = (null != rw) ? rw.Model.Attack : Attack.BLANK;
      var melee = w as ItemMeleeWeapon;
      m_CurrentMeleeAttack = (null != melee) ? melee.Model.BaseMeleeAttack(Model) : Model.UnarmedAttack;

      m_CurrentDefence = Model.BaseDefence;
      var armor = GetEquippedArmor();
      if (null != armor) m_CurrentDefence += armor.Model.ToDefence();
    }

    void IDeserializationCallback.OnDeserialization(object? sender)
    {
      if (null != m_Followers) foreach(var fo in m_Followers) fo.m_Leader = this;
    }

    public void RepairLoad()
    {
      CommandLinePlayer();
      // Support savefile hacking.
      // If the controller is null, intent was to hand control from the player to the AI.
      // Give them AI controllers here.
      if (null == m_Controller) Controller = Model.InstanciateController(this);

      Controller.RepairLoad();
    }


    public void RevertToAI() {
      if (m_Controller is PlayerController pc) {
        var subc = Interlocked.Exchange(ref m_Subconscious, null);
        if (null != subc) {
          if (!(subc is PlayerController)) {
            Controller = subc;
            return;
          }
        }
        Controller = Model.InstanciateController(this);
        return;
      }
      if (m_Subconscious is PlayerController) m_Subconscious = null;
      if (null == m_Controller) Controller = Model.InstanciateController(this);
    }

	public void PrefixName(string prefix) => m_Name = prefix+" "+m_Name;

    public int DamageBonusVsUndeads {
      get {
        return SKILL_NECROLOGY_UNDEAD_BONUS * MySkills.GetSkillLevel(Skills.IDs.NECROLOGY);
      }
    }

    public int MaxThrowRange(int baseRange)
    {
      return baseRange + SKILL_STRONG_THROW_BONUS * MySkills.GetSkillLevel(Skills.IDs.STRONG);
    }

  // alpha10
  /// <summary>
  /// Estimate chances to hit with a ranged attack. <br></br>
  /// Simulate a large number of rolls attack vs defence and returns % of hits.
  /// </summary>
  /// <param name="actor"></param>
  /// <param name="target"></param>
  /// <param name="shotCounter">0 for normal shot, 1 for 1st rapid fire shot, 2 for 2nd rapid fire shot</param>
  /// <returns>[0..100]</returns>
  public int ComputeChancesRangedHit(Actor target, int shotCounter)
  {
#if DEBUG
    if (0 > shotCounter || 2 < shotCounter) throw new ArgumentOutOfRangeException(nameof(shotCounter));
#endif
    Attack attack = RangedAttack(Rules.InteractionDistance(in m_Location,target.Location),target);
    Defence defence = target.Defence;

    int hitValue = (shotCounter == 0 ? attack.HitValue : shotCounter == 1 ? attack.Hit2Value : attack.Hit3Value);
    int defValue = defence.Value;

    float ranged_hit = Rules.SkillProbabilityDistribution(defValue).LessThan(Rules.SkillProbabilityDistribution(hitValue));
    return (int)(100* ranged_hit);
  }

    // strictly speaking, 1 step is allowed but we do not check LoF here
    private string ReasonCouldntFireAt(Actor target)
    {
      if (!(GetEquippedWeapon() is ItemRangedWeapon itemRangedWeapon)) return "no ranged weapon equipped";
      if (m_CurrentRangedAttack.Range+1 < Rules.InteractionDistance(in m_Location, target.Location)) return "out of range";
      if (itemRangedWeapon.Ammo <= 0) return "no ammo left";
      if (target.IsDead) return "already dead!";
      return "";
    }

#if DEAD_FUNC
    public bool CouldFireAt(Actor target, out string reason)
    {
      reason = ReasonCouldntFireAt(target);
      return string.IsNullOrEmpty(reason);
    }
#endif

    public bool CouldFireAt(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCouldntFireAt(target));
    }

#if DEAD_FUNC
    // this one is very hypothetical -- note absence of ranged weapon validity checks
    private string ReasonCouldntFireAt(Actor target, int range)
    {
      if (range+1 < Rules.InteractionDistance(Location, target.Location)) return "out of range";
      if (target.IsDead) return "already dead!";
      return "";
    }

    public bool CouldFireAt(Actor target, int range, out string reason)
    {
      reason = ReasonCouldntFireAt(target,range);
      return string.IsNullOrEmpty(reason);
    }

    public bool CouldFireAt(Actor target, int range)
    {
      return string.IsNullOrEmpty(ReasonCouldntFireAt(target,range));
    }
#endif

    private string ReasonCantFireAt(Actor target, List<Point> LoF)
    {
      LoF?.Clear();
      if (!(GetEquippedWeapon() is ItemRangedWeapon itemRangedWeapon)) return "no ranged weapon equipped";
      int dist = Rules.InteractionDistance(in m_Location, target.Location);
      var range = m_CurrentRangedAttack.Range;
      if (range < dist) return "out of range";
      if (itemRangedWeapon.Ammo <= 0) return "no ammo left";
      if (1<dist && !LOS.CanTraceFireLine(in m_Location, target.Location, range, LoF)) return "no line of fire";
      if (target.IsDead) return "already dead!";
      return "";
    }

    public bool CanFireAt(Actor target, List<Point> LoF, out string reason)
    {
      reason = ReasonCantFireAt(target,LoF);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanFireAt(Actor target, List<Point> LoF)
    {
      return string.IsNullOrEmpty(ReasonCantFireAt(target, LoF));
    }

    public bool CanFireAt(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantFireAt(target, null));
    }

    // very hypothetical -- lack of ranged weapon validity checks
    private string ReasonCantFireAt(Actor target, int range, List<Point> LoF)
    {
      LoF?.Clear();
      int dist = Rules.InteractionDistance(in m_Location, target.Location);
      if (range < dist) return "out of range";
      if (1<dist && !LOS.CanTraceFireLine(in m_Location, target.Location, range, LoF)) return "no line of fire";
      if (target.IsDead) return "already dead!";
      return "";
    }

#if DEAD_FUNC
    public bool CanFireAt(Actor target, int range, List<Point> LoF, out string reason)
    {
      reason = ReasonCantFireAt(target,range,LoF);
      return string.IsNullOrEmpty(reason);
    }
#endif

    public bool CanFireAt(Actor target, int range)
    {
      return string.IsNullOrEmpty(ReasonCantFireAt(target,range,null));
    }

    private string ReasonCantContrafactualFireAt(Actor target, in Point p)
    {
      var range = m_CurrentRangedAttack.Range;
      if (range < Rules.GridDistance(in p, target.Location.Position)) return "out of range";
      if (!LOS.CanTraceHypotheticalFireLine(new Location(Location.Map,p), target.Location, range, this)) return "no line of fire";
      return "";
    }

#if DEAD_FUNC
	public bool CanContrafactualFireAt(Actor target, Point p, out string reason)
	{
	  reason = ReasonCantContrafactualFireAt(target, in p);
	  return string.IsNullOrEmpty(reason);
	}
#endif

    public bool CanContrafactualFireAt(Actor target, Point p)
	{
	  return string.IsNullOrEmpty(ReasonCantContrafactualFireAt(target, in p));
	}

#if B_MOVIE_MARTIAL_ARTS
    public int UsingPolearmInBMovie {
      get {
        if (IsRunning) return 0;
        if (GetEquippedWeapon() is ItemMeleeWeapon melee && melee.Model.IsMartialArts) {
          if (Gameplay.Item_IDs.UNIQUE_FATHER_TIME_SCYTHE != melee.ModelID) return 0; // Cf Tai Chi for why the scythe can be weaponized
          return MySkills.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
        }
        return 0;
      }
    }
#endif

    public string ReasonCantMeleeAttack(Actor target)
    {
#if B_MOVIE_MARTIAL_ARTS
      bool in_range = Rules.IsAdjacent(in m_Location, target.Location);
      // even martial arts 1 unlocks extended range.
      if (!in_range && 0<UsingPolearmInBMovie && 2==Rules.GridDistance(in m_Location,target.Location)) in_range = true;
      if (!in_range) return "not adjacent";
#else
      if (!Rules.IsAdjacent(Location, target.Location)) return "not adjacent";
#endif
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

    public Defence Defence {
      get {
        if (IsSleeping) return Defence.BLANK;
        var skills = MySkills;
        int num1 = Rules.SKILL_AGILE_DEF_BONUS * skills.GetSkillLevel(Skills.IDs.AGILE) + Rules.SKILL_ZAGILE_DEF_BONUS * skills.GetSkillLevel(Skills.IDs.Z_AGILE);
        float num2 = (float) (m_CurrentDefence.Value + num1);
        if (IsExhausted) num2 /= 2f;
        else if (IsSleepy) num2 *= 0.75f;
        return new Defence((int) num2, m_CurrentDefence.Protection_Hit, m_CurrentDefence.Protection_Shot);
      }
    }

    public Attack MeleeWeaponAttack(ItemMeleeWeaponModel model, Actor? target = null)
    {
      Attack baseAttack = model.BaseMeleeAttack(Model);
      var skills = MySkills;
      int hitBonus = SKILL_AGILE_ATK_BONUS * skills.GetSkillLevel(Skills.IDs.AGILE) + SKILL_ZAGILE_ATK_BONUS * skills.GetSkillLevel(Skills.IDs.Z_AGILE);
      int damageBonus = SKILL_STRONG_DMG_BONUS * skills.GetSkillLevel(Skills.IDs.STRONG) + SKILL_ZSTRONG_DMG_BONUS * skills.GetSkillLevel(Skills.IDs.Z_STRONG);
      if (model.IsMartialArts) {
        int skill = skills.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
        if (0!=skill) {
          hitBonus += SKILL_MARTIAL_ARTS_ATK_BONUS * skill;
          damageBonus += SKILL_MARTIAL_ARTS_DMG_BONUS * skill;
        }
      }
      if (target?.Model.Abilities.IsUndead ?? false) damageBonus += DamageBonusVsUndeads;
      float hit = (float)baseAttack.HitValue + (float) hitBonus;
      if (IsExhausted) hit /= 2f;
      else if (IsSleepy) hit *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) hit, baseAttack.DamageValue + damageBonus, baseAttack.StaminaPenalty);
    }

    public Attack MeleeWeaponAttack(ItemMeleeWeaponModel model, MapObject objToBreak)
    {
      Attack baseAttack = model.BaseMeleeAttack(Model);
      var skills = MySkills;
      int hitBonus = SKILL_AGILE_ATK_BONUS * skills.GetSkillLevel(Skills.IDs.AGILE) + SKILL_ZAGILE_ATK_BONUS * skills.GetSkillLevel(Skills.IDs.Z_AGILE);
      int damageBonus = SKILL_STRONG_DMG_BONUS * skills.GetSkillLevel(Skills.IDs.STRONG) + SKILL_ZSTRONG_DMG_BONUS * skills.GetSkillLevel(Skills.IDs.Z_STRONG);
      if (model.IsMartialArts) {
        int skill = skills.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
        if (0!=skill) {
          hitBonus += SKILL_MARTIAL_ARTS_ATK_BONUS * skill;
          damageBonus += SKILL_MARTIAL_ARTS_DMG_BONUS * skill;
        }
      }

      // alpha10: add tool damage bonus vs map objects
      if (GetEquippedWeapon() is ItemMeleeWeapon melee) damageBonus += melee.Model.ToolBashDamageBonus;

      float hit = (float)baseAttack.HitValue + (float) hitBonus;
      if (IsExhausted) hit /= 2f;
      else if (IsSleepy) hit *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) hit, baseAttack.DamageValue + damageBonus, baseAttack.StaminaPenalty);
    }

    public Attack UnarmedMeleeAttack(Actor? target=null)
    {
      var skills = MySkills;
      int num3 = SKILL_AGILE_ATK_BONUS * skills.GetSkillLevel(Skills.IDs.AGILE) + SKILL_ZAGILE_ATK_BONUS * skills.GetSkillLevel(Skills.IDs.Z_AGILE);
      int num4 = SKILL_STRONG_DMG_BONUS * skills.GetSkillLevel(Skills.IDs.STRONG) + SKILL_ZSTRONG_DMG_BONUS * skills.GetSkillLevel(Skills.IDs.Z_STRONG);
      {
      int skill = skills.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
      if (0 != skill) {
        num3 += SKILL_MARTIAL_ARTS_ATK_BONUS * skill;
        num4 += SKILL_MARTIAL_ARTS_DMG_BONUS * skill;
      }
      }
      if (target?.Model.Abilities.IsUndead ?? false) num4 += DamageBonusVsUndeads;
      Attack baseAttack = Model.UnarmedAttack;
      float num5 = (float)baseAttack.HitValue + (float) num3;
      if (IsExhausted) num5 /= 2f;
      else if (IsSleepy) num5 *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) num5, baseAttack.DamageValue + num4, baseAttack.StaminaPenalty);
    }

    public ItemMeleeWeapon? GetBestMeleeWeapon()
    {
      var tmp = m_Inventory?.GetItemsByType<ItemMeleeWeapon>();
      if (null == tmp) return null;
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

    public ItemMeleeWeapon? GetBestMeleeWeapon(MapObject toBreak)    // XXX don't actually use this parameter, just controls overload
    {
      var tmp = m_Inventory?.GetItemsByType<ItemMeleeWeapon>();
      if (null == tmp) return null;
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
      var tmp = m_Inventory?.GetItemsByType<ItemMeleeWeapon>();
      if (null == tmp) return null;
      int martial_arts_rating = UnarmedMeleeAttack().Rating;
      int? ret = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        int num2 = MeleeWeaponAttack(obj.Model).Rating;
        if (num2 <= martial_arts_rating) continue;
        if (null != ret && num2 <= ret.Value) continue;
        ret = num2;
      }
      return ret;
    }

    // ultimately these two will be thin wrappers, as CurrentMeleeAttack/CurrentRangedAttack are themselves mathematical functions
    // of the equipped weapon which OrderableAI *will* want to vary when choosing an appropriate weapon
    public Attack MeleeAttack(Actor? target = null) {
      if (GetEquippedWeapon() is ItemMeleeWeapon tmp_melee) return MeleeWeaponAttack(tmp_melee.Model, target);
      return UnarmedMeleeAttack(target);
    }

    public Attack MeleeAttack(MapObject target) {
      if (GetEquippedWeapon() is ItemMeleeWeapon tmp_melee) return MeleeWeaponAttack(tmp_melee.Model, target);
      return UnarmedMeleeAttack();
    }

    public Attack BestMeleeAttack(Actor? target = null)
    {
      var tmp_melee = GetBestMeleeWeapon();
      if (null!=tmp_melee) return MeleeWeaponAttack(tmp_melee.Model, target);
      return UnarmedMeleeAttack(target);
    }

    public Attack HypotheticalRangedAttack(Attack baseAttack, int distance, Actor? target = null)
    {
      int hitMod = 0;
      int dmgBonus = 0;
      switch (baseAttack.Kind) {
        case AttackKind.FIREARM:
          {
          int skill = MySkills.GetSkillLevel(Skills.IDs.FIREARMS);
          if (0 != skill) {
            hitMod = SKILL_FIREARMS_ATK_BONUS * skill;
            dmgBonus = SKILL_FIREARMS_DMG_BONUS * skill;
          }
          }
          break;
        case AttackKind.BOW:
          {
          int skill = MySkills.GetSkillLevel(Skills.IDs.BOWS);
          if (0 != skill) {
            hitMod = SKILL_BOWS_ATK_BONUS * skill;
            dmgBonus = SKILL_BOWS_DMG_BONUS * skill;
          }
          }
          break;
      }
      if (target?.Model.Abilities.IsUndead ?? false) dmgBonus += DamageBonusVsUndeads;

      int efficientRange = baseAttack.EfficientRange;
      // alpha10 distance as % modifier instead of flat bonus
      float distanceMod = 1;
      if (distance != efficientRange) {
        float distanceScale = (efficientRange - distance) / (float)baseAttack.Range;
        // bigger effect (penalty) beyond efficient range
        if (distance > efficientRange) {
//        distanceScale *= 2;   0% chance to hit at maximum range, but GUI is in-range
          distanceScale = (efficientRange - distance) / (float)((baseAttack.Range-efficientRange)+1);
        }
        distanceMod = 1 + distanceScale;
      }
      float hit = (baseAttack.HitValue + hitMod) * distanceMod; // XXX natural vector data structure, but this is a hot path so may want this unrolled
      float rapidHit1 = (baseAttack.Hit2Value + hitMod) * distanceMod;
      float rapidHit2 = (baseAttack.Hit3Value + hitMod) * distanceMod;

      const float FIRING_WHEN_SLP_EXHAUSTED = 0.50f; // -50%
      const float FIRING_WHEN_SLP_SLEEPY = 0.75f; // -25%
      const float FIRING_WHEN_STA_TIRED = 0.75f; // -25%
      const float FIRING_WHEN_STA_NOT_FULL = 0.9f; // -10%

      // sleep penalty.
      if (IsExhausted) {
        hit *= FIRING_WHEN_SLP_EXHAUSTED;
        rapidHit1 *= FIRING_WHEN_SLP_EXHAUSTED;
        rapidHit2 *= FIRING_WHEN_SLP_EXHAUSTED;
      } else if (IsSleepy) {
        hit *= FIRING_WHEN_SLP_SLEEPY;
        rapidHit1 *= FIRING_WHEN_SLP_SLEEPY;
        rapidHit2 *= FIRING_WHEN_SLP_SLEEPY;
      }

      // stamina penalty.
      if (IsTired) {
        hit *= FIRING_WHEN_STA_TIRED;
        rapidHit1 *= FIRING_WHEN_STA_TIRED;
        rapidHit2 *= FIRING_WHEN_STA_TIRED;
      } else if (StaminaPoints < MaxSTA) {
        hit *= FIRING_WHEN_STA_NOT_FULL;
        rapidHit1 *= FIRING_WHEN_STA_NOT_FULL;
        rapidHit2 *= FIRING_WHEN_STA_NOT_FULL;
      }

      if (IsExhausted) hit /= 2f;
      else if (IsSleepy) hit *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) hit, baseAttack.DamageValue + dmgBonus, baseAttack.StaminaPenalty, baseAttack.Range, (int)rapidHit1, (int)rapidHit2);
    }

    public Attack RangedAttack(int distance, Actor? target = null)
    {
      return HypotheticalRangedAttack(m_CurrentRangedAttack, distance, target);
    }

#region Tracker management
    public void Tracks(ref Span<bool> find_us)
    {
        find_us[(int)ItemTrackerModel.TrackingOffset.POLICE_FACTION] = IsFaction(GameFactions.IDs.ThePolice);
        if (GetEquippedItem(DollPart.LEFT_HAND) is ItemTracker track && !track.IsUseless) track.Model.Tracks(ref find_us);

        // the police radio
        track = GetEquippedItem(DollPart.HIP_HOLSTER) as ItemTracker;
        if (null != track && !track.IsUseless) track.Model.Tracks(ref find_us);
        // Tracking leaders makes sense, but not in single PC mode
        if (0 >= CountFollowers && null != LiveLeader) find_us[(int)ItemTrackerModel.TrackingOffset.FOLLOWER_AND_LEADER] = false;
    }

    public bool HasActiveCellPhone {
      get {
        return null != m_Inventory?.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader && !it.IsUseless);
      }
    }

    public bool HasCellPhone {
      get {
        return null != m_Inventory?.GetFirstMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless);
      }
    }

    public bool HasActivePoliceRadio {
      get {
        if (GameFactions.IDs.ThePolice==m_FactionID) return true;
        return null != m_Inventory?.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackPolice);  // charges on walking so won't stay useless
      }
    }

    public bool HasPoliceRadio {
      get {
        if (GameFactions.IDs.ThePolice==m_FactionID) return true;
        return null != m_Inventory?.GetFirstMatching<ItemTracker>(it => it.CanTrackPolice);  // charges on walking so won't stay useless
      }
    }

    // For now, entirely implicit.  It's also CHAR technology so recharges like a police radio.
    public bool HasActiveArmyRadio {
      get {
        if (GameFactions.IDs.TheArmy==m_FactionID) return true;
        return false;
      }
    }

    public bool HasArmyRadio {
      get {
        if (GameFactions.IDs.TheArmy==m_FactionID) return true;
        return false;
      }
    }

    public bool NeedActivePoliceRadio {
      get {
        if (GameFactions.IDs.ThePolice==m_FactionID) return false; // implicit
        // XXX disallow murderers under certain conditions, etc
        var leader = LiveLeader;
        if (null != leader) return leader.HasActivePoliceRadio; // XXX \todo change target: deep chain of command
        if (null != m_Followers) foreach(Actor fo in m_Followers) if (fo.HasPoliceRadio) return true;
        return false;
      }
    }

    public bool NeedActiveCellPhone {
      get {
        if (!WantCellPhone) return false;
        var leader = LiveLeader;
        if (null != leader) return leader.HasActiveCellPhone; // XXX \todo change target: deep chain of command
        if (null != m_Followers) foreach(Actor fo in m_Followers) if (fo.HasCellPhone) return true;
        return false;
      }
    }

    public bool WantPoliceRadio {
      get {
        if (GameFactions.IDs.ThePolice == m_FactionID) return false; // police have implicit police radios
        bool have_cellphone = HasCellPhone;
        bool have_army = HasArmyRadio;
        if (!have_cellphone && !have_army) return true;

        bool out_of_comm(Actor a) {
          if (have_cellphone && a.HasCellPhone) return false;
          if (have_army && a.HasArmyRadio) return false;
          return true;
        }

        var leader = LiveLeader;
        if (null != leader) return out_of_comm(leader); // XXX \todo change target: deep chain of command
        if (null != m_Followers) foreach(Actor fo in m_Followers) if (out_of_comm(fo)) return true;
        return false;
      }
    }

    public bool WantCellPhone {
      get {
        bool have_police = HasPoliceRadio;
        bool have_army = HasArmyRadio;
        if (!have_police && !have_army) return true;

        bool out_of_comm(Actor a) {
          if (have_police && a.HasPoliceRadio) return false;
          if (have_army && a.HasArmyRadio) return false;
          return true;
        }

        var leader = LiveLeader;
        if (null != leader) return out_of_comm(leader); // XXX \todo change target: deep chain of command
        if (null != m_Followers) foreach(Actor fo in m_Followers) if (out_of_comm(fo)) return true;
        return false;
      }
    }

    public ItemTracker? GetEquippedCellPhone()
    {
      return m_Inventory?.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader);
    }
#endregion

#if PROTOTYPE
    public List<Actor>? grepAllInRadioRange(Func<Actor, bool> ok, Location? origin = null)
    {
      bool police_radio = HasActivePoliceRadio;
      bool army_radio = HasActiveArmyRadio;
      if (!police_radio && !army_radio) return null;
#if DEBUG
      if (police_radio && army_radio) throw new InvalidOperationException("need to implement dual police and army radio case");
#endif
      if (null == origin) origin = Location;
      var radio_location = Rules.PoliceRadioLocation(origin.Value);
      var radio_range = radio_location.RadioDistricts;

      List<Actor> ret = new();

      Session.Get.World.DoForAllMaps(map => {
        foreach (Actor actor in map.Actors.ToList()) {   // subject to multi-threading race
          if (this == actor) continue;
          // XXX defer implementing dual radios
          if (police_radio) {
            if (!actor.HasActivePoliceRadio) continue;
          } else {
            if (!actor.HasActiveArmyRadio) continue;
          }
          var dest_radio_location = Rules.PoliceRadioLocation(actor.Location);
          if (RogueGame.POLICE_RADIO_RANGE < Rules.GridDistance(radio_location, in dest_radio_location)) continue;

          if (ok(actor)) ret.Add(actor);
        }
        },d => radio_range.Contains(d.WorldPosition));

      return 0<ret.Count ? ret : null;
    }
#endif

    public void MessageAllInDistrictByRadio(Action<Actor> op, Func<Actor, bool> test, Action<PlayerController> msg_player, Action<PlayerController> defer_msg_player, Func<Actor, bool> msg_player_test, Location? origin=null)
    {
      bool player_initiated = RogueGame.IsPlayer(this);
      bool simulating = RogueGame.IsSimulating;
      bool police_radio = HasActivePoliceRadio;
      bool army_radio = HasActiveArmyRadio;
      if (!police_radio && !army_radio) return;
#if DEBUG
      if (police_radio && army_radio) throw new InvalidOperationException("need to implement dual police and army radio case");
#endif
      if (null == origin) origin = Location;

      var radio_location = Rules.PoliceRadioLocation(origin.Value);
      var radio_range = radio_location.RadioDistricts;
      // \todo ultimately, we'd like to prescreen whether the current player is one of the radio targets and pre-empt viewport panning if so
      World.Get.DoForAllMaps(map => {
        foreach (Actor actor in map.Actors.ToList()) {   // subject to multi-threading race
          if (this == actor) continue;
          // XXX defer implementing dual radios
          if (police_radio) {
            if (!actor.HasActivePoliceRadio) continue;
          } else {
            if (!actor.HasActiveArmyRadio) continue;
          }
          if (actor.IsDead) continue;
          if (actor.IsSleeping) continue;   // can't hear when sleeping (this is debatable; might be interesting to be woken up by high-priority messages once radio alarms are implemented)
          var dest_radio_location = Rules.PoliceRadioLocation(actor.Location);
          if (RogueGame.POLICE_RADIO_RANGE < Rules.GridDistance(radio_location, in dest_radio_location)) continue;

          // note: UI redraw will fail if IsSimulating; should be deferring message in that case
          if (actor.Controller is PlayerController pc) {
            if (msg_player_test(actor)) {
              // IsSimulating: defer
              // actor is RogueGame::Player: maybe don't need this at all, maybe defer (option?)
              // otherwise: pan and live display
              if (player_initiated || simulating) defer_msg_player(pc);
              else {
                RogueGame.Game.PanViewportTo(actor);
                player_initiated = true; // not really, but this prevents distracting re-pans for background police radio
                msg_player(pc);
              }
            }
          }

          // use cases.
          // aggressing all faction in district: civilian/survivor cannot initiate, and have no obligation to respond if they get the message
          // reporting z: civilian can initiate and respond (but threat tracking needed to respond)
          if (test(actor)) op(actor);
        }
        },d => radio_range.Contains(d.WorldPosition));
    }

    public Actor? Sees(Actor? a)
    {
      if (null == a) return null;
      if (this == a) return null;
      if (a.IsDead) return null;
      return (Controller.IsVisibleTo(a) ? a : null);  // inline IsVisibleToPlayer here, for generality
    }

    // leadership/follower handling
    public void AddFollower(Actor other)
    {
      if (null == m_Followers) m_Followers = new List<Actor>(1);
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

    public void RemoveFollower(Actor other)
    {
      var followers = m_Followers;
      if (null == followers) throw new InvalidOperationException(Name+": no followers");
      other._NoLongerFollower();
      followers.Remove(other);
//    other._NoLongerFollower(); // multi-threaded race condition when here?
      if (0 == followers.Count) Interlocked.Exchange(ref m_Followers, null);
    }

    public void RemoveAllFollowers()
    {
#if OBSOLETE
      while (m_Followers != null && m_Followers.Count > 0)
        RemoveFollower(m_Followers[0]);
#endif
      var followers = m_Followers;
      if (null != followers) {
        foreach(var fo in followers) fo._NoLongerFollower();
        Interlocked.Exchange(ref m_Followers, null);
      }
    }

    public void SetTrustIn(Actor other, int trust) => (m_TrustDict ??= new())[other] = trust;

    public int GetTrustIn(Actor other)
    {
      if (null == m_TrustDict) return 0;
      if (m_TrustDict.TryGetValue(other,out int trust)) return trust;
      return 0;
    }

    public int TrustIncrease {
      get {
        return 1 + SKILL_CHARISMATIC_TRUST_BONUS * MySkills.GetSkillLevel(Skills.IDs.CHARISMATIC);
      }
    }

    public ThreatTracking? Threats {
      get {
        if (IsFaction(GameFactions.IDs.ThePolice)) return Session.Get.Police.Threats;
        return null;
      }
    }

    public LocationSet? InterestingLocs {
      get {
        if (IsFaction(GameFactions.IDs.ThePolice)) return Session.Get.Police.Investigate;
        return null;
      }
    }

    public IEnumerable<Actor>? Aggressing { get { return m_AggressorOf; } }
    public IEnumerable<Actor>? Aggressors { get { return m_SelfDefenceFrom; } }

    // these two are tightly integrated.
    private void MarkAsAggressorOf(Actor other)
    {
      if (m_AggressorOf == null) m_AggressorOf = new List<Actor>{ other };
      else if (m_AggressorOf.Contains(other)) return;
      else m_AggressorOf.Add(other);
      Threats?.RecordTaint(other, other.Location);
    }

    private void MarkAsSelfDefenceFrom(Actor other)
    {
      if (m_SelfDefenceFrom == null) m_SelfDefenceFrom = new List<Actor>{ other };
      else if (m_SelfDefenceFrom.Contains(other)) return;
      else m_SelfDefenceFrom.Add(other);
      Threats?.RecordTaint(other, other.Location);
    }

    public void Aggress(Actor defender) {
      Activity = Activity.FIGHTING;
      m_TargetActor = defender;
      if (!IsEnemyOf(defender)) RogueGame.Game.DoMakeAggression(this, defender);
    }

    // No UI updates here (that differs by context)
    public void RecordAggression(Actor other)
    {
#if DEBUG
       if (other.IsDead) throw new ArgumentNullException(nameof(other));
       if (IsDead) throw new ArgumentNullException("this.IsDead");
#endif
       MarkAsAggressorOf(other);
       other.MarkAsSelfDefenceFrom(this);
    }

    public bool IsAggressorOf(Actor other)
    {
      return m_AggressorOf?.Contains(other) ?? false;
    }

    public bool IsSelfDefenceFrom(Actor other)
    {
      return m_SelfDefenceFrom?.Contains(other) ?? false;
    }

    private void RemoveAggressorOf(Actor other)
    {
      if (m_AggressorOf == null) return;
      m_AggressorOf.Remove(other);
      if (0 >= m_AggressorOf.Count) m_AggressorOf = null;
    }

    private void RemoveSelfDefenceFrom(Actor other)
    {
      if (m_SelfDefenceFrom == null) return;
      m_SelfDefenceFrom.Remove(other);
      if (0 >= m_SelfDefenceFrom.Count) m_SelfDefenceFrom = null;
    }

    public void RemoveAllAgressorSelfDefenceRelations()
    {
      if (null != m_AggressorOf) {
        foreach(Actor other in m_AggressorOf) other.RemoveSelfDefenceFrom(this);
        m_AggressorOf = null;
      }
      if (null != m_SelfDefenceFrom) {
        foreach(Actor other in m_SelfDefenceFrom) other.RemoveAggressorOf(this);
        m_SelfDefenceFrom = null;
      }
    }

    public bool IsEnemyOf(Actor target, bool checkGroups = true)    // extra parameter from RS Alpha 10
    {
      if (Faction.IsEnemyOf(target.Faction)) return true;
      if (IsFaction(target) && IsInAGang && target.IsInAGang && GangID != target.GangID) return true;
      if (ArePersonalEnemies(target)) return true;
      return checkGroups && AreIndirectEnemies(target);
    }

    private bool ArePersonalEnemies(Actor other) // RS alpha 10 had better name
    {
      if (other.IsDead) return false;
      // following *should* be symmetric
      return (m_AggressorOf?.Contains(other) ?? false) || (m_SelfDefenceFrom?.Contains(other) ?? false) || other.IsAggressorOf(this) || other.IsSelfDefenceFrom(this);
    }
#nullable restore

    public bool AreIndirectEnemies(Actor other)
    {
      if (other?.IsDead ?? true) return false;

      // my followers enemies are my enemies
      static bool IsEnemyOfMyFollowers(Actor groupActor, Actor target)
      {
        if (null == groupActor.m_Followers) return false;
        foreach (Actor follower in groupActor.m_Followers)
          if (follower.IsEnemyOf(target, false)) return true;
        return false;
      }

      // my leader enemies are my enemies.
      // my mates enemies are my enemies.
      static bool IsEnemyOfMyLeaderOrMates(Actor groupActor, Actor target)
      {
        var leader = groupActor.LiveLeader;
        if (null == leader) return false;
        if (leader.IsEnemyOf(target, false)) return true;
        return IsEnemyOfMyFollowers(leader, target);
      }

      if (IsEnemyOfMyLeaderOrMates(this, other)) return true;
      if (IsEnemyOfMyFollowers(this,other)) return true;
      if (IsEnemyOfMyLeaderOrMates(other, this)) return true;
      if (IsEnemyOfMyFollowers(other,this)) return true;
      return false;
    }

#nullable enable
    // not just our FoV.
    public List<Actor>? GetEnemiesInFov(Location[] fov)
    {
      if (1 >= fov.Length) return null;  // sleeping?
      var actorList = new List<Actor>(fov.Length-1); // assuming ok to thrash GC
      foreach (var loc in fov) {
        var actorAt = loc.Actor;
        if (actorAt != null && actorAt != this && IsEnemyOf(actorAt)) {
          actorList.Add(actorAt);
        }
      }
      return actorList.SortIncreasing(x => Rules.InteractionStdDistance(x.Location, m_Location));
    }

    // stripped down from above
    public bool AnyEnemiesInFov(Location[] fov)
    {
      if (1 >= fov.Length) return false;  // sleeping?
      foreach (var loc in fov) {
        var actorAt = loc.Actor;
        if (actorAt != null && actorAt != this && IsEnemyOf(actorAt)) return true;
      }
      return false;
    }

    // We do not handle the enemy relations here.
    public HashSet<Actor>? Allies {
      get {
        var ret = new HashSet<Actor>();
        // 1) police have all other police as allies.
        if (IsFaction(GameFactions.IDs.ThePolice)) ret = (World.Get.PoliceInRadioRange(Location) ?? ret);
        // 2) leader/follower cliques are allies.
        m_Followers?.AppendTo(ret);
        var leader = LiveLeader;
        if (null != leader) { // 2019-08-14: currently mutually exclusive with above for NPCs
          ret.Add(leader);
          leader.m_Followers?.AppendTo(ret);
          ret.Remove(this);
        }
        return (0<ret.Count ? ret : null);
      }
    }

    // ignores faction alliances
    public HashSet<Actor>? ChainOfCommand {
      get {
        var ret = new HashSet<Actor>();
        m_Followers?.AppendTo(ret);
        var leader = LiveLeader;
        if (null != leader) {
          ret.Add(leader);
          leader.m_Followers?.AppendTo(ret);
          ret.Remove(this);
        }
        return (0<ret.Count ? ret : null);
      }
    }

    // alpha10
    /// <summary>
    /// Is this other actor our leader, a follower or a mate.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsInGroupWith(Actor other)
    {
      var leader = LiveLeader;
      if (null != leader) {
        if (   other == leader   // my leader?
            || other.Leader == leader)    // a mate?
          return true;
      }
      return m_Followers?.Contains(other) ?? false; // a follower?
    }

    public bool IsAlly(Actor other)
    {
      if (IsInGroupWith(other)) return true;
      if (IsFaction(GameFactions.IDs.ThePolice)) return other.IsFaction(GameFactions.IDs.ThePolice);
      return false;
    }

    public List<Actor>? FilterAllies(IEnumerable<Actor>? src, Predicate<Actor>? test = null)
    {
        if (null == src || !src.Any()) return null;
        List<Actor>? ret = null;
        foreach(var a in src) {
          if (!IsAlly(a)) continue;
          if (null==test || test(a)) (ret ??= new List<Actor>(src.Count())).Add(a);
        }
        return ret;
    }
#nullable restore

    // map-related, loosely
    public void RemoveFromMap()
    {
      Location.Map?.Remove(this);   // DuckMan and other uniques start with null map before spawning
    }

#nullable enable
    public bool WouldBeAdjacentToEnemy(Map map,Point p)
    {
      return map.HasAnyAdjacent(p, a => a.IsEnemyOf(this));
    }

    public bool IsAdjacentToEnemy {
      get {
        return WouldBeAdjacentToEnemy(Location.Map,Location.Position);
      }
    }

    public Dictionary<Point,Actor> GetMoveBlockingActors(Point pt)
    {
      var ret = new Dictionary<Point,Actor>();
      if (pt == Location.Position) return ret;
      var a = Location.Map.GetActorAtExt(pt);
      if (null!=a) ret[pt] = a;
      if (!Rules.IsAdjacent(in pt,Location.Position)) return ret;
#if B_MOVIE_MARTIAL_ARTS
      if (0 < UsingPolearmInBMovie) {
        // Polearms actually have range 2 (cf. Dungeon Crawl Stone Soup).
        // this would look much more reasonable at Angband space-time scale of 900 turns per hour, than the historical 30 turns/hour
        foreach(var pt2 in pt.Adjacent()) {
          if (2!=Rules.GridDistance(Location.Position,in pt2)) continue;
          a = Location.Map.GetActorAtExt(pt2);
          if (null==a || !IsEnemyOf(a)) continue;          // Only hostiles may block movement at range 2.
          ret.Add(pt2, a);
        }
      }
#endif
      return ret;
    }

    public bool IsBefore(Actor other)
    {
      if (Location.Map != other.Location.Map) return District.IsBefore(Location.Map, other.Location.Map);
      foreach (Actor actor1 in Location.Map.Actors) {
        if (actor1 == this) return true;
        if (actor1 == other) return false;
      }
      return true;  // probable error condition
    }

    public bool IsOnCouch {
      get {
        return Location.Map.GetMapObjectAt(Location.Position)?.IsCouch ?? false;
      }
    }

    public bool IsInside {
      get {
        return Location.Map.IsInsideAt(Location.Position);
      }
    }

    public void TargetedActivity(Activity what, Actor whom)
    {
#if IRRATIONAL_CAUTION
      if (Activity.CHASING != what && Activity.FIGHTING != what && Activity.FOLLOWING != what) throw new InvalidOperationException("untargetable activity");
#endif
      Activity = what;
      m_TargetActor = whom;
    }

    private List<Point>? FastestStepTo(Map m,Point src,Point dest)
    {
      int dist = Rules.GridDistance(in src,in dest);
      if (1==dist) return new List<Point>{ dest };
      IEnumerable<Point> tmp = src.Adjacent().Where(pt=> dist>Rules.GridDistance(in pt,in dest) && m.IsWalkableFor(pt,this));
      return tmp.Any() ? tmp.ToList() : null;
    }

    /// <param name="target">may be denormalized to guarantee origin.Map==target.Map</param>
    private List<List<Point>>? _MinStepPathTo(in Location origin, in Location target)
    {
      Map m = origin.Map;
      Point src = origin.Position;
      Point dest = target.Position;
      var tmp = FastestStepTo(m,src,dest);
      if (null == tmp) return null;
      var ret = new List<List<Point>> { tmp };
      while(!tmp.Contains(dest)) {
        int dist = Rules.GridDistance(in dest,tmp[0]);
        if (1==dist) {
          tmp = new List<Point>{dest};
        } else {
          HashSet<Point> tmp2 = new HashSet<Point>();
          foreach(Point pt in tmp) {
            var tmp3 = FastestStepTo(m,pt,dest);
            if (null != tmp3) tmp2.UnionWith(tmp3);
          }
          if (0 >= tmp2.Count) return null;
          tmp = tmp2.ToList();
        }
        ret.Add(tmp);
      }
      return ret;
    }

    public List<List<Point>>? MinStepPathTo(in Location origin, in Location target)
    {
      if (origin==target) return null;

      if (origin.Map!=target.Map) {
        Location? test = origin.Map.Denormalize(in target);
        if (null == test) return null;
        return _MinStepPathTo(in origin, test.Value);
      }
      return _MinStepPathTo(in origin, in target);
    }

    public List<Location>? OneStepRange(Location loc)
    {
      var no_path = (Controller as Gameplay.AI.ObjectiveAI)?.Goal<Engine.Goal.DeathTrapped>();
      var ret = new List<Location>();
      foreach(Direction dir in Direction.COMPASS) {
        Location test = loc+dir;
        if (!test.Map.IsWalkableFor(test.Position, this)) continue;
#if MAPGEN_OK
        if (!Map.Canonical(ref test)) throw new InvalidProgramException("exit leading out of world");    // problem is in map generation: RogueGame::GenerateWorld
#else
        if (!Map.Canonical(ref test)) continue;
#endif
        ret.Add(test);
      }
      var exit = Model.Abilities.AI_CanUseAIExits ? loc.Exit : null;
      // deathtrapped revision intentionally omitted: not used in pathing
      if (null != exit) {
        var tmp = new ActionUseExit(this, in loc);
        if (loc == Location) {
          if (tmp.IsPerformable()) ret.Add(exit.Location);
        } else {
          ret.Add(exit.Location);
          // simulate Exit::ReasonIsBlocked
          switch(exit.Location.IsBlockedForPathing) {
          case 0: break;
          case 1: if (!CanJump) ret.Remove(exit.Location);
            break;
          default: ret.Remove(exit.Location);
            break;
          }
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public List<Point>? OneStepRange(Map m,Point p)
    {
      IEnumerable<Point> tmp = p.Adjacent().Where(pt=>m.IsWalkableFor(pt,this));
      return tmp.Any() ? tmp.ToList() : null;
    }

    public Dictionary<Location, ActionMoveDelta>? MovesTo(in Location dest) {
      var ret = new Dictionary<Location, ActionMoveDelta>();
      if (CanEnter(dest)) {
        foreach(var pt in dest.Position.Adjacent()) {
          Location src = new Location(dest.Map, pt);
          if (!CanEnter(ref src)) continue;
          ret[src] = new ActionMoveDelta(this, dest, src);
        }
        var e = Model.Abilities.AI_CanUseAIExits ? dest.Exit : null;
        if (null != e && e.ToMap.DistrictPos == dest.Map.DistrictPos && CanEnter(e.Location)) {
          // we are assuming this is a two-way exit.  \todo fix when introducing rooftops, e.g. helipads
          ret[e.Location] = new ActionMoveDelta(this, dest, e.Location);
        }
      }
      // tentatively not handling generators, etc. here
      return 0 < ret.Count ? ret : null;
    }

#if PROTOTYPE
    public Dictionary<Location, ActionMoveDelta>? MovesFrom(in Location src) {
      var ret = new Dictionary<Location, ActionMoveDelta>();
      if (CanEnter(src)) {
        foreach(var pt in src.Position.Adjacent()) {
          Location dest = new Location(src.Map, pt);
          if (!CanEnter(ref dest)) continue;
          ret[dest] = new ActionMoveDelta(this, dest, src);
        }
        var e = Model.Abilities.AI_CanUseAIExits ? src.Exit : null;
        if (null != e && e.ToMap.DistrictPos == src.Map.DistrictPos && CanEnter(e.Location)) {
          // this would work with a one-way exit.  \todo review when introducing rooftops, e.g. helipads
          ret[e.Location] = new ActionMoveDelta(this, e.Location, src);
        }
      }
      // tentatively not handling generators, etc. here
      return 0 < ret.Count ? ret : null;
    }

    public List<Location>? ReachableFrom(in Location src) {
      var ret = new List<Location>();
      if (CanEnter(src)) {
        foreach(var pt in src.Position.Adjacent()) {
          Location dest = new Location(src.Map, pt);
          if (!CanEnter(ref dest)) continue;
          ret.Add(dest);
        }
        var e = Model.Abilities.AI_CanUseAIExits ? src.Exit : null;
        if (null != e && e.ToMap.DistrictPos == src.Map.DistrictPos && CanEnter(e.Location)) {
          // this would work with a one-way exit.  \todo review when introducing rooftops, e.g. helipads
          ret.Add(e.Location);
        }
      }
      // tentatively not handling generators, etc. here
      return 0 < ret.Count ? ret : null;
    }
#endif

    public Dictionary<Location,ActorAction>? OnePathRange(in Location loc)
    {
      var ret = new Dictionary<Location,ActorAction>();
      foreach(Direction dir in Direction.COMPASS) {
        Location test = loc+dir;
        ActorAction tmp = Rules.IsPathableFor(this, in test);
        if (null == tmp) continue;
#if MAPGEN_OK
        if (!Map.Canonical(ref test)) throw new InvalidProgramException("exit leading out of world");    // problem is in map generation: RogueGame::GenerateWorld
#else
        if (!Map.Canonical(ref test)) continue;
#endif
        ret.Add(test, tmp);
      }
      var exit = Model.Abilities.AI_CanUseAIExits ? loc.Exit : null;
      if (null != exit) { // the pathable check also handles death-trapping, but exits don't check for that
        var no_path = (Controller as Gameplay.AI.ObjectiveAI)?.Goal<Engine.Goal.DeathTrapped>();
        if (null != no_path && no_path.IsBanned(exit.Location)) exit = null;
      }
      if (null != exit) {
        var tmp = new ActionUseExit(this, in loc);
        if (loc == Location) {
          if (tmp.IsPerformable()) ret.Add(exit.Location, tmp);
        } else {
          ret.Add(exit.Location, tmp);
          // simulate Exit::ReasonIsBlocked
          switch(exit.Location.IsBlockedForPathing) {
          case 0: break;
          case 1: if (!CanJump) ret.Remove(exit.Location);
            break;
          default: ret.Remove(exit.Location);
            break;
          }
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public Dictionary<Location,ActorAction> OnePath(in Location loc, Dictionary<Location, ActorAction> already)
    {
      var ret = new Dictionary<Location, ActorAction>(9);
      foreach(Direction dir in Direction.COMPASS) {
        Location dest = loc+dir;
        if (!Map.Canonical(ref dest)) continue;
        if (already.TryGetValue(dest, out var relay)) {
          ret.Add(dest, relay);
          continue;
        }
        if (Location==dest) {
          ret.Add(dest, new ActionMoveStep(this, dest.Position));
          continue;
        }
        // 2020-03-19 This is multi-threading sensitive -- the Sokoban preventer may have to reach across threads
        // generators fail CanEnter but can be pathable so that would have to be accounted for
        if (null != (relay = Rules.IsPathableFor(this, in dest) ?? (CanEnter(dest) ? new ActionMoveDelta(this, in dest, in loc) : null))) ret.Add(dest, relay);
      }
      var exit = Model.Abilities.AI_CanUseAIExits ? loc.Exit : null;
      if (null != exit) { // the pathable check also handles death-trapping, but exits don't check for that
        var no_path = (Controller as Gameplay.AI.ObjectiveAI)?.Goal<Engine.Goal.DeathTrapped>();
        if (null != no_path && no_path.IsBanned(exit.Location)) exit = null;
      }
      if (null != exit) {
        var tmp = new ActionUseExit(this, in loc);
        if (loc == Location) {
          if (tmp.IsPerformable()) ret.Add(exit.Location, tmp);
          else {
            var obj = exit.Location.MapObject;
            if (null != obj && CanBreak(obj)) {
              ret.Add(exit.Location, new ActionBreak(this, obj));
            }
          }
        } else {
          ret.Add(exit.Location, new ActionUseExit(this, in loc));
          // simulate Exit::ReasonIsBlocked
          switch(exit.Location.IsBlockedForPathing) {
          case 0: break;
          case 1: if (!CanJump) ret.Remove(exit.Location);
            break;
          default: ret.Remove(exit.Location);
            break;
          }
        }
      }
      return ret;
    }

    public Dictionary<Location,ActorAction> OnePath(Location loc)   // adapter
    {
      var already = new Dictionary<Location, ActorAction>();
      return OnePath(in loc,already);
    }

    public Dictionary<Point,ActorAction> OnePath(Map m, in Point p, Dictionary<Point, ActorAction> already)
    {
      var ret = new Dictionary<Point, ActorAction>(9);
      foreach(var pt in p.Adjacent()) {
        if (already.TryGetValue(pt, out var relay)) {
          ret[pt] = relay;
          continue;
        }
        Location dest = new Location(m,pt);
        if (!Map.Canonical(ref dest)) continue;
        if (Location==dest) {
          ret[pt] = new ActionMoveStep(this, in pt);
          continue;
        }
        ActorAction tmp = Rules.IsPathableFor(this, in dest);
        if (null == tmp && CanEnter(dest)) tmp = new ActionMoveDelta(this, in dest, new Location(m,p));
        if (null != tmp) ret[pt] = tmp;
      }
      return ret;
    }

    public Dictionary<Point,ActorAction> OnePathPt(Location loc)   // adapter
    {
      var already = new Dictionary<Point, ActorAction>();
      return OnePath(loc.Map,loc.Position,already);
    }

    public List<Point>? LegalSteps { get { return OneStepRange(Location.Map, Location.Position);  } }

    public HashSet<Point>? NextStepRange(Map m,HashSet<Point> past, IEnumerable<Point> now)
    {
#if DEBUG
      if (!now.Any()) throw new InvalidOperationException("!now.Any() : do not step into nowhere");
#endif
      var ret = new HashSet<Point>();
      foreach(Point pt in now) {
        var tmp = OneStepRange(m,pt);
        if (null == tmp) continue;
        var tmp2 = new HashSet<Point>(tmp);
        tmp2.ExceptWith(past);
        tmp2.ExceptWith(now);
        ret.UnionWith(tmp2);
      }
      return (0<ret.Count ? ret : null);
    }

    private string ReasonCantBreak(MapObject mapObj)
    {
      if (!Model.Abilities.CanBreakObjects) return "cannot break objects";
      if (IsTired) return "tired";
      if (mapObj.BreakState != MapObject.Break.BREAKABLE && !((mapObj as DoorWindow)?.IsBarricaded ?? false)) return "can't break this object";
      if (mapObj.Location.StrictHasActorAt) return "someone is there";
      return "";
    }

    public bool CanBreak(MapObject mapObj, out string reason)
    {
      reason = ReasonCantBreak(mapObj);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBreak(MapObject mapObj) => string.IsNullOrEmpty(ReasonCantBreak(mapObj));

    public bool AbleToPush {
      get {
        if (Model.Abilities.CanPush) return true;
        var skills = MySkills;
        return 0 < skills.GetSkillLevel(Skills.IDs.STRONG) || 0 < skills.GetSkillLevel(Skills.IDs.Z_STRONG);
      }
    }

    private string ReasonCantPush(MapObject mapObj)
    {
      if (!AbleToPush) return "cannot push objects";
      if (IsTired) return "tired";
      if (!mapObj.IsMovable) return "cannot be moved";
      if (mapObj.Location.StrictHasActorAt) return "someone is there";
      if (mapObj.IsOnFire) return "on fire";
      if (null != m_DraggedCorpse) return "dragging a corpse";
      var code = District.UsesCrossDistrictView(Location.Map);
      if (0 >= code) {
        if (Location.Map != mapObj.Location.Map) return "hard to push vertically";
      } else if (code != District.UsesCrossDistrictView(mapObj.Location.Map)) return "hard to push vertically";
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

    // currently other is unused, but that is not an invariant
    private string ReasonCantShove(Actor other)
    {
      if (!AbleToPush) return "cannot shove people";
      if (IsTired) return "tired";
      if (null != m_DraggedCorpse) return "dragging a corpse";
      return "";
    }

    public bool CanShove(Actor other, out string reason)
    {
      reason = ReasonCantShove(other);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanShove(Actor other)
    {
      return string.IsNullOrEmpty(ReasonCantShove(other));
    }

    private string ReasonCantBeShovedTo(Location to)
    {
      if (!Map.Canonical(ref to)) return "out of map";
      if (!to.TileModel.IsWalkable) return "blocked";
      if (!to.MapObject?.IsWalkable ?? false) return "blocked by an object";
      if (null != to.Actor) return "blocked by someone";
      return "";
    }

    public bool CanBeShovedTo(Point toPos, out string reason) => string.IsNullOrEmpty(reason = ReasonCantBeShovedTo(new Location(Location.Map, toPos)));
    public bool CanBeShovedTo(Point toPos) => string.IsNullOrEmpty(ReasonCantBeShovedTo(new Location(Location.Map, toPos)));
    /// <param name="to">Assumed to be in canonical form (in bounds)</param>
    public bool CanBeShovedTo(in Location to, out string reason) { return string.IsNullOrEmpty(reason = ReasonCantBeShovedTo(to)); }
    /// <param name="to">Assumed to be in canonical form (in bounds)</param>
    public bool CanBeShovedTo(in Location to) => string.IsNullOrEmpty(ReasonCantBeShovedTo(to));

    public Dictionary<Location, Direction> ShoveDestinations {
      get {
         return Map.ValidDirections(in m_Location, loc => CanBeShovedTo(in loc));
      }
    }

    // alpha10: pull support
    private string ReasonCantPull(MapObject mapObj, in Location moveTo)
    {
      string ret = ReasonCantPush(mapObj);
      if (!string.IsNullOrEmpty(ret)) return ret;

      var other = Location.MapObject;
      if (null != other) return string.Format("{0} is blocking", other.TheName);

      moveTo.Map.IsWalkableFor(moveTo.Position, this, out ret);
      return ret;
    }

    public bool CanPull(MapObject mapObj, Location moveTo)
    {
      return string.IsNullOrEmpty(ReasonCantPull(mapObj, in moveTo));
    }

    public bool CanPull(MapObject mapObj, in Location moveTo, out string reason)
    {
      reason = ReasonCantPull(mapObj, in moveTo);
      return string.IsNullOrEmpty(reason);
    }

    private string ReasonCantPull(Actor other, in Location dest)
    {
      string ret = ReasonCantShove(other);
      if (!string.IsNullOrEmpty(ret)) return ret;

      dest.IsWalkableFor(this, out ret);
      return ret;
    }

    private string ReasonCantPull(Actor other, Point moveToPos)
    {
      string ret = ReasonCantShove(other);
      if (!string.IsNullOrEmpty(ret)) return ret;

      Location.Map.IsWalkableFor(moveToPos, this, out ret);
      return ret;
    }

    public bool CanPull(Actor other, in Point moveToPos, out string reason)
    {
      reason = ReasonCantPull(other, moveToPos);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanPull(Actor other, Point moveToPos)
    {
      return string.IsNullOrEmpty(ReasonCantPull(other, moveToPos));
    }

    public bool CanPull(Actor other, in Location dest, out string reason)
    {
      reason = ReasonCantPull(other, in dest);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanPull(Actor other, in Location dest)
    {
      return string.IsNullOrEmpty(ReasonCantPull(other, in dest));
    }

    // these two are optimized for RogueGame::HandlePlayerPull (fast-fail)
    public string ReasonCantPush(string verb="push")
    {
      if (!AbleToPush) return "Cannot "+verb+" objects.";
      if (IsTired) return "Too tired to " + verb + ".";
      if (null != m_DraggedCorpse) return "Cannot " + verb + ": dragging corpse of " + m_DraggedCorpse.DeadGuy.Name+".";
      return "";
    }

    public string ReasonCantPull()
    {
      string ret = ReasonCantPush("pull");
      if (!string.IsNullOrEmpty(ret)) return ret;

      var other = Location.MapObject;
      if (null != other) return string.Format("Cannot pull: {0} is blocking", other.TheName);

      return ret;
    }
    // alpha10: end pull support

    private string ReasonCantClose(DoorWindow door)
    {
      if (!Model.Abilities.CanUseMapObjects) return "can't use objects";
      if (!door.IsOpen) return "not open";
      if (door.Location.StrictHasActorAt) return "someone is there";
      return "";
    }

    public bool CanClose(DoorWindow door, out string reason)
    {
      reason = ReasonCantClose(door);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanClose(DoorWindow door) => string.IsNullOrEmpty(ReasonCantClose(door));

    public int ScaleBarricadingPoints(int baseBarricadingPoints)
    {
      int barBonus = (int)(/* (double) */ SKILL_CARPENTRY_BARRICADING_BONUS * /* (int) */ (baseBarricadingPoints * MySkills.GetSkillLevel(Skills.IDs.CARPENTRY)));    // carpentry skill

      // alpha10: tool build bonus
      if (GetEquippedWeapon() is ItemMeleeWeapon melee) {
        float toolBonus = melee.Model.ToolBuildBonus;
        if (0 != toolBonus) barBonus += (int)(baseBarricadingPoints * toolBonus);
      }
      return baseBarricadingPoints + barBonus;
    }

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

    // we have a for loop that requires splitting CanBarricade into two halves for efficiency
    private string ReasonCouldntBarricade()
    {
      if (!Model.Abilities.CanBarricade) return "no ability to barricade";
      if (m_Inventory == null || m_Inventory.IsEmpty) return "no items";
      if (!m_Inventory.Has<ItemBarricadeMaterial>()) return "no barricading material";
      return "";
    }

#if DEAD_FUNC
    public bool CouldBarricade(out string reason)
    {
      reason = ReasonCouldntBarricade();
      return string.IsNullOrEmpty(reason);
    }
#endif

    public bool CouldBarricade()
    {
      return string.IsNullOrEmpty(ReasonCouldntBarricade());
    }

    private string ReasonCantBash(DoorWindow door)
	{
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

    public bool CanOpen(DoorWindow door) => string.IsNullOrEmpty(ReasonCantOpen(door));

    public int BarricadingMaterialNeedForFortification(bool isLarge)
    {
      return Math.Max(1, (isLarge ? 4 : 2) - (MySkills.GetSkillLevel(Skills.IDs.CARPENTRY) >= 3 ? SKILL_CARPENTRY_LEVEL3_BUILD_BONUS : 0));
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
      if (!Model.Abilities.CanUseMapObjects) return "cannot use map objects";
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

    // leave the dead parameter in there, for now.
    // E.g., non-CHAR power generators might actually *need fuel*
    private string ReasonCantSwitch(PowerGenerator powGen)
	{
      if (!Model.Abilities.CanUseMapObjects) return "cannot use map objects";
      return "";
	}

    public bool CanSwitch(PowerGenerator powGen, out string reason)
    {
	  reason = ReasonCantSwitch(powGen);
	  return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanSwitch(PowerGenerator powGen)
    {
	  return string.IsNullOrEmpty(ReasonCantSwitch(powGen));
    }
#endif

    // event timing
    public void SpendActionPoints(int actionCost = BASE_ACTION_COST)
    {
      Interlocked.Add(ref m_ActionPoints, -actionCost);
    }

    public void Wait() { m_ActionPoints = 0; }
    public bool CanActThisTurn { get { return 0 < m_ActionPoints; } }

    public bool CanActNextTurn {
      get {
        if (CanActThisTurn) return 0 < m_ActionPoints + Speed - BASE_ACTION_COST;
        return 0 < m_ActionPoints + Speed;
      }
    }

    public bool WillActAgainBefore(Actor other)
    {
      return other.m_ActionPoints <= 0 && (!other.CanActNextTurn || IsBefore(other));
    }

    public bool CannotActNow {
      get { return 0 >= m_ActionPoints || IsSleeping; }
    }

    public int Speed {
      get {
        float num = Model.DollBody.Speed;    // an exhausted, sleepy living dragging a corpse in heavy armor, below 36 here, will have a speed of zero
        if (IsTired) { num *= 2f; num /= 3f; }
        if (IsExhausted) num /= 2f;
        else if (IsSleepy) { num *= 2f; num /= 3f; }
        var armor = GetEquippedArmor();
        if (null != armor) num -= armor.Weight;
        if (null != m_DraggedCorpse) num /= 2f;
        return Math.Max((int) num, 0);
      }
    }

    public float TUorder => (float)(m_ActionPoints) / Speed; // release block: division by zero risk

    // n is the number of our actions
    public int HowManyTimesOtherActs(int n,Actor other)
    {   // n=1:
#if DEBUG
      if (1 > n) throw new InvalidOperationException("not useful to check how many times other can act before this action");
#endif
      int my_ap = m_ActionPoints;
      int my_actions = 0;
      while(0 < my_ap) { // assuming this never gets very large
        my_ap -= BASE_ACTION_COST;
        my_actions++;
      }
      if (my_actions>n) return 0;
      // if in another district, AP is current and does not need prediction
      int other_ap = other.m_ActionPoints + ((Location.Map.DistrictPos != other.Location.Map.DistrictPos || IsBefore(other)) ? 0 : other.Speed);
      int other_actions = 0;
      while(0 < other_ap) { // assuming this never gets very large
        other_ap -= BASE_ACTION_COST;
        other_actions++;
      }
      if (my_actions==n) return other_actions;
      while(my_actions<n) {
        my_ap += Speed;
        while(0 < my_ap) { // assuming this never gets very large
          my_ap -= BASE_ACTION_COST;
          my_actions++;
        }
        if (my_actions>n) break;
        other_ap += other.Speed;
        while(0 < other_ap) { // assuming this never gets very large
          other_ap -= BASE_ACTION_COST;
          other_actions++;
        }
      }
      return other_actions;
    }

    // infection
    public int InfectionHPs { get => MaxHPs + MaxSTA; }

    public void Infect(int i) {
      if (Session.Get.HasInfection) m_Infection = Math.Min(InfectionHPs, m_Infection + i);    // intentional no-op if mode doesn't have infection
    }

    public void Cure(int i) => m_Infection = Math.Max(0, m_Infection - i);
    public int InfectionPercent { get => 100 * m_Infection / InfectionHPs; }

    public int InfectionForDamage(int dmg)
    {
      return dmg + (int) (SKILL_ZINFECTOR_BONUS * /* (int) */ (MySkills.GetSkillLevel(Skills.IDs.Z_INFECTOR) * dmg));
    }

    // health
    public int MaxHPs {
      get {
        var skills = MySkills;
        int num = SKILL_TOUGH_HP_BONUS * skills.GetSkillLevel(Skills.IDs.TOUGH) + SKILL_ZTOUGH_HP_BONUS * skills.GetSkillLevel(Skills.IDs.Z_TOUGH);
        return Model.BaseHitPoints + num;
      }
    }

    public void RegenHitPoints(int hpRegen) => m_HitPoints = Math.Min(MaxHPs, m_HitPoints + hpRegen);

    public int HealChanceBonus {
      get => SKILL_HARDY_HEAL_CHANCE_BONUS * MySkills.GetSkillLevel(Skills.IDs.HARDY);
    }

    public int BiteHpRegen(int dmg) // only for undead, however
    {
      return dmg + (int)(/* (double) */ SKILL_ZEATER_REGEN_BONUS * /* (int) */(MySkills.GetSkillLevel(Skills.IDs.Z_EATER) * dmg));
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

    public int RunningStaminaCost(in Location dest)
    {
      if (Location.RequiresJump(in dest)) return Rules.STAMINA_COST_RUNNING+Rules.STAMINA_COST_JUMP+NightSTApenalty;
      return Rules.STAMINA_COST_RUNNING + NightSTApenalty;
    }

    public int WalkingStaminaCost(in Location dest)
    {
      if (Location.RequiresJump(in dest)) return Rules.STAMINA_COST_JUMP+NightSTApenalty;
      return 0;
    }

    public int MaxSTA {
      get => Model.BaseStaminaPoints + SKILL_HIGH_STAMINA_STA_BONUS * MySkills.GetSkillLevel(Skills.IDs.HIGH_STAMINA);
    }

    public bool IsTired {
      get => Model.Abilities.CanTire && m_StaminaPoints < STAMINA_MIN_FOR_ACTIVITY;
    }

    // 2023-04-22: For now, assume conditions for crouching are those for running.
    // revisit the stamina, etc. questions later
    // e.g., if we introduce the WEAK trait then *standing up* is problematic for a WEAK living
    private string ReasonCantCrouch()
    {
      if (!Model.Abilities.CanRun) return "no ability to crouch";
      if (StaminaPoints < STAMINA_MIN_FOR_ACTIVITY) return "not enough stamina to crouch";
      return "";
    }

    public bool CanCrouch(out string reason)
    {
      reason = ReasonCantCrouch();
      return string.IsNullOrEmpty(reason);
    }

    public bool CanCrouch()
    {
      return string.IsNullOrEmpty(ReasonCantCrouch());
    }

    private string ReasonCantRun()
    {
      if (!Model.Abilities.CanRun) return "no ability to run";
      if (StaminaPoints < STAMINA_MIN_FOR_ACTIVITY) return "not enough stamina to run";
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

	public bool RunIsFreeMove { get { return BASE_ACTION_COST/2 < m_ActionPoints; } }
	public bool WalkIsFreeMove { get { return BASE_ACTION_COST < m_ActionPoints; } }
    public int EnergyDrain { get { return BASE_ACTION_COST - Model.DollBody.Speed; } }
	public bool NextMoveLostWithoutRunOrWait { get { return EnergyDrain >= m_ActionPoints; } }

    /// <returns>0 have move, 1 would have move with one walk replaced by run, 2 no move</returns>
    public int MoveLost(int turns, int walk, int run) {
      int working = m_ActionPoints;
      working += turns* Model.DollBody.Speed;
      working -= BASE_ACTION_COST* walk;
      working -= (BASE_ACTION_COST/2)*run;
      if (-(BASE_ACTION_COST / 2) >= working) return 2;   // positively lost
      if (0 >= working) return 1;   // need to run to get a move
      return 0; // have move
    }

    public bool CanJump {
      get {
       if (Model.Abilities.CanJump) return true;
       var skills = MySkills;
       return 0 < skills.GetSkillLevel(Skills.IDs.AGILE)
           || 0 < skills.GetSkillLevel(Skills.IDs.Z_AGILE);
      }
    }

    private string ReasonCantUseExit()
    {
      if (!IsPlayer && !Model.Abilities.AI_CanUseAIExits) return "this AI can't use exits";
      return "";
    }

    public bool CanUseExit()
    {
      return string.IsNullOrEmpty(ReasonCantUseExit());
    }

    public bool CanUseExit(out string reason)
    {
      reason = ReasonCantUseExit();
      return string.IsNullOrEmpty(reason);
    }

	// Ultimately, we do plan to allow the AI to cross district boundaries
	private string ReasonCantLeaveMap(Point dest)
	{
      var exitAt = Location.Map.GetExitAt(dest);
      if (null == exitAt) return "no exit to leave map with";
      return exitAt.ReasonIsBlocked(this);
	}

    public bool CanLeaveMap(Point dest, out string reason)
    {
	  reason = ReasonCantLeaveMap(dest);
	  return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanLeaveMap(Point dest)
    {
	  return string.IsNullOrEmpty(ReasonCantLeaveMap(dest));
    }
#endif

    // optimized for pathfinding and ActionMoveDelta
    /// <param name="loc">preconditon: IsInBounds</param>
    private bool _CanEnter(in Location loc)
    { // reference is Map::IsWalkableFor, taking an ActorModel
      if (!loc.TileModel.IsWalkable) return false;
      // we don't check actors as this is a "is this valid, ever" test
      var mapObjectAt = loc.MapObject;
      if (mapObjectAt?.IsWalkable ?? true) return true;
      if (mapObjectAt.IsJumpable) return CanJump;
      if (mapObjectAt is DoorWindow door) {
        // Cf. Actor::ReasonNotWalkableFor.  Yes, rats *can* go through barricaded windows/doors
        if (Model.Abilities.IsSmall) return !door.IsClosed;
        // pathfinding livings will break barricaded doors (they'll prefer to go around it)
        if (door.BarricadePoints > 0) return CanBash(door) || Model.CanBreak(door);
        if (door.IsClosed) return CanOpen(door) || CanBash(door);
      } else if (Model.Abilities.IsSmall) return true;
      // have to inline relevant parts of Actor::CanPush
      if (mapObjectAt.IsMovable && !mapObjectAt.IsOnFire && AbleToPush) return true;
      return false;
    }

    public bool CanEnter(Location loc)
    {
      if (!Map.Canonical(ref loc)) return false;
      return _CanEnter(in loc);
    }

    public bool CanEnter(ref Location loc)
    {
      if (!Map.Canonical(ref loc)) return false;
      return _CanEnter(in loc);
    }

    public bool StrictCanEnter(in Location loc)
    {
      if (!Location.IsInBounds(in loc)) return false;
      return _CanEnter(in loc);
    }

    // generators work on point-based pathfinding
    public HashSet<Point> CastToBumpableDestinations(Map m,IEnumerable<Point> src) {
      var ret = new HashSet<Point>();
      foreach(var pt in src) {
        var loc = new Location(m, pt);
        if (!CanEnter(loc)) {
          foreach(var pt2 in pt.Adjacent()) {
            if (CanEnter(new Location(m, pt2))) ret.Add(pt2);
          }
          continue;
        }
        // containers are also usually best to target adjacent, but usually getting within 1 triggers alternate behavior
        ret.Add(pt);
      }
      return ret;
    }

    // likewise inventories
    public HashSet<Point> CastToInventoryAccessibleDestinations(Map m,IEnumerable<Point>? src) {
      var ret = new HashSet<Point>();
      if (null!=src) foreach(var pt in src) {
        var loc = new Location(m, pt);
        var obj = loc.MapObject;
        if (CanEnter(loc) && !(obj is ShelfLike)) {
          ret.Add(pt);
          continue;
        } else {
          foreach(var pt2 in pt.Adjacent()) {
            if (CanEnter(new Location(m, pt2))) ret.Add(pt2);
          }
          continue;
        }
      }
      return ret;
    }

    public List<Location>? MutuallyAdjacentFor(Location a, Location b)
    {
      if (3 <= Rules.InteractionDistance(in a, in b)) return null;
      var e = a.Exit;
      if (null != e && e.Location==b) return null;  // may not be true indefinitely (helicopters and/or building roofs)
      var ret = new List<Location>();
      foreach(var dir in Direction.COMPASS) {
        var loc = a+dir;
        if (CanEnter(ref loc) && 1 == Rules.GridDistance(in loc, in b)) ret.Add(loc);
      }
      return (0<ret.Count) ? ret : null;
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
      m_StaminaPoints = Model.Abilities.CanTire ? Math.Min(MaxSTA, m_StaminaPoints + staminaRegen) : STAMINA_INFINITE;
    }

    // sanity
    public bool IsInsane { get => Model.Abilities.HasSanity && 0 >= m_Sanity; }
    public int MaxSanity { get => Model.BaseSanity; }

    public int ScaleSanRegen(int baseValue)
    {
      return baseValue + (int)(/* (double) */ SKILL_STRONG_PSYCHE_ENT_BONUS * /* (int) */ (baseValue * MySkills.GetSkillLevel(Skills.IDs.STRONG_PSYCHE)));
    }

    public void SpendSanity(int sanCost)   // \todo unclear whether ok to rely on guard clause
    {
      if (Model.Abilities.HasSanity && 0 > (m_Sanity -= sanCost)) m_Sanity = 0;
    }

    public void RegenSanity(int sanRegen)   // \todo unclear whether ok to rely on guard clause
    {
      if (Model.Abilities.HasSanity) m_Sanity = Math.Min(MaxSanity, m_Sanity + sanRegen);
    }

    public int DisturbedLevel {
      get {
        const int SANITY_UNSTABLE_LEVEL = 2 * WorldTime.TURNS_PER_DAY;
        return (int) (SANITY_UNSTABLE_LEVEL * (1.0 - SKILL_STRONG_PSYCHE_LEVEL_BONUS * MySkills.GetSkillLevel(Skills.IDs.STRONG_PSYCHE)));
      }
    }

#nullable restore

    public bool IsDisturbed { get => Model.Abilities.HasSanity && Sanity <= DisturbedLevel; }

    public int HoursUntilUnstable {
      get {
        int num = Sanity - DisturbedLevel;
        return 0 < num ? num / WorldTime.TURNS_PER_HOUR : 0;
      }
    }

    // hunger
    public bool IsHungry { get { return _has_to_eat && FOOD_HUNGRY_LEVEL >= m_FoodPoints; } }
    public bool IsStarving { get { return _has_to_eat && 0 >= m_FoodPoints; } }
    public bool IsRotHungry { get { return Model.Abilities.IsRotting && ROT_HUNGRY_LEVEL >= m_FoodPoints; } }
    public bool IsRotStarving { get { return Model.Abilities.IsRotting && 0 >= m_FoodPoints; } }

    public int HoursUntilHungry {
      get {
        int num = FoodPoints - FOOD_HUNGRY_LEVEL;
        return (0 >= num ? 0 : num / WorldTime.TURNS_PER_HOUR);
      }
    }

    public bool IsAlmostHungry { get { return _has_to_eat && HoursUntilHungry <= 3; } }

    public int HoursUntilRotHungry {
      get {
        int num = FoodPoints - ROT_HUNGRY_LEVEL;
        return (0 >= num ? 0 : num / WorldTime.TURNS_PER_HOUR);
      }
    }

    public bool IsAlmostRotHungry  { get => Model.Abilities.IsRotting && HoursUntilRotHungry <= 3; }

    public int MaxFood {
      get {
        int num = (int) (Model.BaseFoodPoints * MySkills.GetSkillLevel(Skills.IDs.LIGHT_EATER) * SKILL_LIGHT_EATER_MAXFOOD_BONUS);
        return Model.BaseFoodPoints + num;
      }
    }

    public int MaxRot {
      get {
        int num = (int) (Model.BaseFoodPoints * MySkills.GetSkillLevel(Skills.IDs.Z_LIGHT_EATER) * SKILL_ZLIGHT_EATER_MAXFOOD_BONUS);
        return Model.BaseFoodPoints + num;
      }
    }

    public void Appetite(int f) => m_FoodPoints = Math.Max(0, m_FoodPoints - f);
    public void LivingEat(int f) => m_FoodPoints = Math.Min(m_FoodPoints + f, MaxFood);

    public void RottingEat(int f) { // intentionally not including healing-on-eating-flesh effect here
      m_FoodPoints = Math.Min(m_FoodPoints + BiteNutritionValue(f), MaxRot);
    }

#nullable enable
    // theoretically should be returning an enumeration
    // 0: failed to start dragging
    // 1: ok to start dragging
    // 2: ok to stop dragging
    private KeyValuePair<int, string> ReasonCantStartStopDrag(Corpse corpse) {
      if (null != m_DraggedCorpse) {
        if (corpse == m_DraggedCorpse) return new KeyValuePair<int, string>(2, string.Empty); // we may stop dragging this
        return new KeyValuePair<int, string>(0, "already dragging a corpse");
      }
      if (IsTired) return new KeyValuePair<int, string>(0, "tired");
      if (corpse.Location != Location || !Location.Map.Has(corpse)) return new KeyValuePair<int, string>(0, "not in same location");
      if (corpse.IsDragged) return new KeyValuePair<int, string>(0, "corpse is already being dragged"); // XXX should not happen when tested here
      return new KeyValuePair<int, string>(1, string.Empty); // we may start dragging this;
    }

    public int CanStartStopDrag(Corpse corpse, out string reason) // XXX \todo AI needs to learn how to drag corpses, but this likely will go through
    {
      var encode = ReasonCantStartStopDrag(corpse);
      reason = encode.Value;
      return encode.Key;
    }

    public int ScaleMedicineEffect(int baseEffect)
    {
      return baseEffect + (int)Math.Ceiling(/* (double) */ SKILL_MEDIC_BONUS * /* (int) */ (MySkills.GetSkillLevel(Skills.IDs.MEDIC) * baseEffect));
    }

    private string ReasonCantRevive(Corpse corpse)
    {
      if (0 == MySkills.GetSkillLevel(Skills.IDs.MEDIC)) return "lack medic skill";
      if (corpse.Location != Location) return "not there";
      if (corpse.RotLevel > 0) return "corpse not fresh";
      if (!m_Inventory.Has(Gameplay.Item_IDs.MEDICINE_MEDIKIT)) return "no medikit";
      return "";
    }

    public bool CanRevive(Corpse corpse, out string reason)
    {
      reason = ReasonCantRevive(corpse);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanRevive(Corpse corpse)
    {
      return string.IsNullOrEmpty(ReasonCantRevive(corpse));
    }

    public int ReviveChance(Corpse corpse)
    {
      if (!CanRevive(corpse)) return 0;
      return corpse.FreshnessPercent / 4 + MySkills.GetSkillLevel(Skills.IDs.MEDIC) * SKILL_MEDIC_REVIVE_BONUS;
    }

    public int DamageVsCorpses {
      get => m_CurrentMeleeAttack.DamageValue / 2 + SKILL_NECROLOGY_CORPSE_BONUS * MySkills.GetSkillLevel(Skills.IDs.NECROLOGY);
    }

    // sleep
    public int TurnsUntilSleepy {
      get {
        int num = SleepPoints - SLEEP_SLEEPY_LEVEL;
        if (num <= 0) return 0;
        WorldTime now = new WorldTime(Location.Map.LocalTime);
        int turns = 0;
        while(0<num) {
          int delta_t = WorldTime.TURNS_PER_HOUR-now.Tick;
          int awake_cost = (now.IsNight ? 2 : 1);
          int SLP_cost = awake_cost*delta_t;
          if (SLP_cost > num) {
            turns += num/awake_cost;
            break;
          }
          num -= SLP_cost;
          turns += delta_t;
          now.TurnCounter += delta_t;
        }
        return turns;
      }
    }

    public int SleepRegen(bool isOnCouch)
    {
      const int SLEEP_COUCH_SLEEPING_REGEN = 6;
      const int SLEEP_NOCOUCH_SLEEPING_REGEN = 4;
      int num1 = isOnCouch ? SLEEP_COUCH_SLEEPING_REGEN : SLEEP_NOCOUCH_SLEEPING_REGEN;
      int num2 = (int) (/* (double) */ SKILL_AWAKE_SLEEP_REGEN_BONUS * /* (int) */(num1* MySkills.GetSkillLevel(Skills.IDs.AWAKE)));
      return num1 + num2;
    }

#if PROTOTYPE
    public int EstimateWakeup(int delta,bool IsOnCouch)
    {
      int rest_rate = SleepRegen(IsOnCouch);
      int num = Rules.SLEEP_BASE_POINTS - SleepPoints;
      WorldTime now = new WorldTime(Location.Map.LocalTime);
      if (0<delta) {
        do {
          int delta_t = WorldTime.TURNS_PER_HOUR-now.Tick;
          int awake_cost = (now.IsNight ? 2 : 1);
          if (delta<=delta_t) {
            num += delta*awake_cost;
            delta = 0;
            now.TurnCounter += delta;
          } else {
            num += delta_t*awake_cost;
            delta -= delta_t;
            now.TurnCounter += delta_t;
          }
        } while(0<delta);
      }
      return now.TurnCounter + num/rest_rate+1;  // XXX ignore exactly divisible special case; waking up one turn earlier isn't a disaster
    }
#endif

    public int HoursUntilSleepy { get { return TurnsUntilSleepy/WorldTime.TURNS_PER_HOUR; } }
    public bool IsAlmostSleepy { get { return Model.Abilities.HasToSleep && 3 >= HoursUntilSleepy; } }
    public bool IsSleepy { get { return Model.Abilities.HasToSleep && SLEEP_SLEEPY_LEVEL >= SleepPoints; } }
    public bool IsExhausted { get { return Model.Abilities.HasToSleep && 0 >= SleepPoints; } }

    public bool WouldLikeToSleep {
      get {
        return IsAlmostSleepy /* || IsSleepy */;    // cf above partial ordering
      }
    }

    public int MaxSleep {
      get {
        int num = (int) (/* (double) */ SKILL_AWAKE_SLEEP_BONUS * /* (int) */ (Model.BaseSleepPoints * MySkills.GetSkillLevel(Skills.IDs.AWAKE)));
        return Model.BaseSleepPoints + num;
      }
    }

    private string ReasonCantSleep()
    {
      if (!Model.Abilities.HasToSleep) return "no ability to sleep";
      if (IsHungry || IsStarving) return "hungry";
      if (SleepPoints >= MaxSleep - WorldTime.TURNS_PER_HOUR) return "not sleepy at all";
      return "";
    }

    public bool CanSleep(out string reason)
    {
      reason = ReasonCantSleep();
      return string.IsNullOrEmpty(reason);
    }

    public bool CanSleep()
    {
      return string.IsNullOrEmpty(ReasonCantSleep());
    }

    public bool WantToSleepNow { get { return WouldLikeToSleep && CanSleep(); } }

    public void StartSleeping()
    {
      // all battery powered items other than the police radio are left hand, currently
      // the police radio is DollPart.HIP_HOLSTER, *but* it recharges on movement faster than it drains
      var it = GetEquippedItem(DollPart.LEFT_HAND);
      if (it is BatteryPowered) it.UnequippedBy(this);
      // the above is not appropriate for collapsing from exhaustion, just intentional sleeping

      SpendActionPoints();
      StopDraggingCorpse();
      Activity = Data.Activity.SLEEPING;
      IsSleeping = true;
    }


    public void Rest(int s) => m_SleepPoints = Math.Min(m_SleepPoints + s, MaxSleep);
    public void Drowse(int s) => m_SleepPoints = Math.Max(0, m_SleepPoints - s);

    public int LoudNoiseWakeupChance(int noiseDistance)
    {
      const int LOUD_NOISE_BASE_WAKEUP_CHANCE = 10;
      const int LOUD_NOISE_DISTANCE_BONUS = 10;
      return LOUD_NOISE_BASE_WAKEUP_CHANCE + SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS * MySkills.GetSkillLevel(Skills.IDs.LIGHT_SLEEPER) + Math.Max(0, (Rules.LOUD_NOISE_RADIUS - noiseDistance) * LOUD_NOISE_DISTANCE_BONUS);
    }

    public void Vomit()
    {
      const int FOOD_VOMIT_STA_COST = 100;

      m_StaminaPoints -= FOOD_VOMIT_STA_COST;
      Drowse(WorldTime.TURNS_PER_HOUR);
      Appetite(WorldTime.TURNS_PER_HOUR);
      // \todo more "accurate" duration, should it make sense for other reasons
      // RS Alpha 10.1- was permanent
      Location.Map.AddTimedDecoration(Location.Position, Gameplay.GameImages.DECO_VOMIT, District.IsSewersMap(Location.Map) ? 3*WorldTime.HOURS_PER_DAY*WorldTime.TURNS_PER_HOUR : 7 * WorldTime.HOURS_PER_DAY * WorldTime.TURNS_PER_HOUR, TRUE);
    }

    // bypasses armor
    public bool RawDamage(int dmg) => 0 >= Interlocked.Add(ref m_HitPoints, -dmg);

    public bool TakeDamage(int dmg)
    {
      const int BODY_ARMOR_BREAK_CHANCE = 2;

      if (Model.Abilities.CanTire) m_StaminaPoints -= dmg;
      var game = RogueGame.Game;
      var worn = GetEquippedArmor();
      if (null != worn && Rules.Get.RollChance(BODY_ARMOR_BREAK_CHANCE)) {
        Destroyed(worn);
        game.UI_ItemBreaks(this, worn);
      }
      if (IsSleeping) game.DoWakeUp(this);
      return RawDamage(dmg);
    }

    public void RevivedBy(Actor medic)
    {
#if IRRATIONAL_CAUTION
        if (!IsDead) throw new InvalidOperationException("can only revive the dead");
        if (0 >= medic.MySkills.GetSkillLevel(Skills.IDs.MEDIC)) throw new InvalidOperationException("unqualified medic");
#endif
        IsDead = false;
        m_HitPoints = 5 + medic.MySkills.GetSkillLevel(Skills.IDs.MEDIC);
        m_Doll.RemoveDecoration(GameImages.BLOODIED);
        Activity = Activity.IDLE;
        m_TargetActor = null;
    }

    // alpha10: boring items moved to ItemEntertaimment from Actor
    // inventory stats

    // This is the authoritative source for a living actor's maximum inventory.
    // As C# has no analog to a C++ const method or const local variables,
    // use this to prevent accidental overwriting of MaxCapacity by bugs.
    public int MaxInv {
      get {
        return Model.BaseInventoryCapacity + SKILL_HAULER_INV_BONUS * MySkills.GetSkillLevel(Skills.IDs.HAULER);
      }
    }

    public bool Take(Item it) {
      return m_Inventory?.AddAll(it) ?? false;
    }

    public bool HasItemOfModel(Model.Item model)
    {
      return m_Inventory?.HasModel(model) ?? false;
    }

    public int Count(Model.Item model)
    {
      return m_Inventory?.Count(model) ?? 0;
    }

    public int CountQuantityOf<_T_>() where _T_ : Item
    {
      return m_Inventory?.CountQuantityOf<_T_>() ?? 0;
    }

    public int CountItemsOfSameType(Type tt)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
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

    public int CountItems<_T_>() where _T_ : Item
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
      return m_Inventory.Where(it=>it is _T_).Select(it => it.Quantity).Sum();
    }

    public bool Has<_T_>() where _T_ : Item
    {
      return m_Inventory?.Has<_T_>() ?? false;
    }

    public bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable) return m_Inventory.CountQuantityOf(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
    }

    public bool HasAtLeastFullStackOf(Model.Item it, int n) => m_Inventory?.HasAtLeastFullStackOf(it, n) ?? false; // release mode IL 20h i.e. would inline
//  public bool HasAtLeastFullStackOf(Item it, int n) => HasAtLeastFullStackOf(it.Model, n); // manually inlined below
    public bool HasAtLeastFullStackOf(Item it, int n) => m_Inventory?.HasAtLeastFullStackOf(it.Model, n) ?? false;

    public ItemMeleeWeapon? GetWorstMeleeWeapon()
    {
      var melee = m_Inventory?.GetItemsByType<ItemMeleeWeapon>();
      if (null == melee) return null;
      if (1 == melee.Count) return melee[0];
      // some sort of invariant problem here
      // NOTE: martial arts influences the apparent rating considerably
      int rate(ItemMeleeWeapon w) { return MeleeWeaponAttack(w.Model).Rating; }

      return melee.Where(Item.notEquipped).Minimize(rate) ?? melee.Minimize(rate);
    }

    public ItemBodyArmor? GetBestBodyArmor() => m_Inventory?.Maximize<ItemBodyArmor, int>(ItemBodyArmor.Rate);
    public ItemBodyArmor? GetWorstBodyArmor() => m_Inventory?.Minimize<ItemBodyArmor, int>(Item.notEquipped, ItemBodyArmor.Rate);

    public bool HasEnoughFoodFor(int nutritionNeed, ItemFood? exclude=null)
    {
      if (!_has_to_eat) return true;
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      var tmp = m_Inventory.GetItemsByType<ItemFood>();
      if (null == tmp) return false;
      int turnCounter = Location.Map.LocalTime.TurnCounter;
//    int num = 0;
      int num = m_FoodPoints-FOOD_HUNGRY_LEVEL;
      if (num >= nutritionNeed) return true;
      foreach (ItemFood tmpFood in tmp) {
        if (exclude==tmpFood) continue;
        num += tmpFood.NutritionAt(turnCounter)*tmpFood.Quantity;
        if (num >= nutritionNeed) return true;
      }
      return false;
    }

    public int ItemNutritionValue(int baseValue)
    {
      return baseValue + (int)(/* (double) */ SKILL_LIGHT_EATER_FOOD_BONUS * /* (int) */ (baseValue * MySkills.GetSkillLevel(Skills.IDs.LIGHT_EATER)));
    }

    public int BiteNutritionValue(int baseValue)
    {
      const float CORPSE_EATING_NUTRITION_FACTOR = 10f;

      var skills = MySkills;
      return (int) (CORPSE_EATING_NUTRITION_FACTOR + SKILL_ZLIGHT_EATER_FOOD_BONUS * skills.GetSkillLevel(Skills.IDs.Z_LIGHT_EATER) + SKILL_LIGHT_EATER_FOOD_BONUS * skills.GetSkillLevel(Skills.IDs.LIGHT_EATER)) * baseValue;
    }

    public int CurrentNutritionOf(ItemFood food)
    {
      return ItemNutritionValue(food.NutritionAt(Location.Map.LocalTime.TurnCounter));
    }

    public int CharismaticTradeChance {
      get {
        return SKILL_CHARISMATIC_TRADE_BONUS * MySkills.GetSkillLevel(Skills.IDs.CHARISMATIC);
      }
    }

    public List<Item>? GetInterestingTradeableItems(Actor buyer) // called from RogueGame::PickItemToTrade so forced to be public no matter where
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

    public List<Item>? GetRationalTradeableItems(Gameplay.AI.OrderableAI buyer)    // only called from AI trading decision making
    {
#if DEBUG
      if (!Model.Abilities.CanTrade) throw new InvalidOperationException(Name+" cannot trade");
#endif

      var objList = m_Inventory.Where(it=> buyer.IsRationalTradeItem(it) && (Controller as Gameplay.AI.OrderableAI).IsTradeableItem(it));
      return objList.Any() ? objList.ToList() : null;
    }

    // equipped items
    public Item? GetEquippedItem(DollPart part)
    {
      if (DollPart.TORSO == part) return GetEquippedArmor();
      return m_Inventory?.GetFirst<Item>(obj => obj.EquippedPart == part);
    }

    public Item? GetEquippedItem(Gameplay.Item_IDs id)
    {
      return m_Inventory?.GetFirst<Item>(obj => obj.ModelID == id && DollPart.NONE != obj.EquippedPart);
    }

    // considering these as change target
    public ItemBodyArmor? GetEquippedArmor()
    {
      if (null != m_InventorySlots && m_InventorySlots[SLOT_H_TORSO] is ItemBodyArmor armor) return armor;
      // legacy implementation
      return m_Inventory?.GetFirst<ItemBodyArmor>(obj => obj.EquippedPart == DollPart.TORSO);
    }

    public ItemBodyArmor? UsingCopArmor()
    {
      var ret = GetEquippedArmor();
      if (null != ret && ret.IsFriendlyForCops()) return ret;
      return null;
    }

    // this cannot be side-effecting (martial arts, grenades)
    public Item? GetEquippedWeapon() { return GetEquippedItem(DollPart.RIGHT_HAND); }
    public bool HasEquipedRangedWeapon() { return GetEquippedWeapon() is ItemRangedWeapon; }

    // maybe this should be over on the Inventory object
    public Item? GetItem(Gameplay.Item_IDs id) { return m_Inventory?.GetFirst(id); }

    private string ReasonCantTradeWith(Actor target)
    {
#if OBSOLETE
      if (target.IsPlayer) return "target is player";
#else
      if (!IsPlayer && target.IsPlayer) return "target is player";
#endif
      if (!Model.Abilities.CanTrade && target.Leader != this) return "can't trade";
      if (!target.Model.Abilities.CanTrade && target.Leader != this) return "target can't trade";
      if (IsEnemyOf(target)) return "is an enemy";
      if (target.IsSleeping) return "is sleeping";
      if (m_Inventory == null || m_Inventory.IsEmpty) return "nothing to offer";
      if (target.m_Inventory == null || target.m_Inventory.IsEmpty) return "has nothing to trade";
      // alpha10 dont bother someone who is fighting or fleeing
      if (target.Activity == Activity.CHASING || target.Activity == Activity.FIGHTING || target.Activity == Activity.FLEEING || target.Activity == Activity.FLEEING_FROM_EXPLOSIVE) {
        if (!target.IsPlayer) return "in combat";
      }
      // RS Revived: no trading with differing treacherous factions; should be "by AI controller" but player controller would need to simulate underlying target
      switch(m_FactionID)
      {
      case GameFactions.IDs.TheCHARCorporation:
      case GameFactions.IDs.TheBikers:
      case GameFactions.IDs.TheGangstas:
        if (target.m_FactionID!=m_FactionID) return "untrustworthy";
        break;
      }

      switch(target.m_FactionID)
      {
      case GameFactions.IDs.TheCHARCorporation:
      case GameFactions.IDs.TheBikers:
      case GameFactions.IDs.TheGangstas:
        if (target.m_FactionID!=m_FactionID) return "anticipates treachery";
        break;
      }

#if OBSOLETE
      if (!IsPlayer) {
#else
      if (!IsPlayer && !target.IsPlayer) {
#endif
        var theirs = target.GetRationalTradeableItems(this.Controller as Gameplay.AI.OrderableAI);
        if (null == theirs) return "target unwilling to trade";
        var mine = GetRationalTradeableItems(target.Controller as Gameplay.AI.OrderableAI);
        if (null == mine) return "unwilling to trade";
        bool ok = false;
        foreach(Item want in theirs) {
          foreach(Item have in mine) {
            if (   !Gameplay.AI.ObjectiveAI.TradeVeto(have, want)
                && !Gameplay.AI.ObjectiveAI.TradeVeto(want, have)) {
               ok = true;
               break;
            }
          }
        }
        if (!ok) return "no mutually acceptable trade";
      }
      return "";
    }

    public bool CanTradeWith(Actor target, out string reason)
    {
      reason = ReasonCantTradeWith(target);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanTradeWith(Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantTradeWith(target));
    }

    private string ReasonCantUseItem(Item it)
    {
      if (!Model.Abilities.CanUseItems) return "no ability to use items";
      if (it is ItemWeapon) return "to use a weapon, equip it";
      if (it is ItemBodyArmor) return "to use armor, wear it";
      if (it is ItemBarricadeMaterial) return "to use material, build a barricade";
      if (!(it is UsableItem obj)) return "not a usable item type"; // cannot throw due to insane actions
      var err = obj.ReasonCantUse(this);
      if (!string.IsNullOrEmpty(err)) return err;
      if (!m_Inventory?.Contains(it) ?? true) return "not in inventory";
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

    public bool CanGetItems() {
      return Model.Abilities.HasInventory && Model.Abilities.CanUseMapObjects && null != m_Inventory;
    }

    private string ReasonCantGet(Item it)
    {
      if (!CanGetItems()) return "no inventory";
      if (m_Inventory.IsFull && !m_Inventory.CanAddAtLeastOne(it)) return "inventory is full";
      if (it is ItemTrap trap && trap.IsTriggered) return "triggered trap";
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

    public bool MayTakeFrom(in Data.Model.InvOrigin stack)
    {
      // Rules.IsAdjacent would also check the other side of the stairs.
      if (1 >= Rules.GridDistance(in m_Location, stack.Location)) return true;
      return false;
    }

    public bool MayTakeFromStackAt(in Location loc)
    {
      if (Location == loc) return true;
      if (1 != Rules.GridDistance(in m_Location, in loc)) return false; // Rules.IsAdjacent would also check the other side of the stairs
      // currently all containers are not-walkable for UI reasons.
      return loc.Map.GetMapObjectAt(loc.Position) is ShelfLike;
    }

    public bool StackIsBlocked(in Location loc)
    {
      if (Location == loc) return false;
      var obj = loc.MapObject;    // XXX this check should affect BehaviorResupply
      if (null == obj) return false;
      if (!(obj is ShelfLike) && !loc.IsWalkableFor(this)) {
        // Cf. Actor::CanOpen
        if (obj is DoorWindow doorWindow && doorWindow.IsBarricaded) return true;
        // Cf. Actor::CanPush; closed door/window is not pushable but can be handled
        else if (!obj.IsMovable) return true; // would have to handle OnFire if that could happen
      }
      // e.g., inventory with both armed and unarmed bear traps
      if (2*loc.Map.TrapsUnavoidableMaxDamageAtFor(loc.Position,this)>=m_HitPoints) return true;
      return false;
    }

    private string ReasonCantGiveTo(Actor target, Item gift)
    {
      if (IsEnemyOf(target)) return "enemy";
      if (gift.IsEquipped) return "equipped";
      if (target.IsSleeping) return "sleeping";
      return target.ReasonCantGet(gift);
    }

    public bool CanGiveTo(Actor target, Item gift, out string reason)
    {
      reason = ReasonCantGiveTo(target,gift);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanGiveTo(Actor target, Item gift) => string.IsNullOrEmpty(ReasonCantGiveTo(target, gift));

    private string ReasonCantEquip(Item it)
    {
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
      if (!it.IsEquipped) return "not equipped";
      if (!m_Inventory?.Contains(it) ?? true) return "not in inventory";
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
      if (!m_Inventory?.Contains(it) ?? true) return "not in inventory";
      return "";
    }

    public bool CanDrop(Item it, out string reason)
    {
      reason = ReasonCantDrop(it);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanDrop(Item it) => string.IsNullOrEmpty(ReasonCantDrop(it));

    public void SkillUpgrade(Skills.IDs id)
    {
      MySkills.AddOrIncreaseSkill(id);
      switch(id)
      {
      case Skills.IDs.HAULER: if (null != m_Inventory) m_Inventory.MaxCapacity = MaxInv;
        break;
      case Skills.IDs.TOUGH: m_HitPoints += SKILL_TOUGH_HP_BONUS;
        break;
      case Skills.IDs.Z_TOUGH: m_HitPoints += SKILL_ZTOUGH_HP_BONUS;
        break;
      }
    }

    // flag handling
    private bool GetFlag(Flags f) { return (m_Flags & f) != Flags.NONE; }

    private void SetFlag(Flags f, bool value)
    {
      if (value)
        m_Flags |= f;
      else
        m_Flags &= ~f;
    }

#if DEAD_FUNC
    private void OneFlag(Flags f)
    {
      m_Flags |= f;
    }

    private void ZeroFlag(Flags f)
    {
      m_Flags &= ~f;
    }
#endif
    private void Clear(Flags f) { m_Flags &= ~f; }
    private void Set(Flags f) { m_Flags |= f; }

#region vision
    // if this ends up on hot path, consider inlining
    private int DarknessFOV { get => Model.Abilities.IsUndead ? Model.BaseViewRange : MINIMAL_FOV; }

    // alpha10
    public bool CanSeeSky { get => !IsDead && !IsSleeping && Location.Map.Lighting == Lighting.OUTSIDE; }

    private static int LivingNightFovPenalty(WorldTime time)
    {
      switch (time.Phase) {
        case DayPhase.SUNSET: return FOV_PENALTY_SUNSET;
        case DayPhase.EVENING: return FOV_PENALTY_EVENING;
        case DayPhase.MIDNIGHT: return FOV_PENALTY_MIDNIGHT;
        case DayPhase.DEEP_NIGHT: return FOV_PENALTY_DEEP_NIGHT;
        case DayPhase.SUNRISE: return FOV_PENALTY_SUNRISE;
        default: return 0;
      }
    }

    public int NightFovPenalty(WorldTime time) => Model.Abilities.IsUndead ? 0 : LivingNightFovPenalty(time);

    private static int LivingWeatherFovPenalty(Weather weather)
    {
      switch (weather) {
        case Weather.RAIN: return FOV_PENALTY_RAIN;
        case Weather.HEAVY_RAIN: return FOV_PENALTY_HEAVY_RAIN;
        default: return 0;
      }
    }

    public int WeatherFovPenalty(Weather weather) => Model.Abilities.IsUndead ? 0 : LivingWeatherFovPenalty(weather);

    /* XXX we have a lighting paradox (worse in Staying Alive where on-fire cars are light sources, but can be triggered here as well)
     * In a subway, base FOV range is 8 in a lit section, but 2 in an unlit section -- even when at the border between a lit and unlit
     * subway map
     *
     * The "intuitive" way is to say that each location has a light level, and that if the light level equals or exceeds the required-to-see
     * light level then it's in view; flashlights "work" by lowering the required-to-see light level (as we're gamey and don't micro-manage
     * things like exactly where the flashlight is pointing, *especially* when at the default time scale of 2 minutes/turn.)
     */
    public short FOVrangeNoFlashlight(WorldTime time, Weather weather)
    {
      if (IsSleeping) return 0;
      int FOV = Model.BaseViewRange;
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
      if (Location.MapObject?.StandOnFovBonus ?? false) FOV += FOV_BONUS_STANDING_ON_OBJECT;
      return (short)Math.Max(MINIMAL_FOV, FOV);
    }

    private short LightBonus {
      get {
        if (GetEquippedItem(DollPart.LEFT_HAND) is ItemLight light && 0 < light.Batteries) return light.FovBonus;
        return 0;
      }
    }
    private static bool UsingLight(Actor a) => 0 < a.LightBonus;

    public short FOVrange(WorldTime time, Weather weather)
    {
      if (IsSleeping) return 0; // repeat this short-circuit here for correctness
      var FOV = FOVrangeNoFlashlight(time, weather);
      Lighting light = Location.Map.Lighting;
      if (light == Lighting.DARKNESS || (light == Lighting.OUTSIDE && time.IsNight)) {
        var lightBonus = LightBonus;
        if (0 == lightBonus && Location.Map.HasAnyAdjacent(Location.Position, UsingLight)) lightBonus = 1;
        FOV += lightBonus;
      }
      return Math.Max(MINIMAL_FOV, FOV);
    }

    static public int MaxLivingFOV(Map map)
    {
      WorldTime time = map.LocalTime;
      Lighting light = map.Lighting;
      int FOV = MAX_BASE_VISION;
      switch (light) {
        case Lighting.DARKNESS:
          FOV = MINIMAL_FOV;
          break;
        case Lighting.OUTSIDE:
          FOV -= LivingNightFovPenalty(time) + LivingWeatherFovPenalty(World.Get.Weather);
          break;
      }
      FOV += FOV_BONUS_STANDING_ON_OBJECT;  // but there are no relevant objects except on the entry map

      if (light == Lighting.DARKNESS || (light == Lighting.OUTSIDE && time.IsNight)) {
        FOV += MAX_LIGHT_FOV_BONUS;
      }
      return Math.Max(MINIMAL_FOV, FOV);
    }
#endregion

    public int ZGrabChance(Actor victim) => MySkills.GetSkillLevel(Skills.IDs.Z_GRAB) * SKILL_ZGRAB_CHANCE;

    // smell
    public double Smell {
      get {
        return (1.0 + SKILL_ZTRACKER_SMELL_BONUS * MySkills.GetSkillLevel(Skills.IDs.Z_TRACKER)) * Model.BaseSmellRating;
      }
    }

    public int SmellThreshold {
      get {
        if (IsSleeping) return -1;
        // Even a skill level of 1 will give a ZM a raw negative smell threshold.
        return Math.Max(1,(OdorScent.MAX_STRENGTH+1) - (int) (Smell * OdorScent.MAX_STRENGTH));
      }
    }

    // event handlers
    // should prefer to call it.EquippedBy(whom); rather than whom.Equip(it); , for side effects
    public void Equip(Item it)
    {
      it.Equip();
      var model = it.Model;
      if (model is ItemMeleeWeaponModel melee) {
        m_CurrentMeleeAttack = melee.BaseMeleeAttack(Model);
        return;
      }
      if (model is ItemRangedWeaponModel rw) {
        m_CurrentRangedAttack = rw.Attack;   // value-copy due to struct Attack
        return;
      }
      if (model is Data.Model.BodyArmor armor) {
        m_CurrentDefence += armor.ToDefence();
        return;
      }
      if (it is BatteryPowered powered) {
        --powered.Batteries;
        if (powered.AugmentsSenses(this)) Controller.UpdateSensors();
        return;
      }
    }

    public void OnUnequipItem(Item it)
    {
      var model = it.Model;
      if (model is ItemMeleeWeaponModel) {
        m_CurrentMeleeAttack = Model.UnarmedAttack;
        return;
      }
      if (model is ItemRangedWeaponModel) {
        m_CurrentRangedAttack = Attack.BLANK;
        return;
      }
      if (model is Data.Model.BodyArmor armor) {
        m_CurrentDefence -= armor.ToDefence();
        if (null != m_InventorySlots && it == m_InventorySlots[SLOT_H_TORSO]) {
          if (m_InventorySlots.Transfer(SLOT_H_TORSO, new(this))) return;
          m_InventorySlots.Transfer(SLOT_H_TORSO, new(Location));
        }
        return;
      }
    }

    public void Destroyed(Item it)
    {
      if (null == m_InventorySlots || !m_InventorySlots.Destroyed(it)) {
        m_Inventory?.RemoveAllQuantity(it);
      }
      it.UnequippedBy(this, false);
    }

    public void Remove(Item it, bool canMessage=true)
    {
      it.UnequippedBy(this, canMessage);
      m_Inventory.RemoveAllQuantity(it);
    }

    // Note that an event-based Sees implementation (anchored in RogueGame) cannot avoid constructing messages
    // even when no players would recieve them.
#region Event-based Say implementation
    public struct SayArgs
    {
      public readonly Actor _target;
      public readonly List<UI.Message> messages;
      public readonly bool _important;
      public bool shown;

      public SayArgs(Actor target, bool important)
      {
        _target = target;
        messages = new();
        _important = important;
        shown = false;
      }
    }

    public static event EventHandler<SayArgs>? Says;

    // experimental...testing an event approach to this
    public void Say(Actor target, string text, Sayflags flags)
    {
      Color sayColor = ((flags & Sayflags.IS_DANGER) != 0) ? RogueGame.SAYOREMOTE_DANGER_COLOR : RogueGame.SAYOREMOTE_NORMAL_COLOR;

      if ((flags & Sayflags.IS_FREE_ACTION) == 0) SpendActionPoints();

      var handler = Says; // work around non-atomic test, etc.
      if (null != handler) {
        SayArgs tmp = new SayArgs(target, target.IsPlayer || (flags & Sayflags.IS_IMPORTANT) != 0);
        tmp.messages.Add(RogueGame.MakeMessage(this, string.Format("to {0} : ", target.TheName), sayColor));
        tmp.messages.Add(RogueGame.MakeMessage(this, string.Format("\"{0}\"", text), sayColor));
        handler(this,tmp);
      }
    }
#endregion
#region Event-based Dies implementation
    public readonly struct DieArgs
    {
      public readonly Actor _deadGuy;
      public readonly Actor? _killer;
      public readonly string _reason;

      public DieArgs(Actor deadGuy, Actor? killer, string reason)
      {
        _deadGuy = deadGuy;
        _killer = killer;
        _reason = reason;
      }
    }

    public static event EventHandler<DieArgs>? Dies;

    public void Killed(string reason, Actor? killer=null) {
      var handler = Dies; // work around non-atomic test, etc.
      if (null != handler) {
        DieArgs tmp = new DieArgs(this,killer,reason);
        handler(this,tmp);
      }
    }
#endregion

    public static event EventHandler? Moving;
    public void Moved() { Moving?.Invoke(this, null); }

    // death-related administrative functions
    public void RecordKill(Actor victim) => ActorScoring.AddKill(victim, Session.Get.WorldTime.TurnCounter);

    // administrative functions whose presence here is not clearly advisable but they improve the access situation here
    public void StartingSkill(Skills.IDs skillID,int n=1)
    {
      while(0< n--) {
        if (m_Skills.GetSkillLevel(skillID) >= Skills.MaxSkillLevel(skillID)) return;
        m_Skills.AddOrIncreaseSkill(skillID);
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
      if (null != m_Inventory) m_Inventory.MaxCapacity = MaxInv;
    }

    public void CreateCivilianDeductFoodSleep() {
      var rules = Rules.Get;
      m_FoodPoints -= rules.Roll(0, m_FoodPoints / 4);
      m_SleepPoints -= rules.Roll(0, m_SleepPoints / 4);
    }

    public bool AfterAction()
    {
      m_previousHitPoints = m_HitPoints;
      m_previousFoodPoints = m_FoodPoints;
      m_previousSleepPoints = m_SleepPoints;
      m_previousSanity = m_Sanity;
      return 0<m_ActionPoints;
    }

    public void DropScent()
    {
      const int LIVING_SCENT_DROP = OdorScent.MAX_STRENGTH;
      const int UNDEAD_MASTER_SCENT_DROP = OdorScent.MAX_STRENGTH;

      // decay suppressor
      if (0 < OdorSuppressorCounter) {
        if (0 > (OdorSuppressorCounter -= Location.OdorsDecay())) OdorSuppressorCounter = 0;
        return;
      }

      if (Model.Abilities.IsUndead) {
        if (Model.Abilities.IsUndeadMaster) Location.Map.RefreshScentAt(Odor.UNDEAD_MASTER, UNDEAD_MASTER_SCENT_DROP, Location.Position);
      } else
        Location.Map.RefreshScentAt(Odor.LIVING, LIVING_SCENT_DROP, Location.Position);
    }

    public void PreTurnStart()
    {
       DropScent();
       if (!IsSleeping) Interlocked.Add(ref m_ActionPoints, Speed);
       if (m_StaminaPoints < MaxSTA) RegenStaminaPoints(STAMINA_REGEN_WAIT);
       // Stop tired actors from running.
       if (IsRunning && m_StaminaPoints < STAMINA_MIN_FOR_ACTIVITY) {
         Walk();
         if (Controller is PlayerController pc) {
           pc.AddMessage(RogueGame.MakeMessage(this, string.Format("{0} too tired to continue running!", Engine.RogueGame.VERB_BE.Conjugate(this))));
         }
       }
    }

    // This prepares an actor for being a PC.  Note that hacking the player controller in
    // by hex-editing does work flawlessly at the Actor level.
    public void PrepareForPlayerControl()
    {
      var leader = Leader;
      if (null != leader && !leader.IsPlayer) leader.RemoveFollower(this);   // needed if leader is NPC
    }

    // This is a backstop for bugs elsewhere.
    // Just optimize everything that's an Actor or contains an Actor.
    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      if (m_TargetActor?.IsDead ?? false) m_TargetActor = null;
      if (m_Leader?.IsDead ?? false) m_Leader = null;
      // \todo match RS alpha 10's prune of trust entries in dead actors (m_TrustDict for us)
      // to avoid weirdness we want to drop only if no revivable corpse is in-game
      if (null != m_Followers) {
        m_Followers.OnlyIfNot(IsDeceased);
        if (0 >= m_Followers.Count) m_Followers = null;
        else m_Followers.TrimExcess();
      }
      if (null != m_AggressorOf) {
        m_AggressorOf.OnlyIfNot(IsDeceased);
        if (0 >= m_AggressorOf.Count) m_AggressorOf = null;
        else m_AggressorOf.TrimExcess();
      }
      if (null != m_SelfDefenceFrom) {
        m_SelfDefenceFrom.OnlyIfNot(IsDeceased);
        if (0 >= m_SelfDefenceFrom.Count) m_SelfDefenceFrom = null;
        else m_SelfDefenceFrom.TrimExcess();
      }
    }

    public override string ToString() { return Name; }

#region IEquatable<>
	// C# docs indicate using Actor as a key wants these
    public bool Equals(Actor? x)
    {
      if (null == x) return false;
      if (SpawnTime != x.SpawnTime) return false;
      if (m_Name != x.m_Name) return false;
      if (Location!=x.Location) return false;
      return true;
    }

    public override bool Equals(object? obj)
    {
      return obj is Actor tmp && Equals(tmp);
    }

    public override int GetHashCode() => SpawnTime ^ m_Name.GetHashCode();
#endregion

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
      IS_CROUCHING = 64,
    }
  }

  static internal class Actor_ext {
    public static int CountUndead(this IEnumerable<Actor> src)
    {
      return src.Count(a => a.Model.Abilities.IsUndead);
    }
  }
}

namespace Zaimoni.Serialization
{

    public partial interface ISave
    {
        static void LinearLoad(DecodeObjects decode, Action<ActorTag[]> handler)
        {
            int count = 0;
            Formatter.Deserialize7bit(decode.src, ref count);
            if (0 >= count) return; // no action needed
            var dest = new ActorTag[count];

            int n = 0;
            while (0 < count) {
                dest[n] = new ActorTag(decode);
                n++;
            }
            handler(dest);
        }
    }
}

