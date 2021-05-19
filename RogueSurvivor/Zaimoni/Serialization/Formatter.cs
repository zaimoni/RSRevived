using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zaimoni.Serialization
{
    abstract public class Formatter
    {
        protected readonly StreamingContext _context;

        public Formatter(StreamingContext context) {
            _context = context;
        }

        // need to work out how to handle struct and enum (defective C# generics compared to C++)
        public T Deserialize<T>(Stream src) where T:class
        {
            if (!src.CanRead) throw new InvalidOperationException(nameof(src)+" cannot read");
            var anchor = new LoadManager(_context);
            return Deserialize<T>(src, anchor);
        }

        public void Serialize<T>(Stream dest, T src) where T:class
        {
            if (!dest.CanWrite) throw new InvalidOperationException(nameof(src) + " cannot write");
            var anchor = new SaveManager(_context);
            Serialize(dest, src, anchor);
        }

        private T Deserialize<T>(Stream src, LoadManager anchor) where T:class
        {
            var expected = typeof(T);
            // if we have a constructor from formatter, call it
            // if we have a static override, use it

            // obsolete:
            // read the type spec from the stream
            // then pull in the top-level type if we recognize it
            return default;
        }

        private void Serialize<T>(Stream dest, T src, SaveManager anchor) where T:class
        {
            var expected = typeof(T);
            // if we have a direct-saver, use it

            // obsolete:
            // ensure the type T is encoded in the stream
            // then
        }

        private void Deserialize<T>(Stream dest, ref T src, LoadManager anchor) where T : class
        {
            // ensure the type T is encoded in the stream
            // then
        }

#if FAIL
        // this strategy doesn't actually work in C#: we don't have the means to
        // call a non-generic function from a generic function efficiently.
        // (System.Reflection is not efficient)
        protected abstract bool trivialSerialize<T>(Stream dest, T src);
        protected abstract bool trivialDeserialize<T>(Stream dest, ref T src);
#endif
        // structs have to go through code generation using these as exemplars
#region integer basis
        public abstract void Serialize(Stream dest, ulong src);
        public abstract void Serialize(Stream dest, long src);
        public abstract void Deserialize(Stream src, ref ulong dest);
        public abstract void Deserialize(Stream src, ref long dest);
#endregion

#region integer adapters
        public void Serialize(Stream dest, uint src) { Serialize(dest, (ulong)src); }
        public void Serialize(Stream dest, ushort src) { Serialize(dest, (ulong)src); }

        public void Serialize(Stream dest, int src) { Serialize(dest, (long)src); }
        public void Serialize(Stream dest, short src) { Serialize(dest, (long)src); }

        public void Deserialize(Stream src, ref uint dest) {
            ulong relay = 0;
            Deserialize(src, ref relay);
            if (uint.MaxValue < relay) throw new InvalidDataException("huge uint found");
            dest = (uint)relay;
        }

        public void Deserialize(Stream src, ref ushort dest) {
            ulong relay = 0;
            Deserialize(src, ref relay);
            if (ushort.MaxValue < relay) throw new InvalidDataException("huge ushort found");
            dest = (ushort)relay;
        }

        public void Deserialize(Stream src, ref int dest)
        {
            long relay = 0;
            Deserialize(src, ref relay);
            if (int.MaxValue < relay || int.MinValue > relay) throw new InvalidDataException("huge int found");
            dest = (int)relay;
        }

        public void Deserialize(Stream src, ref short dest)
        {
            long relay = 0;
            Deserialize(src, ref relay);
            if (short.MaxValue < relay || short.MinValue > relay) throw new InvalidDataException("huge short found");
            dest = (short)relay;
        }
#endregion

#region byte basis
        public void Serialize(Stream dest, byte src) { trivialSerialize(dest, src); }
        public void Serialize(Stream dest, sbyte src) { trivialSerialize(dest, src); }
        public void Deserialize(Stream src, ref byte dest) { trivialDeserialize(src, ref dest); }
        public void Deserialize(Stream src, ref sbyte dest) { trivialDeserialize(src, ref dest); }

        // outside users should not need to provide additional overrides for the trivialSerialize/trivialDeserialize naming scheme
        protected abstract void trivialSerialize(Stream dest, byte src);
        protected abstract void trivialDeserialize(Stream src, ref byte dest);
        protected abstract void trivialSerialize(Stream dest, sbyte src);
        protected abstract void trivialDeserialize(Stream src, ref sbyte dest);
#endregion

#region object references
        public abstract void SerializeNull(Stream dest);
        public abstract void SerializeObjCode(Stream dest, ulong code);
        public abstract ulong DeserializeObjCode(Stream src);
#endregion

#region strings
        public abstract void Serialize(Stream dest, string src);
        public abstract void Deserialize(Stream src, ref string dest);
#endregion

#region enums
        public void SerializeEnum<T>(Stream dest, T src) where T : IConvertible // catches enums, and some others
        {
            var e_type = typeof(T);
            switch (Type.GetTypeCode(Enum.GetUnderlyingType(e_type))) // but this will hard-fail on non-enums
            {
            case TypeCode.Byte:
                trivialSerialize(dest, src.ToByte(null));
                return;
            case TypeCode.SByte:
                trivialSerialize(dest, src.ToSByte(null));
                return;
            case TypeCode.Int16:
                Serialize(dest, src.ToInt16(null));
                return;
            case TypeCode.Int32:
                Serialize(dest, src.ToInt32(null));
                return;
            case TypeCode.Int64:
                Serialize(dest, src.ToInt64(null));
                return;
            case TypeCode.UInt16:
                Serialize(dest, src.ToUInt16(null));
                return;
            case TypeCode.UInt32:
                Serialize(dest, src.ToUInt32(null));
                return;
            case TypeCode.UInt64:
                Serialize(dest, src.ToUInt64(null));
                return;
            default: throw new InvalidOperationException("SerializeEnum cannot handle "+e_type.ToString());
            }
        }

        public void DeserializeEnum<T>(Stream src, ref T dest) where T : Enum
        {
            var e_type = typeof(T);
            switch (Type.GetTypeCode(Enum.GetUnderlyingType(e_type)))
            {
            case TypeCode.Byte:
                {
                byte relay = 0;
                trivialDeserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.SByte:
                {
                sbyte relay = 0;
                trivialDeserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.Int16:
                {
                short relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.Int32:
                {
                int relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.Int64:
                {
                long relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.UInt16:
                {
                ushort relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.UInt32:
                {
                uint relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.UInt64:
                {
                ulong relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            default: throw new InvalidOperationException("DeserializeEnum cannot handle "+e_type.ToString());
            }
        }
#endregion
    }

    public class BinaryFormatter : Formatter
    {
        public BinaryFormatter(StreamingContext context) : base(context) { }

        // sbyte values -8 ... 8 are used by the integer encoding subsystem
        // we likely want to reserve "nearest 127/-128" first, as a long-range future-resistance scheme

        const sbyte null_code = sbyte.MaxValue;
        const sbyte obj_ref_code = sbyte.MinValue;

        // 7-bit encoding/decoding of unsigned integers was supported by BinaryReader/BinaryWriter.  Not of use for text formats.
        // 2021-04-24: Can't predict whether BinaryReader/BinaryWriter is included in the deprecation, so re-implement.
#region 7-bit encoding
        protected void Serialize7bit(Stream dest, ulong src) {
            Span<byte> relay = stackalloc byte[10];
            int ub = 0;
            while ((ulong)(sbyte.MaxValue) < src) {
                relay[ub++] = (byte)(src % 128 + 128);
                src /= 128;
            }
            relay[ub++] = (byte)src;
            dest.Write(relay.Slice(0, ub));
        }
        protected void Serialize7bit(Stream dest, uint src) { Serialize7bit(dest, (ulong)src); }
        protected void Serialize7bit(Stream dest, ushort src) { Serialize7bit(dest, (ulong)src); }

        protected void Serialize7bit(Stream dest, long src)
        {
            if (0 > src) throw new InvalidOperationException("cannot encode negative integer in 7-bit encoding");
            Serialize7bit(dest, (ulong)src);
        }

        protected void Serialize7bit(Stream dest, int src) {
            if (0 > src) throw new InvalidOperationException("cannot encode negative integer in 7-bit encoding");
            Serialize7bit(dest, (ulong)src);
        }

        protected void Serialize7bit(Stream dest, short src)
        {
            if (0 > src) throw new InvalidOperationException("cannot encode negative integer in 7-bit encoding");
            Serialize7bit(dest, (ulong)src);
        }

        protected ulong Deserialize7bit(Stream src) {
            ulong scale = 1;
            ulong dest = 0;
            byte relay = 0;
            trivialDeserialize(src, ref relay);
            while (sbyte.MaxValue < relay) {
                dest += scale * (ulong)(relay % 128);
                scale *= 128;
                trivialDeserialize(src, ref relay);
            }
            dest += scale * relay;
            return dest;
        }

        protected void Deserialize7bit(Stream src, ref ulong dest)
        {
            dest = Deserialize7bit(src);
        }

        protected void Deserialize7bit(Stream src, ref uint dest)
        {
            var staging = Deserialize7bit(src);
            if (uint.MaxValue < staging) throw new InvalidDataException("huge uint found");
            dest = (uint)staging;
        }

        protected void Deserialize7bit(Stream src, ref ushort dest)
        {
            var staging = Deserialize7bit(src);
            if (ushort.MaxValue < staging) throw new InvalidDataException("huge ushort found");
            dest = (ushort)staging;
        }

        protected void Deserialize7bit(Stream src, ref long dest)
        {
            var staging = Deserialize7bit(src);
            if (long.MaxValue < staging) throw new InvalidDataException("huge int found");
            dest = (long)staging;
        }

        protected void Deserialize7bit(Stream src, ref int dest)
        {
            var staging = Deserialize7bit(src);
            if (int.MaxValue < staging) throw new InvalidDataException("huge int found");
            dest = (int)staging;
        }

        protected void Deserialize7bit(Stream src, ref short dest)
        {
            var staging = Deserialize7bit(src);
            if ((ulong)(short.MaxValue) < staging) throw new InvalidDataException("huge short found");
            dest = (short)staging;
        }
#endregion

#if FAIL
        protected override bool trivialSerialize<T>(Stream dest, T src) {
            var method = typeof(BinaryFormatter).GetMethod("_trivialSerialize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic,
                null, System.Reflection.CallingConventions.Standard, new Type[]{ typeof(Stream), typeof(T)}, null);
            if (null == method) return false;
            method.Invoke(null, new object[] { src });
            return true;
        }

        protected override bool trivialDeserialize<T>(Stream dest, ref T src) {
            var method = typeof(BinaryFormatter).GetMethod("_trivialDeserialize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic,
                null, System.Reflection.CallingConventions.Standard, new Type[]{ typeof(Stream), typeof(T).MakeByRefType() }, null);
            if (null == method) return false;
            T relay;
            method.Invoke(null, new object[] { dest, relay });
            src = relay;
            return true;
        }

        static private void _trivialSerialize(Stream dest, byte src) { dest.WriteByte(src); }
        static private void _trivialSerialize(Stream dest, char src) { dest.WriteByte((byte)src); }

        static private void _trivialDeserialize(Stream src, ref byte dest) {
            var code = src.ReadByte();
            if (-1 == code) throw new InvalidOperationException("stream ended unexpectedly");
            dest = (byte)code;
        }

        static private void _trivialDeserialize(Stream src, ref char dest)
        {
            var code = src.ReadByte();
            if (-1 == code) throw new InvalidOperationException("stream ended unexpectedly");
            dest = (char)code;
        }
#endif

#region byte basis
        protected override void trivialSerialize(Stream dest, byte src) { dest.WriteByte(src); }
        protected override void trivialSerialize(Stream dest, sbyte src) { dest.WriteByte((byte)src); }

        protected override void trivialDeserialize(Stream src, ref byte dest)
        {
            var code = src.ReadByte();
            if (-1 == code) throw new InvalidDataException("stream ended unexpectedly");
            dest = (byte)code;
        }
        protected override void trivialDeserialize(Stream src, ref sbyte dest)
        {
            var code = src.ReadByte();
            if (-1 == code) throw new InvalidDataException("stream ended unexpectedly");
            dest = (sbyte)code;
        }
#endregion

#region integer basis
        private sbyte _format(ulong src, in Span<byte> dest)
        {
            sbyte ub = 0;
            while (0 < src) {
                dest[ub++] = (byte)(src % 256);
                src /= 256;
            }
            return ub;
        }

        // following file:///C:/Ruby27-x64/share/doc/ruby/html/marshal_rdoc.html
        public override void Serialize(Stream dest, ulong src) {
            Span<byte> relay = stackalloc byte[8];
            var ub = _format(src, in relay);
            trivialSerialize(dest, ub);
            if (1 <= ub) {
                byte scan = 0;
                do {
                    trivialSerialize(dest, relay[scan]);
                } while (++scan < ub);
            }
        }

        public override void Deserialize(Stream src, ref ulong dest) {
            dest = 0;
            ulong scale = 1;
            sbyte ub = 0;
            trivialDeserialize(src, ref ub);
            if (0 > ub || 8 < ub) throw new InvalidDataException("does not fit in ulong");
            byte scan = 0;
            while (0 < ub) {
                trivialDeserialize(src, ref scan);
                dest += scale * scan;
                scale *= 256;
                --ub;
            }
        }

        // we, like Ruby, are relying on the hardware signed integer format being 2's-complement
        // that is, "trailing" 0xFF are not significant
        // the two other C-supported representations for negative integers require different handling
        public override void Serialize(Stream dest, long src)
        {
            if (0 <= src) {
                Serialize(dest, (ulong)src);
                return;
            }

            Span<byte> relay = stackalloc byte[8];
            var ub = _format((ulong)src, in relay);
            while (2 <= ub && 255 == relay[ub - 1]) --ub;
            trivialSerialize(dest, (sbyte)(-ub));
            if (1 <= ub) {
                byte scan = 0;
                do {
                    trivialSerialize(dest, relay[scan]);
                } while (++scan < ub);
            }
        }

        public override void Deserialize(Stream src, ref long dest) {
            dest = 0;
            long scale = 1;
            sbyte ub = 0;
            trivialDeserialize(src, ref ub);
            if (-8 > ub || 8 < ub) throw new InvalidDataException("does not fit in long");
            byte scan = 0;
            while (0 < ub) {
                trivialDeserialize(src, ref scan);
                dest += scale * scan;
                scale *= 256;
                --ub;
            }
            if (0 <= ub) return;
            while (0 > ub) {
                trivialDeserialize(src, ref scan);
                var test = scan - 256;
                dest += scale * test;
                scale *= 256;
                ++ub;
            }
        }
#endregion

#region object references
        public override void SerializeNull(Stream dest)
        {
            trivialSerialize(dest, null_code);
        }

        public override void SerializeObjCode(Stream dest, ulong code)
        {
            // \todo? micro-optimization: integrate the size of the encoding into the signal byte
            trivialSerialize(dest, obj_ref_code);
            Serialize7bit(dest, code);
        }

        public override ulong DeserializeObjCode(Stream src)
        {
            sbyte signal = 0;
            trivialDeserialize(src, ref signal);
            if (null_code == signal) return 0;
            if (obj_ref_code != signal) throw new InvalidDataException("expected object reference");
            return Deserialize7bit(src);
        }
#endregion

#region strings
        private void Serialize(Stream dest, Rune src)
        {
            Span<byte> relay = stackalloc byte[4]; // 2021-04-22: currently 4 (check on compiler upgrade)
            var encoded = src.EncodeToUtf8(relay);
            dest.Write(relay.Slice(0, encoded));
        }

        public override void Serialize(Stream dest, string src) {
            var runes = src.EnumerateRunes().ToArray();
            var ub = runes.Length;
            var bytes = 0;
            var i = 0;
            while (i < ub) bytes += runes[i++].Utf8SequenceLength;

            Serialize7bit(dest, bytes); // record byte-length explicitly

            i = 0;
            while (i < ub) Serialize(dest, runes[i++]);
        }

        public override void Deserialize(Stream src, ref string dest)
        {
            int bytes = 0;
            Deserialize7bit(src, ref bytes);

            int total_read = 0;
            Span<byte> relay = new byte[bytes];

            while (4 > total_read && total_read < bytes) {
                var read = src.Read(relay.Slice(total_read));
                if (0 >= read) throw new InvalidDataException("string truncated");
                total_read += read;
            }

            Span<char> encode = stackalloc char[2]; // 2021-04-22: currently 2 (check on compiler upgrade)
            dest = string.Empty;
            while (4 <= total_read || total_read == bytes) {
                if (System.Buffers.OperationStatus.Done != Rune.DecodeFromUtf8(relay, out var result, out var bytesConsumed)) throw new InvalidDataException("string corrupt");
                var n = result.EncodeToUtf16(encode);
                dest += new string(encode.Slice(0, n));
                bytes -= bytesConsumed;
                total_read -= bytesConsumed;
                relay = relay.Slice(bytesConsumed);
                while (4 > total_read && total_read < bytes) {
                    var read = src.Read(relay.Slice(total_read));
                    if (0 >= read) throw new InvalidDataException("string truncated");
                    total_read += read;
                }
            }
        }
#endregion
    }
}
