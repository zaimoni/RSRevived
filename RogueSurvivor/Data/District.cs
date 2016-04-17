// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.District
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class District
  {
    private List<Map> m_Maps = new List<Map>(3);
    private Point m_WorldPosition;
    private DistrictKind m_Kind;
    private string m_Name;
    private Map m_EntryMap;
    private Map m_SewersMap;
    private Map m_SubwayMap;

    public Point WorldPosition
    {
      get
      {
        return this.m_WorldPosition;
      }
    }

    public DistrictKind Kind
    {
      get
      {
        return this.m_Kind;
      }
    }

    public string Name
    {
      get
      {
        return this.m_Name;
      }
      set
      {
        this.m_Name = value;
      }
    }

    public IEnumerable<Map> Maps
    {
      get
      {
        return (IEnumerable<Map>) this.m_Maps;
      }
    }

    public int CountMaps
    {
      get
      {
        return this.m_Maps.Count;
      }
    }

    public Map EntryMap
    {
      get
      {
        return this.m_EntryMap;
      }
      set
      {
        if (this.m_EntryMap != null)
          this.RemoveMap(this.m_EntryMap);
        this.m_EntryMap = value;
        if (value == null)
          return;
        this.AddMap(value);
      }
    }

    public Map SewersMap
    {
      get
      {
        return this.m_SewersMap;
      }
      set
      {
        if (this.m_SewersMap != null)
          this.RemoveMap(this.m_SewersMap);
        this.m_SewersMap = value;
        if (value == null)
          return;
        this.AddMap(value);
      }
    }

    public Map SubwayMap
    {
      get
      {
        return this.m_SubwayMap;
      }
      set
      {
        if (this.m_SubwayMap != null)
          this.RemoveMap(this.m_SubwayMap);
        this.m_SubwayMap = value;
        if (value == null)
          return;
        this.AddMap(value);
      }
    }

    public bool HasSubway
    {
      get
      {
        return this.m_SubwayMap != null;
      }
    }

    public District(Point worldPos, DistrictKind kind)
    {
      this.m_WorldPosition = worldPos;
      this.m_Kind = kind;
    }

    protected void AddMap(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      if (this.m_Maps.Contains(map))
        return;
      map.District = this;
      this.m_Maps.Add(map);
    }

    public void AddUniqueMap(Map map)
    {
      this.AddMap(map);
    }

    public Map GetMap(int index)
    {
      return this.m_Maps[index];
    }

    protected void RemoveMap(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      this.m_Maps.Remove(map);
      map.District = (District) null;
    }

    public void OptimizeBeforeSaving()
    {
      this.m_Maps.TrimExcess();
      foreach (Map mMap in this.m_Maps)
        mMap.OptimizeBeforeSaving();
    }

    public override int GetHashCode()
    {
      return this.m_WorldPosition.GetHashCode();
    }
  }
}
