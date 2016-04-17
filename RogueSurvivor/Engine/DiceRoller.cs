// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.DiceRoller
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class DiceRoller
  {
    private Random m_Rng;

    public DiceRoller(int seed)
    {
      this.m_Rng = new Random(seed);
    }

    public DiceRoller()
      : this((int) DateTime.UtcNow.Ticks)
    {
    }

    public int Roll(int min, int max)
    {
      if (max <= min)
        return min;
      int num;
      lock (this.m_Rng)
        num = this.m_Rng.Next(min, max);
      if (num >= max)
        num = max - 1;
      return num;
    }

    public float RollFloat()
    {
      lock (this.m_Rng)
        return (float) this.m_Rng.NextDouble();
    }

    public bool RollChance(int chance)
    {
      return this.Roll(0, 100) < chance;
    }
  }
}
