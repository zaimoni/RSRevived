// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMeleeAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal interface CombatAction
  {
    Actor target { get; }  // of m_Actor
  }

   [Serializable]
  internal class ActionMeleeAttack : ActorAction, CombatAction
    {
    private readonly Actor m_Target;

    public ActionMeleeAttack(Actor actor, Actor target) : base(actor)
    {
      m_Target = target;
      actor.Activity = Activity.IDLE;   // transition to fighting is in DoMeleeAttack
    }

    public Actor target { get { return m_Target; } }

    public override bool IsLegal()
    {
      return m_Actor.CanMeleeAttack(m_Target);
    }

    public override void Perform()
    {
      RogueGame.Game.DoMeleeAttack(m_Actor, m_Target);
    }
  }
}
