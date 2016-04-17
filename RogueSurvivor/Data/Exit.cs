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
    public Map ToMap { get; set; }

    public Point ToPosition { get; set; }

    public bool IsAnAIExit { get; set; }

    public Exit(Map toMap, Point toPosition)
    {
      this.ToMap = toMap;
      this.ToPosition = toPosition;
    }
  }
}
