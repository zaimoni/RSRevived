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
    private const int LOS_MEMORY = 10;
    private const int FOLLOW_LEADER_MIN_DIST = 1;
    private const int FOLLOW_LEADER_MAX_DIST = 2;
    private const int EXPLORATION_LOCATIONS = 30;
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
      this.m_Exploration = new ExplorationData(30, 3);
    }

    protected override void CreateSensors()
    {
      this.m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS);
      this.m_MemLOSSensor = new MemorizedSensor((Sensor) this.m_LOSSensor, 10);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      return this.m_MemLOSSensor.Sense(game, this.m_Actor);
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterSameMap(game, percepts);
      if (this.Order != null)
      {
        ActorAction actorAction = this.ExecuteOrder(game, this.Order, percepts1);
        if (actorAction == null)
        {
          this.SetOrder((ActorOrder) null);
        }
        else
        {
          this.m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }
      }
      this.m_Actor.IsRunning = false;
      List<Percept> percepts2 = this.FilterEnemies(game, percepts1);
      List<Percept> perceptList = this.FilterCurrent(game, percepts2);
      bool flag1 = this.m_Actor.HasLeader && !this.DontFollowLeader;
      bool flag2 = perceptList != null;
      bool flag3 = percepts2 != null;
      this.m_Exploration.Update(this.m_Actor.Location);
      ActorAction actorAction1 = this.BehaviorFleeFromExplosives(game, this.FilterStacks(game, percepts1));
      if (actorAction1 != null)
      {
        this.m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
        return actorAction1;
      }
      if (flag2)
      {
        ActorAction actorAction2 = this.BehaviorThrowGrenade(game, this.m_LOSSensor.FOV, perceptList);
        if (actorAction2 != null)
          return actorAction2;
      }
      ActorAction actorAction3 = this.BehaviorEquipWeapon(game);
      if (actorAction3 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction3;
      }
      ActorAction actorAction4 = this.BehaviorEquipBodyArmor(game);
      if (actorAction4 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction4;
      }
      if (flag2)
      {
        if (game.Rules.RollChance(50))
        {
          List<Percept> friends = this.FilterNonEnemies(game, percepts1);
          if (friends != null)
          {
            ActorAction actorAction2 = this.BehaviorWarnFriends(game, friends, this.FilterNearest(game, perceptList).Percepted as Actor);
            if (actorAction2 != null)
            {
              this.m_Actor.Activity = Activity.IDLE;
              return actorAction2;
            }
          }
        }
        List<Percept> percepts3 = this.FilterFireTargets(game, perceptList);
        if (percepts3 != null)
        {
          Percept target = this.FilterNearest(game, percepts3);
          ActorAction actorAction2 = this.BehaviorRangedAttack(game, target);
          if (actorAction2 != null)
          {
            this.m_Actor.Activity = Activity.FIGHTING;
            this.m_Actor.TargetActor = target.Percepted as Actor;
            return actorAction2;
          }
        }
        ActorAction actorAction5 = this.BehaviorFightOrFlee(game, perceptList, true, true, ActorCourage.COURAGEOUS, SoldierAI.FIGHT_EMOTES);
        if (actorAction5 != null)
          return actorAction5;
      }
      ActorAction actorAction6 = this.BehaviorRestIfTired(game);
      if (actorAction6 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction6;
      }
      if (flag3)
      {
        Percept target = this.FilterNearest(game, percepts2);
        ActorAction actorAction2 = this.BehaviorChargeEnemy(game, target);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.FIGHTING;
          this.m_Actor.TargetActor = target.Percepted as Actor;
          return actorAction2;
        }
      }
      ActorAction actorAction7 = this.BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (actorAction7 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction7;
      }
      if (!flag3 && this.WouldLikeToSleep(game, this.m_Actor) && (this.IsInside(this.m_Actor) && game.Rules.CanActorSleep(this.m_Actor)))
      {
        ActorAction actorAction2 = this.BehaviorSecurePerimeter(game, this.m_LOSSensor.FOV);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
        ActorAction actorAction5 = this.BehaviorSleep(game, this.m_LOSSensor.FOV);
        if (actorAction5 != null)
        {
          if (actorAction5 is ActionSleep)
            this.m_Actor.Activity = Activity.SLEEPING;
          return actorAction5;
        }
      }
      List<Percept> percepts4 = this.Filter(game, percepts2, (Predicate<Percept>) (p => p.Turn != this.m_Actor.Location.Map.LocalTime.TurnCounter));
      if (percepts4 != null)
      {
        Percept target = this.FilterNearest(game, percepts4);
        if (this.m_Actor.Location == target.Location)
        {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, this.m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
        }
        ActorAction actorAction2 = this.BehaviorChargeEnemy(game, target);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.FIGHTING;
          this.m_Actor.TargetActor = target.Percepted as Actor;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(50))
      {
        ActorAction actorAction2 = this.BehaviorBuildLargeFortification(game, 1);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(20))
      {
        ActorAction actorAction2 = this.BehaviorBuildSmallFortification(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (flag1)
      {
        Point position = this.m_Actor.Leader.Location.Position;
        ActorAction actorAction2 = this.BehaviorHangAroundActor(game, this.m_Actor.Leader, position, 1, 2);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.FOLLOWING;
          this.m_Actor.TargetActor = this.m_Actor.Leader;
          return actorAction2;
        }
      }
      if (this.m_Actor.CountFollowers > 0)
      {
        Actor target;
        ActorAction actorAction2 = this.BehaviorDontLeaveFollowersBehind(game, 4, out target);
        if (actorAction2 != null)
        {
          if (game.Rules.RollChance(50))
          {
            if (target.IsSleeping)
              game.DoEmote(this.m_Actor, string.Format("patiently waits for {0} to wake up.", (object) target.Name));
            else if (this.m_LOSSensor.FOV.Contains(target.Location.Position))
              game.DoEmote(this.m_Actor, string.Format("{0}! Don't lag behind!", (object) target.Name));
            else
              game.DoEmote(this.m_Actor, string.Format("Where the hell is {0}?", (object) target.Name));
          }
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      ActorAction actorAction8 = this.BehaviorExplore(game, this.m_Exploration);
      if (actorAction8 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction8;
      }
      this.m_Actor.Activity = Activity.IDLE;
      return this.BehaviorWander(game);
    }
  }
}
