using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Zaimoni.Data
{
    /// <summary>
    /// basic floodfill pathfinder
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
            if (null == Forward) throw new ArgumentNullException("Forward");
            if (null == Inverse) throw new ArgumentNullException("Inverse");
            if (null == InDomain) throw new ArgumentNullException("InDomain");
            Contract.EndContractBlock();
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
            if (null == Forward) throw new ArgumentNullException("Forward");
            if (null == Inverse) throw new ArgumentNullException("Inverse");
            Contract.EndContractBlock();
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

        public void Approve(IEnumerable<T> src)
        {
            foreach(T tmp in src) {
                _blacklist.Remove(tmp);
            }
        }

        // basic pathfinding.  _map is initialized with a cost function measuring how expensive moving to any goal is.
        public void GoalDistance(IEnumerable<T> goals, int max_depth, T start)
        {
            if (null == start) throw new ArgumentNullException("start");
            if (null == goals) throw new ArgumentNullException("goals");
            Contract.EndContractBlock();
            if (!_inDomain(start)) throw new ArgumentOutOfRangeException("start","illegal value");
//          Contract.EndContractBlock();
            _map.Clear();
            Queue<T> gen0 = new Queue<T>();
            foreach(T tmp in goals) {
                if (_blacklist.Contains(tmp)) continue;
                if (!_inDomain(tmp)) continue;
                _map[tmp] = 0;
                gen0.Enqueue(tmp);
            }
            while(0 < gen0.Count && 0 < max_depth && !_map.ContainsKey(start)) {
                --max_depth;
                Queue<T> gen1 = new Queue<T>();
                Dictionary<T,Dictionary<T, int>> candidate_dict = new Dictionary<T, Dictionary<T, int>>();
                while (0 < gen0.Count) {
                    T tmp = gen0.Dequeue();
                    Dictionary<T, int> candidates = _forward(tmp);
                    Dictionary<T, int> legal = new Dictionary<T, int>();
                    foreach (T tmp2 in candidates.Keys) {
                        if (_blacklist.Contains(tmp2)) continue;
                        if (_map.ContainsKey(tmp2)) continue;
                        if (!_inDomain(tmp2)) continue;
                        legal[tmp2] = candidates[tmp2];
                    }
                    if (0 < legal.Count) candidate_dict[tmp] = legal;
                }
                gen0 = new Queue<T>(candidate_dict.Keys);
                while (0< gen0.Count) {
                    T tmp = gen0.Dequeue();
                    int Distance = _map[tmp];
                    foreach (T tmp2 in candidate_dict[tmp].Keys) {
                        if (!_map.ContainsKey(tmp2)) {
                            _map[tmp2] = Distance + candidate_dict[tmp][tmp2];
                            gen1.Enqueue(tmp2);
                            continue;
                        }
                        if (_map[tmp2] > Distance + candidate_dict[tmp][tmp2]) _map[tmp2] = Distance + candidate_dict[tmp][tmp2];
                    }
                }
                gen0 = gen1;
            }
        }

        public Dictionary<T, int> Approach(T current_pos) {
            if (!_map.ContainsKey(current_pos)) throw new ArgumentOutOfRangeException("current_pos","not in the cost map");
//          Contract.EndContractBlock();
            int current_cost = _map[current_pos];
            if (0 == current_cost) return null;   // already at a goal
            Dictionary<T, int> tmp = _inverse(current_pos);
            Dictionary<T, int> ret = new Dictionary<T, int>(tmp.Count);
            foreach (T tmp2 in tmp.Keys) {
                if (_map[tmp2] < current_cost) ret[tmp2] = _map[tmp2];
            }
            if (0 == ret.Count) return null;
            return ret;
        }

        public Dictionary<T, int> Flee(T current_pos) {
            if (!_map.ContainsKey(current_pos)) throw new ArgumentOutOfRangeException("current_pos","not in the cost map");
//          Contract.EndContractBlock();
            int current_cost = _map[current_pos];
            Dictionary<T, int> tmp = _forward(current_pos);
            Dictionary<T, int> ret = new Dictionary<T, int>(tmp.Count);
            foreach (T tmp2 in tmp.Keys) {
                if (_map[tmp2] > current_cost) ret[tmp2] = _map[tmp2];
            }
            if (0 == ret.Count) return null;
            return ret;
        }
    }
}