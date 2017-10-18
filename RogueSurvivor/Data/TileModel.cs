// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TileModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  internal class TileModel
  {
    public static readonly TileModel UNDEF = new TileModel("", Color.Pink, false, true);

    private int m_ID;

    public int ID {
      get { return m_ID; }
      set {
#if DEBUG
        if (-1 != m_ID) throw new InvalidOperationException("can only assign tile id once");
        if (0 > value) throw new ArgumentOutOfRangeException("0 > "+nameof(value));
#endif
        m_ID = value;
      }
    }
    public readonly string ImageID;
    public readonly bool IsWalkable;
    public readonly bool IsTransparent;
    public readonly Color MinimapColor;
    public readonly bool IsWater;
    public readonly string WaterCoverImageID;

    public TileModel(string imageID, Color minimapColor, bool isWalkable, bool isTransparent, string waterCoverImageID=null)
    {
#if DEBUG
      if (null == imageID) throw new ArgumentNullException(nameof(imageID));  // the undef tile is empty-string imageID
#endif
      m_ID = -1;
      ImageID = imageID;
      IsWalkable = isWalkable;
      IsTransparent = isTransparent;
      MinimapColor = minimapColor;
      if (!string.IsNullOrEmpty(waterCoverImageID)) {
        IsWater = true;
        WaterCoverImageID  = waterCoverImageID;
      }
    }
  }
}
