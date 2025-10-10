using System;

#nullable enable

namespace djack.RogueSurvivor.Data.Model
{
    public class Item : Zaimoni.Data.Factory<Data.Item>
    {
        public readonly Gameplay.Item_IDs ID;
        public readonly string SingleName;
        public readonly string PluralName;
        public readonly string ImageID;
        public readonly string FlavorDescription;
        public readonly DollPart EquipmentPart;
        public readonly bool DontAutoEquip;
        public readonly bool IsForbiddenToAI;

        private int m_StackingLimit = 1;
        private bool m_Artifact = false;

        public bool IsPlural { get; protected set; }
        public bool IsProper { get => m_Artifact; } // XXX will have to fix this eventually
        public bool IsUnbreakable {
            get => m_Artifact;
            protected set {
                m_Artifact = value;
                IsUnique = true;
            }
        } // artifacts
        public bool IsUnique { get; set; }

        public int StackingLimit
        {
            get => m_StackingLimit;
            protected set
            {
#if DEBUG
                if (0 >= value) throw new ArgumentOutOfRangeException(nameof(value), value, "0 >= value");
#endif
                m_StackingLimit = value;
            }
        }

        public bool IsStackable { get => 2 <= m_StackingLimit; }
        public bool IsEquipable { get => EquipmentPart != DollPart.NONE; }
        public bool CanStackMore(int qty) => 2 <= m_StackingLimit && qty < m_StackingLimit;

        public Item(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, string flavor = "", DollPart part = DollPart.NONE, bool no_autoequip = false)
        {
            ID = _id;
            SingleName = aName;
            PluralName = theNames;
            ImageID = imageID;
            FlavorDescription = flavor;
            // if we are not equippable, then there is no operational difference whether we auto-equip or not.
            DontAutoEquip = DollPart.NONE == (EquipmentPart = part) || no_autoequip;

            IsForbiddenToAI = _id == Gameplay.Item_IDs.UNIQUE_SUBWAY_BADGE;
        }

        public virtual Data.Item create() => throw new InvalidOperationException("override Model.Item::create to do anything useful");
        // C# makes the secure access level (private) a syntax error
        public virtual Data.Item from(in Item_s src) => new(this, src.QtyLike);
        public Data.Item From(Item_s src)  {
            if (src.ModelID != ID) throw new InvalidOperationException("tracing");
            return from(in src);
        }
    }
}
