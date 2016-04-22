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
    private MemorizedSensor m_LOSSensor;
    private SmellSensor m_LivingSmellSensor;
    private SmellSensor m_MasterSmellSensor;

    protected override void CreateSensors()
    {
      this.m_LOSSensor = new MemorizedSensor((Sensor) new LOSSensor(LOSSensor.SensingFilter.ACTORS), LOS_MEMORY);
      this.m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
      this.m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      List<Percept> perceptList = this.m_LOSSensor.Sense(game, this.m_Actor);
      perceptList.AddRange((IEnumerable<Percept>) this.m_LivingSmellSensor.Sense(game, this.m_Actor));
      perceptList.AddRange((IEnumerable<Percept>) this.m_MasterSmellSensor.Sense(game, this.m_Actor));
      return perceptList;
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterSameMap(game, percepts);
      List<Percept> percepts2 = this.FilterEnemies(game, percepts1);
      if (percepts2 != null)
      {
        List<Percept> perceptList1 = this.Filter(game, percepts2, (Predicate<Percept>) (p => p.Turn == this.m_Actor.Location.Map.LocalTime.TurnCounter));
        if (perceptList1 != null)
        {
          Percept percept1 = (Percept) null;
          ActorAction actorAction1 = (ActorAction) null;
          float num1 = (float) int.MaxValue;
          foreach (Percept percept2 in perceptList1)
          {
            float num2 = (float) game.Rules.GridDistance(this.m_Actor.Location.Position, percept2.Location.Position);
            if ((double) num2 < (double) num1)
            {
              ActorAction actorAction2 = this.BehaviorStupidBumpToward(game, percept2.Location.Position);
              if (actorAction2 != null)
              {
                num1 = num2;
                percept1 = percept2;
                actorAction1 = actorAction2;
              }
            }
          }
          if (actorAction1 != null)
          {
            this.m_Actor.Activity = Activity.CHASING;
            this.m_Actor.TargetActor = percept1.Percepted as Actor;
            return actorAction1;
          }
        }
        List<Percept> perceptList2 = this.Filter(game, percepts2, (Predicate<Percept>) (p => p.Turn != this.m_Actor.Location.Map.LocalTime.TurnCounter));
        if (perceptList2 != null)
        {
          Percept percept1 = (Percept) null;
          ActorAction actorAction1 = (ActorAction) null;
          float num1 = (float) int.MaxValue;
          foreach (Percept percept2 in perceptList2)
          {
            float num2 = (float) game.Rules.GridDistance(this.m_Actor.Location.Position, percept2.Location.Position);
            if ((double) num2 < (double) num1)
            {
              ActorAction actorAction2 = this.BehaviorStupidBumpToward(game, percept2.Location.Position);
              if (actorAction2 != null)
              {
                num1 = num2;
                percept1 = percept2;
                actorAction1 = actorAction2;
              }
            }
          }
          if (actorAction1 != null)
          {
            this.m_Actor.Activity = Activity.CHASING;
            this.m_Actor.TargetActor = percept1.Percepted as Actor;
            return actorAction1;
          }
        }
      }
      ActorAction actorAction = this.BehaviorTrackScent(game, this.m_LivingSmellSensor.Scents);
      if (actorAction != null)
      {
        this.m_Actor.Activity = Activity.TRACKING;
        return actorAction;
      }
      this.m_Actor.Activity = Activity.IDLE;
      return this.BehaviorWander(game);
    }
  }
}
