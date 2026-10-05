using System;
using System.Collections.Generic;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine.Goal
{
    [Serializable]
    internal class DeathTrapped : Objective, IJsonStaged<DeathTrapped>
    {
        private List<Location> banned;

        #region JSON save/load

        private DeathTrapped(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

        {

            banned = src.Require("Banned").Deserialize<List<Location>>(options) ?? throw new JsonException("null Banned");

        }


        static DeathTrapped IJsonStaged<DeathTrapped>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new DeathTrapped(JsonActor(src), src, options));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            writer.WriteArray("Banned", banned, options);

        }

        #endregion


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
