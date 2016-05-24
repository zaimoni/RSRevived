// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Location
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal struct Location
  {
    private Map m_Map;
    private Point m_Position;

    public Map Map
    {
      get
      {
        return m_Map;
      }
    }

    public Point Position
    {
      get
      {
        return m_Position;
      }
    }

    public Location(Map map, Point position)
    {
      if (map == null)
        throw new ArgumentNullException("map");
            m_Map = map;
            m_Position = position;
    }

    public static bool operator ==(Location lhs, Location rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Location lhs, Location rhs)
    {
      return !lhs.Equals(rhs);
    }
    
    // silence compiler warnings
    public bool Equals(Location x)
    {
      return m_Map == x.m_Map && m_Position == x.m_Position;
    }

    public override bool Equals(object obj)
    {
      Location? tmp = obj as Location?;
      if (null == tmp) return false;
      return Equals(tmp.Value);
    }

    public static Location operator +(Location lhs, Direction rhs)
    {
      return new Location(lhs.m_Map, new Point(lhs.m_Position.X + rhs.Vector.X, lhs.m_Position.Y + rhs.Vector.Y));
    }

    public override int GetHashCode()
    {
      return m_Map.GetHashCode() ^ m_Position.GetHashCode();
    }

    public override string ToString()
    {
      throw new NotImplementedException();
    }
  }
}
