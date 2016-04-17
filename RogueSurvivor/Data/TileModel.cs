// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TileModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  internal class TileModel
  {
    public static readonly TileModel UNDEF = new TileModel("", Color.Pink, false, true);
    private int m_ID;
    private string m_ImageID;
    private bool m_IsWalkable;
    private bool m_IsTransparent;
    private Color m_MinimapColor;

    public int ID
    {
      get
      {
        return this.m_ID;
      }
      set
      {
        this.m_ID = value;
      }
    }

    public string ImageID
    {
      get
      {
        return this.m_ImageID;
      }
    }

    public bool IsWalkable
    {
      get
      {
        return this.m_IsWalkable;
      }
    }

    public bool IsTransparent
    {
      get
      {
        return this.m_IsTransparent;
      }
    }

    public Color MinimapColor
    {
      get
      {
        return this.m_MinimapColor;
      }
    }

    public bool IsWater { get; set; }

    public string WaterCoverImageID { get; set; }

    public TileModel(string imageID, Color minimapColor, bool IsWalkable, bool IsTransparent)
    {
      this.m_ImageID = imageID;
      this.m_IsWalkable = IsWalkable;
      this.m_IsTransparent = IsTransparent;
      this.m_MinimapColor = minimapColor;
    }
  }
}
