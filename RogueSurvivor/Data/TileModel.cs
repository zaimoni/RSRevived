// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TileModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using Color = System.Drawing.Color;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  public sealed class TileModel
  {
    public static readonly TileModel UNDEF = new TileModel(GameTiles.IDs.UNDEF, "", Color.Pink, false, true);

    public readonly int ID;
    public readonly string ImageID;
    public readonly bool IsWalkable;
    public readonly bool IsTransparent;
    public readonly Color MinimapColor;
    public readonly bool IsWater;   // 2020-05-09 should be ok to pay RAM for CPU here
    public readonly string? WaterCoverImageID;

    public TileModel(GameTiles.IDs id, string imageID, Color minimapColor, bool isWalkable, bool isTransparent, string? waterCoverImageID=null)
    { // the undef tile is empty-string imageID
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
