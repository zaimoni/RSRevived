// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Rules
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define POLICE_NO_QUESTIONS_ASKED
#define B_MOVIE_MARTIAL_ARTS

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Size = Zaimoni.Data.Vector2D_short;   // likely to go obsolete with transition to a true vector type

namespace djack.RogueSurvivor.Engine
{
  internal class Rules
  {
    public const int INFECTION_LEVEL_1_WEAK = 10;
    public const int INFECTION_LEVEL_2_TIRED = 30;
    public const int INFECTION_LEVEL_3_VOMIT = 50;
    public const int INFECTION_LEVEL_4_BLEED = 75;
    public const int INFECTION_LEVEL_5_DEATH = 100;
    public const int INFECTION_LEVEL_1_WEAK_STA = 24;
    public const int INFECTION_LEVEL_2_TIRED_STA = 24;
    public const int INFECTION_LEVEL_2_TIRED_SLP = 90;
    public const int INFECTION_LEVEL_4_BLEED_HP = 6;
    public const int INFECTION_EFFECT_TRIGGER_CHANCE_1000 = 2;
    public const int UPGRADE_SKILLS_TO_CHOOSE_FROM = 5;
    public const int UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM = 2;
    public static int SKILL_AGILE_DEF_BONUS = 4;
    public static double SKILL_CARPENTRY_BARRICADING_BONUS = 0.15;
    public static int SKILL_CHARISMATIC_TRUST_BONUS = 2;
    public static int SKILL_CHARISMATIC_TRADE_BONUS = 10;
    public static int SKILL_HARDY_HEAL_CHANCE_BONUS = 1;
    public static int SKILL_LIGHT_FEET_TRAP_BONUS = 15; // alpha10
    public static int SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = 20; // alpha10
    public static double SKILL_MEDIC_BONUS = 0.15;
    public const int SKILL_MEDIC_LEVEL_FOR_REVIVE_EST = 1;
    public static int SKILL_NECROLOGY_CORPSE_BONUS = 4;
    public const int SKILL_NECROLOGY_LEVEL_FOR_INFECTION = 3;
    public const int SKILL_NECROLOGY_LEVEL_FOR_RISE = 5;
    public static double SKILL_STRONG_PSYCHE_LEVEL_BONUS = 0.15;
    public static double SKILL_STRONG_PSYCHE_ENT_BONUS = 0.15;
    public static int SKILL_UNSUSPICIOUS_BONUS = 20;   // alpha10
    public static int SKILL_ZAGILE_DEF_BONUS = 2;
    public static double SKILL_ZEATER_REGEN_BONUS = 0.2f;
    public static int SKILL_ZLIGHT_FEET_TRAP_BONUS = 3;
    public static int SKILL_ZGRAB_CHANCE = 4;   // alpha10
    public static double SKILL_ZINFECTOR_BONUS = 0.15f;
    public const int BASE_ACTION_COST = 100;
    public const int BASE_SPEED = 100;
    public const int STAMINA_COST_RUNNING = 4;
    public const int STAMINA_REGEN_PER_TURN = 2;
    public const int STAMINA_COST_JUMP = 8;
    public const int STAMINA_COST_MELEE_ATTACK = 8;
    public const int STAMINA_COST_MOVE_DRAGGED_CORPSE = 8;
    public const int JUMP_STUMBLE_CHANCE = 25;
    public const int JUMP_STUMBLE_ACTION_COST = 100;
    public const int BARRICADING_MAX = 80;
    public const int MELEE_WEAPON_BREAK_CHANCE = 1;
    public const int MELEE_WEAPON_FRAGILE_BREAK_CHANCE = 3;
    public const int FIREARM_JAM_CHANCE_NO_RAIN = 1;
    public const int FIREARM_JAM_CHANCE_RAIN = 3;
    public const int BODY_ARMOR_BREAK_CHANCE = 2;
    public const int FOOD_BASE_POINTS = 2*Actor.FOOD_HUNGRY_LEVEL;
    public const int ROT_BASE_POINTS = 2*Actor.ROT_HUNGRY_LEVEL;
    public const int SLEEP_BASE_POINTS = 2*Actor.SLEEP_SLEEPY_LEVEL;
    public const int SANITY_BASE_POINTS = 4*WorldTime.TURNS_PER_DAY;
    public const int SANITY_UNSTABLE_LEVEL = 2*WorldTime.TURNS_PER_DAY;
    public const int SANITY_NIGHTMARE_CHANCE = 2;
    public const int SANITY_NIGHTMARE_SLP_LOSS = 2*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_NIGHTMARE_SAN_LOSS = WorldTime.TURNS_PER_HOUR;
    public const int SANITY_NIGHTMARE_STA_LOSS = 10 * STAMINA_COST_RUNNING;  // alpha10 -- worth running for 10 turns
    public const int SANITY_INSANE_ACTION_CHANCE = 5;
    public const int SANITY_HIT_BUTCHERING_CORPSE = WorldTime.TURNS_PER_HOUR;
    public const int SANITY_HIT_UNDEAD_EATING_CORPSE = 2*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_HIT_LIVING_EATING_CORPSE = 4*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_HIT_EATEN_ALIVE = 4*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_HIT_ZOMBIFY = 2*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_HIT_BOND_DEATH = 8*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_RECOVER_KILL_UNDEAD = 3*WorldTime.TURNS_PER_HOUR;
    public const int SANITY_RECOVER_BOND_CHANCE = 5;
    public const int SANITY_RECOVER_BOND = 4 * WorldTime.TURNS_PER_HOUR;  // was 1h
    public const int SANITY_RECOVER_CHAT_OR_TRADE = 3 * WorldTime.TURNS_PER_HOUR;
    public const int FOOD_STARVING_DEATH_CHANCE = 5;
    public const int FOOD_EXPIRED_VOMIT_CHANCE = 25;
    public const int FOOD_VOMIT_STA_COST = 100;
    public const int ROT_STARVING_HP_CHANCE = 5;
    public const int ROT_HUNGRY_SKILL_CHANCE = 5;
    public const int SLEEP_EXHAUSTION_COLLAPSE_CHANCE = 5;
    public const int SLEEP_ON_COUCH_HEAL_CHANCE = 5;
    public const int SLEEP_HEAL_HITPOINTS = 2;
    public const int CHAT_RADIUS = 1;   // would space-time scale close to angband scale (900 turns/hour), but not much as hard to hear past 20' or so
    public const int LOUD_NOISE_RADIUS = WorldTime.TURNS_PER_HOUR/6;    // space-time scales; value 5 for 30 turns/hour
    private const int LOUD_NOISE_BASE_WAKEUP_CHANCE = 10;
    private const int LOUD_NOISE_DISTANCE_BONUS = 10;
    public const int VICTIM_DROP_GENERIC_ITEM_CHANCE = 50;
    public const int VICTIM_DROP_AMMOFOOD_ITEM_CHANCE = 100;
    public const int IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE = 25;
    public const int ZTRACKINGRADIUS = 6;
    public const int DEFAULT_ACTOR_WEIGHT = 10;
    public const int FIRE_RAIN_TEST_CHANCE = 1;
    public const int FIRE_RAIN_PUT_OUT_CHANCE = 10;
    public const int TRUST_NEUTRAL = 0;
    public const int TRUST_MIN = -12*WorldTime.TURNS_PER_HOUR;
    public const int TRUST_MAX = 2*WorldTime.TURNS_PER_DAY;
    public const int TRUST_BASE_INCREASE = 1;
    public const int TRUST_GOOD_GIFT_INCREASE = 3*WorldTime.TURNS_PER_HOUR;
    public const int TRUST_MISC_GIFT_INCREASE = WorldTime.TURNS_PER_HOUR/3;
    public const int TRUST_GIVE_ITEM_ORDER_PENALTY = -WorldTime.TURNS_PER_HOUR;
    public const int TRUST_LEADER_KILL_ENEMY = 3*WorldTime.TURNS_PER_HOUR;
//  public const int TRUST_REVIVE_BONUS = 12*WorldTime.TURNS_PER_HOUR;  // removed in RS Alpha 10
    public const int MURDERER_SPOTTING_BASE_CHANCE = 5;
    public const int MURDERER_SPOTTING_DISTANCE_PENALTY = 1;
    public const int MURDER_SPOTTING_MURDERCOUNTER_BONUS = 5;
    private const float INFECTION_BASE_FACTOR = 1f;
    private const int CORPSE_ZOMBIFY_BASE_CHANCE = 0;
    public const int CORPSE_ZOMBIFY_DELAY = 6*WorldTime.TURNS_PER_HOUR;
    private const float CORPSE_ZOMBIFY_INFECTIONP_FACTOR = 1f;
    private const float CORPSE_ZOMBIFY_NIGHT_FACTOR = 2f;
    private const float CORPSE_ZOMBIFY_DAY_FACTOR = 0.01f;
    private const float CORPSE_ZOMBIFY_TIME_FACTOR = 0.001388889f;
    private const float CORPSE_EATING_NUTRITION_FACTOR = 10f;
    private const float CORPSE_EATING_INFECTION_FACTOR = 0.1f;
    private const float CORPSE_DECAY_PER_TURN = 0.005555556f;   // 1/180 per turn
    public const int GIVE_RARE_ITEM_DAY = 7;
    public const int GIVE_RARE_ITEM_CHANCE = 5;
    private DiceRoller m_DiceRoller;

#region Session save/load assistants
    public void Load(SerializationInfo info, StreamingContext context)
    {
      m_DiceRoller = (DiceRoller) info.GetValue("m_DiceRoller", typeof(DiceRoller));
    }

    public void Save(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_DiceRoller", DiceRoller,typeof(DiceRoller));
    }
#endregion

    public DiceRoller DiceRoller {
      get {
        return m_DiceRoller;
      }
    }

    public Rules(DiceRoller diceRoller)
    {
#if DEBUG
      if (null == diceRoller) throw new ArgumentNullException(nameof(diceRoller));
#endif
      m_DiceRoller = diceRoller;
    }

    public int Roll(int min, int max)
    {
      return m_DiceRoller.Roll(min, max);
    }

    public short Roll(int min, short max)
    {
      return m_DiceRoller.Roll((short)min, max);
    }


    public short Roll(short min, short max)
    {
      return m_DiceRoller.Roll(min, max);
    }

    public bool RollChance(int chance)
    {
      return m_DiceRoller.RollChance(chance);
    }

#if DEAD_FUNC
    public float RollFloat()
    {
      return m_DiceRoller.RollFloat();
    }

    public float Randomize(float value, float deviation)
    {
      float num = deviation / 2f;
      return (float) ((double) value - (double) num * (double)m_DiceRoller.RollFloat() + (double) num * (double)m_DiceRoller.RollFloat());
    }

    public int RollX(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      return m_DiceRoller.Roll(0, map.Width);
    }

    public int RollY(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      return m_DiceRoller.Roll(0, map.Height);
    }
#endif

    public Direction RollDirection()
    {
      return m_DiceRoller.Choose(Direction.COMPASS);
    }

    public int RollSkill(int skillValue)
    {
      if (skillValue <= 0)
        return 0;
      return (m_DiceRoller.Roll(0, skillValue + 1) + m_DiceRoller.Roll(0, skillValue + 1)) / 2;
    }

    private static int _Average(int x, int y) { return x+y/2; }

    public static DenormalizedProbability<int> SkillProbabilityDistribution(int skillValue)
    {
      if (0 >= skillValue) return ConstantDistribution<int>.Get(0);
      DenormalizedProbability<int> sk_prob = UniformDistribution.Get(0,skillValue);
      return DenormalizedProbability<int>.Apply(sk_prob*sk_prob,_Average);  // XXX \todo cache this
    }

    public int RollDamage(int damageValue)
    {
      if (damageValue <= 0) return 0;
      return m_DiceRoller.Roll(damageValue / 2, damageValue + 1);
    }

    static public MapObject CanActorPutItemIntoContainer(Actor actor, in Point position)
    {
#if DEBUG
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
      MapObject mapObjectAt = actor.Location.Map.GetMapObjectAtExt(position);
      if (null == mapObjectAt) return null;
      return string.IsNullOrEmpty(mapObjectAt.ReasonCantPutItemIn(actor)) ? mapObjectAt : null;
    }

    static public MapObject CanActorPutItemIntoContainer(Actor actor, in Point position, out string reason)
    {
#if DEBUG
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
      MapObject mapObjectAt = actor.Location.Map.GetMapObjectAt(position);
      reason = mapObjectAt?.ReasonCantPutItemIn(actor) ?? "object is not a container";
      return string.IsNullOrEmpty(mapObjectAt.ReasonCantPutItemIn(actor)) ? mapObjectAt : null;
    }

    public bool CanActorEatFoodOnGround(Actor actor, Item it, out string reason)
    {
#if DEBUG
      if (actor == null) throw new ArgumentNullException("actor");
      if (it == null) throw new ArgumentNullException("item");
#endif
      if (!(it is ItemFood))
      {
        reason = "not food";
        return false;
      }
      if (!actor.Location.Items?.Contains(it) ?? true)
      {
        reason = "item not here";
        return false;
      }
      reason = "";
      return true;
    }

    private static ActorAction IsBumpableFor(Actor actor, Map map, Point point, out string reason)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
      reason = "";
      Location loc = new Location(map,point);

      if (1>=actor.Controller.FastestTrapKill(in loc)) {
        reason = "deathtrapped";
        return null;
      }

      var actors_in_way = actor.GetMoveBlockingActors(point);
      if (actors_in_way.TryGetValue(point,out Actor actorAt)) {
        if (actor.IsEnemyOf(actorAt)) {
          return (actor.CanMeleeAttack(actorAt, out reason) ? new ActionMeleeAttack(actor, actorAt) : null);
        }
#if B_MOVIE_MARTIAL_ARTS
        if (1<actors_in_way.Count) {
          Actor target = null;
          foreach(var pt_actor in actors_in_way) {
            if (pt_actor.Value == actorAt) continue;
            if (!actor.CanMeleeAttack(pt_actor.Value,out reason)) continue;
            if (null == target || target.HitPoints>pt_actor.Value.HitPoints) target = pt_actor.Value;
          }
          return (null!=target ? new ActionMeleeAttack(actor, target) : null);
        }
#endif
		// player as leader should be able to switch with player as follower
		// NPCs shouldn't be leading players anyway
        if ((actor.IsPlayer || !actorAt.IsPlayer) && actor.CanSwitchPlaceWith(actorAt, out reason))
          return new ActionSwitchPlace(actor, actorAt);
        if (!actor.CanChatWith(actorAt, out reason)) return null;
        if (   ((actor.Controller as Gameplay.AI.OrderableAI)?.ProposeSwitchPlaces(actorAt.Location) ?? false)
            && !((actorAt.Controller as Gameplay.AI.OrderableAI)?.RejectSwitchPlaces(actor.Location) ?? true))
           return new ActionSwitchPlaceEmergency(actor,actorAt);
        return new ActionChat(actor, actorAt);
#if B_MOVIE_MARTIAL_ARTS
      } else if (0<actors_in_way.Count) {   // range-2 issue.  Identify weakest enemy.
        Actor target = null;
        foreach(var pt_actor in actors_in_way) {
          if (!actor.CanMeleeAttack(pt_actor.Value,out reason)) continue;
          if (null == target || target.HitPoints>pt_actor.Value.HitPoints) target = pt_actor.Value;
        }
        return (null!=target ? new ActionMeleeAttack(actor, target) : null);
#endif
      }
      if (!map.IsInBounds(point)) {
	    return (actor.CanLeaveMap(point, out reason) ? new ActionLeaveMap(actor, in point) : null);
      }
      ActionMoveStep actionMoveStep = new ActionMoveStep(actor, in point);
      if (actionMoveStep.IsLegal()) {
        reason = "";
        return actionMoveStep;
      }
      reason = actionMoveStep.FailReason;
      MapObject mapObjectAt = map.GetMapObjectAt(point);
      if (mapObjectAt != null) {
        if (mapObjectAt is DoorWindow door) {
          if (door.IsClosed) {
            if (actor.CanOpen(door, out reason)) return new ActionOpenDoor(actor, door);
            if (actor.CanBash(door, out reason)) return new ActionBashDoor(actor, door);
            return null;
          }
          // covers barricaded broken windows...otherwise redundant.
          if (door.BarricadePoints > 0) {
            // Z will bash barricaded doors but livings won't, except for specific overrides
            // this does conflict with the tourism behavior
            if (actor.CanBash(door, out reason)) return new ActionBashDoor(actor, door);
            reason = "cannot bash the barricade";
            return null;
          }
        }
        var act = (actor.Controller as Gameplay.AI.ObjectiveAI)?.WouldGetFromContainer(in loc);
        if (null != act) return act;
        // release block: \todo would like to restore inventory-grab capability for InsaneHumanAI (and feral dogs, when bringing them up)
        // only Z want to break arbitrary objects; thus the guard clause
        if (actor.Model.Abilities.CanBashDoors && actor.CanBreak(mapObjectAt, out reason))
          return new ActionBreak(actor, mapObjectAt);
        if (mapObjectAt is PowerGenerator powGen) {
          if (powGen.IsOn) {
            Item tmp = actor.GetEquippedItem(DollPart.LEFT_HAND);   // normal lights and trackers
            if (tmp != null && actor.CanRecharge(tmp, out reason))
              return new ActionRechargeItemBattery(actor, tmp);
            tmp = actor.GetEquippedItem(DollPart.RIGHT_HAND);   // formal correctness
            if (tmp != null && actor.CanRecharge(tmp, out reason))
              return new ActionRechargeItemBattery(actor, tmp);
            tmp = actor.GetEquippedItem(DollPart.HIP_HOLSTER);   // the police tracker
            if (tmp != null && actor.CanRecharge(tmp, out reason))
              return new ActionRechargeItemBattery(actor, tmp);
          }
          if (actor.CanSwitch(powGen, out reason)) {
             if (actor.IsPlayer || !powGen.IsOn) return new ActionSwitchPowerGenerator(actor, powGen);
          }
          return null;
        }
      }
      return null;
    }

    public static ActorAction IsBumpableFor(Actor actor, in Location location)
    {
      return IsBumpableFor(actor, in location, out string reason);
    }

    public static ActorAction IsBumpableFor(Actor actor, in Location location, out string reason)
    {
      return IsBumpableFor(actor, location.Map, location.Position, out reason);
    }

    // Pathfindability is not quite the same as bumpability
    // * ok to break barricaded doors on fastest path
    // * only valid for subclasses of ObjectiveAI/OrderableAI (which can pathfind in the first place).
    // * ok to push objects aside
    // * not ok to chat as a time-cost action (could be re-implemented)
    private static ActorAction IsPathableFor(Actor actor, Location loc, out string reason)
    {
#if DEBUG
      if (null == loc.Map) throw new ArgumentNullException(nameof(loc)+".Map");
      if (null == actor) throw new ArgumentNullException(nameof(actor));
      if (!(actor.Controller is Gameplay.AI.ObjectiveAI)) throw new InvalidOperationException("!(actor.Controller is Gameplay.AI.ObjectiveAI)");
#endif
      reason = "";

      if (1 >= actor.Controller.FastestTrapKill(in loc)) {
        reason = "deathtrapped";
        return null;
      }

      // unclear whether B_MOVIE_MARTIAL_ARTS requires pathfinding changes or not; changes would go here
      var map = loc.Map;
      var point = loc.Position;
      if (!map.IsInBounds(point)) {
	    return (actor.CanLeaveMap(point, out reason) ? new ActionLeaveMap(actor, in point) : null);
      }
      ActionMoveStep actionMoveStep = new ActionMoveStep(actor, in loc);
      if (loc.IsWalkableFor(actor, out reason)) {
        reason = "";
        if (map!=actor.Location.Map) {
          // check for exit leading here and substitute if so.  Cf. BaseAI::BehaviorUseExit
          Exit exit = actor.Model.Abilities.AI_CanUseAIExits ? actor.Location.Exit : null;
          if (null != exit && exit.Location==loc) {
           if (string.IsNullOrEmpty(exit.ReasonIsBlocked(actor))) return new ActionUseExit(actor, actor.Location);
           Actor a = exit.Location.Actor;
           if (a != null && actor.IsEnemyOf(a) && actor.CanMeleeAttack(a)) return new ActionMeleeAttack(actor, a);
           MapObject obj = exit.Location.MapObject;
           if (obj != null && actor.CanBreak(obj)) return new ActionBreak(actor, obj);
           return null;
          }
        }
        return actionMoveStep;
      }

      Gameplay.AI.ObjectiveAI ai = actor.Controller as Gameplay.AI.ObjectiveAI;

      // respec of IsWalkableFor guarantees that any actor will be adjacent
      reason = actionMoveStep.FailReason;
      Actor actorAt = map.GetActorAt(point);
      if (actorAt != null) {
        if (actor.IsEnemyOf(actorAt)) {
          return (actor.CanMeleeAttack(actorAt, out reason) ? new ActionMeleeAttack(actor, actorAt) : null);
        }
		// player as leader should be able to switch with player as follower
		// NPCs shouldn't be leading players anyway
        if (actor.IsPlayer || !actorAt.IsPlayer) {
          if (actor.CanSwitchPlaceWith(actorAt, out reason)) return new ActionSwitchPlace(actor, actorAt);
        }

        // no chat when pathfinding
        // but it is ok to shove other actors
        if (actor.AbleToPush && actor.CanShove(actorAt)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           // we are adjacent due to the early-escape above
           var push_dest = actorAt.ShoveDestinations;

           bool push_legal = 1<=push_dest.Count;
           if (push_legal) {
             var self_block = ai.WantToGoHere(actorAt.Location);
             if (null != self_block && 1==self_block.Count) {
               push_dest.OnlyIf(pt => !self_block.Contains(pt));
               push_legal = 1<=push_dest.Count;
             }
           }
           if (push_legal) {
             bool i_am_in_his_way = false;
             bool i_can_help = false;
             var help_him = (actorAt.Controller as Gameplay.AI.ObjectiveAI)?.WantToGoHere(actorAt.Location);
             if (null != help_him) {
               i_am_in_his_way = help_him.Contains(actor.Location);
               if (push_dest.NontrivialFilter(x => help_him.Contains(x.Key))) push_dest.OnlyIf(pt => help_him.Contains(pt));
               i_can_help = help_him.Contains(push_dest.First().Key);
             }

             // function target
             var candidates_2 = push_dest.Where(pt => !Rules.IsAdjacent(actor.Location, pt.Key));
             var candidates_1 = push_dest.Where(pt => Rules.IsAdjacent(actor.Location, pt.Key));
             var candidates = (i_can_help && candidates_2.Any()) ? candidates_2.ToList() : null;
             if (null == candidates && !i_am_in_his_way && i_can_help && candidates_1.Any()) candidates = candidates_1.ToList();
             if (null == candidates && i_am_in_his_way) {
               // HMM...maybe step aside instead?
               var considering = actor.MutuallyAdjacentFor(actor.Location,actorAt.Location);
               if (null != considering) {
                 considering = considering.FindAll(pt => pt.IsWalkableFor(actor));
                 if (0 < considering.Count) return new ActionMoveStep(actor, RogueForm.Game.Rules.DiceRoller.Choose(considering));
               }
             }

             // legacy initialization
             if (null == candidates && candidates_2.Any()) candidates = candidates_2.ToList();
             if (null == candidates && candidates_1.Any()) candidates = candidates_1.ToList();
             // end function target

             if (null != candidates) return new ActionShove(actor,actorAt,RogueForm.Game.Rules.DiceRoller.Choose(candidates).Value);
           }
        }

        // check for mutual-advantage switching place between ais
        if (   ((actor.Controller as Gameplay.AI.OrderableAI)?.ProposeSwitchPlaces(actorAt.Location) ?? false)
            && !((actorAt.Controller as Gameplay.AI.OrderableAI)?.RejectSwitchPlaces(actor.Location) ?? true)) {
           return new ActionSwitchPlaceEmergency(actor,actorAt);    // this is an AI cheat so shouldn't be happening that much
        }

        // consider re-legalizing chat here
        return null;
      }
      MapObject mapObjectAt = map.GetMapObjectAt(point);
      if (mapObjectAt != null) {
        if (mapObjectAt is DoorWindow door) {
          if (door.BarricadePoints > 0) {
            // pathfinding livings will break barricaded doors (they'll prefer to go around it)
            if (actor.CanBash(door, out reason)) return new ActionBashDoor(actor, door);
            if (actor.CanBreak(door, out reason)) return new ActionBreak(actor, door);
            reason = "cannot bash the barricade";
            return null;
          }
          if (door.IsClosed) {
            if (actor.CanOpen(door, out reason)) return new ActionOpenDoor(actor, door);
            if (actor.CanBash(door, out reason)) return new ActionBashDoor(actor, door);
            return null;
          }
        }
        var act = ai?.WouldGetFromContainer(in loc);
        if (null != act) return act;
        // release block: \todo would like to restore inventory-grab capability for InsaneHumanAI (and feral dogs, when bringing them up)
        // only Z want to break arbitrary objects; thus the guard clause
        if (actor.Model.Abilities.CanBashDoors && actor.CanBreak(mapObjectAt, out reason))
          return new ActionBreak(actor, mapObjectAt);

        // pushing is very bad for bumping, but ok for pathing
        if (actor.AbleToPush && actor.CanPush(mapObjectAt)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           var push_dest = Map.ValidDirections(mapObjectAt.Location, loc2 => {
               // short-circuit language requirement on operator && failed here
               if (!mapObjectAt.CanPushTo(in loc2)) return false;
               if (loc.Map.HasExitAt(loc2.Position)) return false;   // pushing onto an exit is very disruptive; may be ok tactically, but not when pathing
               return !loc2.Map.PushCreatesSokobanPuzzle(loc2.Position, actor);
           });   // does not trivially create a Sokoban puzzle (can happen in police station)

           bool is_adjacent = IsAdjacent(actor.Location, mapObjectAt.Location);
           bool push_legal = (is_adjacent ? 1 : 2)<=push_dest.Count;
           if (is_adjacent) {
             if (push_legal) {
               var self_block = ai.WantToGoHere(mapObjectAt.Location);
               if (null != self_block && 1==self_block.Count) push_dest.OnlyIf(pt => !self_block.Contains(pt));

               // function target
               List<KeyValuePair<Location, Direction>> candidates = null;
               var candidates_2 = push_dest.Where(pt => !IsAdjacent(actor.Location, pt.Key));
               var candidates_1 = push_dest.Where(pt => IsAdjacent(actor.Location, pt.Key));
               if (candidates_2.Any()) candidates = candidates_2.ToList();
               if (null == candidates && candidates_1.Any()) candidates = candidates_1.ToList();
               // end function target

               if (null != candidates) return new ActionPush(actor,mapObjectAt,RogueForm.Game.Rules.DiceRoller.Choose(candidates).Value);
             } else {
               // proceed with pull if we can't push safely
               var possible = mapObjectAt.Location.Position.Adjacent();
               var pull_dests = possible.Where(pt => 1==Rules.GridDistance(actor.Location,new Location(mapObjectAt.Location.Map,pt)));
               if (pull_dests.Any()) {
                 return new ActionPull(actor,mapObjectAt,RogueForm.Game.Rules.DiceRoller.Choose(pull_dests));
               }
             }
           }
        }

        // \todo consider eliminating power generators as a pathable target (rely on behaviors instead and path to adjacent squares)
        if (mapObjectAt is PowerGenerator powGen) {
          if (powGen.IsOn) {
            Item tmp = actor.GetEquippedItem(DollPart.LEFT_HAND);   // normal lights and trackers
            if (tmp != null && actor.CanRecharge(tmp, out reason))
              return new ActionRechargeItemBattery(actor, tmp);
            tmp = actor.GetEquippedItem(DollPart.RIGHT_HAND);   // formal correctness
            if (tmp != null && actor.CanRecharge(tmp, out reason))
              return new ActionRechargeItemBattery(actor, tmp);
            tmp = actor.GetEquippedItem(DollPart.HIP_HOLSTER);   // the police tracker
            if (tmp != null && actor.CanRecharge(tmp, out reason))
              return new ActionRechargeItemBattery(actor, tmp);
          }
          if (actor.CanSwitch(powGen, out reason)) {
             if (actor.IsPlayer || !powGen.IsOn) return new ActionSwitchPowerGenerator(actor, powGen);
          }
          return null;
        }
      }
      return null;
    }

    public static ActorAction IsPathableFor(Actor actor, in Location location)
    {
      return IsPathableFor(actor, location, out string reason);
    }

    public int ActorDamageVsCorpses(Actor a)
    {
      return a.CurrentMeleeAttack.DamageValue / 2 + Rules.SKILL_NECROLOGY_CORPSE_BONUS * a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.NECROLOGY);
    }

    // These two somewhat counter-intuitively consider "same location" as adjacent
    public static bool IsAdjacent(in Location a, in Location b)
    {
      if (a.Map != b.Map) {
        Location? test = a.Map.Denormalize(in b);
        if (null == test) {
          Exit exit = a.Exit;
          return null != exit && exit.Location == b;
        }
        IsAdjacent(a.Position, test.Value.Position);
      }
      return IsAdjacent(a.Position, b.Position);
    }

    public static bool IsAdjacent(in Point pA, in Point pB)
    {
      if (Math.Abs(pA.X - pB.X) < 2)
        return Math.Abs(pA.Y - pB.Y) < 2;
      return false;
    }

    // L-infinity metric i.e. distance in moves
    public static int GridDistance(Point pt)
    {
      return Math.Max(Math.Abs(pt.X), Math.Abs(pt.Y));
    }

    public static int GridDistance(in Point pA, in Point pB) { return GridDistance(pB - pA); }

    public static int GridDistance(in Location locA, in Location locB)
    {
      if (null == locA.Map) return int.MaxValue;
      if (null == locB.Map) return int.MaxValue;
      if (locA.Map==locB.Map) return GridDistance(locB.Position - locA.Position);
      Location? tmp = locA.Map.Denormalize(in locB);
      if (null == tmp) return int.MaxValue;
      return GridDistance(tmp.Value.Position - locA.Position);
    }

    public static int GridDistanceFn(Location locA, Location locB)  // for function pointer usage
    {
      return GridDistance(in locA, in locB);
    }

    // Euclidean plane distance
    public static double StdDistance(in Point from, in Point to) { return StdDistance(to - from); }

    public static double StdDistance(in Point v)
    {
      return Math.Sqrt((double) (v.X * v.X + v.Y * v.Y));
    }

    public static double StdDistance(in Location from, in Location to)
    {
      Location? test = from.Map.Denormalize(in to);
      if (null == test) return double.MaxValue;
      return StdDistance(test.Value.Position - from.Position);
    }

    public static double InteractionStdDistance(in Location from, in Location to)
    {
      Location? test = from.Map.Denormalize(in to);
      if (null == test) {
        Exit exit = from.Exit;
        return (null != exit && exit.Location == to) ? 1.0 : double.MaxValue;
      }
      return StdDistance(test.Value.Position - from.Position);
    }

    // allows stairways for melee range, etc.
    public static int InteractionDistance(in Location a, in Location b)
    {
      if (a.Map != b.Map) {
        Location? test = a.Map.Denormalize(in b);
        if (null == test) {
          Exit exit = a.Exit;
          return (null != exit && exit.Location == b) ? 1 : int.MaxValue;
        }
        return GridDistance(a.Position, test.Value.Position);
      }
      return GridDistance(a.Position, b.Position);
    }

    public static bool IsMurder(Actor killer, Actor victim)
    {
      if (null == killer) return false;
      if (null == victim) return false;
      if (victim.Model.Abilities.IsUndead) return false;
      if (killer.Model.Abilities.IsLawEnforcer && victim.MurdersOnRecord(killer) > 0) return false;
#if POLICE_NO_QUESTIONS_ASKED
      if (killer.Model.Abilities.IsLawEnforcer && killer.Threats.IsThreat(victim)) return false;
#endif
      if (killer.Faction.IsEnemyOf(victim.Faction)) return false;

      // If your leader is a cop i.e. First Class Citizen, killing his enemies should not trigger murder charges.
      Actor killer_leader = killer.LiveLeader;
      if (killer_leader?.Model.Abilities.IsLawEnforcer ?? false) {
        if (killer_leader.IsEnemyOf(victim)) return false;
        if (victim.IsSelfDefenceFrom(killer.Leader)) return false;  // XXX redundant?
      }

      // \todo National Guard is likely to have unusual handling as well.

      // Framed for murder.  Since this is an apocalypse, self-defence doesn't count no matter what the law was pre-apocalypse
      if (victim.Model.Abilities.IsLawEnforcer) return true;
      if (victim.HasLeader && victim.Leader.Model.Abilities.IsLawEnforcer) return true;

      // resume old definition
      if (killer.IsSelfDefenceFrom(victim)) return false;

      // XXX RS Alpha 9; went away in RS Alpha 10.  The important case (police leading civilian) is handled above.  May need reimplementing (cf. Actor::ChainOfCommand)
      if (killer_leader?.IsSelfDefenceFrom(victim) ?? false) return false;
      if (killer.CountFollowers > 0 && !killer.Followers.Any(fo => fo.IsSelfDefenceFrom(victim))) return false;

      return true;
    }

    public static int ActorSanRegenValue(Actor actor, int baseValue)
    {
      return baseValue + (int)(/* (double) */ SKILL_STRONG_PSYCHE_ENT_BONUS * /* (int) */ (baseValue * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG_PSYCHE)));
    }

    public static int ActorDisturbedLevel(Actor actor)
    {
      return (int) (SANITY_UNSTABLE_LEVEL * (1.0 - SKILL_STRONG_PSYCHE_LEVEL_BONUS * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.STRONG_PSYCHE)));
    }

    public static int ActorMedicineEffect(Actor actor, int baseEffect)
    {
      return baseEffect + (int)Math.Ceiling(/* (double) */ SKILL_MEDIC_BONUS * /* (int) */ (actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) * baseEffect));
    }

    public static int ActorHealChanceBonus(Actor actor)
    {
      return SKILL_HARDY_HEAL_CHANCE_BONUS * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.HARDY);
    }

    public static int ActorBarricadingPoints(Actor actor, int baseBarricadingPoints)
    {
      int barBonus = (int)(/* (double) */ SKILL_CARPENTRY_BARRICADING_BONUS * /* (int) */ (baseBarricadingPoints * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY)));    // carpentry skill

      // alpha10: tool build bonus
      if (actor.GetEquippedWeapon() is ItemMeleeWeapon melee) {
        float toolBonus = melee.Model.ToolBuildBonus;
        if (0 != toolBonus) barBonus += (int)(baseBarricadingPoints * toolBonus);
      }
      return baseBarricadingPoints + barBonus;
    }

    public static int ActorLoudNoiseWakeupChance(Actor actor, int noiseDistance)
    {
      return LOUD_NOISE_BASE_WAKEUP_CHANCE + SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LIGHT_SLEEPER) + Math.Max(0, (LOUD_NOISE_RADIUS - noiseDistance) * LOUD_NOISE_DISTANCE_BONUS);
    }

    public static int ActorTrustIncrease(Actor actor)
    {
      return 1 + SKILL_CHARISMATIC_TRUST_BONUS * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CHARISMATIC);
    }

    public static int ActorCharismaticTradeChance(Actor actor)
    {
      return SKILL_CHARISMATIC_TRADE_BONUS * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CHARISMATIC);
    }

    public static int ActorUnsuspicousChance(Actor observer, Actor actor)
    {
      const int UNSUSPICIOUS_BAD_OUTFIT_PENALTY = 75;   // these two are logically independent
      const int UNSUSPICIOUS_GOOD_OUTFIT_BONUS = 75;
      int baseChance = SKILL_UNSUSPICIOUS_BONUS * actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.UNSUSPICIOUS);

      // retain general-purpose code within the cases
      if (actor.GetEquippedItem(DollPart.TORSO) is ItemBodyArmor armor && !armor.IsNeutral) {
        int bonus() {
          switch((GameFactions.IDs)observer.Faction.ID) {
            case GameFactions.IDs.ThePolice:
              if (armor.IsHostileForCops()) return UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
              else if (armor.IsFriendlyForCops()) return UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
              break;
            case GameFactions.IDs.TheBikers: 
            case GameFactions.IDs.TheGangstas: 
              if (armor.IsHostileForBiker(observer.GangID)) return UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
              else if (armor.IsFriendlyForBiker(observer.GangID)) return UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
            break;
          }
          return 0;
        }

        baseChance += bonus();
      }
      return baseChance;
    }

    public static int ActorSpotMurdererChance(Actor spotter, Actor murderer)
    {
      return MURDERER_SPOTTING_BASE_CHANCE + MURDER_SPOTTING_MURDERCOUNTER_BONUS * murderer.MurdersCounter - MURDERER_SPOTTING_DISTANCE_PENALTY * InteractionDistance(spotter.Location, murderer.Location);
    }

    public static int ActorBiteHpRegen(Actor a, int dmg)
    {
      return dmg + (int)(/* (double) */ SKILL_ZEATER_REGEN_BONUS * /* (int) */(a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_EATER) * dmg));
    }

    public static int CorpseEatingInfectionTransmission(int infection)
    {
      return (int) (0.1 * (double) infection);
    }

    public static int InfectionForDamage(Actor infector, int dmg)
    {
      return dmg + (int) (SKILL_ZINFECTOR_BONUS * /* (int) */ (infector.Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_INFECTOR) * dmg));
    }

    public static int InfectionEffectTriggerChance1000(int infectionPercent)
    {
      return INFECTION_EFFECT_TRIGGER_CHANCE_1000 + infectionPercent / 5;
    }

    public static float CorpseDecayPerTurn(Corpse c)
    {
      return CORPSE_DECAY_PER_TURN;
    }

    public static int CorpseZombifyChance(Corpse c, WorldTime timeNow, bool checkDelay = true)
    {
      int num1 = timeNow.TurnCounter - c.Turn;
      if (checkDelay && num1 < CORPSE_ZOMBIFY_DELAY) return 0;
      int num2 = c.DeadGuy.InfectionPercent;
      if (checkDelay) {
        int num3 = num2 >= 100 ? 1 : 100 / (1 + num2);
        if (timeNow.TurnCounter % num3 != 0)
          return 0;
      }
      float num4 = 0.0f + 1f * (float) num2 - (float) (int) ((double) num1 / (double) WorldTime.TURNS_PER_DAY);
      return Math.Max(0, Math.Min(100, !timeNow.IsNight ? (int) (num4 * CORPSE_ZOMBIFY_DAY_FACTOR) : (int) (num4 * CORPSE_ZOMBIFY_NIGHT_FACTOR)));
    }

    public static int CorpseReviveHPs(Actor actor, Corpse corpse)
    {
      return 5 + actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC);
    }

    public bool CheckTrapStepOnBreaks(ItemTrap trap, MapObject mobj = null)
    {
      int breakChance = trap.Model.BreakChance;
      if (mobj != null) breakChance *= mobj.Weight;
      return RollChance(breakChance);
    }

    public bool CheckTrapEscapeBreaks(ItemTrap trap, Actor a)
    {
      return RollChance(trap.Model.BreakChanceWhenEscape);
    }

    public bool CheckTrapEscape(ItemTrap trap, Actor a)
    {
      if (trap.IsSafeFor(a)) return true;  // alpha10
      return RollChance(0 + (a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LIGHT_FEET) * SKILL_LIGHT_FEET_TRAP_BONUS + a.Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_LIGHT_FEET) * SKILL_ZLIGHT_FEET_TRAP_BONUS) + (100 - trap.Model.BlockChance * trap.Quantity));
    }

    public static int ZGrabChance(Actor grabber, Actor victim)
    {
      return grabber.Sheet.SkillTable.GetSkillLevel(Skills.IDs.Z_GRAB) * SKILL_ZGRAB_CHANCE;
    }

    // unsure where this should go...parking here for now
    public static Location PoliceRadioLocation(Location loc)
    {
      if (loc.Map == loc.Map.District.EntryMap) return loc;
      // sewers and subway are 1-1 with entry map
      if (loc.Map == loc.Map.District.SewersMap || loc.Map==loc.Map.District.SubwayMap) return new Location(loc.Map.District.EntryMap,loc.Position);
retry:
      var entry_e = loc.Map.FirstExitFor(loc.Map.District.EntryMap);
      if (null != entry_e) {
        return new Location(entry_e.Value.Value.Location.Map, entry_e.Value.Value.Location.Position + (loc.Position - entry_e.Value.Key));
      }
      // far from surface.  Currently one of hospital or police station
      var in_hospital = Session.Get.UniqueMaps.NavigateHospital(loc.Map);
      if (null != in_hospital) {
        var e = loc.Map.FirstExitFor(in_hospital.Value.Key);
#if DEBUG
        if (null == e) throw new InvalidProgramException("should be able to ascend to surface");
#endif
        loc = new Location(e.Value.Value.Location.Map, e.Value.Value.Location.Position-(loc.Position - e.Value.Key));
        goto retry;
      }
      var in_police_Station = Session.Get.UniqueMaps.NavigatePoliceStation(loc.Map);
      if (null != in_police_Station) {
        // Jails.  Considered rotated 90 counterclockwise
#if DEBUG
        if (loc.Map!=Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap) throw new InvalidProgramException("police station only has two levels");
#endif
        var e = loc.Map.FirstExitFor(in_police_Station.Value.Key);
#if DEBUG
        if (null == e) throw new InvalidProgramException("should be able to ascend to surface");
#endif
        Size raw_delta = loc.Position - e.Value.Key;
        Size delta = new Size(raw_delta.Y,-raw_delta.X);
        loc = new Location(e.Value.Value.Location.Map,e.Value.Value.Location.Position+delta);
        goto retry;
      }
      return loc;   // if not in the entry map, source map is not close to surface
    }

  } // end Rules class

// still want this
  internal static class ext_Rules
  {
    public static bool IsScheduledBefore(in this Point lhs, in Point rhs) { // Cf. ScheduleAdjacentForAdvancePlay.  Almost anti-symmetric, irreflexive relation
      if (lhs.X <= rhs.X && lhs.Y <= rhs.Y) return false;   // this quadrant comes after us.  Includes equality.
      if (lhs.X >= rhs.X && lhs.Y >= rhs.Y) return true;    // this quadrant comes before us.  Would include equality except that already happened.

      // strictly speaking, only need to be accurate for adjacent points
      Point abs_delta = (lhs-rhs).coord_xform(Math.Abs);
#if REFERENCE
      if (abs_delta.X  == 2*abs_delta.Y) return false;   // the ambiguity line; overflow-vulnerable
#endif
      int diag_delta = abs_delta.X - abs_delta.Y;
      if (diag_delta == abs_delta.Y) return false;  // the ambiguity line (why we are not anti-symmetric)
      if (lhs.Y < rhs.Y) {
        return diag_delta > abs_delta.Y; // we constrain (X-1,Y+1) so 0<1 must fail
      } else {
        return diag_delta < abs_delta.Y; // we are constrained by (X+1,Y-1) so 0 < 1 must pass
      }
    }
  }
}
