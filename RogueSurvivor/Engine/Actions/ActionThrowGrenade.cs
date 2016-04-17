// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionThrowGrenade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionThrowGrenade : ActorAction
  {
    private Point m_ThrowPos;

    public ActionThrowGrenade(Actor actor, RogueGame game, Point throwPos)
      : base(actor, game)
    {
      this.m_ThrowPos = throwPos;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorThrowTo(this.m_Actor, this.m_ThrowPos, (List<Point>) null, out this.m_FailReason);
    }

    public override void Perform()
    {
      if (this.m_Actor.GetEquippedWeapon() is ItemPrimedExplosive)
        this.m_Game.DoThrowGrenadePrimed(this.m_Actor, this.m_ThrowPos);
      else
        this.m_Game.DoThrowGrenadeUnprimed(this.m_Actor, this.m_ThrowPos);
    }
  }
}
