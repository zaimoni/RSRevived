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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;
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
    private Location m_prevLocation;

    public BaseAI()
    {
    }

    protected Location PrevLocation
    {
      get
      {
        return m_prevLocation;
      }
    }

    public override ActorAction GetAction(RogueGame game)
    {
      Contract.Ensures(null != Contract.Result<ActorAction>());
      Contract.Ensures(Contract.Result<ActorAction>().IsLegal());
      if (m_prevLocation.Map == null) m_prevLocation = m_Actor.Location;
      m_Actor.TargetActor = null;
      ActorAction actorAction = SelectAction(game);
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
      return percepts.Filter(p => p.Location.Map == map);
    }

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
      Point a_pos = m_Actor.Location.Position;
      return percepts.Minimize(p=>Rules.StdDistance(a_pos, p.Location.Position));
    }

    protected Percept FilterStrongestScent(List<Percept> scents)
    {
      if (scents == null || scents.Count == 0)
        return (Percept) null;
      Percept percept = (Percept) null;
      SmellSensor.AIScent aiScent1 = (SmellSensor.AIScent) null;
      foreach (Percept scent in scents)
      {
        SmellSensor.AIScent aiScent2 = scent.Percepted as SmellSensor.AIScent;
        if (aiScent2 == null)
          throw new InvalidOperationException("percept not an aiScent");
        if (percept == null || aiScent2.Strength > aiScent1.Strength)
        {
          aiScent1 = aiScent2;
          percept = scent;
        }
      }
      return percept;
    }

    // dead function?
    protected List<Percept> FilterActorsModel(List<Percept> percepts, ActorModel model)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = null;
      foreach (Percept percept in percepts) {
        Actor actor = percept.Percepted as Actor;
        if (null != actor && actor.Model == model) {
          if (null == perceptList) perceptList = new List<Percept>(percepts.Count);
          perceptList.Add(percept);
        }
      }
      return perceptList;
    }

    protected List<Percept> FilterFireTargets(List<Percept> percepts)
    {
      return percepts.FilterT<Actor>(target => m_Actor.CanFireAt(target));
    }

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

    // firearms use grid i.e. L-infinity distance
    protected List<Percept_<_T_>> SortByGridDistance<_T_>(List<Percept_<_T_>> percepts) where _T_:class
    {
      if (null == percepts || 0 == percepts.Count) return null;
      if (1==percepts.Count) return percepts;

      List<Percept_<_T_>> perceptList = new List<Percept_<_T_>>(percepts);
      Point from = m_Actor.Location.Position;
      Dictionary<Percept_<_T_>, int> dict = new Dictionary<Percept_<_T_>, int>(perceptList.Count);
      foreach(Percept_<_T_> p in perceptList) {
        dict.Add(p,Rules.GridDistance(p.Location.Position, from));
      }
      perceptList.Sort((pA, pB) => dict[pA].CompareTo(dict[pB]));
      return perceptList;
    }

    protected List<Percept> SortByDate(List<Percept> percepts)
    {
      if (null == percepts || 0 == percepts.Count) return null;
      List<Percept> perceptList = new List<Percept>(percepts);
      perceptList.Sort((pA, pB) => pB.Turn.CompareTo(pA.Turn));
      return perceptList;
    }

    // policy change for behaviors: unless the action from a behavior is being used to decide whether to commit to the behavior,
    // a behavior should handle all free actions itself and return only non-free actions.

    protected ActorAction BehaviorWander(Predicate<Location> goodWanderLocFn=null)
    {
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        if (goodWanderLocFn != null && !goodWanderLocFn(location)) return false;
        return isValidWanderAction(Rules.IsBumpableFor(m_Actor, location));
      }), (Func<Direction, float>) (dir =>
      {
        int num = RogueForm.Game.Rules.Roll(0, 666);
        if (m_Actor.Model.Abilities.IsIntelligent && null != m_Actor.Location.Map.GetActivatedTrapAt((m_Actor.Location + dir).Position))
          num -= 1000;
        return (float) num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      return (choiceEval != null ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    protected ActorAction BehaviorBumpToward(Point goal, Func<Point, Point, float> distanceFn)
    {
      BaseAI.ChoiceEval<ActorAction> choiceEval = ChooseExtended(Direction.COMPASS_LIST, (Func<Direction, ActorAction>) (dir =>
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
      }), (Func<Direction, float>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        if (distanceFn != null) return distanceFn(location.Position, goal);
        return (float)Rules.StdDistance(location.Position, goal);
      }), (Func<float, float, bool>) ((a, b) =>
      {
        if (!float.IsNaN(a)) return (double) a < (double) b;
        return false;
      }));
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
          int trapsMaxDamage = ComputeTrapsMaxDamage(m_Actor.Location.Map, ptA);
          if (trapsMaxDamage > 0) {
            if (trapsMaxDamage >= m_Actor.HitPoints) return float.NaN;
            num += 0.42f;
          }
        }
        return num;
      }));
    }

    protected ActorAction BehaviorHeadFor(Point goal)
    {
      if (m_Actor.Model.Abilities.IsIntelligent) return BehaviorIntelligentBumpToward(goal);
      return BehaviorStupidBumpToward(goal);
    }

    // A number of the melee enemy targeting sequences not only work on grid distance,
    // they need to return a coordinated action/target pair.
    protected ActorAction TargetGridMelee(List<Percept> perceptList)
    {
      if (null == perceptList) return null; // inefficient, but reduces lines of code elsewhere
      ActorAction ret = null;
      int num1 = int.MaxValue;
      foreach (Percept percept in perceptList) {
        int num2 = Rules.GridDistance(m_Actor.Location.Position, percept.Location.Position);
        if (num2 < num1) {
          ActorAction tmp = BehaviorStupidBumpToward(percept.Location.Position);
          if (null != tmp) {
            num1 = num2;
            ret = tmp;
            m_Actor.Activity = Activity.CHASING;
            m_Actor.TargetActor = percept.Percepted as Actor;
          }
        }
      }
      return ret;
    }

    protected ActorAction BehaviorWalkAwayFrom(RogueGame game, Percept goal)
    {
      return BehaviorWalkAwayFrom(new List<Percept>(1) { goal });
    }

    protected ActorAction BehaviorWalkAwayFrom(List<Percept> goals)
    {
      Actor leader = m_Actor.Leader;
      bool flag = m_Actor.HasLeader && m_Actor.GetEquippedWeapon() is ItemRangedWeapon;
      Actor actor = (flag ? GetNearestTargetFor(m_Actor.Leader) : null);
      bool checkLeaderLoF = actor != null && actor.Location.Map == m_Actor.Location.Map;
      List<Point> leaderLoF = null;
      if (checkLeaderLoF) {
        leaderLoF = new List<Point>(1);
        ItemRangedWeapon itemRangedWeapon = m_Actor.GetEquippedWeapon() as ItemRangedWeapon;
        LOS.CanTraceFireLine(leader.Location, actor.Location.Position, (itemRangedWeapon.Model as ItemRangedWeaponModel).Attack.Range, leaderLoF);
      }
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir => IsValidFleeingAction(Rules.IsBumpableFor(m_Actor, m_Actor.Location + dir))), (Func<Direction, float>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        float num = SafetyFrom(location.Position, goals);
        if (m_Actor.HasLeader) {
          num -= (float)Rules.StdDistance(location.Position, m_Actor.Leader.Location.Position);
          if (checkLeaderLoF && leaderLoF.Contains(location.Position)) --num;
        }
        return num;
      }), (Func<float, float, bool>) ((a, b) => (double) a > (double) b));
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    protected ActionMeleeAttack BehaviorMeleeAttack(Actor target)
    {
      Contract.Requires(null != target);
      return (m_Actor.CanMeleeAttack(target) ? new ActionMeleeAttack(m_Actor, target) : null);
    }

    protected ActionRangedAttack BehaviorRangedAttack(Actor target)
    {
      Contract.Requires(null != target);
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

    protected int ComputeTrapsMaxDamage(Map map, Point pos)
    {
      Inventory itemsAt = map.GetItemsAt(pos);
      if (itemsAt == null) return 0;
      int num = 0;
      foreach (Item obj in itemsAt.Items) {
        ItemTrap itemTrap = obj as ItemTrap;
        if (itemTrap != null) num += itemTrap.TrapModel.Damage;
      }
      return num;
    }

    protected ActorAction BehaviorBuildTrap(RogueGame game)
    {
      ItemTrap itemTrap = m_Actor.Inventory.GetFirst<ItemTrap>();
      if (itemTrap == null) return null;
      string reason;
      if (!IsGoodTrapSpot(m_Actor.Location.Map, m_Actor.Location.Position, out reason)) return null;
      if (!itemTrap.IsActivated && !itemTrap.TrapModel.ActivatesWhenDropped)
        return new ActionUseItem(m_Actor, itemTrap);
      game.DoEmote(m_Actor, string.Format("{0} {1}!", (object) reason, (object) itemTrap.AName));
      return new ActionDropItem(m_Actor, itemTrap);
    }

    protected bool IsGoodTrapSpot(Map map, Point pos, out string reason)
    {
      reason = "";
      bool isInside = map.GetTileAt(pos).IsInside;
      if (!isInside && map.GetCorpsesAt(pos) != null) reason = "that corpse will serve as a bait for";
      else if (m_prevLocation.Map.GetTileAt(m_prevLocation.Position).IsInside != isInside) reason = "protecting the building with";
      else {
        MapObject mapObjectAt = map.GetMapObjectAt(pos);
        if (mapObjectAt != null && mapObjectAt is DoorWindow) reason = "protecting the doorway with";
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

    protected ActorAction BehaviorAttackBarricade(RogueGame game)
    {
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        DoorWindow doorWindow = map.GetMapObjectAt(pt) as DoorWindow;
        return ((null != doorWindow) ? doorWindow.IsBarricaded : false);
      }));
      if (pointList == null) return null;
      DoorWindow doorWindow1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]) as DoorWindow;
      return (m_Actor.CanBreak(doorWindow1) ? new ActionBreak(m_Actor, doorWindow1) : null);
    }

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

    protected ActionPush BehaviorPushNonWalkableObject(RogueGame game)
    {
      if (!m_Actor.AbleToPush) return null;
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (mapObjectAt == null || mapObjectAt.IsWalkable) return false;
        return m_Actor.CanPush(mapObjectAt);
      }));
      if (pointList == null) return null;
      MapObject mapObjectAt1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      ActionPush tmp = new ActionPush(m_Actor, mapObjectAt1, game.Rules.RollDirection());
      return (tmp.IsLegal() ? tmp : null);
    }

    protected ActionPush BehaviorPushNonWalkableObjectForFood(RogueGame game)
    {
      if (!m_Actor.AbleToPush) return null;
      Map map = m_Actor.Location.Map;
      List<Point> pointList = map.FilterAdjacentInMap(m_Actor.Location.Position, (Predicate<Point>) (pt =>
      {
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        // Wrecked cars are very tiring to push, and are jumpable so they don't need to be pushed.
        if (mapObjectAt == null || mapObjectAt.IsWalkable || mapObjectAt.IsJumpable) return false;
        return m_Actor.CanPush(mapObjectAt);
      }));
      if (pointList == null) return null;
      MapObject mapObjectAt1 = map.GetMapObjectAt(pointList[game.Rules.Roll(0, pointList.Count)]);
      ActionPush tmp = new ActionPush(m_Actor, mapObjectAt1, game.Rules.RollDirection());
      return (tmp.IsLegal() ? tmp : null);
    }

	protected HashSet<Point> FriendsLoF(List<Percept> enemies, List<Percept> friends)
	{
	  if (null == enemies) return null;
	  if (null == friends) return null;
	  IEnumerable<Actor> friends2 = friends.Select(p=>p.Percepted as Actor).Where(a=>HasEquipedRangedWeapon(a));
	  if (!friends2.Any()) return null;
	  HashSet<Point> tmp = new HashSet<Point>();
	  foreach(Actor f in friends2) {
	    foreach(Actor e in enemies.Select(p => p.Percepted as Actor)) {
		  if (!f.IsEnemyOf(e)) continue;
		  if (f.CurrentRangedAttack.Range<Rules.GridDistance(f.Location.Position,e.Location.Position)) continue;
		  List<Point> line = new List<Point>();
	      LOS.CanTraceFireLine(f.Location, e.Location.Position, f.CurrentRangedAttack.Range, line);
		  foreach(Point pt in line) {
		    tmp.Add(pt);
		  }
		}
	  }
	  return (0<tmp.Count ? tmp : null);
	}

    protected List<Point> DecideMove_WaryOfTraps(List<Point> src)
    {
	  Dictionary<Point,int> trap_damage_field = new Dictionary<Point,int>();
	  foreach (Point pt in src) {
		trap_damage_field[pt] = ComputeTrapsMaxDamage(m_Actor.Location.Map, pt);
	  }
	  IEnumerable<Point> safe = src.Where(pt => 0>=trap_damage_field[pt]);
	  int new_dest = safe.Count();
      if (0 == new_dest) {
		safe = src.Where(pt => m_Actor.HitPoints>trap_damage_field[pt]);
		new_dest = safe.Count();
      }
      return ((0 < new_dest && new_dest < src.Count) ? safe.ToList() : src);
    }

    private List<Point> DecideMove_Avoid(List<Point> src, IEnumerable<Point> avoid)
    {
      if (null == avoid) return src;
      IEnumerable<Point> ok = src.Except(avoid);
	  int new_dest = ok.Count();
      return ((0 < new_dest && new_dest < src.Count) ? ok.ToList() : src);
    }

    private List<Point> DecideMove_NoJump(List<Point> src)
    {
      IEnumerable<Point> no_jump = src.Where(pt=> {
        MapObject tmp2 = m_Actor.Location.Map.GetMapObjectAt(pt);
        return null==tmp2 || !tmp2.IsJumpable;
      });
	  int new_dest = no_jump.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_jump.ToList() : src);
    }

    private List<Point> DecideMove_maximize_visibility(List<Point> dests, HashSet<Point> tainted, HashSet<Point> new_los, Dictionary<Point,HashSet<Point>> hypothetical_los) {
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

    protected ActorAction DecideMove(IEnumerable<Point> src, List<Percept> enemies=null, List<Percept> friends=null)
	{
	  Contract.Requires(null != src);
	  List<Point> tmp = src.ToList();

	  // do not get in the way of allies' line of fire
	  if (2 <= tmp.Count) tmp = DecideMove_Avoid(tmp, FriendsLoF(enemies, friends));

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

	  if (null != threats && 2<=tmp.Count) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map), new_los, hypothetical_los);
	  }
	  if (null != sights_to_see && 2<=tmp.Count) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map);
        if (null!=inspect) tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
	  }

      // weakly prefer not to jump
      if (2 <= tmp.Count)  tmp = DecideMove_NoJump(tmp);
	  while(0<tmp.Count) {
	    int i = RogueForm.Game.Rules.Roll(0, tmp.Count);
		ActorAction ret = Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, tmp[i]));
		if (null != ret && ret.IsLegal()) return ret;
		tmp.RemoveAt(i);
	  }
	  return null;
	}

    // src_r2 is the desired destination list
    // src are legal steps
    protected ActorAction DecideMove(IEnumerable<Point> src, IEnumerable<Point> src_r2, List<Percept> enemies, List<Percept> friends)
	{
	  Contract.Requires(null != src);
	  Contract.Requires(null != src_r2);
	  List<Point> tmp = src.ToList();
	  List<Point> tmp2 = src_r2.ToList();

	  // do not get in the way of allies' line of fire
	  if (2 <= tmp2.Count) tmp2 = DecideMove_Avoid(tmp, FriendsLoF(enemies, friends));

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

	  if (null != threats && 2<=tmp2.Count) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map), new_los, hypothetical_los);
	  }
	  if (null != sights_to_see && 2<=tmp2.Count) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map);
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
      Contract.Requires(null != damage_field);
      Contract.Requires(null != legal_steps);
#if DEBUG
      Contract.Requires(damage_field.ContainsKey(m_Actor.Location.Position));
#endif
      IEnumerable<Point> tmp_point = legal_steps.Where(pt=>!damage_field.ContainsKey(pt));
      if (tmp_point.Any()) return tmp_point.ToList();
      tmp_point = legal_steps.Where(p=> damage_field[p] < damage_field[m_Actor.Location.Position]);
      return (tmp_point.Any() ? tmp_point.ToList() : null);
    }

    protected List<Point> FindRunRetreat(Dictionary<Point,int> damage_field, IEnumerable<Point> legal_steps)
    {
      Contract.Requires(null != damage_field);
      Contract.Requires(null != legal_steps);
#if DEBUG
      Contract.Requires(damage_field.ContainsKey(m_Actor.Location.Position));
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
      if (null != retreat || 2 > retreat.Count()) return;

      HashSet<Point> cornered = new HashSet<Point>(retreat);
      foreach(Point pt in Enumerable.Range(0,16).Select(i=>m_Actor.Location.Position.RadarSweep(2,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count<retreat.Count) retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    protected void AvoidBeingRunCornered(List<Point> run_retreat)
    {
      if (null != run_retreat || 2 > run_retreat.Count()) return;

      HashSet<Point> cornered = new HashSet<Point>(run_retreat);
      foreach(Point pt in Enumerable.Range(0,24).Select(i=>m_Actor.Location.Position.RadarSweep(3,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count<run_retreat.Count) run_retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    protected virtual ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other == null || other.IsDead) return null;
      int num = Rules.GridDistance(m_Actor.Location.Position, other.Location.Position);
      if (FOV.Contains(other.Location.Position) && num <= maxDist) return new ActionWait(m_Actor);
      if (other.Location.Map != m_Actor.Location.Map) {
        Exit exitAt = m_Actor.Location.Exit;
        if (exitAt != null && exitAt.ToMap == other.Location.Map && m_Actor.CanUseExit(m_Actor.Location.Position))
          return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      ActorAction actorAction = BehaviorIntelligentBumpToward(other.Location.Position);
      if (actorAction == null || !actorAction.IsLegal()) return null;
      if (other.IsRunning) RunIfPossible();
      return actorAction;
    }

    protected ActorAction BehaviorHangAroundActor(RogueGame game, Actor other, Point otherPosition, int minDist, int maxDist)
    {
      if (other == null || other.IsDead) return null;
      int num = 0;
      Point p;
      do {
        p = otherPosition;
        p.X += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        p.Y += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        m_Actor.Location.Map.TrimToBounds(ref p);
      }
      while (Rules.GridDistance(p, otherPosition) < minDist && ++num < 100);
      ActorAction a = BehaviorIntelligentBumpToward(p);
      if (a == null || !IsValidMoveTowardGoalAction(a) || !a.IsLegal()) return null;
      if (other.IsRunning) RunIfPossible();
      return a;
    }

    protected ActorAction BehaviorTrackScent(List<Percept> scents)
    {
      if (scents == null || scents.Count == 0) return null;
      Percept percept = FilterStrongestScent(scents);
      Map map = m_Actor.Location.Map;
      if (!(m_Actor.Location.Position == percept.Location.Position))
        return BehaviorIntelligentBumpToward(percept.Location.Position);
      if (map.HasExitAt(m_Actor.Location.Position) && m_Actor.Model.Abilities.AI_CanUseAIExits)
        return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      return null;
    }

    protected virtual ActorAction BehaviorChargeEnemy(Percept target)
    {
      Actor actor = target.Percepted as Actor;
      ActorAction tmpAction = BehaviorMeleeAttack(actor);
#if DEBUG
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location))
          return new ActionWait(m_Actor);
        tmpAction = BehaviorHeadFor(target.Location.Position);
        if (null == tmpAction) return null;
        if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) RunIfPossible();
        return tmpAction;
      } catch(System.Exception e) {
        throw;
      } finally {
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = actor;
        }
      }
#else
      if (null != tmpAction) return tmpAction;
      if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location))
        return new ActionWait(m_Actor);
      tmpAction = BehaviorHeadFor(target.Location.Position);
      if (null == tmpAction) return null;
      if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) RunIfPossible();
      return tmpAction;
#endif
    }

    // Feral dogs use BehaviorFightOrFlee; simplified version of what OrderableAI uses
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, bool hasVisibleLeader, bool isLeaderFighting, string[] emotes)
    {
      ActorCourage courage = ActorCourage.CAUTIOUS;
      Percept target = FilterNearest(enemies);
      bool doRun = false;	// only matters when fleeing
      Actor enemy = target.Percepted as Actor;
      bool decideToFlee;
      if (HasEquipedRangedWeapon(enemy))
        decideToFlee = false;
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
        tmpAction = BehaviorWalkAwayFrom(enemies);
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
        int dist = Rules.GridDistance(m_Actor.Location.Position,target.Location.Position);
        if (m_Actor.WillActAgainBefore(enemy) && 2==dist) {
          // Neither free hit, nor clearly safe to close.  Main options are charge-hit and wait
          // We could also reposition for tactical advantage i.e. ability to retreat
          return new ActionWait(m_Actor);   // default
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

    protected ActorAction BehaviorExplore(RogueGame game, ExplorationData exploration, ActorCourage courage=ActorCourage.CAUTIOUS)
    {
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position.X - m_prevLocation.Position.X, m_Actor.Location.Position.Y - m_prevLocation.Position.Y);
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS_LIST, (Func<Direction, bool>) (dir =>
      {
        Location location = m_Actor.Location + dir;
        if (exploration.HasExplored(location)) return false;
        return IsValidMoveTowardGoalAction(Rules.IsBumpableFor(m_Actor, location));
      }), (Func<Direction, float>) (dir =>
      {
        Location loc = m_Actor.Location + dir;
        Map map = loc.Map;
        Point position = loc.Position;
        if (m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous && ComputeTrapsMaxDamage(map, position) >= m_Actor.HitPoints)
          return float.NaN;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position.X, position.Y))) num += 1000;
        if (!exploration.HasExplored(loc)) num += 500;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) num += 100;
        if (null != map.GetActivatedTrapAt(position)) num += -50;
        if (map.GetTileAt(position.X, position.Y).IsInside) {
          if (map.LocalTime.IsNight) num += 50;
        }
        else if (!map.LocalTime.IsNight) num += 50;
        if (dir == prevDirection) num += 25;
        return (float) (num + game.Rules.Roll(0, 10));
      }), (Func<float, float, bool>) ((a, b) =>
      {
        if (!float.IsNaN(a)) return (double) a > (double) b;
        return false;
      }));
      if (choiceEval != null) return new ActionBump(m_Actor, choiceEval.Choice);
      return null;
    }

    protected ActionCloseDoor BehaviorCloseDoorBehindMe(RogueGame game, Location previousLocation)
    {
      DoorWindow door = previousLocation.Map.GetMapObjectAt(previousLocation.Position) as DoorWindow;
      if (door == null) return null;
      return (m_Actor.CanClose(door) ? new ActionCloseDoor(m_Actor, door) : null);
    }

    protected ActorAction BehaviorUseExit(BaseAI.UseExitFlags useFlags)
    {
      Exit exitAt = m_Actor.Location.Exit;
      if (!exitAt?.IsAnAIExit ?? true) return null;
      if ((useFlags & BaseAI.UseExitFlags.DONT_BACKTRACK) != BaseAI.UseExitFlags.NONE && exitAt.Location == m_prevLocation) return null;
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
      return (m_Actor.CanUseExit(m_Actor.Location.Position) ? new ActionUseExit(m_Actor, m_Actor.Location.Position) : null);
    }

    // belongs with CivilianAI, or possibly OrderableAI but NatGuard may not have access to the crime listings
    protected ActorAction BehaviorEnforceLaw(RogueGame game, List<Percept> percepts)
    {
      if (!m_Actor.Model.Abilities.IsLawEnforcer) return null;
      if (percepts == null) return null;
      List<Percept> percepts1 = percepts.FilterT<Actor>(a => 0< a.MurdersCounter && !m_Actor.IsEnemyOf(a));
      if (null == percepts1) return null;
      Percept percept = FilterNearest(percepts1);
      Actor target = percept.Percepted as Actor;
      if (game.Rules.RollChance(game.Rules.ActorUnsuspicousChance(m_Actor, target))) {
        game.DoEmote(target, string.Format("moves unnoticed by {0}.", (object)m_Actor.Name));
        return null;
      }
      game.DoEmote(m_Actor, string.Format("takes a closer look at {0}.", (object) target.Name));
      int chance = Rules.ActorSpotMurdererChance(m_Actor, target);
      if (!game.Rules.RollChance(chance)) return null;
      game.DoMakeAggression(m_Actor, target);
      m_Actor.TargetActor = target;
      // players are special: they get to react to this first
      return new ActionSay(m_Actor, target, string.Format("HEY! YOU ARE WANTED FOR {0} MURDER{1}!", (object) target.MurdersCounter, target.MurdersCounter > 1 ? (object) "s" : (object) ""), (target.IsPlayer ? RogueGame.Sayflags.IS_IMPORTANT : RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION));
    }

    protected ActorAction BehaviorGoEatFoodOnGround(List<Percept> stacksPercepts)
    {
      if (stacksPercepts == null) return null;
      List<Percept> percepts = stacksPercepts.Filter(p => (p.Percepted as Inventory).Has<ItemFood>());
      if (percepts == null) return null;
      ItemFood firstByType = m_Actor.Location.Items?.GetFirst<ItemFood>();
      if (null != firstByType) return new ActionEatFoodOnGround(m_Actor, firstByType);
      Percept percept = FilterNearest(percepts);
      return BehaviorStupidBumpToward(percept.Location.Position);
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
      return BehaviorHeadFor(percept.Location.Position);
    }

    protected ActorAction BehaviorGoReviveCorpse(RogueGame game, List<Percept> percepts)
    {
	  if (!Session.Get.HasCorpses) return null;
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) == 0) return null;
      if (!m_Actor.HasItemOfModel(game.GameItems.MEDIKIT)) return null;
      List<Percept> corpsePercepts = percepts.FilterT<List<Corpse>>().Filter(p =>
      {
        foreach (Corpse corpse in p.Percepted as List<Corpse>) {
          if (game.Rules.CanActorReviveCorpse(m_Actor, corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return true;
        }
        return false;
      });
      if (null == corpsePercepts) return null;
      Percept percept = FilterNearest(corpsePercepts);
	  if (m_Actor.Location.Position==percept.Location.Position) {
        foreach (Corpse corpse in (percept.Percepted as List<Corpse>)) {
          if (game.Rules.CanActorReviveCorpse(m_Actor, corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return new ActionReviveCorpse(m_Actor, corpse);
        }
	  }
      return BehaviorHeadFor(percept.Location.Position);
    }

    protected void RunIfPossible()
    {
      if (!m_Actor.CanRun()) return;
      m_Actor.IsRunning = true;
    }

    protected void RunIfAdvisable(Point dest)
    {
      if (!m_Actor.CanRun()) return;
      if (m_Actor.WillTireAfterRunning(dest)) return;
      m_Actor.IsRunning = true;
    }

	protected void RunIfReasonable(Point dest)
	{
      if (!m_Actor.CanRun()) return;
      if (m_Actor.WillTireAfterRunning(dest)) return;
	  if (!m_Actor.RunIsFreeMove) {
        m_Actor.IsRunning = true;	// re-setup free move
		return;
	  }
	  // past this point, "reasonable" can vary.  One can either favor accuracy with ranged weapons, or try to move as fast as possible without compromising stance
	  // favoring accuracy would stop here
	}

    /// <summary>
    /// Compute safety from a list of dangers at a given position.
    /// </summary>
    /// <param name="from">position to compute the safety</param>
    /// <param name="dangers">dangers to avoid</param>
    /// <returns>a heuristic value, the higher the better the safety from the dangers</returns>
    protected float SafetyFrom(Point from, List<Percept> dangers)
    {
      Map map = m_Actor.Location.Map;

      // Heuristics:
      // Primary: Get away from dangers.
      // Weighting factors:
      // 1 Avoid getting in corners.
      // 2 Prefer going outside/inside if majority of dangers are inside/outside.
      // 3 If can tire, prefer not jumping.
#region Primary: Get away from dangers.
      float avgDistance = (float) (dangers.Select(p => p.Location.Position).Sum(pt => Rules.GridDistance(from, pt))) / (1 + dangers.Count);
#endregion
#region 1 Avoid getting in corners.
      int countFreeSquares = 0;
      foreach (Direction direction in Direction.COMPASS) {
        Point point = from + direction;
        if (point == m_Actor.Location.Position || map.IsWalkableFor(point, m_Actor))
          ++countFreeSquares;
      }
      float avoidCornerBonus = countFreeSquares * 0.1f;
#endregion
#region 2 Prefer going outside/inside if majority of dangers are inside/outside.
      bool isFromInside = map.GetTileAt(from).IsInside;
      int majorityDangersInside = 0;
      foreach (Percept danger in dangers) {
        if (map.GetTileAt(danger.Location.Position).IsInside)
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
        MapObject mapObjectAt = map.GetMapObjectAt(from);
        if (mapObjectAt != null && mapObjectAt.IsJumpable) jumpPenalty = 0.1f;
      }
#endregion
      float heuristicFactorBonus = 1f + avoidCornerBonus + inOutBonus - jumpPenalty;
      return avgDistance * heuristicFactorBonus;
    }

    protected BaseAI.ChoiceEval<_T_> Choose<_T_>(List<_T_> listOfChoices, Func<_T_, bool> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (listOfChoices.Count == 0) return null;
      bool flag = false;
      float num = 0.0f;
      List<BaseAI.ChoiceEval<_T_>> choiceEvalList1 = new List<BaseAI.ChoiceEval<_T_>>(listOfChoices.Count);
      foreach(_T_ tmp in listOfChoices) {
        if (isChoiceValidFn(tmp)) {
          float f = evalChoiceFn(tmp);
          if (float.IsNaN(f)) continue;
          choiceEvalList1.Add(new BaseAI.ChoiceEval<_T_>(tmp, f));
          if (!flag || isBetterEvalThanFn(f, num)) {
            flag = true;
            num = f;
          }
        }
      }
      if (choiceEvalList1.Count == 0) return null;
      if (choiceEvalList1.Count == 1) return choiceEvalList1[0];
      List<BaseAI.ChoiceEval<_T_>> choiceEvalList2 = new List<BaseAI.ChoiceEval<_T_>>(choiceEvalList1.Count);
      foreach(BaseAI.ChoiceEval<_T_> tmp in choiceEvalList1) {
        if (tmp.Value == num) choiceEvalList2.Add(tmp);
      }
      return choiceEvalList2[RogueForm.Game.Rules.Roll(0, choiceEvalList2.Count)];
    }

    protected BaseAI.ChoiceEval<_DATA_> ChooseExtended<_T_, _DATA_>(List<_T_> listOfChoices, Func<_T_, _DATA_> isChoiceValidFn, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (listOfChoices.Count == 0) return null;
      bool flag = false;
      float num = 0.0f;
      List<BaseAI.ChoiceEval<_DATA_>> choiceEvalList1 = new List<BaseAI.ChoiceEval<_DATA_>>(listOfChoices.Count);
      foreach(_T_ tmp in listOfChoices)
      {
        _DATA_ choice = isChoiceValidFn(tmp);
        if (null == choice) continue;
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;
        choiceEvalList1.Add(new BaseAI.ChoiceEval<_DATA_>(choice, f));
        if (!flag || isBetterEvalThanFn(f, num)) {
          flag = true;
          num = f;
        }
      }
      if (choiceEvalList1.Count == 0) return null;
      if (choiceEvalList1.Count == 1) return choiceEvalList1[0];
      List<BaseAI.ChoiceEval<_DATA_>> choiceEvalList2 = new List<BaseAI.ChoiceEval<_DATA_>>(choiceEvalList1.Count);
      foreach(BaseAI.ChoiceEval<_DATA_> tmp in choiceEvalList1) {
        if (tmp.Value == num) choiceEvalList2.Add(tmp);
      }
      if (choiceEvalList2.Count == 0) return null;
      return choiceEvalList2[RogueForm.Game.Rules.Roll(0, choiceEvalList2.Count)];
    }

    protected bool IsValidFleeingAction(ActorAction a)
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
        return IsInterestingItem(it);
      }
      OrderableAI downcast = this as OrderableAI;
      if (null==downcast) return false;
      if (a is ActionChat) {
        return downcast.Directives.CanTrade || (a as ActionChat).Target == m_Actor.Leader;
      }
      return false;
    }

    protected bool IsValidMoveTowardGoalAction(ActorAction a)
    {
      if (a != null && !(a is ActionChat) && (!(a is ActionGetFromContainer) && !(a is ActionSwitchPowerGenerator)))
        return !(a is ActionRechargeItemBattery);
      return false;
    }

    protected bool IsSoldier(Actor actor)
    {
      if (actor != null) return actor.Controller is SoldierAI;
      return false;
    }

    protected bool IsOccupiedByOther(Map map, Point position)
    {
      Actor actorAt = map.GetActorAt(position);
      if (actorAt != null) return actorAt != m_Actor;
      return false;
    }

    protected bool HasEquipedRangedWeapon(Actor actor)
    {
      return actor.GetEquippedWeapon() is ItemRangedWeapon;
    }

    protected bool WantToEvadeMelee(Actor actor, ActorCourage courage, Actor target)
    {
//    if (WillTireAfterAttack(actor)) return true;  // post-process this, handling this here is awful for rats
      if (actor.Speed > target.Speed) {
        if (actor.WillActAgainBefore(target)) return false; // caller must handle distance 2 correctly.
        if (target.TargetActor == actor) return true;
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
	  if (Rules.WillOtherActTwiceBefore(a,b)) b_dam *= 2;
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

    protected bool HasSpeedAdvantage(Actor actor, Actor target)
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
      Map map = actor.Location.Map;
      Actor actor1 = null;
      int num1 = int.MaxValue;
      foreach (Actor actor2 in map.Actors) {
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

    public static bool IsZoneChange(Map map, Point pos)
    {
      List<Zone> zonesHere = map.GetZonesAt(pos.X, pos.Y);
      if (zonesHere == null) return false;
      return map.HasAnyAdjacentInMap(pos, (Predicate<Point>) (adj =>
      {
        List<Zone> zonesAt = map.GetZonesAt(adj.X, adj.Y);
        if (zonesAt == null) return false;
        if (zonesHere == null) return true;
        foreach (Zone zone in zonesAt) {
          if (!zonesHere.Contains(zone)) return true;
        }
        return false;
      }));
    }

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
