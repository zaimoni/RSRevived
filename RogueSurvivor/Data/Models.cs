// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Models
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#nullable enable

// 2020-02-15, historical design issue: the intent of this class appears to be to provide a game engine API
// that isolates game-specific models from outside code, *BUT* it is useful to enforce type-checking
// by allowing the array access operator [] to have an overload for the game-specific ID enumeration
namespace djack.RogueSurvivor.Data
{
  internal static class Models
  {
    readonly private static FactionDB m_Factions = new Gameplay.GameFactions();
    readonly private static TileModelDB m_Tiles = new Gameplay.GameTiles();

    // Actors and Items should also be static-contructed, but those require a UI parameter to report back on file load success/failure
    public static ActorModelDB Actors { get; set; }
    public static FactionDB Factions { get { return m_Factions; } }
    public static ItemModelDB Items { get; set; }
    public static TileModelDB Tiles { get { return m_Tiles; } }
  }
}
