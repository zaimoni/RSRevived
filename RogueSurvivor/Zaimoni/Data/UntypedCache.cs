using System;
using System.Collections.Generic;

namespace Zaimoni.Data
{
    class UntypedCache<K>
    {
        private Dictionary<K, object> _map = null;

        // Create Read Update Delete API
        bool Has(K key) { return _map?.ContainsKey(key) ?? false; }
        T Get<T>(K key) {
          if (null == _map) return default;
          if (!_map.TryGetValue(key, out var test)) return default;
          if (test is T ret) return ret;
#if DEBUG
          throw new InvalidOperationException("value is not of required type");
#else
          return default;
#endif
        }

        void Unset(K key) {
            if (null != _map) {
              _map.Remove(key);
              if (0 >= _map.Count)  _map = null;
            }
        }

        void Set(K key, object val) {
          (_map ?? (_map = new Dictionary<K, object>()))[key] = val;
        }
    }
}
