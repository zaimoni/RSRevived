// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.AI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.JSON;

using Rectangle = Zaimoni.Data.Box2D<short>;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
  [Serializable]
  public readonly struct AIScent
  {
    public readonly int Strength;

    public AIScent(int strength)
    {
      Strength = strength;
    }
  }

  [Serializable]
  internal class SmellSensor
  {
    private readonly Odor m_OdorToSmell;
    private readonly List<Percept_s<AIScent>> m_List = new(9);

    public List<Percept_s<AIScent>> Scents { get { return m_List; } }

    public SmellSensor(Odor odorToSmell)
    {
      m_OdorToSmell = odorToSmell;
    }

#region JSON save/load
    private SmellSensor(JsonElement src, JsonSerializerOptions options)
    {
      m_OdorToSmell = src.RequireEnum<Odor>("Odor");
      if (src.TryGetProperty("Scents", out var x)) foreach (var scent in x.EnumerateArray()) m_List.Add(scent.Deserialize<Percept_s<AIScent>>(options));
    }

    public static SmellSensor fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WriteString("Odor", m_OdorToSmell.ToString());
      if (0 < m_List.Count) writer.WriteArray("Scents", m_List, options);
      writer.WriteEndObject();
    }
#endregion

    public List<Percept_s<AIScent>> Sense(Actor actor)
    {
#if DEBUG
      if (OdorScent.MIN_STRENGTH > actor.SmellThreshold) throw new ArgumentOutOfRangeException(nameof(actor), OdorScent.MIN_STRENGTH.ToString()+" > actor.SmellThreshold");
#endif
      m_List.Clear();
      int num = actor.SmellThreshold;  // floors at 1
      Rectangle survey = new Rectangle(actor.Location.Position+Direction.NW, 3, 3);
      Map map = actor.Location.Map;
      int turnCounter = map.LocalTime.TurnCounter;
      survey.DoForEach(pt => {
        var scentAt = map.GetScentByOdorAt(m_OdorToSmell, in pt); // 0 is the no-scent value
        if (scentAt >= num) m_List.Add(new(new AIScent(scentAt), turnCounter, new Location(map, pt)));
      });
      return m_List;
    }
  }
}

namespace Zaimoni.JsonConvert
{
    using System.Text.Json;
    using Zaimoni.JSON;
    using AIScent = djack.RogueSurvivor.Gameplay.AI.Sensors.AIScent;

    // internal: the sensor is
    internal sealed class SmellSensor : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor>
    {
        public override djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options) => djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor.fromJson(JsonElement.ParseValue(ref reader), options);
        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor src, JsonSerializerOptions options) => src.toJson(writer, options);
    }

    public sealed class Scent : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.AI.Percept_s<AIScent>>
    {
        public override djack.RogueSurvivor.Engine.AI.Percept_s<AIScent> Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            var stage = JsonElement.ParseValue(ref reader);
            return new(new AIScent(stage.Require("Strength").GetInt32()), stage.Require("Turn").GetInt32(), stage.Require("Location").Deserialize<djack.RogueSurvivor.Data.Location>(options));
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.AI.Percept_s<AIScent> src, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Turn", src.Turn);
            writer.WritePropertyName("Location");
            JsonSerializer.Serialize(writer, src.Location, options);
            writer.WriteNumber("Strength", src.Percepted.Strength);
            writer.WriteEndObject();
        }
    }
}
