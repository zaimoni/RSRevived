using System;
using djack.RogueSurvivor.Data;

using OrderableAI = djack.RogueSurvivor.Gameplay.AI.OrderableAI;
using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    public abstract class ActionTradeWith : ActorAction,ActorGive,ActorTake
    {
        protected readonly Item m_TakeItem;
        protected readonly Item m_GiveItem;

        protected ActionTradeWith(Actor actor, Item give, Item take) : base(actor)
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

        static public ActionTradeWith Cast(in InvOrigin dest, Actor actor, Item give, Item take)
        {
            if (null == dest.inv || !dest.inv.Contains(take)) {
#if DEBUG
                throw new InvalidOperationException("cannot take from this inventory");
#endif
                return null;    // arguably invariant failure
            }

            if (null != dest.obj_owner) {
                // trade w/container
                return new ActionTradeWithContainer(actor, give, take, dest.obj_owner);
            }
            if (null != dest.loc) {
                return new ActionTradeWithGround(actor, give, take, dest.loc.Value);
            }
            if (null != dest.a_owner && !dest.a_owner.IsPlayer) {
                return new ActionTradeWithActor(actor, give, take, dest.a_owner);
            }
#if DEBUG
            throw new InvalidOperationException("tracing"); // need to verify null return
#endif
            return null;
        }

        static public ActionTradeWith Cast(Location loc, Actor actor, Item give, Item take)
        {
            var obj = loc.MapObject as ShelfLike;
            var obj_inv = obj?.NonEmptyInventory;
            if (null != obj_inv && obj_inv.Contains(take)) return new ActionTradeWithContainer(actor, give, take, obj);
            var g_inv = loc.Items;
            if (null != g_inv && g_inv.Contains(take)) return new ActionTradeWithGround(actor, give, take, loc);
            var a_inv = loc.Actor?.Inventory;
            if (null != a_inv && a_inv.Contains(take) && !loc.Actor.IsPlayer) return new ActionTradeWithActor(actor, give, take, loc.Actor);
#if DEBUG
            throw new InvalidOperationException("tracing"); // need to verify null return
#endif
            return null;
        }

        static public ActionTradeWith Cast(Point pt, Actor actor, Item give, Item take)
        {
            return Cast(new Location(actor.Location.Map, pt), actor, give, take);
        }
    }

    [Serializable]
    internal class ActionTradeWithContainer : ActionTradeWith,Target<MapObject>
    {
        private readonly ShelfLike m_obj;

        public ActionTradeWithContainer(Actor actor, Item give, Item take, ShelfLike obj) : base(actor, give, take)
        {
#if DEBUG
            var inv = obj.Inventory;
            if (inv.Contains(m_GiveItem) || !inv.Contains(m_TakeItem)) throw new ArgumentNullException(nameof(obj)+".Inventory");
#endif
            m_obj = obj;
            actor.Activity = Activity.IDLE;
        }

        public MapObject What { get { return m_obj; } }

        public override bool IsLegal()
        {
            var inv = m_obj.Inventory;
            if (null == inv || inv.Contains(m_GiveItem) || !inv.Contains(m_TakeItem)) return false;
            return base.IsLegal();
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return m_Actor.MayTakeFromStackAt(m_obj.Location);
        }

        public override void Perform()
        {
            RogueGame.Game.DoTradeWithContainer(m_Actor,in m_obj,m_GiveItem,m_TakeItem);
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
            return 1 >= Rules.GridDistance(m_Actor.Location, m_Location);
        }

        public override void Perform()
        {
            RogueGame.Game.DoTradeWithGround(m_Actor, in m_Location, m_GiveItem, m_TakeItem);
        }
    }

    [Serializable]
    public sealed class ActionTradeWithActor : ActionTradeWith,TargetActor
    {
        private readonly Actor m_Whom;
        public Actor Whom { get { return m_Whom;  } }

        public ActionTradeWithActor(Actor actor, Item give, Item take, Actor whom) : base(actor, give, take)
        {
#if DEBUG
            var a_inv = whom.Inventory;
            if (null == a_inv || a_inv.Contains(m_GiveItem) || !a_inv.Contains(m_TakeItem)) throw new ArgumentNullException(nameof(whom) + ".Inventory");
            if (!(whom.Controller is OrderableAI)) throw new ArgumentNullException("whom.Controller as OrderableAI");
#endif
            m_Whom = whom;
            actor.Activity = Activity.IDLE;
        }

        public override bool IsLegal()
        {
            if (m_Whom.Inventory.Contains(m_GiveItem)) return false;
            if (!m_Whom.Inventory.Contains(m_TakeItem)) return false;
            return base.IsLegal();
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return Rules.IsAdjacent(m_Actor.Location, m_Whom.Location);
        }

        public override void Perform()
        {
            RogueGame.Game.DoTrade(m_Actor.Controller as OrderableAI, m_Whom.Controller as OrderableAI, m_GiveItem, m_TakeItem);
        }
    }
}
