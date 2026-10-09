using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine
{
  /// <summary>Keys to play before the keyboard is heard from: --keys=TOKENS, or --keys=@FILE for a response file; several, in order.
  /// Tokens are separated by whitespace, and # starts a comment that runs to the end of the line.
  /// A token is [MODIFIER+]...NAME[*COUNT].  MODIFIER is ctrl (or control), alt, or shift.  NAME is a W3C UI Events code value
  /// (KeyA, Digit8, Numpad8, Enter, Escape, ArrowUp, F1, Space, NumpadAdd, Period, ...), or failing that a System.Windows.Forms.Keys
  /// member (NumPad8, D8, Return, OemPeriod, ...); neither is case-sensitive.  Digit8 is the keyboard's 8, Numpad8 the keypad's.
  /// COUNT, a positive integer, delivers the key that many times: Numpad8*3 is Numpad8 Numpad8 Numpad8.
  /// The script is the person at the keyboard until it runs out: every wait for a key takes from it first, and the shortcuts of
  /// an unattended test run (TestDrive.Unattended) wait for it to run out.  Each key delivered goes to the log, so that the log
  /// of a crash says how far the script got.  A script should end where a player is asked to act, not inside a prompt.</summary>
  internal static class KeyScript
  {
    private readonly record struct Scripted(string Token, Keys Key);

    private static readonly Queue<Scripted> s_Keys = new();

    static public bool IsEmpty => 0 >= s_Keys.Count;

    /// <returns>true if and only if arg was ours</returns>
    /// <exception cref="ArgumentException">a token that is not a key, or a count that is not a positive integer</exception>
    static public bool Parse(string arg)
    {
      const string prefix = "--keys=";
      if (!arg.StartsWith(prefix)) return false;
      var text = arg.Substring(prefix.Length);
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
        if (!int.TryParse(token.Substring(star + 1), out count) || 0 >= count) throw new ArgumentException("--keys: " + token + ": the count after * is a positive integer");
        name = token.Substring(0, star);
      }
      var key = ParseKey(name) ?? throw new ArgumentException("--keys: " + token + ": not a key");
      var scripted = new Scripted(name, key);
      while (0 < count--) s_Keys.Enqueue(scripted);
    }

    /// <summary>the next scripted key, logged; null once the script has run out</summary>
    static public KeyEventArgs? Next()
    {
      if (!s_Keys.TryDequeue(out var scripted)) return null;
      Logger.WriteLine(Logger.Stage.RUN_DEBUG, "key script: " + scripted.Token + (IsEmpty ? " (last)" : ""));
      return new KeyEventArgs(scripted.Key);
    }

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

    // W3C UI Events code values that are not spelled as the System.Windows.Forms.Keys member, by lower case.
    // (KeyA..KeyZ, Digit0..Digit9, Numpad0..Numpad9 and F1..F24 are handled by pattern or by the enum.)
    private static readonly Dictionary<string, Keys> s_W3C = new() {
      { "enter", Keys.Return }, { "numpadenter", Keys.Return },   // WinForms does not tell the keypad's Enter apart
      { "escape", Keys.Escape }, { "space", Keys.Space }, { "tab", Keys.Tab }, { "backspace", Keys.Back },
      { "delete", Keys.Delete }, { "insert", Keys.Insert }, { "home", Keys.Home }, { "end", Keys.End },
      { "pageup", Keys.PageUp }, { "pagedown", Keys.PageDown },
      { "arrowup", Keys.Up }, { "arrowdown", Keys.Down }, { "arrowleft", Keys.Left }, { "arrowright", Keys.Right },
      { "numpadadd", Keys.Add }, { "numpadsubtract", Keys.Subtract }, { "numpadmultiply", Keys.Multiply },
      { "numpaddivide", Keys.Divide }, { "numpaddecimal", Keys.Decimal },
      { "minus", Keys.OemMinus }, { "equal", Keys.Oemplus },
      { "bracketleft", Keys.OemOpenBrackets }, { "bracketright", Keys.OemCloseBrackets },
      { "backslash", Keys.OemPipe }, { "intlbackslash", Keys.OemBackslash },
      { "semicolon", Keys.OemSemicolon }, { "quote", Keys.OemQuotes }, { "backquote", Keys.Oemtilde },
      { "comma", Keys.Oemcomma }, { "period", Keys.OemPeriod }, { "slash", Keys.OemQuestion },
      { "capslock", Keys.CapsLock }, { "numlock", Keys.NumLock }, { "scrolllock", Keys.Scroll },
      { "printscreen", Keys.PrintScreen }, { "pause", Keys.Pause }, { "contextmenu", Keys.Apps },
    };

    // a modifier on its own never reaches the game (RogueForm.UI_PostKey drops it)
    private static readonly HashSet<Keys> s_ModifierKeys = new() {
      Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey, Keys.Menu, Keys.LMenu, Keys.RMenu
    };

    static private Keys? KeyCode(string name)
    {
      if (0 >= name.Length) return null;
      var lower = name.ToLowerInvariant();
      if (s_W3C.TryGetValue(lower, out var w3c)) return w3c;
      if (4 == lower.Length && lower.StartsWith("key") && char.IsAsciiLetterLower(lower[3])) return Keys.A + (lower[3] - 'a');
      if (6 == lower.Length && lower.StartsWith("digit") && char.IsAsciiDigit(lower[5])) return Keys.D0 + (lower[5] - '0');
      if (7 == lower.Length && lower.StartsWith("numpad") && char.IsAsciiDigit(lower[6])) return Keys.NumPad0 + (lower[6] - '0');
      if (!Enum.TryParse<Keys>(name, true, out var key)) return null;   // a System.Windows.Forms.Keys member: NumPad8, D8, Return, F1, ...
      if (Keys.None == (key & Keys.KeyCode) || Keys.None != (key & Keys.Modifiers)) return null;   // a modifier flag or a mask, not a key
      if (s_ModifierKeys.Contains(key)) return null;
      return key;
    }
  }
}
