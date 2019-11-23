// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUseExit
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionUseExit : ActorAction,ActorDest
  {
    private readonly Location m_ExitPoint;

#nullable enable

    public Exit Exit { get { return m_ExitPoint.Exit!; } }
    public bool IsNotBlocked { get { return Exit.IsNotBlocked(m_Actor); } }
    public Location dest { get { return Exit.Location; } }

    public ActionUseExit(Actor actor, in Location exitPoint)
      : base(actor)
    {
#if DEBUG
      if (null == exitPoint.Exit) throw new ArgumentNullException("exitPoint.Exit");
#endif
      m_ExitPoint = exitPoint;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.Location.Map==m_ExitPoint.Map && m_Actor.CanUseExit(m_ExitPoint.Position, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoUseExit(m_Actor, m_ExitPoint.Position);
    }
  }
}
