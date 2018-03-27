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

    // unpacking delta codes for < = >
    public static Point sgn_from_delta_code(ref int delta_code)
    {
      if (0==delta_code) return new Point(0,0);
      int scale = 3;
      int threshold = scale/2;
      int index = 0;
      // XXX lack of error checking even in debug mode
      if (0<delta_code) {
        while(threshold < delta_code) {
          if (threshold+scale >= delta_code) {
            delta_code -= scale;
            return new Point(1,index+1);
          }
          scale *= 3;
          threshold = scale/2;
          index++;
        }
        delta_code = 0;
        return new Point(1,0);
      } else {
        while(-threshold > delta_code) {
          if (-threshold-scale <= delta_code) {
            delta_code += scale;
            return new Point(-1,index+1);
          }
          scale *= 3;
          threshold = scale/2;
          index++;
        }
        delta_code = 0;
        return new Point(-1,0);
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

    // Following might actually be redundant due to System.Linq, but a dictionary i.e. associative array really is two sequences (keys and values)
    public static Dictionary<Key, Value> OnlyIf<Key,Value>(this Dictionary<Key,Value> src,Predicate<Value> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      foreach(Key k in src.Keys.ToList()) {
        if (!fn(src[k])) src.Remove(k);
      }
      return src;
    }

    public static Dictionary<Key, Value> OnlyIf<Key,Value>(this Dictionary<Key,Value> src,Predicate<Key> fn)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      foreach(Key k in src.Keys.ToList()) {
        if (!fn(k)) src.Remove(k);
      }
      return src;
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

    // Some library classes have less than useful ToString() overrides.
    // We go with Ruby syntax x.to_s() rather than Python syntax str(x)
    public static string to_s<T>(this HashSet<T> x) {
      if (0 >= x.Count) return "{}";
      List<string> tmp = new List<string>(x.Count);
      foreach(T iter in x) {
        tmp.Add(iter.ToString());
      }
      tmp[0] = "{"+ tmp[0];
      tmp[tmp.Count-1] += "} ("+tmp.Count.ToString()+")";
      return string.Join(",\n",tmp);
    }

    public static string to_s(this Point x) {
      return "("+x.X.ToString()+","+x.Y.ToString()+")";
    }
  } // ext_Drawing
}   // Zaimoni.Data
