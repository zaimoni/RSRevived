﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Keybindings
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Keybindings
  {
    private readonly Dictionary<PlayerCommand, Keys> m_CommandToKeyData = new Dictionary<PlayerCommand, Keys>();
    private readonly Dictionary<Keys, PlayerCommand> m_KeyToCommand = new Dictionary<Keys, PlayerCommand>();

    public Keybindings()
    {
      ResetToDefaults();
    }

    public void ResetToDefaults()
    {
      m_CommandToKeyData.Clear();
      m_KeyToCommand.Clear();
            Set(PlayerCommand.BARRICADE_MODE, Keys.B);
            Set(PlayerCommand.BREAK_MODE, Keys.K);
            Set(PlayerCommand.CLOSE_DOOR, Keys.C);
            Set(PlayerCommand.FIRE_MODE, Keys.F);
            Set(PlayerCommand.HELP_MODE, Keys.H);
            Set(PlayerCommand.KEYBINDING_MODE, Keys.K | Keys.Shift);
            Set(PlayerCommand.ITEM_SLOT_0, Keys.D1);
            Set(PlayerCommand.ITEM_SLOT_1, Keys.D2);
            Set(PlayerCommand.ITEM_SLOT_2, Keys.D3);
            Set(PlayerCommand.ITEM_SLOT_3, Keys.D4);
            Set(PlayerCommand.ITEM_SLOT_4, Keys.D5);
            Set(PlayerCommand.ITEM_SLOT_5, Keys.D6);
            Set(PlayerCommand.ITEM_SLOT_6, Keys.D7);
            Set(PlayerCommand.ITEM_SLOT_7, Keys.D8);
            Set(PlayerCommand.ITEM_SLOT_8, Keys.D9);
            Set(PlayerCommand.ITEM_SLOT_9, Keys.D0);
            Set(PlayerCommand.ABANDON_GAME, Keys.A | Keys.Shift);
            Set(PlayerCommand.ADVISOR, Keys.H | Keys.Shift);
            Set(PlayerCommand.BUILD_LARGE_FORTIFICATION, Keys.N | Keys.Control);
            Set(PlayerCommand.BUILD_SMALL_FORTIFICATION, Keys.N);
            Set(PlayerCommand.CITY_INFO, Keys.I);   // Shift: item info; CTRL: cheat map
            Set(PlayerCommand.EAT_CORPSE, Keys.E | Keys.Shift);
            Set(PlayerCommand.GIVE_ITEM, Keys.G);
            Set(PlayerCommand.HINTS_SCREEN_MODE, Keys.H | Keys.Control);
            Set(PlayerCommand.INITIATE_TRADE, Keys.E);
            Set(PlayerCommand.LOAD_GAME, Keys.L | Keys.Shift);
            Set(PlayerCommand.MARK_ENEMIES_MODE, Keys.E | Keys.Control);
            Set(PlayerCommand.MESSAGE_LOG, Keys.M | Keys.Shift);
            Set(PlayerCommand.MOVE_E, Keys.NumPad6);
            Set(PlayerCommand.MOVE_N, Keys.NumPad8);
            Set(PlayerCommand.MOVE_NE, Keys.NumPad9);
            Set(PlayerCommand.MOVE_NW, Keys.NumPad7);
            Set(PlayerCommand.MOVE_S, Keys.NumPad2);
            Set(PlayerCommand.MOVE_SE, Keys.NumPad3);
            Set(PlayerCommand.MOVE_SW, Keys.NumPad1);
            Set(PlayerCommand.MOVE_W, Keys.NumPad4);
            Set(PlayerCommand.ORDER_MODE, Keys.O);
            Set(PlayerCommand.PULL_MODE, Keys.P | Keys.Control); // alpha10; XXX \todo convert Keys.P to an anchor key so we don't have to configure this
            Set(PlayerCommand.PUSH_MODE, Keys.P);
            Set(PlayerCommand.QUIT_GAME, Keys.Q | Keys.Shift);
            Set(PlayerCommand.REVIVE_CORPSE, Keys.R | Keys.Shift);
            Set(PlayerCommand.RUN_TOGGLE, Keys.R);
            Set(PlayerCommand.SAVE_GAME, Keys.S | Keys.Shift);
            Set(PlayerCommand.SCREENSHOT, Keys.N | Keys.Shift);
            Set(PlayerCommand.SHOUT, Keys.S);
            Set(PlayerCommand.SLEEP, Keys.Z);
            Set(PlayerCommand.SWITCH_PLACE, Keys.S | Keys.Control);
            Set(PlayerCommand.LEAD_MODE, Keys.T);
            Set(PlayerCommand.USE_SPRAY, Keys.A);
            Set(PlayerCommand.USE_EXIT, Keys.X);
            Set(PlayerCommand.WAIT_OR_SELF, Keys.NumPad5);
    }

    public Keys Get(PlayerCommand command)
    {
      if (m_CommandToKeyData.TryGetValue(command, out Keys keys)) return keys;
      return Keys.None;
    }

    public PlayerCommand Get(Keys key)
    {
      if (m_KeyToCommand.TryGetValue(key, out PlayerCommand playerCommand)) return playerCommand;
      return PlayerCommand.NONE;
    }

    public void Set(PlayerCommand command, Keys key)
    {
      PlayerCommand key1 = Get(key);
      if (key1 != PlayerCommand.NONE) m_CommandToKeyData.Remove(key1);
      Keys key2 = Get(command);
      if (key2 != Keys.None) m_KeyToCommand.Remove(key2);
      m_CommandToKeyData[command] = key;
      m_KeyToCommand[key] = command;
    }

    private bool _CheckForAnchorHardCodedConflict(Keys x)
    {
      // anchor keybinding cannot have any modifier keys as they block the anchored commands
      if ((x & Keys.Control) != Keys.None) return true;
      if ((x & Keys.Shift) != Keys.None) return true;
      if ((x & Keys.Alt) != Keys.None) return true;
      // no other explicit keybinding may alias a modifier to an anchor keybinding.
      // Just reserve all seven possible modifier sets.  This is a UI function so speed doesn't matter
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Alt))) return true;
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Control))) return true;
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Shift))) return true;
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Alt | Keys.Control))) return true;
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Alt | Keys.Shift))) return true;
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Control | Keys.Shift))) return true;
      if (m_KeyToCommand.Keys.Any(k => k == (x | Keys.Alt | Keys.Control | Keys.Shift))) return true;
      return false;
    }

    public bool CheckForConflict()
    {
      foreach (Keys key1 in  m_CommandToKeyData.Values) {
        if (m_KeyToCommand.Keys.Count(k => k == key1) > 1) return true;
      }

      if (_CheckForAnchorHardCodedConflict(RogueGame.KeyBindings.Get(PlayerCommand.CITY_INFO))) return true;
      if (_CheckForAnchorHardCodedConflict(RogueGame.KeyBindings.Get(PlayerCommand.ORDER_MODE))) return true;

      return false;
    }

    public static void Save(Keybindings kb, string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
      if (null == kb) throw new ArgumentNullException(nameof(kb));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings...");
	  filepath.BinarySerialize(kb);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings... done!");
    }

    public static Keybindings Load(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings...");
      Keybindings keybindings;
      try {
	    keybindings = filepath.BinaryDeserialize<Keybindings>();
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load keybindings (first run?), using defaults.");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        keybindings = new Keybindings();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings... done!");
      return keybindings;
    }
  }
}
