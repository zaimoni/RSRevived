using System;
using System.Collections.Generic;

#nullable enable

namespace Zaimoni.Data
{
    class TimeCache<K,V> where K:notnull
    {
      private readonly Dictionary<int, Dictionary<K,V> > _map = new Dictionary<int, Dictionary<K, V>>();
      private int _now;

      public bool Expire(int t0) {
        lock(_map) {
          _map.OnlyIf(t => t > t0);
          return 0 >= _map.Count;
        }
      }
      public void Now(int t0) {
        lock(_map) {
          if (!_map.ContainsKey(t0)) _map.Add(t0, new Dictionary<K, V>());
          _now = t0;
        }
      }

      public bool TryGetValue(K key, out V value)
      {
        lock(_map) {
          if (_map[_now].TryGetValue(key,out value)) return true;
          foreach(KeyValuePair<int, Dictionary<K, V> > x in _map) {
            if (x.Key == _now) continue;
            if (x.Value.TryGetValue(key,out value)) {
              if (x.Key < _now) {
                _map[_now][key] = value;
                x.Value.Remove(key);
              }
              return true;
            }
          }
        }
        return false;
      }

      public void Set(K key, V value)
      {
        lock(_map) {
          if (!_map.TryGetValue(_now, out var cache)) _map.Add(_now, (cache = new Dictionary<K, V>()));
          cache[key] = value;
          foreach(KeyValuePair<int, Dictionary<K, V> > x in _map) {
            if (x.Key != _now) x.Value.Remove(key);
          }
        }
      }

      public void Validate(Predicate<V> fn)
      {
        lock(_map) { _map[_now].OnlyIf(fn); }
      }
    }
}
