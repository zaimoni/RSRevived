// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Corpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Runtime.Serialization;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  public sealed class Corpse : ILocation, ISerializable, IJsonStaged<Corpse>
  {
    public const int ZOMBIFY_DELAY = 6*WorldTime.TURNS_PER_HOUR;

    public readonly Actor DeadGuy;
    public readonly int Turn;
    public float HitPoints;
    public readonly int MaxHitPoints;
    public readonly float Rotation;
    public readonly float Scale; // currently not used properly

    public Actor? DraggedBy { get {
      var who = Location.Actor;
      if (null == who || who.IsDead) return null;
      return who.DraggedCorpse == this ? who : null;
    } }

    public bool IsDragged { get {
      var who = Location.Actor;
      if (null == who || who.IsDead) return false;
      return who.DraggedCorpse == this;
    } }

    public Corpse(Actor deadGuy, float rotation, float scale=1f)
    {
      DeadGuy = deadGuy;
      Turn = deadGuy.Location.Map.LocalTime.TurnCounter;
      HitPoints = (float)deadGuy.MaxHPs;
      MaxHitPoints = deadGuy.MaxHPs;
      Rotation = rotation;
      Scale = Math.Max(0.0f, Math.Min(1f, scale));
      DeadGuy.Location.Map.Add(this);
    }

#region Implement ISerializable
    protected Corpse(SerializationInfo info, StreamingContext context)
    {
      info.read(ref DeadGuy, "DeadGuy");
      Turn = info.GetInt32("Turn");
      MaxHitPoints = info.GetInt32("MaxHitPoints");
      HitPoints = info.GetSingle("HitPoints");
      Rotation = info.GetSingle("Rotation");
      Scale = info.GetSingle("Scale");
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("DeadGuy", DeadGuy);
      info.AddValue("Turn", Turn);
      info.AddValue("MaxHitPoints", MaxHitPoints);
      info.AddValue("HitPoints", HitPoints);
      info.AddValue("Rotation", Rotation);
      info.AddValue("Scale", Scale);
     }
#endregion

#region JSON save/load
    // We own our dead actor: it is saved in full here, and nowhere else.  Our location is its location.
    // Unlike the public constructor, this does not add us to our map: the map that owns us does that.
    private Corpse(JsonElement src, JsonSerializerOptions options)
    {
      src.RecordRef(this);
      DeadGuy = src.Require("DeadGuy").Deserialize<Actor>(options) ?? throw new JsonException("null DeadGuy");
      Turn = src.Require("Turn").GetInt32();
      HitPoints = src.Require("HitPoints").GetSingle();
      MaxHitPoints = src.TryGetProperty("MaxHitPoints", out var relay) ? relay.GetInt32() : DeadGuy.MaxHPs;
      Rotation = src.Require("Rotation").GetSingle();
      Scale = src.TryGetProperty("Scale", out relay) ? relay.GetSingle() : 1f;
    }

    static Corpse IJsonStaged<Corpse>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    // implied when omitted: MaxHitPoints equal to what our dead actor's would be now, a Scale of 1
    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WritePropertyName("DeadGuy");
      JsonSerializer.Serialize(writer, DeadGuy, options);
      writer.WriteNumber("Turn", Turn);
      writer.WriteNumber("HitPoints", HitPoints);
      if (DeadGuy.MaxHPs != MaxHitPoints) writer.WriteNumber("MaxHitPoints", MaxHitPoints);
      writer.WriteNumber("Rotation", Rotation);
      if (1f != Scale) writer.WriteNumber("Scale", Scale);
    }
#endregion

    public Location Location {
        get { return DeadGuy.Location; }
        set {
          Point? same_map = default;
          if (DeadGuy.Location.Map == value.Map) same_map = DeadGuy.Location.Position;
          if (null == same_map) DeadGuy.Location.Map.Remove(this);
          DeadGuy.Location = value;
          if (null == same_map) DeadGuy.Location.Map.Add(this);
          else DeadGuy.Location.Map.UpdateCache(this, same_map.Value);
        }
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
