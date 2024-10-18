// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Scoring
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Zaimoni.Serialization;

#nullable enable

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
    public sealed class Scoring : ISerialize
    {
    private static readonly Achievement[] Achievements = new Achievement[(int)Achievement.IDs._COUNT] {
        new Achievement(Achievement.IDs.REACHED_DAY_07, "Reached Day 7", "Did not reach XXX", new string[1] {
        "Keep staying alive!"
      }, GameMusics.HEYTHERE, 1000),
        new Achievement(Achievement.IDs.REACHED_DAY_14, "Reached Day 14", "Did not reach XXX", new string[1]{
        "Keep staying alive!"
      }, GameMusics.HEYTHERE, 1000),
        new Achievement(Achievement.IDs.REACHED_DAY_21, "Reached Day 21", "Did not reach XXX", new string[1]{
        "Keep staying alive!"
      }, GameMusics.HEYTHERE, 1000),
        new Achievement(Achievement.IDs.REACHED_DAY_28, "Reached Day 28", "Did not reach XXX", new string[1] {
        "Is this the end?"
      }, GameMusics.HEYTHERE, 1000),
        new Achievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE, "Broke into a CHAR Office", "Did not broke into XXX", new string[1]{
        "Now try not to die too soon..."
      }, GameMusics.HEYTHERE, 1000),
        new Achievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY, "Found the CHAR Underground Facility", "Did not found XXX", new string[1]{
        "Now, where is the light switch?..."
      }, GameMusics.CHAR_UNDERGROUND_FACILITY, 2000),
        new Achievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY, "Powered the CHAR Underground Facility", "Did not XXX the XXX", new string[5]{
        "Personal message from the game developper : ",
        "Sorry, the rest of the plot is missing.",
        "For now its a dead end.",
        "Enjoy the rest of the game.",
        "See you in a next game version :)"
      }, GameMusics.CHAR_UNDERGROUND_FACILITY, 3000),
        new Achievement(Achievement.IDs.KILLED_THE_SEWERS_THING, "Killed The Sewers Thing", "Did not kill the XXX", new string[1] {
        "One less Thing to worry about!"
      }, GameMusics.HEYTHERE, 1000)
    };

    private int m_ReincarnationNumber;
    public TimeSpan RealLifePlayingTime = new TimeSpan(0L);   // RogueGame: 1 write access

    public int ReincarnationNumber { get { return m_ReincarnationNumber; } }

    public Scoring()
    {
#if DEBUG
      var ub = Achievements.Length;
      while(0 <= --ub) {
        if (ub != (int)Achievements[ub].ID) throw new InvalidProgramException("mismatch between achievement id and array position: " + ub.ToString() + " " + Achievements[ub].ID.ToString());
      }
#endif
    }

    protected Scoring(DecodeObjects decode)
    {
       Formatter.Deserialize(decode.src, ref m_ReincarnationNumber);
       Formatter.Deserialize(decode.src, ref RealLifePlayingTime);
    }

    void ISerialize.save(EncodeObjects encode)
    {
       Formatter.Serialize(encode.dest, m_ReincarnationNumber);
       Formatter.Serialize(encode.dest, RealLifePlayingTime);
    }

    public void SaveLoadOk(Scoring test) {
        var err = string.Empty;
        if (m_ReincarnationNumber != test.m_ReincarnationNumber) err += "m_ReincarnationNumber != test.m_ReincarnationNumber: " + m_ReincarnationNumber.ToString() + " "+ test.m_ReincarnationNumber.ToString() + "\n";
        if (RealLifePlayingTime != test.RealLifePlayingTime) err += "RealLifePlayingTime != test.RealLifePlayingTime: " + RealLifePlayingTime.ToString() + " "+ test.RealLifePlayingTime.ToString() + "\n";

        if (!string.IsNullOrEmpty(err)) throw new InvalidOperationException(err);
    }

    static private int field_code(ref Utf8JsonReader reader) {
        if (reader.ValueTextEquals("ReincarnationNumber")) return 1;
        else if (reader.ValueTextEquals("RealLifePlayingTime")) return 2;

        RogueGame.Game.ErrorPopup(reader.GetString());
        throw new JsonException();
    }

    private Scoring(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      if (JsonTokenType.StartObject != reader.TokenType) throw new JsonException();
      int origin_depth = reader.CurrentDepth;
      reader.Read();

      void read(ref Utf8JsonReader reader) {
          int code = field_code(ref reader);
          reader.Read();

          switch (code) {
          case 1:
              m_ReincarnationNumber = reader.GetInt32();
              break;
          case 2:
              RealLifePlayingTime = new TimeSpan(reader.GetInt64());
              break;
          }
      }

      while (reader.CurrentDepth != origin_depth || JsonTokenType.EndObject != reader.TokenType) {
          if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

          read(ref reader);

          reader.Read();
      }

      if (JsonTokenType.EndObject != reader.TokenType) throw new JsonException();
    }

    public static Scoring fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options) => new Scoring(ref reader, options);

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options) {
      writer.WriteStartObject();
      writer.WriteNumber("ReincarnationNumber", m_ReincarnationNumber);
      writer.WriteNumber("RealLifePlayingTime", RealLifePlayingTime.Ticks);
      writer.WriteEndObject();
    }

    public int StartNewLife() => ++m_ReincarnationNumber;
    public void UseReincarnation() => ++m_ReincarnationNumber;
    public void Deincarnate() => --m_ReincarnationNumber;
    public static Achievement GetAchievement(Achievement.IDs id) => Achievements[(int) id];
  }

  [Serializable]
  public sealed class ActorScoring
  {
    private readonly Actor m_Actor;
    private readonly bool[] Achievements_completed = new bool[(int) Achievement.IDs._COUNT];
    private readonly Dictionary<GameActors.IDs, int> m_FirstKills = new Dictionary<GameActors.IDs, int>();
    private readonly Dictionary<GameActors.IDs, int> m_KillCounts = new Dictionary<GameActors.IDs, int>();
    private readonly HashSet<GameActors.IDs> m_Sightings = new HashSet<GameActors.IDs>();
    private readonly List<KeyValuePair<int,string>> m_Events = new List<KeyValuePair<int, string>>();
    private readonly HashSet<Map> m_VisitedMaps = new HashSet<Map>();
    private string? m_DeathReason;

    public string DeathReason {
      get { return m_DeathReason ?? (m_Actor.Model.Abilities.IsUndead ? "Still undead" : "Still alive"); }
      set { if (!string.IsNullOrEmpty(m_DeathReason)) m_DeathReason = value; }
    }

    public ActorScoring(Actor src)
    {
      m_Actor = src;
    }

    public string Name { get { return m_Actor.UnmodifiedName; } }
    public int TurnsSurvived { get { return m_Actor.Location.Map.LocalTime.TurnCounter-m_Actor.SpawnTime; } }
    public int SurvivalPoints { get { return 2*TurnsSurvived; } }

    public void AddKill(Actor victim, int turn)
    {
      GameActors.IDs id = victim.Model.ID;
      if (!m_FirstKills.ContainsKey(id)) {
        m_FirstKills[id] = turn;
        m_KillCounts[id] = 1;
        AddEvent(turn, string.Format("Killed first {0}.", GameActors.From(id).Name));
      } else m_KillCounts[id]++;
    }

    public int KillsCount { get => m_KillCounts.Values.Sum(); }

    public int KillPoints { get {
      const int SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD = 360;
      int ret = 0;
      foreach(var x in m_KillCounts) {  // XXX only works correctly for civilians
        var model = GameActors.From(x.Key);
        ret += model.ScoreValue*x.Value;
        if (model.Abilities.IsUndead) continue;
        if (!m_Actor.Model.Abilities.IsUndead) continue;
        ret += SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD*x.Value;
      }
      return ret;
    } }

    public void DescribeKills(TextFile textFile, string he_or_she /* XXX \todo calculate from m_Actor */)
    {
      if (0 >= m_KillCounts.Count) {
        textFile.Append(string.Format("{0} was a pacifist. Or too scared to fight.", he_or_she));
      } else {
        foreach (var kill in m_KillCounts) {
          var model = GameActors.From(kill.Key);
          string str3 = kill.Value > 1 ? model.PluralName : model.Name;
          textFile.Append(string.Format("{0,4} {1}.", kill.Value, str3));
        }
      }
    }

    public double DifficultyRating {
      get {
        return RogueGame.Options.DifficultyRating(m_Actor.Faction.ID);    // don't worry about reincarnation count with per-actor scoring
      }
    }

    public int AchievementPoints { get {
      int ret = 0;
      Achievement.IDs i = Achievement.IDs._COUNT;
      while(0 < i--) {
        if (!Achievements_completed[(int)i]) continue;
        ret += Scoring.GetAchievement(i).ScoreValue;
      }
      return ret;
    } }

    public int TotalPoints {
      get {
        return (int)(DifficultyRating * (KillPoints + SurvivalPoints + AchievementPoints));
      }
    }

    public void AddSighting(GameActors.IDs actorModelID)
    {
      if (m_Sightings.Contains(actorModelID)) return;
      int turn = Session.Get.WorldTime.TurnCounter;
      m_Sightings.Add(actorModelID);
      AddEvent(turn, string.Format("Sighted first {0}.", GameActors.From(actorModelID).Name));
    }

    public bool HasSighted(GameActors.IDs actorModelID) // this only *has* to work for Jason Myers, and it controls UI text
    {
      return m_Sightings.Contains(actorModelID);
    }

    public IEnumerable<KeyValuePair<int, string>> Events { get { return m_Events; } }

    public void AddEvent(int turn, string text)
    {
      lock (m_Events) m_Events.Add(new KeyValuePair<int, string>(turn, text));
    }

    public void DescribeEvents(TextFile textFile, string he_or_she)
    {
      if (0 >= m_Events.Count) {
        textFile.Append(string.Format("{0} had a quiet life. Or dull and boring.", he_or_she));
      } else {
        foreach (var x in m_Events)
          textFile.Append(string.Format("- {0,13} : {1}", new WorldTime(x.Key).ToString(), x.Value));
      }
    }

    public bool HasVisited(Map map)
    {
      return m_VisitedMaps.Contains(map);
    }

    public void AddVisit(int turn, Map map)
    {
      lock (m_VisitedMaps) m_VisitedMaps.Add(map);
    }

    public int CompletedAchievementsCount { get {
      return Achievements_completed.Count(x => x);
    } }

    public bool HasCompletedAchievement(Achievement.IDs id)
    {
      return Achievements_completed[(int) id];
    }

    public void SetCompletedAchievement(Achievement.IDs id)
    {
      Achievements_completed[(int) id] = true;
    }

    public void DescribeAchievements(TextFile textFile)
    {
      Achievement.IDs i = 0;
      do {
        Achievement achievement = Scoring.GetAchievement(i);
        textFile.Append(Achievements_completed[(int)i] ? string.Format("- {0} for {1} points!", achievement.Name, achievement.ScoreValue)
                                                       : string.Format("- Fail : {0}.", achievement.TeaseName));
      } while(Achievement.IDs._COUNT > ++i);
    }
  }

  public sealed class Scoring_fatality
  {
    private readonly List<Actor>? m_FollowersWhenDied = null;
    public readonly Actor Killer;
    private Actor? m_ZombifiedPlayer;
    public readonly string DeathPlace;

    public List<Actor>? FollowersWhendDied { get { return m_FollowersWhenDied; } }
    public Actor? ZombifiedPlayer { get { return m_ZombifiedPlayer; } }

    public Scoring_fatality(Actor killer, Actor victim, string death_loc) // historically victim is a player, but we don't check that here
    {
      Killer = killer;
      DeathPlace = death_loc;
      m_FollowersWhenDied = victim.Followers?.ToList();
    }

    public void SetZombifiedPlayer(Actor z)
    {
      m_ZombifiedPlayer = z;
    }
  }
}

namespace Zaimoni.JsonConvert
{
    public class Scoring : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.Scoring>
    {
        public override djack.RogueSurvivor.Engine.Scoring Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            return djack.RogueSurvivor.Engine.Scoring.fromJson(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.Scoring src, JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
