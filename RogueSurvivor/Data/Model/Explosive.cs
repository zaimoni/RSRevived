using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Data.Model
{
    public class Explosive : Item
    {
        public readonly int FuseDelay;
        private readonly BlastAttack m_Attack;
        public readonly PrimedExplosive Primed;
        public readonly int MaxThrowDistance;

        public ref readonly BlastAttack BlastAttack { get { return ref m_Attack; } }

        public Explosive(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string flavor, PrimedExplosive primed, int maxThrow, int stackingLimit)
        : base(_id, aName, theNames, imageID, flavor, DollPart.RIGHT_HAND)
        {
            FuseDelay = fuseDelay;
            m_Attack = attack;
            Primed = primed;
            MaxThrowDistance = maxThrow;
            StackingLimit = stackingLimit;
        }

        // formally incorrect, but we only have grenades
        public override ItemGrenade create() { return new ItemGrenade(this); }
    }
}
