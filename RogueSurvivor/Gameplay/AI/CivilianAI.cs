// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CivilianAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_SELECTACTION

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using djack.RogueSurvivor.Engine.Actions;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CivilianAI : OrderableAI
  {
#nullable enable
    private static readonly string[] FIGHT_EMOTES = new string[MAX_EMOTES] {
      "Go away",
      "Damn it I'm trapped!",
      "I'm not afraid"
    };
    private const int LOS_MEMORY = 1;   // just enough memory to not walk into exploding grenades
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
    private const int HUNGRY_CHARGE_EMOTE_CHANCE = 50;
    private const int HUNGRY_PUSH_OBJECTS_CHANCE = 25;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS | LOSSensor.SensingFilter.CORPSES;

    private readonly MemorizedSensor<LOSSensor> m_MemLOSSensor;
    private int m_SafeTurns;
    private readonly ExplorationData m_Exploration = new ExplorationData();
    private string[] m_Emotes;

    public CivilianAI(Actor src) : base(src)
    {
      m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES, src), LOS_MEMORY);
      m_Emotes = FIGHT_EMOTES;
    }

    // we don't have memory, but we do have taboo trades
    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      TabooTrades?.OnlyIfNot(Actor.IsDeceased);
    }

    public override void TakeControl()
    {
      base.TakeControl();
      ReviewItemRatings();  // XXX \todo should be in ObjectiveAI override
    }

    public void InstallUniqueEmotes(string[] src)
    {
#if DEBUG
      if (!m_Actor.IsUnique) throw new InvalidOperationException("only uniques can get better emotes");
#endif
      m_Emotes = src;
    }


    public override List<Percept> UpdateSensors()
    {
      return m_MemLOSSensor.Sense();
    }

    public override HashSet<Point> FOV { get { return m_MemLOSSensor.Sensor.FOV; } }
    public override Location[] FOVloc { get { return m_MemLOSSensor.Sensor.FOVloc; } }
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_MemLOSSensor.Sensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_MemLOSSensor.Sensor.enemies; } }
    public override Dictionary<Location, Inventory>? items_in_FOV { get { return m_MemLOSSensor.Sensor.items; } }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(Actor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (CanSee(target.Location)) return "Come on {0}! Hurry up!";
      else return "Where the hell is {0}?";
    }
#nullable restore

    protected override ActorAction SelectAction()
    {
      var game = RogueForm.Game;

      ClearMovePlan();
      // \todo start extraction target: BehaviorEquipBestItems (cf RS Alpha 10)
      BehaviorEquipBestBodyArmor();

      // start item juggling
      if (!BehaviorEquipCellPhone(game) && !BehaviorEquipLight()) {}
      // end extraction target: BehaviorEquipBestItems
      // end item juggling check
      _all = FilterSameMap(UpdateSensors());
      var current = FilterCurrent(_all);    // this tests fast
      ReviewItemRatings();  // XXX highly inefficient when called here; should "update on demand"

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+m_Actor.Location.Map.LocalTime.TurnCounter.ToString());
#endif
      m_Actor.Walk();    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (null != Order) {
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "under orders");
#endif
        var actorAction = ExecuteOrder(game, Order, current);
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
        if (null!=ItemMemory && null!=m_Actor.InterestingLocs && !District.IsSewersMap(m_Actor.Location.Map)) {
          var _items = ItemMemory;
          var tourism = m_Actor.InterestingLocs;
          var map = m_Actor.Location.Map;
          m_Exploration.Update(m_Actor.Location,zone => {
            zone.Bounds.DoForEach(pt => { if (!_items.HaveEverSeen(new Location(map, pt))) tourism.Record(map, in pt); });
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

      _enemies = SortByGridDistance(FilterEnemies(current)); // this tests fast; makes InCombat valid

      // civilians track how long since they've seen trouble
      if (InCombat) m_SafeTurns = 0;
      else ++m_SafeTurns;

      // if we have no enemies and have not fled an explosion, our friends can see that we're safe
      if (null == _enemies) AdviseFriendsOfSafety();
      else m_LastEnemySaw = Rules.Get.DiceRoller.Choose(_enemies);

      // New objectives system
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, Objectives.Count.ToString()+" objectives");
#endif
      if (0<Objectives.Count) {
        ActorAction goal_action = null;
        foreach(Objective o in new List<Objective>(Objectives)) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && !o.IsExpired)  Logger.WriteLine(Logger.Stage.RUN_MAIN, o.ToString());
#endif
          if (o.IsExpired) Objectives.Remove(o);
          else if (o.UrgentAction(out goal_action)) {
            if (null==goal_action) Objectives.Remove(o);
#if DEBUG
            else if (!goal_action.IsPerformable()) throw new InvalidOperationException("result of UrgentAction should be legal");
#else
            else if (!goal_action.IsPerformable()) Objectives.Remove(o);
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

      if (!Directives.CanThrowGrenades && m_Actor.GetEquippedWeapon() is ItemGrenade grenade) grenade.UnequippedBy(m_Actor);

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

      tmpAction = ManageMeleeRisk(available_ranged_weapons);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "managing melee risk: "+tmpAction);
#endif
      if (null != tmpAction) return tmpAction;

      if (null != _enemies && Directives.CanThrowGrenades) {
        tmpAction = BehaviorThrowGrenade();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "toss grenade");
#endif
        if (null != tmpAction) return tmpAction;
      }

      // \todo doesn't handle properly interactions w/inventory management when enemies out of melee range; e.g. if have shotgun ammo, should take empty shotgun if not in immediate danger
      tmpAction = BehaviorEquipWeapon(available_ranged_weapons);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "probably reloading");
#endif
      if (null != tmpAction) return tmpAction;

	  var friends = FilterNonEnemies(current);
      if (null != _enemies) {
        if (null != friends && Rules.Get.RollChance(50)) {
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
      // at this point, even if enemies are in sight we have no useful direct combat action

      tmpAction = NonCombatReflexMoves();
      if (null != tmpAction) return tmpAction;

      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && Directives.CanSleep && m_Actor.WantToSleepNow) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorNavigateToSleep");
#endif
        tmpAction = BehaviorNavigateToSleep();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "navigating to sleep");
#endif
        if (null != tmpAction) return tmpAction;
      }

      // attempting extortion from cops should have consequences.
      // XXX as should doing it to a civilian whose leader is a cop (and in communication)
      if (   RogueGame.Options.IsAggressiveHungryCiviliansOn
          && current != null
          && !m_Actor.HasLeader
          && !m_Actor.Model.Abilities.IsLawEnforcer
          && (m_Actor.IsHungry
          && !m_Actor.Has<ItemFood>())) {
        var target = FilterNearest(current.FilterCast<Actor>(a =>
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
            if (Rules.Get.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              RogueGame.DoSay(m_Actor, target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION | RogueGame.Sayflags.IS_DANGER);
            if (!m_Actor.TargetActor.IsSleeping) {
              if (m_Actor.TargetActor.Faction.ID.ExtortionIsAggression()) {
                game.DoMakeAggression(m_Actor,m_Actor.TargetActor);
              } else if (m_Actor.TargetActor.Faction.ID.LawIgnoresExtortion()) {
                game.DoMakeAggression(m_Actor.TargetActor,m_Actor);
              } // XXX the target needs an AI modifier to handle this appropriately
            }
            game.PropagateSight(m_Actor.Location, a => {
              if (a.Leader != m_Actor && m_Actor.Leader != a) {
                if (a.Faction.ID.ExtortionIsAggression()) {
                  RogueGame.DoSay(a, m_Actor, string.Format("ATTEMPTED MURDER! {0} HAS AGGRESSED {1}!", m_Actor.TheName, m_Actor.TargetActor.TheName), RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
                  if (0 == m_Actor.MurdersOnRecord(a) || 0==m_Actor.MurdersCounter) m_Actor.HasMurdered(m_Actor.TargetActor); // XXX not really, but keeps things from going weird for civilian followers of police
                  game.RadioNotifyAggression(a, m_Actor, "(police radio, {0}) Executing {1} for attempted murder."); // XXX \todo correct name for radio
                  game.DoMakeAggression(a, m_Actor);
                }
              }
            });

            return tmpAction;
          }
        }
      }

      tmpAction = BehaviorUseStenchKiller();    // civilian-specific
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using stench killer");
#endif
      if (null != tmpAction) return tmpAction;

      bool? combat_unready = false;  // currently only matters for law enforcement
      if (m_Actor.Model.Abilities.IsLawEnforcer && !(combat_unready = CombatUnready()).Value) {
        tmpAction = BehaviorEnforceLaw();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law (combat ready)");
#endif
        if (null != tmpAction) return tmpAction;
      }

      // XXX if we have item memory, check whether "critical items" have a known location.  If so, head for them (floodfill pathfinding)
      // XXX leaders should try to check what their followers use as well.
      var items = WhatHaveISeen();
      if (null != items && null != _legal_path) {
        HashSet<Gameplay.GameItems.IDs> critical = WhatDoINeedNow();    // out of ammo, or hungry without food
        // while we want to account for what our followers want, we don't want to block our followers from the items either
        critical.IntersectWith(items);
        if (0 < critical.Count) {
          // Unfortunately, this used to cause bizarrely long hang times in the sewers.  What we really want is some way to properly record
          // multi-pathing such that an ai that can reach an inventory target "first" excludes it from its allies' consideration
          // this is unstable if it's *JUST* critical items; include want to mitigate pathing loops
          HashSet<Gameplay.GameItems.IDs> want = WhatDoIWantNow();
          want.IntersectWith(items);
          critical.UnionWith(want);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorResupply");
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "critical: "+critical.to_s());
#endif
          tmpAction = BehaviorResupply(critical);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorResupply ok; "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null != tmpAction) return tmpAction;
        }
      }

#if PROTOTYPE
      // more urgent to build a trap if our inventory is full
      if (m_Actor.Inventory.IsFull) {
        var use_trap = new Gameplay.AI.Goals.SetTrap(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor);
        if (use_trap.UrgentAction(out var ret) && null!=ret) {
          Objectives.Insert(0, use_trap);
          return ret;
        }
      }
#endif

      var rules = Rules.Get;
      if (rules.RollChance(BUILD_TRAP_CHANCE)) {
        tmpAction = BehaviorBuildTrap(game);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build trap");
#endif
        if (null != tmpAction) return tmpAction;
      }

      if (rules.RollChance(BUILD_LARGE_FORT_CHANCE)) { // difference in relative ordering with soldiers is ok
        tmpAction = BehaviorBuildLargeFortification(START_FORT_LINE_CHANCE);
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build large fortification");
#endif
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (rules.RollChance(BUILD_SMALL_FORT_CHANCE)) {
        tmpAction = BehaviorBuildSmallFortification();
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
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorFollowActor");
#endif
        tmpAction = BehaviorFollowActor(m_Actor.Leader, 1);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorFollowActor: "+(tmpAction?.ToString() ?? "null"));
#endif
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      } else if (m_Actor.CountFollowers < m_Actor.MaxFollowers) {
        var want_leader = friends?.Filter(a => m_Actor.CanTakeLeadOf(a.Percepted));
        if (m_Actor.Model.Abilities.IsLawEnforcer) want_leader = want_leader?.Filter(a => 0 >= a.MurdersCounter);
        FilterOutUnreachablePercepts(ref want_leader, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP);
        // \todo release block; next savegame; do not allow police to lead murderers
        var target = FilterNearest(want_leader);
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
            m_Actor.TargetActor = target.Percepted;
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
        if (rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE)) {
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
      if (null == m_Actor.Threats && null == m_Actor.InterestingLocs) {
        if (rules.RollChance(USE_EXIT_CHANCE)) {
          tmpAction = BehaviorUseExit(UseExitFlags.DONT_BACKTRACK);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "use exit for no good reason");
#endif
          if (null != tmpAction) return tmpAction;
        }
      }

      var percept1 = current?.FilterFirst(p =>
      {
        var actor = p.Percepted as Actor;
        if (actor == null || actor == m_Actor) return false;
        return actor.Controller is SoldierAI;
      });
      if (percept1 != null) m_LastSoldierSaw = percept1;

      if (m_Actor.Model.Abilities.IsLawEnforcer && combat_unready.Value) {
        tmpAction = BehaviorEnforceLaw();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law (combat unready)");
#endif
        if (null != tmpAction) return tmpAction;
      }

      // XXX civilians that start in a boarded-up building (sewer maintenance, gun shop, hardware store
      // should stay there until they get the all-clear from the police

      // The newer movement behaviors using floodfill pathing, etc. depend on there being legal walking moves
#region floodfill pathfinder
      if (null != _legal_path) {
        if (!InCombat) {    // want to recompute if threat tracking active within view rectangle
          tmpAction = UsePreexistingLambdaPath();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using pre-existing path");
#endif
          if (null != tmpAction) return tmpAction;
        }

        // advanced pathing ultimately reduces to various flavors of calls to (specializations) of 
        // public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering advanced pathing");
#endif
        if (null == combat_unready) combat_unready = CombatUnready();
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
        var update_path = ForceLambdaPath();
        Func<Map,HashSet<Point>> pathing_targets = null;
        ThreatTracking threats = m_Actor.Threats;
        HashSet<Point> hunt_threat(Map m) {
          if (District.IsSewersMap(m) && Session.Get.HasZombiesInSewers) return new HashSet<Point>(); // unclearable
          var ret = threats.ThreatWhere(m);
          if (0<ret.Count) update_path.StageView(m,ret);
          return ret;
        }

        if (!combat_unready.Value && null != threats && threats.Any()) pathing_targets = hunt_threat;

        LocationSet sights_to_see = m_Actor.InterestingLocs;
        HashSet<Point> tourism(Map m) {
          var ret = sights_to_see.In(m);
          if (0<ret.Count) update_path.StageView(m,ret);
          return ret;
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
            bool reject(Point pt) { return handled.Rect.Contains(pt); }
            target.RemoveWhere(reject);
            update_path.UnStageView(m,reject);
          }
        }
        if (null != allies) pathing_targets.Postfilter(already_handled);

        HashSet<Point> generators(Map m) {
          var gens = Generators(m);
          if (null == gens) return new HashSet<Point>();
          if (WantToRecharge()) {
            update_path.StageGenerators(gens);
            return m_Actor.CastToBumpableDestinations(m,gens.Select(obj => obj.Location.Position));
          }
          var gens_off = gens.Where(obj => !obj.IsOn);
          if (gens_off.Any()) {
            update_path.StageGenerators(gens_off);
            return m_Actor.CastToBumpableDestinations(m, gens_off.Select(obj => obj.Location.Position));   // XXX should be for map
          }
          return new HashSet<Point>();
        }

        if (HasBehaviorThatRecallsToSurface) pathing_targets = pathing_targets.Union(generators);

        HashSet<Point> resupply_want(Map m)
        {
          var ret = WhereIs(want, m);
          if (0 < ret.Count) update_path.StageInventory(m,ret);
          return m_Actor.CastToInventoryAccessibleDestinations(m,ret);
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
          bool reject_view(Location loc) { return !view.Contains(in loc); }

          // 1) view pathing
          _caller = CallChain.SelectAction_LambdaPath;
          tmpAction = BehaviorPathTo(pathing_targets,prefilter_view, reject_view);
          _caller = CallChain.NONE;
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within view: "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null!=tmpAction) return tmpAction;
          update_path.ForgetStaging();
          // 2) minimap range pathing, if distinct from view
          if (0!= map_code) {
            view = m_Actor.Location.MiniMapView;
            d_span = view.DistrictSpan;

            Predicate<Exit> exit_for_here(Map map) {
                Map m2 = map;   // required for proper lambda-function
                bool ret(Exit e) {
                  return e.ToMap == m2 && view.Contains(e.Location);
                }
                return ret;
            }

            bool prefilter_minimap(Map m) {
              if (m==m_Actor.Location.Map) return false;
              if (0 >= map_code) return true;  // CHAR underground base and basements are their own minimaps
              if (!d_span.Contains(m.District.WorldPosition)) return true;
              // entry map is code 1, and is promiscuous (want to respond to basements, etc.)
              int other_map_code = District.UsesCrossDistrictView(m);
              if (map_code == other_map_code) return false;
              if (1< map_code) return 1!=other_map_code;    // only consider entry map from subway/sewer for minimap pathfinding
              // entry map cares "where" the other map's entrance is
              if (1 < other_map_code) return false;  // subway and sewer always ok
              // hospital and police station go fully into scope if the entrance is there; basements can just check directly
              var e_map = m.District.EntryMap;
              var e_for_surface = exit_for_here(e_map);
              if (m.destination_maps.Get.Contains(e_map)) return !m.AnyExits(e_for_surface);
              var unique_m = Session.Get.UniqueMaps;
              if (null!= unique_m.NavigatePoliceStation(m)) return !unique_m.PoliceStation_OfficesLevel.TheMap.AnyExits(e_for_surface);
              else if (null != unique_m.NavigateHospital(m)) return !unique_m.Hospital_Admissions.TheMap.AnyExits(e_for_surface);
              return true;
            }

#if PROTOTYPE
            // possible default blacklisting is fine for this
            bool postfilter_minimap(Location goals)
            {
              return true;   // \todo implement
            }
#endif
          _caller = CallChain.SelectAction_LambdaPath;
            tmpAction = BehaviorPathTo(pathing_targets,prefilter_minimap /*,postfilter_minimap*/);
          _caller = CallChain.NONE;
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within minimap: "+(tmpAction?.ToString() ?? "null"));
#endif
            // object constancy: remember that we're going somewhere on the destination level
            if (   tmpAction is ActionUseExit leaving && leaving.dest.Map.District == m_Actor.Location.Map.District
                && 1==District.UsesCrossDistrictView(m_Actor.Location.Map) && 2!= District.UsesCrossDistrictView(leaving.dest.Map)) {
                var dest_view = District.UsesCrossDistrictView(leaving.dest.Map);
                if (2 != dest_view) {
                  HashSet<Location>? dests = null;
                  if (0<Session.Get.UniqueMaps.HospitalDepth(leaving.dest.Map)) dests = update_path.Unstage(loc => 0 < Session.Get.UniqueMaps.HospitalDepth(loc.Map));
                  if (0<Session.Get.UniqueMaps.PoliceStationDepth(leaving.dest.Map)) dests = update_path.Unstage(loc => 0 < Session.Get.UniqueMaps.PoliceStationDepth(loc.Map));
                  if (0 >= dest_view) dests = update_path.Unstage(loc => leaving.dest.Map != loc.Map);
                  else dests = update_path.Unstage(loc => dest_view != District.UsesCrossDistrictView(loc.Map));
                  if (0 < dests.Count) {
                    var goal = new Goals.AcquireLineOfSight(m_Actor, dests);
                    Objectives.Insert(0, goal);
                    AddFOVevent(goal);
                  }
                }
            }
            if (null!=tmpAction) return tmpAction;
            update_path.ForgetStaging();
          }
          // 3) world pathing (no prefilter/postfilter, ok to hunt threat even if combat unready)
          if (combat_unready.Value && null != threats && threats.Any()) pathing_targets = pathing_targets.Otherwise(hunt_threat);

          _caller = CallChain.SelectAction_LambdaPath;
          tmpAction = BehaviorPathTo(pathing_targets);
          _caller = CallChain.NONE;
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within world: "+(tmpAction?.ToString() ?? "null"));
#endif
          if (null!=tmpAction) return tmpAction;
          update_path.ForgetStaging();
        }
      }
#endregion

	  if (null != friends) {
        if (m_LastRaidHeard != null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastRaidHeard, TELL_FRIEND_ABOUT_RAID_CHANCE);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about raid");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastSoldierSaw != null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastSoldierSaw, TELL_FRIEND_ABOUT_SOLDIER_CHANCE);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about soldier");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastEnemySaw != null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastEnemySaw, TELL_FRIEND_ABOUT_ENEMY_CHANCE);
          if (null != tmpAction) {
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about enemy");
#endif
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastItemsSaw != null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastItemsSaw, TELL_FRIEND_ABOUT_ITEMS_CHANCE);
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
          if (rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

#if DEBUG
      if (null != m_Actor.Threats || null != m_Actor.InterestingLocs) {
        Session.Get.World.DaimonMap(); // for accuracy
        // test game is crashing here -- looks like issue is a death-trapped exit from the sewers that can be "fixed"
        // start function extraction target
        var same_floor_deathtraps = DeathTrapsInSight();
        if (null != same_floor_deathtraps) {
          var do_this = CanDisarmDeathtrap(same_floor_deathtraps);
          if (null != do_this) {
            var coordinate_this = new Goals.Cooperate(m_Actor, do_this);
            var tenable = coordinate_this.UrgentAction(out var next_action);
            if (null != next_action) {
              if (!coordinate_this.IsExpired) {
                var allies = m_Actor.Allies;
                if (null != allies) {
                  var zone = m_Actor.Location.Map.ClearableZoneAt(m_Actor.Location.Position);
                  foreach(var ally in allies) {
                    if (!InCommunicationWith(ally)) continue;
                    if (CanSee(ally.Location) && ally.Controller.CanSee(m_Actor.Location)) {
                      (ally.Controller as ObjectiveAI).SetObjective(new Goals.Cooperate(ally, do_this));
                      continue;
                    }
                    if (null != zone) {
                      var ally_zone = ally.Location.Map.ClearableZoneAt(ally.Location.Position);
                      if (zone == ally_zone) {
                        (ally.Controller as ObjectiveAI).SetObjective(new Goals.Cooperate(ally, do_this));
                        continue;
                      }
                    }
                  }
                }
                SetObjective(coordinate_this);
              }
              throw new InvalidOperationException("test case");
              return next_action;
            }
            throw new InvalidOperationException("test case");
          }
          throw new InvalidOperationException("test case");
        }
        // end function extraction target
        throw new InvalidOperationException("test case");
      }
#endif

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
