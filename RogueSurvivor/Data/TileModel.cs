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
    public int ID { get; set; }
    public string ImageID { get; private set; }
    public bool IsWalkable { get; private set; }
    public bool IsTransparent { get; private set; }
    public Color MinimapColor { get; private set; }
    public bool IsWater { get; set; }
    public string WaterCoverImageID { get; set; }

    public TileModel(string imageID, Color minimapColor, bool isWalkable, bool isTransparent)
    {
      ImageID = imageID;
      IsWalkable = isWalkable;
      IsTransparent = isTransparent;
      MinimapColor = minimapColor;
    }
  }
}
