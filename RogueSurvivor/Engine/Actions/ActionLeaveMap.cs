// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionLeaveMap
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionLeaveMap : ActorAction
  {
    private readonly Point m_ExitPoint;

    public ActionLeaveMap(Actor actor, in Point exitPoint)
      : base(actor)
    {
#if DEBUG
       if (actor.Location.Map.IsInBounds(exitPoint)) throw new ArgumentOutOfRangeException(nameof(exitPoint), exitPoint, "must not be InBounds");
#endif
       m_ExitPoint = exitPoint;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanLeaveMap(m_ExitPoint, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoLeaveMap(m_Actor, m_ExitPoint);
    }

    public override string ToString()
    {
      return m_Actor.Name+"leaving "+m_Actor.Location.ToString()+" for "+m_ExitPoint.ToString();
    }
  }
}
