// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMeleeAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionMeleeAttack : ActorAction
  {
    private readonly Actor m_Target;

    public ActionMeleeAttack(Actor actor, Actor target)
      : base(actor)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      m_Target = target;
      actor.Activity = Activity.IDLE;   // transition to fighting is in DoMeleeAttack
    }

    public override bool IsLegal()
    {
      return m_Actor.CanMeleeAttack(m_Target);
    }

    public override void Perform()
    {
      RogueForm.Game.DoMeleeAttack(m_Actor, m_Target);
    }
  }
}
