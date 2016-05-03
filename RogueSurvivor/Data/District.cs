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
        return m_WorldPosition;
      }
    }

    public DistrictKind Kind
    {
      get
      {
        return m_Kind;
      }
    }

    public string Name
    {
      get
      {
        return m_Name;
      }
      set
      {
                m_Name = value;
      }
    }

    public IEnumerable<Map> Maps
    {
      get
      {
        return (IEnumerable<Map>)m_Maps;
      }
    }

    public int CountMaps
    {
      get
      {
        return m_Maps.Count;
      }
    }

    public Map EntryMap
    {
      get
      {
        return m_EntryMap;
      }
      set
      {
        if (m_EntryMap != null)
                    RemoveMap(m_EntryMap);
                m_EntryMap = value;
        if (value == null)
          return;
                AddMap(value);
      }
    }

    public Map SewersMap
    {
      get
      {
        return m_SewersMap;
      }
      set
      {
        if (m_SewersMap != null)
                    RemoveMap(m_SewersMap);
                m_SewersMap = value;
        if (value == null)
          return;
                AddMap(value);
      }
    }

    public Map SubwayMap
    {
      get
      {
        return m_SubwayMap;
      }
      set
      {
        if (m_SubwayMap != null)
                    RemoveMap(m_SubwayMap);
                m_SubwayMap = value;
        if (value == null)
          return;
                AddMap(value);
      }
    }

    public bool HasSubway
    {
      get
      {
        return m_SubwayMap != null;
      }
    }

    public District(Point worldPos, DistrictKind kind)
    {
            m_WorldPosition = worldPos;
            m_Kind = kind;
    }

    protected void AddMap(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      if (m_Maps.Contains(map))
        return;
      map.District = this;
            m_Maps.Add(map);
    }

    public void AddUniqueMap(Map map)
    {
            AddMap(map);
    }

    public Map GetMap(int index)
    {
      return m_Maps[index];
    }

    protected void RemoveMap(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
            m_Maps.Remove(map);
      map.District = (District) null;
    }

    public void OptimizeBeforeSaving()
    {
            m_Maps.TrimExcess();
      foreach (Map mMap in m_Maps)
        mMap.OptimizeBeforeSaving();
    }

    public override int GetHashCode()
    {
      return m_WorldPosition.GetHashCode();
    }
  }
}
