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
    public string Name { get; set; }

    public int TotalPoints { get; set; }

    public int DifficultyPercent { get; set; }

    public int SurvivalPoints { get; set; }

    public int KillPoints { get; set; }

    public int AchievementPoints { get; set; }

    public int TurnSurvived { get; set; }

    public TimeSpan PlayingTime { get; set; }

    public string SkillsDescription { get; set; }

    public string Death { get; set; }

    public static HiScore FromScoring(string name, Scoring sc, string skillsDescription)
    {
      if (sc == null)
        throw new ArgumentNullException("scoring");
      return new HiScore()
      {
        AchievementPoints = sc.AchievementPoints,
        Death = sc.DeathReason,
        DifficultyPercent = (int) (100.0 * (double) sc.DifficultyRating),
        KillPoints = sc.KillPoints,
        Name = name,
        PlayingTime = sc.RealLifePlayingTime,
        SkillsDescription = skillsDescription,
        SurvivalPoints = sc.SurvivalPoints,
        TotalPoints = sc.TotalPoints,
        TurnSurvived = sc.TurnsSurvived
      };
    }
  }
}
