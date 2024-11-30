using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Zaimoni.Collections
{
    // Filtering enumerator.  In principle we could use one with Func<T,bool> as well.
    // The general version should not be capable of serialization.

    // If profiling says there is a CPU problem, we can allow the undefined behavior of accessing 
    // an out-of-range Current through by eliminating the ok member variable.
    internal class EnumeratorNondefault<T> : IEnumerator<T> where T:class
    {
        private readonly IEnumerator<T> enumerator;
        private bool ok = false;

        public EnumeratorNondefault(IEnumerator<T> src) => enumerator = src;
        public EnumeratorNondefault(IEnumerable<T> src) => enumerator = src.GetEnumerator();

        object? System.Collections.IEnumerator.Current { get {
                if (!ok) return default;
                return enumerator.Current;
            } }

        public T? Current { get {
                if (!ok) return default;
                return enumerator.Current;
            } }

        public void Dispose() => enumerator.Dispose();

        public bool MoveNext() {
            while (enumerator.MoveNext()) {
                var test = enumerator.Current;
                if (test == default) continue;
                ok = true;
                return true;
            }
            ok = false;
            return false;
        }

        public void Reset()
        {
            enumerator.Reset();
            ok = false;
        }

    }
}
