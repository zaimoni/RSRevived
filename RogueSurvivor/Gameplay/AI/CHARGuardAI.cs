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
using System.Drawing;
using System.Linq;

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

    private readonly MemorizedSensor m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);

    private List<Actor> _squad = null;

    public CHARGuardAI()
    {
    }

    public override bool UsesExplosives { get { return false; } }

    public override void OptimizeBeforeSaving()
    {
      m_MemLOSSensor.Forget(m_Actor);
    }

    public override List<Percept> UpdateSensors()
    {
      return m_MemLOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).FOV; } }
    public override Dictionary<Point,Actor> friends_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).friends; } }
    public override Dictionary<Point,Actor> enemies_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).enemies; } }
    public override Dictionary<Location, Inventory> items_in_FOV { get { return (m_MemLOSSensor.Sensor as LOSSensor).items; } }
    protected override void SensorsOwnedBy(Actor actor) { (m_MemLOSSensor.Sensor as LOSSensor).OwnedBy(actor); }

    public override string AggressedBy(Actor aggressor)
    {
      if (GameFactions.ThePolice == aggressor.Faction && 1 > Session.Get.ScriptStage_PoliceCHARrelations) {
        // same technical issues as DoMakeEnemyOfCop
        Session.Get.ScriptStage_PoliceCHARrelations = 1;
//      GameFactions.ThePolice.AddEnemy(GameFactions.TheCHARCorporation);   // works here, but parallel when loading the game doesn't
        RogueGame.DamnCHARtoPoliceInvestigation();
        return "Just following ORDERS!";
      }
      return base.AggressedBy(aggressor);
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      ClearMovePlan();
      BehaviorEquipBestBodyArmor();

      List<Percept> percepts_all = FilterSameMap(UpdateSensors());

      m_Actor.IsRunning = false;    // alpha 10: don't run by default

      // OrderableAI specific: respond to orders
      if (null != Order) {
        ActorAction actorAction = ExecuteOrder(game, Order, percepts_all);
        if (null != actorAction) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.Activity = Activity.IDLE; // backstop

      // New objectives system
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
            else return goal_action;
          }
        }
      }

      // Mysteriously, CHAR guards do not throw grenades even though their offices stock them.
      List<Percept> old_enemies = FilterEnemies(percepts_all);
      List<Percept> current_enemies = SortByGridDistance(FilterCurrent(old_enemies));

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      InitAICache(percepts_all);

      // XXX the proper weapon should be calculated like a player....
      // range 1: if melee weapon has a good enough one-shot kill rate, use it
      // any range: of all ranged weapons available, use the weakest one with a good enough one-shot kill rate
      // we may estimate typical damage as 5/8ths of the damage rating for linear approximations
      // use above both for choosing which threat to target, and actual weapon equipping
      // Intermediate data structure: Dictionary<Actor,Dictionary<Item,float>>

      if (null == current_enemies) AdviseFriendsOfSafety();

      List<Engine.Items.ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(available_ranged_weapons, current_enemies);
      if (null != tmpAction) return tmpAction;

      tmpAction = BehaviorEquipWeapon(game, available_ranged_weapons, current_enemies);
      if (null != tmpAction) return tmpAction;

      if (null != current_enemies) {
        tmpAction = BehaviorFightOrFlee(game, current_enemies, ActorCourage.COURAGEOUS, FIGHT_EMOTES, RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS);
        if (null != tmpAction) return tmpAction;
      }

	  List<Percept> friends = FilterNonEnemies(percepts_all);
      if (null != friends) {
        List<Percept> percepts3 = friends.Filter(p =>
        {
          Actor actor = p.Percepted as Actor;
          return actor.Faction != GameFactions.TheCHARCorporation &&  RogueGame.IsInCHARProperty(actor.Location) && p.Turn == m_Actor.Location.Map.LocalTime.TurnCounter; // alpha10 bug fix only if visible right now!
        });
        if (percepts3 != null) {
          Actor target = FilterNearest(percepts3).Percepted as Actor;
          // Now that we can get crowds of civilians, they should react to seeing others betrayed
          // also note that the CHAR armor comes with an inbuilt CHAR radio (they invented the hyper-efficient radios the police and army use)
          // however, CHAR guards are on zone defense so the immediate aggression is just the current CHAR office (underground base is whole base, however)
          Aggress(target);
          // betrayal reaction
          foreach(var witness in percepts3) {
            Actor a = witness.Percepted as Actor;
            if (a == target) continue;
            if (a.IsSleeping) continue;
            // XXX cheat...assume symmetric visibility
            Aggress(a);
          }
          // XXX should have some reaction from witnesses that weren't aggressed
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target;
          // players are special: they get to react to being aggressed
          return new ActionSay(m_Actor, target, "Hey YOU!", (target.IsPlayer ? RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER : RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER | RogueGame.Sayflags.IS_FREE_ACTION));
        }
      }
      if (null != current_enemies && null != friends) {
        tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
      if (null != tmpAction) return tmpAction;

      if (old_enemies != null) {
        Percept target = FilterNearest(old_enemies);
        if (m_Actor.Location == target.Location) {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
        }
        if (CanReachSimple(target.Location, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP)) {
          tmpAction = BehaviorChargeEnemy(target,false,false);
          if (null != tmpAction) return tmpAction;
        }
      }

      if (null == old_enemies && WantToSleepNow) {
        tmpAction = BehaviorNavigateToSleep();
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorDropUselessItem();
      if (null != tmpAction) return tmpAction;

      // stack grabbing/trade goes here

      if (m_Actor.HasLeader && !DontFollowLeader) {
        tmpAction = BehaviorFollowActor(m_Actor.Leader, 1);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      } else if (m_Actor.CountFollowers < m_Actor.MaxFollowers) {
        // XXX \todo CHAR Guard leading civilian would get ugly quickly; either disallow, or ignore trespassing when CHAR guard is leader
        var want_leader = friends.FilterT<Actor>(a => m_Actor.CanTakeLeadOf(a));
        FilterOutUnreachablePercepts(ref want_leader, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP);
        Percept target = FilterNearest(want_leader);
        if (target != null) {
          tmpAction = BehaviorLeadActor(target);
          if (null != tmpAction) {
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }

      // critical item memory check goes here

      // possible we don't want CHAR guard leadership at all.  The stay-near-leader behavior doesn't fit, regardless (would go here)

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorWander(null, loc => RogueGame.IsInCHAROffice(loc));
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }

    private void Aggress(Actor target)
    {
      var game = RogueForm.Game;
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
