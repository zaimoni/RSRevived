using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
  public static class ext_Drawing
  {
    // 8r coordinates at grid distance r
    // 0..2r: y constant r, x increment -r to r
    // 2r...4r: x constant r, y decrement r to -r
    // 4r..64: y constant -r, x decrement r to -r
    // 4r to 8r i.e. 0: x constant -r, y increment -r to r
    public static Point RadarSweep(this Point origin,int radius,int i)
    {
#if DEBUG
      if (0 >= radius || int.MaxValue/8 < radius) throw new ArgumentOutOfRangeException(nameof(radius),radius,"must be in 1.."+(int.MaxValue / 8).ToString());
      if (int.MaxValue - radius < origin.X || int.MinValue + radius > origin.X) throw new ArgumentOutOfRangeException(nameof(origin.X),origin.X.ToString(), "must be in "+(int.MinValue + radius).ToString()+".." +(int.MaxValue - radius).ToString());
      if (int.MaxValue - radius < origin.Y || int.MinValue + radius > origin.Y) throw new ArgumentOutOfRangeException(nameof(origin.Y),origin.Y.ToString(), "must be in "+(int.MinValue + radius).ToString()+".." +(int.MaxValue - radius).ToString());
#endif
      // normalize i
      i %= 8*radius;
      if (0>i) i+=8*radius;

      // parentheses are to deny compiler the option to reorder to overflow
      if (2*radius>i) return new Point((i-radius)+origin.X,radius + origin.Y);
      if (4*radius>i) return new Point(radius + origin.X, (3*radius- i) + origin.Y);
      if (6*radius>i) return new Point((5*radius-i) + origin.X, -radius + origin.Y);
      /* if (8*radius>i) */ return new Point(-radius + origin.X, (i-7* radius) + origin.Y);
    }

    public static Vector2D_int RadarSweep(this Vector2D_int origin,int radius,int i)
    {
#if DEBUG
      if (0 >= radius || int.MaxValue/8 < radius) throw new ArgumentOutOfRangeException(nameof(radius),radius,"must be in 1.."+(int.MaxValue / 8).ToString());
      if (int.MaxValue - radius < origin.X || int.MinValue + radius > origin.X) throw new ArgumentOutOfRangeException(nameof(origin.X),origin.X.ToString(), "must be in "+(int.MinValue + radius).ToString()+".." +(int.MaxValue - radius).ToString());
      if (int.MaxValue - radius < origin.Y || int.MinValue + radius > origin.Y) throw new ArgumentOutOfRangeException(nameof(origin.Y),origin.Y.ToString(), "must be in "+(int.MinValue + radius).ToString()+".." +(int.MaxValue - radius).ToString());
#endif
      // normalize i
      i %= 8*radius;
      if (0>i) i+=8*radius;

      // parentheses are to deny compiler the option to reorder to overflow
      if (2*radius>i) return new Vector2D_int((i-radius)+origin.X,radius + origin.Y);
      if (4*radius>i) return new Vector2D_int(radius + origin.X, (3*radius- i) + origin.Y);
      if (6*radius>i) return new Vector2D_int((5*radius-i) + origin.X, -radius + origin.Y);
      /* if (8*radius>i) */ return new Vector2D_int(-radius + origin.X, (i-7* radius) + origin.Y);
    }

    public static Vector2D_short RadarSweep(this Vector2D_short origin,short radius,int i)
    {
#if DEBUG
      if (0 >= radius || int.MaxValue/8 < radius) throw new ArgumentOutOfRangeException(nameof(radius),radius,"must be in 1.."+(int.MaxValue / 8).ToString());
      if (short.MaxValue - radius < origin.X || short.MinValue + radius > origin.X) throw new ArgumentOutOfRangeException(nameof(origin.X),origin.X.ToString(), "must be in "+(short.MinValue + radius).ToString()+".." +(short.MaxValue - radius).ToString());
      if (short.MaxValue - radius < origin.Y || short.MinValue + radius > origin.Y) throw new ArgumentOutOfRangeException(nameof(origin.Y),origin.Y.ToString(), "must be in "+(short.MinValue + radius).ToString()+".." +(short.MaxValue - radius).ToString());
#endif
      // normalize i
      i %= 8*radius;
      if (0>i) i+=8*radius;

      // parentheses are to deny compiler the option to reorder to overflow
      if (2*radius>i) return new Vector2D_short((short)(i-radius),radius) + origin;
      if (4*radius>i) return new Vector2D_short(radius, (short)(3*radius- i)) + origin;
      if (6*radius>i) return new Vector2D_short((short)(5*radius-i), (short)(-radius)) + origin;
      /* if (8*radius>i) */ return new Vector2D_short((short)(-radius), (short)(i-7* radius)) + origin;
    }

    // null testFn is a different signature for efficiency reasons
    public static void DoForEach(this Rectangle rect, Action<Point> doFn, Predicate<Point> testFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          if (testFn(point)) doFn(point);
        }
      }
    }

    public static void DoForEach(this Rectangle rect, Action<Point> doFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          doFn(point);
        }
      }
    }

    public static void DoForEachOnEdge(this Rectangle rect, Action<Point> doFn, Predicate<Point> testFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        point.Y = rect.Top;
        if (testFn(point)) doFn(point);
        point.Y = rect.Bottom-1;
        if (testFn(point)) doFn(point);
      }
      if (2 >= rect.Height) return;
      for (point.Y = rect.Top+1; point.Y < rect.Bottom-2; ++point.Y) {
        point.X = rect.Left;
        if (testFn(point)) doFn(point);
        point.X = rect.Right-1;
        if (testFn(point)) doFn(point);
      }
    }

    // imitate Enumerable interface here
    public static List<Point> Where(this Rectangle rect, Predicate<Point> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      List<Point> ret = new List<Point>();
      rect.DoForEach(pt => ret.Add(pt),testFn);
      return ret;
    }

    public static List<Point> WhereOnEdge(this Rectangle rect, Predicate<Point> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      List<Point> ret = new List<Point>();
      rect.DoForEachOnEdge(pt => ret.Add(pt),testFn);
      return ret;
    }

    public static Point? FirstOrDefault(this Rectangle rect, Predicate<Point> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          if (testFn(point)) return point;
        }
      }
      return null;
    }

    public static bool Any(this Rectangle rect, Predicate<Point> testFn)
    {
#if DEBUG
      if (null == testFn) throw new ArgumentNullException(nameof(testFn));
#endif
      return  null != rect.FirstOrDefault(testFn);
    }

    // morally a dual-Any test that only iterates once.
    public static KeyValuePair<bool,bool> HaveItBothWays<T>(this IEnumerable<T> src, Predicate<T> test)
    {
      if (null==src || !src.Any()) return new KeyValuePair<bool, bool>(false,false);
      bool yes = false;
      bool no = false;
      foreach(var x in src) {
        if (test(x)) {
          yes = true;
          if (no) return new KeyValuePair<bool, bool>(true, true);
        } else {
          no = true;
          if (yes) return new KeyValuePair<bool, bool>(true, true);
        }
      }
      return new KeyValuePair<bool, bool>(yes, no);
    }

    // Angband-style rarity table support
    public static T UseRarityTable<T>(this IEnumerable<KeyValuePair<T,int>> src, int r)
    {
      foreach(var x in src) {
        if (x.Value > r) return x.Key;
        r -= x.Value;
      }
      throw new InvalidProgramException("should not reach end of UseRarityTable");
    }

    // unpacking delta codes for < = >
    public static Vector2D_short_stack sgn_from_delta_code(ref int delta_code)
    {
      if (0==delta_code) return new Vector2D_short_stack(0,0);
      int scale = 3;
      int threshold = scale/2;
      int index = 0;
      // XXX lack of error checking even in debug mode
      if (0<delta_code) {
        while(threshold < delta_code) {
          if (threshold+scale >= delta_code) {
            delta_code -= scale;
            return new Vector2D_short_stack(1,index+1);
          }
          scale *= 3;
          threshold = scale/2;
          index++;
        }
        delta_code = 0;
        return new Vector2D_short_stack(1,0);
      } else {
        while(-threshold > delta_code) {
          if (-threshold-scale <= delta_code) {
            delta_code += scale;
            return new Vector2D_short_stack(-1,index+1);
          }
          scale *= 3;
          threshold = scale/2;
          index++;
        }
        delta_code = 0;
        return new Vector2D_short_stack(-1,0);
      }
    }

    // low-level bitmap manipulations.  Technically redundant due to System.Drawing.Graphics.
    public static Bitmap MonochromeRectangle(Color tint, int w, int h)
    {
      Bitmap img = new Bitmap(w,h);
      for (int x = 0; x < w; ++x) {
        for (int y = 0; y < h; ++y) {
          img.SetPixel(x,y,tint);
        }
      }
      return img;
    }

    // y and x1 aware of negative offsets being from the other end
    // half-open interval [x0,x1[
    public static void HLine(this Bitmap img, Color tint, int y, int x0, int x1)
    {
      if (img.Height < y) return;
      if (0 > y) {
        y += img.Height;
        if (0 > y) return;
      }
      if (0 > x1) {
        x1 += img.Width+1;
        if (0 > x1) return;
      } else if (img.Width < x1) x1 = img.Width;
      if (0 > x0) x0 = 0;
      while(x0<x1) {
        img.SetPixel(x0++, y, tint);
      }
    }

    // x and y1 aware of negative offsets being from the other end
    // half-open interval [y0,y1[
    public static void VLine(this Bitmap img, Color tint, int x, int y0, int y1)
    {
      if (img.Width < x) return;
      if (0 > x) {
        x += img.Width;
        if (0 > x) return;
      }
      if (0 > y1) {
        y1 += img.Height+1;
        if (0 > y1) return;
      } else if (img.Height < y1) y1 = img.Height;
      if (0 > y0) y0 = 0;
      while(y0 < y1) {
        img.SetPixel(x, y0++, tint);
      }
    }

#if PROTOTYPE
    public static Bitmap SkewCopy(this Bitmap img, int w, int h, Func<Point,Point> transform)
    {
#if DEBUG
      if (null == transform) throw new ArgumentNullException(nameof(transform));
#endif
      Bitmap dest = new Bitmap(w,h);
      Point pt = new Point(0,0);
      for (pt.X = 0; pt.X < w; ++pt.X ) {
        for (pt.Y = 0; pt.Y < h; ++pt.Y) {
          Point src = transform(pt);
          if (0 > src.X || img.Width  <= src.X) continue;
          if (0 > src.Y || img.Height <= src.Y) continue;
          dest.SetPixel(pt.X,pt.Y,img.GetPixel(src.X,src.Y));
        }
      }
      return dest;
    }
#endif

    public static Bitmap Splice(this Bitmap src, Bitmap splice, Func<Point,bool> use_src)
    {
#if DEBUG
      if (null == use_src) throw new ArgumentNullException(nameof(use_src));
      if (null == src) throw new ArgumentNullException(nameof(src));
      if (null == splice) throw new ArgumentNullException(nameof(splice));
      if (src.Width!=splice.Width) throw new InvalidOperationException("src.Width!=splice.Width");
      if (src.Height!=splice.Height) throw new InvalidOperationException("src.Width!=splice.Width");
#endif
      int w = src.Width;
      int h = src.Height;
      Bitmap dest = new Bitmap(w,h);
      Point pt = new Point(0,0);
      for (pt.X = 0; pt.X < w; ++pt.X ) {
        for (pt.Y = 0; pt.Y < h; ++pt.Y) {
          dest.SetPixel(pt.X,pt.Y,(use_src(pt) ? src : splice).GetPixel(pt.X,pt.Y));
        }
      }
      return dest;
    }

    // Following might actually be redundant due to System.Linq, but a dictionary i.e. associative array really is two sequences (keys and values)
    public static void OnlyIf<Key,Value>(this Dictionary<Key,Value> src,Predicate<Value> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      var reject = new List<Key>(src.Count);
      foreach(var x in src) if (!fn(x.Value)) reject.Add(x.Key);
      foreach(var x in reject) src.Remove(x);
    }

    public static void OnlyIf<Key,Value>(this Dictionary<Key,Value> src,Predicate<Key> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      var reject = new List<Key>(src.Count);
      foreach(var x in src) if (!fn(x.Key)) reject.Add(x.Key);
      foreach(var x in reject) src.Remove(x);
    }

    public static void OnlyIfMinimal<Key,Value>(this Dictionary<Key,Value> src) where Value:IComparable<Value>
    {
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      if (1 >= src.Count) return;
      var reject = new List<Key>();
      var accept = new List<Key>();
      Value num1 = (Value)typeof(Value).GetField("MaxValue").GetValue(default(Value));
      foreach(var x in src) {
        int comp = num1.CompareTo(x.Value);
        if (0>comp) {
            reject.Add(x.Key);
            continue;
        }
        if (0<comp) {
            reject.AddRange(accept);
            accept.Clear();
            num1 = x.Value;
        }
        accept.Add(x.Key);
      }
      foreach(var x in reject) src.Remove(x);
    }

    public static void OnlyIfMaximal<Key,Value>(this Dictionary<Key,Value> src) where Value:IComparable<Value>
    {
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      if (1 >= src.Count) return;
      var reject = new List<Key>();
      var accept = new List<Key>();
      Value num1 = (Value)typeof(Value).GetField("MinValue").GetValue(default(Value));
      foreach(var x in src) {
        int comp = num1.CompareTo(x.Value);
        if (0<comp) {
            reject.Add(x.Key);
            continue;
        }
        if (0>comp) {
            reject.AddRange(accept);
            accept.Clear();
            num1 = x.Value;
        }
        accept.Add(x.Key);
      }
      foreach(var x in reject) src.Remove(x);
    }

    public static T Minimize<T,R>(this IEnumerable<T> src,Func<T,R> metric) where R:IComparable
    {
#if DEBUG
      if (null == metric) throw new ArgumentNullException(nameof(metric));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      R num1 = (R)typeof(R).GetField("MaxValue").GetValue(default(R));
      T ret = default(T);
      foreach(T test in src) {
         R num2 = metric(test);
         if (0>num2.CompareTo(num1)) {
           ret = test;
           num1 = num2;
         }
      }
      return ret;
    }

    public static T Maximize<T,R>(this IEnumerable<T> src,Func<T,R> metric) where R:IComparable
    {
#if DEBUG
      if (null == metric) throw new ArgumentNullException(nameof(metric));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      R num1 = (R)typeof(R).GetField("MinValue").GetValue(default(R));
      T ret = default(T);
      foreach(T test in src) {
         R num2 = metric(test);
         if (0<num2.CompareTo(num1)) {
           ret = test;
           num1 = num2;
         }
      }
      return ret;
    }

    public static Dictionary<T,U> CloneOnlyMinimal<T,U,R>(this Dictionary<T, U> src,Func<U,R> metric) where R:IComparable
    {
#if DEBUG
      if (null == metric) throw new ArgumentNullException(nameof(metric));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      if (1 >= src.Count) return src;
      R num1 = (R)typeof(R).GetField("MaxValue").GetValue(default(R));
      var ret = new Dictionary<T, U>();
      foreach(var x in src) {
         R num2 = metric(x.Value);
         int comp = num2.CompareTo(num1);
         if (0 < comp) continue;
         if (0 > comp) {
           ret.Clear();
           num1 = num2;
         }
         ret.Add(x.Key,x.Value);
      }
      return ret;
    }

    public static Dictionary<T,U> CloneOnlyMinimal<T,U,R>(this Dictionary<T, U> src,Func<T,R> metric) where R:IComparable
    {
#if DEBUG
      if (null == metric) throw new ArgumentNullException(nameof(metric));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      if (1 >= src.Count) return src;
      R num1 = (R)typeof(R).GetField("MaxValue").GetValue(default(R));
      var ret = new Dictionary<T, U>();
      foreach(var x in src) {
         R num2 = metric(x.Key);
         int comp = num2.CompareTo(num1);
         if (0 < comp) continue;
         if (0 > comp) {
           ret.Clear();
           num1 = num2;
         }
         ret.Add(x.Key,x.Value);
      }
      return ret;
    }

    // generic loop iteration ... probably inefficient compared to inlining
    public static bool ActOnce<T>(this IEnumerable<T> src, Action<T> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
      if (!src.Any()) return false;
      fn(src.First());
      return true;;
    }

    public static bool ActOnce<T>(this IEnumerable<T> src, Action<T> fn, Func<T, bool> test)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == test) throw new ArgumentNullException(nameof(test));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      foreach (T x in src) {
        if (test(x)) {
          fn(x);
          return true;
        }
      }
      return false;
    }

    public static void DoForEach<T>(this IEnumerable<T> src, Action<T> fn, Func<T, bool> test)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == test) throw new ArgumentNullException(nameof(test));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      foreach (T x in src) {
        if (test(x)) fn(x);
      }
    }

    // for efficiency
    public static void DoForEach<T>(this IEnumerable<T> src, Action<T> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      foreach (T x in src) fn(x);
    }

    public static bool ValueEqual<T>(this HashSet<T> lhs, HashSet<T> rhs)
    {
      if (null == lhs) return null==rhs;
      if (null == rhs) return false;
      if (lhs.Count!=rhs.Count) return false;
      foreach(var x in lhs) if (!rhs.Contains(x)) return false;
      return true;
    }

    // logic operations
    public static Predicate<T> And<T>(this Predicate<T> lhs, Predicate<T> rhs) {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) return l;
      bool and(T src) { return l(src) && r(src); };
      return and;
    }

    public static Predicate<T> Or<T>(this Predicate<T> lhs, Predicate<T> rhs) {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) return l;
      bool or(T src) { return l(src) || r(src); };
      return or;
    }

    // C-ish.
    public static Func<T,int> Or<T>(this Func<T, int> lhs, Func<T, int> rhs) {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) return l;
      int or(T src) {
        var test = l(src);
        if (0 != test) return test;
        return r(src);
      };
      return or;
    }

    public static Func<T,bool> Or<T>(this Func<T, bool> lhs, Func<T, bool> rhs) {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) return l;
      bool or(T src) { return l(src) || r(src); };
      return or;
    }

    // set theory operations
    // we accept null as a shorthand for the function on domain T that returns constant empty set HashSet<U>
    public static Func<T, HashSet<U>> Union<T,U>(this Func<T, HashSet<U>> lhs, Func<T, HashSet<U>> rhs)
    {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) return l;
      HashSet<U> ret(T src) {
        var x = l(src);
        var y = r(src);
        if (null == x) return y;
        if (null != y) x.UnionWith(y);
        return x;
      }
      return ret;
    }

    // logic operation ... want to use rhs only if lhs has no targets
    public static Func<T, HashSet<U>> Otherwise<T,U>(this Func<T, HashSet<U>> lhs, Func<T, HashSet<U>> rhs)
    {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) return l;
      HashSet<U> ret(T src) {
        var x = l(src);
        if (null!=x && 0<x.Count) return x;
        return r(src);
      }
      return ret;
    }

    public static Func<T, HashSet<U>> Postfilter<T,U>(this Func<T, HashSet<U>> lhs, Action<T, HashSet<U>> rhs)
    {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return null;
      if (null == rhs) return l;
      HashSet<U> ret(T src) {
        var x = l(src);
        if (null != x && 0 < x.Count) r(src,x);
        return x;
      }
      return ret;
    }

    // function composition is right-associative in higher math
    // reverse parameter order here to make function call chaining clean
    public static Func<T,V> Compose<T,U,V>(this Func<T,U> rhs, Func<U, V> lhs)
    {
      if (null == lhs) throw new ArgumentNullException(nameof(lhs));
      if (null == rhs) throw new ArgumentNullException(nameof(rhs));
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      V ret(T src) {
        return l(r(src));
      }
      return ret;
    }

#if CTHORPE_DEFECTIVE_GENERICS
    public static Func<T,U> Compose<T,U>(this Func<T,T> rhs, Func<T, U> lhs)
    {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == rhs) return l;
      if (null == lhs) throw new ArgumentNullException(nameof(lhs));
      U ret(T src) {
        return l(r(src));
      }
      return ret;
    }

    public static Func<T,U> Compose<T,U>(this Func<T,U> rhs, Func<U, U> lhs)
    {
      var l = lhs;  // local copies needed to get true lambda calculus
      var r = rhs;
      if (null == lhs) return r;
      if (null == rhs) throw new ArgumentNullException(nameof(rhs));
      U ret(T src) {
        return l(r(src));
      }
      return ret;
    }
#endif

    // interpreting points to linear arrays
    public static void convert(this Rectangle rect, Point pt, ref int i)
    {
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (!rect.Contains(pt)) throw new InvalidOperationException("tried to encode point not in rectangle");
      i = (pt.X - rect.Left) + rect.Width*(pt.Y - rect.Top);
    }

    public static void convert(this Rectangle rect, int i, ref Point pt)
    {
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (0 > i) throw new InvalidOperationException("index not in rectangle");
      pt.X = i%rect.Width + rect.Left;
      pt.Y = i/rect.Width + rect.Top;
    }

    public static void convert(this Box2D_int rect, Vector2D_int pt, ref int i)
    {
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (!rect.Contains(pt)) throw new InvalidOperationException("tried to encode point not in rectangle");
      i = (pt.X - rect.Left) + rect.Width*(pt.Y - rect.Top);
    }

    public static void convert(this Box2D_int rect, int i, ref Vector2D_int pt)
    {
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (0 > i) throw new InvalidOperationException("index not in rectangle");
      pt.X = i%rect.Width + rect.Left;
      pt.Y = i/rect.Width + rect.Top;
    }

    public static void convert(this Box2D_short rect, Vector2D_short pt, ref int i)
    {
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (!rect.Contains(pt)) throw new InvalidOperationException("tried to encode point not in rectangle");
      i = (pt.X - rect.Left) + rect.Width*(pt.Y - rect.Top);
    }

    public static void convert(this Box2D_short rect, int i, ref Vector2D_short pt)
    {
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (0 > i) throw new InvalidOperationException("index not in rectangle");
      pt.X = (short)(i%rect.Width + rect.Left);
      pt.Y = (short)(i/rect.Width + rect.Top);
    }

    // HashSet not useful as dictionary key (need value equality rather than underlying C pointer equality to be useful)
    // the resulting UNICODE string need not be valid as a UNICODE string.  We just need value-comparison of strings to be value-comparison of the hashset it came from.
    public static string Encode(this Rectangle rect, Point pt)
    {
      // C# char is 16-bit unsigned
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (255<rect.Width || 255<rect.Height) throw new InvalidProgramException("must extend Zaimoni.Data.Encode(Rectangle,Point)");
      if (!rect.Contains(pt)) throw new InvalidOperationException("tried to encode point not in rectangle");
      return new string(new char[] { (char)(256 * (pt.X - rect.Left) + (pt.Y - rect.Top)) });
    }

    public static string Encode(this Box2D_int rect, Vector2D_int pt)
    {
      // C# char is 16-bit unsigned
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (255<rect.Width || 255<rect.Height) throw new InvalidProgramException("must extend Zaimoni.Data.Encode(Rectangle,Point)");
      if (!rect.Contains(pt)) throw new InvalidOperationException("tried to encode point not in rectangle");
      return new string(new char[] { (char)(256 * (pt.X - rect.Left) + (pt.Y - rect.Top)) });
    }

    public static string Encode(this Box2D_short rect, Vector2D_short pt)
    {
      // C# char is 16-bit unsigned
      if (0>=rect.Width || 0>=rect.Height) throw new InvalidOperationException("empty rectangle");
      if (255<rect.Width || 255<rect.Height) throw new InvalidProgramException("must extend Zaimoni.Data.Encode(Rectangle,Point)");
      if (!rect.Contains(pt)) throw new InvalidOperationException("tried to encode point not in rectangle");
      return new string(new char[] { (char)(256 * (pt.X - rect.Left) + (pt.Y - rect.Top)) });
    }


    public static string Encode(this uint src)
    {
       if (char.MaxValue >= src) return new string(new char[] { (char)src });
       return new string(new char[] { (char)(src>>16),(char)(src & char.MaxValue) });
    }

    public static string Encode(this Rectangle rect, HashSet<Point> src)
    {
      if (null==src || 0 >= src.Count) return string.Empty;
      var tmp = src.ToList();
      tmp.Sort((a,b)=> {
          var test = a.X.CompareTo(b.X);
          if (0 != test) return test;
          return a.Y.CompareTo(b.Y);
      });
      return string.Concat(tmp.Select(pt => rect.Encode(pt)));
    }

    // encode a dictionary to a string
    public static string Encode(this Rectangle rect, Dictionary<Point,int> src)
    {
      if (null==src || 0 >= src.Count) return string.Empty;
      var tmp = src.Keys.ToList();
      tmp.Sort((a,b)=> {
          var test = a.X.CompareTo(b.X);
          if (0 != test) return test;
          return a.Y.CompareTo(b.Y);
      });
      return string.Concat(tmp.Select(pt => rect.Encode(pt)+((uint)(src[pt])).Encode()));
    }

    public static string Encode(this Box2D_int rect, Dictionary<Vector2D_int, int> src)
    {
      if (null==src || 0 >= src.Count) return string.Empty;
      var tmp = src.Keys.ToList();
      tmp.Sort((a,b)=> {
          var test = a.X.CompareTo(b.X);
          if (0 != test) return test;
          return a.Y.CompareTo(b.Y);
      });
      return string.Concat(tmp.Select(pt => rect.Encode(pt)+((uint)(src[pt])).Encode()));
    }

    public static string Encode(this Box2D_short rect, Dictionary<Vector2D_short, int> src)
    {
      if (null==src || 0 >= src.Count) return string.Empty;
      var tmp = src.Keys.ToList();
      tmp.Sort((a,b)=> {
          var test = a.X.CompareTo(b.X);
          if (0 != test) return test;
          return a.Y.CompareTo(b.Y);
      });
      return string.Concat(tmp.Select(pt => rect.Encode(pt)+((uint)(src[pt])).Encode()));
    }

    // Some library classes have less than useful ToString() overrides.
    // We go with Ruby syntax x.to_s() rather than Python syntax str(x)
    public static string to_s<T>(this HashSet<T> x) {
      if (null == x) return "null";
      if (0 >= x.Count) return "{}";
      var tmp = new List<string>(x.Count);
      foreach(T iter in x) {
        tmp.Add(iter.to_s());
      }
      tmp[0] = "{"+ tmp[0];
      tmp[tmp.Count-1] += "} ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T>(this List<List<T>> x) {  // would be redundant in C++
      if (null == x) return "null";
      if (0 >= x.Count) return "[]";
      var tmp = new List<string>(x.Count);
      foreach(var iter in x) {
        tmp.Add(iter.to_s());
      }
      tmp[0] = "["+ tmp[0];
      tmp[tmp.Count-1] += "] ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T>(this List<T[]> x) {
      if (null == x) return "null";
      if (0 >= x.Count) return "[]";
      var tmp = new List<string>(x.Count);
      foreach(var iter in x) {
        tmp.Add(iter.to_s());
      }
      tmp[0] = "["+ tmp[0];
      tmp[tmp.Count-1] += "] ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T>(this T[] x) {
      if (null == x) return "null";
      if (0 >= x.Length) return "[]";
      var tmp = new List<string>(x.Length);
      foreach(var iter in x) {
        tmp.Add(iter.to_s());
      }
      tmp[0] = "["+ tmp[0];
      tmp[tmp.Count-1] += "] ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T>(this List<T> x) {
      if (null == x) return "null";
      if (0 >= x.Count) return "[]";
      var tmp = new List<string>(x.Count);
      foreach(var iter in x) {
        tmp.Add(iter.to_s());
      }
      tmp[0] = "["+ tmp[0];
      tmp[tmp.Count-1] += "] ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T,U>(this Dictionary<T,List<U>> x) {
      if (null == x) return "null";
      if (0 >= x.Count) return "{}";
      var tmp = new List<string>(x.Count);
      foreach(var iter in x) {
        tmp.Add(iter.Key.to_s()+":"+iter.Value.to_s());
      }
      tmp[0] = "{"+ tmp[0];
      tmp[tmp.Count-1] += "} ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T,U>(this Dictionary<T,U> x) {
      if (null == x) return "null";
      if (0 >= x.Count) return "{}";
      var tmp = new List<string>(x.Count);
      foreach(var iter in x) {
        tmp.Add(iter.Key.to_s()+":"+iter.Value.to_s());
      }
      tmp[0] = "{"+ tmp[0];
      tmp[tmp.Count-1] += "} ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s<T>(this IEnumerable<T> x) {
      if (null == x) return "null";
      if (0 >= x.Count()) return "[]";
      var tmp = new List<string>(x.Count());
      foreach(T iter in x) {
        tmp.Add(iter.to_s());
      }
      tmp[0] = "["+ tmp[0];
      tmp[tmp.Count-1] += "] ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s(this Point x) {
      return "("+x.X.ToString()+","+x.Y.ToString()+")";
    }

    // ultimate fallback
    public static string to_s<T>(this T x) {
      if (null == x) return "null";
      return x.ToString();
    }
  } // ext_Drawing

  // Gödel encoding using Chinese remainder theorem
  public static class crmth
  {
    public static void encode(ref int dest, int data, int _base)
    {
#if DEBUG
      if (1 >= _base) throw new InvalidOperationException("1 >= _base");
      if (0 == _base%2) throw new InvalidOperationException("0 == _base%2");
      if (_base/2 < data) throw new InvalidOperationException("0 == _base/2 < data");
      if (-(_base / 2) > data) throw new InvalidOperationException("0 == -(_base / 2) > data");
      if (int.MaxValue/_base <= dest) throw new InvalidOperationException("int.MaxValue/_base <= data");
      if (int.MinValue/_base >= dest) throw new InvalidOperationException("int.MinValue/_base >= dest");
#endif
      dest *= _base;
      dest += data;
    }

    public static int decode(ref int src, int _base)
    {
#if DEBUG
      if (1 >= _base) throw new InvalidOperationException("1 >= _base");
      if (0 == _base%2) throw new InvalidOperationException("0 == _base%2");
#endif
      int threshold = _base/2;
      int ret = src % _base;
      if (threshold < ret) ret -= _base;
      else if (-threshold > ret) ret += _base;
      src -= ret;
      src /= _base;
      return ret;
    }

    public static void encode(ref uint dest, uint data, uint _base)
    {
#if DEBUG
      if (1 >= _base) throw new InvalidOperationException("1 >= _base");
      if (uint.MaxValue/_base <= dest) throw new InvalidOperationException("int.MaxValue/_base <= data");
#endif
      dest *= _base;
      dest += data;
    }

    public static uint decode(ref uint src, uint _base)
    {
#if DEBUG
      if (1 >= _base) throw new InvalidOperationException("1 >= _base");
#endif
      uint ret = src % _base;
      src -= ret;
      src /= _base;
      return ret;
    }

    public static uint unordered_pair(uint lhs, uint rhs, uint _base)
    {
#if DEBUG
      if (1 >= _base) throw new InvalidOperationException("1 >= _base");
      if (_base <= lhs) throw new InvalidOperationException("_base <= lhs");
      if (_base <= rhs) throw new InvalidOperationException("_base <= rhs");
#endif
      return (lhs < rhs) ? _base*lhs+rhs : _base * rhs + lhs;
    }

    public static uint max_unordered_pair(uint _base)
    {
#if DEBUG
      if (1 >= _base) throw new InvalidOperationException("1 >= _base");
      if (uint.MaxValue/_base < _base) throw new InvalidOperationException("int.MaxValue/_base <= data");
#endif
      return (_base-2)*_base+(_base-1);
    }

    public static bool is_valid_unordered_pair(uint src, uint _base)
    {
      if (max_unordered_pair(_base) < src) return false;
      uint high = src%_base;
      uint low = src/_base;
      return low<high;
    }
  }

  // not nearly as agile as C++
  public static class Functor
  {
    // trivial functors
    public static void NOP<T>(T x) { }
    public static bool TRUE<T>(T x) { return true; }
    public static bool FALSE<T>(T x) { return false; }
    public static T IDENTITY<T>(T x) { return x; }
    public static T DEFAULT<T>(T x) { return default; }
    public static int ZERO<T>(T x) { return 0; }
  }

  // const correctness hack
  public readonly ref struct const_<T> where T:struct
  {
    public readonly T cache;

    public const_(T src) {
      cache = src;
    }
  }

}   // Zaimoni.Data
