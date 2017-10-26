// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.DiceRoller
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class DiceRoller
  {
    private readonly Random m_Rng;

    public DiceRoller(int seed)
    {
      m_Rng = new Random(seed);
    }

    public DiceRoller()
      : this((int) DateTime.UtcNow.Ticks)
    {
    }

    public int Roll(int min, int max)
    {
      if (max <= min || 1 == max-min) return min;
      // should not need to defend aganst bugs in the C# library
      lock(m_Rng) { return m_Rng.Next(min, max); }
    }

#if DEAD_FUNC
    public float RollFloat()
    {
      lock(m_Rng) { return (float) m_Rng.NextDouble(); }
    }
#endif

    public bool RollChance(int chance)
    {
      if (100<=chance) return true;
      if (0>=chance) return false;
      return Roll(0, 100) < chance; // mathematical range 0, ..., 99 allows above specializations
    }
  }
}
