// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.LOS
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define ANGBAND

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace djack.RogueSurvivor.Engine
{
  internal static class LOS
  {
    // FOV cache subsystem -- this is to enable removing FOV data from the savefile.
    // 0.9.9 unstable 2017-02-17 had a measured load time of over 3 minutes 30 seconds at turn 0, and one minute 45 seconds at turn 90.
    // at that version, the game seems to run perfectly fast once loaded (on the development machine) so trading speed of the game for speed of loading
    // makes sense.
    private static readonly Dictionary<Map,Zaimoni.Data.TimeCache<KeyValuePair<Point,int>,HashSet<Point>>> FOVcache = new Dictionary<Map,Zaimoni.Data.TimeCache<KeyValuePair<Point,int>,HashSet<Point>>>();

    public static void Expire(Map m) { if (FOVcache.TryGetValue(m,out var target) && target.Expire(m.LocalTime.TurnCounter-2)) FOVcache.Remove(m); }
    public static void Now(Map map) {
      if (!FOVcache.ContainsKey(map)) FOVcache[map] = new Zaimoni.Data.TimeCache<KeyValuePair<Point,int>,HashSet<Point>>();
      FOVcache[map].Now(map.LocalTime.TurnCounter);
    }

    public static void Validate(Map map, Predicate<HashSet<Point>> fn) {
      if (FOVcache.TryGetValue(map,out var target)) target.Validate(fn);
    }

    // Optimal FOV offset subsystem to deal with some ugly inverse problems
    // this is symmetric, unlike actual FOV calculations at range 5+ (range 4- is symmetric by construction)
    private static readonly Dictionary<int,System.Collections.ObjectModel.ReadOnlyCollection<Point>> OptimalFOVOffsets = new Dictionary<int,System.Collections.ObjectModel.ReadOnlyCollection<Point>>();
    public static System.Collections.ObjectModel.ReadOnlyCollection<Point> OptimalFOV(int range)
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
          if (Rules.StdDistance(origin, pt) > edge_of_maxrange) continue;
          // initialize all octants at once
          tmp.Add(pt);
          if (pt.X == pt.Y) { // diagonal
            tmp.Add(new Point(pt.X,-pt.Y));
            tmp.Add(new Point(-pt.X,pt.Y));
            tmp.Add(new Point(-pt.X,-pt.Y));
          } else if (0 == pt.Y) {   // cardinal
            tmp.Add(new Point(-pt.X,0));
            tmp.Add(new Point(0,pt.X));
            tmp.Add(new Point(0,-pt.X));
          } else { // typical
            tmp.Add(new Point(pt.X,-pt.Y));
            tmp.Add(new Point(-pt.X,pt.Y));
            tmp.Add(new Point(-pt.X,-pt.Y));
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

#if ANGBAND
#else
    private static bool AsymetricBresenhamTrace(int maxSteps, Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
    {
      int num1 = Math.Abs(xTo - xFrom) << 1;
      int num2 = Math.Abs(yTo - yFrom) << 1;
      int num3 = xTo > xFrom ? 1 : -1;
      int num4 = yTo > yFrom ? 1 : -1;
      if (line != null)
        line.Add(new Point(xFrom, yFrom));
      int num5 = 0;
      if (num1 >= num2)
      {
        int num6 = num2 - (num1 >> 1);
        while (xFrom != xTo)
        {
          if (num6 >= 0 && (num6 != 0 || num3 > 0))
          {
            yFrom += num4;
            num6 -= num1;
          }
          xFrom += num3;
          num6 += num2;
          if (++num5 > maxSteps || !fn(xFrom, yFrom))
            return false;
          if (line != null)
            line.Add(new Point(xFrom, yFrom));
        }
      }
      else
      {
        int num6 = num1 - (num2 >> 1);
        while (yFrom != yTo)
        {
          if (num6 >= 0 && (num6 != 0 || num4 > 0))
          {
            xFrom += num3;
            num6 -= num2;
          }
          yFrom += num4;
          num6 += num1;
          if (++num5 > maxSteps || !fn(xFrom, yFrom))
            return false;
          if (line != null)
            line.Add(new Point(xFrom, yFrom));
        }
      }
      return true;
    }

    private static bool AsymetricBresenhamTrace(Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
    {
      return LOS.AsymetricBresenhamTrace(int.MaxValue, map, xFrom, yFrom, xTo, yTo, line, fn);
    }
#endif

#if ANGBAND
    private static bool AngbandlikeTrace(int maxSteps, int xFrom, int yFrom, int xTo, int yTo, Func<int, int, bool> fn, List<Point> line = null)
    {
#if DEBUG
        if (null == fn) throw new ArgumentNullException(nameof(fn));
        if (0 > maxSteps) throw new ArgumentOutOfRangeException("0 < maxSteps", maxSteps.ToString());
#endif
        Point start = new Point(xFrom, yFrom);
        line?.Add(start);
        if (0 == maxSteps) return true;

        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xAbsDelta = (0 <= xDelta ? xDelta : -xDelta);
        int yAbsDelta = (0 <= yDelta ? yDelta : -yDelta);
        int needRange = (xAbsDelta < yAbsDelta ? yAbsDelta : xAbsDelta);
        int actualRange = (needRange < maxSteps ? needRange : maxSteps);

        Direction tmp = Direction.To(xFrom,yFrom,xTo,yTo,out Direction knightmove);
        Point end = start + needRange * tmp;
        Direction offset = Direction.To(end.X, end.Y, xTo, yTo);
        int i = 0;
        if (offset == Direction.NEUTRAL)
            {  // cardinal direction
            do  {
                start += tmp;
                if (!fn(start.X, start.Y)) return false;
                line?.Add(new Point(start.X, start.Y));
                }
            while (++i < actualRange);
            return start.X == xTo && start.Y == yTo;
            }
        Direction alt_step = Direction.FromVector(new Point(tmp.Vector.X + offset.Vector.X, tmp.Vector.Y + offset.Vector.Y));
        Point err = new Point(xTo - end.X, yTo - end.Y);
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
                if (!fn(start.X, start.Y)) return false;
                line?.Add(start);
                continue;
                }
            else if (numerator < needRange)
                {
                start += tmp;
                if (!fn(start.X, start.Y)) return false;
                line?.Add(start);
                continue;
                };
            if (0==knightmove_parity)
                {   // chess knight's move paradox: for distance 2, we have +/1 +/2
                Point test = start+tmp;
                if (!fn(test.X, test.Y)) {
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
                if (!fn(test.X, test.Y)) knightmove_parity = 1;
                }
            if (0==knightmove_parity && null!=line) knight_moves.Add(line.Count);
            if (-1==knightmove_parity)
                {
                start += alt_step;
                numerator -= 2 * needRange;
                if (!fn(start.X, start.Y)) return false;
                line?.Add(start);
                continue;
                }
//          knightmove_parity = 1;  // do not *commit* to knight move parity here (unnecessary asymmetry, interferes with cover/stealth mechanics), 0 should mean both options are legal
            start += tmp;
            if (!fn(start.X, start.Y)) return false;
            line?.Add(start);
            }
        while (++i < actualRange);
        return start.X == xTo && start.Y == yTo;
    }
#endif

    public static bool CanTraceViewLine(Location fromLocation, Point toPosition, int maxRange = int.MaxValue, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point goal = toPosition;
#if ANGBAND
      return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) => map.IsTransparent(x, y) || x == goal.X && y == goal.Y),line);
#else
      return LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (List<Point>)null, (Func<int, int, bool>)((x, y) => map.IsTransparent(x, y) || x == goal.X && y == goal.Y));
#endif
    }

    public static bool CanTraceViewLine(Location from, Location to, int maxRange, List<Point> line = null)
    {
      if (from.Map == to.Map) return CanTraceViewLine(from, to.Position, maxRange);
      Location? test = from.Map.Denormalize(to);
      if (null == test) return false;
      return CanTraceViewLine(from, test.Value.Position, maxRange, line);
    }

    public static bool CanTraceHypotheticalFireLine(Location fromLocation, Point toPosition, int maxRange, Actor shooter, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      Point goal = toPosition;
            return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
				if (x == start.X && y == start.Y) return true;
				if (x == goal.X && y == goal.Y) return true;
				if (x == shooter.Location.Position.X && y == shooter.Location.Position.Y) return true;  // testing whether can fire from FromLocation, so not really here
				return !map.IsBlockingFire(x, y);
            }), line);
    }

    public static bool CanTraceHypotheticalFireLine(Location from, Location to, int maxRange, Actor shooter, List<Point> line=null)
    {
      Location? test = from.Map.Denormalize(to);
      if (null == test) return false;
      return CanTraceHypotheticalFireLine(from, test.Value.Position, maxRange, shooter, line);
    }

    public static bool CanTraceFireLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      Point goal = toPosition;
#if ANGBAND
      return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingFire(x, y))
                    return true;
                return false;
            }), line);
#else
      bool fireLineClear = true;
            LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, line, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingFire(x, y))
                    return true;
                fireLineClear = false;
                return true;
            }));
      return fireLineClear;
#endif
    }

    public static bool CanTraceFireLine(Location fromLocation, Location toLocation, int maxRange, List<Point> line=null)
    {
      if (fromLocation.Map==toLocation.Map) return CanTraceFireLine(fromLocation, toLocation.Position, maxRange, line);
      Location? tmp = fromLocation.Map.Denormalize(toLocation);
      if (null == tmp) return false;
      return CanTraceFireLine(fromLocation, tmp.Value.Position, maxRange, line);
    }

    public static bool CanTraceThrowLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line=null)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
#if ANGBAND
      return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || !map.IsBlockingThrow(x, y))
                    return true;
                return false;
            }), line);
#else
      Point goal = toPosition;
      bool throwLineClear = true;
                  LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, line, (Func<int, int, bool>) ((x, y) =>
                  {
                    if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingThrow(x, y))
                      return true;
                    throwLineClear = false;
                    return true;
                  }));
      if (map.IsBlockingThrow(toPosition.X, toPosition.Y))
        throwLineClear = false;
      return throwLineClear;
#endif
        }

        private static bool FOVSub(Location fromLocation, Point toPosition, int maxRange, ref HashSet<Point> visibleSet)
    {
      Map map = fromLocation.Map;
      HashSet<Point> visibleSetRef = visibleSet;
      Point goal = toPosition;
#if ANGBAND
            return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
                bool flag = x == goal.X && y == goal.Y || map.IsTransparent(x, y);
                if (flag)
                    visibleSetRef.Add(new Point(x, y));
                return flag;
            }));
#else
                  return LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (List<Point>) null, (Func<int, int, bool>) ((x, y) =>
                  {
                    bool flag = x == goal.X && y == goal.Y || map.IsTransparent(x, y);
                    if (flag)
                      visibleSetRef.Add(new Point(x, y));
                    return flag;
                  }));
#endif
        }

    // To cache FOV centrally, we would have to be able to invalidate on change of mapobject position or transparency reliably
    // and also ditch the cache when it got "old"
    // note that actors only block their own hypothetical lines of fire, not hypothetical throwing lines or hypothetical FOV
    // the return of a cached value is assumed to be by value
    public static HashSet<Point> ComputeFOVFor(Location a_loc, int maxRange)
    {
      if (FOVcache[a_loc.Map].TryGetValue(new KeyValuePair<Point,int>(a_loc.Position,maxRange),out HashSet<Point> visibleSet)) return new HashSet<Point>(visibleSet);
      visibleSet = new HashSet<Point>{ a_loc.Position };
      if (0 >= maxRange) return visibleSet;
      Map map = a_loc.Map;
      Point position = a_loc.Position;
      List<Point> pointList1 = new List<Point>();
      foreach(Point point1 in OptimalFOV(maxRange).Select(pt=>new Point(pt.X+a_loc.Position.X,pt.Y+a_loc.Position.Y))) {
        if (!a_loc.Map.IsValid(point1)) continue;
          if (visibleSet.Contains(point1)) continue;
          if (!LOS.FOVSub(a_loc, point1, maxRange, ref visibleSet)) {
            bool flag = false;
            TileModel tileModel = map.GetTileModelAtExt(point1);
            if (!tileModel.IsTransparent && !tileModel.IsWalkable) flag = true;
            else if (map.HasMapObjectAtExt(point1)) flag = true;
            if (flag) pointList1.Add(point1);
          } else visibleSet.Add(point1);
      }

      // Postprocess map objects and tiles whose edges would reasonably be seen
      List<Point> pointList2 = new List<Point>(pointList1.Count);
      foreach (Point point2 in pointList1)
      { // if visibility is blocked for cardinal directions, post-processing merely makes what should be invisible, visible
        if (position.X == point2.X) continue;   // due N/S
        if (position.Y == point2.Y) continue;   // due E/W
        // tests for due NE/NW/SE/SW are more complex.

        int num = 0;
        foreach (Point point3 in Direction.COMPASS.Select(dir=> point2 + dir)) {
          if (!visibleSet.Contains(point3)) continue;
          TileModel tileModel = map.GetTileModelAtExt(point3);
          if (tileModel.IsTransparent && tileModel.IsWalkable) ++num;
        }
        if (num >= 3) pointList2.Add(point2);
      }
      visibleSet.UnionWith(pointList2);
      FOVcache[a_loc.Map].Set(new KeyValuePair<Point,int>(a_loc.Position,maxRange),new HashSet<Point>(visibleSet));
      return visibleSet;
    }

    public static HashSet<Point> ComputeFOVFor(Actor actor, Location a_loc)
    {
      return ComputeFOVFor(a_loc, actor.FOVrange(actor.Location.Map.LocalTime, Session.Get.World.Weather));
    }

    public static HashSet<Point> ComputeFOVFor(Actor actor)
    {
	  return ComputeFOVFor(actor, actor.Location);
    }
  }
}
