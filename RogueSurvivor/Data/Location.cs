// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Location
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

// map coordinate definitions.  Want to switch this away from System.Drawing.Point to get a better hash function in.
using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;
using Size = Zaimoni.Data.Vector2D<short>;   // likely to go obsolete with transition to a true vector type

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal readonly struct Location : IEquatable<Location>
    {
    public readonly Map Map;
    public readonly Point Position;

    public Location(Map map, Point position)
    {
      Map = map
#if DEBUG
        ?? throw new ArgumentNullException(nameof(map))
#endif
      ;
      Position = position;
    }

    public Location(Map map, int x, int y)
    {
      Map = map
#if DEBUG
        ?? throw new ArgumentNullException(nameof(map))
#endif
      ;
      Position = new((short)x, (short)y);
    }

    // projection functions for Linq
    [NonSerialized] public static Func<Location, Point> pos = loc => loc.Position;

    public static Location operator +(Location lhs, Direction rhs)
    {
      return new Location(lhs.Map, lhs.Position+rhs);
    }

    // thin wrappers
#nullable enable
    public MapObject? MapObject { get { return Map.GetMapObjectAt(Position); } }
#nullable restore
    public bool HasMapObject { get { return Map.HasMapObjectAt(Position); } }
#nullable enable
    public Actor? Actor { get { return Map.GetActorAt(Position); } }
    public bool StrictHasActorAt { get { return Map.StrictHasActorAt(Position); } }
    public void Add(Corpse c) { Map.AddAt(c, Position); }
#nullable restore
    public void Place(Actor actor) { Map.PlaceAt(actor, in Position); }
    public void Place(MapObject obj) { Map.PlaceAt(obj, Position); }
    public void Drop(Item it) { Map.DropItemAt(it, in Position); }
    public bool IsWalkableFor(Actor actor) { return Map.IsWalkableFor(Position, actor); }
    public bool IsWalkableFor(Actor actor, out string reason) { return Map.IsWalkableFor(Position, actor, out reason); }
#nullable enable
    public Inventory? Items { get { return Map.GetItemsAt(Position); } }
    public Exit? Exit { get { return Map.GetExitAt(Position); } }
    public List<Corpse>? Corpses { get { return Map.GetCorpsesAt(Position); } }
    public Tile Tile { get { return Map.GetTileAt(Position); } }
    public TileModel TileModel { get { return Map.GetTileModelAt(Position); } }
#nullable restore
    public int IsBlockedForPathing { get { return Map.IsBlockedForPathing(Position); } }
    public void AddDecoration(string imageID) { Map.AddDecorationAt(imageID, Position); }

    static public bool IsInBounds(in Location loc) { return loc.Map.IsInBounds(loc.Position); }
#nullable enable
    static public bool RequiresJump(in Location loc) { return loc.MapObject?.IsJumpable ?? false; }
    static public bool NoJump(Location loc) { return !loc.MapObject?.IsJumpable ?? true; }
    static public bool NoJump<T>(KeyValuePair<Location,T> loc_x) { return !loc_x.Key.MapObject?.IsJumpable ?? true; }
#nullable restore

    // Map version is not cross-district
    public void ForEachAdjacent(Action<Location> op)
    {
      if (null == Map) return;
      foreach(var pt in Position.Adjacent()) {
        var test = new Location(Map,pt);
        if (Map.CanEnter(ref test)) op(test);
      }
      var e = Exit;
      if (null != e) op(e.Location);
    }

    public List<Location>? Adjacent(Predicate<Location> ok)
    {
      if (null == Map) return null;

      List<Location> ret = new();
      ForEachAdjacent(loc => {
          if (ok(loc)) ret.Add(loc);
      });

      return 0 < ret.Count ? ret : null;
    }

    public bool ChokepointIsContested(Actor viewpoint) {
      // exit-based
      var e = Exit;
      if (null != e) {
        var a = e.Location.Actor;
        if (null != a && a!=viewpoint && !a.IsEnemyOf(viewpoint)) return true;
      }
      // check map for topology-based
      foreach (var dir in Direction.COMPASS) {
        var loc = this+dir;
        if (2> Engine.Rules.GridDistance(in loc, viewpoint.Location)) continue;
        var a = loc.Actor;
        if (null == a || a.IsEnemyOf(viewpoint)) continue;
        var steps = a.LegalSteps;
        if (null == steps || 1>=steps.Count) return true;
        // \todo: educated guess based on last known move
      }

      return false;
    }

#nullable enable
    public ZoneLoc? ClearableZone { get { return  Map.ClearableZoneAt(Position); } }

    public List<ZoneLoc>? ClearableZones { get {
      var ret = new List<ZoneLoc>();
      var z = Map.ClearableZonesAt(Position);
      if (null != z) {
        ret.AddRange(z);
        return ret;
      }
      foreach(var dir in Direction.COMPASS) {
        var loc = this + dir;
        if (!Map.Canonical(ref loc)) continue;
        if (!loc.TileModel.IsWalkable) continue;
        z = loc.Map.ClearableZonesAt(loc.Position);
        if (null != z) foreach(var zone in z) if (!ret.Contains(zone)) ret.Add(zone);
      }
      return (0 < ret.Count) ? ret : null;
    } }
#nullable restore

    // AI should have similar UI to player
    // analogs of various viewing rectangles for AI use
    public Rectangle ViewRect { get { return new Rectangle(Position - (Point)Engine.RogueGame.VIEW_RADIUS, (Point)(1 + 2 * Engine.RogueGame.VIEW_RADIUS)); } }
    public ZoneLoc View { get { return new ZoneLoc(Map,new Rectangle(Position - (Point)Engine.RogueGame.VIEW_RADIUS, (Point)(1 + 2 * Engine.RogueGame.VIEW_RADIUS))); } }

    public ZoneLoc MiniMapView { get {
      if (0 >= District.UsesCrossDistrictView(Map)) {
        return new ZoneLoc(Map,Map.Rect);
      } else {
        return new ZoneLoc(Map,new Rectangle(Position - (Point)Engine.RogueGame.MINIMAP_RADIUS, (Point)(1 + 2 * Engine.RogueGame.MINIMAP_RADIUS)));
      }
    } }

    public Rectangle LocalView { get {
      return (0 == District.UsesCrossDistrictView(Map)) ? Map.Rect : ViewRect;
    } }

#nullable enable
    public ZoneLoc[]? TrivialDistanceZones { get {
        var dest = new List<ZoneLoc>();
        var z = Map.TrivialPathingFor(Position);
        if (null != z) {
          dest.AddRange(z);
          foreach(var zone in z) {
            var x = zone.ExitZones;
            if (null != x) {
                foreach(var z2 in x) if (z2.Exits.Contains(this) && !dest.Contains(z2)) dest.Add(z2);
            }
          }
        }
        foreach (var dir in Direction.COMPASS) {
            var loc = this + dir;
            if (!Map.Canonical(ref loc)) continue;
            if (!loc.TileModel.IsWalkable) continue;
            z = Map.TrivialPathingFor(Position);
            if (null != z) foreach(var zone in z) if (!dest.Contains(zone)) dest.Add(zone);
        }
        return (0 < dest.Count) ? dest.ToArray() : null;
     } }

    public bool BlocksLivingPathfinding { get {
        if (!TileModel.IsWalkable) return true;
        var obj = MapObject;
        return null != obj && obj.BlocksLivingPathfinding;
    } }
#nullable restore

    // alpha10
    public short OdorsDecay()
    {
      short decay = 1;  // base decay

      // sewers?
      if (District.IsSewersMap(Map)) decay += 2;
      // outside? = weather affected.
      else if (!Map.IsInsideAt(Position)) {   // alpha10 weather affect only outside tiles
        switch (Engine.Session.Get.World.Weather) {
        case Weather.CLEAR:
        case Weather.CLOUDY:
          // default decay.
          break;
        case Weather.RAIN:
          decay += 1;
          break;
        case Weather.HEAVY_RAIN:
          decay += 2;
          break;
        default: throw new InvalidCastException("unhandled weather");
        }
      }

      return decay;
    }

    public Rectangle RadioDistricts {
      get {
        Rectangle ret = new Rectangle(Map.DistrictPos, new Size(1, 1));
        const int radius = Engine.RogueGame.POLICE_RADIO_RANGE + 100/2; // 100: magic constant for CHAR Underground base, the single largest human-scale map in the game
        var district_size = Engine.RogueGame.Options.DistrictSize;
        var topleft = new Vector2D_stack<int>(Position.X-radius,Position.Y-radius);
        while(0>topleft.X && 0<ret.Left) {
          topleft.X += district_size;
          --ret.X;
          ++ret.Width;
        }
        while(0>topleft.Y && 0<ret.Top) {
          topleft.Y += district_size;
          --ret.Y;
          ++ret.Height;
        }
        var bottomright = new Vector2D_stack<int>(Position.X+radius,Position.Y+radius);
        while(district_size <= bottomright.X) {
          bottomright.X -= district_size;
          ++ret.Width;
        }
        while(district_size <= bottomright.Y) {
          bottomright.Y -= district_size;
          ++ret.Height;
        }
        Engine.Session.Get.World.TrimToBounds(ref ret);
        return ret;
      }
    }

#region IEquatable<>
    public static bool operator ==(Location lhs, Location rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Location lhs, Location rhs)
    {
      return !lhs.Equals(rhs);
    }

    public bool Equals(Location x)
    {
      return Map == x.Map && Position == x.Position;
    }

    public override bool Equals(object obj)
    {
      var tmp = obj as Location?;
      return null != tmp && Equals(tmp.Value);
    }

    // note that System.Drawing.Point's hashcode implementation is XOR of coordinates
    // i.e. has a high collision rate.
    public override int GetHashCode()
    {
      return Map.GetHashCode() ^ (Position.X+Engine.RogueGame.MAP_MAX_WIDTH*Position.Y);
    }
#endregion

    public override string ToString()
    {
      return Map.Name+"@"+Position.X.ToString()+","+Position.Y.ToString();
    }
  }
}
