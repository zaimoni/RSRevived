using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // only need to see one target to expire
    [Serializable]
    class AcquireLineOfSight : Objective, LatePathable, Observer<Location[]>, IJsonStaged<AcquireLineOfSight>
    {
        readonly private HashSet<Location> _locs;
        [NonSerialized] ObjectiveAI oai;

        #region JSON save/load

        private AcquireLineOfSight(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src) {
            _locs = src.Require("Locations").Deserialize<HashSet<Location>>(options) ?? throw new JsonException("null Locations");

            OnDeserialized();
        }


        static AcquireLineOfSight IJsonStaged<AcquireLineOfSight>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new AcquireLineOfSight(JsonActor(src), src, options));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            writer.WriteArray("Locations", _locs, options);

        }

        #endregion


        public AcquireLineOfSight(Actor who, in Location loc) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (!who.CanEnter(loc)) throw new InvalidOperationException("unpathable");
#endif
            _locs = new HashSet<Location> { loc };
            OnDeserialized();
        }

        public AcquireLineOfSight(Actor who, IEnumerable<Location> locs) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            _locs = new HashSet<Location>(locs);
            _locs.RemoveWhere(goal => !who.CanEnter(goal));
#if DEBUG
            if (0 >= _locs.Count) throw new ArgumentNullException(nameof(locs));
#endif
            OnDeserialized();
        }

        public AcquireLineOfSight(Actor who, HashSet<Location> locs) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            _locs = locs; // intentional by-reference assignment
            _locs.RemoveWhere(goal => !who.CanEnter(goal));
#if DEBUG
            if (0 >= _locs.Count) throw new ArgumentNullException(nameof(locs));
#endif
            OnDeserialized();
        }

        private void OnDeserialized()
        {
            if (m_Actor.Controller is ObjectiveAI ai) oai = ai;
            else throw new ArgumentNullException(nameof(oai));
        }

        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            if (IsExpired) return true;

            var allies = m_Actor.Allies;
            if (null != allies) foreach (var friend in allies) { // if any in-communication ally can see the location, clear it
                if (!oai.InCommunicationWith(friend)) continue;
                if (_locs.Any(loc => friend.Controller.CanSee(in loc))) {
                    _isExpired = true;
                    return true;
                }
            }

            // defer actual pathing action to allow other non-combat heuristics to cut in
            return false;
        }

        public ActorAction? Pathing() { return 0 < oai.InterruptLongActivity() ? null : oai.BehaviorPathTo(_locs); }

        public bool update(Location[] src) {
            if (_locs.Any(x => 0 <= Array.IndexOf(src, x))) {
                _isExpired = true;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Acquiring line of sight to any of " + _locs.to_s();
        }
    }
}
