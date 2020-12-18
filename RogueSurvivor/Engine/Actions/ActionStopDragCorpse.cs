// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionStopDragCorpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionStopDragCorpse : ActorAction
  {
    private readonly Corpse m_Target;

    public ActionStopDragCorpse(Actor actor, Corpse target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
    }

    public override bool IsLegal()
    {
      bool ret = 2 == m_Actor.CanStartStopDrag(m_Target, out m_FailReason); // actually IsPerformable implementation
      if (!ret && string.IsNullOrEmpty(m_FailReason)) m_FailReason = "not dragging a corpse";
      return ret;
    }

    public override void Perform()
    {
      RogueGame.Game.DoStopDragCorpse(m_Actor);
    }
  }
}
