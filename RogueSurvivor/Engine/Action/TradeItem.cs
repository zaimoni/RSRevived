using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    public sealed class TradeItem : ActorAction, ActorGive, ActorTake, TargetActor
    {
        private readonly Item m_TakeItem;
        private readonly Item m_GiveItem;
        private readonly Data.Model.InvOrigin m_dest;
        [NonSerialized] private bool m_Crouching = false;

        public Item Give { get => m_GiveItem; }
        public Item Take { get => m_TakeItem; }
        public Actor? Whom { get => m_dest.a_owner; }

        private TradeItem(Actor actor, Item give, Item take, in Data.Model.InvOrigin dest) : base(actor)
        {
#if DEBUG
            if (null == give) throw new ArgumentNullException(nameof(give));
            if (null == take) throw new ArgumentNullException(nameof(take));
#endif
            m_GiveItem = give;
            m_TakeItem = take;
            m_dest = dest;
        }

        public override bool IsLegal()
        {
            return m_Actor.IsCarrying(m_GiveItem) && m_dest.IsCarrying(m_TakeItem);
        }

        public override bool IsPerformable()
        {
            var dist = Rules.GridDistance(m_Actor.Location, m_dest.Location);
            if (null != m_dest.a_owner) {
              if (!Rules.IsAdjacent(m_Actor.Location, m_dest.Location)) return false;
            } else if (1 < dist) return false;
            if (!base.IsPerformable()) return false;
            m_Crouching = 1 == dist && m_dest.IsGroundInventory;
            if (m_Crouching)
            {
                if (!m_Actor.CanCrouch(out m_FailReason)) return false;
                var actor = m_dest.Location.Actor;
                if (null != actor)
                {
                    var allies = m_Actor.Allies;
                    if (null == allies) return false;
                    if (!allies.Contains(actor)) return false;
                }
            }
            return true;
        }

        public override void Perform() {
            Data.Model.InvOrigin src = new(m_Actor);
            src.RejectCrossLink(m_dest);

            m_Actor.Activity = Activity.IDLE;
            m_Actor.SpendActionPoints();
            if (m_Crouching) {
                m_Actor.Crouch();
                var code = m_dest.Location.OnReachInto(m_Actor);
                if (0 >= code) return; // we took a hit -- cancel taking item
            } else if (null != m_dest.obj_owner) {
                // need to stand to make this work
                m_Actor.StandUp();
            }
            src.Trade(m_GiveItem, m_TakeItem, m_dest);

            RogueGame.Game.UI_TradeItem(m_Actor, m_GiveItem, m_TakeItem);
        }

        static public TradeItem? Cast(in Data.Model.InvOrigin dest, Actor actor, Item give, Item take)
        {
            var stage = new TradeItem(actor, give, take, in dest);
            if (!stage.IsPerformable()) return null;
            return stage;
        }

        static public TradeItem? Cast(Location loc, Actor actor, Item give, Item take)
        {
            if (loc.MapObject is ShelfLike obj) {
                if (obj.IsCarrying(take)) return Cast(new Data.Model.InvOrigin(obj), actor, give, take);
            }
            var a = loc.Actor;
            if (!a?.IsPlayer ?? false) {
                if (a.IsCarrying(take)) return Cast(new Data.Model.InvOrigin(a), actor, give, take);
            }

            var g_inv = loc.Items;
            if (g_inv?.Contains(take) ?? false) return Cast(new Data.Model.InvOrigin(loc), actor, give, take);
#if DEBUG
            throw new InvalidOperationException("tracing"); // need to verify null return
#endif
            return null;
        }

        static public TradeItem? ScheduleCast(in Data.Model.InvOrigin dest, Actor actor, Item give, Item take)
        {
            var stage = new TradeItem(actor, give, take, in dest);
            if (!stage.IsLegal()) return null;
            return stage;
        }

#if PROTOTYPE
        static public TradeItem? ScheduleCast(Location loc, Actor actor, Item give, Item take)
        {   // if we need this, adapt implementation of Cast
        }
#endif
    }
}
