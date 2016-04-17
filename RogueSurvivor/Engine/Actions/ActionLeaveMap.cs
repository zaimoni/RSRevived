// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionLeaveMap
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionLeaveMap : ActorAction
  {
    private Point m_ExitPoint;

    public Point ExitPoint
    {
      get
      {
        return this.m_ExitPoint;
      }
    }

    public ActionLeaveMap(Actor actor, RogueGame game, Point exitPoint)
      : base(actor, game)
    {
      this.m_ExitPoint = exitPoint;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorLeaveMap(this.m_Actor, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoLeaveMap(this.m_Actor, this.m_ExitPoint, true);
    }
  }
}
