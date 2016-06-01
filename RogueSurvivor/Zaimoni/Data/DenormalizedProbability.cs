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
    class DenormalizedProbability<T>
    {
        private Dictionary<T, float> _weights;

        public DenormalizedProbability()
        {
            _weights = new Dictionary<T, float>();
        }

        public DenormalizedProbability(Dictionary<T, float> src)
        {
            _weights = new Dictionary<T, float>(src);
            foreach(T tmp in new List<T>(_weights.Keys)) {
                if (0 >= _weights[tmp]) _weights.Remove(tmp);
            }
        }

        public DenormalizedProbability(DenormalizedProbability<T> src)
        {
            _weights = new Dictionary<T, float>(src._weights);
        }

        public float this[T x]
        {
            get {
                if (!_weights.ContainsKey(x)) return 0.0f;
                return _weights[x];
            }
            set {
                if (0 < value) {
                    _weights[x] = value;
                    return;
                }
                if (_weights.ContainsKey(x)) _weights.Remove(x);
            }
        }

        public void Normalize(float target = 1f)
        {
#if DEBUG
            if (0f >= target || 1f < target) throw new ArgumentOutOfRangeException("value to normalize to must be in (0,1]");
#endif
            if (0 >= _weights.Count) return;    // must have at least one entry to normalize
retry:      List<T> tmp = new List<T>(_weights.Keys);
            if (1 == tmp.Count) {   // if only one entry, trivial normalization
                _weights[tmp[0]] = target;
                return;
            }
            List<float> tmp2 = new List<float>(_weights.Values);
            tmp2.Sort();    // nonstrictly increasing order is generally the most numerically stable way to sum positive floating point numerals
            double sum = 0.0;
            foreach(float tmp3 in tmp2) {
                if (float.MaxValue - sum <= tmp3) {
                    _divide_by_2();
                    goto retry;
                }
                sum += tmp3;
            }
            // could start micro-optimizing here
            double target_x_2 = 2.0 * target;
            while(target_x_2 <= sum) {
                _divide_by_2();
                sum /= 2.0;
                if (1 >= _weights.Count) goto retry;    // trivialized
            }
            double target_div_2 = target / 2.0;
            while(target_div_2 >= sum) {
                _multiply_by_2();
                sum *= 2.0;
            }
            double scale = target / sum;
            if (1.0 == scale) return;   // already normalized
            // we should not be capable of overflow at this point
            if (1.0 > scale) {
                double lb = float.Epsilon / scale;
                bool pruned = false;
                foreach (T tmp3 in new List<T>(_weights.Keys)) {
                    if (lb <= _weights[tmp3]) continue;
                    _weights.Remove(tmp3);  // would underflow
                    pruned = true;
                }
                if (pruned) goto retry;
            }
            foreach (T tmp3 in new List<T>(_weights.Keys)) {
                _weights[tmp3] = (float)(_weights[tmp3]*scale);
            }
        }

        // Assume we are on typical hardware: i.e. base 2 floating point numerals.
        // then division by 2 is exact and a relatively safe way to prevent overflow.
        private void _divide_by_2()
        {
            foreach(T tmp in new List<T>(_weights.Keys)) {
                _weights[tmp] /= 2f;
                if (0f >= _weights[tmp]) _weights.Remove(tmp);
            }
        }

        // multiplication by 2 also relatively safe on typical hardware
        private void _multiply_by_2()
        {
            foreach (T tmp in new List<T>(_weights.Keys)) {
                _weights[tmp] *= 2f;
            }
        }
    }
}
