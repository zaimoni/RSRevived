// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CHARGuardAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CHARGuardAI : OrderableAI, IJsonStaged<CHARGuardAI>
  {
    // JSON: saves $type only so far (the AI reloads with amnesia)
    static CHARGuardAI IJsonStaged<CHARGuardAI>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new CHARGuardAI(JsonOwner));

    private static readonly string[] FIGHT_EMOTES = new string[3]
    {
      "Go away",
      "Damn it I'm trapped!",
      "Hey"
    };
    private const int LOS_MEMORY = WorldTime.TURNS_PER_HOUR/3;
    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS;

    private MemorizedSensor<LOSSensor> m_MemLOSSensor;

    private List<Actor>? _squad = null;

    public CHARGuardAI(Actor src) : base(src)
    {
      m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES, src), LOS_MEMORY);
    }

    #region JSON save/load
    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      base.toJsonFields(writer, options);
      writer.WritePropertyName("MemLOS");
      JsonSerializer.Serialize(writer, m_MemLOSSensor, options);
      if (_squad is not null && 0 < _squad.Count) writer.WriteRefs("Squad", _squad);
    }

    protected override void loadJsonFields(JsonElement src, JsonSerializerOptions options)
    {
      base.loadJsonFields(src, options);
      JsonElement x;
      m_MemLOSSensor = src.Require("MemLOS").Deserialize<MemorizedSensor<LOSSensor>>(options) ?? throw new JsonException("null MemLOS");
      if (src.TryGetProperty("Squad", out x)) x.ReadRefs(_squad = new());
    }

    protected override bool HasJsonState { get {
      _squad = _squad?.Optimize();
      return base.HasJsonState || (_squad is not null && 0 < _squad.Count);
    } }
    #endregion

    public override bool UsesExplosives { get => false; }

    public override List<Percept> UpdateSensors() => m_MemLOSSensor.Sense();
    public override HashSet<Point> FOV { get => m_MemLOSSensor.Sensor.FOV; }
    public override Location[] FOVloc { get => m_MemLOSSensor.Sensor.FOVloc; }
    public override Dictionary<Location, Actor>? friends_in_FOV { get => m_MemLOSSensor.Sensor.friends; }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get => m_MemLOSSensor.Sensor.enemies; }
    public override Dictionary<Location, Data.Model.InvOrigin>? items_in_FOV { get => m_MemLOSSensor.Sensor.items; }

    public override string AggressedBy(Actor aggressor)
    {
      if (aggressor.IsFaction(GameFactions.IDs.ThePolice) && 1 > Session.Get.ScriptStage_PoliceCHARrelations) {
        // same technical issues as DoMakeEnemyOfCop
        Session.Get.ScriptStage_PoliceCHARrelations = 1;
//      GameFactions.ThePolice.AddEnemy(GameFactions.TheCHARCorporation);   // works here, but parallel when loading the game doesn't
        RogueGame.DamnCHARtoPoliceInvestigation();
        return "Just following ORDERS!";
      }
      return base.AggressedBy(aggressor);
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

      InitAICache(_all);

      var old_enemies = FilterEnemies(_all);
      _enemies = SortByGridDistance(FilterCurrent(old_enemies));
      if (_enemies is null) AdviseFriendsOfSafety();

      const bool tracing = false; // debugging hook

      if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, Objectives.Count.ToString()+" objectives");
      // New objectives system
      if (0<Objectives.Count) {
        foreach(Objective o in new List<Objective>(Objectives)) {
          if (o.IsExpired) Remove(o);
          else if (o.UrgentAction(out var goal_action)) {
            if (tracing) RogueGame.Game.InfoPopup(o.ToString()+": "+(goal_action?.ToString() ?? "null"));

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

      // Mysteriously, CHAR guards do not throw grenades even though their offices stock them.

      ActorAction? tmpAction = null;

      if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, (_enemies is null ? "null == _enemies" : _enemies.Count.ToString()+" enemies"));

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
      if (tracing && tmpAction is not null) Logger.WriteLine(Logger.Stage.RUN_MAIN, "CombinatoricCombat: " + tmpAction.ToString());
      if (tmpAction is not null) return tmpAction;

      var available_ranged_weapons = m_Actor.GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(available_ranged_weapons);
      if (tracing && tmpAction is not null) RogueGame.Game.InfoPopup("ManageMeleeRisk: "+tmpAction.ToString());
      if (tmpAction is not null) return tmpAction;

      tmpAction = BehaviorEquipWeapon(available_ranged_weapons);
      if (tracing && tmpAction is not null) RogueGame.Game.InfoPopup("BehaviorEquipWeapon: "+tmpAction.ToString());
      if (tmpAction is not null) return tmpAction;

      if (_enemies is not null) {
        tmpAction = BehaviorFightOrFlee(ActorCourage.COURAGEOUS, FIGHT_EMOTES, RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS);
        if (tracing && tmpAction is not null) RogueGame.Game.InfoPopup("BehaviorFightOrFlee: "+tmpAction.ToString());
        if (tmpAction is not null) return tmpAction;
      }
      // at this point, even if enemies are in sight we have no useful direct combat action

      // Secure CHAR property
      var friends = FilterNonEnemies(_all);
      if (friends is not null) {
        var percepts3 = friends?.Filter(p =>
        {
          Actor actor = p.Percepted;
          return !actor.IsFaction(GameFactions.IDs.TheCHARCorporation) && RogueGame.IsInCHARProperty(actor.Location) && p.Turn == m_Actor.Location.Map.LocalTime.TurnCounter; // alpha10 bug fix only if visible right now!
        });
        if (percepts3 is not null) {
          Actor target = FilterNearest(percepts3).Percepted;
          // Now that we can get crowds of civilians, they should react to seeing others betrayed
          // also note that the CHAR armor comes with an inbuilt CHAR radio (they invented the hyper-efficient radios the police and army use)
          // however, CHAR guards are on zone defense so the immediate aggression is just the current CHAR office (underground base is whole base, however)
          Aggress(target);
          // betrayal reaction
          foreach(var witness in percepts3) {
            Actor a = witness.Percepted;
            if (a == target) continue;
            if (a.IsSleeping) continue;
            // XXX cheat...assume symmetric visibility
            Aggress(a);
          }
          // XXX should have some reaction from witnesses that weren't aggressed
          m_Actor.TargetedActivity(Activity.FIGHTING, target);
          // players are special: they get to react to being aggressed
          return new ActionSay(m_Actor, target, "Hey YOU!", (target.IsPlayer ? Sayflags.IS_IMPORTANT | Sayflags.IS_DANGER : Sayflags.IS_IMPORTANT | Sayflags.IS_DANGER | Sayflags.IS_FREE_ACTION));
        }
      }
      if (_enemies is not null) {
        if (friends is not null) {
          var nearest_enemy = FilterNearest(_enemies);
          if (nearest_enemy is not null && Rules.Get.RollChance(50)) { // null check due to InferActor goal
            tmpAction = BehaviorWarnFriends(friends, nearest_enemy.Percepted);
            if (tmpAction is not null) return tmpAction;
          }
        }
      }

      tmpAction = NonCombatReflexMoves();
      if (tmpAction is not null) return tmpAction;

      if (old_enemies is not null) {
        var target = FilterNearest(old_enemies);
        if (m_Actor.Location == target.Location) {
          Actor actor = target.Percepted;
          target = new Percept_<Actor>(actor, m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);  // XXX inerrant tracking \todo fix
        }
        if (CanReachSimple(target.Location, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP)) {
          tmpAction = BehaviorChargeEnemy(target,false,false);
          if (tmpAction is not null) return tmpAction;
        }
      }

      if (old_enemies is null && m_Actor.WantToSleepNow) {
        tmpAction = BehaviorNavigateToSleep();
        if (tmpAction is not null) return tmpAction;
      }

      var leader = m_Actor.LiveLeader;
      if (leader is not null && !DontFollowLeader) {
        tmpAction = BehaviorFollowActor(leader, 1);
        if (tmpAction is not null) {
          m_Actor.TargetedActivity(Activity.FOLLOWING, leader);
          return tmpAction;
        }
      } else  {
        tmpAction = RecruitLOS();
        if (tmpAction is not null) return tmpAction;
      }

      // critical item memory check goes here

      // possible we don't want CHAR guard leadership at all.  The stay-near-leader behavior doesn't fit, regardless (would go here)

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorWander(null, RogueGame.IsInCHAROffice);
      if (tmpAction is not null) return tmpAction;
      // \todo path to CHAR office?
      return BehaviorWander();
    }

    private void Aggress(Actor target)
    {
      var game = RogueGame.Game;
      game.DoMakeAggression(m_Actor, target);   // XXX needs to be more effective
      foreach(var guard in _squad) {
        if (m_Actor==guard) continue;
        if (guard.IsDead || guard.IsSleeping) continue;
        game.DoMakeAggression(guard, target);
      }
    }

    static public void DeclareSquad(List<Actor> squad)  // used in map creation
    {
      foreach(Actor a in squad) {
        if (a.Controller is CHARGuardAI ai) ai._squad = squad;
      }
    }
  }
}
