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
using System.Diagnostics.Contracts;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class FeralDogAI : BaseAI
  {
    private static readonly string[] FIGHT_EMOTES = new string[MAX_EMOTES]
    {
      "waf",
      "waf!?",
      "GRRRRR WAF WAF"
    };
    private const int FOLLOW_NPCLEADER_MAXDIST = 1;
    private const int FOLLOW_PLAYERLEADER_MAXDIST = 1;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private readonly LOSSensor m_LOSSensor = new LOSSensor(VISION_SEES);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);

    public FeralDogAI()
    {
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    protected override void SensorsOwnedBy(Actor actor) { m_LOSSensor.OwnedBy(actor); }

    public override List<Percept> UpdateSensors()
    {
      List<Percept> perceptList = m_LOSSensor.Sense(m_Actor);
      perceptList.AddRange(m_LivingSmellSensor.Sense(m_Actor));
      return perceptList;
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      List<Percept> percepts_all = FilterSameMap(UpdateSensors());

      // dogs target their leader's enemy before the usual check for enemies
      if (m_Actor.HasLeader) {
        Actor targetActor = m_Actor.Leader.TargetActor;
        if (targetActor != null && targetActor.Location.Map == m_Actor.Location.Map) {
          RogueGame.DoSay(m_Actor, targetActor, "GRRRRRR WAF WAF", RogueGame.Sayflags.IS_FREE_ACTION);
          ActorAction actorAction = BehaviorStupidBumpToward(targetActor.Location);
          if (actorAction != null) {
            m_Actor.IsRunning = true;
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = targetActor;
            return actorAction;
          }
        }
      }

      List<Percept> enemies = FilterEnemies(percepts_all);
      // dogs cannot order their followers to stay behind
      if (enemies != null) {
        ActorAction actorAction = BehaviorFightOrFlee(game, enemies, FeralDogAI.FIGHT_EMOTES);
        if (actorAction != null) {
          m_Actor.IsRunning = true;
          return actorAction;
        }
      }
      if (m_Actor.IsAlmostHungry) {
        ActorAction actorAction = BehaviorGoEatFoodOnGround(percepts_all.FilterT<Inventory>());
        if (actorAction != null) {
          m_Actor.IsRunning = true;
          m_Actor.Activity = Activity.IDLE;
          return actorAction;
        }
      }
      if (m_Actor.IsHungry) {
        ActorAction actorAction = BehaviorGoEatCorpse(percepts_all);
        if (actorAction != null) {
          m_Actor.IsRunning = true;
          m_Actor.Activity = Activity.IDLE;
          return actorAction;
        }
      }
      if (m_Actor.IsSleepy) {
        m_Actor.Activity = Activity.SLEEPING;
        return new ActionSleep(m_Actor);
      }
      if (m_Actor.HasLeader) {
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        ActorAction actorAction = BehaviorFollowActor(m_Actor.Leader, maxDist);
        if (actorAction != null) {
          m_Actor.IsRunning = true;
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return actorAction;
        }
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
