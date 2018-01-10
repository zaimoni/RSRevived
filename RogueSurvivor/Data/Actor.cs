// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Actor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define XDISTRICT_PATHING

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
using ActionUseExit = djack.RogueSurvivor.Engine.Actions.ActionUseExit;
using Skills = djack.RogueSurvivor.Gameplay.Skills;
using PowerGenerator = djack.RogueSurvivor.Engine.MapObjects.PowerGenerator;
using Fortification = djack.RogueSurvivor.Engine.MapObjects.Fortification;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Actor
  {
    public const int FOOD_HUNGRY_LEVEL = WorldTime.TURNS_PER_DAY;
    public const int ROT_HUNGRY_LEVEL = 2*WorldTime.TURNS_PER_DAY;
    public const int SLEEP_SLEEPY_LEVEL = 30*WorldTime.TURNS_PER_HOUR;
    private const int STAMINA_INFINITE = 99;
    public const int STAMINA_MIN_FOR_ACTIVITY = 10; // would space-time scale if stamina itself space-time scaled
    private const int NIGHT_STA_PENALTY = 2;
    public const int STAMINA_REGEN_WAIT = 2;
    public const int TRUST_BOND_THRESHOLD = Rules.TRUST_MAX;
    public const int TRUST_TRUSTING_THRESHOLD = 12*WorldTime.TURNS_PER_HOUR;
    private const int LIVING_SCENT_DROP = OdorScent.MAX_STRENGTH;
    private const int UNDEAD_MASTER_SCENT_DROP = OdorScent.MAX_STRENGTH;

    // most/all of these FOV modifiers should space-time scale
    private const int MINIMAL_FOV = 2;
    private const int FOV_PENALTY_SUNSET = 1;
    private const int FOV_PENALTY_EVENING = 2;
    private const int FOV_PENALTY_MIDNIGHT = 3;
    private const int FOV_PENALTY_DEEP_NIGHT = 4;
    private const int FOV_PENALTY_SUNRISE = 2;
    private const int FOV_PENALTY_RAIN = 1;
    private const int FOV_PENALTY_HEAVY_RAIN = 2;
    private const int FOV_BONUS_STANDING_ON_OBJECT = 1;
    private const int MAX_LIGHT_FOV_BONUS = 2;   // XXX should be read from configuration files (lights)
    private const int MAX_BASE_VISION = 8;       // XXX should be read from configuration files (actors)
    public const int  MAX_VISION = MAX_BASE_VISION + FOV_BONUS_STANDING_ON_OBJECT - FOV_PENALTY_SUNSET + MAX_LIGHT_FOV_BONUS;   // should be calculated after configuration files read

    private const int FIRE_DISTANCE_VS_RANGE_MODIFIER = 2;
    private const float FIRING_WHEN_STA_TIRED = 0.75f;
    private const float FIRING_WHEN_STA_NOT_FULL = 0.9f;

    public static float SKILL_AWAKE_SLEEP_BONUS = 0.1f;
    public static float SKILL_AWAKE_SLEEP_REGEN_BONUS = 0.17f;    // XXX 0.17f makes this useful at L1
    public static int SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = 1;
    public static int SKILL_HAULER_INV_BONUS = 1;
    public static int SKILL_HIGH_STAMINA_STA_BONUS = 5;
    public static int SKILL_LEADERSHIP_FOLLOWER_BONUS = 1;
    public static float SKILL_LIGHT_EATER_FOOD_BONUS = 0.2f;
    public static float SKILL_LIGHT_EATER_MAXFOOD_BONUS = 0.15f;
    public static int SKILL_NECROLOGY_UNDEAD_BONUS = 2;
    public static int SKILL_STRONG_THROW_BONUS = 1;
    public static int SKILL_TOUGH_HP_BONUS = 3;
    public static float SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = 0.15f;
    public static int SKILL_ZTOUGH_HP_BONUS = 4;
    public static float SKILL_ZTRACKER_SMELL_BONUS = 0.1f;

    public static int SKILL_AGILE_ATK_BONUS = 2;
    public static int SKILL_BOWS_ATK_BONUS = 5;
    public static int SKILL_BOWS_DMG_BONUS = 2;
    public static int SKILL_FIREARMS_ATK_BONUS = 5;
    public static int SKILL_FIREARMS_DMG_BONUS = 2;
    public static int SKILL_MARTIAL_ARTS_ATK_BONUS = 3;
    public static int SKILL_MARTIAL_ARTS_DMG_BONUS = 1;
    public static int SKILL_STRONG_DMG_BONUS = 2;
    public static int SKILL_ZAGILE_ATK_BONUS = 1;
    public static float SKILL_ZLIGHT_EATER_FOOD_BONUS = 0.1f;
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

    public string UnmodifiedName { get { return m_Name; } }

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
        int playerDelta = 0;
        if (null != m_Controller) {
          if (IsPlayer) playerDelta -= 1;
          m_Controller.LeaveControl();
        }
        m_Controller = value;
        if (null != m_Controller) {
          m_Controller.TakeControl(this);
          if (IsPlayer) playerDelta += 1;
        }
        if (null != m_Location.Map && 0!=playerDelta) m_Location.Map.Players.Recalc();
      }
    }

    public bool IsPlayer { get { return m_Controller is PlayerController; } }
    public int SpawnTime { get { return m_SpawnTime; } }

    public Gameplay.GameGangs.IDs GangID {
      get {
        return m_GangID;
      }
      set {
        m_GangID = value;
      }
    }

    public bool IsInAGang { get { return m_GangID != 0; } }
    public Doll Doll { get { return m_Doll; } }

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

    public Inventory Inventory { get { return m_Inventory; } }

    public int HitPoints {
      get {
        return m_HitPoints;
      }
      set {
        m_HitPoints = value;
      }
    }

    public int PreviousHitPoints { get { return m_previousHitPoints; } }

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

    public int FoodPoints { get { return m_FoodPoints; } }
    public int PreviousFoodPoints { get { return m_previousFoodPoints; } }
    public int SleepPoints { get { return m_SleepPoints; } }
    public int PreviousSleepPoints { get { return m_previousSleepPoints; } }
    public int Sanity { get { return m_Sanity; } }
    public int PreviousSanity { get { return m_previousSanity; } }

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

    public int LastActionTurn { get { return m_LastActionTurn; } }

    public Location Location {
      get {
        return m_Location;
      }
      set {
        m_Location = value;
      }
    }

    public Activity Activity;

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

    public Attack CurrentMeleeAttack { get { return m_CurrentMeleeAttack; } }
    public Attack CurrentRangedAttack { get { return m_CurrentRangedAttack; } }
    public Defence CurrentDefence { get { return m_CurrentDefence; } }

    // Leadership
    public Actor Leader { get { return m_Leader; } }
    public Actor LiveLeader { get { return (null != m_Leader && !m_Leader.IsDead ? m_Leader : null); } }

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

    public bool IsTrustingLeader {
      get {
        return HasLeader && TRUST_TRUSTING_THRESHOLD <= TrustInLeader;
      }
    }

    public bool HasBondWith(Actor target)
    {
      if (Leader == target)      return TRUST_BOND_THRESHOLD <= TrustInLeader;
      if (target.Leader == this) return TRUST_BOND_THRESHOLD <= target.TrustInLeader;
      return false;
    }

    public IEnumerable<Actor> Followers { get { return m_Followers; } }

    public int CountFollowers {
      get {
        return m_Followers?.Count ?? 0;
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

    private string ReasonCantCancelLead(Actor target)
    {
#if DEBUG
      if (target == null) throw new ArgumentNullException(nameof(target));
#endif
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

    private string ReasonCantShout()
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

#if DEAD_FUNC
    public bool CanChatWith(Actor target)
    {
	  return string.IsNullOrEmpty(ReasonCantChatWith(target));
    }
#endif

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

#if DEAD_FUNC
    public IEnumerable<Actor> AggressorOf { get { return m_AggressorOf; } }
    public int CountAggressorOf { get { return m_AggressorOf?.Count ?? 0; } }
    public IEnumerable<Actor> SelfDefenceFrom { get { return m_SelfDefenceFrom; } }
    public int CountSelfDefenceFrom { get { return m_SelfDefenceFrom?.Count ?? 0; } }
#endif

    public int MurdersCounter {
      get {
        // Even if this were not an apocalypse, law enforcement should get some slack in interpreting intent, etc.
        int planning_to_murder = 0;
        if (IsHungry) planning_to_murder = m_AggressorOf?.Count(a=> null != a?.Inventory.GetItemsByType<ItemFood>()) ?? 0;
        return m_MurdersCounter+planning_to_murder;
      }
      set { // nominates chunk of RogueGame::KillActor as member function
        m_MurdersCounter = value;
      }
    }

    public int Infection { get { return m_Infection; } }
    public Corpse DraggedCorpse { get { return m_DraggedCorpse; } }

    public void Drag(Corpse c)
    {
      c.DraggedBy = this;
      m_DraggedCorpse = c;
    }

    public Corpse StopDraggingCorpse()
    {
      Corpse ret = m_DraggedCorpse;
      if (null != ret) {
        ret.DraggedBy = null;
        m_DraggedCorpse = null;
      }
      return ret;
    }

    public bool IsDebuggingTarget {
      get {
        if ("Lucy Lambert"==Name) return true;
        return false;
      }
    }

    public Actor(ActorModel model, Faction faction, int spawnTime, string name="", bool isProperName=false, bool isPluralName=false)
    {
#if DEBUG
      if (null == model) throw new ArgumentNullException(nameof(model));
      if (null == faction) throw new ArgumentNullException(nameof(faction));
#endif
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

    public void Retype(ActorModel model)
    {
      m_ModelID = model.ID;
    }

    private void OnModelSet()
    {
      ActorModel model = Model;
      m_Doll = new Doll(model);
      m_Sheet = new ActorSheet(model.StartingSheet);
      m_ActionPoints = m_Doll.Body.Speed;
      m_HitPoints = m_previousHitPoints = m_Sheet.BaseHitPoints;
      m_StaminaPoints = m_previousStamina = m_Sheet.BaseStaminaPoints;
      m_FoodPoints = m_previousFoodPoints = m_Sheet.BaseFoodPoints;
      m_SleepPoints = m_previousSleepPoints = m_Sheet.BaseSleepPoints;
      m_Sanity = m_previousSanity = m_Sheet.BaseSanity;
      m_Inventory = (model.Abilities.HasInventory ? new Inventory(model.StartingSheet.BaseInventoryCapacity)
                                                  : null); // any previous inventory will be irrevocably destroyed
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

    public int MaxThrowRange(int baseRange)
    {
      return baseRange + SKILL_STRONG_THROW_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG);
    }

    private string ReasonCouldntThrowTo(Point pos, List<Point> LoF)
    {
      if (LoF != null) LoF.Clear();
      ItemGrenade itemGrenade = GetEquippedWeapon() as ItemGrenade;
      ItemGrenadePrimed itemGrenadePrimed = GetEquippedWeapon() as ItemGrenadePrimed;
      if (itemGrenade == null && itemGrenadePrimed == null) return "no grenade equipped";

      ItemGrenadeModel itemGrenadeModel = itemGrenade == null ? itemGrenadePrimed.Model.GrenadeModel : itemGrenade.Model;
      int maxRange = MaxThrowRange(itemGrenadeModel.MaxThrowDistance);
      if (Rules.GridDistance(Location.Position, pos) > maxRange) return "out of throwing range";
      if (!LOS.CanTraceThrowLine(Location, pos, maxRange, LoF)) return "no line of throwing";
      return "";
    }

    public bool CanThrowTo(Point pos, out string reason, List<Point> LoF=null)
    {
      reason = ReasonCouldntThrowTo(pos,LoF);
      return string.IsNullOrEmpty(reason);
    }

#if DEAD_FUNC
    public bool CanThrowTo(Point pos, List<Point> LoF=null)
    {
      return string.IsNullOrEmpty(ReasonCouldntThrowTo(pos,LoF));
    }
#endif

    // strictly speaking, 1 step is allowed but we do not check LoF here
    private string ReasonCouldntFireAt(Actor target)
    {
      Contract.Requires(null != target);
      ItemRangedWeapon itemRangedWeapon = GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon == null) return "no ranged weapon equipped";
      if (CurrentRangedAttack.Range+1 < Rules.GridDistance(Location, target.Location)) return "out of range";
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
      Contract.Requires(null != target);
      if (range+1 < Rules.GridDistance(Location, target.Location)) return "out of range";
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
      Contract.Requires(null != target);
      LoF?.Clear();
      ItemRangedWeapon itemRangedWeapon = GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon == null) return "no ranged weapon equipped";
      if (CurrentRangedAttack.Range < Rules.GridDistance(Location, target.Location)) return "out of range";
      if (itemRangedWeapon.Ammo <= 0) return "no ammo left";
      if (!LOS.CanTraceFireLine(Location, target.Location, CurrentRangedAttack.Range, LoF)) return "no line of fire";
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
      Contract.Requires(null != target);
      LoF?.Clear();
      if (range < Rules.GridDistance(Location, target.Location)) return "out of range";
      if (!LOS.CanTraceFireLine(Location, target.Location, range, LoF)) return "no line of fire";
      if (target.IsDead) return "already dead!";
      return "";
    }

#if FAIL
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

    private string ReasonCantContrafactualFireAt(Actor target, Point p)
    {
      Contract.Requires(null != target);
      if (CurrentRangedAttack.Range < Rules.GridDistance(p, target.Location.Position)) return "out of range";
      if (!LOS.CanTraceHypotheticalFireLine(new Location(Location.Map,p), target.Location.Position, CurrentRangedAttack.Range, this)) return "no line of fire";
      return "";
    }

#if DEAD_FUNC
	public bool CanContrafactualFireAt(Actor target, Point p, out string reason)
	{
	  reason = ReasonCantContrafactualFireAt(target,p);
	  return string.IsNullOrEmpty(reason);
	}
#endif

	public bool CanContrafactualFireAt(Actor target, Point p)
	{
	  return string.IsNullOrEmpty(ReasonCantContrafactualFireAt(target, p));
	}

    public string ReasonCantMeleeAttack(Actor target)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      if (!Rules.IsAdjacent(Location, target.Location)) return "not adjacent";
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

    public Attack MeleeWeaponAttack(ItemMeleeWeaponModel model, Actor target = null)
    {
      Attack baseAttack = model.BaseMeleeAttack(Sheet);
      int num3 = Actor.SKILL_AGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.AGILE) + Actor.SKILL_ZAGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_AGILE);
      int num4 = Actor.SKILL_STRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG) + Actor.SKILL_ZSTRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_STRONG);
      if (target != null && target.Model.Abilities.IsUndead)
        num4 += DamageBonusVsUndeads;
      float num5 = (float)baseAttack.HitValue + (float) num3;
      if (IsExhausted) num5 /= 2f;
      else if (IsSleepy) num5 *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) num5, baseAttack.DamageValue + num4, baseAttack.StaminaPenalty);
    }

    public Attack UnarmedMeleeAttack(Actor target=null)
    {
      int num3 = Actor.SKILL_AGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.AGILE) + Actor.SKILL_ZAGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_AGILE);
      int num4 = Actor.SKILL_STRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG) + Actor.SKILL_ZSTRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_STRONG);
      num3 += Actor.SKILL_MARTIAL_ARTS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
      num4 += Actor.SKILL_MARTIAL_ARTS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS);
      if (target != null && target.Model.Abilities.IsUndead)
        num4 += DamageBonusVsUndeads;
      Attack baseAttack = Model.StartingSheet.UnarmedAttack;
      float num5 = (float)baseAttack.HitValue + (float) num3;
      if (IsExhausted) num5 /= 2f;
      else if (IsSleepy) num5 *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) num5, baseAttack.DamageValue + num4, baseAttack.StaminaPenalty);
    }

    // does not properly account for martial arts
    public ItemMeleeWeapon GetBestMeleeWeapon()
    {
      if (Inventory == null) return null;
      List<ItemMeleeWeapon> tmp = Inventory.GetItemsByType<ItemMeleeWeapon>();
      if (null == tmp) return null;
      int martial_arts_rating = UnarmedMeleeAttack().Rating;
      int num1 = 0;
      ItemMeleeWeapon itemMeleeWeapon1 = null;
      foreach (ItemMeleeWeapon obj in tmp) {
        int num2 = MeleeWeaponAttack(obj.Model).Rating;    // XXX should disallow martial arts bonus
        if (num2 <= martial_arts_rating || num2 <= num1) continue;
        num1 = num2;
        itemMeleeWeapon1 = obj;
      }
      return itemMeleeWeapon1;
    }

    // ultimately these two will be thin wrappers, as CurrentMeleeAttack/CurrentRangedAttack are themselves mathematical functions
    // of the equipped weapon which OrderableAI *will* want to vary when choosing an appropriate weapon
    public Attack MeleeAttack(Actor target = null) { return HypotheticalMeleeAttack(CurrentMeleeAttack, target); }

    public Attack BestMeleeAttack(Actor target = null)
    {
      ItemMeleeWeapon tmp_melee = GetBestMeleeWeapon();
      Attack base_melee_attack = (null!=tmp_melee ? tmp_melee.Model.BaseMeleeAttack(Sheet) : CurrentMeleeAttack);
      return HypotheticalMeleeAttack(base_melee_attack, target);
    }

    public Attack HypotheticalRangedAttack(Attack baseAttack, int distance, Actor target = null)
    {
      int num1 = 0;
      int num2 = 0;
      switch (baseAttack.Kind)
      {
        case AttackKind.FIREARM:
          num1 = SKILL_FIREARMS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.FIREARMS);
          num2 = SKILL_FIREARMS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.FIREARMS);
          break;
        case AttackKind.BOW:
          num1 = SKILL_BOWS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.BOWS);
          num2 = SKILL_BOWS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Skills.IDs.BOWS);
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

    public Attack RangedAttack(int distance, Actor target = null)
    {
      return HypotheticalRangedAttack(CurrentRangedAttack, distance, target);
    }

    public bool HasActiveCellPhone {
      get {
        return null != Inventory?.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader && !it.IsUseless);
      }
    }

    public bool HasCellPhone {
      get {
        return null != Inventory?.GetFirstMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless);
      }
    }


    public bool HasActivePoliceRadio {
      get {
        if ((int)Gameplay.GameFactions.IDs.ThePolice==m_FactionID) return true;
        return null != Inventory?.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackPolice);  // charges on walking so won't stay useless
      }
    }

    public bool HasPoliceRadio {
      get {
        if ((int)Gameplay.GameFactions.IDs.ThePolice==m_FactionID) return true;
        return null != Inventory?.GetFirstMatching<ItemTracker>(it => it.CanTrackPolice);  // charges on walking so won't stay useless
      }
    }

    // For now, entirely implicit.  It's also CHAR technology so recharges like a police radio.
    public bool HasActiveArmyRadio {
      get {
        if ((int)Gameplay.GameFactions.IDs.TheArmy==m_FactionID) return true;
        return false;
      }
    }

    public bool HasArmyRadio {
      get {
        if ((int)Gameplay.GameFactions.IDs.TheArmy==m_FactionID) return true;
        return false;
      }
    }

    public bool NeedActivePoliceRadio {
      get {
        if ((int)Gameplay.GameFactions.IDs.ThePolice==m_FactionID) return false; // implicit
        // XXX disallow murderers under certain conditions, etc
        Actor leader = LiveLeader;
        if (null != leader) return leader.HasActivePoliceRadio;
        if (0 >= CountFollowers) return false;
        foreach(Actor fo in Followers) {
          if (fo.HasPoliceRadio) return true;
        }
        return false;
      }
    }

    public bool NeedActiveCellPhone {
      get {
        Actor leader = LiveLeader;
        if (null != leader) {
          return leader.HasActiveCellPhone;
        }
        if (0 < CountFollowers) {
          foreach(Actor fo in Followers) {
            if (fo.HasCellPhone) return true;
          }
        }
        return false;
      }
    }

    public bool WantPoliceRadio {
      get {
        if ((int)Gameplay.GameFactions.IDs.ThePolice == m_FactionID) return false; // police have implicit police radios
        bool have_cellphone = HasCellPhone;
        bool have_army = HasArmyRadio;
        if (!have_cellphone && !have_army) return true;
        Actor leader = LiveLeader;
        if (null != leader) {
          if (have_cellphone && leader.HasCellPhone) return false;
          if (have_army && leader.HasArmyRadio) return false;
          return true;
        }
        if (0 < CountFollowers) {
          foreach(Actor fo in Followers) {
            if (have_cellphone && fo.HasCellPhone) continue;
            if (have_army && fo.HasArmyRadio) continue;
            return true;
          }
        }
        return false;
      }
    }

    public bool WantCellPhone {
      get {
        bool have_police = HasPoliceRadio;
        bool have_army = HasArmyRadio;
        if (!have_police && !have_army) return true;
        Actor leader = LiveLeader;
        if (null != leader) {
          if (have_police && leader.HasPoliceRadio) return false;
          if (have_army && leader.HasArmyRadio) return false;
          return true;
        }
        if (0 < CountFollowers) {
          foreach(Actor fo in Followers) {
            if (have_police && fo.HasPoliceRadio) continue;
            if (have_army && fo.HasArmyRadio) continue;
            return true;
          }
        }
        return false;
      }
    }

    public ItemTracker GetEquippedCellPhone()
    {
      return Inventory?.GetFirstMatching<ItemTracker>(it => it.IsEquipped && it.CanTrackFollowersOrLeader);
    }

    public bool MessagePlayerOnce(Action<Actor> fn, Func<Actor, bool> pred =null)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
      if (IsPlayer && !IsDead && (null == pred || pred(this))) {
        fn(this);
        return true;
      }
      if (Location.Map.MessagePlayerOnce(fn,pred)) return true;
      return Location.Map.District.MessagePlayerOnce(Location.Map,fn,pred);
    }

    public void MessageAllInDistrictByRadio(Action<Actor> op, Func<Actor, bool> test, Action<Actor> msg_player, Func<Actor, bool> msg_player_test, Location? origin=null)
    {
#if DEBUG
      if (null == op) throw new ArgumentNullException(nameof(op));
      if (null == test) throw new ArgumentNullException(nameof(test));
      if (null == msg_player) throw new ArgumentNullException(nameof(msg_player));
      if (null == msg_player_test) throw new ArgumentNullException(nameof(msg_player_test));
#endif
      bool police_radio = HasActivePoliceRadio;
      bool army_radio = HasActiveArmyRadio;
      if (!police_radio && !army_radio) return;
#if DEBUG
      if (police_radio && army_radio) throw new InvalidOperationException("need to implement dual police and army radio case");
#endif
      if (null == origin) origin = Location;
      foreach (Map map in origin.Value.Map.District.Maps) {
        foreach (Actor actor in map.Actors) {
          if (this == actor) continue;
          // XXX defer implementing dual radios
          if (police_radio) {
            if (!actor.HasActivePoliceRadio) continue;
          } else {
            if (!actor.HasActiveArmyRadio) continue;
          }
          if (actor.IsSleeping) continue;   // can't hear when sleeping (this is debatable; might be interesting to be woken up by high-priority messages once radio alarms are implemented)

          if (actor.IsPlayer && msg_player_test(actor)) {
            RogueForm.Game.PanViewportTo(actor);
            msg_player(actor);
          }

          // use cases.
          // aggressing all faction in district: civilian/survivor cannot initiate, and have no obligation to respond if they get the message
          // reporting z: civilian can initiate and respond (but threat tracking needed to respond)
          if (test(actor)) op(actor);
        }
      }
    }

    public Actor Sees(Actor a)
    {
      if (null == a) return null;
      if (this == a) return null;
      if (a.IsDead) return null;
      return (Controller.IsVisibleTo(a) ? a : null);  // inline IsVisibleToPlayer here, for generality
    }

    // leadership/follower handling
    public void AddFollower(Actor other)
    {
      Contract.Requires(null != other);
      if (m_Followers != null && m_Followers.Contains(other)) throw new ArgumentException("other is already a follower");
      if (m_Followers == null) m_Followers = new List<Actor>(1);
      m_Followers.Add(other);
      if (other.Leader != null) other.Leader.RemoveFollower(other);
      other.m_Leader = this;
    }

    public void RemoveFollower(Actor other)
    {
      Contract.Requires(null != other);
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
      if (m_TrustDict.TryGetValue(other,out int trust)) return trust;
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

    public IEnumerable<Actor> Aggressing { get { return null != m_AggressorOf ? m_AggressorOf : new List<Actor>(); } }
    public IEnumerable<Actor> Aggressors { get { return null != m_SelfDefenceFrom ? m_SelfDefenceFrom : new List<Actor>(); } }

    public void MarkAsAgressorOf(Actor other)
    {
      if (other == null || other.IsDead) return;
      if (m_AggressorOf == null) m_AggressorOf = new List<Actor>{ other };
      else if (m_AggressorOf.Contains(other)) return;
      else m_AggressorOf.Add(other);
      Threats?.RecordTaint(other, other.Location);
    }

    public void MarkAsSelfDefenceFrom(Actor other)
    {
      if (other == null || other.IsDead) return;
      if (m_SelfDefenceFrom == null) m_SelfDefenceFrom = new List<Actor>{ other };
      else if (m_SelfDefenceFrom.Contains(other)) return;
      else m_SelfDefenceFrom.Add(other);
      Threats?.RecordTaint(other, other.Location);
    }

    public bool IsAggressorOf(Actor other)
    {
      return m_AggressorOf?.Contains(other) ?? false;
    }

    public bool IsSelfDefenceFrom(Actor other)
    {
      return m_SelfDefenceFrom?.Contains(other) ?? false;
    }

    public void RemoveAggressorOf(Actor other)
    {
      if (m_AggressorOf == null) return;
      m_AggressorOf.Remove(other);
      if (0 >= m_AggressorOf.Count) m_AggressorOf = null;
    }

    public void RemoveSelfDefenceFrom(Actor other)
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

    public bool IsEnemyOf(Actor target)
    {
      if (null == target) return false;
      if (Faction.IsEnemyOf(target.Faction)) return true;
      if (Faction == target.Faction && IsInAGang && target.IsInAGang && GangID != target.GangID) return true;
      if (AreDirectEnemies(target)) return true;
      return AreIndirectEnemies(target);
    }


    public bool AreDirectEnemies(Actor other)
    {
      if (other == null || other.IsDead) return false;
      return (m_AggressorOf?.Contains(other) ?? false) || (m_SelfDefenceFrom?.Contains(other) ?? false) || other.IsAggressorOf(this) || other.IsSelfDefenceFrom(this);
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

    // not just our FoV.
    public List<Actor> GetEnemiesInFov(HashSet<Point> fov)
    {
#if DEBUG
      if (null == fov) throw new ArgumentNullException(nameof(fov));
#endif
      if (1 >= fov.Count) return null;  // sleeping?
      List<Actor> actorList = new List<Actor>(fov.Count-1); // assuming ok to thrash GC
      foreach (Point position in fov) {
        Actor actorAt = Location.Map.GetActorAtExt(position.X,position.Y);
        if (actorAt != null && actorAt != this && IsEnemyOf(actorAt)) {
          actorList.Add(actorAt);
        }
      }
      if (2 <= actorList.Count) {
        actorList.Sort((Comparison<Actor>) ((a, b) =>
        {
          double num1 = Rules.StdDistance(a.Location, Location);
          double num2 = Rules.StdDistance(b.Location, Location);
          return num1.CompareTo(num2);
        }));
      }
      return (1<=actorList.Count ? actorList : null);
    }

    // We do not handle the enemy relations here.
    public HashSet<Actor> Allies {
      get {
        HashSet<Actor> ret = new HashSet<Actor>();
        // 1) police have all other police as allies.
        if ((int)Gameplay.GameFactions.IDs.ThePolice == Faction.ID) {
          foreach(Map m in Location.Map.District.Maps) ret.UnionWith(m.Police.Get);
          ret.Remove(this);
        }
        // 2) leader/follower cliques are allies.
        if (0 < CountFollowers) ret.UnionWith(m_Followers);
        else if (HasLeader) {
          ret.Add(Leader);
          ret.UnionWith(Leader.Followers);
          ret.Remove(this);
        }
        return (0<ret.Count ? ret : null);
      }
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
      foreach (Actor actor1 in Location.Map.Actors) {
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
        return Location.Map.GetTileAt(Location.Position).IsInside;
      }
    }

    public List<Point> FastestStepTo(Map m,Point src,Point dest)
    {
      int dist = Rules.GridDistance(src,dest);
      if (1==dist) return new List<Point>{ dest };
      IEnumerable<Point> tmp = Direction.COMPASS.Select(dir=>src+dir).Where(pt=> dist>Rules.GridDistance(pt,dest) && m.IsWalkableFor(pt,this));
      return tmp.Any() ? tmp.ToList() : null;
    }

    public List<List<Point> > MinStepPathTo(Location origin, Location target)
    {
      if (origin==target) return null;

      if (origin.Map!=target.Map) {
        Location? test = origin.Map.Denormalize(target);
        if (null == test) return null;
        target = test.Value;
      }

      Map m = origin.Map;
      Point src = origin.Position;
      Point dest = target.Position;
      List<List<Point> > ret = new List<List<Point> >();
      List<Point> tmp = FastestStepTo(m,src,dest);
      if (null == tmp) return null;
      ret.Add(tmp);
      while(!tmp.Contains(dest)) {
        int dist = Rules.GridDistance(dest,tmp[0]);
        if (1==dist) {
          tmp = new List<Point>{dest};
        } else {
          HashSet<Point> tmp2 = new HashSet<Point>();
          foreach(Point pt in tmp) {
            List<Point> tmp3 = FastestStepTo(m,pt,dest);
            if (null == tmp3) continue;
            tmp2.UnionWith(tmp3);
          }
          if (0 >= tmp2.Count) return null;
          tmp = tmp2.ToList();
        }
        ret.Add(tmp);
      }
      return ret;
    }

    public List<Point> OneStepRange(Map m,Point p)
    {
      IEnumerable<Point> tmp = Direction.COMPASS.Select(dir=>p+dir).Where(pt=>m.IsWalkableFor(pt,this));
      return tmp.Any() ? tmp.ToList() : null;
    }

    public Dictionary<Location,ActorAction> OnePathRange(Location loc)
    {
      var ret = new Dictionary<Location,ActorAction>();
      foreach(Direction dir in Direction.COMPASS) {
        Location test = loc+dir;
        ActorAction tmp = Rules.IsPathableFor(this,test);
        if (null == tmp) continue;
        if (!test.Map.IsInBounds(test.Position)) {
          Location? test2 = test.Map.Normalize(test.Position);
          if (null == test2) throw new ArgumentNullException(nameof(test2));
          test = test2.Value;
        }
        ret[test] = tmp;
      }
      Exit exit = Model.Abilities.AI_CanUseAIExits ? loc.Exit : null;
      if (null != exit) {
        ActionUseExit tmp = new ActionUseExit(this, loc.Position);
        if (loc == Location) {
          if (tmp.IsLegal()) ret[exit.Location] = tmp;
        } else {
          ret[exit.Location] = tmp;
          // simulate Exit::ReasonIsBlocked
          MapObject mapObjectAt = exit.Location.MapObject;
          if (   null != mapObjectAt
              && !mapObjectAt.IsCouch) {
            if (!mapObjectAt.IsJumpable || !CanJump) ret.Remove(exit.Location);
          }
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public List<Point> OnePathRange(Map m, Point p)
    {
      IEnumerable<Point> tmp = Direction.COMPASS.Select(dir=>p+dir).Where(pt=>null!=Rules.IsPathableFor(this,new Location(m,pt)));
      return tmp.Any() ? tmp.ToList() : null;
    }

    public Dictionary<Location,ActorAction> OnePath(Location loc, Dictionary<Location, ActorAction> already)
    {
      var ret = new Dictionary<Location, ActorAction>(9);
      foreach(Direction dir in Direction.COMPASS) {
        Location dest = loc+dir;
        if (!dest.Map.IsInBounds(dest.Position)) {
          Location? test = dest.Map.Normalize(dest.Position);
          if (null == test) continue;
          dest = test.Value;
        }
        if (already.ContainsKey(dest)) {
          ret[dest] = already[dest];
          continue;
        }
        if (Location==dest) {
          ret[dest] = new Engine.Actions.ActionMoveStep(this, dest.Position);
          continue;
        }
        ActorAction tmp = Rules.IsPathableFor(this, dest);
        if (null == tmp) {
          if (dest.Map.GetMapObjectAt(dest.Position)?.IsContainer ?? false) tmp = new Engine.Actions.ActionMoveStep(this, dest.Position); // XXX wrong no matter what
        }
        if (null != tmp) ret[dest] = tmp;
      }
      Exit exit = Model.Abilities.AI_CanUseAIExits ? loc.Exit : null;
      if (null != exit) {
        ret[exit.Location] = new ActionUseExit(this, loc.Position);
        // simulate Exit::ReasonIsBlocked
        MapObject mapObjectAt = exit.Location.MapObject;
        if (   null != mapObjectAt
            && !mapObjectAt.IsCouch) {
          if (!mapObjectAt.IsJumpable || !CanJump) ret.Remove(exit.Location);
        }
      }
      return ret;
    }

    public Dictionary<Point,ActorAction> OnePath(Map m, Point p, Dictionary<Point, ActorAction> already)
    {
      var ret = new Dictionary<Point, ActorAction>(9);
      foreach(Direction dir in Direction.COMPASS) {
        Point pt = p+dir;
        if (already.ContainsKey(pt)) {
          ret[pt] = already[pt];
          continue;
        }
        if (Location==(new Location(m,pt))) {
          ret[pt] = new Engine.Actions.ActionMoveStep(this, pt);
          continue;
        }
        ActorAction tmp = Rules.IsPathableFor(this, new Location(m, pt));
        if (null == tmp) {
          if (m.GetMapObjectAt(pt)?.IsContainer ?? false) tmp = new Engine.Actions.ActionMoveStep(this, pt); // XXX wrong no matter what
        }
        if (null != tmp) ret[pt] = tmp;
      }
      return ret;
    }

    public List<Point> LegalSteps { get { return OneStepRange(Location.Map, Location.Position);  } }

    public HashSet<Point> NextStepRange(Map m,HashSet<Point> past, IEnumerable<Point> now)
    {
      Contract.Requires(0<now.Count());
      HashSet<Point> ret = new HashSet<Point>();
      foreach(Point pt in now) {
        List<Point> tmp = OneStepRange(m,pt);
        if (null == tmp) continue;
        HashSet<Point> tmp2 = new HashSet<Point>(tmp);
        tmp2.ExceptWith(past);
        tmp2.ExceptWith(now);
        ret.UnionWith(tmp2);
      }
      return (0<ret.Count ? ret : null);
    }

    private string ReasonCantBreak(MapObject mapObj)
    {
#if DEBUG
      if (null == mapObj) throw new ArgumentNullException(nameof(mapObj));
#endif
      if (!Model.Abilities.CanBreakObjects) return "cannot break objects";
      if (IsTired) return "tired";
      bool flag = (mapObj as DoorWindow)?.IsBarricaded ?? false;
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

    // currently other is unused, but that is not an invariant
    private string ReasonCantShove(Actor other)
    {
      Contract.Requires(null != other);
      if (!AbleToPush) return "cannot shove people";
      if (IsTired) return "tired";
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

    private string ReasonCantBeShovedTo(Point toPos)
    {
      Map map = Location.Map;
      if (!map.IsInBounds(toPos)) return "out of map";  // XXX should be IsValid
      if (!map.GetTileModelAt(toPos).IsWalkable) return "blocked"; // XXX should be GetTileModelAtExt
      MapObject mapObjectAt = map.GetMapObjectAt(toPos);
      if (mapObjectAt != null && !mapObjectAt.IsWalkable) return "blocked by an object";
      if (map.HasActorAt(toPos)) return "blocked by someone";
      return "";
    }

    public bool CanBeShovedTo(Point toPos, out string reason)
    {
      reason = ReasonCantBeShovedTo(toPos);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBeShovedTo(Point toPos)
    {
      return string.IsNullOrEmpty(ReasonCantBeShovedTo(toPos));
    }

    public Dictionary<Point, Direction> ShoveDestinations {
      get {
         return Location.Map.ValidDirections(Location.Position, (m, pt) => CanBeShovedTo(pt));
      }
    }

    private string ReasonCantClose(DoorWindow door)
    {
#if DEBUG
      if (null == door) throw new ArgumentNullException(nameof(door));
#endif
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
      if (Inventory == null || Inventory.IsEmpty) return "no items";
      if (!Inventory.Has<ItemBarricadeMaterial>()) return "no barricading material";
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
#if DEBUG
      if (null == door) throw new ArgumentNullException(nameof(door));
#endif
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

#if DEAD_FUNC
    public bool CanBash(DoorWindow door)
    {
	  return string.IsNullOrEmpty(ReasonCantBash(door));
    }
#endif

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

    public int BarricadingMaterialNeedForFortification(bool isLarge)
    {
      return Math.Max(1, (isLarge ? 4 : 2) - (Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) >= 3 ? SKILL_CARPENTRY_LEVEL3_BUILD_BONUS : 0));
    }

    private string ReasonCantBuildFortification(Point pos, bool isLarge)
    {
      if (0 >= Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY)) return "no skill in carpentry";

      Map map = Location.Map;
      if (!map.GetTileModelAtExt(pos).IsWalkable) return  "cannot build on walls";

      int num = BarricadingMaterialNeedForFortification(isLarge);
      if (CountItems<ItemBarricadeMaterial>() < num) return string.Format("not enough barricading material, need {0}.", (object) num);
      if (map.HasMapObjectAt(pos) || map.HasActorAt(pos)) return "blocked";
      return "";
    }

    public bool CanBuildFortification(Point pos, bool isLarge, out string reason)
    {
	  reason = ReasonCantBuildFortification(pos,isLarge);
	  return string.IsNullOrEmpty(reason);
    }

    public bool CanBuildFortification(Point pos, bool isLarge)
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

#if DEAD_FUNC
    public bool CanSwitch(PowerGenerator powGen)
    {
	  return string.IsNullOrEmpty(ReasonCantSwitch(powGen));
    }
#endif

	private string ReasonCantRecharge(Item it)
	{
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
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

#if DEAD_FUNC
    public bool CanRecharge(Item it)
    {
	  return string.IsNullOrEmpty(ReasonCantRecharge(it));
    }
#endif

    // event timing
    public void SpendActionPoints(int actionCost)
    {
      m_ActionPoints -= actionCost;
      m_LastActionTurn = Location.Map.LocalTime.TurnCounter;
    }

    public void Wait()
    {
      m_ActionPoints = 0;
      m_LastActionTurn = Location.Map.LocalTime.TurnCounter;
    }

    public bool CanActThisTurn {
      get {
        return 0 < m_ActionPoints;
      }
    }

    public bool CanActNextTurn {
      get {
        if (CanActThisTurn) return 0 < m_ActionPoints + Speed - Rules.BASE_ACTION_COST;
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
        if (GetEquippedItem(DollPart.TORSO) is Engine.Items.ItemBodyArmor armor) num -= armor.Weight;
        if (DraggedCorpse != null) num /= 2f;
        return Math.Max((int) num, 0);
      }
    }

    // n is the number of our actions
    public int HowManyTimesOtherActs(int n,Actor other)
    {   // n=1:
      Contract.Requires(1<=n);
      int my_ap = m_ActionPoints;
      int my_actions = 0;
      while(0 < my_ap) { // assuming this never gets very large
        my_ap -= Rules.BASE_ACTION_COST;
        my_actions++;
      }
      if (my_actions>n) return 0;
      int other_ap = other.ActionPoints+(IsBefore(other) ? 0 : other.Speed);
      int other_actions = 0;
      while(0 < other_ap) { // assuming this never gets very large
        other_ap -= Rules.BASE_ACTION_COST;
        other_actions++;
      }
      if (my_actions==n) return other_actions;
      while(my_actions<n) {
        my_ap += Speed;
        while(0 < my_ap) { // assuming this never gets very large
          my_ap -= Rules.BASE_ACTION_COST;
          my_actions++;
        }
        if (my_actions>n) break;
        other_ap += other.Speed;
        while(0 < other_ap) { // assuming this never gets very large
          other_ap -= Rules.BASE_ACTION_COST;
          other_actions++;
        }
      }
      return other_actions;
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

    public int RunningStaminaCost(Point dest)
    {
      MapObject mapObjectAt = Location.Map.GetMapObjectAt(dest);
      if (mapObjectAt != null && !mapObjectAt.IsWalkable && mapObjectAt.IsJumpable) return Rules.STAMINA_COST_RUNNING+Rules.STAMINA_COST_JUMP+NightSTApenalty;
      return Rules.STAMINA_COST_RUNNING;
    }

#if DEAD_FUNC
    public bool WillTireAfterRunning(Point dest)
    {
      return WillTireAfter(RunningStaminaCost(dest));
    }
#endif

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
      if (!Location.Map.HasExitAt(exitPoint)) return "no exit there";
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
	private string ReasonCantLeaveMap(Point dest)
	{
#if XDISTRICT_PATHING
      Exit exitAt = Location.Map.GetExitAt(dest);
      if (null == exitAt) return "no exit to leave map with";
      return exitAt.ReasonIsBlocked(this);
#else
      if (!IsPlayer) return "can't leave maps";
      return "";
#endif
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

    public int HoursUntilHungry {
      get {
        int num = FoodPoints - FOOD_HUNGRY_LEVEL;
        return (0 >= num ? 0 : num / WorldTime.TURNS_PER_HOUR);
      }
    }

    public bool IsAlmostHungry {
      get {
        return Model.Abilities.HasToEat && HoursUntilHungry <= 3;
      }
    }

    public int HoursUntilRotHungry {
      get {
        int num = FoodPoints - ROT_HUNGRY_LEVEL;
        return (0 >= num ? 0 : num / WorldTime.TURNS_PER_HOUR);
      }
    }

    public bool IsAlmostRotHungry  {
      get {
        return Model.Abilities.IsRotting && HoursUntilRotHungry <= 3;
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
      int num2 = (int) ((double) num1 * (double) SKILL_AWAKE_SLEEP_REGEN_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Skills.IDs.AWAKE));
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

    public bool IsAlmostSleepy {
      get {
        if (!Model.Abilities.HasToSleep) return false;
        return 3 >= HoursUntilSleepy;
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

    private string ReasonCantSleep()
    {
      if (IsSleeping) return "already sleeping";
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

    public _T_ GetFirstMatching<_T_>() where _T_ : Item // needed as null vacuum
    {
      return m_Inventory?.GetFirstMatching<_T_>();
    }

    public _T_ GetFirstMatching<_T_>(Predicate<_T_> fn) where _T_ : Item
    {
      return m_Inventory?.GetFirstMatching<_T_>(fn);
    }

    public bool HasItemOfModel(ItemModel model)
    {
      return m_Inventory?.HasModel(model) ?? false;
    }

    public int Count(ItemModel model)
    {
      return m_Inventory?.Count(model) ?? 0;
    }

    public int CountItemsQuantityOfModel(ItemModel model)
    {
      return m_Inventory?.CountQuantityOf(model) ?? 0;
    }

    public int CountQuantityOf<_T_>() where _T_ : Item
    {
      return m_Inventory?.CountQuantityOf<_T_>() ?? 0;
    }

    public int CountItemsOfSameType(Type tt)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
      int num = 0;
      foreach (Item obj in m_Inventory.Items) {
        if (obj.IsUseless) continue;
        if (obj.GetType() == tt) ++num;
      }
      return num;
    }

    public int CountItems<_T_>() where _T_ : Item
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return 0;
      return m_Inventory.Items.Where(it=>it is _T_).Select(it => it.Quantity).Sum();
    }

    public bool Has<_T_>() where _T_ : Item
    {
      return m_Inventory?.Has<_T_>() ?? false;
    }

    public bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable)
        return CountItemsQuantityOfModel(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
    }

    public bool HasAtLeastFullStackOf(Item it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable)
        return CountItemsQuantityOfModel(it.Model) >= n * it.Model.StackingLimit;
      return m_Inventory.Count(it.Model) >= n;
    }

    public bool HasAtLeastFullStackOfItemModel(Item it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable)
        return CountItemsQuantityOfModel(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
    }

    public bool HasAtLeastFullStackOf(Gameplay.GameItems.IDs it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      ItemModel model = Models.Items[(int)it];
      if (model.IsStackable) return CountItemsQuantityOfModel(model) >= n * model.StackingLimit;
      return Count(model) >= n; // XXX assumes each model goes with a specific item type
    }

    public ItemMeleeWeapon GetWorstMeleeWeapon()
    {
      var melee = Inventory?.GetItemsByType<ItemMeleeWeapon>();
      if (null == melee) return null;
      if (1 == melee.Count) return melee[0];
      return melee.Where(w=> !w.IsEquipped).Minimize(w=>w.Model.Attack.Rating);
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
    private ItemRangedWeapon GetCompatibleRangedWeapon(ItemAmmoModel am)
    {
#if DEBUG
      if (null == am) throw new ArgumentNullException(nameof(am));
      if (null == m_Inventory) throw new ArgumentNullException(nameof(m_Inventory));
#endif
      ItemRangedWeapon rw = m_Inventory.GetFirst<ItemRangedWeapon>(it => it.AmmoType == am.AmmoType && it.Ammo < it.Model.MaxAmmo);
      if (null != rw) return rw;
      return m_Inventory.GetFirst<ItemRangedWeapon>(it => it.AmmoType == am.AmmoType);
    }

    public ItemRangedWeapon GetCompatibleRangedWeapon(ItemAmmo am)
    {
      return GetCompatibleRangedWeapon(am.Model);
    }

    public ItemRangedWeapon GetCompatibleRangedWeapon(Gameplay.GameItems.IDs am)
    {
      return GetCompatibleRangedWeapon(Models.Items[(int)am] as ItemAmmoModel);
    }

    public bool HasEnoughFoodFor(int nutritionNeed, ItemFood exclude=null)
    {
      if (!Model.Abilities.HasToEat) return true;
      if (null == Inventory || Inventory.IsEmpty) return false;
      List<ItemFood> tmp = Inventory.GetItemsByType<ItemFood>();
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
      int num = (int) ((double) baseValue * (double) SKILL_LIGHT_EATER_FOOD_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Skills.IDs.LIGHT_EATER));
      return baseValue + num;
    }

    public int BiteNutritionValue(int baseValue)
    {
      return (int) (10.0 + (double) (SKILL_ZLIGHT_EATER_FOOD_BONUS * (float) Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_LIGHT_EATER)) + (double) (SKILL_LIGHT_EATER_FOOD_BONUS * (float) Sheet.SkillTable.GetSkillLevel(Skills.IDs.LIGHT_EATER))) * baseValue;
    }

    public int CurrentNutritionOf(ItemFood food)
    {
      return ItemNutritionValue(food.NutritionAt(Location.Map.LocalTime.TurnCounter));
    }

    // prevents sinking IsInterestingTradeItem and IsTradeableItem below ActorController (these must work for both OrderableAI and PlayerController)
    public List<Item> GetInterestingTradeableItems(Actor buyer) // called from RogueGame::PickItemToTrade so forced to be public no matter where
    {
#if DEBUG
      if (!Model.Abilities.CanTrade) throw new InvalidOperationException("cannot trade");
      if (!buyer.Model.Abilities.CanTrade) throw new InvalidOperationException("cannot trade");
#endif

#if OBSOLETE
#else
      if (buyer.IsPlayer && IsPlayer) return Inventory.Items.ToList();
#endif

      // IsInterestingTradeItem includes a charisma check i.e. RNG invocation, so cannot use .Any() prescreen safely
      List<Item> objList = Inventory.Items.Where(it=> (buyer.Controller as Gameplay.AI.ObjectiveAI).IsInterestingTradeItem(this, it) && (Controller as Gameplay.AI.OrderableAI).IsTradeableItem(it)).ToList();
      return 0<objList.Count ? objList : null;
    }

    public List<Item> GetRationalTradeableItems(Gameplay.AI.OrderableAI buyer)    // only called from AI trading decision making
    {
#if DEBUG
      if (null == buyer) throw new ArgumentNullException(nameof(buyer));
      if (!Model.Abilities.CanTrade) throw new InvalidOperationException(Name+" cannot trade");
#endif

//    if (buyer.IsPlayer) return Inventory.Items

      IEnumerable<Item> objList = Inventory.Items.Where(it=> buyer.IsRationalTradeItem(this, it) && (Controller as Gameplay.AI.OrderableAI).IsTradeableItem(it));
      return objList.Any() ? objList.ToList() : null;
    }

    // equipped items
    public Item GetEquippedItem(DollPart part)
    {
      if (null == m_Inventory || DollPart.NONE == part) return null;
      return m_Inventory.Items.FirstOrDefault(obj => obj.EquippedPart == part);
    }

    public Item GetEquippedItem(Gameplay.GameItems.IDs id)
    {
      if (null == m_Inventory) return null;
      return m_Inventory.Items.FirstOrDefault(obj => obj.Model.ID == id && DollPart.NONE != obj.EquippedPart);
    }

    // this cannot be side-effecting (martial arts, grenades)
    public Item GetEquippedWeapon()
    {
      return GetEquippedItem(DollPart.RIGHT_HAND);
    }

    public bool HasEquipedRangedWeapon()
    {
      return GetEquippedWeapon() is ItemRangedWeapon;
    }

    // maybe this should be over on the Inventory object
    public Item GetItem(Gameplay.GameItems.IDs id)
    {
      if (null == m_Inventory) return null;
      return m_Inventory.Items.FirstOrDefault(obj => obj.Model.ID == id);
    }

    private string ReasonCantTradeWith(Actor target)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
#if OBSOLETE
      if (target.IsPlayer) return "target is player";
#else
      if (!IsPlayer && target.IsPlayer) return "target is player";
#endif
      if (!Model.Abilities.CanTrade && target.Leader != this) return "can't trade";
      if (!target.Model.Abilities.CanTrade && target.Leader != this) return "target can't trade";
      if (IsEnemyOf(target)) return "is an enemy";
      if (target.IsSleeping) return "is sleeping";
      if (Inventory == null || Inventory.IsEmpty) return "nothing to offer";
      if (target.Inventory == null || target.Inventory.IsEmpty) return "has nothing to trade";
#if OBSOLETE
      if (!IsPlayer) {
#else
      if (!IsPlayer && !target.IsPlayer) {
#endif
        List<Item> theirs = target.GetRationalTradeableItems(this.Controller as Gameplay.AI.OrderableAI);
        if (null == theirs) return "target unwilling to trade";
        List<Item> mine = GetRationalTradeableItems(target.Controller as Gameplay.AI.OrderableAI);
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
        if (itemRangedWeapon.Ammo >= itemRangedWeapon.Model.MaxAmmo) return "weapon already fully loaded";
      }
      else if (it is ItemSprayScent)
      {
        if (it.IsUseless) return "no spray left.";
      }
      else if (it is ItemTrap)
      {
        if (!(it as ItemTrap).Model.UseToActivate) return "does not activate manually";
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
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
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

    public bool MayTakeFromStackAt(Location loc)
    {
      if (Location == loc) return true;
      if (!Rules.IsAdjacent(Location,loc)) return false;
      // currently all containers are not-walkable for UI reasons.
      return loc.Map.GetMapObjectAt(loc.Position)?.IsContainer ?? false;
    }

    public bool StackIsBlocked(Location loc, out MapObject obj)
    {
      obj = (loc != Location ? loc.Map.GetMapObjectAt(loc.Position) : null);    // XXX this check should affect BehaviorResupply
      if (null == obj) return false;
      if (!obj.IsContainer && !loc.IsWalkableFor(this)) {
        // Cf. Actor::CanOpen
        if (obj is DoorWindow doorWindow && doorWindow.IsBarricaded) return true;
        // Cf. Actor::CanPush; closed door/window is not pushable but can be handled
        else if (!obj.IsMovable) return true; // would have to handle OnFire if that could happen
      }
      return false;
    }

    private string ReasonCantGiveTo(Actor target, Item gift)
    {
#if DEBUG
      if (null == target) throw new ArgumentException(nameof(target));
      if (null == gift) throw new ArgumentException(nameof(gift));
#endif
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

#if DEAD_FUNC
    public bool CanGiveTo(Actor target, Item gift)
    {
      return string.IsNullOrEmpty(ReasonCantGiveTo(target, gift));
    }
#endif

    private string ReasonCantGetFromContainer(Point position)
    {
      MapObject mapObjectAt = Location.Map.GetMapObjectAt(position);
      if (!mapObjectAt?.IsContainer ?? true) return "object is not a container";
      Inventory itemsAt = Location.Map.GetItemsAt(position);
      if (itemsAt == null) return "nothing to take there";
	  // XXX should be "can't get any of the items in the container"
      if (!IsPlayer && !CanGet(itemsAt.TopItem)) return "cannot take an item";
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
      switch(id)
      {
      case Skills.IDs.HAULER: if (null != m_Inventory) Inventory.MaxCapacity = MaxInv;
        break;
      case Skills.IDs.TOUGH: m_HitPoints += Actor.SKILL_TOUGH_HP_BONUS;
        break;
      case Skills.IDs.Z_TOUGH: m_HitPoints += Actor.SKILL_ZTOUGH_HP_BONUS;
        break;
      }
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

#if DEAD_FUNC
    private void OneFlag(Actor.Flags f)
    {
      m_Flags |= f;
    }

    private void ZeroFlag(Actor.Flags f)
    {
      m_Flags &= ~f;
    }
#endif

    // vision
    public int DarknessFOV {
      get {
        if (Model.Abilities.IsUndead) return Sheet.BaseViewRange;
        return MINIMAL_FOV;
      }
    }

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

    public int NightFovPenalty(WorldTime time)
    {
      return (Model.Abilities.IsUndead ? 0 : LivingNightFovPenalty(time));
    }

    private static int LivingWeatherFovPenalty(Weather weather)
    {
      switch (weather) {
        case Weather.RAIN: return FOV_PENALTY_RAIN;
        case Weather.HEAVY_RAIN: return FOV_PENALTY_HEAVY_RAIN;
        default: return 0;
      }
    }

    public int WeatherFovPenalty(Weather weather)
    {
      return (Model.Abilities.IsUndead ? 0 : LivingWeatherFovPenalty(weather));
    }

    int LightBonus {
      get {
        if (GetEquippedItem(DollPart.LEFT_HAND) is ItemLight light && 0 < light.Batteries) return light.FovBonus;
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
      if (mapObjectAt != null && mapObjectAt.StandOnFovBonus) FOV += FOV_BONUS_STANDING_ON_OBJECT;
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
                return 0 < (map.GetActorAt(pt)?.LightBonus ?? 0);
              })))
            lightBonus = 1;
        }
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
          FOV -= LivingNightFovPenalty(time) + LivingWeatherFovPenalty(Engine.Session.Get.World.Weather);
          break;
      }
      FOV += FOV_BONUS_STANDING_ON_OBJECT;  // but there are no relevant objects except on the entry map

      if (light == Lighting.DARKNESS || (light == Lighting.OUTSIDE && time.IsNight)) {
        FOV += MAX_LIGHT_FOV_BONUS;
      }
      return Math.Max(MINIMAL_FOV, FOV);
    }

    // smell
    public float Smell {
      get {
        return (float) (1.0 + (double) SKILL_ZTRACKER_SMELL_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_TRACKER)) * Model.StartingSheet.BaseSmellRating;
      }
    }

    public int SmellThreshold {
      get {
        if (IsSleeping) return -1;
        // Even a skill level of 1 will give a ZM a raw negative smell threshold.
        return Math.Max(1,(OdorScent.MAX_STRENGTH+1) - (int) ((double)Smell * OdorScent.MAX_STRENGTH));
      }
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
        if (it is ItemLight) Controller.UpdateSensors();
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

    // Note that an event-based Sees implementation (anchored in RogueGame) cannot avoid constructing messages
    // even when no players would recieve them.
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
    public void Moved() { Moving?.Invoke(this, null); }

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

    public void AfterAction()
    {
      m_previousHitPoints = m_HitPoints;
      m_previousFoodPoints = m_FoodPoints;
      m_previousSleepPoints = m_SleepPoints;
      m_previousSanity = m_Sanity;
    }

    public void DropScent()
    {
      if (Model.Abilities.IsUndead) {
        if (!Model.Abilities.IsUndeadMaster) return;
        Location.Map.RefreshScentAt(Odor.UNDEAD_MASTER, UNDEAD_MASTER_SCENT_DROP, Location.Position);
      } else
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
