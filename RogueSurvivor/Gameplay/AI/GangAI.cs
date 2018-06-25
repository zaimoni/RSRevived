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
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

    private readonly MemorizedSensor m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly ExplorationData m_Exploration = new ExplorationData();

    public GangAI()
    {
    }

    public override bool UsesExplosives { get { return false; } }

    public override void OptimizeBeforeSaving()
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      return m_MemLOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }
    public override Dictionary<Point,Actor> friends_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).friends; } }
    public override Dictionary<Point,Actor> enemies_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).enemies; } }
    protected override void SensorsOwnedBy(Actor actor) { (m_MemLOSSensor.Sensor as LOSSensor).OwnedBy(actor); }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(Actor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (CanSee(target.Location)) return "Hey {0}! Fucking move!";
      else return "Where is that {0} retard?";
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      ClearMovePlan();
      BehaviorEquipBodyArmor();

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight()) {
          BehaviorUnequipLeftItem(game);
      }
      // end item juggling check

      List<Percept> percepts_all = FilterSameMap(UpdateSensors());

      m_Actor.IsRunning = false;    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (null != Order) {
        ActorAction actorAction = ExecuteOrder(game, Order, percepts_all);
        if (null != actorAction) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.Activity = Activity.IDLE; // backstop

      m_Exploration.Update(m_Actor.Location);

      // New objectives systems
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
            else return goal_action;
          }
        }
      }

      List<Percept> old_enemies = FilterEnemies(percepts_all);
      List<Percept> current_enemies = SortByGridDistance(FilterCurrent(old_enemies));

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      InitAICache(percepts_all);

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      if (null == current_enemies) AdviseFriendsOfSafety();

      List<Engine.Items.ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(available_ranged_weapons, current_enemies);
      if (null != tmpAction) return tmpAction;

      tmpAction = BehaviorEquipWeapon(game, available_ranged_weapons, current_enemies);
      if (null != tmpAction) return tmpAction;

	  List<Percept> friends = FilterNonEnemies(percepts_all);
      if (null != current_enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
          if (null != tmpAction) return tmpAction;
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, ActorCourage.COURAGEOUS, GangAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
      if (null != tmpAction) return tmpAction;

      if (null != old_enemies && !m_Actor.IsTired) {    // difference between gang and CHAR/soldier is ok here
        tmpAction = BehaviorChargeEnemy(FilterNearest(old_enemies), false, false);
        if (null != tmpAction) return tmpAction;
      }

      // handle food after enemies checks
      tmpAction = BehaviorEatProactively();
      if (null != tmpAction) return tmpAction;

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorEat();
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsStarving || m_Actor.IsInsane) {
          tmpAction = BehaviorGoEatCorpse(percepts_all);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }

      if (null == old_enemies && WantToSleepNow) {
        tmpAction = BehaviorNavigateToSleep();
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorDropUselessItem();
      if (null != tmpAction) return tmpAction;

      if (null == current_enemies) {
        Map map = m_Actor.Location.Map;
        List<Percept> interestingStacks = percepts_all.FilterT<Inventory>().FilterOut(p =>
        {
          if (p.Turn != map.LocalTime.TurnCounter) return true; // not in sight
          if (IsOccupiedByOther(p.Location)) return true; // blocked
          return null == BehaviorWouldGrabFromStack(p.Location, p.Percepted as Inventory);
        });
        if (interestingStacks != null) {
          Percept percept = FilterNearest(interestingStacks);
          tmpAction = BehaviorGrabFromStack(percept.Location, percept.Percepted as Inventory);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      if (null == current_enemies) {
        // rewriting this to work around a paradoxical bug indicating runtime state corruption
        var mayStealFrom = FilterCurrent(percepts_all).FilterT<Actor>(a =>
        {
          if ((a.Inventory?.IsEmpty ?? true) || IsFriendOf(a)) return false;
          if (!game.Rules.RollChance(Rules.ActorUnsuspicousChance(m_Actor, a))) return HasAnyInterestingItem(a.Inventory);
          game.DoEmote(a, string.Format("moves unnoticed by {0}.", (object)m_Actor.Name));
          return false;
        });
        if (null!= mayStealFrom) {
          // alpha10 make sure to consider only reachable victims
          // gangs can break & push stuff
          FilterOutUnreachablePercepts(ref mayStealFrom, RouteFinder.SpecialActions.ADJ_TO_DEST_IS_GOAL | RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.BREAK | RouteFinder.SpecialActions.PUSH);
          if (0 < mayStealFrom.Count) {
            Percept victimize = FilterNearest(mayStealFrom);
            Actor target = victimize.Percepted as Actor;
            Item obj = target.Inventory?.GetFirstMatching<Item>(it => IsInterestingItem(it));
            game.DoMakeAggression(m_Actor, target);
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = target;
            return new ActionSay(m_Actor, target, string.Format("Hey! That's some nice {0} you have here!", obj.Model.SingleName), RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER); // takes turn for game balance
          }
        }
      }

      tmpAction = BehaviorAttackBarricade();    // gang-specific
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
        var want_leader = friends.FilterT<Actor>(a => m_Actor.CanTakeLeadOf(a));
        FilterOutUnreachablePercepts(ref want_leader, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP);
        Percept target = FilterNearest(want_leader);
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
        tmpAction = BehaviorDontLeaveFollowersBehind(3, out Actor target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorExplore(m_Exploration);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
