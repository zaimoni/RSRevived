using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

#nullable enable

namespace Zaimoni.SetTheory
{
    // An enumerated mathematical function is just a Dictionary

    // unfortunately, C# does not have C++ concept testing so we cannot compile-time test for an appropriate implementation.
    // use C-suffix naming scheme to compensate
    [Serializable]
    public class RelationC<K,V> where K:class
    {
        // lock(_rel) for read lock
        // lock(this) for write lock
        // acquire write lock before read lock
        private List<KeyValuePair<K,List<V>>> _rel = new();

        public RelationC() { }

        public void Clear() { lock(this) { _rel = new(); } }

        public void Add(K key, V val)
        {
            lock (this) {
                lock (_rel) {
                    foreach (var xy in _rel) {
                        if (xy.Key == key) {
                            if (!xy.Value.Contains(val)) xy.Value.Add(val);
                            return;
                        }
                    }
                    _rel.Add(new(key, [val]));
                }
            }
        }

        public void Add(K key, IEnumerable<V> vals)
        {
            lock (this) {
                lock (_rel) {
                    foreach (var xy in _rel) {
                        if (xy.Key == key) {
                            foreach (var val in vals) {
                                if (!xy.Value.Contains(val)) xy.Value.Add(val);
                            }
                            return;
                        }
                    }
                    _rel.Add(new(key, new(vals)));
                }
            }
        }

        public bool Remove(K key, V val)
        {
            lock (this) {
                lock (_rel) {
                    var n = _rel.Count;
                    while (0 <= --n) {
                        var xy = _rel[n];
                        if (xy.Key != key) continue;
                        var ret = xy.Value.Remove(val);
                        if (ret && 0 >= xy.Value.Count) _rel.RemoveAt(n);
                        return ret;
                    }
                    return false;
                }
            }
        }

        public bool Remove(K key)
        {
            lock (this) {
                lock (_rel) {
                    var n = _rel.Count;
                    while (0 <= --n) {
                        var xy = _rel[n];
                        if (xy.Key != key) continue;
                        _rel.RemoveAt(n);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Remove(K key, [NotNullWhen(true)] out List<V>? range)
        {
            range = default;
            lock (this) {
                lock (_rel) {
                    var n = _rel.Count;
                    while (0 <= --n) {
                        var xy = _rel[n];
                        if (xy.Key != key) continue;
                        range = xy.Value;
                        _rel.RemoveAt(n);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Contains(K key) => null != Range(key);

        public bool Contains(V val) {
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (xy.Value.Contains(val)) return true;
                }
            }
            return false;
        }

        public bool Contains(K key, V val)
        {
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (xy.Key != key) continue;
                    return xy.Value.Contains(val);
                }
            }
            return false;
        }

        public IEnumerable<V>? Range(K key) {
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (xy.Key != key) continue;
                    return xy.Value;
                }
            }
            return default;
        }

        public List<K>? Domain(V val) {
            List<K> ret = new();
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (xy.Value.Contains(val)) ret.Add(xy.Key);
                }
            }
            return 0<ret.Count ? ret : default;
        }

        public bool EmptyDomain() => 0 >= _rel.Count;

        public void ForAll(Action<K, V> op) {
            lock (_rel) {
                foreach (var xy in _rel) {
                    foreach(var y in xy.Value) op(xy.Key, y);
                }
            }
        }
    }

    [Serializable]

    public class RelationS<K,V> where K:IEquatable<K>
    {
        private List<KeyValuePair<K,List<V>>> _rel = new();

        public RelationS() { }

        public void Clear() { lock(this) { _rel = new(); } }

        public void Add(K key, V val)
        {
            lock (this) {
                lock (_rel) {
                    foreach (var xy in _rel) {
                        if (xy.Key.Equals(key)) {
                            if (!xy.Value.Contains(val)) xy.Value.Add(val);
                            return;
                        }
                    }
                    _rel.Add(new(key, [val]));
                }
            }
        }

        public void Add(K key, IEnumerable<V> vals)
        {
            lock (this) {
                lock (_rel) {
                    foreach (var xy in _rel) {
                        if (xy.Key.Equals(key)) {
                            foreach (var val in vals) {
                                if (!xy.Value.Contains(val)) xy.Value.Add(val);
                            }
                            return;
                        }
                    }
                    _rel.Add(new(key, new(vals)));
                }
            }
        }

        public bool Remove(K key, V val)
        {
            lock (this) {
                lock (_rel) {
                    var n = _rel.Count;
                    while (0 <= --n) {
                        var xy = _rel[n];
                        if (!xy.Key.Equals(key)) continue;
                        var ret = xy.Value.Remove(val);
                        if (ret && 0 >= xy.Value.Count) _rel.RemoveAt(n);
                        return ret;
                    }
                    return false;
                }
            }
        }

        public bool Remove(K key)
        {
            lock (this) {
                lock (_rel) {
                    var n = _rel.Count;
                    while (0 <= --n) {
                        var xy = _rel[n];
                        if (!xy.Key.Equals(key)) continue;
                        _rel.RemoveAt(n);
                        return true;
                    }
                    return false;
                }
            }
        }

        public bool Remove(K key, [NotNullWhen(true)] out List<V>? range)
        {
            range = default;
            lock (this) {
                lock (_rel) {
                    var n = _rel.Count;
                    while (0 <= --n) {
                        var xy = _rel[n];
                        if (!xy.Key.Equals(key)) continue;
                        range = xy.Value;
                        _rel.RemoveAt(n);
                        return true;
                    }
                    return false;
                }
            }
        }

        public IEnumerable<V>? Range(K key) {
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (!xy.Key.Equals(key)) continue;
                    return xy.Value;
                }
            }
            return default;
        }

        public bool Contains(K key) => null != Range(key);

        public bool Contains(V val) {
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (xy.Value.Contains(val)) return true;
                }
            }
            return false;
        }

        public bool Contains(K key, V val)
        {
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (!xy.Key.Equals(key)) continue;
                    return xy.Value.Contains(val);
                }
            }
            return false;
        }

        public List<K>? Domain(V val) {
            List<K> ret = new();
            lock (_rel) {
                var n = _rel.Count;
                while (0 <= --n) {
                    var xy = _rel[n];
                    if (xy.Value.Contains(val)) ret.Add(xy.Key);
                }
            }
            return 0<ret.Count ? ret : default;
        }

        public bool EmptyDomain() => 0 >= _rel.Count;
    }
}
