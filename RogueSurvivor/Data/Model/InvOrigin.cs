using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace djack.RogueSurvivor.Data.Model
{
    [Serializable]
    public readonly record struct InvOrigin : IInventory, ILocation_readonly
    {
        public readonly LivingActor? a_owner = null;
        public readonly ShelfLike? obj_owner = null;
        public readonly Location? loc = null;

        public InvOrigin(LivingActor owner)
        {
#if DEBUG
            a_owner = owner ?? throw new ArgumentNullException(nameof(owner));
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

        public InvOrigin(Location src, LivingActor agent) : this(src)
        {
#if DEBUG
            a_owner = agent ?? throw new ArgumentNullException(nameof(agent));
#else
            a_owner = agent;
#endif
        }

        public InvOrigin(Location src) => loc = src;

        public InvOrigin(Location src, Data.Item it) {
            var shelf = src.MapObject as ShelfLike;
            var ginv = src.Items;
            if (shelf is null) {
                loc = src;
                return;
            }
            if (ginv is not null && ginv.Contains(it)) {
                loc = src;
                return;
            }
            // we prefer to stage to containers
            if (shelf.Inventory.Contains(it) || !shelf.Inventory.IsFull) {
                obj_owner = shelf;
                return;
            }
            loc = src;
            return;
        }

        public InvOrigin(LivingActor? _a_owner, ShelfLike? _obj_owner, Location? _loc)
        {
            a_owner = _a_owner;
            obj_owner = _obj_owner;
            loc = _loc;
        }

        private Data.IInventory? IInv { get {
          if (a_owner is not null) return a_owner;
          if (obj_owner is not null) return obj_owner;
          return null;
        } }

        public Data.Inventory? Inventory { get {
            if (a_owner is not null) return a_owner.Inventory;
            if (obj_owner is not null) return obj_owner.Inventory;
            if (loc is not null) return loc.Value.Items;
            throw new InvalidOperationException("tracing");
        } }

        public bool IsEmpty { get {
            if (a_owner is not null) return a_owner.Inventory.IsEmpty;
            if (obj_owner is not null) return obj_owner.Inventory.IsEmpty;
            if (loc is not null) {
                 if (loc.Value.Items is not null) return loc.Value.Items.IsEmpty;
            }
            return true;
        } }

        public Location Location { get {
            if (loc is not null) return loc.Value;
            if (a_owner is not null) return a_owner.Location;
            if (obj_owner is not null) return obj_owner.Location;
            throw new InvalidOperationException("tracing");
        } }

        public bool IsGroundInventory { get => a_owner is null && obj_owner is null; }

        public bool Stance(Actor a) {
            var dist = Engine.Rules.GridDistance(a.Location, Location);
            if (1 != dist) return true; // not relevant
            if (IsGroundInventory) {
                a.Crouch();
                var code = Location.OnReachInto(a);
                if (0 >= code) return false; // we took a hit -- cancel taking item
            } else if (obj_owner is not null) {
                // need to stand to make this work
                a.StandUp();
            }
            return true;
        }

        public bool IsAccessible(in Location origin)
        {
            if (1 == Engine.Rules.GridDistance(origin, Location)) {
                if (loc is not null) {
                  if (loc.Value.MapObject?.BlocksReachInto() ?? false) return false;
                }
                return true;
            }
            return origin == Location;
        }

        public bool Exists { get {
            if (loc is not null) return loc.Value.Items is not null;
            if (obj_owner is not null) return obj_owner.Location.MapObject == obj_owner;
            // must be actor inventory.
            return !a_owner!.IsDead;
        } }

        public void fireChange() {
            // Police ai cheats
            if (a_owner is null || !a_owner.IsFaction(GameFactions.IDs.ThePolice)) {
                if (loc is not null) Engine.Session.Get.Police.Investigate.Record(loc.Value);
                if (obj_owner is not null) Engine.Session.Get.Police.Investigate.Record(obj_owner.Location);
            }
        }

        // start change targets for centralized actor handling
        public bool CanAddAll(Data.Item it)
        {
            if (a_owner is not null) return a_owner.CanTake(it);
            var inv = Inventory;
            if (inv is not null) return inv.CanAddAll(it);
            return loc is not null;
        }

        public bool AddAll(Data.Item it)
        {
            if (a_owner is not null) return a_owner.Take(it);
            var inv = Inventory;
            if (inv is not null) return inv.AddAll(it);
            if (loc is not null) {
                loc.Value.Drop(it);
                return true;
            }
            return false;
        }

        int AddAsMuchAsPossible(Data.Item it) {
            if (a_owner is not null) return a_owner.TakeAsMuchAsPossible(it);
            var inv = Inventory;
            if (inv is not null) return inv.AddAsMuchAsPossible(it);
            if (loc is not null) {
                loc.Value.Drop(it);
                return it.Quantity;
            }
            return 0;
        }

        public bool IsCarrying(Data.Item it) {
            var iinv = IInv;
            if (iinv is not null) return iinv.IsCarrying(it);

            return Inventory?.Contains(it) ?? false;
        }

        public bool IsCarrying(Gameplay.Item_IDs id)
        {
            var iinv = IInv;
            if (iinv is not null) return iinv.IsCarrying(id);

            return Inventory?.Contains(id) ?? false;
        }

        public bool IsCarrying(IEnumerable<Gameplay.Item_IDs> src) {
            foreach(var id in src) if (IsCarrying(id)) return true;
            return false;
        }

        public void Remove(Data.Item it, bool canMessage = true)
        {
            var iinv = IInv;
            if (iinv is not null) {
                iinv.Remove(it, canMessage);
                return;
            }

            loc.Value.Map.RemoveAt(it, loc.Value.Position);
        }

        public bool CanTake(Data.Item it) {
            var iinv = IInv;
            if (iinv is not null) return iinv.CanTake(it);

            return Inventory?.CanAddAll(it) ?? loc is not null;
        }

        public bool Take(Data.Item it) {
            var iinv = IInv;
            if (iinv is not null) return iinv.Take(it);
            if (loc is not null) {
                loc.Value.Drop(it);
                return true;
            }
            return false;
        }

        public bool Has(Gameplay.Item_IDs id) => GetFirst(id) is not null;

        public Data.Item? GetFirst(Gameplay.Item_IDs id)
        {
            var inv = Inventory;
            Data.Item? ret = inv?.GetFirst(id);
            if (ret is not null) return ret;
            // auto-equip items path. non-autoequip items would be handled before checking hammerspace inventory
            if (a_owner is not null) {
                var slots = a_owner.InventorySlots;
                if (slots is not null) return slots.GetFirst(id);
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
            if (other is not null) Inventory?.RejectCrossLink(other);
        }

        [Conditional("DEBUG")]
        public void RejectCrossLink(InvOrigin other) => RejectCrossLink(other.Inventory);

        public override string ToString()
        {
            if (obj_owner is not null) return obj_owner.ToString();
            if (loc is not null) return loc.Value.ToString();
            if (a_owner is not null) return a_owner.Name;
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
       if (inv is null || !inv.Contains(obj)) throw new InvalidOperationException("!inv.Contains(obj)");
#endif
     }

    public override string ToString() => Origin.ToString() + " containing " + it.ToString();
  }
}

namespace djack.RogueSurvivor.Data {
  internal static partial class ext_RSRevived {
    public static Model.InvOrigin? NullIfEmpty(this Model.InvOrigin src) {
      return src.IsEmpty ? null : src;
    }
  }
}

namespace Zaimoni.JsonConvert
{
    using System.Text.Json;
    using Zaimoni.JSON;

    public sealed class InvOrigin : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Data.Model.InvOrigin>
    {
        public override djack.RogueSurvivor.Data.Model.InvOrigin Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            var stage = JsonElement.ParseValue(ref reader);
            var actor = stage.TryGetProperty("Actor", out var x) ? x.RequireRef<djack.RogueSurvivor.Data.LivingActor>() : null;
            var shelf = stage.TryGetProperty("Shelf", out x) ? x.RequireRef<djack.RogueSurvivor.Data.ShelfLike>() : null;
            djack.RogueSurvivor.Data.Location? loc = stage.TryGetProperty("Location", out x) ? x.Deserialize<djack.RogueSurvivor.Data.Location>(options) : null;
            if (actor is null && shelf is null && loc is null) throw new JsonException("inventory origin of nothing: " + stage.GetRawText());
            return new(actor, shelf, loc);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Data.Model.InvOrigin src, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (src.a_owner is not null) writer.WriteRef("Actor", src.a_owner);
            if (src.obj_owner is not null) writer.WriteRef("Shelf", src.obj_owner);
            if (src.loc is not null) {
                writer.WritePropertyName("Location");
                JsonSerializer.Serialize(writer, src.loc.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
