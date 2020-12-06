using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using System.Runtime.Serialization;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // Civilian: view every single coordinate in the zone
    // Police: view every contaminated square in the zone (tourism and/or threat)
    // ZoneLoc member must be "suitable for clearing" (in particular, completely within bounds so its locations are in canonical form)

    // police version
    [Serializable]
    class ClearZone : Objective,Pathable,Observer<Location[]>
    {
        private ZoneLoc m_Zone;
        private HashSet<Point> m_Unverified = new HashSet<Point>();
        [NonSerialized] private ObjectiveAI oai;
        [NonSerialized] private ThreatTracking threats;
        [NonSerialized] private LocationSet tourism;

        public ClearZone(int t0, Actor who, ZoneLoc dest) : base(t0, who) {
            m_Zone = dest;
            threats = who.Threats    // these two should agree on whether they're null or not
#if DEBUG
              ?? throw new ArgumentNullException("who.Threats")
#endif
            ;
            tourism = who.InterestingLocs
#if DEBUG
              ?? throw new ArgumentNullException("who.InterestingLocs")
#endif
            ;
            Func<Point,bool> ok = pt => m_Zone.Rect.Contains(pt);
            m_Unverified.UnionWith(threats.ThreatWhere(dest.m).Where(ok));
            m_Unverified.UnionWith(tourism.In(dest.m).Where(ok));
#if OBSOLETE
            // the civilian case
            if (null == threats && null == sights_to_see) {
                m_Unverified.UnionWith(m_Zone.Rect.Where(pt => who.CanEnter(new Location(m_Zone.m, pt)))); // \todo? eliminate GC thrashing
            }
#endif
            // nonserialized fields
            oai = (m_Actor.Controller as ObjectiveAI)
#if DEBUG
              ?? throw new ArgumentNullException("who.Controller is ObjectiveAI")
#endif
            ;
        }

        [OnDeserialized] private void OnDeserialized(StreamingContext context)
        {
            oai = (m_Actor.Controller as ObjectiveAI)!;
            threats = m_Actor.Threats!;
            tourism = m_Actor.InterestingLocs!;
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            Predicate<Point>? is_cleared = null;
            var threats_at = threats.ThreatWhere(m_Zone.m); // should have both of these null or non-null; other cases are formal completeness
            if (0 < threats_at.Count) is_cleared = pt => !threats_at.Contains(pt);
            var tourism_at = tourism.In(m_Zone.m);
            if (0 < tourism_at.Count) is_cleared = is_cleared.And(pt => !tourism_at.Contains(pt));
            else if (null == is_cleared) {
                _isExpired = true;
                return true;
            }
            m_Unverified.RemoveWhere(is_cleared);
            if (0 >= m_Unverified.Count) {
                _isExpired = true;
                return true;
            }
            if (0 < oai.InterruptLongActivity()) return false;
            ret = Pathing();
            return true;
        }

        public bool update(Location[] fov) {
            if (    null != oai.enemies_in_FOV // expire immediately if enemy sighted
                || !m_Zone.Contains(m_Actor.Location)) { // we left the zone for some reason
                _isExpired = true;
                return true;
            }
            foreach (var loc in fov) {
              if (m_Zone.m == loc.Map) m_Unverified.Remove(loc.Position);
            }
            if (0 >= m_Unverified.Count) {
                _isExpired = true;
                return true;
            }
            return false;
        }

        public ActorAction? Pathing() {
            if (m_Actor.IsPlayer) return null;  // just advise AIs, don't remove player agency
            var goals = new HashSet<Location>();
            foreach (var pt in m_Unverified) goals.Add(new Location(m_Zone.m, pt));
            return oai.BehaviorPathTo(goals); // would need value-copy anyway of goals
        }
    }
}
