// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SoldierAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SoldierAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "Damn",
      "Fuck I'm cornered",
      "Die"
    };
    private const int LOS_MEMORY = WorldTime.TURNS_PER_HOUR/3;
    private const int FOLLOW_LEADER_MIN_DIST = 1;
    private const int FOLLOW_LEADER_MAX_DIST = 2;
    private const int BUILD_SMALL_FORT_CHANCE = 20;
    private const int BUILD_LARGE_FORT_CHANCE = 50;
    private const int START_FORT_LINE_CHANCE = 1;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS;

    private MemorizedSensor m_MemLOSSensor;
    private ExplorationData m_Exploration;

    public SoldierAI()
    {
      m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
      m_Exploration = new ExplorationData();
    }

    public override void OptimizeBeforeSaving()
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    protected override List<Percept> _UpdateSensors()
    {
      return m_MemLOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);

      BehaviorEquipBodyArmor(game);
      
      // OrderableAI specific: respond to orders
      if (null != Order)
      {
        ActorAction actorAction = ExecuteOrder(game, Order, percepts1);
        if (null != actorAction)
          {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
          }

        SetOrder(null);
      }
      m_Actor.IsRunning = false;

      m_Exploration.Update(m_Actor.Location);

      // fleeing from explosives is done before the enemies check
      ActorAction tmpAction = BehaviorFleeFromExplosives(game, percepts1);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
        return tmpAction;
      }

      List<Percept> enemies = FilterEnemies(percepts1);
      List<Percept> current_enemies = FilterCurrent(enemies);

      // throwing a grenade overrides normal weapon equipping choices
      if (null != current_enemies) {
        tmpAction = BehaviorThrowGrenade(game, current_enemies);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      // all free actions have to be before targeting enemies
      if (null != current_enemies) {
        if (game.Rules.RollChance(50)) {
          List<Percept> friends = FilterNonEnemies(percepts1);
          if (friends != null) {
            tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.IDLE;
              return tmpAction;
            }
          }
        }
        List<Percept> percepts3 = FilterFireTargets(game, current_enemies);
        if (percepts3 != null) {
          Actor target = FilterNearest(percepts3).Percepted as Actor;
          tmpAction = BehaviorRangedAttack(game, target);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = target;
            return tmpAction;
          }
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, true, true, ActorCourage.COURAGEOUS, SoldierAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorRestIfTired();
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (null != enemies) {
        Percept target = FilterNearest(enemies);
        tmpAction = BehaviorChargeEnemy(game, target);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }
      tmpAction = BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (null == enemies && m_Actor.WouldLikeToSleep && (m_Actor.IsInside && game.Rules.CanActorSleep(m_Actor))) {
        tmpAction = BehaviorSecurePerimeter(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        tmpAction = BehaviorSleep(game);
        if (null != tmpAction) {
          if (tmpAction is ActionSleep)
            m_Actor.Activity = Activity.SLEEPING;
          return tmpAction;
        }
      }
      if (current_enemies != null) {
        Percept target = FilterNearest(current_enemies);
        if (m_Actor.Location == target.Location) {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
        }
        tmpAction = BehaviorChargeEnemy(game, target);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(BUILD_LARGE_FORT_CHANCE)) {
        tmpAction = BehaviorBuildLargeFortification(game, START_FORT_LINE_CHANCE);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(BUILD_SMALL_FORT_CHANCE)) {
        tmpAction = BehaviorBuildSmallFortification(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (m_Actor.HasLeader && !DontFollowLeader) {
        Point position = m_Actor.Leader.Location.Position;
        tmpAction = BehaviorHangAroundActor(game, m_Actor.Leader, position, FOLLOW_LEADER_MIN_DIST, FOLLOW_LEADER_MAX_DIST);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      }
      if (m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(game, 4, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE)) {
            if (target.IsSleeping)
              game.DoEmote(m_Actor, string.Format("patiently waits for {0} to wake up.", (object) target.Name));
            else if (FOV.Contains(target.Location.Position))
              game.DoEmote(m_Actor, string.Format("{0}! Don't lag behind!", (object) target.Name));
            else
              game.DoEmote(m_Actor, string.Format("Where the hell is {0}?", (object) target.Name));
          }
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      tmpAction = BehaviorExplore(game, m_Exploration);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
