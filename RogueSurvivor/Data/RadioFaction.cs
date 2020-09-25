using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    // factions with proper CHAR-tech radios (currently just police) are Organized Force and get better AI.
    // would be reasonable for all of CHAR, National Guard, and Black Ops to have CHAR tech radios as well

    // support classes
    [Serializable]
    class ImplicitRadio : Observer<Location[]>
    {
        public readonly ThreatTracking Threats;
        public readonly LocationSet Investigate;
        public readonly Gameplay.GameFactions.IDs FactionID;

        public ImplicitRadio(RadioFaction faction)
        {
            Threats = faction.Threats;
            Investigate = faction.Investigate;
            FactionID = faction.FactionID;
        }

        public bool update(Location[] fov) {
            var my_faction = Models.Factions[(int)FactionID];
            Investigate.Seen(fov);
            foreach (var loc in fov) {
                var actorAt = loc.Actor;
                if (null != actorAt && (my_faction.IsEnemyOf(actorAt.Faction) || Threats.IsThreat(actorAt))) {
                    Threats.Sighted(actorAt, loc);
                    continue;
                }
                Threats.Cleared(new Location[] { loc });
            }
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
        public static bool IsSecret { get { return (m ?? (m = Engine.Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap)).IsSecret; } }

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
    class RadioFaction
    {
        public readonly Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> ItemMemory = new Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>();
        public readonly ThreatTracking Threats = new ThreatTracking();
        public readonly LocationSet Investigate = new LocationSet();
#if PROTOTYPE
        private readonly List<KeyValuePair<Actor, Actor>> m_Aggressors = new List<KeyValuePair<Actor, Actor>>(); // \todo? migrate from RogueGame::KillActor?
#endif
        static private ImplicitRadio? s_implicitRadio = null;

        public readonly djack.RogueSurvivor.Gameplay.GameFactions.IDs FactionID;
        public readonly djack.RogueSurvivor.Gameplay.GameItems.IDs RadioID;

        public RadioFaction(djack.RogueSurvivor.Gameplay.GameFactions.IDs faction, djack.RogueSurvivor.Gameplay.GameItems.IDs radio)
        {
            FactionID = faction;
            RadioID = radio;
        }

        public ImplicitRadio implicitRadio { get {
            if (null != s_implicitRadio) return s_implicitRadio;
            return s_implicitRadio = new ImplicitRadio(this);
        } }

#if PROTOTYPE
        public ExplicitRadio explicitRadio(Item radio)  { return new ExplicitRadio(this, radio); }
#endif

        public void Clear() {
            ItemMemory.Clear();
            Threats.Clear();
            Investigate.Clear();
#if PROTOTYPE
            m_Aggressors.Clear();
#endif
        }

        public bool IsMine(Actor a) { return (int)FactionID == a.Faction.ID; }
        public bool IsEnemy(Actor a) { return a.Faction.IsEnemyOf(Models.Factions[(int)FactionID]) || Threats.IsThreat(a); }

        public void TrackThroughExitSpawn(Actor a)
        {
            if (IsEnemy(a)) Threats.RecordTaint(a, a.Location);
        }

#if PROTOTYPE
        void AggressedBy(Actor myfac, Actor other) {
#if DEBUG
            if (!IsMine(myfac)) throw new InvalidOperationException("invariant violation");
#else
            if (!IsMine(myfac)) return;
#endif
            if (myfac.Faction.IsEnemyOf(other.Faction)) return;
            foreach(var x in m_Aggressors) if (x.Key == myfac && x.Value==other) return;
            m_Aggressors.Add(new KeyValuePair<Actor, Actor>(myfac, other));
        }

        void Killed(Actor a) {
            if (a.Faction.IsEnemyOf(Models.Factions[(int)FactionID])) return;
            var could_forget = new List<Actor>();
            var ub = m_Aggressors.Count;
            while (0 < --ub) {
                if (m_Aggressors[ub].Value == a) {
                    m_Aggressors.RemoveAt(ub);
                    continue;
                }
                if (m_Aggressors[ub].Key == a) {
                    could_forget.Add(m_Aggressors[ub].Value);
                    m_Aggressors.RemoveAt(ub);
                    continue;
                }
            }
        }
#endif
    }
}
