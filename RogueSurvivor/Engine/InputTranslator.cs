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
      // debugging/cheat commands
      if (Session.Get.CMDoptionExists("socrates-daimon")) {
        if (key.KeyData == (Keys.I | Keys.Control)) return PlayerCommand.DAIMON_MAP;
      }
      if (key.KeyData == (Keys.A | Keys.Control)) return PlayerCommand.ABANDON_PC;

      // allow configuring this when we want to break format : V.0.10.0
      if (key.KeyData == (Keys.O | Keys.Shift)) return PlayerCommand.ALLIES_INFO;

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
