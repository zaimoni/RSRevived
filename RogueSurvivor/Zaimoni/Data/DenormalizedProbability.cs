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

    [Serializable]
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

        public int Count {
            get {
              return _weights.Count;
            }
        }

        public Dictionary<T,float>.KeyCollection Keys {
            get {
                return _weights.Keys;
            }
        }

        public float this[T x]
        {
            get {
                if (!_weights.ContainsKey(x)) return 0.0f;
                return _weights[x];
            }
            set {
                if (0 < value) _weights[x] = value;
                else if (_weights.ContainsKey(x)) _weights.Remove(x);
            }
        }

        public void Normalize(float target = 1f)
        {
#if DEBUG
            if (0f >= target || 1f < target) throw new ArgumentOutOfRangeException(nameof(target), target, "must be in (0,1]");
#endif
            if (0 >= _weights.Count) return;    // must have at least one entry to normalize
retry:      if (1 == _weights.Count) {
              foreach(T x in _weights.Keys) {
                _weights[x] = target;
                return;
              }
            }
            double sum = 0.0;
            float scale = 1f;
            {   // scoping brace
                List<float> tmp2 = new List<float>(_weights.Values);
            tmp2.Sort();    // nonstrictly increasing order is not a bad way to sum positive floating point numerals
            foreach(float tmp3 in tmp2) {
                while(float.MaxValue - sum <= tmp3 * scale) {
                    if (_divide_by(2f)) goto retry;
                    sum /= 2f;
                    scale /= 2f;
                }
                sum += tmp3*scale;
            }
            }   // end scoping brace
            // could start micro-optimizing here
            double threshold = 2.0 * target;
            scale = 1f;
            float discard = float.Epsilon;
            while(threshold <= sum/scale) {
                scale *= 2f;
                discard *= 2f;
                if (_weights.Values.Any(x => x<discard)) {
                    _divide_by(scale);
                    goto retry;
                }
            }
            threshold = target / 2f;
            scale = 1f;
            while(threshold >= sum*scale) scale *= 2f;
            sum *= scale;
            _multiply_by(scale);

            scale = (float)(target / sum);
            if (1.0 == scale) return;   // already normalized
            _multiply_by(scale);
        }

        // powers of 2 are the safest choice
        private bool _divide_by(float n)
        {
            bool deleted = false;
            Dictionary<T, float> tmp = new Dictionary<T, float>();

            foreach (KeyValuePair<T, float> x in _weights) {
                float tmp2 = x.Value / n;
                if (0f >= tmp2) {
                    deleted = true;
                    continue;  // drop underflow
                }
                tmp[x.Key] = tmp2;
            }

            _weights = tmp;
            return deleted;
        }

        private void _multiply_by(float n)
        {
            Dictionary<T, float> tmp = new Dictionary<T, float>();

            foreach (KeyValuePair<T,float> x in _weights) tmp[x.Key] = 2f*x.Value;

            _weights = tmp;
        }
    }
}
