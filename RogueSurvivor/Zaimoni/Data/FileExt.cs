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
            using (Stream stream = filepath.CreateStream(true)) {
				(new BinaryFormatter()).Serialize(stream, src);
				stream.Flush();
			}
		}
		public static _T_ BinaryDeserialize<_T_>(this string filepath)
		{
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            using (Stream stream = filepath.CreateStream(false)) {
				return (_T_)(new BinaryFormatter()).Deserialize(stream);
			}
		}

#nullable enable
        public static void read<T>(this SerializationInfo info, ref T dest, string src) where T : class
        {
            var tmp = info.GetValue(src, typeof(T)) as T; // should have thrown already but this function doesn't have proper annotations
            if (null == tmp) throw new ArgumentNullException(src);
            else dest = tmp;
        }
#nullable restore
	}
}
