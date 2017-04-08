using djack.RogueSurvivor.Data;
using System.Drawing;
using System.Diagnostics.Contracts;

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
      Contract.Requires(null != target);
      m_Target = target;
      m_Direction = pushDir;
      m_To = target.Location.Position + pushDir;
    }

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
