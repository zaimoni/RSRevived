﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Logger
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.IO;

namespace djack.RogueSurvivor
{
  internal static class Logger
  {
    readonly private static List<string> s_Lines = new List<string>();
    readonly private static object s_Mutex = new object();

    public static IEnumerable<string> Lines { get { return Logger.s_Lines; } }

#if DEAD_FUNC
    public static void Clear()
    {
      lock (Logger.s_Mutex) { Logger.s_Lines.Clear(); }
    }
#endif

    public static void CreateFile()
    {
      lock (s_Mutex) {
        var path = LogFilePath();
        if (File.Exists(path)) File.Delete(path);
        Directory.CreateDirectory(SetupConfig.DirPath);
        using var text = File.CreateText(path);
      }
    }

    public static void WriteLine(Logger.Stage stage, string text)
    {
      lock (s_Mutex) {
        string str = string.Format("{0} {1} : {2}", s_Lines.Count, StageToString(stage), text);
        s_Lines.Add(str);
        Console.Out.WriteLine(str);
        using var streamWriter = File.AppendText(Logger.LogFilePath());
        streamWriter.WriteLine(str);
        streamWriter.Flush();
      }
    }

    private static string LogFilePath()
    {
      return SetupConfig.DirPath + "\\log.txt";
    }

    private static string StageToString(Logger.Stage s)
    {
      switch (s)
      {
        case Logger.Stage.INIT_MAIN:
          return "init main";
        case Logger.Stage.RUN_MAIN:
          return "run main";
        case Logger.Stage.CLEAN_MAIN:
          return "clean main";
        case Logger.Stage.INIT_GFX:
          return "init gfx";
        case Logger.Stage.RUN_GFX:
          return "run gfx";
        case Logger.Stage.CLEAN_GFX:
          return "clean gfx";
        case Logger.Stage.INIT_SOUND:
          return "init sound";
        case Logger.Stage.RUN_SOUND:
          return "run sound";
        case Logger.Stage.CLEAN_SOUND:
          return "clean sound";
        default:
          return "misc";
      }
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
    }
  }
}
