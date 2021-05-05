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
        private Dictionary<Type, Dictionary<object, ulong>> encodings = new();
        private List<KeyValuePair<ulong, Action<Stream>>> to_save = new();

        EncodeObjects(StreamingContext _context, Formatter _format)
        {
            context = _context;
            format = _format;
        }

        // precondition: src not null
        public ulong Saving<T>(T src) where T : ISerialize
        {
            var type = typeof(T);
            if (encodings.TryGetValue(type, out var cache)) {
                if (cache.TryGetValue(src, out ulong code)) return code;
            } else encodings.Add(type, cache = new());
            cache.Add(src, ++seen);

            // \todo handle polymorphism -- loader must know which subclass to load
            // yes, appears to be subverting historical architecture
            to_save.Add(new(seen, dest => {
                if (src is IOnSerializing x) x.OnSerializing(in context);
                src.save(dest, this);
                if (src is IOnSerialized y) y.OnSerialized(in context);
            }));
            return seen;
        }

        public bool SaveNext(Stream dest) {
            if (0 >= to_save.Count) return false;
            var next = to_save[0];
            to_save.RemoveAt(0);

            // \todo intertie w/SaveManager
            format.SerializeObjCode(dest, next.Key);
            // \todo intertie w/SaveManager

            next.Value(dest); // write object itself to hard drive
            return true;
        }

        static public T[] Linearize<T>(IEnumerable<T>? src) {
            if (null == src || !src.Any()) return null;
            return src.ToArray();
        }
    }
}
