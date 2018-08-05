using System;
using System.Drawing;
using System.Collections.Generic;

namespace Zaimoni.Data
{
    public static class Compass
    {
    	// 2-dimensional compass rose
	    public enum XCOMlike : byte
        {
            N = 0,
            NE,
            E,
            SE,
            S,
            SW,
            W,
            NW,
        };
        // some extensions
        public enum reference : byte
        {
            NEUTRAL = XCOMlike.NW + 1,
            XCOM_STRICT_UB = XCOMlike.NW + 1,
            XCOM_EXT_STRICT_UB = NEUTRAL + 1,
        };

        static public Point Anchor(this Rectangle src, XCOMlike dir_code)  // Tk/Tcl anchor point for aligning a rectangle
        {
        switch(dir_code)
        {
        case XCOMlike.N: return new Point(src.Width / 2 + src.Left, src.Top);
        case XCOMlike.NE: return new Point(src.Right - 1, src.Top);
        case XCOMlike.E: return new Point(src.Right - 1, src.Height / 2 + src.Top);
        case XCOMlike.SE: return new Point(src.Right - 1, src.Bottom - 1);
        case XCOMlike.S: return new Point(src.Width / 2 + src.Left, src.Bottom - 1);
        case XCOMlike.SW: return new Point(src.Left, src.Bottom - 1);
        case XCOMlike.W: return new Point(src.Left, src.Height / 2 + src.Top);
        case XCOMlike.NW: return new Point(src.Left, src.Top);
        default: throw new InvalidOperationException("direction code out of range");
        }
        }

        // precompile some constants we use internally
        private enum _ref : uint
        {
            N_S = XCOMlike.N * reference.XCOM_EXT_STRICT_UB + XCOMlike.S,
            E_W = XCOMlike.E * reference.XCOM_EXT_STRICT_UB + XCOMlike.W,
            NE_SW = XCOMlike.NE * reference.XCOM_EXT_STRICT_UB + XCOMlike.SW,
            NW_SE = XCOMlike.SE * reference.XCOM_EXT_STRICT_UB + XCOMlike.NW,
            N_NEUTRAL = XCOMlike.N * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            NE_NEUTRAL = XCOMlike.NE * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            E_NEUTRAL = XCOMlike.E * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            SE_NEUTRAL = XCOMlike.SE * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            S_NEUTRAL = XCOMlike.S * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            SW_NEUTRAL = XCOMlike.SW * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            W_NEUTRAL = XCOMlike.W * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL,
            NW_NEUTRAL = XCOMlike.NW * reference.XCOM_EXT_STRICT_UB + reference.NEUTRAL
        }

        // We now can encode a tuple of directions using Chinese remainder theorem encoding.
        // Our immediate use case is to model anchor points within a "box", like Tk/Tcl does.
        static public uint LineSegment(uint origin, uint dest)
        {
#if DEBUG
          if (origin == dest) throw new InvalidOperationException("origin == dest");
#endif
          return crmth.unordered_pair(origin,dest, (uint)reference.XCOM_EXT_STRICT_UB);
        }

        // non-strict contains
        static public bool LineSegmentContains(uint lhs, uint rhs)
        {
#if DEBUG
          if (!crmth.is_valid_unordered_pair(lhs, (uint)reference.XCOM_EXT_STRICT_UB)) throw new InvalidOperationException("!crmth.is_valid_unordered_pair(lhs, (uint)reference.XCOM_EXT_STRICT_UB)");
          if (!crmth.is_valid_unordered_pair(rhs, (uint)reference.XCOM_EXT_STRICT_UB)) throw new InvalidOperationException("!crmth.is_valid_unordered_pair(rhs, (uint)reference.XCOM_EXT_STRICT_UB)");
#endif
          if (lhs==rhs) return true;
          switch(lhs)
          {
          case (uint)_ref.N_S:
            if ((uint)_ref.N_NEUTRAL == rhs) return true;
            if ((uint)_ref.S_NEUTRAL == rhs) return true;
            return false;
          case (uint)_ref.E_W:
            if ((uint)_ref.E_NEUTRAL == rhs) return true;
            if ((uint)_ref.W_NEUTRAL == rhs) return true;
            return false;
          case (uint)_ref.NE_SW:
            if ((uint)_ref.NE_NEUTRAL == rhs) return true;
            if ((uint)_ref.SW_NEUTRAL == rhs) return true;
            return false;
          case (uint)_ref.NW_SE:
            if ((uint)_ref.SE_NEUTRAL == rhs) return true;
            if ((uint)_ref.NW_NEUTRAL == rhs) return true;
            return false;
          // XXX general implementation would have the four edges of the reference square \todo IMPLEMENT
          default: return false;
          }
          return false; // in case of typos
        }

        // Five line segments based on the extended compass may be encoded into a 32-bit unsigned integer without unusual measures.

        public class LineGraph
        {
          private uint _radix_point = 0;
          private uint _radix_segment = 0;
          private uint _graph = 0;
          Func<uint,uint, bool> _contains = null;

          public uint Radix { get { return _radix_point; } }

          // defaults are to be correct for the local direction enumeration
          public LineGraph(uint graph = 0, uint radix = (uint)reference.XCOM_EXT_STRICT_UB, Func<uint, uint, bool> contains = null) {
#if DEBUG
            if (1 >= radix) throw new InvalidOperationException("1 >= radix");
#endif
            _radix_point = radix;
            _radix_segment = crmth.max_unordered_pair(radix)+1;
            _graph = graph;
            if (null != contains) _contains = contains;
            else if ((uint)reference.XCOM_EXT_STRICT_UB == _radix_point) _contains = LineSegmentContains;
          }

          public void AddLineSegment(uint origin, uint dest)
          {
            uint delta = crmth.unordered_pair(origin, dest, _radix_point);
            if (0 == _graph) {
              _graph = delta;
              return;
            }
            if (_graph < _radix_segment) {
              if (_graph == delta) return;
              if (null != _contains) {
                if (_contains(_graph,delta)) return;
                if (_contains(delta,_graph)) {
                  _graph = delta;
                   return;
                }
              }
              crmth.encode(ref _graph, delta, _radix_segment);
              return;
            }
            uint working = _graph;
            var staging = new List<uint>();
            while(0 < working) {    // be slightly inefficient for ease of verification
              uint inspect = crmth.decode(ref working, _radix_segment);
              if (delta == inspect) return; // no-op
              if (null != _contains && _contains(inspect,delta)) return; // no-op
              if (null != _contains && _contains(delta, inspect)) staging.Add(delta);
              // XXX complete implementation would check for extension of line segment \todo IMPLEMENT
              else staging.Add(inspect);
            }
            // XXX other processing to normal form would happen here
            // reassemble
            staging.Sort();
            staging.Reverse();
            working = 0;
            foreach(uint seg in staging) {
              crmth.encode(ref working, seg, _radix_segment);
            }
            _graph = working;
          }

          public bool ContainsLineSegment(uint seg)
          {
            if (0 == _graph) return false;
#if DEBUG
            if (seg >= _radix_segment) throw new InvalidOperationException("line segment encoding out of range");
#endif
            if (_graph < _radix_segment) {
              if (_graph == seg) return true;
              if (null != _contains) return _contains(_graph,seg);
              return false;
            }
            uint working = _graph;
            // assume we are close to normal form
            while(0 < working) {    // be slightly inefficient for ease of verification
              uint inspect = crmth.decode(ref working, _radix_segment);
              if (seg == inspect) return true; // no-op
              if (null != _contains) {
                if (_contains(inspect,seg)) return true; // ok
                if (_contains(seg, inspect)) return false;   // implies failure when in normal form
              }
            }            
            return false;
          }
        
          // XXX remove line segment operation
        }
    }
}
