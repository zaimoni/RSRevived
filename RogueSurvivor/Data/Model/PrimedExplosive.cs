namespace djack.RogueSurvivor.Data.Model
{
    public class PrimedExplosive : Item
    {
        public readonly int FuseDelay;
        private readonly BlastAttack m_Attack;

        public ref readonly BlastAttack BlastAttack { get { return ref m_Attack; } }

        public PrimedExplosive(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string flavor)
        : base(_id, aName, theNames, imageID, flavor, DollPart.RIGHT_HAND)
        {
            FuseDelay = fuseDelay;
            m_Attack = attack;
        }

        public override Data._Item.PrimedExplosive from(in Item_s src) => new(Gameplay.GameItems.From((Gameplay.Item_IDs)((int)ID-1)) as Explosive, src.QtyLike);
    }
}
