using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine._Goal
{
    internal class CoverTrap : Condition<Actor>
    {
        private Location[] m_Locations;

        public CoverTrap(IEnumerable<Location> locs) { m_Locations = locs.ToArray(); }

        public static bool IsDone(in Location loc) {
            if (loc.Map.IsTrapCoveringMapObjectAt(loc.Position)) return true;
            return null == loc.Items?.GetItemsByType<ItemTrap>(trap => trap.IsActivated && 0 < trap.Model.Damage);
        }

        public static bool IsDone(in Location loc, Actor viewpoint) {
            int trapsMaxDamage = loc.Map.TrapsUnavoidableMaxDamageAtFor(loc.Position, viewpoint);
            return 0 >= trapsMaxDamage;
        }

#region Condition<Actor> implementation
        public bool IsDone() {
            foreach (var loc in m_Locations) if (IsDone(in loc)) return true;
            return false;
        }

        public int StatusCode() { return IsDone() ? 0 : 1; }

        public bool IsDone(Actor viewpoint) {
            foreach (var loc in m_Locations) if (IsDone(in loc, viewpoint)) return true;
            return false;
        }

        public int StatusCode(Actor viewpoint) { return IsDone(viewpoint) ? 0 : 1; }
#endregion
    }
}
