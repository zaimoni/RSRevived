// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_NAVIGATE
// #define TRACE_GOALS
#define INTEGRITY_CHECK_ITEM_RETURN_CODE
// #define TIME_TURNS

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
#if TIME_TURNS
using System.Diagnostics;
#endif
using System.Linq;
using Zaimoni.Data;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
using Rectangle = Zaimoni.Data.Box2D_int;
#else
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
#endif

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
    // OrderableAI subsumes all close-to-normal livings.  The Police Faction AI needs the following capabilities
    // * sweep district for threat
    // * explore
    // * pick up Y at X
    // * do Y at X (e.g., turn on generators)
    // * drop Y at X
    // * do Y at X at time T (e.g., sleep; be in position for invasion)

    // examples for actor objective stack, which is checked only when no enemies in sight:
    // Be at Location at time t
    // Have Location in view at time t (probably best to delegate this)
    // get item type at time t
    // sleep (on bed) at time t with food value >= and sanity value >=
    // * reversed-sense of these inequalities may also be of use
    // Engine.AI.Percept looks related, but reverse-engineering the required action is problematic
    [Serializable]
    internal abstract class Objective
    {
      protected int turn;   // turn count of WorldTime .. will need a more complex representation at some point.
      protected readonly Actor m_Actor;   // owning actor is likely important
      protected bool _isExpired;

      public int TurnCounter { get { return turn; } }
      public bool IsExpired { get { return _isExpired; } }
      public Actor Actor { get { return m_Actor; } }

      protected Objective(int t0, Actor who)
      {
#if DEBUG
         if (null == who) throw new ArgumentNullException(nameof(who));
#endif
         turn = t0;
         m_Actor = who;
      }

      // convention: return true to take action; ret null if self-cancellation indicated.
      public abstract bool UrgentAction(out ActorAction ret);

      public virtual List<Objective> Subobjectives() { return null; }
    }

    // workaround for breaking action loops
    [Serializable]
    internal class Goal_NextAction : Objective
    {
      public readonly ActorAction Intent;

      public Goal_NextAction(int t0, Actor who, ActorAction intent)
      : base(t0,who)
      {
#if DEBUG
        if (null == intent) throw new ArgumentNullException(nameof(intent));
#endif
        Intent = intent;
      }

      // always execute.  Expire on execution
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (!Intent.IsLegal()) {
          _isExpired = true;
          return true;
        }
        // XXX need some sense of what a combat action is
        if (null != m_Actor.Controller.enemies_in_FOV) return false;
       _isExpired = true;
        if (Intent.IsLegal()) ret = Intent;
        return true;
      }
    }

    [Serializable]
    internal class Goal_NextCombatAction : Objective
    {
        public readonly ActorAction Intent_engaged;
        public readonly ActorAction Intent_disengaged;

        public Goal_NextCombatAction(int t0, Actor who, ActorAction engaged, ActorAction disengaged)
        : base(t0, who)
        {
#if DEBUG
            if (null == engaged && null==disengaged) throw new ArgumentNullException(nameof(engaged)+"; "+nameof(disengaged));
#endif
            Intent_engaged = engaged;
            Intent_disengaged = disengaged;
        }

        // always execute.  Expire on execution
        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            _isExpired = true;
            var test = m_Actor.Controller.enemies_in_FOV;
            if (null == test) {
                if (null != Intent_disengaged && Intent_disengaged.IsLegal()) ret = Intent_disengaged;  // \todo may need to call auxilliary function instead
            } else {
                if (null != Intent_engaged && Intent_engaged.IsLegal()) ret = Intent_engaged;  // \todo may need to call auxilliary function instead
            }
            if (ret is ActionMoveStep step && m_Actor.Controller is ObjectiveAI ai) m_Actor.IsRunning = ai.RunIfAdvisable(step.dest);
            return true;
        }
    }

    [Serializable]
    internal class Goal_LastAction<T> : Objective
    {
      public readonly T Intent;

      public Goal_LastAction(int t0, Actor who, T intent)
      : base(t0,who)
      {
#if DEBUG
        if (null == intent) throw new ArgumentNullException(nameof(intent));
#endif
        Intent = intent;
      }

      public T LastAction { get {return Intent; } }

      // expire when two turns old
      // we are an influence on behaviors so we don't actually execute
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (2 <= m_Actor.Location.Map.LocalTime.TurnCounter - TurnCounter) {
          _isExpired = true;
          return true;
        }
        return false;
      }
    }

    [Serializable]
    internal class Goal_DoNotPickup : Objective
    {
      public readonly GameItems.IDs Avoid;

      public Goal_DoNotPickup(int t0, Actor who, GameItems.IDs avoid)
      : base(t0,who)
      {
        Avoid = avoid;
      }

      // never execute, as we're an influence on another behavior.
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        // expire if the offending item is not in LoS
        if (m_Actor.Controller.FOV.Select(pt => m_Actor.Location.Map.GetItemsAt(pt)).Any(inv => null!=inv && inv.Has(Avoid))) return false;
//      if (m_Actor.Inventory.Has(Avoid)) return false; // checking whether this is actually needed
        _isExpired = true;  // but expire if the offending item is not in LOS or inventory
        return false;
      }
    }

    [Serializable]
    internal class Goal_BreakLineOfSight : Objective
    {
      readonly private HashSet<Location> _locs;

      public Goal_BreakLineOfSight(int t0, Actor who, Location loc)
      : base(t0,who)
      {
        _locs = new HashSet<Location>{loc};
      }

      public Goal_BreakLineOfSight(int t0, Actor who, IEnumerable<Location> locs)
      : base(t0,who)
      {
        _locs = new HashSet<Location>(locs);
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        var denorm = m_Actor.Location.Map.Denormalize(_locs);
        if (null == denorm) return true;
        IEnumerable<Point> tmp =  denorm.Select(loc => loc.Position).Intersect(m_Actor.Controller.FOV);
        if (!tmp.Any()) return true;
        if (0 < (m_Actor.Controller as ObjectiveAI).InterruptLongActivity()) return false;
        ret = (m_Actor.Controller as OrderableAI).BehaviorWalkAwayFrom(denorm,null);
        return true;
      }

      public override string ToString()
      {
        return "Breaking line of sight to "+_locs.to_s();
      }
    }

    [Serializable]
    internal class Goal_AcquireLineOfSight : Objective
    {
      readonly private HashSet<Location> _locs;

      public Goal_AcquireLineOfSight(int t0, Actor who, Location loc)
      : base(t0,who)
      {
        _locs = new HashSet<Location>{loc};
      }

      public Goal_AcquireLineOfSight(int t0, Actor who, IEnumerable<Location> locs)
      : base(t0,who)
      {
        _locs = new HashSet<Location>(locs);
      }

      public void NewTarget(Location target)
      {
        _locs.Add(target);
      }

      public void NewTarget(IEnumerable<Location> target)
      {
        _locs.UnionWith(target);
      }

      public void RemoveTarget(Location target)
      {
        _locs.Remove(target);
      }

      public void RemoveTarget(IEnumerable<Location> target)
      {
        _locs.ExceptWith(target);
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        IEnumerable<Location> tmp = _locs.Where(loc => !m_Actor.Controller.CanSee(loc));
        if (!tmp.Any()) return true;
        ObjectiveAI ai = m_Actor.Controller as ObjectiveAI; // invariant: non-null
        // if any in-communication ally can see the location, clear it
        foreach(Actor friend in m_Actor.Allies) {
          if (!ai.InCommunicationWith(friend)) continue;
          tmp = tmp.Where(loc => !friend.Controller.CanSee(loc));
          if (!tmp.Any()) return true;
        }
        if (_locs.Count > tmp.Count()) {
          var relay = tmp.ToList();
          _locs.Clear();
          _locs.UnionWith(relay);
          // once we are caching inverse-FOV, clear that here
        }
        if (0 < ai.InterruptLongActivity()) return false;
        // XXX \todo really want inverse-FOVs for destinations; trigger calculation/retrieval from cache here
        ret = ai.BehaviorPathTo(m => new HashSet<Point>(_locs.Where(l => l.Map==m).Select(l => l.Position)));
        return true;
      }

      public override string ToString()
      {
        return "Acquiring line of sight to "+_locs.to_s();
      }
    }

    // \todo re-implement as mini-threat tracking
    [Serializable]
    internal class Goal_Terminate : Objective
    {
      readonly private Dictionary<Actor,HashSet<Location>> _target_locs = new Dictionary<Actor, HashSet<Location>>();

      public void NewTarget(Actor target)
      {
#if DEBUG
        if (!(target?.IsEnemyOf(m_Actor) ?? false)) throw new ArgumentNullException(nameof(target));
#endif
        if (_target_locs.TryGetValue(target, out var locs)) {
          locs.Clear();
          locs.Add(target.Location);
        } else _target_locs[target] = new HashSet<Location> { target.Location };
      }

      public Goal_Terminate(int t0, Actor who, Actor target)
      : base(t0,who)
      {
        NewTarget(target);
      }

      public Goal_Terminate(int t0, Actor who, IEnumerable<Actor> targets)
      : base(t0,who)
      {
        foreach(Actor a in targets) NewTarget(a);
      }

      private bool SuppressVisibleFor(Actor a)
      {
        foreach(var x in _target_locs) {
          x.Value.RemoveWhere(loc => a.Controller.CanSee(loc));
        }
        return false;
      }

      private void RefreshLocations()
      {
        foreach(var x in _target_locs) {
          if (x.Value.Contains(x.Key.Location)) continue;
          if (x.Key.Controller is ObjectiveAI) {
            var candidates = x.Key.OnePathRange(x.Key.Location);  // XXX fails for Z
            x.Value.IntersectWith(candidates.Keys);
          } else {
            var candidates = x.Key.OneStepRange(x.Key.Location);
            x.Value.IntersectWith(candidates);
          }
          // XXX \todo would like a little more fuzz in the results, but need CPU down first (start-game less than 1 second?)
          x.Value.Add(x.Key.Location);
        }
      }

      private HashSet<Point> WhereIn(Map m)
      {
        var ret = new HashSet<Point>();
        foreach(var x in _target_locs) ret.UnionWith(x.Value.Where(y=>y.Map==m).Select(y=>y.Position));
        return ret;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        _target_locs.OnlyIf(a => !a.IsDead);
        if (0 >= _target_locs.Count) return true;
        if (null != m_Actor.Controller.enemies_in_FOV) return false;    // we do use InterruptLongActivity later.  Don't want non-combat interrupts to interfere here
        if (SuppressVisibleFor(m_Actor)) return true;

        ObjectiveAI ai = m_Actor.Controller as ObjectiveAI; // invariant: non-null
        // if any in-communication ally can see the location, clear it
        var allies = m_Actor.Allies;
        if (null != allies) {
          foreach(Actor friend in allies) {
            if (!ai.InCommunicationWith(friend)) continue;
            if (null != friend.Controller.enemies_in_FOV) continue;
            if (SuppressVisibleFor(friend)) return true;
          }
        }
        RefreshLocations();
        if (ai is PlayerController) return false;   // do not hijack player (use case is threat detection)
        if (0 < ai.InterruptLongActivity()) return false;
        // XXX \todo really want inverse-FOVs for destinations; trigger calculation/retrieval from cache here
        ret = ai.BehaviorPathTo(m => WhereIn(m));
        return true;
      }

      public override string ToString()
      {
        return "Securing area";
      }
    }

    [Serializable]
    internal class Goal_PathTo : Objective
    {
      private readonly HashSet<Location> _locs;
      private readonly bool walking;

      public Goal_PathTo(int t0, Actor who, Location loc, bool walk=false)
      : base(t0,who)
      {
        _locs = new HashSet<Location>{loc};
        walking = walk;
      }

      public Goal_PathTo(int t0, Actor who, IEnumerable<Location> locs)
      : base(t0,who)
      {
        _locs = new HashSet<Location>(locs);
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (_locs.Contains(m_Actor.Location)) {
          _isExpired = true;
          return true;
        }

        if (ObjectiveAI.ReactionCode.NONE != (m_Actor.Controller as ObjectiveAI).InterruptLongActivity()) {
          _isExpired = true;    // cancel: something urgent
          return true;
        }

        ret = (m_Actor.Controller as ObjectiveAI).BehaviorPathTo(m => new HashSet<Point>(_locs.Where(loc => loc.Map==m).Select(loc => loc.Position)));
        if (!(ret?.IsLegal() ?? false)) {
          ret = null;
          _isExpired = true;    // cancel: buggy
          return true;
        }
        if (walking) m_Actor.Walk();
        return true;
      }
    }

    [Serializable]
    internal class Goal_PathToStack : Objective
    {
      private readonly List<Percept_<Inventory>> _stacks = new List<Percept_<Inventory>>(1);

      public IEnumerable<Inventory> Inventories { get { return _stacks.Select(p => p.Percepted); } }
      public IEnumerable<Location> Destinations { get { return _stacks.Select(p => p.Location); } }

      public Goal_PathToStack(int t0, Actor who, Location loc)
      : base(t0,who)
      {
#if DEBUG
        if (!(who.Controller is OrderableAI)) throw new InvalidOperationException("need an ai with inventory");
#endif
        if (!loc.Map.IsInBounds(loc.Position)) {
          Location? test = loc.Map.Normalize(loc.Position);
          if (null == test) return;
          loc = test.Value;
        }
        newStack(loc);
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        int i = _stacks.Count;
        if (0 >= i) {
          _isExpired = true;
          return true;
        }

        // update if stack is actually in sight
        while(0 < i--) {
          { // scope var p
          var p = _stacks[i];
          if (m_Actor.Controller.CanSee(p.Location)) {
            Inventory inv = p.Location.Items;
            if (inv?.IsEmpty ?? true) {
              _stacks.RemoveAt(i);
              continue;
            }
            _stacks[i] = new Percept_<Inventory>(inv, m_Actor.Location.Map.LocalTime.TurnCounter, p.Location);
          }
          } // end scope var p
          // XXX \todo some telepathic leakage since this isn't a value copy
          if (_stacks[i].Percepted.IsEmpty || !(m_Actor.Controller as OrderableAI).WouldGrabFromStack(_stacks[i].Location, _stacks[i].Percepted)) {
            _stacks.RemoveAt(i);
            continue;
          }
        }
        // at this point, all stacks appear valid and interesting
        if (0 >= _stacks.Count) {
          _isExpired = true;
          return true;
        }

        if (m_Actor.Controller.InCombat) return false;

        var at_target = _stacks.FirstOrDefault(p => m_Actor.MayTakeFromStackAt(p.Location));
        if (null != at_target) {
          ActorAction tmpAction = (m_Actor.Controller as OrderableAI).BehaviorGrabFromAccessibleStack(at_target.Location, at_target.Percepted);
          if (tmpAction?.IsLegal() ?? false) {
            ret = tmpAction;
            m_Actor.Activity = Activity.IDLE;
            _isExpired = true;  // we don't play well with action chains
            return true;
          }
          // invariant failure
#if DEBUG
          throw new InvalidOperationException("Prescreen for avoidng taboo tile marking failed: "+ret.to_s()+"; "+ at_target.Percepted.ToString()+"; "+ (m_Actor.Controller as OrderableAI).WouldGrabFromStack(at_target.Location, at_target.Percepted).ToString());
#else
          _stacks.Remove(at_target);
          if (0 >= _stacks.Count) {
            _isExpired = true;
            return true;
          }
#endif
        }

        // let other AI processing kick in before final pathing
        return false;
      }

      public void newStack(Location loc) {
        Inventory inv = loc.Items;
        if (inv?.IsEmpty ?? true) return;
        if (!(m_Actor.Controller as OrderableAI).WouldGrabFromStack(loc, inv)) return;

        int i = _stacks.Count;
        // update if stack is present
        while(0 < i--) {
          var p = _stacks[i];
          if (p.Location==loc) {
            _stacks[i] = new Percept_<Inventory>(inv, m_Actor.Location.Map.LocalTime.TurnCounter, p.Location);
            return;
          }
        }

        _stacks.Add(new Percept_<Inventory>(inv, m_Actor.Location.Map.LocalTime.TurnCounter, loc));   // otherwise, add
      }

      public ActorAction Pathing()
      {
        var _locs = _stacks.Select(p => p.Location);

        var ret = (m_Actor.Controller as OrderableAI).BehaviorPathTo(m => new HashSet<Point>(_locs.Where(loc => loc.Map==m).Select(loc => loc.Position)));
        return (ret?.IsLegal() ?? false) ? ret : null;
      }

      public override string ToString()
      {
        return "Pathing to "+ _stacks.to_s();
      }
    }

    [Serializable]
    internal class Goal_HintPathToActor : Objective
    {
      private readonly Actor _dest;
      private readonly ActorAction _when_at_target;

      public Goal_HintPathToActor(int t0, Actor who, Actor dest, ActorAction at_target=null)
      : base(t0,who)
      {
        _dest = dest;
        _when_at_target = at_target;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (turn < Session.Get.WorldTime.TurnCounter) { // deadline up
          _isExpired = true;
          return true;
        }
        if (null != _when_at_target && !_when_at_target.IsLegal()) {    // no longer viable
          _isExpired = true;
          return true;
        }
        if (m_Actor.Controller.InCombat) return false;
        if (Rules.IsAdjacent(m_Actor.Location,_dest.Location)) {
          ret = _when_at_target ?? new ActionWait(m_Actor);    // XXX should try to optimize ActionWait to any constructive non-movement action
          return true;
        }
        return false;
      }

      public ActorAction Pathing()
      {
        Location? test = m_Actor.Location.Map.Denormalize(_dest.Location);
        if (null!=test && m_Actor.Controller.FOV.Contains(test.Value.Position)) {
          var goals = new Dictionary<Point, int>{
            [test.Value.Position] = Rules.GridDistance(test.Value, m_Actor.Location)
          };
          ActorAction head_for = (m_Actor.Controller as OrderableAI).BehaviorEfficientlyHeadFor(goals);
          if (null==head_for || !head_for.IsLegal()) {
           _isExpired = true;
            return null;
          }
          return head_for;
        }

        IEnumerable<Point> dest_pts = m_Actor.Location.Position.Adjacent().Where(pt => m_Actor.Location.Map.IsWalkableFor(pt, m_Actor));
        ActorAction ret = (m_Actor.Controller as OrderableAI).BehaviorPathTo(m => (m == m_Actor.Location.Map ? new HashSet<Point>(dest_pts) : new HashSet<Point>()));
        if (null== ret || !ret.IsLegal()) {
         _isExpired = true;
          return null;
        }
        return ret;
      }

      public override string ToString()
      {
        return "Pathing to "+_dest.Name + " for " + (_when_at_target?.ToString() ?? "null");
      }
    }

    [Serializable]
    internal class Goal_BreakBarricade : Objective
    {
      private readonly Location _dest;  // 2018-08-12: Using DoorWindow here doesn't work -- AI continues breaking the barricade even after it is gone

      public Goal_BreakBarricade(int t0, Actor who, DoorWindow dest)
      : base(t0, who)
      {
#if DEBUG
        if (null == dest) throw new ArgumentNullException(nameof(dest));
#endif
        _dest = dest.Location;
      }

      public DoorWindow Target { get { return _dest.MapObject as DoorWindow; } }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        var door = Target;
        if (!door?.IsBarricaded ?? true) {  // it's down now
          _isExpired = true;
          return true;
        }
        if (ObjectiveAI.ReactionCode.NONE != (m_Actor.Controller as ObjectiveAI).InterruptLongActivity()) {
          _isExpired = true;    // cancel: something urgent
          return true;
        }
        if (Rules.IsAdjacent(m_Actor.Location, _dest)) {
          if (m_Actor.CanBreak(door)) {
            ret = new ActionBreak(m_Actor, door);
            return true;
          }
#if DEBUG
          if (!m_Actor.IsTired) throw new InvalidOperationException("!m_Actor.IsTired");
#endif
          // check for helpers that are ready and politely back off for them
          var escape = new List<Point>(8);
          var motive = new List<Point>(8);
          foreach(Point pt in m_Actor.Location.Position.Adjacent()) {
            if (Rules.IsAdjacent(pt, _dest.Position)) continue;
            if (m_Actor.Location.Map.IsWalkableFor(pt,m_Actor)) escape.Add(pt);
            else {
              Actor helper = m_Actor.Location.Map.GetActorAt(pt);
              if (null == helper) continue;
              if (!helper.IsTired && null != (helper.Controller as ObjectiveAI)?.Goal<Goal_BreakBarricade>(o => o.Target == door)) {
                motive.Add(pt);
              }
            }
          }
          if (0 < motive.Count && 0<escape.Count) {
            ret = new ActionMoveStep(m_Actor,RogueForm.Game.Rules.DiceRoller.Choose(escape));
            return true;
          }

          ret = new ActionWait(m_Actor);
          return true;
        }
        // unusual pathing requirement: do not push helpers; give helpers room to step aside
        var helpers_at = new Dictionary<Point,Actor>();
        var move_to = new HashSet<Point>();
        foreach(Point pt in _dest.Position.Adjacent()) {
          if (_dest.Map.IsWalkableFor(pt,m_Actor.Model)) {
            move_to.Add(pt);
            continue;
          };
          Actor a = _dest.Map.GetActorAt(pt);
          if (null == a) continue;
          if (m_Actor.IsEnemyOf(a)) continue;   // XXX intelligent enemies might react, but that's handled in sound processing
          var is_helping = (a.Controller as ObjectiveAI).Goal<Goal_BreakBarricade>(o => o.Target==door);
          if (null != is_helping) helpers_at[pt] = a;
          else move_to.Add(pt);
        }
        if (0<helpers_at.Count) {
          foreach(Point pt in move_to.ToList()) {
            bool ok = false;
            foreach(var x in helpers_at) {
              if (Rules.IsAdjacent(pt,x.Key)) {
                ok = true;
                break;
              }
            }
            if (!ok) move_to.Remove(pt);
          }
          if (0 >= move_to.Count) {
            foreach(var x in helpers_at) {
              foreach(Point pt in x.Key.Adjacent()) {
                if (move_to.Contains(pt)) continue;
                if (!_dest.Map.IsWalkableFor(pt,m_Actor.Model)) continue;
                move_to.Add(pt);
              }
            }
            if (0 >= move_to.Count) return false;   // XXX overcrowded \todo good time to wander
          }
        }
        if (0 < move_to.Count) {
          ret = (m_Actor.Controller as OrderableAI).BehaviorPathTo(m => (m == _dest.Map ? move_to : new HashSet<Point>()));
          return true;
        }
        return false;
      }
    }

#if FAIL
    [Serializable]
    public Goal_Sleep : Objective
    {
    }

    [Serializable]
    // fulfilled by one of: sleep, stimulates, non-leader ally pushing/shoving
    public Goal_Wakeup : Objective
    {
    }

    [Serializable]
    public Goal_BeThereAt : Objective
    {
      private int _start_time;
      private Map _dest_map;
      private HashSet<Point> _dest_pts = new HashSet<Point>();
//    List<Objective> _sub_goals = new List<Objective>();

      public Goal_BeThereAt(Location dest, int t0)
      : base(t0)
      {
        _dest_map = dest.Map;
        _dest_pts.Add(dest.Position);
//      _bootstrap_subgoals();
//      _calc_subgoals();
      }

      // This is "be at X at t0"; it does not imply guarding.  Rather, this is for
      // pre-positioning for the midnight invasions.
      public Goal_BeThereAt(Location src, Map dest_map, IEnumerable<Point> dest_pts, int t0, int STA_buffer=0)
      : base(t0)
      {
        _dest_map = dest_map;
        _dest_pts.UnionWith(dest_pts);
//      _bootstrap_subgoals();
//      _calc_subgoals();
      }

      bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        // if no subgoals are urgent, check pathing
        int steps = 0;
        Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps();
        if (dest.Map == m_Actor.Location.Map) {
	      navigate.GoalDistance(dest.Position,int.MaxValue,m_Actor.Location.Position);
          steps = navigate.Cost();
        } else {
          // XXX need the costs for each map in the sequence
          HashSet<Exit> valid_exits;
          HashSet<Map> exit_maps = m_Actor.Location.Map.PathTo(dest.Map, out valid_exits);

	      Exit exitAt = m_Actor.Location.Exit;
          if (exitAt != null && exit_maps.Contains(exitAt.ToMap) && m_Actor.CanUseExit(m_Actor.Location.Position))
            return new ActionUseExit(m_Actor, m_Actor.Location.Position);   // would prefer return BehaviorUseExit(game, BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES)
	      navigate.GoalDistance(m_Actor.Location.Map.ExitLocations(valid_exits),int.MaxValue,m_Actor.Location.Position);
          steps = navigate.Cost();
	    }
        // XXX convert steps to actual turns
        if (TurnCounter-steps>m_Actor.Location.Map.LocalTime.TurnCounter) return false;  // not urgent
        // final pathing
	    Dictionary<Point, int> tmp = navigate.Approach(m_Actor.Location.Position);
        ret = DecideMove(tmp.Keys, null, null);
        return null != ret;
	    return DecideMove(tmp.Keys, null, null);	// only called when no enemies in sight anyway
      }

      public virtual List<Objective> Subobjectives() { return new List<Objective>(_sub_goals); }
    }
#endif

    [Serializable]
  internal abstract class OrderableAI : ObjectiveAI
    {
    private const int EMOTE_GRAB_ITEM_CHANCE = 30;
    private const int LAW_ENFORCE_CHANCE = 30;
    private const int IN_LEADER_LOF_SAFETY_PENALTY = 1;  // alpha10 int

    // taboos really belong here
    private List<Actor> m_TabooTrades;

    // these relate to PC orders for NPCs.  Alpha 9 had no support for AI orders to AI.
    private ActorDirective m_Directive;
    private ActorOrder m_Order;
    protected Percept m_LastEnemySaw;
    protected Percept m_LastItemsSaw;
    protected Percept m_LastSoldierSaw;
    protected Percept m_LastRaidHeard;
    protected bool m_ReachedPatrolPoint;
    protected int m_ReportStage;

    public bool DontFollowLeader { get; set; }

    protected OrderableAI()
    {
    }

    public ActorDirective Directives { get { return m_Directive ?? (m_Directive = new ActorDirective()); } }
    protected List<Actor> TabooTrades { get { return m_TabooTrades; } }
    public ActorOrder Order { get { return m_Order; } }

    // doesn't include no-enemies check, inside check or any directives/orders
    // would like to trigger pathing to inside to enable sleeping
    // but have to distinguish between AI w/o item memory and AI w/item memory, etc.
    protected bool WantToSleepNow { get {
      return m_Actor.WouldLikeToSleep && m_Actor.CanSleep();
    } }

    public void SetOrder(ActorOrder newOrder)
    {
      m_Order = newOrder;
      m_ReachedPatrolPoint = false;
      m_ReportStage = 0;
    }

    protected ActorAction ExecuteOrder(RogueGame game, ActorOrder order, List<Percept> percepts)
    {
      if (!m_Actor.HasLeader) return null;
      switch (order.Task) {
        case ActorTasks.BARRICADE_ONE:
          return ExecuteBarricading(order.Location, false);
        case ActorTasks.BARRICADE_MAX:
          return ExecuteBarricading(order.Location, true);
        case ActorTasks.GUARD:
          return ExecuteGuard(order.Location, percepts);  // cancelled by enamies sighted
        case ActorTasks.PATROL:
          return ExecutePatrol(order.Location, percepts);  // cancelled by enamies sighted
        case ActorTasks.DROP_ALL_ITEMS:
          return ExecuteDropAllItems();
        case ActorTasks.BUILD_SMALL_FORTIFICATION:
          return ExecuteBuildFortification(order.Location, false);
        case ActorTasks.BUILD_LARGE_FORTIFICATION:
          return ExecuteBuildFortification(order.Location, true);
        case ActorTasks.REPORT_EVENTS:
          return ExecuteReport(game, percepts);  // cancelled by enamies sighted
        case ActorTasks.SLEEP_NOW:
          return ExecuteSleepNow(game, percepts);  // cancelled by enamies sighted
        case ActorTasks.FOLLOW_TOGGLE:
          return ExecuteToggleFollow(game);
        case ActorTasks.WHERE_ARE_YOU:
          return ExecuteReportPosition();
        default:
          throw new NotImplementedException("order task not handled");
      }
    }

    private ActorAction ExecuteBarricading(Location location, bool toTheMax)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      if (!(location.Map.GetMapObjectAt(location.Position) is DoorWindow door)) return null;
      if (!m_Actor.CanBarricade(door)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction= new ActionBarricadeDoor(m_Actor, door);
        if (!toTheMax) SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(location, false, false);
      if (null == tmpAction) return null;
      m_Actor.Run();
      return tmpAction;
    }

    private ActorAction ExecuteBuildFortification(Location location, bool isLarge)
    {
      if (m_Actor.Location.Map != location.Map) return null;
      if (!m_Actor.CanBuildFortification(location.Position, isLarge)) return null;
      ActorAction tmpAction = null;
      if (Rules.IsAdjacent(m_Actor.Location.Position, location.Position)) {
        tmpAction = new ActionBuildFortification(m_Actor, location.Position, isLarge);
        if (!tmpAction.IsLegal()) return null;
        SetOrder(null);
        return tmpAction;
      }
      tmpAction = BehaviorIntelligentBumpToward(location, false, false);
      if (null == tmpAction) return null;
      m_Actor.Run();
      return tmpAction;
    }

    private ActorAction ExecuteGuard(Location location, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }

      if (m_Actor.Location.Position != location.Position) {
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(location, false, false);
        if (actorAction3 != null) {
          m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (m_Actor.IsHungry) {
        ActorAction actorAction3 = BehaviorEat();
        if (actorAction3 != null) return actorAction3;
      }

      ActorAction actorAction4 = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (actorAction4 != null) return actorAction4;

      return new ActionWait(m_Actor);
    }

    private ActorAction ExecutePatrol(Location location, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }
      if (!m_ReachedPatrolPoint)
        m_ReachedPatrolPoint = m_Actor.Location.Position == location.Position;

      if (!m_ReachedPatrolPoint) {
        ActorAction actorAction3 = BehaviorIntelligentBumpToward(location, false, false);
        if (actorAction3 != null) {
          m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (m_Actor.IsHungry) {
        ActorAction actorAction3 = BehaviorEat();
        if (actorAction3 != null) return actorAction3;
      }

      ActorAction actorAction4 = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (actorAction4 != null) return actorAction4;

      List<Zone> patrolZones = location.Map.GetZonesAt(Order.Location.Position);
      return BehaviorWander(null, loc =>
      {
        List<Zone> zonesAt = loc.Map.GetZonesAt(loc.Position);
        if (zonesAt == null) return false;
        foreach (Zone zone1 in zonesAt) {
          foreach (Zone zone2 in patrolZones) {
            if (zone1 == zone2) return true;
          }
        }
        return false;
      });
    }

    private ActorAction ExecuteDropAllItems()
    {
      if (m_Actor.Inventory.IsEmpty) return null;

      // alpha10.1 bugfix followers drop all was looping
      // use drop item behaviour on the first item it can.
      foreach(Item it in m_Actor.Inventory.Items) {
        ActorAction dropAction = BehaviorDropItem(it);
        if (null != dropAction) return dropAction;
      }

      // we still have at least one item but cannot drop it for some reason,
      // consider the order done.
      return null;
    }

    private ActorAction ExecuteReport(RogueGame game, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }
      ActorAction actorAction = null;
      bool flag = false;
      switch (m_ReportStage)
      {
        case 0:
          actorAction = m_LastRaidHeard == null ? new ActionSay(m_Actor, m_Actor.Leader, "No raids heard.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastRaidHeard);
          ++m_ReportStage;
          break;
        case 1:
          actorAction = m_LastEnemySaw == null ? new ActionSay(m_Actor, m_Actor.Leader, "No enemies sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
          ++m_ReportStage;
          break;
        case 2:
          actorAction = m_LastItemsSaw == null ? new ActionSay(m_Actor, m_Actor.Leader, "No items sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
          ++m_ReportStage;
          break;
        case 3:
          actorAction = m_LastSoldierSaw == null ? new ActionSay(m_Actor, m_Actor.Leader, "No soldiers sighted.", RogueGame.Sayflags.NONE) : BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
          ++m_ReportStage;
          break;
        case 4:
          flag = true;
          actorAction = new ActionSay(m_Actor, m_Actor.Leader, "That's it.", RogueGame.Sayflags.NONE);
          break;
      }
      if (flag) SetOrder(null);
      return actorAction ?? new ActionSay(m_Actor, m_Actor.Leader, "Let me think...", RogueGame.Sayflags.NONE);
    }

    private ActorAction ExecuteSleepNow(RogueGame game, List<Percept> percepts)
    {
      List<Percept> enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted as Actor;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", (object) actor.Name));
      }
      if (m_Actor.CanSleep(out string reason)) {
        return (m_Actor.Location.Map.LocalTime.TurnCounter % 2 == 0 ? (ActorAction)(new ActionSleep(m_Actor)) : new ActionWait(m_Actor));
      }
      SetOrder(null);
      game.DoEmote(m_Actor, string.Format("I can't sleep now : {0}.", (object) reason));
      return new ActionWait(m_Actor);
    }

    private ActorAction ExecuteToggleFollow(RogueGame game)
    {
      SetOrder(null);
      DontFollowLeader = !DontFollowLeader;
      game.DoEmote(m_Actor, DontFollowLeader ? "OK I'll do my stuff, see you soon!" : "I'm ready!");
      return new ActionWait(m_Actor);
    }

    private ActorAction ExecuteReportPosition()
    {
      SetOrder(null);
      string text = string.Format("I'm in {0} at {1},{2}.", m_Actor.Location.Map.Name, m_Actor.Location.Position.X, m_Actor.Location.Position.Y);
      return new ActionSay(m_Actor, m_Actor.Leader, text, RogueGame.Sayflags.NONE);
    }

    // this assumes conditions like "everything is in FOV" so that a floodfill pathfinding is not needed.
    // we also assume no enemies in sight.
    // XXX as a de-facto leaf function, we can get away with destructive modifications to goals
    public ActorAction BehaviorEfficientlyHeadFor(Dictionary<Point,int> goals)
    {
      if (0>=goals.Count) return null;
      if (null == _legal_steps) return null;
      List<Point> legal_steps = (2 <= _legal_steps.Count) ? DecideMove_WaryOfTraps(_legal_steps) : _legal_steps;    // need working copy here
      if (2 <= legal_steps.Count) {
        int min_dist = goals.Values.Min();
        // this breaks down if 2+ goals equidistant.
        {
        var nearest = new List<Point>(goals.Count);
        foreach(var x in goals) {
          if (x.Value>min_dist) continue;
          nearest.Add(x.Key);
        }
        if (1<nearest.Count) {
           int i = RogueForm.Game.Rules.DiceRoller.Roll(0,nearest.Count);
           nearest.RemoveAt(i);
           foreach(Point pt in nearest) goals.Remove(pt);
        }
        }
        // exactly one minimum-cost goal now
        int near_scale = goals.Count+1;
        var efficiency = new Dictionary<Point,int>();
        foreach(Point pt in legal_steps) {
          efficiency[pt] = 0;
          foreach(var pt_delta in goals) {
            // relies on FOV not being "too large"
            int delta = pt_delta.Value-Rules.GridDistance(pt, pt_delta.Key);
            if (min_dist == pt_delta.Value) {
              efficiency[pt] += near_scale*delta;
            } else {
              efficiency[pt] += delta;
            }
          }
        }
        int fast_approach = efficiency.Values.Max();
        efficiency.OnlyIf(val=>fast_approach==val);
        legal_steps = new List<Point>(efficiency.Keys);
      }

	  ActorAction tmpAction = DecideMove(legal_steps);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      return null;
    }

    public bool IsRationalTradeItem(Actor speaker, Item offeredItem)    // Cf. ActorControllerAI::IsInterestingTradeItem
    {
#if DEBUG
      if (null == speaker) throw new ArgumentNullException(nameof(speaker));
      if (!speaker.Model.Abilities.CanTrade) throw new ArgumentOutOfRangeException(nameof(speaker),"both parties trading must be capable of it");
      if (!m_Actor.Model.Abilities.CanTrade) throw new ArgumentOutOfRangeException(nameof(speaker),"both parties trading must be capable of it");
#endif
      return IsInterestingItem(offeredItem);
    }

    private void MaximizeRangedTargets(List<Point> dests, List<Percept> enemies)
    {
      if (null == dests || 2>dests.Count) return;

      var targets = new Dictionary<Point,int>();
      int max_range = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
      foreach(Point pt in dests) {
        targets[pt] = enemies.Count(p => LOS.CanTraceHypotheticalFireLine(new Location(m_Actor.Location.Map,pt), p.Location.Position, max_range, m_Actor));
      }
      int max_LoF = targets.Values.Max();
      dests.RemoveAll(pt => max_LoF>targets[pt]);
    }

    protected List<ItemRangedWeapon> GetAvailableRangedWeapons()
    {
      IEnumerable<ItemRangedWeapon> tmp_rw = ((!Directives.CanFireWeapons || m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) ? null : m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)));
      return (null!=tmp_rw && tmp_rw.Any() ? tmp_rw.ToList() : null);
    }

    protected bool HasBehaviorThatRecallsToSurface {
      get {
        if (null != m_Actor.Threats) return true;   // hunting threat
        if (null != m_Actor.InterestingLocs) return true;   // tourism
        if (m_Actor.HasLeader) {
          if (m_Actor.Leader.IsPlayer) return true; // typically true...staying in the subway forever isn't a good idea
          if ((m_Actor.Leader.Controller as OrderableAI)?.HasBehaviorThatRecallsToSurface ?? false) return true;   // if leader has recall-to-surface behavior, following him recalls to surface
        }
        return false;
      }
    }

    protected bool WantToRecharge(ItemLight it)
    {
      int burn_time = 0;
      switch(it.Model.ID)
      {
      case GameItems.IDs.LIGHT_FLASHLIGHT: burn_time = m_Actor.Location.Map.LocalTime.SunsetToDawnDuration+2*WorldTime.TURNS_PER_HOUR;
        break;
      case GameItems.IDs.LIGHT_BIG_FLASHLIGHT: burn_time = m_Actor.Location.Map.LocalTime.MidnightToDawnDuration+WorldTime.TURNS_PER_HOUR;
        break;
#if DEBUG
      default: throw new InvalidOperationException("Unhandled light type " + it.Model.ID.ToString());
#else
      default: return false;
#endif
      }
      return it.Batteries<burn_time;
    }

    protected bool WantToRecharge() {
      return m_Actor.Inventory.GetItemsByType<ItemLight>()?.Any(it => WantToRecharge(it)) ?? false;
    }

    // XXX *could* eliminate int turn by defining it as location.Map.LocalTime.TurnCounter
    public void OnRaid(RaidType raid, Location location, int turn)
    {
      if (m_Actor.IsSleeping) return;
      string str;
      switch (raid) {
        case RaidType.BIKERS:
          str = "motorcycles coming";
          break;
        case RaidType.GANGSTA:
          str = "cars coming";
          break;
        case RaidType.BLACKOPS:
          str = "a chopper hovering";
          break;
        case RaidType.SURVIVORS:
          str = "honking coming";
          break;
        case RaidType.NATGUARD:
          str = "the army coming";
          break;
        case RaidType.ARMY_SUPLLIES:
          str = "a chopper hovering";
          break;
        default:
          throw new ArgumentOutOfRangeException(string.Format("unhandled raidtype {0}", (object) raid.ToString()));
      }
      m_LastRaidHeard = new Percept((object) str, turn, location);
    }

    // Behaviors and support functions
    // but all body armors are equipped to the torso slot(?)
    private ItemBodyArmor GetEquippedBodyArmor()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemBodyArmor armor) return armor;
      }
      return null;
    }

    protected void BehaviorEquipBestBodyArmor()
    {
      ItemBodyArmor bestBodyArmor = m_Actor.GetBestBodyArmor();
      if (bestBodyArmor == null) return;
      if (GetEquippedBodyArmor() != bestBodyArmor) RogueForm.Game.DoEquipItem(m_Actor, bestBodyArmor);
    }

    protected ActorAction ManageMeleeRisk(List<ItemRangedWeapon> available_ranged_weapons, List<Percept> enemies)
    {
      ActorAction tmpAction = null;
      if ((null != _retreat || null != _run_retreat) && null != available_ranged_weapons && null!=enemies) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(_retreat, enemies);
        MaximizeRangedTargets(_run_retreat, enemies);
        IEnumerable<Actor> fast_enemies = enemies.Select(p => p.Percepted as Actor).Where(a => a.Speed >= 2 * m_Actor.Speed);   // typically rats.
        if (fast_enemies.Any()) return null;    // not practical to run from rats.
        // ranged weapon: fast retreat ok
        // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
        // XXX we also want to be close enough to fire at all
#if TIME_TURNS
      Stopwatch timer = Stopwatch.StartNew();
#endif
        tmpAction = (_safe_run_retreat ? DecideMove(_legal_steps, _run_retreat) : ((null != _retreat) ? DecideMove(_retreat) : null));
#if TIME_TURNS
        timer.Stop();
        if (0<timer.ElapsedMilliseconds) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": DecideMove " + timer.ElapsedMilliseconds.ToString()+"ms; "+safe_run_retreat.ToString());
#endif
        if (null != tmpAction) {
          if (tmpAction is ActionMoveStep test) {
            // all setup should have been done in the run-retreat case
            if (!_safe_run_retreat) {
              m_Actor.IsRunning = RunIfAdvisable(test.dest);
#if PROTOTYPE
              if (m_Actor.IsRunning) {
                // \todo set up Goal_NextAction or Goal_NextCombatAction
                // * if attackable enemies, attack
                // * else if not in damage field, rest (to reset AP) or try to improve tactical positioning/get away further
                // * else do not contrain processing (null out)
              }
#endif
            }
            if (_safe_run_retreat) m_Actor.Run();
            else m_Actor.IsRunning = RunIfAdvisable(test.dest);
          }
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }

      if (null != _retreat) {
        // need stamina to melee: slow retreat ok
        if (WillTireAfterAttack(m_Actor)) {
	      tmpAction = DecideMove(_retreat);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        // have slow enemies nearby
        if (null != _slow_melee_threat) {
	      tmpAction = DecideMove(_retreat);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
      }
      // end melee risk management check
      return null;
    }

    private void ETAToKill(Actor en, int dist, ItemRangedWeapon rw, Dictionary<Actor, int> best_weapon_ETAs, Dictionary<Actor, ItemRangedWeapon> best_weapons=null)
    {
      Attack tmp = m_Actor.HypotheticalRangedAttack(rw.Model.Attack, dist, en);
	  int a_dam = tmp.DamageValue - en.CurrentDefence.Protection_Shot;
      if (0 >= a_dam) return;   // do not update ineffective weapons
      int a_kill_b_in = ((8*en.HitPoints)/(5*a_dam))+2;	// assume bad luck when attacking.
      // Also, assume one fluky miss and compensate for overkill returning 0 rather than 1 attacks.
      if (a_kill_b_in > rw.Ammo) {  // account for reloading weapon
        int turns = a_kill_b_in-rw.Ammo;
        a_kill_b_in++;
        a_kill_b_in += turns/rw.Model.MaxAmmo;
      }
      if (null == best_weapons) {
        best_weapon_ETAs[en] = a_kill_b_in;
        return;
      } else if (!best_weapons.ContainsKey(en) || best_weapon_ETAs[en] > a_kill_b_in) {
        best_weapons[en] = rw;
        best_weapon_ETAs[en] = a_kill_b_in;
        return;
      } else if (2 == best_weapon_ETAs[en]) {
        Attack tmp2 = m_Actor.HypotheticalRangedAttack(best_weapons[en].Model.Attack, dist, en);
        if (tmp.DamageValue < tmp2.DamageValue) {   // lower damage for overkill is usually better
          best_weapons[en] = rw;
          best_weapon_ETAs[en] = a_kill_b_in;
        }
        return;
      }
    }

    private ActorAction Equip(ItemRangedWeapon rw) {
      if (!rw.IsEquipped /* && m_Actor.CanEquip(rw) */) RogueForm.Game.DoEquipItem(m_Actor, rw);
      if (0 >= rw.Ammo) {
        ItemAmmo ammo = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
        if (null != ammo) return new ActionUseItem(m_Actor, ammo);
      }
      return null;
    }

    private ActorAction BehaviorMeleeSnipe(Actor en, Attack tmp_attack, bool one_on_one)
    {
      if (en.HitPoints>tmp_attack.DamageValue/2) return null;
      ActorAction tmpAction = null;
      // can one-shot
      if (!m_Actor.WillTireAfter(Rules.STAMINA_COST_MELEE_ATTACK + tmp_attack.StaminaPenalty)) {    // safe
        tmpAction = BehaviorMeleeAttack(en);
        if (null != tmpAction) return tmpAction;
      }
      if (one_on_one && tmp_attack.HitValue>=2*en.CurrentDefence.Value) { // probably ok
        tmpAction = BehaviorMeleeAttack(en);
        if (null != tmpAction) return tmpAction;
      }
      return null;
    }

    private HashSet<Point> GetRangedAttackFromZone(List<Percept> enemies)
    {
      var ret = new HashSet<Point>();
      var danger = new HashSet<Point>();
      foreach(Percept en in enemies) {
        if (null!=((en.Percepted as Actor).Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo()) continue;
        foreach(var pt in en.Location.Position.Adjacent()) danger.Add(pt);
      }
      int range = m_Actor.CurrentRangedAttack.Range;
      System.Collections.ObjectModel.ReadOnlyCollection<Point> optimal_FOV = LOS.OptimalFOV(range);
      foreach(Percept en in enemies) {
        foreach(Point pt in optimal_FOV.Select(p => new Point(p.X+en.Location.Position.X,p.Y+en.Location.Position.Y))) {
          if (ret.Contains(pt)) continue;
          if (danger.Contains(pt)) continue;
          if (!m_Actor.Location.Map.IsValid(pt)) continue;
          var LoF = new List<Point>();  // XXX micro-optimization?: create once, clear N rather than create N
          if (LOS.CanTraceHypotheticalFireLine(new Location(en.Location.Map,pt), en.Location.Position, range, m_Actor, LoF)) ret.UnionWith(LoF);
          // if "safe" attack possible init danger in different/earlier loop
        }
      }
      ret.Remove(m_Actor.Location.Position);    // if we could fire from here we wouldn't have called this; prevents invariant crash later
      return ret;
    }

    // forked from BaseAI::BehaviorEquipWeapon
    protected ActorAction BehaviorEquipWeapon(RogueGame game, List<ItemRangedWeapon> available_ranged_weapons, List<Percept> enemies)
    {
#if DEBUG
      if ((null == available_ranged_weapons) != (null == GetBestRangedWeaponWithAmmo())) throw new InvalidOperationException("(null == available_ranged_weapons) != (null == GetBestRangedWeaponWithAmmo())");
#endif

      // migrated from CivilianAI::SelectAction
      ActorAction tmpAction = null;
      if (null != enemies) {
        if (1==Rules.InteractionDistance(enemies[0].Location,m_Actor.Location)) {
          // something adjacent...check for one-shotting
          ItemMeleeWeapon tmp_melee = m_Actor.GetBestMeleeWeapon();
          if (null!=tmp_melee) {
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              tmpAction = BehaviorMeleeSnipe(en, m_Actor.MeleeWeaponAttack(tmp_melee.Model, en),null==_immediate_threat || (1==_immediate_threat.Count && _immediate_threat.Contains(en)));
              if (null != tmpAction) {
                if (!tmp_melee.IsEquipped) game.DoEquipItem(m_Actor, tmp_melee);
                return tmpAction;
              }
            }
          } else { // also check for no-weapon one-shotting
            foreach(Percept p in enemies) {
              if (!Rules.IsAdjacent(p.Location.Position,m_Actor.Location.Position)) break;
              Actor en = p.Percepted as Actor;
              tmpAction = BehaviorMeleeSnipe(en, m_Actor.UnarmedMeleeAttack(en), null == _immediate_threat || (1 == _immediate_threat.Count && _immediate_threat.Contains(en)));
              if (null != tmpAction) {
                if (0 < m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS)) {
                  Item tmp_w = m_Actor.GetEquippedWeapon();
                  if (null != tmp_w) game.DoUnequipItem(m_Actor,tmp_w);
                }
                return tmpAction;
              }
            }
          }
        }
      }

      // if no ranged weapons, use BaseAI
      // OrderableAI::GetAvailableRangedWeapons knows about AI disabling of ranged weapons
      if (null == available_ranged_weapons) return base.BehaviorEquipWeapon(game);

      // if no enemies in sight, reload all ranged weapons and then equip longest-range weapon
      // XXX there may be more important objectives than this
      if (null == enemies) {
        foreach(ItemRangedWeapon rw in available_ranged_weapons) {
          if (rw.Model.MaxAmmo <= rw.Ammo) continue;
          ItemAmmo am = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
          if (null == am) continue;
          if (0 == rw.Ammo || (rw.Model.MaxAmmo - rw.Ammo) >= am.Quantity) {
            tmpAction = Equip(rw);
            if (null != tmpAction) return tmpAction;
            return new ActionUseItem(m_Actor,am);
          }
        }
        return Equip(GetBestRangedWeaponWithAmmo());
      }
      // at this point, null != enemies, we have a ranged weapon available, and melee one-shot is not feasible
      // also, damage field should be non-null because enemies is non-null

      int best_range = available_ranged_weapons.Select(rw => rw.Model.Attack.Range).Max();
      List<Percept> en_in_range = FilterFireTargets(enemies,best_range);

      // if no enemies in range, or just one available ranged weapon, use the best one
      if (null == en_in_range || 1==available_ranged_weapons.Count) {
        tmpAction = Equip(GetBestRangedWeaponWithAmmo());
        if (null != tmpAction) return tmpAction;
      }

      if (null == en_in_range && null != _legal_steps) {
        List<Percept> percepts2 = FilterPossibleFireTargets(enemies);
		if (null != percepts2) {
		  IEnumerable<Point> tmp = _legal_steps.Where(p=>null!=FilterContrafactualFireTargets(percepts2,p));
		  if (tmp.Any()) {
	        tmpAction = DecideMove(tmp);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.FIGHTING;
              if (tmpAction is ActionMoveStep test) {
                m_Actor.IsRunning = RunIfAdvisable(test.dest);
#if PROTOTYPE
                if (m_Actor.IsRunning) {
                   // \todo set up Goal_NextAction or Goal_NextCombatAction
                   // * if attackable enemies, attack
                   // * else if not in damage field, rest (to reset AP) or try to improve tactical positioning/get away further
                   // * else do not contrain processing (null out)
                }
#endif
              }
              return tmpAction;
            }
		  }
        }

        // XXX need to use floodfill pathfinder
        var fire_from_here = GetRangedAttackFromZone(enemies);
        if (2<=fire_from_here.Count) NavigateFilter(fire_from_here);
        tmpAction = BehaviorNavigate(fire_from_here);
        if (null != tmpAction) return tmpAction;
      }

      if (null == en_in_range) return null; // no enemies in range, no constructive action: do something else

      // filter immediate threat by being in range
      var immediate_threat_in_range = (null!=_immediate_threat ? new HashSet<Actor>(_immediate_threat) : new HashSet<Actor>());
      if (null != _immediate_threat) immediate_threat_in_range.IntersectWith(en_in_range.Select(p => p.Percepted as Actor));

      if (1 == available_ranged_weapons.Count) {
        if (1 == en_in_range.Count) {
          return BehaviorRangedAttack(en_in_range[0].Percepted as Actor);
        } else if (1 == immediate_threat_in_range.Count) {
          return BehaviorRangedAttack(immediate_threat_in_range.First());
        }
      }

      // Get ETA stats
      var best_weapon_ETAs = new Dictionary<Actor,int>();
      var best_weapons = new Dictionary<Actor,ItemRangedWeapon>();
      if (1<available_ranged_weapons.Count) {
        foreach(Percept p in en_in_range) {
          Actor a = p.Percepted as Actor;
          int range = Rules.InteractionDistance(m_Actor.Location, p.Location);
          foreach(ItemRangedWeapon rw in available_ranged_weapons) {
            if (range > rw.Model.Attack.Range) continue;
            ETAToKill(a, range, rw,best_weapon_ETAs, best_weapons);
          }
        }
      } else {
        foreach(Percept p in en_in_range) {
          Actor a = p.Percepted as Actor;
          ETAToKill(a,Rules.InteractionDistance(m_Actor.Location,p.Location), available_ranged_weapons[0], best_weapon_ETAs);
        }
      }

      // cf above: we got here because there were multiple ranged weapons to choose from in these cases
      if (1 == en_in_range.Count) {
        Actor a = en_in_range[0].Percepted as Actor;
        return Equip(best_weapons[a]) ?? BehaviorRangedAttack(a);
      } else if (1 == immediate_threat_in_range.Count) {
        Actor a = immediate_threat_in_range.First();
        return Equip(best_weapons[a]) ?? BehaviorRangedAttack(a);
      }
      // at this point: there definitely is more than one enemy in range
      // if there are any immediate threat, there are at least two immediate threat
      if (2 <= immediate_threat_in_range.Count) {
        int ETA_min = immediate_threat_in_range.Select(a => best_weapon_ETAs[a]).Min();
        immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => best_weapon_ETAs[a] == ETA_min));
        if (2 <= immediate_threat_in_range.Count) {
          int HP_min = ((2 >= ETA_min) ? immediate_threat_in_range.Select(a => a.HitPoints).Max() : immediate_threat_in_range.Select(a => a.HitPoints).Min());
          immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => a.HitPoints == HP_min));
          if (2 <= immediate_threat_in_range.Count) {
           int dist_min = immediate_threat_in_range.Select(a => Rules.InteractionDistance(m_Actor.Location,a.Location)).Min();
           immediate_threat_in_range = new HashSet<Actor>(immediate_threat_in_range.Where(a => Rules.InteractionDistance(m_Actor.Location, a.Location) == dist_min));
          }
        }
        Actor actor = immediate_threat_in_range.First();
        if (1 < available_ranged_weapons.Count) {
         tmpAction = Equip(best_weapons[actor]);
         if (null != tmpAction) return tmpAction;
        }
        return BehaviorRangedAttack(actor);
      }
      // at this point, no immediate threat in range
      {
        int ETA_min = en_in_range.Select(p => best_weapon_ETAs[p.Percepted as Actor]).Min();
        if (2==ETA_min) {
          // snipe something
          en_in_range = new List<Percept>(en_in_range.Where(p => ETA_min == best_weapon_ETAs[p.Percepted as Actor]));
          if (2<=en_in_range.Count) {
            int HP_max = en_in_range.Select(p => (p.Percepted as Actor).HitPoints).Max();
            en_in_range = new List<Percept>(en_in_range.Where(p => (p.Percepted as Actor).HitPoints == HP_max));
            if (2<=en_in_range.Count) {
             int dist_min = en_in_range.Select(p => Rules.InteractionDistance(m_Actor.Location,p.Location)).Min();
             en_in_range = new List<Percept>(en_in_range.Where(p => Rules.InteractionDistance(m_Actor.Location, p.Location) == dist_min));
            }
          }
          Actor actor = en_in_range.First().Percepted as Actor;
          if (1 < available_ranged_weapons.Count) {
            tmpAction = Equip(best_weapons[actor]);
            if (null != tmpAction) return tmpAction;
          }
          return BehaviorRangedAttack(actor);
        }
      }

      // just deal with something close
      {
        int dist_min = en_in_range.Select(p => Rules.InteractionDistance(m_Actor.Location,p.Location)).Min();
        en_in_range = new List<Percept>(en_in_range.Where(p => Rules.InteractionDistance(m_Actor.Location, p.Location) == dist_min));
        if (2<=en_in_range.Count) {
          int HP_min = en_in_range.Select(p => (p.Percepted as Actor).HitPoints).Min();
          en_in_range = new List<Percept>(en_in_range.Where(p => (p.Percepted as Actor).HitPoints == HP_min));
        }
        Actor actor = en_in_range.First().Percepted as Actor;
        if (1 < available_ranged_weapons.Count) {
          tmpAction = Equip(best_weapons[actor]);
          if (null != tmpAction) return tmpAction;
        }
        return BehaviorRangedAttack(actor);
      }
    }

    // This is only called when the actor is hungry.  It doesn't need to do food value corrections
    protected ItemFood GetBestEdibleItem()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      int need = m_Actor.MaxFood - m_Actor.FoodPoints;
      ItemFood obj1 = null;
      int rating = int.MinValue;
      foreach (Item obj2 in m_Actor.Inventory.Items) {
        if (!(obj2 is ItemFood food)) continue;
        int num3 = 0;
        int num4 = food.NutritionAt(turnCounter);
        int num5 = num4 - need;
        if (num5 > 0) num3 -= num5;
        if (!food.IsPerishable) num3 -= num4;
        if (num3 > rating) {
          obj1 = food;
          rating = num3;
        }
      }
      return obj1;
    }

    // This is more pro-active.  We might want to flag whether
    // an AI uses the behavior based on this
    protected ItemFood GetBestPerishableItem()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
      int need = m_Actor.MaxFood - m_Actor.FoodPoints;
      ItemFood obj1 = null;
      int rating = int.MinValue;
      foreach (Item obj2 in m_Actor.Inventory.Items) {
        if (!(obj2 is ItemFood food)) continue;
        if (!food.IsPerishable) continue;
        if (food.IsSpoiledAt(turnCounter)) continue;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4 > need) continue; // more work needed
        int num3 = need-num4;
        if (num3 > rating) {
          obj1 = food;
          rating = num3;
        }
      }
      return obj1;
    }

    protected ActorAction BehaviorEat()
    {
      ItemFood bestEdibleItem = GetBestEdibleItem();
      if (null == bestEdibleItem) return null;
      return (m_Actor.CanUse(bestEdibleItem) ? new ActionUseItem(m_Actor, bestEdibleItem) : null);
    }

    protected ActorAction BehaviorEatProactively()
    {
      Item bestEdibleItem = GetBestPerishableItem();
      if (null == bestEdibleItem) return null;
      return (m_Actor.CanUse(bestEdibleItem) ? new ActionUseItem(m_Actor, bestEdibleItem) : null);
    }

    protected void BehaviorUnequipLeftItem(RogueGame game)
    {
      Item equippedItem = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
      if (null != equippedItem) game.DoUnequipItem(m_Actor, equippedItem);
    }

    protected ItemLight GetLight()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      ItemLight ret = null;
      foreach(Item obj in m_Actor.Inventory.Items) { 
        if (obj is ItemLight light && !light.IsUseless && (ret?.IsLessUsableThan(light) ?? true)) ret = light;
      }
      return ret;
    }

    /// <returns>true if and only if light should be equipped</returns>
    protected bool BehaviorEquipLight()
    {
      ItemLight tmp = GetLight();
      if (null == tmp || !NeedsLight()) return false;
      if (!tmp.IsEquipped /* && (m_Actor.CanEquip(tmp)  */) RogueForm.Game.DoEquipItem(m_Actor, tmp);
      return true;
    }

    /// <returns>true if and only if a cell phone is required to be equipped</returns>
    protected bool BehaviorEquipCellPhone(RogueGame game)
    {
      bool wantCellPhone = m_Actor.NeedActiveCellPhone; // XXX could dial 911, at least while that desk is manned
      if (!wantCellPhone) return false;
      ItemTracker equippedCellPhone = m_Actor.GetEquippedCellPhone();
      if (null != equippedCellPhone) return true;
      ItemTracker firstTracker = m_Actor.Inventory.GetFirstMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless);
      if (firstTracker != null /* && m_Actor.CanEquip(firstTracker) */) {
        game.DoEquipItem(m_Actor, firstTracker);
        return true;
      }
      return false;
    }

    /// <returns>null, or a legal ActionThrowGrenade</returns>
    protected ActorAction BehaviorThrowGrenade(RogueGame game, List<Percept> enemies)
    {
      if (3 > (enemies?.Count ?? 0)) return null;
      ItemGrenade firstGrenade = m_Actor.Inventory.GetFirstMatching<ItemGrenade>();
      if (firstGrenade == null) return null;
      ItemGrenadeModel itemGrenadeModel = firstGrenade.Model;
      int maxRange = m_Actor.MaxThrowRange(itemGrenadeModel.MaxThrowDistance);
      Point? bestSpot = null;
      int bestSpotScore = 0;
      foreach (Point point in m_Actor.Controller.FOV) {
        int my_dist = Rules.GridDistance(m_Actor.Location.Position, point);
        if (itemGrenadeModel.BlastAttack.Radius >= my_dist) continue;
        if (maxRange < my_dist) continue;
        if (!LOS.CanTraceThrowLine(m_Actor.Location, point, maxRange)) continue;
        if (_blast_field?.Contains(point) ?? false) continue;
        int score = 0;
        Rectangle blast_zone = new Rectangle(point.X-itemGrenadeModel.BlastAttack.Radius, point.Y-itemGrenadeModel.BlastAttack.Radius, 2*itemGrenadeModel.BlastAttack.Radius+1, 2*itemGrenadeModel.BlastAttack.Radius+1);
        // XXX \todo we want to evaluate the damage for where threat is *when the grenade explodes*
        if (   !blast_zone.Any(pt => {
                  if (!m_Actor.Location.Map.IsValid(pt)) return false;
                  Actor actorAt = m_Actor.Location.Map.GetActorAtExt(pt);
                  if (null == actorAt) return false;
                  if (actorAt == m_Actor) throw new ArgumentOutOfRangeException("actorAt == m_Actor"); // probably an invariant failure
                  int distance = Rules.GridDistance(new Location(m_Actor.Location.Map,point), actorAt.Location);
//                  if (distance > itemGrenadeModel.BlastAttack.Radius) throw new ArgumentOutOfRangeException("distance > itemGrenadeModel.BlastAttack.Radius"); // again, probably an invariant failure
                  if (m_Actor.IsEnemyOf(actorAt)) {
                    score += (itemGrenadeModel.BlastAttack.DamageAt(distance) * actorAt.MaxHPs);
                    return false;
                  }
//                  num2 = -1;
                  return true;
               })
            &&  score>bestSpotScore) {
            bestSpot = point;
            bestSpotScore = score;
          }
      }
      if (null == bestSpot /* || !nullable.HasValue */) return null;  // 2nd test probably redundant
      if (!firstGrenade.IsEquipped) game.DoEquipItem(m_Actor, firstGrenade);
      ActorAction actorAction = new ActionThrowGrenade(m_Actor, bestSpot.Value);
      if (!actorAction.IsLegal()) throw new ArgumentOutOfRangeException("created illegal ActionThrowGrenade");  // invariant failure
      return actorAction;
    }

    private static string MakeCentricLocationDirection(Location from, Location to)
    {
      if (from.Map != to.Map) return string.Format("in {0}", (object) to.Map.Name);
      Point position1 = from.Position;
      Point position2 = to.Position;
      Point v = new Point(position2.X - position1.X, position2.Y - position1.Y);
      return string.Format("{0} tiles to the {1}", (object) (int) Rules.StdDistance(v), (object) Direction.ApproximateFromVector(v));
    }

    private bool IsItemWorthTellingAbout(Item it)
    {
      return it != null && !(it is ItemBarricadeMaterial) && (m_Actor.Inventory.IsEmpty || !m_Actor.Inventory.Contains(it));
    }

    protected ActorAction BehaviorTellFriendAboutPercept(RogueGame game, Percept percept)
    {
      Map map = m_Actor.Location.Map;
      Dictionary<Point,Actor> friends = map.FindAdjacent(m_Actor.Location.Position,(m,pt) => {
        Actor a = m.GetActorAtExt(pt);
        return (null == a || a.IsSleeping || m_Actor.IsEnemyOf(a)) ? null : a;
      });
      if (0 >= friends.Count) return null;
      Actor actorAt1 = RogueForm.Game.Rules.DiceRoller.Choose(friends).Value;
      string str1 = MakeCentricLocationDirection(m_Actor.Location, percept.Location);
      string str2 = string.Format("{0} ago", WorldTime.MakeTimeDurationMessage(m_Actor.Location.Map.LocalTime.TurnCounter - percept.Turn));
      string text;
      if (percept.Percepted is Actor old_a)
        text = string.Format("I saw {0} {1} {2}.", old_a.Name, str1, str2);
      else if (percept.Percepted is Inventory inventory) {
        if (inventory.IsEmpty) return null;
        Item it = game.Rules.DiceRoller.Choose(inventory.Items);
        if (!IsItemWorthTellingAbout(it)) return null;
        int num = actorAt1.FOVrange(map.LocalTime, Session.Get.World.Weather);
        if ((double) Rules.StdDistance(percept.Location, actorAt1.Location) <= (double) (2 + num)) return null;
        text = string.Format("I saw {0} {1} {2}.", it.AName, str1, str2);
      } else if (percept.Percepted is string str3) {
        text = string.Format("I heard {0} {1} {2}!", str3, str1, str2);
      } else throw new InvalidOperationException("unhandled percept.Percepted type");
      return new ActionSay(m_Actor, actorAt1, text, RogueGame.Sayflags.NONE);
    }

    protected ActionCloseDoor BehaviorCloseDoorBehindMe(Location previousLocation)
    {
      if (!(previousLocation.MapObject is DoorWindow door)) return null;
      if (!Rules.IsAdjacent(previousLocation,m_Actor.Location) || !m_Actor.CanClose(door)) return null;
      foreach(var pt in previousLocation.Position.Adjacent()) {
        Actor actor = previousLocation.Map.GetActorAtExt(pt);
        if (null == actor) continue;
        if (actor.Controller is ObjectiveAI ai) {
          Dictionary<Point, int> tmp = ai.MovePlanIf(actor.Location.Position);
          if (tmp?.ContainsKey(previousLocation.Position) ?? false) return null;
        }
      }
      return new ActionCloseDoor(m_Actor, door, true);
    }

    private ActorAction BehaviorSecurePerimeter()
    {
      Map map = m_Actor.Location.Map;
      var want_to_resolve = new Dictionary<Point,int>();
      foreach (Point position in m_Actor.Controller.FOV) {
        if (!(map.GetMapObjectAt(position) is DoorWindow door)) continue;
        if (door.IsOpen && m_Actor.CanClose(door)) {
          if (Rules.IsAdjacent(position, m_Actor.Location.Position)) {
            // this can trigger close-open loop with someone who is merely traveling
            // check for duplicating last action
            if (null!=Goal<Goal_LastAction<ActionCloseDoor>>(o => o.LastAction.Door == door)) {
              // break action loop; plausibly the conflicting actor will be in the doorway next time
              return new ActionWait(m_Actor);
            }
            // proceed
            ActionCloseDoor tmp = new ActionCloseDoor(m_Actor, door, m_Actor.Location == PrevLocation);
            Objectives.Add(new Goal_LastAction<ActionCloseDoor>(m_Actor.Location.Map.LocalTime.TurnCounter,m_Actor,tmp));
            return tmp;
          }
          want_to_resolve[position] = Rules.GridDistance(position, m_Actor.Location.Position);
        }
        if (door.IsWindow && !door.IsBarricaded && m_Actor.CanBarricade(door)) {
          if (Rules.IsAdjacent(position, m_Actor.Location.Position))
            return new ActionBarricadeDoor(m_Actor, door);
          want_to_resolve[position] = Rules.GridDistance(position, m_Actor.Location.Position);
        }
      }
      // we could floodfill this, of course -- but everything is in LoS so try something else
      // we want to head for a nearest objective in such a way that the distance to all of the other objectives is minimized
      return BehaviorEfficientlyHeadFor(want_to_resolve);
    }

    protected ActionShout BehaviorWarnFriends(List<Percept> friends, Actor nearestEnemy)
    {
#if DEBUG
      if (null == nearestEnemy) throw new ArgumentNullException(nameof(nearestEnemy));
#endif
      if (Rules.IsAdjacent(m_Actor.Location, nearestEnemy.Location)) return null;
      if (m_Actor.HasLeader && m_Actor.Leader.IsSleeping) return new ActionShout(m_Actor);
      foreach (Percept friend in friends) {
        if (!(friend.Percepted is Actor actor)) throw new ArgumentException("percept not an actor");
        if (actor != m_Actor && (actor.IsSleeping && !m_Actor.IsEnemyOf(actor)) && actor.IsEnemyOf(nearestEnemy)) {
          return new ActionShout(m_Actor, string.Format("Wake up {0}! {1} sighted!", actor.Name, nearestEnemy.Name));
        }
      }
      return null;
    }

    protected ActorAction BehaviorDontLeaveFollowersBehind(int distance, out Actor target)
    {
      target = null;
      // Scan the group:
      // - Find farthest member of the group.
      // - If at least half the group is close enough we consider the group cohesion to be good enough and do nothing.
      int halfGroup = m_Actor.CountFollowers / 2;
	  if (0 >= halfGroup) return null;	// automatic do nothing(!)
      int worstDist = Int32.MinValue;
      int closeCount = 0;

      foreach (Actor follower in m_Actor.Followers) {
        int dist = Rules.InteractionDistance(follower.Location,m_Actor.Location);
        if (dist <= distance && ++closeCount >= halfGroup) return null;
        if (dist > worstDist) {
          target = follower;
          worstDist = dist;
        }
      }
      if (target == null) return null;
      return BehaviorPathToAdjacent(target.Location);
    }

    protected ActorAction BehaviorLeadActor(Percept target)
    {
      Actor target1 = target.Percepted as Actor;
      if (!m_Actor.CanTakeLeadOf(target1)) return null;
      if (Rules.IsAdjacent(m_Actor.Location, target1.Location)) return new ActionTakeLead(m_Actor, target1);
      // need an after-action "hint" to the target on where/who to go to
      if (!m_Actor.WillActAgainBefore(target1) && null == (target1.Controller as OrderableAI)?.Goal<Goal_HintPathToActor>()) {
        int t0 = Session.Get.WorldTime.TurnCounter+m_Actor.HowManyTimesOtherActs(1,target1)-(m_Actor.IsBefore(target1) ? 1 : 0);
        (target1.Controller as OrderableAI)?.Objectives.Insert(0,new Goal_HintPathToActor(t0, target1, m_Actor));    // AI disallowed from leading player so fine
      }
      return BehaviorIntelligentBumpToward(target1.Location, false, false);
    }

    protected ActionUseItem BehaviorUseMedecine(int factorHealing, int factorStamina, int factorSleep, int factorCure, int factorSan)
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory?.IsEmpty ?? true) return null;
      // \todo should be less finicky about SLP/Inf/SAN when enemies in sight
      bool needHP = m_Actor.HitPoints < m_Actor.MaxHPs;
      bool needSTA = m_Actor.IsTired;
      bool needSLP = m_Actor.WouldLikeToSleep;
      bool needCure = m_Actor.Infection > 0;
      bool needSan = m_Actor.Model.Abilities.HasSanity && m_Actor.Sanity < 3*m_Actor.MaxSanity/4;
      if (!needHP && !needSTA && (!needSLP && !needCure) && !needSan) return null;
      // XXX \todo following does not handle bandaids vs. medikit properly at low hp deficits
      ChoiceEval<ItemMedicine> choiceEval = Choose(inventory.GetItemsByType<ItemMedicine>(), it =>
      {
        int num = 0;
        if (needHP) num += factorHealing * it.Healing;
        if (needSTA) num += factorStamina * it.StaminaBoost;
        if (needSLP) num += factorSleep * it.SleepBoost;
        if (needCure) num += factorCure * it.InfectionCure;
        if (needSan) num += factorSan * it.SanityCure;
        return (float) num;
      }, (a, b) => a > b);
      if (choiceEval == null || (double) choiceEval.Value <= 0.0) return null;
      return new ActionUseItem(m_Actor, choiceEval.Choice); // legal only for OrderableAI
    }

    protected override ActorAction BehaviorChargeEnemy(Percept target, bool canCheckBreak, bool canCheckPush)
    {
      Actor actor = target.Percepted as Actor;
      ActorAction tmpAction = BehaviorMeleeAttack(actor);
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (null != tmpAction) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location)) {
          var tmp = m_Actor.BestMeleeAttack(actor);
#if DEBUG
          tmpAction = DoctrineRecoverSTA(Actor.STAMINA_MIN_FOR_ACTIVITY + Rules.STAMINA_COST_MELEE_ATTACK + tmp.StaminaPenalty);
          if (null == tmpAction) throw new ArgumentNullException(nameof(tmpAction));
          return tmpAction;
#else
          return DoctrineRecoverSTA(Actor.STAMINA_MIN_FOR_ACTIVITY + Rules.STAMINA_COST_MELEE_ATTACK + tmp.StaminaPenalty);
#endif
        }
      // we want to allow moving *away* from the target if that unblocks lines of fire from allies (but keep in mind short-term memory overload)
      // should be more careful about damaging traps
        Func<Point,Point,float> close_in = (ptA,ptB) => {
          if (ptA == ptB) return 0.0f;
          if (0 < m_Actor.Location.Map.TrapsMaxDamageAtFor(ptA, m_Actor)) return float.NaN; // just cancel the move
          return (float)Rules.StdDistance(ptA, ptB);
        };
        var friends = friends_in_FOV;
        if (null != friends) {
          List<List<Point>> LoF = null;
          foreach(var x in friends) {
            var rw = (x.Value.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo();
            if (null == rw) continue;
            // contrafactual fire test, with the best rw rather than the equipped rw (if any)
            if (rw.Model.Attack.Range < Rules.GridDistance(x.Key, target.Location)) continue;
            var line = new List<Point>();
            if (!LOS.CanTraceHypotheticalFireLine(x.Key, target.Location, rw.Model.Attack.Range, x.Value, line)) continue;
            (LoF ?? (LoF = new List<List<Point>>())).Add(line);
          }
          if (null != LoF) {
            close_in = close_in.Postprocess((ptA,ptB,dist) => {
                foreach (var line in LoF) {
                    if (line.Contains(ptA)) dist += 1;
                }
                return dist;
            });
          }
        }
        tmpAction = BehaviorBumpToward(target.Location, canCheckBreak, canCheckPush, close_in); // end inlining modifications to intelligent bumping toward
        if (null == tmpAction) return null;
        if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) m_Actor.Run();
        return tmpAction;
      } finally {
        if (null != tmpAction) {
          m_Actor.Activity = Activity.FIGHTING;
          m_Actor.TargetActor = actor;
        }
      }
    }

    private HashSet<Point> AlliesNeedLoFvs(Actor enemy)
    {
      var friends = m_Actor.Controller.friends_in_FOV;
      if (null == friends) return null;
      var ret = new HashSet<Point>();
      var LoF = new List<Point>();
      foreach(var friend_where in friends) {
        var rw = (friend_where.Value.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo();
        if (null == rw) continue;
        if (!friend_where.Value.IsEnemyOf(enemy)) continue;
        // XXX not quite right (should use best reloadable weapon)
        if (friend_where.Value.CanFireAt(enemy,LoF)) {
          ret.UnionWith(LoF);
          continue;
        }
        int range = rw.Model.Attack.Range;
        // XXX not quite right (visibiblity range check omitted)
        if (LOS.CanTraceViewLine(friend_where.Key, enemy.Location, range, LoF)) {
          ret.UnionWith(LoF);
          continue;
        }
      }
      return (0 < ret.Count ? ret : null);
    }

#if DEAD_FUNC
    public ActorAction BehaviorWalkAwayFrom(IEnumerable<Point> goals, HashSet<Point> LoF_reserve)
    {
      Actor leader = m_Actor.LiveLeader;
      var leader_rw = (null != leader ? leader.GetEquippedWeapon() as ItemRangedWeapon : null);
      Actor actor = (null != leader_rw ? GetNearestTargetFor(m_Actor.Leader) : null);
      bool checkLeaderLoF = actor != null && actor.Location.Map == m_Actor.Location.Map;    // XXX \todo cross-map conversion
      List<Point> leaderLoF = null;
      if (checkLeaderLoF) {
        leaderLoF = new List<Point>(1);
        LOS.CanTraceFireLine(leader.Location, actor.Location, leader_rw.Model.Attack.Range, leaderLoF);
      }
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location location = m_Actor.Location + dir;
        if (!IsValidFleeingAction(Rules.IsBumpableFor(m_Actor, location))) return float.NaN;
        float num = SafetyFrom(location.Position, goals);
        if (LoF_reserve?.Contains(location.Position) ?? false) --num;
        if (null != leader) {
          num -= (float)Rules.StdDistance(location, leader.Location);
          if (leaderLoF?.Contains(location.Position) ?? false) num -= (float)IN_LEADER_LOF_SAFETY_PENALTY;
        }
        return num;
      }, (a, b) => a > b);
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }
#endif

    public ActorAction BehaviorWalkAwayFrom(IEnumerable<Location> goals, HashSet<Point> LoF_reserve)
    {
      Actor leader = m_Actor.LiveLeader;
      var leader_rw = (null != leader ? leader.GetEquippedWeapon() as ItemRangedWeapon : null);
      Actor actor = (null != leader_rw ? GetNearestTargetFor(m_Actor.Leader) : null);
      bool checkLeaderLoF = actor != null && actor.Location.Map == m_Actor.Location.Map;    // XXX \todo cross-map conversion
      List<Point> leaderLoF = null;
      if (checkLeaderLoF) {
        leaderLoF = new List<Point>(1);
        LOS.CanTraceFireLine(leader.Location, actor.Location, leader_rw.Model.Attack.Range, leaderLoF);
      }
      ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir => {
        Location location = m_Actor.Location + dir;
        if (!IsValidFleeingAction(Rules.IsBumpableFor(m_Actor, location))) return float.NaN;
        float num = SafetyFrom(location, goals);
        if (LoF_reserve?.Contains(location.Position) ?? false) --num;
        if (null != leader) {
          num -= (float)Rules.StdDistance(location, leader.Location);
          if (leaderLoF?.Contains(location.Position) ?? false) num -= (float)IN_LEADER_LOF_SAFETY_PENALTY;
        }
        return num;
      }, (a, b) => a > b);
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    private ActorAction BehaviorFlee(Actor enemy, HashSet<Point> LoF_reserve, bool doRun, string[] emotes)
    {
      var game = RogueForm.Game;
      ActorAction tmpAction = null;
      if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
        game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[0], enemy.Name));
        // All OrderableAI instances currently can both use map objects, and barricade
        // there is an inventory check requirement on barricading as well
        // due to preconditions it is mutually exclusive that a door be closable or barricadable
        // however, we do not want to obstruct line of fire of allies
        {
        bool could_barricade = m_Actor.CouldBarricade();
        var close_doors = new Dictionary<Point,DoorWindow>();
        var barricade_doors = new Dictionary<Point,DoorWindow>();
        foreach(Point pt in Direction.COMPASS.Select(dir => m_Actor.Location.Position + dir)) {
          if (LoF_reserve?.Contains(pt) ?? false) continue;
          if (!(m_Actor.Location.Map.GetMapObjectAt(pt) is DoorWindow door)) continue;
          if (!IsBetween(m_Actor.Location.Position, pt, enemy.Location.Position)) continue;
          if (m_Actor.CanClose(door)) {
            if ((!Rules.IsAdjacent(pt, enemy.Location.Position) || !enemy.CanClose(door))) close_doors[pt] = door;
          } else if (could_barricade && door.CanBarricade()) {
            barricade_doors[pt] = door;
          }
        }
        if (0 < close_doors.Count) {
          var dest = game.Rules.DiceRoller.Choose(close_doors);
          Objectives.Insert(0,new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, dest.Value.Location));
          return new ActionCloseDoor(m_Actor, dest.Value, m_Actor.Location == PrevLocation);
        } else if (0 < barricade_doors.Count) {
          var dest = game.Rules.DiceRoller.Choose(barricade_doors);
          Objectives.Insert(0,new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, dest.Value.Location));
          return new ActionBarricadeDoor(m_Actor, dest.Value);
        }
        }   // enable automatic GC
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
        // XXX we should run for the exit here ...
        if (!_damage_field?.ContainsKey(m_Actor.Location.Position) ?? true) {
          tmpAction = BehaviorUseMedecine(2, 2, 1, 0, 0);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        // XXX or run for the exit here
        tmpAction = (null!= m_Actor.Controller.enemies_in_FOV ? BehaviorWalkAwayFrom(m_Actor.Controller.enemies_in_FOV.Keys, LoF_reserve) : null);
        if (null != tmpAction) {
          if (doRun) m_Actor.Run();
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_TRAPPED_CHANCE))
            game.DoEmote(m_Actor, emotes[1], true);
          return BehaviorMeleeAttack(enemy);
        }
        return null;
    }

    // sunk from BaseAI
    protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, ActorCourage courage, string[] emotes, RouteFinder.SpecialActions allowedChargeActions)
    {
#if DEBUG
      if (_blast_field?.Contains(m_Actor.Location.Position) ?? false) throw new InvalidOperationException("should not reach BehaviorFightFlee when in blast field");
#endif
      // it is possible to reach here with a ranged weapon.  (Char Office assault, for instance)  In this case, we don't have Line of Fire.
      if (null != GetBestRangedWeaponWithAmmo()) return null;

      List<Point> legal_steps = _legal_steps; // XXX working reference due to following postprocessing
      if (null != _blast_field && null != legal_steps) {
        IEnumerable<Point> test = legal_steps.Where(pt => !_blast_field.Contains(pt));
        legal_steps = (test.Any() ? test.ToList() : null);
      }

      // this needs a serious rethinking; dashing into an ally's line of fire is immersion-breaking.
      Percept target = FilterNearest(enemies);  // may not be enemies[0] due to this using StdDistance rather than GridDistance
      Actor enemy = target.Percepted as Actor;

      bool doRun = false;	// only matters when fleeing
      bool decideToFlee = (null != legal_steps);
      if (decideToFlee) {
        if (enemy.HasEquipedRangedWeapon()) decideToFlee = false;
        else if (m_Actor.Model.Abilities.IsLawEnforcer && enemy.MurdersCounter > 0)
          decideToFlee = false;
        else if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, enemy.Location))
          decideToFlee = true;
        else if (m_Actor.Leader != null && ActorCourage.COURAGEOUS == courage) {
	      decideToFlee = false;
        } else {
          switch (courage) {
            case ActorCourage.COWARD:
              decideToFlee = true;
              doRun = true;
              break;
            case ActorCourage.CAUTIOUS:
            case ActorCourage.COURAGEOUS:
              decideToFlee = WantToEvadeMelee(m_Actor, courage, enemy);
              doRun = !HasSpeedAdvantage(m_Actor, enemy);
              break;
            default:
              throw new ArgumentOutOfRangeException("unhandled courage");
          }
        }
        if (!decideToFlee && WillTireAfterAttack(m_Actor)) {
          decideToFlee = true;    // but do not run as otherwise we won't build up stamina
        }
      }

      // alpha10
      // Improve STA management a bit.
      // Cancel running if this would make us tired and is not a free move
      if (doRun && WillTireAfterRunning(m_Actor) && !m_Actor.RunIsFreeMove) doRun = false;

      // alpha10 
      // If we have no ranged weapon and target is unreachable, no point charging him as we can't get into
      // melee contact. Flee if enemy has equipped a ranged weapon and do nothing if not.
      if (!decideToFlee) {
//      if (null == GetBestRangedWeaponWithAmmo())  {   // call contract
           // check route
           if (!CanReachSimple(enemy.Location, allowedChargeActions)) {
             if (null == (enemy.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo()) return null;  // no ranged weapon, unreachable: harmless?
             decideToFlee = true;   // get out of here, now
           }
//      }
      }

      var LoF_reserve = AlliesNeedLoFvs(enemy);
      ActorAction tmpAction = null;
      if (decideToFlee) {
        tmpAction = BehaviorFlee(enemy, LoF_reserve, doRun, emotes);
        if (null != tmpAction) return tmpAction;
      }

      List<Percept> approachable_enemies = enemies.Where(p => Rules.IsAdjacent(m_Actor.Location, p.Location)).ToList();

      if (0 >= approachable_enemies.Count) {
        if (null != legal_steps) {
          // nearest enemy is not adjacent.  Filter by whether it's legal to approach.
          approachable_enemies = enemies.Where(p => {
            int dist = Rules.GridDistance(m_Actor.Location,p.Location);
            return legal_steps.Any(pt => dist>Rules.GridDistance(new Location(m_Actor.Location.Map,pt),p.Location));
          }).ToList();
          if (0 >= approachable_enemies.Count) approachable_enemies = null;
        }
      }

      // if enemy is not approachable then following checks are invalid
      if (!approachable_enemies?.Contains(target) ?? true) return new ActionWait(m_Actor);

      // redo the pause check
      if (m_Actor.Speed > enemy.Speed && 2 == Rules.GridDistance(m_Actor.Location, target.Location)) {
          if (   !m_Actor.WillActAgainBefore(enemy)
              || !m_Actor.RunIsFreeMove)    // XXX assumes eneumy wants to close
            return new ActionWait(m_Actor);
          if (null != legal_steps) {
            // cannot close at normal speed safely; run-hit may be ok
            var dash_attack = new Dictionary<Point,ActorAction>();
            ReserveSTA(0,1,0,0);  // reserve stamina for 1 melee attack
            List<Point> attack_possible = legal_steps.Where(pt => Rules.IsAdjacent(pt,enemy.Location.Position)
              && !(LoF_reserve?.Contains(pt) ?? false)
              && (dash_attack[pt] = Rules.IsBumpableFor(m_Actor,new Location(m_Actor.Location.Map,pt))) is ActionMoveStep
              && RunIfAdvisable(pt)).ToList();
            ReserveSTA(0,0,0,0);  // baseline
            if (!attack_possible.Any()) return new ActionWait(m_Actor);
            // XXX could filter down attack_possible some more
            m_Actor.IsRunning = true;
            return dash_attack[game.Rules.DiceRoller.Choose(attack_possible)];
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
    
    public ActorAction WouldUseAccessibleStack(Location dest,bool is_real=false) {
        Dictionary<Point, Inventory> stacks = dest.Map.GetAccessibleInventories(dest.Position);
        if (0 < (stacks?.Count ?? 0)) {
          foreach(var x in stacks) {
            Location? loc = (dest.Map.IsInBounds(x.Key) ? new Location(dest.Map, x.Key) : dest.Map.Normalize(x.Key));
            if (null == loc) throw new ArgumentNullException(nameof(loc));
            ActorAction tmpAction = WouldGrabFromAccessibleStack(loc.Value, x.Value, is_real);
            if (null != tmpAction) return tmpAction;
          }
        }
        return null;
    }

    public ActorAction BehaviorUseAdjacentStack() { return WouldUseAccessibleStack(m_Actor.Location, true); }

	protected ActorAction BehaviorPathTo(Location dest)
	{
      var tmp = BehaviorUseAdjacentStack();
      if (null != tmp) return tmp;

      if (dest.Map!=m_Actor.Location.Map) {
        return BehaviorPathTo(m => (m==dest.Map ? new HashSet<Point> { dest.Position } : new HashSet<Point>()));
      }

      return BehaviorNavigate(new HashSet<Point> { dest.Position });
	}

	protected ActorAction BehaviorPathToAdjacent(Location dest)
	{
      var tmp = BehaviorUseAdjacentStack();
      if (null != tmp) return tmp;

      var range = m_Actor.OnePathRange(dest);
      if (null == range) return null;
      range.OnlyIf((Predicate<ActorAction>)(action => (action is ActionMoveStep || action is ActionPush || action is ActionOpenDoor || action is ActionUseExit) && !VetoAction(action)));  // only allow actions that prefigure moving to destination quickly and politely
      if (0 >= range.Count) return null;
      // start function target
      var adjacent = m_Actor.OnePathRange(m_Actor.Location);
      if (null == adjacent) return null;
      adjacent.OnlyIf((Predicate<ActorAction>)(action => (action is ActionMoveStep || action is ActionPush || action is ActionOpenDoor || action is ActionUseExit) && action.IsLegal() && !VetoAction(action)));  // only allow actions that prefigure moving to destination quickly
      if (0 >= adjacent.Count) return null;
      foreach(var x in range) {
        if (adjacent.TryGetValue(x.Key,out var act)) return act;
      }
      // end function target
      var init_costs = new Dictionary<Location,int>();
      foreach(var x in range) init_costs[x.Key] = 0;

      return BehaviorPathTo(PathfinderFor(init_costs,new HashSet<Map>()));
	}

    protected ActorAction BehaviorHangAroundActor(RogueGame game, Actor other, int minDist, int maxDist)
    {
      if (other?.IsDead ?? true) return null;
      Point otherPosition = other.Location.Position;
      int num = 0;
      Location loc;
      do {
        Point p = otherPosition;
        p.X += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        p.Y += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
        loc = new Location(other.Location.Map,p);
        if (100 < ++num) return null;
        if (loc == m_Actor.Location) return new ActionWait(m_Actor);    // XXX check what BehaviorIntelligentBumpToward does
        if (!loc.IsWalkableFor(m_Actor)) continue;
      }
      while(Rules.GridDistance(loc,other.Location) < minDist);

	  ActorAction actorAction = BehaviorPathTo(loc);
      if (!actorAction?.IsLegal() ?? true) return null;
      if (actorAction is ActionMoveStep tmp) {
        if (Rules.GridDistance(m_Actor.Location, tmp.dest) > maxDist)
           m_Actor.IsRunning = RunIfAdvisable(tmp.dest);
	  }
      return actorAction;
    }

    private bool InProximity(Location src, Location dest, int maxDist)
    {  // due to definitions, for maxDist=1 the other two tests are implied by the GridDistance test for m_Actor.Location
       return Rules.GridDistance(src, dest) <= maxDist
           && (1 >= maxDist || (CanSee(dest) && null != m_Actor.MinStepPathTo(src, dest)));
    }

    protected override ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other?.IsDead ?? true) return null;
      if (InProximity(m_Actor.Location, other.Location, maxDist)) {
          RecordCloseToActor(other, maxDist);
          return new ActionWait(m_Actor);
      }
	  ActorAction actorAction = BehaviorPathToAdjacent(other.Location);
      if (!actorAction?.IsLegal() ?? true) return null;
      if (actorAction is ActionMoveStep tmp) {
        if (  Rules.GridDistance(m_Actor.Location.Position, tmp.dest.Position) > maxDist
           || other.Location.Map != m_Actor.Location.Map)
           m_Actor.IsRunning = RunIfAdvisable(tmp.dest);
	  }
      return actorAction;
    }

    // belongs with CivilianAI, or possibly OrderableAI but NatGuard may not have access to the crime listings
    protected ActorAction BehaviorEnforceLaw()
    {
#if DEBUG
      if (!m_Actor.Model.Abilities.IsLawEnforcer) throw new InvalidOperationException("!m_Actor.Model.Abilities.IsLawEnforcer");
#endif
      // XXX this should affect reputation
      var friends = friends_in_FOV;
      if (null == friends) return null;
      Dictionary<Location,Actor> murderers = null;
      foreach(var x in friends) {
        if (0 >= x.Value.MurdersCounter) continue;
        (murderers ?? (murderers = new Dictionary<Location, Actor>()))[x.Key] = x.Value;
      }
      if (null == murderers) return null;
      RogueGame game = RogueForm.Game;
      if (!game.Rules.RollChance(LAW_ENFORCE_CHANCE)) return null;
      friends = null;  // enable auto GC
      foreach(var x in murderers) {
        if (game.Rules.RollChance(Rules.ActorUnsuspicousChance(m_Actor, x.Value))) game.DoEmote(x.Value, string.Format("moves unnoticed by {0}.", m_Actor.Name));
        else (friends ?? new Dictionary<Location, Actor>())[x.Key] = x.Value;
      }
      if (null == friends) return null;
      // at this point, entries in friends are murderers that have elicited suspicion
      foreach(var x in friends) {
        game.DoEmote(m_Actor, string.Format("takes a closer look at {0}.", x.Value.Name));
        if (!game.Rules.RollChance(Rules.ActorSpotMurdererChance(m_Actor, x.Value))) continue;
        // XXX \todo V.0.10.0 this needs a rethinking (a well-armed murderer may be of more use killing z, a weak one should be assassinated)
        game.DoMakeAggression(m_Actor, x.Value);
        m_Actor.TargetActor = x.Value;
        // players are special: they get to react to this first
        return new ActionSay(m_Actor, x.Value, string.Format("HEY! YOU ARE WANTED FOR {0}!", "murder".QtyDesc(x.Value.MurdersCounter).ToUpper()), (x.Value.IsPlayer ? RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER : RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER | RogueGame.Sayflags.IS_FREE_ACTION));
      }
      return null;
    }

    protected ActorAction BehaviorBuildLargeFortification(RogueGame game, int startLineChance)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < m_Actor.BarricadingMaterialNeedForFortification(true)) return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || map.IsOnMapBorder(point) || map.HasActorAt(point) || (map.HasExitAt(point) || map.IsInsideAt(point)))
          return false;
        int num1 = map.CountAdjacentTo(point, ptAdj => !map.GetTileModelAt(ptAdj).IsWalkable); // allows IsInBounds above
        int num2 = map.CountAdjacent<Fortification>(point, fortification => !fortification.IsTransparent);
        return (num1 == 3 && num2 == 0 && game.Rules.RollChance(startLineChance)) || (num1 == 0 && num2 == 1);
      }, dir => game.Rules.Roll(0, 666), (a, b) => a > b);
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!m_Actor.CanBuildFortification(point1, true)) return null;
      return new ActionBuildFortification(m_Actor, point1, true);
    }

    protected static bool IsDoorwayOrCorridor(Map map, Point pos)
    { // all of these can use IsInBounds
      if (!map.GetTileModelAt(pos).IsWalkable) return false;
      Point p5 = pos + Direction.NE;
      bool flag_ne = map.IsInBounds(p5) && !map.GetTileModelAt(p5).IsWalkable;
      Point p6 = pos + Direction.NW;
      bool flag_nw = map.IsInBounds(p6) && !map.GetTileModelAt(p6).IsWalkable;
      Point p7 = pos + Direction.SE;
      bool flag_se = map.IsInBounds(p7) && !map.GetTileModelAt(p7).IsWalkable;
      Point p8 = pos + Direction.SW;
      bool flag_sw = map.IsInBounds(p8) && !map.GetTileModelAt(p8).IsWalkable;
      bool no_corner = !flag_ne && !flag_se && !flag_nw && !flag_sw;
      if (!no_corner) return false;

      Point p1 = pos + Direction.N;
      bool flag_n = map.IsInBounds(p1) && !map.GetTileModelAt(p1).IsWalkable;
      Point p2 = pos + Direction.S;
      bool flag_s = map.IsInBounds(p2) && !map.GetTileModelAt(p2).IsWalkable;
      Point p3 = pos + Direction.E;
      bool flag_e = map.IsInBounds(p3) && !map.GetTileModelAt(p3).IsWalkable;
      Point p4 = pos + Direction.W;
      bool flag_w = map.IsInBounds(p4) && !map.GetTileModelAt(p4).IsWalkable;
      return (flag_n && flag_s && !flag_e && !flag_w) || (flag_e && flag_w && !flag_n && !flag_s);
    }

    protected ActorAction BehaviorBuildSmallFortification(RogueGame game)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < m_Actor.BarricadingMaterialNeedForFortification(false)) return null;
      Map map = m_Actor.Location.Map;
      BaseAI.ChoiceEval<Direction> choiceEval = Choose(Direction.COMPASS, dir =>
      {
        Point point = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(point) || !map.IsWalkable(point) || map.IsOnMapBorder(point) || map.HasActorAt(point) || map.HasExitAt(point))
          return false;
        return IsDoorwayOrCorridor(map, point); // this allows using IsInBounds rather than IsValid
      }, dir => game.Rules.Roll(0, 666), (a, b) => a > b);
      if (choiceEval == null) return null;
      Point point1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!m_Actor.CanBuildFortification(point1, false)) return null;
      return new ActionBuildFortification(m_Actor, point1, false);
    }

    /// <returns>0 for disallowed, 1 for allowed, 2+ for "better".</returns>
    protected int SleepLocationRating(Location loc)
    {
      // the legacy tests
      MapObject obj = loc.Map.GetMapObjectAtExt(loc.Position);
      if (obj is DoorWindow) return -1;  // contextual; need to be aware of doors
      if (loc.Map.AnyAdjacentExt<DoorWindow>(loc.Position)) return 0;
      if (loc.Map.HasExitAtExt(loc.Position)) return 0;    // both unsafe, and problematic for pathing in general
      if (m_Actor.Location!=loc && loc.Map.HasActorAt(loc.Position)) return 0;  // contextual
      if (obj?.IsCouch ?? false) return 1;  // jail cells are ok even though their geometry is bad

      bool wall_at(Point pt) { return loc.Map.IsValid(pt) ? !loc.Map.GetTileModelAtExt(pt).IsWalkable : true; } // invalid is impassable so acts like a wall

      // geometric code (walls, etc)
      if (!loc.Map.IsInsideAtExt(loc.Position)) return 0;
      if (!loc.Map.GetTileModelAtExt(loc.Position).IsWalkable) return 0;
      // we don't want to sleep next to anything that looks like an ex-door
      bool[] walls = new bool[Direction.COMPASS.Length];
      foreach(Direction dir in Direction.COMPASS) walls[dir.Index] = wall_at(loc.Position + dir);

      // reference code...likely not optimal, but easy to verify
      if (walls[Direction.N.Index] && walls[Direction.S.Index]) return 0;
      if (walls[Direction.W.Index] && walls[Direction.E.Index]) return 0;
      if (walls[Direction.N.Index] && walls[Direction.NW.Index] != walls[Direction.NE.Index]) return 0;
      if (walls[Direction.S.Index] && walls[Direction.SW.Index] != walls[Direction.SE.Index]) return 0;
      if (walls[Direction.W.Index] && walls[Direction.NW.Index] != walls[Direction.SW.Index]) return 0;
      if (walls[Direction.E.Index] && walls[Direction.NE.Index] != walls[Direction.SE.Index]) return 0;
      if (!walls[Direction.N.Index] && walls[Direction.NW.Index] && walls[Direction.NE.Index]) return 0;
      if (!walls[Direction.S.Index] && walls[Direction.SW.Index] && walls[Direction.SE.Index]) return 0;
      if (!walls[Direction.W.Index] && walls[Direction.NW.Index] && walls[Direction.SW.Index]) return 0;
      if (!walls[Direction.E.Index] && walls[Direction.NE.Index] && walls[Direction.SE.Index]) return 0;

      if (!walls[Direction.N.Index] && walls[Direction.NW.Index] && wall_at(loc.Position + Direction.NW + Direction.W)) return 0;
      if (!walls[Direction.W.Index] && walls[Direction.NW.Index] && wall_at(loc.Position + Direction.NW + Direction.N)) return 0;
      if (!walls[Direction.S.Index] && walls[Direction.SW.Index] && wall_at(loc.Position + Direction.SW + Direction.W)) return 0;
      if (!walls[Direction.W.Index] && walls[Direction.SW.Index] && wall_at(loc.Position + Direction.SW + Direction.S)) return 0;
      if (!walls[Direction.N.Index] && walls[Direction.NE.Index] && wall_at(loc.Position + Direction.NE + Direction.E)) return 0;
      if (!walls[Direction.E.Index] && walls[Direction.NE.Index] && wall_at(loc.Position + Direction.NE + Direction.N)) return 0;
      if (!walls[Direction.S.Index] && walls[Direction.SE.Index] && wall_at(loc.Position + Direction.SE + Direction.E)) return 0;
      if (!walls[Direction.E.Index] && walls[Direction.SE.Index] && wall_at(loc.Position + Direction.SE + Direction.S)) return 0;

      // contextual code.  Context-free version would return int.MaxValue to request this.  static value would asusmed "passed"
      // [handled elsewhere] if the LOS has a non-broken door then we're on the wrong side.  We should be pathing to it, but *not* securing the perimter.
      // XXX \todo treat already-sleeping as "somewhat like a wall"...want to be very sure it is possible to leave without waking anyone up
      // more geometric code (approval rather than veto)
      return 1;
    }

    private Dictionary<Point, int> GetSleepLocsInLOS(out Dictionary<Point, int> couches, out Dictionary<Point,int> doors)
    {
      couches = new Dictionary<Point,int>();
      doors = new Dictionary<Point, int>();
      var ret = new Dictionary<Point,int>();
      foreach(Point pt in m_Actor.Controller.FOV) {
        int rating = SleepLocationRating(new Location(m_Actor.Location.Map, pt));
        if (-1 == rating) {
          // use the same delta-encoding system that Map::Normalize does
          int delta = 0;
          if (m_Actor.Location.Position.X<pt.X) delta += 1;
          else if (m_Actor.Location.Position.X > pt.X) delta -= 1;
          if (m_Actor.Location.Position.Y < pt.Y) delta += 3;
          else if (m_Actor.Location.Position.Y > pt.Y) delta -= 3;
          doors[pt] = delta;
        }
        if ( 0 >= rating) continue;
        int dist = Rules.GridDistance(m_Actor.Location.Position, pt);
        ret[pt] = dist;
        if (m_Actor.Location.Map.GetMapObjectAt(pt)?.IsCouch ?? false) couches[pt] = dist;
      }
      return ret;
    }

    private ActorAction BehaviorSleep(Dictionary<Point,int> sleep_locs, Dictionary<Point,int> couches)
    {
#if DEBUG
      if (0 >= (sleep_locs?.Count ?? 0)) throw new ArgumentNullException(nameof(sleep_locs));
      if (null == couches) throw new ArgumentNullException(nameof(couches));
#endif
      if (!m_Actor.CanSleep()) return null;
      // Do not sleep next to a door/window
      if (0>=SleepLocationRating(m_Actor.Location)) {
        return BehaviorEfficientlyHeadFor(0<couches.Count ? couches : sleep_locs);  // null return ok here?
      }
      if (!m_Actor.IsOnCouch) { // head for a couch if in plain sight
        ActorAction tmpAction = BehaviorEfficientlyHeadFor(couches);
        if (null != tmpAction) return tmpAction;
      }

      // all battery powered items other than the police radio are left hand, currently
      // the police radio is DollPart.HIP_HOLSTER, *but* it recharges on movement faster than it drains
      Item it = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
      if (it is BatteryPowered) RogueForm.Game.DoUnequipItem(m_Actor, it);
      return new ActionSleep(m_Actor);
    }

    private ActorAction BehaviorNavigateToSleep(Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> item_memory)
    {
#if DEBUG
        if (null == item_memory) throw new ArgumentNullException(nameof(item_memory));
#endif
        bool known_bed(Location loc) {  // XXX depending on incoming events this may not be conservative enough
            if (!loc.Map.IsInsideAt(loc.Position)) return false;
            if (!item_memory.HaveEverSeen(loc, out int when)) return false;
            // be as buggy as the display, which shows objects in their current positions
            if (!(loc.MapObject?.IsCouch ?? false)) return false;
            return !(loc.Actor?.IsSleeping ?? false);   // cheat: bed should not have someone already sleeping in it
        }
        var navigate = m_Actor.Location.Map.PathfindLocSteps(m_Actor);
        // would like to pathfind to an indoors bed, but the heuristics for finding the goals are inefficient
        navigate.GoalDistance(known_bed, m_Actor.Location);
        return BehaviorPathTo(navigate);
    }

    protected ActorAction BehaviorNavigateToSleep()
    {
      var item_memory = m_Actor.Controller.ItemMemory;
      if (!m_Actor.IsInside) {
        if (null != item_memory) return BehaviorNavigateToSleep(item_memory);
        // XXX this is stymied by closed, opaque doors which logically have inside squares near them; also ex-doorways
        // ignore barricaded doors on residences (they have lots of doors).  Do not respect those in shops, subways, or (vintage) the sewer maintenance.
        // \todo replace by more reasonable foreach loop
        IEnumerable<Location> see_inside = FOV.Where(pt => m_Actor.Location.Map.GetTileAtExt(pt).IsInside && m_Actor.Location.Map.IsWalkableFor(pt,m_Actor)).Select(pt2 => new Location(m_Actor.Location.Map,pt2));
        return BehaviorHeadFor(see_inside, false, false);
      }

      if (null != item_memory) {
        // reject if the smallest zone containing this location does not have a bed
        Rectangle scan_this = m_Actor.Location.Map.Rect;
        var z_list = m_Actor.Location.Map.GetZonesAt(m_Actor.Location.Position);
        if (null != z_list) foreach(var z in z_list) {
          if (scan_this.Width < z.Bounds.Width) continue;
          if (scan_this.Height < z.Bounds.Height) continue;
          if (scan_this.Width > z.Bounds.Width || scan_this.Height > z.Bounds.Height) scan_this = z.Bounds;
        }

        bool has_free_bed = false;
        scan_this.DoForEach(pt => {   // XXX \todo define Any for a Rectangle
            Location loc = new Location(m_Actor.Location.Map, pt);
            if (!loc.Map.IsInsideAt(loc.Position)) return;
            // be as buggy as the display, which shows objects in their current positions
            if (!(loc.MapObject?.IsCouch ?? false)) return;
            if (!(loc.Actor?.IsSleeping ?? false)) has_free_bed = true;   // cheat: bed should not have someone already sleeping in it
        });
        if (!has_free_bed) return BehaviorNavigateToSleep(item_memory);
      }

      ActorAction tmpAction = null;
      Dictionary<Point, int> sleep_locs = GetSleepLocsInLOS(out Dictionary<Point,int> couches, out Dictionary<Point,int> doors);
      if (0 >= sleep_locs.Count) {
         // \todo we probably should be using full pathing to the nearest valid location anyway
         // XXX \todo exploration data available for *some* OrderableAI subclasses: is it useful here?
         return BehaviorWander(null, loc => loc.Map.IsInsideAtExt(loc.Position)); // XXX explore behavior would be better but that needs fixing
      }

      // \todo trigger secure perimeter if we have appropriate squares whose viewability is not blocked by doors
      if (0<doors.Count) {
        var beyond_door_sleep_locs = new Dictionary<Point,int>();
        var in_bounds = new HashSet<Point>(sleep_locs.Keys);

        foreach(var pos_type in doors) {
          foreach(Point pt in in_bounds.ToList()) {
            // beyond-X
            switch(pos_type.Value)
            {
            case -4:
            case -1:
            case  2:
               if (pos_type.Key.X>pt.X) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
                 continue;
               }
               break;
            case -2:
            case  1:
            case  4:
               if (pos_type.Key.X<pt.X) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
                 continue;
               }
              break;
            }
            // beyond-Y
            switch(pos_type.Value)
            {
            case -4:
            case -3:
            case -2:
               if (pos_type.Key.Y>pt.Y) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
               }
               break;
            case 2:
            case 3:
            case 4:
               if (pos_type.Key.Y<pt.Y) {
                 beyond_door_sleep_locs[pt] = sleep_locs[pt];
                 in_bounds.Remove(pt);
               }
               break;
            }
          }
//        if (0 >= in_bounds.Count) break;  // unclear whether micro-optimization or micro-pessimization
        }
        if (0 < beyond_door_sleep_locs.Count) {
          if (0 >= in_bounds.Count) return BehaviorEfficientlyHeadFor(beyond_door_sleep_locs);
          sleep_locs.OnlyIf(pt => in_bounds.Contains(pt));
        }
      }

      tmpAction = BehaviorSecurePerimeter();
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = BehaviorSleep(sleep_locs,couches);
      if (null != tmpAction) {
        if (tmpAction is ActionSleep) m_Actor.Activity = Activity.SLEEPING;
        return tmpAction;
      }
      return null;
    }

    protected ActorAction BehaviorRestIfTired()
    {
      if (m_Actor.StaminaPoints >= Actor.STAMINA_MIN_FOR_ACTIVITY) return null;
      return new ActionWait(m_Actor);
    }

    protected override ActorAction BehaviorExplore(ExplorationData exploration)
    {
      ActorCourage courage = Directives.Courage;
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position.X - PrevLocation.Position.X, m_Actor.Location.Position.Y - PrevLocation.Position.Y);
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
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
        const int AVOID_TRAPS = -1000; // alpha10 greatly increase penalty and x by potential damage; was -50
        const int EXPLORE_INOUT = 50;
        const int EXPLORE_DIRECTION = 25;
        const int EXPLORE_RANDOM = 10;

//      if (exploration.HasExplored(loc)) return float.NaN;
        Map map = loc.Map;
        Point position = loc.Position;
        int trap_max_damage = m_Actor.Model.Abilities.IsIntelligent ? map.TrapsMaxDamageAtFor(position,m_Actor) : 0;
        if (m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous && trap_max_damage >= m_Actor.HitPoints)
          return float.NaN;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position))) num += EXPLORE_ZONES;
        if (!exploration.HasExplored(loc)) num += EXPLORE_LOCS;
        MapObject mapObjectAt = map.GetMapObjectAt(position);
        // this is problematic when the door is the previous location.  Do not overwhelm in/out
        if (mapObjectAt != null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) {
          num += (loc != PrevLocation ? EXPLORE_BARRICADES : -EXPLORE_DIRECTION);
        }
        if (0<trap_max_damage) num += trap_max_damage*AVOID_TRAPS;
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

    public bool HasAnyInterestingItem(IEnumerable<Item> Items)
    {
#if DEBUG
      if (!Items?.Any() ?? true) throw new ArgumentNullException(nameof(Items));
#endif
      return Items.Any(it => IsInterestingItem(it));
    }

    public bool HasAnyInterestingItem(Inventory inv)
    {
#if DEBUG
      if (inv?.IsEmpty ?? true) throw new ArgumentNullException(nameof(inv));
#endif
      return HasAnyInterestingItem(inv.Items);
    }

    // XXX \todo replace/augment this behavior to generally take action to manage sanity
    // that is: entertainment (currently), medication, and chatting.
    // the use medicine behavior is used in combat so it should not be nearly as finicky about sanity as the non-combat management here
    protected ActorAction BehaviorUseEntertainment()
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory.IsEmpty) return null;
      ItemEntertainment itemEntertainment = inventory.GetFirst<ItemEntertainment>();
      if (itemEntertainment == null) return null;
      return (m_Actor.CanUse(itemEntertainment) ? new ActionUseItem(m_Actor, itemEntertainment) : null);
    }

#region stench killer
    // stench killer support -- don't want to lock down to the only user, CivilianAI
    // actually, this particular heuristic is *bad* because it causes the z to lose tracking too close to shelter.
    // with the new scent-suppressor mechaniics, the cutpoints are somewhat reasonable but extra distance/LoS breaking is needed
    private bool IsGoodStenchKillerSpot(Map map, Point pos)
    {
#if OBSOLETE
      if (map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, pos) > 0) return false;
#endif
      // 2. Spray in a good position:
      //    2.1 entering or leaving a building.
      if (PrevLocation.Map.IsInsideAt(PrevLocation.Position) != map.IsInsideAt(pos)) return true;
      //    2.3 an exit.
      if (map.HasExitAt(pos)) return true;
      //    2.2 a door/window.
      return map.GetMapObjectAt(pos) is DoorWindow;
    }

    protected ItemSprayScent GetEquippedStenchKiller()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item obj in m_Actor.Inventory.Items) {
        if (obj.IsEquipped && obj is ItemSprayScent spray && Odor.SUPPRESSOR == spray.Model.Odor)
          return spray;
      }
      return null;
    }

    protected ItemSprayScent GetFirstStenchKiller(Predicate<ItemSprayScent> fn)
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      return m_Actor.Inventory.GetFirstMatching<ItemSprayScent>(fn);
    }

    protected ActionSprayOdorSuppressor BehaviorUseStenchKiller()
    {
      if (!(m_Actor.GetEquippedItem(DollPart.LEFT_HAND) is ItemSprayScent spray)) return null;
      if (spray.IsUseless) return null;
      // alpha 10 redefined spray suppression to work on the odor source, not the odor
      // if not proper odor, nope.
      if (spray.Model.Odor != Odor.SUPPRESSOR) return null;

      // first check if wants to use it on self, then check on adj leader/follower
      Actor sprayOn = null;

      bool WantsToSprayOn(Actor a)
      {
        // never spray on player, could mess with his tactics
        if (a.IsPlayer) return false;

        // only if self or adjacent
        if (a != m_Actor && !Rules.IsAdjacent(m_Actor.Location, a.Location)) return false;

        // dont spray if already suppressed for 2h or more
        if (a.OdorSuppressorCounter >= 2 * WorldTime.TURNS_PER_HOUR) return false;

        // spot must be interesting to spray for either us or the target.
        if (IsGoodStenchKillerSpot(m_Actor.Location.Map, m_Actor.Location.Position)) return true;
        if (IsGoodStenchKillerSpot(a.Location.Map, a.Location.Position)) return true;
        return false;
      }

      // self?...
      if (WantsToSprayOn(m_Actor)) sprayOn = m_Actor;
      else {
        // ...adj leader/mates/followers
        if (m_Actor.HasLeader) {
          if (WantsToSprayOn(m_Actor.Leader)) sprayOn = m_Actor.Leader;
          else {
            foreach (Actor mate in m_Actor.Leader.Followers)
              if (sprayOn == null && mate != m_Actor && WantsToSprayOn(mate))
                sprayOn = mate;
          }
        }

        if (sprayOn == null && m_Actor.CountFollowers > 0) {
          foreach (Actor follower in m_Actor.Followers)
            if (sprayOn == null && WantsToSprayOn(follower)) sprayOn = follower;
        }
      }

      //  spray?
      if (sprayOn != null) {
        ActionSprayOdorSuppressor sprayIt = new ActionSprayOdorSuppressor(m_Actor, spray, sprayOn);
        if (sprayIt.IsLegal()) return sprayIt;
      }

      return null;  // nope.
    }

    protected bool BehaviorEquipStenchKiller(RogueGame game)
    {
      if (!IsGoodStenchKillerSpot(m_Actor.Location.Map, m_Actor.Location.Position)) return false;
      if (GetEquippedStenchKiller() != null) return true;
      ItemSprayScent firstStenchKiller = GetFirstStenchKiller(it => !it.IsUseless);
      if (firstStenchKiller != null /* && m_Actor.CanEquip(firstStenchKiller) */) {
        game.DoEquipItem(m_Actor, firstStenchKiller);
        return true;
      }
      return false;
    }
#endregion

    protected ActorAction BehaviorGoReviveCorpse(List<Percept> percepts)
    {
	  if (!Session.Get.HasCorpses) return null;
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) == 0) return null;
      if (!m_Actor.HasItemOfModel(GameItems.MEDIKIT)) return null;
      List<Percept> corpsePercepts = percepts.FilterT<List<Corpse>>().Filter(p =>
      {
        foreach (Corpse corpse in p.Percepted as List<Corpse>) {
          if (m_Actor.CanRevive(corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return true;
        }
        return false;
      });
      if (null == corpsePercepts) return null;
      Percept percept = FilterNearest(corpsePercepts);
	  if (m_Actor.Location.Position==percept.Location.Position) {
        foreach (Corpse corpse in (percept.Percepted as List<Corpse>)) {
          if (m_Actor.CanRevive(corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return new ActionReviveCorpse(m_Actor, corpse);
        }
	  }
      return BehaviorHeadFor(percept.Location,false,false);
    }

#region ground inventory stacks
    private List<Item> InterestingItems(IEnumerable<Item> Items)
    {
#if DEBUG
      if (null == Items) throw new ArgumentNullException(nameof(Items));
#endif
      var exclude = new HashSet<GameItems.IDs>(Objectives.Where(o=>o is Goal_DoNotPickup).Select(o=>(o as Goal_DoNotPickup).Avoid));
      IEnumerable<Item> tmp = Items.Where(it => !exclude.Contains(it.Model.ID) && IsInterestingItem(it));
      return (tmp.Any() ? tmp.ToList() : null);
    }

    private List<Item> InterestingItems(Inventory inv)
    {
      return InterestingItems(inv.Items);
    }

    private Item MostInterestingItemInStack(Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      List<Item> interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      return obj;
    }

    protected override ActorAction BehaviorWouldGrabFromStack(Location loc, Inventory stack)
    {
      return BehaviorGrabFromStack(loc,stack,false);
    }

    public bool WouldGrabFromStack(Location loc, Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.StackIsBlocked(loc)) return false;
      return WouldGrabFromAccessibleStack(loc,stack)?.IsLegal() ?? false;
    }

    public ActorAction WouldGrabFromAccessibleStack(Location loc, Inventory stack, bool is_real=false)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = !m_Actor.CanGet(obj) && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, loc.Position) : null);
#if DEBUG
#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      if (cant_get && null == recover) {
        int obj_code = ItemRatingCode(obj);
        foreach(Item it in m_Actor.Inventory.Items) {
          int it_code = ItemRatingCode(it);
          if (obj_code > it_code) throw new InvalidOperationException("passing up more important item than what is in inventory");
        }
        return null;
      }
#else
      if (cant_get && null == recover) return null;
#endif
#else
      if (cant_get && null == recover) return null;
#endif

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = new ActionTakeItem(m_Actor, loc, obj);

      if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
        if (null == recover) return null;
        if (!recover.IsLegal()) return null;
        if (recover is ActionDropItem drop) {
          if (obj.Model.ID == drop.Item.Model.ID) return null;
          if (is_real) Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, drop.Item.Model.ID));
        }
        if (is_real) Objectives.Insert(0,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter+1,m_Actor,tmp));
        if (is_real && RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
        return recover;
      }
      if (!tmp.IsLegal()) return null;    // in case this is the biker/trap pickup crash [cairo123]
      if (is_real && RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
      return tmp;
    }


    public ActorAction BehaviorGrabFromAccessibleStack(Location loc, Inventory stack)
    {
      return WouldGrabFromAccessibleStack(loc, stack, true);
    }

    protected ActorAction BehaviorGrabFromStack(Location loc, Inventory stack, bool is_real = true)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.StackIsBlocked(loc)) return null;

      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = !m_Actor.CanGet(obj) && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, loc.Position) : null);
#if DEBUG
#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      if (cant_get && null == recover) {
        int obj_code = ItemRatingCode(obj);
        foreach(Item it in m_Actor.Inventory.Items) {
          int it_code = ItemRatingCode(it);
          if (obj_code > it_code) throw new InvalidOperationException("passing up more important item than what is in inventory");
        }
        return null;
      }
#else
      if (cant_get && null == recover) return null;
#endif
#else
      if (cant_get && null == recover) return null;
#endif

      // the get item checks do not validate that inventory is not full
      ActorAction tmp = null;
      // XXX ActionGetFromContainer is obsolete.  Bypass BehaviorIntelligentBumpToward for containers.
      bool may_take = is_real ? m_Actor.MayTakeFromStackAt(loc) : true;

      if (may_take) {
        tmp = new ActionTakeItem(m_Actor, loc, obj);
        if (!tmp.IsLegal() && m_Actor.Inventory.IsFull) {
          if (null == recover) return null;
          if (!recover.IsLegal()) return null;
          if (recover is ActionDropItem drop) {
            if (obj.Model.ID == drop.Item.Model.ID) return null;
            if (is_real) Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, drop.Item.Model.ID));
          }
          if (is_real) Objectives.Insert(0,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter+1,m_Actor,tmp));
          if (is_real && RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
            RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
          return recover;
        }
        if (!tmp.IsLegal()) return null;    // in case this is the biker/trap pickup crash [cairo123]
        if (is_real && RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
        return tmp;
      }
      { // scoping brace
      if (null == _legal_steps) return null;
      int current_distance = Rules.GridDistance(m_Actor.Location, loc);
      Location? denorm = m_Actor.Location.Map.Denormalize(loc);
      var costs = new Dictionary<Point,int>();
      var vis_costs = new Dictionary<Point,int>();
      if (_legal_steps.Contains(denorm.Value.Position)) {
        Point pt = denorm.Value.Position;
        Location test = new Location(m_Actor.Location.Map,pt);
        costs[pt] = 1;
        // this particular heuristic breaks badly if it loses sight of its target
        if (LOS.ComputeFOVFor(m_Actor,test).Contains(denorm.Value.Position)) vis_costs[pt] = 1;
      } else {
        foreach(Point pt in _legal_steps) {
          Location test = new Location(m_Actor.Location.Map,pt);
          int dist = Rules.GridDistance(test,loc);
          if (dist >= current_distance) continue;
          costs[pt] = dist;
          // this particular heuristic breaks badly if it loses sight of its target
          if (!LOS.ComputeFOVFor(m_Actor,test).Contains(denorm.Value.Position)) continue;
          vis_costs[pt] = dist;
        }
        // above fails if a direct diagonal path is blocked.
        if (0 >= costs.Count) {
          foreach(Point pt in _legal_steps) {
            Location test = new Location(m_Actor.Location.Map,pt);
            int dist = Rules.GridDistance(test,loc);
            if (dist == current_distance) continue;
            costs[pt] = dist;
            // this particular heuristic breaks badly if it loses sight of its target
            if (!LOS.ComputeFOVFor(m_Actor,test).Contains(denorm.Value.Position)) continue;
            vis_costs[pt] = dist;
          }
        }
      }

      ActorAction tmpAction = DecideMove(vis_costs.Keys);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
        m_Actor.Activity = Activity.IDLE;
        if (is_real && RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
        return tmpAction;
      }
      tmpAction = DecideMove(costs.Keys);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
        m_Actor.Activity = Activity.IDLE;
        if (is_real && RogueForm.Game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
        return tmpAction;
      }
      } // end scoping brace
      return null;
    }

    protected ActorAction BehaviorRequestCriticalFromGroup()
    {
        var clan = m_Actor.ChainOfCommand;
        if (null == clan) return null;
        var critical = WhatDoINeedNow();
        if (0 >= critical.Count) return null;
        var insurance = new Dictionary<Actor, GameItems.IDs>();   // trading CPU for lower GC might be empirically ok here (null with alloc on first use)
        var want = new Dictionary<Actor, GameItems.IDs>();
        foreach (var a in clan) {
            if (!CanSee(a.Location)) continue;  // don't want to mention this sort of thing over radio
            if (!InCommunicationWith(a)) continue;
            if (a.IsPlayer) continue; // \todo self-order for this
            var have = (a.Controller as ObjectiveAI).NonCriticalInInventory();
            if (null != have.Key) {
                foreach (var x in have.Key) {
                    if (critical.Contains(x)) {
                        insurance[a] = x;
                        break;
                    }
                }
                if (insurance.ContainsKey(a)) continue; // possible MSIL compaction here (local bool vs. member function call); may not be a real micro-optimization
            }
            if (null != have.Value) {
                foreach (var x in have.Value) {
                    if (critical.Contains(x)) {
                        want[a] = x;
                        break;
                    }
                }
            }
        }
        ActorAction tmpAction = null;
        int min_dist = int.MaxValue;
        Actor donor = null;
        ActionGiveTo donate = null;
        if (0 < insurance.Count) {
            foreach (var x in insurance) {
                int dist = Rules.InteractionDistance(m_Actor.Location, x.Key.Location);
                if (dist >= min_dist) continue;
                var request = new ActionGiveTo(x.Key, m_Actor, x.Value);
                if (!request.IsLegal()) continue;
                if (1 >= dist) return request;
                if (null!=(x.Key.Controller as ObjectiveAI).Goal<Goal_HintPathToActor>()) continue; // stacking this can backfire badly
                min_dist = dist;
                donor = x.Key;
                donate = request;
            }
            if (null != donate) {
                // we are assuming we can path to the target, but we were able to recruit/be recruited earlier
                int t0 = Session.Get.WorldTime.TurnCounter + min_dist;    // overestimate
                var my_plan = new Goal_HintPathToActor(t0, m_Actor, donor, donate);    // XXX \todo any reasonable action that stays in range 1 of the donor is fine
                tmpAction = my_plan.Pathing();
                if (null != tmpAction) {
                    // could lift this test up to the previous loop but unlikely to have player-visible consequences i.e. not worth CPU
                    var your_plan = new Goal_HintPathToActor(t0, donor, m_Actor, donate);
                    (donor.Controller as OrderableAI).Objectives.Insert(0,your_plan);
                    Objectives.Insert(0,my_plan);
                    return tmpAction;
                }
                return null;
            }
        }
        if (0 < want.Count) {
            foreach (var x in want) {
                int dist = Rules.InteractionDistance(m_Actor.Location, x.Key.Location);
                if (dist >= min_dist) continue;
                var request = new ActionGiveTo(x.Key, m_Actor, x.Value);
                if (!request.IsLegal()) continue;
                if (1 >= dist) return request;
                if (null!=(x.Key.Controller as ObjectiveAI).Goal<Goal_HintPathToActor>()) continue; // stacking this can backfire badly
                min_dist = dist;
                donor = x.Key;
                donate = request;
            }
            if (null != donate) {
                // we are assuming we can path to the target, but we were able to recruit/be recruited earlier
                int t0 = Session.Get.WorldTime.TurnCounter + min_dist;    // overestimate

                var my_plan = new Goal_HintPathToActor(t0, m_Actor, donor, donate);    // XXX \todo any reasonable action that stays in range 1 of the donor is fine
                tmpAction = my_plan.Pathing();
                if (null != tmpAction) {    // could lift this test up to the previous loop but unlikely to have player-visible consequences i.e. not worth CPU
                    var your_plan = new Goal_HintPathToActor(t0, donor, m_Actor, donate);
                    (donor.Controller as OrderableAI).Objectives.Insert(0,your_plan);
                    Objectives.Insert(0,my_plan);
                    return tmpAction;
                }
                return null;
            }
        }
        return null;
    }

    protected ActorAction BehaviorFindTrade(List<Percept> friends)
    {
#if DEBUG
        if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException("must want to trade");
#endif
        var percepts2 = GetTradingTargets(friends); // this should only return legal trading targets
        if (null == percepts2) return null;
        Actor actor = FilterNearest(percepts2).Percepted as Actor;
        // We are having CPU loading problems, so don't retest the legality of the trade
        if (Rules.IsAdjacent(m_Actor.Location, actor.Location)) {
          MarkActorAsRecentTrade(actor);
          RogueGame.DoSay(m_Actor, actor, string.Format("Hey {0}, let's make a deal!", (object) actor.Name), RogueGame.Sayflags.IS_FREE_ACTION);  // formerly paid AP cost here rather than in RogueGame::DoTrade
          return new ActionTrade(m_Actor, actor);
        }
        ActorAction tmpAction = BehaviorIntelligentBumpToward(actor.Location, false, false);
        if (null == tmpAction) return null;
        // alpha10 announce it to make it clear to the player whats happening but dont spend AP (free action)
        // might spam for a few turns, but its better than not understanding whats going on.
        RogueGame.DoSay(m_Actor, actor, String.Format("Hey {0}, let's make a deal!", actor.Name), RogueGame.Sayflags.IS_FREE_ACTION);

        m_Actor.Activity = Activity.FOLLOWING;
        m_Actor.TargetActor = actor;
        // need an after-action "hint" to the target on where/who to go to
        if (!m_Actor.WillActAgainBefore(actor) && null==(actor.Controller as OrderableAI)?.Goal<Goal_HintPathToActor>()) {
          int t0 = Session.Get.WorldTime.TurnCounter+m_Actor.HowManyTimesOtherActs(1, actor) -(m_Actor.IsBefore(actor) ? 1 : 0);
          (actor.Controller as OrderableAI)?.Objectives.Insert(0,new Goal_HintPathToActor(t0, actor, m_Actor, new ActionTrade(actor,m_Actor)));    // AI disallowed from initiating trades with player so fine
        }
        return tmpAction;
    }
#endregion

#if FAIL
    // Gun bunny AI support
    // for efficiency purposes we probably want to return both the probability of hitting (current)
    // and some proxy for the "damage distribution"
    protected Dictionary<Actor,Dictionary<Item,float>> AttackEfficiency(List<Actor> enemies)
    {
      List<Item> tmpMelee = m_Actor.Inventory.Items.Where(it => it.Model.GetType()==typeof(ItemMeleeWeaponModel));
      List<Item> tmpRanged = m_Actor.Inventory.Items.Where(it => it.Model.GetType()==typeof(ItemRangedWeaponModel));
      Dictionary<Actor,Dictionary<Item,float>> ret = new Dictionary<Actor,Dictionary<Item,float>>(enemies.Count);
      foreach(Actor tmp in enemies) {
        CurrentDefence tmpDef = tmp.CurrentDefence;
        Dictionary<Item,float> prob = new Dictionary<Item,float>();
        if (0 < tmpMelee.Count && 1==Rules.GridDistance(m_Actor.Location.Position,tmp.Location.Position)) {
          // m_CurrentMeleeAttack = (it.Model as ItemMeleeWeaponModel).BaseMeleeAttack(Sheet);
          foreach(Item it in tmpMelee) {
            Attack tmpAtt = (it.Model as ItemMeleeWeaponModel).BaseMeleeAttack(m_Actor.Sheet);
          }
        }
        if (0 < tmpRanged.Count) {
          // m_CurrentRangedAttack = (it.Model as ItemRangedWeaponModel).Attack;   // value-copy due to struct Attack
          foreach(Item it in tmpRanged) {
            Attack tmpAtt = (it.Model as ItemRangedWeaponModel).Attack;
            if (Rules.GridDistance(m_Actor.Location.Position,tmp.Location.Position) > tmp2.Range) continue;
          }
        }
      }
      return ret;
    }
#endif

    // XXX arguably should be member function; doing this for code locality
    protected static bool IsLegalPathingAction(ActorAction act)
    {
      if (act is ActionMoveStep) return true;
      if (act is ActionSwitchPlace) return true;
      if (act is ActionOpenDoor) return true;
      if (act is ActionBashDoor) return true;
      if (act is ActionBreak) return true;
      if (act is ActionPush) return true;
      if (act is ActionShove) return true;
      return false;
    }

    protected void NavigateFilter(HashSet<Point> tainted)
    {
#if DEBUG
      if (null==tainted || 0 >=tainted.Count) throw new ArgumentNullException(nameof(tainted));
      if (tainted.Contains(m_Actor.Location.Position)) throw new InvalidOperationException("tainted.Contains(m_Actor.Location.Position)");
#endif
      int min_dist = int.MaxValue;
      int max_dist = int.MinValue;
      var dist = new Dictionary<Point,int>();
      foreach(var pt in tainted) {
        int tmp = Rules.GridDistance(pt, m_Actor.Location.Position);
        dist[pt] = tmp;
        if (tmp > max_dist) max_dist = tmp;
        if (tmp < min_dist) min_dist = tmp;
      }
      if (min_dist >= max_dist-min_dist) return;
      int min_2x = 2*min_dist;
      foreach(var x in dist) {
        if (x.Value > min_2x) tainted.Remove(x.Key);
      }
    }

    // April 7 2017: This is called directly only by the same-map threat and tourism behaviors
    // These two behaviors both like a "spread out" where each non-follower ally heads for the targets nearer to them than
    // to the other non-follower allies
    protected ActorAction BehaviorNavigate(HashSet<Point> tainted)
    {
#if DEBUG
      if (null==tainted || 0 >=tainted.Count) throw new ArgumentNullException(nameof(tainted));
      if (tainted.Contains(m_Actor.Location.Position)) throw new InvalidOperationException("tainted.Contains(m_Actor.Location.Position)");
#endif

      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      navigate.GoalDistance(tainted, m_Actor.Location.Position);
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && !navigate.Domain.Contains(m_Actor.Location.Position)) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": navigate destination unreachable from ("+m_Actor.Location.Position.X.ToString()+","+ m_Actor.Location.Position.Y.ToString() + ")");
        List<string> msg = new List<string>();
        foreach(Point pt in navigate.Domain) {
          msg.Add("(" + pt.X.ToString() + "," + pt.Y.ToString() + "): " + navigate.Cost(pt));
        }
        msg.Sort();
        foreach(string x in msg) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, x);
        }
        msg.Clear();
        foreach(Point pt in navigate.black_list) {
          msg.Add("(" + pt.X.ToString() + "," + pt.Y.ToString() + "): " + navigate.Cost(pt));
        }
        msg.Sort();
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "black list");
        foreach(string x in msg) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, x);
        }
      }
#endif
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;

      Dictionary<Point, int> dest = PlanApproach(navigate);
      if (null==dest || 0 >= dest.Count) {
        Dictionary<Point, ActorAction> legal = m_Actor.OnePath(m_Actor.Location.Map, m_Actor.Location.Position, new Dictionary<Point, ActorAction>());  // \todo not truly reliable w/o proper get-from-container action
        if (0 < legal.Count) {
          // workaround
          var bypass = new Dictionary<Point, int>();
          foreach(var x in legal) {
            int cost = navigate.Cost(x.Key);
            if (int.MaxValue == cost) continue;
            if (!x.Value.IsLegal()) continue;
            bypass[x.Key] = cost;
          }
          dest = bypass;
        }
#if DEBUG
        if (0 >= dest.Count) throw new InvalidOperationException("should be able to close in");
#else
        if (0 >= dest.Count) return null;
#endif      
      }

      var exposed = new Dictionary<Point,int>();

      foreach(Point pt in dest.Keys) {
#if TRACE_NAVIGATE
        ActorAction tmp = Rules.IsPathableFor(m_Actor,new Location(m_Actor.Location.Map,pt), out string err);
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": ("+pt.X.ToString()+","+pt.Y.ToString()+") "+(null==tmp ? "null ("+err+")" : tmp.ToString()));
#else
        ActorAction tmp = Rules.IsPathableFor(m_Actor,new Location(m_Actor.Location.Map,pt));
#endif
        if (null == tmp || !tmp.IsLegal() || !IsLegalPathingAction(tmp)) continue;
        HashSet<Point> los = LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt));
        los.IntersectWith(tainted);
        exposed[pt] = los.Count;
      }
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget && 0 >= dest.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no possible moves for navigation");
      if (m_Actor.IsDebuggingTarget && 0 >= exposed.Count) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": no acceptable moves for navigation");
      if (m_Actor.IsDebuggingTarget && 0 < exposed.Count) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+": considering navigation from "+m_Actor.Location.Position.to_s());
        List<string> msg = new List<string>();
        msg.Add("src:" + m_Actor.Location.Position.to_s() + ": " + navigate.Cost(m_Actor.Location.Position).ToString());
        foreach(Point pt in exposed.Keys) {
          msg.Add(pt.to_s() + ": " + navigate.Cost(pt).ToString() + "," + exposed[pt].ToString());
        }
        msg.Sort();
        foreach(string x in msg) {
          Logger.WriteLine(Logger.Stage.RUN_MAIN, x);
        }
        msg.Clear();
      }
#endif
      if (0 >= exposed.Count) return null;

      int most_exposed = exposed.Values.Max();
      if (0<most_exposed) exposed.OnlyIf(val=>most_exposed<=val);
      var costs = new Dictionary<Point,int>();
      foreach(Point pt in exposed.Keys) {
        costs[pt] = navigate.Cost(pt);
      }
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "calling DecideMove");
#endif
      ActorAction ret = DecideMove(costs);
#if TRACE_NAVIGATE
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "final: "+(ret?.ToString() ?? "null"));
#endif
      if (null == ret) return null; // can happen due to postprocessing
      if (ret is ActionMoveStep test) {
        ReserveSTA(0,1,0,0);    // for now, assume we must reserve one melee attack of stamina (which is at least as much as one push/jump, typically)
        m_Actor.IsRunning = RunIfAdvisable(test.dest); // XXX should be more tactically aware
        ReserveSTA(0,0,0,0);
      }
      return ret;
    }

    protected ActorAction BehaviorHastyNavigate(IEnumerable<Point> tainted)
    {
#if DEBUG
      if (!tainted.Any()) throw new InvalidOperationException("0 >= tainted.Count()");
#endif
      Zaimoni.Data.FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      navigate.GoalDistance(tainted, m_Actor.Location.Position);
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      ActorAction ret = DecideMove(PlanApproach(navigate));
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest); // XXX should be more tactically aware
      return ret;
    }

    protected ActorAction BehaviorHeadForExit(Dictionary<Point,Exit> valid_exits)
    {
#if DEBUG
      if (null == valid_exits) throw new ArgumentNullException(nameof(valid_exits));
#endif
      if (0 >= valid_exits.Count) return null;
      if (valid_exits.ContainsKey(m_Actor.Location.Position)) {
        return BehaviorUseExit(BaseAI.UseExitFlags.BREAK_BLOCKING_OBJECTS | BaseAI.UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      return BehaviorHastyNavigate(valid_exits.Keys);
    }

    protected bool HaveThreatsInCurrentMap()
    {
      ThreatTracking threats = m_Actor.Threats;
      if (null == threats) return false;
      if (m_Actor.Location.Map == m_Actor.Location.Map.District.SewersMap && Session.Get.HasZombiesInSewers) return false;
      return 0< threats.ThreatWhere(m_Actor.Location.Map).Count;   // XXX could be more efficient?
    }

    private HashSet<Point> PartialInvertLOS(HashSet<Point> tainted, Map m, int radius)
    {
      var ret = new HashSet<Point>(tainted);
      var ideal = LOS.OptimalFOV(radius);
      foreach(var pt in tainted) {
        if (!m.WouldBlacklistFor(pt,m_Actor)) continue;
        foreach(var offset in ideal) {
          Point test = new Point(pt.X+offset.X,pt.Y+offset.Y);
          if (!m.IsInBounds(pt)) continue;  // have commited to point-based pathfinding when calling this
          if (ret.Contains(test)) continue;
          if (LOS.CanTraceViewLine(new Location(m,test),pt)) ret.Add(test);
        }
      }
      return ret;
    }

    protected bool HaveTourismInCurrentMap()
    {
      LocationSet sights_to_see = m_Actor.InterestingLocs;
      if (null == sights_to_see) return false;
      HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map);
      return 0<tainted.Count;   // XXX could be more efficient?
    }

    protected FloodfillPathfinder<Point> PathfinderFor(Func<Map, HashSet<Point>> targets_at)
    {
#if DEBUG
      if (null == targets_at) throw new ArgumentNullException(nameof(targets_at));
      if (!(this is CivilianAI)) throw new InvalidOperationException("unhandled OrderableAI subclass");
#endif
      FloodfillPathfinder<Point> navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);
      HashSet<Point> where_to_go = targets_at(m_Actor.Location.Map);
      if (0<where_to_go.Count) navigate.GoalDistance(where_to_go, m_Actor.Location.Position);
      // have to debar GangAI/SoldierAI/CHARGuardAI until this is fixed
      if (!m_Actor.Model.Abilities.AI_CanUseAIExits) {
        if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
        return navigate;
      }

      Dictionary<Point,Exit> valid_exits = m_Actor.Location.Map.GetExits(exit => {  // simulate Exit::ReasonIsBlocked
#if DEBUG
        int blocked = exit.Location.IsBlockedForPathing;
        switch(blocked)
#else
        switch(exit.Location.IsBlockedForPathing)
#endif
        {
        case 2: return false;
        case 1: return m_Actor.CanJump;
#if DEBUG
        case 0: return true;    // not if someone is sleeping on a couch
        default: throw new InvalidOperationException("exit.Location.IsBlockedForPathing out of range: "+blocked.ToString());
#else
        default: return true;
#endif
        }
      });
      var hazards = new Dictionary<Map, HashSet<Point>>();
      foreach(Map m in m_Actor.Location.Map.destination_maps.Get) {
        hazards[m] = targets_at(m);
      }
      hazards.OnlyIf(val=>0<val.Count);
      if (0 >= hazards.Count) {
        if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
        if (0 >= where_to_go.Count) return null;
        return navigate;
      }
//    veto_hazards(hazards);
      foreach(KeyValuePair<Map,HashSet<Point>> m_dests in hazards) {
        var exits_for_m = new Dictionary<Point,Exit>(valid_exits);
        exits_for_m.OnlyIf(exit => exit.ToMap == m_dests.Key);
        List<Point> remote_dests = new List<Point>(exits_for_m.Values.Select(exit => exit.Location.Position));
        FloodfillPathfinder<Point> remote_navigate = m_dests.Key.PathfindSteps(m_Actor);
        remote_navigate.GoalDistance(m_dests.Value, remote_dests);

        foreach(KeyValuePair<Point, Exit> tmp in exits_for_m) {
          if (!remote_navigate.Domain.Contains(tmp.Value.Location.Position)) return null;
          int cost = remote_navigate.Cost(tmp.Value.Location.Position);
          if (int.MaxValue == cost) continue;   // should be same as not in domain, but evidently not.
          navigate.ReviseGoalDistance(tmp.Key, cost+1,m_Actor.Location.Position);   // works even for denormalized coordinates as long as they're adjacent to real coordinates
        }
      }
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      return navigate;
    }

    protected ActorAction BehaviorResupply(HashSet<GameItems.IDs> critical)
    {
      var tmp = BehaviorUseAdjacentStack();
      if (null != tmp) return tmp;
      return BehaviorPathTo(m => WhereIs(critical, m));
    }

    public bool ProposeSwitchPlaces(Location dest)
    {
      if (null!=BehaviorUseAdjacentStack()) return false;
      if (null!=WouldUseAccessibleStack(dest)) return true;
      var track_inv = Goal<Goal_PathToStack>();
      if (null != track_inv) {
        if (track_inv.Destinations.Select(loc => Rules.GridDistance(loc,m_Actor.Location)).Min() > track_inv.Destinations.Select(loc => Rules.GridDistance(loc, m_Actor.Location)).Min()) return false;
//      if (track_inv.Destinations.Select(loc => Rules.GridDistance(loc,m_Actor.Location)).Min() < track_inv.Destinations.Select(loc => Rules.GridDistance(loc, m_Actor.Location)).Min()) return true;
      }
      return true;
    }

    public bool RejectSwitchPlaces(Location dest)
    {
      if (null!=BehaviorUseAdjacentStack()) return true;
      if (null!=WouldUseAccessibleStack(dest)) return false;
      var track_inv = Goal<Goal_PathToStack>();
      if (null != track_inv) {
        var old_dist = track_inv.Destinations.Select(loc => Rules.GridDistance(loc, m_Actor.Location)).Min();
        var new_dist = track_inv.Destinations.Select(loc => Rules.GridDistance(loc, dest)).Min();
        if (old_dist > new_dist) return false;
        if (old_dist < new_dist) return true;
      }
      if (MovePlanIf(m_Actor.Location.Position)?.ContainsKey(dest.Position) ?? false) return false;    // needs adjusting for cross-district

      var already_near_actor = GetCloseToActor();
      if (   null!=already_near_actor.Key && 0<already_near_actor.Value // reject default-initialization
          && InProximity(dest, already_near_actor.Key.Location, already_near_actor.Value))
        return false;

      return true;
    }

    protected bool NeedsLight()
    {
      switch (m_Actor.Location.Map.Lighting)
      {
        case Lighting.DARKNESS: return true;
        case Lighting.LIT: return false;
#if DEBUG
        case Lighting.OUTSIDE:
#else
        default:
#endif
          if (!m_Actor.Location.Map.LocalTime.IsNight) return false;

          // use threat tracking/tourism when available
          ThreatTracking threats = m_Actor.Threats;
          LocationSet sights_to_see = m_Actor.InterestingLocs;
          if (null != threats || null != sights_to_see) {
            int no_light_range = m_Actor.FOVrangeNoFlashlight(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
            HashSet<Point> no_light_FOV = LOS.ComputeFOVFor(m_Actor.Location, no_light_range);
            HashSet<Point> danger_point_FOV = LOS.ComputeFOVFor(m_Actor.Location, no_light_range+3);
            danger_point_FOV.ExceptWith(no_light_FOV);

            int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
            Rectangle view = new Rectangle(m_Actor.Location.Position.X - tmp_LOSrange, m_Actor.Location.Position.Y - tmp_LOSrange, 2*tmp_LOSrange+1,2*tmp_LOSrange+1);

            if (null!=threats) {
              HashSet<Point> tainted = threats.ThreatWhere(m_Actor.Location.Map, view);
              tainted.IntersectWith(danger_point_FOV);
              if (0<tainted.Count) return true;
            }
            if (null!=sights_to_see) {
              HashSet<Point> tainted = sights_to_see.In(m_Actor.Location.Map, view);
              tainted.IntersectWith(danger_point_FOV);
              if (0<tainted.Count) return true;
            }
            if (null!=threats && null!=sights_to_see) return false;
          }

          // resume legacy implementation
          if (Session.Get.World.Weather != Weather.HEAVY_RAIN) return !m_Actor.IsInside;
          return true;
#if DEBUG
        default: throw new ArgumentOutOfRangeException("unhandled lighting");
#endif
      }
    }

    // taboos
    protected void MarkActorAsRecentTrade(Actor other)
    {
      if (m_TabooTrades == null) m_TabooTrades = new List<Actor>(1);
      else if (m_TabooTrades.Contains(other)) return;
      m_TabooTrades.Add(other);
    }

    public bool IsActorTabooTrade(Actor other)
    {
      if (m_TabooTrades == null) return false;
      return m_TabooTrades.Contains(other);
    }

    protected void ExpireTaboos()
    {
      // maintain taboo information
      // actors ok to clear at midnight
      if (m_Actor.Location.Map.LocalTime.IsStrikeOfMidnight) m_TabooTrades = null;
    }
  }
}
