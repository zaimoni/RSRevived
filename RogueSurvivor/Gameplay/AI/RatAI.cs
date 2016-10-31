// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.RatAI
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

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class RatAI : BaseAI
  {
    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private LOSSensor m_LOSSensor;
    private SmellSensor m_LivingSmellSensor;

    public RatAI()
    {
      m_LOSSensor = new LOSSensor(VISION_SEES);
      m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    }

    public override List<Percept> UpdateSensors()
    {
      List<Percept> perceptList = m_LOSSensor.Sense(m_Actor);
      perceptList.AddRange(m_LivingSmellSensor.Sense(m_Actor));
      return perceptList;
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());
      List<Percept> percepts1 = FilterSameMap(percepts);
      List<Percept> enemies = FilterEnemies(percepts1);
      ActorAction tmpAction;
      Actor tmpActor;
      if (enemies != null) {
        List<Percept> current_enemies = FilterCurrent(enemies);
        if (current_enemies != null) {
          tmpAction = TargetGridMelee(current_enemies, out tmpActor);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = tmpActor;
            return tmpAction;
          }
        }
        List<Percept> perceptList2 = Filter(enemies, (Predicate<Percept>) (p => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter));
        if (perceptList2 != null) {
          tmpAction = TargetGridMelee(perceptList2, out tmpActor);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = tmpActor;
            return tmpAction;
          }
        }
      }
      ActorAction actorAction = BehaviorGoEatCorpse(FilterCorpses(percepts1));
      if (actorAction != null) {
        m_Actor.Activity = Activity.IDLE;
        return actorAction;
      }
      ActorAction actorAction3 = BehaviorTrackScent(game, m_LivingSmellSensor.Scents);
      if (actorAction3 != null) {
        m_Actor.Activity = Activity.TRACKING;
        return actorAction3;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
