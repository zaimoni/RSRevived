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
            foreach(T tmp in src.Keys) {
                if (0 >= _weights[tmp]) _weights.Remove(tmp);
            }
        }

        public DenormalizedProbability(DenormalizedProbability<T> src)
        {
            _weights = new Dictionary<T, float>(src._weights);
        }

        public int Count { get { return _weights.Count; } }
        public Dictionary<T,float>.KeyCollection Keys { get { return _weights.Keys; } }

        public float this[T x]
        {
            get {
                if (!_weights.ContainsKey(x)) return 0.0f;
                return _weights[x];
            }
            set {
                if (0 < value) _weights[x] = value;
                else _weights.Remove(x);
            }
        }

        public void Normalize(float target = 1f)
        {
#if DEBUG
            if (0f >= target || 1f < target) throw new ArgumentOutOfRangeException(nameof(target), target, "must be in (0,1]");
#endif
            if (0 >= _weights.Count) return;    // must have at least one entry to normalize
retry:      if (1 == _weights.Count) {
              var tmp = new Dictionary<T,float>();
              foreach(var x in _weights) tmp[x.Key] = target;
              _weights = tmp;
              return;
            }
            double threshold = 2.0*target;
retry2:     double sum = 0.0;
            // accept inaccuracy in exchange for not flogging the GC
            {   // scoping brace
            foreach(var x in _weights) {
                while(threshold - sum <= x.Value) {
                    if (_divide_by(2f) && 1>=Count) goto retry;
                    goto retry2;
                }
                sum += x.Value;
            }
            }   // end scoping brace
            if (target == sum) return;
            double scale = target/sum;
            if (1.0-2*float.Epsilon < scale && 1.0+2*float.Epsilon > scale) return; // close to normalized

            {
            var tmp = new Dictionary<T, float>();
            foreach(var x in _weights) {
                float tmp2 = (float)(x.Value*scale);
                if (0f == tmp2) continue;                
                tmp[x.Key] = tmp2;
            }
            _weights = tmp;
            }
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
    }
}
