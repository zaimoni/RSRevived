// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMoveStep
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionMoveStep : ActorAction
  {
    private Location m_NewLocation;

	public Location dest { get { return m_NewLocation; } }

    public ActionMoveStep(Actor actor, Direction direction)
      : base(actor)
    {
      m_NewLocation = actor.Location + direction;
    }

    public ActionMoveStep(Actor actor, Point to)
      : base(actor)
    {
      m_NewLocation = new Location(actor.Location.Map, to);
    }

    public override bool IsLegal()
    {
      return m_NewLocation.IsWalkableFor(m_Actor, out m_FailReason) && Rules.IsAdjacent(m_Actor.Location, m_NewLocation);
    }

    public override void Perform()
    {
      RogueForm.Game.DoMoveActor(m_Actor, m_NewLocation);
    }

    public override string ToString()
    {
      return "step: "+m_Actor.Location+" to "+m_NewLocation;
    }
  }
}
