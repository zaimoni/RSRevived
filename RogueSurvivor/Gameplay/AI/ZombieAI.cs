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

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

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

    private readonly MemorizedSensor m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    private SmellSensor m_MasterSmellSensor = null;
    private ExplorationData m_Exploration = null;

    public ZombieAI()
    {
    }

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      if (!m_Actor.Model.Abilities.IsUndeadMaster) m_MasterSmellSensor = new SmellSensor(Odor.UNDEAD_MASTER);
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
      if (null != m_MasterSmellSensor) perceptList.AddRange(m_MasterSmellSensor.Sense(m_Actor));
      return perceptList;
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }

    protected override ActorAction SelectAction(RogueGame game)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());
      List<Percept> percepts1 = FilterSameMap(UpdateSensors());

      if (m_Actor.Model.Abilities.ZombieAI_Explore) m_Exploration.Update(m_Actor.Location);

      List<Percept> enemies = FilterEnemies(percepts1);
      ActorAction tmpAction;
      if (enemies != null) {
        tmpAction = TargetGridMelee(FilterCurrent(enemies));
        if (null != tmpAction) return tmpAction;
        tmpAction = TargetGridMelee(FilterOld(enemies));
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorGoEatCorpse(percepts1);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (m_Actor.Model.Abilities.AI_CanUseAIExits && game.Rules.RollChance(USE_EXIT_CHANCE)) {
        tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES | BaseAI.UseExitFlags.DONT_BACKTRACK);
        if (null != tmpAction) {
          m_MemLOSSensor.Clear();
          return tmpAction;
        }
      }
      if (!m_Actor.Model.Abilities.IsUndeadMaster) {
        Percept percept = FilterNearest(percepts1.FilterT<Actor>(a => a.Model.Abilities.IsUndeadMaster));
        if (percept != null) {
          tmpAction = BehaviorStupidBumpToward(RandomPositionNear(game.Rules, m_Actor.Location.Map, percept.Location.Position, 3));
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FOLLOWING;
            m_Actor.TargetActor = percept.Percepted as Actor;
            return tmpAction;
          }
        }
      }
      if (!m_Actor.Model.Abilities.IsUndeadMaster) {
        tmpAction = BehaviorTrackScent(m_MasterSmellSensor.Scents);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.TRACKING;
          return tmpAction;
        }
      }
      tmpAction = BehaviorTrackScent(m_LivingSmellSensor.Scents);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.TRACKING;
        return tmpAction;
      }
      if (m_Actor.AbleToPush && game.Rules.RollChance(PUSH_OBJECT_CHANCE)) {
        tmpAction = BehaviorPushNonWalkableObject(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (m_Actor.Model.Abilities.ZombieAI_Explore) {
        tmpAction = BehaviorExplore(game, m_Exploration);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
