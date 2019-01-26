using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool Equals(object obj)
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

        public bool Equals(object obj)
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

        public bool Equals(object obj) => throw new NotSupportedException();
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

        public bool Equals(object obj) => throw new NotSupportedException();
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
}
