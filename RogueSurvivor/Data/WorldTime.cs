// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.WorldTime
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class WorldTime
  {
    public const int TURNS_PER_HOUR = 30;
    public const int TURNS_PER_DAY = 720;
    private int m_TurnCounter;
    private int m_Day;
    private int m_Hour;
    private DayPhase m_Phase;
    private bool m_IsNight;
    private bool m_IsStrikeOfMidnight;
    private bool m_IsStrikeOfMidday;

    public int TurnCounter
    {
      get
      {
        return m_TurnCounter;
      }
      set
      {
        m_TurnCounter = value;
        RecomputeDate();
      }
    }

    public int Day
    {
      get
      {
        return this.m_Day;
      }
    }

    public int Hour
    {
      get
      {
        return this.m_Hour;
      }
    }

    public bool IsNight
    {
      get
      {
        return this.m_IsNight;
      }
    }

    public DayPhase Phase
    {
      get
      {
        return this.m_Phase;
      }
    }

    public bool IsStrikeOfMidnight
    {
      get
      {
        return this.m_IsStrikeOfMidnight;
      }
    }

    public bool IsStrikeOfMidday
    {
      get
      {
        return this.m_IsStrikeOfMidday;
      }
    }

    public WorldTime()
      : this(0)
    {
    }

    public WorldTime(WorldTime src)
      : this(src.TurnCounter)
    {
    }

    public WorldTime(int turnCounter)
    {
      if (turnCounter < 0)
        throw new ArgumentOutOfRangeException("turnCounter < 0");
      this.m_TurnCounter = turnCounter;
      this.RecomputeDate();
    }

    private void RecomputeDate()
    {
      int num1 = this.m_TurnCounter;
      this.m_Day = num1 / 720;
      int num2 = num1 - this.m_Day * 720;
      this.m_Hour = num2 / 30;
      int num3 = num2 - this.m_Hour * 30;
      switch (this.m_Hour)
      {
        case 0:
          this.m_Phase = DayPhase.MIDNIGHT;
          this.m_IsNight = true;
          break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
          this.m_Phase = DayPhase.DEEP_NIGHT;
          this.m_IsNight = true;
          break;
        case 6:
          this.m_Phase = DayPhase.SUNRISE;
          this.m_IsNight = false;
          break;
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
          this.m_Phase = DayPhase.MORNING;
          this.m_IsNight = false;
          break;
        case 12:
          this.m_Phase = DayPhase.MIDDAY;
          this.m_IsNight = false;
          break;
        case 13:
        case 14:
        case 15:
        case 16:
        case 17:
          this.m_Phase = DayPhase.AFTERNOON;
          this.m_IsNight = false;
          break;
        case 18:
          this.m_Phase = DayPhase.SUNSET;
          this.m_IsNight = true;
          break;
        case 19:
        case 20:
        case 21:
        case 22:
        case 23:
          this.m_Phase = DayPhase.EVENING;
          this.m_IsNight = true;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled hour");
      }
       // the only updates happening to TurnCounter are from operator++
       // that is, the old value used for strike of midnight/midday is always
       // one less than the current value
       m_IsStrikeOfMidnight = (0 == num3 && m_Phase == DayPhase.MIDNIGHT);
       m_IsStrikeOfMidday = (0 == num3 && m_Phase == DayPhase.MIDDAY);
     }

        public override string ToString()
    {
      return string.Format("day {0} hour {1:D2}", (object) this.Day, (object) this.Hour);
    }

    public static string MakeTimeDurationMessage(int turns)
    {
      if (turns < 30)
        return "less than a hour";
      if (turns < 720)
      {
        int num = turns / 30;
        if (num == 1)
          return "about 1 hour";
        return string.Format("about {0} hours", (object) num);
      }
      WorldTime worldTime = new WorldTime(turns);
      if (worldTime.Day == 1)
        return "about 1 day";
      return string.Format("about {0} days", (object) worldTime.Day);
    }
  }
}
