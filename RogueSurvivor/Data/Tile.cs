// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Tile
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Tile
  {
    private int m_ModelID;
    private Tile.Flags m_Flags;
    private List<string> m_Decorations;

    public TileModel Model
    {
      get
      {
        return Models.Tiles[this.m_ModelID];
      }
      set
      {
        this.m_ModelID = value.ID;
      }
    }

    public bool IsInside
    {
      get
      {
        return (this.m_Flags & Tile.Flags.IS_INSIDE) != Tile.Flags.NONE;
      }
      set
      {
        if (value)
          this.m_Flags |= Tile.Flags.IS_INSIDE;
        else
          this.m_Flags &= ~Tile.Flags.IS_INSIDE;
      }
    }

    public bool IsInView
    {
      get
      {
        return (this.m_Flags & Tile.Flags.IS_IN_VIEW) != Tile.Flags.NONE;
      }
      set
      {
        if (value)
          this.m_Flags |= Tile.Flags.IS_IN_VIEW;
        else
          this.m_Flags &= ~Tile.Flags.IS_IN_VIEW;
      }
    }

    public bool IsVisited
    {
      get
      {
        return (this.m_Flags & Tile.Flags.IS_VISITED) != Tile.Flags.NONE;
      }
      set
      {
        if (value)
          this.m_Flags |= Tile.Flags.IS_VISITED;
        else
          this.m_Flags &= ~Tile.Flags.IS_VISITED;
      }
    }

    public bool HasDecorations
    {
      get
      {
        return this.m_Decorations != null;
      }
    }

    public IEnumerable<string> Decorations
    {
      get
      {
        return (IEnumerable<string>) this.m_Decorations;
      }
    }

    public Tile(TileModel model)
    {
      if (model == null)
        throw new ArgumentNullException("model");
      this.m_ModelID = model.ID;
    }

    public void AddDecoration(string imageID)
    {
      if (this.m_Decorations == null)
        this.m_Decorations = new List<string>(1);
      if (this.m_Decorations.Contains(imageID))
        return;
      this.m_Decorations.Add(imageID);
    }

    public bool HasDecoration(string imageID)
    {
      if (this.m_Decorations == null)
        return false;
      return this.m_Decorations.Contains(imageID);
    }

    public void RemoveAllDecorations()
    {
      if (this.m_Decorations != null)
        this.m_Decorations.Clear();
      this.m_Decorations = (List<string>) null;
    }

    public void RemoveDecoration(string imageID)
    {
      if (this.m_Decorations == null || !this.m_Decorations.Remove(imageID) || this.m_Decorations.Count != 0)
        return;
      this.m_Decorations = (List<string>) null;
    }

    public void OptimizeBeforeSaving()
    {
      if (this.m_Decorations == null)
        return;
      this.m_Decorations.TrimExcess();
    }

    [System.Flags]
    private enum Flags
    {
      NONE = 0,
      IS_INSIDE = 1,
      IS_IN_VIEW = 2,
      IS_VISITED = 4,
    }
  }
}
