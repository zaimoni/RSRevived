using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    public class RangedAttack : ActorAction, CombatAction
    {
        public readonly Location src;
        public readonly Location dest;
        private Location[] LoF;
        public readonly ItemRangedWeapon rw;
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
            if (null == rw) rw = actor.GetEquippedWeapon() as ItemRangedWeapon;
            if (null == rw) return null;
            var test = LOS.AbstractFireLine(src, dest);
            if (null == test) return null;
            if (rw.Model.Attack.Range < test.Length) return null;
            return new RangedAttack(actor, rw, src, dest, mode, test);
        }

        static public RangedAttack? Cast(Actor actor, Location dest, FireMode mode, ItemRangedWeapon? rw = null, Location? origin = null)
        {
            Location src = null == origin ? actor.Location : origin.Value;
            if (null == rw) rw = actor.GetEquippedWeapon() as ItemRangedWeapon;
            if (null == rw) return null;
            var test = LOS.AbstractFireLine(src, dest);
            if (null == test) return null;
            if (rw.Model.Attack.Range < test.Length) return null;
            var ret = new RangedAttack(actor, rw, src, dest, mode, test);
            return ret.IsPerformable() ? ret : null;
        }

        static public void Coverage(Data.Model.CombatActor en, List<Location> dests, FireMode mode, List<ItemRangedWeapon> rws, List<Engine.Actions.CombatAction> catalog)
        {
            Location src = en.Location;
            foreach (var dest in dests) {
                var test = LOS.AbstractFireLine(src, dest);
                if (null == test) continue;
                foreach (var rw in rws) {
                    if (test.Length > rw.Model.Attack.Range) continue;
                    catalog.Add(new RangedAttack(en.who, rw, src, dest, mode, test));
                }
            }
        }

        static public void Coverage(Data.Model.CombatActor en, List<Location> dests, List<ItemRangedWeapon> rws, List<Engine.Actions.CombatAction> catalog, List<KeyValuePair<Engine.Actions.CombatAction, Engine.Actions.CombatAction>> double_attack, List<KeyValuePair<Engine._Action.MoveStep, Engine.Actions.CombatAction>> dash_attack, List<KeyValuePair<Engine.Actions.CombatAction, Engine._Action.MoveStep>> potshot)
        {
            Location src = en.Location;
            foreach (var dest in dests) {
                var test = LOS.AbstractFireLine(src, dest);
                if (null == test) continue;
                foreach (var rw in rws) {
                    if (test.Length > rw.Model.Attack.Range) continue;
                    var aimed = new RangedAttack(en.who, rw, src, dest, FireMode.AIMED, test);
                    catalog.Add(aimed);
                    var rapid = new RangedAttack(en.who, rw, src, dest, FireMode.RAPID, test);
                    double_attack.Add(new(rapid, rapid));
                    double_attack.Add(new(rapid, aimed));
                    var dash = en.RunSteps;
                    if (null == dash) continue;
                    foreach (var move in dash) {
                        potshot.Add(new(rapid, move));
                        var snipe = ScheduleCast(en.who, dest, FireMode.RAPID, rw, move.dest);
                        if (null == snipe) continue;
                        dash_attack.Add(new(move, snipe));
                    }
                }
            }
        }

        public override string ToString() {
            return FMode.ToString()+" "+rw.ToString() +" from "+src.ToString()+" to "+dest.ToString();
        }
    }
}
