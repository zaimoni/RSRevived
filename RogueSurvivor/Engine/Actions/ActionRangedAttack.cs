﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRangedAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  // VAPORWARE need to include burst fire as well (both aimed and snap/hipfire)
  // * convert army rifle to burst-competent, scale army rifle clip accordingly; leave sniper rifle alone
  // * machine pistol: not in gun stores; allow for survivalist caches and SWAT police (leaders?).
  [Serializable]
  internal enum FireMode
  {
    AIMED = 0,  // single, aimed shot; keyword default
    RAPID       // single snap shot
  }

  [Serializable]
  internal class ActionRangedAttack : ActorAction, CombatAction
  {
    private readonly List<Point> m_LoF = new List<Point>();
    private readonly Actor m_Target;
    private readonly FireMode m_Mode;

    public ActionRangedAttack(Actor actor, Actor target, FireMode mode=default) : base(actor)
    {
      m_Target = target;
      m_Mode = mode;
    }

    public Actor target { get { return m_Target; } }

    public override bool IsLegal()
    {
      m_LoF.Clear();
      return m_Actor.CanFireAt(m_Target, m_LoF, out m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoRangedAttack(m_Actor, m_Target, m_LoF, m_Mode);
    }
  }
}
