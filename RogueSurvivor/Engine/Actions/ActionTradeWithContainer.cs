using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    internal class ActionTradeWithContainer : ActorAction
    {
        private readonly Location m_Location;   // savefile break \todo respecify to MapObject
        private readonly Item m_TakeItem;
        private readonly Item m_GiveItem;

        public ActionTradeWithContainer(Actor actor, Item give, Item take, Location loc)
        : base(actor)
        {
#if DEBUG
            if (null == give) throw new ArgumentNullException(nameof(give));
            if (null == take) throw new ArgumentNullException(nameof(take));
#endif
            if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc), loc, "non-canonical");
            m_Location = loc;
            m_GiveItem = give;
            m_TakeItem = take;
            actor.Activity = Activity.IDLE;
        }

        public ActionTradeWithContainer(Actor actor, Item give, Item take, Point pt)
        : this(actor, give, take, new Location(actor.Location.Map, pt)) {}

        public Item Give { get { return m_GiveItem; } }
        public Item Take { get { return m_TakeItem; } }

        public override bool IsLegal()
        {
            if (m_Location.Items?.Contains(m_GiveItem) ?? false) return false;
            if (m_Actor.Inventory.Contains(m_TakeItem)) return false;
            return true;    // XXX implement this correctly at some point
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return Rules.IsAdjacent(m_Actor.Location, in m_Location);
        }

        public override void Perform()
        {
            RogueForm.Game.DoTradeWithContainer(m_Actor,in m_Location,m_GiveItem,m_TakeItem);
        }
    }
}
