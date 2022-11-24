// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMoveStep
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionMoveStep : ActorAction, ActorDest
    {
    private readonly Location m_NewLocation;

	public Location dest { get { return m_NewLocation; } }

    public ActionMoveStep(Actor actor, in Point to) : base(actor)
    {
      m_NewLocation = new Location(actor.Location.Map, to);
    }

    public ActionMoveStep(Actor actor, in Location to) : base(actor)
    {
      m_NewLocation = to;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanEnter(m_NewLocation);
    }

    public override bool IsPerformable()
    {
      return m_NewLocation.IsWalkableFor(m_Actor, out m_FailReason) && Rules.IsAdjacent(m_Actor.Location, m_NewLocation);
    }

    public override void Perform()
    {
      if (m_Actor.Location.Map==m_NewLocation.Map) RogueGame.Game.DoMoveActor(m_Actor, in m_NewLocation);
      else if (m_Actor.Location.Map.DistrictPos!=m_NewLocation.Map.DistrictPos) {
        var test = m_Actor.Location.Map.Denormalize(in m_NewLocation);
        RogueGame.Game.DoLeaveMap(m_Actor, test.Value.Position);
      } else RogueGame.Game.DoLeaveMap(m_Actor, m_Actor.Location.Position);
    }

    public override string ToString()
    {
      return "step: "+m_Actor.Location+" to "+m_NewLocation;
    }
  }
}
