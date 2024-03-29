﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameImages
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define CGI_ICONS

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using Zaimoni.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// when converting to System.Windows.Media, would need two classes:
// * BitmapImage loads from a file
// * WriteableBitmap is the CGI version.
// Both of these are subclasses of BitmapSource which in turn is a subclass of ImageSource.

namespace djack.RogueSurvivor.Gameplay
{
  internal static class GameImages
  {
    private static readonly Dictionary<string, Bitmap> s_Images = new Dictionary<string, Bitmap>();
    private static readonly Dictionary<string, Bitmap> s_GrayLevelImages = new Dictionary<string, Bitmap>();
    private static readonly List<string> s_Mods = new List<string>();
    public const string ACTIVITY_CHASING = "Activities\\chasing";
    public const string ACTIVITY_CHASING_PLAYER = "Activities\\chasing_player";
    public const string ACTIVITY_TRACKING = "Activities\\tracking";
    public const string ACTIVITY_FLEEING = "Activities\\fleeing";
    public const string ACTIVITY_FLEEING_FROM_EXPLOSIVE = "Activities\\fleeing_explosive";
    public const string ACTIVITY_FOLLOWING = "Activities\\following";
    public const string ACTIVITY_FOLLOWING_ORDER = "Activities\\following_order";
    public const string ACTIVITY_FOLLOWING_PLAYER = "Activities\\following_player";
    public const string ACTIVITY_FOLLOWING_LEADER = "Activities\\following_leader";  // alpha10
    public const string ACTIVITY_SLEEPING = "Activities\\sleeping";
    public const string ICON_BLAST = "Icons\\blast";
    public const string ICON_CAN_TRADE = "Icons\\can_trade";
    public const string ICON_THREAT_SAFE = "Icons\\threat_safe";
    public const string ICON_THREAT_DANGER = "Icons\\threat_danger";
    public const string ICON_THREAT_HIGH_DANGER = "Icons\\threat_high_danger";
    public const string ICON_CANT_RUN = "Icons\\cant_run";
    public const string ICON_CROUCHING = "Icons\\crouching";
    public const string ICON_EXPIRED_FOOD = "Icons\\expired_food";
    public const string ICON_SPOILED_FOOD = "Icons\\spoiled_food";
    public const string ICON_FOOD_ALMOST_HUNGRY = "Icons\\food_almost_hungry";
    public const string ICON_FOOD_HUNGRY = "Icons\\food_hungry";
    public const string ICON_FOOD_STARVING = "Icons\\food_starving";
    public const string ICON_HEALING = "Icons\\healing";
    public const string ICON_IS_TARGET = "Icons\\is_target";
    public const string ICON_IS_TARGETTED = "Icons\\is_targetted";
    public const string ICON_IS_TARGETING = "Icons\\is_targeting";  // alpha10
    public const string ICON_IS_IN_GROUP = "Icons\\is_in_group";  // alpha10
    public const string ICON_KILLED = "Icons\\killed";
    public const string ICON_LEADER = "Icons\\leader";
    public const string ICON_MELEE_ATTACK = "Icons\\melee_attack";
    public const string ICON_MELEE_MISS = "Icons\\melee_miss";
    public const string ICON_MELEE_DAMAGE = "Icons\\melee_damage";
    public const string ICON_ODOR_SUPPRESSED = "Icons\\odor_suppressed";  // alpha10
    public const string ICON_OUT_OF_AMMO = "Icons\\out_of_ammo";
    public const string ICON_OUT_OF_BATTERIES = "Icons\\out_of_batteries";
    public const string ICON_RANGED_ATTACK = "Icons\\ranged_attack";
    public const string ICON_RANGED_MISS = "Icons\\ranged_miss";
    public const string ICON_RANGED_DAMAGE = "Icons\\ranged_damage";
    public const string ICON_RUNNING = "Icons\\running";
    public const string ICON_ROT_ALMOST_HUNGRY = "Icons\\rot_almost_hungry";
    public const string ICON_ROT_HUNGRY = "Icons\\rot_hungry";
    public const string ICON_ROT_STARVING = "Icons\\rot_starving";
    public const string ICON_SLEEP_ALMOST_SLEEPY = "Icons\\sleep_almost_sleepy";
    public const string ICON_SLEEP_SLEEPY = "Icons\\sleep_sleepy";
    public const string ICON_SLEEP_EXHAUSTED = "Icons\\sleep_exhausted";
    public const string ICON_TARGET = "Icons\\target";
    public const string ICON_LINE_BAD = "Icons\\line_bad";
    public const string ICON_LINE_BLOCKED = "Icons\\line_blocked";
    public const string ICON_LINE_CLEAR = "Icons\\line_clear";
    public const string ICON_SCENT_LIVING = "Icons\\scent_living";
    public const string ICON_SCENT_ZOMBIEMASTER = "Icons\\scent_zm";
    public const string ICON_SCENT_LIVING_SUPRESSOR = "Icons\\scent_living_supressor";
    public const string ICON_AGGRESSOR = "Icons\\enemy_you_aggressor";  // XXX these three should be CGI
    public const string ICON_INDIRECT_ENEMIES = "Icons\\enemy_indirect";
    public const string ICON_SELF_DEFENCE = "Icons\\enemy_you_self_defence";
    public const string ICON_TRAP_ACTIVATED = "Icons\\trap_activated";
    public const string ICON_TRAP_ACTIVATED_SAFE_GROUP = "Icons\\trap_activated_safe_group";  // alpha10
    public const string ICON_TRAP_ACTIVATED_SAFE_PLAYER = "Icons\\trap_activated_safe_player";  // alpha10
    public const string ICON_TRAP_TRIGGERED = "Icons\\trap_triggered";
    public const string ICON_TRAP_TRIGGERED_SAFE_GROUP = "Icons\\trap_triggered_safe_group";  // alpha10
    public const string ICON_TRAP_TRIGGERED_SAFE_PLAYER = "Icons\\trap_triggered_safe_player";  // alpha10
    public const string ICON_SANITY_DISTURBED = "Icons\\sanity_disturbed";
    public const string ICON_SANITY_INSANE = "Icons\\sanity_insane";
    public const string ICON_BORING_ITEM = "Icons\\boring_item";
    public const string ICON_ZGRAB = "Icons\\zgrab";  // alpha10
    public const string TILE_FLOOR_ASPHALT = "Tiles\\floor_asphalt";
    public const string TILE_FLOOR_CONCRETE = "Tiles\\floor_concrete";
    public const string TILE_FLOOR_GRASS_SWNE_CONCRETE_W = "Tiles\\floor_grass_swne_concrete_w";
    public const string TILE_FLOOR_GRASS_SWNE_CONCRETE_E = "Tiles\\floor_grass_swne_concrete_e";
    public const string TILE_FLOOR_GRASS_SENW_CONCRETE_W = "Tiles\\floor_grass_senw_concrete_w";
    public const string TILE_FLOOR_GRASS_SENW_CONCRETE_E = "Tiles\\floor_grass_senw_concrete_e";
    public const string TILE_FLOOR_GRASS = "Tiles\\floor_grass";
    public const string TILE_FLOOR_OFFICE = "Tiles\\floor_office";
    public const string TILE_FLOOR_PLANKS = "Tiles\\floor_planks";
    public const string TILE_FLOOR_SEWER_WATER = "Tiles\\floor_sewer_water";
    public const string TILE_FLOOR_SEWER_WATER_ANIM1 = "Tiles\\floor_sewer_water_anim1";
    public const string TILE_FLOOR_SEWER_WATER_ANIM2 = "Tiles\\floor_sewer_water_anim2";
    public const string TILE_FLOOR_SEWER_WATER_ANIM3 = "Tiles\\floor_sewer_water_anim3";
    public const string TILE_FLOOR_SEWER_WATER_COVER = "Tiles\\floor_sewer_water_cover";
    public const string TILE_FLOOR_TILES = "Tiles\\floor_tiles";
    public const string TILE_FLOOR_WALKWAY = "Tiles\\floor_walkway";
    public const string TILE_ROAD_ASPHALT_NS = "Tiles\\road_asphalt_ns";
    public const string TILE_ROAD_ASPHALT_EW = "Tiles\\road_asphalt_ew";
    public const string TILE_ROAD_ASPHALT_SWNE = "Tiles\\road_asphalt_swne";
    public const string TILE_ROAD_ASPHALT_SENW = "Tiles\\road_asphalt_senw";
    public const string TILE_ROAD_ASPHALT_SWNE_CONCRETE_W = "Tiles\\road_asphalt_swne_concrete_w";
    public const string TILE_ROAD_ASPHALT_SWNE_CONCRETE_E = "Tiles\\road_asphalt_swne_concrete_e";
    public const string TILE_ROAD_ASPHALT_SENW_CONCRETE_W = "Tiles\\road_asphalt_senw_concrete_w";
    public const string TILE_ROAD_ASPHALT_SENW_CONCRETE_E = "Tiles\\road_asphalt_senw_concrete_e";
    public const string TILE_RAIL_NS = "Tiles\\rail_ns";
    public const string TILE_RAIL_EW = "Tiles\\rail_ew";
    public const string TILE_RAIL_SWNE = "Tiles\\rail_swne";
    public const string TILE_RAIL_SWNE_WALL_W = "Tiles\\rail_swne_wall_w";
    public const string TILE_RAIL_SWNE_WALL_E = "Tiles\\rail_swne_wall_e";
    public const string TILE_RAIL_SENW = "Tiles\\rail_senw";
    public const string TILE_RAIL_SENW_WALL_W = "Tiles\\rail_senw_wall_w";
    public const string TILE_RAIL_SENW_WALL_E = "Tiles\\rail_senw_wall_e";
    public const string TILE_WALL_BRICK = "Tiles\\wall_brick";
    public const string TILE_WALL_CHAR_OFFICE = "Tiles\\wall_char_office";
    public const string TILE_WALL_HOSPITAL = "Tiles\\wall_hospital";
    public const string TILE_WALL_SEWER = "Tiles\\wall_sewer";
    public const string TILE_WALL_STONE = "Tiles\\wall_stone";
    public const string DECO_BLOODIED_FLOOR = "Tiles\\Decoration\\bloodied_floor";
    public const string DECO_BLOODIED_WALL = "Tiles\\Decoration\\bloodied_wall";
    public const string DECO_ZOMBIE_REMAINS = "Tiles\\Decoration\\zombie_remains";
    public const string DECO_VOMIT = "Tiles\\Decoration\\vomit";
    public const string DECO_POSTERS1 = "Tiles\\Decoration\\posters1";
    public const string DECO_POSTERS2 = "Tiles\\Decoration\\posters2";
    public const string DECO_TAGS1 = "Tiles\\Decoration\\tags1";
    public const string DECO_TAGS2 = "Tiles\\Decoration\\tags2";
    public const string DECO_TAGS3 = "Tiles\\Decoration\\tags3";
    public const string DECO_TAGS4 = "Tiles\\Decoration\\tags4";
    public const string DECO_TAGS5 = "Tiles\\Decoration\\tags5";
    public const string DECO_TAGS6 = "Tiles\\Decoration\\tags6";
    public const string DECO_TAGS7 = "Tiles\\Decoration\\tags7";
    public const string DECO_SHOP_CONSTRUCTION = "Tiles\\Decoration\\shop_construction";
    public const string DECO_SHOP_GENERAL_STORE = "Tiles\\Decoration\\shop_general_store";
    public const string DECO_SHOP_GROCERY = "Tiles\\Decoration\\shop_grocery";
    public const string DECO_SHOP_GUNSHOP = "Tiles\\Decoration\\shop_gunshop";
    public const string DECO_SHOP_PHARMACY = "Tiles\\Decoration\\shop_pharmacy";
    public const string DECO_SHOP_SPORTSWEAR = "Tiles\\Decoration\\shop_sportswear";
    public const string DECO_SHOP_HUNTING = "Tiles\\Decoration\\shop_hunting";
    public const string DECO_CHAR_OFFICE = "Tiles\\Decoration\\char_office";
    public const string DECO_CHAR_FLOOR_LOGO = "Tiles\\Decoration\\char_floor_logo";
    public const string DECO_CHAR_POSTER1 = "Tiles\\Decoration\\char_poster1";
    public const string DECO_CHAR_POSTER2 = "Tiles\\Decoration\\char_poster2";
    public const string DECO_CHAR_POSTER3 = "Tiles\\Decoration\\char_poster3";
    public const string DECO_PLAYER_TAG1 = "Tiles\\Decoration\\player_tag";
    public const string DECO_PLAYER_TAG2 = "Tiles\\Decoration\\player_tag2";
    public const string DECO_PLAYER_TAG3 = "Tiles\\Decoration\\player_tag3";
    public const string DECO_PLAYER_TAG4 = "Tiles\\Decoration\\player_tag4";
    public const string DECO_ROGUEDJACK_TAG = "Tiles\\Decoration\\roguedjack";
    public const string DECO_SEWER_LADDER = "Tiles\\Decoration\\sewer_ladder";
    public const string DECO_SEWER_HOLE = "Tiles\\Decoration\\sewer_hole";
    public const string DECO_SEWERS_BUILDING = "Tiles\\Decoration\\sewers_building";
    public const string DECO_SUBWAY_BUILDING = "Tiles\\Decoration\\subway_building";
    public const string DECO_STAIRS_UP = "Tiles\\Decoration\\stairs_up";
    public const string DECO_STAIRS_DOWN = "Tiles\\Decoration\\stairs_down";
    public const string DECO_POWER_SIGN_BIG = "Tiles\\Decoration\\power_sign_big";
    public const string DECO_POLICE_STATION = "Tiles\\Decoration\\police_station";
    public const string DECO_HOSPITAL = "Tiles\\Decoration\\hospital";
    public const string OBJ_TREE = "MapObjects\\tree";
    public const string OBJ_WOODEN_DOOR_CLOSED = "MapObjects\\wooden_door_closed";
    public const string OBJ_WOODEN_DOOR_OPEN = "MapObjects\\wooden_door_open";
    public const string OBJ_WOODEN_DOOR_BROKEN = "MapObjects\\wooden_door_broken";
    public const string OBJ_GLASS_DOOR_CLOSED = "MapObjects\\glass_door_closed";
    public const string OBJ_GLASS_DOOR_OPEN = "MapObjects\\glass_door_open";
    public const string OBJ_GLASS_DOOR_BROKEN = "MapObjects\\glass_door_broken";
    public const string OBJ_CHAR_DOOR_CLOSED = "MapObjects\\dark_door_closed";
    public const string OBJ_CHAR_DOOR_OPEN = "MapObjects\\dark_door_open";
    public const string OBJ_CHAR_DOOR_BROKEN = "MapObjects\\dark_door_broken";
    public const string OBJ_WINDOW_CLOSED = "MapObjects\\window_closed";
    public const string OBJ_WINDOW_OPEN = "MapObjects\\window_open";
    public const string OBJ_WINDOW_BROKEN = "MapObjects\\window_broken";
    public const string OBJ_BENCH = "MapObjects\\bench";
    public const string OBJ_FENCE = "MapObjects\\fence";
    public const string OBJ_CAR1 = "MapObjects\\car1";
    public const string OBJ_CAR2 = "MapObjects\\car2";
    public const string OBJ_CAR3 = "MapObjects\\car3";
    public const string OBJ_CAR4 = "MapObjects\\car4";
    public const string OBJ_SHOP_SHELF = "MapObjects\\shop_shelf";
    public const string OBJ_BED = "MapObjects\\bed";
    public const string OBJ_WARDROBE = "MapObjects\\wardrobe";
    public const string OBJ_TABLE = "MapObjects\\table";
    public const string OBJ_FRIDGE = "MapObjects\\fridge";
    public const string OBJ_DRAWER = "MapObjects\\drawer";
    public const string OBJ_CHAIR = "MapObjects\\chair";
    public const string OBJ_NIGHT_TABLE = "MapObjects\\nighttable";
    public const string OBJ_CHAR_CHAIR = "MapObjects\\char_chair";
    public const string OBJ_CHAR_TABLE = "MapObjects\\char_table";
    public const string OBJ_IRON_BENCH = "MapObjects\\iron_bench";
    public const string OBJ_IRON_DOOR_OPEN = "MapObjects\\iron_door_open";
    public const string OBJ_IRON_DOOR_CLOSED = "MapObjects\\iron_door_closed";
    public const string OBJ_IRON_DOOR_BROKEN = "MapObjects\\iron_door_broken";
    public const string OBJ_IRON_FENCE = "MapObjects\\iron_fence";
    public const string OBJ_BARRELS = "MapObjects\\barrels";
    public const string OBJ_JUNK = "MapObjects\\junk";
    public const string OBJ_POWERGEN_OFF = "MapObjects\\power_generator_off";
    public const string OBJ_POWERGEN_ON = "MapObjects\\power_generator_on";
    public const string OBJ_GATE_CLOSED = "MapObjects\\gate_closed";
    public const string OBJ_GATE_OPEN = "MapObjects\\gate_open";
    public const string OBJ_BOARD = "MapObjects\\announcement_board";
    public const string OBJ_SMALL_WOODEN_FORTIFICATION = "MapObjects\\wooden_small_fortification";
    public const string OBJ_LARGE_WOODEN_FORTIFICATION = "MapObjects\\wooden_large_fortification";
    public const string OBJ_HOSPITAL_BED = "MapObjects\\hospital_bed";
    public const string OBJ_HOSPITAL_CHAIR = "MapObjects\\hospital_chair";
    public const string OBJ_HOSPITAL_NIGHT_TABLE = "MapObjects\\hospital_nighttable";
    public const string OBJ_HOSPITAL_WARDROBE = "MapObjects\\hospital_wardrobe";
    public const string OBJ_HOSPITAL_DOOR_OPEN = "MapObjects\\hospital_door_open";
    public const string OBJ_HOSPITAL_DOOR_CLOSED = "MapObjects\\hospital_door_closed";
    public const string OBJ_HOSPITAL_DOOR_BROKEN = "MapObjects\\hospital_door_broken";
    public const string OBJ_GARDEN_FENCE = "MapObjects\\garden_fence"; // alpha10
    public const string OBJ_WIRE_FENCE = "MapObjects\\wire_fence"; // alpha10
    public const string PLAYER_FOLLOWER = "Actors\\player_follower";    // XXX these three should be CGI
    public const string PLAYER_FOLLOWER_TRUST = "Actors\\player_follower_trust";
    public const string PLAYER_FOLLOWER_BOND = "Actors\\player_follower_bond";
    public const string ACTOR_SKELETON = "Actors\\skeleton";
    public const string ACTOR_RED_EYED_SKELETON = "Actors\\red_eyed_skeleton";
    public const string ACTOR_RED_SKELETON = "Actors\\red_skeleton";
    public const string ACTOR_ZOMBIE = "Actors\\zombie";
    public const string ACTOR_DARK_EYED_ZOMBIE = "Actors\\dark_eyed_zombie";
    public const string ACTOR_DARK_ZOMBIE = "Actors\\dark_zombie";
    public const string ACTOR_MALE_NEOPHYTE = "Actors\\male_neophyte";
    public const string ACTOR_FEMALE_NEOPHYTE = "Actors\\female_neophyte";
    public const string ACTOR_MALE_DISCIPLE = "Actors\\male_disciple";
    public const string ACTOR_FEMALE_DISCIPLE = "Actors\\female_disciple";
    public const string ACTOR_ZOMBIE_MASTER = "Actors\\zombie_master";
    public const string ACTOR_ZOMBIE_LORD = "Actors\\zombie_lord";
    public const string ACTOR_ZOMBIE_PRINCE = "Actors\\zombie_prince";
    public const string ACTOR_RAT_ZOMBIE = "Actors\\rat_zombie";
    public const string ACTOR_SEWERS_THING = "Actors\\sewers_thing";
    public const string ACTOR_JASON_MYERS = "Actors\\jason_myers";
    public const string ACTOR_BIG_BEAR = "Actors\\big_bear";
    public const string ACTOR_FAMU_FATARU = "Actors\\famu_fataru";
    public const string ACTOR_SANTAMAN = "Actors\\santaman";
    public const string ACTOR_ROGUEDJACK = "Actors\\roguedjack";
    public const string ACTOR_DUCKMAN = "Actors\\duckman";
    public const string ACTOR_HANS_VON_HANZ = "Actors\\hans_von_hanz";
    public const string ACTOR_FATHER_TIME = "Actors\\father_time";
    // Father Time's icon spec: male_skin5.png male_shoes1.png female_pants2.png male_shirt5.png male_eyes6.png [glasses] male_hair8.png (hat concealing baldness)
    // "shirt" is a black trenchcoat, recolored to 208 248 208 to get a light-green kimono-type outfit
    public const string BLOODIED = "Actors\\Decoration\\bloodied";
    public const string MALE_SKIN1 = "Actors\\Decoration\\male_skin1";
    public const string MALE_SKIN2 = "Actors\\Decoration\\male_skin2";
    public const string MALE_SKIN3 = "Actors\\Decoration\\male_skin3";
    public const string MALE_SKIN4 = "Actors\\Decoration\\male_skin4";
    public const string MALE_SKIN5 = "Actors\\Decoration\\male_skin5";
    public const string MALE_HAIR1 = "Actors\\Decoration\\male_hair1";
    public const string MALE_HAIR2 = "Actors\\Decoration\\male_hair2";
    public const string MALE_HAIR3 = "Actors\\Decoration\\male_hair3";
    public const string MALE_HAIR4 = "Actors\\Decoration\\male_hair4";
    public const string MALE_HAIR5 = "Actors\\Decoration\\male_hair5";
    public const string MALE_HAIR6 = "Actors\\Decoration\\male_hair6";
    public const string MALE_HAIR7 = "Actors\\Decoration\\male_hair7";
    public const string MALE_HAIR8 = "Actors\\Decoration\\male_hair8";
    public const string MALE_SHIRT1 = "Actors\\Decoration\\male_shirt1";
    public const string MALE_SHIRT2 = "Actors\\Decoration\\male_shirt2";
    public const string MALE_SHIRT3 = "Actors\\Decoration\\male_shirt3";
    public const string MALE_SHIRT4 = "Actors\\Decoration\\male_shirt4";
    public const string MALE_SHIRT5 = "Actors\\Decoration\\male_shirt5";
    public const string MALE_PANTS1 = "Actors\\Decoration\\male_pants1";
    public const string MALE_PANTS2 = "Actors\\Decoration\\male_pants2";
    public const string MALE_PANTS3 = "Actors\\Decoration\\male_pants3";
    public const string MALE_PANTS4 = "Actors\\Decoration\\male_pants4";
    public const string MALE_PANTS5 = "Actors\\Decoration\\male_pants5";
    public const string MALE_SHOES1 = "Actors\\Decoration\\male_shoes1";
    public const string MALE_SHOES2 = "Actors\\Decoration\\male_shoes2";
    public const string MALE_SHOES3 = "Actors\\Decoration\\male_shoes3";
    public const string MALE_EYES1 = "Actors\\Decoration\\male_eyes1";
    public const string MALE_EYES2 = "Actors\\Decoration\\male_eyes2";
    public const string MALE_EYES3 = "Actors\\Decoration\\male_eyes3";
    public const string MALE_EYES4 = "Actors\\Decoration\\male_eyes4";
    public const string MALE_EYES5 = "Actors\\Decoration\\male_eyes5";
    public const string MALE_EYES6 = "Actors\\Decoration\\male_eyes6";
    public const string FEMALE_SKIN1 = "Actors\\Decoration\\female_skin1";
    public const string FEMALE_SKIN2 = "Actors\\Decoration\\female_skin2";
    public const string FEMALE_SKIN3 = "Actors\\Decoration\\female_skin3";
    public const string FEMALE_SKIN4 = "Actors\\Decoration\\female_skin4";
    public const string FEMALE_SKIN5 = "Actors\\Decoration\\female_skin5";
    public const string FEMALE_HAIR1 = "Actors\\Decoration\\female_hair1";
    public const string FEMALE_HAIR2 = "Actors\\Decoration\\female_hair2";
    public const string FEMALE_HAIR3 = "Actors\\Decoration\\female_hair3";
    public const string FEMALE_HAIR4 = "Actors\\Decoration\\female_hair4";
    public const string FEMALE_HAIR5 = "Actors\\Decoration\\female_hair5";
    public const string FEMALE_HAIR6 = "Actors\\Decoration\\female_hair6";
    public const string FEMALE_HAIR7 = "Actors\\Decoration\\female_hair7";
    public const string FEMALE_SHIRT1 = "Actors\\Decoration\\female_shirt1";
    public const string FEMALE_SHIRT2 = "Actors\\Decoration\\female_shirt2";
    public const string FEMALE_SHIRT3 = "Actors\\Decoration\\female_shirt3";
    public const string FEMALE_SHIRT4 = "Actors\\Decoration\\female_shirt4";
    public const string FEMALE_PANTS1 = "Actors\\Decoration\\female_pants1";
    public const string FEMALE_PANTS2 = "Actors\\Decoration\\female_pants2";
    public const string FEMALE_PANTS3 = "Actors\\Decoration\\female_pants3";
    public const string FEMALE_PANTS4 = "Actors\\Decoration\\female_pants4";
    public const string FEMALE_PANTS5 = "Actors\\Decoration\\female_pants5";
    public const string FEMALE_SHOES1 = "Actors\\Decoration\\female_shoes1";
    public const string FEMALE_SHOES2 = "Actors\\Decoration\\female_shoes2";
    public const string FEMALE_SHOES3 = "Actors\\Decoration\\female_shoes3";
    public const string FEMALE_EYES1 = "Actors\\Decoration\\female_eyes1";
    public const string FEMALE_EYES2 = "Actors\\Decoration\\female_eyes2";
    public const string FEMALE_EYES3 = "Actors\\Decoration\\female_eyes3";
    public const string FEMALE_EYES4 = "Actors\\Decoration\\female_eyes4";
    public const string FEMALE_EYES5 = "Actors\\Decoration\\female_eyes5";
    public const string FEMALE_EYES6 = "Actors\\Decoration\\female_eyes6";
    public const string ARMY_HELMET = "Actors\\Decoration\\army_helmet";
    public const string ARMY_PANTS = "Actors\\Decoration\\army_pants";
    public const string ARMY_SHIRT = "Actors\\Decoration\\army_shirt";
    public const string ARMY_SHOES = "Actors\\Decoration\\army_shoes";
    public const string BIKER_HAIR1 = "Actors\\Decoration\\biker_hair1";
    public const string BIKER_HAIR2 = "Actors\\Decoration\\biker_hair2";
    public const string BIKER_HAIR3 = "Actors\\Decoration\\biker_hair3";
    public const string BIKER_PANTS = "Actors\\Decoration\\biker_pants";
    public const string BIKER_SHOES = "Actors\\Decoration\\biker_shoes";
    public const string GANGSTA_HAT = "Actors\\Decoration\\gangsta_hat";
    public const string GANGSTA_PANTS = "Actors\\Decoration\\gangsta_pants";
    public const string GANGSTA_SHIRT = "Actors\\Decoration\\gangsta_shirt";
    public const string CHARGUARD_HAIR = "Actors\\Decoration\\charguard_hair";
    public const string CHARGUARD_PANTS = "Actors\\Decoration\\charguard_pants";
    public const string POLICE_HAT = "Actors\\Decoration\\police_hat";
    public const string POLICE_UNIFORM = "Actors\\Decoration\\police_uniform";
    public const string POLICE_PANTS = "Actors\\Decoration\\police_pants";
    public const string POLICE_SHOES = "Actors\\Decoration\\police_shoes";
    public const string BLACKOP_SUIT = "Actors\\Decoration\\blackop_suit";
    public const string HOSPITAL_DOCTOR_UNIFORM = "Actors\\Decoration\\hospital_doctor_uniform";
    public const string HOSPITAL_NURSE_UNIFORM = "Actors\\Decoration\\hospital_nurse_uniform";
    public const string HOSPITAL_PATIENT_UNIFORM = "Actors\\Decoration\\hospital_patient_uniform";
    public const string SURVIVOR_MALE_BANDANA = "Actors\\Decoration\\survivor_male_bandana";
    public const string SURVIVOR_FEMALE_BANDANA = "Actors\\Decoration\\survivor_female_bandana";
    public const string DOG_SKIN1 = "Actors\\Decoration\\dog_skin1";
    public const string DOG_SKIN2 = "Actors\\Decoration\\dog_skin2";
    public const string DOG_SKIN3 = "Actors\\Decoration\\dog_skin3";
    public const string ITEM_SLOT = "Items\\itemslot";
    public const string ITEM_EQUIPPED = "Items\\itemequipped";  // XXX should be CGI
    public const string ITEM_AMMO_LIGHT_PISTOL = "Items\\item_ammo_light_pistol";
    public const string ITEM_AMMO_HEAVY_PISTOL = "Items\\item_ammo_heavy_pistol";
    public const string ITEM_AMMO_LIGHT_RIFLE = "Items\\item_ammo_light_rifle";
    public const string ITEM_AMMO_HEAVY_RIFLE = "Items\\item_ammo_heavy_rifle";
    public const string ITEM_AMMO_SHOTGUN = "Items\\item_ammo_shotgun";
    public const string ITEM_AMMO_BOLTS = "Items\\item_ammo_bolts";
    public const string ITEM_ARMY_BODYARMOR = "Items\\item_army_bodyarmor";
    public const string ITEM_ARMY_PISTOL = "Items\\item_army_pistol";
    public const string ITEM_ARMY_RATION = "Items\\item_army_ration";
    public const string ITEM_ARMY_RIFLE = "Items\\item_army_rifle";
    public const string ITEM_BANDAGES = "Items\\item_bandages";
    public const string ITEM_BARBED_WIRE = "Items\\item_barbed_wire";
    public const string ITEM_BEAR_TRAP = "Items\\item_bear_trap";
    public const string ITEM_BASEBALL_BAT = "Items\\item_baseballbat";
    public const string ITEM_BIGBEAR_BAT = "Items\\item_bigbear_bat";
    public const string ITEM_BIG_FLASHLIGHT = "Items\\item_big_flashlight";
    public const string ITEM_BIG_FLASHLIGHT_OUT = "Items\\item_big_flashlight_out";
    public const string ITEM_BOOK = "Items\\item_book";
    public const string ITEM_BLACKOPS_GPS = "Items\\item_blackops_gps";
    public const string ITEM_CANNED_FOOD = "Items\\item_canned_food";
    public const string ITEM_CELL_PHONE = "Items\\item_cellphone";
    public const string ITEM_CHAR_LIGHT_BODYARMOR = "Items\\item_CHAR_light_bodyarmor";
    public const string ITEM_CROWBAR = "Items\\item_crowbar";
    public const string ITEM_COMBAT_KNIFE = "Items\\item_combat_knife";
    public const string ITEM_EMPTY_CAN = "Items\\item_empty_can";
    public const string ITEM_FAMU_FATARU_KATANA = "Items\\item_famu_fataru_katana";
    public const string ITEM_FATHER_TIME_SCYTHE = "Items\\item_father_time_scythe";
    public const string ITEM_FLASHLIGHT = "Items\\item_flashlight";
    public const string ITEM_FLASHLIGHT_OUT = "Items\\item_flashlight_out";
    public const string ITEM_FREE_ANGELS_JACKET = "Items\\item_free_angels_jacket";
    public const string ITEM_GRENADE = "Items\\item_grenade";
    public const string ITEM_GRENADE_PRIMED = "Items\\item_grenade_primed";
    public const string ITEM_JASON_MYERS_AXE = "Items\\item_jason_myers_axe";
    public const string ITEM_GOLF_CLUB = "Items\\item_golfclub";
    public const string ITEM_GROCERIES = "Items\\item_groceries";
    public const string ITEM_HANS_VON_HANZ_PISTOL = "Items\\item_hans_von_hanz_pistol";
    public const string ITEM_HELLS_SOULS_JACKET = "Items\\item_hells_souls_jacket";
    public const string ITEM_HUGE_HAMMER = "Items\\item_huge_hammer";
    public const string ITEM_HUNTER_VEST = "Items\\item_hunter_vest";
    public const string ITEM_HUNTING_CROSSBOW = "Items\\item_hunting_crossbow";
    public const string ITEM_HUNTING_RIFLE = "Items\\item_hunting_rifle";
    public const string ITEM_IMPROVISED_CLUB = "Items\\item_improvised_club";
    public const string ITEM_IMPROVISED_SPEAR = "Items\\item_improvised_spear";
    public const string ITEM_IRON_GOLF_CLUB = "Items\\item_iron_golfclub";
    public const string ITEM_KOLT_REVOLVER = "Items\\item_kolt_revolver";
    public const string ITEM_MAGAZINE = "Items\\item_magazine";
    public const string ITEM_MEDIKIT = "Items\\item_medikit";
    public const string ITEM_PISTOL = "Items\\item_pistol";
    public const string ITEM_PILLS_ANTIVIRAL = "Items\\item_pills_antiviral";
    public const string ITEM_PILLS_BLUE = "Items\\item_pills_blue";
    public const string ITEM_PILLS_GREEN = "Items\\item_pills_green";
    public const string ITEM_PILLS_SAN = "Items\\item_pills_san";
    public const string ITEM_POLICE_JACKET = "Items\\item_police_jacket";
    public const string ITEM_POLICE_RADIO = "Items\\item_police_radio";
    public const string ITEM_POLICE_RIOT_ARMOR = "Items\\item_police_riot_armor";
    public const string ITEM_PRECISION_RIFLE = "Items\\item_precision_rifle";
    public const string ITEM_ROGUEDJACK_KEYBOARD = "Items\\item_roguedjack_keyboard";
    public const string ITEM_SANTAMAN_SHOTGUN = "Items\\item_santaman_shotgun";
    public const string ITEM_SHOTGUN = "Items\\item_shotgun";
    public const string ITEM_SHOVEL = "Items\\item_shovel";
    public const string ITEM_SMALL_HAMMER = "Items\\item_small_hammer";
    public const string ITEM_SPIKES = "Items\\item_spikes";
    public const string ITEM_SHORT_SHOVEL = "Items\\item_short_shovel";
    public const string ITEM_SPRAYPAINT = "Items\\item_spraypaint";
    public const string ITEM_SPRAYPAINT2 = "Items\\item_spraypaint2";
    public const string ITEM_SPRAYPAINT3 = "Items\\item_spraypaint3";
    public const string ITEM_SPRAYPAINT4 = "Items\\item_spraypaint4";
    public const string ITEM_STENCH_KILLER = "Items\\item_stench_killer";
    public const string ITEM_SUBWAY_BADGE = "Items\\item_subway_badge";
    public const string ITEM_TRUNCHEON = "Items\\item_truncheon";
    public const string ITEM_WOODEN_PLANK = "Items\\item_wooden_plank";
    public const string ITEM_ZTRACKER = "Items\\item_ztracker";
    public const string EFFECT_BARRICADED = "Effects\\barricaded";
    public const string EFFECT_ONFIRE = "Effects\\onFire";
    public const string UNDEF = "undef";
    public const string MAP_EXIT = "map_exit";
    public const string MINI_PLAYER_POSITION = "mini_player_position";
    public const string MINI_PLAYER_TAG1 = "mini_player_tag";
    public const string MINI_PLAYER_TAG2 = "mini_player_tag2";
    public const string MINI_PLAYER_TAG3 = "mini_player_tag3";
    public const string MINI_PLAYER_TAG4 = "mini_player_tag4";
    public const string MINI_FOLLOWER_POSITION = "mini_follower_position";
    public const string MINI_UNDEAD_POSITION = "mini_undead_position";
    public const string MINI_BLACKOPS_POSITION = "mini_blackops_position";
    public const string MINI_POLICE_POSITION = "mini_police_position";
    public const string TRACK_FOLLOWER_POSITION = "track_follower_position";
    public const string TRACK_UNDEAD_POSITION = "track_undead_position";
    public const string TRACK_BLACKOPS_POSITION = "track_blackops_position";
    public const string TRACK_POLICE_POSITION = "track_police_position";
    public const string TRACK_POLICE_CIVILIAN_POSITION = "track_police_civilian_position";
    public const string TRACK_POLICE_DEPUTY_POSITION = "track_police_deputy_position";
    public const string TRACK_POLICE_MURDERER_POSITION = "track_police_murderer_position";
    public const string TRACK_POLICE_EN_CIVILIAN_POSITION = "track_police_en_civilian_position";
    public const string TRACK_POLICE_EN_MURDERER_POSITION = "track_police_en_murderer_position";
    public const string WEATHER_RAIN1 = "weather_rain1";
    public const string WEATHER_RAIN2 = "weather_rain2";
    public const string WEATHER_HEAVY_RAIN1 = "weather_heavy_rain1";
    public const string WEATHER_HEAVY_RAIN2 = "weather_heavy_rain2";
    public const string CORPSE_DRAGGED = "corpse_dragged";  // XXX should be CGI
    public const string ROT1_1 = "rot1_1";
    public const string ROT1_2 = "rot1_2";
    public const string ROT2_1 = "rot2_1";
    public const string ROT2_2 = "rot2_2";
    public const string ROT3_1 = "rot3_1";
    public const string ROT3_2 = "rot3_2";
    public const string ROT4_1 = "rot4_1";
    public const string ROT4_2 = "rot4_2";
    public const string ROT5_1 = "rot5_1";
    public const string ROT5_2 = "rot5_2";
    private const string FOLDER = "Resources\\Images\\";

    // monochrome overlays
    public const string THREAT_OVERLAY = "threat";
    public const string TOURISM_OVERLAY = "tourism";
    public const string ITEMS_OVERLAY = "items";
    public const string THREAT_AND_TOURISM_OVERLAY = "threat_tourism";
    public const string LINE_OF_FIRE_OVERLAY = "lof";

    public static void LoadResources(IRogueUI ui)
    {
      s_Mods.AddRange(ui.Mods); // get mod list from ui

      Notify(ui, "icons...");
      Load(ACTIVITY_CHASING);
      Load(ACTIVITY_CHASING_PLAYER);
      Load(ACTIVITY_TRACKING);
      Load(ACTIVITY_FLEEING);
      Load(ACTIVITY_FLEEING_FROM_EXPLOSIVE);
      Load(ACTIVITY_FOLLOWING);
      Load(ACTIVITY_FOLLOWING_ORDER);
      Load(ACTIVITY_FOLLOWING_LEADER);
      Load(ACTIVITY_FOLLOWING_PLAYER);
      Load(ACTIVITY_SLEEPING);
      Load(ICON_BLAST);
      Load(ICON_CAN_TRADE);
      Load(ICON_THREAT_SAFE);
      Load(ICON_THREAT_DANGER);
      Load(ICON_THREAT_HIGH_DANGER);
      Load(ICON_CANT_RUN);
      Load(ICON_EXPIRED_FOOD);
      Load(ICON_SPOILED_FOOD);
      Load(ICON_FOOD_ALMOST_HUNGRY);
      Load(ICON_FOOD_HUNGRY);
      Load(ICON_FOOD_STARVING);
      Load(ICON_HEALING);
      Load(ICON_IS_TARGET);
      Load(ICON_IS_TARGETTED);
      Load(ICON_IS_TARGETING);  // alpha10
      Load(ICON_IS_IN_GROUP);  // alpha10
      Load(ICON_KILLED);
      Load(ICON_LEADER);
      Load(ICON_MELEE_ATTACK);
      Load(ICON_MELEE_MISS);
      Load(ICON_MELEE_DAMAGE);
      Load(ICON_OUT_OF_AMMO);
      Load(ICON_OUT_OF_BATTERIES);
      Load(ICON_RANGED_ATTACK);
      Load(ICON_RANGED_MISS);
      Load(ICON_RANGED_DAMAGE);
#if CGI_ICONS
#else
      Load(ICON_RUNNING);
#endif
      Load(ICON_ROT_ALMOST_HUNGRY);
      Load(ICON_ROT_HUNGRY);
      Load(ICON_ROT_STARVING);
      Load(ICON_SLEEP_ALMOST_SLEEPY);
      Load(ICON_SLEEP_SLEEPY);
      Load(ICON_SLEEP_EXHAUSTED);
      Load(ICON_TARGET);
      Load(ICON_LINE_BAD);
      Load(ICON_LINE_BLOCKED);
      Load(ICON_LINE_CLEAR);
      Load(ICON_SCENT_LIVING);
      Load(ICON_SCENT_ZOMBIEMASTER);
      //Load(ICON_SCENT_LIVING_SUPRESSOR); // alpha10 obsolete; spellcheck
      Load(ICON_ODOR_SUPPRESSED);  // alpha10
      Load(ICON_AGGRESSOR);
      Load(ICON_INDIRECT_ENEMIES);
      Load(ICON_SELF_DEFENCE);
      Load(ICON_TRAP_ACTIVATED);
      Load(ICON_TRAP_ACTIVATED_SAFE_GROUP);  // alpha10
      Load(ICON_TRAP_ACTIVATED_SAFE_PLAYER);  // alpha10
      Load(ICON_TRAP_TRIGGERED);
      Load(ICON_TRAP_TRIGGERED_SAFE_GROUP);  // alpha10
      Load(ICON_TRAP_TRIGGERED_SAFE_PLAYER);  // alpha10
      Load(ICON_SANITY_DISTURBED);
      Load(ICON_SANITY_INSANE);
      Load(ICON_BORING_ITEM);
      Load(ICON_ZGRAB);  // alpha10
      Notify(ui, "tiles...");
      Load(TILE_FLOOR_ASPHALT);
      Load(TILE_FLOOR_CONCRETE);
      Load(TILE_FLOOR_GRASS);
      Load(TILE_FLOOR_OFFICE);
      Load(TILE_FLOOR_PLANKS);
      Load(TILE_FLOOR_SEWER_WATER);
      Load(TILE_FLOOR_SEWER_WATER_ANIM1);
      Load(TILE_FLOOR_SEWER_WATER_ANIM2);
      Load(TILE_FLOOR_SEWER_WATER_ANIM3);
      Load(TILE_FLOOR_SEWER_WATER_COVER);
      Load(TILE_FLOOR_TILES);
      Load(TILE_FLOOR_WALKWAY);
      Load(TILE_ROAD_ASPHALT_EW);
      Load(TILE_ROAD_ASPHALT_SWNE);
      Load(TILE_RAIL_EW);
      Load(TILE_WALL_BRICK);
      Load(TILE_WALL_CHAR_OFFICE);
      Load(TILE_WALL_HOSPITAL);
      Load(TILE_WALL_SEWER);
      Load(TILE_WALL_STONE);
      Load(TILE_RAIL_SWNE);

      Notify(ui, "tile decorations...");
      Load(DECO_BLOODIED_FLOOR);
      Load(DECO_BLOODIED_WALL);
      Load(DECO_ZOMBIE_REMAINS);
      Load(DECO_VOMIT);
      Load(DECO_POSTERS1);
      Load(DECO_POSTERS2);
      Load(DECO_TAGS1);
      Load(DECO_TAGS2);
      Load(DECO_TAGS3);
      Load(DECO_TAGS4);
      Load(DECO_TAGS5);
      Load(DECO_TAGS6);
      Load(DECO_TAGS7);
      Load(DECO_SHOP_CONSTRUCTION);
      Load(DECO_SHOP_GENERAL_STORE);
      Load(DECO_SHOP_GROCERY);
      Load(DECO_SHOP_GUNSHOP);
      Load(DECO_SHOP_PHARMACY);
      Load(DECO_SHOP_SPORTSWEAR);
      Load(DECO_SHOP_HUNTING);
      Load(DECO_CHAR_OFFICE);
      Load(DECO_CHAR_FLOOR_LOGO);
      Load(DECO_CHAR_POSTER1);
      Load(DECO_CHAR_POSTER2);
      Load(DECO_CHAR_POSTER3);
      Load(DECO_PLAYER_TAG1);
      Load(DECO_PLAYER_TAG2);
      Load(DECO_PLAYER_TAG3);
      Load(DECO_PLAYER_TAG4);
      Load(DECO_ROGUEDJACK_TAG);
      Load(DECO_SEWER_LADDER);
      Load(DECO_SEWER_HOLE);
      Load(DECO_SEWERS_BUILDING);
      Load(DECO_SUBWAY_BUILDING);
      Load(DECO_STAIRS_UP);
      Load(DECO_STAIRS_DOWN);
      Load(DECO_POWER_SIGN_BIG);
      Load(DECO_POLICE_STATION);
      Load(DECO_HOSPITAL);
      Notify(ui, "map objects...");
      Load(OBJ_TREE);
      Load(OBJ_WOODEN_DOOR_CLOSED);
      Load(OBJ_WOODEN_DOOR_OPEN);
      Load(OBJ_WOODEN_DOOR_BROKEN);
      Load(OBJ_GLASS_DOOR_CLOSED);
      Load(OBJ_GLASS_DOOR_OPEN);
      Load(OBJ_GLASS_DOOR_BROKEN);
      Load(OBJ_CHAR_DOOR_CLOSED);
      Load(OBJ_CHAR_DOOR_OPEN);
      Load(OBJ_CHAR_DOOR_BROKEN);
      Load(OBJ_WINDOW_CLOSED);
      Load(OBJ_WINDOW_OPEN);
      Load(OBJ_WINDOW_BROKEN);
      Load(OBJ_BENCH);
      Load(OBJ_FENCE);
      Load(OBJ_CAR1);
      Load(OBJ_CAR2);
      Load(OBJ_CAR3);
      Load(OBJ_CAR4);
      Load(OBJ_SHOP_SHELF);
      Load(OBJ_BED);
      Load(OBJ_WARDROBE);
      Load(OBJ_TABLE);
      Load(OBJ_FRIDGE);
      Load(OBJ_DRAWER);
      Load(OBJ_CHAIR);
      Load(OBJ_NIGHT_TABLE);
      Load(OBJ_CHAR_CHAIR);
      Load(OBJ_CHAR_TABLE);
      Load(OBJ_IRON_BENCH);
      Load(OBJ_IRON_DOOR_OPEN);
      Load(OBJ_IRON_DOOR_CLOSED);
      Load(OBJ_IRON_DOOR_BROKEN);
      Load(OBJ_IRON_FENCE);
      Load(OBJ_BARRELS);
      Load(OBJ_JUNK);
      Load(OBJ_POWERGEN_OFF);
      Load(OBJ_POWERGEN_ON);
      Load(OBJ_GATE_CLOSED);
      Load(OBJ_GATE_OPEN);
      Load(OBJ_BOARD);
      Load(OBJ_SMALL_WOODEN_FORTIFICATION);
      Load(OBJ_LARGE_WOODEN_FORTIFICATION);
      Load(OBJ_HOSPITAL_BED);
      Load(OBJ_HOSPITAL_CHAIR);
      Load(OBJ_HOSPITAL_NIGHT_TABLE);
      Load(OBJ_HOSPITAL_WARDROBE);
      Load(OBJ_HOSPITAL_DOOR_OPEN);
      Load(OBJ_HOSPITAL_DOOR_CLOSED);
      Load(OBJ_HOSPITAL_DOOR_BROKEN);
      Load(OBJ_GARDEN_FENCE);   // alpha 10
      Load(OBJ_WIRE_FENCE);   // alpha 10
      Notify(ui, "actors...");
#if CGI_ICONS
#else
      Load(PLAYER_FOLLOWER);
      Load(PLAYER_FOLLOWER_TRUST);
#endif
      Load(PLAYER_FOLLOWER_BOND);
      Load(ACTOR_SKELETON);
      Load(ACTOR_RED_EYED_SKELETON);
      Load(ACTOR_RED_SKELETON);
      Load(ACTOR_ZOMBIE);
      Load(ACTOR_DARK_EYED_ZOMBIE);
      Load(ACTOR_DARK_ZOMBIE);
      Load(ACTOR_MALE_NEOPHYTE);
      Load(ACTOR_FEMALE_NEOPHYTE);
      Load(ACTOR_MALE_DISCIPLE);
      Load(ACTOR_FEMALE_DISCIPLE);
      Load(ACTOR_ZOMBIE_MASTER);
      Load(ACTOR_ZOMBIE_LORD);
      Load(ACTOR_ZOMBIE_PRINCE);
      Load(ACTOR_RAT_ZOMBIE);
      Load(ACTOR_SEWERS_THING);
      Load(ACTOR_JASON_MYERS);
      Load(ACTOR_BIG_BEAR);
      Load(ACTOR_FAMU_FATARU);
      Load(ACTOR_SANTAMAN);
      Load(ACTOR_ROGUEDJACK);
      Load(ACTOR_DUCKMAN);
      Load(ACTOR_HANS_VON_HANZ);
      Load(ACTOR_FATHER_TIME);
      Notify(ui, "actor decorations...");
      Load(BLOODIED);
      Load(MALE_SKIN1);
      Load(MALE_SKIN2);
      Load(MALE_SKIN3);
      Load(MALE_SKIN4);
      Load(MALE_SKIN5);
      Load(MALE_HAIR1);
      Load(MALE_HAIR2);
      Load(MALE_HAIR3);
      Load(MALE_HAIR4);
      Load(MALE_HAIR5);
      Load(MALE_HAIR6);
      Load(MALE_HAIR7);
      Load(MALE_HAIR8);
      Load(MALE_SHIRT1);
      Load(MALE_SHIRT2);
      Load(MALE_SHIRT3);
      Load(MALE_SHIRT4);
      Load(MALE_SHIRT5);
      Load(MALE_PANTS1);
      Load(MALE_PANTS2);
      Load(MALE_PANTS3);
      Load(MALE_PANTS4);
      Load(MALE_PANTS5);
      Load(MALE_SHOES1);
      Load(MALE_SHOES2);
      Load(MALE_SHOES3);
      Load(MALE_EYES1);
      Load(MALE_EYES2);
      Load(MALE_EYES3);
      Load(MALE_EYES4);
      Load(MALE_EYES5);
      Load(MALE_EYES6);
      Load(FEMALE_SKIN1);
      Load(FEMALE_SKIN2);
      Load(FEMALE_SKIN3);
      Load(FEMALE_SKIN4);
      Load(FEMALE_SKIN5);
      Load(FEMALE_HAIR1);
      Load(FEMALE_HAIR2);
      Load(FEMALE_HAIR3);
      Load(FEMALE_HAIR4);
      Load(FEMALE_HAIR5);
      Load(FEMALE_HAIR6);
      Load(FEMALE_HAIR7);
      Load(FEMALE_SHIRT1);
      Load(FEMALE_SHIRT2);
      Load(FEMALE_SHIRT3);
      Load(FEMALE_SHIRT4);
      Load(FEMALE_PANTS1);
      Load(FEMALE_PANTS2);
      Load(FEMALE_PANTS3);
      Load(FEMALE_PANTS4);
      Load(FEMALE_PANTS5);
      Load(FEMALE_SHOES1);
      Load(FEMALE_SHOES2);
      Load(FEMALE_SHOES3);
      Load(FEMALE_EYES1);
      Load(FEMALE_EYES2);
      Load(FEMALE_EYES3);
      Load(FEMALE_EYES4);
      Load(FEMALE_EYES5);
      Load(FEMALE_EYES6);
      Load(ARMY_HELMET);
      Load(ARMY_PANTS);
      Load(ARMY_SHIRT);
      Load(ARMY_SHOES);
      Load(BIKER_HAIR1);
      Load(BIKER_HAIR2);
      Load(BIKER_HAIR3);
      Load(BIKER_PANTS);
      Load(BIKER_SHOES);
      Load(GANGSTA_HAT);
      Load(GANGSTA_PANTS);
      Load(GANGSTA_SHIRT);
      Load(CHARGUARD_HAIR);
      Load(CHARGUARD_PANTS);
      Load(POLICE_HAT);
      Load(POLICE_UNIFORM);
      Load(POLICE_PANTS);
      Load(POLICE_SHOES);
      Load(BLACKOP_SUIT);
      Load(HOSPITAL_DOCTOR_UNIFORM);
      Load(HOSPITAL_NURSE_UNIFORM);
      Load(HOSPITAL_PATIENT_UNIFORM);
      Load(SURVIVOR_MALE_BANDANA);
      Load(SURVIVOR_FEMALE_BANDANA);
      Load(DOG_SKIN1);
      Load(DOG_SKIN2);
      Load(DOG_SKIN3);
      Notify(ui, "items...");
#if CGI_ICONS
#else
      Load(ITEM_SLOT);
      Load(ITEM_EQUIPPED);
#endif
      Load(ITEM_AMMO_LIGHT_PISTOL);
      Load(ITEM_AMMO_HEAVY_PISTOL);
      Load(ITEM_AMMO_LIGHT_RIFLE);
      Load(ITEM_AMMO_HEAVY_RIFLE);
      Load(ITEM_AMMO_SHOTGUN);
      Load(ITEM_AMMO_BOLTS);
      Load(ITEM_ARMY_BODYARMOR);
      Load(ITEM_ARMY_PISTOL);
      Load(ITEM_ARMY_RATION);
      Load(ITEM_ARMY_RIFLE);
      Load(ITEM_BANDAGES);
      Load(ITEM_BARBED_WIRE);
      Load(ITEM_BEAR_TRAP);
      Load(ITEM_BASEBALL_BAT);
      Load(ITEM_BIGBEAR_BAT);
      Load(ITEM_BIG_FLASHLIGHT);
      Load(ITEM_BIG_FLASHLIGHT_OUT);
      Load(ITEM_BOOK);
      Load(ITEM_BLACKOPS_GPS);
      Load(ITEM_CANNED_FOOD);
      Load(ITEM_CELL_PHONE);
      Load(ITEM_CHAR_LIGHT_BODYARMOR);
      Load(ITEM_CROWBAR);
      Load(ITEM_COMBAT_KNIFE);
      Load(ITEM_EMPTY_CAN);
      Load(ITEM_FAMU_FATARU_KATANA);
      Load(ITEM_FATHER_TIME_SCYTHE);
      Load(ITEM_FLASHLIGHT);
      Load(ITEM_FLASHLIGHT_OUT);
      Load(ITEM_FREE_ANGELS_JACKET);
      Load(ITEM_GRENADE);
      Load(ITEM_GRENADE_PRIMED);
      Load(ITEM_JASON_MYERS_AXE);
      Load(ITEM_GOLF_CLUB);
      Load(ITEM_GROCERIES);
      Load(ITEM_HANS_VON_HANZ_PISTOL);
      Load(ITEM_HELLS_SOULS_JACKET);
      Load(ITEM_HUGE_HAMMER);
      Load(ITEM_HUNTER_VEST);
      Load(ITEM_HUNTING_CROSSBOW);
      Load(ITEM_HUNTING_RIFLE);
      Load(ITEM_IMPROVISED_CLUB);
      Load(ITEM_IMPROVISED_SPEAR);
      Load(ITEM_IRON_GOLF_CLUB);
      Load(ITEM_KOLT_REVOLVER);
      Load(ITEM_MAGAZINE);
      Load(ITEM_MEDIKIT);
      Load(ITEM_PISTOL);
      Load(ITEM_PILLS_ANTIVIRAL);
      Load(ITEM_PILLS_BLUE);
      Load(ITEM_PILLS_GREEN);
      Load(ITEM_PILLS_SAN);
      Load(ITEM_POLICE_JACKET);
      Load(ITEM_POLICE_RADIO);
      Load(ITEM_POLICE_RIOT_ARMOR);
      Load(ITEM_PRECISION_RIFLE);
      Load(ITEM_ROGUEDJACK_KEYBOARD);
      Load(ITEM_SANTAMAN_SHOTGUN);
      Load(ITEM_SHOTGUN);
      Load(ITEM_SHOVEL);
      Load(ITEM_SMALL_HAMMER);
      Load(ITEM_SPIKES);
      Load(ITEM_SHORT_SHOVEL);
      Load(ITEM_SPRAYPAINT);
      Load(ITEM_SPRAYPAINT2);
      Load(ITEM_SPRAYPAINT3);
      Load(ITEM_SPRAYPAINT4);
      Load(ITEM_STENCH_KILLER);
      Load(ITEM_SUBWAY_BADGE);
      Load(ITEM_TRUNCHEON);
      Load(ITEM_WOODEN_PLANK);
      Load(ITEM_ZTRACKER);
      Notify(ui, "effects...");
      Load(EFFECT_BARRICADED);
      Load(EFFECT_ONFIRE);
      Notify(ui, "misc...");
      Load(UNDEF);
      Load(MAP_EXIT);
      Load(MINI_PLAYER_POSITION);
      Load(MINI_PLAYER_TAG1);
      Load(MINI_PLAYER_TAG2);
      Load(MINI_PLAYER_TAG3);
      Load(MINI_PLAYER_TAG4);
      Load(MINI_FOLLOWER_POSITION);
      Load(MINI_UNDEAD_POSITION);
      Load(MINI_BLACKOPS_POSITION);
      Load(MINI_POLICE_POSITION);
      Load(TRACK_FOLLOWER_POSITION);
      Load(TRACK_UNDEAD_POSITION);
      Load(TRACK_BLACKOPS_POSITION);
#if CGI_ICONS
#else
      Load(TRACK_POLICE_POSITION);
#endif
      Load(WEATHER_RAIN1);
      Load(WEATHER_RAIN2);
      Load(WEATHER_HEAVY_RAIN1);
      Load(WEATHER_HEAVY_RAIN2);
      Load(CORPSE_DRAGGED);
      Load(ROT1_1);
      Load(ROT1_2);
      Load(ROT2_1);
      Load(ROT2_2);
      Load(ROT3_1);
      Load(ROT3_2);
      Load(ROT4_1);
      Load(ROT4_2);
      Load(ROT5_1);
      Load(ROT5_2);
      Notify(ui, "CGI...");
      // 2018-07-28: left spacing of stripes on road tiles as-is; true equal gaps visually would be 3-6-3 rather than 4-4-4, 
      // but that amkes it harder to see the underlying grid.  Gray shade here was easy to get to from MS Paint 3D but not MS Paint.
      // 2018-07-30: concrete: 175 175 175 is good match
      // walkway: MS Paint medium gray 128 128 128
      // main color floor tiles: 208 208 208 (consider for inner hospital drop shadow)
      // main color office tiles, hospital: 230 230 230
      // border for office, floor tiles, hospital: MS Paint medium gray 127 127 127; consider for intermediate hospital drop shadow
      // outer border for hospital: 64 64 64
      // inner drop shadow for hospital: 192 192 192
      // 2018-07-31: EW railway: 64 64 64 drop shadow; 160 160 160 outer rail; 127 127 17 (stock dark gray) central bar; 100 100 80 ground under railway.
      MonochromeTile(THREAT_OVERLAY, Color.FromArgb(0x32ff0000));
      MonochromeTile(TOURISM_OVERLAY, Color.FromArgb(0x320000ff));
      MonochromeTile(ITEMS_OVERLAY, Color.FromArgb(0x3200ff00));
      MonochromeTile(THREAT_AND_TOURISM_OVERLAY, Color.FromArgb(0x32ff00ff));
      MonochromeTile(LINE_OF_FIRE_OVERLAY, Color.FromArgb(0x32ff7f00));
#if CGI_ICONS
      // cf competing implementation : RogueGame::OverlayRect class
      MonochromeBorderTile(ITEM_EQUIPPED, Color.FromArgb(0x26,0x80,0), Color.FromArgb(0x4c,0xff,0));    // PNG measured background was a5h ffh 7fh, no easy way to reconcile with screen
      MonochromeBorderTile(PLAYER_FOLLOWER, Color.Transparent, Color.FromArgb(0x00,0x99,0x99));
      MonochromeBorderTile(PLAYER_FOLLOWER_TRUST, Color.Transparent, Color.Cyan);
      MonochromeDropshadowTile(ITEM_SLOT, Color.Transparent, Color.Silver, Color.Gray);
/*
running icon: #4CFF00
can't run icon: #FF0000
crouching: #0000FF
 */

      Recolor(ICON_RUNNING, s_Images[ICON_CANT_RUN], new Point(2, 29), Color.FromArgb(0x4C, 0xFF, 0));

      TrackerIcon(TRACK_POLICE_POSITION, Color.White, Color.FromArgb(5, 48, 245), "P");
      // we need more synthetic rail tiles : scaled rotation, or if that is too difficult skew 45 degrees left, skew 45 degrees right
#endif
      Recolor(ICON_CROUCHING, s_Images[ICON_CANT_RUN], new Point(2, 29), Color.FromArgb(0, 0, 0xFF));

      // enhanced police tracker identification.  Only police and deputies see the murderer status
      TrackerIcon(TRACK_POLICE_CIVILIAN_POSITION, Color.White, Color.FromArgb(5, 48, 245), "C");
      TrackerIcon(TRACK_POLICE_DEPUTY_POSITION, Color.White, Color.FromArgb(5, 48, 245), "D");
      TrackerIcon(TRACK_POLICE_MURDERER_POSITION, Color.White, Color.FromArgb(5, 48, 245), "M");
      TrackerIcon(TRACK_POLICE_EN_CIVILIAN_POSITION, Color.Magenta, Color.FromArgb(5, 48, 245), "C"); // these two need blackops background color
      TrackerIcon(TRACK_POLICE_EN_MURDERER_POSITION, Color.Magenta, Color.FromArgb(5, 48, 245), "M");

      // 5 synthetic rail tiles to be constructed from TILE_RAIL_SWNE
      VReflect(TILE_RAIL_SENW,TILE_RAIL_SWNE);
      TLBRSplice(TILE_RAIL_SWNE_WALL_E,TILE_RAIL_SENW, TILE_WALL_BRICK);
      BRTLSplice(TILE_RAIL_SWNE_WALL_W, TILE_WALL_BRICK, TILE_RAIL_SENW);
      BLTRSplice(TILE_RAIL_SENW_WALL_E,TILE_RAIL_SWNE, TILE_WALL_BRICK);
      TRBLSplice(TILE_RAIL_SENW_WALL_W, TILE_WALL_BRICK, TILE_RAIL_SWNE);

      // 5 synthetic road tiles to be constructed from TILE_ROAD_ASPHALT_SWNE
      VReflect(TILE_ROAD_ASPHALT_SENW, TILE_ROAD_ASPHALT_SWNE);
      TLBRSplice(TILE_ROAD_ASPHALT_SWNE_CONCRETE_E, TILE_ROAD_ASPHALT_SENW, TILE_FLOOR_CONCRETE);
      BRTLSplice(TILE_ROAD_ASPHALT_SWNE_CONCRETE_W, TILE_FLOOR_CONCRETE, TILE_ROAD_ASPHALT_SENW);
      BLTRSplice(TILE_ROAD_ASPHALT_SENW_CONCRETE_E, TILE_ROAD_ASPHALT_SWNE, TILE_FLOOR_CONCRETE);
      TRBLSplice(TILE_ROAD_ASPHALT_SENW_CONCRETE_W, TILE_FLOOR_CONCRETE, TILE_ROAD_ASPHALT_SWNE);

      // 4 synthetic tiles to be constructed from grass and concrete
      TLBRSplice(TILE_FLOOR_GRASS_SWNE_CONCRETE_E, TILE_FLOOR_GRASS, TILE_FLOOR_CONCRETE);
      BRTLSplice(TILE_FLOOR_GRASS_SWNE_CONCRETE_W, TILE_FLOOR_CONCRETE, TILE_FLOOR_GRASS);
      BLTRSplice(TILE_FLOOR_GRASS_SENW_CONCRETE_E, TILE_FLOOR_GRASS, TILE_FLOOR_CONCRETE);
      TRBLSplice(TILE_FLOOR_GRASS_SENW_CONCRETE_W, TILE_FLOOR_CONCRETE, TILE_FLOOR_GRASS);

      RotateTile(TILE_ROAD_ASPHALT_NS, TILE_ROAD_ASPHALT_EW);
      RotateTile(TILE_RAIL_NS, TILE_RAIL_EW);
      Notify(ui, "done!");
    }

    private static bool LoadMod(string id)
    {
      // it is ok to not load a mod image (it may not exist)
      foreach(var path in s_Mods) {
        string filename = path + id + ".png";
        try {
#if LINUX
          filename = filename.Replace("\\", "/");
#endif
          Bitmap img = new Bitmap(filename);
          s_Images.Add(id, img);
          s_GrayLevelImages.Add(id, GameImages.MakeGrayLevel(img));
          return true;
        } catch (Exception) {
        }
      }
      return false;
    }

    private static void Load(string id)
    {
      string filename = FOLDER + id + ".png";
      // we must be able to load the base image or else
      try {
#if LINUX
        filename = filename.Replace("\\", "/");
#endif
        Bitmap img = new Bitmap(filename);
        s_Images.Add(id, img);
        s_GrayLevelImages.Add(id, GameImages.MakeGrayLevel(img));
      } catch (Exception) {
        throw new ArgumentException("coud not load image id=" + id + "; file=" + filename);
      }
      LoadMod(id);
    }

    private static void MonochromeTile(string id, Color tint)
    {
      if (LoadMod(id)) return;
      s_Images.Add(id, Zaimoni.Data.ext_Drawing.MonochromeRectangle(tint, RogueGame.TILE_SIZE, RogueGame.TILE_SIZE));
    }

    private static void MonochromeBorderTile(string id, Color background, Color border)
    {
      if (LoadMod(id)) return;
      Bitmap img = ext_Drawing.MonochromeRectangle(background, RogueGame.TILE_SIZE, RogueGame.TILE_SIZE);
      img.HLine(border,0,0,-1);
      img.HLine(border,-1,0,-1);
      img.VLine(border,0,1,-2);
      img.VLine(border,-1,1,-2);
      s_Images.Add(id, img);
    }

    private static void MonochromeDropshadowTile(string id, Color background, Color border, Color shadow)
    {
      if (LoadMod(id)) return;
      Bitmap img = ext_Drawing.MonochromeRectangle(background, RogueGame.TILE_SIZE, RogueGame.TILE_SIZE);
      img.HLine(border,0,0,-1);
      img.HLine(border,-1,0,-1);
      img.VLine(border,0,1,-2);
      img.VLine(border,-1,1,-2);
      img.HLine(shadow,1,1,-2);
      img.VLine(shadow,1,2,-2);
      s_Images.Add(id, img);
    }

    private static void TrackerIcon(string id, Color fg, Color border, string tag) {
      if (LoadMod(id)) return;
      Bitmap img = ext_Drawing.MonochromeRectangle(Color.FromArgb(0, Color.White), RogueGame.TILE_SIZE, RogueGame.TILE_SIZE);
      const int origin = RogueGame.TILE_SIZE / 8;
      const int diameter = RogueGame.TILE_SIZE - (2 * origin);
      img.Ellipse(Color.Gray, origin, origin, diameter, diameter);

      const float radius = (diameter - 1) / 2.0f;
      const float center = radius + origin;

      byte raw_alpha(int x, int y) {
        var x_delta = (float)x - center;
        var y_delta = (float)y - center;
        var scale = Math.Sqrt(x_delta * x_delta + y_delta * y_delta);
        if (radius <= scale) return 0;
        var scale2 = ((radius - scale)/(radius))*255.0 + 0.5;
        return (byte)scale2;
      }

      img.ApplyAlpha(raw_alpha);

      const int origin_border = RogueGame.TILE_SIZE / 4;
      const int diameter_border = RogueGame.TILE_SIZE - (2 * origin_border);
      const int origin_disc = RogueGame.TILE_SIZE / 4+2;
      const int diameter_disc = RogueGame.TILE_SIZE - (2 * origin_disc);

      img.Ellipse(border, origin_border, origin_border, diameter_border, diameter_border);
      img.Ellipse(fg, origin_disc, origin_disc, diameter_disc, diameter_disc);

      using(var g = Graphics.FromImage(img)) {
        g.DrawString(tag, IRogueUI.UI.NormalFont, GDIPlusGameCanvas.GetColorBrush(border), (RogueGame.TILE_SIZE - IRogueUI.LINE_SPACING) / 2, (RogueGame.TILE_SIZE - IRogueUI.LINE_SPACING)/2);
      }

      s_Images.Add(id, img);
    }

    private static void RotateTile(string id, string src)
    {
      Bitmap bitmap = new Bitmap(s_Images[src]);
      bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
      s_Images.Add(id,bitmap);
      bitmap = new Bitmap(s_GrayLevelImages[src]);
      bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
      s_GrayLevelImages.Add(id,bitmap);
    }

    private static void VReflect(string id, string src)
    {
      Bitmap bitmap = new Bitmap(s_Images[src]);
      bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
      s_Images.Add(id,bitmap);
      bitmap = new Bitmap(s_GrayLevelImages[src]);
      bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
      s_GrayLevelImages.Add(id,bitmap);
    }

    private static bool TopLeftInclusive(Point pt) { return pt.X <= pt.Y; }
    private static bool TopLeftExclusive(Point pt) { return pt.X <  pt.Y; }

    private static void TLBRSplice(string id, string lhs, string rhs)
    {
      s_Images[id] = s_Images[lhs].Splice(s_Images[rhs], TopLeftInclusive);
      s_GrayLevelImages[id] = s_GrayLevelImages[lhs].Splice(s_GrayLevelImages[rhs], TopLeftInclusive);
    }

    private static void BRTLSplice(string id, string lhs, string rhs)
    {
      s_Images[id] = s_Images[lhs].Splice(s_Images[rhs], TopLeftExclusive);
      s_GrayLevelImages[id] = s_GrayLevelImages[lhs].Splice(s_GrayLevelImages[rhs], TopLeftExclusive);
    }

    private static bool TopRightInclusive(Point pt) { return pt.X <= ((RogueGame.TILE_SIZE - 1) - pt.Y); }
    private static bool TopRightExclusive(Point pt) { return pt.X <  ((RogueGame.TILE_SIZE - 1) - pt.Y); }

    private static void BLTRSplice(string id, string lhs, string rhs)
    {
      s_Images[id] = s_Images[lhs].Splice(s_Images[rhs], TopRightInclusive);
      s_GrayLevelImages[id] = s_GrayLevelImages[lhs].Splice(s_GrayLevelImages[rhs], TopRightInclusive);
    }

    private static void TRBLSplice(string id, string lhs, string rhs)
    {
      s_Images[id] = s_Images[lhs].Splice(s_Images[rhs], TopRightExclusive);
      s_GrayLevelImages[id] = s_GrayLevelImages[lhs].Splice(s_GrayLevelImages[rhs], TopRightExclusive);
    }

    private static void Recolor(string id_dest, Bitmap src, Point origin, Color color)
    {
      var stage = src.Recolor(origin, color);
      s_Images[id_dest] = stage;
      s_GrayLevelImages[id_dest] = MakeGrayLevel(stage);
    }


    private static Bitmap MakeGrayLevel(Bitmap img)
    {
      const float GRAYLEVEL_DIM_FACTOR = 0.55f;
      Bitmap bitmap = new Bitmap(img);
      for (int x = 0; x < bitmap.Width; ++x) {
        for (int y = 0; y < bitmap.Height; ++y) {
          Color pixel = img.GetPixel(x, y);
          int num = (int) (255 * GRAYLEVEL_DIM_FACTOR * pixel.GetBrightness());
          bitmap.SetPixel(x, y, Color.FromArgb((int) pixel.A, num, num, num));
        }
      }
      return bitmap;
    }

    private static void Notify(IRogueUI ui, string stage) => ui.DrawHeadNote("Loading resources: " + stage);

    public static Image Get(string imageID)
    {
      if (s_Images.TryGetValue(imageID, out Bitmap image)) return image;
#if DEBUG
      throw new InvalidOperationException("missing image '"+imageID+"'");
#else
      return s_Images["undef"];
#endif
    }

    public static Image GetGrayLevel(string imageID)
    {
      if (s_GrayLevelImages.TryGetValue(imageID, out Bitmap image)) return image;
#if DEBUG
      throw new InvalidOperationException("missing image '"+imageID+"'");
#else
      return s_GrayLevelImages["undef"];
#endif
    }
  }
}
