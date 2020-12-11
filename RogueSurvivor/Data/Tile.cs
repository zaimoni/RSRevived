// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Tile
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  internal class Tile
  {
    public readonly byte ID;
    private Flags m_Flags;
    private readonly Location m_Location;

    public TileModel Model { get { return Models.Tiles[ID]; } }

    public bool IsInside {
      get {
        return (m_Flags & Flags.IS_INSIDE) != Flags.NONE;
      }
      private set {
        if (value) m_Flags |= Flags.IS_INSIDE;
        else m_Flags &= ~Flags.IS_INSIDE;
      }
    }

    public bool IsInView {
      get {
        return (m_Flags & Flags.IS_IN_VIEW) != Flags.NONE;
      }
      set {
        if (value) m_Flags |= Flags.IS_IN_VIEW;
        else m_Flags &= ~Flags.IS_IN_VIEW;
      }
    }

    public bool IsVisited {
      get {
        return (m_Flags & Flags.IS_VISITED) != Flags.NONE;
      }
      set {
        if (value) m_Flags |= Flags.IS_VISITED;
        else m_Flags &= ~Flags.IS_VISITED;
      }
    }

    public bool HasDecorations { get { return m_Location.Map.HasDecorationsAt(m_Location.Position); } }
    public void DoForAllDecorations(Action<string> op) { m_Location.Map.DoForAllDecorationsAt(m_Location.Position, op); }

    public Tile(int modelID, bool inside, Location loc)
    {
#if DEBUG
      if (0>modelID || 255<modelID) throw new InvalidOperationException("0 > modelID || 255 < modelID");
#endif
      ID = (byte)modelID;
      IsInside = inside;
      m_Location = loc;
    }

    public void AddDecoration(string imageID) { m_Location.AddDecoration(imageID); }

#if DEAD_FUNC
    public bool HasDecoration(string imageID)
    {
      return m_Location.Map.HasDecorationAt(imageID,m_Location.Position);
    }
#endif

    public void RemoveAllDecorations()
    {
      m_Location.Map.RemoveAllDecorationsAt(m_Location.Position);
    }

#if DEAD_FUNC
    public void RemoveDecoration(string imageID)
    {
      m_Location.Map.RemoveDecorationAt(imageID,m_Location.Position);
    }
#endif

    [System.Flags]
    private enum Flags
    {
      NONE = 0,
      IS_INSIDE = 1,    // tile flag
      IS_IN_VIEW = 2,   // tile-player flag
      IS_VISITED = 4,   // tile-player flag
    }
  }
}
