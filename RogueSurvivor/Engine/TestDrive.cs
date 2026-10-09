using System;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine
{
  /// <summary>Command line options that drive the program, rather than the game: how to start, and whether to stop as soon as a
  /// player would be asked to act.  Unlike Session.CommandLineOptions these describe a test run, not the game being played, so
  /// they are never saved.</summary>
  internal static class TestDrive
  {
    public enum StartAs : byte
    {
      MAIN_MENU = 0, // default: ask
      NEW_GAME,      // the new game dialogs, which --spawn etc. fill in
      LOAD_GAME,     // the default save
      LOAD_TEST      // the test-format save (Session.TestFormat; nothing to load when there is none)
    }

    static public StartAs Start = StartAs.MAIN_MENU;

    /// <summary>Save to the default slot, then quit, at the first point a player would be asked to act.</summary>
    static public bool SaveAndExit = false;

    /// <summary>true if and only if no one is watching: popups that wait for a key should not.  An input script (--input) is someone, until it runs out.</summary>
    static public bool Unattended { get => SaveAndExit && InputScript.IsEmpty; }

    /// <returns>true if and only if arg was one of ours</returns>
    static public bool Parse(string arg)
    {
      switch (arg) {
      case "--start=new": Start = StartAs.NEW_GAME; return true;
      case "--start=load": Start = StartAs.LOAD_GAME; return true;
      case "--start=load-test": Start = StartAs.LOAD_TEST; return true;
      case "--save-and-exit": SaveAndExit = true; return true;
      default:
        try {
          return InputScript.Parse(arg);
        } catch (Exception ex) {   // a token that is not input, or a response file that cannot be read
          Failed(ex.Message);   // the program still runs, so that whoever is attending can read the log
          return true;
        }
      }
    }

    /// <summary>What the program should exit with.  Non-zero if and only if a test run failed.</summary>
    static public int ExitCode = 0;

    static public void Failed(string why)
    {
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "test drive failed: " + why);
      ExitCode = 1;
    }
  }
}
