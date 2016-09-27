// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Program
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Globalization;
using System.Windows.Forms;

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
        foreach(string tmp in args) { 
          if (tmp.StartsWith("--seed=") && 0 == Engine.Session.COMMAND_LINE_SEED) { 
            int tmp2;
            if (int.TryParse(tmp.Substring(7), out tmp2)) Engine.Session.COMMAND_LINE_SEED = tmp2;
          }
          if ("--subway-cop"==tmp) Engine.Session.CommandLineOptions["subway-cop"] = "";    // key just has to exist
          if ("--socrates-daimon"==tmp) Engine.Session.CommandLineOptions["socrates-daimon"] = "";    // key just has to exist
        }
      }

      Logger.CreateFile();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "starting program...");
      Logger.WriteLine(Logger.Stage.INIT_MAIN, string.Format("date : {0}.", (object) DateTime.Now.ToString()));
      Logger.WriteLine(Logger.Stage.INIT_MAIN, string.Format("game version : {0}.", (object) SetupConfig.GAME_VERSION));
      Application.CurrentCulture = CultureInfo.InvariantCulture;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "loading setup...");
      SetupConfig.Load();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup : " + SetupConfig.toString(SetupConfig.Video) + ", " + SetupConfig.toString(SetupConfig.Sound));
#if DEBUG
      using (RogueForm rogueForm = new RogueForm())
      {
        Application.Run((Form)rogueForm);
      }
#else
      using (RogueForm rogueForm = new RogueForm())
      {
        try
        {
          Application.Run((Form) rogueForm);
        }
        catch (Exception ex)
        {
          using (Bugreport bugreport = new Bugreport(ex))
          {
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
