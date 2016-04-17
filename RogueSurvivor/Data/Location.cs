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
        return this.m_Map;
      }
    }

    public Point Position
    {
      get
      {
        return this.m_Position;
      }
    }

    public Location(Map map, Point position)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      this.m_Map = map;
      this.m_Position = position;
    }

    public static bool operator ==(Location lhs, Location rhs)
    {
      if (lhs.m_Map == rhs.m_Map)
        return lhs.m_Position == rhs.m_Position;
      return false;
    }

    public static bool operator !=(Location lhs, Location rhs)
    {
      return !(lhs == rhs);
    }

    public static Location operator +(Location lhs, Direction rhs)
    {
      return new Location(lhs.m_Map, new Point(lhs.m_Position.X + rhs.Vector.X, lhs.m_Position.Y + rhs.Vector.Y));
    }

    public override int GetHashCode()
    {
      return this.m_Map.GetHashCode() ^ this.m_Position.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj == null || !(obj is Location))
        return false;
      return this == (Location) obj;
    }

    public override string ToString()
    {
      throw new NotImplementedException();
    }
  }
}
