#define B_MOVIE_MARTIAL_ARTS

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace djack.RogueSurvivor.Engine._Action
{
    public class MeleeAttack : ActorAction, CombatAction
    {
        private Location src;
        private Location dest;

        private MeleeAttack(Actor actor, Location origin, Location target) : base(actor)
        {
            src = origin;
            dest = target;
        }

        public Actor? target { get => dest.Actor; }

        public override bool IsLegal() => true;

        public override bool IsPerformable() {
//          if (!IsLegal()) return false;
            if (src != m_Actor.Location) return false;

            var victim = target;
            if (null == victim) return false;
            if (!m_Actor.IsEnemyOf(victim)) return false;

            return CanMeleeAttack(victim);
        }

        public override void Perform()
        {
            var melee = m_Actor.GetBestMeleeWeapon();
            if (null != melee) {
                if (!melee.IsEquipped) melee.EquippedBy(m_Actor); // relies on equipping being a free action
            } else {
                if (m_Actor.GetEquippedWeapon() is ItemMeleeWeapon tmp_melee) tmp_melee.UnequippedBy(m_Actor);
            }

            RogueGame.Game.DoMeleeAttack(m_Actor, target);
        }

        private string ReasonCantMeleeAttack(Actor target)
        {
            if (m_Actor.StaminaPoints < Actor.STAMINA_MIN_FOR_ACTIVITY) return "not enough stamina to attack";
            if (target.IsDead) return "already dead!";
            return "";
        }

        public bool CanMeleeAttack(Actor target, out string reason)
        {
            reason = ReasonCantMeleeAttack(target);
            return string.IsNullOrEmpty(reason);
        }

        public bool CanMeleeAttack(Actor target)
        {
            return string.IsNullOrEmpty(ReasonCantMeleeAttack(target));
        }

        static public MeleeAttack? ScheduleCast(Actor actor, Location dest, FireMode mode, ItemMeleeWeapon? melee = null, Location? origin = null)
        {
            Location src = null == origin ? actor.Location : origin.Value;

#if B_MOVIE_MARTIAL_ARTS
            bool in_range = Rules.IsAdjacent(src, dest);
            // even martial arts 1 unlocks extended range.
            if (!in_range && 0 < actor.UsingPolearmInBMovie && 2 == Rules.GridDistance(src, dest)) in_range = true;
            if (!in_range) return null;
#else
            if (!Rules.IsAdjacent(origin, dest)) return null;
#endif

            return new MeleeAttack(actor, src, dest);
        }

        static public MeleeAttack? Cast(Actor actor, Location dest, Location? origin = null)
        {
            Location src = null == origin ? actor.Location : origin.Value;
            var ret = new MeleeAttack(actor, src, dest);
            return ret.IsPerformable() ? ret : null;
        }
    }
}
