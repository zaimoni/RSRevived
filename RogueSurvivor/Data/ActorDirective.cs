// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorDirective
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class ActorDirective
  {
    public bool CanTakeItems { get; set; }

    public bool CanFireWeapons { get; set; }

    public bool CanThrowGrenades { get; set; }

    public bool CanSleep { get; set; }

    public bool CanTrade { get; set; }

    public ActorCourage Courage { get; set; }

    public ActorDirective()
    {
            Reset();
    }

    public void Reset()
    {
            CanTakeItems = true;
            CanFireWeapons = true;
            CanThrowGrenades = true;
            CanSleep = true;
            CanTrade = true;
            Courage = ActorCourage.CAUTIOUS;
    }

    public static string CourageString(ActorCourage c)
    {
      switch (c)
      {
        case ActorCourage.COWARD:
          return "Coward";
        case ActorCourage.CAUTIOUS:
          return "Cautious";
        case ActorCourage.COURAGEOUS:
          return "Courageous";
        default:
          throw new ArgumentOutOfRangeException("unhandled courage");
      }
    }
  }
}
