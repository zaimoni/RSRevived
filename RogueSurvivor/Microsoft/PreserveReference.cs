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
       // loading: a reference can be read before its definition
       private Dictionary<string, List<Action<object>>>? _fixups = null;

       private Dictionary<string, object> ReferenceToObject { get => _referenceIdToObjectMap ??= new(); }
       private Dictionary<object, string> ObjectToReference { get => _objectToReferenceIdMap ??= new(ReferenceEqualityComparer.Instance); }
       private HashSet<object> Defined { get => _defined ??= new(ReferenceEqualityComparer.Instance); }

       public PreserveReferenceResolver() { }

       public override void AddReference(string referenceId, object value)
       {
           if (!ReferenceToObject.TryAdd(referenceId, value)) throw new JsonException("duplicate id found: " + referenceId);
           if (null != _fixups && _fixups.Remove(referenceId, out var pending)) {
               foreach (var fixup in pending) fixup(value);
           }
       }

       /// <summary>the id for value, assigning one if needed.  Does not claim the definition has been written.</summary>
       public string Reference(object value)
       {
           if (!ObjectToReference.TryGetValue(value, out string? referenceId)) {
               _referenceCount++;
               referenceId = _referenceCount.ToString();
               _objectToReferenceIdMap!.Add(value, referenceId);
           }
           return referenceId;
       }

       /// <param name="alreadyExists">false if and only if the caller is now responsible for writing the definition</param>
       public override string GetReference(object value, out bool alreadyExists)
       {
           var referenceId = Reference(value);
           alreadyExists = !Defined.Add(value);
           return referenceId;
       }

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

       /// <summary>runs fixup immediately if the id is already loaded; otherwise, when it is loaded.</summary>
       public void Defer<T>(string referenceId, Action<T> fixup) where T:class
       {
           void relay(object src) {
               if (src is T dest) fixup(dest);
               else throw new JsonException("type mismatch: " + referenceId + " is not " + typeof(T).FullName);
           }

           if (TryResolve(referenceId, out object? value)) {
               relay(value!);
               return;
           }
           _fixups ??= new();
           if (!_fixups.TryGetValue(referenceId, out var pending)) _fixups.Add(referenceId, pending = new());
           pending.Add(relay);
       }

       /// <summary>null if and only if the save, or load, was self-consistent.</summary>
       public string? Errors()
       {
           List<string>? err = null;
           if (null != _objectToReferenceIdMap) {
               foreach (var x in _objectToReferenceIdMap) {
                   if (null != _defined && _defined.Contains(x.Key)) continue;
                   (err ??= new()).Add("referenced but never saved: " + x.Value + " (" + x.Key.GetType().FullName + ")");
               }
           }
           if (null != _fixups) {
               foreach (var x in _fixups) (err ??= new()).Add("referenced but never loaded: " + x.Key);
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

       /// <summary>call after each complete save, or load.  Resets even when it throws.</summary>
       static public void Finish()
       {
           var err = s_resolve?.Errors();
           s_resolve = null;
           if (null != err) throw new JsonException(err);
       }
   }
}
