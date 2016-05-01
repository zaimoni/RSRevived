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
    private const int EXPLORATION_LOCATIONS = WorldTime.TURNS_PER_HOUR;
    private const int EXPLORATION_ZONES = 3;
    private const int BUILD_SMALL_FORT_CHANCE = 20;
    private const int BUILD_LARGE_FORT_CHANCE = 50;
    private const int START_FORT_LINE_CHANCE = 1;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;
    private LOSSensor m_LOSSensor;
    private MemorizedSensor m_MemLOSSensor;
    private ExplorationData m_Exploration;

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      m_Exploration = new ExplorationData(EXPLORATION_LOCATIONS, EXPLORATION_ZONES);
    }

    protected override void CreateSensors()
    {
      this.m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS);
      this.m_MemLOSSensor = new MemorizedSensor((Sensor) this.m_LOSSensor, LOS_MEMORY);
    }

    public override void OptimizeBeforeSaving()
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      return this.m_MemLOSSensor.Sense(game, this.m_Actor);
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);
      
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
      ActorAction tmpAction = this.BehaviorFleeFromExplosives(game, this.FilterStacks(game, percepts1));
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
        return tmpAction;
      }

      List<Percept> percepts2 = FilterEnemies(game, percepts1);
      List<Percept> perceptList = FilterCurrent(percepts2);
      bool flag1 = m_Actor.HasLeader && !DontFollowLeader;
      bool flag3 = percepts2 != null;

      // throwing a grenade overrides normal weapon equipping choices
      if (null != perceptList)
      {
        tmpAction = BehaviorThrowGrenade(game, this.m_LOSSensor.FOV, perceptList);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = BehaviorEquipBodyArmor(game);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      // all free actions have to be before targeting enemies
      if (null != perceptList) {
        if (game.Rules.RollChance(50)) {
          List<Percept> friends = this.FilterNonEnemies(game, percepts1);
          if (friends != null) {
            tmpAction = BehaviorWarnFriends(game, friends, FilterNearest(perceptList).Percepted as Actor);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.IDLE;
              return tmpAction;
            }
          }
        }
        List<Percept> percepts3 = this.FilterFireTargets(game, perceptList);
        if (percepts3 != null) {
          Percept target = FilterNearest(percepts3);
          tmpAction = this.BehaviorRangedAttack(game, target);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
        tmpAction = BehaviorFightOrFlee(game, perceptList, true, true, ActorCourage.COURAGEOUS, SoldierAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = this.BehaviorRestIfTired(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (flag3) {
        Percept target = FilterNearest(percepts2);
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
      if (!flag3 && m_Actor.WouldLikeToSleep && (this.IsInside(this.m_Actor) && game.Rules.CanActorSleep(this.m_Actor))) {
        tmpAction = BehaviorSecurePerimeter(game, this.m_LOSSensor.FOV);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        tmpAction = BehaviorSleep(game, m_LOSSensor.FOV);
        if (null != tmpAction) {
          if (tmpAction is ActionSleep)
            m_Actor.Activity = Activity.SLEEPING;
          return tmpAction;
        }
      }
      List<Percept> percepts4 = FilterCurrent(percepts2);
      if (percepts4 != null) {
        Percept target = FilterNearest(percepts4);
        if (this.m_Actor.Location == target.Location) {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, this.m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
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
      if (flag1) {
        Point position = m_Actor.Leader.Location.Position;
        tmpAction = BehaviorHangAroundActor(game, this.m_Actor.Leader, position, FOLLOW_LEADER_MIN_DIST, FOLLOW_LEADER_MAX_DIST);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = this.m_Actor.Leader;
          return tmpAction;
        }
      }
      if (this.m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(game, 4, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE)) {
            if (target.IsSleeping)
              game.DoEmote(m_Actor, string.Format("patiently waits for {0} to wake up.", (object) target.Name));
            else if (this.m_LOSSensor.FOV.Contains(target.Location.Position))
              game.DoEmote(m_Actor, string.Format("{0}! Don't lag behind!", (object) target.Name));
            else
              game.DoEmote(m_Actor, string.Format("Where the hell is {0}?", (object) target.Name));
          }
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      tmpAction = BehaviorExplore(game, this.m_Exploration);
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
