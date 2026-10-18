using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Zaimoni.Data;
using ActionMoveDelta = djack.RogueSurvivor.Engine.Actions.ActionMoveDelta;
using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Gameplay.AI.Tools
{
    class MinStepPathStats
    {
        public readonly Location origin2;
        public readonly Point target;
        public readonly Point delta;
        private readonly KeyValuePair<int, int> span_spread;
        private Direction[] xy;

        public Point origin => origin2.Position;
        public Direction[] advancing { get { return xy; } }
        public int span { get { return span_spread.Key; } }
        public int spread { get { return span_spread.Value; } }

        public MinStepPathStats(in Location src, in Location dest)
        {
            origin2 = src;
            var denorm = src.Map.Denormalize(in dest);
            if (null == denorm) throw new ArgumentNullException(nameof(denorm));
            if (src == denorm) throw new InvalidOperationException("self-pathing");
            target = denorm.Value.Position;
            delta = target - origin2.Position;

            (var dominant, var absDelta) = delta.DominantDir();
            switch (dominant)
            {
                case Compass.XCOMlike.NE:
                case Compass.XCOMlike.SE:
                case Compass.XCOMlike.SW:
                case Compass.XCOMlike.NW:
                    span_spread = new(absDelta.X, 0);
                    break;
                case Compass.XCOMlike.E:
                case Compass.XCOMlike.W:
                    span_spread = new(absDelta.X, absDelta.X - absDelta.Y);
                    break;
                case Compass.XCOMlike.N:
                case Compass.XCOMlike.S:
                    span_spread = new(absDelta.Y, absDelta.Y - absDelta.X);
                    break;
            }

            resetOrigin(absDelta);
        }

        [MemberNotNull(nameof(xy))]
        private void resetOrigin(Point absDelta)
        {
            bool x_dominant = absDelta.X > absDelta.Y;
#if Z_COMPASS
#else
            if (x_dominant) {
                if (0 < delta.X) xy = (0 <= delta.Y) ? new Direction[] { Direction.SE, Direction.E, Direction.NE } : new Direction[] { Direction.NE, Direction.E, Direction.SE };
                else xy = (0 <= delta.Y) ? new Direction[] { Direction.SW, Direction.W, Direction.NW } : new Direction[] { Direction.NW, Direction.W, Direction.SW };
            } else {
                if (0 < delta.Y) xy = (0 <= delta.X) ? new Direction[] { Direction.SE, Direction.S, Direction.SW } : new Direction[] { Direction.SW, Direction.S, Direction.SE };
                else xy = (0 <= delta.X) ? new Direction[] { Direction.NE, Direction.N, Direction.NW } : new Direction[] { Direction.NW, Direction.N, Direction.NE };
            }
#endif
        }
    }

    class MinStepPath
    {
        public MinStepPathStats stats;
        private Actor m_Actor;

        private readonly Dictionary<KeyValuePair<Point, Point>, ActionMoveDelta> _moves;
        private readonly HashSet<Point> _impassible;
        private readonly HashSet<Point> _clear;

        public MinStepPath(Actor a, in Location src, in Location dest) {
#if DEBUG
           if (!a.MayEnter(src)) throw a.Spotlight(new InvalidOperationException("must be able to exist at the origin"));
           if (!a.MayEnter(dest)) throw a.Spotlight(new InvalidOperationException("must be able to exist at the destination"));
#endif
            m_Actor = a;
            stats = new MinStepPathStats(in src, in dest);
        }
    }
}
