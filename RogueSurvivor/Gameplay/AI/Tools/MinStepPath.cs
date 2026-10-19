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
        public readonly Location origin;
        public readonly Point target;
        public readonly Point delta;
        private readonly KeyValuePair<int, int> span_spread;
        private readonly Direction[] xy;

        public Direction[] advancing { get { return xy; } }
        public int span { get { return span_spread.Key; } }
        public int spread { get { return span_spread.Value; } }

        public MinStepPathStats(in Location src, in Location dest)
        {
            origin = src;
            var denorm = src.Map.Denormalize(in dest);
            if (denorm is null) throw new ArgumentNullException(nameof(denorm));
            if (src == denorm) throw new InvalidOperationException("self-pathing");
            target = denorm.Value.Position;
            delta = target - origin.Position;

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
            switch (dominant)
            {
                case Compass.XCOMlike.NE:
                    xy = [Direction.NE];
                    return;
                case Compass.XCOMlike.SE:
                    xy = [Direction.SE];
                    return;
                case Compass.XCOMlike.SW:
                    xy = [Direction.SW];
                    return;
                case Compass.XCOMlike.NW:
                    xy = [Direction.NW];
                    return;
                case Compass.XCOMlike.E:
                    if (1 == spread) {
                        if (0 < delta.Y) {
                            xy = [Direction.SE, Direction.E];
                            return;
                        }
                        xy = [Direction.NE, Direction.E];
                        return;
                    } else {
                        if (0 < delta.Y) {
                            xy = [Direction.SE, Direction.E, Direction.NE];
                            return;
                        }
                        xy = [Direction.NE, Direction.E, Direction.SE];
                        return;
                    }
                case Compass.XCOMlike.W:
                    if (1 == spread) {
                        if (0 < delta.Y) {
                            xy = [Direction.SW, Direction.W];
                            return;
                        }
                        xy = [Direction.NW, Direction.W];
                        return;
                    } else {
                        if (0 < delta.Y) {
                            xy = [Direction.SW, Direction.W, Direction.NW];
                            return;
                        }
                        xy = [Direction.NW, Direction.W, Direction.SW];
                        return;
                    }
                case Compass.XCOMlike.N:
                    if (1 == spread) {
                        if (0 < delta.X) {
                            xy = [Direction.NE, Direction.N];
                            return;
                        }
                        xy = [Direction.NW, Direction.N];
                        return;
                    } else {
                        if (0 < delta.X) {
                            xy = [Direction.NE, Direction.N, Direction.NW];
                            return;
                        }
                        xy = [Direction.NW, Direction.N, Direction.NE];
                        return;
                    }
                case Compass.XCOMlike.S:
                    if (1 == spread) {
                        if (0 < delta.X) {
                            xy = [Direction.SE, Direction.S];
                            return;
                        }
                        xy = [Direction.SW, Direction.S];
                        return;
                    } else {
                        if (0 < delta.X) {
                            xy = [Direction.SE, Direction.S, Direction.SW];
                            return;
                        }
                        xy = [Direction.SW, Direction.S, Direction.SE];
                        return;
                    }
             }
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
