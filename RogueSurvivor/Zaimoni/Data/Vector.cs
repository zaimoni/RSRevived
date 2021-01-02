﻿using System;
using System.Collections.Generic;

/*
 * We do not use C# generics here because they compile-error due to being defective relative to C++ templates.  We would need to
 * be able to whitelist according to arbitrary member functions/operators (C++ compile-errors when the required interface is missing,
 * but C# requires an explicit whitelisting at least through 8.0)
 */

namespace Zaimoni.Data
{
    // C naming conventions
    [Serializable]
    public struct Vector2D_int : IComparable<Vector2D_int>,IEquatable<Vector2D_int>
    {
        public int X;
        public int Y;

        public Vector2D_int(int x, int y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_int(int x) => new Vector2D_int(x,x);

        static readonly public Vector2D_int Empty = new Vector2D_int(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }
        static public readonly Vector2D_int MaxValue = new Vector2D_int(int.MaxValue, int.MaxValue);
        static public readonly Vector2D_int MinValue = new Vector2D_int(int.MinValue, int.MinValue);

        static public bool operator ==(Vector2D_int lhs, Vector2D_int rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_int lhs, Vector2D_int rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_int lhs, Vector2D_int rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_int lhs, Vector2D_int rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_int lhs, Vector2D_int rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_int lhs, Vector2D_int rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_int operator +(Vector2D_int lhs, Vector2D_int rhs) { return new Vector2D_int(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int operator -(Vector2D_int lhs, Vector2D_int rhs) { return new Vector2D_int(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_int operator -(Vector2D_int src) { return new Vector2D_int(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int operator *(int lhs, Vector2D_int rhs) { return new Vector2D_int(lhs*rhs.X, lhs*rhs.Y); }
        static public Vector2D_int operator *(Vector2D_int lhs, int rhs) { return new Vector2D_int(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int operator /(Vector2D_int lhs, int rhs) { return new Vector2D_int(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_int coord_xform(Func<int, int> op) { return new Vector2D_int(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_int other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_int other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D_int test && Equals(test);
        }

        public override int GetHashCode()
        {
            return unchecked(1 + 19 * X + 19 * 17 * Y);
        }
    };

    [Serializable]
    public readonly struct Vector2D_int_r : IComparable<Vector2D_int_r>, IEquatable<Vector2D_int_r>
    {
        public readonly int X;
        public readonly int Y;

        public Vector2D_int_r(int x, int y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_int_r(int x) => new Vector2D_int_r(x, x);

        static readonly public Vector2D_int_r Empty = new Vector2D_int_r(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }
        static public readonly Vector2D_int_r MaxValue = new Vector2D_int_r(int.MaxValue, int.MaxValue);
        static public readonly Vector2D_int_r MinValue = new Vector2D_int_r(int.MinValue, int.MinValue);

        static public bool operator ==(Vector2D_int_r lhs, Vector2D_int_r rhs) { return lhs.Equals(in rhs); }
        static public bool operator !=(Vector2D_int_r lhs, Vector2D_int_r rhs) { return !lhs.Equals(in rhs); }
        static public bool operator <(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 > lhs.CompareTo(in rhs); }
        static public bool operator >(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 < lhs.CompareTo(in rhs); }
        static public bool operator <=(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 >= lhs.CompareTo(in rhs); }
        static public bool operator >=(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 <= lhs.CompareTo(in rhs); }

        // vector arithmetic
        static public Vector2D_int_r operator +(in Vector2D_int_r lhs, in Vector2D_int_r rhs) { return new Vector2D_int_r(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int_r operator -(in Vector2D_int_r lhs, in Vector2D_int_r rhs) { return new Vector2D_int_r(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_int_r operator -(Vector2D_int_r src) { return new Vector2D_int_r(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int_r operator *(int lhs, in Vector2D_int_r rhs) { return new Vector2D_int_r(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_int_r operator *(in Vector2D_int_r lhs, int rhs) { return new Vector2D_int_r(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int_r operator /(in Vector2D_int_r lhs, int rhs) { return new Vector2D_int_r(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_int_r coord_xform(Func<int, int> op) { return new Vector2D_int_r(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_int_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        public int CompareTo(in Vector2D_int_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_int_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public bool Equals(in Vector2D_int_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D_int test && Equals(test);
        }

        public override int GetHashCode()
        {
            return unchecked(1 + 19 * X + 19 * 17 * Y);
        }
    };

    public ref struct Vector2D_int_stack
    {
        public int X;
        public int Y;

        public Vector2D_int_stack(int x, int y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_int_stack(int x) => new Vector2D_int_stack(x, x);

        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_int_stack operator +(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return new Vector2D_int_stack(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int_stack operator -(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return new Vector2D_int_stack(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_int_stack operator -(Vector2D_int_stack src) { return new Vector2D_int_stack(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int_stack operator *(int lhs, Vector2D_int_stack rhs) { return new Vector2D_int_stack(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_int_stack operator *(Vector2D_int_stack lhs, int rhs) { return new Vector2D_int_stack(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int_stack operator /(Vector2D_int_stack lhs, int rhs) { return new Vector2D_int_stack(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_int_stack coord_xform(Func<int, int> op) { return new Vector2D_int_stack(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_int_stack other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_int_stack other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException(); // arguable, but example class did this
    };

    public readonly ref struct Vector2D_int_stack_r
    {
        public readonly int X;
        public readonly int Y;

        public Vector2D_int_stack_r(int x, int y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_int_stack_r(int x) => new Vector2D_int_stack_r(x, x);

        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_int_stack_r operator +(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return new Vector2D_int_stack_r(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int_stack_r operator -(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return new Vector2D_int_stack_r(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_int_stack_r operator -(Vector2D_int_stack_r src) { return new Vector2D_int_stack_r(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int_stack_r operator *(int lhs, in Vector2D_int_stack_r rhs) { return new Vector2D_int_stack_r(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_int_stack_r operator *(in Vector2D_int_stack_r lhs, int rhs) { return new Vector2D_int_stack_r(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int_stack_r operator /(in Vector2D_int_stack_r lhs, int rhs) { return new Vector2D_int_stack_r(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_int_stack_r coord_xform(Func<int, int> op) { return new Vector2D_int_stack_r(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(in Vector2D_int_stack_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(in Vector2D_int_stack_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException();
    };

    [Serializable]
    public struct Box2D_int : IComparable<Box2D_int>, IEquatable<Box2D_int>
    {
        private Vector2D_int _anchor;
        private Vector2D_int _dim;

        public Box2D_int(Vector2D_int origin, Vector2D_int size)
        {
            _anchor = origin;
            _dim = size;
        }

        public Box2D_int(int originx, int originy, int sizex, int sizey)
        {
            _anchor = new Vector2D_int(originx, originy);
            _dim = new Vector2D_int(sizex, sizey);
        }

        public Box2D_int(Vector2D_int origin, short sizex, short sizey)
        {
            _anchor = origin;
            _dim = new Vector2D_int(sizex, sizey);
        }

        static public Box2D_int FromLTRB(int left, int top, int right, int bottom) { return new Box2D_int(left,top,right-left,bottom-top); }

        static readonly public Box2D_int Empty = new Box2D_int(0, 0, 0, 0);
        public bool IsEmpty { get { return 0 == _anchor.X && 0 == _anchor.Y && 0 == _dim.X && 0 == _dim.Y; } }

        static public bool operator ==(Box2D_int lhs, Box2D_int rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Box2D_int lhs, Box2D_int rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Box2D_int lhs, Box2D_int rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Box2D_int lhs, Box2D_int rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Box2D_int lhs, Box2D_int rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Box2D_int lhs, Box2D_int rhs) { return 0 <= lhs.CompareTo(rhs); }

        #region pure getters
        public int Bottom { get { return _anchor.Y + _dim.Y; } }
        public int Left { get { return _anchor.X; } }
        public int Right { get { return _anchor.X + _dim.X; } }
        public int Top { get { return _anchor.Y; } }
#endregion

#region get/set pairs
        public int Height {
            get { return _dim.Y; }
            set { _dim.Y = value; }
        }

        public int Width
        {
            get { return _dim.X; }
            set { _dim.X = value; }
        }

        public Vector2D_int Size
        {
            get { return _dim; }
            set { _dim = value; }
        }

        public Vector2D_int Location
        {
            get { return _anchor; }
            set { _anchor = value; }
        }

        public int X
        {
            get { return _anchor.X; }
            set { _anchor.X = value; }
        }

        public int Y
        {
            get { return _anchor.Y; }
            set { _anchor.Y = value; }
        }
        #endregion

        static public Box2D_int Union(Box2D_int lhs, Box2D_int rhs) {
            Box2D_int ret = lhs;
            if (ret.X > rhs.X) {
              ret.Width += ret.X - rhs.X;
              ret.X = rhs.X;
            }
            if (ret.Y > rhs.Y) {
                ret.Height += ret.Y - rhs.Y;
                ret.Y = rhs.Y;
            }
            if (ret.Right < rhs.Right) ret.Width += rhs.Right - ret.Right;
            if (ret.Bottom < rhs.Bottom) ret.Height += rhs.Bottom - ret.Bottom;
            return ret;
        }

        public Box2D_int Intersect(Box2D_int rhs) {
            Box2D_int ret = this;
            if (ret.X < rhs.X) {
              ret.Width -= rhs.X - ret.X;
              ret.X = rhs.X;
            }
            if (ret.Y < rhs.Y) {
                ret.Height -= rhs.Y - ret.Y;
                ret.Y = rhs.Y;
            }
            if (ret.Right > rhs.Right) ret.Width -= ret.Right - rhs.Right;
            if (ret.Bottom > rhs.Bottom) ret.Height -= ret.Bottom - rhs.Bottom;
            return ret;
        }

        // these are not the safest implementations for integer math
        public bool Contains(int x, int y) { return _anchor.X <= x && x < Right && _anchor.Y <= y && y < Bottom; }
        public bool Contains(Vector2D_int src) { return _anchor.X <= src.X && src.X < Right && _anchor.Y <= src.Y && src.Y < Bottom; }
        public bool Contains(Box2D_int src) { return Left <= src.Left && src.Right <= Right && Top <= src.Top && src.Bottom <= Bottom; }

        // closely related to compass rose ordering
        public int EdgeCode(Vector2D_int src)
        {
            int ret = 0;
            if (_anchor.X == src.X) ret += 8;
            if (Right - 1 == src.X) ret += 2;
            if (_anchor.Y == src.Y) ret += 1;
            if (Bottom - 1 == src.Y) ret += 4;
            return ret;
        }

#nullable enable
    public Vector2D_int? FirstOrDefault(Predicate<Vector2D_int> testFn)
    {
      Vector2D_int point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        for (point.Y = Top; point.Y < Bottom; ++point.Y) {
          if (testFn(point)) return point;
        }
      }
      return null;
    }

    public bool Any(Predicate<Vector2D_int> testFn)
    {
      return  null != FirstOrDefault(testFn);
    }

    public void DoForEach(Action<Vector2D_int> doFn, Predicate<Vector2D_int> testFn)
    {
      Vector2D_int point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        for (point.Y = Top; point.Y < Bottom; ++point.Y) {
          if (testFn(point)) doFn(point);
        }
      }
    }

    public void DoForEach<T>(Action<T> doFn, Func<Vector2D_int,T?> testFn) where T:class
    {
      Vector2D_int pt = new Vector2D_int();
      for (pt.X = Left; pt.X < Right; ++pt.X) {
        for (pt.Y = Top; pt.Y < Bottom; ++pt.Y) {
          var test = testFn(pt);
          if (null != test) doFn(test);
        }
      }
    }

    public void DoForEach(Action<Vector2D_int> doFn)
    {
      Vector2D_int point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        for (point.Y = Top; point.Y < Bottom; ++point.Y) {
          doFn(point);
        }
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_int> doFn)
    {
      var point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        point.Y = Top;
        doFn(point);
        point.Y = Bottom-1;
        doFn(point);
      }
      if (2 >= Height) return;
      for (point.Y = Top+1; point.Y < Bottom-1; ++point.Y) {
        point.X = Left;
        doFn(point);
        point.X = Right-1;
        doFn(point);
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_int> doFn, Predicate<Vector2D_int> testFn)
    {
      var point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        point.Y = Top;
        if (testFn(point)) doFn(point);
        point.Y = Bottom-1;
        if (testFn(point)) doFn(point);
      }
      if (2 >= Height) return;
      for (point.Y = Top+1; point.Y < Bottom-1; ++point.Y) {
        point.X = Left;
        if (testFn(point)) doFn(point);
        point.X = Right-1;
        if (testFn(point)) doFn(point);
      }
    }

    public List<Vector2D_int> Where(Predicate<Vector2D_int> testFn)
    {
      List<Vector2D_int> ret = new List<Vector2D_int>();
      DoForEach(pt => ret.Add(pt),testFn);
      return ret;
    }

    public List<Vector2D_int> WhereOnEdge(Predicate<Vector2D_int> testFn)
    {
      var ret = new List<Vector2D_int>();
      DoForEachOnEdge(pt => ret.Add(pt),testFn);
      return ret;
    }

    public void WhereOnEdge(ref Stack<Vector2D_int> dest, Predicate<Vector2D_int> testFn)
    {
      var poshort = new Vector2D_int();   // inline DoForEachOnEdge
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        poshort.Y = Top;
        if (testFn(poshort)) dest.push(ref poshort);
        poshort.Y = (short)(Bottom -1);
        if (testFn(poshort)) dest.push(ref poshort);
      }
      if (2 >= Height) return;
      for (poshort.Y = (short)(Top +1); poshort.Y < Bottom-2; ++poshort.Y) {
        poshort.X = Left;
        if (testFn(poshort)) dest.push(ref poshort);
        poshort.X = (short)(Right -1);
        if (testFn(poshort)) dest.push(ref poshort);
      }
    }
#nullable restore

        // lexicographic sort; IComparable<>
        public int CompareTo(Box2D_int other)
        {
            int ret = _anchor.CompareTo(other._anchor);
            if (0 != ret) return ret;
            return _dim.CompareTo(other._dim);
        }

        // IEquatable<>
        public bool Equals(Box2D_int other)
        {
            return _anchor == other._anchor && _dim == other._dim;
        }

        public override bool Equals(object obj)
        {
            return obj is Box2D_int test && Equals(test);
        }

        public override int GetHashCode()
        {
            return _anchor.GetHashCode()^_dim.GetHashCode();
        }
    }

    [Serializable]
    public struct Vector2D_short : IComparable<Vector2D_short>,IEquatable<Vector2D_short>
    {
        public short X;
        public short Y;

        public Vector2D_short(short x, short y)
        {
            X = x;
            Y = y;
        }
        public Vector2D_short(short x, int y)
        {
            X = x;
            Y = (short)y;
        }
        public Vector2D_short(int x, short y)
        {
            X = (short)x;
            Y = y;
        }
        public Vector2D_short(int x, int y)
        {
            X = (short)x;
            Y = (short)y;
        }
        static public explicit operator Vector2D_short(short x) => new Vector2D_short(x, x);

        static readonly public Vector2D_short Empty = new Vector2D_short(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }
        static public readonly Vector2D_short MaxValue = new Vector2D_short(short.MaxValue, short.MaxValue);
        static public readonly Vector2D_short MinValue = new Vector2D_short(short.MinValue, short.MinValue);

        static public bool operator ==(Vector2D_short lhs, Vector2D_short rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_short lhs, Vector2D_short rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_short lhs, Vector2D_short rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_short lhs, Vector2D_short rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_short lhs, Vector2D_short rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_short lhs, Vector2D_short rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_short operator +(Vector2D_short lhs, Vector2D_short rhs) { return new Vector2D_short((short)(lhs.X + rhs.X), (short)(lhs.Y + rhs.Y)); }
        static public Vector2D_short operator -(Vector2D_short lhs, Vector2D_short rhs) { return new Vector2D_short((short)(lhs.X - rhs.X), (short)(lhs.Y - rhs.Y)); }
        static public Vector2D_short operator -(Vector2D_short src) { return new Vector2D_short((short)(-src.X), (short)(-src.Y)); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_short operator *(short lhs, Vector2D_short rhs) { return new Vector2D_short((short)(lhs *rhs.X), (short)(lhs *rhs.Y)); }
        static public Vector2D_short operator *(Vector2D_short lhs, short rhs) { return new Vector2D_short((short)(lhs.X * rhs), (short)(lhs.Y * rhs)); }
        static public Vector2D_short operator /(Vector2D_short lhs, short rhs) { return new Vector2D_short((short)(lhs.X / rhs), (short)(lhs.Y / rhs)); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_short coord_xform(Func<short, short> op) { return new Vector2D_short(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_short other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_short other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D_short test && Equals(test);
        }

        public override int GetHashCode()
        {
            return unchecked(1 + 19 * X + 19 * 17 * Y);
        }
    };

    [Serializable]
    public readonly struct Vector2D_short_r : IComparable<Vector2D_short_r>, IEquatable<Vector2D_short_r>
    {
        public readonly short X;
        public readonly short Y;

        public Vector2D_short_r(short x, short y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_short_r(short x) => new Vector2D_short_r(x, x);

        static readonly public Vector2D_short_r Empty = new Vector2D_short_r(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }
        static public readonly Vector2D_short_r MaxValue = new Vector2D_short_r(short.MaxValue, short.MaxValue);
        static public readonly Vector2D_short_r MinValue = new Vector2D_short_r(short.MinValue, short.MinValue);

        static public bool operator ==(Vector2D_short_r lhs, Vector2D_short_r rhs) { return lhs.Equals(in rhs); }
        static public bool operator !=(Vector2D_short_r lhs, Vector2D_short_r rhs) { return !lhs.Equals(in rhs); }
        static public bool operator <(Vector2D_short_r lhs, Vector2D_short_r rhs) { return 0 > lhs.CompareTo(in rhs); }
        static public bool operator >(Vector2D_short_r lhs, Vector2D_short_r rhs) { return 0 < lhs.CompareTo(in rhs); }
        static public bool operator <=(Vector2D_short_r lhs, Vector2D_short_r rhs) { return 0 >= lhs.CompareTo(in rhs); }
        static public bool operator >=(Vector2D_short_r lhs, Vector2D_short_r rhs) { return 0 <= lhs.CompareTo(in rhs); }

        // vector arithmetic
        static public Vector2D_short_r operator +(in Vector2D_short_r lhs, in Vector2D_short_r rhs) { return new Vector2D_short_r((short)(lhs.X + rhs.X), (short)(lhs.Y + rhs.Y)); }
        static public Vector2D_short_r operator -(in Vector2D_short_r lhs, in Vector2D_short_r rhs) { return new Vector2D_short_r((short)(lhs.X - rhs.X), (short)(lhs.Y - rhs.Y)); }
        static public Vector2D_short_r operator -(Vector2D_short_r src) { return new Vector2D_short_r((short)(-src.X), (short)(-src.Y)); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_short_r operator *(short lhs, in Vector2D_short_r rhs) { return new Vector2D_short_r((short)(lhs * rhs.X), (short)(lhs * rhs.Y)); }
        static public Vector2D_short_r operator *(in Vector2D_short_r lhs, short rhs) { return new Vector2D_short_r((short)(lhs.X * rhs), (short)(lhs.Y * rhs)); }
        static public Vector2D_short_r operator /(in Vector2D_short_r lhs, short rhs) { return new Vector2D_short_r((short)(lhs.X / rhs), (short)(lhs.Y / rhs)); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_short_r coord_xform(Func<short, short> op) { return new Vector2D_short_r(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_short_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        public int CompareTo(in Vector2D_short_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_short_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public bool Equals(in Vector2D_short_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D_short test && Equals(test);
        }

        public override int GetHashCode()
        {
            return unchecked(1 + 19 * X + 19 * 17 * Y);
        }
    };

    public ref struct Vector2D_short_stack
    {
        public short X;
        public short Y;

        public Vector2D_short_stack(short x, short y)
        {
            X = x;
            Y = y;
        }
        public Vector2D_short_stack(short x, int y)
        {
            X = x;
            Y = (short)y;
        }
        public Vector2D_short_stack(int x, short y)
        {
            X = (short)x;
            Y = y;
        }
        public Vector2D_short_stack(int x, int y)
        {
            X = (short)x;
            Y = (short)y;
        }
        static public explicit operator Vector2D_short_stack(short x) => new Vector2D_short_stack(x, x);

        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_short_stack operator +(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return new Vector2D_short_stack((short)(lhs.X + rhs.X), (short)(lhs.Y + rhs.Y)); }
        static public Vector2D_short_stack operator -(Vector2D_short_stack lhs, Vector2D_short_stack rhs) { return new Vector2D_short_stack((short)(lhs.X - rhs.X), (short)(lhs.Y - rhs.Y)); }
        static public Vector2D_short_stack operator -(Vector2D_short_stack src) { return new Vector2D_short_stack((short)(-src.X), (short)(-src.Y)); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_short_stack operator *(short lhs, Vector2D_short_stack rhs) { return new Vector2D_short_stack((short)(lhs * rhs.X), (short)(lhs * rhs.Y)); }
        static public Vector2D_short_stack operator *(Vector2D_short_stack lhs, short rhs) { return new Vector2D_short_stack((short)(lhs.X * rhs), (short)(lhs.Y * rhs)); }
        static public Vector2D_short_stack operator /(Vector2D_short_stack lhs, short rhs) { return new Vector2D_short_stack((short)(lhs.X / rhs), (short)(lhs.Y / rhs)); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_short_stack coord_xform(Func<short, short> op) { return new Vector2D_short_stack(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_short_stack other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_short_stack other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException(); // arguable, but example class did this
    };

    public readonly ref struct Vector2D_short_stack_r
    {
        public readonly short X;
        public readonly short Y;

        public Vector2D_short_stack_r(short x, short y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_short_stack_r(short x) => new Vector2D_short_stack_r(x, x);

        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_short_stack_r operator +(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return new Vector2D_short_stack_r((short)(lhs.X + rhs.X), (short)(lhs.Y + rhs.Y)); }
        static public Vector2D_short_stack_r operator -(in Vector2D_short_stack_r lhs, in Vector2D_short_stack_r rhs) { return new Vector2D_short_stack_r((short)(lhs.X - rhs.X), (short)(lhs.Y - rhs.Y)); }
        static public Vector2D_short_stack_r operator -(Vector2D_short_stack_r src) { return new Vector2D_short_stack_r((short)(-src.X), (short)(-src.Y)); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_short_stack_r operator *(short lhs, in Vector2D_short_stack_r rhs) { return new Vector2D_short_stack_r((short)(lhs * rhs.X), (short)(lhs * rhs.Y)); }
        static public Vector2D_short_stack_r operator *(in Vector2D_short_stack_r lhs, short rhs) { return new Vector2D_short_stack_r((short)(lhs.X * rhs), (short)(lhs.Y * rhs)); }
        static public Vector2D_short_stack_r operator /(in Vector2D_short_stack_r lhs, short rhs) { return new Vector2D_short_stack_r((short)(lhs.X / rhs), (short)(lhs.Y / rhs)); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_short_stack_r coord_xform(Func<short, short> op) { return new Vector2D_short_stack_r(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(in Vector2D_short_stack_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(in Vector2D_short_stack_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException();
    };

    [Serializable]
    public struct Box2D_short : IComparable<Box2D_short>, IEquatable<Box2D_short>
    {
        private Vector2D_short _anchor;
        private Vector2D_short _dim;

        public Box2D_short(Vector2D_short origin, Vector2D_short size)
        {
            _anchor = origin;
            _dim = size;
        }

        public Box2D_short(int originx, int originy, int sizex, int sizey)
        {
            _anchor = new Vector2D_short(originx, originy);
            _dim = new Vector2D_short(sizex, sizey);
        }

        public Box2D_short(Vector2D_short origin, short sizex, short sizey)
        {
            _anchor = origin;
            _dim = new Vector2D_short(sizex, sizey);
        }

        static public Box2D_short FromLTRB(int left, int top, int right, int bottom) { return new Box2D_short(left, top, right - left, bottom - top); }

        static readonly public Box2D_short Empty = new Box2D_short(0, 0, 0, 0);
        public bool IsEmpty { get { return 0 == _anchor.X && 0 == _anchor.Y && 0 == _dim.X && 0 == _dim.Y; } }

        static public bool operator ==(Box2D_short lhs, Box2D_short rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Box2D_short lhs, Box2D_short rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Box2D_short lhs, Box2D_short rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Box2D_short lhs, Box2D_short rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Box2D_short lhs, Box2D_short rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Box2D_short lhs, Box2D_short rhs) { return 0 <= lhs.CompareTo(rhs); }

#region pure getters
        public short Bottom { get { return (short)(_anchor.Y + _dim.Y); } }
        public short Left { get { return _anchor.X; } }
        public short Right { get { return (short)(_anchor.X + _dim.X); } }
        public short Top { get { return _anchor.Y; } }
#endregion

#region get/set pairs
        public short Height {
            get { return _dim.Y; }
            set { _dim.Y = value; }
        }

        public short Width
        {
            get { return _dim.X; }
            set { _dim.X = value; }
        }

        public Vector2D_short Size
        {
            get { return _dim; }
            set { _dim = value; }
        }

        public Vector2D_short Location
        {
            get { return _anchor; }
            set { _anchor = value; }
        }

        public short X
        {
            get { return _anchor.X; }
            set { _anchor.X = value; }
        }

        public short Y
        {
            get { return _anchor.Y; }
            set { _anchor.Y = value; }
        }
        #endregion

        static public Box2D_short Union(Box2D_short lhs, Box2D_short rhs) {
            Box2D_short ret = lhs;
            if (ret.X > rhs.X) {
              ret.Width += (short)(ret.X - rhs.X);
              ret.X = rhs.X;
            }
            if (ret.Y > rhs.Y) {
                ret.Height += (short)(ret.Y - rhs.Y);
                ret.Y = rhs.Y;
            }
            if (ret.Right < rhs.Right) ret.Width += (short)(rhs.Right - ret.Right);
            if (ret.Bottom < rhs.Bottom) ret.Height += (short)(rhs.Bottom - ret.Bottom);
            return ret;
        }

        public Box2D_short Intersect(Box2D_short rhs) {
            Box2D_short ret = this;
            if (ret.X < rhs.X) {
              ret.Width -= (short)(rhs.X - ret.X);
              ret.X = rhs.X;
            }
            if (ret.Y < rhs.Y) {
                ret.Height -= (short)(rhs.Y - ret.Y);
                ret.Y = rhs.Y;
            }
            if (ret.Right > rhs.Right) ret.Width -= (short)(ret.Right - rhs.Right);
            if (ret.Bottom > rhs.Bottom) ret.Height -= (short)(ret.Bottom - rhs.Bottom);
            return ret;
        }

        // these are not the safest implementations for integer math
        public bool Contains(short x, short y) { return _anchor.X <= x && x < Right && _anchor.Y <= y && y < Bottom; }
        public bool Contains(Vector2D_short src) { return _anchor.X <= src.X && src.X < Right && _anchor.Y <= src.Y && src.Y < Bottom; }
        public bool Contains(Box2D_short src) { return Left <= src.Left && src.Right <= Right && Top <= src.Top && src.Bottom <= Bottom; }

        // closely related to compass rose ordering
        public int EdgeCode(Vector2D_short src) {
            int ret = 0;
            if (_anchor.X == src.X) ret += 8;
            if (Right - 1 == src.X) ret += 2;
            if (_anchor.Y == src.Y) ret += 1;
            if (Bottom - 1 == src.Y) ret += 4;
            return ret;
        }

#nullable enable
    public Vector2D_short? FirstOrDefault(Predicate<Vector2D_short> testFn)
    {
      Vector2D_short poshort = new Vector2D_short();
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        for (poshort.Y = Top; poshort.Y < Bottom; ++poshort.Y) {
          if (testFn(poshort)) return poshort;
        }
      }
      return null;
    }

    public bool Any(Predicate<Vector2D_short> testFn)
    {
      return  null != FirstOrDefault(testFn);
    }

    public void DoForEach(Action<Vector2D_short> doFn, Predicate<Vector2D_short> testFn)
    {
      Vector2D_short poshort = new Vector2D_short();
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        for (poshort.Y = Top; poshort.Y < Bottom; ++poshort.Y) {
          if (testFn(poshort)) doFn(poshort);
        }
      }
    }

    public void DoForEach<T>(Action<T> doFn, Func<Vector2D_short,T?> testFn) where T:class
    {
      Vector2D_short pt = new Vector2D_short();
      for (pt.X = Left; pt.X < Right; ++pt.X) {
        for (pt.Y = Top; pt.Y < Bottom; ++pt.Y) {
          var test = testFn(pt);
          if (null != test) doFn(test);
        }
      }
    }

    public void DoForEach(Action<Vector2D_short> doFn)
    {
      Vector2D_short poshort = new Vector2D_short();
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        for (poshort.Y = Top; poshort.Y < Bottom; ++poshort.Y) {
          doFn(poshort);
        }
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_short> doFn)
    {
      var poshort = new Vector2D_short();
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        poshort.Y = Top;
        doFn(poshort);
        poshort.Y = (short)(Bottom -1);
        doFn(poshort);
      }
      if (2 >= Height) return;
      for (poshort.Y = (short)(Top +1); poshort.Y < Bottom-1; ++poshort.Y) {
        poshort.X = Left;
        doFn(poshort);
        poshort.X = (short)(Right -1);
        doFn(poshort);
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_short> doFn, Predicate<Vector2D_short> testFn)
    {
      var poshort = new Vector2D_short();
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        poshort.Y = Top;
        if (testFn(poshort)) doFn(poshort);
        poshort.Y = (short)(Bottom -1);
        if (testFn(poshort)) doFn(poshort);
      }
      if (2 >= Height) return;
      for (poshort.Y = (short)(Top +1); poshort.Y < Bottom-1; ++poshort.Y) {
        poshort.X = Left;
        if (testFn(poshort)) doFn(poshort);
        poshort.X = (short)(Right -1);
        if (testFn(poshort)) doFn(poshort);
      }
    }

    public List<Vector2D_short> Where(Predicate<Vector2D_short> testFn)
    {
      List<Vector2D_short> ret = new List<Vector2D_short>();
      DoForEach(pt => ret.Add(pt),testFn);
      return ret;
    }

    public List<Vector2D_short> WhereOnEdge(Predicate<Vector2D_short> testFn)
    {
      var ret = new List<Vector2D_short>();
      DoForEachOnEdge(pt => ret.Add(pt),testFn);
      return ret;
    }

    public void WhereOnEdge(ref Stack<Vector2D_short> dest, Predicate<Vector2D_short> testFn)
    {
      var poshort = new Vector2D_short();   // inline DoForEachOnEdge
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        poshort.Y = Top;
        if (testFn(poshort)) dest.push(ref poshort);
        poshort.Y = (short)(Bottom -1);
        if (testFn(poshort)) dest.push(ref poshort);
            }
      if (2 >= Height) return;
      for (poshort.Y = (short)(Top +1); poshort.Y < Bottom-2; ++poshort.Y) {
        poshort.X = Left;
        if (testFn(poshort)) dest.push(ref poshort);
        poshort.X = (short)(Right -1);
        if (testFn(poshort)) dest.push(ref poshort);
      }
    }
#nullable restore

        // lexicographic sort; IComparable<>
        public int CompareTo(Box2D_short other)
        {
            int ret = _anchor.CompareTo(other._anchor);
            if (0 != ret) return ret;
            return _dim.CompareTo(other._dim);
        }

        // IEquatable<>
        public bool Equals(Box2D_short other)
        {
            return _anchor == other._anchor && _dim == other._dim;
        }

        public override bool Equals(object obj)
        {
            return obj is Box2D_short test && Equals(test);
        }

        public override int GetHashCode()
        {
            return _anchor.GetHashCode()^_dim.GetHashCode();
        }
    }

    [Serializable]
    public struct Vector2D_long : IComparable<Vector2D_long>,IEquatable<Vector2D_long>
    {
        public long X;
        public long Y;

        public Vector2D_long(long x, long y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_long(long x) => new Vector2D_long(x, x);

        static readonly public Vector2D_long Empty = new Vector2D_long(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }
        static public readonly Vector2D_long MaxValue = new Vector2D_long(long.MaxValue, long.MaxValue);
        static public readonly Vector2D_long MinValue = new Vector2D_long(long.MinValue, long.MinValue);

        static public bool operator ==(Vector2D_long lhs, Vector2D_long rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_long lhs, Vector2D_long rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_long lhs, Vector2D_long rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_long lhs, Vector2D_long rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_long lhs, Vector2D_long rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_long lhs, Vector2D_long rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_long operator +(Vector2D_long lhs, Vector2D_long rhs) { return new Vector2D_long(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_long operator -(Vector2D_long lhs, Vector2D_long rhs) { return new Vector2D_long(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_long operator -(Vector2D_long src) { return new Vector2D_long(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_long operator *(long lhs, Vector2D_long rhs) { return new Vector2D_long(lhs*rhs.X, lhs*rhs.Y); }
        static public Vector2D_long operator *(Vector2D_long lhs, long rhs) { return new Vector2D_long(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_long operator /(Vector2D_long lhs, long rhs) { return new Vector2D_long(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_long coord_xform(Func<long, long> op) { return new Vector2D_long(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_long other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_long other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D_long test && Equals(test);
        }

        public override int GetHashCode()
        {
            return (int)(unchecked(1 + 19 * X + 19 * 17 * Y));
        }
    };

    [Serializable]
    public readonly struct Vector2D_long_r : IComparable<Vector2D_long_r>, IEquatable<Vector2D_long_r>
    {
        public readonly long X;
        public readonly long Y;

        public Vector2D_long_r(long x, long y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_long_r(long x) => new Vector2D_long_r(x, x);

        static readonly public Vector2D_long_r Empty = new Vector2D_long_r(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }
        static public readonly Vector2D_long_r MaxValue = new Vector2D_long_r(long.MaxValue, long.MaxValue);
        static public readonly Vector2D_long_r MinValue = new Vector2D_long_r(long.MinValue, long.MinValue);

        static public bool operator ==(Vector2D_long_r lhs, Vector2D_long_r rhs) { return lhs.Equals(in rhs); }
        static public bool operator !=(Vector2D_long_r lhs, Vector2D_long_r rhs) { return !lhs.Equals(in rhs); }
        static public bool operator <(Vector2D_long_r lhs, Vector2D_long_r rhs) { return 0 > lhs.CompareTo(in rhs); }
        static public bool operator >(Vector2D_long_r lhs, Vector2D_long_r rhs) { return 0 < lhs.CompareTo(in rhs); }
        static public bool operator <=(Vector2D_long_r lhs, Vector2D_long_r rhs) { return 0 >= lhs.CompareTo(in rhs); }
        static public bool operator >=(Vector2D_long_r lhs, Vector2D_long_r rhs) { return 0 <= lhs.CompareTo(in rhs); }

        // vector arithmetic
        static public Vector2D_long_r operator +(in Vector2D_long_r lhs, in Vector2D_long_r rhs) { return new Vector2D_long_r(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_long_r operator -(in Vector2D_long_r lhs, in Vector2D_long_r rhs) { return new Vector2D_long_r(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_long_r operator -(Vector2D_long_r src) { return new Vector2D_long_r(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_long_r operator *(long lhs, in Vector2D_long_r rhs) { return new Vector2D_long_r(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_long_r operator *(in Vector2D_long_r lhs, long rhs) { return new Vector2D_long_r(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_long_r operator /(in Vector2D_long_r lhs, long rhs) { return new Vector2D_long_r(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_long_r coord_xform(Func<long, long> op) { return new Vector2D_long_r(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public int CompareTo(Vector2D_long_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        public int CompareTo(in Vector2D_long_r other)
        {
            int ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_long_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public bool Equals(in Vector2D_long_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D_long test && Equals(test);
        }

        public override int GetHashCode()
        {
            return (int)unchecked(1 + 19 * X + 19 * 17 * Y);
        }
    };

    public ref struct Vector2D_long_stack
    {
        public long X;
        public long Y;

        public Vector2D_long_stack(long x, long y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_long_stack(long x) => new Vector2D_long_stack(x, x);

        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_long_stack operator +(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return new Vector2D_long_stack(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_long_stack operator -(Vector2D_long_stack lhs, Vector2D_long_stack rhs) { return new Vector2D_long_stack(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_long_stack operator -(Vector2D_long_stack src) { return new Vector2D_long_stack(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_long_stack operator *(long lhs, Vector2D_long_stack rhs) { return new Vector2D_long_stack(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_long_stack operator *(Vector2D_long_stack lhs, long rhs) { return new Vector2D_long_stack(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_long_stack operator /(Vector2D_long_stack lhs, long rhs) { return new Vector2D_long_stack(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_long_stack coord_xform(Func<long, long> op) { return new Vector2D_long_stack(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public long CompareTo(Vector2D_long_stack other)
        {
            long ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(Vector2D_long_stack other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException(); // arguable, but example class did this
    };

    public readonly ref struct Vector2D_long_stack_r
    {
        public readonly long X;
        public readonly long Y;

        public Vector2D_long_stack_r(long x, long y)
        {
            X = x;
            Y = y;
        }
        static public explicit operator Vector2D_long_stack_r(long x) => new Vector2D_long_stack_r(x, x);

        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_long_stack_r operator +(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return new Vector2D_long_stack_r(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_long_stack_r operator -(in Vector2D_long_stack_r lhs, in Vector2D_long_stack_r rhs) { return new Vector2D_long_stack_r(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        static public Vector2D_long_stack_r operator -(Vector2D_long_stack_r src) { return new Vector2D_long_stack_r(-src.X, -src.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_long_stack_r operator *(long lhs, in Vector2D_long_stack_r rhs) { return new Vector2D_long_stack_r(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_long_stack_r operator *(in Vector2D_long_stack_r lhs, long rhs) { return new Vector2D_long_stack_r(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_long_stack_r operator /(in Vector2D_long_stack_r lhs, long rhs) { return new Vector2D_long_stack_r(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

        // other coordinate-wise operations
        public Vector2D_long_stack_r coord_xform(Func<long, long> op) { return new Vector2D_long_stack_r(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        // lexicographic sort; IComparable<>
        public long CompareTo(in Vector2D_long_stack_r other)
        {
            long ret = X.CompareTo(other.X);
            if (0 != ret) return ret;
            return Y.CompareTo(other.Y);
        }

        // IEquatable<>
        public bool Equals(in Vector2D_long_stack_r other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public override string ToString() => throw new NotSupportedException();
    };

    [Serializable]
    public struct Box2D_long : IComparable<Box2D_long>, IEquatable<Box2D_long>
    {
        private Vector2D_long _anchor;
        private Vector2D_long _dim;

        public Box2D_long(Vector2D_long origin, Vector2D_long size)
        {
            _anchor = origin;
            _dim = size;
        }

        public Box2D_long(long originx, long originy, long sizex, long sizey)
        {
            _anchor = new Vector2D_long(originx, originy);
            _dim = new Vector2D_long(sizex, sizey);
        }

        public Box2D_long(Vector2D_long origin, short sizex, short sizey)
        {
            _anchor = origin;
            _dim = new Vector2D_long(sizex, sizey);
        }

        static public Box2D_long FromLTRB(long left, long top, long right, long bottom) { return new Box2D_long(left,top,right-left,bottom-top); }

        static readonly public Box2D_long Empty = new Box2D_long(0, 0, 0, 0);
        public bool IsEmpty { get { return 0 == _anchor.X && 0 == _anchor.Y && 0 == _dim.X && 0 == _dim.Y; } }

        static public bool operator ==(Box2D_long lhs, Box2D_long rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Box2D_long lhs, Box2D_long rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Box2D_long lhs, Box2D_long rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Box2D_long lhs, Box2D_long rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Box2D_long lhs, Box2D_long rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Box2D_long lhs, Box2D_long rhs) { return 0 <= lhs.CompareTo(rhs); }

        #region pure getters
        public long Bottom { get { return _anchor.Y + _dim.Y; } }
        public long Left { get { return _anchor.X; } }
        public long Right { get { return _anchor.X + _dim.X; } }
        public long Top { get { return _anchor.Y; } }
#endregion

#region get/set pairs
        public long Height {
            get { return _dim.Y; }
            set { _dim.Y = value; }
        }

        public long Width
        {
            get { return _dim.X; }
            set { _dim.X = value; }
        }

        public Vector2D_long Size
        {
            get { return _dim; }
            set { _dim = value; }
        }

        public Vector2D_long Location
        {
            get { return _anchor; }
            set { _anchor = value; }
        }

        public long X
        {
            get { return _anchor.X; }
            set { _anchor.X = value; }
        }

        public long Y
        {
            get { return _anchor.Y; }
            set { _anchor.Y = value; }
        }
        #endregion

        static public Box2D_long Union(Box2D_long lhs, Box2D_long rhs) {
            Box2D_long ret = lhs;
            if (ret.X > rhs.X) {
              ret.Width += ret.X - rhs.X;
              ret.X = rhs.X;
            }
            if (ret.Y > rhs.Y) {
                ret.Height += ret.Y - rhs.Y;
                ret.Y = rhs.Y;
            }
            if (ret.Right < rhs.Right) ret.Width += rhs.Right - ret.Right;
            if (ret.Bottom < rhs.Bottom) ret.Height += rhs.Bottom - ret.Bottom;
            return ret;
        }

        public Box2D_long Intersect(Box2D_long rhs) {
            Box2D_long ret = this;
            if (ret.X < rhs.X) {
              ret.Width -= rhs.X - ret.X;
              ret.X = rhs.X;
            }
            if (ret.Y < rhs.Y) {
                ret.Height -= rhs.Y - ret.Y;
                ret.Y = rhs.Y;
            }
            if (ret.Right > rhs.Right) ret.Width -= ret.Right - rhs.Right;
            if (ret.Bottom > rhs.Bottom) ret.Height -= ret.Bottom - rhs.Bottom;
            return ret;
        }

        // these are not the safest implementations for integer math
        public bool Contains(long x, long y) { return _anchor.X <= x && x < Right && _anchor.Y <= y && y < Bottom; }
        public bool Contains(Vector2D_long src) { return _anchor.X <= src.X && src.X < Right && _anchor.Y <= src.Y && src.Y < Bottom; }
        public bool Contains(Box2D_long src) { return Left <= src.Left && src.Right <= Right && Top <= src.Top && src.Bottom <= Bottom; }

        // closely related to compass rose ordering
        public int EdgeCode(Vector2D_short src)
        {
            int ret = 0;
            if (_anchor.X == src.X) ret += 8;
            if (Right - 1 == src.X) ret += 2;
            if (_anchor.Y == src.Y) ret += 1;
            if (Bottom - 1 == src.Y) ret += 4;
            return ret;
        }

#nullable enable
    public Vector2D_long? FirstOrDefault(Predicate<Vector2D_long> testFn)
    {
      Vector2D_long polong = new Vector2D_long();
      for (polong.X = Left; polong.X < Right; ++polong.X) {
        for (polong.Y = Top; polong.Y < Bottom; ++polong.Y) {
          if (testFn(polong)) return polong;
        }
      }
      return null;
    }

    public bool Any(Predicate<Vector2D_long> testFn)
    {
      return  null != FirstOrDefault(testFn);
    }

    public void DoForEach(Action<Vector2D_long> doFn, Predicate<Vector2D_long> testFn)
    {
      Vector2D_long polong = new Vector2D_long();
      for (polong.X = Left; polong.X < Right; ++polong.X) {
        for (polong.Y = Top; polong.Y < Bottom; ++polong.Y) {
          if (testFn(polong)) doFn(polong);
        }
      }
    }

    public void DoForEach<T>(Action<T> doFn, Func<Vector2D_long,T?> testFn) where T:class
    {
      Vector2D_long pt = new Vector2D_long();
      for (pt.X = Left; pt.X < Right; ++pt.X) {
        for (pt.Y = Top; pt.Y < Bottom; ++pt.Y) {
          var test = testFn(pt);
          if (null != test) doFn(test);
        }
      }
    }

    public void DoForEach(Action<Vector2D_long> doFn)
    {
      Vector2D_long polong = new Vector2D_long();
      for (polong.X = Left; polong.X < Right; ++polong.X) {
        for (polong.Y = Top; polong.Y < Bottom; ++polong.Y) {
          doFn(polong);
        }
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_long> doFn)
    {
      var polong = new Vector2D_long();
      for (polong.X = Left; polong.X < Right; ++polong.X) {
        polong.Y = Top;
        doFn(polong);
        polong.Y = Bottom-1;
        doFn(polong);
      }
      if (2 >= Height) return;
      for (polong.Y = Top+1; polong.Y < Bottom-1; ++polong.Y) {
        polong.X = Left;
        doFn(polong);
        polong.X = Right-1;
        doFn(polong);
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_long> doFn, Predicate<Vector2D_long> testFn)
    {
      var polong = new Vector2D_long();
      for (polong.X = Left; polong.X < Right; ++polong.X) {
        polong.Y = Top;
        if (testFn(polong)) doFn(polong);
        polong.Y = Bottom-1;
        if (testFn(polong)) doFn(polong);
      }
      if (2 >= Height) return;
      for (polong.Y = Top+1; polong.Y < Bottom-1; ++polong.Y) {
        polong.X = Left;
        if (testFn(polong)) doFn(polong);
        polong.X = Right-1;
        if (testFn(polong)) doFn(polong);
      }
    }

    public List<Vector2D_long> Where(Predicate<Vector2D_long> testFn)
    {
      List<Vector2D_long> ret = new List<Vector2D_long>();
      DoForEach(pt => ret.Add(pt),testFn);
      return ret;
    }

    public List<Vector2D_long> WhereOnEdge(Predicate<Vector2D_long> testFn)
    {
      var ret = new List<Vector2D_long>();
      DoForEachOnEdge(pt => ret.Add(pt),testFn);
      return ret;
    }

    public void WhereOnEdge(ref Stack<Vector2D_long> dest, Predicate<Vector2D_long> testFn)
    {
      var poshort = new Vector2D_long();   // inline DoForEachOnEdge
      for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
        poshort.Y = Top;
        if (testFn(poshort)) dest.push(ref poshort);
        poshort.Y = (short)(Bottom -1);
        if (testFn(poshort)) dest.push(ref poshort);
      }
      if (2 >= Height) return;
      for (poshort.Y = (short)(Top +1); poshort.Y < Bottom-2; ++poshort.Y) {
        poshort.X = Left;
        if (testFn(poshort)) dest.push(ref poshort);
        poshort.X = (short)(Right -1);
        if (testFn(poshort)) dest.push(ref poshort);
      }
    }
#nullable restore

        // lexicographic sort; IComparable<>
        public int CompareTo(Box2D_long other)
        {
            int ret = _anchor.CompareTo(other._anchor);
            if (0 != ret) return ret;
            return _dim.CompareTo(other._dim);
        }

        // IEquatable<>
        public bool Equals(Box2D_long other)
        {
            return _anchor == other._anchor && _dim == other._dim;
        }

        public override bool Equals(object obj)
        {
            return obj is Box2D_long test && Equals(test);
        }

        public override int GetHashCode()
        {
            return _anchor.GetHashCode()^_dim.GetHashCode();
        }
    }

    public static class ext_Vector {
        public static bool Hull(this IEnumerable<Vector2D_short> src, ref Span<Vector2D_short> hull)
        {
            hull[0] = new Vector2D_short(short.MaxValue, short.MaxValue);
            hull[1] = new Vector2D_short(short.MinValue, short.MinValue);
            short tmp;
            foreach(var pt in src) {
                if ((tmp = pt.X) < hull[0].X) hull[0].X = tmp;
                if (tmp > hull[1].X) hull[1].X = tmp;
                if ((tmp = pt.Y) < hull[0].Y) hull[0].Y = tmp;
                if (tmp > hull[1].Y) hull[1].Y = tmp;
            }
            if (hull[0].X <= hull[1].X) {
                hull[1] += (Vector2D_short)1;
                return true;
            }
            return false;
        }
    }
}
