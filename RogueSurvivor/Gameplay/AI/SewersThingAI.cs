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

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SewersThingAI : BaseAI
  {
    private const int LOS_MEMORY = 2*(WorldTime.TURNS_PER_HOUR);

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private readonly MemorizedSensor m_LOSSensor;
    private readonly SmellSensor m_LivingSmellSensor;
    private readonly SmellSensor m_MasterSmellSensor;

    public SewersThingAI()
    {
      m_LOSSensor = new MemorizedSensor((Sensor) new LOSSensor(VISION_SEES), LOS_MEMORY);
      m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
      m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);
    }

    public override void OptimizeBeforeSaving()
    {
      m_LOSSensor.Forget(m_Actor);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      List<Percept> perceptList = m_LOSSensor.Sense(game, m_Actor);
      perceptList.AddRange((IEnumerable<Percept>)m_LivingSmellSensor.Sense(game, m_Actor));
      perceptList.AddRange((IEnumerable<Percept>)m_MasterSmellSensor.Sense(game, m_Actor));
      return perceptList;
    }

    public override HashSet<Point> FOV { get { return (m_LOSSensor.Sensor as LOSSensor).FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);
      List<Percept> enemies = FilterEnemies(game, percepts1);
      ActorAction tmpAction;
      Actor tmpActor;
      if (enemies != null) {
        List<Percept> current_enemies = FilterCurrent(enemies);
        if (current_enemies != null) {
          tmpAction = TargetGridMelee(game, current_enemies, out tmpActor);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = tmpActor;
            return tmpAction;
          }
        }
        List<Percept> perceptList2 = Filter(game, enemies, (Predicate<Percept>) (p => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter));
        if (perceptList2 != null) {
          tmpAction = TargetGridMelee(game, perceptList2, out tmpActor);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = tmpActor;
            return tmpAction;
          }
        }
      }
      ActorAction actorAction = BehaviorTrackScent(game, m_LivingSmellSensor.Scents);
      if (actorAction != null) {
        m_Actor.Activity = Activity.TRACKING;
        return actorAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
