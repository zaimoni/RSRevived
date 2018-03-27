// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.WorldTime
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Runtime.Serialization;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class WorldTime : ISerializable
    {
    public const int HOURS_PER_DAY = 24;    // not scalable
    public const int TURNS_PER_HOUR = 30;   // defines space-time scale.  Standard game is 30 turns/hour, district size 50
                                            // Angband space-time scale is 900 turns/hour, district size 1500
    public const int TURNS_PER_DAY = HOURS_PER_DAY * TURNS_PER_HOUR;
    private static readonly DayPhase[] _phases = new DayPhase[HOURS_PER_DAY];
    private static readonly bool[] _is_night = new bool[HOURS_PER_DAY];

    private int m_TurnCounter;
    private int m_Day;
    private int m_Hour;
    private int m_Tick;

    public int TurnCounter
    {
      get {
#if DEBUG
        if (0 > m_TurnCounter) throw new InvalidOperationException("0 > TurnCounter");
#endif
        return m_TurnCounter;
      }
      set {
#if DEBUG
        if (0 > value) throw new InvalidOperationException("0 > TurnCounter");
#endif
        m_TurnCounter = value;
        m_Day = Math.DivRem(m_TurnCounter,TURNS_PER_DAY,out m_Hour);
        m_Hour = Math.DivRem(m_Hour,TURNS_PER_HOUR,out m_Tick);
      }
    }

    public int Day { get { return m_Day; } }
    public int Hour { get { return m_Hour; } }
    public int Tick { get { return m_Tick; } }
    public bool IsNight { get { return _is_night[m_Hour]; } }
    public DayPhase Phase { get { return _phases[m_Hour]; } }
    public bool IsStrikeOfHour(int n) { return n==m_Hour && 0==m_Tick; }
    public bool IsStrikeOfMidnight { get { return IsStrikeOfHour(0); } }
    public bool IsStrikeOfMidday { get { return IsStrikeOfHour(12); } }

    // These two are correct only on the equator.  Providing thin-wrappers so
    // it is easy to bulk them out to account for latitude (after optimizing the game to be playable at 900 turns/hour)
    public bool IsDawn { get { return IsStrikeOfHour(6); } }
    public bool IsDusk { get { return IsStrikeOfHour(18); } }

    static WorldTime()
    {
      int hour=0;
      do {
        switch (hour)
        {
        case 0:
          _phases[hour] = DayPhase.MIDNIGHT;
          _is_night[hour] = true;
          break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
          _phases[hour] = DayPhase.DEEP_NIGHT;
          _is_night[hour] = true;
          break;
        case 6:
          _phases[hour] = DayPhase.SUNRISE;
          _is_night[hour] = false;
          break;
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
          _phases[hour] = DayPhase.MORNING;
          _is_night[hour] = false;
          break;
        case 12:
          _phases[hour] = DayPhase.MIDDAY;
          _is_night[hour] = false;
          break;
        case 13:
        case 14:
        case 15:
        case 16:
        case 17:
          _phases[hour] = DayPhase.AFTERNOON;
          _is_night[hour] = false;
          break;
        case 18:
          _phases[hour] = DayPhase.SUNSET;
          _is_night[hour] = true;
          break;
        case 19:
        case 20:
        case 21:
        case 22:
        case 23:
          _phases[hour] = DayPhase.EVENING;
          _is_night[hour] = true;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled hour",hour.ToString());
        }
      } while(++hour < HOURS_PER_DAY);
    }

    public WorldTime(WorldTime src)
      : this(src.TurnCounter)
    {
#if DEBUG
      if (null==src) throw new ArgumentNullException(nameof(src));
#endif
    }

    public WorldTime(int turnCounter=0)
    {
#if DEBUG
      if (0 > turnCounter) throw new ArgumentOutOfRangeException(nameof(turnCounter),turnCounter, "0 > turnCounter");
#endif
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

    public static int Turn(int day,int hour)
    {
#if DEBUG
      if (0>day) throw new ArgumentOutOfRangeException(nameof(day),day, "0>day");
      if (int.MaxValue / TURNS_PER_DAY < day) throw new ArgumentOutOfRangeException(nameof(day), day, "(int.MaxValue / TURNS_PER_DAY < day");
      if (0>hour) throw new ArgumentOutOfRangeException(nameof(hour), hour, "0>hour");
      if ((int.MaxValue - (TURNS_PER_DAY * day)) / TURNS_PER_HOUR < hour) throw new ArgumentOutOfRangeException(nameof(hour), hour, "(int.MaxValue - (TURNS_PER_DAY * day)) / TURNS_PER_HOUR < hour");
#endif
      return TURNS_PER_DAY*day + TURNS_PER_HOUR*hour;
    }

    public int MidnightToDawnDuration {
      get {
        if (6>m_Hour) return Turn(m_Day,6)-TurnCounter;
        return 6*TURNS_PER_HOUR;
      }
    }

    public int SunsetToDawnDuration {
      get {
        if (6>m_Hour) return Turn(m_Day,6)-TurnCounter;
        if (18<=m_Hour) return Turn(m_Day+1,6)-TurnCounter;
        return 12*TURNS_PER_HOUR;
      }
    }
  }
}
