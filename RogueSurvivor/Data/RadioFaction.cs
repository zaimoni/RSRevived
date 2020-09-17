using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    // factions with proper CHAR-tech radios (currently just police) are Organized Force and get better AI.
    // would be reasonable for all of CHAR, National Guard, and Black Ops to have CHAR tech radios as well

    // support classes
    class ImplicitRadio : Observer<Location[]>
    {
        public readonly ThreatTracking Threats;
        public readonly LocationSet Investigate;
        public readonly djack.RogueSurvivor.Gameplay.GameFactions.IDs FactionID;

        public ImplicitRadio(RadioFaction faction)
        {
            Threats = faction.Threats;
            Investigate = faction.Investigate;
            FactionID = faction.FactionID;
        }

        public bool update(Location[] fov) {
            var my_faction = Models.Factions[(int)FactionID];
            foreach (var loc in fov) {
                Investigate.Seen(loc);
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

    class ExplicitRadio : Observer<Location[]>
    {
        public readonly ThreatTracking Threats;
        public readonly LocationSet Investigate;
        public readonly djack.RogueSurvivor.Gameplay.GameFactions.IDs FactionID;
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
            foreach (var loc in fov) {
                Investigate.Seen(loc);
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

    class RadioFaction
    {
        private readonly Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> m_ItemMemory = new Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>();
        private readonly ThreatTracking m_ThreatTracking = new ThreatTracking();
        private readonly LocationSet m_Investigate = new LocationSet();
        private readonly List<KeyValuePair<Actor, Actor>> m_Aggressors = new List<KeyValuePair<Actor, Actor>>();
        static private ImplicitRadio? s_implicitRadio = null;

        public readonly djack.RogueSurvivor.Gameplay.GameFactions.IDs FactionID;
        public readonly djack.RogueSurvivor.Gameplay.GameItems.IDs RadioID;

        // accessors
        public Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> ItemMemory { get { return m_ItemMemory; } }
        public ThreatTracking Threats { get { return m_ThreatTracking; } }
        public LocationSet Investigate { get { return m_Investigate; } }

        RadioFaction(djack.RogueSurvivor.Gameplay.GameFactions.IDs faction, djack.RogueSurvivor.Gameplay.GameItems.IDs radio)
        {
            FactionID = faction;
            RadioID = radio;
        }

        ImplicitRadio implicitRadio { get {
            if (null != s_implicitRadio) return s_implicitRadio;
            return s_implicitRadio = new ImplicitRadio(this);
        } }

        ExplicitRadio explicitRadio(Item radio)  { return new ExplicitRadio(this, radio); }

        bool IsMine(Actor a) { return (int)FactionID == a.Faction.ID; }

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
            foreach (var actor in could_forget) if (!m_Aggressors.Any(x => x.Value == actor)) Threats.Cleared(actor);
        }
    }
}
