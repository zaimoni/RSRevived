﻿using System;

#nullable enable

namespace djack.RogueSurvivor.Data.Model
{
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
            a_owner = agent;
        }

        public InvOrigin(Location src) => loc = src;

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

        public override string ToString()
        {
            if (null != obj_owner) return obj_owner.ToString();
            if (null != loc) return loc.Value.ToString();
            if (null != a_owner) return a_owner.Name;
            return "(invalid inventory source)";
        }
    }
}
