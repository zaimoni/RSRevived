// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueItems
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  public class UniqueItems : Zaimoni.Serialization.ISerialize
  {
    public UniqueItem TheSubwayWorkerBadge = default;

    public UniqueItems() {}

#region implement Zaimoni.Serialization.ISerialize
    protected UniqueItems(Zaimoni.Serialization.DecodeObjects decode) {
        Zaimoni.Serialization.ISave.Load(decode, ref TheSubwayWorkerBadge, (it,spawned) => {
            TheSubwayWorkerBadge = new(it, spawned);
        });
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.ISave.Save(encode, in TheSubwayWorkerBadge);
    }
#endregion

    private UniqueItems(JsonElement src, JsonSerializerOptions options) {
        if (src.TryGetProperty("TheSubwayWorkerBadge", out var relay)) TheSubwayWorkerBadge = load(relay, options);
    }

    static private UniqueItem load(JsonElement src, JsonSerializerOptions options) {
        return new(src.Require("Item").Deserialize<Data.Item>(options) ?? throw new JsonException("null unique item"), src.Require("Spawned").GetBoolean());
    }

    static private void save(Utf8JsonWriter writer, string name, in UniqueItem src, JsonSerializerOptions options) {
        if (null == src.TheItem) return;    // not generated yet
        writer.WritePropertyName(name);
        writer.WriteStartObject();
        writer.WritePropertyName("Item");
        JsonSerializer.Serialize(writer, src.TheItem, options);
        writer.WriteBoolean("Spawned", src.IsSpawned);
        writer.WriteEndObject();
    }

    public static UniqueItems fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options) {
        var src = JsonElement.ParseValue(ref reader);
        if (JsonValueKind.Object != src.ValueKind) throw new JsonException("expected object for UniqueItems");
        return new(src, options);
    }

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options) {
        writer.WriteStartObject();
        save(writer, "TheSubwayWorkerBadge", in TheSubwayWorkerBadge, options);
        writer.WriteEndObject();
    }
  }
}

namespace Zaimoni.JsonConvert
{
    public class UniqueItems : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.UniqueItems>
    {
        public override djack.RogueSurvivor.Engine.UniqueItems Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            return djack.RogueSurvivor.Engine.UniqueItems.fromJson(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.UniqueItems src, JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
