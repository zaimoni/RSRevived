// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.World
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;

#nullable enable

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
    static public readonly Point CHAR_City_Origin = new Point(0,0);  // may need to be a proper Rectangle?

    private readonly District[,] m_DistrictsGrid;
    private readonly short m_Size;
    private District? m_PlayerDistrict = null;
    private District? m_SimDistrict = null;
    private readonly Queue<District> m_Ready;   // \todo this is expected to have a small maximum that can be hard-coded; measure it
    public Weather Weather { get; private set; }
    public int NextWeatherCheckTurn { get; private set; } // alpha10

    public short Size { get { return m_Size; } }
    public short CitySize { get { return m_Size; } }  // not guaranteed to be the same as the above

    public bool InBounds(int x,int y) {
      return 0 <= x && m_Size > x && 0 <= y && m_Size > y;
    }
    public bool InBounds(Point pt) {
      return 0 <= pt.X && m_Size > pt.X && 0 <= pt.Y && m_Size > pt.Y;
    }
    public District? At(int x, int y) { return InBounds(x, y) ? m_DistrictsGrid[x, y] : null; }
    public District? At(Point pt) { return InBounds(pt) ? m_DistrictsGrid[pt.X, pt.Y] : null; }


    public District this[int x, int y]
    {
      get {
#if DEBUG
        if (!InBounds(x, y)) throw new InvalidOperationException("not in bounds");
#endif
        return m_DistrictsGrid[x, y];
      }
      set {
#if DEBUG
        if (!InBounds(x, y)) throw new InvalidOperationException("not in bounds");
#endif
        m_DistrictsGrid[x, y] = value;
      }
    }

    public District this[Point pt]
    {
      get {
#if DEBUG
        if (!InBounds(pt)) throw new InvalidOperationException("not in bounds");
#endif
        return m_DistrictsGrid[pt.X, pt.Y];
      }
      set {
#if DEBUG
        if (!InBounds(pt)) throw new InvalidOperationException("not in bounds");
#endif
        m_DistrictsGrid[pt.X, pt.Y] = value;
      }
    }

    // unsure that city/game world will be a square of districts indefinitely so use this wrapper
    /// <returns>The last district in the turn sequencing order</returns>
    public District Last { get { return m_DistrictsGrid[m_Size - 1, m_Size - 1]; } }
    /// <returns>district is on east edge of world</returns>
    public bool Edge_E(District d) { return d.WorldPosition.X == m_Size - 1; }
    /// <returns>district is on south edge of world</returns>
    public bool Edge_S(District d) { return d.WorldPosition.Y == m_Size - 1; }
    /// <returns>district is on north or east edge of world</returns>
    public bool Edge_N_or_E(District d) { return 0 >= d.WorldPosition.X || 0 >= d.WorldPosition.Y; }    // arguably should be 0 ==

    // static bool WithinCityLimits(Point pos) { return true; }  // VAPORWARE
    // supported layouts are: E-W, N-S, 
    // the four Ts with 3 cardinal directions linked to neutral (represented as two line segments)
    // 4-way interesection (represented as two line segments, E-W and N-S)
    // diagonals: S-E, S-W, N-E, N-W
    public uint SubwayLayout(Point pos)
    {
      if (40 > Engine.RogueGame.Options.DistrictSize) return 0; // 30 is known to be impossible to get a subway station.  40 is ok
      // precompute some line segments
      const uint E_W = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint N_S = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint N_E = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.E;
      const uint N_W = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint S_E = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint S_W = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint N_NEUTRAL = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint E_NEUTRAL = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint S_NEUTRAL = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint W_NEUTRAL = (uint)Compass.XCOMlike.W * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint FOUR_WAY = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;
      const uint N_TEE = N_NEUTRAL * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;
      const uint S_TEE = S_NEUTRAL * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_W;
      const uint E_TEE = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + E_NEUTRAL;
      const uint W_TEE = N_S * (uint)Compass.reference.XCOM_LINE_SEGMENT_UB + W_NEUTRAL;
      if (0 == pos.Y) {
        if (0 == pos.X) return S_E;
        else if (Size / 2 == pos.X) return S_TEE;
        else if (Size - 1 == pos.X) return S_W;
        else return E_W;
      } else if (Size  / 2 == pos.Y) {
        if (0 == pos.X) return E_TEE;
        else if (Size / 2 == pos.X) return FOUR_WAY;
        else if (Size - 1 == pos.X) return W_TEE;
        else return E_W;
      } else if (Size -1 == pos.Y) {
        if (0 == pos.X) return N_E;
        else if (Size / 2 == pos.X) return N_TEE;
        else if (Size - 1 == pos.X) return N_W;
        else return E_W;
      } else if (0 == pos.X) return N_S;
      else if (Size / 2 == pos.X) return N_S;
      else if (Size - 1 == pos.X) return N_S;
      return 0; // any valid layout will have at least one line segment and thus be non-zero
    }

    // cannot return IEnumerable<District>, but this does not error
    public void DoForAllDistricts(Action<District> op)
    {
      foreach(District d in m_DistrictsGrid) op(d);
    }

    public void DoForAllMaps(Action<Map> op,Predicate<District> ok)
    {
      foreach(District d in m_DistrictsGrid) {
        if (null != ok && !ok(d)) continue;
        foreach(Map m in d.Maps) op(m);
      }
    }

    public void DoForAllMaps(Action<Map> op)
    {
      foreach(District d in m_DistrictsGrid) {
        foreach(Map m in d.Maps) op(m);
      }
    }

    public bool Any(Predicate<Map> test)
    {
      foreach(District d in m_DistrictsGrid) {
        foreach(Map m in d.Maps) if (test(m)) return true;
      }
      return false;
    }

    public void DoForAllActors(Action<Actor> op) { foreach(District d in m_DistrictsGrid) d.DoForAllActors(op); }
    public void DoForAllGroundInventories(Action<Location,Inventory> op) { foreach (District d in m_DistrictsGrid) d.DoForAllGroundInventories(op); }

    public World(short size)
    {
#if DEBUG
      if (0 >= size) throw new ArgumentOutOfRangeException(nameof(size),size, "0 >= size");
#endif
      m_DistrictsGrid = new District[size, size];
      m_Size = size;
//    Weather = Weather.CLEAR;
      var rules = Engine.Rules.Get;
      Weather = (Weather)(rules.Roll(0, (int)Weather._COUNT));
      NextWeatherCheckTurn = rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);  // alpha10
      m_Ready = new Queue<District>(size*size);
    }

    public HashSet<Actor>? PoliceInRadioRange(Location loc, Predicate<Actor>? test =null)
    {
      Location radio_pos = Engine.Rules.PoliceRadioLocation(loc);
      HashSet<Actor>? ret = null;
      DoForAllMaps(m=> {
          foreach (var a in m.Police.Get) {
              if (a.Location == loc) continue;
              if (null!=test && !test(a)) continue;
              Location other_radio_pos = Engine.Rules.PoliceRadioLocation(a.Location);
              if (Engine.RogueGame.POLICE_RADIO_RANGE >= Engine.Rules.GridDistance(radio_pos, in other_radio_pos)) (ret ??= new HashSet<Actor>()).Add(a); //  \todo change target for range reduction from being underground
          }
      });
      return ret;
    }

    public HashSet<Actor>? EveryoneInPoliceRadioRange(Location loc, Predicate<Actor>? test=null)
    {
      Location radio_pos = Engine.Rules.PoliceRadioLocation(loc);
      HashSet<Actor>? ret = null;
      DoForAllMaps(m=> {
          foreach (var a in m.Actors) {
              if (a.Location == loc) continue;
              if (!a.HasActivePoliceRadio) continue;
              if (null!=test && !test(a)) continue;
              Location other_radio_pos = Engine.Rules.PoliceRadioLocation(a.Location);
              if (Engine.RogueGame.POLICE_RADIO_RANGE >= Engine.Rules.GridDistance(radio_pos, in other_radio_pos)) (ret ??= new HashSet<Actor>()).Add(a); //  \todo change target for range reduction from being underground
          }
      });
      return ret;
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
      dest.WriteLine(".power {font-size:8px}");
      dest.WriteLine(".lfort {font-size:11px}");
      dest.WriteLine(".chair {font-size:17px}");
      dest.WriteLine("-->\n</style>");
      dest.WriteLine("</head><body>");

      District viewpoint = Engine.RogueGame.CurrentMap.District;
      viewpoint.DaimonMap(dest);
      for(int x = 0; x<Size; x++) {
        for(int y = 0; y<Size; y++) {
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
      var rules = Engine.Rules.Get;
      NextWeatherCheckTurn = Engine.Session.Get.WorldTime.TurnCounter + rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);
      switch (Weather) {
        case Weather.CLEAR:
          Weather = Weather.CLOUDY;
          return "Clouds are hiding the sky.";
        case Weather.CLOUDY:
          if (rules.RollChance(50)) {
            Weather = Weather.CLEAR;
            return "The sky is clear again.";
          }
          Weather = Weather.RAIN;
          return "Rain is starting to fall.";
        case Weather.RAIN:
          if (rules.RollChance(50)) {
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

    // Simulation support
    // the public functions all lock on m_PCready in order to ensure thread aborts don't leave us in
    // an inconsistent state
    public void ScheduleForAdvancePlay() {
      lock(m_Ready) {
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
    private void ScheduleForAdvancePlay(District d, District? origin=null)
    {
      District irrational_caution = d; // retain original district for debugging purposes
      if (irrational_caution == m_PlayerDistrict) return;
      if (irrational_caution == m_SimDistrict) return;
      if (m_Ready.Contains(irrational_caution)) return;

      // these are based on morally readonly properties and thus can be used without a lock
      int district_turn = irrational_caution.EntryMap.LocalTime.TurnCounter;

      foreach(var pt in d.WorldPosition.Adjacent()) {
        var test = At(pt);
        if (null == test) continue;
        if (test.IsBefore(irrational_caution) && test.EntryMap.LocalTime.TurnCounter <= district_turn) return;  // reject causality loop
      }

      // we're clear.
      m_Ready.Enqueue(irrational_caution);
    }

    public void ScheduleAdjacentForAdvancePlay(District d)
    {
      // d.WorldPosition is morally readonly
      var tmp_E = At(d.WorldPosition + Direction.E);
      var tmp_SW = At(d.WorldPosition + Direction.SW);
      // other directions not needed.  An early protoype also used Direction.NW but this caused global vs. local time skew

      lock (m_Ready) {
#if DEBUG
        if (m_Ready.Contains(d)) throw new InvalidOperationException("already-complete district "+d.Name+" scheduled");
#endif
        Interlocked.CompareExchange(ref m_PlayerDistrict, null, d);
        Interlocked.CompareExchange(ref m_SimDistrict, null, d);
        // the ones that would typically be scheduled
        if (null != tmp_E) ScheduleForAdvancePlay(tmp_E,d);
#if DEBUG
        if (m_Ready.Contains(d)) throw new InvalidOperationException("already-complete district "+d.Name+" scheduled");
#endif
        if (null != tmp_SW) ScheduleForAdvancePlay(tmp_SW, d);
#if DEBUG
        if (m_Ready.Contains(d)) throw new InvalidOperationException("already-complete district "+d.Name+" scheduled");
#endif
		if (0 >= m_Ready.Count && null == m_PlayerDistrict && null== m_SimDistrict) ScheduleForAdvancePlay(m_DistrictsGrid[0, 0], d);
#if DEBUG
        if (m_Ready.Contains(d)) throw new InvalidOperationException("already-complete district "+d.Name+" scheduled");
#endif
      }
    }

    // avoiding property idiom for these as they affect World state
    public District? CurrentPlayerDistrict()
    {
      if (null != m_PlayerDistrict) return m_PlayerDistrict;
      lock (m_Ready) {
restart:
        if (0 >= m_Ready.Count) return null;
        District tmp = m_Ready.Dequeue();
        if (tmp.RequiresUI || null != m_SimDistrict) {
          Interlocked.CompareExchange(ref m_PlayerDistrict, tmp, null);
          return m_PlayerDistrict;
        }
        Interlocked.CompareExchange(ref m_SimDistrict, tmp, null);
        goto restart;
      }
    }

    public District? CurrentSimulationDistrict()
    {
      if (null != m_SimDistrict) return m_SimDistrict;
      lock (m_Ready) {
        if (0 >= m_Ready.Count) return null;
        if (!m_Ready.Any(d => !d.RequiresUI)) return null;
        District? tmp = null;
        while((tmp = m_Ready.Dequeue()).RequiresUI) m_Ready.Enqueue(tmp);
        Interlocked.CompareExchange(ref m_SimDistrict, tmp, null);
        return m_SimDistrict;
      }
    }

    private void _RejectActorActorInventoryCrossLink(List<string> errors, Actor origin)
    {
      var o_inv = origin.Inventory;
      if (null == o_inv || o_inv.IsEmpty) return;
      DoForAllActors(a => {
        if (a == origin) return;
        var err = a.Inventory?._HasCrossLink(o_inv);
        if (!string.IsNullOrEmpty(err)) errors.Add(origin.Name +", " + a.Name +" " + err);
      });
    }

    private void _RejectActorActorInventoryCrossLink(List<string> errors)
    {
      DoForAllActors(origin => _RejectActorActorInventoryCrossLink(errors, origin));
    }

    private void _RejectGroundGroundInventoryCrossLink(List<string> errors, Location o_loc, Inventory o_inv)
    {
      if (o_inv.IsEmpty) return;
      DoForAllGroundInventories((loc, inv) => {
        if (o_loc == loc) return;
        if (o_inv == inv) {
          errors.Add("bi-located inventory: "+o_loc+"; "+loc);
          return;
        }
        if (inv.IsEmpty) return;
        var err = inv._HasCrossLink(o_inv);
        if (!string.IsNullOrEmpty(err)) errors.Add(o_loc + "; " + loc + ": " + err);
      });
    }

    private void _RejectGroundGroundInventoryCrossLink(List<string> errors)
    {
      DoForAllGroundInventories((loc,inv) => _RejectGroundGroundInventoryCrossLink(errors, loc, inv));
    }

    private void _RejectActorGroundInventoryCrossLink(List<string> errors, Actor origin)
    {
      var o_inv = origin.Inventory;
      if (null == o_inv || o_inv.IsEmpty) return;
      DoForAllGroundInventories((loc, inv) => {
        if (inv.IsEmpty) return;
        var err = inv._HasCrossLink(o_inv);
        if (!string.IsNullOrEmpty(err)) errors.Add(origin.Name + "; " + loc + ": " + err);
      });
    }

    private void _RejectActorGroundInventoryCrossLink(List<string> errors)
    {
      DoForAllActors(origin => _RejectActorGroundInventoryCrossLink(errors, origin));
    }

    private void _RejectActorInventoryZero(List<string> errors)
    {
      DoForAllActors(a => {
        var err = a.Inventory?._HasZeroQuantityOrDuplicate();
        if (!string.IsNullOrEmpty(err)) errors.Add(a.Name +": " + err +": " + a.Inventory);
      });
    }

    private void _RejectActorInventoryMultiEquipped(List<string> errors)
    {
      DoForAllActors(a => {
        var err = a.Inventory?._HasMultiEquippedItems();
        if (!string.IsNullOrEmpty(err)) errors.Add(a.Name +": " + err +": " + a.Inventory);
      });
    }

    private void _RejectGroundInventoryZero(List<string> errors)    // multi-thread sensitive
    {
      DoForAllGroundInventories((loc,inv) => {
        if (loc.Map.District == (Engine.RogueGame.IsSimulating ? m_PlayerDistrict : m_SimDistrict)) return;
        var err = inv._HasZeroQuantityOrDuplicate();
        if (!string.IsNullOrEmpty(err)) errors.Add(loc + ": " + err +": " + inv);
      });
    }

    [Conditional("DEBUG")]
    public void _RejectInventoryDamage(List<string> errors)
    {
      // repairable checks
RestartActorZeroCheck:
      try {
        _RejectActorInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorZeroCheck;
        throw;
      }
RestartActorMultiEquippedCheck:
      try {
        _RejectActorInventoryMultiEquipped(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorMultiEquippedCheck;
        throw;
      }
RestartGroundZeroCheck:
      try {
        _RejectGroundInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartGroundZeroCheck;
        throw;
      }
RestartActorGroundCrossLinkCheck:
      try {
        _RejectActorGroundInventoryCrossLink(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorGroundCrossLinkCheck;
        throw;
      }
      // non-repairable checks (context-sensitive)
RestartActorActorCrossLinkCheck:
      try {
        _RejectActorActorInventoryCrossLink(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorActorCrossLinkCheck;
        throw;
      }
RestartGroundGroundCrossLinkCheck:
      try {
        _RejectGroundGroundInventoryCrossLink(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartGroundGroundCrossLinkCheck;
        throw;
      }
    }

    [Conditional("DEBUG")]
    public void _RejectInventoryDamage(List<string> errors, Actor a)
    {
RestartActorZeroCheck:
      try {
        _RejectActorInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorZeroCheck;
        throw;
      }
RestartGroundZeroCheck:
      try {
        _RejectGroundInventoryZero(errors);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartGroundZeroCheck;
        throw;
      }
RestartActorGroundCrossLinkCheck:
      try {
        _RejectActorGroundInventoryCrossLink(errors, a);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorGroundCrossLinkCheck;
        throw;
      }
      // non-repairable checks (context-sensitive)
RestartActorActorCrossLinkCheck:
      try {
        _RejectActorActorInventoryCrossLink(errors, a);
      } catch (InvalidOperationException e) {
        if (e.Message.Contains("Collection was modified")) goto RestartActorActorCrossLinkCheck;
        throw;
      }
    }

    public void TrimToBounds(ref Rectangle src)
    {
      var test = src.Left;
      if (0>test) {
        src.Width += test;
        src.X = 0;
      }

      test = src.Right;
      if (Size < test) src.Width -= (short)(test-Size);

      test = src.Top;
      if (0>test) {
        src.Height += test;
        src.Y = 0;
      }

      test = src.Bottom;
      if (Size < test) src.Width -= (short)(test-Size);
    }

    public static string CoordToString(int x, int y)
    {
      return string.Format("{0}{1}", (object) (char) (65 + x), (object) y);
    }
  }
}
