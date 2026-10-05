using System;
using System.IO;

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
	}
}
