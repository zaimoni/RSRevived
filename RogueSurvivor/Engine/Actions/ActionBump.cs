// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBump
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBump : ActorAction
  {
    private readonly Direction m_Direction;
    private readonly Location m_NewLocation;
    private readonly ActorAction m_ConcreteAction;

    public Direction Direction
    {
      get
      {
        return m_Direction;
      }
    }

    public ActorAction ConcreteAction
    {
      get
      {
        return m_ConcreteAction;
      }
    }

    public ActionBump(Actor actor, RogueGame game, Direction direction)
      : base(actor, game)
    {
            m_Direction = direction;
            m_NewLocation = actor.Location + direction;
            m_ConcreteAction = game.Rules.IsBumpableFor(m_Actor, game, m_NewLocation, out m_FailReason);
    }

    public override bool IsLegal()
    {
      if (m_ConcreteAction == null)
        return false;
      return m_ConcreteAction.IsLegal();
    }

    public override void Perform()
    {
      if (m_ConcreteAction == null)
        return;
            m_ConcreteAction.Perform();
    }
  }
}
