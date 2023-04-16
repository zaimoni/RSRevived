using System;
using System.Collections.Generic;

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class TakeFromLocation : ActorAction,Zaimoni.Data.BackwardPlan<Actions.ActionMoveDelta>    // similar to ActionTake
    {
        private readonly Gameplay.GameItems.IDs m_ID;
        private readonly Location m_loc;    // ground inventory; mapobject would be a different class once fully developed
        [NonSerialized] private Item? m_Item;

        public TakeFromLocation(Actor actor, Gameplay.GameItems.IDs id, Location loc) : base(actor)
        {
            if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc), loc, "has no canonical form");
            m_ID = id;
            m_loc = loc;
        }

        public override bool IsLegal()
        {
            var item_memory = m_Actor.Controller.ItemMemory;
            if (null != item_memory) return item_memory.WhatIsAt(m_loc)?.Contains(m_ID) ?? false;
            if (m_Actor.Controller.CanSee(m_loc)) return m_loc.Items?.Has(m_ID) ?? false;
            return true;
        }

        public override bool IsPerformable() {
            if (!IsLegal()) return false;
            var stacks = Map.GetAccessibleInventorySources(m_Actor.Location);
            if (null == stacks) return false;
            foreach (var stack in stacks) {
                if (null != stack.loc && stack.loc.Value == m_loc) {
                    m_Item = stack.inv.GetBestDestackable(Gameplay.GameItems.From(m_ID));
                    return null != m_Item;
                }
            }
            return false;
        }

        public override void Perform()
        {
            RogueGame.Game.DoTakeItem(m_Actor, m_loc, m_Item!); // \todo fix should be the more general handling
        }

        public override bool AreEquivalent(ActorAction? src)
        {
            return src is TakeFromLocation alt && m_ID == alt.m_ID && m_loc == alt.m_loc;
        }

        public override bool Abort() { return m_Actor.Controller.CanSee(m_loc); }   // fail over to in-sight item processing

        private List<Location> origin_range {
            get {
                var ret = new List<Location>();
                if (m_Actor.CanEnter(m_loc)) ret.Add(m_loc);    // future-proofing
                // handle containers (will go obsolete eventually)
                var obj = m_loc.MapObject as ShelfLike;
                if (null != obj) {
                    foreach (var pt in m_loc.Position.Adjacent()) {
                        var test = new Location(m_loc.Map, pt);
                        if (m_Actor.CanEnter(ref test)) ret.Add(test);
                    }
                }
                return ret;
            }
        }

        public List<Actions.ActionMoveDelta>? prequel()
        {
            List<Actions.ActionMoveDelta>? ret = null;
            var ok_dest = origin_range;
            foreach (var dest in ok_dest) {
                foreach (var pt in dest.Position.Adjacent()) {
                    var test = new Location(m_loc.Map, pt);
                    if (m_Actor.CanEnter(ref test) && !ok_dest.Contains(test))
                        (ret ??= new List<Actions.ActionMoveDelta>()).Add(new Actions.ActionMoveDelta(m_Actor, dest, test));
                }
            }
            return ret;
        }
    }
}

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class TakeFromLocation_memory : WorldUpdate    // similar to ActionTake
    {
        private readonly Gameplay.GameItems.IDs m_ID;
        private readonly Location m_loc;    // ground inventory; mapobject would be a different class once fully developed
        private readonly Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> m_memory;
        [NonSerialized] private Location[]? m_origin;
        [NonSerialized] private Item? m_Item;

        public TakeFromLocation_memory(Gameplay.GameItems.IDs id, Location loc, Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items)
        {
            if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc), loc, "has no canonical form");
            m_ID = id;
            m_loc = loc;
            m_memory = items;
        }

        public override bool IsLegal() { return m_memory.WhatIsAt(m_loc)?.Contains(m_ID) ?? false; }
        public override bool IsRelevant() { return false; }
        public override bool IsRelevant(Location loc) { return 0 <= Array.IndexOf(m_origin ??= origin_range, loc); }

        public override bool IsSuppressed(Actor a)
        {   // \todo more sophisticated response to enemies in sight
            return null != a.Controller.enemies_in_FOV;
        }

        public override ActorAction? Bind(Actor src) {
            var act = new _Action.TakeFromLocation(src, m_ID, m_loc);
            return act.IsPerformable() ? act : null;
        }

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = Bind(src);
            if (null == act) return null;
            return new(act, null);
        }

        public override void Blacklist(HashSet<Location> goals) {} // intentional no-op

        public override void Goals(HashSet<Location> goals)
        {
            goals.UnionWith(m_origin ??= origin_range);
        }

        private Location[] origin_range {
            get {
                var ret = new List<Location>();
                if (m_loc.TileModel.IsWalkable) ret.Add(m_loc);    // future-proofing
                // handle containers (will go obsolete eventually)
                var obj = m_loc.MapObject as ShelfLike;
                if (null != obj) {
                    foreach (var pt in m_loc.Position.Adjacent()) {
                        var test = new Location(m_loc.Map, pt);
                        if (Map.Canonical(ref test) && test.TileModel.IsWalkable) ret.Add(test);
                    }
                }
                return ret.ToArray();
            }
        }
    }
}
