// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.HiScore
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class HiScore
  {
    public string Name { get; }
    public int TotalPoints { get; }
    public int DifficultyPercent { get; }
    public int SurvivalPoints { get; }
    public int KillPoints { get; }
    public int AchievementPoints { get; }
    public int TurnSurvived { get; }
    public TimeSpan PlayingTime { get; }
    public string SkillsDescription { get; }
    public string Death { get; }

    // JSON: every property, by name
    [System.Text.Json.Serialization.JsonConstructor]
    public HiScore(string name, int totalPoints, int difficultyPercent, int survivalPoints, int killPoints, int achievementPoints, int turnSurvived, TimeSpan playingTime, string skillsDescription, string death)
    {
      Name = name;
      TotalPoints = totalPoints;
      DifficultyPercent = difficultyPercent;
      SurvivalPoints = survivalPoints;
      KillPoints = killPoints;
      AchievementPoints = achievementPoints;
      TurnSurvived = turnSurvived;
      PlayingTime = playingTime;
      SkillsDescription = skillsDescription;
      Death = death;
    }

    // default constructor
    public HiScore()
    {
      Death = "no death";
      DifficultyPercent = 0;
      KillPoints = 0;
      Name = "no one";
      PlayingTime = TimeSpan.Zero;
      SurvivalPoints = 0;
      TotalPoints = 0;
      TurnSurvived = 0;
      SkillsDescription = "no skills";
    }

    public HiScore(Scoring sc, ActorScoring asc, string skillsDescription)
    {
#if DEBUG
      if (null == sc) throw new ArgumentNullException(nameof(sc));
      if (null == asc) throw new ArgumentNullException(nameof(asc));
#endif
      AchievementPoints = asc.AchievementPoints;
      Death = asc.DeathReason;
      DifficultyPercent = (int)(100.0 * asc.DifficultyRating);
      KillPoints = asc.KillPoints;
      Name = asc.Name;
      PlayingTime = sc.RealLifePlayingTime;
      SkillsDescription = skillsDescription;
      SurvivalPoints = asc.SurvivalPoints;
      TotalPoints = asc.TotalPoints;
      TurnSurvived = asc.TurnsSurvived;
    }

    public bool is_valid()
    {
      if (string.IsNullOrEmpty(Death)) return false;
      if (0 > DifficultyPercent) return false;
      if (0 > KillPoints) return false;
      if (string.IsNullOrEmpty(Name)) return false;
//    if ( ... PlayingTime) return false;
      if (0 > SurvivalPoints) return false;
      if (0 > TotalPoints) return false;
      if (0 > TurnSurvived) return false;
      if (string.IsNullOrEmpty(SkillsDescription)) return false;
      return true;
    }
  }
}
