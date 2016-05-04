// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.OdorScent
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class OdorScent
  {
    public const int MIN_STRENGTH = 1;
    public const int MAX_STRENGTH = 9*WorldTime.TURNS_PER_HOUR;

    public Odor Odor { get; private set; }

    public int Strength { get; private set; }

    public Point Position { get; private set; }

    public OdorScent(Odor odor, int strength, Point position)
    {
      Odor = odor;
      Strength = Math.Min(MAX_STRENGTH, strength);
      Position = position;
    }

    public void Change(int amount)
    {
      int num = Strength + amount;
      if (num < 1) num = 0;
      else if (num > MAX_STRENGTH) num = MAX_STRENGTH;
      Strength = num;
    }

    public void Set(int value)
    {
      int num = value;
      if (num < 1) num = 0;
      else if (num > MAX_STRENGTH) num = MAX_STRENGTH;
      Strength = num;
    }
  }
}
