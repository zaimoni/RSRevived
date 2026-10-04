// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Message
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Zaimoni.Data;
using Zaimoni.JSON;

using Color = System.Drawing.Color;

#nullable enable

namespace djack.RogueSurvivor.UI
{
  [Serializable]
  public class Message
  {
    public readonly string Text;
    public readonly Color Color;
    public readonly int Turn;

    public Message(string text, int turn, Color color)
    {
#if DEBUG
      if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
#endif
      Text = turn.ToString()+" "+text.Capitalize();
      Color = color;
      Turn = turn;
    }

    public Message(string text, int turn) : this(text, turn, Color.White) {}

    // the JSON load: the text is already as shown
    private Message(int turn, Color color, string text)
    {
      Text = text;
      Color = color;
      Turn = turn;
    }

    public static Message fromJson(System.Text.Json.JsonElement src)
    {
      return new(src.Require("Turn").GetInt32(), Zaimoni.JsonConvert.Message.ReadColor(src.Require("Color")), src.Require("Text").GetString() ?? throw new System.Text.Json.JsonException("null message text"));
    }

    public void toJson(System.Text.Json.Utf8JsonWriter writer)
    {
      writer.WriteStartObject();
      writer.WriteNumber("Turn", Turn);
      Zaimoni.JsonConvert.Message.WriteColor(writer, "Color", Color);
      writer.WriteString("Text", Text);
      writer.WriteEndObject();
    }
  }
}

namespace Zaimoni.JsonConvert
{
  using System.Text.Json;

  public sealed class Message : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.UI.Message>
  {
    public override djack.RogueSurvivor.UI.Message Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options) => djack.RogueSurvivor.UI.Message.fromJson(JsonElement.ParseValue(ref reader));
    public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.UI.Message src, JsonSerializerOptions options) => src.toJson(writer);

    /// <summary>a known color by its name, any other as #AARRGGBB</summary>
    static public void WriteColor(Utf8JsonWriter writer, string name, Color src)
    {
      writer.WriteString(name, src.IsKnownColor ? src.Name : "#" + src.ToArgb().ToString("X8"));
    }

    static public Color ReadColor(JsonElement src)
    {
      var text = src.GetString() ?? throw new JsonException("null color");
      if (text.StartsWith('#')) return Color.FromArgb(int.Parse(text.AsSpan(1), System.Globalization.NumberStyles.HexNumber));
      var ret = Color.FromName(text);
      if (!ret.IsKnownColor) throw new JsonException("unknown color: " + text);
      return ret;
    }
  }
}
