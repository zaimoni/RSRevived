// strategy is from https://stackoverflow.com/questions/69741572/resolve-cycle-references-of-complex-type-during-json-serialization-using-system
// Microsoft's auto-generated support for reference cycle handling is not available as-is to overrides based on
// System.Text.Json.Serialization.JsonConverter.  So...we just fork it.
// This file is under the MIT license.

// https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/PreserveReferenceHandler.cs
// https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/PreserveReferenceResolver.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace Zaimoni.JSON
{
   internal sealed class PreserveReferenceResolver : ReferenceResolver
   {
       private uint _referenceCount = 0;
       private Dictionary<string, object>? _referenceIdToObjectMap = null;
       private Dictionary<object, string>? _objectToReferenceIdMap = null;
       // saving: an id can be handed out (forward reference) before its definition is written
       private HashSet<object>? _defined = null;
       // Saving is two passes.  The first (wanted null) hands every definition an id and records what was referenced; the second
       // (wanted: that set) hands ids only to those, densely numbered, and an object nothing refers to is written without one.
       private readonly HashSet<object>? _wanted;
       private HashSet<object>? _referenced = null;
       // loading: a reference can be read before its definition.  These run in the order they were deferred.
       private List<KeyValuePair<string, Action<object>>>? _fixups = null;

       private Dictionary<string, object> ReferenceToObject { get => _referenceIdToObjectMap ??= new(); }
       private Dictionary<object, string> ObjectToReference { get => _objectToReferenceIdMap ??= new(ReferenceEqualityComparer.Instance); }
       private HashSet<object> Defined { get => _defined ??= new(ReferenceEqualityComparer.Instance); }

       public PreserveReferenceResolver(HashSet<object>? wanted = null) { _wanted = wanted; }

       /// <summary>what Reference was asked for: the objects something refers to</summary>
       public HashSet<object> Referenced { get => _referenced ??= new(ReferenceEqualityComparer.Instance); }

       public override void AddReference(string referenceId, object value)
       {
           if (!ReferenceToObject.TryAdd(referenceId, value)) throw new JsonException("duplicate id found: " + referenceId);
       }

       /// <summary>the id for a reference to value, assigning one if needed.  Does not claim the definition has been written.</summary>
       public string Reference(object value)
       {
           Referenced.Add(value);
           if (!ObjectToReference.TryGetValue(value, out string? referenceId)) {
               if (null != _wanted && !_wanted.Contains(value)) throw new JsonException("referenced in the second pass of the save but not the first: " + value.GetType().FullName + ": " + value.ToString());
               referenceId = NewId(value);
           }
           return referenceId;
       }

       private string NewId(object value)
       {
           _referenceCount++;
           var referenceId = _referenceCount.ToString();
           ObjectToReference.Add(value, referenceId);
           return referenceId;
       }

       /// <summary>for a definition</summary>
       /// <param name="alreadyExists">false if and only if the caller is now responsible for writing the definition</param>
       /// <returns>the id to write; null when nothing refers to value (second pass), which is then written without one</returns>
       public string? Define(object value, out bool alreadyExists)
       {
           alreadyExists = !Defined.Add(value);
           if (alreadyExists) return Reference(value);   // written already: this is a reference to it
           if (ObjectToReference.TryGetValue(value, out string? referenceId)) return referenceId;   // referenced before defined
           if (null != _wanted && !_wanted.Contains(value)) return null;
           return NewId(value);
       }

       /// <summary>System.Text.Json's own path, for a reference type it serializes itself: always an id</summary>
       public override string GetReference(object value, out bool alreadyExists)
           => Define(value, out alreadyExists) ?? (ObjectToReference.TryGetValue(value, out string? referenceId) ? referenceId : NewId(value));

       public bool TryResolve(string referenceId, out object? value)
       {
           if (null == _referenceIdToObjectMap) {
               value = null;
               return false;
           }
           return _referenceIdToObjectMap.TryGetValue(referenceId, out value);
       }

       public override object ResolveReference(string referenceId)
       {
            if (TryResolve(referenceId, out object? value)) return value!;

            throw new JsonException("id not found: " + referenceId);
       }

       /// <summary>Queues fixup for RunFixups, even when the id is already loaded.
       /// Running some fixups at once and others later would reorder the lists and dictionaries they fill.</summary>
       public void Defer<T>(string referenceId, Action<T> fixup) where T:class
       {
           void relay(object src) {
               if (src is T dest) fixup(dest);
               else throw new JsonException("type mismatch: " + referenceId + " is not " + typeof(T).FullName);
           }

           (_fixups ??= new()).Add(new(referenceId, relay));
       }

       /// <summary>Runs the deferred fixups, in the order they were deferred.  Call once everything is loaded.
       /// Those whose id never loaded are left for Errors to report.</summary>
       public void RunFixups()
       {
           if (null == _fixups) return;
           List<KeyValuePair<string, Action<object>>>? unresolved = null;
           for (int i = 0; i < _fixups.Count; i++) {   // a fixup is allowed to defer more
               var x = _fixups[i];
               if (TryResolve(x.Key, out object? value)) x.Value(value!);
               else (unresolved ??= new()).Add(x);
           }
           _fixups = unresolved;
       }

       /// <summary>null if and only if the save, or load, was self-consistent.</summary>
       public string? Errors()
       {
           List<string>? err = null;
           if (null != _objectToReferenceIdMap) {
               foreach (var x in _objectToReferenceIdMap) {
                   if (null != _defined && _defined.Contains(x.Key)) continue;
                   (err ??= new()).Add("referenced but never saved: " + x.Value + " (" + x.Key.GetType().FullName + ": " + x.Key.ToString() + ")");
               }
           }
           if (null != _fixups) {
               string? last = null;
               foreach (var x in _fixups) {
                   if (x.Key == last) continue;
                   (err ??= new()).Add("referenced but never loaded: " + (last = x.Key));
               }
           }
           return null == err ? null : string.Join("\n", err);
       }
   }

   internal sealed class PreserveReferenceHandler : ReferenceHandler
   {
       static private PreserveReferenceResolver? s_resolve = null;

       public override ReferenceResolver CreateResolver() => (s_resolve ??= new());

       static public void Reset() => s_resolve = null;
       static public PreserveReferenceResolver Resolver { get => (s_resolve ??= new()); }

       /// <summary>The first of a save's two passes: run it to nowhere, so that the resolver learns which objects anything refers to.
       /// Leaves the resolver primed for the second pass, which hands ids only to those; Finish, or Reset, as usual after it.</summary>
       static public void PrimeFromCountingPass(Action pass)
       {
           var counting = s_resolve = new();
           try {
               pass();
           } catch {
               s_resolve = null;
               throw;
           }
           s_resolve = new(counting.Referenced);
       }

       /// <summary>call after each complete save, or load.  Runs the deferred fixups of a load.  Resets even when it throws.</summary>
       static public void Finish()
       {
           string? err;
           try {
               s_resolve?.RunFixups();
               err = s_resolve?.Errors();
           } finally {
               s_resolve = null;
           }
           if (null != err) throw new JsonException(err);
       }
   }
}
