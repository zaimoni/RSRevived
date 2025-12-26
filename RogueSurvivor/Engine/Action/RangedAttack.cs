using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    public class RangedAttack : ActorAction, CombatAction
    {
        private Location src;
        private Location dest;
        private Location[] LoF;
        private ItemRangedWeapon rw;
        public readonly FireMode FMode;

        private RangedAttack(Actor actor, ItemRangedWeapon _rw, Location origin, Location target, FireMode mode, Location[] line) : base(actor)
        {
            src = origin;
            dest = target;
            LoF = line;
            rw = _rw;
            FMode = mode;
        }

        public Actor? target { get => dest.Actor; }

        public override bool IsLegal() => true;

        public override bool IsPerformable() {
//          if (!IsLegal()) return false;
            if (src != m_Actor.Location) return false;
            if (!m_Actor.IsCarrying(rw)) return false;

            var victim = target;
            if (null == victim) return false;
            if (!m_Actor.IsEnemyOf(victim)) return false;

            return CanFireAt(victim, out m_FailReason);
        }

        public override void Perform() {
            if (!rw.IsEquipped) rw.EquippedBy(m_Actor); // relies on equipping being a free action

            var victim = target!;
            m_Actor.Aggress(victim);
            var oai = m_Actor.Controller as ObjectiveAI;
//          oai.RecordLoF(m_LoF);
            switch (FMode)
            {
                case FireMode.AIMED:
                    m_Actor.SpendActionPoints();
                    RogueGame.Game.DoSingleRangedAttack(m_Actor, victim, LoF, 0);
                    break;
                case FireMode.RAPID:
                    m_Actor.SpendActionPoints(Actor.BASE_ACTION_COST / 2);
                    RogueGame.Game.DoSingleRangedAttack(m_Actor, victim, LoF, oai.Recoil + 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled mode");
            }
            if (!victim.IsDead) oai.RecruitHelp(victim);
            victim.InferEnemy(m_Actor);
        }

        // from m_Actor::ReasonCantFireAt
        private string ReasonCantFireAt(Actor target)
        {
            int dist = LoF.Length;
            var range = rw.Model.Attack.Range;
            if (range < dist) return "out of range";
            if (rw.Ammo <= 0) return "no ammo left";
            int i = dist-1;
            while (0 <= --i) {
                if (LoF[i].CurrentlyBlocksRangedAttacks) return "no line of fire";
            }
            if (target.IsDead) return "already dead!";
            return "";
       }

       private bool CanFireAt(Actor target, out string reason)
       {
            reason = ReasonCantFireAt(target);
            return string.IsNullOrEmpty(reason);
       }

        public bool CanFireAt(Actor target) => string.IsNullOrEmpty(ReasonCantFireAt(target));


        static public RangedAttack? ScheduleCast(Actor actor, Location dest, FireMode mode, ItemRangedWeapon? rw = null, Location? origin = null) {
            Location src = null == origin ? actor.Location : origin.Value;
            var test = LOS.AbstractFireLine(src, dest);
            if (null == test) return null;
            return new RangedAttack(actor, rw, src, dest, mode, test);
        }

        static public RangedAttack? Cast(Actor actor, Location dest, FireMode mode, ItemRangedWeapon? rw = null, Location? origin = null)
        {
            Location src = null == origin ? actor.Location : origin.Value;
            if (null == rw) rw = actor.GetEquippedWeapon() as ItemRangedWeapon;
            if (null == rw) return null;
            var test = LOS.AbstractFireLine(src, dest);
            if (null == test) return null;
            var ret = new RangedAttack(actor, rw, src, dest, mode, test);
            return ret.IsPerformable() ? ret : null;
        }
    }
}
