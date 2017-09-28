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

        public NonSerializedCache(Func<U> bootstrap)
        {
#if DEBUG
            if (null == bootstrap) throw new ArgumentNullException(nameof(bootstrap));
#endif
            m_bootstrap = bootstrap;
        }

        U Get { get { return m_cache ?? (m_cache = m_bootstrap()); } }
        void Recalc() { m_cache = null; }

        // use Get.FirstOrDefault, etc. for normal read-only uses
        // following wrap destructive operations
        public bool ActOnce(Action<T> fn, Func<T, bool> test)
        {
#if DEBUG
            if (null == fn) throw new ArgumentNullException(nameof(fn));
            if (null == test) throw new ArgumentNullException(nameof(test));
#endif
            foreach (T x in Get.ToList()) {
                if (test(x)) {
                    fn(x);
                    return true;
                }
            }
            return false;
        }

        public void ActAll(Action<T> fn, Func<T, bool> test)
        {
#if DEBUG
            if (null == fn) throw new ArgumentNullException(nameof(fn));
            if (null == test) throw new ArgumentNullException(nameof(test));
#endif
            foreach (T x in Get.ToList()) {
                if (test(x)) fn(x);
            }
        }

        // for efficiency
        public void ActAll(Action<T> fn)
        {
#if DEBUG
            if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
            foreach (T x in Get.ToList()) fn(x);
        }
    }
}
