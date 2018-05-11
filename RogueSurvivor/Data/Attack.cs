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

    public readonly AttackKind Kind;
    public readonly Verb Verb;
    public readonly int DisarmChance;
    public readonly int HitValue;
    public readonly int DamageValue;
    public readonly int StaminaPenalty;
    public readonly int Range;

    public int EfficientRange { get { return Range / 2; } }

    // alpha10
    /// <summary>
    /// Secondary hit roll value.
    /// Eg: rapid fire 1st shot
    /// </summary>
    public int Hit2Value { get { return (int)(HitValue* Engine.Rules.RAPID_FIRE_FIRST_SHOT_ACCURACY); } }

    /// <summary>
    /// Tertiary hit roll value.
    /// Eg: rapid fire 2nd shot
    /// </summary>
    public int Hit3Value { get { return (int)(HitValue* Engine.Rules.RAPID_FIRE_SECOND_SHOT_ACCURACY); } }

    public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue, int staminaPenalty = 0, int range = 0)
    {
#if DEBUG
      if (null == verb) throw new ArgumentNullException(nameof(verb));
#endif
      Kind = kind;
      Verb = verb;
      HitValue = hitValue;
      DamageValue = damageValue;
      StaminaPenalty = staminaPenalty;
      Range = range;
      DisarmChance = 0; // mockup.
    }

    public int Rating { 
      get {
        return 10000 * DamageValue + 100 * HitValue + -StaminaPenalty;
      }
    }
  }
}
