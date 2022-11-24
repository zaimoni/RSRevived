// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SewersThingAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SewersThingAI : BaseAI
  {
    private const int LOS_MEMORY = 2*(WorldTime.TURNS_PER_HOUR);

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private readonly MemorizedSensor<LOSSensor> m_MemLOSSensor;
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    private readonly SmellSensor m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);

    public SewersThingAI(Actor src) : base(src)
    {
      m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES, src), LOS_MEMORY);
    }

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      m_MemLOSSensor.Forget();
    }

    public override List<Percept> UpdateSensors()
    {
      m_LivingSmellSensor.Sense(m_Actor);
      m_MasterSmellSensor.Sense(m_Actor);
      return m_MemLOSSensor.Sense();
    }

    public override HashSet<Point> FOV { get { return m_MemLOSSensor.Sensor.FOV; } }
    public override Location[] FOVloc { get { return m_MemLOSSensor.Sensor.FOVloc; } }
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_MemLOSSensor.Sensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_MemLOSSensor.Sensor.enemies; } }

    protected override ActorAction? SelectAction()
    {
      ActorAction? tmpAction;
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
