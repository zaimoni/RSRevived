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

namespace djack.RogueSurvivor.Engine
{
  internal static class LOS
  {
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
        if (0 > maxSteps) throw new ArgumentOutOfRangeException("0 < maxSteps", maxSteps.ToString());
#endif
        Point start = new Point(xFrom, yFrom);
        line?.Add(new Point(start.X, start.Y));
        if (0 == maxSteps) return true;

        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xAbsDelta = (0 <= xDelta ? xDelta : -xDelta);
        int yAbsDelta = (0 <= yDelta ? yDelta : -yDelta);
        int needRange = (xAbsDelta < yAbsDelta ? yAbsDelta : xAbsDelta);
        int actualRange = (needRange < maxSteps ? needRange : maxSteps);
        int minAbsDelta = (xAbsDelta < yAbsDelta) ? xAbsDelta : yAbsDelta;
        int maxAbsDelta = (xAbsDelta < yAbsDelta) ? yAbsDelta : xAbsDelta;

        Direction knightmove;
        Direction tmp = Direction.To(xFrom,yFrom,xTo,yTo,out knightmove);
        Point end = needRange * tmp;    // estimate here
        end.X += xFrom;
        end.Y += yFrom;
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
        do  {
            numerator += 2*alt_count;
            if (numerator>needRange)
                {
                start += alt_step;
                numerator -= 2*needRange;
                if (!fn(start.X, start.Y)) return false;
                line?.Add(new Point(start.X, start.Y));
                continue;
                }
            else if (numerator < needRange)
                {
                start += tmp;
                if (!fn(start.X, start.Y)) return false;
                line?.Add(new Point(start.X, start.Y));
                continue;
                };
            if (0==knightmove_parity)
                {   // chess knight's move paradox: for distance 2, we have +/1 +/2
                start += tmp;
                if (!fn(start.X, start.Y)) knightmove_parity = -1;
                start -= tmp;
                }
            if (0==knightmove_parity)
                {   // chess knight's move paradox: for distance 2, we have +/1 +/2
                start += alt_step;
                if (!fn(start.X, start.Y)) knightmove_parity = 1;
                start -= alt_step;
                }
            if (-1==knightmove_parity)
                {
                start += alt_step;
                numerator -= 2 * needRange;
                if (!fn(start.X, start.Y)) return false;
                line?.Add(new Point(start.X, start.Y));
                continue;
                }
            knightmove_parity = 1;
            start += tmp;
            if (!fn(start.X, start.Y)) return false;
            line?.Add(new Point(start.X, start.Y));
            }
        while (++i < actualRange);
        return start.X == xTo && start.Y == yTo;
    }
#endif

    public static bool CanTraceViewLine(Location fromLocation, Point toPosition, int maxRange)
    {
      Map map = fromLocation.Map;
      Point goal = toPosition;
#if ANGBAND
      return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) => map.IsTransparent(x, y) || x == goal.X && y == goal.Y));
#else
      return LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (List<Point>)null, (Func<int, int, bool>)((x, y) => map.IsTransparent(x, y) || x == goal.X && y == goal.Y));
#endif
    }

    public static bool CanTraceViewLine(Location fromLocation, Point toPosition)
    {
      return LOS.CanTraceViewLine(fromLocation, toPosition, int.MaxValue);
    }

    public static bool CanTraceFireLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line)
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

        public static bool CanTraceThrowLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      Point goal = toPosition;
#if ANGBAND
      return LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || !map.IsBlockingThrow(x, y))
                    return true;
                return false;
            }), line);
#else
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

    public static HashSet<Point> ComputeFOVFor(Rules rules, Actor actor, WorldTime time, Weather weather)
    {
      Location location = actor.Location;
      HashSet<Point> visibleSet = new HashSet<Point>();
      Point position = location.Position;
      Map map = location.Map;
      int maxRange = rules.ActorFOV(actor, time, weather);
      int x1 = position.X - maxRange;
      int x2 = position.X + maxRange;
      int y1 = position.Y - maxRange;
      int y2 = position.Y + maxRange;
      map.TrimToBounds(ref x1, ref y1);
      map.TrimToBounds(ref x2, ref y2);
      Point point1 = new Point();
      List<Point> pointList1 = new List<Point>();
      for (int x3 = x1; x3 <= x2; ++x3)
      {
        point1.X = x3;
        for (int y3 = y1; y3 <= y2; ++y3)
        {
          point1.Y = y3;
          if ((double) Rules.LOSDistance(position, point1) <= (double) maxRange && !visibleSet.Contains(point1))
          {
            if (!LOS.FOVSub(location, point1, maxRange, ref visibleSet))
            {
              bool flag = false;
              Tile tileAt = map.GetTileAt(x3, y3);
              MapObject mapObjectAt = map.GetMapObjectAt(x3, y3);
              if (!tileAt.Model.IsTransparent && !tileAt.Model.IsWalkable)
                flag = true;
              else if (mapObjectAt != null)
                flag = true;
              if (flag)
                pointList1.Add(point1);
            }
            else
              visibleSet.Add(point1);
          }
        }
      }

      // Postprocess map objects and tiles whose edges would reasonably be seen
      List<Point> pointList2 = new List<Point>(pointList1.Count);
      foreach (Point point2 in pointList1)
      {
        int num = 0;
        foreach (Direction direction in Direction.COMPASS)
        {
          Point point3 = point2 + direction;
          if (visibleSet.Contains(point3))
          {
            Tile tileAt = map.GetTileAt(point3.X, point3.Y);
            if (tileAt.Model.IsTransparent && tileAt.Model.IsWalkable)
              ++num;
          }
        }
        if (num >= 3)
          pointList2.Add(point2);
      }
      foreach (Point point2 in pointList2)
        visibleSet.Add(point2);
    return visibleSet;
    }
  }
}
