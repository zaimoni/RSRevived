using System;
using System.Collections.Generic;
using System.IO;

namespace Zaimoni.Serialization
{
    class EncodeObjects
    {
        private ulong seen = 0;
        private Dictionary<Type, Dictionary<object, ulong>> encodings = new();
        private List<KeyValuePair<ulong, Action<Stream>>> to_save = new();

        public ulong Saving<T>(T src) where T:class
        {
            var type = typeof(T);
            if (encodings.TryGetValue(type, out var cache)) {
                if (cache.TryGetValue(src, out var code)) return code;
                cache.Add(src, ++seen);
            } else {
                encodings.Add(type, new Dictionary<object, ulong> { [src] = ++seen });
            }
            to_save.Add(new(seen, null)); // \todo construct action for saving src and schedule it
            return seen;
        }

        public bool SaveNext(Stream dest) {
            if (0 >= to_save.Count) return false;
            var next = to_save[0];
            to_save.RemoveAt(0);
            // \todo write object id at next.Key to hard drive
            // \todo write object itself to hard drive
            return true;
        }
    }
}
