using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine._Action
{
    internal class SwitchPlace : ActorAction, ActorDest, ActorOrigin, IsPerformableUsesUI
    {
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;
        [NonSerialized] private bool m_Live = false;

        private SwitchPlace(Actor actor, Location newLocation, Location origin) : base(actor)
        {
            m_NewLocation = newLocation;
            m_Origin = origin;
        }

        public Location dest => m_NewLocation;

        public Location origin => m_Origin;

        public override bool IsLegal() => true;

        public override bool IsPerformable()
        {
            if (m_Actor.Location != m_Origin) {
                m_FailReason = "not at origin";
                return false;
            }
            var target = m_NewLocation.Actor;
            if (target is null) {
                m_FailReason = "no one there";
                return false;
            }
            if (m_Actor.IsEnemyOf(target)) {
                m_FailReason = "is enemy";
                return false;
            }
            if (target.IsSleeping) {
                m_FailReason = "sleeping";
                return false;
            }
            if (!target.Model.CanEnter(m_Origin)) {
                m_FailReason = "does not fit";
                return false;
            }
            if (target.Controller is not Gameplay.AI.ObjectiveAI) {
                m_FailReason = "not cooperative enough to switch";
                return false;
            }
            if (target.Controller is PlayerController pc) {
                if (!m_Live) return true;
                if (RogueGame.Game.YesNoPopup("Switch places with " + target.Name)) return true;
                m_FailReason = "declined by target";
                return false;
            }
            var who = target as LivingActor;
            // legacy ActionSwitchPlace requires leader to initiate
            if (who is not null && who.LiveLeader == m_Actor) return true;
            // legacy ActionSwitchPlaceEmergency can be rejected by AI
            if (target.Controller is Gameplay.AI.OrderableAI t_ordai) {
                if (!t_ordai.RejectSwitchPlaces(m_Actor.Location)) return true;
                m_FailReason = "declined by target";
                return false;
            }
            return true;
        }

        public override void Perform()
        {
#if DEBUG
            if (!m_Live && !RequiresUI()) throw new InvalidOperationException("only planning-mode performability checks");
#endif
            RogueGame.Game.DoSwitchPlace(m_Actor, m_NewLocation.Actor ?? throw new ArgumentNullException("m_NewLocation.Actor"));
        }

        public void EnableUI() => m_Live = true;

        public bool RequiresUI()
        {
            var who = m_NewLocation.Actor;
            return who is not null && who.IsPlayer;
        }

        public bool LeaderSwitchPlace { get {
           var who = m_NewLocation.Actor as LivingActor;
           return who is not null && who.LiveLeader == m_Actor;
        } }

        public static SwitchPlace? schedule(Actor actor, Location newLocation, Location origin) {
            if (1 != Rules.InteractionDistance(newLocation, origin)) return null;
            if (!actor.Model.CanEnter(newLocation)) return null;
            if (!actor.Model.CanEnter(origin)) return null;
            return new(actor, newLocation, origin);
        }

        public static SwitchPlace? schedule(Actor actor, Actor target)
        {
            if (actor.IsEnemyOf(target)) return null;
            if (target.IsSleeping) return null;
            if (!target.Model.CanEnter(actor.Location)) return null;
            if (target.Controller is not Gameplay.AI.ObjectiveAI) return null;
            return schedule(actor, target.Location, actor.Location);
        }

        public static SwitchPlace? create(Actor actor, Actor target)
        {
            var ret = schedule(actor, target);
            return (ret is null || !ret.IsPerformable()) ? null : ret;
        }
    }
}
