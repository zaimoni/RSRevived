// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionStopDragCorpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionStopDragCorpse : ActorAction
  {
    private readonly Corpse m_Target;

    public ActionStopDragCorpse(Actor actor, RogueGame game, Corpse target)
      : base(actor, game)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      this.m_Target = target;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorStopDragCorpse(this.m_Actor, this.m_Target, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoStopDragCorpse(this.m_Actor, this.m_Target);
    }
  }
}
