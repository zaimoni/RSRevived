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
      this.m_LocationsQueueSize = locationsToRemember;
      this.m_LocationsQueue = new Queue<Location>(locationsToRemember);
      this.m_ZonesQueueSize = zonesToRemember;
      this.m_ZonesQueue = new Queue<Zone>(zonesToRemember);
    }

    public void Clear()
    {
      this.m_LocationsQueue.Clear();
      this.m_ZonesQueue.Clear();
    }

    public bool HasExplored(Location loc)
    {
      return this.m_LocationsQueue.Contains(loc);
    }

    public void AddExplored(Location loc)
    {
      if (this.m_LocationsQueue.Count >= this.m_LocationsQueueSize)
        this.m_LocationsQueue.Dequeue();
      this.m_LocationsQueue.Enqueue(loc);
    }

    public bool HasExplored(Zone zone)
    {
      return this.m_ZonesQueue.Contains(zone);
    }

    public bool HasExplored(List<Zone> zones)
    {
      if (zones == null || zones.Count == 0)
        return true;
      foreach (Zone zone in zones)
      {
        if (!this.m_ZonesQueue.Contains(zone))
          return false;
      }
      return true;
    }

    public void AddExplored(Zone zone)
    {
      if (this.m_ZonesQueue.Count >= this.m_ZonesQueueSize)
        this.m_ZonesQueue.Dequeue();
      this.m_ZonesQueue.Enqueue(zone);
    }

    public void Update(Location location)
    {
      this.AddExplored(location);
      List<Zone> zonesAt = location.Map.GetZonesAt(location.Position.X, location.Position.Y);
      if (zonesAt == null || zonesAt.Count <= 0)
        return;
      foreach (Zone zone in zonesAt)
      {
        if (!this.HasExplored(zone))
          this.AddExplored(zone);
      }
    }
  }
}
