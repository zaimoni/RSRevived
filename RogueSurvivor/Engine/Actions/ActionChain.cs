using System;
using System.Collections.Generic;
using djack.RogueSurvivor.Data;

using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    internal class ActionChain : ActorAction
    {
        private readonly List<ActorAction> m_Actions;

        public ActionChain(Actor actor, List<ActorAction> actions)
        : base(actor)
        {
#if DEBUG
            if (null == actions || 2 > actions.Count) throw new ArgumentNullException(nameof(actions));
            if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
            m_Actions = actions;
            m_FailReason = actions[0].FailReason;
        }

        public override bool IsLegal()
        {
            return m_Actions[0].IsLegal();
        }

        public override bool IsPerformable()
        {
            return m_Actions[0].IsPerformable();
        }

        public override void Perform()
        {
            (m_Actor.Controller as ObjectiveAI).ExecuteActionChain(m_Actions);
        }
    }
}
