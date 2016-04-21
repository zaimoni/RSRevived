// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Direction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Drawing;

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
    public static readonly Direction[] COMPASS = new Direction[8]
    {
      Direction.N,
      Direction.NE,
      Direction.E,
      Direction.SE,
      Direction.S,
      Direction.SW,
      Direction.W,
      Direction.NW
    };
    public static readonly List<Direction> COMPASS_LIST = new List<Direction>()
    {
      Direction.N,
      Direction.NE,
      Direction.E,
      Direction.SE,
      Direction.S,
      Direction.SW,
      Direction.W,
      Direction.NW
    };
    public static readonly Direction[] COMPASS_4 = new Direction[4]
    {
      Direction.N,
      Direction.E,
      Direction.S,
      Direction.W
    };
    private int m_Index;
    private string m_Name;
    private Point m_Vector;
    private PointF m_NormalizedVector;

    public int Index
    {
      get
      {
        return this.m_Index;
      }
    }

    public string Name
    {
      get
      {
        return this.m_Name;
      }
    }

    public Point Vector
    {
      get
      {
        return this.m_Vector;
      }
    }

    public PointF NormalizedVector
    {
      get
      {
        return this.m_NormalizedVector;
      }
    }

    private Direction(int index, string name, Point vector)
    {
      this.m_Index = index;
      this.m_Name = name;
      this.m_Vector = vector;
      float num = (float) Math.Sqrt((double) (vector.X * vector.X + vector.Y * vector.Y));
      if ((double) num != 0.0)
        this.m_NormalizedVector = new PointF((float) vector.X / num, (float) vector.Y / num);
      else
        this.m_NormalizedVector = PointF.Empty;
    }

    public static Point operator +(Point lhs, Direction rhs)
    {
      return new Point(lhs.X + rhs.Vector.X, lhs.Y + rhs.Vector.Y);
    }

    public static Point operator *(int lhs, Direction rhs)
    {
        return new Point(lhs * rhs.Vector.X, lhs * rhs.Vector.Y);
    }

    public static Direction FromVector(Point v)
    {
      foreach (Direction direction in Direction.COMPASS)
      {
        if (direction.Vector == v)
          return direction;
      }
      return (Direction) null;
    }

    public static Direction FromVector(int vx, int vy)
    {
      foreach (Direction direction in Direction.COMPASS)
      {
        if (direction.Vector.X == vx & direction.Vector.Y == vy)
          return direction;
      }
      return (Direction) null;
    }

    public static Direction To(int xFrom, int yFrom, int xTo, int yTo)
    {
        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xDeltaSgn = (0 == xDelta ? 0 : (0 < xDelta ? 1 : -1));
        int yDeltaSgn = (0 == xDelta ? 0 : (0 < xDelta ? 1 : -1));
        int dirCode = 3 * xDeltaSgn + yDeltaSgn;
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
        case -4: return Direction.NW;
        case -2: return Direction.SW;
        case 2: return Direction.NE;
        case 4: return Direction.SE;
        }
    }

    // this version reports on the chess knight move issue
    public static Direction To(int xFrom, int yFrom, int xTo, int yTo, out Direction alt)
    {
        int xDelta = xTo - xFrom;
        int yDelta = yTo - yFrom;
        int xDeltaSgn = (0 == xDelta ? 0 : (0 < xDelta ? 1 : -1));
        int yDeltaSgn = (0 == xDelta ? 0 : (0 < xDelta ? 1 : -1));
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
        case -4: return Direction.NW;
        case -2: return Direction.SW;
        case 2: return Direction.NE;
        case 4: return Direction.SE;
        }
    }

    public static Direction ApproximateFromVector(Point v)
    {
      PointF pointF = (PointF) v;
      float num1 = (float) Math.Sqrt((double) pointF.X * (double) pointF.X + (double) pointF.Y * (double) pointF.Y);
      if ((double) num1 == 0.0)
        return Direction.N;
      pointF.X /= num1;
      pointF.Y /= num1;
      float num2 = float.MaxValue;
      Direction direction1 = Direction.N;
      foreach (Direction direction2 in Direction.COMPASS)
      {
        float num3 = Math.Abs(pointF.X - direction2.NormalizedVector.X) + Math.Abs(pointF.Y - direction2.NormalizedVector.Y);
        if ((double) num3 < (double) num2)
        {
          direction1 = direction2;
          num2 = num3;
        }
      }
      return direction1;
    }

    public static Direction Right(Direction d)
    {
      return Direction.COMPASS[(d.m_Index + 1) % 8];
    }

    public static Direction Left(Direction d)
    {
      return Direction.COMPASS[(d.m_Index - 1) % 8];
    }

    public static Direction Opposite(Direction d)
    {
      return Direction.COMPASS[(d.m_Index + 4) % 8];
    }

    public override string ToString()
    {
      return this.m_Name;
    }
  }
}
