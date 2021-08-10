#define FORCE_STD_DICTIONARY_GC

using System;
using System.Collections.Generic;
using System.IO;
using Zaimoni.Data;

#nullable enable

namespace Zaimoni.Serialization
{
    public class DecodeObjects
    {
        public readonly StreamingContext context;
        public readonly Formatter format;
        private Dictionary<ulong, List<Action<object>>?> requested = new();
        private Dictionary<Type, Dictionary<ulong, object>> encodings = new();
        private Dictionary<ulong, Type> type_for_code = new();

        public DecodeObjects()
        {
            context = new StreamingContext();
            format = new Formatter(context);
        }

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

        private static readonly Type[] integrated_constructor = new Type[]{ typeof(Stream), typeof(DecodeObjects) };

        // primary data load, not load-from-reference
        public T Load<T>(Stream src) where T : class
        {
            var code = format.Peek(src);
            if (Formatter.null_code == code) return default;    // usually null
            // Formatter.SerializeTypeCode(dest, t_code);
            format.DeserializeTypeCode(src, type_for_code);
            var t_code = format.DeserializeTypeCode(src);
            if (!type_for_code.TryGetValue(t_code, out var type)) throw new InvalidOperationException("requested type code not mapped");
            var o_code = format.DeserializeObjCodeAfterTypecode(src);

            var coop_constructor = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, integrated_constructor, null);
            if (null != coop_constructor) {
                return (T)coop_constructor.Invoke(new object[] { src, this });
            }

            throw new InvalidOperationException("unhandled type "+type.AssemblyQualifiedName);
        }

        public T LoadInline<T>(Stream src)
        {
            var type = typeof(T);
            var coop_constructor = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, integrated_constructor, null);
            if (null != coop_constructor) {
                return (T)coop_constructor.Invoke(new object[] { src, this });
            }

            throw new InvalidOperationException("unhandled type "+type.AssemblyQualifiedName);
        }

#region example boilerplate based on LinearizedElement<T>
        private void LoadFrom(Stream src, ref string dest) => Formatter.Deserialize(src, ref dest);
#endregion


#region example boilerplate based on LinearSave<T>
        public void LoadFrom(Stream src, ref Dictionary<string, string> dest)
        {
            dest = new();
            ulong count = 0;
            Formatter.Deserialize7bit(src, ref count);
            while (0 < count) {
                --count;
                // if either key or value type requires object ids, this would trigger an indirect-load implementation
                string key = string.Empty;
                string value = string.Empty;
                LoadFrom(src, ref key);
                LoadFrom(src, ref value);
                // Intentionally use first value if duplicate keys
                if (!dest.ContainsKey(key)) dest.Add(key, value);
            }
        }
#endregion

    }

    public static partial class Virtual
    {
        public static _T_ BinaryLoad<_T_>(this string filepath) where _T_ : class
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            var decode = new DecodeObjects();
            using var stream = filepath.CreateStream(false);
            _T_ ret = decode.Load<_T_>(stream);
            stream.Flush();
            return ret;
        }
    }
}
