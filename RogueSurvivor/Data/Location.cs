// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Location
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  // This must *not* implement ISerializable.  Save-load puts the Prisoner Who Should Not Be at 0,0 rather than his intended location if the reasonable optimization
  // of saving/loading short rather than int is done.
  [Serializable]
  internal struct Location
  {
    private readonly Map m_Map;
    private Point m_Position;

    public Map Map { get { return m_Map; } }

    public Point Position { get { return m_Position; } }

    public Location(Map map, Point position)
    {
      Contract.Requires(null != map);
      m_Map = map;
      m_Position = position;
    }

    // == operator is useful
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
      return new Location(lhs.m_Map, lhs.m_Position+rhs);
    }

    // thin wrappers
    public MapObject MapObject { get { return m_Map.GetMapObjectAt(m_Position); } }
    public Actor Actor { get { return m_Map.GetActorAt(m_Position); } }
    public void AddCorpse(Corpse c) { m_Map.AddCorpseAt(c, m_Position); }
    public void PlaceActor(Actor actor) { m_Map.PlaceActorAt(actor, m_Position); }
    public bool IsWalkableFor(Actor actor) { return m_Map.IsWalkableFor(m_Position, actor); }
    public bool IsWalkableFor(Actor actor, out string reason) { return m_Map.IsWalkableFor(m_Position, actor, out reason); }
    public Inventory Items { get { return m_Map.GetItemsAt(m_Position); } }
    public Exit Exit { get { return m_Map.GetExitAt(m_Position); } }

    public override int GetHashCode()
    {
      return m_Map.GetHashCode() ^ m_Position.GetHashCode();
    }

    public override string ToString()
    {
      return m_Map.Name+" ("+m_Position.X.ToString()+","+m_Position.Y.ToString()+")";
    }
  }
}
