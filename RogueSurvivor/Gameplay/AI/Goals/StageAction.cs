using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    internal class StageAction : Objective
    {
      public readonly ActorAction Intent;

      public StageAction(int t0, Actor who, ActorAction free_act) : base(t0,who)
      {
#if DEBUG
        if (!(who.Controller is ObjectiveAI)) throw new ArgumentOutOfRangeException(nameof(who), who, "not intelligent enough to plan ahead");
#endif
        Intent = free_act;
      }

      // always execute.  Expire on execution
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (Intent.IsPerformable() && !Intent.Abort()) {
          // XXX need some sense of what a combat action is
          var ai = m_Actor.Controller;
          if (null != ai.enemies_in_FOV) return false;

          var oai = (ai as ObjectiveAI)!;
          if (!oai.VetoAction(Intent)) oai.Stage(Intent);
        }
       _isExpired = true;
        return true;
      }
    }
}
