using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using System;
using System.Collections.Generic;
using System.Numerics;

/*
 * We do not use C# generics here because they compile-error due to being defective relative to C++ templates.  We would need to
 * be able to whitelist according to arbitrary member functions/operators (C++ compile-errors when the required interface is missing,
 * but C# requires an explicit whitelisting at least through 8.0)
 */

namespace Zaimoni.Data
{
    // C naming conventions
    [Serializable]
    public readonly struct Vector2D_int_r : IComparable<Vector2D_int_r>, IEquatable<Vector2D_int_r>, Fn_to_s
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
    public struct Vector2D_short : IComparable<Vector2D_short>, IEquatable<Vector2D_short>, Fn_to_s
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
        static public Vector2D_short operator *(short lhs, Vector2D_short rhs) { return new Vector2D_short((short)(lhs * rhs.X), (short)(lhs * rhs.Y)); }
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
    public readonly struct Vector2D_short_r : IComparable<Vector2D_short_r>, IEquatable<Vector2D_short_r>, Fn_to_s
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
            return null != FirstOrDefault(testFn);
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

        public void DoForEach<T>(Action<T> doFn, Func<Vector2D_short, T?> testFn) where T : class
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
                poshort.Y = (short)(Bottom - 1);
                doFn(poshort);
            }
            if (2 >= Height) return;
            for (poshort.Y = (short)(Top + 1); poshort.Y < Bottom - 1; ++poshort.Y) {
                poshort.X = Left;
                doFn(poshort);
                poshort.X = (short)(Right - 1);
                doFn(poshort);
            }
        }

        public void DoForEachOnEdge(Action<Vector2D_short> doFn, Predicate<Vector2D_short> testFn)
        {
            var poshort = new Vector2D_short();
            for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
                poshort.Y = Top;
                if (testFn(poshort)) doFn(poshort);
                poshort.Y = (short)(Bottom - 1);
                if (testFn(poshort)) doFn(poshort);
            }
            if (2 >= Height) return;
            for (poshort.Y = (short)(Top + 1); poshort.Y < Bottom - 1; ++poshort.Y) {
                poshort.X = Left;
                if (testFn(poshort)) doFn(poshort);
                poshort.X = (short)(Right - 1);
                if (testFn(poshort)) doFn(poshort);
            }
        }

        public List<Vector2D_short> Where(Predicate<Vector2D_short> testFn)
        {
            List<Vector2D_short> ret = new List<Vector2D_short>();
            DoForEach(pt => ret.Add(pt), testFn);
            return ret;
        }

        public List<Vector2D_short> WhereOnEdge(Predicate<Vector2D_short> testFn)
        {
            var ret = new List<Vector2D_short>();
            DoForEachOnEdge(pt => ret.Add(pt), testFn);
            return ret;
        }

        public void WhereOnEdge(ref Stack<Vector2D_short> dest, Predicate<Vector2D_short> testFn)
        {
            var poshort = new Vector2D_short();   // inline DoForEachOnEdge
            for (poshort.X = Left; poshort.X < Right; ++poshort.X) {
                poshort.Y = Top;
                if (testFn(poshort)) dest.push(ref poshort);
                poshort.Y = (short)(Bottom - 1);
                if (testFn(poshort)) dest.push(ref poshort);
            }
            if (2 >= Height) return;
            for (poshort.Y = (short)(Top + 1); poshort.Y < Bottom - 2; ++poshort.Y) {
                poshort.X = Left;
                if (testFn(poshort)) dest.push(ref poshort);
                poshort.X = (short)(Right - 1);
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
            return _anchor.GetHashCode() ^ _dim.GetHashCode();
        }
    }

    [Serializable]
    public readonly struct Vector2D_long_r : IComparable<Vector2D_long_r>, IEquatable<Vector2D_long_r>, Fn_to_s
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
    public record struct Vector2D<T> : Fn_to_s,
        System.Numerics.IAdditionOperators<Vector2D<T>, Vector2D<T>, Vector2D<T>>,
        System.Numerics.IAdditiveIdentity<Vector2D<T>, Vector2D<T>>,
        System.Numerics.IDivisionOperators<Vector2D<T>, T, Vector2D<T>>,
        System.Numerics.IMinMaxValue<Vector2D<T>>,
        System.Numerics.IMultiplyOperators<Vector2D<T>, T, Vector2D<T>>,
        System.Numerics.ISubtractionOperators<Vector2D<T>, Vector2D<T>, Vector2D<T>>
        where T:System.Numerics.INumberBase<T>,
                System.Numerics.IMinMaxValue<T>,
                IConvertible
    {
        public T X;
        public T Y;

        public Vector2D(T x, T y)
        {
            X = x;
            Y = y;
        }

        static public explicit operator Vector2D<T>(T x) => new(x, x);
        static readonly public Vector2D<T> Empty = new Vector2D<T>(T.Zero, T.Zero);
        public bool IsEmpty { get { return Empty == this; } }

        static private readonly Vector2D<T> _maxValue = new Vector2D<T>(T.MaxValue, T.MaxValue);
        static private readonly Vector2D<T> _minValue = new Vector2D<T>(T.MinValue, T.MinValue);

        public static Vector2D<T> MaxValue { get { return _maxValue; } }
        public static Vector2D<T> MinValue { get { return _minValue; } }

        public static Vector2D<T> AdditiveIdentity { get { return Empty; } }

        public static Vector2D<T> operator checked +(Vector2D<T> lhs, Vector2D<T> rhs)
        {
            checked {
                lhs.X += rhs.X;
                lhs.Y += rhs.Y;
            }
            return lhs;
        }

        public static Vector2D<T> operator +(Vector2D<T> lhs, Vector2D<T> rhs) {
            lhs.X += rhs.X;
            lhs.Y += rhs.Y;
            return lhs;
        }

        public static Vector2D<T> operator checked -(Vector2D<T> lhs, Vector2D<T> rhs)
        {
            checked {
                lhs.X -= rhs.X;
                lhs.Y -= rhs.Y;
            }
            return lhs;
        }

        public static Vector2D<T> operator -(Vector2D<T> lhs, Vector2D<T> rhs)
        {
            lhs.X -= rhs.X;
            lhs.Y -= rhs.Y;
            return lhs;
        }

        public static Vector2D<T> operator checked *(Vector2D<T> lhs, T rhs)
        {
            checked {
                lhs.X *= rhs;
                lhs.Y *= rhs;
            }
            return lhs;
        }

        public static Vector2D<T> operator *(Vector2D<T> lhs, T rhs)
        {
            lhs.X *= rhs;
            lhs.Y *= rhs;
            return lhs;
        }

        public static Vector2D<T> operator checked /(Vector2D<T> lhs, T rhs)
        {
            checked {
                lhs.X /= rhs;
                lhs.Y /= rhs;
            }
            return lhs;
        }

        public static Vector2D<T> operator /(Vector2D<T> lhs, T rhs)
        {
            lhs.X /= rhs;
            lhs.Y /= rhs;
            return lhs;
        }

        // these two are reasonable, but don't have viable C# interface
        public static Vector2D<T> operator checked *(T lhs, Vector2D<T> rhs)
        {
            checked {
                rhs.X *= lhs;
                rhs.Y *= lhs;
            }
            return rhs;
        }

        public static Vector2D<T> operator *(T lhs, Vector2D<T> rhs)
        {
            rhs.X *= lhs;
            rhs.Y *= lhs;
            return rhs;
        }

        public Vector2D<T> coord_xform(Func<T, T> op) { return new Vector2D<T>(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        public override int GetHashCode()
        {
            return unchecked(1 + 19 * X.ToInt32(default) + 19 * 17 * Y.ToInt32(default));
        }

    }

    public record struct Box2D<T>
        where T:IConvertible,
                IComparable<T>,
                System.Numerics.INumberBase<T>,
                System.Numerics.IMinMaxValue<T>
    {
        private Vector2D<T> _anchor;
        private Vector2D<T> _dim;

        public Box2D(Vector2D<T> origin, Vector2D<T> size)
        {
            _anchor = origin;
            _dim = size;
        }

        public Box2D(Vector2D<T> origin, T sizex, T sizey)
        {
            _anchor = origin;
            _dim = new(sizex, sizey);
        }

#if PROTOTYPE
        public Box2D_short(int originx, int originy, int sizex, int sizey)
        {
            _anchor = new Vector2D_short(originx, originy);
            _dim = new Vector2D_short(sizex, sizey);
        }

        static public Box2D_short FromLTRB(int left, int top, int right, int bottom) { return new Box2D_short(left, top, right - left, bottom - top); }
#endif

        static readonly public Box2D<T> Empty = new(Vector2D<T>.Empty, Vector2D<T>.Empty);
        public bool IsEmpty { get { return Empty == this; } }

#region pure getters
        public T Bottom { get { return _anchor.Y + _dim.Y; } }
        public T Left { get { return _anchor.X; } }
        public T Right { get { return _anchor.X + _dim.X; } }
        public T Top { get { return _anchor.Y; } }
#endregion

#region get/set pairs
        public T Height
        {
            get { return _dim.Y; }
            set { _dim.Y = value; }
        }

        public T Width
        {
            get { return _dim.X; }
            set { _dim.X = value; }
        }

        public Vector2D<T> Size
        {
            get { return _dim; }
            set { _dim = value; }
        }

        public Vector2D<T> Location
        {
            get { return _anchor; }
            set { _anchor = value; }
        }

        public T X
        {
            get { return _anchor.X; }
            set { _anchor.X = value; }
        }

        public T Y
        {
            get { return _anchor.Y; }
            set { _anchor.Y = value; }
        }
#endregion

        static public Box2D<T> Union(Box2D<T> lhs, Box2D<T> rhs)
        {
            Box2D<T> ret = lhs;
            if (0 < ret.X.CompareTo(rhs.X)) {
                ret.Width += ret.X - rhs.X;
                ret.X = rhs.X;
            }
            if (0 < ret.Y.CompareTo(rhs.Y)) {
                ret.Height += ret.Y - rhs.Y;
                ret.Y = rhs.Y;
            }
            if (0 > ret.Right.CompareTo(rhs.Right)) ret.Width += rhs.Right - ret.Right;
            if (0 > ret.Bottom.CompareTo(rhs.Bottom)) ret.Height += rhs.Bottom - ret.Bottom;
            return ret;
        }

        public Box2D<T> Intersect(Box2D<T> rhs)
        {
            Box2D<T> ret = this;
            if (0 > ret.X.CompareTo(rhs.X)) {
                ret.Width -= rhs.X - ret.X;
                ret.X = rhs.X;
            }
            if (0 > ret.Y.CompareTo(rhs.Y))
            {
                ret.Height -= rhs.Y - ret.Y;
                ret.Y = rhs.Y;
            }
            if (0 < ret.Right.CompareTo(rhs.Right)) ret.Width -= ret.Right - rhs.Right;
            if (0 < ret.Bottom.CompareTo(rhs.Bottom)) ret.Height -= ret.Bottom - rhs.Bottom;
            return ret;
        }

        // these are not the safest implementations for integer math
        private bool ContainsX(T origin) => 0 >= _anchor.X.CompareTo(origin) && 0 > origin.CompareTo(Right);
        private bool ContainsY(T origin) => 0 >= _anchor.X.CompareTo(origin) && 0 > origin.CompareTo(Bottom);
        public bool Contains(T x, T y) => ContainsX(x) && ContainsY(Y);
        public bool Contains(Vector2D<T> src) => ContainsX(src.X) && ContainsY(src.Y);

        public bool Contains(Box2D<T> src) {
            return 0 >= Left.CompareTo(src.Left) && 0 >= src.Right.CompareTo(Right) && 0>= Top.CompareTo(src.Top)
                && 0 >= src.Bottom.CompareTo(Bottom);
        }

        // closely related to compass rose ordering
        public int EdgeCode(Vector2D<T> src)
        {
            int ret = 0;
            if (_anchor.X == src.X) ret += 8;
            if (Right - T.One == src.X) ret += 2;
            if (_anchor.Y == src.Y) ret += 1;
            if (Bottom - T.One == src.Y) ret += 4;
            return ret;
        }

#nullable enable
        public Vector2D<T>? FirstOrDefault(Predicate<Vector2D<T>> testFn)
        {
            Vector2D<T> pt = new();
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X) {
                for (pt.Y = Top; 0 > pt.Y.CompareTo(Bottom); ++pt.Y) {
                    if (testFn(pt)) return pt;
                }
            }
            return null;
        }

        public bool Any(Predicate<Vector2D<T>> testFn) => null != FirstOrDefault(testFn);

        public void DoForEach(Action<Vector2D<T>> doFn, Predicate<Vector2D<T>> testFn)
        {
            Vector2D<T> pt = new();
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X) {
                for (pt.Y = Top; 0 > pt.Y.CompareTo(Bottom); ++pt.Y) {
                    if (testFn(pt)) doFn(pt);
                }
            }
        }

        public void DoForEach<U>(Action<U> doFn, Func<Vector2D<T>, U?> testFn) where U : class
        {
            Vector2D<T> pt = new();
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X) {
                for (pt.Y = Top; 0 > pt.Y.CompareTo(Bottom); ++pt.Y) {
                    var test = testFn(pt);
                    if (null != test) doFn(test);
                }
            }
        }

        public void DoForEach(Action<Vector2D<T>> doFn)
        {
            Vector2D<T> pt = new();
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X) {
                for (pt.Y = Top; 0 > pt.Y.CompareTo(Bottom); ++pt.Y) {
                    doFn(pt);
                }
            }
        }

        public void DoForEachOnEdge(Action<Vector2D<T>> doFn)
        {
            Vector2D<T> pt = new();
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X) {
                pt.Y = Top;
                doFn(pt);
                pt.Y = Bottom - T.One;
                doFn(pt);
            }
            if (0 <= (T.One+T.One).CompareTo(Height)) return;
            for (pt.Y = Top + T.One; 0 > pt.Y.CompareTo(Bottom - T.One); ++pt.Y) {
                pt.X = Left;
                doFn(pt);
                pt.X = Right - T.One;
                doFn(pt);
            }
        }

        public void DoForEachOnEdge(Action<Vector2D<T>> doFn, Predicate<Vector2D<T>> testFn)
        {
            Vector2D<T> pt = new();
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X) {
                pt.Y = Top;
                if (testFn(pt)) doFn(pt);
                pt.Y = Bottom - T.One;
                if (testFn(pt)) doFn(pt);
            }
            if (0 <= (T.One + T.One).CompareTo(Height)) return;
            for (pt.Y = Top + T.One; 0 > pt.Y.CompareTo(Bottom - T.One); ++pt.Y) {
                pt.X = Left;
                if (testFn(pt)) doFn(pt);
                pt.X = Right - T.One;
                if (testFn(pt)) doFn(pt);
            }
        }

        public List<Vector2D<T>> Where(Predicate<Vector2D<T>> testFn)
        {
            List<Vector2D<T>> ret = new();
            DoForEach(pt => ret.Add(pt), testFn);
            return ret;
        }

        public List<Vector2D<T>> WhereOnEdge(Predicate<Vector2D<T>> testFn)
        {
            List<Vector2D<T>> ret = new();
            DoForEachOnEdge(pt => ret.Add(pt), testFn);
            return ret;
        }

        public void WhereOnEdge(ref Stack<Vector2D<T>> dest, Predicate<Vector2D<T>> testFn)
        {
            Vector2D<T> pt = new();   // inline DoForEachOnEdge
            for (pt.X = Left; 0 > pt.X.CompareTo(Right); ++pt.X)
            {
                pt.Y = Top;
                if (testFn(pt)) dest.push(ref pt);
                pt.Y = Bottom - T.One;
                if (testFn(pt)) dest.push(ref pt);
            }
            if (0 <= (T.One + T.One).CompareTo(Height)) return;
            for (pt.Y = Top + T.One; 0 > pt.Y.CompareTo(Bottom - (T.One + T.One)); ++pt.Y) {
                pt.X = Left;
                if (testFn(pt)) dest.push(ref pt);
                pt.X = Right - T.One;
                if (testFn(pt)) dest.push(ref pt);
            }
        }
#nullable restore

        public override int GetHashCode() => _anchor.GetHashCode() ^ _dim.GetHashCode();
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
