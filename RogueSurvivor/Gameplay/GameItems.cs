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
    private ItemModel[] m_Models = new ItemModel[69];
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

    public override ItemModel this[int id]
    {
      get
      {
        return this.m_Models[id];
      }
    }

    public ItemModel this[GameItems.IDs id]
    {
      get
      {
        return this[(int) id];
      }
      private set
      {
        this.m_Models[(int) id] = value;
        this.m_Models[(int) id].ID = (int) id;
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

    private bool CheckPlural(string name, string plural)
    {
      return name == plural;
    }

    public void CreateModels()
    {
      ItemMedicineModel itemMedicineModel1 = new ItemMedicineModel(this.DATA_MEDICINE_BANDAGE.NAME, this.DATA_MEDICINE_BANDAGE.PLURAL, "Items\\item_bandages", this.DATA_MEDICINE_BANDAGE.HEALING, this.DATA_MEDICINE_BANDAGE.STAMINABOOST, this.DATA_MEDICINE_BANDAGE.SLEEPBOOST, this.DATA_MEDICINE_BANDAGE.INFECTIONCURE, this.DATA_MEDICINE_BANDAGE.SANITYCURE);
      itemMedicineModel1.IsPlural = true;
      itemMedicineModel1.StackingLimit = this.DATA_MEDICINE_BANDAGE.STACKINGLIMIT;
      itemMedicineModel1.FlavorDescription = this.DATA_MEDICINE_BANDAGE.FLAVOR;
      this[GameItems.IDs.MEDICINE_BANDAGES] = (ItemModel) itemMedicineModel1;
      ItemMedicineModel itemMedicineModel3 = new ItemMedicineModel(this.DATA_MEDICINE_MEDIKIT.NAME, this.DATA_MEDICINE_MEDIKIT.PLURAL, "Items\\item_medikit", this.DATA_MEDICINE_MEDIKIT.HEALING, this.DATA_MEDICINE_MEDIKIT.STAMINABOOST, this.DATA_MEDICINE_MEDIKIT.SLEEPBOOST, this.DATA_MEDICINE_MEDIKIT.INFECTIONCURE, this.DATA_MEDICINE_MEDIKIT.SANITYCURE);
      itemMedicineModel3.FlavorDescription = this.DATA_MEDICINE_MEDIKIT.FLAVOR;
      this[GameItems.IDs.MEDICINE_MEDIKIT] = (ItemModel) itemMedicineModel3;
      ItemMedicineModel itemMedicineModel5 = new ItemMedicineModel(this.DATA_MEDICINE_PILLS_STA.NAME, this.DATA_MEDICINE_PILLS_STA.PLURAL, "Items\\item_pills_green", this.DATA_MEDICINE_PILLS_STA.HEALING, this.DATA_MEDICINE_PILLS_STA.STAMINABOOST, this.DATA_MEDICINE_PILLS_STA.SLEEPBOOST, this.DATA_MEDICINE_PILLS_STA.INFECTIONCURE, this.DATA_MEDICINE_PILLS_STA.SANITYCURE);
      itemMedicineModel5.IsPlural = true;
      itemMedicineModel5.StackingLimit = this.DATA_MEDICINE_PILLS_STA.STACKINGLIMIT;
      itemMedicineModel5.FlavorDescription = this.DATA_MEDICINE_PILLS_STA.FLAVOR;
      this[GameItems.IDs.MEDICINE_PILLS_STA] = (ItemModel) itemMedicineModel5;
      ItemMedicineModel itemMedicineModel7 = new ItemMedicineModel(this.DATA_MEDICINE_PILLS_SLP.NAME, this.DATA_MEDICINE_PILLS_SLP.PLURAL, "Items\\item_pills_blue", this.DATA_MEDICINE_PILLS_SLP.HEALING, this.DATA_MEDICINE_PILLS_SLP.STAMINABOOST, this.DATA_MEDICINE_PILLS_SLP.SLEEPBOOST, this.DATA_MEDICINE_PILLS_SLP.INFECTIONCURE, this.DATA_MEDICINE_PILLS_SLP.SANITYCURE);
      itemMedicineModel7.IsPlural = true;
      itemMedicineModel7.StackingLimit = this.DATA_MEDICINE_PILLS_SLP.STACKINGLIMIT;
      itemMedicineModel7.FlavorDescription = this.DATA_MEDICINE_PILLS_SLP.FLAVOR;
      this[GameItems.IDs.MEDICINE_PILLS_SLP] = (ItemModel) itemMedicineModel7;
      ItemMedicineModel itemMedicineModel9 = new ItemMedicineModel(this.DATA_MEDICINE_PILLS_SAN.NAME, this.DATA_MEDICINE_PILLS_SAN.PLURAL, "Items\\item_pills_san", this.DATA_MEDICINE_PILLS_SAN.HEALING, this.DATA_MEDICINE_PILLS_SAN.STAMINABOOST, this.DATA_MEDICINE_PILLS_SAN.SLEEPBOOST, this.DATA_MEDICINE_PILLS_SAN.INFECTIONCURE, this.DATA_MEDICINE_PILLS_SAN.SANITYCURE);
      itemMedicineModel9.IsPlural = true;
      itemMedicineModel9.StackingLimit = this.DATA_MEDICINE_PILLS_SAN.STACKINGLIMIT;
      itemMedicineModel9.FlavorDescription = this.DATA_MEDICINE_PILLS_SAN.FLAVOR;
      this[GameItems.IDs.MEDICINE_PILLS_SAN] = (ItemModel) itemMedicineModel9;
      ItemMedicineModel itemMedicineModel11 = new ItemMedicineModel(this.DATA_MEDICINE_PILLS_ANTIVIRAL.NAME, this.DATA_MEDICINE_PILLS_ANTIVIRAL.PLURAL, "Items\\item_pills_antiviral", this.DATA_MEDICINE_PILLS_ANTIVIRAL.HEALING, this.DATA_MEDICINE_PILLS_ANTIVIRAL.STAMINABOOST, this.DATA_MEDICINE_PILLS_ANTIVIRAL.SLEEPBOOST, this.DATA_MEDICINE_PILLS_ANTIVIRAL.INFECTIONCURE, this.DATA_MEDICINE_PILLS_ANTIVIRAL.SANITYCURE);
      itemMedicineModel11.IsPlural = true;
      itemMedicineModel11.StackingLimit = this.DATA_MEDICINE_PILLS_ANTIVIRAL.STACKINGLIMIT;
      itemMedicineModel11.FlavorDescription = this.DATA_MEDICINE_PILLS_ANTIVIRAL.FLAVOR;
      this[GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL] = (ItemModel) itemMedicineModel11;

      // Food
      ItemFoodModel itemFoodModel1 = new ItemFoodModel(this.DATA_FOOD_ARMY_RATION.NAME, this.DATA_FOOD_ARMY_RATION.PLURAL, "Items\\item_army_ration", this.DATA_FOOD_ARMY_RATION.NUTRITION, this.DATA_FOOD_ARMY_RATION.BESTBEFORE);
      itemFoodModel1.IsPlural = this.CheckPlural(this.DATA_FOOD_ARMY_RATION.NAME, this.DATA_FOOD_ARMY_RATION.PLURAL);
      itemFoodModel1.StackingLimit = this.DATA_FOOD_ARMY_RATION.STACKINGLIMIT;
      itemFoodModel1.FlavorDescription = this.DATA_FOOD_ARMY_RATION.FLAVOR;
      this[GameItems.IDs.FOOD_ARMY_RATION] = (ItemModel) itemFoodModel1;
      ItemFoodModel itemFoodModel3 = new ItemFoodModel(this.DATA_FOOD_GROCERIES.NAME, this.DATA_FOOD_GROCERIES.PLURAL, "Items\\item_groceries", this.DATA_FOOD_GROCERIES.NUTRITION, this.DATA_FOOD_GROCERIES.BESTBEFORE);
      itemFoodModel3.IsPlural = this.CheckPlural(this.DATA_FOOD_GROCERIES.NAME, this.DATA_FOOD_GROCERIES.PLURAL);
      itemFoodModel3.StackingLimit = this.DATA_FOOD_GROCERIES.STACKINGLIMIT;
      itemFoodModel3.FlavorDescription = this.DATA_FOOD_GROCERIES.FLAVOR;
      this[GameItems.IDs.FOOD_GROCERIES] = (ItemModel) itemFoodModel3;
      ItemFoodModel itemFoodModel5 = new ItemFoodModel(this.DATA_FOOD_CANNED_FOOD.NAME, this.DATA_FOOD_CANNED_FOOD.PLURAL, "Items\\item_canned_food", this.DATA_FOOD_CANNED_FOOD.NUTRITION, this.DATA_FOOD_CANNED_FOOD.BESTBEFORE);
      itemFoodModel5.IsPlural = this.CheckPlural(this.DATA_FOOD_CANNED_FOOD.NAME, this.DATA_FOOD_CANNED_FOOD.PLURAL);
      itemFoodModel5.StackingLimit = this.DATA_FOOD_CANNED_FOOD.STACKINGLIMIT;
      itemFoodModel5.FlavorDescription = this.DATA_FOOD_CANNED_FOOD.FLAVOR;
      this[GameItems.IDs.FOOD_CANNED_FOOD] = (ItemModel) itemFoodModel5;

      // melee weapons
      ItemMeleeWeaponModel meleeWeaponModel1 = new ItemMeleeWeaponModel(this.DATA_MELEE_BASEBALLBAT.NAME, this.DATA_MELEE_BASEBALLBAT.PLURAL, "Items\\item_baseballbat", new Attack(AttackKind.PHYSICAL, new Verb("smash", "smashes"), this.DATA_MELEE_BASEBALLBAT.ATK, this.DATA_MELEE_BASEBALLBAT.DMG, this.DATA_MELEE_BASEBALLBAT.STA));
      meleeWeaponModel1.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel1.StackingLimit = this.DATA_MELEE_BASEBALLBAT.STACKINGLIMIT;
      meleeWeaponModel1.FlavorDescription = this.DATA_MELEE_BASEBALLBAT.FLAVOR;
      meleeWeaponModel1.IsFragile = this.DATA_MELEE_BASEBALLBAT.ISFRAGILE;
      this[GameItems.IDs.MELEE_BASEBALLBAT] = (ItemModel) meleeWeaponModel1;
      ItemMeleeWeaponModel meleeWeaponModel3 = new ItemMeleeWeaponModel(this.DATA_MELEE_COMBAT_KNIFE.NAME, this.DATA_MELEE_COMBAT_KNIFE.PLURAL, "Items\\item_combat_knife", new Attack(AttackKind.PHYSICAL, new Verb("stab", "stabs"), this.DATA_MELEE_COMBAT_KNIFE.ATK, this.DATA_MELEE_COMBAT_KNIFE.DMG, this.DATA_MELEE_COMBAT_KNIFE.STA));
      meleeWeaponModel3.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel3.StackingLimit = this.DATA_MELEE_COMBAT_KNIFE.STACKINGLIMIT;
      meleeWeaponModel3.FlavorDescription = this.DATA_MELEE_COMBAT_KNIFE.FLAVOR;
      meleeWeaponModel3.IsFragile = this.DATA_MELEE_COMBAT_KNIFE.ISFRAGILE;
      this[GameItems.IDs.MELEE_COMBAT_KNIFE] = (ItemModel) meleeWeaponModel3;
      ItemMeleeWeaponModel meleeWeaponModel5 = new ItemMeleeWeaponModel(this.DATA_MELEE_CROWBAR.NAME, this.DATA_MELEE_CROWBAR.PLURAL, "Items\\item_crowbar", new Attack(AttackKind.PHYSICAL, new Verb("strike"), this.DATA_MELEE_CROWBAR.ATK, this.DATA_MELEE_CROWBAR.DMG, this.DATA_MELEE_CROWBAR.STA));
      meleeWeaponModel5.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel5.StackingLimit = this.DATA_MELEE_CROWBAR.STACKINGLIMIT;
      meleeWeaponModel5.FlavorDescription = this.DATA_MELEE_CROWBAR.FLAVOR;
      meleeWeaponModel5.IsFragile = this.DATA_MELEE_CROWBAR.ISFRAGILE;
      this[GameItems.IDs.MELEE_CROWBAR] = (ItemModel) meleeWeaponModel5;
      ItemMeleeWeaponModel meleeWeaponModel7 = new ItemMeleeWeaponModel(this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE.NAME, this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE.PLURAL, "Items\\item_jason_myers_axe", new Attack(AttackKind.PHYSICAL, new Verb("slash", "slashes"), this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE.ATK, this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE.DMG, this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE.STA));
      meleeWeaponModel7.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel7.IsProper = true;
      meleeWeaponModel7.FlavorDescription = this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE.FLAVOR;
      meleeWeaponModel7.IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_JASON_MYERS_AXE] = (ItemModel) meleeWeaponModel7;
      ItemMeleeWeaponModel meleeWeaponModel9 = new ItemMeleeWeaponModel(this.DATA_MELEE_GOLFCLUB.NAME, this.DATA_MELEE_GOLFCLUB.PLURAL, "Items\\item_golfclub", new Attack(AttackKind.PHYSICAL, new Verb("strike"), this.DATA_MELEE_GOLFCLUB.ATK, this.DATA_MELEE_GOLFCLUB.DMG, this.DATA_MELEE_GOLFCLUB.STA));
      meleeWeaponModel9.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel9.StackingLimit = this.DATA_MELEE_GOLFCLUB.STACKINGLIMIT;
      meleeWeaponModel9.FlavorDescription = this.DATA_MELEE_GOLFCLUB.FLAVOR;
      meleeWeaponModel9.IsFragile = this.DATA_MELEE_GOLFCLUB.ISFRAGILE;
      this[GameItems.IDs.MELEE_GOLFCLUB] = (ItemModel) meleeWeaponModel9;
      ItemMeleeWeaponModel meleeWeaponModel11 = new ItemMeleeWeaponModel(this.DATA_MELEE_IRON_GOLFCLUB.NAME, this.DATA_MELEE_IRON_GOLFCLUB.PLURAL, "Items\\item_iron_golfclub", new Attack(AttackKind.PHYSICAL, new Verb("strike"), this.DATA_MELEE_IRON_GOLFCLUB.ATK, this.DATA_MELEE_IRON_GOLFCLUB.DMG, this.DATA_MELEE_IRON_GOLFCLUB.STA));
      meleeWeaponModel11.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel11.StackingLimit = this.DATA_MELEE_IRON_GOLFCLUB.STACKINGLIMIT;
      meleeWeaponModel11.FlavorDescription = this.DATA_MELEE_IRON_GOLFCLUB.FLAVOR;
      meleeWeaponModel11.IsFragile = this.DATA_MELEE_IRON_GOLFCLUB.ISFRAGILE;
      this[GameItems.IDs.MELEE_IRON_GOLFCLUB] = (ItemModel) meleeWeaponModel11;
      ItemMeleeWeaponModel meleeWeaponModel13 = new ItemMeleeWeaponModel(this.DATA_MELEE_HUGE_HAMMER.NAME, this.DATA_MELEE_HUGE_HAMMER.PLURAL, "Items\\item_huge_hammer", new Attack(AttackKind.PHYSICAL, new Verb("smash", "smashes"), this.DATA_MELEE_HUGE_HAMMER.ATK, this.DATA_MELEE_HUGE_HAMMER.DMG, this.DATA_MELEE_HUGE_HAMMER.STA));
      meleeWeaponModel13.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel13.StackingLimit = this.DATA_MELEE_HUGE_HAMMER.STACKINGLIMIT;
      meleeWeaponModel13.FlavorDescription = this.DATA_MELEE_HUGE_HAMMER.FLAVOR;
      meleeWeaponModel13.IsFragile = this.DATA_MELEE_HUGE_HAMMER.ISFRAGILE;
      this[GameItems.IDs.MELEE_HUGE_HAMMER] = (ItemModel) meleeWeaponModel13;
      ItemMeleeWeaponModel meleeWeaponModel15 = new ItemMeleeWeaponModel(this.DATA_MELEE_SHOVEL.NAME, this.DATA_MELEE_SHOVEL.PLURAL, "Items\\item_shovel", new Attack(AttackKind.PHYSICAL, new Verb("strike"), this.DATA_MELEE_SHOVEL.ATK, this.DATA_MELEE_SHOVEL.DMG, this.DATA_MELEE_SHOVEL.STA));
      meleeWeaponModel15.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel15.StackingLimit = this.DATA_MELEE_SHOVEL.STACKINGLIMIT;
      meleeWeaponModel15.FlavorDescription = this.DATA_MELEE_SHOVEL.FLAVOR;
      meleeWeaponModel15.IsFragile = this.DATA_MELEE_SHOVEL.ISFRAGILE;
      this[GameItems.IDs.MELEE_SHOVEL] = (ItemModel) meleeWeaponModel15;
      ItemMeleeWeaponModel meleeWeaponModel17 = new ItemMeleeWeaponModel(this.DATA_MELEE_SHORT_SHOVEL.NAME, this.DATA_MELEE_SHORT_SHOVEL.PLURAL, "Items\\item_short_shovel", new Attack(AttackKind.PHYSICAL, new Verb("strike"), this.DATA_MELEE_SHORT_SHOVEL.ATK, this.DATA_MELEE_SHORT_SHOVEL.DMG, this.DATA_MELEE_SHORT_SHOVEL.STA));
      meleeWeaponModel17.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel17.StackingLimit = this.DATA_MELEE_SHORT_SHOVEL.STACKINGLIMIT;
      meleeWeaponModel17.FlavorDescription = this.DATA_MELEE_SHORT_SHOVEL.FLAVOR;
      meleeWeaponModel17.IsFragile = this.DATA_MELEE_SHORT_SHOVEL.ISFRAGILE;
      this[GameItems.IDs.MELEE_SHORT_SHOVEL] = (ItemModel) meleeWeaponModel17;
      ItemMeleeWeaponModel meleeWeaponModel19 = new ItemMeleeWeaponModel(this.DATA_MELEE_TRUNCHEON.NAME, this.DATA_MELEE_TRUNCHEON.PLURAL, "Items\\item_truncheon", new Attack(AttackKind.PHYSICAL, new Verb("strike"), this.DATA_MELEE_TRUNCHEON.ATK, this.DATA_MELEE_TRUNCHEON.DMG, this.DATA_MELEE_TRUNCHEON.STA));
      meleeWeaponModel19.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel19.StackingLimit = this.DATA_MELEE_TRUNCHEON.STACKINGLIMIT;
      meleeWeaponModel19.FlavorDescription = this.DATA_MELEE_TRUNCHEON.FLAVOR;
      meleeWeaponModel19.IsFragile = this.DATA_MELEE_TRUNCHEON.ISFRAGILE;
      this[GameItems.IDs.MELEE_TRUNCHEON] = (ItemModel)meleeWeaponModel19;
      GameItems.MeleeWeaponData meleeWeaponData1 = this.DATA_MELEE_IMPROVISED_CLUB;
      ItemMeleeWeaponModel meleeWeaponModel21 = new ItemMeleeWeaponModel(meleeWeaponData1.NAME, meleeWeaponData1.PLURAL, "Items\\item_improvised_club", new Attack(AttackKind.PHYSICAL, new Verb("strike"), meleeWeaponData1.ATK, meleeWeaponData1.DMG, meleeWeaponData1.STA));
      meleeWeaponModel21.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel21.StackingLimit = meleeWeaponData1.STACKINGLIMIT;
      meleeWeaponModel21.FlavorDescription = meleeWeaponData1.FLAVOR;
      meleeWeaponModel21.IsFragile = meleeWeaponData1.ISFRAGILE;
      this[GameItems.IDs.MELEE_IMPROVISED_CLUB] = (ItemModel) meleeWeaponModel21;
      GameItems.MeleeWeaponData meleeWeaponData2 = this.DATA_MELEE_IMPROVISED_SPEAR;
      ItemMeleeWeaponModel meleeWeaponModel23 = new ItemMeleeWeaponModel(meleeWeaponData2.NAME, meleeWeaponData2.PLURAL, "Items\\item_improvised_spear", new Attack(AttackKind.PHYSICAL, new Verb("pierce"), meleeWeaponData2.ATK, meleeWeaponData2.DMG, meleeWeaponData2.STA));
      meleeWeaponModel23.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel23.StackingLimit = meleeWeaponData2.STACKINGLIMIT;
      meleeWeaponModel23.FlavorDescription = meleeWeaponData2.FLAVOR;
      meleeWeaponModel23.IsFragile = meleeWeaponData2.ISFRAGILE;
      this[GameItems.IDs.MELEE_IMPROVISED_SPEAR] = (ItemModel) meleeWeaponModel23;
      GameItems.MeleeWeaponData meleeWeaponData3 = this.DATA_MELEE_SMALL_HAMMER;
      ItemMeleeWeaponModel meleeWeaponModel25 = new ItemMeleeWeaponModel(meleeWeaponData3.NAME, meleeWeaponData3.PLURAL, "Items\\item_small_hammer", new Attack(AttackKind.PHYSICAL, new Verb("smash"), meleeWeaponData3.ATK, meleeWeaponData3.DMG, meleeWeaponData3.STA));
      meleeWeaponModel25.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel25.StackingLimit = meleeWeaponData3.STACKINGLIMIT;
      meleeWeaponModel25.FlavorDescription = meleeWeaponData3.FLAVOR;
      meleeWeaponModel25.IsFragile = meleeWeaponData3.ISFRAGILE;
      this[GameItems.IDs.MELEE_SMALL_HAMMER] = (ItemModel) meleeWeaponModel25;
      GameItems.MeleeWeaponData meleeWeaponData4 = this.DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA;
      ItemMeleeWeaponModel meleeWeaponModel27 = new ItemMeleeWeaponModel(meleeWeaponData4.NAME, meleeWeaponData4.PLURAL, "Items\\item_famu_fataru_katana", new Attack(AttackKind.PHYSICAL, new Verb("slash", "slashes"), meleeWeaponData4.ATK, meleeWeaponData4.DMG, meleeWeaponData4.STA));
      meleeWeaponModel27.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel27.FlavorDescription = meleeWeaponData4.FLAVOR;
      meleeWeaponModel27.IsProper = true;
      meleeWeaponModel27.IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_FAMU_FATARU_KATANA] = (ItemModel) meleeWeaponModel27;
      GameItems.MeleeWeaponData meleeWeaponData5 = this.DATA_MELEE_UNIQUE_BIGBEAR_BAT;
      ItemMeleeWeaponModel meleeWeaponModel29 = new ItemMeleeWeaponModel(meleeWeaponData5.NAME, meleeWeaponData5.PLURAL, "Items\\item_bigbear_bat", new Attack(AttackKind.PHYSICAL, new Verb("smash", "smashes"), meleeWeaponData5.ATK, meleeWeaponData5.DMG, meleeWeaponData5.STA));
      meleeWeaponModel29.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel29.FlavorDescription = meleeWeaponData5.FLAVOR;
      meleeWeaponModel29.IsProper = true;
      meleeWeaponModel29.IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_BIGBEAR_BAT] = (ItemModel) meleeWeaponModel29;
      GameItems.MeleeWeaponData meleeWeaponData6 = this.DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD;
      ItemMeleeWeaponModel meleeWeaponModel31 = new ItemMeleeWeaponModel(meleeWeaponData6.NAME, meleeWeaponData6.PLURAL, "Items\\item_roguedjack_keyboard", new Attack(AttackKind.PHYSICAL, new Verb("bash", "bashes"), meleeWeaponData6.ATK, meleeWeaponData6.DMG, meleeWeaponData6.STA));
      meleeWeaponModel31.EquipmentPart = DollPart._FIRST;
      meleeWeaponModel31.FlavorDescription = meleeWeaponData6.FLAVOR;
      meleeWeaponModel31.IsProper = true;
      meleeWeaponModel31.IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_ROGUEDJACK_KEYBOARD] = (ItemModel) meleeWeaponModel31;

      // ranged weapons
      GameItems.RangedWeaponData rangedWeaponData1 = this.DATA_RANGED_ARMY_PISTOL;
      ItemRangedWeaponModel rangedWeaponModel1 = new ItemRangedWeaponModel(rangedWeaponData1.NAME, rangedWeaponData1.FLAVOR, "Items\\item_army_pistol", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData1.ATK, rangedWeaponData1.DMG, 0, rangedWeaponData1.RANGE), rangedWeaponData1.MAXAMMO, AmmoType.HEAVY_PISTOL);
      rangedWeaponModel1.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel1.FlavorDescription = rangedWeaponData1.FLAVOR;
      this[GameItems.IDs.RANGED_ARMY_PISTOL] = (ItemModel) rangedWeaponModel1;
      GameItems.RangedWeaponData rangedWeaponData2 = this.DATA_RANGED_ARMY_RIFLE;
      ItemRangedWeaponModel rangedWeaponModel3 = new ItemRangedWeaponModel(rangedWeaponData2.NAME, rangedWeaponData2.FLAVOR, "Items\\item_army_rifle", new Attack(AttackKind.FIREARM, new Verb("fire a salvo at", "fires a salvo at"), rangedWeaponData2.ATK, rangedWeaponData2.DMG, 0, rangedWeaponData2.RANGE), rangedWeaponData2.MAXAMMO, AmmoType.HEAVY_RIFLE);
      rangedWeaponModel3.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel3.FlavorDescription = rangedWeaponData2.FLAVOR;
      this[GameItems.IDs.RANGED_ARMY_RIFLE] = (ItemModel) rangedWeaponModel3;
      GameItems.RangedWeaponData rangedWeaponData3 = this.DATA_RANGED_HUNTING_CROSSBOW;
      ItemRangedWeaponModel rangedWeaponModel5 = new ItemRangedWeaponModel(rangedWeaponData3.NAME, rangedWeaponData3.FLAVOR, "Items\\item_hunting_crossbow", new Attack(AttackKind.BOW, new Verb("shoot"), rangedWeaponData3.ATK, rangedWeaponData3.DMG, 0, rangedWeaponData3.RANGE), rangedWeaponData3.MAXAMMO, AmmoType.BOLT);
      rangedWeaponModel5.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel5.FlavorDescription = rangedWeaponData3.FLAVOR;
      this[GameItems.IDs.RANGED_HUNTING_CROSSBOW] = (ItemModel) rangedWeaponModel5;
      GameItems.RangedWeaponData rangedWeaponData4 = this.DATA_RANGED_HUNTING_RIFLE;
      ItemRangedWeaponModel rangedWeaponModel7 = new ItemRangedWeaponModel(rangedWeaponData4.NAME, rangedWeaponData4.FLAVOR, "Items\\item_hunting_rifle", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData4.ATK, rangedWeaponData4.DMG, 0, rangedWeaponData4.RANGE), rangedWeaponData4.MAXAMMO, AmmoType.LIGHT_RIFLE);
      rangedWeaponModel7.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel7.FlavorDescription = rangedWeaponData4.FLAVOR;
      this[GameItems.IDs.RANGED_HUNTING_RIFLE] = (ItemModel) rangedWeaponModel7;
      GameItems.RangedWeaponData rangedWeaponData5 = this.DATA_RANGED_PISTOL;
      ItemRangedWeaponModel rangedWeaponModel9 = new ItemRangedWeaponModel(rangedWeaponData5.NAME, rangedWeaponData5.FLAVOR, "Items\\item_pistol", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData5.ATK, rangedWeaponData5.DMG, 0, rangedWeaponData5.RANGE), rangedWeaponData5.MAXAMMO, AmmoType._FIRST);
      rangedWeaponModel9.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel9.FlavorDescription = rangedWeaponData5.FLAVOR;
      this[GameItems.IDs.RANGED_PISTOL] = (ItemModel) rangedWeaponModel9;
      GameItems.RangedWeaponData rangedWeaponData6 = this.DATA_RANGED_KOLT_REVOLVER;
      ItemRangedWeaponModel rangedWeaponModel11 = new ItemRangedWeaponModel(rangedWeaponData6.NAME, rangedWeaponData6.FLAVOR, "Items\\item_kolt_revolver", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData6.ATK, rangedWeaponData6.DMG, 0, rangedWeaponData6.RANGE), rangedWeaponData6.MAXAMMO, AmmoType._FIRST);
      rangedWeaponModel11.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel11.FlavorDescription = rangedWeaponData6.FLAVOR;
      this[GameItems.IDs.RANGED_KOLT_REVOLVER] = (ItemModel) rangedWeaponModel11;
      GameItems.RangedWeaponData rangedWeaponData7 = this.DATA_RANGED_PRECISION_RIFLE;
      ItemRangedWeaponModel rangedWeaponModel13 = new ItemRangedWeaponModel(rangedWeaponData7.NAME, rangedWeaponData7.FLAVOR, "Items\\item_precision_rifle", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData7.ATK, rangedWeaponData7.DMG, 0, rangedWeaponData7.RANGE), rangedWeaponData7.MAXAMMO, AmmoType.HEAVY_RIFLE);
      rangedWeaponModel13.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel13.FlavorDescription = rangedWeaponData7.FLAVOR;
      this[GameItems.IDs.RANGED_PRECISION_RIFLE] = (ItemModel) rangedWeaponModel13;
      GameItems.RangedWeaponData rangedWeaponData8 = this.DATA_RANGED_SHOTGUN;
      ItemRangedWeaponModel rangedWeaponModel15 = new ItemRangedWeaponModel(rangedWeaponData8.NAME, rangedWeaponData8.FLAVOR, "Items\\item_shotgun", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData8.ATK, rangedWeaponData8.DMG, 0, rangedWeaponData8.RANGE), rangedWeaponData8.MAXAMMO, AmmoType.SHOTGUN);
      rangedWeaponModel15.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel15.FlavorDescription = rangedWeaponData8.FLAVOR;
      this[GameItems.IDs.RANGED_SHOTGUN] = (ItemModel) rangedWeaponModel15;
      GameItems.RangedWeaponData rangedWeaponData9 = this.DATA_UNIQUE_SANTAMAN_SHOTGUN;
      ItemRangedWeaponModel rangedWeaponModel17 = new ItemRangedWeaponModel(rangedWeaponData9.NAME, rangedWeaponData9.FLAVOR, "Items\\item_santaman_shotgun", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData9.ATK, rangedWeaponData9.DMG, 0, rangedWeaponData9.RANGE), rangedWeaponData9.MAXAMMO, AmmoType.SHOTGUN);
      rangedWeaponModel17.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel17.FlavorDescription = rangedWeaponData9.FLAVOR;
      rangedWeaponModel17.IsProper = true;
      rangedWeaponModel17.IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_SANTAMAN_SHOTGUN] = (ItemModel) rangedWeaponModel17;
      GameItems.RangedWeaponData rangedWeaponData10 = this.DATA_UNIQUE_HANS_VON_HANZ_PISTOL;
      ItemRangedWeaponModel rangedWeaponModel19 = new ItemRangedWeaponModel(rangedWeaponData10.NAME, rangedWeaponData10.FLAVOR, "Items\\item_hans_von_hanz_pistol", new Attack(AttackKind.FIREARM, new Verb("shoot"), rangedWeaponData10.ATK, rangedWeaponData10.DMG, 0, rangedWeaponData10.RANGE), rangedWeaponData10.MAXAMMO, AmmoType._FIRST);
      rangedWeaponModel19.EquipmentPart = DollPart._FIRST;
      rangedWeaponModel19.FlavorDescription = rangedWeaponData10.FLAVOR;
      rangedWeaponModel19.IsProper = true;
      rangedWeaponModel19.IsUnbreakable = true;
      this[GameItems.IDs.UNIQUE_HANS_VON_HANZ_PISTOL] = (ItemModel) rangedWeaponModel19;

      // Ammunition
      ItemAmmoModel itemAmmoModel1 = new ItemAmmoModel("light pistol bullets", "light pistol bullets", "Items\\item_ammo_light_pistol", AmmoType._FIRST, 20);
      itemAmmoModel1.IsPlural = true;
      itemAmmoModel1.FlavorDescription = "";
      this[GameItems.IDs.AMMO_LIGHT_PISTOL] = (ItemModel) itemAmmoModel1;
      ItemAmmoModel itemAmmoModel3 = new ItemAmmoModel("heavy pistol bullets", "heavy pistol bullets", "Items\\item_ammo_heavy_pistol", AmmoType.HEAVY_PISTOL, 12);
      itemAmmoModel3.IsPlural = true;
      itemAmmoModel3.FlavorDescription = "";
      this[GameItems.IDs.AMMO_HEAVY_PISTOL] = (ItemModel) itemAmmoModel3;
      ItemAmmoModel itemAmmoModel5 = new ItemAmmoModel("light rifle bullets", "light rifle bullets", "Items\\item_ammo_light_rifle", AmmoType.LIGHT_RIFLE, 14);
      itemAmmoModel5.IsPlural = true;
      itemAmmoModel5.FlavorDescription = "";
      this[GameItems.IDs.AMMO_LIGHT_RIFLE] = (ItemModel) itemAmmoModel5;
      ItemAmmoModel itemAmmoModel7 = new ItemAmmoModel("heavy rifle bullets", "heavy rifle bullets", "Items\\item_ammo_heavy_rifle", AmmoType.HEAVY_RIFLE, 20);
      itemAmmoModel7.IsPlural = true;
      itemAmmoModel7.FlavorDescription = "";
      this[GameItems.IDs.AMMO_HEAVY_RIFLE] = (ItemModel) itemAmmoModel7;
      ItemAmmoModel itemAmmoModel9 = new ItemAmmoModel("shotgun shells", "shotgun shells", "Items\\item_ammo_shotgun", AmmoType.SHOTGUN, 10);
      itemAmmoModel9.IsPlural = true;
      itemAmmoModel9.FlavorDescription = "";
      this[GameItems.IDs.AMMO_SHOTGUN] = (ItemModel) itemAmmoModel9;
      ItemAmmoModel itemAmmoModel11 = new ItemAmmoModel("crossbow bolts", "crossbow bolts", "Items\\item_ammo_bolts", AmmoType.BOLT, 30);
      itemAmmoModel11.IsPlural = true;
      itemAmmoModel11.FlavorDescription = "";
      this[GameItems.IDs.AMMO_BOLTS] = (ItemModel) itemAmmoModel11;
      
      // grenade, in its various states
      GameItems.ExplosiveData explosiveData = this.DATA_EXPLOSIVE_GRENADE;
      int[] damage = new int[explosiveData.RADIUS + 1];
      for (int index = 0; index < explosiveData.RADIUS + 1; ++index)
        damage[index] = explosiveData.DMG[index];

      ItemGrenadeModel itemGrenadeModel1 = new ItemGrenadeModel(explosiveData.NAME, explosiveData.PLURAL, "Items\\item_grenade", explosiveData.FUSE, new BlastAttack(explosiveData.RADIUS, damage, true, false), "Icons\\blast", explosiveData.MAXTHROW);
      itemGrenadeModel1.EquipmentPart = DollPart._FIRST;
      itemGrenadeModel1.StackingLimit = explosiveData.STACKLINGLIMIT;
      itemGrenadeModel1.FlavorDescription = explosiveData.FLAVOR;
      this[GameItems.IDs.EXPLOSIVE_GRENADE] = (ItemModel) itemGrenadeModel1;
      ItemGrenadePrimedModel grenadePrimedModel1 = new ItemGrenadePrimedModel("primed " + explosiveData.NAME, "primed " + explosiveData.PLURAL, "Items\\item_grenade_primed", this[GameItems.IDs.EXPLOSIVE_GRENADE] as ItemGrenadeModel);
      grenadePrimedModel1.EquipmentPart = DollPart._FIRST;
      this[GameItems.IDs.EXPLOSIVE_GRENADE_PRIMED] = (ItemModel) grenadePrimedModel1;

      // carpentry
      GameItems.BarricadingMaterialData barricadingMaterialData = this.DATA_BAR_WOODEN_PLANK;
      ItemBarricadeMaterialModel barricadeMaterialModel1 = new ItemBarricadeMaterialModel(barricadingMaterialData.NAME, barricadingMaterialData.PLURAL, "Items\\item_wooden_plank", barricadingMaterialData.VALUE);
      barricadeMaterialModel1.StackingLimit = barricadingMaterialData.STACKINGLIMIT;
      barricadeMaterialModel1.FlavorDescription = barricadingMaterialData.FLAVOR;
      this[GameItems.IDs.BAR_WOODEN_PLANK] = (ItemModel) barricadeMaterialModel1;

      // body armor
      GameItems.ArmorData armorData1 = this.DATA_ARMOR_ARMY;
      ItemBodyArmorModel itemBodyArmorModel1 = new ItemBodyArmorModel(armorData1.NAME, armorData1.PLURAL, "Items\\item_army_bodyarmor", armorData1.PRO_HIT, armorData1.PRO_SHOT, armorData1.ENC, armorData1.WEIGHT);
      itemBodyArmorModel1.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel1.FlavorDescription = armorData1.FLAVOR;
      this[GameItems.IDs.ARMOR_ARMY_BODYARMOR] = (ItemModel) itemBodyArmorModel1;
      GameItems.ArmorData armorData2 = this.DATA_ARMOR_CHAR;
      ItemBodyArmorModel itemBodyArmorModel3 = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_CHAR_light_bodyarmor", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      itemBodyArmorModel3.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel3.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_CHAR_LIGHT_BODYARMOR] = (ItemModel) itemBodyArmorModel3;
      armorData2 = this.DATA_ARMOR_HELLS_SOULS_JACKET;
      ItemBodyArmorModel itemBodyArmorModel5 = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_hells_souls_jacket", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      itemBodyArmorModel5.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel5.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_HELLS_SOULS_JACKET] = (ItemModel) itemBodyArmorModel5;
      armorData2 = this.DATA_ARMOR_FREE_ANGELS_JACKET;
      ItemBodyArmorModel itemBodyArmorModel7 = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_free_angels_jacket", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      itemBodyArmorModel7.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel7.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_FREE_ANGELS_JACKET] = (ItemModel) itemBodyArmorModel7;
      armorData2 = this.DATA_ARMOR_POLICE_JACKET;
      ItemBodyArmorModel itemBodyArmorModel9 = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_police_jacket", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      itemBodyArmorModel9.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel9.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_POLICE_JACKET] = (ItemModel) itemBodyArmorModel9;
      armorData2 = this.DATA_ARMOR_POLICE_RIOT;
      ItemBodyArmorModel itemBodyArmorModel11 = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_police_riot_armor", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      itemBodyArmorModel11.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel11.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_POLICE_RIOT] = (ItemModel) itemBodyArmorModel11;
      armorData2 = this.DATA_ARMOR_HUNTER_VEST;
      ItemBodyArmorModel itemBodyArmorModel13 = new ItemBodyArmorModel(armorData2.NAME, armorData2.PLURAL, "Items\\item_hunter_vest", armorData2.PRO_HIT, armorData2.PRO_SHOT, armorData2.ENC, armorData2.WEIGHT);
      itemBodyArmorModel13.EquipmentPart = DollPart.TORSO;
      itemBodyArmorModel13.FlavorDescription = armorData2.FLAVOR;
      this[GameItems.IDs.ARMOR_HUNTER_VEST] = (ItemModel) itemBodyArmorModel13;

      // trackers
      GameItems.TrackerData trackerData = this.DATA_TRACKER_CELL_PHONE;
      ItemTrackerModel itemTrackerModel1 = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_cellphone", ItemTrackerModel.TrackingFlags.FOLLOWER_AND_LEADER, trackerData.BATTERIES * 30);
      itemTrackerModel1.EquipmentPart = DollPart.LEFT_HAND;
      itemTrackerModel1.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_CELL_PHONE] = (ItemModel) itemTrackerModel1;
      trackerData = this.DATA_TRACKER_ZTRACKER;
      ItemTrackerModel itemTrackerModel3 = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_ztracker", ItemTrackerModel.TrackingFlags.UNDEADS, trackerData.BATTERIES * 30);
      itemTrackerModel3.EquipmentPart = DollPart.LEFT_HAND;
      itemTrackerModel3.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_ZTRACKER] = (ItemModel) itemTrackerModel3;
      trackerData = this.DATA_TRACKER_BLACKOPS_GPS;
      ItemTrackerModel itemTrackerModel5 = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_blackops_gps", ItemTrackerModel.TrackingFlags.BLACKOPS_FACTION, trackerData.BATTERIES * 30);
      itemTrackerModel5.EquipmentPart = DollPart.LEFT_HAND;
      itemTrackerModel5.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_BLACKOPS] = (ItemModel) itemTrackerModel5;
      trackerData = this.DATA_TRACKER_POLICE_RADIO;
      ItemTrackerModel itemTrackerModel7 = new ItemTrackerModel(trackerData.NAME, trackerData.PLURAL, "Items\\item_police_radio", ItemTrackerModel.TrackingFlags.POLICE_FACTION, trackerData.BATTERIES * 30);
      itemTrackerModel7.EquipmentPart = DollPart.LEFT_HAND;
      itemTrackerModel7.FlavorDescription = trackerData.FLAVOR;
      this[GameItems.IDs.TRACKER_POLICE_RADIO] = (ItemModel) itemTrackerModel7;

      // spray paint
      GameItems.SprayPaintData sprayPaintData = this.DATA_SPRAY_PAINT1;
      ItemSprayPaintModel itemSprayPaintModel1 = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag");
      itemSprayPaintModel1.EquipmentPart = DollPart.LEFT_HAND;
      itemSprayPaintModel1.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT1] = (ItemModel) itemSprayPaintModel1;
      sprayPaintData = this.DATA_SPRAY_PAINT2;
      ItemSprayPaintModel itemSprayPaintModel3 = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint2", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag2");
      itemSprayPaintModel3.EquipmentPart = DollPart.LEFT_HAND;
      itemSprayPaintModel3.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT2] = (ItemModel) itemSprayPaintModel3;
      sprayPaintData = this.DATA_SPRAY_PAINT3;
      ItemSprayPaintModel itemSprayPaintModel5 = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint3", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag3");
      itemSprayPaintModel5.EquipmentPart = DollPart.LEFT_HAND;
      itemSprayPaintModel5.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT3] = (ItemModel) itemSprayPaintModel5;
      sprayPaintData = this.DATA_SPRAY_PAINT4;
      ItemSprayPaintModel itemSprayPaintModel7 = new ItemSprayPaintModel(sprayPaintData.NAME, sprayPaintData.PLURAL, "Items\\item_spraypaint4", sprayPaintData.QUANTITY, "Tiles\\Decoration\\player_tag4");
      itemSprayPaintModel7.EquipmentPart = DollPart.LEFT_HAND;
      itemSprayPaintModel7.FlavorDescription = sprayPaintData.FLAVOR;
      this[GameItems.IDs.SPRAY_PAINT4] = (ItemModel)itemSprayPaintModel7;

      // Flashlights
      GameItems.LightData lightData = this.DATA_LIGHT_FLASHLIGHT;
      ItemLightModel itemLightModel1 = new ItemLightModel(lightData.NAME, lightData.PLURAL, "Items\\item_flashlight", lightData.FOV, lightData.BATTERIES * 30, "Items\\item_flashlight_out");
      itemLightModel1.EquipmentPart = DollPart.LEFT_HAND;
      itemLightModel1.FlavorDescription = lightData.FLAVOR;
      this[GameItems.IDs.LIGHT_FLASHLIGHT] = (ItemModel) itemLightModel1;
      lightData = this.DATA_LIGHT_BIG_FLASHLIGHT;
      ItemLightModel itemLightModel3 = new ItemLightModel(lightData.NAME, lightData.PLURAL, "Items\\item_big_flashlight", lightData.FOV, lightData.BATTERIES * 30, "Items\\item_big_flashlight_out");
      itemLightModel3.EquipmentPart = DollPart.LEFT_HAND;
      itemLightModel3.FlavorDescription = lightData.FLAVOR;
      this[GameItems.IDs.LIGHT_BIG_FLASHLIGHT] = (ItemModel) itemLightModel3;

      // stench killer
      GameItems.ScentSprayData scentSprayData = this.DATA_SCENT_SPRAY_STENCH_KILLER;
      ItemSprayScentModel itemSprayScentModel1 = new ItemSprayScentModel(scentSprayData.NAME, scentSprayData.PLURAL, "Items\\item_stench_killer", scentSprayData.QUANTITY, Odor.PERFUME_LIVING_SUPRESSOR, scentSprayData.STRENGTH * 30);
      itemSprayScentModel1.EquipmentPart = DollPart.LEFT_HAND;
      itemSprayScentModel1.FlavorDescription = scentSprayData.FLAVOR;
      this[GameItems.IDs.SCENT_SPRAY_STENCH_KILLER] = (ItemModel)itemSprayScentModel1;

      // Traps
      GameItems.TrapData trapData = this.DATA_TRAP_EMPTY_CAN;
      ItemTrapModel itemTrapModel1 = new ItemTrapModel(trapData.NAME, trapData.PLURAL, "Items\\item_empty_can", trapData.STACKING, trapData.CHANCE, trapData.DAMAGE, trapData.DROP_ACTIVATE, trapData.USE_ACTIVATE, trapData.IS_ONE_TIME, trapData.BREAK_CHANCE, trapData.BLOCK_CHANCE, trapData.BREAK_CHANCE_ESCAPE, trapData.IS_NOISY, trapData.NOISE_NAME, trapData.IS_FLAMMABLE);
      itemTrapModel1.FlavorDescription = trapData.FLAVOR;
      this[GameItems.IDs.TRAP_EMPTY_CAN] = (ItemModel) itemTrapModel1;
      trapData = this.DATA_TRAP_BEAR_TRAP;
      ItemTrapModel itemTrapModel3 = new ItemTrapModel(trapData.NAME, trapData.PLURAL, "Items\\item_bear_trap", trapData.STACKING, trapData.CHANCE, trapData.DAMAGE, trapData.DROP_ACTIVATE, trapData.USE_ACTIVATE, trapData.IS_ONE_TIME, trapData.BREAK_CHANCE, trapData.BLOCK_CHANCE, trapData.BREAK_CHANCE_ESCAPE, trapData.IS_NOISY, trapData.NOISE_NAME, trapData.IS_FLAMMABLE);
      itemTrapModel3.FlavorDescription = trapData.FLAVOR;
      this[GameItems.IDs.TRAP_BEAR_TRAP] = (ItemModel) itemTrapModel3;
      trapData = this.DATA_TRAP_SPIKES;
      ItemTrapModel itemTrapModel5 = new ItemTrapModel(trapData.NAME, trapData.PLURAL, "Items\\item_spikes", trapData.STACKING, trapData.CHANCE, trapData.DAMAGE, trapData.DROP_ACTIVATE, trapData.USE_ACTIVATE, trapData.IS_ONE_TIME, trapData.BREAK_CHANCE, trapData.BLOCK_CHANCE, trapData.BREAK_CHANCE_ESCAPE, trapData.IS_NOISY, trapData.NOISE_NAME, trapData.IS_FLAMMABLE);
      itemTrapModel5.FlavorDescription = trapData.FLAVOR;
      this[GameItems.IDs.TRAP_SPIKES] = (ItemModel) itemTrapModel5;
      trapData = this.DATA_TRAP_BARBED_WIRE;
      ItemTrapModel itemTrapModel7 = new ItemTrapModel(trapData.NAME, trapData.PLURAL, "Items\\item_barbed_wire", trapData.STACKING, trapData.CHANCE, trapData.DAMAGE, trapData.DROP_ACTIVATE, trapData.USE_ACTIVATE, trapData.IS_ONE_TIME, trapData.BREAK_CHANCE, trapData.BLOCK_CHANCE, trapData.BREAK_CHANCE_ESCAPE, trapData.IS_NOISY, trapData.NOISE_NAME, trapData.IS_FLAMMABLE);
      itemTrapModel7.FlavorDescription = trapData.FLAVOR;
      this[GameItems.IDs.TRAP_BARBED_WIRE] = (ItemModel) itemTrapModel7;

      // entertainment
      GameItems.EntData entData1 = this.DATA_ENT_BOOK;
      ItemEntertainmentModel entertainmentModel1 = new ItemEntertainmentModel(entData1.NAME, entData1.PLURAL, "Items\\item_book", entData1.VALUE, entData1.BORECHANCE);
      entertainmentModel1.StackingLimit = entData1.STACKING;
      entertainmentModel1.FlavorDescription = entData1.FLAVOR;
      this[GameItems.IDs.ENT_BOOK] = (ItemModel) entertainmentModel1;
      GameItems.EntData entData2 = this.DATA_ENT_MAGAZINE;
      ItemEntertainmentModel entertainmentModel3 = new ItemEntertainmentModel(entData2.NAME, entData2.PLURAL, "Items\\item_magazine", entData2.VALUE, entData2.BORECHANCE);
      entertainmentModel3.StackingLimit = entData2.STACKING;
      entertainmentModel3.FlavorDescription = entData2.FLAVOR;
      this[GameItems.IDs.ENT_MAGAZINE] = (ItemModel) entertainmentModel3;

      this[GameItems.IDs.UNIQUE_SUBWAY_BADGE] = new ItemModel("Subway Worker Badge", "Subways Worker Badges", "Items\\item_subway_badge")
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

    private CSVLine FindLineForModel(CSVTable table, GameItems.IDs modelID)
    {
      foreach (CSVLine line in table.Lines)
      {
        if (line[0].ParseText() == modelID.ToString())
          return line;
      }
      return (CSVLine) null;
    }

    private _DATA_TYPE_ GetDataFromCSVTable<_DATA_TYPE_>(IRogueUI ui, CSVTable table, Func<CSVLine, _DATA_TYPE_> fn, GameItems.IDs modelID)
    {
      CSVLine lineForModel = this.FindLineForModel(table, modelID);
      if (lineForModel == null)
        throw new InvalidOperationException(string.Format("model {0} not found", (object) modelID.ToString()));
      try
      {
        return fn(lineForModel);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(string.Format("invalid data format for model {0}; exception : {1}", (object) modelID.ToString(), (object) ex.ToString()));
      }
    }

    private bool LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, GameItems.IDs[] idsToRead, out _DATA_TYPE_[] data)
    {
      this.Notify(ui, kind, "loading file...");
      List<string> stringList = new List<string>();
      bool flag = true;
      using (StreamReader streamReader = File.OpenText(path))
      {
        while (!streamReader.EndOfStream)
        {
          string str = streamReader.ReadLine();
          if (flag)
            flag = false;
          else
            stringList.Add(str);
        }
        streamReader.Close();
      }
      this.Notify(ui, kind, "parsing CSV...");
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), fieldsCount);
      this.Notify(ui, kind, "reading data...");
      data = new _DATA_TYPE_[idsToRead.Length];
      for (int index = 0; index < idsToRead.Length; ++index)
        data[index] = this.GetDataFromCSVTable<_DATA_TYPE_>(ui, toTable, fn, idsToRead[index]);
      this.Notify(ui, kind, "done!");
      return true;
    }

    public bool LoadMedicineFromCSV(IRogueUI ui, string path)
    {
      GameItems.MedecineData[] data;
      this.LoadDataFromCSV<GameItems.MedecineData>(ui, path, "medicine items", 10, new Func<CSVLine, GameItems.MedecineData>(GameItems.MedecineData.FromCSVLine), new GameItems.IDs[6]
      {
        GameItems.IDs._FIRST,
        GameItems.IDs.MEDICINE_MEDIKIT,
        GameItems.IDs.MEDICINE_PILLS_SLP,
        GameItems.IDs.MEDICINE_PILLS_STA,
        GameItems.IDs.MEDICINE_PILLS_SAN,
        GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL
      }, out data);
      this.DATA_MEDICINE_BANDAGE = data[0];
      this.DATA_MEDICINE_MEDIKIT = data[1];
      this.DATA_MEDICINE_PILLS_SLP = data[2];
      this.DATA_MEDICINE_PILLS_STA = data[3];
      this.DATA_MEDICINE_PILLS_SAN = data[4];
      this.DATA_MEDICINE_PILLS_ANTIVIRAL = data[5];
      return true;
    }

    public bool LoadFoodFromCSV(IRogueUI ui, string path)
    {
      GameItems.FoodData[] data;
      this.LoadDataFromCSV<GameItems.FoodData>(ui, path, "food items", 7, new Func<CSVLine, GameItems.FoodData>(GameItems.FoodData.FromCSVLine), new GameItems.IDs[3]
      {
        GameItems.IDs.FOOD_ARMY_RATION,
        GameItems.IDs.FOOD_CANNED_FOOD,
        GameItems.IDs.FOOD_GROCERIES
      }, out data);
      this.DATA_FOOD_ARMY_RATION = data[0];
      this.DATA_FOOD_CANNED_FOOD = data[1];
      this.DATA_FOOD_GROCERIES = data[2];
      return true;
    }

    public bool LoadMeleeWeaponsFromCSV(IRogueUI ui, string path)
    {
      GameItems.MeleeWeaponData[] data;
      this.LoadDataFromCSV<GameItems.MeleeWeaponData>(ui, path, "melee weapons items", 9, new Func<CSVLine, GameItems.MeleeWeaponData>(GameItems.MeleeWeaponData.FromCSVLine), new GameItems.IDs[16]
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
      this.DATA_MELEE_BASEBALLBAT = data[0];
      this.DATA_MELEE_COMBAT_KNIFE = data[1];
      this.DATA_MELEE_CROWBAR = data[2];
      this.DATA_MELEE_GOLFCLUB = data[3];
      this.DATA_MELEE_HUGE_HAMMER = data[4];
      this.DATA_MELEE_IRON_GOLFCLUB = data[5];
      this.DATA_MELEE_SHOVEL = data[6];
      this.DATA_MELEE_SHORT_SHOVEL = data[7];
      this.DATA_MELEE_TRUNCHEON = data[8];
      this.DATA_MELEE_UNIQUE_JASON_MYERS_AXE = data[9];
      this.DATA_MELEE_IMPROVISED_CLUB = data[10];
      this.DATA_MELEE_IMPROVISED_SPEAR = data[11];
      this.DATA_MELEE_SMALL_HAMMER = data[12];
      this.DATA_MELEE_UNIQUE_FAMU_FATARU_KATANA = data[13];
      this.DATA_MELEE_UNIQUE_BIGBEAR_BAT = data[14];
      this.DATA_MELEE_UNIQUE_ROGUEDJACK_KEYBOARD = data[15];
      return true;
    }

    public bool LoadRangedWeaponsFromCSV(IRogueUI ui, string path)
    {
      GameItems.RangedWeaponData[] data;
      this.LoadDataFromCSV<GameItems.RangedWeaponData>(ui, path, "ranged weapons items", 8, new Func<CSVLine, GameItems.RangedWeaponData>(GameItems.RangedWeaponData.FromCSVLine), new GameItems.IDs[10]
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
      this.DATA_RANGED_ARMY_PISTOL = data[0];
      this.DATA_RANGED_ARMY_RIFLE = data[1];
      this.DATA_RANGED_HUNTING_CROSSBOW = data[2];
      this.DATA_RANGED_HUNTING_RIFLE = data[3];
      this.DATA_RANGED_KOLT_REVOLVER = data[4];
      this.DATA_RANGED_PISTOL = data[5];
      this.DATA_RANGED_PRECISION_RIFLE = data[6];
      this.DATA_RANGED_SHOTGUN = data[7];
      this.DATA_UNIQUE_SANTAMAN_SHOTGUN = data[8];
      this.DATA_UNIQUE_HANS_VON_HANZ_PISTOL = data[9];
      return true;
    }

    public bool LoadExplosivesFromCSV(IRogueUI ui, string path)
    {
      GameItems.ExplosiveData[] data;
      this.LoadDataFromCSV<GameItems.ExplosiveData>(ui, path, "explosives items", 14, new Func<CSVLine, GameItems.ExplosiveData>(GameItems.ExplosiveData.FromCSVLine), new GameItems.IDs[1]
      {
        GameItems.IDs.EXPLOSIVE_GRENADE
      }, out data);
      this.DATA_EXPLOSIVE_GRENADE = data[0];
      return true;
    }

    public bool LoadBarricadingMaterialFromCSV(IRogueUI ui, string path)
    {
      GameItems.BarricadingMaterialData[] data;
      this.LoadDataFromCSV<GameItems.BarricadingMaterialData>(ui, path, "barricading items", 6, new Func<CSVLine, GameItems.BarricadingMaterialData>(GameItems.BarricadingMaterialData.FromCSVLine), new GameItems.IDs[1]
      {
        GameItems.IDs.BAR_WOODEN_PLANK
      }, out data);
      this.DATA_BAR_WOODEN_PLANK = data[0];
      return true;
    }

    public bool LoadArmorsFromCSV(IRogueUI ui, string path)
    {
      GameItems.ArmorData[] data;
      this.LoadDataFromCSV<GameItems.ArmorData>(ui, path, "armors items", 8, new Func<CSVLine, GameItems.ArmorData>(GameItems.ArmorData.FromCSVLine), new GameItems.IDs[7]
      {
        GameItems.IDs.ARMOR_ARMY_BODYARMOR,
        GameItems.IDs.ARMOR_CHAR_LIGHT_BODYARMOR,
        GameItems.IDs.ARMOR_HELLS_SOULS_JACKET,
        GameItems.IDs.ARMOR_FREE_ANGELS_JACKET,
        GameItems.IDs.ARMOR_POLICE_JACKET,
        GameItems.IDs.ARMOR_POLICE_RIOT,
        GameItems.IDs.ARMOR_HUNTER_VEST
      }, out data);
      this.DATA_ARMOR_ARMY = data[0];
      this.DATA_ARMOR_CHAR = data[1];
      this.DATA_ARMOR_HELLS_SOULS_JACKET = data[2];
      this.DATA_ARMOR_FREE_ANGELS_JACKET = data[3];
      this.DATA_ARMOR_POLICE_JACKET = data[4];
      this.DATA_ARMOR_POLICE_RIOT = data[5];
      this.DATA_ARMOR_HUNTER_VEST = data[6];
      return true;
    }

    public bool LoadTrackersFromCSV(IRogueUI ui, string path)
    {
      GameItems.TrackerData[] data;
      this.LoadDataFromCSV<GameItems.TrackerData>(ui, path, "trackers items", 5, new Func<CSVLine, GameItems.TrackerData>(GameItems.TrackerData.FromCSVLine), new GameItems.IDs[4]
      {
        GameItems.IDs.TRACKER_BLACKOPS,
        GameItems.IDs.TRACKER_CELL_PHONE,
        GameItems.IDs.TRACKER_ZTRACKER,
        GameItems.IDs.TRACKER_POLICE_RADIO
      }, out data);
      this.DATA_TRACKER_BLACKOPS_GPS = data[0];
      this.DATA_TRACKER_CELL_PHONE = data[1];
      this.DATA_TRACKER_ZTRACKER = data[2];
      this.DATA_TRACKER_POLICE_RADIO = data[3];
      return true;
    }

    public bool LoadSpraypaintsFromCSV(IRogueUI ui, string path)
    {
      GameItems.SprayPaintData[] data;
      this.LoadDataFromCSV<GameItems.SprayPaintData>(ui, path, "spraypaint items", 5, new Func<CSVLine, GameItems.SprayPaintData>(GameItems.SprayPaintData.FromCSVLine), new GameItems.IDs[4]
      {
        GameItems.IDs.SPRAY_PAINT1,
        GameItems.IDs.SPRAY_PAINT2,
        GameItems.IDs.SPRAY_PAINT3,
        GameItems.IDs.SPRAY_PAINT4
      }, out data);
      this.DATA_SPRAY_PAINT1 = data[0];
      this.DATA_SPRAY_PAINT2 = data[1];
      this.DATA_SPRAY_PAINT3 = data[2];
      this.DATA_SPRAY_PAINT4 = data[3];
      return true;
    }

    public bool LoadLightsFromCSV(IRogueUI ui, string path)
    {
      GameItems.LightData[] data;
      this.LoadDataFromCSV<GameItems.LightData>(ui, path, "lights items", 6, new Func<CSVLine, GameItems.LightData>(GameItems.LightData.FromCSVLine), new GameItems.IDs[2]
      {
        GameItems.IDs.LIGHT_FLASHLIGHT,
        GameItems.IDs.LIGHT_BIG_FLASHLIGHT
      }, out data);
      this.DATA_LIGHT_FLASHLIGHT = data[0];
      this.DATA_LIGHT_BIG_FLASHLIGHT = data[1];
      return true;
    }

    public bool LoadScentspraysFromCSV(IRogueUI ui, string path)
    {
      GameItems.ScentSprayData[] data;
      this.LoadDataFromCSV<GameItems.ScentSprayData>(ui, path, "scentsprays items", 6, new Func<CSVLine, GameItems.ScentSprayData>(GameItems.ScentSprayData.FromCSVLine), new GameItems.IDs[1]
      {
        GameItems.IDs.SCENT_SPRAY_STENCH_KILLER
      }, out data);
      this.DATA_SCENT_SPRAY_STENCH_KILLER = data[0];
      return true;
    }

    public bool LoadTrapsFromCSV(IRogueUI ui, string path)
    {
      GameItems.TrapData[] data;
      this.LoadDataFromCSV<GameItems.TrapData>(ui, path, "traps items", 16, new Func<CSVLine, GameItems.TrapData>(GameItems.TrapData.FromCSVLine), new GameItems.IDs[4]
      {
        GameItems.IDs.TRAP_EMPTY_CAN,
        GameItems.IDs.TRAP_BEAR_TRAP,
        GameItems.IDs.TRAP_SPIKES,
        GameItems.IDs.TRAP_BARBED_WIRE
      }, out data);
      this.DATA_TRAP_EMPTY_CAN = data[0];
      this.DATA_TRAP_BEAR_TRAP = data[1];
      this.DATA_TRAP_SPIKES = data[2];
      this.DATA_TRAP_BARBED_WIRE = data[3];
      return true;
    }

    public bool LoadEntertainmentFromCSV(IRogueUI ui, string path)
    {
      GameItems.EntData[] data;
      this.LoadDataFromCSV<GameItems.EntData>(ui, path, "entertainment items", 7, new Func<CSVLine, GameItems.EntData>(GameItems.EntData.FromCSVLine), new GameItems.IDs[2]
      {
        GameItems.IDs.ENT_BOOK,
        GameItems.IDs.ENT_MAGAZINE
      }, out data);
      this.DATA_ENT_BOOK = data[0];
      this.DATA_ENT_MAGAZINE = data[1];
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
