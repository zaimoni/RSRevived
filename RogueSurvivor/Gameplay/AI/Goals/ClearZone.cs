using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
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

        public ClearZone(int t0, Actor who, ZoneLoc dest) : base(t0, who) {
            m_Zone = dest;
            var threats = who.Threats!;    // these two should agree on whether they're null or not
            var sights_to_see = who.InterestingLocs!;
            Func<Point,bool> ok = pt => m_Zone.Rect.Contains(pt);
            m_Unverified.UnionWith(threats.ThreatWhere(dest.m).Where(ok));
            m_Unverified.UnionWith(sights_to_see.In(dest.m).Where(ok));
#if OBSOLETE
            // the divilian case
            if (null == threats && null == sights_to_see) {
                m_Unverified.UnionWith(m_Zone.Rect.Where(pt => who.CanEnter(new Location(m_Zone.m, pt)))); // \todo? eliminate GC thrashing
            }
#endif
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            var threats_at = m_Actor.Threats?.ThreatWhere(m_Zone.m); // should have both of these null or non-null; other cases are formal completeness
            var tourism_at = m_Actor.InterestingLocs?.In(m_Zone.m);
            m_Unverified.RemoveWhere(pt => !threats_at.Contains(pt) && !tourism_at.Contains(pt));
            if (0 >= m_Unverified.Count) {
                _isExpired = true;
                return true;
            }
            if (0 < (m_Actor.Controller as ObjectiveAI)!.InterruptLongActivity()) return false;
            ret = Pathing();
            return true;
        }

        public bool update(Location[] fov) {
            if (null != m_Actor.Controller.enemies_in_FOV) { // expire immediately if enemy sighted
                _isExpired = true;
                return true;
            }
            if (!m_Zone.Contains(m_Actor.Location)) { // we left the zone for some reason
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
            var goals = new HashSet<Location>();
            foreach (var pt in m_Unverified) goals.Add(new Location(m_Zone.m, pt));
            return (m_Actor.Controller as ObjectiveAI)!.BehaviorPathTo(goals); // would need value-copy anyway of goals
        }
    }
}
