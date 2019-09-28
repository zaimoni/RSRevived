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
using System.Runtime.Serialization;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  /// <summary>
  /// Zombie AI : used by Zombie, Zombie Master and Zombified branches.
  /// </summary>
  internal class ZombieAI : BaseAI
  {
    private const int LOS_MEMORY = 2*(WorldTime.TURNS_PER_HOUR/3);
    private const int USE_EXIT_CHANCE = 50;
    private const int FOLLOW_SCENT_THROUGH_EXIT_CHANCE = 90;    // dead constant?
    private const int PUSH_OBJECT_CHANCE = 20;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private readonly MemorizedSensor m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    private SmellSensor m_MasterSmellSensor;
    private ExplorationData m_Exploration;

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

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      m_LivingSmellSensor.Sense(m_Actor);
      m_MasterSmellSensor?.Sense(m_Actor);
      return m_MemLOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }
    public override Dictionary<Location, Actor> friends_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).friends; } }
    public override Dictionary<Location, Actor> enemies_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).enemies; } }
    protected override void SensorsOwnedBy(Actor actor) { (m_MemLOSSensor.Sensor as LOSSensor).OwnedBy(actor); }

    protected override ActorAction SelectAction(RogueGame game)
    {
      var tmp_abilities = m_Actor.Model.Abilities;
      if (tmp_abilities.ZombieAI_Explore && m_Actor.Location != PrevLocation) m_Exploration.Update(m_Actor.Location);

      ActorAction tmpAction;
      if (null != (_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors()))))) {
        tmpAction = TargetGridMelee(FilterCurrent(_enemies));
        if (null != tmpAction) return tmpAction;
        tmpAction = TargetGridMelee(FilterOld(_enemies));
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorGoEatCorpse(_all);
      if (null != tmpAction) return tmpAction;

      var tmp_rules = game.Rules;
      if (tmp_abilities.AI_CanUseAIExits && tmp_rules.RollChance(USE_EXIT_CHANCE)) {
        tmpAction = BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES | UseExitFlags.DONT_BACKTRACK);
        if (null != tmpAction) {
          m_MemLOSSensor.Clear();
          return tmpAction;
        }
      }
      if (!tmp_abilities.IsUndeadMaster) {
        Percept percept = FilterNearest(_all.FilterT<Actor>(a => a.Model.Abilities.IsUndeadMaster));
        if (percept != null) {
          tmpAction = BehaviorStupidBumpToward(RandomPositionNear(tmp_rules, percept.Location, 3), true, true);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FOLLOWING;
            m_Actor.TargetActor = percept.Percepted as Actor;
            return tmpAction;
          }
        }
      }
      if (!tmp_abilities.IsUndeadMaster) {
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
      if (m_Actor.AbleToPush && tmp_rules.RollChance(PUSH_OBJECT_CHANCE)) {
        tmpAction = BehaviorPushNonWalkableObject();
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (tmp_abilities.ZombieAI_Explore) {
        tmpAction = BehaviorExplore(m_Exploration);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      return BehaviorWander();  // alpha 10.1: do not use exploration data even though we have it
    }
  }
}
