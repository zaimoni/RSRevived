#define FORCE_STD_DICTIONARY_GC

using System;
using System.Collections.Generic;

#nullable enable

namespace Zaimoni.Serialization
{
    class DecodeObjects
    {
        private Dictionary<ulong, List<Action<object>>?> requested = new();
        private Dictionary<Type, Dictionary<ulong, object>> encodings = new();

        public object Seen(ulong id)
        {
            if (requested.ContainsKey(id)) return null; // already seen, not yet recorded
            foreach (var x in encodings.Values) {
                if (x.TryGetValue(id, out var ret)) return ret;
            }
            requested.Add(id, null); // record that we want this
            return null;
        }

        // Intended model is a handler that assigns an object, then starts a no-op task to signal that the assignment has happened.
        public void Schedule(ulong id, Action<object> handler)
        {
            if (!requested.TryGetValue(id, out var exec)) throw new InvalidOperationException("can only schedule handlers for a registered object");
            if (null == exec) requested[id] = (exec = new());
            exec.Add(handler);
        }

        public void Register<T>(ulong id, T src) where T : class
        {
            if (null == src) throw new ArgumentNullException(nameof(src));
            if (!requested.TryGetValue(id, out var exec)) throw new InvalidOperationException("only can register a constructed object once");
            // exec is a list of object handlers "waiting" on this registration
            var type = typeof(T);
            if (encodings.TryGetValue(type, out var cache)) {
                if (cache.ContainsKey(id)) throw new InvalidOperationException("only can register a constructed object once");
            } else encodings.Add(type, (cache = new()));
            cache.Add(id, src);
#if FORCE_STD_DICTIONARY_GC
            requested[id] = null;
#endif
            requested.Remove(id);
            if (null != exec) foreach (var handler in exec) handler(src);
        }

        public static void load<K, V>(KeyValuePair<K, V>[] src, ref Dictionary<K, V>? dest) where K:notnull
        {
            if (null == src || 0 >= src.Length) {
                if (null != dest && 0 < dest.Count) dest = new();
                return;
            }
            var ub = src.Length;
            var ret = new Dictionary<K, V>(ub);
            while (0 <= --ub) {
                ref var x = ref src[ub];
                ret.Add(x.Key, x.Value);
            }
            dest = ret;
        }
    }
}
