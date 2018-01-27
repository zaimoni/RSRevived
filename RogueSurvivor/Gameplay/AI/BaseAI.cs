﻿// Decompiled with JetBrains decompiler
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
    protected const int FLEE_THROUGH_EXIT_CHANCE = 50;
    protected const int EMOTE_FLEE_CHANCE = 30;
    private const int EMOTE_FLEE_TRAPPED_CHANCE = 50;
    protected const int EMOTE_CHARGE_CHANCE = 30;
    private const float MOVE_DISTANCE_PENALTY = 0.42f;
    private const float LEADER_LOF_PENALTY = 1f;
    public const int MAX_EMOTES = 3;    // 0: flee; 1: last stand; 2:charge

    private Location m_prevLocation;

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
      if (!actorAction?.IsLegal() ?? false) throw new InvalidOperationException("illegal action returned from SelectAction");
#endif
      m_prevLocation = m_Actor.Location;
      if (actorAction != null) return actorAction;
      return new ActionWait(m_Actor);
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

    static protected Percept FilterStrongestScent(List<Percept> scents)
    {
      if (scents == null || scents.Count == 0) return null;
      Percept percept = null;
      int scent_strength = 0;   // minimum valid scent strength is 1
      foreach (Percept scent in scents) {
        SmellSensor.AIScent aiScent2 = scent.Percepted as SmellSensor.AIScent;
#if DEBUG
        if (aiScent2 == null) throw new InvalidOperationException("percept not an aiScent");
#endif
        if (aiScent2.Strength > scent_strength) {
          scent_strength = aiScent2.Strength;
          percept = scent;
        }
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
      if (null == percepts || 0 == percepts.Count) return null;
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

    protected ActorAction BehaviorWander(Predicate<Location> goodWanderLocFn=null)
    {
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location loc = m_Actor.Location + dir;
        if (null != goodWanderLocFn && !goodWanderLocFn(loc)) return float.NaN;
        if (!isValidWanderAction(Rules.IsBumpableFor(m_Actor, loc))) return float.NaN;
        if (!loc.Map.IsInBounds(loc.Position)) {
          Location? test = loc.Map.Normalize(loc.Position);
          if (null == test) return float.NaN;
          loc = test.Value;
        }
        int num = RogueForm.Game.Rules.Roll(0, 666);
        if (m_Actor.Model.Abilities.IsIntelligent && null != loc.Map.GetActivatedTrapAt(loc.Position))
          num -= 1000;
        return (float) num;
      }, (a, b) => a > b);
      return (choiceEval != null ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    protected ActorAction BehaviorBumpToward(Point goal, Func<Point, Point, float> distanceFn)
    {
#if DEBUG
      if (null == distanceFn) throw new ArgumentNullException(nameof(distanceFn));
#endif
      ChoiceEval<ActorAction> choiceEval = ChooseExtended(Direction.COMPASS, dir =>
      {
        Location location = m_Actor.Location + dir;
        ActorAction a = Rules.IsBumpableFor(m_Actor, location);
        if (a == null) {
          if (m_Actor.Model.Abilities.IsUndead && m_Actor.AbleToPush) {
            MapObject mapObjectAt = location.MapObject;
            if (mapObjectAt != null && m_Actor.CanPush(mapObjectAt)) {
              Direction pushDir = RogueForm.Game.Rules.RollDirection();
              if (mapObjectAt.CanPushTo(mapObjectAt.Location.Position + pushDir))
                return new ActionPush(m_Actor, mapObjectAt, pushDir);
            }
          }
          return null;
        }
        if (location.Position == goal || IsValidMoveTowardGoalAction(a)) return a;
        return null;
      }, dir => distanceFn(m_Actor.Location.Position + dir, goal), (a, b) => a < b);
      if (choiceEval != null) return choiceEval.Choice;
      return null;
    }

    protected ActorAction BehaviorStupidBumpToward(Point goal)
    {
      return BehaviorBumpToward(goal, (Func<Point, Point, float>) ((ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(ptA, ptB);
        if (!m_Actor.Location.Map.IsWalkableFor(ptA, m_Actor)) num += 0.42f;
        return num;
      }));
    }

    protected ActorAction BehaviorStupidBumpToward(Location goal)
    {
      if (m_Actor.Location.Map == goal.Map) return BehaviorStupidBumpToward(goal.Position);
      Location? test = m_Actor.Location.Map.Denormalize(goal);
      if (null == test) return null;
      return BehaviorStupidBumpToward(test.Value.Position);
    }

    protected ActorAction BehaviorIntelligentBumpToward(Point goal)
    {
      float currentDistance = (float)Rules.StdDistance(m_Actor.Location.Position, goal);
      ActorCourage courage = (this as OrderableAI)?.Directives.Courage ?? ActorCourage.CAUTIOUS;
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
      return BehaviorBumpToward(goal, (Func<Point, Point, float>) ((ptA, ptB) =>
      {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(ptA, ptB);
        if ((double) num >= (double) currentDistance) return float.NaN;
        if (!imStarvingOrCourageous) {
          int trapsMaxDamage = m_Actor.Location.Map.TrapsMaxDamageAt(ptA);
          if (trapsMaxDamage > 0) {
            if (trapsMaxDamage >= m_Actor.HitPoints) return float.NaN;
            num += 0.42f;
          }
        }
        return num;
      }));
    }

    protected ActorAction BehaviorIntelligentBumpToward(Location goal)
    {
      if (m_Actor.Location.Map == goal.Map) return BehaviorIntelligentBumpToward(goal.Position);
      Location? test = m_Actor.Location.Map.Denormalize(goal);
      if (null == test) return null;
      return BehaviorIntelligentBumpToward(test.Value.Position);
    }

    protected ActorAction BehaviorHeadFor(Location goal)
    {
      if (m_Actor.Model.Abilities.IsIntelligent) return BehaviorIntelligentBumpToward(goal);
      return BehaviorStupidBumpToward(goal);
    }

    protected ActorAction BehaviorHeadFor(IEnumerable<Location> goals)
    {
      if (!goals?.Any() ?? true) return null;
      int dist = int.MaxValue;
      ActorAction ret = null;
      foreach(Location goal in goals) {
        int new_dist = Rules.GridDistance(m_Actor.Location, goal);
        if (dist <= new_dist) continue;
        ActorAction tmp = BehaviorHeadFor(goal);
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
        ActorAction tmp = BehaviorStupidBumpToward(percept.Location);
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
      if (m_Actor.CanFireAt(target)) {
        m_Actor.Activity = Activity.FIGHTING;
        m_Actor.TargetActor = target;
        return new ActionRangedAttack(m_Actor, target);
      }
      return null;
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
      game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) reason, (object) itemTrap.AName));
      return new ActionDropItem(m_Actor, itemTrap);
    }

    protected bool IsGoodTrapSpot(Map map, Point pos, out string reason)
    {
      reason = "";
      bool isInside = map.IsInsideAt(pos);
      if (!isInside && map.GetCorpsesAt(pos) != null) reason = "that corpse will serve as a bait for";
      else if (m_prevLocation.Map.IsInsideAt(m_prevLocation.Position) != isInside) reason = "protecting the building with";
      else {
        if (map.GetMapObjectAt(pos) is DoorWindow) reason = "protecting the doorway with";
        else if (map.HasExitAt(pos)) reason = "protecting the exit with";
      }
      if (string.IsNullOrEmpty(reason)) return false;
      Inventory itemsAt = map.GetItemsAt(pos);
      if (null == itemsAt) return true;
      return 3 >= itemsAt.Items.Count(it =>
      {
          ItemTrap itemTrap = it as ItemTrap;
          return null != itemTrap && itemTrap.IsActivated;
      });
    }

    protected ActorAction BehaviorAttackBarricade()
    {
      Map map = m_Actor.Location.Map;
      Dictionary<Point,DoorWindow> doors = map.FindAdjacent(m_Actor.Location.Position, (m,pt) => {
        DoorWindow doorWindow = m.GetMapObjectAtExt(pt) as DoorWindow;
        return ((doorWindow?.IsBarricaded ?? false) ? doorWindow : null);
      });
      if (0 >= doors.Count) return null;
      DoorWindow doorWindow1 = RogueForm.Game.Rules.Choose(doors).Value;
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
        return BehaviorIntelligentBumpToward(percept.Location.Position);
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
      ActionPush tmp = new ActionPush(m_Actor, RogueForm.Game.Rules.Choose(objs).Value, RogueForm.Game.Rules.RollDirection());
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
      ActionPush tmp = new ActionPush(m_Actor, RogueForm.Game.Rules.Choose(objs).Value, RogueForm.Game.Rules.RollDirection());
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

    protected List<Point> DecideMove_WaryOfTraps(List<Point> src)
    {
	  Dictionary<Point,int> trap_damage_field = new Dictionary<Point,int>();
	  foreach (Point pt in src) {
		trap_damage_field[pt] = m_Actor.Location.Map.TrapsMaxDamageAt(pt);
	  }
	  IEnumerable<Point> safe = src.Where(pt => 0>=trap_damage_field[pt]);
	  int new_dest = safe.Count();
      if (0 == new_dest) {
		safe = src.Where(pt => m_Actor.HitPoints>trap_damage_field[pt]);
		new_dest = safe.Count();
      }
      return ((0 < new_dest && new_dest < src.Count) ? safe.ToList() : src);
    }

    static private List<Point> DecideMove_Avoid(List<Point> src, IEnumerable<Point> avoid)
    {
      if (null == avoid) return src;
      IEnumerable<Point> ok = src.Except(avoid);
	  int new_dest = ok.Count();
      return ((0 < new_dest && new_dest < src.Count) ? ok.ToList() : src);
    }

    private List<Location> DecideMove_Avoid(List<Location> src, IEnumerable<Point> avoid)
    {
      if (null == avoid) return src;
      List<Location> ok = new List<Location>();
      foreach(Location loc in src) {
        Location? test = m_Actor.Location.Map.Denormalize(loc);
        if (null != test && avoid.Contains(test.Value.Position)) continue;  // null is expected for using a same-district exit
        ok.Add(loc);
      }
	  int new_dest = ok.Count;
      return ((0 < new_dest && new_dest < src.Count) ? ok : src);
    }


    private List<Point> DecideMove_NoJump(List<Point> src)
    {
      IEnumerable<Point> no_jump = src.Where(pt=> {
        MapObject tmp2 = m_Actor.Location.Map.GetMapObjectAt(pt);
          return !tmp2?.IsJumpable ?? true;
      });
	  int new_dest = no_jump.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_jump.ToList() : src);
    }

    private List<Location> DecideMove_NoJump(List<Location> src)
    {
      IEnumerable<Location> no_jump = src.Where(loc=> {
        MapObject tmp2 = loc.MapObject;
        return !tmp2?.IsJumpable ?? true;
      });
	  int new_dest = no_jump.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_jump.ToList() : src);
    }

    static private List<Point> DecideMove_maximize_visibility(List<Point> dests, HashSet<Point> tainted, HashSet<Point> new_los, Dictionary<Point,HashSet<Point>> hypothetical_los) {
        tainted.IntersectWith(new_los);
        if (0>=tainted.Count) return dests;
        Dictionary<Point,int> taint_exposed = new Dictionary<Point,int>();
        foreach(Point pt in dests) {
          if (!hypothetical_los.ContainsKey(pt)) {
            taint_exposed[pt] = 0;
            continue;
          }
          HashSet<Point> tmp2 = new HashSet<Point>(hypothetical_los[pt]);
          tmp2.IntersectWith(tainted);
          taint_exposed[pt] = tmp2.Count;
        }
        int max_taint_exposed = dests.Select(pt=>taint_exposed[pt]).Max();
        taint_exposed.OnlyIf(val=>max_taint_exposed==val);
        return taint_exposed.Keys.ToList();
    }

    private List<Location> DecideMove_maximize_visibility(List<Location> dests, HashSet<Point> tainted, HashSet<Point> new_los, Dictionary<Point,HashSet<Point>> hypothetical_los) {
        tainted.IntersectWith(new_los);
        if (0>=tainted.Count) return dests;
        var taint_exposed = new Dictionary<Location,int>();
        foreach(Location loc in dests) {
          Location? test = m_Actor.Location.Map.Denormalize(loc);
          if (null == test) {   // assume same-district exit use...don't really want to do this when other targets are close
            taint_exposed[loc] = 0;
            continue;
          }
          if (!hypothetical_los.ContainsKey(test.Value.Position)) {
            taint_exposed[loc] = 0;
            continue;
          }
          HashSet<Point> tmp2 = new HashSet<Point>(hypothetical_los[test.Value.Position]);
          tmp2.IntersectWith(tainted);
          taint_exposed[loc] = tmp2.Count;
        }
        int max_taint_exposed = dests.Select(pt=>taint_exposed[pt]).Max();
        taint_exposed.OnlyIf(val=>max_taint_exposed==val);
        return taint_exposed.Keys.ToList();
    }

    protected ActorAction DecideMove(IEnumerable<Point> src)
	{
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
	  List<Point> tmp = src.ToList();

	  // do not get in the way of allies' line of fire
	  if (2 <= tmp.Count) tmp = DecideMove_Avoid(tmp, FriendsLoF());

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  bool want_LOS_heuristics = false;
	  ThreatTracking threats = m_Actor.Threats;
	  if (null != threats) want_LOS_heuristics = true;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  if (null != sights_to_see) want_LOS_heuristics = true;

	  Dictionary<Point,HashSet<Point>> hypothetical_los = ((want_LOS_heuristics && 2 <= tmp.Count) ? new Dictionary<Point,HashSet<Point>>() : null);
      HashSet<Point> new_los = new HashSet<Point>();
	  if (null != hypothetical_los) {
	    // only need points newly in FOV that aren't currently
	    foreach(Point pt in tmp) {
	      hypothetical_los[pt] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt)).Except(FOV));
          new_los.UnionWith(hypothetical_los[pt]);
	    }
	  }
      // only need to check if new locations seen
      if (0 >= new_los.Count) {
        threats = null;
        sights_to_see = null;
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position.X - tmp_LOSrange, m_Actor.Location.Position.Y - tmp_LOSrange, 2*tmp_LOSrange+1,2*tmp_LOSrange+1);

      if (null != threats && 2<=tmp.Count) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
	  }
	  if (null != sights_to_see && 2<=tmp.Count) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
	  }

      // weakly prefer not to jump
      if (2 <= tmp.Count)  tmp = DecideMove_NoJump(tmp);
	  var secondary = new List<ActorAction>();
	  while(0<tmp.Count) {
	    int i = RogueForm.Game.Rules.Roll(0, tmp.Count);
		ActorAction ret = Rules.IsPathableFor(m_Actor, new Location(m_Actor.Location.Map, tmp[i]));
        if (null == ret || !ret.IsLegal()) {    // not really an option
		  tmp.RemoveAt(i);
          continue;
        }
        if (ret is ActionShove shove && shove.Target.Controller is ObjectiveAI ai) {
           Dictionary<Point, int> ok_dests = ai.MovePlanIf(shove.Target.Location.Position);
           if (Rules.IsAdjacent(shove.To,m_Actor.Location.Position)) {
             // non-moving shove...would rather not spend the stamina if there is a better option
             if (null != ok_dests  && ok_dests.ContainsKey(shove.To)) secondary.Add(ret); // shove is to a wanted destination
       		 tmp.RemoveAt(i);
             continue;
           }
           if (   null == ok_dests // shove is rude
               || !ok_dests.ContainsKey(shove.To)) // shove is not to a wanted destination
                {
                secondary.Add(ret);
    		    tmp.RemoveAt(i);
                continue;
                }
        }
		return ret;
	  }
      if (0<secondary.Count) return secondary[RogueForm.Game.Rules.Roll(0,secondary.Count)];
	  return null;
	}

    protected ActorAction DecideMove(Dictionary<Location,int> src)
	{
      if (null == src) return null; // does happen
      var legal_steps = m_Actor.OnePathRange(m_Actor.Location); // other half
	  List<Location> tmp = src.Keys.ToList();

	  // do not get in the way of allies' line of fire
	  if (2 <= tmp.Count) tmp = DecideMove_Avoid(tmp, FriendsLoF());

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  bool want_LOS_heuristics = false;
	  ThreatTracking threats = m_Actor.Threats;
	  if (null != threats) want_LOS_heuristics = true;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  if (null != sights_to_see) want_LOS_heuristics = true;

	  Dictionary<Point,HashSet<Point>> hypothetical_los = ((want_LOS_heuristics && 2 <= tmp.Count) ? new Dictionary<Point,HashSet<Point>>() : null);
      HashSet<Point> new_los = new HashSet<Point>();
	  if (null != hypothetical_los) {
	    // only need points newly in FOV that aren't currently
	    foreach(var x in tmp) {
          if (!legal_steps.ContainsKey(x)) continue;
          if (legal_steps[x] is ActionUseExit) continue;
          Location? test = m_Actor.Location.Map.Denormalize(x);
          if (null == test) throw new ArgumentNullException(nameof(test));
	      hypothetical_los[test.Value.Position] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, test.Value).Except(FOV));
          new_los.UnionWith(hypothetical_los[test.Value.Position]);
	    }
	  }
      // only need to check if new locations seen
      if (0 >= new_los.Count) {
        threats = null;
        sights_to_see = null;
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position.X - tmp_LOSrange, m_Actor.Location.Position.Y - tmp_LOSrange, 2*tmp_LOSrange+1,2*tmp_LOSrange+1);

      if (null != threats && 2<=tmp.Count) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
	  }
	  if (null != sights_to_see && 2<=tmp.Count) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
	  }

      // weakly prefer not to jump
      if (2 <= tmp.Count)  tmp = DecideMove_NoJump(tmp);
	  var secondary = new List<ActorAction>();
	  while(0<tmp.Count) {
	    int i = RogueForm.Game.Rules.Roll(0, tmp.Count);
		ActorAction ret = (legal_steps.ContainsKey(tmp[i]) ? legal_steps[tmp[i]] : null);
        if (!ret?.IsLegal() ?? true) {    // not really an option
		  tmp.RemoveAt(i);
          continue;
        }
        if (ret is ActionUseExit use_exit && string.IsNullOrEmpty(use_exit.Exit.ReasonIsBlocked(m_Actor))) {
		  tmp.RemoveAt(i);
          continue;
        };
        if (ret is ActionShove shove && shove.Target.Controller is ObjectiveAI ai) {
           Dictionary<Point, int> ok_dests = ai.MovePlanIf(shove.Target.Location.Position);
           if (Rules.IsAdjacent(shove.To,m_Actor.Location.Position)) {
             // non-moving shove...would rather not spend the stamina if there is a better option
             if (null != ok_dests  && ok_dests.ContainsKey(shove.To)) secondary.Add(ret); // shove is to a wanted destination
       		 tmp.RemoveAt(i);
             continue;
           }
           if (   null == ok_dests // shove is rude
               || !ok_dests.ContainsKey(shove.To)) // shove is not to a wanted destination
                {
                secondary.Add(ret);
    		    tmp.RemoveAt(i);
                continue;
                }
        }
		return ret;
	  }
      if (0<secondary.Count) return secondary[RogueForm.Game.Rules.Roll(0,secondary.Count)];
	  return null;
	}

    // direct move cost adapter; note reference copy of parameter
    protected ActorAction DecideMove(Dictionary<Point,int> dests)
	{
#if DEBUG
      if (null == dests) throw new ArgumentNullException(nameof(dests));
#endif
      if (0 >= dests.Count) return null;
      int min_cost = dests.Values.Min();
      dests.OnlyIf(val => min_cost>=val);
      ActorAction tmp = DecideMove(dests.Keys);
      if (null != tmp) return tmp;
      return null;
	}

    // src_r2 is the desired destination list
    // src are legal steps
    protected ActorAction DecideMove(IEnumerable<Point> src, IEnumerable<Point> src_r2, List<Percept> enemies, List<Percept> friends)
	{
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
      if (null == src_r2) throw new ArgumentNullException(nameof(src_r2));
#endif
	  List<Point> tmp = src.ToList();
	  List<Point> tmp2 = src_r2.ToList();

	  // do not get in the way of allies' line of fire
	  if (2 <= tmp2.Count) tmp2 = DecideMove_Avoid(tmp, FriendsLoF());

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  bool want_LOS_heuristics = false;
	  ThreatTracking threats = m_Actor.Threats;
	  if (null != threats) want_LOS_heuristics = true;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  if (null != sights_to_see) want_LOS_heuristics = true;

	  Dictionary<Point,HashSet<Point>> hypothetical_los = ((want_LOS_heuristics && 2 <= tmp.Count) ? new Dictionary<Point,HashSet<Point>>() : null);
      HashSet<Point> new_los = new HashSet<Point>();
	  if (null != hypothetical_los) {
	    // only need points newly in FOV that aren't currently
	    foreach(Point pt in tmp2) {
	      hypothetical_los[pt] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt)).Except(FOV));
          new_los.UnionWith(hypothetical_los[pt]);
	    }
	  }
      // only need to check if new locations seen
      if (0 >= new_los.Count) {
        threats = null;
        sights_to_see = null;
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position.X - tmp_LOSrange, m_Actor.Location.Position.Y - tmp_LOSrange, 2*tmp_LOSrange+1,2*tmp_LOSrange+1);

	  if (null != threats && 2<=tmp2.Count) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
	  }
	  if (null != sights_to_see && 2<=tmp2.Count) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
	  }

      // weakly prefer not to jump
      if (2 <= tmp2.Count)  tmp2 = DecideMove_NoJump(tmp2);

      // filter down intermediate destinations
      IEnumerable<Point> tmp3 = tmp.Where(pt => tmp2.Select(pt2 => Rules.IsAdjacent(pt,pt2)).Any());
      if (!tmp3.Any()) return null;
      tmp = tmp3.ToList();

	  while(0<tmp.Count) {
	    int i = RogueForm.Game.Rules.Roll(0, tmp.Count);
		ActorAction ret = Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, tmp[i]));
		if (null != ret && ret.IsLegal() && ret is ActionMoveStep) {
          RunIfPossible();
          return ret;
        }
		tmp.RemoveAt(i);
	  }
	  return null;
	}

    protected List<Point> FindRetreat(Dictionary<Point,int> damage_field, IEnumerable<Point> legal_steps)
    {
#if DEBUG
      if (null == damage_field) throw new ArgumentNullException(nameof(damage_field));
      if (null == legal_steps) throw new ArgumentNullException(nameof(legal_steps));
      if (!damage_field.ContainsKey(m_Actor.Location.Position)) throw new InvalidOperationException("!damage_field.ContainsKey(m_Actor.Location.Position)");
#endif
      IEnumerable<Point> tmp_point = legal_steps.Where(pt=>!damage_field.ContainsKey(pt));
      if (tmp_point.Any()) return tmp_point.ToList();
      tmp_point = legal_steps.Where(p=> damage_field[p] < damage_field[m_Actor.Location.Position]);
      return (tmp_point.Any() ? tmp_point.ToList() : null);
    }

    protected List<Point> FindRunRetreat(Dictionary<Point,int> damage_field, IEnumerable<Point> legal_steps)
    {
#if DEBUG
      if (null == damage_field) throw new ArgumentNullException(nameof(damage_field));
      if (null == legal_steps) throw new ArgumentNullException(nameof(legal_steps));
      if (!damage_field.ContainsKey(m_Actor.Location.Position)) throw new InvalidOperationException("!damage_field.ContainsKey(m_Actor.Location.Position)");
#endif
      HashSet<Point> ret = new HashSet<Point>(Enumerable.Range(0, 16).Select(i => m_Actor.Location.Position.RadarSweep(2, i)).Where(pt => m_Actor.Location.Map.IsWalkableFor(pt, m_Actor)));
      ret.RemoveWhere(pt => !legal_steps.Select(pt2 => Rules.IsAdjacent(pt,pt2)).Any());
      IEnumerable<Point> tmp_point = ret.Where(pt=>!damage_field.ContainsKey(pt));
      if (tmp_point.Any()) return tmp_point.ToList();
      tmp_point = ret.Where(pt=> damage_field[pt] < damage_field[m_Actor.Location.Position]);
      return (tmp_point.Any() ? tmp_point.ToList() : null);
    }

    protected void AvoidBeingCornered(List<Point> retreat)
    {
      if (2 > (retreat?.Count ?? 0)) return;

      HashSet<Point> cornered = new HashSet<Point>(retreat);
      foreach(Point pt in Enumerable.Range(0,16).Select(i=>m_Actor.Location.Position.RadarSweep(2,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count<retreat.Count) retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    protected void AvoidBeingRunCornered(List<Point> run_retreat)
    {
      if (2 > (run_retreat?.Count ?? 0)) return;

      HashSet<Point> cornered = new HashSet<Point>(run_retreat);
      foreach(Point pt in Enumerable.Range(0,24).Select(i=>m_Actor.Location.Position.RadarSweep(3,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count<run_retreat.Count) run_retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    protected virtual ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other == null || other.IsDead) return null;
      int num = Rules.GridDistance(m_Actor.Location, other.Location);
      if (CanSee(other.Location) && num <= maxDist) return new ActionWait(m_Actor);
      if (other.Location.Map != m_Actor.Location.Map) {
        Exit exitAt = m_Actor.Location.Exit;
        if (exitAt != null && exitAt.ToMap == other.Location.Map && m_Actor.CanUseExit(m_Actor.Location.Position))
          return BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      ActorAction actorAction = BehaviorIntelligentBumpToward(other.Location);
      if (actorAction == null || !actorAction.IsLegal()) return null;
      if (other.IsRunning) RunIfPossible();
      return actorAction;
    }

    protected ActorAction BehaviorTrackScent(List<Percept> scents)
    {
      if (0 >= (scents?.Count ?? 0)) return null;
      Percept percept = FilterStrongestScent(scents);
      if (!(m_Actor.Location == percept.Location))
        return BehaviorIntelligentBumpToward(percept.Location);
      if (m_Actor.Location.Map.HasExitAt(m_Actor.Location.Position) && m_Actor.Model.Abilities.AI_CanUseAIExits)
        return BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      return null;
    }

    protected virtual ActorAction BehaviorChargeEnemy(Percept target)
    {
      Actor actor = target.Percepted as Actor;
      ActorAction tmpAction = BehaviorMeleeAttack(actor);
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location))
          return new ActionWait(m_Actor);
        tmpAction = BehaviorHeadFor(target.Location);
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
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, string[] emotes)
    {
      ActorCourage courage = ActorCourage.CAUTIOUS;
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
          game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[0], (object) enemy.Name));
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
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(50))
            game.DoEmote(m_Actor, emotes[1]);
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
      tmpAction = BehaviorChargeEnemy(target);
      if (null != tmpAction) {
        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) emotes[2], (object) enemy.Name));
        return tmpAction;
      }
      return null;
    }

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
        if (exploration.HasExplored(loc)) return float.NaN;
        Map map = loc.Map;
        Point position = loc.Position;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position))) num += 1000;
        /* if (!exploration.HasExplored(loc)) */ num += 500;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) num += 100;
        if (null != map.GetActivatedTrapAt(position)) num += -50;
        if (map.IsInsideAtExt(position)) {
          if (map.LocalTime.IsNight) num += 50;
        }
        else if (!map.LocalTime.IsNight) num += 50;
        if (dir == prevDirection) num += 25;
        return (float) (num + RogueForm.Game.Rules.Roll(0, 10));
      }, (a, b) => a > b);
      if (choiceEval != null) return new ActionBump(m_Actor, choiceEval.Choice);
      return null;
    }

    protected ActorAction BehaviorUseExit(BaseAI.UseExitFlags useFlags)
    {
      Exit exitAt = m_Actor.Location.Exit;
      if (!exitAt?.IsAnAIExit ?? true) return null;
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
      return BehaviorStupidBumpToward(percept.Location);
    }

    protected ActorAction BehaviorGoEatCorpse(List<Percept> percepts)
    {
	  if (!Session.Get.HasCorpses) return null;
      if (!m_Actor.CanEatCorpse) return null;
      if (m_Actor.Model.Abilities.IsUndead && m_Actor.HitPoints >= m_Actor.MaxHPs) return null;
	  List<Percept> corpsesPercepts = percepts.FilterT<List<Corpse>>();
	  if (null == corpsesPercepts) return null;
      Percept percept = FilterNearest(corpsesPercepts);
	  if (m_Actor.Location.Position==percept.Location.Position) {
        return new ActionEatCorpse(m_Actor, (percept.Percepted as List<Corpse>)[0]);
	  }
      return BehaviorHeadFor(percept.Location);
    }

    protected void RunIfPossible()
    {
      if (!m_Actor.CanRun()) return;
      m_Actor.IsRunning = true;
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
      if (null == listOfChoices ||  0 >= listOfChoices.Count()) return null;

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
        if (choiceEvalDict.ContainsKey(f)) {
          choiceEvalDict[f].Add(tmp2);
        } else {
          choiceEvalDict[f] = new List<ChoiceEval<_T_>>{ tmp2 };
        }
      }

      if (!choiceEvalDict.TryGetValue(num, out List<ChoiceEval<_T_>> ret_from)) return null;
      if (1 == ret_from.Count) return ret_from[0];
      return ret_from[RogueForm.Game.Rules.Roll(0, ret_from.Count)];
    }

    static protected ChoiceEval<_T_> Choose<_T_>(IEnumerable<_T_> listOfChoices, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
#if DEBUG
      if (null == evalChoiceFn) throw new ArgumentNullException(nameof(evalChoiceFn));
      if (null == isBetterEvalThanFn) throw new ArgumentNullException(nameof(isBetterEvalThanFn));
#endif
      if (null == listOfChoices ||  0 >= listOfChoices.Count()) return null;

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
      return candidates[RogueForm.Game.Rules.Roll(0, candidates.Count)];
    }

    // isBetterThanEvalFn will never see NaN
    static protected ChoiceEval<_DATA_> ChooseExtended<_T_, _DATA_>(IEnumerable<_T_> listOfChoices, Func<_T_, _DATA_> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
#if DEBUG
      if (null == isChoiceValidFn) throw new ArgumentNullException(nameof(isChoiceValidFn));
      if (null == evalChoiceFn) throw new ArgumentNullException(nameof(evalChoiceFn));
      if (null == isBetterEvalThanFn) throw new ArgumentNullException(nameof(isBetterEvalThanFn));
#endif
      if (null == listOfChoices || 0 >= listOfChoices.Count()) return null;

      Dictionary<float, List<ChoiceEval<_DATA_>>> choiceEvalDict = new Dictionary<float, List<ChoiceEval<_DATA_>>>();

      float num = float.NaN;
      foreach(_T_ tmp in listOfChoices) {
        _DATA_ choice = isChoiceValidFn(tmp);
        if (null == choice) continue;
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;
        if (float.IsNaN(num)) {
          num = f;
        } else if (isBetterEvalThanFn(f, num)) {
          num = f;
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;

        ChoiceEval< _DATA_ > tmp2 = new ChoiceEval<_DATA_>(choice, f);
        if (choiceEvalDict.ContainsKey(f)) {
          choiceEvalDict[f].Add(tmp2);
        } else {
          choiceEvalDict[f] = new List<ChoiceEval<_DATA_>>{ tmp2 };
        }
      }

      if (!choiceEvalDict.TryGetValue(num, out List<ChoiceEval<_DATA_>> ret_from)) return null;
      if (1 == ret_from.Count) return ret_from[0];
      return ret_from[RogueForm.Game.Rules.Roll(0, ret_from.Count)];
    }

    static protected bool IsValidFleeingAction(ActorAction a)
    {
      if (null == a) return false;
      if (!(a is ActionMoveStep) && !(a is ActionOpenDoor))
        return a is ActionSwitchPlace;
      return true;
    }

    protected bool isValidWanderAction(ActorAction a)
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
      OrderableAI downcast = this as OrderableAI;
      if (null==downcast) return false;
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

    protected static Actor FindWeakerInMelee(Actor a, Actor b)
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
