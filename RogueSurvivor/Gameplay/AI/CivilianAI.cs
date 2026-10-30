// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CivilianAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using djack.RogueSurvivor.Engine.Actions;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CivilianAI : OrderableAI, IJsonStaged<CivilianAI>
  {
    // JSON: saves $type only so far (the AI reloads with amnesia)
    static CivilianAI IJsonStaged<CivilianAI>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new CivilianAI(JsonOwner));

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

    private MemorizedSensor<LOSSensor> m_MemLOSSensor;
    private int m_SafeTurns;
    private ExplorationData m_Exploration = new ExplorationData();
    private string[] m_Emotes;

    public CivilianAI(Actor src) : base(src)
    {
      m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES, src), LOS_MEMORY);
      m_Emotes = FIGHT_EMOTES;
    }

    #region JSON save/load
    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      base.toJsonFields(writer, options);
      writer.WritePropertyName("MemLOS");
      JsonSerializer.Serialize(writer, m_MemLOSSensor, options);
      if (0 != m_SafeTurns) writer.WriteNumber("SafeTurns", m_SafeTurns);
      if (!m_Exploration.IsEmpty) {
        writer.WritePropertyName("Exploration");
        m_Exploration.toJson(writer, options);
      }
      if (!ReferenceEquals(m_Emotes, FIGHT_EMOTES)) writer.WriteArray("Emotes", m_Emotes, options);   // a unique's own
    }

    protected override void loadJsonFields(JsonElement src, JsonSerializerOptions options)
    {
      base.loadJsonFields(src, options);
      JsonElement x;
      m_MemLOSSensor = src.Require("MemLOS").Deserialize<MemorizedSensor<LOSSensor>>(options) ?? throw new JsonException("null MemLOS");
      if (src.TryGetProperty("SafeTurns", out x)) m_SafeTurns = x.GetInt32();
      if (src.TryGetProperty("Exploration", out x)) m_Exploration = new(x, options);
      if (src.TryGetProperty("Emotes", out x)) {
        var emotes = x.Deserialize<string[]>(options) ?? throw new JsonException("null Emotes");
        // the default is recognized by reference; a save that spelled it out (the binary-era saves did) must not keep spelling it out
        m_Emotes = emotes.SequenceEqual(FIGHT_EMOTES) ? FIGHT_EMOTES : emotes;
      }
    }

    protected override bool HasJsonState => base.HasJsonState || 0 != m_SafeTurns || !m_Exploration.IsEmpty || !ReferenceEquals(m_Emotes, FIGHT_EMOTES);
    #endregion

    public void InstallUniqueEmotes(string[] src)
    {
#if DEBUG
      if (!m_Actor.IsUnique) throw new InvalidOperationException("only uniques can get better emotes");
#endif
      m_Emotes = src;
    }

    public override List<Percept> UpdateSensors() => m_MemLOSSensor.Sense();
    public override HashSet<Point> FOV { get => m_MemLOSSensor.Sensor.FOV; }
    public override Location[] FOVloc { get => m_MemLOSSensor.Sensor.FOVloc; }
    public override Dictionary<Location, Actor>? friends_in_FOV { get => m_MemLOSSensor.Sensor.friends; }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get => m_MemLOSSensor.Sensor.enemies; }
    public override Dictionary<Location, Data.Model.InvOrigin>? items_in_FOV { get => m_MemLOSSensor.Sensor.items; }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(LivingActor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (CanSee(target.Location)) return "Come on {0}! Hurry up!";
      else return "Where the hell is {0}?";
    }

    protected override ActorAction? SelectAction()
    {
      var game = RogueGame.Game;

      ClearMovePlan();
      // \todo start extraction target: BehaviorEquipBestItems (cf RS Alpha 10)

      // start item juggling
      if (!BehaviorEquipPoliceRadio() && !BehaviorEquipCellPhone() && !BehaviorEquipLight()) {}
      // end extraction target: BehaviorEquipBestItems
      // end item juggling check
      _all = FilterSameMap(UpdateSensors());
      var current = FilterCurrent(_all);    // this tests fast
      ReviewItemRatings();  // XXX highly inefficient when called here; should "update on demand"

      const bool tracing = false;

      if (tracing) Logger.Trace(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+m_Actor.Location.Map.LocalTime.TurnCounter.ToString());
      m_Actor.Walk();    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (Order is not null) {
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "under orders");
        var actorAction = ExecuteOrder(Order, current);
        if (actorAction is not null) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "implementing orders");
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.Activity = Activity.IDLE; // backstop

      if (m_Actor.Location!=PrevLocation) {
        // sewers are not a good choice for default-tourism
        if (ItemMemory is not null && m_Actor.InterestingLocs is not null && !District.IsSewersMap(m_Actor.Location.Map)) {
          var _items = ItemMemory;
          var tourism = m_Actor.InterestingLocs;
          var map = m_Actor.Location.Map;
          m_Exploration.Update(m_Actor.Location,zone => {
            foreach (var pt in zone.Rect) if (!_items.HaveEverSeen(new Location(map, pt))) tourism.Record(map, in pt);
          });
        } else {
          m_Exploration.Update(m_Actor.Location);
        }
      }

      ExpireTaboos();
      InitAICache(current, _all);

      // get out of the range of explosions if feasible
      ActorAction? tmpAction = BehaviorFleeExplosives();
      if (tmpAction is not null) return tmpAction;

      _enemies = SortByGridDistance(FilterEnemies(current)); // this tests fast; makes InCombat valid

      // civilians track how long since they've seen trouble
      if (InCombat) m_SafeTurns = 0;
      else ++m_SafeTurns;

      // if we have no enemies and have not fled an explosion, our friends can see that we're safe
      if (_enemies is null) AdviseFriendsOfSafety();
      else {
        var seen = Rules.Get.DiceRoller.Choose(_enemies);
        m_LastEnemySaw = new(new ActorTag(seen.Percepted), seen.Turn, seen.Location);
      }

      // New objectives system
      if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, Objectives.Count.ToString() + " objectives");
      if (0<Objectives.Count) {
        foreach(Objective o in new List<Objective>(Objectives)) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, o.ToString()+(o.IsExpired ? " : expired" : ""));
          if (o.IsExpired) Remove(o);
          else if (o.UrgentAction(out var goal_action)) {
            if (tracing) {
              if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup(goal_action is null ? "expiring by null"  : "proceeding : "+ goal_action.ToString());
            }
            if (goal_action is null) Remove(o);
#if DEBUG
            else if (!goal_action.IsPerformable()) throw m_Actor.Spotlight(new InvalidOperationException("result of UrgentAction should be performable"));
#else
            else if (!goal_action.IsPerformable()) Objectives.Remove(o);
#endif
            else {
              if (tracing) {
                if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup("returning to task: " + o.ToString());
              }
              return goal_action;
            }
          } else if (tracing) Logger.WriteLine(Logger.Stage.RUN_DEBUG, "deferred goal " + o.ToString());
        }
      }

      if (tracing) {
        if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup(_enemies is null ? "null == _enemies" : _enemies.Count.ToString() + " enemies");
      }

      if (!(Directives_nocreate?.CanThrowGrenades ?? ActorDirective.CanThrowGrenades_default) && m_Actor.GetEquippedWeapon() is ItemGrenade grenade) grenade.UnequippedBy(m_Actor);

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

      tmpAction = CombinatoricCombat();
      if (tracing && tmpAction is not null) Logger.WriteLine(Logger.Stage.RUN_MAIN, "CombinatoricCombat: " + tmpAction.ToString());
      if (tmpAction is not null) return tmpAction;

      var available_ranged_weapons = m_Actor.GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(available_ranged_weapons);
      if (tmpAction is not null) {
        if (tracing) {
          if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup("managing melee risk: "+tmpAction.ToString());
        }
        return tmpAction;
      }

      if (_enemies is not null && (Directives_nocreate?.CanThrowGrenades ?? ActorDirective.CanThrowGrenades_default)) {
        tmpAction = BehaviorThrowGrenade();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "toss grenade");
          return tmpAction;
        }
      }

      // \todo doesn't handle properly interactions w/inventory management when enemies out of melee range; e.g. if have shotgun ammo, should take empty shotgun if not in immediate danger
      tmpAction = BehaviorEquipWeapon(available_ranged_weapons);
      if (tmpAction is not null) {
        if (tracing) {
          if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup("equip weapon: "+tmpAction.ToString());
        }
        return tmpAction;
      }

      var friends = FilterNonEnemies(current);
      if (_enemies is not null) {
        if (friends is not null) {
          var nearest_enemy = FilterNearest(_enemies);
          if (nearest_enemy is not null && Rules.Get.RollChance(50)) { // null check due to InferActor goal
            tmpAction = BehaviorWarnFriends(friends, nearest_enemy.Percepted);
            if (tmpAction is not null) {
              if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "warning friends");
              return tmpAction;
            }
          }
        }
        // \todo use damage_field to improve on BehaviorFightOrFlee
        tmpAction = BehaviorFightOrFlee((Directives_nocreate?.Courage ?? ActorDirective.Courage_default), m_Emotes, RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS);
        if (tmpAction is not null) {
            if (tracing) {
                if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup("fight-or-flee: "+tmpAction.ToString());
            }
            return tmpAction;
        }
      }
      // at this point, even if enemies are in sight we have no useful direct combat action

      tmpAction = NonCombatReflexMoves();
      if (tmpAction is not null) {
        if (tracing) {
          if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup("non-combat reflex: "+tmpAction.ToString());
        }
        return tmpAction;
      } else if (tracing) {
        if (!RogueGame.IsSimulating) RogueGame.Game.InfoPopup("non-combat reflex: null");
      }

      if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && (Directives_nocreate?.CanSleep ?? ActorDirective.CanSleep_default) && m_Actor.WantToSleepNow) {
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorNavigateToSleep");
        tmpAction = BehaviorNavigateToSleep();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "navigating to sleep");
          return tmpAction;
        }
      }

      var leader = m_Actor.LiveLeader;
      // attempting extortion from cops should have consequences.
      // XXX as should doing it to a civilian whose leader is a cop (and in communication)
      if (   RogueGame.Options.IsAggressiveHungryCiviliansOn
          && current is not null
          && leader is null
          && !m_Actor.Model.Abilities.IsLawEnforcer
          && (m_Actor.IsHungry
          && !m_Actor.Has<ItemFood>())) {
        var target = FilterNearest(current.FilterCast<Actor>(a =>
        {
          if (a == m_Actor || a.IsDead || a is not LivingActor mark || mark.Inventory.IsEmpty || mark.Leader == m_Actor || m_Actor.Leader == a)
            return false;
          if (mark.Inventory.Has<ItemFood>()) return true;
          return a.Location.Items?.Has<ItemFood>() ?? false;
        }));
        if (target is not null) {
          tmpAction = BehaviorChargeEnemy(target, true, true);
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "starving, attacking for food");
            if (Rules.Get.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              m_Actor.Say(target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
            if (!m_Actor.TargetActor.IsSleeping) {
              if (m_Actor.TargetActor.Faction.ID.ExtortionIsAggression()) {
                game.DoMakeAggression(m_Actor,m_Actor.TargetActor);
              } else if (m_Actor.TargetActor.Faction.ID.LawIgnoresExtortion()) {
                game.DoMakeAggression(m_Actor.TargetActor,m_Actor);
              } // XXX the target needs an AI modifier to handle this appropriately
            }
            RogueGame.PropagateSight(m_Actor.Location, a => {
              if ((a as LivingActor)?.Leader != m_Actor && m_Actor.Leader != a) {
                if (a.Faction.ID.ExtortionIsAggression()) {
                  a.Say(m_Actor, string.Format("ATTEMPTED MURDER! {0} HAS AGGRESSED {1}!", m_Actor.TheName, m_Actor.TargetActor.TheName), Sayflags.IS_IMPORTANT | Sayflags.IS_FREE_ACTION);
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
      if (tmpAction is not null) {
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using stench killer");
        return tmpAction;
      }

      bool? combat_unready = false;  // currently only matters for law enforcement
      if (m_Actor.Model.Abilities.IsLawEnforcer && !(combat_unready = CombatUnready()).Value) {
        tmpAction = BehaviorEnforceLaw();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law (combat ready)");
          return tmpAction;
        }
      }

      // XXX if we have item memory, check whether "critical items" have a known location.  If so, head for them (floodfill pathfinding)
      // XXX leaders should try to check what their followers use as well.
      var items = WhatHaveISeen();
      if (leader is null || DontFollowLeader) {
      if (items is not null && _legal_path is not null) {
        // BehaviorResupply has some pathing inefficency issues...it really should:
        // * full location goals, rather than map/point pairs
        // * index goals by which items are causing the goal to exist
        var critical = WhatDoINeedNow();    // out of ammo, or hungry without food
        // while we want to account for what our followers want, we don't want to block our followers from the items either
        critical.IntersectWith(items);
        if (0 < critical.Count) {
          // Unfortunately, this used to cause bizarrely long hang times in the sewers.  What we really want is some way to properly record
          // multi-pathing such that an ai that can reach an inventory target "first" excludes it from its allies' consideration
          // this is unstable if it's *JUST* critical items; include want to mitigate pathing loops
          var want = WhatDoIWantNow();
          want.IntersectWith(items);
          critical.UnionWith(want);
          if (tracing) {
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorResupply");
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "critical: "+critical.to_s());
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "previous goals: "+(GetPreviousGoals()?.to_s() ?? "null"));
          }
          tmpAction = BehaviorResupply(critical);
          if (tracing) {
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorResupply ok; "+(tmpAction?.ToString() ?? "null"));
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "goals: "+(GetPreviousGoals()?.to_s() ?? "null"));
          }
          if (tmpAction is not null) {
            _used_advanced_pathing = true; // object constancy
            return tmpAction;
          }
        }
      }
      }

      var rules = Rules.Get;
      if (rules.RollChance(BUILD_TRAP_CHANCE)) {
        tmpAction = BehaviorBuildTrap();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build trap");
          return tmpAction;
        }
      }

      if (rules.RollChance(BUILD_LARGE_FORT_CHANCE)) { // difference in relative ordering with soldiers is ok
        tmpAction = BehaviorBuildLargeFortification(START_FORT_LINE_CHANCE);
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build large fortification");
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (rules.RollChance(BUILD_SMALL_FORT_CHANCE)) {
        tmpAction = BehaviorBuildSmallFortification();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "build small fortification");
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      if (leader is not null && !DontFollowLeader) {
        // \todo interposition target for pathing hints, etc. from leader
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorFollowActor");
        tmpAction = BehaviorFollowActor(leader, 1);
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorFollowActor: "+(tmpAction?.ToString() ?? "null"));
        if (tmpAction is not null) {
          m_Actor.TargetedActivity(Activity.FOLLOWING, leader);
          return tmpAction;
        }
      } else {
        tmpAction = RecruitLOS();
        if (tmpAction is not null) return tmpAction;
        tmpAction = RecruitRadio();
        if (tmpAction is not null) return tmpAction;
      }
      // XXX if we are a leader, we should try to rearrange items for our followers (no one starving while another has a lot of food)
      // XXX if we are a follower, we should try to avoid being hurt by the leader's rearranging our items

      if (m_Actor.IsHungry) {
        tmpAction = BehaviorAttackBarricade();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for food behind barricade");
          game.DoEmote(m_Actor, "Open damn it! I know there is food there!", true);
          return tmpAction;
        }
        if (rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE)) {
          tmpAction = BehaviorPushNonWalkableObjectForFood();
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for food behind non-walkable objects");
            game.DoEmote(m_Actor, "Where is all the damn food?!", true);
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }
      tmpAction = BehaviorGoReviveCorpse(current);  // not logically CivilianAI only
      if (tmpAction is not null) {
        m_Actor.Activity = Activity.IDLE;
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "revive corpse");
        return tmpAction;
      }
      if (m_Actor.Threats is null && m_Actor.InterestingLocs is null) {
        if (rules.RollChance(USE_EXIT_CHANCE)) {
          tmpAction = BehaviorUseExit(UseExitFlags.DONT_BACKTRACK);
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "use exit for no good reason");
            return tmpAction;
          }
        }
      }

      var percept1 = current?.FilterFirst(p => {
          if (p.Percepted is not Actor actor || actor == m_Actor) return false;
          return actor.Controller is SoldierAI;
      });
      if (percept1 is not null) m_LastSoldierSaw = new(new ActorTag((Actor)percept1.Percepted), percept1.Turn, percept1.Location);

      if (m_Actor.Model.Abilities.IsLawEnforcer && combat_unready.Value) {
        tmpAction = BehaviorEnforceLaw();
        if (tmpAction is not null) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "enforcing law (combat unready)");
          return tmpAction;
        }
      }

      // XXX civilians that start in a boarded-up building (sewer maintenance, gun shop, hardware store
      // should stay there until they get the all-clear from the police

      // The newer movement behaviors using floodfill pathing, etc. depend on there being legal walking moves
#region floodfill pathfinder
      if (_legal_path is not null) {
        if (!InCombat) {    // want to recompute if threat tracking active within view rectangle
          tmpAction = UsePreexistingLambdaPath();
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "using pre-existing path");
            return tmpAction;
          }
        }

        // advanced pathing ultimately reduces to various flavors of calls to (specializations) of 
        // public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering advanced pathing");
        if (combat_unready is null) combat_unready = CombatUnready();
        HashSet<Gameplay.Item_IDs> want = (items is not null ? WhatDoIWantNow() : new());    // non-emergency things
        // while we want to account for what our followers want, we don't want to block our followers from the items either
        if (items is not null) want.IntersectWith(items);
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "want: "+want.to_s());

        // 2019-01-31: historical range sorting is:, This map, other maps [old implementation pruned 2019-03-18]
        // However, this equates the district entry map with the much smaller basement, and does not cope well with space-time scaling or the cross-district minimap
        // what would make sense is: local, radio range (minimap), "world" (last may not need immediate implementing, other maps may do for now)
        // local is the viewport for large maps, and the map for small maps (CHAR Underground base is "large" but does not cross-district path)
        // radio range is everything that fits on the minimap; distinct from local only for large maps
        // convention: a null map has been blacklisted
        // if the final return value is null, we know the map was blacklisted and do not need to expand from it
        var update_path = ForceLambdaPath();
        Func<Map,HashSet<Point>>? pathing_targets = null;
        ThreatTracking? threats = m_Actor.Threats;

        HashSet<Point> hunt_threat(Map m) {
          if (District.IsSewersMap(m) && Session.Get.HasZombiesInSewers) return new HashSet<Point>(); // unclearable
          var ret = threats.ThreatWhere(m);
          if (0<ret.Count) {
            update_path.StageView(m,ret);
          }
          return ret;
        }

        if (!combat_unready.Value && threats is not null && threats.Any()) pathing_targets = hunt_threat;

        var sights_to_see = m_Actor.InterestingLocs;
        HashSet<Point> tourism(Map m) {
          var ret = sights_to_see.In(m);
          if (0<ret.Count) {
            update_path.StageView(m,ret);
          }
          return ret;
        }
        if (sights_to_see is not null) pathing_targets = pathing_targets.Otherwise(tourism);

        // police want to exclude threat/tourism in indoor zones already covered by leader-types (other handling will cover engaged threat)
        var allies = m_Actor.Allies;
        void already_handled(Map m, HashSet<Point> target) {
          foreach(var a in allies) {
            if (a.HasLeader && 0 >= a.CountFollowers) continue; // bottom of chain of command, no authority to clear zones on own
            if (a.IsSleeping) continue; // not expected to do anything useful
            var handled = (a.Controller as ObjectiveAI)?.ClearingThisZone();
            if (handled is null) continue;
            if (handled.m != m) continue;
            bool reject(Point pt) { return handled.Rect.Contains(pt); }
            target.RemoveWhere(reject);
            update_path.UnStageView(m,reject);
          }
        }
        if (allies is not null) pathing_targets.Postfilter(already_handled);

        HashSet<Point> generators(Map m) {
          var gens = Generators(m);
          if (gens is null) return new HashSet<Point>();
          if (WantToRecharge()) {
            update_path.StageGenerators(gens);
            return m_Actor.Model.CastToBumpableDestinations(m,gens.Select(obj => obj.Location.Position));
          }
          var gens_off = gens.Where(obj => !obj.IsOn);
          if (gens_off.Any()) {
            update_path.StageGenerators(gens_off);
            return m_Actor.Model.CastToBumpableDestinations(m, gens_off.Select(obj => obj.Location.Position));   // XXX should be for map
          }
          return new HashSet<Point>();
        }

        if (HasBehaviorThatRecallsToSurface) pathing_targets = pathing_targets.Union(generators);

        HashSet<Point> resupply_want(Map m)
        {
          var ret = WhereIs(want, m);
          if (0 < ret.Count) update_path.StageInventory(m,ret);
          return m_Actor.Model.CastToInventoryAccessibleDestinations(m,ret);
        }

        if (0 < want.Count) pathing_targets = pathing_targets.Union(resupply_want);
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing targets: "+(pathing_targets is null ? "null" : "non-null"));
        if (pathing_targets is not null) {
          var view = m_Actor.Location.View;
          var d_span = view.DistrictSpan;
          int map_code = District.UsesCrossDistrictView(m_Actor.Location.Map);

          // The prefilter functions are going into HashSet<>.RemoveWhere so they have to return false to accept, true to reject
          bool prefilter_view(Map m) {
            if (m==m_Actor.Location.Map) return false;
            if (0 >= map_code) return true;
            if (map_code != District.UsesCrossDistrictView(m)) return true;
            return !d_span.Contains(m.DistrictPos);
          }

          // these two may need to be new parameters for BehaviorPathTo
          bool reject_view(Location loc) { return !view.Contains(in loc); }

          // 1) view pathing
          _caller = CallChain.SelectAction_LambdaPath;
          tmpAction = BehaviorPathTo(pathing_targets,prefilter_view, reject_view);
          _caller = CallChain.NONE;
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within view: "+(tmpAction?.ToString() ?? "null"));
          if (tmpAction is not null) return tmpAction;
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
              if (!d_span.Contains(m.DistrictPos)) return true;
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
              if (unique_m.NavigatePoliceStation(m) is not null) return !unique_m.PoliceStation_OfficesLevel.TheMap.AnyExits(e_for_surface);
              else if (unique_m.NavigateHospital(m) is not null) return !unique_m.Hospital_Admissions.TheMap.AnyExits(e_for_surface);
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
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within minimap: "+(tmpAction?.ToString() ?? "null"));
            // object constancy: remember that we're going somewhere on the destination level
            if (tmpAction is ActionUseExit leaving) {
                if (   leaving.dest.Map.DistrictPos == m_Actor.Location.Map.DistrictPos
                    && 1 == District.UsesCrossDistrictView(m_Actor.Location.Map) && 2 != District.UsesCrossDistrictView(leaving.dest.Map)) {
                    var dest_view = District.UsesCrossDistrictView(leaving.dest.Map);
                    if (2 != dest_view) {
                        HashSet<Location>? dests = null;
                        if (0<Session.Get.UniqueMaps.HospitalDepth(leaving.dest.Map)) dests = update_path.Unstage(loc => 0 < Session.Get.UniqueMaps.HospitalDepth(loc.Map));
                        if (0<Session.Get.UniqueMaps.PoliceStationDepth(leaving.dest.Map)) dests = update_path.Unstage(loc => 0 < Session.Get.UniqueMaps.PoliceStationDepth(loc.Map));
                        if (0 >= dest_view) dests = update_path.Unstage(loc => leaving.dest.Map != loc.Map);
                        else dests = update_path.Unstage(loc => dest_view != District.UsesCrossDistrictView(loc.Map));
                        if (0 < dests.Count) {
                            if (tracing) {
                                Logger.WriteLine(Logger.Stage.RUN_DEBUG, "LoS on cross-district dests: " + dests.to_s());
                            }
                            var goal = new Goals.AcquireLineOfSight(m_Actor, dests);
                            SetObjective(goal);
                            AddFOVevent(goal);
                            return tmpAction;
                        }
                    }
                }
            } else if (tmpAction is ActionLeaveMap crossdistrict_exit) {
                var dest_view = District.UsesCrossDistrictView(crossdistrict_exit.dest.Map);
                if (2 != dest_view) {
                    var dests = update_path.Unstage(loc => dest_view != District.UsesCrossDistrictView(loc.Map));
                    if (0 < dests.Count) {
                        if (tracing) {
                            Logger.WriteLine(Logger.Stage.RUN_DEBUG, "LoS on dests: " + dests.to_s());
                        }
                        var goal = new Goals.AcquireLineOfSight(m_Actor, dests);
                        SetObjective(goal);
                        AddFOVevent(goal);
                        return tmpAction;
                    }
                }
            }

            if (tmpAction is not null) {
              var considering = GetPreviousGoals(m_Actor.Location);
              var unseen = considering?.Where(loc => !CanSee(loc)).ToHashSet();
              if (unseen is not null && 0 < unseen.Count) {
                if (tracing) {
                  Logger.WriteLine(Logger.Stage.RUN_DEBUG, "LoS on unseen: " + unseen.to_s());
                }
                var goal = new Goals.AcquireLineOfSight(m_Actor, unseen);
                SetObjective(goal);
                AddFOVevent(goal);
              }
              return tmpAction;
            }

            update_path.ForgetStaging();
          }
          // 3) world pathing (no prefilter/postfilter, ok to hunt threat even if combat unready)
          if (combat_unready.Value && threats is not null && threats.Any()) pathing_targets = pathing_targets.Otherwise(hunt_threat);

          _caller = CallChain.SelectAction_LambdaPath;
          tmpAction = BehaviorPathTo(pathing_targets);
          _caller = CallChain.NONE;
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "pathing within world: "+(tmpAction?.ToString() ?? "null"));
          if (tmpAction is not null) return tmpAction;
          update_path.ForgetStaging();
        }
      }
#endregion

      if (friends is not null) {
        if (m_LastRaidHeard is not null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastRaidHeard, TELL_FRIEND_ABOUT_RAID_CHANCE);
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about raid");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastSoldierSaw is not null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastSoldierSaw, TELL_FRIEND_ABOUT_SOLDIER_CHANCE);
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about soldier");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastEnemySaw is not null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastEnemySaw, TELL_FRIEND_ABOUT_ENEMY_CHANCE);
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about enemy");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
        if (m_LastItemsSaw is not null) {
          tmpAction = BehaviorTellFriendAboutPercept(m_LastItemsSaw, TELL_FRIEND_ABOUT_ITEMS_CHANCE);
          if (tmpAction is not null) {
            if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "chat about items");
            m_Actor.Activity = Activity.IDLE;
            return tmpAction;
          }
        }
      }

      if (m_Actor.CountFollowers > 0) {
        if (BehaviorDontLeaveFollowersBehind(2) is (var act, var target)) {
          if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "dont't leave followers");
          if (rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return act;
        }
      }

      // Try to reuse previous goals before explore/wander 2020-11-20 zaimoni
      var prev_goals = GetPreviousGoals(m_Actor.Location);
      if (prev_goals is not null) {
          var ok = prev_goals.Where(loc => loc.IsWalkableFor(m_Actor));
          if (ok.Any()) {
            if (ok.Count() < prev_goals.Count) prev_goals = ok.ToHashSet();
            tmpAction = BehaviorPathTo(prev_goals);
            if (tmpAction is not null) {
              m_Actor.Activity = Activity.IDLE;
              return tmpAction;
            }
          }
          tmpAction = BehaviorMakeTime();
          if (tmpAction is not null) return tmpAction;
          return new ActionWait(m_Actor);
      }

#if FALSE_POSITIVE
      if (m_Actor.Threats is not null || m_Actor.InterestingLocs is not null) {
        Session.Get.World.DaimonMap(); // for accuracy
        throw new InvalidOperationException("test case");
      }
#endif

      tmpAction = BehaviorExplore(m_Exploration);
      if (tmpAction is not null) {
        if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "unguided exploration");
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, "wandering");
      return BehaviorWander(m_Exploration);
    }
  }
}
