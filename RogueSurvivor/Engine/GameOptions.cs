﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.GameOptions
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
/*
 * DEFAULT_DISTRICT_SIZE is a key time-scaling parameter.  It is 5/3 of WorldTime.TURNS_PER_HOUR.
 * At the standard 30 turns/hour, this is 50
 * At the other extreme of interest (15 turns/minute, 900/hour) it would be 1,500.  This *plausibly*
 * is unmanagable even with Dungeon Crawl Stone Soup's large map UI features.
 * One would expect DEFAULT_MAX_CIVILIANS/DOGS/UNDEAD to scale as the square of the time unit
 * Note that weapon and visibility ranges *also* scale with time in order to stay proportionate with the district distance scale.
 * The stamina cost for pushing cars should not scale, but the stamina stat of livings should scale (as should the relevant skills).
 */

// Design decision: we do not want to reference Session.Get.GameMode in the accessors.
// RogueGame::HandleOptions wants all of these setters to be public

  [Serializable]
  internal struct GameOptions
  {
    public const int DEFAULT_DISTRICT_SIZE = 50;
    public const int DEFAULT_MAX_CIVILIANS = 25;
    public const int DEFAULT_MAX_DOGS = 0;  // 5
    public const int DEFAULT_MAX_UNDEADS = 100;
    public const int DEFAULT_SPAWN_SKELETON_CHANCE = 60;
    public const int DEFAULT_SPAWN_ZOMBIE_CHANCE = 30;
    public const int DEFAULT_SPAWN_ZOMBIE_MASTER_CHANCE = 10;
    public const int DEFAULT_CITY_SIZE = 5;
    public const SimRatio DEFAULT_SIM_DISTRICTS = SimRatio.FULL;
    public const int DEFAULT_ZOMBIFICATION_CHANCE = 100;
    public const int DEFAULT_DAY_ZERO_UNDEADS_PERCENT = 30;
    public const int DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE = 5;
    public const int DEFAULT_STARVED_ZOMBIFICATION_CHANCE = 50;
    public const int DEFAULT_MAX_REINCARNATIONS = 1;
    public const int DEFAULT_NATGUARD_FACTOR = 100;
    public const int DEFAULT_SUPPLIESDROP_FACTOR = 100;
    public const ZupDays DEFAULT_ZOMBIFIEDS_UPGRADE_DAYS = ZupDays.THREE;
    private int m_DistrictSize;
    private int m_MaxCivilians;
    private int m_MaxDogs;
    private int m_MaxUndeads;
    private bool m_PlayMusic;
    private int m_MusicVolume;
    private bool m_AnimDelay;
    private bool m_ShowMinimap;
    private bool m_EnabledAdvisor;
    private bool m_CombatAssistant;
    private bool m_ShowPlayerTagsOnMinimap;
    private int m_SpawnSkeletonChance;
    private int m_SpawnZombieChance;
    private int m_SpawnZombieMasterChance;
    private short m_CitySize;
    private bool m_NPCCanStarveToDeath;
    private int m_ZombificationChance;
    private bool m_RevealStartingDistrict;
    private bool m_AllowUndeadsEvolution;
    private int m_DayZeroUndeadsPercent;
    private int m_ZombieInvasionDailyIncrease;
    private int m_StarvedZombificationChance;
    private int m_MaxReincarnations;
    private bool m_CanReincarnateAsRat;
    private bool m_CanReincarnateToSewers;
    private bool m_IsLivingReincRestricted;
    private bool m_Permadeath;
    private bool m_DeathScreenshot;
    private bool m_AggressiveHungryCivilians;
    private int m_NatGuardFactor;
    private int m_SuppliesDropFactor;
    private bool m_ShowTargets;
    private bool m_ShowPlayerTargets;
    private ZupDays m_ZupDays;
    private bool m_RatsUpgrade;
    private bool m_SkeletonsUpgrade;
    private bool m_ShamblersUpgrade;

    public bool PlayMusic
    {
      get
      {
        return m_PlayMusic;
      }
      set
      {
                m_PlayMusic = value;
      }
    }

    public int MusicVolume {
      get {
        return m_MusicVolume;
      }
      set {
        if (value < 0) value = 0;
        if (value > 100) value = 100;
        m_MusicVolume = value;
      }
    }

    public bool ShowPlayerTagsOnMinimap
    {
      get
      {
        return m_ShowPlayerTagsOnMinimap;
      }
      set
      {
                m_ShowPlayerTagsOnMinimap = value;
      }
    }

    public bool IsAnimDelayOn
    {
      get
      {
        return m_AnimDelay;
      }
      set
      {
                m_AnimDelay = value;
      }
    }

    public bool IsMinimapOn
    {
      get
      {
        return m_ShowMinimap;
      }
      set
      {
                m_ShowMinimap = value;
      }
    }

    public bool IsAdvisorEnabled
    {
      get
      {
        return m_EnabledAdvisor;
      }
      set
      {
                m_EnabledAdvisor = value;
      }
    }

    public bool IsCombatAssistantOn
    {
      get
      {
        return m_CombatAssistant;
      }
      set
      {
                m_CombatAssistant = value;
      }
    }

    static public bool CitySize_ok(int x)
    {
      return 3<=x && 7 >= x;
    }

    public short CitySize {
      get {
        return m_CitySize;
      }
      set {
        if (value < 3) value = 3;
        if (value > 7) value = 7;
        m_CitySize = value;
      }
    }

    public int MaxCivilians {
      get {
        return m_MaxCivilians;
      }
      set {
        if (value < 10) value = 10;
        if (value > 75) value = 75;
        m_MaxCivilians = value;
      }
    }

    public int MaxDogs {
      get {
        return m_MaxDogs;
      }
      set {
        if (value < 0) value = 0;
        if (value > 75) value = 75;
        m_MaxDogs = value;
      }
    }

    public int MaxUndeads {
      get {
        return m_MaxUndeads;
      }
      set {
        if (value < 10) value = 10;
        if (value > 200) value = 200;
        m_MaxUndeads = value;
      }
    }

    public int SpawnSkeletonChance {
      get {
        return m_SpawnSkeletonChance;
      }
      set {
        if (value < 0) value = 0;
        if (value > 100) value = 100;
        m_SpawnSkeletonChance = value;
      }
    }

    public int SpawnZombieChance {
      get {
        return m_SpawnZombieChance;
      }
      set {
        if (value < 0) value = 0;
        if (value > 100) value = 100;
        m_SpawnZombieChance = value;
      }
    }

    public int SpawnZombieMasterChance {
      get {
        return m_SpawnZombieMasterChance;
      }
      set {
        if (value < 0) value = 0;
        if (value > 100) value = 100;
        m_SpawnZombieMasterChance = value;
      }
    }

    static public bool DistrictSize_ok(int x)
    {
      return 0==(x%10) && 30 <= x && Math.Min(RogueGame.MAP_MAX_HEIGHT, RogueGame.MAP_MAX_WIDTH) >= x;
    }

    public int DistrictSize
    {
      get {
        return m_DistrictSize;
      }
      set {
        if (value < 30) value = 30;
        if (value > RogueGame.MAP_MAX_HEIGHT || value > RogueGame.MAP_MAX_WIDTH)
          value = Math.Min(RogueGame.MAP_MAX_HEIGHT, RogueGame.MAP_MAX_WIDTH);
        m_DistrictSize = value;
      }
    }

    public bool NPCCanStarveToDeath
    {
      get
      {
        return m_NPCCanStarveToDeath;
      }
      set
      {
                m_NPCCanStarveToDeath = value;
      }
    }

    public int ZombificationChance
    {
      get {
        return m_ZombificationChance;
      }
      set {
        if (value < 10) value = 10;
        if (value > 100) value = 100;
        m_ZombificationChance = value;
      }
    }

    public bool RevealStartingDistrict
    {
      get
      {
        return m_RevealStartingDistrict;
      }
      set
      {
                m_RevealStartingDistrict = value;
      }
    }

    public bool AllowUndeadsEvolution
    {
      get
      {
        return m_AllowUndeadsEvolution;
      }
      set
      {
                m_AllowUndeadsEvolution = value;
      }
    }

    public int DayZeroUndeadsPercent
    {
      get {
        return m_DayZeroUndeadsPercent;
      }
      set {
        if (value < 10) value = 10;
        if (value > 100) value = 100;
        m_DayZeroUndeadsPercent = value;
      }
    }

    public int ZombieInvasionDailyIncrease
    {
      get {
        return m_ZombieInvasionDailyIncrease;
      }
      set {
        if (value < 1) value = 1;
        if (value > 20) value = 20;
        m_ZombieInvasionDailyIncrease = value;
      }
    }

    public int StarvedZombificationChance
    {
      get {
        return m_StarvedZombificationChance;
      }
      set {
        if (value < 0) value = 0;
        if (value > 100) value = 100;
        m_StarvedZombificationChance = value;
      }
    }

    public int MaxReincarnations {
      get {
        return m_MaxReincarnations;
      }
      set {
        if (value < 0) value = 0;
        if (value > 7) value = 7;
        m_MaxReincarnations = value;
      }
    }

    public bool CanReincarnateAsRat
    {
      get
      {
        return m_CanReincarnateAsRat;
      }
      set
      {
                m_CanReincarnateAsRat = value;
      }
    }

    public bool CanReincarnateToSewers
    {
      get
      {
        return m_CanReincarnateToSewers;
      }
      set
      {
                m_CanReincarnateToSewers = value;
      }
    }

    public bool IsLivingReincRestricted
    {
      get
      {
        return m_IsLivingReincRestricted;
      }
      set
      {
                m_IsLivingReincRestricted = value;
      }
    }

    public bool IsPermadeathOn
    {
      get
      {
        return m_Permadeath;
      }
      set
      {
                m_Permadeath = value;
      }
    }

    public bool IsDeathScreenshotOn
    {
      get
      {
        return m_DeathScreenshot;
      }
      set
      {
                m_DeathScreenshot = value;
      }
    }

    public bool IsAggressiveHungryCiviliansOn
    {
      get
      {
        return m_AggressiveHungryCivilians;
      }
      set
      {
                m_AggressiveHungryCivilians = value;
      }
    }

    public int NatGuardFactor {
      get {
        return m_NatGuardFactor;
      }
      set {
        if (value < 0) value = 0;
        if (value > 200) value = 200;
        m_NatGuardFactor = value;
      }
    }

    public int SuppliesDropFactor {
      get {
        return m_SuppliesDropFactor;
      }
      set {
        if (value < 0) value = 0;
        if (value > 200) value = 200;
        m_SuppliesDropFactor = value;
      }
    }

    public bool ShowTargets
    {
      get
      {
        return m_ShowTargets;
      }
      set
      {
                m_ShowTargets = value;
      }
    }

    public bool ShowPlayerTargets
    {
      get
      {
        return m_ShowPlayerTargets;
      }
      set
      {
                m_ShowPlayerTargets = value;
      }
    }

    public ZupDays ZombifiedsUpgradeDays
    {
      get
      {
        return m_ZupDays;
      }
      set
      {
                m_ZupDays = value;
      }
    }

    public bool RatsUpgrade
    {
      get
      {
        return m_RatsUpgrade;
      }
      set
      {
                m_RatsUpgrade = value;
      }
    }

    public bool SkeletonsUpgrade
    {
      get
      {
        return m_SkeletonsUpgrade;
      }
      set
      {
                m_SkeletonsUpgrade = value;
      }
    }

    public bool ShamblersUpgrade
    {
      get
      {
        return m_ShamblersUpgrade;
      }
      set
      {
                m_ShamblersUpgrade = value;
      }
    }

    public bool DEV_ShowActorsStats { get; set; }

    public void ResetToDefaultValues()
    {
            m_DistrictSize = DEFAULT_DISTRICT_SIZE;
            m_MaxCivilians = DEFAULT_MAX_CIVILIANS;
            m_MaxUndeads = DEFAULT_MAX_UNDEADS;
            m_MaxDogs = DEFAULT_MAX_DOGS;
            m_PlayMusic = true;
            m_MusicVolume = 100;
            m_AnimDelay = true;
            m_ShowMinimap = true;
            m_ShowPlayerTagsOnMinimap = true;
            m_EnabledAdvisor = true;
            m_CombatAssistant = false;
            m_SpawnSkeletonChance = DEFAULT_SPAWN_SKELETON_CHANCE;
            m_SpawnZombieChance = DEFAULT_SPAWN_ZOMBIE_CHANCE;
            m_SpawnZombieMasterChance = DEFAULT_SPAWN_ZOMBIE_MASTER_CHANCE;
            m_CitySize = DEFAULT_CITY_SIZE;
            m_NPCCanStarveToDeath = true;
            m_ZombificationChance = DEFAULT_ZOMBIFICATION_CHANCE;
            m_RevealStartingDistrict = true;
            m_AllowUndeadsEvolution = true;
            m_DayZeroUndeadsPercent = DEFAULT_DAY_ZERO_UNDEADS_PERCENT;
            m_ZombieInvasionDailyIncrease = DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE;
            m_StarvedZombificationChance = DEFAULT_STARVED_ZOMBIFICATION_CHANCE;
            m_MaxReincarnations = DEFAULT_MAX_REINCARNATIONS;
            m_CanReincarnateAsRat = false;
            m_CanReincarnateToSewers = false;
            m_IsLivingReincRestricted = false;
            m_Permadeath = false;
            m_DeathScreenshot = true;
            m_AggressiveHungryCivilians = true;
            m_NatGuardFactor = DEFAULT_NATGUARD_FACTOR;
            m_SuppliesDropFactor = DEFAULT_SUPPLIESDROP_FACTOR;
            m_ShowTargets = true;
            m_ShowPlayerTargets = true;
            m_ZupDays = ZupDays.THREE;
            m_RatsUpgrade = false;
            m_SkeletonsUpgrade = false;
            m_ShamblersUpgrade = false;
    }

    public float DifficultyRating(Gameplay.GameFactions.IDs side)
    { // Historically supported factions are civilians and zombies
      var us = Gameplay.GameFactions.From(side);
      float rating = 1f;

      ///////////////////
      // Constant factors.
      // Harder:
      // - Don't reveal starting map : +10%
      // Survivor Easier/Undead Harder:
      // - Disable NPC starvation    : -10%/+10%
      // Harder/Easier:
      // - Nat Guards                : -50% -> +50%
      // - Supplies                  : -50% -> +50%
      // - Zombifieds UpDay 
      //////////////////
      if (!RevealStartingDistrict) rating += 0.1f;    // theoretically affects everyone

      if (!NPCCanStarveToDeath) rating += (!Gameplay.GameFactions.TheCivilians.IsEnemyOf(us) ? -0.1f : 0.1f);  // very arguable
      if (NatGuardFactor != GameOptions.DEFAULT_NATGUARD_FACTOR && Gameplay.GameFactions.TheBlackOps!=us) {
        // blackops are strictly after national guard so they don't care aobut this setting either way
        float k = (float) (NatGuardFactor - GameOptions.DEFAULT_NATGUARD_FACTOR) / (float)GameOptions.DEFAULT_NATGUARD_FACTOR;
        rating += 0.5f*(!Gameplay.GameFactions.TheArmy.IsEnemyOf(us) ? -k : k);
      }
      if (SuppliesDropFactor != GameOptions.DEFAULT_SUPPLIESDROP_FACTOR) {
        // while almost everyone can use medikits, the main influence of this is on food supply
        float k = (float) (SuppliesDropFactor - GameOptions.DEFAULT_SUPPLIESDROP_FACTOR) / (float)GameOptions.DEFAULT_SUPPLIESDROP_FACTOR;
        rating += 0.5f*(!Gameplay.GameFactions.TheCivilians.IsEnemyOf(us) ? -k : k);
      }
      if (ZombifiedsUpgradeDays != GameOptions.ZupDays.THREE) {
        static float DeltaDifficulty(ZupDays x) {
          switch (x) {
            case ZupDays.ONE: return 0.5f;
            case ZupDays.TWO: return 0.25f;
            case ZupDays.FOUR: return -0.1f;
            case ZupDays.FIVE: return -0.2f;
            case ZupDays.SIX: return  -0.3f;
            case ZupDays.SEVEN: return -0.4f;
            case ZupDays.OFF: return -0.5f;
#if DEBUG
            default: throw new InvalidProgramException("unhandled case");
#else
            default: return 0.0f;
#endif
          }
        }

        rating += DeltaDifficulty(ZombifiedsUpgradeDays)*(Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? 0.5f : -0.5f);
      }

      //////////////////
      // Dynamic factors:
      // !reversed for undeads!
      // - Density            : f(mapsize, civs+undeads), +/- 99%   // alpha10.1 removed citysize from difficulty formula
      // - Undeads            : f(undeads/civs, day0, invasion%), +/- 50%
      // - Civilians          : f(zombification%, canstarve&starvedzomb%), +/- 50%
      ////////////////////
      {
#if OBSOLETE
      float reference = (float) Math.Sqrt(DEFAULT_MAX_UNDEADS+ DEFAULT_MAX_CIVILIANS) / (DEFAULT_CITY_SIZE * DEFAULT_DISTRICT_SIZE * DEFAULT_DISTRICT_SIZE);
      float relative_deviation = ((float) Math.Sqrt((double) (MaxCivilians + MaxUndeads)) / (float) (CitySize * DistrictSize * DistrictSize) - reference) / reference;
#else
      float reference = (float) Math.Sqrt(DEFAULT_MAX_UNDEADS+ DEFAULT_MAX_CIVILIANS) / (DEFAULT_DISTRICT_SIZE * DEFAULT_DISTRICT_SIZE);
      float relative_deviation = ((float) Math.Sqrt((double) (MaxCivilians + MaxUndeads)) / (float) (DistrictSize * DistrictSize) - reference) / reference;
#endif
      rating += relative_deviation * (!Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? - 0.99f :  0.99f);
      }

      {
      const float reference = (float)(DEFAULT_MAX_UNDEADS) /(float)(DEFAULT_MAX_CIVILIANS);
      float relative_deviation = ((float) MaxUndeads / (float) MaxCivilians - reference) / reference;
      float relative_deviation_start = (float) (DayZeroUndeadsPercent - DEFAULT_DAY_ZERO_UNDEADS_PERCENT) / (float)DEFAULT_DAY_ZERO_UNDEADS_PERCENT;
      float relative_deviation_speed = (float) (ZombieInvasionDailyIncrease - 5) / 5f;
      float k = (float)(0.3*relative_deviation + 0.05*relative_deviation_start + 0.15*relative_deviation_speed);
      rating += Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? k : -k;
      }

      if (!Session.Get.HasInfection) {  // starvation zombification chance only affects classic and z-war modes.  Does affect difficulty vacuously in RS Alpha 9
        const float reference = (float)(DEFAULT_MAX_CIVILIANS* DEFAULT_ZOMBIFICATION_CHANCE);
        float relative_deviation = ((float) (MaxCivilians * ZombificationChance) - reference) / reference;
        const float reference_scale = (float)(DEFAULT_MAX_CIVILIANS* DEFAULT_STARVED_ZOMBIFICATION_CHANCE);
        float relative_deviation_scale = NPCCanStarveToDeath ? ((float) (MaxCivilians * StarvedZombificationChance) - reference_scale) / reference_scale : -1f;
        float k = (float)(0.3*relative_deviation + 0.2*relative_deviation_scale);
        rating += Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ?  k : -k;
      }

      /////////////
      // Scaling factors.
      // - Disable undeads evolution  : x0.5 / x2
      // - Enable Combat Assistant    : x0.75
      // - Enable permadeath          : x2
      // - Aggressive Hungry Civs     : x0.5 / x2
      // - Rats Upgrade               : x1.10 / x0.90
      // - Skeletons Upgrade          : x1.20 / x0.80
      // - Shamblers Upgrade          : x1.25 / x0.75
      ////////////
      if (!AllowUndeadsEvolution && Session.Get.HasEvolution) rating *= (Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? 0.5f : 2f);
      if (IsCombatAssistantOn) rating *= 0.75f;
      if (IsPermadeathOn) rating *= 2f;
      if (!IsAggressiveHungryCiviliansOn) rating *= (!Gameplay.GameFactions.TheCivilians.IsEnemyOf(us) ? 0.5f : 2f);
      if (Session.Get.HasAllZombies) {
        if (RatsUpgrade) rating *= (Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? 1.1f : 0.9f);
        if (SkeletonsUpgrade) rating *= (Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? 1.2f : 0.8f);
        if (ShamblersUpgrade) rating *= (Gameplay.GameFactions.TheUndeads.IsEnemyOf(us) ? 1.25f : 0.75f);
      }

      return Math.Max(rating, 0.0f);
    }

    public static string Name(IDs option)
    {
      switch (option)
      {
        case IDs.UI_MUSIC:
          return "   (Sfx) Music";
        case IDs.UI_MUSIC_VOLUME:
          return "   (Sfx) Music Volume";
        case IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
          return "   (Gfx) Show Tags on Minimap";
        case IDs.UI_ANIM_DELAY:
          return "   (Gfx) Animations Delay";
        case IDs.UI_SHOW_MINIMAP:
          return "   (Gfx) Show Minimap";
        case IDs.UI_ADVISOR:
          return "  (Help) Enable Advisor";
        case IDs.UI_COMBAT_ASSISTANT:
          return "  (Help) Combat Assistant";
        case IDs.UI_SHOW_TARGETS:
          return "  (Help) Show Other Actors' Targets";
        case IDs.UI_SHOW_PLAYER_TARGETS:
          return "  (Help) Show Player Targets";
        case IDs.GAME_DISTRICT_SIZE:
          return "   (Map) District Map Size";
        case IDs.GAME_MAX_CIVILIANS:
          return "(Living) Max Civilians";
        case IDs.GAME_MAX_DOGS:
          return "(Living) Max Dogs";
        case IDs.GAME_MAX_UNDEADS:
          return "(Undead) Max Undeads";
        case IDs.GAME_SIMULATE_DISTRICTS:
          return "   (Sim) Districts Simulation";
        case IDs.GAME_SIMULATE_SLEEP:
          return "   (Sim) Simulate when Sleeping";
        case IDs.GAME_SIM_THREAD:
          return "   (Sim) Synchronous Simulation";
        case IDs.GAME_SPAWN_SKELETON_CHANCE:
          return "(Undead) Spawn Skeleton chance";
        case IDs.GAME_SPAWN_ZOMBIE_CHANCE:
          return "(Undead) Spawn Zombie chance";
        case IDs.GAME_SPAWN_ZOMBIE_MASTER_CHANCE:
          return "(Undead) Spawn Zombie Master chance";
        case IDs.GAME_CITY_SIZE:
          return "   (Map) City Size";
        case IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
          return "(Living) NPCs can starve to death";
        case IDs.GAME_ZOMBIFICATION_CHANCE:
          return "(Living) Zombification Chance";
        case IDs.GAME_REVEAL_STARTING_DISTRICT:
          return "   (Map) Reveal Starting District";
        case IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
          return "(Undead) Allow Undeads Evolution";
        case IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
          return "(Undead) Day 0 Undeads";
        case IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
          return "(Undead) Invasion Daily Increase";
        case IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
          return "(Living) Starved Zombification";
        case IDs.GAME_MAX_REINCARNATIONS:
          return " (Reinc) Max Reincarnations";
        case IDs.GAME_REINCARNATE_AS_RAT:
          return " (Reinc) Can Reincarnate as Rat";
        case IDs.GAME_REINCARNATE_TO_SEWERS:
          return " (Reinc) Can Reincarnate to Sewers";
        case IDs.GAME_REINC_LIVING_RESTRICTED:
          return " (Reinc) Civilians only Reinc.";
        case IDs.GAME_PERMADEATH:
          return " (Death) Permadeath";
        case IDs.GAME_DEATH_SCREENSHOT:
          return " (Death) Death Screenshot";
        case IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
          return "(Living) Aggressive Hungry Civs";
        case IDs.GAME_NATGUARD_FACTOR:
          return " (Event) National Guard";
        case IDs.GAME_SUPPLIESDROP_FACTOR:
          return " (Event) Supplies Drop";
        case IDs.GAME_UNDEADS_UPGRADE_DAYS:
          return "(Undead) Undeads Skills Upgrade Days";
        case IDs.GAME_RATS_UPGRADE:
          return "(Undead) Rats Skill Upgrade";
        case IDs.GAME_SKELETONS_UPGRADE:
          return "(Undead) Skeletons Skill Upgrade";
        case IDs.GAME_SHAMBLERS_UPGRADE:
          return "(Undead) Shamblers Skill Upgrade";
        default:
          throw new InvalidOperationException("unhandled option");
      }
    }

    // alpha10
    public static string Describe(IDs option)
    {
      switch (option) {
        case IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: return "Allows hungry civilians to attack other people for food.";
        case IDs.GAME_ALLOW_UNDEADS_EVOLUTION: return "ALWAYS OFF IN VTG-VINTAGE MODE.\nAllows undeads to evolve into stronger forms.";
        case IDs.GAME_CITY_SIZE: return "Size of the city grid. The city is a square grid of districts.\nLarger cities are more fun but rapidly increases game saves size and loading time.";
        case IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: return "Percentage of max undeads spawned when the game starts.";
        case IDs.GAME_DEATH_SCREENSHOT: return "Takes a screenshot when you die and save it to the game Config\\Screenshot folder.";
        case IDs.GAME_DISTRICT_SIZE: return "How large are the maps in tiles. Larger maps are more fun but increase game saves size and loading time.";
        case IDs.GAME_MAX_CIVILIANS: return "Maximum number of civilians on a map. More civilians makes the game easier for livings, but slows the game down.";
        case IDs.GAME_MAX_DOGS: return "OPTION IS UNUSED YOU SHOULDNT BE READING THIS :)";
        case IDs.GAME_MAX_REINCARNATIONS: return "Number of times you can reincarnate in a game after your character dies.\nSet it to 0 to disable reincarnation altogether.";
        case IDs.GAME_MAX_UNDEADS: return "Maximum number of undeads on a map. More undeads makes the game more challenging for livings, but slows the game down.";
        case IDs.GAME_NATGUARD_FACTOR: return "Affects how likely the National Guard event happens.\n100 is default, 0 to disable.";
        case IDs.GAME_NPC_CAN_STARVE_TO_DEATH: return "When NPCs are starving they can die. When disabled ai characters will never die from hunger.";
        case IDs.GAME_PERMADEATH: return "Deletes your saved game when you die so you can't reload your way out. Extra challenge and tension.";
        case IDs.GAME_RATS_UPGRADE: return "ALWAYS OFF IN VTG-VINTAGE MODE.\nCan Rats type of undeads upgrade their skills like other undeads.\nNot recommended unless you want super annoying rats.";
        case IDs.GAME_REVEAL_STARTING_DISTRICT: return "You start the game with knowing parts of the map you start in.";
        case IDs.GAME_REINC_LIVING_RESTRICTED: return "Limit choices of reincarnations as livings to civilians only. If disabled allow you to reincarnte into all kinds of livings.";
        case IDs.GAME_REINCARNATE_AS_RAT: return "Enables the possibility to reincarnate into a zombie rat.";
        case IDs.GAME_REINCARNATE_TO_SEWERS: return "Enables the possibility to reincarnate into the sewers.";
        case IDs.GAME_SHAMBLERS_UPGRADE: return "ALWAYS OFF IN VTG-VINTAGE MODE.\nCan Shamblers type of undeads upgrade their skills like other undeads.";
        case IDs.GAME_SKELETONS_UPGRADE: return "ALWAYS OFF IN VTG-VINTAGE MODE.\nCan Skeletons type of undeads upgrade their skills like other undeads.";
        case IDs.GAME_SIMULATE_DISTRICTS: return "The game simulates what is happening in districts around you. You should keep this option maxed for better gameplay.\nWhen the simulation happens depends on other sim options.";
        case IDs.GAME_SIMULATE_SLEEP: return "Performs simulation when you are sleeping. Recommended if synchronous sim is off.";
        case IDs.GAME_SIM_THREAD: return "Performs simulation in a separate thread while you are playing. Recommended unless the game is unstable.";
        case IDs.GAME_SPAWN_SKELETON_CHANCE: return "YOU SHOULDNT BE READING THIS :)";
        case IDs.GAME_SPAWN_ZOMBIE_CHANCE: return "YOU SHOULDNT BE READING THIS :)";
        case IDs.GAME_SPAWN_ZOMBIE_MASTER_CHANCE: return "YOU SHOULDNT BE READING THIS :)";
        case IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: return "ONLY IN STD-STANDARD MODE.\nIf NPCs can starve to death, chances of turning into a zombie.";
        case IDs.GAME_SUPPLIESDROP_FACTOR: return "Affects how likely the supplies drop event happens.\n100 is default, 0 to disable.";
        case IDs.GAME_UNDEADS_UPGRADE_DAYS: return "How often can undeads upgrade their skills. They usually upgrade at a slower pace than livings.";
        case IDs.GAME_ZOMBIFICATION_CHANCE: return "ONLY IN STD-STANDARD MODE.\nSome undeads have the ability to turn their living victims into zombies after killing them.\nThis option control the chances of zombification. Changing this value has a large impact on game difficulty.\nException: the player is always checked for zombification when killed in all game modes.";
        case IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: return "The zombies invasion increases in size each day, to fill up to Max Undeads on a map.";
        case IDs.UI_ANIM_DELAY: return "Enable or disable delays when showing actions or events on the map.\nYou should keep it on when learning the game and then disable it for a faster play.";
        case IDs.UI_MUSIC: return "Enable or disable ingame musics. Musics are not essential for gameplay. If you can't hear music, try the configuration program.";
        case IDs.UI_MUSIC_VOLUME: return "Music volume.";
        case IDs.UI_SHOW_MINIMAP: return "Display or hide the minimap.\nThe minimap could potentially crash the game on some very old graphics cards.";
        case IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: return "Highlight tags painted by the player as yellow dots in the minimap.";
        case IDs.UI_ADVISOR: return "Enable or disable the ingame hints system. The advisor helps you learn the game for the living side.\nIt will only tell you hints it didn't already tell you.\nAll hints are also available from the main menu.";
        case IDs.UI_COMBAT_ASSISTANT: return "When enabled draws a colored circle icon on your enemies.\nGreen = you can safely act twice before your enemy\nYellow = your enemy will act after you\nRed = your enemy will act twice after you";
        case IDs.UI_SHOW_TARGETS: return "When mouse over an actor, will draw icons on actors that are targeting, are targeted or are in group with this actor.";
        case IDs.UI_SHOW_PLAYER_TARGETS: return "Will draw icons on actors that are targeting you.";
        default: throw new InvalidOperationException("unhandled option");
      }
    }

    public static string Name(ReincMode mode)
    {
      switch (mode) {
        case ReincMode.RANDOM_FOLLOWER: return "Random Follower";
        case ReincMode.KILLER: return "Your Killer";
        case ReincMode.ZOMBIFIED: return "Your Zombie Self";
        case ReincMode.RANDOM_LIVING: return "Random Living";
        case ReincMode.RANDOM_UNDEAD: return "Random Undead";
        case ReincMode.RANDOM_ACTOR: return "Random Actor";
        default: throw new InvalidOperationException("unhandled ReincMode");
      }
    }

    public static string Name(SimRatio ratio)
    {
      switch (ratio) {
        case SimRatio.OFF: return "OFF";
        case SimRatio.ONE_QUARTER: return "25%";
        case SimRatio.ONE_THIRD: return "33%";
        case SimRatio.HALF: return "50%";
        case SimRatio.TWO_THIRDS: return "66%";
        case SimRatio.THREE_QUARTER: return "75%";
        case SimRatio.FULL: return "FULL";
        default: throw new InvalidOperationException("unhandled simRatio");
      }
    }

    public static float SimRatioToFloat(SimRatio ratio)
    {
      switch (ratio) {
        case SimRatio.OFF: return 0.0f;
        case SimRatio.ONE_QUARTER: return 0.25f;
        case SimRatio.ONE_THIRD: return 0.3333333f;
        case SimRatio.HALF: return 0.5f;
        case SimRatio.TWO_THIRDS: return 0.6666667f;
        case SimRatio.THREE_QUARTER: return 0.75f;
        case SimRatio.FULL: return 1f;
        default: throw new InvalidOperationException("unhandled simRatio");
      }
    }

    public static string Name(ZupDays d)
    {
      switch (d) {
        case ZupDays.ONE: return "1 d";
        case ZupDays.TWO: return "2 d";
        case ZupDays.THREE: return "3 d";
        case ZupDays.FOUR: return "4 d";
        case ZupDays.FIVE: return "5 d";
        case ZupDays.SIX: return "6 d";
        case ZupDays.SEVEN: return "7 d";
        case ZupDays.OFF: return "OFF";
        default: throw new InvalidOperationException("unhandled zupDays");
      }
    }

    public static bool IsZupDay(ZupDays d, int day)
    {
      switch (d) {
        case ZupDays.ONE: return true;
        case ZupDays.TWO: return day % 2 == 0;
        case ZupDays.THREE: return day % 3 == 0;
        case ZupDays.FOUR: return day % 4 == 0;
        case ZupDays.FIVE: return day % 5 == 0;
        case ZupDays.SIX: return day % 6 == 0;
        case ZupDays.SEVEN: return day % 7 == 0;
        default: return false;
      }
    }

    public string DescribeValue(IDs option)
    {
      switch (option)
      {
        case IDs.UI_MUSIC:
          return !PlayMusic ? "OFF" : "ON ";
        case IDs.UI_MUSIC_VOLUME:
          return MusicVolume.ToString() + "%";
        case IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
          return !ShowPlayerTagsOnMinimap ? "NO " : "YES";
        case IDs.UI_ANIM_DELAY:
          return !IsAnimDelayOn ? "OFF" : "ON ";
        case IDs.UI_SHOW_MINIMAP:
          return !IsMinimapOn ? "OFF" : "ON ";
        case IDs.UI_ADVISOR:
          return !IsAdvisorEnabled ? "NO " : "YES";
        case IDs.UI_COMBAT_ASSISTANT:
          return !IsCombatAssistantOn ? "OFF   (default OFF)" : "ON    (default OFF)";
        case IDs.UI_SHOW_TARGETS:
          return !ShowTargets ? "OFF   (default ON)" : "ON    (default ON)";
        case IDs.UI_SHOW_PLAYER_TARGETS:
          return !ShowPlayerTargets ? "OFF   (default ON)" : "ON    (default ON)";
        case IDs.GAME_DISTRICT_SIZE:
          return string.Format("{0:D2}*   (default {1:D2})", (object)DistrictSize, (object) DEFAULT_DISTRICT_SIZE);
        case IDs.GAME_MAX_CIVILIANS:
          return string.Format("{0:D3}*  (default {1:D3})", (object)MaxCivilians, (object) DEFAULT_MAX_CIVILIANS);
        case IDs.GAME_MAX_DOGS:
          return string.Format("{0:D3}*  (default {1:D3})", (object)MaxDogs, (object) DEFAULT_MAX_DOGS);
        case IDs.GAME_MAX_UNDEADS:
          return string.Format("{0:D3}*  (default {1:D3})", (object)MaxUndeads, (object) DEFAULT_MAX_UNDEADS);
        case IDs.GAME_SIMULATE_DISTRICTS:
          return string.Format("{0,-4}* (default {1})", (object) GameOptions.Name(SimRatio.FULL), (object) GameOptions.Name(SimRatio.FULL));
        case IDs.GAME_SIMULATE_SLEEP:
          return "NO [hardcoded]";
        case IDs.GAME_SIM_THREAD:
          return "YES [hardcoded]";
        case IDs.GAME_CITY_SIZE:
          return string.Format("{0:D2}*   (default {1:D2})", (object)CitySize, (object) DEFAULT_CITY_SIZE);
        case IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
          return !NPCCanStarveToDeath ? "NO    (default YES)" : "YES   (default YES)";
        case IDs.GAME_ZOMBIFICATION_CHANCE:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)ZombificationChance, (object) 100);
        case IDs.GAME_REVEAL_STARTING_DISTRICT:
          return !RevealStartingDistrict ? "NO    (default YES)" : "YES   (default YES)";
        case IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
          return !AllowUndeadsEvolution ? "NO    (default YES)" : "YES   (default YES)";
        case IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)DayZeroUndeadsPercent, (object) DEFAULT_DAY_ZERO_UNDEADS_PERCENT);
        case IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)ZombieInvasionDailyIncrease, (object) DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE);
        case IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)StarvedZombificationChance, (object) DEFAULT_STARVED_ZOMBIFICATION_CHANCE);
        case IDs.GAME_MAX_REINCARNATIONS:
          return string.Format("{0:D3}   (default {1:D3})", (object)MaxReincarnations, (object) DEFAULT_MAX_REINCARNATIONS);
        case IDs.GAME_REINCARNATE_AS_RAT:
          return !CanReincarnateAsRat ? "NO    (default NO)" : "YES   (default NO)";
        case IDs.GAME_REINCARNATE_TO_SEWERS:
          return !CanReincarnateToSewers ? "NO    (default NO)" : "YES   (default NO)";
        case IDs.GAME_REINC_LIVING_RESTRICTED:
          return !IsLivingReincRestricted ? "NO    (default NO)" : "YES   (default NO)";
        case IDs.GAME_PERMADEATH:
          return !IsPermadeathOn ? "NO    (default NO)" : "YES   (default NO)";
        case IDs.GAME_DEATH_SCREENSHOT:
          return !IsDeathScreenshotOn ? "NO    (default YES)" : "YES   (default YES)";
        case IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
          return !IsAggressiveHungryCiviliansOn ? "OFF   (default ON)" : "ON    (default ON)";
        case IDs.GAME_NATGUARD_FACTOR:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)NatGuardFactor, (object) DEFAULT_NATGUARD_FACTOR);
        case IDs.GAME_SUPPLIESDROP_FACTOR:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)SuppliesDropFactor, (object) DEFAULT_SUPPLIESDROP_FACTOR);
        case IDs.GAME_UNDEADS_UPGRADE_DAYS:
          return string.Format("{0:D3}   (default {1:D3})", (object) GameOptions.Name(ZombifiedsUpgradeDays), (object) GameOptions.Name(ZupDays.THREE));
        case IDs.GAME_RATS_UPGRADE:
          return !RatsUpgrade ? "NO    (default NO)" : "YES   (default NO)";
        case IDs.GAME_SKELETONS_UPGRADE:
          return !SkeletonsUpgrade ? "NO    (default NO)" : "YES   (default NO)";
        case IDs.GAME_SHAMBLERS_UPGRADE:
          return !ShamblersUpgrade ? "NO    (default NO)" : "YES   (default NO)";
        default:
          return "???";
      }
    }

    public static void Save(GameOptions options, string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving options...");
	  filepath.BinarySerialize(options);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving options... done!");
    }

    public static GameOptions Load(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading options...");
      GameOptions gameOptions;
      try {
#if LINUX
        filepath = filepath.Replace("\\", "/");
#endif
	    gameOptions = filepath.BinaryDeserialize<GameOptions>();
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load options (no custom options?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "returning default values.");
        gameOptions = new GameOptions();
        gameOptions.ResetToDefaultValues();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading options... done!");
      return gameOptions;
    }

    public enum IDs
    {
      UI_MUSIC,
      UI_MUSIC_VOLUME,
      UI_SHOW_PLAYER_TAG_ON_MINIMAP,
      UI_ANIM_DELAY,
      UI_SHOW_MINIMAP,
      UI_ADVISOR,
      UI_COMBAT_ASSISTANT,
      UI_SHOW_TARGETS,
      UI_SHOW_PLAYER_TARGETS,
      GAME_DISTRICT_SIZE,
      GAME_MAX_CIVILIANS,
      GAME_MAX_DOGS,
      GAME_MAX_UNDEADS,
      GAME_SIMULATE_DISTRICTS,
      GAME_SIMULATE_SLEEP,
      GAME_SIM_THREAD,
      GAME_SPAWN_SKELETON_CHANCE,
      GAME_SPAWN_ZOMBIE_CHANCE,
      GAME_SPAWN_ZOMBIE_MASTER_CHANCE,
      GAME_CITY_SIZE,
      GAME_NPC_CAN_STARVE_TO_DEATH,
      GAME_ZOMBIFICATION_CHANCE,
      GAME_REVEAL_STARTING_DISTRICT,
      GAME_ALLOW_UNDEADS_EVOLUTION,
      GAME_DAY_ZERO_UNDEADS_PERCENT,
      GAME_ZOMBIE_INVASION_DAILY_INCREASE,
      GAME_STARVED_ZOMBIFICATION_CHANCE,
      GAME_MAX_REINCARNATIONS,
      GAME_REINCARNATE_AS_RAT,
      GAME_REINCARNATE_TO_SEWERS,
      GAME_REINC_LIVING_RESTRICTED,
      GAME_PERMADEATH,
      GAME_DEATH_SCREENSHOT,
      GAME_AGGRESSIVE_HUNGRY_CIVILIANS,
      GAME_NATGUARD_FACTOR,
      GAME_SUPPLIESDROP_FACTOR,
      GAME_UNDEADS_UPGRADE_DAYS,
      GAME_RATS_UPGRADE,
      GAME_SKELETONS_UPGRADE,
      GAME_SHAMBLERS_UPGRADE,
    }

    public enum ZupDays
    {
      ONE = 0,
      TWO = 1,
      THREE = 2,
      FOUR = 3,
      FIVE = 4,
      SIX = 5,
      SEVEN = 6,
      OFF = 7,
      _COUNT = 8,
    }

    public enum SimRatio
    {
      OFF = 0,
      ONE_QUARTER = 1,
      ONE_THIRD = 2,
      HALF = 3,
      TWO_THIRDS = 4,
      THREE_QUARTER = 5,
      FULL = 6,
      _COUNT = 7,
    }

    public enum ReincMode
    {
      RANDOM_FOLLOWER = 0,
      KILLER = 1,
      ZOMBIFIED = 2,
      RANDOM_LIVING = 3,
      RANDOM_UNDEAD = 4,
      RANDOM_ACTOR = 5,
      _COUNT = 6,
    }
  }

  internal static class GameOptions_ext
  {
    public static bool HighDetail(this GameOptions.SimRatio x, int turn)
    {
      switch(x) {
        case GameOptions.SimRatio.OFF: return true;
        case GameOptions.SimRatio.ONE_QUARTER: return turn % 4 != 0;
        case GameOptions.SimRatio.ONE_THIRD: return turn % 3 != 0;
        case GameOptions.SimRatio.HALF: return turn % 2 == 1;
        case GameOptions.SimRatio.TWO_THIRDS: return turn % 3 == 2;
        case GameOptions.SimRatio.THREE_QUARTER: return turn % 4 == 3;
#if DEBUG
        case GameOptions.SimRatio.FULL: return false;
        default: throw new ArgumentOutOfRangeException("unhandled simRatio");
#else
        default: return false;
#endif
      }
    }
  }
}
