using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using Point = Zaimoni.Data.Vector2D_short;


namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    internal class Pathfinder
    {
        private readonly LocationFunction<int>[] m_GoalTypes;

        public Pathfinder(int n) {
#if DEBUG
            if (0 >= n) throw new InvalidOperationException("backing array must be positive length");
#endif
            m_GoalTypes = new LocationFunction<int>[n];
            while(0 <= --n) m_GoalTypes[n] = new LocationFunction<int>();
        }

#region forwarding to array interface
        public LocationFunction<int> this[int n] { get { return m_GoalTypes[n]; } }
        public int Length { get { return m_GoalTypes.Length; } }
#endregion

        public LocationFunction<int> Goals() {
          int compare(int lhs, int rhs) { return lhs.CompareTo(rhs); }

          int ub = m_GoalTypes.Length;
          var ret = m_GoalTypes[--ub].ForwardingClone();
          while(0 <= --ub) ret.ForwardingMerge(m_GoalTypes[ub], compare);
          return ret;
        }

        public void Remove(IEnumerable<Location> locs) {
            foreach (var x in m_GoalTypes) x.Remove(locs);
        }

        public void Remove(Location loc) {
            foreach (var x in m_GoalTypes) x.Remove(loc);
        }
    }
}
