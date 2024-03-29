﻿using System;
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
        private ulong scheduled = 0;

        public StageArray(T?[] src, Action<T[]> handler)
        {
            relay = src;
            onDone = handler;
        }

        public void Verify()
        {
            if (0 < scheduled) return;
            foreach (var x in relay) {
                if (null == x) return;
            }
            onDone(relay!);
        }

        public void Schedule() {
            scheduled++;
        }

        public void Complete()
        {
            if (0 < scheduled) {
                if (0 >= --scheduled) onDone(relay!);
            }
        }
    }

    // should not implement this on structs, to avoid boxing
    public interface ISerialize
    {   // need something heavier as the second parameter
//      void load(Stream src, DecodeObjects context); // unsure if this is needed (constructor overload?)
        void save(EncodeObjects encode);
    }

    public partial interface ISave
    {
        // C# 11.  Forces compiler error until we actually know what we're doing.
#region ISerialize support
#if PROTOTYPE
        static abstract void Save(EncodeObjects encode, object src);
        static abstract void InlineSave(EncodeObjects encode, object src);
#endif

        static void Save(EncodeObjects encode, ISerialize src) {
            var code = encode.Saving(src); // obligatory, in spite of type prefix/suffix
            if (0 < code) Formatter.SerializeObjCode(encode.dest, code);
            else throw new ArgumentNullException("src");
        }

        static void InlineSave(EncodeObjects encode, ISerialize src) => src.save(encode);

        static void Save(EncodeObjects encode, KeyValuePair<string, object> src) {
            Formatter.Serialize(encode.dest, src.Key);
            encode.SaveObject(src.Value);
        }

#endregion

#region 7bit support, basis cases
#if PROTOTYPE
        static abstract void Serialize7bit(Stream dest, object src);
#endif
        static void Serialize7bit(Stream dest, ulong src) => Formatter.Serialize7bit(dest, src);
        static void Serialize7bit(Stream dest, uint src) => Formatter.Serialize7bit(dest, src);
        static void Serialize7bit(Stream dest, ushort src) => Formatter.Serialize7bit(dest, src);
        static void Serialize7bit(Stream dest, long src) => Formatter.Serialize7bit(dest, src);
        static void Serialize7bit(Stream dest, int src) => Formatter.Serialize7bit(dest, src);
        static void Serialize7bit(Stream dest, short src) => Formatter.Serialize7bit(dest, src);

#if PROTOTYPE
        static abstract void Deserialize7bit(Stream src, ref object dest);
#endif
        static void Deserialize7bit(Stream src, ref ulong dest) => Formatter.Deserialize7bit(src, ref dest);
        static void Deserialize7bit(Stream src, ref uint dest) => Formatter.Deserialize7bit(src, ref dest);
        static void Deserialize7bit(Stream src, ref ushort dest) => Formatter.Deserialize7bit(src, ref dest);
        static void Deserialize7bit(Stream src, ref long dest) => Formatter.Deserialize7bit(src, ref dest);
        static void Deserialize7bit(Stream src, ref int dest) => Formatter.Deserialize7bit(src, ref dest);
        static void Deserialize7bit(Stream src, ref short dest) => Formatter.Deserialize7bit(src, ref dest);
#endregion

#region LinearSave/Load basis
        static void LinearSave<T>(EncodeObjects encode, IEnumerable<T>? src) where T:ISerialize {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(encode.dest, count);
            if (0 < count) {
                foreach (var x in src!) Save(encode, x);
            }
        }

        static void LinearLoad<T>(DecodeObjects decode, Action<T[]> handler) where T:class
        {
            int count = 0;
            Formatter.Deserialize7bit(decode.src, ref count);
            if (0 >= count) return; // no action needed
            var dest = new T[count];
            var stage = new StageArray<T>(dest, handler);

            // function extraction target does not work -- out/ref parameter needs accessing from lambda function
            int n = 0;
            while (0 < count) {
                --count;
                var code = Formatter.DeserializeObjCode(decode.src);
                if (0 < code) {
                    var obj = decode.Seen(code);
                    if (null != obj) {
                        if (obj is T w) dest[n] = w;
                        else throw new InvalidOperationException(nameof(T) + " object not loaded");
                    } else {
                        var i = n;
                        decode.Schedule(code, (o) => {
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
#endregion

        static void LinearSave(EncodeObjects encode, IEnumerable<KeyValuePair<string, object> >? src)
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(encode.dest, count);
            if (0 < count) {
                foreach (var x in src!) Save(encode, x);
            }
        }

        static void LinearLoad(DecodeObjects decode, Action<KeyValuePair<string, object>[]> handler)
        {
            int count = 0;
            Formatter.Deserialize7bit(decode.src, ref count);
            if (0 >= count) return; // no action needed
            var dest = new KeyValuePair<string, object>[count];
            var stage = new StageArray<KeyValuePair<string, object>>(dest, handler);

            // function extraction target does not work -- out/ref parameter needs accessing from lambda function
            int n = 0;
            while (0 < count) {
                --count;
                string t_str = null;
                Formatter.Deserialize(decode.src, ref t_str);
                var code = decode.LoadObject();
                if (null != code.Key) {
                    dest[n++] = new(t_str, code.Key);
                    continue;
                }
                if (0 >= code.Value) throw new InvalidOperationException("object not loaded");

                var i = n;
                decode.Schedule(code.Value, (o) => {
                    dest[i] = new(t_str, o);
                    stage.Verify();
                });
                n++;
            }
            stage.Verify();
        }

        static void LinearSave(EncodeObjects encode, IEnumerable<string>? src)
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(encode.dest, count);
            if (0 < count) {
                foreach (var x in src!) Formatter.Serialize(encode.dest, x);
            }
        }

        static void LinearLoad(DecodeObjects decode, out string[]? dest)
        {
            dest = null;
            int count = 0;
            Formatter.Deserialize7bit(decode.src, ref count);
            if (0 >= count) return; // no action needed
            dest = new string[count];

            // function extraction target does not work -- out/ref parameter needs accessing from lambda function
            int n = 0;
            while (0 < count) {
                --count;
                Formatter.Deserialize(decode.src, ref dest[n++]);
            }
        }
    }

    public class EncodeObjects
    {
        public readonly StreamingContext context;
        public readonly Formatter format;
        public readonly Stream dest;
        private ulong seen = 0;
        private ulong type_seen = 0;
        private Dictionary<Type, ulong> type_code_of = new();
        private Dictionary<Type, Dictionary<object, ulong>> encodings = new();
        private List<Action<Stream>> to_save = new();

        public EncodeObjects(Stream _dest)
        {
            dest = _dest;
            context = new StreamingContext();
            format = new Formatter(context);
        }

        // precondition: src not null
        public ulong Saving(ISerialize src)
        {
            if (null == src) return 0; // likely should handle this at a higher level; signals writing a null code rather than an object reference

            var type = src.GetType();
            var t_code = getTypeCode(type, out var handler);
            if (encodings.TryGetValue(type, out var cache)) {
                if (cache.TryGetValue(src, out ulong code)) return code;
            } else encodings.Add(type, cache = new());
            cache.Add(src, ++seen);
            var local_seen = seen;

            // \todo handle polymorphism -- loader must know which subclass to load
            // yes, appears to be subverting historical architecture
            to_save.Add(dest => {
                if (null != handler) handler(dest);
                Formatter.SerializeTypeCode(dest, t_code);
                Formatter.SerializeObjCode(dest, local_seen);
                if (src is IOnSerializing x) x.OnSerializing(in context);
                src.save(this);
                if (src is IOnSerialized y) y.OnSerialized(in context);
            });
            return seen;
        }

        public bool SaveNext() {
            if (0 >= to_save.Count) return false;

            var next = to_save[0];
            to_save.RemoveAt(0);
            next(dest); // write object itself to hard drive
            return true;
        }

        public void SaveObject(object? src) {
            if (null == src) {
                Formatter.SerializeNull(dest);
                return;
            }
            if (src is ISerialize origin) {
                var code = Saving(origin);
                if (0 == code) Formatter.SerializeNull(dest);
                else Formatter.SerializeObjCode(dest, code);
                return;
            }
            if (Formatter.SaveObject(src, dest)) return;
            throw new InvalidOperationException("fell through EncodeObjects::SaveObject: "+src.GetType().Name);
        }

#region example boilerplate based on LinearizedElement<T>
        private void SaveTo(string src) => Formatter.Serialize(dest, src);

        private void SaveTo(in KeyValuePair<string, string> src)
        {
            SaveTo(src.Key);
            SaveTo(src.Value);
        }
#endregion

#region example boilerplate based on LinearSave<T>
        public void SaveTo(IEnumerable<KeyValuePair<string, string> >? src)
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(dest, count);
            if (0 < count) {
                foreach (var x in src) SaveTo(in x);
            }
        }

        public void SaveTo(IEnumerable<byte>? src)
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(dest, count);
            if (0 < count) {
                foreach (var x in src) Formatter.Serialize(dest, x);
            }
        }

        public void SaveTo7bit(IEnumerable<ulong>? src)
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(dest, count);
            if (0 < count) {
                foreach (var x in src) Formatter.Serialize7bit(dest, x);
            }
        }

        public void SaveTo7bit(IEnumerable<int>? src)
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(dest, count);
            if (0 < count)
            {
                foreach (var x in src) Formatter.Serialize7bit(dest, x);
            }
        }

        public void SaveTo7bit(int[,,]? src)
        {
            var rank = src?.Rank ?? 0;
            if (0 < rank) {
                Span<int> ub = stackalloc int[3];
                var iter = new int[3];
                var n = 0;
                while(rank > n) {
                    ub[n] = src.GetUpperBound(n) + 1;   // non-strict upper bound
                    Formatter.Serialize7bit(dest, ub[n++]);
                }
                iter[0] = 0;
                while (ub[0] > iter[0]) {
                    iter[1] = 0;
                    while (ub[1] > iter[1]) {
                        iter[2] = 0;
                        while (ub[2] > iter[2]) {
                            Formatter.Serialize7bit(dest, (int)src.GetValue(iter));
                            iter[2]++;
                        }
                        iter[1]++;
                    }
                    iter[0]++;
                }
            } else {
                Formatter.Serialize7bit(dest, 0);
            }
        }

        public void SaveTo(byte[,]? src)
        {
            var rank = src?.Rank ?? 0;
            if (0 < rank) {
                Span<int> ub = stackalloc int[2];
                var iter = new int[2];
                var n = 0;
                while(rank > n) {
                    ub[n] = src.GetUpperBound(n) + 1;   // non-strict upper bound
                    Formatter.Serialize7bit(dest, ub[n++]);
                }
                iter[0] = 0;
                while (ub[0] > iter[0]) {
                    iter[1] = 0;
                    while (ub[1] > iter[1]) {
                        Formatter.Serialize(dest, (byte)src.GetValue(iter));
                        iter[1]++;
                    }
                    iter[0]++;
                }
            } else {
                Formatter.Serialize7bit(dest, 0);
            }
        }

        public void SaveTo<T>(IEnumerable<T>? src) where T : ISerialize
        {
            var count = src?.Count() ?? 0;
            Formatter.Serialize7bit(dest, count);
            if (0 < count) {
                foreach (var x in src) {
                    if (null == x) Formatter.SerializeNull(dest);
                    else {
                        var code = Saving(x);
                        if (0 == code) Formatter.SerializeNull(dest);
                        else Formatter.SerializeObjCode(dest, code);
                    }
                }
            }
        }

        public void SaveTo<T>(T[,]? src) where T:ISerialize
        {
            var rank = src?.Rank ?? 0;
            if (0 < rank) {
                Span<int> ub = stackalloc int[2];
                var iter = new int[2];
                var n = 0;
                while(rank > n) {
                    ub[n] = src.GetUpperBound(n) + 1;   // non-strict upper bound
                    Formatter.Serialize7bit(dest, ub[n++]);
                }
                iter[0] = 0;
                while (ub[0] > iter[0]) {
                    iter[1] = 0;
                    while (ub[1] > iter[1]) {
                        T stage = (T)src.GetValue(iter);
                        if (null == stage) Formatter.SerializeNull(dest);
                        else {
                            var code = Saving(stage);
                            if (0 == code) Formatter.SerializeNull(dest);
                            else Formatter.SerializeObjCode(dest, code);
                        }
                        iter[1]++;
                    }
                    iter[0]++;
                }
            } else {
                Formatter.Serialize7bit(dest, 0);
            }
        }
#endregion

        private ulong getTypeCode(Type src, out Action<Stream>? exec) {
            exec = null;
            if (type_code_of.TryGetValue(src, out var code)) return code;
            type_code_of.Add(src, ++type_seen);

            var t_code = type_seen;
            exec = dest => Formatter.SerializeTypeCode(dest, t_code, src.FullName);

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
            using var stream = filepath.CreateStream(true);
            var encode = new EncodeObjects(stream);
            encode.Saving(src);
            while (encode.SaveNext());
            stream.Flush();
        }
    }
}
