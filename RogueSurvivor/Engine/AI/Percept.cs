// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.Percept
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.AI
{
  [Serializable]
  internal class WhereWhen
  {
    // MemorizedSensor needs public setters for these
    public int Turn { get; set; }
    public Location Location { get; set; }

    public WhereWhen(Location loc, int t0)
    {
      Contract.Requires(0 <= t0);
      Contract.Requires(null != loc.Map);
      Turn = t0;
      Location = loc;
    }

    public int GetAge(int t1)
    {
      return Math.Max(0, t1 - Turn);
    }
  }

  [Serializable]
  internal class Percept_<_T_> : WhereWhen where _T_:class
  {
    private _T_ m_Percepted;

    public _T_ Percepted {
      get {
        return m_Percepted;
      }
    }

    public Percept_(_T_ percepted, int turn, Location location)
     : base(location,turn)
    {
      Contract.Requires(null != percepted);
      m_Percepted = percepted;
    }
  }

  internal static class ext_Percept
  {
    internal static List<Percept_<_T_>> Filter<_T_>(this IEnumerable<Percept_<_T_>> percepts, Predicate<Percept_<_T_>> predicateFn) where _T_:class
    {
      if (null == percepts || 0 == percepts.Count()) return null;
      IEnumerable<Percept_<_T_>> tmp = percepts.Where(p=> predicateFn(p));
      return tmp.Any() ? tmp.ToList() : null;
    }
	internal static List<Percept_<object>> FilterT<_T_>(this IEnumerable<Percept_<object>> percepts, Predicate<_T_> fn=null) where _T_:class
	{
      if (null == percepts || 0 == percepts.Count()) return null;
      IEnumerable<Percept_<object>> tmp = percepts.Where(p=>p.Percepted is _T_);
	  if (null != fn) tmp = tmp.Where(p=>fn(p.Percepted as _T_));
	  return (tmp.Any() ? tmp.ToList() : null);
	}

        // for completeness
#if FAIL
	internal static List<Percept_<_T_>> FilterCast<_T_>(this IEnumerable<Percept_<object>> percepts, Predicate<_T_> fn=null) where _T_:class
	{
      if (null == percepts || 0 == percepts.Count()) return null;
      IEnumerable<Percept_<object>> tmp = percepts.Where(p=>p.Percepted is _T_);
	  if (null != fn) tmp = tmp.Where(p=>fn(p.Percepted as _T_));
	  if (!tmp.Any()) return null;
	  List<Percept_<_T_>> ret = new List<Percept_<_T_>>();
	  foreach(Percept_<object> p in tmp) {
	    ret.Add(new Percept_<_T_>(p.Percepted as _T_, p.Turn, p.Location));
	  }
	  return ret;
	}
#endif

    internal static Percept_<_T_> FilterFirst<_T_>(this IEnumerable<Percept_<_T_>> percepts, Predicate<Percept_<_T_>> predicateFn) where _T_:class
    {
      if (null == percepts || 0 == percepts.Count()) return null;
      foreach (Percept_<_T_> percept in percepts) {
        if (predicateFn(percept)) return percept;
      }
      return null;
    }

    internal static List<Percept_<_T_>> FilterOut<_T_>(this IEnumerable<Percept_<_T_>> percepts, Predicate<Percept_<_T_>> rejectPredicateFn) where _T_:class
    {
      return percepts.Filter(p => !rejectPredicateFn(p));
    }
  }
}
