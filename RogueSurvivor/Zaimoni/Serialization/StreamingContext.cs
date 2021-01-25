using System;

#nullable enable

namespace Zaimoni.Serialization
{
    /// <summary>
    /// Part of cloning the System.Runtime.Serialization specification, which is deprecated and cannot be assumed to be available indefinitely.
    /// </summary>
    public readonly struct StreamingContext
    {
        public readonly States State;
        public readonly object? Context;

        StreamingContext(States st, object? xtra = null) {
            State = st;
            Context = xtra;
        }

        // comment out the ones we don't handle
        [Flags]
        public enum States : byte {
//          CrossProcess = 1, // different process on same computer
//          CrossMachine = 2, // different computer
            File = 4, // files can last longer than the process, and do not require data from the process to deserialize
//          Persistence = 8, // some sort of persisted store; can last longer than the process, and do not require data from the process to deserialize
//          Remoting = 16, // context in an unknown location; may or may not be on same computer
//          Other = 32, // explicitly unknown (not foreseen)
//          Clone = 64, // in-memory deep clone within same process; handles/references to unmanaged resources should remain valid
//          CrossAppDomain = 128, // different Application Domain
            All = 255 // any defined source/destination context; morally unknown
        }
    }
}
