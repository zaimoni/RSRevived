using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Zaimoni.Data
{
    public interface Factory<out T>
    {
      T create();
    }

	static class FileExt
	{
		public static Stream CreateStream(this string filepath, bool save)
		{
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            return new FileStream(filepath, save ? FileMode.Create : FileMode.Open, save ? FileAccess.Write : FileAccess.Read, save ? FileShare.None : FileShare.Read);
		}

		public static void BinarySerialize<_T_>(this string filepath, _T_ src)
		{
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            using var stream = filepath.CreateStream(true);
			(new BinaryFormatter()).Serialize(stream, src);
			stream.Flush();
		}
		public static _T_ BinaryDeserialize<_T_>(this string filepath)
		{
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            using var stream = filepath.CreateStream(false);
    		return (_T_)(new BinaryFormatter()).Deserialize(stream);
		}

        // Configuration files: attribute-driven JSON, readable, enums by name (phase 4 step 2).
        private static readonly System.Text.Json.JsonSerializerOptions s_config_opts = new() {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,   // not HTML: quotes and non-ASCII as themselves
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public static void JsonSerialize<_T_>(this string filepath, _T_ src)
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            using var stream = filepath.CreateStream(true);
            System.Text.Json.JsonSerializer.Serialize(stream, src, s_config_opts);
            stream.Flush();
        }

        public static _T_ JsonDeserialize<_T_>(this string filepath)
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            using var stream = filepath.CreateStream(false);
            var ret = System.Text.Json.JsonSerializer.Deserialize<_T_>(stream, s_config_opts);
            if (null == ret) throw new InvalidDataException("no " + typeof(_T_).Name + " in " + filepath);
            return ret;
        }

#nullable enable
        public static void read<T>(this SerializationInfo info, ref T dest, string src) where T : class
        {
            dest = info.GetValue(src, typeof(T)) as T ?? throw new ArgumentNullException(src);
        }

        public static void read_nullsafe<T>(this SerializationInfo info, ref T? dest, string src) where T : class
        {
            dest = info.GetValue(src, typeof(T)) as T; // should have thrown already but this function doesn't have proper annotations
        }

        public static void read_s<T>(this SerializationInfo info, ref T dest, string src) where T : struct // different function name due to defective C# generic functions relative to C++ templates
        {
            object? tmp = info.GetValue(src, typeof(T)); // should have thrown already but this function doesn't have proper annotations
            dest = (T)(tmp ?? throw new ArgumentNullException(src));
        }
#nullable restore
	}
}
