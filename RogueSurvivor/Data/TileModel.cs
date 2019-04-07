// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TileModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Color = System.Drawing.Color;

namespace djack.RogueSurvivor.Data
{
  internal class TileModel
  {
    public static readonly TileModel UNDEF = new TileModel(Gameplay.GameTiles.IDs.UNDEF, "", Color.Pink, false, true);

    public readonly int ID;
    public readonly string ImageID;
    public readonly bool IsWalkable;
    public readonly bool IsTransparent;
    public readonly Color MinimapColor;
    public readonly bool IsWater;
    public readonly string WaterCoverImageID;

    public TileModel(Gameplay.GameTiles.IDs id, string imageID, Color minimapColor, bool isWalkable, bool isTransparent, string waterCoverImageID=null)
    {
#if DEBUG
      if (null == imageID) throw new ArgumentNullException(nameof(imageID));  // the undef tile is empty-string imageID
#endif
      ID = (int)id;
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
