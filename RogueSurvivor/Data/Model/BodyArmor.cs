namespace djack.RogueSurvivor.Data.Model
{
    public sealed class BodyArmor : Item
    {
        public readonly int Protection_Hit;
        public readonly int Protection_Shot;
        public readonly int Encumbrance;
        public readonly int Weight;

        public BodyArmor(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int protection_hit, int protection_shot, int encumbrance, int weight, string flavor)
          : base(_id, aName, theNames, imageID, flavor, DollPart.TORSO)
        {
            Protection_Hit = protection_hit;
            Protection_Shot = protection_shot;
            Encumbrance = encumbrance;
            Weight = weight;
        }

        public Defence ToDefence() => new Defence(-Encumbrance, Protection_Hit, Protection_Shot);
        public int Rating { get => Protection_Hit + Protection_Shot; }

        public override Engine.Items.ItemBodyArmor create() => new(this);
    }
}
