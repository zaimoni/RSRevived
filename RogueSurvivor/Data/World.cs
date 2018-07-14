// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.World
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define NOSKEW_SCHEDULER
#define SCHEDULER_IS_RACY

using System;
using System.Collections.Generic;
using System.Linq;
using Point = System.Drawing.Point;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class World
  {
    // alpha10
    // weather stays from 1h to 3 days and then change
    private const int WEATHER_MIN_DURATION = 1 * WorldTime.TURNS_PER_HOUR;
    private const int WEATHER_MAX_DURATION = 3 * WorldTime.TURNS_PER_DAY;

    // VAPORWARE: non-city districts outside of city limits (both gas station and National Guard base will be outside city limits)
    // static public readonly Point CHAR_CIty_Origin = new Point(0,0);  // VAPORWARE

    private readonly District[,] m_DistrictsGrid;
    private readonly int m_Size;
    private readonly Queue<District> m_PCready;
    private readonly Queue<District> m_NPCready;
    private readonly Queue<District> m_NPClive;
    public Weather Weather { get; private set; }
    public int NextWeatherCheckTurn { get; set; } // alpha10

    [NonSerialized]
    private Dictionary<Map, HashSet<Point>> m_BlankPositionDict;

    public int Size { get { return m_Size; } }

    public District this[int x, int y]
    {
      get {
#if DEBUG
        if (0> x || Size <= x) throw new ArgumentOutOfRangeException(nameof(x),x, "x must be between 0 and "+(Size-1).ToString()+" inclusive");
        if (0> y || Size <= y) throw new ArgumentOutOfRangeException(nameof(y),y, "x must be between 0 and "+(Size-1).ToString()+" inclusive");
#endif
        return m_DistrictsGrid[x, y];
      }
      set {
#if DEBUG
        if (0> x || Size <= x) throw new ArgumentOutOfRangeException(nameof(x),x, "x must be between 0 and "+(Size-1).ToString()+" inclusive");
        if (0> y || Size <= y) throw new ArgumentOutOfRangeException(nameof(y),y, "x must be between 0 and "+(Size-1).ToString()+" inclusive");
#endif
        m_DistrictsGrid[x, y] = value;
      }
    }

    // unsure that city/game world will be a square of districts indefinitely so use this wrapper
    /// <returns>The last district in the turn sequencing order</returns>
    public District Last { get { return m_DistrictsGrid[m_Size-1, m_Size-1]; } }

    // static bool WithinCityLimits(Point pos) { return true; }  // VAPORWARE
    
    // cannot return IEnumerable<District>, but this does not error
    public void DoForAllDistricts(Action<District> op)
    {
#if DEBUG
      if (null == op) throw new ArgumentNullException(nameof(op));
#endif
      foreach(District d in m_DistrictsGrid) op(d);
    }

    public void DoForAllActors(Action<Actor> op)
    {
#if DEBUG
      if (null == op) throw new ArgumentNullException(nameof(op));
#endif
      foreach(District d in m_DistrictsGrid) {
        foreach(Map m in d.Maps) {
          foreach(Actor a in m.Actors) op(a);
        }
      }
    }

    public World(int size)
    {
#if DEBUG
      if (0 >= size) throw new ArgumentOutOfRangeException(nameof(size),size, "0 >= size");
#endif
      m_DistrictsGrid = new District[size, size];
      m_Size = size;
//    Weather = Weather.CLEAR;
      Weather = (Weather)(RogueForm.Game?.Rules.Roll(0, (int)Weather._COUNT) ?? 0);
      NextWeatherCheckTurn = (RogueForm.Game?.Rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION) ?? 0);  // alpha10
      m_PCready = new Queue<District>(size*size);
      m_NPCready = new Queue<District>(size*size);
      m_NPClive = new Queue<District>(1);
    }

    // low-level utilities
    public Dictionary<Map, HashSet<Point>> BlankPositionDict {
      get {
        if (null == m_BlankPositionDict) {
          var tmp = new Dictionary<Map, HashSet<Point>>(m_BlankPositionDict);
          foreach(District d in m_DistrictsGrid) {
            foreach(Map m in d.Maps) {
              tmp[m] = new HashSet<Point>();
            }
          }
          m_BlankPositionDict = tmp;
        }
        return new Dictionary<Map, HashSet<Point>>(m_BlankPositionDict);
      }
    }

    public void DaimonMap()
    {
      if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
      Zaimoni.Data.OutTextFile dest = new Zaimoni.Data.OutTextFile(SetupConfig.DirPath + "\\daimon_map.html");

      // initiate HTML page so we can style things properly
      dest.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">");
      dest.WriteLine("<html><head>");
      // CSS styles
      dest.WriteLine("<style type='text/css'>\n<!--");
      dest.WriteLine("pre {font-family: 'Courier New', monospace; font-size:15px}");
      dest.WriteLine(".inv {font-size:11px}");
      dest.WriteLine(".car {font-size:7px}");
      dest.WriteLine(".lfort {font-size:11px}");
      dest.WriteLine(".chair {font-size:17px}");
      dest.WriteLine("-->\n</style>");
      dest.WriteLine("</head><body>");

      District viewpoint = Engine.RogueGame.CurrentMap.District;
      viewpoint.DaimonMap(dest);
      int x = 0;
      int y = 0;
      for(x = 0; x<Size; x++) {
        for(y = 0; y<Size; y++) {
          if (x== viewpoint.WorldPosition.X && y == viewpoint.WorldPosition.Y) continue;
          District d = this[x,y];
//        lock(d) { // this is causing a deadlock
            d.DaimonMap(dest);
//        }
        }
      }

      // typical HTML page termination
      dest.WriteLine("</body></html>");
      dest.Close();
    }

    public string WeatherChanges()
    {
      NextWeatherCheckTurn = Engine.Session.Get.WorldTime.TurnCounter + RogueForm.Game.Rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);
      switch (Weather) {
        case Weather.CLEAR:
          Weather = Weather.CLOUDY;
          return "Clouds are hiding the sky.";
        case Weather.CLOUDY:
          if (RogueForm.Game.Rules.RollChance(50)) {
            Weather = Weather.CLEAR;
            return "The sky is clear again.";
          }
          Weather = Weather.RAIN;
          return "Rain is starting to fall.";
        case Weather.RAIN:
          if (RogueForm.Game.Rules.RollChance(50)) {
            Weather = Weather.CLOUDY;
            return "The rain has stopped.";
          }
          Weather = Weather.HEAVY_RAIN;
          return "The weather is getting worse!";
#if DEBUG
        case Weather.HEAVY_RAIN:
#else
        default:
#endif
          Weather = Weather.RAIN;
          return "The rain is less heavy.";
#if DEBUG
        default: throw new InvalidOperationException("unhandled weather");
#endif
      }
    }

    // possible micro-optimization target
    public int PlayerCount {
      get {
        int ret = 0;
        for (int index1 = 0; index1 < m_Size; ++index1) {
          for (int index2 = 0; index2 < m_Size; ++index2)
            ret += m_DistrictsGrid[index1, index2].PlayerCount;
        }
        return ret;
      }
    }

    public List<District> PlayerDistricts {
      get {
        List<District> ret = new List<District>(m_Size*m_Size);
        for (int index1 = 0; index1 < m_Size; ++index1) {
          for (int index2 = 0; index2 < m_Size; ++index2) {
            if (0 < m_DistrictsGrid[index1, index2].PlayerCount) ret.Add(m_DistrictsGrid[index1, index2]);
          }
        }
        return ret;
      }
    }

    public void MakePC() {
      if (Engine.Session.CommandLineOptions.ContainsKey("PC")) {    // could be call graph precondition, but this doesn't have to be fast
        string[] names = Engine.Session.CommandLineOptions["PC"].Split('\0');
        foreach(District d in m_DistrictsGrid) {
          foreach(Map m in d.Maps) {
            foreach(Actor a in m.Actors) {
              if (!names.Contains(a.UnmodifiedName)) continue;
              if (a.IsPlayer) continue;
              a.Controller = new PlayerController();
            }
          }
        }
      }
    }

    // Simulation support
    // the public functions all lock on m_PCready in order to ensure thread aborts don't leave us in
    // an inconsistent state
    public void ScheduleForAdvancePlay() {
      lock (m_PCready) {
        ScheduleForAdvancePlay(m_DistrictsGrid[0,0]);
      }
    }

/*
 Idea here is to schedule the districts so that they never get "too far ahead" if we should want to build out cross-district pathfinding or line of sight.

 At game start for a 3x3 city, we have
 000
 000
 000

 i.e. only A1 is legal to run.  After it has run, we are at
 100
 000
 000

 We would like to schedule both A2 and B1, but A2 requires B1 to have already run.  After B1 has run:
 110
 000
 000

 B1 enables both C1 and A2 to be scheduled.  It's closer to "standard" to schedule C1 before A2.  After C1 and A2 runs:
 111
 100
 000

 we are now clear to schedule B2 (the first PC district in a 3x3 game).  After B2 has run
 111
 110
 000

 All of C2, A3, and A1 can be scheduled.  In "standard" we would defer A1 until after C2 had been scheduled, but that is a "global"
 constraint.  We do want A1 run twice before B2 is run twice.
 */
    private void ScheduleForAdvancePlay(District d)
    {
      District irrational_caution = d; // so we don't write to a locked variable while it is locked
retry:
      d = irrational_caution;
      if (m_PCready.Contains(d)) return;
      if (m_NPCready.Contains(d)) return;
      if (m_NPClive.Contains(d)) return;

      // these are based on morally readonly properties and thus can be used without a lock
      int x = d.WorldPosition.X;
      int y = d.WorldPosition.Y;
      District tmp = null;

#if SCHEDULER_IS_RACY
#else
      lock (d) {
#endif
        int district_turn = d.EntryMap.LocalTime.TurnCounter;
        // district 1 northwest must be at a strictly later gametime to not be lagged relative to us
        tmp = ((0 < x && 0 < y) ? m_DistrictsGrid[x - 1, y - 1] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 north must be at a strictly later gametime to not be lagged relative to us
        tmp = (0 < y ? m_DistrictsGrid[x, y - 1] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 northeast must be at a strictly later gametime to not be lagged relative to us
        tmp = ((0 < y && m_Size > x + 1) ? m_DistrictsGrid[x + 1, y - 1] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 west must be at a strictly later gametime to not be lagged relative to us
        tmp = (0 < x ? m_DistrictsGrid[x - 1, y] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 east must not be too far behind us
        tmp = (m_Size > x + 1 ? m_DistrictsGrid[x + 1,y] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 southwest must not be too far behind us
        tmp = ((m_Size > y + 1 && 0 < x) ? m_DistrictsGrid[x - 1, y + 1] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 south must not be too far behind us
        tmp = (m_Size > y + 1 ? m_DistrictsGrid[x, y + 1] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif
        // district 1 southeast must not be too far behind us
        tmp = ((m_Size > x + 1 && m_Size > y + 1) ? m_DistrictsGrid[x + 1,y + 1] : null);
#if SCHEDULER_IS_RACY
        if (null != tmp && tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
          irrational_caution = tmp;
          goto retry;
        }
#else
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
#endif

        // we're clear.
        if (d.RequiresUI) m_PCready.Enqueue(d);
        else m_NPCready.Enqueue(d);
#if SCHEDULER_IS_RACY
#else
      }
#endif
    }

    public void ScheduleAdjacentForAdvancePlay(District d)
    {
      // d.WorldPosition is morally readonly
      int x = d.WorldPosition.X;
      int y = d.WorldPosition.Y;
      District tmp_E = (m_Size > x + 1 ? m_DistrictsGrid[x + 1, y] : null);
      District tmp_SW = ((m_Size > y + 1 && 0 < x) ? m_DistrictsGrid[x - 1, y + 1] : null);
#if NOSKEW_SCHEDULER
#else
      District tmp_NW = ((0 < x && 0 < y) ? m_DistrictsGrid[x - 1, y - 1] : null);
#endif
#if FAIL
      District tmp_N = (0 < y ? m_DistrictsGrid[x, y - 1] : null);
      District tmp_W = (0 < x ? m_DistrictsGrid[x - 1, y] : null);
      District tmp_S = (m_Size > y + 1 ? m_DistrictsGrid[x, y + 1] : null);
      District tmp_NE = ((0 < y && m_Size > x + 1) ? m_DistrictsGrid[x + 1, y - 1] : null);
      District tmp_SE = ((m_Size > x + 1 && m_Size > y + 1) ? m_DistrictsGrid[x + 1, y + 1] : null);
#endif

      lock(m_PCready) {
        if (0 < m_PCready.Count && d == m_PCready.Peek()) m_PCready.Dequeue();
        if (0 < m_NPClive.Count && d == m_NPClive.Peek()) m_NPClive.Dequeue();

        // the ones that would typically be scheduled
        if (null != tmp_E) ScheduleForAdvancePlay(tmp_E);
        if (null != tmp_SW) ScheduleForAdvancePlay(tmp_SW);
#if NOSKEW_SCHEDULER
		if (Last == d) ScheduleForAdvancePlay(m_DistrictsGrid[0, 0]);
#else
        if (null != tmp_NW) ScheduleForAdvancePlay(tmp_NW);	// XXX causes global vs. local time skew
#endif

        // backstops
#if FAIL
        if (null != tmp_N) ScheduleForAdvancePlay(tmp_N);
        if (null != tmp_W) ScheduleForAdvancePlay(tmp_W);
        if (null != tmp_NE) ScheduleForAdvancePlay(tmp_NE);
        if (null != tmp_S) ScheduleForAdvancePlay(tmp_S);
        if (null != tmp_SE) ScheduleForAdvancePlay(tmp_SE);
#endif
      }
    }

    // avoiding property idiom for these as they affect World state
    public District CurrentPlayerDistrict()
    {
      lock(m_PCready) {
        while(0 < m_PCready.Count) {
          District tmp = m_PCready.Peek();
          if (1 == m_PCready.Count || tmp.RequiresUI) return tmp;
          m_NPCready.Enqueue(m_PCready.Dequeue());
        }
        if (0 >= m_NPCready.Count) return null;
        m_PCready.Enqueue(m_NPCready.Dequeue());
        return m_PCready.Peek();
      }
    }

    public District CurrentSimulationDistrict()
    {
      lock(m_PCready) {
        while(0 == m_NPClive.Count && 0 < m_NPCready.Count) {
          District tmp = m_NPCready.Peek();
          if (!tmp.RequiresUI) m_NPClive.Enqueue(m_NPCready.Dequeue());
          else m_PCready.Enqueue(m_NPCready.Dequeue());
        }
        if (0 < m_NPClive.Count) return m_NPClive.Peek();
      }
      return null;
    }

    public void TrimToBounds(ref int x, ref int y)
    {
      if (x < 0) x = 0;
      else if (x >= m_Size) x = m_Size - 1;
      if (y < 0) y = 0;
      else if (y >= m_Size) y = m_Size - 1;
    }

    public static string CoordToString(int x, int y)
    {
      return string.Format("{0}{1}", (object) (char) (65 + x), (object) y);
    }

    public void OptimizeBeforeSaving()
    {
      for (int index1 = 0; index1 < m_Size; ++index1) {
        for (int index2 = 0; index2 < m_Size; ++index2)
          m_DistrictsGrid[index1, index2].OptimizeBeforeSaving();
      }
    }
  }
}
