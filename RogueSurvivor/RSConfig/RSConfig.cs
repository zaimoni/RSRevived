// Decompiled with JetBrains decompiler
// Type: Setup.Program
// Assembly: RSConfig, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A6245E6-7D9A-4424-BC16-B17B9A5036B9
// Assembly location: C:\Private.app\RS9Alpha.Hg\RSConfig.exe

using System;
using System.Windows.Forms;

namespace Setup
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new ConfigForm());
    }
  }
}
