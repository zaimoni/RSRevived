﻿// Decompiled with JetBrains decompiler
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
    public const int HOURS_PER_DAY = 24;    // not scalable
    public const int TURNS_PER_HOUR = 30;   // defines space-time scale
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
        Contract.Ensures(0<= m_TurnCounter);
        return m_TurnCounter;
      }
      set {
        Contract.Requires(0<=value);
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
      Contract.Requires(null!=src);
    }

    public WorldTime(int turnCounter=0)
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
      Contract.Requires(0<=day);
      Contract.Requires(0<=hour);
      Contract.Requires(int.MaxValue/TURNS_PER_DAY>=day);
      Contract.Requires((int.MaxValue-(TURNS_PER_DAY*day))/TURNS_PER_HOUR>=hour);
      return TURNS_PER_DAY*day + TURNS_PER_HOUR*hour;
    }
  }
}
