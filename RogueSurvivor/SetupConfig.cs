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
    public const string GAME_NAME = "Rogue Survivor Revived";
    public const string GAME_NAME_CAPS = "ROGUE SURVIVOR REVIVED";
    public const string GAME_VERSION = "0.10.0 unstable (2021-03-25)";

    public static eVideo Video { get; set; }
    public static eSound Sound { get; set; }

    public static string DirPath { get { return Path.Combine(Environment.CurrentDirectory, "Config") + Path.DirectorySeparatorChar; } }
    private static string FilePath { get { return Path.Combine(DirPath, "setup.dat"); } }

    public static void Save()
    {
      using var text = File.CreateText(FilePath);
      text.WriteLine(toString(Video));
      text.WriteLine(toString(Sound));
    }

    public static void Load()
    {
      var path = FilePath;
      if (File.Exists(path)) {
        using var streamReader = File.OpenText(path);
        Video = toVideo(streamReader.ReadLine());
        Sound = toSound(streamReader.ReadLine());
      } else {
        path = DirPath;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        Video = eVideo.VIDEO_GDI_PLUS;
        Sound = eSound.SOUND_NOSOUND;
        Save();
      }
    }

    public static string toString(eVideo v) { return v.ToString(); }
    public static string toString(eSound s) { return s.ToString(); }

    public static eVideo toVideo(string s)
    {
//      if (s == SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX.ToString())
//        return SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX;
      return s == eVideo.VIDEO_GDI_PLUS.ToString() ? eVideo.VIDEO_GDI_PLUS : eVideo.VIDEO_INVALID;
    }

    public static SetupConfig.eSound toSound(string s)
    {
//      if (s == SetupConfig.eSound.SOUND_MANAGED_DIRECTX.ToString())
//        return SetupConfig.eSound.SOUND_MANAGED_DIRECTX;
      if (s == eSound.SOUND_WAV.ToString()) return eSound.SOUND_WAV;
      return s == eSound.SOUND_NOSOUND.ToString() ? eSound.SOUND_NOSOUND : eSound.SOUND_INVALID;
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
      SOUND_WAV,    // formerly SOUND_SFML
      SOUND_NOSOUND,
      _COUNT,
    }
  }
}
