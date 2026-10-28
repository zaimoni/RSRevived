using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine._Action
{
    internal class Fallback : ActorAction, Resolvable, IsPerformableUsesUI
    {
        private readonly List<ActorAction> m_Options;
        [NonSerialized] private int t0 = -1;
        [NonSerialized] private ActorAction? _result = null;

        public Fallback(Actor actor, List<ActorAction> options) : base(actor)
        {
            options.RejectEmptyActions();
#if DEBUG
            if (options[^1] is IsPerformableUsesUI test && test.RequiresUI()) throw new InvalidOperationException("last option of Fallback: RequireUI()");
            foreach (var act in options) {
                if (!act.PerformedBy(actor)) throw new InvalidOperationException(act.ToString()+" not performed by "+actor.ToString());
            }
#endif
            m_Options = options;
        }

        public ActorAction? ConcreteAction => _result ?? (0 < m_Options.Count ? m_Options[0] : null);

        public override bool IsLegal()
        {
            string? stage_fail = null;
            bool ret = false;
            var n = m_Options.Count;
            while (0 <= --n) {
                if (m_Options[n].IsLegal()) {
                    ret = true;
                    continue;
                }
                stage_fail = m_Options[n].FailReason;
                if (_result == m_Options[n]) _result = null;
                m_Options.RemoveAt(n);
            }
            m_FailReason = ret ? null : stage_fail;
            return ret;
        }

        // left-to-right short-circuit, like Lisp or Prolog
        public override bool IsPerformable()
        {
            if (!IsLegal()) return false;
            if (t0 == m_Actor.Location.Map.LocalTime.TurnCounter) return _result is not null;

            int strict_ub = m_Options.Count;
            int n = -1;
            string? stage_fail = null;
            while (strict_ub > ++n) {
                if (m_Options[n].IsPerformable()) {
                    t0 = m_Actor.Location.Map.LocalTime.TurnCounter;
                    _result = m_Options[n];
                    m_FailReason = "";
                    return true;
                }
                stage_fail = m_Options[n].FailReason;
            }
            t0 = m_Actor.Location.Map.LocalTime.TurnCounter;
            _result = null;
            m_FailReason ??= stage_fail;
            return false;
        }

        public override void Perform() {
#if DEBUG
            if (_result is null) throw new InvalidOperationException("_Action.Fallback resolved to null");
#endif
            _result?.Perform();
        }

        public void EnableUI()
        {
            foreach (var act in m_Options) (act as IsPerformableUsesUI)?.EnableUI();
            t0 = -1;
            _result = null;
        }

        public bool RequiresUI()
        {
            foreach (var act in m_Options) {
                if (act is IsPerformableUsesUI test && test.RequiresUI()) return true;
            }
            return false;
        }

    }
}
