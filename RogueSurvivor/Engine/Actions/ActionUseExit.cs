// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUseExit
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionUseExit : ActorAction
  {
    private Point m_ExitPoint;

    public ActionUseExit(Actor actor, Point exitPoint)
      : base(actor)
    {
      m_ExitPoint = exitPoint;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanUseExit(m_ExitPoint, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoUseExit(m_Actor, m_ExitPoint);
    }
  }
}
