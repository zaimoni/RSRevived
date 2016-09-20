using System.IO;

namespace Zaimoni.Data
{
    class OutTextFile
    {
        public readonly string filepath;
        private StreamWriter _file = null;

        OutTextFile(string dest) {
            filepath = dest;
        }

        public void WriteLine(string src) {
            lock (filepath) {
                if (null == _file) {
                    if (File.Exists(filepath)) File.Delete(filepath);
                    _file = File.CreateText(filepath);
                }
                _file.WriteLine(src);
                _file.Flush();
            }
        }

        public void Close() {   // XXX C# goes inefficient with a true destructor so don't provide one
            lock (filepath) {
                if (null != _file) {
                    _file.Close();
                    _file = null;
                }
            }
        }
    }
}
