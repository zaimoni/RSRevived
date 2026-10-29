// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SoldierAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SoldierAI : OrderableAI, IJsonStaged<SoldierAI>
  {
    // JSON: saves $type only so far (the AI reloads with amnesia)
    static SoldierAI IJsonStaged<SoldierAI>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new SoldierAI(JsonOwner));

    private static readonly string[] FIGHT_EMOTES = new string[3]
    {
      "Damn",
      "Fuck I'm cornered",
      "Die"
    };
    private const int LOS_MEMORY = WorldTime.TURNS_PER_HOUR/3;
    private const int FOLLOW_LEADER_MIN_DIST = 1;
    private const int FOLLOW_LEADER_MAX_DIST = 2;
    private const int BUILD_SMALL_FORT_CHANCE = 20;
    private const int BUILD_LARGE_FORT_CHANCE = 50;
    private const int START_FORT_LINE_CHANCE = 1;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS;

    private MemorizedSensor<LOSSensor> m_MemLOSSensor;
    private ExplorationData m_Exploration = new ExplorationData();

    public SoldierAI(Actor src) : base(src)
    {
      m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES, src), LOS_MEMORY);
    }

    #region JSON save/load
    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      base.toJsonFields(writer, options);
      writer.WritePropertyName("MemLOS");
      JsonSerializer.Serialize(writer, m_MemLOSSensor, options);
      if (!m_Exploration.IsEmpty) {
        writer.WritePropertyName("Exploration");
        m_Exploration.toJson(writer, options);
      }
    }

    protected override void loadJsonFields(JsonElement src, JsonSerializerOptions options)
    {
      base.loadJsonFields(src, options);
      JsonElement x;
      m_MemLOSSensor = src.Require("MemLOS").Deserialize<MemorizedSensor<LOSSensor>>(options) ?? throw new JsonException("null MemLOS");
      if (src.TryGetProperty("Exploration", out x)) m_Exploration = new(x, options);
    }

    protected override bool HasJsonState => base.HasJsonState || !m_Exploration.IsEmpty;
    #endregion

    public override List<Percept> UpdateSensors()
    {
      return m_MemLOSSensor.Sense();
    }

    public override HashSet<Point> FOV { get => m_MemLOSSensor.Sensor.FOV; }
    public override Location[] FOVloc { get => m_MemLOSSensor.Sensor.FOVloc; }
    public override Dictionary<Location, Actor>? friends_in_FOV { get => m_MemLOSSensor.Sensor.friends; }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get => m_MemLOSSensor.Sensor.enemies; }
    public override Dictionary<Location, Data.Model.InvOrigin>? items_in_FOV { get => m_MemLOSSensor.Sensor.items; }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(LivingActor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (CanSee(target.Location)) return "{0}! Don't lag behind!";
      else return "Where the hell is {0}?";
    }

    protected override ActorAction? SelectAction()
    {
      ClearMovePlan();

      _all = FilterSameMap(UpdateSensors());

      m_Actor.Walk();    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (Order is not null) {
        var actorAction = ExecuteOrder(Order, _all);
        if (actorAction is not null) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.Activity = Activity.IDLE; // backstop

      if (m_Actor.Location!=PrevLocation) m_Exploration.Update(m_Actor.Location);
      InitAICache(_all, _all);

      // get out of the range of explosions if feasible
      var tmpAction = BehaviorFleeExplosives();
      if (tmpAction is not null) return tmpAction;

      var old_enemies = FilterEnemies(_all);
      _enemies = SortByGridDistance(FilterCurrent(old_enemies));

      // if we have no enemies and have not fled an explosion, our friends can see that we're safe
      if (_enemies is null) AdviseFriendsOfSafety();

      // New objectives system
      if (0<Objectives.Count) {
        foreach(Objective o in new List<Objective>(Objectives)) {
          if (o.IsExpired) Remove(o);
          else if (o.UrgentAction(out var goal_action)) {
            if (goal_action is null) Remove(o);
#if DEBUG
            else if (!goal_action.IsPerformable()) throw m_Actor.Spotlight(new InvalidOperationException("result of UrgentAction should be legal"));
#else
            else if (!goal_action.IsPerformable()) Objectives.Remove(o);
#endif
            else return goal_action;
          }
        }
      }

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      tmpAction = CombinatoricCombat();
//    if (tracing && null != tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "CombinatoricCombat: " + tmpAction.ToString());
      if (tmpAction is not null) return tmpAction;

      var available_ranged_weapons = m_Actor.GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(available_ranged_weapons);
      if (tmpAction is not null) return tmpAction;

      if (_enemies is not null) {
        tmpAction = BehaviorThrowGrenade();
        if (tmpAction is not null) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(available_ranged_weapons);
      if (tmpAction is not null) return tmpAction;

      // all free actions have to be before targeting enemies

      var friends = FilterNonEnemies(_all);
      if (_enemies is not null) {
        if (friends is not null) {
          var nearest_enemy = FilterNearest(_enemies);
          if (nearest_enemy is not null && Rules.Get.RollChance(50)) { // null check due to InferActor goal
            tmpAction = BehaviorWarnFriends(friends, nearest_enemy.Percepted);
            if (tmpAction is not null) return tmpAction;
          }
        }
        tmpAction = BehaviorFightOrFlee(ActorCourage.COURAGEOUS, FIGHT_EMOTES, RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS);
        if (tmpAction is not null) return tmpAction;
      }
      // at this point, even if enemies are in sight we have no useful direct combat action

      tmpAction = NonCombatReflexMoves();
      if (tmpAction is not null) return tmpAction;

      if (old_enemies is not null) {
        tmpAction = BehaviorChargeEnemy(FilterNearest(old_enemies), false, false);
        if (tmpAction is not null) return tmpAction;
      }

      if (old_enemies is null && m_Actor.WantToSleepNow) {
        tmpAction = BehaviorNavigateToSleep();
        if (tmpAction is not null) return tmpAction;
      }

      var rules = Rules.Get;
      if (rules.RollChance(BUILD_LARGE_FORT_CHANCE)) {
        tmpAction = BehaviorBuildLargeFortification(START_FORT_LINE_CHANCE);
        if (tmpAction is not null) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (rules.RollChance(BUILD_SMALL_FORT_CHANCE)) {
        tmpAction = BehaviorBuildSmallFortification();
        if (tmpAction is not null) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      var leader = m_Actor.LiveLeader;
      if (leader is not null && !DontFollowLeader) {
        tmpAction = BehaviorHangAroundActor(leader, FOLLOW_LEADER_MIN_DIST, FOLLOW_LEADER_MAX_DIST);    // SoldierAI difference here probably ok
        if (tmpAction is not null) {
          m_Actor.TargetedActivity(Activity.FOLLOWING, leader);
          return tmpAction;
        }
      } else  {
        tmpAction = RecruitLOS();
        if (tmpAction is not null) return tmpAction;
      }

      // critical item memory check goes here

      if (m_Actor.CountFollowers > 0) {
        if (BehaviorDontLeaveFollowersBehind(4) is (var act, var target)) {
          if (rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            RogueGame.Game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return act;
        }
      }

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorExplore(m_Exploration);
      if (tmpAction is not null) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      return BehaviorWander(m_Exploration);
    }
  }
}
