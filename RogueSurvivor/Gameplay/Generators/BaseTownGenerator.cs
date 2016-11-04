// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.BaseTownGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Gameplay.Generators
{
  internal class BaseTownGenerator : BaseMapGenerator
  {
    public static readonly BaseTownGenerator.Parameters DEFAULT_PARAMS = new BaseTownGenerator.Parameters()
    {
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
    private static string[] CHAR_POSTERS = new string[3]
    {
      "Tiles\\Decoration\\char_poster1",
      "Tiles\\Decoration\\char_poster2",
      "Tiles\\Decoration\\char_poster3"
    };
    private static readonly string[] POSTERS = new string[2]
    {
      "Tiles\\Decoration\\posters1",
      "Tiles\\Decoration\\posters2"
    };
    private static readonly string[] TAGS = new string[7]
    {
      "Tiles\\Decoration\\tags1",
      "Tiles\\Decoration\\tags2",
      "Tiles\\Decoration\\tags3",
      "Tiles\\Decoration\\tags4",
      "Tiles\\Decoration\\tags5",
      "Tiles\\Decoration\\tags6",
      "Tiles\\Decoration\\tags7"
    };
    private BaseTownGenerator.Parameters m_Params = BaseTownGenerator.DEFAULT_PARAMS;
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
    protected DiceRoller m_DiceRoller;
    private List<BaseTownGenerator.Block> m_SurfaceBlocks;

    public BaseTownGenerator.Parameters Params
    {
      get
      {
        return m_Params;
      }
      set
      {
                m_Params = value;
      }
    }

    public BaseTownGenerator(RogueGame game, BaseTownGenerator.Parameters parameters)
      : base(game)
    {
            m_Params = parameters;
            m_DiceRoller = new DiceRoller();
    }

    public override Map Generate(int seed)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map map = new Map(seed, "Base City", m_Params.MapWidth, m_Params.MapHeight);
      map.District = m_Params.District;

      TileFill(map, m_Game.GameTiles.FLOOR_GRASS);
      List<BaseTownGenerator.Block> list = new List<BaseTownGenerator.Block>();
      Rectangle rect = new Rectangle(0, 0, map.Width, map.Height);
      MakeBlocks(map, true, ref list, rect);
      List<BaseTownGenerator.Block> blockList1 = new List<BaseTownGenerator.Block>((IEnumerable<BaseTownGenerator.Block>) list);
      List<BaseTownGenerator.Block> blockList2 = new List<BaseTownGenerator.Block>(blockList1.Count);
      m_SurfaceBlocks = new List<BaseTownGenerator.Block>(list.Count);
      foreach (BaseTownGenerator.Block copyFrom in list)
        m_SurfaceBlocks.Add(new BaseTownGenerator.Block(copyFrom));
      if (m_Params.GeneratePoliceStation)
      {
        BaseTownGenerator.Block policeBlock;
        MakePoliceStation(map, list, out policeBlock);
        blockList1.Remove(policeBlock);
      }
      if (m_Params.GenerateHospital)
      {
        BaseTownGenerator.Block hospitalBlock;
                MakeHospital(map, list, out hospitalBlock);
        blockList1.Remove(hospitalBlock);
      }
      blockList2.Clear();
      foreach (BaseTownGenerator.Block b in blockList1)
      {
        if (m_DiceRoller.RollChance(m_Params.ShopBuildingChance) && MakeShopBuilding(map, b))
          blockList2.Add(b);
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      int num = 0;
      foreach (BaseTownGenerator.Block b in blockList1)
      {
        if (m_Params.District.Kind == DistrictKind.BUSINESS && num == 0 || m_DiceRoller.RollChance(m_Params.CHARBuildingChance))
        {
          BaseTownGenerator.CHARBuildingType charBuildingType = MakeCHARBuilding(map, b);
          if (charBuildingType == BaseTownGenerator.CHARBuildingType.OFFICE)
          {
            ++num;
                        PopulateCHAROfficeBuilding(map, b);
          }
          if (charBuildingType != BaseTownGenerator.CHARBuildingType.NONE)
            blockList2.Add(b);
        }
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      foreach (BaseTownGenerator.Block b in blockList1)
      {
        if (m_DiceRoller.RollChance(m_Params.ParkBuildingChance) && MakeParkBuilding(map, b))
          blockList2.Add(b);
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      foreach (BaseTownGenerator.Block b in blockList1)
      {
                MakeHousingBuilding(map, b);
        blockList2.Add(b);
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
            AddWreckedCarsOutside(map, rect);
            DecorateOutsideWallsWithPosters(map, rect, m_Params.PostersChance);
            DecorateOutsideWallsWithTags(map, rect, m_Params.TagsChance);
      return map;
    }

    public virtual Map GenerateSewersMap(int seed, District district)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map sewers = new Map(seed, "sewers", district.EntryMap.Width, district.EntryMap.Height)
      {
        Lighting = Lighting.DARKNESS
      };
      sewers.AddZone(MakeUniqueZone("sewers", sewers.Rect));
      TileFill(sewers, m_Game.GameTiles.WALL_SEWER);

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
      List<BaseTownGenerator.Block> list = new List<BaseTownGenerator.Block>(m_SurfaceBlocks.Count);
      MakeBlocks(sewers, false, ref list, new Rectangle(0, 0, sewers.Width, sewers.Height));

#region 2. Make tunnels.
      foreach (BaseTownGenerator.Block block in list)
        TileRectangle(sewers, m_Game.GameTiles.FLOOR_SEWER_WATER, block.Rectangle);
      foreach (BaseTownGenerator.Block block in list) {
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
            if (m_DiceRoller.RollChance(3) && sewers.GetTileAt(x, y).Model.IsWalkable) {
              Tile tileAt = surface.GetTileAt(x, y);
              if (tileAt.Model.IsWalkable && sewers.GetMapObjectAt(x, y) == null && !tileAt.IsInside && ((tileAt.Model == m_Game.GameTiles.FLOOR_WALKWAY || tileAt.Model == m_Game.GameTiles.FLOOR_GRASS) && surface.GetMapObjectAt(x, y) == null))
              {
                Point point = new Point(x, y);
                if (!sewers.HasAnyAdjacentInMap(point, (Predicate<Point>) (p => sewers.GetExitAt(p) != null)) && !surface.HasAnyAdjacentInMap(point, (Predicate<Point>) (p => surface.GetExitAt(p) != null)))
                {
                  AddExit(sewers, point, surface, point, "Tiles\\Decoration\\sewer_ladder", true);
                  AddExit(surface, point, sewers, point, "Tiles\\Decoration\\sewer_hole", true);
                  ++countLinks;
                }
              }
            }
          }
        }
      }
      while (countLinks < 1);
#endregion

#region 4. Additional jobs.
      for (int x = 0; x < sewers.Width; ++x)
      {
        for (int y = 0; y < sewers.Height; ++y)
          sewers.GetTileAt(x, y).IsInside = true;
      }
#endregion

#region 5. Sewers Maintenance Room & Building(surface).
      List<BaseTownGenerator.Block> blockList = (List<BaseTownGenerator.Block>) null;
      foreach (BaseTownGenerator.Block mSurfaceBlock in m_SurfaceBlocks)
      {
        if (mSurfaceBlock.BuildingRect.Width <= m_Params.MinBlockSize + 2 && (mSurfaceBlock.BuildingRect.Height <= m_Params.MinBlockSize + 2 && !IsThereASpecialBuilding(surface, mSurfaceBlock.InsideRect)))
        {
          bool flag = true;
          for (int left = mSurfaceBlock.Rectangle.Left; left < mSurfaceBlock.Rectangle.Right && flag; ++left)
          {
            for (int top = mSurfaceBlock.Rectangle.Top; top < mSurfaceBlock.Rectangle.Bottom && flag; ++top)
            {
              if (sewers.GetTileAt(left, top).Model.IsWalkable)
                flag = false;
            }
          }
          if (flag)
          {
            if (blockList == null)
              blockList = new List<BaseTownGenerator.Block>(m_SurfaceBlocks.Count);
            blockList.Add(mSurfaceBlock);
            break;
          }
        }
      }
      Rectangle buildingRect;
      if (blockList != null)
      {
        BaseTownGenerator.Block block = blockList[m_DiceRoller.Roll(0, blockList.Count)];
                ClearRectangle(surface, block.BuildingRect);
                TileFill(surface, m_Game.GameTiles.FLOOR_CONCRETE, block.BuildingRect);
                m_SurfaceBlocks.Remove(block);
        BaseTownGenerator.Block b1 = new BaseTownGenerator.Block(block.Rectangle);
        buildingRect = b1.BuildingRect;
        int x = buildingRect.Left + buildingRect.Width / 2;
        int top = buildingRect.Top;
        int num2 = buildingRect.Height / 2;
        int y = top + num2;
        Point exitPosition = new Point(x, y);
                MakeSewersMaintenanceBuilding(surface, true, b1, sewers, exitPosition);
        BaseTownGenerator.Block b2 = new BaseTownGenerator.Block(block.Rectangle);
                MakeSewersMaintenanceBuilding(sewers, false, b2, surface, exitPosition);
      }
#endregion

#region 6. Some rooms.
      foreach (BaseTownGenerator.Block block in list)
      {
        if (m_DiceRoller.RollChance(SEWERS_ROOM_CHANCE) && CheckForEachTile(block.BuildingRect, (Predicate<Point>) (pt => !sewers.GetTileAt(pt).Model.IsWalkable)))
        {
          TileFill(sewers, m_Game.GameTiles.FLOOR_CONCRETE, block.InsideRect);
          Map map1 = sewers;
          buildingRect = block.BuildingRect;
          int left1 = buildingRect.Left;
          buildingRect = block.BuildingRect;
          int num2 = buildingRect.Width / 2;
          int x1 = left1 + num2;
          buildingRect = block.BuildingRect;
          int top1 = buildingRect.Top;
          TileModel floorConcrete1 = m_Game.GameTiles.FLOOR_CONCRETE;
          map1.SetTileModelAt(x1, top1, floorConcrete1);
          Map map2 = sewers;
          buildingRect = block.BuildingRect;
          int left2 = buildingRect.Left;
          buildingRect = block.BuildingRect;
          int num3 = buildingRect.Width / 2;
          int x2 = left2 + num3;
          buildingRect = block.BuildingRect;
          int y1 = buildingRect.Bottom - 1;
          TileModel floorConcrete2 = m_Game.GameTiles.FLOOR_CONCRETE;
          map2.SetTileModelAt(x2, y1, floorConcrete2);
          Map map3 = sewers;
          buildingRect = block.BuildingRect;
          int left3 = buildingRect.Left;
          buildingRect = block.BuildingRect;
          int top2 = buildingRect.Top;
          buildingRect = block.BuildingRect;
          int num4 = buildingRect.Height / 2;
          int y2 = top2 + num4;
          TileModel floorConcrete3 = m_Game.GameTiles.FLOOR_CONCRETE;
          map3.SetTileModelAt(left3, y2, floorConcrete3);
          Map map4 = sewers;
          buildingRect = block.BuildingRect;
          int x3 = buildingRect.Right - 1;
          buildingRect = block.BuildingRect;
          int top3 = buildingRect.Top;
          buildingRect = block.BuildingRect;
          int num5 = buildingRect.Height / 2;
          int y3 = top3 + num5;
          TileModel floorConcrete4 = m_Game.GameTiles.FLOOR_CONCRETE;
          map4.SetTileModelAt(x3, y3, floorConcrete4);
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
      for (int x = 0; x < sewers.Width; ++x) {
        for (int y = 0; y < sewers.Height; ++y) {
          if (m_DiceRoller.RollChance(SEWERS_ITEM_CHANCE) && sewers.IsWalkable(x, y))
          {
            Item it;
            switch (m_DiceRoller.Roll(0, 3))
            {
              case 0:
                it = MakeItemBigFlashlight();
                break;
              case 1:
                it = MakeItemCrowbar();
                break;
              case 2:
                it = MakeItemSprayPaint();
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled roll");
            }
            sewers.DropItemAt(it, x, y);
          }
        }
      }
#endregion

#region 9. Tags.
      for (int x = 0; x < sewers.Width; ++x) {
        for (int y = 0; y < sewers.Height; ++y) {
          if (m_DiceRoller.RollChance(SEWERS_TAG_CHANCE))
          {
            Tile tileAt = sewers.GetTileAt(x, y);
            if (!tileAt.Model.IsWalkable && CountAdjWalkables(sewers, x, y) >= 2)
              tileAt.AddDecoration(BaseTownGenerator.TAGS[m_DiceRoller.Roll(0, BaseTownGenerator.TAGS.Length)]);
          }
        }
      }
#endregion

      sewers.Name = string.Format("Sewers@{0}-{1}", (object) district.WorldPosition.X, (object) district.WorldPosition.Y);
      district.SewersMap = sewers;
      return sewers;
    }

    public virtual Map GenerateSubwayMap(int seed, District district)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map subway = new Map(seed, "subway", district.EntryMap.Width, district.EntryMap.Height)
      {
        Lighting = Lighting.DARKNESS
      };
      TileFill(subway, m_Game.GameTiles.WALL_BRICK);

      subway.Name = string.Format("Subway@{0}-{1}", (object) district.WorldPosition.X, (object) district.WorldPosition.Y);
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
      DoForEachTile(tmp, (Action<Point>)(pt => { subway.SetTileModelAt(pt.X, pt.Y, m_Game.GameTiles.RAIL_EW); }));
      subway.AddZone(MakeUniqueZone("rails", tmp));
      DoForEachTile(new Rectangle(0, railY-1, subway.Width, height+2), (Action<Point>)(pt => { Session.Get.ForcePoliceKnown(new Location(subway, pt)); }));
#endregion

#region 2. Make station linked to surface.
      List<BaseTownGenerator.Block> blockList = (List<BaseTownGenerator.Block>) null;
      foreach (BaseTownGenerator.Block mSurfaceBlock in m_SurfaceBlocks)
      {
        if (mSurfaceBlock.BuildingRect.Width <= m_Params.MinBlockSize + 2 && (mSurfaceBlock.BuildingRect.Height <= m_Params.MinBlockSize + 2 && !IsThereASpecialBuilding(entryMap, mSurfaceBlock.InsideRect)))
        {
           // unclear whether this scales with turns per hour.
           // If anything, at high magnifications we may need to not be "too far" from the rails either
          const int minDistToRails = 8;
          bool flag = false;
          // old test failed for subway.Width/2-1-minDistToRails to subway.Width/2+2+minDistToRails
          // at district size 50: railY 24, upper bound 27; 38 should pass
          // we want a simple interval-does-not-intersect test
          if (mSurfaceBlock.Rectangle.Top - minDistToRails > railY-1+height) flag = true;  // top below critical y
          if (mSurfaceBlock.Rectangle.Bottom + minDistToRails-1 < railY) flag = true;   // bottom above critical y
          if (flag)
          {
            if (blockList == null)
              blockList = new List<BaseTownGenerator.Block>(m_SurfaceBlocks.Count);
            blockList.Add(mSurfaceBlock);
            break;
          }
        }
      }
      if (blockList != null)
      {
        BaseTownGenerator.Block block = blockList[m_DiceRoller.Roll(0, blockList.Count)];
        ClearRectangle(entryMap, block.BuildingRect);
        TileFill(entryMap, m_Game.GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        m_SurfaceBlocks.Remove(block);
        BaseTownGenerator.Block b1 = new BaseTownGenerator.Block(block.Rectangle);
        Point exitPosition = new Point(b1.BuildingRect.Left + b1.BuildingRect.Width / 2, b1.InsideRect.Top);
        MakeSubwayStationBuilding(entryMap, true, b1, subway, exitPosition);
        BaseTownGenerator.Block b2 = new BaseTownGenerator.Block(block.Rectangle);
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
        if (!subway.GetTileAt(x2, y2).Model.IsWalkable) {
          rect = direction != Direction.N ? new Rectangle(x2, y2, toolsRoomWidth, toolsRoomHeight) : new Rectangle(x2, y2 - toolsRoomHeight + 1, toolsRoomWidth, toolsRoomHeight);
          flag1 = CheckForEachTile(rect, (Predicate<Point>) (pt => !subway.GetTileAt(pt).Model.IsWalkable));
        }
        ++num3;
      }
      while (num3 < subway.Width * subway.Height && !flag1);
      if (flag1) {
        TileFill(subway, m_Game.GameTiles.FLOOR_CONCRETE, rect);
        TileRectangle(subway, m_Game.GameTiles.WALL_BRICK, rect);
        PlaceDoor(subway, rect.Left + 2, direction == Direction.N ? rect.Bottom - 1 : rect.Top, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
        subway.AddZone(MakeUniqueZone("tools room", rect));
        DoForEachTile(rect, (Action<Point>) (pt =>
        {
          if (!subway.IsWalkable(pt.X, pt.Y) || CountAdjWalls(subway, pt.X, pt.Y) == 0 || CountAdjDoors(subway, pt.X, pt.Y) > 0) return;
          subway.PlaceMapObjectAt(MakeObjShelf(), pt);
          subway.DropItemAt(MakeShopConstructionItem(), pt);
        }));
        DoForEachTile(rect, (Action<Point>)(pt => { Session.Get.ForcePoliceKnown(new Location(subway, pt)); }));
      }
#endregion
#region 4. Tags & Posters almost everywhere.
      for (int x2 = 0; x2 < subway.Width; ++x2)
      {
        for (int y2 = 0; y2 < subway.Height; ++y2)
        {
          if (m_DiceRoller.RollChance(SUBWAY_TAGS_POSTERS_CHANCE))
          {
            Tile tileAt = subway.GetTileAt(x2, y2);
            if (!tileAt.Model.IsWalkable && CountAdjWalkables(subway, x2, y2) >= 2)
            {
              if (m_DiceRoller.RollChance(50))
                tileAt.AddDecoration(BaseTownGenerator.POSTERS[m_DiceRoller.Roll(0, BaseTownGenerator.POSTERS.Length)]);
              if (m_DiceRoller.RollChance(50))
                tileAt.AddDecoration(BaseTownGenerator.TAGS[m_DiceRoller.Roll(0, BaseTownGenerator.TAGS.Length)]);
            }
          }
        }
      }
#endregion
      // 5. Additional jobs.
      // Mark all the map as inside.
      for (int x2 = 0; x2 < subway.Width; ++x2) {
        for (int y2 = 0; y2 < subway.Height; ++y2)
          subway.GetTileAt(x2, y2).IsInside = true;
      }

      return subway;
    }

    private void QuadSplit(Rectangle rect, int minWidth, int minHeight, out int splitX, out int splitY, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight)
    {
      int width1 = m_DiceRoller.Roll(rect.Width / 3, 2 * rect.Width / 3);
      int height1 = m_DiceRoller.Roll(rect.Height / 3, 2 * rect.Height / 3);
      if (width1 < minWidth)
        width1 = minWidth;
      if (height1 < minHeight)
        height1 = minHeight;
      int width2 = rect.Width - width1;
      int height2 = rect.Height - height1;
      bool flag1;
      bool flag2 = flag1 = true;
      if (width2 < minWidth)
      {
        width1 = rect.Width;
        width2 = 0;
        flag2 = false;
      }
      if (height2 < minHeight)
      {
        height1 = rect.Height;
        height2 = 0;
        flag1 = false;
      }
      splitX = rect.Left + width1;
      splitY = rect.Top + height1;
      topLeft = new Rectangle(rect.Left, rect.Top, width1, height1);
      topRight = !flag2 ? Rectangle.Empty : new Rectangle(splitX, rect.Top, width2, height1);
      bottomLeft = !flag1 ? Rectangle.Empty : new Rectangle(rect.Left, splitY, width1, height2);
      if (flag2 && flag1)
        bottomRight = new Rectangle(splitX, splitY, width2, height2);
      else
        bottomRight = Rectangle.Empty;
    }

    private void MakeBlocks(Map map, bool makeRoads, ref List<BaseTownGenerator.Block> list, Rectangle rect)
    {
      int splitX;
      int splitY;
      Rectangle topLeft;
      Rectangle topRight;
      Rectangle bottomLeft;
      Rectangle bottomRight;
            QuadSplit(rect, m_Params.MinBlockSize + 1, m_Params.MinBlockSize + 1, out splitX, out splitY, out topLeft, out topRight, out bottomLeft, out bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty)
      {
        if (makeRoads)
        {
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_EW], new Rectangle(rect.Left, rect.Top, rect.Width, 1));
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_EW], new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1));
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_NS], new Rectangle(rect.Left, rect.Top, 1, rect.Height));
                    MakeRoad(map, m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_NS], new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height));
          topLeft.Width -= 2;
          topLeft.Height -= 2;
          topLeft.Offset(1, 1);
        }
        list.Add(new BaseTownGenerator.Block(topLeft));
      }
      else
      {
                MakeBlocks(map, makeRoads, ref list, topLeft);
        if (!topRight.IsEmpty)
                    MakeBlocks(map, makeRoads, ref list, topRight);
        if (!bottomLeft.IsEmpty)
                    MakeBlocks(map, makeRoads, ref list, bottomLeft);
        if (bottomRight.IsEmpty)
          return;
                MakeBlocks(map, makeRoads, ref list, bottomRight);
      }
    }

    protected virtual void MakeRoad(Map map, TileModel roadModel, Rectangle rect)
    {
            TileFill(map, roadModel, rect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        if (!m_Game.GameTiles.IsRoadModel(prevmodel))
          return;
        map.SetTileModelAt(x, y, prevmodel);
      }));
      map.AddZone(MakeUniqueZone("road", rect));
    }

    protected virtual void PlaceDoor(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      map.SetTileModelAt(x, y, floor);
            MapObjectPlace(map, x, y, (MapObject) door);
    }

    protected virtual void PlaceDoorIfNoObject(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      if (map.GetMapObjectAt(x, y) != null)
        return;
            PlaceDoor(map, x, y, floor, door);
    }

    protected virtual bool PlaceDoorIfAccessible(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
    {
      int num = 0;
      Point point1 = new Point(x, y);
      foreach (Direction direction in Direction.COMPASS)
      {
        Point point2 = point1 + direction;
        if (map.IsWalkable(point2.X, point2.Y))
          ++num;
      }
      if (num < minAccessibility)
        return false;
            PlaceDoorIfNoObject(map, x, y, floor, door);
      return true;
    }

    protected virtual bool PlaceDoorIfAccessibleAndNotAdjacent(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
    {
      int num = 0;
      Point point1 = new Point(x, y);
      foreach (Direction direction in Direction.COMPASS) {
        Point point2 = point1 + direction;
        if (map.IsWalkable(point2.X, point2.Y)) ++num;
        if (map.GetMapObjectAt(point2.X, point2.Y) is DoorWindow) return false;
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
          Tile tileAt = map.GetTileAt(pt.X, pt.Y);
          if (!tileAt.IsInside && tileAt.Model.IsWalkable && tileAt.Model != m_Game.GameTiles.FLOOR_GRASS) {
            MapObject mapObj = MakeObjWreckedCar(m_DiceRoller);
            if (m_DiceRoller.RollChance(50)) mapObj.Ignite();
            return mapObj;
          }
        }
        return null;
      }));
    }

    protected bool IsThereASpecialBuilding(Map map, Rectangle rect)
    {
      List<Zone> zonesAt = map.GetZonesAt(rect.Left, rect.Top);
      if (null != zonesAt && zonesAt.Any(zone=> zone.Name.Contains("Sewers Maintenance") || zone.Name.Contains("Subway Station") || zone.Name.Contains("office") || zone.Name.Contains("shop")))
        return true;
      return map.HasAnExitIn(rect); // relatively slow compared to above
    }

    protected virtual bool MakeShopBuilding(Map map, BaseTownGenerator.Block b)
    {
      Contract.Requires(null!=map.District);
      if (b.InsideRect.Width < 5 || b.InsideRect.Height < 5) return false;
      TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, m_Game.GameTiles.WALL_STONE, b.BuildingRect);
      TileFill(map, m_Game.GameTiles.FLOOR_TILES, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) => tile.IsInside = true));
      BaseTownGenerator.ShopType shopType = (BaseTownGenerator.ShopType)m_DiceRoller.Roll(0, 7);
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
      MapObjectFill(map, alleysRect, (Func<Point, MapObject>) (pt =>
      {
        if (!horizontalAlleys ? (pt.X - alleysRect.Left) % 2 == 1 && pt.Y != centralAlley : (pt.Y - alleysRect.Top) % 2 == 1 && pt.X != centralAlley)
          return MakeObjShelf();
        return null;
      }));
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      if (horizontalAlleys) {
        if (m_DiceRoller.RollChance(50)) {
          PlaceDoor(map, b.BuildingRect.Left, y1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8) {
            PlaceDoor(map, b.BuildingRect.Left, y1 - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              PlaceDoor(map, b.BuildingRect.Left, y1 + 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          }
        } else {
          PlaceDoor(map, b.BuildingRect.Right - 1, y1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8) {
            PlaceDoor(map, b.BuildingRect.Right - 1, y1 - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              PlaceDoor(map, b.BuildingRect.Right - 1, y1 + 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          }
        }
      } else if (m_DiceRoller.RollChance(50)) {
        PlaceDoor(map, x1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8) {
          PlaceDoor(map, x1 - 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            PlaceDoor(map, x1 + 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        }
      } else {
        PlaceDoor(map, x1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8) {
          PlaceDoor(map, x1 - 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            PlaceDoor(map, x1 + 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        }
      }
      string basename;
      string shopImage;
      switch (shopType) {
        case BaseTownGenerator.ShopType._FIRST:
          shopImage = GameImages.DECO_SHOP_GENERAL_STORE;
          basename = "GeneralStore";
          break;
        case BaseTownGenerator.ShopType.GROCERY:
          shopImage = GameImages.DECO_SHOP_GROCERY;
          basename = "Grocery";
          break;
        case BaseTownGenerator.ShopType.SPORTSWEAR:
          shopImage = GameImages.DECO_SHOP_SPORTSWEAR;
          basename = "Sportswear";
          break;
        case BaseTownGenerator.ShopType.PHARMACY:
          shopImage = GameImages.DECO_SHOP_PHARMACY;
          basename = "Pharmacy";
          break;
        case BaseTownGenerator.ShopType.CONSTRUCTION:
          shopImage = GameImages.DECO_SHOP_CONSTRUCTION;
          basename = "Construction";
          break;
        case BaseTownGenerator.ShopType.GUNSHOP:
          shopImage = GameImages.DECO_SHOP_GUNSHOP;
          basename = "Gunshop";
          break;
        case BaseTownGenerator.ShopType.HUNTING:
          shopImage = GameImages.DECO_SHOP_HUNTING;
          basename = "Hunting Shop";
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled shoptype");
      }
      DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.GetMapObjectAt(x, y) != null || CountAdjDoors(map, x, y) < 1) return null;
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
        if (!map.GetTileAt(x2, y2).Model.IsWalkable)
          PlaceDoor(map, x2, y2, m_Game.GameTiles.FLOOR_TILES, MakeObjWindow());
      }
      if (shopType == BaseTownGenerator.ShopType.GUNSHOP)
        BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      ItemsDrop(map, b.InsideRect, (Func<Point, bool>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || !(mapObjectAt.ImageID == "MapObjects\\shop_shelf")) return false;    // XXX leave unconverted as a red flag for ImageID abuse
        return m_DiceRoller.RollChance(m_Params.ItemInShopShelfChance);
      }), (Func<Point, Item>) (pt => MakeRandomShopItem(shopType)));
      map.AddZone(MakeUniqueZone(basename, b.BuildingRect));
      MakeWalkwayZones(map, b);
      if (m_DiceRoller.RollChance(SHOP_BASEMENT_CHANCE)) {
        int seed = map.Seed << 1 ^ basename.GetHashCode();
        string name = "basement-" + basename;
        rectangle = b.BuildingRect;
        int width = rectangle.Width;
        int height = rectangle.Height;
        Map shopBasement = new Map(seed, name, width, height) {
          Lighting = Lighting.DARKNESS,
          District = map.District
        };
        DoForEachTile(shopBasement.Rect, (Action<Point>) (pt => shopBasement.GetTileAt(pt).IsInside = true));
        TileFill(shopBasement, m_Game.GameTiles.FLOOR_CONCRETE);
        TileRectangle(shopBasement, m_Game.GameTiles.WALL_BRICK, shopBasement.Rect);
        shopBasement.AddZone(MakeUniqueZone("basement", shopBasement.Rect));
        DoForEachTile(shopBasement.Rect, (Action<Point>) (pt =>
        {
          if (!shopBasement.IsWalkable(pt.X, pt.Y) || shopBasement.GetExitAt(pt) != null) return;
          if (m_DiceRoller.RollChance(SHOP_BASEMENT_SHELF_CHANCE_PER_TILE)) {
            shopBasement.PlaceMapObjectAt(MakeObjShelf(), pt);
            if (m_DiceRoller.RollChance(SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF)) {
              Item it = MakeRandomShopItem(shopType);
              if (it != null) shopBasement.DropItemAt(it, pt);
            }
          }
          if (!Session.Get.HasZombiesInBasements || !m_DiceRoller.RollChance(SHOP_BASEMENT_ZOMBIE_RAT_CHANCE)) return;
          shopBasement.PlaceActorAt(CreateNewBasementRatZombie(0), pt);
        }));
        Point point1 = new Point((m_DiceRoller.RollChance(50) ? 1 : shopBasement.Width - 2),(m_DiceRoller.RollChance(50) ? 1 : shopBasement.Height - 2));
        rectangle = b.InsideRect;
        Point point2 = new Point(point1.X - 1 + rectangle.Left, point1.Y - 1 + rectangle.Top);
        AddExit(shopBasement, point1, map, point2, GameImages.DECO_STAIRS_UP, true);
        AddExit(map, point2, shopBasement, point1, GameImages.DECO_STAIRS_DOWN, true);
        if (map.GetMapObjectAt(point2) != null) map.RemoveMapObjectAt(point2.X, point2.Y);
        m_Params.District.AddUniqueMap(shopBasement);
      }
      return true;
    }

    protected virtual BaseTownGenerator.CHARBuildingType MakeCHARBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 8 || b.InsideRect.Height < 8)
        return MakeCHARAgency(map, b) ? BaseTownGenerator.CHARBuildingType.AGENCY : BaseTownGenerator.CHARBuildingType.NONE;
      return MakeCHAROffice(map, b) ? BaseTownGenerator.CHARBuildingType.OFFICE : BaseTownGenerator.CHARBuildingType.NONE;
    }

    protected virtual bool MakeCHARAgency(Map map, BaseTownGenerator.Block b)
    {
      TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
      TileFill(map, m_Game.GameTiles.FLOOR_OFFICE, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        tile.IsInside = true;
        tile.AddDecoration("Tiles\\Decoration\\char_floor_logo");
      }));
      bool flag = b.InsideRect.Width >= b.InsideRect.Height;
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      if (flag)
      {
        if (m_DiceRoller.RollChance(50))
        {
                    PlaceDoor(map, b.BuildingRect.Left, y1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
                        PlaceDoor(map, b.BuildingRect.Left, y1 - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Left, y1 + 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          }
        }
        else
        {
                    PlaceDoor(map, b.BuildingRect.Right - 1, y1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
                        PlaceDoor(map, b.BuildingRect.Right - 1, y1 - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Right - 1, y1 + 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          }
        }
      }
      else if (m_DiceRoller.RollChance(50))
      {
                PlaceDoor(map, x1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
                    PlaceDoor(map, x1 - 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
                        PlaceDoor(map, x1 + 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        }
      }
      else
      {
                PlaceDoor(map, x1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
                    PlaceDoor(map, x1 - 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
                        PlaceDoor(map, x1 + 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        }
      }
      string officeImage = "Tiles\\Decoration\\char_office";
            DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.GetMapObjectAt(x, y) != null || CountAdjDoors(map, x, y) < 1)
          return (string) null;
        return officeImage;
      }));
            MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3)
          return (MapObject) null;
        return MakeObjChair("MapObjects\\char_chair");
      }));
            TileFill(map, m_Game.GameTiles.WALL_CHAR_OFFICE, new Rectangle(b.InsideRect.Left + b.InsideRect.Width / 2 - 1, b.InsideRect.Top + b.InsideRect.Height / 2 - 1, 3, 2), (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration(BaseTownGenerator.CHAR_POSTERS[m_DiceRoller.Roll(0, BaseTownGenerator.CHAR_POSTERS.Length)])));
            DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (CountAdjDoors(map, x, y) > 0)
          return (string) null;
        if (m_DiceRoller.RollChance(25))
          return BaseTownGenerator.CHAR_POSTERS[m_DiceRoller.Roll(0, BaseTownGenerator.CHAR_POSTERS.Length)];
        return (string) null;
      }));
      map.AddZone(MakeUniqueZone("CHAR Agency", b.BuildingRect));
            MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeCHAROffice(Map map, BaseTownGenerator.Block b)
    {
            TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
            TileFill(map, m_Game.GameTiles.FLOOR_OFFICE, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) => tile.IsInside = true));
      Rectangle rectangle1 = b.InsideRect;
      bool flag = rectangle1.Width >= b.InsideRect.Height;
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      Direction direction;
      if (flag)
      {
        if (m_DiceRoller.RollChance(50))
        {
          direction = Direction.W;
                    PlaceDoor(map, b.BuildingRect.Left, y1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
                        PlaceDoor(map, b.BuildingRect.Left, y1 - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
                            PlaceDoor(map, b.BuildingRect.Left, y1 + 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          }
        }
        else
        {
          direction = Direction.E;
                    PlaceDoor(map, b.BuildingRect.Right - 1, y1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
                        PlaceDoor(map, b.BuildingRect.Right - 1, y1 - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              PlaceDoor(map, b.BuildingRect.Right - 1, y1 + 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          }
        }
      } else if (m_DiceRoller.RollChance(50)) {
        direction = Direction.N;
        PlaceDoor(map, x1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8) {
          PlaceDoor(map, x1 - 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            PlaceDoor(map, x1 + 1, b.BuildingRect.Top, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        }
      } else {
        direction = Direction.S;
        PlaceDoor(map, x1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8) {
          PlaceDoor(map, x1 - 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            PlaceDoor(map, x1 + 1, b.BuildingRect.Bottom - 1, m_Game.GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor());
        }
      }
      string officeImage = "Tiles\\Decoration\\char_office";
            DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.GetMapObjectAt(x, y) != null || CountAdjDoors(map, x, y) < 1) return null;
        return officeImage;
      }));
      BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      if (direction == Direction.N)
        TileHLine(map, m_Game.GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left, b.InsideRect.Top + 3, b.InsideRect.Width);
      else if (direction == Direction.S) {
        Map map1 = map;
        TileModel wallCharOffice = m_Game.GameTiles.WALL_CHAR_OFFICE;
        int left = b.InsideRect.Left;
        int top = b.InsideRect.Bottom - 1 - 3;
        rectangle1 = b.InsideRect;
        int width = rectangle1.Width;
                TileHLine(map1, wallCharOffice, left, top, width);
      } else if (direction == Direction.E) {
        Map map1 = map;
        TileModel wallCharOffice = m_Game.GameTiles.WALL_CHAR_OFFICE;
        rectangle1 = b.InsideRect;
        int left = rectangle1.Right - 1 - 3;
        rectangle1 = b.InsideRect;
        int top = rectangle1.Top;
        rectangle1 = b.InsideRect;
        int height = rectangle1.Height;
                TileVLine(map1, wallCharOffice, left, top, height);
      } else {
        if (direction != Direction.W) throw new InvalidOperationException("unhandled door side");
        Map map1 = map;
        TileModel wallCharOffice = m_Game.GameTiles.WALL_CHAR_OFFICE;
        rectangle1 = b.InsideRect;
        int left = rectangle1.Left + 3;
        rectangle1 = b.InsideRect;
        int top = rectangle1.Top;
        rectangle1 = b.InsideRect;
        int height = rectangle1.Height;
                TileVLine(map1, wallCharOffice, left, top, height);
      }
      Rectangle rect1;
      Point point;
      if (direction == Direction.N)
      {
        int x2 = x1 - 1;
        rectangle1 = b.InsideRect;
        int y2 = rectangle1.Top + 3;
        rectangle1 = b.BuildingRect;
        int height = rectangle1.Height - 1 - 3;
        rect1 = new Rectangle(x2, y2, 3, height);
        point = new Point(rect1.Left + 1, rect1.Top);
      }
      else if (direction == Direction.S)
      {
        int x2 = x1 - 1;
        rectangle1 = b.BuildingRect;
        int top = rectangle1.Top;
        rectangle1 = b.BuildingRect;
        int height = rectangle1.Height - 1 - 3;
        rect1 = new Rectangle(x2, top, 3, height);
        point = new Point(rect1.Left + 1, rect1.Bottom - 1);
      }
      else if (direction == Direction.E)
      {
        rectangle1 = b.BuildingRect;
        int left = rectangle1.Left;
        int y2 = y1 - 1;
        rectangle1 = b.BuildingRect;
        int width = rectangle1.Width - 1 - 3;
        rect1 = new Rectangle(left, y2, width, 3);
        point = new Point(rect1.Right - 1, rect1.Top + 1);
      }
      else
      {
        if (direction != Direction.W)
          throw new InvalidOperationException("unhandled door side");
        rectangle1 = b.InsideRect;
        int x2 = rectangle1.Left + 3;
        int y2 = y1 - 1;
        rectangle1 = b.BuildingRect;
        int width = rectangle1.Width - 1 - 3;
        rect1 = new Rectangle(x2, y2, width, 3);
        point = new Point(rect1.Left, rect1.Top + 1);
      }
            TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, rect1);
            PlaceDoor(map, point.X, point.Y, m_Game.GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      Rectangle rect2;
      Rectangle rect3;
      rectangle1 = b.BuildingRect;
      if (flag)
      {
        int left = rect1.Left;
        int top = rectangle1.Top;
        int width = rect1.Width;
        int num1 = 1 + rect1.Top;
        int height1 = num1 - top;
        rect2 = new Rectangle(left, top, width, height1);
        int y2 = rect1.Bottom - 1;
        int bottom = rectangle1.Bottom;
        int height2 = 1 + bottom - rect1.Bottom;
        rect3 = new Rectangle(left, y2, width, height2);
      }
      else
      {
        int left = rectangle1.Left;
        int top = rect1.Top;
        int num1 = 1 + rect1.Left;
        int width1 = num1 - left;
        int height = rect1.Height;
        rect2 = new Rectangle(left, top, width1, height);
        int x2 = rect1.Right - 1;
        int right = rectangle1.Right;
        int width2 = 1 + right - rect1.Right;
        rect3 = new Rectangle(x2, top, width2, height);
      }
      List<Rectangle> list1 = new List<Rectangle>();
            MakeRoomsPlan(map, ref list1, rect2, 4);
      List<Rectangle> list2 = new List<Rectangle>();
            MakeRoomsPlan(map, ref list2, rect3, 4);
      List<Rectangle> rectangleList = new List<Rectangle>(list1.Count + list2.Count);
      rectangleList.AddRange((IEnumerable<Rectangle>) list1);
      rectangleList.AddRange((IEnumerable<Rectangle>) list2);
      foreach (Rectangle rect4 in list1)
      {
                TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, rect4);
        map.AddZone(MakeUniqueZone("Office room", rect4));
      }
      foreach (Rectangle rect4 in list2)
      {
                TileRectangle(map, m_Game.GameTiles.WALL_CHAR_OFFICE, rect4);
        map.AddZone(MakeUniqueZone("Office room", rect4));
      }
      foreach (Rectangle rectangle2 in list1)
      {
        if (flag)
                    PlaceDoor(map, rectangle2.Left + rectangle2.Width / 2, rectangle2.Bottom - 1, m_Game.GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
        else
                    PlaceDoor(map, rectangle2.Right - 1, rectangle2.Top + rectangle2.Height / 2, m_Game.GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      foreach (Rectangle rectangle2 in list2)
      {
        if (flag)
                    PlaceDoor(map, rectangle2.Left + rectangle2.Width / 2, rectangle2.Top, m_Game.GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
        else
                    PlaceDoor(map, rectangle2.Left, rectangle2.Top + rectangle2.Height / 2, m_Game.GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      foreach (Rectangle rectangle2 in rectangleList)
      {
        Point tablePos = new Point(rectangle2.Left + rectangle2.Width / 2, rectangle2.Top + rectangle2.Height / 2);
        map.PlaceMapObjectAt(MakeObjTable("MapObjects\\char_table"), tablePos);
        int num = 2;
        Rectangle rect4 = new Rectangle(rectangle2.Left + 1, rectangle2.Top + 1, rectangle2.Width - 2, rectangle2.Height - 2);
        if (!rect4.IsEmpty)
        {
          for (int index = 0; index < num; ++index)
          {
            Rectangle rect5 = new Rectangle(tablePos.X - 1, tablePos.Y - 1, 3, 3);
            rect5.Intersect(rect4);
                        MapObjectPlaceInGoodPosition(map, rect5, (Func<Point, bool>) (pt => pt != tablePos), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjChair("MapObjects\\char_chair")));
          }
        }
      }
      foreach (Rectangle rect4 in rectangleList)
                ItemsDrop(map, rect4, (Func<Point, bool>) (pt => map.GetTileAt(pt.X, pt.Y).Model == m_Game.GameTiles.FLOOR_OFFICE && map.GetMapObjectAt(pt) == null), (Func<Point, Item>) (pt => MakeRandomCHAROfficeItem()));
      Zone zone = MakeUniqueZone("CHAR Office", b.BuildingRect);
      zone.SetGameAttribute<bool>("CHAR Office", true);
      map.AddZone(zone);
            MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeParkBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 3 || b.InsideRect.Height < 3)
        return false;
            TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            TileFill(map, m_Game.GameTiles.FLOOR_GRASS, b.InsideRect);
            MapObjectFill(map, b.BuildingRect, (Func<Point, MapObject>) (pt =>
      {
        if (pt.X == b.BuildingRect.Left || pt.X == b.BuildingRect.Right - 1 || pt.Y == b.BuildingRect.Top || pt.Y == b.BuildingRect.Bottom - 1)
          return MakeObjFence("MapObjects\\fence");
        return (MapObject) null;
      }));
            MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (m_DiceRoller.RollChance(PARK_TREE_CHANCE))
          return MakeObjTree("MapObjects\\tree");
        return null;
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
      map.SetTileModelAt(x, y, m_Game.GameTiles.FLOOR_WALKWAY);
      ItemsDrop(map, b.InsideRect, (Func<Point, bool>) (pt =>
      {
        if (map.GetMapObjectAt(pt) == null)
          return m_DiceRoller.RollChance(PARK_ITEM_CHANCE);
        return false;
      }), (Func<Point, Item>) (pt => MakeRandomParkItem()));
      map.AddZone(MakeUniqueZone("Park", b.BuildingRect));
            MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeHousingBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 4 || b.InsideRect.Height < 4)
        return false;
            TileRectangle(map, m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
            TileRectangle(map, m_Game.GameTiles.WALL_BRICK, b.BuildingRect);
            TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) => tile.IsInside = true));
      List<Rectangle> list = new List<Rectangle>();
            MakeRoomsPlan(map, ref list, b.BuildingRect, 5);
      foreach (Rectangle roomRect in list)
      {
                MakeHousingRoom(map, roomRect, m_Game.GameTiles.FLOOR_PLANKS, m_Game.GameTiles.WALL_BRICK);
                FillHousingRoomContents(map, roomRect);
      }
      bool flag = false;
      for (int left = b.BuildingRect.Left; left < b.BuildingRect.Right && !flag; ++left)
      {
        for (int top = b.BuildingRect.Top; top < b.BuildingRect.Bottom && !flag; ++top)
        {
          if (!map.GetTileAt(left, top).IsInside)
          {
            DoorWindow doorWindow = map.GetMapObjectAt(left, top) as DoorWindow;
            if (doorWindow != null && !doorWindow.IsWindow)
              flag = true;
          }
        }
      }
      if (!flag)
      {
        do
        {
          int x = m_DiceRoller.Roll(b.BuildingRect.Left, b.BuildingRect.Right);
          int y = m_DiceRoller.Roll(b.BuildingRect.Top, b.BuildingRect.Bottom);
          if (!map.GetTileAt(x, y).IsInside)
          {
            DoorWindow doorWindow = map.GetMapObjectAt(x, y) as DoorWindow;
            if (doorWindow != null && doorWindow.IsWindow)
            {
              map.RemoveMapObjectAt(x, y);
              map.PlaceMapObjectAt((MapObject)MakeObjWoodenDoor(), new Point(x, y));
              flag = true;
            }
          }
        }
        while (!flag);
      }
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_CHANCE))
                m_Params.District.AddUniqueMap(GenerateHouseBasementMap(map, b));
      map.AddZone(MakeUniqueZone("Housing", b.BuildingRect));
            MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual void MakeSewersMaintenanceBuilding(Map map, bool isSurface, BaseTownGenerator.Block b, Map linkedMap, Point exitPosition)
    {
      if (!isSurface)
                TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);
            TileRectangle(map, m_Game.GameTiles.WALL_SEWER, b.BuildingRect);
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left)
      {
        for (int top = b.InsideRect.Top; top < b.InsideRect.Bottom; ++top)
          map.GetTileAt(left, top).IsInside = true;
      }
      Direction direction;
      int x;
      int y;
      switch (m_DiceRoller.Roll(0, 4))
      {
        case 0:
          direction = Direction.N;
          x = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          y = b.BuildingRect.Top;
          map.GetTileAt(x - 1, y).AddDecoration("Tiles\\Decoration\\sewers_building");
          map.GetTileAt(x + 1, y).AddDecoration("Tiles\\Decoration\\sewers_building");
          break;
        case 1:
          direction = Direction.S;
          x = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          y = b.BuildingRect.Bottom - 1;
          map.GetTileAt(x - 1, y).AddDecoration("Tiles\\Decoration\\sewers_building");
          map.GetTileAt(x + 1, y).AddDecoration("Tiles\\Decoration\\sewers_building");
          break;
        case 2:
          direction = Direction.W;
          x = b.BuildingRect.Left;
          y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          map.GetTileAt(x, y - 1).AddDecoration("Tiles\\Decoration\\sewers_building");
          map.GetTileAt(x, y + 1).AddDecoration("Tiles\\Decoration\\sewers_building");
          break;
        case 3:
          direction = Direction.E;
          x = b.BuildingRect.Right - 1;
          y = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          map.GetTileAt(x, y - 1).AddDecoration("Tiles\\Decoration\\sewers_building");
          map.GetTileAt(x, y + 1).AddDecoration("Tiles\\Decoration\\sewers_building");
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
      PlaceDoor(map, x, y, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
      BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      map.GetTileAt(exitPosition.X, exitPosition.Y).AddDecoration(isSurface ? "Tiles\\Decoration\\sewer_hole" : "Tiles\\Decoration\\sewer_ladder");
      map.SetExitAt(exitPosition, new Exit(linkedMap, exitPosition)
      {
        IsAnAIExit = true
      });
      if (!isSurface) {
        Point p = new Point(x, y) + direction;
        while (map.IsInBounds(p) && !map.GetTileAt(p.X, p.Y).Model.IsWalkable) {
          map.SetTileModelAt(p.X, p.Y, m_Game.GameTiles.FLOOR_CONCRETE);
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
          return MakeObjTable("MapObjects\\table");
        }));
      if (m_DiceRoller.RollChance(33)) {
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjBed("MapObjects\\bed")));
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt.X, pt.Y) >= 3 && CountAdjDoors(map, pt.X, pt.Y) == 0;
        }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          map.DropItemAt(MakeItemCannedFood(), pt);
          return MakeObjFridge();
        }));
      }
      Actor newCivilian = CreateNewCivilian(0, RogueGame.REFUGEES_WAVE_ITEMS, 1);
      ActorPlace(m_DiceRoller, b.Rectangle.Width * b.Rectangle.Height, map, newCivilian, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
      map.AddZone(MakeUniqueZone("Sewers Maintenance", b.BuildingRect));
    }

    protected virtual void MakeSubwayStationBuilding(Map map, bool isSurface, BaseTownGenerator.Block b, Map linkedMap, Point exitPosition)
    {
      if (!isSurface) TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);
      TileRectangle(map, m_Game.GameTiles.WALL_SUBWAY, b.BuildingRect);
      DoForEachTile(b.InsideRect,(Action<Point>)(pt => { map.GetTileAt(pt.X, pt.Y).IsInside = true; }));
      DoForEachTile(b.InsideRect,(Action<Point>)(pt => {
          map.GetTileAt(pt.X, pt.Y).IsInside = true;
          Session.Get.ForcePoliceKnown(new Location(map, pt));
      }));
      Direction direction;
      int x1;
      int num;
      switch (!isSurface ? (b.Rectangle.Bottom < map.Width / 2 ? 1 : 0) : m_DiceRoller.Roll(0, 4)) {
        case 0:
          direction = Direction.N;
          x1 = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          num = b.BuildingRect.Top;
          if (isSurface) {
            map.GetTileAt(x1 - 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1 + 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        case 1:
          direction = Direction.S;
          x1 = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          num = b.BuildingRect.Bottom - 1;
          if (isSurface) {
            map.GetTileAt(x1 - 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1 + 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        case 2:
          direction = Direction.W;
          x1 = b.BuildingRect.Left;
          num = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          if (isSurface) {
            map.GetTileAt(x1, num - 1).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1, num + 1).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        case 3:
          direction = Direction.E;
          x1 = b.BuildingRect.Right - 1;
          num = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          if (isSurface) {
            map.GetTileAt(x1, num - 1).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1, num + 1).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
      if (isSurface) {
        map.SetTileModelAt(x1, num, m_Game.GameTiles.FLOOR_CONCRETE);
        map.PlaceMapObjectAt((MapObject)MakeObjGlassDoor(), new Point(x1, num));
      }
      for (int x2 = exitPosition.X - 1; x2 <= exitPosition.X + 1; ++x2) {
        Point point = new Point(x2, exitPosition.Y);
        map.GetTileAt(point.X, point.Y).AddDecoration(isSurface ? "Tiles\\Decoration\\stairs_down" : "Tiles\\Decoration\\stairs_up");
        map.SetExitAt(point, new Exit(linkedMap, point)
        {
          IsAnAIExit = true
        });
      }
      if (!isSurface) {
        map.SetTileModelAt(x1, num, m_Game.GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(x1 + 1, num, m_Game.GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(x1 - 1, num, m_Game.GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(x1 - 2, num, m_Game.GameTiles.WALL_STONE);
        map.SetTileModelAt(x1 + 2, num, m_Game.GameTiles.WALL_STONE);
        DoForEachTile(new Rectangle(x1-2,num,5,1),(Action<Point>)(pt => {
          Session.Get.ForcePoliceKnown(new Location(map, pt));
        }));
        Point p = new Point(x1, num) + direction;
        while (map.IsInBounds(p) && !map.GetTileAt(p.X, p.Y).Model.IsWalkable) {
          map.SetTileModelAt(p.X, p.Y, m_Game.GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X - 1, p.Y, m_Game.GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X + 1, p.Y, m_Game.GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X - 2, p.Y, m_Game.GameTiles.WALL_STONE);
          map.SetTileModelAt(p.X + 2, p.Y, m_Game.GameTiles.WALL_STONE);
          DoForEachTile(new Rectangle(p.X - 2, p.Y, 5,1),(Action<Point>)(pt => {
            Session.Get.ForcePoliceKnown(new Location(map, pt));
          }));
          p += direction;
        }
        int left1 = Math.Max(0, b.BuildingRect.Left - 10);
        int right = Math.Min(map.Width - 1, b.BuildingRect.Right + 10);
        Rectangle rect1;
        int y;
        if (direction == Direction.S) {
          rect1 = Rectangle.FromLTRB(left1, p.Y - 3, right, p.Y);
          y = rect1.Top;
          map.AddZone(MakeUniqueZone("corridor", Rectangle.FromLTRB(x1 - 1, num, x1 + 1 + 1, rect1.Top)));
        } else {
          rect1 = Rectangle.FromLTRB(left1, p.Y + 1, right, p.Y + 1 + 3);
          y = rect1.Bottom - 1;
          map.AddZone(MakeUniqueZone("corridor", Rectangle.FromLTRB(x1 - 1, rect1.Bottom, x1 + 1 + 1, num + 1)));
        }
        TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, rect1);
        for (int left2 = rect1.Left; left2 < rect1.Right; ++left2) {
          if (CountAdjWalls(map, left2, y) >= 3)
            map.PlaceMapObjectAt(MakeObjIronBench(), new Point(left2, y));
        }
        map.AddZone(MakeUniqueZone("platform", rect1));
        Point point1 = direction != Direction.S ? new Point(x1, rect1.Bottom) : new Point(x1, rect1.Top - 1);
        map.PlaceMapObjectAt(MakeObjIronGate(), new Point(point1.X, point1.Y));
        map.PlaceMapObjectAt(MakeObjIronGate(), new Point(point1.X + 1, point1.Y));
        map.PlaceMapObjectAt(MakeObjIronGate(), new Point(point1.X - 1, point1.Y));
        Point point2;
        Rectangle rect2;
        if (x1 > map.Width / 2) {
          point2 = new Point(x1 - 2, num + 2 * direction.Vector.Y);
          rect2 = Rectangle.FromLTRB(point2.X - 4, point2.Y - 2, point2.X + 1, point2.Y + 2 + 1);
        } else {
          point2 = new Point(x1 + 2, num + 2 * direction.Vector.Y);
          rect2 = Rectangle.FromLTRB(point2.X, point2.Y - 2, point2.X + 4, point2.Y + 2 + 1);
        }
        TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, rect2);
        TileRectangle(map, m_Game.GameTiles.WALL_STONE, rect2);
        PlaceDoor(map, point2.X, point2.Y, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
        map.GetTileAt(point2.X, point2.Y - 1).AddDecoration("Tiles\\Decoration\\power_sign_big");
        map.GetTileAt(point2.X, point2.Y + 1).AddDecoration("Tiles\\Decoration\\power_sign_big");
        MapObjectFill(map, rect2, (Func<Point, MapObject>) (pt =>
        {
          if (!map.GetTileAt(pt).Model.IsWalkable) return null;
          if (CountAdjWalls(map, pt.X, pt.Y) < 3 || CountAdjDoors(map, pt.X, pt.Y) > 0) return null;
          return MakeObjPowerGenerator();
        }));
        DoForEachTile(rect2, (Action<Point>)(pt => {
          Session.Get.ForcePoliceKnown(new Location(map, pt));
        }));
      }
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left) {
        for (int y = b.InsideRect.Top + 1; y < b.InsideRect.Bottom - 1; ++y) {
          if (CountAdjWalls(map, left, y) >= 2 && CountAdjDoors(map, left, y) <= 0 && Rules.GridDistance(new Point(left, y), new Point(x1, num)) >= 2)
            map.PlaceMapObjectAt(MakeObjIronBench(), new Point(left, y));
        }
      }
      if (isSurface) {
        Actor newPoliceman = CreateNewPoliceman(0);
        if (Session.Get.CMDoptionExists("subway-cop")) {
          int home_district_xy = Session.Get.World.Size/2;
          if (map.District.WorldPosition == new Point(home_district_xy, home_district_xy)) newPoliceman.Controller = new PlayerController();
        }
        ActorPlace(m_DiceRoller, b.Rectangle.Width * b.Rectangle.Height, map, newPoliceman, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
      }
      map.AddZone(MakeUniqueZone("Subway Station", b.BuildingRect));
    }

    protected virtual void MakeRoomsPlan(Map map, ref List<Rectangle> list, Rectangle rect, int minRoomsSize)
    {
      int splitX;
      int splitY;
      Rectangle topLeft;
      Rectangle topRight;
      Rectangle bottomLeft;
      Rectangle bottomRight;
      QuadSplit(rect, minRoomsSize, minRoomsSize, out splitX, out splitY, out topLeft, out topRight, out bottomLeft, out bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty) {
        list.Add(rect);
      } else {
        MakeRoomsPlan(map, ref list, topLeft, minRoomsSize);
        if (!topRight.IsEmpty)
        {
          topRight.Offset(-1, 0);
          ++topRight.Width;
                    MakeRoomsPlan(map, ref list, topRight, minRoomsSize);
        }
        if (!bottomLeft.IsEmpty)
        {
          bottomLeft.Offset(0, -1);
          ++bottomLeft.Height;
                    MakeRoomsPlan(map, ref list, bottomLeft, minRoomsSize);
        }
        if (bottomRight.IsEmpty)
          return;
        bottomRight.Offset(-1, -1);
        ++bottomRight.Width;
        ++bottomRight.Height;
                MakeRoomsPlan(map, ref list, bottomRight, minRoomsSize);
      }
    }

    protected virtual void MakeHousingRoom(Map map, Rectangle roomRect, TileModel floor, TileModel wall)
    {
            TileFill(map, floor, roomRect);
            TileRectangle(map, wall, roomRect.Left, roomRect.Top, roomRect.Width, roomRect.Height, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        if (map.GetMapObjectAt(x, y) == null)
          return;
        map.SetTileModelAt(x, y, floor);
      }));
      int x1 = roomRect.Left + roomRect.Width / 2;
      int y1 = roomRect.Top + roomRect.Height / 2;
            PlaceIf(map, x1, roomRect.Top, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (HasNoObjectAt(map, x, y) && IsAccessible(map, x, y))
          return CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!IsInside(map, x, y) && !m_DiceRoller.RollChance(25))
          return (MapObject)MakeObjWindow();
        return (MapObject)MakeObjWoodenDoor();
      }));
            PlaceIf(map, x1, roomRect.Bottom - 1, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (HasNoObjectAt(map, x, y) && IsAccessible(map, x, y))
          return CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!IsInside(map, x, y) && !m_DiceRoller.RollChance(25))
          return (MapObject)MakeObjWindow();
        return (MapObject)MakeObjWoodenDoor();
      }));
            PlaceIf(map, roomRect.Left, y1, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (HasNoObjectAt(map, x, y) && IsAccessible(map, x, y))
          return CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!IsInside(map, x, y) && !m_DiceRoller.RollChance(25))
          return (MapObject)MakeObjWindow();
        return (MapObject)MakeObjWoodenDoor();
      }));
            PlaceIf(map, roomRect.Right - 1, y1, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (HasNoObjectAt(map, x, y) && IsAccessible(map, x, y))
          return CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!IsInside(map, x, y) && !m_DiceRoller.RollChance(25))
          return (MapObject)MakeObjWindow();
        return (MapObject)MakeObjWoodenDoor();
      }));
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
                Item it = MakeRandomBedroomItem();
                if (it != null) map.DropItemAt(it, pt2);
                return MakeObjNightTable("MapObjects\\nighttable");
              }));
              return MakeObjBed("MapObjects\\bed");
            }));
          int num2 = m_DiceRoller.Roll(1, 4);
          for (int index = 0; index < num2; ++index)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              Item it = MakeRandomBedroomItem();
              if (it != null) map.DropItemAt(it, pt);
              return (m_DiceRoller.RollChance(50) ? MakeObjWardrobe("MapObjects\\wardrobe") : MakeObjDrawer());
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
                Item it = MakeRandomKitchenItem();
                if (it != null) map.DropItemAt(it, pt);
              }
              Rectangle rect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
              rect.Intersect(insideRoom);
              MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                return pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0;
              }), m_DiceRoller, (Func<Point, MapObject>) (pt2 => MakeObjChair("MapObjects\\chair")));
              return MakeObjTable("MapObjects\\table");
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
              Item it = MakeRandomKitchenItem();
              if (it != null) map.DropItemAt(it, pt);
            }
            MapObjectPlaceInGoodPosition(map, new Rectangle(pt.X - 1, pt.Y - 1, 3, 3), (Func<Point, bool>) (pt2 =>
            {
              return pt2 != pt && CountAdjDoors(map, pt2.X, pt2.Y) == 0;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt2 => MakeObjChair("MapObjects\\chair")));
            return MakeObjTable("MapObjects\\table");
          }));
          MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            return CountAdjWalls(map, pt.X, pt.Y) >= 2 && CountAdjDoors(map, pt.X, pt.Y) == 0;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_IN_FRIDGE; ++index) {
              Item it = MakeRandomKitchenItem();
              if (it != null) map.DropItemAt(it, pt);
            }
            return MakeObjFridge();
          }));
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    protected Item MakeRandomShopItem(BaseTownGenerator.ShopType shop)
    {
      switch (shop)
      {
        case BaseTownGenerator.ShopType._FIRST:
          return MakeShopGeneralItem();
        case BaseTownGenerator.ShopType.GROCERY:
          return MakeShopGroceryItem();
        case BaseTownGenerator.ShopType.SPORTSWEAR:
          return MakeShopSportsWearItem();
        case BaseTownGenerator.ShopType.PHARMACY:
          return MakeShopPharmacyItem();
        case BaseTownGenerator.ShopType.CONSTRUCTION:
          return MakeShopConstructionItem();
        case BaseTownGenerator.ShopType.GUNSHOP:
          return MakeShopGunshopItem();
        case BaseTownGenerator.ShopType.HUNTING:
          return MakeHuntingShopItem();
        default:
          throw new ArgumentOutOfRangeException("unhandled shoptype");
      }
    }

    public Item MakeShopGroceryItem()
    {
      if (m_DiceRoller.RollChance(50))
        return MakeItemCannedFood();
      return MakeItemGroceries();
    }

    public Item MakeShopPharmacyItem()
    {
      switch (m_DiceRoller.Roll(0, 6))
      {
        case 0:
          return MakeItemBandages();
        case 1:
          return MakeItemMedikit();
        case 2:
          return MakeItemPillsSLP();
        case 3:
          return MakeItemPillsSTA();
        case 4:
          return MakeItemPillsSAN();
        case 5:
          return MakeItemStenchKiller();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeShopSportsWearItem()
    {
      switch (m_DiceRoller.Roll(0, 10))
      {
        case 0:
          if (m_DiceRoller.RollChance(30))
            return MakeItemHuntingRifle();
          return MakeItemLightRifleAmmo();
        case 1:
          if (m_DiceRoller.RollChance(30))
            return MakeItemHuntingCrossbow();
          return MakeItemBoltsAmmo();
        case 2:
        case 3:
        case 4:
        case 5:
          return MakeItemBaseballBat();
        case 6:
        case 7:
          return MakeItemIronGolfClub();
        case 8:
        case 9:
          return MakeItemGolfClub();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeShopConstructionItem()
    {
      switch (m_DiceRoller.Roll(0, 24))
      {
        case 0:
        case 1:
        case 2:
          if (!m_DiceRoller.RollChance(50))
            return MakeItemShortShovel();
          return MakeItemShovel();
        case 3:
        case 4:
        case 5:
          return MakeItemCrowbar();
        case 6:
        case 7:
        case 8:
          if (!m_DiceRoller.RollChance(50))
            return MakeItemSmallHammer();
          return MakeItemHugeHammer();
        case 9:
        case 10:
        case 11:
          return (Item)MakeItemWoodenPlank();
        case 12:
        case 13:
        case 14:
          return MakeItemFlashlight();
        case 15:
        case 16:
        case 17:
          return MakeItemBigFlashlight();
        case 18:
        case 19:
        case 20:
          return MakeItemSpikes();
        case 21:
        case 22:
        case 23:
          return MakeItemBarbedWire();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeShopGunshopItem()
    {
      if (m_DiceRoller.RollChance(40))
      {
        switch (m_DiceRoller.Roll(0, 4))
        {
          case 0:
            return MakeItemRandomPistol();
          case 1:
            return MakeItemShotgun();
          case 2:
            return MakeItemHuntingRifle();
          case 3:
            return MakeItemHuntingCrossbow();
          default:
            return (Item) null;
        }
      }
      else
      {
        switch (m_DiceRoller.Roll(0, 4))
        {
          case 0:
            return MakeItemLightPistolAmmo();
          case 1:
            return MakeItemShotgunAmmo();
          case 2:
            return MakeItemLightRifleAmmo();
          case 3:
            return MakeItemBoltsAmmo();
          default:
            return (Item) null;
        }
      }
    }

    public Item MakeHuntingShopItem()
    {
      if (m_DiceRoller.RollChance(50)) {
        if (m_DiceRoller.RollChance(40)) return (0 == m_DiceRoller.Roll(0, 2) ? MakeItemHuntingRifle() : MakeItemHuntingCrossbow());
        return (0 == m_DiceRoller.Roll(0, 2) ? MakeItemLightRifleAmmo() : MakeItemBoltsAmmo());
      }
      return (0 == m_DiceRoller.Roll(0, 2) ? (Item)MakeItemHunterVest() : (Item)MakeItemBearTrap());
    }

    public Item MakeShopGeneralItem()
    {
      switch (m_DiceRoller.Roll(0, 6))
      {
        case 0:
          return MakeShopPharmacyItem();
        case 1:
          return MakeShopSportsWearItem();
        case 2:
          return MakeShopConstructionItem();
        case 3:
          return MakeShopGroceryItem();
        case 4:
          return MakeHuntingShopItem();
        case 5:
          return MakeRandomBedroomItem();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeHospitalItem()
    {
      switch (m_DiceRoller.Roll(0, (Session.Get.HasInfection ? 7 : 6)))
      {
        case 0:
          return MakeItemBandages();
        case 1:
          return MakeItemMedikit();
        case 2:
          return MakeItemPillsSLP();
        case 3:
          return MakeItemPillsSTA();
        case 4:
          return MakeItemPillsSAN();
        case 5:
          return MakeItemStenchKiller();
        case 6:
          return MakeItemPillsAntiviral();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeRandomBedroomItem()
    {
      switch (m_DiceRoller.Roll(0, 24))
      {
        case 0:
        case 1:
          return MakeItemBandages();
        case 2:
          return MakeItemPillsSTA();
        case 3:
          return MakeItemPillsSLP();
        case 4:
          return MakeItemPillsSAN();
        case 5:
        case 6:
        case 7:
        case 8:
          return MakeItemBaseballBat();
        case 9:
          return MakeItemRandomPistol();
        case 10:
          if (m_DiceRoller.RollChance(30))
          {
            if (m_DiceRoller.RollChance(50))
              return MakeItemShotgun();
            return MakeItemHuntingRifle();
          }
          if (m_DiceRoller.RollChance(50))
            return MakeItemShotgunAmmo();
          return MakeItemLightRifleAmmo();
        case 11:
        case 12:
        case 13:
          return MakeItemCellPhone();
        case 14:
        case 15:
          return MakeItemFlashlight();
        case 16:
        case 17:
          return MakeItemLightPistolAmmo();
        case 18:
        case 19:
          return MakeItemStenchKiller();
        case 20:
          return MakeItemHunterVest();
        case 21:
        case 22:
        case 23:
          if (m_DiceRoller.RollChance(50))
            return MakeItemBook();
          return MakeItemMagazines();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeRandomKitchenItem()
    {
      if (m_DiceRoller.RollChance(50))
        return MakeItemCannedFood();
      return MakeItemGroceries();
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
        case 0:
          return MakeItemSprayPaint();
        case 1:
          return MakeItemBaseballBat();
        case 2:
          return MakeItemPillsSLP();
        case 3:
          return MakeItemPillsSTA();
        case 4:
          return MakeItemPillsSAN();
        case 5:
          return MakeItemFlashlight();
        case 6:
          return MakeItemCellPhone();
        case 7:
          return (Item)MakeItemWoodenPlank();
        default:
          throw new ArgumentOutOfRangeException("unhandled item roll");
      }
    }

    protected virtual void DecorateOutsideWallsWithPosters(Map map, Rectangle rect, int chancePerWall)
    {
            DecorateOutsideWalls(map, rect, (Func<int, int, string>) ((x, y) =>
      {
        if (m_DiceRoller.RollChance(chancePerWall))
          return BaseTownGenerator.POSTERS[m_DiceRoller.Roll(0, BaseTownGenerator.POSTERS.Length)];
        return (string) null;
      }));
    }

    protected virtual void DecorateOutsideWallsWithTags(Map map, Rectangle rect, int chancePerWall)
    {
      DecorateOutsideWalls(map, rect, (Func<int, int, string>) ((x, y) =>
      {
        if (m_DiceRoller.RollChance(chancePerWall))
          return BaseTownGenerator.TAGS[m_DiceRoller.Roll(0, BaseTownGenerator.TAGS.Length)];
        return null;
      }));
    }

    protected virtual void PopulateCHAROfficeBuilding(Map map, BaseTownGenerator.Block b)
    {
      for (int index = 0; index < MAX_CHAR_GUARDS_PER_OFFICE; ++index) {
        Actor newCharGuard = CreateNewCHARGuard(0);
        ActorPlace(m_DiceRoller, 100, map, newCharGuard, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
      }
    }

    private Map GenerateHouseBasementMap(Map map, BaseTownGenerator.Block houseBlock)
    {
      Contract.Requires(null!=map.District);
      Rectangle buildingRect = houseBlock.BuildingRect;
      Map basement = new Map(map.Seed << 1 + buildingRect.Left * map.Height + buildingRect.Top, string.Format("basement{0}{1}@{2}-{3}", (object)m_Params.District.WorldPosition.X, (object)m_Params.District.WorldPosition.Y, (object) (buildingRect.Left + buildingRect.Width / 2), (object) (buildingRect.Top + buildingRect.Height / 2)), buildingRect.Width, buildingRect.Height)
      {
        Lighting = Lighting.DARKNESS,
        District = map.District
      };
      basement.AddZone(MakeUniqueZone("basement", basement.Rect));
      TileFill(basement, m_Game.GameTiles.FLOOR_CONCRETE, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.IsInside = true));
      TileRectangle(basement, m_Game.GameTiles.WALL_BRICK, new Rectangle(0, 0, basement.Width, basement.Height));
      Point point = new Point();
      do {
        point.X = m_DiceRoller.Roll(buildingRect.Left, buildingRect.Right);
        point.Y = m_DiceRoller.Roll(buildingRect.Top, buildingRect.Bottom);
      }
      while (!map.GetTileAt(point.X, point.Y).Model.IsWalkable || map.GetMapObjectAt(point.X, point.Y) != null);
      Point basementStairs = new Point(point.X - buildingRect.Left, point.Y - buildingRect.Top);
      AddExit(map, point, basement, basementStairs, "Tiles\\Decoration\\stairs_down", true);
      AddExit(basement, basementStairs, map, point, "Tiles\\Decoration\\stairs_up", true);
      DoForEachTile(basement.Rect, (Action<Point>) (pt =>
      {
        if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_PILAR_CHANCE) || pt == basementStairs) return;
        basement.SetTileModelAt(pt.X, pt.Y, m_Game.GameTiles.WALL_BRICK);
      }));
      MapObjectFill(basement, basement.Rect, (Func<Point, MapObject>) (pt =>
      {
        if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_OBJECT_CHANCE_PER_TILE)) return null;
        if (basement.GetExitAt(pt) != null) return null;
        if (!basement.IsWalkable(pt.X, pt.Y)) return null;
        switch (m_DiceRoller.Roll(0, 5)) {
          case 0:
            return MakeObjJunk();
          case 1:
            return MakeObjBarrels();
          case 2:
            basement.DropItemAt(MakeShopConstructionItem(), pt);
            return MakeObjTable("MapObjects\\table");
          case 3:
            basement.DropItemAt(MakeShopConstructionItem(), pt);
            return MakeObjDrawer();
#if DEBUG
          case 4:
#else
          default:
#endif
            return MakeObjBed("MapObjects\\bed");
#if DEBUG
          default:
            throw new ArgumentOutOfRangeException("unhandled roll");
#endif
        }
      }));
      if (Session.Get.HasZombiesInBasements)
        DoForEachTile(basement.Rect, (Action<Point>) (pt =>
        {
          if (!basement.IsWalkable(pt.X, pt.Y) || basement.GetExitAt(pt) != null || !m_DiceRoller.RollChance(HOUSE_BASEMENT_ZOMBIE_RAT_CHANCE)) return;
          basement.PlaceActorAt(CreateNewBasementRatZombie(0), pt);
        }));
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_WEAPONS_CACHE_CHANCE))
        MapObjectPlaceInGoodPosition(basement, basement.Rect, (Func<Point, bool>) (pt => basement.GetExitAt(pt) == null && basement.IsWalkable(pt.X, pt.Y) && (basement.GetMapObjectAt(pt) == null && basement.GetItemsAt(pt) == null)), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          basement.DropItemAt(MakeItemGrenade(), pt);
          basement.DropItemAt(MakeItemGrenade(), pt);
          for (int index = 0; index < 5; ++index)
            basement.DropItemAt(MakeShopGunshopItem(), pt);
          return MakeObjShelf();
        }));
      return basement;
    }

    public Map GenerateUniqueMap_CHARUnderground(Map surfaceMap, Zone officeZone)
    {
      Contract.Requires(null != surfaceMap);
      Map underground = new Map(surfaceMap.Seed << 3 ^ surfaceMap.Seed, "CHAR Underground Facility", 100, 100) {
        Lighting = Lighting.DARKNESS,
        IsSecret = true,
        District = surfaceMap.District
      };
      TileFill(underground, m_Game.GameTiles.FLOOR_OFFICE, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.IsInside = true));
      TileRectangle(underground, m_Game.GameTiles.WALL_CHAR_OFFICE, new Rectangle(0, 0, underground.Width, underground.Height));
      Zone zone1 = null;
      Point point1 = new Point();
      bool flag;
      do {
        do {
          int x = m_DiceRoller.Roll(officeZone.Bounds.Left, officeZone.Bounds.Right);
          int y = m_DiceRoller.Roll(officeZone.Bounds.Top, officeZone.Bounds.Bottom);
          List<Zone> zonesAt = surfaceMap.GetZonesAt(x, y);
          if (zonesAt != null && zonesAt.Count != 0) {
            foreach (Zone zone2 in zonesAt) {
              if (zone2.Name.Contains("room")) {
                zone1 = zone2;
                break;
              }
            }
          }
        }
        while (zone1 == null);
        int num = 0;
        do {
          point1.X = m_DiceRoller.Roll(zone1.Bounds.Left, zone1.Bounds.Right);
          point1.Y = m_DiceRoller.Roll(zone1.Bounds.Top, zone1.Bounds.Bottom);
          flag = surfaceMap.IsWalkable(point1.X, point1.Y);
          ++num;
        }
        while (num < 100 && !flag);
      }
      while (!flag);
      DoForEachTile(zone1.Bounds, (Action<Point>) (pt =>
      {
        if (!(surfaceMap.GetMapObjectAt(pt) is DoorWindow)) return;
        surfaceMap.RemoveMapObjectAt(pt.X, pt.Y);
        DoorWindow doorWindow = MakeObjIronDoor();
        doorWindow.Barricade(Rules.BARRICADING_MAX);
        surfaceMap.PlaceMapObjectAt((MapObject) doorWindow, pt);
      }));
      Point point2 = new Point(underground.Width / 2, underground.Height / 2);
      underground.SetExitAt(point2, new Exit(surfaceMap, point1));
      underground.GetTileAt(point2.X, point2.Y).AddDecoration(GameImages.DECO_STAIRS_UP);
      surfaceMap.SetExitAt(point1, new Exit(underground, point2));
      surfaceMap.GetTileAt(point1.X, point1.Y).AddDecoration(GameImages.DECO_STAIRS_DOWN);
      ForEachAdjacent(underground, point2.X, point2.Y, (Action<Point>) (pt => underground.GetTileAt(pt).AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO)));
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
                TileRectangle(underground, m_Game.GameTiles.WALL_CHAR_OFFICE, rect5);
      foreach (Rectangle rectangle in list)
      {
        Point position1 = rectangle.Left < underground.Width / 2 ? new Point(rectangle.Right - 1, rectangle.Top + rectangle.Height / 2) : new Point(rectangle.Left, rectangle.Top + rectangle.Height / 2);
        if (underground.GetMapObjectAt(position1) == null)
        {
          DoorWindow door = MakeObjCharDoor();
                    PlaceDoorIfAccessibleAndNotAdjacent(underground, position1.X, position1.Y, m_Game.GameTiles.FLOOR_OFFICE, 6, door);
        }
        Point position2 = rectangle.Top < underground.Height / 2 ? new Point(rectangle.Left + rectangle.Width / 2, rectangle.Bottom - 1) : new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top);
        if (underground.GetMapObjectAt(position2) == null)
        {
          DoorWindow door = MakeObjCharDoor();
                    PlaceDoorIfAccessibleAndNotAdjacent(underground, position2.X, position2.Y, m_Game.GameTiles.FLOOR_OFFICE, 6, door);
        }
      }
      for (int right = rect1.Right; right < rect4.Left; ++right)
      {
                PlaceDoor(underground, right, rect1.Bottom - 1, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
                PlaceDoor(underground, right, rect3.Top, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
      }
      for (int bottom = rect1.Bottom; bottom < rect3.Top; ++bottom)
      {
                PlaceDoor(underground, rect1.Right - 1, bottom, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
                PlaceDoor(underground, rect2.Left, bottom, m_Game.GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
      }
      foreach (Rectangle wallsRect in list)
      {
        Rectangle rectangle = new Rectangle(wallsRect.Left + 1, wallsRect.Top + 1, wallsRect.Width - 2, wallsRect.Height - 2);
        string basename;
        if (wallsRect.Left == 0 && wallsRect.Top == 0 || wallsRect.Left == 0 && wallsRect.Bottom == underground.Height || wallsRect.Right == underground.Width && wallsRect.Top == 0 || wallsRect.Right == underground.Width && wallsRect.Bottom == underground.Height)
        {
          basename = "Power Room";
                    MakeCHARPowerRoom(underground, wallsRect, rectangle);
        }
        else
        {
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
      for (int x = 0; x < underground.Width; ++x)
      {
        for (int y = 0; y < underground.Height; ++y)
        {
          if (m_DiceRoller.RollChance(25))
          {
            Tile tileAt = underground.GetTileAt(x, y);
            if (!tileAt.Model.IsWalkable)
              tileAt.AddDecoration(BaseTownGenerator.CHAR_POSTERS[m_DiceRoller.Roll(0, BaseTownGenerator.CHAR_POSTERS.Length)]);
            else
              continue;
          }
          if (m_DiceRoller.RollChance(20))
          {
            Tile tileAt = underground.GetTileAt(x, y);
            if (tileAt.Model.IsWalkable)
              tileAt.AddDecoration("Tiles\\Decoration\\bloodied_floor");
            else
              tileAt.AddDecoration("Tiles\\Decoration\\bloodied_wall");
          }
        }
      }
      int width = underground.Width;
      for (int index1 = 0; index1 < width; ++index1)
      {
        Actor newUndead = CreateNewUndead(0);
        while (true) {
          GameActors.IDs index2 = m_Game.NextUndeadEvolution((GameActors.IDs) newUndead.Model.ID);
          if (index2 != (GameActors.IDs) newUndead.Model.ID)
            newUndead.Model = m_Game.GameActors[index2];
          else
            break;
        }
        ActorPlace(m_DiceRoller, underground.Width * underground.Height, underground, newUndead, (Predicate<Point>) (pt => underground.GetExitAt(pt) == null));
      }
      int num1 = underground.Width / 10;
      for (int index = 0; index < num1; ++index) {
        Actor newCharGuard = CreateNewCHARGuard(0);
        ActorPlace(m_DiceRoller, underground.Width * underground.Height, underground, newCharGuard, (Predicate<Point>) (pt => underground.GetExitAt(pt) == null));
      }
      return underground;
    }

    private void MakeCHARArmoryRoom(Map map, Rectangle roomRect)
    {
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        if (map.GetExitAt(pt) != null) return null;
        if (!m_DiceRoller.RollChance(20)) return null;
        map.DropItemAt(!m_DiceRoller.RollChance(20) ? (!m_DiceRoller.RollChance(20) ? (!m_DiceRoller.RollChance(20) ? (!m_DiceRoller.RollChance(30) ? (Item)(m_DiceRoller.RollChance(50) ? MakeItemShotgunAmmo() : MakeItemLightRifleAmmo()) : (Item)(m_DiceRoller.RollChance(50) ? MakeItemShotgun() : MakeItemHuntingRifle())) : (Item)MakeItemGrenade()) : (Item)(m_DiceRoller.RollChance(50) ? MakeItemZTracker() : MakeItemBlackOpsGPS())) : (Item)MakeItemCHARLightBodyArmor(), pt);
        return MakeObjShelf();
      }));
    }

    private void MakeCHARStorageRoom(Map map, Rectangle roomRect)
    {
      TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, roomRect);
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) > 0) return null;
        if (map.GetExitAt(pt) != null) return null;
        if (!m_DiceRoller.RollChance(50)) return null;
        return (m_DiceRoller.RollChance(50) ? MakeObjJunk() : MakeObjBarrels());
      }));
      for (int left = roomRect.Left; left < roomRect.Right; ++left) {
        for (int top = roomRect.Top; top < roomRect.Bottom; ++top) {
          if (CountAdjWalls(map, left, top) <= 0 && map.GetMapObjectAt(left, top) == null)
            map.DropItemAt(MakeShopConstructionItem(), left, top);
        }
      }
    }

    private void MakeCHARLivingRoom(Map map, Rectangle roomRect)
    {
      TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, roomRect, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration("Tiles\\Decoration\\char_floor_logo")));
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        if (map.GetExitAt(pt) != null) return null;
        if (!m_DiceRoller.RollChance(30)) return null;
        if (m_DiceRoller.RollChance(50)) return MakeObjBed("MapObjects\\bed");
        return MakeObjFridge();
      }));
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) > 0) return null;
        if (map.GetExitAt(pt) != null) return null;
        if (!m_DiceRoller.RollChance(30)) return null;
        if (!m_DiceRoller.RollChance(30)) return MakeObjChair("MapObjects\\char_chair");
        MapObject mapObject = MakeObjTable("MapObjects\\char_table");
        map.DropItemAt(MakeItemCannedFood(), pt);
        return mapObject;
      }));
    }

    private void MakeCHARPharmacyRoom(Map map, Rectangle roomRect)
    {
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt.X, pt.Y) < 3) return null;
        if (map.GetExitAt(pt) != null) return null;
        if (!m_DiceRoller.RollChance(20)) return null;
        map.DropItemAt(MakeHospitalItem(), pt);
        return MakeObjShelf();
      }));
    }

    private void MakeCHARPowerRoom(Map map, Rectangle wallsRect, Rectangle roomRect)
    {
      TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, roomRect);
      DoForEachTile(wallsRect, (Action<Point>) (pt =>
      {
        if (!(map.GetMapObjectAt(pt) is DoorWindow)) return;
        map.ForEachAdjacentInMap(pt, (Action<Point>) (ptAdj =>
        {
          Tile tileAt = map.GetTileAt(ptAdj);
          if (tileAt.Model.IsWalkable)
            return;
          tileAt.RemoveAllDecorations();
          tileAt.AddDecoration("Tiles\\Decoration\\power_sign_big");
        }));
      }));
      DoForEachTile(roomRect, (Action<Point>) (pt =>
      {
        if (!map.GetTileAt(pt).Model.IsWalkable || map.GetExitAt(pt) != null || CountAdjWalls(map, pt.X, pt.Y) < 3) return;
        map.PlaceMapObjectAt(MakeObjPowerGenerator(), pt);
      }));
    }

    private void MakePoliceStation(Map map, List<BaseTownGenerator.Block> freeBlocks, out BaseTownGenerator.Block policeBlock)
    {
      policeBlock = freeBlocks[m_DiceRoller.Roll(0, freeBlocks.Count)];
      Point stairsToLevel1;
      GeneratePoliceStation(map, policeBlock, out stairsToLevel1);
      Map stationOfficesLevel = GeneratePoliceStation_OfficesLevel(map, policeBlock, stairsToLevel1);
      Map stationJailsLevel = GeneratePoliceStation_JailsLevel(stationOfficesLevel);
      AddExit(map, stairsToLevel1, stationOfficesLevel, new Point(1, 1), "Tiles\\Decoration\\stairs_down", true);
      AddExit(stationOfficesLevel, new Point(1, 1), map, stairsToLevel1, "Tiles\\Decoration\\stairs_up", true);
      AddExit(stationOfficesLevel, new Point(1, stationOfficesLevel.Height - 2), stationJailsLevel, new Point(1, 1), "Tiles\\Decoration\\stairs_down", true);
      AddExit(stationJailsLevel, new Point(1, 1), stationOfficesLevel, new Point(1, stationOfficesLevel.Height - 2), "Tiles\\Decoration\\stairs_up", true);
      m_Params.District.AddUniqueMap(stationOfficesLevel);
      m_Params.District.AddUniqueMap(stationJailsLevel);
      Session.Get.UniqueMaps.PoliceStation_OfficesLevel = new UniqueMap(stationOfficesLevel);
      Session.Get.UniqueMaps.PoliceStation_JailsLevel = new UniqueMap(stationJailsLevel);
    }

    private void GeneratePoliceStation(Map surfaceMap, BaseTownGenerator.Block policeBlock, out Point stairsToLevel1)
    {
      TileFill(surfaceMap, m_Game.GameTiles.FLOOR_TILES, policeBlock.InsideRect);
      TileRectangle(surfaceMap, m_Game.GameTiles.WALL_POLICE_STATION, policeBlock.BuildingRect);
      TileRectangle(surfaceMap, m_Game.GameTiles.FLOOR_WALKWAY, policeBlock.Rectangle);
      DoForEachTile(policeBlock.InsideRect,(Action<Point>)(pt => {
          surfaceMap.GetTileAt(pt).IsInside = true;
          Session.Get.ForcePoliceKnown(new Location(surfaceMap, pt));
      }));
      Point point = new Point(policeBlock.BuildingRect.Left + policeBlock.BuildingRect.Width / 2, policeBlock.BuildingRect.Bottom - 1);
      surfaceMap.GetTileAt(point.X - 1, point.Y).AddDecoration("Tiles\\Decoration\\police_station");
      surfaceMap.GetTileAt(point.X + 1, point.Y).AddDecoration("Tiles\\Decoration\\police_station");
      Rectangle rect = Rectangle.FromLTRB(policeBlock.BuildingRect.Left, policeBlock.BuildingRect.Top + 2, policeBlock.BuildingRect.Right, policeBlock.BuildingRect.Bottom);
      TileRectangle(surfaceMap, m_Game.GameTiles.WALL_POLICE_STATION, rect);
      PlaceDoor(surfaceMap, rect.Left + rect.Width / 2, rect.Top, m_Game.GameTiles.FLOOR_TILES, MakeObjIronDoor());
      PlaceDoor(surfaceMap, point.X, point.Y, m_Game.GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      DoForEachTile(rect, (Action<Point>) (pt =>
      {
        if (!surfaceMap.IsWalkable(pt.X, pt.Y) || CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)
          return;
        surfaceMap.PlaceMapObjectAt(MakeObjBench(), pt);
      }));
      stairsToLevel1 = new Point(point.X, policeBlock.InsideRect.Top);
      surfaceMap.AddZone(MakeUniqueZone("Police Station", policeBlock.BuildingRect));
      MakeWalkwayZones(surfaceMap, policeBlock);
    }

    private Map GeneratePoliceStation_OfficesLevel(Map surfaceMap, BaseTownGenerator.Block policeBlock, Point exitPos)
    {
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Offices", 20, 20)
      {
        Lighting = Lighting.DARKNESS
      };
      map.District = surfaceMap.District;

      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_TILES);
      TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
      List<Rectangle> list = new List<Rectangle>();
      MakeRoomsPlan(map, ref list, rect1, 5);
      foreach (Rectangle rect2 in list) {
        Rectangle rect3 = Rectangle.FromLTRB(rect2.Left + 1, rect2.Top + 1, rect2.Right - 1, rect2.Bottom - 1);
        if (rect2.Right == map.Width) {
          TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, rect2);
          PlaceDoor(map, rect2.Left, rect2.Top + rect2.Height / 2, m_Game.GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
          DoForEachTile(rect3, (Action<Point>) (pt =>
          {
            if (!map.IsWalkable(pt.X, pt.Y) || CountAdjWalls(map, pt.X, pt.Y) == 0 || CountAdjDoors(map, pt.X, pt.Y) > 0) return;
            map.PlaceMapObjectAt(MakeObjShelf(), pt);
            Item it;
            switch (m_DiceRoller.Roll(0, 10)) {
              case 0:
              case 1:
                it = m_DiceRoller.RollChance(50) ? MakeItemPoliceJacket() : MakeItemPoliceRiotArmor();
                break;
              case 2:
              case 3:
                it = m_DiceRoller.RollChance(50) ? (Item)(m_DiceRoller.RollChance(50) ? MakeItemFlashlight() : MakeItemBigFlashlight()) : (Item)MakeItemPoliceRadio();
                break;
              case 4:
              case 5:
                it = MakeItemTruncheon();
                break;
              case 6:
              case 7:
                it = m_DiceRoller.RollChance(30) ? (Item)MakeItemPistol() : (Item)MakeItemLightPistolAmmo();
                break;
              case 8:
              case 9:
                it = m_DiceRoller.RollChance(30) ? (Item)MakeItemShotgun() : (Item)MakeItemShotgunAmmo();
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled roll");
            }
            map.DropItemAt(it, pt);
          }));
          map.AddZone(MakeUniqueZone("security", rect3));
        } else {
          TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, rect2);
          TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, rect2);
          PlaceDoor(map, rect2.Left, rect2.Top + rect2.Height / 2, m_Game.GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());
          MapObjectPlaceInGoodPosition(map, rect3, (Func<Point, bool>) (pt =>
          {
            return map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjTable("MapObjects\\table")));
          MapObjectPlaceInGoodPosition(map, rect3, (Func<Point, bool>) (pt =>
          {
            return map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjChair("MapObjects\\chair")));
          MapObjectPlaceInGoodPosition(map, rect3, (Func<Point, bool>) (pt =>
          {
            return map.IsWalkable(pt.X, pt.Y) && CountAdjDoors(map, pt.X, pt.Y) == 0;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjChair("MapObjects\\chair")));
          map.AddZone(MakeUniqueZone("office", rect3));
        }
      }
      DoForEachTile(new Rectangle(1, 1, 1, map.Height - 2), (Action<Point>) (pt =>
      {
        if (pt.Y % 2 == 1 || !map.IsWalkable(pt) || CountAdjWalls(map, pt) != 3)
          return;
        map.PlaceMapObjectAt(MakeObjIronBench(), pt);
      }));
      for (int index = 0; index < 5; ++index) {
        Actor newPoliceman = CreateNewPoliceman(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newPoliceman);
      }
      DoForEachTile(map.Rect, (Action<Point>)(pt => {
        Session.Get.ForcePoliceKnown(new Location(map, pt));
      }));
      return map;
    }

    private Map GeneratePoliceStation_JailsLevel(Map surfaceMap)
    {
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Jails", 22, 6)
      {
        Lighting = Lighting.DARKNESS
      };
      map.District = surfaceMap.District;
      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_TILES);
      TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, map.Rect);
      List<Rectangle> rectangleList = new List<Rectangle>();
      int x = 0;
      while (x + 3 <= map.Width) {
        Rectangle rect = new Rectangle(x, 3, 3, 3);
        rectangleList.Add(rect);
        TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE, rect);
        TileRectangle(map, m_Game.GameTiles.WALL_POLICE_STATION, rect);
        Point position1 = new Point(x + 1, 4);
        map.PlaceMapObjectAt(MakeObjIronBench(), position1);
        Point position2 = new Point(x + 1, 3);
        map.SetTileModelAt(position2.X, position2.Y, m_Game.GameTiles.FLOOR_CONCRETE);
        map.PlaceMapObjectAt(MakeObjIronGate(), position2);
        map.AddZone(MakeUniqueZone("jail", rect));
        x += 2;
      }
      Rectangle rect1 = Rectangle.FromLTRB(1, 1, map.Width, 3);
      map.AddZone(MakeUniqueZone("cells corridor", rect1));
      map.PlaceMapObjectAt(MakeObjPowerGenerator(), new Point(map.Width - 2, 1));
      for (int index = 0; index < rectangleList.Count - 1; ++index) {
        Rectangle rectangle = rectangleList[index];
        Actor newCivilian = CreateNewCivilian(0, 0, 1);
        while (!newCivilian.Inventory.IsEmpty)
          newCivilian.Inventory.RemoveAllQuantity(newCivilian.Inventory[0]);
        newCivilian.Inventory.AddAll(MakeItemGroceries());
        map.PlaceActorAt(newCivilian, new Point(rectangle.Left + 1, rectangle.Top + 1));
      }
      Rectangle rectangle1 = rectangleList[rectangleList.Count - 1];
      Actor newCivilian1 = CreateNewCivilian(0, 0, 1);
      newCivilian1.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian1.Inventory.MaxCapacity; ++index)
        newCivilian1.Inventory.AddAll(MakeItemArmyRation());
      map.PlaceActorAt(newCivilian1, new Point(rectangle1.Left + 1, rectangle1.Top + 1));
      Session.Get.UniqueActors.PoliceStationPrisonner = new UniqueActor() {
        TheActor = newCivilian1,
        IsSpawned = true
      };
      DoForEachTile(map.Rect, (Action<Point>)(pt => {
          Session.Get.ForcePoliceKnown(new Location(map, pt));
      }));
      return map;
    }

    private void MakeHospital(Map map, List<BaseTownGenerator.Block> freeBlocks, out BaseTownGenerator.Block hospitalBlock)
    {
      Contract.Requires(null!=map.District);
      hospitalBlock = freeBlocks[m_DiceRoller.Roll(0, freeBlocks.Count)];
      GenerateHospitalEntryHall(map, hospitalBlock);
      Map hospitalAdmissions = GenerateHospital_Admissions(map.Seed << 1 ^ map.Seed, map.District);
      Map hospitalOffices = GenerateHospital_Offices(map.Seed << 2 ^ map.Seed, map.District);
      Map hospitalPatients = GenerateHospital_Patients(map.Seed << 3 ^ map.Seed, map.District);
      Map hospitalStorage = GenerateHospital_Storage(map.Seed << 4 ^ map.Seed);
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

    private void GenerateHospitalEntryHall(Map surfaceMap, BaseTownGenerator.Block block)
    {
      TileFill(surfaceMap, m_Game.GameTiles.FLOOR_TILES, block.InsideRect);
      TileRectangle(surfaceMap, m_Game.GameTiles.WALL_HOSPITAL, block.BuildingRect);
      TileRectangle(surfaceMap, m_Game.GameTiles.FLOOR_WALKWAY, block.Rectangle);
      DoForEachTile(block.InsideRect, (Action<Point>) (pt => surfaceMap.GetTileAt(pt).IsInside = true));
      Point point1 = new Point(block.BuildingRect.Left + block.BuildingRect.Width / 2, block.BuildingRect.Bottom - 1);
      Point point2 = new Point(point1.X - 1, point1.Y);
      surfaceMap.GetTileAt(point2.X - 1, point2.Y).AddDecoration("Tiles\\Decoration\\hospital");
      surfaceMap.GetTileAt(point1.X + 1, point1.Y).AddDecoration("Tiles\\Decoration\\hospital");
      Rectangle rect = Rectangle.FromLTRB(block.BuildingRect.Left, block.BuildingRect.Top, block.BuildingRect.Right, block.BuildingRect.Bottom);
      PlaceDoor(surfaceMap, point1.X, point1.Y, m_Game.GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      PlaceDoor(surfaceMap, point2.X, point2.Y, m_Game.GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      DoForEachTile(rect, (Action<Point>) (pt =>
      {
        if (pt.Y == block.InsideRect.Top || (pt.Y == block.InsideRect.Bottom - 1 || !surfaceMap.IsWalkable(pt.X, pt.Y) || (CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)))
          return;
        surfaceMap.PlaceMapObjectAt(MakeObjIronBench(), pt);
      }));
      surfaceMap.AddZone(MakeUniqueZone("Hospital", block.BuildingRect));
      MakeWalkwayZones(surfaceMap, block);
    }

    private Map GenerateHospital_Admissions(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Admissions", 13, 33)
      {
        Lighting = Lighting.DARKNESS,
        District = d
      };
      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_TILES);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, rect);
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
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalPatient, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "patient room")));
      }
      for (int index = 0; index < 4; ++index) {
        Actor newHospitalNurse = CreateNewHospitalNurse(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalNurse, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      for (int index = 0; index < 1; ++index) {
        Actor newHospitalDoctor = CreateNewHospitalDoctor(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalDoctor, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      return map;
    }

    private Map GenerateHospital_Offices(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Offices", 13, 33)
      {
        Lighting = Lighting.DARKNESS,
        District = d
      };
      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_TILES);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, rect);
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
      for (int index = 0; index < 5; ++index) {
        Actor newHospitalNurse = CreateNewHospitalNurse(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalNurse, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "office")));
      }
      for (int index = 0; index < 2; ++index) {
        Actor newHospitalDoctor = CreateNewHospitalDoctor(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalDoctor, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "office")));
      }
      return map;
    }

    private Map GenerateHospital_Patients(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Patients", 13, 49)
      {
        Lighting = Lighting.DARKNESS,
        District = d
      };
      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_TILES);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, rect);
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
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalPatient, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "patient room")));
      }
      for (int index = 0; index < 8; ++index) {
        Actor newHospitalNurse = CreateNewHospitalNurse(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalNurse, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      for (int index = 0; index < 2; ++index) {
        Actor newHospitalDoctor = CreateNewHospitalDoctor(0);
        ActorPlace(m_DiceRoller, map.Width * map.Height, map, newHospitalDoctor, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      return map;
    }

    private Map GenerateHospital_Storage(int seed)
    {
      Map map = new Map(seed, "Hospital - Storage", 51, 16)
      {
        Lighting = Lighting.DARKNESS
      };
      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_TILES);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(0, 0, map.Width, 4);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, rect1);
      map.AddZone(MakeUniqueZone("north corridor", rect1));
      Rectangle rect2 = Rectangle.FromLTRB(0, rect1.Bottom - 1, map.Width, rect1.Bottom - 1 + 4);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, rect2);
      map.SetTileModelAt(1, rect2.Top, m_Game.GameTiles.FLOOR_TILES);
      map.PlaceMapObjectAt(MakeObjIronGate(), new Point(1, rect2.Top));
      map.AddZone(MakeUniqueZone("central corridor", rect2));
      Rectangle rectangle1 = new Rectangle(2, rect2.Bottom - 1, map.Width - 2, 4);
      int left1 = rectangle1.Left;
      while (left1 <= map.Width - 5) {
        Rectangle room = new Rectangle(left1, rectangle1.Top, 5, 4);
        MakeHospitalStorageRoom(map, "storage", room);
        left1 += 4;
      }
      map.SetTileModelAt(1, rectangle1.Top, m_Game.GameTiles.FLOOR_TILES);
      Rectangle rect3 = Rectangle.FromLTRB(0, rectangle1.Bottom - 1, map.Width, rectangle1.Bottom - 1 + 4);
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, rect3);
      map.SetTileModelAt(1, rect3.Top, m_Game.GameTiles.FLOOR_TILES);
      map.AddZone(MakeUniqueZone("south corridor", rect3));
      Rectangle rectangle2 = new Rectangle(2, rect3.Bottom - 1, map.Width - 2, 4);
      int left2 = rectangle2.Left;
      while (left2 <= map.Width - 5) {
        Rectangle room = new Rectangle(left2, rectangle2.Top, 5, 4);
        MakeHospitalStorageRoom(map, "storage", room);
        left2 += 4;
      }
      map.SetTileModelAt(1, rectangle2.Top, m_Game.GameTiles.FLOOR_TILES);
      return map;
    }

    private Map GenerateHospital_Power(int seed, District d)
    {
      Map map = new Map(seed, "Hospital - Power", 10, 10) {
        Lighting = Lighting.DARKNESS,
        District = d
      };
      DoForEachTile(map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      TileFill(map, m_Game.GameTiles.FLOOR_CONCRETE);
      TileRectangle(map, m_Game.GameTiles.WALL_BRICK, map.Rect);
      Rectangle rect = Rectangle.FromLTRB(1, 1, 3, map.Height);
      map.AddZone(MakeUniqueZone("corridor", rect));
      for (int y = 1; y < map.Height - 2; ++y)
        map.PlaceMapObjectAt(MakeObjIronFence(), new Point(2, y));
      Rectangle room = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
      map.AddZone(MakeUniqueZone("power room", room));
      DoForEachTile(room, (Action<Point>) (pt =>
      {
        if (pt.X == room.Left || !map.IsWalkable(pt) || CountAdjWalls(map, pt) < 3) return;
        map.PlaceMapObjectAt((MapObject)MakeObjPowerGenerator(), pt);
      }));
      Actor named = m_Game.GameActors.JasonMyers.CreateNamed(m_Game.GameFactions.ThePsychopaths, "Jason Myers", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_JASON_MYERS);
      named.StartingSkill(Skills.IDs.TOUGH,3);
      named.StartingSkill(Skills.IDs.STRONG,3);
      named.StartingSkill(Skills.IDs._FIRST,3);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,3);
      named.Inventory.AddAll(MakeItemJasonMyersAxe());
      map.PlaceActorAt(named, new Point(map.Width / 2, map.Height / 2));
      Session.Get.UniqueActors.JasonMyers = new UniqueActor() {
        TheActor = named,
        IsSpawned = true
      };
      return map;
    }

    private Actor CreateNewHospitalPatient(int spawnTime)
    {
      Actor numberedName = (m_Rules.Roll(0, 2) == 0 ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian).CreateNumberedName(m_Game.GameFactions.TheCivilians, 0);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = "Patient " + numberedName.Name;
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\hospital_patient_uniform");
      return numberedName;
    }

    private Actor CreateNewHospitalNurse(int spawnTime)
    {
      Actor numberedName = m_Game.GameActors.FemaleCivilian.CreateNumberedName(m_Game.GameFactions.TheCivilians, 0);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = "Nurse " + numberedName.Name;
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_NURSE_UNIFORM);
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.MEDIC);
      numberedName.Inventory.AddAll(MakeItemBandages());
      return numberedName;
    }

    private Actor CreateNewHospitalDoctor(int spawnTime)
    {
      Actor numberedName = m_Game.GameActors.MaleCivilian.CreateNumberedName(m_Game.GameFactions.TheCivilians, 0);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = "Doctor " + numberedName.Name;
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
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      int x = isFacingEast ? room.Right - 1 : room.Left;
      PlaceDoor(map, x, room.Top + 1, m_Game.GameTiles.FLOOR_TILES, MakeObjHospitalDoor());
      Point position1 = new Point(room.Left + room.Width / 2, room.Bottom - 2);
      map.PlaceMapObjectAt(MakeObjBed("MapObjects\\hospital_bed"), position1);
      map.PlaceMapObjectAt(MakeObjChair("MapObjects\\hospital_chair"), new Point(isFacingEast ? position1.X + 1 : position1.X - 1, position1.Y));
      Point position2 = new Point(isFacingEast ? position1.X - 1 : position1.X + 1, position1.Y);
      map.PlaceMapObjectAt(MakeObjNightTable("MapObjects\\hospital_nighttable"), position2);
      if (m_DiceRoller.RollChance(50)) {
        int num = m_DiceRoller.Roll(0, 3);
        Item it = null;
        switch (num) {
          case 0:
            it = MakeShopPharmacyItem();
            break;
          case 1:
            it = MakeItemGroceries();
            break;
          case 2:
            it = MakeItemBook();
            break;
        }
        if (it != null)
          map.DropItemAt(it, position2);
      }
      map.PlaceMapObjectAt(MakeObjWardrobe("MapObjects\\hospital_wardrobe"), new Point(isFacingEast ? room.Left + 1 : room.Right - 2, room.Top + 1));
    }

    private void MakeHospitalOfficeRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
            TileFill(map, m_Game.GameTiles.FLOOR_PLANKS, room);
            TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      int x1 = isFacingEast ? room.Right - 1 : room.Left;
      int y = room.Top + 2;
            PlaceDoor(map, x1, y, m_Game.GameTiles.FLOOR_TILES, MakeObjWoodenDoor());
      int x2 = isFacingEast ? room.Left + 2 : room.Right - 3;
      map.PlaceMapObjectAt(MakeObjTable("MapObjects\\table"), new Point(x2, y));
      map.PlaceMapObjectAt(MakeObjChair("MapObjects\\chair"), new Point(x2 - 1, y));
      map.PlaceMapObjectAt(MakeObjChair("MapObjects\\chair"), new Point(x2 + 1, y));
    }

    private void MakeHospitalStorageRoom(Map map, string baseZoneName, Rectangle room)
    {
      TileRectangle(map, m_Game.GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      PlaceDoor(map, room.Left + 2, room.Top, m_Game.GameTiles.FLOOR_TILES, MakeObjHospitalDoor());
      DoForEachTile(room, (Action<Point>) (pt =>
      {
        if (!map.IsWalkable(pt) || CountAdjDoors(map, pt.X, pt.Y) > 0) return;
        map.PlaceMapObjectAt(MakeObjShelf(), pt);
        Item it = m_DiceRoller.RollChance(80) ? MakeHospitalItem() : MakeItemCannedFood();
        if (it.Model.IsStackable)
          it.Quantity = it.Model.StackingLimit;
        map.DropItemAt(it, pt);
      }));
    }

    public void GiveRandomItemToActor(DiceRoller roller, Actor actor, int spawnTime)
    {
      Item it;
      if (new WorldTime(spawnTime).Day > Rules.GIVE_RARE_ITEM_DAY && roller.RollChance(Rules.GIVE_RARE_ITEM_CHANCE)) {
        switch (roller.Roll(0, (Session.Get.HasInfection ? 6 : 5))) {
          case 0:
            it = MakeItemGrenade();
            break;
          case 1:
            it = MakeItemArmyBodyArmor();
            break;
          case 2:
            it = MakeItemHeavyPistolAmmo();
            break;
          case 3:
            it = MakeItemHeavyRifleAmmo();
            break;
          case 4:
            it = MakeItemCombatKnife();
            break;
          case 5:
            it = MakeItemPillsAntiviral();
            break;
          default:
            it = (Item) null;
            break;
        }
      } else {
        switch (roller.Roll(0, 10)) {
          case 0:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType.CONSTRUCTION);
            break;
          case 1:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType._FIRST);
            break;
          case 2:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType.GROCERY);
            break;
          case 3:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType.GUNSHOP);
            break;
          case 4:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType.PHARMACY);
            break;
          case 5:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType.SPORTSWEAR);
            break;
          case 6:
            it = MakeRandomShopItem(BaseTownGenerator.ShopType.HUNTING);
            break;
          case 7:
            it = MakeRandomParkItem();
            break;
          case 8:
            it = MakeRandomBedroomItem();
            break;
          case 9:
            it = MakeRandomKitchenItem();
            break;
          default:
            it = (Item) null;
            break;
        }
      }
      if (it == null)
        return;
      actor.Inventory.AddAll(it);
    }

    public Actor CreateNewRefugee(int spawnTime, int itemsToCarry)
    {
      Actor actor;
      if (m_DiceRoller.RollChance(Params.PolicemanChance))
      {
        actor = CreateNewPoliceman(spawnTime);
        for (int index = 0; index < itemsToCarry && actor.Inventory.CountItems < actor.Inventory.MaxCapacity; ++index)
                    GiveRandomItemToActor(m_DiceRoller, actor, spawnTime);
      }
      else
        actor = CreateNewCivilian(spawnTime, itemsToCarry, 1);
      int count = 1 + new WorldTime(spawnTime).Day;
            GiveRandomSkillsToActor(m_DiceRoller, actor, count);
      return actor;
    }

    public Actor CreateNewSurvivor(int spawnTime)
    {
      bool flag = m_Rules.Roll(0, 2) == 0;
      Actor numberedName = (flag ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian).CreateNumberedName(m_Game.GameFactions.TheSurvivors, spawnTime);
            GiveNameToActor(m_DiceRoller, numberedName);
            DressCivilian(m_DiceRoller, numberedName);
      numberedName.Doll.AddDecoration(DollPart.HEAD, flag ? "Actors\\Decoration\\survivor_male_bandana" : "Actors\\Decoration\\survivor_female_bandana");
      numberedName.Inventory.AddAll(MakeItemCannedFood());
      numberedName.Inventory.AddAll(MakeItemArmyRation());
      if (m_DiceRoller.RollChance(50))
      {
        numberedName.Inventory.AddAll(MakeItemArmyRifle());
        if (m_DiceRoller.RollChance(50))
          numberedName.Inventory.AddAll(MakeItemHeavyRifleAmmo());
        else
          numberedName.Inventory.AddAll(MakeItemGrenade());
      }
      else
      {
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

    public Actor CreateNewNakedHuman(int spawnTime, int itemsToCarry, int skills)
    {
      return (m_Rules.Roll(0, 2) == 0 ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian).CreateNumberedName(m_Game.GameFactions.TheCivilians, spawnTime);
    }

    public Actor CreateNewCivilian(int spawnTime, int itemsToCarry, int skills)
    {
      Actor numberedName = (m_Rules.Roll(0, 2) == 0 ? m_Game.GameActors.MaleCivilian : m_Game.GameActors.FemaleCivilian).CreateNumberedName(m_Game.GameFactions.TheCivilians, spawnTime);
      DressCivilian(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      for (int index = 0; index < itemsToCarry; ++index)
        GiveRandomItemToActor(m_DiceRoller, numberedName, spawnTime);
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, skills);
      numberedName.CreateCivilianDeductFoodSleep(m_Rules);
      return numberedName;
    }

    public Actor CreateNewPoliceman(int spawnTime)
    {
      Actor numberedName = m_Game.GameActors.Policeman.CreateNumberedName(m_Game.GameFactions.ThePolice, spawnTime);
      DressPolice(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = "Cop " + numberedName.Name;
      GiveRandomSkillsToActor(m_DiceRoller, numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.FIREARMS);
      numberedName.StartingSkill(Skills.IDs.LEADERSHIP);
      if (m_DiceRoller.RollChance(50)) {
        numberedName.Inventory.AddAll(MakeItemPistol());
        numberedName.Inventory.AddAll(MakeItemLightPistolAmmo());
      } else {
        numberedName.Inventory.AddAll(MakeItemShotgun());
        numberedName.Inventory.AddAll(MakeItemShotgunAmmo());
      }
      numberedName.Inventory.AddAll(MakeItemTruncheon());
      numberedName.Inventory.AddAll(MakeItemFlashlight());
//    numberedName.Inventory.AddAll(MakeItemPoliceRadio()); // class prop, implicit for police
      if (m_DiceRoller.RollChance(50)) {
        numberedName.Inventory.AddAll(m_DiceRoller.RollChance(80) ? MakeItemPoliceJacket() : MakeItemPoliceRiotArmor());
      }
      return numberedName;
    }

    public Actor CreateNewUndead(int spawnTime)
    {
      Actor actor;
      if (Session.Get.HasAllZombies) {
        int num = m_Rules.Roll(0, 100);
        actor = (num < RogueGame.Options.SpawnSkeletonChance ? m_Game.GameActors.Skeleton : (num < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance ? m_Game.GameActors.Zombie : (num < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance + RogueGame.Options.SpawnZombieMasterChance ? m_Game.GameActors.ZombieMaster : m_Game.GameActors.Skeleton))).CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);
      } else {
        actor = MakeZombified(null, CreateNewCivilian(spawnTime, 0, 0), spawnTime);
        int num = new WorldTime(spawnTime).Day / 2;
        if (num > 0) {
          for (int index = 0; index < num; ++index) {
            Skills.IDs? nullable = m_Game.ZombifySkill((Skills.IDs)m_Rules.Roll(0, (int)Skills.IDs._COUNT));
            if (nullable.HasValue) actor.SkillUpgrade(nullable.Value);
          }
          actor.RecomputeStartingStats();
        }
      }
      return actor;
    }

    public Actor MakeZombified(Actor zombifier, Actor deadVictim, int turn)
    {
      string properName = string.Format("{0}'s zombie", (object) deadVictim.UnmodifiedName);
      Actor named = (deadVictim.Doll.Body.IsMale ? m_Game.GameActors.MaleZombified : m_Game.GameActors.FemaleZombified).CreateNamed(zombifier == null ? m_Game.GameFactions.TheUndeads : zombifier.Faction, properName, deadVictim.IsPluralName, turn);
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
      return (m_DiceRoller.RollChance(80) ? m_Game.GameActors.RatZombie : m_Game.GameActors.Zombie).CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewBasementRatZombie(int spawnTime)
    {
      if (!Session.Get.HasAllZombies) return CreateNewUndead(spawnTime);
      return m_Game.GameActors.RatZombie.CreateNumberedName(m_Game.GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewCHARGuard(int spawnTime)
    {
      Actor numberedName = m_Game.GameActors.CHARGuard.CreateNumberedName(m_Game.GameFactions.TheCHARCorporation, spawnTime);
      DressCHARGuard(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = "Gd. " + numberedName.Name;
      numberedName.Inventory.AddAll(MakeItemShotgun());
      numberedName.Inventory.AddAll(MakeItemShotgunAmmo());
      numberedName.Inventory.AddAll(MakeItemCHARLightBodyArmor());
      return numberedName;
    }

    public Actor CreateNewArmyNationalGuard(int spawnTime, string rankName)
    {
      Actor numberedName = m_Game.GameActors.NationalGuard.CreateNumberedName(m_Game.GameFactions.TheArmy, spawnTime);
      DressArmy(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = rankName + " " + numberedName.Name;
      numberedName.Inventory.AddAll(MakeItemArmyRifle());
      numberedName.Inventory.AddAll(MakeItemHeavyRifleAmmo());
      numberedName.Inventory.AddAll(MakeItemArmyPistol());
      numberedName.Inventory.AddAll(MakeItemHeavyPistolAmmo());
      numberedName.Inventory.AddAll(MakeItemArmyBodyArmor());
      ItemBarricadeMaterial barricadeMaterial = MakeItemWoodenPlank();
      barricadeMaterial.Quantity = m_Game.GameItems.WOODENPLANK.StackingLimit;
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
      Actor numberedName = m_Game.GameActors.BikerMan.CreateNumberedName(m_Game.GameFactions.TheBikers, spawnTime);
      numberedName.GangID = gangId;
      DressBiker(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Controller = new GangAI();
      numberedName.Inventory.AddAll(m_DiceRoller.RollChance(50) ? MakeItemCrowbar() : MakeItemBaseballBat());
      numberedName.Inventory.AddAll(MakeItemBikerGangJacket(gangId));
      int count = new WorldTime(spawnTime).Day - RogueGame.BIKERS_RAID_DAY;
      if (count > 0)
        GiveRandomSkillsToActor(m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewGangstaMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = m_Game.GameActors.GangstaMan.CreateNumberedName(m_Game.GameFactions.TheGangstas, spawnTime);
      numberedName.GangID = gangId;
      DressGangsta(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Controller = (ActorController) new GangAI();
      numberedName.Inventory.AddAll(m_DiceRoller.RollChance(50) ? (Item)MakeItemRandomPistol() : (Item)MakeItemBaseballBat());
      int count = new WorldTime(spawnTime).Day - RogueGame.GANGSTAS_RAID_DAY;
      if (count > 0)
        GiveRandomSkillsToActor(m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewBlackOps(int spawnTime, string rankName)
    {
      Actor numberedName = m_Game.GameActors.BlackOps.CreateNumberedName(m_Game.GameFactions.TheBlackOps, spawnTime);
      DressBlackOps(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Name = rankName + " " + numberedName.Name;
      numberedName.Inventory.AddAll(MakeItemPrecisionRifle());
      numberedName.Inventory.AddAll(MakeItemHeavyRifleAmmo());
      numberedName.Inventory.AddAll(MakeItemArmyPistol());
      numberedName.Inventory.AddAll(MakeItemHeavyPistolAmmo());
      numberedName.Inventory.AddAll(MakeItemBlackOpsGPS());
      return numberedName;
    }

    public Actor CreateNewFeralDog(int spawnTime)
    {
      Actor numberedName = m_Game.GameActors.FeralDog.CreateNumberedName(m_Game.GameFactions.TheFerals, spawnTime);
      SkinDog(m_DiceRoller, numberedName);
      return numberedName;
    }

    private void AddExit(Map from, Point fromPosition, Map to, Point toPosition, string exitImageID, bool isAnAIExit)
    {
      from.SetExitAt(fromPosition, new Exit(to, toPosition)
      {
        IsAnAIExit = isAnAIExit
      });
      from.GetTileAt(fromPosition).AddDecoration(exitImageID);
    }

    protected void MakeWalkwayZones(Map map, BaseTownGenerator.Block b)
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

      public District District { get; set; }

      public bool GeneratePoliceStation { get; set; }

      public bool GenerateHospital { get; set; }

      public int MapWidth {
        get {
          return m_MapWidth;
        }
        set {
          if (value <= 0 || value > RogueGame.MAP_MAX_WIDTH)
            throw new ArgumentOutOfRangeException("MapWidth");
          m_MapWidth = value;
        }
      }

      public int MapHeight {
        get {
          return m_MapHeight;
        }
        set {
          if (value <= 0 || value > RogueGame.MAP_MAX_HEIGHT)
            throw new ArgumentOutOfRangeException("MapHeight");
          m_MapHeight = value;
        }
      }

      public int MinBlockSize
      {
        get
        {
          return m_MinBlockSize;
        }
        set
        {
          if (value < 4 || value > 32)
            throw new ArgumentOutOfRangeException("MinBlockSize must be [4..32]");
                    m_MinBlockSize = value;
        }
      }

      public int WreckedCarChance
      {
        get
        {
          return m_WreckedCarChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("WreckedCarChance must be [0..100]");
                    m_WreckedCarChance = value;
        }
      }

      public int ShopBuildingChance
      {
        get
        {
          return m_ShopBuildingChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("ShopBuildingChance must be [0..100]");
                    m_ShopBuildingChance = value;
        }
      }

      public int ParkBuildingChance
      {
        get
        {
          return m_ParkBuildingChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("ParkBuildingChance must be [0..100]");
                    m_ParkBuildingChance = value;
        }
      }

      public int CHARBuildingChance
      {
        get
        {
          return m_CHARBuildingChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("CHARBuildingChance must be [0..100]");
                    m_CHARBuildingChance = value;
        }
      }

      public int PostersChance
      {
        get
        {
          return m_PostersChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("PostersChance must be [0..100]");
                    m_PostersChance = value;
        }
      }

      public int TagsChance
      {
        get
        {
          return m_TagsChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("TagsChance must be [0..100]");
                    m_TagsChance = value;
        }
      }

      public int ItemInShopShelfChance
      {
        get
        {
          return m_ItemInShopShelfChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("ItemInShopShelfChance must be [0..100]");
                    m_ItemInShopShelfChance = value;
        }
      }

      public int PolicemanChance
      {
        get
        {
          return m_PolicemanChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("PolicemanChance must be [0..100]");
                    m_PolicemanChance = value;
        }
      }
    }

    public class Block
    {
      public Rectangle Rectangle { get; set; }

      public Rectangle BuildingRect { get; set; }

      public Rectangle InsideRect { get; set; }

      public Block(Rectangle rect)
      {
                ResetRectangle(rect);
      }

      public Block(BaseTownGenerator.Block copyFrom)
      {
                Rectangle = copyFrom.Rectangle;
                BuildingRect = copyFrom.BuildingRect;
                InsideRect = copyFrom.InsideRect;
      }

      public void ResetRectangle(Rectangle rect)
      {
                Rectangle = rect;
                BuildingRect = new Rectangle(rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2);
                InsideRect = new Rectangle(BuildingRect.Left + 1, BuildingRect.Top + 1, BuildingRect.Width - 2, BuildingRect.Height - 2);
      }
    }

    protected enum ShopType : byte
    {
      GENERAL_STORE = 0,
      _FIRST = 0,
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
