// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSleep
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionSleep : ActorAction
  {
    public ActionSleep(Actor actor) : base(actor) {}
    public override bool IsLegal() => m_Actor.CanSleep(out m_FailReason);
    public override void Perform() => m_Actor.StartSleeping();
  }
}
