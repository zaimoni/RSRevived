// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.GangAI
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
  internal class GangAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "Fuck you",
      "Fuck it I'm trapped!",
      "Come on"
    };
    private const int FOLLOW_NPCLEADER_MAXDIST = 1;
    private const int FOLLOW_PLAYERLEADER_MAXDIST = 1;
    private const int LOS_MEMORY = WorldTime.TURNS_PER_HOUR/3;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS;

    private MemorizedSensor m_MemorizedSensor;
    private ExplorationData m_Exploration;

    public GangAI()
    {
      m_MemorizedSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
      m_Exploration = new ExplorationData();
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
      
      BehaviorEquipBodyArmor(game);

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight(game)) {
          BehaviorUnequipLeftItem(game);
      }
      // end item juggling check

      // OrderableAI specific: respond to orders
      if (null != Order) {
        ActorAction actorAction = ExecuteOrder(game, Order, percepts1);
        if (null != actorAction) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.IsRunning = false;

      m_Exploration.Update(m_Actor.Location);

      // Bikers and gangsters don't throw grenades
      ActorAction tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      List<Percept> enemies = FilterEnemies(percepts1);
      List<Percept> current_enemies = FilterCurrent(enemies);
      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && FOV.Contains(m_Actor.Leader.Location.Position);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && IsAdjacentToEnemy(m_Actor.Leader);

      // all free actions must be above the enemies check
      if (null != current_enemies && ((m_Actor.HasLeader && !DontFollowLeader) || game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))) {
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
      }
      if (null != current_enemies) {
        if (game.Rules.RollChance(50)) {
          List<Percept> friends = FilterNonEnemies(percepts1);
          if (friends != null) {
            tmpAction = BehaviorWarnFriends(game, friends, FilterNearest(current_enemies).Percepted as Actor);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.IDLE;
              return tmpAction;
            }
          }
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, hasVisibleLeader, isLeaderFighting, ActorCourage.COURAGEOUS, GangAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = BehaviorRestIfTired(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (null != current_enemies && !m_Actor.IsTired) {
        Percept target = FilterNearest(current_enemies);
        tmpAction = BehaviorChargeEnemy(game, target);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }

      // handle food after enemies checks
      tmpAction = BehaviorEatProactively(game);
      if (null != tmpAction) return tmpAction;

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorEat(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        if (m_Actor.IsStarving || m_Actor.IsInsane) {
          tmpAction = BehaviorGoEatCorpse(game, FilterCorpses(percepts1));
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      if (null == enemies && m_Actor.WouldLikeToSleep && (m_Actor.IsInside && game.Rules.CanActorSleep(m_Actor))) {
        tmpAction = BehaviorSecurePerimeter(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        tmpAction = BehaviorSleep(game);
        if (null != tmpAction) {
          if (tmpAction is ActionSleep) m_Actor.Activity = Activity.SLEEPING;
          return tmpAction;
        }
      }
      tmpAction = BehaviorDropUselessItem(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      if (null == current_enemies) {
        Map map = m_Actor.Location.Map;
        List<Percept> percepts3 = FilterOut(FilterStacks(percepts1), (Predicate<Percept>) (p =>
        {
          if (p.Turn == map.LocalTime.TurnCounter)
            return IsOccupiedByOther(map, p.Location.Position);
          return true;
        }));
        if (percepts3 != null) {
          Percept percept = FilterNearest(percepts3);
          tmpAction = BehaviorGrabFromStack(game, percept.Location.Position, percept.Percepted as Inventory);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      if (null == current_enemies) {
        Map map = m_Actor.Location.Map;
        // rewriting this to work around a paradoxical bug indicating runtime state corruption
        Percept victimize = FilterNearest(FilterActors(FilterCurrent(percepts1), (Predicate<Actor>) (a =>
        {
          if (a.Inventory == null || a.Inventory.CountItems == 0 || IsFriendOf(a)) return false;
          if (!game.Rules.RollChance(game.Rules.ActorUnsuspicousChance(m_Actor, a))) return HasAnyInterestingItem(a.Inventory);
          game.DoEmote(a, string.Format("moves unnoticed by {0}.", (object)m_Actor.Name));
          return false;
        })));
        if (null!=victimize) {
          Actor target = victimize.Percepted as Actor;
          Item obj = FirstInterestingItem(target.Inventory);
          game.DoMakeAggression(m_Actor, target);
          m_Actor.Activity = Activity.CHASING;
          m_Actor.TargetActor = target;
          return new ActionSay(m_Actor, game, target, string.Format("Hey! That's some nice {0} you have here!", (object) obj.Model.SingleName), RogueGame.Sayflags.IS_IMPORTANT);
        }
      }
      tmpAction = BehaviorAttackBarricade(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (m_Actor.HasLeader && !DontFollowLeader) {
        Point position = m_Actor.Leader.Location.Position;
        bool isVisible = FOV.Contains(position);
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        tmpAction = BehaviorFollowActor(game, m_Actor.Leader, position, isVisible, maxDist);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      }
      if (!(m_Actor.HasLeader && !DontFollowLeader) && m_Actor.CountFollowers < game.Rules.ActorMaxFollowers(m_Actor)) {
        Percept target = FilterNearest(FilterNonEnemies(percepts1));
        if (target != null) {
          tmpAction = BehaviorLeadActor(game, target);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }
      if (m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(game, 3, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(50)) {
            if (target.IsSleeping)
              game.DoEmote(m_Actor, string.Format("patiently waits for {0} to wake up.", (object) target.Name));
            else if (FOV.Contains(target.Location.Position))
              game.DoEmote(m_Actor, string.Format("Hey {0}! Fucking move!", (object) target.Name));
            else
              game.DoEmote(m_Actor, string.Format("Where is that {0} retard?", (object) target.Name));
          }
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      tmpAction = BehaviorExplore(game, m_Exploration);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
