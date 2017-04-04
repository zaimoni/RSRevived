// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.GameMode
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal enum GameMode
  {
    GM_STANDARD,
    GM_CORPSES_INFECTION,
    GM_VINTAGE,
  }

  internal enum GameMode_Bounds
  {
    _COUNT = (int)GameMode.GM_VINTAGE+1
  }
}
