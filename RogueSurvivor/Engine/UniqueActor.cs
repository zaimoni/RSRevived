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
    public bool IsSpawned { get; set; }

    public Actor TheActor { get; set; }

    public bool IsWithRefugees { get; set; }

    public string EventThemeMusic { get; set; }

    public string EventMessage { get; set; }
  }
}
