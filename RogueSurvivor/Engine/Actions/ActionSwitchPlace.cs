// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSwitchPlace
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionSwitchPlace : ActorAction
  {
    private readonly Actor m_Target;

    public ActionSwitchPlace(Actor actor, Actor target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanSwitchPlaceWith(m_Target,out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoSwitchPlace(m_Actor, m_Target);
    }
  }

  internal class ActionSwitchPlaceEmergency : ActorAction
  {
    private readonly Actor m_Target;

    public ActionSwitchPlaceEmergency(Actor actor, Actor target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
    }

    public override bool IsLegal()
    {
      return Rules.IsAdjacent(m_Actor.Location,m_Target.Location);  // XXX doesn't set FailReason
    }

    public override void Perform()
    {
      RogueForm.Game.DoSwitchPlace(m_Actor, m_Target);
    }
  }
}
