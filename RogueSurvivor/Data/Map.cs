// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Map
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Linq;
using System.Diagnostics.Contracts;

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
    private readonly Dictionary<Point,List<string>> m_Decorations = new Dictionary<Point,List<string>>();
    private readonly Dictionary<Point, Exit> m_Exits = new Dictionary<Point, Exit>();
    private readonly List<Zone> m_Zones = new List<Zone>(5);
    private readonly List<Actor> m_ActorsList = new List<Actor>(5);
    private int m_iCheckNextActorIndex;
    private readonly List<MapObject> m_MapObjectsList = new List<MapObject>(5);
    private readonly Dictionary<Point, Inventory> m_GroundItemsByPosition = new Dictionary<Point, Inventory>(5);
    private readonly List<Corpse> m_CorpsesList = new List<Corpse>(5);
    private readonly List<OdorScent> m_Scents = new List<OdorScent>(128);
    private readonly List<TimedTask> m_Timers = new List<TimedTask>(5);
    [NonSerialized]
    private readonly Dictionary<Point, Actor> m_aux_ActorsByPosition = new Dictionary<Point, Actor>(5);
    [NonSerialized]
    private readonly Dictionary<Point, MapObject> m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>(5);
    [NonSerialized]
    private readonly Dictionary<Point, List<Corpse>> m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>(5);
    [NonSerialized]
    private readonly Dictionary<Point, List<OdorScent>> m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>(128);
    [NonSerialized]
    private List<Actor> m_aux_Players;

    public bool IsSecret { get; set; }

    public Lighting Lighting
    {
      get {
        return m_Lighting;
      }
      set {
        m_Lighting = value;
      }
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

    public IEnumerable<OdorScent> Scents { get { return m_Scents; } }

    public Map(int seed, string name, District d, int width, int height)
    {
      Contract.Requires(null != name);
      Contract.Requires(0 < width);
      Contract.Requires(0 < height);
      Seed = seed;
      Name = name;
      Width = width;
      Height = height;
	  District = d;
      Rect = new Rectangle(0, 0, width, height);
      LocalTime = new WorldTime();
      Lighting = Lighting.OUTSIDE;
      IsSecret = false;
      m_TileIDs = new byte[width, height];
      m_IsInside = new byte[width*height-1/8+1];
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
      m_Scents = (List<OdorScent>) info.GetValue("m_Scents", typeof (List<OdorScent>));
      m_Timers = (List<TimedTask>) info.GetValue("m_Timers", typeof (List<TimedTask>));
      m_TileIDs = (byte[,]) info.GetValue("m_TileIDs", typeof (byte[,]));
      m_IsInside = (byte[]) info.GetValue("m_IsInside", typeof (byte[]));
      m_Decorations = (Dictionary<Point, List<string>>) info.GetValue("m_Decorations", typeof(Dictionary<Point, List<string>>));
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
      info.AddValue("m_Scents", (object)m_Scents);
      info.AddValue("m_Timers", (object)m_Timers);
      info.AddValue("m_TileIDs", (object)m_TileIDs);
      info.AddValue("m_IsInside", (object)m_IsInside);
      info.AddValue("m_Decorations", (object)m_Decorations);
    }
#endregion

    public bool IsInBounds(int x, int y)
    {
      return 0 <= x && x < Width && 0 <= y && y < Height;
    }

    public bool IsInBounds(Point p)
    {
      return IsInBounds(p.X, p.Y);
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

    public void SetTileModelAt(int x, int y, TileModel model)
    {
      if (!IsInBounds(x, y)) throw new ArgumentOutOfRangeException("position out of map bounds");
      if (model == null) throw new ArgumentNullException("model");
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
      return (m_Decorations.ContainsKey(pt) ? m_Decorations[pt] : null);
    }

    public void AddDecorationAt(string imageID, Point pt)
    {
      if (!m_Decorations.ContainsKey(pt)) m_Decorations[pt] = new List<string>() { imageID };
      else if (m_Decorations[pt].Contains(imageID)) return;
      m_Decorations[pt].Add(imageID);
    }

    public bool HasDecorationAt(string imageID, Point pt)
    {
      return m_Decorations.ContainsKey(pt) && m_Decorations[pt].Contains(imageID);
    }

    public void RemoveAllDecorationsAt(Point pt)
    {
      m_Decorations.Remove(pt);
    }

    public void RemoveDecorationAt(string imageID, Point pt)
    {
      if (!m_Decorations.ContainsKey(pt)) return;
      if (!m_Decorations[pt].Remove(imageID)) return;
      if (0 < m_Decorations.Count) return;
      m_Decorations.Remove(pt);
    }

    public Exit GetExitAt(Point pos)
    {
      Exit exit;
      if (m_Exits.TryGetValue(pos, out exit))
        return exit;
      return null;
    }

    public Exit GetExitAt(int x, int y)
    {
      return GetExitAt(new Point(x, y));
    }

    public Dictionary<Point,Exit> GetExits(Predicate<Exit> fn) {
      Contract.Requires(null != fn);
      Dictionary<Point,Exit> ret = new Dictionary<Point, Exit>();
      foreach(Point pt in m_Exits.Keys) {
        if (fn(m_Exits[pt])) ret[pt] = m_Exits[pt];
      }
      return ret;
    }

    public void SetExitAt(Point pos, Exit exit)
    {
      m_Exits.Add(pos, exit);
    }

    public void RemoveExitAt(Point pos)
    {
      m_Exits.Remove(pos);
    }

    public bool HasAnExitIn(Rectangle rect)
    {
      for (int left = rect.Left; left < rect.Right; ++left) {
        for (int top = rect.Top; top < rect.Bottom; ++top) {
          if (GetExitAt(left, top) != null) return true;
        }
      }
      return false;
    }

    public Point? GetExitPos(Exit exit)
    {
      if (exit == null) return null;
      foreach (KeyValuePair<Point, Exit> mExit in m_Exits) {
        if (mExit.Value == exit) return mExit.Key;
      }
      return null;
    }

	public List<Point> ExitLocations(IEnumerable<Exit> src)
	{
	  List<Point> ret = new List<Point>();
	  foreach(Exit e in src) {
	    Point? pt = GetExitPos(e);
		if (pt.HasValue) ret.Add(pt.Value);
	  }
	  return (0<ret.Count ? ret : null);
	}

	Dictionary<Point,int> OneStepForPathfinder(Point pt)
	{
	  Dictionary<Point,int> ret = new Dictionary<Point, int>();
	  foreach(Direction dir in Direction.COMPASS) {
	    Point dest = pt+dir;
	    if (!IsInBounds(dest)) continue;
		if (!GetTileAt(dest).Model.IsWalkable) continue;
	    MapObject tmp = GetMapObjectAt(dest);
	    if (null==tmp || tmp.IsWalkable || tmp.IsJumpable) {
	      ret[dest] = 1;
	      continue;
	    }
		DoorWindow door = tmp as DoorWindow;
		if (null != door) {
		  // door should be closed as it isn't walkable.
		  if (door.IsClosed) {
		    int cost = 2;
		    if (0<door.BarricadePoints) cost += (door.BarricadePoints+7)/8;	// handwave time cost for fully rested unarmed woman with infinite stamina
			ret[dest] = cost;
			continue;
		  }
		}
		if (tmp.IsMovable) {
		  int cost = 2;	// time cost for a non-optimal push.  Cars are jumpable so we don't get here with them.
		  ret[dest] = cost;	// time cost for pushing
		  continue;
		}
		if (tmp.BreakState == MapObject.Break.BREAKABLE) {
		  int cost = 1;
		  if (0<tmp.HitPoints) cost += (tmp.HitPoints+7)/8;	// time cost to break, as per barricade
		  ret[dest] = cost;
		  continue;
		}
	    // the following objects are neither walkable nor jumpable: burning cars, barricaded windows/doors, large fortifications, some heavy furnitute
	    // we do not want to path through burning cars.  Pathing through others is ok in some conditions but not others.
		// we probably also want to account for traps.
	  }
	  return ret;
	}

    // Default pather.  Recovery options would include allowing chat, and allowing pushing.
	public Zaimoni.Data.FloodfillPathfinder<Point> PathfindSteps(Actor actor)
	{
	  Zaimoni.Data.FloodfillPathfinder<Point> m_StepPather = null;	// convert this to a non-zerialized member variable as cache
	  if (null == m_StepPather) {
	    Func<Point, Dictionary<Point,int>> fn = (pt=>OneStepForPathfinder(pt));
	    m_StepPather = new Zaimoni.Data.FloodfillPathfinder<Point>(fn, fn, (pt=> this.IsInBounds(pt)));
	  }
      Zaimoni.Data.FloodfillPathfinder<Point> ret = new Zaimoni.Data.FloodfillPathfinder<Point>(m_StepPather);
      { 
	    Point p = new Point();
		for (p.X = 0; p.X < Width; ++p.X) {
		  for (p.Y = 0; p.Y < Height; ++p.Y) {
            if (p == actor.Location.Position) continue;
            ActorAction tmp = Engine.Rules.IsBumpableFor(actor, new Location(this, p));
            if (null==tmp) {
              m_StepPather.Blacklist(p);
              continue;
            }
            if (tmp is Engine.Actions.ActionBump) tmp = (tmp as Engine.Actions.ActionBump).ConcreteAction;
            if (Engine.Rules.IsAdjacent(p, actor.Location.Position) && tmp is Engine.Actions.ActionChat) m_StepPather.Blacklist(p);
	      }
	    }
      }
      return ret;
    }

    // for AI pathing, currently.
    private HashSet<Map> _PathTo(Map dest, out HashSet<Exit> exits)
    { // disallow the CHAR underground facility for now.  (remember to disallow secret maps even when it is enabled)
	  exits = new HashSet<Exit>(Exits.Where(e => e.IsAnAIExit && string.IsNullOrEmpty(e.ReasonIsBlocked()) && Engine.Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap!=e.ToMap));
	  // should be at least one by construction
	  HashSet<Map> exit_maps = new HashSet<Map>(exits.Select(e=>e.ToMap));
      if (1>=exit_maps.Count) return exit_maps;
      if (exit_maps.Contains(dest)) {
        exit_maps.Clear();
        exit_maps.Add(dest);
        exits.RemoveWhere(e => e.ToMap!=dest);
        return exit_maps;
      }
      // cross-district is ruled out due to AI exit restriction, currently
      return exit_maps;
    }

    // for AI pathing, currently.
    public HashSet<Map> PathTo(Map dest, out HashSet<Exit> exits)
    { 
      HashSet<Map> exit_maps = _PathTo(dest,out exits);
      if (1>=exit_maps.Count) return exit_maps;

      HashSet<Exit> inv_exits;
      HashSet<Map> inv_exit_maps = dest._PathTo(this,out inv_exits); 

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

      // cross-district is ruled out due to AI exit restriction, currently
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

    public List<Zone> GetZonesAt(int x, int y)
    {
      IEnumerable<Zone> zoneList = m_Zones.Where(z => z.Bounds.Contains(x, y));
      return zoneList.Any() ? zoneList.ToList() : null;
    }

    // XXX dead function?
    public Zone GetZoneByName(string name)
    {
      return m_Zones.FirstOrDefault(mZone => mZone.Name == name);
    }

    public Zone GetZoneByPartialName(string partOfname)
    {
      return m_Zones.FirstOrDefault(mZone => mZone.Name.Contains(partOfname));
    }

    public bool HasZonePartiallyNamedAt(Point pos, string partOfName)
    {
      List<Zone> zonesAt = GetZonesAt(pos.X, pos.Y);
      if (zonesAt == null) return false;
      return zonesAt.Any(zone=>zone.Name.Contains(partOfName));
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
      Actor actor;
      if (m_aux_ActorsByPosition.TryGetValue(position, out actor)) return actor;
      return null;
    }

    public Actor GetActorAt(int x, int y)
    {
      return GetActorAt(new Point(x, y));
    }

    public void PlaceActorAt(Actor actor, Point position)
    {
      Contract.Requires(null != actor);
      Contract.Requires(IsInBounds(position));
      Actor actorAt = GetActorAt(position);
      if (actorAt == actor) throw new InvalidOperationException("actor already at position");
      if (actorAt != null) throw new InvalidOperationException("another actor already at position");
      if (HasActor(actor))
        m_aux_ActorsByPosition.Remove(actor.Location.Position);
      else {
        m_ActorsList.Add(actor);
        if (actor.IsPlayer) m_aux_Players = null;
      }
      m_aux_ActorsByPosition.Add(position, actor);
      actor.Location = new Location(this, position);
      m_iCheckNextActorIndex = 0;
    }

    public void MoveActorToFirstPosition(Actor actor)
    {
      if (!m_ActorsList.Contains(actor)) throw new ArgumentException("actor not in map");
      if (1 == m_ActorsList.Count) return;
      m_ActorsList.Remove(actor);
      m_ActorsList.Insert(0, actor);
      m_iCheckNextActorIndex = 0;
    }

    public void RemoveActor(Actor actor)
    {
      if (!m_ActorsList.Contains(actor)) return;
      m_ActorsList.Remove(actor);
      m_aux_ActorsByPosition.Remove(actor.Location.Position);
      m_iCheckNextActorIndex = 0;
      m_aux_Players = null;
    }

    public Actor NextActorToAct { 
      get {
        Contract.Ensures(null==Contract.Result<Actor>() || (Contract.Result<Actor>().CanActThisTurn && !Contract.Result<Actor>().IsSleeping));
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
      Contract.Requires(null != actor);
      if (!IsInBounds(x, y)) return "out of map";
      if (!GetTileModelAt(x, y).IsWalkable) return "blocked";
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt != null && !mapObjectAt.IsWalkable) {
        if (mapObjectAt.IsJumpable) {
          if (!actor.CanJump) return "cannot jump";
          if (actor.StaminaPoints < Engine.Rules.STAMINA_COST_JUMP) return "not enough stamina to jump";
        } else if (actor.Model.Abilities.IsSmall) {
          DoorWindow doorWindow = mapObjectAt as DoorWindow;
          if (doorWindow != null && doorWindow.IsClosed) return "cannot slip through closed door";
        } else return "blocked by object";
      }
      if (GetActorAt(x, y) != null) return "someone is there";
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
    public List<Actor> Players { 
      get {
        if (null != m_aux_Players) return m_aux_Players;
        m_aux_Players = m_ActorsList.Where(a => a.IsPlayer && !a.IsDead).ToList();
        return m_aux_Players;
      }
    }

    public void RecalcPlayers() { m_aux_Players = null; }

    public int PlayerCount { 
      get {
        return Players.Count;
      }
    }

    public Actor FindPlayer { 
      get {
        if (0 == Players.Count) return null;
        return Players[0];
      }
    }

    public bool MessagePlayerOnce(Action<Actor> fn, Predicate<Actor> pred=null)
    {
      Contract.Requires(null!=fn);
      IEnumerable<Actor> tmp = Players;
      if (null!=pred) tmp = tmp.Where(a => pred(a));
      Actor player = tmp.FirstOrDefault();
      if (null != player) {
        RogueForm.Game.PanViewportTo(player);
        fn(player);
        return true;
      }
      return false;
    }

    // police on map
    public List<Actor> Police { 
      get {
        IEnumerable<Actor> police = m_ActorsList.Where(a=> (int)Gameplay.GameFactions.IDs.ThePolice == a.Faction.ID && !a.IsDead);
        return police.Any() ? police.ToList() : null;
      }
    }

    // map object manipulation functions
    public bool HasMapObject(MapObject mapObj)
    {
      return m_MapObjectsList.Contains(mapObj);
    }

    public MapObject GetMapObjectAt(Point position)
    {
      MapObject mapObject;
      if (m_aux_MapObjectsByPosition.TryGetValue(position, out mapObject)) {
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

    public void PlaceMapObjectAt(MapObject mapObj, Point position)
    {
      Contract.Requires(null != mapObj);
      Contract.Requires(IsInBounds(position));
      MapObject mapObjectAt = GetMapObjectAt(position);
      if (mapObjectAt == mapObj) return;
      if (mapObjectAt != null)
        throw new InvalidOperationException("another mapObject already at position");
      if (!GetTileAt(position.X, position.Y).Model.IsWalkable)
        throw new InvalidOperationException("cannot place map objects on unwalkable tiles");
      if (HasMapObject(mapObj))
        m_aux_MapObjectsByPosition.Remove(mapObj.Location.Position);
      else
        m_MapObjectsList.Add(mapObj);
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

    public void OpenAllGates()
    {
      foreach(MapObject mapObject in MapObjects.Where(obj=>Gameplay.GameImages.OBJ_GATE_CLOSED==obj.ImageID)) {
        mapObject.IsWalkable = true;
        mapObject.ImageID = Gameplay.GameImages.OBJ_GATE_OPEN;
      }
    }
 
    public Inventory GetItemsAt(Point position)
    {
      Contract.Ensures(null == Contract.Result<Inventory>() || !Contract.Result<Inventory>().IsEmpty);
      if (!IsInBounds(position)) return null;
      Inventory inventory;
      if (m_GroundItemsByPosition.TryGetValue(position, out inventory))
        return inventory;
      return null;
    }

    public Inventory GetItemsAt(int x, int y)
    {
      return GetItemsAt(new Point(x, y));
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

    public void DropItemAt(Item it, Point position)
    {
      Contract.Requires(null != it);
      Contract.Requires(IsInBounds(position));
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null) {
        Inventory inventory = new Inventory(GROUND_INVENTORY_SLOTS);
        m_GroundItemsByPosition.Add(position, inventory);
        inventory.AddAll(it);
      } else if (itemsAt.IsFull) {
        int quantity = it.Quantity;
        int quantityAdded;
        itemsAt.AddAsMuchAsPossible(it, out quantityAdded);
        if (quantityAdded >= quantity)
          return;
        itemsAt.RemoveAllQuantity(itemsAt.BottomItem);
        itemsAt.AddAsMuchAsPossible(it, out quantityAdded);
      }
      else
        itemsAt.AddAll(it);
    }

    public void DropItemAt(Item it, int x, int y)
    {
      DropItemAt(it, new Point(x, y));
    }

    public void RemoveItemAt(Item it, Point position)
    {
	  Contract.Requires(null != it);
	  Contract.Requires(IsInBounds(position));
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null) throw new ArgumentException("no items at this position");
      if (!itemsAt.Contains(it)) throw new ArgumentException("item not at this position");
      itemsAt.RemoveAllQuantity(it);
      if (!itemsAt.IsEmpty) return;
      m_GroundItemsByPosition.Remove(position);
    }

    public void RemoveItemAt(Item it, int x, int y)
    {
      RemoveItemAt(it, new Point(x, y));
    }

    public void RemoveAllItemsAt(Point position)
    {
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null) return;
      m_GroundItemsByPosition.Remove(position);
    }

    public List<Corpse> GetCorpsesAt(Point p)
    {
      List<Corpse> corpseList;
      if (m_aux_CorpsesByPosition.TryGetValue(p, out corpseList))
        return corpseList;
      return null;
    }

    public List<Corpse> GetCorpsesAt(int x, int y)
    {
      return GetCorpsesAt(new Point(x, y));
    }

    public bool HasCorpse(Corpse c)
    {
      return m_CorpsesList.Contains(c);
    }

    public void AddCorpseAt(Corpse c, Point p)
    {
      if (m_CorpsesList.Contains(c)) throw new ArgumentException("corpse already in this map");
      c.Position = p;
      m_CorpsesList.Add(c);
      InsertCorpseAtPos(c);
      c.DeadGuy.Location = new Location(this, p);
    }

    public void MoveCorpseTo(Corpse c, Point newPos)
    {
      if (!m_CorpsesList.Contains(c)) throw new ArgumentException("corpse not in this map");
      RemoveCorpseFromPos(c);
      c.Position = newPos;
      InsertCorpseAtPos(c);
      c.DeadGuy.Location = new Location(this, newPos);
    }

    public void RemoveCorpse(Corpse c)
    {
      if (!m_CorpsesList.Contains(c)) throw new ArgumentException("corpse not in this map");
      m_CorpsesList.Remove(c);
      RemoveCorpseFromPos(c);
    }

    public bool TryRemoveCorpseOf(Actor a)
    {
      foreach (Corpse mCorpses in m_CorpsesList) {
        if (mCorpses.DeadGuy == a) {
          RemoveCorpse(mCorpses);
          return true;
        }
      }
      return false;
    }

    private void RemoveCorpseFromPos(Corpse c)
    {
      List<Corpse> corpseList;
      if (!m_aux_CorpsesByPosition.TryGetValue(c.Position, out corpseList)) return;
      corpseList.Remove(c);
      if (corpseList.Count != 0) return;
      m_aux_CorpsesByPosition.Remove(c.Position);
    }

    private void InsertCorpseAtPos(Corpse c)
    {
      List<Corpse> corpseList;
      if (m_aux_CorpsesByPosition.TryGetValue(c.Position, out corpseList))
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
      if (!IsInBounds(position)) return 0;
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor != null) return scentByOdor.Strength;
      return 0;
    }

    private OdorScent GetScentByOdor(Odor odor, Point p)
    {
      List<OdorScent> odorScentList;
      if (!m_aux_ScentsByPosition.TryGetValue(p, out odorScentList)) return null;
      foreach (OdorScent odorScent in odorScentList) {
        if (odorScent.Odor == odor) return odorScent;
      }
      return null;
    }

    private void AddNewScent(OdorScent scent)
    {
      if (!m_Scents.Contains(scent)) m_Scents.Add(scent);
      List<OdorScent> odorScentList;
      if (m_aux_ScentsByPosition.TryGetValue(scent.Position, out odorScentList))
      {
        odorScentList.Add(scent);
      }
      else
      {
        odorScentList = new List<OdorScent>(2);
        odorScentList.Add(scent);
                m_aux_ScentsByPosition.Add(scent.Position, odorScentList);
      }
    }

    public void ModifyScentAt(Odor odor, int strengthChange, Point position)
    {
      if (!IsInBounds(position))
        throw new ArgumentOutOfRangeException("position");
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor == null)
                AddNewScent(new OdorScent(odor, strengthChange, position));
      else
        scentByOdor.Change(strengthChange);
    }

    public void RefreshScentAt(Odor odor, int freshStrength, Point position)
    {
      if (!IsInBounds(position))
        throw new ArgumentOutOfRangeException(string.Format("position; ({0},{1}) map {2} odor {3}", (object) position.X, (object) position.Y, (object) Name, (object) odor.ToString()));
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor == null)
      {
        AddNewScent(new OdorScent(odor, freshStrength, position));
      }
      else
      {
        if (scentByOdor.Strength >= freshStrength) return;
        scentByOdor.Set(freshStrength);
      }
    }

    public void RemoveScent(OdorScent scent)
    {
            m_Scents.Remove(scent);
      List<OdorScent> odorScentList;
      if (!m_aux_ScentsByPosition.TryGetValue(scent.Position, out odorScentList))
        return;
      odorScentList.Remove(scent);
      if (odorScentList.Count != 0)
        return;
            m_aux_ScentsByPosition.Remove(scent.Position);
    }

    public bool IsTransparent(int x, int y)
    {
      if (!IsInBounds(x, y) || !GetTileModelAt(x, y).IsTransparent)
        return false;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return true;
      return mapObjectAt.IsTransparent;
    }

    public bool IsWalkable(int x, int y)
    {
      if (!IsInBounds(x, y) || !GetTileModelAt(x, y).IsWalkable)
        return false;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return true;
      return mapObjectAt.IsWalkable;
    }

    public bool IsWalkable(Point p)
    {
      return IsWalkable(p.X, p.Y);
    }

    public bool IsBlockingFire(int x, int y)
    {
      if (!IsInBounds(x, y) || !GetTileModelAt(x, y).IsTransparent)
        return true;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsTransparent || GetActorAt(x, y) != null;
    }

    public bool IsBlockingThrow(int x, int y)
    {
      if (!IsInBounds(x, y) || !GetTileModelAt(x, y).IsWalkable)
        return true;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsWalkable && !mapObjectAt.IsJumpable;
    }

    public List<Point> FilterAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position)) return null;
      IEnumerable<Point> tmp = Direction.COMPASS.Select(dir=>position+dir).Where(p=>IsInBounds(p) && predicateFn(p));
      return (0<tmp.Count() ? new List<Point>(tmp) : null);
    }

    public bool HasAnyAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position)) return false;
      return Direction.COMPASS.Select(dir => position + dir).Any(p=>IsInBounds(p) && predicateFn(p));
    }

    public int CountAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position)) return 0;
      return Direction.COMPASS.Select(dir => position + dir).Count(p=>IsInBounds(p) && predicateFn(p));
    }

    public void ForEachAdjacentInMap(Point position, Action<Point> fn)
    {
      Contract.Requires(null != fn);
      if (!IsInBounds(position)) return;
      foreach (Direction direction in Direction.COMPASS) {
        Point p = position + direction;
        if (IsInBounds(p)) fn(p);
      }
    }

    public Point? FindFirstInMap(Predicate<Point> predicateFn)
    {
      Contract.Requires(null != predicateFn);
      Point point = new Point();
      for (point.X = 0; point.X < Width; ++point.X) {
        for (point.Y = 0; point.Y < Height; ++point.Y) {
          if (predicateFn(point)) return point;
        }
      }
      return null;
    }

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
      List<string> actor_data = new List<string>();
      string[][] ascii_map = new string[Height][];
      foreach(int y in Enumerable.Range(0, Height)) {
        ascii_map[y] = new string[Width];
        foreach(int x in Enumerable.Range(0, Width)) {
          // XXX does not handle transparent walls or opaque non-walls
          ascii_map[y][x] = (GetTileModelAt(x,y).IsWalkable ? "." : "#");    // typical floor tile if walkable, typical wall otherwise
          if (null!=GetExitAt(x,y)) ascii_map[y][x] = ">";                  // downwards exit
#region map objects
          MapObject tmp_obj = GetMapObjectAt(x,y);  // micro-optimization target (one Point temporary involved)
          if (null!=tmp_obj) {
            if (tmp_obj.IsCouch) {
              ascii_map[y][x] = "="; // XXX no good icon for bed...we have no rings so this is not-awful
            } else if (tmp_obj.IsTransparent && !tmp_obj.IsWalkable) { 
              ascii_map[y][x] = "|"; // either a gate or an iron wall.
            } else {
              Engine.MapObjects.DoorWindow tmp_door = tmp_obj as Engine.MapObjects.DoorWindow;
              if (null!=tmp_door) {
                if (tmp_door.IsBarricaded) {
                  ascii_map[y][x] = "+"; // no good icon...pretend it's a closed door
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
            string item_str = "&"; // Angband/Nethack pile.
            foreach (Item it in inv.Items) {
              inv_data.Add("<tr><td>"+p_txt+"</td><td>"+it.Model.ID.ToString()+"</td></tr>\n");
            }
            ascii_map[y][x] = item_str;
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
                a_str = "<span style='background:lightblue'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.GANGSTA_MAN:
                a_str = "<span style='background:red;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.BLACKOPS_MAN:
                a_str = "<span style='background:black;color:white'>"+a_str+"</span>"; break;
              case Gameplay.GameActors.IDs.SEWERS_THING:
              case Gameplay.GameActors.IDs.JASON_MYERS:
                a_str = "<span style='background:darkred;color:white'>"+a_str+"</span>"; break;
            }          
            actor_data.Add("<tr><td"+ pos_css + ">" + p_txt + "</td><td>" + a.UnmodifiedName + "</td><td>"+a.ActionPoints.ToString()+ "</td><td>"+a.HitPoints.ToString()+ "</td></tr>\n");
            ascii_map[a.Location.Position.Y][a.Location.Position.X] = a_str;
          }
#endregion
        }
      }
      if (0>=inv_data.Count && 0>=actor_data.Count) return;
      if (0<actor_data.Count) {
        dest.WriteLine("<table border=2 cellspacing=1 cellpadding=1 align=left>");
        dest.WriteLine("<tr><th>pos</th><th>name</th><th>AP</th><th>HP</th></tr>");
        foreach(string s in actor_data) dest.WriteLine(s);
        dest.WriteLine("</table>");
      }
      if (0<inv_data.Count) {
        dest.WriteLine("<table border=2 cellspacing=1 cellpadding=1 align=right>");
        foreach(string s in inv_data) dest.WriteLine(s);
        dest.WriteLine("</table>");
      }
      dest.WriteLine("<pre style='clear:both'>");
      foreach (int y in Enumerable.Range(0, Height)) {
        dest.WriteLine(String.Join("",ascii_map[y]));
      }
      dest.WriteLine("</pre>");
    }

    private void ReconstructAuxiliaryFields()
    {
      m_aux_ActorsByPosition.Clear();
      foreach (Actor mActors in m_ActorsList)
        m_aux_ActorsByPosition.Add(mActors.Location.Position, mActors);
      m_aux_MapObjectsByPosition.Clear();
      foreach (MapObject mMapObjects in m_MapObjectsList)
        m_aux_MapObjectsByPosition.Add(mMapObjects.Location.Position, mMapObjects);
      m_aux_ScentsByPosition.Clear();
      foreach (OdorScent mScent in m_Scents) {
        List<OdorScent> odorScentList;
        if (m_aux_ScentsByPosition.TryGetValue(mScent.Position, out odorScentList)) {
          odorScentList.Add(mScent);
        } else {
          odorScentList = new List<OdorScent>()
          {
            mScent
          };
          m_aux_ScentsByPosition.Add(mScent.Position, odorScentList);
        }
      }
      m_aux_CorpsesByPosition.Clear();
      foreach (Corpse mCorpses in m_CorpsesList) {
        List<Corpse> corpseList;
        if (m_aux_CorpsesByPosition.TryGetValue(mCorpses.Position, out corpseList))
          corpseList.Add(mCorpses);
        else
          m_aux_CorpsesByPosition.Add(mCorpses.Position, new List<Corpse>(1)
          {
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

      m_aux_Players = null;
    }

    public void OptimizeBeforeSaving()
    {
      int i = 0;
      if (null != m_ActorsList) {
        i = m_ActorsList.Count;
        while (0 < i--) {
          if (m_ActorsList[i].IsDead) m_ActorsList.RemoveAt(i);
        }
      }

      foreach (Actor mActors in m_ActorsList)
        mActors.OptimizeBeforeSaving();
      m_ActorsList.TrimExcess();
      m_MapObjectsList.TrimExcess();
      m_Scents.TrimExcess();
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
