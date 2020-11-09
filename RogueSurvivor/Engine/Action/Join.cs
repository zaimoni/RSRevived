using System;
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
    class Join : WorldUpdate
    {
        private List<WorldUpdate> m_Options;
        private WorldUpdate m_Sequel;

        public WorldUpdate Sequel { get { return m_Sequel; } }

        public Join(List<WorldUpdate> options, WorldUpdate sequel)
        {
#if DEBUG
            if (!sequel.IsLegal()) throw new InvalidOperationException("illegal sequel");
#endif
            m_Options = options;
            m_Sequel = sequel;
        }

        public Join(WorldUpdate option, WorldUpdate sequel)
        {
#if DEBUG
            if (!sequel.IsLegal()) throw new InvalidOperationException("illegal sequel");
#endif
            m_Options = new List<WorldUpdate> { option };
            m_Sequel = sequel;
        }

        public override bool IsLegal()
        {
            var ub = m_Options.Count;
            while (0 <= --ub) if (m_Options[ub].IsLegal()) return true;
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

        public override ActorAction? Bind(Actor src)
        {
#if DEBUG
            throw new InvalidOperationException("tracing");
#endif
            var opts = new List<ActorAction>();
            foreach (var x in m_Options) {
                var act = x.Bind(src);
                if (null != act) opts.Add(act);
            }
            if (0 >= opts.Count) return null;
            return new _Action.Join(src, opts, m_Sequel);
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
    class Join : ActorAction
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


    }
}