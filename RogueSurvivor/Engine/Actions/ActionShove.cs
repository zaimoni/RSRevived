using djack.RogueSurvivor.Data;
using System;

using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionShove : ActorAction, ActorDest
    {
    private readonly Actor m_Target;
    private readonly Direction m_Direction;
    private readonly Point m_To;

    public ActionShove(Actor actor, Actor target, Direction pushDir) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
      m_Direction = pushDir;
      m_To = target.Location.Position + pushDir;
    }

    public Actor Target { get { return m_Target; } }
    public Point To { get { return m_To; } }
    public Direction Dir { get { return m_Direction; } }
    public Location dest { get { return m_Target.Location; } }
    public Location a_dest { get { return new Location(m_Target.Location.Map,m_To); } }

    public override bool IsLegal()
    {
      if (m_Actor.CanShove(m_Target))
        return m_Target.CanBeShovedTo(m_To, out m_FailReason);
      return false;
    }

    public override void Perform()
    {
      RogueGame.Game.DoShove(m_Actor, m_Target, a_dest);
    }
  }
}
