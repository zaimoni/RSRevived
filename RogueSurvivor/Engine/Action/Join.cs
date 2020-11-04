﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;

using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;
using Goal_NextAction = djack.RogueSurvivor.Gameplay.AI.Goal_NextAction;

#nullable enable

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class Join : WorldUpdate
    {
        private List<WorldUpdate> m_options;
        private WorldUpdate? m_sequel = null;

        public Join(List<WorldUpdate> options)
        {
            m_options = options;
        }

        public Join(List<WorldUpdate> options, WorldUpdate sequel)
        {
            m_options = options;
            m_sequel = sequel;
        }

        public Join(WorldUpdate option, WorldUpdate sequel)
        {
            m_options = new List<WorldUpdate> { option };
            m_sequel = sequel;
        }

        public override bool IsLegal()
        {
            foreach (var act in m_options) if (act.IsLegal()) return true;
            return false;
        }

        public override bool IsRelevant()
        {
            foreach (var act in m_options) if (act.IsRelevant()) return true;
            return false;
        }

        public override bool IsRelevant(Location loc)
        {
            foreach (var act in m_options) if (act.IsRelevant(loc)) return true;
            return false;
        }

        public override ActorAction? Bind(Actor src)
        {
            var opts = new List<ActorAction>();
            foreach (var x in m_options) {
                var act = x.Bind(src);
                if (null != act) opts.Add(act);
            }
            if (0 >= opts.Count) return null;
            if (null != m_sequel && !m_sequel.IsLegal()) m_sequel = null;
            return new _Action.Join(src, opts, m_sequel);
        }

        public override void Blacklist(HashSet<Location> goals)
        {
            foreach (var act in m_options) act.Blacklist(goals);
        }

        public override void Goals(HashSet<Location> goals)
        {
            foreach (var act in m_options) act.Goals(goals);
        }
    }
}

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class Join : ActorAction
    {
      private List<ActorAction> m_options;
      private WorldUpdate? m_sequel = null;
      [NonSerialized] private List<ActorAction>? _real_options;

      public Join(Actor actor, List<ActorAction> options, WorldUpdate? sequel = null) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
        m_options = options;
        m_sequel = sequel;
      }

      public override bool IsLegal() {
        foreach(var act in m_options) if (act.IsLegal()) return true;
        return false;
      }
      
      public override bool IsPerformable() {
        _real_options = m_options.FindAll(act => act.IsPerformable());
        if (0 >= _real_options.Count) _real_options = null;
        return null != _real_options;
      }

      public override void Perform() {
#if DEBUG
        if (null == _real_options) throw new ArgumentNullException(nameof(_real_options));
#endif
        var act = Rules.Get.DiceRoller.Choose(_real_options); // \todo more sophisticated choice logic
        act.Perform();
       
        var sequel = m_sequel?.Bind(m_Actor);

        if (null != sequel) {
          (m_Actor.Controller as ObjectiveAI).SetObjective(new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter + 1, m_Actor, sequel));
        }
      }


    }
}