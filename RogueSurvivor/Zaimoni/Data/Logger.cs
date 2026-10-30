using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Zaimoni.Data
{
    static public partial class Logger
    {
        readonly private static object s_Mutex = new object();
        private static string? s_LogCreated = null;
        private static int s_LineCount = 0; // don't worry about rollover

#region Implement to complete class
        static public partial string LogDirectory();
        static public partial string LogFile();
        static private partial string toString(Stage stage);
#endregion

        static public string LogFilePath() { return Path.Combine(LogDirectory(), LogFile()); }

        private static string LogPath() // callers provide lock
        {
          if (s_LogCreated is null) {
              var path = LogFilePath();
              if (File.Exists(path)) File.Delete(path);
              Directory.CreateDirectory(LogDirectory());
              using var text = File.CreateText(path);
              s_LogCreated = path;
          }
          return s_LogCreated;
        }

        public static void WriteLine(Stage stage, string text)
        {
            lock (s_Mutex) {
                string str = string.Format("{0} {1} : {2}", s_LineCount++, toString(stage), text);
                Console.Out.WriteLine(str);
                using var streamWriter = File.AppendText(LogPath());
                streamWriter.WriteLine(str);
                streamWriter.Flush();
            }
        }

        // the caller's own name prefixes the message, so extracting or renaming the caller keeps the log honest
        public static void Trace(Stage stage, string msg, [CallerMemberName] string who = "") => WriteLine(stage, string.IsNullOrEmpty(msg) ? who : who + ": " + msg);
        public static void Trace(string msg, [CallerMemberName] string who = "") => Trace(Stage.RUN_DEBUG, msg, who);

        public static List<string> Lines { get {
            var ret = new List<string>();
            string? str = null;
            using var streamReader = new StreamReader(LogPath());
            while(true) {
                str = streamReader.ReadLine();
                if (str is null) break;
                ret.Add(str);
            }
            return ret;
        } }
    }
}
