// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.ISoundManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  internal interface ISoundManager : IDisposable
  {
    bool IsMusicEnabled { get; set; }

    int Volume { get; set; }

    bool Load(string musicname, string filename);

    void Unload(string musicname);

    void Play(string musicname);

    void PlayIfNotAlreadyPlaying(string musicname);

    void PlayLooping(string musicname);

    void ResumeLooping(string musicname);

    void Stop(string musicname);

    void StopAll();

    bool IsPlaying(string musicname);

    bool IsPaused(string musicname);

    bool HasEnded(string musicname);
  }
}
