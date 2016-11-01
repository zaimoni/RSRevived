// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSwitchPlace
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionSwitchPlace : ActorAction
  {
    private readonly Actor m_Target;

    public ActionSwitchPlace(Actor actor, Actor target)
      : base(actor)
    {
      Contract.Requires(null != target);
      m_Target = target;
    }

    public override bool IsLegal()
    {
      return RogueForm.Game.Rules.CanActorSwitchPlaceWith(m_Actor, m_Target);
    }

    public override void Perform()
    {
      RogueForm.Game.DoSwitchPlace(m_Actor, m_Target);
    }
  }
}
