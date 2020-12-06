// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeLead
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionTakeLead : ActorAction
  {
    private readonly Actor m_Target;

    public ActionTakeLead(Actor actor, Actor target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanTakeLeadOf(m_Target);
    }

    public override void Perform()
    {
      RogueForm.Game.DoTakeLead(m_Actor, m_Target);
    }

    public override string ToString()
    {
      return m_Actor.Name + " trying to lead " + m_Target.Name + "; " + m_Actor.Location + ", " + m_Target.Location;
    }

  }
}
