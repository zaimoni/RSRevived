using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
    /// <summary>
    /// basic floodfill pathfinder.  Morally a Dijkstra mapper.
    /// </summary>
    /// <typeparam name="T">The coordinate type of the space the path is through; for Map, Point.</typeparam>
    public class FloodfillPathfinder<T>
    {
        readonly private HashSet<T> _blacklist;     // coordinates that cannot be entered at all.  For Map, usually walls.
        private Predicate<T> _blacklist_fn;
        readonly private Func<T, bool> _inDomain;   // legal coordinates; for Map, usually InBounds

        readonly private Dictionary<T, int> _map;

        readonly private Func<T, Dictionary<T, int>> _forward;
        readonly private Func<T, Dictionary<T, int>> _inverse;

        public FloodfillPathfinder(Func<T, Dictionary<T, int>> Forward, Func<T, Dictionary<T, int>> Inverse, Func<T, bool> InDomain)
        {
#if DEBUG
            if (null == Forward) throw new ArgumentNullException(nameof(Forward));
            if (null == Inverse) throw new ArgumentNullException(nameof(Inverse));
            if (null == InDomain) throw new ArgumentNullException(nameof(InDomain));
#endif
            _blacklist = new HashSet<T>();
            _blacklist_fn = null;
            _inDomain = InDomain;   // required
            _map = new Dictionary<T, int>();
            _forward = Forward; // required
            _inverse = Inverse; // useful but not required
        }

        // Need a value copy constructor for typical uses
        public FloodfillPathfinder(FloodfillPathfinder<T> src)
        {
#if DEBUG
            if (null == src) throw new ArgumentNullException(nameof(src));
#endif
            _blacklist = new HashSet<T>(src._blacklist);
            _blacklist_fn = src._blacklist_fn;
            _inDomain = src._inDomain;
            _map = new Dictionary<T, int>(src._map);
            _forward = src._forward;
            _inverse = src._inverse;
        }

        // retain domain and blacklist, change specification of forward and inverse which invalidates the map itself
        public FloodfillPathfinder(FloodfillPathfinder<T> src, Func<T, Dictionary<T, int>> Forward, Func<T, Dictionary<T, int>> Inverse)
        {
#if DEBUG
            if (null == Forward) throw new ArgumentNullException(nameof(Forward));
            if (null == Inverse) throw new ArgumentNullException(nameof(Inverse));
            if (null == src) throw new ArgumentNullException(nameof(src));
#endif
            _blacklist = new HashSet<T>(src._blacklist);
            _blacklist_fn = src._blacklist_fn;
            _inDomain = src._inDomain;
            _map = new Dictionary<T, int>();
            _forward = Forward;
            _inverse = Inverse;
        }


        // blacklist manipulation
        public void Blacklist(IEnumerable<T> src)
        {
            _blacklist.UnionWith(src);
        }

        public void Blacklist(T src)
        {
            if (_inDomain(src)) _blacklist.Add(src);
        }

        public void InstallBlacklist(Predicate<T> src) {
            _blacklist_fn = src;
        }

        public HashSet<T> black_list { get { return new HashSet<T>(_blacklist); } }

        public void Approve(IEnumerable<T> src)
        {
            _blacklist.ExceptWith(src);
        }

        public void Approve(T src)
        {
            _blacklist.Remove(src);
        }

        public void GoalDistance(T goal, T start, int max_cost = int.MaxValue)
        {
            T[] tmp = { goal };
            GoalDistance(tmp, start, max_cost);
        }

        // basic pathfinding.  _map is initialized with a cost function measuring how expensive moving to any goal is.
        public void GoalDistance(IEnumerable<T> goals, T start, int max_cost = int.MaxValue)
        {
            T[] tmp = { start };
            GoalDistance(goals, tmp, max_cost);
        }

        public void GoalDistance(Predicate<T> goals, T start, int max_cost = int.MaxValue)
        {
            T[] tmp = { start };
            GoalDistance(goals, tmp, max_cost);
        }

        // the calculation state at any time is an ordered map _map,__now.  _map is a member variable, while _now historically was a local variable
        public void _reset(Dictionary<int, HashSet<T>> _now)
        {
#if DEBUG
            if (null == _now) throw new ArgumentNullException(nameof(_now));
#endif
            _map.Clear();
            _now.Clear();
        }

        public Dictionary<T, int> _snapshot() { return new Dictionary<T, int>(_map); }

        public void _restore(Dictionary<T, int> src, Dictionary<int, HashSet<T>> _now, Dictionary<int, HashSet<T>> src_now)
        {
            var working_src_now = new Dictionary<int, HashSet<T>>(src_now); // need a value copy here
            foreach (var x in src) {
                bool exists = _map.TryGetValue(x.Key, out int old_cost);
                if (exists && old_cost <= x.Value) {
                    if (working_src_now.TryGetValue(x.Value, out var dest2)) {
                        if (dest2.Remove(x.Key) && 0 >= dest2.Count) working_src_now.Remove(x.Value);
                    }
                    continue;
                }
                // XXX \todo some sense of where to re-start based on geometry rather than full forward testing
                if (_now.TryGetValue(old_cost, out var dest)) {
                    if (dest.Remove(x.Key) && 0 >= dest.Count) _now.Remove(old_cost);
                }
                if (working_src_now.TryGetValue(x.Value, out var dest3) && dest3.Contains(x.Key)) {
                    if (_now.TryGetValue(x.Value, out HashSet<T> dest4)) dest4.Add(x.Key);
                    else _now[x.Value] = new HashSet<T> { x.Key };
                }
            }
        }

#if PROTOTYPE
        public bool _bootstrap(Dictionary<T,int> goal_costs, Dictionary<int, HashSet<T>> _now)
        {
#if DEBUG
            if (null == _now) throw new ArgumentNullException(nameof(_now));
#endif
            bool have_updated = false;
            foreach(var x in goal_costs) {
                if (_blacklist.Contains(x.Key)) continue;
                if (!_inDomain(x.Key)) continue;
                if (null != _blacklist_fn && _blacklist_fn(x.Key)) continue;
                bool has_prior_cost = _map.TryGetValue(x.Key, out int old_cost);
                if (has_prior_cost && old_cost <= x.Value) continue;
                bool old_key = _now.TryGetValue(x.Value, out var now);
                if (!old_key) now = new HashSet<T>();
                _map[x.Key] = x.Value;
                now.Add(x.Key);
                if (!old_key) _now[x.Value] = now;
                have_updated = true;
                if (has_prior_cost) {
                    if (_now.TryGetValue(old_cost, out var elder)) {
                        if (elder.Remove(x.Key) && 0 >= elder.Count) _now.Remove(old_cost);
                    }
                }
            }
            return have_updated;
        }
#endif

        public bool _bootstrap(IEnumerable<T> goals, Dictionary<int, HashSet<T>> _now)
        {
#if DEBUG
            if (null == _now) throw new ArgumentNullException(nameof(_now));
#endif
            IEnumerable<T> legal_goals = goals.Where(tmp => !_blacklist.Contains(tmp) && _inDomain(tmp));
            if (null != _blacklist_fn) legal_goals = legal_goals.Where(tmp => !_blacklist_fn(tmp));
            if (!legal_goals.Any()) return false; // not an error condition when merging in new goals into an existing map
            bool old_key = _now.TryGetValue(0, out var now);
            if (!old_key) now = new HashSet<T>();
#if OBSOLETE
            // 2019-01-06 the corner case this tries to micro-optimize for doesn't happen often enough
            foreach (T tmp in legal_goals) {
                if (_map.TryGetValue(tmp, out int test) && 0 == test) continue;
                _map[tmp] = 0;
                now.Add(tmp);
            }
            if (!old_key && 0<now.Count) _now[0] = now;
#else
            foreach (T tmp in legal_goals) _map[tmp] = 0;
            now.UnionWith(legal_goals);
            if (!old_key) _now[0] = now;
#endif
            return true;
        }

        public void _iterate(Dictionary<int, HashSet<T>> _now, int max_cost = int.MaxValue)
        {
              int cost = _now.Keys.Min();
              int max_delta_cost = max_cost - cost;
              foreach(T tmp in _now[cost]) {
                Dictionary<T, int> candidates = _forward(tmp);
                foreach (KeyValuePair<T, int> tmp2 in candidates) {
                  if (_blacklist.Contains(tmp2.Key)) continue;
                  if (!_inDomain(tmp2.Key)) continue;
                  if (max_delta_cost<= tmp2.Value) continue;
                  if (null != _blacklist_fn && _blacklist_fn(tmp2.Key)) continue;
#if DEBUG
                  if (0 >= tmp2.Value) throw new InvalidOperationException("pathological cost function given to FloodfillFinder");
#else
                  if (0 >= tmp2.Value) continue;    // disallow pathological cost functions
#endif
                  int new_cost = cost+tmp2.Value;
                  if (_map.TryGetValue(tmp2.Key,out int old_cost)) {
                    if (old_cost <= new_cost) continue;
                    if (_now[old_cost].Remove(tmp2.Key) && 0 >= _now[old_cost].Count) _now.Remove(old_cost);
                  }
                  _map[tmp2.Key] = new_cost;
                  if (_now.TryGetValue(new_cost, out HashSet<T> dest)) dest.Add(tmp2.Key);
                  else _now[new_cost] = new HashSet<T>{tmp2.Key};
                }
              }
              _now.Remove(cost);
        }

        public void PartialGoalDistance(IEnumerable<T> start, Dictionary<int, HashSet<T>> _now, int max_cost=int.MaxValue)
        {
#if DEBUG
            if (null == start) throw new ArgumentNullException(nameof(start));
            if (null == _now) throw new ArgumentNullException(nameof(_now));
#endif
            while(0<_now.Count && start.Any(pos => !_map.ContainsKey(pos))) _iterate(_now,max_cost);
        }

        // \todo need to be able to checkpoint/resume this (CPU optimization)
        public void GoalDistance(IEnumerable<T> goals, IEnumerable<T> start, int max_cost=int.MaxValue)
        {
#if DEBUG
            if (null == start) throw new ArgumentNullException(nameof(start));
            if (null == goals) throw new ArgumentNullException(nameof(goals));
#endif
            if (start.Any(pos => !_inDomain(pos))) throw new ArgumentOutOfRangeException(nameof(start),"contains out-of-domain values");
            _map.Clear();

            // a proper Dijkstra search is in increasing cost order
            Dictionary<int, HashSet<T>> _now = new Dictionary<int, HashSet<T>>();
            if (!_bootstrap(goals, _now)) throw new InvalidOperationException("must have at least one goal");

            while (0 < _now.Count && start.Any(pos => !_map.ContainsKey(pos))) _iterate(_now, max_cost);    // inlined PartialGoalDistance
        }

#if PROTOTYPE
        public void GoalDistance(Dictionary<T,int> goal_costs, IEnumerable<T> start, int max_cost=int.MaxValue)
        {
#if DEBUG
            if (null == start) throw new ArgumentNullException(nameof(start));
            if (null == goal_costs) throw new ArgumentNullException(nameof(goal_costs));
#endif
            if (start.Any(pos => !_inDomain(pos))) throw new ArgumentOutOfRangeException(nameof(start),"contains out-of-domain values");
            _map.Clear();

            // a proper Dijkstra search is in increasing cost order
            Dictionary<int, HashSet<T>> _now = new Dictionary<int, HashSet<T>>();
            if (!_bootstrap(goal_costs, _now)) throw new InvalidOperationException("must have at least one goal");

            while (0 < _now.Count && start.Any(pos => !_map.ContainsKey(pos))) _iterate(_now, max_cost);    // inlined PartialGoalDistance
        }
#endif

        // \todo need to be able to checkpoint/resume this (CPU optimization)
        public void GoalDistance(Predicate<T> goals, IEnumerable<T> start, int max_cost=int.MaxValue)
        {
#if DEBUG
            if (null == start) throw new ArgumentNullException(nameof(start));
            if (null == goals) throw new ArgumentNullException(nameof(goals));
#endif
            if (start.Any(pos => !_inDomain(pos))) throw new ArgumentOutOfRangeException(nameof(start),"contains out-of-domain values");
            _map.Clear();

            // this morally does a breadth-first search until some goals are found, then recurses to a more normal implementation

            // a proper Dijkstra search is in increasing cost order
            Dictionary<int, HashSet<T>> _now = new Dictionary<int, HashSet<T>>();
            IEnumerable<T> legal_start = start.Where(tmp => !_blacklist.Contains(tmp) && _inDomain(tmp));
            if (null != _blacklist_fn) legal_start = legal_start.Where(tmp => !_blacklist_fn(tmp));
            if (!legal_start.Any()) throw new InvalidOperationException("no legal starting points");
            if (legal_start.Any(tmp => goals(tmp))) return;    // no-op; not a hard error but should not have called
            if (!_bootstrap(legal_start, _now)) return;    // no-op; should be a hard error but a runtime issue

            var found = new HashSet<T>();

            while (0<_now.Count && start.Any(pos => !_map.ContainsKey(pos))) {
              int cost = _now.Keys.Min();
              int max_delta_cost = max_cost - cost;
              foreach(T tmp in _now[cost]) {
                Dictionary<T, int> candidates = _forward(tmp);
                foreach (KeyValuePair<T, int> tmp2 in candidates) {
                  if (_blacklist.Contains(tmp2.Key)) continue;
                  if (!_inDomain(tmp2.Key)) continue;
                  if (max_delta_cost<= tmp2.Value) continue;
                  if (null != _blacklist_fn && _blacklist_fn(tmp2.Key)) continue;
#if DEBUG
                  if (0 >= tmp2.Value) throw new InvalidOperationException("pathological cost function given to FloodfillFinder");
#else
                  if (0 >= tmp2.Value) continue;    // disallow pathological cost functions
#endif
                  int new_cost = cost+tmp2.Value;
                  if (_map.TryGetValue(tmp2.Key,out int old_cost)) {
                    if (old_cost <= new_cost) continue;
                    if (_now[old_cost].Remove(tmp2.Key) && 0 >= _now[old_cost].Count) _now.Remove(old_cost);
                  }
                  _map[tmp2.Key] = new_cost;
                  if (_now.TryGetValue(new_cost, out HashSet<T> dest)) dest.Add(tmp2.Key);
                  else _now[new_cost] = new HashSet<T>{tmp2.Key};
                  if (goals(tmp2.Key)) found.Add(tmp2.Key);
                }
              }
              _now.Remove(cost);
              if (0 < found.Count) break;
            }
            if (0 < found.Count) GoalDistance(found, start, max_cost);
        }

        public void ReviseGoalDistance(T pos, int new_cost, T start)
        {
            if (_map.TryGetValue(pos, out int old_cost) && old_cost <= new_cost) return;   // alternate route not useful
            int max_cost = Cost(start);
            if (max_cost <= new_cost) return;   // we assume the _forward cost function is not pathological i.e. all costs positive

            HashSet<T> now = new HashSet<T>{pos};
            _map[pos] = new_cost;

            while(0<now.Count && !now.Contains(start)) {
              HashSet<T> next = new HashSet<T>();
              foreach(T tmp in now) {
                int cost = _map[tmp];
                Dictionary<T, int> candidates = _forward(tmp);
                foreach (KeyValuePair<T, int> tmp2 in candidates) {
                  if (_blacklist.Contains(tmp2.Key)) continue;
                  if (!_inDomain(tmp2.Key)) continue;
                  if (max_cost-cost<=tmp2.Value) continue;
                  if (null != _blacklist_fn && _blacklist_fn(tmp2.Key)) continue;
                  int new_dist = cost+tmp2.Value;
                  if (_map.TryGetValue(tmp2.Key,out int test) && test <= new_dist) continue;
                  _map[tmp2.Key] = new_dist;
                  next.Add(tmp2.Key);
                }
              }
              now = next;
            }
        }

        public void ReviseGoalDistance(T pos, int new_cost, IEnumerable<T> start)
        {
            if (_map.TryGetValue(pos, out int old_cost) && old_cost <= new_cost) return;   // alternate route not useful
            foreach(T x in start) {
              int max_cost = Cost(x);
              if (max_cost <= new_cost) continue;   // we assume the _forward cost function is not pathological i.e. all costs positive
              ReviseGoalDistance(pos, new_cost, x);  // XXX performance: refactor rather than recurse
            }
        }

        public IEnumerable<T> Domain { get { return _map.Keys; } }

        public int Cost(T pos)
        {
            return (_map.TryGetValue(pos,out int ret)) ? ret: int.MaxValue;
        }

        public Dictionary<T, int> Approach(T current_pos) {
            if (!_map.TryGetValue(current_pos,out int current_cost)) throw new ArgumentOutOfRangeException(nameof(current_pos), "not in the cost map");
            if (0 == current_cost) return null;   // already at a goal
            Dictionary<T, int> tmp = _inverse(current_pos);
            Dictionary<T, int> ret = new Dictionary<T, int>(tmp.Count);
            foreach (T tmp2 in tmp.Keys) {
                if (_map.TryGetValue(tmp2,out int cost) && cost < current_cost) ret[tmp2] = _map[tmp2];
            }
            if (0 < ret.Count) return ret;
            foreach (T tmp2 in tmp.Keys) {
                if (_map.TryGetValue(tmp2,out int cost) && cost == current_cost) ret[tmp2] = _map[tmp2];
            }
            return (0 < ret.Count) ? ret : null;
        }

        // normal use case is to set depth and then do local optimizations to the returned path
        public List<List<T>> MinStepPathTo(T current_pos,int depth = 0)
        {
            if (!_map.TryGetValue(current_pos, out int current_cost)) throw new ArgumentOutOfRangeException(nameof(current_pos), "not in the cost map");
            if (0 == current_cost) return null;   // already at a goal
            var ret = new List<List<T>>();
            List<T> next = null;
            var bootstrap = Approach(current_pos);
            if (null == bootstrap) return null;
            next = bootstrap.Keys.ToList();
            ret.Add(next);
            if (0 < depth && 1 == next.Count) return ret;    // no optimization indicated
            while (!next.Any(pos => 0 >= _map[pos]) && (0 >= depth || depth > ret.Count)) {
                List<T> working = null; /// would prefer HashSet, but work to ensure T is hashable is excessive in the general case
                foreach (T loc in next) {
                    int reference_cost = Cost(loc);
                    var tmp = Approach(loc);
                    if (null == tmp) continue;
                    tmp.OnlyIf(pt => reference_cost > Cost(pt));
                    if (0 >= tmp.Count) continue;
                    if (null == working) {
                        working = new List<T>(tmp.Keys);
                        continue;
                    }
                    tmp.OnlyIf(pt => !working.Contains(pt));
                    if (0 >= tmp.Count) continue;
                    working.AddRange(tmp.Keys);
                }
                if (null == working) break;
                ret.Add(working);
                next = working;
            }
            return ret;
        }

#if DEAD_FUNC
        public Dictionary<T, int> Flee(T current_pos) {
            if (!_map.TryGetValue(current_pos,out int current_cost)) throw new ArgumentOutOfRangeException(nameof(current_pos), "not in the cost map");
            Dictionary<T, int> tmp = _forward(current_pos);
            Dictionary<T, int> ret = new Dictionary<T, int>(tmp.Count);
            foreach (T tmp2 in tmp.Keys) {
                if (_map.TryGetValue(tmp2, out int cost) && cost > current_cost) ret[tmp2] = _map[tmp2];
            }
            return (0 < ret.Count) ? ret : null;
        }
#endif
    }
}