// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.Percept
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
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
}
