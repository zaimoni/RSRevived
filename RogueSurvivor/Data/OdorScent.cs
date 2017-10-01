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

    public readonly Point Position;
    private short m_Strength;
    public readonly Odor Odor;

    public int Strength {
      get {
        return m_Strength;
      }
      set {
        if (1 > value) m_Strength = 0;
        else if (MAX_STRENGTH < value) m_Strength = MAX_STRENGTH;
        else m_Strength = (short)value;
      }
    }

    public OdorScent(Odor odor, int strength, Point position)
    {
      Odor = odor;
      Strength = strength;
      Position = position;
    }
  }
}
