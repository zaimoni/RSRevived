using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

// Polymorphic save/load, for class hierarchies using hand-written System.Text.Json converters.
// A custom JsonConverter<Base> bypasses [JsonDerivedType] the same way it bypasses ReferenceHandler,
// so we write the $type discriminator ourselves.
//
// Usage: each concrete type T in the hierarchy implements IJsonStaged<T>.  Then
//   var items = new Zaimoni.JSON.Polymorphic<Item>();
//   items.Add(new Zaimoni.JsonConvert.Via<Item, ItemFood>("ItemFood"));               // save/load believed complete
//   items.Add(new Zaimoni.JsonConvertIncomplete.Via<Item, ItemTracker>("ItemTracker"));  // not yet
//   options.Converters.Add(items);

namespace Zaimoni.JSON
{
    public interface IJsonStaged<TSelf> where TSelf : class, IJsonStaged<TSelf>
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

    /// <summary>non-generic view of Polymorphic, for auditing</summary>
    public interface IPolymorphic
    {
        Type Root { get; }
        bool IsComplete { get; }
        /// <returns>the $type tags whose save/load is not yet complete</returns>
        List<string> Incomplete { get; }
    }

    static public class Audit
    {
        /// <summary>What still blocks these options from being a real save format.
        /// Only sees registered converters: a type with no converter at all is not listed.</summary>
        static public List<string> IncompleteConverters(this JsonSerializerOptions options)
        {
            List<string> ret = new();
            foreach (var x in options.Converters) {
                if (x is IPolymorphic poly) {
                    foreach (var tag in poly.Incomplete) ret.Add(poly.Root.Name + ": " + tag);
                    continue;
                }
                var t = x.GetType();
                if ("Zaimoni.JsonConvert" == t.Namespace) continue;
                if (t.IsGenericType) ret.Add(t.GetGenericArguments()[0].Name);  // e.g. Sealed<T>
                else ret.Add(t.Name);
            }
            return ret;
        }
    }

    /// <summary>Shared implementation of Zaimoni.JsonConvert.Sealed and Zaimoni.JsonConvertIncomplete.Sealed</summary>
    public abstract class SealedBase<T> : JsonConverter<T> where T : class, IJsonStaged<T>
    {
        /// <param name="compactAnonymous">an object nothing refers to is a value: one line, where that reads well</param>
        protected SealedBase(bool compactAnonymous) { m_CompactAnonymous = compactAnonymous; }

        private readonly bool m_CompactAnonymous;

        public override void Write(Utf8JsonWriter writer, T src, JsonSerializerOptions options)
        {
            if (!src.TryStartDefinition(writer, out var id)) return;
            if (null == id && m_CompactAnonymous) {
                writer.WriteCompact(w => {
                    w.WriteStartDefinition(null);
                    src.toJsonFields(w, options);
                    w.WriteEndObject();
                });
                return;
            }
            writer.WriteStartDefinition(id);
            src.toJsonFields(writer, options);
            writer.WriteEndObject();
        }

        public override T Read(ref Utf8JsonReader reader, Type want, JsonSerializerOptions options)
        {
            var src = JsonElement.ParseValue(ref reader);
            if (JsonValueKind.Object != src.ValueKind) throw new JsonException("expected object for " + typeof(T).FullName);
            if (!src.TryGetProperty("$ref", out var relay_ref)) return T.fromJson(src, options);
            var id = relay_ref.GetString() ?? throw new JsonException("null $ref");
            return PreserveReferenceHandler.Resolver.ResolveReference(id) as T ?? throw new JsonException("type mismatch: " + id + " is not " + typeof(T).FullName);
        }
    }

    public sealed class Polymorphic<TBase> : JsonConverter<TBase>, IPolymorphic where TBase : class
    {
        public Type Root { get => typeof(TBase); }

        private readonly Dictionary<string, Subtype<TBase>> m_ByTag = new();
        private readonly Dictionary<Type, Subtype<TBase>> m_ByType = new();
        private readonly bool m_CompactAnonymous;

        public Polymorphic() : this(false) { }

        /// <param name="compactAnonymous">an object nothing refers to is a value: one line, where that reads well (items, map objects, actions)</param>
        public Polymorphic(bool compactAnonymous) { m_CompactAnonymous = compactAnonymous; }

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
        public void RequireAllSubtypes() => RequireAllSubtypes(t => true);

        /// <param name="wanted">which concrete subtypes belong in a save; the others must not be registered</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public void RequireAllSubtypes(Predicate<Type> wanted)
        {
            List<string>? missing = null;
            List<string>? extra = null;
            foreach (var t in typeof(TBase).Assembly.GetTypes()) {
                if (t.IsAbstract || t.IsInterface || t.IsGenericTypeDefinition) continue;
                if (!typeof(TBase).IsAssignableFrom(t)) continue;
                if (wanted(t)) {
                    if (!m_ByType.ContainsKey(t)) (missing ??= new()).Add(t.FullName ?? t.Name);
                } else if (m_ByType.ContainsKey(t)) (extra ??= new()).Add(t.FullName ?? t.Name);
            }
            if (null != missing) throw new InvalidOperationException("not registered for JSON save/load of " + typeof(TBase).FullName + ": " + string.Join(", ", missing));
            if (null != extra) throw new InvalidOperationException("registered for JSON save/load of " + typeof(TBase).FullName + " but not meant to be saved: " + string.Join(", ", extra));
        }

        // fields declared as a derived type must dispatch as well
        public override bool CanConvert(Type src) => typeof(TBase).IsAssignableFrom(src);

        public override void Write(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options)
        {
            if (!src.TryStartDefinition(writer, out var id)) return;
            if (!m_ByType.TryGetValue(src.GetType(), out var entry)) throw new JsonException("not registered for JSON save of " + typeof(TBase).FullName + ": " + src.GetType().FullName);
            var tag = typeof(TBase) == src.GetType() ? null : entry.Tag;   // the root class is implied
            if (null == id && m_CompactAnonymous) {
                writer.WriteCompact(w => {
                    w.WriteStartDefinition(null, tag);
                    entry.Save(w, src, options);
                    w.WriteEndObject();
                });
                return;
            }
            writer.WriteStartDefinition(id, tag);
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
                Subtype<TBase>? entry;
                if (src.TryGetProperty("$type", out var relay_type)) {
                    var tag = relay_type.GetString() ?? throw new JsonException("null $type");
                    if (!m_ByTag.TryGetValue(tag, out entry)) throw new JsonException("unknown $type " + tag + " for " + typeof(TBase).FullName);
                } else if (!m_ByType.TryGetValue(typeof(TBase), out entry)) throw new JsonException("missing $type for " + typeof(TBase).FullName);   // no $type: the root class, when it is itself registered
                ret = entry.Load(src, options);
            }
            if (!want.IsInstanceOfType(ret)) throw new JsonException("type mismatch: " + ret.GetType().FullName + " is not " + want.FullName);
            return ret;
        }
    }
}

namespace Zaimoni.JsonConvert
{
    /// <summary>For a class with identity ($id/$ref) but no subclasses: no $type is written.</summary>
    public sealed class Sealed<T> : Zaimoni.JSON.SealedBase<T> where T : class, Zaimoni.JSON.IJsonStaged<T>
    {
        public Sealed() : base(false) { }
        public Sealed(bool compactAnonymous) : base(compactAnonymous) { }
    }

    public sealed class Via<TBase, T> : Zaimoni.JSON.Subtype<TBase> where TBase : class where T : class, TBase, Zaimoni.JSON.IJsonStaged<T>
    {
        public Via(string tag) : base(tag) { }

        public override Type Type { get => typeof(T); }
        public override TBase Load(JsonElement src, JsonSerializerOptions options) => T.fromJson(src, options);
        public override void Save(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options) => ((T)src).toJsonFields(writer, options);
    }
}

namespace Zaimoni.JsonConvertIncomplete
{
    /// <summary>For a class with identity ($id/$ref) but no subclasses: no $type is written.</summary>
    public sealed class Sealed<T> : Zaimoni.JSON.SealedBase<T> where T : class, Zaimoni.JSON.IJsonStaged<T>
    {
        public Sealed() : base(false) { }
        public Sealed(bool compactAnonymous) : base(compactAnonymous) { }
    }

    public sealed class Via<TBase, T> : Zaimoni.JSON.Subtype<TBase> where TBase : class where T : class, TBase, Zaimoni.JSON.IJsonStaged<T>
    {
        public Via(string tag) : base(tag) { }

        public override Type Type { get => typeof(T); }
        public override TBase Load(JsonElement src, JsonSerializerOptions options) => T.fromJson(src, options);
        public override void Save(Utf8JsonWriter writer, TBase src, JsonSerializerOptions options) => ((T)src).toJsonFields(writer, options);
    }
}
