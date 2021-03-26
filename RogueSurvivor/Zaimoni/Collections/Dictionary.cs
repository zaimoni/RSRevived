using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization;
using Zaimoni.Data;

#nullable enable

// re-implementation of System.Collections.Generic.Dictionary
// cloning: https://referencesource.microsoft.com/#mscorlib/system/collections/generic/dictionary.cs,cc27fcdd81291584,references

namespace Zaimoni.Collections
{
    [Serializable]
    public class Dictionary<Key, Value> : IDictionary<Key, Value>, IDictionary, IReadOnlyDictionary<Key, Value>, ISerializable, IDeserializationCallback
    {
        [Serializable]
        private struct Entry
        {
            [NonSerialized] public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            [NonSerialized] public int prev;        // Index of previous entry, -1 if first
            [NonSerialized] public int next;        // Index of next entry, -1 if last
            // serialize these two, not the above three
            public Key key;         // Key of entry
            public Value value;     // Value of entry
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

        public Value this[Key key]
        {
            get {
                int i = FindEntry(key);
                if (0 > i) throw new KeyNotFoundException("key not found: "+key.ToString());
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
                return true;
            }
            return false;
        }

        public bool Remove(Key key)
        {
            int i = FindEntry(key);
            if (0 <= i) {
                Remove(i);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (0 <= activeList) {
                version++;
                while (0 < count) {
                    ref var staging = ref entries[--count];
                    if (0 <= staging.hashCode) {
                        staging.hashCode = -1;
                        staging.prev = -1;
                        staging.next = -1;
                        staging.key = default;
                        staging.value = default;
                    }
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
            if (0 > doomed || entries.Length <= doomed) throw new InvalidOperationException("invalid deletion index");
            ref var staging = ref entries[doomed];
            if (-1 == staging.hashCode) return; // not really there after all

            // unlink us
            version++;
            if (1 == Count % 2) {
#if DEBUG
                if (-1 == entries[activeList].next && 1 < Count) throw new InvalidOperationException("clearing dictionary when non-final deleting");
#endif
                activeList = entries[activeList].prev;
            }
            if (0 <= staging.prev) {
                if (0 <= staging.next) {
                    entries[staging.next].prev = staging.prev;
                    entries[staging.prev].next = staging.next;
                } else {
                    entries[staging.prev].next = -1;
                }
            } else {
                if (0 <= staging.next) {
                    entries[staging.next].prev = staging.prev;
                } else {
                }
            }

            staging.hashCode = -1;
            staging.prev = -1;
            staging.next = freeList;
            staging.key = default;  // must trigger GC here
            staging.value = default;

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

            return FindEntry(key, out _, out _);
        }

        private int FindEntry(Key key, out int scan_lb, out int scan_ub)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            scan_lb = -1;
            scan_ub = -1;
            if (0 > activeList) return -1;

            int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

            bool false_positive = false;
            int scan = activeList;

            while (0 <= scan) {
                var code = hashCode.CompareTo(entries[scan].hashCode);
                if (0 == code) {
                    scan_lb = scan;
                    scan_ub = scan;
                    if (Comparer.Equals(entries[scan].key, key)) return scan;
                    false_positive = true;
                    break;
                } else if (0 < code) { // greater than
#if DEBUG
                    if (hashCode < entries[scan].hashCode) throw new InvalidOperationException("inverted add: scan/prev; " + scan.ToString() + ": " + hashCode.ToString() + ", " + entries[scan].hashCode.ToString());
#endif
                    var probe = entries[scan].next;
                    if (0 > probe) {
                        scan_lb = scan;
                        break;
                    }
                    if (hashCode >= entries[probe].hashCode) {
                        scan = probe;
                        continue;
                    }
#if DEBUG
                    if (hashCode > entries[probe].hashCode) throw new InvalidOperationException("inverted add: scan/prev #2; " + scan.ToString() + ": " + entries[scan].hashCode + ", " + hashCode.ToString() + ", " + entries[probe].hashCode.ToString());
#endif
                    scan_lb = scan;
                    scan_ub = probe;
                    break;
                } else /* if (0 > code) */ { // less than
#if DEBUG
                    if (hashCode > entries[scan].hashCode) throw new InvalidOperationException("inverted add: scan/next; " + scan.ToString() + ": " + hashCode.ToString() + ", " + entries[scan].hashCode.ToString());
#endif
                    var probe = entries[scan].prev;
                    if (0 > probe) {
                        scan_ub = scan;
                        break;
                    }
                    if (hashCode <= entries[probe].hashCode) {
                        scan = probe;
                        continue;
                    }
#if DEBUG
                    if (hashCode < entries[probe].hashCode) throw new InvalidOperationException("inverted add: scan/next #2; " + scan.ToString() + ": " + entries[probe].hashCode.ToString() + ", " + hashCode.ToString() + ", " + entries[scan].hashCode.ToString());
#endif
                    scan_ub = scan;
                    scan_lb = probe;
                    break;
                }
            }

            if (false_positive) {
                while (0 <= scan_lb) {
                    var probe = entries[scan_lb].prev;
                    if (hashCode != entries[probe].hashCode) break;
                    if (Comparer.Equals(entries[probe].key, key)) return probe;
                    scan_lb = probe;
                }
                while (0 <= scan_ub)
                {
                    var probe = entries[scan_ub].prev;
                    if (hashCode != entries[probe].hashCode) break;
                    if (Comparer.Equals(entries[probe].key, key)) return probe;
                    scan_ub = probe;
                }
                // not here?  "Insert at top"
                scan_lb = scan_ub;
                scan_ub = entries[scan_lb].next;
            }
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
                version++;
                if (0 == entries.Length) entries = new Entry[16];
                entries[0].hashCode = hashCode;
                entries[0].prev = -1;
                entries[0].next = -1;
                entries[0].key = key;
                entries[0].value = value;
                count = 1;
                activeList = 0;
                freeCount = 0;
                freeList = -1;
                return;
            }

            // find insertion point
            var index = FindEntry(key, out int scan_lb, out int scan_ub);
            if (0 <= index) {
                if (add) throw new ArgumentException("adding duplicate", nameof(key));
                version++;
                entries[index].value = value;
                return;
            }

            // scan_lb, scan_ub now bracket the intended entry point (as a linked list)
#if DEBUG
            if (0 <= scan_lb && entries[scan_lb].hashCode > hashCode) throw new InvalidOperationException("inverted add: prev; " + scan_lb.ToString() + ", " + scan_ub.ToString() + ": " + hashCode.ToString() + ", " + entries[scan_lb].hashCode.ToString());
            if (0 <= scan_ub && entries[scan_ub].hashCode < hashCode) throw new InvalidOperationException("inverted add: next; " + scan_lb.ToString() + ", " + scan_ub.ToString() + ": " + hashCode.ToString() + ", " + entries[scan_ub].hashCode.ToString());
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

            entries[index].hashCode = hashCode;
            entries[index].prev = scan_lb;
            entries[index].next = scan_ub;
            entries[index].key = key;
            entries[index].value = value;
            version++;
            if (0 <= scan_lb) entries[scan_lb].next = index;
            if (0 <= scan_ub) entries[scan_ub].prev = index;
            if (1 == Count % 2) {
#if DEBUG
                if (-1 == entries[activeList].next) throw new InvalidOperationException("clearing dictionary when inserting");
#endif
                activeList = entries[activeList].next;
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (null == key) throw new ArgumentNullException(nameof(key));
            return key is Key;
        }

        private void Resize() => Resize(0 == entries.Length ? 16 : 2 * entries.Length);

        private void Resize(int newSize)
        {
            if (newSize <= entries.Length) return; // no-op; might make sense to actively shrink but we can force-copy for that

            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            entries = newEntries;
        }

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
                if (version != dictionary.version) throw new InvalidOperationException("enumeration failed: collection changed"); // \todo line this up

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
                if (version != dictionary.version) throw new InvalidOperationException("enumeration failed: collection changed"); // \todo line this up

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
                    if (version != dictionary.version) throw new InvalidOperationException("enumeration failed: collection changed"); // \todo line this up

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
                    if (version != dictionary.version) throw new InvalidOperationException("enumeration failed: collection changed"); // \todo line this up

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
                    if (version != dictionary.version) throw new InvalidOperationException("enumeration failed: collection changed"); // \todo line this up

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
                    if (version != dictionary.version) throw new InvalidOperationException("enumeration failed: collection changed"); // \todo line this up

                    index = 0;
                    currentValue = default;
                }
            }
        }
    }
}