using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace djack.RogueSurvivor.Data
{
    internal interface IInventory
    {
        public Inventory Inventory { get; }

        bool IsCarrying(Item it);
        bool IsCarrying(RogueSurvivor.Gameplay.Item_IDs it);

        void Remove(Item it, bool canMessage = true);
        bool CanTake(Item it);
        bool Take(Item it);

    }
}
