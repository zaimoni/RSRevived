// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Attack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal struct Attack
  {
    [NonSerialized]
    public static readonly Attack BLANK = new Attack(AttackKind.PHYSICAL, new Verb("<blank>"), 0, 0, 0, 0);

    public AttackKind Kind { get; private set; }
    public Verb Verb { get; private set; }
    public int HitValue { get; private set; }
    public int DamageValue { get; private set; }
    public int StaminaPenalty { get; private set; }
    public int Range { get; private set; }

    public int EfficientRange
    {
      get
      {
        return this.Range / 2;
      }
    }

    public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue, int staminaPenalty = 0, int range = 0)
    {
      if (verb == null) throw new ArgumentNullException("verb");
      Kind = kind;
      Verb = verb;
      HitValue = hitValue;
      DamageValue = damageValue;
      StaminaPenalty = staminaPenalty;
      Range = range;
    }
  }
}
