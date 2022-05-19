using System;
using System.Collections.Generic;
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
        readonly bool is_running;

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
        public override ActorAction? Bind(Actor src) {
            if (!_Action.MoveStep._CanConstruct(m_Origin, m_NewLocation, is_running, src)) return null;
            _Action.MoveStep ret = new(m_Origin, m_NewLocation, is_running, src);
            return ret.IsPerformable() ? ret : null;
        }

        public override void Blacklist(HashSet<Location> goals) { }
        public override void Goals(HashSet<Location> goals) { goals.Add(m_Origin); }

        public static KeyValuePair<List<MoveStep>, HashSet<Location>>? outbound(Location origin, Actor who, Predicate<Location> ok, bool run)
        {
            List<MoveStep> ret = new();
            HashSet<Location> next = new();

            origin.ForEachAdjacent(dest => {
                if (ok(dest) && _Action.MoveStep._CanConstruct(origin, dest, true, who)) {
                    next.Add(dest);
                    ret.Add(new MoveStep(origin, dest, run));
                }
            });

            return (0 < ret.Count) ? new(ret, next) : null;
        }

        public static KeyValuePair<List<MoveStep>, HashSet<Location>>? inbound(Actor who, Location dest, Predicate<Location> ok, bool run)
        {
            List<MoveStep> ret = new();
            HashSet<Location> next = new();

            dest.ForEachAdjacent(origin => {
                if (ok(origin) && _Action.MoveStep._CanConstruct(origin, dest, true, who))
                {
                    next.Add(origin);
                    ret.Add(new MoveStep(origin, dest, run));
                }
            });

            return (0 < ret.Count) ? new(ret, next) : null;
        }
    }
}

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    internal class MoveStep : ActorAction, ActorDest, ActorOrigin
    {
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;
        readonly bool is_running;

        public Location dest { get { return m_NewLocation; } }  // of m_Actor
        public Location origin { get { return m_Origin; } }

        static public bool _CanConstruct(Location from, Location to, bool run, Actor actor) {
#if DEBUG
            if (1 != Rules.InteractionDistance(to, from)) return false;
#endif
            if (!actor.CanEnter(from)) return false;
            if (!actor.CanEnter(to)) return false;
            if (run && !actor.Model.Abilities.CanRun) return false;
            return true;
        }

        public MoveStep(Location from, Location to, bool run, Actor actor) : base(actor)
        {
            m_NewLocation = to;
            m_Origin = from;
            is_running = run;
#if DEBUG
            if (1 != Rules.InteractionDistance(m_NewLocation, m_Origin)) throw new InvalidOperationException("move delta must be adjacent");
            if (!m_Actor.CanEnter(m_Origin)) throw new InvalidOperationException("must be able to exist at the origin");
            if (!m_Actor.CanEnter(m_NewLocation)) throw new InvalidOperationException("must be able to exist at the destination");
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
    }
}
