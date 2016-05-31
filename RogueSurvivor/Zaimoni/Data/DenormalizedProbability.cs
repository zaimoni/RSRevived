using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
    class DenormalizedProbability<T>
    {
        private Dictionary<T, int> _weights;

        public DenormalizedProbability()
        {
            _weights = new Dictionary<T, int>();
        }

        public DenormalizedProbability(Dictionary<T, int> src)
        {
            _weights = new Dictionary<T, int>(src);
        }

        public DenormalizedProbability(DenormalizedProbability<T> src)
        {
            _weights = new Dictionary<T, int>(src._weights);
        }

        public void MakeSafeToScaleBy(int x)
        {

        }
    }
}
