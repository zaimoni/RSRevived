// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionStartDragCorpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionStartDragCorpse : ActorAction
  {
    private readonly Corpse m_Target;

    public ActionStartDragCorpse(Actor actor, Corpse target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
    }

    public override bool IsLegal()
    {
      bool ret = 1 == m_Actor.CanStartStopDrag(m_Target, out m_FailReason); // actually IsPerformable implementation
      if (!ret && string.IsNullOrEmpty(m_FailReason)) m_FailReason = "already dragging corpse";
      return ret;
    }

    public override void Perform()
    {
      RogueGame.Game.DoStartDragCorpse(m_Actor, m_Target);
    }
  }
}
