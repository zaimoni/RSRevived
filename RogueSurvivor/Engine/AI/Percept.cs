// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.Percept
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine.AI
{
  [Serializable]
  public class WhereWhen : Fn_to_s, ILocation
    {
    // MemorizedSensor needs public setters for these
    public int Turn { get; set; }
    public Location Location { get; set; }

    public WhereWhen(in Location loc, int t0)
    {
#if DEBUG
      if (0 > t0) throw new ArgumentOutOfRangeException(nameof(t0),t0, "0 > t0");
      if (null == loc.Map) throw new ArgumentNullException(nameof(loc.Map));
#endif
      Turn = t0;
      Location = loc;
    }

    public int GetAge(int t1)
    {
      return Math.Max(0, t1 - Turn);
    }

    public override string ToString()
    {
      return Turn.ToString()+": "+Location.ToString();
    }

    public string to_s() => ToString();
  }

  [Serializable]
  public sealed class Percept_<_T_> : WhereWhen
  {
    readonly private _T_ m_Percepted;

    public _T_ Percepted { get { return m_Percepted; } }

    public Percept_(_T_ percepted, int turn, in Location location)
     : base(in location,turn)
    {
#if DEBUG
      if (null == percepted) throw new ArgumentNullException(nameof(percepted));
#endif
      m_Percepted = percepted;
    }

    public override string ToString()
    {
      return Percepted.ToString()+" @ "+base.ToString();
    }
  }

    [Serializable]
    public struct Percept_s<_T_> {
        public int Turn;
        public Location Location;
        readonly public _T_ Percepted;

        public Percept_s(_T_ src, int t0, Location loc) {
            Turn = t0;
            Location = loc;
            Percepted = src;
        }

        public readonly int GetAge(int t1) => Math.Max(0, t1 - Turn);
        public override string ToString() => Percepted.ToString() + " @ " + Turn.ToString() + ": " + Location.ToString();
    }

    // in general, the tmp.Any() ? tmp.ToList() : null construct requires that the Any() call be de facto deterministic; RNG can easily break things
    internal static class ext_Percept
  {
#nullable enable
    internal static List<_T_>? Filter<_T_>(this IEnumerable<_T_> percepts, Func<_T_,bool> predicateFn) where _T_:WhereWhen
    {
      if (!percepts.Any()) return null;
      IEnumerable<_T_> tmp = percepts.Where(predicateFn);
      return tmp.Any() ? tmp.ToList() : null;
    }

    internal static List<Percept_<_T_>>? Filter<_T_>(this IEnumerable<Percept_<_T_>> percepts, Func<_T_,bool> test) where _T_:class
    {
      if (!percepts.Any()) return null;
      List<Percept_<_T_>>? ret = null;
      foreach(var p in percepts) if (test(p.Percepted)) (ret ??= new List<Percept_<_T_>>()).Add(p);
      return ret;
    }

    internal static List<_T_>? FilterCurrent<_T_>(this IEnumerable<_T_> percepts, int turn) where _T_:WhereWhen
    {
      if (!percepts.Any()) return null;
      List<_T_>? ret = null;
      foreach(var p in percepts) if (turn == p.Turn) (ret ??= new List<_T_>()).Add(p);
      return ret;
    }

    internal static List<_T_>? FilterOld<_T_>(this IEnumerable<_T_> percepts, int turn) where _T_:WhereWhen
    {
      if (!percepts.Any()) return null;
      List<_T_>? ret = null;
      foreach(var p in percepts) if (turn > p.Turn) (ret ??= new List<_T_>()).Add(p);
      return ret;
    }

	internal static List<Percept_<object>>? FilterT<_T_>(this IEnumerable<Percept_<object>> percepts) where _T_:class
	{
      if (!percepts.Any()) return null;
      List<Percept_<object>>? ret = null;
      foreach(var p in percepts) if (p.Percepted is _T_) (ret ??= new List<Percept_<object>>()).Add(p);
      return ret;
	}

	internal static List<Percept_<object>>? FilterT<_T_>(this IEnumerable<Percept_<object>>? percepts, Predicate<_T_> fn) where _T_:class
	{
      if (null == percepts || !percepts.Any()) return null;
      List<Percept_<object>>? ret = null;
      foreach(var p in percepts) if (p.Percepted is _T_ test && fn(test)) (ret ??= new List<Percept_<object>>()).Add(p);
      return ret;
	}

	internal static bool Any<_T_>(this IEnumerable<Percept_<_T_>>? percepts, Predicate<_T_> fn) where _T_:class
	{
      if (null == percepts || !percepts.Any()) return false;
      foreach(var p in percepts) if (fn(p.Percepted)) return true;
      return false;
	}

#if DEAD_FUNC
	internal static Zaimoni.Data.Stack<_T_> ToZStack<_T_>(this IEnumerable<Percept_<object>>? percepts, Predicate<_T_> fn) where _T_:class
	{
      if (null == percepts || !percepts.Any()) throw new ArgumentNullException(nameof(percepts));
      var ret = new Zaimoni.Data.Stack<_T_>(new _T_[percepts.Count()]);
      foreach(var p in percepts) if (p.Percepted is _T_ test && fn(test)) ret.push(test);
      return ret;
	}
#endif

    // for completeness
	internal static List<Percept_<_T_>>? FilterCast<_T_>(this IEnumerable<Percept_<object>> percepts, Predicate<_T_> fn) where _T_:class
	{
      if (!percepts.Any()) return null;
      List<Percept_<_T_>>? ret = null;
      foreach(var p in percepts) {
        // XXX arguably should be tmp.Count() but unclear how CPU vs. GC thrashing works here
        if (p.Percepted is _T_ test && fn(test)) (ret ??= new()).Add(new(test, p.Turn, p.Location));
      }
	  return ret;
	}

	internal static List<Percept_<_T_>>? FilterCast<_T_>(this IEnumerable<Percept_<object>> percepts) where _T_:class
	{
      if (!percepts.Any()) return null;
      List<Percept_<_T_>>? ret = null;
      foreach(var p in percepts) {
        // XXX arguably should be tmp.Count() but unclear how CPU vs. GC thrashing works here
        if (p.Percepted is _T_ test) (ret ??= new()).Add(new(test, p.Turn, p.Location));
      }
	  return ret;
	}

    internal static _T_? FilterFirst<_T_>(this IEnumerable<_T_> percepts, Predicate<_T_> predicateFn) where _T_:WhereWhen
    {
      if (!percepts.Any()) return null;
      foreach(var percept in percepts) if (predicateFn(percept)) return percept;
      return null;
    }
#nullable restore
  }
}

namespace Zaimoni.JsonConvert
{
    using djack.RogueSurvivor.Data;
    using System.Text.Json;
    using Zaimoni.JSON;
    using Loc = djack.RogueSurvivor.Data.Location;   // Location here is the converter

    /// <summary>a percept of an actor by tag: the actor itself may be long gone</summary>
    public sealed class PerceptOfTag : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.AI.Percept_<ActorTag>>
    {
        public override djack.RogueSurvivor.Engine.AI.Percept_<ActorTag> Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            var stage = JsonElement.ParseValue(ref reader);
            return new(stage.Require("ActorTag").Deserialize<ActorTag>(options), stage.Require("Turn").GetInt32(), stage.Require("Location").Deserialize<Loc>(options));
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.AI.Percept_<ActorTag> src, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Turn", src.Turn);
            writer.WritePropertyName("Location");
            JsonSerializer.Serialize(writer, src.Location, options);
            writer.WritePropertyName("ActorTag");
            JsonSerializer.Serialize(writer, src.Percepted, options);
            writer.WriteEndObject();
        }
    }

    /// <summary>A percept's payload is one of a closed set of unrelated types, so the key naming it says which: no $type.</summary>
    public sealed class Percept<T> : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.AI.Percept_<T>> where T : class
    {
        public override djack.RogueSurvivor.Engine.AI.Percept_<T> Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            var stage = JsonElement.ParseValue(ref reader);
            var turn = stage.Require("Turn").GetInt32();
            var loc = stage.Require("Location").Deserialize<Loc>(options);
            object payload;
            if (stage.TryGetProperty("Actor", out var x)) payload = x.RequireRef<Actor>();
            else if (stage.TryGetProperty("Inventory", out x)) payload = x.RequireRef<Inventory>();
            else if (stage.TryGetProperty("Stack", out x)) payload = x.Deserialize<djack.RogueSurvivor.Data.Model.InvOrigin>(options);
            else if (stage.TryGetProperty("Corpses", out x)) payload = x.RequireRefs<Corpse>();
            else if (stage.TryGetProperty("MapObject", out x)) payload = x.RequireRef<MapObject>();
            else if (stage.TryGetProperty("Text", out x)) payload = x.GetString() ?? throw new JsonException("null percept text");
            else throw new JsonException("percept of nothing: " + stage.GetRawText());
            return new(payload as T ?? throw new JsonException("percept is not " + typeof(T).FullName), turn, in loc);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.AI.Percept_<T> src, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Turn", src.Turn);
            writer.WritePropertyName("Location");
            JsonSerializer.Serialize(writer, src.Location, options);
            switch (src.Percepted) {
            case Actor a: writer.WriteRef("Actor", a); break;
            case Inventory inv: writer.WriteRef("Inventory", inv); break;
            case djack.RogueSurvivor.Data.Model.InvOrigin stack:
                writer.WritePropertyName("Stack");
                JsonSerializer.Serialize(writer, stack, options);
                break;
            case List<Corpse> corpses: writer.WriteRefs("Corpses", corpses); break;
            case MapObject obj: writer.WriteRef("MapObject", obj); break;
            case string text: writer.WriteString("Text", text); break;
            default: throw new JsonException("percept of " + src.Percepted.GetType().FullName + " is not saved");
            }
            writer.WriteEndObject();
        }
    }
}
