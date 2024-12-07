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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using Zaimoni.Data;

using Rectangle = Zaimoni.Data.Box2D<short>;
using Point = Zaimoni.Data.Vector2D<short>;
using Size = Zaimoni.Data.Vector2D<short>;   // likely to go obsolete with transition to a true vector type
using djack.RogueSurvivor.Engine._Action;

namespace djack.RogueSurvivor.Engine
{
  public sealed class Rules
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
    public const int UPGRADE_SKILLS_TO_CHOOSE_FROM = 5;
    public const int UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM = 2;
    public static int SKILL_AGILE_DEF_BONUS = 4;
    public static int SKILL_LIGHT_FEET_TRAP_BONUS = 15; // alpha10
    public const int SKILL_MEDIC_LEVEL_FOR_REVIVE_EST = 1;
    public const int SKILL_NECROLOGY_LEVEL_FOR_INFECTION = 3;
    public const int SKILL_NECROLOGY_LEVEL_FOR_RISE = 5;
    public static double SKILL_STRONG_PSYCHE_LEVEL_BONUS = 0.15;
    public static int SKILL_ZAGILE_DEF_BONUS = 2;
    public static int SKILL_ZLIGHT_FEET_TRAP_BONUS = 3;
    public const int STAMINA_COST_RUNNING = 4;
    public const int STAMINA_REGEN_PER_TURN = 2;
    public const int STAMINA_COST_JUMP = 8;
    public const int STAMINA_COST_MELEE_ATTACK = 8;
    public const int BARRICADING_MAX = 80;
    public const int MELEE_WEAPON_BREAK_CHANCE = 1;
    public const int MELEE_WEAPON_FRAGILE_BREAK_CHANCE = 3;
    public const int FIREARM_JAM_CHANCE_NO_RAIN = 1;
    public const int FIREARM_JAM_CHANCE_RAIN = 3;
    public const int FOOD_BASE_POINTS = 2*Actor.FOOD_HUNGRY_LEVEL;
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
    public const int SLEEP_EXHAUSTION_COLLAPSE_CHANCE = 5;
    public const int SLEEP_ON_COUCH_HEAL_CHANCE = 5;
    public const int SLEEP_HEAL_HITPOINTS = 2;
    public const int CHAT_RADIUS = 1;   // would space-time scale close to angband scale (900 turns/hour), but not much as hard to hear past 20' or so
    public const int LOUD_NOISE_RADIUS = WorldTime.TURNS_PER_HOUR/6;    // space-time scales; value 5 for 30 turns/hour
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
    public const int GIVE_RARE_ITEM_DAY = 7;
    public const int GIVE_RARE_ITEM_CHANCE = 5;
#nullable enable
    private static Rules? s_Rules;
    private DiceRoller m_DiceRoller;

    public static Rules Get { get { return s_Rules ??= new Rules(); } }
    public static void Reset() { s_Rules = null; }

#region Session save/load assistants
    public void Load(SerializationInfo info, StreamingContext context)
    {
      info.read(ref m_DiceRoller, "m_DiceRoller");
    }

    public void Save(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_DiceRoller", DiceRoller,typeof(DiceRoller));
    }

    public void Load(Zaimoni.Serialization.DecodeObjects decode) => m_DiceRoller = decode.LoadInline<DiceRoller>();
    public void Save(Zaimoni.Serialization.EncodeObjects encode) => Zaimoni.Serialization.ISave.InlineSave(encode, m_DiceRoller);

    static public void Load(ref Utf8JsonReader reader)
    {
      s_Rules = JsonSerializer.Deserialize<Rules>(ref reader, Session.JSON_opts) ?? throw new JsonException();
    }
#endregion

    public DiceRoller DiceRoller { get { return m_DiceRoller; } }

    private Rules() { m_DiceRoller = new DiceRoller(Session.Seed); }
    private Rules(DiceRoller src) { m_DiceRoller = src; }

    static private int field_code(ref Utf8JsonReader reader) {
      if (reader.ValueTextEquals("m_DiceRoller")) return 1;
      else throw new JsonException();
    }

    public static Rules fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options) {
      if (JsonTokenType.StartObject != reader.TokenType) throw new JsonException();
      reader.Read();

      if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

      int stage = field_code(ref reader);

      reader.Read();

      var stage_rng = JsonSerializer.Deserialize<DiceRoller>(ref reader, djack.RogueSurvivor.Engine.Session.JSON_opts) ?? throw new JsonException();
      reader.Read();

      return new Rules(stage_rng);
    }

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options) {
      writer.WriteStartObject();
      writer.WritePropertyName("m_DiceRoller");
      JsonSerializer.Serialize(writer, m_DiceRoller, djack.RogueSurvivor.Engine.Session.JSON_opts);
      writer.WriteEndObject();
    }

    public int Roll(int min, int max) => m_DiceRoller.Roll(min, max);
    public short Roll(int min, short max) => m_DiceRoller.Roll((short)min, max);
    public short Roll(short min, short max) => m_DiceRoller.Roll(min, max);
    public bool RollChance(int chance) => m_DiceRoller.RollChance(chance);
    public Direction RollDirection() => m_DiceRoller.Choose(Direction.COMPASS);

    public int RollSkill(int skillValue)
    {  // 2019-11-22 Release mode IL Code size       40 (0x28)
      if (skillValue <= 0) return 0;
      return (m_DiceRoller.Roll(0, skillValue + 1) + m_DiceRoller.Roll(0, skillValue + 1)) / 2;
    }

    private static int _Average(int x, int y) { return x+y/2; }
    // don't expect that many incoming values, so should be fine with this not expiring
    private readonly static Dictionary<int, DenormalizedProbability<int>> _SkillProbabilityDistribution_cache = new Dictionary<int, DenormalizedProbability<int>>();

    public static DenormalizedProbability<int> SkillProbabilityDistribution(int skillValue)
    {
      if (0 >= skillValue) return ConstantDistribution<int>.Get(0);
      lock(_SkillProbabilityDistribution_cache) {
        if (_SkillProbabilityDistribution_cache.TryGetValue(skillValue, out var cache)) return cache;
        DenormalizedProbability<int> sk_prob = UniformDistribution.Get(0,skillValue);
        var ret = DenormalizedProbability<int>.Apply(sk_prob * sk_prob, _Average);
        _SkillProbabilityDistribution_cache.Add(skillValue, ret);
        return ret;
      }
    }

    public int RollDamage(int damageValue)
    {
      if (damageValue <= 0) return 0;
      return m_DiceRoller.Roll(damageValue / 2, damageValue + 1);
    }

    public static DenormalizedProbability<int> DamageProbabilityDistribution(int damageValue)
    {
      if (damageValue <= 0) return ConstantDistribution<int>.Get(0);
      return UniformDistribution.Get(damageValue / 2, damageValue);
    }

    static public ShelfLike? CanActorPutItemIntoContainer(Actor actor, in Point position)
    {
      var mapObjectAt = actor.Location.Map.GetMapObjectAtExt(position) as ShelfLike;
      if (null == mapObjectAt) return null;
      return string.IsNullOrEmpty(mapObjectAt.ReasonCantPutItemIn(actor)) ? mapObjectAt : null;
    }
#nullable restore

    static public bool CanActorEatFoodOnGround(Actor actor, Item it, out string reason)
    {
#if DEBUG
      if (actor == null) throw new ArgumentNullException(nameof(actor));
      if (it == null) throw new ArgumentNullException(nameof(it));
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

    private static ActorAction? IsBumpableFor(Actor actor, Map map, Point point, out string reason)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
      reason = "";
      Location loc = new Location(map,point);
      if (!actor.CanEnter(ref loc)) {
        var targets = actor.GetMoveBlockingActors(point);
        var m_attack = targets.CanBumpMelee(actor, point, out reason);
        if (null != m_attack) return m_attack;
        var act = loc.MapObject?.IsBumpableFor(actor, out reason);
        if (null != act) return act;
        reason = "blocked";
        return null;
      }
      if (1>=actor.Controller.FastestTrapKill(in loc)) {
        reason = "deathtrapped";
        return null;
      }

      var actors_in_way = actor.GetMoveBlockingActors(point);
      var melee = actors_in_way.CanBumpMelee(actor, point, out reason);
      if (null != melee) return melee;
      if (actors_in_way.TryGetValue(point,out Actor actorAt)) {
		// player as leader should be able to switch with player as follower
		// NPCs shouldn't be leading players anyway
        if ((actor.IsPlayer || !actorAt.IsPlayer) && actor.CanSwitchPlaceWith(actorAt, out reason))
          return new ActionSwitchPlace(actor, actorAt);
        if (!actor.CanChatWith(actorAt, out reason)) return null;
        if (actor.Controller is Gameplay.AI.OrderableAI myai && myai.ProposeSwitchPlaces(actorAt.Location)) {
          if (actorAt.Controller is Gameplay.AI.OrderableAI oai && !oai.RejectSwitchPlaces(actor.Location)) {
            return new ActionSwitchPlaceEmergency(actor,actorAt);    // this is an AI cheat so shouldn't be happening that much
          }
        }
        return new ActionChat(actor, actorAt);
      }
      var mapObjectAt = loc.MapObject;
      if (!map.IsInBounds(point)) {
        if (!actor.CanLeaveMap(point, out reason)) return null;
        if (actor.IsPlayer) {
          var obj_act = (actor.Controller as Gameplay.AI.ObjectiveAI)?.WouldGetFrom(mapObjectAt as ShelfLike);
          var inv_act = (actor.Controller as Gameplay.AI.ObjectiveAI)?.WouldGetFrom(in loc);
          if (null != obj_act || null != inv_act) {
            List<ActorAction> stage = new() { new ActionLeaveMap(actor, in point) };
            if (null != obj_act) stage.Add(obj_act);
            if (null != inv_act) stage.Add(inv_act);
            return new Choice(actor, stage);
          }
        }
	    return new ActionLeaveMap(actor, in point);
      }
      var actionMoveStep = new ActionMoveStep(actor, in point);
      if (actionMoveStep.IsPerformable()) {
        reason = "";
        if (actor.IsPlayer) {
          var obj_act = (actor.Controller as Gameplay.AI.ObjectiveAI)?.WouldGetFrom(mapObjectAt as ShelfLike);
          var inv_act = (actor.Controller as Gameplay.AI.ObjectiveAI)?.WouldGetFrom(in loc);
          if (null != obj_act || null != inv_act) {
            List<ActorAction> stage = new() { actionMoveStep };
            if (null != obj_act) stage.Add(obj_act);
            if (null != inv_act) stage.Add(inv_act);
            return new Choice(actor, stage);
          }
        }
        return actionMoveStep;
      }
      reason = actionMoveStep.FailReason;
      return mapObjectAt?.IsBumpableFor(actor, out reason);
    }

    public static ActorAction IsBumpableFor(Actor actor, in Location location)
    {
      return IsBumpableFor(actor, in location, out string reason);
    }

    public static ActorAction IsBumpableFor(Actor actor, in Location location, out string reason)
    {
      return IsBumpableFor(actor, location.Map, location.Position, out reason);
    }

#nullable enable
    // Key is Adjacent, Value is non-Adjacent
    public static KeyValuePair<IEnumerable<KeyValuePair<Location, T>>, IEnumerable<KeyValuePair<Location, T>>> ClassifyAdjacent<T>(Dictionary<Location, T> src, Location origin)
    {
      return new KeyValuePair<IEnumerable<KeyValuePair<Location, T>>, IEnumerable<KeyValuePair<Location, T>>>(src.Where(pt => IsAdjacent(origin, pt.Key)), src.Where(pt => !IsAdjacent(origin, pt.Key)));
    }

    public static List<KeyValuePair<Location, T>>? PreferNonAdjacent<T>(Dictionary<Location, T> src, Location origin)
    {
      var test = ClassifyAdjacent(src, origin);
      if (test.Value.Any()) return test.Value.ToList();
      if (test.Key.Any()) return test.Key.ToList();
      return null;
    }

    public static void MaximizeVisibility<T>(ref List<KeyValuePair<Location, T>> candidates, Actor actor, MapObject obj)
    {
      var threat = actor.Threats; // assumed that Threats and InterestingLocs are either both null, or both non-null
      if (null != threat && 2 <= candidates.Count) {
        var radius = Actor.MAX_VISION + 1;
        var domain = new Rectangle(actor.Location.Position - (Point)radius, (Size)(2*radius +1));
        var a_map = actor.Location.Map;
        var dests = threat.ThreatWhere(a_map, domain);
        dests.UnionWith(actor.InterestingLocs.In(a_map, domain));
        if (0 < dests.Count) {
          var z = new ZoneLoc(a_map, domain);
          var mk = new MapKripke(z, actor);
          var los = new Dictionary<Location, HashSet<Point>>();
          var fov_range = actor.FOVrange(a_map.LocalTime, World.Get.Weather);
          int max_los = 0;
          foreach (var x in candidates) {
            mk.Place(obj, x.Key);
            var vision = LOS.ComputeFOVFor(mk, obj.Location, fov_range);
            vision.IntersectWith(dests);
            if (vision.Count > max_los) {
              max_los = vision.Count;
              los.Clear();
              los.Add(x.Key, vision);
            } else if (vision.Count == max_los) {
              los.Add(x.Key, vision);
            }
          }
          candidates.OnlyIf(loc_dir => los.ContainsKey(loc_dir.Key));
        }
      }
    }

#nullable restore

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

      // While this is a logical point for breaking off full pathability processing, the tests are themselves surprisingly expensive
      // 2019-10-31: both actor.Controller.CanSee(loc) and outside-of-FOV range test are profile-slow

      // unclear whether B_MOVIE_MARTIAL_ARTS requires pathfinding changes or not; changes would go here
      var map = loc.Map;
      var point = loc.Position;
      if (!map.IsInBounds(point)) {
	    return (actor.CanLeaveMap(point, out reason) ? new ActionLeaveMap(actor, in point) : null);
      }
      if (loc.IsWalkableFor(actor, out reason)) {
        var actionMoveStep = new ActionMoveStep(actor, in loc);
        if (map!=actor.Location.Map) {
          // check for exit leading here and substitute if so.  Cf. BaseAI::BehaviorUseExit
          var exit = actor.Model.Abilities.AI_CanUseAIExits ? actor.Location.Exit : null;
          if (null != exit && exit.Location==loc) {
           if (exit.IsNotBlocked(out var a, out var obj, actor)) return new ActionUseExit(actor, actor.Location);
           if (a != null && actor.IsEnemyOf(a) && actor.CanMeleeAttack(a)) return new ActionMeleeAttack(actor, a);
           if (obj != null && actor.CanBreak(obj)) return new ActionBreak(actor, obj);
           return null;
          }
        }
        return actionMoveStep;
      }

      Gameplay.AI.ObjectiveAI ai = actor.Controller as Gameplay.AI.ObjectiveAI;

      // respec of IsWalkableFor guarantees that any actor will be adjacent
      var actorAt = map.GetActorAt(point);
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
           push_dest.OnlyIf(loc => Gameplay.AI.ObjectiveAI.VetoExit(actorAt, loc.Exit));

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
             var test = ClassifyAdjacent(push_dest, actor.Location);
             var candidates = (i_can_help && test.Value.Any()) ? test.Value.ToList() : null;
             if (null == candidates && !i_am_in_his_way && i_can_help && test.Key.Any()) candidates = test.Key.ToList();
             if (null == candidates && i_am_in_his_way) {
               // HMM...maybe step aside instead?
               var considering = actor.MutuallyAdjacentFor(actor.Location,actorAt.Location);
               if (null != considering) {
                 considering = considering.FindAll(pt => pt.IsWalkableFor(actor));
                 if (0 < considering.Count) return new ActionMoveStep(actor, Get.DiceRoller.Choose(considering));
               }
             }

             // legacy initialization
             if (null == candidates && test.Value.Any()) candidates = test.Value.ToList();
             if (null == candidates && test.Key.Any()) candidates = test.Key.ToList();
             // end function target

             if (null != candidates) return new ActionShove(actor,actorAt, Get.DiceRoller.Choose(candidates).Value);
           }
        }

        // check for mutual-advantage switching place between ais
        if (actor.Controller is Gameplay.AI.OrderableAI myai && myai.ProposeSwitchPlaces(actorAt.Location)) {
          if (actorAt.Controller is Gameplay.AI.OrderableAI oai && !oai.RejectSwitchPlaces(actor.Location)) {
            return new ActionSwitchPlaceEmergency(actor,actorAt);    // this is an AI cheat so shouldn't be happening that much
          }
        }

        // consider re-legalizing chat here
        return null;
      }
      var mapObjectAt = map.GetMapObjectAt(point);
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
        var act = ai?.WouldGetFrom(mapObjectAt as ShelfLike);
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

               var candidates = PreferNonAdjacent(push_dest, actor.Location);
               if (null != candidates) {
                 MaximizeVisibility(ref candidates, actor, mapObjectAt);
                 return new ActionPush(actor,mapObjectAt, Get.DiceRoller.Choose(candidates).Value);
               }
             } else {
               // proceed with pull if we can't push safely
               var pull_dests = mapObjectAt.Location.Adjacent(loc => 1 == GridDistance(actor.Location, loc));
               if (null != pull_dests) return new ActionPull(actor, mapObjectAt, Get.DiceRoller.Choose(pull_dests));
             }
           }
        }

        // \todo consider eliminating power generators as a pathable target (rely on behaviors instead and path to adjacent squares)
        if (mapObjectAt is PowerGenerator powGen) {
          if (powGen.IsOn) {
            var charge = ActionRechargeItemBattery.Recharge(actor, actor.GetEquippedItem(DollPart.LEFT_HAND));   // normal lights and trackers
            if (null != charge) return charge;
            charge = ActionRechargeItemBattery.Recharge(actor, actor.GetEquippedItem(DollPart.RIGHT_HAND));  // formal correctness
            if (null != charge) return charge;
            charge = ActionRechargeItemBattery.Recharge(actor, actor.GetEquippedItem(DollPart.HIP_HOLSTER)); // the police tracker
            if (null != charge) return charge;
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

    // These two somewhat counter-intuitively consider "same location" as adjacent
    public static bool IsAdjacent(in Location a, in Location b)
    {
      if (a.Map != b.Map) {
        Location? test = a.Map.Denormalize(in b);
        if (null == test) {
          var exit = a.Exit;
          return null != exit && exit.Location == b;
        }
        return IsAdjacent(a.Position, test.Value.Position);
      }
      return IsAdjacent(a.Position, b.Position);
    }

    public static bool IsAdjacent(in Point pA, in Point pB)
    {
      return Math.Abs(pA.X - pB.X) < 2 && Math.Abs(pA.Y - pB.Y) < 2;
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

#nullable enable
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
        var exit = from.Exit;
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
          var exit = a.Exit;
          return (null != exit && exit.Location == b) ? 1 : int.MaxValue;
        }
        return GridDistance(a.Position, test.Value.Position);
      }
      return GridDistance(a.Position, b.Position);
    }

    public static int ZoneWalkDistance(in Location a, in Location b)
    {
      if (a.Map == b.Map) return GridDistance(a.Position, b.Position);
      Location? test = a.Map.Denormalize(in b);
      if (null != test) return GridDistance(a.Position, test.Value.Position);
      var exit = b.Exit;
      if (null != exit) {
        if (a.Map == exit.Location.Map) return GridDistance(a.Position, exit.Location.Position)+1;
        test = a.Map.Denormalize(exit.Location);
        if (null != test) return GridDistance(a.Position, test.Value.Position)+1;
      }
      exit = a.Exit;
      if (null != exit) {
        if (b.Map == exit.Location.Map) return GridDistance(b.Position, exit.Location.Position)+1;
        test = b.Map.Denormalize(exit.Location);
        if (null != test) return GridDistance(b.Position, test.Value.Position)+1;
      }
      return int.MaxValue;
    }

    // for jump point pathing
    public static int L1_Distance(Point pt) { return Math.Abs(pt.X)+Math.Abs(pt.Y); }
    public static int L1_Distance(in Point pA, in Point pB) { return L1_Distance(pB - pA); }

    public static int L1_Distance(in Location a, in Location b)
    {
      if (null == a.Map) return int.MaxValue;
      if (null == b.Map) return int.MaxValue;
      if (a.Map == b.Map) return L1_Distance(b.Position - a.Position);
      Location? tmp = a.Map.Denormalize(in b);
      if (null == tmp) return int.MaxValue;
      return L1_Distance(tmp.Value.Position - a.Position);
    }

    public static int JumpPoint_Distance(in Point a, in Point b)
    {
      var pt = a - b;
      var grid = GridDistance(pt);
      if (1 >= grid) return grid;
      return L1_Distance(pt);
    }

    public static int JumpPoint_Distance(in Location a, in Location b)
    {
      if (null == a.Map) return int.MaxValue;
      if (null == b.Map) return int.MaxValue;
      if (a.Map == b.Map) return JumpPoint_Distance(a.Position, b.Position);
      Location? tmp = a.Map.Denormalize(in b);
      if (null == tmp) return int.MaxValue;
      return JumpPoint_Distance(a.Position, tmp.Value.Position);
    }

    public static bool IsMurder(Actor? killer, Actor victim)
    {
      if (null == killer) return false;
      if (victim.Model.Abilities.IsUndead) return false;
      bool k_first_class = killer.IsFirstClassCitizen();
      // 2024-10-09 zaimoni: for now, pretend army has access to police records
      if (k_first_class) {
        if (Session.Get.Police.IsTargeted(victim)) return false;
        // legacy
        if (0 < victim.MurdersOnRecord(killer)) return false;
      }
#if POLICE_NO_QUESTIONS_ASKED
      if (killer.Model.Abilities.IsLawEnforcer && killer.Threats.IsThreat(victim)) return false;
#endif
      if (killer.Faction.IsEnemyOf(victim.Faction)) return false;

      // If your leader is a cop i.e. First Class Citizen, killing his enemies should not trigger murder charges.
      var killer_leader = killer.LiveLeader;
      if (!k_first_class) {
        if (null != killer_leader && killer_leader.IsFirstClassCitizen()) {
          if (Session.Get.Police.IsTargeted(victim)) return false;
          if (victim.MurdersOnRecord(killer_leader) > 0) return false;
          if (killer_leader.IsEnemyOf(victim)) return false;
          if (victim.IsSelfDefenceFrom(killer_leader)) return false;  // XXX redundant?
        }
      }

      // Framed for murder.  Since this is an apocalypse, self-defence doesn't count no matter what the law was pre-apocalypse
      if (victim.IsFirstClassCitizen()) return true;
      if (victim.LiveLeader?.IsFirstClassCitizen() ?? false) return true;

      // resume old definition
      if (killer.IsSelfDefenceFrom(victim)) return false;

      // XXX RS Alpha 9; went away in RS Alpha 10.  The important case (police leading civilian) is handled above.  May need reimplementing (cf. Actor::ChainOfCommand)
      if (killer_leader?.IsSelfDefenceFrom(victim) ?? false) return false;
      if (null != killer.FirstFollower(fo => !fo.IsSelfDefenceFrom(victim))) return false;

      return true;
    }

    public static int InfectionEffectTriggerChance1000(int infectionPercent)
    {
      const int INFECTION_EFFECT_TRIGGER_CHANCE_1000 = 2;
      return INFECTION_EFFECT_TRIGGER_CHANCE_1000 + infectionPercent / 5;
    }

    public bool CheckTrapEscapeBreaks(ItemTrap trap, Actor a)
    {
      return RollChance(trap.Model.BreakChanceWhenEscape);
    }

    public bool CheckTrapEscape(ItemTrap trap, Actor a)
    {
      if (trap.IsSafeFor(a)) return true;  // alpha10
      return RollChance(trap.EscapeChanceFor(a));
    }
#nullable restore

    // unsure where this should go...parking here for now
    public static Location PoliceRadioLocation(Location loc)
    {
      if (District.IsEntryMap(loc.Map)) return loc;
      // sewers and subway are 1-1 with entry map
      if (District.IsSubwayOrSewersMap(loc.Map)) return new Location(loc.Map.District.EntryMap,loc.Position);
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
        Size delta = new(raw_delta.Y, (short)(-raw_delta.X));
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

    public static ActorAction? IsBumpableFor(this MapObject mapObjectAt, Actor actor,  out string reason) {
      reason = null;
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
      var act = (actor.Controller as Gameplay.AI.ObjectiveAI)?.WouldGetFrom(mapObjectAt as ShelfLike);
      if (null != act) return act;
        // release block: \todo would like to restore inventory-grab capability for InsaneHumanAI (and feral dogs, when bringing them up)
        // only Z want to break arbitrary objects; thus the guard clause
      if (actor.Model.Abilities.CanBashDoors && actor.CanBreak(mapObjectAt, out reason))
          return new ActionBreak(actor, mapObjectAt);
      if (mapObjectAt is PowerGenerator powGen) {
          if (powGen.IsOn) {
            var charge = ActionRechargeItemBattery.Recharge(actor, actor.GetEquippedItem(DollPart.LEFT_HAND));   // normal lights and trackers
            if (null != charge) return charge;
            charge = ActionRechargeItemBattery.Recharge(actor, actor.GetEquippedItem(DollPart.RIGHT_HAND));  // formal correctness
            if (null != charge) return charge;
            charge = ActionRechargeItemBattery.Recharge(actor, actor.GetEquippedItem(DollPart.HIP_HOLSTER)); // the police tracker
            if (null != charge) return charge;
          }
          if (actor.CanSwitch(powGen, out reason)) {
             if (actor.IsPlayer || !powGen.IsOn) return new ActionSwitchPowerGenerator(actor, powGen);
                }
                return null;
            }
            return null;
        }

    public static ActionMeleeAttack? CanBumpMelee(this Dictionary<Point, Actor> actors_in_way, Actor actor, Point point, out string reason) {
#if B_MOVIE_MARTIAL_ARTS
      ActionMeleeAttack? find_target(Dictionary<Point, Actor> actors_in_way, Actor actorAt, out string reason) {
          reason = null;
          Actor? target = null;
          foreach(var pt_actor in actors_in_way.Values) {
            if (pt_actor == actorAt) continue;
            if (!actor.CanMeleeAttack(pt_actor, out reason)) continue;
            if (null == target || target.HitPoints>pt_actor.HitPoints) target = pt_actor;
          }
          return (null != target ? new ActionMeleeAttack(actor, target) : null);
      }
#endif

      reason = null;
      if (actors_in_way.TryGetValue(point,out Actor actorAt)) {
        if (actor.IsEnemyOf(actorAt)) {
          return (actor.CanMeleeAttack(actorAt, out reason) ? new ActionMeleeAttack(actor, actorAt) : null);
        }
#if B_MOVIE_MARTIAL_ARTS
        if (1<actors_in_way.Count) return find_target(actors_in_way, actorAt, out reason);
#endif
#if B_MOVIE_MARTIAL_ARTS
      } else if (0 < actors_in_way.Count) {   // range-2 issue.  Identify weakest enemy.
         return find_target(actors_in_way, actor, out reason); // not really
#endif
      }
      return null;
    }
  }
}

namespace Zaimoni.JsonConvert
{
    public class Rules : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.Rules>
    {
        public override djack.RogueSurvivor.Engine.Rules Read(ref System.Text.Json.Utf8JsonReader reader, Type src, System.Text.Json.JsonSerializerOptions options)
        {
            return djack.RogueSurvivor.Engine.Rules.fromJson(ref reader, options);
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, djack.RogueSurvivor.Engine.Rules src, System.Text.Json.JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
