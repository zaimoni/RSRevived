using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Zaimoni.Data;
using Zaimoni.JsonConvertIncomplete;

namespace djack.RogueSurvivor.Data.Model
{
    public class CombatActor : ILocation
    {
        public readonly Actor who;
        private int m_HitPoints;
        private int m_StaminaPoints;
        private int m_SleepPoints;
        private Location m_Location;
        private int m_ActionPoints;
        private WorldTime time;
        private sbyte _recoil = 0;

        // cache fields
        private readonly List<Engine._Action.MoveStep>? run_steps;
        private readonly ZoneLoc runaway;
        private readonly List<Engine.Items.ItemRangedWeapon>? ready_rw;
#if PROTOTYPE
        private readonly DenormalizedProbability<int> defense_dist;
        private readonly KeyValuePair<DenormalizedProbability<int>,
                         KeyValuePair<DenormalizedProbability<int>, DenormalizedProbability<int>?>> melee_dist;
#endif

        public sbyte Recoil { get => _recoil; }
        public IEnumerable<Engine._Action.MoveStep> RunSteps => run_steps;

        public CombatActor(Actor src) {
            who = src;
            m_HitPoints = src.HitPoints;
            m_StaminaPoints = src.StaminaPoints;
            m_Location = src.Location;
            m_ActionPoints = src.ActionPoints;
            time = new(src.Location.Map.LocalTime); // must be value copy
            _recoil = (who.Controller as Gameplay.AI.ObjectiveAI)?.Recoil ?? 0;

            normalizeAP();

            run_steps = Engine._Action.MoveStep.RunFrom(src.Location, src);
            runaway = m_Location.LinfCircle(RunIsFreeMove ? 2 : 1);
            ready_rw = who.ReadyRangedWeapons();
#if PROTOTYPE
            defense_dist = Rules.SkillProbabilityDistribution(who.Defence.Value);
            var melee_attack = who.BestMeleeAttack();
            var m_a_dist = Rules.SkillProbabilityDistribution(melee_attack.HitValue);
            var m_dam_dist = Rules.SkillProbabilityDistribution(melee_attack.DamageValue);
            var necro_dam = who.DamageBonusVsUndeads;
            var m_dam_undead_dist = (0!=necro_dam ? Rules.SkillProbabilityDistribution(melee_attack.DamageValue+ necro_dam) : m_dam_dist);
            melee_dist = new(m_a_dist, new(m_dam_dist, m_dam_undead_dist));
#endif
        }

        public Location Location
        {
            get => m_Location;
            set { m_Location = value; }
        }

        // closely related to Actor::PreTurnStart()
        private void normalizeAP() {
            while (0 >= m_ActionPoints) {
                m_ActionPoints += who.Speed;
                if (m_StaminaPoints < MaxSTA) RegenStaminaPoints(Actor.STAMINA_REGEN_WAIT);
                Drowse(Location.Map.LocalTime.IsNight ? 2 : 1);
                time.TurnCounter++;
            }
        }

        private bool RapidRecoilOk(Engine.Items.ItemRangedWeapon rw) {
            var test = rw.Model.Attack;
            return 4 >= test.HitValue - test.Hit3Value;
        }

        public KeyValuePair<List<Engine.Actions.CombatAction>,
               KeyValuePair<List<KeyValuePair<Engine.Actions.CombatAction, Engine.Actions.CombatAction>>,
                            List<KeyValuePair<Engine._Action.MoveStep, Engine.Actions.CombatAction>>>> DamageField(List<Data.Model.CombatActor> others) {
            List<Engine.Actions.CombatAction> direct = new();
            List<KeyValuePair<Engine.Actions.CombatAction, Engine.Actions.CombatAction>> double_attack = new();
            List<KeyValuePair<Engine._Action.MoveStep, Engine.Actions.CombatAction>> dash_attack = new();
            List<KeyValuePair<Engine.Actions.CombatAction, Engine._Action.MoveStep>> potshot = new();

            var domain = runaway.Listing;

            foreach (var en in others) {
                if (!who.IsEnemyOf(en.who)) continue;
                if (null != en.ready_rw) {
                    if (en.NextMoveLostWithoutRunOrWait) {
                        Engine._Action.RangedAttack.Coverage(en, domain, Engine.Actions.FireMode.RAPID, ready_rw, direct);
                    } else if (en.RunIsFreeMove) {
                        Engine._Action.RangedAttack.Coverage(en, domain, en.ready_rw, direct, double_attack, dash_attack, potshot);
                    } else {
                        Engine._Action.RangedAttack.Coverage(en, domain, (0==en.Recoil ? Engine.Actions.FireMode.RAPID : Engine.Actions.FireMode.AIMED), en.ready_rw, direct);
                    }
                }
                Engine._Action.MeleeAttack.Coverage(en, domain, direct);
                if (en.RunIsFreeMove) {
                    Engine._Action.MeleeAttack.Coverage(en, domain, dash_attack);
                }
            }
            // potshot does not actually add to the damage field
            return new(direct, new(double_attack, dash_attack));
        }

        public KeyValuePair<List<Engine.Actions.CombatAction>,
               KeyValuePair<List<KeyValuePair<Engine.Actions.CombatAction, Engine.Actions.CombatAction>>,
               KeyValuePair<List<KeyValuePair<Engine._Action.MoveStep, Engine.Actions.CombatAction>>,
                            List<KeyValuePair<Engine.Actions.CombatAction, Engine._Action.MoveStep>>>>> AttackField(List<Data.Model.CombatActor> others) {
            List<Engine.Actions.CombatAction> direct = new();
            List<KeyValuePair<Engine.Actions.CombatAction, Engine.Actions.CombatAction>> double_attack = new();
            List<KeyValuePair<Engine._Action.MoveStep, Engine.Actions.CombatAction>> dash_attack = new();
            List<KeyValuePair<Engine.Actions.CombatAction, Engine._Action.MoveStep>> potshot = new();

            List<Location> domain = new();
            foreach (var en in others) {
                if (!who.IsEnemyOf(en.who)) continue;
                domain.Add(en.Location);
            }

            if (null != ready_rw) {
               if (NextMoveLostWithoutRunOrWait) {
                 Engine._Action.RangedAttack.Coverage(this, domain, Engine.Actions.FireMode.RAPID, ready_rw, direct);
               } else if (RunIsFreeMove) {
                 Engine._Action.RangedAttack.Coverage(this, domain, ready_rw, direct, double_attack, dash_attack, potshot);
               } else {
                 Engine._Action.RangedAttack.Coverage(this, domain, (0==Recoil ? Engine.Actions.FireMode.RAPID : Engine.Actions.FireMode.AIMED), ready_rw, direct);
               }
            }

            int i = double_attack.Count;
            while (0 <= --i) {
                var x = double_attack[i];
                var rw = (x.Key as Engine._Action.RangedAttack)?.rw;
                if (null == rw) continue;
                if (RapidRecoilOk(rw)) continue;
                if (1<Recoil || FireMode.RAPID == (x.Value as Engine._Action.RangedAttack)!.FMode) {
                    double_attack.RemoveAt(i);
                }
            }

            Engine._Action.MeleeAttack.Coverage(this, domain, direct);
            if (RunIsFreeMove) {
                Engine._Action.MeleeAttack.Coverage(this, domain, dash_attack);
            }

            return new(direct, new(double_attack, new(dash_attack, potshot)));
        }

        private void Drowse(int s) => m_SleepPoints = Math.Max(0, m_SleepPoints - s);

        public int TurnsUntilSleepy {
            get {
                int num = m_SleepPoints - Actor.SLEEP_SLEEPY_LEVEL;
                if (num <= 0) return 0;
                WorldTime now = new WorldTime(Location.Map.LocalTime);
                int turns = 0;
                while (0 < num) {
                    int delta_t = WorldTime.TURNS_PER_HOUR - now.Tick;
                    int awake_cost = (now.IsNight ? 2 : 1);
                    int SLP_cost = awake_cost * delta_t;
                    if (SLP_cost > num) {
                        turns += num / awake_cost;
                        break;
                    }
                    num -= SLP_cost;
                    turns += delta_t;
                    now.TurnCounter += delta_t;
                }
                return turns;
            }
        }


        public int HoursUntilSleepy { get => TurnsUntilSleepy / WorldTime.TURNS_PER_HOUR; }
        public bool IsAlmostSleepy { get => who.Model.Abilities.HasToSleep && 3 >= HoursUntilSleepy; }
        public bool IsSleepy { get => who.Model.Abilities.HasToSleep && Actor.SLEEP_SLEEPY_LEVEL >= m_SleepPoints; }
        public bool IsExhausted { get => who.Model.Abilities.HasToSleep && 0 >= m_SleepPoints; }

        public bool WouldLikeToSleep { get {
          return IsAlmostSleepy /* || IsSleepy */;    // cf above partial ordering
        } }

        public int MaxSleep { get {
          int num = (int)(/* (double) */ Actor.SKILL_AWAKE_SLEEP_BONUS * /* (int) */ (who.Model.BaseSleepPoints * who.MySkills.GetSkillLevel(Skills.IDs.AWAKE)));
          return who.Model.BaseSleepPoints + num;
        } }

        private void RegenStaminaPoints(int staminaRegen)
        {
            m_StaminaPoints = who.Model.Abilities.CanTire ? Math.Min(MaxSTA, m_StaminaPoints + staminaRegen) : Actor.STAMINA_INFINITE;
        }

        public int NightSTApenalty { get {
            if (!Location.Map.LocalTime.IsNight) return 0;
            if (who.Model.Abilities.IsUndead) return 0;
            return Actor.NIGHT_STA_PENALTY;
        } }
        public bool WillTireAfter(int staminaCost)
        {
            if (!who.Model.Abilities.CanTire) return false;
            if (0 < staminaCost) staminaCost += NightSTApenalty;
            if (IsExhausted) staminaCost *= 2;
            return Actor.STAMINA_MIN_FOR_ACTIVITY > m_StaminaPoints - staminaCost;
        }

        public int RunningStaminaCost(in Location dest)
        {
            if (Location.RequiresJump(in dest)) return Rules.STAMINA_COST_RUNNING + Rules.STAMINA_COST_JUMP + NightSTApenalty;
            return Rules.STAMINA_COST_RUNNING + NightSTApenalty;
        }

        public int WalkingStaminaCost(in Location dest)
        {
            if (Location.RequiresJump(in dest)) return Rules.STAMINA_COST_JUMP + NightSTApenalty;
            return 0;
        }

        public int MaxSTA
        {
            get => who.Model.BaseStaminaPoints + Actor.SKILL_HIGH_STAMINA_STA_BONUS * who.MySkills.GetSkillLevel(Gameplay.Skills.IDs.HIGH_STAMINA);
        }

        public bool IsTired
        {
            get => who.Model.Abilities.CanTire && m_StaminaPoints < Actor.STAMINA_MIN_FOR_ACTIVITY;
        }


        public bool RunIsFreeMove { get => Actor.BASE_ACTION_COST / 2 < m_ActionPoints; }
        public bool WalkIsFreeMove { get => Actor.BASE_ACTION_COST < m_ActionPoints; }
        public int EnergyDrain { get => Actor.BASE_ACTION_COST - who.Model.DollBody.Speed; }
        public bool NextMoveLostWithoutRunOrWait { get => EnergyDrain >= m_ActionPoints; }

        public static int CompareAP(CombatActor lhs, CombatActor rhs) {
            var code = lhs.time.TurnCounter.CompareTo(rhs.time.TurnCounter);
            if (code != 0) return code;
            if (lhs.who.Location.Map != rhs.who.Location.Map) return District.IsBefore(lhs.who.Location.Map, rhs.who.Location.Map) ? -1 : 1;
            foreach (var a in lhs.Location.Map.Actors) {
                if (a == lhs.who) return -1;
                if (a == rhs.who) return 1;
            }
            return lhs.m_ActionPoints.CompareTo(rhs.m_ActionPoints);
        }

        public override string ToString() { return who.ToString() + "@" + m_Location.ToString()
          + "; " + time.TurnCounter.ToString() + ":" + m_ActionPoints.ToString() + "; "
          + m_HitPoints.ToString() + "/" + m_StaminaPoints.ToString() + "/" + _recoil.ToString()
          + "\nrun is free move: " + (RunIsFreeMove ? "true" : "false")
          + "\nflight bounds: " + runaway.ToString()
          + "\nrw: " + (null == ready_rw ? "null" : ready_rw.to_s())
          + "\n" + (null == run_steps ? "null" : run_steps.to_s()); }

    }
}

