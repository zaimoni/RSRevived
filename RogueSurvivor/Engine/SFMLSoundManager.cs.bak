// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.SFMLSoundManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using SFML.Audio;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine
{
  internal class SFMLSoundManager : ISoundManager, IDisposable
  {
    private bool m_IsMusicEnabled;
    private int m_Volume;
    private Dictionary<string, Music> m_Musics;

    public bool IsMusicEnabled
    {
      get
      {
        return this.m_IsMusicEnabled;
      }
      set
      {
        this.m_IsMusicEnabled = value;
      }
    }

    public int Volume
    {
      get
      {
        return this.m_Volume;
      }
      set
      {
        this.m_Volume = value;
        this.OnVolumeChange();
      }
    }

    public SFMLSoundManager()
    {
      this.m_Musics = new Dictionary<string, Music>();
      this.m_Volume = 100;
    }

    private string FullName(string fileName)
    {
      return fileName + ".ogg";
    }

    public bool Load(string musicname, string filename)
    {
      filename = this.FullName(filename);
      Logger.WriteLine(Logger.Stage.INIT_SOUND, string.Format("loading music {0} file {1}", (object) musicname, (object) filename));
      try
      {
        Music music = new Music(filename);
        this.m_Musics.Add(musicname, music);
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.INIT_SOUND, string.Format("failed to load music file {0} exception {1}.", (object) filename, (object) ex.ToString()));
      }
      return true;
    }

    public void Unload(string musicname)
    {
      this.m_Musics.Remove(musicname);
    }

    private void OnVolumeChange()
    {
      foreach (Music music in this.m_Musics.Values)
        music.Volume = (float) this.m_Volume;
    }

    public void Play(string musicname)
    {
      Music audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing music {0}.", (object) musicname));
      this.Play(audio);
    }

    public void PlayIfNotAlreadyPlaying(string musicname)
    {
      Music audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio) || this.IsPlaying(audio))
        return;
      this.Play(audio);
    }

    public void PlayLooping(string musicname)
    {
      Music audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing looping music {0}.", (object) musicname));
      audio.Loop = true;
      this.Play(audio);
    }

    public void ResumeLooping(string musicname)
    {
      Music audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("resuming looping music {0}.", (object) musicname));
      this.Resume(audio);
    }

    public void Stop(string musicname)
    {
      Music audio;
      if (!this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("stopping music {0}.", (object) musicname));
      this.Stop(audio);
    }

    public void StopAll()
    {
      Logger.WriteLine(Logger.Stage.RUN_SOUND, "stopping all musics.");
      foreach (Music audio in this.m_Musics.Values)
        this.Stop(audio);
    }

    public bool IsPlaying(string musicname)
    {
      Music audio;
      if (this.m_Musics.TryGetValue(musicname, out audio))
        return this.IsPlaying(audio);
      return false;
    }

    public bool IsPaused(string musicname)
    {
      Music audio;
      if (this.m_Musics.TryGetValue(musicname, out audio))
        return this.IsPaused(audio);
      return false;
    }

    public bool HasEnded(string musicname)
    {
      Music audio;
      if (this.m_Musics.TryGetValue(musicname, out audio))
        return this.HasEnded(audio);
      return false;
    }

    private void Stop(Music audio)
    {
      audio.Stop();
    }

    private void Play(Music audio)
    {
      audio.Stop();
      audio.Volume = (float) this.m_Volume;
      audio.Play();
    }

    private void Resume(Music audio)
    {
      audio.Play();
    }

    private bool IsPlaying(Music audio)
    {
      return audio.Status == SoundStatus.Playing;
    }

    private bool IsPaused(Music audio)
    {
      return audio.Status == SoundStatus.Paused;
    }

    private bool HasEnded(Music audio)
    {
      if (audio.Status != SoundStatus.Stopped)
        return (double) audio.PlayingOffset >= (double) audio.Duration;
      return true;
    }

    public void Dispose()
    {
      Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing SFMLMusicManager...");
      foreach (string key in this.m_Musics.Keys)
      {
        Music music = this.m_Musics[key];
        if (music == null)
        {
          Logger.WriteLine(Logger.Stage.CLEAN_SOUND, string.Format("WARNING: null music for key {0}", (object) key));
        }
        else
        {
          Logger.WriteLine(Logger.Stage.CLEAN_SOUND, string.Format("disposing music {0}.", (object) key));
          music.Dispose();
        }
      }
      this.m_Musics.Clear();
      Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing SFMLMusicManager done.");
    }
  }
}
