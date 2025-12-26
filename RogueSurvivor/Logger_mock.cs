using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

#nullable enable

namespace Zaimoni.Data
{
    // Example of how to complete Zaimoni.Data.Logger
    static public partial class Logger
    {
        static public partial string LogDirectory()
        {
            return Path.Combine(Environment.CurrentDirectory, "Config");
        }

        static public partial string LogFile()
        {
            return "log.txt";
        }

        public enum Stage
        {
            INIT_MAIN,
            RUN_MAIN,
            CLEAN_MAIN,
            INIT_GFX,
            RUN_GFX,
            CLEAN_GFX,
            INIT_SOUND,
            RUN_SOUND,
            CLEAN_SOUND,
            RUN_DEBUG,
        }

        static private partial string toString(Stage s)
        {
            switch (s)
            {
            case Stage.INIT_MAIN: return "init main";
            case Stage.RUN_MAIN: return "run main";
            case Stage.CLEAN_MAIN: return "clean main";
            case Stage.INIT_GFX: return "init gfx";
            case Stage.RUN_GFX: return "run gfx";
            case Stage.CLEAN_GFX: return "clean gfx";
            case Stage.INIT_SOUND: return "init sound";
            case Stage.RUN_SOUND: return "run sound";
            case Stage.CLEAN_SOUND: return "clean sound";
            case Stage.RUN_DEBUG: return "run debug";
            default: return "misc";
            }
        }
    }
}
