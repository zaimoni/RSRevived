using System;
using System.Collections.Generic;

namespace Zaimoni.Data
{
    [Serializable]
    class UntypedCache<K>
    {
        private Dictionary<K, object> _map = null;

        // Create Read Update Delete API
        public bool Has(K key) { return _map?.ContainsKey(key) ?? false; }
        public T Get<T>(K key) {
          if (null == _map) return default;
          if (!_map.TryGetValue(key, out var test)) return default;
          if (test is T ret) return ret;
          return default;
        }

        public void Unset(K key) {
            if (null != _map) {
              _map.Remove(key);
              if (0 >= _map.Count)  _map = null;
            }
        }

        public void Set(K key, object val) {
          (_map ??= new Dictionary<K, object>())[key] = val;
        }
    }
}
