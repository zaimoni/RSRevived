﻿#define FORCE_STD_DICTIONARY_GC

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Zaimoni.Data;

#nullable enable

namespace Zaimoni.Serialization
{
    // assistant class
    public class StageArray<T>
    {
        private readonly T?[] relay;
        private readonly Action<T[]> onDone;

        public StageArray(T?[] src, Action<T[]> handler) {
            relay = src;
            onDone = handler;
        }

        public void Verify() {
            foreach(var x in relay) {
                if (null == x) return;
            }
            onDone(relay!);
        }
    }

    public class DecodeObjects
    {
        public readonly StreamingContext context;
        public readonly Formatter format;
        public readonly Stream src;
        private readonly Dictionary<ulong, List<Action<object>>?> requested = new();
        private readonly Dictionary<Type, Dictionary<ulong, object>> encodings = new();
        private readonly Dictionary<ulong, Type> type_for_code = new();

        public DecodeObjects(Stream _src)
        {
            src = _src;
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

        private static readonly Type[] integrated_constructor = new Type[]{ typeof(DecodeObjects) };

        // primary data load, not load-from-reference
        public T? Load<T>(out ulong o_code) where T : class
        {
            format.DeserializeTypeCode(src, type_for_code);
            if (Formatter.null_code == format.Preview) {
                o_code = 0;
                return null;
            }

            if (Formatter.obj_ref_code == format.Preview) {
                o_code = format.DeserializeObjCodeAfterTypecode(src);
                var obj = Seen(o_code);
                if (obj is T want) return want;
                if (null != obj) throw new InvalidOperationException("requested object is not a "+typeof(T).AssemblyQualifiedName);
                return null;
            }

            var t_code = format.DeserializeTypeCode(src);
            if (!type_for_code.TryGetValue(t_code, out var type)) throw new InvalidOperationException("requested type code not mapped");
            o_code = format.DeserializeObjCodeAfterTypecode(src);

            var coop_constructor = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, integrated_constructor, null);
            if (null != coop_constructor) {
                return (T)coop_constructor.Invoke(new object[] { this });
            }

            throw new InvalidOperationException("unhandled type "+type.AssemblyQualifiedName);
        }

        public T LoadInline<T>()
        {
            var type = typeof(T);
            var coop_constructor = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, integrated_constructor, null);
            if (null != coop_constructor) {
                return (T)coop_constructor.Invoke(new object[] { this });
            }

            throw new InvalidOperationException("unhandled type "+type.AssemblyQualifiedName);
        }

        public bool LoadNext()
        {
            if (0 >= requested.Count) return false;

            format.DeserializeTypeCode(src, type_for_code);
            if (Formatter.null_code == format.Preview) return false;    // usually null
            var t_code = format.DeserializeTypeCode(src);
            if (!type_for_code.TryGetValue(t_code, out var type)) throw new InvalidOperationException("requested type code not mapped");
            var o_code = format.DeserializeObjCodeAfterTypecode(src);
            if (!encodings.TryGetValue(type, out var prior)) encodings.Add(type, prior = new());
            if (prior.TryGetValue(o_code, out var preexist)) throw new InvalidOperationException("trying to load object twice: "+o_code.ToString()+", "+preexist.ToString());
            if (!requested.TryGetValue(o_code, out var handlers)) throw new InvalidOperationException("trying to load unwanted object");

            var coop_constructor = type.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, integrated_constructor, null);
            if (null != coop_constructor) {
                var obj = coop_constructor.Invoke(new object[] { this });
                prior.Add(o_code, obj);
                if (null != handlers) {
                    var ub = handlers.Count;
                    while (0 <= --ub) {
                        handlers[ub](obj);
                        handlers[ub] = null;
                    }
                    requested[o_code] = null;
                }
                requested.Remove(o_code);
                return true;
            }

            throw new InvalidOperationException("unhandled type "+type.AssemblyQualifiedName);
        }


#region example boilerplate based on LinearizedElement<T>
        private void LoadFrom(ref string dest) => Formatter.Deserialize(src, ref dest);
#endregion


#region example boilerplate based on LinearSave<T>
        public void LoadFrom(ref Dictionary<string, string> dest)
        {
            dest = new();
            ulong count = 0;
            Formatter.Deserialize7bit(src, ref count);
            while (0 < count) {
                --count;
                // if either key or value type requires object ids, this would trigger an indirect-load implementation
                string key = string.Empty;
                string value = string.Empty;
                LoadFrom(ref key);
                LoadFrom(ref value);
                // Intentionally use first value if duplicate keys
                if (!dest.ContainsKey(key)) dest.Add(key, value);
            }
        }

        public void LoadFrom(ref byte[] dest)
        {
            ulong count = 0;
            Formatter.Deserialize7bit(src, ref count);
            dest = new byte[count];
            byte tmp_byte = 0;
            while (0 < count) {
                Formatter.Deserialize(src, ref tmp_byte);
                dest[--count] = tmp_byte;
            }
        }

        public void LinearLoad<T>(Action<T[]> handler) where T:class
        {
            int count = 0;
            Formatter.Deserialize7bit(src, ref count);
            if (0 >= count) return; // no action needed
            var dest = new T[count];
            var stage = new StageArray<T>(dest, handler);

            // function extraction target does not work -- out/ref parameter needs accessing from lambda function
            int n = 0;
            while (0 < count) {
                --count;
                var code = Formatter.DeserializeObjCode(src);
                if (0 < code) {
                    var obj = Seen(code);
                    if (null != obj) {
                        if (obj is T w) dest[n] = w;
                        else throw new InvalidOperationException(nameof(T) + " object not loaded");
                    } else {
                        var i = n;
                        Schedule(code, (o) => {
                            if (o is T w) dest[i] = w;
                            else throw new InvalidOperationException(nameof(T) + " object not loaded");
                            stage.Verify();
                        });
                    }
                }
                else throw new InvalidOperationException(nameof(T) + " object not loaded");
                n++;
            }
            stage.Verify();
        }

        public void LoadFrom7bit(ref ulong[] dest)
        {
            int count = 0;
            Formatter.Deserialize7bit(src, ref count);
            if (null == dest || dest.Length != count) dest = new ulong[count];

            int n = 0;
            while (0 < count) {
                --count;
                Formatter.Deserialize7bit(src, ref dest[n++]);
            }
        }

        public void LoadFrom7bit(ref int[] dest)
        {
            int count = 0;
            Formatter.Deserialize7bit(src, ref count);
            if (null == dest || dest.Length != count) dest = new int[count];

            int n = 0;
            while (0 < count)
            {
                --count;
                Formatter.Deserialize7bit(src, ref dest[n++]);
            }
        }

        public void LoadFrom7bit(ref int[,,]? dest)
        {
            var n = 0;
            Span<int> ub = stackalloc int[3];
            Formatter.Deserialize7bit(src, ref ub[n++]);
            if (0 == ub[0]) {
                dest = null;
                return;
            }
            while (3 > n) Formatter.Deserialize7bit(src, ref ub[n++]);
            // insecure: doesn't validate bounds before allocating \todo fix
            dest = new int[ub[0], ub[1], ub[2]];

            int stage = 0;
            var iter = new int[3];
            iter[0] = 0;
            while (ub[0] > iter[0]) {
                iter[1] = 0;
                while (ub[1] > iter[1]) {
                    iter[2] = 0;
                    while (ub[2] > iter[2]) {
                        Formatter.Deserialize7bit(src, ref stage);
                        dest.SetValue(stage, iter);
                        iter[2]++;
                    }
                    iter[1]++;
                }
                iter[0]++;
            }
        }

        public void LoadFrom(ref byte[,]? dest)
        {
            var n = 0;
            Span<int> ub = stackalloc int[2];
            Formatter.Deserialize7bit(src, ref ub[n++]);
            if (0 == ub[0]) {
                dest = null;
                return;
            }
            while (2 > n) Formatter.Deserialize7bit(src, ref ub[n++]);
            // insecure: doesn't validate bounds before allocating \todo fix
            var local_dest = new byte[ub[0], ub[1]]; // should be null-initialized
            dest = local_dest; // should be null-initialized

            byte tmp_byte = 0;
            var iter = new int[2];

            iter[0] = 0;
            while (ub[0] > iter[0]) {
                iter[1] = 0;
                while (ub[1] > iter[1]) {
                    Formatter.Deserialize(src, ref tmp_byte);
                    dest[ub[0], ub[1]] = tmp_byte;
                    iter[1]++;
                }
                iter[0]++;
            }
        }

        public void LoadFrom<T>(ref T[,]? dest) where T:class
        {
            var n = 0;
            Span<int> ub = stackalloc int[2];
            Formatter.Deserialize7bit(src, ref ub[n++]);
            if (0 == ub[0]) {
                dest = null;
                return;
            }
            while (2 > n) Formatter.Deserialize7bit(src, ref ub[n++]);
            // insecure: doesn't validate bounds before allocating \todo fix
            var local_dest = new T[ub[0], ub[1]]; // should be null-initialized
            dest = local_dest; // should be null-initialized

            int stage = 0;
            var iter = new int[2];

            Action<object> load_handler() {
                var iter_clone = iter.ToArray(); // need JavaScript/Perl closure i.e. value copy
                return (o) => {
                    if (o is T w) local_dest.SetValue(w, iter_clone);
                    else throw new InvalidOperationException("incompatible object loaded");
                };
            };

            iter[0] = 0;
            while (ub[0] > iter[0]) {
                iter[1] = 0;
                while (ub[1] > iter[1]) {
                    // function extraction target does not work -- out/ref parameter needs accessing from lambda function
                    var code = Formatter.DeserializeObjCode(src);
                    if (0 < code) {
                        var obj = Seen(code);
                        if (null != obj) {
                            if (obj is T w) dest.SetValue(w, iter);
                            else throw new InvalidOperationException("incompatible object loaded");
                        } else {
                            Schedule(code, load_handler());
                        }
                    };
                    // null is ok for library code
                    // end failed function extraction target
                    iter[1]++;
                }
                iter[0]++;
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
            using var stream = filepath.CreateStream(false);
            var decode = new DecodeObjects(stream);
            ulong discard;
            _T_ ret = decode.Load<_T_>(out discard);
            while (decode.LoadNext());
            stream.Flush();
            return ret;
        }
    }
}
