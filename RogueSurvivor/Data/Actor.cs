// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Actor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
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

    public static float SKILL_AWAKE_SLEEP_BONUS = 0.15f;    // XXX 0.17f makes this useful at L1
    public static int SKILL_HAULER_INV_BONUS = 1;
    public static int SKILL_HIGH_STAMINA_STA_BONUS = 5;
    public static float SKILL_LIGHT_EATER_MAXFOOD_BONUS = 0.15f;
    public static int SKILL_TOUGH_HP_BONUS = 3;
    public static float SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = 0.15f;
    public static int SKILL_ZTOUGH_HP_BONUS = 4;

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
    private List<TrustRecord> m_TrustList;
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
        if (m_Controller != null)
          m_Controller.LeaveControl();
        m_Controller = value;
        if (m_Controller != null)
          m_Controller.TakeControl(this);
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

    public int PreviousSleepPoints
    {
      get
      {
        return m_previousSleepPoints;
      }
    }

    public int Sanity
    {
      get {
        return m_Sanity;
      }
    }

    public int PreviousSanity
    {
      get
      {
        return m_previousSanity;
      }
      set
      {
                m_previousSanity = value;
      }
    }

    public ActorSheet Sheet
    {
      get
      {
        return m_Sheet;
      }
    }

    public int ActionPoints
    {
      get
      {
        return m_ActionPoints;
      }
      set
      {
                m_ActionPoints = value;
      }
    }

    public int LastActionTurn {
      get {
        return m_LastActionTurn;
      }
    }

    public Location Location
    {
      get
      {
        return m_Location;
      }
      set
      {
                m_Location = value;
      }
    }

    public Activity Activity
    {
      get
      {
        return m_Activity;
      }
      set
      {
                m_Activity = value;
      }
    }

    public Actor TargetActor
    {
      get
      {
        return m_TargetActor;
      }
      set
      {
                m_TargetActor = value;
      }
    }

    public int AudioRange
    {
      get
      {
        return m_Sheet.BaseAudioRange + m_AudioRangeMod;
      }
    }

    public int AudioRangeMod
    {
      get
      {
        return m_AudioRangeMod;
      }
      set
      {
                m_AudioRangeMod = value;
      }
    }

    public Attack CurrentMeleeAttack
    {
      get
      {
        return m_CurrentMeleeAttack;
      }
      set
      {
                m_CurrentMeleeAttack = value;
      }
    }

    public Attack CurrentRangedAttack
    {
      get
      {
        return m_CurrentRangedAttack;
      }
      set
      {
                m_CurrentRangedAttack = value;
      }
    }

    public Defence CurrentDefence
    {
      get
      {
        return m_CurrentDefence;
      }
      set
      {
                m_CurrentDefence = value;
      }
    }

    public Actor Leader
    {
      get
      {
        return m_Leader;
      }
    }

    public bool HasLeader
    {
      get
      {
        if (m_Leader != null)
          return !m_Leader.IsDead;
        return false;
      }
    }

    public int TrustInLeader
    {
      get
      {
        return m_TrustInLeader;
      }
      set
      {
                m_TrustInLeader = value;
      }
    }

    public IEnumerable<Actor> Followers
    {
      get
      {
        return (IEnumerable<Actor>)m_Followers;
      }
    }

    public int CountFollowers
    {
      get
      {
        if (m_Followers == null)
          return 0;
        return m_Followers.Count;
      }
    }

    public int KillsCount
    {
      get
      {
        return m_KillsCount;
      }
      set
      {
                m_KillsCount = value;
      }
    }

    public IEnumerable<Actor> AggressorOf
    {
      get
      {
        return (IEnumerable<Actor>)m_AggressorOf;
      }
    }

    public int CountAggressorOf
    {
      get
      {
        if (m_AggressorOf == null)
          return 0;
        return m_AggressorOf.Count;
      }
    }

    public IEnumerable<Actor> SelfDefenceFrom
    {
      get
      {
        return (IEnumerable<Actor>)m_SelfDefenceFrom;
      }
    }

    public int CountSelfDefenceFrom
    {
      get
      {
        if (m_SelfDefenceFrom == null)
          return 0;
        return m_SelfDefenceFrom.Count;
      }
    }

    public int MurdersCounter
    {
      get
      {
        return m_MurdersCounter;
      }
      set
      {
                m_MurdersCounter = value;
      }
    }

    public int Infection
    {
      get
      {
        return m_Infection;
      }
      set
      {
                m_Infection = value;
      }
    }

    public Corpse DraggedCorpse
    {
      get
      {
        return m_DraggedCorpse;
      }
      set
      {
                m_DraggedCorpse = value;
      }
    }

    public Actor(ActorModel model, Faction faction, string name, bool isProperName, bool isPluralName, int spawnTime)
    {
      if (model == null)
        throw new ArgumentNullException("model");
      if (faction == null)
        throw new ArgumentNullException("faction");
      if (name == null)
        throw new ArgumentNullException("name");
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

    public void AddFollower(Actor other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      if (m_Followers != null && m_Followers.Contains(other))
        throw new ArgumentException("other is already a follower");
      if (m_Followers == null)
                m_Followers = new List<Actor>(1);
            m_Followers.Add(other);
      if (other.Leader != null)
        other.Leader.RemoveFollower(other);
      other.m_Leader = this;
    }

    public void RemoveFollower(Actor other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      if (m_Followers == null)
        throw new InvalidOperationException("no followers");
            m_Followers.Remove(other);
      if (m_Followers.Count == 0)
                m_Followers = (List<Actor>) null;
      other.m_Leader = (Actor) null;
      AIController aiController = other.Controller as AIController;
      if (aiController == null)
        return;
      aiController.Directives.Reset();
      aiController.SetOrder((ActorOrder) null);
    }

    public void RemoveAllFollowers()
    {
      while (m_Followers != null && m_Followers.Count > 0)
                RemoveFollower(m_Followers[0]);
    }

    public void SetTrustIn(Actor other, int trust)
    {
      if (m_TrustList == null)
      {
                m_TrustList = new List<TrustRecord>(1)
        {
          new TrustRecord()
          {
            Actor = other,
            Trust = trust
          }
        };
      }
      else
      {
        foreach (TrustRecord mTrust in m_TrustList)
        {
          if (mTrust.Actor == other)
          {
            mTrust.Trust = trust;
            return;
          }
        }
                m_TrustList.Add(new TrustRecord()
        {
          Actor = other,
          Trust = trust
        });
      }
    }

    public void AddTrustIn(Actor other, int amount)
    {
            SetTrustIn(other, GetTrustIn(other) + amount);
    }

    public int GetTrustIn(Actor other)
    {
      if (m_TrustList == null)
        return 0;
      foreach (TrustRecord mTrust in m_TrustList)
      {
        if (mTrust.Actor == other)
          return mTrust.Trust;
      }
      return 0;
    }

    public void MarkAsAgressorOf(Actor other)
    {
      if (other == null || other.IsDead)
        return;
      if (m_AggressorOf == null)
                m_AggressorOf = new List<Actor>(1);
      else if (m_AggressorOf.Contains(other))
        return;
            m_AggressorOf.Add(other);
    }

    public void MarkAsSelfDefenceFrom(Actor other)
    {
      if (other == null || other.IsDead)
        return;
      if (m_SelfDefenceFrom == null)
                m_SelfDefenceFrom = new List<Actor>(1);
      else if (m_SelfDefenceFrom.Contains(other))
        return;
            m_SelfDefenceFrom.Add(other);
    }

    public bool IsAggressorOf(Actor other)
    {
      if (m_AggressorOf == null)
        return false;
      return m_AggressorOf.Contains(other);
    }

    public bool IsSelfDefenceFrom(Actor other)
    {
      if (m_SelfDefenceFrom == null)
        return false;
      return m_SelfDefenceFrom.Contains(other);
    }

    public void RemoveAggressorOf(Actor other)
    {
      if (m_AggressorOf == null)
        return;
            m_AggressorOf.Remove(other);
      if (m_AggressorOf.Count != 0)
        return;
            m_AggressorOf = (List<Actor>) null;
    }

    public void RemoveSelfDefenceFrom(Actor other)
    {
      if (m_SelfDefenceFrom == null)
        return;
            m_SelfDefenceFrom.Remove(other);
      if (m_SelfDefenceFrom.Count != 0)
        return;
            m_SelfDefenceFrom = (List<Actor>) null;
    }

    public void RemoveAllAgressorSelfDefenceRelations()
    {
      while (m_AggressorOf != null)
      {
        Actor other = m_AggressorOf[0];
                RemoveAggressorOf(other);
        other.RemoveSelfDefenceFrom(this);
      }
      while (m_SelfDefenceFrom != null)
      {
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

    public void SpendActionPoints(int actionCost)
    {
      m_ActionPoints -= actionCost;
      m_LastActionTurn = Location.Map.LocalTime.TurnCounter;
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

    public Item GetEquippedWeapon()
    {
      return GetEquippedItem(DollPart._FIRST);
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

    // This prepares an actor for being a PC.  Note that hacking the player controller in
    // by hex-editing does work flawlessly at the Actor, level, so whether any of this 
    // is needed is a good question.
    public void PrepareForPlayerControl()
    {
      if (m_Inventory == null) m_Inventory = new Inventory(1);  // but the GUI still won't display it for undead
      if (Sheet.SkillTable == null) Sheet.SkillTable = new SkillTable(); 
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
