using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

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
            const bool tracing = false;
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "MinStepPath::Find: " + m_Actor.Name);

            // start reference implementation by Claude
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

                if (next.Any(loc => targets.ContainsKey(loc))) {
                    if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "before cleanup: " + ret.to_s());
                    RequireEndsInGoal(ret, targets);

                    int n = -2;
                    while (ret.Count - 3 >= ++n) {
                        if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "n: " + n.ToString());
                        uint code = FavorClearTwoStepRoutes(ret, n);
                        if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "ok: FavorClearTwoStepRoutes(ret, " + n.ToString() + ", "+(1 >= n ? "true" : "false") +")");
                        if (8 == code) return null;
                        if (0 != (1 & code) && 0 < n) {
                            n -= 2;
                            continue;
                        }
                    }

                    if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "after cleanup: " + ret.to_s());
                    return ret;
                }

                frontier = next;
            }
        }

        private void RequireEndsInGoal(List<List<Location>> ret, Dictionary<Location, int> targets) {
            int ub = ret.Count - 1;
            int tail_count = ret[ub].Count;
            ret[ub].OnlyIf(loc => targets.ContainsKey(loc));
#if DEBUG
            if (ret[ub].Count < tail_count) throw new InvalidOperationException("MinStepPath::RequireEndsInGoal: should implement prunedown");
#endif
        }

        private (Dictionary<Location, List<Location>>, HashSet<Location>, uint) CatalogStep(List<Location> start, List<Location> end) {
            Dictionary<Location, List<Location>> forward = new();
            HashSet<Location> range = new();

            foreach (var loc in start) {
                List<Location> stage = new();
                foreach (var loc2 in end) {
                    if (1 != Rules.GridDistance(loc, loc2)) continue;
                    stage.Add(loc2);
                    range.Add(loc2);
                }
                if (0 < stage.Count) forward.Add(loc, stage);
            }

            uint code = 0;
            if (forward.Count < start.Count) {
                start.Clear();
                start.AddRange(forward.Keys);
                code |= 1;
            }
            if (range.Count < end.Count) {
                end.Clear();
                end.AddRange(range);
                code |= 2;
            }
            return (forward, range, code);
        }

        private List<Location[]>? EnumerateTwoStepRoutes(List<Location> start, List<Location> mid, List<Location> end) {
            const bool tracing = false;
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "MinStepPath::EnumerateTwoStepRoutes: " + m_Actor.Name);
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "(" + start.Count.ToString()+", " + mid.Count.ToString() + ", " + end.Count.ToString() + ")");

            var (dict, _, code) = CatalogStep(start, mid);
            var (dict2, _, code2) = CatalogStep(mid, end);
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "(" + start.Count.ToString() + ", " + mid.Count.ToString() + ", " + end.Count.ToString() + ") " + code.ToString() + " " + code2.ToString());
            if (0 != (code2 & 1)) {
                (dict, _, _) = CatalogStep(start, mid);
                if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "(" + start.Count.ToString() + ", " + mid.Count.ToString() + ", " + end.Count.ToString() + ")" + code.ToString() + " " + code2.ToString());
            }

            List<Location[]> ret = new();
            foreach (var x in dict) {
                foreach(var x2 in x.Value) {
                    if (!dict2.TryGetValue(x2, out var range)) continue;
                    foreach (var x3 in range) {
                        Location[] stage = [x.Key, x2, x3];
                        ret.Add(stage);
                    }
                }
            }
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "returning: MinStepPath::EnumerateTwoStepRoutes");
            return 0 < ret.Count ? ret : null;
        }

        public uint FavorClearTwoStepRoutes(List<List<Location>> ret, int offset) {
            if (ret.Count - 3 < offset) return 0; // not enough space
            bool respond_to_actor = -1 == offset;
            const bool tracing = false;
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "MinStepPath::FavorClearTwoStepRoutes: " + m_Actor.Name);

            var test = (-1 < offset) ? EnumerateTwoStepRoutes(ret[offset], ret[offset + 1], ret[offset + 2])
                                     : EnumerateTwoStepRoutes([m_Actor.Location], ret[offset + 1], ret[offset + 2]);
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "ok: EnumerateTwoStepRoutes");
            if (test is null) return 8;

            List<Location> clear0 = new();
            List<Location> cluttered0 = new();
            List<Location> clear1 = new();
            List<Location> cluttered1 = new();
            List<Location> clear2 = new();
            List<Location> cluttered2 = new();

            if (-1 < offset) {
                foreach (var loc in ret[offset]) {
                    if (Cluttered(loc, respond_to_actor)) cluttered0.Add(loc);
                    else clear0.Add(loc);
                }
            } else cluttered0.Add(m_Actor.Location);
            foreach (var loc in ret[offset + 1]) {
                if (Cluttered(loc, respond_to_actor)) cluttered1.Add(loc);
                else clear1.Add(loc);
            }
            foreach (var loc in ret[offset + 2]) {
                if (Cluttered(loc, respond_to_actor)) cluttered2.Add(loc);
                else clear2.Add(loc);
            }

            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "test: "+test.to_s());

            List<Location[]> clear = new();
            List<Location[]> cluttered = new();
            foreach (var x in test) {
                if (0 < clear0.Count && 0 < cluttered0.Count && cluttered0.Contains(x[0])) cluttered.Add(x);
                else if (0 < clear1.Count && 0 < cluttered1.Count && cluttered1.Contains(x[1])) cluttered.Add(x);
                else if (0 < clear2.Count && 0 < cluttered2.Count && cluttered2.Contains(x[2])) cluttered.Add(x);
                else clear.Add(x);
            }
            if (0 >= clear.Count) return 0;
            if (0 >= cluttered.Count) return 0;
            foreach (var x in cluttered) test.Remove(x);

            uint code = 0;
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "have_pruned: "+test.to_s());

            // trading CPU for reduced GC loading
            List<Location> image0 = new();
            List<Location> image1 = new();
            List<Location> image2 = new();

            foreach (var x in test) {
                if (!image0.Contains(x[0])) image0.Add(x[0]);
                if (!image1.Contains(x[1])) image1.Add(x[1]);
                if (!image2.Contains(x[2])) image2.Add(x[2]);
            }

            if (-1 < offset && image0.Count < ret[offset].Count) {
                ret[offset] = image0;
                code |= 1;
            }
            if (image1.Count < ret[offset + 1].Count) {
                ret[offset + 1] = image1;
                code |= 2;
            }
            if (image2.Count < ret[offset + 2].Count) {
                ret[offset + 2] = image2;
                code |= 4;
            }
            return code;
        }

        private static bool Cluttered(Location loc, bool respond_to_actor) {
            var mapobj = loc.MapObject;
            if (mapobj is not null && !mapobj.IsWalkable) return true;
            else if (respond_to_actor) {
                var other = loc.Actor;
                if (other is not null) return true;
            }
            return false;
        }

        public static MinStepPath? create(Actor a, in Location src, Dictionary<Location, int> dest, Predicate<Location>? veto = null)
        {
            foreach (var x in dest) {
                if (src.Map.Denormalize(x.Key) is not Location denorm) return null;
                if (src == denorm) throw new InvalidOperationException("self-pathing");
            }
            return new MinStepPath(a, in src, dest, veto);
        }

        public static MinStepPath? create(Actor a, in Location src, IEnumerable<Location> dest, Predicate<Location>? veto = null)
        {
            var goal_costs = new Dictionary<Location, int>();

            foreach (var loc in dest) {
                if (src.Map.Denormalize(loc) is not Location denorm) return null;
                if (src == denorm) throw new InvalidOperationException("self-pathing");
                goal_costs.Add(loc, 0);
            }

            return new MinStepPath(a, in src, goal_costs, veto);
        }
    }
}
