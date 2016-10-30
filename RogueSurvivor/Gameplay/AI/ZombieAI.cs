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
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class ZombieAI : BaseAI
  {
    private const int LOS_MEMORY = 2*(WorldTime.TURNS_PER_HOUR/3);
    private const int USE_EXIT_CHANCE = 50;
    private const int FOLLOW_SCENT_THROUGH_EXIT_CHANCE = 90;    // dead constant?
    private const int PUSH_OBJECT_CHANCE = 20;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private MemorizedSensor m_MemLOSSensor;
    private SmellSensor m_LivingSmellSensor;
    private SmellSensor m_MasterSmellSensor;
    private ExplorationData m_Exploration;

    public ZombieAI()
    {
      m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
      m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
      m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);
    }

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      if (!m_Actor.Model.Abilities.ZombieAI_Explore) return;
      m_Exploration = new ExplorationData();
    }

    public override void OptimizeBeforeSaving()
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      List<Percept> perceptList = m_MemLOSSensor.Sense(m_Actor);
      perceptList.AddRange(m_LivingSmellSensor.Sense(m_Actor));
      perceptList.AddRange(m_MasterSmellSensor.Sense(m_Actor));
      return perceptList;
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());
      List<Percept> percepts1 = FilterSameMap(percepts);

      if (m_Actor.Model.Abilities.ZombieAI_Explore) m_Exploration.Update(m_Actor.Location);

      List<Percept> enemies = FilterEnemies(percepts1);
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
        List<Percept> perceptList2 = Filter(enemies, (Predicate<Percept>) (p => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter));
        if (perceptList2 != null) {
          tmpAction = TargetGridMelee(game, perceptList2, out tmpActor);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = tmpActor;
            return tmpAction;
          }
        }
      }
      tmpAction = BehaviorGoEatCorpse(game, FilterCorpses(percepts1));
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (m_Actor.Model.Abilities.AI_CanUseAIExits && game.Rules.RollChance(USE_EXIT_CHANCE)) {
        ActorAction actorAction = BehaviorUseExit(game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES | BaseAI.UseExitFlags.DONT_BACKTRACK);
        if (actorAction != null) {
          m_MemLOSSensor.Clear();
          return actorAction;
        }
      }
      if (!m_Actor.Model.Abilities.IsUndeadMaster) {
        Percept percept = FilterNearest(FilterActors(percepts1, (Predicate<Actor>) (a => a.Model.Abilities.IsUndeadMaster)));
        if (percept != null) {
          ActorAction actorAction = BehaviorStupidBumpToward(game, RandomPositionNear(game.Rules, m_Actor.Location.Map, percept.Location.Position, 3));
          if (actorAction != null) {
            m_Actor.Activity = Activity.FOLLOWING;
            m_Actor.TargetActor = percept.Percepted as Actor;
            return actorAction;
          }
        }
      }
      if (!m_Actor.Model.Abilities.IsUndeadMaster) {
        ActorAction actorAction = BehaviorTrackScent(game, m_MasterSmellSensor.Scents);
        if (actorAction != null) {
          m_Actor.Activity = Activity.TRACKING;
          return actorAction;
        }
      }
      ActorAction actorAction3 = BehaviorTrackScent(game, m_LivingSmellSensor.Scents);
      if (actorAction3 != null) {
        m_Actor.Activity = Activity.TRACKING;
        return actorAction3;
      }
      if (m_Actor.CanPush && game.Rules.RollChance(PUSH_OBJECT_CHANCE)) {
        ActorAction actorAction1 = BehaviorPushNonWalkableObject(game);
        if (actorAction1 != null) {
          m_Actor.Activity = Activity.IDLE;
          return actorAction1;
        }
      }
      if (m_Actor.Model.Abilities.ZombieAI_Explore) {
        ActorAction actorAction1 = BehaviorExplore(game, m_Exploration);
        if (actorAction1 != null) {
          m_Actor.Activity = Activity.IDLE;
          return actorAction1;
        }
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
