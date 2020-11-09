using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class PushOnto : WorldUpdate, Actions.ObjectOrigin, Actions.ObjectDest
    {
        private readonly Location m_NewLocation;
        private readonly Location m_From;
        private readonly int m_MaxWeight;
        [NonSerialized] private Location[]? m_origin;

        public Location obj_origin { get { return m_From; } }
        public Location obj_dest { get { return m_NewLocation; } }

        public PushOnto(Location from, Location to, int maxWeight=10)
        {
#if DEBUG
            if (1 != Rules.InteractionDistance(in from, in to)) throw new InvalidOperationException("move delta must be adjacent");
#endif
            if (!Map.CanEnter(ref from)) throw new InvalidOperationException("must be able to exist at the origin");
            if (!Map.CanEnter(ref to)) throw new InvalidOperationException("must be able to exist at the destination");
            m_NewLocation = to;
            m_From = from;
            m_MaxWeight = maxWeight; // default to ignoring cars as push targets
        }

        public override bool IsLegal() {
            var obj = m_From.MapObject;
            if (null != obj) {
                if (!obj.IsMovable) return false;
                if (obj.IsOnFire) return false;
                if (m_MaxWeight < obj.Weight) return false;
            }
            obj = m_NewLocation.MapObject;
            if (null != obj) {
                if (!obj.IsMovable) return false;
                if (obj.IsOnFire) return false;
                if (m_MaxWeight < obj.Weight) return false;
            }
            return true;
        }
        public override bool IsRelevant() {
            if (null != m_NewLocation.MapObject) return false;
            var obj = m_From.MapObject;
            return null != obj && obj.IsMovable && !obj.IsOnFire;
        }
        public override bool IsRelevant(Location loc) {
            return IsRelevant() && 1 == Rules.GridDistance(m_From, loc);
        }
        public override ActorAction? Bind(Actor src) {
            var act = new _Action.PushOnto(src, m_From, m_NewLocation);
            return act.IsPerformable() ? act : null;
        }

        public override void Blacklist(HashSet<Location> goals) {
            goals.Remove(m_From);  // don't want actor here
        }

        public override void Goals(HashSet<Location> goals) {
            goals.UnionWith(m_origin ??= origin_range);
        }

        private Location[] origin_range {
            get {
                var ret = new List<Location>();
                foreach (var pt in m_From.Position.Adjacent()) {
                    var test = new Location(m_From.Map, pt);
                    if (Map.Canonical(ref test) && test.TileModel.IsWalkable) ret.Add(test);
                }
                return ret.ToArray();
            }
        }

    }
}

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class PushOnto : ActorAction,Actions.ObjectOrigin,Actions.ObjectDest
    {
        private readonly MapObject m_Object;
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;
        [NonSerialized] List<Location>? _dests = null;

        public Location obj_origin { get { return m_Origin; } }
        public Location obj_dest { get { return m_NewLocation; } }

        public PushOnto(Actor actor, Location from, Location to, MapObject? obj = null) : base(actor)
        {
#if DEBUG
            if (1 != Rules.InteractionDistance(in from, in to)) throw new InvalidOperationException("move delta must be adjacent");
#endif
            if (!actor.CanEnter(ref from)) throw new InvalidOperationException("must be able to exist at the origin");
            if (!actor.CanEnter(ref to)) throw new InvalidOperationException("must be able to exist at the destination");
            if (null == obj) obj = from.MapObject;
            if (null == obj) throw new ArgumentNullException(nameof(obj));
            m_NewLocation = to;
            m_Origin = from;
            m_Object = obj;
        }

        public override bool IsLegal()
        {
            if (m_Object.Location != m_Origin) return false;
            return true;
        }

        public override bool IsPerformable() {
            if (!IsLegal()) return false;
            if (null != m_NewLocation.MapObject) return false;
            if (1 != Rules.GridDistance(m_Actor.Location, m_Object.Location)) return false;
            if (!m_Actor.CanPush(m_Object, out m_FailReason)) return false;
            if (m_Actor.Location == m_NewLocation) { // pull
                var dests = new List<Location>();
                foreach (var pt in m_NewLocation.Position.Adjacent()) {
                    var loc = new Location(m_NewLocation.Map, pt);
                    if (!m_Actor.CanEnter(ref loc)) continue;
                    if (loc == m_Origin) continue;
                    if (!m_Actor.CanPull(m_Object, loc)) continue;
                    dests.Add(loc);
                }
                if (0 >= dests.Count) return false;
                _dests = dests;
                return true;
            } else { // push
                if (!m_Object.CanPushTo(in m_NewLocation)) return false;
            }
            return true;
        }

        public override void Perform()
        {
            if (m_Actor.Location == m_NewLocation) { // pull
                RogueForm.Game.DoPull(m_Actor, m_Object, Rules.Get.DiceRoller.Choose(_dests));
            } else { // push
                RogueForm.Game.DoPush(m_Actor, m_Object, in m_NewLocation);
            }
        }
    }
}
