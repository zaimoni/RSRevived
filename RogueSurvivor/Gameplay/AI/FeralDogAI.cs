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

      // dogs target their leader's enemy before the usual check for enemies
      if (m_Actor.HasLeader)
      {
        Actor targetActor = m_Actor.Leader.TargetActor;
        if (targetActor != null && targetActor.Location.Map == m_Actor.Location.Map)
        {
          game.DoSay(m_Actor, targetActor, "GRRRRRR WAF WAF", RogueGame.Sayflags.IS_FREE_ACTION);
          ActorAction actorAction = BehaviorStupidBumpToward(game, targetActor.Location.Position);
          if (actorAction != null)
          {
                        m_Actor.IsRunning = true;
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = targetActor;
            return actorAction;
          }
        }
      }

      List<Percept> enemies = FilterEnemies(game, percepts1);
      // dogs cannot order their followers to stay behind
      bool hasVisibleLeader = m_Actor.HasLeader && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
      bool isLeaderFighting = m_Actor.HasLeader && IsAdjacentToEnemy(game, m_Actor.Leader);
      if (enemies != null)
      {
        ActorAction actorAction = BehaviorFightOrFlee(game, enemies, hasVisibleLeader, isLeaderFighting, Directives.Courage, FeralDogAI.FIGHT_EMOTES);
        if (actorAction != null)
        {
                    m_Actor.IsRunning = true;
          return actorAction;
        }
      }
      if (game.IsAlmostHungry(m_Actor))
      {
        List<Percept> stacksPercepts = FilterStacks(game, percepts1);
        if (stacksPercepts != null)
        {
          ActorAction actorAction = BehaviorGoEatFoodOnGround(game, stacksPercepts);
          if (actorAction != null)
          {
                        m_Actor.IsRunning = true;
                        m_Actor.Activity = Activity.IDLE;
            return actorAction;
          }
        }
      }
      if (m_Actor.IsHungry)
      {
        List<Percept> corpsesPercepts = FilterCorpses(game, percepts1);
        if (corpsesPercepts != null)
        {
          ActorAction actorAction = BehaviorGoEatCorpse(game, corpsesPercepts);
          if (actorAction != null)
          {
                        m_Actor.IsRunning = true;
                        m_Actor.Activity = Activity.IDLE;
            return actorAction;
          }
        }
      }
      if (m_Actor.IsSleepy)
      {
                m_Actor.Activity = Activity.SLEEPING;
        return (ActorAction) new ActionSleep(m_Actor, game);
      }
      if (m_Actor.HasLeader)
      {
        Point position = m_Actor.Leader.Location.Position;
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        ActorAction actorAction = BehaviorFollowActor(game, m_Actor.Leader, position, hasVisibleLeader, maxDist);
        if (actorAction != null)
        {
                    m_Actor.IsRunning = true;
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
          return actorAction;
        }
      }
            m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
