// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.BlastAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  public readonly struct BlastAttack
  {
    public readonly int Radius;
    public readonly int[] Damage;
    public readonly bool CanDamageObjects;
    public readonly bool CanDestroyWalls;   // XXX not implemented, so hard-errors if true

    public BlastAttack(int radius, int[] damage, bool canDamageObjects, bool canDestroyWalls)
    {
#if DEBUG
      if (0>radius) throw new ArgumentOutOfRangeException(nameof(radius), radius, "0>radius");
      if (damage.Length != radius+1) throw new ArgumentOutOfRangeException(nameof(damage), damage.Length, "damage.Length != radius+1");
#endif
      Radius = radius;
      Damage = damage;
      CanDamageObjects = canDamageObjects;
      CanDestroyWalls = canDestroyWalls;
    }

    public readonly int DamageAt(int distance)
    {
#if DEBUG
      if (distance < 0 || distance > Radius) throw new ArgumentOutOfRangeException(nameof(distance), distance, "out of range");
#endif
      return Damage[distance];
    }
  }
}
