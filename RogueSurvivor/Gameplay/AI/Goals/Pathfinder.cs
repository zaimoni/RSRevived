using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;
using Point = Zaimoni.Data.Vector2D_short;


namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    internal class Pathfinder
    {
        private readonly LocationFunction<Point>[] m_GoalTypes;

        public Pathfinder(int n) {
            m_GoalTypes = new LocationFunction<Point>[n];
            while(0 <= --n) m_GoalTypes[n] = new LocationFunction<Point>();
        }

        // forwarding to array interface
        public LocationFunction<Point> this[int n] { get { return m_GoalTypes[n]; } }
        public int Length { get { return m_GoalTypes.Length; } }
    }
}
