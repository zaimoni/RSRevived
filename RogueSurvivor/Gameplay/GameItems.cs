// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameItems
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

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
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.MELEE_BASEBALLBAT,
            IDs.MELEE_COMBAT_KNIFE,
            IDs.MELEE_CROWBAR,
            IDs.UNIQUE_JASON_MYERS_AXE,
            IDs.MELEE_HUGE_HAMMER,
            IDs.MELEE_SMALL_HAMMER,
            IDs.MELEE_GOLFCLUB,
            IDs.MELEE_IRON_GOLFCLUB,
            IDs.MELEE_SHOVEL,
            IDs.MELEE_SHORT_SHOVEL,
            IDs.MELEE_TRUNCHEON,
            IDs.MELEE_IMPROVISED_CLUB,
            IDs.MELEE_IMPROVISED_SPEAR});
    public static readonly System.Collections.ObjectModel.ReadOnlyCollection<IDs> medicine
    = new System.Collections.ObjectModel.ReadOnlyCollection<IDs>(new List<IDs> { IDs.MEDICINE_BANDAGES,
        IDs.MEDICINE_MEDIKIT,
        IDs.MEDICINE_PILLS_STA,
        IDs.MEDICINE_PILLS_SLP,
        IDs.MEDICINE_PILLS_SAN,
        IDs.MEDICINE_PILLS_ANTIVIRAL});

    private GameItems.MedecineData DATA_MEDICINE_BANDAGE;
    private GameItems.MedecineData DATA_MEDICINE_MEDIKIT;
    private GameItems.MedecineData DATA_MEDICINE_PILLS_STA;
    private GameItems.MedecineData DATA_MEDICINE_PILLS_SLP;
    private GameItems.MedecineData DATA_MEDICINE_PILLS_SAN;
    private GameItems.MedecineData DATA_MEDICINE_PILLS_ANTIVIRAL;
    private GameItems.FoodData DATA_FOOD_ARMY_RATION;
    private GameItems.FoodData DATA_FOOD_GROCERIES;
    private GameItems.FoodData DATA_FOOD_CANNED_FOOD;
    private GameItems.MeleeWeaponData DATA_MELEE_CROWBAR;
    private GameItems.MeleeWeaponData DATA_MELEE_BASEBALLBAT;
    private GameItems.MeleeWeaponData DATA_MELEE_COMBAT_KNIFE;
    private GameItems.MeleeWeaponData DATA_MELEE_UNIQUE_JASON_MYERS_AXE;
    private GameItems.MeleeWeaponData DATA_MELEE_GOLFCLUB;
    private GameItems.MeleeWeaponData DATA_MELEE_HUGE_HAMMER;
    private GameItems.MeleeWeaponData DATA_MELEE_SMALL_HAMMER;
    private GameItems.MeleeWeaponData DATA_MELEE_IRON_GOLFCLUB;
    private GameItems.MeleeWeaponData DATA_MELEE_SHOVEL;
    private GameItems.MeleeWeaponData DATA_MELEE_SHORT_SHOVEL;
    private GameItems.MeleeWeaponData DATA_MELEE_TRUNCHEON;
    private GameItems.MeleeWeaponData DATA_MELEE_IMPROVISED_CLUB;
    private GameItems.MeleeWeaponData DATA_MELEE_IMPROVISED_SPEAR;
    private GameItems.MeleeWeaponData DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA;
    private GameItems.MeleeWeaponData DATA_MELEE_UNIQUE_BIGBEAR_BAT;
    private GameItems.MeleeWeaponData DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD;
    private GameItems.RangedWeaponData DATA_RANGED_ARMY_PISTOL;
    private GameItems.RangedWeaponData DATA_RANGED_ARMY_RIFLE;
    private GameItems.RangedWeaponData DATA_RANGED_HUNTING_CROSSBOW;
    private GameItems.RangedWeaponData DATA_RANGED_HUNTING_RIFLE;
    private GameItems.RangedWeaponData DATA_RANGED_KOLT_REVOLVER;
    private GameItems.RangedWeaponData DATA_RANGED_PISTOL;
    private GameItems.RangedWeaponData DATA_RANGED_PRECISION_RIFLE;
    private GameItems.RangedWeaponData DATA_RANGED_SHOTGUN;
    private GameItems.RangedWeaponData DATA_UNIQUE_SANTAMAN_SHOTGUN;
    private GameItems.RangedWeaponData DATA_UNIQUE_HANS_VON_HANZ_PISTOL;
    private GameItems.ExplosiveData DATA_EXPLOSIVE_GRENADE;
    private GameItems.BarricadingMaterialData DATA_BAR_WOODEN_PLANK;
    private GameItems.ArmorData DATA_ARMOR_ARMY;
    private GameItems.ArmorData DATA_ARMOR_CHAR;
    private GameItems.ArmorData DATA_ARMOR_HELLS_SOULS_JACKET;
    private GameItems.ArmorData DATA_ARMOR_FREE_ANGELS_JACKET;
    private GameItems.ArmorData DATA_ARMOR_POLICE_JACKET;
    private GameItems.ArmorData DATA_ARMOR_POLICE_RIOT;
    private GameItems.ArmorData DATA_ARMOR_HUNTER_VEST;
    private GameItems.TrackerData DATA_TRACKER_BLACKOPS_GPS;
    private GameItems.TrackerData DATA_TRACKER_CELL_PHONE;
    private GameItems.TrackerData DATA_TRACKER_ZTRACKER;
    private GameItems.TrackerData DATA_TRACKER_POLICE_RADIO;
    private GameItems.SprayPaintData DATA_SPRAY_PAINT1;
    private GameItems.SprayPaintData DATA_SPRAY_PAINT2;
    private GameItems.SprayPaintData DATA_SPRAY_PAINT3;
    private GameItems.SprayPaintData DATA_SPRAY_PAINT4;
    private GameItems.LightData DATA_LIGHT_FLASHLIGHT;
    private GameItems.LightData DATA_LIGHT_BIG_FLASHLIGHT;
    private GameItems.ScentSprayData DATA_SCENT_SPRAY_STENCH_KILLER;
    private GameItems.TrapData DATA_TRAP_EMPTY_CAN;
    private GameItems.TrapData DATA_TRAP_BEAR_TRAP;
    private GameItems.TrapData DATA_TRAP_SPIKES;
    private GameItems.TrapData DATA_TRAP_BARBED_WIRE;
    private GameItems.EntData DATA_ENT_BOOK;
    private GameItems.EntData DATA_ENT_MAGAZINE;

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

    private static void _setModel(IDs id, ItemModel model)
    {
      m_Models[(int) id] = model;
      m_Models[(int) id].ID = id;
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
#endif
      // Medicine
      _setModel(IDs.MEDICINE_BANDAGES, new ItemMedicineModel(DATA_MEDICINE_BANDAGE.NAME, DATA_MEDICINE_BANDAGE.PLURAL, GameImages.ITEM_BANDAGES, DATA_MEDICINE_BANDAGE.HEALING, DATA_MEDICINE_BANDAGE.STAMINABOOST, DATA_MEDICINE_BANDAGE.SLEEPBOOST, DATA_MEDICINE_BANDAGE.INFECTIONCURE, DATA_MEDICINE_BANDAGE.SANITYCURE, DATA_MEDICINE_BANDAGE.FLAVOR, DATA_MEDICINE_BANDAGE.STACKINGLIMIT));
      _setModel(IDs.MEDICINE_MEDIKIT, new ItemMedicineModel(DATA_MEDICINE_MEDIKIT.NAME, DATA_MEDICINE_MEDIKIT.PLURAL, GameImages.ITEM_MEDIKIT, DATA_MEDICINE_MEDIKIT.HEALING, DATA_MEDICINE_MEDIKIT.STAMINABOOST, DATA_MEDICINE_MEDIKIT.SLEEPBOOST, DATA_MEDICINE_MEDIKIT.INFECTIONCURE, DATA_MEDICINE_MEDIKIT.SANITYCURE, DATA_MEDICINE_MEDIKIT.FLAVOR));
      _setModel(IDs.MEDICINE_PILLS_STA, new ItemMedicineModel(DATA_MEDICINE_PILLS_STA.NAME, DATA_MEDICINE_PILLS_STA.PLURAL, GameImages.ITEM_PILLS_GREEN, DATA_MEDICINE_PILLS_STA.HEALING, DATA_MEDICINE_PILLS_STA.STAMINABOOST, DATA_MEDICINE_PILLS_STA.SLEEPBOOST, DATA_MEDICINE_PILLS_STA.INFECTIONCURE, DATA_MEDICINE_PILLS_STA.SANITYCURE, DATA_MEDICINE_PILLS_STA.FLAVOR, DATA_MEDICINE_PILLS_STA.STACKINGLIMIT));
      _setModel(IDs.MEDICINE_PILLS_SLP, new ItemMedicineModel(DATA_MEDICINE_PILLS_SLP.NAME, DATA_MEDICINE_PILLS_SLP.PLURAL, GameImages.ITEM_PILLS_BLUE, DATA_MEDICINE_PILLS_SLP.HEALING, DATA_MEDICINE_PILLS_SLP.STAMINABOOST, DATA_MEDICINE_PILLS_SLP.SLEEPBOOST, DATA_MEDICINE_PILLS_SLP.INFECTIONCURE, DATA_MEDICINE_PILLS_SLP.SANITYCURE, DATA_MEDICINE_PILLS_SLP.FLAVOR, DATA_MEDICINE_PILLS_SLP.STACKINGLIMIT));
      _setModel(IDs.MEDICINE_PILLS_SAN, new ItemMedicineModel(DATA_MEDICINE_PILLS_SAN.NAME, DATA_MEDICINE_PILLS_SAN.PLURAL, GameImages.ITEM_PILLS_SAN, DATA_MEDICINE_PILLS_SAN.HEALING, DATA_MEDICINE_PILLS_SAN.STAMINABOOST, DATA_MEDICINE_PILLS_SAN.SLEEPBOOST, DATA_MEDICINE_PILLS_SAN.INFECTIONCURE, DATA_MEDICINE_PILLS_SAN.SANITYCURE, DATA_MEDICINE_PILLS_SAN.FLAVOR, DATA_MEDICINE_PILLS_SAN.STACKINGLIMIT));
      _setModel(IDs.MEDICINE_PILLS_ANTIVIRAL, new ItemMedicineModel(DATA_MEDICINE_PILLS_ANTIVIRAL.NAME, DATA_MEDICINE_PILLS_ANTIVIRAL.PLURAL, GameImages.ITEM_PILLS_ANTIVIRAL, DATA_MEDICINE_PILLS_ANTIVIRAL.HEALING, DATA_MEDICINE_PILLS_ANTIVIRAL.STAMINABOOST, DATA_MEDICINE_PILLS_ANTIVIRAL.SLEEPBOOST, DATA_MEDICINE_PILLS_ANTIVIRAL.INFECTIONCURE, DATA_MEDICINE_PILLS_ANTIVIRAL.SANITYCURE, DATA_MEDICINE_PILLS_ANTIVIRAL.FLAVOR, DATA_MEDICINE_PILLS_ANTIVIRAL.STACKINGLIMIT));

      // Food
      _setModel(IDs.FOOD_ARMY_RATION, new ItemFoodModel(DATA_FOOD_ARMY_RATION.NAME, DATA_FOOD_ARMY_RATION.PLURAL, GameImages.ITEM_ARMY_RATION, DATA_FOOD_ARMY_RATION.NUTRITION, DATA_FOOD_ARMY_RATION.BESTBEFORE, DATA_FOOD_ARMY_RATION.STACKINGLIMIT, DATA_FOOD_ARMY_RATION.FLAVOR));
      _setModel(IDs.FOOD_GROCERIES, new ItemFoodModel(DATA_FOOD_GROCERIES.NAME, DATA_FOOD_GROCERIES.PLURAL, GameImages.ITEM_GROCERIES, DATA_FOOD_GROCERIES.NUTRITION, DATA_FOOD_GROCERIES.BESTBEFORE, DATA_FOOD_GROCERIES.STACKINGLIMIT, DATA_FOOD_GROCERIES.FLAVOR));
      _setModel(IDs.FOOD_CANNED_FOOD, new ItemFoodModel(DATA_FOOD_CANNED_FOOD.NAME, DATA_FOOD_CANNED_FOOD.PLURAL, GameImages.ITEM_CANNED_FOOD, DATA_FOOD_CANNED_FOOD.NUTRITION, DATA_FOOD_CANNED_FOOD.BESTBEFORE, DATA_FOOD_CANNED_FOOD.STACKINGLIMIT, DATA_FOOD_CANNED_FOOD.FLAVOR));

      // melee weapons
      _setModel(IDs.MELEE_BASEBALLBAT, new ItemMeleeWeaponModel(DATA_MELEE_BASEBALLBAT.NAME, DATA_MELEE_BASEBALLBAT.PLURAL, GameImages.ITEM_BASEBALL_BAT, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_BASEBALLBAT.ATK, DATA_MELEE_BASEBALLBAT.DMG, DATA_MELEE_BASEBALLBAT.STA), DATA_MELEE_BASEBALLBAT.FLAVOR) {
          IsFragile = DATA_MELEE_BASEBALLBAT.ISFRAGILE,
          StackingLimit = DATA_MELEE_BASEBALLBAT.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_COMBAT_KNIFE, new ItemMeleeWeaponModel(DATA_MELEE_COMBAT_KNIFE.NAME, DATA_MELEE_COMBAT_KNIFE.PLURAL, GameImages.ITEM_COMBAT_KNIFE, new Attack(AttackKind.PHYSICAL, STAB, DATA_MELEE_COMBAT_KNIFE.ATK, DATA_MELEE_COMBAT_KNIFE.DMG, DATA_MELEE_COMBAT_KNIFE.STA), DATA_MELEE_COMBAT_KNIFE.FLAVOR) {
          IsFragile = DATA_MELEE_COMBAT_KNIFE.ISFRAGILE,
          StackingLimit = DATA_MELEE_COMBAT_KNIFE.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_CROWBAR, new ItemMeleeWeaponModel(DATA_MELEE_CROWBAR.NAME, DATA_MELEE_CROWBAR.PLURAL, GameImages.ITEM_CROWBAR, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_CROWBAR.ATK, DATA_MELEE_CROWBAR.DMG, DATA_MELEE_CROWBAR.STA), DATA_MELEE_CROWBAR.FLAVOR) {
          IsFragile = DATA_MELEE_CROWBAR.ISFRAGILE,
          StackingLimit = DATA_MELEE_CROWBAR.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_GOLFCLUB, new ItemMeleeWeaponModel(DATA_MELEE_GOLFCLUB.NAME, DATA_MELEE_GOLFCLUB.PLURAL, GameImages.ITEM_GOLF_CLUB, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_GOLFCLUB.ATK, DATA_MELEE_GOLFCLUB.DMG, DATA_MELEE_GOLFCLUB.STA), DATA_MELEE_GOLFCLUB.FLAVOR) {
          IsFragile = DATA_MELEE_GOLFCLUB.ISFRAGILE,
          StackingLimit = DATA_MELEE_GOLFCLUB.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_IRON_GOLFCLUB, new ItemMeleeWeaponModel(DATA_MELEE_IRON_GOLFCLUB.NAME, DATA_MELEE_IRON_GOLFCLUB.PLURAL, GameImages.ITEM_IRON_GOLF_CLUB, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_IRON_GOLFCLUB.ATK, DATA_MELEE_IRON_GOLFCLUB.DMG, DATA_MELEE_IRON_GOLFCLUB.STA), DATA_MELEE_IRON_GOLFCLUB.FLAVOR) {
          IsFragile = DATA_MELEE_IRON_GOLFCLUB.ISFRAGILE,
          StackingLimit = DATA_MELEE_IRON_GOLFCLUB.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_HUGE_HAMMER, new ItemMeleeWeaponModel(DATA_MELEE_HUGE_HAMMER.NAME, DATA_MELEE_HUGE_HAMMER.PLURAL, GameImages.ITEM_HUGE_HAMMER, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_HUGE_HAMMER.ATK, DATA_MELEE_HUGE_HAMMER.DMG, DATA_MELEE_HUGE_HAMMER.STA), DATA_MELEE_HUGE_HAMMER.FLAVOR) {
          IsFragile = DATA_MELEE_HUGE_HAMMER.ISFRAGILE,
          StackingLimit = DATA_MELEE_HUGE_HAMMER.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_SHOVEL, new ItemMeleeWeaponModel(DATA_MELEE_SHOVEL.NAME, DATA_MELEE_SHOVEL.PLURAL, GameImages.ITEM_SHOVEL, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_SHOVEL.ATK, DATA_MELEE_SHOVEL.DMG, DATA_MELEE_SHOVEL.STA), DATA_MELEE_SHOVEL.FLAVOR) {
          IsFragile = DATA_MELEE_SHOVEL.ISFRAGILE,
          StackingLimit = DATA_MELEE_SHOVEL.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_SHORT_SHOVEL, new ItemMeleeWeaponModel(DATA_MELEE_SHORT_SHOVEL.NAME, DATA_MELEE_SHORT_SHOVEL.PLURAL, GameImages.ITEM_SHORT_SHOVEL, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_SHORT_SHOVEL.ATK, DATA_MELEE_SHORT_SHOVEL.DMG, DATA_MELEE_SHORT_SHOVEL.STA), DATA_MELEE_SHORT_SHOVEL.FLAVOR){
          IsFragile = DATA_MELEE_SHORT_SHOVEL.ISFRAGILE,
          StackingLimit = DATA_MELEE_SHORT_SHOVEL.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_TRUNCHEON, new ItemMeleeWeaponModel(DATA_MELEE_TRUNCHEON.NAME, DATA_MELEE_TRUNCHEON.PLURAL, GameImages.ITEM_TRUNCHEON, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_TRUNCHEON.ATK, DATA_MELEE_TRUNCHEON.DMG, DATA_MELEE_TRUNCHEON.STA), DATA_MELEE_TRUNCHEON.FLAVOR) {
          IsFragile = DATA_MELEE_TRUNCHEON.ISFRAGILE,
          StackingLimit = DATA_MELEE_TRUNCHEON.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_IMPROVISED_CLUB, new ItemMeleeWeaponModel(DATA_MELEE_IMPROVISED_CLUB.NAME, DATA_MELEE_IMPROVISED_CLUB.PLURAL, GameImages.ITEM_IMPROVISED_CLUB, new Attack(AttackKind.PHYSICAL, STRIKE, DATA_MELEE_IMPROVISED_CLUB.ATK, DATA_MELEE_IMPROVISED_CLUB.DMG, DATA_MELEE_IMPROVISED_CLUB.STA), DATA_MELEE_IMPROVISED_CLUB.FLAVOR) {
          IsFragile = DATA_MELEE_IMPROVISED_CLUB.ISFRAGILE,
          StackingLimit = DATA_MELEE_IMPROVISED_CLUB.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_IMPROVISED_SPEAR, new ItemMeleeWeaponModel(DATA_MELEE_IMPROVISED_SPEAR.NAME, DATA_MELEE_IMPROVISED_SPEAR.PLURAL, GameImages.ITEM_IMPROVISED_SPEAR, new Attack(AttackKind.PHYSICAL, PIERCE, DATA_MELEE_IMPROVISED_SPEAR.ATK, DATA_MELEE_IMPROVISED_SPEAR.DMG, DATA_MELEE_IMPROVISED_SPEAR.STA), DATA_MELEE_IMPROVISED_SPEAR.FLAVOR) {
          IsFragile = DATA_MELEE_IMPROVISED_SPEAR.ISFRAGILE,
          StackingLimit = DATA_MELEE_IMPROVISED_SPEAR.STACKINGLIMIT
      });
      _setModel(IDs.MELEE_SMALL_HAMMER, new ItemMeleeWeaponModel(DATA_MELEE_SMALL_HAMMER.NAME, DATA_MELEE_SMALL_HAMMER.PLURAL, GameImages.ITEM_SMALL_HAMMER, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_SMALL_HAMMER.ATK, DATA_MELEE_SMALL_HAMMER.DMG, DATA_MELEE_SMALL_HAMMER.STA), DATA_MELEE_SMALL_HAMMER.FLAVOR) {
          IsFragile = DATA_MELEE_SMALL_HAMMER.ISFRAGILE,
          StackingLimit = DATA_MELEE_SMALL_HAMMER.STACKINGLIMIT
      });
      _setModel(IDs.UNIQUE_JASON_MYERS_AXE, new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_JASON_MYERS_AXE.NAME, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.PLURAL, GameImages.ITEM_JASON_MYERS_AXE, new Attack(AttackKind.PHYSICAL, SLASH, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.ATK, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.DMG, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.STA), DATA_MELEE_UNIQUE_JASON_MYERS_AXE.FLAVOR) {
          IsProper = true,
          IsUnbreakable = true,
          IsUnique = true
      });
      _setModel(IDs.UNIQUE_FAMU_FATARU_KATANA, new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.NAME, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.PLURAL, GameImages.ITEM_FAMU_FATARU_KATANA, new Attack(AttackKind.PHYSICAL, SLASH, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.ATK, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.DMG, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.STA), DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.FLAVOR) {
          IsProper = true,
          IsUnbreakable = true,
          IsUnique = true
      });
      _setModel(IDs.UNIQUE_BIGBEAR_BAT, new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_BIGBEAR_BAT.NAME, DATA_MELEE_UNIQUE_BIGBEAR_BAT.PLURAL, GameImages.ITEM_BIGBEAR_BAT, new Attack(AttackKind.PHYSICAL, SMASH, DATA_MELEE_UNIQUE_BIGBEAR_BAT.ATK, DATA_MELEE_UNIQUE_BIGBEAR_BAT.DMG, DATA_MELEE_UNIQUE_BIGBEAR_BAT.STA), DATA_MELEE_UNIQUE_BIGBEAR_BAT.FLAVOR) {
          IsProper = true,
          IsUnbreakable = true,
          IsUnique = true
      });
      _setModel(IDs.UNIQUE_ROGUEDJACK_KEYBOARD, new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.NAME, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.PLURAL, GameImages.ITEM_ROGUEDJACK_KEYBOARD, new Attack(AttackKind.PHYSICAL, RogueGame.VERB_BASH, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.ATK, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.DMG, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.STA), DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.FLAVOR) {
          IsProper = true,
          IsUnbreakable = true,
          IsUnique = true
      });

      // ranged weapons
      _setModel(IDs.RANGED_ARMY_PISTOL, new ItemRangedWeaponModel(DATA_RANGED_ARMY_PISTOL.NAME, DATA_RANGED_ARMY_PISTOL.FLAVOR, GameImages.ITEM_ARMY_PISTOL, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_ARMY_PISTOL.ATK, DATA_RANGED_ARMY_PISTOL.DMG, 0, DATA_RANGED_ARMY_PISTOL.RANGE), DATA_RANGED_ARMY_PISTOL.MAXAMMO, AmmoType.HEAVY_PISTOL, DATA_RANGED_ARMY_PISTOL.FLAVOR));
      _setModel(IDs.RANGED_ARMY_RIFLE, new ItemRangedWeaponModel(DATA_RANGED_ARMY_RIFLE.NAME, DATA_RANGED_ARMY_RIFLE.FLAVOR, GameImages.ITEM_ARMY_RIFLE, new Attack(AttackKind.FIREARM, FIRE_SALVO_AT, DATA_RANGED_ARMY_RIFLE.ATK, DATA_RANGED_ARMY_RIFLE.DMG, 0, DATA_RANGED_ARMY_RIFLE.RANGE), DATA_RANGED_ARMY_RIFLE.MAXAMMO, AmmoType.HEAVY_RIFLE, DATA_RANGED_ARMY_RIFLE.FLAVOR));
      _setModel(IDs.RANGED_HUNTING_CROSSBOW, new ItemRangedWeaponModel(DATA_RANGED_HUNTING_CROSSBOW.NAME, DATA_RANGED_HUNTING_CROSSBOW.FLAVOR, GameImages.ITEM_HUNTING_CROSSBOW, new Attack(AttackKind.BOW, SHOOT, DATA_RANGED_HUNTING_CROSSBOW.ATK, DATA_RANGED_HUNTING_CROSSBOW.DMG, 0, DATA_RANGED_HUNTING_CROSSBOW.RANGE), DATA_RANGED_HUNTING_CROSSBOW.MAXAMMO, AmmoType.BOLT, DATA_RANGED_HUNTING_CROSSBOW.FLAVOR));
      _setModel(IDs.RANGED_HUNTING_RIFLE, new ItemRangedWeaponModel(DATA_RANGED_HUNTING_RIFLE.NAME, DATA_RANGED_HUNTING_RIFLE.FLAVOR, GameImages.ITEM_HUNTING_RIFLE, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_HUNTING_RIFLE.ATK, DATA_RANGED_HUNTING_RIFLE.DMG, 0, DATA_RANGED_HUNTING_RIFLE.RANGE), DATA_RANGED_HUNTING_RIFLE.MAXAMMO, AmmoType.LIGHT_RIFLE, DATA_RANGED_HUNTING_RIFLE.FLAVOR));
      _setModel(IDs.RANGED_PISTOL, new ItemRangedWeaponModel(DATA_RANGED_PISTOL.NAME, DATA_RANGED_PISTOL.FLAVOR, GameImages.ITEM_PISTOL, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_PISTOL.ATK, DATA_RANGED_PISTOL.DMG, 0, DATA_RANGED_PISTOL.RANGE), DATA_RANGED_PISTOL.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_RANGED_PISTOL.FLAVOR));
      _setModel(IDs.RANGED_KOLT_REVOLVER, new ItemRangedWeaponModel(DATA_RANGED_KOLT_REVOLVER.NAME, DATA_RANGED_KOLT_REVOLVER.FLAVOR, GameImages.ITEM_KOLT_REVOLVER, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_KOLT_REVOLVER.ATK, DATA_RANGED_KOLT_REVOLVER.DMG, 0, DATA_RANGED_KOLT_REVOLVER.RANGE), DATA_RANGED_KOLT_REVOLVER.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_RANGED_KOLT_REVOLVER.FLAVOR));
      _setModel(IDs.RANGED_PRECISION_RIFLE, new ItemRangedWeaponModel(DATA_RANGED_PRECISION_RIFLE.NAME, DATA_RANGED_PRECISION_RIFLE.FLAVOR, GameImages.ITEM_PRECISION_RIFLE, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_PRECISION_RIFLE.ATK, DATA_RANGED_PRECISION_RIFLE.DMG, 0, DATA_RANGED_PRECISION_RIFLE.RANGE), DATA_RANGED_PRECISION_RIFLE.MAXAMMO, AmmoType.HEAVY_RIFLE, DATA_RANGED_PRECISION_RIFLE.FLAVOR));
      _setModel(IDs.RANGED_SHOTGUN, new ItemRangedWeaponModel(DATA_RANGED_SHOTGUN.NAME, DATA_RANGED_SHOTGUN.FLAVOR, GameImages.ITEM_SHOTGUN, new Attack(AttackKind.FIREARM, SHOOT, DATA_RANGED_SHOTGUN.ATK, DATA_RANGED_SHOTGUN.DMG, 0, DATA_RANGED_SHOTGUN.RANGE), DATA_RANGED_SHOTGUN.MAXAMMO, AmmoType.SHOTGUN, DATA_RANGED_SHOTGUN.FLAVOR));
      _setModel(IDs.UNIQUE_SANTAMAN_SHOTGUN, new ItemRangedWeaponModel(DATA_UNIQUE_SANTAMAN_SHOTGUN.NAME, DATA_UNIQUE_SANTAMAN_SHOTGUN.FLAVOR, GameImages.ITEM_SANTAMAN_SHOTGUN, new Attack(AttackKind.FIREARM, SHOOT, DATA_UNIQUE_SANTAMAN_SHOTGUN.ATK, DATA_UNIQUE_SANTAMAN_SHOTGUN.DMG, 0, DATA_UNIQUE_SANTAMAN_SHOTGUN.RANGE), DATA_UNIQUE_SANTAMAN_SHOTGUN.MAXAMMO, AmmoType.SHOTGUN, DATA_UNIQUE_SANTAMAN_SHOTGUN.FLAVOR) {
          IsProper = true,
          IsUnbreakable = true,
          IsUnique = true
      });
      _setModel(IDs.UNIQUE_HANS_VON_HANZ_PISTOL, new ItemRangedWeaponModel(DATA_UNIQUE_HANS_VON_HANZ_PISTOL.NAME, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.FLAVOR, GameImages.ITEM_HANS_VON_HANZ_PISTOL, new Attack(AttackKind.FIREARM, SHOOT, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.ATK, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.DMG, 0, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.RANGE), DATA_UNIQUE_HANS_VON_HANZ_PISTOL.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.FLAVOR) {
          IsProper = true,
          IsUnbreakable = true,
          IsUnique = true
      });

      // Ammunition
      _setModel(IDs.AMMO_LIGHT_PISTOL, new ItemAmmoModel(GameImages.ITEM_AMMO_LIGHT_PISTOL, AmmoType.LIGHT_PISTOL, 20));
      _setModel(IDs.AMMO_HEAVY_PISTOL, new ItemAmmoModel(GameImages.ITEM_AMMO_HEAVY_PISTOL, AmmoType.HEAVY_PISTOL, 12));
      _setModel(IDs.AMMO_LIGHT_RIFLE, new ItemAmmoModel(GameImages.ITEM_AMMO_LIGHT_RIFLE, AmmoType.LIGHT_RIFLE, 14));
      _setModel(IDs.AMMO_HEAVY_RIFLE, new ItemAmmoModel(GameImages.ITEM_AMMO_HEAVY_RIFLE, AmmoType.HEAVY_RIFLE, 20));
      _setModel(IDs.AMMO_SHOTGUN, new ItemAmmoModel(GameImages.ITEM_AMMO_SHOTGUN, AmmoType.SHOTGUN, 10));
      _setModel(IDs.AMMO_BOLTS, new ItemAmmoModel(GameImages.ITEM_AMMO_BOLTS, AmmoType.BOLT, 30));

      // grenade, in its various states
      int[] damage = new int[DATA_EXPLOSIVE_GRENADE.RADIUS + 1];
      for (int index = 0; index < DATA_EXPLOSIVE_GRENADE.RADIUS + 1; ++index)
        damage[index] = DATA_EXPLOSIVE_GRENADE.DMG[index];   // XXX explosiveData.DMG is returned with a mismatched length

      _setModel(IDs.EXPLOSIVE_GRENADE, new ItemGrenadeModel(DATA_EXPLOSIVE_GRENADE.NAME, DATA_EXPLOSIVE_GRENADE.PLURAL, GameImages.ITEM_GRENADE, DATA_EXPLOSIVE_GRENADE.FUSE, new BlastAttack(DATA_EXPLOSIVE_GRENADE.RADIUS, damage, true, false), GameImages.ICON_BLAST, DATA_EXPLOSIVE_GRENADE.MAXTHROW, DATA_EXPLOSIVE_GRENADE.STACKLINGLIMIT, DATA_EXPLOSIVE_GRENADE.FLAVOR));
      _setModel(IDs.EXPLOSIVE_GRENADE_PRIMED, new ItemGrenadePrimedModel("primed " + DATA_EXPLOSIVE_GRENADE.NAME, "primed " + DATA_EXPLOSIVE_GRENADE.PLURAL, GameImages.ITEM_GRENADE_PRIMED, this[IDs.EXPLOSIVE_GRENADE] as ItemGrenadeModel));

      // carpentry
      GameItems.BarricadingMaterialData barricadingMaterialData = DATA_BAR_WOODEN_PLANK;
      ItemBarricadeMaterialModel barricadeMaterialModel1 = new ItemBarricadeMaterialModel(barricadingMaterialData.NAME, barricadingMaterialData.PLURAL, GameImages.ITEM_WOODEN_PLANK, barricadingMaterialData.VALUE);
      barricadeMaterialModel1.StackingLimit = barricadingMaterialData.STACKINGLIMIT;
      barricadeMaterialModel1.FlavorDescription = barricadingMaterialData.FLAVOR;
      _setModel(IDs.BAR_WOODEN_PLANK, barricadeMaterialModel1);

      // body armor
      _setModel(IDs.ARMOR_ARMY_BODYARMOR, new ItemBodyArmorModel(DATA_ARMOR_ARMY.NAME, DATA_ARMOR_ARMY.PLURAL, GameImages.ITEM_ARMY_BODYARMOR, DATA_ARMOR_ARMY.PRO_HIT, DATA_ARMOR_ARMY.PRO_SHOT, DATA_ARMOR_ARMY.ENC, DATA_ARMOR_ARMY.WEIGHT, DATA_ARMOR_ARMY.FLAVOR));
      _setModel(IDs.ARMOR_CHAR_LIGHT_BODYARMOR, new ItemBodyArmorModel(DATA_ARMOR_CHAR.NAME, DATA_ARMOR_CHAR.PLURAL, GameImages.ITEM_CHAR_LIGHT_BODYARMOR, DATA_ARMOR_CHAR.PRO_HIT, DATA_ARMOR_CHAR.PRO_SHOT, DATA_ARMOR_CHAR.ENC, DATA_ARMOR_CHAR.WEIGHT, DATA_ARMOR_CHAR.FLAVOR));
      _setModel(IDs.ARMOR_HELLS_SOULS_JACKET, new ItemBodyArmorModel(DATA_ARMOR_HELLS_SOULS_JACKET.NAME, DATA_ARMOR_HELLS_SOULS_JACKET.PLURAL, GameImages.ITEM_HELLS_SOULS_JACKET, DATA_ARMOR_HELLS_SOULS_JACKET.PRO_HIT, DATA_ARMOR_HELLS_SOULS_JACKET.PRO_SHOT, DATA_ARMOR_HELLS_SOULS_JACKET.ENC, DATA_ARMOR_HELLS_SOULS_JACKET.WEIGHT, DATA_ARMOR_HELLS_SOULS_JACKET.FLAVOR));
      _setModel(IDs.ARMOR_FREE_ANGELS_JACKET, new ItemBodyArmorModel(DATA_ARMOR_FREE_ANGELS_JACKET.NAME, DATA_ARMOR_FREE_ANGELS_JACKET.PLURAL, GameImages.ITEM_FREE_ANGELS_JACKET, DATA_ARMOR_FREE_ANGELS_JACKET.PRO_HIT, DATA_ARMOR_FREE_ANGELS_JACKET.PRO_SHOT, DATA_ARMOR_FREE_ANGELS_JACKET.ENC, DATA_ARMOR_FREE_ANGELS_JACKET.WEIGHT, DATA_ARMOR_FREE_ANGELS_JACKET.FLAVOR));
      _setModel(IDs.ARMOR_POLICE_JACKET, new ItemBodyArmorModel(DATA_ARMOR_POLICE_JACKET.NAME, DATA_ARMOR_POLICE_JACKET.PLURAL, GameImages.ITEM_POLICE_JACKET, DATA_ARMOR_POLICE_JACKET.PRO_HIT, DATA_ARMOR_POLICE_JACKET.PRO_SHOT, DATA_ARMOR_POLICE_JACKET.ENC, DATA_ARMOR_POLICE_JACKET.WEIGHT, DATA_ARMOR_POLICE_JACKET.FLAVOR));
      _setModel(IDs.ARMOR_POLICE_RIOT, new ItemBodyArmorModel(DATA_ARMOR_POLICE_RIOT.NAME, DATA_ARMOR_POLICE_RIOT.PLURAL, GameImages.ITEM_POLICE_RIOT_ARMOR, DATA_ARMOR_POLICE_RIOT.PRO_HIT, DATA_ARMOR_POLICE_RIOT.PRO_SHOT, DATA_ARMOR_POLICE_RIOT.ENC, DATA_ARMOR_POLICE_RIOT.WEIGHT, DATA_ARMOR_POLICE_RIOT.FLAVOR));
      _setModel(IDs.ARMOR_HUNTER_VEST, new ItemBodyArmorModel(DATA_ARMOR_HUNTER_VEST.NAME, DATA_ARMOR_HUNTER_VEST.PLURAL, GameImages.ITEM_HUNTER_VEST, DATA_ARMOR_HUNTER_VEST.PRO_HIT, DATA_ARMOR_HUNTER_VEST.PRO_SHOT, DATA_ARMOR_HUNTER_VEST.ENC, DATA_ARMOR_HUNTER_VEST.WEIGHT, DATA_ARMOR_HUNTER_VEST.FLAVOR));

      // trackers
      _setModel(IDs.TRACKER_CELL_PHONE, new ItemTrackerModel(DATA_TRACKER_CELL_PHONE.NAME, DATA_TRACKER_CELL_PHONE.PLURAL, GameImages.ITEM_CELL_PHONE, ItemTrackerModel.TrackingFlags.FOLLOWER_AND_LEADER, DATA_TRACKER_CELL_PHONE.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.LEFT_HAND, DATA_TRACKER_CELL_PHONE.FLAVOR));
      _setModel(IDs.TRACKER_ZTRACKER, new ItemTrackerModel(DATA_TRACKER_ZTRACKER.NAME, DATA_TRACKER_ZTRACKER.PLURAL, GameImages.ITEM_ZTRACKER, ItemTrackerModel.TrackingFlags.UNDEADS, DATA_TRACKER_ZTRACKER.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.LEFT_HAND, DATA_TRACKER_ZTRACKER.FLAVOR));
      _setModel(IDs.TRACKER_BLACKOPS, new ItemTrackerModel(DATA_TRACKER_BLACKOPS_GPS.NAME, DATA_TRACKER_BLACKOPS_GPS.PLURAL, GameImages.ITEM_BLACKOPS_GPS, ItemTrackerModel.TrackingFlags.BLACKOPS_FACTION, DATA_TRACKER_BLACKOPS_GPS.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.LEFT_HAND, DATA_TRACKER_BLACKOPS_GPS.FLAVOR));
      _setModel(IDs.TRACKER_POLICE_RADIO, new ItemTrackerModel(DATA_TRACKER_POLICE_RADIO.NAME, DATA_TRACKER_POLICE_RADIO.PLURAL, GameImages.ITEM_POLICE_RADIO, ItemTrackerModel.TrackingFlags.POLICE_FACTION, DATA_TRACKER_POLICE_RADIO.BATTERIES * WorldTime.TURNS_PER_HOUR, DollPart.HIP_HOLSTER, DATA_TRACKER_POLICE_RADIO.FLAVOR));

      // spray paint
      _setModel(IDs.SPRAY_PAINT1, new ItemSprayPaintModel(DATA_SPRAY_PAINT1.NAME, DATA_SPRAY_PAINT1.PLURAL, GameImages.ITEM_SPRAYPAINT, DATA_SPRAY_PAINT1.QUANTITY, GameImages.DECO_PLAYER_TAG1, DATA_SPRAY_PAINT1.FLAVOR));
      _setModel(IDs.SPRAY_PAINT2, new ItemSprayPaintModel(DATA_SPRAY_PAINT2.NAME, DATA_SPRAY_PAINT2.PLURAL, GameImages.ITEM_SPRAYPAINT2, DATA_SPRAY_PAINT2.QUANTITY, GameImages.DECO_PLAYER_TAG2, DATA_SPRAY_PAINT2.FLAVOR));
      _setModel(IDs.SPRAY_PAINT3, new ItemSprayPaintModel(DATA_SPRAY_PAINT3.NAME, DATA_SPRAY_PAINT3.PLURAL, GameImages.ITEM_SPRAYPAINT3, DATA_SPRAY_PAINT3.QUANTITY, GameImages.DECO_PLAYER_TAG3, DATA_SPRAY_PAINT3.FLAVOR));
      _setModel(IDs.SPRAY_PAINT4, new ItemSprayPaintModel(DATA_SPRAY_PAINT4.NAME, DATA_SPRAY_PAINT4.PLURAL, GameImages.ITEM_SPRAYPAINT4, DATA_SPRAY_PAINT4.QUANTITY, GameImages.DECO_PLAYER_TAG4, DATA_SPRAY_PAINT4.FLAVOR));

      // Flashlights
      _setModel(IDs.LIGHT_FLASHLIGHT, new ItemLightModel(DATA_LIGHT_FLASHLIGHT.NAME, DATA_LIGHT_FLASHLIGHT.PLURAL, GameImages.ITEM_FLASHLIGHT, DATA_LIGHT_FLASHLIGHT.FOV, DATA_LIGHT_FLASHLIGHT.BATTERIES * WorldTime.TURNS_PER_HOUR, GameImages.ITEM_FLASHLIGHT_OUT, DATA_LIGHT_FLASHLIGHT.FLAVOR));
      _setModel(IDs.LIGHT_BIG_FLASHLIGHT, new ItemLightModel(DATA_LIGHT_BIG_FLASHLIGHT.NAME, DATA_LIGHT_BIG_FLASHLIGHT.PLURAL, GameImages.ITEM_BIG_FLASHLIGHT, DATA_LIGHT_BIG_FLASHLIGHT.FOV, DATA_LIGHT_BIG_FLASHLIGHT.BATTERIES * WorldTime.TURNS_PER_HOUR, GameImages.ITEM_BIG_FLASHLIGHT_OUT, DATA_LIGHT_BIG_FLASHLIGHT.FLAVOR));

      // stench killer
      _setModel(IDs.SCENT_SPRAY_STENCH_KILLER, new ItemSprayScentModel(DATA_SCENT_SPRAY_STENCH_KILLER.NAME, DATA_SCENT_SPRAY_STENCH_KILLER.PLURAL, GameImages.ITEM_STENCH_KILLER, DATA_SCENT_SPRAY_STENCH_KILLER.QUANTITY, Odor.PERFUME_LIVING_SUPRESSOR, DATA_SCENT_SPRAY_STENCH_KILLER.STRENGTH * 30, DATA_SCENT_SPRAY_STENCH_KILLER.FLAVOR));

      // Traps
      _setModel(IDs.TRAP_EMPTY_CAN, new ItemTrapModel(DATA_TRAP_EMPTY_CAN.NAME, DATA_TRAP_EMPTY_CAN.PLURAL, GameImages.ITEM_EMPTY_CAN, DATA_TRAP_EMPTY_CAN.STACKING, DATA_TRAP_EMPTY_CAN.CHANCE, DATA_TRAP_EMPTY_CAN.DAMAGE, DATA_TRAP_EMPTY_CAN.DROP_ACTIVATE, DATA_TRAP_EMPTY_CAN.USE_ACTIVATE, DATA_TRAP_EMPTY_CAN.IS_ONE_TIME, DATA_TRAP_EMPTY_CAN.BREAK_CHANCE, DATA_TRAP_EMPTY_CAN.BLOCK_CHANCE, DATA_TRAP_EMPTY_CAN.BREAK_CHANCE_ESCAPE, DATA_TRAP_EMPTY_CAN.IS_NOISY, DATA_TRAP_EMPTY_CAN.NOISE_NAME, DATA_TRAP_EMPTY_CAN.IS_FLAMMABLE, DATA_TRAP_EMPTY_CAN.FLAVOR));
      _setModel(IDs.TRAP_BEAR_TRAP, new ItemTrapModel(DATA_TRAP_BEAR_TRAP.NAME, DATA_TRAP_BEAR_TRAP.PLURAL, GameImages.ITEM_BEAR_TRAP, DATA_TRAP_BEAR_TRAP.STACKING, DATA_TRAP_BEAR_TRAP.CHANCE, DATA_TRAP_BEAR_TRAP.DAMAGE, DATA_TRAP_BEAR_TRAP.DROP_ACTIVATE, DATA_TRAP_BEAR_TRAP.USE_ACTIVATE, DATA_TRAP_BEAR_TRAP.IS_ONE_TIME, DATA_TRAP_BEAR_TRAP.BREAK_CHANCE, DATA_TRAP_BEAR_TRAP.BLOCK_CHANCE, DATA_TRAP_BEAR_TRAP.BREAK_CHANCE_ESCAPE, DATA_TRAP_BEAR_TRAP.IS_NOISY, DATA_TRAP_BEAR_TRAP.NOISE_NAME, DATA_TRAP_BEAR_TRAP.IS_FLAMMABLE, DATA_TRAP_BEAR_TRAP.FLAVOR));
      _setModel(IDs.TRAP_SPIKES, new ItemTrapModel(DATA_TRAP_SPIKES.NAME, DATA_TRAP_SPIKES.PLURAL, GameImages.ITEM_SPIKES, DATA_TRAP_SPIKES.STACKING, DATA_TRAP_SPIKES.CHANCE, DATA_TRAP_SPIKES.DAMAGE, DATA_TRAP_SPIKES.DROP_ACTIVATE, DATA_TRAP_SPIKES.USE_ACTIVATE, DATA_TRAP_SPIKES.IS_ONE_TIME, DATA_TRAP_SPIKES.BREAK_CHANCE, DATA_TRAP_SPIKES.BLOCK_CHANCE, DATA_TRAP_SPIKES.BREAK_CHANCE_ESCAPE, DATA_TRAP_SPIKES.IS_NOISY, DATA_TRAP_SPIKES.NOISE_NAME, DATA_TRAP_SPIKES.IS_FLAMMABLE, DATA_TRAP_SPIKES.FLAVOR));
      _setModel(IDs.TRAP_BARBED_WIRE, new ItemTrapModel(DATA_TRAP_BARBED_WIRE.NAME, DATA_TRAP_BARBED_WIRE.PLURAL, GameImages.ITEM_BARBED_WIRE, DATA_TRAP_BARBED_WIRE.STACKING, DATA_TRAP_BARBED_WIRE.CHANCE, DATA_TRAP_BARBED_WIRE.DAMAGE, DATA_TRAP_BARBED_WIRE.DROP_ACTIVATE, DATA_TRAP_BARBED_WIRE.USE_ACTIVATE, DATA_TRAP_BARBED_WIRE.IS_ONE_TIME, DATA_TRAP_BARBED_WIRE.BREAK_CHANCE, DATA_TRAP_BARBED_WIRE.BLOCK_CHANCE, DATA_TRAP_BARBED_WIRE.BREAK_CHANCE_ESCAPE, DATA_TRAP_BARBED_WIRE.IS_NOISY, DATA_TRAP_BARBED_WIRE.NOISE_NAME, DATA_TRAP_BARBED_WIRE.IS_FLAMMABLE, DATA_TRAP_BARBED_WIRE.FLAVOR));

      // entertainment
      _setModel(IDs.ENT_BOOK, new ItemEntertainmentModel(DATA_ENT_BOOK.NAME, DATA_ENT_BOOK.PLURAL, GameImages.ITEM_BOOK, DATA_ENT_BOOK.VALUE, DATA_ENT_BOOK.BORECHANCE, DATA_ENT_BOOK.STACKING, DATA_ENT_BOOK.FLAVOR));
      _setModel(IDs.ENT_MAGAZINE, new ItemEntertainmentModel(DATA_ENT_MAGAZINE.NAME, DATA_ENT_MAGAZINE.PLURAL, GameImages.ITEM_MAGAZINE, DATA_ENT_MAGAZINE.VALUE, DATA_ENT_MAGAZINE.BORECHANCE, DATA_ENT_MAGAZINE.STACKING, DATA_ENT_MAGAZINE.FLAVOR));

      _setModel(IDs.UNIQUE_SUBWAY_BADGE, new ItemModel("Subway Worker Badge", "Subways Worker Badges", GameImages.ITEM_SUBWAY_BADGE)
      {
        DontAutoEquip = true,
        EquipmentPart = DollPart.LEFT_HAND,
        FlavorDescription = "You got yourself a new job!",
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

    private static bool LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, GameItems.IDs[] idsToRead, out _DATA_TYPE_[] data)
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
      return true;
    }

    private bool LoadMedicineFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "medicine items", MedecineData.COUNT_FIELDS, new Func<CSVLine, MedecineData>(MedecineData.FromCSVLine), new IDs[6]
      {
        IDs._FIRST,
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
      return true;
    }

    private bool LoadFoodFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "food items", FoodData.COUNT_FIELDS, new Func<CSVLine, FoodData>(FoodData.FromCSVLine), new IDs[3]
      {
        IDs.FOOD_ARMY_RATION,
        IDs.FOOD_CANNED_FOOD,
        IDs.FOOD_GROCERIES
      }, out FoodData[] data);
      DATA_FOOD_ARMY_RATION = data[0];
      DATA_FOOD_CANNED_FOOD = data[1];
      DATA_FOOD_GROCERIES = data[2];
      return true;
    }

    private bool LoadMeleeWeaponsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "melee weapons items", MeleeWeaponData.COUNT_FIELDS, new Func<CSVLine, MeleeWeaponData>(MeleeWeaponData.FromCSVLine), new IDs[16]
      {
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
        IDs.UNIQUE_ROGUEDJACK_KEYBOARD
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
      return true;
    }

    private bool LoadRangedWeaponsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "ranged weapons items", RangedWeaponData.COUNT_FIELDS, new Func<CSVLine, RangedWeaponData>(RangedWeaponData.FromCSVLine), new IDs[10]
      {
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
      return true;
    }

    private bool LoadExplosivesFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "explosives items", ExplosiveData.COUNT_FIELDS, new Func<CSVLine, ExplosiveData>(ExplosiveData.FromCSVLine), new IDs[1]
      {
        IDs.EXPLOSIVE_GRENADE
      }, out ExplosiveData[] data);
      DATA_EXPLOSIVE_GRENADE = data[0];
      return true;
    }

    private bool LoadBarricadingMaterialFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "barricading items", BarricadingMaterialData.COUNT_FIELDS, new Func<CSVLine, BarricadingMaterialData>(BarricadingMaterialData.FromCSVLine), new IDs[1]
      {
        IDs.BAR_WOODEN_PLANK
      }, out BarricadingMaterialData[] data);
      DATA_BAR_WOODEN_PLANK = data[0];
      return true;
    }

    private bool LoadArmorsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "armors items", ArmorData.COUNT_FIELDS, new Func<CSVLine, ArmorData>(ArmorData.FromCSVLine), new IDs[7]
      {
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
      return true;
    }

    private bool LoadTrackersFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "trackers items", TrackerData.COUNT_FIELDS, new Func<CSVLine, TrackerData>(TrackerData.FromCSVLine), new IDs[4]
      {
        IDs.TRACKER_BLACKOPS,
        IDs.TRACKER_CELL_PHONE,
        IDs.TRACKER_ZTRACKER,
        IDs.TRACKER_POLICE_RADIO
      }, out TrackerData[] data);
      DATA_TRACKER_BLACKOPS_GPS = data[0];
      DATA_TRACKER_CELL_PHONE = data[1];
      DATA_TRACKER_ZTRACKER = data[2];
      DATA_TRACKER_POLICE_RADIO = data[3];
      return true;
    }

    private bool LoadSpraypaintsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "spraypaint items", SprayPaintData.COUNT_FIELDS, new Func<CSVLine, SprayPaintData>(SprayPaintData.FromCSVLine), new IDs[4]
      {
        IDs.SPRAY_PAINT1,
        IDs.SPRAY_PAINT2,
        IDs.SPRAY_PAINT3,
        IDs.SPRAY_PAINT4
      }, out SprayPaintData[] data);
      DATA_SPRAY_PAINT1 = data[0];
      DATA_SPRAY_PAINT2 = data[1];
      DATA_SPRAY_PAINT3 = data[2];
      DATA_SPRAY_PAINT4 = data[3];
      return true;
    }

    private bool LoadLightsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "lights items", LightData.COUNT_FIELDS, new Func<CSVLine, LightData>(LightData.FromCSVLine), new IDs[2]
      {
        IDs.LIGHT_FLASHLIGHT,
        IDs.LIGHT_BIG_FLASHLIGHT
      }, out LightData[] data);
      DATA_LIGHT_FLASHLIGHT = data[0];
      DATA_LIGHT_BIG_FLASHLIGHT = data[1];
      return true;
    }

    private bool LoadScentspraysFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "scentsprays items", ScentSprayData.COUNT_FIELDS, new Func<CSVLine, ScentSprayData>(ScentSprayData.FromCSVLine), new IDs[1]
      {
        IDs.SCENT_SPRAY_STENCH_KILLER
      }, out ScentSprayData[] data);
      DATA_SCENT_SPRAY_STENCH_KILLER = data[0];
      return true;
    }

    private bool LoadTrapsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "traps items", TrapData.COUNT_FIELDS, new Func<CSVLine, TrapData>(TrapData.FromCSVLine), new IDs[4]
      {
        IDs.TRAP_EMPTY_CAN,
        IDs.TRAP_BEAR_TRAP,
        IDs.TRAP_SPIKES,
        IDs.TRAP_BARBED_WIRE
      }, out TrapData[] data);
      DATA_TRAP_EMPTY_CAN = data[0];
      DATA_TRAP_BEAR_TRAP = data[1];
      DATA_TRAP_SPIKES = data[2];
      DATA_TRAP_BARBED_WIRE = data[3];
      return true;
    }

    private bool LoadEntertainmentFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      LoadDataFromCSV(ui, path, "entertainment items", EntData.COUNT_FIELDS, new Func<CSVLine, EntData>(EntData.FromCSVLine), new IDs[2]
      {
        IDs.ENT_BOOK,
        IDs.ENT_MAGAZINE
      }, out EntData[] data);
      DATA_ENT_BOOK = data[0];
      DATA_ENT_MAGAZINE = data[1];
      return true;
    }

    public enum IDs
    {
      _FIRST = 0,
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
      ARMOR_ARMY_BODYARMOR = 33,
      ARMOR_CHAR_LIGHT_BODYARMOR = 34,
      ARMOR_HELLS_SOULS_JACKET = 35,
      ARMOR_FREE_ANGELS_JACKET = 36,
      ARMOR_POLICE_JACKET = 37,
      ARMOR_POLICE_RIOT = 38,
      ARMOR_HUNTER_VEST = 39,
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
      _COUNT = 69,
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
      public const int COUNT_FIELDS = 9;

      public string NAME;
      public string PLURAL;
      public int ATK;
      public int DMG;
      public int STA;
      public int STACKINGLIMIT;
      public bool ISFRAGILE;
      public string FLAVOR;

      public static MeleeWeaponData FromCSVLine(CSVLine line)
      {
        return new MeleeWeaponData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          ATK = line[3].ParseInt(),
          DMG = line[4].ParseInt(),
          STA = line[5].ParseInt(),
          STACKINGLIMIT = line[6].ParseInt(),
          ISFRAGILE = line[7].ParseBool(),
          FLAVOR = line[8].ParseText()
        };
      }
    }

    private struct RangedWeaponData
    {
      public const int COUNT_FIELDS = 8;

      public string NAME;
      public string PLURAL;
      public int ATK;
      public int DMG;
      public int RANGE;
      public int MAXAMMO;
      public string FLAVOR;

      public static RangedWeaponData FromCSVLine(CSVLine line)
      {
        return new RangedWeaponData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          ATK = line[3].ParseInt(),
          DMG = line[4].ParseInt(),
          RANGE = line[5].ParseInt(),
          MAXAMMO = line[6].ParseInt(),
          FLAVOR = line[7].ParseText()
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
      public int FOV;
      public int BATTERIES;
      public string FLAVOR;

      public static LightData FromCSVLine(CSVLine line)
      {
        return new LightData {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          FOV = line[3].ParseInt(),
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
