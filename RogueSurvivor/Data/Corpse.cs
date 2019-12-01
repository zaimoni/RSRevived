// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Corpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Corpse
  {
    public const int ZOMBIFY_DELAY = 6*WorldTime.TURNS_PER_HOUR;

    public readonly Actor DeadGuy;
    public readonly int Turn;
    public Point Position;
    public float HitPoints;
    public readonly int MaxHitPoints;
    public readonly float Rotation;
    public readonly float Scale; // currently not used properly
    public Actor? DraggedBy;

    public bool IsDragged { get { return !DraggedBy?.IsDead ?? false; } }

    public Corpse(Actor deadGuy, float rotation, float scale=1f)
    {
      DeadGuy = deadGuy;
      Turn = deadGuy.Location.Map.LocalTime.TurnCounter;
      HitPoints = (float)deadGuy.MaxHPs;
      MaxHitPoints = deadGuy.MaxHPs;
      Rotation = rotation;
      Scale = Math.Max(0.0f, Math.Min(1f, scale));
    }

    public int FreshnessPercent {
      get {
        return (int) (100.0 * (double) HitPoints / (double)DeadGuy.MaxHPs);
      }
    }

    public int ZombifyChance(WorldTime timeNow, bool checkDelay = true) // hypothetical AI would want the explicit parameter
    {
      const float CORPSE_ZOMBIFY_NIGHT_FACTOR = 2f;
      const float CORPSE_ZOMBIFY_DAY_FACTOR = 0.01f;
      const float CORPSE_ZOMBIFY_INFECTIONP_FACTOR = 1f;
      const int CORPSE_ZOMBIFY_BASE_CHANCE = 0;

      int num1 = timeNow.TurnCounter - Turn;
      if (checkDelay && num1 < ZOMBIFY_DELAY) return 0;
      int num2 = DeadGuy.InfectionPercent;
      if (checkDelay) {
        int num3 = num2 >= 100 ? 1 : 100 / (1 + num2);
        if (timeNow.TurnCounter % num3 != 0)
          return 0;
      }
      float num4 = CORPSE_ZOMBIFY_BASE_CHANCE + CORPSE_ZOMBIFY_INFECTIONP_FACTOR * (float) num2 - (float) (int) ((double) num1 / (double) WorldTime.TURNS_PER_DAY);
      return Math.Max(0, Math.Min(100, (int)(num4 * (timeNow.IsNight ? CORPSE_ZOMBIFY_NIGHT_FACTOR : CORPSE_ZOMBIFY_DAY_FACTOR))));
    }

    public static float DecayPerTurn()
    {
      const float CORPSE_DECAY_PER_TURN = 0.005555556f;   // 1/180 per turn
      return CORPSE_DECAY_PER_TURN;
    }

    // returns true if and only if destroyed
    public bool TakeDamage(float dmg) { return 0.0 >= (HitPoints -= dmg); }

    public int TransmitInfection { get { return DeadGuy.Infection/10; } }   // due to eating

    public int RotLevel {
      get {
        int num = FreshnessPercent;
        if (num < 5) return 5;
        if (num < 25) return 4;
        if (num < 50) return 3;
        if (num < 75) return 2;
        return num < 90 ? 1 : 0;
      }
    }

    public override string ToString()
    {
      return "corpse of "+DeadGuy.Name;
    }
  }
}
