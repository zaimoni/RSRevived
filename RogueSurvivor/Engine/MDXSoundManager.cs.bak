// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MDXSoundManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine
{
  internal class MDXSoundManager : ISoundManager, IDisposable
  {
    private bool m_IsMusicEnabled;
    private int m_Volume;
    private int m_Attenuation;
    private Dictionary<string, Audio> m_Musics;

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

    public MDXSoundManager()
    {
      this.m_Musics = new Dictionary<string, Audio>();
      this.Volume = 100;
    }

    private string FullName(string fileName)
    {
      return fileName + ".mp3";
    }

    public bool Load(string musicname, string filename)
    {
      filename = this.FullName(filename);
      Logger.WriteLine(Logger.Stage.INIT_SOUND, string.Format("loading music {0} file {1}", (object) musicname, (object) filename));
      try
      {
        Audio audio = new Audio(filename);
        this.m_Musics.Add(musicname, audio);
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
      this.m_Attenuation = this.ComputeDXAttenuationFromVolume();
      foreach (Audio audio in this.m_Musics.Values)
      {
        try
        {
          audio.Volume = -this.m_Attenuation;
        }
        catch (DirectXException ex)
        {
        }
      }
    }

    private int ComputeDXAttenuationFromVolume()
    {
      if (this.m_Volume <= 0)
        return 10000;
      return (100 - this.m_Volume) * 2500 / 100;
    }

    public void Play(string musicname)
    {
      Audio audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing music {0}.", (object) musicname));
      this.Play(audio);
    }

    public void PlayIfNotAlreadyPlaying(string musicname)
    {
      Audio audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio) || this.IsPlaying(audio))
        return;
      this.Play(audio);
    }

    public void PlayLooping(string musicname)
    {
      Audio audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing looping music {0}.", (object) musicname));
      audio.Ending += new EventHandler(this.music_Ending);
      this.Play(audio);
    }

    public void ResumeLooping(string musicname)
    {
      Audio audio;
      if (!this.m_IsMusicEnabled || !this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("resuming looping music {0}.", (object) musicname));
      this.Resume(audio);
    }

    private void music_Ending(object sender, EventArgs e)
    {
      this.Play((Audio) sender);
    }

    public void Stop(string musicname)
    {
      Audio audio;
      if (!this.m_Musics.TryGetValue(musicname, out audio))
        return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("stopping music {0}.", (object) musicname));
      this.Stop(audio);
    }

    public void StopAll()
    {
      Logger.WriteLine(Logger.Stage.RUN_SOUND, "stopping all musics.");
      foreach (Audio audio in this.m_Musics.Values)
        this.Stop(audio);
    }

    public bool IsPlaying(string musicname)
    {
      Audio audio;
      if (this.m_Musics.TryGetValue(musicname, out audio))
        return this.IsPlaying(audio);
      return false;
    }

    public bool IsPaused(string musicname)
    {
      Audio audio;
      if (this.m_Musics.TryGetValue(musicname, out audio))
        return this.IsPaused(audio);
      return false;
    }

    public bool HasEnded(string musicname)
    {
      Audio audio;
      if (this.m_Musics.TryGetValue(musicname, out audio))
        return this.HasEnded(audio);
      return false;
    }

    private void Stop(Audio audio)
    {
      audio.Ending -= new EventHandler(this.music_Ending);
      audio.Pause();
    }

    private void Play(Audio audio)
    {
      audio.Stop();
      audio.SeekCurrentPosition(0.0, SeekPositionFlags.AbsolutePositioning);
      audio.Volume = -this.m_Attenuation;
      audio.Play();
    }

    private void Resume(Audio audio)
    {
      audio.Play();
    }

    private bool IsPlaying(Audio audio)
    {
      if (audio.CurrentPosition > 0.0 && audio.CurrentPosition < audio.Duration)
        return audio.State == StateFlags.Running;
      return false;
    }

    private bool IsPaused(Audio audio)
    {
      return (audio.State & StateFlags.Paused) != StateFlags.Stopped;
    }

    private bool HasEnded(Audio audio)
    {
      return audio.CurrentPosition >= audio.Duration;
    }

    public void Dispose()
    {
      Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing MDXMusicManager...");
      foreach (string key in this.m_Musics.Keys)
      {
        Audio audio = this.m_Musics[key];
        if (audio == null)
        {
          Logger.WriteLine(Logger.Stage.CLEAN_SOUND, string.Format("WARNING: null music for key {0}", (object) key));
        }
        else
        {
          Logger.WriteLine(Logger.Stage.CLEAN_SOUND, string.Format("disposing music {0}.", (object) key));
          audio.Dispose();
        }
      }
      this.m_Musics.Clear();
      Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing MDXMusicManager done.");
    }
  }
}
