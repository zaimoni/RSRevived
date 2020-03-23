using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class TakeFromLocation : ActorAction    // similar to ActionTake
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
            var stacks = m_Actor.Location.Map.GetAccessibleInventories(m_Actor.Location.Position);
            if (0 >= stacks.Count) return false;
            var denorm = m_Actor.Location.Map.Denormalize(in m_loc);
            if (null == denorm || !stacks.ContainsKey(denorm.Value.Position)) return false;
            m_Item = m_loc.Items.GetBestDestackable(Models.Items[(int)m_ID]);
            return true;
        }

        public override void Perform()
        {
            m_Actor.Inventory.RejectCrossLink(m_loc.Items!);
            RogueForm.Game.DoTakeItem(m_Actor, m_loc, m_Item!);
            m_loc.Items?.RejectCrossLink(m_Actor.Inventory);
        }

        public override bool AreEquivalent(ActorAction? src)
        {
            return src is TakeFromLocation alt && m_ID == alt.m_ID && m_loc == alt.m_loc;
        }

        public List<Location> origin_range {
            get {
                var ret = new List<Location> { m_loc };
                // handle containers (will go obsolete eventually)
                var obj = m_loc.MapObject;
                if (null != obj && obj.IsContainer) {
                    foreach (var pt in m_loc.Position.Adjacent()) {
                        var test = new Location(m_loc.Map, pt);
                        if (Map.Canonical(ref test)) ret.Add(test);
                    }
                }
                return ret;
            }
        }

    }
}
