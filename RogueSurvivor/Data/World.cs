// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.World
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class World
  {
    private District[,] m_DistrictsGrid;
    private int m_Size;
    private Queue<District> m_PCready;
    private Queue<District> m_NPCready;

    public Weather Weather { get; set; }

    public int Size
    {
      get {
        return m_Size;
      }
    }

    public District this[int x, int y]
    {
      get {
        if (x < 0 || x >= m_Size) throw new ArgumentOutOfRangeException("x");
        if (y < 0 || y >= m_Size) throw new ArgumentOutOfRangeException("y");
        return m_DistrictsGrid[x, y];
      }
      set {
        if (x < 0 || x >= m_Size) throw new ArgumentOutOfRangeException("x");
        if (y < 0 || y >= m_Size) throw new ArgumentOutOfRangeException("y");
        m_DistrictsGrid[x, y] = value;
      }
    }

    public World(int size)
    {
      if (size <= 0) throw new ArgumentOutOfRangeException("size <=0");
      m_DistrictsGrid = new District[size, size];
      m_Size = size;
      Weather = Weather.CLEAR;
      m_PCready = new Queue<District>(size*size);
      m_NPCready = new Queue<District>(size*size);
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
    public void ScheduleForAdvancePlay() {
      ScheduleForAdvancePlay(m_DistrictsGrid[0,0]);
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

 i.e. B1 and A2 are legal to run.  After both of those have run we are at
 110
 100
 000

 and all of A0,C0,B1,A2 are legal to run.  We would prefer to run A0 last.
 */
    private void ScheduleForAdvancePlay(District d)
    {
      if (m_PCready.Contains(d)) return;
      if (m_NPCready.Contains(d)) return;

      District irrational_caution = d; // so we don't write to a locked variable while it is locked
      // these are based on morally readonly properties and thus can be used without a lock
retry:
      d = irrational_caution;
      int x = d.WorldPosition.X;
      int y = d.WorldPosition.Y;
      District tmp = null;

      lock (d) { 
        int district_turn = d.EntryMap.LocalTime.TurnCounter;
        // district 1 north must be at a strictly later gametime to not be lagged relative to us
        tmp = (0 < y ? m_DistrictsGrid[x, y - 1] : null);
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
        // district 1 west must be at a strictly later gametime to not be lagged relative to us
        tmp = (0 < x ? m_DistrictsGrid[x - 1, y] : null);
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter <= district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
        // district 1 south must not be too far behind us
        tmp = (m_Size > y + 1 ? m_DistrictsGrid[x, y + 1] : null);
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
        // district 1 east must not be too far behind us
        tmp = (m_Size > x+1 ? m_DistrictsGrid[x+1,y] : null);
        if (null != tmp) {
          lock(tmp) {
            if (tmp.EntryMap.LocalTime.TurnCounter < district_turn) {
              irrational_caution = tmp;
              goto retry;
            }
          }
        }
        // we're clear.
        if (0 < d.PlayerCount) m_PCready.Enqueue(d);
        else m_NPCready.Enqueue(d);
      }
    }

    public void ScheduleAdjacentForAdvancePlay(District d)
    {
      int x = d.WorldPosition.X;
      int y = d.WorldPosition.Y;
      District tmp_N = (0 < y ? m_DistrictsGrid[x, y - 1] : null);
      District tmp_W = (0 < x ? m_DistrictsGrid[x - 1, y] : null);
      District tmp_S = (m_Size > y + 1 ? m_DistrictsGrid[x, y + 1] : null);
      District tmp_E = (m_Size > x + 1 ? m_DistrictsGrid[x + 1, y] : null);
      if (null != tmp_S) ScheduleForAdvancePlay(tmp_S);
      if (null != tmp_E) ScheduleForAdvancePlay(tmp_E);
      if (null != tmp_N) ScheduleForAdvancePlay(tmp_N);
      if (null != tmp_W) ScheduleForAdvancePlay(tmp_W);
    }

    // avoiding property idiom for these two as they affect World state
    public District NextPlayerDistrict()
    {
      while(0 < m_PCready.Count) {
        District tmp = m_PCready.Dequeue();
        if (0 < tmp.PlayerCount) return tmp;
        m_NPCready.Enqueue(tmp);
      }
      return null;
    }

    public District NextSimulationDistrict()
    {
      while(0 < m_NPCready.Count) {
        District tmp = m_NPCready.Dequeue();
        if (0 == tmp.PlayerCount) return tmp;
        m_PCready.Enqueue(tmp);
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
