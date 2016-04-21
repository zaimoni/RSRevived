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
        return this.m_Params;
      }
      set
      {
        this.m_Params = value;
      }
    }

    public BaseTownGenerator(RogueGame game, BaseTownGenerator.Parameters parameters)
      : base(game)
    {
      this.m_Params = parameters;
      this.m_DiceRoller = new DiceRoller();
    }

    public override Map Generate(int seed)
    {
      this.m_DiceRoller = new DiceRoller(seed);
      Map map = new Map(seed, "Base City", this.m_Params.MapWidth, this.m_Params.MapHeight);
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_GRASS);
      List<BaseTownGenerator.Block> list = new List<BaseTownGenerator.Block>();
      Rectangle rect = new Rectangle(0, 0, map.Width, map.Height);
      this.MakeBlocks(map, true, ref list, rect);
      List<BaseTownGenerator.Block> blockList1 = new List<BaseTownGenerator.Block>((IEnumerable<BaseTownGenerator.Block>) list);
      List<BaseTownGenerator.Block> blockList2 = new List<BaseTownGenerator.Block>(blockList1.Count);
      this.m_SurfaceBlocks = new List<BaseTownGenerator.Block>(list.Count);
      foreach (BaseTownGenerator.Block copyFrom in list)
        this.m_SurfaceBlocks.Add(new BaseTownGenerator.Block(copyFrom));
      if (this.m_Params.GeneratePoliceStation)
      {
        BaseTownGenerator.Block policeBlock;
        this.MakePoliceStation(map, list, out policeBlock);
        blockList1.Remove(policeBlock);
      }
      if (this.m_Params.GenerateHospital)
      {
        BaseTownGenerator.Block hospitalBlock;
        this.MakeHospital(map, list, out hospitalBlock);
        blockList1.Remove(hospitalBlock);
      }
      blockList2.Clear();
      foreach (BaseTownGenerator.Block b in blockList1)
      {
        if (this.m_DiceRoller.RollChance(this.m_Params.ShopBuildingChance) && this.MakeShopBuilding(map, b))
          blockList2.Add(b);
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      int num = 0;
      foreach (BaseTownGenerator.Block b in blockList1)
      {
        if (this.m_Params.District.Kind == DistrictKind.BUSINESS && num == 0 || this.m_DiceRoller.RollChance(this.m_Params.CHARBuildingChance))
        {
          BaseTownGenerator.CHARBuildingType charBuildingType = this.MakeCHARBuilding(map, b);
          if (charBuildingType == BaseTownGenerator.CHARBuildingType.OFFICE)
          {
            ++num;
            this.PopulateCHAROfficeBuilding(map, b);
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
        if (this.m_DiceRoller.RollChance(this.m_Params.ParkBuildingChance) && this.MakeParkBuilding(map, b))
          blockList2.Add(b);
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
      blockList2.Clear();
      foreach (BaseTownGenerator.Block b in blockList1)
      {
        this.MakeHousingBuilding(map, b);
        blockList2.Add(b);
      }
      foreach (BaseTownGenerator.Block block in blockList2)
        blockList1.Remove(block);
      this.AddWreckedCarsOutside(map, rect);
      this.DecorateOutsideWallsWithPosters(map, rect, this.m_Params.PostersChance);
      this.DecorateOutsideWallsWithTags(map, rect, this.m_Params.TagsChance);
      return map;
    }

    public virtual Map GenerateSewersMap(int seed, District district)
    {
      this.m_DiceRoller = new DiceRoller(seed);
      Map sewers = new Map(seed, "sewers", district.EntryMap.Width, district.EntryMap.Height)
      {
        Lighting = Lighting._FIRST
      };
      sewers.AddZone(this.MakeUniqueZone("sewers", sewers.Rect));
      this.TileFill(sewers, this.m_Game.GameTiles.WALL_SEWER);
      Map surface = district.EntryMap;
      List<BaseTownGenerator.Block> list = new List<BaseTownGenerator.Block>(this.m_SurfaceBlocks.Count);
      this.MakeBlocks(sewers, false, ref list, new Rectangle(0, 0, sewers.Width, sewers.Height));
      foreach (BaseTownGenerator.Block block in list)
        this.TileRectangle(sewers, this.m_Game.GameTiles.FLOOR_SEWER_WATER, block.Rectangle);
      foreach (BaseTownGenerator.Block block in list)
      {
        if (m_DiceRoller.RollChance(SEWERS_IRON_FENCE_PER_BLOCK_CHANCE))
        {
          bool flag = false;
          int x1;
          int y1;
          int x2;
          int y2;
          do
          {
            int num = this.m_DiceRoller.Roll(0, 4);
            switch (num)
            {
              case 0:
              case 1:
                x1 = this.m_DiceRoller.Roll(block.Rectangle.Left, block.Rectangle.Right - 1);
                y1 = num == 0 ? block.Rectangle.Top : block.Rectangle.Bottom - 1;
                x2 = x1;
                y2 = num == 0 ? y1 - 1 : y1 + 1;
                break;
              case 2:
              case 3:
                x1 = num == 2 ? block.Rectangle.Left : block.Rectangle.Right - 1;
                y1 = this.m_DiceRoller.Roll(block.Rectangle.Top, block.Rectangle.Bottom - 1);
                x2 = num == 2 ? x1 - 1 : x1 + 1;
                y2 = y1;
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled roll");
            }
            if (!sewers.IsOnMapBorder(x1, y1) && !sewers.IsOnMapBorder(x2, y2) && (this.CountAdjWalls(sewers, x1, y1) == 3 && this.CountAdjWalls(sewers, x2, y2) == 3))
              flag = true;
          }
          while (!flag);
          this.MapObjectPlace(sewers, x1, y1, this.MakeObjIronFence("MapObjects\\iron_fence"));
          this.MapObjectPlace(sewers, x2, y2, this.MakeObjIronFence("MapObjects\\iron_fence"));
        }
      }
      int num1 = 0;
      do
      {
        for (int x = 0; x < sewers.Width; ++x)
        {
          for (int y = 0; y < sewers.Height; ++y)
          {
            if (this.m_DiceRoller.RollChance(3) && sewers.GetTileAt(x, y).Model.IsWalkable)
            {
              Tile tileAt = surface.GetTileAt(x, y);
              if (tileAt.Model.IsWalkable && sewers.GetMapObjectAt(x, y) == null && !tileAt.IsInside && ((tileAt.Model == this.m_Game.GameTiles.FLOOR_WALKWAY || tileAt.Model == this.m_Game.GameTiles.FLOOR_GRASS) && surface.GetMapObjectAt(x, y) == null))
              {
                Point point = new Point(x, y);
                if (!sewers.HasAnyAdjacentInMap(point, (Predicate<Point>) (p => sewers.GetExitAt(p) != null)) && !surface.HasAnyAdjacentInMap(point, (Predicate<Point>) (p => surface.GetExitAt(p) != null)))
                {
                  this.AddExit(sewers, point, surface, point, "Tiles\\Decoration\\sewer_ladder", true);
                  this.AddExit(surface, point, sewers, point, "Tiles\\Decoration\\sewer_hole", true);
                  ++num1;
                }
              }
            }
          }
        }
      }
      while (num1 < 1);
      for (int x = 0; x < sewers.Width; ++x)
      {
        for (int y = 0; y < sewers.Height; ++y)
          sewers.GetTileAt(x, y).IsInside = true;
      }
      List<BaseTownGenerator.Block> blockList = (List<BaseTownGenerator.Block>) null;
      foreach (BaseTownGenerator.Block mSurfaceBlock in this.m_SurfaceBlocks)
      {
        if (mSurfaceBlock.BuildingRect.Width <= this.m_Params.MinBlockSize + 2 && (mSurfaceBlock.BuildingRect.Height <= this.m_Params.MinBlockSize + 2 && !this.IsThereASpecialBuilding(surface, mSurfaceBlock.InsideRect)))
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
              blockList = new List<BaseTownGenerator.Block>(this.m_SurfaceBlocks.Count);
            blockList.Add(mSurfaceBlock);
            break;
          }
        }
      }
      Rectangle buildingRect;
      if (blockList != null)
      {
        BaseTownGenerator.Block block = blockList[this.m_DiceRoller.Roll(0, blockList.Count)];
        this.ClearRectangle(surface, block.BuildingRect);
        this.TileFill(surface, this.m_Game.GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        this.m_SurfaceBlocks.Remove(block);
        BaseTownGenerator.Block b1 = new BaseTownGenerator.Block(block.Rectangle);
        buildingRect = b1.BuildingRect;
        int x = buildingRect.Left + buildingRect.Width / 2;
        int top = buildingRect.Top;
        int num2 = buildingRect.Height / 2;
        int y = top + num2;
        Point exitPosition = new Point(x, y);
        this.MakeSewersMaintenanceBuilding(surface, true, b1, sewers, exitPosition);
        BaseTownGenerator.Block b2 = new BaseTownGenerator.Block(block.Rectangle);
        this.MakeSewersMaintenanceBuilding(sewers, false, b2, surface, exitPosition);
      }
      foreach (BaseTownGenerator.Block block in list)
      {
        if (m_DiceRoller.RollChance(SEWERS_ROOM_CHANCE) && CheckForEachTile(sewers, block.BuildingRect, (Predicate<Point>) (pt => !sewers.GetTileAt(pt).Model.IsWalkable)))
        {
          this.TileFill(sewers, this.m_Game.GameTiles.FLOOR_CONCRETE, block.InsideRect);
          Map map1 = sewers;
          buildingRect = block.BuildingRect;
          int left1 = buildingRect.Left;
          buildingRect = block.BuildingRect;
          int num2 = buildingRect.Width / 2;
          int x1 = left1 + num2;
          buildingRect = block.BuildingRect;
          int top1 = buildingRect.Top;
          TileModel floorConcrete1 = this.m_Game.GameTiles.FLOOR_CONCRETE;
          map1.SetTileModelAt(x1, top1, floorConcrete1);
          Map map2 = sewers;
          buildingRect = block.BuildingRect;
          int left2 = buildingRect.Left;
          buildingRect = block.BuildingRect;
          int num3 = buildingRect.Width / 2;
          int x2 = left2 + num3;
          buildingRect = block.BuildingRect;
          int y1 = buildingRect.Bottom - 1;
          TileModel floorConcrete2 = this.m_Game.GameTiles.FLOOR_CONCRETE;
          map2.SetTileModelAt(x2, y1, floorConcrete2);
          Map map3 = sewers;
          buildingRect = block.BuildingRect;
          int left3 = buildingRect.Left;
          buildingRect = block.BuildingRect;
          int top2 = buildingRect.Top;
          buildingRect = block.BuildingRect;
          int num4 = buildingRect.Height / 2;
          int y2 = top2 + num4;
          TileModel floorConcrete3 = this.m_Game.GameTiles.FLOOR_CONCRETE;
          map3.SetTileModelAt(left3, y2, floorConcrete3);
          Map map4 = sewers;
          buildingRect = block.BuildingRect;
          int x3 = buildingRect.Right - 1;
          buildingRect = block.BuildingRect;
          int top3 = buildingRect.Top;
          buildingRect = block.BuildingRect;
          int num5 = buildingRect.Height / 2;
          int y3 = top3 + num5;
          TileModel floorConcrete4 = this.m_Game.GameTiles.FLOOR_CONCRETE;
          map4.SetTileModelAt(x3, y3, floorConcrete4);
          sewers.AddZone(this.MakeUniqueZone("room", block.InsideRect));
        }
      }
      this.MapObjectFill(sewers, new Rectangle(0, 0, sewers.Width, sewers.Height), (Func<Point, MapObject>) (pt =>
      {
        if (!m_DiceRoller.RollChance(SEWERS_JUNK_CHANCE))
          return (MapObject) null;
        if (!sewers.IsWalkable(pt.X, pt.Y))
          return (MapObject) null;
        return this.MakeObjJunk("MapObjects\\junk");
      }));
      for (int x = 0; x < sewers.Width; ++x)
      {
        for (int y = 0; y < sewers.Height; ++y)
        {
          if (m_DiceRoller.RollChance(SEWERS_ITEM_CHANCE) && sewers.IsWalkable(x, y))
          {
            Item it;
            switch (this.m_DiceRoller.Roll(0, 3))
            {
              case 0:
                it = this.MakeItemBigFlashlight();
                break;
              case 1:
                it = this.MakeItemCrowbar();
                break;
              case 2:
                it = this.MakeItemSprayPaint();
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled roll");
            }
            sewers.DropItemAt(it, x, y);
          }
        }
      }
      for (int x = 0; x < sewers.Width; ++x)
      {
        for (int y = 0; y < sewers.Height; ++y)
        {
          if (m_DiceRoller.RollChance(SEWERS_TAG_CHANCE))
          {
            Tile tileAt = sewers.GetTileAt(x, y);
            if (!tileAt.Model.IsWalkable && this.CountAdjWalkables(sewers, x, y) >= 2)
              tileAt.AddDecoration(BaseTownGenerator.TAGS[this.m_DiceRoller.Roll(0, BaseTownGenerator.TAGS.Length)]);
          }
        }
      }
      return sewers;
    }

    public virtual Map GenerateSubwayMap(int seed, District district)
    {
      this.m_DiceRoller = new DiceRoller(seed);
      Map subway = new Map(seed, "subway", district.EntryMap.Width, district.EntryMap.Height)
      {
        Lighting = Lighting._FIRST
      };
      this.TileFill(subway, this.m_Game.GameTiles.WALL_BRICK);
      Map entryMap = district.EntryMap;
      int x1 = 0;
      int num1 = subway.Width - 1;
      int y1 = subway.Width / 2 - 1;
      int height = 4;
      for (int x2 = x1; x2 <= num1; ++x2)
      {
        for (int y2 = y1; y2 < y1 + height; ++y2)
          subway.SetTileModelAt(x2, y2, this.m_Game.GameTiles.RAIL_EW);
      }
      subway.AddZone(this.MakeUniqueZone("rails", new Rectangle(x1, y1, num1 - x1 + 1, height)));
      List<BaseTownGenerator.Block> blockList = (List<BaseTownGenerator.Block>) null;
      foreach (BaseTownGenerator.Block mSurfaceBlock in this.m_SurfaceBlocks)
      {
        if (mSurfaceBlock.BuildingRect.Width <= this.m_Params.MinBlockSize + 2 && (mSurfaceBlock.BuildingRect.Height <= this.m_Params.MinBlockSize + 2 && !this.IsThereASpecialBuilding(entryMap, mSurfaceBlock.InsideRect)))
        {
          bool flag = true;
          int num2 = 8;
          for (int x2 = mSurfaceBlock.Rectangle.Left - num2; x2 < mSurfaceBlock.Rectangle.Right + num2 && flag; ++x2)
          {
            for (int y2 = mSurfaceBlock.Rectangle.Top - num2; y2 < mSurfaceBlock.Rectangle.Bottom + num2 && flag; ++y2)
            {
              if (subway.IsInBounds(x2, y2) && subway.GetTileAt(x2, y2).Model.IsWalkable)
                flag = false;
            }
          }
          if (flag)
          {
            if (blockList == null)
              blockList = new List<BaseTownGenerator.Block>(this.m_SurfaceBlocks.Count);
            blockList.Add(mSurfaceBlock);
            break;
          }
        }
      }
      if (blockList != null)
      {
        BaseTownGenerator.Block block = blockList[this.m_DiceRoller.Roll(0, blockList.Count)];
        this.ClearRectangle(entryMap, block.BuildingRect);
        this.TileFill(entryMap, this.m_Game.GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        this.m_SurfaceBlocks.Remove(block);
        BaseTownGenerator.Block b1 = new BaseTownGenerator.Block(block.Rectangle);
        Point exitPosition = new Point(b1.BuildingRect.Left + b1.BuildingRect.Width / 2, b1.InsideRect.Top);
        this.MakeSubwayStationBuilding(entryMap, true, b1, subway, exitPosition);
        BaseTownGenerator.Block b2 = new BaseTownGenerator.Block(block.Rectangle);
        this.MakeSubwayStationBuilding(subway, false, b2, entryMap, exitPosition);
      }
      Direction direction = this.m_DiceRoller.RollChance(50) ? Direction.N : Direction.S;
      Rectangle rect = Rectangle.Empty;
      bool flag1 = false;
      int num3 = 0;
      do
      {
        int x2 = this.m_DiceRoller.Roll(10, subway.Width - 10);
        int y2 = direction == Direction.N ? y1 - 1 : y1 + height;
        if (!subway.GetTileAt(x2, y2).Model.IsWalkable)
        {
          rect = direction != Direction.N ? new Rectangle(x2, y2, 5, 5) : new Rectangle(x2, y2 - 5 + 1, 5, 5);
          flag1 = this.CheckForEachTile(subway, rect, (Predicate<Point>) (pt => !subway.GetTileAt(pt).Model.IsWalkable));
        }
        ++num3;
      }
      while (num3 < subway.Width * subway.Height && !flag1);
      if (flag1)
      {
        this.TileFill(subway, this.m_Game.GameTiles.FLOOR_CONCRETE, rect);
        this.TileRectangle(subway, this.m_Game.GameTiles.WALL_BRICK, rect);
        this.PlaceDoor(subway, rect.Left + 2, direction == Direction.N ? rect.Bottom - 1 : rect.Top, this.m_Game.GameTiles.FLOOR_CONCRETE, this.MakeObjIronDoor());
        subway.AddZone(this.MakeUniqueZone("tools room", rect));
        this.DoForEachTile(subway, rect, (Action<Point>) (pt =>
        {
          if (!subway.IsWalkable(pt.X, pt.Y) || this.CountAdjWalls(subway, pt.X, pt.Y) == 0 || this.CountAdjDoors(subway, pt.X, pt.Y) > 0)
            return;
          subway.PlaceMapObjectAt(this.MakeObjShelf("MapObjects\\shop_shelf"), pt);
          subway.DropItemAt(this.MakeShopConstructionItem(), pt);
        }));
      }
      for (int x2 = 0; x2 < subway.Width; ++x2)
      {
        for (int y2 = 0; y2 < subway.Height; ++y2)
        {
          if (m_DiceRoller.RollChance(SUBWAY_TAGS_POSTERS_CHANCE))
          {
            Tile tileAt = subway.GetTileAt(x2, y2);
            if (!tileAt.Model.IsWalkable && this.CountAdjWalkables(subway, x2, y2) >= 2)
            {
              if (this.m_DiceRoller.RollChance(50))
                tileAt.AddDecoration(BaseTownGenerator.POSTERS[this.m_DiceRoller.Roll(0, BaseTownGenerator.POSTERS.Length)]);
              if (this.m_DiceRoller.RollChance(50))
                tileAt.AddDecoration(BaseTownGenerator.TAGS[this.m_DiceRoller.Roll(0, BaseTownGenerator.TAGS.Length)]);
            }
          }
        }
      }
      for (int x2 = 0; x2 < subway.Width; ++x2)
      {
        for (int y2 = 0; y2 < subway.Height; ++y2)
          subway.GetTileAt(x2, y2).IsInside = true;
      }
      return subway;
    }

    private void QuadSplit(Rectangle rect, int minWidth, int minHeight, out int splitX, out int splitY, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight)
    {
      int width1 = this.m_DiceRoller.Roll(rect.Width / 3, 2 * rect.Width / 3);
      int height1 = this.m_DiceRoller.Roll(rect.Height / 3, 2 * rect.Height / 3);
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
      this.QuadSplit(rect, this.m_Params.MinBlockSize + 1, this.m_Params.MinBlockSize + 1, out splitX, out splitY, out topLeft, out topRight, out bottomLeft, out bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty)
      {
        if (makeRoads)
        {
          this.MakeRoad(map, this.m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_EW], new Rectangle(rect.Left, rect.Top, rect.Width, 1));
          this.MakeRoad(map, this.m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_EW], new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1));
          this.MakeRoad(map, this.m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_NS], new Rectangle(rect.Left, rect.Top, 1, rect.Height));
          this.MakeRoad(map, this.m_Game.GameTiles[GameTiles.IDs.ROAD_ASPHALT_NS], new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height));
          topLeft.Width -= 2;
          topLeft.Height -= 2;
          topLeft.Offset(1, 1);
        }
        list.Add(new BaseTownGenerator.Block(topLeft));
      }
      else
      {
        this.MakeBlocks(map, makeRoads, ref list, topLeft);
        if (!topRight.IsEmpty)
          this.MakeBlocks(map, makeRoads, ref list, topRight);
        if (!bottomLeft.IsEmpty)
          this.MakeBlocks(map, makeRoads, ref list, bottomLeft);
        if (bottomRight.IsEmpty)
          return;
        this.MakeBlocks(map, makeRoads, ref list, bottomRight);
      }
    }

    protected virtual void MakeRoad(Map map, TileModel roadModel, Rectangle rect)
    {
      this.TileFill(map, roadModel, rect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        if (!this.m_Game.GameTiles.IsRoadModel(prevmodel))
          return;
        map.SetTileModelAt(x, y, prevmodel);
      }));
      map.AddZone(this.MakeUniqueZone("road", rect));
    }

    protected virtual void PlaceDoor(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      map.SetTileModelAt(x, y, floor);
      this.MapObjectPlace(map, x, y, (MapObject) door);
    }

    protected virtual void PlaceDoorIfNoObject(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      if (map.GetMapObjectAt(x, y) != null)
        return;
      this.PlaceDoor(map, x, y, floor, door);
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
      this.PlaceDoorIfNoObject(map, x, y, floor, door);
      return true;
    }

    protected virtual bool PlaceDoorIfAccessibleAndNotAdjacent(Map map, int x, int y, TileModel floor, int minAccessibility, DoorWindow door)
    {
      int num = 0;
      Point point1 = new Point(x, y);
      foreach (Direction direction in Direction.COMPASS)
      {
        Point point2 = point1 + direction;
        if (map.IsWalkable(point2.X, point2.Y))
          ++num;
        if (map.GetMapObjectAt(point2.X, point2.Y) is DoorWindow)
          return false;
      }
      if (num < minAccessibility)
        return false;
      this.PlaceDoorIfNoObject(map, x, y, floor, door);
      return true;
    }

    protected virtual void AddWreckedCarsOutside(Map map, Rectangle rect)
    {
      this.MapObjectFill(map, rect, (Func<Point, MapObject>) (pt =>
      {
        if (this.m_DiceRoller.RollChance(this.m_Params.WreckedCarChance))
        {
          Tile tileAt = map.GetTileAt(pt.X, pt.Y);
          if (!tileAt.IsInside && tileAt.Model.IsWalkable && tileAt.Model != this.m_Game.GameTiles.FLOOR_GRASS)
          {
            MapObject mapObj = this.MakeObjWreckedCar(this.m_DiceRoller);
            if (this.m_DiceRoller.RollChance(50))
              this.m_Game.ApplyOnFire(mapObj);
            return mapObj;
          }
        }
        return (MapObject) null;
      }));
    }

    protected bool IsThereASpecialBuilding(Map map, Rectangle rect)
    {
      List<Zone> zonesAt = map.GetZonesAt(rect.Left, rect.Top);
      if (zonesAt != null)
      {
        bool flag = false;
        foreach (Zone zone in zonesAt)
        {
          if (zone.Name.Contains("Sewers Maintenance") || zone.Name.Contains("Subway Station") || (zone.Name.Contains("office") || zone.Name.Contains("shop")))
          {
            flag = true;
            break;
          }
        }
        if (flag)
          return true;
      }
      return map.HasAnExitIn(rect);
    }

    protected virtual bool MakeShopBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 5 || b.InsideRect.Height < 5)
        return false;
      this.TileRectangle(map, this.m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_STONE, b.BuildingRect);
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) => tile.IsInside = true));
      BaseTownGenerator.ShopType shopType = (BaseTownGenerator.ShopType) this.m_DiceRoller.Roll(0, 7);
      int left1 = b.InsideRect.Left;
      int top1 = b.InsideRect.Top;
      int right = b.InsideRect.Right;
      int bottom = b.InsideRect.Bottom;
      bool horizontalAlleys = b.Rectangle.Width >= b.Rectangle.Height;
      int centralAlley;
      if (horizontalAlleys)
      {
        ++left1;
        --right;
        centralAlley = b.InsideRect.Left + b.InsideRect.Width / 2;
      }
      else
      {
        ++top1;
        --bottom;
        centralAlley = b.InsideRect.Top + b.InsideRect.Height / 2;
      }
      Rectangle alleysRect = Rectangle.FromLTRB(left1, top1, right, bottom);
      this.MapObjectFill(map, alleysRect, (Func<Point, MapObject>) (pt =>
      {
        if (!horizontalAlleys ? (pt.X - alleysRect.Left) % 2 == 1 && pt.Y != centralAlley : (pt.Y - alleysRect.Top) % 2 == 1 && pt.X != centralAlley)
          return this.MakeObjShelf("MapObjects\\shop_shelf");
        return (MapObject) null;
      }));
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      if (horizontalAlleys)
      {
        if (this.m_DiceRoller.RollChance(50))
        {
          this.PlaceDoor(map, b.BuildingRect.Left, y1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
            this.PlaceDoor(map, b.BuildingRect.Left, y1 - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              this.PlaceDoor(map, b.BuildingRect.Left, y1 + 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          }
        }
        else
        {
          this.PlaceDoor(map, b.BuildingRect.Right - 1, y1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
            this.PlaceDoor(map, b.BuildingRect.Right - 1, y1 - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              this.PlaceDoor(map, b.BuildingRect.Right - 1, y1 + 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          }
        }
      }
      else if (this.m_DiceRoller.RollChance(50))
      {
        this.PlaceDoor(map, x1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
          this.PlaceDoor(map, x1 - 1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            this.PlaceDoor(map, x1 + 1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        }
      }
      else
      {
        this.PlaceDoor(map, x1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
          this.PlaceDoor(map, x1 - 1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            this.PlaceDoor(map, x1 + 1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        }
      }
      string basename;
      string shopImage;
      switch (shopType)
      {
        case BaseTownGenerator.ShopType._FIRST:
          shopImage = "Tiles\\Decoration\\shop_general_store";
          basename = "GeneralStore";
          break;
        case BaseTownGenerator.ShopType.GROCERY:
          shopImage = "Tiles\\Decoration\\shop_grocery";
          basename = "Grocery";
          break;
        case BaseTownGenerator.ShopType.SPORTSWEAR:
          shopImage = "Tiles\\Decoration\\shop_sportswear";
          basename = "Sportswear";
          break;
        case BaseTownGenerator.ShopType.PHARMACY:
          shopImage = "Tiles\\Decoration\\shop_pharmacy";
          basename = "Pharmacy";
          break;
        case BaseTownGenerator.ShopType.CONSTRUCTION:
          shopImage = "Tiles\\Decoration\\shop_construction";
          basename = "Construction";
          break;
        case BaseTownGenerator.ShopType.GUNSHOP:
          shopImage = "Tiles\\Decoration\\shop_gunshop";
          basename = "Gunshop";
          break;
        case BaseTownGenerator.ShopType.HUNTING:
          shopImage = "Tiles\\Decoration\\shop_hunting";
          basename = "Hunting Shop";
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled shoptype");
      }
      this.DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.GetMapObjectAt(x, y) != null || this.CountAdjDoors(map, x, y) < 1)
          return (string) null;
        return shopImage;
      }));
      Rectangle rectangle;
      if (m_DiceRoller.RollChance(SHOP_WINDOW_CHANCE))
      {
        int x2;
        int y2;
        rectangle = b.BuildingRect;
        switch (m_DiceRoller.Roll(0, 4))
        {
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
          case 3:
            x2 = rectangle.Right - 1;
            y2 = rectangle.Top + rectangle.Height / 2;
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled side");
        }
        bool flag = true;
        if (map.GetTileAt(x2, y2).Model.IsWalkable)
          flag = false;
        if (flag)
          this.PlaceDoor(map, x2, y2, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjWindow());
      }
      if (shopType == BaseTownGenerator.ShopType.GUNSHOP)
        this.BarricadeDoors(map, b.BuildingRect, 80);
      this.ItemsDrop(map, b.InsideRect, (Func<Point, bool>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || !(mapObjectAt.ImageID == "MapObjects\\shop_shelf"))
          return false;
        return this.m_DiceRoller.RollChance(this.m_Params.ItemInShopShelfChance);
      }), (Func<Point, Item>) (pt => this.MakeRandomShopItem(shopType)));
      map.AddZone(this.MakeUniqueZone(basename, b.BuildingRect));
      this.MakeWalkwayZones(map, b);
      if (this.m_DiceRoller.RollChance(SHOP_BASEMENT_CHANCE))
      {
        int seed = map.Seed << 1 ^ basename.GetHashCode();
        string name = "basement-" + basename;
        rectangle = b.BuildingRect;
        int width = rectangle.Width;
        int height = rectangle.Height;
        Map shopBasement = new Map(seed, name, width, height)
        {
          Lighting = Lighting._FIRST
        };
        this.DoForEachTile(shopBasement, shopBasement.Rect, (Action<Point>) (pt => shopBasement.GetTileAt(pt).IsInside = true));
        this.TileFill(shopBasement, this.m_Game.GameTiles.FLOOR_CONCRETE);
        this.TileRectangle(shopBasement, this.m_Game.GameTiles.WALL_BRICK, shopBasement.Rect);
        shopBasement.AddZone(this.MakeUniqueZone("basement", shopBasement.Rect));
        this.DoForEachTile(shopBasement, shopBasement.Rect, (Action<Point>) (pt =>
        {
          if (!shopBasement.IsWalkable(pt.X, pt.Y) || shopBasement.GetExitAt(pt) != null)
            return;
          if (m_DiceRoller.RollChance(SHOP_BASEMENT_SHELF_CHANCE_PER_TILE))
          {
            shopBasement.PlaceMapObjectAt(this.MakeObjShelf("MapObjects\\shop_shelf"), pt);
            if (m_DiceRoller.RollChance(SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF))
            {
              Item it = MakeRandomShopItem(shopType);
              if (it != null)
                shopBasement.DropItemAt(it, pt);
            }
          }
          if (!Rules.HasZombiesInBasements(this.m_Game.Session.GameMode) || !m_DiceRoller.RollChance(SHOP_BASEMENT_ZOMBIE_RAT_CHANCE))
            return;
          shopBasement.PlaceActorAt(this.CreateNewBasementRatZombie(0), pt);
        }));
        Point point1 = new Point();
        point1.X = this.m_DiceRoller.RollChance(50) ? 1 : shopBasement.Width - 2;
        point1.Y = this.m_DiceRoller.RollChance(50) ? 1 : shopBasement.Height - 2;
        int num1 = point1.X - 1;
        rectangle = b.InsideRect;
        int left2 = rectangle.Left;
        int x2 = num1 + left2;
        int num2 = point1.Y - 1;
        int top2 = rectangle.Top;
        int y2 = num2 + top2;
        Point point2 = new Point(x2, y2);
        this.AddExit(shopBasement, point1, map, point2, "Tiles\\Decoration\\stairs_up", true);
        this.AddExit(map, point2, shopBasement, point1, "Tiles\\Decoration\\stairs_down", true);
        if (map.GetMapObjectAt(point2) != null)
          map.RemoveMapObjectAt(point2.X, point2.Y);
        this.m_Params.District.AddUniqueMap(shopBasement);
      }
      return true;
    }

    protected virtual BaseTownGenerator.CHARBuildingType MakeCHARBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 8 || b.InsideRect.Height < 8)
        return this.MakeCHARAgency(map, b) ? BaseTownGenerator.CHARBuildingType.AGENCY : BaseTownGenerator.CHARBuildingType.NONE;
      return this.MakeCHAROffice(map, b) ? BaseTownGenerator.CHARBuildingType.OFFICE : BaseTownGenerator.CHARBuildingType.NONE;
    }

    protected virtual bool MakeCHARAgency(Map map, BaseTownGenerator.Block b)
    {
      this.TileRectangle(map, this.m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_OFFICE, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        tile.IsInside = true;
        tile.AddDecoration("Tiles\\Decoration\\char_floor_logo");
      }));
      bool flag = b.InsideRect.Width >= b.InsideRect.Height;
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      if (flag)
      {
        if (this.m_DiceRoller.RollChance(50))
        {
          this.PlaceDoor(map, b.BuildingRect.Left, y1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
            this.PlaceDoor(map, b.BuildingRect.Left, y1 - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              this.PlaceDoor(map, b.BuildingRect.Left, y1 + 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          }
        }
        else
        {
          this.PlaceDoor(map, b.BuildingRect.Right - 1, y1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
            this.PlaceDoor(map, b.BuildingRect.Right - 1, y1 - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              this.PlaceDoor(map, b.BuildingRect.Right - 1, y1 + 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          }
        }
      }
      else if (this.m_DiceRoller.RollChance(50))
      {
        this.PlaceDoor(map, x1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
          this.PlaceDoor(map, x1 - 1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            this.PlaceDoor(map, x1 + 1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        }
      }
      else
      {
        this.PlaceDoor(map, x1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
          this.PlaceDoor(map, x1 - 1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            this.PlaceDoor(map, x1 + 1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        }
      }
      string officeImage = "Tiles\\Decoration\\char_office";
      this.DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.GetMapObjectAt(x, y) != null || this.CountAdjDoors(map, x, y) < 1)
          return (string) null;
        return officeImage;
      }));
      this.MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (this.CountAdjWalls(map, pt.X, pt.Y) < 3)
          return (MapObject) null;
        return this.MakeObjChair("MapObjects\\char_chair");
      }));
      this.TileFill(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, new Rectangle(b.InsideRect.Left + b.InsideRect.Width / 2 - 1, b.InsideRect.Top + b.InsideRect.Height / 2 - 1, 3, 2), (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration(BaseTownGenerator.CHAR_POSTERS[this.m_DiceRoller.Roll(0, BaseTownGenerator.CHAR_POSTERS.Length)])));
      this.DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (this.CountAdjDoors(map, x, y) > 0)
          return (string) null;
        if (this.m_DiceRoller.RollChance(25))
          return BaseTownGenerator.CHAR_POSTERS[this.m_DiceRoller.Roll(0, BaseTownGenerator.CHAR_POSTERS.Length)];
        return (string) null;
      }));
      map.AddZone(this.MakeUniqueZone("CHAR Agency", b.BuildingRect));
      this.MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeCHAROffice(Map map, BaseTownGenerator.Block b)
    {
      this.TileRectangle(map, this.m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_OFFICE, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) => tile.IsInside = true));
      Rectangle rectangle1 = b.InsideRect;
      bool flag = rectangle1.Width >= b.InsideRect.Height;
      int x1 = b.Rectangle.Left + b.Rectangle.Width / 2;
      int y1 = b.Rectangle.Top + b.Rectangle.Height / 2;
      Direction direction;
      if (flag)
      {
        if (this.m_DiceRoller.RollChance(50))
        {
          direction = Direction.W;
          this.PlaceDoor(map, b.BuildingRect.Left, y1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
            this.PlaceDoor(map, b.BuildingRect.Left, y1 - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              this.PlaceDoor(map, b.BuildingRect.Left, y1 + 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          }
        }
        else
        {
          direction = Direction.E;
          this.PlaceDoor(map, b.BuildingRect.Right - 1, y1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Height >= 8)
          {
            this.PlaceDoor(map, b.BuildingRect.Right - 1, y1 - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
            if (b.InsideRect.Height >= 12)
              this.PlaceDoor(map, b.BuildingRect.Right - 1, y1 + 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          }
        }
      }
      else if (this.m_DiceRoller.RollChance(50))
      {
        direction = Direction.N;
        this.PlaceDoor(map, x1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
          this.PlaceDoor(map, x1 - 1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            this.PlaceDoor(map, x1 + 1, b.BuildingRect.Top, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        }
      }
      else
      {
        direction = Direction.S;
        this.PlaceDoor(map, x1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        if (b.InsideRect.Width >= 8)
        {
          this.PlaceDoor(map, x1 - 1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
          if (b.InsideRect.Width >= 12)
            this.PlaceDoor(map, x1 + 1, b.BuildingRect.Bottom - 1, this.m_Game.GameTiles.FLOOR_WALKWAY, this.MakeObjGlassDoor());
        }
      }
      string officeImage = "Tiles\\Decoration\\char_office";
      this.DecorateOutsideWalls(map, b.BuildingRect, (Func<int, int, string>) ((x, y) =>
      {
        if (map.GetMapObjectAt(x, y) != null || this.CountAdjDoors(map, x, y) < 1)
          return (string) null;
        return officeImage;
      }));
      this.BarricadeDoors(map, b.BuildingRect, 80);
      if (direction == Direction.N)
        this.TileHLine(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left, b.InsideRect.Top + 3, b.InsideRect.Width);
      else if (direction == Direction.S)
      {
        Map map1 = map;
        TileModel wallCharOffice = this.m_Game.GameTiles.WALL_CHAR_OFFICE;
        int left = b.InsideRect.Left;
        int top = b.InsideRect.Bottom - 1 - 3;
        rectangle1 = b.InsideRect;
        int width = rectangle1.Width;
        this.TileHLine(map1, wallCharOffice, left, top, width);
      }
      else if (direction == Direction.E)
      {
        Map map1 = map;
        TileModel wallCharOffice = this.m_Game.GameTiles.WALL_CHAR_OFFICE;
        rectangle1 = b.InsideRect;
        int left = rectangle1.Right - 1 - 3;
        rectangle1 = b.InsideRect;
        int top = rectangle1.Top;
        rectangle1 = b.InsideRect;
        int height = rectangle1.Height;
        this.TileVLine(map1, wallCharOffice, left, top, height);
      }
      else
      {
        if (direction != Direction.W)
          throw new InvalidOperationException("unhandled door side");
        Map map1 = map;
        TileModel wallCharOffice = this.m_Game.GameTiles.WALL_CHAR_OFFICE;
        rectangle1 = b.InsideRect;
        int left = rectangle1.Left + 3;
        rectangle1 = b.InsideRect;
        int top = rectangle1.Top;
        rectangle1 = b.InsideRect;
        int height = rectangle1.Height;
        this.TileVLine(map1, wallCharOffice, left, top, height);
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
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, rect1);
      this.PlaceDoor(map, point.X, point.Y, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjCharDoor());
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
      this.MakeRoomsPlan(map, ref list1, rect2, 4);
      List<Rectangle> list2 = new List<Rectangle>();
      this.MakeRoomsPlan(map, ref list2, rect3, 4);
      List<Rectangle> rectangleList = new List<Rectangle>(list1.Count + list2.Count);
      rectangleList.AddRange((IEnumerable<Rectangle>) list1);
      rectangleList.AddRange((IEnumerable<Rectangle>) list2);
      foreach (Rectangle rect4 in list1)
      {
        this.TileRectangle(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, rect4);
        map.AddZone(this.MakeUniqueZone("Office room", rect4));
      }
      foreach (Rectangle rect4 in list2)
      {
        this.TileRectangle(map, this.m_Game.GameTiles.WALL_CHAR_OFFICE, rect4);
        map.AddZone(this.MakeUniqueZone("Office room", rect4));
      }
      foreach (Rectangle rectangle2 in list1)
      {
        if (flag)
          this.PlaceDoor(map, rectangle2.Left + rectangle2.Width / 2, rectangle2.Bottom - 1, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjCharDoor());
        else
          this.PlaceDoor(map, rectangle2.Right - 1, rectangle2.Top + rectangle2.Height / 2, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjCharDoor());
      }
      foreach (Rectangle rectangle2 in list2)
      {
        if (flag)
          this.PlaceDoor(map, rectangle2.Left + rectangle2.Width / 2, rectangle2.Top, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjCharDoor());
        else
          this.PlaceDoor(map, rectangle2.Left, rectangle2.Top + rectangle2.Height / 2, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjCharDoor());
      }
      foreach (Rectangle rectangle2 in rectangleList)
      {
        Point tablePos = new Point(rectangle2.Left + rectangle2.Width / 2, rectangle2.Top + rectangle2.Height / 2);
        map.PlaceMapObjectAt(this.MakeObjTable("MapObjects\\char_table"), tablePos);
        int num = 2;
        Rectangle rect4 = new Rectangle(rectangle2.Left + 1, rectangle2.Top + 1, rectangle2.Width - 2, rectangle2.Height - 2);
        if (!rect4.IsEmpty)
        {
          for (int index = 0; index < num; ++index)
          {
            Rectangle rect5 = new Rectangle(tablePos.X - 1, tablePos.Y - 1, 3, 3);
            rect5.Intersect(rect4);
            this.MapObjectPlaceInGoodPosition(map, rect5, (Func<Point, bool>) (pt => pt != tablePos), this.m_DiceRoller, (Func<Point, MapObject>) (pt => this.MakeObjChair("MapObjects\\char_chair")));
          }
        }
      }
      foreach (Rectangle rect4 in rectangleList)
        this.ItemsDrop(map, rect4, (Func<Point, bool>) (pt => map.GetTileAt(pt.X, pt.Y).Model == this.m_Game.GameTiles.FLOOR_OFFICE && map.GetMapObjectAt(pt) == null), (Func<Point, Item>) (pt => this.MakeRandomCHAROfficeItem()));
      Zone zone = this.MakeUniqueZone("CHAR Office", b.BuildingRect);
      zone.SetGameAttribute<bool>("CHAR Office", true);
      map.AddZone(zone);
      this.MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeParkBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 3 || b.InsideRect.Height < 3)
        return false;
      this.TileRectangle(map, this.m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_GRASS, b.InsideRect);
      this.MapObjectFill(map, b.BuildingRect, (Func<Point, MapObject>) (pt =>
      {
        if (pt.X == b.BuildingRect.Left || pt.X == b.BuildingRect.Right - 1 || pt.Y == b.BuildingRect.Top || pt.Y == b.BuildingRect.Bottom - 1)
          return this.MakeObjFence("MapObjects\\fence");
        return (MapObject) null;
      }));
      this.MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (m_DiceRoller.RollChance(PARK_TREE_CHANCE))
          return MakeObjTree("MapObjects\\tree");
        return null;
      }));
      this.MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (m_DiceRoller.RollChance(PARK_BENCH_CHANCE))
          return MakeObjBench("MapObjects\\bench");
        return null;
      }));
      int x;
      int y;
      switch (this.m_DiceRoller.Roll(0, 4))
      {
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
      map.SetTileModelAt(x, y, this.m_Game.GameTiles.FLOOR_WALKWAY);
      this.ItemsDrop(map, b.InsideRect, (Func<Point, bool>) (pt =>
      {
        if (map.GetMapObjectAt(pt) == null)
          return m_DiceRoller.RollChance(PARK_ITEM_CHANCE);
        return false;
      }), (Func<Point, Item>) (pt => this.MakeRandomParkItem()));
      map.AddZone(this.MakeUniqueZone("Park", b.BuildingRect));
      this.MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual bool MakeHousingBuilding(Map map, BaseTownGenerator.Block b)
    {
      if (b.InsideRect.Width < 4 || b.InsideRect.Height < 4)
        return false;
      this.TileRectangle(map, this.m_Game.GameTiles.FLOOR_WALKWAY, b.Rectangle);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_BRICK, b.BuildingRect);
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_PLANKS, b.InsideRect, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) => tile.IsInside = true));
      List<Rectangle> list = new List<Rectangle>();
      this.MakeRoomsPlan(map, ref list, b.BuildingRect, 5);
      foreach (Rectangle roomRect in list)
      {
        this.MakeHousingRoom(map, roomRect, this.m_Game.GameTiles.FLOOR_PLANKS, this.m_Game.GameTiles.WALL_BRICK);
        this.FillHousingRoomContents(map, roomRect);
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
          int x = this.m_DiceRoller.Roll(b.BuildingRect.Left, b.BuildingRect.Right);
          int y = this.m_DiceRoller.Roll(b.BuildingRect.Top, b.BuildingRect.Bottom);
          if (!map.GetTileAt(x, y).IsInside)
          {
            DoorWindow doorWindow = map.GetMapObjectAt(x, y) as DoorWindow;
            if (doorWindow != null && doorWindow.IsWindow)
            {
              map.RemoveMapObjectAt(x, y);
              map.PlaceMapObjectAt((MapObject) this.MakeObjWoodenDoor(), new Point(x, y));
              flag = true;
            }
          }
        }
        while (!flag);
      }
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_CHANCE))
        this.m_Params.District.AddUniqueMap(this.GenerateHouseBasementMap(map, b));
      map.AddZone(this.MakeUniqueZone("Housing", b.BuildingRect));
      this.MakeWalkwayZones(map, b);
      return true;
    }

    protected virtual void MakeSewersMaintenanceBuilding(Map map, bool isSurface, BaseTownGenerator.Block b, Map linkedMap, Point exitPosition)
    {
      if (!isSurface)
        this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_SEWER, b.BuildingRect);
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left)
      {
        for (int top = b.InsideRect.Top; top < b.InsideRect.Bottom; ++top)
          map.GetTileAt(left, top).IsInside = true;
      }
      Direction direction;
      int x;
      int y;
      switch (this.m_DiceRoller.Roll(0, 4))
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
      this.PlaceDoor(map, x, y, this.m_Game.GameTiles.FLOOR_CONCRETE, this.MakeObjIronDoor());
      this.BarricadeDoors(map, b.BuildingRect, 80);
      map.GetTileAt(exitPosition.X, exitPosition.Y).AddDecoration(isSurface ? "Tiles\\Decoration\\sewer_hole" : "Tiles\\Decoration\\sewer_ladder");
      map.SetExitAt(exitPosition, new Exit(linkedMap, exitPosition)
      {
        IsAnAIExit = true
      });
      if (!isSurface)
      {
        Point p = new Point(x, y) + direction;
        while (map.IsInBounds(p) && !map.GetTileAt(p.X, p.Y).Model.IsWalkable)
        {
          map.SetTileModelAt(p.X, p.Y, this.m_Game.GameTiles.FLOOR_CONCRETE);
          p += direction;
        }
      }
      int num = this.m_DiceRoller.Roll(Math.Max(b.InsideRect.Width, b.InsideRect.Height), 2 * Math.Max(b.InsideRect.Width, b.InsideRect.Height));
      for (int index = 0; index < num; ++index)
        this.MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          if (this.CountAdjWalls(map, pt.X, pt.Y) >= 3)
            return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
          return false;
        }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          map.DropItemAt(this.MakeShopConstructionItem(), pt);
          return this.MakeObjTable("MapObjects\\table");
        }));
      if (this.m_DiceRoller.RollChance(33))
      {
        this.MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          if (this.CountAdjWalls(map, pt.X, pt.Y) >= 3)
            return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
          return false;
        }), this.m_DiceRoller, (Func<Point, MapObject>) (pt => this.MakeObjBed("MapObjects\\bed")));
        this.MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          if (this.CountAdjWalls(map, pt.X, pt.Y) >= 3)
            return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
          return false;
        }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          map.DropItemAt(this.MakeItemCannedFood(), pt);
          return this.MakeObjFridge("MapObjects\\fridge");
        }));
      }
      Actor newCivilian = this.CreateNewCivilian(0, 3, 1);
      this.ActorPlace(this.m_DiceRoller, b.Rectangle.Width * b.Rectangle.Height, map, newCivilian, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
      map.AddZone(this.MakeUniqueZone("Sewers Maintenance", b.BuildingRect));
    }

    protected virtual void MakeSubwayStationBuilding(Map map, bool isSurface, BaseTownGenerator.Block b, Map linkedMap, Point exitPosition)
    {
      if (!isSurface)
        this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, b.InsideRect);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_SUBWAY, b.BuildingRect);
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left)
      {
        for (int top = b.InsideRect.Top; top < b.InsideRect.Bottom; ++top)
          map.GetTileAt(left, top).IsInside = true;
      }
      Direction direction;
      int x1;
      int num;
      switch (!isSurface ? (b.Rectangle.Bottom < map.Width / 2 ? 1 : 0) : this.m_DiceRoller.Roll(0, 4))
      {
        case 0:
          direction = Direction.N;
          x1 = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          num = b.BuildingRect.Top;
          if (isSurface)
          {
            map.GetTileAt(x1 - 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1 + 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        case 1:
          direction = Direction.S;
          x1 = b.BuildingRect.Left + b.BuildingRect.Width / 2;
          num = b.BuildingRect.Bottom - 1;
          if (isSurface)
          {
            map.GetTileAt(x1 - 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1 + 1, num).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        case 2:
          direction = Direction.W;
          x1 = b.BuildingRect.Left;
          num = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          if (isSurface)
          {
            map.GetTileAt(x1, num - 1).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1, num + 1).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        case 3:
          direction = Direction.E;
          x1 = b.BuildingRect.Right - 1;
          num = b.BuildingRect.Top + b.BuildingRect.Height / 2;
          if (isSurface)
          {
            map.GetTileAt(x1, num - 1).AddDecoration("Tiles\\Decoration\\subway_building");
            map.GetTileAt(x1, num + 1).AddDecoration("Tiles\\Decoration\\subway_building");
            break;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
      if (isSurface)
      {
        map.SetTileModelAt(x1, num, this.m_Game.GameTiles.FLOOR_CONCRETE);
        map.PlaceMapObjectAt((MapObject) this.MakeObjGlassDoor(), new Point(x1, num));
      }
      for (int x2 = exitPosition.X - 1; x2 <= exitPosition.X + 1; ++x2)
      {
        Point point = new Point(x2, exitPosition.Y);
        map.GetTileAt(point.X, point.Y).AddDecoration(isSurface ? "Tiles\\Decoration\\stairs_down" : "Tiles\\Decoration\\stairs_up");
        map.SetExitAt(point, new Exit(linkedMap, point)
        {
          IsAnAIExit = true
        });
      }
      if (!isSurface)
      {
        map.SetTileModelAt(x1, num, this.m_Game.GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(x1 + 1, num, this.m_Game.GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(x1 - 1, num, this.m_Game.GameTiles.FLOOR_CONCRETE);
        map.SetTileModelAt(x1 - 2, num, this.m_Game.GameTiles.WALL_STONE);
        map.SetTileModelAt(x1 + 2, num, this.m_Game.GameTiles.WALL_STONE);
        Point p = new Point(x1, num) + direction;
        while (map.IsInBounds(p) && !map.GetTileAt(p.X, p.Y).Model.IsWalkable)
        {
          map.SetTileModelAt(p.X, p.Y, this.m_Game.GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X - 1, p.Y, this.m_Game.GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X + 1, p.Y, this.m_Game.GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p.X - 2, p.Y, this.m_Game.GameTiles.WALL_STONE);
          map.SetTileModelAt(p.X + 2, p.Y, this.m_Game.GameTiles.WALL_STONE);
          p += direction;
        }
        int left1 = Math.Max(0, b.BuildingRect.Left - 10);
        int right = Math.Min(map.Width - 1, b.BuildingRect.Right + 10);
        Rectangle rect1;
        int y;
        if (direction == Direction.S)
        {
          rect1 = Rectangle.FromLTRB(left1, p.Y - 3, right, p.Y);
          y = rect1.Top;
          map.AddZone(this.MakeUniqueZone("corridor", Rectangle.FromLTRB(x1 - 1, num, x1 + 1 + 1, rect1.Top)));
        }
        else
        {
          rect1 = Rectangle.FromLTRB(left1, p.Y + 1, right, p.Y + 1 + 3);
          y = rect1.Bottom - 1;
          map.AddZone(this.MakeUniqueZone("corridor", Rectangle.FromLTRB(x1 - 1, rect1.Bottom, x1 + 1 + 1, num + 1)));
        }
        this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, rect1);
        for (int left2 = rect1.Left; left2 < rect1.Right; ++left2)
        {
          if (this.CountAdjWalls(map, left2, y) >= 3)
            map.PlaceMapObjectAt(this.MakeObjIronBench("MapObjects\\iron_bench"), new Point(left2, y));
        }
        map.AddZone(this.MakeUniqueZone("platform", rect1));
        Point point1 = direction != Direction.S ? new Point(x1, rect1.Bottom) : new Point(x1, rect1.Top - 1);
        map.PlaceMapObjectAt(this.MakeObjIronGate("MapObjects\\gate_closed"), new Point(point1.X, point1.Y));
        map.PlaceMapObjectAt(this.MakeObjIronGate("MapObjects\\gate_closed"), new Point(point1.X + 1, point1.Y));
        map.PlaceMapObjectAt(this.MakeObjIronGate("MapObjects\\gate_closed"), new Point(point1.X - 1, point1.Y));
        Point point2;
        Rectangle rect2;
        if (x1 > map.Width / 2)
        {
          point2 = new Point(x1 - 2, num + 2 * direction.Vector.Y);
          rect2 = Rectangle.FromLTRB(point2.X - 4, point2.Y - 2, point2.X + 1, point2.Y + 2 + 1);
        }
        else
        {
          point2 = new Point(x1 + 2, num + 2 * direction.Vector.Y);
          rect2 = Rectangle.FromLTRB(point2.X, point2.Y - 2, point2.X + 4, point2.Y + 2 + 1);
        }
        this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, rect2);
        this.TileRectangle(map, this.m_Game.GameTiles.WALL_STONE, rect2);
        this.PlaceDoor(map, point2.X, point2.Y, this.m_Game.GameTiles.FLOOR_CONCRETE, this.MakeObjIronDoor());
        map.GetTileAt(point2.X, point2.Y - 1).AddDecoration("Tiles\\Decoration\\power_sign_big");
        map.GetTileAt(point2.X, point2.Y + 1).AddDecoration("Tiles\\Decoration\\power_sign_big");
        this.MapObjectFill(map, rect2, (Func<Point, MapObject>) (pt =>
        {
          if (!map.GetTileAt(pt).Model.IsWalkable)
            return (MapObject) null;
          if (this.CountAdjWalls(map, pt.X, pt.Y) < 3 || this.CountAdjDoors(map, pt.X, pt.Y) > 0)
            return (MapObject) null;
          return (MapObject) this.MakeObjPowerGenerator("MapObjects\\power_generator_off", "MapObjects\\power_generator_on");
        }));
      }
      for (int left = b.InsideRect.Left; left < b.InsideRect.Right; ++left)
      {
        for (int y = b.InsideRect.Top + 1; y < b.InsideRect.Bottom - 1; ++y)
        {
          if (this.CountAdjWalls(map, left, y) >= 2 && this.CountAdjDoors(map, left, y) <= 0 && this.m_Game.Rules.GridDistance(new Point(left, y), new Point(x1, num)) >= 2)
            map.PlaceMapObjectAt(this.MakeObjIronBench("MapObjects\\iron_bench"), new Point(left, y));
        }
      }
      if (isSurface)
      {
        Actor newPoliceman = this.CreateNewPoliceman(0);
        this.ActorPlace(this.m_DiceRoller, b.Rectangle.Width * b.Rectangle.Height, map, newPoliceman, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
      }
      map.AddZone(this.MakeUniqueZone("Subway Station", b.BuildingRect));
    }

    protected virtual void MakeRoomsPlan(Map map, ref List<Rectangle> list, Rectangle rect, int minRoomsSize)
    {
      int splitX;
      int splitY;
      Rectangle topLeft;
      Rectangle topRight;
      Rectangle bottomLeft;
      Rectangle bottomRight;
      this.QuadSplit(rect, minRoomsSize, minRoomsSize, out splitX, out splitY, out topLeft, out topRight, out bottomLeft, out bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty)
      {
        list.Add(rect);
      }
      else
      {
        this.MakeRoomsPlan(map, ref list, topLeft, minRoomsSize);
        if (!topRight.IsEmpty)
        {
          topRight.Offset(-1, 0);
          ++topRight.Width;
          this.MakeRoomsPlan(map, ref list, topRight, minRoomsSize);
        }
        if (!bottomLeft.IsEmpty)
        {
          bottomLeft.Offset(0, -1);
          ++bottomLeft.Height;
          this.MakeRoomsPlan(map, ref list, bottomLeft, minRoomsSize);
        }
        if (bottomRight.IsEmpty)
          return;
        bottomRight.Offset(-1, -1);
        ++bottomRight.Width;
        ++bottomRight.Height;
        this.MakeRoomsPlan(map, ref list, bottomRight, minRoomsSize);
      }
    }

    protected virtual void MakeHousingRoom(Map map, Rectangle roomRect, TileModel floor, TileModel wall)
    {
      this.TileFill(map, floor, roomRect);
      this.TileRectangle(map, wall, roomRect.Left, roomRect.Top, roomRect.Width, roomRect.Height, (Action<Tile, TileModel, int, int>) ((tile, prevmodel, x, y) =>
      {
        if (map.GetMapObjectAt(x, y) == null)
          return;
        map.SetTileModelAt(x, y, floor);
      }));
      int x1 = roomRect.Left + roomRect.Width / 2;
      int y1 = roomRect.Top + roomRect.Height / 2;
      this.PlaceIf(map, x1, roomRect.Top, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (this.HasNoObjectAt(map, x, y) && this.IsAccessible(map, x, y))
          return this.CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!this.IsInside(map, x, y) && !this.m_DiceRoller.RollChance(25))
          return (MapObject) this.MakeObjWindow();
        return (MapObject) this.MakeObjWoodenDoor();
      }));
      this.PlaceIf(map, x1, roomRect.Bottom - 1, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (this.HasNoObjectAt(map, x, y) && this.IsAccessible(map, x, y))
          return this.CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!this.IsInside(map, x, y) && !this.m_DiceRoller.RollChance(25))
          return (MapObject) this.MakeObjWindow();
        return (MapObject) this.MakeObjWoodenDoor();
      }));
      this.PlaceIf(map, roomRect.Left, y1, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (this.HasNoObjectAt(map, x, y) && this.IsAccessible(map, x, y))
          return this.CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!this.IsInside(map, x, y) && !this.m_DiceRoller.RollChance(25))
          return (MapObject) this.MakeObjWindow();
        return (MapObject) this.MakeObjWoodenDoor();
      }));
      this.PlaceIf(map, roomRect.Right - 1, y1, floor, (Func<int, int, bool>) ((x, y) =>
      {
        if (this.HasNoObjectAt(map, x, y) && this.IsAccessible(map, x, y))
          return this.CountAdjDoors(map, x, y) == 0;
        return false;
      }), (Func<int, int, MapObject>) ((x, y) =>
      {
        if (!this.IsInside(map, x, y) && !this.m_DiceRoller.RollChance(25))
          return (MapObject) this.MakeObjWindow();
        return (MapObject) this.MakeObjWoodenDoor();
      }));
    }

    protected virtual void FillHousingRoomContents(Map map, Rectangle roomRect)
    {
      Rectangle insideRoom = new Rectangle(roomRect.Left + 1, roomRect.Top + 1, roomRect.Width - 2, roomRect.Height - 2);
      switch (this.m_DiceRoller.Roll(0, 10))
      {
        case 0:
        case 1:
        case 2:
        case 3:
        case 4:
          int num1 = this.m_DiceRoller.Roll(1, 3);
          for (int index = 0; index < num1; ++index)
            this.MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              if (this.CountAdjWalls(map, pt.X, pt.Y) >= 3)
                return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
              return false;
            }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              Rectangle rect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
              rect.Intersect(insideRoom);
              this.MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                if (pt2 != pt && this.CountAdjDoors(map, pt2.X, pt2.Y) == 0)
                  return this.CountAdjWalls(map, pt2.X, pt2.Y) > 0;
                return false;
              }), this.m_DiceRoller, (Func<Point, MapObject>) (pt2 =>
              {
                Item it = this.MakeRandomBedroomItem();
                if (it != null)
                  map.DropItemAt(it, pt2);
                return this.MakeObjNightTable("MapObjects\\nighttable");
              }));
              return this.MakeObjBed("MapObjects\\bed");
            }));
          int num2 = this.m_DiceRoller.Roll(1, 4);
          for (int index = 0; index < num2; ++index)
            this.MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              if (this.CountAdjWalls(map, pt.X, pt.Y) >= 2)
                return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
              return false;
            }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              Item it = this.MakeRandomBedroomItem();
              if (it != null)
                map.DropItemAt(it, pt);
              if (this.m_DiceRoller.RollChance(50))
                return this.MakeObjWardrobe("MapObjects\\wardrobe");
              return this.MakeObjDrawer("MapObjects\\drawer");
            }));
          break;
        case 5:
        case 6:
        case 7:
          int num3 = this.m_DiceRoller.Roll(1, 3);
          for (int index1 = 0; index1 < num3; ++index1)
            this.MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              if (this.CountAdjWalls(map, pt.X, pt.Y) == 0)
                return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
              return false;
            }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              for (int index = 0; index < HOUSE_LIVINGROOM_ITEMS_ON_TABLE; ++index)
              {
                Item it = this.MakeRandomKitchenItem();
                if (it != null)
                  map.DropItemAt(it, pt);
              }
              Rectangle rect = new Rectangle(pt.X - 1, pt.Y - 1, 3, 3);
              rect.Intersect(insideRoom);
              this.MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                if (pt2 != pt)
                  return this.CountAdjDoors(map, pt2.X, pt2.Y) == 0;
                return false;
              }), this.m_DiceRoller, (Func<Point, MapObject>) (pt2 => this.MakeObjChair("MapObjects\\chair")));
              return this.MakeObjTable("MapObjects\\table");
            }));
          int num4 = this.m_DiceRoller.Roll(1, 3);
          for (int index = 0; index < num4; ++index)
            this.MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              if (this.CountAdjWalls(map, pt.X, pt.Y) >= 2)
                return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
              return false;
            }), this.m_DiceRoller, (Func<Point, MapObject>) (pt => this.MakeObjDrawer("MapObjects\\drawer")));
          break;
        case 8:
        case 9:
          this.MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            if (this.CountAdjWalls(map, pt.X, pt.Y) == 0)
              return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
            return false;
          }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_ON_TABLE; ++index)
            {
              Item it = this.MakeRandomKitchenItem();
              if (it != null)
                map.DropItemAt(it, pt);
            }
            this.MapObjectPlaceInGoodPosition(map, new Rectangle(pt.X - 1, pt.Y - 1, 3, 3), (Func<Point, bool>) (pt2 =>
            {
              if (pt2 != pt)
                return this.CountAdjDoors(map, pt2.X, pt2.Y) == 0;
              return false;
            }), this.m_DiceRoller, (Func<Point, MapObject>) (pt2 => this.MakeObjChair("MapObjects\\chair")));
            return this.MakeObjTable("MapObjects\\table");
          }));
          this.MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            if (this.CountAdjWalls(map, pt.X, pt.Y) >= 2)
              return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
            return false;
          }), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_IN_FRIDGE; ++index)
            {
              Item it = this.MakeRandomKitchenItem();
              if (it != null)
                map.DropItemAt(it, pt);
            }
            return this.MakeObjFridge("MapObjects\\fridge");
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
          return this.MakeShopGeneralItem();
        case BaseTownGenerator.ShopType.GROCERY:
          return this.MakeShopGroceryItem();
        case BaseTownGenerator.ShopType.SPORTSWEAR:
          return this.MakeShopSportsWearItem();
        case BaseTownGenerator.ShopType.PHARMACY:
          return this.MakeShopPharmacyItem();
        case BaseTownGenerator.ShopType.CONSTRUCTION:
          return this.MakeShopConstructionItem();
        case BaseTownGenerator.ShopType.GUNSHOP:
          return this.MakeShopGunshopItem();
        case BaseTownGenerator.ShopType.HUNTING:
          return this.MakeHuntingShopItem();
        default:
          throw new ArgumentOutOfRangeException("unhandled shoptype");
      }
    }

    public Item MakeShopGroceryItem()
    {
      if (this.m_DiceRoller.RollChance(50))
        return this.MakeItemCannedFood();
      return this.MakeItemGroceries();
    }

    public Item MakeShopPharmacyItem()
    {
      switch (this.m_DiceRoller.Roll(0, 6))
      {
        case 0:
          return this.MakeItemBandages();
        case 1:
          return this.MakeItemMedikit();
        case 2:
          return this.MakeItemPillsSLP();
        case 3:
          return this.MakeItemPillsSTA();
        case 4:
          return this.MakeItemPillsSAN();
        case 5:
          return this.MakeItemStenchKiller();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeShopSportsWearItem()
    {
      switch (this.m_DiceRoller.Roll(0, 10))
      {
        case 0:
          if (this.m_DiceRoller.RollChance(30))
            return this.MakeItemHuntingRifle();
          return this.MakeItemLightRifleAmmo();
        case 1:
          if (this.m_DiceRoller.RollChance(30))
            return this.MakeItemHuntingCrossbow();
          return this.MakeItemBoltsAmmo();
        case 2:
        case 3:
        case 4:
        case 5:
          return this.MakeItemBaseballBat();
        case 6:
        case 7:
          return this.MakeItemIronGolfClub();
        case 8:
        case 9:
          return this.MakeItemGolfClub();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeShopConstructionItem()
    {
      switch (this.m_DiceRoller.Roll(0, 24))
      {
        case 0:
        case 1:
        case 2:
          if (!this.m_DiceRoller.RollChance(50))
            return this.MakeItemShortShovel();
          return this.MakeItemShovel();
        case 3:
        case 4:
        case 5:
          return this.MakeItemCrowbar();
        case 6:
        case 7:
        case 8:
          if (!this.m_DiceRoller.RollChance(50))
            return this.MakeItemSmallHammer();
          return this.MakeItemHugeHammer();
        case 9:
        case 10:
        case 11:
          return (Item) this.MakeItemWoodenPlank();
        case 12:
        case 13:
        case 14:
          return this.MakeItemFlashlight();
        case 15:
        case 16:
        case 17:
          return this.MakeItemBigFlashlight();
        case 18:
        case 19:
        case 20:
          return this.MakeItemSpikes();
        case 21:
        case 22:
        case 23:
          return this.MakeItemBarbedWire();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeShopGunshopItem()
    {
      if (this.m_DiceRoller.RollChance(40))
      {
        switch (this.m_DiceRoller.Roll(0, 4))
        {
          case 0:
            return this.MakeItemRandomPistol();
          case 1:
            return this.MakeItemShotgun();
          case 2:
            return this.MakeItemHuntingRifle();
          case 3:
            return this.MakeItemHuntingCrossbow();
          default:
            return (Item) null;
        }
      }
      else
      {
        switch (this.m_DiceRoller.Roll(0, 4))
        {
          case 0:
            return this.MakeItemLightPistolAmmo();
          case 1:
            return this.MakeItemShotgunAmmo();
          case 2:
            return this.MakeItemLightRifleAmmo();
          case 3:
            return this.MakeItemBoltsAmmo();
          default:
            return (Item) null;
        }
      }
    }

    public Item MakeHuntingShopItem()
    {
      if (this.m_DiceRoller.RollChance(50))
      {
        if (this.m_DiceRoller.RollChance(40))
        {
          switch (this.m_DiceRoller.Roll(0, 2))
          {
            case 0:
              return this.MakeItemHuntingRifle();
            case 1:
              return this.MakeItemHuntingCrossbow();
            default:
              return (Item) null;
          }
        }
        else
        {
          switch (this.m_DiceRoller.Roll(0, 2))
          {
            case 0:
              return this.MakeItemLightRifleAmmo();
            case 1:
              return this.MakeItemBoltsAmmo();
            default:
              return (Item) null;
          }
        }
      }
      else
      {
        switch (this.m_DiceRoller.Roll(0, 2))
        {
          case 0:
            return this.MakeItemHunterVest();
          case 1:
            return this.MakeItemBearTrap();
          default:
            return (Item) null;
        }
      }
    }

    public Item MakeShopGeneralItem()
    {
      switch (this.m_DiceRoller.Roll(0, 6))
      {
        case 0:
          return this.MakeShopPharmacyItem();
        case 1:
          return this.MakeShopSportsWearItem();
        case 2:
          return this.MakeShopConstructionItem();
        case 3:
          return this.MakeShopGroceryItem();
        case 4:
          return this.MakeHuntingShopItem();
        case 5:
          return this.MakeRandomBedroomItem();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeHospitalItem()
    {
      switch (this.m_DiceRoller.Roll(0, 7))
      {
        case 0:
          return this.MakeItemBandages();
        case 1:
          return this.MakeItemMedikit();
        case 2:
          return this.MakeItemPillsSLP();
        case 3:
          return this.MakeItemPillsSTA();
        case 4:
          return this.MakeItemPillsSAN();
        case 5:
          return this.MakeItemStenchKiller();
        case 6:
          return this.MakeItemPillsAntiviral();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeRandomBedroomItem()
    {
      switch (this.m_DiceRoller.Roll(0, 24))
      {
        case 0:
        case 1:
          return this.MakeItemBandages();
        case 2:
          return this.MakeItemPillsSTA();
        case 3:
          return this.MakeItemPillsSLP();
        case 4:
          return this.MakeItemPillsSAN();
        case 5:
        case 6:
        case 7:
        case 8:
          return this.MakeItemBaseballBat();
        case 9:
          return this.MakeItemRandomPistol();
        case 10:
          if (this.m_DiceRoller.RollChance(30))
          {
            if (this.m_DiceRoller.RollChance(50))
              return this.MakeItemShotgun();
            return this.MakeItemHuntingRifle();
          }
          if (this.m_DiceRoller.RollChance(50))
            return this.MakeItemShotgunAmmo();
          return this.MakeItemLightRifleAmmo();
        case 11:
        case 12:
        case 13:
          return this.MakeItemCellPhone();
        case 14:
        case 15:
          return this.MakeItemFlashlight();
        case 16:
        case 17:
          return this.MakeItemLightPistolAmmo();
        case 18:
        case 19:
          return this.MakeItemStenchKiller();
        case 20:
          return this.MakeItemHunterVest();
        case 21:
        case 22:
        case 23:
          if (this.m_DiceRoller.RollChance(50))
            return this.MakeItemBook();
          return this.MakeItemMagazines();
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
    }

    public Item MakeRandomKitchenItem()
    {
      if (this.m_DiceRoller.RollChance(50))
        return this.MakeItemCannedFood();
      return this.MakeItemGroceries();
    }

    public Item MakeRandomCHAROfficeItem()
    {
      switch (this.m_DiceRoller.Roll(0, 10))
      {
        case 0:
          if (this.m_DiceRoller.RollChance(10))
            return this.MakeItemGrenade();
          if (this.m_DiceRoller.RollChance(30))
            return this.MakeItemShotgun();
          return this.MakeItemShotgunAmmo();
        case 1:
        case 2:
          if (this.m_DiceRoller.RollChance(50))
            return this.MakeItemBandages();
          return this.MakeItemMedikit();
        case 3:
          return this.MakeItemCannedFood();
        case 4:
          if (!this.m_DiceRoller.RollChance(50))
            return (Item) null;
          if (this.m_DiceRoller.RollChance(50))
            return this.MakeItemZTracker();
          return this.MakeItemBlackOpsGPS();
        default:
          return (Item) null;
      }
    }

    public Item MakeRandomParkItem()
    {
      switch (this.m_DiceRoller.Roll(0, 8))
      {
        case 0:
          return this.MakeItemSprayPaint();
        case 1:
          return this.MakeItemBaseballBat();
        case 2:
          return this.MakeItemPillsSLP();
        case 3:
          return this.MakeItemPillsSTA();
        case 4:
          return this.MakeItemPillsSAN();
        case 5:
          return this.MakeItemFlashlight();
        case 6:
          return this.MakeItemCellPhone();
        case 7:
          return (Item) this.MakeItemWoodenPlank();
        default:
          throw new ArgumentOutOfRangeException("unhandled item roll");
      }
    }

    protected virtual void DecorateOutsideWallsWithPosters(Map map, Rectangle rect, int chancePerWall)
    {
      this.DecorateOutsideWalls(map, rect, (Func<int, int, string>) ((x, y) =>
      {
        if (this.m_DiceRoller.RollChance(chancePerWall))
          return BaseTownGenerator.POSTERS[this.m_DiceRoller.Roll(0, BaseTownGenerator.POSTERS.Length)];
        return (string) null;
      }));
    }

    protected virtual void DecorateOutsideWallsWithTags(Map map, Rectangle rect, int chancePerWall)
    {
      this.DecorateOutsideWalls(map, rect, (Func<int, int, string>) ((x, y) =>
      {
        if (this.m_DiceRoller.RollChance(chancePerWall))
          return BaseTownGenerator.TAGS[this.m_DiceRoller.Roll(0, BaseTownGenerator.TAGS.Length)];
        return (string) null;
      }));
    }

    protected virtual void PopulateCHAROfficeBuilding(Map map, BaseTownGenerator.Block b)
    {
      for (int index = 0; index < MAX_CHAR_GUARDS_PER_OFFICE; ++index)
      {
        Actor newCharGuard = this.CreateNewCHARGuard(0);
        this.ActorPlace(this.m_DiceRoller, 100, map, newCharGuard, b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height);
      }
    }

    private Map GenerateHouseBasementMap(Map map, BaseTownGenerator.Block houseBlock)
    {
      Rectangle buildingRect = houseBlock.BuildingRect;
      Map basement = new Map(map.Seed << 1 + buildingRect.Left * map.Height + buildingRect.Top, string.Format("basement{0}{1}@{2}-{3}", (object) this.m_Params.District.WorldPosition.X, (object) this.m_Params.District.WorldPosition.Y, (object) (buildingRect.Left + buildingRect.Width / 2), (object) (buildingRect.Top + buildingRect.Height / 2)), buildingRect.Width, buildingRect.Height)
      {
        Lighting = Lighting._FIRST
      };
      basement.AddZone(this.MakeUniqueZone("basement", basement.Rect));
      this.TileFill(basement, this.m_Game.GameTiles.FLOOR_CONCRETE, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.IsInside = true));
      this.TileRectangle(basement, this.m_Game.GameTiles.WALL_BRICK, new Rectangle(0, 0, basement.Width, basement.Height));
      Point point = new Point();
      do
      {
        point.X = this.m_DiceRoller.Roll(buildingRect.Left, buildingRect.Right);
        point.Y = this.m_DiceRoller.Roll(buildingRect.Top, buildingRect.Bottom);
      }
      while (!map.GetTileAt(point.X, point.Y).Model.IsWalkable || map.GetMapObjectAt(point.X, point.Y) != null);
      Point basementStairs = new Point(point.X - buildingRect.Left, point.Y - buildingRect.Top);
      this.AddExit(map, point, basement, basementStairs, "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(basement, basementStairs, map, point, "Tiles\\Decoration\\stairs_up", true);
      this.DoForEachTile(basement, basement.Rect, (Action<Point>) (pt =>
      {
        if (!this.m_DiceRoller.RollChance(20) || pt == basementStairs)
          return;
        basement.SetTileModelAt(pt.X, pt.Y, this.m_Game.GameTiles.WALL_BRICK);
      }));
      this.MapObjectFill(basement, basement.Rect, (Func<Point, MapObject>) (pt =>
      {
        if (!this.m_DiceRoller.RollChance(10))
          return (MapObject) null;
        if (basement.GetExitAt(pt) != null)
          return (MapObject) null;
        if (!basement.IsWalkable(pt.X, pt.Y))
          return (MapObject) null;
        switch (this.m_DiceRoller.Roll(0, 5))
        {
          case 0:
            return this.MakeObjJunk("MapObjects\\junk");
          case 1:
            return this.MakeObjBarrels("MapObjects\\barrels");
          case 2:
            basement.DropItemAt(this.MakeShopConstructionItem(), pt);
            return this.MakeObjTable("MapObjects\\table");
          case 3:
            basement.DropItemAt(this.MakeShopConstructionItem(), pt);
            return this.MakeObjDrawer("MapObjects\\drawer");
          case 4:
            return this.MakeObjBed("MapObjects\\bed");
          default:
            throw new ArgumentOutOfRangeException("unhandled roll");
        }
      }));
      if (Rules.HasZombiesInBasements(this.m_Game.Session.GameMode))
        this.DoForEachTile(basement, basement.Rect, (Action<Point>) (pt =>
        {
          if (!basement.IsWalkable(pt.X, pt.Y) || basement.GetExitAt(pt) != null || !this.m_DiceRoller.RollChance(5))
            return;
          basement.PlaceActorAt(this.CreateNewBasementRatZombie(0), pt);
        }));
      if (this.m_DiceRoller.RollChance(20))
        this.MapObjectPlaceInGoodPosition(basement, basement.Rect, (Func<Point, bool>) (pt => basement.GetExitAt(pt) == null && basement.IsWalkable(pt.X, pt.Y) && (basement.GetMapObjectAt(pt) == null && basement.GetItemsAt(pt) == null)), this.m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          basement.DropItemAt(this.MakeItemGrenade(), pt);
          basement.DropItemAt(this.MakeItemGrenade(), pt);
          for (int index = 0; index < 5; ++index)
            basement.DropItemAt(this.MakeShopGunshopItem(), pt);
          return this.MakeObjShelf("MapObjects\\shop_shelf");
        }));
      return basement;
    }

    public Map GenerateUniqueMap_CHARUnderground(Map surfaceMap, Zone officeZone)
    {
      Map underground = new Map(surfaceMap.Seed << 3 ^ surfaceMap.Seed, "CHAR Underground Facility", 100, 100)
      {
        Lighting = Lighting._FIRST,
        IsSecret = true
      };
      this.TileFill(underground, this.m_Game.GameTiles.FLOOR_OFFICE, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.IsInside = true));
      this.TileRectangle(underground, this.m_Game.GameTiles.WALL_CHAR_OFFICE, new Rectangle(0, 0, underground.Width, underground.Height));
      Zone zone1 = (Zone) null;
      Point point1 = new Point();
      bool flag;
      do
      {
        do
        {
          int x = this.m_DiceRoller.Roll(officeZone.Bounds.Left, officeZone.Bounds.Right);
          int y = this.m_DiceRoller.Roll(officeZone.Bounds.Top, officeZone.Bounds.Bottom);
          List<Zone> zonesAt = surfaceMap.GetZonesAt(x, y);
          if (zonesAt != null && zonesAt.Count != 0)
          {
            foreach (Zone zone2 in zonesAt)
            {
              if (zone2.Name.Contains("room"))
              {
                zone1 = zone2;
                break;
              }
            }
          }
        }
        while (zone1 == null);
        int num = 0;
        do
        {
          point1.X = this.m_DiceRoller.Roll(zone1.Bounds.Left, zone1.Bounds.Right);
          point1.Y = this.m_DiceRoller.Roll(zone1.Bounds.Top, zone1.Bounds.Bottom);
          flag = surfaceMap.IsWalkable(point1.X, point1.Y);
          ++num;
        }
        while (num < 100 && !flag);
      }
      while (!flag);
      this.DoForEachTile(surfaceMap, zone1.Bounds, (Action<Point>) (pt =>
      {
        if (!(surfaceMap.GetMapObjectAt(pt) is DoorWindow))
          return;
        surfaceMap.RemoveMapObjectAt(pt.X, pt.Y);
        DoorWindow doorWindow = this.MakeObjIronDoor();
        doorWindow.BarricadePoints = 80;
        surfaceMap.PlaceMapObjectAt((MapObject) doorWindow, pt);
      }));
      Point point2 = new Point(underground.Width / 2, underground.Height / 2);
      underground.SetExitAt(point2, new Exit(surfaceMap, point1));
      underground.GetTileAt(point2.X, point2.Y).AddDecoration("Tiles\\Decoration\\stairs_up");
      surfaceMap.SetExitAt(point1, new Exit(underground, point2));
      surfaceMap.GetTileAt(point1.X, point1.Y).AddDecoration("Tiles\\Decoration\\stairs_down");
      this.ForEachAdjacent(underground, point2.X, point2.Y, (Action<Point>) (pt => underground.GetTileAt(pt).AddDecoration("Tiles\\Decoration\\char_floor_logo")));
      Rectangle rect1 = Rectangle.FromLTRB(0, 0, underground.Width / 2 - 1, underground.Height / 2 - 1);
      Rectangle rect2 = Rectangle.FromLTRB(underground.Width / 2 + 1 + 1, 0, underground.Width, rect1.Bottom);
      Rectangle rect3 = Rectangle.FromLTRB(0, underground.Height / 2 + 1 + 1, rect1.Right, underground.Height);
      Rectangle rect4 = Rectangle.FromLTRB(rect2.Left, rect3.Top, underground.Width, underground.Height);
      List<Rectangle> list = new List<Rectangle>();
      this.MakeRoomsPlan(underground, ref list, rect3, 6);
      this.MakeRoomsPlan(underground, ref list, rect4, 6);
      this.MakeRoomsPlan(underground, ref list, rect1, 6);
      this.MakeRoomsPlan(underground, ref list, rect2, 6);
      foreach (Rectangle rect5 in list)
        this.TileRectangle(underground, this.m_Game.GameTiles.WALL_CHAR_OFFICE, rect5);
      foreach (Rectangle rectangle in list)
      {
        Point position1 = rectangle.Left < underground.Width / 2 ? new Point(rectangle.Right - 1, rectangle.Top + rectangle.Height / 2) : new Point(rectangle.Left, rectangle.Top + rectangle.Height / 2);
        if (underground.GetMapObjectAt(position1) == null)
        {
          DoorWindow door = this.MakeObjCharDoor();
          this.PlaceDoorIfAccessibleAndNotAdjacent(underground, position1.X, position1.Y, this.m_Game.GameTiles.FLOOR_OFFICE, 6, door);
        }
        Point position2 = rectangle.Top < underground.Height / 2 ? new Point(rectangle.Left + rectangle.Width / 2, rectangle.Bottom - 1) : new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top);
        if (underground.GetMapObjectAt(position2) == null)
        {
          DoorWindow door = this.MakeObjCharDoor();
          this.PlaceDoorIfAccessibleAndNotAdjacent(underground, position2.X, position2.Y, this.m_Game.GameTiles.FLOOR_OFFICE, 6, door);
        }
      }
      for (int right = rect1.Right; right < rect4.Left; ++right)
      {
        this.PlaceDoor(underground, right, rect1.Bottom - 1, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjIronDoor());
        this.PlaceDoor(underground, right, rect3.Top, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjIronDoor());
      }
      for (int bottom = rect1.Bottom; bottom < rect3.Top; ++bottom)
      {
        this.PlaceDoor(underground, rect1.Right - 1, bottom, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjIronDoor());
        this.PlaceDoor(underground, rect2.Left, bottom, this.m_Game.GameTiles.FLOOR_OFFICE, this.MakeObjIronDoor());
      }
      foreach (Rectangle wallsRect in list)
      {
        Rectangle rectangle = new Rectangle(wallsRect.Left + 1, wallsRect.Top + 1, wallsRect.Width - 2, wallsRect.Height - 2);
        string basename;
        if (wallsRect.Left == 0 && wallsRect.Top == 0 || wallsRect.Left == 0 && wallsRect.Bottom == underground.Height || wallsRect.Right == underground.Width && wallsRect.Top == 0 || wallsRect.Right == underground.Width && wallsRect.Bottom == underground.Height)
        {
          basename = "Power Room";
          this.MakeCHARPowerRoom(underground, wallsRect, rectangle);
        }
        else
        {
          switch (wallsRect.Left >= underground.Width / 2 || wallsRect.Top >= underground.Height / 2 ? (wallsRect.Left < underground.Width / 2 || wallsRect.Top >= underground.Height / 2 ? (wallsRect.Left >= underground.Width / 2 || wallsRect.Top < underground.Height / 2 ? 3 : 2) : 1) : 0)
          {
            case 0:
              basename = "Armory";
              this.MakeCHARArmoryRoom(underground, rectangle);
              break;
            case 1:
              basename = "Storage";
              this.MakeCHARStorageRoom(underground, rectangle);
              break;
            case 2:
              basename = "Living";
              this.MakeCHARLivingRoom(underground, rectangle);
              break;
            case 3:
              basename = "Pharmacy";
              this.MakeCHARPharmacyRoom(underground, rectangle);
              break;
            default:
              throw new ArgumentOutOfRangeException("unhandled role");
          }
        }
        underground.AddZone(this.MakeUniqueZone(basename, rectangle));
      }
      for (int x = 0; x < underground.Width; ++x)
      {
        for (int y = 0; y < underground.Height; ++y)
        {
          if (this.m_DiceRoller.RollChance(25))
          {
            Tile tileAt = underground.GetTileAt(x, y);
            if (!tileAt.Model.IsWalkable)
              tileAt.AddDecoration(BaseTownGenerator.CHAR_POSTERS[this.m_DiceRoller.Roll(0, BaseTownGenerator.CHAR_POSTERS.Length)]);
            else
              continue;
          }
          if (this.m_DiceRoller.RollChance(20))
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
        Actor newUndead = this.CreateNewUndead(0);
        while (true)
        {
          GameActors.IDs index2 = this.m_Game.NextUndeadEvolution((GameActors.IDs) newUndead.Model.ID);
          if (index2 != (GameActors.IDs) newUndead.Model.ID)
            newUndead.Model = this.m_Game.GameActors[index2];
          else
            break;
        }
        this.ActorPlace(this.m_DiceRoller, underground.Width * underground.Height, underground, newUndead, (Predicate<Point>) (pt => underground.GetExitAt(pt) == null));
      }
      int num1 = underground.Width / 10;
      for (int index = 0; index < num1; ++index)
      {
        Actor newCharGuard = this.CreateNewCHARGuard(0);
        this.ActorPlace(this.m_DiceRoller, underground.Width * underground.Height, underground, newCharGuard, (Predicate<Point>) (pt => underground.GetExitAt(pt) == null));
      }
      return underground;
    }

    private void MakeCHARArmoryRoom(Map map, Rectangle roomRect)
    {
      this.MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (this.CountAdjWalls(map, pt.X, pt.Y) < 3)
          return (MapObject) null;
        if (map.GetExitAt(pt) != null)
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(20))
          return (MapObject) null;
        map.DropItemAt(!this.m_DiceRoller.RollChance(20) ? (!this.m_DiceRoller.RollChance(20) ? (!this.m_DiceRoller.RollChance(20) ? (!this.m_DiceRoller.RollChance(30) ? (this.m_DiceRoller.RollChance(50) ? this.MakeItemShotgunAmmo() : this.MakeItemLightRifleAmmo()) : (this.m_DiceRoller.RollChance(50) ? this.MakeItemShotgun() : this.MakeItemHuntingRifle())) : this.MakeItemGrenade()) : (this.m_DiceRoller.RollChance(50) ? this.MakeItemZTracker() : this.MakeItemBlackOpsGPS())) : this.MakeItemCHARLightBodyArmor(), pt);
        return this.MakeObjShelf("MapObjects\\shop_shelf");
      }));
    }

    private void MakeCHARStorageRoom(Map map, Rectangle roomRect)
    {
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, roomRect);
      this.MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (this.CountAdjWalls(map, pt.X, pt.Y) > 0)
          return (MapObject) null;
        if (map.GetExitAt(pt) != null)
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(50))
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(50))
          return this.MakeObjBarrels("MapObjects\\barrels");
        return this.MakeObjJunk("MapObjects\\junk");
      }));
      for (int left = roomRect.Left; left < roomRect.Right; ++left)
      {
        for (int top = roomRect.Top; top < roomRect.Bottom; ++top)
        {
          if (this.CountAdjWalls(map, left, top) <= 0 && map.GetMapObjectAt(left, top) == null)
            map.DropItemAt(this.MakeShopConstructionItem(), left, top);
        }
      }
    }

    private void MakeCHARLivingRoom(Map map, Rectangle roomRect)
    {
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_PLANKS, roomRect, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration("Tiles\\Decoration\\char_floor_logo")));
      this.MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (this.CountAdjWalls(map, pt.X, pt.Y) < 3)
          return (MapObject) null;
        if (map.GetExitAt(pt) != null)
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(30))
          return (MapObject) null;
        if (this.m_DiceRoller.RollChance(50))
          return this.MakeObjBed("MapObjects\\bed");
        return this.MakeObjFridge("MapObjects\\fridge");
      }));
      this.MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (this.CountAdjWalls(map, pt.X, pt.Y) > 0)
          return (MapObject) null;
        if (map.GetExitAt(pt) != null)
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(30))
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(30))
          return this.MakeObjChair("MapObjects\\char_chair");
        MapObject mapObject = this.MakeObjTable("MapObjects\\char_table");
        map.DropItemAt(this.MakeItemCannedFood(), pt);
        return mapObject;
      }));
    }

    private void MakeCHARPharmacyRoom(Map map, Rectangle roomRect)
    {
      this.MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (this.CountAdjWalls(map, pt.X, pt.Y) < 3)
          return (MapObject) null;
        if (map.GetExitAt(pt) != null)
          return (MapObject) null;
        if (!this.m_DiceRoller.RollChance(20))
          return (MapObject) null;
        map.DropItemAt(this.MakeHospitalItem(), pt);
        return this.MakeObjShelf("MapObjects\\shop_shelf");
      }));
    }

    private void MakeCHARPowerRoom(Map map, Rectangle wallsRect, Rectangle roomRect)
    {
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, roomRect);
      this.DoForEachTile(map, wallsRect, (Action<Point>) (pt =>
      {
        if (!(map.GetMapObjectAt(pt) is DoorWindow))
          return;
        this.DoForEachAdjacentInMap(map, pt, (Action<Point>) (ptAdj =>
        {
          Tile tileAt = map.GetTileAt(ptAdj);
          if (tileAt.Model.IsWalkable)
            return;
          tileAt.RemoveAllDecorations();
          tileAt.AddDecoration("Tiles\\Decoration\\power_sign_big");
        }));
      }));
      this.DoForEachTile(map, roomRect, (Action<Point>) (pt =>
      {
        if (!map.GetTileAt(pt).Model.IsWalkable || map.GetExitAt(pt) != null || this.CountAdjWalls(map, pt.X, pt.Y) < 3)
          return;
        map.PlaceMapObjectAt((MapObject) this.MakeObjPowerGenerator("MapObjects\\power_generator_off", "MapObjects\\power_generator_on"), pt);
      }));
    }

    private void MakePoliceStation(Map map, List<BaseTownGenerator.Block> freeBlocks, out BaseTownGenerator.Block policeBlock)
    {
      policeBlock = freeBlocks[this.m_DiceRoller.Roll(0, freeBlocks.Count)];
      Point stairsToLevel1;
      this.GeneratePoliceStation(map, policeBlock, out stairsToLevel1);
      Map stationOfficesLevel = this.GeneratePoliceStation_OfficesLevel(map, policeBlock, stairsToLevel1);
      Map stationJailsLevel = this.GeneratePoliceStation_JailsLevel(stationOfficesLevel);
      this.AddExit(map, stairsToLevel1, stationOfficesLevel, new Point(1, 1), "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(stationOfficesLevel, new Point(1, 1), map, stairsToLevel1, "Tiles\\Decoration\\stairs_up", true);
      this.AddExit(stationOfficesLevel, new Point(1, stationOfficesLevel.Height - 2), stationJailsLevel, new Point(1, 1), "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(stationJailsLevel, new Point(1, 1), stationOfficesLevel, new Point(1, stationOfficesLevel.Height - 2), "Tiles\\Decoration\\stairs_up", true);
      this.m_Params.District.AddUniqueMap(stationOfficesLevel);
      this.m_Params.District.AddUniqueMap(stationJailsLevel);
      this.m_Game.Session.UniqueMaps.PoliceStation_OfficesLevel = new UniqueMap()
      {
        TheMap = stationOfficesLevel
      };
      this.m_Game.Session.UniqueMaps.PoliceStation_JailsLevel = new UniqueMap()
      {
        TheMap = stationJailsLevel
      };
    }

    private void GeneratePoliceStation(Map surfaceMap, BaseTownGenerator.Block policeBlock, out Point stairsToLevel1)
    {
      this.TileFill(surfaceMap, this.m_Game.GameTiles.FLOOR_TILES, policeBlock.InsideRect);
      this.TileRectangle(surfaceMap, this.m_Game.GameTiles.WALL_POLICE_STATION, policeBlock.BuildingRect);
      this.TileRectangle(surfaceMap, this.m_Game.GameTiles.FLOOR_WALKWAY, policeBlock.Rectangle);
      this.DoForEachTile(surfaceMap, policeBlock.InsideRect, (Action<Point>) (pt => surfaceMap.GetTileAt(pt).IsInside = true));
      Point point = new Point(policeBlock.BuildingRect.Left + policeBlock.BuildingRect.Width / 2, policeBlock.BuildingRect.Bottom - 1);
      surfaceMap.GetTileAt(point.X - 1, point.Y).AddDecoration("Tiles\\Decoration\\police_station");
      surfaceMap.GetTileAt(point.X + 1, point.Y).AddDecoration("Tiles\\Decoration\\police_station");
      Rectangle rect = Rectangle.FromLTRB(policeBlock.BuildingRect.Left, policeBlock.BuildingRect.Top + 2, policeBlock.BuildingRect.Right, policeBlock.BuildingRect.Bottom);
      this.TileRectangle(surfaceMap, this.m_Game.GameTiles.WALL_POLICE_STATION, rect);
      this.PlaceDoor(surfaceMap, rect.Left + rect.Width / 2, rect.Top, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjIronDoor());
      this.PlaceDoor(surfaceMap, point.X, point.Y, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjGlassDoor());
      this.DoForEachTile(surfaceMap, rect, (Action<Point>) (pt =>
      {
        if (!surfaceMap.IsWalkable(pt.X, pt.Y) || this.CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || this.CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)
          return;
        surfaceMap.PlaceMapObjectAt(this.MakeObjBench("MapObjects\\bench"), pt);
      }));
      stairsToLevel1 = new Point(point.X, policeBlock.InsideRect.Top);
      surfaceMap.AddZone(this.MakeUniqueZone("Police Station", policeBlock.BuildingRect));
      this.MakeWalkwayZones(surfaceMap, policeBlock);
    }

    private Map GeneratePoliceStation_OfficesLevel(Map surfaceMap, BaseTownGenerator.Block policeBlock, Point exitPos)
    {
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Offices", 20, 20)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_POLICE_STATION, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
      List<Rectangle> list = new List<Rectangle>();
      this.MakeRoomsPlan(map, ref list, rect1, 5);
      foreach (Rectangle rect2 in list)
      {
        Rectangle rect3 = Rectangle.FromLTRB(rect2.Left + 1, rect2.Top + 1, rect2.Right - 1, rect2.Bottom - 1);
        if (rect2.Right == map.Width)
        {
          this.TileRectangle(map, this.m_Game.GameTiles.WALL_POLICE_STATION, rect2);
          this.PlaceDoor(map, rect2.Left, rect2.Top + rect2.Height / 2, this.m_Game.GameTiles.FLOOR_CONCRETE, this.MakeObjIronDoor());
          this.DoForEachTile(map, rect3, (Action<Point>) (pt =>
          {
            if (!map.IsWalkable(pt.X, pt.Y) || this.CountAdjWalls(map, pt.X, pt.Y) == 0 || this.CountAdjDoors(map, pt.X, pt.Y) > 0)
              return;
            map.PlaceMapObjectAt(this.MakeObjShelf("MapObjects\\shop_shelf"), pt);
            Item it;
            switch (this.m_DiceRoller.Roll(0, 10))
            {
              case 0:
              case 1:
                it = this.m_DiceRoller.RollChance(50) ? this.MakeItemPoliceJacket() : this.MakeItemPoliceRiotArmor();
                break;
              case 2:
              case 3:
                it = this.m_DiceRoller.RollChance(50) ? (this.m_DiceRoller.RollChance(50) ? this.MakeItemFlashlight() : this.MakeItemBigFlashlight()) : this.MakeItemPoliceRadio();
                break;
              case 4:
              case 5:
                it = this.MakeItemTruncheon();
                break;
              case 6:
              case 7:
                it = this.m_DiceRoller.RollChance(30) ? this.MakeItemPistol() : this.MakeItemLightPistolAmmo();
                break;
              case 8:
              case 9:
                it = this.m_DiceRoller.RollChance(30) ? this.MakeItemShotgun() : this.MakeItemShotgunAmmo();
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled roll");
            }
            map.DropItemAt(it, pt);
          }));
          map.AddZone(this.MakeUniqueZone("security", rect3));
        }
        else
        {
          this.TileFill(map, this.m_Game.GameTiles.FLOOR_PLANKS, rect2);
          this.TileRectangle(map, this.m_Game.GameTiles.WALL_POLICE_STATION, rect2);
          this.PlaceDoor(map, rect2.Left, rect2.Top + rect2.Height / 2, this.m_Game.GameTiles.FLOOR_PLANKS, this.MakeObjWoodenDoor());
          this.MapObjectPlaceInGoodPosition(map, rect3, (Func<Point, bool>) (pt =>
          {
            if (map.IsWalkable(pt.X, pt.Y))
              return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
            return false;
          }), this.m_DiceRoller, (Func<Point, MapObject>) (pt => this.MakeObjTable("MapObjects\\table")));
          this.MapObjectPlaceInGoodPosition(map, rect3, (Func<Point, bool>) (pt =>
          {
            if (map.IsWalkable(pt.X, pt.Y))
              return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
            return false;
          }), this.m_DiceRoller, (Func<Point, MapObject>) (pt => this.MakeObjChair("MapObjects\\chair")));
          this.MapObjectPlaceInGoodPosition(map, rect3, (Func<Point, bool>) (pt =>
          {
            if (map.IsWalkable(pt.X, pt.Y))
              return this.CountAdjDoors(map, pt.X, pt.Y) == 0;
            return false;
          }), this.m_DiceRoller, (Func<Point, MapObject>) (pt => this.MakeObjChair("MapObjects\\chair")));
          map.AddZone(this.MakeUniqueZone("office", rect3));
        }
      }
      this.DoForEachTile(map, new Rectangle(1, 1, 1, map.Height - 2), (Action<Point>) (pt =>
      {
        if (pt.Y % 2 == 1 || !map.IsWalkable(pt) || this.CountAdjWalls(map, pt) != 3)
          return;
        map.PlaceMapObjectAt(this.MakeObjIronBench("MapObjects\\iron_bench"), pt);
      }));
      for (int index = 0; index < 5; ++index)
      {
        Actor newPoliceman = this.CreateNewPoliceman(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newPoliceman);
      }
      return map;
    }

    private Map GeneratePoliceStation_JailsLevel(Map surfaceMap)
    {
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Jails", 22, 6)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_POLICE_STATION, map.Rect);
      List<Rectangle> rectangleList = new List<Rectangle>();
      int x = 0;
      while (x + 3 <= map.Width)
      {
        Rectangle rect = new Rectangle(x, 3, 3, 3);
        rectangleList.Add(rect);
        this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE, rect);
        this.TileRectangle(map, this.m_Game.GameTiles.WALL_POLICE_STATION, rect);
        Point position1 = new Point(x + 1, 4);
        map.PlaceMapObjectAt(this.MakeObjIronBench("MapObjects\\iron_bench"), position1);
        Point position2 = new Point(x + 1, 3);
        map.SetTileModelAt(position2.X, position2.Y, this.m_Game.GameTiles.FLOOR_CONCRETE);
        map.PlaceMapObjectAt(this.MakeObjIronGate("MapObjects\\gate_closed"), position2);
        map.AddZone(this.MakeUniqueZone("jail", rect));
        x += 2;
      }
      Rectangle rect1 = Rectangle.FromLTRB(1, 1, map.Width, 3);
      map.AddZone(this.MakeUniqueZone("cells corridor", rect1));
      map.PlaceMapObjectAt((MapObject) this.MakeObjPowerGenerator("MapObjects\\power_generator_off", "MapObjects\\power_generator_on"), new Point(map.Width - 2, 1));
      for (int index = 0; index < rectangleList.Count - 1; ++index)
      {
        Rectangle rectangle = rectangleList[index];
        Actor newCivilian = this.CreateNewCivilian(0, 0, 1);
        while (!newCivilian.Inventory.IsEmpty)
          newCivilian.Inventory.RemoveAllQuantity(newCivilian.Inventory[0]);
        newCivilian.Inventory.AddAll(this.MakeItemGroceries());
        map.PlaceActorAt(newCivilian, new Point(rectangle.Left + 1, rectangle.Top + 1));
      }
      Rectangle rectangle1 = rectangleList[rectangleList.Count - 1];
      Actor newCivilian1 = this.CreateNewCivilian(0, 0, 1);
      newCivilian1.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian1.Inventory.MaxCapacity; ++index)
        newCivilian1.Inventory.AddAll(this.MakeItemArmyRation());
      map.PlaceActorAt(newCivilian1, new Point(rectangle1.Left + 1, rectangle1.Top + 1));
      this.m_Game.Session.UniqueActors.PoliceStationPrisonner = new UniqueActor()
      {
        TheActor = newCivilian1,
        IsSpawned = true
      };
      return map;
    }

    private void MakeHospital(Map map, List<BaseTownGenerator.Block> freeBlocks, out BaseTownGenerator.Block hospitalBlock)
    {
      hospitalBlock = freeBlocks[this.m_DiceRoller.Roll(0, freeBlocks.Count)];
      this.GenerateHospitalEntryHall(map, hospitalBlock);
      Map hospitalAdmissions = this.GenerateHospital_Admissions(map.Seed << 1 ^ map.Seed);
      Map hospitalOffices = this.GenerateHospital_Offices(map.Seed << 2 ^ map.Seed);
      Map hospitalPatients = this.GenerateHospital_Patients(map.Seed << 3 ^ map.Seed);
      Map hospitalStorage = this.GenerateHospital_Storage(map.Seed << 4 ^ map.Seed);
      Map hospitalPower = this.GenerateHospital_Power(map.Seed << 5 ^ map.Seed);
      Point point1 = new Point(hospitalBlock.InsideRect.Left + hospitalBlock.InsideRect.Width / 2, hospitalBlock.InsideRect.Top);
      Point point2 = new Point(hospitalAdmissions.Width / 2, 1);
      this.AddExit(map, point1, hospitalAdmissions, point2, "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(hospitalAdmissions, point2, map, point1, "Tiles\\Decoration\\stairs_up", true);
      Point point3 = new Point(hospitalAdmissions.Width / 2, hospitalAdmissions.Height - 2);
      Point point4 = new Point(hospitalOffices.Width / 2, 1);
      this.AddExit(hospitalAdmissions, point3, hospitalOffices, point4, "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(hospitalOffices, point4, hospitalAdmissions, point3, "Tiles\\Decoration\\stairs_up", true);
      Point point5 = new Point(hospitalOffices.Width / 2, hospitalOffices.Height - 2);
      Point point6 = new Point(hospitalPatients.Width / 2, 1);
      this.AddExit(hospitalOffices, point5, hospitalPatients, point6, "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(hospitalPatients, point6, hospitalOffices, point5, "Tiles\\Decoration\\stairs_up", true);
      Point point7 = new Point(hospitalPatients.Width / 2, hospitalPatients.Height - 2);
      Point point8 = new Point(1, 1);
      this.AddExit(hospitalPatients, point7, hospitalStorage, point8, "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(hospitalStorage, point8, hospitalPatients, point7, "Tiles\\Decoration\\stairs_up", true);
      Point point9 = new Point(hospitalStorage.Width - 2, 1);
      Point point10 = new Point(1, 1);
      this.AddExit(hospitalStorage, point9, hospitalPower, point10, "Tiles\\Decoration\\stairs_down", true);
      this.AddExit(hospitalPower, point10, hospitalStorage, point9, "Tiles\\Decoration\\stairs_up", true);
      this.m_Params.District.AddUniqueMap(hospitalAdmissions);
      this.m_Params.District.AddUniqueMap(hospitalOffices);
      this.m_Params.District.AddUniqueMap(hospitalPatients);
      this.m_Params.District.AddUniqueMap(hospitalStorage);
      this.m_Params.District.AddUniqueMap(hospitalPower);
      this.m_Game.Session.UniqueMaps.Hospital_Admissions = new UniqueMap()
      {
        TheMap = hospitalAdmissions
      };
      this.m_Game.Session.UniqueMaps.Hospital_Offices = new UniqueMap()
      {
        TheMap = hospitalOffices
      };
      this.m_Game.Session.UniqueMaps.Hospital_Patients = new UniqueMap()
      {
        TheMap = hospitalPatients
      };
      this.m_Game.Session.UniqueMaps.Hospital_Storage = new UniqueMap()
      {
        TheMap = hospitalStorage
      };
      this.m_Game.Session.UniqueMaps.Hospital_Power = new UniqueMap()
      {
        TheMap = hospitalPower
      };
    }

    private void GenerateHospitalEntryHall(Map surfaceMap, BaseTownGenerator.Block block)
    {
      this.TileFill(surfaceMap, this.m_Game.GameTiles.FLOOR_TILES, block.InsideRect);
      this.TileRectangle(surfaceMap, this.m_Game.GameTiles.WALL_HOSPITAL, block.BuildingRect);
      this.TileRectangle(surfaceMap, this.m_Game.GameTiles.FLOOR_WALKWAY, block.Rectangle);
      this.DoForEachTile(surfaceMap, block.InsideRect, (Action<Point>) (pt => surfaceMap.GetTileAt(pt).IsInside = true));
      Point point1 = new Point(block.BuildingRect.Left + block.BuildingRect.Width / 2, block.BuildingRect.Bottom - 1);
      Point point2 = new Point(point1.X - 1, point1.Y);
      surfaceMap.GetTileAt(point2.X - 1, point2.Y).AddDecoration("Tiles\\Decoration\\hospital");
      surfaceMap.GetTileAt(point1.X + 1, point1.Y).AddDecoration("Tiles\\Decoration\\hospital");
      Rectangle rect = Rectangle.FromLTRB(block.BuildingRect.Left, block.BuildingRect.Top, block.BuildingRect.Right, block.BuildingRect.Bottom);
      this.PlaceDoor(surfaceMap, point1.X, point1.Y, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjGlassDoor());
      this.PlaceDoor(surfaceMap, point2.X, point2.Y, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjGlassDoor());
      this.DoForEachTile(surfaceMap, rect, (Action<Point>) (pt =>
      {
        if (pt.Y == block.InsideRect.Top || (pt.Y == block.InsideRect.Bottom - 1 || !surfaceMap.IsWalkable(pt.X, pt.Y) || (this.CountAdjWalls(surfaceMap, pt.X, pt.Y) == 0 || this.CountAdjDoors(surfaceMap, pt.X, pt.Y) > 0)))
          return;
        surfaceMap.PlaceMapObjectAt(this.MakeObjIronBench("MapObjects\\iron_bench"), pt);
      }));
      surfaceMap.AddZone(this.MakeUniqueZone("Hospital", block.BuildingRect));
      this.MakeWalkwayZones(surfaceMap, block);
    }

    private Map GenerateHospital_Admissions(int seed)
    {
      Map map = new Map(seed, "Hospital - Admissions", 13, 33)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(this.MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, 5, map.Height);
      int y1 = 0;
      while (y1 <= map.Height - 5)
      {
        Rectangle room = new Rectangle(rectangle1.Left, y1, 5, 5);
        this.MakeHospitalPatientRoom(map, "patient room", room, true);
        y1 += 4;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - 5, 0, 5, map.Height);
      int y2 = 0;
      while (y2 <= map.Height - 5)
      {
        Rectangle room = new Rectangle(rectangle2.Left, y2, 5, 5);
        this.MakeHospitalPatientRoom(map, "patient room", room, false);
        y2 += 4;
      }
      for (int index = 0; index < 10; ++index)
      {
        Actor newHospitalPatient = this.CreateNewHospitalPatient(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalPatient, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "patient room")));
      }
      for (int index = 0; index < 4; ++index)
      {
        Actor newHospitalNurse = this.CreateNewHospitalNurse(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalNurse, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      for (int index = 0; index < 1; ++index)
      {
        Actor newHospitalDoctor = this.CreateNewHospitalDoctor(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalDoctor, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      return map;
    }

    private Map GenerateHospital_Offices(int seed)
    {
      Map map = new Map(seed, "Hospital - Offices", 13, 33)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(this.MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, 5, map.Height);
      int y1 = 0;
      while (y1 <= map.Height - 5)
      {
        Rectangle room = new Rectangle(rectangle1.Left, y1, 5, 5);
        this.MakeHospitalOfficeRoom(map, "office", room, true);
        y1 += 4;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - 5, 0, 5, map.Height);
      int y2 = 0;
      while (y2 <= map.Height - 5)
      {
        Rectangle room = new Rectangle(rectangle2.Left, y2, 5, 5);
        this.MakeHospitalOfficeRoom(map, "office", room, false);
        y2 += 4;
      }
      for (int index = 0; index < 5; ++index)
      {
        Actor newHospitalNurse = this.CreateNewHospitalNurse(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalNurse, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "office")));
      }
      for (int index = 0; index < 2; ++index)
      {
        Actor newHospitalDoctor = this.CreateNewHospitalDoctor(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalDoctor, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "office")));
      }
      return map;
    }

    private Map GenerateHospital_Patients(int seed)
    {
      Map map = new Map(seed, "Hospital - Patients", 13, 49)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(4, 0, 5, map.Height);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(this.MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, 5, map.Height);
      int y1 = 0;
      while (y1 <= map.Height - 5)
      {
        Rectangle room = new Rectangle(rectangle1.Left, y1, 5, 5);
        this.MakeHospitalPatientRoom(map, "patient room", room, true);
        y1 += 4;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - 5, 0, 5, map.Height);
      int y2 = 0;
      while (y2 <= map.Height - 5)
      {
        Rectangle room = new Rectangle(rectangle2.Left, y2, 5, 5);
        this.MakeHospitalPatientRoom(map, "patient room", room, false);
        y2 += 4;
      }
      for (int index = 0; index < 20; ++index)
      {
        Actor newHospitalPatient = this.CreateNewHospitalPatient(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalPatient, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "patient room")));
      }
      for (int index = 0; index < 8; ++index)
      {
        Actor newHospitalNurse = this.CreateNewHospitalNurse(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalNurse, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      for (int index = 0; index < 2; ++index)
      {
        Actor newHospitalDoctor = this.CreateNewHospitalDoctor(0);
        this.ActorPlace(this.m_DiceRoller, map.Width * map.Height, map, newHospitalDoctor, (Predicate<Point>) (pt => map.HasZonePartiallyNamedAt(pt, "corridor")));
      }
      return map;
    }

    private Map GenerateHospital_Storage(int seed)
    {
      Map map = new Map(seed, "Hospital - Storage", 51, 16)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_TILES);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(0, 0, map.Width, 4);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, rect1);
      map.AddZone(this.MakeUniqueZone("north corridor", rect1));
      Rectangle rect2 = Rectangle.FromLTRB(0, rect1.Bottom - 1, map.Width, rect1.Bottom - 1 + 4);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, rect2);
      map.SetTileModelAt(1, rect2.Top, this.m_Game.GameTiles.FLOOR_TILES);
      map.PlaceMapObjectAt(this.MakeObjIronGate("MapObjects\\gate_closed"), new Point(1, rect2.Top));
      map.AddZone(this.MakeUniqueZone("central corridor", rect2));
      Rectangle rectangle1 = new Rectangle(2, rect2.Bottom - 1, map.Width - 2, 4);
      int left1 = rectangle1.Left;
      while (left1 <= map.Width - 5)
      {
        Rectangle room = new Rectangle(left1, rectangle1.Top, 5, 4);
        this.MakeHospitalStorageRoom(map, "storage", room);
        left1 += 4;
      }
      map.SetTileModelAt(1, rectangle1.Top, this.m_Game.GameTiles.FLOOR_TILES);
      Rectangle rect3 = Rectangle.FromLTRB(0, rectangle1.Bottom - 1, map.Width, rectangle1.Bottom - 1 + 4);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, rect3);
      map.SetTileModelAt(1, rect3.Top, this.m_Game.GameTiles.FLOOR_TILES);
      map.AddZone(this.MakeUniqueZone("south corridor", rect3));
      Rectangle rectangle2 = new Rectangle(2, rect3.Bottom - 1, map.Width - 2, 4);
      int left2 = rectangle2.Left;
      while (left2 <= map.Width - 5)
      {
        Rectangle room = new Rectangle(left2, rectangle2.Top, 5, 4);
        this.MakeHospitalStorageRoom(map, "storage", room);
        left2 += 4;
      }
      map.SetTileModelAt(1, rectangle2.Top, this.m_Game.GameTiles.FLOOR_TILES);
      return map;
    }

    private Map GenerateHospital_Power(int seed)
    {
      Map map = new Map(seed, "Hospital - Power", 10, 10)
      {
        Lighting = Lighting._FIRST
      };
      this.DoForEachTile(map, map.Rect, (Action<Point>) (pt => map.GetTileAt(pt).IsInside = true));
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_CONCRETE);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_BRICK, map.Rect);
      Rectangle rect = Rectangle.FromLTRB(1, 1, 3, map.Height);
      map.AddZone(this.MakeUniqueZone("corridor", rect));
      for (int y = 1; y < map.Height - 2; ++y)
        map.PlaceMapObjectAt(this.MakeObjIronFence("MapObjects\\iron_fence"), new Point(2, y));
      Rectangle room = Rectangle.FromLTRB(3, 0, map.Width, map.Height);
      map.AddZone(this.MakeUniqueZone("power room", room));
      this.DoForEachTile(map, room, (Action<Point>) (pt =>
      {
        if (pt.X == room.Left || !map.IsWalkable(pt) || this.CountAdjWalls(map, pt) < 3)
          return;
        map.PlaceMapObjectAt((MapObject) this.MakeObjPowerGenerator("MapObjects\\power_generator_off", "MapObjects\\power_generator_on"), pt);
      }));
      Actor named = this.m_Game.GameActors.JasonMyers.CreateNamed(this.m_Game.GameFactions.ThePsychopaths, "Jason Myers", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\jason_myers");
      this.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      named.Inventory.AddAll(this.MakeItemJasonMyersAxe());
      map.PlaceActorAt(named, new Point(map.Width / 2, map.Height / 2));
      this.m_Game.Session.UniqueActors.JasonMyers = new UniqueActor()
      {
        TheActor = named,
        IsSpawned = true
      };
      return map;
    }

    private Actor CreateNewHospitalPatient(int spawnTime)
    {
      Actor numberedName = (this.m_Rules.Roll(0, 2) == 0 ? this.m_Game.GameActors.MaleCivilian : this.m_Game.GameActors.FemaleCivilian).CreateNumberedName(this.m_Game.GameFactions.TheCivilians, 0);
      this.SkinNakedHuman(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = "Patient " + numberedName.Name;
      numberedName.Controller = (ActorController) new CivilianAI();
      this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, 1);
      numberedName.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\hospital_patient_uniform");
      return numberedName;
    }

    private Actor CreateNewHospitalNurse(int spawnTime)
    {
      Actor numberedName = this.m_Game.GameActors.FemaleCivilian.CreateNumberedName(this.m_Game.GameFactions.TheCivilians, 0);
      this.SkinNakedHuman(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = "Nurse " + numberedName.Name;
      numberedName.Controller = (ActorController) new CivilianAI();
      numberedName.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\hospital_nurse_uniform");
      this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, 1);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.MEDIC);
      numberedName.Inventory.AddAll(this.MakeItemBandages());
      return numberedName;
    }

    private Actor CreateNewHospitalDoctor(int spawnTime)
    {
      Actor numberedName = this.m_Game.GameActors.MaleCivilian.CreateNumberedName(this.m_Game.GameFactions.TheCivilians, 0);
      this.SkinNakedHuman(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = "Doctor " + numberedName.Name;
      numberedName.Controller = (ActorController) new CivilianAI();
      numberedName.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\hospital_doctor_uniform");
      this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, 1);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.MEDIC);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.MEDIC);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.MEDIC);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.LEADERSHIP);
      numberedName.Inventory.AddAll(this.MakeItemMedikit());
      numberedName.Inventory.AddAll(this.MakeItemBandages());
      return numberedName;
    }

    private void MakeHospitalPatientRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, room);
      map.AddZone(this.MakeUniqueZone(baseZoneName, room));
      int x = isFacingEast ? room.Right - 1 : room.Left;
      this.PlaceDoor(map, x, room.Top + 1, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjHospitalDoor());
      Point position1 = new Point(room.Left + room.Width / 2, room.Bottom - 2);
      map.PlaceMapObjectAt(this.MakeObjBed("MapObjects\\hospital_bed"), position1);
      map.PlaceMapObjectAt(this.MakeObjChair("MapObjects\\hospital_chair"), new Point(isFacingEast ? position1.X + 1 : position1.X - 1, position1.Y));
      Point position2 = new Point(isFacingEast ? position1.X - 1 : position1.X + 1, position1.Y);
      map.PlaceMapObjectAt(this.MakeObjNightTable("MapObjects\\hospital_nighttable"), position2);
      if (this.m_DiceRoller.RollChance(50))
      {
        int num = this.m_DiceRoller.Roll(0, 3);
        Item it = (Item) null;
        switch (num)
        {
          case 0:
            it = this.MakeShopPharmacyItem();
            break;
          case 1:
            it = this.MakeItemGroceries();
            break;
          case 2:
            it = this.MakeItemBook();
            break;
        }
        if (it != null)
          map.DropItemAt(it, position2);
      }
      map.PlaceMapObjectAt(this.MakeObjWardrobe("MapObjects\\hospital_wardrobe"), new Point(isFacingEast ? room.Left + 1 : room.Right - 2, room.Top + 1));
    }

    private void MakeHospitalOfficeRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
      this.TileFill(map, this.m_Game.GameTiles.FLOOR_PLANKS, room);
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, room);
      map.AddZone(this.MakeUniqueZone(baseZoneName, room));
      int x1 = isFacingEast ? room.Right - 1 : room.Left;
      int y = room.Top + 2;
      this.PlaceDoor(map, x1, y, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjWoodenDoor());
      int x2 = isFacingEast ? room.Left + 2 : room.Right - 3;
      map.PlaceMapObjectAt(this.MakeObjTable("MapObjects\\table"), new Point(x2, y));
      map.PlaceMapObjectAt(this.MakeObjChair("MapObjects\\chair"), new Point(x2 - 1, y));
      map.PlaceMapObjectAt(this.MakeObjChair("MapObjects\\chair"), new Point(x2 + 1, y));
    }

    private void MakeHospitalStorageRoom(Map map, string baseZoneName, Rectangle room)
    {
      this.TileRectangle(map, this.m_Game.GameTiles.WALL_HOSPITAL, room);
      map.AddZone(this.MakeUniqueZone(baseZoneName, room));
      this.PlaceDoor(map, room.Left + 2, room.Top, this.m_Game.GameTiles.FLOOR_TILES, this.MakeObjHospitalDoor());
      this.DoForEachTile(map, room, (Action<Point>) (pt =>
      {
        if (!map.IsWalkable(pt) || this.CountAdjDoors(map, pt.X, pt.Y) > 0)
          return;
        map.PlaceMapObjectAt(this.MakeObjShelf("MapObjects\\shop_shelf"), pt);
        Item it = this.m_DiceRoller.RollChance(80) ? this.MakeHospitalItem() : this.MakeItemCannedFood();
        if (it.Model.IsStackable)
          it.Quantity = it.Model.StackingLimit;
        map.DropItemAt(it, pt);
      }));
    }

    public void GiveRandomItemToActor(DiceRoller roller, Actor actor, int spawnTime)
    {
      Item it;
      if (new WorldTime(spawnTime).Day > 7 && roller.RollChance(5))
      {
        switch (roller.Roll(0, 6))
        {
          case 0:
            it = this.MakeItemGrenade();
            break;
          case 1:
            it = this.MakeItemArmyBodyArmor();
            break;
          case 2:
            it = this.MakeItemHeavyPistolAmmo();
            break;
          case 3:
            it = this.MakeItemHeavyRifleAmmo();
            break;
          case 4:
            it = this.MakeItemPillsAntiviral();
            break;
          case 5:
            it = this.MakeItemCombatKnife();
            break;
          default:
            it = (Item) null;
            break;
        }
      }
      else
      {
        switch (roller.Roll(0, 10))
        {
          case 0:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType.CONSTRUCTION);
            break;
          case 1:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType._FIRST);
            break;
          case 2:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType.GROCERY);
            break;
          case 3:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType.GUNSHOP);
            break;
          case 4:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType.PHARMACY);
            break;
          case 5:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType.SPORTSWEAR);
            break;
          case 6:
            it = this.MakeRandomShopItem(BaseTownGenerator.ShopType.HUNTING);
            break;
          case 7:
            it = this.MakeRandomParkItem();
            break;
          case 8:
            it = this.MakeRandomBedroomItem();
            break;
          case 9:
            it = this.MakeRandomKitchenItem();
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
      if (this.m_DiceRoller.RollChance(this.Params.PolicemanChance))
      {
        actor = this.CreateNewPoliceman(spawnTime);
        for (int index = 0; index < itemsToCarry && actor.Inventory.CountItems < actor.Inventory.MaxCapacity; ++index)
          this.GiveRandomItemToActor(this.m_DiceRoller, actor, spawnTime);
      }
      else
        actor = this.CreateNewCivilian(spawnTime, itemsToCarry, 1);
      int count = 1 + new WorldTime(spawnTime).Day;
      this.GiveRandomSkillsToActor(this.m_DiceRoller, actor, count);
      return actor;
    }

    public Actor CreateNewSurvivor(int spawnTime)
    {
      bool flag = this.m_Rules.Roll(0, 2) == 0;
      Actor numberedName = (flag ? this.m_Game.GameActors.MaleCivilian : this.m_Game.GameActors.FemaleCivilian).CreateNumberedName(this.m_Game.GameFactions.TheSurvivors, spawnTime);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      this.DressCivilian(this.m_DiceRoller, numberedName);
      numberedName.Doll.AddDecoration(DollPart.HEAD, flag ? "Actors\\Decoration\\survivor_male_bandana" : "Actors\\Decoration\\survivor_female_bandana");
      numberedName.Inventory.AddAll(this.MakeItemCannedFood());
      numberedName.Inventory.AddAll(this.MakeItemArmyRation());
      if (this.m_DiceRoller.RollChance(50))
      {
        numberedName.Inventory.AddAll(this.MakeItemArmyRifle());
        if (this.m_DiceRoller.RollChance(50))
          numberedName.Inventory.AddAll(this.MakeItemHeavyRifleAmmo());
        else
          numberedName.Inventory.AddAll(this.MakeItemGrenade());
      }
      else
      {
        numberedName.Inventory.AddAll(this.MakeItemShotgun());
        if (this.m_DiceRoller.RollChance(50))
          numberedName.Inventory.AddAll(this.MakeItemShotgunAmmo());
        else
          numberedName.Inventory.AddAll(this.MakeItemGrenade());
      }
      numberedName.Inventory.AddAll(this.MakeItemMedikit());
      switch (this.m_DiceRoller.Roll(0, 3))
      {
        case 0:
          numberedName.Inventory.AddAll(this.MakeItemPillsSLP());
          break;
        case 1:
          numberedName.Inventory.AddAll(this.MakeItemPillsSTA());
          break;
        case 2:
          numberedName.Inventory.AddAll(this.MakeItemPillsSAN());
          break;
      }
      numberedName.Inventory.AddAll(this.MakeItemArmyBodyArmor());
      int count = 3 + new WorldTime(spawnTime).Day;
      this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, count);
      numberedName.Controller = (ActorController) new CivilianAI();
      int max1 = (int) (0.25 * (double) numberedName.FoodPoints);
      numberedName.FoodPoints = numberedName.FoodPoints - this.m_Rules.Roll(0, max1);
      int max2 = (int) (0.25 * (double) numberedName.SleepPoints);
      numberedName.SleepPoints = numberedName.SleepPoints - this.m_Rules.Roll(0, max2);
      return numberedName;
    }

    public Actor CreateNewNakedHuman(int spawnTime, int itemsToCarry, int skills)
    {
      return (this.m_Rules.Roll(0, 2) == 0 ? this.m_Game.GameActors.MaleCivilian : this.m_Game.GameActors.FemaleCivilian).CreateNumberedName(this.m_Game.GameFactions.TheCivilians, spawnTime);
    }

    public Actor CreateNewCivilian(int spawnTime, int itemsToCarry, int skills)
    {
      Actor numberedName = (this.m_Rules.Roll(0, 2) == 0 ? this.m_Game.GameActors.MaleCivilian : this.m_Game.GameActors.FemaleCivilian).CreateNumberedName(this.m_Game.GameFactions.TheCivilians, spawnTime);
      this.DressCivilian(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      for (int index = 0; index < itemsToCarry; ++index)
        this.GiveRandomItemToActor(this.m_DiceRoller, numberedName, spawnTime);
      this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, skills);
      numberedName.Controller = (ActorController) new CivilianAI();
      int max1 = (int) (0.25 * (double) numberedName.FoodPoints);
      numberedName.FoodPoints = numberedName.FoodPoints - this.m_Rules.Roll(0, max1);
      int max2 = (int) (0.25 * (double) numberedName.SleepPoints);
      numberedName.SleepPoints = numberedName.SleepPoints - this.m_Rules.Roll(0, max2);
      return numberedName;
    }

    public Actor CreateNewPoliceman(int spawnTime)
    {
      Actor numberedName = this.m_Game.GameActors.Policeman.CreateNumberedName(this.m_Game.GameFactions.ThePolice, spawnTime);
      this.DressPolice(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = "Cop " + numberedName.Name;
      this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, 1);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.FIREARMS);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.LEADERSHIP);
      numberedName.Controller = (ActorController) new CivilianAI();
      if (this.m_DiceRoller.RollChance(50))
      {
        numberedName.Inventory.AddAll(this.MakeItemPistol());
        numberedName.Inventory.AddAll(this.MakeItemLightPistolAmmo());
      }
      else
      {
        numberedName.Inventory.AddAll(this.MakeItemShotgun());
        numberedName.Inventory.AddAll(this.MakeItemShotgunAmmo());
      }
      numberedName.Inventory.AddAll(this.MakeItemTruncheon());
      numberedName.Inventory.AddAll(this.MakeItemFlashlight());
      numberedName.Inventory.AddAll(this.MakeItemPoliceRadio());
      if (this.m_DiceRoller.RollChance(50))
      {
        if (this.m_DiceRoller.RollChance(80))
          numberedName.Inventory.AddAll(this.MakeItemPoliceJacket());
        else
          numberedName.Inventory.AddAll(this.MakeItemPoliceRiotArmor());
      }
      return numberedName;
    }

    public Actor CreateNewUndead(int spawnTime)
    {
      Actor actor;
      if (Rules.HasAllZombies(this.m_Game.Session.GameMode))
      {
        int num = this.m_Rules.Roll(0, 100);
        actor = (num < RogueGame.Options.SpawnSkeletonChance ? this.m_Game.GameActors.Skeleton : (num < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance ? this.m_Game.GameActors.Zombie : (num < RogueGame.Options.SpawnSkeletonChance + RogueGame.Options.SpawnZombieChance + RogueGame.Options.SpawnZombieMasterChance ? this.m_Game.GameActors.ZombieMaster : this.m_Game.GameActors.Skeleton))).CreateNumberedName(this.m_Game.GameFactions.TheUndeads, spawnTime);
      }
      else
      {
        actor = this.MakeZombified((Actor) null, this.CreateNewCivilian(spawnTime, 0, 0), spawnTime);
        int num = new WorldTime(spawnTime).Day / 2;
        if (num > 0)
        {
          for (int index = 0; index < num; ++index)
          {
            Skills.IDs? nullable = this.m_Game.ZombifySkill((Skills.IDs) this.m_Rules.Roll(0, 29));
            if (nullable.HasValue)
              this.m_Game.SkillUpgrade(actor, nullable.Value);
          }
          this.RecomputeActorStartingStats(actor);
        }
      }
      return actor;
    }

    public Actor MakeZombified(Actor zombifier, Actor deadVictim, int turn)
    {
      string properName = string.Format("{0}'s zombie", (object) deadVictim.UnmodifiedName);
      Actor named = (deadVictim.Doll.Body.IsMale ? this.m_Game.GameActors.MaleZombified : this.m_Game.GameActors.FemaleZombified).CreateNamed(zombifier == null ? this.m_Game.GameFactions.TheUndeads : zombifier.Faction, properName, deadVictim.IsPluralName, turn);
      for (DollPart part = DollPart._FIRST; part < DollPart._COUNT; ++part)
      {
        List<string> decorations = deadVictim.Doll.GetDecorations(part);
        if (decorations != null)
        {
          foreach (string imageID in decorations)
            named.Doll.AddDecoration(part, imageID);
        }
      }
      named.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\bloodied");
      return named;
    }

    public Actor CreateNewSewersUndead(int spawnTime)
    {
      if (!Rules.HasAllZombies(this.m_Game.Session.GameMode))
        return this.CreateNewUndead(spawnTime);
      return (this.m_DiceRoller.RollChance(80) ? this.m_Game.GameActors.RatZombie : this.m_Game.GameActors.Zombie).CreateNumberedName(this.m_Game.GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewBasementRatZombie(int spawnTime)
    {
      if (!Rules.HasAllZombies(this.m_Game.Session.GameMode))
        return this.CreateNewUndead(spawnTime);
      return this.m_Game.GameActors.RatZombie.CreateNumberedName(this.m_Game.GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewSubwayUndead(int spawnTime)
    {
      if (!Rules.HasAllZombies(this.m_Game.Session.GameMode))
        return this.CreateNewUndead(spawnTime);
      return this.m_Game.GameActors.Zombie.CreateNumberedName(this.m_Game.GameFactions.TheUndeads, spawnTime);
    }

    public Actor CreateNewCHARGuard(int spawnTime)
    {
      Actor numberedName = this.m_Game.GameActors.CHARGuard.CreateNumberedName(this.m_Game.GameFactions.TheCHARCorporation, spawnTime);
      this.DressCHARGuard(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = "Gd. " + numberedName.Name;
      numberedName.Inventory.AddAll(this.MakeItemShotgun());
      numberedName.Inventory.AddAll(this.MakeItemShotgunAmmo());
      numberedName.Inventory.AddAll(this.MakeItemCHARLightBodyArmor());
      return numberedName;
    }

    public Actor CreateNewArmyNationalGuard(int spawnTime, string rankName)
    {
      Actor numberedName = this.m_Game.GameActors.NationalGuard.CreateNumberedName(this.m_Game.GameFactions.TheArmy, spawnTime);
      this.DressArmy(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = rankName + " " + numberedName.Name;
      numberedName.Inventory.AddAll(this.MakeItemArmyRifle());
      numberedName.Inventory.AddAll(this.MakeItemHeavyRifleAmmo());
      numberedName.Inventory.AddAll(this.MakeItemArmyPistol());
      numberedName.Inventory.AddAll(this.MakeItemHeavyPistolAmmo());
      numberedName.Inventory.AddAll(this.MakeItemArmyBodyArmor());
      ItemBarricadeMaterial barricadeMaterial = this.MakeItemWoodenPlank();
      barricadeMaterial.Quantity = this.m_Game.GameItems.WOODENPLANK.StackingLimit;
      numberedName.Inventory.AddAll((Item) barricadeMaterial);
      this.GiveStartingSkillToActor(numberedName, Skills.IDs.CARPENTRY);
      int count = new WorldTime(spawnTime).Day - 3;
      if (count > 0)
        this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewBikerMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = this.m_Game.GameActors.BikerMan.CreateNumberedName(this.m_Game.GameFactions.TheBikers, spawnTime);
      numberedName.GangID = (int) gangId;
      this.DressBiker(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Controller = (ActorController) new GangAI();
      numberedName.Inventory.AddAll(this.m_DiceRoller.RollChance(50) ? this.MakeItemCrowbar() : this.MakeItemBaseballBat());
      numberedName.Inventory.AddAll(this.MakeItemBikerGangJacket(gangId));
      int count = new WorldTime(spawnTime).Day - 2;
      if (count > 0)
        this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewGangstaMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = this.m_Game.GameActors.GangstaMan.CreateNumberedName(this.m_Game.GameFactions.TheGangstas, spawnTime);
      numberedName.GangID = (int) gangId;
      this.DressGangsta(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Controller = (ActorController) new GangAI();
      numberedName.Inventory.AddAll(this.m_DiceRoller.RollChance(50) ? this.MakeItemRandomPistol() : this.MakeItemBaseballBat());
      int count = new WorldTime(spawnTime).Day - 7;
      if (count > 0)
        this.GiveRandomSkillsToActor(this.m_DiceRoller, numberedName, count);
      return numberedName;
    }

    public Actor CreateNewBlackOps(int spawnTime, string rankName)
    {
      Actor numberedName = this.m_Game.GameActors.BlackOps.CreateNumberedName(this.m_Game.GameFactions.TheBlackOps, spawnTime);
      this.DressBlackOps(this.m_DiceRoller, numberedName);
      this.GiveNameToActor(this.m_DiceRoller, numberedName);
      numberedName.Name = rankName + " " + numberedName.Name;
      numberedName.Inventory.AddAll(this.MakeItemPrecisionRifle());
      numberedName.Inventory.AddAll(this.MakeItemHeavyRifleAmmo());
      numberedName.Inventory.AddAll(this.MakeItemArmyPistol());
      numberedName.Inventory.AddAll(this.MakeItemHeavyPistolAmmo());
      numberedName.Inventory.AddAll(this.MakeItemBlackOpsGPS());
      return numberedName;
    }

    public Actor CreateNewFeralDog(int spawnTime)
    {
      Actor numberedName = this.m_Game.GameActors.FeralDog.CreateNumberedName(this.m_Game.GameFactions.TheFerals, spawnTime);
      this.SkinDog(this.m_DiceRoller, numberedName);
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
      map.AddZone(this.MakeUniqueZone("walkway", new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width - 1, 1)));
      map.AddZone(this.MakeUniqueZone("walkway", new Rectangle(rectangle.Left + 1, rectangle.Bottom - 1, rectangle.Width - 1, 1)));
      map.AddZone(this.MakeUniqueZone("walkway", new Rectangle(rectangle.Right - 1, rectangle.Top, 1, rectangle.Height - 1)));
      map.AddZone(this.MakeUniqueZone("walkway", new Rectangle(rectangle.Left, rectangle.Top + 1, 1, rectangle.Height - 1)));
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

      public int MapWidth
      {
        get
        {
          return this.m_MapWidth;
        }
        set
        {
          if (value <= 0 || value > 100)
            throw new ArgumentOutOfRangeException("MapWidth");
          this.m_MapWidth = value;
        }
      }

      public int MapHeight
      {
        get
        {
          return this.m_MapHeight;
        }
        set
        {
          if (value <= 0 || value > 100)
            throw new ArgumentOutOfRangeException("MapHeight");
          this.m_MapHeight = value;
        }
      }

      public int MinBlockSize
      {
        get
        {
          return this.m_MinBlockSize;
        }
        set
        {
          if (value < 4 || value > 32)
            throw new ArgumentOutOfRangeException("MinBlockSize must be [4..32]");
          this.m_MinBlockSize = value;
        }
      }

      public int WreckedCarChance
      {
        get
        {
          return this.m_WreckedCarChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("WreckedCarChance must be [0..100]");
          this.m_WreckedCarChance = value;
        }
      }

      public int ShopBuildingChance
      {
        get
        {
          return this.m_ShopBuildingChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("ShopBuildingChance must be [0..100]");
          this.m_ShopBuildingChance = value;
        }
      }

      public int ParkBuildingChance
      {
        get
        {
          return this.m_ParkBuildingChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("ParkBuildingChance must be [0..100]");
          this.m_ParkBuildingChance = value;
        }
      }

      public int CHARBuildingChance
      {
        get
        {
          return this.m_CHARBuildingChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("CHARBuildingChance must be [0..100]");
          this.m_CHARBuildingChance = value;
        }
      }

      public int PostersChance
      {
        get
        {
          return this.m_PostersChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("PostersChance must be [0..100]");
          this.m_PostersChance = value;
        }
      }

      public int TagsChance
      {
        get
        {
          return this.m_TagsChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("TagsChance must be [0..100]");
          this.m_TagsChance = value;
        }
      }

      public int ItemInShopShelfChance
      {
        get
        {
          return this.m_ItemInShopShelfChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("ItemInShopShelfChance must be [0..100]");
          this.m_ItemInShopShelfChance = value;
        }
      }

      public int PolicemanChance
      {
        get
        {
          return this.m_PolicemanChance;
        }
        set
        {
          if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException("PolicemanChance must be [0..100]");
          this.m_PolicemanChance = value;
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
        this.ResetRectangle(rect);
      }

      public Block(BaseTownGenerator.Block copyFrom)
      {
        this.Rectangle = copyFrom.Rectangle;
        this.BuildingRect = copyFrom.BuildingRect;
        this.InsideRect = copyFrom.InsideRect;
      }

      public void ResetRectangle(Rectangle rect)
      {
        this.Rectangle = rect;
        this.BuildingRect = new Rectangle(rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2);
        this.InsideRect = new Rectangle(this.BuildingRect.Left + 1, this.BuildingRect.Top + 1, this.BuildingRect.Width - 2, this.BuildingRect.Height - 2);
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
