using System;

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
          default: return false;
          }
          return false; // in case of typos
        }
    }
}
