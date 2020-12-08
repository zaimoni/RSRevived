﻿// Decompiled with JetBrains decompiler
// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Map
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define AUDIT_ACTOR_MOVEMENT
// #define AUDIT_ITEM_INVARIANTS

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Linq;
using Zaimoni.Data;

using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;
using ItemMeleeWeapon = djack.RogueSurvivor.Engine.Items.ItemMeleeWeapon;

// map coordinate definitions.  Want to switch this away from System.Drawing.Point to get a better hash function in.
using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
using Size = Zaimoni.Data.Vector2D_short;   // likely to go obsolete with transition to a true vector type

namespace djack.RogueSurvivor.Data
{
  // VAPORWARE this is currently per-map and works off of where the viewer is, not what the light level of the viewing target is.
  // Staying Alive has made a massive fix attempt and should be reviewed. 
  [Serializable]
  internal enum Lighting
  {
    DARKNESS = 0,
    OUTSIDE = 1,
    LIT = 2
  }

  [Serializable]
  internal class Map : ISerializable
  {
    public const int GROUND_INVENTORY_SLOTS = 10;
    public readonly int Seed;
    public readonly Point DistrictPos; // should be District.WorldPosition by construction
#nullable enable
    [NonSerialized] private District? m_District; // keep reference cycle out of savefile
    public District District { get { return m_District!; } }
#nullable restore
    public readonly string Name;
    private string m_BgMusic;  // alpha10
#nullable enable
    private Lighting m_Lighting;
	public readonly WorldTime LocalTime;
    public readonly Size Extent;
	public short Width { get {return Extent.X;} }
	public short Height { get {return Extent.Y;} }
	[NonSerialized] public readonly Rectangle Rect;
    private readonly byte[,] m_TileIDs;
    private readonly byte[] m_IsInside;
    private readonly Dictionary<Point,HashSet<string>> m_Decorations = new Dictionary<Point,HashSet<string>>();
    private readonly Dictionary<Point, Exit> m_Exits = new Dictionary<Point, Exit>();
    private readonly List<Zone> m_Zones = new List<Zone>(5);
    private readonly List<Actor> m_ActorsList = new List<Actor>(5);
    private int m_iCheckNextActorIndex;
    private readonly List<MapObject> m_MapObjectsList = new List<MapObject>(5);
    private readonly Dictionary<Point, Inventory> m_GroundItemsByPosition = new Dictionary<Point, Inventory>(5);
    private readonly List<Corpse> m_CorpsesList = new List<Corpse>(5);
    private readonly Dictionary<Point, List<OdorScent>> m_ScentsByPosition = new Dictionary<Point, List<OdorScent>>(128);
    private readonly List<TimedTask> m_Timers = new List<TimedTask>(5); // The end-of-turn timers.
    // position inverting caches
    [NonSerialized] private readonly Dictionary<Point, Actor> m_aux_ActorsByPosition = new Dictionary<Point, Actor>(5);
    [NonSerialized] private readonly Dictionary<Point, MapObject> m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>(5);
    [NonSerialized] private readonly Dictionary<Point, List<Corpse>> m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>(5);
    // AI support caches, etc.
    [NonSerialized] public readonly NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>> Players;
    [NonSerialized] public readonly NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>> Police;
    [NonSerialized] public readonly NonSerializedCache<List<MapObject>, Engine.MapObjects.PowerGenerator, ReadOnlyCollection<Engine.MapObjects.PowerGenerator>> PowerGenerators;
    [NonSerialized] public readonly NonSerializedCache<Map, Map, HashSet<Map>> destination_maps;
    // map geometry
#if PRERELEASE_MOTHBALL
    [NonSerialized] private readonly List<LinearChokepoint> m_Chokepoints = new List<LinearChokepoint>();
#endif
    [NonSerialized] private readonly List<Point> m_FullCorner_nw = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FullCorner_ne = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FullCorner_se = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FullCorner_sw = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FlushWall_n = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FlushWall_s = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FlushWall_w = new List<Point>();
    [NonSerialized] private readonly List<Point> m_FlushWall_e = new List<Point>();
    [NonSerialized] private Dictionary<Rectangle, ZoneLoc>? m_ClearableZones = null;
    [NonSerialized] private Dictionary<Rectangle,ZoneLoc>? m_CanonicalZones = null;
    // this is going to want an end-of-turn map updater
    [NonSerialized] public readonly TimeCache<string, Dictionary<Location, int>> pathing_exits_to_goals = new TimeCache<string, Dictionary<Location, int>>(); // type is that needed by user, not that generated by pathfinding
    [NonSerialized] private /* readonly */ int _hash;
#if FAKE_ECS
    [NonSerialized] private static List<Map> _transcode = new List<Map>();
#endif

    public bool IsSecret { get; private set; }

    public void Expose() {
      IsSecret = false;
      foreach(Map m in destination_maps.Get) {
        m.destination_maps.Recalc();
      }
    }

    public string BgMusic { // alpha10
      get { return m_BgMusic; }
      set { m_BgMusic = value; }    // retain this as we probably want to imposs access controls
    }

    public Lighting Lighting { get { return m_Lighting; } }
    public bool Illuminate(bool on) {
#if DEBUG
      if (Lighting.OUTSIDE == Lighting) throw new InvalidOperationException(nameof(Lighting)+": not useful to artificially light outside ");
#endif
      if (on) {
        if (Lighting.LIT==Lighting) return false;
        m_Lighting = Lighting.LIT;
        return true;
      }
      if (Lighting.DARKNESS==Lighting) return false;
      m_Lighting = Lighting.DARKNESS;
      return true;
    }

    public IEnumerable<Zone> Zones { get { return m_Zones; } }
    public IEnumerable<Exit> Exits { get { return m_Exits.Values; } }
    public IEnumerable<Actor> Actors { get { return m_ActorsList; } }
    public IEnumerable<MapObject> MapObjects { get { return m_MapObjectsList; } }
    public IEnumerable<Corpse> Corpses { get { return m_CorpsesList; } }
    public int CountCorpses { get { return m_CorpsesList.Count; } }

    // there is a very rare multi-threading related crash due to m_ActorsList (the parameter for these) being adjusted
    // mid-enumeration
    private static ReadOnlyCollection<Actor> _findPlayers(List<Actor> src)
    {
      return new ReadOnlyCollection<Actor>(src.FindAll(a => a.IsPlayer && !a.IsDead));
    }

    private static ReadOnlyCollection<Actor> _findPolice(List<Actor> src)
    {
      return new ReadOnlyCollection<Actor>(src.FindAll(a => a.IsFaction(Gameplay.GameFactions.IDs.ThePolice) && !a.IsDead));
    }

    private static ReadOnlyCollection<Engine.MapObjects.PowerGenerator> _findPowerGenerators(IEnumerable<MapObject> src)
    {
      return new ReadOnlyCollection<Engine.MapObjects.PowerGenerator>(src.OfType< Engine.MapObjects.PowerGenerator >().ToList());
    }
#nullable restore

    public Map(int seed, string name, District d, int width, int height, Lighting light=Lighting.OUTSIDE, bool secret=false)
    {
#if DEBUG
      if (0 >= width) throw new ArgumentOutOfRangeException(nameof(width), width, "0 >= width");
      if (0 >= height) throw new ArgumentOutOfRangeException(nameof(height), height, "0 >= height");
#endif
      Seed = seed;
      Name = name
#if DEBUG
        ?? throw new ArgumentNullException(nameof(name))
#endif
      ;
      Extent = new Size(width,height);
	  m_District = d;
      DistrictPos = d.WorldPosition;
      Rect = new Rectangle(Point.Empty, Extent);
      LocalTime = new WorldTime();
      m_Lighting = light;
      IsSecret = secret;
      m_TileIDs = new byte[width, height];
      m_IsInside = new byte[width*height-1/8+1];
      Players = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPlayers);
      Police = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPolice);
      PowerGenerators = new NonSerializedCache<List<MapObject>, Engine.MapObjects.PowerGenerator, ReadOnlyCollection<Engine.MapObjects.PowerGenerator>>(m_MapObjectsList, _findPowerGenerators);
      destination_maps = new NonSerializedCache<Map, Map, HashSet<Map>>(this,m=>new HashSet<Map>(m_Exits.Values.Select(exit => exit.ToMap).Where(map => !map.IsSecret)));
      OnConstructed(ref _hash);
    }

#region Implement ISerializable
    protected Map(SerializationInfo info, StreamingContext context)
    {
      Seed = info.GetInt32("m_Seed");
      info.read_s(ref DistrictPos, "m_DistrictPos");
      Name = info.GetString("m_Name");
      info.read(ref LocalTime, "m_LocalTime");
      info.read_s(ref Extent, "m_Extent");
      Rect = new Rectangle(Point.Empty,Extent);
      info.read(ref m_Exits, "m_Exits");
      info.read(ref m_Zones, "m_Zones");
      info.read(ref m_ActorsList, "m_ActorsList");
      info.read(ref m_MapObjectsList, "m_MapObjectsList");
      info.read(ref m_GroundItemsByPosition, "m_GroundItemsByPosition");
      info.read(ref m_CorpsesList, "m_CorpsesList");
      info.read_s(ref m_Lighting, "m_Lighting");
      info.read(ref m_ScentsByPosition, "m_ScentsByPosition");
      info.read(ref m_Timers, "m_Timers");
      info.read(ref m_TileIDs, "m_TileIDs");
      info.read(ref m_IsInside, "m_IsInside");
      info.read(ref m_Decorations, "m_Decorations");
      m_BgMusic = info.GetString("m_BgMusic");   // alpha10
      // readonly block
      Players = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPlayers);
      Police = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPolice);
      PowerGenerators = new NonSerializedCache<List<MapObject>, Engine.MapObjects.PowerGenerator, ReadOnlyCollection<Engine.MapObjects.PowerGenerator>>(m_MapObjectsList, _findPowerGenerators);
      destination_maps = new NonSerializedCache<Map, Map, HashSet<Map>>(this,m=>new HashSet<Map>(m_Exits.Values.Select(exit => exit.ToMap).Where(map => !map.IsSecret)));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
#if CPU_HOG_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "preparing to save: "+this);
#endif
      info.AddValue("m_Seed", Seed);
      info.AddValue("m_DistrictPos", DistrictPos);
      info.AddValue("m_Name", Name);
      info.AddValue("m_LocalTime", LocalTime);
      info.AddValue("m_Extent", Extent);
      info.AddValue("m_Exits", m_Exits);
      info.AddValue("m_Zones", m_Zones);
      info.AddValue("m_ActorsList", m_ActorsList);  // this fails when Actor is ISerializable(!): length ok, all values null
      info.AddValue("m_MapObjectsList", m_MapObjectsList);
      info.AddValue("m_GroundItemsByPosition", m_GroundItemsByPosition);
      info.AddValue("m_CorpsesList", m_CorpsesList);
      info.AddValue("m_Lighting", m_Lighting);
      info.AddValue("m_ScentsByPosition", m_ScentsByPosition);
      info.AddValue("m_Timers", m_Timers);
      info.AddValue("m_TileIDs", m_TileIDs);
      info.AddValue("m_IsInside", m_IsInside);
      info.AddValue("m_Decorations", m_Decorations);
      info.AddValue("m_BgMusic", m_BgMusic);    // alpha10
#if CPU_HOG_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "ready to save: "+this);
#endif
     }

    [OnDeserialized] private void OnDeserialized(StreamingContext context)
    {
      ReconstructAuxiliaryFields();
      RegenerateMapGeometry();
      OnConstructed(ref _hash);
    }

    public void AfterLoad(District d) {
      if (DistrictPos == d.WorldPosition) m_District = d;
#if DEBUG
      else throw new InvalidOperationException("district backpointer repair rejected");
#endif
    }

    private void OnConstructed(ref int hash)
    {
      pathing_exits_to_goals.Now(LocalTime.TurnCounter);
      hash = Name.GetHashCode() ^ District.GetHashCode();
#if FAKE_ECS
      _transcode.Add(this);
#endif
    }
#endregion

    // once the peace walls are down, IsInBounds will refer to the actual map data.
    // IsValid will allow "translating" coordinates to adjacent maps in order to fulfil the dereference
    // IsStrictlyValid will *require* "translating" coordinates to adjacent maps in order to fulfil the dereference
    // That is, IsValid := IsInBounds XOR IsStrictlyValid
    public bool IsInBounds(Point p)
    {
      int test;
      return 0 <= (test = p.X) && test < Width && 0 <= (test = p.Y) && test < Height;
    }

    public bool IsOnEdge(Point p)   // but not necessarily in bounds
    {
      int test;
      return 0==(test = p.X) || Width-1==test || 0==(test = p.Y) || Height-1==test;
    }

    // return value of zero may be either "in bounds", or "not valid at all"
    public int DistrictDeltaCode(Point pt)
    {
      int ret = 0;

      int test;
      if (0>(test = pt.X)) ret -= 1;
      else if (Width <= test) ret += 1;

      if (0>(test = pt.Y)) ret -= 3;
      else if (Height <= test) ret += 3;

      return ret;
    }

    public void TrimToBounds(ref Point p)
    {
      int nonstrict_ub;
      short test;
      if ((test = p.X) < 0) p.X = 0;
      else if (test > (nonstrict_ub = Width - 1)) p.X = (short)nonstrict_ub;
      if ((test = p.Y) < 0) p.Y = 0;
      else if (test> (nonstrict_ub = Height - 1)) p.Y = (short)nonstrict_ub;
    }

    public void TrimToBounds(ref Rectangle r)
    {
#if DEBUG
      if (r.X >= Width) throw new ArgumentOutOfRangeException(nameof(r.X),r.X, "r.X >= Width");
      if (r.Y >= Height) throw new ArgumentOutOfRangeException(nameof(r.Y),r.Y, "r.Y >= Height");
      if (0 > r.Right) throw new ArgumentOutOfRangeException(nameof(r.Right),r.Right, "0 > r.Right");
      if (0 > r.Bottom) throw new ArgumentOutOfRangeException(nameof(r.Bottom),r.Bottom, "0 > r.Bottom");
#endif
      int nonstrict_ub;
      short test;
      if ((test = r.X) < 0) {
        r.Width += test;
        r.X = 0;
      }

      if ((test = r.Y) < 0) {
        r.Width += test;
        r.Y = 0;
      }

      if ((test = r.Right) > (nonstrict_ub = Width - 1))  r.Width -= (short)(test - nonstrict_ub);
      if ((test = r.Bottom) > (nonstrict_ub = Height - 1)) r.Height -= (short)(test - nonstrict_ub);
    }

    public bool IsValid(Point p) { return IsInBounds(p) || null != Normalize(p); } // potentially obsolete; cf. Map::Canonical family

    /// <summary>
    /// Cf. Actor::_CanEnter and CanEnter
    /// </summary>
    /// <returns>true if and only if it is possible to configure an actor that can enter the location.</returns>
    static public bool CanEnter(ref Location loc)
    {
      if (!Map.Canonical(ref loc)) return false;
      return loc.TileModel.IsWalkable;
    }

    static public bool Canonical(ref Location loc) {
      if (loc.Map.IsInBounds(loc.Position)) return true;
      var test = loc.Map._Normalize(loc.Position);
      if (null == test) return false;
      loc = test.Value;
      return true;
    }

    /// <param name="pt">Precondition: not in bounds</param>
    private Location? _Normalize(Point pt)
    {
#if IRRATIONAL_CAUTION
      if (IsInBounds(pt)) throw new InvalidOperationException("precondition violated: IsInBounds(pt)");
#endif
      int map_code = District.UsesCrossDistrictView(this);
      if (0>=map_code) return null;
      int delta_code = DistrictDeltaCode(pt);
      if (0==delta_code) return null;
      Point new_district = DistrictPos;    // System.Drawing.Point is a struct: this is a value copy
      Vector2D_int_stack district_delta = new Vector2D_int_stack(0,0);
      while(0!=delta_code) {
        var tmp = Zaimoni.Data.ext_Drawing.sgn_from_delta_code(ref delta_code);
        // XXX: reject Y other than 0,1 in debug mode
        if (1==tmp.Y) {
          district_delta.Y = tmp.X;
          new_district.Y += tmp.X;
          if (0>new_district.Y) return null;
          if (Engine.Session.Get.World.Size<=new_district.Y) return null;
        } else if (0==tmp.Y) {
          district_delta.X = tmp.X;
          new_district.X += tmp.X;
          if (0>new_district.X) return null;
          if (Engine.Session.Get.World.Size<=new_district.X) return null;
        }
      }
      // following fails if district size strictly less than the half-view radius
      Map dest = Engine.Session.Get.World[new_district.X,new_district.Y].CrossDistrictViewing(map_code);
      if (null==dest) return null;
      if (1==district_delta.X) pt.X -= Width;
      else if (-1==district_delta.X) pt.X += dest.Width;
      if (1==district_delta.Y) pt.Y -= Height;
      else if (-1==district_delta.Y) pt.Y += dest.Height;
      return dest.IsInBounds(pt) ? new Location(dest, pt) : dest._Normalize(pt);
    }

#nullable enable
    public Location? Normalize(Point pt)
    {
      if (IsInBounds(pt)) return null;
      return _Normalize(pt);
    }

    public Location? Denormalize(in Location loc)
    {
      if (this == loc.Map) {
        if (IsValid(loc.Position)) return loc;  // operator ? : syntax errors; Location? not inferred from Location and null
        else return null;
      }
      int map_code = District.UsesCrossDistrictView(this);
      if (0>=map_code || map_code != District.UsesCrossDistrictView(loc.Map)) return null;
      Vector2D_int_stack district_delta = new Vector2D_int_stack(loc.Map.DistrictPos.X- DistrictPos.X, loc.Map.DistrictPos.Y - DistrictPos.Y);

      // fails at district delta coordinates of absolute value 2+ where intermediate maps do not have same width/height as the endpoint of interest
      Point not_in_bounds = loc.Position;
      if (0 < district_delta.X) not_in_bounds.X += (short)(district_delta.X*Width);
      else if (0 > district_delta.X) not_in_bounds.X += (short)(district_delta.X * loc.Map.Width);
      if (0 < district_delta.Y) not_in_bounds.Y += (short)(district_delta.Y*Height);
      else if (0 > district_delta.Y) not_in_bounds.Y += (short)(district_delta.Y * loc.Map.Height);

      return new Location(this,not_in_bounds);
    }

    public List<Location>? Denormalize(IEnumerable<Location> locs)
    {
      var ret = new List<Location>(locs.Count());
      foreach(var x in locs) {
        Location? test = Denormalize(in x);
        if (null != test) ret.Add(test.Value);
      }
      return 0<ret.Count ? ret : null;
    }

    public bool IsInViewRect(Location loc, Rectangle view)
    {
      if (this != loc.Map) {
        Location? test = Denormalize(in loc);
        if (null == test) return false;
        loc = test.Value;
      }
      return view.Contains(loc.Position);
    }

    // this looks wrong, may need fixing later
    public bool IsOnMapBorder(Point pt)
    {
      return 0 == pt.X || pt.X == Width-1 || 0 == pt.Y || pt.Y == Height-1;
    }

    /// <summary>
    /// GetTileAt does not bounds-check for efficiency reasons;
    /// the typical use case is known to be in bounds by construction.
    /// </summary>
    public Tile GetTileAt(int x, int y)
    {
      int i = y*Width+x;
      return new Tile(m_TileIDs[x,y],(0!=(m_IsInside[i/8] & (1<<(i%8)))),new Location(this,new Point(x,y)));
    }

    /// <summary>
    /// GetTileAt does not bounds-check for efficiency reasons;
    /// the typical use case is known to be in bounds by construction.
    /// </summary>
    public Tile GetTileAt(Point p)
    {
      int i = p.Y*Width+p.X;
      return new Tile(m_TileIDs[p.X,p.Y],(0!=(m_IsInside[i/8] & (1<<(i%8)))),new Location(this,p));
    }

    public void SetIsInsideAt(int x, int y, bool inside=true)
    {
      int i = y*Width+x;
      if (inside) {
        m_IsInside[i/8] |= (byte)(1<<(i%8));
      } else {
        m_IsInside[i/8] &= (byte)(255&(~(1<<(i%8))));
      }
    }

    public void SetIsInsideAt(Point pt, bool inside=true)
    {
      SetIsInsideAt(pt.X,pt.Y, inside);
    }

    public bool IsInsideAt(int x, int y)
    {
      int i = y*Width+x;
      return 0!=(m_IsInside[i/8] & (1<<(i%8)));
    }

    public bool IsInsideAt(Point p)
    {
      int i = p.Y*Width+p.X;
      return 0!=(m_IsInside[i/8] & (1<<(i%8)));
    }

    public bool IsInsideAtExt(Point p)
    {
      if (IsInBounds(p)) return IsInsideAt(p);
      Location? test = _Normalize(p);
      return null!=test && test.Value.Map.IsInsideAt(test.Value.Position);
    }

    public void SetTileModelAt(int x, int y, TileModel model)
    {
#if DEBUG
      if (!IsInBounds(new Point(x, y))) throw new ArgumentOutOfRangeException("("+nameof(x)+","+nameof(y)+")", "(" + x.ToString() + "," + y.ToString() + ")", "!IsInBounds(x,y)");
#endif
      m_TileIDs[x, y] = (byte)(model.ID);
    }

    public void SetTileModelAt(Point pt, TileModel model)
    {
#if DEBUG
      if (!IsInBounds(pt)) throw new InvalidOperationException("!IsInBounds(pt)");
#endif
      m_TileIDs[pt.X, pt.Y] = (byte)(model.ID);
    }

    public TileModel GetTileModelAt(int x, int y) { return Models.Tiles[m_TileIDs[x,y]]; }
    public TileModel GetTileModelAt(Point pt) { return Models.Tiles[m_TileIDs[pt.X, pt.Y]]; }

    public KeyValuePair<TileModel?,Location> GetTileModelLocation(Point pt)
    {
      if (IsInBounds(pt)) return new KeyValuePair<TileModel?, Location>(Models.Tiles[m_TileIDs[pt.X, pt.Y]], new Location(this,pt));
      Location? loc = _Normalize(pt);   // XXX would have to handle out-of-bounds exits when building with peace walls
      if (null == loc) return default;
      return new KeyValuePair<TileModel?, Location>(loc.Value.Map.GetTileModelAt(loc.Value.Position), loc.Value);
    }

    // possibly denormalized versions
    /// <returns>null if and only if location is invalid rather than merely denormalized</returns>
    public TileModel? GetTileModelAtExt(Point pt)
    {
      if (IsInBounds(pt)) return Models.Tiles[m_TileIDs[pt.X, pt.Y]];   //      return GetTileModelAt(x,y);
      Location? loc = _Normalize(pt);
      if (null == loc) return null;
      return loc.Value.Map.GetTileModelAt(loc.Value.Position);
    }

    public bool TileIsWalkable(Point pt)
    {   // 2019-8-27 release mode IL Code size       108 (0x6c)
      // should evaluate: IsValid(pt) && GetTileModelAtExt(pt).IsWalkable
      if (IsInBounds(pt)) return Models.Tiles[m_TileIDs[pt.X, pt.Y]].IsWalkable;
      Location? loc = _Normalize(pt);
      return null != loc && loc.Value.Map.GetTileModelAt(loc.Value.Position).IsWalkable;
    }

    // thin wrappers based on Tile API
    public bool HasDecorationsAt(Point pt) { return m_Decorations.ContainsKey(pt); }

    public void DoForAllDecorationsAt(Point pt, Action<string> op)
    {
      if (m_Decorations.TryGetValue(pt, out var cache)) foreach(var x in cache) op(x);
    }

    public void AddDecorationAt(string imageID, in Point pt)
    {
      if (m_Decorations.TryGetValue(pt, out var ret)) {
        ret.Add(imageID);
      } else {
        m_Decorations.Add(pt, new HashSet<string>{ imageID });
      }
    }

    public bool HasDecorationAt(string imageID, in Point pt)
    {
      return m_Decorations.TryGetValue(pt, out var ret) && ret.Contains(imageID);
    }

    public void RemoveAllDecorationsAt(Point pt) { m_Decorations.Remove(pt); }

    public void RemoveDecorationAt(string imageID, in Point pt)
    {
      if (m_Decorations.TryGetValue(pt, out var ret)
          && ret.Remove(imageID)
          && 0 >= ret.Count)
          m_Decorations.Remove(pt);
    }

    public void AddTimedDecoration(Point pt, string imageId, int deltaT, Func<Map,Point,bool> test)
    {
      if (test(this, pt)) {
        if (!HasDecorationAt(imageId, in pt)) AddDecorationAt(imageId, in pt);
        bool not_yet = true;
        int ub = m_Timers.Count;
        while(0 <= --ub) {
          if (!(m_Timers[ub] is Engine.Tasks.TaskRemoveDecoration clean) || clean.WillRemove != imageId) continue;
          // VAPORWARE some sort of handling re stacked decaying decorations
          if (clean.TurnsLeft >= deltaT) {  // not formally correct, but for our current use case the > doesn't actually happen
            clean.Add(pt);
            not_yet = false;
          } else if (clean.Remove(pt)) m_Timers.RemoveAt(ub);
        }
        if (not_yet) m_Timers.Add(new Engine.Tasks.TaskRemoveDecoration(deltaT, in pt, imageId));
      }
    }


    public bool HasExitAt(in Point pos) { return m_Exits.ContainsKey(pos); }

    public bool HasExitAtExt(Point pos)
    {   // 2019-08-27 release mode IL Code size       72 (0x48) [invalidated]
      if (IsInBounds(pos)) return HasExitAt(in pos);
      Location? test = _Normalize(pos);
      return null != test && test.Value.Map.HasExitAt(test.Value.Position);
    }

    public Exit? GetExitAt(Point pos)
    {
      if (m_Exits.TryGetValue(pos, out var exit)) return exit;
      return null;
    }

    public Dictionary<Location,Exit> GetExits(Predicate<Exit> test) {
      var ret = new Dictionary<Location, Exit>();
      foreach(var x in m_Exits) {
        if (test(x.Value)) {
          var loc = new Location(this, x.Key);
          if (!Canonical(ref loc)) continue; // \todo invariant violation if this fails, but we need to guarantee normal form
          ret.Add(loc, x.Value);
        }
      }
      return ret;
    }

    public bool AnyExits(Predicate<Exit> test) {
      foreach(var x in m_Exits) {
        if (test(x.Value)) return true;
      }
      return false;
    }

#if DEAD_FUNC
    public Dictionary<Point,Exit> ExitsFor(Map dest) {
      var ret = new Dictionary<Point, Exit>();
      foreach(var x in m_Exits) {
        if (x.Value.ToMap == dest) ret.Add(x.Key, x.Value);
      }
      return ret;
    }
#endif

    public KeyValuePair<Point,Exit>? FirstExitFor(Map dest) {
      foreach(var x in m_Exits) if (x.Value.ToMap == dest) return x;
      return null;
    }

    public List<Point> GetEdge()    // \todo refactor to a cache variable setter
    {
      var ret = new List<Point>();
      bool explicit_edge = false;
      foreach(var x in m_Exits) {
        if (IsInBounds(x.Key)) ret.Add(x.Key);
        else explicit_edge = true;
      }
      if (explicit_edge) {
        for(short x = 0; x<Width; x++) {
          Point test = new Point(x, Height - 1);
          if (GetTileModelAt(test).IsWalkable) ret.Add(test);
          test.Y = 0;
          if (GetTileModelAt(test).IsWalkable) ret.Add(test);
        }
        for(short y = 1; y<Height-1; y++) {
          Point test = new Point(Width - 1, y);
          if (GetTileModelAt(test).IsWalkable) ret.Add(test);
          test.X = 0;
          if (GetTileModelAt(test).IsWalkable) ret.Add(test);
        }
      }
      return ret;
    }

    public void ForEachExit(Action<Point,Exit> op)
    {
       foreach(var x in m_Exits) op(x.Key,x.Value);
    }

    public void SetExitAt(Point pos, Exit exit) {
      m_Exits.Add(pos, exit);
    }

#if DEAD_FUNC
    public void RemoveExitAt(Point pos)
    {
      m_Exits.Remove(pos);
    }
#endif

    public bool HasAnExitIn(Rectangle rect)
    {
      return rect.Any(pt => HasExitAt(in pt));
    }

    public static int PathfinderMoveCosts(ActorAction act)
    {
        static int teardown_turns(MapObject obj) {
		    int cost = 1;
            if (obj is DoorWindow door && 0<door.BarricadePoints) cost += (door.BarricadePoints+7)/8;	// handwave time cost for fully rested unarmed woman with infinite stamina
            else cost += (obj.HitPoints+7)/8;	// time cost to break, as per barricade
            return cost;
        }

        if (act is Engine.Actions.Resolvable delta) return PathfinderMoveCosts(delta.ConcreteAction);
        if (act is Engine.Actions.ActionShove) return 4;    // impolite so penalize just more than walking around
        if (   act is Engine.Actions.ActionOpenDoor  // extra turn
            || act is Engine.Actions.ActionPush  // assume non-moving i.e. extra turn; also costs stamina
            || act is Engine.Actions.ActionPull  // extra turn; also costs stamina
            || act is Engine.Actions.ActionSwitchPlace // yes, this is hard-coded as 2 standard actions
            || act is Engine.Actions.ActionSwitchPlaceEmergency)
            return 2;
        if (act is Engine.Actions.ActionBashDoor bash) return teardown_turns(bash.Target);
        if (act is Engine.Actions.ActionBreak act_break) return teardown_turns(act_break.Target);
        return 1;  // normal case
    }

    private static Dictionary<Location,int> OneStepForPathfinder(in Location loc, Actor a, Dictionary<Location,ActorAction> already)
	{  // 2019-08-26 release mode IL Code size       99 (0x63) [invalidated]
	  var ret = new Dictionary<Location, int>();
      Dictionary<Location, ActorAction> moves = a.OnePath(in loc, already);
      foreach(var move in moves) {
        var pt = move.Key;
        if (1>= a.Controller.FastestTrapKill(in pt)) continue;
        ret.Add(pt, PathfinderMoveCosts(move.Value));
      }
	  return ret;
	}

    private Dictionary<Point,int> OneStepForPathfinder(Point pt, Actor a, Dictionary<Point,ActorAction> already)
	{ // 2019-09-07 release mode IL Code size       121 (0x79)
	  var ret = new Dictionary<Point, int>();
      Dictionary<Point,ActorAction> moves = a.OnePath(this, in pt, already);
      foreach(var move in moves) {
        var pt2 = move.Key;
        if (1>= a.Controller.FastestTrapKill(new Location(a.Location.Map, pt2))) continue;
        ret.Add(pt2, PathfinderMoveCosts(move.Value));
      }
	  return ret;
	}
#nullable restore

    public bool WouldBlacklistFor(Point pt,Actor actor,bool is_real=false)
    {
      if (pt == actor.Location.Position && this == actor.Location.Map) return false;
      if (actor.Model.Abilities.AI_CanUseAIExits) {
        if (Gameplay.AI.ObjectiveAI.VetoExit(actor, GetExitAt(pt))) return true;
      }

      var dest = new Location(this, pt);
      if (1 == Engine.Rules.InteractionDistance(dest, actor.Location)) {
        if (actor.Controller is Gameplay.AI.ObjectiveAI oai && oai.LegalPathingIsValid) return null == oai.LegalPathing(dest);
      }

      if (actor.CanEnter(dest)) return false;
      if (   1 == Engine.Rules.InteractionDistance(dest, actor.Location) // \todo is this morally dead code?
          && null == Engine.Rules.IsPathableFor(actor, dest)) return true;
      // generators may not be entered, but are still (unreliably) pathable
      if (GetMapObjectAtExt(pt) is Engine.MapObjects.PowerGenerator) return false;
#if OBSOLETE
      // most of the following is likely obsolete, if not all
      if (null != Engine.Rules.IsPathableFor(actor, new Location(this, pt))) return false;
      var mapobj = GetMapObjectAtExt(pt);
      if (null!=mapobj) {
        Location loc = new Location(this, pt);
        if (mapobj.IsContainer) {
          var inv = GetItemsAt(pt);
          if (null==inv || inv.IsEmpty) {
            // cheating ai: update item memory immediately since we had to check anyway
            if (is_real) actor.Controller.ItemMemory?.Set(loc,null,LocalTime.TurnCounter);
          } else if (actor.Location.Map!=this) return false;  // not correct, but the correct test below is using a class that assumes same-map
          else if (actor.Controller is Gameplay.AI.OrderableAI ai && null!=ai.WouldGrabFromAccessibleStack(loc, inv)) return false;
        }
        if (mapobj is Engine.MapObjects.PowerGenerator) return false;
        if (mapobj is DoorWindow) return false;
      }
#endif
      return true;
    }

	public Zaimoni.Data.FloodfillPathfinder<Location> PathfindLocSteps(Actor actor)
	{
      var already = new Dictionary<Location,ActorAction>();

      Dictionary<Location,int> fn(Location loc) { return OneStepForPathfinder(in loc, actor, already); }

	  var ret = new Zaimoni.Data.FloodfillPathfinder<Location>(fn, fn, loc => actor.StrictCanEnter(in loc));
      Rect.DoForEach(pt => ret.Blacklist(new Location(this, pt)), pt => WouldBlacklistFor(pt, actor, true));
      return ret;
    }

    // Default pather.  Recovery options would include allowing chat, and allowing pushing.
	public Zaimoni.Data.FloodfillPathfinder<Point> PathfindSteps(Actor actor)
	{
      var already = new Dictionary<Point,ActorAction>();

      Dictionary<Point,int> fn(Point pt) { return OneStepForPathfinder(pt, actor, already); }

	  var ret = new Zaimoni.Data.FloodfillPathfinder<Point>(fn, fn, (pt=> IsInBounds(pt)));
      Rect.DoForEach(pt => ret.Blacklist(pt), pt => WouldBlacklistFor(pt, actor, true));
      return ret;
    }

    // for AI pathing, currently.
    private HashSet<Map> _PathTo(Map dest, out HashSet<Exit> exits)
    { // disallow secret maps
	  // should be at least one by construction
	  exits = new HashSet<Exit>(m_Exits.Values.Where(e => e.IsNotBlocked()));
	  var exit_maps = new HashSet<Map>(destination_maps.Get);
      if (1>=exit_maps.Count) return exit_maps;
retry:
      if (exit_maps.Contains(dest)) {
        exit_maps.Clear();
        exit_maps.Add(dest);
        exits.RemoveWhere(e => e.ToMap!=dest);
        return exit_maps;
      }
      exit_maps.RemoveWhere(m=> 1==m.destination_maps.Get.Count);
      exits.RemoveWhere(e => !exit_maps.Contains(e.ToMap));
      if (1>=exit_maps.Count) return exit_maps;

	  var dest_exit_maps = new HashSet<Map>(dest.destination_maps.Get);
      if (1 == dest_exit_maps.Count) {
        dest = dest_exit_maps.ToList()[0];
        goto retry;
      }

      dest_exit_maps.IntersectWith(exit_maps);
      if (1 <= dest_exit_maps.Count) {
        dest = dest_exit_maps.ToList()[0];
        goto retry;
      }

      // special area navigation
      {
      KeyValuePair<Map,Map>? src_alt = Engine.Session.Get.UniqueMaps.NavigatePoliceStation(this);
      KeyValuePair<Map,Map>? dest_alt = Engine.Session.Get.UniqueMaps.NavigatePoliceStation(dest);
      if (null != src_alt && null == dest_alt) {    // probably dead code
        dest = src_alt.Value.Key;
        goto retry;
      }
      }
      {
      KeyValuePair<Map,Map>? src_alt = Engine.Session.Get.UniqueMaps.NavigateHospital(this);
      KeyValuePair<Map,Map>? dest_alt = Engine.Session.Get.UniqueMaps.NavigateHospital(dest);
      if (null != src_alt && null == dest_alt) {
        dest = src_alt.Value.Key;
        goto retry;
      }
      }

      if (dest.DistrictPos != DistrictPos) {
        int dest_extended = District.UsesCrossDistrictView(dest);
        if (0 == dest_extended) {
          dest = dest.District.EntryMap;
          goto retry;
        }
        if (3 == dest_extended && dest.Exits.Any(e => e.ToMap == dest.District.EntryMap)) {
          dest = dest.District.EntryMap;
          goto retry;
        }
        int this_extended = District.UsesCrossDistrictView(this);
        if (0==this_extended) {
          dest = District.EntryMap;
          goto retry;
        }
        if (3 == this_extended && Exits.Any(e => e.ToMap == District.EntryMap)) {
          dest = District.EntryMap;
          goto retry;
        }
        if (2==dest_extended && 2==this_extended) {
          dest = District.EntryMap;
          goto retry;
        }
        if (1==dest_extended && 2==this_extended) {
          dest = District.EntryMap;
          goto retry;
        }
        if (2==dest_extended && 1==this_extended) {
          dest = dest.District.EntryMap;
          goto retry;
        }
        if (1==dest_extended && 1==this_extended) {
          int x_delta = dest.DistrictPos.X - DistrictPos.X;
          int y_delta = dest.DistrictPos.Y - DistrictPos.Y;
          int abs_x_delta = (0<=x_delta ? x_delta : -x_delta);
          int abs_y_delta = (0<=y_delta ? y_delta : -y_delta);
          int sgn_x_delta = (0<=x_delta ? (0 == x_delta ? 0 : 1) : -1);
          int sgn_y_delta = (0<=y_delta ? (0 == y_delta ? 0 : 1) : -1);
          if (abs_x_delta<abs_y_delta) {
            dest = Engine.Session.Get.World[DistrictPos.X, DistrictPos.Y + sgn_y_delta].EntryMap;
            goto retry;
          } else if (abs_x_delta > abs_y_delta) {
            dest = Engine.Session.Get.World[DistrictPos.X + sgn_x_delta, DistrictPos.Y].EntryMap;
            goto retry;
          } else if (2 <= abs_x_delta) {
            dest = Engine.Session.Get.World[DistrictPos.X + sgn_x_delta, DistrictPos.Y + sgn_y_delta].EntryMap;
            goto retry;
          } else return exit_maps;  // no particular insight, not worth a debug crash
        }
      }
#if DEBUG
      if (dest.DistrictPos != DistrictPos) throw new InvalidOperationException("test case: cross-district map not handled");
#endif
      // no particular insight
      return exit_maps;
    }

    // for AI pathing, currently.
    public HashSet<Map> PathTo(Map dest, out HashSet<Exit> exits)
    {
      HashSet<Map> exit_maps = _PathTo(dest,out exits);
      if (1>=exit_maps.Count) return exit_maps;

      HashSet<Map> inv_exit_maps = dest._PathTo(this,out HashSet<Exit> inv_exits);

      var intersect = new HashSet<Map>(exit_maps);
      intersect.IntersectWith(inv_exit_maps);
      if (0<intersect.Count) {
        exit_maps = intersect;
        exits.RemoveWhere(e => !exit_maps.Contains(e.ToMap));
        if (1>=exit_maps.Count) return exit_maps;
      }

#if FAIL
      // XXX topology of these special locations has to be accounted for as they're more than 1 level deep
      bool is_special = name.StartsWith("Police Station - ");
      bool dest_is_special = name.StartsWith("Police Station - ");
      // ...
      bool is_special = name.StartsWith("Hospital - ");
      bool dest_is_special = name.StartsWith("Hospital - ");
      // ...
#endif

      // do something uninteillgent
      return exit_maps;
    }

#nullable enable
#region zones: map generation support
    public void AddZone(Zone zone) { m_Zones.Add(zone); }
    public void RemoveZone(Zone zone) { m_Zones.Remove(zone); }

    public void RemoveAllZonesAt(Point pt)
    {
      var zonesAt = GetZonesAt(pt);
      if (null != zonesAt) foreach (var zone in zonesAt) RemoveZone(zone);
    }
#endregion

    /// <remark>shallow copy needed to be safe for foreach loops</remark>
    /// <returns>null, or a non-empty list of zones</returns>
    public List<Zone>? GetZonesAt(Point pt)
    {
      var zoneList = m_Zones.FindAll(z => z.Bounds.Contains(pt));
      return (0<zoneList.Count) ? zoneList : null;
    }

    public List<Zone>? GetZonesByPartialName(string partOfname)
    {
      var ret = m_Zones.Where(z => z.Name.Contains(partOfname));
      return ret.Any() ? ret.ToList() : null;
    }

    public Zone? GetZoneByPartialName(string partOfname)
    {
      foreach(var z in m_Zones) if (z.Name.Contains(partOfname)) return z;
      return null;
    }

    public bool HasZonePartiallyNamedAt(Point pos, string partOfName)
    {
      return GetZonesAt(pos)?.Any(zone=>zone.Name.Contains(partOfName)) ?? false;
    }

    public ZoneLoc? ClearableZoneAt(Point pt) {
      foreach (var x in m_ClearableZones!) if (x.Key.Contains(pt)) return x.Value;
      return null;
    }

    public void OnMapGenerated()
    { // coordinates with StdTownGenerator::Generate
      // 1) flush all NoCivSpawn zones
      m_Zones.OnlyIfNot(z => "NoCivSpawn" == z.Name);
    }

    // assumes world has been substantially loaded/generated (in particular, real-time extending a map would require
    // invalidating some of this)
    public void RegenerateZoneExits()
    {
      // whole-map data
      if (null == m_CanonicalZones) {
        m_CanonicalZones = new Dictionary<Rectangle,ZoneLoc>();
        var staging = new List<ZoneLoc>();
        ZoneLoc? test = null;
        foreach(var z in m_Zones) {
          if (m_CanonicalZones.ContainsKey(z.Bounds)) continue;
          test = new ZoneLoc(this, z.Bounds);
          m_CanonicalZones.Add(z.Bounds, test);
          int ub = staging.Count;
          while(0 <= --ub) {
            if (test.Rect.Contains(staging[ub].Rect)) { // if we contain another zone, we are not clearable
              test = null;
              break;
            }
            if (staging[ub].Rect.Contains(test.Rect)) { // if another zone contains us, it is not clearable
              staging.RemoveAt(ub);
              continue;
            }
          }
          if (null != test) staging.Add(test);
        }
        m_ClearableZones = new Dictionary<Rectangle,ZoneLoc>();
        foreach(var z in staging) m_ClearableZones.Add(z.Rect, z);
      }
      // per-zone data
      foreach(var z in m_Zones) {
        if (z.VolatileAttribute.HasKey("exits")) continue;
        var locs = new HashSet<Location>();
        var staging = m_CanonicalZones[z.Bounds];
        var walking = staging.WalkOut();
        if (null != walking) locs.UnionWith(walking.Select(act => act.dest));
        var vertical = staging.grep(loc => null != loc.Exit);
        if (null != vertical) locs.UnionWith(vertical.Select(loc => loc.Exit!.Location));
        z.VolatileAttribute.Set("exits", locs.ToArray());
        var zones = new Dictionary<Map,HashSet<Zone>>(); // using default pointer-equality, so duplicate coordinates aren't deduplicated
        foreach(var loc in locs) {
          var dest_zones = loc.Map.GetZonesAt(loc.Position);
          if (null != dest_zones) {
            if (!zones.TryGetValue(loc.Map, out var cache)) zones.Add(loc.Map,(cache = new HashSet<Zone>()));
            foreach(var zone in dest_zones) cache.Add(zone);
          }
        }
        var ordered_zones = new List<ZoneLoc>();
        var order_staging = new Dictionary<int, List<Zone>>();
        foreach(var mapzone in zones) {
          foreach(var x in mapzone.Value) {
            var area = (x.Bounds.Right-x.Bounds.Left)*(x.Bounds.Bottom-x.Bounds.Top);
            if (!order_staging.TryGetValue(area, out var cache)) order_staging.Add(area, (cache = new List<Zone>()));
            cache.Add(x);
          }
          while(0 < order_staging.Count) {
            int index = order_staging.Keys.Max();
            foreach(var x in order_staging[index]) ordered_zones.Add(new ZoneLoc(mapzone.Key, x.Bounds));
            order_staging.Remove(index);
          }
        }
        z.VolatileAttribute.Set("exit_zones", ordered_zones.ToArray());
      }
    }

    public void ActorPositionHull(ref Span<Point> hull)
    {
        hull[0] = Extent;
        hull[1] = Point.Empty;
        foreach(Actor a in m_ActorsList) {
            if (a.Location.Position.X < hull[0].X) hull[0].X = a.Location.Position.X;
            if (a.Location.Position.X > hull[1].X) hull[1].X = a.Location.Position.X;
            if (a.Location.Position.Y < hull[0].Y) hull[0].Y = a.Location.Position.Y;
            if (a.Location.Position.Y > hull[1].Y) hull[1].Y = a.Location.Position.Y;
        }
        if (hull[0].X <= hull[1].X) hull[1] += Direction.SE;
    }

    public bool RequiresUI(Point delta) {
        Span<Point> hull = stackalloc Point[2];
        ActorPositionHull(ref hull);
        if (hull[0].X <= hull[1].X) {
            var zone = new ZoneLoc(this, new Rectangle(hull[0] - delta, (hull[1] - hull[0]) + 2*delta));
            var canon = zone.GetCanonical;
            if (null != canon) {
                foreach(var z in canon) {
                    if (z.m == this) continue;
                    var players = z.m.Players.Get;
                    foreach(var a in players) {
                        if (z.Contains(a.Location)) return true;
                    }
                }
            }
        }
        return false;
    }

    public void DoForAllActors(Action<Actor> op) { foreach(Actor a in m_ActorsList) op(a); }

    public void DoForAllInventory(Action<Inventory> op)
    {
      foreach (var x in m_GroundItemsByPosition) op(x.Value);
      foreach (var actor in m_ActorsList) {
        var inv = actor.Inventory;
        if (null != inv) op(inv);
      }
    }

    public void DoForAllGroundInventories(Action<Location,Inventory> op)
    {
      foreach (var x in m_GroundItemsByPosition) op(new Location(this, x.Key), x.Value);
    }

    public int SumOverAllInventory(Func<Inventory,int> xform)
    {
      int ret = 0;

      foreach (var x in m_GroundItemsByPosition) ret += xform(x.Value);
      foreach (var actor in m_ActorsList) {
        var inv = actor.Inventory;
        if (null != inv) ret += xform(inv);
      }

      return ret;
    }

    public bool DoForOneInventory(Func<Inventory,Location,bool> test)
    {
      foreach (var x in m_GroundItemsByPosition) {
        if (test(x.Value,new Location(this,x.Key))) {
          if (x.Value.IsEmpty) m_GroundItemsByPosition.Remove(x.Key);
          return true;
        }
      }
      foreach (var actor in m_ActorsList) {
        var inv = actor.Inventory;
        if (null != inv && test(inv,actor.Location)) return true;
      }
      return false;
    }

    // Actor manipulation functions
    public bool HasActor(Actor actor) { return m_ActorsList.Contains(actor); }

    public Actor? GetActorAt(Point position)
    {
      if (m_aux_ActorsByPosition.TryGetValue(position, out var actor)) return actor;
      return null;
    }

    public Actor? GetActorAtExt(Point pt)
    {
      if (IsInBounds(pt)) return GetActorAt(pt);
      Location? test = _Normalize(pt);
      return null == test ? null : test.Value.Map.GetActorAt(test.Value.Position);
    }

    public bool StrictHasActorAt(Point pt) { return m_aux_ActorsByPosition.ContainsKey(pt); }

    public bool HasActorAt(in Point pt)
    {   // 2019-08-27 release mode IL Code size       87 (0x57) [invalidated]
      if (m_aux_ActorsByPosition.ContainsKey(pt)) return true;
      if (IsInBounds(pt)) return false;
      Location? tmp = _Normalize(pt);
      return null != tmp && tmp.Value.Map.m_aux_ActorsByPosition.ContainsKey(tmp.Value.Position);
    }

    public void Recalc(Actor actor)
    {
      if (actor.IsPlayer) Players.Recalc();
      if (actor.IsFaction(Gameplay.GameFactions.IDs.ThePolice)) Police.Recalc();
    }

    public void PlaceAt(Actor actor, in Point position)
    {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
      var actorAt = GetActorAt(position);
      if (null != actorAt) throw new ArgumentOutOfRangeException(nameof(position),position, (actorAt == actor ? "actor already at position" : "another actor already at position"));
#endif
      lock(m_aux_ActorsByPosition) {
        // test game behaved rather badly when a second Samantha Collins was imprisoned on turn 0
        bool knows_on_map = actor.Location.Map == this;
        bool already_on_map = m_ActorsList.Contains(actor);
        if (already_on_map) {
          if (!knows_on_map) throw new InvalidOperationException(actor.Name+" did not know s/he was in the map");
#if AUDIT_ACTOR_MOVEMENT
          if (m_ActorsList.IndexOf(actor)<m_ActorsList.LastIndexOf(actor)) throw new InvalidOperationException(actor.Name + " is double-included");
          if (!m_aux_ActorsByPosition.ContainsKey(actor.Location.Position)) {
            foreach(var x in m_aux_ActorsByPosition) {
              if (x.Value==actor) new InvalidOperationException("map location cache out of sync");
            }
            throw new InvalidOperationException("map location cache out of sync");
          }
          if (m_aux_ActorsByPosition[actor.Location.Position]!=actor) {
            foreach(var x in m_aux_ActorsByPosition) {
             if (x.Value==actor) new InvalidOperationException("map location cache out of sync");
            }
            throw new InvalidOperationException("map location cache out of sync");
          }
#endif
          m_aux_ActorsByPosition.Remove(actor.Location.Position);
        } else {
#if AUDIT_ACTOR_MOVEMENT
          foreach(var x in m_aux_ActorsByPosition) {
           if (x.Value==actor) new InvalidOperationException("map location cache out of sync");
          }
#endif
          if (!knows_on_map) actor.RemoveFromMap();
          m_ActorsList.Add(actor);
          Engine.LOS.Now(this);
          Recalc(actor);
        }
        m_aux_ActorsByPosition.Add(position, actor);
        actor.Location = new Location(this, position);
      } // lock(m_aux_ActorsByPosition)
      m_iCheckNextActorIndex = 0;
    }

    public void MoveActorToFirstPosition(Actor actor)
    {
#if DEBUG
      if (!m_ActorsList.Contains(actor)) throw new ArgumentException("actor not in map");
#endif
#if AUDIT_ACTOR_MOVEMENT
      if (m_ActorsList.IndexOf(actor)<m_ActorsList.LastIndexOf(actor)) throw new InvalidOperationException(actor.Name + " is double-included");
#endif
      if (1 == m_ActorsList.Count) return;
      m_ActorsList.Remove(actor);
      m_ActorsList.Insert(0, actor);
      m_iCheckNextActorIndex = 0;
      Recalc(actor);
    }

    public void Remove(Actor actor)
    {
#if DEBUG
      // why you *really* should be using Actor::RemoveFromMap()
      if (this!=actor.Location.Map) throw new InvalidOperationException(actor.Name + " does not think he is in map to be removed from");
#endif
      lock(m_aux_ActorsByPosition) {
#if AUDIT_ACTOR_MOVEMENT
        if (m_ActorsList.IndexOf(actor)<m_ActorsList.LastIndexOf(actor)) throw new InvalidOperationException(actor.Name+" is double-included");
#endif
        if (m_ActorsList.Remove(actor)) {
#if AUDIT_ACTOR_MOVEMENT
          if (!m_aux_ActorsByPosition.ContainsKey(actor.Location.Position)) {
            foreach(var x in m_aux_ActorsByPosition) {
              if (x.Value==actor) new InvalidOperationException("map location cache out of sync");
            }
            throw new InvalidOperationException("map location cache out of sync");
          }
          if (m_aux_ActorsByPosition[actor.Location.Position]!=actor) {
            foreach(var x in m_aux_ActorsByPosition) {
              if (x.Value==actor) new InvalidOperationException("map location cache out of sync");
            }
            throw new InvalidOperationException("map location cache out of sync");
          }
#endif
          m_aux_ActorsByPosition.Remove(actor.Location.Position);
          m_iCheckNextActorIndex = 0;
          Recalc(actor);
        }
#if AUDIT_ACTOR_MOVEMENT
        foreach(var x in m_aux_ActorsByPosition) {
          if (x.Value == actor) throw new InvalidOperationException(actor.Name+" still in position cache");
        }
#endif
      }
    }

    public Actor? NextActorToAct {
      get {
        int countActors = m_ActorsList.Count;
        // use working copy of m_iCheckNextActorIndex to mitigate multi-threading issues
        for (int checkNextActorIndex = m_iCheckNextActorIndex; checkNextActorIndex < countActors; ++checkNextActorIndex) {
          var actor = m_ActorsList[checkNextActorIndex];
          if (actor.CanActThisTurn && !actor.IsSleeping) {
            m_iCheckNextActorIndex = checkNextActorIndex;
            return actor;
          }
        }
        m_iCheckNextActorIndex = countActors;
        return null;
      }
    }

    // 2019-01-24: profiling indicates this is a cache target, but CPU cost of using cache ~25% greater than not having one
    private string ReasonNotWalkableFor(Point pt, ActorModel model)
    {
      var tile_loc = GetTileModelLocation(pt);
      if (null == tile_loc.Key) return "out of map";
      if (!tile_loc.Key.IsWalkable) return "blocked";
      var mapObjectAt = tile_loc.Value.MapObject;
      if (null != mapObjectAt && !mapObjectAt.IsWalkable) {
        if (mapObjectAt.IsJumpable) {
          if (!model.Abilities.CanJump) return "cannot jump";
        } else if (model.Abilities.IsSmall) {
          if (mapObjectAt is DoorWindow doorWindow && doorWindow.IsClosed) return "cannot slip through closed door";
        } else return "blocked by object";
      }
      if (tile_loc.Value.StrictHasActorAt) return "someone is there";  // XXX includes actor himself
      return "";
    }

    public bool IsWalkableFor(Point p, ActorModel model)
    {
      return string.IsNullOrEmpty(ReasonNotWalkableFor(p, model));
    }

    public bool IsWalkableFor(Point p, ActorModel model, out string reason)
    {
      reason = ReasonNotWalkableFor(p, model);
      return string.IsNullOrEmpty(reason);
    }

    private string ReasonNotWalkableFor(Point pt, Actor actor)
    {
      var tile_loc = GetTileModelLocation(pt);
      if (null == tile_loc.Key) return "out of map";
      if (!tile_loc.Key.IsWalkable) return "blocked";
      var mapObjectAt = tile_loc.Value.MapObject;
      if (null != mapObjectAt && !mapObjectAt.IsWalkable) {
        if (mapObjectAt.IsJumpable) {
          if (!actor.CanJump) return "cannot jump";
          // We only have to be completely accurate when adjacent to a square.
          if (actor.StaminaPoints < Engine.Rules.STAMINA_COST_JUMP && Engine.Rules.IsAdjacent(actor.Location,new Location(this,pt))) return "not enough stamina to jump";
        } else if (actor.Model.Abilities.IsSmall) {
          if (mapObjectAt is DoorWindow doorWindow && doorWindow.IsClosed) return "cannot slip through closed door";
        } else return "blocked by object";
      }
      // 1) does not have to be accurate except when adjacent
      // 2) treat null map as "omni-adjacent" (happens during spawning)
      if (tile_loc.Value.StrictHasActorAt && (null==actor.Location.Map || Engine.Rules.IsAdjacent(actor.Location,tile_loc.Value))) return "someone is there";  // XXX includes actor himself
      if (actor.DraggedCorpse != null && actor.IsTired) return "dragging a corpse when tired";
      return "";
    }

    public bool IsWalkableFor(Point p, Actor actor)
    {
      return string.IsNullOrEmpty(ReasonNotWalkableFor(p, actor));
    }

    public bool IsWalkableFor(Point p, Actor actor, out string reason)
    {
      reason = ReasonNotWalkableFor(p, actor);
      return string.IsNullOrEmpty(reason);
    }

    // AI-ish, but essentially a map geometry property
    // we are considering a non-jumpable pushable object here (e.g. shop shelves)
    public bool PushCreatesSokobanPuzzle(Point dest,Actor actor)
    { // 2019-08-27 release mode IL Code size       703 (0x2bf) [invalidated]
      if (HasExitAt(in dest)) return true;   // this just isn't a good idea for pathing

      Span<bool> is_wall = stackalloc bool[8];   // these default-initialize to false
      Span<bool> blocked = stackalloc bool[8];
      Span<bool> no_go = stackalloc bool[8];
      sbyte dir;
      foreach(Point pt2 in dest.Adjacent()) {
        if (actor.Location.Map==this && actor.Location.Position==pt2) continue;
        if (IsWalkableFor(pt2,actor.Model)) continue;   // not interested in stamina for this
        no_go[dir = Direction.FromVector(pt2 - dest).Index] = true;    // non-null by construction
        (TileIsWalkable(pt2) ? blocked : is_wall)[dir] = true;
      }
      // corners and walls are generally ok.  2019-01-04: preliminary tests suggest this is not a micro-optimization target
      if (is_wall[(int)Compass.XCOMlike.NW] && is_wall[(int)Compass.XCOMlike.N] && is_wall[(int)Compass.XCOMlike.NE] && !is_wall[(int)Compass.XCOMlike.S] && (!is_wall[(int)Compass.XCOMlike.E] || !is_wall[(int)Compass.XCOMlike.W])) return false;
      if (is_wall[(int)Compass.XCOMlike.SW] && is_wall[(int)Compass.XCOMlike.S] && is_wall[(int)Compass.XCOMlike.SE] && !is_wall[(int)Compass.XCOMlike.N] && (!is_wall[(int)Compass.XCOMlike.E] || !is_wall[(int)Compass.XCOMlike.W])) return false;
      if (is_wall[(int)Compass.XCOMlike.NW] && is_wall[(int)Compass.XCOMlike.W] && is_wall[(int)Compass.XCOMlike.SW] && !is_wall[(int)Compass.XCOMlike.E] && (!is_wall[(int)Compass.XCOMlike.N] || !is_wall[(int)Compass.XCOMlike.S])) return false;
      if (is_wall[(int)Compass.XCOMlike.NE] && is_wall[(int)Compass.XCOMlike.E] && is_wall[(int)Compass.XCOMlike.SE] && !is_wall[(int)Compass.XCOMlike.W] && (!is_wall[(int)Compass.XCOMlike.N] || !is_wall[(int)Compass.XCOMlike.S])) return false;

      // blocking access to something that could be next to wall/corner is problematic
      if (blocked[(int)Compass.XCOMlike.N] && blocked[(int)Compass.XCOMlike.W] && no_go[(int)Compass.XCOMlike.NW] 
          && (no_go[(int)Compass.XCOMlike.NE] || no_go[(int)Compass.XCOMlike.SW])) return true;
      if (blocked[(int)Compass.XCOMlike.N] && blocked[(int)Compass.XCOMlike.E] && no_go[(int)Compass.XCOMlike.NE] 
          && (no_go[(int)Compass.XCOMlike.NW] || no_go[(int)Compass.XCOMlike.SE])) return true;
      if (blocked[(int)Compass.XCOMlike.S] && blocked[(int)Compass.XCOMlike.W] && no_go[(int)Compass.XCOMlike.SW]
          && (no_go[(int)Compass.XCOMlike.SE] || no_go[(int)Compass.XCOMlike.NW])) return true;
      if (blocked[(int)Compass.XCOMlike.S] && blocked[(int)Compass.XCOMlike.E] && no_go[(int)Compass.XCOMlike.SE]
          && (no_go[(int)Compass.XCOMlike.SW] || no_go[(int)Compass.XCOMlike.NE])) return true;

      return false;
    }

    // tracking players on map
    public int PlayerCorpseCount {
      get {
        int now = Engine.Session.Get.WorldTime.TurnCounter;
        return m_CorpsesList.Count(c => c.DeadGuy.IsPlayer && Corpse.ZOMBIFY_DELAY<= now - c.Turn);    // align with Corpse::ZombifyChance
      }
    }

    public int PlayerCount { get { return Players.Get.Count; } }

    public Actor? FindPlayer {
      get {
        var pl_list = Players.Get;
        return (0<pl_list.Count) ? pl_list[0] : null;
      }
    }

    public bool MessagePlayerOnce(Action<Actor> fn, Func<Actor, bool>? pred =null)
    {
      void pan_to(Actor a) {
          RogueForm.Game.PanViewportTo(a);
          fn(a);
      };

      return (null == pred ? Players.Get.ActOnce(pan_to)
                           : Players.Get.ActOnce(pan_to, pred));
    }

    // map object manipulation functions
    public bool HasMapObject(MapObject x) { return m_MapObjectsList.Contains(x); }

    public MapObject? GetMapObjectAt(Point pos)
    {
      if (m_aux_MapObjectsByPosition.TryGetValue(pos, out var mapObject)) {
#if DEBUG
        // existence check for bugs relating to map object location
        if (this!=mapObject.Location.Map) throw new InvalidOperationException("map object and map disagree on map");
        if (pos!=mapObject.Location.Position) throw new InvalidOperationException("map object and map disagree on position");
#endif
        return mapObject;
      }
      return null;
    }

    public MapObject? GetMapObjectAtExt(Point pt)
    {   // 2019-08-27 release mode IL Code size       72 (0x48)
      if (IsInBounds(pt)) return GetMapObjectAt(pt);
      Location? test = _Normalize(pt);
      return null == test ? null : test.Value.Map.GetMapObjectAt(test.Value.Position);
    }

    public bool HasMapObjectAt(Point position)
    {
      return m_aux_MapObjectsByPosition.ContainsKey(position);
    }

    public void PlaceAt(MapObject mapObj, Point position)
    {
      if (!IsInBounds(position)) {
        // cross-map push or similar
        Location? test = _Normalize(position);
#if DEBUG
        if (null == test) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsValid(position)");
#endif
        test.Value.Map.PlaceAt(mapObj,test.Value.Position); // intentionally not using thin wrapper
        return;
      }
#if DEBUG
      if (!GetTileModelAt(position).IsWalkable) throw new ArgumentOutOfRangeException(nameof(position),position, "!GetTileModelAt(position).IsWalkable");
#endif
      var mapObjectAt = GetMapObjectAt(position);
      if (mapObjectAt == mapObj) return;
#if DEBUG
      if (null != mapObjectAt) throw new ArgumentOutOfRangeException(nameof(position), position, "null != GetMapObjectAt(position)");
#endif
      bool update_item_memory = mapObj.IsContainer && !mapObj.Inventory.IsEmpty;
      // cf Map::PlaceAt(Actor,Position)
      if (null != mapObj.Location.Map && HasMapObject(mapObj)) {
        if (update_item_memory) {
          if (Engine.Session.Get.Police.ItemMemory.HaveEverSeen(mapObj.Location))
            Engine.Session.Get.Police.Investigate.Record(mapObj.Location);   // XXX \todo should message based on item memories
        }
        m_aux_MapObjectsByPosition.Remove(mapObj.Location.Position);
      }
      else {
        if (null != mapObj.Location.Map && this != mapObj.Location.Map) mapObj.Remove();
        m_MapObjectsList.Add(mapObj);
      }
      m_aux_MapObjectsByPosition.Add(position, mapObj);
      mapObj.Location = new Location(this, position);
      if (update_item_memory) Engine.Session.Get.Police.Investigate.Record(mapObj.Location);
    }

    public void RemoveMapObjectAt(Point pt)
    {
      var mapObjectAt = GetMapObjectAt(pt);
      if (mapObjectAt == null) return;
      m_MapObjectsList.Remove(mapObjectAt);
      m_aux_MapObjectsByPosition.Remove(pt);
    }

    // this will need rethinking when off-ground inventory (chairs, tables) happens
    public bool IsTrapCoveringMapObjectAt(Point pos) { return GetMapObjectAt(pos)?.CoversTraps ?? false; }

    public int TrapsUnavoidableMaxDamageAtFor(Point pos, Actor a) {
      if (IsTrapCoveringMapObjectAt(pos)) return 0;
      return GetItemsAt(pos)?.TrapsMaxDamageFor(a) ?? 0;
    }

    public void OpenAllGates()
    {
      foreach(var obj in m_MapObjectsList) {
        if (MapObject.IDs.IRON_GATE_CLOSED != obj.ID) continue;
        obj.ID = MapObject.IDs.IRON_GATE_OPEN;
        RogueForm.Game.OnLoudNoise(obj.Location,this== Engine.Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap ? "cell opening" : "gate opening");
      }
    }

    public double PowerRatio { get {
        var gens = PowerGenerators.Get;
        return (double)(gens.Count(it => it.IsOn))/gens.Count;
    } }

    public bool HasItemsAt(Point pos) { return m_GroundItemsByPosition.ContainsKey(pos); }

    public Inventory? GetItemsAt(Point position)
    {
      if (m_GroundItemsByPosition.TryGetValue(position, out var inventory)) {
        if (!inventory.IsEmpty) return inventory;
        m_GroundItemsByPosition.Remove(position);
      }
      return null;
    }

    public Inventory? GetItemsAtExt(Point pt)
    {   // 2019-08-27 release mode IL Code size       72 (0x48) [invalidated]
      if (IsInBounds(pt)) return GetItemsAt(pt);
      Location? test = _Normalize(pt);
      return null == test ? null : test.Value.Items;
    }

    /// <summary>
    /// All inventories at that position, regardless of type.  Actors are not meant to be equally efficient at noticing inventories.
    /// </summary>
    static public List<Inventory>? AllItemsAt(Location loc, Actor? a=null)
    {
       if (!Map.Canonical(ref loc)) return null;
       var g_inv = loc.Items;
       var ret = (null == g_inv || g_inv.IsEmpty ? null : new List<Inventory> { g_inv });
       var obj = loc.MapObject;
       var o_inv = (null != obj && obj.IsContainer ? obj.Inventory : null);
       if (null != o_inv && !o_inv.IsEmpty) (ret ??= new List<Inventory>()).Add(o_inv);
       return ret;
    }

    static public List<Inventory>? AllItemsAt(Location loc, Predicate<Inventory> ok, Actor? a=null)
    {
       if (!Map.Canonical(ref loc)) return null;
       var g_inv = loc.Items;
       var ret = (null == g_inv || g_inv.IsEmpty || !ok(g_inv) ? null : new List<Inventory> { g_inv });
       var obj = loc.MapObject;
       var o_inv = (null != obj && obj.IsContainer ? obj.Inventory : null);
       if (null != o_inv && !o_inv.IsEmpty && ok(o_inv)) (ret ??= new List<Inventory>()).Add(o_inv);
       return ret;
    }

    public Dictionary<Point, Inventory> GetAccessibleInventories(Point pt)
    {
      var ground_inv = new Dictionary<Point, Inventory>();
      var inv = GetItemsAtExt(pt);
      if (null != inv && !inv.IsEmpty) ground_inv.Add(pt, inv);
      foreach(var adjacent in pt.Adjacent()) {
        var loc = new Location(this, adjacent);
        if (!Canonical(ref loc)) continue;
        var obj = loc.MapObject;
        if (null != obj && obj.IsContainer) {
          inv = obj.Inventory;
          if (null != inv && !inv.IsEmpty) {
            ground_inv.Add(adjacent, inv); // XXX this is scheduled for revision
            // ultimately, we'd like some notion of stance *if* that doesn't make the UI too complicated.
            // \todo for now, A Miracle Occurs
            var losing_inv = loc.Items;
            while(null != losing_inv && !inv.IsFull) {
              loc.Map.TransferFrom(losing_inv.TopItem, loc.Position, inv);
              losing_inv = loc.Items;
            }
#if DEBUG
            if (null != losing_inv) throw new InvalidOperationException("lost ground inventory");
#endif
          } else {
            inv = loc.Items;
            if (null != inv) ground_inv.Add(adjacent, inv); // XXX this is scheduled for revision
          }
        }
      }
      return ground_inv;
    }

    // Clairvoyant.  Useful for fine-tuning map generation and little else
    private KeyValuePair<Point, Inventory>? GetInventoryHaving(Gameplay.GameItems.IDs id)
    {
      if (District.Maps.Contains(this)) throw new InvalidOperationException("do not use GetInventoryHaving except during map generation");
      foreach (var x in m_GroundItemsByPosition) if (x.Value.Has(id)) return x;
      foreach (var x in m_aux_MapObjectsByPosition) if (x.Value.IsContainer && x.Value.Inventory.Has(id)) return new KeyValuePair<Point, Inventory>(x.Key, x.Value.Inventory);
      return null;
    }

    public void DropItemAt(Item it, in Point position)
    {
#if DEBUG
      if (!GetTileModelAt(position).IsWalkable) throw new InvalidOperationException("tried to drop "+it+" on a wall at "+(new Location(this,position)));
      if (0 >= it.Quantity) throw new InvalidOperationException("already zero");
#endif
      var itemsAt = GetItemsAt(position);
      itemsAt?.RepairContains(it, "already had ");
      if (itemsAt == null) {
        Inventory inventory = new Inventory(GROUND_INVENTORY_SLOTS);
        m_GroundItemsByPosition.Add(position, inventory);
        inventory.AddAll(it);
      } else if (itemsAt.IsFull) {
        int quantity = it.Quantity;
        int quantityAdded = itemsAt.AddAsMuchAsPossible(it);
        if (quantityAdded >= quantity) return;
        // Hammerspace inventory is already gamey.  We can afford to be even more gamey if it makes things more playable.
        // ensure that legendary artifacts don't disappear (yes, could infinite-loop but there aren't that many artifacts)
        Item crushed = itemsAt.BottomItem!;
        while(crushed.Model.IsUnbreakable || crushed.IsUnique) {
          itemsAt.RemoveAllQuantity(crushed);
          itemsAt.AddAll(crushed);
          crushed = itemsAt.BottomItem!;
        }
        // the test game series ending with the savefile break on April 28 2018 had a number of stacks with lots of baseball bats.  If there are two or more
        // destructible melee weapons in a stack, the worst one can be destroyed with minimal inconvenience.
        // Cf. Actor::GetWorstMeleeWeapon
        {
        var melee = itemsAt.GetItemsByType<ItemMeleeWeapon>()?.Where(m => !m.Model.IsUnbreakable && !m.IsUnique);
        if (2 <= (melee?.Count() ?? 0)) crushed = melee.Minimize(w => w.Model.Attack.Rating);
        }

        // other (un)reality checks go here
        itemsAt.RemoveAllQuantity(crushed);
        itemsAt.AddAsMuchAsPossible(it);
      }
      else
        itemsAt.AddAll(it);
    }

    public void DropItemAtExt(Item it, in Point position)
    {
      if (IsInBounds(position)) {
        DropItemAt(it, in position);
        return;
      }
      Location? tmp = _Normalize(position);
#if DEBUG
      if (null == tmp) throw new ArgumentOutOfRangeException(nameof(position),position,"invalid position for Item "+nameof(it));
#endif
      tmp.Value.Map.DropItemAt(it,tmp.Value.Position);
    }

#if DEAD_FUNC
    public void RemoveItemAt(Item it, in Point position)
    {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      var itemsAt = GetItemsAt(position);
#if DEBUG
      if (null == itemsAt) throw new ArgumentNullException(nameof(itemsAt),":= GetItemsAt(position)");
      if (!itemsAt.Contains(it)) throw new ArgumentOutOfRangeException(nameof(itemsAt),"item not at this position");
#endif
      itemsAt.RemoveAllQuantity(it);
      if (itemsAt.IsEmpty) m_GroundItemsByPosition.Remove(position);
    }
#endif

    public void TransferFrom(Item it, in Point position, Inventory dest) {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
      if (0 >= it.Quantity) throw new InvalidOperationException("already zero");
#endif
      var itemsAt = GetItemsAt(position);
#if DEBUG
      if (null == itemsAt) throw new ArgumentNullException(nameof(itemsAt),":= GetItemsAt(position)");
#endif
      if (itemsAt.Transfer(it, dest)) m_GroundItemsByPosition.Remove(position);
    }

    public bool RemoveAt<T>(Predicate<T> test, in Point pos) where T:Item
    {
#if DEBUG
      if (!IsInBounds(pos)) throw new ArgumentOutOfRangeException(nameof(pos), pos, "!IsInBounds(pos)");
#endif
      var itemsAt = GetItemsAt(pos);
      if (null == itemsAt) return false;
      var doomed = itemsAt.GetItemsByType(test);
      if (null == doomed) return false;
      foreach(T it in doomed) itemsAt.RemoveAllQuantity(it);
      if (itemsAt.IsEmpty) m_GroundItemsByPosition.Remove(pos);
      else itemsAt.RepairZeroQty(); // reddit/greenimba 2020-10-23
      return true;
    }

    public void RemoveAtExt<T>(Predicate<T> test, Point position) where T:Item
    {
      if (IsInBounds(position)) {
        RemoveAt(test, in position);
        return;
      }
      Location? remap = _Normalize(position);
      if (null != remap) remap.Value.Map.RemoveAt(test, remap.Value.Position);
    }

    // Clairvoyant.
    public bool TakeItemType(Gameplay.GameItems.IDs id, Inventory dest)
    {
      var src = GetInventoryHaving(id);
      if (null == src) return false;
      var it = src.Value.Value.GetFirst(id);
      if (null == it) return false;
      if (src.Value.Value.Transfer(it, dest)) m_GroundItemsByPosition.Remove(src.Value.Key);
      return true;
    }

    // Clairvoyant.
    public bool SwapItemTypes(Gameplay.GameItems.IDs want, Gameplay.GameItems.IDs donate, Inventory dest)
    {
      var giving = dest.GetFirst(donate);
      if (null == giving) return TakeItemType(want, dest);

      var src = GetInventoryHaving(want);
      if (null == src) return false;
      var it = src.Value.Value.GetFirst(want);
      if (null == it) return false;
      it.Unequip();

      src.Value.Value.RemoveAllQuantity(it);
      dest.RemoveAllQuantity(giving);
      src.Value.Value.AddAsMuchAsPossible(giving);
      dest.AddAsMuchAsPossible(it);
      return true;
    }

#if DEAD_FUNC
    public void RemoveItemAtExt(Item it, Point position)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (IsInBounds(position)) {
        RemoveItemAt(it,position);
        return;
      }
      Location? test = _Normalize(position);
      if (null != test) test.Value.Map.RemoveItemAt(it, test.Value.Position);
    }

    public void RemoveItemAt(Item it, int x, int y)
    {
      RemoveItemAt(it, new Point(x, y));
    }
#endif

#nullable enable
    /// <remark>Map generation depends on this being no-fail</remark>
    public void RemoveAllItemsAt(Point position) { m_GroundItemsByPosition.Remove(position); }

    public List<Corpse>? GetCorpsesAt(Point p)
    {
      if (m_aux_CorpsesByPosition.TryGetValue(p, out var corpseList)) return corpseList;
      return null;
    }

    public bool HasCorpsesAt(Point p) { return m_aux_CorpsesByPosition.ContainsKey(p); }
    public bool Has(Corpse c) { return m_CorpsesList.Contains(c); }

    public void AddAt(Corpse c, Point p)
    {
      if (m_CorpsesList.Contains(c)) throw new ArgumentException("corpse already in this map");
      c.Position = p;
      m_CorpsesList.Add(c);
      InsertAtPos(c);
      c.DeadGuy.Location = new Location(this, p);
    }

    public void MoveTo(Corpse c, Point newPos)
    {
      if (!m_CorpsesList.Contains(c)) throw new ArgumentException("corpse not in this map");
      RemoveFromPos(c);
      c.Position = newPos;
      InsertAtPos(c);
      c.DeadGuy.Location = new Location(this, newPos);
    }

    public void Remove(Corpse c)
    {
      if (!m_CorpsesList.Remove(c)) throw new ArgumentException("corpse not in this map");
      RemoveFromPos(c);
    }

    public void Destroy(Corpse c)
    {
      c.DraggedBy?.StopDraggingCorpse();
      Remove(c);
    }

    public bool TryRemoveCorpseOf(Actor a)
    {
      foreach (var c in m_CorpsesList) {
        if (c.DeadGuy == a) {
          Remove(c);
          return true;
        }
      }
      return false;
    }

    public bool HasCorpseOf(Actor a)
    {
      foreach (var c in m_CorpsesList) if (c.DeadGuy == a) return true;
      return false;
    }

    private void RemoveFromPos(Corpse c)
    {
      if (!m_aux_CorpsesByPosition.TryGetValue(c.Position, out var corpseList)) return;
      corpseList.Remove(c);
      if (0 >= corpseList.Count) m_aux_CorpsesByPosition.Remove(c.Position);
    }

    private void InsertAtPos(Corpse c)
    {
      if (m_aux_CorpsesByPosition.TryGetValue(c.Position, out var corpseList))
        corpseList.Insert(0, c);
      else
        m_aux_CorpsesByPosition.Add(c.Position, new List<Corpse>(1) { c });
    }

    public void AddTimer(TimedTask t) { m_Timers.Add(t); }
#if DEAD_FUNC
    // would be expected by Create-Read-Update-Delete idiom
    public void RemoveTimer(TimedTask t) { m_Timers.Remove(t); }
#endif

    public void UpdateTimers()
    {
      if (0 >= m_Timers.Count) return;

      bool elapse(TimedTask timer) {
        timer.Tick(this);   // ok for this to add timers; removal is by downward index sweep
        return timer.IsCompleted;
      }

      m_Timers.OnlyIfNot(elapse);
    }

    public KeyValuePair<bool,bool> AdvanceLocalTime()
    {
      bool wasNight = LocalTime.IsNight;
      ++LocalTime.TurnCounter;
      bool isDay = !LocalTime.IsNight;

      if (0 < m_ActorsList.Count) Engine.LOS.Now(this);
      Engine.LOS.Expire(this);
      return new KeyValuePair<bool,bool>(wasNight == isDay, isDay);
    }

    public int GetScentByOdorAt(Odor odor, in Point position)
    {
      if (IsInBounds(position)) {
        var scentByOdor = GetScentByOdor(odor, in position);
        if (scentByOdor != null) return scentByOdor.Strength;
      } else {
        Location? tmp = _Normalize(position);
        if (null != tmp) {
          var scentByOdor = tmp.Value.Map.GetScentByOdor(odor, tmp.Value.Position);
          if (scentByOdor != null) return scentByOdor.Strength;
        }
      }
      return 0;
    }

    private OdorScent? GetScentByOdor(Odor odor, in Point p)
    {
      if (!m_ScentsByPosition.TryGetValue(p, out var odorScentList)) return null;
      foreach (var odorScent in odorScentList) if (odorScent.Odor == odor) return odorScent;
      return null;
    }
#nullable restore

#if OBSOLETE
    private void AddNewScent(OdorScent scent, Point position)
    {
      if (m_ScentsByPosition.TryGetValue(position, out List<OdorScent> odorScentList)) {
        odorScentList.Add(scent);
      } else {
        m_ScentsByPosition.Add(position, new List<OdorScent>(2) { scent });
      }
    }

    public void ModifyScentAt(Odor odor, int strengthChange, Point position)
    {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor == null) {
        if (0 < strengthChange) AddNewScent(new OdorScent(odor, strengthChange), position);
      } else
        scentByOdor.Strength += strengthChange;
    }
#endif

#nullable enable
    public void RefreshScentAt(Odor odor, int freshStrength, Point position)
    {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      if (m_ScentsByPosition.TryGetValue(position, out var odorScentList)) {
        foreach (var odorScent in odorScentList) {
          if (odorScent.Odor == odor) {
            if (odorScent.Strength < freshStrength) odorScent.Strength = freshStrength;
            return;
          };
        }
        odorScentList.Add(new OdorScent(odor, freshStrength));
      } else {
        m_ScentsByPosition.Add(position, new List<OdorScent>(2) { new OdorScent(odor, freshStrength) });
      };
    }
#nullable restore

#if DEAD_FUNC
    public void RemoveScent(OdorScent scent)
    {
      if (!m_ScentsByPosition.TryGetValue(scent.Position, out List<OdorScent> odorScentList)) return;
      odorScentList.Remove(scent);
      if (0 >= odorScentList.Count) m_ScentsByPosition.Remove(scent.Position);
    }
#endif

#nullable enable
    public void DecayScents()
    {
      // Cf. Location.OdorsDecay
      short mapOdorDecayRate = 1;
      if (District.IsSewersMap(this)) mapOdorDecayRate += 2;

      var discard = new List<OdorScent>();
      List<Point>? discard2 = null;
      foreach(var tmp in m_ScentsByPosition) {
        short odorDecayRate = (3==mapOdorDecayRate ? mapOdorDecayRate : new Location(this,tmp.Key).OdorsDecay()); // XXX could micro-optimize further
        tmp.Value.OnlyIfNot(scent => scent.Decay(odorDecayRate));
        if (0 >= tmp.Value.Count) (discard2 ??= new List<Point>()).Add(tmp.Key);
      }
      if (null != discard2) foreach(var x in discard2) m_ScentsByPosition.Remove(x);
    }

    public void PreTurnStart()
    {
      m_iCheckNextActorIndex = 0;
      foreach (var actor in m_ActorsList) actor.PreTurnStart();
    }

    public bool IsTransparent(Point pt)
    {
      var tile_loc = GetTileModelLocation(pt);
      if (!tile_loc.Key?.IsTransparent ?? true) return false;
      return tile_loc.Value.MapObject?.IsTransparent ?? true;
    }

    public bool IsWalkable(int x, int y) { return IsWalkable(new Point(x,y)); }

    public bool IsWalkable(Point pt)
    {
      var tile_loc = GetTileModelLocation(pt);
      if (!tile_loc.Key?.IsWalkable ?? true) return false;
      return tile_loc.Value.MapObject?.IsWalkable ?? true;
    }

    public bool UnconditionallyBlockingFire(Point pt)
    {
      var tile_loc = GetTileModelLocation(pt);
      if (!tile_loc.Key?.IsTransparent ?? true) return true;
      var obj = tile_loc.Value.MapObject;
      return null != obj && !obj.IsMovable && !obj.IsTransparent;
    }

    public bool IsBlockingFire(Point pt)
    { // 2019-08-29 release mode IL Code size       75 (0x4b)
      var tile_loc = GetTileModelLocation(pt);
      if (!tile_loc.Key?.IsTransparent ?? true) return true;
      var loc = tile_loc.Value;
      if (loc.StrictHasActorAt) return true;
      return !loc.MapObject?.IsTransparent ?? false;
    }

    public bool IsBlockingThrow(Point pt)
    {
      var tile_loc = GetTileModelLocation(pt);
      if (!tile_loc.Key?.IsWalkable ?? true) return false;
      var obj = tile_loc.Value.MapObject;
      return obj != null && !obj.IsWalkable && !obj.IsJumpable;
    }

    /// <returns>0 not blocked, 1 jumping required, 2 blocked (for livings)</returns>
    public int IsBlockedForPathing(Point pt)
    { // 2019-08-29 release mode IL Code size       41 (0x29)
      // blockers are:
      // walls (hard) !map.GetTileModelAt(pt).IsWalkable
      // non-enterable objects (hard)
      // jumpable objects (soft) map.GetMapObjectAt(pt)
      var obj = GetMapObjectAtExt(pt);
      if (null == obj || obj.IsCouch || obj.IsWalkable) return 0;
      if (obj.IsJumpable) return 1;
      return 2;
    }

#if DEAD_FUNC
    /// <returns>0 not blocked, 1 jumping required both ways, 2 one wall one jump, 3 two walls (for livings)</returns>
    private int IsPathingChokepoint(Point x0, Point x1)
    {
      int x0_blocked = IsBlockedForPathing(x0);
      if (0== x0_blocked) return 0;
      int blocked = x0_blocked*IsBlockedForPathing(x1);
      // range is: 0,1,2,4; want to return 0...3
      return 4==blocked ? 3 : blocked;
    }
#endif

    /// <returns>worst blockage status code of IsBlockedForPathing</returns>
    public int CreatesPathingChokepoint(Point pt)
    {
      int block_N = IsBlockedForPathing(pt+Direction.N);
      int block_S = IsBlockedForPathing(pt+Direction.S);
      if (2==block_N && 2==block_S) return 2;
      int block_W = IsBlockedForPathing(pt+Direction.W);
      int block_E = IsBlockedForPathing(pt+Direction.E);
      if (2==block_W && 2==block_E) return 2;
      if (1==block_N*block_S) return 1;
      if (1==block_W*block_E) return 1;
      // would return 0 here when testing for *is* a pathing chokepoint
      if (1==block_N && 0<IsBlockedForPathing(pt+Direction.N+Direction.N)) return 1;
      if (1==block_S && 0<IsBlockedForPathing(pt+Direction.S+Direction.S)) return 1;
      if (1==block_W && 0<IsBlockedForPathing(pt+Direction.W+Direction.W)) return 1;
      if (1==block_E && 0<IsBlockedForPathing(pt+Direction.E+Direction.E)) return 1;
      return 0;
    }

    /// <returns>non-null dictionary whose Location keys are in canonical form (in bounds)</returns>
    static public Dictionary<Location,Direction> ValidDirections(in Location loc, Predicate<Location> testFn)
    {
      var ret = new Dictionary<Location,Direction>(8);
      foreach(Direction dir in Direction.COMPASS) {
        var pt = loc+dir;
        if (Canonical(ref pt) && testFn(pt)) ret.Add(pt, dir);
      }
      return ret;
    }

    public void EndTurn()
    {
        if (IsSecret) return;   // time-stopped
        pathing_exits_to_goals.Now(LocalTime.TurnCounter);
    }

    /// <remark>testFn has to tolerate denormalized coordinates</remark>
    public Dictionary<Point,T> FindAdjacent<T>(Point pos, Func<Map,Point,T?> testFn) where T:class
    {
#if DEBUG
      if (!IsInBounds(pos)) throw new InvalidOperationException("!IsInBounds(pos)");
#endif
      var ret = new Dictionary<Point,T>();
      foreach(Point pt in pos.Adjacent()) {
        var test = testFn(this,pt);
        if (null != test) ret.Add(pt, test);
      }
      return ret;
    }

    public List<Point>? FilterAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position)) return null;
      IEnumerable<Point> tmp = position.Adjacent().Where(p=>IsInBounds(p) && predicateFn(p));
      return (tmp.Any() ? tmp.ToList() : null);
    }

    public bool HasAnyAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position)) return false;
      return position.Adjacent().Any(p=>IsInBounds(p) && predicateFn(p));
    }

#if DEAD_FUNC
    public bool HasAnyAdjacent(Point position, Predicate<Point> predicateFn)
    {
      if (!IsValid(position)) return false;
      return position.Adjacent().Any(p=>IsValid(p) && predicateFn(p));
    }
#endif

    public bool HasAnyAdjacent(Point position, Predicate<Actor> test)
    {
      if (!IsValid(position)) return false;
      foreach(Point pt in position.Adjacent()) {
        if (!IsValid(pt)) continue;
        var a = GetActorAtExt(pt);
        if (null != a && test(a)) return true;
      }
      return false;
    }

    public int CountAdjacentTo(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position)) return 0;
      return position.Adjacent().Count(p=>IsInBounds(p) && predicateFn(p));
    }

    public int CountAdjacent<T>(Point pos) where T:MapObject
    {
      return CountAdjacentTo(pos, pt => GetMapObjectAt(pt) is T);
    }

    public int CountAdjacent<T>(Point pos,Predicate<T> test) where T:MapObject
    {
      return CountAdjacentTo(pos, pt => GetMapObjectAt(pt) is T obj && test(obj));
    }

    public bool AnyAdjacent<T>(Point pos) where T:MapObject
    {
      return HasAnyAdjacentInMap(pos, pt => GetMapObjectAt(pt) is T);
    }

    public bool AnyAdjacentExt<T>(Point pos) where T:MapObject
    {
      return HasAnyAdjacentInMap(pos, pt => GetMapObjectAtExt(pt) is T);
    }

    public bool AnyAdjacent<T>(Point pos,Predicate<T> test) where T:MapObject
    {
      return HasAnyAdjacentInMap(pos, pt => GetMapObjectAt(pt) is T obj && test(obj));
    }

    public void ForEachAdjacent(Point position, Action<Point> fn)
    {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      foreach(var dir in Direction.COMPASS) {
        var pt = position+dir;
        if (IsInBounds(pt)) fn(pt);
      }
    }

    // pathfinding support
    public Rectangle NavigationScope {
      get {
       if (District.IsSewersMap(this)) return new Rectangle(DistrictPos + Direction.NW, new Size(3, 3)); // sewers are not well-connected...next district over may be needed
       if (District.IsSubwayMap(this) && 0>= PowerGenerators.Get.Count) return new Rectangle(DistrictPos + Direction.NW, new Size(3, 3)); // subway w/o generators should have an entrance "close by"
       return new Rectangle(DistrictPos, new Size(1, 1));
     }
    }
#nullable restore

#if PRERELEASE_MOTHBALL
    private void fuse_chokepoints(List<Point[]> candidates, Direction normal_dir, Action<Location[]> install)
    {
        var one_adjacent = new List<Point[]>(candidates.Count);
        var two_adjacent = new List<Point[]>(candidates.Count);

        foreach(var x in candidates) {
          switch(candidates.Count(y => 1==Engine.Rules.GridDistance(x[0],y[0]) && (x[0].X==y[0].X || x[0].Y == y[0].Y)))
          {
          case 2: two_adjacent.Add(x); break;
          case 1: one_adjacent.Add(x); break;
          case 0: install(new Location[] { new Location(this, x[0]) }); break;
          default: throw new InvalidOperationException("should not have more than two chokepoints adjacent to a chokepoint");
          }
        }

        var inverse_dir = -normal_dir;
        int i = one_adjacent.Count;
        while (0 < i--) {
#if DEBUG
          if (0 != one_adjacent.Count%2) throw new InvalidProgramException("expected paired endpoints");
#endif
          var choke = one_adjacent[i];
          one_adjacent.RemoveAt(i);
          var anchor = new List<Location> { new Location(this,choke[0]) };
          var check_normal = choke[0]+normal_dir;
          int test_for;
          if (0 <= (test_for = one_adjacent.FindIndex(x => x[0] == check_normal))) {
            anchor.Add(new Location(this,one_adjacent[test_for][0]));
            one_adjacent.RemoveAt(test_for);
            i--;
            install(anchor.ToArray());
            continue;
          }
          var check_inverse = choke[0]+ inverse_dir;
          if (0 <= (test_for = one_adjacent.FindIndex(x => x[0] == check_inverse))) {
            anchor.Insert(0,new Location(this,one_adjacent[test_for][0]));
            one_adjacent.RemoveAt(test_for);
            i--;
            install(anchor.ToArray());
            continue;
          }
          test_for = two_adjacent.FindIndex(x => x[0] == check_normal);
          if (0 <= test_for) {
            do {
              anchor.Add(new Location(this, two_adjacent[test_for][0]));
              check_normal = two_adjacent[test_for][0]+normal_dir;
              two_adjacent.RemoveAt(test_for);
            } while(0 <= (test_for = two_adjacent.FindIndex(x => x[0] == check_normal)));
            if (0 > (test_for = one_adjacent.FindIndex(x => x[0] == check_normal))) throw new InvalidProgramException("expected to find matching endpoint");
            anchor.Add(new Location(this,one_adjacent[test_for][0]));
            one_adjacent.RemoveAt(test_for);
            install(anchor.ToArray());
            i--;
            continue;
          }
          test_for = two_adjacent.FindIndex(x => x[0] == check_inverse);
          if (0 <= test_for) {
            do {
              anchor.Insert(0,new Location(this, two_adjacent[test_for][0]));
              check_inverse = two_adjacent[test_for][0]+inverse_dir;
              two_adjacent.RemoveAt(test_for);
            } while(0 <= (test_for = two_adjacent.FindIndex(x => x[0] == check_inverse)));
            if (0 > (test_for = one_adjacent.FindIndex(x => x[0] == check_inverse))) throw new InvalidProgramException("expected to find matching endpoint");
            anchor.Insert(0,new Location(this,one_adjacent[test_for][0]));
            one_adjacent.RemoveAt(test_for);
            install(anchor.ToArray());
            i--;
            continue;
          }
          throw new InvalidProgramException("expected matching endpoint, not a singleton");
        }
    }

    public void RegenerateChokepoints() {
      var working = new List<LinearChokepoint>();

      // define a chokepoint as a width-1 corridor.  We don't handle vertical exits here (those need entries in map pairs)
      // we assume the vertical exits are added after us, or in a different cache
      var test = new Dictionary<Point,int>();
      var chokepoint_candidates = new List<Point[]>(Rect.Width * Rect.Height);
      Rect.DoForEach(pt => {
          test[pt] = 0;
          if (0 == pt.X) test[pt + Direction.W] = 0;
          if (0 == pt.Y) test[pt + Direction.N] = 0;
          if (Rect.Width - 1 == pt.X) test[pt + Direction.E] = 0;
          if (Rect.Height - 1 == pt.Y) test[pt + Direction.S] = 0;
      });
      Rect.DoForEach(pt => {
          Point[] candidate = { pt, pt + Direction.W, pt + Direction.E, pt + Direction.N, pt + Direction.S };
          int i = candidate.Length;
          while (0 < i--) test[candidate[i]] += 1;
          foreach (var x in candidate) test[x] += 1;
          chokepoint_candidates.Add(candidate);
      });
      var chokepoint_candidates_ns = new List<Point[]>(chokepoint_candidates.Count);
      var chokepoint_candidates_ew = new List<Point[]>(chokepoint_candidates.Count);

#region chokepoint rejection
      void chokepoint_rejected(Point[] candidate) {
         int i = candidate.Length;
         while(0 < i--) {
           if (!test.ContainsKey(candidate[i])) continue;
           if (0 >= (test[candidate[i]] -= 1)) test.Remove(candidate[i]);
         }
      }
      void ew_chokepoint_rejected(Point[] candidate) {
         if (test.ContainsKey(candidate[0]) && 0 >= (test[candidate[0]] -= 1)) test.Remove(candidate[0]);
         if (test.ContainsKey(candidate[3]) && 0 >= (test[candidate[3]] -= 1)) test.Remove(candidate[3]);
         if (test.ContainsKey(candidate[4]) && 0 >= (test[candidate[4]] -= 1)) test.Remove(candidate[4]);
      }
      void ns_chokepoint_rejected(Point[] candidate) {
         if (test.ContainsKey(candidate[0]) && 0 >= (test[candidate[0]] -= 1)) test.Remove(candidate[0]);
         if (test.ContainsKey(candidate[1]) && 0 >= (test[candidate[1]] -= 1)) test.Remove(candidate[1]);
         if (test.ContainsKey(candidate[2]) && 0 >= (test[candidate[2]] -= 1)) test.Remove(candidate[2]);
      }
      void chokepoint_rejected_ns(Point[] candidate) {
         if (test.ContainsKey(candidate[1]) && 0 >= (test[candidate[1]] -= 1)) test.Remove(candidate[1]);
         if (test.ContainsKey(candidate[2]) && 0 >= (test[candidate[2]] -= 1)) test.Remove(candidate[2]);
         chokepoint_candidates_ew.Add(candidate);
      }
      void chokepoint_rejected_ew(Point[] candidate) {
         if (test.ContainsKey(candidate[3]) && 0 >= (test[candidate[3]] -= 1)) test.Remove(candidate[3]);
         if (test.ContainsKey(candidate[4]) && 0 >= (test[candidate[4]] -= 1)) test.Remove(candidate[4]);
         chokepoint_candidates_ns.Add(candidate);
      }
#endregion

      bool pt_walkable(Point p) {
        bool walkable = IsValid(p) && GetTileModelAtExt(p).IsWalkable;
#if PROTOTYPE
        if (walkable) {
          var obj = GetMapObjectAtExt(p);
          if (null != obj && !(obj is DoorWindow) && !obj.IsWalkable && !obj.IsJumpable) walkable = false;  // i.e., car extinguishing can trigger recalc
        }
#endif
        return walkable;
      }

      void test_for_chokepoint(Point p) {
        bool walkable = pt_walkable(p);
        test.Remove(p);
#region List::RemoveAt incompatible with iterating when rejecting chokepoints
        if (walkable) {
          int i = chokepoint_candidates_ew.Count;
          while(0 < i--) {
            var tmp = chokepoint_candidates_ew[i];
            if (   p == tmp[1]
                || p == tmp[2]) {
              ew_chokepoint_rejected(tmp);
              chokepoint_candidates_ew.RemoveAt(i);
            };
          }
          i = chokepoint_candidates_ns.Count;
          while(0 < i--) {
            var tmp = chokepoint_candidates_ns[i];
            if (   p == tmp[3]
                || p == tmp[4]) {
              ns_chokepoint_rejected(tmp);
              chokepoint_candidates_ns.RemoveAt(i);
            };
          }
          i = chokepoint_candidates.Count;
          while(0 < i--) {
            var tmp = chokepoint_candidates[i];
            if (   p == tmp[1]
                || p == tmp[2]) {
              chokepoint_rejected_ew(tmp);
              chokepoint_candidates.RemoveAt(i);
            };
            if (   p == tmp[3]
                || p == tmp[4]) {
              chokepoint_rejected_ns(tmp);
              chokepoint_candidates.RemoveAt(i);
            };
          }
        } else {
         int i = chokepoint_candidates_ew.Count;
         while(0 < i--) {
            var tmp = chokepoint_candidates_ew[i];
            if (p == tmp[0]) {
              ew_chokepoint_rejected(tmp);
              chokepoint_candidates_ew.RemoveAt(i);
              break;
            };
          }
         i = chokepoint_candidates_ns.Count;
         while(0 < i--) {
            var tmp = chokepoint_candidates_ns[i];
            if (p == tmp[0]) {
              ns_chokepoint_rejected(tmp);
              chokepoint_candidates_ns.RemoveAt(i);
              break;
            };
          }
         i = chokepoint_candidates.Count;
         while(0 < i--) {
            var tmp = chokepoint_candidates[i];
            if (p == tmp[0]) {
              chokepoint_rejected(tmp);
              chokepoint_candidates.RemoveAt(i);
              break;
            };
          }
        }
#endregion
      }

      while(0 < test.Count) {
        var x = test.First();
        test_for_chokepoint(x.Key);
      }

      if (0 >= chokepoint_candidates.Count && 0 >= chokepoint_candidates_ew.Count && 0 >= chokepoint_candidates_ns.Count) {
        m_Chokepoints.Clear();
        return;
      }

      // if there are dual-mode chokepoints, they will not fuse.  Double-list them (once for each orientation)
      foreach(var choke in chokepoint_candidates) {
        var anchor = new List<Location> { new Location(this,choke[0]) };
        var ns_entrance = new List<Location>();
        var ns_exit = new List<Location>();
        var ew_entrance = new List<Location>();
        var ew_exit = new List<Location>();

        var pt_test = choke[0] + Direction.N;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) ns_entrance.Add(loc);
        };
        pt_test = choke[0] + Direction.S;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) ns_exit.Add(loc);
        };
        pt_test = choke[0] + Direction.W;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) ew_entrance.Add(loc);
        };
        pt_test = choke[0] + Direction.E;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) ew_exit.Add(loc);
        };
        pt_test = choke[0] + Direction.NW;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) {
            ns_entrance.Add(loc);
            ew_entrance.Add(loc);
          }
        };
        pt_test = choke[0] + Direction.NE;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) {
            ns_entrance.Add(loc);
            ew_exit.Add(loc);
          }
        };
        pt_test = choke[0] + Direction.SE;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) {
            ns_exit.Add(loc);
            ew_exit.Add(loc);
          }
        };
        pt_test = choke[0] + Direction.SW;
        if (pt_walkable(pt_test)) {
          var loc = new Location(this, pt_test);
          if (loc.ForceCanonical()) {
            ns_exit.Add(loc);
            ew_entrance.Add(loc);
          }
        };

        var anchor_array = anchor.ToArray();
        working.Add(new LinearChokepoint(ns_entrance.ToArray(), anchor_array, ns_exit.ToArray()));
        working.Add(new LinearChokepoint(ew_entrance.ToArray(), anchor_array, ew_exit.ToArray()));
      }

      if (0 >= chokepoint_candidates_ew.Count && 0 >= chokepoint_candidates_ns.Count) {
        // nearly ACID update
        m_Chokepoints.Clear();
        m_Chokepoints.AddRange(working);
        return;
      }

      void install_ns_chokepoint(Location[] anchor)
      {
        var ns_entrance = new List<Location>();
        var ns_exit = new List<Location>();

        var pt_test = anchor[0] + Direction.N;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ns_entrance.Add(pt_test);
        };
        pt_test = anchor[anchor.Length-1] + Direction.S;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ns_exit.Add(pt_test);
        };
        pt_test = anchor[0] + Direction.NW;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ns_entrance.Add(pt_test);
        };
        pt_test = anchor[0] + Direction.NE;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ns_entrance.Add(pt_test);
        };
        pt_test = anchor[anchor.Length - 1] + Direction.SE;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ns_exit.Add(pt_test);
        };
        pt_test = anchor[anchor.Length - 1] + Direction.SW;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ns_exit.Add(pt_test);
        };

        working.Add(new LinearChokepoint(ns_entrance.ToArray(), anchor, ns_exit.ToArray()));
      }

      void install_ew_chokepoint(Location[] anchor)
      {
        var ew_entrance = new List<Location>();
        var ew_exit = new List<Location>();

        var pt_test = anchor[0] + Direction.W;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ew_entrance.Add(pt_test);
        };
        pt_test = anchor[anchor.Length - 1] + Direction.E;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ew_exit.Add(pt_test);
        };
        pt_test = anchor[0] + Direction.NW;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ew_entrance.Add(pt_test);
        };
        pt_test = anchor[anchor.Length - 1] + Direction.NE;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ew_exit.Add(pt_test);
        };
        pt_test = anchor[anchor.Length - 1] + Direction.SE;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ew_exit.Add(pt_test);
        };
        pt_test = anchor[0] + Direction.SW;
        if (pt_walkable(pt_test.Position)) {
          if (pt_test.ForceCanonical()) ew_entrance.Add(pt_test);
        };

        working.Add(new LinearChokepoint(ew_entrance.ToArray(), anchor, ew_exit.ToArray()));
      }

      // chokepoints can fuse with other chokepoints of the same type.
      fuse_chokepoints(chokepoint_candidates_ns, Direction.E, install_ew_chokepoint);
      fuse_chokepoints(chokepoint_candidates_ew, Direction.S, install_ns_chokepoint);

      // nearly ACID update
      m_Chokepoints.Clear();
      m_Chokepoints.AddRange(working);
    }

    public LinearChokepoint EnteringChokepoint(Location origin, Location dest) {
      var candidates = m_Chokepoints.FindAll(choke => choke.Chokepoint.Contains(dest));
      if (0 >= candidates.Count) return null;
      candidates = candidates.FindAll(choke => choke.Entrance.Contains(origin) || choke.Exit.Contains(origin));
      if (0 >= candidates.Count) return null;
      return candidates[0];
    }
#endif

    public void RegenerateMapGeometry() {
      int crm_encode(Vector2D_int_stack pt) { return pt.X + Rect.Width*pt.Y; }    // chinese remainder theorem encoding
      Vector2D_int_stack crm_decode(int n) { return new Vector2D_int_stack(n%Rect.Width,n/Rect.Width); }    // chinese remainder theorem decoding

      // we don't care about being completely correct for outdoors, here.  This has to support the indoor situation only
      Span<bool> wall_horz3 = stackalloc bool[Rect.Height*Rect.Width];
      Span<bool> wall_vert3 = stackalloc bool[Rect.Height*Rect.Width];
      Span<bool> space_horz3 = stackalloc bool[Rect.Height*Rect.Width];
      Span<bool> space_vert3 = stackalloc bool[Rect.Height*Rect.Width];
      Vector2D_int_stack p;
      p.X = Rect.Width;
      while(0 < p.X--) {
        p.Y = Rect.Height;
        while(0 < p.Y--) {
          if (Width - 3 > p.X) {
            if (  !GetTileModelAt(p.X, p.Y).IsWalkable
               && !GetTileModelAt(p.X + 1, p.Y).IsWalkable
               && !GetTileModelAt(p.X + 2, p.Y).IsWalkable) wall_horz3[crm_encode(p)] = true;
            if (  GetTileModelAt(p.X, p.Y).IsWalkable
               && GetTileModelAt(p.X + 1, p.Y).IsWalkable
               && GetTileModelAt(p.X + 2, p.Y).IsWalkable) space_horz3[crm_encode(p)] = true;
            }
          if (Height - 3 > p.Y) {
            if (  !GetTileModelAt(p.X, p.Y).IsWalkable
               && !GetTileModelAt(p.X, p.Y + 1).IsWalkable
               && !GetTileModelAt(p.X, p.Y + 2).IsWalkable) wall_vert3[crm_encode(p)] = true;
            if (  GetTileModelAt(p.X, p.Y).IsWalkable
               && GetTileModelAt(p.X, p.Y + 1).IsWalkable
               && GetTileModelAt(p.X, p.Y + 2).IsWalkable) space_vert3[crm_encode(p)] = true;
          }
        }
      }

      // We run very early (map loading/new game) so we get no benefit from trying to fake ACID update.
      m_FullCorner_nw.Clear();
      m_FullCorner_ne.Clear();
      m_FullCorner_se.Clear();
      m_FullCorner_sw.Clear();
      m_FlushWall_n.Clear();
      m_FlushWall_s.Clear();
      m_FlushWall_w.Clear();
      m_FlushWall_e.Clear();

      int i = Rect.Width*Rect.Height;
      Vector2D_int_stack tmp;
      int tmp_i;
      while(0 < i--) {
        if (!wall_horz3[i] && !wall_vert3[i]) continue;
        p = crm_decode(i);
        if (wall_horz3[i] && wall_vert3[i]) {
          // nw corner candidate
          if (   space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y+1))]
              && space_vert3[tmp_i]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y+2))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+2,p.Y+1))]) m_FullCorner_nw.Add(new Point(p.X+1,p.Y+1));
        }
        // [tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X,p.Y))]
        if (wall_horz3[i]) {
          // must test for: flush wall n/s
          // can test for cleanly: corner ne
          if (   Rect.Height-2 > p.Y
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X, p.Y+1))]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X, p.Y+2))]) m_FlushWall_n.Add(new Point(p.X+1,p.Y+1));
          if (   2 <= p.Y
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X, p.Y-1))]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X, p.Y-2))]) m_FlushWall_s.Add(new Point(p.X+1,p.Y-1));
          if (   Rect.Width-2 > p.X
              && 1 <= p.X
              && wall_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+2, p.Y))] 
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X-1,p.Y+1))]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X-1,p.Y+2))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X  ,p.Y+1))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y+1))]) m_FullCorner_ne.Add(new Point(p.X-1,p.Y+1));
          // do SE here as well
          if (   Rect.Width-2 > p.X
              && 1 <= p.X
              && 3 <= p.Y
              && wall_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+2, p.Y-2))]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X-1,p.Y-1))]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X-1,p.Y-2))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X  ,p.Y-3))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y-3))]) m_FullCorner_se.Add(new Point(p.X-1,p.Y-1));
        }
        if (wall_vert3[i]) {
          // must test for: flush wall e/w
          // can test for cleanly: corner sw
          if (   Rect.Width-2 > p.X
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1, p.Y))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+2, p.Y))]) m_FlushWall_w.Add(new Point(p.X+1,p.Y+1));
          if (   2 <= p.X
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X-1, p.Y))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X-2, p.Y))]) m_FlushWall_e.Add(new Point(p.X-1,p.Y+1));
          if (   Rect.Width-2 > p.X
              && 1 <= p.Y
              && Rect.Height - 2 > p.Y
              && wall_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X, p.Y+2))] 
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y))]
              && space_horz3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y+1))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+1,p.Y-1))]
              && space_vert3[tmp_i = crm_encode(tmp = new Vector2D_int_stack(p.X+2,p.Y-1))]) m_FullCorner_sw.Add(new Point(p.X+1,p.Y-1));
        }
      } // end while(0 < i--)
    }

    // accessors for map geometry
    // XXX linear...may want to do something about that
    public bool IsNWCorner(Point pt) { return m_FullCorner_nw.Contains(pt); }
    public bool IsNECorner(Point pt) { return m_FullCorner_ne.Contains(pt); }
    public bool IsSWCorner(Point pt) { return m_FullCorner_sw.Contains(pt); }
    public bool IsSECorner(Point pt) { return m_FullCorner_se.Contains(pt); }
    public bool IsFlushNWall(Point pt) { return m_FlushWall_n.Contains(pt); }
    public bool IsFlushSWall(Point pt) { return m_FlushWall_s.Contains(pt); }
    public bool IsFlushWWall(Point pt) { return m_FlushWall_w.Contains(pt); }
    public bool IsFlushEWall(Point pt) { return m_FlushWall_e.Contains(pt); }

    // cheat map similar to savefile viewer
    public void DaimonMap(Zaimoni.Data.OutTextFile dest) {
      if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
      dest.WriteLine(Name+"<br>");
      // XXX since the lock at the district level causes deadlocking, we may be inconsistent for simulation districtions
      List<Actor> tmp_Actors = (0< m_ActorsList.Count ? new List<Actor>(m_ActorsList) : null);
      List<Point> inv_locs = (0<m_GroundItemsByPosition.Count ? new List<Point>(m_GroundItemsByPosition.Keys) : null);
//    if (null==tmp_Actors && null==inv_locs) return;
      // we have one of actors or items here...full map has motivation
      var inv_data = new List<string>();
      string[] actor_headers = { "pos", "name", "Priority", "AP", "HP", "Inventory" };  // XXX would be function-static in C++
      List<string> actor_data = new List<string>();
      string[][] ascii_map = new string[Height][];

      void _process_inv(Inventory inv, int x, int y) {
        if (null != inv && !inv.IsEmpty) {
          string p_txt = '('+x.ToString()+','+y.ToString()+')';
          foreach (Item it in inv.Items) {
            inv_data.Add("<tr class='inv'><td>"+p_txt+"</td><td>"+it.ToString()+"</td></tr>");
          }
          ascii_map[y][x] = "&"; // Angband/Nethack pile.
        }
      }

      foreach(short y in Enumerable.Range(0, Height)) {
        ascii_map[y] = new string[Width];
        foreach(short x in Enumerable.Range(0, Width)) {
          // XXX does not handle transparent walls or opaque non-walls
          Point pt = new Point(x,y);
          ascii_map[y][x] = (GetTileModelAt(x,y).IsWalkable ? "." : "#");    // typical floor tile if walkable, typical wall otherwise
          if (HasExitAt(pt)) ascii_map[y][x] = ">";                  // downwards exit
#region map objects
          const string tree_symbol = "&#x2663;"; // unicode: card suit club looks enough like a tree
          const string car_symbol = "<span class='car'>&#x1F698;</span>";   // unicode: oncoming car
          const string drawer_symbol = "&#x2584;";    // unicode: block elements
          const string shop_shelf_symbol = "&#x25A1;";    // unicode: geometric shapes
          const string large_fortification_symbol = "<span class='lfort'>&#x25A6;</span>";    // unicode: geometric shapes
          const string power_symbol = "&#x2B4D;";    // unicode: misc symbols & arrows
          const string closed_gate = "<span class='lfort'>&#x2630;</span>";    // unicode: misc symbols (I Ching heaven)
          const string iron_fence = "<span class='lfort'>&#x2632;</span>";    // unicode: misc symbols (I Ching fire)
          const string open_gate = "<span class='lfort'>&#x2637;</span>";    // unicode: misc symbols (I Ching earth)
          const string chair = "<span class='chair'>&#x2441;</span>";    // unicode: OCR chair
          var tmp_obj = GetMapObjectAt(pt);  // micro-optimization target (one Point temporary involved)
          if (null!=tmp_obj) {
            if (tmp_obj.IsCouch) {
              ascii_map[y][x] = "="; // XXX no good icon for bed...we have no rings so this is not-awful
            } else if (MapObject.IDs.TREE == tmp_obj.ID) {
              ascii_map[y][x] = tree_symbol;
            } else if (MapObject.IDs.CAR1 == tmp_obj.ID) {
              ascii_map[y][x] = car_symbol; // unicode: oncoming car
            } else if (MapObject.IDs.CAR2 == tmp_obj.ID) {
              ascii_map[y][x] = car_symbol; // unicode: oncoming car
            } else if (MapObject.IDs.CAR3 == tmp_obj.ID) {
              ascii_map[y][x] = car_symbol; // unicode: oncoming car
            } else if (MapObject.IDs.CAR4 == tmp_obj.ID) {
              ascii_map[y][x] = car_symbol; // unicode: oncoming car
            } else if (MapObject.IDs.DRAWER == tmp_obj.ID) {
              ascii_map[y][x] = drawer_symbol;
            } else if (MapObject.IDs.SHOP_SHELF == tmp_obj.ID) {
              ascii_map[y][x] = shop_shelf_symbol;
            } else if (MapObject.IDs.LARGE_FORTIFICATION == tmp_obj.ID) {
              ascii_map[y][x] = large_fortification_symbol;
            } else if (MapObject.IDs.CHAR_POWER_GENERATOR == tmp_obj.ID) {
              ascii_map[y][x] = ((tmp_obj as Engine.MapObjects.PowerGenerator).IsOn ? "<span class='power' style='background:green'>" : "<span class='power' style='background:red'>") +power_symbol+" </span>";
            } else if (MapObject.IDs.IRON_GATE_CLOSED == tmp_obj.ID) {
              ascii_map[y][x] = closed_gate;
            } else if (MapObject.IDs.IRON_FENCE == tmp_obj.ID || MapObject.IDs.WIRE_FENCE == tmp_obj.ID) {
              ascii_map[y][x] = iron_fence;
            } else if (MapObject.IDs.IRON_GATE_OPEN == tmp_obj.ID) {
              ascii_map[y][x] = open_gate;
            } else if (MapObject.IDs.CHAIR == tmp_obj.ID) {
              ascii_map[y][x] = chair;
            } else if (MapObject.IDs.CHAR_CHAIR == tmp_obj.ID) {
              ascii_map[y][x] = chair;
            } else if (MapObject.IDs.HOSPITAL_CHAIR == tmp_obj.ID) {
              ascii_map[y][x] = chair;
            } else if (tmp_obj.IsTransparent && !tmp_obj.IsWalkable) {
              ascii_map[y][x] = "|"; // gate; iron wall
            } else {
              if (tmp_obj is Engine.MapObjects.DoorWindow tmp_door) {
                if (tmp_door.IsBarricaded) {
                  ascii_map[y][x] = large_fortification_symbol; // no good icon...pretend it's a large fortification since it would have to be torn down to be passed through
                } else if (tmp_door.IsClosed) {
                  ascii_map[y][x] = "+"; // typical closed door
                } else if (tmp_door.IsOpen) {
                  ascii_map[y][x] = "'"; // typical open door
                } else /* if (tmp_door.IsBroken */ {
                  ascii_map[y][x] = "'"; // typical broken door
                }
              }
            }
            if (tmp_obj.IsContainer) _process_inv(tmp_obj.Inventory, x, y);
		  }
#endregion
          _process_inv(GetItemsAt(pt), x, y);
#region actors
          Actor a = GetActorAt(pt);
          if (null!=a && !a.IsDead) {
            string p_txt = '('+a.Location.Position.X.ToString()+','+ a.Location.Position.Y.ToString()+')';
            string a_str = a.Faction.ID.ToString(); // default to the faction numeral
            string pos_css = "";
            if (a.Controller is PlayerController) {
              a_str = "@";
              pos_css = " style='background:lightgreen'";
            };
            switch(a.Model.ID) {
              case Gameplay.GameActors.IDs.UNDEAD_SKELETON:
                a_str = "<span style='background:orange'>s</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_RED_EYED_SKELETON:
                a_str = "<span style='background:red'>s</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_RED_SKELETON:
                a_str = "<span style='background:darkred'>s</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE:
                a_str = "<span style='background:orange'>S</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE:
                a_str = "<span style='background:red'>S</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_DARK_ZOMBIE:
                a_str = "<span style='background:darkred'>S</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
                a_str = "<span style='background:orange'>Z</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE_LORD:
                a_str = "<span style='background:red'>Z</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE_PRINCE:
                a_str = "<span style='background:darkred'>Z</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
              case Gameplay.GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
                a_str = "<span style='background:orange'>d</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_MALE_NEOPHYTE:
              case Gameplay.GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE:
                a_str = "<span style='background:red'>d</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_MALE_DISCIPLE:
              case Gameplay.GameActors.IDs.UNDEAD_FEMALE_DISCIPLE:
                a_str = "<span style='background:darkred'>d</span>"; break;
              case Gameplay.GameActors.IDs.UNDEAD_RAT_ZOMBIE:
                a_str = "<span style='background:orange'>r</span>"; break;
              case Gameplay.GameActors.IDs.MALE_CIVILIAN:
              case Gameplay.GameActors.IDs.FEMALE_CIVILIAN:
                a_str = "<span style='background:lightgreen'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.FERAL_DOG:
                a_str = "<span style='background:lightgreen'>C</span>"; break;    // C in Angband, Nethack
              case Gameplay.GameActors.IDs.CHAR_GUARD:
                a_str = "<span style='background:darkgray;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.ARMY_NATIONAL_GUARD:
                a_str = "<span style='background:darkgreen;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.BIKER_MAN:
                a_str = "<span style='background:darkorange;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.POLICEMAN:
              case Gameplay.GameActors.IDs.POLICEWOMAN:
                a_str = "<span style='background:lightblue'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.GANGSTA_MAN:
                a_str = "<span style='background:red;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.BLACKOPS_MAN:
                a_str = "<span style='background:black;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.SEWERS_THING:
              case Gameplay.GameActors.IDs.JASON_MYERS:
                a_str = "<span style='background:darkred;color:white'>"+a_str+"</span>"; break;
            }
            var actor_stats = new List<string> { " " };

            if (a.Model.Abilities.HasToEat) {
              if (a.IsStarving) actor_stats.Add("<span style='background-color:black; color:red'>H</span>");
              else if (a.IsHungry) actor_stats.Add("<span style='background-color:black; color:yellow'>H</span>");
              else if (a.IsAlmostHungry) actor_stats.Add("<span style='background-color:black; color:green'>H</span>");
            }
            else if (a.Model.Abilities.IsRotting) {
              if (a.IsRotStarving) actor_stats.Add("<span style='background-color:black; color:red'>H</span>");
              else if (a.IsRotHungry) actor_stats.Add("<span style='background-color:black; color:yellow'>R</span>");
              else if (a.IsAlmostRotHungry) actor_stats.Add("<span style='background-color:black; color:green'>R</span>");
            }
            if (a.Model.Abilities.HasSanity) {
              if (a.IsInsane) actor_stats.Add("<span style='background-color:black; color:red'>I</span>");
              else if (a.IsDisturbed) actor_stats.Add("<span style='background-color:black; color:yellow'>I</span>");
            }
            if (a.Model.Abilities.HasToSleep) {
              if (a.IsExhausted) actor_stats.Add("<span style='background-color:black; color:red'>Z</span>");
              else if (a.IsSleepy) actor_stats.Add("<span style='background-color:black; color:yellow'>Z</span>");
              else if (a.IsAlmostSleepy) actor_stats.Add("<span style='background-color:black; color:green'>Z</span>");
            }
            if (a.IsSleeping) actor_stats.Add("<span style='background-color:black; color:cyan'>Z</span>");
            if (0 < a.MurdersCounter) actor_stats.Add("<span style='background-color:black; color:red'>M</span>");
            if (0 < a.CountFollowers) actor_stats.Add("<span style='background-color:black; color:cyan'>L</span>");
            if (null != a.LiveLeader) actor_stats.Add("<span style='background-color:black; color:cyan'>F:"+a.LiveLeader.Name+"</span>");

            actor_data.Add("<tr><td"+ pos_css + ">" + p_txt + "</td><td>" + a.UnmodifiedName + string.Join("", actor_stats) + "</td><td>"+m_ActorsList.IndexOf(a).ToString()+"</td><td>"+a.ActionPoints.ToString()+ "</td><td>"+a.HitPoints.ToString()+ "</td><td class='inv'>"+(null==a.Inventory ? "" : (a.Inventory.IsEmpty ? "" : a.Inventory.ToString()))+"</td></tr>");
            ascii_map[a.Location.Position.Y][a.Location.Position.X] = a_str;
          }
#endregion
        }
      }
      if (0 < m_aux_CorpsesByPosition.Count) {
        // need a value copy of relevant (infected) corpses
        var tmp = new Dictionary<Point, List<Corpse>>(m_aux_CorpsesByPosition.Count);

        static bool is_problem_corpse(Corpse c) { return 0 < c.DeadGuy.InfectionPercent; }

        foreach(var x in m_aux_CorpsesByPosition) {
          if (0>= x.Value.Count) continue;
          if (!x.Value.Any(is_problem_corpse)) continue;
          tmp[x.Key] = x.Value.FindAll(is_problem_corpse);
        }
        if (0 < tmp.Count) dest.WriteLine("<pre>Problematic corpses:\n"+tmp.to_s()+"</pre>");
      }
      if (0>=inv_data.Count && 0>=actor_data.Count) return;
      if (0<actor_data.Count) {
        dest.WriteLine("<table border=2 cellspacing=1 cellpadding=1 align=left>");
        dest.WriteLine("<tr><th>"+string.Join("</th><th>", actor_headers) + "</th></tr>");
        foreach(string s in actor_data) dest.WriteLine(s);
        dest.WriteLine("</table>");
      }
      if (0<inv_data.Count) {
        dest.WriteLine("<table border=2 cellspacing=1 cellpadding=1 align=right>");
        foreach(string s in inv_data) dest.WriteLine(s);
        dest.WriteLine("</table>");
      }
      dest.WriteLine("<a name='"+Name+"'></a>");
      dest.WriteLine("<pre style='clear:both'>");
      foreach (int y in Enumerable.Range(0, Height)) {
        dest.WriteLine(String.Join("",ascii_map[y]));
      }
      dest.WriteLine("</pre>");
    }

#if FAKE_ECS
    // C++-style transcoding to integers
    static public Map decode(int n) { return _transcode[n]; }
    public int encode() { return _transcode.IndexOf(this); }
#endif

    private void ReconstructAuxiliaryFields()
    {
      Engine.LOS.Now(this);
      m_aux_ActorsByPosition.Clear();
      foreach (Actor mActors in m_ActorsList) {
        // XXX defensive coding: it is possible for actors to duplicate, apparently
        if (m_aux_ActorsByPosition.TryGetValue(mActors.Location.Position, out var doppleganger)) {
          if (  mActors.Name != doppleganger.Name
             || mActors.SpawnTime!=doppleganger.SpawnTime)
            throw new InvalidOperationException("non-clone savefile corruption");
        } else {
          m_aux_ActorsByPosition.Add(mActors.Location.Position, mActors);
        }
        (mActors.Controller as PlayerController)?.InstallHandlers();
      }
      m_aux_MapObjectsByPosition.Clear();
      foreach (var obj in m_MapObjectsList) m_aux_MapObjectsByPosition.Add(obj.Location.Position, obj);
      m_aux_CorpsesByPosition.Clear();
      foreach (var mCorpses in m_CorpsesList) {
        if (m_aux_CorpsesByPosition.TryGetValue(mCorpses.Position, out var corpseList))
          corpseList.Add(mCorpses);
        else
          m_aux_CorpsesByPosition.Add(mCorpses.Position, new List<Corpse>(1) { mCorpses });
      }
    }

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      m_ActorsList.OnlyIfNot(Actor.IsDeceased);

      m_ActorsList.TrimExcess();    // 2019-09-28: unsure if these actually do anything useful (inherited from Alpha 9)
      m_MapObjectsList.TrimExcess();
      m_Zones.TrimExcess();
      m_CorpsesList.TrimExcess();
      m_Timers.TrimExcess();
#if DEBUG
      foreach(var x in m_GroundItemsByPosition) x.Value.RepairZeroQty();
#endif
    }

    public override int GetHashCode() { return _hash; }

    public override string ToString()
    {
      return Name+" ("+Width.ToString()+","+Height.ToString()+") in "+District.Name;
    }
  }
}
