using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
    internal class ActionPutInContainer : ActorAction
    {
        private readonly Point m_Position;
        private readonly Item m_Item;
        private MapObject m_Container = null;   // would be non-serialized

        public Item Item { get { return m_Item; } }

        public ActionPutInContainer(Actor actor, Item it, Point position)
        : base(actor)
        {
#if DEBUG
            if (null == it) throw new ArgumentNullException(nameof(it));
#endif
            m_Item = it;
            m_Position = position;
            actor.Activity = Activity.IDLE;
        }

        public override bool IsLegal()
        {
            m_Container = Rules.CanActorPutItemIntoContainer(m_Actor, in m_Position, out m_FailReason);
            return null != m_Container;
        }

        public override void Perform()
        {
            RogueForm.Game.DoPutItemInContainer(m_Actor,m_Container ?? Rules.CanActorPutItemIntoContainer(m_Actor, in m_Position, out m_FailReason),m_Item);
        }
    }
}
