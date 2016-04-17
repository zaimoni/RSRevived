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
    private Fortification m_Fort;

    public ActionRepairFortification(Actor actor, RogueGame game, Fortification fort)
      : base(actor, game)
    {
      if (fort == null)
        throw new ArgumentNullException("fort");
      this.m_Fort = fort;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorRepairFortification(this.m_Actor, this.m_Fort, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoRepairFortification(this.m_Actor, this.m_Fort);
    }
  }
}
