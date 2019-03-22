// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.District
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Zaimoni.Data;
using Point = System.Drawing.Point;
#endif

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class District
  {
    private readonly List<Map> m_Maps = new List<Map>(3);
    public readonly Point WorldPosition;
    public readonly DistrictKind Kind;
    private string m_Name;
    private Map m_EntryMap;
    private Map m_SewersMap;
    private Map m_SubwayMap;

    public string Name {
      get {
        return m_Name;
      }
      private set {	// XXX GenerateEntryMap updates outside of constructor
        m_Name = value;
      }
    }

    public IEnumerable<Map> Maps { get { return m_Maps; } }
    public int CountMaps { get { return m_Maps.Count; } }

    public Map EntryMap {
      get {
        return m_EntryMap;
      }
      private set {
        if (m_EntryMap != null) RemoveMap(m_EntryMap);
        m_EntryMap = value;
        if (value == null) return;
        AddMap(value);
        // the police faction knows all outside squares (map revealing effect)
        m_EntryMap.Rect.DoForEach(pt=> {
          if (Gameplay.Generators.BaseTownGenerator.PoliceKnowAtGameStart(m_EntryMap,pt)) return;
          if (!m_EntryMap.IsInsideAt(pt)) {
            Engine.Session.Get.ForcePoliceKnown(new Location(m_EntryMap, pt));
            return;
          }
          if (Engine.Session.Get.PoliceInvestigate.Contains(new Location(m_EntryMap,pt))) return; // already known
          Engine.Session.Get.PoliceInvestigate.Record(m_EntryMap, pt);
        });
      }
    }

    public Map SewersMap {
      get {
        return m_SewersMap;
      }
      set { // used from BaseTownGenerator::GenerateSewersMap
        if (m_SewersMap != null) RemoveMap(m_SewersMap);
        m_SewersMap = value;
        if (value == null) return;
        AddMap(value);
      }
    }

    public Map SubwayMap {
      get {
        return m_SubwayMap;
      }
      set { // used from BaseTownGenerator::GenerateSubwayMap
        if (m_SubwayMap != null) RemoveMap(m_SubwayMap);
        m_SubwayMap = value;
        if (value == null) return;
        AddMap(value);
      }
    }

    public bool HasSubway { get { return m_SubwayMap != null; } }

    public District(Point worldPos, DistrictKind kind)
    {
      WorldPosition = worldPos;
      Kind = kind;
    }

    // map manipulation
    protected void AddMap(Map map)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (map.District != this) throw new InvalidOperationException("map.District != this");
#endif
      if (m_Maps.Contains(map)) return;
//    map.District = this;
      m_Maps.Add(map);
    }

    public void AddUniqueMap(Map map)
    {
      AddMap(map);
    }

    public Map GetMap(int index)
    {
      return m_Maps[index];
    }

    protected void RemoveMap(Map map)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
#endif
      m_Maps.Remove(map);
//    map.District = null;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="m"></param>
    /// <returns>positive: cross-district viewing
    /// zero: not at all
    /// negative: large map that doesn't actually go cross-district</returns>
    // positive return codes are cross-district viewing
    // zero is "not at all"
    // negative values are "large maps, but no going off edges" (maps close to police radio radius e.g. CHAR Undergound base)
    private int CrossDistrictView_code(Map m)
    {
      if (m==m_EntryMap) return 1;
      if (m==m_SewersMap) return 2;
      if (m==m_SubwayMap) return 3;
      if (m==Engine.Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap) return -1;
      return 0;
    }

    public Map CrossDistrictViewing(int x)
    {
      switch(x)
      {
      case 1: return m_EntryMap;
      case 2: return m_SewersMap;
      case 3: return m_SubwayMap;
      default: return null;
      }
    }

    static public int UsesCrossDistrictView(Map m)
    {
      return m.District.CrossDistrictView_code(m);
    }


    public bool HasAccessiblePowerGenerators {  // \todo 2019-03-20: currently dead; unsure whether this is reusable or not
      get {
        if (0 < (m_SubwayMap?.PowerGenerators.Get.Count ?? 0)) return true;
        // The hospital doesn't count, here.
        // The police station is arguable, but the plot consequences are such that the current generator in the jails shouldn't count.
        if (this == Engine.Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District) return true;  // \todo when power door locks go in this becomes accessible only to police
        return false;
      }
    }

    // before cross district viewing, this was simply a PlayerCount check
    public bool RequiresUI {
      get {
        if (0 < PlayerCount) return true;
        // \todo Anything that initates UI from outside of the current district has to be PC to avoid a deadlock.
        // once this is working fully, we will never have to reclassify districts.

        // At the base space-time scale (30 turns/hour, 50x50 districts), we have
        // * maximum hearing range 15
        // * maximum viewing radius (8-1)[early evening]+2(big flashlight)+1(on car)=10.
        // so the worst-cases are
        // * LOS: grenade explosion at grid distance 12 spills into view at grid distance 10
        // * sound: a ranged weapon user at grid distance 16 dashes to distance 15, then fires
        // this is evaluated once per scheduling, so we are uncachable.
        // 2017-09-28: grenades are an immediate issue.  melee/ranged combat and noise sources may not be
        // 2017-09-29: moving in PC line of sight is an immediate issue.

        // PC rising as a zomnbie is a UI trigger now that PCs are not Specially Vulnerable
        if (0 < PlayerCorpseCount) return true;
        return false;
      }
    }

    public int PlayerCorpseCount {
      get {
        int ret = 0;
        foreach(Map tmp in Maps) {
          ret += tmp.PlayerCorpseCount;
        }
        return ret;
      }
    }

    // possible micro-optimization target
    public int PlayerCount {
      get {
        int ret = 0;
        foreach(Map tmp in Maps) {
          ret += tmp.PlayerCount;
        }
        return ret;
      }
    }

    public Actor FindPlayer(Map already_failed) {
       foreach(Map tmp in Maps) {
         if (tmp == already_failed) continue;
         Actor tmp2 = tmp.FindPlayer;
         if (null != tmp2) return tmp2;
       }
       return null;
    }

    public bool MessagePlayerOnce(Map already_failed, Action<Actor> fn, Func<Actor, bool> pred =null)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
      foreach(Map map in Maps) {
        if (map == already_failed) continue;
        if (map.MessagePlayerOnce(fn,pred)) return true;
      }
      return false;
    }

    public bool ReadyForNextTurn
    {
      get {
        foreach(Map tmp in Maps) {
          if (!tmp.IsSecret && null != tmp.NextActorToAct) return false;
        }
        return true;
      }
    }

    public void EndTurn()
    {
        foreach(Map m in Maps) {
          m.EndTurn();
        }
    }

    // cheat map similar to savefile viewer
    public void DaimonMap(Zaimoni.Data.OutTextFile dest) {
      if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
      m_EntryMap.DaimonMap(dest);   // name of this is also the district name
      m_SewersMap.DaimonMap(dest);
      if (null!= m_SubwayMap) m_SubwayMap.DaimonMap(dest);
      foreach(Map map in m_Maps) {
        if (map == m_EntryMap) continue;
        if (map == m_SewersMap) continue;
        if (map == m_SubwayMap) continue;
        map.DaimonMap(dest);
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
      int seed = Engine.Session.Get.Seed + y * world.Size + x;

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
      Name = EntryMap.Name;
    }

    public void OptimizeBeforeSaving()
    {
      m_Maps.TrimExcess();
      foreach (Map mMap in m_Maps)
        mMap.OptimizeBeforeSaving();
    }

    public override int GetHashCode()
    {
      return WorldPosition.GetHashCode();
    }
  }
}
