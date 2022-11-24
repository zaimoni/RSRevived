﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBuildFortification
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBuildFortification : ActorAction
  {
    private readonly Point m_BuildPos;
    private readonly bool m_IsLarge;

    public ActionBuildFortification(Actor actor, in Point buildPos, bool isLarge) : base(actor)
    {
      m_BuildPos = buildPos;
      m_IsLarge = isLarge;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanBuildFortification(in m_BuildPos, m_IsLarge);
    }

    public override void Perform()
    {
      RogueGame.Game.DoBuildFortification(m_Actor, in m_BuildPos, m_IsLarge);
    }
  }
}
