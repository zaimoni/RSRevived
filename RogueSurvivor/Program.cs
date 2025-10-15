// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Program
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Globalization;
using System.Windows.Forms;
using Zaimoni.Data;

namespace djack.RogueSurvivor
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      if (null!=args) {
        // help option is impractical: C stdout is inaccessible in a GUI C# program
        bool reading_PC = false;
        foreach (string tmp in args) {
          if (reading_PC) {
            if (Engine.Session.CommandLineOptions.ContainsKey("PC")) {
              Engine.Session.CommandLineOptions["PC"] += "\0"+tmp;
            } else {
              Engine.Session.CommandLineOptions["PC"] = tmp;
            }
            reading_PC = false;
          }
          if (tmp.StartsWith("--seed=") && 0 == Engine.Session.COMMAND_LINE_SEED) {
            if (int.TryParse(tmp.Substring(7), out int tmp2)) Engine.Session.COMMAND_LINE_SEED = tmp2;
          }
          if (tmp.StartsWith("--spawn=") && !Engine.Session.CommandLineOptions.ContainsKey("spawn")) {
            Engine.Session.CommandLineOptions["spawn"] = tmp.Substring(8);
          }
          if (tmp.StartsWith("--spawn-district=") && !Engine.Session.CommandLineOptions.ContainsKey("spawn-district")) {
            Engine.Session.CommandLineOptions["spawn-district"] = tmp.Substring(17);
          }
          if (tmp.StartsWith("--city=") && !Engine.Session.CommandLineOptions.ContainsKey("city")) {
            Engine.Session.CommandLineOptions["city"] = tmp.Substring(7);
          }
          if ("--subway-cop"==tmp) Engine.Session.CommandLineOptions["subway-cop"] = "";    // key just has to exist
          if ("--socrates-daimon"==tmp) Engine.Session.CommandLineOptions["socrates-daimon"] = "";    // key just has to exist
          if ("--faust"==tmp) Engine.Session.CommandLineOptions["faust"] = "";    // key just has to exist
          if ("--no-spawn"==tmp) Engine.Session.CommandLineOptions["no-spawn"] = "";    // key just has to exist
          if ("--PC"==tmp) reading_PC=true;
          // XXX more command-line options
          // --spawn : choice sequence for the new game dialog set; do not allow random at any stage.
          // --spawn-district : override district and optionally position.
          //    The center district (historically C2, now D3) is default.
          //    district-only override would be e.g. C1.
          //    C1@5,6 would override both district and position
          // --no-spawn : do not create a random PC.  requires a --PC option to be viable.  Incompatible with --spawn and --spawn-district
          // --city : specify city size and dimensions; default is 5,50
        }
        if (Engine.Session.CommandLineOptions.ContainsKey("no-spawn")) {
          if (   !Engine.Session.CommandLineOptions.ContainsKey("PC")
//            ||  Engine.Session.CommandLineOptions.ContainsKey("spawn")    // turns out we do need the first character of this regardless
              ||  Engine.Session.CommandLineOptions.ContainsKey("spawn-district"))
            Engine.Session.CommandLineOptions.Remove("no-spawn");
        }
      }

      Logger.WriteLine(Logger.Stage.INIT_MAIN, "starting program...");
      Logger.WriteLine(Logger.Stage.INIT_MAIN, string.Format("date : {0}.", (object) DateTime.Now.ToString()));
      Logger.WriteLine(Logger.Stage.INIT_MAIN, string.Format("game version : {0}.", (object) SetupConfig.GAME_VERSION));
      Application.CurrentCulture = CultureInfo.InvariantCulture;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "loading setup...");
      SetupConfig.Load();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup : " + SetupConfig.toString(SetupConfig.Video) + ", " + SetupConfig.toString(SetupConfig.Sound));
#if LEGACY
      using (RogueForm rogueForm = new()) {
        Application.Run(rogueForm);
      }
#else
      using(RogueForm rogueForm = new()) {
        try {
          Application.Run(rogueForm);
        } catch (Exception ex) {
          using (Bugreport bugreport = new(ex)) {
            int num = (int) bugreport.ShowDialog();
          }
          Application.Exit();
        }
      }
#endif
      Logger.WriteLine(Logger.Stage.CLEAN_MAIN, "exiting program...");
    }
  }
}
