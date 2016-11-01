// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Tile
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Tile
  {
    private int m_ModelID;
    private Tile.Flags m_Flags;
    private List<string> m_Decorations;

    public TileModel Model {
      get {
        Contract.Ensures(null!=Contract.Result<TileModel>());
        return Models.Tiles[m_ModelID];
      }
      set {
        Contract.Requires(null!=value);
        m_ModelID = value.ID;
      }
    }

    public bool IsInside {
      get {
        return (m_Flags & Tile.Flags.IS_INSIDE) != Tile.Flags.NONE;
      }
      set {
        if (value)
          m_Flags |= Tile.Flags.IS_INSIDE;
        else
          m_Flags &= ~Tile.Flags.IS_INSIDE;
      }
    }

    public bool IsInView
    {
      get
      {
        return (m_Flags & Tile.Flags.IS_IN_VIEW) != Tile.Flags.NONE;
      }
      set
      {
        if (value)
                    m_Flags |= Tile.Flags.IS_IN_VIEW;
        else
                    m_Flags &= ~Tile.Flags.IS_IN_VIEW;
      }
    }

    public bool IsVisited
    {
      get
      {
        return (m_Flags & Tile.Flags.IS_VISITED) != Tile.Flags.NONE;
      }
      set
      {
        if (value)
                    m_Flags |= Tile.Flags.IS_VISITED;
        else
                    m_Flags &= ~Tile.Flags.IS_VISITED;
      }
    }

    public bool HasDecorations
    {
      get
      {
        return m_Decorations != null;
      }
    }

    public IEnumerable<string> Decorations
    {
      get
      {
        return (IEnumerable<string>)m_Decorations;
      }
    }

    public Tile(TileModel model)
    {
      if (model == null)
        throw new ArgumentNullException("model");
            m_ModelID = model.ID;
    }

    public void AddDecoration(string imageID)
    {
      if (m_Decorations == null)
                m_Decorations = new List<string>(1);
      if (m_Decorations.Contains(imageID))
        return;
            m_Decorations.Add(imageID);
    }

    public bool HasDecoration(string imageID)
    {
      if (m_Decorations == null)
        return false;
      return m_Decorations.Contains(imageID);
    }

    public void RemoveAllDecorations()
    {
      if (m_Decorations != null)
                m_Decorations.Clear();
            m_Decorations = (List<string>) null;
    }

    public void RemoveDecoration(string imageID)
    {
      if (m_Decorations == null || !m_Decorations.Remove(imageID) || m_Decorations.Count != 0)
        return;
            m_Decorations = (List<string>) null;
    }

    public void OptimizeBeforeSaving()
    {
      if (m_Decorations == null)
        return;
            m_Decorations.TrimExcess();
    }

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
