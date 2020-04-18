using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
    /// <summary>
    /// 2-key dictionary; intended use case is Location, GameItem id, turn
    /// </summary>
    /// <typeparam name="Key1">The key for which a nothing observed entry is meaningful; e.g. Point</typeparam>
    /// <typeparam name="Key2">The key for which something has to be observed; e.g. GameItems.IDs</typeparam>
    /// <typeparam name="Range">the value, e.g. turn</typeparam>
    [Serializable]
    class Ary2Dictionary<Key1, Key2, Range>
    {
        readonly private Dictionary<Key1, Range> _no_entries = new Dictionary<Key1, Range>();
        readonly private Dictionary<Key1, KeyValuePair<Range, HashSet<Key2>>> _first_second_dict = new Dictionary<Key1, KeyValuePair<Range, HashSet<Key2>>>();
        readonly private Dictionary<Key2, Dictionary<Key1, Range>> _second_first_dict = new Dictionary<Key2, Dictionary<Key1, Range>>();

        public Ary2Dictionary() {}

        public void Clear() {
            lock (_no_entries) {
            lock (_first_second_dict) {
            lock (_second_first_dict) { // defines canonical locking sequence
                 _no_entries.Clear();
                _first_second_dict.Clear();
                _second_first_dict.Clear();
            }
            }
            }
        }

        public bool HaveEverSeen(Key1 key) {
            lock (_no_entries) { if (_no_entries.ContainsKey(key)) return true; }
            lock (_first_second_dict) { if (_first_second_dict.ContainsKey(key)) return true; }
            return false;
        }

        public bool HaveEverSeen(Key1 key, out Range value) {
            lock (_no_entries) { if (_no_entries.TryGetValue(key, out value)) return true; }
            lock (_first_second_dict) {
                if (_first_second_dict.TryGetValue(key, out var x)) {
                    value = x.Key;
                    return true;
                }
            }
            return false;
        }

        // Yes, value copy for these two
        public HashSet<Key2> WhatIsAt(Key1 key) {
            lock (_first_second_dict) { return _first_second_dict.TryGetValue(key, out var src) ? src.Value : null; }
        }

        public Dictionary<Key1, Range> WhereIs(Key2 key)
        {
            lock (_second_first_dict) { return (_second_first_dict.TryGetValue(key, out var src)) ? new Dictionary<Key1, Range>(src) : null; }
        }

        public Dictionary<Key1, Range> WhereIs(Key2 key, Predicate<Key1> test)
        {   // copy constructor failed by race condition: need to use a multi-threaded dictionary
            lock (_second_first_dict) {
                if (!_second_first_dict.TryGetValue(key, out var src)) return null;
                var ret = new Dictionary<Key1, Range>(src.Count);
                foreach (var x in src) if (test(x.Key)) ret.Add(x.Key, x.Value);
                return 0<ret.Count ? ret : null;
            }
        }

        public List<Key2> WhatHaveISeen() {
            lock (_second_first_dict) { return new List<Key2>(_second_first_dict.Keys); }
        }

        public void Set(Key1 key, IEnumerable<Key2> keys2, Range value) {
            if (!keys2?.Any() ?? true) {
                lock (_no_entries) {
                lock (_first_second_dict) {
                lock (_second_first_dict) {
                    _first_second_dict.Remove(key);
                    _no_entries[key] = value;
                    Remove(key);
                }
                }
                }
                return;
            }

            var incoming = new HashSet<Key2>(keys2);
            List<Key2> expired = null;
            lock (_no_entries) {
            lock (_first_second_dict) {
            lock (_second_first_dict) {
            _first_second_dict[key] = new KeyValuePair<Range, HashSet<Key2>>(value, new HashSet<Key2>(incoming));
            foreach (var x in _second_first_dict) {
                if (incoming.Contains(x.Key)) {
                  x.Value[key] = value;
                  incoming.Remove(x.Key);
                  continue;
                }
                if (x.Value.Remove(key) && 0 >= x.Value.Count) (expired ?? (expired = new List<Key2>(_second_first_dict.Count))).Add(x.Key);
            }
            if (null != expired) foreach (Key2 tmp in expired) _second_first_dict.Remove(tmp);
            foreach(Key2 tmp in incoming) {
              _second_first_dict[tmp] = new Dictionary<Key1, Range> { [key] = value };
            }

            _no_entries.Remove(key);
            }
            }
            }
        }

        private void Remove(Key1 key)   // uses caller for the required lock
        {
            List<Key2> expired = null;
            foreach (var x in _second_first_dict) {
                if (x.Value.Remove(key) && 0 >= x.Value.Count) (expired ?? (expired = new List<Key2>(_second_first_dict.Count))).Add(x.Key);
            }
            if (null != expired) foreach (Key2 tmp in expired) _second_first_dict.Remove(tmp);
        }

#if FAIL
        public void Instruct(Ary2Dictionary<Key1, Key2, Range> student)
        {
            Range student_last_taught;
            foreach(KeyValuePair<Key1, Range> tmp in _no_entries) {
              if (student.HaveEverSeen(tmp.Key, out student_last_taught) && student_last_taught>=tmp.Value) continue;
              student.Set(tmp.Key,null,tmp.Value);
            }

            foreach(KeyValuePair<Key1, Dictionary<Key2, Range>> tmp in _first_second_dict) {
            }
        }
#endif
    }
}
