using System;
using System.Diagnostics;

#nullable enable

namespace djack.RogueSurvivor.Data.Model
{
    [Serializable]
    public readonly record struct InvOrigin : IInventory, ILocation_readonly
    {
        public readonly Actor? a_owner = null;
        public readonly ShelfLike? obj_owner = null;
        public readonly Location? loc = null;

        public InvOrigin(Actor owner)
        {
#if DEBUG
            a_owner = owner ?? throw new ArgumentNullException(nameof(owner));
            if (null == owner.Inventory) throw new ArgumentNullException("owner.Inventory");
#else
            a_owner = owner;
#endif
        }

        public InvOrigin(ShelfLike owner)
        {
#if DEBUG
            obj_owner = owner ?? throw new ArgumentNullException(nameof(owner));
#else
            obj_owner = owner;
#endif
        }

        public InvOrigin(Location src, Actor agent) : this(src)
        {
#if DEBUG
            a_owner = agent ?? throw new ArgumentNullException(nameof(agent));
            if (null == agent.Inventory) throw new ArgumentNullException("agent.Inventory");
#else
            a_owner = agent;
#endif
        }

        public InvOrigin(Location src) => loc = src;

        public InvOrigin(Actor? _a_owner, ShelfLike? _obj_owner, Location? _loc)
        {
            a_owner = _a_owner;
            obj_owner = _obj_owner;
            loc = _loc;
        }

        private Data.IInventory? IInv { get {
          if (null != a_owner) return a_owner;
          if (null != obj_owner) return obj_owner;
          return null;
        } }

        public Data.Inventory? Inventory { get {
            if (null != a_owner) return a_owner.Inventory;
            if (null != obj_owner) return obj_owner.Inventory;
            if (null != loc) return loc.Value.Items;
            throw new InvalidOperationException("tracing");
        } }

        public Location Location { get {
            if (null != loc) return loc.Value;
            if (null != a_owner) return a_owner.Location;
            if (null != obj_owner) return obj_owner.Location;
            throw new InvalidOperationException("tracing");
        } }

        public bool IsGroundInventory { get => null == a_owner && null == obj_owner; }

        public bool Stance(Actor a) {
            var dist = Engine.Rules.GridDistance(a.Location, Location);
            if (1 != dist) return true; // not relevant
            if (IsGroundInventory) {
                a.Crouch();
                var code = Engine.RogueGame.Game.OnActorReachIntoTile(a, Location);
                if (0 >= code) return false; // we took a hit -- cancel taking item
            } else if (null != obj_owner) {
                // need to stand to make this work
                a.StandUp();
            }
            return true;
        }

        // not really...some extra conditions involved
        public bool IsAccessible(Location origin)
        {
            if (1 == Engine.Rules.GridDistance(origin, Location)) return true;
            if (null != loc) return loc.Value == origin;
            return false;
        }

        public bool Exists { get {
            if (null != loc) return null != loc.Value.Items;
            if (null != obj_owner) return obj_owner.Location.MapObject == obj_owner;
            // must be actor inventory.
            return !a_owner!.IsDead;
        } }

        public void fireChange() {
            // Police ai cheats
            if (null == a_owner || !a_owner.IsFaction(GameFactions.IDs.ThePolice)) {
                if (null != loc) Engine.Session.Get.Police.Investigate.Record(loc.Value);
                if (null != obj_owner) Engine.Session.Get.Police.Investigate.Record(obj_owner.Location);
            }
        }

        // start change targets for centralized actor handling
        public bool CanAddAll(Data.Item it)
        {
            if (null != a_owner) return a_owner.CanTake(it);
            var inv = Inventory;
            if (null != inv) return inv.CanAddAll(it);
            return null != loc;
        }

        public bool AddAll(Data.Item it)
        {
            if (null != a_owner) return a_owner.Take(it);
            var inv = Inventory;
            if (null != inv) return inv.AddAll(it);
            if (null != loc) {
                loc.Value.Drop(it);
                return true;
            }
            return false;
        }

        int AddAsMuchAsPossible(Data.Item it) {
            if (null != a_owner) return a_owner.TakeAsMuchAsPossible(it);
            var inv = Inventory;
            if (null != inv) return inv.AddAsMuchAsPossible(it);
            if (null != loc) {
                loc.Value.Drop(it);
                return it.Quantity;
            }
            return 0;
        }

        public bool IsCarrying(Data.Item it) {
            var iinv = IInv;
            if (null != iinv) return iinv.IsCarrying(it);

            return Inventory?.Contains(it) ?? false;
        }

        public void Remove(Data.Item it, bool canMessage = true)
        {
            var iinv = IInv;
            if (null != iinv) {
                iinv.Remove(it, canMessage);
                return;
            }

            loc.Value.Map.RemoveAt(it, loc.Value.Position);
        }

        public bool CanTake(Data.Item it) {
            var iinv = IInv;
            if (null != iinv) return iinv.CanTake(it);

            return Inventory?.CanAddAll(it) ?? null != loc;
        }

        public bool Take(Data.Item it) {
            var iinv = IInv;
            if (null != iinv) return iinv.Take(it);
            if (null != loc) {
                loc.Value.Drop(it);
                return true;
            }
            return false;
        }

        public Data.Item? GetFirst(Gameplay.Item_IDs id)
        {
            var inv = Inventory;
            Data.Item? ret = inv?.GetFirst(id);
            if (null != ret) return ret;
            // auto-equip items path. non-autoequip items would be handled before checking hammerspace inventory
            if (null != a_owner) {
                var slots = a_owner.InventorySlots;
                if (null != slots) return slots.GetFirst(id);
            }
            return null;
        }

        public bool Transfer(Data.Item it, InvOrigin dest) {
          if (dest.CanTake(it)) {
            Remove(it);
            dest.Take(it);
            return true;
          }
          dest.AddAsMuchAsPossible(it);
          return false;
        }
        // end change targets for centralized actor handling

        public void Trade(Data.Item give, Data.Item take, InvOrigin dest)
        {
            dest.Remove(take);
            Remove(give);
            if (!give.IsUseless) dest.Take(give);
            Take(take);
        }

        [Conditional("DEBUG")]
        public void RejectCrossLink(Data.Inventory? other) {
            if (null != other) Inventory?.RejectCrossLink(other);
        }

        [Conditional("DEBUG")]
        public void RejectCrossLink(InvOrigin other) => RejectCrossLink(other.Inventory);

        public override string ToString()
        {
            if (null != obj_owner) return obj_owner.ToString();
            if (null != loc) return loc.Value.ToString();
            if (null != a_owner) return a_owner.Name;
            return "(invalid inventory source)";
        }
    }

  [Serializable]
  public readonly record struct InventorySource<T> where T:Data.Item
  {
     public readonly InvOrigin Origin;
     public readonly T it;

     public InventorySource(InvOrigin src, T obj) {
       Origin = src;
       it = obj
#if DEBUG
          ?? throw new ArgumentNullException(nameof(obj));
#endif
        ;
#if DEBUG
       var inv = src.Inventory;
       if (null == inv || !inv.Contains(obj)) throw new InvalidOperationException("!inv.Contains(obj)");
#endif
     }

    public override string ToString() => Origin.ToString() + " containing " + it.ToString();
  }
}
