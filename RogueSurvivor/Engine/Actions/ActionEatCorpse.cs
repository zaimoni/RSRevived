// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionEatCorpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  // schedulable version would target a Location
  internal class ActionEatCorpse : ActorAction,NotSchedulable
  {
    private readonly Corpse m_Target;

    public ActionEatCorpse(Actor actor, Corpse target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
     ;
    }

    public override bool IsLegal()
    {
      m_FailReason = ReasonCant();
      return string.IsNullOrEmpty(m_FailReason);
    }

    // strictly speaking, performability requires being in the same location
    // but all three of our construction callers handle this; this is a legacy class

    public override void Perform()
    {
      RogueGame.Game.DoEatCorpse(m_Actor, m_Target);
    }

    // AI support
    // this has to be coordinated with ReasonCantEatCorpse
    static public bool WantTo(Actor a) {
      if (a.Model.Abilities.IsUndead) return a.HitPoints < a.MaxHPs; // legal regardless, but AI would not choose this
      if (a.IsStarving) return true;
      return a.IsInsane;
    }

    private string ReasonCant()
    { // this needs revision for feral dogs, which do not have sanity and should be able to eat corpses when merely hungry
      if (!m_Actor.Model.Abilities.IsUndead && !m_Actor.IsStarving && !m_Actor.IsInsane) return "not starving or insane";
      return "";
    }

  }
}
