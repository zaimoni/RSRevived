// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.ExplorationData
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class ExplorationData
  {
    private const int EXPLORATION_LOCATIONS = WorldTime.TURNS_PER_HOUR;
    private const int EXPLORATION_ZONES = 3;

    private readonly Queue<Location> m_LocationsQueue;
    private readonly Queue<Zone> m_ZonesQueue;

    public ExplorationData()
    {
      m_LocationsQueue = new Queue<Location>(EXPLORATION_LOCATIONS);
      m_ZonesQueue = new Queue<Zone>(EXPLORATION_ZONES);
    }

    public void Clear()
    {
      m_LocationsQueue.Clear();
      m_ZonesQueue.Clear();
    }

    public bool HasExplored(Location loc)
    {
      return m_LocationsQueue.Contains(loc);
    }

    private void AddExplored(Location loc)
    {
      if (m_LocationsQueue.Count >= EXPLORATION_LOCATIONS)
        m_LocationsQueue.Dequeue();
      m_LocationsQueue.Enqueue(loc);
    }

    public bool HasExplored(Zone zone)
    {
      return m_ZonesQueue.Contains(zone);
    }

    public bool HasExplored(List<Zone> zones)
    {
      return !zones?.Any(zone => !m_ZonesQueue.Contains(zone)) ?? true;
    }

    private void AddExplored(Zone zone)
    {
      if (m_ZonesQueue.Count >= EXPLORATION_ZONES)
        m_ZonesQueue.Dequeue();
      m_ZonesQueue.Enqueue(zone);
    }

    public void Update(Location location)
    {
      AddExplored(location);
      List<Zone> zonesAt = location.Map.GetZonesAt(location.Position);
      if (zonesAt == null || zonesAt.Count <= 0) return;
      foreach (Zone zone in zonesAt) {
        if (!HasExplored(zone)) AddExplored(zone);
      }
    }
  }
}
