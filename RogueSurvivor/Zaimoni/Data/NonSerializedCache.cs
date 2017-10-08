using System;
using System.Collections.Generic;

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

    class Dataflow<src,T> where T : struct
    {
        private src m_src;
        private T? m_cache;
        private readonly Func<src,T> m_bootstrap;
        private readonly Action<src> m_invalidate;

        public Dataflow(src x, Func<src,T> bootstrap, Action<src> invalidate = null)
        {
#if DEBUG
            if (null == x) throw new ArgumentNullException(nameof(x));
            if (null == bootstrap) throw new ArgumentNullException(nameof(bootstrap));
#endif
            m_src = x;
            m_bootstrap = bootstrap;
            m_invalidate = invalidate;
        }

        public T Get {
            get {
                if (null == m_cache) m_cache = m_bootstrap(m_src);
                return m_cache.Value;
            }
        }

        public void Recalc() {
            m_cache = null;
            m_invalidate?.Invoke(m_src);
        }
    }   // Dataflow
}
