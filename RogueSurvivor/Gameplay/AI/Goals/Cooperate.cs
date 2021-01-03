using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System;
using System.Collections.Generic;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // need this level of indirection so that all actors know about the plan's progress
    [Serializable]
    class SharedPlan
    {
        private WorldUpdate m_Plan;
        private Condition<Actor> m_Target;
        private bool m_Expired = false;

        public bool IsExpired { get { return m_Expired; } }

        public SharedPlan(WorldUpdate src, Condition<Actor> done) {
#if DEBUG
            if (!src.IsLegal()) throw new InvalidOperationException("illegal objective");
#endif
            m_Plan = src;
            m_Target = done;
        }

        public bool IsLegal() { return !m_Target.IsDone() && m_Plan.IsLegal(); }

        public bool IsSuppressed(Actor a) {
            // we ask the plan, because combat plans would not be suppressed just by enemies in sight
            return m_Plan.IsSuppressed(a);
        }

        public bool IsRelevant(Location loc) {
#if DEBUG
            if (!m_Plan.IsLegal()) throw new InvalidOperationException("illegal objective");
#else
            if (!m_Plan.IsLegal()) {
                m_Expired = true;
                return false;
            }
#endif
            return m_Plan.IsRelevant(loc);
        }

        public ActorAction? Bind(Actor who) {
            if (m_Plan.IsRelevant(who.Location)) {
                var test = m_Plan;
                while(ForceRelevant(who.Location, ref test));
                var act = test.Bind(who);
                if (null != act) {
                    if (test is Engine.Op.Join join && join.Sequel.IsLegal()) m_Plan = join.Sequel;
                    else m_Expired = true;
                    return act;
                }
            }
            return null;
        }

        public HashSet<Location>? Goals(Actor actor) {
#if DEBUG
            if (!m_Plan.IsLegal()) throw new InvalidOperationException("illegal objective");
#else
            if (!m_Plan.IsLegal()) return null;
#endif
            var ret = new HashSet<Location>();
            m_Plan.Goals(ret);
            m_Plan.Blacklist(ret);
            ret.RemoveWhere(loc => !actor.CanEnter(loc)); // matters for burning cars
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

        public Cooperate(Actor who, SharedPlan plan) : base(who.Location.Map.LocalTime.TurnCounter, who) {
#if DEBUG
            if (!plan.IsLegal()) throw new InvalidOperationException("tried to cooperate with illegal plan");
#endif
            m_Plan = plan;
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            if (m_Plan.IsExpired) return true; // expired
            if (!m_Plan.IsLegal()) {
                _isExpired = true;
                return true;
            }
            if (m_Plan.IsRelevant(m_Actor.Location)) {
                ret = m_Plan.Bind(m_Actor);
                if (null != ret) {
                    if (m_Plan.IsExpired) _isExpired = true;
                    return true;
                }
                ret = new ActionWait(m_Actor); // not necessarily (crowd control issues)
                return true;
            }
            var goals = m_Plan.Goals(m_Actor);
            if (null != goals) {
#if DEBUG
                if (goals.Contains(m_Actor.Location)) throw new InvalidOperationException("test case");
#endif
                ret = (m_Actor.Controller as ObjectiveAI).BehaviorPathTo(goals);
                return null != ret;
            }
            return false;
        }
    }
}
