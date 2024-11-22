using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace djack.RogueSurvivor.Data.Model
{
    public class MapObject
    {
        public const int CAR_WEIGHT = 100;
        public const int MAX_NORMAL_WEIGHT = 10;

        private static MapObject[] models = new MapObject[_COUNT];
        static public MapObject from(int n) { return models[n]; }

        public readonly IDs ID;
        public readonly byte Weight; // Weight is positive if and only if the object is movable
        public readonly Flags flags;
        public readonly int jumpLevel;
        public readonly int MaxHitPoints;

        [ModuleInitializer]
        internal static void init() {
            models = new MapObject[_COUNT];
            int ub = _COUNT;
            while(0 <= --ub) models[ub] = new MapObject(ub);
        }

        private MapObject(int id) {
            ID = (IDs)id;
            flags = default;
            Weight = _ID_Weight(ID);
            if (0 < Weight) flags |= Flags.IS_MOVABLE;
            if (_ID_GivesWood(ID)) flags |= Flags.GIVES_WOOD;
            if (_ID_StandOnFOVbonus(ID)) flags |= Flags.STANDON_FOV_BONUS;
            if (_ID_BreaksWhenFiredThrough(ID)) flags |= Flags.BREAKS_WHEN_FIRED_THROUGH;
            if (_ID_IsCouch(ID)) flags |= Flags.IS_COUCH;
            if (_ID_IsPlural(ID)) flags |= Flags.IS_PLURAL;
            if (_ID_MaterialIsTransparent(ID)) flags |= Flags.IS_MATERIAL_TRANSPARENT;
            if (_ID_IsWalkable(ID)) flags |= Flags.IS_WALKABLE;
            jumpLevel = _ID_Jumplevel(ID);
            MaxHitPoints = _ID_MaxHP(ID);

#if DEBUG
            if (default != (flags & Flags.STANDON_FOV_BONUS) && 0>=jumpLevel) throw new InvalidOperationException("must be able to jump on an object providing FOV bonus for standing on it");
            if (default != (flags & Flags.IS_WALKABLE) && 0<jumpLevel) throw new InvalidOperationException("map objects may not be both walkable and jumpable");
#endif
        }

        private static byte _ID_Weight(IDs x) {
            switch (x) {
                case IDs.SMALL_FORTIFICATION: return 4;
                case IDs.BED: return 6; // XXX all beds should have same weight
                case IDs.HOSPITAL_BED: return 6;
                case IDs.CHAIR: return 1;   // XXX all chairs should have same weight
                case IDs.HOSPITAL_CHAIR: return 1;
                case IDs.CHAR_CHAIR: return 1;
                case IDs.TABLE: return 2;   // XXX all tables should have same weight
                case IDs.CHAR_TABLE: return 2;
                case IDs.NIGHT_TABLE: return 1; // XXX all night tables should have same weight
                case IDs.HOSPITAL_NIGHT_TABLE: return 1;
                case IDs.DRAWER: return 6;
                case IDs.FRIDGE: return 10;
                case IDs.WARDROBE: return 10;   // all wardrobes should have same weight
                case IDs.HOSPITAL_WARDROBE: return 10;
                case IDs.CAR1: return CAR_WEIGHT;  // all cars should have same weight
                case IDs.CAR2: return CAR_WEIGHT;
                case IDs.CAR3: return CAR_WEIGHT;
                case IDs.CAR4: return CAR_WEIGHT;
                case IDs.SHOP_SHELF: return 6;
                case IDs.JUNK: return 6;
                case IDs.BARRELS: return 10;
                default: return 0;  // not moveable
            }
        }

    private static bool _ID_GivesWood(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return true;
        case IDs.GARDEN_FENCE: return true;
        case IDs.TREE: return true;
        case IDs.DOOR: return true;
        case IDs.WINDOW: return true;
        case IDs.HOSPITAL_DOOR: return true;
        case IDs.BENCH: return true;
        case IDs.LARGE_FORTIFICATION: return true;
        case IDs.SMALL_FORTIFICATION: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
        case IDs.CHAIR: return true;
        case IDs.HOSPITAL_CHAIR: return true;
        case IDs.CHAR_CHAIR: return true;
        case IDs.TABLE: return true;
        case IDs.CHAR_TABLE: return true;
        case IDs.NIGHT_TABLE: return true;
        case IDs.HOSPITAL_NIGHT_TABLE: return true;
        case IDs.DRAWER: return true;
        case IDs.WARDROBE: return true;
        case IDs.HOSPITAL_WARDROBE: return true;
        case IDs.SHOP_SHELF: return true;
        case IDs.JUNK: return true;
        case IDs.BARRELS: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_StandOnFOVbonus(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return true;
        case IDs.GARDEN_FENCE: return true;
        case IDs.WIRE_FENCE: return true;
        case IDs.CAR1: return true;
        case IDs.CAR2: return true;
        case IDs.CAR3: return true;
        case IDs.CAR4: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_BreaksWhenFiredThrough(IDs x)
    {
      switch (x) {
        case IDs.WINDOW: return true;
        case IDs.GLASS_DOOR: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_IsCouch(IDs x)
    {
      switch (x) {
        case IDs.BENCH: return true;
        case IDs.IRON_BENCH: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_IsPlural(IDs x)
    {
      switch (x) {
        case IDs.JUNK: return true;
        case IDs.BARRELS: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_MaterialIsTransparent(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return true;
        case IDs.IRON_FENCE: return true;
        case IDs.GARDEN_FENCE: return true;
        case IDs.WIRE_FENCE: return true;
        case IDs.IRON_GATE_CLOSED: return true;
        case IDs.IRON_GATE_OPEN: return true;
        case IDs.WINDOW: return true;
        case IDs.GLASS_DOOR: return true;
        case IDs.BENCH: return true;
        case IDs.IRON_BENCH: return true;
        case IDs.SMALL_FORTIFICATION: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
        case IDs.CHAIR: return true;
        case IDs.HOSPITAL_CHAIR: return true;
        case IDs.CHAR_CHAIR: return true;
        case IDs.TABLE: return true;
        case IDs.CHAR_TABLE: return true;
        case IDs.NIGHT_TABLE: return true;
        case IDs.HOSPITAL_NIGHT_TABLE: return true;
        case IDs.DRAWER: return true;
        case IDs.WARDROBE: return true;
        case IDs.HOSPITAL_WARDROBE: return true;
        case IDs.CAR1: return true;
        case IDs.CAR2: return true;
        case IDs.CAR3: return true;
        case IDs.CAR4: return true;
        case IDs.JUNK: return true;
        case IDs.BARRELS: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private int _ID_Jumplevel(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return 1;
        case IDs.GARDEN_FENCE: return 1;
        case IDs.WIRE_FENCE: return 1;
        case IDs.BENCH: return 1;
        case IDs.IRON_BENCH: return 1;
        case IDs.SMALL_FORTIFICATION: return 1;
        case IDs.CHAIR: return 1;
        case IDs.HOSPITAL_CHAIR: return 1;
        case IDs.CHAR_CHAIR: return 1;
        case IDs.TABLE: return 1;
        case IDs.CHAR_TABLE: return 1;
        case IDs.NIGHT_TABLE: return 1;
        case IDs.HOSPITAL_NIGHT_TABLE: return 1;
        case IDs.CAR1: return 1;
        case IDs.CAR2: return 1;
        case IDs.CAR3: return 1;
        case IDs.CAR4: return 1;
//      case IDs.: return 1;
        default: return 0;
      }
    }

    static private bool _ID_IsWalkable(IDs x)
    {
      switch (x) {
        case IDs.IRON_GATE_OPEN: return true;
        case IDs.DOOR: return true;
        case IDs.WINDOW: return true;
        case IDs.HOSPITAL_DOOR: return true;
        case IDs.GLASS_DOOR: return true;
        case IDs.CHAR_DOOR: return true;
        case IDs.IRON_DOOR: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_StartsBroken(IDs x)
    {
      switch (x) {
        case IDs.CAR1: return true;
        case IDs.CAR2: return true;
        case IDs.CAR3: return true;
        case IDs.CAR4: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private int _ID_MaxHP(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return 10 * DoorWindow.BASE_HITPOINTS;
        case IDs.GARDEN_FENCE: return DoorWindow.BASE_HITPOINTS / 2;
        case IDs.WIRE_FENCE: return DoorWindow.BASE_HITPOINTS;
        case IDs.TREE: return 10 * DoorWindow.BASE_HITPOINTS;
        case IDs.IRON_GATE_CLOSED: return 20 * DoorWindow.BASE_HITPOINTS;
        case IDs.DOOR: return DoorWindow.BASE_HITPOINTS;
        case IDs.WINDOW: return DoorWindow.BASE_HITPOINTS / 4;
        case IDs.HOSPITAL_DOOR: return DoorWindow.BASE_HITPOINTS;
        case IDs.GLASS_DOOR: return DoorWindow.BASE_HITPOINTS / 4;
        case IDs.CHAR_DOOR: return 4 * DoorWindow.BASE_HITPOINTS;
        case IDs.IRON_DOOR: return 8 * DoorWindow.BASE_HITPOINTS;
        case IDs.BENCH: return 2 * DoorWindow.BASE_HITPOINTS;
        case IDs.SMALL_FORTIFICATION: return Fortification.SMALL_BASE_HITPOINTS;
        case IDs.LARGE_FORTIFICATION: return Fortification.LARGE_BASE_HITPOINTS;
        case IDs.BED: return 2 * DoorWindow.BASE_HITPOINTS;
        case IDs.HOSPITAL_BED: return 2 * DoorWindow.BASE_HITPOINTS;
        case IDs.CHAIR: return DoorWindow.BASE_HITPOINTS / 3;
        case IDs.HOSPITAL_CHAIR: return DoorWindow.BASE_HITPOINTS / 3;
        case IDs.CHAR_CHAIR: return DoorWindow.BASE_HITPOINTS / 3;
        case IDs.TABLE: return DoorWindow.BASE_HITPOINTS;
        case IDs.CHAR_TABLE: return DoorWindow.BASE_HITPOINTS;
        case IDs.NIGHT_TABLE: return DoorWindow.BASE_HITPOINTS / 3;
        case IDs.HOSPITAL_NIGHT_TABLE: return DoorWindow.BASE_HITPOINTS / 3;
        case IDs.DRAWER: return DoorWindow.BASE_HITPOINTS;
        case IDs.FRIDGE: return 6 * DoorWindow.BASE_HITPOINTS;
        case IDs.WARDROBE: return 6 * DoorWindow.BASE_HITPOINTS;
        case IDs.HOSPITAL_WARDROBE: return 6 * DoorWindow.BASE_HITPOINTS;
        case IDs.SHOP_SHELF: return DoorWindow.BASE_HITPOINTS;
        case IDs.JUNK: return DoorWindow.BASE_HITPOINTS;
        case IDs.BARRELS: return 2 * DoorWindow.BASE_HITPOINTS;
        default: return 0;
      }
    }

        [System.Flags]
        public enum Flags
        {
            IS_AN = 1,    // XXX dead, retaining for historical reference
            IS_PLURAL = 2,
            IS_MATERIAL_TRANSPARENT = 4,
            IS_WALKABLE = 8,
            IS_CONTAINER = 16,    // XXX dead, retaining for historical reference
            IS_COUCH = 32,
            GIVES_WOOD = 64,
            IS_MOVABLE = 128,
            BREAKS_WHEN_FIRED_THROUGH = 256,
            STANDON_FOV_BONUS = 512,
        }

        public enum IDs : byte
        {
            FENCE = 0,    // not-pushable ID block
            IRON_FENCE,
            GARDEN_FENCE,
            WIRE_FENCE,
            BOARD,
            TREE,
            IRON_GATE_CLOSED,
            IRON_GATE_OPEN,
            CHAR_POWER_GENERATOR, // Tesla technology
            DOOR,
            WINDOW,
            HOSPITAL_DOOR,
            GLASS_DOOR,
            CHAR_DOOR,
            IRON_DOOR,
            BENCH,
            IRON_BENCH,
            LARGE_FORTIFICATION,
            SMALL_FORTIFICATION,  // pushable ID block
            BED,
            HOSPITAL_BED,
            CHAIR,
            HOSPITAL_CHAIR,
            CHAR_CHAIR,
            TABLE,
            CHAR_TABLE,
            NIGHT_TABLE,
            HOSPITAL_NIGHT_TABLE,
            DRAWER,
            FRIDGE,
            WARDROBE,
            HOSPITAL_WARDROBE,
            CAR1,
            CAR2,
            CAR3,
            CAR4,
            SHOP_SHELF,
            JUNK,
            BARRELS
            //    on release of v0.10.0.0 all enumeration values "freeze" and new enumeration values must be later for savefile compatibility (until value 256 is needed at which point the savefile breaks)
        }

        const int _COUNT = (int)IDs.BARRELS + 1;
    }
}
