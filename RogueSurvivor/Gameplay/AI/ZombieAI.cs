// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.ZombieAI
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
  internal class ZombieAI : BaseAI
  {
    private const int LOS_MEMORY = 20;
    private const int EXPLORATION_LOCATIONS = 30;
    private const int EXPLORATION_ZONES = 3;
    private const int USE_EXIT_CHANCE = 50;
    private const int FOLLOW_SCENT_THROUGH_EXIT_CHANCE = 90;    // dead constant?
    private const int PUSH_OBJECT_CHANCE = 20;
    private MemorizedSensor m_MemLOSSensor;
    private SmellSensor m_LivingSmellSensor;
    private SmellSensor m_MasterSmellSensor;
    private ExplorationData m_Exploration;

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      if (!this.m_Actor.Model.Abilities.ZombieAI_Explore) return;
      m_Exploration = new ExplorationData(EXPLORATION_LOCATIONS, EXPLORATION_ZONES);
    }

    protected override void CreateSensors()
    {
      this.m_MemLOSSensor = new MemorizedSensor((Sensor) new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES), LOS_MEMORY);
      this.m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
      this.m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      List<Percept> perceptList = this.m_MemLOSSensor.Sense(game, this.m_Actor);
      perceptList.AddRange((IEnumerable<Percept>) this.m_LivingSmellSensor.Sense(game, this.m_Actor));
      perceptList.AddRange((IEnumerable<Percept>) this.m_MasterSmellSensor.Sense(game, this.m_Actor));
      return perceptList;
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      HashSet<Point> fov = (this.m_MemLOSSensor.Sensor as LOSSensor).FOV;
      List<Percept> percepts1 = this.FilterSameMap(game, percepts);
      if (this.m_Actor.Model.Abilities.ZombieAI_Explore)
        this.m_Exploration.Update(this.m_Actor.Location);
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
      List<Percept> corpsesPercepts = this.FilterCorpses(game, percepts1);
      if (corpsesPercepts != null)
      {
        ActorAction actorAction = this.BehaviorGoEatCorpse(game, corpsesPercepts);
        if (actorAction != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction;
        }
      }
      if (this.m_Actor.Model.Abilities.AI_CanUseAIExits && game.Rules.RollChance(USE_EXIT_CHANCE))
      {
        ActorAction actorAction = this.BehaviorUseExit(game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES | BaseAI.UseExitFlags.DONT_BACKTRACK);
        if (actorAction != null)
        {
          this.m_MemLOSSensor.Clear();
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction;
        }
      }
      if (!this.m_Actor.Model.Abilities.IsUndeadMaster)
      {
        Percept percept = this.FilterNearest(game, this.FilterActors(game, percepts1, (Predicate<Actor>) (a => a.Model.Abilities.IsUndeadMaster)));
        if (percept != null)
        {
          ActorAction actorAction = this.BehaviorStupidBumpToward(game, this.RandomPositionNear(game.Rules, this.m_Actor.Location.Map, percept.Location.Position, 3));
          if (actorAction != null)
          {
            this.m_Actor.Activity = Activity.FOLLOWING;
            this.m_Actor.TargetActor = percept.Percepted as Actor;
            return actorAction;
          }
        }
      }
      if (!this.m_Actor.Model.Abilities.IsUndeadMaster)
      {
        ActorAction actorAction = this.BehaviorTrackScent(game, this.m_MasterSmellSensor.Scents);
        if (actorAction != null)
        {
          this.m_Actor.Activity = Activity.TRACKING;
          return actorAction;
        }
      }
      ActorAction actorAction3 = this.BehaviorTrackScent(game, this.m_LivingSmellSensor.Scents);
      if (actorAction3 != null)
      {
        this.m_Actor.Activity = Activity.TRACKING;
        return actorAction3;
      }
      if (game.Rules.HasActorPushAbility(this.m_Actor) && game.Rules.RollChance(PUSH_OBJECT_CHANCE))
      {
        ActorAction actorAction1 = this.BehaviorPushNonWalkableObject(game);
        if (actorAction1 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction1;
        }
      }
      if (this.m_Actor.Model.Abilities.ZombieAI_Explore)
      {
        ActorAction actorAction1 = this.BehaviorExplore(game, this.m_Exploration);
        if (actorAction1 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction1;
        }
      }
      this.m_Actor.Activity = Activity.IDLE;
      return this.BehaviorWander(game);
    }
  }
}
