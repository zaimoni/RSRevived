// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.WorldTime
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Linq;
using System.Runtime.Serialization;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class WorldTime : ISerializable
    {
    public const int HOURS_PER_DAY = 24;    // not scalable
    public const int TURNS_PER_HOUR = 30;   // defines space-time scale.  Standard game is 30 turns/hour, district size 50
                                            // Angband space-time scale is 900 turns/hour, district size 1500
    public const int TURNS_PER_DAY = HOURS_PER_DAY * TURNS_PER_HOUR;

    private const int HOUR_MIDNIGHT = 0;
    private const int HOUR_SUNRISE = 6;     // XXX equatorial; would like to use a more real calendar
    private const int HOUR_NOON = 12;
    private const int HOUR_SUNSET = 18;

    private static readonly DayPhase[] _phases = Enumerable.Range(0, HOURS_PER_DAY).Select(h => phase(h)).ToArray();
    private static readonly bool[] _is_night = Enumerable.Range(0, HOURS_PER_DAY).Select(h => is_night(h)).ToArray();

    private int m_TurnCounter;
    private int m_Day;
    private int m_Hour;
    private int m_Tick;

    public int TurnCounter
    {
      get { return m_TurnCounter; }
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
    public bool IsStrikeOfMidnight { get { return IsStrikeOfHour(HOUR_MIDNIGHT); } }
    public bool IsStrikeOfMidday { get { return IsStrikeOfHour(HOUR_NOON); } }

    // These two are correct only on the equator.  Providing thin-wrappers so
    // it is easy to bulk them out to account for latitude (after optimizing the game to be playable at 900 turns/hour)
    public bool IsDawn { get { return IsStrikeOfHour(HOUR_SUNRISE); } }
    public bool IsDusk { get { return IsStrikeOfHour(HOUR_SUNSET); } }

    /// <remark>only has to work for 0...23</remark>
    static private bool is_night(int hour) { return 6 >= hour || 18 <= hour; }
    /// <remark>only has to work for 0...23</remark>
    static private DayPhase phase(int hour) {
      switch(hour)
      {
      case HOUR_MIDNIGHT: return DayPhase.MIDNIGHT;
      case HOUR_SUNRISE: return DayPhase.SUNRISE;
      case HOUR_NOON: return DayPhase.MIDDAY;
      case HOUR_SUNSET: return DayPhase.SUNSET;
      }
      if (HOUR_SUNRISE > hour) return DayPhase.DEEP_NIGHT;
      if (HOUR_NOON > hour) return DayPhase.MORNING;
      if (HOUR_SUNSET > hour) return DayPhase.AFTERNOON;
      return DayPhase.EVENING;
    }

    public WorldTime(WorldTime src) : this(src.TurnCounter)
    {
    }

    public WorldTime(int turnCounter=0)
    {
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
      return string.Format("day {0} hour {1:D2}", m_Day, m_Hour);
    }

    public static string MakeTimeDurationMessage(int turns)
    {
      if (turns < TURNS_PER_HOUR) return "less than a hour";
      if (turns < TURNS_PER_DAY) {
        int num = turns / TURNS_PER_HOUR;
        if (num == 1) return "about 1 hour";
        return string.Format("about {0} hours", (object) num);
      }
      WorldTime worldTime = new WorldTime(turns);
      if (worldTime.Day == 1) return "about 1 day";
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
        if (HOUR_SUNRISE > m_Hour) return Turn(m_Day, HOUR_SUNRISE) -TurnCounter;
        return HOUR_SUNRISE * TURNS_PER_HOUR;
      }
    }

    public int SunsetToDawnDuration {
      get {
        if (HOUR_SUNRISE > m_Hour) return Turn(m_Day, HOUR_SUNRISE) -TurnCounter;
        if (HOUR_SUNSET <= m_Hour) return Turn(m_Day+1, HOUR_SUNRISE) -TurnCounter;
        return (HOUR_SUNSET- HOUR_SUNRISE)*TURNS_PER_HOUR;
      }
    }
  }
}
