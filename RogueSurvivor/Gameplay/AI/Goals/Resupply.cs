using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    internal class Resupply : Objective, LatePathable
    {
        private readonly Item_IDs[] critical;
        private readonly Location[] dests;
        [NonSerialized] private ObjectiveAI oai;

        public Resupply(Actor who, IEnumerable<Item_IDs> what, IEnumerable<Location> head_for) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            if (!(who.Controller is ObjectiveAI ai)) throw new InvalidOperationException("need an ai with inventory");
            oai = ai;

            critical = what.ToArray();
            dests = head_for.ToArray();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            oai = (m_Actor.Controller as ObjectiveAI)!;
        }

        private bool desires_changed(HashSet<Item_IDs> src) {
            if (critical.Length != src.Count) return true;
            foreach (var x in critical) if (!src.Contains(x)) return true;
            var seen = oai.items_in_FOV;
            if (null != seen) {
                foreach (var x in seen) {
                    foreach (var it in critical) {
                        if (x.Value.Has(it)) return true;
                    }
                }
            }
            return false;
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;

            if (m_Actor.Controller.IsEngaged) { // expire now
                _isExpired = true;
                return true;
            }
            if (0 <= Array.IndexOf(dests, m_Actor.Location)) {
                _isExpired = true;
                return true;
            }

            var items = oai.WhatHaveISeen();
            var need = oai.WhatDoINeedNow();
            var want = oai.WhatDoIWantNow();

            need.IntersectWith(items!);
            want.IntersectWith(items!);
            if (0 == need.Count) {
                if (desires_changed(want)) {
                    _isExpired = true;
                    return true;
                }
            } else {
                if (0 < want.Count) need.UnionWith(want);
                if (desires_changed(need)) {
                    _isExpired = true;
                    return true;
                }
            }

            // let other AI processing kick in before final pathing
            return false;
        }


        public ActorAction? Pathing()
        {
            var _locs = dests.Where(loc => !loc.StrictHasActorAt);
            if (!_locs.Any()) return null;

            var ret = oai.BehaviorPathTo(new HashSet<Location>(_locs));
            return (ret?.IsPerformable() ?? false) ? ret : null;
        }
    }
}
