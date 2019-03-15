using djack.RogueSurvivor.Data;
using System;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionShove : ActorAction
  {
    private readonly Actor m_Target;
    private readonly Direction m_Direction;
    private readonly Point m_To;

    public ActionShove(Actor actor, Actor target, Direction pushDir)
      : base(actor)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      m_Target = target;
      m_Direction = pushDir;
      m_To = target.Location.Position + pushDir;
    }

    public Actor Target { get { return m_Target; } }
    public Point To { get { return m_To; } }
    public Direction Dir { get { return m_Direction; } }

    public override bool IsLegal()
    {
      if (m_Actor.CanShove(m_Target))
        return m_Target.CanBeShovedTo(m_To, out m_FailReason);
      return false;
    }

    public override void Perform()
    {
      RogueForm.Game.DoShove(m_Actor, m_Target, m_To);
    }
  }
}
