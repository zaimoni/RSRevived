// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorDirective
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  // This is set *only* by the player; a total replacement with the objectives system is anticipated.
  [Serializable]
  public class ActorDirective
  {
    public const bool CanThrowGrenades_default = true;
    public const bool CanSleep_default = true;
    public const bool CanTrade_default = true;
    public const ActorCourage Courage_default = ActorCourage.CAUTIOUS;

    public bool CanThrowGrenades = CanThrowGrenades_default;
    public bool CanSleep = CanSleep_default;    // may want this indefinitely (useful to disable when appointing guards)
    public bool CanTrade = CanTrade_default;
    public ActorCourage Courage = Courage_default;
  }
}
