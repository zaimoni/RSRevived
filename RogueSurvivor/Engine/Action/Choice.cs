using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable

// namespace Action conflicts with C# STL Action<>
namespace djack.RogueSurvivor.Engine._Action
{
    internal class Choice : ActorAction, Resolvable
    {
        private readonly List<ActorAction> m_Options;

        [Conditional("DEBUG")]
        private void _RejectEmptyActions(IEnumerable<ActorAction> src) {
            if (null == src || 0 >= src.Count()) throw new ArgumentNullException(nameof(src));
        }

        public Choice(Actor actor, IEnumerable<ActorAction> src) : base(actor) {
            _RejectEmptyActions(src);
            m_Options = src.ToList();
        }

        public Choice(Actor actor, List<ActorAction> src) : base(actor) {
            _RejectEmptyActions(src);
            m_Options = src;
        }

        public override bool IsLegal() {
            var ub = m_Options.Count;
            while (0 <= --ub) {
                if (m_Options[ub].IsLegal()) return true;
                m_Options.RemoveAt(ub);
            }
            return false;
        }

        public override bool IsPerformable()
        {
            foreach (var act in m_Options) {
                if (act.IsPerformable()) return true;
            }
            return false;
        }

        public override void Perform() => ConcreteAction?.Perform();

        public ActorAction ConcreteAction {
            get {
                var ub = m_Options.Count;
                while (0 <= --ub) {
                    if (m_Options[ub].IsPerformable()) continue;
                    m_Options.RemoveAt(ub);
                }
                ub = m_Options.Count;
                if (0 >= ub) throw new InvalidOperationException("no performable actions to choose from");
                return m_Actor.Controller.Choose(m_Options);
            }
        }

    }
}
