// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Exit
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Exit
  {
    Location m_Location;

    public Map ToMap { get { return m_Location.Map; } }
    public Location Location { get { return m_Location; } }

    public bool IsAnAIExit { get; set; }

    public Exit(Map toMap, Point toPosition)
    {
      m_Location = new Location(toMap,toPosition);
    }
  }
}
