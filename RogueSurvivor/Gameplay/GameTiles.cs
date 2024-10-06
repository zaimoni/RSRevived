// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameTiles
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Gameplay;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  public static class GameTiles
  {
    private static readonly Color DRK_GRAY1 = Color.DimGray;
    private static readonly Color DRK_GRAY2 = Color.DarkGray;
    private static readonly Color DRK_RED = Color.FromArgb(128, 0, 0);
    private static readonly Color LIT_GRAY1 = Color.Gray;
    private static readonly Color LIT_GRAY2 = Color.LightGray;
    private static readonly Color LIT_GRAY3 = Color.FromArgb(230, 230, 230);
    private static readonly Color LIT_BROWN = Color.BurlyWood;

    // There are no transparent walls at this time.
    // There are no opaque non-walls at this time.
    // According to the map class, a transparent wall permits ranged combat, and blocks both walking and throwing.
    // * use this to implement a gun port.  May need to be adjacent to the transparent wall before it's transparent.
    // According to the map class, an opaque non-wall blocks ranged combat, but permits both walking and throwing.
    // * use this to implement a smoke grenade, or thick smoke from a fire
    private static readonly TileModel[] m_Models = new TileModel[]{
      TileModel.UNDEF,
      new TileModel(IDs.FLOOR_ASPHALT, GameImages.TILE_FLOOR_ASPHALT, LIT_GRAY1, true, true),
      new TileModel(IDs.FLOOR_CONCRETE, GameImages.TILE_FLOOR_CONCRETE, LIT_GRAY2, true, true),
      new TileModel(IDs.FLOOR_GRASS, GameImages.TILE_FLOOR_GRASS, Color.Green, true, true),
      new TileModel(IDs.FLOOR_OFFICE, GameImages.TILE_FLOOR_OFFICE, LIT_GRAY3, true, true),
      new TileModel(IDs.FLOOR_PLANKS, GameImages.TILE_FLOOR_PLANKS, LIT_BROWN, true, true),
      new TileModel(IDs.FLOOR_SEWER_WATER, GameImages.TILE_FLOOR_SEWER_WATER, Color.Blue, true, true, GameImages.TILE_FLOOR_SEWER_WATER_COVER),
      new TileModel(IDs.FLOOR_TILES, GameImages.TILE_FLOOR_TILES, LIT_GRAY2, true, true),
      new TileModel(IDs.FLOOR_WALKWAY, GameImages.TILE_FLOOR_WALKWAY, LIT_GRAY2, true, true),
      new TileModel(IDs.ROAD_ASPHALT_EW, GameImages.TILE_ROAD_ASPHALT_EW, LIT_GRAY1, true, true),
      new TileModel(IDs.ROAD_ASPHALT_NS, GameImages.TILE_ROAD_ASPHALT_NS, LIT_GRAY1, true, true),
      new TileModel(IDs.RAIL_EW, GameImages.TILE_RAIL_EW, LIT_GRAY1, true, true),
      new TileModel(IDs.WALL_BRICK, GameImages.TILE_WALL_BRICK, DRK_GRAY1, false, false),
      new TileModel(IDs.WALL_CHAR_OFFICE, GameImages.TILE_WALL_CHAR_OFFICE, DRK_RED, false, false),
      new TileModel(IDs.WALL_HOSPITAL, GameImages.TILE_WALL_HOSPITAL, Color.White, false, false),
      new TileModel(IDs.WALL_POLICE_STATION, GameImages.TILE_WALL_STONE, Color.CadetBlue, false, false),
      new TileModel(IDs.WALL_SEWER, GameImages.TILE_WALL_SEWER, Color.DarkGreen, false, false),
      new TileModel(IDs.WALL_STONE, GameImages.TILE_WALL_STONE, DRK_GRAY1, false, false),
      new TileModel(IDs.WALL_SUBWAY, GameImages.TILE_WALL_STONE, Color.Blue, false, false),
      new TileModel(IDs.RAIL_NS, GameImages.TILE_RAIL_NS, LIT_GRAY1, true, true),
      new TileModel(IDs.RAIL_SWNE, GameImages.TILE_RAIL_SWNE, LIT_GRAY1, true, true),
      new TileModel(IDs.RAIL_SWNE_WALL_W, GameImages.TILE_RAIL_SWNE_WALL_W, Color.Blue, false, false),
      new TileModel(IDs.RAIL_SWNE_WALL_E, GameImages.TILE_RAIL_SWNE_WALL_E, Color.Blue, false, false),
      new TileModel(IDs.RAIL_SENW, GameImages.TILE_RAIL_SENW, LIT_GRAY1, true, true),
      new TileModel(IDs.RAIL_SENW_WALL_W, GameImages.TILE_RAIL_SENW_WALL_W, Color.Blue, false, false),
      new TileModel(IDs.RAIL_SENW_WALL_E, GameImages.TILE_RAIL_SENW_WALL_E, Color.Blue, false, false),
      new TileModel(IDs.ROAD_ASPHALT_SWNE, GameImages.TILE_ROAD_ASPHALT_SWNE, LIT_GRAY1, true, true),
      new TileModel(IDs.ROAD_ASPHALT_SENW, GameImages.TILE_ROAD_ASPHALT_SENW, LIT_GRAY1, true, true),
      new TileModel(IDs.ROAD_ASPHALT_SWNE_CONCRETE_E, GameImages.TILE_ROAD_ASPHALT_SWNE_CONCRETE_E, LIT_GRAY1, true, true),
      new TileModel(IDs.ROAD_ASPHALT_SWNE_CONCRETE_W, GameImages.TILE_ROAD_ASPHALT_SWNE_CONCRETE_W, LIT_GRAY1, true, true),
      new TileModel(IDs.ROAD_ASPHALT_SENW_CONCRETE_E, GameImages.TILE_ROAD_ASPHALT_SENW_CONCRETE_E, LIT_GRAY1, true, true),
      new TileModel(IDs.ROAD_ASPHALT_SENW_CONCRETE_W, GameImages.TILE_ROAD_ASPHALT_SENW_CONCRETE_W, LIT_GRAY1, true, true),
      new TileModel(IDs.FLOOR_GRASS_SWNE_CONCRETE_E, GameImages.TILE_FLOOR_GRASS_SWNE_CONCRETE_E, LIT_GRAY2, true, true),
      new TileModel(IDs.FLOOR_GRASS_SWNE_CONCRETE_W, GameImages.TILE_FLOOR_GRASS_SWNE_CONCRETE_W, LIT_GRAY2, true, true),
      new TileModel(IDs.FLOOR_GRASS_SENW_CONCRETE_E, GameImages.TILE_FLOOR_GRASS_SENW_CONCRETE_E, LIT_GRAY2, true, true),
      new TileModel(IDs.FLOOR_GRASS_SENW_CONCRETE_W, GameImages.TILE_FLOOR_GRASS_SENW_CONCRETE_W, LIT_GRAY2, true, true),
    };

    static public TileModel From(int id) { return m_Models[id]; }

#region static forwarders
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
    static public TileModel WALL_HOSPITAL { get { return m_Models[(int)IDs.WALL_HOSPITAL]; } }
    static public TileModel WALL_POLICE_STATION { get { return m_Models[(int)IDs.WALL_POLICE_STATION]; } }
    static public TileModel WALL_SEWER { get { return m_Models[(int)IDs.WALL_SEWER]; } }
    static public TileModel WALL_STONE { get { return m_Models[(int)IDs.WALL_STONE]; } }
    static public TileModel WALL_SUBWAY { get { return m_Models[(int)IDs.WALL_SUBWAY]; } }
    static public TileModel RAIL_NS { get { return m_Models[(int)IDs.RAIL_NS]; } }
    static public TileModel RAIL_SWNE { get { return m_Models[(int)IDs.RAIL_SWNE]; } }
    static public TileModel RAIL_SWNE_WALL_W { get { return m_Models[(int)IDs.RAIL_SWNE_WALL_W]; } }
    static public TileModel RAIL_SWNE_WALL_E { get { return m_Models[(int)IDs.RAIL_SWNE_WALL_E]; } }
    static public TileModel RAIL_SENW { get { return m_Models[(int)IDs.RAIL_SENW]; } }
    static public TileModel RAIL_SENW_WALL_W { get { return m_Models[(int)IDs.RAIL_SENW_WALL_W]; } }
    static public TileModel RAIL_SENW_WALL_E { get { return m_Models[(int)IDs.RAIL_SENW_WALL_E]; } }
    static public TileModel ROAD_ASPHALT_SWNE { get { return m_Models[(int)IDs.ROAD_ASPHALT_SWNE]; } }
    static public TileModel ROAD_ASPHALT_SENW { get { return m_Models[(int)IDs.ROAD_ASPHALT_SENW]; } }
    static public TileModel ROAD_ASPHALT_SWNE_CONCRETE_E { get { return m_Models[(int)IDs.ROAD_ASPHALT_SWNE_CONCRETE_E]; } }
    static public TileModel ROAD_ASPHALT_SWNE_CONCRETE_W { get { return m_Models[(int)IDs.ROAD_ASPHALT_SWNE_CONCRETE_W]; } }
    static public TileModel ROAD_ASPHALT_SENW_CONCRETE_E { get { return m_Models[(int)IDs.ROAD_ASPHALT_SENW_CONCRETE_E]; } }
    static public TileModel ROAD_ASPHALT_SENW_CONCRETE_W { get { return m_Models[(int)IDs.ROAD_ASPHALT_SENW_CONCRETE_W]; } }
    static public TileModel FLOOR_GRASS_SWNE_CONCRETE_E { get { return m_Models[(int)IDs.FLOOR_GRASS_SWNE_CONCRETE_E]; } }
    static public TileModel FLOOR_GRASS_SWNE_CONCRETE_W { get { return m_Models[(int)IDs.FLOOR_GRASS_SWNE_CONCRETE_W]; } }
    static public TileModel FLOOR_GRASS_SENW_CONCRETE_E { get { return m_Models[(int)IDs.FLOOR_GRASS_SENW_CONCRETE_E]; } }
    static public TileModel FLOOR_GRASS_SENW_CONCRETE_W { get { return m_Models[(int)IDs.FLOOR_GRASS_SENW_CONCRETE_W]; } }
#endregion

    static public bool IsRoadModel(TileModel model)
    {
      return ROAD_ASPHALT_EW==model || ROAD_ASPHALT_NS==model;
    }

    public enum IDs {
      UNDEF = 0,
      FLOOR_ASPHALT,
      FLOOR_CONCRETE,
      FLOOR_GRASS,
      FLOOR_OFFICE,
      FLOOR_PLANKS,
      FLOOR_SEWER_WATER,
      FLOOR_TILES,
      FLOOR_WALKWAY,
      ROAD_ASPHALT_EW,
      ROAD_ASPHALT_NS,
      RAIL_EW,
      WALL_BRICK,
      WALL_CHAR_OFFICE,
      WALL_HOSPITAL,
      WALL_POLICE_STATION,
      WALL_SEWER,
      WALL_STONE,
      WALL_SUBWAY,
      RAIL_NS,  // new RS Revived 0.10.0.0
      RAIL_SWNE,
      RAIL_SWNE_WALL_W,
      RAIL_SWNE_WALL_E,
      RAIL_SENW,
      RAIL_SENW_WALL_W,
      RAIL_SENW_WALL_E,
      ROAD_ASPHALT_SWNE,
      ROAD_ASPHALT_SENW,
      ROAD_ASPHALT_SWNE_CONCRETE_E,
      ROAD_ASPHALT_SWNE_CONCRETE_W,
      ROAD_ASPHALT_SENW_CONCRETE_E,
      ROAD_ASPHALT_SENW_CONCRETE_W,
      FLOOR_GRASS_SWNE_CONCRETE_E,
      FLOOR_GRASS_SWNE_CONCRETE_W,
      FLOOR_GRASS_SENW_CONCRETE_E,
      FLOOR_GRASS_SENW_CONCRETE_W,
      _COUNT
    }
  }
}
