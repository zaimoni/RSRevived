// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMoveStep
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

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

    public ActionMoveStep(Actor actor, Location to)
      : base(actor)
    {
      m_NewLocation = to;
    }

    public override bool IsLegal()
    {
      return m_NewLocation.IsWalkableFor(m_Actor, out m_FailReason) && Rules.IsAdjacent(m_Actor.Location, m_NewLocation);
    }

    public override void Perform()
    {
      if (m_Actor.Location.Map==m_NewLocation.Map) RogueForm.Game.DoMoveActor(m_Actor, m_NewLocation);
      else if (m_Actor.Location.Map.District!=m_NewLocation.Map.District) {
        var test = m_Actor.Location.Map.Denormalize(m_NewLocation);
        RogueForm.Game.DoLeaveMap(m_Actor, test.Value.Position, true);
      } else RogueForm.Game.DoLeaveMap(m_Actor, m_Actor.Location.Position, true);
    }

    public override string ToString()
    {
      return "step: "+m_Actor.Location+" to "+m_NewLocation;
    }
  }
}
