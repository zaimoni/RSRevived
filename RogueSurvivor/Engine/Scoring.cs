// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Scoring
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Linq;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Scoring
  {
    private readonly Dictionary<GameActors.IDs, Scoring.KillData> m_Kills = new Dictionary<GameActors.IDs, Scoring.KillData>();
    private readonly HashSet<GameActors.IDs> m_Sightings = new HashSet<GameActors.IDs>();
    private readonly List<GameEventData> m_Events = new List<GameEventData>();
    private readonly HashSet<Map> m_VisitedMaps = new HashSet<Map>();
    public const int MAX_ACHIEVEMENTS = 8;
    public const int SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD = 360;
    private int m_StartScoringTurn;
    private int m_ReincarnationNumber;
    private int m_KillPoints;
    private DifficultySide m_Side;

    private readonly Achievement[] Achievements = new Achievement[(int) Achievement.IDs._COUNT];
    public int TurnsSurvived;   // RogueGame: 3 write access
    public string DeathReason;  // RogueGame: 1 write access
    public TimeSpan RealLifePlayingTime = new TimeSpan(0L);   // RogueGame: 1 write access

    public DifficultySide Side {
      get {
        return m_Side;
      }
      set { // 4 references in RogueGame
        m_Side = value;
      }
    }

    public int StartScoringTurn { get { return m_StartScoringTurn; } }
    public int ReincarnationNumber { get { return m_ReincarnationNumber; } }
    public IEnumerable<Scoring.GameEventData> Events { get { return m_Events; } }
    public bool HasNoEvents { get { return m_Events.Count == 0; } }
    public IEnumerable<Scoring.KillData> Kills { get { return m_Kills.Values; } }
    public bool HasNoKills { get { return m_Kills.Count == 0; } }
    public int KillPoints { get { return m_KillPoints; } }
    public int SurvivalPoints { get { return 2 * (TurnsSurvived - StartScoringTurn); } }

    public int AchievementPoints { get { return Achievements.Sum(x => x.IsDone ? x.ScoreValue : 0); } }

    public float DifficultyRating {
      get { // Live.  Thus only valid to set with values calculated at reincarnation number 0; otherwise it's double-penalized
        float ret = RogueGame.Options.DifficultyRating(DifficultySide.FOR_SURVIVOR == Session.Get.Scoring.Side ? GameFactions.IDs.TheCivilians : GameFactions.IDs.TheUndeads);
        return ret / (float) (1 + m_ReincarnationNumber);
      }
    }

    public int TotalPoints {
      get {
        return (int) ((double)DifficultyRating * (double) (m_KillPoints + SurvivalPoints + AchievementPoints));
      }
    }

    public int CompletedAchievementsCount { get { return Achievements.Count(x => x.IsDone); } }

    public Scoring()
    {
      InitAchievement(new Achievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE, "Broke into a CHAR Office", "Did not broke into XXX", new string[1]{
        "Now try not to die too soon..."
      }, GameMusics.HEYTHERE, 1000));
      InitAchievement(new Achievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY, "Found the CHAR Underground Facility", "Did not found XXX", new string[1]{
        "Now, where is the light switch?..."
      }, GameMusics.CHAR_UNDERGROUND_FACILITY, 2000));
      InitAchievement(new Achievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY, "Powered the CHAR Underground Facility", "Did not XXX the XXX", new string[5]{
        "Personal message from the game developper : ",
        "Sorry, the rest of the plot is missing.",
        "For now its a dead end.",
        "Enjoy the rest of the game.",
        "See you in a next game version :)"
      }, GameMusics.CHAR_UNDERGROUND_FACILITY, 3000));
      InitAchievement(new Achievement(Achievement.IDs.KILLED_THE_SEWERS_THING, "Killed The Sewers Thing", "Did not kill the XXX", new string[1] {
        "One less Thing to worry about!"
      }, GameMusics.HEYTHERE, 1000));
      InitAchievement(new Achievement(Achievement.IDs.REACHED_DAY_07, "Reached Day 7", "Did not reach XXX", new string[1] {
        "Keep staying alive!"
      }, GameMusics.HEYTHERE, 1000));
      InitAchievement(new Achievement(Achievement.IDs.REACHED_DAY_14, "Reached Day 14", "Did not reach XXX", new string[1]{
        "Keep staying alive!"
      }, GameMusics.HEYTHERE, 1000));
      InitAchievement(new Achievement(Achievement.IDs.REACHED_DAY_21, "Reached Day 21", "Did not reach XXX", new string[1]{
        "Keep staying alive!"
      }, GameMusics.HEYTHERE, 1000));
      InitAchievement(new Achievement(Achievement.IDs.REACHED_DAY_28, "Reached Day 28", "Did not reach XXX", new string[1] {
        "Is this the end?"
      }, GameMusics.HEYTHERE, 1000));
    }

    public void StartNewLife(int gameTurn)
    {
      ++m_ReincarnationNumber;
      foreach (Achievement achievement in Achievements)
        achievement.IsDone = false;
      m_VisitedMaps.Clear();
      m_Events.Clear();
      m_Sightings.Clear();
      m_Kills.Clear();
      m_KillPoints = 0;
      m_StartScoringTurn = gameTurn;
    }

    public void UseReincarnation()
    {
      ++m_ReincarnationNumber;
    }

    public bool HasCompletedAchievement(Achievement.IDs id)
    {
      return Achievements[(int) id].IsDone;
    }

    public void SetCompletedAchievement(Achievement.IDs id)
    {
      Achievements[(int) id].IsDone = true;
    }

    public Achievement GetAchievement(Achievement.IDs id)
    {
      return Achievements[(int) id];
    }

    private void InitAchievement(Achievement a)
    {
      Achievements[(int) a.ID] = a;
    }

    public void DescribeAchievements(TextFile textFile)
    {
      foreach (Achievement achievement in Achievements) {
        if (achievement.IsDone)
          textFile.Append(string.Format("- {0} for {1} points!", achievement.Name, achievement.ScoreValue));
        else
          textFile.Append(string.Format("- Fail : {0}.", achievement.TeaseName));
      }
    }

    public static float ComputeDifficultyRating(GameOptions options, DifficultySide side, int reincarnationNumber)
    {
      float num1 = 1f;
      if (!options.RevealStartingDistrict) num1 += 0.1f;
      if (!options.NPCCanStarveToDeath) num1 += (DifficultySide.FOR_SURVIVOR==side ? -0.1f : 0.1f);
      if (options.NatGuardFactor != GameOptions.DEFAULT_NATGUARD_FACTOR) {
        float num2 = (float) (options.NatGuardFactor - GameOptions.DEFAULT_NATGUARD_FACTOR) / (float)GameOptions.DEFAULT_NATGUARD_FACTOR;
        num1 += 0.5f*(DifficultySide.FOR_SURVIVOR==side ? -num2 : num2);
      }
      if (options.SuppliesDropFactor != GameOptions.DEFAULT_SUPPLIESDROP_FACTOR) {
        float num2 = (float) (options.SuppliesDropFactor - GameOptions.DEFAULT_SUPPLIESDROP_FACTOR) / (float)GameOptions.DEFAULT_SUPPLIESDROP_FACTOR;
        num1 += 0.5f*(DifficultySide.FOR_SURVIVOR==side ? -num2 : num2);
      }
      if (options.ZombifiedsUpgradeDays != GameOptions.ZupDays.THREE)
      {
        float num2 = 0.0f;
        switch (options.ZombifiedsUpgradeDays)
        {
          case GameOptions.ZupDays.ONE:
            num2 = 0.5f;
            break;
          case GameOptions.ZupDays.TWO:
            num2 = 0.25f;
            break;
          case GameOptions.ZupDays.FOUR:
            num2 -= 0.1f;
            break;
          case GameOptions.ZupDays.FIVE:
            num2 -= 0.2f;
            break;
          case GameOptions.ZupDays.SIX:
            num2 -= 0.3f;
            break;
          case GameOptions.ZupDays.SEVEN:
            num2 -= 0.4f;
            break;
          case GameOptions.ZupDays.OFF:
            num2 = -0.5f;
            break;
        }
        num1 += 0.5f*(DifficultySide.FOR_SURVIVOR==side ? num2 : -num2);
      }
      float num3 = (float) Math.Sqrt(GameOptions.DEFAULT_MAX_UNDEADS+ GameOptions.DEFAULT_MAX_CIVILIANS) / (GameOptions.DEFAULT_CITY_SIZE * GameOptions.DEFAULT_DISTRICT_SIZE * GameOptions.DEFAULT_DISTRICT_SIZE);
      float num4 = ((float) Math.Sqrt((double) (options.MaxCivilians + options.MaxUndeads)) / (float) (options.CitySize * options.DistrictSize * options.DistrictSize) - num3) / num3;
      float num5 = side != DifficultySide.FOR_SURVIVOR ? num1 - 0.99f * num4 : num1 + 0.99f * num4;
      const float num6 = (float)(GameOptions.DEFAULT_MAX_UNDEADS) /(float)(GameOptions.DEFAULT_MAX_CIVILIANS);
      float num7 = ((float) options.MaxUndeads / (float) options.MaxCivilians - num6) / num6;
      float num8 = (float) (options.DayZeroUndeadsPercent - GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT) / (float)GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT;
      float num9 = (float) (options.ZombieInvasionDailyIncrease - 5) / 5f;
      float num10 = side != DifficultySide.FOR_SURVIVOR ? num5 - (float) (0.3 * (double) num7 + 0.05 * (double) num8 + 0.15 * (double) num9) : num5 + (float) (0.3 * (double) num7 + 0.05 * (double) num8 + 0.15 * (double) num9);
      const float num11 = (float)(GameOptions.DEFAULT_MAX_CIVILIANS* GameOptions.DEFAULT_ZOMBIFICATION_CHANCE);
      float num12 = ((float) (options.MaxCivilians * options.ZombificationChance) - num11) / num11;
      const float num13 = (float)(GameOptions.DEFAULT_MAX_CIVILIANS* GameOptions.DEFAULT_STARVED_ZOMBIFICATION_CHANCE);
      float num14 = ((float) (options.MaxCivilians * options.StarvedZombificationChance) - num13) / num13;
      if (!options.NPCCanStarveToDeath)
        num14 = -1f;
      float num15 = side != DifficultySide.FOR_SURVIVOR ? num10 - (float) (0.3 * (double) num12 + 0.2 * (double) num14) : num10 + (float) (0.3 * (double) num12 + 0.2 * (double) num14);

      if (!options.AllowUndeadsEvolution && Session.Get.HasEvolution) num15 *= (DifficultySide.FOR_SURVIVOR==side ? 0.5f : 2f);
      if (options.IsCombatAssistantOn) num15 *= 0.75f;
      if (options.IsPermadeathOn) num15 *= 2f;
      if (!options.IsAggressiveHungryCiviliansOn) num15 *= (DifficultySide.FOR_SURVIVOR == side ? 0.5f : 2f);
      if (GameMode.GM_VINTAGE != Session.Get.GameMode && options.RatsUpgrade) num15 *= (DifficultySide.FOR_SURVIVOR == side ? 1.1f : 0.9f);
      if (GameMode.GM_VINTAGE != Session.Get.GameMode && options.SkeletonsUpgrade) num15 *= (DifficultySide.FOR_SURVIVOR == side ? 1.2f : 0.8f);
      if (GameMode.GM_VINTAGE != Session.Get.GameMode && options.ShamblersUpgrade) num15 *= (DifficultySide.FOR_SURVIVOR == side ? 1.25f : 0.75f);

      return Math.Max(num15 / (float) (1 + reincarnationNumber), 0.0f);
    }

    public void AddKill(Actor player, Actor victim, int turn)
    {
      GameActors.IDs id = victim.Model.ID;
      if (m_Kills.TryGetValue(id, out KillData killData)) {
        ++killData.Amount;
      } else {
        m_Kills.Add(id, new KillData(id, turn));
        AddEvent(turn, string.Format("Killed first {0}.", Models.Actors[(int)id].Name));
      }
      m_KillPoints += Models.Actors[(int)id].ScoreValue;
      if (m_Side != DifficultySide.FOR_UNDEAD || Models.Actors[(int)id].Abilities.IsUndead) return;
      m_KillPoints += SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD;
    }

    public void AddSighting(GameActors.IDs actorModelID)
    {
      if (m_Sightings.Contains(actorModelID)) return;
      int turn = Session.Get.WorldTime.TurnCounter;
      m_Sightings.Add(actorModelID);
      AddEvent(turn, string.Format("Sighted first {0}.", Models.Actors[(int)actorModelID].Name));
    }

    public bool HasSighted(GameActors.IDs actorModelID)
    {
      return m_Sightings.Contains(actorModelID);
    }

    public bool HasVisited(Map map)
    {
      return m_VisitedMaps.Contains(map);
    }

    public void AddVisit(int turn, Map map)
    {
      lock (m_VisitedMaps) m_VisitedMaps.Add(map);
    }

    public void AddEvent(int turn, string text)
    {
      lock (m_Events) m_Events.Add(new GameEventData(turn, text));
    }

    [Serializable]
    public class KillData
    {
      public readonly GameActors.IDs ActorModelID;
      public int Amount;
      public readonly int FirstKillTurn;

      public KillData(Gameplay.GameActors.IDs actorModelID, int turn)
      {
        ActorModelID = actorModelID;
        Amount = 1;
        FirstKillTurn = turn;
      }
    }

    [Serializable]
    public class GameEventData
    {
      public readonly int Turn;
      public readonly string Text;

      public GameEventData(int turn, string text)
      {
        Turn = turn;
        Text = text;
      }
    }
  }

  [Serializable]
  internal class ActorScoring
  {
    private readonly Actor m_Actor;
    private readonly Dictionary<GameActors.IDs, int> m_FirstKills = new Dictionary<GameActors.IDs, int>();
    private readonly Dictionary<GameActors.IDs, int> m_KillCounts = new Dictionary<GameActors.IDs, int>();
    private readonly HashSet<GameActors.IDs> m_Sightings = new HashSet<GameActors.IDs>();
    private readonly List<KeyValuePair<int,string>> m_Events = new List<KeyValuePair<int, string>>();
    private readonly HashSet<Map> m_VisitedMaps = new HashSet<Map>();

    public ActorScoring(Actor src)
    {
      m_Actor = src;
    }

    public void AddKill(Actor victim, int turn)
    {
      GameActors.IDs id = victim.Model.ID;
      if (!m_FirstKills.ContainsKey(id)) {
        m_FirstKills[id] = turn;
        m_KillCounts[id] = 1;
        AddEvent(turn, string.Format("Killed first {0}.", Models.Actors[(int)id].Name));
      } else m_KillCounts[id]++;
    }

    public int KillPoints { get {
      const int SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD = 360;
      int ret = 0;
      foreach(var x in m_KillCounts) {  // XXX only works correctly for civilians
        ret += Models.Actors[(int)x.Key].ScoreValue*x.Value;
        if (Models.Actors[(int)x.Key].Abilities.IsUndead) continue;
        if (!m_Actor.Model.Abilities.IsUndead) continue;
        ret += SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD*x.Value;
      }
      return ret;
    } }

    public void AddSighting(GameActors.IDs actorModelID)
    {
      if (m_Sightings.Contains(actorModelID)) return;
      int turn = Session.Get.WorldTime.TurnCounter;
      m_Sightings.Add(actorModelID);
      AddEvent(turn, string.Format("Sighted first {0}.", Models.Actors[(int)actorModelID].Name));
    }

    public bool HasSighted(GameActors.IDs actorModelID)
    {
      return m_Sightings.Contains(actorModelID);
    }

    public IEnumerable<KeyValuePair<int, string>> Events { get { return m_Events; } }

    public void AddEvent(int turn, string text)
    {
      lock (m_Events) m_Events.Add(new KeyValuePair<int, string>(turn, text));
    }

    public bool HasVisited(Map map)
    {
      return m_VisitedMaps.Contains(map);
    }

    public void AddVisit(int turn, Map map)
    {
      lock (m_VisitedMaps) m_VisitedMaps.Add(map);
    }
  }

  // not clear if this should be serializable
  [Serializable]
  internal class Scoring_fatality
  {
    private List<Actor> m_FollowersWhenDied = null;
    public readonly Actor Killer;
    private Actor m_ZombifiedPlayer;
    public readonly string DeathPlace;

    public List<Actor> FollowersWhendDied { get { return m_FollowersWhenDied; } }
    public Actor ZombifiedPlayer { get { return m_ZombifiedPlayer; } }

    public Scoring_fatality(Actor killer, Actor victim, string death_loc) // historically victim is a player, but we don't check that here
    {
      Killer = killer;
      DeathPlace = death_loc;
      int ub = victim.CountFollowers;
      if (0 < ub) m_FollowersWhenDied = new List<Actor>(victim.Followers);
    }

    public void SetZombifiedPlayer(Actor z)
    {
      m_ZombifiedPlayer = z;
    }
  }
}
