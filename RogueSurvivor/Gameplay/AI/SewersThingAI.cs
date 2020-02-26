﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SewersThingAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SewersThingAI : BaseAI
  {
    private const int LOS_MEMORY = 2*(WorldTime.TURNS_PER_HOUR);

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private readonly MemorizedSensor<LOSSensor> m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    private readonly SmellSensor m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);

    public SewersThingAI()
    {
    }

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      m_LivingSmellSensor.Sense(m_Actor);
      m_MasterSmellSensor.Sense(m_Actor);
      return m_MemLOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return m_MemLOSSensor.Sensor.FOV; } }
#nullable restore
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_MemLOSSensor.Sensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_MemLOSSensor.Sensor.enemies; } }
#nullable restore
    protected override void SensorsOwnedBy(Actor actor) { m_MemLOSSensor.Sensor.OwnedBy(actor); }

    protected override ActorAction SelectAction(RogueGame game)
    {
      ActorAction tmpAction;
      if (null != (_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors()))))) {
        tmpAction = TargetGridMelee(FilterCurrent(_enemies));
        if (null != tmpAction) return tmpAction;
        tmpAction = TargetGridMelee(FilterOld(_enemies));
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorTrackScent(m_LivingSmellSensor.Scents);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.TRACKING;
        return tmpAction;
      }
      return BehaviorWander();
    }
  }
}
