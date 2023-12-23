﻿using djack.RogueSurvivor.Gameplay.AI;
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
    class ImplicitRadio : Observer<Location[]>
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
    class RadioFaction
    {
        public readonly Ary2Dictionary<Location, Gameplay.Item_IDs, int> ItemMemory = new();
        public readonly ThreatTracking Threats = new();
        public readonly LocationSet Investigate = new();
        static private ImplicitRadio? s_implicitRadio = null;

        public readonly GameFactions.IDs FactionID;
        public readonly Gameplay.Item_IDs RadioID;

        // rethinking police/crime system.  We want to track
        // murder: police, natguard care about this.
        // hangry assault: police, natguard care about this.
        // assault of own: no restriction
        // killing of own: no restriction

        public RadioFaction(GameFactions.IDs faction, Gameplay.Item_IDs radio)
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

        public bool IsMine(Actor a) { return a.IsFaction(FactionID); }
        public bool IsEnemy(Actor a) { return a.Faction.IsEnemyOf(GameFactions.From(FactionID)) || Threats.IsThreat(a); }

        public void TrackThroughExitSpawn(Actor a)
        {
            if (IsEnemy(a)) Threats.RecordTaint(a, a.Location);
        }
    }
}
