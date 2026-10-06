// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.Board
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  internal sealed class Board : MapObject, IJsonStaged<Board>
    {
    public readonly string[] Text;

    public Board(string imageID, string[] text) : base(imageID) {
      Text = text;
    }

    private Board(JsonElement src, JsonSerializerOptions options) : base(src, options) {
        var stage = new System.Collections.Generic.List<string>();
        foreach (var x in src.Require("Text").EnumerateArray()) stage.Add(x.GetString() ?? throw new JsonException("null line of text"));
        Text = stage.ToArray();
    }

    static Board IJsonStaged<Board>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) {
        base.toJsonFields(writer, options);
        writer.WriteArray("Text", Text, options);
    }
  }
}
