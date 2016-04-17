// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.NullSoundManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  internal class NullSoundManager : ISoundManager, IDisposable
  {
    public bool IsMusicEnabled { get; set; }

    public int Volume { get; set; }

    public bool Load(string musicname, string filename)
    {
      return true;
    }

    public void Unload(string musicname)
    {
    }

    public void Play(string musicname)
    {
    }

    public void PlayIfNotAlreadyPlaying(string musicname)
    {
    }

    public void PlayLooping(string musicname)
    {
    }

    public void ResumeLooping(string musicname)
    {
    }

    public void Stop(string musicname)
    {
    }

    public void StopAll()
    {
    }

    public bool IsPlaying(string musicname)
    {
      return false;
    }

    public bool IsPaused(string musicname)
    {
      return false;
    }

    public bool HasEnded(string musicname)
    {
      return true;
    }

    public void Dispose()
    {
    }
  }
}
