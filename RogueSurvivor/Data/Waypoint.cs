using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    internal class Waypoint
    {
        public Location dest;
        public string label;

        public Waypoint(Location loc, string text)
        {
            dest = loc;
            label = text;
        }
    };

    [Serializable]
    public struct Waypoint_s
    {
        public Location dest;
        public string label;

        public Waypoint_s(Location loc, string text) {
            dest = loc;
            label = text;
        }
    };
}

namespace Zaimoni.JsonConvert
{
    using System.Text.Json;
    using Zaimoni.JSON;

    public sealed class Waypoint : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Data.Waypoint_s>
    {
        public override djack.RogueSurvivor.Data.Waypoint_s Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            var stage = JsonElement.ParseValue(ref reader);
            return new(stage.Require("Location").Deserialize<djack.RogueSurvivor.Data.Location>(options), stage.Require("Label").GetString() ?? throw new JsonException("null waypoint label"));
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Data.Waypoint_s src, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Location");
            JsonSerializer.Serialize(writer, src.dest, options);
            writer.WriteString("Label", src.label);
            writer.WriteEndObject();
        }
    }
}
