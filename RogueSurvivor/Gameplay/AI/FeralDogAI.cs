// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.FeralDogAI
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
  internal class FeralDogAI : BaseAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "waf",
      "waf!?",
      "GRRRRR WAF WAF"
    };
    private const int FOLLOW_NPCLEADER_MAXDIST = 1;
    private const int FOLLOW_PLAYERLEADER_MAXDIST = 1;
    private LOSSensor m_LOSSensor;
    private SmellSensor m_LivingSmellSensor;

    protected override void CreateSensors()
    {
      this.m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES);
      this.m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      List<Percept> perceptList = this.m_LOSSensor.Sense(game, this.m_Actor);
      perceptList.AddRange((IEnumerable<Percept>) this.m_LivingSmellSensor.Sense(game, this.m_Actor));
      return perceptList;
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterSameMap(game, percepts);

      // dogs target their leader's enemy before the usual check for enemies
      if (this.m_Actor.HasLeader)
      {
        Actor targetActor = this.m_Actor.Leader.TargetActor;
        if (targetActor != null && targetActor.Location.Map == this.m_Actor.Location.Map)
        {
          game.DoSay(this.m_Actor, targetActor, "GRRRRRR WAF WAF", RogueGame.Sayflags.IS_FREE_ACTION);
          ActorAction actorAction = this.BehaviorStupidBumpToward(game, targetActor.Location.Position);
          if (actorAction != null)
          {
            this.m_Actor.IsRunning = true;
            this.m_Actor.Activity = Activity.FIGHTING;
            this.m_Actor.TargetActor = targetActor;
            return actorAction;
          }
        }
      }

      List<Percept> enemies = this.FilterEnemies(game, percepts1);
      bool flag = this.m_Actor.HasLeader && this.m_LOSSensor.FOV.Contains(this.m_Actor.Leader.Location.Position);
      bool isLeaderFighting = this.m_Actor.HasLeader && this.IsAdjacentToEnemy(game, this.m_Actor.Leader);
      if (enemies != null)
      {
        ActorAction actorAction = this.BehaviorFightOrFlee(game, enemies, flag, isLeaderFighting, this.Directives.Courage, FeralDogAI.FIGHT_EMOTES);
        if (actorAction != null)
        {
          this.m_Actor.IsRunning = true;
          return actorAction;
        }
      }
      if (game.IsAlmostHungry(this.m_Actor))
      {
        List<Percept> stacksPercepts = this.FilterStacks(game, percepts1);
        if (stacksPercepts != null)
        {
          ActorAction actorAction = this.BehaviorGoEatFoodOnGround(game, stacksPercepts);
          if (actorAction != null)
          {
            this.m_Actor.IsRunning = true;
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction;
          }
        }
      }
      if (game.Rules.IsActorHungry(this.m_Actor))
      {
        List<Percept> corpsesPercepts = this.FilterCorpses(game, percepts1);
        if (corpsesPercepts != null)
        {
          ActorAction actorAction = this.BehaviorGoEatCorpse(game, corpsesPercepts);
          if (actorAction != null)
          {
            this.m_Actor.IsRunning = true;
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction;
          }
        }
      }
      if (game.Rules.IsActorSleepy(this.m_Actor))
      {
        this.m_Actor.Activity = Activity.SLEEPING;
        return (ActorAction) new ActionSleep(this.m_Actor, game);
      }
      if (this.m_Actor.HasLeader)
      {
        Point position = this.m_Actor.Leader.Location.Position;
        int maxDist = this.m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        ActorAction actorAction = this.BehaviorFollowActor(game, this.m_Actor.Leader, position, flag, maxDist);
        if (actorAction != null)
        {
          this.m_Actor.IsRunning = true;
          this.m_Actor.Activity = Activity.FOLLOWING;
          this.m_Actor.TargetActor = this.m_Actor.Leader;
          return actorAction;
        }
      }
      this.m_Actor.Activity = Activity.IDLE;
      return this.BehaviorWander(game);
    }
  }
}
