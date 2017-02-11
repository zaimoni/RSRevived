// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TileModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  internal class TileModel
  {
    public static readonly TileModel UNDEF = new TileModel("", Color.Pink, false, true);
    
    private int m_ID;

    public int ID {
      get { return m_ID; }
      set {
        Contract.Requires(-1 == ID);
        Contract.Requires(0 <= value);
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
