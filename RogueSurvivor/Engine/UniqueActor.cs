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
  internal class UniqueActor
  {
    public bool IsSpawned;
    public readonly Actor TheActor;
    public readonly bool IsWithRefugees;
    public readonly string EventThemeMusic;
    public readonly string EventMessage;

	public UniqueActor(Actor a, bool spawn_now, bool refugee=false, string music=null, string msg=null)
	{
	  IsSpawned = spawn_now;
	  TheActor = a;
	  IsWithRefugees = refugee;
	  EventThemeMusic = music;
	  EventMessage = msg;
	}
  }
}
