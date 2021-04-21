using System;
using System.Collections.Generic;
using Zaimoni.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Zaimoni.Serialization
{
    interface IOnDeserializing // this may not make much sense in a proper RAII context
    {
        void OnDeserializing(in StreamingContext context);
    }

    interface IOnDeserialized
    {
        void OnDeserialized();
    }

    interface IOnFullyDeserialized
    {
        void OnFullyDeserialized();
    }


    /// <summary>
    /// Part of cloning the System.Runtime.Serialization specification, which is deprecated and cannot be assumed to be available indefinitely.
    /// This corresponds to ObjectManager
    /// </summary>
    public sealed class LoadManager
    {
        private readonly StreamingContext _context;
        private readonly Dictionary<int, List<Action<object>>> _fixups = new Dictionary<int, List<Action<object>>>();
        private readonly List<KVpair<int, KeyValuePair<object, Type>?>> _seen = new List<KVpair<int, KeyValuePair<object, Type>?>>(); // would prefer a dictionary-type data structure here
        private readonly List<IOnFullyDeserialized?> _onDeserializedTargets = new List<IOnFullyDeserialized?>(); // could use event syntax for this

        public LoadManager(StreamingContext context)
        {
            _context = context;
        }

        public object? GetObject(int id) {
            if (0 >= id) throw new ArgumentOutOfRangeException(nameof(id), "0 >= "+id.ToString());
            lock (_seen) {
                // not what we want
                var ub = _seen.Count;
                while (0 <= --ub) {
                    if (_seen[ub].Key == id) {
                        if (null == _seen[ub].Value) return null; // forward-reference
                        return _seen[ub].Value.Value.Key;
                    }
                }
                _seen.Add(new KVpair<int, KeyValuePair<object, Type>?>(id, null)); // assume that it will be there eventually
            }
            return null;
        }

        private void exec_fixup(int id, object src)
        {
            lock (_fixups) {
                if (_fixups.TryGetValue(id, out var cache)) {
                    foreach (var op in cache) op(src);
                    _fixups.Remove(id);
                }
            }
        }

        static private void RaiseOnDeserializedEvent<T>(T obj) {
            if (obj is IOnDeserialized x) x.OnDeserialized();
        }

        public void RaiseOnDeserializedEvent()
        { // \todo remove if redundant
            lock (_onDeserializedTargets) {
                var ub = _onDeserializedTargets.Count;
                // \todo what we would like to do is "move" the backing array without copying,
                // do the countdown on the backing array, then let it scope out
                while (0 < --ub) {
                    _onDeserializedTargets[ub]!.OnFullyDeserialized();
                    _onDeserializedTargets[ub] = null; // trigger GC if we had boxed a struct
                }
                _onDeserializedTargets.Clear();
            }
        }

        private void RegisterFullDeserialization<T>(T obj) {
            if (obj is IOnFullyDeserialized x) lock (_onDeserializedTargets) { _onDeserializedTargets.Add(x); }
        }

        public void Register<T>(T obj, int id) where T:class {
            if (null == obj) throw new ArgumentNullException(nameof(obj));
            if (0 >= id) throw new ArgumentOutOfRangeException(nameof(id), "0 >= " + id.ToString());
            var src = new KeyValuePair<object, Type>(obj, obj.GetType());
            lock (_seen) {
                var ub = _seen.Count;
                while (0 <= --ub) {
                    if (_seen[ub].Key == id) { // \todo separate exception type for serialization errors
                        if (null != _seen[ub].Value) throw new InvalidOperationException("already registered "+id.ToString());
                        _seen[ub].Value = src;
                        exec_fixup(id, obj);
                        RaiseOnDeserializedEvent(obj);
                        RegisterFullDeserialization(obj);
                        return;
                    }
                }
                _seen.Add(new KVpair<int, KeyValuePair<object, Type>?>(id, src));
                exec_fixup(id, obj);
                RaiseOnDeserializedEvent(obj);
                RegisterFullDeserialization(obj);
            }
        }

        public void RegisterFixup(int id, Action<object> handler) {
            lock (_fixups) {
                if (_fixups.TryGetValue(id, out var cache)) cache.Add(handler);
                else _fixups.Add(id, new List<Action<object>> { handler });
            }
        }

        public void RaiseOnSerializingEvent<T>(T obj) {
            if (obj is IOnDeserializing x) x.OnDeserializing(in _context);
        }
    }
}
