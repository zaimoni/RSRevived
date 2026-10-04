// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.MemorizedSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Engine.AI
{
  [Serializable]
  internal class MemorizedSensor<T> : Sensor where T:Sensor
  {
#nullable enable
    private List<Percept> m_Percepts = new List<Percept>();
    private readonly T m_Sensor;
    private readonly int m_Persistance;

    public T Sensor { get { return m_Sensor; } }

    public MemorizedSensor(T noMemorySensor, int persistance)
    {
#if DEBUG
      if (null == noMemorySensor) throw new ArgumentNullException(nameof(noMemorySensor));
#endif
      m_Sensor = noMemorySensor;
      m_Persistance = persistance;
    }

    // memorized sensor, etc. would need a rethinking if it was possible to debrief the recently revived
    // (this is data-live for corpses through their controller)
    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      Forget();
    }

#region JSON save/load
    private MemorizedSensor(JsonElement src, JsonSerializerOptions options)
    {
      m_Persistance = src.Require("Persistance").GetInt32();
      m_Sensor = src.Require("Sensor").Deserialize<T>(options) ?? throw new JsonException("null Sensor");
      if (src.TryGetProperty("Memory", out var x)) m_Percepts = x.Deserialize<List<Percept>>(options) ?? throw new JsonException("null Memory");
    }

    public static MemorizedSensor<T> fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      OptimizeBeforeSaving(default);    // as the legacy save does: forget what is stale
      writer.WriteStartObject();
      writer.WriteNumber("Persistance", m_Persistance);
      writer.WritePropertyName("Sensor");
      JsonSerializer.Serialize(writer, m_Sensor, options);
      if (0 < m_Percepts.Count) writer.WriteArray("Memory", m_Percepts, options);
      writer.WriteEndObject();
    }
#endregion

    public void Clear() { m_Percepts.Clear(); }
#nullable restore

    private void Forget()
    {
      var actor = Viewpoint;
      if (null == actor) return;
      if (null == actor.Location.Map) return; // Duckman superheroically crashes this, otherwise
      // memorized sensor is only used for vision
      var FOVloc = actor.Controller.FOVloc;
      var tmp = new List<Percept>(m_Percepts.Count);
      foreach(var p in m_Percepts) {
        if (p.GetAge(actor.Location.Map.LocalTime.TurnCounter)>m_Persistance) continue;
        if (p.Percepted is Actor a) {
          if (a.IsDead) continue;
//        if (a.Location.Map != actor.Location.Map) continue;   // XXX valid for RS Alpha 6, invalid for RS Revived; want to verify other changes first
        } else {    // actors need a different test than the following
          if (0 <= Array.IndexOf(FOVloc, p.Location)) continue;
        }
        if (p.Percepted is Inventory inv && inv.IsEmpty) continue;
        if (p.Percepted is List<Corpse> corpses && 0>=corpses.Count) continue;

        tmp.Add(p);
      }
      m_Percepts = tmp;
    }

#nullable enable
    public Actor Viewpoint { get { return m_Sensor.Viewpoint; } }

    public List<Percept> Sense()
    {
      Forget();
      var tmp = m_Sensor.Sense();   // time is m_Actor.Location.Map.LocalTime.TurnCounter
      int ub = tmp.Count;
      if (0 >= tmp.Count) return m_Percepts;
      while(0 <= --ub) {
        var percept = tmp[ub];
        foreach (Percept mPercept in m_Percepts) {
          if (mPercept.Percepted == percept.Percepted) {
            mPercept.Location = percept.Location;
            mPercept.Turn = percept.Turn;
            tmp.RemoveAt(ub);
            break;
          }
        }
      }
      if (0 >= tmp.Count) return m_Percepts;

      // huge automatic multipler isn't a good idea here
      int newSize = m_Percepts.Count + tmp.Count;
      if (newSize > m_Percepts.Capacity) m_Percepts.Capacity = newSize;
      m_Percepts.AddRange(tmp);
      return m_Percepts;
    }
#nullable restore
  }
}

namespace Zaimoni.JsonConvert
{
    using System.Text.Json;

    // internal: the sensors are
    internal sealed class MemorizedSensor<T> : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.AI.MemorizedSensor<T>> where T : djack.RogueSurvivor.Engine.AI.Sensor
    {
        public override djack.RogueSurvivor.Engine.AI.MemorizedSensor<T> Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options) => djack.RogueSurvivor.Engine.AI.MemorizedSensor<T>.fromJson(JsonElement.ParseValue(ref reader), options);
        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.AI.MemorizedSensor<T> src, JsonSerializerOptions options) => src.toJson(writer, options);
    }
}
