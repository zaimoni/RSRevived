using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Zaimoni.Data;

#nullable enable

namespace Zaimoni.Serialization
{
    public interface ISerialize
    {   // need something heavier as the second parameter
//      void load(Stream src, DecodeObjects context); // unsure if this is needed (constructor overload?)
        void save(Stream dest, EncodeObjects encode);
    }

    public class EncodeObjects
    {
        public readonly StreamingContext context;
        public readonly Formatter format;
        private ulong seen = 0;
        private ulong type_seen = 0;
        private Dictionary<Type, ulong> type_code_of = new();
        private Dictionary<Type, Dictionary<object, ulong>> encodings = new();
        private List<Action<Stream>> to_save = new();
        private List<Action<Stream>> to_save_type = new();

        public EncodeObjects()
        {
            context = new StreamingContext();
            format = new Formatter(context);
        }

        // precondition: src not null
        public ulong Saving(ISerialize src)
        {
            if (null == src) return 0; // likely should handle this at a higher level; signals writing a null code rather than an object reference

            var type = src.GetType();
            var t_code = getTypeCode(type);
            if (encodings.TryGetValue(type, out var cache)) {
                if (cache.TryGetValue(src, out ulong code)) return code;
            } else encodings.Add(type, cache = new());
            cache.Add(src, ++seen);

            // \todo handle polymorphism -- loader must know which subclass to load
            // yes, appears to be subverting historical architecture
            to_save.Add(dest => {
                Formatter.SerializeTypeCode(dest, t_code);
                Formatter.SerializeObjCode(dest, seen);
                if (src is IOnSerializing x) x.OnSerializing(in context);
                src.save(dest, this);
                if (src is IOnSerialized y) y.OnSerialized(in context);
            });
            return seen;
        }

        public bool SaveNext(Stream dest) {
            if (0 >= to_save.Count) return false;
            // if there are type code entries, flush them
            var save_type_ub = to_save_type.Count;
            if (0 < save_type_ub) {
                int i = 0;
                while (i < save_type_ub) {
                    to_save_type[i](dest);
                    to_save_type[i] = null;  // early GC
                }
                to_save_type = new();   // force GC
            }

            var next = to_save[0];
            to_save.RemoveAt(0);
            next(dest); // write object itself to hard drive
            return true;
        }

        private Dictionary<Type, Action<Stream, object>> m_LinearizedElement_cache = new();
        private Action<Stream, object> LinearizedElement<T>()
        {
            var t_info = typeof(T);
            if (m_LinearizedElement_cache.TryGetValue(t_info, out var handler)) return handler;
            // \todo interface as easy mode
            // \todo check for direct formatter support
            if (t_info.IsEnum) {
                var method_candidates = typeof(Formatter).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                // \todo route to Formatter::SerializeEnum<T>
                throw new InvalidOperationException(t_info.FullName + "\n" + method_candidates.to_s()); // need to determine what is needed here
            }

            if (t_info.IsGenericType) { // will not handle arrays
                // expect KeyValuePair to route this way
                throw new InvalidOperationException(t_info.FullName); // need to debug what is needed here
            } else {
                var method_candidates = typeof(Formatter).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                // todo: check for Formatter::Serialize variant
                throw new InvalidOperationException(t_info.FullName + "\n" + method_candidates.to_s()); // need to determine what is needed here
            }

            throw new InvalidOperationException(t_info.FullName + "\nattempting to return null");
            return null; // non-functional shim to get things building
        }

        public void LinearSave<T>(IEnumerable<T>? src, Stream dest) {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(dest, count);
            if (0 < count) {
                var handler = LinearizedElement<T>();
                foreach (var x in src) handler(dest, x);
            }
        }

        private ulong getTypeCode(Type src) {
            if (type_code_of.TryGetValue(src, out var code)) return code;
            type_code_of.Add(src, ++type_seen);

            to_save_type.Add(dest => {
                Formatter.SerializeTypeCode(dest, type_seen, src.FullName);
            });

            return type_seen;
        }
    }

    public static partial class Virtual
    {
        public static void BinarySave<_T_>(this string filepath, _T_ src) where _T_: ISerialize
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            var encode = new EncodeObjects();
            encode.Saving(src);
            using var stream = filepath.CreateStream(true);
            while (encode.SaveNext(stream));
            stream.Flush();
        }
    }
}
