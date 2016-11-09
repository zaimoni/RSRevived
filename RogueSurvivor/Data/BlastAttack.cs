// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.BlastAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal struct BlastAttack
  {
    public readonly int Radius;
    public readonly int[] Damage;
    public readonly bool CanDamageObjects;
    public readonly bool CanDestroyWalls;

    public BlastAttack(int radius, int[] damage, bool canDamageObjects, bool canDestroyWalls)
    {
      Contract.Requires(damage.Length == radius + 1);
      Contract.Requires(0<=radius);
      Radius = radius;
      Damage = damage;
      CanDamageObjects = canDamageObjects;
      CanDestroyWalls = canDestroyWalls;
    }
  }
}
