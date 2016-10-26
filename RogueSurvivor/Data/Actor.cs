// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Actor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine.Items;
using System;
using System.Drawing;
using System.Collections.Generic;

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
    private int m_ModelID;
    private int m_FactionID;
    private int m_GangID;
    private string m_Name;
    private ActorController m_Controller;
    private ActorSheet m_Sheet;
    private int m_SpawnTime;
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
        return Models.Actors[m_ModelID];
      }
      set {
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

    public int GangID {
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

    public Actor(ActorModel model, Faction faction, string name, bool isProperName, bool isPluralName, int spawnTime)
    {
      if (model == null) throw new ArgumentNullException("model");
      if (faction == null) throw new ArgumentNullException("faction");
      if (name == null) throw new ArgumentNullException("name");
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

    public Actor(ActorModel model, Faction faction, int spawnTime)
      : this(model, faction, model.Name, false, false, spawnTime)
    {
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
      m_CurrentMeleeAttack = model.StartingSheet.UnarmedAttack;
      m_CurrentDefence = model.StartingSheet.BaseDefence;
      m_CurrentRangedAttack = Attack.BLANK;
    }

    public int DamageBonusVsUndeads {
      get {
        return Actor.SKILL_NECROLOGY_UNDEAD_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.NECROLOGY);
      }
    }

    public Attack HypotheticalMeleeAttack(Attack baseAttack, Actor target = null)
    {
      int num3 = Actor.SKILL_AGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.AGILE) + Actor.SKILL_ZAGILE_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.Z_AGILE);
      int num4 = Actor.SKILL_STRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.STRONG) + Actor.SKILL_ZSTRONG_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.Z_STRONG);
      if (GetEquippedWeapon() == null)
      {
        num3 += Actor.SKILL_MARTIAL_ARTS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.MARTIAL_ARTS);
        num4 += Actor.SKILL_MARTIAL_ARTS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.MARTIAL_ARTS);
      }
      if (target != null && target.Model.Abilities.IsUndead)
        num4 += DamageBonusVsUndeads;
      float num5 = (float)baseAttack.HitValue + (float) num3;
      if (IsExhausted) num5 /= 2f;
      else if (IsSleepy) num5 *= 0.75f;
      return new Attack(baseAttack.Kind, baseAttack.Verb, (int) num5, baseAttack.DamageValue + num4, baseAttack.StaminaPenalty);
    }

    // ultimately these two will be thin wrappers, as CurrentMeleeAttack/CurrentRangedAttack are themselves mathematical functions
    // of the equipped weapon which OrderableAI *will* want to vary when choosing an appropriate weapon
    public Attack MeleeAttack(Actor target = null) { return HypotheticalMeleeAttack(CurrentMeleeAttack, target); }

    public Attack RangedAttack(int distance, Actor target = null)
    {
      Attack baseAttack = CurrentRangedAttack;
      int num1 = 0;
      int num2 = 0;
      switch (baseAttack.Kind)
      {
        case AttackKind.FIREARM:
          num1 = Actor.SKILL_FIREARMS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.FIREARMS);
          num2 = Actor.SKILL_FIREARMS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.FIREARMS);
          break;
        case AttackKind.BOW:
          num1 = Actor.SKILL_BOWS_ATK_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.BOWS);
          num2 = Actor.SKILL_BOWS_DMG_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.BOWS);
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

    public void MarkAsAgressorOf(Actor other)
    {
      if (other == null || other.IsDead) return;
      if (m_AggressorOf == null) m_AggressorOf = new List<Actor>(1);
      else if (m_AggressorOf.Contains(other)) return;
      m_AggressorOf.Add(other);
    }

    public void MarkAsSelfDefenceFrom(Actor other)
    {
      if (other == null || other.IsDead) return;
      if (m_SelfDefenceFrom == null) m_SelfDefenceFrom = new List<Actor>(1);
      else if (m_SelfDefenceFrom.Contains(other)) return;
      m_SelfDefenceFrom.Add(other);
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

    // health
    public int MaxHPs {
      get {
        int num = SKILL_TOUGH_HP_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.TOUGH) + SKILL_ZTOUGH_HP_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.Z_TOUGH);
        return Sheet.BaseHitPoints + num;
      }
    }

    public void RegenHitPoints(int hpRegen)
    {
      m_HitPoints = Math.Min(MaxHPs, m_HitPoints + hpRegen);
    }

    // stamina
    public bool WillTireAfter(int staminaCost)
    {
      if (!Model.Abilities.CanTire) return false;
      if (Location.Map.LocalTime.IsNight && staminaCost > 0)
        staminaCost += Model.Abilities.IsUndead ? 0 : NIGHT_STA_PENALTY;
      if (IsExhausted) staminaCost *= 2;
      return m_StaminaPoints + STAMINA_MIN_FOR_ACTIVITY < staminaCost;
    }

    public int MaxSTA {
      get {
        int num = SKILL_HIGH_STAMINA_STA_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.HIGH_STAMINA);
        return Sheet.BaseStaminaPoints + num;
      }
    }

    public bool IsTired {
      get {
        if (!Model.Abilities.CanTire) return false;
        return m_StaminaPoints < STAMINA_MIN_FOR_ACTIVITY;
      }
    }

    public bool CanRun(out string reason)
    {
      if (!Model.Abilities.CanRun) {
        reason = "no ability to run";
        return false;
      }
      if (StaminaPoints < Actor.STAMINA_MIN_FOR_ACTIVITY) {
        reason = "not enough stamina to run";
        return false;
      }
      reason = "";
      return true;
    }

    public bool CanRun()
    {
      string reason;
      return CanRun(out reason);
    }

    public bool CanJump {
      get {
       return Model.Abilities.CanJump
            || 0 < Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.AGILE)
            || 0 < Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.Z_AGILE);
      }
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
        int num = (int) ((double) Sheet.BaseFoodPoints * (double) SKILL_LIGHT_EATER_MAXFOOD_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.LIGHT_EATER));
        return Sheet.BaseFoodPoints + num;
      }
    }

    public int MaxRot {
      get {
        int num = (int) ((double) Sheet.BaseFoodPoints * (double) SKILL_ZLIGHT_EATER_MAXFOOD_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.Z_LIGHT_EATER));
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
        int num = (int) ((double) Sheet.BaseSleepPoints * (double) SKILL_AWAKE_SLEEP_BONUS * (double) Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.AWAKE));
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
        int num = SKILL_HAULER_INV_BONUS * Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.HAULER);
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

    public bool HasItemOfType(Type tt)
    {
      if (Inventory == null || Inventory.IsEmpty) return false;
      return Inventory.HasItemOfType(tt);
    }

    public bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
    {
      if (null == m_Inventory || m_Inventory.IsEmpty) return false;
      if (it.Model.IsStackable)
        return CountItemsQuantityOfModel(it.Model) >= n * it.Model.StackingLimit;
      return CountItemsOfSameType(it.GetType()) >= n;
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

    public Skill SkillUpgrade(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      Sheet.SkillTable.AddOrIncreaseSkill(id);
      Skill skill = Sheet.SkillTable.GetSkill(id);
      if (id == djack.RogueSurvivor.Gameplay.Skills.IDs.HAULER && Inventory != null)
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

    public int FOVrange(WorldTime time, Weather weather)
    {
      if (IsSleeping) return 0;
      int val2 = Sheet.BaseViewRange;
      Lighting lighting = Location.Map.Lighting;
      switch (lighting)
      {
        case Lighting.DARKNESS:
          val2 = DarknessFOV;
          goto case Lighting.LIT;
        case Lighting.OUTSIDE:
          val2 -= NightFovPenalty(time) + WeatherFovPenalty(weather);
          goto case Lighting.LIT;
        case Lighting.LIT:
          if (IsExhausted) val2 -= 2;
          else if (IsSleepy) --val2;
          if (lighting == Lighting.DARKNESS || (lighting == Lighting.OUTSIDE && time.IsNight))
          {
            int num = LightBonus;
            if (num == 0)
            {
              Map map = Location.Map;
              if (map.HasAnyAdjacentInMap(Location.Position, (Predicate<System.Drawing.Point>) (pt =>
              {
                Actor actorAt = map.GetActorAt(pt);
                if (actorAt == null) return false;
                return 0 < actorAt.LightBonus;
              })))
                num = 1;
            }
            val2 += num;
          }
          MapObject mapObjectAt = Location.Map.GetMapObjectAt(Location.Position);
          if (mapObjectAt != null && mapObjectAt.StandOnFovBonus) ++val2;
          return Math.Max(MINIMAL_FOV, val2);
        default:
          throw new ArgumentOutOfRangeException("unhandled lighting");
      }
    }

    // event handlers
    public void OnEquipItem(Engine.RogueGame game, Item it)
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
      if (it.Model is ItemTrackerModel) {
        --(it as ItemTracker).Batteries;
        return;
      }
      if (it.Model is ItemLightModel) {
        --(it as ItemLight).Batteries;
        if (IsPlayer) game.UpdatePlayerFOV(this);
        return;
      }
    }

    public void OnUnequipItem(Engine.RogueGame game, Item it)
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

    public static EventHandler<SayArgs> Says;

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

    // administrative functions whose presence here is not clearly advisable but they improve the access situation here
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

    public void CreateCivilianDeductFoodSleep(Engine.Rules r) { 
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
      if (m_Inventory == null) m_Inventory = new Inventory(1);  // but the GUI still won't display it for undead; test removing for 0.10.0
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
