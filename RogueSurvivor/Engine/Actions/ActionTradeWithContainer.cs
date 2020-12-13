﻿using System;
using djack.RogueSurvivor.Data;

using OrderableAI = djack.RogueSurvivor.Gameplay.AI.OrderableAI;
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

    internal interface TargetActor
    {
        public Actor Whom { get; }
    }

    internal interface TargetCorpse
    {
        public Corpse What { get; }
    }

    internal interface Target<out T> where T:MapObject
    {
        public T What { get; }
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
            if (null != obj && obj.IsContainer && obj.Inventory.Contains(take)) return new ActionTradeWithContainer(actor, give, take, obj);
            var g_inv = loc.Items;
            if (null != g_inv && g_inv.Contains(take)) return new ActionTradeWithGround(actor, give, take, loc);
            var a_inv = loc.Actor?.Inventory;
            if (null != a_inv && a_inv.Contains(take)) return new ActionTradeWithActor(actor, give, take, loc.Actor);
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
        private readonly MapObject m_obj;

        public ActionTradeWithContainer(Actor actor, Item give, Item take, MapObject obj) : base(actor, give, take)
        {
#if DEBUG
            var inv = obj.Inventory;
            if (null == inv || inv.Contains(m_GiveItem) || !inv.Contains(m_TakeItem)) throw new ArgumentNullException(nameof(obj)+".Inventory");
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
            return m_Actor.Location == m_Location;
        }

        public override void Perform()
        {
            RogueGame.Game.DoTradeWithGround(m_Actor, in m_Location, m_GiveItem, m_TakeItem);
        }
    }

    [Serializable]
    internal class ActionTradeWithActor : ActionTradeWith,TargetActor
    {
        private readonly Actor m_Whom;
        public Actor Whom { get { return m_Whom;  } }

        public ActionTradeWithActor(Actor actor, Item give, Item take, Actor whom) : base(actor, give, take)
        {
#if DEBUG
            var a_inv = whom.Inventory;
            if (null == a_inv || a_inv.Contains(m_GiveItem) || !a_inv.Contains(m_TakeItem)) throw new ArgumentNullException(nameof(whom) + ".Inventory");
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
