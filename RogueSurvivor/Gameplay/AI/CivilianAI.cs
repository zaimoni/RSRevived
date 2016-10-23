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

    private LOSSensor m_LOSSensor;
    private int m_SafeTurns;
    private ExplorationData m_Exploration;
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

    protected override List<Percept> _UpdateSensors()
    {
      List<Percept> tmp = m_LOSSensor.Sense(m_Actor);
      if ((int)Gameplay.GameFactions.IDs.ThePolice == m_Actor.Faction.ID) {
        // police report the items they see to other police.
        // \todo This implementation is too powerful; it should be a mutual-update between police
        // in the same district
        Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> ItemMemory = Session.Get.PoliceItemMemory;
        // police report threat (or lack thereof) to other police
        ThreatTracking inferred_threat = Session.Get.PoliceThreatTracking;
        HashSet<Point> has_threat = new HashSet<Point>();

        // report actually seen threat
        foreach(Engine.AI.Percept p in tmp) {
          Actor tmp3 = p.Percepted as Actor;
          if (null == tmp3 || tmp3.IsDead || !m_Actor.IsEnemyOf(tmp3)) continue;
          inferred_threat.Sighted(tmp3,p.Location);
          has_threat.Add(p.Location.Position);
        }
        // update the enhanced item memory here
        Dictionary<Location,HashSet< Gameplay.GameItems.IDs >> seen_items = new Dictionary<Location, HashSet<Gameplay.GameItems.IDs>>();
        foreach(Engine.AI.Percept p in tmp) {
          Inventory tmp3 = p.Percepted as Inventory;
          if (null == tmp3 || 0 >= tmp3.CountItems) continue;
          seen_items[p.Location] = new HashSet<Gameplay.GameItems.IDs>(tmp3.Items.Select(x => x.Model.ID));
        }
        // ensure fact what is in sight is current, is recorded
        foreach (Point pt in FOV) {
          Location tmp3 = new Location(m_Actor.Location.Map, pt);
          if (!has_threat.Contains(pt)) inferred_threat.Cleared(tmp3);
          if (seen_items.ContainsKey(tmp3)) { ItemMemory.Set(tmp3, seen_items[tmp3], m_Actor.Location.Map.LocalTime.TurnCounter); }
          else { ItemMemory.Set(tmp3, null, m_Actor.Location.Map.LocalTime.TurnCounter); }
        }        
      }

      return tmp;
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);

      BehaviorEquipBodyArmor(game);

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight(game) && !BehaviorEquipStenchKiller(game)) {
        BehaviorUnequipLeftItem(game);
      }
      // end item juggling check
      
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

      m_Exploration.Update(m_Actor.Location);

      ExpireTaboos();

      List<Percept> enemies = SortByGridDistance(FilterEnemies(game, percepts1));
      // civilians track how long since they've seen trouble
      if (null != enemies) m_SafeTurns = 0;
      else ++m_SafeTurns;

      if (null != enemies)
        m_LastEnemySaw = enemies[game.Rules.Roll(0, enemies.Count)];

      if (!Directives.CanThrowGrenades) {
        ItemGrenade itemGrenade = m_Actor.GetEquippedWeapon() as ItemGrenade;
        if (itemGrenade != null) {
          game.DoUnequipItem(m_Actor, itemGrenade);
        }
      }

      ActorAction tmpAction = BehaviorFleeFromExplosives(game, percepts1);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
        return tmpAction;
      }

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
        if (tmp_point.Any()) retreat = new List<Point>(tmp_point);
        // XXX we should be checking for running retreats before damaging ones
        // that would allow handling grenades as a damage field source
        if (null == retreat) {
          tmp_point = legal_steps.Where(p=> damage_field[p] < damage_field[m_Actor.Location.Position]);
          if (tmp_point.Any()) retreat = new List<Point>(tmp_point);
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
      List<ItemRangedWeapon> available_ranged_weapons = (null!=tmp_rw && 0<tmp_rw.Count() ? new List<ItemRangedWeapon>(tmp_rw) : null);

      // ranged weapon: fast retreat ok
      // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
      // XXX we also want to be close enough to fire at all
      if (null != retreat && null!=available_ranged_weapons) {
        Point tmp = retreat[game.Rules.Roll(0,retreat.Count)];
        tmpAction = new ActionMoveStep(m_Actor, tmp);
        if (tmpAction.IsLegal()) {
          RunIfAdvisable(tmp);
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // need stamina to melee: slow retreat ok
      if (null != retreat && WillTireAfterAttack(m_Actor)) {
        Point tmp = retreat[game.Rules.Roll(0,retreat.Count)];
        tmpAction = new ActionMoveStep(m_Actor, tmp);
        if (tmpAction.IsLegal()) {
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }
      // have slow enemies nearby
      if (null != retreat && null!=slow_threat) {
        Point tmp = retreat[game.Rules.Roll(0,retreat.Count)];
        tmpAction = new ActionMoveStep(m_Actor, tmp);
        if (tmpAction.IsLegal()) {
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
                tmpAction = BehaviorMeleeAttack(game,en);
                if (null != tmpAction) {
                  game.DoEquipItem(m_Actor, tmp_melee);
                  return tmpAction;
                }
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(game,en);
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
                tmpAction = BehaviorMeleeAttack(game,en);
                if (null != tmpAction) return tmpAction;
              }
              if (1==enemies.Count && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
                tmpAction = BehaviorMeleeAttack(game,en);
                if (null != tmpAction) return tmpAction;
              }
            }
          }
        }
      }

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      // all free actions must be above the enemies check
      if (null != enemies && m_Actor.GetEquippedWeapon() is ItemRangedWeapon)
      {
        List<Percept> percepts2 = FilterFireTargets(game, enemies);
        if (percepts2 != null)
        {
          Actor actor = FilterNearest(percepts2).Percepted as Actor;
          ActorAction actorAction5 = BehaviorRangedAttack(game, actor);
          if (actorAction5 != null)
          {
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = actor;
            return actorAction5;
          }
        }
      }

      bool hasVisibleLeader = (m_Actor.HasLeader && !DontFollowLeader) && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
      bool isLeaderFighting = (m_Actor.HasLeader && !DontFollowLeader) && IsAdjacentToEnemy(game, m_Actor.Leader);
      bool assistLeader = hasVisibleLeader && isLeaderFighting && !m_Actor.IsTired;

      if (null != enemies)
      {
        if (game.Rules.RollChance(50))
        {
          List<Percept> friends = FilterNonEnemies(game, percepts1);
          if (friends != null)
          {
            ActorAction actorAction2 = BehaviorWarnFriends(game, friends, FilterNearest(enemies).Percepted as Actor);
            if (actorAction2 != null)
            {
              m_Actor.Activity = Activity.IDLE;
              return actorAction2;
            }
          }
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        ActorAction actorAction5 = BehaviorFightOrFlee(game, enemies, hasVisibleLeader, isLeaderFighting, Directives.Courage, m_Emotes);
        if (actorAction5 != null)
          return actorAction5;
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
      if (null != enemies && assistLeader)
      {
        Percept target = FilterNearest(enemies);
        tmpAction = BehaviorChargeEnemy(game, target);
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

      if (m_Actor.IsHungry)
      {
        tmpAction = BehaviorEat(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        if (m_Actor.IsStarving || m_Actor.IsInsane)
        {
          tmpAction = BehaviorGoEatCorpse(game, FilterCorpses(percepts1));
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && Directives.CanSleep && (m_Actor.WouldLikeToSleep && m_Actor.IsInside) && game.Rules.CanActorSleep(m_Actor))
      {
        tmpAction = BehaviorSecurePerimeter(game);
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
      tmpAction = BehaviorDropUselessItem(game);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      if (null == enemies && Directives.CanTakeItems)
      {
        Map map = m_Actor.Location.Map;
        List<Percept> perceptList2 = SortByDistance(FilterOut(FilterStacks(percepts1), (Predicate<Percept>) (p =>
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
                  MarkTileAsTaboo(percept.Location.Position,WorldTime.TURNS_PER_HOUR+game.Session.CurrentMap.LocalTime.TurnCounter);
                  perceptList2.Remove(percept);
                  goto retry;
                }
              }
            }
          }
          if (actorAction5 != null && actorAction5.IsLegal())
          {
            m_Actor.Activity = Activity.IDLE;
            return actorAction5;
          }
#if DATAFLOW_TRACE
          Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+"has abandoned getting the items at "+ percept.Location.Position);
#endif
          MarkTileAsTaboo(percept.Location.Position,WorldTime.TURNS_PER_HOUR+game.Session.CurrentMap.LocalTime.TurnCounter);
          game.DoEmote(m_Actor, "Mmmh. Looks like I can't reach what I want.");
        }
        if (Directives.CanTrade && HasAnyTradeableItem(m_Actor.Inventory))
        {
          List<Item> TradeableItems = GetTradeableItems(m_Actor.Inventory);
          List<Percept> percepts2 = FilterOut(FilterNonEnemies(game, percepts1), (Predicate<Percept>) (p =>
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
          if (percepts2 != null)
          {
            Actor actor = FilterNearest(percepts2).Percepted as Actor;
            if (Rules.IsAdjacent(m_Actor.Location, actor.Location)) {
              tmpAction = new ActionTrade(m_Actor, actor);
              if (tmpAction.IsLegal()) {
                MarkActorAsRecentTrade(actor);
                game.DoSay(m_Actor, actor, string.Format("Hey {0}, let's make a deal!", (object) actor.Name), RogueGame.Sayflags.NONE);
                return tmpAction;
              }
            } else {
              tmpAction = BehaviorIntelligentBumpToward(game, actor.Location.Position);
              if (null != tmpAction) {
                m_Actor.Activity = Activity.FOLLOWING;
                m_Actor.TargetActor = actor;
                return tmpAction;
              }
            }
          }
        }
      }
      if (RogueGame.Options.IsAggressiveHungryCiviliansOn && percepts1 != null && (!m_Actor.HasLeader && !m_Actor.Model.Abilities.IsLawEnforcer) && (m_Actor.IsHungry && !m_Actor.HasItemOfType(typeof(ItemFood))))
      {
        Percept target = FilterNearest(FilterActors(percepts1, (Predicate<Actor>) (a =>
        {
          if (a == m_Actor || a.IsDead || (a.Inventory == null || a.Inventory.IsEmpty) || (a.Leader == m_Actor || m_Actor.Leader == a))
            return false;
          if (a.Inventory.HasItemOfType(typeof (ItemFood)))
            return true;
          Inventory itemsAt = a.Location.Map.GetItemsAt(a.Location.Position);
          if (itemsAt == null || itemsAt.IsEmpty)
            return false;
          return itemsAt.HasItemOfType(typeof (ItemFood));
        })));
        if (target != null) {
          tmpAction = BehaviorChargeEnemy(game, target);
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
        tmpAction = BehaviorUseStenchKiller(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      tmpAction = BehaviorCloseDoorBehindMe(game, PrevLocation);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (m_Actor.Model.Abilities.HasSanity)
      {
        if (m_Actor.Sanity < 3*m_Actor.MaxSanity/4) {
          tmpAction = BehaviorUseEntertainment(game);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        tmpAction = BehaviorDropBoringEntertainment(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (m_Actor.HasLeader && !DontFollowLeader)
      {
        Point position1 = m_Actor.Leader.Location.Position;
        bool isVisible = m_LOSSensor.FOV.Contains(position1);
        int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        tmpAction = BehaviorFollowActor(game, m_Actor.Leader, position1, isVisible, maxDist);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      }
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP) >= 1 && (!(m_Actor.HasLeader && !DontFollowLeader) && m_Actor.CountFollowers < game.Rules.ActorMaxFollowers(m_Actor)))
      {
        Percept target = FilterNearest(FilterNonEnemies(game, percepts1));
        if (target != null) {
          tmpAction = BehaviorLeadActor(game, target);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.IDLE;
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }
      if (m_Actor.IsHungry)
      {
        tmpAction = BehaviorAttackBarricade(game);
        if (null != tmpAction) {
          game.DoEmote(m_Actor, "Open damn it! I know there is food there!");
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
        if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE))
        {
          tmpAction = BehaviorPushNonWalkableObjectForFood(game);
          if (null != tmpAction) {
            game.DoEmote(m_Actor, "Where is all the damn food?!");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      tmpAction = BehaviorGoReviveCorpse(game, FilterCorpses(percepts1));
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (game.Rules.RollChance(USE_EXIT_CHANCE)) {
        tmpAction = BehaviorUseExit(game, BaseAI.UseExitFlags.DONT_BACKTRACK);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(BUILD_TRAP_CHANCE)) {
        tmpAction = BehaviorBuildTrap(game);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
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
      if (m_LastRaidHeard != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_RAID_CHANCE)) {
        tmpAction = BehaviorTellFriendAboutPercept(game, m_LastRaidHeard);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      Percept percept1 = FilterFirst(game, percepts1, (Predicate<Percept>) (p =>
      {
        Actor actor = p.Percepted as Actor;
        if (actor == null || actor == m_Actor)
          return false;
        return IsSoldier(actor);
      }));
      if (percept1 != null)
                m_LastSoldierSaw = percept1;
      if (game.Rules.RollChance(TELL_FRIEND_ABOUT_SOLDIER_CHANCE) && m_LastSoldierSaw != null) {
        tmpAction = BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(TELL_FRIEND_ABOUT_ENEMY_CHANCE) && m_LastEnemySaw != null) {
        tmpAction = BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (game.Rules.RollChance(TELL_FRIEND_ABOUT_ITEMS_CHANCE) && m_LastItemsSaw != null) {
        tmpAction = BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (m_Actor.Model.Abilities.IsLawEnforcer && percepts1 != null && game.Rules.RollChance(LAW_ENFORCE_CHANCE))
      {
        Actor target;
        tmpAction = BehaviorEnforceLaw(game, percepts1, out target);
        if (null != tmpAction) {
          m_Actor.TargetActor = target;
          return tmpAction;
        }
      }
      if (m_Actor.CountFollowers > 0)
      {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(game, 2, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE)) {
            if (target.IsSleeping) {
              game.DoEmote(m_Actor, string.Format("patiently waits for {0} to wake up.", (object) target.Name));
            } else {
              if (m_LOSSensor.FOV.Contains(target.Location.Position))
                game.DoEmote(m_Actor, string.Format("Come on {0}! Hurry up!", (object) target.Name));
              else
                game.DoEmote(m_Actor, string.Format("Where the hell is {0}?", (object) target.Name));
            }
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
