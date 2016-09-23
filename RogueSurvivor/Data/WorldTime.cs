// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.WorldTime
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class WorldTime : ISerializable
    {
    public const int TURNS_PER_HOUR = 30;
    public const int TURNS_PER_DAY = 24*TURNS_PER_HOUR;

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
        Contract.Ensures(0<= m_TurnCounter);
        return m_TurnCounter;
      }
      set
      {
        Contract.Requires(0<=value);
        m_TurnCounter = value;
        RecomputeDate();
      }
    }

    public int Day
    {
      get
      {
        return m_Day;
      }
    }

    public int Hour
    {
      get
      {
        return m_Hour;
      }
    }

    public bool IsNight
    {
      get
      {
        return m_IsNight;
      }
    }

    public DayPhase Phase
    {
      get
      {
        return m_Phase;
      }
    }

    public bool IsStrikeOfMidnight
    {
      get
      {
        return m_IsStrikeOfMidnight;
      }
    }

    public bool IsStrikeOfMidday
    {
      get
      {
        return m_IsStrikeOfMidday;
      }
    }

    public WorldTime()
      : this(0)
    {
    }

    public WorldTime(WorldTime src)
      : this(src.TurnCounter)
    {
      Contract.Requires(null!=src);
    }

    public WorldTime(int turnCounter)
    {
      Contract.Requires(0<=turnCounter);
      TurnCounter = turnCounter;
    }

#region Implement ISerializable
    // general idea is Plain Old Data before objects.
    protected WorldTime(SerializationInfo info, StreamingContext context)
    {
      TurnCounter = info.GetInt32("TurnCounter");
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("TurnCounter", m_TurnCounter);
    }
#endregion

    private void RecomputeDate()
    {
      int num1 = m_TurnCounter;
      m_Day = num1 / TURNS_PER_DAY;
      int num2 = num1 - m_Day * TURNS_PER_DAY;
      m_Hour = num2 / TURNS_PER_HOUR;
      int num3 = num2 - m_Hour * TURNS_PER_HOUR;
      switch (m_Hour)
      {
        case 0:
                    m_Phase = DayPhase.MIDNIGHT;
                    m_IsNight = true;
          break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
                    m_Phase = DayPhase.DEEP_NIGHT;
                    m_IsNight = true;
          break;
        case 6:
                    m_Phase = DayPhase.SUNRISE;
                    m_IsNight = false;
          break;
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
                    m_Phase = DayPhase.MORNING;
                    m_IsNight = false;
          break;
        case 12:
                    m_Phase = DayPhase.MIDDAY;
                    m_IsNight = false;
          break;
        case 13:
        case 14:
        case 15:
        case 16:
        case 17:
                    m_Phase = DayPhase.AFTERNOON;
                    m_IsNight = false;
          break;
        case 18:
                    m_Phase = DayPhase.SUNSET;
                    m_IsNight = true;
          break;
        case 19:
        case 20:
        case 21:
        case 22:
        case 23:
                    m_Phase = DayPhase.EVENING;
                    m_IsNight = true;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled hour",m_Hour.ToString()+"; "+m_TurnCounter.ToString());
      }
       // the only updates happening to TurnCounter are from operator++
       // that is, the old value used for strike of midnight/midday is always
       // one less than the current value
       m_IsStrikeOfMidnight = (0 == num3 && m_Phase == DayPhase.MIDNIGHT);
       m_IsStrikeOfMidday = (0 == num3 && m_Phase == DayPhase.MIDDAY);
     }

    public override string ToString()
    {
      return string.Format("day {0} hour {1:D2}", (object)Day, (object)Hour);
    }

    public static string MakeTimeDurationMessage(int turns)
    {
      if (turns < TURNS_PER_HOUR)
        return "less than a hour";
      if (turns < TURNS_PER_DAY)
      {
        int num = turns / TURNS_PER_HOUR;
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
