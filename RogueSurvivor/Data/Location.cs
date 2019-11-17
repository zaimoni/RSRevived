﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Location
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using Zaimoni.Data;

// map coordinate definitions.  Want to switch this away from System.Drawing.Point to get a better hash function in.
using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
using Size = Zaimoni.Data.Vector2D_short;   // likely to go obsolete with transition to a true vector type

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal readonly struct Location : IEquatable<Location>
    {
    public readonly Map Map;
    public readonly Point Position;

    public Location(Map map, Point position)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
#endif
      Map = map;
      Position = position;
    }

    public static Location operator +(Location lhs, Direction rhs)
    {
      return new Location(lhs.Map, lhs.Position+rhs);
    }

    // thin wrappers
#nullable enable
    public MapObject? MapObject { get { return Map.GetMapObjectAt(Position); } }
#nullable restore
    public bool HasMapObject { get { return Map.HasMapObjectAt(Position); } }
    public Actor Actor { get { return Map.GetActorAt(Position); } }
    public bool StrictHasActorAt { get { return Map.StrictHasActorAt(Position); } }
#nullable enable
    public void Add(Corpse c) { Map.AddAt(c, Position); }
#nullable restore
    public void Place(Actor actor) { Map.PlaceAt(actor, in Position); }
    public void Place(MapObject obj) { Map.PlaceAt(obj, Position); }
    public void Drop(Item it) { Map.DropItemAt(it, in Position); }
    public bool IsWalkableFor(Actor actor) { return Map.IsWalkableFor(Position, actor); }
    public bool IsWalkableFor(Actor actor, out string reason) { return Map.IsWalkableFor(Position, actor, out reason); }
#nullable enable
    public Inventory? Items { get { return Map.GetItemsAt(Position); } }
#nullable restore
    public Exit Exit { get { return Map.GetExitAt(Position); } }
#nullable enable
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

    public bool ChokepointIsContested(Actor viewpoint) {
      // exit-based
      var e = Exit;
      if (null != e) {
        Actor a = e.Location.Actor;
        if (null != a && a!=viewpoint && !a.IsEnemyOf(viewpoint)) return true;
      }
      // check map for topology-based
      foreach (var dir in Direction.COMPASS) {
        var loc = this+dir;
        if (2> Engine.Rules.GridDistance(in loc, viewpoint.Location)) continue;
        Actor a = loc.Actor;
        if (null == a || a.IsEnemyOf(viewpoint)) continue;
        var steps = a.LegalSteps;
        if (null == steps || 1>=steps.Count) return true;
        // \todo: educated guess based on last known move
      }

      return false;
    }

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

    // alpha10
    public int OdorsDecay()
    {
      int decay = 1;  // base decay

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
        Rectangle ret = new Rectangle(Map.District.WorldPosition,new Size(1,1));
        if (District.IsEntryMap(Map)) return ret;  // RS behavior
        const int radius = Engine.RogueGame.POLICE_RADIO_RANGE + 100/2; // 100: magic constant for CHAR Underground base, the single largest human-scale map in the game
        var topleft = new Vector2D_int_stack(Position.X-radius,Position.Y-radius);
        while(0>topleft.X && 0<ret.Left) {
          topleft.X += Engine.RogueGame.Options.DistrictSize;
          --ret.X;
          ++ret.Width;
        }
        while(0>topleft.Y && 0<ret.Top) {
          topleft.Y += Engine.RogueGame.Options.DistrictSize;
          --ret.Y;
          ++ret.Height;
        }
        var bottomright = new Vector2D_int_stack(Position.X+radius,Position.Y+radius);
        while(Engine.RogueGame.Options.DistrictSize <= bottomright.X) {
          bottomright.X -= Engine.RogueGame.Options.DistrictSize;
          ++ret.Width;
        }
        while(Engine.RogueGame.Options.DistrictSize <= bottomright.Y) {
          bottomright.Y -= Engine.RogueGame.Options.DistrictSize;
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
