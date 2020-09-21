using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
    // Use float rather than double to conserve savefile size.
    // Be sure to use industry-standard practice of double for intermediate calculations
    // We plausibly won't get large enough for failure of int to fit in float to be a problem
    // Note that we are still probability rather than quasiprobability; thus, negative values
    // are illegal.
    // Default value if no key is zero.

    public class DenormalizedProbability<T>
    {
        private readonly Dictionary<T, float> _weights;
        public readonly Dataflow<Dictionary<T, float>, double> Norm;

        Dictionary<T, float> Weights {
            set {
                _weights.Clear();
                foreach (var x in value) _weights[x.Key] = x.Value;
                Norm.Recalc();
            }
        }

        public DenormalizedProbability()
        {
            _weights = new Dictionary<T, float>();
            Norm = new Dataflow<Dictionary<T, float>, double>(_weights, _current_norm);
        }

        public DenormalizedProbability(Dictionary<T, float> src)
        {
            _weights = new Dictionary<T, float>(src);
            Norm = new Dataflow<Dictionary<T, float>, double>(_weights, _current_norm);
        }

        public DenormalizedProbability(DenormalizedProbability<T> src)
        {
#if DEBUG
            if (null == src) throw new ArgumentNullException(nameof(src));
#endif
            _weights = new Dictionary<T, float>(src._weights);
            Norm = new Dataflow<Dictionary<T, float>, double>(_weights, _current_norm);
        }

        public int Count { get { return _weights.Count; } }
        public Dictionary<T, float>.KeyCollection Keys { get { return _weights.Keys; } }

        public float this[T x]
        {
            get {
                return (_weights.TryGetValue(x, out float ret)) ? ret : 0.0f;
            }
            set {
                if (0 < value) _weights[x] = value;
                else _weights.Remove(x);
                Norm.Recalc();
            }
        }

        public void Normalize(float target = 1f)
        {
#if DEBUG
            if (0f >= target || 1f < target) throw new ArgumentOutOfRangeException(nameof(target), target, "must be in (0,1]");
#endif
            if (0 >= _weights.Count) return;    // must have at least one entry to normalize
retry:      if (target == Norm.Get) return;
            double scale = target / Norm.Get;
            if (1.0 - 2 * float.Epsilon < scale && 1.0 + 2 * float.Epsilon > scale) return; // close to normalized
            Norm.Recalc();
            if (1 == _weights.Count) {
                var keys = _weights.Keys.ToList();
                if (target == _weights[keys[0]]) return;
                _weights[keys[0]] = target;
                return;
            }

            {
                var tmp = new Dictionary<T, float>(_weights);
                foreach (var x in tmp) {
                    float tmp2 = (float)(x.Value * scale);
                    if (0f == tmp2) _weights.Remove(x.Key);
                    _weights[x.Key] = tmp2;
                }
            }
//          goto retry; // easily infinite-loops
        }

        // overload operator *
        public static DenormalizedProbability<KeyValuePair<T, T>> operator *(DenormalizedProbability<T> lhs, DenormalizedProbability<T> rhs)
        {
#if DEBUG
            if (null == lhs) throw new ArgumentNullException(nameof(lhs));
            if (null == rhs) throw new ArgumentNullException(nameof(rhs));
#endif
            var ret = new DenormalizedProbability<KeyValuePair<T, T>>();
            foreach (var x in lhs._weights) {
                foreach (var y in rhs._weights) {
                    // \todo reality-check tmp before risking overflow
                    var tmp = x.Value * y.Value;
                    if (0f >= tmp) continue;
                    ret[new KeyValuePair<T, T>(x.Key, y.Key)] = tmp;
                }
            }
            return ret;
        }

        public static DenormalizedProbability<T> Apply(DenormalizedProbability<KeyValuePair<T, T>> src, Func<T, T, T> op)
        {
#if DEBUG
            if (null == op) throw new ArgumentNullException(nameof(op));
            if (null == src) throw new ArgumentNullException(nameof(src));
#endif
            var ret = new DenormalizedProbability<T>();
            foreach (var x in src._weights) {
                ret[op(x.Key.Key, x.Key.Value)] += x.Value;
            }
            ret.Normalize();
            return ret;
        }

        private static double _current_norm(Dictionary<T, float> src) {
            double sum = 0.0;
            foreach (var x in src) {
                sum += x.Value;
            }
            return sum;
        }
    }   //     class DenormalizedProbability<T>

    static public class ConstantDistribution<T> where T : struct
    {
        readonly private static Dictionary<T, DenormalizedProbability<T>> _cache = new Dictionary<T, DenormalizedProbability<T>>();

        public static DenormalizedProbability<T> Get(T x)
        {
            if (_cache.TryGetValue(x, out var val)) return new DenormalizedProbability<T>(val);
            var raw = new Dictionary<T, float> { [x] = 1 };
            var ret = new DenormalizedProbability<T>(raw);
            _cache[x] = new DenormalizedProbability<T>(ret);
            return ret;
        }
    }   // ConstantDistribution<T>

    static public class UniformDistribution
    {
        readonly private static Dictionary<KeyValuePair<int,int>, DenormalizedProbability<int>> _cache = new Dictionary<KeyValuePair<int, int>, DenormalizedProbability<int>>();

        public static DenormalizedProbability<int> Get(int lb, int ub)
        {
#if DEBUG
            if (lb > ub) throw new InvalidOperationException("lb > ub");
#endif
            if (lb==ub) return ConstantDistribution<int>.Get(lb);
            var index = new KeyValuePair<int, int>(lb, ub);
            if (_cache.TryGetValue(index, out var val)) return new DenormalizedProbability<int>(val);
            var raw = new Dictionary<int, float>();
            for (int x = lb; x <= ub; x++) raw[x] = 1;
            var ret = new DenormalizedProbability<int>(raw);
            ret.Normalize();
            _cache[index] = new DenormalizedProbability<int>(ret);
            return ret;
        }
    }   // UniformDistribution

    static public class Probability_ext
    {
        static private int _less_than(int lhs, int rhs) { return lhs < rhs ? 1 : 0; }
        static public float LessThan(this DenormalizedProbability<int> lhs, DenormalizedProbability<int> rhs)
        {
            DenormalizedProbability<int> ret_src = DenormalizedProbability<int>.Apply(lhs * rhs, _less_than);
            return ret_src[1];
        }

        static public float LessThan(this int lhs, DenormalizedProbability<int> rhs)
        {
            DenormalizedProbability<int> LHS = ConstantDistribution<int>.Get(lhs);
            DenormalizedProbability<int> ret_src = DenormalizedProbability<int>.Apply(LHS * rhs, _less_than);
            return ret_src[1];
        }
    }
}
