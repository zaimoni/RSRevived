﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionEatCorpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionEatCorpse : ActorAction
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
      return m_Actor.CanEatCorpse(out m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoEatCorpse(m_Actor, m_Target);
    }
  }
}
