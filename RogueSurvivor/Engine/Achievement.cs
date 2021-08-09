// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Achievement
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Engine
{
  internal class Achievement
  {
    public readonly IDs ID;
    public readonly string Name;
    public readonly string TeaseName;
    public readonly string[] Text;
    public readonly string MusicID;
    public readonly int ScoreValue;

    public Achievement(IDs id, string name, string teaseName, string[] text, string musicID, int scoreValue)
    {
            ID = id;
            Name = name;
            TeaseName = teaseName;
            Text = text;
            MusicID = musicID;
            ScoreValue = scoreValue;
    }

    public enum IDs
    {
      REACHED_DAY_07 = 0,
      REACHED_DAY_14 = 1,
      REACHED_DAY_21 = 2,
      REACHED_DAY_28 = 3,
      CHAR_BROKE_INTO_OFFICE = 4,
      CHAR_FOUND_UNDERGROUND_FACILITY = 5,
      CHAR_POWER_UNDERGROUND_FACILITY = 6,
      KILLED_THE_SEWERS_THING = 7,
      _COUNT = 8,
    }
  }
}
