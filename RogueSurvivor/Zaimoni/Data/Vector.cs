using System;

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

#region pure getters
        int Bottom { get { return _anchor.Y + _dim.Y; } }
        int Left { get { return _anchor.X; } }
        int Right { get { return _anchor.X + _dim.X; } }
        int Top { get { return _anchor.Y; } }
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

        // these are not the safest implementations for integer math
        public bool Contains(Vector2D_int src) { return _anchor.X <= src.X && src.X < Right && _anchor.Y <= src.Y && src.Y < Bottom; }
        public bool Contains(Box2D_int src) { return Left <= src.Left && src.Right <= Right && Top <= src.Top && src.Bottom <= Bottom; }

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
