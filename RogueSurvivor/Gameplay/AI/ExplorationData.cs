// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.ExplorationData
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class ExplorationData
  {
    private int m_LocationsQueueSize;
    private Queue<Location> m_LocationsQueue;
    private int m_ZonesQueueSize;
    private Queue<Zone> m_ZonesQueue;

    public ExplorationData(int locationsToRemember, int zonesToRemember)
    {
      if (locationsToRemember < 1)
        throw new ArgumentOutOfRangeException("locationsQueueSize < 1");
      if (zonesToRemember < 1)
        throw new ArgumentOutOfRangeException("zonesQueueSize < 1");
            m_LocationsQueueSize = locationsToRemember;
            m_LocationsQueue = new Queue<Location>(locationsToRemember);
            m_ZonesQueueSize = zonesToRemember;
            m_ZonesQueue = new Queue<Zone>(zonesToRemember);
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

    public void AddExplored(Location loc)
    {
      if (m_LocationsQueue.Count >= m_LocationsQueueSize)
                m_LocationsQueue.Dequeue();
            m_LocationsQueue.Enqueue(loc);
    }

    public bool HasExplored(Zone zone)
    {
      return m_ZonesQueue.Contains(zone);
    }

    public bool HasExplored(List<Zone> zones)
    {
      if (zones == null || zones.Count == 0)
        return true;
      foreach (Zone zone in zones)
      {
        if (!m_ZonesQueue.Contains(zone))
          return false;
      }
      return true;
    }

    public void AddExplored(Zone zone)
    {
      if (m_ZonesQueue.Count >= m_ZonesQueueSize)
                m_ZonesQueue.Dequeue();
            m_ZonesQueue.Enqueue(zone);
    }

    public void Update(Location location)
    {
            AddExplored(location);
      List<Zone> zonesAt = location.Map.GetZonesAt(location.Position.X, location.Position.Y);
      if (zonesAt == null || zonesAt.Count <= 0)
        return;
      foreach (Zone zone in zonesAt)
      {
        if (!HasExplored(zone))
                    AddExplored(zone);
      }
    }
  }
}
