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

        public void Normalize(float target)
        {

        }
    }
}
