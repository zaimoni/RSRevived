using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

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
			Contract.Requires(null != Forward);
			Contract.Requires(null != Inverse);
			Contract.Requires(null != InDomain);
            _blacklist = new HashSet<T>();
            _inDomain = InDomain;   // required
            _map = new Dictionary<T, int>();
            _forward = Forward; // required
            _inverse = Inverse; // useful but not required
        }

        // Need a value copy constructor for typical uses
        public FloodfillPathfinder(FloodfillPathfinder<T> src)
        {
            _blacklist = new HashSet<T>(src._blacklist);
            _inDomain = src._inDomain;
            _map = new Dictionary<T, int>(src._map);
            _forward = src._forward;
            _inverse = src._inverse;
        }

        // retain domain and blacklist, change specification of forward and inverse which invalidates the map itself
        public FloodfillPathfinder(FloodfillPathfinder<T> src, Func<T, Dictionary<T, int>> Forward, Func<T, Dictionary<T, int>> Inverse)
        {
			Contract.Requires(null != Forward);
			Contract.Requires(null != Inverse);
            _blacklist = new HashSet<T>(src._blacklist);
            _inDomain = src._inDomain;
            _map = new Dictionary<T, int>();
            _forward = Forward;
            _inverse = Inverse;
        }


        // blacklist manipulation
        public void Blacklist(IEnumerable<T> src)
        {
            foreach(T tmp in src) {
                if (_inDomain(tmp)) _blacklist.Add(tmp);
            }
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

        public void GoalDistance(IEnumerable<T> goals, IEnumerable<T> start, int max_cost=int.MaxValue)
        {
            Contract.Requires(null != start);
            Contract.Requires(null != goals);
//          Contract.Requires(!goals.Contains(start));
            if (start.Any(pos => !_inDomain(pos))) throw new ArgumentOutOfRangeException("start","illegal value");
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
                  if (_map.ContainsKey(tmp2.Key)) {
                    int old_cost = _map[tmp2.Key];
                    if (old_cost <= new_cost) continue;
                    if (_now[old_cost].Remove(tmp2.Key) && 0 >= _now[old_cost].Count) _now.Remove(old_cost);
                  }
                  _map[tmp2.Key] = new_cost;
                  if (_now.ContainsKey(new_cost)) _now[new_cost].Add(tmp2.Key);
                  else _now[new_cost] = new HashSet<T>(){tmp2.Key};
                } 
              }
              _now.Remove(cost);
            }
        }

        public void ReviseGoalDistance(T pos, int new_cost, T start)
        {
            if (_map.ContainsKey(pos) && _map[pos] <= new_cost) return;   // alternate route not useful
            int max_cost = Cost(start);
            if (max_cost <= new_cost) return;   // we assume the _forward cost function is not pathological i.e. all costs positive
          
            HashSet<T> now = new HashSet<T>(){pos};
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
                  if (_map.ContainsKey(tmp2.Key) && _map[tmp2.Key] <= new_dist) continue;
                  _map[tmp2.Key] = new_dist;
                  next.Add(tmp2.Key);
                } 
              }
              now = next;
            }
        }

        public IEnumerable<T> Domain { get { return _map.Keys; } }

        public int Cost(T pos)
        {
            return _map.ContainsKey(pos) ? _map[pos] : int.MaxValue;
        }

        public Dictionary<T, int> Approach(T current_pos) {
            if (!_map.ContainsKey(current_pos)) throw new ArgumentOutOfRangeException("current_pos","not in the cost map");
//          Contract.EndContractBlock();
            int current_cost = _map[current_pos];
            if (0 == current_cost) return null;   // already at a goal
            Dictionary<T, int> tmp = _inverse(current_pos);
            Dictionary<T, int> ret = new Dictionary<T, int>(tmp.Count);
            foreach (T tmp2 in tmp.Keys) {
				if (!_map.ContainsKey(tmp2)) continue;
                if (_map[tmp2] < current_cost) ret[tmp2] = _map[tmp2];
            }
            if (0 == ret.Count) return null;
            return ret;
        }

        public Dictionary<T, int> Flee(T current_pos) {
            if (!_map.ContainsKey(current_pos)) throw new ArgumentOutOfRangeException("current_pos", "not in the cost map");
//          Contract.EndContractBlock();
            int current_cost = _map[current_pos];
            Dictionary<T, int> tmp = _forward(current_pos);
            Dictionary<T, int> ret = new Dictionary<T, int>(tmp.Count);
            foreach (T tmp2 in tmp.Keys) {
				if (!_map.ContainsKey(tmp2)) continue;
				if (_map[tmp2] > current_cost) ret[tmp2] = _map[tmp2];
            }
            if (0 == ret.Count) return null;
            return ret;
        }
    }
}