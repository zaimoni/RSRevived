// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionChat
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionChat : ActorAction
  {
    private readonly Actor m_Target;

    public Actor Target
    {
      get
      {
        return this.m_Target;
      }
    }

    public ActionChat(Actor actor, RogueGame game, Actor target)
      : base(actor, game)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      this.m_Target = target;
    }

    public override bool IsLegal()
    {
      return true;
    }

    public override void Perform()
    {
      this.m_Game.DoChat(this.m_Actor, this.m_Target);
    }
  }
}
