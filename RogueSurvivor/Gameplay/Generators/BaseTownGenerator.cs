// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.BaseTownGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Gameplay.Generators
{
  internal class BaseTownGenerator : BaseMapGenerator
  {
    public static readonly BaseTownGenerator.Parameters DEFAULT_PARAMS = new BaseTownGenerator.Parameters {
      MapWidth = RogueGame.MAP_MAX_WIDTH,
      MapHeight = RogueGame.MAP_MAX_HEIGHT,
      MinBlockSize = 11,
      WreckedCarChance = 10,
      ShopBuildingChance = 10,
      ParkBuildingChance = 10,
      CHARBuildingChance = 10,
      PostersChance = 2,
      TagsChance = 2,
      ItemInShopShelfChance = 100,
      PolicemanChance = 15
    };
    private static readonly string[] CHAR_POSTERS = new string[3]
    {
      GameImages.DECO_CHAR_POSTER1,
      GameImages.DECO_CHAR_POSTER2,
      GameImages.DECO_CHAR_POSTER3
    };
    private static readonly string[] POSTERS = new string[2]
    {
      GameImages.DECO_POSTERS1,
      GameImages.DECO_POSTERS2
    };
    private static readonly string[] TAGS = new string[7]
    {
      GameImages.DECO_TAGS1,
      GameImages.DECO_TAGS2,
      GameImages.DECO_TAGS3,
      GameImages.DECO_TAGS4,
      GameImages.DECO_TAGS5,
      GameImages.DECO_TAGS6,
      GameImages.DECO_TAGS7
    };

    // not going full read-only types on these two for now (relying on access control instead)
    private readonly GameItems.IDs[] bedroom_ranged_candidates;
    private readonly KeyValuePair<GameItems.IDs,GameItems.IDs>[] survivalist_ranged_candidates;

    private Parameters m_Params = BaseTownGenerator.DEFAULT_PARAMS;
    private readonly string[] m_PC_names;
    private const int PARK_TREE_CHANCE = 25;
    private const int PARK_BENCH_CHANCE = 5;
    private const int PARK_ITEM_CHANCE = 5;
    private const int MAX_CHAR_GUARDS_PER_OFFICE = 3;
    private const int SEWERS_ITEM_CHANCE = 1;
    private const int SEWERS_JUNK_CHANCE = 10;
    private const int SEWERS_TAG_CHANCE = 10;
    private const int SEWERS_IRON_FENCE_PER_BLOCK_CHANCE = 50;
    private const int SEWERS_ROOM_CHANCE = 20;
    private const int SUBWAY_TAGS_POSTERS_CHANCE = 20;
    private const int HOUSE_LIVINGROOM_ITEMS_ON_TABLE = 2;
    private const int HOUSE_KITCHEN_ITEMS_ON_TABLE = 2;
    private const int HOUSE_KITCHEN_ITEMS_IN_FRIDGE = 3;
    private const int HOUSE_BASEMENT_CHANCE = 30;
    private const int HOUSE_BASEMENT_OBJECT_CHANCE_PER_TILE = 10;
    private const int HOUSE_BASEMENT_PILAR_CHANCE = 20;
    private const int HOUSE_BASEMENT_WEAPONS_CACHE_CHANCE = 20;
    private const int HOUSE_BASEMENT_ZOMBIE_RAT_CHANCE = 5;
    private const int SHOP_BASEMENT_CHANCE = 30;
    private const int SHOP_BASEMENT_SHELF_CHANCE_PER_TILE = 5;
    private const int SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF = 33;
    private const int SHOP_WINDOW_CHANCE = 30;
    private const int SHOP_BASEMENT_ZOMBIE_RAT_CHANCE = 5;
    private List<Block> m_SurfaceBlocks;

    public Parameters Params
    {
      get {
        return m_Params;
      }
      set { // required by District::GenerateEntryMap
        m_Params = value;
      }
    }

    public BaseTownGenerator(RogueGame game, Parameters parameters)
      : base(game)
    {
      m_Params = parameters;
      if (Engine.Session.CommandLineOptions.ContainsKey("PC")) m_PC_names = Engine.Session.CommandLineOptions["PC"].Split('\0');

      // hook for planned pre-apocalypse politics
      // following is RED STATE, RED CITY
      List<GameItems.IDs> working_bedroom = new List<GameItems.IDs> {
        // XXX these three require firearms to be legal for civilians to generate in bedrooms
        GameItems.IDs.RANGED_HUNTING_CROSSBOW,
        GameItems.IDs.RANGED_HUNTING_RIFLE,
        GameItems.IDs.RANGED_SHOTGUN,
        // XXX these two require concealed carry to be legal to generate in bedrooms
        GameItems.IDs.RANGED_PISTOL,
        GameItems.IDs.RANGED_KOLT_REVOLVER
      };
      bedroom_ranged_candidates = working_bedroom.ToArray();

      // any not-so-legal ranged weapons were obtained the same way the grenades were.  (U.S.: could be obtained via connections with a grade C license
      // holder [registered private army] under the 1934 automatic weapons ban.)
      // true military weapons not represented here, the ammo is assumed too hard to get pre-apocalypse
      // no duplication of ammo between primary and secondary ranged weapon
      List<KeyValuePair<GameItems.IDs, GameItems.IDs>> working_survivalist = new List<KeyValuePair<GameItems.IDs, GameItems.IDs>> {
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_CROSSBOW, GameItems.IDs.RANGED_HUNTING_RIFLE),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_CROSSBOW, GameItems.IDs.RANGED_SHOTGUN),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_CROSSBOW, GameItems.IDs.RANGED_PISTOL),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_CROSSBOW, GameItems.IDs.RANGED_KOLT_REVOLVER),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_RIFLE, GameItems.IDs.RANGED_HUNTING_CROSSBOW),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_RIFLE, GameItems.IDs.RANGED_SHOTGUN),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_RIFLE, GameItems.IDs.RANGED_PISTOL),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_HUNTING_RIFLE, GameItems.IDs.RANGED_KOLT_REVOLVER),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_SHOTGUN, GameItems.IDs.RANGED_HUNTING_CROSSBOW),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_SHOTGUN, GameItems.IDs.RANGED_HUNTING_RIFLE),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_SHOTGUN, GameItems.IDs.RANGED_PISTOL),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_SHOTGUN, GameItems.IDs.RANGED_KOLT_REVOLVER),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_PISTOL, GameItems.IDs.RANGED_HUNTING_CROSSBOW),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_PISTOL, GameItems.IDs.RANGED_HUNTING_RIFLE),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_PISTOL, GameItems.IDs.RANGED_SHOTGUN),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_KOLT_REVOLVER, GameItems.IDs.RANGED_HUNTING_CROSSBOW),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_KOLT_REVOLVER, GameItems.IDs.RANGED_HUNTING_RIFLE),
        new KeyValuePair<GameItems.IDs,GameItems.IDs>(GameItems.IDs.RANGED_KOLT_REVOLVER, GameItems.IDs.RANGED_SHOTGUN),
      };
      survivalist_ranged_candidates = working_survivalist.ToArray();
    }

    public override Map Generate(int seed, string name)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map map = new Map(seed, name, m_Params.District, m_Params.MapWidth, m_Params.MapHeight);

      TileFill(map, GameTiles.FLOOR_GRASS);
      List<Block> list = new List<Block>();
      Rectangle rect = new Rectangle(0, 0, map.Width, map.Height);
      MakeBlocks(map, true, ref list, rect);
      List<Block> blockList1 = new List<Block>(list);
      List<Block> blockList2 = new List<Block>(blockList1.Count);
      m_SurfaceBlocks = new List<Block>(list.Count);
      foreach (Block copyFrom in list)
        m_SurfaceBlocks.Add(new Block(copyFrom));
      if (m_Params.GeneratePoliceStation) {
        MakePoliceStation(map, list, out Block policeBlock);
        blockList1.Remove(policeBlock);
      }
      if (m_Params.GenerateHospital) {
        MakeHospital(map, list, out Block hospitalBlock);
        blockList1.Remove(hospitalBlock);
      }
      blockList2.Clear();
      foreach (Block b in blockList1) {
        if (m_DiceRoller.RollChance(m_Params.ShopBuildingChance) && MakeShopBuilding(map, b))
          blockList2.Add(b);
      }
      foreach (Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      int num = 0;
      foreach (Block b in blockList1) {
        if ((m_Params.District.Kind == DistrictKind.BUSINESS && num == 0) || m_DiceRoller.RollChance(m_Params.CHARBuildingChance)) {
          CHARBuildingType charBuildingType = MakeCHARBuilding(map, b);
          if (charBuildingType == CHARBuildingType.OFFICE) ++num;
          if (charBuildingType != CHARBuildingType.NONE) blockList2.Add(b);
        }
      }
      foreach (Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      foreach (Block b in blockList1) {
        if (m_DiceRoller.RollChance(m_Params.ParkBuildingChance) && MakeParkBuilding(map, b))
          blockList2.Add(b);
      }
      foreach (Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      foreach (Block b in blockList1) {
        MakeHousingBuilding(map, b);
        blockList2.Add(b);
      }
      foreach (Block block in blockList2)
        blockList1.Remove(block);
      AddWreckedCarsOutside(map, rect);
      DecorateOutsideWallsWithPosters(map, rect, m_Params.PostersChance);
      DecorateOutsideWallsWithTags(map, rect, m_Params.TagsChance);
      return map;
    }

    public virtual Map GenerateSewersMap(int seed, District district)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map sewers = new Map(seed, string.Format("Sewers@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y), district, district.EntryMap.Width, district.EntryMap.Height, Lighting.DARKNESS);
      sewers.AddZone(MakeUniqueZone("sewers", sewers.Rect));
      TileFill(sewers, GameTiles.WALL_SEWER, true);

      ///////////////////////////////////////////////////
      // 1. Make blocks.
      // 2. Make tunnels.
      // 3. Link with surface.
      // 4. Additional jobs.
      // 5. Sewers Maintenance Room & Building(surface).
      // 6. Some rooms.
      // 7. Objects.
      // 8. Items.
      // 9. Tags.
      ///////////////////////////////////////////////////

      Map surface = district.EntryMap;

      // 1. Make blocks.
      List<Block> list = new List<Block>(m_SurfaceBlocks.Count);
      MakeBlocks(sewers, false, ref list, new Rectangle(0, 0, sewers.Width, sewers.Height));

#region 2. Make tunnels.
      foreach (Block block in list)
        TileRectangle(sewers, GameTiles.FLOOR_SEWER_WATER, block.Rectangle);
      foreach (Block block in list) {
        if (!m_DiceRoller.RollChance(SEWERS_IRON_FENCE_PER_BLOCK_CHANCE)) continue;
        bool flag = false;
        int x1;
        int y1;
        int x2;
        int y2;
        do {
          int num = m_DiceRoller.Roll(0, 4);
          switch (num) {
            case 0:
            case 1:
              x1 = m_DiceRoller.Roll(block.Rectangle.Left, block.Rectangle.Right - 1);
              y1 = num == 0 ? block.Rectangle.Top : block.Rectangle.Bottom - 1;
              x2 = x1;
              y2 = num == 0 ? y1 - 1 : y1 + 1;
              break;
            case 2:
            case 3:
              x1 = num == 2 ? block.Rectangle.Left : block.Rectangle.Right - 1;
              y1 = m_DiceRoller.Roll(block.Rectangle.Top, block.Rectangle.Bottom - 1);
              x2 = num == 2 ? x1 - 1 : x1 + 1;
              y2 = y1;
              break;
            default:
              throw new ArgumentOutOfRangeException("unhandled roll");
          }
          if (!sewers.IsOnMapBorder(x1, y1) && !sewers.IsOnMapBorder(x2, y2) && (CountAdjWalls(sewers, x1, y1) == 3 && CountAdjWalls(sewers, x2, y2) == 3))
            flag = true;
        }
        while (!flag);
        MapObjectPlace(sewers, x1, y1, MakeObjIronFence());
        MapObjectPlace(sewers, x2, y2, MakeObjIronFence());
      }
#endregion

#region 3. Link with surface.
      int countLinks = 0;
      do {
        for (int x = 0; x < sewers.Width; ++x) {
          for (int y = 0; y < sewers.Height; ++y) {
            if (m_DiceRoller.RollChance(3) && sewers.GetTileModelAt(x, y).IsWalkable) {
              Tile tileAt = surface.GetTileAt(x, y);
              if (tileAt.Model.IsWalkable && !sewers.HasMapObjectAt(x, y) && !tileAt.IsInside && ((tileAt.Model == GameTiles.FLOOR_WALKWAY || tileAt.Model == GameTiles.FLOOR_GRASS) && !surface.HasMapObjectAt(x, y)))
              {
                Point point = new Point(x, y);
                if (!sewers.HasAnyAdjacentInMap(point, (Predicate<Point>) (p => sewers.HasExitAt(p))) && !surface.HasAnyAdjacentInMap(point, (Predicate<Point>) (p => surface.HasExitAt(p))))
                {
                  AddExit(sewers, point, surface, point, GameImages.DECO_SEWER_LADDER, true);
                  AddExit(surface, point, sewers, point, GameImages.DECO_SEWER_HOLE, true);
                  ++countLinks;
                }
              }
            }
          }
        }
      }
      while (countLinks < 1);
#endregion

#region 5. Sewers Maintenance Room & Building(surface).
      List<Block> blockList = null;
      foreach (Block mSurfaceBlock in m_SurfaceBlocks) {
        if (    mSurfaceBlock.BuildingRect.Width <= m_Params.MinBlockSize + 2
            &&  mSurfaceBlock.BuildingRect.Height <= m_Params.MinBlockSize + 2 
            && !IsThereASpecialBuilding(surface, mSurfaceBlock.InsideRect)
            && !mSurfaceBlock.Rectangle.Any(pt => sewers.GetTileModelAt(pt).IsWalkable)) {
          (blockList ?? (blockList = new List<Block>(m_SurfaceBlocks.Count))).Add(mSurfaceBlock);
        }
      }
      Rectangle buildingRect;
      if (blockList != null) {
        Block block = m_DiceRoller.Choose(blockList);
        ClearRectangle(surface, block.BuildingRect);
        TileFill(surface, GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        m_SurfaceBlocks.Remove(block);
        Block b1 = new Block(block.Rectangle);
        buildingRect = b1.BuildingRect;
        int x = buildingRect.Left + buildingRect.Width / 2;
        int y = buildingRect.Top + buildingRect.Height / 2;
        Point exitPosition = new Point(x, y);
        MakeSewersMaintenanceBuilding(surface, true, b1, sewers, exitPosition);
        Block b2 = new Block(block.Rectangle);
        MakeSewersMaintenanceBuilding(sewers, false, b2, surface, exitPosition);
      }
#endregion

#region 6. Some rooms.
      foreach (Block block in list)
      {
        if (m_DiceRoller.RollChance(SEWERS_ROOM_CHANCE) && CheckForEachTile(block.BuildingRect, (Predicate<Point>) (pt => !sewers.GetTileModelAt(pt).IsWalkable)))
        {
          TileFill(sewers, GameTiles.FLOOR_CONCRETE, block.InsideRect);
          buildingRect = block.BuildingRect;
          int x1 = buildingRect.Left + buildingRect.Width / 2;
          sewers.SetTileModelAt(x1, buildingRect.Top, GameTiles.FLOOR_CONCRETE);
          int x2 = buildingRect.Left + buildingRect.Width / 2;
          int y1 = buildingRect.Bottom - 1;
          sewers.SetTileModelAt(x2, y1, GameTiles.FLOOR_CONCRETE);
          int y2 = buildingRect.Top + buildingRect.Height / 2;
          sewers.SetTileModelAt(buildingRect.Left, y2, GameTiles.FLOOR_CONCRETE);
          int x3 = buildingRect.Right - 1;
          int y3 = buildingRect.Top + buildingRect.Height / 2;
          sewers.SetTileModelAt(x3, y3, GameTiles.FLOOR_CONCRETE);
          sewers.AddZone(MakeUniqueZone("room", block.InsideRect));
        }
      }
#endregion

#region 7. Objects.
      MapObjectFill(sewers, new Rectangle(0, 0, sewers.Width, sewers.Height), (Func<Point, MapObject>) (pt =>
      {
        if (!m_DiceRoller.RollChance(SEWERS_JUNK_CHANCE)) return null;
        if (!sewers.IsWalkable(pt.X, pt.Y)) return null;
        return MakeObjJunk();
      }));
#endregion

#region 8. Items.
      Func<Item> sewers_stock = () => {
        switch (m_DiceRoller.Roll(0, 3)) {
          case 0: return MakeItemBigFlashlight();
          case 1: return  MakeItemCrowbar();
#if DEBUG
          case 2:
#else
          default:
#endif
            return MakeItemSprayPaint();
#if DEBUG
          default: throw new ArgumentOutOfRangeException("unhandled roll");
#endif
        }
      };
      sewers.Rect.DoForEach(pt => {
        sewers.DropItemAt(sewers_stock(), pt);
      },pt => { 
        return sewers.IsWalkable(pt) && m_DiceRoller.RollChance(SEWERS_ITEM_CHANCE);
      });
#endregion

#region 9. Tags.
      for (int x = 0; x < sewers.Width; ++x) {
        for (int y = 0; y < sewers.Height; ++y) {
          if (m_DiceRoller.RollChance(SEWERS_TAG_CHANCE)) {
            Tile tileAt = sewers.GetTileAt(x, y);
            if (!tileAt.Model.IsWalkable && CountAdjWalkables(sewers, x, y) >= 2)
              tileAt.AddDecoration(m_DiceRoller.Choose(TAGS));
          }
        }
      }
#endregion

      district.SewersMap = sewers;
      return sewers;
    }

    public Map GenerateSubwayMap(int seed, District district)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map subway = new Map(seed, string.Format("Subway@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y), district, district.EntryMap.Width, district.EntryMap.Height, Lighting.DARKNESS);
      TileFill(subway, GameTiles.WALL_BRICK, true);

      district.SubwayMap = subway;

      /////////////////////////////////////
      // 1. Trace rail line.
      // 2. Make station linked to surface?
      // 3. Small tools room.
      // 4. Tags & Posters almost everywhere.
      // 5. Additional jobs.
      /////////////////////////////////////

#region 1. Trace rail line.
      // rail line is 4 squares high (does not scale until close to 900 turns/hour)
      // reseruved coordinates are y1 to y1+3 inclusive, so subway.Width/2-1 to subway.Width/2+2
      Map entryMap = district.EntryMap;
      int railY = subway.Width / 2 - 1;
      const int height = 4;
      Rectangle tmp = new Rectangle(0, railY, subway.Width, height); // start as rails
      DoForEachTile(tmp, (Action<Point>)(pt => { subway.SetTileModelAt(pt.X, pt.Y, GameTiles.RAIL_EW); }));
      subway.AddZone(MakeUniqueZone("rails", tmp));
      DoForEachTile(new Rectangle(0, railY-1, subway.Width, height+2), (Action<Point>)(pt => { Session.Get.ForcePoliceKnown(new Location(subway, pt)); }));
#endregion

#region 2. Make station linked to surface.
      List<Block> blockList = null;
      foreach (Block mSurfaceBlock in m_SurfaceBlocks) {
        if (mSurfaceBlock.BuildingRect.Width > m_Params.MinBlockSize + 2) continue;
        if (mSurfaceBlock.BuildingRect.Height > m_Params.MinBlockSize + 2) continue;
        if (IsThereASpecialBuilding(entryMap, mSurfaceBlock.InsideRect)) continue;
        // unclear whether this scales with turns per hour.
        // If anything, at high magnifications we may need to not be "too far" from the rails either
        const int minDistToRails = 8;
        bool flag = false;
        // old test failed for subway.Width/2-1-minDistToRails to subway.Width/2+2+minDistToRails
        // at district size 50: railY 24, upper bound 27; 38 should pass
        // we want a simple interval-does-not-intersect test
        if (mSurfaceBlock.Rectangle.Top - minDistToRails > railY-1+height) flag = true;  // top below critical y
        if (mSurfaceBlock.Rectangle.Bottom + minDistToRails-1 < railY) flag = true;   // bottom above critical y
        if (flag) {
          if (blockList == null) blockList = new List<Block>(m_SurfaceBlocks.Count);
          blockList.Add(mSurfaceBlock);
          break;
        }
      }
      if (blockList != null) {
        Block block = m_DiceRoller.Choose(blockList);
        ClearRectangle(entryMap, block.BuildingRect);
        TileFill(entryMap, GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        m_SurfaceBlocks.Remove(block);
        Block b1 = new Block(block.Rectangle);
        Point exitPosition = new Point(b1.BuildingRect.Left + b1.BuildingRect.Width / 2, b1.InsideRect.Top);
        MakeSubwayStationBuilding(entryMap, true, b1, subway, exitPosition);
        Block b2 = new Block(block.Rectangle);
        MakeSubwayStationBuilding(subway, false, b2, entryMap, exitPosition);
      }
#endregion
#region 3.  Small tools room.
      const int toolsRoomWidth = 5;
      const int toolsRoomHeight = 5;
      Direction direction = m_DiceRoller.RollChance(50) ? Direction.N : Direction.S;
      Rectangle rect = Rectangle.Empty;
      bool flag1 = false;
      int num3 = 0;
      do {
        int x2 = m_DiceRoller.Roll(10, subway.Width - 10);
        int y2 = direction == Direction.N ? railY - 1 : railY + height;
        if (!subway.GetTileModelAt(x2, y2).IsWalkable) {
          rect = direction != Direction.N ? new Rectangle(x2, y2, toolsRoomWidth, toolsRoomHeight) : new Rectangle(x2, y2 - toolsRoomHeight + 1, toolsRoomWidth, toolsRoomHeight);
          flag1 = CheckForEachTile(rect, (Predicate<Point>) (pt => !subway.GetTileModelAt(pt).IsWalkable));
        }
        ++num3;
      }
      while (num3 < subway.Width * subway.Height && !flag1);
      if (flag1) {
        TileFill(subway, GameTiles.FLOOR_CONCRETE, rect);
        TileRectangle(subway, GameTiles.WALL_BRICK, rect);
        PlaceDoor(subway, rect.Left + 2, direction == Direction.N ? rect.Bottom - 1 : rect.Top, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
        subway.AddZone(MakeUniqueZone("tools room", rect));
        DoForEachTile(rect, (Action<Point>) (pt =>
        {
          if (!subway.IsWalkable(pt.X, pt.Y) || CountAdjWalls(subway, pt.X, pt.Y) == 0 || CountAdjDoors(subway, pt.X, pt.Y) > 0) return;
          subway.PlaceAt(MakeObjShelf(), pt);
          subway.DropItemAt(MakeShopConstructionItem(), pt);
        }));
        DoForEachTile(rect, (Action<Point>)(pt => { Session.Get.ForcePoliceKnown(new Location(subway, pt)); }));
      }
#endregion
#region 4. Tags & Posters almost everywhere.
      for (int x2 = 0; x2 < subway.Width; ++x2) {
        for (int y2 = 0; y2 < subway.Height; ++y2) {
          if (m_DiceRoller.RollChance(SUBWAY_TAGS_POSTERS_CHANCE)) {
            Tile tileAt = subway.GetTileAt(x2, y2);
            if (!tileAt.Model.IsWalkable && CountAdjWalkables(subway, x2, y2) >= 2) {
              if (m_DiceRoller.RollChance(50))
                tileAt.AddDecoration(m_DiceRoller.Choose(POSTERS));
              if (m_DiceRoller.RollChance(50))
                tileAt.AddDecoration(m_DiceRoller.Choose(TAGS));
            }
          }
        }
      }
#endregion

      return subway;
    }

    // XXX object orientation would lift this to BaseMapGenerator, starting a chain reaction of object orientation related changes.
    // Deferring until a non-town map is needed (something outside of city limits...National Guard base or the gas station supplying the bikers and gangsters)
    private void QuadSplit(Rectangle rect, int minWidth, int minHeight, out int splitX, out int splitY, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight)
    {
      int width1 = m_DiceRoller.Roll(rect.Width / 3, 2 * rect.Width / 3);
      int height1 = m_DiceRoller.Roll(rect.Height / 3, 2 * rect.Height / 3);
      if (width1 < minWidth) width1 = minWidth;
      if (height1 < minHeight) height1 = minHeight;
      int width2 = rect.Width - width1;
      int height2 = rect.Height - height1;
      bool flag1 = true;
      bool flag2 = true;
      if (width2 < minWidth) {
        width1 = rect.Width;
        width2 = 0;
        flag2 = false;
      }
      if (height2 < minHeight) {
        height1 = rect.Height;
        height2 = 0;
        flag1 = false;
      }
      splitX = rect.Left + width1;
      splitY = rect.Top + height1;
      topLeft = new Rectangle(rect.Left, rect.Top, width1, height1);
      topRight = (flag2 ? new Rectangle(splitX, rect.Top, width2, height1) : Rectangle.Empty);
      bottomLeft = (flag1 ? new Rectangle(rect.Left, splitY, width1, height2) : Rectangle.Empty);
      bottomRight = ((flag2 && flag1) ? new Rectangle(splitX, splitY, width2, height2) : Rectangle.Empty);
    }

    private void MakeBlocks(Map map, bool makeRoads, ref List<Block> list, Rectangle rect)
    {
      QuadSplit(rect, m_Params.MinBlockSize + 1, m_Params.MinBlockSize + 1, out int splitX, out int splitY, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty) {
        if (makeRoads) {
          MakeRoad(map, GameTiles.ROAD_ASPHALT_EW, new Rectangle(rect.Left, rect.Top, rect.Width, 1));
          MakeRoad(map, GameTiles.ROAD_ASPHALT_EW, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1));
          MakeRoad(map, GameTiles.ROAD_ASPHALT_NS, new Rectangle(rect.Left, rect.Top, 1, rect.Height));
          MakeRoad(map, GameTiles.ROAD_ASPHALT_NS, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height));
          topLeft.Width -= 2;
          topLeft.Height -= 2;
          topLeft.Offset(1, 1);
        }
        list.Add(new Block(topLeft));
      } else {
        MakeBlocks(map, makeRoads, ref list, topLeft);
        if (!topRight.IsEmpty) MakeBlocks(map, makeRoads, ref list, topRight);
        if (!bottomLeft.IsEmpty) MakeBlocks(map, makeRoads, ref list, bottomLeft);
        if (bottomRight.IsEmpty) return;
        MakeBlocks(map, makeRoads, ref list, bottomRight);
      }
    }

    protected virtual void MakeRoad(Map map, TileModel roadModel, Rectangle rect)
    {
      TileFill(map, roadModel, rect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        if (!GameTiles.IsRoadModel(prevmodel)) return;
        map.SetTileModelAt(x, y, prevmodel);
      }));
      map.AddZone(MakeUniqueZone("road", rect));
    }

    protected virtual void PlaceDoor(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      map.SetTileModelAt(x, y, floor);
      MapObjectPlace(map, x, y, door);
    }

    protected virtual void PlaceDoorIfNoObject(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      if (!map.HasMapObjectAt(x, y)) PlaceDoor(map, x, y, floor, door);
    }

    protected virtual bool PlaceDoorIfAccessible(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
    {
      Point pt = new Point(x, y);
      int num = Direction.COMPASS.Select(d => pt+d).Count(pt2 => map.IsWalkable(pt2));  // includes IsInBounds check
      if (num < minAccessibility) return false;
      PlaceDoorIfNoObject(map, x, y, floor, door);
      return true;
    }

    protected virtual bool PlaceDoorIfAccessibleAndNotAdjacent(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
    {
      int num = 0;
      Point point1 = new Point(x, y);
      foreach (Direction direction in Direction.COMPASS) {  // micro-optimized: loop combines a reject-any check with a counting operation
        Point point2 = point1 + direction;
        if (map.IsWalkable(point2)) ++num;
        if (map.GetMapObjectAt(point2) is DoorWindow) return false;
      }
      if (num < minAccessibility) return false;
      PlaceDoorIfNoObject(map, x, y, floor, door);
      return true;
    }

    protected virtual void AddWreckedCarsOutside(Map map, Rectangle rect)
    {
      MapObjectFill(map, rect, (Func<Point, MapObject>) (pt =>
      {
        if (m_DiceRoller.RollChance(m_Params.WreckedCarChance)) {
          Tile tileAt = map.GetTileAt(pt);
          if (!tileAt.IsInside && tileAt.Model.IsWalkable && tileAt.Model != GameTiles.FLOOR_GRASS) {
            MapObject mapObj = MakeObjWreckedCar(m_DiceRoller);
            if (m_DiceRoller.RollChance(50)) mapObj.Ignite();
            return mapObj;
          }
        }
        return null;
      }));
    }

    static private bool IsThereASpecialBuilding(Map map, Rectangle rect)
    {
      List<Zone> zonesAt = map.GetZonesAt(rect.Left, rect.Top);
      if (null != zonesAt && zonesAt.Any(zone=> zone.Name.Contains("Sewers Maintenance") || zone.Name.Contains("Subway Station") || zone.Name.Contains("office") || zone.Name.Contains("shop")))
        return true;
      return map.HasAnExitIn(rect); // relatively slow compared to above
    }

    protected virtual bool MakeShopBuilding(Map map, Block b)
    {
#if DEBUG
      if (null == map.District) throw new ArgumentNullException(nameof(map.District));
#endif
      if (b.InsideRect.Width < 5 || b.InsideRect.Height < 5) return false;
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_STONE, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_TILES, b.InsideRect, true);
      ShopType shopType = (ShopType)m_DiceRoller.Roll(0, (int)ShopType._COUNT);
      int left1 = b.InsideRect.Left;
      int top1 = b.InsideRect.Top;
      int right = b.InsideRect.Right;
      int bottom = b.InsideRect.Bottom;
      bool horizontalAlleys = b.Rectangle.Width >= b.Rectangle.Height;
      int centralAlley;
      if (horizontalAlleys) {
        ++left1;
        --right;
        centralAlley = b.InsideRect.Left + b.InsideRect.Width / 2;
      } else {
        ++top1;
        --bottom;
        centralAlley = b.InsideRect.Top + b.InsideRect.Height / 2;
      }
      Rectangle alleysRect = Rectangle.FromLTRB(left1, top1, right, bottom);
      MapObjectFill(map, alleysRect, pt => {
        if (!horizontalAlleys ? (pt.X - alleysRect.Left) % 2 == 1 && pt.Y != centralAlley : (pt.Y - alleysRect.Top) % 2 == 1 && pt.X != centralAlley) {
          return MakeObjShelf();    // XXX why not the shop items as well at this time?
        }
        return null;
      });
      PlaceShoplikeEntrance(map, b, GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor);
      string basename;
      string shopImage;
      switch (shopType) {
        case ShopType.GENERAL_STORE:
          shopImage = GameImages.DECO_SHOP_GENERAL_STORE;
          basename = "GeneralStore";
          break;
        case ShopType.GROCERY:
          shopImage = GameImages.DECO_SHOP_GROCERY;
          basename = "Grocery";
          break;
        case ShopType.SPORTSWEAR:
          shopImage = GameImages.DECO_SHOP_SPORTSWEAR;
          basename = "Sportswear";
          break;
        case ShopType.PHARMACY:
          shopImage = GameImages.DECO_SHOP_PHARMACY;
          basename = "Pharmacy";
          break;
        case ShopType.CONSTRUCTION:
          shopImage = GameImages.DECO_SHOP_CONSTRUCTION;
          basename = "Construction";
          break;
        case ShopType.GUNSHOP:
          shopImage = GameImages.DECO_SHOP_GUNSHOP;
          basename = "Gunshop";
          break;
        case ShopType.HUNTING:
          shopImage = GameImages.DECO_SHOP_HUNTING;
          basename = "Hunting Shop";
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled shoptype");
      }
      DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.HasMapObjectAt(x, y) || CountAdjDoors(map, x, y) < 1) return null;
        return shopImage;
      }));
      Rectangle rectangle;
      if (m_DiceRoller.RollChance(SHOP_WINDOW_CHANCE)) {
        int x2;
        int y2;
        rectangle = b.BuildingRect;
        switch (m_DiceRoller.Roll(0, 4)) {
          case 0:
            x2 = rectangle.Left + rectangle.Width / 2;
            y2 = rectangle.Top;
            break;
          case 1:
            x2 = rectangle.Left + rectangle.Width / 2;
            y2 = rectangle.Bottom - 1;
            break;
          case 2:
            x2 = rectangle.Left;
            y2 = rectangle.Top + rectangle.Height / 2;
            break;
#if DEBUG
          case 3:
#else
          default:
#endif
            x2 = rectangle.Right - 1;
            y2 = rectangle.Top + rectangle.Height / 2;
            break;
#if DEBUG
          default:
            throw new ArgumentOutOfRangeException("unhandled side");
#endif
        }
        if (!map.GetTileModelAt(x2, y2).IsWalkable)
          PlaceDoor(map, x2, y2, GameTiles.FLOOR_TILES, MakeObjWindow());
      }
      if (shopType == ShopType.GUNSHOP)
        BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      ItemsDrop(map, b.InsideRect, pt => {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || MapObject.IDs.SHOP_SHELF != mapObjectAt.ID) return false;
        return m_DiceRoller.RollChance(m_Params.ItemInShopShelfChance);
      }, pt => MakeRandomShopItem(shopType));
      map.AddZone(MakeUniqueZone(basename, b.BuildingRect));
      MakeWalkwayZones(map, b);
      DoForEachTile(b.BuildingRect,pt => {
          Session.Get.ForcePoliceKnown(new Location(map, pt));  // XXX exceptionally cheating police AI
      });
      if (m_DiceRoller.RollChance(SHOP_BASEMENT_CHANCE)) {
        int seed = map.Seed << 1 ^ basename.GetHashCode();
        string name = "basement-" + basename + string.Format("{0}{1}@{2}-{3}", (object)m_Params.District.WorldPosition.X, (object)m_Params.District.WorldPosition.Y, (object)(b.BuildingRect.Left + b.BuildingRect.Width / 2), (object)(b.BuildingRect.Top + b.BuildingRect.Height / 2));
        rectangle = b.BuildingRect;
        int width = rectangle.Width;
        int height = rectangle.Height;
        Map shopBasement = new Map(seed, name, map.District, width, height, Lighting.DARKNESS);
        TileFill(shopBasement, GameTiles.FLOOR_CONCRETE, true);
        TileRectangle(shopBasement, GameTiles.WALL_BRICK, shopBasement.Rect);
        shopBasement.AddZone(MakeUniqueZone("basement", shopBasement.Rect));
        DoForEachTile(shopBasement.Rect, (Action<Point>) (pt =>
        {
          Session.Get.PoliceInvestigate.Record(shopBasement, pt);
          if (!shopBasement.IsWalkable(pt.X, pt.Y) || shopBasement.HasExitAt(pt)) return;
          if (m_DiceRoller.RollChance(SHOP_BASEMENT_SHELF_CHANCE_PER_TILE)) {
            shopBasement.PlaceAt(MakeObjShelf(), pt);
            if (m_DiceRoller.RollChance(SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF)) {
              Session.Get.PoliceInvestigate.Record(shopBasement, pt);
              MakeRandomShopItem(shopType)?.DropAt(shopBasement,pt);              
            }
          }
          if (!Session.Get.HasZombiesInBasements || !m_DiceRoller.RollChance(SHOP_BASEMENT_ZOMBIE_RAT_CHANCE)) return;
          shopBasement.PlaceAt(CreateNewBasementRatZombie(0), pt);
        }));
        Point point1 = new Point((m_DiceRoller.RollChance(50) ? 1 : shopBasement.Width - 2),(m_DiceRoller.RollChance(50) ? 1 : shopBasement.Height - 2));
        rectangle = b.InsideRect;
        Point point2 = new Point(point1.X - 1 + rectangle.Left, point1.Y - 1 + rectangle.Top);
        AddExit(shopBasement, point1, map, point2, GameImages.DECO_STAIRS_UP, true);
        AddExit(map, point2, shopBasement, point1, GameImages.DECO_STAIRS_DOWN, true);
        if (!map.HasMapObjectAt(point2)) map.RemoveMapObjectAt(point2.X, point2.Y);
        m_Params.District.AddUniqueMap(shopBasement);
      }
      return true;
    }

    private CHARBuildingType MakeCHARBuilding(Map map, Block b)
    {
      if (b.InsideRect.Width < 8 || b.InsideRect.Height < 8)
        return MakeCHARAgency(map, b) ? CHARBuildingType.AGENCY : CHARBuildingType.NONE;
      return MakeCHAROffice(map, b) ? CHARBuildingType.OFFICE : CHARBuildingType.NONE;
    }

    // natural BaseMapGenerator, but anchored by PlaceDoor
    protected Direction PlaceShoplikeEntrance(Map map, Block b, TileModel model, Func<DoorWindow> make_door)
    {
      bool orientation_ew = b.InsideRect.Width >= b.InsideRect.Height;
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      Direction ret;
      int door_edge;
      if (orientation_ew) {
        if (m_DiceRoller.RollChance(50)) {
          ret = Direction.W;
          door_edge = b.BuildingRect.Left;
        } else {
          ret = Direction.E;
          door_edge = b.BuildingRect.Right - 1;
        }
        PlaceDoor(map, door_edge, y1, model, make_door());
          if (b.InsideRect.Height >= 8) {
            PlaceDoor(map, door_edge, y1 - 1, model, make_door());
            if (b.InsideRect.Height >= 12)
              PlaceDoor(map, door_edge, y1 + 1, model, make_door());
        }
      } else {
        if (m_DiceRoller.RollChance(50)) {
          ret = Direction.N;
          door_edge = b.BuildingRect.Top;
        } else {
          ret = Direction.S;
          door_edge = b.BuildingRect.Bottom - 1;
        }
        PlaceDoor(map, x1, door_edge, model, make_door());
        if (b.InsideRect.Width >= 8) {
          PlaceDoor(map, x1 - 1, door_edge, model, make_door());
          if (b.InsideRect.Width >= 12)
            PlaceDoor(map, x1 + 1, door_edge, model, make_door());
        }
      }
      return ret;
    }

    private bool MakeCHARAgency(Map map, Block b)
    {
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_OFFICE, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        map.SetIsInsideAt(x,y);
        tile.AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO);
      }));
      PlaceShoplikeEntrance(map, b, GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor);
      DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.HasMapObjectAt(x, y) || CountAdjDoors(map, x, y) < 1) return null;
        return GameImages.DECO_CHAR_OFFICE;
      }));
      MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        return MakeObjChair(GameImages.OBJ_CHAR_CHAIR);
      }));
      TileFill(map, GameTiles.WALL_CHAR_OFFICE, new Rectangle(b.InsideRect.Left + b.InsideRect.Width / 2 - 1, b.InsideRect.Top + b.InsideRect.Height / 2 - 1, 3, 2), (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration(m_DiceRoller.Choose(CHAR_POSTERS))));
      DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (CountAdjDoors(map, x, y) > 0) return null;
        if (m_DiceRoller.RollChance(25)) return m_DiceRoller.Choose(CHAR_POSTERS);
        return null;
      }));
      map.AddZone(MakeUniqueZone("CHAR Agency", b.BuildingRect));
      MakeWalkwayZones(map, b);
      return true;
    }

    private void PopulateCHAROfficeBuilding(Map map, Point[] locs)
    {
      for (int index = 0; index < MAX_CHAR_GUARDS_PER_OFFICE; ++index) {
        map.PlaceAt(CreateNewCHARGuard(0), locs[index]); // do not use the ActorPlace function as we have pre-arranged the conditions when initializing the locs array
      }
    }

    private bool MakeCHAROffice(Map map, Block b)
    {
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_OFFICE, b.InsideRect, true);
      bool orientation_ew = b.InsideRect.Width >= b.InsideRect.Height;  // must agree with copy in PlaceShoplikeEntrance
      Point midpoint = new Point(b.Rectangle.Left + b.Rectangle.Width / 2, b.Rectangle.Top + b.Rectangle.Height / 2);
      Direction direction = PlaceShoplikeEntrance(map, b, GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor);
      DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.HasMapObjectAt(x, y) || CountAdjDoors(map, x, y) < 1) return null;
        return GameImages.DECO_CHAR_OFFICE;
      }));
      BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);

      Point[] CHAR_guard_locs = new Point[MAX_CHAR_GUARDS_PER_OFFICE];
      int end_foyer_wall = -1;
      if (direction == Direction.N) {
        end_foyer_wall = b.InsideRect.Top + 3;
        CHAR_guard_locs[0] = new Point(midpoint.X,end_foyer_wall-1);
        CHAR_guard_locs[1] = new Point(midpoint.X-1, b.InsideRect.Top);
        CHAR_guard_locs[2] = new Point(midpoint.X+1, b.InsideRect.Top);
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(b.InsideRect.Left, end_foyer_wall, b.InsideRect.Width, b.InsideRect.Height-3)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
      } else if (direction == Direction.S) {
        end_foyer_wall = b.InsideRect.Bottom - 1 - 3;
        CHAR_guard_locs[0] = new Point(midpoint.X,end_foyer_wall+1);
        CHAR_guard_locs[1] = new Point(midpoint.X-1, b.InsideRect.Bottom - 1);
        CHAR_guard_locs[2] = new Point(midpoint.X+1, b.InsideRect.Bottom - 1);
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height-3)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
      } else if (direction == Direction.E) {
        end_foyer_wall = b.InsideRect.Right - 1 - 3;
        CHAR_guard_locs[0] = new Point(end_foyer_wall+1, midpoint.Y);
        CHAR_guard_locs[1] = new Point(b.InsideRect.Right - 1, midpoint.Y-1);
        CHAR_guard_locs[2] = new Point(b.InsideRect.Right - 1, midpoint.Y+1);
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width-3, b.InsideRect.Height)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
#if DEBUG
      } else if (direction == Direction.W) {
#else
      } else {
#endif
        end_foyer_wall = b.InsideRect.Left + 3;
        CHAR_guard_locs[0] = new Point(end_foyer_wall-1, midpoint.Y);
        CHAR_guard_locs[1] = new Point(b.InsideRect.Left, midpoint.Y-1);
        CHAR_guard_locs[2] = new Point(b.InsideRect.Left, midpoint.Y+1);
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(end_foyer_wall, b.InsideRect.Top, b.InsideRect.Width-3, b.InsideRect.Height)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
      }
#if DEBUG
      else throw new InvalidOperationException("unhandled door side");
#endif

      if (orientation_ew) TileVLine(map, GameTiles.WALL_CHAR_OFFICE, end_foyer_wall, b.InsideRect.Top, b.InsideRect.Height);
      else TileHLine(map, GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left, end_foyer_wall, b.InsideRect.Width);

      Rectangle restricted_zone;
      if (direction == Direction.N) {
        restricted_zone = new Rectangle(midpoint.X - 1, end_foyer_wall, 3, b.BuildingRect.Height - 1 - 3);
      } else if (direction == Direction.S) {
        restricted_zone = new Rectangle(midpoint.X - 1, b.BuildingRect.Top, 3, b.BuildingRect.Height - 1 - 3);
      } else if (direction == Direction.E) {
        restricted_zone = new Rectangle(b.BuildingRect.Left, midpoint.Y - 1, b.BuildingRect.Width - 1 - 3, 3);
#if DEBUG
      } else if (direction == Direction.W) {
#else
      } else {
#endif
        restricted_zone = new Rectangle(end_foyer_wall, midpoint.Y - 1, b.BuildingRect.Width - 1 - 3, 3);
      }
#if DEBUG
      else throw new InvalidOperationException("unhandled door side");
#endif

      TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, restricted_zone);

      { // \todo arrange for this door to be mechanically locked
      Point chokepoint_door_pos = (orientation_ew ? new Point(end_foyer_wall, midpoint.Y) : new Point(midpoint.X, end_foyer_wall));
      PlaceDoor(map, chokepoint_door_pos.X, chokepoint_door_pos.Y, GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      
      Rectangle rect2;
      Rectangle rect3;
      if (orientation_ew) {
        int left = restricted_zone.Left;
        int top = b.BuildingRect.Top;
        int width = restricted_zone.Width;
        rect2 = new Rectangle(left, top, width, midpoint.Y - top);
        rect3 = new Rectangle(left, midpoint.Y + 1, width, b.BuildingRect.Bottom - midpoint.Y -1);
      } else {
        int left = b.BuildingRect.Left;
        int top = restricted_zone.Top;
        int height = restricted_zone.Height;
        rect2 = new Rectangle(left, top, midpoint.X - left, height);
        rect3 = new Rectangle(midpoint.X + 1, top, b.BuildingRect.Right - midpoint.X -1, height);
      }
      List<Rectangle> list1 = new List<Rectangle>();
      MakeRoomsPlan(map, ref list1, rect2, 4);
      List<Rectangle> list2 = new List<Rectangle>();
      MakeRoomsPlan(map, ref list2, rect3, 4);
      List<Rectangle> rectangleList = new List<Rectangle>(list1.Count + list2.Count);
      rectangleList.AddRange(list1);
      rectangleList.AddRange(list2);
      foreach (Rectangle rect4 in list1) {
        TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, rect4);
        map.AddZone(MakeUniqueZone("Office room", rect4));
      }
      foreach (Rectangle rect4 in list2) {
        TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, rect4);
        map.AddZone(MakeUniqueZone("Office room", rect4));
      }
      foreach (Rectangle rectangle2 in list1){
        if (orientation_ew)
          PlaceDoor(map, rectangle2.Left + rectangle2.Width / 2, rectangle2.Bottom - 1, GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
        else
          PlaceDoor(map, rectangle2.Right - 1, rectangle2.Top + rectangle2.Height / 2, GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      foreach (Rectangle rectangle2 in list2) {
        if (orientation_ew)
          PlaceDoor(map, rectangle2.Left + rectangle2.Width / 2, rectangle2.Top, GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
        else
          PlaceDoor(map, rectangle2.Left, rectangle2.Top + rectangle2.Height / 2, GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      var table_pos = new List<Point>(rectangleList.Count);
      var chair_pos = new List<Point>(8);
      var chairs_pos = new List<KeyValuePair<Point,Point>>(28);
      foreach (Rectangle rectangle2 in rectangleList) {
        Point tablePos = new Point(rectangle2.Left + rectangle2.Width / 2, rectangle2.Top + rectangle2.Height / 2);
        map.PlaceAt(MakeObjTable(GameImages.OBJ_CHAR_TABLE), tablePos);
        table_pos.Add(tablePos);
        Rectangle rect4 = new Rectangle(rectangle2.Left + 1, rectangle2.Top + 1, rectangle2.Width - 2, rectangle2.Height - 2);
        if (rect4.IsEmpty) continue;
        Rectangle rect5 = new Rectangle(tablePos.X - 1, tablePos.Y - 1, 3, 3);
        rect5.Intersect(rect4);
        rect5.DoForEach(pt=>chair_pos.Add(pt),pt=> !map.HasMapObjectAt(pt));    // table is already placed
        if (2 >= chair_pos.Count) {
          foreach(Point pt in chair_pos) MakeObjChair(GameImages.OBJ_CHAR_CHAIR)?.PlaceAt(map,pt);
          chair_pos.Clear();
          continue;
        }
        // enumerate Choose(n,2) options
        int i = chair_pos.Count;
        while(1 < i--)
            {
            int j = i;
            while(0 < j--)
                {
                chairs_pos.Add(new KeyValuePair<Point,Point>(chair_pos[i],chair_pos[j]));
                }
            }
        // \todo geometric postprocessing
        KeyValuePair<Point,Point> chairs_at = m_DiceRoller.Choose(chairs_pos);
        MakeObjChair(GameImages.OBJ_CHAR_CHAIR)?.PlaceAt(map,chairs_at.Key);
        MakeObjChair(GameImages.OBJ_CHAR_CHAIR)?.PlaceAt(map,chairs_at.Value);
        chair_pos.Clear();
        chairs_pos.Clear();
      }
      foreach (Rectangle rect4 in rectangleList)
        ItemsDrop(map, rect4, (pt => map.GetTileModelAt(pt) == GameTiles.FLOOR_OFFICE && !map.HasMapObjectAt(pt)), pt => MakeRandomCHAROfficeItem());

      (new ItemEntertainment(GameItems.CHAR_GUARD_MANUAL))?.DropAt(map, m_DiceRoller.Choose(table_pos));
      Zone zone = MakeUniqueZone("CHAR Office", b.BuildingRect);
      zone.SetGameAttribute<bool>("CHAR Office", true);
      map.AddZone(zone);
      MakeWalkwayZones(map, b);

      PopulateCHAROfficeBuilding(map, CHAR_guard_locs);
      return true;
    }

    protected virtual bool MakeParkBuilding(Map map, Block b)
    {
      if (b.InsideRect.Width < 3 || b.InsideRect.Height < 3) return false;
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileFill(map, GameTiles.FLOOR_GRASS, b.InsideRect);
      MapObjectFill(map, b.BuildingRect, (Func<Point, MapObject>) (pt =>
      {
        if (pt.X == b.BuildingRect.Left || pt.X == b.BuildingRect.Right - 1 || pt.Y == b.BuildingRect.Top || pt.Y == b.BuildingRect.Bottom - 1)
          return MakeObjFence();
        return null;
      }));
      MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        return (m_DiceRoller.RollChance(PARK_TREE_CHANCE) ? MakeObjTree() : null);
      }));
      MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        return (m_DiceRoller.RollChance(PARK_BENCH_CHANCE) ? MakeObjBench() : null);
      }));
      int x;
      int y;
      switch (m_DiceRoller.Roll(0, 4)) {
        case 0:
          x = b.BuildingRect.Left;
          y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          break;
        case 1:
          x = b.BuildingRect.Right - 1;
          y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          break;
        case 3:
          x = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          y = b.BuildingRect.Top;
          break;
        default:
          x = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          y = b.BuildingRect.Bottom - 1;
          break;
      }
      map.RemoveMapObjectAt(x, y);
      map.SetTileModelAt(x, y, GameTiles.FLOOR_WALKWAY);
      ItemsDrop(map, b.InsideRect, (pt =>
      {
        if (!map.HasMapObjectAt(pt)) return m_DiceRoller.RollChance(PARK_ITEM_CHANCE);
        return false;
      }), (Func<Point, Item>) (pt => MakeRandomParkItem()));
      map.AddZone(MakeUniqueZone("Park", b.BuildingRect));
      MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeHousingBuilding(Map map, Block b)
    {
      if (b.InsideRect.Width < 4 || b.InsideRect.Height < 4) return false;
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_BRICK, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_PLANKS, b.InsideRect, true);
      List<Rectangle> list = new List<Rectangle>();
      MakeRoomsPlan(map, ref list, b.BuildingRect, 5);
      foreach (Rectangle roomRect in list) {
        MakeHousingRoom(map, roomRect, GameTiles.FLOOR_PLANKS, GameTiles.WALL_BRICK);
        FillHousingRoomContents(map, roomRect);
      }
      // XXX post-processing: converts inside windows to doors
      // backstop for a post-condition of MakeHousingRoom
      bool flag = b.BuildingRect.Any(pt => !map.IsInsideAt(pt) && (!(map.GetMapObjectAt(pt) as DoorWindow)?.IsWindow ?? false));
      while(!flag) {
          int x = m_DiceRoller.Roll(b.BuildingRect.Left, b.BuildingRect.Right);
          int y = m_DiceRoller.Roll(b.BuildingRect.Top, b.BuildingRect.Bottom);
          if (!map.IsInsideAt(x, y)) {
            if (map.GetMapObjectAt(x, y) is DoorWindow window && window.IsWindow) {
              map.RemoveMapObjectAt(x, y);
              map.PlaceAt(MakeObjWoodenDoor(), new Point(x, y));
              flag = true;
            }
          }
      }
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_CHANCE))
        m_Params.District.AddUniqueMap(GenerateHouseBasementMap(map, b));
      map.AddZone(MakeUniqueZone("Housing", b.BuildingRect));
      MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual void MakeSewersMaintenanceBuilding(Map map, bool isSurface, Block b, Map linkedMap, Point exitPosition)
    {
      if (!isSurface) TileFill(map, GameTiles.FLOOR_CONCRETE, b.InsideRect);
      TileRectangle(map, GameTiles.WALL_SEWER, b.BuildingRect);
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left) {
        for (int top = b.InsideRect.Top; top < b.InsideRect.Bottom; ++top)
          map.SetIsInsideAt(left, top);
      }
      Direction direction;
      int x;
      int y;
      switch (m_DiceRoller.Roll(0, 4)) {
        case 0:
          direction = Direction.N;
          x = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          y = b.BuildingRect.Top;
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x - 1, y);
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x + 1, y);
          break;
        case 1:
          direction = Direction.S;
          x = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          y = b.BuildingRect.Bottom - 1;
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x - 1, y);
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x + 1, y);
          break;
        case 2:
          direction = Direction.W;
          x = b.BuildingRect.Left;
          y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x, y - 1);
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x, y + 1);
          break;
#if DEBUG
        case 3:
#else
        default:
#endif
          direction = Direction.E;
          x = b.BuildingRect.Right - 1;
          y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x, y - 1);
          map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, x, y + 1);
          break;
#if DEBUG
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
#endif
      }
      PlaceDoor(map, x, y, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
      BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      AddExit(map, exitPosition, linkedMap, exitPosition, (isSurface ? GameImages.DECO_SEWER_HOLE : GameImages.DECO_SEWER_LADDER), true);
      if (!isSurface) {
        Point p = new Point(x, y) + direction;
        while (map.IsInBounds(p) && !map.GetTileModelAt(p).IsWalkable) {
          map.SetTileModelAt(p.X, p.Y, GameTiles.FLOOR_CONCRETE);
          p += direction;
        }
      }
      int num = m_DiceRoller.Roll(Math.Max(b.InsideRect.Width, b.InsideRect.Height), 2 * Math.Max(b.InsideRect.Width, b.InsideRect.Height));
      for (int index = 0; index < num; ++index)
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          map.DropItemAt(MakeShopConstructionItem(), pt);
          Session.Get.PoliceInvestigate.Record(map, pt);
          return MakeObjTable(GameImages.OBJ_TABLE);
        }));
      if (m_DiceRoller.RollChance(33)) {
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjBed(GameImages.OBJ_BED)));
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          map.DropItemAt(MakeItemCannedFood(), pt);
          Session.Get.PoliceInvestigate.Record(map, pt);
          return MakeObjFridge();
        }));
      }
      Actor newCivilian = CreateNewCivilian(0, RogueGame.REFUGEES_WAVE_ITEMS, 1);
      ActorPlace(m_DiceRoller, map, newCivilian, b.InsideRect);
      map.AddZone(MakeUniqueZone("Sewers Maintenance", b.BuildingRect));
    }

    /// <remark>isSurface parameter cannot be calculated as map.District.EntryMap == map because that hasn't been initialized yet</remark>
    private void MakeSubwayStationBuilding(Map map, bool isSurface, Block b, Map linkedMap, Point exitPosition)
    {
      if (!isSurface) TileFill(map, GameTiles.FLOOR_CONCRETE, b.InsideRect, true);
      TileRectangle(map, GameTiles.WALL_SUBWAY, b.BuildingRect);
      DoForEachTile(b.BuildingRect,pt => {
          Session.Get.ForcePoliceKnown(new Location(map, pt));
          Session.Get.PoliceInvestigate.Seen(map, pt);
      });
      Direction direction;
      Point doorAt = new Point(-1,-1);
      bool orientation_ew = false;
      switch (!isSurface ? (b.Rectangle.Bottom < map.Width / 2 ? 1 : 0) : m_DiceRoller.Roll(0, 4)) {
        case 0:
          direction = Direction.N;
          doorAt.X = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          doorAt.Y = b.BuildingRect.Top;
          break;
        case 1:
          direction = Direction.S;
          doorAt.X = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          doorAt.Y = b.BuildingRect.Bottom - 1;
          break;
        case 2:
          direction = Direction.W;
          orientation_ew = true;
          doorAt.X = b.BuildingRect.Left;
          doorAt.Y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          break;
#if DEBUG
        case 3:
#else
        default:
#endif
          direction = Direction.E;
          orientation_ew = true;
          doorAt.X = b.BuildingRect.Right - 1;
          doorAt.Y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          break;
#if DEBUG
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
#endif
      }
      if (isSurface) {
        map.SetTileModelAt(doorAt.X, doorAt.Y, GameTiles.FLOOR_CONCRETE);
        map.PlaceAt(MakeObjGlassDoor(), doorAt);
        if (orientation_ew) { 
            map.AddDecorationAt(GameImages.DECO_SUBWAY_BUILDING, doorAt + Direction.N);
            map.AddDecorationAt(GameImages.DECO_SUBWAY_BUILDING, doorAt + Direction.S);
        } else { 
            map.AddDecorationAt(GameImages.DECO_SUBWAY_BUILDING, doorAt + Direction.W);
            map.AddDecorationAt(GameImages.DECO_SUBWAY_BUILDING, doorAt + Direction.E);
        }
      }
      for (int x2 = exitPosition.X - 1; x2 <= exitPosition.X + 1; ++x2) {
        Point point = new Point(x2, exitPosition.Y);
        AddExit(map, point, linkedMap, point, (isSurface ? GameImages.DECO_STAIRS_DOWN : GameImages.DECO_STAIRS_UP), true);
      }
      if (!isSurface) {
        map.SetTileModelAt(doorAt.X, doorAt.Y, GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(doorAt.X + 1, doorAt.Y, GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(doorAt.X - 1, doorAt.Y, GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(doorAt.X - 2, doorAt.Y, GameTiles.WALL_STONE);
        map.SetTileModelAt(doorAt.X + 2, doorAt.Y, GameTiles.WALL_STONE);
        DoForEachTile(new Rectangle(doorAt.X - 2, doorAt.Y, 5,1),pt => Session.Get.ForcePoliceKnown(new Location(map, pt)));
        Point p = doorAt + direction;
        while (map.IsInBounds(p) && !map.GetTileModelAt(p).IsWalkable) {
          map.SetTileModelAt(p.X, p.Y, GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X - 1, p.Y, GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X + 1, p.Y, GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X - 2, p.Y, GameTiles.WALL_STONE);
          map.SetTileModelAt(p.X + 2, p.Y, GameTiles.WALL_STONE);
          DoForEachTile(new Rectangle(p.X - 2, p.Y, 5,1),pt => Session.Get.ForcePoliceKnown(new Location(map, pt)));
          p += direction;
        }
        int left1 = Math.Max(0, b.BuildingRect.Left - 10);
        int right = Math.Min(map.Width - 1, b.BuildingRect.Right + 10);
        Rectangle rect1;
        int y;
        if (direction == Direction.S) {
          rect1 = Rectangle.FromLTRB(left1, p.Y - 3, right, p.Y);
          y = rect1.Top;
          map.AddZone(MakeUniqueZone("corridor", Rectangle.FromLTRB(doorAt.X - 1, doorAt.Y, doorAt.X + 1 + 1, rect1.Top)));
        } else {
          rect1 = Rectangle.FromLTRB(left1, p.Y + 1, right, p.Y + 1 + 3);
          y = rect1.Bottom - 1;
          map.AddZone(MakeUniqueZone("corridor", Rectangle.FromLTRB(doorAt.X - 1, rect1.Bottom, doorAt.X + 1 + 1, doorAt.Y + 1)));
        }
        TileFill(map, GameTiles.FLOOR_CONCRETE, rect1);
        for (int left2 = rect1.Left; left2 < rect1.Right; ++left2) {
          if (CountAdjWalls(map, left2, y) >= 3)
            map.PlaceAt(MakeObjIronBench(), new Point(left2, y));
        }
        DoForEachTile(rect1,pt => Session.Get.ForcePoliceKnown(new Location(map, pt)));
        map.AddZone(MakeUniqueZone("platform", rect1));
        Point point1 = direction != Direction.S ? new Point(doorAt.X, rect1.Bottom) : new Point(doorAt.X, rect1.Top - 1);
        map.PlaceAt(MakeObjIronGate(), new Point(point1.X, point1.Y));
        map.PlaceAt(MakeObjIronGate(), new Point(point1.X + 1, point1.Y));
        map.PlaceAt(MakeObjIronGate(), new Point(point1.X - 1, point1.Y));
        Point point2;
        Rectangle rect2;
        if (doorAt.X > map.Width / 2) {
          point2 = new Point(doorAt.X - 2, doorAt.Y + 2 * direction.Vector.Y);
          rect2 = Rectangle.FromLTRB(point2.X - 4, point2.Y - 2, point2.X + 1, point2.Y + 2 + 1);
        } else {
          point2 = new Point(doorAt.X + 2, doorAt.Y + 2 * direction.Vector.Y);
          rect2 = Rectangle.FromLTRB(point2.X, point2.Y - 2, point2.X + 4, point2.Y + 2 + 1);
        }
        TileFill(map, GameTiles.FLOOR_CONCRETE, rect2);
        TileRectangle(map, GameTiles.WALL_STONE, rect2);
        PlaceDoor(map, point2.X, point2.Y, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
        map.AddDecorationAt(GameImages.DECO_POWER_SIGN_BIG, point2.X, point2.Y - 1);
        map.AddDecorationAt(GameImages.DECO_POWER_SIGN_BIG, point2.X, point2.Y + 1);
        MapObjectFill(map, rect2, (Func<Point, MapObject>) (pt =>
        {
          if (!map.GetTileModelAt(pt).IsWalkable) return null;
          if (CountAdjWalls(map, pt.X, pt.Y) < 3 || CountAdjDoors(map, pt.X, pt.Y) > 0) return null;
          return MakeObjPowerGenerator();
        }));
        DoForEachTile(rect2, pt => Session.Get.ForcePoliceKnown(new Location(map, pt)));
      }
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left) {
        for (int y = b.InsideRect.Top + 1; y < b.InsideRect.Bottom - 1; ++y) {
          if (CountAdjWalls(map, left, y) >= 2 && CountAdjDoors(map, left, y) <= 0 && !Rules.IsAdjacent(new Point(left, y), doorAt))
            map.PlaceAt(MakeObjIronBench(), new Point(left, y));
        }
      }
      if (isSurface) {
        Actor newPoliceman = CreateNewPoliceman(0);
        if (Session.Get.CMDoptionExists("subway-cop")) {
          int home_district_xy = Session.Get.World.Size/2;
          if (map.District.WorldPosition == new Point(home_district_xy, home_district_xy)) newPoliceman.Controller = new PlayerController();
        }
        ActorPlace(m_DiceRoller, map, newPoliceman, b.InsideRect);
      }
      map.AddZone(MakeUniqueZone("Subway Station", b.BuildingRect));
    }

    protected virtual void MakeRoomsPlan(Map map, ref List<Rectangle> list, Rectangle rect, int minRoomsSize)
    {
      QuadSplit(rect, minRoomsSize, minRoomsSize, out int splitX, out int splitY, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty) {
        list.Add(rect);
      } else {
        MakeRoomsPlan(map, ref list, topLeft, minRoomsSize);
        if (!topRight.IsEmpty) {
          topRight.Offset(-1, 0);
          ++topRight.Width;
          MakeRoomsPlan(map, ref list, topRight, minRoomsSize);
        }
        if (!bottomLeft.IsEmpty) {
          bottomLeft.Offset(0, -1);
          ++bottomLeft.Height;
          MakeRoomsPlan(map, ref list, bottomLeft, minRoomsSize);
        }
        if (bottomRight.IsEmpty) return;
        bottomRight.Offset(-1, -1);
        ++bottomRight.Width;
        ++bottomRight.Height;
        MakeRoomsPlan(map, ref list, bottomRight, minRoomsSize);
      }
    }

    protected virtual void MakeHousingRoom(Map map, Rectangle roomRect, TileModel floor, TileModel wall)
    {
      TileFill(map, floor, roomRect);
      TileRectangle(map, wall, roomRect.Left, roomRect.Top, roomRect.Width, roomRect.Height, (tile, prevmodel, x, y) =>
      {
        if (!map.HasMapObjectAt(x, y)) return;
        map.SetTileModelAt(x, y, floor);
      });
      int x1 = roomRect.Left + roomRect.Width / 2;
      int y1 = roomRect.Top + roomRect.Height / 2;
      Func<int,int,bool> door_window_ok = (x, y) => !map.HasMapObjectAt(x, y) && IsAccessible(map, x, y) && 0 == CountAdjDoors(map, x, y);
      Func<int, int, MapObject> make_door_window = (x, y) => ((!IsInside(map, x, y) && !m_DiceRoller.RollChance(25)) ? MakeObjWindow() : MakeObjWoodenDoor());

      PlaceIf(map, x1, roomRect.Top, floor, door_window_ok, make_door_window);
      PlaceIf(map, x1, roomRect.Bottom - 1, floor, door_window_ok, make_door_window);
      PlaceIf(map, roomRect.Left, y1, floor, door_window_ok, make_door_window);
      PlaceIf(map, roomRect.Right - 1, y1, floor, door_window_ok, make_door_window);
    }

    protected virtual void FillHousingRoomContents(Map map, Rectangle roomRect)
    {
      Rectangle insideRoom = new Rectangle(roomRect.Left + 1, roomRect.Top + 1, roomRect.Width - 2, roomRect.Height - 2);
      switch (m_DiceRoller.Roll(0, 10))
      {
        case 0:
        case 1:
        case 2:
        case 3:
        case 4:
          int num1 = m_DiceRoller.Roll(1, 3);
          for (int index = 0; index < num1; ++index)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              Rectangle rect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
              rect.Intersect(insideRoom);
              MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                return pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0 &&  CountAdjWalls(map, pt2.X, pt2.Y) > 0;
              }), m_DiceRoller, (Func<Point, MapObject>) (pt2 =>
              {
                map.DropItemAt(MakeRandomBedroomItem(), pt2);
                Session.Get.PoliceInvestigate.Record(map, pt2);
                return MakeObjNightTable(GameImages.OBJ_NIGHT_TABLE);
              }));
              return MakeObjBed(GameImages.OBJ_BED);
            }));
          int num2 = m_DiceRoller.Roll(1, 4);
          for (int index = 0; index < num2; ++index)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              map.DropItemAt(MakeRandomBedroomItem(), pt);
              Session.Get.PoliceInvestigate.Record(map, pt);
              return (m_DiceRoller.RollChance(50) ? MakeObjWardrobe(GameImages.OBJ_WARDROBE) : MakeObjDrawer());
            }));
          break;
        case 5:
        case 6:
        case 7:
          int num3 = m_DiceRoller.Roll(1, 3);
          for (int index1 = 0; index1 < num3; ++index1)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt.X, pt.Y) == 0 &&  CountAdjDoors(map, pt.X, pt.Y) == 0;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              for (int index = 0; index < HOUSE_LIVINGROOM_ITEMS_ON_TABLE; ++index) {
                map.DropItemAt(MakeRandomKitchenItem(), pt);
              }
              Session.Get.PoliceInvestigate.Record(map, pt);
              Rectangle rect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
              rect.Intersect(insideRoom);
              MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                return pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0;
              }), m_DiceRoller, pt2 => MakeObjChair(GameImages.OBJ_CHAIR));
              return MakeObjTable(GameImages.OBJ_TABLE);
            }));
          int num4 = m_DiceRoller.Roll(1, 3);
          for (int index = 0; index < num4; ++index)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjDrawer()));
          break;
        case 8:
        case 9:
          MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            return CountAdjWalls(map, pt.X, pt.Y) == 0 && CountAdjDoors(map, pt.X, pt.Y) == 0;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_ON_TABLE; ++index) {
              map.DropItemAt(MakeRandomKitchenItem(), pt);
            }
            Session.Get.PoliceInvestigate.Record(map, pt);
            MapObjectPlaceInGoodPosition(map, new Rectangle(pt.X - 1, pt.Y - 1, 3, 3), (Func<Point, bool>) (pt2 =>
            {
              return pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0;
            }), m_DiceRoller, pt2 => MakeObjChair(GameImages.OBJ_CHAIR));
            return MakeObjTable(GameImages.OBJ_TABLE);
          }));
          MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            return CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_IN_FRIDGE; ++index) {
              map.DropItemAt(MakeRandomKitchenItem(), pt);
            }
            Session.Get.PoliceInvestigate.Record(map, pt);
            return MakeObjFridge();
          }));
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    private Item MakeRandomShopItem(ShopType shop)
    {
      switch (shop) {
        case ShopType.GENERAL_STORE: return MakeShopGeneralItem();
        case ShopType.GROCERY: return MakeShopGroceryItem();
        case ShopType.SPORTSWEAR: return MakeShopSportsWearItem();
        case ShopType.PHARMACY: return MakeShopPharmacyItem();
        case ShopType.CONSTRUCTION: return MakeShopConstructionItem();
        case ShopType.GUNSHOP: return MakeShopGunshopItem();
        case ShopType.HUNTING: return MakeHuntingShopItem();
        default: throw new ArgumentOutOfRangeException("unhandled shoptype");
      }
    }

    private ItemFood MakeShopGroceryItem()
    {
      return (m_DiceRoller.RollChance(50) ? MakeItemCannedFood() : MakeItemGroceries());
    }

    private Item MakeShopPharmacyItem()
    {
      switch (m_DiceRoller.Roll(0, 6)) {
        case 0: return MakeItemBandages();
        case 1: return MakeItemMedikit();
        case 2: return MakeItemPillsSLP();
        case 3: return MakeItemPillsSTA();
        case 4: return MakeItemPillsSAN();
#if DEBUG
        case 5: return MakeItemStenchKiller();
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return MakeItemStenchKiller();
#endif
      }
    }

    private Item MakeShopSportsWearItem()
    {
      switch (m_DiceRoller.Roll(0, 10))
      {
        case 0: return (m_DiceRoller.RollChance(30) ? (Item)MakeItemHuntingRifle() : MakeItemLightRifleAmmo());
        case 1: return (m_DiceRoller.RollChance(30) ? (Item)MakeItemHuntingCrossbow() : MakeItemBoltsAmmo());
        case 2:
        case 3:
        case 4:
        case 5: return MakeItemBaseballBat();
        case 6:
        case 7: return MakeItemIronGolfClub();
        case 8:
#if DEBUG
        case 9: return MakeItemGolfClub();
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return MakeItemGolfClub();
#endif
      }
    }

    private Item MakeShopConstructionItem()
    {
      switch (m_DiceRoller.Roll(0, 24)) {
        case 0:
        case 1:
        case 2: return (m_DiceRoller.RollChance(50) ? MakeItemShovel() : MakeItemShortShovel());
        case 3:
        case 4:
        case 5: return MakeItemCrowbar();
        case 6:
        case 7:
        case 8: return (m_DiceRoller.RollChance(50) ? MakeItemHugeHammer() : MakeItemSmallHammer());
        case 9:
        case 10:
        case 11: return MakeItemWoodenPlank();
        case 12:
        case 13:
        case 14: return MakeItemFlashlight();
        case 15:
        case 16:
        case 17: return MakeItemBigFlashlight();
        case 18:
        case 19:
        case 20: return MakeItemSpikes();
#if DEBUG
        case 21:
        case 22:
        case 23: return MakeItemBarbedWire();
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return MakeItemBarbedWire();
#endif
      }
    }

    private Item MakeShopGunshopItem()
    {
      if (m_DiceRoller.RollChance(40)) {
        switch (m_DiceRoller.Roll(0, 4)) {
          case 0: return MakeItemRandomPistol();
          case 1: return MakeItemShotgun();
          case 2: return MakeItemHuntingRifle();
#if DEBUG
          case 3: return MakeItemHuntingCrossbow();
          default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
          default: return MakeItemHuntingCrossbow();
#endif
        }
      }

      switch (m_DiceRoller.Roll(0, 4)) {
        case 0: return MakeItemLightPistolAmmo();
        case 1: return MakeItemShotgunAmmo();
        case 2: return MakeItemLightRifleAmmo();
#if DEBUG
        case 3: return MakeItemBoltsAmmo();
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return MakeItemBoltsAmmo();
#endif
      }
    }

    private Item MakeHuntingShopItem()
    {
      if (m_DiceRoller.RollChance(50)) {
        if (m_DiceRoller.RollChance(40)) return (0 == m_DiceRoller.Roll(0, 2) ? MakeItemHuntingRifle() : MakeItemHuntingCrossbow());
        return (0 == m_DiceRoller.Roll(0, 2) ? MakeItemLightRifleAmmo() : MakeItemBoltsAmmo());
      }
      return (0 == m_DiceRoller.Roll(0, 2) ? (Item)MakeItemHunterVest() : (Item)MakeItemBearTrap());
    }

    private Item MakeShopGeneralItem()
    {
      switch (m_DiceRoller.Roll(0, 6)) {
        case 0: return MakeShopPharmacyItem();
        case 1: return MakeShopSportsWearItem();
        case 2: return MakeShopConstructionItem();
        case 3: return MakeShopGroceryItem();
        case 4: return MakeHuntingShopItem();
#if DEBUG
        case 5: return MakeRandomBedroomItem();
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return MakeRandomBedroomItem();
#endif
      }
    }

    private Item MakeHospitalItem()
    {
      switch (m_DiceRoller.Roll(0, (Session.Get.HasInfection ? 7 : 6))) {
        case 0: return MakeItemBandages();
        case 1: return MakeItemMedikit();
        case 2: return MakeItemPillsSLP();
        case 3: return MakeItemPillsSTA();
        case 4: return MakeItemPillsSAN();
        case 5: return MakeItemStenchKiller();
#if DEBUG
        case 6: return MakeItemPillsAntiviral();
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return MakeItemPillsAntiviral();
#endif
      }
    }

    private Item MakeRandomBedroomItem()
    {
      switch (m_DiceRoller.Roll(0, 24)) {
        case 0:
        case 1: return MakeItemBandages();
        case 2: return MakeItemPillsSTA();
        case 3: return MakeItemPillsSLP();
        case 4: return MakeItemPillsSAN();
        case 5:
        case 6:
        case 7:
        case 8: return MakeItemBaseballBat();
        case 9: return MakeItemRandomPistol();
        case 10:
          if (m_DiceRoller.RollChance(30)) return (m_DiceRoller.RollChance(50) ? MakeItemShotgun() : MakeItemHuntingRifle());
          return (m_DiceRoller.RollChance(50) ? MakeItemShotgunAmmo() : MakeItemLightRifleAmmo());
        case 11:
        case 12:
        case 13: return MakeItemCellPhone();
        case 14:
        case 15: return MakeItemFlashlight();
        case 16:
        case 17: return MakeItemLightPistolAmmo();
        case 18:
        case 19: return MakeItemStenchKiller();
        case 20: return MakeItemHunterVest();
#if DEBUG
        case 21:
        case 22:
        case 23: return (m_DiceRoller.RollChance(50) ? MakeItemBook() : MakeItemMagazines());
        default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
        default: return (m_DiceRoller.RollChance(50) ? MakeItemBook() : MakeItemMagazines());
#endif
      }
    }

    private ItemFood MakeRandomKitchenItem()
    {
      return (m_DiceRoller.RollChance(50) ? MakeItemCannedFood() : MakeItemGroceries());
    }

    public Item MakeRandomCHAROfficeItem()
    {
      switch (m_DiceRoller.Roll(0, 10))
      {
        case 0:
          if (m_DiceRoller.RollChance(10))
            return MakeItemGrenade();
          if (m_DiceRoller.RollChance(30))
            return MakeItemShotgun();
          return MakeItemShotgunAmmo();
        case 1:
        case 2:
          if (m_DiceRoller.RollChance(50))
            return MakeItemBandages();
          return MakeItemMedikit();
        case 3:
          return MakeItemCannedFood();
        case 4:
          if (!m_DiceRoller.RollChance(50))
            return (Item) null;
          if (m_DiceRoller.RollChance(50))
            return MakeItemZTracker();
          return MakeItemBlackOpsGPS();
        default:
          return (Item) null;
      }
    }

    public Item MakeRandomParkItem()
    {
      switch (m_DiceRoller.Roll(0, 8))
      {
        case 0: return MakeItemSprayPaint();
        case 1: return MakeItemBaseballBat();
        case 2: return MakeItemPillsSLP();
        case 3: return MakeItemPillsSTA();
        case 4: return MakeItemPillsSAN();
        case 5: return MakeItemFlashlight();
        case 6: return MakeItemCellPhone();
#if DEBUG
        case 7:
#else
        default:
#endif
          return MakeItemWoodenPlank();
#if DEBUG
        default: throw new ArgumentOutOfRangeException("unhandled item roll");
#endif
      }
    }

    protected virtual void DecorateOutsideWallsWithPosters(Map map, Rectangle rect, int chancePerWall)
    {
      DecorateOutsideWalls(map, rect, (x, y) => (m_DiceRoller.RollChance(chancePerWall) ? m_DiceRoller.Choose(POSTERS) : null));
    }

    protected virtual void DecorateOutsideWallsWithTags(Map map, Rectangle rect, int chancePerWall)
    {
      DecorateOutsideWalls(map, rect, (x, y) => (m_DiceRoller.RollChance(chancePerWall) ? m_DiceRoller.Choose(TAGS) : null));
    }

    // CHAR building codes have accounted for the possibility of a Z apocalypse.
    private static void _HouseBasementCornerBuildingCode(Map basement, Point basementStairs,Point corner, Point diag_step)
    {
      Point large = (basement.Rect.Bottom <= basement.Rect.Right ? new Point(corner.X,diag_step.Y) : new Point(diag_step.X,corner.Y));
      Point small = (basement.Rect.Bottom > basement.Rect.Right ? new Point(corner.X,diag_step.Y) : new Point(diag_step.X,corner.Y));
      if (   GameTiles.WALL_BRICK == basement.GetTileModelAt(large)
          && GameTiles.WALL_BRICK == basement.GetTileModelAt(small)) {
        if (corner==basementStairs) {
          // The Sokoban gate is required to work.
          basement.SetTileModelAt(diag_step.X, diag_step.Y, GameTiles.FLOOR_CONCRETE);
          Session.Get.PoliceInvestigate.Record(basement,diag_step);
          basement.SetTileModelAt(large.X, large.Y, GameTiles.FLOOR_CONCRETE);
          Session.Get.PoliceInvestigate.Record(basement,large);
        } else if (   GameTiles.WALL_BRICK == basement.GetTileModelAt(diag_step)
                   && GameTiles.FLOOR_CONCRETE == basement.GetTileModelAt(corner)) {
          basement.SetTileModelAt(corner.X, corner.Y, GameTiles.WALL_BRICK);
          basement.SetTileModelAt(diag_step.X, diag_step.Y, GameTiles.FLOOR_CONCRETE);
          Session.Get.PoliceInvestigate.Record(basement,diag_step);
          Session.Get.PoliceInvestigate.Seen(basement,corner);
        }
      }
    }

    private static bool _ForceHouseBasementConnected(Map basement,Point basementStairs)
    {
      // basement.Rect.Top and basement.Rect.Left are hardcoded 0
      // coordinates 0, width-1, height-1 are already brick walls
      // basic disconnects is, with two walls:
      // XXX
      // X.X
      // XXX
      _HouseBasementCornerBuildingCode(basement, basementStairs, new Point(1,1), new Point(2,2));
      _HouseBasementCornerBuildingCode(basement, basementStairs, new Point(1, basement.Rect.Bottom-2), new Point(2, basement.Rect.Bottom - 3));
      _HouseBasementCornerBuildingCode(basement, basementStairs, new Point(basement.Rect.Right - 2, 1), new Point(basement.Rect.Right - 3, 2));
      _HouseBasementCornerBuildingCode(basement, basementStairs, new Point(basement.Rect.Right - 2, basement.Rect.Bottom - 2), new Point(basement.Rect.Right - 3, basement.Rect.Bottom - 3));
#if FAIL
      HashSet<Point> tainted = Session.Get.PoliceInvestigate.In(basement);
      // 0<tainted.Count by construction
      // basementStairs not tainted by construction
      Zaimoni.Data.FloodfillPathfinder<Point> navigate = basement.PathfindSteps();   // assume an actor suitable for OrderableAI.  No actors yet so that simplifies things
      navigate.GoalDistance(basementStairs, tainted);   // may need another thin wrapper
      tainted.ExceptWith(navigate.Domain);
      if (0<tainted.Count) {
        // recovery code: find a brick wall that is adjacent to both unreachable and reachable squares, preferably 2+ uncreachable
        // reposition brick wall from into an unreachable square
        // redo
      }
#endif
      return true;
    }

    private Map GenerateHouseBasementMap(Map map, Block houseBlock)
    {
#if DEBUG
      if (null == map.District) throw new ArgumentNullException(nameof(map.District));
#endif
      Rectangle buildingRect = houseBlock.BuildingRect;
      Map basement = new Map(map.Seed << 1 + buildingRect.Left * map.Height + buildingRect.Top, string.Format("basement{0}{1}@{2}-{3}", (object)m_Params.District.WorldPosition.X, (object)m_Params.District.WorldPosition.Y, (object) (buildingRect.Left + buildingRect.Width / 2), (object) (buildingRect.Top + buildingRect.Height / 2)), map.District, buildingRect.Width, buildingRect.Height, Lighting.DARKNESS);
      basement.AddZone(MakeUniqueZone("basement", basement.Rect));
      TileFill(basement, GameTiles.FLOOR_CONCRETE, true);
      TileRectangle(basement, GameTiles.WALL_BRICK, new Rectangle(0, 0, basement.Width, basement.Height));
      var candidates = new List<Point>();
      buildingRect.DoForEach(pt => candidates.Add(pt), pt => map.GetTileModelAt(pt).IsWalkable && !map.HasMapObjectAt(pt));
      Point point = m_DiceRoller.Choose(candidates);
      Point basementStairs = new Point(point.X - buildingRect.Left, point.Y - buildingRect.Top);
      AddExit(map, point, basement, basementStairs, GameImages.DECO_STAIRS_DOWN, true);
      AddExit(basement, basementStairs, map, point, GameImages.DECO_STAIRS_UP, true);
      DoForEachTile(basement.Rect, (Action<Point>) (pt =>
      {
        Session.Get.PoliceInvestigate.Record(basement,pt);
        if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_PILAR_CHANCE) || pt == basementStairs) return;
        if (GameTiles.WALL_BRICK == basement.GetTileModelAt(pt)) return; // already wall
        // We are iterating all rows Y in each column X
        // XXX so if we end up disconnecting we find out vertically
        // basement.Rect.Top and basement.Rect.Left are hardcoded 0
        // coordinates 0, width-1, height-1 are already brick walls
        Session.Get.PoliceInvestigate.Seen(basement,pt);    // not so freak coincidence for pillars to be completely screened
        basement.SetTileModelAt(pt.X, pt.Y, GameTiles.WALL_BRICK);
      }));
      // Tourism will fail if not all targets are accessible from the exit.  Transposing should be safe here.
      while(!_ForceHouseBasementConnected(basement,basementStairs));
      MapObjectFill(basement, basement.Rect, (Func<Point, MapObject>) (pt =>
      {
        if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_OBJECT_CHANCE_PER_TILE)) return null;
        if (basement.HasExitAt(pt)) return null;
        if (!basement.IsWalkable(pt.X, pt.Y)) return null;
        switch (m_DiceRoller.Roll(0, 5)) {
          case 0:
            return MakeObjJunk();
          case 1:
            return MakeObjBarrels();
          case 2:
            basement.DropItemAt(MakeShopConstructionItem(), pt);
            return MakeObjTable(GameImages.OBJ_TABLE);
          case 3:
            basement.DropItemAt(MakeShopConstructionItem(), pt);
            return MakeObjDrawer();
#if DEBUG
          case 4:
#else
          default:
#endif
            return MakeObjBed(GameImages.OBJ_BED);
#if DEBUG
          default:
            throw new ArgumentOutOfRangeException("unhandled roll");
#endif
        }
      }));
      if (Session.Get.HasZombiesInBasements)
        DoForEachTile(basement.Rect, (Action<Point>) (pt =>
        {
          if (!basement.IsWalkable(pt.X, pt.Y) || basement.HasExitAt(pt) || !m_DiceRoller.RollChance(HOUSE_BASEMENT_ZOMBIE_RAT_CHANCE)) return;
          basement.PlaceAt(CreateNewBasementRatZombie(0), pt);
        }));
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_WEAPONS_CACHE_CHANCE))
        MapObjectPlaceInGoodPosition(basement, basement.Rect, (Func<Point, bool>) (pt => !basement.HasExitAt(pt) && basement.IsWalkable(pt) && (!basement.HasMapObjectAt(pt) && !basement.HasItemsAt(pt))), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        { // survivalist weapons cache.  Grenades were not acquired locally.  Guaranteed usable.
          basement.DropItemAt(MakeItemGrenade(), pt);
          basement.DropItemAt(MakeItemGrenade(), pt);
          // There will be a primary ranged weapon (with 2 ammo clips)
          // and a secondary ranged weapon (with one ammo clip)
          KeyValuePair<GameItems.IDs,GameItems.IDs> survivalist_cache_ranged = m_DiceRoller.Choose(survivalist_ranged_candidates);
          basement.DropItemAt(MakeRangedWeapon(survivalist_cache_ranged.Key), pt);
          basement.DropItemAt(MakeAmmo(survivalist_cache_ranged.Key), pt);
          basement.DropItemAt(MakeAmmo(survivalist_cache_ranged.Key), pt);
          basement.DropItemAt(MakeRangedWeapon(survivalist_cache_ranged.Value), pt);
          basement.DropItemAt(MakeAmmo(survivalist_cache_ranged.Value), pt);
          Session.Get.PoliceInvestigate.Record(basement, pt);
          return MakeObjShelf();
        }));
      return basement;
    }

    public Map GenerateUniqueMap_CHARUnderground(Map surfaceMap, Zone officeZone)
    {
#if DEBUG
      if (null == surfaceMap) throw new ArgumentNullException(nameof(surfaceMap));
#endif
      Map underground = new Map(surfaceMap.Seed << 3 ^ surfaceMap.Seed, string.Format("CHAR Underground Facility @{0}-{1}", surfaceMap.District.WorldPosition.X, surfaceMap.District.WorldPosition.Y), surfaceMap.District, 100, 100, Lighting.DARKNESS, true);
      TileFill(underground, GameTiles.FLOOR_OFFICE, true);
      TileRectangle(underground, GameTiles.WALL_CHAR_OFFICE, new Rectangle(0, 0, underground.Width, underground.Height));
      Zone zone1 = null;
      Point point1 = new Point();
      bool flag = false;
      do {
        // We do not want to evaluate this for each point in the office
        do {
          int x = m_DiceRoller.Roll(officeZone.Bounds.Left, officeZone.Bounds.Right);
          int y = m_DiceRoller.Roll(officeZone.Bounds.Top, officeZone.Bounds.Bottom);
          List<Zone> zonesAt = surfaceMap.GetZonesAt(x, y);
          if (0 < (zonesAt?.Count ?? 0)) {
            foreach (Zone zone2 in zonesAt) {
              if (zone2.Name.Contains("room")) {
                zone1 = zone2;
                break;
              }
            }
          }
        }
        while (zone1 == null);
        var candidates = new List<Point>();
        zone1.Bounds.DoForEach(pt => candidates.Add(pt),pt => surfaceMap.IsWalkable(pt));
        if (0 >= candidates.Count) continue;
        point1 = m_DiceRoller.Choose(candidates);
        flag = true;
      }
      while (!flag);
      DoForEachTile(zone1.Bounds, (Action<Point>) (pt =>
      {
        if (!(surfaceMap.GetMapObjectAt(pt) is DoorWindow)) return;
        surfaceMap.RemoveMapObjectAt(pt.X, pt.Y);
        DoorWindow doorWindow = MakeObjIronDoor();
        doorWindow.Barricade(Rules.BARRICADING_MAX);
        surfaceMap.PlaceAt(doorWindow, pt);
      }));
      Point point2 = new Point(underground.Width / 2, underground.Height / 2);
      AddExit(underground, point2, surfaceMap, point1, GameImages.DECO_STAIRS_UP, true);
      AddExit(surfaceMap, point1, underground, point2, GameImages.DECO_STAIRS_DOWN, true);
      underground.ForEachAdjacent(point2, (Action<Point>) (pt => underground.AddDecorationAt(GameImages.DECO_CHAR_FLOOR_LOGO, pt)));
      Rectangle rect1 = Rectangle.FromLTRB(0, 0, underground.Width / 2 - 1, underground.Height / 2 - 1);
      Rectangle rect2 = Rectangle.FromLTRB(underground.Width / 2 + 1 + 1, 0, underground.Width, rect1.Bottom);
      Rectangle rect3 = Rectangle.FromLTRB(0, underground.Height / 2 + 1 + 1, rect1.Right, underground.Height);
      Rectangle rect4 = Rectangle.FromLTRB(rect2.Left, rect3.Top, underground.Width, underground.Height);
      List<Rectangle> list = new List<Rectangle>();
      MakeRoomsPlan(underground, ref list, rect3, 6);
      MakeRoomsPlan(underground, ref list, rect4, 6);
      MakeRoomsPlan(underground, ref list, rect1, 6);
      MakeRoomsPlan(underground, ref list, rect2, 6);
      foreach (Rectangle rect5 in list)
        TileRectangle(underground, GameTiles.WALL_CHAR_OFFICE, rect5);
      foreach (Rectangle rectangle in list) {
        Point position1 = rectangle.Left < underground.Width / 2 ? new Point(rectangle.Right - 1, rectangle.Top + rectangle.Height / 2) : new Point(rectangle.Left, rectangle.Top + rectangle.Height / 2);
        if (!underground.HasMapObjectAt(position1)) {
          PlaceDoorIfAccessibleAndNotAdjacent(underground, position1.X, position1.Y, GameTiles.FLOOR_OFFICE, 6, MakeObjCharDoor());
        }
        Point position2 = rectangle.Top < underground.Height / 2 ? new Point(rectangle.Left + rectangle.Width / 2, rectangle.Bottom - 1) : new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top);
        if (!underground.HasMapObjectAt(position2)) {
          PlaceDoorIfAccessibleAndNotAdjacent(underground, position2.X, position2.Y, GameTiles.FLOOR_OFFICE, 6, MakeObjCharDoor());
        }
      }
      for (int right = rect1.Right; right < rect4.Left; ++right) {
        PlaceDoor(underground, right, rect1.Bottom - 1, GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
        PlaceDoor(underground, right, rect3.Top, GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
      }
      for (int bottom = rect1.Bottom; bottom < rect3.Top; ++bottom) {
        PlaceDoor(underground, rect1.Right - 1, bottom, GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
        PlaceDoor(underground, rect2.Left, bottom, GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
      }
      foreach (Rectangle wallsRect in list)
      {
        Rectangle rectangle = new Rectangle(wallsRect.Left + 1, wallsRect.Top + 1, wallsRect.Width - 2, wallsRect.Height - 2);
        string basename;
        if (wallsRect.Left == 0 && wallsRect.Top == 0 || wallsRect.Left == 0 && wallsRect.Bottom == underground.Height || wallsRect.Right == underground.Width && wallsRect.Top == 0 || wallsRect.Right == underground.Width && wallsRect.Bottom == underground.Height)
        {
          basename = "Power Room";
          MakeCHARPowerRoom(underground, wallsRect, rectangle);
        } else {
          switch (wallsRect.Left >= underground.Width / 2 || wallsRect.Top >= underground.Height / 2 ? (wallsRect.Left < underground.Width / 2 || wallsRect.Top >= underground.Height / 2 ? (wallsRect.Left >= underground.Width / 2 || wallsRect.Top < underground.Height / 2 ? 3 : 2) : 1) : 0)
          {
            case 0:
              basename = "Armory";
              MakeCHARArmoryRoom(underground, rectangle);
              break;
            case 1:
              basename = "Storage";
              MakeCHARStorageRoom(underground, rectangle);
              break;
            case 2:
              basename = "Living";
              MakeCHARLivingRoom(underground, rectangle);
              break;
            case 3:
              basename = "Pharmacy";
              MakeCHARPharmacyRoom(underground, rectangle);
              break;
            default:
              throw new ArgumentOutOfRangeException("unhandled role");
          }
        }
        underground.AddZone(MakeUniqueZone(basename, rectangle));
      }
      for (int x = 0; x < underground.Width; ++x) {
        for (int y = 0; y < underground.Height; ++y) {
          if (m_DiceRoller.RollChance(25)) {
            Tile tileAt = underground.GetTileAt(x, y);
            if (tileAt.Model.IsWalkable) continue;
            tileAt.AddDecoration(m_DiceRoller.Choose(CHAR_POSTERS));
          }
          if (m_DiceRoller.RollChance(20)) {
            Tile tileAt = underground.GetTileAt(x, y);
            tileAt.AddDecoration(tileAt.Model.IsWalkable ? GameImages.DECO_BLOODIED_FLOOR : GameImages.DECO_BLOODIED_WALL);
          }
        }
      }
      Predicate<Point> actor_ok_here = pt => !underground.HasExitAt(pt);
      int width = underground.Width;
      for (int index1 = 0; index1 < width; ++index1) {
        Actor newUndead = CreateNewUndead(0);
        while (true) {
          GameActors.IDs index2 = newUndead.Model.ID.NextUndeadEvolution();
          if (index2 == newUndead.Model.ID) break;
          newUndead.Model = m_Game.GameActors[index2];
        }
        ActorPlace(m_DiceRoller, underground, newUndead, actor_ok_here);
      }
      int num1 = underground.Width / 10;
      for (int index = 0; index < num1; ++index) {
        Actor newCharGuard = CreateNewCHARGuard(0);
        ActorPlace(m_DiceRoller, underground, newCharGuard, actor_ok_here);
      }
      return underground;
    }

    private void MakeCHARArmoryRoom(Map map, Rectangle roomRect)
    {
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        if (map.HasExitAt(pt)) return null;
        if (!m_DiceRoller.RollChance(20)) return null;
        map.DropItemAt(!m_DiceRoller.RollChance(20) ? (!m_DiceRoller.RollChance(20) ? (!m_DiceRoller.RollChance(20) ? (!m_DiceRoller.RollChance(30) ? (Item)(m_DiceRoller.RollChance(50) ? MakeItemShotgunAmmo() : MakeItemLightRifleAmmo()) : (Item)(m_DiceRoller.RollChance(50) ? MakeItemShotgun() : MakeItemHuntingRifle())) : (Item)MakeItemGrenade()) : (Item)(m_DiceRoller.RollChance(50) ? MakeItemZTracker() : MakeItemBlackOpsGPS())) : (Item)MakeItemCHARLightBodyArmor(), pt);
        return MakeObjShelf();
      }));
    }

    private void MakeCHARStorageRoom(Map map, Rectangle roomRect)
    {
      TileFill(map, GameTiles.FLOOR_CONCRETE, roomRect);
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) > 0) return null;
        if (map.HasExitAt(pt)) return null;
        if (!m_DiceRoller.RollChance(50)) return null;
        return (m_DiceRoller.RollChance(50) ? MakeObjJunk() : MakeObjBarrels());
      }));
      for (int left = roomRect.Left; left < roomRect.Right; ++left) {
        for (int top = roomRect.Top; top < roomRect.Bottom; ++top) {
          if (CountAdjWalls(map, left, top) <= 0 && !map.HasMapObjectAt(left, top))
            map.DropItemAt(MakeShopConstructionItem(), left, top);
        }
      }
    }

    private void MakeCHARLivingRoom(Map map, Rectangle roomRect)
    {
      TileFill(map, GameTiles.FLOOR_PLANKS, roomRect, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO)));
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        if (map.HasExitAt(pt)) return null;
        if (!m_DiceRoller.RollChance(30)) return null;
        if (m_DiceRoller.RollChance(50)) return MakeObjBed(GameImages.OBJ_BED);
        return MakeObjFridge();
      }));
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) > 0) return null;
        if (map.HasExitAt(pt)) return null;
        if (!m_DiceRoller.RollChance(30)) return null;
        if (!m_DiceRoller.RollChance(30)) return MakeObjChair(GameImages.OBJ_CHAR_CHAIR);
        MapObject mapObject = MakeObjTable(GameImages.OBJ_CHAR_TABLE);
        map.DropItemAt(MakeItemCannedFood(), pt);
        return mapObject;
      }));
    }

    private void MakeCHARPharmacyRoom(Map map, Rectangle roomRect)
    {
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        if (map.HasExitAt(pt)) return null;
        if (!m_DiceRoller.RollChance(20)) return null;
        map.DropItemAt(MakeHospitalItem(), pt);
        return MakeObjShelf();
      }));
    }

    private static void MakeCHARPowerRoom(Map map, Rectangle wallsRect, Rectangle roomRect)
    {
      TileFill(map, GameTiles.FLOOR_CONCRETE, roomRect);
      DoForEachTile(wallsRect, (Action<Point>) (pt =>
      {
        if (!(map.GetMapObjectAt(pt) is DoorWindow)) return;
        map.ForEachAdjacent(pt, (Action<Point>) (ptAdj =>
        {
          Tile tileAt = map.GetTileAt(ptAdj);
          if (tileAt.Model.IsWalkable) return;
          tileAt.RemoveAllDecorations();
          tileAt.AddDecoration(GameImages.DECO_POWER_SIGN_BIG);
        }));
      }));
      DoForEachTile(roomRect, (Action<Point>) (pt =>
      {
        if (!map.GetTileModelAt(pt).IsWalkable || map.HasExitAt(pt) || CountAdjWalls(map, pt.X, pt.Y) < 3) return;
        map.PlaceAt(MakeObjPowerGenerator(), pt);
      }));
    }

    private void MakePoliceStation(Map map, List<Block> freeBlocks, out Block policeBlock)
    {
      policeBlock = m_DiceRoller.Choose(freeBlocks);
      GeneratePoliceStation(map, policeBlock, out Point stairsToLevel1);
      Map stationOfficesLevel = GeneratePoliceStation_OfficesLevel(map);
      Map stationJailsLevel = GeneratePoliceStation_JailsLevel(stationOfficesLevel);
      AddExit(map, stairsToLevel1, stationOfficesLevel, new Point(1, 1), GameImages.DECO_STAIRS_DOWN, true);
      AddExit(stationOfficesLevel, new Point(1, 1), map, stairsToLevel1, GameImages.DECO_STAIRS_UP, true);
      AddExit(stationOfficesLevel, new Point(1, stationOfficesLevel.Height - 2), stationJailsLevel, new Point(1, 1), GameImages.DECO_STAIRS_DOWN, true);
      AddExit(stationJailsLevel, new Point(1, 1), stationOfficesLevel, new Point(1, stationOfficesLevel.Height - 2), GameImages.DECO_STAIRS_UP, true);
      m_Params.District.AddUniqueMap(stationOfficesLevel);
      m_Params.District.AddUniqueMap(stationJailsLevel);
      Session.Get.UniqueMaps.PoliceStation_OfficesLevel = new UniqueMap(stationOfficesLevel);
      Session.Get.UniqueMaps.PoliceStation_JailsLevel = new UniqueMap(stationJailsLevel);
    }

    private void GeneratePoliceStation(Map surfaceMap, Block policeBlock, out Point stairsToLevel1)
    {
      TileFill(surfaceMap, GameTiles.FLOOR_TILES, policeBlock.InsideRect, true);
      TileRectangle(surfaceMap, GameTiles.WALL_POLICE_STATION, policeBlock.BuildingRect);
      TileRectangle(surfaceMap, GameTiles.FLOOR_WALKWAY, policeBlock.Rectangle);
      DoForEachTile(policeBlock.InsideRect,pt => Session.Get.ForcePoliceKnown(new Location(surfaceMap, pt)));
      Point point = new Point(policeBlock.BuildingRect.Left + policeBlock.BuildingRect.Width / 2, policeBlock.BuildingRect.Bottom - 1);
      surfaceMap.AddDecorationAt(GameImages.DECO_POLICE_STATION, point.X - 1, point.Y);
      surfaceMap.AddDecorationAt(GameImages.DECO_POLICE_STATION, point.X + 1, point.Y);
      surfaceMap.AddZone(new Zone("NoCivSpawn", new Rectangle(policeBlock.BuildingRect.Left,policeBlock.BuildingRect.Top,policeBlock.BuildingRect.Width,3)));  // once the power locks go in civilians won't be able to path here
      Rectangle rect = Rectangle.FromLTRB(policeBlock.BuildingRect.Left, policeBlock.BuildingRect.Top + 2, policeBlock.BuildingRect.Right, policeBlock.BuildingRect.Bottom);
      TileRectangle(surfaceMap, GameTiles.WALL_POLICE_STATION, rect);
      PlaceDoor(surfaceMap, rect.Left + rect.Width / 2, rect.Top, GameTiles.FLOOR_TILES, MakeObjIronDoor());
      PlaceDoor(surfaceMap, point.X, point.Y, GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      DoForEachTile(rect, (Action<Point>) (pt =>
      {
        if (!surfaceMap.IsWalkable(pt.X, pt.Y) || CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)
          return;
        surfaceMap.PlaceAt(MakeObjBench(), pt);
      }));
      stairsToLevel1 = new Point(point.X, policeBlock.InsideRect.Top);
      surfaceMap.AddZone(MakeUniqueZone("Police Station", policeBlock.BuildingRect));
      MakeWalkwayZones(surfaceMap, policeBlock);
    }

    private Map GeneratePoliceStation_OfficesLevel(Map surfaceMap)
    {
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Offices", surfaceMap.District, 20, 20, Lighting.LIT);

      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_POLICE_STATION, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
      List<Rectangle> list = new List<Rectangle>();
      // XXX to maximize supplies: need to roll 9-right-wdith on the first horizonal split
      // XXX while this permits 4 rooms vertically, access will be flaky...probably better to have 3
      MakeRoomsPlan(map, ref list, rect1, 5);

      Func<Item> stock_armory = () => {
        switch (m_DiceRoller.Roll(0, 10)) {
          case 0:
          case 1: return m_DiceRoller.RollChance(50) ? MakeItemPoliceJacket() : MakeItemPoliceRiotArmor();
          case 2:
          case 3: return  m_DiceRoller.RollChance(50) ? (Item)(m_DiceRoller.RollChance(50) ? MakeItemFlashlight() : MakeItemBigFlashlight()) : (Item)MakeItemPoliceRadio();
          case 4:
          case 5: return MakeItemTruncheon();
          case 6:
          case 7: return m_DiceRoller.RollChance(30) ? (Item)MakeItemPistol() : (Item)MakeItemLightPistolAmmo();
          case 8:
          case 9: return m_DiceRoller.RollChance(30) ? (Item)MakeItemShotgun() : (Item)MakeItemShotgunAmmo();
          default: throw new ArgumentOutOfRangeException("unhandled roll");
        }
      };

      foreach (Rectangle rect2 in list) {
        Rectangle rect3 = Rectangle.FromLTRB(rect2.Left + 1, rect2.Top + 1, rect2.Right - 1, rect2.Bottom - 1);
        if (rect2.Right == map.Width) {
          TileRectangle(map, GameTiles.WALL_POLICE_STATION, rect2);
          PlaceDoor(map, rect2.Left, rect2.Top + rect2.Height / 2, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
          DoForEachTile(rect3, pt => {
            if (!map.IsWalkable(pt.X, pt.Y) || CountAdjWalls(map, pt.X, pt.Y) == 0 || CountAdjDoors(map, pt.X, pt.Y) > 0) return;
            map.PlaceAt(MakeObjShelf(), pt);
            map.DropItemAt(stock_armory(), pt);
          });
          map.AddZone(MakeUniqueZone("security", rect3));
          continue;
        }
        // \todo try to leave a non-jumping path to the doors
        TileFill(map, GameTiles.FLOOR_PLANKS, rect2);
        TileRectangle(map, GameTiles.WALL_POLICE_STATION, rect2);
        PlaceDoor(map, rect2.Left, rect2.Top + rect2.Height / 2, GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());    // \todo if this door is on the main hallway (x coordinate 3) need to exclude fleeing prisoners
        // top-left room has generator rather than furniture.  At Day 0 turn 0 it is on for backstory and gameplay reasons.
        if (0 == rect2.Top && 3 == rect2.Left) {
          PowerGenerator power = MakeObjPowerGenerator();
          power.TogglePower();
          map.PlaceAt(power, new Point(6,1)); // close, but not so close that using it keeps the door from auto-locking
          continue;
        }
        // \todo genenrator goes in the office with left-top 3,0
        MapObjectPlaceInGoodPosition(map, rect3, pt => {
          return map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }, m_DiceRoller, pt => MakeObjTable(GameImages.OBJ_TABLE));
        MapObjectPlaceInGoodPosition(map, rect3, pt => {
          return map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }, m_DiceRoller, pt => MakeObjChair(GameImages.OBJ_CHAIR));
        MapObjectPlaceInGoodPosition(map, rect3, pt => {
          return map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }, m_DiceRoller, pt => MakeObjChair(GameImages.OBJ_CHAIR));
        map.AddZone(MakeUniqueZone("office", rect3));
      }
      DoForEachTile(new Rectangle(1, 1, 1, map.Height - 2), pt => {
        if (pt.Y % 2 == 1 || !map.IsWalkable(pt) || CountAdjWalls(map, pt) != 3) return;
        map.PlaceAt(MakeObjIronBench(), pt);
      });
      for (int index = 0; index < 5; ++index) {
        ActorPlace(m_DiceRoller, map, CreateNewPoliceman(0));
      }
      DoForEachTile(map.Rect, pt => { Session.Get.ForcePoliceKnown(new Location(map, pt)); });
      return map;
    }

    private Map GeneratePoliceStation_JailsLevel(Map surfaceMap)
    {
      const int JAILS_WIDTH = 22;
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Jails", surfaceMap.District, JAILS_WIDTH, 6, Lighting.LIT);
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_POLICE_STATION, map.Rect);
      List<Rectangle> rectangleList = new List<Rectangle>();
      int x = 0;
      while (x + 2 < map.Width) {
        Rectangle rect = new Rectangle(x, 3, 3, 3);
        rectangleList.Add(rect);
        TileFill(map, GameTiles.FLOOR_CONCRETE, rect);
        TileRectangle(map, GameTiles.WALL_POLICE_STATION, rect);
        map.PlaceAt(MakeObjIronBench(), new Point(x + 1, 4));
        Point position2 = new Point(x + 1, 3);
        map.SetTileModelAt(position2.X, position2.Y, GameTiles.FLOOR_CONCRETE);
        map.PlaceAt(MakeObjIronGate(), position2);
        map.AddZone(MakeUniqueZone("jail", rect));
        x += 2;
      }
      Rectangle rect1 = Rectangle.FromLTRB(1, 1, map.Width, 3);
      map.AddZone(MakeUniqueZone("cells corridor", rect1));
      map.PlaceAt(MakeObjPowerGenerator(), new Point(map.Width - 2, 1));

      foreach(Rectangle r in rectangleList) {
        Point dest = new Point(r.Left + 1, r.Top + 1);
        Actor newCivilian = CreateNewCivilian(0, 0, 1);
        if (JAILS_WIDTH-3 == dest.X) {
          // a political prisoner
          newCivilian.Name = "The Prisoner Who Should Not Be";
          for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
            newCivilian.Inventory.AddAll(MakeItemArmyRation());
          Session.Get.UniqueActors.PoliceStationPrisonner = new UniqueActor(newCivilian,true);
        } else {
          // being held with cause, at least as understood before the z-apocalypse
          newCivilian.Inventory.AddAll(MakeItemGroceries());
        }
        map.PlaceAt(newCivilian, dest);
      }
#if FAIL
      for (int index = 0; index < rectangleList.Count - 1; ++index) {   // this loop stops before The Prisoner Who Should Not Be (map::PlaceActorAt would hard-error otherwise)
        Rectangle rectangle = rectangleList[index];
        Actor newCivilian = CreateNewCivilian(0, 0, 1);
        newCivilian.Inventory.AddAll(MakeItemGroceries());
        // XXX \todo give these civilians the PathTo (outside the police station) objective
        map.PlaceActorAt(newCivilian, new Point(rectangle.Left + 1, rectangle.Top + 1));
      }
      Rectangle rectangle1 = rectangleList[rectangleList.Count - 1];
      Actor newCivilian1 = CreateNewCivilian(0, 0, 1);
      newCivilian1.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian1.Inventory.MaxCapacity; ++index)
        newCivilian1.Inventory.AddAll(MakeItemArmyRation());
      map.PlaceActorAt(newCivilian1, new Point(rectangle1.Left + 1, rectangle1.Top + 1));
      Session.Get.UniqueActors.PoliceStationPrisonner = new UniqueActor(newCivilian1,true);
#endif
      DoForEachTile(map.Rect, pt => Session.Get.ForcePoliceKnown(new Location(map, pt)));
      return map;
    }

    private void MakeHospital(Map map, List<Block> freeBlocks, out Block hospitalBlock)
    {
#if DEBUG
      if (null == map.District) throw new ArgumentNullException(nameof(map.District));
#endif
      hospitalBlock = m_DiceRoller.Choose(freeBlocks);
      GenerateHospitalEntryHall(map, hospitalBlock);
      Map hospitalAdmissions = GenerateHospital_Admissions(map.Seed << 1 ^ map.Seed, map.District);
      Map hospitalOffices = GenerateHospital_Offices(map.Seed << 2 ^ map.Seed, map.District);
      Map hospitalPatients = GenerateHospital_Patients(map.Seed << 3 ^ map.Seed, map.District);
      Map hospitalStorage = GenerateHospital_Storage(map.Seed << 4 ^ map.Seed, map.District);
      Map hospitalPower = GenerateHospital_Power(map.Seed << 5 ^ map.Seed, map.District);
      Point point1 = new Point(hospitalBlock.InsideRect.Left + hospitalBlock.InsideRect.Width / 2, hospitalBlock.InsideRect.Top);
      Point point2 = new Point(hospitalAdmissions.Width / 2, 1);
      AddExit(map, point1, hospitalAdmissions, point2, GameImages.DECO_STAIRS_DOWN, true);
      AddExit(hospitalAdmissions, point2, map, point1, GameImages.DECO_STAIRS_UP, true);
      Point point3 = new Point(hospitalAdmissions.Width / 2, hospitalAdmissions.Height - 2);
      Point point4 = new Point(hospitalOffices.Width / 2, 1);
      AddExit(hospitalAdmissions, point3, hospitalOffices, point4, GameImages.DECO_STAIRS_DOWN, true);
      AddExit(hospitalOffices, point4, hospitalAdmissions, point3, GameImages.DECO_STAIRS_UP, true);
      Point point5 = new Point(hospitalOffices.Width / 2, hospitalOffices.Height - 2);
      Point point6 = new Point(hospitalPatients.Width / 2, 1);
      AddExit(hospitalOffices, point5, hospitalPatients, point6, GameImages.DECO_STAIRS_DOWN, true);
      AddExit(hospitalPatients, point6, hospitalOffices, point5, GameImages.DECO_STAIRS_UP, true);
      Point point7 = new Point(hospitalPatients.Width / 2, hospitalPatients.Height - 2);
      Point point8 = new Point(1, 1);
      AddExit(hospitalPatients, point7, hospitalStorage, point8, GameImages.DECO_STAIRS_DOWN, true);
      AddExit(hospitalStorage, point8, hospitalPatients, point7, GameImages.DECO_STAIRS_UP, true);
      Point point9 = new Point(hospitalStorage.Width - 2, 1);
      Point point10 = new Point(1, 1);
      AddExit(hospitalStorage, point9, hospitalPower, point10, GameImages.DECO_STAIRS_DOWN, true);
      AddExit(hospitalPower, point10, hospitalStorage, point9, GameImages.DECO_STAIRS_UP, true);
      m_Params.District.AddUniqueMap(hospitalAdmissions);
      m_Params.District.AddUniqueMap(hospitalOffices);
      m_Params.District.AddUniqueMap(hospitalPatients);
      m_Params.District.AddUniqueMap(hospitalStorage);
      m_Params.District.AddUniqueMap(hospitalPower);
      Session.Get.UniqueMaps.Hospital_Admissions = new UniqueMap(hospitalAdmissions);
      Session.Get.UniqueMaps.Hospital_Offices = new UniqueMap(hospitalOffices);
      Session.Get.UniqueMaps.Hospital_Patients = new UniqueMap(hospitalPatients);
      Session.Get.UniqueMaps.Hospital_Storage = new UniqueMap(hospitalStorage);
      Session.Get.UniqueMaps.Hospital_Power = new UniqueMap(hospitalPower);
    }

    private void GenerateHospitalEntryHall(Map surfaceMap, Block block)
    {
      TileFill(surfaceMap, GameTiles.FLOOR_TILES, block.InsideRect, true);
      TileRectangle(surfaceMap, GameTiles.WALL_HOSPITAL, block.BuildingRect);
      TileRectangle(surfaceMap, GameTiles.FLOOR_WALKWAY, block.Rectangle);
      Point point1 = new Point(block.BuildingRect.Left + block.BuildingRect.Width / 2, block.BuildingRect.Bottom - 1);
      Point point2 = new Point(point1.X - 1, point1.Y);
      surfaceMap.AddDecorationAt(GameImages.DECO_HOSPITAL, point2.X - 1, point2.Y);
      surfaceMap.AddDecorationAt(GameImages.DECO_HOSPITAL, point1.X + 1, point1.Y);
      Rectangle rect = Rectangle.FromLTRB(block.BuildingRect.Left, block.BuildingRect.Top, block.BuildingRect.Right, block.BuildingRect.Bottom);
      PlaceDoor(surfaceMap, point1.X, point1.Y, GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      PlaceDoor(surfaceMap, point2.X, point2.Y, GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      DoForEachTile(rect, (Action<Point>) (pt =>
      {
        if (pt.Y == block.InsideRect.Top || (pt.Y == block.InsideRect.Bottom - 1 || !surfaceMap.IsWalkable(pt.X, pt.Y) || (CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)))
          return;
        surfaceMap.PlaceAt(MakeObjIronBench(), pt);
      }));
      surfaceMap.AddZone(MakeUniqueZone("Hospital", block.BuildingRect));
      MakeWalkwayZones(surfaceMap, block);
    }

    private Map GenerateHospital_Admissions(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Admissions", d, 13, 33, Lighting.DARKNESS);
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, 5, map.Height);
      int y1 = 0;
      while (y1 <= map.Height - 5) {
        Rectangle room = new Rectangle(rectangle1.Left, y1, 5, 5);
        MakeHospitalPatientRoom(map, "patient room", room, true);
        y1 += 4;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - 5, 0, 5, map.Height);
      int y2 = 0;
      while (y2 <= map.Height - 5) {
        Rectangle room = new Rectangle(rectangle2.Left, y2, 5, 5);
        MakeHospitalPatientRoom(map, "patient room", room, false);
        y2 += 4;
      }
      for (int index = 0; index < 10; ++index) {
        Actor newHospitalPatient = CreateNewHospitalPatient(0);
        ActorPlace(m_DiceRoller, map, newHospitalPatient, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "patient room")));
      }
      Predicate<Point> in_corridor = (pt => map.HasZonePartiallyNamedAt(pt, "corridor"));
      for (int index = 0; index < 4; ++index) {
        Actor newHospitalNurse = CreateNewHospitalNurse(0);
        ActorPlace(m_DiceRoller, map, newHospitalNurse, in_corridor);
      }
      for (int index = 0; index < 1; ++index) {
        Actor newHospitalDoctor = CreateNewHospitalDoctor(0);
        ActorPlace(m_DiceRoller, map, newHospitalDoctor, in_corridor);
      }
      return map;
    }

    private Map GenerateHospital_Offices(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Offices", d, 13, 33, Lighting.DARKNESS);
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, 5, map.Height);
      int y1 = 0;
      while (y1 <= map.Height - 5) {
        Rectangle room = new Rectangle(rectangle1.Left, y1, 5, 5);
        MakeHospitalOfficeRoom(map, "office", room, true);
        y1 += 4;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - 5, 0, 5, map.Height);
      int y2 = 0;
      while (y2 <= map.Height - 5) {
        Rectangle room = new Rectangle(rectangle2.Left, y2, 5, 5);
        MakeHospitalOfficeRoom(map, "office", room, false);
        y2 += 4;
      }
      Predicate<Point> in_office = (pt => map.HasZonePartiallyNamedAt(pt, "office"));
      for (int index = 0; index < 5; ++index) {
        Actor newHospitalNurse = CreateNewHospitalNurse(0);
        ActorPlace(m_DiceRoller, map, newHospitalNurse, in_office);
      }
      for (int index = 0; index < 2; ++index) {
        Actor newHospitalDoctor = CreateNewHospitalDoctor(0);
        ActorPlace(m_DiceRoller, map, newHospitalDoctor, in_office);
      }
      return map;
    }

    private Map GenerateHospital_Patients(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Patients", d, 13, 49, Lighting.DARKNESS);
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, 5, map.Height);
      int y1 = 0;
      while (y1 <= map.Height - 5) {
        Rectangle room = new Rectangle(rectangle1.Left, y1, 5, 5);
        MakeHospitalPatientRoom(map, "patient room", room, true);
        y1 += 4;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - 5, 0, 5, map.Height);
      int y2 = 0;
      while (y2 <= map.Height - 5) {
        Rectangle room = new Rectangle(rectangle2.Left, y2, 5, 5);
        MakeHospitalPatientRoom(map, "patient room", room, false);
        y2 += 4;
      }
      for (int index = 0; index < 20; ++index) {
        Actor newHospitalPatient = CreateNewHospitalPatient(0);
        ActorPlace(m_DiceRoller, map, newHospitalPatient, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "patient room")));
      }
      Predicate<Point> in_corridor = (pt => map.HasZonePartiallyNamedAt(pt, "corridor"));
      for (int index = 0; index < 8; ++index) {
        Actor newHospitalNurse = CreateNewHospitalNurse(0);
        ActorPlace(m_DiceRoller, map, newHospitalNurse, in_corridor);
      }
      for (int index = 0; index < 2; ++index) {
        Actor newHospitalDoctor = CreateNewHospitalDoctor(0);
        ActorPlace(m_DiceRoller, map, newHospitalDoctor, in_corridor);
      }
      return map;
    }

    private Map GenerateHospital_Storage(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Storage", d, 51, 16, Lighting.DARKNESS);
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(0, 0, map.Width, 4);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect1);
      map.AddZone(MakeUniqueZone("north corridor", rect1));
      Rectangle rect2 = Rectangle.FromLTRB(0, rect1.Bottom - 1, map.Width, rect1.Bottom - 1 + 4);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect2);
      map.SetTileModelAt(1, rect2.Top, GameTiles.FLOOR_TILES);
      map.PlaceAt(MakeObjIronGate(), new Point(1, rect2.Top));
      map.AddZone(MakeUniqueZone("central corridor", rect2));
      Rectangle rectangle1 = new Rectangle(2, rect2.Bottom - 1, map.Width - 2, 4);
      int left1 = rectangle1.Left;
      while (left1 <= map.Width - 5) {
        Rectangle room = new Rectangle(left1, rectangle1.Top, 5, 4);
        MakeHospitalStorageRoom(map, "storage", room);
        left1 += 4;
      }
      map.SetTileModelAt(1, rectangle1.Top, GameTiles.FLOOR_TILES);
      Rectangle rect3 = Rectangle.FromLTRB(0, rectangle1.Bottom - 1, map.Width, rectangle1.Bottom - 1 + 4);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect3);
      map.SetTileModelAt(1, rect3.Top, GameTiles.FLOOR_TILES);
      map.AddZone(MakeUniqueZone("south corridor", rect3));
      Rectangle rectangle2 = new Rectangle(2, rect3.Bottom - 1, map.Width - 2, 4);
      int left2 = rectangle2.Left;
      while (left2 <= map.Width - 5) {
        Rectangle room = new Rectangle(left2, rectangle2.Top, 5, 4);
        MakeHospitalStorageRoom(map, "storage", room);
        left2 += 4;
      }
      map.SetTileModelAt(1, rectangle2.Top, GameTiles.FLOOR_TILES);
      return map;
    }

    private static Map GenerateHospital_Power(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Power", d, 10, 10, Lighting.DARKNESS);
      TileFill(map, GameTiles.FLOOR_CONCRETE, true);
      TileRectangle(map, GameTiles.WALL_BRICK, map.Rect);
      Rectangle rect = Rectangle.FromLTRB(1, 1, 3, map.Height);
      map.AddZone(MakeUniqueZone("corridor", rect));
      for (int y = 1; y < map.Height - 2; ++y)
        map.PlaceAt(MakeObjIronFence(), new Point(2, y));
      Rectangle room = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
      map.AddZone(MakeUniqueZone("power room", room));
      DoForEachTile(room, (Action<Point>) (pt =>
      {
        if (pt.X == room.Left || !map.IsWalkable(pt) || CountAdjWalls(map, pt) < 3) return;
        map.PlaceAt(MakeObjPowerGenerator(), pt);
      }));
      Actor named = GameActors.JasonMyers.CreateNamed(GameFactions.ThePsychopaths, "Jason Myers", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_JASON_MYERS);
      named.StartingSkill(Skills.IDs.TOUGH,3);
      named.StartingSkill(Skills.IDs.STRONG,3);
      named.StartingSkill(Skills.IDs._FIRST,3);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,3);
      named.Inventory.AddAll(MakeItemJasonMyersAxe());
      map.PlaceAt(named, new Point(map.Width / 2, map.Height / 2));
      Session.Get.UniqueActors.JasonMyers = new UniqueActor(named,true,false,GameMusics.INSANE);
      return map;
    }

    private Actor CreateNewHospitalPatient(int spawnTime)
    {
      Actor numberedName = (m_DiceRoller.Roll(0, 2) == 0 ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Patient");
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_PATIENT_UNIFORM);
      return numberedName;
    }

    private Actor CreateNewHospitalNurse(int spawnTime)
    {
      Actor numberedName = GameActors.FemaleCivilian.CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Nurse");
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_NURSE_UNIFORM);
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.MEDIC);
      numberedName.Inventory.AddAll(MakeItemBandages());
      return numberedName;
    }

    private Actor CreateNewHospitalDoctor(int spawnTime)
    {
      Actor numberedName = GameActors.MaleCivilian.CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Doctor");
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_DOCTOR_UNIFORM);
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.MEDIC,3);
      numberedName.StartingSkill(Skills.IDs.LEADERSHIP);
      numberedName.Inventory.AddAll(MakeItemMedikit());
      numberedName.Inventory.AddAll(MakeItemBandages());
      return numberedName;
    }

    private void MakeHospitalPatientRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
      TileRectangle(map, GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      int x = isFacingEast ? room.Right - 1 : room.Left;
      PlaceDoor(map, x, room.Top + 1, GameTiles.FLOOR_TILES, MakeObjHospitalDoor());
      Point position1 = new Point(room.Left + room.Width / 2, room.Bottom - 2);
      map.PlaceAt(MakeObjBed(GameImages.OBJ_HOSPITAL_BED), position1);
      map.PlaceAt(MakeObjChair(GameImages.OBJ_HOSPITAL_CHAIR), new Point(isFacingEast ? position1.X + 1 : position1.X - 1, position1.Y));
      Point position2 = new Point(isFacingEast ? position1.X - 1 : position1.X + 1, position1.Y);
      map.PlaceAt(MakeObjNightTable(GameImages.OBJ_HOSPITAL_NIGHT_TABLE), position2);

      // Inefficient, but avoids polluting interface
      Func<Item> furnish = () => {
        switch (m_DiceRoller.Roll(0, 3)) {
          case 0: return MakeShopPharmacyItem();
          case 1: return MakeItemGroceries();
#if DEBUG
          case 2: return MakeItemBook();
          default: throw new InvalidOperationException("unhandled roll result");
#else
          default: return MakeItemBook();
#endif
        }
      };

      if (m_DiceRoller.RollChance(50)) map.DropItemAt(furnish(), position2);
      map.PlaceAt(MakeObjWardrobe(GameImages.OBJ_HOSPITAL_WARDROBE), new Point(isFacingEast ? room.Left + 1 : room.Right - 2, room.Top + 1));
    }

    private void MakeHospitalOfficeRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
      TileFill(map, GameTiles.FLOOR_PLANKS, room);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      int x1 = isFacingEast ? room.Right - 1 : room.Left;
      int y = room.Top + 2;
      PlaceDoor(map, x1, y, GameTiles.FLOOR_TILES, MakeObjWoodenDoor());
      int x2 = isFacingEast ? room.Left + 2 : room.Right - 3;
      map.PlaceAt(MakeObjTable(GameImages.OBJ_TABLE), new Point(x2, y));
      map.PlaceAt(MakeObjChair(GameImages.OBJ_CHAIR), new Point(x2 - 1, y));
      map.PlaceAt(MakeObjChair(GameImages.OBJ_CHAIR), new Point(x2 + 1, y));
    }

    private void MakeHospitalStorageRoom(Map map, string baseZoneName, Rectangle room)
    {
      TileRectangle(map, GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      PlaceDoor(map, room.Left + 2, room.Top, GameTiles.FLOOR_TILES, MakeObjHospitalDoor());
      DoForEachTile(room, (Action<Point>) (pt =>
      {
        if (!map.IsWalkable(pt) || CountAdjDoors(map, pt.X, pt.Y) > 0) return;
        map.PlaceAt(MakeObjShelf(), pt);
        Item it = m_DiceRoller.RollChance(80) ? MakeHospitalItem() : MakeItemCannedFood();
        if (it.Model.IsStackable)
          it.Quantity = it.Model.StackingLimit;
        map.DropItemAt(it, pt);
      }));
    }

    private void GiveRandomItemToActor(DiceRoller roller, Actor actor, int spawnTime)
    {
      Func<Item> equip_this = () => {
        if (new WorldTime(spawnTime).Day > Rules.GIVE_RARE_ITEM_DAY && roller.RollChance(Rules.GIVE_RARE_ITEM_CHANCE)) {
          switch (roller.Roll(0, (Session.Get.HasInfection ? 6 : 5))) {
            case 0: return MakeItemGrenade();
            case 1: return MakeItemArmyBodyArmor();
            case 2: return MakeItemHeavyPistolAmmo();
            case 3: return MakeItemHeavyRifleAmmo();
            case 4: return MakeItemCombatKnife();
#if DEBUG
            case 5: return MakeItemPillsAntiviral();
            default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
            default: return MakeItemPillsAntiviral();
#endif
          }
        }
    
        switch (roller.Roll(0, 10)) {
          case 0: return MakeRandomShopItem(ShopType.CONSTRUCTION);
          case 1: return MakeRandomShopItem(ShopType.GENERAL_STORE);
          case 2: return MakeRandomShopItem(ShopType.GROCERY);
          case 3: return MakeRandomShopItem(ShopType.GUNSHOP);
          case 4: return MakeRandomShopItem(ShopType.PHARMACY);
          case 5: return MakeRandomShopItem(ShopType.SPORTSWEAR);
          case 6: return MakeRandomShopItem(ShopType.HUNTING);
          case 7: return MakeRandomParkItem();
          case 8: return MakeRandomBedroomItem();
#if DEBUG
          case 9: return MakeRandomKitchenItem();
          default: throw new ArgumentOutOfRangeException("unhandled roll");
#else
          default: return MakeRandomKitchenItem();
#endif
        }
      };

      actor.Inventory.AddAll(equip_this());
    }

    public Actor CreateNewRefugee(int spawnTime, int itemsToCarry)
    {
      Actor actor;
      if (m_DiceRoller.RollChance(Params.PolicemanChance)) {
        actor = CreateNewPoliceman(spawnTime);
        for (int index = 0; index < itemsToCarry && actor.Inventory.CountItems < actor.Inventory.MaxCapacity; ++index)
          GiveRandomItemToActor(m_DiceRoller, actor, spawnTime);
      } else
        actor = CreateNewCivilian(spawnTime, itemsToCarry, 1);
      int count = 1 + new WorldTime(spawnTime).Day;
      GiveRandomSkillsToActor(m_DiceRoller, actor, count);
      return actor;
    }

    public Actor CreateNewSurvivor(int spawnTime)
    {
      bool flag = m_DiceRoller.Roll(0, 2) == 0;
      Actor numberedName = (flag ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheSurvivors, spawnTime);
      GiveNameToActor(m_DiceRoller, numberedName);
      DressCivilian(m_DiceRoller, numberedName);
      numberedName.Doll.AddDecoration(DollPart.HEAD, flag ? GameImages.SURVIVOR_MALE_BANDANA : GameImages.SURVIVOR_FEMALE_BANDANA);
      numberedName.Inventory.AddAll(MakeItemCannedFood());
      numberedName.Inventory.AddAll(MakeItemArmyRation());
      if (m_DiceRoller.RollChance(50)) {
        numberedName.Inventory.AddAll(MakeItemArmyRifle());
        if (m_DiceRoller.RollChance(50))
          numberedName.Inventory.AddAll(MakeItemHeavyRifleAmmo());
        else
          numberedName.Inventory.AddAll(MakeItemGrenade());
      } else {
        numberedName.Inventory.AddAll(MakeItemShotgun());
        if (m_DiceRoller.RollChance(50))
          numberedName.Inventory.AddAll(MakeItemShotgunAmmo());
        else
          numberedName.Inventory.AddAll(MakeItemGrenade());
      }
      numberedName.Inventory.AddAll(MakeItemMedikit());
      switch (m_DiceRoller.Roll(0, 3))
      {
        case 0:
          numberedName.Inventory.AddAll(MakeItemPillsSLP());
          break;
        case 1:
          numberedName.Inventory.AddAll(MakeItemPillsSTA());
          break;
        case 2:
          numberedName.Inventory.AddAll(MakeItemPillsSAN());
          break;
      }
      numberedName.Inventory.AddAll(MakeItemArmyBodyArmor());
      int count = 3 + new WorldTime(spawnTime).Day;
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, count);
      numberedName.CreateCivilianDeductFoodSleep(m_Rules);
      return numberedName;
    }

#if DEAD_FUNC
    public Actor CreateNewNakedHuman(int spawnTime)
    {
      return (m_Rules.Roll(0, 2) == 0 ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheCivilians, spawnTime);
    }
#endif

    public Actor CreateNewCivilian(int spawnTime, int itemsToCarry, int skills)
    {
      Actor numberedName = (m_DiceRoller.Roll(0, 2) == 0 ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      DressCivilian(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      for (int index = 0; index < itemsToCarry; ++index)
        GiveRandomItemToActor(m_DiceRoller, numberedName, spawnTime);
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, skills);
      numberedName.CreateCivilianDeductFoodSleep(m_Rules);
      if (m_PC_names?.Contains(numberedName.UnmodifiedName) ?? false) {
        numberedName.Controller = new PlayerController();
      }
      return numberedName;
    }

    public Actor CreateNewPoliceman(int spawnTime)
    {
      Actor numberedName = GameActors.Policeman.CreateNumberedName(GameFactions.ThePolice, spawnTime);
      DressPolice(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Cop");
      // Notable skills
      // martial arts 1 makes the starting baton useless
      // XXX probably should be used as a trade good rather than dropped ASAP
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.FIREARMS);
      numberedName.StartingSkill(Skills.IDs.LEADERSHIP);
      // While auto-equip here would be nice, it is unclear that RogueForm.Game.DoEquipItem is safe to call here.
      // Inline the functional part instead.
      if (m_DiceRoller.RollChance(50)) {
        var it = MakeItemPistol();
        numberedName.Inventory.AddAll(it);
        numberedName.Inventory.AddAll(MakeItemLightPistolAmmo());
        it.Equip();
        numberedName.OnEquipItem(it);
      } else {
        var it = MakeItemShotgun();
        numberedName.Inventory.AddAll(it);
        numberedName.Inventory.AddAll(MakeItemShotgunAmmo());
        it.Equip();
        numberedName.OnEquipItem(it);
      }
      numberedName.Inventory.AddAll(MakeItemTruncheon());
      numberedName.Inventory.AddAll(MakeItemFlashlight());
//    numberedName.Inventory.AddAll(MakeItemPoliceRadio()); // class prop, implicit for police
      if (m_DiceRoller.RollChance(50)) {
        var it = m_DiceRoller.RollChance(80) ? MakeItemPoliceJacket() : MakeItemPoliceRiotArmor();
        numberedName.Inventory.AddAll(it);
        it.Equip();
        numberedName.OnEquipItem(it);
      }
      if (m_PC_names?.Contains(numberedName.UnmodifiedName) ?? false) {
        numberedName.Controller = new PlayerController();
      }
      return numberedName;
    }

    public Actor CreateNewUndead(int spawnTime)
    {
      Actor actor;
      if (Session.Get.HasAllZombies) {
        int num = m_DiceRoller.Roll(0, 100);
        actor = (num < RogueGame.Options.SpawnSkeletonChance ? GameActors.Skeleton 
              : (num < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance ? GameActors.Zombie
              : (num < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance + RogueGame.Options.SpawnZombieMasterChance ? GameActors.ZombieMaster
              : GameActors.Skeleton))).CreateNumberedName(GameFactions.TheUndeads, spawnTime);
      } else {
        actor = MakeZombified(null, CreateNewCivilian(spawnTime, 0, 0), spawnTime);
        int num = new WorldTime(spawnTime).Day / 2;
        if (num > 0) {
          for (int index = 0; index < num; ++index) {
            Skills.IDs? nullable = ((Skills.IDs)m_DiceRoller.Roll(0, (int)Skills.IDs._COUNT)).Zombify();
            if (nullable.HasValue) actor.SkillUpgrade(nullable.Value);
          }
          actor.RecomputeStartingStats();
        }
      }
      return actor;
    }

    public static Actor MakeZombified(Actor zombifier, Actor deadVictim, int turn)
    {
      string properName = string.Format("{0}'s zombie", (object) deadVictim.UnmodifiedName);
      Actor named = (deadVictim.Doll.Body.IsMale ? GameActors.MaleZombified : GameActors.FemaleZombified).CreateNamed(zombifier == null ? GameFactions.TheUndeads : zombifier.Faction, properName, deadVictim.IsPluralName, turn);
      named.ActionPoints = 0;
      for (DollPart part = DollPart._FIRST; part < DollPart._COUNT; ++part) {
        List<string> decorations = deadVictim.Doll.GetDecorations(part);
        if (decorations != null) {
          foreach (string imageID in decorations)
            named.Doll.AddDecoration(part, imageID);
        }
      }
      named.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\bloodied");
      return named;
    }

    public Actor CreateNewSewersUndead(int spawnTime)
    {
      if (!Session.Get.HasAllZombies) return CreateNewUndead(spawnTime);
      return (m_DiceRoller.RollChance(80) ? GameActors.RatZombie : GameActors.Zombie).CreateNumberedName(GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewBasementRatZombie(int spawnTime)
    {
      if (!Session.Get.HasAllZombies) return CreateNewUndead(spawnTime);
      return GameActors.RatZombie.CreateNumberedName(GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewCHARGuard(int spawnTime)
    {
      Actor numberedName = GameActors.CHARGuard.CreateNumberedName(GameFactions.TheCHARCorporation, spawnTime);
      DressCHARGuard(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Gd.");
      numberedName.Inventory.AddAll(MakeItemShotgun());
      numberedName.Inventory.AddAll(MakeItemShotgunAmmo());
      numberedName.Inventory.AddAll(MakeItemCHARLightBodyArmor());
      return numberedName;
    }

    public Actor CreateNewArmyNationalGuard(int spawnTime, string rankName)
    {
      Actor numberedName = GameActors.NationalGuard.CreateNumberedName(GameFactions.TheArmy, spawnTime);
      DressArmy(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, rankName);
      numberedName.Inventory.AddAll(MakeItemArmyRifle());
      numberedName.Inventory.AddAll(MakeItemHeavyRifleAmmo());
      numberedName.Inventory.AddAll(MakeItemArmyPistol());
      numberedName.Inventory.AddAll(MakeItemHeavyPistolAmmo());
      numberedName.Inventory.AddAll(MakeItemArmyBodyArmor());
      ItemBarricadeMaterial barricadeMaterial = MakeItemWoodenPlank();
      barricadeMaterial.Quantity = GameItems.WOODENPLANK.StackingLimit;
      numberedName.Inventory.AddAll(barricadeMaterial);
      // National Guard training includes firing range and construction.
      // The minimum physical fitness standards slide off with age.
      // If we had a melee weapons skill it would be here.
      // Maximum acceptable fat % 23%, discharged at 26%
      numberedName.StartingSkill(Skills.IDs.CARPENTRY);
      numberedName.StartingSkill(Skills.IDs.FIREARMS);
      int count = new WorldTime(spawnTime).Day - RogueGame.NATGUARD_DAY;
      if (count > 0)
        GiveRandomSkillsToActor(m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewBikerMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = GameActors.BikerMan.CreateNumberedName(GameFactions.TheBikers, spawnTime);
      numberedName.GangID = gangId;
      DressBiker(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Inventory.AddAll(m_DiceRoller.RollChance(50) ? MakeItemCrowbar() : MakeItemBaseballBat());
      numberedName.Inventory.AddAll(MakeItemBikerGangJacket(gangId));
      int count = new WorldTime(spawnTime).Day - RogueGame.BIKERS_RAID_DAY;
      if (count > 0)
        GiveRandomSkillsToActor(m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewGangstaMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = GameActors.GangstaMan.CreateNumberedName(GameFactions.TheGangstas, spawnTime);
      numberedName.GangID = gangId;
      DressGangsta(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Inventory.AddAll(m_DiceRoller.RollChance(50) ? (Item)MakeItemRandomPistol() : (Item)MakeItemBaseballBat());
      int count = new WorldTime(spawnTime).Day - RogueGame.GANGSTAS_RAID_DAY;
      if (count > 0)
        GiveRandomSkillsToActor(m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewBlackOps(int spawnTime, string rankName)
    {
      Actor numberedName = GameActors.BlackOps.CreateNumberedName(GameFactions.TheBlackOps, spawnTime);
      DressBlackOps(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, rankName);
      numberedName.Inventory.AddAll(MakeItemPrecisionRifle());
      numberedName.Inventory.AddAll(MakeItemHeavyRifleAmmo());
      numberedName.Inventory.AddAll(MakeItemArmyPistol());
      numberedName.Inventory.AddAll(MakeItemHeavyPistolAmmo());
      numberedName.Inventory.AddAll(MakeItemBlackOpsGPS());
      return numberedName;
    }

    public Actor CreateNewFeralDog(int spawnTime)
    {
      Actor numberedName = GameActors.FeralDog.CreateNumberedName(GameFactions.TheFerals, spawnTime);
      SkinDog(m_DiceRoller, numberedName);
      return numberedName;
    }

    static private void AddExit(Map from, Point fromPosition, Map to, Point toPosition, string exitImageID, bool isAnAIExit)
    {
      from.SetExitAt(fromPosition, new Exit(to, toPosition, isAnAIExit));
      from.AddDecorationAt(exitImageID, fromPosition);
    }

    static private void MakeWalkwayZones(Map map, Block b)
    {
      Rectangle rectangle = b.Rectangle;
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width - 1, 1)));
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(rectangle.Left + 1, rectangle.Bottom - 1, rectangle.Width - 1, 1)));
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(rectangle.Right - 1, rectangle.Top, 1, rectangle.Height - 1)));
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(rectangle.Left, rectangle.Top + 1, 1, rectangle.Height - 1)));
    }

    public struct Parameters
    {
      private int m_MapWidth;
      private int m_MapHeight;
      private int m_MinBlockSize;
      private int m_WreckedCarChance;
      private int m_CHARBuildingChance;
      private int m_ShopBuildingChance;
      private int m_ParkBuildingChance;
      private int m_PostersChance;
      private int m_TagsChance;
      private int m_ItemInShopShelfChance;
      private int m_PolicemanChance;

      // these have operational reasons for being public-writable
      public bool GeneratePoliceStation;
      public bool GenerateHospital;
      public District District;

      // map generation is naturally slow, so we can afford to hard-validate even in release mode
      public int MapWidth {
        get {
          return m_MapWidth;
        }
        set {
          if (value <= 0 || value > RogueGame.MAP_MAX_WIDTH)
            throw new ArgumentOutOfRangeException(nameof(MapWidth),value,"must be in 1.."+ RogueGame.MAP_MAX_WIDTH.ToString());
          m_MapWidth = value;
        }
      }

      public int MapHeight {
        get {
          return m_MapHeight;
        }
        set {
          if (value <= 0 || value > RogueGame.MAP_MAX_HEIGHT)
            throw new ArgumentOutOfRangeException(nameof(MapHeight),value,"must be in 1.."+ RogueGame.MAP_MAX_HEIGHT.ToString());
          m_MapHeight = value;
        }
      }

      public int MinBlockSize {
        get {
          return m_MinBlockSize;
        }
        set {
          if (value < 4 || value > 32)
            throw new ArgumentOutOfRangeException(nameof(MinBlockSize),value,"must be in 4..32");
          m_MinBlockSize = value;
        }
      }

      public int WreckedCarChance {
        get {
          return m_WreckedCarChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(WreckedCarChance),value,"must be in 0..100");
          m_WreckedCarChance = value;
        }
      }

      public int ShopBuildingChance {
        get {
          return m_ShopBuildingChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(ShopBuildingChance),value,"must be in 0..100");
          m_ShopBuildingChance = value;
        }
      }

      public int ParkBuildingChance {
        get {
          return m_ParkBuildingChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(ParkBuildingChance),value,"must be in 0..100");
          m_ParkBuildingChance = value;
        }
      }

      public int CHARBuildingChance {
        get {
          return m_CHARBuildingChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(CHARBuildingChance),value,"must be in 0..100");
          m_CHARBuildingChance = value;
        }
      }

      public int PostersChance {
        get {
          return m_PostersChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(PostersChance),value,"must be in 0..100");
          m_PostersChance = value;
        }
      }

      public int TagsChance {
        get {
          return m_TagsChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(TagsChance),value,"must be in 0..100");
          m_TagsChance = value;
        }
      }

      public int ItemInShopShelfChance {
        get {
          return m_ItemInShopShelfChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(ItemInShopShelfChance),value,"must be in 0..100");
          m_ItemInShopShelfChance = value;
        }
      }

      public int PolicemanChance {
        get {
          return m_PolicemanChance;
        }
        set {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(PolicemanChance),value,"must be in 0..100");
          m_PolicemanChance = value;
        }
      }
    }

    public class Block
    {
      public readonly Rectangle Rectangle;
      public readonly Rectangle BuildingRect;
      public readonly Rectangle InsideRect;

      public Block(Rectangle rect)
      {
        Rectangle = rect;
        BuildingRect = new Rectangle(rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2);
        InsideRect = new Rectangle(BuildingRect.Left + 1, BuildingRect.Top + 1, BuildingRect.Width - 2, BuildingRect.Height - 2);
      }

      public Block(Block copyFrom)
      {
        Rectangle = copyFrom.Rectangle;
        BuildingRect = copyFrom.BuildingRect;
        InsideRect = copyFrom.InsideRect;
      }
    }

    protected enum ShopType : byte
    {
      GENERAL_STORE = 0,
      GROCERY = 1,
      SPORTSWEAR = 2,
      PHARMACY = 3,
      CONSTRUCTION = 4,
      GUNSHOP = 5,
      HUNTING = 6,
      _COUNT = 7,
    }

    protected enum CHARBuildingType : byte
    {
      NONE,
      AGENCY,
      OFFICE,
    }
  }
}
