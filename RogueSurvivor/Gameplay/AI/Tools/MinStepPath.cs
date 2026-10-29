using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        private MinStepPathStats(in Location src, in Location denorm)
        {
            origin = src;
            target = denorm.Position;
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
                default: throw new InvalidOperationException("unhandled case");
             }
        }

        public static MinStepPathStats? create(in Location src, in Location dest) {
            if (src.Map.Denormalize(in dest) is not Location denorm) return null;
            if (src == denorm) throw new InvalidOperationException("self-pathing");
            return new MinStepPathStats(src, denorm);
        }
    }

    /// <summary>
    /// Straight-line pathfinder: a path exists only if some goal can be reached in exactly its grid distance.
    /// Fails, rather than navigating indirectly (around an obstacle, or through an exit).
    /// </summary>
    class MinStepPath : ActorOrigin
    {
        private readonly Actor m_Actor;
        private readonly Location m_Origin;
        private readonly Dictionary<Location, int> m_Dest;   // goal, and the cost of what remains after reaching it
        private readonly Predicate<Location>? m_Veto;
        private readonly HashSet<Location> m_Seen = new();
        private readonly HashSet<Location> m_Blacklist = new();

        public Location origin => m_Origin;

        public MinStepPath(Actor a, in Location src, Dictionary<Location, int> dest, Predicate<Location>? veto = null) {
#if DEBUG
           if (!a.MayEnter(src)) throw a.Spotlight(new InvalidOperationException("must be able to exist at the origin"));
           foreach(var loc in dest.Keys) if (!a.MayEnter(loc)) throw a.Spotlight(new InvalidOperationException("must be able to exist at all destinations"));
#endif
            m_Actor = a;
            m_Origin = src;
            m_Dest = dest;
            m_Veto = veto;
            m_Seen.Add(origin);
        }

        public void Blacklist(in Location loc) => m_Blacklist.Add(loc);

        /// <returns>null, or the path as layers: layer 0 is adjacent to the origin, the last layer contains a goal, and
        /// each layer is one step closer to that goal than the one before it.</returns>
        public List<List<Location>>? Find()
        {
            // The endpoints are the goals that tie for least (grid distance, plus the goal's own cost).
            // A goal that cannot be denormalized into the origin's map needs an exit, so it has no straight line.
            var targets = new Dictionary<Location, int>();   // goal, grid distance from the origin
            int best = int.MaxValue;
            foreach (var x in m_Dest) {
                var goal = x.Key;
                int dist = Rules.GridDistance(in m_Origin, in goal);
                if (int.MaxValue == dist || 0 >= dist) continue;
                int total = dist + x.Value;
                if (total > best) continue;
                if (total < best) {
                    best = total;
                    targets.Clear();
                }
                targets.Add(goal, dist);
            }
            if (0 >= targets.Count) return null;

            // a location is on a straight line if it is one step closer to some endpoint than the layer before it
            bool advances(in Location loc, int step) {
                foreach (var x in targets) {
                    var goal = x.Key;
                    if (x.Value - step == Rules.GridDistance(in loc, in goal)) return true;
                }
                return false;
            }

            var ret = new List<List<Location>>();
            var frontier = new List<Location> { m_Origin };
            int step = 0;
            while (true) {
                ++step;
                var next = new List<Location>();
                foreach (var loc in frontier) {
                    foreach (var dir in Direction.COMPASS) {
                        var test = loc + dir;
                        if (!Map.Canonical(ref test)) continue;
                        if (m_Seen.Contains(test)) continue;
                        if (!advances(in test, step)) continue;
                        m_Seen.Add(test);   // accepted or rejected, it gets no second look
                        if (m_Blacklist.Contains(test)) continue;
                        if (m_Veto is not null && m_Veto(test)) continue;
                        if (!m_Actor.MayEnter(ref test)) continue;
                        next.Add(test);
                    }
                }
                if (0 >= next.Count) return null;
                ret.Add(next);
                if (next.Any(loc => targets.ContainsKey(loc))) return ret;
                frontier = next;
            }
        }

        public static MinStepPath? create(Actor a, in Location src, Dictionary<Location, int> dest, Predicate<Location>? veto = null)
        {
            foreach (var loc_dist in dest) {
                if (src.Map.Denormalize(loc_dist.Key) is not Location denorm) return null;
                if (src == denorm) throw new InvalidOperationException("self-pathing");
            }
            return new MinStepPath(a, in src, dest, veto);
        }
    }
}
