// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorController
  {
    protected Actor m_Actor;

    public virtual void TakeControl(Actor actor)
    {
      this.m_Actor = actor;
    }

    public virtual void LeaveControl()
    {
      this.m_Actor = (Actor) null;
    }

    public abstract ActorAction GetAction(RogueGame game);
  }
}
