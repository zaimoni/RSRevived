using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

namespace Zaimoni.JSON
{
    static public class JSON_ext
    {
        static public string? TrySaveAsRef<T>(this T src, Utf8JsonWriter writer) where T:class {
            var resolve = PreserveReferenceHandler.Resolver;
            var id = resolve.GetReference(src, out bool alreadyExists);
            if (alreadyExists) {
                writer.WriteStartObject();
                writer.WriteString("$ref", id);
                writer.WriteEndObject();
                return null;
            }
            return id;
        }

        static public void SaveAsRef<T>(this T src, Utf8JsonWriter writer) where T : class
        {
            var resolve = PreserveReferenceHandler.Resolver;
            var id = resolve.Reference(src);
            writer.WriteStartObject();
            writer.WriteString("$ref", id);
            writer.WriteEndObject();
        }

        public static T? TryReadRef<T>(this ref Utf8JsonReader reader) where T:class
        {
            var test = reader;
            test.Read();
            if (JsonTokenType.PropertyName == test.TokenType
                && test.ValueTextEquals("$ref"))
            {
                test.Read();
                var id = test.GetString();
                var resolve = PreserveReferenceHandler.Resolver;
                var obj = resolve.ResolveReference(id);
                reader.Skip();
                if (obj is T dest) return dest;
                throw new JsonException("type mismatch: " + id + " is not "+typeof(T).FullName);
            }
            return null;
        }

        /// <summary>Starts the definition of an object.  Caller writes the fields, then WriteEndObject.</summary>
        /// <param name="id">from TrySaveAsRef</param>
        static public void WriteStartDefinition(this Utf8JsonWriter writer, string id, string? type = null)
        {
            writer.WriteStartObject();
            writer.WriteString("$id", id);
            if (null != type) writer.WriteString("$type", type);
        }

        /// <summary>Consumes the leading $id and $type properties, in either order.</summary>
        /// <param name="reader">Precondition: on the first token after StartObject.  Postcondition: on the first non-metadata property, or EndObject</param>
        public static void ReadMetadata(this ref Utf8JsonReader reader, out string? id, out string? type)
        {
            id = null;
            type = null;
            while (JsonTokenType.PropertyName == reader.TokenType) {
                if (reader.ValueTextEquals("$id")) {
                    if (null != id) throw new JsonException("duplicate $id");
                    reader.Read();
                    id = reader.GetString();
                } else if (reader.ValueTextEquals("$type")) {
                    if (null != type) throw new JsonException("duplicate $type");
                    reader.Read();
                    type = reader.GetString();
                } else return;
                reader.Read();
            }
        }

        /// <summary>For the save, load, save again round-trip test.</summary>
        /// <returns>null if and only if the two texts agree; otherwise, where they first disagree</returns>
        public static string? FirstDifference(string[] first, string[] second)
        {
            var ub = Math.Min(first.Length, second.Length);
            for (var i = 0; i < ub; i++) {
                if (first[i] == second[i]) continue;
                return "line " + (i + 1).ToString() + "\nfirst save : " + first[i].Trim() + "\nsecond save: " + second[i].Trim();
            }
            if (first.Length == second.Length) return null;
            return "agree for " + ub.ToString() + " lines; first save has " + first.Length.ToString() + " lines, second save has " + second.Length.ToString();
        }

        /// <summary>Registers obj under src's $id, if it has one.</summary>
        public static void RecordRef<T>(this JsonElement src, T obj) where T : class
        {
            if (src.TryGetProperty("$id", out var relay_id)) (relay_id.GetString() ?? throw new JsonException("null $id")).RecordRef(obj);
        }

        /// <summary>like JsonElement.GetProperty, but reports a missing field as a JsonException</summary>
        public static JsonElement Require(this JsonElement src, string name)
        {
            if (src.TryGetProperty(name, out var ret)) return ret;
            throw new JsonException("missing field: " + name);
        }

        /// <summary>Reads an enum saved by name.  Numbers, and names that are not declared, are rejected.</summary>
        public static T RequireEnum<T>(this JsonElement src, string name) where T : struct, Enum
        {
            var stage = src.Require(name);
            if (JsonValueKind.String == stage.ValueKind) {
                var text = stage.GetString()!;
                if (0 < text.Length && !char.IsDigit(text[0]) && '-' != text[0] && Enum.TryParse<T>(text, out var ret)) return ret;
            }
            throw new JsonException("unrecognized " + typeof(T).Name + " for " + name + ": " + stage.GetRawText());
        }

        /// <summary>An owned collection, as a bare JSON array: no $id/$values wrapper, as it is never shared.</summary>
        public static void WriteArray<T>(this Utf8JsonWriter writer, string name, IEnumerable<T> src, JsonSerializerOptions options)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
            foreach (var x in src) JsonSerializer.Serialize(writer, x, options);
            writer.WriteEndArray();
        }

        public static void RecordRef<T>(this string relay_id, T src) where T : class {
            var resolve = PreserveReferenceHandler.Resolver;
            resolve.AddReference(relay_id, src);
        }
    }
}
