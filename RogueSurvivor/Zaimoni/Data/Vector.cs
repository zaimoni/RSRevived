using System;
using System.Collections.Generic;

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

        static readonly public Vector2D_int Empty = new Vector2D_int(0, 0);
        public bool IsEmpty { get { return 0 == X && 0 == Y; } }

        static public bool operator ==(Vector2D_int lhs, Vector2D_int rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_int lhs, Vector2D_int rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_int lhs, Vector2D_int rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_int lhs, Vector2D_int rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_int lhs, Vector2D_int rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_int lhs, Vector2D_int rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_int operator +(Vector2D_int lhs, Vector2D_int rhs) { return new Vector2D_int(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int operator -(Vector2D_int lhs, Vector2D_int rhs) { return new Vector2D_int(lhs.X - rhs.X, lhs.Y - rhs.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int operator *(int lhs, Vector2D_int rhs) { return new Vector2D_int(lhs*rhs.X, lhs*rhs.Y); }
        static public Vector2D_int operator *(Vector2D_int lhs, int rhs) { return new Vector2D_int(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int operator /(Vector2D_int lhs, int rhs) { return new Vector2D_int(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

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

        static public bool operator ==(Vector2D_int_r lhs, Vector2D_int_r rhs) { return lhs.Equals(in rhs); }
        static public bool operator !=(Vector2D_int_r lhs, Vector2D_int_r rhs) { return !lhs.Equals(in rhs); }
        static public bool operator <(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 > lhs.CompareTo(in rhs); }
        static public bool operator >(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 < lhs.CompareTo(in rhs); }
        static public bool operator <=(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 >= lhs.CompareTo(in rhs); }
        static public bool operator >=(Vector2D_int_r lhs, Vector2D_int_r rhs) { return 0 <= lhs.CompareTo(in rhs); }

        // vector arithmetic
        static public Vector2D_int_r operator +(in Vector2D_int_r lhs, in Vector2D_int_r rhs) { return new Vector2D_int_r(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int_r operator -(in Vector2D_int_r lhs, in Vector2D_int_r rhs) { return new Vector2D_int_r(lhs.X - rhs.X, lhs.Y - rhs.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int_r operator *(int lhs, in Vector2D_int_r rhs) { return new Vector2D_int_r(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_int_r operator *(in Vector2D_int_r lhs, int rhs) { return new Vector2D_int_r(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int_r operator /(in Vector2D_int_r lhs, int rhs) { return new Vector2D_int_r(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

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

        static public bool operator ==(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_int_stack operator +(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return new Vector2D_int_stack(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int_stack operator -(Vector2D_int_stack lhs, Vector2D_int_stack rhs) { return new Vector2D_int_stack(lhs.X - rhs.X, lhs.Y - rhs.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int_stack operator *(int lhs, Vector2D_int_stack rhs) { return new Vector2D_int_stack(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_int_stack operator *(Vector2D_int_stack lhs, int rhs) { return new Vector2D_int_stack(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int_stack operator /(Vector2D_int_stack lhs, int rhs) { return new Vector2D_int_stack(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

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

        static public bool operator ==(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return lhs.Equals(rhs); }
        static public bool operator !=(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return !lhs.Equals(rhs); }
        static public bool operator <(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 > lhs.CompareTo(rhs); }
        static public bool operator >(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 < lhs.CompareTo(rhs); }
        static public bool operator <=(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 >= lhs.CompareTo(rhs); }
        static public bool operator >=(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return 0 <= lhs.CompareTo(rhs); }

        // vector arithmetic
        static public Vector2D_int_stack_r operator +(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return new Vector2D_int_stack_r(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        static public Vector2D_int_stack_r operator -(in Vector2D_int_stack_r lhs, in Vector2D_int_stack_r rhs) { return new Vector2D_int_stack_r(lhs.X - rhs.X, lhs.Y - rhs.Y); }

        // ignore dot product for integer vectors for now.

        // scalar product/division
        static public Vector2D_int_stack_r operator *(int lhs, in Vector2D_int_stack_r rhs) { return new Vector2D_int_stack_r(lhs * rhs.X, lhs * rhs.Y); }
        static public Vector2D_int_stack_r operator *(in Vector2D_int_stack_r lhs, int rhs) { return new Vector2D_int_stack_r(lhs.X * rhs, lhs.Y * rhs); }
        static public Vector2D_int_stack_r operator /(in Vector2D_int_stack_r lhs, int rhs) { return new Vector2D_int_stack_r(lhs.X / rhs, lhs.Y / rhs); } // arguable whether this is useful

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

#if PROTOTYPE
    // template approach
    [Serializable]
    public struct Vector2D<T>
    {
        public T X;
        public T Y;

        public Vector2D(T x, T y) {
            X = x;
            Y = y;
        }
    };

    public ref struct Vector2D_stack<T>
    {
        public T X;
        public T Y;

        public Vector2D_stack(T x, T y)
        {
            X = x;
            Y = y;
        }
    };

    [Serializable]
    public readonly struct Vector2D_r<T>
    {
        public readonly T X;
        public readonly T Y;

        public Vector2D_r(T x, T y)
        {
            X = x;
            Y = y;
        }
    };

    public readonly ref struct Vector2D_stack_r<T>
    {
        public readonly T X;
        public readonly T Y;

        public Vector2D_stack_r(T x, T y)
        {
            X = x;
            Y = y;
        }
    };

    public static class ext_Vector
    {
        // C# 7.3 has very crippled overloading compared to C++ here.  Operator syntax disallowed; cannot use generics due to no pattern-matching to require a specific function be defined.
        public static Vector2D<int> Add(this Vector2D<int> lhs, Vector2D<int> rhs) { return new Vector2D<int>(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        public static Vector2D<short> Add(this Vector2D<short> lhs, Vector2D<short> rhs) { return new Vector2D<short>((short)(lhs.X + rhs.X), (short)(lhs.Y + rhs.Y)); }
        public static Vector2D<sbyte> Add(this Vector2D<sbyte> lhs, Vector2D<sbyte> rhs) { return new Vector2D<sbyte>((sbyte)(lhs.X + rhs.X), (sbyte)(lhs.Y + rhs.Y)); }
    };
#endif

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

    public Vector2D_int? FirstOrDefault(Predicate<Vector2D_int> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
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
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      return  null != FirstOrDefault(testFn);
    }

    public void DoForEach(Action<Vector2D_int> doFn, Predicate<Vector2D_int> testFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      Vector2D_int point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        for (point.Y = Top; point.Y < Bottom; ++point.Y) {
          if (testFn(point)) doFn(point);
        }
      }
    }

    public void DoForEach(Action<Vector2D_int> doFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
#endif
      Vector2D_int point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        for (point.Y = Top; point.Y < Bottom; ++point.Y) {
          doFn(point);
        }
      }
    }

    public void DoForEachOnEdge(Action<Vector2D_int> doFn, Predicate<Vector2D_int> testFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      var point = new Vector2D_int();
      for (point.X = Left; point.X < Right; ++point.X) {
        point.Y = Top;
        if (testFn(point)) doFn(point);
        point.Y = Bottom-1;
        if (testFn(point)) doFn(point);
      }
      if (2 >= Height) return;
      for (point.Y = Top+1; point.Y < Bottom-2; ++point.Y) {
        point.X = Left;
        if (testFn(point)) doFn(point);
        point.X = Right-1;
        if (testFn(point)) doFn(point);
      }
    }

    public List<Vector2D_int> Where(Predicate<Vector2D_int> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      List<Vector2D_int> ret = new List<Vector2D_int>();
      DoForEach(pt => ret.Add(pt),testFn);
      return ret;
    }

    public List<Vector2D_int> WhereOnEdge(Predicate<Vector2D_int> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      var ret = new List<Vector2D_int>();
      DoForEachOnEdge(pt => ret.Add(pt),testFn);
      return ret;
    }

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
}
