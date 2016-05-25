using System;
using System.Collections.Generic;

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
        readonly private Dictionary<Key1, Range> _no_entries;
        readonly private Dictionary<Key1, Dictionary<Key2, Range>> _first_second_dict;
        readonly private Dictionary<Key2, Dictionary<Key1, Range>> _second_first_dict;

        public Ary2Dictionary() {
            _no_entries = new Dictionary<Key1, Range>();
            _first_second_dict = new Dictionary<Key1, Dictionary<Key2, Range>>();
            _second_first_dict = new Dictionary<Key2, Dictionary<Key1, Range>>();
        }

        public bool HaveEverSeen(Key1 key, out Range value) {
            if (_no_entries.TryGetValue(key, out value)) return true;
            if (_first_second_dict.ContainsKey(key)) {
                foreach (Range tmp in _first_second_dict[key].Values) {
                    value = tmp;
                    return true;
                }
            }
            return false;
        }

        // Yes, value copy for these two
        public Dictionary<Key2,Range> WhatIsAt(Key1 key) {
            if (_first_second_dict.ContainsKey(key)) return new Dictionary<Key2, Range>(_first_second_dict[key]);
            return null;
        }

        public Dictionary<Key1, Range> WhereIs(Key2 key)
        {
            if (_second_first_dict.ContainsKey(key)) return new Dictionary<Key1, Range>(_second_first_dict[key]);
            return null;
        }

        public List<Key2> WhatHaveISeen() {
            return new List<Key2>(_second_first_dict.Keys);
        }

        public void Set(Key1 key, IEnumerable<Key2> keys2, Range value) {
            if (null == keys2) {
                if (_first_second_dict.ContainsKey(key))  Remove(keys2, key);
                _first_second_dict.Remove(key);
                _no_entries[key] = value;
                return;
            }

            // there was a pre-existing entry.  Take a set difference and update.
            bool at_least_one_key2 = false;
            HashSet<Key2> removed = (_first_second_dict.ContainsKey(key) ? new HashSet<Key2>(_first_second_dict[key].Keys) : new HashSet<Key2>());
            foreach (Key2 tmp in keys2) {
                removed.Remove(tmp);
                at_least_one_key2 = true;
            }
            if (!at_least_one_key2) { // keys2 morally null
                if (_first_second_dict.ContainsKey(key))  Remove(keys2, key);
                _first_second_dict.Remove(key);
                _no_entries[key] = value;
                return;
            }

            if (0 < removed.Count) {
                Remove(removed, key);
                foreach(Key2 tmp in removed) {
                    _first_second_dict[key].Remove(tmp);
                }
            }
#if FAIL
            if (0 < changed.Count)
            {
                Update(changed, key, value);
                if (!_first_second_dict.ContainsKey(key)) _first_second_dict[key] = new Dictionary<Key2, Range>(changed.Count);
                foreach (Key2 tmp in changed)
                {
                    _first_second_dict[key][tmp] = value;
                }
            }
#else
            Update(keys2, key, value);
            if (!_first_second_dict.ContainsKey(key)) _first_second_dict[key] = new Dictionary<Key2, Range>();
            foreach (Key2 tmp in keys2) {
                _first_second_dict[key][tmp] = value;
            }
#endif
            _no_entries.Remove(key);
        }

        private void Remove(IEnumerable<Key2> src, Key1 key) {
            foreach(Key2 tmp in src) {
                _second_first_dict[tmp].Remove(key);
                if (0 == _second_first_dict[tmp].Count) _second_first_dict.Remove(tmp);
            }
        }

        private void Update(IEnumerable<Key2> src, Key1 key, Range value) {
            foreach (Key2 tmp in src) {
                if (!_second_first_dict.ContainsKey(tmp)) _second_first_dict[tmp] = new Dictionary<Key1, Range>(1);
                _second_first_dict[tmp][key] = value;
            }
        }
    }
}
