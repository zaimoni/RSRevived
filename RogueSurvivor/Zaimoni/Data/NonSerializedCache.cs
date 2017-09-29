using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zaimoni.Data
{
    class NonSerializedCache<src,T,U> where src:class where U : class, IEnumerable<T>
    {
        private src m_src;
        private U m_cache;
        private readonly Func<src,U> m_bootstrap;
        private readonly Action<src> m_invalidate;

        public NonSerializedCache(src x, Func<src,U> bootstrap, Action<src> invalidate = null)
        {
#if DEBUG
            if (null == x) throw new ArgumentNullException(nameof(x));
            if (null == bootstrap) throw new ArgumentNullException(nameof(bootstrap));
#endif
            m_src = x;
            m_bootstrap = bootstrap;
            m_invalidate = invalidate;
        }

        public U Get { get { return m_cache ?? (m_cache = m_bootstrap(m_src)); } }
        public void Recalc() {
            m_cache = null;
            m_invalidate?.Invoke(m_src);
        }
    }   // NonSerializedCache
}
