using System;
using System.Collections.Generic;

namespace Zaimoni.Serialization
{
    class EncodeObjects
    {
        private ulong seen = 0;
        private Dictionary<Type, Dictionary<object, ulong>> encodings = new();

        public ulong Saving<T>(T src) where T:class
        {
            var type = typeof(T);
            if (encodings.TryGetValue(type, out var cache)) {
                if (cache.TryGetValue(src, out var code)) return code;
                cache.Add(src, ++seen);
                return seen;
            }
            encodings.Add(type, new Dictionary<object, ulong> { [src] = ++seen });
            return seen;
        }
    }
}
