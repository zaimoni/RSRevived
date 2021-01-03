using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine._Goal
{
    internal class CoverTrap : Condition<Actor>
    {
        private Location m_Location;

        CoverTrap(Location loc) { m_Location = loc; }

        public static bool IsDone(in Location loc) {
            if (loc.Map.IsTrapCoveringMapObjectAt(loc.Position)) return true;
            return null == loc.Items?.GetItemsByType<ItemTrap>(trap => trap.IsActivated && 0 < trap.Model.Damage);
        }

        public static bool IsDone(in Location loc, Actor viewpoint) {
            int trapsMaxDamage = loc.Map.TrapsUnavoidableMaxDamageAtFor(loc.Position, viewpoint);
            return 0 >= trapsMaxDamage;
        }

#region Condition<Actor> implementation
        public bool IsDone() { return IsDone(in m_Location); }
        public int StatusCode() { return IsDone() ? 0 : 1; }
        public bool IsDone(Actor viewpoint) { return IsDone(in m_Location, viewpoint); }
        public int StatusCode(Actor viewpoint) { return IsDone(viewpoint) ? 0 : 1; }
#endregion
    }
}
