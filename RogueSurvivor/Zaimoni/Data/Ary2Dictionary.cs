using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        readonly private ConcurrentDictionary<Key1, Range> _no_entries;
        readonly private ConcurrentDictionary<Key1, KeyValuePair<Range, HashSet<Key2>>> _first_second_dict;
        readonly private ConcurrentDictionary<Key2, ConcurrentDictionary<Key1, Range>> _second_first_dict;

        public Ary2Dictionary() {
            _no_entries = new ConcurrentDictionary<Key1, Range>();
            _first_second_dict = new ConcurrentDictionary<Key1, KeyValuePair<Range, HashSet<Key2>>>();
            _second_first_dict = new ConcurrentDictionary<Key2, ConcurrentDictionary<Key1, Range>>();
        }

        public void Clear() {
            _no_entries.Clear();
            _first_second_dict.Clear();
            _second_first_dict.Clear();
        }

#if DEAD_FUNC
        public bool HaveEverSeen(Key1 key) {
            if (_no_entries.ContainsKey(key)) return true;
            if (_first_second_dict.ContainsKey(key)) return true;
            return false;
        }
#endif

        public bool HaveEverSeen(Key1 key, out Range value) {
            if (_no_entries.TryGetValue(key, out value)) return true;
            if (_first_second_dict.TryGetValue(key, out var x)) {
                value = x.Key;
                return true;
            }
            return false;
        }

        // Yes, value copy for these two
        public HashSet<Key2> WhatIsAt(Key1 key) {
            return _first_second_dict.TryGetValue(key, out var src) ? src.Value : null;
        }

        public Dictionary<Key1, Range> WhereIs(Key2 key)
        {   // copy constructor failed by race condition: need to use a multi-threaded dictionary
            if (_second_first_dict.TryGetValue(key,out var src)) return new Dictionary<Key1, Range>(src);
            return null;
        }

        public List<Key2> WhatHaveISeen() {
            return new List<Key2>(_second_first_dict.Keys);
        }

        public void Set(Key1 key, IEnumerable<Key2> keys2, Range value) {
            List<Key2> expired = new List<Key2>();
            Range val;
            if (!keys2?.Any() ?? true) {
                _first_second_dict.TryRemove(key,out KeyValuePair<Range, HashSet<Key2>> val3);
                _no_entries[key] = value;
                Remove(key);
                return;
            }

            HashSet<Key2> incoming = new HashSet<Key2>(keys2);
            _first_second_dict[key] = new KeyValuePair<Range, HashSet<Key2>>(value, new HashSet<Key2>(incoming));
            foreach (KeyValuePair<Key2, ConcurrentDictionary<Key1, Range>> tmp in _second_first_dict) {
                if (incoming.Contains(tmp.Key)) {
                  tmp.Value[key] = value;
                  incoming.Remove(tmp.Key);
                  continue;
                }
                if (tmp.Value.TryRemove(key,out val) && 0 >= tmp.Value.Count) expired.Add(tmp.Key);
            }
            foreach (Key2 tmp in expired) _second_first_dict.TryRemove(tmp,out ConcurrentDictionary<Key1, Range> val2);
            foreach(Key2 tmp in incoming) {
              ConcurrentDictionary<Key1,Range> tmp2 = new ConcurrentDictionary<Key1, Range> { [key] = value };
              _second_first_dict[tmp] = tmp2;
            }

            _no_entries.TryRemove(key,out val);
        }

        private void Remove(Key1 key)
        {
            List<Key2> expired = new List<Key2>();
            foreach (KeyValuePair<Key2, ConcurrentDictionary<Key1, Range>> tmp in _second_first_dict) {
                tmp.Value.TryRemove(key, out Range val);
                if (0 >= tmp.Value.Count) expired.Add(tmp.Key);
            }
            foreach (Key2 tmp in expired) _second_first_dict.TryRemove(tmp, out ConcurrentDictionary<Key1, Range> val2);
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
