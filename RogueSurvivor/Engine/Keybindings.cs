// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Keybindings
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using djack.RogueSurvivor.UI;

using Zaimoni.Data;

// GDI+ types
using Color = System.Drawing.Color;

// savefile break: reposition to djack.RogueSurvivor.UI
namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Keybindings
  {
    private readonly Dictionary<PlayerCommand, Keys> m_CommandToKeyData = new();
    private readonly Dictionary<Keys, PlayerCommand> m_KeyToCommand = new();

    public Keybindings() => ResetToDefaults();

    private void ResetToDefaults()
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
            Set(PlayerCommand.UNLOAD, Keys.U);
            Set(PlayerCommand.LEAD_MODE, Keys.T);
            Set(PlayerCommand.USE_SPRAY, Keys.A);
            Set(PlayerCommand.USE_EXIT, Keys.X);
            Set(PlayerCommand.WAIT_OR_SELF, Keys.NumPad5);
    }

    public string AsString(PlayerCommand command)
    {
      if (m_CommandToKeyData.TryGetValue(command, out Keys keys)) return keys.ToString();
      return string.Empty;
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

    private void Set(PlayerCommand command, Keys key)
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

    private bool CheckForConflict()
    {
      foreach (Keys key1 in  m_CommandToKeyData.Values) {
        if (m_KeyToCommand.Keys.Count(k => k == key1) > 1) return true;
      }

      if (_CheckForAnchorHardCodedConflict(RogueGame.KeyBindings.Get(PlayerCommand.CITY_INFO))) return true;
      if (_CheckForAnchorHardCodedConflict(RogueGame.KeyBindings.Get(PlayerCommand.ORDER_MODE))) return true;
      if (_CheckForAnchorHardCodedConflict(RogueGame.KeyBindings.Get(PlayerCommand.LEAD_MODE))) return true;
      if (_CheckForAnchorHardCodedConflict(RogueGame.KeyBindings.Get(PlayerCommand.FIRE_MODE))) return true;

      return false;
    }

    private void Save(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings...");
	  filepath.BinarySerialize(this);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings... done!");
    }

    public static Keybindings Load(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings... ("+filepath+")");
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

    public void HandleRedefineKeys()
    {
      // need to maintain: label to command mapping
      // then generate current keybindings
      // then read off position from reference array
      // screen layout may fail with more than 51 entries; at 49 entries currently
      var command_labels = new KeyValuePair<string, PlayerCommand>[] {
          new KeyValuePair< string,PlayerCommand >("Move N", PlayerCommand.MOVE_N),
          new KeyValuePair< string,PlayerCommand >("Move NE", PlayerCommand.MOVE_NE),
          new KeyValuePair< string,PlayerCommand >("Move E", PlayerCommand.MOVE_E),
          new KeyValuePair< string,PlayerCommand >("Move SE", PlayerCommand.MOVE_SE),
          new KeyValuePair< string,PlayerCommand >("Move S", PlayerCommand.MOVE_S),
          new KeyValuePair< string,PlayerCommand >("Move SW", PlayerCommand.MOVE_SW),
          new KeyValuePair< string,PlayerCommand >("Move W", PlayerCommand.MOVE_W),
          new KeyValuePair< string,PlayerCommand >("Move NW", PlayerCommand.MOVE_NW),
          new KeyValuePair< string,PlayerCommand >("Wait", PlayerCommand.WAIT_OR_SELF),
          new KeyValuePair< string,PlayerCommand >("Abandon Game", PlayerCommand.ABANDON_GAME),
          new KeyValuePair< string,PlayerCommand >("Advisor Hint", PlayerCommand.ADVISOR),
          new KeyValuePair< string,PlayerCommand >("Barricade", PlayerCommand.BARRICADE_MODE),
          new KeyValuePair< string,PlayerCommand >("Break", PlayerCommand.BREAK_MODE),
          new KeyValuePair< string,PlayerCommand >("Build Large Fortification", PlayerCommand.BUILD_LARGE_FORTIFICATION),
          new KeyValuePair< string,PlayerCommand >("Build Small Fortification", PlayerCommand.BUILD_SMALL_FORTIFICATION),
          new KeyValuePair< string,PlayerCommand >("City Info", PlayerCommand.CITY_INFO),
          new KeyValuePair< string,PlayerCommand >("Close", PlayerCommand.CLOSE_DOOR),
          new KeyValuePair< string,PlayerCommand >("Fire", PlayerCommand.FIRE_MODE),
          new KeyValuePair< string,PlayerCommand >("Give", PlayerCommand.GIVE_ITEM),
          new KeyValuePair< string,PlayerCommand >("Help", PlayerCommand.HELP_MODE),
          new KeyValuePair< string,PlayerCommand >("Hints screen", PlayerCommand.HINTS_SCREEN_MODE),
          new KeyValuePair< string,PlayerCommand >("Initiate Trade", PlayerCommand.INITIATE_TRADE),
          new KeyValuePair< string,PlayerCommand >("Item 1 slot", PlayerCommand.ITEM_SLOT_0),
          new KeyValuePair< string,PlayerCommand >("Item 2 slot", PlayerCommand.ITEM_SLOT_1),
          new KeyValuePair< string,PlayerCommand >("Item 3 slot", PlayerCommand.ITEM_SLOT_2),
          new KeyValuePair< string,PlayerCommand >("Item 4 slot", PlayerCommand.ITEM_SLOT_3),
          new KeyValuePair< string,PlayerCommand >("Item 5 slot", PlayerCommand.ITEM_SLOT_4),
          new KeyValuePair< string,PlayerCommand >("Item 6 slot", PlayerCommand.ITEM_SLOT_5),
          new KeyValuePair< string,PlayerCommand >("Item 7 slot", PlayerCommand.ITEM_SLOT_6),
          new KeyValuePair< string,PlayerCommand >("Item 8 slot", PlayerCommand.ITEM_SLOT_7),
          new KeyValuePair< string,PlayerCommand >("Item 9 slot", PlayerCommand.ITEM_SLOT_8),
          new KeyValuePair< string,PlayerCommand >("Item 10 slot", PlayerCommand.ITEM_SLOT_9),
          new KeyValuePair< string,PlayerCommand >("Lead", PlayerCommand.LEAD_MODE),
          new KeyValuePair< string,PlayerCommand >("Load Game", PlayerCommand.LOAD_GAME),
          new KeyValuePair< string,PlayerCommand >("Mark Enemies", PlayerCommand.MARK_ENEMIES_MODE),
          new KeyValuePair< string,PlayerCommand >("Messages Log", PlayerCommand.MESSAGE_LOG),
          new KeyValuePair< string,PlayerCommand >("Order", PlayerCommand.ORDER_MODE),
          new KeyValuePair< string,PlayerCommand >("Pull", PlayerCommand.PULL_MODE),
          new KeyValuePair< string,PlayerCommand >("Push", PlayerCommand.PUSH_MODE),
          new KeyValuePair< string,PlayerCommand >("Quit Game", PlayerCommand.QUIT_GAME),
          new KeyValuePair< string,PlayerCommand >("Redefine Keys", PlayerCommand.KEYBINDING_MODE),
          new KeyValuePair< string,PlayerCommand >("Run", PlayerCommand.RUN_TOGGLE),
          new KeyValuePair< string,PlayerCommand >("Save Game", PlayerCommand.SAVE_GAME),
          new KeyValuePair< string,PlayerCommand >("Screenshot", PlayerCommand.SCREENSHOT),
          new KeyValuePair< string,PlayerCommand >("Shout", PlayerCommand.SHOUT),
          new KeyValuePair< string,PlayerCommand >("Sleep", PlayerCommand.SLEEP),
          new KeyValuePair< string,PlayerCommand >("Switch Place", PlayerCommand.SWITCH_PLACE),
          new KeyValuePair< string,PlayerCommand >("Unload", PlayerCommand.UNLOAD),
          new KeyValuePair< string,PlayerCommand >("Use Exit", PlayerCommand.USE_EXIT),
          new KeyValuePair< string,PlayerCommand >("Use Spray", PlayerCommand.USE_SPRAY),
        };

      string[] entries = command_labels.Select(x => x.Key).ToArray();
      var m_UI = IRogueUI.UI; // backward compatibility
      const int BOLD_LINE_SPACING = IRogueUI.BOLD_LINE_SPACING;
      var game = RogueGame.Game;
      const int gx = 0;
      int gy = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        string[] values = command_labels.Select(x => Get(x.Value).ToString()).ToArray();

        gy = 0;
        m_UI.ClearScreen();
        game.DrawHeader();
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, "Redefine keys", 0, gy, new Color?());
        gy += BOLD_LINE_SPACING;
        game.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy);
        if (CheckForConflict()) {
          m_UI.UI_DrawStringBold(Color.Red, "Conflicting keys. Please redefine the keys so the commands don't overlap.", gx, gy, new Color?());
          gy += BOLD_LINE_SPACING;
        }
        m_UI.DrawFootnote("cursor to move, ENTER to rebind a key, ESC to save and leave");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("rebinding {0}, press the new key.", command_labels[currentChoice].Key), gx, gy, new Color?());
        m_UI.UI_Repaint();
        Keys key = Keys.None;
        while(true) {
          KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
          if (keyEventArgs.KeyCode != Keys.ShiftKey && keyEventArgs.KeyCode != Keys.ControlKey && !keyEventArgs.Alt) {
            key = keyEventArgs.KeyData;
            break;
          }
        }
        if (0>currentChoice || command_labels.Length<=currentChoice) throw new InvalidOperationException("unhandled selected");
        PlayerCommand command = command_labels[currentChoice].Value;
        Set(command, key);
        return null;
      });
      do game.ChoiceMenu(choice_handler, setup_handler, entries.Length);
      while(CheckForConflict());

      // save the keybindings
      m_UI.DrawHeadNote("Saving keybindings...");
      Save(Path.Combine(RogueGame.GetUserConfigPath(), "keys.dat"));
      m_UI.DrawHeadNote("Saving keybindings... done!");
    }
  }
}
