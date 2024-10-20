// strategy is from https://stackoverflow.com/questions/69741572/resolve-cycle-references-of-complex-type-during-json-serialization-using-system
// Microsoft's auto-generated support for reference cycle handling is not available as-is to overrides based on
// System.Text.Json.Serialization.JsonConverter.  So...we just fork it.
// This file is under the MIT license.

// https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/PreserveReferenceHandler.cs
// https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/PreserveReferenceResolver.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

       private Dictionary<string, object> ReferenceToObject { get => _referenceIdToObjectMap ??= new(); }
       private Dictionary<object, string> ObjectToReference { get => _objectToReferenceIdMap ??= new(ReferenceEqualityComparer.Instance); }

       public PreserveReferenceResolver() { }

       public override void AddReference(string referenceId, object value)
       {
           if (!ReferenceToObject.TryAdd(referenceId, value)) throw new JsonException("duplicate id found: " + referenceId);
       }

       public override string GetReference(object value, out bool alreadyExists)
       {
           alreadyExists = ObjectToReference.TryGetValue(value, out string? referenceId);
           if (!alreadyExists) {
               _referenceCount++;
               referenceId = _referenceCount.ToString();
               _objectToReferenceIdMap!.Add(value, referenceId);
           }
           return referenceId!;
       }

       public override object ResolveReference(string referenceId)
       {
            if (null == _referenceIdToObjectMap) throw new JsonException("id not found: " + referenceId);

            if (_referenceIdToObjectMap.TryGetValue(referenceId, out object? value)) return value;

            throw new JsonException("id not found: " + referenceId);
       }
   }

   internal sealed class PreserveReferenceHandler : ReferenceHandler
   {
       static private PreserveReferenceResolver? s_resolve = null;

       public override ReferenceResolver CreateResolver() => (s_resolve ??= new());

       static public void Reset() => s_resolve = null;
       static public PreserveReferenceResolver Resolver { get => (s_resolve ??= new()); }
   }
}
