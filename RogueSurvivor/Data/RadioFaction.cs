using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

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
        public readonly ThreatTracking Threats;
        public readonly LocationSet Investigate;
        public readonly GameFactions.IDs FactionID;

        public ImplicitRadio(RadioFaction faction)
        {
            Threats = faction.Threats;
            Investigate = faction.Investigate;
            FactionID = faction.FactionID;
        }

        public bool update(Location[] fov) {
            var my_faction = GameFactions.From(FactionID);
            Investigate.Seen(fov);
            var stage = new Dictionary<Map, List<Point>>();
            foreach (var loc in fov) {
                var actorAt = loc.Actor;
                if (null != actorAt && (my_faction.IsEnemyOf(actorAt.Faction) || Threats.IsThreat(actorAt))) {
                    Threats.Sighted(actorAt, loc);
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
    public record class SVOevent(ActorTag s, sbyte v, ActorTag d_o, int t0) {
        public readonly ActorTag subject = s;
        public readonly sbyte v_code = v;
        public readonly ActorTag direct_object = d_o;
        public readonly int Turn = t0;
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
            if (IsMine(victim)) return new(new(killer), 1, new(victim), t0); // faction kill
            if (Rules.IsMurder(killer, victim)) return new(new(killer), 3, new(victim), t0);
            return null;
        }

        public SVOevent? EncodeAggression(Actor killer, Actor victim, int t0) {
            if (Rules.IsMurder(killer, victim)) {
                if (IsMine(victim)) { // faction aggression
                    return new(new(killer), 2, new(victim), t0);
                } else { // assault
                    return new(new(killer), 4, new(victim), t0);
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

        public List<ActorTag>? Wanted() {
            List<ActorTag> ret = new();
            // want time-reversed order
            int i = m_EventLog.Count;
            while (0 <= --i) {
                var whom = m_EventLog[i].subject;
                if (!ret.Contains(whom)) ret.Add(whom);
            }
            return (0<ret.Count) ? ret : null;
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
            List<KeyValuePair<ActorTag, Ranking>> ret = m_Quick.ToList();

            static int cmp(KeyValuePair<ActorTag, Ranking> lhs, KeyValuePair<ActorTag, Ranking> rhs) {
                var code = lhs.Value.TotalPoints.CompareTo(rhs.Value.TotalPoints);
                if (0 != code) return -code;
                code = lhs.Value.t0.CompareTo(rhs.Value.t0);
                if (0 != code) return -code;
                return lhs.Key.Name.CompareTo(rhs.Key.Name);
            }

            ret.Sort(cmp);

            return ret;
        }
    }
}
