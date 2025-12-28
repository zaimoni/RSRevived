using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    internal class MoveStep : WorldUpdate, ActorDest, ActorOrigin
    {
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;
        private readonly bool is_running;

        public Location dest { get { return m_NewLocation; } }  // of m_Actor
        public Location origin { get { return m_Origin; } }

        public MoveStep(Location from, Location to, bool run)
        {
            m_NewLocation = to;
            m_Origin = from;
            is_running = run;
#if DEBUG
            if (1 != Rules.InteractionDistance(m_NewLocation, m_Origin)) throw new InvalidOperationException("move delta must be adjacent");
#endif
        }

        public override bool IsLegal() { return true; }
        public override bool IsRelevant() { return false; }
        public override bool IsRelevant(Location loc) {
            return loc != m_Origin;
        }
        public override bool IsSuppressed(Actor a) { return false; }

        /// <returns>null, or a Performable action</returns>
        public override _Action.MoveStep? Bind(Actor src) {
            return _Action.MoveStep.Cast(m_Origin, m_NewLocation, is_running, src);
        }

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = Bind(src);
            if (null == act) return null;
            return new(act, null);
        }

        public override void Blacklist(HashSet<Location> goals) { }
        public override void Goals(HashSet<Location> goals) { goals.Add(m_Origin); }

        public static List<KeyValuePair<Location, MoveStep>>? outbound(Location origin, Actor who, Predicate<Location> ok, bool run)
        {
            List<KeyValuePair<Location, MoveStep>> ret = new();

            origin.ForEachAdjacent(dest => {
                if (ok(dest)) {
                    var move = ScheduleCast(origin, dest, run, who);
                    if (null != move) ret.Add(new(dest, move));
                }
            });

            return (0 < ret.Count) ? ret : null;
        }

        static public MoveStep? ScheduleCast(Location from, Location to, bool run, Actor actor) {
            if (1 != Rules.InteractionDistance(in from, in to)) return null;
            if (!actor.CanEnter(from)) return null;
            if (!actor.CanEnter(to)) return null;
            if (run && !actor.Model.Abilities.CanRun) return null;
            return new MoveStep(from, to, run);
        }


    }
}

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    public class MoveStep : ActorAction, ActorDest, ActorOrigin
    {
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;
        public readonly bool is_running;

        public Location dest { get { return m_NewLocation; } }  // of m_Actor
        public Location origin { get { return m_Origin; } }

        [Conditional("DEBUG")]
        static public void _Ok(in Location origin, in Location dest, Actor actor) {
            if (1 != Rules.InteractionDistance(in dest, in origin)) throw new InvalidOperationException("move delta must be adjacent");
            if (!actor.CanEnter(dest)) throw new InvalidOperationException("must be able to exist at the destination");
        }

        private MoveStep(Location from, Location to, bool run, Actor actor) : base(actor)
        {
            m_NewLocation = to;
            m_Origin = from;
            is_running = run;
#if DEBUG
            if (!m_Actor.CanEnter(m_Origin)) throw new InvalidOperationException("must be able to exist at the origin");
#endif
        }

        public override bool IsLegal() { return true; }

        public override bool IsPerformable() {
            if (m_Actor.Location != m_Origin) return false;
            if (is_running && !m_Actor.CanRun()) return false;
            return m_NewLocation.IsWalkableFor(m_Actor, out m_FailReason);
        }

        public override void Perform() {
            if (is_running) m_Actor.Run();
            else m_Actor.Walk();

            if (m_Actor.Location.Map==m_NewLocation.Map) RogueGame.Game.DoMoveActor(m_Actor, in m_NewLocation);
            else if (m_Actor.Location.Map.DistrictPos!=m_NewLocation.Map.DistrictPos) {
                var test = m_Actor.Location.Map.Denormalize(in m_NewLocation);
                RogueGame.Game.DoLeaveMap(m_Actor, test.Value.Position);
            } else RogueGame.Game.DoLeaveMap(m_Actor, m_Actor.Location.Position);
        }

        static public MoveStep? ScheduleCast(Location from, Location to, bool run, Actor actor) {
            if (1 != Rules.InteractionDistance(in from, in to)) return null;
            if (!actor.CanEnter(to)) return null;
            if (!actor.CanEnter(from)) return null;
            if (run && !actor.Model.Abilities.CanRun) return null;
            return new MoveStep(from, to, run, actor);
        }

        static public MoveStep? Cast(Location from, Location to, bool run, Actor actor)
        {
            var ret = ScheduleCast(from, to, run, actor);
            if (null != ret && ret.IsPerformable()) return ret;
            return null;
        }

        static public List<MoveStep>? RunFrom(Location from, Actor actor) {
            if (!actor.CanEnter(from)) return null;
            if (!actor.Model.Abilities.CanRun) return null;

            List<MoveStep> ret = new();
            foreach (var dir in Direction.COMPASS) {
                var to = from + dir;
                if (!actor.CanEnter(ref to)) continue;
                var move = new MoveStep(from, to, true, actor);
                if (!move.IsPerformable()) continue;
                ret.Add(move);
            }
            return 0<ret.Count ? ret : null;
        }

        public override string ToString()
        {
            return (is_running ? "run" : "walk")+" from " + origin.ToString() + " to " + dest.ToString();
        }
    }
}
    