// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CivilianAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define DATAFLOW_TRACE

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
    private int m_SafeTurns;
    private readonly ExplorationData m_Exploration;
    private string[] m_Emotes;

    public CivilianAI()
    {
      m_LOSSensor = new LOSSensor(VISION_SEES);
      m_SafeTurns = 0;
      m_Exploration = new ExplorationData();
      m_LastEnemySaw = null;
      m_LastItemsSaw = null;
      m_LastSoldierSaw = null;
      m_LastRaidHeard = null;
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
#if FAIL
      ActorAction tmpAction = BehaviorFleeFromExplosives(percepts1);
      if (null != tmpAction) return tmpAction;
#endif

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      Dictionary<Point,int> damage_field = new Dictionary<Point, int>();
      List<Actor> slow_melee_threat = new List<Actor>();
      if (null != enemies) VisibleMaximumDamage(damage_field, slow_melee_threat);
      bool in_blast_field = AddExplosivesToDamageField(damage_field, percepts1);  // only civilians and soldiers respect explosives; CHAR and gang don't
      if (0>=damage_field.Count) damage_field = null;
      if (0>= slow_melee_threat.Count) slow_melee_threat = null;
      List<Point> retreat = null;
      IEnumerable<Point> tmp_point;
      // calculate retreat destinations if possibly needed
      if (null != damage_field && null!=legal_steps && damage_field.ContainsKey(m_Actor.Location.Position)) {
        tmp_point = legal_steps.Where(pt=>!damage_field.ContainsKey(pt));
        if (tmp_point.Any()) retreat = tmp_point.ToList();
        // XXX we should be checking for running retreats before damaging ones
        // that would allow handling grenades as a damage field source
        if (null == retreat) {
          tmp_point = legal_steps.Where(p=> damage_field[p] < damage_field[m_Actor.Location.Position]);
          if (tmp_point.Any()) retreat = tmp_point.ToList();
        }
      }
      // prefer retreating where we have further room to retreat
      if (null != retreat && 2<=retreat.Count) {
        HashSet<Point> cornered = new HashSet<Point>(retreat);
        foreach(Point pt in Enumerable.Range(0,16).Select(i=>m_Actor.Location.Position.RadarSweep(2,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
          if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) break;
        }
        if (0<cornered.Count && cornered.Count<retreat.Count) retreat = new List<Point>(retreat.Except(cornered));
      }

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      IEnumerable<ItemRangedWeapon> tmp_rw = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>()?.Where(rw => 0 < rw.Ammo || null != m_Actor.GetCompatibleAmmoItem(rw));
      List<ItemRangedWeapon> available_ranged_weapons = (null!=tmp_rw && tmp_rw.Any() ? tmp_rw.ToList() : null);
	  List<Percept> friends = FilterNonEnemies(percepts1);

      // get out of the range of explosions if feasible
      if (in_blast_field && null != retreat) {
	    tmpAction = DecideMove(retreat, enemies, friends);
        if (null != tmpAction) {
		  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
          if (null != tmpAction2) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
          return tmpAction;
        }
      }

      // ranged weapon: prefer to maintain LoF when retreating
      if (null!= retreat && 2 <= retreat.Count && null!= available_ranged_weapons) {
        Dictionary<Point,int> targets = new Dictionary<Point,int>();
        int max_range = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
        foreach(Point pt in retreat) {
          targets[pt] = 0;
          foreach(Percept p in enemies) {
            if (LOS.CanTraceHypotheticalFireLine(new Location(m_Actor.Location.Map,pt), p.Location.Position, max_range, m_Actor)) targets[pt]++;    // hard-code current LOS as range
          }
        }
        int max_LoF = targets.Values.Max();
        targets.OnlyIf(val=>val==max_LoF);
        retreat = targets.Keys.ToList();
      }

      // ranged weapon: fast retreat ok
      // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
      // XXX we also want to be close enough to fire at all
      if (null != retreat && null!=available_ranged_weapons) {
	    tmpAction = DecideMove(retreat, enemies, friends);
        if (null != tmpAction) {
		  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
          if (null != tmpAction2) RunIfAdvisable(tmpAction2.dest.Position);
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // need stamina to melee: slow retreat ok
      if (null != retreat && WillTireAfterAttack(m_Actor)) {
	    tmpAction = DecideMove(retreat, enemies, friends);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // have slow enemies nearby
      if (null != retreat && null != slow_melee_threat) {
	    tmpAction = DecideMove(retreat, enemies, friends);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // end melee risk management check

      if (null != enemies && Directives.CanThrowGrenades)
      {
        tmpAction = BehaviorThrowGrenade(game, enemies);
        if (null != tmpAction) return tmpAction;
      }

      if (null != enemies) {
        if (1==Rules.GridDistance(enemies[0].Location.Position,m_Actor.Location.Position)) {
          // something adjacent...check for one-shotting
          ItemMeleeWeapon tmp_melee = m_Actor.GetBestMeleeWeapon(it => !IsItemTaboo(it));
          if (null!=tmp_melee) {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              Attack tmp_attack = m_Actor.HypotheticalMeleeAttack((tmp_melee.Model as ItemMeleeWeaponModel).BaseMeleeAttack(m_Actor.Sheet),en);
              if (en.HitPoints>tmp_attack.DamageValue/2) continue;
              // can one-shot
              if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {    // safe
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  if (!tmp_melee.IsEquipped) game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  if (!tmp_melee.IsEquipped) game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
            }
          }
        }
      }

      tmpAction = BehaviorEquipWeapon(game, legal_steps, damage_field, available_ranged_weapons, enemies, friends);
      if (null != tmpAction) return tmpAction;

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && m_Actor.Leader.IsAdjacentToEnemy;
      bool assistLeader = hasVisibleLeader && isLeaderFighting && !m_Actor.IsTired;

      if (null != enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(enemies).Percepted as Actor);
          if (null != tmpAction) return tmpAction;
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        tmpAction = BehaviorFightOrFlee(game, enemies, hasVisibleLeader, isLeaderFighting, Directives.Courage, m_Emotes);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
      if (null != tmpAction) return tmpAction;

      if (null != enemies && assistLeader) {
        Percept target = FilterNearest(enemies);
        tmpAction = BehaviorChargeEnemy(target);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }

      // handle food after enemies check
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

      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && Directives.CanSleep && OkToSleepNow) {
        tmpAction = BehaviorSecurePerimeter();
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        tmpAction = BehaviorSleep(game);
        if (null != tmpAction) {
          if (tmpAction is ActionSleep)
            m_Actor.Activity = Activity.SLEEPING;
          return tmpAction;
        }
      }
      tmpAction = BehaviorDropUselessItem();
      if (null != tmpAction) return tmpAction;

      if (null == enemies && Directives.CanTakeItems) {
        Map map = m_Actor.Location.Map;
        List<Percept> perceptList2 = percepts1.FilterT<Inventory>().FilterOut(p =>
        {
          if (p.Turn != map.LocalTime.TurnCounter) return true; // not in sight
          if (IsOccupiedByOther(map, p.Location.Position)) return true; // blocked
          if (IsTileTaboo(p.Location.Position)) return true;    // already ruled out
          return null==BehaviorWouldGrabFromStack(game, p.Location.Position, p.Percepted as Inventory);
        });
        if (perceptList2 != null) {
          Percept percept = FilterNearest(perceptList2);
          m_LastItemsSaw = percept;
          ActorAction actorAction5 = BehaviorGrabFromStack(game, percept.Location.Position, percept.Percepted as Inventory);
          if (actorAction5 != null && actorAction5.IsLegal()) {
            m_Actor.Activity = Activity.IDLE;
            return actorAction5;
          }
          // XXX the main valid way this could fail, is a stack behind a non-walkable, etc., object that isn't a container
          // could happen in normal play in the sewers
          // under is handled within the Behavior functions
#if DATAFLOW_TRACE
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+"has abandoned getting the items at "+ percept.Location.Position);
#endif
          MarkTileAsTaboo(percept.Location.Position,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);
          game.DoEmote(m_Actor, "Mmmh. Looks like I can't reach what I want.");
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
            // XXX if both parties have exactly one interesting tradeable item, check that the trade is allowed by the mutual-advantage filter (extract from RogueGame::PickItemToTrade)
            return !(actor.Controller as OrderableAI).HasAnyInterestingItem(TradeableItems);    // other half of m_Actor.GetInterestingTradeableItems(...)
          });
          if (percepts2 != null) {
            Actor actor = FilterNearest(percepts2).Percepted as Actor;  // XXX unstable; this can throw a null error
            if (Rules.IsAdjacent(m_Actor.Location, actor.Location)) {
              tmpAction = new ActionTrade(m_Actor, actor);
              if (tmpAction.IsLegal()) {
                MarkActorAsRecentTrade(actor);
                game.DoSay(m_Actor, actor, string.Format("Hey {0}, let's make a deal!", (object) actor.Name), RogueGame.Sayflags.NONE);
                return tmpAction;
              }
            } else {
              tmpAction = BehaviorIntelligentBumpToward(actor.Location.Position);
              if (null != tmpAction) {
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
            if (game.Rules.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              game.DoSay(m_Actor, target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION);
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }
      if (game.Rules.RollChance(USE_STENCH_KILLER_CHANCE)) {
        tmpAction = BehaviorUseStenchKiller();
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorCloseDoorBehindMe(game, PrevLocation);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (m_Actor.Model.Abilities.HasSanity) {
        if (m_Actor.Sanity < 3*m_Actor.MaxSanity/4) {
          tmpAction = BehaviorUseEntertainment();
          if (null != tmpAction)  return tmpAction;
        }
        tmpAction = BehaviorDropBoringEntertainment(game);
        if (null != tmpAction) return tmpAction;
      }
      if (m_Actor.HasLeader && !DontFollowLeader) {
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        tmpAction = BehaviorFollowActor(m_Actor.Leader, maxDist);
        if (null != tmpAction) {
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
        if (0 < m_Actor.CountFollowers) {
          foreach (Actor fo in m_Actor.Followers) {
            HashSet<Gameplay.GameItems.IDs> fo_crit = (fo.Controller as OrderableAI)?.WhatDoINeedNow();
            if (null != fo_crit) critical.UnionWith(fo_crit);
          }
        }
        critical.IntersectWith(items);
        if (0 < critical.Count) {
          tmpAction = BehaviorResupply(critical);
          if (null != tmpAction) return tmpAction;
        }
      }

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorAttackBarricade(game);
        if (null != tmpAction) {
          game.DoEmote(m_Actor, "Open damn it! I know there is food there!");
          return tmpAction;
        }
        if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE)) {
          tmpAction = BehaviorPushNonWalkableObjectForFood(game);
          if (null != tmpAction) {
            game.DoEmote(m_Actor, "Where is all the damn food?!");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      tmpAction = BehaviorGoReviveCorpse(game, percepts1);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (game.Rules.RollChance(USE_EXIT_CHANCE)) {
        tmpAction = BehaviorUseExit(game, BaseAI.UseExitFlags.DONT_BACKTRACK);
        if (null != tmpAction) return tmpAction;
      }
      if (game.Rules.RollChance(BUILD_TRAP_CHANCE)) {
        tmpAction = BehaviorBuildTrap(game);
        if (null != tmpAction) return tmpAction;
      }
      if (game.Rules.RollChance(BUILD_LARGE_FORT_CHANCE)) {
        tmpAction = BehaviorBuildLargeFortification(game, 1);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(BUILD_SMALL_FORT_CHANCE)) {
        tmpAction = BehaviorBuildSmallFortification(game);
        if (null != tmpAction) {
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
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastSoldierSaw != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_SOLDIER_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastEnemySaw != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_ENEMY_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastItemsSaw != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_ITEMS_CHANCE)) {
          tmpAction = BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_Actor.Model.Abilities.IsLawEnforcer && game.Rules.RollChance(LAW_ENFORCE_CHANCE)) {
          tmpAction = BehaviorEnforceLaw(game, friends);
          if (null != tmpAction) return tmpAction;
        }
	  }
      if (m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(2, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      // hunt down threats
      tmpAction = BehaviorHuntDownThreat();
      if (null != tmpAction) return tmpAction;

      // tourism
      tmpAction = BehaviorTourism();
      if (null != tmpAction) return tmpAction;

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
