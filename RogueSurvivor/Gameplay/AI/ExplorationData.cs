// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.ExplorationData
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class ExplorationData
  {
    private const int EXPLORATION_LOCATIONS = WorldTime.TURNS_PER_HOUR;
    private const int EXPLORATION_ZONES = 3;    // unsure whether this space-time scales or not; WorldTime.TURNS_PER_HOUR/10 if it scales

    // alpha 10.1: Queue -> List
    private readonly List<Location> m_LocationsQueue = new List<Location>(EXPLORATION_LOCATIONS);
    private readonly List<Zone> m_ZonesQueue = new List<Zone>(EXPLORATION_ZONES);

    public ExplorationData() {}

#if DEAD_FUNC
    public void Clear()
    {
      m_LocationsQueue.Clear();
      m_ZonesQueue.Clear();
    }
#endif

    public bool HasExplored(in Location loc) { return m_LocationsQueue.Contains(loc); }

    private void AddExplored(in Location loc)
    {
      int i = m_LocationsQueue.IndexOf(loc);
      if (-1 < i) {
        if (m_LocationsQueue.Count - 1 <= i) return;
        m_LocationsQueue.RemoveAt(i);   // prevent duplicates
      } else if (m_LocationsQueue.Count >= EXPLORATION_LOCATIONS) m_LocationsQueue.RemoveAt(0);
      m_LocationsQueue.Add(loc);
    }

    // alpha10.1
    /// <param name="loc"></param>
    /// <returns>0 for locs not explored</returns>
    public int GetExploredAge(in Location loc)
    {
      int i = m_LocationsQueue.LastIndexOf(loc);    // Irrational caution (in case deduplication fails); IndexOf should be fine
      return (-1 < i) ? m_LocationsQueue.Count-i : 0;
    }

#if DEAD_FUNC
    public bool HasExplored(Zone zone)
    {
      return m_ZonesQueue.Contains(zone);
    }
#endif

    public bool HasExplored(List<Zone> zones)
    {
      return !zones?.Any(zone => !m_ZonesQueue.Contains(zone)) ?? true;
    }

    private void AddExplored(Zone zone)
    {
      int i = m_ZonesQueue.IndexOf(zone);
      if (-1 < i) {
        if (m_ZonesQueue.Count - 1 <= i) return;
        m_ZonesQueue.RemoveAt(i);   // prevent duplicates
      } else if (m_ZonesQueue.Count >= EXPLORATION_ZONES) m_ZonesQueue.RemoveAt(0);
      m_ZonesQueue.Add(zone);
    }

    private void AddExplored(Zone zone, Action<Zone> new_zone_handler)
    {
      int i = m_ZonesQueue.IndexOf(zone);
      if (-1 < i) {
        if (m_ZonesQueue.Count - 1 <= i) return;
        m_ZonesQueue.RemoveAt(i);   // prevent duplicates
      } else {
        if (m_ZonesQueue.Count >= EXPLORATION_ZONES) m_ZonesQueue.RemoveAt(0);
      }
      m_ZonesQueue.Add(zone);
    }

    // alpha10.1
    /// <param name="zone"></param>
    /// <returns>0 for zones not explored</returns>
    public int GetExploredAge(Zone zone)
    {
      int i = m_ZonesQueue.LastIndexOf(zone);    // Irrational caution (in case deduplication fails); IndexOf should be fine
      return (-1 < i) ? m_ZonesQueue.Count-i : 0;
    }

    /// <summary>
    /// Get age of most recently explored from list ("youngest")
    /// </summary>
    /// <param name="zones">can be null or empty, will return 0</param>
    /// <returns></returns>
    public int GetExploredAge(List<Zone> zones)
    {
      if (0 >= (zones?.Count ?? 0)) return 0;

      int youngestAge = int.MaxValue;
      foreach (Zone z in zones) {
        int age = GetExploredAge(z);
        if (age < youngestAge) youngestAge = age;
      }
      return youngestAge;
    }

    public void Update(Location location)
    {
      AddExplored(in location);
      var zonesAt = location.Map.GetZonesAt(location.Position);
      if (null == zonesAt || 0 >= zonesAt.Count) return;
      foreach (Zone zone in zonesAt) AddExplored(zone);
    }

    public void Update(Location location, Action<Zone> new_zone_handler)
    {
      AddExplored(in location);
      var zonesAt = location.Map.GetZonesAt(location.Position);
      if (null == zonesAt || 0 >= zonesAt.Count) return;
      foreach (Zone zone in zonesAt) AddExplored(zone, new_zone_handler);
    }
  }
}
