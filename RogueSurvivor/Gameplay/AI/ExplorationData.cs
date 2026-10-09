// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.ExplorationData
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  public sealed class ExplorationData
  {
    private const int EXPLORATION_LOCATIONS = WorldTime.TURNS_PER_HOUR;
    private const int EXPLORATION_ZONES = 3;    // unsure whether this space-time scales or not; WorldTime.TURNS_PER_HOUR/10 if it scales

    // for BaseAI/OrderableAI::BehaviorExplore
    private const int SCORE_LOCS = 500;
    public const int SCORE_BARRICADES = 100;
    public const int SCORE_INOUT = 50;
    private const int SCORE_DELTA = ((SCORE_LOCS + SCORE_BARRICADES + SCORE_INOUT) / (EXPLORATION_LOCATIONS - 4)) + 1;

    // alpha 10.1: Queue -> List
    private readonly List<Location> m_LocationsQueue = new(EXPLORATION_LOCATIONS);
    private readonly List<ZoneLoc> m_ZonesQueue = new(EXPLORATION_ZONES);

    public ExplorationData() {}

#region JSON save/load
    // A zone does not know its map; a ZoneLoc does, so the zones are saved as ZoneLoc ([map, rectangle]).
    public ExplorationData(JsonElement src, JsonSerializerOptions options)
    {
      if (src.TryGetProperty("Locations", out var x)) m_LocationsQueue.AddRange(x.Deserialize<List<Location>>(options) ?? throw new JsonException("null Locations"));
      if (src.TryGetProperty("Zones", out x)) m_ZonesQueue.AddRange(x.Deserialize<List<ZoneLoc>>(options) ?? throw new JsonException("null Zones"));
    }

    public bool IsEmpty { get => 0 >= m_LocationsQueue.Count && 0 >= m_ZonesQueue.Count; }

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      if (0 < m_LocationsQueue.Count) writer.WriteArray("Locations", m_LocationsQueue, options);
      if (0 < m_ZonesQueue.Count) writer.WriteArray("Zones", m_ZonesQueue, options);
      writer.WriteEndObject();
    }
#endregion

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

#if DEAD_FUNC
    // alpha10.1
    /// <param name="loc"></param>
    /// <returns>0 for locs not explored</returns>
    public int GetExploredAge(in Location loc)
    {
      int i = m_LocationsQueue.LastIndexOf(loc);    // Irrational caution (in case deduplication fails); IndexOf should be fine
      return (-1 < i) ? m_LocationsQueue.Count-i : 0;
    }
#endif
    public int AgeScore(in Location loc) {
      int i = m_LocationsQueue.LastIndexOf(loc);    // Irrational caution (in case deduplication fails); IndexOf should be fine
      if (0 > i) return SCORE_LOCS;
      return (m_LocationsQueue.Count - i)*SCORE_DELTA - SCORE_BARRICADES - SCORE_INOUT;
    }

    public bool HasExplored(Map m, List<Zone>? zones)
    {
      return !zones?.Any(zone => !m_ZonesQueue.Contains(new ZoneLoc(m, zone))) ?? true;
    }

    private void AddExplored(ZoneLoc zone)
    {
      int i = m_ZonesQueue.IndexOf(zone);
      if (-1 < i) {
        if (m_ZonesQueue.Count - 1 <= i) return;
        m_ZonesQueue.RemoveAt(i);   // prevent duplicates
      } else if (m_ZonesQueue.Count >= EXPLORATION_ZONES) m_ZonesQueue.RemoveAt(0);
      m_ZonesQueue.Add(zone);
    }

    private void AddExplored(ZoneLoc zone, Action<ZoneLoc> new_zone_handler)
    {
      int i = m_ZonesQueue.IndexOf(zone);
      if (-1 < i) {
        if (m_ZonesQueue.Count - 1 <= i) return;
        m_ZonesQueue.RemoveAt(i);   // prevent duplicates
      } else if (m_ZonesQueue.Count >= EXPLORATION_ZONES) m_ZonesQueue.RemoveAt(0);
      m_ZonesQueue.Add(zone);
      new_zone_handler(zone);
    }

#if DEAD_FUNC
    // alpha10.1
    /// <param name="zone"></param>
    /// <returns>0 for zones not explored</returns>
    public int GetExploredAge(ZoneLoc zone)
    {
      int i = m_ZonesQueue.LastIndexOf(zone);    // Irrational caution (in case deduplication fails); IndexOf should be fine
      return (-1 < i) ? m_ZonesQueue.Count-i : 0;
    }

    /// <summary>
    /// Get age of most recently explored from list ("youngest")
    /// </summary>
    /// <param name="zones">can be null or empty, will return 0</param>
    /// <returns></returns>
    public int GetExploredAge(List<ZoneLoc> zones)
    {
      if (0 >= (zones?.Count ?? 0)) return 0;

      int youngestAge = int.MaxValue;
      foreach (ZoneLoc z in zones) {
        int age = GetExploredAge(z);
        if (age < youngestAge) youngestAge = age;
      }
      return youngestAge;
    }
#endif

        public void Update(Location location)
    {
      AddExplored(in location);
      var zonesAt = location.Map.GetZonesAt(location.Position);
      if (null == zonesAt || 0 >= zonesAt.Count) return;
      foreach (Zone zone in zonesAt) AddExplored(new ZoneLoc(location.Map, zone));
    }

    public void Update(Location location, Action<ZoneLoc> new_zone_handler)
    {
      AddExplored(in location);
      var zonesAt = location.Map.GetZonesAt(location.Position);
      if (null == zonesAt || 0 >= zonesAt.Count) return;
      foreach (Zone zone in zonesAt) AddExplored(new ZoneLoc(location.Map, zone), new_zone_handler);
    }
  }
}
