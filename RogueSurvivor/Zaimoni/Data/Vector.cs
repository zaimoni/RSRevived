using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

/*
 * We do not use C# generics here because they compile-error due to being defective relative to C++ templates.  We would need to
 * be able to whitelist according to arbitrary member functions/operators (C++ compile-errors when the required interface is missing,
 * but C# requires an explicit whitelisting at least through 8.0)
 */
/*
 * C# 11, 2022-11-23: above historical faults are still materially correct, but they were blocking the use of C# 
 * for heavy numerical work.  Interfaces needed for the whitelisting strategy were introduced, by extending 
 * the language syntax (static abstract interface member functions).
 */

namespace Zaimoni.Data
{
    [Serializable]
    public record struct Vector2D<T> : Fn_to_s,
        IAdditionOperators<Vector2D<T>, Vector2D<T>, Vector2D<T>>,
        IAdditiveIdentity<Vector2D<T>, Vector2D<T>>,
        IDivisionOperators<Vector2D<T>, T, Vector2D<T>>,
        IMinMaxValue<Vector2D<T>>,
        IMultiplyOperators<Vector2D<T>, T, Vector2D<T>>,
        ISubtractionOperators<Vector2D<T>, Vector2D<T>, Vector2D<T>>,
        IUnaryNegationOperators<Vector2D<T>, Vector2D<T>>
        where T :INumberBase<T>,
                IMinMaxValue<T>,
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

        public static Vector2D<T> operator -(Vector2D<T> src)
        {
            src.X = -src.X;
            src.Y = -src.Y;
            return src;
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

    // readonly version of above would be Vector2D_r, but this seems to be a dead type currently
    // readonly ref version of above would be Vector2D_stack_r, but this seems to be a dead type currently

    public ref struct Vector2D_stack<T>
        where T : INumberBase<T>,
                  IMinMaxValue<T>,
                  IConvertible
    {
        public T X;
        public T Y;

        public Vector2D_stack(T x, T y)
        {
            X = x;
            Y = y;
        }

        static public explicit operator Vector2D_stack<T>(T x) => new(x, x);

        public static Vector2D_stack<T> MaxValue { get { return new Vector2D_stack<T>(T.MaxValue, T.MaxValue); } }
        public static Vector2D_stack<T> MinValue { get { return new Vector2D_stack<T>(T.MinValue, T.MinValue); } }
        public static Vector2D_stack<T> AdditiveIdentity { get { return new Vector2D_stack<T>(T.Zero, T.Zero); } }

        public static Vector2D_stack<T> operator checked +(Vector2D_stack<T> lhs, Vector2D_stack<T> rhs)
        {
            checked
            {
                lhs.X += rhs.X;
                lhs.Y += rhs.Y;
            }
            return lhs;
        }

        public static Vector2D_stack<T> operator +(Vector2D_stack<T> lhs, Vector2D_stack<T> rhs)
        {
            lhs.X += rhs.X;
            lhs.Y += rhs.Y;
            return lhs;
        }

        public static Vector2D_stack<T> operator checked -(Vector2D_stack<T> lhs, Vector2D_stack<T> rhs)
        {
            checked {
                lhs.X -= rhs.X;
                lhs.Y -= rhs.Y;
            }
            return lhs;
        }

        public static Vector2D_stack<T> operator -(Vector2D_stack<T> lhs, Vector2D_stack<T> rhs)
        {
            lhs.X -= rhs.X;
            lhs.Y -= rhs.Y;
            return lhs;
        }

        public static Vector2D_stack<T> operator checked *(Vector2D_stack<T> lhs, T rhs)
        {
            checked {
                lhs.X *= rhs;
                lhs.Y *= rhs;
            }
            return lhs;
        }

        public static Vector2D_stack<T> operator *(Vector2D_stack<T> lhs, T rhs)
        {
            lhs.X *= rhs;
            lhs.Y *= rhs;
            return lhs;
        }

        public static Vector2D_stack<T> operator checked /(Vector2D_stack<T> lhs, T rhs)
        {
            checked {
                lhs.X /= rhs;
                lhs.Y /= rhs;
            }
            return lhs;
        }

        public static Vector2D_stack<T> operator /(Vector2D_stack<T> lhs, T rhs)
        {
            lhs.X /= rhs;
            lhs.Y /= rhs;
            return lhs;
        }

        // these two are reasonable, but don't have viable C# interface
        public static Vector2D_stack<T> operator checked *(T lhs, Vector2D_stack<T> rhs)
        {
            checked {
                rhs.X *= lhs;
                rhs.Y *= lhs;
            }
            return rhs;
        }

        public static Vector2D_stack<T> operator *(T lhs, Vector2D_stack<T> rhs)
        {
            rhs.X *= lhs;
            rhs.Y *= lhs;
            return rhs;
        }

        public Vector2D_stack<T> coord_xform(Func<T, T> op) { return new Vector2D_stack<T>(op(X), op(Y)); }

        public string to_s() => "(" + X.ToString() + "," + Y.ToString() + ")";

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
    }

    [Serializable]
    public record struct Box2D<T> : Fn_to_s
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

        public Box2D(T originx, T originy, T sizex, T sizey)
        {
            _anchor = new Vector2D<T>(originx, originy);
            _dim = new Vector2D<T>(sizex, sizey);
        }

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
        private bool ContainsY(T origin) => 0 >= _anchor.Y.CompareTo(origin) && 0 > origin.CompareTo(Bottom);
        public bool Contains(T x, T y) => ContainsX(x) && ContainsY(y);
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
        public string to_s() => "[" + _anchor.to_s() + "," + _dim.to_s() + "]";

        public override int GetHashCode() => _anchor.GetHashCode() ^ _dim.GetHashCode();
    }

    public static class ext_Vector {
        public static T To<U, T>(this U src) where U : IConvertible
        {
            var method = typeof(IConvertible).GetMethod("ToInt16");
            if (typeof(T) == method.ReturnType) return (T)method.Invoke(src, new object[1]);
            throw new InvalidOperationException("unimplemented");
        }

        static public Box2D<short> FromLTRB_short<U>(U left, U top, U right, U bottom) where U:IConvertible, ISubtractionOperators<U,U,U>
        {
            return new Box2D<short>(left.To<U,short>(), top.To<U, short>(), (right - left).To<U, short>(), (bottom - top).To<U, short>());
        }

        public static void Normalize(ref this Vector2D<float> src) {
            // \todo? try to be clever about floating point overflow here (likely not needed)
            float EuclideanNorm = (float)Math.Sqrt((double)(src.X * src.X + src.Y * src.Y));
            if (0.0 < EuclideanNorm) {
                src /= EuclideanNorm;
            } else {
                src = Vector2D<float>.Empty;
            }
        }

        public static bool Hull(this IEnumerable<Vector2D<short>> src, ref Span<Vector2D<short>> hull)
        {
            hull[0] = Vector2D<short>.MaxValue;
            hull[1] = Vector2D<short>.MinValue;
            short tmp;
            foreach(var pt in src) {
                if ((tmp = pt.X) < hull[0].X) hull[0].X = tmp;
                if (tmp > hull[1].X) hull[1].X = tmp;
                if ((tmp = pt.Y) < hull[0].Y) hull[0].Y = tmp;
                if (tmp > hull[1].Y) hull[1].Y = tmp;
            }
            if (hull[0].X <= hull[1].X) {
                hull[1] += (Vector2D<short>)1;
                return true;
            }
            return false;
        }
    }
}

// dogfood our own savefile infrastructure

namespace Zaimoni.Serialization
{
    public partial interface ISave
    {
#region 7bit support
        static void Serialize7bit(Stream dest, in Data.Vector2D<short> src)
        {
            Serialize7bit(dest, src.X);
            Serialize7bit(dest, src.Y);
        }
        static void Deserialize7bit(Stream dest, ref Data.Vector2D<short> src)
        {
            Deserialize7bit(dest, ref src.X);
            Deserialize7bit(dest, ref src.Y);
        }

        static void Serialize7bit(Stream dest, in Data.Vector2D<ushort> src)
        {
            Serialize7bit(dest, src.X);
            Serialize7bit(dest, src.Y);
        }
        static void Deserialize7bit(Stream dest, ref Data.Vector2D<ushort> src)
        {
            Deserialize7bit(dest, ref src.X);
            Deserialize7bit(dest, ref src.Y);
        }

        static void Serialize7bit(Stream dest, in Data.Box2D<short> src)
        {
            Serialize7bit(dest, src.Location);
            Serialize7bit(dest, src.Size);
        }
        static void Deserialize7bit(Stream dest, ref Data.Box2D<short> src)
        {
            Data.Vector2D<short> stage = default;
            Deserialize7bit(dest, ref stage);
            src.Location = stage;
            Deserialize7bit(dest, ref stage);
            src.Size = stage;
        }
#endregion
    }
}