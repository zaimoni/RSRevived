// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.BaseAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal abstract class BaseAI : ActorController
    {
    protected const int FLEE_THROUGH_EXIT_CHANCE = 90;  // alpha10 increased from 50%
    protected const int EMOTE_FLEE_CHANCE = 30;
    protected const int EMOTE_FLEE_TRAPPED_CHANCE = 50;
    protected const int EMOTE_CHARGE_CHANCE = 30;
    private const float MOVE_DISTANCE_PENALTY = 0.42f;
    private const float MOVE_INTO_TRAPS_PENALTY = 1;  // alpha10
    public const int MAX_EMOTES = 3;    // 0: flee; 1: last stand; 2:charge

    private Location m_prevLocation;
    [NonSerialized] protected RouteFinder m_RouteFinder;    // alpha10
#nullable enable
    [NonSerialized] protected List<Percept>? _all;           // cache variables of use to all AI subclasses
    [NonSerialized] protected List<Percept_<Actor>>? _enemies;       // usage varies...Z don't care about "current", OrderableAI subclasses do
#nullable restore
    [NonSerialized] private bool _processing;

    protected BaseAI(Actor src) : base(src) {}

    public Location PrevLocation { get { return m_prevLocation; } }
    public void UpdatePrevLocation() { m_prevLocation = m_Actor.Location; } // for PlayerController

    protected void _initAICache()
    {
      _processing = true;
    }

    protected virtual void ResetAICache()
    {
      _all = null;
      _enemies = null;
      _processing = false;
    }

    public override bool IsMyTurn() { return _processing; }

    protected virtual void RecordLastAction(ActorAction act) { }    // no-op at this level

    public override ActorAction? GetAction()
    {
      if (m_prevLocation.Map == null) m_prevLocation = m_Actor.Location;
      m_Actor.TargetActor = null;
      var actorAction = SelectAction();
#if DEBUG
      if (null != actorAction && !actorAction.IsPerformable()) throw new InvalidOperationException("illegal action returned from SelectAction");
#endif
      if ((this is ObjectiveAI ai)) {
        if (ai.VetoAction(actorAction)) actorAction = new ActionWait(m_Actor);
#if DEBUG
        bool is_rewritable = actorAction is ActionMoveStep || actorAction is ActionMoveDelta || actorAction is ActionWait
                          || actorAction is ActionCloseDoor || actorAction is Resolvable || actorAction is ActionLeaveMap
                          || actorAction is ActionSleep || actorAction is ActionChat || actorAction is ActionPush || actorAction is ActionPull
                          || actorAction is ActionShove;
        var backup = actorAction;
#endif
        var alt = ai.RewriteAction(actorAction);
        if (null!=alt && alt.IsPerformable()) actorAction = alt;
#if DEBUG
        if (!is_rewritable && (actorAction is ActionMoveStep || actorAction is ActionMoveDelta)) throw new InvalidOperationException("tracing");
#endif
        ai.ScheduleFollowup(actorAction);
      }
      ResetAICache();
      RecordLastAction(actorAction);
      if (!(actorAction is ActionCloseDoor)) m_prevLocation = m_Actor.Location;
      return actorAction ?? new ActionWait(m_Actor);
    }

    /// <returns>non-null, action x for which x.IsPerformable() is true</returns>
#nullable enable
    protected abstract ActorAction? SelectAction();

/*
    NOTE: List<Percept>, as a list data structure, takes O(n) time/RAM to reset its capacity down
    to its real size.  Since C# is a garbage collected language, this would actually worsen
    the RAM loading until the next explicit GC.Collect() call (typically within a fraction of a second).
    The only realistic mitigation is to pro-rate the capacity request.
 */
    // April 22, 2016: testing indicates this does not need micro-optimization
    protected List<Percept_<_T_>>? FilterSameMap<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      Map map = m_Actor.Location.Map;
      Func< Percept_ < _T_ >,bool> same_map = p => null != map.Denormalize(p.Location);
      var e = m_Actor.Location.Exit;
      if (null != e) same_map = same_map.Or(p => e.Location==p.Location);
      return percepts.Filter(same_map);
    }

    protected List<Percept_<Actor>>? FilterEnemies(List<Percept>? percepts)
    {
      return percepts?.FilterCast<Actor>(target => target!=m_Actor && m_Actor.IsEnemyOf(target));
    }

    protected List<Percept_<Actor>>? FilterNonEnemies(List<Percept>? percepts)
    {
      return percepts?.FilterCast<Actor>(target => target!=m_Actor && !m_Actor.IsEnemyOf(target));
    }

    protected List<Percept_<_T_>>? FilterCurrent<_T_>(List<Percept_<_T_>>? percepts) where _T_:class
    {
      return percepts?.FilterCurrent(m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    protected List<Percept_<_T_>>? FilterOld<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      return percepts.FilterOld(m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    // GangAI's mugging target selection triggered a race condition
    // that allowed a non-null non-empty percepts
    // to be seen as returning null from FilterNearest anyway, from
    // the outside (Contracts saw a non-null return)
    protected Percept_<_T_>? FilterNearest<_T_>(List<Percept_<_T_>>? percepts) where _T_:class
    {
      return percepts?.Minimize(p=>Rules.InteractionStdDistance(m_Actor.Location, p.Location));
    }

    public KeyValuePair<Location, T> FilterNearest<T>(Dictionary<Location,T>? src)
    {
        return src?.Minimize(x => Rules.InteractionDistance(m_Actor.Location, x.Key)) ?? default;
    }

    protected List<Percept_<Actor>>? FilterFireTargets(List<Percept_<Actor>>? percepts, int range)
    {
      return percepts?.Filter(target => m_Actor.CanFireAt(target,range));
    }

    protected List<Percept_<Actor>>? FilterPossibleFireTargets(List<Percept_<Actor>>? percepts)
    {
      return percepts?.Filter(target => m_Actor.CouldFireAt(target));
    }

    protected bool AnyContrafactualFireTargets(List<Percept_<Actor>> percepts, Point p)
    {
      return percepts.Any(target => m_Actor.CanContrafactualFireAt(target,p));
    }

    // firearms use grid i.e. L-infinity distance
    protected List<Percept_<_T_>>? SortByGridDistance<_T_>(List<Percept_<_T_>>? percepts) where _T_:class
    {
      Location from = m_Actor.Location;
      return percepts?.SortIncreasing(p => Rules.InteractionDistance(p.Location, from));
    }
#nullable restore

    // policy change for behaviors: unless the action from a behavior is being used to decide whether to commit to the behavior,
    // a behavior should handle all free actions itself and return only non-free actions.

    /// <param name="exploration">can be null for ais with no exploration</param>
    protected ActorAction? BehaviorWander(ExplorationData? exploration=null, Predicate<Location>? goodWanderLocFn=null)
    {
      var choiceEval = Choose(Direction.COMPASS, dir => {
        Location next = m_Actor.Location + dir;
        if (null != goodWanderLocFn && !goodWanderLocFn(next)) return float.NaN;
        var bump = Rules.IsBumpableFor(m_Actor, in next);
        if (!IsValidWanderAction(bump)) return float.NaN;
        if (!Map.Canonical(ref next)) return float.NaN;
#if DEBUG
        if (!bump.IsPerformable()) throw new InvalidOperationException("non-null non-performable bump"); 
#else
        if (!bump.IsPerformable()) return float.NaN;
#endif

        int score = 0;

        // alpha10.1
        const int BREAKING_OBJ = -50000;
        const int BACKTRACKING = -10000;
        const int BREAKING_BARRICADES = -1000;
        const int AVOID_TRAPS = -1000;
        const int UNEXPLORED_LOC = 1000;  // should not happen, see below
        const int DOORWINDOWS = 100;
        const int EXITS = 50;
        const int INSIDE_WHEN_ALMOST_SLEEPY = 100;
        const int WANDER_RANDOM = 10;  // alpha10.1 much smaller random factor

        if (next == m_prevLocation) score += BACKTRACKING;
        if (m_Actor.Model.Abilities.IsIntelligent && 0 < next.Map.TrapsUnavoidableMaxDamageAtFor(next.Position,m_Actor)) score += AVOID_TRAPS;

        // alpha10.1 prefer unexplored/oldest
        // unexplored should not happen because exploration rule is tested before wander rule but just to be more robust...
        if (exploration != null) {
          int locAge = exploration.GetExploredAge(in next);
          score += (0 == locAge) ? UNEXPLORED_LOC : locAge;
        }

        // alpha10.1 prefer wandering to doorwindows and exits. 
        // helps civs ai getting stuck in semi-infinite loop when running out of new exploration to do.
        // as a side effect, make ais with no exploration data (eg zombies) more eager to visit door/windows and exits.
        if (next.MapObject is DoorWindow) score += DOORWINDOWS;
        var e = next.Exit;
        if (null != e) score += (e.Location==m_prevLocation ? BACKTRACKING : EXITS);    // Staying Alive: match fix to BehaviorExplore

        // alpha10.1 prefer inside when almost sleepy
        if (m_Actor.IsAlmostSleepy && next.Map.IsInsideAt(next.Position)) score += INSIDE_WHEN_ALMOST_SLEEPY;

        // alpha10.1 add random factor
        score += Rules.Get.Roll(0, WANDER_RANDOM);

        return (float) score;
      }, (a, b) => a > b);
      m_Actor.Activity = Activity.IDLE;
      return (choiceEval != null ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

        // alpha10
        /// <summary>
        /// For intelligent npcs, additional cost to distance cost when chosing which adj tile to bump to.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <see cref="BehaviorBumpToward(RogueGame, Point, bool, bool, Func{Point, Point, float})"/>
        protected float EstimateBumpActionCost(in Location loc, ActorAction action)
        {
            float cost = 0;

            if (action is ActionBump bump) action = bump.ConcreteAction;

            // Consuming additional sta
            if (m_Actor.Model.Abilities.CanTire) {
                // jumping
                if (action is ActionMoveStep) {
                    if (Location.RequiresJump(in loc)) cost = MOVE_DISTANCE_PENALTY;
                }

                // actions that always consume sta or may take more than one turn
                if (action is ActionBashDoor || action is ActionBreak || action is ActionPush) cost = MOVE_DISTANCE_PENALTY;
            }

            return cost;
        }

    protected ActorAction? BehaviorBumpToward(Point goal, bool canCheckBreak, bool canCheckPush, Func<Point, Point, float> distanceFn)
    {
#if DEBUG
      if (null == distanceFn) throw new ArgumentNullException(nameof(distanceFn));
#endif
      var choiceEval = ChooseExtended(Direction.COMPASS, dir => {
        Location next = m_Actor.Location + dir;
        ActorAction a = Rules.IsBumpableFor(m_Actor, in next);
        if (a == null) {
          if (m_Actor.Model.Abilities.IsUndead && m_Actor.AbleToPush) {
            var mapObjectAt = next.MapObject;
            if (mapObjectAt != null && m_Actor.CanPush(mapObjectAt)) {
              Direction pushDir = Rules.Get.RollDirection();
              if (mapObjectAt.CanPushTo(mapObjectAt.Location.Position + pushDir))
                return new ActionPush(m_Actor, mapObjectAt, pushDir);
            }
          }

          // alpha10 check special actions
          if (canCheckBreak) {
            var obj = m_Actor.Location.Map.GetMapObjectAt(next.Position);
            if (null != obj && m_Actor.CanBreak(obj)) return new ActionBreak(m_Actor, obj);
          }
          if (canCheckPush) {
            var obj = m_Actor.Location.Map.GetMapObjectAt(next.Position);
            if (null != obj && m_Actor.CanPush(obj)) {
                // push in a valid direction at random
                List<Direction> validPushes = new List<Direction>(8);
                foreach (Direction pushDir in Direction.COMPASS) {
                  var dest = obj.Location.Position + pushDir;
                  if (obj.Location.Map.IsInBounds(dest) && obj.Location.Map.HasExitAt(in dest)) continue;  // constructor crash
                  if (obj.CanPushTo(dest)) validPushes.Add(pushDir);
                }
                if (validPushes.Count > 0) return new ActionPush(m_Actor, obj, Rules.Get.DiceRoller.Choose(validPushes));
            }
          }

          return null;
        }
        return (next.Position == goal || IsValidMoveTowardGoalAction(a)) ? a : null;
      }, (dir, action) => {
        Location next = m_Actor.Location + dir;
        float cost = distanceFn(next.Position, goal);

        // alpha10 add action cost heuristic if npc is intelligent
        if (!float.IsNaN(cost) && m_Actor.Model.Abilities.IsIntelligent) cost += EstimateBumpActionCost(in next, action);

        return cost;
      }, (a, b) => a < b);
      if (choiceEval != null) return choiceEval.Choice;
      return null;
    }

#nullable enable
    protected ActorAction? BehaviorBumpToward(Location goal, bool canCheckBreak, bool canCheckPush, Func<Point, Point, float> distanceFn)
    {
      var map = m_Actor.Location.Map;
      if (map == goal.Map) return BehaviorBumpToward(goal.Position, canCheckBreak, canCheckPush, distanceFn);
      Location? test = map.Denormalize(in goal);
      if (null == test) return null;
      return BehaviorBumpToward(test.Value.Position, canCheckBreak, canCheckPush, distanceFn);
    }

    protected ActorAction? BehaviorStupidBumpToward(Point goal, bool canCheckBreak, bool canCheckPush)
    {
      return BehaviorBumpToward(goal, canCheckBreak, canCheckPush, (ptA, ptB) => {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(in ptA, in ptB);
        if (!m_Actor.Location.Map.IsWalkableFor(ptA, m_Actor)) num += MOVE_DISTANCE_PENALTY;
        return num;
      });
    }

    protected ActorAction? BehaviorStupidBumpToward(in Location goal, bool canCheckBreak, bool canCheckPush)
    {
      var map = m_Actor.Location.Map;
      if (map == goal.Map) return BehaviorStupidBumpToward(goal.Position, canCheckBreak, canCheckPush);
      Location? test = map.Denormalize(in goal);
      if (null == test) return null;
      return BehaviorStupidBumpToward(test.Value.Position, canCheckBreak, canCheckPush);
    }

    protected ActorAction? BehaviorIntelligentBumpToward(Point goal, bool canCheckBreak, bool canCheckPush)
    {
      float currentDistance = (float)Rules.StdDistance(m_Actor.Location.Position, in goal);
      Func<Point,Point,float> close_in = (ptA,ptB) => {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(in ptA, in ptB);
        return (num >= currentDistance) ? float.NaN : num;
      };

      ActorCourage courage = (this as OrderableAI)?.Directives.Courage ?? ActorCourage.CAUTIOUS;
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
      if (imStarvingOrCourageous) close_in = close_in.Postprocess((ptA,ptB,dist) => {
          var loc = new Location(m_Actor.Location.Map, ptA);
          if (!Map.Canonical(ref loc)) return float.NaN;
          var turns_to_fatality = m_Actor.Controller.FastestTrapKill(in loc);
          if (1 >= turns_to_fatality) return float.NaN;
          if (int.MaxValue > turns_to_fatality) return dist + MOVE_INTO_TRAPS_PENALTY;
          return dist;
      });
       
      return BehaviorBumpToward(goal, canCheckBreak, canCheckPush, close_in);
    }

    protected ActorAction? BehaviorIntelligentBumpToward(in Location goal, bool canCheckBreak, bool canCheckPush)
    {
      var map = m_Actor.Location.Map;
      if (map == goal.Map) return BehaviorIntelligentBumpToward(goal.Position, canCheckBreak, canCheckPush);
      Location? test = map.Denormalize(in goal);
      if (null == test) return null;
      return BehaviorIntelligentBumpToward(test.Value.Position, canCheckBreak, canCheckPush);
    }

    protected ActorAction? BehaviorHeadFor(in Location goal, bool canCheckBreak, bool canCheckPush)
    {
      if (m_Actor.Model.Abilities.IsIntelligent) return BehaviorIntelligentBumpToward(in goal, canCheckBreak, canCheckPush);
      return BehaviorStupidBumpToward(in goal, canCheckBreak, canCheckPush);
    }

    public ActorAction? BehaviorHeadFor(IEnumerable<Location>? goals, bool canCheckBreak, bool canCheckPush)
    {
      if (null == goals || !goals.Any()) return null;
      ActorAction? tmp;
      int dist = int.MaxValue;
      ActorAction? ret = null;
      foreach(Location goal in goals) {
        int new_dist = Rules.GridDistance(m_Actor.Location, in goal);
        if (dist <= new_dist) continue;
        tmp = BehaviorHeadFor(in goal, canCheckBreak, canCheckPush);
        if (null == tmp) continue;
        dist = new_dist;
        ret = tmp;
      }
      return ret;
    }

    // A number of the melee enemy targeting sequences not only work on grid distance,
    // they need to return a coordinated action/target pair.
    // We assume the list is sorted in increasing order of grid distance
    protected ActorAction? TargetGridMelee(List<Percept_<Actor>>? perceptList)
    {
      if (null == perceptList) return null; // inefficient, but reduces lines of code elsewhere
      foreach (var percept in perceptList) {
        var tmp = BehaviorStupidBumpToward(percept.Location, true, true);
        if (null != tmp) {
          m_Actor.Activity = Activity.CHASING;
          m_Actor.TargetActor = percept.Percepted;
          return tmp;
        }
      }
      return null;
    }
#nullable restore

    protected ActorAction BehaviorWalkAwayFrom(IEnumerable<Point> goals)
    {
      var leader = m_Actor.LiveLeader;
      var choiceEval = Choose(Direction.COMPASS, dir => {
        Location location = m_Actor.Location + dir;
        var bump = Rules.IsBumpableFor(m_Actor, in location);
        if (!IsValidFleeingAction(bump)) return float.NaN;
#if DEBUG
        if (!bump.IsPerformable()) throw new InvalidOperationException("non-null non-performable bump");
#else
        if (!bump.IsPerformable()) return float.NaN;
#endif
        float num = SafetyFrom(location.Position, goals);
        if (null != leader) {
          num -= (float)Rules.StdDistance(in location, leader.Location);
        }
        return num;
      }, (a, b) => a > b);
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

#nullable enable
    protected ActionMeleeAttack? BehaviorMeleeAttack(Actor target)
    {
      return (m_Actor.CanMeleeAttack(target) ? new ActionMeleeAttack(m_Actor, target) : null);
    }

    /// <returns>null, or a non-free action</returns>
    protected ActorAction? BehaviorEquipWeapon()
    {
      // One of our callers is InsaneHumanAI::SelectAction.  As this AI is always insane, it does not trigger
      // random insane actions which could pick up ranged weapons.
      // Thus, no AI that calls this function has a usable firearm in inventory.

      var tmp_a = m_Actor;
      var equippedWeapon = tmp_a.GetEquippedWeapon();
      var bestMeleeWeapon = tmp_a.GetBestMeleeWeapon();   // rely on OrderableAI doing the right thing

      if (bestMeleeWeapon == null) {
        equippedWeapon?.UnequippedBy(tmp_a);    // unusable ranged weapon
        return null;
      }
      if (equippedWeapon != bestMeleeWeapon) bestMeleeWeapon.EquippedBy(tmp_a);
      return null;
    }
#nullable restore

    protected ActorAction BehaviorBuildTrap(RogueGame game)
    {
      var itemTrap = m_Actor.Inventory.GetFirst<ItemTrap>();
      if (itemTrap == null) return null;
      if (!IsGoodTrapSpot(m_Actor.Location.Map, m_Actor.Location.Position, out string reason)) return null;
      if (!itemTrap.IsActivated && !itemTrap.Model.ActivatesWhenDropped)
        return new ActionUseItem(m_Actor, itemTrap);
      game.DoEmote(m_Actor, string.Format("{0} {1}!", reason, itemTrap.AName), true);
      return new ActionDropItem(m_Actor, itemTrap);
    }

#nullable enable
    protected bool IsGoodTrapSpot(Map map, Point pos, out string reason)
    {
      reason = "";
      if (map.IsTrapCoveringMapObjectAt(pos)) return false; // if it won't trigger, waste of a trap.  Other two calls don't have a corresponding object re-fetch.
      bool isInside = map.IsInsideAt(pos);
      // not really ... z just heals up on the corpse
      if (!isInside && map.HasCorpsesAt(pos)) reason = "that corpse will serve as a bait for";
      // These all have the same problem: z will wander right over the trap no matter what when scent-tracking.
      // What is important is ability of livings to avoid the trap.
      // single spikes are ok as proporty markers, but the more damaging traps are all problematic.
      else if (m_prevLocation.Map.IsInsideAt(m_prevLocation.Position) != isInside) reason = "protecting the building with";
      else {
        if (map.GetMapObjectAt(pos) is DoorWindow) reason = "protecting the doorway with";
        else if (map.HasExitAt(in pos)) reason = "protecting the exit with";
      }
      if (string.IsNullOrEmpty(reason)) return false;
      var itemsAt = map.GetItemsAt(pos);
      if (null == itemsAt) return true;
      return 3 >= itemsAt.Items.Count(it => it is ItemTrap itemTrap && itemTrap.IsActivated);
    }

    protected ActorAction? BehaviorAttackBarricade()
    {
      Map map = m_Actor.Location.Map;
      Dictionary<Point,DoorWindow> doors = map.FindAdjacent(m_Actor.Location.Position, (m,pt) => {
        return (m.GetMapObjectAtExt(pt) is DoorWindow dw && dw.IsBarricaded) ? dw : null;
      });
      if (0 >= doors.Count) return null;
      DoorWindow doorWindow1 = Rules.Get.DiceRoller.Choose(doors).Value;
      return m_Actor.CanBreak(doorWindow1) ? new ActionBreak(m_Actor, doorWindow1) : null;
    }
#nullable restore

#if DEAD_FUNC
    // intentionally disabled in alpha 9; for ZM AI
    protected ActorAction BehaviorAssaultBreakables(HashSet<Point> fov)
    {
      Map map = m_Actor.Location.Map;
      List<Percept> percepts = null;
      foreach (Point position in fov) {
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && mapObjectAt.IsBreakable) {
          (percepts ?? (percepts = new List<Percept>())).Add(new Percept(mapObjectAt, map.LocalTime.TurnCounter, new Location(map, position)));
        }
      }
      if (percepts == null) return null;
      Percept percept = FilterNearest(percepts);
      if (!Rules.IsAdjacent(m_Actor.Location.Position, percept.Location.Position))
        return BehaviorIntelligentBumpToward(percept.Location.Position, true, true);
      return (m_Actor.CanBreak(percept.Percepted as MapObject) ? new ActionBreak(m_Actor, percept.Percepted as MapObject) : null);
    }
#endif

#nullable enable
    protected ActionPush? BehaviorPushNonWalkableObject()
    {
      if (!m_Actor.AbleToPush) return null;
      Map map = m_Actor.Location.Map;
      Dictionary<Point,MapObject> objs = map.FindAdjacent(m_Actor.Location.Position,(m,pt) => {
        var o = m.GetMapObjectAtExt(pt);
        if (null == o || o.IsWalkable) return null;
        return (m_Actor.CanPush(o) ? o : null);
      });
      if (0 >= objs.Count) return null;
      var rules = Rules.Get;
      var tmp = new ActionPush(m_Actor, rules.DiceRoller.Choose(objs).Value, rules.RollDirection());
      return (tmp.IsPerformable() ? tmp : null);
    }
#nullable restore

    protected ActionPush BehaviorPushNonWalkableObjectForFood()
    {
      if (!m_Actor.AbleToPush) return null;
      // Substantial re-implementation.  Historical one can make the police offices very hard to write a correct pathing for.
      // Some of these reality checks can be weakened for mentally disturbed livings
      var food_blockers = new Dictionary<Point,MapObject>();
      Map map = m_Actor.Location.Map;
      // do not worry about inside vs. outside here
      foreach(var pt in FOV) {
        var o = map.GetMapObjectAtExt(pt);
        if (null != o && !o.IsWalkable && !o.IsJumpable && m_Actor.CanPush(o)) food_blockers.Add(pt, o);
      }
      if (0 >= food_blockers.Count) return null;

      // 1) object on exit is bad (blocks access to basement stash).
      // 2) ### map geometry
      //    #..
      //    #..
      // 3) ### map geometry
      //    ...
      //    ...

      var verified = new Dictionary<Point,MapObject>();
      var unclear = new Dictionary<Point,MapObject>();
      foreach(var x in food_blockers) {
        var loc = new Location(map,x.Key);
        Map.Canonical(ref loc);
        if (null != loc.Exit) { // on same-district exit is clearly bad
          verified.Add(x.Key,x.Value);
          continue;
        }
        // flush against a wall, or in a corner, is fine
        if (loc.Map.IsFlushNWall(loc.Position)) continue;
        if (loc.Map.IsFlushSWall(loc.Position)) continue;
        if (loc.Map.IsFlushWWall(loc.Position)) continue;
        if (loc.Map.IsFlushEWall(loc.Position)) continue;
        if (loc.Map.IsNWCorner(loc.Position)) continue;
        if (loc.Map.IsNECorner(loc.Position)) continue;
        if (loc.Map.IsSWCorner(loc.Position)) continue;
        if (loc.Map.IsSECorner(loc.Position)) continue;
        unclear.Add(x.Key, x.Value);
      }
      if (0 >= verified.Count && 0 >=unclear.Count) return null;

      bool adjacent(Point pt) { return 1 == Rules.GridDistance(m_Actor.Location.Position, in pt); }

      // fail over to legacy code here
      // \todo also evaluate pulls (return type weakening)
      unclear.OnlyIf(adjacent);
      verified.OnlyIf(adjacent);
      if (0 >= verified.Count && 0 >=unclear.Count) return null;

      static bool safe_push(ActionPush act) {
        var loc = act.obj_dest;
        if (loc.Map.IsFlushNWall(loc.Position)) return true;
        if (loc.Map.IsFlushSWall(loc.Position)) return true;
        if (loc.Map.IsFlushWWall(loc.Position)) return true;
        if (loc.Map.IsFlushEWall(loc.Position)) return true;
        if (loc.Map.IsNWCorner(loc.Position)) return true;
        if (loc.Map.IsNECorner(loc.Position)) return true;
        if (loc.Map.IsSWCorner(loc.Position)) return true;
        if (loc.Map.IsSECorner(loc.Position)) return true;
        return false;
      }

      var pushes = new List<ActionPush>();
      foreach(var x in verified) {
        foreach(var dir in Direction.COMPASS) {
          var dest = x.Key+dir;
          if (map.IsInBounds(dest) && map.HasExitAt(in dest)) continue; // constructor crash; don't want to do this even when tactical pushing implemented
          var act = new ActionPush(m_Actor, x.Value, dir);
          if (act.IsPerformable()) pushes.Add(act);
        }
      }
      if (pushes.NontrivialFilter(safe_push)) pushes = pushes.FindAll(safe_push);
      if (0 < pushes.Count) return Rules.Get.DiceRoller.Choose(pushes);

      foreach(var x in unclear) {
        foreach(var dir in Direction.COMPASS) {
          var dest = x.Key+dir;
          if (map.IsInBounds(dest) && map.HasExitAt(in dest)) continue; // constructor crash; don't want to do this even when tactical pushing implemented
          var act = new ActionPush(m_Actor, x.Value, dir);
          if (act.IsPerformable()) pushes.Add(act);
        }
      }
      if (pushes.NontrivialFilter(safe_push)) pushes = pushes.FindAll(safe_push);
      if (0 < pushes.Count) return Rules.Get.DiceRoller.Choose(pushes);

      return null;
    }

#nullable enable
    protected virtual ActorAction? BehaviorFollowActor(Actor? other, int maxDist)
    {
      if (null == other || other.IsDead) return null;
      int num = Rules.GridDistance(m_Actor.Location, other.Location);
      if (CanSee(other.Location) && num <= maxDist) return new ActionWait(m_Actor);
      if (other.Location.Map != m_Actor.Location.Map) {
        var exitAt = m_Actor.Location.Exit;
        if (exitAt != null && exitAt.ToMap == other.Location.Map && m_Actor.CanUseExit())
          return BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      var actorAction = BehaviorIntelligentBumpToward(other.Location, false, false);
      if (actorAction == null || !actorAction.IsPerformable()) return null;
      if (other.IsRunning) m_Actor.Run();
      return actorAction;
    }

    protected ActorAction? BehaviorTrackScent(List<Percept_<AIScent>>? scents)
    {
      if (null == scents || 0 >= scents.Count) return null;
      var percept = scents.Maximize(scent => scent.Percepted.Strength);
      if (m_Actor.Location != percept.Location) {
        var tmpAction = BehaviorIntelligentBumpToward(percept.Location, false, false);
        if (null!=tmpAction) return tmpAction;
        var dir = Direction.FromVector(percept.Location.Position-m_Actor.Location.Position);
        // Cf. Angband.
        if (null!=dir) {
          if (Rules.Get.DiceRoller.RollChance(50)) { // anti-clockwise bias
            tmpAction = BehaviorIntelligentBumpToward(percept.Location+dir.Left, false, false);
            if (null!=tmpAction) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location+dir.Right, false, false);
            if (null!=tmpAction) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Left.Left, false, false);
            if (null!=tmpAction) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Right.Right, false, false);
            if (null!=tmpAction) return tmpAction;
          } else {  // clockwise bias
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Right, false, false);
            if (null!=tmpAction) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Left, false, false);
            if (null!=tmpAction) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Right.Right, false, false);
            if (null!=tmpAction) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Left.Left, false, false);
            if (null!=tmpAction) return tmpAction;
          }
        }
        return null;
      }
      if (m_Actor.Location.Map.HasExitAt(m_Actor.Location.Position) && m_Actor.Model.Abilities.AI_CanUseAIExits)
        return BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      return null;
    }
#nullable restore

    protected virtual ActorAction BehaviorChargeEnemy(Percept_<Actor> target, bool canCheckBreak, bool canCheckPush)
    {
      Actor actor = target.Percepted;
      ActorAction? tmpAction = BehaviorMeleeAttack(actor);
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location)) return new ActionWait(m_Actor);
        tmpAction = BehaviorHeadFor(target.Location, canCheckBreak, canCheckPush);
        if (null == tmpAction) return null;
        if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) m_Actor.Run();
        return tmpAction;
      } catch(System.Exception) {
        throw;
      } finally {
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = actor;
        }
      }
    }

    // Feral dogs use BehaviorFightOrFlee; simplified version of what OrderableAI uses
    protected ActorAction? BehaviorFightOrFlee(RogueGame game, string[] emotes, RouteFinder.SpecialActions allowedChargeActions)
    {
      const ActorCourage courage = ActorCourage.CAUTIOUS;
      var target = FilterNearest(_enemies);
      bool doRun = false;	// only matters when fleeing
      Actor enemy = target.Percepted;
      bool decideToFlee;
      if (enemy.HasEquipedRangedWeapon()) decideToFlee = false;
      else if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, enemy.Location))
        decideToFlee = true;
      else {
        decideToFlee = WantToEvadeMelee(m_Actor, courage, enemy);
        doRun = !HasSpeedAdvantage(m_Actor, enemy);
      }
      if (!decideToFlee && WillTireAfterAttack(m_Actor)) {
        decideToFlee = true;    // but do not run as otherwise we won't build up stamina
      }

      ActorAction? tmpAction = null;

      if (decideToFlee) {
        if (m_Actor.Model.Abilities.CanTalk && Rules.Get.RollChance(EMOTE_FLEE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[0], enemy.Name));
        // using map objects goes here
        // barricading goes here
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && (Lighting.DARKNESS== m_Actor.Location.Map.Lighting || Rules.Get.RollChance(FLEE_THROUGH_EXIT_CHANCE))) {
          tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.NONE);
          if (null != tmpAction) {
            bool flag3 = true;
            if (m_Actor.HasLeader) {
              var exitAt = m_Actor.Location.Exit;
              if (exitAt != null) flag3 = m_Actor.Leader.Location.Map == exitAt.ToMap;
            }
            if (flag3) {
              m_Actor.Activity = Activity.FLEEING;
              return tmpAction;
            }
          }
        }
        // XXX we should run for the exit here
        tmpAction = BehaviorWalkAwayFrom(_enemies.Select(p => p.Location.Position));
        if (null != tmpAction) {
          if (doRun) m_Actor.Run();
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (m_Actor.Model.Abilities.CanTalk && Rules.Get.RollChance(EMOTE_FLEE_TRAPPED_CHANCE))
            game.DoEmote(m_Actor, emotes[1], true);
          return BehaviorMeleeAttack(target.Percepted as Actor);
        }
        return null;
      } // if (decldeToFlee)

      // redo the pause check
      if (m_Actor.Speed > enemy.Speed) {
        int dist = Rules.GridDistance(m_Actor.Location,target.Location);
        if (2==dist) {
          if (!m_Actor.WillActAgainBefore(enemy)) return new ActionWait(m_Actor);
          // cannot close at normal speed safely; run-hit ok but requires situational analysis
          return new ActionWait(m_Actor);
        }
      }

      // charge
      tmpAction = BehaviorChargeEnemy(target, true, true);
      if (null != tmpAction) {
        if (m_Actor.Model.Abilities.CanTalk && Rules.Get.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[2], enemy.Name), true);
        return tmpAction;
      }
      return null;
    }

    // the intelligent callers are using the override at OrderableAI
    protected virtual ActorAction BehaviorExplore(ExplorationData exploration)
    {
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position - m_prevLocation.Position);
      var choiceEval = Choose(Direction.COMPASS, dir => {
        Location loc = m_Actor.Location + dir;
        var bump = Rules.IsBumpableFor(m_Actor, in loc);
        if (!IsValidMoveTowardGoalAction(bump)) return float.NaN;
        if (!Map.Canonical(ref loc)) return float.NaN;
#if DEBUG
        if (!bump.IsPerformable()) throw new InvalidOperationException("non-null non-performable bump");
#else
        if (!bump.IsPerformable()) return float.NaN;
#endif

        const int EXPLORE_ZONES = 1000;
        const int EXPLORE_LOCS = 500;
        const int EXPLORE_BARRICADES = 100;
        const int EXPLORE_INOUT = 50;
        const int EXPLORE_DIRECTION = 25;
        const int EXPLORE_RANDOM = 10;

//      if (exploration.HasExplored(loc)) return float.NaN;
        Map map = loc.Map;
        Point position = loc.Position;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position))) num += EXPLORE_ZONES;
        if (!exploration.HasExplored(in loc)) num += EXPLORE_LOCS;
        var mapObjectAt = map.GetMapObjectAt(position);
        // this is problematic when the door is the previous location.  Do not overwhelm in/out
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) {
          num += (loc != PrevLocation ? EXPLORE_BARRICADES : -EXPLORE_DIRECTION);
        }
        if (map.IsInsideAtExt(position)) {
          if (map.LocalTime.IsNight) num += EXPLORE_INOUT;
        }
        else if (!map.LocalTime.IsNight) num += EXPLORE_INOUT;
        if (dir == prevDirection) num += EXPLORE_DIRECTION;
        return (float) (num + Rules.Get.Roll(0, EXPLORE_RANDOM));
      }, (a, b) => a > b);
      if (choiceEval != null) return new ActionBump(m_Actor, choiceEval.Choice);
      return null;
    }

#nullable enable
    protected ActorAction? BehaviorUseExit(UseExitFlags useFlags)
    {
      var exitAt = m_Actor.Location.Exit;
      if (null == exitAt) return null;
      if ((useFlags & UseExitFlags.DONT_BACKTRACK) != UseExitFlags.NONE && exitAt.Location == m_prevLocation) return null;
      if (exitAt.IsNotBlocked(out var actorAt, out var mapObjectAt, m_Actor)) return (m_Actor.CanUseExit() ? new ActionUseExit(m_Actor, m_Actor.Location) : null);
      if ((useFlags & UseExitFlags.ATTACK_BLOCKING_ENEMIES) != UseExitFlags.NONE) {
        if (actorAt != null && m_Actor.IsEnemyOf(actorAt) && m_Actor.CanMeleeAttack(actorAt))
          return new ActionMeleeAttack(m_Actor, actorAt);
      }
      if ((useFlags & UseExitFlags.BREAK_BLOCKING_OBJECTS) != UseExitFlags.NONE) {
        if (mapObjectAt != null && m_Actor.CanBreak(mapObjectAt))
          return new ActionBreak(m_Actor, mapObjectAt);
      }
      return null;
    }

    protected ActorAction? BehaviorGoEatFoodOnGround(List<Percept>? stacksPercepts)
    {
      if (stacksPercepts == null) return null;
      var percepts = stacksPercepts.FilterT<Inventory>(inv => inv.Has<ItemFood>());
      if (percepts == null) return null;
      var firstByType = m_Actor.Location.Items?.GetFirst<ItemFood>();
      if (null != firstByType) return new ActionEatFoodOnGround(m_Actor, firstByType);
      var percept = FilterNearest(percepts);
      return BehaviorStupidBumpToward(percept.Location,false,false);
    }

    protected ActorAction? BehaviorGoEatCorpse(List<Percept>? percepts)
    {
	  if (   !Session.Get.HasCorpses
          || !m_Actor.CanEatCorpse()
          || (m_Actor.Model.Abilities.IsUndead && m_Actor.HitPoints >= m_Actor.MaxHPs))
        return null;
	  var corpsesPercepts = percepts?.FilterT<List<Corpse>>();
	  if (null == corpsesPercepts) return null;
      m_Actor.Activity = Activity.IDLE;
      var percept = FilterNearest(corpsesPercepts);
	  if (m_Actor.Location == percept.Location) return new ActionEatCorpse(m_Actor, (percept.Percepted as List<Corpse>)[0]);
      return BehaviorHeadFor(percept.Location,true,true);
    }
#nullable restore

    /// <summary>
    /// Compute safety from a list of dangers at a given position.
    /// </summary>
    /// <param name="from">position to compute the safety</param>
    /// <param name="dangers">dangers to avoid</param>
    /// <returns>a heuristic value, the higher the better the safety from the dangers</returns>
    protected float SafetyFrom(Point from, IEnumerable<Point> dangers)
    {
      Map map = m_Actor.Location.Map;

      // Heuristics:
      // Primary: Get away from dangers.
      // Weighting factors:
      // 1 Avoid getting in corners.
      // 2 Prefer going outside/inside if majority of dangers are inside/outside.
      // 3 If can tire, prefer not jumping.
#region Primary: Get away from dangers.
      float avgDistance = (float) (dangers.Sum(pt => Rules.GridDistance(in from, in pt))) / (1 + dangers.Count());
#endregion
#region 1 Avoid getting in corners.
      int countFreeSquares = map.CountAdjacentTo(from,pt => pt == m_Actor.Location.Position || map.IsWalkableFor(pt, m_Actor));
      float avoidCornerBonus = countFreeSquares * 0.1f;
#endregion
#region 2 Prefer going outside/inside if majority of dangers are inside/outside.
      bool isFromInside = map.IsInsideAtExt(from);
      int majorityDangersInside = dangers.ExcessTrue(pt => map.IsInsideAtExt(pt));
      const float inOutFactor = 1.25f;
      float inOutBonus = 0.0f;
      if (isFromInside) {
        if (majorityDangersInside < 0) inOutBonus = inOutFactor;
      }
      else if (majorityDangersInside > 0) inOutBonus = inOutFactor;
#endregion
#region 3 If can tire, prefer not jumping.
      float jumpPenalty = 0.0f;
      if (m_Actor.Model.Abilities.CanTire && m_Actor.Model.Abilities.CanJump) {
        var mapObjectAt = map.GetMapObjectAtExt(from);
        if (mapObjectAt != null && mapObjectAt.IsJumpable) jumpPenalty = 0.1f;
      }
#endregion
      float heuristicFactorBonus = 1f + avoidCornerBonus + inOutBonus - jumpPenalty;
      return avgDistance * heuristicFactorBonus;
    }

    // as above, but location-based
    protected float SafetyFrom(in Location from, IEnumerable<Location> dangers)
    {
      Map map = m_Actor.Location.Map;

      // Heuristics:
      // Primary: Get away from dangers.
      // Weighting factors:
      // 1 Avoid getting in corners.
      // 2 Prefer going outside/inside if majority of dangers are inside/outside.
      // 3 If can tire, prefer not jumping.
#region Primary: Get away from dangers.
      double avgDistance = 0.0;
      int count_plus_1 = 1;
      foreach(var pt in dangers) {
        count_plus_1++;
        avgDistance += Rules.InteractionDistance(in from, in pt);
      }
      avgDistance /= count_plus_1;
#endregion
#region 1 Avoid getting in corners.
      int countFreeSquares = map.CountAdjacentTo(from.Position,pt => pt == m_Actor.Location.Position || map.IsWalkableFor(pt, m_Actor));
      float avoidCornerBonus = countFreeSquares * 0.1f;
#endregion
#region 2 Prefer going outside/inside if majority of dangers are inside/outside.
      bool isFromInside = from.Map.IsInsideAtExt(from.Position);
      int majorityDangersInside = dangers.ExcessTrue(loc => loc.Map.IsInsideAtExt(loc.Position));
      const float inOutFactor = 1.25f;
      float inOutBonus = 0.0f;
      if (isFromInside) {
        if (majorityDangersInside < 0) inOutBonus = inOutFactor;
      }
      else if (majorityDangersInside > 0) inOutBonus = inOutFactor;
#endregion
#region 3 If can tire, prefer not jumping.
      float jumpPenalty = 0.0f;
      if (m_Actor.Model.Abilities.CanTire && m_Actor.Model.Abilities.CanJump) {
        if (Location.RequiresJump(in from)) jumpPenalty = 0.1f;
      }
#endregion
      float heuristicFactorBonus = 1f + avoidCornerBonus + inOutBonus - jumpPenalty;
      return (float)(avgDistance * heuristicFactorBonus);
    }

#nullable enable
    // isBetterThanEvalFn will never see NaN
    static protected ChoiceEval<_T_> Choose<_T_>(IEnumerable<_T_>? listOfChoices, Func<_T_, bool> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (null == listOfChoices || !listOfChoices.Any()) return null;

      Dictionary<float, List<ChoiceEval<_T_>>> choiceEvalDict = new Dictionary<float, List<ChoiceEval<_T_>>>();

      float num = float.NaN;
      foreach(_T_ tmp in listOfChoices) {
        if (!isChoiceValidFn(tmp)) continue;
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;
        if (float.IsNaN(num) || isBetterEvalThanFn(f, num)) {
          num = f;
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;

        ChoiceEval< _T_ > tmp2 = new ChoiceEval<_T_>(tmp, f);
        if (choiceEvalDict.TryGetValue(f,out var dest)) dest.Add(tmp2);
        else choiceEvalDict.Add(f, new List<ChoiceEval<_T_>>{ tmp2 });
      }

      if (!choiceEvalDict.TryGetValue(num, out List<ChoiceEval<_T_>> ret_from)) return null;
      if (1 == ret_from.Count) return ret_from[0];
      return Rules.Get.DiceRoller.Choose(ret_from);
    }

    static protected ChoiceEval<_T_>? Choose<_T_>(IEnumerable<_T_>? listOfChoices, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (null == listOfChoices || !listOfChoices.Any()) return null;

      float num = float.NaN;
      List<ChoiceEval<_T_>> candidates = new List<ChoiceEval<_T_>>();
      foreach(_T_ tmp in listOfChoices) {
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;   // NaN is not valid
        if (float.IsNaN(num) || isBetterEvalThanFn(f, num)) {
          num = f;
          candidates.Clear();
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;
        candidates.Add(new ChoiceEval<_T_>(tmp, f));
      }
      if (0 >= candidates.Count) return null;
      return Rules.Get.DiceRoller.Choose(candidates);
    }

    // isBetterThanEvalFn will never see NaN
    static protected ChoiceEval<_DATA_>? ChooseExtended<_T_, _DATA_>(IEnumerable<_T_>? listOfChoices, Func<_T_, _DATA_> isChoiceValidFn, Func<_T_, _DATA_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (null == listOfChoices || !listOfChoices.Any()) return null;

      Dictionary<float, List<ChoiceEval<_DATA_>>> choiceEvalDict = new Dictionary<float, List<ChoiceEval<_DATA_>>>();

      float num = float.NaN;
      foreach(_T_ tmp in listOfChoices) {
        _DATA_ choice = isChoiceValidFn(tmp);
        if (null == choice) continue;
        float f = evalChoiceFn(tmp, choice);
        if (float.IsNaN(f)) continue;
        if (float.IsNaN(num) || isBetterEvalThanFn(f, num)) {
          num = f;
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;

        ChoiceEval< _DATA_ > tmp2 = new ChoiceEval<_DATA_>(choice, f);
        if (choiceEvalDict.TryGetValue(f,out var dest))  dest.Add(tmp2);
        else choiceEvalDict.Add(f, new List<ChoiceEval<_DATA_>>{ tmp2 });
      }

      if (!choiceEvalDict.TryGetValue(num, out List<ChoiceEval<_DATA_>> ret_from)) return null;
      return Rules.Get.DiceRoller.Choose(ret_from);
    }

    protected bool IsValidFleeingAction(ActorAction? a)
    {
      if (null == a) return false;
      if (a is Resolvable res) return IsValidFleeingAction(res.ConcreteAction);
      return a is ActionMoveStep || a is ActionOpenDoor || a is ActionSwitchPlace;
    }
#nullable restore

    protected bool IsValidWanderAction(ActorAction a)
    {
      if (null == a) return false;
      if (a is ActionMoveStep) return true;
      if (a is ActionSwitchPlace) return true;
//    if (a is ActionPush) return true; // wasn't being generated in RS alpha 9...results do not look good
      if (a is ActionOpenDoor) return true;
      if (a is ActionBashDoor) return true;
      if (a is ActionBarricadeDoor) return true;
/*
      // locked down to PlayerController so Jason Myers/CHAR guards no longer an issue
      if (a is ActionGetFromContainer get_from) {    // XXX Jason Myers: not OrderableAI but capable of this
        return (m_Actor.Controller as ObjectiveAI)?.IsInterestingItem(get_from.Item) ?? true;
      }
*/
      if (this is OrderableAI ordai) {
        if (a is ActionChat chat) {
          return ordai.Directives.CanTrade || chat.Target == m_Actor.Leader;
        }
      }
      return false;
    }

#nullable enable
    protected bool IsValidMoveTowardGoalAction(ActorAction? a)
    {
      if (null == a) return false;
      if (a is Resolvable res) return IsValidMoveTowardGoalAction(res.ConcreteAction);
      // blacklist actions inappropriate for bump movement
      if (a is ActionChat) return false;
      if (a is ActionGetFromContainer) return false;
      if (a is ActionRechargeItemBattery) return false;
      if (a is ActionSwitchPlaceEmergency) return false;
      if (a is ActionSwitchPowerGenerator) return false;
      return true;
    }
#nullable restore

    protected bool IsOccupiedByOther(Location loc)  // percept locations are normalized
    {
      var actorAt = loc.Actor;
      return null != actorAt && actorAt != m_Actor;
    }

#nullable enable
    protected bool WantToEvadeMelee(Actor actor, ActorCourage courage, Actor target)
    {
//    if (WillTireAfterAttack(actor)) return true;  // post-process this, handling this here is awful for rats
      if (actor.Speed > target.Speed) {
        if (actor.WillActAgainBefore(target)) return false; // caller must handle distance 2 correctly.
        if (Rules.IsAdjacent(actor.Location,target.Location) && Actor.STAMINA_MIN_FOR_ACTIVITY<=target.StaminaPoints) return true;  // back-and-smack indicated
//      if (target.TargetActor == actor) return true;
        return false;
      }
      var weakerInMelee = FindWeakerInMelee(m_Actor, target);
      return weakerInMelee != target && (weakerInMelee == m_Actor || courage != ActorCourage.COURAGEOUS);
    }

    private static Actor? FindWeakerInMelee(Actor a, Actor b)
    {
	  int a_dam = a.MeleeAttack(b).DamageValue - b.CurrentDefence.Protection_Hit;
	  int b_dam = b.MeleeAttack(a).DamageValue - a.CurrentDefence.Protection_Hit;
      if (Actor.STAMINA_MIN_FOR_ACTIVITY > a.StaminaPoints && Actor.STAMINA_MIN_FOR_ACTIVITY <= b.StaminaPoints) a_dam = 0;
      if (Actor.STAMINA_MIN_FOR_ACTIVITY > b.StaminaPoints && Actor.STAMINA_MIN_FOR_ACTIVITY <= a.StaminaPoints) b_dam = 0;
	  if (0 >= a_dam) return a;
	  if (0 >= b_dam) return b;
      int speed_factor = a.HowManyTimesOtherActs(1, b);
      if (1 < speed_factor) b_dam *= a.HowManyTimesOtherActs(1, b);
      else if (1 > speed_factor) a_dam *= 2;    // usually double-move
	  if (a_dam/2 >= b.HitPoints) return b;	// one-shot if hit
	  if (b_dam >= a.HitPoints) return a; // could die if hit
	  int a_kill_b_in = ((8*b.HitPoints)/(5*a_dam));	// assume bad luck when attacking
	  int b_kill_a_in = ((8*a.HitPoints)/(7*b_dam));	// assume bad luck when defending
	  if (a_kill_b_in < b_kill_a_in) return b;
	  if (a_kill_b_in > b_kill_a_in) return a;
//	  int num1 = a.HitPoints + a.MeleeAttack(b).DamageValue;
//      int num2 = b.HitPoints + b.MeleeAttack(a).DamageValue;
//      if (num1 < num2) return a;
//      if (num1 > num2) return b;
      return null;
    }
#nullable restore

    protected static bool WillTireAfterAttack(Actor actor)
    {
      return actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK+ actor.CurrentMeleeAttack.StaminaPenalty);
    }

    // XXX doesn't work in the presence of jumping
    protected static bool WillTireAfterRunning(Actor actor)
    {
      return actor.WillTireAfter(Rules.STAMINA_COST_RUNNING);
    }

    static protected bool HasSpeedAdvantage(Actor actor, Actor target)
    {
      int num1 = actor.Speed;
      int num2 = target.Speed;
      return (num1 > num2) || (actor.CanRun() && !target.CanRun() && !WillTireAfterRunning(actor) && num1 * 2 > num2);
    }

    protected static bool IsBetween(Point A, in Point between, Point B)
    {
      return Rules.StdDistance(in A, in between) + Rules.StdDistance(in B, in between) <= Rules.StdDistance(in A, in B) + 0.25;
    }

    protected bool IsFriendOf(Actor other) { return !m_Actor.IsEnemyOf(other) && m_Actor.IsFaction(other); }

    // XXX these two break down cross-map
    protected bool CanReachSimple(in Location dest, RouteFinder.SpecialActions allowedActions)
    {
       (m_RouteFinder ??= new RouteFinder(this)).AllowedActions = allowedActions;
       return m_RouteFinder.CanReachSimple(RogueForm.Game, in dest, Rules.GridDistance(m_Actor.Location, in dest), Rules.GridDistanceFn);
    }

    protected void FilterOutUnreachablePercepts<_T_>(ref List<_T_> percepts, RouteFinder.SpecialActions allowedActions) where _T_:WhereWhen
    {
      percepts?.OnlyIf(p => CanReachSimple(p.Location, allowedActions));
    }

    protected static Location RandomPositionNear(Rules rules, Location goal, int range)
    { // XXX \todo see if it's practical to be more efficient
      Rectangle _range = new Rectangle((Point)(-range),(Point)(2*range+1));
      Map m = goal.Map;
      Point pt;
      do {
        pt = goal.Position + rules.DiceRoller.Choose(_range);
      } while(!m.IsValid(pt));
      return new Location(m, pt);
    }

    protected class ChoiceEval<_T_>
    {
      public readonly _T_ Choice;
      public readonly float Value;

      public ChoiceEval(_T_ choice, float value)
      {
        Choice = choice;
        Value = value;
      }

      public override string ToString()
      {
        return string.Format("ChoiceEval({0}; {1:F})", null == Choice ? "NULL" : Choice.ToString(), Value);
      }
    }

    [System.Flags]
    protected enum UseExitFlags
    {
      NONE = 0,
      BREAK_BLOCKING_OBJECTS = 1,
      ATTACK_BLOCKING_ENEMIES = 2,
      DONT_BACKTRACK = 4,
    }
  }

  internal static class ext_BaseAI
  {
    public static Func<Point, Point, float> Postprocess(this Func<Point, Point, float> src, Func<Point, Point, float, float> post)
    {
      if (null == post) {
        Func<Point, Point, float> l = src;  // local copies needed to get true lambda calculus
        return l;
      }
      float ret(Point ptA,Point ptB) {
        float x = src(ptA,ptB);
        if (!float.IsNaN(x)) x = post(ptA,ptB,x);
        return x;
      }
      return ret;
    }
  }
}
