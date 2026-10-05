using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using djack.RogueSurvivor.Data;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    internal class Resupply : Objective, LatePathable, IJsonStaged<Resupply>
    {
        private readonly Item_IDs[] critical;
        private readonly Location[] dests;
        [NonSerialized] private ObjectiveAI oai;

        #region JSON save/load

        private Resupply(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

        {

            List<Item_IDs> kinds = new();

            foreach (var x in src.Require("Critical").EnumerateArray()) kinds.Add(JSON_ext.ParseEnum<Item_IDs>(x.GetString()));

            critical = kinds.ToArray();

            dests = src.Require("Destinations").Deserialize<Location[]>(options) ?? throw new JsonException("null Destinations");

            OnDeserialized(default);

        }


        static Resupply IJsonStaged<Resupply>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new Resupply(JsonActor(src), src, options));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            writer.WriteStartArray("Critical");

            foreach (var x in critical) writer.WriteStringValue(x.ToString());

            writer.WriteEndArray();

            writer.WriteArray("Destinations", dests, options);

        }

        #endregion


        public Resupply(Actor who, IEnumerable<Item_IDs> what, IEnumerable<Location> head_for) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            if (!(who.Controller is ObjectiveAI ai)) throw new InvalidOperationException("need an ai with inventory");
            oai = ai;

            critical = what.ToArray();
            dests = head_for.ToArray();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            oai = (m_Actor.Controller as ObjectiveAI)!;
        }

        private bool desires_changed(HashSet<Item_IDs> src) {
            if (critical.Length != src.Count) return true;
            foreach (var x in critical) if (!src.Contains(x)) return true;
            var seen = oai.items_in_FOV;
            if (null != seen) {
                foreach (var x in seen) {
                    foreach (var it in critical) {
                        if (x.Value.Inventory.Has(it)) return true;
                    }
                }
            }
            return false;
        }

        public override bool UrgentAction_readonly(out ActorAction? ret)

        {

          ret = null;   // asks what the AI needs and wants: deliberation, not stored state

          return false;

        }


        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;

            if (m_Actor.Controller.IsEngaged) { // expire now
                _isExpired = true;
                return true;
            }
            if (0 <= Array.IndexOf(dests, m_Actor.Location)) {
                _isExpired = true;
                return true;
            }

            var items = oai.WhatHaveISeen();
            var need = oai.WhatDoINeedNow();
            var want = oai.WhatDoIWantNow();

            need.IntersectWith(items!);
            want.IntersectWith(items!);
            if (0 == need.Count) {
                if (desires_changed(want)) {
                    _isExpired = true;
                    return true;
                }
            } else {
                if (0 < want.Count) need.UnionWith(want);
                if (desires_changed(need)) {
                    _isExpired = true;
                    return true;
                }
            }

            // let other AI processing kick in before final pathing
            return false;
        }


        public ActorAction? Pathing()
        {
            var _locs = dests.Where(loc => !loc.StrictHasActorAt);
            if (!_locs.Any()) return null;

            var ret = oai.BehaviorPathTo(new(_locs));
            return (ret?.IsPerformable() ?? false) ? ret : null;
        }
    }
}
