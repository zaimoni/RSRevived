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

      ActorAction tmpAction = BehaviorFleeFromExplosives(percepts1);
      if (null != tmpAction) return tmpAction;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      Dictionary<Point,int> damage_field = (null != enemies ? VisibleMaximumDamage() : null);
      // \todo visible primed explosives also have a damage field that civilians and soldiers respect, CHAR and gang don't
      List<Point> retreat = null;
      List<Actor> slow_threat = null;
      IEnumerable<Point> tmp_point;
      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      if (null != damage_field && null!=legal_steps && damage_field.ContainsKey(m_Actor.Location.Position)) {
        IEnumerable<Percept> tmp_percept = enemies.Where(p=>1==Rules.GridDistance(m_Actor.Location.Position,p.Location.Position));
        if (tmp_percept.Any()) slow_threat = new List<Actor>(tmp_percept.Select(p=>(p.Percepted as Actor)));
        tmp_point = legal_steps.Where(pt=>!damage_field.ContainsKey(pt));
        if (tmp_point.Any()) retreat = tmp_point.ToList();
        // XXX we should be checking for running retreats before damaging ones
        // that would allow handling grenades as a damage field source
        if (null == retreat) {
          tmp_point = legal_steps.Where(p=> damage_field[p] < damage_field[m_Actor.Location.Position]);
          if (tmp_point.Any()) retreat = tmp_point.ToList();
        }
      }
      // XXX should not block line of fire to mutual enemies of a non-enemy
      // prefer not to jump
      if (null != retreat && 2 <= retreat.Count) {
        tmp_point = retreat.Where(pt=> {
          MapObject tmp = m_Actor.Location.Map.GetMapObjectAt(pt);
          return null==tmp || tmp.IsWalkable || tmp.IsJumpable;
        });
        if (tmp_point.Count()<retreat.Count()) retreat = new List<Point>(tmp_point);
      }
      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      IEnumerable<ItemRangedWeapon> tmp_rw = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>()?.Where(rw => 0 < rw.Ammo || null != GetCompatibleAmmoItem(rw));
      List<ItemRangedWeapon> available_ranged_weapons = (null!=tmp_rw && tmp_rw.Any() ? tmp_rw.ToList() : null);
	  List<Percept> friends = FilterNonEnemies(percepts1);

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
      if (null != retreat && null != slow_threat) {
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
          ItemMeleeWeapon tmp_melee = GetBestMeleeWeapon(it => !IsItemTaboo(it));
          if (null!=tmp_melee && !tmp_melee.IsEquipped) {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              Attack tmp_attack = m_Actor.HypotheticalMeleeAttack((tmp_melee.Model as ItemMeleeWeaponModel).BaseMeleeAttack(m_Actor.Sheet),en);
              if (en.HitPoints>tmp_attack.DamageValue/2) continue;
              // can one-shot
              if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {    // safe
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) {
                  game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
            }
          } else {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              Attack tmp_attack = m_Actor.MeleeAttack(en);
              if (en.HitPoints>tmp_attack.DamageValue/2) continue;
              // can one-shot
              if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) return tmpAction;
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(en);
                if (null != tmpAction) return tmpAction;
              }
            }
          }
        }
      }

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction) return tmpAction;

      // all free actions must be above the enemies check
      if (null != enemies && m_Actor.GetEquippedWeapon() is ItemRangedWeapon) {
        List<Percept> percepts2 = FilterFireTargets(enemies);
        if (percepts2 != null) {
		  if (null != damage_field  && 2<=percepts2.Count && !damage_field.ContainsKey(m_Actor.Location.Position)) {
		    // attempt to snipe with current weapon
		    foreach(Percept p in enemies) {
              Actor en = p.Percepted as Actor;
			  if (m_Actor.CurrentRangedAttack.Range<Rules.GridDistance(m_Actor.Location.Position,en.Location.Position)) continue;
              Attack tmp_attack = m_Actor.RangedAttack(Rules.GridDistance(m_Actor.Location.Position, en.Location.Position));
              if (en.HitPoints>tmp_attack.DamageValue/2) continue;
			  // can one-shot
              tmpAction = BehaviorRangedAttack(en);
              if (tmpAction != null) {
                m_Actor.Activity = Activity.FIGHTING;
                m_Actor.TargetActor = en;
                return tmpAction;
              }
			}
		  }

		  // normally, shoot at nearest target
          Actor actor = FilterNearest(percepts2).Percepted as Actor;
          tmpAction = BehaviorRangedAttack(actor);
          if (tmpAction != null) {
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = actor;
            return tmpAction;
          }
        }
      }

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && m_Actor.Leader.IsAdjacentToEnemy;
      bool assistLeader = hasVisibleLeader && isLeaderFighting && !m_Actor.IsTired;

      if (null != enemies && friends != null) {
        if (game.Rules.RollChance(50)) {
          ActorAction actorAction2 = BehaviorWarnFriends(friends, FilterNearest(enemies).Percepted as Actor);
          if (actorAction2 != null) return actorAction2;
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        ActorAction actorAction5 = BehaviorFightOrFlee(game, enemies, hasVisibleLeader, isLeaderFighting, Directives.Courage, m_Emotes);
        if (actorAction5 != null) return actorAction5;
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

#if FAIL
      // the new objectives system should trigger after all enemies-handling behavior
      List<Objectives> ai_objectives = Objectives;
      if (null != ai_objectives) {
      }
#endif

      // handle food after enemies check
      tmpAction = BehaviorEatProactively(game);
      if (null != tmpAction) return tmpAction;

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorEat();
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsStarving || m_Actor.IsInsane) {
          tmpAction = BehaviorGoEatCorpse(FilterT<List<Corpse>>(percepts1));
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && Directives.CanSleep && (m_Actor.WouldLikeToSleep && m_Actor.IsInside) && game.Rules.CanActorSleep(m_Actor)) {
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
        List<Percept> perceptList2 = SortByDistance(FilterOut(FilterT<Inventory>(percepts1), (Predicate<Percept>) (p =>
        {
          if (p.Turn != map.LocalTime.TurnCounter) return true; // not in sight
          if (IsOccupiedByOther(map, p.Location.Position)) return true; // blocked
          if (IsTileTaboo(p.Location.Position)) return true;    // already ruled out
          Inventory tmp = p.Percepted as Inventory;
          if (!HasAnyInterestingItem(tmp)) return true; // nothing interesting
          if (m_Actor.Inventory.CountItems < m_Actor.MaxInv) return false;  // obviously have space, ok
          foreach (Item it in tmp.Items) {
            if (IsItemTaboo(it) || !IsInterestingItem(it)) continue;
            foreach (Item it2 in m_Actor.Inventory.Items) {
              if (RHSMoreInteresting(it2, it)) return false;    // clearly more interesting than what we have
            }
          }
          return true;  // no, not really interesting after all
        })));
        if (perceptList2 != null)
        {
retry:    Percept percept = FilterNearest(perceptList2);
          m_LastItemsSaw = percept;
          Inventory stack = percept.Percepted as Inventory;
          ActorAction actorAction5 = BehaviorGrabFromStack(game, percept.Location.Position, stack);
          if (actorAction5 != null && actorAction5.IsLegal() && actorAction5 is ActionTakeItem) {
            Item tmp = (actorAction5 as ActionTakeItem).Item;
            // check for "more interesting stack"
            foreach(Percept p in perceptList2) {
              if (p == percept) continue;
              Inventory inv = p.Percepted as Inventory;
              foreach (Item it in inv.Items) {
                if (IsItemTaboo(it) || !IsInterestingItem(it)) continue;
                if (RHSMoreInteresting(tmp, it)) {  // we have a wrong stack
                  MarkTileAsTaboo(percept.Location.Position,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);
                  perceptList2.Remove(percept);
                  goto retry;
                }
              }
            }
          }
          if (actorAction5 != null && actorAction5.IsLegal()) {
            m_Actor.Activity = Activity.IDLE;
            return actorAction5;
          }
#if DATAFLOW_TRACE
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+"has abandoned getting the items at "+ percept.Location.Position);
#endif
          MarkTileAsTaboo(percept.Location.Position,WorldTime.TURNS_PER_HOUR+Session.Get.CurrentMap.LocalTime.TurnCounter);
          game.DoEmote(m_Actor, "Mmmh. Looks like I can't reach what I want.");
        }
        if (Directives.CanTrade && HasAnyTradeableItem(m_Actor.Inventory)) {
          List<Item> TradeableItems = GetTradeableItems(m_Actor.Inventory);
          List<Percept> percepts2 = FilterOut(friends, (Predicate<Percept>) (p =>
          {
            if (p.Turn != map.LocalTime.TurnCounter)
              return true;
            Actor actor = p.Percepted as Actor;
            if (actor.IsPlayer) return true;
            if (!game.Rules.CanActorInitiateTradeWith(m_Actor, actor)) return true;
            if (IsActorTabooTrade(actor)) return true;
            if (!HasAnyInterestingItem(actor.Inventory)) return true;
            return !(actor.Controller as BaseAI).HasAnyInterestingItem(TradeableItems);
          }));
          if (percepts2 != null) {
            Actor actor = FilterNearest(percepts2).Percepted as Actor;
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
      }
      if (RogueGame.Options.IsAggressiveHungryCiviliansOn && percepts1 != null && (!m_Actor.HasLeader && !m_Actor.Model.Abilities.IsLawEnforcer) && (m_Actor.IsHungry && !m_Actor.Has<ItemFood>()))
      {
        Percept target = FilterNearest(FilterT<Actor>(percepts1, (Predicate<Actor>) (a =>
        {
          if (a == m_Actor || a.IsDead || (a.Inventory == null || a.Inventory.IsEmpty) || (a.Leader == m_Actor || m_Actor.Leader == a))
            return false;
          if (a.Inventory.Has<ItemFood>()) return true;
          Inventory itemsAt = a.Location.Map.GetItemsAt(a.Location.Position);
          return null != itemsAt && itemsAt.Has<ItemFood>();
        })));
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

      Percept percept1 = FilterFirst(percepts1, (Predicate<Percept>) (p =>
      {
        Actor actor = p.Percepted as Actor;
        if (actor == null || actor == m_Actor) return false;
        return IsSoldier(actor);
      }));
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
          Actor target;
          tmpAction = BehaviorEnforceLaw(game, friends, out target);
          if (null != tmpAction) {
            m_Actor.TargetActor = target;
            return tmpAction;
          }
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
      tmpAction = BehaviorExplore(game, m_Exploration);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
