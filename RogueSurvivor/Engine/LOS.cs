// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.LOS
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine
{
  internal static class LOS
  {
    // FOV cache subsystem -- this is to enable removing FOV data from the savefile.
    // 0.9.9 unstable 2017-02-17 had a measured load time of over 3 minutes 30 seconds at turn 0, and one minute 45 seconds at turn 90.
    // at that version, the game seems to run perfectly fast once loaded (on the development machine) so trading speed of the game for speed of loading
    // makes sense.
    // 2019-04-14: while we would like to lock access to FOVcache (there is a multi-threading crash issue manifesting as an "impossible" null cache value,
    // this deadlocks on PC district change
    private static readonly Dictionary<Map,Zaimoni.Data.TimeCache<KeyValuePair<Point,int>,HashSet<Point>>> FOVcache = new Dictionary<Map,Zaimoni.Data.TimeCache<KeyValuePair<Point,int>,HashSet<Point>>>();

    public static void Expire(Map m) { if (FOVcache.TryGetValue(m,out var target) && target.Expire(m.LocalTime.TurnCounter-2)) FOVcache.Remove(m); }
    public static void Now(Map map) {
      if (!FOVcache.TryGetValue(map,out var cache)) FOVcache[map] = cache = new Zaimoni.Data.TimeCache<KeyValuePair<Point,int>,HashSet<Point>>();
      cache.Now(map.LocalTime.TurnCounter);
    }

    public static void Validate(Map map, Predicate<HashSet<Point>> fn) {
      if (FOVcache.TryGetValue(map,out var target)) target.Validate(fn);
    }

    // Optimal FOV offset subsystem to deal with some ugly inverse problems
    // this is symmetric, unlike actual FOV calculations at range 5+ (range 4- is symmetric by construction)
    private static readonly Dictionary<int,System.Collections.ObjectModel.ReadOnlyCollection<Point>> OptimalFOVOffsets = new Dictionary<int,System.Collections.ObjectModel.ReadOnlyCollection<Point>>();
    public static System.Collections.ObjectModel.ReadOnlyCollection<Point> OptimalFOV(short range)
    {
      if (OptimalFOVOffsets.TryGetValue(range,out var ret)) return ret;    // TryGetValue indicated
      List<Point> tmp = new List<Point>();
      // Cf. ComputeFOVFor
      double edge_of_maxrange = range+0.5;
      Point origin = new Point(0,0);
      Point pt = new Point();
      for (pt.X = range; pt.X > 0; --pt.X) {
        for (pt.Y = pt.X; pt.Y >= 0; --pt.Y) {
          // We want to reject points that are out of range, but still look circular in an open space
          // the historical multipler was Math.Sqrt(.75)
          // however, since we are in a cartesian gridspace the "radius to the edge of the square at max_range on the coordinate axis" is "radius to midpoint of square"+.5
          if (Rules.StdDistance(in origin, in pt) > edge_of_maxrange) continue;
          // initialize all octants at once
          tmp.Add(pt);
          tmp.Add(-pt);
          if (pt.X == pt.Y) { // diagonal
            tmp.Add(new Point(pt.X,-pt.Y));
            tmp.Add(new Point(-pt.X,pt.Y));
          } else if (0 == pt.Y) {   // cardinal
            tmp.Add(new Point(0,pt.X));
            tmp.Add(new Point(0,-pt.X));
          } else { // typical
            tmp.Add(new Point(pt.X,-pt.Y));
            tmp.Add(new Point(-pt.X,pt.Y));
            tmp.Add(new Point(pt.Y,pt.X));
            tmp.Add(new Point(pt.Y,-pt.X));
            tmp.Add(new Point(-pt.Y,pt.X));
            tmp.Add(new Point(-pt.Y,-pt.X));
          }
        }
      }
      System.Collections.ObjectModel.ReadOnlyCollection<Point> tmp2 = new System.Collections.ObjectModel.ReadOnlyCollection<Point>(tmp);
      OptimalFOVOffsets[range] = tmp2;
      return tmp2;
    }

    private static bool AngbandlikeTrace(int maxSteps, in Point from, in Point to, Func<Point, bool> fn, List<Point> line = null)
    {
#if DEBUG
        if (null == fn) throw new ArgumentNullException(nameof(fn));
        if (0 > maxSteps) throw new ArgumentOutOfRangeException("0 < maxSteps", maxSteps.ToString());
#endif
        Point start = from;
        line?.Add(start);
        if (0 == maxSteps) return true;

        Point delta = to - from;
        Point absDelta = delta.coord_xform(Math.Abs);
        var needRange = (absDelta.X < absDelta.Y ? absDelta.Y : absDelta.X);
        int actualRange = (needRange < maxSteps ? needRange : maxSteps);

        Direction tmp = Direction.To(from.X, from.Y, to.X, to.Y);
        Point end = start + needRange * tmp;
        Direction offset = Direction.To(end.X, end.Y, to.X, to.Y);
        int i = 0;
        if (offset == Direction.NEUTRAL)
            {  // cardinal direction
            do  {
                start += tmp;
                if (!fn(start)) return false;
                line?.Add(start);
                }
            while (++i < actualRange);
            return start == to;
            }
        Direction alt_step = Direction.FromVector(tmp.Vector + offset.Vector);
        var err = to - end;
        int alt_count = (0 == err.X ? err.Y : err.X);
        if (0 > alt_count) alt_count = -alt_count;

        // center to center spread is: 2 4 6 8,...
        // but we cross over at 1,1 3, 1 3 5, ...

        int knightmove_parity = 0;
        int numerator = 0;  // denominator is need range
        var knight_moves = new List<int>();
        do  {
            numerator += 2*alt_count;
            if (numerator>needRange)
                {
                start += alt_step;
                numerator -= 2*needRange;
                if (!fn(start)) return false;
                line?.Add(start);
                continue;
                }
            else if (numerator < needRange)
                {
                start += tmp;
                if (!fn(start)) return false;
                line?.Add(start);
                continue;
                };
            if (0==knightmove_parity)
                {   // chess knight's move paradox: for distance 2, we have +/1 +/2
                Point test = start+tmp;
                if (!fn(test)) {
                  knightmove_parity = -1;
                  foreach(int fix_me in knight_moves) {
                    // earlier steps must be revised
                    line[fix_me] -= tmp;
                    line[fix_me] += alt_step;
                  }
                }
                }
            if (0==knightmove_parity)
                {   // chess knight's move paradox: for distance 2, we have +/1 +/2
                Point test = start+alt_step;
                if (!fn(test)) knightmove_parity = 1;
                }
            if (0==knightmove_parity && null!=line) knight_moves.Add(line.Count);
            if (-1==knightmove_parity)
                {
                start += alt_step;
                numerator -= 2 * needRange;
                if (!fn(start)) return false;
                line?.Add(start);
                continue;
                }
//          knightmove_parity = 1;  // do not *commit* to knight move parity here (unnecessary asymmetry, interferes with cover/stealth mechanics), 0 should mean both options are legal
            start += tmp;
            if (!fn(start)) return false;
            line?.Add(start);
            }
        while (++i < actualRange);
        return start == to;
    }

    public static bool CanTraceViewLine(in Location fromLocation, Point toPosition, int maxRange = int.MaxValue, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      return AngbandlikeTrace(maxRange, fromLocation.Position, in toPosition, pt => map.IsTransparent(pt) || pt == toPosition, line);
    }

    public static bool CanTraceViewLine(in Location from, in Location to, int maxRange = int.MaxValue, List<Point> line = null)
    {
      if (from.Map == to.Map) return CanTraceViewLine(in from, to.Position, maxRange);
      Location? test = from.Map.Denormalize(in to);
      if (null == test) return false;
      return CanTraceViewLine(in from, test.Value.Position, maxRange, line);
    }

    public static Location[] IdealFireLine(in Location fromLocation, Point toPosition, int maxRange)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      var line = new List<Point>();
      if (!AngbandlikeTrace(maxRange, in start, in toPosition, pt => pt==start || pt==toPosition || !map.UnconditionallyBlockingFire(pt), line))
         return null;
      if (2 >= line.Count) return null; // nothing can get in the way

      int i = line.Count-2;
      var ret = new Location[i];
      while(0 <= --i) {
        ret[i] = new Location(map, line[i + 1]);
        Map.Canonical(ref ret[i]);  // invariant failure if this returns false
      }
      return ret;
    }

    public static Location[] IdealFireLine(Location from, Location to, int maxRange)
    {
      Location? test = from.Map.Denormalize(in to);
      if (null == test) return null;
      return IdealFireLine(in from, test.Value.Position, maxRange);
    }

    public static bool CanTraceHypotheticalFireLine(in Location fromLocation, Point toPosition, int maxRange, Actor shooter, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      return AngbandlikeTrace(maxRange, fromLocation.Position, in toPosition, pt =>
            {
				if (pt == start) return true;
				if (pt == toPosition) return true;
				if (pt == shooter.Location.Position) return true;  // testing whether can fire from FromLocation, so not really here
				return !map.IsBlockingFire(pt);
            }, line);
    }

    public static bool CanTraceHypotheticalFireLine(in Location from, Location to, int maxRange, Actor shooter, List<Point> line=null)
    {
      Location? test = from.Map.Denormalize(in to);
      if (null == test) return false;
      return CanTraceHypotheticalFireLine(in from, test.Value.Position, maxRange, shooter, line);
    }

    public static bool CanTraceFireLine(in Location fromLocation, Point toPosition, int maxRange, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      return AngbandlikeTrace(maxRange, in start, in toPosition, pt => pt == start || pt == toPosition || !map.IsBlockingFire(pt), line);
    }

    public static bool CanTraceFireLine(in Location fromLocation, in Location toLocation, int maxRange, List<Point> line=null)
    {
      if (fromLocation.Map==toLocation.Map) return CanTraceFireLine(in fromLocation, toLocation.Position, maxRange, line);
      Location? tmp = fromLocation.Map.Denormalize(in toLocation);
      if (null == tmp) return false;
      return CanTraceFireLine(in fromLocation, tmp.Value.Position, maxRange, line);
    }

    public static bool CanTraceThrowLine(in Location fromLocation, in Point toPosition, int maxRange, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      return AngbandlikeTrace(maxRange, in start, in toPosition, pt => pt == start || !map.IsBlockingThrow(pt), line);
    }

    private static bool FOVSub(in Location fromLocation, Point toPosition, int maxRange, ref HashSet<Point> visibleSet)
    {
      Map map = fromLocation.Map;
      HashSet<Point> visibleSetRef = visibleSet;
      return AngbandlikeTrace(maxRange, fromLocation.Position, in toPosition, pt => {
                bool flag = pt== toPosition || map.IsTransparent(pt);
                if (flag) visibleSetRef.Add(pt);
                return flag;
      });
    }

    // To cache FOV centrally, we would have to be able to invalidate on change of mapobject position or transparency reliably
    // and also ditch the cache when it got "old"
    // note that actors only block their own hypothetical lines of fire, not hypothetical throwing lines or hypothetical FOV
    // the return of a cached value is assumed to be by value
#nullable enable
    private static Point VisibilityCheck(Point origin, Point dest, out Direction? alt)
    {
      alt = null;
      int delta_x = dest.X - origin.X;
      int delta_y = dest.Y - origin.Y;
      Point ret = dest;
      ret.X += (short)(0>delta_x ? 1 : -1);
      ret.Y += (short)(0 >delta_y ? 1 : -1);
      int abs_delta_x = 0 < delta_x ? delta_x : -delta_x;
      int abs_delta_y = 0 < delta_y ? delta_y : -delta_y;
      if      (abs_delta_x < abs_delta_y) alt = 0 < delta_x ? Direction.W : Direction.E;
      else if (abs_delta_x > abs_delta_y) alt = 0 < delta_y ? Direction.N : Direction.S;
      return ret;
    }

    public static HashSet<Point> ComputeFOVFor(in Location a_loc, short maxRange)
    {
      if (!FOVcache.TryGetValue(a_loc.Map,out var cache)) {
        cache = new Zaimoni.Data.TimeCache<KeyValuePair<Point, int>, HashSet<Point>>();
        cache.Now(Session.Get.WorldTime.TurnCounter);
        FOVcache[a_loc.Map] = cache; // \todo? could use Add if using a lock
      }
      if (cache.TryGetValue(new KeyValuePair<Point,int>(a_loc.Position,maxRange),out HashSet<Point> visibleSet)) return new HashSet<Point>(visibleSet);
      visibleSet = new HashSet<Point>{ a_loc.Position };
      if (0 >= maxRange) return visibleSet;
      Map map = a_loc.Map;
      Point position = a_loc.Position;
      List<Point> pointList1 = new List<Point>();
      foreach(Point point1 in OptimalFOV(maxRange).Select(pt=>pt+position)) {
        if (visibleSet.Contains(point1)) continue;
        var tile_loc = map.GetTileModelLocation(point1);
        if (null == tile_loc.Key) continue;
        if (!LOS.FOVSub(in a_loc, point1, maxRange, ref visibleSet)) {
            bool flag = false;
            TileModel tileModel = tile_loc.Key;
            if (!tileModel.IsTransparent && !tileModel.IsWalkable) flag = true;
            else if (tile_loc.Value.HasMapObject) flag = true;
            if (flag) pointList1.Add(point1);
        } else visibleSet.Add(point1);
      }

      // Postprocess map objects and tiles whose edges would reasonably be seen
      List<Point> pointList2 = new List<Point>(pointList1.Count);
      foreach (Point point2 in pointList1)
      { // if visibility is blocked for cardinal directions, post-processing merely makes what should be invisible, visible
        if (position.X == point2.X) continue;   // due N/S
        if (position.Y == point2.Y) continue;   // due E/W
        Point diag = VisibilityCheck(position, point2, out var lateral);
        if (null == lateral) continue;  // due NE/NW/SE/SW

        // tests for due NE/NW/SE/SW are more complex.  Unfortunately, the legacy postprocessing fails with barricaded glass doors
        // ..@.
        // #+++
        // S.Z.

        if (!visibleSet.Contains(diag) || !map.IsTransparent(diag)) continue;
        diag = point2 + lateral; // XXX abuse variable name
        if (!visibleSet.Contains(diag) || !map.IsTransparent(diag)) continue;
        pointList2.Add(point2);

#if OBSOLETE
          // unfortunately, a barricaded glass door is
          TileModel tileModel = map.GetTileModelAtExt(point3);
          if (tileModel.IsTransparent && tileModel.IsWalkable) ++num;
#endif
      }
      visibleSet.UnionWith(pointList2);
      FOVcache[a_loc.Map].Set(new KeyValuePair<Point,int>(a_loc.Position,maxRange),new HashSet<Point>(visibleSet));
      return visibleSet;
    }

    public static HashSet<Point> ComputeFOVFor(Location a_loc, int maxRange)
    {
      return ComputeFOVFor(in a_loc, (short)maxRange);
    }

    public static HashSet<Point> ComputeFOVFor(Actor actor, in Location a_loc)
    {
      return ComputeFOVFor(in a_loc, actor.FOVrange(actor.Location.Map.LocalTime, Session.Get.World.Weather));
    }

    public static HashSet<Point> ComputeFOVFor(Actor actor)
    {
	  return ComputeFOVFor(actor, actor.Location);
    }
#nullable restore
  }
}
