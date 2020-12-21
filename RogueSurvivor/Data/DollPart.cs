// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.DollPart
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal enum DollPart
  {
    NONE = 0,
    RIGHT_HAND = 1,
    _FIRST = RIGHT_HAND,     // actually want this since NONE is not a valid decoration index
    LEFT_HAND = 2,
    HEAD = 3,
    TORSO = 4,
    LEGS = 5,
    FEET = 6,
    SKIN = 7,
    EYES = 8,
    _COUNT = 9,     // last decoration index
    HIP_HOLSTER = _COUNT,
  }
}
