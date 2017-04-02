// Decompiled with JetBrains decompiler
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
using System.Drawing;
using System.Diagnostics.Contracts;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SewersThingAI : BaseAI
  {
    private const int LOS_MEMORY = 2*(WorldTime.TURNS_PER_HOUR);

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private readonly MemorizedSensor m_LOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    private readonly SmellSensor m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);

    public SewersThingAI()
    {
    }

    public override void OptimizeBeforeSaving()
    {
      m_LOSSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      List<Percept> perceptList = m_LOSSensor.Sense(m_Actor);
      perceptList.AddRange(m_LivingSmellSensor.Sense(m_Actor));
      perceptList.AddRange(m_MasterSmellSensor.Sense(m_Actor));
      return perceptList;
    }

    public override HashSet<Point> FOV { get { return (m_LOSSensor.Sensor as LOSSensor).FOV; } }
    protected override void SensorsOwnedBy(Actor actor) { (m_LOSSensor.Sensor as LOSSensor).OwnedBy(actor); }

    protected override ActorAction SelectAction(RogueGame game)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());
      List<Percept> percepts1 = FilterSameMap(UpdateSensors());
      List<Percept> enemies = SortByGridDistance(FilterEnemies(percepts1));
      ActorAction tmpAction;
      if (enemies != null) {
        tmpAction = TargetGridMelee(FilterCurrent(enemies));
        if (null != tmpAction) return tmpAction;
        tmpAction = TargetGridMelee(FilterOld(enemies));
        if (null != tmpAction) return tmpAction;
      }
      ActorAction actorAction = BehaviorTrackScent(m_LivingSmellSensor.Scents);
      if (actorAction != null) {
        m_Actor.Activity = Activity.TRACKING;
        return actorAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
