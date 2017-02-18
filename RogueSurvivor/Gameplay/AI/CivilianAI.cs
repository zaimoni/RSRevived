// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CivilianAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_SELECTACTION

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CivilianAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "Go away",
      "Damn it I'm trapped!",
      "I'm not afraid"
    };
    private static string[] BIG_BEAR_EMOTES = new string[3]
    {
      "You fool",
      "I'm fooled!",
      "Be a man"
    };
    private static string[] FAMU_FATARU_EMOTES = new string[3]
    {
      "Bakemono",
      "Nani!?",
      "Kawaii"
    };
    private static string[] SANTAMAN_EMOTES = new string[3]
    {
      "DEM BLOODY KIDS!",
      "LEAVE ME ALONE I AIN'T HAVE NO PRESENTS!",
      "MERRY FUCKIN' CHRISTMAS"
    };
    private static string[] ROGUEDJACK_EMOTES = new string[3]
    {
      "Sorry butt I am le busy,",
      "I should have redone ze AI rootines!",
      "Let me test le something on you"
    };
    private static string[] DUCKMAN_EMOTES = new string[3]
    {
      "I'LL QUACK YOU BACK",
      "THIS IS MY FINAL QUACK",
      "I'M GONNA QUACK YOU"
    };
    private static string[] HANS_VON_HANZ_EMOTES = new string[3]
    {
      "RAUS",
      "MEIN FUHRER!",
      "KOMM HIER BITE"
    };
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
    private const int LAW_ENFORCE_CHANCE = 30;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS | LOSSensor.SensingFilter.CORPSES;

    private readonly LOSSensor m_LOSSensor;
    private int m_SafeTurns = 0;
    private readonly ExplorationData m_Exploration = new ExplorationData();
    private string[] m_Emotes;

    public CivilianAI()
    {
      m_LOSSensor = new LOSSensor(VISION_SEES);
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
      return m_LOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(Actor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (FOV.Contains(target.Location.Position)) return "Come on {0}! Hurry up!";
      else return "Where the hell is {0}?";
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());

      BehaviorEquipBodyArmor(game);

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight(game) && !BehaviorEquipStenchKiller(game)) {
        BehaviorUnequipLeftItem(game);
      }
      // end item juggling check

      List<Percept> percepts1 = FilterSameMap(UpdateSensors());

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

      m_Exploration.Update(m_Actor.Location);

      ExpireTaboos();

      List<Percept> enemies = SortByGridDistance(FilterEnemies(percepts1));
      // civilians track how long since they've seen trouble
      if (null != enemies) m_SafeTurns = 0;
      else ++m_SafeTurns;

      if (null != enemies) m_LastEnemySaw = enemies[game.Rules.Roll(0, enemies.Count)];

      if (!Directives.CanThrowGrenades) {
        ItemGrenade itemGrenade = m_Actor.GetEquippedWeapon() as ItemGrenade;
        if (itemGrenade != null) {
          game.DoUnequipItem(m_Actor, itemGrenade);
        }
      }

      // obsolete: not needed with AddExplosivesToDamageField
      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      Dictionary<Point,int> damage_field = new Dictionary<Point, int>();
      List<Actor> slow_melee_threat = new List<Actor>();
      HashSet<Actor> immediate_threat = new HashSet<Actor>();
      if (null != enemies) VisibleMaximumDamage(damage_field, slow_melee_threat, immediate_threat);
      AddTrapsToDamageField(damage_field, percepts1);
      bool in_blast_field = AddExplosivesToDamageField(damage_field, percepts1);  // only civilians and soldiers respect explosives; CHAR and gang don't
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
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat, enemies, friends) : ((null != retreat) ? DecideMove(retreat, enemies, friends) : null));
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "fleeing explosives");
#endif
		  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
          if (null != tmpAction2) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
          return tmpAction;
        }
      }

      List<ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      if ((null != retreat || null != run_retreat) && null != available_ranged_weapons && null!=enemies) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(retreat, enemies);
        MaximizeRangedTargets(run_retreat, enemies);

        // ranged weapon: fast retreat ok
        // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
        // XXX we also want to be close enough to fire at all
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat, enemies, friends) : ((null != retreat) ? DecideMove(retreat, enemies, friends) : null));
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "ranged weapon retreat");
#endif
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
	    tmpAction = DecideMove(retreat, enemies, friends);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "too tired for melee retreat");
#endif
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // have slow enemies nearby
      if (null != retreat && null != slow_melee_threat) {
	    tmpAction = DecideMove(retreat, enemies, friends);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "slow melee retreat");
#endif
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // end melee risk management check

      if (null != enemies && Directives.CanThrowGrenades) {
        tmpAction = BehaviorThrowGrenade(game, enemies);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "toss grenade");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game, legal_steps, damage_field, available_ranged_weapons, enemies, friends, immediate_threat);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "probably reloading");
#endif
      if (null != tmpAction) return tmpAction;

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
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
        tmpAction = BehaviorFightOrFlee(game, enemies, hasVisibleLeader, isLeaderFighting, Directives.Courage, m_Emotes);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "having to fight w/o ranged weapons");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "medicating");
#endif
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resting");
#endif
      if (null != tmpAction) return tmpAction;

      if (null != enemies && assistLeader) {    // difference between civilian and CHAR/soldier is ok here
        Percept target = FilterNearest(enemies);
        tmpAction = BehaviorChargeEnemy(target);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "assisting leader in melee");
#endif
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }

      // handle food after enemies check
      tmpAction = BehaviorEatProactively(game);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "eating proactively");
#endif
      if (null != tmpAction) return tmpAction;

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
#if TRACE_SELECTACTION
            else {
              if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "returning to task");
              return goal_action;
            }
#else
            else return goal_action;
#endif
          }
        }
      }

      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && Directives.CanSleep && OkToSleepNow) {
        tmpAction = BehaviorSecurePerimeter();
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "securing perimeter");
#endif
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        tmpAction = BehaviorSleep(game);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "sleeping");
#endif
          if (tmpAction is ActionSleep)
            m_Actor.Activity = Activity.SLEEPING;
          return tmpAction;
        }
      }
      tmpAction = BehaviorDropUselessItem();
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "ditching useless item");
#endif
      if (null != tmpAction) return tmpAction;

      if (null == enemies && Directives.CanTakeItems) {
        Map map = m_Actor.Location.Map;
        List<Percept> perceptList2 = percepts1.FilterT<Inventory>().FilterOut(p =>
        {
          if (p.Turn != map.LocalTime.TurnCounter) return true; // not in sight
          if (IsOccupiedByOther(map, p.Location.Position)) return true; // blocked
          if (IsTileTaboo(p.Location.Position)) return true;    // already ruled out
          if (m_Actor.Location.Position!= p.Location.Position && null==m_Actor.MinStepPathTo(map, m_Actor.Location.Position, p.Location.Position)) return true;    // something wrong, e.g. iron gates in way
          return null==BehaviorWouldGrabFromStack(game, p.Location.Position, p.Percepted as Inventory);
        });
        if (perceptList2 != null) {
          Percept percept = FilterNearest(perceptList2);
          m_LastItemsSaw = percept;
          tmpAction = BehaviorGrabFromStack(game, percept.Location.Position, percept.Percepted as Inventory);
          if (null != tmpAction && tmpAction.IsLegal()) {
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
          MarkTileAsTaboo(percept.Location.Position,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);
          game.DoEmote(m_Actor, "Mmmh. Looks like I can't reach what I want.");
#if DEBUG
          throw new InvalidOperationException("Prescreen for avoidng taboo tile marking failed");
#endif
        }
        if (Directives.CanTrade && HasAnyTradeableItem()) {
          List<Item> TradeableItems = GetTradeableItems();  // iterating over friends next so m_Actor.GetInterestingTradeableItems(...) is inappropriate; do first half here
          List<Percept> percepts2 = friends.FilterOut(p =>
          {
            if (p.Turn != map.LocalTime.TurnCounter)
              return true;
            Actor actor = p.Percepted as Actor;
            if (actor.IsPlayer) return true;
            if (!m_Actor.CanTradeWith(actor)) return true;
            if (IsActorTabooTrade(actor)) return true;
            if (null == actor.GetRationalTradeableItems(this)) return true;   // XXX avoid Charisma check
            if (null==m_Actor.MinStepPathTo(map, m_Actor.Location.Position, p.Location.Position)) return true;    // something wrong, e.g. iron gates in way.  Usual case is police visiting jail.
            // XXX if both parties have exactly one interesting tradeable item, check that the trade is allowed by the mutual-advantage filter (extract from RogueGame::PickItemToTrade)
            return !(actor.Controller as OrderableAI).HasAnyInterestingItem(TradeableItems);    // other half of m_Actor.GetInterestingTradeableItems(...)
          });
          if (percepts2 != null) {
            Actor actor = FilterNearest(percepts2).Percepted as Actor;
            if (Rules.IsAdjacent(m_Actor.Location, actor.Location)) {
              tmpAction = new ActionTrade(m_Actor, actor);
              if (tmpAction.IsLegal()) {
#if TRACE_SELECTACTION
                if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "trading");
#endif
                MarkActorAsRecentTrade(actor);
                game.DoSay(m_Actor, actor, string.Format("Hey {0}, let's make a deal!", (object) actor.Name), RogueGame.Sayflags.NONE);
                return tmpAction;
              }
            } else {
              tmpAction = BehaviorIntelligentBumpToward(actor.Location.Position);
              if (null != tmpAction) {
#if TRACE_SELECTACTION
                if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "preparing to trade");
#endif
                m_Actor.Activity = Activity.FOLLOWING;
                m_Actor.TargetActor = actor;
                return tmpAction;
              }
            }
          }
        }
      } // null == enemies && Directives.CanTakeItems
      if (RogueGame.Options.IsAggressiveHungryCiviliansOn && percepts1 != null && (!m_Actor.HasLeader && !m_Actor.Model.Abilities.IsLawEnforcer) && (m_Actor.IsHungry && !m_Actor.Has<ItemFood>()))
      {
        Percept target = FilterNearest(percepts1.FilterT<Actor>(a =>
        {
          if (a == m_Actor || a.IsDead || (a.Inventory == null || a.Inventory.IsEmpty) || (a.Leader == m_Actor || m_Actor.Leader == a))
            return false;
          if (a.Inventory.Has<ItemFood>()) return true;
          Inventory itemsAt = a.Location.Map.GetItemsAt(a.Location.Position);
          return null != itemsAt && itemsAt.Has<ItemFood>();
        }));
        if (target != null) {
          tmpAction = BehaviorChargeEnemy(target);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "starving, attacking for food");
#endif
            if (game.Rules.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              game.DoSay(m_Actor, target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION);
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = target.Percepted as Actor;
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
      tmpAction = BehaviorCloseDoorBehindMe(game, PrevLocation);    // civilian-specific
      if (null != tmpAction) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "closing door");
#endif
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      if (m_Actor.Model.Abilities.HasSanity) {  // not logically civilian-specific, but needs a rework anyway
        if (m_Actor.Sanity < 3*m_Actor.MaxSanity/4) {
          tmpAction = BehaviorUseEntertainment();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using entertainment");
#endif
          if (null != tmpAction)  return tmpAction;
        }
        tmpAction = BehaviorDropBoringEntertainment(game);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "drop boring entertainment");
#endif
        if (null != tmpAction) return tmpAction;
      }

      if (m_Actor.HasLeader && !DontFollowLeader) {
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
      }
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP) >= 1 && (!(m_Actor.HasLeader && !DontFollowLeader) && m_Actor.CountFollowers < m_Actor.MaxFollowers))
      {
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

      // XXX if we have item memory, check whether "critical items" have a known location.  If so, head for them (floodfill pathfinding)
      // XXX leaders try to check what their followers use as well.
      List<Gameplay.GameItems.IDs> items = WhatHaveISeen();
      if (null != items) {
        HashSet<Gameplay.GameItems.IDs> critical = WhatDoINeedNow();    // out of ammo, or hungry without food
        // while we want to account for what our followers want, we don't want to block our followers from the items either
#if FAIL
        if (0 < m_Actor.CountFollowers) {
          foreach (Actor fo in m_Actor.Followers) {
            HashSet<Gameplay.GameItems.IDs> fo_crit = (fo.Controller as OrderableAI)?.WhatDoINeedNow();
            if (null != fo_crit) critical.UnionWith(fo_crit);
          }
        }
#endif
        critical.IntersectWith(items);
        if (0 < critical.Count) {
          tmpAction = BehaviorResupply(critical);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resupplying");
#endif
          if (null != tmpAction) return tmpAction;
        }
      }

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorAttackBarricade(game);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for food behind barricade");
#endif
          game.DoEmote(m_Actor, "Open damn it! I know there is food there!");
          return tmpAction;
        }
        if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE)) {
          tmpAction = BehaviorPushNonWalkableObjectForFood(game);
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
      tmpAction = BehaviorGoReviveCorpse(game, percepts1);  // not logically CivilianAI only
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "revive corpse");
#endif
        return tmpAction;
      }
      if (game.Rules.RollChance(USE_EXIT_CHANCE)) {
        tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.DONT_BACKTRACK);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "use exit for no good reason");
#endif
        if (null != tmpAction) return tmpAction;
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

      Percept percept1 = percepts1.FilterFirst(p =>
      {
        Actor actor = p.Percepted as Actor;
        if (actor == null || actor == m_Actor) return false;
        return IsSoldier(actor);
      });
      if (percept1 != null) m_LastSoldierSaw = percept1;

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
        if (m_Actor.Model.Abilities.IsLawEnforcer && game.Rules.RollChance(LAW_ENFORCE_CHANCE)) {
          tmpAction = BehaviorEnforceLaw(game, friends);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law");
#endif
          if (null != tmpAction) return tmpAction;
        }
	  }

      if (m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(2, out target);
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

      // XXX civilians that start in a boarded-up building (sewer maintenance, gun shop, hardware store
      // should stay there until they get the all-clear from the police

      // The newer movement behaviors using floodfill pathing, etc. depend on there being legal walking moves
      if (null!=legal_steps) {
        HashSet<GameItems.IDs> critical = WhatDoINeedNow();
        critical.IntersectWith(GameItems.ammo);
        if (0 >= critical.Count) {
          // hunt down threats -- works for police
          tmpAction = BehaviorHuntDownThreatCurrentMap();
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

        if (0 >= critical.Count) {
          // hunt down threats -- works for police
          if (m_Actor.Location.Map!=m_Actor.Location.Map.District.EntryMap) {
            tmpAction = BehaviorHuntDownThreatOtherMaps();
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "hunting down threat, other maps -- on surface");
#endif
            if (null != tmpAction) return tmpAction;
          }
        }

        // tourism -- works for police
        tmpAction = BehaviorTourismOtherMaps();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "tourism, current map");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorExplore(game, m_Exploration, Directives.Courage);
      if (null != tmpAction) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "unguided exploration");
#endif
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "wandering");
#endif
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
