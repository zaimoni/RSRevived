// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.InputTranslator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Windows.Forms;

namespace djack.RogueSurvivor.Engine
{
  internal static class InputTranslator
  {
    public static PlayerCommand KeyToCommand(KeyEventArgs key)
    {
      Keys cityInfo = RogueGame.KeyBindings.Get(PlayerCommand.CITY_INFO);
      if (Session.Get.CMDoptionExists("socrates-daimon")) {
        if (key.KeyData == (cityInfo | Keys.Control)) return PlayerCommand.DAIMON_MAP;  // debugging/cheat command
      }
      if (key.KeyData == (cityInfo | Keys.Shift)) return PlayerCommand.ITEM_INFO;
      if (key.KeyData == (cityInfo | Keys.Shift | Keys.Control)) return PlayerCommand.FACTION_INFO;

      Keys orders = RogueGame.KeyBindings.Get(PlayerCommand.ORDER_MODE);
      if (key.KeyData == (orders | Keys.Shift)) return PlayerCommand.OPTIONS_MODE;
      if (key.KeyData == (orders | Keys.Control)) return PlayerCommand.ALLIES_INFO;

      if (key.KeyData == (Keys.A | Keys.Control)) return PlayerCommand.ABANDON_PC;  // debugging/cheat command

      PlayerCommand playerCommand = RogueGame.KeyBindings.Get(key.KeyData);
      if (playerCommand != PlayerCommand.NONE)
        return playerCommand;
      if (key.Modifiers != Keys.None)
      {
        Keys keyData = key.KeyData;
        if ((key.Modifiers & Keys.Control) != Keys.None)
          keyData ^= Keys.Control;
        if ((key.Modifiers & Keys.Shift) != Keys.None)
          keyData ^= Keys.Shift;
        if ((key.Modifiers & Keys.Alt) != Keys.None)
          keyData ^= Keys.Alt;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_0))
          return PlayerCommand.ITEM_SLOT_0;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_1))
          return PlayerCommand.ITEM_SLOT_1;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_2))
          return PlayerCommand.ITEM_SLOT_2;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_3))
          return PlayerCommand.ITEM_SLOT_3;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_4))
          return PlayerCommand.ITEM_SLOT_4;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_5))
          return PlayerCommand.ITEM_SLOT_5;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_6))
          return PlayerCommand.ITEM_SLOT_6;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_7))
          return PlayerCommand.ITEM_SLOT_7;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_8))
          return PlayerCommand.ITEM_SLOT_8;
        if (keyData == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_9))
          return PlayerCommand.ITEM_SLOT_9;
      }
      return PlayerCommand.NONE;
    }
  }
}
