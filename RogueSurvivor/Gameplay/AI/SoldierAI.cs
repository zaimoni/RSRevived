// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SoldierAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_SELECTACTION

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SoldierAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
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

    private readonly MemorizedSensor m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
    private readonly ExplorationData m_Exploration = new ExplorationData();

    public SoldierAI()
    {
    }

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
    protected override void SensorsOwnedBy(Actor actor) { (m_MemLOSSensor.Sensor as LOSSensor).OwnedBy(actor); }

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(Actor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (CanSee(target.Location)) return "{0}! Don't lag behind!";
      else return "Where the hell is {0}?";
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      ClearMovePlan();
      BehaviorEquipBodyArmor();

      List<Percept> percepts_all = FilterSameMap(UpdateSensors());

#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": "+m_Actor.Location.Map.LocalTime.TurnCounter.ToString());
#endif

      // OrderableAI specific: respond to orders
      if (null != Order) {
        ActorAction actorAction = ExecuteOrder(game, Order, percepts_all);
        if (null != actorAction) {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }

        SetOrder(null);
      }
      m_Actor.IsRunning = false;
      m_Actor.Activity = Activity.IDLE; // backstop

      m_Exploration.Update(m_Actor.Location);

      // New objectives system
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, Objectives.Count.ToString()+" objectives");
#endif
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

      List<Percept> old_enemies = FilterEnemies(percepts_all);
      List<Percept> current_enemies = SortByGridDistance(FilterCurrent(old_enemies));
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == old_enemies ? "null == current_enemies" : old_enemies.Count.ToString()+" enemies"));
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, (null == current_enemies ? "null == current_enemies" : current_enemies.Count.ToString()+" enemies"));
#endif

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      List<Point> legal_steps = m_Actor.LegalSteps;
      var damage_field = new Dictionary<Point, int>();
      var slow_melee_threat = new List<Actor>();
      var immediate_threat = new HashSet<Actor>();
      var blast_field = new HashSet<Point>();
      if (null != current_enemies) VisibleMaximumDamage(damage_field, slow_melee_threat, immediate_threat);
      AddTrapsToDamageField(damage_field, percepts_all);
      bool in_blast_field = AddExplosivesToDamageField(damage_field, blast_field, percepts_all);  // only civilians and soldiers respect explosives; CHAR and gang don't
      if (0>=damage_field.Count) damage_field = null;
      if (0>= slow_melee_threat.Count) slow_melee_threat = null;
      if (0>= immediate_threat.Count) immediate_threat = null;
      if (0>= blast_field.Count) blast_field = null;

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

	  List<Percept> friends = FilterNonEnemies(percepts_all);

      // get out of the range of explosions if feasible
      if (in_blast_field) {
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat) : ((null != retreat) ? DecideMove(retreat) : null));
        if (null != tmpAction) {
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "fleeing explosives");
#endif
          if (tmpAction is ActionMoveStep) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
          return tmpAction;
        }
      }

      List<Engine.Items.ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      tmpAction = ManageMeleeRisk(legal_steps, retreat, run_retreat, safe_run_retreat, available_ranged_weapons, current_enemies, slow_melee_threat);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "managing melee risk");
#endif
      if (null != tmpAction) return tmpAction;

      if (null != current_enemies) {
        tmpAction = BehaviorThrowGrenade(game, current_enemies, blast_field);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "toss grenade");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game, legal_steps, available_ranged_weapons, current_enemies, immediate_threat);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "probably reloading");
#endif
      if (null != tmpAction) return tmpAction;

      // all free actions have to be before targeting enemies
      if (null != current_enemies) {
        if (null != friends && game.Rules.RollChance(50)) {
          tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "warning friends");
#endif
          if (null != tmpAction) return tmpAction;
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, damage_field, ActorCourage.COURAGEOUS, SoldierAI.FIGHT_EMOTES, blast_field);
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "having to fight w/o ranged weapons");
#endif
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorUseMedecine ok"); // TRACER
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "medicating");
#endif
      if (null != tmpAction) return tmpAction;
      tmpAction = BehaviorRestIfTired();
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorRestIfTired ok"); // TRACER
      if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "resting");
#endif
      if (null != tmpAction) return tmpAction;

      if (null != old_enemies) {
        tmpAction = BehaviorChargeEnemy(FilterNearest(old_enemies));
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget ) Logger.WriteLine(Logger.Stage.RUN_MAIN, "charging enemies");
#endif
        if (null != tmpAction) return tmpAction;
      }

      if (null == old_enemies && WantToSleepNow) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorNavigateToSleep");
#endif
        tmpAction = BehaviorNavigateToSleep();
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget && null!=tmpAction) Logger.WriteLine(Logger.Stage.RUN_MAIN, "navigating to sleep");
#endif
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorDropUselessItem();
      if (null != tmpAction) return tmpAction;

      if (current_enemies != null) {    // this is morally dead code (needs testing)
        Percept target = FilterNearest(current_enemies);
        if (m_Actor.Location == target.Location) {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
        }
        tmpAction = BehaviorChargeEnemy(target);
        if (null != tmpAction) return tmpAction;
      }

      // stack grabbing/trade goes here

      if (game.Rules.RollChance(BUILD_LARGE_FORT_CHANCE)) {
        tmpAction = BehaviorBuildLargeFortification(game, START_FORT_LINE_CHANCE);
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

      if (m_Actor.HasLeader && !DontFollowLeader) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling BehaviorHangAroundActor");
#endif
        tmpAction = BehaviorHangAroundActor(game, m_Actor.Leader, FOLLOW_LEADER_MIN_DIST, FOLLOW_LEADER_MAX_DIST);    // SoldierAI difference here probably ok
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorHangAroundActor: "+(tmpAction?.ToString() ?? "null"));
#endif
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      } else if (m_Actor.CountFollowers < m_Actor.MaxFollowers) {
        Percept target = FilterNearest(friends);
        if (target != null) {
          tmpAction = BehaviorLeadActor(target);
          if (null != tmpAction) {
            m_Actor.TargetActor = target.Percepted as Actor;
            return tmpAction;
          }
        }
      }

      // critical item memory check goes here

      if (m_Actor.CountFollowers > 0) {
        tmpAction = BehaviorDontLeaveFollowersBehind(4, out Actor target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      // hunt down threats would go here
      // tourism would go here

      tmpAction = BehaviorExplore(m_Exploration);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
