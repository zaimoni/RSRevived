using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.Design.AxImporter;

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
            var id = resolve.GetReference(src);
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

        public static void RecordRef<T>(this string relay_id, T src) where T : class {
            var resolve = PreserveReferenceHandler.Resolver;
            resolve.AddReference(relay_id, src);
        }
    }
}
