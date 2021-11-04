using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zaimoni.Serialization
{
    // 2021-05-21: policy change ... just hard-code for binary formatting
    // likely JSON/XML will be covered by others indefinitely
    public class Formatter
    {
        private readonly StreamingContext _context;
        private ulong version = 0;
        private sbyte preview = 0;

        // sbyte values -8 ... 8 are used by the integer encoding subsystem
        // we likely want to reserve "nearest 127/-128" first, as a long-range future-resistance scheme

        public const sbyte null_code = sbyte.MaxValue;
        public const sbyte obj_ref_code = sbyte.MinValue;
        public const sbyte type_code = sbyte.MinValue + 1;
        public const sbyte type_ref_code = sbyte.MinValue + 2;

        public Formatter(StreamingContext context) {
            _context = context;
        }

#if OBSOLETE
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
#endif

#if FAIL
        // this strategy doesn't actually work in C#: we don't have the means to
        // call a non-generic function from a generic function efficiently.
        // (System.Reflection is not efficient)
        protected abstract bool trivialSerialize<T>(Stream dest, T src);
        protected abstract bool trivialDeserialize<T>(Stream dest, ref T src);
#endif

#region version support
        ulong Version {
            get { return version; }
            set {
                if (0 < version) throw new InvalidOperationException("can only set version once");
                if (0 < value) version = value;
            }
        }

        void ReadVersion(Stream dest) {
            if (0 >= version) Deserialize7bit(dest, ref version);
        }
        void SaveVersion(Stream dest) {
            if (0 < version) Serialize7bit(dest, version);
        }
#endregion

        // 7-bit encoding/decoding of unsigned integers was supported by BinaryReader/BinaryWriter.  Not of use for text formats.
        // 2021-04-24: Can't predict whether BinaryReader/BinaryWriter is included in the deprecation, so re-implement.
#region 7-bit encoding
        static public void Serialize7bit(Stream dest, ulong src)
        {
            Span<byte> relay = stackalloc byte[10];
            int ub = 0;
            while ((ulong)(sbyte.MaxValue) < src) {
                relay[ub++] = (byte)(src % 128 + 128);
                src /= 128;
            }
            relay[ub++] = (byte)src;
            dest.Write(relay.Slice(0, ub));
        }
        static public void Serialize7bit(Stream dest, uint src) => Serialize7bit(dest, (ulong)src);
        static public void Serialize7bit(Stream dest, ushort src) => Serialize7bit(dest, (ulong)src);

        static public void Serialize7bit(Stream dest, long src)
        {
            if (0 > src) throw new InvalidOperationException("cannot encode negative integer in 7-bit encoding");
            Serialize7bit(dest, (ulong)src);
        }

        static public void Serialize7bit(Stream dest, int src)
        {
            if (0 > src) throw new InvalidOperationException("cannot encode negative integer in 7-bit encoding");
            Serialize7bit(dest, (ulong)src);
        }

        static public void Serialize7bit(Stream dest, short src)
        {
            if (0 > src) throw new InvalidOperationException("cannot encode negative integer in 7-bit encoding");
            Serialize7bit(dest, (ulong)src);
        }

        static private ulong Deserialize7bit(Stream src)
        {
            ulong scale = 1;
            ulong dest = 0;
            byte relay = 0;
            Deserialize(src, ref relay);
            while (sbyte.MaxValue < relay) {
                dest += scale * (ulong)(relay % 128);
                scale *= 128;
                Deserialize(src, ref relay);
            }
            dest += scale * relay;
            return dest;
        }

        static public void Deserialize7bit(Stream src, ref ulong dest) => dest = Deserialize7bit(src);

        static public void Deserialize7bit(Stream src, ref uint dest)
        {
            var staging = Deserialize7bit(src);
            if (uint.MaxValue < staging) throw new InvalidDataException("huge uint found");
            dest = (uint)staging;
        }

        static public void Deserialize7bit(Stream src, ref ushort dest)
        {
            var staging = Deserialize7bit(src);
            if (ushort.MaxValue < staging) throw new InvalidDataException("huge ushort found");
            dest = (ushort)staging;
        }

        static public void Deserialize7bit(Stream src, ref long dest)
        {
            var staging = Deserialize7bit(src);
            if (long.MaxValue < staging) throw new InvalidDataException("huge int found");
            dest = (long)staging;
        }

        static public void Deserialize7bit(Stream src, ref int dest)
        {
            var staging = Deserialize7bit(src);
            if (int.MaxValue < staging) throw new InvalidDataException("huge int found");
            dest = (int)staging;
        }

        static public void Deserialize7bit(Stream src, ref short dest)
        {
            var staging = Deserialize7bit(src);
            if ((ulong)(short.MaxValue) < staging) throw new InvalidDataException("huge short found");
            dest = (short)staging;
        }
#endregion

        // structs have to go through code generation using these as exemplars
#region integer basis
        static private sbyte _format(ulong src, in Span<byte> dest)
        {
            sbyte ub = 0;
            while (0 < src) {
                dest[ub++] = (byte)(src % 256);
                src /= 256;
            }
            return ub;
        }

        // following file:///C:/Ruby27-x64/share/doc/ruby/html/marshal_rdoc.html
        static public void Serialize(Stream dest, ulong src)
        {
            Span<byte> relay = stackalloc byte[8];
            var ub = _format(src, in relay);
            Serialize(dest, ub);
            if (1 <= ub) {
                byte scan = 0;
                do {
                    Serialize(dest, relay[scan]);
                } while (++scan < ub);
            }
        }

        // we, like Ruby, are relying on the hardware signed integer format being 2's-complement
        // that is, "trailing" 0xFF are not significant
        // the two other C-supported representations for negative integers require different handling
        static public void Serialize(Stream dest, long src)
        {
            if (0 <= src) {
                Serialize(dest, (ulong)src);
                return;
            }

            Span<byte> relay = stackalloc byte[8];
            var ub = _format((ulong)src, in relay);
            while (2 <= ub && 255 == relay[ub - 1]) --ub;
            Serialize(dest, (sbyte)(-ub));
            if (1 <= ub) {
                byte scan = 0;
                do {
                    Serialize(dest, relay[scan]);
                } while (++scan < ub);
            }
        }

        static public void Deserialize(Stream src, ref ulong dest)
        {
            dest = 0;
            ulong scale = 1;
            sbyte ub = 0;
            Deserialize(src, ref ub);
            if (0 > ub || 8 < ub) throw new InvalidDataException("does not fit in ulong");
            byte scan = 0;
            while (0 < ub) {
                Deserialize(src, ref scan);
                dest += scale * scan;
                scale *= 256;
                --ub;
            }
        }

        static public void Deserialize(Stream src, ref long dest)
        {
            dest = 0;
            long scale = 1;
            sbyte ub = 0;
            Deserialize(src, ref ub);
            if (-8 > ub || 8 < ub) throw new InvalidDataException("does not fit in long");
            byte scan = 0;
            while (0 < ub) {
                Deserialize(src, ref scan);
                dest += scale * scan;
                scale *= 256;
                --ub;
            }
            if (0 <= ub) return;
            while (0 > ub) {
                Deserialize(src, ref scan);
                var test = scan - 256;
                dest += scale * test;
                scale *= 256;
                ++ub;
            }
        }
#endregion

#region integer adapters
        static public void Serialize(Stream dest, uint src) { Serialize(dest, (ulong)src); }
        static public void Serialize(Stream dest, ushort src) { Serialize(dest, (ulong)src); }

        static public void Serialize(Stream dest, int src) { Serialize(dest, (long)src); }
        static public void Serialize(Stream dest, short src) { Serialize(dest, (long)src); }

        static public void Deserialize(Stream src, ref uint dest) {
            ulong relay = 0;
            Deserialize(src, ref relay);
            if (uint.MaxValue < relay) throw new InvalidDataException("huge uint found");
            dest = (uint)relay;
        }

        static public void Deserialize(Stream src, ref ushort dest) {
            ulong relay = 0;
            Deserialize(src, ref relay);
            if (ushort.MaxValue < relay) throw new InvalidDataException("huge ushort found");
            dest = (ushort)relay;
        }

        static public void Deserialize(Stream src, ref int dest)
        {
            long relay = 0;
            Deserialize(src, ref relay);
            if (int.MaxValue < relay || int.MinValue > relay) throw new InvalidDataException("huge int found");
            dest = (int)relay;
        }

        static public void Deserialize(Stream src, ref short dest)
        {
            long relay = 0;
            Deserialize(src, ref relay);
            if (short.MaxValue < relay || short.MinValue > relay) throw new InvalidDataException("huge short found");
            dest = (short)relay;
        }
#endregion

#region byte basis
        static public void Serialize(Stream dest, byte src) => dest.WriteByte(src);
        static public void Serialize(Stream dest, sbyte src) => dest.WriteByte((byte)src);

        static private byte ReadByte(Stream src) {
            var code = src.ReadByte();
            if (-1 == code) throw new InvalidDataException("stream ended unexpectedly");
            return (byte)code;
        }

        static public void Deserialize(Stream src, ref byte dest) { dest = ReadByte(src); }
        static public void Deserialize(Stream src, ref sbyte dest) { dest = (sbyte)ReadByte(src); }
#endregion

        public sbyte Preview { get { return preview; } }
        public sbyte Peek(Stream src) {
            preview = (sbyte)ReadByte(src);
            return preview;
        }

#region object references
        static public void SerializeNull(Stream dest) => Serialize(dest, null_code);
        static public void SerializeObjCode(Stream dest, ulong code)
        {
            Serialize(dest, obj_ref_code);
            Serialize7bit(dest, code);
        }

        static public ulong DeserializeObjCode(Stream src)
        {
            sbyte signal = 0;
            Deserialize(src, ref signal);
            if (null_code == signal) return 0;
            if (obj_ref_code != signal) throw new InvalidDataException("expected object reference: "+obj_ref_code.ToString() + " " + signal.ToString());
            return Deserialize7bit(src);
        }

        public ulong DeserializeObjCodeAfterTypecode(Stream src)
        {
            if (null_code == Preview) return 0;
            if (obj_ref_code != Preview) throw new InvalidDataException("expected object reference: " + obj_ref_code.ToString() + " " + Preview.ToString());
            return Deserialize7bit(src); // ideally would reset Preview to an invalid state
        }

        static public void SerializeTypeCode(Stream dest, ulong code, string name)
        {
            Serialize(dest, type_code);
            Serialize7bit(dest, code);
            Serialize(dest, name);
        }

        public void DeserializeTypeCode(Stream src, Dictionary<ulong, Type> dest)
        {
            while (type_code == preview) {
                ulong stage_code = 0;
                string stage_name = string.Empty;
                Deserialize7bit(src, ref stage_code);
                if (0 == stage_code) throw new InvalidOperationException("invalid type encoding");
                if (dest.ContainsKey(stage_code)) throw new InvalidOperationException("duplicate type encoding");
                Deserialize(src, ref stage_name);
                var type = Type.GetType(stage_name);
                if (null == type) throw new InvalidOperationException("did not recover type from its name");
                dest.Add(stage_code, type);
                Peek(src);
            }
        }

        static public void SerializeTypeCode(Stream dest, ulong code)
        {
            Serialize(dest, type_ref_code);
            Serialize7bit(dest, code);
        }

        public ulong DeserializeTypeCode(Stream src)
        {
            if (type_ref_code != preview) throw new InvalidOperationException("did not find expected type code: "+ preview);
            ulong code = 0;
            Deserialize7bit(src, ref code);
            if (0 == code) throw new InvalidOperationException("found null type code");
            Peek(src);
            return code;
        }
#endregion

#region strings
        static private void Serialize(Stream dest, Rune src)
        {
            Span<byte> relay = stackalloc byte[4]; // 2021-04-22: currently 4 (check on compiler upgrade)
            var encoded = src.EncodeToUtf8(relay);
            dest.Write(relay.Slice(0, encoded));
        }

        static public void Serialize(Stream dest, string src)
        {
            var runes = src.EnumerateRunes().ToArray();
            var ub = runes.Length;
            var bytes = 0;
            var i = 0;
            while (i < ub) bytes += runes[i++].Utf8SequenceLength;

            Serialize7bit(dest, bytes); // record byte-length explicitly

            i = 0;
            while (i < ub) Serialize(dest, runes[i++]);
        }

        static public void Deserialize(Stream src, ref string dest)
        {
            int bytes = 0;
            Deserialize7bit(src, ref bytes);
            if (0 == bytes) {
                dest = string.Empty;
                return;
            }

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
                var code = Rune.DecodeFromUtf8(relay, out var result, out var bytesConsumed);
                if (System.Buffers.OperationStatus.Done != code) throw new InvalidDataException("string corrupt: "+code.ToString()+", "+total_read.ToString()+", "+bytes.ToString()+", "+bytesConsumed.ToString());
                var n = result.EncodeToUtf16(encode);
                dest += new string(encode.Slice(0, n));
                bytes -= bytesConsumed;
                if (0 == bytes) return;
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

#region enums
        static public void SerializeEnum<T>(Stream dest, T src) where T : IConvertible // catches enums, and some others
        {
            var e_type = typeof(T);
            switch (Type.GetTypeCode(Enum.GetUnderlyingType(e_type))) // but this will hard-fail on non-enums
            {
            case TypeCode.Byte:
                Serialize(dest, src.ToByte(null));
                return;
            case TypeCode.SByte:
                Serialize(dest, src.ToSByte(null));
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

        static public void DeserializeEnum<T>(Stream src, ref T dest) where T : Enum
        {
            var e_type = typeof(T);
            switch (Type.GetTypeCode(Enum.GetUnderlyingType(e_type)))
            {
            case TypeCode.Byte:
                {
                byte relay = 0;
                Deserialize(src, ref relay);
                dest = (T)Enum.ToObject(e_type, relay);
                }
                return;
            case TypeCode.SByte:
                {
                sbyte relay = 0;
                Deserialize(src, ref relay);
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

        // Policy decision needed regarding where types from the standard library go (Formatter vs. DecodeObjects/EncodeObjects pair)
#region thin-wrappers around core types specified above
        static public void Serialize(Stream dest, TimeSpan src) => Serialize(dest, src.Ticks);

        static public void Deserialize(Stream src, ref TimeSpan dest)
        {
            long relay = 0;
            Deserialize(src, ref relay);
            dest = new TimeSpan(relay);
        }
#endregion
    }
}
