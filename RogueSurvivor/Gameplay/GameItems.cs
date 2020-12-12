// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameItems
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.IO;

using Point = Zaimoni.Data.Vector2D_short;
using Color = System.Drawing.Color;

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameItems : ItemModelDB
  {
    // like GameActors, these Verbs trigger a saveload file bloat cycle
    private static readonly Verb FIRE_SALVO_AT = new Verb("fire a salvo at", "fires a salvo at");
    private static readonly Verb PIERCE = new Verb("pierce");
    private static readonly Verb SHOOT = new Verb("shoot");
    private static readonly Verb SLASH = new Verb("slash", "slashes");
    private static readonly Verb SMASH = new Verb("smash", "smashes");
    private static readonly Verb STAB = new Verb("stab");
    private static readonly Verb STRIKE = new Verb("strike");

    private static readonly ItemModel[] m_Models = new ItemModel[(int) IDs._COUNT];
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> ammo
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.AMMO_LIGHT_PISTOL,
            IDs.AMMO_HEAVY_PISTOL,
            IDs.AMMO_SHOTGUN,
            IDs.AMMO_LIGHT_RIFLE,
            IDs.AMMO_HEAVY_RIFLE,
            IDs.AMMO_BOLTS
    });
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> ranged
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.RANGED_ARMY_PISTOL,
        IDs.RANGED_ARMY_RIFLE,
        IDs.RANGED_HUNTING_CROSSBOW,
        IDs.RANGED_HUNTING_RIFLE,
        IDs.RANGED_KOLT_REVOLVER,
        IDs.RANGED_PISTOL,
        IDs.RANGED_PRECISION_RIFLE,
        IDs.RANGED_SHOTGUN,
        IDs.UNIQUE_SANTAMAN_SHOTGUN,
        IDs.UNIQUE_HANS_VON_HANZ_PISTOL
    });
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> armor
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.ARMOR_ARMY_BODYARMOR,
            IDs.ARMOR_CHAR_LIGHT_BODYARMOR,
            IDs.ARMOR_HELLS_SOULS_JACKET,
            IDs.ARMOR_FREE_ANGELS_JACKET,
            IDs.ARMOR_POLICE_JACKET,
            IDs.ARMOR_POLICE_RIOT,
            IDs.ARMOR_HUNTER_VEST});
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> food
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.FOOD_ARMY_RATION,
            IDs.FOOD_GROCERIES,
            IDs.FOOD_CANNED_FOOD});
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> melee
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> {
        IDs.MELEE_BASEBALLBAT,
        IDs.MELEE_COMBAT_KNIFE,
        IDs.MELEE_CROWBAR,
        IDs.MELEE_GOLFCLUB,
        IDs.MELEE_HUGE_HAMMER,
        IDs.MELEE_IRON_GOLFCLUB,
        IDs.MELEE_SHOVEL,
        IDs.MELEE_SHORT_SHOVEL,
        IDs.MELEE_TRUNCHEON,
        IDs.UNIQUE_JASON_MYERS_AXE,
        IDs.MELEE_IMPROVISED_CLUB,
        IDs.MELEE_IMPROVISED_SPEAR,
        IDs.MELEE_SMALL_HAMMER,
        IDs.UNIQUE_FAMU_FATARU_KATANA,
        IDs.UNIQUE_BIGBEAR_BAT,
        IDs.UNIQUE_ROGUEDJACK_KEYBOARD,
        IDs.UNIQUE_FATHER_TIME_SCYTHE
    });
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> medicine
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.MEDICINE_BANDAGES,
        IDs.MEDICINE_MEDIKIT,
        IDs.MEDICINE_PILLS_STA,
        IDs.MEDICINE_PILLS_SLP,
        IDs.MEDICINE_PILLS_SAN,
        IDs.MEDICINE_PILLS_ANTIVIRAL});

    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> restoreSAN
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.MEDICINE_PILLS_SAN,
        IDs.ENT_BOOK,
        IDs.ENT_MAGAZINE,
        IDs.ENT_CHAR_GUARD_MANUAL});

    private MedecineData DATA_MEDICINE_BANDAGE;
    private MedecineData DATA_MEDICINE_MEDIKIT;
    private MedecineData DATA_MEDICINE_PILLS_STA;
    private MedecineData DATA_MEDICINE_PILLS_SLP;
    private MedecineData DATA_MEDICINE_PILLS_SAN;
    private MedecineData DATA_MEDICINE_PILLS_ANTIVIRAL;
    private FoodData DATA_FOOD_ARMY_RATION;
    private FoodData DATA_FOOD_GROCERIES;
    private FoodData DATA_FOOD_CANNED_FOOD;
    private MeleeWeaponData DATA_MELEE_CROWBAR;
    private MeleeWeaponData DATA_MELEE_BASEBALLBAT;
    private MeleeWeaponData DATA_MELEE_COMBAT_KNIFE;
    private MeleeWeaponData DATA_MELEE_UNIQUE_JASON_MYERS_AXE;
    private MeleeWeaponData DATA_MELEE_GOLFCLUB;
    private MeleeWeaponData DATA_MELEE_HUGE_HAMMER;
    private MeleeWeaponData DATA_MELEE_SMALL_HAMMER;
    private MeleeWeaponData DATA_MELEE_IRON_GOLFCLUB;
    private MeleeWeaponData DATA_MELEE_SHOVEL;
    private MeleeWeaponData DATA_MELEE_SHORT_SHOVEL;
    private MeleeWeaponData DATA_MELEE_TRUNCHEON;
    private MeleeWeaponData DATA_MELEE_IMPROVISED_CLUB;
    private MeleeWeaponData DATA_MELEE_IMPROVISED_SPEAR;
    private MeleeWeaponData DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA;
    private MeleeWeaponData DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE;
    private MeleeWeaponData DATA_MELEE_UNIQUE_BIGBEAR_BAT;
    private MeleeWeaponData DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD;
    private RangedWeaponData DATA_RANGED_ARMY_PISTOL;
    private RangedWeaponData DATA_RANGED_ARMY_RIFLE;
    private RangedWeaponData DATA_RANGED_HUNTING_CROSSBOW;
    private RangedWeaponData DATA_RANGED_HUNTING_RIFLE;
    private RangedWeaponData DATA_RANGED_KOLT_REVOLVER;
    private RangedWeaponData DATA_RANGED_PISTOL;
    private RangedWeaponData DATA_RANGED_PRECISION_RIFLE;
    private RangedWeaponData DATA_RANGED_SHOTGUN;
    private RangedWeaponData DATA_UNIQUE_SANTAMAN_SHOTGUN;
    private RangedWeaponData DATA_UNIQUE_HANS_VON_HANZ_PISTOL;
    private ExplosiveData DATA_EXPLOSIVE_GRENADE;
    private BarricadingMaterialData DATA_BAR_WOODEN_PLANK;
    private ArmorData DATA_ARMOR_ARMY;
    private ArmorData DATA_ARMOR_CHAR;
    private ArmorData DATA_ARMOR_HELLS_SOULS_JACKET;
    private ArmorData DATA_ARMOR_FREE_ANGELS_JACKET;
    private ArmorData DATA_ARMOR_POLICE_JACKET;
    private ArmorData DATA_ARMOR_POLICE_RIOT;
    private ArmorData DATA_ARMOR_HUNTER_VEST;
    private TrackerData DATA_TRACKER_BLACKOPS_GPS;
    private TrackerData DATA_TRACKER_CELL_PHONE;
    private TrackerData DATA_TRACKER_ZTRACKER;
    private TrackerData DATA_TRACKER_POLICE_RADIO;
    private SprayPaintData DATA_SPRAY_PAINT1;
    private SprayPaintData DATA_SPRAY_PAINT2;
    private SprayPaintData DATA_SPRAY_PAINT3;
    private SprayPaintData DATA_SPRAY_PAINT4;
    private LightData DATA_LIGHT_FLASHLIGHT;
    private LightData DATA_LIGHT_BIG_FLASHLIGHT;
    private ScentSprayData DATA_SCENT_SPRAY_STENCH_KILLER;
    private TrapData DATA_TRAP_EMPTY_CAN;
    private TrapData DATA_TRAP_BEAR_TRAP;
    private TrapData DATA_TRAP_SPIKES;
    private TrapData DATA_TRAP_BARBED_WIRE;
    private EntData DATA_ENT_BOOK;
    private EntData DATA_ENT_MAGAZINE;

    public ItemModel this[int id] {
      get {
        return m_Models[id];
      }
    }

    public ItemModel this[IDs id] {
      get {
        return this[(int) id];
      }
    }

    private static void _setModel(ItemModel model) {
#if DEBUG
      if (null != m_Models[(int) model.ID]) throw new InvalidOperationException("can only set item model once");
#endif
      m_Models[(int) model.ID] = model;
    }

	public static _T_ Cast<_T_>(int id) where _T_:ItemModel
	{
	  return m_Models[id] as _T_;
	}

    static public ItemMedicineModel BANDAGE {
      get {
        return m_Models[(int)IDs.MEDICINE_BANDAGES] as ItemMedicineModel;
      }
    }

    static public ItemMedicineModel MEDIKIT {
      get {
        return m_Models[(int)IDs.MEDICINE_MEDIKIT] as ItemMedicineModel;
      }
    }

    static public ItemMedicineModel PILLS_STA {
      get {
        return m_Models[(int)IDs.MEDICINE_PILLS_STA] as ItemMedicineModel;
      }
    }

    static public ItemMedicineModel PILLS_SLP {
      get {
        return m_Models[(int)IDs.MEDICINE_PILLS_SLP] as ItemMedicineModel;
      }
    }

    static public ItemMedicineModel PILLS_SAN {
      get {
        return m_Models[(int)IDs.MEDICINE_PILLS_SAN] as ItemMedicineModel;
      }
    }

    static public ItemMedicineModel PILLS_ANTIVIRAL {
      get {
        return m_Models[(int)IDs.MEDICINE_PILLS_ANTIVIRAL] as ItemMedicineModel;
      }
    }

    static public ItemFoodModel ARMY_RATION {
      get {
        return m_Models[(int)IDs.FOOD_ARMY_RATION] as ItemFoodModel;
      }
    }

    static public ItemFoodModel GROCERIES {
      get {
        return m_Models[(int)IDs.FOOD_GROCERIES] as ItemFoodModel;
      }
    }

    static public ItemFoodModel CANNED_FOOD {
      get {
        return m_Models[(int)IDs.FOOD_CANNED_FOOD] as ItemFoodModel;
      }
    }

    static public ItemMeleeWeaponModel CROWBAR {
      get {
        return m_Models[(int)IDs.MELEE_CROWBAR] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel BASEBALLBAT {
      get {
        return m_Models[(int)IDs.MELEE_BASEBALLBAT] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel COMBAT_KNIFE {
      get {
        return m_Models[(int)IDs.MELEE_COMBAT_KNIFE] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel UNIQUE_JASON_MYERS_AXE {
      get {
        return m_Models[(int)IDs.UNIQUE_JASON_MYERS_AXE] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel GOLFCLUB {
      get {
        return m_Models[(int)IDs.MELEE_GOLFCLUB] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel HUGE_HAMMER {
      get {
        return m_Models[(int)IDs.MELEE_HUGE_HAMMER] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel SMALL_HAMMER {
      get {
        return m_Models[(int)IDs.MELEE_SMALL_HAMMER] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel IRON_GOLFCLUB {
      get {
        return m_Models[(int)IDs.MELEE_IRON_GOLFCLUB] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel SHOVEL {
      get {
        return m_Models[(int)IDs.MELEE_SHOVEL] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel SHORT_SHOVEL {
      get {
        return m_Models[(int)IDs.MELEE_SHORT_SHOVEL] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel TRUNCHEON {
      get {
        return m_Models[(int)IDs.MELEE_TRUNCHEON] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel IMPROVISED_CLUB {
      get {
        return m_Models[(int)IDs.MELEE_IMPROVISED_CLUB] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel IMPROVISED_SPEAR {
      get {
        return m_Models[(int)IDs.MELEE_IMPROVISED_SPEAR] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel UNIQUE_FAMU_FATARU_KATANA {
      get {
        return m_Models[(int)IDs.UNIQUE_FAMU_FATARU_KATANA] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel UNIQUE_FATHER_TIME_SCYTHE {
      get {
        return m_Models[(int)IDs.UNIQUE_FATHER_TIME_SCYTHE] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel UNIQUE_BIGBEAR_BAT {
      get {
        return m_Models[(int)IDs.UNIQUE_BIGBEAR_BAT] as ItemMeleeWeaponModel;
      }
    }

    static public ItemMeleeWeaponModel UNIQUE_ROGUEDJACK_KEYBOARD {
      get {
        return m_Models[(int)IDs.UNIQUE_ROGUEDJACK_KEYBOARD] as ItemMeleeWeaponModel;
      }
    }

    static public ItemRangedWeaponModel ARMY_PISTOL {
      get {
        return m_Models[(int)IDs.RANGED_ARMY_PISTOL] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel ARMY_RIFLE {
      get {
        return m_Models[(int)IDs.RANGED_ARMY_RIFLE] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel HUNTING_CROSSBOW {
      get {
        return m_Models[(int)IDs.RANGED_HUNTING_CROSSBOW] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel HUNTING_RIFLE {
      get {
        return m_Models[(int)IDs.RANGED_HUNTING_RIFLE] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel KOLT_REVOLVER {
      get {
        return m_Models[(int)IDs.RANGED_KOLT_REVOLVER] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel PISTOL {
      get {
        return m_Models[(int)IDs.RANGED_PISTOL] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel PRECISION_RIFLE {
      get {
        return m_Models[(int)IDs.RANGED_PRECISION_RIFLE] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel SHOTGUN {
      get {
        return m_Models[(int)IDs.RANGED_SHOTGUN] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel UNIQUE_SANTAMAN_SHOTGUN {
      get {
        return m_Models[(int)IDs.UNIQUE_SANTAMAN_SHOTGUN] as ItemRangedWeaponModel;
      }
    }

    static public ItemRangedWeaponModel UNIQUE_HANS_VON_HANZ_PISTOL {
      get {
        return m_Models[(int)IDs.UNIQUE_HANS_VON_HANZ_PISTOL] as ItemRangedWeaponModel;
      }
    }

    static public ItemAmmoModel AMMO_LIGHT_PISTOL {
      get {
        return m_Models[(int)IDs.AMMO_LIGHT_PISTOL] as ItemAmmoModel;
      }
    }

    static public ItemAmmoModel AMMO_HEAVY_PISTOL {
      get {
        return m_Models[(int)IDs.AMMO_HEAVY_PISTOL] as ItemAmmoModel;
      }
    }

    static public ItemAmmoModel AMMO_LIGHT_RIFLE {
      get {
        return m_Models[(int)IDs.AMMO_LIGHT_RIFLE] as ItemAmmoModel;
      }
    }

    static public ItemAmmoModel AMMO_HEAVY_RIFLE {
      get {
        return m_Models[(int)IDs.AMMO_HEAVY_RIFLE] as ItemAmmoModel;
      }
    }

    static public ItemAmmoModel AMMO_SHOTGUN {
      get {
        return m_Models[(int)IDs.AMMO_SHOTGUN] as ItemAmmoModel;
      }
    }

    static public ItemAmmoModel AMMO_BOLTS {
      get {
        return m_Models[(int)IDs.AMMO_BOLTS] as ItemAmmoModel;
      }
    }

    static public ItemGrenadeModel GRENADE {
      get {
        return m_Models[(int)IDs.EXPLOSIVE_GRENADE] as ItemGrenadeModel;
      }
    }

    static public ItemGrenadePrimedModel GRENADE_PRIMED {
      get {
        return m_Models[(int)IDs.EXPLOSIVE_GRENADE_PRIMED] as ItemGrenadePrimedModel;
      }
    }

    static public ItemBarricadeMaterialModel WOODENPLANK {
      get {
        return m_Models[(int)IDs.BAR_WOODEN_PLANK] as ItemBarricadeMaterialModel;
      }
    }

    static public ItemBodyArmorModel ARMY_BODYARMOR {
      get {
        return m_Models[(int)IDs.ARMOR_ARMY_BODYARMOR] as ItemBodyArmorModel;
      }
    }

    static public ItemBodyArmorModel CHAR_LT_BODYARMOR
    {
      get {
        return m_Models[(int)IDs.ARMOR_CHAR_LIGHT_BODYARMOR] as ItemBodyArmorModel;
      }
    }

    static public ItemBodyArmorModel HELLS_SOULS_JACKET {
      get {
        return m_Models[(int)IDs.ARMOR_HELLS_SOULS_JACKET] as ItemBodyArmorModel;
      }
    }

    static public ItemBodyArmorModel FREE_ANGELS_JACKET {
      get {
        return m_Models[(int)IDs.ARMOR_FREE_ANGELS_JACKET] as ItemBodyArmorModel;
      }
    }

    static public ItemBodyArmorModel POLICE_JACKET {
      get {
        return m_Models[(int)IDs.ARMOR_POLICE_JACKET] as ItemBodyArmorModel;
      }
    }

    static public ItemBodyArmorModel POLICE_RIOT {
      get {
        return m_Models[(int)IDs.ARMOR_POLICE_RIOT] as ItemBodyArmorModel;
      }
    }

    static public ItemBodyArmorModel HUNTER_VEST {
      get {
        return m_Models[(int)IDs.ARMOR_HUNTER_VEST] as ItemBodyArmorModel;
      }
    }

    static public ItemTrackerModel BLACKOPS_GPS {
      get {
        return m_Models[(int)IDs.TRACKER_BLACKOPS] as ItemTrackerModel;
      }
    }

    static public ItemTrackerModel CELL_PHONE {
      get {
        return m_Models[(int)IDs.TRACKER_CELL_PHONE] as ItemTrackerModel;
      }
    }

    static public ItemTrackerModel ZTRACKER {
      get {
        return m_Models[(int)IDs.TRACKER_ZTRACKER] as ItemTrackerModel;
      }
    }

    static public ItemTrackerModel POLICE_RADIO {
      get {
        return m_Models[(int)IDs.TRACKER_POLICE_RADIO] as ItemTrackerModel;
      }
    }

    static public ItemSprayPaintModel SPRAY_PAINT1 {
      get{
        return m_Models[(int)IDs.SPRAY_PAINT1] as ItemSprayPaintModel;
      }
    }

    static public ItemSprayPaintModel SPRAY_PAINT2 {
      get {
        return m_Models[(int)IDs.SPRAY_PAINT2] as ItemSprayPaintModel;
      }
    }

    static public ItemSprayPaintModel SPRAY_PAINT3 {
      get {
        return m_Models[(int)IDs.SPRAY_PAINT3] as ItemSprayPaintModel;
      }
    }

    static public ItemSprayPaintModel SPRAY_PAINT4 {
      get {
        return m_Models[(int)IDs.SPRAY_PAINT4] as ItemSprayPaintModel;
      }
    }

    static public ItemLightModel FLASHLIGHT {
      get {
        return m_Models[(int)IDs.LIGHT_FLASHLIGHT] as ItemLightModel;
      }
    }

    static public ItemLightModel BIG_FLASHLIGHT {
      get {
        return m_Models[(int)IDs.LIGHT_BIG_FLASHLIGHT] as ItemLightModel;
      }
    }

    static public ItemSprayScentModel STENCH_KILLER {
      get {
        return m_Models[(int)IDs.SCENT_SPRAY_STENCH_KILLER] as ItemSprayScentModel;
      }
    }

    static public ItemTrapModel EMPTY_CAN {
      get {
        return m_Models[(int)IDs.TRAP_EMPTY_CAN] as ItemTrapModel;
      }
    }

    static public ItemTrapModel BEAR_TRAP {
      get {
        return m_Models[(int)IDs.TRAP_BEAR_TRAP] as ItemTrapModel;
      }
    }

    static public ItemTrapModel SPIKES {
      get {
        return m_Models[(int)IDs.TRAP_SPIKES] as ItemTrapModel;
      }
    }

    static public ItemTrapModel BARBED_WIRE {
      get {
        return m_Models[(int)IDs.TRAP_BARBED_WIRE] as ItemTrapModel;
      }
    }

    static public ItemEntertainmentModel BOOK {
      get {
        return m_Models[(int)IDs.ENT_BOOK] as ItemEntertainmentModel;
      }
    }

    static public ItemEntertainmentModel MAGAZINE {
      get {
        return m_Models[(int)IDs.ENT_MAGAZINE] as ItemEntertainmentModel;
      }
    }

    static public ItemEntertainmentModel CHAR_GUARD_MANUAL {
      get {
        return m_Models[(int)IDs.ENT_CHAR_GUARD_MANUAL] as ItemEntertainmentModel;
      }
    }

    static public ItemModel UNIQUE_SUBWAY_BADGE {
      get {
        return m_Models[(int)IDs.UNIQUE_SUBWAY_BADGE];
      }
    }

    public GameItems(IRogueUI ui)
    {
#if DEBUG
      if (null == ui) throw new ArgumentNullException(nameof(ui));
#endif
      Models.Items = this;

      LoadMedicineFromCSV(ui, "Resources\\Data\\Items_Medicine.csv");
      LoadFoodFromCSV(ui, "Resources\\Data\\Items_Food.csv");
      LoadMeleeWeaponsFromCSV(ui, "Resources\\Data\\Items_MeleeWeapons.csv");
      LoadRangedWeaponsFromCSV(ui, "Resources\\Data\\Items_RangedWeapons.csv");
      LoadExplosivesFromCSV(ui, "Resources\\Data\\Items_Explosives.csv");
      LoadBarricadingMaterialFromCSV(ui, "Resources\\Data\\Items_Barricading.csv");
      LoadArmorsFromCSV(ui, "Resources\\Data\\Items_Armors.csv");
      LoadTrackersFromCSV(ui, "Resources\\Data\\Items_Trackers.csv");
      LoadSpraypaintsFromCSV(ui, "Resources\\Data\\Items_Spraypaints.csv");
      LoadLightsFromCSV(ui, "Resources\\Data\\Items_Lights.csv");
      LoadScentspraysFromCSV(ui, "Resources\\Data\\Items_Scentsprays.csv");
      LoadTrapsFromCSV(ui, "Resources\\Data\\Items_Traps.csv");
      LoadEntertainmentFromCSV(ui, "Resources\\Data\\Items_Entertainment.csv");
      CreateModels();
    }

    private void CreateModels()
    {
#if DEBUG
      if (0 != (int)AmmoType.LIGHT_PISTOL) throw new InvalidOperationException("Reasonable C conversion between AmmoType and GameItems.IDs invalid");
      if ((int)AmmoType.HEAVY_PISTOL - (int)AmmoType.LIGHT_PISTOL != (int)IDs.AMMO_HEAVY_PISTOL - (int)IDs.AMMO_LIGHT_PISTOL) throw new InvalidOperationException("Reasonable C conversion between AmmoType and GameItems.IDs invalid");
      if ((int)AmmoType.LIGHT_RIFLE - (int)AmmoType.LIGHT_PISTOL != (int)IDs.AMMO_LIGHT_RIFLE - (int)IDs.AMMO_LIGHT_PISTOL) throw new InvalidOperationException("Reasonable C conversion between AmmoType and GameItems.IDs invalid");
      if ((int)AmmoType.HEAVY_RIFLE - (int)AmmoType.LIGHT_PISTOL != (int)IDs.AMMO_HEAVY_RIFLE - (int)IDs.AMMO_LIGHT_PISTOL) throw new InvalidOperationException("Reasonable C conversion between AmmoType and GameItems.IDs invalid");
      if ((int)AmmoType.SHOTGUN - (int)AmmoType.LIGHT_PISTOL != (int)IDs.AMMO_SHOTGUN - (int)IDs.AMMO_LIGHT_PISTOL) throw new InvalidOperationException("Reasonable C conversion between AmmoType and GameItems.IDs invalid");
      if ((int)AmmoType.BOLT - (int)AmmoType.LIGHT_PISTOL != (int)IDs.AMMO_BOLTS - (int)IDs.AMMO_LIGHT_PISTOL) throw new InvalidOperationException("Reasonable C conversion between AmmoType and GameItems.IDs invalid");

      if (1!=(int)GameGangs.IDs.BIKER_HELLS_SOULS-(int)GameGangs.IDs.NONE) throw new InvalidOperationException("Reasonable C conversion between gang id and their good armor is invalid");
      if ((int)IDs.ARMOR_FREE_ANGELS_JACKET-(int)IDs.ARMOR_HELLS_SOULS_JACKET!=(int)GameGangs.IDs.BIKER_FREE_ANGELS-(int)GameGangs.IDs.BIKER_HELLS_SOULS) throw new InvalidOperationException("Reasonable C conversion between gang id and their good armor is invalid");
      const int police_jacket_delta = (int)IDs.ARMOR_POLICE_JACKET-(int)IDs.ARMOR_HELLS_SOULS_JACKET;
      const int police_riot_delta = (int)IDs.ARMOR_POLICE_RIOT-(int)IDs.ARMOR_HELLS_SOULS_JACKET;
      if (-2 > police_jacket_delta || 0 <= police_jacket_delta) throw new InvalidOperationException("Reasonable C conversion between gang id and their good armor is invalid");
      if (-2 > police_riot_delta || 0 <= police_riot_delta) throw new InvalidOperationException("Reasonable C conversion between gang id and their good armor is invalid");

      // no good place to do the Direction crosschecks so do them here
      if (1 != (int)(PlayerCommand.MOVE_NE) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");
      if (2 != (int)(PlayerCommand.MOVE_E) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");
      if (3 != (int)(PlayerCommand.MOVE_SE) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");
      if (4 != (int)(PlayerCommand.MOVE_S) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");
      if (5 != (int)(PlayerCommand.MOVE_SW) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");
      if (6 != (int)(PlayerCommand.MOVE_W) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");
      if (7 != (int)(PlayerCommand.MOVE_NW) - (int)(PlayerCommand.MOVE_N)) throw new InvalidOperationException("Reasonable C conversion between PlayerCommand and Direction invalid");

      // No good place to do this regression testing either
      Point origin = new Point(0,0);
      if (!origin.IsScheduledBefore(origin+Direction.W)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (!origin.IsScheduledBefore(origin+Direction.NW)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (!origin.IsScheduledBefore(origin+Direction.N)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (!origin.IsScheduledBefore(origin+Direction.NE)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (origin.IsScheduledBefore(origin+Direction.E)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (origin.IsScheduledBefore(origin+Direction.SE)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (origin.IsScheduledBefore(origin+Direction.S)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
      if (origin.IsScheduledBefore(origin+Direction.SW)) throw new InvalidOperationException("IsScheduledBefore does not agree with no-skew scheduler");
#endif
      // Medicine
      _setModel(new ItemMedicineModel(IDs.MEDICINE_BANDAGES, DATA_MEDICINE_BANDAGE.NAME, DATA_MEDICINE_BANDAGE.PLURAL, GameImages.ITEM_BANDAGES, DATA_MEDICINE_BANDAGE.HEALING, DATA_MEDICINE_BANDAGE.STAMINABOOST, DATA_MEDICINE_BANDAGE.SLEEPBOOST, DATA_MEDICINE_BANDAGE.INFECTIONCURE, DATA_MEDICINE_BANDAGE.SANITYCURE, DATA_MEDICINE_BANDAGE.FLAVOR, DATA_MEDICINE_BANDAGE.STACKINGLIMIT));
      _setModel(new ItemMedicineModel(IDs.MEDICINE_MEDIKIT, DATA_MEDICINE_MEDIKIT.NAME, DATA_MEDICINE_MEDIKIT.PLURAL, GameImages.ITEM_MEDIKIT, DATA_MEDICINE_MEDIKIT.HEALING, DATA_MEDICINE_MEDIKIT.STAMINABOOST, DATA_MEDICINE_MEDIKIT.SLEEPBOOST, DATA_MEDICINE_MEDIKIT.INFECTIONCURE, DATA_MEDICINE_MEDIKIT.SANITYCURE, DATA_MEDICINE_MEDIKIT.FLAVOR));
      _setModel(new ItemMedicineModel(IDs.MEDICINE_PILLS_STA, DATA_MEDICINE_PILLS_STA.NAME, DATA_MEDICINE_PILLS_STA.PLURAL, GameImages.ITEM_PILLS_GREEN, DATA_MEDICINE_PILLS_STA.HEALING, DATA_MEDICINE_PILLS_STA.STAMINABOOST, DATA_MEDICINE_PILLS_STA.SLEEPBOOST, DATA_MEDICINE_PILLS_STA.INFECTIONCURE, DATA_MEDICINE_PILLS_STA.SANITYCURE, DATA_MEDICINE_PILLS_STA.FLAVOR, DATA_MEDICINE_PILLS_STA.STACKINGLIMIT));
      _setModel(new ItemMedicineModel(IDs.MEDICINE_PILLS_SLP, DATA_MEDICINE_PILLS_SLP.NAME, DATA_MEDICINE_PILLS_SLP.PLURAL, GameImages.ITEM_PILLS_BLUE, DATA_MEDICINE_PILLS_SLP.HEALING, DATA_MEDICINE_PILLS_SLP.STAMINABOOST, DATA_MEDICINE_PILLS_SLP.SLEEPBOOST, DATA_MEDICINE_PILLS_SLP.INFECTIONCURE, DATA_MEDICINE_PILLS_SLP.SANITYCURE, DATA_MEDICINE_PILLS_SLP.FLAVOR, DATA_MEDICINE_PILLS_SLP.STACKINGLIMIT));
      _setModel(new ItemMedicineModel(IDs.MEDICINE_PILLS_SAN, DATA_MEDICINE_PILLS_SAN.NAME, DATA_MEDICINE_PILLS_SAN.PLURAL, GameImages.ITEM_PILLS_SAN, DATA_MEDICINE_PILLS_SAN.HEALING, DATA_MEDICINE_PILLS_SAN.STAMINABOOST, DATA_MEDICINE_PILLS_SAN.SLEEPBOOST, DATA_MEDICINE_PILLS_SAN.INFECTIONCURE, DATA_MEDICINE_PILLS_SAN.SANITYCURE, DATA_MEDICINE_PILLS_SAN.FLAVOR, DATA_MEDICINE_PILLS_SAN.STACKINGLIMIT));
      _setModel(new ItemMedicineModel(IDs.MEDICINE_PILLS_ANTIVIRAL, DATA_MEDICINE_PILLS_ANTIVIRAL.NAME, DATA_MEDICINE_PILLS_ANTIVIRAL.PLURAL, GameImages.ITEM_PILLS_ANTIVIRAL, DATA_MEDICINE_PILLS_ANTIVIRAL.HEALING, DATA_MEDICINE_PILLS_ANTIVIRAL.STAMINABOOST, DATA_MEDICINE_PILLS_ANTIVIRAL.SLEEPBOOST, DATA_MEDICINE_PILLS_ANTIVIRAL.INFECTIONCURE, DATA_MEDICINE_PILLS_ANTIVIRAL.SANITYCURE, DATA_MEDICINE_PILLS_ANTIVIRAL.FLAVOR, DATA_MEDICINE_PILLS_ANTIVIRAL.STACKINGLIMIT));

      // Food
      _setModel(new ItemFoodModel(IDs.FOOD_ARMY_RATION, DATA_FOOD_ARMY_RATION.NAME, DATA_FOOD_ARMY_RATION.PLURAL, GameImages.ITEM_ARMY_RATION, DATA_FOOD_ARMY_RATION.NUTRITION, DATA_FOOD_ARMY_RATION.BESTBEFORE, DATA_FOOD_ARMY_RATION.STACKINGLIMIT, DATA_FOOD_ARMY_RATION.FLAVOR));
      _setModel(new ItemFoodModel(IDs.FOOD_GROCERIES, DATA_FOOD_GROCERIES.NAME, DATA_FOOD_GROCERIES.PLURAL, GameImages.ITEM_GROCERIES, DATA_FOOD_GROCERIES.NUTRITION, DATA_FOOD_GROCERIES.BESTBEFORE, DATA_FOOD_GROCERIES.STACKINGLIMIT, DATA_FOOD_GROCERIES.FLAVOR));
      _setModel(new ItemFoodModel(IDs.FOOD_CANNED_FOOD, DATA_FOOD_CANNED_FOOD.NAME, DATA_FOOD_CANNED_FOOD.PLURAL, GameImages.ITEM_CANNED_FOOD, DATA_FOOD_CANNED_FOOD.NUTRITION, DATA_FOOD_CANNED_FOOD.BESTBEFORE, DATA_FOOD_CANNED_FOOD.STACKINGLIMIT, DATA_FOOD_CANNED_FOOD.FLAVOR));

      // melee weapons
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_BASEBALLBAT, DATA_MELEE_BASEBALLBAT.NAME, GameImages.ITEM_BASEBALL_BAT, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_BASEBALLBAT.ATK, DATA_MELEE_BASEBALLBAT.DMG, DATA_MELEE_BASEBALLBAT.STA), DATA_MELEE_BASEBALLBAT.FLAVOR, DATA_MELEE_BASEBALLBAT.TOOLBASHDMGBONUS, DATA_MELEE_BASEBALLBAT.TOOLBUILDBONUS, DATA_MELEE_BASEBALLBAT.STACKINGLIMIT, DATA_MELEE_BASEBALLBAT.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_COMBAT_KNIFE, DATA_MELEE_COMBAT_KNIFE.NAME, GameImages.ITEM_COMBAT_KNIFE, new Attack(AttackKind.PHYSICAL, STAB, DATA_MELEE_COMBAT_KNIFE.ATK, DATA_MELEE_COMBAT_KNIFE.DMG, DATA_MELEE_COMBAT_KNIFE.STA), DATA_MELEE_COMBAT_KNIFE.FLAVOR, DATA_MELEE_COMBAT_KNIFE.TOOLBASHDMGBONUS, DATA_MELEE_COMBAT_KNIFE.TOOLBUILDBONUS, DATA_MELEE_COMBAT_KNIFE.STACKINGLIMIT, DATA_MELEE_COMBAT_KNIFE.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_CROWBAR, DATA_MELEE_CROWBAR.NAME, GameImages.ITEM_CROWBAR, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_CROWBAR.ATK, DATA_MELEE_CROWBAR.DMG, DATA_MELEE_CROWBAR.STA), DATA_MELEE_CROWBAR.FLAVOR, DATA_MELEE_CROWBAR.TOOLBASHDMGBONUS, DATA_MELEE_CROWBAR.TOOLBUILDBONUS, DATA_MELEE_CROWBAR.STACKINGLIMIT, DATA_MELEE_CROWBAR.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_GOLFCLUB, DATA_MELEE_GOLFCLUB.NAME, GameImages.ITEM_GOLF_CLUB, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_GOLFCLUB.ATK, DATA_MELEE_GOLFCLUB.DMG, DATA_MELEE_GOLFCLUB.STA), DATA_MELEE_GOLFCLUB.FLAVOR, DATA_MELEE_GOLFCLUB.TOOLBASHDMGBONUS, DATA_MELEE_GOLFCLUB.TOOLBUILDBONUS, DATA_MELEE_GOLFCLUB.STACKINGLIMIT, DATA_MELEE_GOLFCLUB.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_IRON_GOLFCLUB, DATA_MELEE_IRON_GOLFCLUB.NAME, GameImages.ITEM_IRON_GOLF_CLUB, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_IRON_GOLFCLUB.ATK, DATA_MELEE_IRON_GOLFCLUB.DMG, DATA_MELEE_IRON_GOLFCLUB.STA), DATA_MELEE_IRON_GOLFCLUB.FLAVOR, DATA_MELEE_IRON_GOLFCLUB.TOOLBASHDMGBONUS, DATA_MELEE_IRON_GOLFCLUB.TOOLBUILDBONUS, DATA_MELEE_IRON_GOLFCLUB.STACKINGLIMIT, DATA_MELEE_IRON_GOLFCLUB.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_HUGE_HAMMER, DATA_MELEE_HUGE_HAMMER.NAME, GameImages.ITEM_HUGE_HAMMER, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_HUGE_HAMMER.ATK, DATA_MELEE_HUGE_HAMMER.DMG, DATA_MELEE_HUGE_HAMMER.STA), DATA_MELEE_HUGE_HAMMER.FLAVOR, DATA_MELEE_HUGE_HAMMER.TOOLBASHDMGBONUS, DATA_MELEE_HUGE_HAMMER.TOOLBUILDBONUS, DATA_MELEE_HUGE_HAMMER.STACKINGLIMIT, DATA_MELEE_HUGE_HAMMER.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_SHOVEL, DATA_MELEE_SHOVEL.NAME, GameImages.ITEM_SHOVEL, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_SHOVEL.ATK, DATA_MELEE_SHOVEL.DMG, DATA_MELEE_SHOVEL.STA), DATA_MELEE_SHOVEL.FLAVOR, DATA_MELEE_SHOVEL.TOOLBASHDMGBONUS, DATA_MELEE_SHOVEL.TOOLBUILDBONUS, DATA_MELEE_SHOVEL.STACKINGLIMIT, DATA_MELEE_SHOVEL.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_SHORT_SHOVEL, DATA_MELEE_SHORT_SHOVEL.NAME, GameImages.ITEM_SHORT_SHOVEL, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_SHORT_SHOVEL.ATK, DATA_MELEE_SHORT_SHOVEL.DMG, DATA_MELEE_SHORT_SHOVEL.STA), DATA_MELEE_SHORT_SHOVEL.FLAVOR, DATA_MELEE_SHORT_SHOVEL.TOOLBASHDMGBONUS, DATA_MELEE_SHORT_SHOVEL.TOOLBUILDBONUS, DATA_MELEE_SHORT_SHOVEL.STACKINGLIMIT, DATA_MELEE_SHORT_SHOVEL.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_TRUNCHEON, DATA_MELEE_TRUNCHEON.NAME, GameImages.ITEM_TRUNCHEON, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_TRUNCHEON.ATK, DATA_MELEE_TRUNCHEON.DMG, DATA_MELEE_TRUNCHEON.STA), DATA_MELEE_TRUNCHEON.FLAVOR, DATA_MELEE_TRUNCHEON.TOOLBASHDMGBONUS, DATA_MELEE_TRUNCHEON.TOOLBUILDBONUS, DATA_MELEE_TRUNCHEON.STACKINGLIMIT, DATA_MELEE_TRUNCHEON.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_IMPROVISED_CLUB, DATA_MELEE_IMPROVISED_CLUB.NAME, GameImages.ITEM_IMPROVISED_CLUB, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_IMPROVISED_CLUB.ATK, DATA_MELEE_IMPROVISED_CLUB.DMG, DATA_MELEE_IMPROVISED_CLUB.STA), DATA_MELEE_IMPROVISED_CLUB.FLAVOR, DATA_MELEE_IMPROVISED_CLUB.TOOLBASHDMGBONUS, DATA_MELEE_IMPROVISED_CLUB.TOOLBUILDBONUS, DATA_MELEE_IMPROVISED_CLUB.STACKINGLIMIT, DATA_MELEE_IMPROVISED_CLUB.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_IMPROVISED_SPEAR, DATA_MELEE_IMPROVISED_SPEAR.NAME, GameImages.ITEM_IMPROVISED_SPEAR, new Attack(AttackKind.PHYSICAL, PIERCE, DATA_MELEE_IMPROVISED_SPEAR.ATK, DATA_MELEE_IMPROVISED_SPEAR.DMG, DATA_MELEE_IMPROVISED_SPEAR.STA), DATA_MELEE_IMPROVISED_SPEAR.FLAVOR, DATA_MELEE_IMPROVISED_SPEAR.TOOLBASHDMGBONUS, DATA_MELEE_IMPROVISED_SPEAR.TOOLBUILDBONUS, DATA_MELEE_IMPROVISED_SPEAR.STACKINGLIMIT, DATA_MELEE_IMPROVISED_SPEAR.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.MELEE_SMALL_HAMMER, DATA_MELEE_SMALL_HAMMER.NAME, GameImages.ITEM_SMALL_HAMMER, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_SMALL_HAMMER.ATK, DATA_MELEE_SMALL_HAMMER.DMG, DATA_MELEE_SMALL_HAMMER.STA), DATA_MELEE_SMALL_HAMMER.FLAVOR, DATA_MELEE_SMALL_HAMMER.TOOLBASHDMGBONUS, DATA_MELEE_SMALL_HAMMER.TOOLBUILDBONUS, DATA_MELEE_SMALL_HAMMER.STACKINGLIMIT, DATA_MELEE_SMALL_HAMMER.ISFRAGILE));
      _setModel(new ItemMeleeWeaponModel(IDs.UNIQUE_JASON_MYERS_AXE, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.NAME, GameImages.ITEM_JASON_MYERS_AXE, new Attack(AttackKind.PHYSICAL, SLASH, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.ATK, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.DMG, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.STA), DATA_MELEE_UNIQUE_JASON_MYERS_AXE.FLAVOR, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.TOOLBASHDMGBONUS, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.TOOLBUILDBONUS, true));
      _setModel(new ItemMeleeWeaponModel(IDs.UNIQUE_FAMU_FATARU_KATANA, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.NAME, GameImages.ITEM_FAMU_FATARU_KATANA, new Attack(AttackKind.PHYSICAL, SLASH, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.ATK, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.DMG, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.STA), DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.FLAVOR, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.TOOLBASHDMGBONUS, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.TOOLBUILDBONUS, true));
      _setModel(new ItemMeleeWeaponModel(IDs.UNIQUE_BIGBEAR_BAT, DATA_MELEE_UNIQUE_BIGBEAR_BAT.NAME, GameImages.ITEM_BIGBEAR_BAT, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_UNIQUE_BIGBEAR_BAT.ATK, DATA_MELEE_UNIQUE_BIGBEAR_BAT.DMG, DATA_MELEE_UNIQUE_BIGBEAR_BAT.STA), DATA_MELEE_UNIQUE_BIGBEAR_BAT.FLAVOR, DATA_MELEE_UNIQUE_BIGBEAR_BAT.TOOLBASHDMGBONUS, DATA_MELEE_UNIQUE_BIGBEAR_BAT.TOOLBUILDBONUS, true));
      _setModel(new ItemMeleeWeaponModel(IDs.UNIQUE_ROGUEDJACK_KEYBOARD, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.NAME, GameImages.ITEM_ROGUEDJACK_KEYBOARD, new Attack(AttackKind.PHYSICAL, RogueGame.VERB_BASH, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.ATK, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.DMG, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.STA), DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.FLAVOR, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.TOOLBASHDMGBONUS, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.TOOLBUILDBONUS, true));
      _setModel(new ItemMeleeWeaponModel(IDs.UNIQUE_FATHER_TIME_SCYTHE, DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.NAME, GameImages.ITEM_FATHER_TIME_SCYTHE, new Attack(AttackKind.PHYSICAL, SLASH, DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.ATK, DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.DMG, DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.STA), DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.FLAVOR, DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.TOOLBASHDMGBONUS, DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE.TOOLBUILDBONUS, true));

      // ranged weapons
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_ARMY_PISTOL, DATA_RANGED_ARMY_PISTOL.NAME, DATA_RANGED_ARMY_PISTOL.FLAVOR, GameImages.ITEM_ARMY_PISTOL, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_ARMY_PISTOL.ATK, DATA_RANGED_ARMY_PISTOL.DMG, 0, DATA_RANGED_ARMY_PISTOL.RANGE, DATA_RANGED_ARMY_PISTOL.RAPID1, DATA_RANGED_ARMY_PISTOL.RAPID2), DATA_RANGED_ARMY_PISTOL.MAXAMMO, AmmoType.HEAVY_PISTOL, DATA_RANGED_ARMY_PISTOL.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_ARMY_RIFLE, DATA_RANGED_ARMY_RIFLE.NAME, DATA_RANGED_ARMY_RIFLE.FLAVOR, GameImages.ITEM_ARMY_RIFLE, new Attack(AttackKind.FIREARM, FIRE_SALVO_AT, DATA_RANGED_ARMY_RIFLE.ATK, DATA_RANGED_ARMY_RIFLE.DMG, 0, DATA_RANGED_ARMY_RIFLE.RANGE, DATA_RANGED_ARMY_RIFLE.RAPID1, DATA_RANGED_ARMY_RIFLE.RAPID2), DATA_RANGED_ARMY_RIFLE.MAXAMMO, AmmoType.HEAVY_RIFLE, DATA_RANGED_ARMY_RIFLE.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_HUNTING_CROSSBOW, DATA_RANGED_HUNTING_CROSSBOW.NAME, DATA_RANGED_HUNTING_CROSSBOW.FLAVOR, GameImages.ITEM_HUNTING_CROSSBOW, new Attack(AttackKind.BOW, SHOOT, DATA_RANGED_HUNTING_CROSSBOW.ATK, DATA_RANGED_HUNTING_CROSSBOW.DMG, 0, DATA_RANGED_HUNTING_CROSSBOW.RANGE, DATA_RANGED_HUNTING_CROSSBOW.RAPID1, DATA_RANGED_HUNTING_CROSSBOW.RAPID2), DATA_RANGED_HUNTING_CROSSBOW.MAXAMMO, AmmoType.BOLT, DATA_RANGED_HUNTING_CROSSBOW.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_HUNTING_RIFLE, DATA_RANGED_HUNTING_RIFLE.NAME, DATA_RANGED_HUNTING_RIFLE.FLAVOR, GameImages.ITEM_HUNTING_RIFLE, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_HUNTING_RIFLE.ATK, DATA_RANGED_HUNTING_RIFLE.DMG, 0, DATA_RANGED_HUNTING_RIFLE.RANGE, DATA_RANGED_HUNTING_RIFLE.RAPID1, DATA_RANGED_HUNTING_RIFLE.RAPID2), DATA_RANGED_HUNTING_RIFLE.MAXAMMO, AmmoType.LIGHT_RIFLE, DATA_RANGED_HUNTING_RIFLE.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_PISTOL, DATA_RANGED_PISTOL.NAME, DATA_RANGED_PISTOL.FLAVOR, GameImages.ITEM_PISTOL, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_PISTOL.ATK, DATA_RANGED_PISTOL.DMG, 0, DATA_RANGED_PISTOL.RANGE, DATA_RANGED_PISTOL.RAPID1, DATA_RANGED_PISTOL.RAPID2), DATA_RANGED_PISTOL.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_RANGED_PISTOL.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_KOLT_REVOLVER, DATA_RANGED_KOLT_REVOLVER.NAME, DATA_RANGED_KOLT_REVOLVER.FLAVOR, GameImages.ITEM_KOLT_REVOLVER, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_KOLT_REVOLVER.ATK, DATA_RANGED_KOLT_REVOLVER.DMG, 0, DATA_RANGED_KOLT_REVOLVER.RANGE, DATA_RANGED_KOLT_REVOLVER.RAPID1, DATA_RANGED_KOLT_REVOLVER.RAPID2), DATA_RANGED_KOLT_REVOLVER.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_RANGED_KOLT_REVOLVER.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_PRECISION_RIFLE, DATA_RANGED_PRECISION_RIFLE.NAME, DATA_RANGED_PRECISION_RIFLE.FLAVOR, GameImages.ITEM_PRECISION_RIFLE, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_PRECISION_RIFLE.ATK, DATA_RANGED_PRECISION_RIFLE.DMG, 0, DATA_RANGED_PRECISION_RIFLE.RANGE, DATA_RANGED_PRECISION_RIFLE.RAPID1, DATA_RANGED_PRECISION_RIFLE.RAPID2), DATA_RANGED_PRECISION_RIFLE.MAXAMMO, AmmoType.HEAVY_RIFLE, DATA_RANGED_PRECISION_RIFLE.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.RANGED_SHOTGUN, DATA_RANGED_SHOTGUN.NAME, DATA_RANGED_SHOTGUN.FLAVOR, GameImages.ITEM_SHOTGUN, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_SHOTGUN.ATK, DATA_RANGED_SHOTGUN.DMG, 0, DATA_RANGED_SHOTGUN.RANGE, DATA_RANGED_SHOTGUN.RAPID1, DATA_RANGED_SHOTGUN.RAPID2), DATA_RANGED_SHOTGUN.MAXAMMO, AmmoType.SHOTGUN, DATA_RANGED_SHOTGUN.FLAVOR));
      _setModel(new ItemRangedWeaponModel(IDs.UNIQUE_SANTAMAN_SHOTGUN, DATA_UNIQUE_SANTAMAN_SHOTGUN.NAME, DATA_UNIQUE_SANTAMAN_SHOTGUN.FLAVOR, GameImages.ITEM_SANTAMAN_SHOTGUN, new Attack(AttackKind.FIREARM, SHOOT, DATA_UNIQUE_SANTAMAN_SHOTGUN.ATK, DATA_UNIQUE_SANTAMAN_SHOTGUN.DMG, 0, DATA_UNIQUE_SANTAMAN_SHOTGUN.RANGE, DATA_UNIQUE_SANTAMAN_SHOTGUN.RAPID1, DATA_UNIQUE_SANTAMAN_SHOTGUN.RAPID2), DATA_UNIQUE_SANTAMAN_SHOTGUN.MAXAMMO, AmmoType.SHOTGUN, DATA_UNIQUE_SANTAMAN_SHOTGUN.FLAVOR, true));
      _setModel(new ItemRangedWeaponModel(IDs.UNIQUE_HANS_VON_HANZ_PISTOL, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.NAME, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.FLAVOR, GameImages.ITEM_HANS_VON_HANZ_PISTOL, new Attack(AttackKind.FIREARM, SHOOT, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.ATK, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.DMG, 0, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.RANGE, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.RAPID1, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.RAPID2), DATA_UNIQUE_HANS_VON_HANZ_PISTOL.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.FLAVOR, true));

      // Ammunition
      _setModel(new ItemAmmoModel(IDs.AMMO_LIGHT_PISTOL, GameImages.ITEM_AMMO_LIGHT_PISTOL, AmmoType.LIGHT_PISTOL, 20));
      _setModel(new ItemAmmoModel(IDs.AMMO_HEAVY_PISTOL, GameImages.ITEM_AMMO_HEAVY_PISTOL, AmmoType.HEAVY_PISTOL, 12));
      _setModel(new ItemAmmoModel(IDs.AMMO_LIGHT_RIFLE, GameImages.ITEM_AMMO_LIGHT_RIFLE, AmmoType.LIGHT_RIFLE, 14));
      _setModel(new ItemAmmoModel(IDs.AMMO_HEAVY_RIFLE, GameImages.ITEM_AMMO_HEAVY_RIFLE, AmmoType.HEAVY_RIFLE, 20));
      _setModel(new ItemAmmoModel(IDs.AMMO_SHOTGUN, GameImages.ITEM_AMMO_SHOTGUN, AmmoType.SHOTGUN, 10));
      _setModel(new ItemAmmoModel(IDs.AMMO_BOLTS, GameImages.ITEM_AMMO_BOLTS, AmmoType.BOLT, 30));

      // grenade, in its various states
      int[] damage = new int[DATA_EXPLOSIVE_GRENADE.RADIUS + 1];
      for (int index = 0; index < DATA_EXPLOSIVE_GRENADE.RADIUS + 1; ++index)
        damage[index] = DATA_EXPLOSIVE_GRENADE.DMG[index];   // XXX explosiveData.DMG is returned with a mismatched length

      _setModel(new ItemGrenadeModel(IDs.EXPLOSIVE_GRENADE, DATA_EXPLOSIVE_GRENADE.NAME, DATA_EXPLOSIVE_GRENADE.PLURAL, GameImages.ITEM_GRENADE, DATA_EXPLOSIVE_GRENADE.FUSE, new BlastAttack(DATA_EXPLOSIVE_GRENADE.RADIUS, damage, true, false), GameImages.ICON_BLAST, DATA_EXPLOSIVE_GRENADE.MAXTHROW, DATA_EXPLOSIVE_GRENADE.STACKLINGLIMIT, DATA_EXPLOSIVE_GRENADE.FLAVOR));
      _setModel(new ItemGrenadePrimedModel(IDs.EXPLOSIVE_GRENADE_PRIMED, "primed " + DATA_EXPLOSIVE_GRENADE.NAME, "primed " + DATA_EXPLOSIVE_GRENADE.PLURAL, GameImages.ITEM_GRENADE_PRIMED, this[IDs.EXPLOSIVE_GRENADE] as ItemGrenadeModel));

      // carpentry
      _setModel(new ItemBarricadeMaterialModel(IDs.BAR_WOODEN_PLANK, DATA_BAR_WOODEN_PLANK.NAME, DATA_BAR_WOODEN_PLANK.PLURAL, GameImages.ITEM_WOODEN_PLANK, DATA_BAR_WOODEN_PLANK.VALUE, DATA_BAR_WOODEN_PLANK.STACKINGLIMIT, DATA_BAR_WOODEN_PLANK.FLAVOR));

      // body armor
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_ARMY_BODYARMOR, DATA_ARMOR_ARMY.NAME, DATA_ARMOR_ARMY.PLURAL, GameImages.ITEM_ARMY_BODYARMOR, DATA_ARMOR_ARMY.PRO_HIT, DATA_ARMOR_ARMY.PRO_SHOT, DATA_ARMOR_ARMY.ENC, DATA_ARMOR_ARMY.WEIGHT, DATA_ARMOR_ARMY.FLAVOR));
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_CHAR_LIGHT_BODYARMOR, DATA_ARMOR_CHAR.NAME, DATA_ARMOR_CHAR.PLURAL, GameImages.ITEM_CHAR_LIGHT_BODYARMOR, DATA_ARMOR_CHAR.PRO_HIT, DATA_ARMOR_CHAR.PRO_SHOT, DATA_ARMOR_CHAR.ENC, DATA_ARMOR_CHAR.WEIGHT, DATA_ARMOR_CHAR.FLAVOR));
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_HELLS_SOULS_JACKET, DATA_ARMOR_HELLS_SOULS_JACKET.NAME, DATA_ARMOR_HELLS_SOULS_JACKET.PLURAL, GameImages.ITEM_HELLS_SOULS_JACKET, DATA_ARMOR_HELLS_SOULS_JACKET.PRO_HIT, DATA_ARMOR_HELLS_SOULS_JACKET.PRO_SHOT, DATA_ARMOR_HELLS_SOULS_JACKET.ENC, DATA_ARMOR_HELLS_SOULS_JACKET.WEIGHT, DATA_ARMOR_HELLS_SOULS_JACKET.FLAVOR));
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_FREE_ANGELS_JACKET, DATA_ARMOR_FREE_ANGELS_JACKET.NAME, DATA_ARMOR_FREE_ANGELS_JACKET.PLURAL, GameImages.ITEM_FREE_ANGELS_JACKET, DATA_ARMOR_FREE_ANGELS_JACKET.PRO_HIT, DATA_ARMOR_FREE_ANGELS_JACKET.PRO_SHOT, DATA_ARMOR_FREE_ANGELS_JACKET.ENC, DATA_ARMOR_FREE_ANGELS_JACKET.WEIGHT, DATA_ARMOR_FREE_ANGELS_JACKET.FLAVOR));
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_POLICE_JACKET, DATA_ARMOR_POLICE_JACKET.NAME, DATA_ARMOR_POLICE_JACKET.PLURAL, GameImages.ITEM_POLICE_JACKET, DATA_ARMOR_POLICE_JACKET.PRO_HIT, DATA_ARMOR_POLICE_JACKET.PRO_SHOT, DATA_ARMOR_POLICE_JACKET.ENC, DATA_ARMOR_POLICE_JACKET.WEIGHT, DATA_ARMOR_POLICE_JACKET.FLAVOR));
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_POLICE_RIOT, DATA_ARMOR_POLICE_RIOT.NAME, DATA_ARMOR_POLICE_RIOT.PLURAL, GameImages.ITEM_POLICE_RIOT_ARMOR, DATA_ARMOR_POLICE_RIOT.PRO_HIT, DATA_ARMOR_POLICE_RIOT.PRO_SHOT, DATA_ARMOR_POLICE_RIOT.ENC, DATA_ARMOR_POLICE_RIOT.WEIGHT, DATA_ARMOR_POLICE_RIOT.FLAVOR));
      _setModel(new ItemBodyArmorModel(IDs.ARMOR_HUNTER_VEST, DATA_ARMOR_HUNTER_VEST.NAME, DATA_ARMOR_HUNTER_VEST.PLURAL, GameImages.ITEM_HUNTER_VEST, DATA_ARMOR_HUNTER_VEST.PRO_HIT, DATA_ARMOR_HUNTER_VEST.PRO_SHOT, DATA_ARMOR_HUNTER_VEST.ENC, DATA_ARMOR_HUNTER_VEST.WEIGHT, DATA_ARMOR_HUNTER_VEST.FLAVOR));

      // trackers
      _setModel(new ItemTrackerModel(IDs.TRACKER_CELL_PHONE, DATA_TRACKER_CELL_PHONE.NAME, DATA_TRACKER_CELL_PHONE.PLURAL, GameImages.ITEM_CELL_PHONE, ItemTrackerModel.TrackingFlags.FOLLOWER_AND_LEADER, DATA_TRACKER_CELL_PHONE.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.LEFT_HAND, DATA_TRACKER_CELL_PHONE.FLAVOR));
      _setModel(new ItemTrackerModel(IDs.TRACKER_ZTRACKER, DATA_TRACKER_ZTRACKER.NAME, DATA_TRACKER_ZTRACKER.PLURAL, GameImages.ITEM_ZTRACKER, ItemTrackerModel.TrackingFlags.UNDEADS, DATA_TRACKER_ZTRACKER.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.LEFT_HAND, DATA_TRACKER_ZTRACKER.FLAVOR));
      _setModel(new ItemTrackerModel(IDs.TRACKER_BLACKOPS, DATA_TRACKER_BLACKOPS_GPS.NAME, DATA_TRACKER_BLACKOPS_GPS.PLURAL, GameImages.ITEM_BLACKOPS_GPS, ItemTrackerModel.TrackingFlags.BLACKOPS_FACTION, DATA_TRACKER_BLACKOPS_GPS.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.LEFT_HAND, DATA_TRACKER_BLACKOPS_GPS.FLAVOR));
      _setModel(new ItemTrackerModel(IDs.TRACKER_POLICE_RADIO, DATA_TRACKER_POLICE_RADIO.NAME, DATA_TRACKER_POLICE_RADIO.PLURAL, GameImages.ITEM_POLICE_RADIO, ItemTrackerModel.TrackingFlags.POLICE_FACTION, DATA_TRACKER_POLICE_RADIO.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.HIP_HOLSTER, DATA_TRACKER_POLICE_RADIO.FLAVOR));

      // spray paint
      _setModel(new ItemSprayPaintModel(IDs.SPRAY_PAINT1, DATA_SPRAY_PAINT1.NAME, DATA_SPRAY_PAINT1.PLURAL, GameImages.ITEM_SPRAYPAINT, DATA_SPRAY_PAINT1.QUANTITY, GameImages.DECO_PLAYER_TAG1, DATA_SPRAY_PAINT1.FLAVOR));
      _setModel(new ItemSprayPaintModel(IDs.SPRAY_PAINT2, DATA_SPRAY_PAINT2.NAME, DATA_SPRAY_PAINT2.PLURAL, GameImages.ITEM_SPRAYPAINT2, DATA_SPRAY_PAINT2.QUANTITY, GameImages.DECO_PLAYER_TAG2, DATA_SPRAY_PAINT2.FLAVOR));
      _setModel(new ItemSprayPaintModel(IDs.SPRAY_PAINT3, DATA_SPRAY_PAINT3.NAME, DATA_SPRAY_PAINT3.PLURAL, GameImages.ITEM_SPRAYPAINT3, DATA_SPRAY_PAINT3.QUANTITY, GameImages.DECO_PLAYER_TAG3, DATA_SPRAY_PAINT3.FLAVOR));
      _setModel(new ItemSprayPaintModel(IDs.SPRAY_PAINT4, DATA_SPRAY_PAINT4.NAME, DATA_SPRAY_PAINT4.PLURAL, GameImages.ITEM_SPRAYPAINT4, DATA_SPRAY_PAINT4.QUANTITY, GameImages.DECO_PLAYER_TAG4, DATA_SPRAY_PAINT4.FLAVOR));

      // Flashlights
      _setModel(new ItemLightModel(IDs.LIGHT_FLASHLIGHT, DATA_LIGHT_FLASHLIGHT.NAME, DATA_LIGHT_FLASHLIGHT.PLURAL, GameImages.ITEM_FLASHLIGHT, DATA_LIGHT_FLASHLIGHT.FOV, DATA_LIGHT_FLASHLIGHT.BATTERIES * WorldTime.TURNS_PER_HOUR, GameImages.ITEM_FLASHLIGHT_OUT, DATA_LIGHT_FLASHLIGHT.FLAVOR));
      _setModel(new ItemLightModel(IDs.LIGHT_BIG_FLASHLIGHT, DATA_LIGHT_BIG_FLASHLIGHT.NAME, DATA_LIGHT_BIG_FLASHLIGHT.PLURAL, GameImages.ITEM_BIG_FLASHLIGHT, DATA_LIGHT_BIG_FLASHLIGHT.FOV, DATA_LIGHT_BIG_FLASHLIGHT.BATTERIES * WorldTime.TURNS_PER_HOUR, GameImages.ITEM_BIG_FLASHLIGHT_OUT, DATA_LIGHT_BIG_FLASHLIGHT.FLAVOR));

      // stench killer
      _setModel(new ItemSprayScentModel(IDs.SCENT_SPRAY_STENCH_KILLER, DATA_SCENT_SPRAY_STENCH_KILLER.NAME, DATA_SCENT_SPRAY_STENCH_KILLER.PLURAL, GameImages.ITEM_STENCH_KILLER, DATA_SCENT_SPRAY_STENCH_KILLER.QUANTITY, Odor.SUPPRESSOR, DATA_SCENT_SPRAY_STENCH_KILLER.STRENGTH * 30, DATA_SCENT_SPRAY_STENCH_KILLER.FLAVOR));

      // Traps
      _setModel(new ItemTrapModel(IDs.TRAP_EMPTY_CAN, DATA_TRAP_EMPTY_CAN.NAME, DATA_TRAP_EMPTY_CAN.PLURAL, GameImages.ITEM_EMPTY_CAN, DATA_TRAP_EMPTY_CAN.STACKING, DATA_TRAP_EMPTY_CAN.CHANCE, DATA_TRAP_EMPTY_CAN.DAMAGE, DATA_TRAP_EMPTY_CAN.DROP_ACTIVATE, DATA_TRAP_EMPTY_CAN.USE_ACTIVATE, DATA_TRAP_EMPTY_CAN.IS_ONE_TIME, DATA_TRAP_EMPTY_CAN.BREAK_CHANCE, DATA_TRAP_EMPTY_CAN.BLOCK_CHANCE, DATA_TRAP_EMPTY_CAN.BREAK_CHANCE_ESCAPE, DATA_TRAP_EMPTY_CAN.IS_NOISY, DATA_TRAP_EMPTY_CAN.NOISE_NAME, DATA_TRAP_EMPTY_CAN.IS_FLAMMABLE, DATA_TRAP_EMPTY_CAN.FLAVOR));
      _setModel(new ItemTrapModel(IDs.TRAP_BEAR_TRAP, DATA_TRAP_BEAR_TRAP.NAME, DATA_TRAP_BEAR_TRAP.PLURAL, GameImages.ITEM_BEAR_TRAP, DATA_TRAP_BEAR_TRAP.STACKING, DATA_TRAP_BEAR_TRAP.CHANCE, DATA_TRAP_BEAR_TRAP.DAMAGE, DATA_TRAP_BEAR_TRAP.DROP_ACTIVATE, DATA_TRAP_BEAR_TRAP.USE_ACTIVATE, DATA_TRAP_BEAR_TRAP.IS_ONE_TIME, DATA_TRAP_BEAR_TRAP.BREAK_CHANCE, DATA_TRAP_BEAR_TRAP.BLOCK_CHANCE, DATA_TRAP_BEAR_TRAP.BREAK_CHANCE_ESCAPE, DATA_TRAP_BEAR_TRAP.IS_NOISY, DATA_TRAP_BEAR_TRAP.NOISE_NAME, DATA_TRAP_BEAR_TRAP.IS_FLAMMABLE, DATA_TRAP_BEAR_TRAP.FLAVOR));
      _setModel(new ItemTrapModel(IDs.TRAP_SPIKES, DATA_TRAP_SPIKES.NAME, DATA_TRAP_SPIKES.PLURAL, GameImages.ITEM_SPIKES, DATA_TRAP_SPIKES.STACKING, DATA_TRAP_SPIKES.CHANCE, DATA_TRAP_SPIKES.DAMAGE, DATA_TRAP_SPIKES.DROP_ACTIVATE, DATA_TRAP_SPIKES.USE_ACTIVATE, DATA_TRAP_SPIKES.IS_ONE_TIME, DATA_TRAP_SPIKES.BREAK_CHANCE, DATA_TRAP_SPIKES.BLOCK_CHANCE, DATA_TRAP_SPIKES.BREAK_CHANCE_ESCAPE, DATA_TRAP_SPIKES.IS_NOISY, DATA_TRAP_SPIKES.NOISE_NAME, DATA_TRAP_SPIKES.IS_FLAMMABLE, DATA_TRAP_SPIKES.FLAVOR));
      _setModel(new ItemTrapModel(IDs.TRAP_BARBED_WIRE, DATA_TRAP_BARBED_WIRE.NAME, DATA_TRAP_BARBED_WIRE.PLURAL, GameImages.ITEM_BARBED_WIRE, DATA_TRAP_BARBED_WIRE.STACKING, DATA_TRAP_BARBED_WIRE.CHANCE, DATA_TRAP_BARBED_WIRE.DAMAGE, DATA_TRAP_BARBED_WIRE.DROP_ACTIVATE, DATA_TRAP_BARBED_WIRE.USE_ACTIVATE, DATA_TRAP_BARBED_WIRE.IS_ONE_TIME, DATA_TRAP_BARBED_WIRE.BREAK_CHANCE, DATA_TRAP_BARBED_WIRE.BLOCK_CHANCE, DATA_TRAP_BARBED_WIRE.BREAK_CHANCE_ESCAPE, DATA_TRAP_BARBED_WIRE.IS_NOISY, DATA_TRAP_BARBED_WIRE.NOISE_NAME, DATA_TRAP_BARBED_WIRE.IS_FLAMMABLE, DATA_TRAP_BARBED_WIRE.FLAVOR));

      // entertainment
      _setModel(new ItemEntertainmentModel(IDs.ENT_BOOK, DATA_ENT_BOOK.NAME, DATA_ENT_BOOK.PLURAL, GameImages.ITEM_BOOK, DATA_ENT_BOOK.VALUE, DATA_ENT_BOOK.BORECHANCE, DATA_ENT_BOOK.STACKING, DATA_ENT_BOOK.FLAVOR));
      _setModel(new ItemEntertainmentModel(IDs.ENT_MAGAZINE, DATA_ENT_MAGAZINE.NAME, DATA_ENT_MAGAZINE.PLURAL, GameImages.ITEM_MAGAZINE, DATA_ENT_MAGAZINE.VALUE, DATA_ENT_MAGAZINE.BORECHANCE, DATA_ENT_MAGAZINE.STACKING, DATA_ENT_MAGAZINE.FLAVOR));
      // this manual is *very* relevant, but dry reading.
      _setModel(new ItemEntertainmentModel(IDs.ENT_CHAR_GUARD_MANUAL, "CHAR Guard Manual","CHAR Guard Manuals", GameImages.ITEM_BOOK, DATA_ENT_MAGAZINE.VALUE, 0, DATA_ENT_BOOK.STACKING, DATA_ENT_BOOK.FLAVOR));

      _setModel(new ItemModel(IDs.UNIQUE_SUBWAY_BADGE, "Subway Worker Badge", "Subways Worker Badges", GameImages.ITEM_SUBWAY_BADGE, "You got yourself a new job!", DollPart.LEFT_HAND, true)
      {
        IsUnique = true,
        IsForbiddenToAI = true
      });
    }

    private static void Notify(IRogueUI ui, string what, string stage)
    {
      ui.UI_Clear(Color.Black);
      ui.UI_DrawStringBold(Color.White, "Loading " + what + " data : " + stage, 0, 0, new Color?());
      ui.UI_Repaint();
    }

    private static void LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, GameItems.IDs[] idsToRead, out _DATA_TYPE_[] data)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      Notify(ui, kind, "loading file...");
      List<string> stringList = new List<string>();
      bool flag = true;
      using (StreamReader streamReader = File.OpenText(path)) {
        while (!streamReader.EndOfStream) {
          string str = streamReader.ReadLine();
          if (flag) flag = false;
          else stringList.Add(str);
        }
      }
      Notify(ui, kind, "parsing CSV...");
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), fieldsCount);
      Notify(ui, kind, "reading data...");
      data = new _DATA_TYPE_[idsToRead.Length];
      for (int index = 0; index < idsToRead.Length; ++index)
        data[index] = toTable.GetDataFor<_DATA_TYPE_, GameItems.IDs>(fn, idsToRead[index]);
      Notify(ui, kind, "done!");
    }

    private void LoadMedicineFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "medicine items", MedecineData.COUNT_FIELDS, new Func<CSVLine, MedecineData>(MedecineData.FromCSVLine), new IDs[6] {
        IDs.MEDICINE_BANDAGES,
        IDs.MEDICINE_MEDIKIT,
        IDs.MEDICINE_PILLS_SLP,
        IDs.MEDICINE_PILLS_STA,
        IDs.MEDICINE_PILLS_SAN,
        IDs.MEDICINE_PILLS_ANTIVIRAL
      }, out MedecineData[] data);
      DATA_MEDICINE_BANDAGE = data[0];
      DATA_MEDICINE_MEDIKIT = data[1];
      DATA_MEDICINE_PILLS_SLP = data[2];
      DATA_MEDICINE_PILLS_STA = data[3];
      DATA_MEDICINE_PILLS_SAN = data[4];
      DATA_MEDICINE_PILLS_ANTIVIRAL = data[5];
    }

    private void LoadFoodFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "food items", FoodData.COUNT_FIELDS, new Func<CSVLine, FoodData>(FoodData.FromCSVLine), new IDs[3] {
        IDs.FOOD_ARMY_RATION,
        IDs.FOOD_CANNED_FOOD,
        IDs.FOOD_GROCERIES
      }, out FoodData[] data);
      DATA_FOOD_ARMY_RATION = data[0];
      DATA_FOOD_CANNED_FOOD = data[1];
      DATA_FOOD_GROCERIES = data[2];
    }

    private void LoadMeleeWeaponsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "melee weapons items", MeleeWeaponData.COUNT_FIELDS, new Func<CSVLine, MeleeWeaponData>(MeleeWeaponData.FromCSVLine), new IDs[] {
        IDs.MELEE_BASEBALLBAT,
        IDs.MELEE_COMBAT_KNIFE,
        IDs.MELEE_CROWBAR,
        IDs.MELEE_GOLFCLUB,
        IDs.MELEE_HUGE_HAMMER,
        IDs.MELEE_IRON_GOLFCLUB,
        IDs.MELEE_SHOVEL,
        IDs.MELEE_SHORT_SHOVEL,
        IDs.MELEE_TRUNCHEON,
        IDs.UNIQUE_JASON_MYERS_AXE,
        IDs.MELEE_IMPROVISED_CLUB,
        IDs.MELEE_IMPROVISED_SPEAR,
        IDs.MELEE_SMALL_HAMMER,
        IDs.UNIQUE_FAMU_FATARU_KATANA,
        IDs.UNIQUE_BIGBEAR_BAT,
        IDs.UNIQUE_ROGUEDJACK_KEYBOARD,
        IDs.UNIQUE_FATHER_TIME_SCYTHE
      }, out MeleeWeaponData[] data);
      DATA_MELEE_BASEBALLBAT = data[0];
      DATA_MELEE_COMBAT_KNIFE = data[1];
      DATA_MELEE_CROWBAR = data[2];
      DATA_MELEE_GOLFCLUB = data[3];
      DATA_MELEE_HUGE_HAMMER = data[4];
      DATA_MELEE_IRON_GOLFCLUB = data[5];
      DATA_MELEE_SHOVEL = data[6];
      DATA_MELEE_SHORT_SHOVEL = data[7];
      DATA_MELEE_TRUNCHEON = data[8];
      DATA_MELEE_UNIQUE_JASON_MYERS_AXE = data[9];
      DATA_MELEE_IMPROVISED_CLUB = data[10];
      DATA_MELEE_IMPROVISED_SPEAR = data[11];
      DATA_MELEE_SMALL_HAMMER = data[12];
      DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA = data[13];
      DATA_MELEE_UNIQUE_BIGBEAR_BAT = data[14];
      DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD = data[15];
      DATA_MELEE_UNIQUE_FATHER_TIME_SCYTHE = data[16];
    }

    private void LoadRangedWeaponsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "ranged weapons items", RangedWeaponData.COUNT_FIELDS, new Func<CSVLine, RangedWeaponData>(RangedWeaponData.FromCSVLine), new IDs[10] {
        IDs.RANGED_ARMY_PISTOL,
        IDs.RANGED_ARMY_RIFLE,
        IDs.RANGED_HUNTING_CROSSBOW,
        IDs.RANGED_HUNTING_RIFLE,
        IDs.RANGED_KOLT_REVOLVER,
        IDs.RANGED_PISTOL,
        IDs.RANGED_PRECISION_RIFLE,
        IDs.RANGED_SHOTGUN,
        IDs.UNIQUE_SANTAMAN_SHOTGUN,
        IDs.UNIQUE_HANS_VON_HANZ_PISTOL
      }, out RangedWeaponData[] data);
      DATA_RANGED_ARMY_PISTOL = data[0];
      DATA_RANGED_ARMY_RIFLE = data[1];
      DATA_RANGED_HUNTING_CROSSBOW = data[2];
      DATA_RANGED_HUNTING_RIFLE = data[3];
      DATA_RANGED_KOLT_REVOLVER = data[4];
      DATA_RANGED_PISTOL = data[5];
      DATA_RANGED_PRECISION_RIFLE = data[6];
      DATA_RANGED_SHOTGUN = data[7];
      DATA_UNIQUE_SANTAMAN_SHOTGUN = data[8];
      DATA_UNIQUE_HANS_VON_HANZ_PISTOL = data[9];
    }

    private void LoadExplosivesFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "explosives items", ExplosiveData.COUNT_FIELDS, new Func<CSVLine, ExplosiveData>(ExplosiveData.FromCSVLine), new IDs[1] {
        IDs.EXPLOSIVE_GRENADE
      }, out ExplosiveData[] data);
      DATA_EXPLOSIVE_GRENADE = data[0];
    }

    private void LoadBarricadingMaterialFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "barricading items", BarricadingMaterialData.COUNT_FIELDS, new Func<CSVLine, BarricadingMaterialData>(BarricadingMaterialData.FromCSVLine), new IDs[1] {
        IDs.BAR_WOODEN_PLANK
      }, out BarricadingMaterialData[] data);
      DATA_BAR_WOODEN_PLANK = data[0];
    }

    private void LoadArmorsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "armors items", ArmorData.COUNT_FIELDS, new Func<CSVLine, ArmorData>(ArmorData.FromCSVLine), new IDs[7] {
        IDs.ARMOR_ARMY_BODYARMOR,
        IDs.ARMOR_CHAR_LIGHT_BODYARMOR,
        IDs.ARMOR_HELLS_SOULS_JACKET,
        IDs.ARMOR_FREE_ANGELS_JACKET,
        IDs.ARMOR_POLICE_JACKET,
        IDs.ARMOR_POLICE_RIOT,
        IDs.ARMOR_HUNTER_VEST
      }, out ArmorData[] data);
      DATA_ARMOR_ARMY = data[0];
      DATA_ARMOR_CHAR = data[1];
      DATA_ARMOR_HELLS_SOULS_JACKET = data[2];
      DATA_ARMOR_FREE_ANGELS_JACKET = data[3];
      DATA_ARMOR_POLICE_JACKET = data[4];
      DATA_ARMOR_POLICE_RIOT = data[5];
      DATA_ARMOR_HUNTER_VEST = data[6];
    }

    private void LoadTrackersFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "trackers items", TrackerData.COUNT_FIELDS, new Func<CSVLine, TrackerData>(TrackerData.FromCSVLine), new IDs[4] {
        IDs.TRACKER_BLACKOPS,
        IDs.TRACKER_CELL_PHONE,
        IDs.TRACKER_ZTRACKER,
        IDs.TRACKER_POLICE_RADIO
      }, out TrackerData[] data);
      DATA_TRACKER_BLACKOPS_GPS = data[0];
      DATA_TRACKER_CELL_PHONE = data[1];
      DATA_TRACKER_ZTRACKER = data[2];
      DATA_TRACKER_POLICE_RADIO = data[3];
    }

    private void LoadSpraypaintsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "spraypaint items", SprayPaintData.COUNT_FIELDS, new Func<CSVLine, SprayPaintData>(SprayPaintData.FromCSVLine), new IDs[4] {
        IDs.SPRAY_PAINT1,
        IDs.SPRAY_PAINT2,
        IDs.SPRAY_PAINT3,
        IDs.SPRAY_PAINT4
      }, out SprayPaintData[] data);
      DATA_SPRAY_PAINT1 = data[0];
      DATA_SPRAY_PAINT2 = data[1];
      DATA_SPRAY_PAINT3 = data[2];
      DATA_SPRAY_PAINT4 = data[3];
    }

    private void LoadLightsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "lights items", LightData.COUNT_FIELDS, new Func<CSVLine, LightData>(LightData.FromCSVLine), new IDs[2] {
        IDs.LIGHT_FLASHLIGHT,
        IDs.LIGHT_BIG_FLASHLIGHT
      }, out LightData[] data);
      DATA_LIGHT_FLASHLIGHT = data[0];
      DATA_LIGHT_BIG_FLASHLIGHT = data[1];
    }

    private void LoadScentspraysFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "scentsprays items", ScentSprayData.COUNT_FIELDS, new Func<CSVLine, ScentSprayData>(ScentSprayData.FromCSVLine), new IDs[1] {
        IDs.SCENT_SPRAY_STENCH_KILLER
      }, out ScentSprayData[] data);
      DATA_SCENT_SPRAY_STENCH_KILLER = data[0];
    }

    private void LoadTrapsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "traps items", TrapData.COUNT_FIELDS, new Func<CSVLine, TrapData>(TrapData.FromCSVLine), new IDs[4]{
        IDs.TRAP_EMPTY_CAN,
        IDs.TRAP_BEAR_TRAP,
        IDs.TRAP_SPIKES,
        IDs.TRAP_BARBED_WIRE
      }, out TrapData[] data);
      DATA_TRAP_EMPTY_CAN = data[0];
      DATA_TRAP_BEAR_TRAP = data[1];
      DATA_TRAP_SPIKES = data[2];
      DATA_TRAP_BARBED_WIRE = data[3];
    }

    private void LoadEntertainmentFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "entertainment items", EntData.COUNT_FIELDS, new Func<CSVLine, EntData>(EntData.FromCSVLine), new IDs[2] {
        IDs.ENT_BOOK,
        IDs.ENT_MAGAZINE
      }, out EntData[] data);
      DATA_ENT_BOOK = data[0];
      DATA_ENT_MAGAZINE = data[1];
    }

    public enum IDs
    {
      MEDICINE_BANDAGES = 0,
      MEDICINE_MEDIKIT = 1,
      MEDICINE_PILLS_STA = 2,
      MEDICINE_PILLS_SLP = 3,
      MEDICINE_PILLS_SAN = 4,
      MEDICINE_PILLS_ANTIVIRAL = 5,
      FOOD_ARMY_RATION = 6,
      FOOD_GROCERIES = 7,
      FOOD_CANNED_FOOD = 8,
      MELEE_BASEBALLBAT = 9,
      MELEE_COMBAT_KNIFE = 10,
      MELEE_CROWBAR = 11,
      UNIQUE_JASON_MYERS_AXE = 12,
      MELEE_HUGE_HAMMER = 13,
      MELEE_SMALL_HAMMER = 14,
      MELEE_GOLFCLUB = 15,
      MELEE_IRON_GOLFCLUB = 16,
      MELEE_SHOVEL = 17,
      MELEE_SHORT_SHOVEL = 18,
      MELEE_TRUNCHEON = 19,
      MELEE_IMPROVISED_CLUB = 20,
      MELEE_IMPROVISED_SPEAR = 21,
      RANGED_ARMY_PISTOL = 22,
      RANGED_ARMY_RIFLE = 23,
      RANGED_HUNTING_CROSSBOW = 24,
      RANGED_HUNTING_RIFLE = 25,
      RANGED_PISTOL = 26,
      RANGED_KOLT_REVOLVER = 27,
      RANGED_PRECISION_RIFLE = 28,
      RANGED_SHOTGUN = 29,
      EXPLOSIVE_GRENADE = 30,
      EXPLOSIVE_GRENADE_PRIMED = 31,
      BAR_WOODEN_PLANK = 32,
      ARMOR_HUNTER_VEST = 33,
      ARMOR_CHAR_LIGHT_BODYARMOR = 34,
      ARMOR_ARMY_BODYARMOR = 35,
      ARMOR_POLICE_JACKET = 36,
      ARMOR_POLICE_RIOT = 37,
      ARMOR_HELLS_SOULS_JACKET = 38,
      ARMOR_FREE_ANGELS_JACKET = 39,
      TRACKER_BLACKOPS = 40,
      TRACKER_CELL_PHONE = 41,
      TRACKER_ZTRACKER = 42,
      TRACKER_POLICE_RADIO = 43,
      SPRAY_PAINT1 = 44,
      SPRAY_PAINT2 = 45,
      SPRAY_PAINT3 = 46,
      SPRAY_PAINT4 = 47,
      SCENT_SPRAY_STENCH_KILLER = 48,
      LIGHT_FLASHLIGHT = 49,
      LIGHT_BIG_FLASHLIGHT = 50,
      AMMO_LIGHT_PISTOL = 51,   // XXX logical origin for AmmoType enum when doing C-style conversion
      AMMO_HEAVY_PISTOL = 52,
      AMMO_LIGHT_RIFLE = 53,
      AMMO_HEAVY_RIFLE = 54,
      AMMO_SHOTGUN = 55,
      AMMO_BOLTS = 56,
      TRAP_EMPTY_CAN = 57,
      TRAP_BEAR_TRAP = 58,
      TRAP_SPIKES = 59,
      TRAP_BARBED_WIRE = 60,
      ENT_BOOK = 61,
      ENT_MAGAZINE = 62,
      UNIQUE_SUBWAY_BADGE = 63,
      UNIQUE_FAMU_FATARU_KATANA = 64,
      UNIQUE_BIGBEAR_BAT = 65,
      UNIQUE_ROGUEDJACK_KEYBOARD = 66,
      UNIQUE_SANTAMAN_SHOTGUN = 67,
      UNIQUE_HANS_VON_HANZ_PISTOL = 68,
      ENT_CHAR_GUARD_MANUAL = 69,
      UNIQUE_FATHER_TIME_SCYTHE = 70,
      _COUNT    // default this to guarantee correct value
    }

    private struct MedecineData
    {
      public const int COUNT_FIELDS = 10;

      public string NAME;
      public string PLURAL;
      public int STACKINGLIMIT;
      public int HEALING;
      public int STAMINABOOST;
      public int SLEEPBOOST;
      public int INFECTIONCURE;
      public int SANITYCURE;
      public string FLAVOR;

      public static MedecineData FromCSVLine(CSVLine line)
      {
        return new MedecineData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          HEALING = line[3].ParseInt(),
          STAMINABOOST = line[4].ParseInt(),
          SLEEPBOOST = line[5].ParseInt(),
          INFECTIONCURE = line[6].ParseInt(),
          SANITYCURE = line[7].ParseInt(),
          STACKINGLIMIT = line[8].ParseInt(),
          FLAVOR = line[9].ParseText()
        };
      }
    }

    private struct FoodData
    {
      public const int COUNT_FIELDS = 7;

      public string NAME;
      public string PLURAL;
      public int NUTRITION;
      public int BESTBEFORE;
      public int STACKINGLIMIT;
      public string FLAVOR;

      public static FoodData FromCSVLine(CSVLine line)
      {
        return new FoodData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          NUTRITION = (int) ((double) Rules.FOOD_BASE_POINTS * (double) line[3].ParseFloat()),
          BESTBEFORE = line[4].ParseInt(),
          STACKINGLIMIT = line[5].ParseInt(),
          FLAVOR = line[6].ParseText()
        };
      }
    }

    private struct MeleeWeaponData
    {
      public const int COUNT_FIELDS = 11;

      public string NAME;
      public int ATK;
      public int DMG;
      public int STA;
      public int DISARM { get; set; }  // alpha10
      public int TOOLBASHDMGBONUS { get; set; }  // alpha10
      public float TOOLBUILDBONUS { get; set; } // alpha10
      public int STACKINGLIMIT;
      public bool ISFRAGILE;
      public string FLAVOR;

      public static MeleeWeaponData FromCSVLine(CSVLine line)
      {
        return new MeleeWeaponData {
          NAME = line[1].ParseText(),
          ATK = line[2].ParseInt(),
          DMG = line[3].ParseInt(),
          STA = line[4].ParseInt(),
          DISARM = line[5].ParseInt(),  // alpha10
          TOOLBASHDMGBONUS = line[6].ParseInt(), // alpha10
          TOOLBUILDBONUS = line[7].ParseFloat(),  // alpha10
          STACKINGLIMIT = line[8].ParseInt(),
          ISFRAGILE = line[9].ParseBool(),
          FLAVOR = line[10].ParseText()
        };
      }
    }

    private struct RangedWeaponData
    {
      public const int COUNT_FIELDS = 10;   // alpha 10

      public string NAME;
      public string PLURAL;
      public int ATK;
      public int RAPID1;   // alpha 10
      public int RAPID2;   // alpha 10
      public int DMG;
      public short RANGE;
      public int MAXAMMO;
      public string FLAVOR;

      public static RangedWeaponData FromCSVLine(CSVLine line)
      {
        return new RangedWeaponData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          ATK = line[3].ParseInt(),
          RAPID1 = line[4].ParseInt(),
          RAPID2 = line[5].ParseInt(),
          DMG = line[6].ParseInt(),
          RANGE = (short)line[7].ParseInt(),
          MAXAMMO = line[8].ParseInt(),
          FLAVOR = line[9].ParseText()
        };
      }
    }

    private struct ExplosiveData
    {
      public const int COUNT_FIELDS = 14;

      public string NAME;
      public string PLURAL;
      public int FUSE;
      public int MAXTHROW;
      public int STACKLINGLIMIT;
      public int RADIUS;
      public int[] DMG;
      public string FLAVOR;

      public static ExplosiveData FromCSVLine(CSVLine line)
      {
        return new ExplosiveData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          FUSE = line[3].ParseInt(),
          MAXTHROW = line[4].ParseInt(),
          STACKLINGLIMIT = line[5].ParseInt(),
          RADIUS = line[6].ParseInt(),
          DMG = new int[6] {
            line[7].ParseInt(),
            line[8].ParseInt(),
            line[9].ParseInt(),
            line[10].ParseInt(),
            line[11].ParseInt(),
            line[12].ParseInt()
          },
          FLAVOR = line[13].ParseText()
        };
      }
    }

    private struct BarricadingMaterialData
    {
      public const int COUNT_FIELDS = 6;

      public string NAME;
      public string PLURAL;
      public int VALUE;
      public int STACKINGLIMIT;
      public string FLAVOR;

      public static BarricadingMaterialData FromCSVLine(CSVLine line)
      {
        return new BarricadingMaterialData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          VALUE = line[3].ParseInt(),
          STACKINGLIMIT = line[4].ParseInt(),
          FLAVOR = line[5].ParseText()
        };
      }
    }

    private struct ArmorData
    {
      public const int COUNT_FIELDS = 8;

      public string NAME;
      public string PLURAL;
      public int PRO_HIT;
      public int PRO_SHOT;
      public int ENC;
      public int WEIGHT;
      public string FLAVOR;

      public static ArmorData FromCSVLine(CSVLine line)
      {
        return new ArmorData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          PRO_HIT = line[3].ParseInt(),
          PRO_SHOT = line[4].ParseInt(),
          ENC = line[5].ParseInt(),
          WEIGHT = line[6].ParseInt(),
          FLAVOR = line[7].ParseText()
        };
      }
    }

    private struct TrackerData
    {
      public const int COUNT_FIELDS = 5;

      public string NAME;
      public string PLURAL;
      public int BATTERIES;
      public string FLAVOR;

      public static TrackerData FromCSVLine(CSVLine line)
      {
        return new TrackerData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          BATTERIES = line[3].ParseInt(),
          FLAVOR = line[4].ParseText()
        };
      }
    }

    private struct SprayPaintData
    {
      public const int COUNT_FIELDS = 5;

      public string NAME;
      public string PLURAL;
      public int QUANTITY;
      public string FLAVOR;

      public static SprayPaintData FromCSVLine(CSVLine line)
      {
        return new SprayPaintData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          QUANTITY = line[3].ParseInt(),
          FLAVOR = line[4].ParseText()
        };
      }
    }

    private struct LightData
    {
      public const int COUNT_FIELDS = 6;

      public string NAME;
      public string PLURAL;
      public short FOV;
      public int BATTERIES;
      public string FLAVOR;

      public static LightData FromCSVLine(CSVLine line)
      {
        return new LightData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          FOV = (short)line[3].ParseInt(),
          BATTERIES = line[4].ParseInt(),
          FLAVOR = line[5].ParseText()
        };
      }
    }

    private struct ScentSprayData
    {
      public const int COUNT_FIELDS = 6;

      public string NAME;
      public string PLURAL;
      public int QUANTITY;
      public int STRENGTH;
      public string FLAVOR;

      public static ScentSprayData FromCSVLine(CSVLine line)
      {
        return new ScentSprayData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          QUANTITY = line[3].ParseInt(),
          STRENGTH = line[4].ParseInt(),
          FLAVOR = line[5].ParseText()
        };
      }
    }

    private struct TrapData
    {
      public const int COUNT_FIELDS = 16;

      public string NAME;
      public string PLURAL;
      public int STACKING;
      public bool USE_ACTIVATE;
      public int CHANCE;
      public int DAMAGE;
      public bool DROP_ACTIVATE;
      public bool IS_ONE_TIME;
      public int BREAK_CHANCE;
      public int BLOCK_CHANCE;
      public int BREAK_CHANCE_ESCAPE;
      public bool IS_NOISY;
      public string NOISE_NAME;
      public bool IS_FLAMMABLE;
      public string FLAVOR;

      public static TrapData FromCSVLine(CSVLine line)
      {
        return new TrapData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          STACKING = line[3].ParseInt(),
          DROP_ACTIVATE = line[4].ParseBool(),
          USE_ACTIVATE = line[5].ParseBool(),
          CHANCE = line[6].ParseInt(),
          DAMAGE = line[7].ParseInt(),
          IS_ONE_TIME = line[8].ParseBool(),
          BREAK_CHANCE = line[9].ParseInt(),
          BLOCK_CHANCE = line[10].ParseInt(),
          BREAK_CHANCE_ESCAPE = line[11].ParseInt(),
          IS_NOISY = line[12].ParseBool(),
          NOISE_NAME = line[13].ParseText(),
          IS_FLAMMABLE = line[14].ParseBool(),
          FLAVOR = line[15].ParseText()
        };
      }
    }

    private struct EntData
    {
      public const int COUNT_FIELDS = 7;

      public string NAME;
      public string PLURAL;
      public int STACKING;
      public int VALUE;
      public int BORECHANCE;
      public string FLAVOR;

      public static EntData FromCSVLine(CSVLine line)
      {
        return new EntData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          STACKING = line[3].ParseInt(),
          VALUE = line[4].ParseInt(),
          BORECHANCE = line[5].ParseInt(),
          FLAVOR = line[6].ParseText()
        };
      }
    }
  }
}
