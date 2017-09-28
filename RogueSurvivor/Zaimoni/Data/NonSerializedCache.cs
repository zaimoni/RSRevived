using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zaimoni.Data
{
    class NonSerializedCache<T,U> where U: class,IEnumerable<T>
    {
        private U m_cache;
        private readonly Func<U> m_bootstrap;
        private readonly Action m_invalidate;

        public NonSerializedCache(Func<U> bootstrap, Action invalidate = null)
        {
#if DEBUG
            if (null == bootstrap) throw new ArgumentNullException(nameof(bootstrap));
#endif
            m_bootstrap = bootstrap;
            m_invalidate = invalidate;
        }

        public U Get { get { return m_cache ?? (m_cache = m_bootstrap()); } }
        public void Recalc() {
            m_cache = null;
            m_invalidate?.Invoke();
        }
    }   // NonSerializedCache
}
