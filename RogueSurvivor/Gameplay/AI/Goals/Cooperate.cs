using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System;
using System.Collections.Generic;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // need this level of indirection so that all actors know about the plan's progress
    [Serializable]
    class SharedPlan
    {
        private WorldUpdate m_Plan;
        private bool m_Expired = false;

        public bool IsExpired { get { return m_Expired; } }

        public SharedPlan(WorldUpdate src) {
            m_Plan = src;
        }

        public bool IsRelevant(Location loc) { return m_Plan.IsRelevant(loc); }

        public ActorAction? Bind(Actor who) {
            if (m_Plan.IsRelevant(who.Location)) {
                var test = m_Plan;
                while(ForceRelevant(who.Location, ref test));
                var act = test.Bind(who);
                if (null != act) {
                    if (test is Engine.Op.Join join) m_Plan = join.Sequel;
                    else m_Expired = true;
                    return act;
                }
            }
            return null;
        }

        public HashSet<Location>? Goals() {
            var ret = new HashSet<Location>();
            m_Plan.Goals(ret);
            m_Plan.Blacklist(ret);
            return 0 < ret.Count ? ret : null;
        }

        static bool ForceRelevant(Location loc, ref WorldUpdate src) {
            if (src is Engine.Op.Fork fork) return fork.ForceRelevant(loc, ref src);
            if (src is Engine.Op.Join join) return join.ForceRelevant(loc, ref src);
            return false;
        }
    }

    [Serializable]
    class Cooperate : Objective
    {
        private SharedPlan m_Plan;

        public Cooperate(int t0, Actor who, SharedPlan plan) : base(t0, who) {
            m_Plan = plan;
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            if (m_Plan.IsRelevant(m_Actor.Location)) {
                ret = m_Plan.Bind(m_Actor);
                if (null != ret) {
                    if (m_Plan.IsExpired) _isExpired = true;
                    return true;
                }
                ret = new ActionWait(m_Actor); // not necessarily (crowd control issues)
                return true;
            }
            var goals = m_Plan.Goals();
            if (null != goals) (m_Actor.Controller as ObjectiveAI).BehaviorPathTo(goals);
            return false;
        }
    }
}
