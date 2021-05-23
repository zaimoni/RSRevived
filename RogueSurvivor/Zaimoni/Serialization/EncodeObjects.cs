using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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

        EncodeObjects()
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
                format.SerializeTypeCode(dest, t_code);
                format.SerializeObjCode(dest, seen);
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

        static public T[] Linearize<T>(IEnumerable<T>? src) {
            if (null == src || !src.Any()) return null;
            return src.ToArray();
        }

        private ulong getTypeCode(Type src) {
            if (type_code_of.TryGetValue(src, out var code)) return code;
            type_code_of.Add(src, ++type_seen);

            to_save_type.Add(dest => {
                format.SerializeTypeCode(dest, type_seen, src.FullName);
            });

            return type_seen;
        }
    }
}
