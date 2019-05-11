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
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
#if TIME_TURNS
using System.Diagnostics;
#endif
using System.Linq;
using Zaimoni.Data;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

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
      ReviewItemRatings();  // XXX \todo should be in ObjectiveAI override
      if (!m_Actor.IsUnique) return;
      UniqueActors tmp = Session.Get.UniqueActors;
      m_Emotes = (m_Actor != tmp.BigBear.TheActor ? (m_Actor != tmp.FamuFataru.TheActor ? (m_Actor != tmp.Santaman.TheActor ? (m_Actor != tmp.Roguedjack.TheActor ? (m_Actor != tmp.Duckman.TheActor ? (m_Actor != tmp.HansVonHanz.TheActor ? CivilianAI.FIGHT_EMOTES : CivilianAI.HANS_VON_HANZ_EMOTES) : CivilianAI.DUCKMAN_EMOTES) : CivilianAI.ROGUEDJACK_EMOTES) : CivilianAI.SANTAMAN_EMOTES) : CivilianAI.FAMU_FATARU_EMOTES) : CivilianAI.BIG_BEAR_EMOTES);
    }

    public override List<Percept> UpdateSensors()
    {
      return m_MemLOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }
    public override Dictionary<Location, Actor> friends_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).friends; } }
    public override Dictionary<Location, Actor> enemies_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).enemies; } }
    public override Dictionary<Location, Inventory> items_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).items; } }
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
      // \todo start extraction target: BehaviorEquipBestItems (cf RS Alpha 10)
      BehaviorEquipBestBodyArmor();

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight() && !BehaviorEquipStenchKiller(game)) {
        BehaviorUnequipLeftItem(game);
      }
      // end extraction target: BehaviorEquipBestItems
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": BehaviorUnequipLeftItem " + timer.ElapsedMilliseconds.ToString()+"ms");
      timer.Restart();
#endif
      // end item juggling check
      _all = FilterSameMap(UpdateSensors());
#if TIME_TURNS
      timer.Stop();
      if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": percepts_all " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif
      List<Percept> current = FilterCurrent(_all);    // this tests fast
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
      m_Actor.Walk();    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (null != Order) {
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "under orders");
#endif
        ActorAction actorAction = ExecuteOrder(game, Order, current);
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

      if (m_Actor.Location!=PrevLocation) {
        // sewers are not a good choice for default-tourism
        if (null!=ItemMemory && null!=m_Actor.InterestingLocs && m_Actor.Location.Map!= m_Actor.Location.Map.District.SewersMap) {
          var _items = ItemMemory;
          var tourism = m_Actor.InterestingLocs;
          var map = m_Actor.Location.Map;
          m_Exploration.Update(m_Actor.Location,zone => {
            zone.Bounds.DoForEach(pt => { if (!_items.HaveEverSeen(new Location(map, pt))) tourism.Record(map, pt); });
          });
        } else {
          m_Exploration.Update(m_Actor.Location);
        }
      }

      ExpireTaboos();
      InitAICache(current, _all);

      // get out of the range of explosions if feasible
      ActorAction tmpAction = BehaviorFleeExplosives();
      if (null != tmpAction) return tmpAction;

      _enemies = SortByGridDistance(FilterEnemies(current)); // this tests fast

      // if we have no enemies and have not fled an explosion, our friends can see that we're safe
      if (null == _enemies) AdviseFriendsOfSafety();

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

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == _enemies ? "null == _enemies" : _enemies.Count.ToString()+" enemies"));
#endif
      // civilians track how long since they've seen trouble
      if (InCombat) m_SafeTurns = 0;
      else ++m_SafeTurns;

      if (null != _enemies) m_LastEnemySaw = game.Rules.DiceRoller.Choose(_enemies);

      if (!Directives.CanThrowGrenades && m_Actor.GetEquippedWeapon() is ItemGrenade grenade) game.DoUnequipItem(m_Actor, grenade);

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      // \todo change target for using Goal_NextCombatAction to short-circuit unhealthy cowardice (or not, main objective processing is above)
      // this action tests whether enemies are in sight and chooses which action to take based on this
      // useful for assault running, dash-and-shoot, take cover and prepare for dash-and-shoot

      List<ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();
#if TIME_TURNS
        timer.Restart();
#endif

      tmpAction = ManageMeleeRisk(available_ranged_weapons);
#if TIME_TURNS
        timer.Stop();
        if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": ManageMeleeRisk " + timer.ElapsedMilliseconds.ToString()+"ms");
#endif
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "managing melee risk");
#endif
      if (null != tmpAction) return tmpAction;

      if (null != _enemies && Directives.CanThrowGrenades) {
        tmpAction = BehaviorThrowGrenade(game);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "toss grenade");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(available_ranged_weapons);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "probably reloading");
#endif
      if (null != tmpAction) return tmpAction;

	  List<Percept> friends = FilterNonEnemies(current);
      if (null != _enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(_enemies).Percepted as Actor);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "warning friends");
#endif
          if (null != tmpAction) return tmpAction;
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        tmpAction = BehaviorFightOrFlee(game, Directives.Courage, m_Emotes, RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS);
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
          tmpAction = BehaviorGoEatCorpse(current);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "cannibalism");
#endif
          if (null != tmpAction) return tmpAction;
        }
      }

      IEnumerable<Engine.MapObjects.PowerGenerator> generators_off = GeneratorsToTurnOn(m_Actor.Location.Map);    // formerly reused much later
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
      if (null == _enemies && Directives.CanTakeItems) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for items to take");
#endif
        var interestingStacks = GetInterestingInventoryStacks(current);
        if (interestingStacks != null) {
          {
          var get_item = new Dictionary<Location, ActorAction>();
          foreach(var at_target in interestingStacks.Where(p => m_Actor.MayTakeFromStackAt(p.Location))) {
            tmpAction = BehaviorGrabFromAccessibleStack(at_target.Location, at_target.Percepted as Inventory);
            if (tmpAction?.IsLegal() ?? false) get_item[at_target.Location] = tmpAction;
          }
          if (1<get_item.Count) {
            var considering = new List<Location>(get_item.Count);
            var dominated = new List<Location>(get_item.Count);
            foreach(var x in get_item) {
              if (0 >= considering.Count) {
                considering.Add(x.Key);
                continue;
              }
              int item_compare = 0;   // new item.CompareTo(any old item) i.e. new item <=> any old item
              switch(x.Value) {
              case ActionTakeItem new_take:
                item_compare = 1;
                foreach(var old_loc in considering) {
                  switch(get_item[old_loc]) {
                  case ActionTakeItem old_take:
                     if (new_take.Item.Model.ID==old_take.Item.Model.ID) { // \todo take from "endangered stack" if quantity-sensitive, otherwise not-endangered stack
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(new_take.Item,old_take.Item)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_take.Item, new_take.Item)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case ActionTradeWithContainer old_trade:
                     if (RHSMoreInteresting(new_take.Item,old_trade.Take)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_trade.Take, new_take.Item)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case ActionUseItem old_use:
                    // generally better to take than use
                    if (old_use.Item.Model.ID!=new_take.Item.Model.ID) dominated.Add(old_loc);
                    else item_compare = 0;
                    break;
                  }
                  if (-1==item_compare) break;
                }
                break;
              case ActionTradeWithContainer new_trade:
                item_compare = 1;
                foreach(var old_loc in considering) {
                  switch(get_item[old_loc]) {
                  case ActionTakeItem old_take:
                     if (RHSMoreInteresting(new_trade.Take,old_take.Item)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_take.Item, new_trade.Take)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case ActionTradeWithContainer old_trade:
                     if (new_trade.Take.Model.ID == old_trade.Take.Model.ID) { // \todo take from "endangered stack" if quantity-sensitive, otherwise not-endangered stack
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(new_trade.Take, old_trade.Take)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_trade.Take, new_trade.Take)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case ActionUseItem old_use:
                    // generally better to take than use
                    if (old_use.Item.Model.ID!= new_trade.Take.Model.ID) dominated.Add(old_loc);
                    else item_compare = 0;
                    break;
                  }
                  if (-1==item_compare) break;
                }
                break;
              case ActionUseItem new_use:
                item_compare = 0;   // new item.CompareTo(any old item) i.e. new item <=> any old item
                foreach(var old_loc in considering) {
                  switch(get_item[old_loc]) {
                    case ActionUseItem old_use:
                      if (old_use.Item.Model.ID==new_use.Item.Model.ID) { // duplicate
                        item_compare = -1;
                        break;
                      }
                      break;
                    case ActionTakeItem old_take:
                      if (old_take.Item.Model.ID!=new_use.Item.Model.ID) { // generally better to take than use
                        item_compare = -1;
                        break;
                      }
                      break;
                  }
                  if (-1==item_compare) break;
                }
                break;
              }
              // respond to item comparison
              if (1 == item_compare) {
                considering.Clear();
                dominated.Clear();
              } else if (0 < dominated.Count) {
                foreach(var reject in dominated) considering.Remove(reject);
                dominated.Clear();
              }
              if (-1 == item_compare) continue;
              considering.Add(x.Key);
            }
            get_item.OnlyIf(loc => considering.Contains(loc));
          }
#if FALSE_POSITIVE
          if (/* m_Actor.IsDebuggingTarget && */ 1<get_item.Count && !m_Actor.Inventory.IsEmpty) throw new InvalidOperationException(m_Actor.Name+", stack choosing: "+get_item.to_s());
#endif
          if (0<get_item.Count) {
            var take = get_item.FirstOrDefault();
            m_Actor.Activity = Activity.IDLE;
            return take.Value;
          }
          }

          // no accessible interesting stacks.  Memorize them just in case.
          {
          var track_inv = Goal<Goal_PathToStack>();
          foreach(Percept p in interestingStacks) {
            if (null == track_inv) {
              track_inv = new Goal_PathToStack(m_Actor.Location.Map.LocalTime.TurnCounter,m_Actor,p.Location);
              Objectives.Add(track_inv);
            } else track_inv.newStack(p.Location);
          }
          }

          Percept percept = FilterNearest(interestingStacks);
          while(null != percept) {
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
            interestingStacks.Remove(percept);
            percept = FilterNearest(interestingStacks);
          }
        }
        {   // leadership or trading requests
        Goal_HintPathToActor remote = Goal<Goal_HintPathToActor>();
        if (null != remote) {
          tmpAction = remote.Pathing();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, remote.ToString()+": "+tmpAction.to_s());
#endif
          if (null != tmpAction) return tmpAction;
        }
        }
        tmpAction = BehaviorRequestCriticalFromGroup();
        if (null != tmpAction) return tmpAction;
        if (Directives.CanTrade) {
          tmpAction = BehaviorFindTrade(friends);
          if (null != tmpAction) return tmpAction;
        }
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "have checked for items to take");
#endif
        {
        Goal_PathToStack remote = Goal<Goal_PathToStack>();
        if (null != remote) {
          tmpAction = remote.Pathing();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, remote.ToString()+": " +tmpAction.to_s());
#endif
          if (null != tmpAction) return tmpAction;
        }
        }
      } // null == enemies && Directives.CanTakeItems

      // attempting extortion from cops should have consequences.
      // XXX as should doing it to a civilian whose leader is a cop (and in communication)
      if (   RogueGame.Options.IsAggressiveHungryCiviliansOn
          && current != null
          && !m_Actor.HasLeader
          && !m_Actor.Model.Abilities.IsLawEnforcer
          && (m_Actor.IsHungry
          && !m_Actor.Has<ItemFood>())) {
        Percept target = FilterNearest(current.FilterT<Actor>(a =>
        {
          if (a == m_Actor || a.IsDead || (a.Inventory == null || a.Inventory.IsEmpty) || (a.Leader == m_Actor || m_Actor.Leader == a))
            return false;
          if (a.Inventory.Has<ItemFood>()) return true;
          return a.Location.Items?.Has<ItemFood>() ?? false;
        }));
        if (target != null) {
          tmpAction = BehaviorChargeEnemy(target, true, true);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "starving, attacking for food");
#endif
            if (game.Rules.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              RogueGame.DoSay(m_Actor, target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION | RogueGame.Sayflags.IS_DANGER);
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
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "critical: "+critical.to_s());
#endif
          tmpAction = BehaviorResupply(critical);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorResupply ok");
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resupplying: "+tmpAction.ToString());
#endif
          if (null != tmpAction) return tmpAction;
        }
      }


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
        var want_leader = friends.FilterT<Actor>(a => m_Actor.CanTakeLeadOf(a));
        FilterOutUnreachablePercepts(ref want_leader, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP);
        Percept target = FilterNearest(want_leader);
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
          game.DoEmote(m_Actor, "Open damn it! I know there is food there!", true);
          return tmpAction;
        }
        if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE)) {
          tmpAction = BehaviorPushNonWalkableObjectForFood();
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for food behind non-walkable objects");
#endif
            game.DoEmote(m_Actor, "Where is all the damn food?!", true);
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      tmpAction = BehaviorGoReviveCorpse(current);  // not logically CivilianAI only
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

      Percept percept1 = current.FilterFirst(p =>
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
      var pathing = m_Actor.OnePath(m_Actor.Location);
      pathing.OnlyIf(action => action.IsLegal());
      
      if (null != _legal_steps || 0<pathing.Count) {
        // advanced pathing ultimately reduces to various flavors of calls to (specializations) of 
        // public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering advanced pathing");
#endif
        bool combat_unready = CombatUnready();
        HashSet<Gameplay.GameItems.IDs> want = (null != items ? WhatDoIWantNow() : new HashSet<Gameplay.GameItems.IDs>());    // non-emergency things
        // while we want to account for what our followers want, we don't want to block our followers from the items either
        if (null != items) want.IntersectWith(items);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "want: "+want.to_s());
#endif

        // 2019-01-31: historical range sorting is:, This map, other maps [old implementation pruned 2019-03-18]
        // However, this equates the district entry map with the much smaller basement, and does not cope well with space-time scaling or the cross-district minimap
        // what would make sense is: local, radio range (minimap), "world" (last may not need immediate implementing, other maps may do for now)
        // local is the viewport for large maps, and the map for small maps (CHAR Underground base is "large" but does not cross-district path)
        // radio range is everything that fits on the minimap; distinct from local only for large maps
        // convention: a null map has been blacklisted
        // if the final return value is null, we know the map was blacklisted and do not need to expand from it
        Func<Map,HashSet<Point>> pathing_targets = null;
        ThreatTracking threats = m_Actor.Threats;
        HashSet<Point> hunt_threat(Map m) {
          return (m == m.District.SewersMap && Session.Get.HasZombiesInSewers) ? new HashSet<Point>() : threats.ThreatWhere(m);
        }

        if (!combat_unready && null != threats && threats.Any()) pathing_targets = hunt_threat;

        LocationSet sights_to_see = m_Actor.InterestingLocs;
        HashSet<Point> tourism(Map m) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "tourism for: "+m.ToString());
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, sights_to_see.In(m).to_s());
#endif
          return sights_to_see.In(m);
        }
        if (null != sights_to_see) pathing_targets = pathing_targets.Otherwise(tourism);

        // police want to exclude threat/tourism in indoor zones already covered by leader-types (other handling will cover engaged threat)
        var allies = m_Actor.Allies;
        void already_handled(Map m, HashSet<Point> target) {
          foreach(var a in allies) {
            if (a.HasLeader && 0 >= a.CountFollowers) continue; // bottom of chain of command, no authority to clear zones on own
            if (a.IsSleeping) continue; // not expected to do anything useful
            var handled = (a.Controller as ObjectiveAI)?.ClearingThisZone();
            if (null == handled) continue;
            if (handled.m != m) continue;
            target.RemoveWhere(pt => handled.Rect.Contains(pt));
          }
        }
        if (null != allies) pathing_targets.Postfilter(already_handled);

        HashSet<Point> generators(Map m) {
          var gens = Generators(m);
          if (null == gens) return new HashSet<Point>();
          if (WantToRecharge()) return new HashSet<Point>(gens.Select(obj => obj.Location.Position));
          var gens_off = gens.Where(obj => !obj.IsOn);
          if (gens_off.Any()) return new HashSet<Point>(gens_off.Select(obj => obj.Location.Position));   // XXX should be for map
          return new HashSet<Point>();
        }

        if (HasBehaviorThatRecallsToSurface) pathing_targets = pathing_targets.Union(generators);

        HashSet<Point> resupply_want(Map m)
        {
          return WhereIs(want,m);
        }

        if (0 < want.Count) pathing_targets = pathing_targets.Union(resupply_want);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing targets: "+(null==pathing_targets ? "null" : "non-null"));
#endif
        if (null != pathing_targets) {
          var view = m_Actor.Location.View;
          var d_span = view.DistrictSpan;
          int map_code = District.UsesCrossDistrictView(m_Actor.Location.Map);

          // The prefilter functions are going into HashSet<>.RemoveWhere so they have to return false to accept, true to reject
          bool prefilter_view(Map m) {
            if (m==m_Actor.Location.Map) return false;
            if (0 >= map_code) return true;
            if (map_code != District.UsesCrossDistrictView(m)) return true;
            return !d_span.Contains(m.District.WorldPosition);
          }

          // these two may need to be new parameters for BehaviorPathTo
          bool reject_view(Location loc) { return !view.Contains(loc); }

          // 1) view pathing
          tmpAction = BehaviorPathTo(pathing_targets,prefilter_view, reject_view);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within view: "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null!=tmpAction) return tmpAction;
          // 2) minimap range pathing, if distinct from view
          if (0!= map_code) {
            view = m_Actor.Location.MiniMapView;
            d_span = view.DistrictSpan;

            bool prefilter_minimap(Map m) {
              if (m==m_Actor.Location.Map) return false;
              if (0 >= map_code) return true;  // large maps like the CHAR undergound base
              if (!d_span.Contains(m.District.WorldPosition)) return true;
              // entry map is code 1, and is promiscuous (want to respond to basements, etc.)
              int other_map_code = District.UsesCrossDistrictView(m);
              if (map_code == other_map_code) return false;
              if (1< map_code) return 1!=other_map_code;    // only consider entry map from subway/sewer for minimap pathfinding
              // entry map cares "where" the other map's entrance is
              if (1 < other_map_code) return false;  // subway and sewer always ok
              // hospital and police station go fully into scope if the entrance is there; basements can just check directly
              if (m.destination_maps.Get.Contains(m.District.EntryMap)) return !m.ExitsFor(m.District.EntryMap).Any(x => view.Contains(x.Value.Location));
              if (null!=Session.Get.UniqueMaps.NavigatePoliceStation(m)) return !Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.ExitsFor(m.District.EntryMap).Any(x => view.Contains(x.Value.Location));
              else if (null != Session.Get.UniqueMaps.NavigateHospital(m)) return !Session.Get.UniqueMaps.Hospital_Admissions.TheMap.ExitsFor(m.District.EntryMap).Any(x => view.Contains(x.Value.Location));
              return true;
            }

#if PROTOTYPE
            // possible default blacklisting is fine for this
            bool postfilter_minimap(Location goals)
            {
              return true;   // \todo implement
            }
#endif
            tmpAction = BehaviorPathTo(pathing_targets,prefilter_minimap /*,postfilter_minimap*/);
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within minimap: "+(tmpAction?.ToString() ?? "null"));
#endif
            if (null!=tmpAction) return tmpAction;
          }
          // 3) world pathing (no prefilter/postfilter, ok to hunt threat even if combat unready)
          if (combat_unready && null != threats && threats.Any()) pathing_targets = pathing_targets.Otherwise(hunt_threat);

          tmpAction = BehaviorPathTo(pathing_targets);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within world: "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null!=tmpAction) return tmpAction;
        }
      }
#endregion

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
      return BehaviorWander(m_Exploration);
    }
  }
}
