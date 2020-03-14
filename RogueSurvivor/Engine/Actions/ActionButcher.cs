using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionButcher : ActorAction
  {
    private readonly Corpse m_Target;

    public ActionButcher(Actor actor, Corpse target)
      : base(actor)
    {
      m_Target = target;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanButcher(m_Target, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoButcherCorpse(m_Actor, m_Target);
    }
  }
}
