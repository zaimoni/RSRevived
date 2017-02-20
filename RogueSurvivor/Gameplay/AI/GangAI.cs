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
using System.Diagnostics.Contracts;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

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

    private readonly MemorizedSensor m_MemorizedSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly ExplorationData m_Exploration = new ExplorationData();

    public GangAI()
    {
    }

    public override void OptimizeBeforeSaving()
    {
      m_MemorizedSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      return m_MemorizedSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemorizedSensor.Sensor as LOSSensor).FOV; } }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(Actor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (FOV.Contains(target.Location.Position)) return "Hey {0}! Fucking move!";
      else return "Where is that {0} retard?";
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());
      
      BehaviorEquipBodyArmor(game);

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight(game)) {
          BehaviorUnequipLeftItem(game);
      }
      // end item juggling check

      List<Percept> percepts1 = FilterSameMap(UpdateSensors());

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

      List<Percept> old_enemies = FilterEnemies(percepts1);
      List<Percept> current_enemies = SortByGridDistance(FilterCurrent(old_enemies));

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      Dictionary<Point,int> damage_field = new Dictionary<Point, int>();
      List<Actor> slow_melee_threat = new List<Actor>();
      HashSet<Actor> immediate_threat = new HashSet<Actor>();
      if (null != current_enemies) VisibleMaximumDamage(damage_field, slow_melee_threat, immediate_threat);
      AddTrapsToDamageField(damage_field, percepts1);
      if (0>=damage_field.Count) damage_field = null;
      if (0>= slow_melee_threat.Count) slow_melee_threat = null;
      if (0>= immediate_threat.Count) immediate_threat = null;

      List<Point> retreat = null;
      List<Point> run_retreat = null;
      bool safe_retreat = false;
      bool safe_run_retreat = false;
      // calculate retreat destinations if possibly needed
      if (null != damage_field && null!=legal_steps && damage_field.ContainsKey(m_Actor.Location.Position)) {
        retreat = FindRetreat(damage_field, legal_steps);
        if (null != retreat) {
          AvoidBeingCornered(retreat);
          safe_retreat = !damage_field.ContainsKey(retreat[0]);
        }
        if (m_Actor.RunIsFreeMove && m_Actor.CanRun() && !safe_retreat) { 
          run_retreat = FindRunRetreat(damage_field, legal_steps);
          if (null != run_retreat) {
            AvoidBeingRunCornered(run_retreat);
            safe_run_retreat = !damage_field.ContainsKey(run_retreat[0]);
          }
        }
      }

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

	  List<Percept> friends = FilterNonEnemies(percepts1);

      List<Engine.Items.ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      if ((null != retreat || null != run_retreat) && null != available_ranged_weapons && null != current_enemies) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(retreat, current_enemies);
        MaximizeRangedTargets(run_retreat, current_enemies);

        // ranged weapon: fast retreat ok
        // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
        // XXX we also want to be close enough to fire at all
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat, current_enemies, friends) : ((null != retreat) ? DecideMove(retreat, current_enemies, friends) : null));
        if (null != tmpAction) {
		  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
          if (null != tmpAction2) {
            if (safe_run_retreat) RunIfPossible();
            else RunIfAdvisable(tmpAction2.dest.Position);
          }
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // need stamina to melee: slow retreat ok
      if (null != retreat && WillTireAfterAttack(m_Actor)) {
	    tmpAction = DecideMove(retreat, current_enemies, friends);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // have slow enemies nearby
      if (null != retreat && null != slow_melee_threat) {
	    tmpAction = DecideMove(retreat, current_enemies, friends);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // end melee risk management check

      tmpAction = BehaviorEquipWeapon(game, legal_steps, damage_field, available_ranged_weapons, current_enemies, friends, immediate_threat);
      if (null != tmpAction) return tmpAction;

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && FOV.Contains(m_Actor.Leader.Location.Position);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && m_Actor.Leader.IsAdjacentToEnemy;

      if (null != current_enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
          if (null != tmpAction) return tmpAction;
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, damage_field, hasVisibleLeader, isLeaderFighting, ActorCourage.COURAGEOUS, GangAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
      if (null != tmpAction) return tmpAction;

      if (null != old_enemies && !m_Actor.IsTired) {    // difference between gang and CHAR/soldier is ok here
        Percept target = FilterNearest(old_enemies);
        tmpAction = BehaviorChargeEnemy(target);
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
        tmpAction = BehaviorEat();
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsStarving || m_Actor.IsInsane) {
          tmpAction = BehaviorGoEatCorpse(percepts1);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }

      // the new objectives system should trigger after all enemies-handling behavior
      if (0<Objectives.Count) {
        ActorAction goal_action = null;
        foreach(Objective o in new List<Objective>(Objectives)) {
          if (o.IsExpired) Objectives.Remove(o);
          else if (o.UrgentAction(out goal_action)) {
            if (null==goal_action) Objectives.Remove(o);
#if DEBUG
            else if (!goal_action.IsLegal()) throw new InvalidOperationException("result of UrgentAction should be legal");
#else
            else if (!goal_action.IsLegal()) Objectives.Remove(o);
#endif
          }
        }
      }

      if (null == old_enemies && OkToSleepNow) {
        tmpAction = BehaviorSecurePerimeter();
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
      tmpAction = BehaviorDropUselessItem();
      if (null != tmpAction) return tmpAction;

      if (null == current_enemies) {
        Map map = m_Actor.Location.Map;
        List<Percept> percepts3 = percepts1.FilterT<Inventory>().FilterOut(p =>
        {
          if (p.Turn != map.LocalTime.TurnCounter) return true; // not in sight
          if (IsOccupiedByOther(map, p.Location.Position)) return true; // blocked
          return null == BehaviorWouldGrabFromStack(game, p.Location.Position, p.Percepted as Inventory);
        });
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
        Percept victimize = FilterNearest(FilterCurrent(percepts1).FilterT<Actor>(a =>
        {
          if (a.Inventory == null || a.Inventory.CountItems == 0 || IsFriendOf(a)) return false;
          if (!game.Rules.RollChance(game.Rules.ActorUnsuspicousChance(m_Actor, a))) return HasAnyInterestingItem(a.Inventory);
          game.DoEmote(a, string.Format("moves unnoticed by {0}.", (object)m_Actor.Name));
          return false;
        }));
        if (null!=victimize) {
          Actor target = victimize.Percepted as Actor;
          Item obj = FirstInterestingItem(target.Inventory);
          game.DoMakeAggression(m_Actor, target);
          m_Actor.Activity = Activity.CHASING;
          m_Actor.TargetActor = target;
          return new ActionSay(m_Actor, target, string.Format("Hey! That's some nice {0} you have here!", (object) obj.Model.SingleName), RogueGame.Sayflags.IS_IMPORTANT); // takes turn for game balance
        }
      }

      tmpAction = BehaviorAttackBarricade(game);    // gang-specific
      if (null != tmpAction) return tmpAction;

      if (m_Actor.HasLeader && !DontFollowLeader) {
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        tmpAction = BehaviorFollowActor(m_Actor.Leader, maxDist);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      } else if (m_Actor.CountFollowers < m_Actor.MaxFollowers) {
        Percept target = FilterNearest(friends);
        if (target != null) {
          tmpAction = BehaviorLeadActor(target);
          if (null != tmpAction) {
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }

      // critical item memory check goes here

      if (m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(3, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorExplore(game, m_Exploration, Directives.Courage);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
