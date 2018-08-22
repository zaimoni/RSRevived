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
        readonly private Func<T, bool> _inDomain;   // legal coordinates; for Map, usually InBounds

        readonly private Dictionary<T, int> _map;

        readonly private Func<T, Dictionary<T, int>> _forward;
        readonly private Func<T, Dictionary<T, int>> _inverse;

        public FloodfillPathfinder(Func<T, Dictionary<T,int>> Forward, Func<T,Dictionary<T, int>> Inverse, Func<T, bool> InDomain)
        {
#if DEBUG
            if (null == Forward) throw new ArgumentNullException(nameof(Forward));
            if (null == Inverse) throw new ArgumentNullException(nameof(Inverse));
            if (null == InDomain) throw new ArgumentNullException(nameof(InDomain));
#endif
            _blacklist = new HashSet<T>();
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

        public HashSet<T> black_list { get { return new HashSet<T>(_blacklist); } }

        public void Approve(IEnumerable<T> src)
        {
            _blacklist.ExceptWith(src);
        }

        public void Approve(T src)
        {
            _blacklist.Remove(src);
        }

		public void GoalDistance(T goal, T start, int max_cost=int.MaxValue)
		{
		  T[] tmp = { goal };
		  GoalDistance(tmp,start,max_cost);
		}

        // basic pathfinding.  _map is initialized with a cost function measuring how expensive moving to any goal is.
        public void GoalDistance(IEnumerable<T> goals, T start, int max_cost=int.MaxValue)
        {
            T[] tmp = { start };
            GoalDistance(goals, tmp, max_cost);
        }

        public void GoalDistance(Predicate<T> goals, T start, int max_cost = int.MaxValue)
        {
            T[] tmp = { start };
            GoalDistance(goals, tmp, max_cost);
        }

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
            HashSet<T> now = new HashSet<T>(goals.Where(tmp => !_blacklist.Contains(tmp) && _inDomain(tmp)));
            foreach(T tmp in now) _map[tmp] = 0;
            _now[0] = now;

            while(0<_now.Count && start.Any(pos => !_map.ContainsKey(pos))) {
              int cost = _now.Keys.Min();
              int max_delta_cost = max_cost - cost;
              foreach(T tmp in _now[cost]) {
                Dictionary<T, int> candidates = _forward(tmp);
                foreach (KeyValuePair<T, int> tmp2 in candidates) {
                  if (_blacklist.Contains(tmp2.Key)) continue;
                  if (!_inDomain(tmp2.Key)) continue;
                  if (max_delta_cost<= tmp2.Value) continue;
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
        }

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
            HashSet<T> now = new HashSet<T>(start.Where(tmp => !_blacklist.Contains(tmp) && _inDomain(tmp)));
            if (0 >= now.Count) return; // XXX error condition
            HashSet<T> found = new HashSet<T>(now.Where(tmp => goals(tmp)));
            if (0 < found.Count) return; // XXX error condition
            foreach(T tmp in now) _map[tmp] = 0;
            _now[0] = now;

            while(0<_now.Count && start.Any(pos => !_map.ContainsKey(pos))) {
              int cost = _now.Keys.Min();
              int max_delta_cost = max_cost - cost;
              foreach(T tmp in _now[cost]) {
                Dictionary<T, int> candidates = _forward(tmp);
                foreach (KeyValuePair<T, int> tmp2 in candidates) {
                  if (_blacklist.Contains(tmp2.Key)) continue;
                  if (!_inDomain(tmp2.Key)) continue;
                  if (max_delta_cost<= tmp2.Value) continue;
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