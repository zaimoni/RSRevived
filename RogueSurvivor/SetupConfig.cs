// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.SetupConfig
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.IO;

namespace djack.RogueSurvivor
{
  public static class SetupConfig
  {
    public const string GAME_NAME = "Rogue Survivor";
    public const string GAME_NAME_CAPS = "ROGUE SURVIVOR";
    public const string GAME_VERSION = "alpha 9 fork RC 5";

    public static SetupConfig.eVideo Video { get; set; }

    public static SetupConfig.eSound Sound { get; set; }

    public static string DirPath
    {
      get
      {
        return Environment.CurrentDirectory + "\\Config\\";
      }
    }

    private static string FilePath
    {
      get
      {
        return SetupConfig.DirPath + "\\setup.dat";
      }
    }

    public static void Save()
    {
      using (StreamWriter text = File.CreateText(SetupConfig.FilePath))
      {
        text.WriteLine(SetupConfig.toString(SetupConfig.Video));
        text.WriteLine(SetupConfig.toString(SetupConfig.Sound));
      }
    }

    public static void Load()
    {
      if (File.Exists(SetupConfig.FilePath))
      {
        using (StreamReader streamReader = File.OpenText(SetupConfig.FilePath))
        {
          SetupConfig.Video = SetupConfig.toVideo(streamReader.ReadLine());
          SetupConfig.Sound = SetupConfig.toSound(streamReader.ReadLine());
        }
      }
      else
      {
        if (!Directory.Exists(SetupConfig.DirPath))
          Directory.CreateDirectory(SetupConfig.DirPath);
        SetupConfig.Video = SetupConfig.eVideo.VIDEO_GDI_PLUS;
        SetupConfig.Sound = SetupConfig.eSound.SOUND_NOSOUND;
        SetupConfig.Save();
      }
    }

    public static string toString(SetupConfig.eVideo v)
    {
      return v.ToString();
    }

    public static string toString(SetupConfig.eSound s)
    {
      return s.ToString();
    }

    public static SetupConfig.eVideo toVideo(string s)
    {
//      if (s == SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX.ToString())
//        return SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX;
      return s == SetupConfig.eVideo.VIDEO_GDI_PLUS.ToString() ? SetupConfig.eVideo.VIDEO_GDI_PLUS : SetupConfig.eVideo.VIDEO_INVALID;
    }

    public static SetupConfig.eSound toSound(string s)
    {
//      if (s == SetupConfig.eSound.SOUND_MANAGED_DIRECTX.ToString())
//        return SetupConfig.eSound.SOUND_MANAGED_DIRECTX;
//      if (s == SetupConfig.eSound.SOUND_SFML.ToString())
//        return SetupConfig.eSound.SOUND_SFML;
      return s == SetupConfig.eSound.SOUND_NOSOUND.ToString() ? SetupConfig.eSound.SOUND_NOSOUND : SetupConfig.eSound.SOUND_INVALID;
    }

    public enum eVideo
    {
      VIDEO_INVALID,
      VIDEO_MANAGED_DIRECTX_XXX, // highest version of DirectX supported in .NET is DirectX 9
      VIDEO_GDI_PLUS,
      _COUNT,
    }

    public enum eSound
    {
      SOUND_INVALID,
      SOUND_MANAGED_DIRECTX_XXX, // highest version of DirectX supported in .NET is DirectX 9
      SOUND_SFML_XXX,    // breaking this as the DLLs aren't compatible with the MSIL architecture
      SOUND_NOSOUND,
      _COUNT,
    }
  }
}
