using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [Conditional("DEBUG")]
        public static void Ok(Actor a, IEnumerable<Location> src) {
            foreach (var loc in src) if (!a.MayEnter(loc)) {
                string err = a.Name + " must be able to exist at all destinations";
                Logger.Trace(err);
                Logger.Trace(src.to_s());
                throw a.Spotlight(new InvalidOperationException(err));
            }
        }

        public MinStepPath(Actor a, in Location src, Dictionary<Location, int> dest, Predicate<Location>? veto = null) {
#if DEBUG
            if (!a.MayEnter(src)) throw a.Spotlight(new InvalidOperationException("must be able to exist at the origin"));
#endif
            Ok(a, dest.Keys);
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
            if (tracing) Logger.Trace(m_Actor.Name);

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
                    if (!RequireEndsInGoal(ret, targets)) {
#if DEBUG
                        throw new InvalidOperationException("the chain that reached the goal was pruned");
#else
                        return null;
#endif
                    }

                    int n = -2;
                    while (ret.Count - 3 >= ++n) {
                        if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "n: " + n.ToString());
                        uint code = FavorClearTwoStepRoutes(ret, n);
                        if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "ok: FavorClearTwoStepRoutes(ret, " + n.ToString() + ", "+(1 >= n ? "true" : "false") +")");
                        if (8 == code) return null;
                        if (0 != (1 & code) && 0 < n) n -= 2;   // this window's first layer shrank: the layer before may have dead ends, revisit it
                    }

#if DEBUG
                    CheckConnected(ret);
#endif
                    if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "after cleanup: " + ret.to_s());
                    return ret;
                }

                frontier = next;
            }
        }

        /// <summary>The final layer keeps only goals; each earlier layer keeps only what still has a successor.</summary>
        /// <returns>false if a layer emptied.  Find's layers always retain the chain that reached the goal, so that is a consistency failure.</returns>
        private static bool RequireEndsInGoal(List<List<Location>> ret, Dictionary<Location, int> targets) {
            int ub = ret.Count - 1;
            ret[ub].OnlyIf(loc => targets.ContainsKey(loc));
            while (0 < ret[ub].Count) {
                if (0 >= ub) return true;
                var next = ret[ub];
                ret[--ub].OnlyIf(loc => next.Any(loc2 => 1 == Rules.GridDistance(in loc, in loc2)));
            }
            return false;
        }

#if DEBUG
        /// <summary>every layer is non-empty, and every location has a neighbor in each adjacent layer</summary>
        private static void CheckConnected(List<List<Location>> ret) {
            int ub = ret.Count;
            while (0 <= --ub) {
                var layer = ret[ub];
                if (0 >= layer.Count) throw new InvalidOperationException("empty layer " + ub);
                if (0 < ub) {
                    var prior = ret[ub - 1];
                    if (!layer.All(loc => prior.Any(loc2 => 1 == Rules.GridDistance(in loc, in loc2)))) throw new InvalidOperationException("orphan in layer " + ub);
                }
                if (ret.Count - 1 > ub) {
                    var next = ret[ub + 1];
                    if (!layer.All(loc => next.Any(loc2 => 1 == Rules.GridDistance(in loc, in loc2)))) throw new InvalidOperationException("dead end in layer " + ub);
                }
            }
        }
#endif

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

        /// <summary>
        /// Within the window of three layers starting at offset (-1: the actor's own location stands in for the first),
        /// keep only the locations on some chain start-mid-end; then, if some chain avoids clutter wherever a clear
        /// alternative exists, keep only those chains.
        /// </summary>
        /// <returns>8: no chain at all.  Otherwise bits 1, 2, 4 flag which of the three layers shrank, for either reason.</returns>
        private uint FavorClearTwoStepRoutes(List<List<Location>> ret, int offset) {
            if (ret.Count - 3 < offset) return 0; // not enough space
            bool respond_to_actor = -1 == offset;
            const bool tracing = false;
            if (tracing) Logger.Trace(m_Actor.Name);

            var start = (-1 < offset) ? ret[offset] : new List<Location> { m_Actor.Location };
            var mid = ret[offset + 1];
            var end = ret[offset + 2];
            int count0 = start.Count;
            int count1 = mid.Count;
            int count2 = end.Count;

            uint shrank() {
                uint code = 0;
                if (-1 < offset && ret[offset].Count < count0) code |= 1;
                if (ret[offset + 1].Count < count1) code |= 2;
                if (ret[offset + 2].Count < count2) code |= 4;
                if (tracing) Logger.Trace("(" + count0 + ", " + count1 + ", " + count2 + ") -> " + code);
                return code;
            }

            // connectivity: every location must lie on some chain
            var (forward, _, _) = CatalogStep(start, mid);
            var (forward2, _, code2) = CatalogStep(mid, end);
            if (0 != (code2 & 1)) (forward, _, _) = CatalogStep(start, mid);
            if (0 >= start.Count) return 8;

            // clutter: a chain is cluttered if it passes through clutter where that layer has a clear alternative
            var avoidable0 = (-1 < offset) ? Avoidable(start, respond_to_actor) : null;
            var avoidable1 = Avoidable(mid, respond_to_actor);
            var avoidable2 = Avoidable(end, respond_to_actor);
            if (avoidable0 is null && avoidable1 is null && avoidable2 is null) return shrank();   // nothing to steer around

            bool clear0(Location loc) => avoidable0 is null || !avoidable0.Contains(loc);
            bool clear1(Location loc) => avoidable1 is null || !avoidable1.Contains(loc);
            bool clear2(Location loc) => avoidable2 is null || !avoidable2.Contains(loc);

            // a location survives if it lies on a clear chain: mid needs a clear predecessor and a clear successor,
            // then start and end need a surviving mid neighbor
            var keep1 = new HashSet<Location>();
            foreach (var x in forward) {
                if (clear0(x.Key)) foreach (var loc in x.Value) keep1.Add(loc);
            }
            keep1.RemoveWhere(loc => !clear1(loc) || !forward2.TryGetValue(loc, out var range) || !range.Any(clear2));
            if (0 >= keep1.Count) return shrank();   // every chain is cluttered somewhere

            var keep2 = new HashSet<Location>();
            foreach (var loc in keep1) {
                foreach (var loc2 in forward2[loc]) if (clear2(loc2)) keep2.Add(loc2);
            }

            if (-1 < offset) {
                var keep0 = start.FindAll(loc => clear0(loc) && forward[loc].Any(keep1.Contains));
                if (keep0.Count < start.Count) ret[offset] = keep0;
            }
            if (keep1.Count < mid.Count) ret[offset + 1] = mid.FindAll(keep1.Contains);
            if (keep2.Count < end.Count) ret[offset + 2] = end.FindAll(keep2.Contains);
            return shrank();
        }

        /// <returns>the cluttered locations of a layer that is neither clear nor cluttered throughout; null if there is nothing to steer around</returns>
        private static HashSet<Location>? Avoidable(List<Location> layer, bool respond_to_actor) {
            HashSet<Location>? ret = null;
            foreach (var loc in layer) {
                if (Cluttered(loc, respond_to_actor)) (ret ??= new()).Add(loc);
            }
            return (ret is null || ret.Count == layer.Count) ? null : ret;
        }

        private static bool Cluttered(Location loc, bool respond_to_actor)
            => (loc.MapObject is { IsWalkable: false }) || (respond_to_actor && loc.Actor is not null);

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
            foreach (var loc in dest) goal_costs.Add(loc, 0);
            return create(a, in src, goal_costs, veto);
        }
    }
}
