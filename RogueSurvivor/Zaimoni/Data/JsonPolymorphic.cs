using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

// Polymorphic save/load, for class hierarchies using hand-written System.Text.Json converters.
// A custom JsonConverter<Base> bypasses [JsonDerivedType] the same way it bypasses ReferenceHandler,
// so we write the $type discriminator ourselves.
//
// Usage: each concrete type T in the hierarchy implements IJsonPolymorphic<T>.  Then
//   var items = new Zaimoni.JSON.Polymorphic<Item>();
//   items.Add(new Zaimoni.JsonConvert.Via<Item, ItemFood>("ItemFood"));               // save/load believed complete
//   items.Add(new Zaimoni.JsonConvertIncomplete.Via<Item, ItemTracker>("ItemTracker"));  // not yet
//   options.Converters.Add(items);

namespace Zaimoni.JSON
{
    public interface IJsonPolymorphic<TSelf> where TSelf : class, IJsonPolymorphic<TSelf>
    {
        /// <param name="src">the whole JSON object, including $id and $type.  Implementations should src.RecordRef(this) before loading anything that could refer back.</param>
        static abstract TSelf fromJson(JsonElement src, JsonSerializerOptions options);
        /// <summary>Writes properties only; the object start, $id, $type, and object end are the dispatcher's.</summary>
        void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options);
    }

    /// <summary>Registry entry.  Completeness is signaled by which namespace the implementing class is in.</summary>
    public abstract class Subtype<TBase> where TBase : class
    {
        public readonly string Tag;

        protected Subtype(string tag)
        {
#if DEBUG
            if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag));
#endif
            Tag = tag;
        }

        public abstract Type Type { get; }
        public abstract TBase Load(JsonElement src, JsonSerializerOptions options);
        public abstract void Save(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options);

        public bool IsComplete { get => "Zaimoni.JsonConvert" == GetType().Namespace; }
    }

    public sealed class Polymorphic<TBase> : JsonConverter<TBase> where TBase : class
    {
        private readonly Dictionary<string, Subtype<TBase>> m_ByTag = new();
        private readonly Dictionary<Type, Subtype<TBase>> m_ByType = new();

        public Polymorphic<TBase> Add(Subtype<TBase> src)
        {
            if (m_ByTag.ContainsKey(src.Tag)) throw new InvalidOperationException("duplicate $type " + src.Tag + " for " + typeof(TBase).FullName);
            if (m_ByType.ContainsKey(src.Type)) throw new InvalidOperationException(src.Type.FullName + " registered twice for " + typeof(TBase).FullName);
            m_ByTag.Add(src.Tag, src);
            m_ByType.Add(src.Type, src);
            return this;
        }

        public bool IsComplete {
            get {
                foreach (var x in m_ByTag) if (!x.Value.IsComplete) return false;
                return true;
            }
        }

        /// <returns>the $type tags whose save/load is not yet complete</returns>
        public List<string> Incomplete {
            get {
                List<string> ret = new();
                foreach (var x in m_ByTag) if (!x.Value.IsComplete) ret.Add(x.Key);
                return ret;
            }
        }

        /// <summary>Fails at startup, rather than mid-save, when a concrete subtype was not registered.
        /// Generic type definitions are not checked: their closed instantiations have to be registered one by one.</summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public void RequireAllSubtypes()
        {
            List<string>? missing = null;
            foreach (var t in typeof(TBase).Assembly.GetTypes()) {
                if (t.IsAbstract || t.IsInterface || t.IsGenericTypeDefinition) continue;
                if (!typeof(TBase).IsAssignableFrom(t)) continue;
                if (!m_ByType.ContainsKey(t)) (missing ??= new()).Add(t.FullName ?? t.Name);
            }
            if (null != missing) throw new InvalidOperationException("not registered for JSON save/load of " + typeof(TBase).FullName + ": " + string.Join(", ", missing));
        }

        // fields declared as a derived type must dispatch as well
        public override bool CanConvert(Type src) => typeof(TBase).IsAssignableFrom(src);

        public override void Write(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options)
        {
            var id = src.TrySaveAsRef(writer);
            if (null == id) return;
            if (!m_ByType.TryGetValue(src.GetType(), out var entry)) throw new JsonException("not registered for JSON save of " + typeof(TBase).FullName + ": " + src.GetType().FullName);
            writer.WriteStartDefinition(id, entry.Tag);
            entry.Save(writer, src, options);
            writer.WriteEndObject();
        }

        public override TBase Read(ref Utf8JsonReader reader, Type want, JsonSerializerOptions options)
        {
            var src = JsonElement.ParseValue(ref reader);
            if (JsonValueKind.Object != src.ValueKind) throw new JsonException("expected object for " + want.FullName);

            TBase ret;
            if (src.TryGetProperty("$ref", out var relay_ref)) {
                var id = relay_ref.GetString() ?? throw new JsonException("null $ref");
                ret = PreserveReferenceHandler.Resolver.ResolveReference(id) as TBase ?? throw new JsonException("type mismatch: " + id + " is not " + typeof(TBase).FullName);
            } else {
                if (!src.TryGetProperty("$type", out var relay_type)) throw new JsonException("missing $type for " + typeof(TBase).FullName);
                var tag = relay_type.GetString() ?? throw new JsonException("null $type");
                if (!m_ByTag.TryGetValue(tag, out var entry)) throw new JsonException("unknown $type " + tag + " for " + typeof(TBase).FullName);
                ret = entry.Load(src, options);
            }
            if (!want.IsInstanceOfType(ret)) throw new JsonException("type mismatch: " + ret.GetType().FullName + " is not " + want.FullName);
            return ret;
        }
    }
}

namespace Zaimoni.JsonConvert
{
    public sealed class Via<TBase, T> : Zaimoni.JSON.Subtype<TBase> where TBase : class where T : class, TBase, Zaimoni.JSON.IJsonPolymorphic<T>
    {
        public Via(string tag) : base(tag) { }

        public override Type Type { get => typeof(T); }
        public override TBase Load(JsonElement src, JsonSerializerOptions options) => T.fromJson(src, options);
        public override void Save(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options) => ((T)src).toJsonFields(writer, options);
    }
}

namespace Zaimoni.JsonConvertIncomplete
{
    public sealed class Via<TBase, T> : Zaimoni.JSON.Subtype<TBase> where TBase : class where T : class, TBase, Zaimoni.JSON.IJsonPolymorphic<T>
    {
        public Via(string tag) : base(tag) { }

        public override Type Type { get => typeof(T); }
        public override TBase Load(JsonElement src, JsonSerializerOptions options) => T.fromJson(src, options);
        public override void Save(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options) => ((T)src).toJsonFields(writer, options);
    }
}
