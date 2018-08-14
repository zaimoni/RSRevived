// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TimedTask
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class TimedTask
  {
    public int TurnsLeft { get; private set; }

    public bool IsCompleted { get { return TurnsLeft <= 0; } }

    protected TimedTask(int turnsLeft)
    {
      TurnsLeft = turnsLeft;
    }

    public void Tick(Map m)
    {
      if (--TurnsLeft > 0) return;
      Trigger(m);
    }

    public abstract void Trigger(Map m);
  }
}
