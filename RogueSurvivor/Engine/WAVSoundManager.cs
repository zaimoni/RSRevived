// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.NullSoundManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Media;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine
{
  internal class WAVSoundManager : IMusicManager,IDisposable
    {
    private readonly Dictionary<string, SoundPlayer> m_Musics = new Dictionary<string, SoundPlayer>();
    private readonly Dictionary<string, SoundPlayer> m_PlayingMusics = new Dictionary<string, SoundPlayer>();

    SoundPlayer m_CurrentAudio; // alpha10
    bool m_IsPlaying = false;

    public bool IsMusicEnabled { get; set; }
    public int Volume { get; set; }
    // alpha10
    public string Music { get; private set; }
    public int Priority { get; private set; }

    public bool IsPlaying { get { return null != m_CurrentAudio && m_IsPlaying; } }
    public bool HasEnded { get { return null != m_CurrentAudio && !m_IsPlaying; } }

    public WAVSoundManager()
    {
      Volume = 100;
    }

    private static string FullName(string fileName)
    {
      return fileName + ".wav";
    }

    public bool Load(string musicname, string filename)
    {
      filename = FullName(filename);    // LINUX define handled at GameMusics
      Logger.WriteLine(Logger.Stage.INIT_SOUND, string.Format("loading music {0} file {1}", (object) musicname, (object) filename));
      try {
        SoundPlayer tmp = new SoundPlayer(filename);
        tmp.LoadAsync();    // default timeout is 10 seconds
        m_Musics.Add(musicname, tmp);
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.INIT_SOUND, string.Format("failed to load music file {0} exception {1}.", (object) filename, (object) ex.ToString()));
      }
      return true;
    }

    public void Unload(string musicname)
    {
      m_Musics.Remove(musicname);
      m_PlayingMusics.Remove(musicname);
    }

    /// <summary>
    /// Restart playing a music from the beginning if music is enabled.
    /// </summary>
    /// <param name="musicname"></param>
    public void Play(string musicname, int priority)
    {
      if (!IsMusicEnabled) return;
      if (!m_Musics.TryGetValue(musicname, out SoundPlayer music)) return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("playing music {0}.", musicname));
      m_IsPlaying = true;
      Music = musicname;
      Priority = priority;
      (m_CurrentAudio = music).PlaySync(); // XXX really should be in new thread but then we don't get feedback on sound termination
      m_IsPlaying = false;
    }

#if OBSOLETE
    public void Play(string musicname)
    {
      if (!IsMusicEnabled) return;
      if (!m_Musics.TryGetValue(musicname, out SoundPlayer audio)) return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing music {0}.", (object) musicname));
      audio.Play();
      m_PlayingMusics[musicname] = audio;
    }

    public void PlayIfNotAlreadyPlaying(string musicname)
    {
      if (!IsMusicEnabled) return;
      if (m_PlayingMusics.TryGetValue(musicname, out SoundPlayer audio)) return;
      if (!m_Musics.TryGetValue(musicname, out audio)) return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing music {0}.", (object) musicname));
      audio.Play();
    }
#endif

    /// <summary>
    /// Restart playing in a loop a music from the beginning if music is enabled.
    /// </summary>
    /// <param name="musicname"></param>
    public void PlayLooping(string musicname, int priority)
    {
      if (!IsMusicEnabled) return;
      if (!m_Musics.TryGetValue(musicname, out SoundPlayer music)) return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, String.Format("playing looping music {0}.", musicname));
//    music.Ending += new EventHandler(music_Ending);
      m_IsPlaying = true;
      Music = musicname;
      Priority = priority;
      (m_CurrentAudio = music).PlayLooping();
    }

#if OBSOLETE
    public void PlayLooping(string musicname)
    {
      if (!IsMusicEnabled) return;
      if (!m_Musics.TryGetValue(musicname, out SoundPlayer audio)) return;
      Logger.WriteLine(Logger.Stage.RUN_SOUND, string.Format("playing looping music {0}.", (object) musicname));
      audio.PlayLooping();
      m_PlayingMusics[musicname] = audio;
    }

    // no distinct resume
    public void ResumeLooping(string musicname)
    {
      if (!IsMusicEnabled) return;
      if (!m_Musics.TryGetValue(musicname, out SoundPlayer audio)) return;
      audio.PlayLooping();
      m_PlayingMusics[musicname] = audio;
    }
#endif

    public void Stop()
    {
      if (null != m_CurrentAudio) m_CurrentAudio.Stop();
      Music = "";
      Priority = MusicPriority.PRIORITY_NULL;
    }

#if OBSOLETE
    public void Stop(string musicname)
    {
      if (!IsMusicEnabled) return;
      if (!m_Musics.TryGetValue(musicname, out SoundPlayer audio)) return;
      audio.Stop();
      m_PlayingMusics.Remove(musicname);
    }

    public void StopAll()
    {
      Logger.WriteLine(Logger.Stage.RUN_SOUND, "stopping all musics.");
      foreach (SoundPlayer audio in m_Musics.Values) {
        audio.Stop();
      }
      m_PlayingMusics.Clear();
    }

    public bool IsPlaying(string musicname)
    {
      if (!IsMusicEnabled) return false;
      if (!m_PlayingMusics.TryGetValue(musicname, out SoundPlayer audio)) return false;
      return true;
    }

    // not meaningful
    public bool IsPaused(string musicname)
    {
      return false;
    }

    // not meaningful
    public bool HasEnded(string musicname)
    {
      return true;
    }
#endif

    protected void Dispose(bool disposing)
    {
      if (!disposing) return;
      Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing WAVSoundManager...");
      foreach (string key in m_Musics.Keys) {
        SoundPlayer tmp = m_Musics[key];
        if (null == tmp) continue;
        Logger.WriteLine(Logger.Stage.CLEAN_SOUND, string.Format("disposing music {0}.", (object) key));
        tmp.Dispose();
      }
      m_Musics.Clear();
      m_PlayingMusics.Clear();
      Logger.WriteLine(Logger.Stage.CLEAN_SOUND, "disposing WAVSoundManager done.");
    }

    public void Dispose() { Dispose(true); }
  }
}
