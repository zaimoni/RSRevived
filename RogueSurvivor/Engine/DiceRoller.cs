// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.DiceRoller
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Linq;

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

    public T ChooseWithoutReplacement<T>(List<T> src) {
      int n = (src?.Count ?? 0);
      if (0 >= n) throw new ArgumentNullException(nameof(src));
      int k = Roll(0, n);
      T ret = src[k];
      src.RemoveAt(k);
      return ret;
    }

    public T Choose<T>(List<T> src) {
      int n = (src?.Count ?? 0);
      if (0 >= n) throw new ArgumentNullException(nameof(src));
      return src[Roll(0, n)];
    }

    public T Choose<T>(T[] src) {
      int n = (src?.Length ?? 0);
      if (0 >= n) throw new ArgumentNullException(nameof(src));
      return src[Roll(0, n)];
    }

    public T Choose<T>(IEnumerable<T> src) {
      int n = (src?.Count() ?? 0);
      if (0 >= n) throw new ArgumentNullException(nameof(src));
      n = Roll(0, n);
      foreach(var x in src) if (0 >= n--) return x;
      throw new ArgumentNullException(nameof(src)); // unreachable with a sufficiently correct compiler
    }
  }
}
