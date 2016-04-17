// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRangedAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionRangedAttack : ActorAction
  {
    private List<Point> m_LoF = new List<Point>();
    private Actor m_Target;
    private FireMode m_Mode;

    public ActionRangedAttack(Actor actor, RogueGame game, Actor target, FireMode mode)
      : base(actor, game)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      this.m_Target = target;
      this.m_Mode = mode;
    }

    public ActionRangedAttack(Actor actor, RogueGame game, Actor target)
      : this(actor, game, target, FireMode.DEFAULT)
    {
    }

    public override bool IsLegal()
    {
      this.m_LoF.Clear();
      return this.m_Game.Rules.CanActorFireAt(this.m_Actor, this.m_Target, this.m_LoF, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoSingleRangedAttack(this.m_Actor, this.m_Target, this.m_LoF, this.m_Mode);
    }
  }
}
