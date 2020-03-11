// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Attack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal enum AttackKind
  {
    PHYSICAL,
    FIREARM,
    BOW,
  }

  [Serializable]
  internal struct Attack
  {
    [NonSerialized]
    public static readonly Attack BLANK = new Attack(AttackKind.PHYSICAL, new Verb("<blank>"), 0, 0, 0, 0);

    public readonly AttackKind Kind;
    public readonly Verb Verb;
    public readonly int DisarmChance;
    public readonly int HitValue;
    public readonly int Hit2Value;  // alpha10: secondary to-hit e.g. rapid fire 1st shot
    public readonly int Hit3Value;  // alpha10: tertiary to-hit e.g. rapid fire 2nd shot
    public readonly int DamageValue;
    public readonly int StaminaPenalty;
    public readonly short Range;

    public int EfficientRange { get { return Range / 2; } }

    public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue, int staminaPenalty = 0, short range = 0, int rapid1 = 0, int rapid2 = 0)
    {
#if DEBUG
      if (null == verb) throw new ArgumentNullException(nameof(verb));
#endif
      Kind = kind;
      Verb = verb;
      HitValue = hitValue;
      Hit2Value = rapid1;
      Hit3Value = rapid2;
      DamageValue = damageValue;
      StaminaPenalty = staminaPenalty;
      Range = range;
      DisarmChance = 0; // mockup.
    }

    public int Rating { 
      get {
        return 100000 * DamageValue + 1000 * HitValue + DisarmChance + -StaminaPenalty;
      }
    }
  }
}
