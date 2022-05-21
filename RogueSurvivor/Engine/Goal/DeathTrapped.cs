using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;

#nullable enable

namespace djack.RogueSurvivor.Engine.Goal
{
    [Serializable]
    internal class DeathTrapped : Objective
    {
        private List<Location> banned;

        public DeathTrapped(Actor who, in Location ban) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            banned = new() { ban };
        }

        // Influence on pathfinding, so never returns an action
        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;

            // Cf. Rules::IsPathableFor
            var ub = banned.Count;
            while (0 <= --ub) {
                if (1 < m_Actor.Controller.FastestTrapKill(banned[ub])) {
                    banned.RemoveAt(ub);    // no longer deathtrapped
                }
            }
            _isExpired = 0 >= banned.Count;
            return _isExpired;
        }

        /// <returns>true iff recorded</returns>
        public bool Ban(in Location ban) {
            var ret = !banned.Contains(ban);
            if (ret) banned.Add(ban);
            return ret;
        }

        public bool IsBanned(Location ban) {
            return banned.Contains(ban);
        }
    }
}
