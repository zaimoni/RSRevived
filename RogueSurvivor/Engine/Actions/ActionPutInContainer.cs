using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
    internal class ActionPutInContainer : ActorAction,ActorGive,Target<MapObject>
    {
        private readonly Item m_Item;
        private readonly MapObject m_Container;

        public Item Give { get { return m_Item; } }
        public MapObject What { get { return m_Container; } }

        public ActionPutInContainer(Actor actor, Item it, MapObject container) : base(actor)
        {
            m_Item = it;
            m_Container = container;
            actor.Activity = Activity.IDLE;
        }

        public override bool IsLegal()
        {
            m_FailReason = m_Container?.ReasonCantPutItemIn(m_Actor) ?? "object is not a container";
            return string.IsNullOrEmpty(m_FailReason);
        }

        public override bool IsPerformable()
        {
            if (1 != Rules.InteractionDistance(m_Actor.Location, m_Container.Location)) return false;
            return base.IsPerformable();
        }

        public override void Perform()
        {
            RogueForm.Game.DoPutItemInContainer(m_Actor, m_Container, m_Item);
        }
    }
}
