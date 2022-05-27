using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using djack.RogueSurvivor.Data;
using Zaimoni.Data;

using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;
using ActionChain = djack.RogueSurvivor.Engine.Actions.ActionChain;

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    internal class Chain : WorldUpdate, CanReduce<WorldUpdate>
    {
        private readonly List<WorldUpdate> m_Actions;

        // for now, assume any constructor ActionChain needs, we need an analog for
        public Chain(List<WorldUpdate> actions)
        {
#if DEBUG
            if (null == actions || 2 > actions.Count) throw new ArgumentNullException(nameof(actions));
            if (actions.Count > _forkBefore(actions)) throw new ArgumentOutOfRangeException(nameof(actions), actions, "fork may only terminate an action chain");
#endif
            m_Actions = actions;
        }

        public Chain(WorldUpdate act0, WorldUpdate act1)
        {
            var chain1 = act1 as Chain;
#if DEBUG
            if (null != chain1 && chain1.m_Actions.Count > _forkBefore(chain1.m_Actions)) throw new ArgumentOutOfRangeException(nameof(chain1), chain1, "fork may only terminate an action chain");
#endif
            if (act0 is Chain chain0) {
#if DEBUG
                if (chain0.m_Actions.Count >= _forkBefore(chain0.m_Actions)) throw new ArgumentOutOfRangeException(nameof(chain0), chain0, "fork may only terminate an action chain");
#endif
                m_Actions = new(chain0.m_Actions);
                if (null != chain1) m_Actions.AddRange(chain1.m_Actions);
                else m_Actions.Add(act1);
            } else if (null != chain1) {
                m_Actions = new(chain1.m_Actions);
                m_Actions.Insert(0, act0);
            } else {
                m_Actions = new(){ act0, act1 };
            }
        }

        public Chain(List<WorldUpdate> src, int lb)
        {
#if DEBUG
            if (null == src || src.Count <= lb) throw new ArgumentNullException(nameof(src));
#endif
            var actions = new List<WorldUpdate>();
            while (lb < src.Count) actions.Add(src[lb++]);
#if DEBUG
            if (actions.Count > _forkBefore(actions)) throw new ArgumentOutOfRangeException(nameof(actions), actions, "fork may only terminate an action chain");
#endif
            m_Actions = actions;
        }

        public override bool IsLegal() {
            foreach (var act in m_Actions) if (!act.IsLegal()) return false;
            return true;
        }

        public WorldUpdate? Reduce()
        {
restart:
          if (m_Actions[0] is CanReduce<WorldUpdate> reducing) {
            var now = reducing.Reduce();
            if (null == now) return null;
            if (now is Chain chain) {
              m_Actions.RemoveAt(0);
              chain.Append(1 < m_Actions.Count ? this : m_Actions[0]);
              return chain;
            } else if (now is Fork fork) {
              m_Actions.RemoveAt(0);
              return fork.Append(1 < m_Actions.Count ? this : m_Actions[0]);
            } else
              m_Actions[0] = now;
          }
          if (m_Actions[0] is CanFinish ending) {
            if (ending.IsCompleted()) {
              if (2 == m_Actions.Count) return m_Actions[1];
              m_Actions.RemoveAt(0);
              goto restart;
            }
          }

          return this;
        }

        /// <returns>null, or a Performable action</returns>
        public override ActorAction? Bind(Actor src) {
#if DEBUG
            if (!(src.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
            // unclear what is correct here.  Be conservative.
            return m_Actions[0].Bind(src);
        }

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = m_Actions[0].Bind(src);
            if (null == act) return null;
            if (2 == m_Actions.Count) return new(act, m_Actions[1]);
            return new(act, new Chain(m_Actions, 1));
        }

        public override bool IsRelevant() { return false; }
        public override bool IsRelevant(Location loc) { return m_Actions[0].IsRelevant(loc); }
        public override bool IsSuppressed(Actor a) { return m_Actions[0].IsSuppressed(a); } // return to this later
        public override void Blacklist(HashSet<Location> goals) { m_Actions[0].Blacklist(goals); }
        public override void Goals(HashSet<Location> goals) { m_Actions[0].Goals(goals); }

      public void Append(WorldUpdate next)
      {
         if (m_Actions[^-1] is Fork final_fork) {
            var join = final_fork.Append(next);
            m_Actions[^-1] = join;
         } else if (next is Chain chain) {
            m_Actions.AddRange(chain.m_Actions);
         }
      }


        private static int _forkBefore(List<WorldUpdate> src)
        {
            int lb = 0;
            while (src.Count > lb) if (src[lb++] is Fork) return lb;
            return int.MaxValue;
        }
    }
}
