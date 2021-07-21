// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Defence
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal struct Defence
  {
    [NonSerialized]
    public static readonly Defence BLANK = new Defence(0, 0, 0);

    // \todo savefile break: convert to readonly fields
    public int Value { get; private set; }
    public int Protection_Hit { get; private set; }
    public int Protection_Shot { get; private set; }

    public Defence(int value, int protection_hit, int protection_shot)
    {
      Value = value;
      Protection_Hit = protection_hit;
      Protection_Shot = protection_shot;
    }

    public static Defence operator +(Defence lhs, Defence rhs)
    {
      return new Defence(lhs.Value + rhs.Value, lhs.Protection_Hit + rhs.Protection_Hit, lhs.Protection_Shot + rhs.Protection_Shot);
    }

    public static Defence operator -(Defence lhs, Defence rhs)
    {
      return new Defence(lhs.Value - rhs.Value, lhs.Protection_Hit - rhs.Protection_Hit, lhs.Protection_Shot - rhs.Protection_Shot);
    }
  }
}
