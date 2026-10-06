using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    // factions with proper CHAR-tech radios (currently just police) are Organized Force and get better AI.
    // would be reasonable for all of CHAR, National Guard, and Black Ops to have CHAR tech radios as well

    // support classes
    [Serializable]
    public class ImplicitRadio : Observer<Location[]>
    {
        public readonly GameFactions.IDs FactionID;
        public readonly RadioFaction Faction;

        public ImplicitRadio(RadioFaction faction)
        {
            FactionID = faction.FactionID;
            Faction = faction;
        }

        ThreatTracking Threats => Faction.Threats;
        LocationSet Investigate => Faction.Investigate;

        public bool update(Location[] fov) {
            var my_faction = GameFactions.From(FactionID);
            Investigate.Seen(fov);
            var stage = new Dictionary<Map, List<Point>>();
            foreach (var loc in fov) {
                var actorAt = loc.Actor;
                if (null != actorAt && (my_faction.IsEnemyOf(actorAt.Faction) || Threats.IsThreat(actorAt))) {
                    Threats.Sighted(actorAt, loc);
                    Session.FromID(FactionID)?.Sighted(actorAt);
                    continue;
                }
                if (!stage.TryGetValue(loc.Map, out var cache)) stage.Add(loc.Map, cache = new List<Point>());
                cache.Add(loc.Position);
            }
            Threats.Cleared(stage);
            return false;
        }
    }

#if PROTOTYPE
    // this should be re-implemented as a modifier
    [Serializable]
    class ExplicitRadio : Observer<Location[]>
    {
        public readonly ThreatTracking Threats;
        public readonly LocationSet Investigate;
        public readonly Gameplay.GameFactions.IDs FactionID;
        public readonly Item Radio;

        public ExplicitRadio(RadioFaction faction, Item radio)
        {
#if DEBUG
            if (faction.RadioID != radio.Model.ID) throw new InvalidOperationException("faction radio not provided");
#endif
            Threats = faction.Threats;
            Investigate = faction.Investigate;
            FactionID = faction.FactionID;
            Radio = radio;
        }

        public bool update(Location[] fov) {
            if (!Radio.IsEquipped || Radio.IsUseless) return true;
            var my_faction = Models.Factions[(int)FactionID];
            Investigate.Seen(fov);
            foreach (var loc in fov) {
                var actorAt = loc.Actor;
                if (null != actorAt && !actorAt.IsDead && (my_faction.IsEnemyOf(actorAt.Faction) || Threats.IsThreat(actorAt))) {
                    Threats.Sighted(actorAt, loc);
                    continue;
                }
                Threats.Cleared(new Location[] { loc });
            }
            return false;
        }
    }
#endif

    [Serializable]
    class LookingForCHARBase : Observer<Location[]>
    {
        [NonSerialized] private static Map? m = null;
        public readonly Actor m_Actor; // for when we treat IsSecret per-faction, etc.

        public LookingForCHARBase(Actor who)
        {
            m_Actor = who;
        }

        // \todo the whole concept of secret map should be per-faction (civilians and police are aligned here)
        public static bool IsSecret { get { return (m ??= Engine.Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap).IsSecret; } }

        public bool update(Location[] fov) {
            if (!IsSecret) return true;  // already found
            foreach (var e in m!.Exits) {
                if (0 <= Array.IndexOf(fov, e.Location)) {
                    m.Expose();
                    // \todo faction-specific handling
                    return true;
                }
            }
            return false;
        }
    }

    // should run after ImplicitRadio, to ensure threat/tourism are accurate
    // doesn't actually use fov, that's just to ensure it processes "early"
    [Serializable]
    class TryToClearZones : Observer<Location[]>
    {
        private readonly ThreatTracking Threats;
        private readonly LocationSet Investigate;
        private readonly Actor m_Actor;

        public TryToClearZones(RadioFaction faction, Actor act)
        {
            Threats = faction.Threats;
            Investigate = faction.Investigate;
            m_Actor = act;
        }

        public bool update(Location[] fov) {
            if (null != m_Actor.Controller.enemies_in_FOV) return false;
            if (m_Actor.Location.Map.District.SewersMap == m_Actor.Location.Map) return false; // even in VTG not worth clearing zones in sewer -- flashlight burn
            if (!m_Actor.Location.Map.IsInsideAt(m_Actor.Location.Position)) return false; // don't try to actively clear outside
            var clear_this = m_Actor.Location.ClearableZone;
            if (null == clear_this) return false;
            if (!(m_Actor.Controller is ObjectiveAI oai)) return false; // invariant violation
            if (null != oai.LivePathing()) return false;
            bool threat = Threats.AnyThreatAt(clear_this);
            bool tourism = Investigate.ContainsAny(clear_this);
            if (threat || tourism) {
                var goal = new Gameplay.AI.Goals.ClearZone(Engine.Session.Get.WorldTime.TurnCounter, m_Actor, clear_this);
                oai.SetObjective(goal);
                oai.AddFOVevent(goal);
            }
            return false;
        }
    }

    [Serializable]
    public record class SVOevent(ActorTag s, WhereWhen seen, sbyte v, ActorTag d_o, int t0) {
        public readonly ActorTag subject = s;
        public readonly Engine.AI.WhereWhen s_last_seen = seen;
//        public readonly ActorTag subject = new(s);
//        public readonly Engine.AI.WhereWhen s_last_seen = new(s.Location, t0);
        public readonly sbyte v_code = v;
        public readonly ActorTag direct_object = d_o;
        public readonly int Turn = t0;

        public SVOevent(Actor s, sbyte v, ActorTag d_o, int t0) : this(new(s), new(s.Location,t0), v, d_o, t0) { }
        public SVOevent(SVOevent src, WhereWhen update) : this(src.subject, update, src.v_code, src.direct_object, src.Turn) { }
    }

    [Serializable]
    public record struct Ranking {
        public readonly int t0;
        public readonly int SurvivalPoints;
        public readonly int KillPoints;
        public readonly int AchievementPoints;

        public Ranking(Actor a) {
            t0 = a.Location.Map.LocalTime.TurnCounter;
            var asc = a.ActorScoring;
            AchievementPoints = asc.AchievementPoints;
            KillPoints = asc.KillPoints;
            SurvivalPoints = asc.SurvivalPoints;
        }

        public Ranking(int _t0, int survival, int kill, int achievement) {
            t0 = _t0;
            SurvivalPoints = survival;
            KillPoints = kill;
            AchievementPoints = achievement;
        }

        public Ranking(Ranking src, ActorScoring asc) {
            t0 = src.t0;
            AchievementPoints = asc.AchievementPoints;
            KillPoints = asc.KillPoints;
            SurvivalPoints = asc.SurvivalPoints;
        }

        public int TotalPoints { get => SurvivalPoints + KillPoints + AchievementPoints; }
    }

    [Serializable]
    public record struct Demise {
        public readonly int t1;
        public readonly ActorTag killer;
        public readonly Location loc;

        public Demise(int _t1, ActorTag _killer, Location _loc) {
            t1 = _t1;
            killer = _killer;
            loc = _loc;
        }

        public Demise(Actor victim, Actor? kill) {
            loc = victim.Location;
            t1 = loc.Map.LocalTime.TurnCounter;
            killer = (null == kill ? default : new(kill));
        }
    }

    [Serializable]
    public class RadioFaction
    {
        public readonly Ary2Dictionary<Location, Gameplay.Item_IDs, int> ItemMemory = new();
        public readonly ThreatTracking Threats = new();
        public readonly LocationSet Investigate = new();
        [NonSerialized] private ImplicitRadio? m_implicitRadio = null; // probably will be non-static on second radio faction buildout

        // rethinking investigation causes:
        // * unknown ground/shelf inventories
        // * does the ever-seen map need to be disentangled from item memory?

        public readonly GameFactions.IDs FactionID;
        public readonly Gameplay.Item_IDs RadioID;

        // rethinking police/crime system.  We want to track
        // murder: police, natguard care about this.
        // hangry assault: police, natguard care about this.
        // assault of own: no restriction
        // killing of own: no restriction

        private List<SVOevent> m_EventLog = new();
        private Dictionary<ActorTag, Ranking> m_Quick = new();
        private Dictionary<ActorTag, KeyValuePair<Ranking,Demise>> m_Dead = new();

        public RadioFaction(GameFactions.IDs faction, Gameplay.Item_IDs radio)
        {
            FactionID = faction;
            RadioID = radio;
        }

#region JSON save/load
        /// <summary>Stops tracking live game events.  For a faction that belongs to a session that is not (yet) the live one.</summary>
        public void Detach() => Threats.Detach();
        public void Attach() => Threats.Attach();

        // SVOevent.v_code is a raw number, as its data design is not finished.  The save uses words.
        static private readonly string[] s_VerbNames = { "FactionKill", "FactionAggression", "Murder", "Assault" };    // v_code 1..4

        static private string Render(in ActorTag src) => "{\"Name\":" + JsonSerializer.Serialize(src.Name) + ",\"SpawnTime\":" + src.SpawnTime.ToString() + "}";
        static private string Render(in Location src) => "[[" + src.Position.X.ToString() + "," + src.Position.Y.ToString() + "]," + JSON_ext.RenderRef(src.Map) + "]";
        static private string Render(in Ranking src) => "[" + src.t0.ToString() + "," + src.SurvivalPoints.ToString() + "," + src.KillPoints.ToString() + "," + src.AchievementPoints.ToString() + "]";

        static private Ranking ReadRanking(JsonElement src)
        {
            if (JsonValueKind.Array != src.ValueKind || 4 != src.GetArrayLength()) throw new JsonException("expected [t0, survival, kill, achievement]: " + src.GetRawText());
            return new(src[0].GetInt32(), src[1].GetInt32(), src[2].GetInt32(), src[3].GetInt32());
        }

        /// <param name="handler">runs when the load finishes: the map is a cross-link</param>
        static private void ReadLocation(JsonElement src, Action<Location> handler)
        {
            if (JsonValueKind.Array != src.ValueKind || 2 != src.GetArrayLength()) throw new JsonException("expected [[x, y], map reference]: " + src.GetRawText());
            var pt = src[0].ReadPoint();
            src[1].ReadRef<Map>(m => handler(new Location(m, pt)));
        }

        // FactionID and RadioID are not saved: whoever constructs us knows them.
        // The event log and the rankings hold ActorTags, not references to actors.  One entry per line:
        // EventLog: {"Subject": tag, "Seen": [[x, y], map reference], "SeenTurn", "Verb", "Object": tag, "Turn"}
        // Quick:    {"Who": tag, "Rank": [t0, survival, kill, achievement]}
        // Dead:     likewise, then "Died": turn, "Killer": tag (omitted when there was none), "Where": [[x, y], map reference]
        public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("ItemMemory");
            ItemMemoryJSON.Save(writer, ItemMemory);
            if (Threats.Any()) {
                writer.WritePropertyName("Threats");
                Threats.toJson(writer);
            }
            if (Investigate.Any()) {
                writer.WritePropertyName("Investigate");
                Investigate.toJson(writer);
            }
            if (0 < m_EventLog.Count) {
                List<string> rows = new(m_EventLog.Count);
                foreach (var x in m_EventLog) {
                    var verb = (1 <= x.v_code && s_VerbNames.Length >= x.v_code) ? s_VerbNames[x.v_code - 1] : x.v_code.ToString();
                    rows.Add("{\"Subject\":" + Render(x.subject) + ",\"Seen\":" + Render(x.s_last_seen.Location) + ",\"SeenTurn\":" + x.s_last_seen.Turn.ToString()
                           + ",\"Verb\":\"" + verb + "\",\"Object\":" + Render(x.direct_object) + ",\"Turn\":" + x.Turn.ToString() + "}");
                }
                writer.WritePropertyName("EventLog");
                writer.WriteArrayOfLines(rows);
            }
            if (0 < m_Quick.Count) {
                List<string> rows = new(m_Quick.Count);
                foreach (var x in m_Quick) rows.Add("{\"Who\":" + Render(x.Key) + ",\"Rank\":" + Render(x.Value) + "}");
                writer.WritePropertyName("Quick");
                writer.WriteArrayOfLines(rows);
            }
            if (0 < m_Dead.Count) {
                List<string> rows = new(m_Dead.Count);
                foreach (var x in m_Dead) {
                    var demise = x.Value.Value;
                    rows.Add("{\"Who\":" + Render(x.Key) + ",\"Rank\":" + Render(x.Value.Key) + ",\"Died\":" + demise.t1.ToString()
                           + (default == demise.killer ? "" : ",\"Killer\":" + Render(demise.killer)) + ",\"Where\":" + Render(demise.loc) + "}");
                }
                writer.WritePropertyName("Dead");
                writer.WriteArrayOfLines(rows);
            }
            writer.WriteEndObject();
        }

        /// <summary>Precondition: we are freshly constructed.</summary>
        public void LoadJson(JsonElement src, JsonSerializerOptions options)
        {
            if (src.TryGetProperty("ItemMemory", out var relay)) ItemMemoryJSON.Load(relay, ItemMemory);
            if (src.TryGetProperty("Threats", out relay)) Threats.LoadJson(relay);
            if (src.TryGetProperty("Investigate", out relay)) Investigate.LoadJson(relay);
            if (src.TryGetProperty("EventLog", out relay)) {
                foreach (var x in relay.EnumerateArray()) {
                    var subject = x.Require("Subject").Deserialize<ActorTag>(options);
                    var seen_turn = x.Require("SeenTurn").GetInt32();
                    var verb = x.Require("Verb").GetString() ?? throw new JsonException("null Verb");
                    var v_code = 1 + Array.IndexOf(s_VerbNames, verb);
                    if (0 >= v_code && !int.TryParse(verb, out v_code)) throw new JsonException("unrecognized Verb: " + verb);
                    var direct_object = x.Require("Object").Deserialize<ActorTag>(options);
                    var turn = x.Require("Turn").GetInt32();
                    ReadLocation(x.Require("Seen"), loc => m_EventLog.Add(new(subject, new(loc, seen_turn), (sbyte)v_code, direct_object, turn)));
                }
            }
            if (src.TryGetProperty("Quick", out relay)) {
                foreach (var x in relay.EnumerateArray()) m_Quick.Add(x.Require("Who").Deserialize<ActorTag>(options), ReadRanking(x.Require("Rank")));
            }
            if (src.TryGetProperty("Dead", out relay)) {
                foreach (var x in relay.EnumerateArray()) {
                    var who = x.Require("Who").Deserialize<ActorTag>(options);
                    var rank = ReadRanking(x.Require("Rank"));
                    var died = x.Require("Died").GetInt32();
                    ActorTag killer = x.TryGetProperty("Killer", out var y) ? y.Deserialize<ActorTag>(options) : default;
                    ReadLocation(x.Require("Where"), loc => m_Dead.Add(who, new(rank, new(died, killer, loc))));
                }
            }
        }
#endregion

        public ImplicitRadio implicitRadio { get => m_implicitRadio ??= new ImplicitRadio(this); }

#if PROTOTYPE
        public ExplicitRadio explicitRadio(Item radio)  { return new ExplicitRadio(this, radio); }
#endif

        public bool IsMine(Actor a) => a.IsFaction(FactionID);
        public bool IsEnemy(Actor a) => a.Faction.IsEnemyOf(GameFactions.From(FactionID)) || Threats.IsThreat(a);

        [NonSerialized] private Func<Actor, bool>? _isMine = null;
        public Func<Actor, bool> isMine() {
            if (null == _isMine) {
                _isMine = a => a.IsFaction(FactionID);
            }
            return _isMine;
        }

        public void TrackThroughExitSpawn(Actor a)
        {
            if (IsEnemy(a)) Threats.RecordTaint(a, a.Location);
        }

        public SVOevent? EncodeKill(Actor? killer, Actor victim, int t0) {
            if (null == killer) return null;
            if (IsMine(victim)) return new(killer, 1, new(victim), t0); // faction kill
            if (Rules.IsMurder(killer, victim)) return new(killer, 3, new(victim), t0);
            return null;
        }

        public SVOevent? EncodeAggression(Actor killer, Actor victim, int t0) {
            if (Rules.IsMurder(killer, victim)) {
                if (IsMine(victim)) { // faction aggression
                    return new(killer, 2, new(victim), t0);
                } else { // assault
                    return new(killer, 4, new(victim), t0);
                };
            }
            return null;
        }

        public void Record(SVOevent? e) {
            if (null != e) m_EventLog.Add(e);
        }

        public bool IsTargeted(ActorTag perp)
        {
            foreach (var x in m_EventLog) if (x.subject == perp) return true;
            return false;
        }
        public bool IsTargeted(Actor perp) => IsTargeted(new ActorTag(perp));

        public List<SVOevent>? WantedFor(ActorTag perp) {
            List<SVOevent>? ret = null;
            foreach (var x in m_EventLog) if (x.subject == perp) (ret ??= new()).Add(x);
            return ret;
        }
        public List<SVOevent>? WantedFor(Actor perp) => WantedFor(new ActorTag(perp));

        public int CountCapitalCrimes(ActorTag perp) {
            int ret = 0;
            foreach (var x in m_EventLog) if (x.subject == perp) ++ret;
            return ret;
        }
        public int CountCapitalCrimes(Actor perp) => CountCapitalCrimes(new ActorTag(perp));

        public int CountMurders(ActorTag perp)
        {
            int ret = 0;
            foreach (var x in m_EventLog) if (x.subject == perp && (1 == x.v_code || 3 == x.v_code)) ++ret;
            return ret;
        }
        public int CountMurders(Actor perp) => CountMurders(new ActorTag(perp));

        public int CountAssaults(ActorTag perp)
        {
            int ret = 0;
            foreach (var x in m_EventLog) if (x.subject == perp && (2 == x.v_code || 4 == x.v_code)) ++ret;
            return ret;
        }
        public int CountAssaults(Actor perp) => CountAssaults(new ActorTag(perp));

        public void Sighted(Actor a) {
            ActorTag evil = new(a);
            int i = m_EventLog.Count;
            while (0 <= --i) {
                var whom = m_EventLog[i].subject;
                if (m_EventLog[i].subject == evil) {
                    m_EventLog[i] = new(m_EventLog[i], new(a.Location, a.Location.Map.LocalTime.TurnCounter));
                }
            }
        }

        public List<KeyValuePair<ActorTag, Engine.AI.WhereWhen> >? Wanted() {
            List<ActorTag> ret = new();
            List<KeyValuePair<ActorTag, Engine.AI.WhereWhen> > ret2 = new();
            // want time-reversed order
            int i = m_EventLog.Count;
            while (0 <= --i) {
                var whom = m_EventLog[i].subject;
                if (!ret.Contains(whom)) {
                    ret.Add(whom);
                    ret2.Add(new(whom, m_EventLog[i].s_last_seen));
                }
            }
            return (0<ret2.Count) ? ret2 : null;
        }

        public void onTurnStart(Actor a) {
            ActorTag index = new(a);
            lock (m_Quick) {
                if (m_Quick.TryGetValue(index, out var rank)) {
                    m_Quick[index] = new(rank, a.ActorScoring);
                } else {
                    m_Quick.Add(index, new(a));
                }
            }
        }

        private void OnKilled(ActorTag perp)
        {
            lock (m_EventLog) {
                var ub = m_EventLog.Count;
                while (0 <= --ub) if (m_EventLog[ub].subject == perp) m_EventLog.RemoveAt(ub);
            }
        }

        public void OnKilled(Actor victim, Actor? killer) {
            ActorTag index = new(victim);
            OnKilled(index);
            lock (m_Quick) {
                if (m_Quick.TryGetValue(index, out var rank)) {
                    m_Dead.Add(index, new(rank, new(victim, killer)));
                    m_Quick.Remove(index);
                }
            }
        }

        public List<KeyValuePair<ActorTag, Ranking>> Rankings() {
            static int cmp(KeyValuePair<ActorTag, Ranking> lhs, KeyValuePair<ActorTag, Ranking> rhs)
            {
                var code = lhs.Value.TotalPoints.CompareTo(rhs.Value.TotalPoints);
                if (0 != code) return -code;
                code = lhs.Value.t0.CompareTo(rhs.Value.t0);
                if (0 != code) return -code;
                return lhs.Key.Name.CompareTo(rhs.Key.Name);
            }

            List<KeyValuePair<ActorTag, Ranking>> ret = m_Quick.ToList();
            ret.Sort(cmp);
            return ret;
        }

        public List<KeyValuePair<ActorTag, KeyValuePair<Ranking, Demise>>> Deceased() {
            static int cmp(KeyValuePair<ActorTag, KeyValuePair<Ranking, Demise>> lhs, KeyValuePair<ActorTag, KeyValuePair<Ranking, Demise>> rhs)
            {
                var code = lhs.Value.Key.TotalPoints.CompareTo(rhs.Value.Key.TotalPoints);
                if (0 != code) return -code;
                code = lhs.Value.Key.t0.CompareTo(rhs.Value.Key.t0);
                if (0 != code) return -code;
                return lhs.Key.Name.CompareTo(rhs.Key.Name);
            }

            List<KeyValuePair<ActorTag, KeyValuePair<Ranking, Demise>>> ret = m_Dead.ToList();
            ret.Sort(cmp);
            return ret;
        }
    }
}

namespace djack.RogueSurvivor.Data
{
    // Item memory: for each location seen, when, and which kinds of item were there.  Not specific to a radio faction.
    // One entry per map: {"Map": reference, "Kinds": [names], "Seen": [...], "Nothing": [...]}.
    // Seen: one line per cell that had items, [x, y, turn, kinds...], each kind an index into Kinds, the block's legend (sorted by name).
    //   A kind by name in place of an index is accepted on load: the shape before 2026-10-06, which also put the cells where
    //   nothing was in Seen, as [x, y, turn].
    // Nothing: the cells where nothing was, as runs along a row at one turn: [turn, y, x0, x1], or [turn, y, x] for one cell.
    //   (The police are told about every tile of every entry map at world creation, so as single cells these dominated the file.)
    static public class ItemMemoryJSON
    {
        private sealed class Block
        {
            public readonly HashSet<Gameplay.Item_IDs> kinds = new();
            public readonly List<KeyValuePair<Point, KeyValuePair<int, HashSet<Gameplay.Item_IDs>>>> seen = new();
            public readonly Dictionary<int, Dictionary<short, List<short>>> nothing = new();   // turn, row, columns
        }

        static public void Save(Utf8JsonWriter writer, Ary2Dictionary<Location, Gameplay.Item_IDs, int> src)
        {
            List<KeyValuePair<Map, Block>> by_map = new();
            Block block(Map m) {
                foreach (var x in by_map) if (x.Key == m) return x.Value;
                by_map.Add(new(m, new()));
                return by_map[by_map.Count - 1].Value;
            }

            var empty = src.GetNothingSeenEnumerator();
            while (empty.MoveNext()) {
                var x = empty.Current;
                var b = block(x.Key.Map);
                if (!b.nothing.TryGetValue(x.Value, out var rows)) b.nothing.Add(x.Value, rows = new());
                if (!rows.TryGetValue(x.Key.Position.Y, out var columns)) rows.Add(x.Key.Position.Y, columns = new());
                columns.Add(x.Key.Position.X);
            }
            foreach (var x in src) {
                var b = block(x.Key.Map);
                b.seen.Add(new(x.Key.Position, x.Value));
                b.kinds.UnionWith(x.Value.Value);
            }

            writer.WriteStartArray();
            foreach (var x in by_map) {
                var b = x.Value;
                writer.WriteStartObject();
                writer.WriteRef("Map", x.Key);
                if (0 < b.seen.Count) {
                    var legend = b.kinds.OrderBy(k => k.ToString(), StringComparer.Ordinal).ToList();
                    var index = new Dictionary<Gameplay.Item_IDs, int>();
                    foreach (var k in legend) index.Add(k, index.Count);
                    writer.WritePropertyName("Kinds");
                    writer.WriteRawValue("[\"" + string.Join("\",\"", legend) + "\"]");
                    List<string> rows = new(b.seen.Count);
                    foreach (var cell in b.seen) {
                        var stage = new System.Text.StringBuilder("[");
                        stage.Append(cell.Key.X).Append(',').Append(cell.Key.Y).Append(',').Append(cell.Value.Key);
                        foreach (var id in cell.Value.Value) stage.Append(',').Append(index[id]);
                        rows.Add(stage.Append(']').ToString());
                    }
                    writer.WritePropertyName("Seen");
                    writer.WriteArrayOfLines(rows);
                }
                if (0 < b.nothing.Count) {
                    List<string> rows = new();
                    foreach (var turn in b.nothing.Keys.OrderBy(t => t)) {
                        var by_row = b.nothing[turn];
                        foreach (var y in by_row.Keys.OrderBy(y => y)) {
                            var columns = by_row[y];
                            columns.Sort();
                            int i = 0;
                            while (i < columns.Count) {
                                int j = i;
                                while (j + 1 < columns.Count && columns[j + 1] == columns[j] + 1) j++;
                                var stage = new System.Text.StringBuilder("[");
                                stage.Append(turn).Append(',').Append(y).Append(',').Append(columns[i]);
                                if (i < j) stage.Append(',').Append(columns[j]);
                                rows.Add(stage.Append(']').ToString());
                                i = j + 1;
                            }
                        }
                    }
                    writer.WritePropertyName("Nothing");
                    writer.WriteArrayOfLines(rows);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        static private Gameplay.Item_IDs Kind(string? name)
        {
            var id = JSON_ext.ParseEnum<Gameplay.Item_IDs>(name);
            if (Gameplay.Item_IDs._COUNT == id) throw new JsonException("not an item kind: _COUNT");
            return id;
        }

        /// <param name="dest">Filled when the load finishes: the maps are cross-links.</param>
        static public void Load(JsonElement src, Ary2Dictionary<Location, Gameplay.Item_IDs, int> dest)
        {
            foreach (var x in src.EnumerateArray()) {
                var block = x;
                x.Require("Map").ReadRef<Map>(m => {
                    List<Gameplay.Item_IDs>? legend = null;
                    if (block.TryGetProperty("Kinds", out var kinds)) {
                        legend = new();
                        foreach (var k in kinds.EnumerateArray()) legend.Add(Kind(k.GetString()));
                    }
                    if (block.TryGetProperty("Seen", out var seen)) {
                        foreach (var row in seen.EnumerateArray()) {
                            var ub = row.GetArrayLength();
                            if (3 > ub) throw new JsonException("expected [x, y, turn, item kinds...]: " + row.GetRawText());
                            List<Gameplay.Item_IDs>? ids = null;
                            for (int i = 3; i < ub; i++) {
                                var k = row[i];
                                if (JsonValueKind.Number == k.ValueKind) {
                                    var n = k.GetInt32();
                                    if (null == legend || 0 > n || legend.Count <= n) throw new JsonException("item kind not in the legend: " + row.GetRawText());
                                    (ids ??= new()).Add(legend[n]);
                                } else (ids ??= new()).Add(Kind(k.GetString()));
                            }
                            dest.Set(new Location(m, new Point(row[0].GetInt16(), row[1].GetInt16())), ids, row[2].GetInt32());
                        }
                    }
                    if (block.TryGetProperty("Nothing", out var nothing)) {
                        foreach (var row in nothing.EnumerateArray()) {
                            var ub = row.GetArrayLength();
                            if (3 != ub && 4 != ub) throw new JsonException("expected [turn, y, x] or [turn, y, x0, x1]: " + row.GetRawText());
                            var turn = row[0].GetInt32();
                            var y = row[1].GetInt16();
                            var x0 = row[2].GetInt16();
                            var x1 = (4 == ub) ? row[3].GetInt16() : x0;
                            if (x1 < x0) throw new JsonException("run out of order: " + row.GetRawText());
                            for (var px = x0; px <= x1; px++) dest.Set(new Location(m, new Point(px, y)), null, turn);
                        }
                    }
                });
            }
        }
    }
}
