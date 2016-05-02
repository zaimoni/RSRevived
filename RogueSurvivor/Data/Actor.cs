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
      get
      {
        return Models.Actors[this.m_ModelID];
      }
      set
      {
        this.m_ModelID = value.ID;
        this.OnModelSet();
      }
    }

    public bool IsUnique
    {
      get
      {
        return this.GetFlag(Actor.Flags.IS_UNIQUE);
      }
      set
      {
        this.SetFlag(Actor.Flags.IS_UNIQUE, value);
      }
    }

    public Faction Faction
    {
      get
      {
        return Models.Factions[this.m_FactionID];
      }
      set
      {
        this.m_FactionID = value.ID;
      }
    }

    public string Name
    {
      get
      {
        if (!this.IsPlayer)
          return this.m_Name;
        return "(YOU) " + this.m_Name;
      }
      set
      {
        this.m_Name = value;
        if (value == null)
          return;
        this.m_Name.Replace("(YOU) ", "");
      }
    }

    public string UnmodifiedName
    {
      get
      {
        return this.m_Name;
      }
    }

    public bool IsProperName
    {
      get
      {
        return this.GetFlag(Actor.Flags.IS_PROPER_NAME);
      }
      set
      {
        this.SetFlag(Actor.Flags.IS_PROPER_NAME, value);
      }
    }

    public bool IsPluralName
    {
      get
      {
        return this.GetFlag(Actor.Flags.IS_PLURAL_NAME);
      }
      set
      {
        this.SetFlag(Actor.Flags.IS_PLURAL_NAME, value);
      }
    }

    public string TheName
    {
      get
      {
        if (!this.IsProperName && !this.IsPluralName)
          return "the " + this.m_Name;
        return this.Name;
      }
    }

    public ActorController Controller
    {
      get
      {
        return this.m_Controller;
      }
      set
      {
        if (this.m_Controller != null)
          this.m_Controller.LeaveControl();
        this.m_Controller = value;
        if (this.m_Controller == null)
          return;
        this.m_Controller.TakeControl(this);
      }
    }

    public bool IsPlayer
    {
      get
      {
        if (this.m_Controller != null)
          return this.m_Controller is PlayerController;
        return false;
      }
    }

    public int SpawnTime
    {
      get
      {
        return this.m_SpawnTime;
      }
    }

    public int GangID
    {
      get
      {
        return this.m_GangID;
      }
      set
      {
        this.m_GangID = value;
      }
    }

    public bool IsInAGang
    {
      get
      {
        return this.m_GangID != 0;
      }
    }

    public Doll Doll
    {
      get
      {
        return this.m_Doll;
      }
    }

    public bool IsDead
    {
      get
      {
        return this.GetFlag(Actor.Flags.IS_DEAD);
      }
      set
      {
        this.SetFlag(Actor.Flags.IS_DEAD, value);
      }
    }

    public bool IsSleeping
    {
      get
      {
        return this.GetFlag(Actor.Flags.IS_SLEEPING);
      }
      set
      {
        this.SetFlag(Actor.Flags.IS_SLEEPING, value);
      }
    }

    public bool IsRunning
    {
      get
      {
        return this.GetFlag(Actor.Flags.IS_RUNNING);
      }
      set
      {
        this.SetFlag(Actor.Flags.IS_RUNNING, value);
      }
    }

    public Inventory Inventory
    {
      get
      {
        return this.m_Inventory;
      }
      set
      {
        this.m_Inventory = value;
      }
    }

    public int HitPoints
    {
      get
      {
        return this.m_HitPoints;
      }
      set
      {
        this.m_HitPoints = value;
      }
    }

    public int PreviousHitPoints
    {
      get
      {
        return this.m_previousHitPoints;
      }
      set
      {
        this.m_previousHitPoints = value;
      }
    }

    public int StaminaPoints
    {
      get
      {
        return this.m_StaminaPoints;
      }
      set
      {
        this.m_StaminaPoints = value;
      }
    }

    public int PreviousStaminaPoints
    {
      get
      {
        return this.m_previousStamina;
      }
      set
      {
        this.m_previousStamina = value;
      }
    }

    public int FoodPoints
    {
      get
      {
        return this.m_FoodPoints;
      }
      set
      {
        this.m_FoodPoints = value;
      }
    }

    public int PreviousFoodPoints
    {
      get
      {
        return this.m_previousFoodPoints;
      }
      set
      {
        this.m_previousFoodPoints = value;
      }
    }

    public int SleepPoints
    {
      get
      {
        return this.m_SleepPoints;
      }
      set
      {
        this.m_SleepPoints = value;
      }
    }

    public int PreviousSleepPoints
    {
      get
      {
        return this.m_previousSleepPoints;
      }
      set
      {
        this.m_previousSleepPoints = value;
      }
    }

    public int Sanity
    {
      get
      {
        return this.m_Sanity;
      }
      set
      {
        this.m_Sanity = value;
      }
    }

    public int PreviousSanity
    {
      get
      {
        return this.m_previousSanity;
      }
      set
      {
        this.m_previousSanity = value;
      }
    }

    public ActorSheet Sheet
    {
      get
      {
        return this.m_Sheet;
      }
    }

    public int ActionPoints
    {
      get
      {
        return this.m_ActionPoints;
      }
      set
      {
        this.m_ActionPoints = value;
      }
    }

    public int LastActionTurn
    {
      get
      {
        return this.m_LastActionTurn;
      }
      set
      {
        this.m_LastActionTurn = value;
      }
    }

    public Location Location
    {
      get
      {
        return this.m_Location;
      }
      set
      {
        this.m_Location = value;
      }
    }

    public Activity Activity
    {
      get
      {
        return this.m_Activity;
      }
      set
      {
        this.m_Activity = value;
      }
    }

    public Actor TargetActor
    {
      get
      {
        return this.m_TargetActor;
      }
      set
      {
        this.m_TargetActor = value;
      }
    }

    public int AudioRange
    {
      get
      {
        return this.m_Sheet.BaseAudioRange + this.m_AudioRangeMod;
      }
    }

    public int AudioRangeMod
    {
      get
      {
        return this.m_AudioRangeMod;
      }
      set
      {
        this.m_AudioRangeMod = value;
      }
    }

    public Attack CurrentMeleeAttack
    {
      get
      {
        return this.m_CurrentMeleeAttack;
      }
      set
      {
        this.m_CurrentMeleeAttack = value;
      }
    }

    public Attack CurrentRangedAttack
    {
      get
      {
        return this.m_CurrentRangedAttack;
      }
      set
      {
        this.m_CurrentRangedAttack = value;
      }
    }

    public Defence CurrentDefence
    {
      get
      {
        return this.m_CurrentDefence;
      }
      set
      {
        this.m_CurrentDefence = value;
      }
    }

    public Actor Leader
    {
      get
      {
        return this.m_Leader;
      }
    }

    public bool HasLeader
    {
      get
      {
        if (this.m_Leader != null)
          return !this.m_Leader.IsDead;
        return false;
      }
    }

    public int TrustInLeader
    {
      get
      {
        return this.m_TrustInLeader;
      }
      set
      {
        this.m_TrustInLeader = value;
      }
    }

    public IEnumerable<Actor> Followers
    {
      get
      {
        return (IEnumerable<Actor>) this.m_Followers;
      }
    }

    public int CountFollowers
    {
      get
      {
        if (this.m_Followers == null)
          return 0;
        return this.m_Followers.Count;
      }
    }

    public int KillsCount
    {
      get
      {
        return this.m_KillsCount;
      }
      set
      {
        this.m_KillsCount = value;
      }
    }

    public IEnumerable<Actor> AggressorOf
    {
      get
      {
        return (IEnumerable<Actor>) this.m_AggressorOf;
      }
    }

    public int CountAggressorOf
    {
      get
      {
        if (this.m_AggressorOf == null)
          return 0;
        return this.m_AggressorOf.Count;
      }
    }

    public IEnumerable<Actor> SelfDefenceFrom
    {
      get
      {
        return (IEnumerable<Actor>) this.m_SelfDefenceFrom;
      }
    }

    public int CountSelfDefenceFrom
    {
      get
      {
        if (this.m_SelfDefenceFrom == null)
          return 0;
        return this.m_SelfDefenceFrom.Count;
      }
    }

    public int MurdersCounter
    {
      get
      {
        return this.m_MurdersCounter;
      }
      set
      {
        this.m_MurdersCounter = value;
      }
    }

    public int Infection
    {
      get
      {
        return this.m_Infection;
      }
      set
      {
        this.m_Infection = value;
      }
    }

    public Corpse DraggedCorpse
    {
      get
      {
        return this.m_DraggedCorpse;
      }
      set
      {
        this.m_DraggedCorpse = value;
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
      this.m_ModelID = model.ID;
      this.m_FactionID = faction.ID;
      this.m_GangID = 0;
      this.m_Name = name;
      this.IsProperName = isProperName;
      this.IsPluralName = isPluralName;
      this.m_Location = new Location();
      this.m_SpawnTime = spawnTime;
      this.IsUnique = false;
      this.IsDead = false;
      this.OnModelSet();
    }

    public Actor(ActorModel model, Faction faction, int spawnTime)
      : this(model, faction, model.Name, false, false, spawnTime)
    {
    }

    private void OnModelSet()
    {
      ActorModel model = this.Model;
      this.m_Doll = new Doll(model.DollBody);
      this.m_Sheet = new ActorSheet(model.StartingSheet);
      this.m_ActionPoints = this.m_Doll.Body.Speed;
      this.m_HitPoints = this.m_previousHitPoints = this.m_Sheet.BaseHitPoints;
      this.m_StaminaPoints = this.m_previousStamina = this.m_Sheet.BaseStaminaPoints;
      this.m_FoodPoints = this.m_previousFoodPoints = this.m_Sheet.BaseFoodPoints;
      this.m_SleepPoints = this.m_previousSleepPoints = this.m_Sheet.BaseSleepPoints;
      this.m_Sanity = this.m_previousSanity = this.m_Sheet.BaseSanity;
      if (model.Abilities.HasInventory)
        this.m_Inventory = new Inventory(model.StartingSheet.BaseInventoryCapacity);
      this.m_CurrentMeleeAttack = model.StartingSheet.UnarmedAttack;
      this.m_CurrentDefence = model.StartingSheet.BaseDefence;
      this.m_CurrentRangedAttack = Attack.BLANK;
    }

    public void AddFollower(Actor other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      if (this.m_Followers != null && this.m_Followers.Contains(other))
        throw new ArgumentException("other is already a follower");
      if (this.m_Followers == null)
        this.m_Followers = new List<Actor>(1);
      this.m_Followers.Add(other);
      if (other.Leader != null)
        other.Leader.RemoveFollower(other);
      other.m_Leader = this;
    }

    public void RemoveFollower(Actor other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      if (this.m_Followers == null)
        throw new InvalidOperationException("no followers");
      this.m_Followers.Remove(other);
      if (this.m_Followers.Count == 0)
        this.m_Followers = (List<Actor>) null;
      other.m_Leader = (Actor) null;
      AIController aiController = other.Controller as AIController;
      if (aiController == null)
        return;
      aiController.Directives.Reset();
      aiController.SetOrder((ActorOrder) null);
    }

    public void RemoveAllFollowers()
    {
      while (this.m_Followers != null && this.m_Followers.Count > 0)
        this.RemoveFollower(this.m_Followers[0]);
    }

    public void SetTrustIn(Actor other, int trust)
    {
      if (this.m_TrustList == null)
      {
        this.m_TrustList = new List<TrustRecord>(1)
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
        foreach (TrustRecord mTrust in this.m_TrustList)
        {
          if (mTrust.Actor == other)
          {
            mTrust.Trust = trust;
            return;
          }
        }
        this.m_TrustList.Add(new TrustRecord()
        {
          Actor = other,
          Trust = trust
        });
      }
    }

    public void AddTrustIn(Actor other, int amount)
    {
      this.SetTrustIn(other, this.GetTrustIn(other) + amount);
    }

    public int GetTrustIn(Actor other)
    {
      if (this.m_TrustList == null)
        return 0;
      foreach (TrustRecord mTrust in this.m_TrustList)
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
      if (this.m_AggressorOf == null)
        this.m_AggressorOf = new List<Actor>(1);
      else if (this.m_AggressorOf.Contains(other))
        return;
      this.m_AggressorOf.Add(other);
    }

    public void MarkAsSelfDefenceFrom(Actor other)
    {
      if (other == null || other.IsDead)
        return;
      if (this.m_SelfDefenceFrom == null)
        this.m_SelfDefenceFrom = new List<Actor>(1);
      else if (this.m_SelfDefenceFrom.Contains(other))
        return;
      this.m_SelfDefenceFrom.Add(other);
    }

    public bool IsAggressorOf(Actor other)
    {
      if (this.m_AggressorOf == null)
        return false;
      return this.m_AggressorOf.Contains(other);
    }

    public bool IsSelfDefenceFrom(Actor other)
    {
      if (this.m_SelfDefenceFrom == null)
        return false;
      return this.m_SelfDefenceFrom.Contains(other);
    }

    public void RemoveAggressorOf(Actor other)
    {
      if (this.m_AggressorOf == null)
        return;
      this.m_AggressorOf.Remove(other);
      if (this.m_AggressorOf.Count != 0)
        return;
      this.m_AggressorOf = (List<Actor>) null;
    }

    public void RemoveSelfDefenceFrom(Actor other)
    {
      if (this.m_SelfDefenceFrom == null)
        return;
      this.m_SelfDefenceFrom.Remove(other);
      if (this.m_SelfDefenceFrom.Count != 0)
        return;
      this.m_SelfDefenceFrom = (List<Actor>) null;
    }

    public void RemoveAllAgressorSelfDefenceRelations()
    {
      while (this.m_AggressorOf != null)
      {
        Actor other = this.m_AggressorOf[0];
        this.RemoveAggressorOf(other);
        other.RemoveSelfDefenceFrom(this);
      }
      while (this.m_SelfDefenceFrom != null)
      {
        Actor other = this.m_SelfDefenceFrom[0];
        this.RemoveSelfDefenceFrom(other);
        other.RemoveAggressorOf(this);
      }
    }

    public bool AreDirectEnemies(Actor other)
    {
      return other != null && !other.IsDead && (this.m_AggressorOf != null && this.m_AggressorOf.Contains(other) || this.m_SelfDefenceFrom != null && this.m_SelfDefenceFrom.Contains(other) || (other.IsAggressorOf(this) || other.IsSelfDefenceFrom(this)));
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

    // sanity
    public bool IsInsane {
      get {
        if (Model.Abilities.HasSanity) return Sanity <= 0;
        return false;
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

    // flag handling
    private bool GetFlag(Actor.Flags f)
    {
      return (this.m_Flags & f) != Actor.Flags.NONE;
    }

    private void SetFlag(Actor.Flags f, bool value)
    {
      if (value)
        this.m_Flags |= f;
      else
        this.m_Flags &= ~f;
    }

    private void OneFlag(Actor.Flags f)
    {
      this.m_Flags |= f;
    }

    private void ZeroFlag(Actor.Flags f)
    {
      this.m_Flags &= ~f;
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
