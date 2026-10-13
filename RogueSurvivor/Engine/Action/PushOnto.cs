using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using djack.RogueSurvivor.Data;
using Zaimoni.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class PushOnto : WorldUpdate, Actions.ObjectOrigin, Actions.ObjectDest, CanFinish, IJsonStaged<PushOnto>
    {
        private readonly Location m_NewLocation;
        private readonly Location m_From;
        private readonly int m_ObjCode; // what we want on the destination
        [NonSerialized] private Location[]? m_origin;

        public Location obj_origin { get { return m_From; } }
        public Location obj_dest { get { return m_NewLocation; } }

        #region JSON save/load
        private PushOnto(JsonElement src, JsonSerializerOptions options)
        {
            m_From = src.Require("From").Deserialize<Location>(options);
            m_NewLocation = src.Require("To").Deserialize<Location>(options);
            m_ObjCode = src.Require("Code").GetInt32();
        }

        static PushOnto IJsonStaged<PushOnto>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new PushOnto(src, options));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("From");
            JsonSerializer.Serialize(writer, m_From, options);
            writer.WritePropertyName("To");
            JsonSerializer.Serialize(writer, m_NewLocation, options);
            writer.WriteNumber("Code", m_ObjCode);   // a bit field
        }
        #endregion

        private PushOnto(Location from, Location to, int code)
        {
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
                if (exit_map == exit_district.SewersMap) return loc.IsInside ? 1 + 16 : 4;
                if (exit_map == exit_district.EntryMap || exit_map == exit_district.SubwayMap) return 1;
                var unique_maps = Session.Get.UniqueMaps;
                if (0 < unique_maps.HospitalDepth(exit_map) || 0 < unique_maps.PoliceStationDepth(exit_map)) return 1;
                var map = loc.Map;
                if (0 < unique_maps.HospitalDepth(map) || 0 < unique_maps.PoliceStationDepth(map)) return 1;
                if (map == map.District.EntryMap) return 1 + 16; // typically a basement
            }
            // not an exit.
            return loc.IsInside ? 9 : 1;
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

        public override ActorAction? Bind(Actor src) => _Action.PushOnto.create(src, m_From, m_NewLocation);

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = Bind(src);
            if (null == act) return null;
            return new(act, null);
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

        // \todo fix this and users so that return value is IsLegal()
        static public PushOnto? schedule(Location from, Location to, int code) {
            if (1 != Rules.InteractionDistance(in from, in to)) return null;
            if (!Map.CanEnter(ref from)) return null;
            if (!Map.CanEnter(ref to)) return null;
            if (!to.TileModel.IsWalkable) return null;

            return new PushOnto(from, to, code);
        }
    }
}

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class PushOnto : ActorAction,Actions.ObjectOrigin,Actions.ObjectDest, IJsonStaged<PushOnto>
    {
        private readonly MapObject m_Object;
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;
        [NonSerialized] List<Location>? _dests = null;

        public Location obj_origin { get { return m_Origin; } }
        public Location obj_dest { get { return m_NewLocation; } }

        #region JSON save/load
        private PushOnto(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
        {
            m_Object = src.Require("Object").RequireRef<MapObject>();
            m_Origin = src.Require("From").Deserialize<Location>(options);
            m_NewLocation = src.Require("To").Deserialize<Location>(options);
        }

        static PushOnto IJsonStaged<PushOnto>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new PushOnto(JsonActor(src), src, options));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            toJsonActor(writer);
            writer.WriteRef("Object", m_Object);
            writer.WritePropertyName("From");
            JsonSerializer.Serialize(writer, m_Origin, options);
            writer.WritePropertyName("To");
            JsonSerializer.Serialize(writer, m_NewLocation, options);
        }
        #endregion

        private PushOnto(Actor actor, Location from, Location to, MapObject obj) : base(actor)
        {
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

        static public PushOnto? create(Actor actor, Location from, Location to) {
            var ret = schedule(actor, from, to);
            if (ret is null || !ret.IsPerformable()) return null;
            return ret;
        }

        static public PushOnto? schedule(Actor actor, Location from, Location to) {
            if (1 != Rules.InteractionDistance(in from, in to)) return null;
            if (!actor.Model.CanEnter(ref from)) return null;
            if (!actor.Model.CanEnter(ref to)) return null;
            if (!to.TileModel.IsWalkable) return null;
            var obj = from.MapObject;
            if (obj is null) return null;

            return new(actor, from, to, obj);
        }
    }
}
