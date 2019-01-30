// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.BaseAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define XDISTRICT_PATHING

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
using System.Drawing;
using System.Linq;
using Zaimoni.Data;

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

    protected BaseAI()
    {
    }

    public Location PrevLocation { get { return m_prevLocation; } }
    public void UpdatePrevLocation() { m_prevLocation = m_Actor.Location; } // for PlayerController

    public override ActorAction GetAction(RogueGame game)
    {
      if (m_prevLocation.Map == null) m_prevLocation = m_Actor.Location;
      m_Actor.TargetActor = null;
      ActorAction actorAction = SelectAction(game);
#if DEBUG
      if (!(actorAction?.IsLegal() ?? true)) throw new InvalidOperationException("illegal action returned from SelectAction");
#endif
      if ((this is ObjectiveAI ai)) {
        if (ai.VetoAction(actorAction)) actorAction = new ActionWait(m_Actor);
#if PROTOTYPE
        ActorAction alt = ai.RewriteAction(actorAction);
        if (alt?.IsLegal() ?? false) actorAction = alt;
#endif
        ai.ResetAICache();
      }
      if (!(actorAction is ActionCloseDoor)) m_prevLocation = m_Actor.Location;
      return actorAction ?? new ActionWait(m_Actor);    // likely redundant
    }

    protected abstract ActorAction SelectAction(RogueGame game);

/*
    NOTE: List<Percept>, as a list data structure, takes O(n) time/RAM to reset its capacity down
    to its real size.  Since C# is a garbage collected language, this would actually worsen
    the RAM loading until the next explicit GC.Collect() call (typically within a fraction of a second).
    The only realistic mitigation is to pro-rate the capacity request.
 */
    // April 22, 2016: testing indicates this does not need micro-optimization
    protected List<Percept_<_T_>> FilterSameMap<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      Map map = m_Actor.Location.Map;
#if XDISTRICT_PATHING
      return percepts.Filter(p => null != map.Denormalize(p.Location));
#else
      return percepts.Filter(p => p.Location.Map == map);
#endif
    }

#if DEAD_FUNC
    // actually deploying this is time-intensive
    protected static List<Percept_<_dest_>> FilterTo<_dest_,_src_>(List<Percept_<_src_>> percepts) where _dest_: class,_src_ where _src_:class
    {
      List<Percept_<_dest_>> tmp = new List<Percept_<_dest_>>();
      foreach(Percept_<_src_> p in percepts) {
        _dest_ tmp2 = p.Percepted as _dest_;
        if (null == tmp2) continue;
        tmp.Add(new Percept_<_dest_>(tmp2,p.Turn,p.Location));
      }
      return (0<tmp.Count ? tmp : null);
    }
#endif

    protected List<Percept> FilterEnemies(List<Percept> percepts)
    {
      return percepts.FilterT<Actor>(target => target!=m_Actor && m_Actor.IsEnemyOf(target));
    }

    protected List<Percept> FilterNonEnemies(List<Percept> percepts)
    {
      return percepts.FilterT<Actor>(target => target!=m_Actor && !m_Actor.IsEnemyOf(target));
    }

    protected List<Percept_<_T_>> FilterCurrent<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      return percepts.Filter(p => p.Turn == turnCounter);
    }

    protected List<Percept_<_T_>> FilterOld<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      return percepts.Filter(p => p.Turn < turnCounter);
    }

    // GangAI's mugging target selection triggered a race condition
    // that allowed a non-null non-empty percepts
    // to be seen as returning null from FilterNearest anyway, from
    // the outside (Contracts saw a non-null return)
    protected Percept_<_T_> FilterNearest<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      if (null == percepts || 0 == percepts.Count) return null;
      return percepts.Minimize(p=>Rules.StdDistance(m_Actor.Location, p.Location));
    }

    static private Percept_<AIScent> FilterStrongestScent(List<Percept_<AIScent>> scents)
    {
#if DEBUG
      if (0 >= (scents?.Count ?? 0)) throw new ArgumentNullException(nameof(scents));
#endif
      Percept_<AIScent> percept = null;
      foreach (var scent in scents) {
        // minimum valid scent strength is 1
        if (scent.Percepted.Strength > (percept?.Percepted.Strength ?? 0)) percept = scent;
      }
      return percept;
    }

#if DEAD_FUNC
    protected List<Percept> FilterFireTargets(List<Percept> percepts)
    {
      return percepts.FilterT<Actor>(target => m_Actor.CanFireAt(target));
    }
#endif

    protected List<Percept> FilterFireTargets(List<Percept> percepts, int range)
    {
      return percepts.FilterT<Actor>(target => m_Actor.CanFireAt(target,range));
    }

    protected List<Percept> FilterPossibleFireTargets(List<Percept> percepts)
    {
      return percepts.FilterT<Actor>(target => m_Actor.CouldFireAt(target));
    }

    protected List<Percept> FilterContrafactualFireTargets(List<Percept> percepts, Point p)
    {
      return percepts.FilterT<Actor>(target => m_Actor.CanContrafactualFireAt(target,p));
    }

#if DEAD_FUNC
    protected List<Percept_<_T_>> SortByDistance<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      if (null == percepts || 0 == percepts.Count) return null;
      Point from = m_Actor.Location.Position;
      List<Percept_<_T_>> perceptList = new List<Percept_<_T_>>(percepts);
      perceptList.Sort(((pA, pB) =>
      {
        double num1 = Rules.StdDistance(pA.Location.Position, from);
        double num2 = Rules.StdDistance(pB.Location.Position, from);
        return num1.CompareTo(num2);
      }));
      return perceptList;
    }
#endif

    // firearms use grid i.e. L-infinity distance
    protected List<Percept_<_T_>> SortByGridDistance<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      if (0 >= (percepts?.Count ?? 0)) return null;
      if (1==percepts.Count) return percepts;

      List<Percept_<_T_>> perceptList = new List<Percept_<_T_>>(percepts);
      Location from = m_Actor.Location;
      Dictionary<Percept_<_T_>, int> dict = new Dictionary<Percept_<_T_>, int>(perceptList.Count);
      foreach(Percept_<_T_> p in perceptList) {
        dict.Add(p,Rules.GridDistance(p.Location, from));
      }
      perceptList.Sort((pA, pB) => dict[pA].CompareTo(dict[pB]));
      return perceptList;
    }

#if DEAD_FUNC
    protected List<Percept> SortByDate(List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = new List<Percept>(percepts);
      perceptList.Sort((pA, pB) => pB.Turn.CompareTo(pA.Turn));
      return perceptList;
    }
#endif

    // policy change for behaviors: unless the action from a behavior is being used to decide whether to commit to the behavior,
    // a behavior should handle all free actions itself and return only non-free actions.

    /// <param name="exploration">can be null for ais with no exploration</param>
    protected ActorAction BehaviorWander(ExplorationData exploration=null, Predicate<Location> goodWanderLocFn=null)
    {
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location next = m_Actor.Location + dir;
        if (null != goodWanderLocFn && !goodWanderLocFn(next)) return float.NaN;
        if (!IsValidWanderAction(Rules.IsBumpableFor(m_Actor, next))) return float.NaN;
        if (!next.Map.IsInBounds(next.Position)) {
          Location? test = next.Map.Normalize(next.Position);
          if (null == test) return float.NaN;
          next = test.Value;
        }
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
        if (m_Actor.Model.Abilities.IsIntelligent && 0 < next.Map.TrapsMaxDamageAtFor(next.Position,m_Actor)) score += AVOID_TRAPS;

        // alpha10.1 prefer unexplored/oldest
        // unexplored should not happen because exploration rule is tested before wander rule but just to be more robust...
        if (exploration != null) {
          int locAge = exploration.GetExploredAge(next);
          score += (0 == locAge) ? UNEXPLORED_LOC : locAge;
        }

        // alpha10.1 prefer wandering to doorwindows and exits. 
        // helps civs ai getting stuck in semi-infinite loop when running out of new exploration to do.
        // as a side effect, make ais with no exploration data (eg zombies) more eager to visit door/windows and exits.
        if (next.MapObject is DoorWindow) score += DOORWINDOWS;
        if (null != next.Exit) score += (next.Exit.Location==m_prevLocation ? BACKTRACKING : EXITS);    // Staying Alive: match fix to BehaviorExplore

        // alpha10.1 prefer inside when almost sleepy
        if (m_Actor.IsAlmostSleepy && next.Map.IsInsideAt(next.Position)) score += INSIDE_WHEN_ALMOST_SLEEPY;

        // alpha10.1 add random factor
        score += RogueForm.Game.Rules.Roll(0, WANDER_RANDOM);

        return (float) score;
      }, (a, b) => a > b);
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
        protected float EstimateBumpActionCost(Location loc, ActorAction action)
        {
            float cost = 0;

            if (action is ActionBump) action = (action as ActionBump).ConcreteAction;

            // Consuming additional sta
            if (m_Actor.Model.Abilities.CanTire) {
                // jumping
                if (action is ActionMoveStep) {
                    MapObject mobj = loc.Map.GetMapObjectAt(loc.Position);
                    if (mobj?.IsJumpable ?? false) cost = MOVE_DISTANCE_PENALTY;
                }

                // actions that always consume sta or may take more than one turn
                if (action is ActionBashDoor || action is ActionBreak || action is ActionPush) cost = MOVE_DISTANCE_PENALTY;
            }

            return cost;
        }

    protected ActorAction BehaviorBumpToward(Point goal, bool canCheckBreak, bool canCheckPush, Func<Point, Point, float> distanceFn)
    {
#if DEBUG
      if (null == distanceFn) throw new ArgumentNullException(nameof(distanceFn));
#endif
      ChoiceEval<ActorAction> choiceEval = ChooseExtended(Direction.COMPASS, dir => {
        Location next = m_Actor.Location + dir;
        ActorAction a = Rules.IsBumpableFor(m_Actor, next);
        if (a == null) {
          if (m_Actor.Model.Abilities.IsUndead && m_Actor.AbleToPush) {
            MapObject mapObjectAt = next.MapObject;
            if (mapObjectAt != null && m_Actor.CanPush(mapObjectAt)) {
              Direction pushDir = RogueForm.Game.Rules.RollDirection();
              if (mapObjectAt.CanPushTo(mapObjectAt.Location.Position + pushDir))
                return new ActionPush(m_Actor, mapObjectAt, pushDir);
            }
          }

          // alpha10 check special actions
          if (canCheckBreak) {
            MapObject obj = m_Actor.Location.Map.GetMapObjectAt(next.Position);
            if (obj != null) {
              if (m_Actor.CanBreak(obj)) return new ActionBreak(m_Actor, obj);
            }
          }
          if (canCheckPush) {
            MapObject obj = m_Actor.Location.Map.GetMapObjectAt(next.Position);
            if (obj != null) {
              if (m_Actor.CanPush(obj)) {
                // push in a valid direction at random
                List<Direction> validPushes = new List<Direction>(8);
                foreach (Direction pushDir in Direction.COMPASS) {
                  if (obj.CanPushTo(obj.Location.Position + pushDir)) validPushes.Add(pushDir);
                }
                if (validPushes.Count > 0) return new ActionPush(m_Actor, obj, RogueForm.Game.Rules.DiceRoller.Choose(validPushes));
              }
            }
          }

          return null;
        }
        if (next.Position == goal || IsValidMoveTowardGoalAction(a)) return a;
        return null;
      }, (dir, action) => {
        Location next = m_Actor.Location + dir;
        float cost = distanceFn(next.Position, goal);

        // alpha10 add action cost heuristic if npc is intelligent
        if (!float.IsNaN(cost) && m_Actor.Model.Abilities.IsIntelligent) cost += EstimateBumpActionCost(next, action);

        return cost;
      }, (a, b) => a < b);
      if (choiceEval != null) return choiceEval.Choice;
      return null;
    }

    protected ActorAction BehaviorStupidBumpToward(Point goal, bool canCheckBreak, bool canCheckPush)
    {
      return BehaviorBumpToward(goal, canCheckBreak, canCheckPush, (ptA, ptB) => {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(ptA, ptB);
        if (!m_Actor.Location.Map.IsWalkableFor(ptA, m_Actor)) num += MOVE_DISTANCE_PENALTY;
        return num;
      });
    }

    protected ActorAction BehaviorStupidBumpToward(Location goal, bool canCheckBreak, bool canCheckPush)
    {
      if (m_Actor.Location.Map == goal.Map) return BehaviorStupidBumpToward(goal.Position, canCheckBreak, canCheckPush);
      Location? test = m_Actor.Location.Map.Denormalize(goal);
      if (null == test) return null;
      return BehaviorStupidBumpToward(test.Value.Position, canCheckBreak, canCheckPush);
    }

    protected ActorAction BehaviorIntelligentBumpToward(Point goal, bool canCheckBreak, bool canCheckPush)
    {
      float currentDistance = (float)Rules.StdDistance(m_Actor.Location.Position, goal);
      ActorCourage courage = (this as OrderableAI)?.Directives.Courage ?? ActorCourage.CAUTIOUS;
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
      return BehaviorBumpToward(goal, canCheckBreak, canCheckPush, (ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(ptA, ptB);
        if ((double) num >= (double) currentDistance) return float.NaN;
        if (!imStarvingOrCourageous) {
          int trapsMaxDamage = m_Actor.Location.Map.TrapsMaxDamageAtFor(ptA,m_Actor);
          if (trapsMaxDamage > 0) {
            if (trapsMaxDamage >= m_Actor.HitPoints) return float.NaN;
            num += MOVE_INTO_TRAPS_PENALTY;
          }
        }
        return num;
      });
    }

    protected ActorAction BehaviorIntelligentBumpToward(Location goal, bool canCheckBreak, bool canCheckPush)
    {
      if (m_Actor.Location.Map == goal.Map) return BehaviorIntelligentBumpToward(goal.Position, canCheckBreak, canCheckPush);
      Location? test = m_Actor.Location.Map.Denormalize(goal);
      if (null == test) return null;
      return BehaviorIntelligentBumpToward(test.Value.Position, canCheckBreak, canCheckPush);
    }

    protected ActorAction BehaviorHeadFor(Location goal, bool canCheckBreak, bool canCheckPush)
    {
      if (m_Actor.Model.Abilities.IsIntelligent) return BehaviorIntelligentBumpToward(goal, canCheckBreak, canCheckPush);
      return BehaviorStupidBumpToward(goal, canCheckBreak, canCheckPush);
    }

    protected ActorAction BehaviorHeadFor(IEnumerable<Location> goals, bool canCheckBreak, bool canCheckPush)
    {
      if (!goals?.Any() ?? true) return null;
      int dist = int.MaxValue;
      ActorAction ret = null;
      foreach(Location goal in goals) {
        int new_dist = Rules.GridDistance(m_Actor.Location, goal);
        if (dist <= new_dist) continue;
        ActorAction tmp = BehaviorHeadFor(goal, canCheckBreak, canCheckPush);
        if (null == tmp) continue;
        dist = new_dist;
        ret = tmp;
      }
      return ret;
    }

    // A number of the melee enemy targeting sequences not only work on grid distance,
    // they need to return a coordinated action/target pair.
    // We assume the list is sorted in increasing order of grid distance
    protected ActorAction TargetGridMelee(List<Percept> perceptList)
    {
      if (null == perceptList) return null; // inefficient, but reduces lines of code elsewhere
      foreach (Percept percept in perceptList) {
        ActorAction tmp = BehaviorStupidBumpToward(percept.Location, true, true);
        if (null != tmp) {
          m_Actor.Activity = Activity.CHASING;
          m_Actor.TargetActor = percept.Percepted as Actor;
          return tmp;
        }
      }
      return null;
    }

    protected ActorAction BehaviorWalkAwayFrom(IEnumerable<Point> goals)
    {
      Actor leader = m_Actor.LiveLeader;
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location location = m_Actor.Location + dir;
        if (!IsValidFleeingAction(Rules.IsBumpableFor(m_Actor, location))) return float.NaN;
        float num = SafetyFrom(location.Position, goals);
        if (null != leader) {
          num -= (float)Rules.StdDistance(location, leader.Location);
        }
        return num;
      }, (a, b) => a > b);
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    protected ActionMeleeAttack BehaviorMeleeAttack(Actor target)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      return (m_Actor.CanMeleeAttack(target) ? new ActionMeleeAttack(m_Actor, target) : null);
    }

    protected ActionRangedAttack BehaviorRangedAttack(Actor target)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      if (!m_Actor.CanFireAt(target)) return null;

      // alpha10
      // select rapid fire if one shot is not enough to kill target, has more than one ammo loaded and chances to hit good enough.
      FireMode fireMode = FireMode.DEFAULT;
      if ((m_Actor.GetEquippedWeapon() as ItemRangedWeapon).Ammo >= 2) {
        Attack rangedAttack = m_Actor.RangedAttack(Rules.GridDistance(m_Actor.Location, target.Location), target);
        if (rangedAttack.DamageValue < target.HitPoints) {
          int rapidHit1Chance = m_Actor.ComputeChancesRangedHit(target, 1);
          int rapidHit2Chance = m_Actor.ComputeChancesRangedHit(target, 2);
          // "good chances" = both hits at least 50%
          // typically the second shot has worse chances to hit (recoil) but a true burst fire weapon would reverse this;
          // it is possible to correct the targeting ellipse that fast even at Angband space-time scale.
          // after configuration merge:
          // * no true burst fire weapons, not even the army rifle
          // * getting true burst fire may require a minimum level of firearms skill, much like martial arts weapons don't work right without martial arts skill
          // * shotguns appear artificially inaccurate (but considering that CHAR guards have them, that may be a case of balance over realism)  High recoil, but also very wide fire cone
          // * not clear why Kolt so much more inaccurate than pistol
          // * not clear why Hanz Von Hanz has steeper drop-off than normal light pistol
          // \todo when the army rifle is configured as a true burst fire weapon, ensure that the army sniper rifle gets 3x the shots from a clip.  Clip size 60(!), but reloading army rifle is 10 shots for 30 ammo.
          // \todo new burst fire weapon: machine pistol (uses light pistol ammo).  Uses 3 ammo at once (handwave last burst), so only gets 7 shots from a light pistol clip (but loads the entire clip!)
          // somewhat exotic (may only be available from survivalist caches as contraband, or possibly an unusual SWAT police weapon)
          if (rapidHit1Chance >= 50 && rapidHit2Chance >= 50) fireMode = FireMode.RAPID;
        }
      }

       // fire!
       m_Actor.Activity = Activity.FIGHTING;
       m_Actor.TargetActor = target;
       return new ActionRangedAttack(m_Actor, target, fireMode);
    }

    /// <returns>null, or a non-free action</returns>
    protected ActorAction BehaviorEquipWeapon(RogueGame game)
    {
      // One of our callers is InsaneHumanAI::SelectAction.  As this AI is always insane, it does not trigger
      // random insane actions which could pick up ranged weapons.
      // Thus, no AI that calls this function has a usable firearm in inventory.

      Item equippedWeapon = m_Actor.GetEquippedWeapon();
      ItemMeleeWeapon bestMeleeWeapon = m_Actor.GetBestMeleeWeapon();   // rely on OrderableAI doing the right thing

      if (bestMeleeWeapon == null) {
        if (null != equippedWeapon) game.DoUnequipItem(m_Actor, equippedWeapon);    // unusable ranged weapon
        return null;
      }
      if (equippedWeapon == bestMeleeWeapon) return null;
      game.DoEquipItem(m_Actor, bestMeleeWeapon);
      return null;
    }

    protected ActorAction BehaviorBuildTrap(RogueGame game)
    {
      ItemTrap itemTrap = m_Actor.Inventory.GetFirst<ItemTrap>();
      if (itemTrap == null) return null;
      if (!IsGoodTrapSpot(m_Actor.Location.Map, m_Actor.Location.Position, out string reason)) return null;
      if (!itemTrap.IsActivated && !itemTrap.Model.ActivatesWhenDropped)
        return new ActionUseItem(m_Actor, itemTrap);
      game.DoEmote(m_Actor, string.Format("{0} {1}!", reason, itemTrap.AName), true);
      return new ActionDropItem(m_Actor, itemTrap);
    }

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
        else if (map.HasExitAt(pos)) reason = "protecting the exit with";
      }
      if (string.IsNullOrEmpty(reason)) return false;
      Inventory itemsAt = map.GetItemsAt(pos);
      if (null == itemsAt) return true;
      return 3 >= itemsAt.Items.Count(it => it is ItemTrap itemTrap && itemTrap.IsActivated);
    }

    protected ActorAction BehaviorAttackBarricade()
    {
      Map map = m_Actor.Location.Map;
      Dictionary<Point,DoorWindow> doors = map.FindAdjacent(m_Actor.Location.Position, (m,pt) => {
        DoorWindow doorWindow = m.GetMapObjectAtExt(pt) as DoorWindow;
        return ((doorWindow?.IsBarricaded ?? false) ? doorWindow : null);
      });
      if (0 >= doors.Count) return null;
      DoorWindow doorWindow1 = RogueForm.Game.Rules.DiceRoller.Choose(doors).Value;
      return (m_Actor.CanBreak(doorWindow1) ? new ActionBreak(m_Actor, doorWindow1) : null);
    }

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

    protected ActionPush BehaviorPushNonWalkableObject()
    {
      if (!m_Actor.AbleToPush) return null;
      Map map = m_Actor.Location.Map;
      Dictionary<Point,MapObject> objs = map.FindAdjacent(m_Actor.Location.Position,(m,pt) => {
        MapObject o = m.GetMapObjectAtExt(pt);
        if (o?.IsWalkable ?? true) return null;
        return (m_Actor.CanPush(o) ? o : null);
      });
      if (0 >= objs.Count) return null;
      ActionPush tmp = new ActionPush(m_Actor, RogueForm.Game.Rules.DiceRoller.Choose(objs).Value, RogueForm.Game.Rules.RollDirection());
      return (tmp.IsLegal() ? tmp : null);
    }

    protected ActionPush BehaviorPushNonWalkableObjectForFood()
    {
      if (!m_Actor.AbleToPush) return null;
      Map map = m_Actor.Location.Map;
      Dictionary<Point,MapObject> objs = map.FindAdjacent(m_Actor.Location.Position,(m,pt) => {
        MapObject o = m.GetMapObjectAtExt(pt);
        if (null == o) return null;
        if (o.IsWalkable) return null;
        if (o.IsJumpable) return null;
        return (m_Actor.CanPush(o) ? o : null);
      });
      if (0 >= objs.Count) return null;
      ActionPush tmp = new ActionPush(m_Actor, RogueForm.Game.Rules.DiceRoller.Choose(objs).Value, RogueForm.Game.Rules.RollDirection());
      return (tmp.IsLegal() ? tmp : null);
    }

	protected HashSet<Point> FriendsLoF()
	{
      Dictionary<Point,Actor> enemies = enemies_in_FOV;
      Dictionary<Point,Actor> friends = friends_in_FOV;
	  if (0 >= (enemies?.Count ?? 0)) return null;
	  if (0 >= (friends?.Count ?? 0)) return null;
	  HashSet<Point> tmp = new HashSet<Point>();
	  foreach(var f in friends) {
        if (!f.Value.HasEquipedRangedWeapon()) continue;
	    foreach(var e in enemies) {
		  if (!f.Value.IsEnemyOf(e.Value)) continue;
		  if (f.Value.CurrentRangedAttack.Range<Rules.GridDistance(f.Value.Location,e.Value.Location)) continue;
		  List<Point> line = new List<Point>();
	      LOS.CanTraceViewLine(f.Value.Location, e.Value.Location, f.Value.CurrentRangedAttack.Range, line);
          tmp.UnionWith(line);
		}
	  }
	  return (0<tmp.Count ? tmp : null);
	}

    protected virtual ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other?.IsDead ?? true) return null;
      int num = Rules.GridDistance(m_Actor.Location, other.Location);
      if (CanSee(other.Location) && num <= maxDist) return new ActionWait(m_Actor);
      if (other.Location.Map != m_Actor.Location.Map) {
        Exit exitAt = m_Actor.Location.Exit;
        if (exitAt != null && exitAt.ToMap == other.Location.Map && m_Actor.CanUseExit(m_Actor.Location.Position))
          return BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      ActorAction actorAction = BehaviorIntelligentBumpToward(other.Location, false, false);
      if (actorAction == null || !actorAction.IsLegal()) return null;
      if (other.IsRunning) RunIfPossible();
      return actorAction;
    }

    protected ActorAction BehaviorTrackScent(List<Percept_<AIScent>> scents)
    {
      if (0 >= (scents?.Count ?? 0)) return null;
      Percept_<AIScent> percept = FilterStrongestScent(scents);
      if (m_Actor.Location != percept.Location) {
        ActorAction tmpAction = BehaviorIntelligentBumpToward(percept.Location, false, false);
        if (null!=tmpAction) return tmpAction;
        var dir = Direction.FromVector(new Point(percept.Location.Position.X-m_Actor.Location.Position.X,percept.Location.Position.Y- m_Actor.Location.Position.Y));
        // Cf. Angband.
        if (null!=dir) {
          if (RogueForm.Game.Rules.DiceRoller.RollChance(50)) { // anti-clockwise bias
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

    protected virtual ActorAction BehaviorChargeEnemy(Percept target, bool canCheckBreak, bool canCheckPush)
    {
      Actor actor = target.Percepted as Actor;
      ActorAction tmpAction = BehaviorMeleeAttack(actor);
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location))
          return new ActionWait(m_Actor);
        tmpAction = BehaviorHeadFor(target.Location, canCheckBreak, canCheckPush);
        if (null == tmpAction) return null;
        if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) RunIfPossible();
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
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, string[] emotes, RouteFinder.SpecialActions allowedChargeActions)
    {
      const ActorCourage courage = ActorCourage.CAUTIOUS;
      Percept target = FilterNearest(enemies);
      bool doRun = false;	// only matters when fleeing
      Actor enemy = target.Percepted as Actor;
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

      ActorAction tmpAction = null;

      if (decideToFlee) {
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[0], enemy.Name));
        // using map objects goes here
        // barricading goes here
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && (Lighting.DARKNESS== m_Actor.Location.Map.Lighting || game.Rules.RollChance(FLEE_THROUGH_EXIT_CHANCE))) {
          tmpAction = BehaviorUseExit(BaseAI.UseExitFlags.NONE);
          if (null != tmpAction) {
            bool flag3 = true;
            if (m_Actor.HasLeader) {
              Exit exitAt = m_Actor.Location.Exit;
              if (exitAt != null) flag3 = m_Actor.Leader.Location.Map == exitAt.ToMap;
            }
            if (flag3) {
              m_Actor.Activity = Activity.FLEEING;
              return tmpAction;
            }
          }
        }
        // XXX we should run for the exit here
        tmpAction = BehaviorWalkAwayFrom(enemies.Select(p => p.Location.Position));
        if (null != tmpAction) {
          if (doRun) RunIfPossible();
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_TRAPPED_CHANCE))
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
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[2], enemy.Name), true);
        return tmpAction;
      }
      return null;
    }

    // the intelligent callers are using the override at OrderableAI
    protected virtual ActorAction BehaviorExplore(ExplorationData exploration)
    {
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position.X - m_prevLocation.Position.X, m_Actor.Location.Position.Y - m_prevLocation.Position.Y);
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location loc = m_Actor.Location + dir;
        if (!IsValidMoveTowardGoalAction(Rules.IsBumpableFor(m_Actor, loc))) return float.NaN;
        if (!loc.Map.IsInBounds(loc.Position)) {
          Location? test = loc.Map.Normalize(loc.Position);
          if (null == test) return float.NaN;
          loc = test.Value;
        }
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
        if (!exploration.HasExplored(loc)) num += EXPLORE_LOCS;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        // this is problematic when the door is the previous location.  Do not overwhelm in/out
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) {
          num += (loc != PrevLocation ? EXPLORE_BARRICADES : -EXPLORE_DIRECTION);
        }
        if (map.IsInsideAtExt(position)) {
          if (map.LocalTime.IsNight) num += EXPLORE_INOUT;
        }
        else if (!map.LocalTime.IsNight) num += EXPLORE_INOUT;
        if (dir == prevDirection) num += EXPLORE_DIRECTION;
        return (float) (num + RogueForm.Game.Rules.Roll(0, EXPLORE_RANDOM));
      }, (a, b) => a > b);
      if (choiceEval != null) return new ActionBump(m_Actor, choiceEval.Choice);
      return null;
    }

    protected ActorAction BehaviorUseExit(BaseAI.UseExitFlags useFlags)
    {
      Exit exitAt = m_Actor.Location.Exit;
      if (null == exitAt) return null;
      if ((useFlags & BaseAI.UseExitFlags.DONT_BACKTRACK) != BaseAI.UseExitFlags.NONE && exitAt.Location == m_prevLocation) return null;
      string reason = exitAt.ReasonIsBlocked(m_Actor);
      if (string.IsNullOrEmpty(reason)) return (m_Actor.CanUseExit(m_Actor.Location.Position) ? new ActionUseExit(m_Actor, m_Actor.Location.Position) : null);
      if ((useFlags & BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES) != BaseAI.UseExitFlags.NONE) {
        Actor actorAt = exitAt.Location.Actor;
        if (actorAt != null && m_Actor.IsEnemyOf(actorAt) && m_Actor.CanMeleeAttack(actorAt))
          return new ActionMeleeAttack(m_Actor, actorAt);
      }
      if ((useFlags & BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS) != BaseAI.UseExitFlags.NONE) {
        MapObject mapObjectAt = exitAt.Location.MapObject;
        if (mapObjectAt != null && m_Actor.CanBreak(mapObjectAt))
          return new ActionBreak(m_Actor, mapObjectAt);
      }
      return null;
    }

    protected ActorAction BehaviorGoEatFoodOnGround(List<Percept> stacksPercepts)
    {
      if (stacksPercepts == null) return null;
      List<Percept> percepts = stacksPercepts.Filter(p => (p.Percepted as Inventory).Has<ItemFood>());
      if (percepts == null) return null;
      ItemFood firstByType = m_Actor.Location.Items?.GetFirst<ItemFood>();
      if (null != firstByType) return new ActionEatFoodOnGround(m_Actor, firstByType);
      Percept percept = FilterNearest(percepts);
      return BehaviorStupidBumpToward(percept.Location,false,false);
    }

    protected ActorAction BehaviorGoEatCorpse(List<Percept> percepts)
    {
	  if (!Session.Get.HasCorpses) return null;
      if (!m_Actor.CanEatCorpse()) return null;
      if (m_Actor.Model.Abilities.IsUndead && m_Actor.HitPoints >= m_Actor.MaxHPs) return null;
	  List<Percept> corpsesPercepts = percepts.FilterT<List<Corpse>>();
	  if (null == corpsesPercepts) return null;
      Percept percept = FilterNearest(corpsesPercepts);
	  if (m_Actor.Location.Position==percept.Location.Position) {
        return new ActionEatCorpse(m_Actor, (percept.Percepted as List<Corpse>)[0]);
	  }
      return BehaviorHeadFor(percept.Location,true,true);
    }

    protected void RunIfPossible()
    {
      m_Actor.IsRunning = m_Actor.CanRun();  // alpha10 fix
    }

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
      float avgDistance = (float) (dangers.Sum(pt => Rules.GridDistance(from, pt))) / (1 + dangers.Count());
#endregion
#region 1 Avoid getting in corners.
      int countFreeSquares = map.CountAdjacentTo(from,pt => pt == m_Actor.Location.Position || map.IsWalkableFor(pt, m_Actor));
      float avoidCornerBonus = countFreeSquares * 0.1f;
#endregion
#region 2 Prefer going outside/inside if majority of dangers are inside/outside.
      bool isFromInside = map.IsInsideAtExt(from);
      int majorityDangersInside = 0;
      foreach (Point danger in dangers) {
        if (map.IsInsideAtExt(danger))
          ++majorityDangersInside;
        else
          --majorityDangersInside;
      }
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
        MapObject mapObjectAt = map.GetMapObjectAtExt(from);
        if (mapObjectAt != null && mapObjectAt.IsJumpable) jumpPenalty = 0.1f;
      }
#endregion
      float heuristicFactorBonus = 1f + avoidCornerBonus + inOutBonus - jumpPenalty;
      return avgDistance * heuristicFactorBonus;
    }

    // isBetterThanEvalFn will never see NaN
    static protected ChoiceEval<_T_> Choose<_T_>(IEnumerable<_T_> listOfChoices, Func<_T_, bool> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
#if DEBUG
      if (null == isChoiceValidFn) throw new ArgumentNullException(nameof(isChoiceValidFn));
      if (null == evalChoiceFn) throw new ArgumentNullException(nameof(evalChoiceFn));
      if (null == isBetterEvalThanFn) throw new ArgumentNullException(nameof(isBetterEvalThanFn));
#endif
      if (!listOfChoices?.Any() ?? true) return null;

      Dictionary<float, List<ChoiceEval<_T_>>> choiceEvalDict = new Dictionary<float, List<ChoiceEval<_T_>>>();

      float num = float.NaN;
      foreach(_T_ tmp in listOfChoices) {
        if (!isChoiceValidFn(tmp)) continue;
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;
        if (float.IsNaN(num)) {
          num = f;
        } else if (isBetterEvalThanFn(f, num)) {
          num = f;
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;

        ChoiceEval< _T_ > tmp2 = new ChoiceEval<_T_>(tmp, f);
        if (choiceEvalDict.TryGetValue(f,out var dest)) dest.Add(tmp2);
        else choiceEvalDict[f] = new List<ChoiceEval<_T_>>{ tmp2 };
      }

      if (!choiceEvalDict.TryGetValue(num, out List<ChoiceEval<_T_>> ret_from)) return null;
      if (1 == ret_from.Count) return ret_from[0];
      return RogueForm.Game.Rules.DiceRoller.Choose(ret_from);
    }

    static protected ChoiceEval<_T_> Choose<_T_>(IEnumerable<_T_> listOfChoices, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
#if DEBUG
      if (null == evalChoiceFn) throw new ArgumentNullException(nameof(evalChoiceFn));
      if (null == isBetterEvalThanFn) throw new ArgumentNullException(nameof(isBetterEvalThanFn));
#endif
      if (!listOfChoices?.Any() ?? true) return null;

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
      return RogueForm.Game.Rules.DiceRoller.Choose(candidates);
    }

    // isBetterThanEvalFn will never see NaN
    static protected ChoiceEval<_DATA_> ChooseExtended<_T_, _DATA_>(IEnumerable<_T_> listOfChoices, Func<_T_, _DATA_> isChoiceValidFn, Func<_T_, _DATA_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
#if DEBUG
      if (null == isChoiceValidFn) throw new ArgumentNullException(nameof(isChoiceValidFn));
      if (null == evalChoiceFn) throw new ArgumentNullException(nameof(evalChoiceFn));
      if (null == isBetterEvalThanFn) throw new ArgumentNullException(nameof(isBetterEvalThanFn));
#endif
      if (!listOfChoices?.Any() ?? true) return null;

      Dictionary<float, List<ChoiceEval<_DATA_>>> choiceEvalDict = new Dictionary<float, List<ChoiceEval<_DATA_>>>();

      float num = float.NaN;
      foreach(_T_ tmp in listOfChoices) {
        _DATA_ choice = isChoiceValidFn(tmp);
        if (null == choice) continue;
        float f = evalChoiceFn(tmp, choice);
        if (float.IsNaN(f)) continue;
        if (float.IsNaN(num)) {
          num = f;
        } else if (isBetterEvalThanFn(f, num)) {
          num = f;
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;

        ChoiceEval< _DATA_ > tmp2 = new ChoiceEval<_DATA_>(choice, f);
        if (choiceEvalDict.TryGetValue(f,out var dest))  dest.Add(tmp2);
        else choiceEvalDict[f] = new List<ChoiceEval<_DATA_>>{ tmp2 };
      }

      if (!choiceEvalDict.TryGetValue(num, out List<ChoiceEval<_DATA_>> ret_from)) return null;
      return RogueForm.Game.Rules.DiceRoller.Choose(ret_from);
    }

    static protected bool IsValidFleeingAction(ActorAction a)
    {
      if (null == a) return false;
      if (!(a is ActionMoveStep) && !(a is ActionOpenDoor))
        return a is ActionSwitchPlace;
      return true;
    }

    protected bool IsValidWanderAction(ActorAction a)
    {
      if (null == a) return false;
      if (a is ActionMoveStep) return true;
      if (a is ActionSwitchPlace) return true;
//    if (a is ActionPush) return true; // wasn't being generated in RS alpha 9...results do not look good
      if (a is ActionOpenDoor) return true;
      if (a is ActionBashDoor) return true;
      if (a is ActionBarricadeDoor) return true;
      if (a is ActionGetFromContainer) {    // XXX Jason Myers: not OrderableAI but capable of this
        Item it = (a as ActionGetFromContainer).Item;
        return (m_Actor.Controller as ObjectiveAI)?.IsInterestingItem(it) ?? true;
      }
      if (!(this is OrderableAI downcast)) return false;
      if (a is ActionChat) {
        return downcast.Directives.CanTrade || (a as ActionChat).Target == m_Actor.Leader;
      }
      return false;
    }

    static protected bool IsValidMoveTowardGoalAction(ActorAction a)
    {
      if (a != null && !(a is ActionChat) && (!(a is ActionGetFromContainer) && !(a is ActionSwitchPowerGenerator)))
        return !(a is ActionRechargeItemBattery);
      return false;
    }

    protected bool IsOccupiedByOther(Location loc)  // percept locations are normalized
    {
      Actor actorAt = loc.Actor;
      if (actorAt != null) return actorAt != m_Actor;
      return false;
    }

    protected bool WantToEvadeMelee(Actor actor, ActorCourage courage, Actor target)
    {
//    if (WillTireAfterAttack(actor)) return true;  // post-process this, handling this here is awful for rats
      if (actor.Speed > target.Speed) {
        if (actor.WillActAgainBefore(target)) return false; // caller must handle distance 2 correctly.
        if (Rules.IsAdjacent(actor.Location,target.Location)) return true;  // back-and-smack indicated
//      if (target.TargetActor == actor) return true;
        return false;
      }
      Actor weakerInMelee = FindWeakerInMelee(m_Actor, target);
      return weakerInMelee != target && (weakerInMelee == m_Actor || courage != ActorCourage.COURAGEOUS);
    }

    private static Actor FindWeakerInMelee(Actor a, Actor b)
    {
	  int a_dam = a.MeleeAttack(b).DamageValue - b.CurrentDefence.Protection_Hit;
	  int b_dam = b.MeleeAttack(a).DamageValue - a.CurrentDefence.Protection_Hit;
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

    protected static bool IsBetween(Point A, Point between, Point B)
    {
      return (double) Rules.StdDistance(A, between) + (double) Rules.StdDistance(B, between) <= (double) Rules.StdDistance(A, B) + 0.25;
    }

    protected bool IsFriendOf(Actor other)
    {
      if (!m_Actor.IsEnemyOf(other)) return m_Actor.Faction == other.Faction;
      return false;
    }

    protected static Actor GetNearestTargetFor(Actor actor)
    {
      Actor actor1 = null;
      int num1 = int.MaxValue;
      foreach (Actor actor2 in actor.Location.Map.Actors) {
        if (!actor2.IsDead && actor2 != actor && actor.IsEnemyOf(actor2)) {
          int num2 = Rules.GridDistance(actor2.Location.Position, actor.Location.Position);
          if (num2 < num1 && (num2 == 1 || LOS.CanTraceViewLine(actor.Location, actor2.Location.Position))) {
            num1 = num2;
            actor1 = actor2;
          }
        }
      }
      return actor1;
    }

    // XXX these two break down cross-map
    protected bool CanReachSimple(Location dest, RouteFinder.SpecialActions allowedActions)
    {
       if (m_RouteFinder == null) m_RouteFinder = new RouteFinder(this);
       m_RouteFinder.AllowedActions = allowedActions;
       int maxDist = Rules.GridDistance(m_Actor.Location, dest);
       return m_RouteFinder.CanReachSimple(RogueForm.Game, dest, maxDist, Rules.GridDistance);
    }

    protected void FilterOutUnreachablePercepts(ref List<Percept> percepts, RouteFinder.SpecialActions allowedActions)
    {
      if (null == percepts) return;
      int i = 0;
      while (i < percepts.Count) {
        if (CanReachSimple(percepts[i].Location, allowedActions)) i++;
        else percepts.RemoveAt(i);
      }
    }

#if DEAD_FUNC
    protected static List<Exit> ListAdjacentExits(Location fromLocation)
    {
      IEnumerable<Exit> adj_exits = Direction.COMPASS.Select(dir=> fromLocation.Map.GetExitAt(fromLocation.Position + dir)).Where(exit=>null!=exit);
      return adj_exits.Any() ? adj_exits.ToList() : null;
    }

    protected Exit PickAnyAdjacentExit(RogueGame game, Location fromLocation)
    {
      List<Exit> exitList = ListAdjacentExits(fromLocation);
      return null != exitList ? exitList[game.Rules.Roll(0, exitList.Count)] : null;
    }
#endif

#if DEAD_FUNC
    public static bool IsZoneChange(Map map, Point pos)
    {
      List<Zone> zonesHere = map.GetZonesAt(pos);
      if (zonesHere == null) return false;
      return map.HasAnyAdjacentInMap(pos, (Predicate<Point>) (adj =>
      {
        List<Zone> zonesAt = map.GetZonesAt(adj);
        if (zonesAt == null) return false;
        foreach (Zone zone in zonesAt) {
          if (!zonesHere.Contains(zone)) return true;
        }
        return false;
      }));
    }
#endif

        protected static Point RandomPositionNear(Rules rules, Map map, Point goal, int range)
    {
      int x = goal.X + rules.Roll(-range, range);
      int y = goal.Y + rules.Roll(-range, range);
      map.TrimToBounds(ref x, ref y);
      return new Point(x, y);
    }

    protected class ChoiceEval<_T_>
    {
      public _T_ Choice { get; private set; }

      public float Value { get; private set; }

      public ChoiceEval(_T_ choice, float value)
      {
                Choice = choice;
                Value = value;
      }

      public override string ToString()
      {
        return string.Format("ChoiceEval({0}; {1:F})", (object)Choice == null ? (object) "NULL" : (object)Choice.ToString(), (object)Value);
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
}
