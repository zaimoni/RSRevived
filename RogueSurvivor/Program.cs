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
    [STAThread]
    private static void Main()
    {
      Logger.CreateFile();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "starting program...");
      Logger.WriteLine(Logger.Stage.INIT_MAIN, string.Format("date : {0}.", (object) DateTime.Now.ToString()));
      Logger.WriteLine(Logger.Stage.INIT_MAIN, string.Format("game version : {0}.", (object) "alpha 9"));
      Application.CurrentCulture = CultureInfo.InvariantCulture;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "loading setup...");
      SetupConfig.Load();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup : " + SetupConfig.toString(SetupConfig.Video) + ", " + SetupConfig.toString(SetupConfig.Sound));
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
      Logger.WriteLine(Logger.Stage.CLEAN_MAIN, "exiting program...");
    }
  }
}
