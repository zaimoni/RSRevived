// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CivilianAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_SELECTACTION
// #define TIME_TURNS

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
#if TIME_TURNS
using System.Diagnostics;
#endif
using System.Drawing;
using System.Linq;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CivilianAI : OrderableAI
  {
    private static readonly string[] FIGHT_EMOTES = new string[MAX_EMOTES]
    {
      "Go away",
      "Damn it I'm trapped!",
      "I'm not afraid"
    };
    private static readonly string[] BIG_BEAR_EMOTES = new string[MAX_EMOTES]
    {
      "You fool",
      "I'm fooled!",
      "Be a man"
    };
    private static readonly string[] FAMU_FATARU_EMOTES = new string[MAX_EMOTES]
    {
      "Bakemono",
      "Nani!?",
      "Kawaii"
    };
    private static readonly string[] SANTAMAN_EMOTES = new string[MAX_EMOTES]
    {
      "DEM BLOODY KIDS!",
      "LEAVE ME ALONE I AIN'T HAVE NO PRESENTS!",
      "MERRY FUCKIN' CHRISTMAS"
    };
    private static readonly string[] ROGUEDJACK_EMOTES = new string[MAX_EMOTES]
    {
      "Sorry butt I am le busy,",
      "I should have redone ze AI rootines!",
      "Let me test le something on you"
    };
    private static readonly string[] DUCKMAN_EMOTES = new string[MAX_EMOTES]
    {
      "I'LL QUACK YOU BACK",
      "THIS IS MY FINAL QUACK",
      "I'M GONNA QUACK YOU"
    };
    private static readonly string[] HANS_VON_HANZ_EMOTES = new string[MAX_EMOTES]
    {
      "RAUS",
      "MEIN FUHRER!",
      "KOMM HIER BITE"
    };
    private const int LOS_MEMORY = 1;   // just enough memory to not walk into exploding grenades
    private const int FOLLOW_NPCLEADER_MAXDIST = 1;
    private const int FOLLOW_PLAYERLEADER_MAXDIST = 1;
    private const int USE_EXIT_CHANCE = 20;
    private const int BUILD_TRAP_CHANCE = 50;
    private const int BUILD_SMALL_FORT_CHANCE = 20;
    private const int BUILD_LARGE_FORT_CHANCE = 50;
    private const int START_FORT_LINE_CHANCE = 1;
    private const int TELL_FRIEND_ABOUT_RAID_CHANCE = 20;
    private const int TELL_FRIEND_ABOUT_ENEMY_CHANCE = 10;
    private const int TELL_FRIEND_ABOUT_ITEMS_CHANCE = 10;
    private const int TELL_FRIEND_ABOUT_SOLDIER_CHANCE = 20;
    private const int MIN_TURNS_SAFE_TO_SLEEP = 10;
    private const int USE_STENCH_KILLER_CHANCE = 75;
    private const int HUNGRY_CHARGE_EMOTE_CHANCE = 50;
    private const int HUNGRY_PUSH_OBJECTS_CHANCE = 25;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS | LOSSensor.SensingFilter.CORPSES;

    private readonly MemorizedSensor m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private int m_SafeTurns;
    private readonly ExplorationData m_Exploration = new ExplorationData();
    private string[] m_Emotes;

    public CivilianAI()
    {
      m_Emotes = CivilianAI.FIGHT_EMOTES;
    }

    // we don't have memory, but we do have taboo trades
    public override void OptimizeBeforeSaving()
    {
      if (null == TabooTrades) return;
      int i = TabooTrades.Count;
      while(0 < i--) {
        if (TabooTrades[i].IsDead) TabooTrades.RemoveAt(i);
      }
    }

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      if (!m_Actor.IsUnique) return;
      UniqueActors tmp = Session.Get.UniqueActors;
      m_Emotes = (m_Actor != tmp.BigBear.TheActor ? (m_Actor != tmp.FamuFataru.TheActor ? (m_Actor != tmp.Santaman.TheActor ? (m_Actor != tmp.Roguedjack.TheActor ? (m_Actor != tmp.Duckman.TheActor ? (m_Actor != tmp.HansVonHanz.TheActor ? CivilianAI.FIGHT_EMOTES : CivilianAI.HANS_VON_HANZ_EMOTES) : CivilianAI.DUCKMAN_EMOTES) : CivilianAI.ROGUEDJACK_EMOTES) : CivilianAI.SANTAMAN_EMOTES) : CivilianAI.FAMU_FATARU_EMOTES) : CivilianAI.BIG_BEAR_EMOTES);
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
      else if (CanSee(target.Location)) return "Come on {0}! Hurry up!";
      else return "Where the hell is {0}?";
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      ClearMovePlan();
      BehaviorEquipBodyArmor();

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight() && !BehaviorEquipStenchKiller(game)) {
        BehaviorUnequipLeftItem(game);
      }
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": BehaviorUnequipLeftItem " + timer.ElapsedMilliseconds.ToString()+"ms");
      timer.Restart();
#endif
      // end item juggling check
      List<Percept> percepts_all = FilterSameMap(UpdateSensors());
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": percepts_all " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif
      List<Percept> percepts1 = FilterCurrent(percepts_all);    // this tests fast
#if TIME_TURNS
      timer.Restart();
#endif
      ReviewItemRatings();  // XXX highly inefficient when called here; should "update on demand"
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": ReviewItemRatings " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+m_Actor.Location.Map.LocalTime.TurnCounter.ToString());
#endif
      m_Actor.IsRunning = false;    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (null != Order) {
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "under orders");
#endif
        ActorAction actorAction = ExecuteOrder(game, Order, percepts1);
        if (null != actorAction) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "implementing orders");
#endif
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.Activity = Activity.IDLE; // backstop

      m_Exploration.Update(m_Actor.Location);

      ExpireTaboos();

      // New objectives system
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, Objectives.Count.ToString()+" objectives");
#endif
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
#if TRACE_SELECTACTION
            else {
              if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "returning to task: "+o.ToString());
              return goal_action;
            }
#else
            else return goal_action;
#endif
          }
        }
      }

      List<Percept> enemies = SortByGridDistance(FilterEnemies(percepts1)); // this tests fast
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == enemies ? "null == enemies" : enemies.Count.ToString()+" enemies"));
#endif
      // civilians track how long since they've seen trouble
      if (null != enemies) m_SafeTurns = 0;
      else ++m_SafeTurns;

      if (null != enemies) m_LastEnemySaw = game.Rules.DiceRoller.Choose(enemies);

      if (!Directives.CanThrowGrenades && m_Actor.GetEquippedWeapon() is ItemGrenade grenade) game.DoUnequipItem(m_Actor, grenade);

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      InitAICache(percepts1, percepts_all);
      bool in_blast_field = _blast_field?.Contains(m_Actor.Location.Position) ?? false;

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      // get out of the range of explosions if feasible
      if (in_blast_field) {
        tmpAction = (_safe_run_retreat ? DecideMove(_legal_steps, _run_retreat) : ((null != _retreat) ? DecideMove(_retreat) : null));
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "fleeing explosives");
#endif
          if (tmpAction is ActionMoveStep) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
          return tmpAction;
        }
      }

      // if we have no enemies and have not fled an explosion, our friends can see that we're safe
      if (null == enemies) AdviseFriendsOfSafety();

      List<ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();
#if TIME_TURNS
        timer.Restart();
#endif

      tmpAction = ManageMeleeRisk(available_ranged_weapons, enemies);
#if TIME_TURNS
        timer.Stop();
        if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": ManageMeleeRisk " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "managing melee risk");
#endif
      if (null != tmpAction) return tmpAction;

      if (null != enemies && Directives.CanThrowGrenades) {
        tmpAction = BehaviorThrowGrenade(game, enemies);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "toss grenade");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game, available_ranged_weapons, enemies);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "probably reloading");
#endif
      if (null != tmpAction) return tmpAction;

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && CanSee(m_Actor.Leader.Location);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && m_Actor.Leader.IsAdjacentToEnemy;
      bool assistLeader = hasVisibleLeader && isLeaderFighting && !m_Actor.IsTired;

	  List<Percept> friends = FilterNonEnemies(percepts1);
      if (null != enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(enemies).Percepted as Actor);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "warning friends");
#endif
          if (null != tmpAction) return tmpAction;
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        tmpAction = BehaviorFightOrFlee(game, enemies, Directives.Courage, m_Emotes);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "having to fight w/o ranged weapons");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorUseMedecine ok"); // TRACER
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "medicating");
#endif
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorRestIfTired ok"); // TRACER
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resting");
#endif
      if (null != tmpAction) return tmpAction;

      if (null != enemies && assistLeader) {    // difference between civilian and CHAR/soldier is ok here
        tmpAction = BehaviorChargeEnemy(FilterNearest(enemies));
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "assisting leader in melee");
#endif
          return tmpAction;
        }
      }

      // handle food after enemies check
      tmpAction = BehaviorEatProactively();
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorEatProactively ok");
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "eating proactively");
#endif
      if (null != tmpAction) return tmpAction;

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (m_Actor.IsHungry ? "hungry" : "not hungry"));
#endif
      if (m_Actor.IsHungry) {
        tmpAction = BehaviorEat();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "eating -- hunger");
#endif
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsStarving || m_Actor.IsInsane) {
          tmpAction = BehaviorGoEatCorpse(percepts1);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "cannibalism");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }

      IEnumerable<Engine.MapObjects.PowerGenerator> generators_off = GeneratorsToTurnOn;    // reused much later
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == generators_off ? "no generators to turn on" : generators_off.Count().ToString()+" genberators to turn on"));
#endif
      if (null != generators_off) {
        foreach(Engine.MapObjects.PowerGenerator gen in generators_off) {   // these are never on map edges
          if (Rules.IsAdjacent(m_Actor.Location.Position,gen.Location.Position)) {
            return new ActionSwitchPowerGenerator(m_Actor, gen);
          }
        }
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "not adjacent to a generator");
#endif
      }


      tmpAction = BehaviorDropUselessItem();    // inventory normalization should normally be a no-op
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "ditching useless item");
#endif
      if (null != tmpAction) return tmpAction;

      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && Directives.CanSleep && WantToSleepNow) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorNavigateToSleep");
#endif
        tmpAction = BehaviorNavigateToSleep();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "navigating to sleep");
#endif
        if (null != tmpAction) return tmpAction;
      }

      if (m_Actor.Model.Abilities.HasSanity) {  // not logically civilian-specific, but needs a rework anyway
        if (m_Actor.Sanity < 3*m_Actor.MaxSanity/4) {
          tmpAction = BehaviorUseEntertainment();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using entertainment");
#endif
          if (null != tmpAction)  return tmpAction;
        }
      }

      // XXX this should lose to same-map threat hunting at close ETA
      tmpAction = InventoryStackTactics();
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "inventory management failsafe triggered");
#endif
      if (null != tmpAction) return tmpAction;

      // XXX this should lose to same-map threat hunting at close ETA
      if (null == enemies && Directives.CanTakeItems) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for items to take");
#endif
        Map map = m_Actor.Location.Map;
        bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == Directives.Courage;
        // following needs to be more sophisticated.
        // 1) identify all stacks, period.
        // 2) categorize stacks by whether they are personally interesting or not.
        // 3) the personally interesting ones get evaluated here. 
        // 4) in-communication followers will be consulted regarding the not-interesting stacks
        var examineStacks = new List<Percept>(percepts1?.Count ?? 0);
        if (null != percepts1) {
          var boringStacks = new List<Percept>(percepts1.Count);
          foreach(Percept p in percepts1) {
            if (!(p.Percepted is Inventory inv)) continue;
            if (m_Actor.StackIsBlocked(p.Location, out MapObject mapObjectAt)) continue; // XXX ignore items under barricades or fortifications
            if (!BehaviorWouldGrabFromStack(p.Location, p.Percepted as Inventory)?.IsLegal() ?? true) {
              boringStacks.Add(p);
              continue;
            }
            if (p.Turn != map.LocalTime.TurnCounter) continue;    // not in sight
            examineStacks.Add(p);
          }
          if (0 < boringStacks.Count) AdviseCellOfInventoryStacks(boringStacks);    // XXX \todo PC leader should do the same
        }
        List<Percept> interestingStacks = examineStacks.FilterT<Inventory>().FilterOut(p => {
          if (IsOccupiedByOther(p.Location)) return true; // blocked
          if (!m_Actor.MayTakeFromStackAt(p.Location)) {    // something wrong, e.g. iron gates in way
            if (!imStarvingOrCourageous && map.TrapsMaxDamageAt(p.Location.Position) >= m_Actor.HitPoints) return true;  // destination deathtrapped
            // check for iron gates, etc in way
            List<List<Point> > path = m_Actor.MinStepPathTo(m_Actor.Location, p.Location);
            if (null == path) return true;
            List<Point> test = path[0].Where(pt => null != Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, pt))).ToList();
            if (0 >= test.Count) return true;
            path[0] = test;
            if (!imStarvingOrCourageous && path[0].Any(pt=> map.TrapsMaxDamageAt(pt) >= m_Actor.HitPoints)) return true;
          }
          return false;
        });
        if (interestingStacks != null) {
          var at_target = interestingStacks.FirstOrDefault(p => m_Actor.MayTakeFromStackAt(p.Location));
          if (null != at_target) {
            m_LastItemsSaw = at_target;
            tmpAction = (m_Actor.Controller as OrderableAI).BehaviorGrabFromAccessibleStack(at_target.Location, at_target.Percepted as Inventory);
            if (tmpAction?.IsLegal() ?? false) {
              m_Actor.Activity = Activity.IDLE;
              return tmpAction;
            }
            // invariant failure
#if DEBUG
            throw new InvalidOperationException("Prescreen for avoidng taboo tile marking failed: "+tmpAction.to_s());
#endif
          }

          // no accessible interesting stacks.  Memorize them just in case.
          {
          var track_inv = Objectives.FirstOrDefault(o => o is Goal_PathToStack) as Goal_PathToStack;
          foreach(Percept p in interestingStacks) {
            if (null == track_inv) {
              track_inv = new Goal_PathToStack(m_Actor.Location.Map.LocalTime.TurnCounter,m_Actor,p.Location);
              Objectives.Add(track_inv);
            } else track_inv.newStack(p.Location);
          }
          }

          Percept percept = FilterNearest(interestingStacks);
          m_LastItemsSaw = percept;
          tmpAction = BehaviorGrabFromStack(percept.Location, percept.Percepted as Inventory);
          if (tmpAction?.IsLegal() ?? false) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "taking from stack");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
          // XXX the main valid way this could fail, is a stack behind a non-walkable, etc., object that isn't a container
          // could happen in normal play in the sewers
          // under is handled within the Behavior functions
#if TRACE_SELECTACTION
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+"has abandoned getting the items at "+ percept.Location.Position);
#endif
#if DEBUG
          ActorAction failed = BehaviorWouldGrabFromStack(percept.Location, percept.Percepted as Inventory);
          throw new InvalidOperationException("Prescreen for avoidng taboo tile marking failed: "+failed.ToString()+" "+failed.IsLegal().ToString());
#endif
        }
        {   // leadership or trading requests
        Goal_HintPathToActor remote = Objectives.FirstOrDefault(o => o is Goal_HintPathToActor) as Goal_HintPathToActor;
        if (null != remote) {
          tmpAction = remote.Pathing();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "remote.Pathing(): "+tmpAction.to_s());
#endif
          if (null != tmpAction) return tmpAction;
        }
        }
        if (Directives.CanTrade) {
          tmpAction = BehaviorFindTrade(friends);
          if (null != tmpAction) return tmpAction;
        }
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "have checked for items to take");
#endif
        {
        Goal_PathToStack remote = Objectives.FirstOrDefault(o => o is Goal_PathToStack) as Goal_PathToStack;
        if (null != remote) {
          tmpAction = remote.Pathing();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "remote.Pathing(): "+tmpAction.to_s());
#endif
          if (null != tmpAction) return tmpAction;
        }
        }
      } // null == enemies && Directives.CanTakeItems

      // attempting extortion from cops should have consequences.
      // XXX as should doing it to a civilian whose leader is a cop (and in communication)
      if (   RogueGame.Options.IsAggressiveHungryCiviliansOn
          && percepts1 != null
          && !m_Actor.HasLeader
          && !m_Actor.Model.Abilities.IsLawEnforcer
          && (m_Actor.IsHungry
          && !m_Actor.Has<ItemFood>())) {
        Percept target = FilterNearest(percepts1.FilterT<Actor>(a =>
        {
          if (a == m_Actor || a.IsDead || (a.Inventory == null || a.Inventory.IsEmpty) || (a.Leader == m_Actor || m_Actor.Leader == a))
            return false;
          if (a.Inventory.Has<ItemFood>()) return true;
          return a.Location.Items?.Has<ItemFood>() ?? false;
        }));
        if (target != null) {
          tmpAction = BehaviorChargeEnemy(target);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "starving, attacking for food");
#endif
            if (game.Rules.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              RogueGame.DoSay(m_Actor, target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION);
            if (!m_Actor.TargetActor.IsSleeping) {
              if (m_Actor.TargetActor.Faction.ID.ExtortionIsAggression()) {
                game.DoMakeAggression(m_Actor,m_Actor.TargetActor);
              } else if (m_Actor.TargetActor.Faction.ID.LawIgnoresExtortion()) {
                game.DoMakeAggression(m_Actor.TargetActor,m_Actor);
              } // XXX the target needs an AI modifier to handle this appropriately
            }
            return tmpAction;
          }
        }
      }
      if (game.Rules.RollChance(USE_STENCH_KILLER_CHANCE)) {    // civilian-specific
        tmpAction = BehaviorUseStenchKiller();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using stench killer");
#endif
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorCloseDoorBehindMe(PrevLocation);    // civilian-specific
      if (null != tmpAction) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "closing door");
#endif
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      // XXX if we have item memory, check whether "critical items" have a known location.  If so, head for them (floodfill pathfinding)
      // XXX leaders should try to check what their followers use as well.
      List<Gameplay.GameItems.IDs> items = WhatHaveISeen();
      if (null != items) {
        HashSet<Gameplay.GameItems.IDs> critical = WhatDoINeedNow();    // out of ammo, or hungry without food
        // while we want to account for what our followers want, we don't want to block our followers from the items either
        critical.IntersectWith(items);
        if (0 < critical.Count) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorResupply");
#endif
          tmpAction = BehaviorResupply(critical);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorResupply ok");
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resupplying: "+tmpAction.ToString());
#endif
          if (null != tmpAction) return tmpAction;
        }
      }

      if (m_Actor.HasLeader && !DontFollowLeader) {
        // \todo interposition target for pathing hints, etc. from leader
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorFollowActor");
#endif
        tmpAction = BehaviorFollowActor(m_Actor.Leader, maxDist);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorFollowActor: "+(tmpAction?.ToString() ?? "null"));
#endif
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      } else if (m_Actor.CountFollowers < m_Actor.MaxFollowers) {
        Percept target = FilterNearest(friends);
        if (target != null) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorLeadActor");
#endif
          tmpAction = BehaviorLeadActor(target);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorLeadActor: " + (tmpAction?.ToString() ?? "null"));
#endif
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "taking lead");
#endif
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }
      // XXX if we are a leader, we should try to rearrange items for our followers (no one starving while another has a lot of food)
      // XXX if we are a follower, we should try to avoid being hurt by the leader's rearranging our items

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorAttackBarricade();
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for food behind barricade");
#endif
          game.DoEmote(m_Actor, "Open damn it! I know there is food there!");
          return tmpAction;
        }
        if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE)) {
          tmpAction = BehaviorPushNonWalkableObjectForFood();
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for food behind non-walkable objects");
#endif
            game.DoEmote(m_Actor, "Where is all the damn food?!");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      tmpAction = BehaviorGoReviveCorpse(percepts1);  // not logically CivilianAI only
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "revive corpse");
#endif
        return tmpAction;
      }
      if (!HaveThreatsInCurrentMap() && !HaveTourismInCurrentMap()) {
        if (game.Rules.RollChance(USE_EXIT_CHANCE)) {
          tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.DONT_BACKTRACK);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "use exit for no good reason");
#endif
          if (null != tmpAction) return tmpAction;
        }
      }

      Percept percept1 = percepts1.FilterFirst(p =>
      {
        Actor actor = p.Percepted as Actor;
        if (actor == null || actor == m_Actor) return false;
        return actor.Controller is SoldierAI;
      });
      if (percept1 != null) m_LastSoldierSaw = percept1;

      if (m_Actor.Model.Abilities.IsLawEnforcer) {
        tmpAction = BehaviorEnforceLaw();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law");
#endif
        if (null != tmpAction) return tmpAction;
      }

      // XXX civilians that start in a boarded-up building (sewer maintenance, gun shop, hardware store
      // should stay there until they get the all-clear from the police

      // The newer movement behaviors using floodfill pathing, etc. depend on there being legal walking moves
#region floodfill pathfinder
      if (null != _legal_steps) {
        // advanced pathing ultimately reduces to various flavors of calls to (specializations) of 
        // public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering advanced pathing");
#endif
        HashSet<GameItems.IDs> combat_critical = WhatDoINeedNow();
        combat_critical.IntersectWith(GameItems.ammo);
        HashSet<Gameplay.GameItems.IDs> want = (null != items ? WhatDoIWantNow() : new HashSet<Gameplay.GameItems.IDs>());    // non-emergency things
        // while we want to account for what our followers want, we don't want to block our followers from the items either
        if (null != items) want.IntersectWith(items);
        bool early_hunt_threat_other_maps = (m_Actor.Location.Map == m_Actor.Location.Map.District.EntryMap && 0 >= want.Count);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "critical: "+combat_critical.to_s());
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "want: "+want.to_s());
#endif

#if PROTOTYPE
        Func<Map,HashSet<Point>> pathing_targets = null;
        ThreatTracking threats = m_Actor.Threats;
        HashSet<Point> hunt_threat(Map m) {
          if (m == m.District.SewersMap && Session.Get.HasZombiesInSewers) return new HashSet<Point>();
          return threats.ThreatWhere(m);
        }

        if (0 >= combat_critical.Count && null != threats && threats.Any()) pathing_targets = hunt_threat;

        LocationSet sights_to_see = m_Actor.InterestingLocs;
        HashSet<Point> tourism(Map m) {
          return sights_to_see.In(m);
        }
        if (null != sights_to_see) pathing_targets = pathing_targets.Union(tourism);

        HashSet<Point> generators(Map m) {
          if (Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap==m) return new HashSet<Point>();  // plot-sensitive; if recharging there's a much closer one to the surface
          if (WantToRecharge()) return new HashSet<Point>(m.PowerGenerators.Get.Select(obj => obj.Location.Position));
          if (generators_off?.Any() ?? false) return new HashSet<Point>(generators_off.Select(obj => obj.Location.Position));
          return new HashSet<Point>();
        }

        if (HasBehaviorThatRecallsToSurface) pathing_targets = pathing_targets.Union(generators);

        HashSet<Point> resupply_want(Map m)
        {
          return WhereIs(want,m);
        }

        if (0 < want.Count) pathing_targets = pathing_targets.Union(resupply_want);
        if (0 < combat_critical.Count && null != threats && threats.Any()) pathing_targets = pathing_targets.Otherwise(hunt_threat);
        if (null != pathing_targets) {
          tmpAction = BehaviorPathTo(pathing_targets);
          if (null!=tmpAction) return tmpAction;
        }
#else
        if (0 >= combat_critical.Count) {
          // hunt down threats -- works for police
#if TIME_TURNS
         timer.Restart();
#endif
#if TRACE_SELECTACTION
         if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "ammo not critically low");
#endif
         tmpAction = BehaviorHuntDownThreatCurrentMap();
#if TIME_TURNS
         timer.Stop();
         if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": BehaviorHuntDownThreatCurrentMap " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif

#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, current map: "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null != tmpAction) return tmpAction;

          // hunt down threats -- works for police
          if (early_hunt_threat_other_maps) {
#if TIME_TURNS
         timer.Restart();
#endif
            tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TIME_TURNS
         timer.Stop();
         if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": BehaviorHuntDownThreatOtherMaps " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- on surface");
#endif
            if (null != tmpAction) return tmpAction;
          }
        }

        // tourism -- works for police
        tmpAction = BehaviorTourismCurrentMap();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "tourism, current map: "+(tmpAction?.ToString() ?? "null"));
#endif
        if (null != tmpAction) return tmpAction;

        if (generators_off?.Any() ?? false) {
          tmpAction = BehaviorHastyNavigate(new HashSet<Point>(generators_off.Select(gen => gen.Location.Position)));
          if (null != tmpAction) return tmpAction;
        }

        if (HasBehaviorThatRecallsToSurface && m_Actor.Location.Map.District.HasAccessiblePowerGenerators) {
          if (WantToRecharge()) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering recharge");
#endif
            FloodfillPathfinder<Point> navigate = PathfinderFor(m => new HashSet<Point>(m.PowerGenerators.Get.Select(obj => obj.Location.Position)));
            tmpAction = BehaviorPathTo(navigate);
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering recharge: "+(tmpAction?.ToString() ?? "null"));
#endif
            if (null != tmpAction) return tmpAction;
          }
#if FAIL
          if (WantToRechargeAtDawn()) {
            FloodfillPathfinder<Point> navigate = PathfinderFor(m => new HashSet<Point>(m.PowerGenerators.Get.Select(obj => obj.Location.Position));
            if (navigate.Cost(m_Actor.Location.Position) <= ...) {
              tmpAction = BehaviorPathTo(navigate);
              if (null != tmpAction) return tmpAction;
            }
          }
#endif
        }

        if (0 < want.Count) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorResupply (want)");
#endif
            tmpAction = BehaviorResupply(want);
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorResupply ok: "+(tmpAction?.ToString() ?? "null"));
#endif
            if (null != tmpAction) return tmpAction;
        }

        if (0 >= combat_critical.Count && !early_hunt_threat_other_maps) {
          // hunt down threats -- works for police
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorHuntDownThreatOtherMaps");
#endif
            tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- not on surface: "+(tmpAction?.ToString() ?? "null"));
#endif
            if (null != tmpAction) return tmpAction;
        }

        // tourism -- works for police
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorTourismOtherMaps");
#endif
        tmpAction = BehaviorTourismOtherMaps();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "tourism, other map: "+(tmpAction?.ToString() ?? "null"));
#endif
        if (null != tmpAction) return tmpAction;
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "advanced pathing failed");
#endif

        // if we cannot do anyting constructive, hunt down threat even if critical shortage
        if (0 < combat_critical.Count) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat even though unprepared");
#endif
          // hunt down threats -- works for police
          tmpAction = BehaviorHuntDownThreatCurrentMap();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, current map: "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null != tmpAction) return tmpAction;

          // hunt down threats -- works for police
          tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- cannot prepare: " + (tmpAction?.ToString() ?? "null"));
#endif
          if (null != tmpAction) return tmpAction;
        }
#endif
      }
#endregion

      if (game.Rules.RollChance(BUILD_TRAP_CHANCE)) {
        tmpAction = BehaviorBuildTrap(game);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build trap");
#endif
        if (null != tmpAction) return tmpAction;
      }

      if (game.Rules.RollChance(BUILD_LARGE_FORT_CHANCE)) { // difference in relative ordering with soldiers is ok
        tmpAction = BehaviorBuildLargeFortification(game, 1);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build large fortification");
#endif
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(BUILD_SMALL_FORT_CHANCE)) {
        tmpAction = BehaviorBuildSmallFortification(game);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build small fortification");
#endif
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

	  if (null != friends) {
        if (m_LastRaidHeard != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_RAID_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastRaidHeard);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about raid");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastSoldierSaw != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_SOLDIER_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about soldier");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastEnemySaw != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_ENEMY_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about enemy");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastItemsSaw != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_ITEMS_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about items");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
	  }

      if (m_Actor.CountFollowers > 0) {
        tmpAction = BehaviorDontLeaveFollowersBehind(2, out Actor target);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "dont't leave followers");
#endif
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      tmpAction = BehaviorExplore(m_Exploration);
      if (null != tmpAction) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "unguided exploration");
#endif
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "wandering");
#endif
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
