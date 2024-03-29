﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;
using Zaimoni.Data;

using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;
using Goal_NextAction = djack.RogueSurvivor.Gameplay.AI.Goal_NextAction;

#nullable enable

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class Join : WorldUpdate, CanReduce<WorldUpdate>
    {
        private List<WorldUpdate> m_Options;
        private WorldUpdate m_Sequel;

        public WorldUpdate Sequel { get { return m_Sequel; } }

        public Join(List<WorldUpdate> options, WorldUpdate sequel)
        {
            m_Options = options;
            m_Sequel = sequel;
#if DEBUG
            if (!IsLegal()) throw new InvalidProgramException("created illegal Join");
#endif
        }

        public Join(WorldUpdate option, WorldUpdate sequel)
        {
            m_Options = new List<WorldUpdate> { option };
            m_Sequel = sequel;
#if DEBUG
            if (!IsLegal()) throw new InvalidProgramException("created illegal Join");
#endif
        }

        public override bool IsLegal()
        {
            var ub = m_Options.Count;
            while (0 <= --ub) if (m_Options[ub].IsLegal()) return m_Sequel.IsLegal();
            return false;
        }

        public override bool IsRelevant()
        {
            var ub = m_Options.Count;
            while (0 <= --ub) if (m_Options[ub].IsRelevant()) return true;
            return false;
        }

        public override bool IsRelevant(Location loc)
        {
            var ub = m_Options.Count;
            while (0 <= --ub) if (m_Options[ub].IsRelevant(loc)) return true;
            return false;
        }

        public override bool IsSuppressed(Actor a)
        {
            var ub = m_Options.Count;
            while (0 <= --ub) if (!m_Options[ub].IsSuppressed(a)) return false;
            return true;
        }

        public WorldUpdate? Reduce()
        {
          if (!m_Sequel.IsLegal()) return null;
          if (m_Sequel is CanFinish x) {
            if (x.IsCompleted()) return m_Sequel;
          }

          var ub = m_Options.Count;
          while (0 <= --ub) {
            var act = m_Options[ub];
            if (!act.IsLegal()) m_Options.RemoveAt(ub);
            if (act is CanFinish y) {
              if (y.IsCompleted()) return m_Sequel;
            }
          };

          switch (m_Options.Count)
          {
          case 1: return new Chain(m_Options[0], m_Sequel);
          case 0: return null;
          default: return this;
          }
        }


        public override ActorAction? Bind(Actor src)
        {
#if DEBUG
            if (!(src.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
            var actions = new List<ActorAction>();
            foreach(var x in m_Options) {
                if (x.IsRelevant(src.Location) && !x.IsSuppressed(src)) {
#if DEBUG
                    if (x is Join) throw new InvalidOperationException("test case");
#endif
                    var act = x.Bind(src);
                    if (null != act) actions.Add(act);
                }
            }
            var act_count = actions.Count;
            if (1 >= act_count) return (0 >= act_count) ? null : actions[0];
            return new _Action.Fork(src, actions);
        }

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = Bind(src);
            if (null == act) return null;
            return new(act, m_Sequel);
        }

        public override void Blacklist(HashSet<Location> goals)
        {
            var ub = m_Options.Count;
            while (0 <= --ub) m_Options[ub].Blacklist(goals);
        }

        public override void Goals(HashSet<Location> goals)
        {
            var ub = m_Options.Count;
            while (0 <= --ub) m_Options[ub].Goals(goals);
        }

        public bool ForkMerge(Join rhs) {
            if (m_Sequel != rhs.m_Sequel) return false;
            foreach (var act in rhs.m_Options) if (!m_Options.Contains(act)) m_Options.Add(act);
            return true;
        }

        public bool ForceRelevant(Location loc, ref WorldUpdate dest)
        {
            var staging = new List<WorldUpdate>();
            foreach (var act in m_Options) if (act.IsRelevant(loc)) staging.Add(act);
            var staged = staging.Count;
            if (1 > staged) throw new InvalidOperationException("tried to force-relevant a not-relevant objective");
            // also of interest: maybe prefilter by whether actions are performable?
            // Fork has a sifting stage here
            if (m_Options.Count > staged) dest = new Join(staging, m_Sequel);
            return false;
        }
    }
}

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class Join : ActorAction, RecursivePathfinderMoveCost
    {
      private List<ActorAction> m_Options;
      private WorldUpdate? m_Sequel = null;
      [NonSerialized] private List<ActorAction>? _real_options;

      public Join(Actor actor, List<ActorAction> options, WorldUpdate? sequel = null) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
        m_Options = options;
        m_Sequel = sequel;
      }

      public override bool IsLegal() {
        var ub = m_Options.Count;
        while (0 <= --ub) if (m_Options[ub].IsLegal()) return true;
        return false;
      }

      public override bool IsPerformable() {
        _real_options = m_Options.FindAll(act => act.IsPerformable());
        if (0 >= _real_options.Count) _real_options = null;
        return null != _real_options;
      }

      public override void Perform() {
#if DEBUG
        if (null == _real_options) throw new ArgumentNullException(nameof(_real_options));
#endif
        var act = Rules.Get.DiceRoller.Choose(_real_options); // \todo more sophisticated choice logic
        act.Perform();

        var sequel = m_Sequel?.Bind(m_Actor);

        if (null != sequel) {
          (m_Actor.Controller as ObjectiveAI).SetObjective(new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter + 1, m_Actor, sequel));
        }
      }

      public int PathfinderMoveCost() {
        int ret = int.MaxValue;
        foreach(var act in m_Options) {
          var test = Map.PathfinderMoveCosts(act);
          if (test < ret) ret = test;
        }
        // \todo try to handle m_Sequel
        return ret;
      }
    }
}