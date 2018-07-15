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
            XCOM_EXT_STRICT_UB = NEUTRAL + 1
        };

        // We now can encode a tuple of directions using Chinese remainder theorem encoding.
        // Our immediate use case is to model anchor points within a "box", like Tk/Tcl does.
        static public uint UnorderedLineSegment(uint origin, uint dest)
        {
#if DEBUG
          if (origin == dest) throw new InvalidOperationException("origin == dest");
#endif       
          return crmth.unordered_pair(origin,dest, (uint)reference.XCOM_EXT_STRICT_UB);
        }
    }
}
