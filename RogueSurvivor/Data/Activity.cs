// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Activity
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal enum Activity
  {
    IDLE,
    CHASING,
    FIGHTING,
    TRACKING,
    FLEEING,
    FOLLOWING,
    SLEEPING,
    FOLLOWING_ORDER,
    FLEEING_FROM_EXPLOSIVE,
  }
}
