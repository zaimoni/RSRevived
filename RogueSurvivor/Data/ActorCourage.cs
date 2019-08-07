// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorCourage
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal enum ActorCourage
  {
    COWARD,
    CAUTIOUS,
    COURAGEOUS,
  }

  internal static class ActorCourage_ext
  {
    internal static string to_s(this ActorCourage c)
    {
      switch (c) {
        case ActorCourage.COWARD: return "Coward";
        case ActorCourage.CAUTIOUS: return "Cautious";
        case ActorCourage.COURAGEOUS: return "Courageous";
        default: throw new ArgumentOutOfRangeException("unhandled courage");
      }
    }
  }
}
