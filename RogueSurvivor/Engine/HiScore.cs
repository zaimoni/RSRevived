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

    public static HiScore FromScoring(string name, Scoring sc, ActorScoring asc, string skillsDescription)
    {
#if DEBUG
      if (null == sc) throw new ArgumentNullException(nameof(sc));
#endif
      return new HiScore{
        AchievementPoints = sc.AchievementPoints,
        Death = sc.DeathReason,
        DifficultyPercent = (int) (100.0 * (double) sc.DifficultyRating),
        KillPoints = sc.KillPoints,
        Name = name,
        PlayingTime = sc.RealLifePlayingTime,
        SkillsDescription = skillsDescription,
        SurvivalPoints = asc.SurvivalPoints,
        TotalPoints = sc.TotalPoints,
        TurnSurvived = asc.TurnsSurvived
      };
    }
  }
}
