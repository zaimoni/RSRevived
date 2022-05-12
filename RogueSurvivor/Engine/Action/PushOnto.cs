using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class PushOnto : WorldUpdate, Actions.ObjectOrigin, Actions.ObjectDest, CanFinish
    {
        private readonly Location m_NewLocation;
        private readonly Location m_From;
        private readonly int m_ObjCode; // what we want on the destination
        [NonSerialized] private Location[]? m_origin;

        public Location obj_origin { get { return m_From; } }
        public Location obj_dest { get { return m_NewLocation; } }

        public PushOnto(Location from, Location to, int code)
        {
#if DEBUG
            if (1 != Rules.InteractionDistance(in from, in to)) throw new InvalidOperationException("move delta must be adjacent");
#endif
            if (!Map.CanEnter(ref from)) throw new InvalidOperationException("must be able to exist at the origin");
            if (!Map.CanEnter(ref to)) throw new InvalidOperationException("must be able to exist at the destination");
            m_NewLocation = to;
            m_From = from;
            m_ObjCode = code;
        }

        // disallow cars for this
        public static bool PushDestOk(in Location loc) {
            var obj = loc.MapObject;
            if (null != obj) {
                if (!obj.IsMovable) return false;
                if (obj.IsOnFire) return false;
                if (MapObject.MAX_NORMAL_WEIGHT < obj.Weight) return false;
            }
            return true;
        }

        public override bool IsLegal() {
            var obj = m_From.MapObject;
            if (null != obj) {
                if (!obj.IsMovable) return false;
                if (obj.IsOnFire) return false;
                return DisarmOk(obj, m_ObjCode);
            }
            obj = m_NewLocation.MapObject;
            if (null != obj) {
                if (!obj.IsMovable) return false;
                if (obj.IsOnFire) return false;
                if (0 == (4 & m_ObjCode) && MapObject.MAX_NORMAL_WEIGHT < obj.Weight) return false;
            }
            return true;
        }

        // should be flag enum return value
        // 0: do not disarm
        // 1: typical jumpable i.e. trap-covering object [formally includes direct-constructing a small fortification]
        // 2: typical non-jumpable i.e. destroying object (but need recovery code for when it doesn't destroy)
        // 4: cars and other trap-covering zombie-proof map objects
        // 8: bed (trap-covering)
        // 16: only if pathfinding
        static public int DisarmWith(in Location loc)
        {
            var e = loc.Exit;
            if (null != e) {
                var exit_map = e.Location.Map;
                var exit_district = exit_map.District;
                if (exit_map == exit_district.SewersMap) return loc.Map.IsInsideAt(loc.Position) ? 1 + 16 : 4;
                if (exit_map == exit_district.EntryMap || exit_map == exit_district.SubwayMap) return 1;
                var unique_maps = Session.Get.UniqueMaps;
                if (0 < unique_maps.HospitalDepth(exit_map) || 0 < unique_maps.PoliceStationDepth(exit_map)) return 1;
                var map = loc.Map;
                if (0 < unique_maps.HospitalDepth(map) || 0 < unique_maps.PoliceStationDepth(map)) return 1;
                if (map == map.District.EntryMap) return 1 + 16; // typically a basement
            }
            // not an exit.
            return loc.Map.IsInsideAt(loc.Position) ? 9 : 1;
        }

        static public int DisarmWithAlt(in Location loc)
        {
            var e = loc.Exit;
            if (null != e) {
                var unique_maps = Session.Get.UniqueMaps;
                if (0 < unique_maps.HospitalDepth(loc.Map) || 0 < unique_maps.HospitalDepth(e.Location.Map)) return 8;
            }
            // not an exit.
            return 0;
        }

        // unclear whether AI needs to see this
        static private bool DisarmOk(MapObject obj, int code)
        {
#if PROTOTYPE
            if (!obj.IsMovable) return false;
            if (obj.IsOnFire) return false;
#endif
            if (0 != (4 & code)) {
                if (obj.CoversTraps && !obj.IsBreakable) return true;
            } else {
                if (MapObject.MAX_NORMAL_WEIGHT < obj.Weight) return false;
            }
            if (0 == (8 & code) && obj.IsCouch) return false;
            if (0 != (1 & code) && obj.CoversTraps) return true;
            if (0 != (2 & code) && obj.TriggersTraps) return true;
            return false;
        }

        public override bool IsRelevant() {
            if (null != m_NewLocation.MapObject) return false;
            var obj = m_From.MapObject;
            return null != obj && obj.IsMovable && !obj.IsOnFire && DisarmOk(obj, m_ObjCode); // breaking traps is more work to get right
        }

        public override bool IsRelevant(Location loc) {
            return IsRelevant() && 1 == Rules.GridDistance(m_From, loc);
        }

        public override bool IsSuppressed(Actor a)
        {   // \todo more sophisticated non-combat response to enemies in sight
            return null != a.Controller.enemies_in_FOV;
        }

        public override ActorAction? Bind(Actor src) {
            var act = new _Action.PushOnto(src, m_From, m_NewLocation);
            return act.IsPerformable() ? act : null;
        }

        public bool IsCompleted()
        {
            var obj = m_NewLocation.MapObject;
            return DisarmOk(obj, m_ObjCode);
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
                RogueGame.Game.DoPull(m_Actor, m_Object, Rules.Get.DiceRoller.Choose(_dests));
            } else { // push
                RogueGame.Game.DoPush(m_Actor, m_Object, in m_NewLocation);
            }
        }
    }
}
