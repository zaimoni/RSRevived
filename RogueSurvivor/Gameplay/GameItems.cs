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
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameItems : ItemModelDB
  {
    private readonly ItemModel[] m_Models = new ItemModel[(int) GameItems.IDs._COUNT];
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

    public override ItemModel this[int id] {
      get {
        return m_Models[id];
      }
    }

    public ItemModel this[GameItems.IDs id] {
      get {
        return this[(int) id];
      }
      private set {
        Contract.Requires(null!=value);
        Contract.Requires((int)id < m_Models.Length);
        m_Models[(int) id] = value;
        m_Models[(int) id].ID = id;
      }
    }

    public ItemMedicineModel BANDAGE
    {
      get
      {
        return this[GameItems.IDs._FIRST] as ItemMedicineModel;
      }
    }

    public ItemMedicineModel MEDIKIT
    {
      get
      {
        return this[GameItems.IDs.MEDICINE_MEDIKIT] as ItemMedicineModel;
      }
    }

    public ItemMedicineModel PILLS_STA
    {
      get
      {
        return this[GameItems.IDs.MEDICINE_PILLS_STA] as ItemMedicineModel;
      }
    }

    public ItemMedicineModel PILLS_SLP
    {
      get
      {
        return this[GameItems.IDs.MEDICINE_PILLS_SLP] as ItemMedicineModel;
      }
    }

    public ItemMedicineModel PILLS_SAN
    {
      get
      {
        return this[GameItems.IDs.MEDICINE_PILLS_SAN] as ItemMedicineModel;
      }
    }

    public ItemMedicineModel PILLS_ANTIVIRAL
    {
      get
      {
        return this[GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL] as ItemMedicineModel;
      }
    }

    public ItemFoodModel ARMY_RATION
    {
      get
      {
        return this[GameItems.IDs.FOOD_ARMY_RATION] as ItemFoodModel;
      }
    }

    public ItemFoodModel GROCERIES
    {
      get
      {
        return this[GameItems.IDs.FOOD_GROCERIES] as ItemFoodModel;
      }
    }

    public ItemFoodModel CANNED_FOOD
    {
      get
      {
        return this[GameItems.IDs.FOOD_CANNED_FOOD] as ItemFoodModel;
      }
    }

    public ItemMeleeWeaponModel CROWBAR
    {
      get
      {
        return this[GameItems.IDs.MELEE_CROWBAR] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel BASEBALLBAT
    {
      get
      {
        return this[GameItems.IDs.MELEE_BASEBALLBAT] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel COMBAT_KNIFE
    {
      get
      {
        return this[GameItems.IDs.MELEE_COMBAT_KNIFE] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel UNIQUE_JASON_MYERS_AXE
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_JASON_MYERS_AXE] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel GOLFCLUB
    {
      get
      {
        return this[GameItems.IDs.MELEE_GOLFCLUB] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel HUGE_HAMMER
    {
      get
      {
        return this[GameItems.IDs.MELEE_HUGE_HAMMER] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel SMALL_HAMMER
    {
      get
      {
        return this[GameItems.IDs.MELEE_SMALL_HAMMER] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel IRON_GOLFCLUB
    {
      get
      {
        return this[GameItems.IDs.MELEE_IRON_GOLFCLUB] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel SHOVEL
    {
      get
      {
        return this[GameItems.IDs.MELEE_SHOVEL] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel SHORT_SHOVEL
    {
      get
      {
        return this[GameItems.IDs.MELEE_SHORT_SHOVEL] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel TRUNCHEON
    {
      get
      {
        return this[GameItems.IDs.MELEE_TRUNCHEON] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel IMPROVISED_CLUB
    {
      get
      {
        return this[GameItems.IDs.MELEE_IMPROVISED_CLUB] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel IMPROVISED_SPEAR
    {
      get
      {
        return this[GameItems.IDs.MELEE_IMPROVISED_SPEAR] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel UNIQUE_FAMU_FATARU_KATANA
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel UNIQUE_BIGBEAR_BAT
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_BIGBEAR_BAT] as ItemMeleeWeaponModel;
      }
    }

    public ItemMeleeWeaponModel UNIQUE_ROGUEDJACK_KEYBOARD
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_ROGUEDJACK_KEYBOARD] as ItemMeleeWeaponModel;
      }
    }

    public ItemRangedWeaponModel ARMY_PISTOL
    {
      get
      {
        return this[GameItems.IDs.RANGED_ARMY_PISTOL] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel ARMY_RIFLE
    {
      get
      {
        return this[GameItems.IDs.RANGED_ARMY_RIFLE] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel HUNTING_CROSSBOW
    {
      get
      {
        return this[GameItems.IDs.RANGED_HUNTING_CROSSBOW] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel HUNTING_RIFLE
    {
      get
      {
        return this[GameItems.IDs.RANGED_HUNTING_RIFLE] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel KOLT_REVOLVER
    {
      get
      {
        return this[GameItems.IDs.RANGED_KOLT_REVOLVER] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel PISTOL
    {
      get
      {
        return this[GameItems.IDs.RANGED_PISTOL] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel PRECISION_RIFLE
    {
      get
      {
        return this[GameItems.IDs.RANGED_PRECISION_RIFLE] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel SHOTGUN
    {
      get
      {
        return this[GameItems.IDs.RANGED_SHOTGUN] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel UNIQUE_SANTAMAN_SHOTGUN
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN] as ItemRangedWeaponModel;
      }
    }

    public ItemRangedWeaponModel UNIQUE_HANS_VON_HANZ_PISTOL
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL] as ItemRangedWeaponModel;
      }
    }

    public ItemAmmoModel AMMO_LIGHT_PISTOL
    {
      get
      {
        return this[GameItems.IDs.AMMO_LIGHT_PISTOL] as ItemAmmoModel;
      }
    }

    public ItemAmmoModel AMMO_HEAVY_PISTOL
    {
      get
      {
        return this[GameItems.IDs.AMMO_HEAVY_PISTOL] as ItemAmmoModel;
      }
    }

    public ItemAmmoModel AMMO_LIGHT_RIFLE
    {
      get
      {
        return this[GameItems.IDs.AMMO_LIGHT_RIFLE] as ItemAmmoModel;
      }
    }

    public ItemAmmoModel AMMO_HEAVY_RIFLE
    {
      get
      {
        return this[GameItems.IDs.AMMO_HEAVY_RIFLE] as ItemAmmoModel;
      }
    }

    public ItemAmmoModel AMMO_SHOTGUN
    {
      get
      {
        return this[GameItems.IDs.AMMO_SHOTGUN] as ItemAmmoModel;
      }
    }

    public ItemAmmoModel AMMO_BOLTS
    {
      get
      {
        return this[GameItems.IDs.AMMO_BOLTS] as ItemAmmoModel;
      }
    }

    public ItemGrenadeModel GRENADE
    {
      get
      {
        return this[GameItems.IDs.EXPLOSIVE_GRENADE] as ItemGrenadeModel;
      }
    }

    public ItemGrenadePrimedModel GRENADE_PRIMED
    {
      get
      {
        return this[GameItems.IDs.EXPLOSIVE_GRENADE_PRIMED] as ItemGrenadePrimedModel;
      }
    }

    public ItemBarricadeMaterialModel WOODENPLANK
    {
      get
      {
        return this[GameItems.IDs.BAR_WOODEN_PLANK] as ItemBarricadeMaterialModel;
      }
    }

    public ItemBodyArmorModel ARMY_BODYARMOR
    {
      get
      {
        return this[GameItems.IDs.ARMOR_ARMY_BODYARMOR] as ItemBodyArmorModel;
      }
    }

    public ItemBodyArmorModel CHAR_LT_BODYARMOR
    {
      get
      {
        return this[GameItems.IDs.ARMOR_CHAR_LIGHT_BODYARMOR] as ItemBodyArmorModel;
      }
    }

    public ItemBodyArmorModel HELLS_SOULS_JACKET
    {
      get
      {
        return this[GameItems.IDs.ARMOR_HELLS_SOULS_JACKET] as ItemBodyArmorModel;
      }
    }

    public ItemBodyArmorModel FREE_ANGELS_JACKET
    {
      get
      {
        return this[GameItems.IDs.ARMOR_FREE_ANGELS_JACKET] as ItemBodyArmorModel;
      }
    }

    public ItemBodyArmorModel POLICE_JACKET
    {
      get
      {
        return this[GameItems.IDs.ARMOR_POLICE_JACKET] as ItemBodyArmorModel;
      }
    }

    public ItemBodyArmorModel POLICE_RIOT
    {
      get
      {
        return this[GameItems.IDs.ARMOR_POLICE_RIOT] as ItemBodyArmorModel;
      }
    }

    public ItemBodyArmorModel HUNTER_VEST
    {
      get
      {
        return this[GameItems.IDs.ARMOR_HUNTER_VEST] as ItemBodyArmorModel;
      }
    }

    public ItemTrackerModel BLACKOPS_GPS
    {
      get
      {
        return this[GameItems.IDs.TRACKER_BLACKOPS] as ItemTrackerModel;
      }
    }

    public ItemTrackerModel CELL_PHONE
    {
      get
      {
        return this[GameItems.IDs.TRACKER_CELL_PHONE] as ItemTrackerModel;
      }
    }

    public ItemTrackerModel ZTRACKER
    {
      get
      {
        return this[GameItems.IDs.TRACKER_ZTRACKER] as ItemTrackerModel;
      }
    }

    public ItemTrackerModel POLICE_RADIO
    {
      get
      {
        return this[GameItems.IDs.TRACKER_POLICE_RADIO] as ItemTrackerModel;
      }
    }

    public ItemSprayPaintModel SPRAY_PAINT1
    {
      get
      {
        return this[GameItems.IDs.SPRAY_PAINT1] as ItemSprayPaintModel;
      }
    }

    public ItemSprayPaintModel SPRAY_PAINT2
    {
      get
      {
        return this[GameItems.IDs.SPRAY_PAINT2] as ItemSprayPaintModel;
      }
    }

    public ItemSprayPaintModel SPRAY_PAINT3
    {
      get
      {
        return this[GameItems.IDs.SPRAY_PAINT3] as ItemSprayPaintModel;
      }
    }

    public ItemSprayPaintModel SPRAY_PAINT4
    {
      get
      {
        return this[GameItems.IDs.SPRAY_PAINT4] as ItemSprayPaintModel;
      }
    }

    public ItemLightModel FLASHLIGHT
    {
      get
      {
        return this[GameItems.IDs.LIGHT_FLASHLIGHT] as ItemLightModel;
      }
    }

    public ItemLightModel BIG_FLASHLIGHT
    {
      get
      {
        return this[GameItems.IDs.LIGHT_BIG_FLASHLIGHT] as ItemLightModel;
      }
    }

    public ItemModel STENCH_KILLER
    {
      get
      {
        return this[GameItems.IDs.SCENT_SPRAY_STENCH_KILLER];
      }
    }

    public ItemModel EMPTY_CAN
    {
      get
      {
        return this[GameItems.IDs.TRAP_EMPTY_CAN];
      }
    }

    public ItemModel BEAR_TRAP
    {
      get
      {
        return this[GameItems.IDs.TRAP_BEAR_TRAP];
      }
    }

    public ItemModel SPIKES
    {
      get
      {
        return this[GameItems.IDs.TRAP_SPIKES];
      }
    }

    public ItemModel BARBED_WIRE
    {
      get
      {
        return this[GameItems.IDs.TRAP_BARBED_WIRE];
      }
    }

    public ItemModel BOOK
    {
      get
      {
        return this[GameItems.IDs.ENT_BOOK];
      }
    }

    public ItemModel MAGAZINE
    {
      get
      {
        return this[GameItems.IDs.ENT_MAGAZINE];
      }
    }

    public ItemModel UNIQUE_SUBWAY_BADGE
    {
      get
      {
        return this[GameItems.IDs.UNIQUE_SUBWAY_BADGE];
      }
    }

    public GameItems()
    {
      Models.Items = (ItemModelDB) this;
    }

    public void CreateModels()
    {
      // Medicine
      this[GameItems.IDs.MEDICINE_BANDAGES] = new ItemMedicineModel(DATA_MEDICINE_BANDAGE.NAME, DATA_MEDICINE_BANDAGE.PLURAL, GameImages.ITEM_BANDAGES, DATA_MEDICINE_BANDAGE.HEALING, DATA_MEDICINE_BANDAGE.STAMINABOOST, DATA_MEDICINE_BANDAGE.SLEEPBOOST, DATA_MEDICINE_BANDAGE.INFECTIONCURE, DATA_MEDICINE_BANDAGE.SANITYCURE, DATA_MEDICINE_BANDAGE.FLAVOR, DATA_MEDICINE_BANDAGE.STACKINGLIMIT);
      this[GameItems.IDs.MEDICINE_MEDIKIT] = new ItemMedicineModel(DATA_MEDICINE_MEDIKIT.NAME, DATA_MEDICINE_MEDIKIT.PLURAL, GameImages.ITEM_MEDIKIT, DATA_MEDICINE_MEDIKIT.HEALING, DATA_MEDICINE_MEDIKIT.STAMINABOOST, DATA_MEDICINE_MEDIKIT.SLEEPBOOST, DATA_MEDICINE_MEDIKIT.INFECTIONCURE, DATA_MEDICINE_MEDIKIT.SANITYCURE, DATA_MEDICINE_MEDIKIT.FLAVOR);
      this[GameItems.IDs.MEDICINE_PILLS_STA] = new ItemMedicineModel(DATA_MEDICINE_PILLS_STA.NAME, DATA_MEDICINE_PILLS_STA.PLURAL, GameImages.ITEM_PILLS_GREEN, DATA_MEDICINE_PILLS_STA.HEALING, DATA_MEDICINE_PILLS_STA.STAMINABOOST, DATA_MEDICINE_PILLS_STA.SLEEPBOOST, DATA_MEDICINE_PILLS_STA.INFECTIONCURE, DATA_MEDICINE_PILLS_STA.SANITYCURE, DATA_MEDICINE_PILLS_STA.FLAVOR, DATA_MEDICINE_PILLS_STA.STACKINGLIMIT);
      this[GameItems.IDs.MEDICINE_PILLS_SLP] = new ItemMedicineModel(DATA_MEDICINE_PILLS_SLP.NAME, DATA_MEDICINE_PILLS_SLP.PLURAL, GameImages.ITEM_PILLS_BLUE, DATA_MEDICINE_PILLS_SLP.HEALING, DATA_MEDICINE_PILLS_SLP.STAMINABOOST, DATA_MEDICINE_PILLS_SLP.SLEEPBOOST, DATA_MEDICINE_PILLS_SLP.INFECTIONCURE, DATA_MEDICINE_PILLS_SLP.SANITYCURE, DATA_MEDICINE_PILLS_SLP.FLAVOR, DATA_MEDICINE_PILLS_SLP.STACKINGLIMIT);
      this[GameItems.IDs.MEDICINE_PILLS_SAN] = new ItemMedicineModel(DATA_MEDICINE_PILLS_SAN.NAME, DATA_MEDICINE_PILLS_SAN.PLURAL, GameImages.ITEM_PILLS_SAN, DATA_MEDICINE_PILLS_SAN.HEALING, DATA_MEDICINE_PILLS_SAN.STAMINABOOST, DATA_MEDICINE_PILLS_SAN.SLEEPBOOST, DATA_MEDICINE_PILLS_SAN.INFECTIONCURE, DATA_MEDICINE_PILLS_SAN.SANITYCURE, DATA_MEDICINE_PILLS_SAN.FLAVOR, DATA_MEDICINE_PILLS_SAN.STACKINGLIMIT);
      this[GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL] = new ItemMedicineModel(DATA_MEDICINE_PILLS_ANTIVIRAL.NAME, DATA_MEDICINE_PILLS_ANTIVIRAL.PLURAL, GameImages.ITEM_PILLS_ANTIVIRAL, DATA_MEDICINE_PILLS_ANTIVIRAL.HEALING, DATA_MEDICINE_PILLS_ANTIVIRAL.STAMINABOOST, DATA_MEDICINE_PILLS_ANTIVIRAL.SLEEPBOOST, DATA_MEDICINE_PILLS_ANTIVIRAL.INFECTIONCURE, DATA_MEDICINE_PILLS_ANTIVIRAL.SANITYCURE, DATA_MEDICINE_PILLS_ANTIVIRAL.FLAVOR, DATA_MEDICINE_PILLS_ANTIVIRAL.STACKINGLIMIT);

      // Food
      this[GameItems.IDs.FOOD_ARMY_RATION] = new ItemFoodModel(DATA_FOOD_ARMY_RATION.NAME, DATA_FOOD_ARMY_RATION.PLURAL, GameImages.ITEM_ARMY_RATION, DATA_FOOD_ARMY_RATION.NUTRITION, DATA_FOOD_ARMY_RATION.BESTBEFORE, DATA_FOOD_ARMY_RATION.STACKINGLIMIT, DATA_FOOD_ARMY_RATION.FLAVOR);
      this[GameItems.IDs.FOOD_GROCERIES] = new ItemFoodModel(DATA_FOOD_GROCERIES.NAME, DATA_FOOD_GROCERIES.PLURAL, GameImages.ITEM_GROCERIES, DATA_FOOD_GROCERIES.NUTRITION, DATA_FOOD_GROCERIES.BESTBEFORE, DATA_FOOD_GROCERIES.STACKINGLIMIT, DATA_FOOD_GROCERIES.FLAVOR);
      this[GameItems.IDs.FOOD_CANNED_FOOD] = new ItemFoodModel(DATA_FOOD_CANNED_FOOD.NAME, DATA_FOOD_CANNED_FOOD.PLURAL, GameImages.ITEM_CANNED_FOOD, DATA_FOOD_CANNED_FOOD.NUTRITION, DATA_FOOD_CANNED_FOOD.BESTBEFORE, DATA_FOOD_CANNED_FOOD.STACKINGLIMIT, DATA_FOOD_CANNED_FOOD.FLAVOR);

      // melee weapons
      this[GameItems.IDs.MELEE_BASEBALLBAT] = new ItemMeleeWeaponModel(DATA_MELEE_BASEBALLBAT.NAME, DATA_MELEE_BASEBALLBAT.PLURAL, GameImages.ITEM_BASEBALL_BAT, new Attack(AttackKind.PHYSICAL, new Verb("smash", "smashes"), DATA_MELEE_BASEBALLBAT.ATK, DATA_MELEE_BASEBALLBAT.DMG, DATA_MELEE_BASEBALLBAT.STA), DATA_MELEE_BASEBALLBAT.FLAVOR) { IsFragile = DATA_MELEE_BASEBALLBAT.ISFRAGILE };
      this[GameItems.IDs.MELEE_BASEBALLBAT].StackingLimit = DATA_MELEE_BASEBALLBAT.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_COMBAT_KNIFE] = new ItemMeleeWeaponModel(DATA_MELEE_COMBAT_KNIFE.NAME, DATA_MELEE_COMBAT_KNIFE.PLURAL, GameImages.ITEM_COMBAT_KNIFE, new Attack(AttackKind.PHYSICAL, new Verb("stab", "stabs"), DATA_MELEE_COMBAT_KNIFE.ATK, DATA_MELEE_COMBAT_KNIFE.DMG, DATA_MELEE_COMBAT_KNIFE.STA), DATA_MELEE_COMBAT_KNIFE.FLAVOR) { IsFragile = DATA_MELEE_COMBAT_KNIFE.ISFRAGILE };
      this[GameItems.IDs.MELEE_COMBAT_KNIFE].StackingLimit = DATA_MELEE_COMBAT_KNIFE.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_CROWBAR] = new ItemMeleeWeaponModel(DATA_MELEE_CROWBAR.NAME, DATA_MELEE_CROWBAR.PLURAL, GameImages.ITEM_CROWBAR, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_CROWBAR.ATK, DATA_MELEE_CROWBAR.DMG, DATA_MELEE_CROWBAR.STA), DATA_MELEE_CROWBAR.FLAVOR) { IsFragile = DATA_MELEE_CROWBAR.ISFRAGILE };
      this[GameItems.IDs.MELEE_CROWBAR].StackingLimit = DATA_MELEE_CROWBAR.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_GOLFCLUB] = new ItemMeleeWeaponModel(DATA_MELEE_GOLFCLUB.NAME, DATA_MELEE_GOLFCLUB.PLURAL, GameImages.ITEM_GOLF_CLUB, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_GOLFCLUB.ATK, DATA_MELEE_GOLFCLUB.DMG, DATA_MELEE_GOLFCLUB.STA), DATA_MELEE_GOLFCLUB.FLAVOR) { IsFragile = DATA_MELEE_GOLFCLUB.ISFRAGILE };
      this[GameItems.IDs.MELEE_GOLFCLUB].StackingLimit = DATA_MELEE_GOLFCLUB.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_IRON_GOLFCLUB] = new ItemMeleeWeaponModel(DATA_MELEE_IRON_GOLFCLUB.NAME, DATA_MELEE_IRON_GOLFCLUB.PLURAL, GameImages.ITEM_IRON_GOLF_CLUB, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_IRON_GOLFCLUB.ATK, DATA_MELEE_IRON_GOLFCLUB.DMG, DATA_MELEE_IRON_GOLFCLUB.STA), DATA_MELEE_IRON_GOLFCLUB.FLAVOR) { IsFragile = DATA_MELEE_IRON_GOLFCLUB.ISFRAGILE };
      this[GameItems.IDs.MELEE_IRON_GOLFCLUB].StackingLimit = DATA_MELEE_IRON_GOLFCLUB.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_HUGE_HAMMER] = new ItemMeleeWeaponModel(DATA_MELEE_HUGE_HAMMER.NAME, DATA_MELEE_HUGE_HAMMER.PLURAL, GameImages.ITEM_HUGE_HAMMER, new Attack(AttackKind.PHYSICAL, new Verb("smash", "smashes"), DATA_MELEE_HUGE_HAMMER.ATK, DATA_MELEE_HUGE_HAMMER.DMG, DATA_MELEE_HUGE_HAMMER.STA), DATA_MELEE_HUGE_HAMMER.FLAVOR) { IsFragile = DATA_MELEE_HUGE_HAMMER.ISFRAGILE };
      this[GameItems.IDs.MELEE_HUGE_HAMMER].StackingLimit = DATA_MELEE_HUGE_HAMMER.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_SHOVEL] = new ItemMeleeWeaponModel(DATA_MELEE_SHOVEL.NAME, DATA_MELEE_SHOVEL.PLURAL, GameImages.ITEM_SHOVEL, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_SHOVEL.ATK, DATA_MELEE_SHOVEL.DMG, DATA_MELEE_SHOVEL.STA), DATA_MELEE_SHOVEL.FLAVOR) { IsFragile = DATA_MELEE_SHOVEL.ISFRAGILE };
      this[GameItems.IDs.MELEE_SHOVEL].StackingLimit = DATA_MELEE_SHOVEL.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_SHORT_SHOVEL] = new ItemMeleeWeaponModel(DATA_MELEE_SHORT_SHOVEL.NAME, DATA_MELEE_SHORT_SHOVEL.PLURAL, GameImages.ITEM_SHORT_SHOVEL, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_SHORT_SHOVEL.ATK, DATA_MELEE_SHORT_SHOVEL.DMG, DATA_MELEE_SHORT_SHOVEL.STA), DATA_MELEE_SHORT_SHOVEL.FLAVOR){ IsFragile = DATA_MELEE_SHORT_SHOVEL.ISFRAGILE };
      this[GameItems.IDs.MELEE_SHORT_SHOVEL].StackingLimit = DATA_MELEE_SHORT_SHOVEL.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_TRUNCHEON] = new ItemMeleeWeaponModel(DATA_MELEE_TRUNCHEON.NAME, DATA_MELEE_TRUNCHEON.PLURAL, GameImages.ITEM_TRUNCHEON, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_TRUNCHEON.ATK, DATA_MELEE_TRUNCHEON.DMG, DATA_MELEE_TRUNCHEON.STA), DATA_MELEE_TRUNCHEON.FLAVOR) { IsFragile = DATA_MELEE_TRUNCHEON.ISFRAGILE };
      this[GameItems.IDs.MELEE_TRUNCHEON].StackingLimit = DATA_MELEE_TRUNCHEON.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_IMPROVISED_CLUB] = new ItemMeleeWeaponModel(DATA_MELEE_IMPROVISED_CLUB.NAME, DATA_MELEE_IMPROVISED_CLUB.PLURAL, GameImages.ITEM_IMPROVISED_CLUB, new Attack(AttackKind.PHYSICAL, new Verb("strike"), DATA_MELEE_IMPROVISED_CLUB.ATK, DATA_MELEE_IMPROVISED_CLUB.DMG, DATA_MELEE_IMPROVISED_CLUB.STA), DATA_MELEE_IMPROVISED_CLUB.FLAVOR) { IsFragile = DATA_MELEE_IMPROVISED_CLUB.ISFRAGILE };
      this[GameItems.IDs.MELEE_IMPROVISED_CLUB].StackingLimit = DATA_MELEE_IMPROVISED_CLUB.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_IMPROVISED_SPEAR] = new ItemMeleeWeaponModel(DATA_MELEE_IMPROVISED_SPEAR.NAME, DATA_MELEE_IMPROVISED_SPEAR.PLURAL, GameImages.ITEM_IMPROVISED_SPEAR, new Attack(AttackKind.PHYSICAL, new Verb("pierce"), DATA_MELEE_IMPROVISED_SPEAR.ATK, DATA_MELEE_IMPROVISED_SPEAR.DMG, DATA_MELEE_IMPROVISED_SPEAR.STA), DATA_MELEE_IMPROVISED_SPEAR.FLAVOR) { IsFragile = DATA_MELEE_IMPROVISED_SPEAR.ISFRAGILE };
      this[GameItems.IDs.MELEE_IMPROVISED_SPEAR].StackingLimit = DATA_MELEE_IMPROVISED_SPEAR.STACKINGLIMIT;
      this[GameItems.IDs.MELEE_SMALL_HAMMER] = new ItemMeleeWeaponModel(DATA_MELEE_SMALL_HAMMER.NAME, DATA_MELEE_SMALL_HAMMER.PLURAL, GameImages.ITEM_SMALL_HAMMER, new Attack(AttackKind.PHYSICAL, new Verb("smash"), DATA_MELEE_SMALL_HAMMER.ATK, DATA_MELEE_SMALL_HAMMER.DMG, DATA_MELEE_SMALL_HAMMER.STA), DATA_MELEE_SMALL_HAMMER.FLAVOR) { IsFragile = DATA_MELEE_SMALL_HAMMER.ISFRAGILE };
      this[GameItems.IDs.MELEE_SMALL_HAMMER].StackingLimit = DATA_MELEE_SMALL_HAMMER.STACKINGLIMIT;
      this[GameItems.IDs.UNIQUE_JASON_MYERS_AXE] = new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_JASON_MYERS_AXE.NAME, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.PLURAL, GameImages.ITEM_JASON_MYERS_AXE, new Attack(AttackKind.PHYSICAL, new Verb("slash", "slashes"), DATA_MELEE_UNIQUE_JASON_MYERS_AXE.ATK, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.DMG, DATA_MELEE_UNIQUE_JASON_MYERS_AXE.STA), DATA_MELEE_UNIQUE_JASON_MYERS_AXE.FLAVOR);
      this[GameItems.IDs.UNIQUE_JASON_MYERS_AXE].IsProper = true;
      this[GameItems.IDs.UNIQUE_JASON_MYERS_AXE].IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA] = new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.NAME, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.PLURAL, GameImages.ITEM_FAMU_FATARU_KATANA, new Attack(AttackKind.PHYSICAL, new Verb("slash", "slashes"), DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.ATK, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.DMG, DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.STA), DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA.FLAVOR);
      this[GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA].IsProper = true;
      this[GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA].IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_BIGBEAR_BAT] = new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_BIGBEAR_BAT.NAME, DATA_MELEE_UNIQUE_BIGBEAR_BAT.PLURAL, GameImages.ITEM_BIGBEAR_BAT, new Attack(AttackKind.PHYSICAL, new Verb("smash", "smashes"), DATA_MELEE_UNIQUE_BIGBEAR_BAT.ATK, DATA_MELEE_UNIQUE_BIGBEAR_BAT.DMG, DATA_MELEE_UNIQUE_BIGBEAR_BAT.STA), DATA_MELEE_UNIQUE_BIGBEAR_BAT.FLAVOR);
      this[GameItems.IDs.UNIQUE_BIGBEAR_BAT].IsProper = true;
      this[GameItems.IDs.UNIQUE_BIGBEAR_BAT].IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_ROGUEDJACK_KEYBOARD] = new ItemMeleeWeaponModel(DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.NAME, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.PLURAL, GameImages.ITEM_ROGUEDJACK_KEYBOARD, new Attack(AttackKind.PHYSICAL, new Verb("bash", "bashes"), DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.ATK, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.DMG, DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.STA), DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD.FLAVOR);
      this[GameItems.IDs.UNIQUE_ROGUEDJACK_KEYBOARD].IsProper = true;
      this[GameItems.IDs.UNIQUE_ROGUEDJACK_KEYBOARD].IsUnbreakable = true;

      // ranged weapons
      this[GameItems.IDs.RANGED_ARMY_PISTOL] = new ItemRangedWeaponModel(DATA_RANGED_ARMY_PISTOL.NAME, DATA_RANGED_ARMY_PISTOL.FLAVOR, GameImages.ITEM_ARMY_PISTOL, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_RANGED_ARMY_PISTOL.ATK, DATA_RANGED_ARMY_PISTOL.DMG, 0, DATA_RANGED_ARMY_PISTOL.RANGE), DATA_RANGED_ARMY_PISTOL.MAXAMMO, AmmoType.HEAVY_PISTOL, DATA_RANGED_ARMY_PISTOL.FLAVOR);
      this[GameItems.IDs.RANGED_ARMY_RIFLE] = new ItemRangedWeaponModel(DATA_RANGED_ARMY_RIFLE.NAME, DATA_RANGED_ARMY_RIFLE.FLAVOR, GameImages.ITEM_ARMY_RIFLE, new Attack(AttackKind.FIREARM, new Verb("fire a salvo at", "fires a salvo at"), DATA_RANGED_ARMY_RIFLE.ATK, DATA_RANGED_ARMY_RIFLE.DMG, 0, DATA_RANGED_ARMY_RIFLE.RANGE), DATA_RANGED_ARMY_RIFLE.MAXAMMO, AmmoType.HEAVY_RIFLE, DATA_RANGED_ARMY_RIFLE.FLAVOR);
      this[GameItems.IDs.RANGED_HUNTING_CROSSBOW] = new ItemRangedWeaponModel(DATA_RANGED_HUNTING_CROSSBOW.NAME, DATA_RANGED_HUNTING_CROSSBOW.FLAVOR, GameImages.ITEM_HUNTING_CROSSBOW, new Attack(AttackKind.BOW, new Verb("shoot"), DATA_RANGED_HUNTING_CROSSBOW.ATK, DATA_RANGED_HUNTING_CROSSBOW.DMG, 0, DATA_RANGED_HUNTING_CROSSBOW.RANGE), DATA_RANGED_HUNTING_CROSSBOW.MAXAMMO, AmmoType.BOLT, DATA_RANGED_HUNTING_CROSSBOW.FLAVOR);
      this[GameItems.IDs.RANGED_HUNTING_RIFLE] = new ItemRangedWeaponModel(DATA_RANGED_HUNTING_RIFLE.NAME, DATA_RANGED_HUNTING_RIFLE.FLAVOR, GameImages.ITEM_HUNTING_RIFLE, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_RANGED_HUNTING_RIFLE.ATK, DATA_RANGED_HUNTING_RIFLE.DMG, 0, DATA_RANGED_HUNTING_RIFLE.RANGE), DATA_RANGED_HUNTING_RIFLE.MAXAMMO, AmmoType.LIGHT_RIFLE, DATA_RANGED_HUNTING_RIFLE.FLAVOR);
      this[GameItems.IDs.RANGED_PISTOL] = new ItemRangedWeaponModel(DATA_RANGED_PISTOL.NAME, DATA_RANGED_PISTOL.FLAVOR, GameImages.ITEM_PISTOL, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_RANGED_PISTOL.ATK, DATA_RANGED_PISTOL.DMG, 0, DATA_RANGED_PISTOL.RANGE), DATA_RANGED_PISTOL.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_RANGED_PISTOL.FLAVOR);
      this[GameItems.IDs.RANGED_KOLT_REVOLVER] = new ItemRangedWeaponModel(DATA_RANGED_KOLT_REVOLVER.NAME, DATA_RANGED_KOLT_REVOLVER.FLAVOR, GameImages.ITEM_KOLT_REVOLVER, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_RANGED_KOLT_REVOLVER.ATK, DATA_RANGED_KOLT_REVOLVER.DMG, 0, DATA_RANGED_KOLT_REVOLVER.RANGE), DATA_RANGED_KOLT_REVOLVER.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_RANGED_KOLT_REVOLVER.FLAVOR);
      this[GameItems.IDs.RANGED_PRECISION_RIFLE] = new ItemRangedWeaponModel(DATA_RANGED_PRECISION_RIFLE.NAME, DATA_RANGED_PRECISION_RIFLE.FLAVOR, GameImages.ITEM_PRECISION_RIFLE, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_RANGED_PRECISION_RIFLE.ATK, DATA_RANGED_PRECISION_RIFLE.DMG, 0, DATA_RANGED_PRECISION_RIFLE.RANGE), DATA_RANGED_PRECISION_RIFLE.MAXAMMO, AmmoType.HEAVY_RIFLE, DATA_RANGED_PRECISION_RIFLE.FLAVOR);
      this[GameItems.IDs.RANGED_SHOTGUN] = new ItemRangedWeaponModel(DATA_RANGED_SHOTGUN.NAME, DATA_RANGED_SHOTGUN.FLAVOR, GameImages.ITEM_SHOTGUN, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_RANGED_SHOTGUN.ATK, DATA_RANGED_SHOTGUN.DMG, 0, DATA_RANGED_SHOTGUN.RANGE), DATA_RANGED_SHOTGUN.MAXAMMO, AmmoType.SHOTGUN, DATA_RANGED_SHOTGUN.FLAVOR);
      this[GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN] = new ItemRangedWeaponModel(DATA_UNIQUE_SANTAMAN_SHOTGUN.NAME, DATA_UNIQUE_SANTAMAN_SHOTGUN.FLAVOR, GameImages.ITEM_SANTAMAN_SHOTGUN, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_UNIQUE_SANTAMAN_SHOTGUN.ATK, DATA_UNIQUE_SANTAMAN_SHOTGUN.DMG, 0, DATA_UNIQUE_SANTAMAN_SHOTGUN.RANGE), DATA_UNIQUE_SANTAMAN_SHOTGUN.MAXAMMO, AmmoType.SHOTGUN, DATA_UNIQUE_SANTAMAN_SHOTGUN.FLAVOR);
      this[GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN].IsProper = true;
      this[GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN].IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL] = new ItemRangedWeaponModel(DATA_UNIQUE_HANS_VON_HANZ_PISTOL.NAME, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.FLAVOR, GameImages.ITEM_HANS_VON_HANZ_PISTOL, new Attack(AttackKind.FIREARM, new Verb("shoot"), DATA_UNIQUE_HANS_VON_HANZ_PISTOL.ATK, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.DMG, 0, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.RANGE), DATA_UNIQUE_HANS_VON_HANZ_PISTOL.MAXAMMO, AmmoType.LIGHT_PISTOL, DATA_UNIQUE_HANS_VON_HANZ_PISTOL.FLAVOR);
      this[GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL].IsProper = true;
      this[GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL].IsUnbreakable = true;

      // Ammunition
      this[GameItems.IDs.AMMO_LIGHT_PISTOL] = new ItemAmmoModel(GameImages.ITEM_AMMO_LIGHT_PISTOL, AmmoType.LIGHT_PISTOL, 20);
      this[GameItems.IDs.AMMO_HEAVY_PISTOL] = new ItemAmmoModel(GameImages.ITEM_AMMO_HEAVY_PISTOL, AmmoType.HEAVY_PISTOL, 12);
      this[GameItems.IDs.AMMO_LIGHT_RIFLE] = new ItemAmmoModel(GameImages.ITEM_AMMO_LIGHT_RIFLE, AmmoType.LIGHT_RIFLE, 14);
      this[GameItems.IDs.AMMO_HEAVY_RIFLE] = new ItemAmmoModel(GameImages.ITEM_AMMO_HEAVY_RIFLE, AmmoType.HEAVY_RIFLE, 20);
      this[GameItems.IDs.AMMO_SHOTGUN] = new ItemAmmoModel(GameImages.ITEM_AMMO_SHOTGUN, AmmoType.SHOTGUN, 10);
      this[GameItems.IDs.AMMO_BOLTS] = new ItemAmmoModel(GameImages.ITEM_AMMO_BOLTS, AmmoType.BOLT, 30);
      
      // grenade, in its various states
      GameItems.ExplosiveData explosiveData = DATA_EXPLOSIVE_GRENADE;
      int[] damage = new int[explosiveData.RADIUS + 1];
      for (int index = 0; index < explosiveData.RADIUS + 1; ++index)
        damage[index] = explosiveData.DMG[index];   // XXX explosiveData.DMG is returned with a mismatched length

      ItemGrenadeModel itemGrenadeModel1 = new ItemGrenadeModel(explosiveData.NAME, explosiveData.PLURAL, "Items\\item_grenade", explosiveData.FUSE, new BlastAttack(explosiveData.RADIUS, damage, true, false), "Icons\\blast", explosiveData.MAXTHROW);
      itemGrenadeModel1.EquipmentPart = DollPart.RIGHT_HAND;
      itemGrenadeModel1.StackingLimit = explosiveData.STACKLINGLIMIT;
      itemGrenadeModel1.FlavorDescription = explosiveData.FLAVOR;
      this[GameItems.IDs.EXPLOSIVE_GRENADE] = (ItemModel) itemGrenadeModel1;
      ItemGrenadePrimedModel grenadePrimedModel1 = new ItemGrenadePrimedModel("primed " + explosiveData.NAME, "primed " + explosiveData.PLURAL, "Items\\item_grenade_primed", this[GameItems.IDs.EXPLOSIVE_GRENADE] as ItemGrenadeModel);
      grenadePrimedModel1.EquipmentPart = DollPart.RIGHT_HAND;
      this[GameItems.IDs.EXPLOSIVE_GRENADE_PRIMED] = (ItemModel) grenadePrimedModel1;

      // carpentry
      GameItems.BarricadingMaterialData barricadingMaterialData = DATA_BAR_WOODEN_PLANK;
      ItemBarricadeMaterialModel barricadeMaterialModel1 = new ItemBarricadeMaterialModel(barricadingMaterialData.NAME, barricadingMaterialData.PLURAL, "Items\\item_wooden_plank", barricadingMaterialData.VALUE);
      barricadeMaterialModel1.StackingLimit = barricadingMaterialData.STACKINGLIMIT;
      barricadeMaterialModel1.FlavorDescription = barricadingMaterialData.FLAVOR;
      this[GameItems.IDs.BAR_WOODEN_PLANK] = (ItemModel) barricadeMaterialModel1;

      // body armor
      GameItems.ArmorData armorData1 = DATA_ARMOR_ARMY;
      ItemBodyArmorModel tmpBodyArmorModel = new ItemBodyArmorModel(armorData1.NAME, armorData1.PLURAL, "Items\\item_army_bodyarmor", armorData1.PRO_HIT, armorData1.PRO_SHOT, armorData1.ENC, armorData1.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData1.FLAVOR;
      this[GameItems.IDs.ARMOR_ARMY_BODYARMOR] = (ItemModel) tmpBodyArmorModel;
      GameItems.ArmorData armorData2 = DATA_ARMOR_CHAR;
      tmpBodyArmorModel = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_CHAR_light_bodyarmor", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_CHAR_LIGHT_BODYARMOR] = (ItemModel) tmpBodyArmorModel;
      armorData2 = DATA_ARMOR_HELLS_SOULS_JACKET;
      tmpBodyArmorModel = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_hells_souls_jacket", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_HELLS_SOULS_JACKET] = (ItemModel) tmpBodyArmorModel;
      armorData2 = DATA_ARMOR_FREE_ANGELS_JACKET;
      tmpBodyArmorModel = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_free_angels_jacket", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_FREE_ANGELS_JACKET] = (ItemModel) tmpBodyArmorModel;
      armorData2 = DATA_ARMOR_POLICE_JACKET;
      tmpBodyArmorModel = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_police_jacket", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_POLICE_JACKET] = (ItemModel) tmpBodyArmorModel;
      armorData2 = DATA_ARMOR_POLICE_RIOT;
      tmpBodyArmorModel = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_police_riot_armor", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_POLICE_RIOT] = (ItemModel) tmpBodyArmorModel;
      armorData2 = DATA_ARMOR_HUNTER_VEST;
      tmpBodyArmorModel = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_hunter_vest", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      tmpBodyArmorModel.EquipmentPart = DollPart.TORSO;
      tmpBodyArmorModel.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_HUNTER_VEST] = (ItemModel) tmpBodyArmorModel;

      // trackers
      GameItems.TrackerData trackerData = DATA_TRACKER_CELL_PHONE;
      ItemTrackerModel tmpTrackerModel = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_cellphone", ItemTrackerModel.TrackingFlags.FOLLOWER_AND_LEADER, trackerData.BATTERIES * WorldTime.TURNS_PER_HOUR);
      tmpTrackerModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpTrackerModel.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_CELL_PHONE] = (ItemModel) tmpTrackerModel;
      trackerData = DATA_TRACKER_ZTRACKER;
      tmpTrackerModel = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_ztracker", ItemTrackerModel.TrackingFlags.UNDEADS, trackerData.BATTERIES * WorldTime.TURNS_PER_HOUR);
      tmpTrackerModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpTrackerModel.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_ZTRACKER] = (ItemModel) tmpTrackerModel;
      trackerData = DATA_TRACKER_BLACKOPS_GPS;
      tmpTrackerModel = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_blackops_gps", ItemTrackerModel.TrackingFlags.BLACKOPS_FACTION, trackerData.BATTERIES * WorldTime.TURNS_PER_HOUR);
      tmpTrackerModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpTrackerModel.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_BLACKOPS] = (ItemModel) tmpTrackerModel;
      trackerData = DATA_TRACKER_POLICE_RADIO;
      tmpTrackerModel = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_police_radio", ItemTrackerModel.TrackingFlags.POLICE_FACTION, trackerData.BATTERIES * WorldTime.TURNS_PER_HOUR);
      tmpTrackerModel.EquipmentPart = DollPart.HIP_HOLSTER;
      tmpTrackerModel.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_POLICE_RADIO] = (ItemModel) tmpTrackerModel;

      // spray paint
      GameItems.SprayPaintData sprayPaintData = DATA_SPRAY_PAINT1;
      ItemSprayPaintModel tmpSprayPaintModel = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag");
      tmpSprayPaintModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpSprayPaintModel.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT1] = (ItemModel) tmpSprayPaintModel;
      sprayPaintData = DATA_SPRAY_PAINT2;
      tmpSprayPaintModel = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint2", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag2");
      tmpSprayPaintModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpSprayPaintModel.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT2] = (ItemModel) tmpSprayPaintModel;
      sprayPaintData = DATA_SPRAY_PAINT3;
      tmpSprayPaintModel = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint3", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag3");
      tmpSprayPaintModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpSprayPaintModel.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT3] = (ItemModel) tmpSprayPaintModel;
      sprayPaintData = DATA_SPRAY_PAINT4;
      tmpSprayPaintModel = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint4", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag4");
      tmpSprayPaintModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpSprayPaintModel.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT4] = (ItemModel)tmpSprayPaintModel;

      // Flashlights
      GameItems.LightData lightData = DATA_LIGHT_FLASHLIGHT;
      ItemLightModel tmpLightModel = new ItemLightModel(lightData.NAME, lightData.PLURAL, "Items\\item_flashlight", lightData.FOV, lightData.BATTERIES * WorldTime.TURNS_PER_HOUR, "Items\\item_flashlight_out");
      tmpLightModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpLightModel.FlavorDescription = lightData.FLAVOR;
      this[GameItems.IDs.LIGHT_FLASHLIGHT] = (ItemModel) tmpLightModel;
      lightData = DATA_LIGHT_BIG_FLASHLIGHT;
      tmpLightModel = new ItemLightModel(lightData.NAME, lightData.PLURAL, "Items\\item_big_flashlight", lightData.FOV, lightData.BATTERIES * WorldTime.TURNS_PER_HOUR, "Items\\item_big_flashlight_out");
      tmpLightModel.EquipmentPart = DollPart.LEFT_HAND;
      tmpLightModel.FlavorDescription = lightData.FLAVOR;
      this[GameItems.IDs.LIGHT_BIG_FLASHLIGHT] = (ItemModel) tmpLightModel;

      // stench killer
      this[GameItems.IDs.SCENT_SPRAY_STENCH_KILLER] = new ItemSprayScentModel(DATA_SCENT_SPRAY_STENCH_KILLER.NAME, DATA_SCENT_SPRAY_STENCH_KILLER.PLURAL, GameImages.ITEM_STENCH_KILLER, DATA_SCENT_SPRAY_STENCH_KILLER.QUANTITY, Odor.PERFUME_LIVING_SUPRESSOR, DATA_SCENT_SPRAY_STENCH_KILLER.STRENGTH * 30, DATA_SCENT_SPRAY_STENCH_KILLER.FLAVOR);

      // Traps
      this[GameItems.IDs.TRAP_EMPTY_CAN] = new ItemTrapModel(DATA_TRAP_EMPTY_CAN.NAME, DATA_TRAP_EMPTY_CAN.PLURAL, GameImages.ITEM_EMPTY_CAN, DATA_TRAP_EMPTY_CAN.STACKING, DATA_TRAP_EMPTY_CAN.CHANCE, DATA_TRAP_EMPTY_CAN.DAMAGE, DATA_TRAP_EMPTY_CAN.DROP_ACTIVATE, DATA_TRAP_EMPTY_CAN.USE_ACTIVATE, DATA_TRAP_EMPTY_CAN.IS_ONE_TIME, DATA_TRAP_EMPTY_CAN.BREAK_CHANCE, DATA_TRAP_EMPTY_CAN.BLOCK_CHANCE, DATA_TRAP_EMPTY_CAN.BREAK_CHANCE_ESCAPE, DATA_TRAP_EMPTY_CAN.IS_NOISY, DATA_TRAP_EMPTY_CAN.NOISE_NAME, DATA_TRAP_EMPTY_CAN.IS_FLAMMABLE, DATA_TRAP_EMPTY_CAN.FLAVOR);
      this[GameItems.IDs.TRAP_BEAR_TRAP] = new ItemTrapModel(DATA_TRAP_BEAR_TRAP.NAME, DATA_TRAP_BEAR_TRAP.PLURAL, GameImages.ITEM_BEAR_TRAP, DATA_TRAP_BEAR_TRAP.STACKING, DATA_TRAP_BEAR_TRAP.CHANCE, DATA_TRAP_BEAR_TRAP.DAMAGE, DATA_TRAP_BEAR_TRAP.DROP_ACTIVATE, DATA_TRAP_BEAR_TRAP.USE_ACTIVATE, DATA_TRAP_BEAR_TRAP.IS_ONE_TIME, DATA_TRAP_BEAR_TRAP.BREAK_CHANCE, DATA_TRAP_BEAR_TRAP.BLOCK_CHANCE, DATA_TRAP_BEAR_TRAP.BREAK_CHANCE_ESCAPE, DATA_TRAP_BEAR_TRAP.IS_NOISY, DATA_TRAP_BEAR_TRAP.NOISE_NAME, DATA_TRAP_BEAR_TRAP.IS_FLAMMABLE, DATA_TRAP_BEAR_TRAP.FLAVOR);
      this[GameItems.IDs.TRAP_SPIKES] = new ItemTrapModel(DATA_TRAP_SPIKES.NAME, DATA_TRAP_SPIKES.PLURAL, GameImages.ITEM_SPIKES, DATA_TRAP_SPIKES.STACKING, DATA_TRAP_SPIKES.CHANCE, DATA_TRAP_SPIKES.DAMAGE, DATA_TRAP_SPIKES.DROP_ACTIVATE, DATA_TRAP_SPIKES.USE_ACTIVATE, DATA_TRAP_SPIKES.IS_ONE_TIME, DATA_TRAP_SPIKES.BREAK_CHANCE, DATA_TRAP_SPIKES.BLOCK_CHANCE, DATA_TRAP_SPIKES.BREAK_CHANCE_ESCAPE, DATA_TRAP_SPIKES.IS_NOISY, DATA_TRAP_SPIKES.NOISE_NAME, DATA_TRAP_SPIKES.IS_FLAMMABLE, DATA_TRAP_SPIKES.FLAVOR);
      this[GameItems.IDs.TRAP_BARBED_WIRE] = new ItemTrapModel(DATA_TRAP_BARBED_WIRE.NAME, DATA_TRAP_BARBED_WIRE.PLURAL, GameImages.ITEM_BARBED_WIRE, DATA_TRAP_BARBED_WIRE.STACKING, DATA_TRAP_BARBED_WIRE.CHANCE, DATA_TRAP_BARBED_WIRE.DAMAGE, DATA_TRAP_BARBED_WIRE.DROP_ACTIVATE, DATA_TRAP_BARBED_WIRE.USE_ACTIVATE, DATA_TRAP_BARBED_WIRE.IS_ONE_TIME, DATA_TRAP_BARBED_WIRE.BREAK_CHANCE, DATA_TRAP_BARBED_WIRE.BLOCK_CHANCE, DATA_TRAP_BARBED_WIRE.BREAK_CHANCE_ESCAPE, DATA_TRAP_BARBED_WIRE.IS_NOISY, DATA_TRAP_BARBED_WIRE.NOISE_NAME, DATA_TRAP_BARBED_WIRE.IS_FLAMMABLE, DATA_TRAP_BARBED_WIRE.FLAVOR);

      // entertainment
      this[GameItems.IDs.ENT_BOOK] = new ItemEntertainmentModel(DATA_ENT_BOOK.NAME, DATA_ENT_BOOK.PLURAL, GameImages.ITEM_BOOK, DATA_ENT_BOOK.VALUE, DATA_ENT_BOOK.BORECHANCE, DATA_ENT_BOOK.STACKING, DATA_ENT_BOOK.FLAVOR);
      this[GameItems.IDs.ENT_MAGAZINE] = new ItemEntertainmentModel(DATA_ENT_MAGAZINE.NAME, DATA_ENT_MAGAZINE.PLURAL, GameImages.ITEM_MAGAZINE, DATA_ENT_MAGAZINE.VALUE, DATA_ENT_MAGAZINE.BORECHANCE, DATA_ENT_MAGAZINE.STACKING, DATA_ENT_MAGAZINE.FLAVOR);

      this[GameItems.IDs.UNIQUE_SUBWAY_BADGE] = new ItemModel("Subway Worker Badge", "Subways Worker Badges", GameImages.ITEM_SUBWAY_BADGE)
      {
        DontAutoEquip = true,
        EquipmentPart = DollPart.LEFT_HAND,
        FlavorDescription = "You got yourself a new job!"
      };
    }

    private void Notify(IRogueUI ui, string what, string stage)
    {
      ui.UI_Clear(Color.Black);
      ui.UI_DrawStringBold(Color.White, "Loading " + what + " data : " + stage, 0, 0, new Color?());
      ui.UI_Repaint();
    }

    private bool LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, GameItems.IDs[] idsToRead, out _DATA_TYPE_[] data)
    {
      Contract.Requires(null!=ui);
      Contract.Requires(!string.IsNullOrEmpty(path));
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

    public bool LoadMedicineFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.MedecineData[] data;
      LoadDataFromCSV(ui, path, "medicine items", 10, new Func<CSVLine, GameItems.MedecineData>(GameItems.MedecineData.FromCSVLine), new GameItems.IDs[6]
      {
        GameItems.IDs._FIRST,
        GameItems.IDs.MEDICINE_MEDIKIT,
        GameItems.IDs.MEDICINE_PILLS_SLP,
        GameItems.IDs.MEDICINE_PILLS_STA,
        GameItems.IDs.MEDICINE_PILLS_SAN,
        GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL
      }, out data);
      DATA_MEDICINE_BANDAGE = data[0];
      DATA_MEDICINE_MEDIKIT = data[1];
      DATA_MEDICINE_PILLS_SLP = data[2];
      DATA_MEDICINE_PILLS_STA = data[3];
      DATA_MEDICINE_PILLS_SAN = data[4];
      DATA_MEDICINE_PILLS_ANTIVIRAL = data[5];
      return true;
    }

    public bool LoadFoodFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.FoodData[] data;
      LoadDataFromCSV(ui, path, "food items", 7, new Func<CSVLine, GameItems.FoodData>(GameItems.FoodData.FromCSVLine), new GameItems.IDs[3]
      {
        GameItems.IDs.FOOD_ARMY_RATION,
        GameItems.IDs.FOOD_CANNED_FOOD,
        GameItems.IDs.FOOD_GROCERIES
      }, out data);
      DATA_FOOD_ARMY_RATION = data[0];
      DATA_FOOD_CANNED_FOOD = data[1];
      DATA_FOOD_GROCERIES = data[2];
      return true;
    }

    public bool LoadMeleeWeaponsFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.MeleeWeaponData[] data;
      LoadDataFromCSV(ui, path, "melee weapons items", 9, new Func<CSVLine, GameItems.MeleeWeaponData>(GameItems.MeleeWeaponData.FromCSVLine), new GameItems.IDs[16]
      {
        GameItems.IDs.MELEE_BASEBALLBAT,
        GameItems.IDs.MELEE_COMBAT_KNIFE,
        GameItems.IDs.MELEE_CROWBAR,
        GameItems.IDs.MELEE_GOLFCLUB,
        GameItems.IDs.MELEE_HUGE_HAMMER,
        GameItems.IDs.MELEE_IRON_GOLFCLUB,
        GameItems.IDs.MELEE_SHOVEL,
        GameItems.IDs.MELEE_SHORT_SHOVEL,
        GameItems.IDs.MELEE_TRUNCHEON,
        GameItems.IDs.UNIQUE_JASON_MYERS_AXE,
        GameItems.IDs.MELEE_IMPROVISED_CLUB,
        GameItems.IDs.MELEE_IMPROVISED_SPEAR,
        GameItems.IDs.MELEE_SMALL_HAMMER,
        GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA,
        GameItems.IDs.UNIQUE_BIGBEAR_BAT,
        GameItems.IDs.UNIQUE_ROGUEDJACK_KEYBOARD
      }, out data);
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

    public bool LoadRangedWeaponsFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.RangedWeaponData[] data;
      LoadDataFromCSV(ui, path, "ranged weapons items", 8, new Func<CSVLine, GameItems.RangedWeaponData>(GameItems.RangedWeaponData.FromCSVLine), new GameItems.IDs[10]
      {
        GameItems.IDs.RANGED_ARMY_PISTOL,
        GameItems.IDs.RANGED_ARMY_RIFLE,
        GameItems.IDs.RANGED_HUNTING_CROSSBOW,
        GameItems.IDs.RANGED_HUNTING_RIFLE,
        GameItems.IDs.RANGED_KOLT_REVOLVER,
        GameItems.IDs.RANGED_PISTOL,
        GameItems.IDs.RANGED_PRECISION_RIFLE,
        GameItems.IDs.RANGED_SHOTGUN,
        GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN,
        GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL
      }, out data);
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

    public bool LoadExplosivesFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.ExplosiveData[] data;
      LoadDataFromCSV(ui, path, "explosives items", 14, new Func<CSVLine, GameItems.ExplosiveData>(GameItems.ExplosiveData.FromCSVLine), new GameItems.IDs[1]
      {
        GameItems.IDs.EXPLOSIVE_GRENADE
      }, out data);
      DATA_EXPLOSIVE_GRENADE = data[0];
      return true;
    }

    public bool LoadBarricadingMaterialFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.BarricadingMaterialData[] data;
      LoadDataFromCSV(ui, path, "barricading items", 6, new Func<CSVLine, GameItems.BarricadingMaterialData>(GameItems.BarricadingMaterialData.FromCSVLine), new GameItems.IDs[1]
      {
        GameItems.IDs.BAR_WOODEN_PLANK
      }, out data);
      DATA_BAR_WOODEN_PLANK = data[0];
      return true;
    }

    public bool LoadArmorsFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.ArmorData[] data;
      LoadDataFromCSV(ui, path, "armors items", 8, new Func<CSVLine, GameItems.ArmorData>(GameItems.ArmorData.FromCSVLine), new GameItems.IDs[7]
      {
        GameItems.IDs.ARMOR_ARMY_BODYARMOR,
        GameItems.IDs.ARMOR_CHAR_LIGHT_BODYARMOR,
        GameItems.IDs.ARMOR_HELLS_SOULS_JACKET,
        GameItems.IDs.ARMOR_FREE_ANGELS_JACKET,
        GameItems.IDs.ARMOR_POLICE_JACKET,
        GameItems.IDs.ARMOR_POLICE_RIOT,
        GameItems.IDs.ARMOR_HUNTER_VEST
      }, out data);
      DATA_ARMOR_ARMY = data[0];
      DATA_ARMOR_CHAR = data[1];
      DATA_ARMOR_HELLS_SOULS_JACKET = data[2];
      DATA_ARMOR_FREE_ANGELS_JACKET = data[3];
      DATA_ARMOR_POLICE_JACKET = data[4];
      DATA_ARMOR_POLICE_RIOT = data[5];
      DATA_ARMOR_HUNTER_VEST = data[6];
      return true;
    }

    public bool LoadTrackersFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.TrackerData[] data;
      LoadDataFromCSV(ui, path, "trackers items", 5, new Func<CSVLine, GameItems.TrackerData>(GameItems.TrackerData.FromCSVLine), new GameItems.IDs[4]
      {
        GameItems.IDs.TRACKER_BLACKOPS,
        GameItems.IDs.TRACKER_CELL_PHONE,
        GameItems.IDs.TRACKER_ZTRACKER,
        GameItems.IDs.TRACKER_POLICE_RADIO
      }, out data);
      DATA_TRACKER_BLACKOPS_GPS = data[0];
      DATA_TRACKER_CELL_PHONE = data[1];
      DATA_TRACKER_ZTRACKER = data[2];
      DATA_TRACKER_POLICE_RADIO = data[3];
      return true;
    }

    public bool LoadSpraypaintsFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.SprayPaintData[] data;
      LoadDataFromCSV(ui, path, "spraypaint items", 5, new Func<CSVLine, GameItems.SprayPaintData>(GameItems.SprayPaintData.FromCSVLine), new GameItems.IDs[4]
      {
        GameItems.IDs.SPRAY_PAINT1,
        GameItems.IDs.SPRAY_PAINT2,
        GameItems.IDs.SPRAY_PAINT3,
        GameItems.IDs.SPRAY_PAINT4
      }, out data);
      DATA_SPRAY_PAINT1 = data[0];
      DATA_SPRAY_PAINT2 = data[1];
      DATA_SPRAY_PAINT3 = data[2];
      DATA_SPRAY_PAINT4 = data[3];
      return true;
    }

    public bool LoadLightsFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.LightData[] data;
      LoadDataFromCSV(ui, path, "lights items", 6, new Func<CSVLine, GameItems.LightData>(GameItems.LightData.FromCSVLine), new GameItems.IDs[2]
      {
        GameItems.IDs.LIGHT_FLASHLIGHT,
        GameItems.IDs.LIGHT_BIG_FLASHLIGHT
      }, out data);
      DATA_LIGHT_FLASHLIGHT = data[0];
      DATA_LIGHT_BIG_FLASHLIGHT = data[1];
      return true;
    }

    public bool LoadScentspraysFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.ScentSprayData[] data;
      LoadDataFromCSV(ui, path, "scentsprays items", 6, new Func<CSVLine, GameItems.ScentSprayData>(GameItems.ScentSprayData.FromCSVLine), new GameItems.IDs[1]
      {
        GameItems.IDs.SCENT_SPRAY_STENCH_KILLER
      }, out data);
      DATA_SCENT_SPRAY_STENCH_KILLER = data[0];
      return true;
    }

    public bool LoadTrapsFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.TrapData[] data;
      LoadDataFromCSV(ui, path, "traps items", 16, new Func<CSVLine, GameItems.TrapData>(GameItems.TrapData.FromCSVLine), new GameItems.IDs[4]
      {
        GameItems.IDs.TRAP_EMPTY_CAN,
        GameItems.IDs.TRAP_BEAR_TRAP,
        GameItems.IDs.TRAP_SPIKES,
        GameItems.IDs.TRAP_BARBED_WIRE
      }, out data);
      DATA_TRAP_EMPTY_CAN = data[0];
      DATA_TRAP_BEAR_TRAP = data[1];
      DATA_TRAP_SPIKES = data[2];
      DATA_TRAP_BARBED_WIRE = data[3];
      return true;
    }

    public bool LoadEntertainmentFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(!string.IsNullOrEmpty(path));
      GameItems.EntData[] data;
      LoadDataFromCSV(ui, path, "entertainment items", 7, new Func<CSVLine, GameItems.EntData>(GameItems.EntData.FromCSVLine), new GameItems.IDs[2]
      {
        GameItems.IDs.ENT_BOOK,
        GameItems.IDs.ENT_MAGAZINE
      }, out data);
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
      AMMO_LIGHT_PISTOL = 51,
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int STACKINGLIMIT { get; set; }

      public int HEALING { get; set; }

      public int STAMINABOOST { get; set; }

      public int SLEEPBOOST { get; set; }

      public int INFECTIONCURE { get; set; }

      public int SANITYCURE { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.MedecineData FromCSVLine(CSVLine line)
      {
        return new GameItems.MedecineData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int NUTRITION { get; set; }

      public int BESTBEFORE { get; set; }

      public int STACKINGLIMIT { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.FoodData FromCSVLine(CSVLine line)
      {
        return new GameItems.FoodData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int ATK { get; set; }

      public int DMG { get; set; }

      public int STA { get; set; }

      public int STACKINGLIMIT { get; set; }

      public bool ISFRAGILE { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.MeleeWeaponData FromCSVLine(CSVLine line)
      {
        return new GameItems.MeleeWeaponData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int ATK { get; set; }

      public int DMG { get; set; }

      public int RANGE { get; set; }

      public int MAXAMMO { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.RangedWeaponData FromCSVLine(CSVLine line)
      {
        return new GameItems.RangedWeaponData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int FUSE { get; set; }

      public int MAXTHROW { get; set; }

      public int STACKLINGLIMIT { get; set; }

      public int RADIUS { get; set; }

      public int[] DMG { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.ExplosiveData FromCSVLine(CSVLine line)
      {
        return new GameItems.ExplosiveData()
        {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          FUSE = line[3].ParseInt(),
          MAXTHROW = line[4].ParseInt(),
          STACKLINGLIMIT = line[5].ParseInt(),
          RADIUS = line[6].ParseInt(),
          DMG = new int[6]
          {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int VALUE { get; set; }

      public int STACKINGLIMIT { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.BarricadingMaterialData FromCSVLine(CSVLine line)
      {
        return new GameItems.BarricadingMaterialData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int PRO_HIT { get; set; }

      public int PRO_SHOT { get; set; }

      public int ENC { get; set; }

      public int WEIGHT { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.ArmorData FromCSVLine(CSVLine line)
      {
        return new GameItems.ArmorData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int BATTERIES { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.TrackerData FromCSVLine(CSVLine line)
      {
        return new GameItems.TrackerData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int QUANTITY { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.SprayPaintData FromCSVLine(CSVLine line)
      {
        return new GameItems.SprayPaintData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int FOV { get; set; }

      public int BATTERIES { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.LightData FromCSVLine(CSVLine line)
      {
        return new GameItems.LightData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int QUANTITY { get; set; }

      public int STRENGTH { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.ScentSprayData FromCSVLine(CSVLine line)
      {
        return new GameItems.ScentSprayData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int STACKING { get; set; }

      public bool USE_ACTIVATE { get; set; }

      public int CHANCE { get; set; }

      public int DAMAGE { get; set; }

      public bool DROP_ACTIVATE { get; set; }

      public bool IS_ONE_TIME { get; set; }

      public int BREAK_CHANCE { get; set; }

      public int BLOCK_CHANCE { get; set; }

      public int BREAK_CHANCE_ESCAPE { get; set; }

      public bool IS_NOISY { get; set; }

      public string NOISE_NAME { get; set; }

      public bool IS_FLAMMABLE { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.TrapData FromCSVLine(CSVLine line)
      {
        return new GameItems.TrapData()
        {
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

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int STACKING { get; set; }

      public int VALUE { get; set; }

      public int BORECHANCE { get; set; }

      public string FLAVOR { get; set; }

      public static GameItems.EntData FromCSVLine(CSVLine line)
      {
        return new GameItems.EntData()
        {
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
