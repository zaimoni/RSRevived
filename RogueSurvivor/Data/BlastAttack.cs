// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.BlastAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal struct BlastAttack
  {
    public int Radius { get; private set; }

    public int[] Damage { get; private set; }

    public bool CanDamageObjects { get; private set; }

    public bool CanDestroyWalls { get; private set; }

    public BlastAttack(int radius, int[] damage, bool canDamageObjects, bool canDestroyWalls)
    {
      this = new BlastAttack();
      if (damage.Length != radius + 1)
        throw new ArgumentException("damage.Length != radius + 1");
      this.Radius = radius;
      this.Damage = damage;
      this.CanDamageObjects = canDamageObjects;
      this.CanDestroyWalls = canDestroyWalls;
    }
  }
}
