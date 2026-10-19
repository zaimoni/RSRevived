using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Zaimoni.Data
{
    [Serializable]
    class UntypedCache<K>
    {
        private Dictionary<K, object>? _map = null;

        // Create Read Update Delete API
        public bool Has(K key) { return _map?.ContainsKey(key) ?? false; }
        [return: MaybeNull] public T Get<T>(K key) {
          if (_map is null) return default;
          if (!_map.TryGetValue(key, out var test)) return default;
          if (test is T ret) return ret;
          return default;
        }

        public void Unset(K key) {
            if (_map is not null) {
              _map.Remove(key);
              if (0 >= _map.Count)  _map = null;
            }
        }

        public void Set(K key, object val) {
          (_map ??= new Dictionary<K, object>())[key] = val;
        }
        public IEnumerable<KeyValuePair<K, object>> Contents { get => _map ?? (IEnumerable<KeyValuePair<K, object>>)Array.Empty<KeyValuePair<K, object>>(); }
    }
}
