using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Zaimoni.Data
{
    // rethinking design here
    // hard-coding Point as the domain didn't work, as that didn't cope with exits.  We really wanted Location as the domain.
    // We probably shouldn't be as concerned with data validation (delegate that to the forward iteration)
    class PlausbilityMap<T>
    {
        /// <summary>
        /// 100% confidence that something is there.
        /// <para>This really should be templated on the type T, but C# doesn't cleanly support this</para>
        /// <para>Numerically, Factorial 7 i.e. 7!  Divisible by 2, 3, ..., 9 which are the possible exit counts from a square</para>
        /// </summary>
        public const int CERTAIN = 5040;

        readonly private Dictionary<T, int> _map;

        readonly private Func<T, Dictionary<T,int>> _forward;
        private int _target_count;

        public PlausbilityMap(Func<T, Dictionary<T, int>> Forward, int TargetCount)
        {
            _map = new Dictionary<T, int>();
            _forward = Forward // required
#if DEBUG
                ?? throw new ArgumentNullException(nameof(Forward))
#endif
            ;
            _target_count = TargetCount;
        }

        // Need a value copy constructor for typical uses
        public PlausbilityMap(PlausbilityMap<T> src)
        {
            _map = new Dictionary<T, int>(src._map);
            _forward = src._forward;
            _target_count = src._target_count;
        }

        public int ConfidenceAt(T x)
        {
            if (_map.ContainsKey(x)) return _map[x];
            return 0;
        }

        // apply an invasion or similar
        public void AddThreat(HashSet<T> SpawnZone, int TargetCount)
        {
            if (int.MaxValue / CERTAIN < TargetCount) throw new ArgumentOutOfRangeException(nameof(TargetCount), "too many threat to add at once");
            if (TargetCount > SpawnZone.Count) throw new InvalidOperationException("more threat than places to spawn in");

            // validate the spawn zone
            Queue<T> valid = new Queue<T>(SpawnZone);

            // add confidence that threat is present
            // this operation does not typically denormalize the confidence map, but it can drive points above CERTAIN which is a denormalization
            int TotalConfidence = TargetCount * CERTAIN;
            while (0 < valid.Count) {
                int _confidence = TotalConfidence / valid.Count;
                T tmp = valid.Dequeue();
                if (_map.ContainsKey(tmp)) _map[tmp] += _confidence;
                else _map[tmp] = _confidence;
                TotalConfidence -= _confidence;
            }
        }

        // apply observations
        public void Observe(HashSet<T> Cleared, HashSet<T> Sighted)
        {
            // positively not there
            if (null != Cleared && 0 < Cleared.Count) {
                foreach (T tmp in Cleared) {
                   _map.Remove(tmp);
                }
            }

            int sighted_count = (null != Sighted ? Sighted.Count : 0);

            if (0 >= sighted_count) {   // no threat sighted: early exit
                Normalize();
                return;
            }

            IgnoreCertain(Sighted);
            Normalize();
            IncludeCertain(Sighted);
        }

        // let time elapse
        // this should not denormalize; exits are problematic to model
        public void TimeStep(HashSet<T> Observed)
        {
            Dictionary<T, int> new_map = new Dictionary<T, int>();
            if (null != Observed && 0 < Observed.Count) {
                foreach(T tmp in Observed) {
                    if (_map.ContainsKey(tmp)) new_map[tmp] = _map[tmp];
                }
            }
            foreach(T tmp in _map.Keys) {
                if (Observed.Contains(tmp)) continue;
                Dictionary<T, int> next_step = _forward(tmp);    // return value is a denormalized weighted probability distribution
                foreach(T tmp2 in new HashSet<T>(next_step.Keys)) {
                    next_step.Remove(tmp2);
                }
                int total_weight = next_step.Values.Sum();
                foreach(T tmp2 in next_step.Keys) {
                    if (new_map.ContainsKey(tmp2)) new_map[tmp2] += (new NormalizeScaler(next_step[tmp2],total_weight)).Scale(_map[tmp]);
                    else new_map[tmp2] = (new NormalizeScaler(next_step[tmp2], total_weight)).Scale(_map[tmp]);
                }
            }
        }

        private void IgnoreCertain(HashSet<T> Certain)
        {
            if (null == Certain || 0 >= Certain.Count) return;
            _target_count -= Certain.Count;
            foreach (T tmp in Certain) {
                _map.Remove(tmp);
            }
        }

        private void IncludeCertain(HashSet<T> Certain)
        {
            if (null == Certain || 0 >= Certain.Count) return;
            _target_count += Certain.Count;
            foreach (T tmp in Certain) {
                _map[tmp] = CERTAIN;
            }
        }

        private bool TrivialNormalize() {
            if (0 >= _map.Count) {
                _target_count = 0;
                return true;
            }
            if (0 >= _target_count) {
                _target_count = 0;
                _map.Clear();
                return true;
            }
            return false;
        }

        private void Normalize()
        {
            // handle corner cases
            if (TrivialNormalize()) return;

            // if everything adds up, do nothing
            int TotalConfidence = _map.Values.Sum();
            if (TotalConfidence == _target_count * CERTAIN) return;

            // we are currently denormalized.
            // locations that are certain are already normalized, so temporarily ignore them
            HashSet<T> Certain = new HashSet<T>(_map.Keys.Where(x => CERTAIN <= _map[x]));
            IgnoreCertain(Certain);

            // corner cases, again
            TrivialNormalize();

            // typical case
            if (0 < _target_count) {
                TotalConfidence = _map.Values.Sum();
                int TheoreticalTotalConfidence = _target_count * CERTAIN;
                // over-certain locations could have denormalized us, so re-check
                if (TotalConfidence != TheoreticalTotalConfidence) {
                    NormalizeScaler scaler = new NormalizeScaler(TheoreticalTotalConfidence,TotalConfidence);
                    int max = _map.Values.Max();
                    if (max != scaler.Scale(max)) { // not a no-op
                        Queue<T> tmp2 = new Queue<T>(_map.Keys);
                        while(0 < tmp2.Count) {
                            T tmp = tmp2.Dequeue();
                            if (0 < _map[tmp]) _map[tmp] = scaler.Scale(_map[tmp]);
                            else _map.Remove(tmp);
                        }
                    }
                }
            }

            // re-add certain locations
            IncludeCertain(Certain);
        }

        class NormalizeScaler
        {
            readonly int _actual;
            readonly int _theoretical;
            readonly Dictionary<int, int> _scale_cache; // not generally useful, but for this use case the range of incoming values is likely to be small.

            public NormalizeScaler(int Theoretical, int Actual)
            {
                _actual = Actual;
                _theoretical = Theoretical;
                _scale_cache = new Dictionary<int, int>();
            }

            // shouldn't need a copy constructor for this use case

            public int Scale(int x)
            {
                if (_scale_cache.ContainsKey(x)) return _scale_cache[x];
                int scaled = 0;
                if (int.MaxValue/ _theoretical >= x)
                {
                    int tmp = _theoretical * x;
                    scaled = tmp / _actual;
                    if (_theoretical < _actual) {
                        int remainder = tmp % _actual;
                        if (0 < remainder) ++scaled;
                    }
                    _scale_cache[x] = scaled;
                    return scaled;
                }
                scaled = (int)((double)_theoretical * x / _actual);
                if (_theoretical < _actual && scaled < x) ++scaled; // not really right, but we don't have infinite-precision arithmetic
                _scale_cache[x] = scaled;
                return scaled;
            }
        }
    }
}