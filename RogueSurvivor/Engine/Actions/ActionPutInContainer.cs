using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
    internal class ActionPutInContainer : ActorAction
    {
        private readonly Point m_Position;
        private readonly Item m_Item;

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
            return RogueForm.Game.Rules.CanActorPutItemIntoContainer(m_Actor, m_Position, out m_FailReason);
        }

        public override void Perform()
        {
            RogueForm.Game.DoPutItemInContainer(m_Actor,m_Position,m_Item);
        }
    }
}
