using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Zaimoni.Data.Cache
{
    public class Associative<K, V> where K : notnull
    {
      private const int CURRENT = 0;
      private const int STALE = 1;
      private readonly Dictionary<K, V>[] m_Cache = new Dictionary<K,V>[2]{new Dictionary<K,V>(), new Dictionary<K,V>()};

      public bool Expire() {
        lock(m_Cache) {
          m_Cache[STALE].Clear();
          var tmp = m_Cache[CURRENT];
          m_Cache[CURRENT] = m_Cache[STALE];
          m_Cache[STALE] = tmp;
          return 0 >= m_Cache[STALE].Count;
        }
      }

      public bool TryGetValue(K key, out V? value)
      {
        lock (m_Cache) {
          if (m_Cache[CURRENT].TryGetValue(key, out value)) return true;
          if (m_Cache[STALE].TryGetValue(key, out value)) {
            m_Cache[CURRENT].Add(key, value);
            m_Cache[STALE].Remove(key);
            return true;
          }
        }
        value = default;
        return false;
      }

      public void Set(K key, V value)
      {
        lock (m_Cache) {
          m_Cache[CURRENT][key] = value;
          m_Cache[STALE].Remove(key);
        }
      }

      public void Validate(Predicate<V> fn)
      {
        lock (m_Cache) {
          m_Cache[CURRENT].OnlyIf(fn);
          m_Cache[STALE].OnlyIf(fn);
        }
      }
    }

#if PROTOTYPE
    class Linear<K, V> where K : notnull
    {
    }
#endif
}
