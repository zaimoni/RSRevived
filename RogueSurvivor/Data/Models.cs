// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Models
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Data
{
  internal class Models
  {
    private static FactionDB m_Factions;
    private static TileModelDB m_Tiles;

    public static ActorModelDB Actors { get; set; }
    public static FactionDB Factions { get { return m_Factions; } }
    public static ItemModelDB Items { get; set; }
    public static TileModelDB Tiles { get { return m_Tiles; } }

    static Models()
    {
      m_Factions = new Gameplay.GameFactions();
      m_Tiles = new Gameplay.GameTiles();
    }
  }
}
