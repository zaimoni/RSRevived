// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.BaseTownGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define MORE_AGGRESSIVE_CONNECTED_BASEMENTS

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;

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
    private readonly KeyValuePair<GameItems.IDs,GameItems.IDs>[] survivalist_ranged_candidates;

    private Parameters m_Params = DEFAULT_PARAMS;
    private const int HOSPITAL_TYPICAL_WIDTH_HEIGHT = 5;
    private const int PARK_TREE_CHANCE = 25;
    private const int PARK_BENCH_CHANCE = 5;
    private const int PARK_ITEM_CHANCE = 5;
    private const int PARK_SHED_CHANCE = 50;  // alpha10
    private const int PARK_SHED_WIDTH = 5;  // alpha10
    private const int PARK_SHED_HEIGHT = 5;  // alpha10
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
    // alpha10 new house stuff
    private const int HOUSE_OUTSIDE_ROOM_NEED_MIN_ROOMS = 4;
    private const int HOUSE_OUTSIDE_ROOM_CHANCE = 75;
    private const int HOUSE_GARDEN_TREE_CHANCE = 10;  // per tile
    private const int HOUSE_PARKING_LOT_CAR_CHANCE = 10;  // per tile
    private const int HOUSE_IS_APARTMENTS_CHANCE = 50;  // alpha10.1 new house floorplan: apartements
    private const int SHOP_BASEMENT_CHANCE = 30;
    private const int SHOP_BASEMENT_SHELF_CHANCE_PER_TILE = 5;
    private const int SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF = 33;
    private const int SHOP_WINDOW_CHANCE = 30;
    private const int SHOP_BASEMENT_ZOMBIE_RAT_CHANCE = 5;
    private List<Block> m_SurfaceBlocks;    // inconsistently used -- while this enables deferred creation of sewers and subways, such deferred creation does stomp on already-generated blocks

    // world generation assistants
    static private Point HospitalWorldPos;
    static private Point PoliceStationWorldPos;
    static private readonly List<List<Point>> SubwayElectrifyPlans = new List<List<Point>>();    // XXX possible template class target: logic problem solver, AND of OR-clauses
    static private readonly HashSet<Point> ForceSubwayStation = new HashSet<Point>();   // XXX possible template class target: logic problem solver, AND of OR-clauses

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

#if DEBUG
      if (bedroom_checksum != bedroom_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: "+ bedroom_stock.Sum(x => x.Value));
      if (CHAR_armory_checksum != CHAR_armory_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: "+ CHAR_armory_stock.Sum(x => x.Value));
      if (CHAR_office_checksum != CHAR_office_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: "+ CHAR_office_stock.Sum(x => x.Value));
      if (construction_shop_checksum != construction_shop_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: "+ construction_shop_stock.Sum(x => x.Value));
      if (gunshop_checksum != gunshop_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: " + gunshop_stock.Sum(x => x.Value));
      if (hospital_shop_checksum != hospital_shop_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: "+ hospital_shop_stock.Sum(x => x.Value));
      if (hunting_shop_checksum != hunting_shop_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: " + hunting_shop_stock.Sum(x => x.Value));
      if (park_checksum != park_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck: " + park_stock.Sum(x => x.Value));
      if (sewer_checksum != sewer_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck " + sewer_stock.Sum(x => x.Value));
      if (sportswear_shop_checksum != sportswear_shop_stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck " + sportswear_shop_stock.Sum(x => x.Value));
#endif

      // hook for planned pre-apocalypse politics
      // following is RED STATE, RED CITY

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

    static public void WorldGenInit(DistrictKind[] zoning)
    {
      // subway city planning.  Each subway station can electrify the subway rails not just for its district, but one district away.
      // make sure all subway rails can be electrified by at least one subway station.
      SubwayElectrifyPlans.Clear();
      ForceSubwayStation.Clear();

      var world = Session.Get.World;
      Rectangle world_bounds = new Rectangle(0,0, world.Size, world.Size);
      world_bounds.DoForEach(pt => {
        if (0<world.SubwayLayout(pt)) {
          var working = new List<Point>();
          if (CanHaveSubwayStationBlocks(Session.Get.World.SubwayLayout(pt))) working.Add(pt);
          foreach(var dir in Direction.COMPASS_4) {
            Point pt2 = pt+dir;
            if (!world_bounds.Contains(pt2)) continue;
            if (0 < world.SubwayLayout(pt2) && CanHaveSubwayStationBlocks(Session.Get.World.SubwayLayout(pt2))) working.Add(pt2);
          }
#if DEBUG
          if (0>=working.Count) throw new InvalidOperationException("isolated node in subway network");
#endif
          SubwayElectrifyPlans.Add(working);
        }
      });
      // generally speaking:
      // police and hospital both strongly prefer one of shopping, business, or general districts and do not like residential or green
      // they also both like a central location
      static bool essential_services_ok(DistrictKind k) {
         switch(k) {
         case DistrictKind.GENERAL: return true;
         case DistrictKind.SHOPPING: return true;
         case DistrictKind.BUSINESS: return true;
         default: return false;
         }
      }

      var anchor = World.CHAR_City_Origin+ world.CHAR_CityLimits.Size/2;
      var scan = new Rectangle(anchor + Direction.NW, (Point)3);
      var essential_services_acceptable = new List<Point>();

      if (essential_services_ok(zoning[world.fromWorldPos(anchor)])) essential_services_acceptable.Add(anchor);

      foreach(var dir in Direction.COMPASS) {
        var w_pos = anchor+dir;
        if (essential_services_ok(zoning[world.fromWorldPos(w_pos)])) essential_services_acceptable.Add(w_pos);
      }

      // Cf. BaseMapGenerator::RandomDistrictInCity().  Not usable here due to sequential choice without replacement.
      var dr = Rules.Get.DiceRoller;
      PoliceStationWorldPos = dr.ChooseWithoutReplacement(essential_services_acceptable);
      HospitalWorldPos = dr.ChooseWithoutReplacement(essential_services_acceptable);
    }

    static public District GetCHARbaseDistrict()
    {
      var districtList = new List<KeyValuePair<District, int>>();
      Session.Get.World.DoForAllDistricts(d=>{
        if (DistrictKind.BUSINESS != d.Kind) return;
        foreach(var zone in d.EntryMap.Zones) {
          if (!zone.Attribute.HasKey("CHAR Office")) continue;
          districtList.Add(new KeyValuePair<District,int>(d, Rules.GridDistance(d.WorldPosition, PoliceStationWorldPos)));
          return;
        }
      });
      int ub = districtList.Count;
      if (0 >= ub) throw new InvalidOperationException("world has no business districts with offices");
      // close to, but not on top of, the police station
      if (2 <= ub) {
        var bounds = districtList.MinMax();
        while (bounds.Key < bounds.Value) {
          if (0 == bounds.Key) {
            while(0 <= --ub) {
              if (0 == districtList[ub].Value) {
                districtList.RemoveAt(ub);
                break;
              }
            }
          } else {
            while(0 <= --ub) {
              if (bounds.Value == districtList[ub].Value) districtList.RemoveAt(ub);
            }
          }
          bounds = districtList.MinMax();
          ub = districtList.Count;
        }
      }
      // as far away from the hospital as possible
      if (2 <= ub) {
        while (0 <= --ub) districtList[ub] = new KeyValuePair<District, int>(districtList[ub].Key, Rules.GridDistance(districtList[ub].Key.WorldPosition, HospitalWorldPos));
        var bounds = districtList.MinMax();
        while (bounds.Key < bounds.Value) {
          ub = districtList.Count;
          while(0 <= --ub) {
            if (bounds.Key == districtList[ub].Value) districtList.RemoveAt(ub);
          }
          bounds = districtList.MinMax();
        }
      }

      return Rules.Get.DiceRoller.Choose(districtList).Key;
    }

    protected void AddWreckedCarsOutside(Map map)
    {
      MapObjectFill(map, map.Rect, pt => {
        if (m_DiceRoller.RollChance(m_Params.WreckedCarChance)) {
          Tile tileAt = map.GetTileAt(pt);
          if (!tileAt.IsInside && tileAt.Model.IsWalkable && tileAt.Model != GameTiles.FLOOR_GRASS) {
            MapObject mapObj = MakeObjWreckedCar(m_DiceRoller);
            if (m_DiceRoller.RollChance(50)) mapObj.Ignite();
            return mapObj;
          }
        }
        return null;
      });
    }

    protected void DecorateOutsideWallsWithPosters(Map map, int chancePerWall)
    {
      DecorateOutsideWalls(map, map.Rect, pt => (m_DiceRoller.RollChance(chancePerWall) ? m_DiceRoller.Choose(POSTERS) : null));
    }

    protected void DecorateOutsideWallsWithTags(Map map, int chancePerWall)
    {
      DecorateOutsideWalls(map, map.Rect, pt => (m_DiceRoller.RollChance(chancePerWall) ? m_DiceRoller.Choose(TAGS) : null));
    }

    public override Map Generate(int seed, string name)
    {
      m_DiceRoller = new DiceRoller(seed);
      Map map = new Map(seed, name, m_Params.District, m_Params.MapWidth, m_Params.MapHeight);

      TileFill(map, GameTiles.FLOOR_GRASS);
restart:
      List<Block> list = new List<Block>();
      MakeBlocks(map, true, ref list, map.Rect);
      List<Block> blockList1 = new List<Block>(list);
      m_SurfaceBlocks = new List<Block>(list.Count);
      foreach (Block copyFrom in list)
        m_SurfaceBlocks.Add(new Block(copyFrom));

      // give subway fairly high priority
      Point world_pos = map.DistrictPos;
      if (0 < Session.Get.World.SubwayLayout(world_pos)) {
        if (ForceSubwayStation.Contains(world_pos)) {
          var test = GetSubwayStationBlocks(map, Session.Get.World.SubwayLayout(world_pos));
          if (null == test) goto restart;
        }
        GenerateSubwayMap(map.Seed << 2 ^ map.Seed, map, out Block subway_station);
        if (null != subway_station) {
          blockList1.RemoveAll(b => b.Rectangle==subway_station.Rectangle);
          // maintain building plans:
          ForceSubwayStation.Remove(world_pos);
          SubwayElectrifyPlans.OnlyIfNot(x => x.Contains(world_pos));
        } else {    // no station here
          int i = SubwayElectrifyPlans.Count;
          while(0 < i--) {
            if (SubwayElectrifyPlans[i].Contains(world_pos)) {
              SubwayElectrifyPlans[i].Remove(world_pos);
              if (1== SubwayElectrifyPlans[i].Count) {
                ForceSubwayStation.Add(SubwayElectrifyPlans[i][0]);
                SubwayElectrifyPlans.RemoveAt(i);
              }
            }
          }
        }
      }

      if (world_pos == PoliceStationWorldPos) MakePoliceStation(map, blockList1);
      if (world_pos == HospitalWorldPos) MakeHospital(map, blockList1);
      List<Block> blockList2 = new List<Block>(blockList1.Count);
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
      AddWreckedCarsOutside(map);
      DecorateOutsideWallsWithPosters(map, m_Params.PostersChance);
      DecorateOutsideWallsWithTags(map, m_Params.TagsChance);
      map.BgMusic = GameMusics.SURFACE; // alpha10: music
      return map;
    }

    private const int sewer_checksum = 12;
    private readonly KeyValuePair<GameItems.IDs, int>[] sewer_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.LIGHT_BIG_FLASHLIGHT,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_CROWBAR,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT1,1),  // RS9: all spray paints equal weight due to function
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT2,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT3,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT4,1)
    };

    public virtual Map GenerateSewersMap(int seed, District district)
    {
      m_DiceRoller = new DiceRoller(seed);
restart:
      Map sewers = new Map(seed, string.Format("Sewers@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y), district, district.EntryMap.Width, district.EntryMap.Height, Lighting.DARKNESS);
      sewers.AddZone(MakeUniqueZone("sewers", sewers.Rect));

      // Building codes require that all passages be 2 wide, even those on the edge of the city.
      var edge_code = Session.Get.World.CHAR_CityLimits.EdgeCode(district.WorldPosition);
      var dev_rect = sewers.Rect;
      if (0 != (edge_code & 1)) {
        dev_rect.Y += 1;
        dev_rect.Height -= 1;
      }
      if (0 != (edge_code & 2)) dev_rect.Width -= 1;
      if (0 != (edge_code & 4)) dev_rect.Height -= 1;
      if (0 != (edge_code & 8)) {
        dev_rect.X += 1;
        dev_rect.Width -= 1;
      }

      if (0 != edge_code) TileFill(sewers, GameTiles.FLOOR_SEWER_WATER, true);
      TileFill(sewers, GameTiles.WALL_SEWER, dev_rect, true);
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: baseline");
#endif

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
      MakeBlocks(sewers, false, ref list, dev_rect);
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: #1 ok");
#endif

#region 2. Make tunnels.
      foreach (Block block in list)
        TileRectangle(sewers, GameTiles.FLOOR_SEWER_WATER, block.Rectangle);
      foreach (Block block in list) {
        if (!m_DiceRoller.RollChance(SEWERS_IRON_FENCE_PER_BLOCK_CHANCE)) continue;
        do {
          Direction dir = m_DiceRoller.Choose(Direction.COMPASS_4);
          bool orientation_ew = (2 == dir.Index%4);
          Point gate = block.Rectangle.Anchor((Compass.XCOMlike)dir.Index);
          if (sewers.IsOnMapBorder(gate)) continue; // \todo make this test always-false
          if (orientation_ew) {
            gate.Y = m_DiceRoller.Roll(block.BuildingRect.Top, block.BuildingRect.Bottom);
          } else {
            gate.X = m_DiceRoller.Roll(block.BuildingRect.Left, block.BuildingRect.Right);
          }
          if (sewers.IsOnMapBorder(gate)) continue;  // just in case \todo make this test always-false
          if (3 != CountAdjWalls(sewers, gate)) continue;
          Point gate2 = gate+dir;
          if (sewers.IsOnMapBorder(gate2)) continue;
          if (3 != CountAdjWalls(sewers, gate2)) continue;
          MapObjectPlace(sewers, gate, MakeObjIronFence());
          MapObjectPlace(sewers, gate2, MakeObjIronFence());
          break;
        } while(true);
      }
#endregion
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: #2 ok");
#endif

#region 3. Link with surface.
      // This stage empirically can infinite-loop.
      {
      var candidates = new List<Point>();
      sewers.Rect.DoForEach(pt => {
        if (!sewers.GetTileModelAt(pt).IsWalkable) return;
        Tile tileAt = surface.GetTileAt(pt);
        if (!tileAt.Model.IsWalkable) return;
        if (sewers.HasMapObjectAt(pt)) return;
        if (tileAt.IsInside) return;
        if (surface.HasMapObjectAt(pt)) return;
        if (tileAt.Model == GameTiles.FLOOR_WALKWAY || tileAt.Model == GameTiles.FLOOR_GRASS) candidates.Add(pt);
      });
      if (0 >= candidates.Count) goto restart;

      int countLinks = 0;
      do {
        foreach(Point pt in candidates) {
          // these two tests will only trigger on sewer exits due to the prefilter above.  Adjacency across district boundaries is a known bug
          if (sewers.HasAnyAdjacentInMap(pt, p => sewers.HasExitAt(in p))) continue;
          if (surface.HasAnyAdjacentInMap(pt, p => surface.HasExitAt(in p))) continue;
          if (1<candidates.Count && !m_DiceRoller.RollChance(3)) continue;
          AddExit(sewers, pt, surface, pt, GameImages.DECO_SEWER_LADDER);
          AddExit(surface, pt, sewers, pt, GameImages.DECO_SEWER_HOLE);
          ++countLinks;
        }
      }
      while (countLinks < 1);
      }
#endregion
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: #3 ok");
#endif

#region 5. Sewers Maintenance Room & Building(surface).
      List<Block> blockList = null;
      foreach (Block mSurfaceBlock in m_SurfaceBlocks) {
        if (    mSurfaceBlock.BuildingRect.Width <= m_Params.MinBlockSize + 2
            &&  mSurfaceBlock.BuildingRect.Height <= m_Params.MinBlockSize + 2
            && !IsThereASpecialBuilding(surface, mSurfaceBlock.InsideRect)
            && !mSurfaceBlock.Rectangle.Any(pt => sewers.GetTileModelAt(pt).IsWalkable)) {
          (blockList ??= new List<Block>(m_SurfaceBlocks.Count)).Add(mSurfaceBlock);
        }
      }
      if (blockList != null) {
        Block block = m_DiceRoller.Choose(blockList);
        ClearRectangle(surface, block.BuildingRect);
        TileFill(surface, GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        m_SurfaceBlocks.Remove(block);
        Rectangle buildingRect = block.BuildingRect;
        var exitPosition = buildingRect.Location + buildingRect.Size / 2;
        MakeSewersMaintenanceBuilding(surface, true, block, sewers, exitPosition);
        MakeSewersMaintenanceBuilding(sewers, false, block, surface, exitPosition);
      }
#endregion
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: #5 ok");
#endif

#region 6. Some rooms.
      foreach (Block block in list)
      {
        if (m_DiceRoller.RollChance(SEWERS_ROOM_CHANCE) && CheckForEachTile(block.BuildingRect, (Predicate<Point>) (pt => !sewers.GetTileModelAt(pt).IsWalkable)))
        {
          TileFill(sewers, GameTiles.FLOOR_CONCRETE, block.InsideRect);
          foreach(var dir in Direction.COMPASS_4) sewers.SetTileModelAt(block.BuildingRect.Anchor((Compass.XCOMlike)dir.Index), GameTiles.FLOOR_CONCRETE);
          sewers.AddZone(MakeUniqueZone("room", block.InsideRect));
        }
      }
#endregion
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: #6 ok");
#endif

#region 7. Objects.
      MapObjectFill(sewers, sewers.Rect, (Func<Point, MapObject>) (pt =>
      {
        if (!m_DiceRoller.RollChance(SEWERS_JUNK_CHANCE)) return null;
        if (!sewers.IsWalkable(pt)) return null;
        return MakeObjJunk();
      }));
#endregion
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: #7 ok");
#endif

#region 8. Items.
      Item sewers_stock() {
          return PostprocessQuantity(GameItems.From(sewer_stock.UseRarityTable(m_DiceRoller.Roll(0, sewer_checksum))).create());
      }
      sewers.Rect.DoForEach(pt => sewers.DropItemAt(sewers_stock(), in pt),
                            pt => sewers.IsWalkable(pt) && m_DiceRoller.RollChance(SEWERS_ITEM_CHANCE));
#endregion
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "GenerateSewersMap: 88 ok");
#endif

#region 9. Tags.
      for (short x = 0; x < sewers.Width; ++x) {    // \todo convert this iteration to a stack point?
        for (short y = 0; y < sewers.Height; ++y) {
          Point pt = new Point(x,y);
          Tile tileAt = sewers.GetTileAt(pt);
          if (!tileAt.Model.IsWalkable && CountAdjWalkables(sewers, pt) >= 2 && m_DiceRoller.RollChance(SEWERS_TAG_CHANCE)) {
            tileAt.AddDecoration(m_DiceRoller.Choose(TAGS));
          }
        }
      }
#endregion

      // technically inappropriate for a generic library
      if (district.WorldPosition == World.CHAR_City_Origin) {
        Point graffiti = dev_rect.Location + Direction.SE;
        sewers.RemoveMapObjectAt(graffiti);
        var graffiti_tile = sewers.GetTileAt(graffiti);
        graffiti_tile.RemoveAllDecorations();
        graffiti_tile.AddDecoration(GameImages.DECO_ROGUEDJACK_TAG);
      }

      // alpha10
      // 10. Music.
      sewers.BgMusic = GameMusics.SEWERS;

      district.SewersMap = sewers;
      return sewers;
    }

    // N-S and E-W rails use this as their reference point.
    static private Point SubwayRail(District d) {
      // original version was reading the incoming dimensions off of the incoming entryMap
      // but this was simply the district size
      int deviate_at = Session.Get.World.CitySize-1;
      int half_dim = RogueGame.Options.DistrictSize/2;
      Point mid_map = (Point)half_dim;
      // need diagonals at An and n0 flush
      if (deviate_at==d.WorldPosition.X) {
        mid_map += Direction.W;
        mid_map += Direction.W;
      }
      if (deviate_at==d.WorldPosition.Y) {
        mid_map += Direction.N;
        mid_map += Direction.N;
      }

      return mid_map + Direction.NW;  // both the N-S and E-W railways use this as their reference point
    }

    static public bool CanHaveSubwayStationBlocks(uint geometry)
    {
      const uint N_NEUTRAL = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint E_NEUTRAL = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint S_NEUTRAL = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint W_NEUTRAL = (uint)Compass.XCOMlike.W * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      var layout = new Compass.LineGraph(geometry);

      if (layout.ContainsLineSegment(E_NEUTRAL)) return true;
      if (layout.ContainsLineSegment(W_NEUTRAL)) return true;
      if (layout.ContainsLineSegment(N_NEUTRAL)) return true;
      if (layout.ContainsLineSegment(S_NEUTRAL)) return true;
      return false;
    }

    // geometry is a Godel-encoded series of compass-point line segments
    public List<Block> GetSubwayStationBlocks(Map entryMap, uint geometry)
    {
      // subway station code isn't meant to handle the diagnonals so bail on those early
      List<Block> blockList = null;
      // entry map has same dimensions as incoming subway map
      // rail line is 4 squares high (does not scale until close to 900 turns/hour)
      // EW: reserved coordinates are y1 to y1+3 inclusive, so subway.Width/2-1 to subway.Width/2+2
      const int height = 4;
      Point rail = SubwayRail(entryMap.District);  // both the N-S and E-W railways use this as their reference point
      const int minDistToRails = 8;
      // precompute some important line segments of interest
      const uint N_NEUTRAL = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint E_NEUTRAL = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint S_NEUTRAL = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint W_NEUTRAL = (uint)Compass.XCOMlike.W * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      var layout = new Compass.LineGraph(geometry);
      var costs = new Dictionary<Rectangle,int>();

      foreach (Block mSurfaceBlock in m_SurfaceBlocks) {
        if (mSurfaceBlock.BuildingRect.Width > m_Params.MinBlockSize + 2) continue;
        if (mSurfaceBlock.BuildingRect.Height > m_Params.MinBlockSize + 2) continue;
        if (IsThereASpecialBuilding(entryMap, mSurfaceBlock.InsideRect)) continue;
        // unclear whether this scales with turns per hour.
        // If anything, at high magnifications we may need to not be "too far" from the rails either
        // old test failed for subway.Width/2-1-minDistToRails to subway.Width/2+2+minDistToRails
        // at district size 50: railY 24, upper bound 27; 38 should pass

        // To trigger critical-y: E_NEUTRAL or W_NEUTRAL line segments relevant
        // To trigger critical-x: N_NEUTRAL or S_NEUTRAL line segments relevant
        bool want_critical_Y = false;
        bool want_critical_X = false;
        if (layout.ContainsLineSegment(E_NEUTRAL) && mSurfaceBlock.BuildingRect.Right >= rail.X) want_critical_Y = true;
        if (layout.ContainsLineSegment(W_NEUTRAL) && mSurfaceBlock.BuildingRect.X < rail.X + height) want_critical_Y = true;
        if (layout.ContainsLineSegment(N_NEUTRAL) && mSurfaceBlock.BuildingRect.Y < rail.Y + height) want_critical_X = true;
        if (layout.ContainsLineSegment(S_NEUTRAL) && mSurfaceBlock.BuildingRect.Bottom >= rail.Y) want_critical_X = true;
        if (!want_critical_X && !want_critical_Y) continue;

        // we want a simple interval-does-not-intersect test
        if (   want_critical_Y
            && mSurfaceBlock.Rectangle.Top - minDistToRails <= rail.Y-1+height  // top below critical y
            && mSurfaceBlock.Rectangle.Bottom + minDistToRails-1 >= rail.Y) continue;   // bottom above critical y
        if (   want_critical_X
            && mSurfaceBlock.Rectangle.Left - minDistToRails <= rail.X-1+height  // left below critical x
            && mSurfaceBlock.Rectangle.Right + minDistToRails-1 >= rail.X) continue;   // right above critical x
        (blockList ??= new List<Block>(m_SurfaceBlocks.Count)).Add(mSurfaceBlock);
        Point exitPosition = mSurfaceBlock.InsideRect.Anchor(Compass.XCOMlike.N);
        if (want_critical_Y) {
          if (mSurfaceBlock.Rectangle.Bottom < rail.Y) costs[mSurfaceBlock.Rectangle] = rail.Y- exitPosition.Y;
          else if (mSurfaceBlock.Rectangle.Top > rail.Y+(height-1)) costs[mSurfaceBlock.Rectangle] = exitPosition.Y - (rail.Y + (height - 1));
        }
        if (want_critical_X) {
          if (want_critical_Y) {
            if (mSurfaceBlock.Rectangle.Right < rail.X) costs[mSurfaceBlock.Rectangle] = Math.Min(costs[mSurfaceBlock.Rectangle], rail.X- exitPosition.X);
            else if (mSurfaceBlock.Rectangle.Left > rail.X+(height-1)) costs[mSurfaceBlock.Rectangle] = Math.Min(costs[mSurfaceBlock.Rectangle], exitPosition.X - (rail.X + (height - 1)));
          } else {
            if (mSurfaceBlock.Rectangle.Right < rail.X) costs[mSurfaceBlock.Rectangle] = rail.X- exitPosition.X;
            else if (mSurfaceBlock.Rectangle.Left > rail.X+(height-1)) costs[mSurfaceBlock.Rectangle] = exitPosition.X - (rail.X + (height - 1));
          }
        }
//      break;
      }
      if (1 >= (blockList?.Count ?? 0)) return blockList;
      // further postprocessing here -- would like to minimize distance to rails *from the stairs*
      // Block is a class (extra work for dictionary), but Rectangle is a struct (and thus ok)
      var tmp = new List<Block>();
      int min_cost = costs.Values.Min();
      foreach(var x in blockList) {
        if (costs[x.Rectangle]==min_cost) tmp.Add(x);
      }
      if (0 < tmp.Count) return tmp;
      return blockList;
    }

    // \todo ultimately we'd like a proper subway network (this is just the EW line)
    // would also need: NS line, T-junctions, a 4-way junction at the center/default starting district, and diagonal bridges
    public Map GenerateSubwayMap(int seed, Map entryMap, out Block block)
    {
      block = null;
      District district = entryMap.District;
      uint layout = Session.Get.World.SubwayLayout(district.WorldPosition);
#if DEBUG
      if (0 >= layout) throw new InvalidOperationException("0 >= layout");
#endif
      var geometry = new Compass.LineGraph(layout);
      m_DiceRoller = new DiceRoller(seed);
      Map subway = new Map(seed, string.Format("Subway@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y), district, entryMap.Width, entryMap.Height, Lighting.DARKNESS);
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
      // reserved coordinates are y1 to y1+3 inclusive, so subway.Width/2-1 to subway.Width/2+2
      Point rail = SubwayRail(district);  // both the N-S and E-W railways use this as their reference point
      const int height = 4;

      // precompute some important line segments of interest
      const uint N_S = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint E_W = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint N_NEUTRAL = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint E_NEUTRAL = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint S_NEUTRAL = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint W_NEUTRAL = (uint)Compass.XCOMlike.W * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.reference.NEUTRAL;
      const uint N_E = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.E;
      const uint N_W = (uint)Compass.XCOMlike.N * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;
      const uint S_E = (uint)Compass.XCOMlike.E * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.S;
      const uint S_W = (uint)Compass.XCOMlike.S * (uint)Compass.reference.XCOM_EXT_STRICT_UB + (uint)Compass.XCOMlike.W;

      // layout logic: E-W more important than N-S
      // the neutral segments are less important than the full-length segments E-W, N-S
      // storage room is adjacent to rails
      void PoliceEnlightenment(Point pt) { Session.Get.ForcePoliceKnown(new Location(subway, pt)); }
      void lay_NS_rail(Point pt) { subway.SetTileModelAt(pt, GameTiles.RAIL_NS); }
      void lay_EW_rail(Point pt) { subway.SetTileModelAt(pt, GameTiles.RAIL_EW); }
      var toolroom_superposition = new HashSet<Rectangle>();   // historically: m_DiceRoller.Roll(10, subway.Width - 10) for anchor point center
      const int toolsRoomWidth = 5; // these should be odd numbers to allow visual centering on-grid for the door
      const int toolsRoomHeight = 5;
      void add_toolroomEWofNS(int y)
      {
        if (10 > y) return;
        if (subway.Height-10 < y) return;
        toolroom_superposition.Add(new Rectangle(rail.X - toolsRoomWidth, y- toolsRoomHeight / 2, 5,5));
        toolroom_superposition.Add(new Rectangle(rail.X+height, y- toolsRoomHeight / 2, 5,5));
      }
      void add_toolroomNSofEW(int x)
      {
        if (10 > x) return;
        if (subway.Width-10 < x) return;
        toolroom_superposition.Add(new Rectangle(x - toolsRoomWidth / 2, rail.Y - toolsRoomHeight, 5,5));
        toolroom_superposition.Add(new Rectangle(x - toolsRoomWidth / 2, rail.Y+height,5,5));
      }

      bool have_NS = false;
      bool have_EW = false;
      if (geometry.ContainsLineSegment(N_S)) {
        have_NS = true;
        Rectangle tmp = new Rectangle(rail.X, 0, height, subway.Height); // start as rails
        DoForEachTile(tmp, lay_NS_rail);
        subway.AddZone(MakeUniqueZone("rails", tmp));
        DoForEachTile(new Rectangle(rail.X-1, 0, height+2, subway.Height), PoliceEnlightenment);
        foreach(int y in Enumerable.Range(10, subway.Height-20)) add_toolroomEWofNS(y);
      }
      if (geometry.ContainsLineSegment(E_W)) {
        have_EW = true;
        Rectangle tmp = new Rectangle(0, rail.Y, subway.Width, height); // start as rails
        DoForEachTile(tmp, lay_EW_rail);
        subway.AddZone(MakeUniqueZone("rails", tmp));
        DoForEachTile(new Rectangle(0, rail.Y-1, subway.Width, height+2), PoliceEnlightenment);
        foreach(int x in Enumerable.Range(10, subway.Width-20)) add_toolroomNSofEW(x);
      }
      if (have_EW && !have_NS) {
        if (geometry.ContainsLineSegment(N_NEUTRAL)) {
          Rectangle tmp = new Rectangle(rail.X, 0, height, rail.Y); // start as rails
          DoForEachTile(tmp, lay_NS_rail);
          subway.AddZone(MakeUniqueZone("rails", tmp));
          DoForEachTile(new Rectangle(rail.X-1, 0, height+2, rail.Y), PoliceEnlightenment);
          foreach(int y in Enumerable.Range(10, rail.Y- toolsRoomHeight / 2-10)) add_toolroomEWofNS(y);
        } else if (geometry.ContainsLineSegment(S_NEUTRAL)) {
          Rectangle tmp = new Rectangle(rail.X, rail.Y+height, height, subway.Height-(rail.Y + height)); // start as rails
          DoForEachTile(tmp, lay_NS_rail);
          subway.AddZone(MakeUniqueZone("rails", tmp));
          DoForEachTile(new Rectangle(rail.X-1, rail.Y + height, height+2, subway.Height-(rail.Y + height)), PoliceEnlightenment);
          foreach(int y in Enumerable.Range(rail.Y+height+ toolsRoomHeight / 2, subway.Height-10)) add_toolroomEWofNS(y);
        }
      }
      if (have_NS && !have_EW) {
        if (geometry.ContainsLineSegment(W_NEUTRAL)) {
          Rectangle tmp = new Rectangle(0, rail.Y, rail.X, height); // start as rails
          DoForEachTile(tmp, lay_EW_rail);
          subway.AddZone(MakeUniqueZone("rails", tmp));
          DoForEachTile(new Rectangle(0, rail.Y-1, rail.X, height+2), PoliceEnlightenment);
          foreach(int x in Enumerable.Range(10, rail.X- toolsRoomWidth / 2-10)) add_toolroomNSofEW(x);
        } else if (geometry.ContainsLineSegment(E_NEUTRAL)) {
          Rectangle tmp = new Rectangle(rail.X+height, rail.Y, subway.Width-(rail.X + height), height); // start as rails
          DoForEachTile(tmp, lay_EW_rail);
          subway.AddZone(MakeUniqueZone("rails", tmp));
          DoForEachTile(new Rectangle(rail.X + height, rail.Y-1, subway.Width-(rail.X + height), height+2), PoliceEnlightenment);
          foreach(int x in Enumerable.Range(rail.X+height+ toolsRoomWidth / 2, subway.Width-10)) add_toolroomNSofEW(x);
        }
      }
      // handle the four diagonals (more synthetic tiles needed
      // \todo tool room  candidates for these layouts IF it could be reached safely once the subway trains are in place
      void lay_NW_SE_rail(Point pt)
      {
        if (subway.IsInBounds(pt)) subway.SetTileModelAt(pt, GameTiles.RAIL_SENW_WALL_W);
        var pt2 = new Point(pt.X, pt.Y + height);
        if (subway.IsInBounds(pt2)) subway.SetTileModelAt(pt2, GameTiles.RAIL_SENW_WALL_E);
        foreach (int delta in Enumerable.Range(1, height-1)) {
          pt.Y++;
          if (subway.IsInBounds(pt)) subway.SetTileModelAt(pt, GameTiles.RAIL_SENW);
        }
      }
      void lay_NE_SW_rail(Point pt)
      {
        if (subway.IsInBounds(pt)) subway.SetTileModelAt(pt, GameTiles.RAIL_SWNE_WALL_E);
        var pt2 = new Point(pt.X, pt.Y + height);
        if (subway.IsInBounds(pt2)) subway.SetTileModelAt(pt2, GameTiles.RAIL_SWNE_WALL_W);
        foreach (int delta in Enumerable.Range(1, height-1)) {
          pt.Y++;
          if (subway.IsInBounds(pt)) subway.SetTileModelAt(pt, GameTiles.RAIL_SWNE);
        }
      }
      if (!have_NS && !have_EW) {
        if (geometry.ContainsLineSegment(N_E)) {	// N ok; E two too high i.e. wanted rail.Y as rail.Y-2
          for (int x = rail.X; x < subway.Width; x++) lay_NE_SW_rail(new Point(x,x-rail.X-height));
        } else if (geometry.ContainsLineSegment(S_W)) {	// W ok; slope 2 short of S endpoint i.e. wanted rail.Y as rail.Y-2
          for (int x = 0; subway.Height > rail.Y+x; x++) lay_NE_SW_rail(new Point(x, rail.Y + x));
        } else if (geometry.ContainsLineSegment(N_W)) {	// ok (rail.X==rail.Y as constructed)
          for (int x = 0; x <= rail.X - 1 +height; x++) lay_NW_SE_rail(new Point(x, rail.Y - 1 - x));
        } else if (geometry.ContainsLineSegment(S_E)) {	// ok (rail.X==rail.Y as constructed)
          for (int y = 0; subway.Width > rail.X + y; y++) lay_NW_SE_rail(new Point(rail.X+y,subway.Height-1-y));
        }
      }
#endregion

#region 2. Make station linked to surface.
      List<Block> blockList = GetSubwayStationBlocks(entryMap, layout);
      if (blockList != null) {
        block = m_DiceRoller.Choose(blockList);
        ClearRectangle(entryMap, block.BuildingRect);   // had been wiping out stores, etc as late-generation
        TileFill(entryMap, GameTiles.FLOOR_CONCRETE, block.BuildingRect);
        Rectangle die = block.Rectangle;
        m_SurfaceBlocks.RemoveAll(b=>b.Rectangle== die);
        Point exitPosition = block.InsideRect.Anchor(Compass.XCOMlike.N);
        Block b1 = new Block(block.Rectangle);  // tolerate these vacuous copies for now -- insulates class data from the called functions
        MakeSubwayStationBuilding(entryMap, true, b1, subway, exitPosition);
        Block b2 = new Block(block.Rectangle);
        MakeSubwayStationBuilding(subway, false, b2, entryMap, exitPosition);
      }
#endregion
#region 3.  Small tools room.
      // filter out not-valid choices
      {
      foreach(int x in Enumerable.Range(10 - toolsRoomWidth / 2, subway.Width - 10 + toolsRoomWidth / 2)) {
        Point pt = new Point(x, rail.Y - 1);
        if (subway.GetTileModelAt(pt).IsWalkable) toolroom_superposition.RemoveWhere(r => r.Contains(pt));
        pt = new Point(x, rail.Y + height);
        if (subway.GetTileModelAt(pt).IsWalkable) toolroom_superposition.RemoveWhere(r => r.Contains(pt));
      }
      foreach(int y in Enumerable.Range(10 - toolsRoomHeight / 2, subway.Height - 10 +toolsRoomHeight / 2)) {
        Point pt = new Point(rail.X - 1,y);
        if (subway.GetTileModelAt(pt).IsWalkable) toolroom_superposition.RemoveWhere(r => r.Contains(pt));
        pt = new Point(rail.X + height, y);
        if (subway.GetTileModelAt(pt).IsWalkable) toolroom_superposition.RemoveWhere(r => r.Contains(pt));
      }
      }

      while (0 < toolroom_superposition.Count) {
        Rectangle rect = m_DiceRoller.Choose(toolroom_superposition);
        var doors = new List<Point>();
        {
        var pt = rect.Anchor(Compass.XCOMlike.N);
        if (subway.GetTileModelAt(pt+Direction.N).IsWalkable) doors.Add(pt);
        pt = rect.Anchor(Compass.XCOMlike.S);
        if (subway.GetTileModelAt(pt+Direction.S).IsWalkable) doors.Add(pt);
        pt = rect.Anchor(Compass.XCOMlike.W);
        if (subway.GetTileModelAt(pt+Direction.W).IsWalkable) doors.Add(pt);
        pt = rect.Anchor(Compass.XCOMlike.E);
        if (subway.GetTileModelAt(pt+Direction.E).IsWalkable) doors.Add(pt);
        if (0 >= doors.Count) {
          toolroom_superposition.Remove(rect);
          continue;
        }
        }
        TileFill(subway, GameTiles.FLOOR_CONCRETE, rect);
        TileRectangle(subway, GameTiles.WALL_BRICK, rect);
        var door = m_DiceRoller.Choose(doors);
        PlaceDoor(subway, door, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
        subway.AddZone(MakeUniqueZone("tools room", rect));
        DoForEachTile(rect, pt => {
          if (!subway.IsWalkable(pt) || CountAdjWalls(subway, pt) == 0 || subway.AnyAdjacent<DoorWindow>(pt)) return;
          var shelf = MakeObjShelf();
          subway.PlaceAt(shelf, pt);
          shelf.Inventory.AddAll(MakeShopConstructionItem());
        });
        break;
      }
#endregion
#region 4. Tags & Posters almost everywhere.
      for (short x2 = 0; x2 < subway.Width; ++x2) {    // \todo convert this iteration to a stack point?
        for (short y2 = 0; y2 < subway.Height; ++y2) {
          Point pt = new Point(x2,y2);
          Tile tileAt = subway.GetTileAt(pt);
          if (!tileAt.Model.IsWalkable && CountAdjWalkables(subway, in pt) >= 2 && m_DiceRoller.RollChance(SUBWAY_TAGS_POSTERS_CHANCE)) {
            if (m_DiceRoller.RollChance(50)) tileAt.AddDecoration(m_DiceRoller.Choose(POSTERS));
            if (m_DiceRoller.RollChance(50)) tileAt.AddDecoration(m_DiceRoller.Choose(TAGS));
          }
        }
      }
#endregion

      // alpha10
      // 6. Music.
      subway.BgMusic = GameMusics.SUBWAY;

      // not practical to do this piecewise
      subway.Rect.DoForEach(pt => {
          if (subway.GetTileModelAt(pt).IsWalkable) Session.Get.ForcePoliceKnown(new Location(subway, pt));
          else if (subway.HasAnyAdjacentInMap(pt,pt2 => subway.GetTileModelAt(pt2).IsWalkable)) Session.Get.ForcePoliceKnown(new Location(subway, pt));
      });

      return subway;
    }

    // main map block list.
    // in practice, m_Params.MinBlockSize is constant 11.  For this value:
#if ANALYSIS
      int railY = subway.Width / 2 - 1; // XXX fortunately width=height
      const int height = 4;
      Rectangle tmp = new Rectangle(0, railY, subway.Width, height); // start as rails
      DoForEachTile(tmp, (Action<Point>)(pt => { subway.SetTileModelAt(pt.X, pt.Y, GameTiles.RAIL_EW); }));
      subway.AddZone(MakeUniqueZone("rails", tmp));

        const int minDistToRails = 8;
        bool flag = false;
        // old test failed for subway.Width/2-1-minDistToRails to subway.Width/2+2+minDistToRails
        // at district size 50: railY 24, upper bound 27; 38 should pass
        // we want a simple interval-does-not-intersect test
        if (mSurfaceBlock.Rectangle.Top - minDistToRails > railY-1+height) flag = true;  // top below critical y
        if (mSurfaceBlock.Rectangle.Bottom + minDistToRails-1 < railY) flag = true;   // bottom above critical y
#endif
    // district size 30: raw split range 10..19; can fail to split immediately.  railY=14; tolerances 7, 25 (-7,+11); subway entrances impossible.
    // district size 40: raw split range 13..26. railY=19; tolerances 12, 30 (very difficult)
    // district size 50: raw split range 16..33. railY=25; tolerances 17,35 (moderately difficult)
    private void MakeBlocks(Map map, bool makeRoads, ref List<Block> list, Rectangle rect)
    {
      QuadSplit(rect, m_Params.MinBlockSize + 1, m_Params.MinBlockSize + 1, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty) {
        if (makeRoads) {
          MakeRoad(map, GameTiles.ROAD_ASPHALT_EW, new Rectangle(rect.Left, rect.Top, rect.Width, 1));
          MakeRoad(map, GameTiles.ROAD_ASPHALT_EW, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1));
          MakeRoad(map, GameTiles.ROAD_ASPHALT_NS, new Rectangle(rect.Left, rect.Top, 1, rect.Height));
          MakeRoad(map, GameTiles.ROAD_ASPHALT_NS, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height));
          topLeft.Location += Direction.SE;
          topLeft.Size += 2*Direction.NW;
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

    static private bool IsThereASpecialBuilding(Map map, Rectangle rect)
    {
      if (map.GetZonesAt(rect.Location)?.Any(zone=> zone.Name.Contains("Sewers Maintenance")
                                                 || zone.Name.Contains("Subway Station")
                                                 || zone.Name.Contains("office")
                                                 || zone.Name.Contains("shop")) ?? false)
        return true;
      return map.HasAnExitIn(rect); // relatively slow compared to above
    }

#if DEAD_FUNC
    // belongs in Map but required reference data is here.  Not guaranteed to remain static.
    // Could be constructed on game load as a nonserialized cache
    static public Zone InChokepointZone(Map m,Point pt)
    {
      var zoneList = m.GetZonesAt(pt);
      foreach(var z in zoneList) {
        foreach(var x in shop_name_images) {
          if (z.Name.Contains(x.Key)) return z;
        }
      }
      return null;
    }
#endif

    static public bool PoliceKnowAtGameStart(Map m,Point pt)
    {
      if (m.HasZonePartiallyNamedAt(pt, "Subway Station")) return true; // police guard starts in the subway station
      if (m.HasZonePartiallyNamedAt(pt, "Police Station")) return true;
      if (m.HasZonePartiallyNamedAt(pt, "CHAR Office")) return true;   // CHAR company town, police first assume things ok
      if (m.HasZonePartiallyNamedAt(pt, "CHAR Agency")) return true;   // CHAR company town, police first assume things ok
      // stores have their own police AI cheat
      foreach(var x in shop_name_images) {
        if (m.HasZonePartiallyNamedAt(pt, x.Key)) return true;
      }
      return false;
    }

      // \todo pull additional shop types from Staying Alive
      // Horticulture/gardening store
      // * grow lights require a working generator.
      // * bamboo can provide "wood".  At one foot per day (i.e. it actually *recovers* if only damaged, and can spread if planted in parks)
    static private readonly KeyValuePair<string, string>[] shop_name_images = new KeyValuePair<string, string>[] {
      new KeyValuePair<string,string>("GeneralStore", GameImages.DECO_SHOP_GENERAL_STORE),
      new KeyValuePair<string,string>("Grocery", GameImages.DECO_SHOP_GROCERY),
      new KeyValuePair<string,string>("Sportswear", GameImages.DECO_SHOP_SPORTSWEAR),
      new KeyValuePair<string,string>("Pharmacy", GameImages.DECO_SHOP_PHARMACY),
      new KeyValuePair<string,string>("Construction", GameImages.DECO_SHOP_CONSTRUCTION),
      new KeyValuePair<string,string>("Gunshop", GameImages.DECO_SHOP_GUNSHOP),
      new KeyValuePair<string,string>("Hunting Shop", GameImages.DECO_SHOP_HUNTING)
    };

    protected virtual bool MakeShopBuilding(Map map, Block b)
    {
      if (b.InsideRect.Width < 5 || b.InsideRect.Height < 5) return false;
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_STONE, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_TILES, b.InsideRect, true);
      ShopType shopType = (ShopType)m_DiceRoller.Roll(0, (int)ShopType._COUNT);
      var left1 = b.InsideRect.Left;
      var top1 = b.InsideRect.Top;
      var right = b.InsideRect.Right;
      var bottom = b.InsideRect.Bottom;
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

      KeyValuePair<string,string> shop_name_image = shop_name_images[(int)shopType];
      DecorateOutsideWalls(map, b.BuildingRect, pt => {
        if (map.HasMapObjectAt(pt) || !map.AnyAdjacent<DoorWindow>(pt)) return null;
        return shop_name_image.Value;
      });

      if (m_DiceRoller.RollChance(SHOP_WINDOW_CHANCE)) {
        Point doorAt = b.BuildingRect.Anchor((Compass.XCOMlike)m_DiceRoller.Choose(Direction.COMPASS_4).Index);

        if (!map.GetTileModelAt(doorAt).IsWalkable) PlaceDoor(map, doorAt, GameTiles.FLOOR_TILES, MakeObjWindow()); // XXX loses 1/4th of windows to the shop door
      }
      if (shopType == ShopType.GUNSHOP) BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      ItemsDrop(map, b.InsideRect, pt => {
        var mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || MapObject.IDs.SHOP_SHELF != mapObjectAt.ID) return false;
        return m_DiceRoller.RollChance(m_Params.ItemInShopShelfChance);
      }, pt => MakeRandomShopItem(shopType));
      map.AddZone(MakeUniqueZone(shop_name_image.Key, b.BuildingRect));
      MakeWalkwayZones(map, b);
      DoForEachTile(b.BuildingRect, map, loc => Session.Get.ForcePoliceKnown(loc)); // XXX exceptionally cheating police AI
      if (m_DiceRoller.RollChance(SHOP_BASEMENT_CHANCE)) {
        int seed = map.Seed << 1 ^ shop_name_image.Key.GetHashCode();
        string name = "basement-" + shop_name_image.Key + string.Format("{0}{1}@{2}-{3}", (object)m_Params.District.WorldPosition.X, (object)m_Params.District.WorldPosition.Y, (object)(b.BuildingRect.Left + b.BuildingRect.Width / 2), (object)(b.BuildingRect.Top + b.BuildingRect.Height / 2));
        Rectangle rectangle = b.BuildingRect;
        int width = rectangle.Width;
        int height = rectangle.Height;
        Map shopBasement = new Map(seed, name, map.District, width, height, Lighting.DARKNESS);
        TileFill(shopBasement, GameTiles.FLOOR_CONCRETE, true);
        TileRectangle(shopBasement, GameTiles.WALL_BRICK, shopBasement.Rect);
        shopBasement.AddZone(MakeUniqueZone("basement", shopBasement.Rect));
        DoForEachTile(shopBasement.Rect, (Action<Point>) (pt =>
        {
          Session.Get.Police.Investigate.Record(shopBasement, in pt);
          if (!shopBasement.IsWalkable(pt) || shopBasement.HasExitAt(in pt)) return;
          if (m_DiceRoller.RollChance(SHOP_BASEMENT_SHELF_CHANCE_PER_TILE)) {
            shopBasement.PlaceAt(MakeObjShelf(), pt);
            if (m_DiceRoller.RollChance(SHOP_BASEMENT_ITEM_CHANCE_PER_SHELF)) {
              Session.Get.Police.Investigate.Record(shopBasement, in pt);
              MakeRandomShopItem(shopType)?.DropAt(shopBasement, in pt);
            }
          }
          if (!Session.Get.HasZombiesInBasements || !m_DiceRoller.RollChance(SHOP_BASEMENT_ZOMBIE_RAT_CHANCE)) return;
          shopBasement.PlaceAt(CreateNewBasementRatZombie(0), in pt);
        }));

        // alpha10 music
        shopBasement.BgMusic = GameMusics.SEWERS;

        Point basementCorner = new Point((m_DiceRoller.RollChance(50) ? 1 : shopBasement.Width - 2),(m_DiceRoller.RollChance(50) ? 1 : shopBasement.Height - 2));
        rectangle = b.InsideRect;
        Point shopCorner = basementCorner + rectangle.Location + Direction.NW;
        AddExit(shopBasement, basementCorner, map, shopCorner, GameImages.DECO_STAIRS_UP);
        AddExit(map, shopCorner, shopBasement, basementCorner, GameImages.DECO_STAIRS_DOWN);

        if (!map.HasMapObjectAt(shopCorner)) map.RemoveMapObjectAt(shopCorner);

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
#if DEBUG
      if (null == make_door) throw new ArgumentNullException(nameof(make_door));
#endif
      bool orientation_ew = b.InsideRect.Width >= b.InsideRect.Height;
      Direction ret;
      if (orientation_ew) {
        ret = m_DiceRoller.RollChance(50) ? Direction.W : Direction.E;
      } else {
        ret = m_DiceRoller.RollChance(50) ? Direction.N : Direction.S;
      }
      Point doorAt = b.BuildingRect.Anchor((Compass.XCOMlike)ret.Index);
      PlaceDoor(map, doorAt, model, make_door());
      if (orientation_ew) {
        if (8 > b.InsideRect.Height) return ret;
        PlaceDoor(map, doorAt + Direction.N, model, make_door());
        if (b.InsideRect.Height >= 12) PlaceDoor(map, doorAt + Direction.S, model, make_door());
      } else {
        if (8 > b.InsideRect.Width) return ret;
        PlaceDoor(map, doorAt+Direction.W, model, make_door());
        if (b.InsideRect.Width >= 12) PlaceDoor(map, doorAt + Direction.E, model, make_door());
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
      DecorateOutsideWalls(map, b.BuildingRect, pt => {
        if (map.HasMapObjectAt(pt) || !map.AnyAdjacent<DoorWindow>(pt)) return null;
        return GameImages.DECO_CHAR_OFFICE;
      });
      MapObjectFill(map, b.InsideRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt) < 3) return null;
        return MakeObjChair(GameImages.OBJ_CHAR_CHAIR);
      }));
      TileFill(map, GameTiles.WALL_CHAR_OFFICE, new Rectangle(b.InsideRect.Location + b.InsideRect.Size / 2 + Direction.NW, 3, 2), (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration(m_DiceRoller.Choose(CHAR_POSTERS))));
      DecorateOutsideWalls(map, b.BuildingRect, pt => {
        if (map.AnyAdjacent<DoorWindow>(pt)) return null;
        if (m_DiceRoller.RollChance(25)) return m_DiceRoller.Choose(CHAR_POSTERS);
        return null;
      });
      map.AddZone(MakeUniqueZone("CHAR Agency", b.BuildingRect));
      MakeWalkwayZones(map, b);
      return true;
    }

    private void PopulateCHAROfficeBuilding(Map map, Point[] locs)
    {
      var squad = new List<Actor>();
      for (int index = 0; index < MAX_CHAR_GUARDS_PER_OFFICE; ++index) {
        var guard = CreateNewCHARGuard(0);
        map.PlaceAt(guard, in locs[index]); // do not use the ActorPlace function as we have pre-arranged the conditions when initializing the locs array
        squad.Add(guard);
      }
      Gameplay.AI.CHARGuardAI.DeclareSquad(squad);
    }

    private bool MakeCHAROffice(Map map, Block b)
    {
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_OFFICE, b.InsideRect, true);
      bool orientation_ew = b.InsideRect.Width >= b.InsideRect.Height;  // must agree with copy in PlaceShoplikeEntrance
      Direction direction = PlaceShoplikeEntrance(map, b, GameTiles.FLOOR_WALKWAY, MakeObjGlassDoor);
      Direction orthogonal = direction.Left.Left;
      DecorateOutsideWalls(map, b.BuildingRect, pt => {
        if (map.HasMapObjectAt(pt) || !map.AnyAdjacent<DoorWindow>(pt)) return null;
        return GameImages.DECO_CHAR_OFFICE;
      });
      BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);

      Point[] CHAR_guard_locs = new Point[MAX_CHAR_GUARDS_PER_OFFICE];
      Point tmp = b.InsideRect.Anchor((Compass.XCOMlike)direction.Index);
      CHAR_guard_locs[1] = tmp + orthogonal;
      CHAR_guard_locs[2] = tmp - orthogonal;
      CHAR_guard_locs[0] = tmp - 2*direction;
      Point chokepoint_door_pos = CHAR_guard_locs[0] - direction;
      if (direction == Direction.N) {
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(b.InsideRect.Left, chokepoint_door_pos.Y, b.InsideRect.Width, b.InsideRect.Height-3)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
        map.AddZone(new Zone("Foyer", new Rectangle(b.InsideRect.Left, b.BuildingRect.Top, b.InsideRect.Width, 4)));
        map.AddZone(new Zone("Hallway", new Rectangle(chokepoint_door_pos.X, b.BuildingRect.Top - 3, 1, b.InsideRect.Height-3)));
      } else if (direction == Direction.S) {
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width, b.InsideRect.Height-3)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
        map.AddZone(new Zone("Foyer", new Rectangle(b.InsideRect.Left, chokepoint_door_pos.Y + 1, b.InsideRect.Width, 4)));
        map.AddZone(new Zone("Hallway", new Rectangle(chokepoint_door_pos.X, b.BuildingRect.Top, 1, b.InsideRect.Height - 3)));
      } else if (direction == Direction.E) {
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(b.InsideRect.Left, b.InsideRect.Top, b.InsideRect.Width-3, b.InsideRect.Height)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
        map.AddZone(new Zone("Foyer", new Rectangle(chokepoint_door_pos.X + 1, b.InsideRect.Top, 4, b.InsideRect.Height)));
        map.AddZone(new Zone("Hallway", new Rectangle(b.InsideRect.Left, chokepoint_door_pos.Y, b.InsideRect.Width-3, 1)));
#if DEBUG
      } else if (direction == Direction.W) {
#else
      } else {
#endif
        map.AddZone(new Zone("NoCivSpawn", new Rectangle(chokepoint_door_pos.X, b.InsideRect.Top, b.InsideRect.Width-3, b.InsideRect.Height)));  // once the normal locks go in civilians won't be able to path here; one of these for each direction
        map.AddZone(new Zone("Foyer", new Rectangle(chokepoint_door_pos.X, b.InsideRect.Top, 4, b.InsideRect.Height)));
        map.AddZone(new Zone("Hallway", new Rectangle(b.InsideRect.Left + 3, chokepoint_door_pos.Y, b.InsideRect.Width - 3, 1)));
      }
#if DEBUG
      else throw new InvalidOperationException("unhandled door side");
#endif

      if (orientation_ew) TileVLine(map, GameTiles.WALL_CHAR_OFFICE, chokepoint_door_pos.X, b.InsideRect.Top, b.InsideRect.Height);
      else TileHLine(map, GameTiles.WALL_CHAR_OFFICE, b.InsideRect.Left, chokepoint_door_pos.Y, b.InsideRect.Width);

      var midpoint = b.Rectangle.Location + b.Rectangle.Size/2;
      Rectangle restricted_zone;
      if (direction == Direction.N) {
        restricted_zone = new Rectangle(midpoint.X - 1, chokepoint_door_pos.Y, 3, b.BuildingRect.Height - 1 - 3);
      } else if (direction == Direction.S) {
        restricted_zone = new Rectangle(midpoint.X - 1, b.BuildingRect.Top, 3, b.BuildingRect.Height - 1 - 3);
      } else if (direction == Direction.E) {
        restricted_zone = new Rectangle(b.BuildingRect.Left, midpoint.Y - 1, b.BuildingRect.Width - 1 - 3, 3);
#if DEBUG
      } else if (direction == Direction.W) {
#else
      } else {
#endif
        restricted_zone = new Rectangle(chokepoint_door_pos.X, midpoint.Y - 1, b.BuildingRect.Width - 1 - 3, 3);
      }
#if DEBUG
      else throw new InvalidOperationException("unhandled door side");
#endif

      TileRectangle(map, GameTiles.WALL_CHAR_OFFICE, restricted_zone);

      // \todo arrange for this door to be mechanically locked
      PlaceDoor(map, chokepoint_door_pos, GameTiles.FLOOR_OFFICE, MakeObjCharDoor());

      Rectangle rect2;
      Rectangle rect3;
      if (orientation_ew) {
        var left = restricted_zone.Left;
        var top = b.BuildingRect.Top;
        var width = restricted_zone.Width;
        rect2 = new Rectangle(left, top, width, midpoint.Y - top);
        rect3 = new Rectangle(left, midpoint.Y + 1, width, b.BuildingRect.Bottom - midpoint.Y -1);
      } else {
        var left = b.BuildingRect.Left;
        var top = restricted_zone.Top;
        var height = restricted_zone.Height;
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
          PlaceDoor(map, rectangle2.Anchor(Compass.XCOMlike.S), GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
        else
          PlaceDoor(map, rectangle2.Anchor(Compass.XCOMlike.E), GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      foreach (Rectangle rectangle2 in list2) {
        if (orientation_ew)
          PlaceDoor(map, rectangle2.Anchor(Compass.XCOMlike.N), GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
        else
          PlaceDoor(map, rectangle2.Anchor(Compass.XCOMlike.W), GameTiles.FLOOR_OFFICE, MakeObjCharDoor());
      }
      var table_pos = new List<Point>(rectangleList.Count);
      var chair_pos = new List<Point>(8);
      var chairs_pos = new List<KeyValuePair<Point,Point>>(28);
      foreach (Rectangle rectangle2 in rectangleList) {
        Point tablePos = rectangle2.Location + rectangle2.Size/2;
        map.PlaceAt(MakeObjTable(GameImages.OBJ_CHAR_TABLE), tablePos);
        table_pos.Add(tablePos);
        Rectangle rect4 = new Rectangle(rectangle2.Location + Direction.SE, rectangle2.Size + 2 * Direction.NW);
        if (rect4.IsEmpty) continue;
        Rectangle rect5 = new Rectangle(tablePos + Direction.NW, 3, 3);
        rect5.Intersect(rect4);
        rect5.DoForEach(pt=>chair_pos.Add(pt),pt=> map.GetTileModelAt(pt).IsWalkable && !map.HasMapObjectAt(pt));    // table is already placed
        if (2 >= chair_pos.Count) {
          foreach(Point pt in chair_pos) MakeObjChair(GameImages.OBJ_CHAR_CHAIR)?.PlaceAt(map, in pt);
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
      zone.Attribute.Set<bool>("CHAR Office", true);
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
      MapObjectFill(map, b.InsideRect, pt => (m_DiceRoller.RollChance(PARK_TREE_CHANCE) ? MakeObjTree() : null));
      MapObjectFill(map, b.InsideRect, pt => (m_DiceRoller.RollChance(PARK_BENCH_CHANCE) ? MakeObjBench() : null));
      Point entranceAt = b.BuildingRect.Anchor((Compass.XCOMlike)m_DiceRoller.Choose(Direction.COMPASS_4).Index);
      map.RemoveMapObjectAt(entranceAt);
      map.SetTileModelAt(entranceAt, GameTiles.FLOOR_WALKWAY);
      ItemsDrop(map, b.InsideRect, pt => {
        if (!map.HasMapObjectAt(pt)) return m_DiceRoller.RollChance(PARK_ITEM_CHANCE);
        return false;
      }, pt => MakeRandomParkItem());
      map.AddZone(MakeUniqueZone("Park", b.BuildingRect));
      MakeWalkwayZones(map, b);

      // alpha10: park shed
      if (b.InsideRect.Width > PARK_SHED_WIDTH+2 && b.InsideRect.Height > PARK_SHED_HEIGHT+2) {
        if (m_DiceRoller.RollChance(PARK_SHED_CHANCE)) {
           // roll shed pos - dont put next to park fences!
           var range = new Rectangle(b.InsideRect.Location+Direction.SE, b.InsideRect.Size-new Point(PARK_SHED_WIDTH + 2, PARK_SHED_HEIGHT + 2));
           Rectangle shedRect = new Rectangle(m_DiceRoller.Choose(range), PARK_SHED_WIDTH, PARK_SHED_HEIGHT);
           ClearRectangle(map, shedRect, false);
           MakeParkShedBuilding(map, "Shed", shedRect);
        }
      }

      return true;
    }

    protected virtual void MakeParkShedBuilding(Map map, string baseZoneName, Rectangle shedBuildingRect)
    {
      Rectangle shedInsideRect = new Rectangle(shedBuildingRect.Location + Direction.SE, shedBuildingRect.Size + 2 * Direction.NW);

      // build building & zone
      TileRectangle(map, GameTiles.WALL_BRICK, shedBuildingRect);
      TileFill(map, GameTiles.FLOOR_PLANKS, shedInsideRect, true);
      map.AddZone(MakeUniqueZone(baseZoneName, shedBuildingRect));

      // place shed door and make sure door front is cleared of objects (trees).
      Direction doorDir = m_DiceRoller.Choose(Direction.COMPASS_4);
      Point doorAt = shedBuildingRect.Anchor((Compass.XCOMlike)doorDir.Index);
      Point doorFront = doorAt+doorDir;
      PlaceDoor(map, doorAt, GameTiles.FLOOR_TILES, MakeObjWoodenDoor());
      map.RemoveMapObjectAt(doorFront);

      // mark as inside and add shelves with tools
      DoForEachTile(shedInsideRect, (pt) =>
      {
        if (!map.IsWalkable(pt)) return;
        if (0 < map.CountAdjacent<DoorWindow>(pt)) return;
        if (0 == CountAdjWalls(map, pt)) return;

        // shelf.
        var shelf = MakeObjShelf();
        map.PlaceAt(shelf, pt);

        // construction item (tools, lights)
        Item it = MakeShopConstructionItem();
        if (it.Model.IsStackable) it.Quantity = it.Model.StackingLimit;
        shelf.Inventory.AddAll(it);
        Session.Get.Police.Investigate.Record(map, in pt);
      });
    }

        // alpha10.1 makes apartements or vanilla house
        protected virtual bool MakeHousingBuilding(Map map, Block b)
        {
            // alpha10.1 decide floorplan
            // apartment?
            if (MakeApartmentsBuilding(map, b)) return true;

            // vanilla house?
            return MakeVanillaHousingBuilding(map, b);
        }

        // alpha10.1 apartment houses.  These do *not* have basements
        protected virtual bool MakeApartmentsBuilding(Map map, Block b)
        {
            ////////////////////////
            // 0. Check suitability
            ////////////////////////
            if (b.InsideRect.Width < 9 || b.InsideRect.Height < 9) return false;
            if (b.InsideRect.Width > 17 || b.InsideRect.Height > 17) return false;
            if (!m_DiceRoller.RollChance(HOUSE_IS_APARTMENTS_CHANCE)) return false; // only check RNG if dimensions ok

            // I pretty much copied and edited the char office algorithm. lame but i'm lazy.

            /////////////////////////////
            // 1. Walkway, floor & walls
            /////////////////////////////
            TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
            TileRectangle(map, GameTiles.WALL_BRICK, b.BuildingRect);
            TileFill(map, GameTiles.FLOOR_PLANKS, b.InsideRect, true);

            //////////////////////////
            // 2. Decide orientation.
            //////////////////////////          
            bool horizontalCorridor = (b.InsideRect.Width >= b.InsideRect.Height);

            /////////////////////////////////////
            // 3. Entry door and opposite window
            /////////////////////////////////////
            #region
            int midX = b.Rectangle.Left + b.Rectangle.Width / 2;
            int midY = b.Rectangle.Top + b.Rectangle.Height / 2;

            Direction doorSide = (horizontalCorridor) ? (m_DiceRoller.RollChance(50) ? Direction.W : Direction.E)
                                                      : (m_DiceRoller.RollChance(50) ? Direction.N : Direction.S);

            PlaceDoor(map, b.BuildingRect.Anchor((Compass.XCOMlike)(doorSide.Index)), GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());
            PlaceDoor(map, b.BuildingRect.Anchor((Compass.XCOMlike)((-doorSide).Index)), GameTiles.FLOOR_PLANKS, MakeObjWindow());
            #endregion

            //////////////////////////////////////////////
            // 4. Make central corridor & side apartments
            //////////////////////////////////////////////
            #region
            Rectangle corridorRect;
            if (doorSide == Direction.N)
                corridorRect = new Rectangle(midX, b.InsideRect.Top, 1, b.BuildingRect.Height - 1);
            else if (doorSide == Direction.S)
                corridorRect = new Rectangle(midX, b.BuildingRect.Top, 1, b.BuildingRect.Height - 1);
            else if (doorSide == Direction.E)
                corridorRect = new Rectangle(b.BuildingRect.Left, midY, b.BuildingRect.Width - 1, 1);
            else if (doorSide == Direction.W)
                corridorRect = new Rectangle(b.InsideRect.Left, midY, b.BuildingRect.Width - 1, 1);
            else
                throw new InvalidOperationException("apartment: unhandled door side");
            #endregion

            //////////////////////
            // 5. Make apartments
            //////////////////////
            #region
            // make wings.
            Rectangle wingOne;
            Rectangle wingTwo;
            if (horizontalCorridor)
            {
                // top side.
                wingOne = Rectangle.FromLTRB(b.BuildingRect.Left, b.BuildingRect.Top, b.BuildingRect.Right, corridorRect.Top);
                // bottom side.
                wingTwo = Rectangle.FromLTRB(b.BuildingRect.Left, corridorRect.Bottom, b.BuildingRect.Right, b.BuildingRect.Bottom);
            }
            else
            {
                // left side
                wingOne = Rectangle.FromLTRB(b.BuildingRect.Left, b.BuildingRect.Top, corridorRect.Left, b.BuildingRect.Bottom);
                // right side
                wingTwo = Rectangle.FromLTRB(corridorRect.Right, b.BuildingRect.Top, b.BuildingRect.Right, b.BuildingRect.Bottom);
            }

            // make apartements in each wing with doors leaving toward corridor and windows to the outside
            // pick sizes so the apartements are not cut into multiple rooms by MakeRoomsPlan
            int apartmentMinXSize, apartmentMinYSize;
            if (horizontalCorridor)
            {
                apartmentMinXSize = 4;
                apartmentMinYSize = b.BuildingRect.Height / 2;
            }
            else
            {
                apartmentMinXSize = b.BuildingRect.Width / 2;
                apartmentMinYSize = 4;
            }

            List<Rectangle> apartementsWingOne = new List<Rectangle>();
            MakeRoomsPlan(map, ref apartementsWingOne, wingOne, apartmentMinXSize, apartmentMinYSize);
            List<Rectangle> apartementsWingTwo = new List<Rectangle>();
            MakeRoomsPlan(map, ref apartementsWingTwo, wingTwo, apartmentMinXSize, apartmentMinYSize);

            List<Rectangle> allApartments = new List<Rectangle>(apartementsWingOne.Count + apartementsWingTwo.Count);
            allApartments.AddRange(apartementsWingOne);
            allApartments.AddRange(apartementsWingTwo);

            foreach (Rectangle apartRect in apartementsWingOne)
                TileRectangle(map, GameTiles.WALL_BRICK, apartRect);
            foreach (Rectangle roomRect in apartementsWingTwo)
                TileRectangle(map, GameTiles.WALL_BRICK, roomRect);

            // put door leading to corridor; and an opposite window if outer wall / a door if inside
            foreach (Rectangle apartRect in apartementsWingOne)
            {
                if (horizontalCorridor)
                {
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.S), GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.N), GameTiles.FLOOR_PLANKS, MakeObjWindow());
                }
                else
                {
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.E), GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.W), GameTiles.FLOOR_PLANKS, MakeObjWindow());
                }
            }
            foreach (Rectangle apartRect in apartementsWingTwo)
            {
                if (horizontalCorridor)
                {
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.N), GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.S), GameTiles.FLOOR_PLANKS, MakeObjWindow());
                }
                else
                {
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.W), GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());
                    PlaceDoor(map, apartRect.Anchor(Compass.XCOMlike.E), GameTiles.FLOOR_PLANKS, MakeObjWindow());
                }
            }

            // fill appartements with furniture and items
            // an "apartement" is one big room that fits all the housing roles: bedroom, kitchen and living room.
            foreach (Rectangle apartRect in allApartments)
            {
                // bedroom
                FillHousingRoomContents(map, apartRect, 0);
                // kitchen
                FillHousingRoomContents(map, apartRect, 8);
                // living room
                FillHousingRoomContents(map, apartRect, 5);
            }
            #endregion

            ///////////
            // 6. Zone
            ///////////
            Zone zone = MakeUniqueZone("Apartements", b.BuildingRect);
            map.AddZone(zone);
            MakeWalkwayZones(map, b);

            // done
            return true;
        }

    // alpha10.1 pre alpha10.1 regular houses
    protected virtual bool MakeVanillaHousingBuilding(Map map, Block b)
    {
      ////////////////////////
      // 0. Check suitability
      ////////////////////////
      if (b.InsideRect.Width < 4 || b.InsideRect.Height < 4) return false;

      /////////////////////////////
      // 1. Walkway, floor & walls
      /////////////////////////////
      TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_BRICK, b.BuildingRect);
      TileFill(map, GameTiles.FLOOR_PLANKS, b.InsideRect, true);

      ///////////////////////
      // 2. Rooms floor plan
      ///////////////////////
      var roomsList = new List<Rectangle>();
      MakeRoomsPlan(map, ref roomsList, b.BuildingRect, 5);

      /////////////////
      // 3. Make rooms
      /////////////////
      // alpha10 make some housings floor plan non rectangular by randomly chosing not to place one border room
      // and replace it with a special "outside" room : a garden, a parking lot.

      int iOutsideRoom = -1;
      HouseOutsideRoomType outsideRoom = HouseOutsideRoomType.GARDEN;
      if (roomsList.Count >= HOUSE_OUTSIDE_ROOM_NEED_MIN_ROOMS && m_DiceRoller.RollChance(HOUSE_OUTSIDE_ROOM_CHANCE)) {
        var outside_rooms = Enumerable.Range(0,roomsList.Count).Where(i => {
          Rectangle r = roomsList[i];
          return r.Left == b.BuildingRect.Left || r.Right == b.BuildingRect.Right || r.Top == b.BuildingRect.Top || r.Bottom == b.BuildingRect.Bottom;
        }).ToList();
        iOutsideRoom = m_DiceRoller.Choose(outside_rooms);
        outsideRoom = (HouseOutsideRoomType) m_DiceRoller.Roll(0, (int)HouseOutsideRoomType._COUNT);
      }

      for (int i = 0; i < roomsList.Count; i++) {
        Rectangle roomRect = roomsList[i];
        if (iOutsideRoom == i) {
          // make sure all tiles are marked as outside
          DoForEachTile(roomRect, (pt) => map.SetIsInsideAt(pt,false));

          // then shrink it properly so we dont overlap with tiles from other rooms and mess things up.
          if (roomRect.Left != b.BuildingRect.Left) {
            roomRect.X++;
            roomRect.Width--;
          }
          if (roomRect.Right != b.BuildingRect.Right)  roomRect.Width--;
          if (roomRect.Top != b.BuildingRect.Top) {
            roomRect.Y++;
            roomRect.Height--;
          }
          if (roomRect.Bottom != b.BuildingRect.Bottom) roomRect.Height--;

          // then fill the outside room
          switch (outsideRoom) {
            case HouseOutsideRoomType.GARDEN:
              TileFill(map, GameTiles.FLOOR_GRASS, roomRect);
              DoForEachTile(roomRect, (pos) => {
                if (map.GetTileModelAt(pos) == GameTiles.FLOOR_GRASS && m_DiceRoller.RollChance(HOUSE_GARDEN_TREE_CHANCE))
                  map.PlaceAt(MakeObjTree(), pos);
              });
              break;
            case HouseOutsideRoomType.PARKING_LOT:
              TileFill(map, GameTiles.FLOOR_ASPHALT, roomRect);
              DoForEachTile(roomRect, (pos) => {
                if (map.GetTileModelAt(pos) == GameTiles.FLOOR_ASPHALT && m_DiceRoller.RollChance(HOUSE_PARKING_LOT_CAR_CHANCE))
                  map.PlaceAt(MakeObjWreckedCar(m_DiceRoller), pos);
              });
              break;
            default: throw new InvalidOperationException("unhandled room type");
          }
        } else {
          MakeHousingRoom(map, roomRect, GameTiles.FLOOR_PLANKS, GameTiles.WALL_BRICK);
          FillHousingRoomContents(map, roomRect);
        }
      }

      // once all rooms are done, enclose the outside room
      if (-1 != iOutsideRoom) {
        Rectangle roomRect = roomsList[iOutsideRoom];
        switch (outsideRoom) {
          case HouseOutsideRoomType.GARDEN:
            DoForEachTile(roomRect, (pos) => {
              if (   (pos.X == roomRect.Left || pos.X == roomRect.Right - 1 || pos.Y == roomRect.Top || pos.Y == roomRect.Bottom - 1)
                  && map.GetTileModelAt(pos) == GameTiles.FLOOR_GRASS) {
                map.RemoveMapObjectAt(pos); // make sure trees are removed
                map.PlaceAt(MakeObjGardenFence(), pos);
              }
            });
            break;
          case HouseOutsideRoomType.PARKING_LOT:
            DoForEachTile(roomRect, (pos) => {
              bool isLotEntry = (pos.X == roomRect.Left + roomRect.Width / 2) || (pos.Y == roomRect.Top + roomRect.Height / 2);
              if (  !isLotEntry && ((pos.X == roomRect.Left || pos.X == roomRect.Right - 1 || pos.Y == roomRect.Top || pos.Y == roomRect.Bottom - 1)
                  && map.GetTileModelAt(pos) == GameTiles.FLOOR_ASPHALT)) {
                map.RemoveMapObjectAt(pos); // make sure cars are removed
                map.PlaceAt(MakeObjWireFence(), pos);
              }
            });
            break;
          default: throw new InvalidOperationException("unhandled room type");
        }
      }

      // XXX post-processing: converts inside windows to doors
      // backstop for a post-condition of MakeHousingRoom
      ///////////////////////////////////////
      // 5. Fix buildings with no door exits
      ///////////////////////////////////////
      bool hasOutsideDoor = false;
      var windows = new List<Point>();
      b.BuildingRect.DoForEach(pt => {
        if (map.IsInsideAt(pt)) return;
        if (!(map.GetMapObjectAt(pt) is DoorWindow obj)) return;
        if (obj.IsWindow) {
          windows.Add(pt);
          return;
        }
        hasOutsideDoor = true;
      });
      if (!hasOutsideDoor) {
        if (0 >= windows.Count) throw new InvalidOperationException("home w/o outside doors");
        var window_at = m_DiceRoller.Choose(windows);
        map.RemoveMapObjectAt(window_at);
        map.PlaceAt(MakeObjWoodenDoor(), window_at);
      }

      ////////////////
      // 6. Basement?
      ////////////////
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_CHANCE)) m_Params.District.AddUniqueMap(GenerateHouseBasementMap(map, b));

      ///////////
      // 7. Zone
      ///////////
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
      Direction direction = m_DiceRoller.Choose(Direction.COMPASS_4);   // \todo CHAR zoning
      Point doorAt = b.BuildingRect.Anchor((Compass.XCOMlike)direction.Index);
      Direction orthogonal = direction.Left.Left;
      map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, doorAt + orthogonal);
      map.AddDecorationAt(GameImages.DECO_SEWERS_BUILDING, doorAt - orthogonal);

      PlaceDoor(map, doorAt, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
      BarricadeDoors(map, b.BuildingRect, Rules.BARRICADING_MAX);
      AddExit(map, exitPosition, linkedMap, exitPosition, (isSurface ? GameImages.DECO_SEWER_HOLE : GameImages.DECO_SEWER_LADDER));
      if (!isSurface) {
        Point p = doorAt + direction;
        while (map.IsInBounds(p) && !map.GetTileModelAt(p).IsWalkable) {
          map.SetTileModelAt(p, GameTiles.FLOOR_CONCRETE);
          p += direction;
        }
      }
      int num = m_DiceRoller.Roll(Math.Max(b.InsideRect.Width, b.InsideRect.Height), 2 * Math.Max(b.InsideRect.Width, b.InsideRect.Height));
      for (int index = 0; index < num; ++index)
        MapObjectPlaceInGoodPosition(map, b.InsideRect, pt => {
          return CountAdjWalls(map, pt) >= 3 && !map.AnyAdjacent<DoorWindow>(pt);
        }, m_DiceRoller, pt => {
          map.DropItemAt(MakeShopConstructionItem(), in pt);
          Session.Get.Police.Investigate.Record(map, in pt);
          return MakeObjTable(GameImages.OBJ_TABLE);
        });
      if (m_DiceRoller.RollChance(33)) {
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt) >= 3 && !map.AnyAdjacent<DoorWindow>(pt);
        }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjBed(GameImages.OBJ_BED)));
        MapObjectPlaceInGoodPosition(map, b.InsideRect, (Func<Point, bool>) (pt =>
        {
          return CountAdjWalls(map, pt) >= 3 && !map.AnyAdjacent<DoorWindow>(pt);
        }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        {
          var fridge = MakeObjFridge();
          fridge.Inventory.AddAll(MakeItemCannedFood());
          Session.Get.Police.Investigate.Record(map, in pt);
          return fridge;
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
      if (isSurface) TileRectangle(map, GameTiles.FLOOR_WALKWAY, b.Rectangle);
      TileRectangle(map, GameTiles.WALL_SUBWAY, b.BuildingRect);
      DoForEachTile(b.BuildingRect, map, loc => {
          Session.Get.ForcePoliceKnown(loc);
          Session.Get.Police.Investigate.Seen(in loc);
          loc.Map.SetIsInsideAt(loc.Position);    // XXX this is a severe change -- combined with the early generation,
          // it guarantees the subway is inside when the NPCs are placed.  With late generation, it was possible to overwrite a park
      });
      const int height = 4;
      Point rail = SubwayRail(map.District);  // both the N-S and E-W railways use this as their reference point

      Direction direction = null;
      if (isSurface) direction = m_DiceRoller.Choose(Direction.COMPASS_4);  // \todo CHAR zoning codes -- should not be directly facing z invasion
      else {
        var options = new Dictionary<Compass.XCOMlike,int>();
        if (b.Rectangle.Bottom < map.Height / 2) {
          Point test = b.BuildingRect.Anchor(Compass.XCOMlike.S);
          if (map.IsWalkable(test.X,rail.Y)) options[Compass.XCOMlike.S] = test.Y-rail.Y;
        }
        if (b.Rectangle.Top > map.Height / 2) {
          Point test = b.BuildingRect.Anchor(Compass.XCOMlike.N);
          if (map.IsWalkable(test.X,rail.Y+height-1)) options[Compass.XCOMlike.N] = (rail.Y+height-1)-test.Y;
        }
        if (b.Rectangle.Right < map.Width / 2) {
          Point test = b.BuildingRect.Anchor(Compass.XCOMlike.E);
          if (map.IsWalkable(rail.X,test.Y)) options[Compass.XCOMlike.E] = test.X-rail.X;
        }
        if (b.Rectangle.Left > map.Width / 2) {
          Point test = b.BuildingRect.Anchor(Compass.XCOMlike.W);
          if (map.IsWalkable(rail.X+height-1, test.Y)) options[Compass.XCOMlike.W] = (rail.X+height-1)-test.X;
        }
        if (0==options.Count) throw new InvalidOperationException("subway station w/o candidate directions");
        options.OnlyIfMinimal();
        direction = Direction.COMPASS[(int)options.Keys.First()];
      }
      bool direction_ew = (2==direction.Index%4);
      Point doorAt = b.BuildingRect.Anchor((Compass.XCOMlike)direction.Index);
      Direction orthogonal = direction.Left.Left;
      switch(orthogonal.Index%4)
      {
      case 2:   // EW
        orthogonal = (doorAt.X > map.Width / 2) ? Direction.W : Direction.E;
        break;
      case 0:   // NS
        orthogonal = (doorAt.Y > map.Height / 2) ? Direction.N : Direction.S;
        break;
      }
      if (isSurface) {
        map.SetTileModelAt(doorAt, GameTiles.FLOOR_CONCRETE);
        map.PlaceAt(MakeObjGlassDoor(), doorAt);
        map.AddDecorationAt(GameImages.DECO_SUBWAY_BUILDING, doorAt + orthogonal);
        map.AddDecorationAt(GameImages.DECO_SUBWAY_BUILDING, doorAt - orthogonal);
      }
      for (int x2 = exitPosition.X - 1; x2 <= exitPosition.X + 1; ++x2) {
        Point point = new Point(x2, exitPosition.Y);
        AddExit(map, point, linkedMap, point, (isSurface ? GameImages.DECO_STAIRS_DOWN : GameImages.DECO_STAIRS_UP));
      }
      if (!isSurface) {
        Point p = doorAt;
        var x2orthogonal = 2 * orthogonal;
        while (map.IsInBounds(p) && !map.GetTileModelAt(p).IsWalkable) {
          map.SetTileModelAt(p, GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p + orthogonal, GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p - orthogonal, GameTiles.FLOOR_CONCRETE);
          map.SetTileModelAt(p - x2orthogonal, GameTiles.WALL_STONE);
          map.SetTileModelAt(p + x2orthogonal, GameTiles.WALL_STONE);
          DoForEachTile(new Rectangle(p+2*Direction.N, 5,1),pt => Session.Get.ForcePoliceKnown(new Location(map, pt)));
          p += direction;
        }
        Point centralGateAt = p - 4*direction;
        Rectangle corridor() {
          switch(direction.Index)
          {
          case (int)Compass.XCOMlike.N: return new Rectangle(doorAt.X - 1, centralGateAt.Y, 3, doorAt.Y - centralGateAt.Y + 1);
          case (int)Compass.XCOMlike.S: return new Rectangle(doorAt.X - 1, doorAt.Y, 3, centralGateAt.Y - doorAt.Y + 1);
          case (int)Compass.XCOMlike.W: return new Rectangle(centralGateAt.X, doorAt.Y - 1, doorAt.X - centralGateAt.X + 1, 3);
          case (int)Compass.XCOMlike.E: return new Rectangle(doorAt.X, doorAt.Y - 1, centralGateAt.X - doorAt.X + 1, 3);
          default: throw new InvalidOperationException("unhandled direction");
          }
        }

        map.AddZone(MakeUniqueZone("corridor", corridor()));

        Rectangle plat() {
          int left = Math.Max(0, b.BuildingRect.Left - 10);
          int right = Math.Min(map.Width - 1, b.BuildingRect.Right + 10);

          // bench layout doesn't look good flush against district boundary
          switch(direction.Index)
          {
          case (int)Compass.XCOMlike.N:
          case (int)Compass.XCOMlike.S:
            left = Math.Max(1, b.BuildingRect.Left - 10);
            right = Math.Min(map.Width - 2, b.BuildingRect.Right + 10);
            break;
          case (int)Compass.XCOMlike.W:
          case (int)Compass.XCOMlike.E:
            left = Math.Max(1, b.BuildingRect.Top - 10);
            right = Math.Min(map.Height - 2, b.BuildingRect.Bottom + 10);
            break;
          default: throw new InvalidOperationException("unhandled direction");
          }
          // don't cross rails with the platform
          // XXX will need revising if diagonal rails go in for t-intersections and 4-way
          switch(direction.Index)
          {
          case (int)Compass.XCOMlike.N:
            if (right > rail.X-2 && left <= rail.X-2 && map.IsWalkable(rail.X, p.Y + 1)) {
              right = rail.X -2;
              break;
            }
            if (right >= rail.X+height+1 && left < rail.X+height+1 && map.IsWalkable(rail.X+height-1, p.Y + 1)) {
              left = rail.X+height+1;
              break;
            }
            break;
          case (int)Compass.XCOMlike.S:
            if (right > rail.X-2 && left <= rail.X-2 && map.IsWalkable(rail.X, centralGateAt.Y + 1)) {
              right = rail.X -2;
              break;
            }
            if (right >= rail.X+height+1 && left < rail.X+height+1 && map.IsWalkable(rail.X+height-1, centralGateAt.Y + 1)) {
              left = rail.X+height+1;
              break;
            }
            break;
          case (int)Compass.XCOMlike.W:
            if (right > rail.Y-2 && left <= rail.Y-2 && map.IsWalkable(p.X + 1, rail.Y)) {
              right = rail.Y -2;
              break;
            }
            if (right >= rail.Y+height+1 && left < rail.Y+height+1 && map.IsWalkable(p.X + 1, rail.Y+height-1)) {
              left = rail.Y+height+1;
              break;
            }
            break;
          case (int)Compass.XCOMlike.E:
            if (right > rail.Y-2 && left <= rail.Y-2 && map.IsWalkable(centralGateAt.X + 1, rail.Y)) {
              right = rail.Y -2;
              break;
            }
            if (right >= rail.Y+height+1 && left < rail.Y+height+1 && map.IsWalkable(centralGateAt.X + 1, rail.Y+height-1)) {
              left = rail.Y+height+1;
              break;
            }
            break;
          default: throw new InvalidOperationException("unhandled direction");
          }
          switch(direction.Index)
          {
          case (int)Compass.XCOMlike.N: return new Rectangle(left, p.Y + 1, right - left, 3);
          case (int)Compass.XCOMlike.S: return new Rectangle(left, centralGateAt.Y + 1, right - left, 3);
          case (int)Compass.XCOMlike.W: return new Rectangle(p.X + 1, left, 3, right - left);
          case (int)Compass.XCOMlike.E: return new Rectangle(centralGateAt.X + 1, left, 3, right - left);
          default: throw new InvalidOperationException("unhandled direction");
          }
        }

        Rectangle platform = plat();
        TileFill(map, GameTiles.FLOOR_CONCRETE, platform);
        platform.Edge((Compass.XCOMlike)(-direction).Index).DoForEach(pt => map.PlaceAt(MakeObjIronBench(), pt),
            pt => (CountAdjWalls(map, pt) >= 3));
        DoForEachTile(platform, map, loc => Session.Get.ForcePoliceKnown(loc));
        map.AddZone(MakeUniqueZone("platform", platform));
        map.PlaceAt(MakeObjIronGate(), centralGateAt);
        map.PlaceAt(MakeObjIronGate(), centralGateAt + orthogonal);
        map.PlaceAt(MakeObjIronGate(), centralGateAt - orthogonal);
        Point point2 = doorAt+ x2orthogonal + 2*direction;

        Rectangle backup() {
          Rectangle ret = direction_ew ? new Rectangle(0,0,5,4) : new Rectangle(0,0,4,5);
          ret.Location = point2 + 2*(direction_ew ? Direction.W : Direction.N);
          switch(orthogonal.Index)
          {
          case (int)Compass.XCOMlike.W:
          case (int)Compass.XCOMlike.N:
            ret.Location += 3*orthogonal;
            break;
          }
          return ret;
        }

        Rectangle rect2 = backup();
        TileFill(map, GameTiles.FLOOR_CONCRETE, rect2);
        TileRectangle(map, GameTiles.WALL_STONE, rect2);
        PlaceDoor(map, point2, GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
        map.AddDecorationAt(GameImages.DECO_POWER_SIGN_BIG, point2+direction);
        map.AddDecorationAt(GameImages.DECO_POWER_SIGN_BIG, point2-direction);
        MapObjectFill(map, rect2, pt => {
          if (!map.GetTileModelAt(pt).IsWalkable) return null;
          if (CountAdjWalls(map, pt) < 3 || map.AnyAdjacent<DoorWindow>(pt)) return null;
          return MakeObjPowerGenerator();
        });
        DoForEachTile(rect2, map, loc => Session.Get.ForcePoliceKnown(loc));
      }
      for (var left = b.InsideRect.Left; left < b.InsideRect.Right; ++left) {
        for (int y = b.InsideRect.Top + 1; y < b.InsideRect.Bottom - 1; ++y) {
          var pt = new Point(left, y);
          if (CountAdjWalls(map, pt) >= 2 && !map.AnyAdjacent<DoorWindow>(pt) && !Rules.IsAdjacent(in pt, in doorAt))
            map.PlaceAt(MakeObjIronBench(), pt);
        }
      }
      if (isSurface) {
        Actor newPoliceman = CreateNewPoliceman(0);
        if (Session.Get.CMDoptionExists("subway-cop")) {
          var home_district_xy = Session.Get.World.Size;
          home_district_xy /= 2;
          if (map.DistrictPos == new Point(home_district_xy, home_district_xy)) newPoliceman.Controller = new PlayerController(newPoliceman);
        }
        ActorPlace(m_DiceRoller, map, newPoliceman, b.InsideRect);
      }
      map.AddZone(MakeUniqueZone("Subway Station", b.BuildingRect));
    }

    // alpha10.1 allow different x and y min size
    protected void MakeRoomsPlan(Map map, ref List<Rectangle> list, Rectangle rect, int minRoomsXSize, int minRoomsYSize=0)
    {
      if (0 >= minRoomsYSize) minRoomsYSize = minRoomsXSize;    // backward compatibility
      QuadSplit(rect, minRoomsXSize, minRoomsYSize, out Rectangle topLeft, out Rectangle topRight, out Rectangle bottomLeft, out Rectangle bottomRight);
      if (topRight.IsEmpty && bottomLeft.IsEmpty && bottomRight.IsEmpty) {
        list.Add(rect);
      } else {
        MakeRoomsPlan(map, ref list, topLeft, minRoomsXSize, minRoomsYSize);
        if (!topRight.IsEmpty) {
          topRight.Location += Direction.W;
          topRight.Size += Direction.E;
          MakeRoomsPlan(map, ref list, topRight, minRoomsXSize, minRoomsYSize);
        }
        if (!bottomLeft.IsEmpty) {
          bottomLeft.Location += Direction.N;
          bottomLeft.Size += Direction.S;
          MakeRoomsPlan(map, ref list, bottomLeft, minRoomsXSize, minRoomsYSize);
        }
        if (bottomRight.IsEmpty) return;
        bottomRight.Location += Direction.NW;
        bottomRight.Size += Direction.SE;
        MakeRoomsPlan(map, ref list, bottomRight, minRoomsXSize, minRoomsYSize);
      }
    }

    protected virtual void MakeHousingRoom(Map map, Rectangle roomRect, TileModel floor, TileModel wall)
    {
      TileFill(map, floor, roomRect);
      TileRectangle(map, wall, roomRect.Left, roomRect.Top, roomRect.Width, roomRect.Height, (tile, prevmodel, pt) =>
      {
        if (!map.HasMapObjectAt(pt)) return;
        map.SetTileModelAt(pt, floor);
      });
      bool door_window_ok(Point pt) { return !map.HasMapObjectAt(pt) && IsAccessible(map, in pt) && !map.AnyAdjacent<DoorWindow>(pt); }
      MapObject make_door_window(Point pt) { return ((!map.IsInsideAt(pt) && !m_DiceRoller.RollChance(25)) ? MakeObjWindow() : MakeObjWoodenDoor()); }

      foreach(var dir in Direction.COMPASS_4) PlaceIf(map, roomRect.Anchor((Compass.XCOMlike)dir.Index), floor, door_window_ok, make_door_window);
    }

    // alpha10.1 can force room role (optional param)
    // FIXME -- room role should be an enum and not hardcoded numbers -_-
    /// <param name="role">-1 roll at random; 0-4 bedroom, 5-7 living room, 8-9 kitchen</param>
    protected virtual void FillHousingRoomContents(Map map, Rectangle roomRect, int role = -1)
    {
      Rectangle insideRoom = new Rectangle(roomRect.Location + Direction.SE, roomRect.Size + 2 * Direction.NW);

      if (-1 == role) role = m_DiceRoller.Roll(0, 10);  // alpha10.1 roll room role if not set

      // alpha10.1 added restriction to not place a mapobj if adj to at least 5 mapobj as to not cramp apartements
      switch (role)
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
              return CountAdjWalls(map, pt) >= 3 && !map.AnyAdjacent<DoorWindow>(pt) && map.CountAdjacent<MapObject>(pt) < 5;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              Rectangle rect = new Rectangle(pt + Direction.NW, 3, 3);
              rect.Intersect(insideRoom);
              MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                return pt2 != pt && !map.AnyAdjacent<DoorWindow>(pt2) &&  CountAdjWalls(map, pt2) > 0 && map.CountAdjacent<MapObject>(pt2) < 5;
              }), m_DiceRoller, (Func<Point, MapObject>) (pt2 =>
              {
                map.DropItemAt(MakeRandomBedroomItem(), in pt2);
                Session.Get.Police.Investigate.Record(map, in pt2);
                return MakeObjNightTable(GameImages.OBJ_NIGHT_TABLE);
              }));
              return MakeObjBed(GameImages.OBJ_BED);
            }));
          int num2 = m_DiceRoller.Roll(1, 4);
          for (int index = 0; index < num2; ++index)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt) >= 2 && !map.AnyAdjacent<DoorWindow>(pt) && map.CountAdjacent<MapObject>(pt) < 5;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              var drawer = (m_DiceRoller.RollChance(50) ? MakeObjWardrobe(GameImages.OBJ_WARDROBE) : MakeObjDrawer());
              drawer.Inventory.AddAll(MakeRandomBedroomItem());
              Session.Get.Police.Investigate.Record(map, in pt);
              return drawer;
            }));
          break;
        case 5:
        case 6:
        case 7:
          int num3 = m_DiceRoller.Roll(1, 3);
          for (int index1 = 0; index1 < num3; ++index1)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt) == 0 &&  !map.AnyAdjacent<DoorWindow>(pt) && map.CountAdjacent<MapObject>(pt) < 5;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
            {
              for (int index = 0; index < HOUSE_LIVINGROOM_ITEMS_ON_TABLE; ++index) {
                map.DropItemAt(MakeRandomKitchenItem(), in pt);
              }
              Session.Get.Police.Investigate.Record(map, in pt);
              Rectangle rect = new Rectangle(pt + Direction.NW, 3, 3);
              rect.Intersect(insideRoom);
              MapObjectPlaceInGoodPosition(map, rect, (Func<Point, bool>) (pt2 =>
              {
                return pt2 != pt && !map.AnyAdjacent<DoorWindow>(pt2) && map.CountAdjacent<MapObject>(pt2) < 5;
              }), m_DiceRoller, pt2 => MakeObjChair(GameImages.OBJ_CHAIR));
              return MakeObjTable(GameImages.OBJ_TABLE);
            }));
          int num4 = m_DiceRoller.Roll(1, 3);
          for (int index = 0; index < num4; ++index)
            MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
            {
              return CountAdjWalls(map, pt) >= 2 && !map.AnyAdjacent<DoorWindow>(pt) && map.CountAdjacent<MapObject>(pt) < 5;
            }), m_DiceRoller, (Func<Point, MapObject>) (pt => MakeObjDrawer()));
          break;
        case 8:
        case 9:
          MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            return CountAdjWalls(map, pt) == 0 && !map.AnyAdjacent<DoorWindow>(pt) && map.CountAdjacent<MapObject>(pt) < 5;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_ON_TABLE; ++index) {
              map.DropItemAt(MakeRandomKitchenItem(), in pt);
            }
            Session.Get.Police.Investigate.Record(map, in pt);
            MapObjectPlaceInGoodPosition(map, new Rectangle(pt + Direction.NW, 3, 3), (Func<Point, bool>) (pt2 =>
            {
              return pt2 != pt && !map.AnyAdjacent<DoorWindow>(pt2) && map.CountAdjacent<MapObject>(pt2) < 5;
            }), m_DiceRoller, pt2 => MakeObjChair(GameImages.OBJ_CHAIR));
            return MakeObjTable(GameImages.OBJ_TABLE);
          }));
          MapObjectPlaceInGoodPosition(map, insideRoom, (Func<Point, bool>) (pt =>
          {
            return CountAdjWalls(map, pt) >= 2 && !map.AnyAdjacent<DoorWindow>(pt) && map.CountAdjacent<MapObject>(pt) < 5;
          }), m_DiceRoller, (Func<Point, MapObject>) (pt =>
          {
            var fridge = MakeObjFridge();
            for (int index = 0; index < HOUSE_KITCHEN_ITEMS_IN_FRIDGE; ++index) {
              fridge.Inventory.AddAll(MakeRandomKitchenItem());
            }
            Session.Get.Police.Investigate.Record(map, in pt);
            return fridge;
          }));
          break;
        default: throw new InvalidProgramException("unhandled roll");
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

    // hospital storeroom wants to simply maximize quantities, not randomize
    private Item PostprocessQuantity(Item it)   // relies on Item being a class rather than a struct
    {
      switch(it.Model.ID) {
      case GameItems.IDs.TRAP_SPIKES:
        it.Quantity = m_DiceRoller.Roll(1, GameItems.BARBED_WIRE.StackingLimit);  // XXX V.0.10.0 align?  RS Alpha 9 has this as well.
        break;
      //
      case GameItems.IDs.TRAP_BARBED_WIRE:
      case GameItems.IDs.MELEE_CROWBAR:
      case GameItems.IDs.FOOD_CANNED_FOOD:
      case GameItems.IDs.ENT_MAGAZINE:
      case GameItems.IDs.EXPLOSIVE_GRENADE:
      case GameItems.IDs.MEDICINE_BANDAGES:
      case GameItems.IDs.MEDICINE_PILLS_SLP:
      case GameItems.IDs.MEDICINE_PILLS_STA:
      case GameItems.IDs.MEDICINE_PILLS_SAN:
      case GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL:
        it.Quantity = m_DiceRoller.Roll(1, it.Model.StackingLimit);
        break;
      }
      return it;
    }

    private ItemFood MakeShopGroceryItem()
    {
      return (m_DiceRoller.RollChance(50) ? MakeItemCannedFood() : MakeItemGroceries());    // groceries duration requires weakening object orientation to post-process
    }

    // hospital and pharmacy use similar item lists
    private const int hospital_shop_checksum = 7;
    private readonly KeyValuePair<GameItems.IDs, int>[] hospital_shop_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_BANDAGES,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_MEDIKIT,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_SLP,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_STA,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_SAN,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SCENT_SPRAY_STENCH_KILLER,1),  // unclear why here rather than hunting shop
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL,1)   // not in pharmacy; requires infection
    };

    private Item MakeShopPharmacyItem()
    {
      return PostprocessQuantity(GameItems.From(hospital_shop_stock.UseRarityTable(m_DiceRoller.Roll(0, hospital_shop_checksum-1))).create());
    }

    // RS Alpha 9: hunting sports: 20%, non-contact sports 80%
    private const int sportswear_shop_checksum = 100;
    private readonly KeyValuePair<GameItems.IDs, int>[] sportswear_shop_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_RIFLE,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_RIFLE,7),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_CROSSBOW,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_BOLTS,7),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_BASEBALLBAT,40),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_IRON_GOLFCLUB,20),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_GOLFCLUB,20)
    };

    private Item MakeShopSportsWearItem()
    {
      return GameItems.From(sportswear_shop_stock.UseRarityTable(m_DiceRoller.Roll(0, sportswear_shop_checksum))).create();
    }

    // original was 1..24 in groups of 3, with some 50-50 splits
    private const int construction_shop_checksum = 48;
    private readonly KeyValuePair<GameItems.IDs, int>[] construction_shop_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_SHOVEL,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_SHORT_SHOVEL,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_CROWBAR,6),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_HUGE_HAMMER,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_SMALL_HAMMER,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.BAR_WOODEN_PLANK,6),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.LIGHT_FLASHLIGHT,6),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.LIGHT_BIG_FLASHLIGHT,6),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRAP_SPIKES,6),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRAP_BARBED_WIRE,6)
    };

    private Item MakeShopConstructionItem()
    {
      return PostprocessQuantity(GameItems.From(construction_shop_stock.UseRarityTable(m_DiceRoller.Roll(0, construction_shop_checksum))).create());
    }

    // RS Alpha 9: 40% ranged weapons, 60% ammo
    private const int gunshop_checksum = 100;
    private static readonly KeyValuePair<GameItems.IDs, int>[] gunshop_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_PISTOL,5),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_KOLT_REVOLVER,5),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_SHOTGUN,10),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_RIFLE,10),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_CROSSBOW,10),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_SHOTGUN,15),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_PISTOL,15),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_RIFLE,15),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_BOLTS,15)
    };

    private Item MakeShopGunshopItem()
    {
      return GameItems.From(gunshop_stock.UseRarityTable(m_DiceRoller.Roll(0, gunshop_checksum))).create();
    }

    // RS Alpha 9: 50% weapons, 50% other.  Why no stench killer?
    private const int hunting_shop_checksum = 40;
    private readonly KeyValuePair<GameItems.IDs, int>[] hunting_shop_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_RIFLE,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_CROSSBOW,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_RIFLE,7),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_BOLTS,7),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.ARMOR_HUNTER_VEST,10),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRAP_BEAR_TRAP,10)
    };

    private Item MakeHuntingShopItem()
    {
      return GameItems.From(hunting_shop_stock.UseRarityTable(m_DiceRoller.Roll(0, hunting_shop_checksum))).create();
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
      return PostprocessQuantity(GameItems.From(hospital_shop_stock.UseRarityTable(m_DiceRoller.Roll(0, (Session.Get.HasInfection ? hospital_shop_checksum : hospital_shop_checksum-1)))).create());
    }

//  This should be influenced by pre-apocalypse politics.  This is the reference red state, red city set the other politics post-process
    private const int bedroom_checksum = 480;
    private readonly KeyValuePair<GameItems.IDs, int>[] bedroom_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_BANDAGES,40),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_STA,20),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_SLP,20),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_SAN,20),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_BASEBALLBAT,80),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRACKER_CELL_PHONE,60),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.LIGHT_FLASHLIGHT,40),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SCENT_SPRAY_STENCH_KILLER,40),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.ARMOR_HUNTER_VEST,20),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.ENT_BOOK,30),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.ENT_MAGAZINE,30),
        // firearms must be civilian-legal for these to be allowed (for this purpose a crossbow is a low-noise firearm, but RS9 doesn't have crossbows as a bedroom item)
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_SHOTGUN,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_RIFLE,3),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_SHOTGUN,7),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_RIFLE,7),
        // concealed carry must be civilian-legal for these to be allowed
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_PISTOL,10),            // RS9: same weight for these due to function with 50/50 weight
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_KOLT_REVOLVER,10),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_PISTOL,40)
    };

    private Item MakeRandomBedroomItem()
    {
      return PostprocessQuantity(GameItems.From(bedroom_stock.UseRarityTable(m_DiceRoller.Roll(0, bedroom_checksum))).create());
    }

    private ItemFood MakeRandomKitchenItem()    // not obviously the same as grocery item
    {
      return (m_DiceRoller.RollChance(50) ? MakeItemCannedFood() : MakeItemGroceries());
    }

    // RS 9 CHAR office had a high rate of null return when creating items
    private const int CHAR_office_checksum = 450;
    private readonly KeyValuePair<GameItems.IDs, int>[] CHAR_office_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.EXPLOSIVE_GRENADE_PRIMED,10),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_SHOTGUN,27),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_SHOTGUN,63),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_BANDAGES,100),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_MEDIKIT,100),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.FOOD_CANNED_FOOD,100),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRACKER_ZTRACKER,25),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRACKER_BLACKOPS,25)
    };

    public Item MakeRandomCHAROfficeItem()
    {
      int choice = m_DiceRoller.Roll(0, CHAR_office_checksum/45*100);   // historically 45% chance of an item
      if (CHAR_office_checksum <= choice) return null;
      return PostprocessQuantity(GameItems.From(CHAR_office_stock.UseRarityTable(choice)).create());
    }

    private const int park_checksum = 32;
    private readonly KeyValuePair<GameItems.IDs, int>[] park_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT1,1),  // RS9: these four have same weight due to a function
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT2,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT3,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.SPRAY_PAINT4,1),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MELEE_BASEBALLBAT,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_SLP,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_STA,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.MEDICINE_PILLS_SAN,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.LIGHT_FLASHLIGHT,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRACKER_CELL_PHONE,4),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.BAR_WOODEN_PLANK,4)
    };

    public Item MakeRandomParkItem()
    {
      return PostprocessQuantity(GameItems.From(park_stock.UseRarityTable(m_DiceRoller.Roll(0, park_checksum))).create());
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
          basement.SetTileModelAt(diag_step, GameTiles.FLOOR_CONCRETE);
          basement.SetTileModelAt(large, GameTiles.FLOOR_CONCRETE);
        } else if (   GameTiles.WALL_BRICK == basement.GetTileModelAt(diag_step)
                   && GameTiles.FLOOR_CONCRETE == basement.GetTileModelAt(corner)) {
          basement.SetTileModelAt(corner, GameTiles.WALL_BRICK);
          basement.SetTileModelAt(diag_step, GameTiles.FLOOR_CONCRETE);
        }
      }
    }

    private bool _ForceHouseBasementConnected(Map basement,Point basementStairs)
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

      var inner = new Rectangle(Direction.SE.Vector,basement.Rect.Size+2*Direction.NW);   // \todo ? could precalculate this in constructor
restart:
      // anchor point is the top-left walkable tile being cut off
      var candidates = new HashSet<Point>();    // for being the center of a canonical 1x1 disconnect
#if MORE_AGGRESSIVE_CONNECTED_BASEMENTS
      var candidates_1x2 = new HashSet<Point>();    // width 1 height 2 disconnect
      var candidates_2x1 = new HashSet<Point>();    // width 2 height 1 disconnect
      inner.DoForEach(pt => {
          candidates.Add(pt);
          if (pt.X < basement.Rect.Width - 2) candidates_2x1.Add(pt);
          if (pt.Y < basement.Rect.Height - 2) candidates_1x2.Add(pt);
      });
#else
      inner.DoForEach(pt => candidates.Add(pt));
#endif
      bool walkable_loc(in Point test) {
        if (!basement.GetTileModelAt(test).IsWalkable) {
          candidates.Remove(test);
#if MORE_AGGRESSIVE_CONNECTED_BASEMENTS
          candidates_1x2.Remove(test);
          candidates_2x1.Remove(test);
          candidates_1x2.Remove(test+Direction.N);
          candidates_2x1.Remove(test+Direction.W);
#endif
          return false;
        }
        // we want to actually inspect all points so do not clear 1x1 candidates that are ruled out by being walkable
        // ok to rule out larger disconnects
#if MORE_AGGRESSIVE_CONNECTED_BASEMENTS
        // these five are the inverse of the top-left corner for rectangular disconnects
        Point tmp_pt = test+Direction.NE;
        candidates_1x2.Remove(tmp_pt);
        candidates_2x1.Remove(tmp_pt);
        tmp_pt = test+Direction.E;
        candidates_1x2.Remove(tmp_pt);
        candidates_2x1.Remove(tmp_pt);
        tmp_pt = test+Direction.SE;
        candidates_1x2.Remove(tmp_pt);
        candidates_2x1.Remove(tmp_pt);
        tmp_pt = test+Direction.S;
        candidates_1x2.Remove(tmp_pt);
        candidates_2x1.Remove(tmp_pt);
        tmp_pt = test+Direction.SW;
        candidates_2x1.Remove(tmp_pt);
        // inverse-SE
        tmp_pt = test+Direction.NW;
        candidates_1x2.Remove(tmp_pt);
        candidates_2x1.Remove(tmp_pt);
        tmp_pt += Direction.W;
        candidates_2x1.Remove(tmp_pt);
        tmp_pt += Direction.NE;
        candidates_1x2.Remove(tmp_pt);
        // inverse-S
        tmp_pt = test+Direction.N;
        candidates_2x1.Remove(tmp_pt);
        tmp_pt += Direction.N;
        candidates_1x2.Remove(tmp_pt);
        tmp_pt += Direction.E;
        candidates_1x2.Remove(tmp_pt);
        // inverse-W
        tmp_pt = test+Direction.W;
        candidates_1x2.Remove(tmp_pt);
        tmp_pt += Direction.W;
        candidates_2x1.Remove(tmp_pt);
        tmp_pt += Direction.S;
        candidates_2x1.Remove(tmp_pt);
#endif
        return true;
      }

      while(0<candidates.Count) {
        var test = candidates.First();
        if (!walkable_loc(in test)) continue;
        bool no_problem = false;
        foreach(var test2 in test.Adjacent()) {
          if (walkable_loc(in test2)) {
            no_problem = true;
            break;
          }
        }
        if (no_problem) {
          candidates.Remove(test);
          continue;
        }
        // still here...problem
        var air = new Dictionary<Point, int>();
        foreach(var test3 in test.Adjacent()) {
            if (test3 == test) continue;
            if (basement.IsOnEdge(test3)) continue;
            air[test3] = basement.CountAdjacentTo(test3,pt => basement.GetTileModelAt(pt).IsWalkable);
        }
        if (!basement.HasExitAt(in test)) {
          air.OnlyIfMaximal();
          var exchange = m_DiceRoller.Choose(air).Key;
          basement.SetTileModelAt(test, GameTiles.WALL_BRICK);
          basement.SetTileModelAt(exchange, GameTiles.FLOOR_CONCRETE);
          goto restart;
        }
#if DEBUG
        throw new InvalidProgramException("need to handle 1x1 exit isolation");
#else
        // silently fail
        candidates.Remove(test);
#endif
      }
#if MORE_AGGRESSIVE_CONNECTED_BASEMENTS
      if (0 < candidates_2x1.Count) throw new InvalidProgramException("need to handle 2x1 disconnect");
      if (0 < candidates_1x2.Count) throw new InvalidProgramException("need to handle 1x2 disconnect");
#endif
      return true;
    }

    private Map GenerateHouseBasementMap(Map map, Block houseBlock)
    {
      Rectangle buildingRect = houseBlock.BuildingRect;
      Map basement = new Map(map.Seed << 1 + buildingRect.Left * map.Height + buildingRect.Top, string.Format("basement{0}{1}@{2}-{3}", (object)m_Params.District.WorldPosition.X, (object)m_Params.District.WorldPosition.Y, (object) (buildingRect.Left + buildingRect.Width / 2), (object) (buildingRect.Top + buildingRect.Height / 2)), map.District, buildingRect.Width, buildingRect.Height, Lighting.DARKNESS);
      basement.AddZone(MakeUniqueZone("basement", basement.Rect));
      TileFill(basement, GameTiles.FLOOR_CONCRETE, true);
      TileRectangle(basement, GameTiles.WALL_BRICK, basement.Rect);
      var candidates = new List<Point>();
      buildingRect.DoForEach(pt => candidates.Add(pt), pt => map.GetTileModelAt(pt).IsWalkable && !map.HasMapObjectAt(pt) && map.IsInsideAt(pt));
      Point point = m_DiceRoller.Choose(candidates);
      Point basementStairs = point - buildingRect.Location;
      AddExit(map, point, basement, basementStairs, GameImages.DECO_STAIRS_DOWN);
      AddExit(basement, basementStairs, map, point, GameImages.DECO_STAIRS_UP);
      DoForEachTile(basement.Rect, (Action<Point>) (pt =>
      {
        if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_PILAR_CHANCE) || pt == basementStairs) return;
        if (GameTiles.WALL_BRICK == basement.GetTileModelAt(pt)) return; // already wall
        // We are iterating all rows Y in each column X
        // XXX so if we end up disconnecting we find out vertically
        // basement.Rect.Top and basement.Rect.Left are hardcoded 0
        // coordinates 0, width-1, height-1 are already brick walls
        basement.SetTileModelAt(pt, GameTiles.WALL_BRICK);
      }));
      // Tourism will fail if not all targets are accessible from the exit.  Transposing should be safe here.
      while(!_ForceHouseBasementConnected(basement,basementStairs));
      // set up police investigation after floor layout is stable
      var basement_items = new HashSet<Gameplay.GameItems.IDs>(construction_shop_stock.Select(item_spec => item_spec.Key));  // XXX \todo could be done early but only needed during world generation
      basement.Rect.DoForEach(pt => {
          if (!basement.GetTileModelAt(pt).IsWalkable) return;
          Session.Get.Police.Investigate.Record(basement, in pt);
          Session.Get.Police.ItemMemory.Set(new Location(basement,pt), basement_items, 0);   // basements generally are low-risk Day 0 for police so coming here when looking for huge hammers is sensible
      });
      MapObjectFill(basement, basement.Rect, (Func<Point, MapObject>) (pt =>
      {
        if (!m_DiceRoller.RollChance(HOUSE_BASEMENT_OBJECT_CHANCE_PER_TILE)) return null;
        if (basement.HasExitAt(in pt)) return null;
        if (!basement.IsWalkable(pt)) return null;
        switch (m_DiceRoller.Roll(0, 5)) {
          case 0: return MakeObjJunk();
          case 1: return MakeObjBarrels();
          case 2:
            basement.DropItemAt(MakeShopConstructionItem(), in pt);
            return MakeObjTable(GameImages.OBJ_TABLE);
          case 3:
            {
            var drawer = MakeObjDrawer();
            drawer.Inventory.AddAll(MakeShopConstructionItem());
            return drawer;
            }
#if DEBUG
          case 4:
#else
          default:
#endif
            return MakeObjBed(GameImages.OBJ_BED);
#if DEBUG
          default: throw new ArgumentOutOfRangeException("unhandled roll");
#endif
        }
      }));
      if (Session.Get.HasZombiesInBasements)
        DoForEachTile(basement.Rect, (Action<Point>) (pt =>
        {
          if (!basement.IsWalkable(pt) || basement.HasExitAt(in pt) || !m_DiceRoller.RollChance(HOUSE_BASEMENT_ZOMBIE_RAT_CHANCE)) return;
          basement.PlaceAt(CreateNewBasementRatZombie(0), in pt);
        }));
      if (m_DiceRoller.RollChance(HOUSE_BASEMENT_WEAPONS_CACHE_CHANCE))
        MapObjectPlaceInGoodPosition(basement, basement.Rect, (Func<Point, bool>) (pt => !basement.HasExitAt(in pt) && basement.IsWalkable(pt) && (!basement.HasMapObjectAt(pt) && !basement.HasItemsAt(pt))), m_DiceRoller, (Func<Point, MapObject>) (pt =>
        { // survivalist weapons cache.  Grenades were not acquired locally.  Guaranteed usable.
          var shelf = MakeObjShelf();
          var o_inv = shelf.Inventory!;
          o_inv.AddAll(MakeItemGrenade());
          o_inv.AddAll(MakeItemGrenade());
          // There will be a primary ranged weapon (with 2 ammo clips)
          // and a secondary ranged weapon (with one ammo clip)
          KeyValuePair<GameItems.IDs,GameItems.IDs> survivalist_cache_ranged = m_DiceRoller.Choose(survivalist_ranged_candidates);
          o_inv.AddAll(ItemRangedWeapon.make(survivalist_cache_ranged.Key));
          o_inv.AddAll(ItemAmmo.make(survivalist_cache_ranged.Key));
          o_inv.AddAll(ItemAmmo.make(survivalist_cache_ranged.Key));
          o_inv.AddAll(ItemRangedWeapon.make(survivalist_cache_ranged.Value));
          o_inv.AddAll(ItemAmmo.make(survivalist_cache_ranged.Value));
          Session.Get.Police.Investigate.Record(basement, in pt);
          return shelf;
        }));

      // alpha10
      // music.
      basement.BgMusic = GameMusics.SEWERS;

      return basement;
    }

    public Map GenerateUniqueMap_CHARUnderground(Map surfaceMap, Zone officeZone)
    {
#if DEBUG
      if (null == surfaceMap) throw new ArgumentNullException(nameof(surfaceMap));
#endif
      Zone zone1 = null;
      Point surfaceExit = new Point();
      {
      bool flag = false;
      do {
        // We do not want to evaluate this for each point in the office
        do {
          var pt = m_DiceRoller.Choose(officeZone.Bounds);
          List<Zone> zonesAt = surfaceMap.GetZonesAt(pt);
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
        surfaceExit = m_DiceRoller.Choose(candidates);
        flag = true;
      }
      while (!flag);
      }

      const int BASE_WIDTH = 100;   // these do not space-time scale
      const int BASE_HEIGHT = 100;
      Map underground = new Map(surfaceMap.Seed << 3 ^ surfaceMap.Seed, string.Format("CHAR Underground Facility @{0}-{1}", surfaceExit.X, surfaceExit.Y), surfaceMap.District, BASE_WIDTH, BASE_HEIGHT, Lighting.DARKNESS, true);
      TileFill(underground, GameTiles.FLOOR_OFFICE, true);
      TileRectangle(underground, GameTiles.WALL_CHAR_OFFICE, underground.Rect);

      DoForEachTile(zone1.Bounds, pt => {
        if (!(surfaceMap.GetMapObjectAt(pt) is DoorWindow)) return;
        surfaceMap.RemoveMapObjectAt(pt);
        DoorWindow doorWindow = MakeObjIronDoor();
        doorWindow.Barricade(Rules.BARRICADING_MAX);
        surfaceMap.PlaceAt(doorWindow, pt);
      });
      Point point2 = underground.Rect.Size/2;
      AddExit(underground, point2, surfaceMap, surfaceExit, GameImages.DECO_STAIRS_UP);
      AddExit(surfaceMap, surfaceExit, underground, point2, GameImages.DECO_STAIRS_DOWN);
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
      foreach (Rectangle rect5 in list) TileRectangle(underground, GameTiles.WALL_CHAR_OFFICE, rect5);
      foreach (Rectangle rectangle in list) {
        Point position1 = rectangle.Anchor(rectangle.Left < underground.Width / 2 ? Compass.XCOMlike.E : Compass.XCOMlike.W);
        if (!underground.HasMapObjectAt(position1)) PlaceDoorIfAccessibleAndNotAdjacent(underground, position1, GameTiles.FLOOR_OFFICE, 6, MakeObjCharDoor());
        Point position2 = rectangle.Anchor(rectangle.Top < underground.Height / 2 ? Compass.XCOMlike.S : Compass.XCOMlike.N);
        if (!underground.HasMapObjectAt(position2)) PlaceDoorIfAccessibleAndNotAdjacent(underground, position2, GameTiles.FLOOR_OFFICE, 6, MakeObjCharDoor());
      }
      for (var right = rect1.Right; right < rect4.Left; ++right) {
        PlaceDoor(underground, new Point(right, rect1.Bottom - 1), GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
        PlaceDoor(underground, new Point(right, rect3.Top), GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
      }
      for (var bottom = rect1.Bottom; bottom < rect3.Top; ++bottom) {
        PlaceDoor(underground, new Point(rect1.Right - 1, bottom), GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
        PlaceDoor(underground, new Point(rect2.Left, bottom), GameTiles.FLOOR_OFFICE, MakeObjIronDoor());
      }
      foreach (Rectangle wallsRect in list)
      {
        Rectangle rectangle = new Rectangle(wallsRect.Location+Direction.SE, wallsRect.Size+2*Direction.NW);
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
        underground.AddZone(MakeUniqueZone(basename, wallsRect));   // 2019-05-13 adjusted to de-crash sleeping; used to be rectangle
      }
      var entrance_foyer_anchor = point2 + Direction.NW;
      underground.AddZone(MakeUniqueZone("entrance foyer", new Rectangle(entrance_foyer_anchor, 3, 3)));
      underground.AddZone(MakeUniqueZone("north-south hallway", new Rectangle(entrance_foyer_anchor.X, 0, 3, BASE_HEIGHT)));
      underground.AddZone(MakeUniqueZone("east-west hallway", new Rectangle(0, entrance_foyer_anchor.Y, BASE_WIDTH, 3)));

      for (int x = 0; x < underground.Width; ++x) {
        for (int y = 0; y < underground.Height; ++y) {
          if (m_DiceRoller.RollChance(25)) {    // \todo map generation break: optimize RNG usage
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

      bool actor_ok_here(Point pt) { return !underground.HasExitAt(in pt); }

      int width = underground.Width;
      for (int index1 = 0; index1 < width; ++index1) {
        Actor newUndead = CreateNewUndead(0);
        if (RogueGame.Options.AllowUndeadsEvolution && Session.Get.HasEvolution) {
          while (true) {
            GameActors.IDs index2 = newUndead.Model.ID.NextUndeadEvolution();
            if (index2 == newUndead.Model.ID) break;
            newUndead.Model = GameActors.From(index2);
          }
        }
        ActorPlace(m_DiceRoller, underground, newUndead, actor_ok_here);
      }
      {
      var squad = new List<Actor>();
      int num1 = underground.Width / 10;
      for (int index = 0; index < num1; ++index) {
        Actor newCharGuard = CreateNewCHARGuard(0);
        ActorPlace(m_DiceRoller, underground, newCharGuard, actor_ok_here);
        squad.Add(newCharGuard);
      }
      Gameplay.AI.CHARGuardAI.DeclareSquad(squad);
      }

      // alpha10
      // 8. Music
      underground.BgMusic = GameMusics.CHAR_UNDERGROUND_FACILITY;

      return underground;
    }

    private const int CHAR_armory_checksum = 2500;
    private readonly KeyValuePair<GameItems.IDs, int>[] CHAR_armory_stock = {
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_SHOTGUN,192),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.RANGED_HUNTING_RIFLE,192),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_SHOTGUN,448),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.AMMO_LIGHT_RIFLE,448),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.EXPLOSIVE_GRENADE_PRIMED,320),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRACKER_ZTRACKER,200),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.TRACKER_BLACKOPS,200),
        new KeyValuePair<GameItems.IDs,int>(GameItems.IDs.ARMOR_CHAR_LIGHT_BODYARMOR,500)
    };

    private void MakeCHARArmoryRoom(Map map, Rectangle roomRect)
    {
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt) < 3) return null;
        if (map.HasExitAt(in pt)) return null;
        int choice = m_DiceRoller.Roll(0, CHAR_armory_checksum / 4*5);   // historically 80% chance of an item
        if (CHAR_armory_checksum <= choice) return null;
        var shelf = MakeObjShelf();
        shelf.Inventory.AddAll(PostprocessQuantity(GameItems.From(CHAR_armory_stock.UseRarityTable(choice)).create()));
        return shelf;
      }));
    }

    private void MakeCHARStorageRoom(Map map, Rectangle roomRect)
    {
      TileFill(map, GameTiles.FLOOR_CONCRETE, roomRect);
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt) > 0) return null;
        if (map.HasExitAt(in pt)) return null;
        if (!m_DiceRoller.RollChance(50)) return null;
        return (m_DiceRoller.RollChance(50) ? MakeObjJunk() : MakeObjBarrels());
      }));
      for (var left = roomRect.Left; left < roomRect.Right; ++left) {
        for (var top = roomRect.Top; top < roomRect.Bottom; ++top) {
          Point pt = new Point(left,top);
          if (CountAdjWalls(map, pt) <= 0 && !map.HasMapObjectAt(pt))
            map.DropItemAt(MakeShopConstructionItem(), in pt);
        }
      }
    }

    private void MakeCHARLivingRoom(Map map, Rectangle roomRect)
    {
      TileFill(map, GameTiles.FLOOR_PLANKS, roomRect, (Action<Tile, TileModel, int, int>) ((tile, model, x, y) => tile.AddDecoration(GameImages.DECO_CHAR_FLOOR_LOGO)));
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt) < 3) return null;
        if (map.HasExitAt(in pt)) return null;
        if (!m_DiceRoller.RollChance(30)) return null;
        if (m_DiceRoller.RollChance(50)) return MakeObjBed(GameImages.OBJ_BED);
        return MakeObjFridge();
      }));
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt) > 0) return null;
        if (map.HasExitAt(in pt)) return null;
        if (!m_DiceRoller.RollChance(30)) return null;
        if (!m_DiceRoller.RollChance(30)) return MakeObjChair(GameImages.OBJ_CHAR_CHAIR);
        map.DropItemAt(MakeItemCannedFood(), in pt);
        return MakeObjTable(GameImages.OBJ_CHAR_TABLE);
      }));
    }

    private void MakeCHARPharmacyRoom(Map map, Rectangle roomRect)
    {
      MapObjectFill(map, roomRect, (Func<Point, MapObject>) (pt =>
      {
        if (CountAdjWalls(map, pt) < 3) return null;
        if (map.HasExitAt(in pt)) return null;
        if (!m_DiceRoller.RollChance(20)) return null;
        var shelf = MakeObjShelf();
        shelf.Inventory.AddAll(MakeHospitalItem());
        return shelf;
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
        if (!map.GetTileModelAt(pt).IsWalkable || map.HasExitAt(in pt) || CountAdjWalls(map, pt) < 3) return;
        map.PlaceAt(MakeObjPowerGenerator(), pt);
      }));
    }

    private void MakePoliceStation(Map map, List<Block> freeBlocks)
    {
      Block policeBlock = m_DiceRoller.ChooseWithoutReplacement(freeBlocks);
      GeneratePoliceStation(map, policeBlock, out Point stairsToLevel1);
      Map officesLevel = GeneratePoliceStation_OfficesLevel(map);
      Map jailsLevel = GeneratePoliceStation_JailsLevel(officesLevel);
      officesLevel.BgMusic = jailsLevel.BgMusic = GameMusics.SURFACE;   // alpha10 music
      AddExit(map, stairsToLevel1, officesLevel, new Point(1, 1), GameImages.DECO_STAIRS_DOWN);
      AddExit(officesLevel, new Point(1, 1), map, stairsToLevel1, GameImages.DECO_STAIRS_UP);
      var offices_jails_origin = officesLevel.Rect.Anchor(Compass.XCOMlike.SW) + Direction.NE;
      AddExit(officesLevel, offices_jails_origin, jailsLevel, new Point(1, 1), GameImages.DECO_STAIRS_DOWN);
      AddExit(jailsLevel, new Point(1, 1), officesLevel, offices_jails_origin, GameImages.DECO_STAIRS_UP);
      m_Params.District.AddUniqueMap(officesLevel);
      m_Params.District.AddUniqueMap(jailsLevel);
      Session.Get.UniqueMaps.PoliceStation_OfficesLevel = new UniqueMap(officesLevel);
      Session.Get.UniqueMaps.PoliceStation_JailsLevel = new UniqueMap(jailsLevel);
    }

    static private void GeneratePoliceStation(Map surfaceMap, Block policeBlock, out Point stairsToLevel1)
    {
      TileFill(surfaceMap, GameTiles.FLOOR_TILES, policeBlock.InsideRect, true);
      TileRectangle(surfaceMap, GameTiles.WALL_POLICE_STATION, policeBlock.BuildingRect);
      TileRectangle(surfaceMap, GameTiles.FLOOR_WALKWAY, policeBlock.Rectangle);
      DoForEachTile(policeBlock.InsideRect, surfaceMap, loc => Session.Get.ForcePoliceKnown(loc));
      Point entryDoorAt = policeBlock.BuildingRect.Anchor(Compass.XCOMlike.S);
      surfaceMap.AddDecorationAt(GameImages.DECO_POLICE_STATION, entryDoorAt+Direction.W);
      surfaceMap.AddDecorationAt(GameImages.DECO_POLICE_STATION, entryDoorAt+Direction.E);
      surfaceMap.AddZone(new Zone("NoCivSpawn", new Rectangle(policeBlock.BuildingRect.Left,policeBlock.BuildingRect.Top,policeBlock.BuildingRect.Width,3)));  // once the power locks go in civilians won't be able to path here
      Rectangle rect = new Rectangle(policeBlock.BuildingRect.Location+2*Direction.S, policeBlock.BuildingRect.Size + 2 * Direction.N);
      TileRectangle(surfaceMap, GameTiles.WALL_POLICE_STATION, rect);
      Point restrictedDoorAt = rect.Anchor(Compass.XCOMlike.N);
      PlaceDoor(surfaceMap, restrictedDoorAt, GameTiles.FLOOR_TILES, MakeObjIronDoor());
      PlaceDoor(surfaceMap, entryDoorAt, GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      DoForEachTile(rect, pt => {
        if (!surfaceMap.IsWalkable(pt) || CountAdjWalls(surfaceMap, pt) == 0 || surfaceMap.AnyAdjacent<DoorWindow>(pt))
          return;
        surfaceMap.PlaceAt(MakeObjBench(), pt);
      });
      stairsToLevel1 = restrictedDoorAt+Direction.N;
      surfaceMap.AddZone(MakeUniqueZone("Police Station", policeBlock.BuildingRect));
      MakeWalkwayZones(surfaceMap, policeBlock);
    }

    private Map GeneratePoliceStation_OfficesLevel(Map surfaceMap)
    {
      const int OFFICES_WIDTH = 20; // these do not space-time scale
      const int OFFICES_HEIGHT = 20;
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Offices", surfaceMap.District, OFFICES_WIDTH, OFFICES_HEIGHT, Lighting.LIT);

      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_POLICE_STATION, map.Rect);
      Rectangle rect1 = Rectangle.FromLTRB(3, 0, OFFICES_WIDTH, OFFICES_HEIGHT);
      List<Rectangle> list = new List<Rectangle>();
      // XXX to maximize supplies: need to roll 9-right-wdith on the first horizonal split
      // XXX while this permits 4 rooms vertically, access will be flaky...probably better to have 3
      MakeRoomsPlan(map, ref list, rect1, 5);

      KeyValuePair<ItemModel, int>[] stock = {
        new KeyValuePair<ItemModel,int>(GameItems.POLICE_JACKET,10),
        new KeyValuePair<ItemModel,int>(GameItems.POLICE_RIOT,10),
        new KeyValuePair<ItemModel,int>(GameItems.FLASHLIGHT,5),
        new KeyValuePair<ItemModel,int>(GameItems.BIG_FLASHLIGHT,5),
        new KeyValuePair<ItemModel,int>(GameItems.POLICE_RADIO,10),
        new KeyValuePair<ItemModel,int>(GameItems.TRUNCHEON,20),
        new KeyValuePair<ItemModel,int>(GameItems.PISTOL,6),
        new KeyValuePair<ItemModel,int>(GameItems.AMMO_LIGHT_PISTOL,14),
        new KeyValuePair<ItemModel,int>(GameItems.SHOTGUN,6),
        new KeyValuePair<ItemModel,int>(GameItems.AMMO_SHOTGUN,14)
      };
#if DEBUG
      if (100 != stock.Sum(x => x.Value)) throw new InvalidProgramException("failed crosscheck");
#endif

      Item stock_armory() { return stock.UseRarityTable(m_DiceRoller.Roll(0, 100)).create(); }

      foreach (Rectangle rect2 in list) {
        Rectangle rect3 = new Rectangle(rect2.Location+Direction.SE, rect2.Size+2*Direction.NW);
        if (rect2.Right == map.Width) {
          TileRectangle(map, GameTiles.WALL_POLICE_STATION, rect2);
          PlaceDoor(map, rect2.Anchor(Compass.XCOMlike.W), GameTiles.FLOOR_CONCRETE, MakeObjIronDoor());
          DoForEachTile(rect3, pt => {
            if (!map.IsWalkable(pt) || CountAdjWalls(map, pt) == 0 || map.AnyAdjacent<DoorWindow>(pt)) return;
            var shelf = MakeObjShelf();
            map.PlaceAt(shelf, pt);
            shelf.Inventory.AddAll(stock_armory());
          });
          map.AddZone(MakeUniqueZone("security", rect3));
          continue;
        }
        TileFill(map, GameTiles.FLOOR_PLANKS, rect2);
        TileRectangle(map, GameTiles.WALL_POLICE_STATION, rect2);
        PlaceDoor(map, rect2.Anchor(Compass.XCOMlike.W), GameTiles.FLOOR_PLANKS, MakeObjWoodenDoor());    // \todo if this door is on the main hallway (x coordinate 3) need to exclude fleeing prisoners
        // top-left room has generator rather than furniture.  At Day 0 turn 0 it is on for backstory and gameplay reasons.
        if (0 == rect2.Top && 3 == rect2.Left) {
          PowerGenerator power = MakeObjPowerGenerator();
          power.TogglePower();
          map.PlaceAt(power, new Point(6,1)); // close, but not so close that using it keeps the door from auto-locking
          map.AddZone(MakeUniqueZone("office", rect3));
          continue;
        }
        // Try to leave a non-jumping path to the doors
        // \todo this is optimizable, but level generation is only done once
        MapObjectPlaceInGoodPosition(map, rect3, pt => {
          return map.IsWalkable(pt) && !map.AnyAdjacent<DoorWindow>(pt);
        }, m_DiceRoller, pt => MakeObjTable(GameImages.OBJ_TABLE));
        MapObjectPlaceInGoodPosition(map, rect3, pt => {
          return map.IsWalkable(pt) && !map.AnyAdjacent<DoorWindow>(pt) && 0 == map.CreatesPathingChokepoint(pt);
        }, m_DiceRoller, pt => MakeObjChair(GameImages.OBJ_CHAIR));
        MapObjectPlaceInGoodPosition(map, rect3, pt => {
          return map.IsWalkable(pt) && !map.AnyAdjacent<DoorWindow>(pt) && 0 == map.CreatesPathingChokepoint(pt);
        }, m_DiceRoller, pt => MakeObjChair(GameImages.OBJ_CHAIR));
        map.AddZone(MakeUniqueZone("office", rect3));
      }
      DoForEachTile(new Rectangle(1, 1, 1, OFFICES_HEIGHT - 2), pt => {
        if (pt.Y % 2 == 1 || !map.IsWalkable(pt) || CountAdjWalls(map, pt) != 3) return;
        map.PlaceAt(MakeObjIronBench(), pt);
      });
      map.AddZone(MakeUniqueZone("west corridor", new Rectangle(0,0,3, OFFICES_HEIGHT)));

#if OBSOLETE
      for (int index = 0; index < 5; ++index) {
        ActorPlace(m_DiceRoller, map, CreateNewPoliceman(0));
      }
#else
      Point[] ideal = new Point[5] { new Point(17, 2), new Point(16, 2), new Point(15, 2), new Point(14, 2), new Point(13, 2) };

      for (int index = 0; index < 5; ++index) {
        map.PlaceAt(CreateNewPoliceman(0), in ideal[index]);
      }

      // XXX AI by default would "stock up" before charging out to the surface.
      // The simplest way to "override" is to say that these are SWAT reserves, so they have already "stocked up"
      // While here, sort the turn order -- nearest to stairs up should go first

      // sort leadership 2 up front to increase plausibility of their getting backup guns
      var impressive_cops = map.Police.Get.Where(a=> 2<=a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP)).ToList();
      if (0<impressive_cops.Count) {
        foreach(Actor cop in impressive_cops) map.MoveActorToFirstPosition(cop);
      }

      // armor tuneup
      foreach(Actor cop in map.Police.Get) {
        if (cop.Inventory.Has(GameItems.IDs.ARMOR_POLICE_RIOT)) continue;
        if (map.SwapItemTypes(GameItems.IDs.ARMOR_POLICE_RIOT, GameItems.IDs.ARMOR_POLICE_JACKET, cop.Inventory)) continue;
        if (cop.Inventory.Has(GameItems.IDs.ARMOR_POLICE_JACKET)) continue;
        map.TakeItemType(GameItems.IDs.ARMOR_POLICE_JACKET, cop.Inventory);
      }

      // if we have a truncheon, we can use it -- get a second one
      foreach(Actor cop in map.Police.Get) {
        if (!cop.Inventory.Has(GameItems.IDs.MELEE_TRUNCHEON)) continue;
        map.TakeItemType(GameItems.IDs.MELEE_TRUNCHEON, cop.Inventory);
      }

      // should be at inventory 4 (martial arts) or 5 (normal) now
      // arm for bear
      // first, try to get a backup gun and clip
      foreach(Actor cop in map.Police.Get) {
        if (cop.Inventory.Has(GameItems.IDs.RANGED_PISTOL)) {
          if (!map.TakeItemType(GameItems.IDs.RANGED_SHOTGUN, cop.Inventory)) continue;
          if (!map.TakeItemType(GameItems.IDs.AMMO_SHOTGUN, cop.Inventory)) continue;
        } else /* if (a.Inventory.Has(GameItems.IDs.RANGED_SHOTGUN)) */ {
          if (!map.TakeItemType(GameItems.IDs.RANGED_PISTOL, cop.Inventory)) continue;
          if (!map.TakeItemType(GameItems.IDs.AMMO_LIGHT_PISTOL, cop.Inventory)) continue;
        }
      }

      // then try to top off ammo
      foreach(Actor cop in map.Police.Get) {
        if (cop.Inventory.IsFull) continue;
        if (!cop.Inventory.Has(GameItems.IDs.AMMO_LIGHT_PISTOL)) {
          // shotgunner, failed to get full backup
          map.TakeItemType(GameItems.IDs.AMMO_SHOTGUN, cop.Inventory);
          continue;
        } else if (!cop.Inventory.Has(GameItems.IDs.AMMO_SHOTGUN)) {
          // pistol; failed to get full backup
          map.TakeItemType(GameItems.IDs.AMMO_LIGHT_PISTOL, cop.Inventory);
          continue;
        } else {
          // full kit and still has a slot open.  Prefer pistol ammo
          map.TakeItemType(GameItems.IDs.AMMO_LIGHT_PISTOL, cop.Inventory);
          if (cop.Inventory.IsFull) continue;
          map.TakeItemType(GameItems.IDs.AMMO_SHOTGUN, cop.Inventory);
          continue;
        }
      }

      // now, to set up the marching order
      var leaders = new List<Actor>();
      var followers = new List<Actor>();
      var typical = map.Police.Get.ToList();

      static Actor DraftLeader(List<Actor> pool) {
        // first: leadership + backup weapons
        Actor awesome = pool.Find(a => a.Inventory.Has(GameItems.IDs.RANGED_PISTOL) && a.Inventory.Has(GameItems.IDs.RANGED_SHOTGUN) && 2 <= a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP));
        if (null != awesome) return awesome;

        // leadership+pistol
        awesome = pool.Find(a => a.Inventory.Has(GameItems.IDs.RANGED_PISTOL) && 2 <= a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP));
        if (null != awesome) return awesome;

        // leadership+shotgun
        awesome = pool.Find(a => a.Inventory.Has(GameItems.IDs.RANGED_SHOTGUN) && 2 <= a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP));
        if (null != awesome) return awesome;

        // backup weapons
        awesome = pool.Find(a => a.Inventory.Has(GameItems.IDs.RANGED_PISTOL) && a.Inventory.Has(GameItems.IDs.RANGED_SHOTGUN));
        if (null != awesome) return awesome;

        // pistol
        awesome = pool.Find(a => a.Inventory.Has(GameItems.IDs.RANGED_PISTOL));
        if (null != awesome) return awesome;

        // shotgun
        return pool.FirstOrDefault();
      }

      static void Draft(List<Actor> dest, List<Actor> pool) {
        Actor leader = DraftLeader(pool);
        dest.Add(leader);
        pool.Remove(leader);
      }

      // draft 2 leaders and assign the rest to followers
      Draft(leaders,typical);
      Draft(leaders,typical);
      Draft(followers,typical);
      Draft(followers,typical);
      Draft(followers,typical);

      // identify type of deployment
      if (2 <= leaders[0].Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP)) {
        // 3-2.
        map.MoveActorToFirstPosition(followers[0]);
        map.MoveActorToFirstPosition(leaders[1]);
        leaders[1].AddFollower(followers[0]);
        followers.RemoveAt(0);
        leaders.RemoveAt(1);
        map.MoveActorToFirstPosition(followers[1]);
        map.MoveActorToFirstPosition(followers[0]);
        map.MoveActorToFirstPosition(leaders[0]);
        leaders[0].AddFollower(followers[0]);
        leaders[0].AddFollower(followers[1]);

        ideal = new Point[5] { new Point(1,1), new Point(2,1), new Point(2,2), new Point(2,5), new Point(1,5) };
      } else {
        // 2-2-1
        map.MoveActorToFirstPosition(followers[0]);
        followers.RemoveAt(0);
        map.MoveActorToFirstPosition(followers[0]);
        map.MoveActorToFirstPosition(leaders[1]);
        leaders[1].AddFollower(followers[0]);
        followers.RemoveAt(0);
        leaders.RemoveAt(1);
        map.MoveActorToFirstPosition(followers[0]);
        map.MoveActorToFirstPosition(leaders[0]);
        leaders[0].AddFollower(followers[0]);

        ideal = new Point[5] { new Point(1,1), new Point(2,1), new Point(2,5), new Point(1,5), new Point(2,9) };
      }

      typical = map.Police.Get.ToList();
      for (int index = 0; index < 5; ++index) {
        map.PlaceAt(typical[index], in ideal[index]);
      }
#endif

      DoForEachTile(map.Rect, map, loc => Session.Get.ForcePoliceKnown(loc));
      return map;
    }

    private Map GeneratePoliceStation_JailsLevel(Map surfaceMap)
    {
      const int JAILS_WIDTH = 22;
      Map map = new Map(surfaceMap.Seed << 1 ^ surfaceMap.Seed, "Police Station - Jails", surfaceMap.District, JAILS_WIDTH, 6, Lighting.LIT);
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_POLICE_STATION, map.Rect);
      List<Rectangle> rectangleList = new List<Rectangle>();
      short x = 0;
      while (x + 2 < JAILS_WIDTH) {
        Rectangle rect = new Rectangle(x, 3, 3, 3);
        rectangleList.Add(rect);
        TileFill(map, GameTiles.FLOOR_CONCRETE, rect);
        TileRectangle(map, GameTiles.WALL_POLICE_STATION, rect);
        Point gate = rect.Anchor(Compass.XCOMlike.N);
        map.PlaceAt(MakeObjIronBench(), gate + Direction.S);
        map.SetTileModelAt(gate, GameTiles.FLOOR_CONCRETE);
        map.PlaceAt(MakeObjIronGate(), gate);
        map.AddZone(MakeUniqueZone("jail", rect));
        x += 2;
      }
      Rectangle rect1 = Rectangle.FromLTRB(1, 1, JAILS_WIDTH, 3);
      map.AddZone(MakeUniqueZone("cells corridor", rect1));
      map.PlaceAt(MakeObjPowerGenerator(), map.Rect.Anchor(Compass.XCOMlike.NE)+Direction.SW);

      foreach(Rectangle r in rectangleList) {
        Point dest = r.Anchor(Compass.XCOMlike.NW)+Direction.SE;
        Actor newCivilian = CreateNewCivilian(0, 0, 1);
        if (JAILS_WIDTH-3 == dest.X) Session.Get.UniqueActors.init_Prisoner(newCivilian);   // a political prisoner
        else {
          // being held with cause, at least as understood before the z-apocalypse
          newCivilian.Inventory.AddAll(MakeItemGroceries());
        }
        map.PlaceAt(newCivilian, in dest);
      }
      DoForEachTile(map.Rect, map, loc => Session.Get.ForcePoliceKnown(loc));
      return map;
    }

    private void MakeHospital(Map map, List<Block> freeBlocks)
    {
      Block hospitalBlock = m_DiceRoller.ChooseWithoutReplacement(freeBlocks);
      GenerateHospitalEntryHall(map, hospitalBlock);
      Map admissions = GenerateHospital_Admissions(map.Seed << 1 ^ map.Seed, map.District);
      Map offices = GenerateHospital_Offices(map.Seed << 2 ^ map.Seed, map.District);
      Map patients = GenerateHospital_Patients(map.Seed << 3 ^ map.Seed, map.District);
      Map storage = GenerateHospital_Storage(map.Seed << 4 ^ map.Seed, map.District);
      Map power = GenerateHospital_Power(map.Seed << 5 ^ map.Seed, map.District);

      // alpha10 music
      admissions.BgMusic = offices.BgMusic = patients.BgMusic = storage.BgMusic = power.BgMusic = GameMusics.HOSPITAL;

      Point entryStairs = hospitalBlock.InsideRect.Anchor(Compass.XCOMlike.N);
      Point admissionsUpStairs = admissions.Rect.Anchor(Compass.XCOMlike.N)+Direction.S;
      AddExit(map, entryStairs, admissions, admissionsUpStairs, GameImages.DECO_STAIRS_DOWN);
      AddExit(admissions, admissionsUpStairs, map, entryStairs, GameImages.DECO_STAIRS_UP);

      Point admissionsDownStairs = admissions.Rect.Anchor(Compass.XCOMlike.S)+Direction.N;
      Point officesUpStairs = offices.Rect.Anchor(Compass.XCOMlike.N)+Direction.S;
      AddExit(admissions, admissionsDownStairs, offices, officesUpStairs, GameImages.DECO_STAIRS_DOWN);
      AddExit(offices, officesUpStairs, admissions, admissionsDownStairs, GameImages.DECO_STAIRS_UP);

      Point officesDownStairs = offices.Rect.Anchor(Compass.XCOMlike.S)+Direction.N;
      Point patientsUpStairs = patients.Rect.Anchor(Compass.XCOMlike.N)+Direction.S;
      AddExit(offices, officesDownStairs, patients, patientsUpStairs, GameImages.DECO_STAIRS_DOWN);
      AddExit(patients, patientsUpStairs, offices, officesDownStairs, GameImages.DECO_STAIRS_UP);

      Point patientsDownStairs = patients.Rect.Anchor(Compass.XCOMlike.S)+Direction.N;
      Point storageUpStairs = Direction.SE.Vector;
      AddExit(patients, patientsDownStairs, storage, storageUpStairs, GameImages.DECO_STAIRS_DOWN);
      AddExit(storage, storageUpStairs, patients, patientsDownStairs, GameImages.DECO_STAIRS_UP);

      Point storageDownStairs = storage.Rect.Anchor(Compass.XCOMlike.NE)+Direction.SW;
      Point powerUpStairs = new Point(1, 1);
      AddExit(storage, storageDownStairs, power, powerUpStairs, GameImages.DECO_STAIRS_DOWN);
      AddExit(power, powerUpStairs, storage, storageDownStairs, GameImages.DECO_STAIRS_UP);

      m_Params.District.AddUniqueMap(admissions);
      m_Params.District.AddUniqueMap(offices);
      m_Params.District.AddUniqueMap(patients);
      m_Params.District.AddUniqueMap(storage);
      m_Params.District.AddUniqueMap(power);

      Session.Get.UniqueMaps.Hospital_Admissions = new UniqueMap(admissions);
      Session.Get.UniqueMaps.Hospital_Offices = new UniqueMap(offices);
      Session.Get.UniqueMaps.Hospital_Patients = new UniqueMap(patients);
      Session.Get.UniqueMaps.Hospital_Storage = new UniqueMap(storage);
      Session.Get.UniqueMaps.Hospital_Power = new UniqueMap(power);
    }

    static private void GenerateHospitalEntryHall(Map surfaceMap, Block block)
    {
      TileFill(surfaceMap, GameTiles.FLOOR_TILES, block.InsideRect, true);
      TileRectangle(surfaceMap, GameTiles.WALL_HOSPITAL, block.BuildingRect);
      TileRectangle(surfaceMap, GameTiles.FLOOR_WALKWAY, block.Rectangle);
      Point point1 = block.BuildingRect.Anchor(Compass.XCOMlike.S);
      Point point2 = point1+Direction.W;
      surfaceMap.AddDecorationAt(GameImages.DECO_HOSPITAL, point2+Direction.W);
      surfaceMap.AddDecorationAt(GameImages.DECO_HOSPITAL, point1+Direction.E);
      Rectangle rect = Rectangle.FromLTRB(block.BuildingRect.Left, block.BuildingRect.Top, block.BuildingRect.Right, block.BuildingRect.Bottom);
      PlaceDoor(surfaceMap, point1, GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      PlaceDoor(surfaceMap, point2, GameTiles.FLOOR_TILES, MakeObjGlassDoor());
      DoForEachTile(rect, (Action<Point>) (pt =>
      {
        if (pt.Y == block.InsideRect.Top || (pt.Y == block.InsideRect.Bottom - 1 || !surfaceMap.IsWalkable(pt) || (CountAdjWalls(surfaceMap, pt) == 0 || surfaceMap.AnyAdjacent<DoorWindow>(pt))))
          return;
        surfaceMap.PlaceAt(MakeObjIronBench(), pt);
      }));
      surfaceMap.AddZone(MakeUniqueZone("Hospital", block.BuildingRect));
      MakeWalkwayZones(surfaceMap, block);
    }

    private Map GenerateHospital_Admissions(int seed, District d)
    {
      const int HALLWAY_LENGTH_IN_OFFICES = 8;

      Map map = new Map(seed, "Hospital - Admissions", d, 3 + 2 * HOSPITAL_TYPICAL_WIDTH_HEIGHT, 1+ HALLWAY_LENGTH_IN_OFFICES * (HOSPITAL_TYPICAL_WIDTH_HEIGHT-1), Lighting.DARKNESS);    // central corridor is 3 wide
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(HOSPITAL_TYPICAL_WIDTH_HEIGHT-1, 0, 5, map.Height);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, map.Height);
      Rectangle patient_room = new Rectangle(0, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, HOSPITAL_TYPICAL_WIDTH_HEIGHT);
      while (patient_room.Y <= map.Height - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalPatientRoom(map, "patient room", patient_room, true);
        patient_room.Y += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - HOSPITAL_TYPICAL_WIDTH_HEIGHT, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, map.Height);
      patient_room = new Rectangle(map.Rect.Right - HOSPITAL_TYPICAL_WIDTH_HEIGHT, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, HOSPITAL_TYPICAL_WIDTH_HEIGHT);
      while (patient_room.Y <= map.Height - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalPatientRoom(map, "patient room", patient_room, false);
        patient_room.Y += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
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
      const int HALLWAY_LENGTH_IN_OFFICES = 8;

      Map map = new Map(seed, "Hospital - Offices", d, 3+2* HOSPITAL_TYPICAL_WIDTH_HEIGHT, 1+ HALLWAY_LENGTH_IN_OFFICES*(HOSPITAL_TYPICAL_WIDTH_HEIGHT-1), Lighting.DARKNESS);  // central corridor is 3 wide
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(HOSPITAL_TYPICAL_WIDTH_HEIGHT-1, 0, 5, map.Height);    // left/right borders are the offices
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, map.Height);
      Rectangle offices_room = new Rectangle(0, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, HOSPITAL_TYPICAL_WIDTH_HEIGHT);
      while (offices_room.Y <= map.Height - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalOfficeRoom(map, "office", offices_room, true);
        offices_room.Y += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - HOSPITAL_TYPICAL_WIDTH_HEIGHT, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, map.Height);
      offices_room = new Rectangle(map.Rect.Right - HOSPITAL_TYPICAL_WIDTH_HEIGHT, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, HOSPITAL_TYPICAL_WIDTH_HEIGHT);
      while (offices_room.Y <= map.Height - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalOfficeRoom(map, "office", offices_room, false);
        offices_room.Y += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
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
      const int HALLWAY_LENGTH_IN_OFFICES = 12;

      Map map = new Map(seed, "Hospital - Patients", d, 3 + 2 * HOSPITAL_TYPICAL_WIDTH_HEIGHT, 1+ HALLWAY_LENGTH_IN_OFFICES*(HOSPITAL_TYPICAL_WIDTH_HEIGHT-1), Lighting.DARKNESS);  // central corridor is 3 wide
      TileFill(map, GameTiles.FLOOR_TILES, true);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, map.Rect);
      Rectangle rect = new Rectangle(HOSPITAL_TYPICAL_WIDTH_HEIGHT-1, 0, 5, map.Height);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect);
      map.AddZone(MakeUniqueZone("corridor", rect));
      Rectangle rectangle1 = new Rectangle(0, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, map.Height);
      Rectangle patient_room = new Rectangle(0,0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, HOSPITAL_TYPICAL_WIDTH_HEIGHT);
      while (patient_room.Y <= map.Height - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalPatientRoom(map, "patient room", patient_room, true);
        patient_room.Y += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
      }
      Rectangle rectangle2 = new Rectangle(map.Rect.Right - HOSPITAL_TYPICAL_WIDTH_HEIGHT, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, map.Height);
      patient_room = new Rectangle(map.Rect.Right - HOSPITAL_TYPICAL_WIDTH_HEIGHT, 0, HOSPITAL_TYPICAL_WIDTH_HEIGHT, HOSPITAL_TYPICAL_WIDTH_HEIGHT);
      while (patient_room.Y <= map.Height - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalPatientRoom(map, "patient room", patient_room, false);
        patient_room.Y += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
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
      const int STORAGE_ROOMS_PER_CORRIDOR = 12;
      const int STORAGE_ROOM_DEPTH = HOSPITAL_TYPICAL_WIDTH_HEIGHT - 1;
      const int STORAGE_CORRIDORS = 2;

      // top corridor, with walls, to the generators is constant height 4
      Map map = new Map(seed, "Hospital - Storage", d, 3+ STORAGE_ROOMS_PER_CORRIDOR*(HOSPITAL_TYPICAL_WIDTH_HEIGHT - 1), 4+STORAGE_CORRIDORS*(2+ STORAGE_ROOM_DEPTH), Lighting.DARKNESS);
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
      Rectangle rectangle1 = new Rectangle(2, rect2.Bottom - 1, map.Width - 2, STORAGE_ROOM_DEPTH);
      Rectangle storage_room = new Rectangle(2, rect2.Bottom - 1, HOSPITAL_TYPICAL_WIDTH_HEIGHT, STORAGE_ROOM_DEPTH);
      while (storage_room.X <= map.Width - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalStorageRoom(map, "storage", storage_room);
        storage_room.X += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
      }
      map.SetTileModelAt(1, storage_room.Y, GameTiles.FLOOR_TILES);
      Rectangle rect3 = Rectangle.FromLTRB(0, rectangle1.Bottom - 1, map.Width, rectangle1.Bottom - 1 + 4);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, rect3);
      map.SetTileModelAt(1, rect3.Top, GameTiles.FLOOR_TILES);
      map.AddZone(MakeUniqueZone("south corridor", rect3));
      storage_room = new Rectangle(2, rect3.Bottom - 1, HOSPITAL_TYPICAL_WIDTH_HEIGHT, STORAGE_ROOM_DEPTH);
      while (storage_room.X <= map.Width - HOSPITAL_TYPICAL_WIDTH_HEIGHT) {
        MakeHospitalStorageRoom(map, "storage", storage_room);
        storage_room.X += HOSPITAL_TYPICAL_WIDTH_HEIGHT-1;
      }
      map.SetTileModelAt(1, storage_room.Y, GameTiles.FLOOR_TILES);
      map.AddZone(MakeUniqueZone("west corridor", new Rectangle(0,0,3,map.Height)));
      return map;
    }

    private static Map GenerateHospital_Power(int seed, District d)
    {
      const int POWER_WIDTH = 10;
      const int POWER_HEIGHT = 10;
      Map map = new Map(seed, "Hospital - Power", d, POWER_WIDTH, POWER_HEIGHT, Lighting.DARKNESS);
      TileFill(map, GameTiles.FLOOR_CONCRETE, true);
      TileRectangle(map, GameTiles.WALL_BRICK, map.Rect);
      Rectangle rect = Rectangle.FromLTRB(1, 1, 3, POWER_HEIGHT);
      map.AddZone(MakeUniqueZone("corridor", rect));
      for (short y = 1; y < POWER_HEIGHT - 2; ++y)
        map.PlaceAt(MakeObjIronFence(), new Point(2, y));
      Rectangle room = Rectangle.FromLTRB(3, 0, POWER_WIDTH, POWER_HEIGHT);
      map.AddZone(MakeUniqueZone("power room", room));
      DoForEachTile(room, (Action<Point>) (pt =>
      {
        if (pt.X == room.Left || !map.IsWalkable(pt) || CountAdjWalls(map, pt) < 3) return;
        map.PlaceAt(MakeObjPowerGenerator(), pt);
      }));
      Session.Get.UniqueActors.init_JasonMyers();
      map.PlaceAt(Session.Get.UniqueActors.JasonMyers.TheActor, new Point(POWER_WIDTH / 2, POWER_HEIGHT / 2));
      return map;
    }

#nullable enable
    private Actor CreateNewHospitalPatient(int spawnTime)
    {
      Actor numberedName = (m_DiceRoller.Roll(0, 2) == 0 ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Patient");
      GiveRandomSkillsToActor(numberedName, 1);
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_PATIENT_UNIFORM);
      return numberedName;
    }

    private Actor CreateNewHospitalNurse(int spawnTime)
    {
      Actor numberedName = GameActors.FemaleCivilian.CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Nurse");
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_NURSE_UNIFORM);
      GiveRandomSkillsToActor(numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.MEDIC);
      numberedName.Inventory.AddAll(PostprocessQuantity(GameItems.BANDAGE.instantiate()));
      return numberedName;
    }

    private Actor CreateNewHospitalDoctor(int spawnTime)
    {
      Actor numberedName = GameActors.MaleCivilian.CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      SkinNakedHuman(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, "Doctor");
      numberedName.Doll.AddDecoration(DollPart.TORSO, GameImages.HOSPITAL_DOCTOR_UNIFORM);
      GiveRandomSkillsToActor(numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.MEDIC,3);
      numberedName.StartingSkill(Skills.IDs.LEADERSHIP);
      numberedName.Inventory.AddAll(GameItems.MEDIKIT.instantiate());
      numberedName.Inventory.AddAll(PostprocessQuantity(GameItems.BANDAGE.instantiate()));
      return numberedName;
    }
#nullable restore

    private void MakeHospitalPatientRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
      TileRectangle(map, GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      Direction facing = isFacingEast ? Direction.E : Direction.W;
      PlaceDoor(map, room.Anchor((Compass.XCOMlike)facing.Index)+Direction.N, GameTiles.FLOOR_TILES, MakeObjHospitalDoor());    // this door is offset from the usual position
      Point bedAt = room.Anchor(Compass.XCOMlike.S)+Direction.N;
      map.PlaceAt(MakeObjBed(GameImages.OBJ_HOSPITAL_BED), bedAt);
      map.PlaceAt(MakeObjChair(GameImages.OBJ_HOSPITAL_CHAIR), bedAt+facing);
      var table = MakeObjNightTable(GameImages.OBJ_HOSPITAL_NIGHT_TABLE);
      var pt = bedAt - facing;
      map.PlaceAt(table, pt);

      // Inefficient, but avoids polluting interface
      Item furnish() {
        switch (m_DiceRoller.Roll(0, 3)) {
          case 0: return MakeShopPharmacyItem();
          case 1: return MakeItemGroceries();
#if DEBUG
          case 2: return GameItems.BOOK.create();
          default: throw new InvalidOperationException("unhandled roll result");
#else
          default: return GameItems.BOOK.create();
#endif
        }
      }

      if (m_DiceRoller.RollChance(50)) map.DropItemAt(furnish(), in pt);
      Direction wardrobe_dir = isFacingEast ? Direction.NW : Direction.NE;
      map.PlaceAt(MakeObjWardrobe(GameImages.OBJ_HOSPITAL_WARDROBE), room.Anchor((Compass.XCOMlike)wardrobe_dir.Index)- wardrobe_dir);
    }

    static private void MakeHospitalOfficeRoom(Map map, string baseZoneName, Rectangle room, bool isFacingEast)
    {
      TileFill(map, GameTiles.FLOOR_PLANKS, room);
      TileRectangle(map, GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      var doorAt = room.Anchor(isFacingEast ? Compass.XCOMlike.E : Compass.XCOMlike.W);
      PlaceDoor(map, doorAt, GameTiles.FLOOR_TILES, MakeObjWoodenDoor());
      var midpoint = room.Location + room.Size/2;
      map.PlaceAt(MakeObjTable(GameImages.OBJ_TABLE), midpoint);
      map.PlaceAt(MakeObjChair(GameImages.OBJ_CHAIR), midpoint+Direction.W);
      map.PlaceAt(MakeObjChair(GameImages.OBJ_CHAIR), midpoint+Direction.E);
    }

    private void MakeHospitalStorageRoom(Map map, string baseZoneName, Rectangle room)
    {
      TileRectangle(map, GameTiles.WALL_HOSPITAL, room);
      map.AddZone(MakeUniqueZone(baseZoneName, room));
      PlaceDoor(map, room.Anchor(Compass.XCOMlike.N), GameTiles.FLOOR_TILES, MakeObjHospitalDoor());
      DoForEachTile(room, pt => {
        if (!map.IsWalkable(pt) || map.AnyAdjacent<DoorWindow>(pt)) return;
        var shelf = MakeObjShelf();
        map.PlaceAt(shelf, pt);
        Item it = m_DiceRoller.RollChance(80) ? MakeHospitalItem() : MakeItemCannedFood();
        if (it.Model.IsStackable) it.Quantity = it.Model.StackingLimit;
        shelf.Inventory.AddAll(it);
      });
    }

    static private readonly GameItems.IDs[] rare_random_item = new GameItems.IDs[] {
        GameItems.IDs.EXPLOSIVE_GRENADE_PRIMED,
        GameItems.IDs.ARMOR_ARMY_BODYARMOR,
        GameItems.IDs.AMMO_HEAVY_PISTOL,
        GameItems.IDs.AMMO_HEAVY_RIFLE,
        GameItems.IDs.MELEE_COMBAT_KNIFE,
        GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL
    };

    private void GiveRandomItemToActor(DiceRoller roller, Actor actor, int spawnTime)
    {
      Item equip_this() {
        if (new WorldTime(spawnTime).Day > Rules.GIVE_RARE_ITEM_DAY && roller.RollChance(Rules.GIVE_RARE_ITEM_CHANCE)) {
          return PostprocessQuantity(GameItems.From(rare_random_item[roller.Roll(0, (Session.Get.HasInfection ? 6 : 5))]).create());
        }

        int choice = roller.Roll(0, (int)ShopType._COUNT + 3);
        switch (choice) {
          case (int)ShopType._COUNT: return MakeRandomParkItem();
          case (int)ShopType._COUNT + 1: return MakeRandomBedroomItem();
          case (int)ShopType._COUNT + 2: return MakeRandomKitchenItem();
#if DEBUG 
          default:
            if ((int)ShopType._COUNT > choice) return MakeRandomShopItem((ShopType)choice);
            throw new InvalidProgramException("unhandled roll");
#else
          default: return MakeRandomShopItem((ShopType)choice);
#endif
         }
      }

      actor.Inventory.AddAll(equip_this());
    }

    public Actor CreateNewRefugee(int spawnTime, int itemsToCarry)
    {
      Actor actor;
      if (m_DiceRoller.RollChance(Params.PolicemanChance)) {
        actor = CreateNewPoliceman(spawnTime);
        for (int index = 0; index < itemsToCarry && !actor.Inventory.IsFull; ++index)
          GiveRandomItemToActor(m_DiceRoller, actor, spawnTime);
      } else
        actor = CreateNewCivilian(spawnTime, itemsToCarry, 1);
      GiveRandomSkillsToActor(actor, 1 + new WorldTime(spawnTime).Day);
      return actor;
    }

    private static readonly GameItems.IDs[] survivor_pills = new GameItems.IDs[] { GameItems.IDs.MEDICINE_PILLS_SLP, GameItems.IDs.MEDICINE_PILLS_STA, GameItems.IDs.MEDICINE_PILLS_SAN };

    public Actor CreateNewSurvivor(int spawnTime)
    {
      bool flag = m_DiceRoller.Roll(0, 2) == 0;
      Actor numberedName = (flag ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheSurvivors, spawnTime);
      GiveNameToActor(m_DiceRoller, numberedName);
      DressCivilian(m_DiceRoller, numberedName);
      numberedName.Doll.AddDecoration(DollPart.HEAD, flag ? GameImages.SURVIVOR_MALE_BANDANA : GameImages.SURVIVOR_FEMALE_BANDANA);
      var inv = numberedName.Inventory!;
      inv.AddAll(MakeItemCannedFood());
      inv.AddAll(GameItems.ARMY_RATION.instantiate());
      {
      var rw = (m_DiceRoller.RollChance(50) ? GameItems.ARMY_RIFLE : GameItems.SHOTGUN).instantiate();
      inv.AddAll(rw);
      inv.AddAll(m_DiceRoller.RollChance(50) ? (Item)ItemAmmo.make(rw.Model.ID) : MakeItemGrenade());
      }
      inv.AddAll(GameItems.MEDIKIT.instantiate());
      inv.AddAll(PostprocessQuantity(GameItems.From(m_DiceRoller.Choose(survivor_pills)).create()));
      inv.AddAll(GameItems.ARMY_BODYARMOR.instantiate());
      GiveRandomSkillsToActor(numberedName, 3 + new WorldTime(spawnTime).Day);
      numberedName.CreateCivilianDeductFoodSleep();
      return numberedName;
    }

    public Actor CreateNewCivilian(int spawnTime, int itemsToCarry, int skills)
    {
      Actor numberedName = (m_DiceRoller.Roll(0, 2) == 0 ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateNumberedName(GameFactions.TheCivilians, spawnTime);
      DressCivilian(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      for (int index = 0; index < itemsToCarry; ++index)
        GiveRandomItemToActor(m_DiceRoller, numberedName, spawnTime);
      GiveRandomSkillsToActor(numberedName, skills);
      numberedName.CreateCivilianDeductFoodSleep();
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
      GiveRandomSkillsToActor(numberedName, 1);
      numberedName.StartingSkill(Skills.IDs.FIREARMS);
      numberedName.StartingSkill(Skills.IDs.LEADERSHIP);
      // While auto-equip here would be nice, it is unclear that RogueForm.Game.DoEquipItem is safe to call here.
      // Inline the functional part instead.
      {
      var rw = (m_DiceRoller.RollChance(50) ? GameItems.PISTOL : GameItems.SHOTGUN).instantiate();
      numberedName.Inventory.AddAll(rw);
      numberedName.Inventory.AddAll(ItemAmmo.make(rw.Model.ID));
      numberedName.Equip(rw);
      }
      // do not issue truncheon if martial arts would nerf it
      if (0 >= numberedName.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS)) numberedName.Inventory.AddAll(GameItems.TRUNCHEON.instantiate());
      numberedName.Inventory.AddAll(GameItems.FLASHLIGHT.create());
//    numberedName.Inventory.AddAll(MakeItemPoliceRadio()); // class prop, implicit for police
      if (m_DiceRoller.RollChance(50)) {
        var armor = (m_DiceRoller.RollChance(80) ? GameItems.POLICE_JACKET : GameItems.POLICE_RIOT).instantiate();
        numberedName.Inventory.AddAll(armor);
        numberedName.Equip(armor);
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

#nullable enable
    public static Actor MakeZombified(Actor? zombifier, Actor deadVictim, int turn)
    {
      string properName = string.Format("{0}'s zombie", deadVictim.UnmodifiedName);
      Actor named = (deadVictim.Doll.Body.IsMale ? GameActors.MaleZombified : GameActors.FemaleZombified).CreateNamed(zombifier == null ? GameFactions.TheUndeads : zombifier.Faction, properName, turn);
      named.APreset();
      for (DollPart part = DollPart._FIRST; part < DollPart._COUNT; ++part) {
        var decorations = deadVictim.Doll.GetDecorations(part);
        if (null == decorations) continue;
        foreach (string imageID in decorations) named.Doll.AddDecoration(part, imageID);
      }
      named.Doll.AddDecoration(DollPart.TORSO, GameImages.BLOODIED);
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

      ItemModel[] default_inv = { GameItems.SHOTGUN, GameItems.AMMO_SHOTGUN, GameItems.CHAR_LT_BODYARMOR };
      foreach(var x in default_inv) numberedName.Inventory.AddAll(x.create());

      return numberedName;
    }

    public Actor CreateNewArmyNationalGuard(int spawnTime, string rankName)
    {
      Actor numberedName = GameActors.NationalGuard.CreateNumberedName(GameFactions.TheArmy, spawnTime);
      DressArmy(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, rankName);

      ItemModel[] default_inv = { GameItems.ARMY_RIFLE, GameItems.AMMO_HEAVY_RIFLE, GameItems.ARMY_PISTOL, GameItems.AMMO_HEAVY_PISTOL, GameItems.ARMY_BODYARMOR };
      foreach(var x in default_inv) numberedName.Inventory.AddAll(x.create());
      numberedName.Inventory.AddAll(GameItems.WOODENPLANK.instantiate(GameItems.WOODENPLANK.StackingLimit));

      // National Guard training includes firing range and construction.
      // The minimum physical fitness standards slide off with age.
      // If we had a melee weapons skill it would be here.
      // Maximum acceptable fat % 23%, discharged at 26%
      numberedName.StartingSkill(Skills.IDs.CARPENTRY);
      numberedName.StartingSkill(Skills.IDs.FIREARMS);
      GiveRandomSkillsToActor(numberedName, new WorldTime(spawnTime).Day - RogueGame.NATGUARD_DAY);
      return numberedName;
    }

    public Actor CreateNewArmyNationalGuard(string rankName, Actor leader)
    {
      var actor = CreateNewArmyNationalGuard(leader.Location.Map.LocalTime.TurnCounter, rankName);
      while(null != leader.FirstFollower(a => a.Name == actor.Name)) GiveNameToActor(m_DiceRoller, actor, rankName);
      return actor;
    }

    public Actor CreateNewBikerMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = GameActors.BikerMan.CreateNumberedName(GameFactions.TheBikers, spawnTime);
      numberedName.GangID = gangId;
      DressBiker(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      numberedName.Inventory.AddAll(PostprocessQuantity((m_DiceRoller.RollChance(50) ? GameItems.CROWBAR : GameItems.BASEBALLBAT).create()));
      numberedName.Inventory.AddAll(ItemBodyArmor.make(gangId));
      GiveRandomSkillsToActor(numberedName, new WorldTime(spawnTime).Day - RogueGame.BIKERS_RAID_DAY);
      return numberedName;
    }

    public Actor CreateNewBikerMan(Actor leader)
    {
      var actor = CreateNewBikerMan(leader.Location.Map.LocalTime.TurnCounter, leader.GangID);
      while(null != leader.FirstFollower(a => a.Name == actor.Name)) GiveNameToActor(m_DiceRoller, actor);
      return actor;
    }

    public Actor CreateNewGangstaMan(int spawnTime, GameGangs.IDs gangId)
    {
      Actor numberedName = GameActors.GangstaMan.CreateNumberedName(GameFactions.TheGangstas, spawnTime);
      numberedName.GangID = gangId;
      DressGangsta(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName);
      // Gangsters don't seem very prepared: no reserve ammo
      numberedName.Inventory.AddAll(m_DiceRoller.RollChance(50) ? (Item)MakeItemRandomPistol() : GameItems.BASEBALLBAT.create());
      GiveRandomSkillsToActor(numberedName, new WorldTime(spawnTime).Day - RogueGame.GANGSTAS_RAID_DAY);
      return numberedName;
    }

    public Actor CreateNewGangstaMan(Actor leader)
    {
      var actor = CreateNewGangstaMan(leader.Location.Map.LocalTime.TurnCounter, leader.GangID);
      while(null != leader.FirstFollower(a => a.Name == actor.Name)) GiveNameToActor(m_DiceRoller, actor);
      return actor;
    }

    public Actor CreateNewBlackOps(int spawnTime, string rankName)
    {
      Actor numberedName = GameActors.BlackOps.CreateNumberedName(GameFactions.TheBlackOps, spawnTime);
      DressBlackOps(m_DiceRoller, numberedName);
      GiveNameToActor(m_DiceRoller, numberedName, rankName);

      ItemModel[] default_inv = { GameItems.PRECISION_RIFLE, GameItems.AMMO_HEAVY_RIFLE, GameItems.ARMY_PISTOL, GameItems.AMMO_HEAVY_PISTOL, GameItems.BLACKOPS_GPS };
      foreach(var x in default_inv) numberedName.Inventory.AddAll(x.create());

      return numberedName;
    }

    public Actor CreateNewBlackOps(string rankName, Actor leader)
    {
      var actor = CreateNewBlackOps(leader.Location.Map.LocalTime.TurnCounter, rankName);
      while(null != leader.FirstFollower(a => a.Name == actor.Name)) GiveNameToActor(m_DiceRoller, actor, rankName);
      return actor;
    }

    public Actor CreateNewFeralDog(int spawnTime)
    {
      Actor numberedName = GameActors.FeralDog.CreateNumberedName(GameFactions.TheFerals, spawnTime);
      SkinDog(m_DiceRoller, numberedName);
      return numberedName;
    }
#nullable restore

    static private void AddExit(Map from, Point fromPosition, Map to, Point toPosition, string exitImageID)
    {
      from.SetExitAt(fromPosition, new Exit(to, in toPosition));
      from.AddDecorationAt(exitImageID, in fromPosition);
    }

    static private void MakeWalkwayZones(Map map, Block b)
    {
      Rectangle rectangle = b.Rectangle;
      var s = rectangle.Size + Direction.NW;
      var o = rectangle.Location + Direction.SE;
      var br = rectangle.Location + rectangle.Size + Direction.NW;
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(rectangle.Left, rectangle.Top, s.X, 1)));
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(o.X, br.Y, s.X, 1)));
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(br.X, rectangle.Top, 1, s.Y)));
      map.AddZone(MakeUniqueZone("walkway", new Rectangle(rectangle.Left, o.Y, 1, s.Y)));
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
        BuildingRect = rect;
        BuildingRect.Location += Direction.SE;
        BuildingRect.Size += 2*Direction.NW;
        InsideRect = BuildingRect;
        InsideRect.Location += Direction.SE;
        InsideRect.Size += 2*Direction.NW;
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
      _COUNT    // auto-define to force correctness
    }

    protected enum CHARBuildingType : byte
    {
      NONE,
      AGENCY,
      OFFICE,
    }

    // alpha10
    protected enum HouseOutsideRoomType : byte
    {
      GARDEN = 0,
      PARKING_LOT,
      _COUNT
    }
  }
}
