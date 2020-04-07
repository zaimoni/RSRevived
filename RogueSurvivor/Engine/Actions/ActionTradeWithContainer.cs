using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
    internal interface ActorGive {
        public Item Give { get; }
    }

    internal interface ActorTake
    {
        public Item Take { get; }
    }

    [Serializable]
    internal abstract class ActionTradeWith : ActorAction,ActorGive,ActorTake
    {
        protected readonly Item m_TakeItem;
        protected readonly Item m_GiveItem;

        public ActionTradeWith(Actor actor, Item give, Item take) : base(actor)
        {
#if DEBUG
            if (null == give || !m_Actor.Inventory.Contains(give)) throw new ArgumentNullException(nameof(give));
            if (null == take || m_Actor.Inventory.Contains(take)) throw new ArgumentNullException(nameof(take));
#endif
            m_GiveItem = give;
            m_TakeItem = take;
            actor.Activity = Activity.IDLE;
        }

        public Item Give { get { return m_GiveItem; } }
        public Item Take { get { return m_TakeItem; } }

        public override bool IsLegal()
        {
            if (!m_Actor.Inventory.Contains(m_GiveItem)) return false;
            if (m_Actor.Inventory.Contains(m_TakeItem)) return false;
            return true;
        }

        public override abstract void Perform();

        static public ActionTradeWith Cast(Location loc, Actor actor, Item give, Item take)
        {
            var obj = loc.MapObject;
            if (null != obj && obj.IsContainer) new ActionTradeWithContainer(actor, give, take, loc);
            return new ActionTradeWithGround(actor, give, take, loc);
        }

        static public ActionTradeWith Cast(Point pt, Actor actor, Item give, Item take)
        {
            return Cast(new Location(actor.Location.Map, pt), actor, give, take);
        }
    }


    [Serializable]
    internal class ActionTradeWithContainer : ActionTradeWith
    {
        private readonly Location m_Location;   // savefile break \todo respecify to MapObject

        public ActionTradeWithContainer(Actor actor, Item give, Item take, Location loc) : base(actor, give, take)
        {
#if DEBUG
            var g_inv = loc.Items;
            if (null == g_inv || g_inv.Contains(m_GiveItem) || !g_inv.Contains(m_TakeItem)) throw new ArgumentNullException(nameof(loc)+".Items");
#endif
            if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc), loc, "non-canonical");
            m_Location = loc;
            actor.Activity = Activity.IDLE;
        }

        public override bool IsLegal()
        {
            var g_inv = m_Location.Items;
            if (null == g_inv || g_inv.Contains(m_GiveItem) || !g_inv.Contains(m_TakeItem)) return false;
            return base.IsLegal();
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

    [Serializable]
    internal class ActionTradeWithGround : ActionTradeWith
    {
        private readonly Location m_Location;

        public ActionTradeWithGround(Actor actor, Item give, Item take, Location loc) : base(actor, give, take)
        {
#if DEBUG
            var g_inv = loc.Items;
            if (null == g_inv || g_inv.Contains(m_GiveItem) || !g_inv.Contains(m_TakeItem)) throw new ArgumentNullException(nameof(loc) + ".Items");
#endif
            if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc), loc, "non-canonical");
            m_Location = loc;
            actor.Activity = Activity.IDLE;
        }

        public override bool IsLegal()
        {
            var g_inv = m_Location.Items;
            if (null == g_inv || g_inv.Contains(m_GiveItem) || !g_inv.Contains(m_TakeItem)) return false;
            return base.IsLegal();
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return Rules.IsAdjacent(m_Actor.Location, in m_Location);
        }

        public override void Perform()
        {
            RogueForm.Game.DoTradeWithContainer(m_Actor, in m_Location, m_GiveItem, m_TakeItem);
        }
    }
}
