// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Map
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define NO_PEACE_WALLS

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Linq;
using Zaimoni.Data;

using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Map : ISerializable
  {
    public const int GROUND_INVENTORY_SLOTS = 10;
    public readonly int Seed;
    public readonly District District;
	public readonly string Name;
    private Lighting m_Lighting;
	public readonly WorldTime LocalTime;
	public readonly int Width;
	public readonly int Height;
	public readonly Rectangle Rect;
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
    private readonly List<TimedTask> m_Timers = new List<TimedTask>(5);
    // position inverting caches
    [NonSerialized]
    private readonly Dictionary<Point, Actor> m_aux_ActorsByPosition = new Dictionary<Point, Actor>(5);
    [NonSerialized]
    private readonly Dictionary<Point, MapObject> m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>(5);
    [NonSerialized]
    private readonly Dictionary<Point, List<Corpse>> m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>(5);
    // AI support caches, etc.
    [NonSerialized]
    public readonly NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>> Players;
    [NonSerialized]
    public readonly NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>> Police;
    [NonSerialized]
    public readonly Dataflow<List<Actor>,int> UndeadCount;
    [NonSerialized]
    public readonly NonSerializedCache<List<MapObject>, Engine.MapObjects.PowerGenerator, ReadOnlyCollection<Engine.MapObjects.PowerGenerator>> PowerGenerators;
    [NonSerialized]
    public readonly Dataflow<Map,Point,Exit> AI_exits;
    [NonSerialized]
    public readonly NonSerializedCache<Map, Map, HashSet<Map>> destination_maps;
    [NonSerialized]
    public readonly Dataflow<Map,Point,Exit> entrymap_exits;
    [NonSerialized]
    public readonly NonSerializedCache<Map, Map, HashSet<Map>> entrymap_destination_maps;

    public bool IsSecret { get; private set; }

    public void Expose() {
      IsSecret = false;
      foreach(Map m in destination_maps.Get) {
        m.destination_maps.Recalc();
      }
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
    public int CountActors { get { return m_ActorsList.Count; } }

    public int CheckNextActorIndex
    {
      get {
        return m_iCheckNextActorIndex;
      }
      set { // nominates RogueGame::NextMapTurn for conversion to Map member function
        m_iCheckNextActorIndex = value;
      }
    }

    public IEnumerable<MapObject> MapObjects { get { return m_MapObjectsList; } }
    public IEnumerable<Inventory> GroundInventories { get { return m_GroundItemsByPosition.Values; } }
    public IEnumerable<Corpse> Corpses { get { return m_CorpsesList; } }
    public int CountCorpses { get { return m_CorpsesList.Count; } }
    public IEnumerable<TimedTask> Timers { get { return m_Timers; } }

    public int CountTimers {
      get {
        return (m_Timers != null ? m_Timers.Count : 0);
      }
    }

    private static ReadOnlyCollection<Actor> _findPlayers(IEnumerable<Actor> src)
    {
      return new ReadOnlyCollection<Actor>(src.Where(a => a.IsPlayer && !a.IsDead).ToList());
    }

    private static ReadOnlyCollection<Actor> _findPolice(IEnumerable<Actor> src)
    {
      return new ReadOnlyCollection<Actor>(src.Where(a => (int)Gameplay.GameFactions.IDs.ThePolice == a.Faction.ID && !a.IsDead).ToList());
    }

    private static int _countUndead(IEnumerable<Actor> src)
    {
      return src.Count(a => a.Model.Abilities.IsUndead);
    }

    private static ReadOnlyCollection<Engine.MapObjects.PowerGenerator> _findPowerGenerators(IEnumerable<MapObject> src)
    {
      return new ReadOnlyCollection<Engine.MapObjects.PowerGenerator>(src.OfType< Engine.MapObjects.PowerGenerator >().ToList());
    }

    private static Dictionary<Point,Exit> _FindAIexits(Map m) { return m.GetExits(exit => exit.IsAnAIExit); }
    private static Dictionary<Point,Exit> _Find_entrymap_exits(Map m) { return m.GetExits(exit => exit.ToMap==exit.ToMap.District.EntryMap); }

    public Map(int seed, string name, District d, int width, int height, Lighting light=Lighting.OUTSIDE, bool secret=false)
    {
#if DEBUG
      if (null == name) throw new ArgumentNullException(nameof(name));
      if (0 >= width) throw new ArgumentOutOfRangeException(nameof(width), width, "0 >= width");
      if (0 >= height) throw new ArgumentOutOfRangeException(nameof(height), height, "0 >= height");
#endif
      Seed = seed;
      Name = name;
      Width = width;
      Height = height;
	  District = d;
      Rect = new Rectangle(0, 0, width, height);
      LocalTime = new WorldTime();
      m_Lighting = light;
      IsSecret = secret;
      m_TileIDs = new byte[width, height];
      m_IsInside = new byte[width*height-1/8+1];
      Players = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPlayers);
      Police = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPolice);
      UndeadCount = new Dataflow<List<Actor>, int>(m_ActorsList, _countUndead); // XXX ... could eliminate this by rewriting end-of-turn event simulation
      PowerGenerators = new NonSerializedCache<List<MapObject>, Engine.MapObjects.PowerGenerator, ReadOnlyCollection<Engine.MapObjects.PowerGenerator>>(m_MapObjectsList, _findPowerGenerators);
      AI_exits = new Dataflow<Map, Point, Exit>(this, _FindAIexits);
      destination_maps = new NonSerializedCache<Map, Map, HashSet<Map>>(this,m=>new HashSet<Map>(AI_exits.Get.Values.Select(exit => exit.ToMap).Where(map => !map.IsSecret)));
      entrymap_exits = new Dataflow<Map, Point, Exit>(this, _Find_entrymap_exits);
      entrymap_destination_maps = new NonSerializedCache<Map, Map, HashSet<Map>>(this,m=>new HashSet<Map>(entrymap_exits.Get.Values.Select(exit => exit.ToMap)));
    }

#region Implement ISerializable
    protected Map(SerializationInfo info, StreamingContext context)
    {
      Seed = (int) info.GetValue("m_Seed", typeof (int));
      District = (District) info.GetValue("m_District", typeof (District));
      Name = (string) info.GetValue("m_Name", typeof (string));
      LocalTime = (WorldTime) info.GetValue("m_LocalTime", typeof (WorldTime));
      Width = (int) info.GetValue("m_Width", typeof (int));
      Height = (int) info.GetValue("m_Height", typeof (int));
      Rect = (Rectangle) info.GetValue("m_Rectangle", typeof (Rectangle));
      m_Exits = (Dictionary<Point, Exit>) info.GetValue("m_Exits", typeof (Dictionary<Point, Exit>));
      m_Zones = (List<Zone>) info.GetValue("m_Zones", typeof (List<Zone>));
      m_ActorsList = (List<Actor>) info.GetValue("m_ActorsList", typeof (List<Actor>));
      m_MapObjectsList = (List<MapObject>) info.GetValue("m_MapObjectsList", typeof (List<MapObject>));
      m_GroundItemsByPosition = (Dictionary<Point, Inventory>) info.GetValue("m_GroundItemsByPosition", typeof (Dictionary<Point, Inventory>));
      m_CorpsesList = (List<Corpse>) info.GetValue("m_CorpsesList", typeof (List<Corpse>));
      m_Lighting = (Lighting) info.GetValue("m_Lighting", typeof (Lighting));
      m_ScentsByPosition = (Dictionary<Point, List<OdorScent>>) info.GetValue("m_ScentsByPosition", typeof (Dictionary<Point, List<OdorScent>>));
      m_Timers = (List<TimedTask>) info.GetValue("m_Timers", typeof (List<TimedTask>));
      m_TileIDs = (byte[,]) info.GetValue("m_TileIDs", typeof (byte[,]));
      m_IsInside = (byte[]) info.GetValue("m_IsInside", typeof (byte[]));
      m_Decorations = (Dictionary<Point, HashSet<string>>) info.GetValue("m_Decorations", typeof(Dictionary<Point, HashSet<string>>));
      Players = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPlayers);
      Police = new NonSerializedCache<List<Actor>, Actor, ReadOnlyCollection<Actor>>(m_ActorsList, _findPolice);
      UndeadCount = new Dataflow<List<Actor>, int>(m_ActorsList,_countUndead);
      PowerGenerators = new NonSerializedCache<List<MapObject>, Engine.MapObjects.PowerGenerator, ReadOnlyCollection<Engine.MapObjects.PowerGenerator>>(m_MapObjectsList, _findPowerGenerators);
      AI_exits = new Dataflow<Map, Point, Exit>(this, _FindAIexits);
      destination_maps = new NonSerializedCache<Map, Map, HashSet<Map>>(this,m=>new HashSet<Map>(AI_exits.Get.Values.Select(exit => exit.ToMap).Where(map => !map.IsSecret)));
      entrymap_exits = new Dataflow<Map, Point, Exit>(this, _Find_entrymap_exits);
      entrymap_destination_maps = new NonSerializedCache<Map, Map, HashSet<Map>>(this,m=>new HashSet<Map>(entrymap_exits.Get.Values.Select(exit => exit.ToMap)));
      ReconstructAuxiliaryFields();
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_Seed", Seed);
      info.AddValue("m_District", (object)District);
      info.AddValue("m_Name", (object)Name);
      info.AddValue("m_LocalTime", (object)LocalTime);
      info.AddValue("m_Width", Width);
      info.AddValue("m_Height", Height);
      info.AddValue("m_Rectangle", (object)Rect);
      info.AddValue("m_Exits", (object)m_Exits);
      info.AddValue("m_Zones", (object)m_Zones);
      info.AddValue("m_ActorsList", (object)m_ActorsList);
      info.AddValue("m_MapObjectsList", (object)m_MapObjectsList);
      info.AddValue("m_GroundItemsByPosition", (object)m_GroundItemsByPosition);
      info.AddValue("m_CorpsesList", (object)m_CorpsesList);
      info.AddValue("m_Lighting", (object)m_Lighting);
      info.AddValue("m_ScentsByPosition", (object)m_ScentsByPosition);
      info.AddValue("m_Timers", (object)m_Timers);
      info.AddValue("m_TileIDs", (object)m_TileIDs);
      info.AddValue("m_IsInside", (object)m_IsInside);
      info.AddValue("m_Decorations", (object)m_Decorations);
    }
#endregion

    // once the peace walls are down, IsInBounds will refer to the actual map data.
    // IsValid will allow "translating" coordinates to adjacent maps in order to fulfil the dereference
    // IsStrictlyValid will *require* "translating" coordinates to adjacent maps in order to fulfil the dereference
    // That is, IsValid := IsInBounds XOR IsStrictlyValid
    public bool IsInBounds(int x, int y)
    {
      return 0 <= x && x < Width && 0 <= y && y < Height;
    }

    public bool IsInBounds(Point p)
    {
      return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
    }

    // return value of zero may be either "in bounds", or "not valid at all"
    public int DistrictDeltaCode(Point pt)
    {
      int ret = 0;

      if (0>pt.X) ret -= 1;
      else if (Width <= pt.X) ret += 1;

      if (0>pt.Y) ret -= 3;
      else if (Height <= pt.Y) ret += 3;

      return ret;
    }

    public void TrimToBounds(ref int x, ref int y)
    {
      if (x < 0) x = 0;
      else if (x > Width - 1) x = Width - 1;
      if (y < 0) y = 0;
      else if (y > Height - 1) y = Height - 1;
    }

    public void TrimToBounds(ref Point p)
    {
      if (p.X < 0) p.X = 0;
      else if (p.X > Width - 1) p.X = Width - 1;
      if (p.Y < 0) p.Y = 0;
      else if (p.Y > Height - 1) p.Y = Height - 1;
    }

    public void TrimToBounds(ref Rectangle r)
    {
#if DEBUG
      if (r.X >= Width) throw new ArgumentOutOfRangeException(nameof(r.X),r.X, "r.X >= Width");
      if (r.Y >= Height) throw new ArgumentOutOfRangeException(nameof(r.Y),r.Y, "r.Y >= Height");
      if (0 > r.Right) throw new ArgumentOutOfRangeException(nameof(r.Right),r.Right, "0 > r.Right");
      if (0 > r.Bottom) throw new ArgumentOutOfRangeException(nameof(r.Bottom),r.Bottom, "0 > r.Bottom");
#endif
      if (r.X < 0) {
        r.Width += r.X;
        r.X = 0;
      }

      if (r.Y < 0) {
        r.Width += r.Y;
        r.Y = 0;
      }

      if (r.Right > Width-1)  r.Width -= (r.Right - Width + 1);
      if (r.Bottom > Height-1) r.Height -= (r.Bottom - Height +1);
    }

    // placeholder for define-controlled redefinitions
    public bool IsValid(int x, int y)
    {
#if NO_PEACE_WALLS
      return IsInBounds(x,y) || IsStrictlyValid(x,y);
#else
      return 0 <= x && x < Width && 0 <= y && y < Height;
#endif
    }

    public bool IsValid(Point p)
    {
#if NO_PEACE_WALLS
      return IsInBounds(p) || IsStrictlyValid(p);
#else
      return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
#endif
    }

    public bool IsStrictlyValid(int x, int y)
    {
#if NO_PEACE_WALLS
      return null != Normalize(new Point(x,y));
#else
      return false;
#endif
    }

    public bool IsStrictlyValid(Point p)
    {
#if NO_PEACE_WALLS
      return null != Normalize(p);
#else
      return false;
#endif
    }
    // end placeholder for define-controlled redefinitions

    static public int UsesCrossDistrictView(Map m)
    {
      return m.District.UsesCrossDistrictView(m);
    }

    public Location? Normalize(Point pt)
    {
      if (IsInBounds(pt)) return null;
      int map_code = UsesCrossDistrictView(this);
      if (0>=map_code) return null;
      int delta_code = DistrictDeltaCode(pt);
      if (0==delta_code) return null;
      Point new_district = District.WorldPosition;    // System.Drawing.Point is a struct: this is a value copy
      Point district_delta = new Point(0,0);
      while(0!=delta_code) {
        Point tmp = Zaimoni.Data.ext_Drawing.sgn_from_delta_code(ref delta_code);
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
      if (dest.IsInBounds(pt)) return new Location(dest,pt);
      return dest.Normalize(pt);
    }

    public Location? Denormalize(Location loc)
    {
      if (this == loc.Map && IsValid(loc.Position)) return loc;
#if NO_PEACE_WALLS
      int map_code = UsesCrossDistrictView(this);
      if (0>=map_code) return null;
      if (map_code != UsesCrossDistrictView(loc.Map)) return null;
      Point district_delta = new Point(loc.Map.District.WorldPosition.X-District.WorldPosition.X, loc.Map.District.WorldPosition.Y - District.WorldPosition.Y);
      if (-1 > district_delta.X || 1 < district_delta.X) return null;   // XXX \todo fails for minimap if district size < 50
      if (-1 > district_delta.Y || 1 < district_delta.Y) return null;
      Point not_in_bounds = loc.Position;
      switch(district_delta.X)
      {
      case 1:
        not_in_bounds.X += Width;
        break;
      case -1:
        not_in_bounds.X -= loc.Map.Width;
        break;
      };
      switch(district_delta.Y)
      {
      case 1:
        not_in_bounds.Y += Height;
        break;
      case -1:
        not_in_bounds.Y -= loc.Map.Height;
        break;
      };
      return new Location(this,not_in_bounds);
#else
      return null;
#endif
    }

    public bool IsInViewRect(Location loc, Rectangle view)
    {
      if (this != loc.Map) {
        Location? test = Denormalize(loc);
        if (null == test) return false;
        loc = test.Value;
      }
      return view.Left <= loc.Position.X && view.Right > loc.Position.X && view.Top <= loc.Position.Y && view.Bottom > loc.Position.Y;
    }

    // these two look wrong, may need fixing later
    public bool IsMapBoundary(int x, int y)
    {
      return -1 == x || x == Width || -1 == y || y == Height;
    }

    public bool IsOnMapBorder(int x, int y)
    {
      return 0 == x || x == Width-1 || 0 == y || y == Height-1;
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

    // for when coordinates may be denormalized
    public Tile GetTileAtExt(int x, int y)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(x,y)) return GetTileAt(x,y);
      Location? loc = Normalize(new Point(x,y));
//    if (null == loc) throw ...;
      return loc.Value.Map.GetTileAt(loc.Value.Position);
#else
      int i = y*Width+x;
      return new Tile(m_TileIDs[x,y],(0!=(m_IsInside[i/8] & (1<<(i%8)))),new Location(this,new Point(x,y)));
#endif
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

    // for when coordinates may be denormalized
    public Tile GetTileAtExt(Point p)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(p)) return GetTileAt(p);
      Location? loc = Normalize(p);
//    if (null == loc) throw ...;
      return loc.Value.Map.GetTileAt(loc.Value.Position);
#else
      int i = p.Y*Width+p.X;
      return new Tile(m_TileIDs[p.X,p.Y],(0!=(m_IsInside[i/8] & (1<<(i%8)))),new Location(this,p));
#endif
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
      Location? test = Normalize(p);
      if (null == test) return false;
      return test.Value.Map.IsInsideAt(test.Value.Position);
    }

    public void SetTileModelAt(int x, int y, TileModel model)
    {
#if DEBUG
      if (null == model) throw new ArgumentNullException(nameof(model));
      if (!IsInBounds(x, y)) throw new ArgumentOutOfRangeException("("+nameof(x)+","+nameof(y)+")", "(" + x.ToString() + "," + y.ToString() + ")", "!IsInBounds(x,y)");
#endif
      m_TileIDs[x, y] = (byte)(model.ID);
    }

    public TileModel GetTileModelAt(int x, int y)
    {
      return Models.Tiles[m_TileIDs[x,y]];
    }

    public TileModel GetTileModelAt(Point pt)
    {
      return GetTileModelAt(pt.X,pt.Y);
    }

    // possibly denormalized versions
    public TileModel GetTileModelAtExt(int x, int y)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(x,y)) return GetTileModelAt(x,y);
      Location? loc = Normalize(new Point(x,y));
//    if (null == loc) throw ...;
      return loc.Value.Map.GetTileModelAt(loc.Value.Position);
#else
      return Models.Tiles[m_TileIDs[x,y]];
#endif
    }

    public TileModel GetTileModelAtExt(Point pt)
    {
      return GetTileModelAtExt(pt.X,pt.Y);
    }

    // thin wrappers based on Tile API
    public bool HasDecorationsAt(int x, int y)
    {
      return HasDecorationsAt(new Point(x,y));
    }

    public IEnumerable<string> DecorationsAt(int x, int y)
    {
      return DecorationsAt(new Point(x,y));
    }

    public void AddDecorationAt(string imageID, int x, int y)
    {
      AddDecorationAt(imageID,new Point(x,y));
    }

    public bool HasDecorationAt(string imageID, int x, int y)
    {
      return HasDecorationAt(imageID,new Point(x,y));
    }

    public void RemoveAllDecorationsAt(int x, int y)
    {
      RemoveAllDecorationsAt(new Point(x,y));
    }

    public void RemoveDecorationAt(string imageID, int x, int y)
    {
      RemoveDecorationAt(imageID,new Point(x,y));
    }

    public bool HasDecorationsAt(Point pt)
    {
      return m_Decorations.ContainsKey(pt);
    }

    public IEnumerable<string> DecorationsAt(Point pt)
    {
      m_Decorations.TryGetValue(pt, out HashSet<string> ret);
      return ret;
    }

    public void AddDecorationAt(string imageID, Point pt)
    {
      if (m_Decorations.TryGetValue(pt, out HashSet<string> ret)) {
        ret.Add(imageID);
      } else {
        m_Decorations[pt] = new HashSet<string>{ imageID };
      }
    }

    public bool HasDecorationAt(string imageID, Point pt)
    {
      return m_Decorations.TryGetValue(pt, out HashSet<string> ret) && ret.Contains(imageID);
    }

    public void RemoveAllDecorationsAt(Point pt) { m_Decorations.Remove(pt); }

    public void RemoveDecorationAt(string imageID, Point pt)
    {
      if (!m_Decorations.TryGetValue(pt, out HashSet<string> ret)) return;
      if (!ret.Remove(imageID)) return;
      if (0 < ret.Count) return;
      m_Decorations.Remove(pt);
    }

    public bool HasExitAt(Point pos) { return m_Exits.ContainsKey(pos); }
    public bool HasExitAt(int x, int y) { return m_Exits.ContainsKey(new Point(x,y)); }

    public bool HasExitAtExt(Point pos)
    {
      if (IsInBounds(pos)) return HasExitAt(pos);
      Location? test = Normalize(pos);
      if (null == test) return false;
      return test.Value.Map.HasExitAt(test.Value.Position);
    }

    public Exit GetExitAt(Point pos)
    {
      if (m_Exits.TryGetValue(pos, out Exit exit)) return exit;
      return null;
    }

    public Exit GetExitAt(int x, int y) { return GetExitAt(new Point(x, y)); }

    public Dictionary<Point,Exit> GetExits(Predicate<Exit> fn) {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
      Dictionary<Point,Exit> ret = new Dictionary<Point, Exit>();
      foreach(var x in m_Exits) {
        if (fn(x.Value)) ret[x.Key] = x.Value;
      }
      return ret;
    }

    public void SetExitAt(Point pos, Exit exit) { m_Exits.Add(pos, exit); }

    public void RemoveExitAt(Point pos)
    {
      m_Exits.Remove(pos);
    }

    public bool HasAnExitIn(Rectangle rect)
    {
      return rect.Any(pt => HasExitAt(pt));
    }

    // <remakr>only caller wants result in-bounds</remark>
	public List<Point> ExitLocations(HashSet<Exit> src)
	{
      if (null==src || 0 >= src.Count) return null;
	  var ret = new HashSet<Point>();
      foreach (KeyValuePair<Point, Exit> mExit in m_Exits) {
        if (!src.Contains(mExit.Value)) continue;
        if (IsInBounds(mExit.Key)) ret.Add(mExit.Key);
        else {
          foreach(Direction dir in Direction.COMPASS) {
            Point pt = mExit.Key+dir;
            if (IsInBounds(pt)) ret.Add(pt);
          }
        }
      }
	  return (0<ret.Count ? ret.ToList() : null);
	}

    Dictionary<Location,int> OneStepForPathfinder(Location loc, Actor a, Dictionary<Location,ActorAction> already)
	{
	  var ret = new Dictionary<Location, int>();
      Dictionary<Location, ActorAction> moves = a.OnePath(loc, already);
      foreach(var move in moves) {
        if (move.Value is Engine.Actions.ActionLeaveMap) continue; // not really in bounds
        if (move.Value is Engine.Actions.ActionShove) {    // impolite so penalize just more than walking around
            ret[move.Key] = 4;
            continue;
        }
        if (   move.Value is Engine.Actions.ActionOpenDoor  // extra turn
            || move.Value is Engine.Actions.ActionPush)  // assume non-optimal
            {
            ret[move.Key] = 2;
            continue;
            }
        if (   move.Value is Engine.Actions.ActionBashDoor
            || move.Value is Engine.Actions.ActionBreak)
            {
    	    MapObject tmp = move.Key.Map.GetMapObjectAtExt(move.Key.Position);
		    int cost = 1;
            if (tmp is DoorWindow door && 0<door.BarricadePoints) cost += (door.BarricadePoints+7)/8;	// handwave time cost for fully rested unarmed woman with infinite stamina
            else cost += (tmp.HitPoints+7)/8;	// time cost to break, as per barricade
            ret[move.Key] = cost;
		    continue;
            }
        ret[move.Key] = 1;  // normal case
      }
	  return ret;
	}

    Dictionary<Point,int> OneStepForPathfinder(Point pt, Actor a, Dictionary<Point,ActorAction> already)
	{
	  var ret = new Dictionary<Point, int>();
      Dictionary<Point,ActorAction> moves = a.OnePath(this, pt, already);
      foreach(var move in moves) {
        if (move.Value is Engine.Actions.ActionLeaveMap) continue; // not really in bounds
        if (move.Value is Engine.Actions.ActionShove) {    // impolite so penalize just more than walking around
            ret[move.Key] = 4;
            continue;
        }
        if (   move.Value is Engine.Actions.ActionOpenDoor  // extra turn
            || move.Value is Engine.Actions.ActionPush)  // assume non-optimal
            {
            ret[move.Key] = 2;
            continue;
            }
        if (   move.Value is Engine.Actions.ActionBashDoor
            || move.Value is Engine.Actions.ActionBreak)
            {
    	    MapObject tmp = GetMapObjectAtExt(move.Key);
		    int cost = 1;
            if (tmp is DoorWindow door && 0<door.BarricadePoints) cost += (door.BarricadePoints+7)/8;	// handwave time cost for fully rested unarmed woman with infinite stamina
            else cost += (tmp.HitPoints+7)/8;	// time cost to break, as per barricade
            ret[move.Key] = cost;
		    continue;
            }
        ret[move.Key] = 1;  // normal case
      }
	  return ret;
	}

	public Zaimoni.Data.FloodfillPathfinder<Location> PathfindLocSteps(Actor actor)
	{
      var already = new Dictionary<Location,ActorAction>();
      Func<Location, Dictionary<Location,int>> fn = (loc=>OneStepForPathfinder(loc, actor, already));
	  var m_StepPather = new Zaimoni.Data.FloodfillPathfinder<Location>(fn, fn, (loc=> loc.Map.IsInBounds(loc.Position)));
      FloodfillPathfinder<Location> ret = new FloodfillPathfinder<Location>(m_StepPather);
      Rect.DoForEach(pt=>ret.Blacklist(new Location(this, pt)),pt=> {
        if (pt == actor.Location.Position && this == actor.Location.Map) return false;
        if (null != Engine.Rules.IsPathableFor(actor, new Location(this, pt))) return false;
        if (GetMapObjectAt(pt)?.IsContainer ?? true) return false;
        return true;
      });
      return ret;
    }

    // Default pather.  Recovery options would include allowing chat, and allowing pushing.
	public Zaimoni.Data.FloodfillPathfinder<Point> PathfindSteps(Actor actor)
	{
      var already = new Dictionary<Point,ActorAction>();
      Func<Point, Dictionary<Point,int>> fn = (pt=>OneStepForPathfinder(pt, actor, already));
	  var m_StepPather = new Zaimoni.Data.FloodfillPathfinder<Point>(fn, fn, (pt=> this.IsInBounds(pt)));
      FloodfillPathfinder<Point> ret = new FloodfillPathfinder<Point>(m_StepPather);
      Rect.DoForEach(pt=>ret.Blacklist(pt),pt=> {
        if (pt == actor.Location.Position && this == actor.Location.Map) return false;
        if (null != Engine.Rules.IsPathableFor(actor, new Location(this, pt))) return false;
        if (GetMapObjectAt(pt)?.IsContainer ?? true) return false;
        return true;
      });
      return ret;
    }

    // for AI pathing, currently.
    private HashSet<Map> _PathTo(Map dest, out HashSet<Exit> exits)
    { // disallow secret maps
	  // should be at least one by construction
	  exits = new HashSet<Exit>(AI_exits.Get.Values.Where(e => string.IsNullOrEmpty(e.ReasonIsBlocked())));
	  HashSet<Map> exit_maps = new HashSet<Map>(destination_maps.Get);
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

	  HashSet<Map> dest_exit_maps = new HashSet<Map>(dest.destination_maps.Get);
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

      if (dest.District != District) {
        int dest_extended = UsesCrossDistrictView(dest);
        if (0 == dest_extended) {
          dest = dest.District.EntryMap;
          goto retry;
        }
        if (3 == dest_extended && dest.Exits.Any(e => e.ToMap == dest.District.EntryMap)) {
          dest = dest.District.EntryMap;
          goto retry;
        }
        int this_extended = UsesCrossDistrictView(this);
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
          int x_delta = dest.District.WorldPosition.X - District.WorldPosition.X;
          int y_delta = dest.District.WorldPosition.Y - District.WorldPosition.Y;
          int abs_x_delta = (0<=x_delta ? x_delta : -x_delta);
          int abs_y_delta = (0<=y_delta ? y_delta : -y_delta);
          int sgn_x_delta = (0<=x_delta ? (0 == x_delta ? 0 : 1) : -1);
          int sgn_y_delta = (0<=y_delta ? (0 == y_delta ? 0 : 1) : -1);
          if (abs_x_delta<abs_y_delta) {
            dest = Engine.Session.Get.World[District.WorldPosition.X, District.WorldPosition.Y + sgn_y_delta].EntryMap;
            goto retry;
          } else if (abs_x_delta > abs_y_delta) {
            dest = Engine.Session.Get.World[District.WorldPosition.X + sgn_x_delta, District.WorldPosition.Y].EntryMap;
            goto retry;
          } else if (2 <= abs_x_delta) {
            dest = Engine.Session.Get.World[District.WorldPosition.X + sgn_x_delta, District.WorldPosition.Y + sgn_y_delta].EntryMap;
            goto retry;
          } else return exit_maps;  // no particular insight, not worth a debug crash
        }
      }
#if DEBUG
      if (dest.District != District) throw new InvalidOperationException("test case: cross-district map not handled");
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

      HashSet<Map> intersect = new HashSet<Map>(exit_maps);
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

    public void AddZone(Zone zone)
    {
      m_Zones.Add(zone);
    }

    public void RemoveZone(Zone zone)
    {
      m_Zones.Remove(zone);
    }

    public void RemoveAllZonesAt(int x, int y)
    {
      List<Zone> zonesAt = GetZonesAt(x, y);
      if (zonesAt == null) return;
      foreach (Zone zone in zonesAt)
        RemoveZone(zone);
    }

    /// <remark>shallow copy needed to be safe for foreach loops</remark>
    /// <returns>null, or a non-empty list of zones</returns>
    public List<Zone> GetZonesAt(int x, int y)
    {
      IEnumerable<Zone> zoneList = m_Zones.Where(z => z.Bounds.Contains(x, y));
      return zoneList.Any() ? zoneList.ToList() : null;
    }

    /// <remark>shallow copy needed to be safe for foreach loops</remark>
    /// <returns>null, or a non-empty list of zones</returns>
    public List<Zone> GetZonesAt(Point pt)
    {
      IEnumerable<Zone> zoneList = m_Zones.Where(z => z.Bounds.Contains(pt));
      return zoneList.Any() ? zoneList.ToList() : null;
    }

#if DEAD_FUNC
    public Zone GetZoneByName(string name)
    {
      return m_Zones.FirstOrDefault(mZone => mZone.Name == name);
    }
#endif

    public Zone GetZoneByPartialName(string partOfname)
    {
      return m_Zones.FirstOrDefault(mZone => mZone.Name.Contains(partOfname));
    }

    public bool HasZonePartiallyNamedAt(Point pos, string partOfName)
    {
      return GetZonesAt(pos)?.Any(zone=>zone.Name.Contains(partOfName)) ?? false;
    }

    public void OnMapGenerated()
    { // coordinates with StdTownGenerator::Generate
      // 1) flush all NoCivSpawn zones
      int i = m_Zones.Count;
      while(0 < i--) {
        if ("NoCivSpawn"==m_Zones[i].Name) m_Zones.RemoveAt(i);
      }
    }

    // Actor manipulation functions
    public bool HasActor(Actor actor)
    {
      return m_ActorsList.Contains(actor);
    }

    public Actor GetActor(int index)
    {
      return m_ActorsList[index];
    }

    public Actor GetActorAt(Point position)
    {
      if (m_aux_ActorsByPosition.TryGetValue(position, out Actor actor)) return actor;
      return null;
    }

    public Actor GetActorAt(int x, int y)
    {
      return GetActorAt(new Point(x, y));
    }

    public Actor GetActorAtExt(int x, int y)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(x,y)) return GetActorAt(new Point(x, y));
      Location? test = Normalize(new Point(x,y));
      if (null==test) return null;
      return test.Value.Map.GetActorAt(test.Value.Position);
#else
      return GetActorAt(new Point(x, y));
#endif
    }

    public Actor GetActorAtExt(Point pt)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(pt)) return GetActorAt(pt);
      Location? test = Normalize(pt);
      if (null==test) return null;
      return test.Value.Map.GetActorAt(test.Value.Position);
#else
      return GetActorAt(pt);
#endif
    }

    public bool HasActorAt(Point position)
    {
#if NO_PEACE_WALLS
      if (m_aux_ActorsByPosition.ContainsKey(position)) return true;
      if (IsInBounds(position)) return false;
      Location? tmp = Normalize(position);
      if (null == tmp) return false;
      return tmp.Value.Map.HasActorAt(tmp.Value.Position);
#else
      return m_aux_ActorsByPosition.ContainsKey(position);
#endif
    }

    public bool HasActorAt(int x, int y)
    {
      return HasActorAt(new Point(x, y));
    }

    public void PlaceAt(Actor actor, Point position)
    {
#if DEBUG
      if (null == actor) throw new ArgumentNullException(nameof(actor));
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
#if DEBUG
      Actor actorAt = GetActorAt(position);
      if (null != actorAt) throw new ArgumentOutOfRangeException(nameof(position),position, (actorAt == actor ? "actor already at position" : "another actor already at position"));
#endif
            lock (m_aux_ActorsByPosition) {
        // test game behaved rather badly when a second Samantha Collins was imprisoned on turn 0
        if (actor.Location.Map == this && HasActor(actor)) {
#if DEBUG
          if (!m_aux_ActorsByPosition.Remove(actor.Location.Position)) {
            foreach(var x in m_aux_ActorsByPosition) {
              if (x.Value==actor) throw new InvalidOperationException(actor.Name+" and map disagree on where (s)he is");
            }
          }
#else
          m_aux_ActorsByPosition.Remove(actor.Location.Position);
#endif
        } else {
          if (null != actor.Location.Map && this != actor.Location.Map) actor.Location.Map.Remove(actor);
          m_ActorsList.Add(actor);
          Engine.LOS.Now(this);
          if (actor.IsPlayer) Players.Recalc();
          if ((int)Gameplay.GameFactions.IDs.ThePolice == actor.Faction.ID) Police.Recalc();
        }
        m_aux_ActorsByPosition.Add(position, actor);
        actor.Location = new Location(this, position);
      }
      m_iCheckNextActorIndex = 0;
    }

    public void MoveActorToFirstPosition(Actor actor)
    {
      if (!m_ActorsList.Contains(actor)) throw new ArgumentException("actor not in map");
      if (1 == m_ActorsList.Count) return;
      m_ActorsList.Remove(actor);
      m_ActorsList.Insert(0, actor);
      m_iCheckNextActorIndex = 0;
      if (actor.IsPlayer) Players.Recalc();
      if ((int)Gameplay.GameFactions.IDs.ThePolice == actor.Faction.ID) Police.Recalc();
    }

    public void Remove(Actor actor)
    {
      lock(m_aux_ActorsByPosition) {
        if (m_ActorsList.Remove(actor)) {
#if DEBUG
          if (!m_aux_ActorsByPosition.Remove(actor.Location.Position)) {
            foreach(var x in m_aux_ActorsByPosition) {
              if (x.Value==actor) throw new InvalidOperationException(actor.Name+" and map disagree on where (s)he is");
            }
          }
#else
          m_aux_ActorsByPosition.Remove(actor.Location.Position);
#endif
          m_iCheckNextActorIndex = 0;
          if (actor.IsPlayer) Players.Recalc();
          if ((int)Gameplay.GameFactions.IDs.ThePolice == actor.Faction.ID) Police.Recalc();
        }
      }
    }

    public Actor NextActorToAct {
      get {
        int countActors = m_ActorsList.Count;
        for (int checkNextActorIndex = m_iCheckNextActorIndex; checkNextActorIndex < countActors; ++checkNextActorIndex) {
          Actor actor = m_ActorsList[checkNextActorIndex];
          if (actor.CanActThisTurn && !actor.IsSleeping) {
            m_iCheckNextActorIndex = checkNextActorIndex;
            return actor;
          }
        }
        return null;
      }
    }

    private string ReasonNotWalkableFor(int x, int y, Actor actor)
    {
#if DEBUG
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
#if NO_PEACE_WALLS
      if (!IsInBounds(x, y) && !HasExitAt(x,y)) return "out of map";
#else
      if (!IsInBounds(x, y)) return "out of map";
#endif
      if (!GetTileModelAtExt(x, y).IsWalkable) return "blocked";
      MapObject mapObjectAt = GetMapObjectAtExt(x, y);
      if (!mapObjectAt?.IsWalkable ?? false) {
        if (mapObjectAt.IsJumpable) {
          if (!actor.CanJump) return "cannot jump";
          if (actor.StaminaPoints < Engine.Rules.STAMINA_COST_JUMP) return "not enough stamina to jump";
        } else if (actor.Model.Abilities.IsSmall) {
          if (mapObjectAt is DoorWindow doorWindow && doorWindow.IsClosed) return "cannot slip through closed door";
        } else return "blocked by object";
      }
      if (HasActorAt(x, y)) return "someone is there";  // XXX includes actor himself
      if (actor.DraggedCorpse != null && actor.IsTired) return "dragging a corpse when tired";
      return "";
    }

    public bool IsWalkableFor(Point p, Actor actor)
    {
      return string.IsNullOrEmpty(ReasonNotWalkableFor(p.X, p.Y, actor));
    }

    public bool IsWalkableFor(Point p, Actor actor, out string reason)
    {
      reason = ReasonNotWalkableFor(p.X, p.Y, actor);
      return string.IsNullOrEmpty(reason);
    }

    // tracking players on map
    public int PlayerCount {
      get {
        return Players.Get.Count;
      }
    }

    public Actor FindPlayer {
      get {
        return Players.Get.FirstOrDefault();
      }
    }

    public Actor FindPlayerWithFOV(Point pt)
    {
      return Players.Get.FirstOrDefault(a => a.Controller.FOV.Contains(pt));
    }

    public bool MessagePlayerOnce(Action<Actor> fn, Func<Actor, bool> pred =null)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
      void pan_to(Actor a) {
          RogueForm.Game.PanViewportTo(a);
          fn(a);
      };

      return (null == pred ? Players.Get.ActOnce(pan_to)
                           : Players.Get.ActOnce(pan_to, pred));
    }

    // map object manipulation functions
    public bool HasMapObject(MapObject mapObj)
    {
      return m_MapObjectsList.Contains(mapObj);
    }

    public MapObject GetMapObjectAt(Point position)
    {
      if (m_aux_MapObjectsByPosition.TryGetValue(position, out MapObject mapObject)) {
#if DEBUG
        // existence check for bugs relating to map object location
        if (this!=mapObject.Location.Map) throw new InvalidOperationException("map object and map disagree on map");
        if (position!=mapObject.Location.Position) throw new InvalidOperationException("map object and map disagree on position");
#endif
        return mapObject;
      }
      return null;
    }

    public MapObject GetMapObjectAt(int x, int y)
    {
      return GetMapObjectAt(new Point(x, y));
    }

    public MapObject GetMapObjectAtExt(int x, int y)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(x,y)) return GetMapObjectAt(new Point(x, y));
      Location? test = Normalize(new Point(x,y));
      if (null==test) return null;
      return test.Value.Map.GetMapObjectAt(test.Value.Position);
#else
      return GetMapObjectAt(new Point(x, y));
#endif
    }

    public MapObject GetMapObjectAtExt(Point pt)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(pt)) return GetMapObjectAt(pt);
      Location? test = Normalize(pt);
      if (null==test) return null;
      return test.Value.Map.GetMapObjectAt(test.Value.Position);
#else
      return GetMapObjectAt(new Point(x, y));
#endif
    }

    public bool HasMapObjectAt(Point position)
    {
      return m_aux_MapObjectsByPosition.ContainsKey(position);
    }

    public bool HasMapObjectAt(int x, int y)
    {
      return m_aux_MapObjectsByPosition.ContainsKey(new Point(x, y));
    }

    public bool HasMapObjectAtExt(Point position)
    {
      if (m_aux_MapObjectsByPosition.ContainsKey(position)) return true;
      Location? test = Normalize(position);
      if (null == test) return false;
      return test.Value.Map.HasMapObjectAt(test.Value.Position);
    }

    public void PlaceAt(MapObject mapObj, Point position)
    {
#if DEBUG
      if (null == mapObj) throw new ArgumentNullException(nameof(mapObj));
      if (!IsValid(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsValid(position)");
      if (!GetTileModelAtExt(position).IsWalkable) throw new ArgumentOutOfRangeException(nameof(position),position, "!GetTileModelAt(position).IsWalkable");
#endif
      MapObject mapObjectAt = GetMapObjectAtExt(position);
      if (mapObjectAt == mapObj) return;
#if DEBUG
      if (null != mapObjectAt) throw new ArgumentOutOfRangeException(nameof(position), position, "null != GetMapObjectAtExt(position)");
#endif
      if (!IsInBounds(position)) {
        // cross-map push or similar
        Location? test = Normalize(position);
        test.Value.Map.PlaceAt(mapObj,test.Value.Position);
        return;
      }
      // cf Map::PlaceAt(Actor,Position)
      if (null != mapObj.Location.Map && HasMapObject(mapObj))
        m_aux_MapObjectsByPosition.Remove(mapObj.Location.Position);
      else {
        if (null != mapObj.Location.Map && this != mapObj.Location.Map) mapObj.Remove();
        m_MapObjectsList.Add(mapObj);
      }
      m_aux_MapObjectsByPosition.Add(position, mapObj);
      mapObj.Location = new Location(this, position);
    }

    public void RemoveMapObjectAt(int x, int y)
    {
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt == null) return;
      m_MapObjectsList.Remove(mapObjectAt);
      m_aux_MapObjectsByPosition.Remove(new Point(x, y));
    }

    public bool IsTrapCoveringMapObjectAt(Point pos)
    {
      MapObject mapObjectAt = GetMapObjectAt(pos);
      if (mapObjectAt == null) return false;
      if (mapObjectAt is DoorWindow) return false;
      if (mapObjectAt.IsJumpable) return true;
      return mapObjectAt.IsWalkable;
    }

    public MapObject GetTrapTriggeringMapObjectAt(Point pos)
    {
      MapObject mapObjectAt = GetMapObjectAt(pos);
      if (mapObjectAt == null) return null;
      if (mapObjectAt is DoorWindow) return null;
      if (mapObjectAt.IsJumpable) return null;
      if (mapObjectAt.IsWalkable) return null;
      return mapObjectAt;
    }

    public int TrapsMaxDamageAt(Point pos)  // XXX exceptionally likely to be a nonserialized cache target
    {
      Inventory itemsAt = GetItemsAt(pos);
      if (itemsAt == null) return 0;
      int num = 0;
      foreach (Item obj in itemsAt.Items) {
        if (obj is Engine.Items.ItemTrap trap && trap.IsActivated) num += trap.Model.Damage;
      }
      return num;
    }


    public void OpenAllGates()
    {
      foreach(MapObject obj in MapObjects) {
        if (MapObject.IDs.IRON_GATE_CLOSED != obj.ID) continue;
        obj.ID = MapObject.IDs.IRON_GATE_OPEN;
      }
    }

    public double PowerRatio {
      get {
        return (double)(PowerGenerators.Get.Count(it => it.IsOn))/PowerGenerators.Get.Count;
      }
    }

    public bool HasItemsAt(Point position)
    {
      if (!IsInBounds(position)) return false;
      return m_GroundItemsByPosition.ContainsKey(position);
    }

    public bool HasItemsAt(int x, int y)
    {
      return HasItemsAt(new Point(x, y));
    }

    public Inventory GetItemsAt(Point position)
    {
      if (!IsInBounds(position)) return null;
      if (m_GroundItemsByPosition.TryGetValue(position, out Inventory inventory)) {
#if DEBUG
        if (inventory?.IsEmpty ?? true) throw new ArgumentNullException(nameof(inventory));
#endif
        return inventory;
      }
      return null;
    }

    public Inventory GetItemsAt(int x, int y)
    {
      return GetItemsAt(new Point(x, y));
    }

    public Inventory GetItemsAtExt(int x, int y)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(x,y)) return GetItemsAt(new Point(x, y));
      Location? test = Normalize(new Point(x,y));
      if (null==test) return null;
      return test.Value.Map.GetItemsAt(test.Value.Position);
#else
      return GetItemsAt(new Point(x, y));
#endif
    }

    public Inventory GetItemsAtExt(Point pt)
    {
      return GetItemsAtExt(pt.X,pt.Y);
    }

    public Dictionary<Point, Inventory> GetAccessibleInventories(Point pt)
    {
      Dictionary<Point,Inventory> ground_inv = new Dictionary<Point, Inventory>();
      Inventory inv = GetItemsAtExt(pt);
      if (!inv?.IsEmpty ?? false) ground_inv[pt] = inv;
      foreach(var dir in Direction.COMPASS) {
        Point adjacent = pt + dir;
        inv = GetItemsAtExt(adjacent);
        if (inv?.IsEmpty ?? true) continue;
        MapObject mapObjectAt = GetMapObjectAtExt(adjacent);
        if (null == mapObjectAt) continue;
        if (!mapObjectAt.IsContainer) continue; // XXX this is scheduled for revision
        ground_inv[adjacent] = inv;
      }
      return ground_inv;
    }


    public Engine.Items.ItemTrap GetActivatedTrapAt(Point pos)
    {
      Inventory itemsAt = GetItemsAt(pos);
      return itemsAt?.GetFirstMatching<Engine.Items.ItemTrap>(it => it.IsActivated);
    }

    public Point? GetGroundInventoryPosition(Inventory groundInv)
    {
      foreach (KeyValuePair<Point, Inventory> keyValuePair in m_GroundItemsByPosition) {
        if (keyValuePair.Value == groundInv) return keyValuePair.Key;
      }
      return null;
    }

    // Clairvoyant.  Useful for fine-tuning map generation and little else
    private KeyValuePair<Point, Inventory>? GetInventoryHaving(Gameplay.GameItems.IDs id)
    {
      if (District.Maps.Contains(this)) throw new InvalidOperationException("do not use GetInventoryHaving except during map generation");
      foreach (KeyValuePair<Point, Inventory> keyValuePair in m_GroundItemsByPosition) {
        if (keyValuePair.Value.Has(id)) return keyValuePair;
      }
      return null;
    }

    public void DropItemAt(Item it, Point position)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null) {
        Inventory inventory = new Inventory(GROUND_INVENTORY_SLOTS);
        m_GroundItemsByPosition.Add(position, inventory);
        inventory.AddAll(it);
      } else if (itemsAt.IsFull) {
        int quantity = it.Quantity;
        int quantityAdded = itemsAt.AddAsMuchAsPossible(it);
        if (quantityAdded >= quantity) return;
        // ensure that legendary artifacts don't disappear (yes, could infinite-loop but there aren't that many artifacts)
        Item crushed = itemsAt.BottomItem;
        itemsAt.RemoveAllQuantity(crushed);
        while(crushed.Model.IsUnbreakable || crushed.IsUnique) {
          itemsAt.AddAll(crushed);
          crushed = itemsAt.BottomItem;
          itemsAt.RemoveAllQuantity(crushed);
        }
        itemsAt.AddAsMuchAsPossible(it);
      }
      else
        itemsAt.AddAll(it);
    }

    public void DropItemAtExt(Item it, Point position)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(position)) {
        DropItemAt(it, position);
        return;
      }
      Location? tmp = Normalize(position);
      if (null == tmp) throw new ArgumentOutOfRangeException(nameof(position),position,"invalid position for Item "+nameof(it));
      tmp.Value.Map.DropItemAt(it,tmp.Value.Position);
#else
      DropItemAt(it,position);
#endif
    }

    public void DropItemAt(Item it, int x, int y)
    {
      DropItemAt(it, new Point(x, y));
    }

    public void RemoveItemAt(Item it, Point position)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      Inventory itemsAt = GetItemsAt(position);
#if DEBUG
      if (null == itemsAt) throw new ArgumentNullException(nameof(itemsAt),":= GetItemsAt(position)");
      if (!itemsAt.Contains(it)) throw new ArgumentOutOfRangeException(nameof(itemsAt),"item not at this position");
#endif
      itemsAt.RemoveAllQuantity(it);
      if (itemsAt.IsEmpty) m_GroundItemsByPosition.Remove(position);
    }

    // Clairvoyant.
    public bool TakeItemType(Gameplay.GameItems.IDs id, Inventory dest)
    {
#if DEBUG
      if (null == dest) throw new ArgumentNullException(nameof(dest));
#endif
      var src = GetInventoryHaving(id);
      if (null == src) return false;
      Item it = src.Value.Value.GetFirst(id);
      if (null == it) return false;
      src.Value.Value.RemoveAllQuantity(it);
      dest.AddAsMuchAsPossible(it);
      if (src.Value.Value.IsEmpty) m_GroundItemsByPosition.Remove(src.Value.Key);
      return true;
    }

    // Clairvoyant.
    public bool SwapItemTypes(Gameplay.GameItems.IDs want, Gameplay.GameItems.IDs donate, Inventory dest)
    {
#if DEBUG
      if (null == dest) throw new ArgumentNullException(nameof(dest));
#endif
      Item giving = dest.GetFirst(donate);
      if (null == giving) return TakeItemType(want, dest);

      var src = GetInventoryHaving(want);
      if (null == src) return false;
      Item it = src.Value.Value.GetFirst(want);
      if (null == it) return false;

      src.Value.Value.RemoveAllQuantity(it);
      dest.RemoveAllQuantity(giving);
      src.Value.Value.AddAsMuchAsPossible(giving);
      dest.AddAsMuchAsPossible(it);
      return true;
    }

    public void RemoveItemAtExt(Item it, Point position)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (IsInBounds(position)) {
        RemoveItemAt(it,position);
        return;
      }
      Location? test = Normalize(position);
      if (null != test) test.Value.Map.RemoveItemAt(it, test.Value.Position);
    }

    public void RemoveItemAt(Item it, int x, int y)
    {
      RemoveItemAt(it, new Point(x, y));
    }

    /// <remark>Map generation depends on this being no-fail</remark>
    public void RemoveAllItemsAt(Point position)
    {
      m_GroundItemsByPosition.Remove(position);
    }

    public List<Corpse> GetCorpsesAt(Point p)
    {
      if (m_aux_CorpsesByPosition.TryGetValue(p, out List<Corpse> corpseList))
        return corpseList;
      return null;
    }

    public List<Corpse> GetCorpsesAtExt(int x, int y)
    {
#if NO_PEACE_WALLS
      if (IsInBounds(x,y)) return GetCorpsesAt(new Point(x, y));
      Location? test = Normalize(new Point(x,y));
      if (null==test) return null;
      return test.Value.Map.GetCorpsesAt(test.Value.Position);
#else
      return GetCorpsesAt(new Point(x, y));
#endif
    }

    public bool Has(Corpse c)
    {
      return m_CorpsesList.Contains(c);
    }

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
      foreach (Corpse mCorpses in m_CorpsesList) {
        if (mCorpses.DeadGuy == a) {
          Remove(mCorpses);
          return true;
        }
      }
      return false;
    }

    private void RemoveFromPos(Corpse c)
    {
      if (!m_aux_CorpsesByPosition.TryGetValue(c.Position, out List<Corpse> corpseList)) return;
      corpseList.Remove(c);
      if (corpseList.Count != 0) return;
      m_aux_CorpsesByPosition.Remove(c.Position);
    }

    private void InsertAtPos(Corpse c)
    {
      if (m_aux_CorpsesByPosition.TryGetValue(c.Position, out List<Corpse> corpseList))
        corpseList.Insert(0, c);
      else
        m_aux_CorpsesByPosition.Add(c.Position, new List<Corpse>(1) { c });
    }

    public void AddTimer(TimedTask t)
    {
      m_Timers.Add(t);
    }

    public void RemoveTimer(TimedTask t)
    {
      m_Timers.Remove(t);
    }

    public int GetScentByOdorAt(Odor odor, Point position)
    {
      if (IsInBounds(position)) {
        OdorScent scentByOdor = GetScentByOdor(odor, position);
        if (scentByOdor != null) return scentByOdor.Strength;
#if NO_PEACE_WALLS
      } else if (IsStrictlyValid(position)) {
        Location? tmp = Normalize(position);
        if (null != tmp) {
          OdorScent scentByOdor = tmp.Value.Map.GetScentByOdor(odor, tmp.Value.Position);
          if (scentByOdor != null) return scentByOdor.Strength;
        }
#endif
      }
      return 0;
    }

    private OdorScent GetScentByOdor(Odor odor, Point p)
    {
      if (!m_ScentsByPosition.TryGetValue(p, out List<OdorScent> odorScentList)) return null;
      foreach (OdorScent odorScent in odorScentList) {
        if (odorScent.Odor == odor) return odorScent;
      }
      return null;
    }

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

    public void RefreshScentAt(Odor odor, int freshStrength, Point position)
    {
#if DEBUG
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor == null) {
        AddNewScent(new OdorScent(odor, freshStrength), position);
      } else if (scentByOdor.Strength < freshStrength) {
        scentByOdor.Strength = freshStrength;
      }
    }

#if DEAD_FUNC
    public void RemoveScent(OdorScent scent)
    {
      if (!m_ScentsByPosition.TryGetValue(scent.Position, out List<OdorScent> odorScentList)) return;
      odorScentList.Remove(scent);
      if (0 >= odorScentList.Count) m_ScentsByPosition.Remove(scent.Position);
    }
#endif

    public void ApplyArtificialStench()
    {
        Dictionary<Point,int> living_suppress = new Dictionary<Point,int>();
        Dictionary<Point,int> living_generate = new Dictionary<Point,int>();
        foreach(var tmp in m_ScentsByPosition) {
          living_suppress[tmp.Key] = 0;
          living_generate[tmp.Key] = 0;
          foreach(OdorScent scent in tmp.Value) {
            switch (scent.Odor) {
              case Odor.PERFUME_LIVING_SUPRESSOR:
                living_suppress[tmp.Key] += scent.Strength;
                continue;
              case Odor.PERFUME_LIVING_GENERATOR:
                living_generate[tmp.Key] += scent.Strength;
                continue;
              default:
                continue;
            }
          }
          if (0 < living_suppress[tmp.Key] && 0 < living_generate[tmp.Key]) {
            int tmp2 = Math.Min(living_suppress[tmp.Key],living_generate[tmp.Key]);
            living_suppress[tmp.Key] -= tmp2;
            living_generate[tmp.Key] -= tmp2;
          }
          if (0 >= living_suppress[tmp.Key]) living_suppress.Remove(tmp.Key);
          if (0 >= living_generate[tmp.Key]) living_generate.Remove(tmp.Key);
        }
        foreach(var x in living_generate) ModifyScentAt(Odor.LIVING, x.Value, x.Key);
        foreach(var x2 in living_suppress) ModifyScentAt(Odor.LIVING, -x2.Value, x2.Key);
    }

    public void DecayScents(int odorDecayRate)
    {
      List<OdorScent> discard = new List<OdorScent>();
      List<Point> discard2 = new List<Point>();
      foreach(var tmp in m_ScentsByPosition) {
        foreach(OdorScent scent in tmp.Value) {
          scent.Strength -= odorDecayRate;
          if (0 >= scent.Strength) discard.Add(scent);  // XXX looks like it could depend on OdorScent being class rather than struct, but if that were to matter we'd have to lock anyway.
        }
        if (0 < discard.Count) {
          foreach(var x in discard) tmp.Value.Remove(x);
          discard.Clear();
          if (0 >= tmp.Value.Count) discard2.Add(tmp.Key);
        }
      }
      if (0 < discard2.Count) {
        foreach(var x in discard2) m_ScentsByPosition.Remove(x);
      }
    }

    public bool IsTransparent(int x, int y)
    {
      if (!IsValid(x, y) || !GetTileModelAtExt(x, y).IsTransparent) return false;
      return GetMapObjectAtExt(x, y)?.IsTransparent ?? true;
    }

    public bool IsWalkable(int x, int y)
    {
      if (!IsValid(x, y) || !GetTileModelAtExt(x, y).IsWalkable) return false;
      return GetMapObjectAtExt(x, y)?.IsWalkable ?? true;
    }

    public bool IsWalkable(Point p)
    {
      return IsWalkable(p.X, p.Y);
    }

    public bool IsBlockingFire(int x, int y)
    {
      if (!IsValid(x, y) || !GetTileModelAtExt(x, y).IsTransparent || HasActorAt(x, y)) return true;
      return !GetMapObjectAtExt(x, y)?.IsTransparent ?? false;
    }

    public bool IsBlockingThrow(int x, int y)
    {
      if (!IsValid(x, y) || !GetTileModelAtExt(x, y).IsWalkable) return true;
      MapObject mapObjectAt = GetMapObjectAtExt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsWalkable && !mapObjectAt.IsJumpable;
    }

    /// <returns>0 not blocked, 1 jumping required, 2 blocked (for livings)</returns>
    public int IsBlockedForPathing(Point pt)
    {
      // blockers are:
      // walls (hard) !map.GetTileModelAt(pt).IsWalkable
      // non-enterable objects (hard)
      // jumpable objects (soft) map.GetMapObjectAt(pt)
      if (!GetTileModelAtExt(pt)?.IsWalkable ?? true) return 2;
      MapObject obj = GetMapObjectAtExt(pt);
      if (null == obj) return 0;
      if (obj.IsCouch) return 0;
      if (obj.IsJumpable) return 1;
      return 2;
    }

    /// <returns>0 not blocked, 1 jumping required both ways, 2 one wall one jump, 3 two walls (for livings)</returns>
    private int IsPathingChokepoint(Point x0, Point x1)
    {
      int x0_blocked = IsBlockedForPathing(x0);
      if (0== x0_blocked) return 0;
      int blocked = x0_blocked*IsBlockedForPathing(x1);
      // range is: 0,1,2,4; want to return 0...3
      return 4==blocked ? 3 : blocked;
    }

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

    public Dictionary<Point,Direction> ValidDirections(Point pos, Func<Map, Point, bool> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      var ret = new Dictionary<Point,Direction>();
      foreach(Direction dir in Direction.COMPASS) {
        Point pt = pos+dir;
        if (!testFn(this,pt)) continue;
        ret[pt] = dir;
      }
      return ret;
    }

    /// <remark>testFn has to tolerate denormalized coordinates</remark>
    public Dictionary<Point,T> FindAdjacent<T>(Point pos, Func<Map,Point,T> testFn) where T:class
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
      if (!IsInBounds(pos)) throw new InvalidOperationException("!IsInBounds(pos)");
#endif
      var ret = new Dictionary<Point,T>();
      foreach(Point pt in Direction.COMPASS.Select(dir => pos + dir)) {
        T test = testFn(this,pt);
        if (null == test) continue;
        ret[pt] = test;
      }
      return ret;
    }

    public List<Point> FilterAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
#if DEBUG
      if (null == predicateFn) throw new ArgumentNullException(nameof(predicateFn));
#endif
      if (!IsInBounds(position)) return null;
      IEnumerable<Point> tmp = Direction.COMPASS.Select(dir=>position+dir).Where(p=>IsInBounds(p) && predicateFn(p));
      return (tmp.Any() ? tmp.ToList() : null);
    }

    public bool HasAnyAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
#if DEBUG
      if (null == predicateFn) throw new ArgumentNullException(nameof(predicateFn));
#endif
      if (!IsInBounds(position)) return false;
      return Direction.COMPASS.Select(dir => position + dir).Any(p=>IsInBounds(p) && predicateFn(p));
    }

    public int CountAdjacentTo(Point position, Predicate<Point> predicateFn)
    {
#if DEBUG
      if (null == predicateFn) throw new ArgumentNullException(nameof(predicateFn));
#endif
      if (!IsInBounds(position)) return 0;
      return Direction.COMPASS.Select(dir => position + dir).Count(p=>IsInBounds(p) && predicateFn(p));
    }

    public int CountAdjacentTo(int x, int y, Predicate<Point> predicateFn)
    {
      return CountAdjacentTo(new Point(x,y), predicateFn);
    }

    public void ForEachAdjacent(Point position, Action<Point> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (!IsInBounds(position)) throw new ArgumentOutOfRangeException(nameof(position),position, "!IsInBounds(position)");
#endif
      foreach (Point p in Direction.COMPASS.Select(d => position+d)) {
        if (IsInBounds(p)) fn(p);
      }
    }

#if DEAD_FUNC
    public void ForEachAdjacent(int x, int y, Action<Point> fn)
    {
      ForEachAdjacent(new Point(x,y),fn);
    }
#endif

    // cheat map similar to savefile viewer
    public void DaimonMap(Zaimoni.Data.OutTextFile dest) {
      if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
      dest.WriteLine(Name+"<br>");
      // XXX since the lock at the district level causes deadlocking, we may be inconsistent for simulation districtions
      List<Actor> tmp_Actors = (0<CountActors ? new List<Actor>(Actors) : null);
      List<Point> inv_locs = (0<m_GroundItemsByPosition.Count ? new List<Point>(m_GroundItemsByPosition.Keys) : null);
      if (null==tmp_Actors && null==inv_locs) return;
      // we have one of actors or items here...full map has motivation
      List<string> inv_data = new List<string>();
      string[] actor_headers = { "pos", "name", "Priority", "AP", "HP", "Inventory" };  // XXX would be function-static in C++
      List<string> actor_data = new List<string>();
      string[][] ascii_map = new string[Height][];
      foreach(int y in Enumerable.Range(0, Height)) {
        ascii_map[y] = new string[Width];
        foreach(int x in Enumerable.Range(0, Width)) {
          // XXX does not handle transparent walls or opaque non-walls
          ascii_map[y][x] = (GetTileModelAt(x,y).IsWalkable ? "." : "#");    // typical floor tile if walkable, typical wall otherwise
          if (HasExitAt(x,y)) ascii_map[y][x] = ">";                  // downwards exit
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
          MapObject tmp_obj = GetMapObjectAt(x,y);  // micro-optimization target (one Point temporary involved)
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
              ascii_map[y][x] = power_symbol;
            } else if (MapObject.IDs.IRON_GATE_CLOSED == tmp_obj.ID) {
              ascii_map[y][x] = closed_gate;
            } else if (MapObject.IDs.IRON_FENCE == tmp_obj.ID) {
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
		  }
#endregion
#region map inventory
          Inventory inv = GetItemsAt(x,y);
          if (null!=inv && 0<inv.CountItems) {
            string p_txt = '('+x.ToString()+','+y.ToString()+')';
            foreach (Item it in inv.Items) {
              inv_data.Add("<tr class='inv'><td>"+p_txt+"</td><td>"+it.ToString()+"</td></tr>");
            }
            ascii_map[y][x] = "&"; // Angband/Nethack pile.
          }
#endregion
#region actors
          Actor a = GetActorAt(x,y);
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
            List<string> actor_stats = new List<string> { " " };

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
            if (0 < a.CountFollowers) actor_stats.Add("<span style='background-color:black; color:cyan'>L</span>");
            if (0 < a.MurdersCounter) actor_stats.Add("<span style='background-color:black; color:red'>M</span>");

            actor_data.Add("<tr><td"+ pos_css + ">" + p_txt + "</td><td>" + a.UnmodifiedName + string.Join("", actor_stats) + "</td><td>"+m_ActorsList.IndexOf(a).ToString()+"</td><td>"+a.ActionPoints.ToString()+ "</td><td>"+a.HitPoints.ToString()+ "</td><td class='inv'>"+(null==a.Inventory ? "" : (a.Inventory.IsEmpty ? "" : a.Inventory.ToString()))+"</td></tr>");
            ascii_map[a.Location.Position.Y][a.Location.Position.X] = a_str;
          }
#endregion
        }
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

    private void ReconstructAuxiliaryFields()
    {
      Engine.LOS.Now(this);
      m_aux_ActorsByPosition.Clear();
      foreach (Actor mActors in m_ActorsList) {
        // XXX defensive coding: it is possible for actors to duplicate, apparently
        if (!m_aux_ActorsByPosition.ContainsKey(mActors.Location.Position)) {
          m_aux_ActorsByPosition.Add(mActors.Location.Position, mActors);
        } else {
          Actor doppleganger = m_aux_ActorsByPosition[mActors.Location.Position];
          if (  mActors.Name != doppleganger.Name
             || mActors.SpawnTime!=doppleganger.SpawnTime)
            throw new InvalidOperationException("non-clone savefile corruption");
        }
        (mActors.Controller as PlayerController)?.InstallHandlers();
      }
      m_aux_MapObjectsByPosition.Clear();
      foreach (MapObject mMapObjects in m_MapObjectsList)
        m_aux_MapObjectsByPosition.Add(mMapObjects.Location.Position, mMapObjects);
      m_aux_CorpsesByPosition.Clear();
      foreach (Corpse mCorpses in m_CorpsesList) {
        if (m_aux_CorpsesByPosition.TryGetValue(mCorpses.Position, out List<Corpse> corpseList))
          corpseList.Add(mCorpses);
        else
          m_aux_CorpsesByPosition.Add(mCorpses.Position, new List<Corpse>(1) {
            mCorpses
          });
      }
      // Support savefile hacking.
      // Check the actors.  If any have null controllers, intent was to hand control from the player to the AI.
      // Give them AI controllers here.
      foreach(Actor tmp in m_ActorsList) {
        if (null != tmp.Controller) continue;
        tmp.Controller = tmp.Model.InstanciateController();
      }
    }

    public void OptimizeBeforeSaving()
    {
      int i = m_ActorsList.Count;
      while (0 < i--) {
        if (m_ActorsList[i].IsDead) m_ActorsList.RemoveAt(i);
      }

      foreach (Actor mActors in m_ActorsList)
        mActors.OptimizeBeforeSaving();
      m_ActorsList.TrimExcess();
      m_MapObjectsList.TrimExcess();
      m_Zones.TrimExcess();
      m_CorpsesList.TrimExcess();
      m_Timers.TrimExcess();
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode() ^ District.GetHashCode();
    }

    public override string ToString()
    {
      return Name+" ("+Width.ToString()+","+Height.ToString()+") in "+District.Name;
    }
  }
}
