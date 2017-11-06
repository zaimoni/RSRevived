// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.PlayerCommand
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define SUICIDE_BY_LONG_WAIT

namespace djack.RogueSurvivor.Engine
{
  internal enum PlayerCommand
  {
    NONE,
    QUIT_GAME,
    HELP_MODE,
    ADVISOR,
    OPTIONS_MODE,
    KEYBINDING_MODE,
    HINTS_SCREEN_MODE,
    SCREENSHOT,
    SAVE_GAME,
    LOAD_GAME,
    ABANDON_GAME,
    MOVE_N,
    MOVE_NE,
    MOVE_E,
    MOVE_SE,
    MOVE_S,
    MOVE_SW,
    MOVE_W,
    MOVE_NW,
    RUN_TOGGLE,
    WAIT_OR_SELF,
#if SUICIDE_BY_LONG_WAIT
    WAIT_LONG,
#else
    WAIT_LONG_XXX,  // don't want to change enumeration, but this command was a deathtrap by typo
#endif
    BARRICADE_MODE,
    BREAK_MODE,
    BUILD_LARGE_FORTIFICATION,
    BUILD_SMALL_FORTIFICATION,
    CLOSE_DOOR,
    EAT_CORPSE,
    FIRE_MODE,
    GIVE_ITEM,
    INITIATE_TRADE,
    LEAD_MODE,
    MARK_ENEMIES_MODE,
    ORDER_MODE,
    PUSH_MODE,
    REVIVE_CORPSE,
    SHOUT,
    SLEEP,
    SWITCH_PLACE,
    USE_EXIT,
    USE_SPRAY,
    CITY_INFO,
    MESSAGE_LOG,
    ITEM_SLOT_0,
    ITEM_SLOT_1,
    ITEM_SLOT_2,
    ITEM_SLOT_3,
    ITEM_SLOT_4,
    ITEM_SLOT_5,
    ITEM_SLOT_6,
    ITEM_SLOT_7,
    ITEM_SLOT_8,
    ITEM_SLOT_9,
    ITEM_INFO,
    DAIMON_MAP,
    ABANDON_PC,
    ALLIES_INFO,
    FACTION_INFO,
   }
}
