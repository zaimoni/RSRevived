// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SkeletonAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SkeletonAI : BaseAI, IJsonStaged<SkeletonAI>
  {
    // JSON: saves $type only so far (the AI reloads with amnesia)
    static SkeletonAI IJsonStaged<SkeletonAI>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new SkeletonAI(JsonOwner));

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private LOSSensor m_LOSSensor;

    public SkeletonAI(Actor src) : base(src)
    {
      m_LOSSensor = new LOSSensor(VISION_SEES, src);
    }

    #region JSON save/load
    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      base.toJsonFields(writer, options);
      writer.WritePropertyName("LOS");
      JsonSerializer.Serialize(writer, m_LOSSensor, options);
    }

    protected override void loadJsonFields(JsonElement src, JsonSerializerOptions options)
    {
      base.loadJsonFields(src, options);
      JsonElement x;
      m_LOSSensor = src.Require("LOS").Deserialize<LOSSensor>(options) ?? throw new JsonException("null LOS");
    }
    #endregion

    public override List<Percept> UpdateSensors()
    {
      return m_LOSSensor.Sense();
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    public override Location[] FOVloc { get { return m_LOSSensor.FOVloc; } }
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_LOSSensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_LOSSensor.enemies; } }

    protected override ActorAction? SelectAction()
    {
      const int IDLE_CHANCE = 80;

      var tmpAction = TargetGridMelee(_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors()))));
      if (null != tmpAction) return tmpAction;

      if (Rules.Get.RollChance(IDLE_CHANCE)) return new ActionWait(m_Actor);
      return BehaviorWander();
    }
  }
}
