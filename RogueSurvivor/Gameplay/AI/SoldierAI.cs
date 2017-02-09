// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SoldierAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics.Contracts;

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

    private readonly MemorizedSensor m_MemLOSSensor;
    private readonly ExplorationData m_Exploration;

    public SoldierAI()
    {
      m_MemLOSSensor = new MemorizedSensor(new LOSSensor(VISION_SEES), LOS_MEMORY);
      m_Exploration = new ExplorationData();
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

    // return value must contain a {0} placeholder for the target name
    private string LeaderText_NotLeavingBehind(Actor target)
    {
      if (target.IsSleeping) return "patiently waits for {0} to wake up.";
      else if (FOV.Contains(target.Location.Position)) return "{0}! Don't lag behind!";
      else return "Where the hell is {0}?";
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());

      BehaviorEquipBodyArmor(game);

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

      List<Percept> old_enemies = FilterEnemies(percepts1);
      List<Percept> current_enemies = SortByGridDistance(FilterCurrent(old_enemies));

#if FAIL
      if (!Directives.CanThrowGrenades) {
        ItemGrenade itemGrenade = m_Actor.GetEquippedWeapon() as ItemGrenade;
        if (itemGrenade != null) {
          game.DoUnequipItem(m_Actor, itemGrenade);
        }
      }

      ActorAction tmpAction = null;

      // melee risk management check
      // if energy above 50, then we have a free move (range 2 evasion, or range 1/attack), otherwise range 1
      // must be above equip weapon check as we don't want to reload in an avoidably dangerous situation
      List<Point> legal_steps = m_Actor.OneStepRange(m_Actor.Location.Map,m_Actor.Location.Position);
      Dictionary<Point,int> damage_field = new Dictionary<Point, int>();
      List<Actor> slow_melee_threat = new List<Actor>();
      HashSet<Actor> immediate_threat = new HashSet<Actor>();
      if (null != enemies) VisibleMaximumDamage(damage_field, slow_melee_threat, immediate_threat);
      AddTrapsToDamageField(damage_field, percepts1);
      bool in_blast_field = AddExplosivesToDamageField(damage_field, percepts1);  // only civilians and soldiers respect explosives; CHAR and gang don't
      if (0>=damage_field.Count) damage_field = null;
      if (0>= slow_melee_threat.Count) slow_melee_threat = null;
      if (0>= immediate_threat.Count) immediate_threat = null;

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

	  List<Percept> friends = FilterNonEnemies(percepts1);

      // get out of the range of explosions if feasible
      if (in_blast_field) {
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat, enemies, friends) : ((null != retreat) ? DecideMove(retreat, enemies, friends) : null));
        if (null != tmpAction) {
		  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
          if (null != tmpAction2) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
          return tmpAction;
        }
      }

      List<ItemRangedWeapon> available_ranged_weapons = GetAvailableRangedWeapons();

      if ((null != retreat || null != run_retreat) && null != available_ranged_weapons) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(retreat, enemies);
        MaximizeRangedTargets(run_retreat, enemies);

        // ranged weapon: fast retreat ok
        // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
        // XXX we also want to be close enough to fire at all
        tmpAction = (safe_run_retreat ? DecideMove(legal_steps, run_retreat, enemies, friends) : ((null != retreat) ? DecideMove(retreat, enemies, friends) : null));
        if (null != tmpAction) {
		  ActionMoveStep tmpAction2 = tmpAction as ActionMoveStep;
          if (null != tmpAction2) {
            if (safe_run_retreat) RunIfPossible();
            else RunIfAdvisable(tmpAction2.dest.Position);
          }
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

      tmpAction = BehaviorEquipWeapon(game, legal_steps, damage_field, available_ranged_weapons, enemies, friends, immediate_threat);
      if (null != tmpAction) return tmpAction;

      // all free actions have to be before targeting enemies
      if (null != current_enemies) {
        if (game.Rules.RollChance(50)) {
          List<Percept> friends = FilterNonEnemies(percepts1);
          if (friends != null) {
            tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
            if (null != tmpAction) return tmpAction;
          }
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, true, true, ActorCourage.COURAGEOUS, SoldierAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }
#else
      // fleeing from explosives is done before the enemies check
      ActorAction tmpAction = BehaviorFleeFromExplosives(percepts1);
      if (null != tmpAction) return tmpAction;

      // throwing a grenade overrides normal weapon equipping choices
      if (null != current_enemies) {
        tmpAction = BehaviorThrowGrenade(game, current_enemies);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction) return tmpAction;

      // all free actions have to be before targeting enemies
      if (null != current_enemies) {
        if (game.Rules.RollChance(50)) {
          List<Percept> friends = FilterNonEnemies(percepts1);
          if (friends != null) {
            tmpAction = BehaviorWarnFriends(friends, FilterNearest(current_enemies).Percepted as Actor);
            if (null != tmpAction) return tmpAction;
          }
        }
        List<Percept> percepts3 = FilterFireTargets(current_enemies);
        if (percepts3 != null) {
          Actor target = FilterNearest(percepts3).Percepted as Actor;
          tmpAction = BehaviorRangedAttack(target);
          if (null != tmpAction) return tmpAction;
        }
        tmpAction = BehaviorFightOrFlee(game, current_enemies, true, true, ActorCourage.COURAGEOUS, SoldierAI.FIGHT_EMOTES);
        if (null != tmpAction) return tmpAction;
      }
#endif
      tmpAction = BehaviorRestIfTired();
      if (null != tmpAction) return tmpAction;
      if (null != old_enemies) {
        Percept target = FilterNearest(old_enemies);
        tmpAction = BehaviorChargeEnemy(target);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }

      tmpAction = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (null != tmpAction) return tmpAction;

      if (null == old_enemies && OkToSleepNow) {
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
      if (current_enemies != null) {
        Percept target = FilterNearest(current_enemies);
        if (m_Actor.Location == target.Location) {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
        }
        tmpAction = BehaviorChargeEnemy(target);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = target.Percepted as Actor;
          return tmpAction;
        }
      }
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
        Point position = m_Actor.Leader.Location.Position;
        tmpAction = BehaviorHangAroundActor(game, m_Actor.Leader, position, FOLLOW_LEADER_MIN_DIST, FOLLOW_LEADER_MAX_DIST);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      }
      if (m_Actor.CountFollowers > 0) {
        Actor target;
        tmpAction = BehaviorDontLeaveFollowersBehind(4, out target);
        if (null != tmpAction) {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
            game.DoEmote(m_Actor, string.Format(LeaderText_NotLeavingBehind(target), target.Name));
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
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
