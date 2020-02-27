// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.Percept
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace djack.RogueSurvivor.Engine.AI
{
  [Serializable]
  internal class WhereWhen
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
  }

  [Serializable]
  internal class Percept_<_T_> : WhereWhen
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

    internal static List<_T_>? FilterCurrent<_T_>(this IEnumerable<_T_> percepts, int turn) where _T_:WhereWhen
    {
      if (!percepts.Any()) return null;
      List<_T_>? ret = null;
      foreach(var p in percepts) if (turn == p.Turn) (ret ?? (ret = new List<_T_>())).Add(p);
      return ret;
    }

    internal static List<_T_>? FilterOld<_T_>(this IEnumerable<_T_> percepts, int turn) where _T_:WhereWhen
    {
      if (!percepts.Any()) return null;
      List<_T_>? ret = null;
      foreach(var p in percepts) if (turn > p.Turn) (ret ?? (ret = new List<_T_>())).Add(p);
      return ret;
    }

	internal static List<Percept_<object>>? FilterT<_T_>(this IEnumerable<Percept_<object>>? percepts) where _T_:class
	{
      if (!percepts.Any()) return null;
      List<Percept_<object>>? ret = null;
      foreach(var p in percepts) if (p.Percepted is _T_) (ret ?? (ret = new List<Percept_<object>>())).Add(p);
      return ret;
	}

	internal static List<Percept_<object>>? FilterT<_T_>(this IEnumerable<Percept_<object>>? percepts, Predicate<_T_> fn) where _T_:class
	{
      if (null == percepts || !percepts.Any()) return null;
      List<Percept_<object>>? ret = null;
      foreach(var p in percepts) if (p.Percepted is _T_ test && fn(test)) (ret ?? (ret = new List<Percept_<object>>())).Add(p);
      return ret;
	}

    // for completeness
	internal static List<Percept_<_T_>>? FilterCast<_T_>(this IEnumerable<Percept_<object>> percepts, Predicate<_T_> fn) where _T_:class
	{
      if (!percepts.Any()) return null;
      List<Percept_<_T_>>? ret = null;
      foreach(var p in percepts) {
        // XXX arguably should be tmp.Count() but unclear how CPU vs. GC thrashing works here
        if (p.Percepted is _T_ test && fn(test)) (ret ?? (ret = new List<Percept_<_T_>>())).Add(new Percept_<_T_>(test, p.Turn, p.Location));
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
