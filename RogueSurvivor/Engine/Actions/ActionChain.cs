using System;
using System.Collections.Generic;
using djack.RogueSurvivor.Data;

using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    internal class ActionChain : ActorAction,Resolvable
    {
        private readonly List<ActorAction> m_Actions;

        public ActorAction ConcreteAction { get { return m_Actions[0]; } }

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

        public ActionChain(ActorAction act0, ActorAction act1)
        : base(act0)
        {
#if DEBUG
            if (!act0.PerformedBy(act1)) throw new ArgumentOutOfRangeException(nameof(act1), act1, "should have same target as " + act0);
#endif
            var chain1 = act1 as ActionChain;
            if (act0 is ActionChain chain0) {
                m_Actions = new List<ActorAction>(chain0.m_Actions);
                if (null != chain1) m_Actions.AddRange(chain1.m_Actions);
                else m_Actions.Add(act1);
            } else if (null != chain1) {
                m_Actions = new List<ActorAction>(chain1.m_Actions);
                m_Actions.Insert(0, act0);
            } else {
                m_Actions = new List<ActorAction> { act0, act1 };
            }
            m_FailReason = m_Actions[0].FailReason;
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

        public override bool AreEquivalent(ActorAction? src)
        {
            int ub = m_Actions.Count;
            if (src is ActionChain chain1) {
                if (ub != chain1.m_Actions.Count) return false;
                while (0 <= --ub) if (!m_Actions[ub].AreEquivalent(chain1.m_Actions[ub])) return false;
                return true;
            }
            if (1 == ub) return m_Actions[0].AreEquivalent(src);
            return false;
        }

        public int CumulativeMoveCost()
        {
            int cost = Map.PathfinderMoveCosts(m_Actions[0]);
            int ub = m_Actions.Count;
            while (1 <= --ub) {
                int test = Map.PathfinderMoveCosts(m_Actions[ub]);
                if (int.MaxValue - cost >= test) cost += test;
                else return int.MaxValue;
            }
            return cost;
        }

        public ActorAction? Next { get {
          return 1<m_Actions.Count ? m_Actions[1] : null;
        } }

        public bool IsSemanticParadox() {
          int ub = m_Actions.Count;
          if (2 > ub) return false; // no context
          if (m_Actions[0] is ActorOrigin a_origin) {
            while(1 <= --ub) {
              if (m_Actions[ub] is ActorDest a_dest && a_dest.dest == a_origin.origin) return true; // path is looped
            }
//          ub = m_Actions.Count;
          }
          return false;
        }
    }
}
