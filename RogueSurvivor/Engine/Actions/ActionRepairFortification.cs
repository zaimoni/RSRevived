// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRepairFortification
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionRepairFortification : ActorAction
  {
    private readonly Fortification m_Fort;

    public ActionRepairFortification(Actor actor, Fortification fort) : base(actor)
    {
      m_Fort = fort
#if DEBUG
        ?? throw new ArgumentNullException(nameof(fort))
#endif
      ;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanRepairFortification(m_Fort, out m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoRepairFortification(m_Actor, m_Fort);
    }
  }
}
