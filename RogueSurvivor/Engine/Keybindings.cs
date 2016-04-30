// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Keybindings
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Keybindings
  {
    private Dictionary<PlayerCommand, Keys> m_CommandToKeyData;
    private Dictionary<Keys, PlayerCommand> m_KeyToCommand;

    public Keybindings()
    {
      this.m_CommandToKeyData = new Dictionary<PlayerCommand, Keys>();
      this.m_KeyToCommand = new Dictionary<Keys, PlayerCommand>();
      this.ResetToDefaults();
    }

    public void ResetToDefaults()
    {
      this.m_CommandToKeyData.Clear();
      this.m_KeyToCommand.Clear();
      this.Set(PlayerCommand.BARRICADE_MODE, Keys.B);
      this.Set(PlayerCommand.BREAK_MODE, Keys.K);
      this.Set(PlayerCommand.CLOSE_DOOR, Keys.C);
      this.Set(PlayerCommand.FIRE_MODE, Keys.F);
      this.Set(PlayerCommand.HELP_MODE, Keys.H);
      this.Set(PlayerCommand.KEYBINDING_MODE, Keys.K | Keys.Shift);
      this.Set(PlayerCommand.ITEM_SLOT_0, Keys.D1);
      this.Set(PlayerCommand.ITEM_SLOT_1, Keys.D2);
      this.Set(PlayerCommand.ITEM_SLOT_2, Keys.D3);
      this.Set(PlayerCommand.ITEM_SLOT_3, Keys.D4);
      this.Set(PlayerCommand.ITEM_SLOT_4, Keys.D5);
      this.Set(PlayerCommand.ITEM_SLOT_5, Keys.D6);
      this.Set(PlayerCommand.ITEM_SLOT_6, Keys.D7);
      this.Set(PlayerCommand.ITEM_SLOT_7, Keys.D8);
      this.Set(PlayerCommand.ITEM_SLOT_8, Keys.D9);
      this.Set(PlayerCommand.ITEM_SLOT_9, Keys.D0);
      this.Set(PlayerCommand.ABANDON_GAME, Keys.A | Keys.Shift);
      this.Set(PlayerCommand.ADVISOR, Keys.H | Keys.Shift);
      this.Set(PlayerCommand.BUILD_LARGE_FORTIFICATION, Keys.N | Keys.Control);
      this.Set(PlayerCommand.BUILD_SMALL_FORTIFICATION, Keys.N);
      this.Set(PlayerCommand.CITY_INFO, Keys.I);
      this.Set(PlayerCommand.EAT_CORPSE, Keys.E | Keys.Shift);
      this.Set(PlayerCommand.GIVE_ITEM, Keys.G);
      this.Set(PlayerCommand.HINTS_SCREEN_MODE, Keys.H | Keys.Control);
      this.Set(PlayerCommand.INITIATE_TRADE, Keys.E);
      this.Set(PlayerCommand.LOAD_GAME, Keys.L | Keys.Shift);
      this.Set(PlayerCommand.MARK_ENEMIES_MODE, Keys.E | Keys.Control);
      this.Set(PlayerCommand.MESSAGE_LOG, Keys.M | Keys.Shift);
      this.Set(PlayerCommand.MOVE_E, Keys.NumPad6);
      this.Set(PlayerCommand.MOVE_N, Keys.NumPad8);
      this.Set(PlayerCommand.MOVE_NE, Keys.NumPad9);
      this.Set(PlayerCommand.MOVE_NW, Keys.NumPad7);
      this.Set(PlayerCommand.MOVE_S, Keys.NumPad2);
      this.Set(PlayerCommand.MOVE_SE, Keys.NumPad3);
      this.Set(PlayerCommand.MOVE_SW, Keys.NumPad1);
      this.Set(PlayerCommand.MOVE_W, Keys.NumPad4);
      this.Set(PlayerCommand.OPTIONS_MODE, Keys.O | Keys.Shift);
      this.Set(PlayerCommand.ORDER_MODE, Keys.O);
      this.Set(PlayerCommand.PUSH_MODE, Keys.P);
      this.Set(PlayerCommand.QUIT_GAME, Keys.Q | Keys.Shift);
      this.Set(PlayerCommand.REVIVE_CORPSE, Keys.R | Keys.Shift);
      this.Set(PlayerCommand.RUN_TOGGLE, Keys.R);
      this.Set(PlayerCommand.SAVE_GAME, Keys.S | Keys.Shift);
      this.Set(PlayerCommand.SCREENSHOT, Keys.N | Keys.Shift);
      this.Set(PlayerCommand.SHOUT, Keys.S);
      this.Set(PlayerCommand.SLEEP, Keys.Z);
      this.Set(PlayerCommand.SWITCH_PLACE, Keys.S | Keys.Control);
      this.Set(PlayerCommand.LEAD_MODE, Keys.T);
      this.Set(PlayerCommand.USE_SPRAY, Keys.A);
      this.Set(PlayerCommand.USE_EXIT, Keys.X);
      this.Set(PlayerCommand.WAIT_OR_SELF, Keys.NumPad5);
      this.Set(PlayerCommand.WAIT_LONG, Keys.W);
    }

    public Keys Get(PlayerCommand command)
    {
      Keys keys;
      if (this.m_CommandToKeyData.TryGetValue(command, out keys))
        return keys;
      return Keys.None;
    }

    public PlayerCommand Get(Keys key)
    {
      PlayerCommand playerCommand;
      if (this.m_KeyToCommand.TryGetValue(key, out playerCommand))
        return playerCommand;
      return PlayerCommand.NONE;
    }

    public void Set(PlayerCommand command, Keys key)
    {
      PlayerCommand key1 = this.Get(key);
      if (key1 != PlayerCommand.NONE)
        this.m_CommandToKeyData.Remove(key1);
      Keys key2 = this.Get(command);
      if (key2 != Keys.None)
        this.m_KeyToCommand.Remove(key2);
      this.m_CommandToKeyData[command] = key;
      this.m_KeyToCommand[key] = command;
    }

    public bool CheckForConflict()
    {
      foreach(Keys key1 in  m_CommandToKeyData.Values) {
        if (m_KeyToCommand.Keys.Count<Keys>((Func<Keys, bool>) (k => k == key1)) > 1) return true;
      }
      return false;
    }

    public static void Save(Keybindings kb, string filepath)
    {
      if (kb == null)
        throw new ArgumentNullException("kb");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings...");
      IFormatter formatter = Keybindings.CreateFormatter();
      Stream stream = Keybindings.CreateStream(filepath, true);
      formatter.Serialize(stream, (object) kb);
      stream.Flush();
      stream.Close();
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings... done!");
    }

    public static Keybindings Load(string filepath)
    {
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings...");
      Keybindings keybindings;
      try
      {
        IFormatter formatter = Keybindings.CreateFormatter();
        Stream stream = Keybindings.CreateStream(filepath, false);
        keybindings = (Keybindings) formatter.Deserialize(stream);
        stream.Close();
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load keybindings (first run?), using defaults.");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        keybindings = new Keybindings();
        keybindings.ResetToDefaults();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings... done!");
      return keybindings;
    }

    private static IFormatter CreateFormatter()
    {
      return (IFormatter) new BinaryFormatter();
    }

    private static Stream CreateStream(string saveName, bool save)
    {
      return (Stream) new FileStream(saveName, save ? FileMode.Create : FileMode.Open, save ? FileAccess.Write : FileAccess.Read, FileShare.None);
    }
  }
}
