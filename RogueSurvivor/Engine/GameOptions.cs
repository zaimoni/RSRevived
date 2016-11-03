// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.GameOptions
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define STABLE_SIM_OPTIONAL

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics.Contracts;
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

  [Serializable]
  internal struct GameOptions
  {
    public const int DEFAULT_DISTRICT_SIZE = 50;
    public const int DEFAULT_MAX_CIVILIANS = 25;
    public const int DEFAULT_MAX_DOGS = 0;
    public const int DEFAULT_MAX_UNDEADS = 100;
    public const int DEFAULT_SPAWN_SKELETON_CHANCE = 60;
    public const int DEFAULT_SPAWN_ZOMBIE_CHANCE = 30;
    public const int DEFAULT_SPAWN_ZOMBIE_MASTER_CHANCE = 10;
    public const int DEFAULT_CITY_SIZE = 5;
    public const GameOptions.SimRatio DEFAULT_SIM_DISTRICTS = GameOptions.SimRatio.FULL;
    public const int DEFAULT_ZOMBIFICATION_CHANCE = 100;
    public const int DEFAULT_DAY_ZERO_UNDEADS_PERCENT = 30;
    public const int DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE = 5;
    public const int DEFAULT_STARVED_ZOMBIFICATION_CHANCE = 50;
    public const int DEFAULT_MAX_REINCARNATIONS = 1;
    public const int DEFAULT_NATGUARD_FACTOR = 100;
    public const int DEFAULT_SUPPLIESDROP_FACTOR = 100;
    public const GameOptions.ZupDays DEFAULT_ZOMBIFIEDS_UPGRADE_DAYS = GameOptions.ZupDays.THREE;
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
#if STABLE_SIM_OPTIONAL
    private GameOptions.SimRatio m_SimulateDistricts;
    private float m_cachedSimRatioFloat;
    private bool m_SimulateWhenSleeping;
    private bool m_SimThread;
#endif
    private bool m_ShowPlayerTagsOnMinimap;
    private int m_SpawnSkeletonChance;
    private int m_SpawnZombieChance;
    private int m_SpawnZombieMasterChance;
    private int m_CitySize;
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
    private GameOptions.ZupDays m_ZupDays;
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

    public int MusicVolume
    {
      get
      {
        return m_MusicVolume;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 100)
          value = 100;
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

    public int CitySize
    {
      get
      {
        return m_CitySize;
      }
      set
      {
        if (value < 3)
          value = 3;
        if (value > 6)
          value = 6;
                m_CitySize = value;
      }
    }

    public int MaxCivilians
    {
      get
      {
        return m_MaxCivilians;
      }
      set
      {
        if (value < 10)
          value = 10;
        if (value > 75)
          value = 75;
                m_MaxCivilians = value;
      }
    }

    public int MaxDogs
    {
      get
      {
        return m_MaxDogs;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 75)
          value = 75;
                m_MaxDogs = value;
      }
    }

    public int MaxUndeads
    {
      get
      {
        return m_MaxUndeads;
      }
      set
      {
        if (value < 10)
          value = 10;
        if (value > 200)
          value = 200;
                m_MaxUndeads = value;
      }
    }

    public int SpawnSkeletonChance
    {
      get
      {
        return m_SpawnSkeletonChance;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 100)
          value = 100;
                m_SpawnSkeletonChance = value;
      }
    }

    public int SpawnZombieChance
    {
      get
      {
        return m_SpawnZombieChance;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 100)
          value = 100;
                m_SpawnZombieChance = value;
      }
    }

    public int SpawnZombieMasterChance
    {
      get
      {
        return m_SpawnZombieMasterChance;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 100)
          value = 100;
                m_SpawnZombieMasterChance = value;
      }
    }

#if STABLE_SIM_OPTIONAL
    public GameOptions.SimRatio SimulateDistricts
    {
      get{
#if STABLE_SIM_OPTIONAL
        return m_SimulateDistricts;
#else
        return GameOptions.SimRatio.FULL;
#endif
      }
#if STABLE_SIM_OPTIONAL
      set {
        m_SimulateDistricts = value;
        m_cachedSimRatioFloat = GameOptions.SimRatioToFloat(m_SimulateDistricts);
      }
#endif
    }
#endif

#if STABLE_SIM_OPTIONAL
    public float SimRatioFloat
    {
      get {
#if STABLE_SIM_OPTIONAL
        return m_cachedSimRatioFloat;
#else
        return GameOptions.SimRatioToFloat(GameOptions.SimRatio.FULL);
#endif
      }
    }
#endif

#if STABLE_SIM_OPTIONAL
    public bool SimulateWhenSleeping
    {
      get {
#if STABLE_SIM_OPTIONAL
        return m_SimulateWhenSleeping;
#else
        return false;
#endif
      }
#if STABLE_SIM_OPTIONAL
      set {
        m_SimulateWhenSleeping = value;
      }
#endif
    }
#endif

#if STABLE_SIM_OPTIONAL
    public bool IsSimON
    {
      get {
#if STABLE_SIM_OPTIONAL
        return m_SimulateDistricts != GameOptions.SimRatio._FIRST;
#else
        return GameOptions.SimRatio.FULL != GameOptions.SimRatio._FIRST;
#endif
      }
    }
#endif

#if STABLE_SIM_OPTIONAL
    public bool SimThread
    {
      get {
#if STABLE_SIM_OPTIONAL
        return m_SimThread;
#else
        return true;
#endif
      }
#if STABLE_SIM_OPTIONAL
      set {
        m_SimThread = value;
      }
#endif
    }
#endif

    public int DistrictSize
    {
      get
      {
        return m_DistrictSize;
      }
      set
      {
        if (value < 30)
          value = 30;
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
      get
      {
        return m_ZombificationChance;
      }
      set
      {
        if (value < 10)
          value = 10;
        if (value > 100)
          value = 100;
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
      get
      {
        return m_DayZeroUndeadsPercent;
      }
      set
      {
        if (value < 10)
          value = 10;
        if (value > 100)
          value = 100;
                m_DayZeroUndeadsPercent = value;
      }
    }

    public int ZombieInvasionDailyIncrease
    {
      get
      {
        return m_ZombieInvasionDailyIncrease;
      }
      set
      {
        if (value < 1)
          value = 1;
        if (value > 20)
          value = 20;
                m_ZombieInvasionDailyIncrease = value;
      }
    }

    public int StarvedZombificationChance
    {
      get
      {
        return m_StarvedZombificationChance;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 100)
          value = 100;
                m_StarvedZombificationChance = value;
      }
    }

    public int MaxReincarnations
    {
      get
      {
        return m_MaxReincarnations;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 7)
          value = 7;
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

    public int NatGuardFactor
    {
      get
      {
        return m_NatGuardFactor;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 200)
          value = 200;
                m_NatGuardFactor = value;
      }
    }

    public int SuppliesDropFactor
    {
      get
      {
        return m_SuppliesDropFactor;
      }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 200)
          value = 200;
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

    public GameOptions.ZupDays ZombifiedsUpgradeDays
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
#if STABLE_SIM_OPTIONAL
            SimulateDistricts = GameOptions.SimRatio.FULL;
            m_SimulateWhenSleeping = false;
            m_SimThread = true;
#endif
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
            m_ZupDays = GameOptions.ZupDays.THREE;
            m_RatsUpgrade = false;
            m_SkeletonsUpgrade = false;
            m_ShamblersUpgrade = false;
    }

    public static string Name(GameOptions.IDs option)
    {
      switch (option)
      {
        case GameOptions.IDs.UI_MUSIC:
          return "   (Sfx) Music";
        case GameOptions.IDs.UI_MUSIC_VOLUME:
          return "   (Sfx) Music Volume";
        case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
          return "   (Gfx) Show Tags on Minimap";
        case GameOptions.IDs.UI_ANIM_DELAY:
          return "   (Gfx) Animations Delay";
        case GameOptions.IDs.UI_SHOW_MINIMAP:
          return "   (Gfx) Show Minimap";
        case GameOptions.IDs.UI_ADVISOR:
          return "  (Help) Enable Advisor";
        case GameOptions.IDs.UI_COMBAT_ASSISTANT:
          return "  (Help) Combat Assistant";
        case GameOptions.IDs.UI_SHOW_TARGETS:
          return "  (Help) Show Actor Targets";
        case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS:
          return "  (Help) Always Show Player Targets";
        case GameOptions.IDs.GAME_DISTRICT_SIZE:
          return "   (Map) District Map Size";
        case GameOptions.IDs.GAME_MAX_CIVILIANS:
          return "(Living) Max Civilians";
        case GameOptions.IDs.GAME_MAX_DOGS:
          return "(Living) Max Dogs";
        case GameOptions.IDs.GAME_MAX_UNDEADS:
          return "(Undead) Max Undeads";
        case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
          return "   (Sim) Districts Simulation";
        case GameOptions.IDs.GAME_SIMULATE_SLEEP:
          return "   (Sim) Simulate when Sleeping";
        case GameOptions.IDs.GAME_SIM_THREAD:
          return "   (Sim) Synchronous Simulation";
        case GameOptions.IDs.GAME_SPAWN_SKELETON_CHANCE:
          return "(Undead) Spawn Skeleton chance";
        case GameOptions.IDs.GAME_SPAWN_ZOMBIE_CHANCE:
          return "(Undead) Spawn Zombie chance";
        case GameOptions.IDs.GAME_SPAWN_ZOMBIE_MASTER_CHANCE:
          return "(Undead) Spawn Zombie Master chance";
        case GameOptions.IDs.GAME_CITY_SIZE:
          return "   (Map) City Size";
        case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
          return "(Living) NPCs can starve to death";
        case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE:
          return "(Living) Zombification Chance";
        case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT:
          return "   (Map) Reveal Starting District";
        case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
          return "(Undead) Allow Undeads Evolution";
        case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
          return "(Undead) Day 0 Undeads";
        case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
          return "(Undead) Invasion Daily Increase";
        case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
          return "(Living) Starved Zombification";
        case GameOptions.IDs.GAME_MAX_REINCARNATIONS:
          return " (Reinc) Max Reincarnations";
        case GameOptions.IDs.GAME_REINCARNATE_AS_RAT:
          return " (Reinc) Can Reincarnate as Rat";
        case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS:
          return " (Reinc) Can Reincarnate to Sewers";
        case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED:
          return " (Reinc) Civilians only Reinc.";
        case GameOptions.IDs.GAME_PERMADEATH:
          return " (Death) Permadeath";
        case GameOptions.IDs.GAME_DEATH_SCREENSHOT:
          return " (Death) Death Screenshot";
        case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
          return "(Living) Aggressive Hungry Civs.";
        case GameOptions.IDs.GAME_NATGUARD_FACTOR:
          return " (Event) National Guard";
        case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR:
          return " (Event) Supplies Drop";
        case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
          return "(Undead) Undeads Skills Upgrade Days";
        case GameOptions.IDs.GAME_RATS_UPGRADE:
          return "(Undead) Rats Skill Upgrade";
        case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
          return "(Undead) Skeletons Skill Upgrade";
        case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
          return "(Undead) Shamblers Skill Upgrade";
        default:
          throw new ArgumentOutOfRangeException("unhandled option");
      }
    }

    public static string Name(GameOptions.ReincMode mode)
    {
      switch (mode)
      {
        case GameOptions.ReincMode.RANDOM_FOLLOWER:
          return "Random Follower";
        case GameOptions.ReincMode.KILLER:
          return "Your Killer";
        case GameOptions.ReincMode.ZOMBIFIED:
          return "Your Zombie Self";
        case GameOptions.ReincMode.RANDOM_LIVING:
          return "Random Living";
        case GameOptions.ReincMode.RANDOM_UNDEAD:
          return "Random Undead";
        case GameOptions.ReincMode.RANDOM_ACTOR:
          return "Random Actor";
        default:
          throw new ArgumentOutOfRangeException("unhandled ReincMode");
      }
    }

    public static string Name(GameOptions.SimRatio ratio)
    {
      switch (ratio)
      {
        case GameOptions.SimRatio._FIRST:
          return "OFF";
        case GameOptions.SimRatio.ONE_QUARTER:
          return "25%";
        case GameOptions.SimRatio.ONE_THIRD:
          return "33%";
        case GameOptions.SimRatio.HALF:
          return "50%";
        case GameOptions.SimRatio.TWO_THIRDS:
          return "66%";
        case GameOptions.SimRatio.THREE_QUARTER:
          return "75%";
        case GameOptions.SimRatio.FULL:
          return "FULL";
        default:
          throw new ArgumentOutOfRangeException("unhandled simRatio");
      }
    }

    public static float SimRatioToFloat(GameOptions.SimRatio ratio)
    {
      switch (ratio)
      {
        case GameOptions.SimRatio._FIRST:
          return 0.0f;
        case GameOptions.SimRatio.ONE_QUARTER:
          return 0.25f;
        case GameOptions.SimRatio.ONE_THIRD:
          return 0.3333333f;
        case GameOptions.SimRatio.HALF:
          return 0.5f;
        case GameOptions.SimRatio.TWO_THIRDS:
          return 0.6666667f;
        case GameOptions.SimRatio.THREE_QUARTER:
          return 0.75f;
        case GameOptions.SimRatio.FULL:
          return 1f;
        default:
          throw new ArgumentOutOfRangeException("unhandled simRatio");
      }
    }

    public static string Name(GameOptions.ZupDays d)
    {
      switch (d)
      {
        case GameOptions.ZupDays._FIRST:
          return "1 d";
        case GameOptions.ZupDays.TWO:
          return "2 d";
        case GameOptions.ZupDays.THREE:
          return "3 d";
        case GameOptions.ZupDays.FOUR:
          return "4 d";
        case GameOptions.ZupDays.FIVE:
          return "5 d";
        case GameOptions.ZupDays.SIX:
          return "6 d";
        case GameOptions.ZupDays.SEVEN:
          return "7 d";
        case GameOptions.ZupDays.OFF:
          return "OFF";
        default:
          throw new ArgumentOutOfRangeException("unhandled zupDays");
      }
    }

    public static bool IsZupDay(GameOptions.ZupDays d, int day)
    {
      switch (d)
      {
        case GameOptions.ZupDays._FIRST:
          return true;
        case GameOptions.ZupDays.TWO:
          return day % 2 == 0;
        case GameOptions.ZupDays.THREE:
          return day % 3 == 0;
        case GameOptions.ZupDays.FOUR:
          return day % 4 == 0;
        case GameOptions.ZupDays.FIVE:
          return day % 5 == 0;
        case GameOptions.ZupDays.SIX:
          return day % 6 == 0;
        case GameOptions.ZupDays.SEVEN:
          return day % 7 == 0;
        default:
          return false;
      }
    }

    public string DescribeValue(GameMode mode, GameOptions.IDs option)
    {
      switch (option)
      {
        case GameOptions.IDs.UI_MUSIC:
          return !PlayMusic ? "OFF" : "ON ";
        case GameOptions.IDs.UI_MUSIC_VOLUME:
          return MusicVolume.ToString() + "%";
        case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
          return !ShowPlayerTagsOnMinimap ? "NO " : "YES";
        case GameOptions.IDs.UI_ANIM_DELAY:
          return !IsAnimDelayOn ? "OFF" : "ON ";
        case GameOptions.IDs.UI_SHOW_MINIMAP:
          return !IsMinimapOn ? "OFF" : "ON ";
        case GameOptions.IDs.UI_ADVISOR:
          return !IsAdvisorEnabled ? "NO " : "YES";
        case GameOptions.IDs.UI_COMBAT_ASSISTANT:
          return !IsCombatAssistantOn ? "OFF   (default OFF)" : "ON    (default OFF)";
        case GameOptions.IDs.UI_SHOW_TARGETS:
          return !ShowTargets ? "OFF   (default ON)" : "ON    (default ON)";
        case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS:
          return !ShowPlayerTargets ? "OFF   (default ON)" : "ON    (default ON)";
        case GameOptions.IDs.GAME_DISTRICT_SIZE:
          return string.Format("{0:D2}*   (default {1:D2})", (object)DistrictSize, (object) DEFAULT_DISTRICT_SIZE);
        case GameOptions.IDs.GAME_MAX_CIVILIANS:
          return string.Format("{0:D3}*  (default {1:D3})", (object)MaxCivilians, (object) DEFAULT_MAX_CIVILIANS);
        case GameOptions.IDs.GAME_MAX_DOGS:
          return string.Format("{0:D3}*  (default {1:D3})", (object)MaxDogs, (object) DEFAULT_MAX_DOGS);
        case GameOptions.IDs.GAME_MAX_UNDEADS:
          return string.Format("{0:D3}*  (default {1:D3})", (object)MaxUndeads, (object) DEFAULT_MAX_UNDEADS);
        case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
#if STABLE_SIM_OPTIONAL
          return string.Format("{0,-4}* (default {1})", (object) GameOptions.Name(SimulateDistricts), (object) GameOptions.Name(GameOptions.SimRatio.FULL));
#else
          return string.Format("{0,-4}* (default {1})", (object) GameOptions.Name(GameOptions.SimRatio.FULL), (object) GameOptions.Name(GameOptions.SimRatio.FULL));
#endif
        case GameOptions.IDs.GAME_SIMULATE_SLEEP:
#if STABLE_SIM_OPTIONAL
          return !SimulateWhenSleeping ? "NO*   (default NO)" : "YES*  (default NO)";
#else
          return "NO*   (default NO)";
#endif
        case GameOptions.IDs.GAME_SIM_THREAD:
#if STABLE_SIM_OPTIONAL
          return !SimThread ? "NO*   (default YES)" : "YES*  (default YES)";
#else
          return "YES*  (default YES)";
#endif
        case GameOptions.IDs.GAME_CITY_SIZE:
          return string.Format("{0:D2}*   (default {1:D2})", (object)CitySize, (object) DEFAULT_CITY_SIZE);
        case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
          return !NPCCanStarveToDeath ? "NO    (default YES)" : "YES   (default YES)";
        case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)ZombificationChance, (object) 100);
        case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT:
          return !RevealStartingDistrict ? "NO    (default YES)" : "YES   (default YES)";
        case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
          if (mode == GameMode.GM_VINTAGE)
            return "---";
          return !AllowUndeadsEvolution ? "NO    (default YES)" : "YES   (default YES)";
        case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)DayZeroUndeadsPercent, (object) DEFAULT_DAY_ZERO_UNDEADS_PERCENT);
        case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)ZombieInvasionDailyIncrease, (object) DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE);
        case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)StarvedZombificationChance, (object) DEFAULT_STARVED_ZOMBIFICATION_CHANCE);
        case GameOptions.IDs.GAME_MAX_REINCARNATIONS:
          return string.Format("{0:D3}   (default {1:D3})", (object)MaxReincarnations, (object) DEFAULT_MAX_REINCARNATIONS);
        case GameOptions.IDs.GAME_REINCARNATE_AS_RAT:
          return !CanReincarnateAsRat ? "NO    (default NO)" : "YES   (default NO)";
        case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS:
          return !CanReincarnateToSewers ? "NO    (default NO)" : "YES   (default NO)";
        case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED:
          return !IsLivingReincRestricted ? "NO    (default NO)" : "YES   (default NO)";
        case GameOptions.IDs.GAME_PERMADEATH:
          return !IsPermadeathOn ? "NO    (default NO)" : "YES   (default NO)";
        case GameOptions.IDs.GAME_DEATH_SCREENSHOT:
          return !IsDeathScreenshotOn ? "NO    (default YES)" : "YES   (default YES)";
        case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
          return !IsAggressiveHungryCiviliansOn ? "OFF   (default ON)" : "ON    (default ON)";
        case GameOptions.IDs.GAME_NATGUARD_FACTOR:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)NatGuardFactor, (object) DEFAULT_NATGUARD_FACTOR);
        case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR:
          return string.Format("{0:D3}%  (default {1:D3}%)", (object)SuppliesDropFactor, (object) DEFAULT_SUPPLIESDROP_FACTOR);
        case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
          return string.Format("{0:D3}   (default {1:D3})", (object) GameOptions.Name(ZombifiedsUpgradeDays), (object) GameOptions.Name(GameOptions.ZupDays.THREE));
        case GameOptions.IDs.GAME_RATS_UPGRADE:
          if (mode == GameMode.GM_VINTAGE)
            return "---";
          return !RatsUpgrade ? "NO    (default NO)" : "YES   (default NO)";
        case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
          if (mode == GameMode.GM_VINTAGE)
            return "---";
          return !SkeletonsUpgrade ? "NO    (default NO)" : "YES   (default NO)";
        case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
          if (mode == GameMode.GM_VINTAGE)
            return "---";
          return !ShamblersUpgrade ? "NO    (default NO)" : "YES   (default NO)";
        default:
          return "???";
      }
    }

    public static void Save(GameOptions options, string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving options...");
      using (Stream stream = filepath.CreateStream(true)) {
        (new BinaryFormatter()).Serialize(stream, (object) options);
        stream.Flush();
	  };
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving options... done!");
    }

    public static GameOptions Load(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading options...");
      GameOptions gameOptions;
      try {
        using (Stream stream = filepath.CreateStream(false)) {
          gameOptions = (GameOptions)(new BinaryFormatter()).Deserialize(stream);
		};
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
      _FIRST = 0,
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
      _FIRST = 0,
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
}
