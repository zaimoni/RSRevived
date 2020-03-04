using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    internal class ActionTradeWithContainer : ActorAction
    {
        private readonly Point m_Position;  // \savefile break: convert to location
        private readonly Item m_TakeItem;
        private readonly Item m_GiveItem;

        public ActionTradeWithContainer(Actor actor, Item give, Item take, Point position)
        : base(actor)
        {
#if DEBUG
            if (null == give) throw new ArgumentNullException(nameof(give));
            if (null == take) throw new ArgumentNullException(nameof(take));
#endif
#if TRACER
            if (actor.IsDebuggingTarget && Gameplay.GameItems.IDs.==give.Model.ID) throw new InvalidOperationException(give.ToString()+"; "+take.ToString());
#endif
            m_Position = position;
            m_GiveItem = give;
            m_TakeItem = take;
            actor.Activity = Activity.IDLE;
        }

        public Item Give { get { return m_GiveItem; } }
        public Item Take { get { return m_TakeItem; } }

        public override bool IsLegal()
        {
            if (m_Actor.Location.Map.GetItemsAtExt(m_Position)?.Contains(m_GiveItem) ?? false) return false;
            if (m_Actor.Inventory.Contains(m_TakeItem)) return false;
            return true;    // XXX implement this correctly at some point
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return Rules.IsAdjacent(m_Actor.Location.Position, in m_Position);
        }

        public override void Perform()
        {
            RogueForm.Game.DoTradeWithContainer(m_Actor,in m_Position,m_GiveItem,m_TakeItem);
        }
    }
}
