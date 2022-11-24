// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Direction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
#if PROTOTYPE
using System.Collections.Generic;
#endif
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D<short>;
using Size = Zaimoni.Data.Vector2D<short>;   // likely to go obsolete with transition to a true vector type
using PointF = Zaimoni.Data.Vector2D<float>;

// XXX C# Point is not a point in a vector space at all.
// C# Size  is closer (closed under + but doesn't honor left/right multiplication by a scalar)

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal sealed class Direction
  {
    public static readonly Direction NEUTRAL = new Direction(-1, "neutral", new Point(0, 0));
    public static readonly Direction N = new Direction(0, "N", new Point(0, -1));
    public static readonly Direction NE = new Direction(1, "NE", new Point(1, -1));
    public static readonly Direction E = new Direction(2, "E", new Point(1, 0));
    public static readonly Direction SE = new Direction(3, "SE", new Point(1, 1));
    public static readonly Direction S = new Direction(4, "S", new Point(0, 1));
    public static readonly Direction SW = new Direction(5, "SW", new Point(-1, 1));
    public static readonly Direction W = new Direction(6, "W", new Point(-1, 0));
    public static readonly Direction NW = new Direction(7, "NW", new Point(-1, -1));
    public static readonly Direction[] COMPASS = new Direction[8] { N, NE, E, SE, S, SW, W, NW };
    public static readonly Direction[] COMPASS_4 = new Direction[4] { N, E, S, W };
    public readonly sbyte Index;
    private readonly string m_Name;
    public readonly Point Vector;
    private readonly PointF NormalizedVector;

    private Direction(int index, string name, Point vector)
    {
      Index = (sbyte)index;
      m_Name = name;
      Vector = vector;
      NormalizedVector = new(vector.X, vector.Y);
      NormalizedVector.Normalize();
    }

    public static Point operator +(Point lhs, Direction rhs) { return lhs + rhs.Vector; }
    public static Point operator -(Point lhs, Direction rhs) { return lhs - rhs.Vector; }
    public static Direction operator -(Direction rhs) { return COMPASS[(rhs.Index + 4) % 8]; }
    public static Size operator *(short lhs, Direction rhs) { return lhs * rhs.Vector; }

    public static Direction FromVector(Point v)
    {
      foreach (Direction direction in COMPASS) if (direction.Vector == v) return direction;
      return null;
    }

    public static Direction To(int xFrom, int yFrom, int xTo, int yTo)
    {
        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xDeltaSgn = (0 == xDelta ? 0 : (0 < xDelta ? 1 : -1));
        int yDeltaSgn = (0 == yDelta ? 0 : (0 < yDelta ? 1 : -1));
        int dirCode = 3 * xDeltaSgn + yDeltaSgn;
        switch (dirCode) {
        case -3: return W;
        case -1: return N;
        case 0: return NEUTRAL;
        case 1: return S;
        case 3: return E;
        }
        int xAbsDelta = (1 == xDeltaSgn ? xDelta : -xDelta);
        int yAbsDelta = (1 == yDeltaSgn ? yDelta : -yDelta);
        if (xAbsDelta == yAbsDelta) goto diagonalExit;
        bool xABSLTy = xAbsDelta < yAbsDelta;
        int scale2 = 2 * (xABSLTy ? xAbsDelta : yAbsDelta);
        int scale1 = (xABSLTy ? yAbsDelta : xAbsDelta);
        // the pathfinder would need to do more work here.
        if (scale2 < scale1) goto diagonalExit;
        if (xABSLTy)
            // y dominant: N/S
            switch (dirCode)
            {
            default: throw new ArgumentOutOfRangeException(nameof(dirCode), dirCode, "(N/S) legal range -4..4");
            case -4: return N;
            case -2: return S;
            case 2: return N;
            case 4: return S;
            }
        // x dominant: E/W
        switch (dirCode)
        {
        default: throw new ArgumentOutOfRangeException(nameof(dirCode), dirCode, "(E/W) legal range -4..4");
        case -4: return W;
        case -2: return W;
        case 2: return E;
        case 4: return E;
        }
diagonalExit:
        switch (dirCode)
        {
        default: throw new ArgumentOutOfRangeException(nameof(dirCode), dirCode, "(diagonal) legal range -4..4");
        case -4: return NW;
        case -2: return SW;
        case 2: return NE;
        case 4: return SE;
        }
    }

#if DEAD_FUNC
    // this version reports on the chess knight move issue
    public static Direction To(int xFrom, int yFrom, int xTo, int yTo, out Direction alt)
    {
        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xDeltaSgn = (0 == xDelta ? 0 : (0 < xDelta ? 1 : -1));
        int yDeltaSgn = (0 == yDelta ? 0 : (0 < yDelta ? 1 : -1));
        int dirCode = 3 * xDeltaSgn + yDeltaSgn;
        alt = null;
        switch (dirCode)
        {
        case -3: return Direction.W;
        case -1: return Direction.N;
        case 0: return Direction.NEUTRAL;
        case 1: return Direction.S;
        case 3: return Direction.E;
        }
        int xAbsDelta = (1 == xDeltaSgn ? xDelta : -xDelta);
        int yAbsDelta = (1 == yDeltaSgn ? yDelta : -yDelta);
        if (xAbsDelta == yAbsDelta) goto diagonalExit;
        bool xABSLTy = xAbsDelta < yAbsDelta;
        int scale2 = 2 * (xABSLTy ? xAbsDelta : yAbsDelta);
        int scale1 = (xABSLTy ? yAbsDelta : xAbsDelta);
        // the pathfinder would need to do more work here.
        if (scale2 < scale1) goto diagonalExit;
        if (scale2 == scale1)
            // Chess knight move: +/- 1, +/-2 or vice versa.
            switch (dirCode)
            {
            default: throw new ArgumentOutOfRangeException("dirCode (knight); legal range -4..4", dirCode.ToString());
            case -4: alt = Direction.NW; break;
            case -2: alt = Direction.SW; break;
            case 2: alt = Direction.NE; break;
            case 4: alt = Direction.SE; break;
            };
        if (xABSLTy)
           // y dominant: N/S
           switch (dirCode)
           {
           default: throw new ArgumentOutOfRangeException("dirCode (N/S); legal range -4..4", dirCode.ToString());
           case -4: return Direction.N;
           case -2: return Direction.S;
           case 2: return Direction.N;
           case 4: return Direction.S;
           };
        // x dominant: E/W
        switch (dirCode)
        {
        default: throw new ArgumentOutOfRangeException("dirCode (E/W); legal range -4..4", dirCode.ToString());
        case -4: return Direction.W;
        case -2: return Direction.W;
        case 2: return Direction.E;
        case 4: return Direction.E;
        };
diagonalExit:
        switch (dirCode)
        {
        default: throw new ArgumentOutOfRangeException("dirCode (diagonal); legal range -4..4", dirCode.ToString());
        case -4: return NW;
        case -2: return SW;
        case 2: return NE;
        case 4: return SE;
        }
    }
#endif

    public static Direction ApproximateFromVector(Point v)
    {
      PointF pointF = new(v.X, v.Y);
      pointF.Normalize();
      if (PointF.Empty == pointF) return N;
      Direction dir = COMPASS.Minimize(d=> Math.Abs(pointF.X - d.NormalizedVector.X) + Math.Abs(pointF.Y - d.NormalizedVector.Y));
      return dir ?? N;
    }

    public Direction Left { get { return COMPASS[(Index + 7) % 8]; } }
    public Direction Right { get { return COMPASS[(Index + 1) % 8]; } }

    public override string ToString() { return m_Name; }
  }

  internal static class Direction_ext {
    private static readonly TimeCache<Point,Point[]> _adjacent = new TimeCache<Point, Point[]>();

    public static void Now() {  // generally not called in an actively multi-threaded context
      var t0 = Engine.Session.Get.WorldTime.TurnCounter;
      _adjacent.Now(t0);
      _adjacent.Expire(t0 - 2);
    }

    public static Point[] Adjacent(in this Point pt) {
      lock (_adjacent) {
        if (_adjacent.TryGetValue(pt, out Point[] value)) return value;
        Point[] ret = new Point[Direction.COMPASS.Length];
        foreach(var dir in Direction.COMPASS) ret[dir.Index] = pt+dir;
        _adjacent.Set(pt, ret);
        return ret;
      }
    }

//  public static Location[] Adjacent(in this Location pt) {...}    // massive slowdown in testing
  }
}
