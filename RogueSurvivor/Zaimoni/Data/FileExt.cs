using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics.Contracts;

namespace Zaimoni.Data
{
	static class FileExt
	{
		public static Stream CreateStream(this string filepath, bool save)
		{
			Contract.Requires(!string.IsNullOrEmpty(filepath));
			return new FileStream(filepath, save ? FileMode.Create : FileMode.Open, save ? FileAccess.Write : FileAccess.Read, save ? FileShare.None : FileShare.Read);
		}

		public static void BinarySerialize<_T_>(this string filepath, _T_ src)
		{
			Contract.Requires(!string.IsNullOrEmpty(filepath));
			using (Stream stream = filepath.CreateStream(true)) {
				(new BinaryFormatter()).Serialize(stream, src);
				stream.Flush();
			}
		}
		public static _T_ BinaryDeserialize<_T_>(this string filepath)
		{
			Contract.Requires(!string.IsNullOrEmpty(filepath));
			using (Stream stream = filepath.CreateStream(false)) {
				return (_T_)(new BinaryFormatter()).Deserialize(stream);
			}
		}
	}
}
