// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Achievement
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Achievement
  {
    public Achievement.IDs ID { get; private set; }

    public string Name { get; private set; }

    public string TeaseName { get; private set; }

    public string[] Text { get; private set; }

    public string MusicID { get; private set; }

    public int ScoreValue { get; private set; }

    public bool IsDone { get; set; }

    public Achievement(Achievement.IDs id, string name, string teaseName, string[] text, string musicID, int scoreValue)
    {
            ID = id;
            Name = name;
            TeaseName = teaseName;
            Text = text;
            MusicID = musicID;
            ScoreValue = scoreValue;
            IsDone = false;
    }

    [Serializable]
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
