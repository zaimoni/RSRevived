// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.LOS
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

//#define ANGBAND

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Engine
{
  internal static class LOS
  {
    public static bool AsymetricBresenhamTrace(int maxSteps, Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
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

    public static bool AsymetricBresenhamTrace(Map map, int xFrom, int yFrom, int xTo, int yTo, List<Point> line, Func<int, int, bool> fn)
    {
      return LOS.AsymetricBresenhamTrace(int.MaxValue, map, xFrom, yFrom, xTo, yTo, line, fn);
    }

    public static bool AngbandlikeTrace(int maxSteps, int xFrom, int yFrom, int xTo, int yTo, Func<int, int, bool> fn, List<Point> line = null)
    {
#if DEBUG
        if (0 >= maxSteps) throw new ArgumentOutOfRangeException("0 < maxSteps", maxSteps.ToString());
#endif
        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xAbsDelta = (0 <= xDelta ? xDelta : -xDelta);
        int yAbsDelta = (0 <= yDelta ? yDelta : -yDelta);
        int needRange = (xAbsDelta < yAbsDelta ? yAbsDelta : xAbsDelta);
        int actualRange = (needRange < maxSteps ? needRange : maxSteps);

        int i = 0;
        Direction knightmove;
        List<Point> line_1 = (null == line ? null : new List<Point>(actualRange + 1));
        Point point_1 = new Point(xFrom, yFrom);
        line?.Add(new Point(point_1.X, point_1.Y));
        Direction tmp = Direction.To(xFrom, yFrom, xTo, yTo, out knightmove);
        if (null != knightmove)
            {  // two possible paths: slope is +/- 1/2 or +/- 2
#if DEBUG
            if (0 != needRange % 2) throw new ArgumentOutOfRangeException("knight move: 0 == needRange%2", maxSteps.ToString());
#endif
            line_1?.Add(new Point(point_1.X, point_1.Y));
            List<Point> line_2 = (null == line ? null : new List<Point>(actualRange + 1));
            Point point_2 = new Point(xFrom, yFrom);
            line_2?.Add(new Point(point_2.X, point_2.Y));
            // the first line is biased towards the primary direction.
            // the second line is biased towards the diagonal direction
            bool ok_1 = true;
            bool ok_2 = true;
            int line_1CMPline_2 = 0;
            do  {
                point_1 += tmp;
                point_2 += knightmove;
                if (ok_1 && !fn(point_1.X, point_1.Y)) ok_1 = false;
                if (ok_2 && !fn(point_2.X, point_2.Y)) ok_2 = false;
                if (!ok_1 && ok_2) line_1CMPline_2 = -1;
                if (ok_1 && !ok_2) line_1CMPline_2 = 1;
                if (!ok_1 && !ok_2)
                    {
                    line = (0 <= line_1CMPline_2 ? line_1 : line_2);
                    return false;
                    }
                if (ok_1) line_1?.Add(new Point(point_1.X, point_1.Y));
                if (ok_2) line_2?.Add(new Point(point_2.X, point_2.Y));
                if (++i >= actualRange) break;
                point_1 += knightmove;
                point_2 += tmp;
                if (ok_1 && !fn(point_1.X, point_1.Y)) ok_1 = false;
                if (ok_2 && !fn(point_2.X, point_2.Y)) ok_2 = false;
                if (!ok_1 && ok_2) line_1CMPline_2 = -1;
                if (ok_1 && !ok_2) line_1CMPline_2 = 1;
                if (!ok_1 && !ok_2)
                    {
                    line = (0 <= line_1CMPline_2 ? line_1 : line_2);
                    return false;
                    }
                if (ok_1) line_1?.Add(new Point(point_1.X, point_1.Y));
                if (ok_2) line_2?.Add(new Point(point_2.X, point_2.Y));
                }
            while (++i < actualRange);
            if (ok_1)
                {
                line = line_1;
                return point_1.X == xTo && point_1.Y == yTo;
                };
            if (ok_2)
                {
                line = line_2;
                return point_2.X == xTo && point_2.Y == yTo;
                };
            line = (0 <= line_1CMPline_2 ? line_1 : line_2);
            return false;
            }

        // only one path
        Point guess = needRange * tmp;
        guess.X += xFrom;
        guess.Y += yFrom;
        Direction offset = Direction.To(guess.X, guess.Y, xTo, yTo);
        if (offset == Direction.NEUTRAL)
            {  // cardinal direction
            do
                {
                point_1 = point_1+tmp;
                if (!fn(point_1.X, point_1.Y)) return false;
                line?.Add(new Point(point_1.X, point_1.Y));
                }
            while (++i < actualRange);
            return point_1.X == xTo && point_1.Y == yTo;
            }

        int err_x = xTo - guess.X;
        int err_y = yTo - guess.Y;
        int absErr_x = (0 <= err_x ? err_x : -err_x);
        int absErr_y = (0 <= err_y ? err_y : -err_y);
        int offBy = (absErr_x < absErr_y ? absErr_y : absErr_x);
        int numerator = 0;  // denominator is needRange;
        // react to nearly diagonal
        if (absErr_x<absErr_y)
            {
            if (2 * absErr_x > absErr_y)
                {
                absErr_x = absErr_y - absErr_x;
                offBy = absErr_x;
                numerator = -offBy-1;
                }
            }
        else{
            if (2 * absErr_y > absErr_x)
                {
                absErr_y = absErr_x - absErr_y;
                offBy = absErr_y;
                numerator = -offBy-1;
                }
            }

        do {
                numerator += offBy+1;
                point_1 += tmp;
                if (numerator>needRange && (point_1.X!=xTo || point_1.Y!=yTo))
                    {
                    point_1 += offset;
                    numerator -= needRange;
                    }
                if (!fn(point_1.X, point_1.Y)) return false;
                line?.Add(new Point(point_1.X, point_1.Y));
            }
        while (++i < actualRange);
        return point_1.X == xTo && point_1.Y == yTo;
    }

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
      bool fireLineClear = true;
#if ANGBAND
            LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingFire(x, y))
                    return true;
                fireLineClear = false;
                return true;
            }), line);
#else
            LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, line, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingFire(x, y))
                    return true;
                fireLineClear = false;
                return true;
            }));
#endif
            return fireLineClear;
    }

    public static bool CanTraceThrowLine(Location fromLocation, Point toPosition, int maxRange, List<Point> line)
    {
      Map map = fromLocation.Map;
      Point start = fromLocation.Position;
      Point goal = toPosition;
      bool throwLineClear = true;
#if ANGBAND
            LOS.AngbandlikeTrace(maxRange, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, (Func<int, int, bool>)((x, y) =>
            {
                if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingThrow(x, y))
                    return true;
                throwLineClear = false;
                return true;
            }), line);
#else
                  LOS.AsymetricBresenhamTrace(maxRange, map, fromLocation.Position.X, fromLocation.Position.Y, toPosition.X, toPosition.Y, line, (Func<int, int, bool>) ((x, y) =>
                  {
                    if (x == start.X && y == start.Y || x == goal.X && y == goal.Y || !map.IsBlockingThrow(x, y))
                      return true;
                    throwLineClear = false;
                    return true;
                  }));
#endif
      if (map.IsBlockingThrow(toPosition.X, toPosition.Y))
        throwLineClear = false;
      return throwLineClear;
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
          if ((double) rules.LOSDistance(position, point1) <= (double) maxRange && !visibleSet.Contains(point1))
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
#if ANGBAND
#else
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
#endif
      return visibleSet;
    }
  }
}
