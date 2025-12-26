// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CHARGuardAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_SELECTACTION

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CHARGuardAI : OrderableAI
  {
    private static readonly string[] FIGHT_EMOTES = new string[3]
    {
      "Go away",
      "Damn it I'm trapped!",
      "Hey"
    };
    private const int LOS_MEMORY = WorldTime.TURNS_PER_HOUR/3;
    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS;

    private readonly MemorizedSensor<LOSSensor> m_MemLOSSensor;

    private List<Actor> _squad = null;

    public CHARGuardAI(Actor src) : base(src)
    {
      m_MemLOSSensor = new MemorizedSensor<LOSSensor>(new LOSSensor(VISION_SEES, src), LOS_MEMORY);
    }

    public override bool UsesExplosives { get => false; }

#nullable enable
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
#nullable restore

    protected override ActorAction SelectAction()
    {
      ClearMovePlan();

      _all = FilterSameMap(UpdateSensors());

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+m_Actor.Location.Map.LocalTime.TurnCounter.ToString());
#endif
      m_Actor.Walk();    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (null != Order) {
        var actorAction = ExecuteOrder(Order, _all);
        if (null != actorAction) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.Activity = Activity.IDLE; // backstop

      InitAICache(_all);

      var old_enemies = FilterEnemies(_all);
      _enemies = SortByGridDistance(FilterCurrent(old_enemies));
      if (null == _enemies) AdviseFriendsOfSafety();

//    const bool tracing = false; // debugging hook
      bool tracing = "Gd. Joseph Thomas" == m_Actor.TheName; // debugging hook

      if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, Objectives.Count.ToString()+" objectives");
      // New objectives system
      if (0<Objectives.Count) {
        ActorAction goal_action = null;
        foreach(Objective o in new List<Objective>(Objectives)) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && !o.IsExpired)  Logger.WriteLine(Logger.Stage.RUN_MAIN, o.ToString());
#endif
          if (o.IsExpired) Objectives.Remove(o);
          else if (o.UrgentAction(out goal_action)) {
            if (tracing) RogueGame.Game.InfoPopup(o.ToString()+": "+(goal_action?.ToString() ?? "null"));

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

      // Mysteriously, CHAR guards do not throw grenades even though their offices stock them.

      ActorAction tmpAction = null;

      if (tracing) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == _enemies ? "null == _enemies" : _enemies.Count.ToString()+" enemies"));

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      List<Engine.Items.ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(available_ranged_weapons);
      if (tracing && null != tmpAction) RogueGame.Game.InfoPopup("ManageMeleeRisk: "+tmpAction.ToString());
      if (null != tmpAction) return tmpAction;

      tmpAction = BehaviorEquipWeapon(available_ranged_weapons);
      if (tracing && null != tmpAction) RogueGame.Game.InfoPopup("BehaviorEquipWeapon: "+tmpAction.ToString());
      if (null != tmpAction) return tmpAction;

      if (null != _enemies) {
        tmpAction = BehaviorFightOrFlee(ActorCourage.COURAGEOUS, FIGHT_EMOTES, RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS);
        if (tracing && null != tmpAction) RogueGame.Game.InfoPopup("BehaviorFightOrFlee: "+tmpAction.ToString());
        if (null != tmpAction) return tmpAction;
      }
      // at this point, even if enemies are in sight we have no useful direct combat action

      // Secure CHAR property
	  var friends = FilterNonEnemies(_all);
      if (null != friends) {
        var percepts3 = friends?.Filter(p =>
        {
          Actor actor = p.Percepted;
          return !actor.IsFaction(GameFactions.IDs.TheCHARCorporation) && RogueGame.IsInCHARProperty(actor.Location) && p.Turn == m_Actor.Location.Map.LocalTime.TurnCounter; // alpha10 bug fix only if visible right now!
        });
        if (percepts3 != null) {
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
      if (null != _enemies) {
        if (null != friends) {
          var nearest_enemy = FilterNearest(_enemies);
          if (null != nearest_enemy && Rules.Get.RollChance(50)) { // null check due to InferActor goal
            tmpAction = BehaviorWarnFriends(friends, nearest_enemy.Percepted);
#if TRACE_SELECTACTION
            if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "warning friends");
#endif
            if (null != tmpAction) return tmpAction;
          }
        }
      }

      tmpAction = NonCombatReflexMoves();
      if (null != tmpAction) return tmpAction;

      if (old_enemies != null) {
        var target = FilterNearest(old_enemies);
        if (m_Actor.Location == target.Location) {
          Actor actor = target.Percepted;
          target = new Percept_<Actor>(actor, m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);  // XXX inerrant tracking \todo fix
        }
        if (CanReachSimple(target.Location, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP)) {
          tmpAction = BehaviorChargeEnemy(target,false,false);
          if (null != tmpAction) return tmpAction;
        }
      }

      if (null == old_enemies && m_Actor.WantToSleepNow) {
        tmpAction = BehaviorNavigateToSleep();
        if (null != tmpAction) return tmpAction;
      }

      var leader = m_Actor.LiveLeader;
      if (null != leader && !DontFollowLeader) {
        tmpAction = BehaviorFollowActor(leader, 1);
        if (null != tmpAction) {
          m_Actor.TargetedActivity(Activity.FOLLOWING, leader);
          return tmpAction;
        }
      } else  {
        tmpAction = RecruitLOS();
        if (null != tmpAction) return tmpAction;
      }

      // critical item memory check goes here

      // possible we don't want CHAR guard leadership at all.  The stay-near-leader behavior doesn't fit, regardless (would go here)

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorWander(null, RogueGame.IsInCHAROffice);
      if (null != tmpAction) return tmpAction;
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
