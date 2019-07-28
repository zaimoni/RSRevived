using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;
using ActionMoveDelta = djack.RogueSurvivor.Engine.Actions.ActionMoveDelta;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_short;
using Size = Zaimoni.Data.Vector2D_short;
#else
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
#endif

namespace djack.RogueSurvivor.Gameplay.AI.Tools
{
    [Serializable]
    class MinStepPathStats
    {
        public readonly Map m;
        public readonly Point target;
        private Point m_Origin;
        private Point m_Delta;
        private KeyValuePair<int, int> span_spread;
        private Direction[] xy;

        public Point origin { get { return m_Origin; } }
        public Point delta { get { return m_Delta; } }
        public Direction[] advancing { get { return xy; } }
        public int span { get { return span_spread.Key; } }
        public int spread { get { return span_spread.Value; } }

        public MinStepPathStats(Location src, Location dest)
        {
            m = src.Map;
            m_Origin = src.Position;
            var denorm = src.Map.Denormalize(dest);
            if (null == denorm) throw new ArgumentNullException(nameof(denorm));
            target = denorm.Value.Position;
            resetOrigin(src);
        }

        private void resetOrigin(Location src)
        {
            var denorm = m.Denormalize(src);
            if (null == denorm) throw new ArgumentNullException(nameof(denorm));
            m_Origin = denorm.Value.Position;

#if Z_VECTOR
            m_Delta = target - m_Origin;
            Point absDelta = m_Delta.coord_xform(Math.Abs);
#else
            m_Delta = new Point(target.X - m_Origin.X, target.Y - m_Origin.Y);
            Point absDelta = new Point(0 <= m_Delta.X ? m_Delta.X : -m_Delta.X, 0 <= m_Delta.Y ? m_Delta.Y : -m_Delta.Y);
#endif
            bool x_dominant = absDelta.X > absDelta.Y;
            span_spread = x_dominant ? new KeyValuePair<int, int>(absDelta.X, absDelta.X - absDelta.Y) : new KeyValuePair<int, int>(absDelta.Y, absDelta.Y - absDelta.X);
#if Z_COMPASS
#else
            if (x_dominant) {
                if (0 < m_Delta.X) xy = (0 <= m_Delta.Y) ? new Direction[] { Direction.SE, Direction.E, Direction.NE } : new Direction[] { Direction.NE, Direction.E, Direction.SE };
                else xy = (0 <= m_Delta.Y) ? new Direction[] { Direction.SW, Direction.W, Direction.NW } : new Direction[] { Direction.NW, Direction.W, Direction.SW };
            } else {
                if (0 < m_Delta.Y) xy = (0 <= m_Delta.X) ? new Direction[] { Direction.SE, Direction.S, Direction.SW } : new Direction[] { Direction.SW, Direction.S, Direction.SE };
                else xy = (0 <= m_Delta.X) ? new Direction[] { Direction.NE, Direction.N, Direction.NW } : new Direction[] { Direction.NW, Direction.N, Direction.NE };
            }
#endif
        }
    }

    [Serializable]
    class MinStepPath
    {
        public MinStepPathStats stats;
        private Actor m_Actor;

        private readonly Dictionary<KeyValuePair<Point, Point>, ActionMoveDelta> _moves;
        private readonly HashSet<Point> _impassible;
        private readonly HashSet<Point> _clear;

        public MinStepPath(Actor a, Location src, Location dest) {
#if DEBUG
           if (!a.CanEnter(src)) throw new InvalidOperationException("must be able to exist at the origin");
           if (!a.CanEnter(dest)) throw new InvalidOperationException("must be able to exist at the destination");
#endif
            m_Actor = a;
            stats = new MinStepPathStats(src, dest);
        }
    }
}
