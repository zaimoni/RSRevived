using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine
{
  /// <summary>Input to play before the keyboard and mouse are heard from: --input=TOKENS, or --input=@FILE for a response file;
  /// several, in order (--keys= is a synonym).  Tokens are separated by whitespace, and # starts a comment that runs to the end of
  /// the line.  Any token may end in *COUNT, a positive integer, which delivers it that many times: Numpad8*3 is Numpad8 Numpad8 Numpad8.
  ///
  /// A key is [MODIFIER+]...NAME.  MODIFIER is ctrl (or control), alt, or shift.  NAME is a W3C UI Events code value (KeyA, Digit8,
  /// Numpad8, Enter, Escape, ArrowUp, F1, Space, NumpadAdd, Period, ...), or failing that a System.Windows.Forms.Keys member (NumPad8,
  /// D8, Return, OemPeriod, ...); neither is case-sensitive.  Digit8 is the keyboard's 8, Numpad8 the keypad's.
  ///
  /// The mouse is mousemove@X,Y or click@X,Y[:BUTTON], in game canvas pixels (MouseEvent.offsetX, offsetY).  BUTTON is the W3C
  /// MouseEvent.button value 0, 1, 2 (3, 4), or its usual name left, middle, right; the default is left.  The game only hears the
  /// mouse where it waits for a key or the mouse, and a click there carries its position; a mouse token where only a key is asked
  /// for is skipped, with a note in the log.
  ///
  /// The script is the person at the keyboard until it runs out: every wait takes from it first, and the shortcuts of an unattended
  /// test run (TestDrive.Unattended) wait for it to run out.  Each token delivered goes to the log, so that the log of a crash says
  /// how far the script got.  A script should end where a player is asked to act: a test run whose script runs out inside a prompt
  /// fails (RejectUnattended), rather than wait for a keyboard no one is at.  After --start=load, the first prompt is a popup: Escape.
  ///
  /// What a person does is logged in the same syntax, as "input: TOKEN" (the script's go as "input script: TOKEN"), so that a log
  /// replays as a script.  Mouse moves are not logged, only clicks: a click carries its position, and moves would swamp the log.</summary>
  internal static class InputScript
  {
    private enum What : byte { KEY, MOUSE_MOVE, CLICK }

    private readonly record struct Scripted(string Token, What Kind, Keys Key, MouseButtons Button, int X, int Y);

    private static readonly Queue<Scripted> s_Queue = new();

    static public bool IsEmpty => 0 >= s_Queue.Count;

    /// <returns>true if and only if arg was ours</returns>
    /// <exception cref="ArgumentException">a token that is not input, or a count that is not a positive integer</exception>
    static public bool Parse(string arg)
    {
      string? text = null;
      foreach (var prefix in new[] { "--input=", "--keys=" }) {
        if (arg.StartsWith(prefix)) {
          text = arg.Substring(prefix.Length);
          break;
        }
      }
      if (null == text) return false;
      if (text.StartsWith("@")) text = File.ReadAllText(text.Substring(1));
      foreach (var line in text.Split('\n')) {
        var code = line;
        var comment = code.IndexOf('#');
        if (0 <= comment) code = code.Substring(0, comment);
        foreach (var token in code.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)) Add(token);
      }
      return true;
    }

    private static void Add(string token)
    {
      var name = token;
      var count = 1;
      var star = token.LastIndexOf('*');
      if (0 <= star) {
        if (!int.TryParse(token.Substring(star + 1), out count) || 0 >= count) throw new ArgumentException("--input: " + token + ": the count after * is a positive integer");
        name = token.Substring(0, star);
      }
      var scripted = ParseToken(name) ?? throw new ArgumentException("--input: " + token + ": not a key, mousemove@X,Y or click@X,Y[:BUTTON]");
      while (0 < count--) s_Queue.Enqueue(scripted);
    }

#region playing
    /// <summary>For a wait that takes only a key: the next scripted key, logged.  A mouse token is skipped, logged.</summary>
    /// <returns>null once the script has run out</returns>
    static public KeyEventArgs? NextKey()
    {
      while (s_Queue.TryDequeue(out var scripted)) {
        if (What.KEY == scripted.Kind) {
          Played(scripted, "");
          return new KeyEventArgs(scripted.Key);
        }
        Played(scripted, " (skipped: the game asked for a key)");
      }
      return null;
    }

    /// <summary>For a wait that takes a key or the mouse: the next scripted input, logged, as RogueForm.WaitKeyOrMouse reports it.</summary>
    /// <param name="pointerX">where the pointer is, for a key</param>
    /// <returns>null once the script has run out</returns>
    static public KeyValuePair<KeyValuePair<KeyEventArgs?, MouseButtons?>, KeyValuePair<int, int>>? Next(int pointerX, int pointerY)
    {
      if (!s_Queue.TryDequeue(out var scripted)) return null;
      Played(scripted, "");
      switch (scripted.Kind) {
      case What.KEY: return new KeyValuePair<KeyValuePair<KeyEventArgs?, MouseButtons?>, KeyValuePair<int, int>>(new(new KeyEventArgs(scripted.Key), null), new(pointerX, pointerY));
      case What.MOUSE_MOVE: return new KeyValuePair<KeyValuePair<KeyEventArgs?, MouseButtons?>, KeyValuePair<int, int>>(default, new(scripted.X, scripted.Y));
      default: return new KeyValuePair<KeyValuePair<KeyEventArgs?, MouseButtons?>, KeyValuePair<int, int>>(new(null, scripted.Button), new(scripted.X, scripted.Y));
      }
    }

    /// <summary>A wait for input, once the script has run out: a test run has no one to answer it.</summary>
    /// <exception cref="InvalidOperationException">in a test run, which TestDrive.Failed then reports</exception>
    static public void RejectUnattended()
    {
      if (TestDrive.Unattended) throw new InvalidOperationException("unattended run asked for input: an input script should end where a player is asked to act, not inside a prompt");
    }

    private static void Played(in Scripted scripted, string note)
    {
      Logger.WriteLine(Logger.Stage.RUN_DEBUG, "input script: " + scripted.Token + note + (IsEmpty ? " (last)" : ""));
    }
#endregion

#region recording
    /// <returns>key, for a one-line call</returns>
    static public KeyEventArgs Record(KeyEventArgs key)
    {
      Logger.WriteLine(Logger.Stage.RUN_DEBUG, "input: " + Render(key.KeyData));
      return key;
    }

    static public void Record(MouseButtons button, int x, int y)
    {
      Logger.WriteLine(Logger.Stage.RUN_DEBUG, "input: " + Render(button, x, y));
    }

    static public string Render(Keys key)
    {
      var ret = "";
      if (Keys.None != (key & Keys.Control)) ret += "ctrl+";
      if (Keys.None != (key & Keys.Alt)) ret += "alt+";
      if (Keys.None != (key & Keys.Shift)) ret += "shift+";
      var code = key & Keys.KeyCode;
      if (Keys.A <= code && Keys.Z >= code) return ret + "Key" + (char)('A' + (code - Keys.A));
      if (Keys.D0 <= code && Keys.D9 >= code) return ret + "Digit" + (char)('0' + (code - Keys.D0));
      if (Keys.NumPad0 <= code && Keys.NumPad9 >= code) return ret + "Numpad" + (char)('0' + (code - Keys.NumPad0));
      if (s_W3CName.TryGetValue(code, out var w3c)) return ret + w3c;
      return ret + code.ToString();   // a System.Windows.Forms.Keys member, which parses back
    }

    static public string Render(MouseButtons button, int x, int y)
    {
      var ret = "click@" + x.ToString() + "," + y.ToString();
      switch (button) {
      case MouseButtons.Left: return ret;
      case MouseButtons.Middle: return ret + ":middle";
      case MouseButtons.Right: return ret + ":right";
      case MouseButtons.XButton1: return ret + ":3";
      case MouseButtons.XButton2: return ret + ":4";
      default: return ret + ":" + button.ToString();   // does not parse back; a chord, which the game does not use
      }
    }
#endregion

#region parsing
    static private Scripted? ParseToken(string token)
    {
      var at = token.IndexOf('@');
      if (0 > at) {
        var key = ParseKey(token);
        return null == key ? null : new Scripted(token, What.KEY, key.Value, MouseButtons.None, 0, 0);
      }
      var kind = token.Substring(0, at).ToLowerInvariant();
      var rest = token.Substring(at + 1);
      var button = MouseButtons.Left;
      var colon = rest.IndexOf(':');
      if (0 <= colon) {
        var name = rest.Substring(colon + 1);
        rest = rest.Substring(0, colon);
        if (!s_Button.TryGetValue(name.ToLowerInvariant(), out button)) return null;
      }
      var xy = rest.Split(',');
      if (2 != xy.Length || !int.TryParse(xy[0], out var x) || !int.TryParse(xy[1], out var y)) return null;
      switch (kind) {
      case "mousemove": return 0 > colon ? new Scripted(token, What.MOUSE_MOVE, Keys.None, MouseButtons.None, x, y) : null;
      case "click": return new Scripted(token, What.CLICK, Keys.None, button, x, y);
      default: return null;
      }
    }

    // W3C MouseEvent.button values, and their usual names
    private static readonly Dictionary<string, MouseButtons> s_Button = new() {
      { "0", MouseButtons.Left }, { "left", MouseButtons.Left },
      { "1", MouseButtons.Middle }, { "middle", MouseButtons.Middle },
      { "2", MouseButtons.Right }, { "right", MouseButtons.Right },
      { "3", MouseButtons.XButton1 }, { "4", MouseButtons.XButton2 },
    };

    static private Keys? ParseKey(string name)
    {
      var modifiers = Keys.None;
      var parts = name.Split('+');
      for (int i = 0; i + 1 < parts.Length; i++) {
        switch (parts[i].ToLowerInvariant()) {
        case "ctrl": case "control": modifiers |= Keys.Control; break;
        case "alt": modifiers |= Keys.Alt; break;
        case "shift": modifiers |= Keys.Shift; break;
        default: return null;
        }
      }
      var code = KeyCode(parts[parts.Length - 1]);
      return null == code ? null : code.Value | modifiers;
    }

    // W3C UI Events code values that are not spelled as the System.Windows.Forms.Keys member.  The first name for a key is
    // the one a recording uses.  (KeyA..KeyZ, Digit0..Digit9, Numpad0..Numpad9 and F1..F24 are handled by pattern or by the enum.)
    private static readonly KeyValuePair<string, Keys>[] s_W3C = {
      new("Enter", Keys.Return), new("NumpadEnter", Keys.Return),   // WinForms does not tell the keypad's Enter apart
      new("Escape", Keys.Escape), new("Space", Keys.Space), new("Tab", Keys.Tab), new("Backspace", Keys.Back),
      new("Delete", Keys.Delete), new("Insert", Keys.Insert), new("Home", Keys.Home), new("End", Keys.End),
      new("PageUp", Keys.PageUp), new("PageDown", Keys.PageDown),
      new("ArrowUp", Keys.Up), new("ArrowDown", Keys.Down), new("ArrowLeft", Keys.Left), new("ArrowRight", Keys.Right),
      new("NumpadAdd", Keys.Add), new("NumpadSubtract", Keys.Subtract), new("NumpadMultiply", Keys.Multiply),
      new("NumpadDivide", Keys.Divide), new("NumpadDecimal", Keys.Decimal),
      new("Minus", Keys.OemMinus), new("Equal", Keys.Oemplus),
      new("BracketLeft", Keys.OemOpenBrackets), new("BracketRight", Keys.OemCloseBrackets),
      new("Backslash", Keys.OemPipe), new("IntlBackslash", Keys.OemBackslash),
      new("Semicolon", Keys.OemSemicolon), new("Quote", Keys.OemQuotes), new("Backquote", Keys.Oemtilde),
      new("Comma", Keys.Oemcomma), new("Period", Keys.OemPeriod), new("Slash", Keys.OemQuestion),
      new("CapsLock", Keys.CapsLock), new("NumLock", Keys.NumLock), new("ScrollLock", Keys.Scroll),
      new("PrintScreen", Keys.PrintScreen), new("Pause", Keys.Pause), new("ContextMenu", Keys.Apps),
    };

    private static readonly Dictionary<string, Keys> s_W3CKey = W3CKey();
    private static readonly Dictionary<Keys, string> s_W3CName = W3CName();

    private static Dictionary<string, Keys> W3CKey()
    {
      Dictionary<string, Keys> ret = new();
      foreach (var x in s_W3C) ret.Add(x.Key.ToLowerInvariant(), x.Value);
      return ret;
    }

    private static Dictionary<Keys, string> W3CName()
    {
      Dictionary<Keys, string> ret = new();
      foreach (var x in s_W3C) ret.TryAdd(x.Value, x.Key);
      return ret;
    }

    // a modifier on its own never reaches the game (RogueForm.UI_PostKey drops it)
    private static readonly HashSet<Keys> s_ModifierKeys = new() {
      Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey, Keys.Menu, Keys.LMenu, Keys.RMenu
    };

    static private Keys? KeyCode(string name)
    {
      if (0 >= name.Length) return null;
      var lower = name.ToLowerInvariant();
      if (s_W3CKey.TryGetValue(lower, out var w3c)) return w3c;
      if (4 == lower.Length && lower.StartsWith("key") && char.IsAsciiLetterLower(lower[3])) return Keys.A + (lower[3] - 'a');
      if (6 == lower.Length && lower.StartsWith("digit") && char.IsAsciiDigit(lower[5])) return Keys.D0 + (lower[5] - '0');
      if (7 == lower.Length && lower.StartsWith("numpad") && char.IsAsciiDigit(lower[6])) return Keys.NumPad0 + (lower[6] - '0');
      if (!Enum.TryParse<Keys>(name, true, out var key)) return null;   // a System.Windows.Forms.Keys member: NumPad8, D8, Return, F1, ...
      if (Keys.None == (key & Keys.KeyCode) || Keys.None != (key & Keys.Modifiers)) return null;   // a modifier flag or a mask, not a key
      if (s_ModifierKeys.Contains(key)) return null;
      return key;
    }
#endregion
  }
}
