// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueActor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  public class UniqueActor
  {
    private bool m_Spawned;
    public readonly Actor TheActor;
    public readonly bool IsWithRefugees;
    public readonly string EventThemeMusic;
    public readonly string EventMessage;

    public bool IsSpawned {
      get { return m_Spawned;  }
      set { if (!m_Spawned) m_Spawned = value; } // \todo? eliminate this outright
    }

    public UniqueActor(Actor a, bool spawn_now, bool refugee=false, string music=null, string msg=null)
	{
	  m_Spawned = spawn_now;
	  TheActor = a;
	  IsWithRefugees = refugee;
	  EventThemeMusic = music;
	  EventMessage = msg;
	}
  }
}
