using System;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;

#nullable enable

namespace djack.RogueSurvivor.Engine.Goal
{
    [Serializable]
    internal class NextAction : Objective
    {
        private readonly ActorAction? Intent_engaged;
        private readonly ActorAction? Intent_disengaged;
        private readonly WorldUpdate? Plan_engaged;
        private readonly WorldUpdate? Plan_disengaged;

        public NextAction(Actor who, ActorAction engaged) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (null == engaged) throw new ArgumentNullException(nameof(engaged));
#endif
            Intent_engaged = engaged;
        }

        public NextAction(Actor who, ActorAction? engaged, ActorAction? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (null == engaged && null == disengaged) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Intent_engaged = engaged;
            Intent_disengaged = disengaged;
        }

        public NextAction(Actor who, ActorAction? engaged, WorldUpdate? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (null == engaged && null == disengaged) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Intent_engaged = engaged;
            Plan_disengaged = disengaged;
        }

        public NextAction(Actor who, WorldUpdate engaged) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (null == engaged) throw new ArgumentNullException(nameof(engaged));
#endif
            Plan_engaged = engaged;
        }

        public NextAction(Actor who, WorldUpdate? engaged, ActorAction? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (null == engaged && null == disengaged) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Plan_engaged = engaged;
            Intent_disengaged = disengaged;
        }

        public NextAction(Actor who, WorldUpdate? engaged, WorldUpdate? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (null == engaged && null == disengaged) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Plan_engaged = engaged;
            Plan_disengaged = disengaged;
        }

        // always execute.  Expire on execution
        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            _isExpired = true;
            if (null == m_Actor.Controller.enemies_in_FOV) {
                ret = _intentDisengaged();
            } else {
                ret = _intentEngaged();
            }
            return true;
        }

        private ActorAction? _intentEngaged() {
            if (null != Intent_engaged && Intent_engaged.IsPerformable()) return Intent_engaged;
            if (null != Plan_engaged) return Plan_engaged.Bind(m_Actor);
            return null;
        }

        private ActorAction? _intentDisengaged()
        {
            if (null != Intent_disengaged && Intent_disengaged.IsPerformable()) return Intent_disengaged;
            if (null != Plan_disengaged) return Plan_disengaged.Bind(m_Actor);
            return null;
        }
    }
}
