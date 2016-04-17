// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBuildFortification
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBuildFortification : ActorAction
  {
    private Point m_BuildPos;
    private bool m_IsLarge;

    public ActionBuildFortification(Actor actor, RogueGame game, Point buildPos, bool isLarge)
      : base(actor, game)
    {
      this.m_BuildPos = buildPos;
      this.m_IsLarge = isLarge;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorBuildFortification(this.m_Actor, this.m_BuildPos, this.m_IsLarge);
    }

    public override void Perform()
    {
      this.m_Game.DoBuildFortification(this.m_Actor, this.m_BuildPos, this.m_IsLarge);
    }
  }
}
