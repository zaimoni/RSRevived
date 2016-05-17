// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CHARGuardAI
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
  internal class CHARGuardAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "Go away",
      "Damn it I'm trapped!",
      "Hey"
    };
    private const int LOS_MEMORY = WorldTime.TURNS_PER_HOUR/3;
    private MemorizedSensor m_MemorizedSensor;

    protected override void CreateSensors()
    {
      m_MemorizedSensor = new MemorizedSensor(new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS), LOS_MEMORY);
    }

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
    }

    public override void OptimizeBeforeSaving()
    {
      m_MemorizedSensor.Forget(m_Actor);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      return m_MemorizedSensor.Sense(game, m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemorizedSensor.Sensor as LOSSensor).FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);

      ActorAction tmpAction = BehaviorEquipBodyArmor(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      
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

      // Mysteriously, CHAR guards do not throw grenades even though their offices stock them.

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      // All free actions go above the check for enemies.
      List<Percept> enemies = FilterEnemies(game, percepts1);
      List<Percept> perceptList1 = FilterCurrent(enemies);
      if (perceptList1 != null) {
        List<Percept> percepts3 = FilterFireTargets(game, perceptList1);
        if (percepts3 != null) {
          Percept target = FilterNearest(percepts3);
          Actor actor = target.Percepted as Actor;
          tmpAction = BehaviorRangedAttack(game, target);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = actor;
            return tmpAction;
          }
        }
      }
      if (perceptList1 != null) {
        object percepted = FilterNearest(perceptList1).Percepted;
        tmpAction = BehaviorFightOrFlee(game, perceptList1, true, true, ActorCourage.COURAGEOUS, CHARGuardAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }
      List<Percept> perceptList2 = FilterNonEnemies(game, percepts1);
      if (perceptList2 != null) {
        List<Percept> percepts3 = Filter(game, perceptList2, (Predicate<Percept>) (p =>
        {
          Actor actor = p.Percepted as Actor;
          if (actor.Faction == game.GameFactions.TheCHARCorporation)
            return false;
          return game.IsInCHARProperty(actor.Location);
        }));
        if (percepts3 != null) {
          Actor target = FilterNearest(percepts3).Percepted as Actor;
          game.DoMakeAggression(m_Actor, target);
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target;
          return new ActionSay(m_Actor, game, target, "Hey YOU!", RogueGame.Sayflags.IS_IMPORTANT);
        }
      }
      if (null != enemies && perceptList2 != null) {
        tmpAction = BehaviorWarnFriends(game, perceptList2, FilterNearest(enemies).Percepted as Actor);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      tmpAction = BehaviorRestIfTired(game);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (enemies != null) {
        Percept target = FilterNearest(enemies);
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
      if (m_Actor.IsSleepy && null == enemies) {
        tmpAction = BehaviorSleep(game);
        if (null != tmpAction) {
          if (tmpAction is ActionSleep)
            m_Actor.Activity = Activity.SLEEPING;
          return tmpAction;
        }
      }
      if (m_Actor.HasLeader && !DontFollowLeader) {
        Point position = m_Actor.Leader.Location.Position;
        bool isVisible = FOV.Contains(m_Actor.Leader.Location.Position);
        tmpAction = BehaviorFollowActor(game, m_Actor.Leader, position, isVisible, 1);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      }
      tmpAction = BehaviorWander(game, (Predicate<Location>) (loc => RogueGame.IsInCHAROffice(loc)));
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
