﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRangedAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionRangedAttack : ActorAction
  {
    private readonly List<Point> m_LoF = new List<Point>();
    private readonly Actor m_Target;
    private readonly FireMode m_Mode;

    public ActionRangedAttack(Actor actor, Actor target, FireMode mode=FireMode.DEFAULT)
      : base(actor)
    {
      Contract.Requires(null != target);
      m_Target = target;
      m_Mode = mode;
    }

    public override bool IsLegal()
    {
      m_LoF.Clear();
      return m_Actor.CanFireAt(m_Target, m_LoF, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoSingleRangedAttack(m_Actor, m_Target, m_LoF, m_Mode);
    }
  }
}
