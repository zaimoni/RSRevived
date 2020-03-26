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
            if (actions.Count > _forkBefore(actions)) throw new ArgumentOutOfRangeException(nameof(actions), actions, "fork may only terminate an action chain");
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
#if DEBUG
            if (null != chain1 && chain1.m_Actions.Count > _forkBefore(chain1.m_Actions)) throw new ArgumentOutOfRangeException(nameof(chain1), chain1, "fork may only terminate an action chain");
#endif
            if (act0 is ActionChain chain0) {
#if DEBUG
                if (chain0.m_Actions.Count >= _forkBefore(chain0.m_Actions)) throw new ArgumentOutOfRangeException(nameof(chain0), chain0, "fork may only terminate an action chain");
#endif
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

        public ActionChain(List<ActorAction> src, int lb) : base(src[0])
        {
#if DEBUG
          if (null == src || src.Count <= lb) throw new ArgumentNullException(nameof(src));
#endif
          var actions = new List<ActorAction>();
          while(lb < src.Count) actions.Add(src[lb++]);
#if DEBUG
          if (actions.Count > _forkBefore(actions)) throw new ArgumentOutOfRangeException(nameof(actions), actions, "fork may only terminate an action chain");
#endif
          m_Actions = actions;
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

        public override bool Abort()
        {
            foreach (var act in m_Actions) if (act.Abort()) return true;
            return false;
        }

        public bool ContainsSuffix(List<ActorAction> lhs, int lb)
        {
            return _equivalent(lhs, lb, m_Actions, 0);
        }

        private static bool _equivalent(List<ActorAction> lhs, int lhs_lb, List<ActorAction> rhs, int rhs_lb)
        {
#if DEBUG
            if (lhs_lb >= lhs.Count) throw new ArgumentOutOfRangeException(nameof(lhs_lb), lhs_lb, "past end of " + nameof(lhs));
            if (rhs_lb >= rhs.Count) throw new ArgumentOutOfRangeException(nameof(rhs_lb), rhs_lb, "past end of " + nameof(rhs));
#endif
            if (lhs.Count - lhs_lb < rhs.Count - rhs_lb) return false;
            while (lhs_lb < lhs.Count && rhs_lb < rhs.Count) {
              if (lhs[lhs_lb].AreEquivalent(rhs[rhs_lb])) {
                ++lhs_lb;
                ++rhs_lb;
                continue;
              }
              if (rhs.Count-1 == rhs_lb && rhs[rhs_lb] is _Action.Fork fork) return fork.ContainsSuffix(lhs, lhs_lb);
              return false;
            }
            return true;
        }

        // chain only cares about full prefix match -- if this returns true, the LHS may be discarded safely
        public override bool AreEquivalent(ActorAction? src)
        {
            int ub = m_Actions.Count;
            if (src is ActionChain chain1) return _equivalent(m_Actions, 0, chain1.m_Actions, 0);
            if (1 == ub) return m_Actions[0].AreEquivalent(src);
            return false;
        }

        public int CumulativeMoveCost()
        {
            int cost = Map.PathfinderMoveCosts(m_Actions[0]);
            int ub = m_Actions.Count;
            while (1 <= --ub) {
                int test = (m_Actions[ub] is _Action.Fork fork) ? fork.CumulativeMoveCost() : Map.PathfinderMoveCosts(m_Actions[ub]);
                if (int.MaxValue - cost >= test) cost += test;
                else return int.MaxValue;
            }
            return cost;
        }

        public ActionChain? splice(ActionChain src)
        {
            if (2 > m_Actions.Count) return null;
            if (2 > src.m_Actions.Count) return null;
            int i = 0;
            while (m_Actions[i].AreEquivalent(src.m_Actions[i])) {
                ++i;
                if (m_Actions.Count <= i) {
                  if (src.m_Actions.Count <= i) return null;    // equivalent...return null to avoid thrashing GC further
                } else if (src.m_Actions.Count <= i) {
                  return this;
                } else {
                  if (m_Actions[i] is _Action.Fork fork_left) {
                    fork_left.splice(new ActionChain(src.m_Actions, i));
                    return this;
                  } else if (src.m_Actions[i] is _Action.Fork fork_right) {
                    fork_right.splice(new ActionChain(m_Actions, i));
                    return src;
                  } else {
                    var args = new List<ActorAction>();
                    args.Add(i + 1 == m_Actions.Count ? m_Actions[i] : new ActionChain(m_Actions, i));
                    args.Add(i + 1 == src.m_Actions.Count ? src.m_Actions[i] : new ActionChain(src.m_Actions, i));
                    var fork = new _Action.Fork(m_Actor, args);
                    if (i + 1 == m_Actions.Count) {
                      m_Actions[i] = fork;
                      return this;
                    } else if (i + 1 == src.m_Actions.Count) {
                      src.m_Actions[i] = fork;
                      return src;
                    } else {
                      int ub = m_Actions.Count;
                      m_Actions[i] = fork;
                      while(i < --ub) m_Actions.RemoveAt(ub);
                      return this;
                    }
                  }
                }
            }
            return null;
        }

        private static int _forkBefore(List<ActorAction> src)
        {
            int lb = 0;
            while (src.Count > lb) if (src[lb++] is _Action.Fork) return lb;
            return int.MaxValue;
        }

        public bool RejectOrigin(Location origin, int i)
        {
          int ub = m_Actions.Count;
#if DEBUG
          if (ub <= i) throw new ArgumentOutOfRangeException(nameof(i), i, "not less than strict upper bound "+ub);
#endif
          do {
            var act = m_Actions[i];
            if (act is ActorDest a_dest) {
              if (a_dest.dest == origin) return true;
            } else if (act is _Action.Fork fork) {
              var test = fork.RejectOrigin(origin);
              if (null == test) return true;
              m_Actions[i] = test;
              return false;
            } else return false;
          } while(ub > ++i);
          return false;
        }

        public bool IsSemanticParadox() {
          int ub = m_Actions.Count;
          if (2 > ub) return false; // no context
          if (m_Actions[0] is ActorOrigin a_origin) RejectOrigin(a_origin.origin, 1);   // don't avoidably loop (may fix terminal fork)
          return false;
        }
    }
}
