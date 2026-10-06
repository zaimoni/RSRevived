using System;
using System.Buffers;
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

        /// <summary>For an enum used as a JSON property name, or as a bare string in an array.  Same rules as RequireEnum.</summary>
        public static T ParseEnum<T>(string? text) where T : struct, Enum
        {
            if (!string.IsNullOrEmpty(text) && !char.IsDigit(text[0]) && '-' != text[0] && Enum.TryParse<T>(text, out var ret)) return ret;
            throw new JsonException("unrecognized " + typeof(T).Name + ": " + text);
        }

        /// <summary>An owned collection, as a bare JSON array: no $id/$values wrapper, as it is never shared.</summary>
        public static void WriteArray<T>(this Utf8JsonWriter writer, string name, IEnumerable<T> src, JsonSerializerOptions options)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
            foreach (var x in src) JsonSerializer.Serialize(writer, x, options);
            writer.WriteEndArray();
        }

        /// <summary>a bare array, as a value: the default collection path would wrap it in {"$id", "$values"}</summary>
        public static void WriteArrayValue<T>(this Utf8JsonWriter writer, IEnumerable<T> src, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var x in src) JsonSerializer.Serialize(writer, x, options);
            writer.WriteEndArray();
        }

        /// <summary>a bare array of bare arrays</summary>
        public static void WriteArrayOfArrays<T>(this Utf8JsonWriter writer, string name, IEnumerable<IEnumerable<T>> src, JsonSerializerOptions options)
        {
            writer.WriteStartArray(name);
            foreach (var x in src) writer.WriteArrayValue(x, options);
            writer.WriteEndArray();
        }

        /// <summary>An array of already-rendered JSON values, one per line.
        /// For bulk data, where the writer's own indenting would put every number on its own line.</summary>
        public static void WriteArrayOfLines(this Utf8JsonWriter writer, IEnumerable<string> src)
        {
            var opts = writer.Options;
            var depth = writer.CurrentDepth;
            string indent(int n) => opts.Indented ? opts.NewLine + new string(opts.IndentCharacter, opts.IndentSize * n) : string.Empty;
            var sep = indent(depth + 1);
            var stage = new StringBuilder();
            stage.Append('[');
            bool empty = true;
            foreach (var x in src) {
                if (!empty) stage.Append(',');
                empty = false;
                stage.Append(sep);
                stage.Append(x);
            }
            if (!empty) stage.Append(indent(depth));
            stage.Append(']');
            writer.WriteRawValue(stage.ToString());
        }

#region cross-links
        // A cross-link refers to an object that some other part of the save owns, and so defines: only a $ref is written here.
        // The owner may come later in the file.  On load, cross-links are filled in by PreserveReferenceHandler.Finish,
        // in file order; until then the field, list or dictionary they fill is empty.

        /// <summary>a reference, as text for WriteArrayOfLines</summary>
        static public string RenderRef<T>(T src) where T : class => "{\"$ref\":\"" + PreserveReferenceHandler.Resolver.Reference(src) + "\"}";

        /// <summary>A value, which has no identity, on one line: rendered without indenting, then written raw.  Entities, which have
        /// $id, stay blocks.  Inside a compact rendering the writer is already unindented, so a nested value writes directly.</summary>
        public static void WriteCompact(this Utf8JsonWriter writer, Action<Utf8JsonWriter> body)
        {
            if (!writer.Options.Indented) {
                body(writer);
                return;
            }
            var buffer = s_compact_buffer ??= new();
            buffer.ResetWrittenCount();
            var compact = s_compact_writer;
            if (null == compact || compact.Options.Encoder != writer.Options.Encoder) {
                compact?.Dispose();
                compact = s_compact_writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false, Encoder = writer.Options.Encoder });
            } else compact.Reset(buffer);
            body(compact);
            compact.Flush();
            writer.WriteRawValue(buffer.WrittenSpan, true);
        }

        [ThreadStatic] private static ArrayBufferWriter<byte>? s_compact_buffer;
        [ThreadStatic] private static Utf8JsonWriter? s_compact_writer;

        /// <summary>a point, as text: [x,y]</summary>
        static public string RenderPoint(Zaimoni.Data.Vector2D<short> src) => "[" + src.X.ToString() + "," + src.Y.ToString() + "]";

        /// <summary>points, as text for WriteArrayOfLines: [[x,y],[x,y]]</summary>
        static public string RenderPoints(IEnumerable<Zaimoni.Data.Vector2D<short>> src)
        {
            var ret = new StringBuilder("[");
            foreach (var pt in src) {
                if (1 < ret.Length) ret.Append(',');
                ret.Append('[').Append(pt.X).Append(',').Append(pt.Y).Append(']');
            }
            return ret.Append(']').ToString();
        }

        static public Zaimoni.Data.Vector2D<short> ReadPoint(this JsonElement src)
        {
            if (JsonValueKind.Array != src.ValueKind || 2 != src.GetArrayLength()) throw new JsonException("expected [x, y]: " + src.GetRawText());
            return new(src[0].GetInt16(), src[1].GetInt16());
        }

        static public void WriteRef<T>(this Utf8JsonWriter writer, string name, T src) where T : class
        {
            writer.WritePropertyName(name);
            src.SaveAsRef(writer);
        }

        /// <summary>one reference per line</summary>
        static public void WriteRefs<T>(this Utf8JsonWriter writer, string name, IEnumerable<T> src) where T : class
        {
            List<string> rows = new();
            foreach (var x in src) rows.Add(RenderRef(x));
            writer.WritePropertyName(name);
            writer.WriteArrayOfLines(rows);
        }

        /// <summary>Each entry is [reference, value], one entry per line.  Intended for values that are numbers or strings.</summary>
        static public void WriteRefDictionary<K, V>(this Utf8JsonWriter writer, string name, IEnumerable<KeyValuePair<K, V>> src, JsonSerializerOptions options) where K : class
        {
            List<string> rows = new();
            foreach (var x in src) rows.Add("[" + RenderRef(x.Key) + "," + JsonSerializer.Serialize(x.Value, options) + "]");
            writer.WritePropertyName(name);
            writer.WriteArrayOfLines(rows);
        }

        static public string RefId(this JsonElement src)
        {
            if (JsonValueKind.Object == src.ValueKind && src.TryGetProperty("$ref", out var id)) return id.GetString() ?? throw new JsonException("null $ref");
            throw new JsonException("expected a reference: " + src.GetRawText());
        }

        /// <param name="assign">runs when the load finishes, not now</param>
        static public void ReadRef<T>(this JsonElement src, Action<T> assign) where T : class => PreserveReferenceHandler.Resolver.Defer(src.RefId(), assign);

        /// <summary>Resolves now, for a reference that must be in hand to construct what holds it (a struct, a readonly field).
        /// The referent has to be loaded already: the file order guarantees that, or this throws.</summary>
        static public T RequireRef<T>(this JsonElement src) where T : class
        {
            var id = src.RefId();
            if (!PreserveReferenceHandler.Resolver.TryResolve(id, out var obj)) throw new JsonException("not yet loaded: " + id);
            return obj as T ?? throw new JsonException("type mismatch: " + id + " is not " + typeof(T).FullName);
        }

        static public List<T> RequireRefs<T>(this JsonElement src) where T : class
        {
            List<T> ret = new();
            foreach (var x in src.EnumerateArray()) ret.Add(x.RequireRef<T>());
            return ret;
        }

        /// <param name="dest">filled when the load finishes, in file order</param>
        static public void ReadRefs<T>(this JsonElement src, ICollection<T> dest) where T : class
        {
            foreach (var x in src.EnumerateArray()) x.ReadRef<T>(dest.Add);
        }

        /// <param name="dest">filled when the load finishes, in file order</param>
        static public void ReadRefDictionary<K, V>(this JsonElement src, IDictionary<K, V> dest, JsonSerializerOptions options) where K : class
        {
            foreach (var x in src.EnumerateArray()) {
                if (JsonValueKind.Array != x.ValueKind || 2 != x.GetArrayLength()) throw new JsonException("expected [reference, value]: " + x.GetRawText());
                var value = x[1].Deserialize<V>(options)!;
                x[0].ReadRef<K>(key => dest.Add(key, value));
            }
        }

        /// <summary>[key, reference] per line: the values are references</summary>
        static public void WriteRefValueDictionary<K, V>(this Utf8JsonWriter writer, string name, IEnumerable<KeyValuePair<K, V>> src, JsonSerializerOptions options) where V : class
        {
            writer.WriteStartArray(name);
            foreach (var x in src) {
                writer.WriteCompact(w => {
                    w.WriteStartArray();
                    JsonSerializer.Serialize(w, x.Key, options);
                    x.Value.SaveAsRef(w);
                    w.WriteEndArray();
                });
            }
            writer.WriteEndArray();
        }

        /// <param name="dest">filled when the load finishes, in file order</param>
        static public void ReadRefValueDictionary<K, V>(this JsonElement src, IDictionary<K, V> dest, JsonSerializerOptions options) where V : class
        {
            foreach (var x in src.EnumerateArray()) {
                if (JsonValueKind.Array != x.ValueKind || 2 != x.GetArrayLength()) throw new JsonException("expected [key, reference]: " + x.GetRawText());
                var key = x[0].Deserialize<K>(options)!;
                x[1].ReadRef<V>(value => dest.Add(key, value));
            }
        }
#endregion

        public static void RecordRef<T>(this string relay_id, T src) where T : class {
            var resolve = PreserveReferenceHandler.Resolver;
            resolve.AddReference(relay_id, src);
        }
    }
}
