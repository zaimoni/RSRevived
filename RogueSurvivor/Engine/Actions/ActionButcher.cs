using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionButcher : ActorAction, NotSchedulable
  {
    private readonly Corpse m_Target;

    public ActionButcher(Actor actor, Corpse target) : base(actor)
    {
      m_Target = target;
    }

    public override bool IsLegal()
    {
      m_FailReason = ReasonCant(m_Actor, m_Target);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoButcherCorpse(m_Actor, m_Target);
    }

    public static string? ReasonCant(Actor a, Corpse c) {
      if (a.IsTired) return "tired";
      if (c.Location != a.Location || !a.Location.Map.Has(c)) return "not in same location";
      return null;
    }
  }
}
