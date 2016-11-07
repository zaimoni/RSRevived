// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Exit
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Exit
  {
    private Location m_Location;	// XXX this cannot be public readonly Location: load fails in the runtime library Nov 5 2016.  Retry after compiler upgrade.
    public readonly bool IsAnAIExit;

    public Map ToMap { get { return m_Location.Map; } }
    public Location Location { get { return m_Location; } }

    public Exit(Map toMap, Point toPosition, bool AIexit=false)
    {
	  Contract.Requires(null!=toMap);
      m_Location = new Location(toMap,toPosition);
	  IsAnAIExit = AIexit;
    }
  }
}
