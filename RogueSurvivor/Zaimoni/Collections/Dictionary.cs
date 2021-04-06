// #define IRRATIONAL_CAUTION
// #define IRRATIONAL_PANIC

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization;
using Zaimoni.Data;
#if DEBUG
using System.Linq;
#endif

#nullable enable

// re-implementation of System.Collections.Generic.Dictionary
// cloning: https://referencesource.microsoft.com/#mscorlib/system/collections/generic/dictionary.cs,cc27fcdd81291584,references

namespace Zaimoni.Collections
{
    [Serializable]
    public class Dictionary<Key, Value> : IDictionary<Key, Value>, IDictionary, IReadOnlyDictionary<Key, Value>, ISerializable, IDeserializationCallback, Fn_to_s
    {
        private const string COLLECTION_WAS_MODIFIED = "enumeration failed: Collection was modified";

        [Serializable]
        private struct Entry : Fn_to_s
        {
            [NonSerialized] public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            [NonSerialized] public int prev;        // Index of previous entry, -1 if first
            [NonSerialized] public int next;        // Index of next entry, -1 if last
            [NonSerialized] public int parent;      // Index of parent of entry, -1 if root
            [NonSerialized] public uint depth;      // cache field: scapegoat depth of subtree rooted here
            // serialize these two, not the above fields
            public Key key;         // Key of entry
            public Value value;     // Value of entry

            public void Clear() {
                hashCode = -1;
                prev = -1;
                next = -1;
                parent = -1;
                depth = 0;
                key = default;
                value = default;
            }

            public void Deallocate(int free)
            {
                hashCode = -1;
                prev = -1;
                next = free;
                parent = -1;
                depth = 0;
                key = default;  // must trigger GC here
                value = default;
            }

            public void NewLeaf(int hash)
            {
                hashCode = hash;
                prev = -1;
                next = -1;
                parent = -1;
                depth = 1;
            }

            public void ValueCopy(in Entry src) {
                hashCode = src.hashCode;
                key = src.key;
                value = src.value;
            }

            public KeyValuePair<Key, Value> to_KV(out int hash) {
                hash = hashCode;
                return new KeyValuePair<Key, Value>(key, value);
            }

            public string to_s() {
                return "[" + (key?.to_s() ?? "null") + ", " + (value?.to_s() ?? "null") + ", " + hashCode.ToString() + ", " + prev.ToString() + ", " + next.ToString() + ", " + parent.ToString() + ", " + depth.ToString() + "]";
            }
        }


        [OptionalField] private Entry[] entries;
        [OptionalField] public readonly IEqualityComparer<Key> Comparer;
        [NonSerialized] private int count;
        [NonSerialized] private int activeList;
        [NonSerialized] private int version;
        [NonSerialized] private int freeList;
        [NonSerialized] private int freeCount;
        [NonSerialized] private KeyCollection? keys; // should be non-serialized
        [NonSerialized] private ValueCollection? values; // should be non-serialized
        [NonSerialized] private object? _syncRoot; // should be non-serialized

        public Dictionary() : this(0, null) { }
        public Dictionary(int capacity) : this(capacity, null) { }
        public Dictionary(IEqualityComparer<Key> comp) : this(0, comp) { }

        public Dictionary(int capacity, IEqualityComparer<Key>? comp)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            entries = new Entry[capacity];
            activeList = -1;
            count = 0;
            freeList = -1;
            freeCount = 0;
            version = 0;
            Comparer = comp ?? EqualityComparer<Key>.Default;
        }

        public Dictionary(IDictionary<Key, Value> dictionary) : this(dictionary, null) { }

        public Dictionary(IDictionary<Key, Value>? dictionary, IEqualityComparer<Key>? comp) :
            this(dictionary?.Count ?? 0, comp)
        {
            if (null != dictionary)
            {
                foreach (var pair in dictionary) Add(pair.Key, pair.Value);
            }
        }

#region save/load support
        protected Dictionary(SerializationInfo info, StreamingContext context)
        {
            info.read(ref Comparer, "m_Comparer");
            Comparer ??= EqualityComparer<Key>.Default;

            info.read(ref entries, "m_Entries");
            entries ??= new Entry[0];

            count = entries.Length;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (EqualityComparer<Key>.Default != Comparer) info.AddValue("m_Comparer", Comparer);
            var ub = Count;
            if (0 < ub) {
                var relay = new Entry[ub];
                var index = 0;
                var i = 0;
                while (i < count) {
                    ref var staging = ref entries[i++];
                    if (0 <= staging.hashCode) relay[index++] = staging;
                }
                info.AddValue("m_Entries", relay);
            }
        }

        [OnDeserializing] private void onDeserializing()
        {   // safe defaults
            activeList = -1;
            count = 0;
            version = 0;
            freeList = -1;
            freeCount = 0;
        }

        void IDeserializationCallback.OnDeserialization(object? sender)
        {
            var ub = count;
            var i = 0;
            count = 0; // reset so calling Add works
            while (i < ub) {
                ref var staging = ref entries[i++];
                Add(staging.key, staging.value);
            }
        }
#endregion

        public int Count { get { return count - freeCount; } }

        public KeyCollection Keys { get { return keys ??= new KeyCollection(this); } }
        ICollection<Key> IDictionary<Key, Value>.Keys { get { return keys ??= new KeyCollection(this); } }
        IEnumerable<Key> IReadOnlyDictionary<Key, Value>.Keys { get { return keys ??= new KeyCollection(this); } }

        public ValueCollection Values { get { return values ??= new ValueCollection(this); } }
        ICollection<Value> IDictionary<Key, Value>.Values { get { return values ??= new ValueCollection(this); } }
        IEnumerable<Value> IReadOnlyDictionary<Key, Value>.Values { get { return values ??= new ValueCollection(this); } }

        public string to_s() {
            var ub = Count;
            if (0 >= ub) return "{}";
            var tmp = new List<string>(ub);
            foreach (var iter in this)
            {
                tmp.Add(iter.Key.to_s() + ":" + iter.Value.to_s());
            }
            tmp[0] = "{" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "} (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        [Conditional("DEBUG")]
        private void _RequireDereferenceableIndex(int n)
        {
            if (0 > n || count <= n) throw new InvalidOperationException("internal index out of bounds");
            if (0 > entries[n].hashCode) throw new InvalidOperationException("considering dereferencing into free list");
        }

        [Conditional("IRRATIONAL_CAUTION")]
        private void _RequireDereferenceableIndexPrecondition(int n) => _RequireDereferenceableIndex(n);

        [Conditional("DEBUG")]
        private void _RequireValidIndex(int n)
        {
            if (count <= n) throw new InvalidOperationException("internal index invalid");
        }

        [Conditional("IRRATIONAL_CAUTION")]
        private void _RequireValidIndexPrecondition(int n) => _RequireValidIndex(n);

        [Conditional("DEBUG")]
        private void _RequireValid(int n)
        {
            if (0 > n) return;
            ref var test = ref entries[n];
            if (0 > test.hashCode) return;
            if (test.hashCode != (Comparer.GetHashCode(test.key) & 0x7FFFFFFF)) throw new InvalidOperationException("corrupt hashcode");
            if (0 > test.parent) {
                if (activeList != n) throw new InvalidOperationException("invalid activeList");
            } else {
                if (activeList == n) throw new InvalidOperationException("invalid activeList #2");
                _RequireDereferenceableIndex(test.parent);
                ref var parent = ref entries[test.parent];
                if (parent.prev != n && parent.next != n) throw new InvalidOperationException("parent has disowned me");
                if (parent.prev == n && parent.next == n) throw new InvalidOperationException("parent has twinned me");
            }
            if (0 <= test.next) {
                _RequireDereferenceableIndex(test.next);
                ref var _next = ref entries[test.next];
                if (_next.parent != n) throw new InvalidOperationException("backlink failed");
                if (test.hashCode > _next.hashCode) throw new InvalidCastException("unreachable: "+test.next.ToString());
            }
            if (0 <= test.prev) {
                _RequireDereferenceableIndex(test.prev);
                ref var _prev = ref entries[test.prev];
                if (_prev.parent != n) throw new InvalidOperationException("backlink failed #2");
                if (test.hashCode < _prev.hashCode) throw new InvalidCastException("unreachable #2: " + test.prev.ToString());
            }
            if (count < test.depth) throw new InvalidOperationException("uncredible subtree depth");
        }

        [Conditional("IRRATIONAL_PANIC")]
        private void _RequireGlobalValid()
        {
            var ub = count;
            while (0 <= --ub) if (0 <= entries[ub].hashCode) _RequireValid(ub);
        }

        [Conditional("DEBUG")]
        private void _RequireContainsKey(Key key)
        {
            int code = 0;
            if (!Keys.Contains(key)) code += 1;
            if (0 > FindEntry(key, out var root, out var leaf)) code += 2;
            var actual = Array.FindIndex(entries, x => Comparer.Equals(x.key, key));
            if (0 > actual) code += 4;
            else if (entries[actual].parent == leaf) code += 8;
            // for this to not crash, we need the base case for to_s to simulate a virtual member function call against
            // the private type Zaimoni.Collections.Dictionary::Entry
            if (0 < code) throw new InvalidOperationException("Key AWOL: " + key.to_s() + "; " + code.ToString() + ", " + Count.ToString() + "\n anchor: " + activeList.ToString() + "; " + root.ToString() + ", " + leaf.ToString() + ", " + actual.ToString() + "\n" + entries.to_s());
            _RequireGlobalValid();
        }

        [Conditional("DEBUG")]
        private void _RequireDoesNotContainsKey(Key key)
        {
            int code = 0;
            if (Keys.Contains(key)) code += 1;
            if (0 <= FindEntry(key, out var root, out var leaf)) code += 2;
            var actual = Array.FindIndex(entries, x => Comparer.Equals(x.key, key));
            if (0 <= actual) code += 4;
            // for this to not crash, we need the base case for to_s to simulate a virtual member function call against
            // the private type Zaimoni.Collections.Dictionary::Entry
            if (0 < code) throw new InvalidOperationException("Key party-crashing: " + key.to_s() + "; " + code.ToString() + ", " + Count.ToString() + "\n anchor: " + activeList.ToString() + "; " + root.ToString() + ", " + leaf.ToString() + ", " + actual.ToString() + "\n" + entries.to_s());
            _RequireGlobalValid();
        }

        public Value this[Key key]
        {
            get {
                int i = FindEntry(key);
                if (0 > i) throw new KeyNotFoundException("key not found: " + key.ToString());
                return entries[i].value;
            }
            set { Insert(key, value, false); }
        }

        public void Add(Key key, Value value) => Insert(key, value, true);
        void ICollection<KeyValuePair<Key, Value>>.Add(KeyValuePair<Key, Value> keyValuePair) => Add(keyValuePair.Key, keyValuePair.Value);

        bool ICollection<KeyValuePair<Key, Value>>.Contains(KeyValuePair<Key, Value> keyValuePair)
        {
            int i = FindEntry(keyValuePair.Key);
            return 0 <= i && EqualityComparer<Value>.Default.Equals(entries[i].value, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<Key, Value>>.Remove(KeyValuePair<Key, Value> keyValuePair)
        {
            int i = FindEntry(keyValuePair.Key);
            if (0 <= i && EqualityComparer<Value>.Default.Equals(entries[i].value, keyValuePair.Value)) {
                Remove(i);
                _RequireDoesNotContainsKey(keyValuePair.Key);
                return true;
            }
            return false;
        }

        public bool Remove(Key key)
        {
            int i = FindEntry(key);
            if (0 <= i) {
                Remove(i);
                _RequireDoesNotContainsKey(key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (0 <= activeList) {
                Interlocked.Increment(ref version);
                while (0 < count) {
                    ref var staging = ref entries[--count];
                    if (0 <= staging.hashCode) staging.Clear();
                }
                activeList = -1;
                count = 0;
                freeList = -1;
                freeCount = 0;
            }
        }

        public bool ContainsKey(Key key) => 0 <= FindEntry(key);

        public bool ContainsValue(Value value) {
            var ub = count;
            if (value == null) {
                while (0 < ub) {
                    ref var staging = ref entries[--ub];
                    if (0 <= staging.hashCode && null == staging.value) return true;
                }
            } else {
                var c = EqualityComparer<Value>.Default;
                while (0 < ub) {
                    ref var staging = ref entries[--ub];
                    if (0 <= staging.hashCode && c.Equals(staging.value, value)) return true;
                }
            }
            return false;
        }

        public Enumerator GetEnumerator() => new(this, Enumerator.KeyValuePair);
        IEnumerator<KeyValuePair<Key, Value>> IEnumerable<KeyValuePair<Key, Value>>.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        public bool TryGetValue(Key key, out Value value)
        {
            int i = FindEntry(key);
            if (0 <= i) {
                value = entries[i].value;
                return true;
            }
            value = default;
            return false;
        }

        internal Value? GetValueOrDefault(Key key) {
            int i = FindEntry(key);
            return 0 <= i ? entries[i].value : default;
        }

        bool ICollection<KeyValuePair<Key,Value>>.IsReadOnly { get { return false; } }
        void ICollection<KeyValuePair<Key,Value>>.CopyTo(KeyValuePair<Key,Value>[] array, int index) => CopyTo(array, index);

        void ICollection.CopyTo(Array array, int index) {
            if (null == array) throw new ArgumentNullException(nameof(array));
            if (1 != array.Rank) throw new ArgumentException("won't copy into multi-dimensional array", nameof(array));
            if( 0 != array.GetLowerBound(0)) throw new ArgumentException("array should have zero lower bound", nameof(array));
            if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "not in bounds");
            if (array.Length - index < Count) throw new ArgumentException("array not large enough");

            if (array is KeyValuePair<Key, Value>[] pairs) {
                CopyTo(pairs, index);
            } else if(array is DictionaryEntry[] dictEntryArray) {
                int i = 0;
                while (i < count) {
                    ref var staging = ref entries[i++];
                    if (0 <= staging.hashCode) {
                        dictEntryArray[index++] = new DictionaryEntry(staging.key, staging.value);
                    }
                }
            } else if (array is object[] objects) {
                try {
                    int i = 0;
                    while (i < count) {
                        ref var staging = ref entries[i++];
                        if (0 <= staging.hashCode) {
                            objects[index++] = new DictionaryEntry(staging.key, staging.value);
                        }
                    }
                } catch (ArrayTypeMismatchException e) {
                    throw new ArgumentException("invalid array type", nameof(array), e);
                }
            } else {
                throw new ArgumentException("invalid array type", nameof(array));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);
        bool ICollection.IsSynchronized { get { return false; } }

        object ICollection.SyncRoot { get {
                if (_syncRoot == null) {
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        bool IDictionary.IsFixedSize { get { return false; } }
        bool IDictionary.IsReadOnly { get { return false; } }

        ICollection IDictionary.Keys { get { return Keys; } }
        ICollection IDictionary.Values { get { return Values; } }

        object? IDictionary.this[object key]
        {
            get {
                if (IsCompatibleKey(key)) {
                    int i = FindEntry((Key)key);
                    if (0 <= i) return entries[i].value;
                }
                return null;
            }

            set {
                if (null == key) throw new ArgumentNullException(nameof(key));
//              ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

                try {
                    Key tempKey = (Key)key;
                    try {
                        this[tempKey] = (Value)value;
                    } catch (InvalidCastException e) {
                        throw new ArgumentException("invalid value", e);
                    }
                } catch (InvalidCastException e) {
                    throw new ArgumentException("invalid key", e);
                }
            }
        }

        void IDictionary.Add(object key, object? value) {
            if (null == key) throw new ArgumentNullException(nameof(key));
//          ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

            try {
                Key tempKey = (Key)key;

                try {
                    Add(tempKey, (Value)value);
                } catch (InvalidCastException e) {
                    throw new ArgumentException("invalid value", e);
                }
            } catch (InvalidCastException e) {
                throw new ArgumentException("invalid key", e);
            }
        }

        bool IDictionary.Contains(object key) {
            if(IsCompatibleKey(key)) {
                return ContainsKey((Key)key);
            }
            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

        void IDictionary.Remove(object key) {
            if (IsCompatibleKey(key)) Remove((Key)key);
        }

        // private access region

        private void CopyTo(KeyValuePair<Key,Value>[] array, int index) {
            if (null == array) throw new ArgumentNullException(nameof(array));
            if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "not in bounds");
            if (array.Length - index < Count) throw new ArgumentException("array not large enough");

            int i = count;
            while (i < count) {
                ref var staging = ref entries[i++];
                if (0 <= staging.hashCode) array[index++] = new KeyValuePair<Key, Value>(staging.key, staging.value);
            }
        }

        private void Remove(int doomed) {
            _RequireDereferenceableIndexPrecondition(doomed);
            Interlocked.Increment(ref version); // break enumerators
retry:
            ref var staging = ref entries[doomed];
#if DEBUG
            if (-1 == staging.parent && activeList!=doomed) throw new InvalidOperationException("tree head out of sync");
#endif

            if (0 > staging.prev && 0 > staging.next) _excise(staging.parent, doomed, -1);
            else if (0 > staging.prev && 0 <= staging.next) _excise(staging.parent, doomed, staging.next);
            else if (0 <= staging.prev && 0 > staging.next) _excise(staging.parent, doomed, staging.prev);
            else { // not so simple
                var successor = InOrderSuccessor(doomed, out var depth_successor);
                var predecessor = InOrderPredecessor(doomed, out var depth_predecessor);

                if (depth_predecessor <= depth_successor) { // remove deeper(?) successor
                    entries[doomed].ValueCopy(in entries[successor]);
                    doomed = successor; // simulate tail-call
                    goto retry;
                } else { // remove deeper predecessor
                    entries[doomed].ValueCopy(in entries[predecessor]);
                    doomed = predecessor; // simulate tail-call
                    goto retry;
                }
            }

            // wrap-up
            staging.Deallocate(freeList);

            freeList = doomed;
            freeCount++;
            if (0 == Count) {
                activeList = -1;
                count = 0;
                freeList = -1;
                freeCount = 0;
            }
        }

        private int FindEntry(Key key)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            if (0 > activeList) return -1;

            return FindEntry(key, stackalloc int[2]);
        }

        private int FindEntry(Key key, out int root, out int leaf)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            root = -1;
            leaf = -1;
            if (0 > activeList) return -1;

            int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

            return _findEntryTree(key, hashCode, activeList, ref root, ref leaf);
        }

        private int FindEntry(Key key, Span<int> root_leaf)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            root_leaf[0] = -1;
            root_leaf[1] = -1;
            if (0 > activeList) return -1;

            int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

            return _findEntryTree(key, hashCode, activeList, ref root_leaf[0], ref root_leaf[1]);
        }

        private int _findEntryTree(Key key, int hashCode, int scan, ref int root, ref int leaf)
        {
#if IRRATIONAL_CAUTION
            void gave_up_too_soon(int leaf)
            {
                if (0 <= leaf) {
                    ref var _leaf = ref entries[leaf];
                    if (Comparer.Equals(_leaf.key, key)) throw new InvalidOperationException("gave up too soon #1");
                    if (0 <= _leaf.prev && Comparer.Equals(entries[_leaf.prev].key, key)) throw new InvalidOperationException("gave up too soon #2\n" + _leaf.to_s() + "\n" + entries[_leaf.prev].to_s() + "\n" + (0 > _leaf.parent ? "" : entries[_leaf.parent].to_s()));
                    if (0 <= _leaf.next && Comparer.Equals(entries[_leaf.next].key, key)) throw new InvalidOperationException("gave up too soon #3\n" + _leaf.to_s()+"\n"+ entries[_leaf.next].to_s()+"\n" + (0 > _leaf.parent ? "" : entries[_leaf.parent].to_s()));
                }
            }
#endif

            while (0 <= scan) {
                _RequireDereferenceableIndexPrecondition(scan);
                ref var _scan = ref entries[scan];

                var code = hashCode.CompareTo(_scan.hashCode); // profiles faster than direct comparison pair
                if (0 < code) { // greater than
                    if (0 > _scan.next) {
                        root = scan;
                        leaf = -1;
                        return -1;
                    }
                    scan = _scan.next;
                } else if (0 > code) { // less than
                    if (0 > _scan.prev) {
                        root = scan;
                        leaf = -1;
                        return -1;
                    }
                    scan = _scan.prev;
                } else /* if (hashCode == _scan.hashCode) */ {
                    if (Comparer.Equals(_scan.key, key)) {
                        root = scan;
                        leaf = scan;
                        return scan;
                    }
                    bool no_next = 0 > _scan.next;
                    if (0 > _scan.prev) {
                        if (no_next) {
                            root = scan;
                            leaf = -1;
                            return -1;
                        }
                        scan = _scan.next;
                        continue;
                    } else if (no_next) {
                        scan = _scan.prev;
                        continue;
                    }
                    int proxy_root1 = -1;
                    int proxy_leaf1 = -1;
                    int candidate;
                    if (0 <= (candidate = _findEntryTree(key, hashCode, _scan.prev, ref proxy_root1, ref proxy_leaf1))) {
                        root = proxy_root1;
                        leaf = proxy_leaf1;
                        return candidate;
                    }

                    int proxy_root2 = -1;
                    int proxy_leaf2 = -1;
                    if (0 <= (candidate = _findEntryTree(key, hashCode, _scan.next, ref proxy_root2, ref proxy_leaf2))) {
                        root = proxy_root2;
                        leaf = proxy_leaf2;
                        return candidate;
                    }

                    if (0 > proxy_leaf1) {
                        root = proxy_root1;
                        leaf = -1;
                        return -1;
                    }
                    if (0 > proxy_leaf2) {
                        root = proxy_root2;
                        leaf = -1;
                        return -1;
                    }
                    // leftward bias
#if IRRATIONAL_CAUTION
                    gave_up_too_soon(proxy_leaf1);
                    gave_up_too_soon(proxy_leaf2);
#endif
                    root = proxy_root1;
                    leaf = proxy_leaf1;
                    return -1;
                }
            }
#if IRRATIONAL_CAUTION
            gave_up_too_soon(leaf);
#endif
            return -1;
        }

        private void Insert(Key key, Value value, bool add)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));

            int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

            if (0 > activeList) {
#if DEBUG
                if (0 < count) throw new InvalidOperationException("non-empty dictionary that looks empty; "+count.ToString());
#endif
                Interlocked.Increment(ref version);
                if (0 == entries.Length) entries = new Entry[15];
                ref var staging = ref entries[0];
                staging.NewLeaf(hashCode);
                staging.key = key;
                staging.value = value;
                count = 1;
                activeList = 0;
                freeCount = 0;
                freeList = -1;
                _RequireContainsKey(key);
                return;
            }

            // find insertion point
            Span<int> root_leaf = stackalloc int[2];
            var index = FindEntry(key, root_leaf);
            if (0 <= index) {
                if (add) throw new ArgumentException("adding duplicate", nameof(key));
                Interlocked.Increment(ref version);
                entries[index].value = value;
                return;
            }

            _RequireDereferenceableIndex(root_leaf[0]);

#if DEBUG
            var old_count = Count;
#endif

            if (0 <= freeList) {
                index = freeList;
                freeList = entries[index].next;
                if (0 > freeList) freeCount = 0;
                else --freeCount;
#if DEBUG
                if (old_count + 1 != Count) throw new InvalidOperationException("external count wiped out: "+old_count.ToString()+", "+Count.ToString());
#endif
            } else {
                if (count == entries.Length) Resize();
                index = count;
                count++;
#if DEBUG
                if (old_count + 1 != Count) throw new InvalidOperationException("external count wiped out #2: " + old_count.ToString() + ", " + Count.ToString());
#endif
            }

            entries[index].prev = -1;
            entries[index].next = -1;
            entries[index].depth = 1;
            entries[index].key = key;
            entries[index].value = value;
            entries[index].parent = root_leaf[0];

            Interlocked.Increment(ref version); // break enumerators
            entries[index].hashCode = hashCode;

            ref var _root = ref entries[root_leaf[0]];
            if (hashCode <= _root.hashCode && -1 == _root.prev) {
                if (_link_prev(root_leaf[0], index)) _scapegoat_rebuild(root_leaf[0]);

                _RequireContainsKey(key);
                return;
            }
            if (hashCode >= _root.hashCode && -1 == _root.next) {
                if (_link_next(root_leaf[0], index)) _scapegoat_rebuild(root_leaf[0]);

                _RequireContainsKey(key);
                return;
            }
            if (hashCode <= _root.hashCode) {
                bool want_rebuild_index = (hashCode >= entries[_root.prev].hashCode)
                                        ? _link_prev(index, _root.prev)
                                        : _link_next(index, _root.prev);
                bool want_rebuild_anchor = _link_prev(root_leaf[0], index);

                if (want_rebuild_index) _scapegoat_rebuild(index);
                else if (want_rebuild_anchor) _scapegoat_rebuild(root_leaf[0]);

                _RequireContainsKey(key);
                return;
            } else /* if (hashCode >= _root.hashCode) */ {
                bool want_rebuild_index = (hashCode >= entries[_root.next].hashCode)
                                        ? _link_prev(index, _root.next)
                                        : _link_next(index, _root.next);
                bool want_rebuild_anchor = _link_next(root_leaf[0], index);

                if (want_rebuild_index) _scapegoat_rebuild(index);
                else if (want_rebuild_anchor) _scapegoat_rebuild(root_leaf[0]);

                _RequireContainsKey(key);
                return;
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            return key is Key;
        }

        private void Resize() => Resize(0 == entries.Length ? 15 : 2 * entries.Length + 1);

        private void Resize(int newSize)
        {
            if (newSize <= entries.Length) return; // no-op; might make sense to actively shrink but we can force-copy for that

            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            entries = newEntries;
        }

#region binary tree support
        uint _scapegoat_depth(int root) {
            if (0 > root) return 0;
            var ret = entries[root].depth;
#if DEBUG
            if (count < ret) throw new InvalidOperationException("uncredible subtree size: "+ret.ToString()+" for "+root.ToString()+"\n"+entries.to_s());
#endif
            return ret;
        }

        bool _update_depth(int root)
        {
            ref var _root = ref entries[root];
            var new_depth = Math.Min(_scapegoat_depth(_root.prev), _scapegoat_depth(_root.next)) + 1;
            if (new_depth != _root.depth) {
                _root.depth = new_depth;
                return true;
            };
            return false;
        }

        bool _link_prev(int anchor, int leaf) {
            _RequireDereferenceableIndexPrecondition(anchor);
            _RequireDereferenceableIndexPrecondition(leaf);
            entries[leaf].parent = anchor;
            entries[anchor].prev = leaf;
            return _update_depth(anchor);
        }

        bool _link_next(int anchor, int leaf) {
            _RequireDereferenceableIndexPrecondition(anchor);
            _RequireDereferenceableIndexPrecondition(leaf);
            entries[leaf].parent = anchor;
            entries[anchor].next = leaf;
            return _update_depth(anchor);
        }

        void _scapegoat_rebuild(int root) {
            if (0 > root) return;
            _RequireDereferenceableIndexPrecondition(root);

            bool update(ref int root) {
                ref var _root = ref entries[root];
retry:
                var prev_depth = _scapegoat_depth(_root.prev);
                var next_depth = _scapegoat_depth(_root.next);

                var working = Math.Max(prev_depth, next_depth) + 1;
#if DEBUG
                if (count < working) throw new InvalidOperationException("uncredible subtree depth: " + working.ToString() + "; " + _root.to_s() + "\n" + entries.to_s());
#endif
                _root.depth = working;

                if (wantRotateNext(root)) {
                    _rotate_next_up(root);
                    goto retry; // simulate tail call of _scapegoat_size_rebuild from _rotate_next_up
                }

                if (wantRotatePrev(root)) {
                    _rotate_prev_up(root);
                    goto retry; // simulate tail call of _scapegoat_size_rebuild from _rotate_prev_up
                }

#if PROTOTYPE
                // we'll run out of RAM before this can overflow
                if (prev_depth < next_depth) {
                    if (prev_depth + 2 <= next_depth) {
                        var inorder_predecessor = InOrderPredecessor(root, out var pred_depth);
                        var inorder_successor = InOrderSuccessor(root, out var succ_depth);
#if DEBUG
                        // backup root; copy successor to root; delete successor, insert copy of root as child of inorder predecessor
                        var backup = _root.to_KV(out var backup_hash);
                        ref var staging = ref entries[inorder_successor];
                        _root.ValueCopy(in staging);
                        _excise(staging.parent, inorder_successor, staging.next);
                        staging.NewLeaf(backup_hash);
                        staging.key = backup.Key;
                        staging.value = backup.Value;

                        staging.parent = inorder_predecessor;
                        if (inorder_predecessor == root) {
                            entries[root].prev = inorder_successor;
                            goto retry;
                        } else {
                            entries[inorder_predecessor].next = inorder_successor;
                            _scapegoat_rebuild(inorder_predecessor);
                            goto retry;
                        }
#else
                        throw new InvalidOperationException("want rebalance: " + prev_depth.ToString() + ", " + next_depth.ToString() + "\n" + inorder_predecessor.ToString() + ", " + inorder_successor.ToString() + ", " + pred_depth.ToString() + ", " + succ_depth.ToString());
#endif
                    }
                } else if (prev_depth > next_depth) {
                    if (next_depth + 2 <= prev_depth) {
                        var inorder_predecessor = InOrderPredecessor(root, out var pred_depth);
                        var inorder_successor = InOrderSuccessor(root, out var succ_depth);
#if DEBUG
                        // backup root; copy predecessor to root; delete predecessor, insert copy of root as child of inorder successor
                        var backup = _root.to_KV(out var backup_hash);
                        ref var staging = ref entries[inorder_predecessor];
                        _root.ValueCopy(in staging);
                        _excise(staging.parent, inorder_predecessor, staging.prev);
                        staging.NewLeaf(backup_hash);
                        staging.key = backup.Key;
                        staging.value = backup.Value;

                        staging.parent = inorder_successor;
                        if (inorder_successor == root) {
                            entries[root].next = inorder_predecessor;
                            goto retry;
                        } else {
                            entries[inorder_successor].prev = inorder_predecessor;
                            _scapegoat_rebuild(inorder_successor);
                            goto retry;
                        }
#else
                        throw new InvalidOperationException("want rebalance #2: " + prev_depth.ToString() + ", " + next_depth.ToString() + "\n" + inorder_predecessor.ToString() + ", " + inorder_successor.ToString() + ", " + pred_depth.ToString() + ", " + succ_depth.ToString());
#endif
                    }
                }
#endif

                if (0 <= _root.parent) {
                  root = _root.parent;
                  return true;
                }
                return false;
            }

            while (update(ref root)) ;
        }

        bool _relink(int host, int doomed, int target)
        {
            _RequireDereferenceableIndex(host); // just in case we're assigning, when it's mandatory
            _RequireValidIndexPrecondition(target);
            ref var parent = ref entries[host];
            if (doomed == parent.prev) parent.prev = target;
#if DEBUG
            else if (doomed != parent.next) throw new InvalidOperationException("child not actually linked to parent");
#endif
            else parent.next = target;
            if (0 <= target) {
                _RequireDereferenceableIndexPrecondition(target);
                entries[target].parent = host;
            }
            return _update_depth(host);
        }

        void _excise(int host, int doomed, int target)
        {
            _RequireValidIndexPrecondition(host);
            _RequireValidIndexPrecondition(target);
            if (0 <= host) {
                if (_relink(host, doomed, target)) _scapegoat_rebuild(host);
            } else if (0 <= target) {
                entries[activeList = target].parent = -1;
            }
            _RequireValid(host);
            _RequireValid(target);
        }

        int InOrderSuccessor(int root, out int depth)
        {
            _RequireDereferenceableIndexPrecondition(root);
            depth = 0;
            var scan = entries[root].next;
            if (0 > scan) return root;
            ++depth;
            while (0 <= entries[scan].prev) {
                scan = entries[scan].prev;
                ++depth;
            }
            return scan;
        }

        int InOrderPredecessor(int root, out int depth)
        {
            _RequireDereferenceableIndexPrecondition(root);
            depth = 0;
            var scan = entries[root].prev;
            if (0 > scan) return root;
            ++depth;
            while (0 <= entries[scan].next) {
                scan = entries[scan].next;
                ++depth;
            }
            return scan;
        }

        /*
                Rotation psuedocode: https://en.wikipedia.org/wiki/Tree_rotation
                Pivot = Root.OS
                Root.OS = Pivot.RS
                Pivot.RS = Root
                Root = Pivot

                also, parent of root-before is parent of pivot-after
        */
        void _rotate_prev_up(int root) {
            _RequireDereferenceableIndexPrecondition(root);
            ref var _root = ref entries[root];
            var anchor = _root.parent;
            var pivot = _root.prev;
            _RequireDereferenceableIndexPrecondition(pivot);
            ref var _pivot = ref entries[pivot];
            _RequireValidIndexPrecondition(_pivot.next);
            _root.prev = _pivot.next;
            if (0 <= _pivot.next) entries[_pivot.next].parent = root;
            _pivot.next = root;
            _root.parent = pivot;
            if (-1 == (_pivot.parent = anchor)) activeList = pivot;
            else _relink(anchor, root, pivot);
            _RequireValid(root);
            _RequireValid(anchor);
            _RequireValid(pivot);
            _RequireGlobalValid();
//          _scapegoat_rebuild(root); // caller does this
        }

        void _rotate_next_up(int root) {
            _RequireDereferenceableIndexPrecondition(root);
            ref var _root = ref entries[root];
            var anchor = _root.parent;
            var pivot = _root.next;
            _RequireDereferenceableIndexPrecondition(pivot);
            ref var _pivot = ref entries[pivot];
            _RequireValidIndexPrecondition(_pivot.prev);
            _root.next = _pivot.prev;
            if (0 <= _pivot.prev) entries[_pivot.prev].parent = root;
            _pivot.prev = root;
            _root.parent = pivot;
            if (-1 == (_pivot.parent = anchor)) activeList = pivot;
            else _relink(anchor, root, pivot);
            _RequireValid(root);
            _RequireValid(anchor);
            _RequireValid(pivot);
            _RequireGlobalValid();
//          _scapegoat_rebuild(root); // caller does this
        }

        bool wantRotatePrev(int root)
        {
            _RequireDereferenceableIndexPrecondition(root);
            ref var _root = ref entries[root];
            if (0 > _root.prev) return false;
            _RequireDereferenceableIndexPrecondition(_root.prev);

            ref var _pivot = ref entries[_root.prev];
//          var height_pivot_swap = _scapegoat_depth(_pivot.next); // this remains at the same height-offset before-after
            var height_pivot_other = _scapegoat_depth(_pivot.prev);
            if (0 == height_pivot_other) return false;

            var height_root_other = _scapegoat_depth(_root.next);
            if (0 == height_root_other) return 1 <= height_pivot_other;

            if (height_root_other >= height_pivot_other) return false;
            return 1 <= height_pivot_other - height_root_other;
        }

        bool wantRotateNext(int root)
        {
            _RequireDereferenceableIndexPrecondition(root);
            ref var _root = ref entries[root];
            if (0 > _root.next) return false;
            _RequireDereferenceableIndexPrecondition(_root.next);

            ref var _pivot = ref entries[_root.next];
//          var height_pivot_swap = _scapegoat_depth(_pivot.prev); // this remains at the same height-offset before-after
            var height_pivot_other = _scapegoat_depth(_pivot.next);
            if (0 == height_pivot_other) return false;

            var height_root_other = _scapegoat_depth(_root.next);
            if (0 == height_root_other) return 1 <= height_pivot_other;

            if (height_root_other >= height_pivot_other) return false;
            return 1 <= height_pivot_other - height_root_other;
        }
#endregion

        // sub-classes
        public struct Enumerator : IEnumerator<KeyValuePair<Key, Value>>, IDictionaryEnumerator
        {
            private readonly Dictionary<Key, Value> dictionary;
            private readonly int version;
            private int index;
            private KeyValuePair<Key, Value> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(Dictionary<Key, Value> dict, int ret_type)
            {
                dictionary = dict;
                version = dict.version;
                index = 0;
                getEnumeratorRetType = ret_type;
                current = default;
            }

            public bool MoveNext() {
                if (version != dictionary.version) throw new InvalidOperationException(COLLECTION_WAS_MODIFIED);

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)dictionary.count) {
                    ref var staging = ref dictionary.entries[index++];
                    if (0 <= staging.hashCode) {
                        current = new KeyValuePair<Key, Value>(staging.key, staging.value);
                        return true;
                    }
                }

                index = dictionary.count + 1;
                current = default;
                return false;
            }

            public KeyValuePair<Key,Value> Current { get { return current; } }

            public void Dispose() {}

            object IEnumerator.Current
            {
                get {
                    if (index == 0 || (index == dictionary.count + 1)) throw new InvalidOperationException("enumeration can't happen");

                    if (getEnumeratorRetType == DictEntry) {
                        return new DictionaryEntry(current.Key, current.Value);
                    } else {
                        return new KeyValuePair<Key, Value>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != dictionary.version) throw new InvalidOperationException(COLLECTION_WAS_MODIFIED);

                index = 0;
                current = default;
            }

            DictionaryEntry IDictionaryEnumerator.Entry {
                get {
                    if (index == 0 || (index == dictionary.count + 1)) throw new InvalidOperationException("enumeration can't happen");

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key {
                get {
                    if (index == 0 || (index == dictionary.count + 1)) throw new InvalidOperationException("enumeration can't happen");

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value {
                get {
                    if (index == 0 || (index == dictionary.count + 1)) throw new InvalidOperationException("enumeration can't happen");

                    return current.Value;
                }
            }
        }

        public sealed class KeyCollection : ICollection<Key>, ICollection, IReadOnlyCollection<Key>
        {
            private readonly Dictionary<Key, Value> dictionary;

            public KeyCollection(Dictionary<Key, Value> dict)
            {
                dictionary = dict ?? throw new ArgumentNullException(nameof(dict));
            }

            public Enumerator GetEnumerator() => new(dictionary);

            public void CopyTo(Key[] array, int index)
            {
                if (null == array) throw new ArgumentNullException(nameof(array));
                if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "not in bounds");
                if (array.Length - index < Count) throw new ArgumentException("array not large enough");

                int count = dictionary.count;
                Entry[] entries = dictionary.entries;
                for (int i = 0; i < count; i++) {
                    ref var staging = ref entries[i];
                    if (0 <= staging.hashCode) array[index++] = staging.key;
                }
            }

            public int Count { get { return dictionary.Count; } }
            bool ICollection<Key>.IsReadOnly { get { return true; } }
            void ICollection<Key>.Add(Key item) => throw new NotSupportedException();
            void ICollection<Key>.Clear() => throw new NotSupportedException();
            bool ICollection<Key>.Contains(Key item) => dictionary.ContainsKey(item);
            bool ICollection<Key>.Remove(Key item) => throw new NotSupportedException();
            IEnumerator<Key> IEnumerable<Key>.GetEnumerator() => new Enumerator(dictionary);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(dictionary);

            void ICollection.CopyTo(Array array, int index)
            {
                if (null == array) throw new ArgumentNullException(nameof(array));
                if (1 != array.Rank) throw new ArgumentException("won't copy into multi-dimensional array", nameof(array));
                if (0 != array.GetLowerBound(0)) throw new ArgumentException("array should have zero lower bound", nameof(array));
                if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "not in bounds");
                if (array.Length - index < Count) throw new ArgumentException("array not large enough");

                if (array is Key[] keys) {
                    CopyTo(keys, index);
                } else if (array is object[] objects) {
                    int count = dictionary.count;
                    Entry[] entries = dictionary.entries;
                    try {
                        for (int i = 0; i < count; i++) {
                            ref var staging = ref entries[i];
                            if (0 <= staging.hashCode) objects[index++] = staging.key;
                        }
                    } catch (ArrayTypeMismatchException e) {
                        throw new ArgumentException("invalid array type", e);
                    }
                } else {
                    throw new ArgumentException("invalid array type", nameof(array));
                }
            }

            bool ICollection.IsSynchronized { get { return false; } }
            object ICollection.SyncRoot { get { return ((ICollection)dictionary).SyncRoot; } }

            public struct Enumerator : IEnumerator<Key>, IEnumerator
            {
                private readonly Dictionary<Key, Value> dictionary;
                private int index;
                private readonly int version;
                private Key currentKey;

                internal Enumerator(Dictionary<Key, Value> dict)
                {
                    dictionary = dict;
                    version = dict.version;
                    index = 0;
                    currentKey = default;
                }

                public void Dispose() { }

                public bool MoveNext()
                {
                    if (version != dictionary.version) throw new InvalidOperationException(COLLECTION_WAS_MODIFIED);

                    while ((uint)index < (uint)dictionary.count) {
                        ref var staging = ref dictionary.entries[index++];
                        if (0 <= staging.hashCode) {
                            currentKey = staging.key;
                            return true;
                        }
                    }

                    index = dictionary.count + 1;
                    currentKey = default;
                    return false;
                }

                public Key Current { get { return currentKey; } }

                object IEnumerator.Current {
                    get {
                        if (index == 0 || (index == dictionary.count + 1)) throw new InvalidOperationException("enumeration can't happen");

                        return currentKey!;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != dictionary.version) throw new InvalidOperationException(COLLECTION_WAS_MODIFIED);

                    index = 0;
                    currentKey = default;
                }
            }
        }

        public sealed class ValueCollection : ICollection<Value>, ICollection, IReadOnlyCollection<Value>
        {
            private readonly Dictionary<Key, Value> dictionary;

            public ValueCollection(Dictionary<Key, Value> dict)
            {
                dictionary = dict ?? throw new ArgumentNullException(nameof(dict));
            }

            public Enumerator GetEnumerator() => new(dictionary);

            public void CopyTo(Value[] array, int index)
            {
                if (null == array) throw new ArgumentNullException(nameof(array));
                if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "not in bounds");
                if (array.Length - index < Count) throw new ArgumentException("array not large enough");

                int count = dictionary.count;
                Entry[] entries = dictionary.entries;
                for (int i = 0; i < count; i++) {
                    ref var staging = ref entries[i];
                    if (0 <= staging.hashCode) array[index++] = staging.value;
                }
            }

            public int Count { get { return dictionary.Count; } }
            bool ICollection<Value>.IsReadOnly { get { return true; } }
            void ICollection<Value>.Add(Value item) => throw new NotSupportedException();
            bool ICollection<Value>.Remove(Value item) => throw new NotSupportedException();
            void ICollection<Value>.Clear() => throw new NotSupportedException();
            bool ICollection<Value>.Contains(Value item) => dictionary.ContainsValue(item);
            IEnumerator<Value> IEnumerable<Value>.GetEnumerator() => new Enumerator(dictionary);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(dictionary);

            void ICollection.CopyTo(Array array, int index)
            {
                if (null == array) throw new ArgumentNullException(nameof(array));
                if (1 != array.Rank) throw new ArgumentException("won't copy into multi-dimensional array", nameof(array));
                if (0 != array.GetLowerBound(0)) throw new ArgumentException("array should have zero lower bound", nameof(array));
                if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "not in bounds");
                if (array.Length - index < Count) throw new ArgumentException("array not large enough");

                if (array is Value[] values) {
                    CopyTo(values, index);
                } else if (array is object[] objects) {
                    int count = dictionary.count;
                    Entry[] entries = dictionary.entries;
                    try {
                        for (int i = 0; i < count; i++) {
                            ref var staging = ref entries[i];
                            if (0 <= staging.hashCode) objects[index++] = staging.value;
                        }
                    } catch (ArrayTypeMismatchException e) {
                        throw new ArgumentException("invalid array type", e);
                    }
                } else {
                    throw new ArgumentException("invalid array type", nameof(array));
                }
            }

            bool ICollection.IsSynchronized { get { return false; } }
            object ICollection.SyncRoot { get { return ((ICollection)dictionary).SyncRoot; } }

            public struct Enumerator : IEnumerator<Value>
            {
                private readonly Dictionary<Key, Value> dictionary;
                private int index;
                private readonly int version;
                private Value currentValue;

                internal Enumerator(Dictionary<Key, Value> dict)
                {
                    dictionary = dict;
                    version = dict.version;
                    index = 0;
                    currentValue = default;
                }

                public void Dispose() {}

                public bool MoveNext()
                {
                    if (version != dictionary.version) throw new InvalidOperationException(COLLECTION_WAS_MODIFIED);

                    while ((uint)index < (uint)dictionary.count) {
                        ref var staging = ref dictionary.entries[index++];

                        if (staging.hashCode >= 0) {
                            currentValue = staging.value;
                            return true;
                        }
                    }
                    index = dictionary.count + 1;
                    currentValue = default;
                    return false;
                }

                public Value Current { get { return currentValue; } }

                object? IEnumerator.Current { get {
                        if (index == 0 || (index == dictionary.count + 1)) throw new InvalidOperationException("enumeration can't happen");

                        return currentValue;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != dictionary.version) throw new InvalidOperationException(COLLECTION_WAS_MODIFIED);

                    index = 0;
                    currentValue = default;
                }
            }
        }
    }
}