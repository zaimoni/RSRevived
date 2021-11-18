// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.District
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal enum DistrictKind
  {
    GENERAL = 0,
    RESIDENTIAL,
    SHOPPING,
    GREEN,
    BUSINESS, // last in-city district type
    INTERSTATE // first outside-city district type
  }

  [Serializable]
  internal class District : Zaimoni.Serialization.ISerialize
  {
#nullable enable
    private readonly List<Map> m_Maps = new List<Map>(3);
    public readonly Point WorldPosition;
    public readonly DistrictKind Kind;
    private string m_Name;
    private Map? m_EntryMap;
    private Map? m_SewersMap;
    private Map? m_SubwayMap;

    public string Name { get { return m_Name; } }

    public IEnumerable<Map> Maps { get { return m_Maps; } }

    public Map EntryMap {
      get { return m_EntryMap!; }
      private set {
        if (m_EntryMap != null) RemoveMap(m_EntryMap);
        m_EntryMap = value;
//      if (value == null) return;
        m_Name = value.Name;    // historical identity of district name and map name
        AddMap(value);
        // the police faction knows all outside squares (map revealing effect)
        m_EntryMap.Rect.DoForEach(pt=> {
          if (Gameplay.Generators.BaseTownGenerator.PoliceKnowAtGameStart(m_EntryMap,pt)) return;
          if (!m_EntryMap.IsInsideAt(pt)) {
            Engine.Session.Get.ForcePoliceKnown(new Location(m_EntryMap, pt));
            return;
          }
          var loc = new Location(m_EntryMap, pt);
          if (Engine.Session.Get.Police.Investigate.Contains(in loc)) return; // already known
          Engine.Session.Get.Police.Investigate.Record(in loc);
        });
      }
    }

    static public bool IsEntryMap(Map m) {
      return m == m.District.m_EntryMap;
    }
#nullable restore

    public Map? SewersMap {
      get { return m_SewersMap; }
      set { // used from BaseTownGenerator::GenerateSewersMap
        if (m_SewersMap != null) RemoveMap(m_SewersMap);
        m_SewersMap = value;
        if (null != value) AddMap(value);
      }
    }

#nullable enable
    static public bool IsSewersMap(Map m) {
      return m == m.District.m_SewersMap;
    }

    public Map? SubwayMap {
      get { return m_SubwayMap; }
      set { // used from BaseTownGenerator::GenerateSubwayMap
        if (m_SubwayMap != null) RemoveMap(m_SubwayMap);
        m_SubwayMap = value;
        if (null != value) AddMap(value);
      }
    }

    static public bool IsSubwayMap(Map m) {
      return m == m.District.m_SubwayMap;
    }

    static public bool IsSubwayOrSewersMap(Map m) {
      return m == m.District.m_SubwayMap || m == m.District.m_SewersMap;
    }

    public District(Point worldPos, DistrictKind kind)
    {
      WorldPosition = worldPos;
      Kind = kind;
    }

    protected District(Zaimoni.Serialization.DecodeObjects decode)
    {
        byte relay_b = 0;

        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref relay_b);
        Kind = (DistrictKind)(relay_b);

        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref WorldPosition.X);
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref WorldPosition.Y);
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref m_Name);

#if PROTOTYPE
        m_Maps = new();
        void onLoaded(Map[] src) { m_Maps.AddRange(src); }
        decode.LinearLoad<Map>(onLoaded);
#endif

/*
    private Map? m_EntryMap;
    private Map m_SewersMap;    // this is going to stop unconditionally existing when the encircling highway goes in
    private Map? m_SubwayMap;
 */
    }

    public void SaveLoadOk(District test) {
        var err = string.Empty;

        if (Kind != test.Kind) err += "District Kind mismatch: "+ Kind.ToString()+ "" + test.Kind.ToString();
        if (WorldPosition != test.WorldPosition) err += "District Kind mismatch: "+ WorldPosition.to_s()+ "" + test.WorldPosition.to_s();

        if (!string.IsNullOrEmpty(err)) throw new InvalidOperationException(err);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode)
    {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)Kind);
        // don't want to support negative world pos coordinates here; if we have to extend west/north dynamically, that's a constructor call
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, WorldPosition.X);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, WorldPosition.Y);
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, m_Name);
#if PROTOTYPE
        encode.SaveTo(m_Maps);
#endif
            /*
                            private Map? m_EntryMap;
                            private Map m_SewersMap;    // this is going to stop unconditionally existing when the encircling highway goes in
                            private Map? m_SubwayMap;
            */
        }

    [OnDeserialized] private void OnDeserialized(StreamingContext context)
    {
      foreach(var m in m_Maps) m.AfterLoad(this);
    }

    public void RepairLoad()
    {
      foreach(var m in m_Maps) m.RepairLoad();
    }

    // map manipulation
    protected void AddMap(Map map)
    {
#if DEBUG
      if (map.District != this) throw new InvalidOperationException("map.District != this");
#endif
      if (!m_Maps.Contains(map)) m_Maps.Add(map);
#if DEBUG
      // some algorithms assume everything is in at least one zone
      map.Rect.DoForEach(pt => {
        if (!map.IsWalkable(pt)) return;    // no problem if not-walkable point is unzoned
        if (!map.IsInsideAt(pt)) return;    // currently we only care if inside points are zoned (could change)
        var zones = map.GetZonesAt(pt);
        if (null == zones) throw new InvalidOperationException(map.Name+": unzoned "+pt.to_s());
      });
      // we're bringing up containers
      map.DoForAllGroundInventories((loc,inv) => {
          var obj = loc.MapObject;
          if (null != obj && obj.IsContainer) throw new InvalidOperationException("failed to convert to proper container use at game start");
      });
#endif
    }

    public void AddUniqueMap(Map map)   // XXX \todo do we really need a public thin wrapper for a protected function (mapgen so cold path)
    {
      AddMap(map);
    }

    protected void RemoveMap(Map map) { m_Maps.Remove(map); }

    [Serializable]
    public struct MapCode {
        public readonly Point Key;
        public readonly short Value;

        public MapCode(Point k, short v) {
            Key = k;
            Value = v;
        }
    }

    public static MapCode encode(Map m) {
      var code = m.District.m_Maps.IndexOf(m);
      if (0 > code) throw new InvalidOperationException("map unknown by its district");
      return new MapCode(m.District.WorldPosition, (short)code);
    }

    // should work if called at or after IDeserializationCallback.OnDeserialization for World
    public static Map decode(MapCode src) {
      var d = World.Get.At(src.Key);
      if (null == d) throw new ArgumentNullException(nameof(d));
      if (0 > src.Value || d.m_Maps.Count <= src.Value) throw new ArgumentOutOfRangeException("src.Value", src.Value.ToString());
      return d.m_Maps[src.Value];
    }
#nullable restore

    /// <returns>positive: cross-district viewing
    /// zero: not at all
    /// negative: large map that doesn't actually go cross-district i.e. "no going off edges", e.g CHAR Underground base</returns>
    private int CrossDistrictView_code(Map m)
    {
      if (m==m_EntryMap) return 1;
      if (m==m_SewersMap) return 2;
      if (m==m_SubwayMap) return 3;
      if (m==Engine.Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap) return -1;
      return 0;
    }

#nullable enable
    public Map? CrossDistrictViewing(int x)
    {
      switch(x)
      {
      case 1: return m_EntryMap;
      case 2: return m_SewersMap;
      case 3: return m_SubwayMap;
      default: return null;
      }
    }

    static public int UsesCrossDistrictView(Map m) { return m.District.CrossDistrictView_code(m); }

    public bool HasAccessiblePowerGenerators {  // \todo 2019-03-20: currently dead; unsure whether this is reusable or not
      get {
        if (0 < (m_SubwayMap?.PowerGenerators.Get.Count ?? 0)) return true;
        // The hospital doesn't count, here.
        // The police station is arguable, but the plot consequences are such that the current generator in the jails shouldn't count.
        if (this == Engine.Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District) return true;  // \todo when power door locks go in this becomes accessible only to police
        return false;
      }
    }

    // references no-skew scheduler; E, SW are immediately after us (and have no specific ordering with respect to each other)
    // only has to work for adjacent districts
    public bool IsBefore(District rhs)
    {
      if (WorldPosition.Y == rhs.WorldPosition.Y) return WorldPosition.X < rhs.WorldPosition.X;
      return WorldPosition.Y < rhs.WorldPosition.Y;
    }

    public static bool IsBefore(Map lhs, Map rhs)
    {
      if (lhs.DistrictPos != rhs.DistrictPos) return lhs.District.IsBefore(rhs.District);
      var maps = lhs.District.m_Maps;
      return maps.IndexOf(lhs)<maps.IndexOf(rhs);
    }
#nullable restore

    // before cross district viewing, this was simply a PlayerCount check
    public bool RequiresUI {
      get {
        if (0 < ViewpointCount) return true;
        // \todo Anything that initates UI from outside of the current district has to be PC to avoid a deadlock.
        // once this is working fully, we will never have to reclassify districts.

        // At the base space-time scale (30 turns/hour, 50x50 districts), we have
        // * maximum hearing range 15
        // * maximum viewing radius (8-1)[early evening]+2(big flashlight)+1(on car)=10.  We have problems if a ranged weapon can hit a visible target.
        // so the worst-cases are
        // * LOS: grenade explosion at grid distance 12 spills into view at grid distance 10
        // * sound: a ranged weapon user at grid distance 16 dashes to distance 15, then fires
        // this is evaluated once per scheduling, so we are uncachable.
        // 2017-09-28: grenades are an immediate issue.  melee/ranged combat and noise sources may not be
        // 2017-09-29: moving in PC line of sight is an immediate issue.

        // PC rising as a zombie is a UI trigger now that PCs are not Specially Vulnerable
        if (0 < PlayerCorpseCount) return true;

        // take rectangular hull of actor positions for the three maps that can be a problem
        Point surface_corner = new Point(2*Actor.MAX_VISION+4, 2*Actor.MAX_VISION+4);
        Point other_corner = new Point(2*Actor.MAX_VISION, 2*Actor.MAX_VISION);

        if (EntryMap.RequiresUI(surface_corner)) return true;
        if (SewersMap?.RequiresUI(other_corner) ?? false) return true;
        if (SubwayMap?.RequiresUI(other_corner) ?? false) return true;

        return false;
      }
    }

#nullable enable
    public int PlayerCorpseCount {
      get {
        int ret = 0;
        foreach(Map tmp in m_Maps) ret += tmp.PlayerCorpseCount;
        return ret;
      }
    }

    // possible micro-optimization target
    public int PlayerCount {
      get {
        int ret = 0;
        foreach(Map tmp in m_Maps) ret += tmp.PlayerCount;
        return ret;
      }
    }

    public int ViewpointCount {
      get {
        int ret = 0;
        foreach(Map tmp in m_Maps) ret += tmp.ViewpointCount;
        return ret;
      }
    }

    public Actor? FindPlayer(Map? already_failed) {
       foreach(Map tmp in m_Maps) {
         if (tmp == already_failed) continue;
         var tmp2 = tmp.FindPlayer;
         if (null != tmp2) return tmp2;
       }
       return null;
    }

    public void DoForAllActors(Action<Actor> op) { foreach(Map m in m_Maps) m.DoForAllActors(op); }
    public void DoForAllGroundInventories(Action<Location,Inventory> op) { foreach (Map m in m_Maps) m.DoForAllGroundInventories(op); }

    public List<Actor> FilterActors(Predicate<Actor> test) {
      var ret = new List<Actor>();
      void include(Actor a) { if (test(a)) ret.Add(a); }
      foreach(Map m in m_Maps) m.DoForAllActors(include);
      return ret;
    }

    public List<Actor> FilterActors(Predicate<Actor> test, Predicate<Map> m_test) {
      var ret = new List<Actor>();
      void include(Actor a) { if (test(a)) ret.Add(a); }
      foreach(Map m in m_Maps) if (m_test(m)) m.DoForAllActors(include);
      return ret;
    }

    public bool MessagePlayerOnce(Map already_failed, Action<Actor> fn, Func<Actor, bool>? pred)
    {
      foreach(Map map in m_Maps) {
        if (map == already_failed) continue;
        if (map.MessagePlayerOnce(fn,pred)) return true;
      }
      return false;
    }

    public bool ReadyForNextTurn
    {
      get {
        foreach(Map tmp in m_Maps) {
          if (!tmp.IsSecret && null != tmp.NextActorToAct) return false;
        }
        return true;
      }
    }

    public void EndTurn()
    {
        foreach(Map m in m_Maps) {
          m.EndTurn();
        }
    }

    public static void DamnCHAROfficesToPoliceInvestigation(District d)
    {
       if (   2 == Engine.Session.Get.ScriptStage_PoliceStationPrisoner
           && d.WorldPosition == Engine.Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.DistrictPos)
         return;    // already tagged
       Map map = d.EntryMap;
       foreach(var zone in map.Zones) {
         if (!zone.Name.Contains("CHAR Office")) continue;
         zone.Bounds.DoForEach(pt => {
             var loc = new Location(map, pt);
             if (!Engine.Session.Get.Police.ItemMemory.HaveEverSeen(loc)) Engine.Session.Get.Police.Investigate.Record(in loc);
         });
       }
    }
#nullable restore

    // cheat map similar to savefile viewer
    public void DaimonMap(OutTextFile dest) {
      if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
      (m_EntryMap as IMap).DaimonMap(dest);   // name of this is also the district name
      (m_SewersMap as IMap)?.DaimonMap(dest);
      (m_SubwayMap as IMap)?.DaimonMap(dest);
      foreach(Map map in m_Maps) {
        if (map == m_EntryMap) continue;
        if (map == m_SewersMap) continue;
        if (map == m_SubwayMap) continue;
        (map as IMap).DaimonMap(dest);
      }
    }

    // low-level support
    public void GenerateEntryMap(World world, int districtSize, Gameplay.Generators.BaseTownGenerator m_TownGenerator)
    {
      int x = WorldPosition.X;
      int y = WorldPosition.Y;

      ///////////////////////////
      // 1. Compute unique seed.
      // 2. Set params for kind.
      // 3. Generate map.
      ///////////////////////////

      // 1. Compute unique seed.
      int seed = Engine.Session.Seed + y * world.Size + x;

#region 2. Set gen params.
      // this must be a value copy or else: BaseTownGenerator.Parameters must be a struct, not a class
      Gameplay.Generators.BaseTownGenerator.Parameters parameters = Gameplay.Generators.BaseTownGenerator.DEFAULT_PARAMS;

      parameters.MapWidth = parameters.MapHeight = districtSize;
      parameters.District = this;
      const int num = 8;
      string str;
      switch (Kind)
      {
        case DistrictKind.GENERAL:
          str = "District";
          break;
        case DistrictKind.RESIDENTIAL:
          str = "Residential District";
          parameters.CHARBuildingChance /= num;
          parameters.ParkBuildingChance /= num;
          parameters.ShopBuildingChance /= num;
          break;
        case DistrictKind.SHOPPING:
          str = "Shopping District";
          parameters.CHARBuildingChance /= num;
          parameters.ShopBuildingChance *= num;
          parameters.ParkBuildingChance /= num;
          break;
        case DistrictKind.GREEN:
          str = "Green District";
          parameters.CHARBuildingChance /= num;
          parameters.ParkBuildingChance *= num;
          parameters.ShopBuildingChance /= num;
          break;
        case DistrictKind.INTERSTATE:
          str = "Interstate Highway";
          // not really...stub this as a Green district for now
          parameters.CHARBuildingChance = 0; // no CHAR offices or buildings
          parameters.ParkBuildingChance *= num;
          parameters.ShopBuildingChance /= num;
          break;
        case DistrictKind.BUSINESS:
          str = "Business District";
          parameters.CHARBuildingChance *= num;
          parameters.ParkBuildingChance /= num;
          parameters.ShopBuildingChance /= num;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled district kind");
      }
#endregion

      // working around an abstract function declaration that *cannot* have the parameters as an argument.
      // different types of maps may have incompatible parameter structs/classes
      // 3. Generate map.
      Gameplay.Generators.BaseTownGenerator.Parameters @params = m_TownGenerator.Params;
      m_TownGenerator.Params = parameters;
      Map map = m_TownGenerator.Generate(seed, string.Format("{0}@{1}", str, World.CoordToString(x, y)));
      m_TownGenerator.Params = @params;

      // done.
      EntryMap = map;
    }

    public override int GetHashCode()
    {
      return WorldPosition.GetHashCode();
    }
  }
}
