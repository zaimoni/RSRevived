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

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class RatAI : BaseAI
  {
    private LOSSensor m_LOSSensor;
    private SmellSensor m_LivingSmellSensor;

    protected override void CreateSensors()
    {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES);
            m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      List<Percept> perceptList = m_LOSSensor.Sense(game, m_Actor);
      perceptList.AddRange((IEnumerable<Percept>)m_LivingSmellSensor.Sense(game, m_Actor));
      return perceptList;
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);
      List<Percept> percepts2 = FilterEnemies(game, percepts1);
      if (percepts2 != null)
      {
        List<Percept> perceptList1 = FilterCurrent(percepts2);
        if (perceptList1 != null)
        {
          Percept percept1;
          ActorAction actorAction1 = TargetGridMelee(game, perceptList1, out percept1);
          if (actorAction1 != null)
          {
                        m_Actor.Activity = Activity.CHASING;
                        m_Actor.TargetActor = percept1.Percepted as Actor;
            return actorAction1;
          }
        }
        List<Percept> perceptList2 = Filter(game, percepts2, (Predicate<Percept>) (p => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter));
        if (perceptList2 != null)
        {
          Percept percept1;
          ActorAction actorAction1 = TargetGridMelee(game, perceptList2, out percept1);
          if (actorAction1 != null)
          {
                        m_Actor.Activity = Activity.CHASING;
                        m_Actor.TargetActor = percept1.Percepted as Actor;
            return actorAction1;
          }
        }
      }
      List<Percept> corpsesPercepts = FilterCorpses(game, percepts1);
      if (corpsesPercepts != null)
      {
        ActorAction actorAction = BehaviorGoEatCorpse(game, corpsesPercepts);
        if (actorAction != null)
        {
                    m_Actor.Activity = Activity.IDLE;
          return actorAction;
        }
      }
      ActorAction actorAction3 = BehaviorTrackScent(game, m_LivingSmellSensor.Scents);
      if (actorAction3 != null)
      {
                m_Actor.Activity = Activity.TRACKING;
        return actorAction3;
      }
            m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
