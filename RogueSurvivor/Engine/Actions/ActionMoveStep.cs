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

    public ActionMoveStep(Actor actor, RogueGame game, Direction direction)
      : base(actor, game)
    {
            m_NewLocation = actor.Location + direction;
    }

    public ActionMoveStep(Actor actor, RogueGame game, Point to)
      : base(actor, game)
    {
            m_NewLocation = new Location(actor.Location.Map, to);
    }

    public override bool IsLegal()
    {
      return m_Game.Rules.IsWalkableFor(m_Actor, m_NewLocation, out m_FailReason);
    }

    public override void Perform()
    {
            m_Game.DoMoveActor(m_Actor, m_NewLocation);
    }
  }
}
