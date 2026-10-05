using System;
using System.Collections.Generic;

using djack.RogueSurvivor.Data;
using Zaimoni.Data;
using System.Text.Json;
using Zaimoni.JSON;

// work required to make Exit class suitable for C# hashing deemed excessive (Actor was problematic)
// but likely would address CPU issues (current implementation is quadratic)
namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    class BlacklistExits : Objective, IJsonStaged<BlacklistExits>
    {
        private readonly Dictionary<int, List<Exit>> _blacklist = new Dictionary<int, List<Exit>>();

        #region JSON save/load

        // Exits have no ids and are compared by reference, so one is written as [from, to] and is the live map's own object

        // again on load: from.Map.GetExitAt(from.Position), checked to still lead to where it led.

        private BlacklistExits(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

        {

            foreach (var x in src.Require("Blacklist").EnumerateArray()) {

                if (JsonValueKind.Array != x.ValueKind || 2 != x.GetArrayLength()) throw new JsonException("expected [turn, [exits]]: " + x.GetRawText());

                List<Exit> exits = new();

                foreach (var pair in x[1].EnumerateArray()) {

                    if (JsonValueKind.Array != pair.ValueKind || 2 != pair.GetArrayLength()) throw new JsonException("expected [from, to]: " + pair.GetRawText());

                    var from = pair[0].Deserialize<Location>(options);

                    var to = pair[1].Deserialize<Location>(options);

                    var e = from.Map.GetExitAt(from.Position) ?? throw new JsonException("no exit at " + from.ToString());

                    if (e.Location != to) throw new JsonException("the exit at " + from.ToString() + " leads to " + e.Location.ToString() + ", not " + to.ToString());

                    exits.Add(e);

                }

                _blacklist.Add(x[0].GetInt32(), exits);

            }

        }


        static BlacklistExits IJsonStaged<BlacklistExits>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new BlacklistExits(JsonActor(src), src, options));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            writer.WriteStartArray("Blacklist");

            foreach (var x in _blacklist) {

                writer.WriteStartArray();

                writer.WriteNumberValue(x.Key);

                writer.WriteStartArray();

                foreach (var e in x.Value) {

                    writer.WriteStartArray();

                    JsonSerializer.Serialize(writer, OriginOf(e), options);

                    JsonSerializer.Serialize(writer, e.Location, options);

                    writer.WriteEndArray();

                }

                writer.WriteEndArray();

                writer.WriteEndArray();

            }

            writer.WriteEndArray();

        }


        // An exit does not know its origin; its map does.  The actor's map first, then the maps around it.

        private Location OriginOf(Exit e)

        {

            var here = m_Actor.Location.Map;

            var pt = here.FindExitOrigin(e);

            if (null != pt) return new Location(here, pt.Value);

            foreach (var m in here.District.Maps) if (null != (pt = m.FindExitOrigin(e))) return new Location(m, pt.Value);

            foreach (var m in e.ToMap.District.Maps) if (null != (pt = m.FindExitOrigin(e))) return new Location(m, pt.Value);

            foreach (var back in e.ToMap.Exits) if (null != (pt = back.ToMap.FindExitOrigin(e))) return new Location(back.ToMap, pt.Value);

            throw new JsonException(m_Actor.UnmodifiedName + " blacklists an exit to " + e.Location.ToString() + " that no nearby map has");

        }

        #endregion


        public BlacklistExits(int t0, Actor who, Exit e)
        : base(t0, who)
        {
            Blacklist(e);
        }

        // this is an influence on behavior; never generates an action
        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            var turn = Engine.Session.Get.WorldTime.TurnCounter;
            var d_size = Engine.RogueGame.Options.DistrictSize;
            _blacklist.OnlyIf(time => turn <= time+d_size);
            if (0 >= _blacklist.Count) _isExpired = true;
            return false;
        }

        public void Blacklist(Exit e) {
            var del = new Zaimoni.Data.Stack<int>(stackalloc int[_blacklist.Count]);
            var turn = Engine.Session.Get.WorldTime.TurnCounter;
            bool landed = false;
            foreach (var x in _blacklist) {
                if (x.Key == turn) {
                    if (!x.Value.Contains(e)) x.Value.Add(e);
                    landed = true;
                    continue;
                }
                if (x.Value.Contains(e)) {
                    x.Value.Remove(e);
                    if (0 >= x.Value.Count) del.push(x.Key);
                }
            }
            if (!landed) _blacklist.Add(turn, new List<Exit> { e });
            int i = del.Count;
            while (0 <= --i) _blacklist.Remove(del[i]);
        }

        public bool Veto(Exit e) {
            foreach (var x in _blacklist) if (x.Value.Contains(e)) return true;
            return false;
        }

        public void Censor(HashSet<Location> goals) {
            bool code_2_gone = false;
            foreach (var x in _blacklist) {
                foreach (var e in x.Value) {
                 var code = District.UsesCrossDistrictView(e.Location.Map);
                  if (2 == code) {
                    if (!code_2_gone) { // sewers
                      goals.RemoveWhere(loc => 2== District.UsesCrossDistrictView(loc.Map));
                      code_2_gone = true;
                    }
                  } else goals.Remove(e.Location);
                }
            }
        }
    }
}
