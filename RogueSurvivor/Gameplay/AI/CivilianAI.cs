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

#if TIME_TURNS
      Stopwatch timer = Stopwatch.StartNew();   // above here tests negligible
#endif

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
      timer.Restart();
#endif
      List<Percept> percepts1 = FilterCurrent(percepts_all);
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": percepts1 " + timer.ElapsedMilliseconds.ToString()+"ms");
      timer.Restart();
#endif
      ReviewItemRatings();  // XXX highly inefficient when called here; should "update on demand"
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": SelectAction prologue " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+m_Actor.Location.Map.LocalTime.TurnCounter.ToString());
#endif

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
      m_Actor.IsRunning = false;
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

      List<Percept> enemies = SortByGridDistance(FilterEnemies(percepts1));
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == enemies ? "null == enemies" : enemies.Count.ToString()+" enemies"));
#endif
      // civilians track how long since they've seen trouble
      if (null != enemies) m_SafeTurns = 0;
      else ++m_SafeTurns;

      if (null != enemies) m_LastEnemySaw = enemies[game.Rules.Roll(0, enemies.Count)];

      if (!Directives.CanThrowGrenades && m_Actor.GetEquippedWeapon() is ItemGrenade grenade) game.DoUnequipItem(m_Actor, grenade);

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      List<Point> legal_steps = m_Actor.LegalSteps;
      Dictionary<Point,int> damage_field = new Dictionary<Point, int>();
      List<Actor> slow_melee_threat = new List<Actor>();
      HashSet<Actor> immediate_threat = new HashSet<Actor>();
      if (null != enemies) VisibleMaximumDamage(damage_field, slow_melee_threat, immediate_threat);
      AddTrapsToDamageField(damage_field, percepts1);
      bool in_blast_field = AddExplosivesToDamageField(damage_field, percepts_all);  // only civilians and soldiers respect explosives; CHAR and gang don't
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

      // get out of the range of explosions if feasible
      if (in_blast_field) {
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat) : ((null != retreat) ? DecideMove(retreat) : null));
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
      if (null == enemies) {
        Dictionary<Actor,ThreatTracking> observers = new Dictionary<Actor, ThreatTracking>();
        if (null != friends) {
          foreach(Percept fr in friends) {
            Actor friend = fr.Percepted as Actor;
            ThreatTracking ally_threat = friend.Threats;
            if (null == ally_threat || m_Actor.Threats == ally_threat) continue;
            if (!InCommunicationWith(friend)) continue;
            observers[friend] = ally_threat;
          }
        }
        HashSet<Actor> allies = m_Actor.Allies; // XXX thrashes garbage collector, possibly should be handled by LoS sensor for the leader only?
        if (null != allies) {
          foreach(Actor friend in allies) {
            ThreatTracking ally_threat = friend.Threats;
            if (null == ally_threat || m_Actor.Threats == ally_threat) continue;
            if (!InCommunicationWith(friend)) continue;
            observers[friend] = ally_threat;
          }
        }
        // but this won't trigger if any of our friends are mutual enemies
        if (0<observers.Count) {
          foreach(KeyValuePair<Actor,ThreatTracking> wary in observers) {
            if (!wary.Key.AnyEnemiesInFov(FOV)) wary.Value.Cleared(m_Actor.Location.Map,FOV);
          }
        }
      }

      List<ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(legal_steps, retreat, run_retreat, safe_run_retreat, available_ranged_weapons, enemies, slow_melee_threat);
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

      tmpAction = BehaviorEquipWeapon(game, legal_steps, available_ranged_weapons, enemies, immediate_threat);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "probably reloading");
#endif
      if (null != tmpAction) return tmpAction;

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && CanSee(m_Actor.Leader.Location);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && m_Actor.Leader.IsAdjacentToEnemy;
      bool assistLeader = hasVisibleLeader && isLeaderFighting && !m_Actor.IsTired;

      if (null != enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(enemies).Percepted as Actor);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "warning friends");
#endif
          if (null != tmpAction) return tmpAction;
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        tmpAction = BehaviorFightOrFlee(game, enemies, damage_field, Directives.Courage, m_Emotes);
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
        List<Percept> perceptList2 = percepts1.FilterT<Inventory>().FilterOut(p =>
        {
          if (p.Turn != map.LocalTime.TurnCounter) return true; // not in sight
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
          return !BehaviorWouldGrabFromStack(p.Location, p.Percepted as Inventory)?.IsLegal() ?? true;
        });
        if (perceptList2 != null) {
          Percept percept = FilterNearest(perceptList2);
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
        if (Directives.CanTrade) {
          tmpAction = BehaviorFindTrade(friends);
          if (null != tmpAction) return tmpAction;
        }
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "have checked for items to take");
#endif
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
        tmpAction = BehaviorFollowActor(m_Actor.Leader, maxDist);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "following leader");
#endif
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      } else if (m_Actor.CountFollowers < m_Actor.MaxFollowers) {
        Percept target = FilterNearest(friends);
        if (target != null) {
          tmpAction = BehaviorLeadActor(target);
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

	  if (null != friends) {
        if (m_Actor.Model.Abilities.IsLawEnforcer) {
          tmpAction = BehaviorEnforceLaw(game, friends);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law");
#endif
          if (null != tmpAction) return tmpAction;
        }
	  }

      // XXX civilians that start in a boarded-up building (sewer maintenance, gun shop, hardware store
      // should stay there until they get the all-clear from the police

      // The newer movement behaviors using floodfill pathing, etc. depend on there being legal walking moves
#region floodfill pathfinder
      if (null!=legal_steps) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering advanced pathing");
#endif
        HashSet<GameItems.IDs> critical = WhatDoINeedNow();
        critical.IntersectWith(GameItems.ammo);
        if (0 >= critical.Count) {
          // hunt down threats -- works for police
#if TIME_TURNS
         timer.Restart();
#endif
         tmpAction = BehaviorHuntDownThreatCurrentMap();
#if TIME_TURNS
         timer.Stop();
         if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": BehaviorHuntDownThreatCurrentMap " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif

#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, current map");
#endif
          if (null != tmpAction) return tmpAction;

          // hunt down threats -- works for police
          if (m_Actor.Location.Map==m_Actor.Location.Map.District.EntryMap) {
            tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- on surface");
#endif
            if (null != tmpAction) return tmpAction;
          }
        }

        // tourism -- works for police
        tmpAction = BehaviorTourismCurrentMap();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "tourism, current map");
#endif
        if (null != tmpAction) return tmpAction;

        if (generators_off?.Any() ?? false) {
          tmpAction = BehaviorHastyNavigate(new HashSet<Point>(generators_off.Select(gen => gen.Location.Position)));
          if (null != tmpAction) return tmpAction;
        }

        if (HasBehaviorThatRecallsToSurface && m_Actor.Location.Map.District.HasAccessiblePowerGenerators) {
          if (WantToRecharge()) {
            FloodfillPathfinder<Point> navigate = PathfinderFor(m => new HashSet<Point>(m.PowerGenerators.Get.Select(obj => obj.Location.Position)));
            tmpAction = BehaviorPathTo(navigate);
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

        if (null != items) {
          HashSet<Gameplay.GameItems.IDs> want = WhatDoIWantNow();    // non-emergency things
          // while we want to account for what our followers want, we don't want to block our followers from the items either
          want.IntersectWith(items);
          if (0 < want.Count) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorResupply (want)");
#endif
            tmpAction = BehaviorResupply(want);
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorResupply ok");
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resupplying: "+tmpAction.ToString());
#endif
            if (null != tmpAction) return tmpAction;
          }
        }

        if (0 >= critical.Count) {
          // hunt down threats -- works for police
          if (m_Actor.Location.Map!=m_Actor.Location.Map.District.EntryMap) {
            tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- not on surface");
#endif
            if (null != tmpAction) return tmpAction;
          }
        }

        // tourism -- works for police
        tmpAction = BehaviorTourismOtherMaps();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "tourism, other map");
#endif
        if (null != tmpAction) return tmpAction;
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "advanced pathing failed");
#endif

        // if we cannot do anyting constructive, hunt down threat even if critical shortage
        if (0 < critical.Count) {
          // hunt down threats -- works for police
          tmpAction = BehaviorHuntDownThreatCurrentMap();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, current map");
#endif
          if (null != tmpAction) return tmpAction;

          // hunt down threats -- works for police
          tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- cannot prepare");
#endif
          if (null != tmpAction) return tmpAction;
        }
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
