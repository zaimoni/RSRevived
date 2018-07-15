// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameTiles
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameTiles : TileModelDB
  {
    private static readonly Color DRK_GRAY1 = Color.DimGray;
    private static readonly Color DRK_GRAY2 = Color.DarkGray;
    private static readonly Color DRK_RED = Color.FromArgb(128, 0, 0);
    private static readonly Color LIT_GRAY1 = Color.Gray;
    private static readonly Color LIT_GRAY2 = Color.LightGray;
    private static readonly Color LIT_GRAY3 = Color.FromArgb(230, 230, 230);
    private static readonly Color LIT_BROWN = Color.BurlyWood;
    private static readonly TileModel[] m_Models = new TileModel[(int) IDs._COUNT];

    public TileModel this[int id] {
      get {
        return m_Models[id];
      }
    }

    static public TileModel FLOOR_ASPHALT { get { return m_Models[(int)IDs.FLOOR_ASPHALT]; } }
    static public TileModel FLOOR_CONCRETE { get { return m_Models[(int)IDs.FLOOR_CONCRETE]; } }
    static public TileModel FLOOR_GRASS { get { return m_Models[(int)IDs.FLOOR_GRASS]; } }
    static public TileModel FLOOR_OFFICE { get { return m_Models[(int)IDs.FLOOR_OFFICE]; } }
    static public TileModel FLOOR_PLANKS { get { return m_Models[(int)IDs.FLOOR_PLANKS]; } }
    static public TileModel FLOOR_SEWER_WATER { get { return m_Models[(int)IDs.FLOOR_SEWER_WATER]; } }
    static public TileModel FLOOR_TILES { get { return m_Models[(int)IDs.FLOOR_TILES]; } }
    static public TileModel FLOOR_WALKWAY { get { return m_Models[(int)IDs.FLOOR_WALKWAY]; } }
    static public TileModel ROAD_ASPHALT_EW { get { return m_Models[(int)IDs.ROAD_ASPHALT_EW]; } }
    static public TileModel ROAD_ASPHALT_NS { get { return m_Models[(int)IDs.ROAD_ASPHALT_NS]; } }
    static public TileModel RAIL_EW { get { return m_Models[(int)IDs.RAIL_EW]; } }
    static public TileModel WALL_BRICK { get { return m_Models[(int)IDs.WALL_BRICK]; } }
    static public TileModel WALL_CHAR_OFFICE { get { return m_Models[(int)IDs.WALL_CHAR_OFFICE]; } }
    static public TileModel WALL_POLICE_STATION { get { return m_Models[(int)IDs.WALL_POLICE_STATION]; } }
    static public TileModel WALL_HOSPITAL { get { return m_Models[(int)IDs.WALL_HOSPITAL]; } }
    static public TileModel WALL_SEWER { get { return m_Models[(int)IDs.WALL_SEWER]; } }
    static public TileModel WALL_STONE { get { return m_Models[(int)IDs.WALL_STONE]; } }
    static public TileModel WALL_SUBWAY { get { return m_Models[(int)IDs.WALL_SUBWAY]; } }

    // There are no transparent walls at this time.
    // There are no opaque non-walls at this time.
    // According to the map class, a transparent wall permits ranged combat, and blocks both walking and throwing.
    // * use this to implement a gun port
    // According to the map class, an opaque non-wall blocks ranged combat, but permits both walking and throwing.
    // * use this to implement a smoke grenade, or thick smoke from a fire
    static GameTiles()
    {
      m_Models[(int)IDs.UNDEF] = TileModel.UNDEF;
      m_Models[(int)IDs.FLOOR_ASPHALT] = new TileModel(GameImages.TILE_FLOOR_ASPHALT, GameTiles.LIT_GRAY1, true, true) { ID = (int)IDs.FLOOR_ASPHALT };
      m_Models[(int)IDs.FLOOR_CONCRETE] = new TileModel(GameImages.TILE_FLOOR_CONCRETE, GameTiles.LIT_GRAY2, true, true) { ID = (int)IDs.FLOOR_CONCRETE };
      m_Models[(int)IDs.FLOOR_GRASS] = new TileModel(GameImages.TILE_FLOOR_GRASS, Color.Green, true, true) { ID = (int)IDs.FLOOR_GRASS };
      m_Models[(int)IDs.FLOOR_OFFICE] = new TileModel(GameImages.TILE_FLOOR_OFFICE, GameTiles.LIT_GRAY3, true, true) { ID = (int)IDs.FLOOR_OFFICE };
      m_Models[(int)IDs.FLOOR_PLANKS] = new TileModel(GameImages.TILE_FLOOR_PLANKS, GameTiles.LIT_BROWN, true, true) { ID = (int)IDs.FLOOR_PLANKS };
      m_Models[(int)IDs.FLOOR_SEWER_WATER] = new TileModel(GameImages.TILE_FLOOR_SEWER_WATER, Color.Blue, true, true, GameImages.TILE_FLOOR_SEWER_WATER_COVER) { ID = (int)IDs.FLOOR_SEWER_WATER };
      m_Models[(int)IDs.FLOOR_TILES] = new TileModel(GameImages.TILE_FLOOR_TILES, GameTiles.LIT_GRAY2, true, true) { ID = (int)IDs.FLOOR_TILES };
      m_Models[(int)IDs.FLOOR_WALKWAY] = new TileModel(GameImages.TILE_FLOOR_WALKWAY, GameTiles.LIT_GRAY2, true, true) { ID = (int)IDs.FLOOR_WALKWAY };
      m_Models[(int)IDs.ROAD_ASPHALT_EW] = new TileModel(GameImages.TILE_ROAD_ASPHALT_EW, GameTiles.LIT_GRAY1, true, true) { ID = (int)IDs.ROAD_ASPHALT_EW };
      m_Models[(int)IDs.ROAD_ASPHALT_NS] = new TileModel(GameImages.TILE_ROAD_ASPHALT_NS, GameTiles.LIT_GRAY1, true, true) { ID = (int)IDs.ROAD_ASPHALT_NS };
      m_Models[(int)IDs.RAIL_EW] = new TileModel(GameImages.TILE_RAIL_EW, GameTiles.LIT_GRAY1, true, true) { ID = (int)IDs.RAIL_EW };
      m_Models[(int)IDs.WALL_BRICK] = new TileModel(GameImages.TILE_WALL_BRICK, GameTiles.DRK_GRAY1, false, false) { ID = (int)IDs.WALL_BRICK };
      m_Models[(int)IDs.WALL_CHAR_OFFICE] = new TileModel(GameImages.TILE_WALL_CHAR_OFFICE, GameTiles.DRK_RED, false, false) { ID = (int)IDs.WALL_CHAR_OFFICE };
      m_Models[(int)IDs.WALL_HOSPITAL] = new TileModel(GameImages.TILE_WALL_HOSPITAL, Color.White, false, false) { ID = (int)IDs.WALL_HOSPITAL };
      m_Models[(int)IDs.WALL_POLICE_STATION] = new TileModel(GameImages.TILE_WALL_STONE, Color.CadetBlue, false, false) { ID = (int)IDs.WALL_POLICE_STATION };
      m_Models[(int)IDs.WALL_SEWER] = new TileModel(GameImages.TILE_WALL_SEWER, Color.DarkGreen, false, false) { ID = (int)IDs.WALL_SEWER };
      m_Models[(int)IDs.WALL_STONE] = new TileModel(GameImages.TILE_WALL_STONE, GameTiles.DRK_GRAY1, false, false) { ID = (int)IDs.WALL_STONE };
      m_Models[(int)IDs.WALL_SUBWAY] = new TileModel(GameImages.TILE_WALL_STONE, Color.Blue, false, false) { ID = (int)IDs.WALL_SUBWAY };
      m_Models[(int)IDs.RAIL_NS] = new TileModel(GameImages.TILE_RAIL_NS, GameTiles.LIT_GRAY1, true, true) { ID = (int)IDs.RAIL_NS };
    }

    public GameTiles()
    {
    }

    static public bool IsRoadModel(TileModel model)
    {
      return ROAD_ASPHALT_EW==model || ROAD_ASPHALT_NS==model;
    }

    public enum IDs
    {
      UNDEF = 0,
      FLOOR_ASPHALT = 1,
      FLOOR_CONCRETE = 2,
      FLOOR_GRASS = 3,
      FLOOR_OFFICE = 4,
      FLOOR_PLANKS = 5,
      FLOOR_SEWER_WATER = 6,
      FLOOR_TILES = 7,
      FLOOR_WALKWAY = 8,
      ROAD_ASPHALT_EW = 9,
      ROAD_ASPHALT_NS = 10,
      RAIL_EW = 11,
      WALL_BRICK = 12,
      WALL_CHAR_OFFICE = 13,
      WALL_HOSPITAL = 14,
      WALL_POLICE_STATION = 15,
      WALL_SEWER = 16,
      WALL_STONE = 17,
      WALL_SUBWAY = 18,
      RAIL_NS,  // new RS Revived 0.10.0.0
      _COUNT,
    }
  }
}
