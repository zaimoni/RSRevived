// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define TRACE_GOALS
#define INTEGRITY_CHECK_ITEM_RETURN_CODE
// #define REPAIR_DO_NOT_PICKUP

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;
using static Zaimoni.Data.Functor;

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using djack.RogueSurvivor.Gameplay.AI.Goals;
using System.Runtime.Serialization;

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
    // workaround for breaking action loops
    [Serializable]
    internal class Goal_NextAction : Objective
    {
      public readonly ActorAction Intent;

      public Goal_NextAction(int t0, Actor who, ActorAction intent) : base(t0,who)
      {
        Intent = intent
#if DEBUG
          ?? throw new ArgumentNullException(nameof(intent))
#endif
        ;
      }

      // always execute.  Expire on execution
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (!Intent.IsPerformable() || Intent.Abort()) {
          _isExpired = true;
          return true;
        }
        // XXX need some sense of what a combat action is
        if (null != m_Actor.Controller.enemies_in_FOV) return false;
        if ((m_Actor.Controller as ObjectiveAI).VetoAction(Intent)) {
          _isExpired = true;
          return true;
        }

       _isExpired = true;
        ret = Intent;
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
            if (null == m_Actor.Controller.enemies_in_FOV) {
                if (null != Intent_disengaged && Intent_disengaged.IsPerformable()) ret = Intent_disengaged;  // \todo may need to call auxilliary function instead
            } else {
                if (null != Intent_engaged && Intent_engaged.IsPerformable()) ret = Intent_engaged;  // \todo may need to call auxilliary function instead
            }
            if (ret is ActionMoveStep step && m_Actor.Controller is ObjectiveAI ai) m_Actor.IsRunning = ai.RunIfAdvisable(step.dest);
            return true;
        }
    }

    [Serializable]
    internal class Goal_NonCombatComplete : Objective
    {
      public readonly ActorAction Intent;

      public Goal_NonCombatComplete(int t0, Actor who, ActorAction intent) : base(t0,who)
      {
        Intent = intent
#if DEBUG
          ?? throw new ArgumentNullException(nameof(intent))
#endif
        ;
      }

      // always execute.  Expire on inability to continue
      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        // XXX need some sense of what a combat action is
        if (null != m_Actor.Controller.enemies_in_FOV) {    // VAPORWARE: respond to enemies "known but not in FOV"
          if (!Intent.IsLegal()) {
            _isExpired = true;
            return true;
          }
          return false;
        }
        if (!Intent.IsPerformable()) {
          _isExpired = true;
          return true;
        }
        ret = Intent;
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
        var dont_ignore_these = (m_Actor.Controller as ObjectiveAI).WhatDoINeedNow();
        dont_ignore_these.UnionWith((m_Actor.Controller as ObjectiveAI).WhatDoIWantNow());
        if (dont_ignore_these.Contains(Avoid)) {
          // instantly expire if critical.
          _isExpired = true;
          return false;
        }

        // expire if the offending item is not in LoS
        var stacks = m_Actor.Controller.items_in_FOV;
        if (null != stacks) foreach(var inv in stacks.Values) if (inv.Has(Avoid)) return false;
//      if (m_Actor.Inventory.Has(Avoid)) return false; // checking whether this is actually needed
        _isExpired = true;  // but expire if the offending item is not in LOS or inventory
        return false;
      }

      public override string ToString()
      {
        return "Avoiding picking up " + Avoid;
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
        var fov = m_Actor.Controller.FOVloc;
        if (!fov.Any(loc => _locs.Contains(loc))) return true;
        if (ObjectiveAI.ReactionCode.SLEEPY < (m_Actor.Controller as ObjectiveAI).InterruptLongActivity()) return false;
        ret = (m_Actor.Controller as OrderableAI).BehaviorWalkAwayFrom(_locs, null);
        return true;
      }

      public override string ToString()
      {
        return "Breaking line of sight to "+_locs.to_s();
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
          x.Value.RemoveWhere(loc => a.Controller.CanSee(in loc));
        }
        return false;
      }

      private void RefreshLocations()
      {
        foreach(var x in _target_locs) {
          if (x.Value.Contains(x.Key.Location)) continue;
          if (x.Key.Controller is ObjectiveAI) {
            var candidates = x.Key.OnePathRange(x.Key.Location);  // XXX fails for Z
            if (null == candidates) x.Value.Clear();
            else x.Value.IntersectWith(candidates.Keys);
          } else {
            var candidates = x.Key.OneStepRange(x.Key.Location);
            if (null == candidates) x.Value.Clear();
            else x.Value.IntersectWith(candidates);
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
        if (ObjectiveAI.ReactionCode.SLEEPY < ai.InterruptLongActivity()) return false;
        // XXX \todo really want inverse-FOVs for destinations; trigger calculation/retrieval from cache here
        ai.ClearLastMove();
        ret = ai.BehaviorPathTo(m => WhereIn(m));
        return true;
      }

      public override string ToString()
      {
        return "Securing area"+ _target_locs.to_s();
      }
    }

    [Serializable]
    internal class Goal_PathTo : Objective
    {
      private readonly HashSet<Location> _locs;
      private readonly bool walking;
      private int turns;

      public Goal_PathTo(int t0, Actor who, in Location loc, bool walk=false,int n=int.MaxValue)
      : base(t0,who)
      {
        _locs = new HashSet<Location>{loc};
        walking = walk;
        turns = n;
      }

      public Goal_PathTo(int t0, Actor who, IEnumerable<Location> locs, bool walk = false, int n = int.MaxValue)
      : base(t0,who)
      {
        _locs = new HashSet<Location>(locs);
        walking = walk;
        turns = n;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (_locs.Contains(m_Actor.Location) || 0 >= turns--) {
          _isExpired = true;
          return true;
        }

        var ai = m_Actor.Controller as ObjectiveAI;
        if (ObjectiveAI.ReactionCode.NONE != ai.InterruptLongActivity() || ai.InCombat) {
          _isExpired = true;    // cancel: something urgent
          return true;
        }

        ret = ai.BehaviorPathTo(_locs);
        if (!(ret?.IsPerformable() ?? false)) {
          ret = null;
          _isExpired = true;    // cancel: buggy
          return true;
        }
        if (walking) m_Actor.Walk();
        else if (ret is ActionMoveStep step) ai.RunIfAdvisable(step.dest);
        else if (ret is ActionMoveDelta delta) {
          if (delta.ConcreteAction is ActionMoveStep step2) ai.RunIfAdvisable(step2.dest);
        }
        return true;
      }
    }

    [Serializable]
    internal class Goal_PathToStack : Objective,LatePathable
    {
      private readonly List<Percept_<Inventory>> _stacks = new List<Percept_<Inventory>>(1);
      [NonSerialized] private OrderableAI ordai;
      [NonSerialized] private List<KeyValuePair<Location, ActorAction>>? _inventory_actions = null;

      public IEnumerable<Inventory> Inventories { get { return _stacks.Select(p => p.Percepted); } }
      public IEnumerable<Location> Destinations { get { return _stacks.Select(p => p.Location); } }

      public Goal_PathToStack(int t0, Actor who, Location loc) : base(t0,who)
      {
        if (!(who.Controller is OrderableAI ai)) throw new InvalidOperationException("need an ai with inventory");
        ordai = ai;
        if (!Map.Canonical(ref loc)) return;
        newStack(in loc);
      }

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
#if DEBUG
      foreach(var x in _stacks) x.Percepted.RepairZeroQty();
#endif
    }

      [OnDeserialized] void OnDeserialized(StreamingContext context) {
        ordai = m_Actor.Controller as OrderableAI;
      }

      /// <returns>true if and only if no stacks remain</returns>
      private bool _removeInvalidStacks()
      {
        _inventory_actions = null;
        int i = _stacks.Count;
        while(0 < i--) {
          Inventory? inv;
          { // scope var p
          var p = _stacks[i];
          inv = exemplarStack(p.Location);
          if (    null == inv    // can crash otherwise in presence of bugs
               || !m_Actor.CanEnter(p.Location)
               || (m_Actor.Controller.CanSee(p.Location) && m_Actor.StackIsBlocked(p.Location))) {
              _stacks.RemoveAt(i);
              ordai.ClearLastMove();
              continue;
          }
          _stacks[i] = new Percept_<Inventory>(inv, m_Actor.Location.Map.LocalTime.TurnCounter, p.Location);
          } // end scope var p

          if (inv.IsEmpty || !ordai.WouldGrabFromStack(_stacks[i].Location, inv)) {
            _stacks.RemoveAt(i);
            continue;
          } else {
            var act = ordai.WouldGrabFromAccessibleStack(_stacks[i].Location, inv);
            if (null == act || !act.IsLegal()) {
              _stacks.RemoveAt(i);
              continue;
            }
            if (m_Actor.MayTakeFromStackAt(_stacks[i].Location) && act.IsPerformable()) {
              (_inventory_actions ??= new List<KeyValuePair<Location, ActorAction>>()).Add(new KeyValuePair<Location, ActorAction>(_stacks[i].Location, act));
            }
          }
        }
        return 0 >= _stacks.Count;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;

        if (_removeInvalidStacks()) {
          _isExpired = true;
          return true;
        }

        if (m_Actor.Controller.IsEngaged) return false;

        if (null != _inventory_actions) {
          // prefilter
          if (2 <= _inventory_actions.Count) {
            var ub = _inventory_actions.Count;
            while(1 <= --ub) {
              var upper_take = _inventory_actions[ub].Value as ActorTake;
              if (null == upper_take) {
                if (_inventory_actions[ub].Value is ActionChain chain) upper_take = chain.LastAction as ActorTake;
              }
              if (null == upper_take) continue;
              var i = ub;
              while(0 <= --i) {
                var take = _inventory_actions[i].Value as ActorTake;
                if (null == take) {
                  if (_inventory_actions[i].Value is ActionChain chain) take = chain.LastAction as ActorTake;
                }
                if (null == take) continue;
                if (ordai.RHSMoreInteresting(take.Take, upper_take.Take)) {
                  _inventory_actions.RemoveAt(i);
                  break;
                } else if (ordai.RHSMoreInteresting(upper_take.Take, take.Take)) {
                  _inventory_actions.RemoveAt(ub);
                  break;
                }
              }
            }
          }
          ret = _inventory_actions[0].Value;
          m_Actor.Activity = Activity.IDLE;
          _isExpired = true;  // we don't play well with action chains
          return true;
        }

        // let other AI processing kick in before final pathing
        return false;
      }

      private Inventory? exemplarStack(in Location loc) // XXX causes telepathic leakage
      {
        var allItems = Map.AllItemsAt(loc, m_Actor);
        if (null == allItems) return null;
        foreach(var inv in allItems) if (ordai.WouldGrabFromStack(in loc, inv)) return inv;
        return null;
      }

      public void newStack(in Location loc) {
#if DEBUG
        // containers can only exist on enterable squares
        if (!m_Actor.CanEnter(loc)) throw new InvalidOperationException(m_Actor.Name+" wants inaccessible ground inventory at "+loc);
#endif
        var relay = exemplarStack(in loc);
        if (null == relay) return;

        int i = _stacks.Count;
        // update if stack is present // \todo
        while(0 < i--) {
          var p = _stacks[i];
          if (p.Location==loc) {
            _stacks[i] = new Percept_<Inventory>(relay, m_Actor.Location.Map.LocalTime.TurnCounter, p.Location);
            return;
          }
        }

        _stacks.Add(new Percept_<Inventory>(relay, m_Actor.Location.Map.LocalTime.TurnCounter, in loc));   // otherwise, add
      }

      public ActorAction Pathing()
      {
        if (_removeInvalidStacks()) return null;
        var _locs = _stacks.Select(p => p.Location).Where(loc => loc.StrictHasActorAt);
        if (!_locs.Any()) return null;

        var ret = ordai.BehaviorPathTo(new HashSet<Location>(_locs));
        return (ret?.IsPerformable() ?? false) ? ret : null;
      }

      public override string ToString()
      {
        return "Pathing to "+ _stacks.to_s();
      }
    }

    [Serializable]
    internal class Goal_HintPathToActor : Objective,Pathable
    {
      private readonly Actor _dest;
      private readonly ActorAction _when_at_target;

      public Actor Whom { get { return _dest; } }

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
          if (null != _when_at_target && !_when_at_target.IsPerformable()) {    // no longer viable
            _isExpired = true;
            return true;
          }
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
      private readonly Location[]? _alternates = null; // for when multiple doors are in a row

#nullable enable
      public Goal_BreakBarricade(Actor who, DoorWindow dest) : base(who.Location.Map.LocalTime.TurnCounter, who)
      {
        _dest = dest.Location;
        var backup_plan = new List<Location>();
        // \todo following doesn't properly handle 3-wide situation
        var loc = dest.Location+Direction.N;
        var door = loc.MapObject as DoorWindow;
        if (null != door && door.IsBarricaded) backup_plan.Add(loc);
        loc = dest.Location+Direction.S;
        door = loc.MapObject as DoorWindow;
        if (null != door && door.IsBarricaded) backup_plan.Add(loc);
        loc = dest.Location+Direction.E;
        door = loc.MapObject as DoorWindow;
        if (null != door && door.IsBarricaded) backup_plan.Add(loc);
        loc = dest.Location+Direction.W;
        door = loc.MapObject as DoorWindow;
        if (null != door && door.IsBarricaded) backup_plan.Add(loc);
        if (0 < backup_plan.Count) _alternates = backup_plan.ToArray();
      }

      public DoorWindow? Target { get { return _dest.MapObject as DoorWindow; } }
#nullable restore

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        var door = Target;
        if (!door?.IsBarricaded ?? true) {  // it's down now
          _isExpired = true;
          return true;
        }
#if DEBUG
        if (door.Location != _dest) throw new InvalidOperationException("door.Location != _dest");
#endif
        if (null != _alternates) {
          foreach(var loc in _alternates) {
            var bypass = loc.MapObject as DoorWindow;
            if (null == bypass || !bypass.IsBarricaded) { // some other way through -- break off
              _isExpired = true;
              return true;
            }
          }
        }
        if (ObjectiveAI.ReactionCode.SLEEPY < (m_Actor.Controller as ObjectiveAI).InterruptLongActivity()) {
          _isExpired = true;    // cancel: something urgent
          return true;
        }
        if (Rules.IsAdjacent(m_Actor.Location, in _dest)) {
          if (m_Actor.CanBreak(door)) {
            ret = new ActionBreak(m_Actor, door);
            return true;
          }
#if DEBUG
          if (!m_Actor.IsTired) throw new InvalidOperationException("!m_Actor.IsTired");
#endif
          var break_from = new List<Location>(8);
          foreach(var pt in _dest.Position.Adjacent()) {
            break_from.Add(new Location(_dest.Map, pt)); // will be normalized since doors aren't on map edges 2020-09-08 zaimoni
          }

          // check for helpers that are ready and politely back off for them
          var escape = new Zaimoni.Data.Stack<Point>(stackalloc Point[8]);
          var motive = new Zaimoni.Data.Stack<Point>(stackalloc Point[8]);
          foreach(Point pt in m_Actor.Location.Position.Adjacent()) {
            if (Rules.IsAdjacent(in pt, _dest.Position)) continue;
            if (m_Actor.Location.Map.IsWalkableFor(pt,m_Actor)) escape.push(pt);
            else {
              Actor helper = m_Actor.Location.Map.GetActorAt(pt);
              if (null == helper) continue;
              if (break_from.Any(loc => Rules.IsAdjacent(loc, helper.Location))) continue;
              if (!helper.IsTired && null != (helper.Controller as ObjectiveAI)?.Goal<Goal_BreakBarricade>(o => o.Target == door)) {
                motive.push(pt);
              }
            }
          }
          if (0 < motive.Count && 0<escape.Count) {
            ret = new ActionMoveStep(m_Actor,Rules.Get.DiceRoller.Choose(escape));
            return true;
          }

          ret = new ActionWait(m_Actor);
          return true;
        }

        // have some sort of duplication issue going on
        if (null != _alternates) {
          foreach(var loc in _alternates) {
            if (Rules.IsAdjacent(m_Actor.Location, loc)) {
              _isExpired = true;
              return true;
            }
          }
        }

        // unusual pathing requirement: do not push helpers; give helpers room to step aside
        var helpers_at = new Dictionary<Point,Actor>();
        var move_to = new HashSet<Point>();
        foreach(Point pt in _dest.Position.Adjacent()) {
          if (_dest.Map.IsWalkableFor(pt,m_Actor.Model)) {
            move_to.Add(pt);
            continue;
          }
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
              if (Rules.IsAdjacent(in pt,x.Key)) {
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
          ret = (m_Actor.Controller as ObjectiveAI).BehaviorPathTo(m => (m == _dest.Map ? move_to : new HashSet<Point>()));
          return true;
        }
        return false;
      }
    }

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
#nullable enable
    private ActorOrder? m_Order;
#nullable restore
    protected Percept_<Actor> m_LastEnemySaw;
    protected Percept m_LastItemsSaw;
    protected Percept m_LastSoldierSaw;
    protected Percept m_LastRaidHeard;
#nullable enable
    [NonSerialized] static private Percept? _lastRaidHeard; // staging for m_LastRaidHeard
    [NonSerialized] private List<Actor>? _adjacent_friends;    // cache variable for above four
#nullable restore
    protected bool m_ReachedPatrolPoint;
    protected int m_ReportStage;

    public bool DontFollowLeader { get; set; }

    protected OrderableAI(Actor src) : base(src) {}

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      // these (last-seen history) need rethinking anyway (as part of a reputation system, etc.)
      if (m_Actor.IsDead) {
        m_LastEnemySaw = null;
        m_LastItemsSaw = null;
        m_LastSoldierSaw = null;
        m_LastRaidHeard = null;
      }
    }

    public ActorDirective Directives { get { return m_Directive ??= new ActorDirective(); } }
    protected List<Actor> TabooTrades { get { return m_TabooTrades; } }
#nullable enable
    public ActorOrder? Order { get { return m_Order; } }
    public void SetOrder(ActorOrder? newOrder)
    {
      m_Order = newOrder;
      m_ReachedPatrolPoint = false;
      m_ReportStage = 0;
    }

    protected override void ResetAICache()
    {
      base.ResetAICache();
      _adjacent_friends = null;
    }

    private List<Actor>? AdjacentFriends() {
      if (null == _adjacent_friends) {
        var scan_friends = friends_in_FOV;
        if (null == scan_friends) return null;
        _adjacent_friends = new List<Actor>();
        foreach(var x in scan_friends) {
          if (!(x.Value.Controller is ObjectiveAI ai)) continue;
          if (   1 >= Rules.InteractionDistance(m_Actor.Location,x.Key)
              && !x.Value.IsSleeping
              && !ai.IsEngaged)   // RS Revived: don't chat to a combatant
            _adjacent_friends.Add(x.Value);
        }
      }
      return 0<_adjacent_friends.Count ? _adjacent_friends : null;
    }
#nullable restore

    public override int FastestTrapKill(in Location loc)
    {
      if (m_Actor.IsStarving || ActorCourage.COURAGEOUS == Directives.Courage) return int.MaxValue;
      int trapsMaxDamage = loc.Map.TrapsUnavoidableMaxDamageAtFor(loc.Position,m_Actor);
      if (0 >= trapsMaxDamage) return int.MaxValue;
      return ((m_Actor.HitPoints-1)/ trapsMaxDamage)+1;
    }

    protected ActorAction? ExecuteOrder(RogueGame game, ActorOrder order, List<Percept> percepts)
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
          return ExecuteReport(percepts);  // cancelled by enamies sighted
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
      tmpAction = BehaviorIntelligentBumpToward(in location, false, false);
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
      tmpAction = BehaviorIntelligentBumpToward(in location, false, false);
      if (null == tmpAction) return null;
      m_Actor.Run();
      return tmpAction;
    }

    private ActorAction ExecuteGuard(Location location, List<Percept> percepts)
    {
      var enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", actor.Name));
      }

      ActorAction? tmpAction = NonCombatReflexMoves();
      if (null != tmpAction) return tmpAction;

      if (m_Actor.Location.Position != location.Position) {
        tmpAction = BehaviorIntelligentBumpToward(in location, false, false);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

      return new ActionWait(m_Actor);
    }

    private ActorAction ExecutePatrol(Location location, List<Percept> percepts)
    {
      var enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", actor.Name));
      }

      ActorAction? tmpAction = NonCombatReflexMoves();
      if (null != tmpAction) return tmpAction;

      if (!m_ReachedPatrolPoint) m_ReachedPatrolPoint = m_Actor.Location.Position == location.Position;
      if (!m_ReachedPatrolPoint) {
        tmpAction = BehaviorIntelligentBumpToward(in location, false, false);
        if (null != tmpAction) {
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }

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

    private ActorAction ExecuteReport(List<Percept> percepts)
    {
      var enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", FilterNearest(enemies).Percepted.Name));
      }
      string text;
      switch (m_ReportStage)
      {
        case 0:
          ++m_ReportStage;
          text = DescribePercept(m_LastRaidHeard, m_Actor.Leader);
          return new ActionSay(m_Actor, m_Actor.Leader, (string.IsNullOrEmpty(text) ? "No raids heard." : text), RogueGame.Sayflags.NONE);
        case 1:
          ++m_ReportStage;
          text = DescribePercept(m_LastEnemySaw, m_Actor.Leader);
          return new ActionSay(m_Actor, m_Actor.Leader, (string.IsNullOrEmpty(text) ? "No enemies sighted." : text), RogueGame.Sayflags.NONE);
        case 2:
          ++m_ReportStage;
          text = DescribePercept(m_LastItemsSaw, m_Actor.Leader);
          return new ActionSay(m_Actor, m_Actor.Leader, (string.IsNullOrEmpty(text) ? "No items sighted." : text), RogueGame.Sayflags.NONE);
        case 3:
          ++m_ReportStage;
          text = DescribePercept(m_LastSoldierSaw, m_Actor.Leader);
          return new ActionSay(m_Actor, m_Actor.Leader, (string.IsNullOrEmpty(text) ? "No soldiers sighted." : text), RogueGame.Sayflags.NONE);
        default:
          SetOrder(null);
          return new ActionSay(m_Actor, m_Actor.Leader, "That's it.", RogueGame.Sayflags.NONE);
      }
    }

    private ActorAction ExecuteSleepNow(RogueGame game, List<Percept> percepts)
    {
      var enemies = FilterEnemies(percepts);
      if (enemies != null) {
        SetOrder(null);
        Actor actor = FilterNearest(enemies).Percepted;
        return new ActionShout(m_Actor, string.Format("{0} sighted!!", actor.Name));
      }
      if (m_Actor.CanSleep(out string reason)) {
        return (m_Actor.Location.Map.LocalTime.TurnCounter % 2 == 0 ? (ActorAction)(new ActionSleep(m_Actor)) : new ActionWait(m_Actor));
      }
      SetOrder(null);
      game.DoEmote(m_Actor, string.Format("I can't sleep now : {0}.", reason));
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

    public bool IsRationalTradeItem(Item offeredItem)    // Cf. ActorControllerAI::IsInterestingTradeItem
    {
#if DEBUG
      if (!m_Actor.Model.Abilities.CanTrade) throw new ArgumentOutOfRangeException(nameof(m_Actor),"both parties trading must be capable of it");
#endif
      return IsInterestingItem(offeredItem);
    }

    private void MaximizeRangedTargets(List<Point> dests, List<Percept_<Actor>> enemies)
    {
      if (null == dests || 2>dests.Count) return;

      var targets = new Dictionary<Point,int>();
      int max_range = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
      foreach(Point pt in dests) {
        targets[pt] = enemies.Count(p => LOS.CanTraceHypotheticalFireLine(new Location(m_Actor.Location.Map,pt), p.Location, max_range, m_Actor));
      }
      int max_LoF = targets.Values.Max();
      dests.RemoveAll(pt => max_LoF>targets[pt]);
    }

    protected List<ItemRangedWeapon> GetAvailableRangedWeapons()
    {
      IEnumerable<ItemRangedWeapon> tmp_rw = ((!Directives.CanFireWeapons || m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) ? null : m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)));
      return (null!=tmp_rw && tmp_rw.Any() ? tmp_rw.ToList() : null);
    }

#nullable enable
    protected bool HasBehaviorThatRecallsToSurface {
      get {
        if (null != m_Actor.Threats) return true;   // hunting threat
        if (null != m_Actor.InterestingLocs) return true;   // tourism
        var leader = m_Actor.LiveLeader;
        if (null != leader) {
          if (leader.IsPlayer) return true; // typically true...staying in the subway forever isn't a good idea
          if ((leader.Controller as OrderableAI)?.HasBehaviorThatRecallsToSurface ?? false) return true;   // if leader has recall-to-surface behavior, following him recalls to surface
        }
        return false;
      }
    }

    static public void BeforeRaid(RaidType raid, in Location location)
    {
      static string text(RaidType raid) {
        switch (raid) {
          case RaidType.BIKERS: return "motorcycles coming";
          case RaidType.GANGSTA: return "cars coming";
          case RaidType.BLACKOPS: return "a chopper hovering";
          case RaidType.SURVIVORS: return "honking coming";
          case RaidType.NATGUARD: return "the army coming";
          case RaidType.ARMY_SUPLLIES: return "a chopper hovering";
          default: throw new InvalidProgramException(string.Format("unhandled raidtype {0}", raid.ToString()));
        }
      }

      _lastRaidHeard = new Percept(text(raid), location.Map.LocalTime.TurnCounter, in location);
    }

    protected override void _onRaid(RaidType raid, in Location location)
    {
      m_LastRaidHeard = _lastRaidHeard!;
    }

    static public void AfterRaid() { _lastRaidHeard = null; }
#nullable restore

    // Behaviors and support functions
    // but all body armors are equipped to the torso slot(?)
    private ItemBodyArmor? GetEquippedBodyArmor()
    {
      return m_Actor.Inventory.GetFirst<ItemBodyArmor>(it => it.IsEquipped);
    }

#nullable enable
    protected void BehaviorEquipBestBodyArmor()
    {
      var bestBodyArmor = m_Actor.GetBestBodyArmor();
      if (bestBodyArmor == null) return;
      if (GetEquippedBodyArmor() != bestBodyArmor) bestBodyArmor.EquippedBy(m_Actor);
    }
#nullable restore

    protected ActorAction ManageMeleeRisk(List<ItemRangedWeapon> available_ranged_weapons)
    {
      ActorAction tmpAction = null;
      if ((null != _retreat || null != _run_retreat) && null != available_ranged_weapons && null!=_enemies) {
        // ranged weapon: prefer to maintain LoF when retreating
        MaximizeRangedTargets(_retreat, _enemies);
        MaximizeRangedTargets(_run_retreat, _enemies);
        IEnumerable<Actor> fast_enemies = _enemies.Select(p => p.Percepted as Actor).Where(a => a.Speed >= 2 * m_Actor.Speed);   // typically rats.
        if (fast_enemies.Any()) return null;    // not practical to run from rats.  (but we still could adjust our position to minimize bites)
        // ranged weapon: fast retreat ok
        // XXX but against ranged-weapon targets or no speed advantage may prefer one-shot kills, etc.
        // XXX we also want to be close enough to fire at all
        // XXX we actually need to signal DecideMove to request additional processing.
        _caller = CallChain.ManageMeleeRisk;
        tmpAction = (_safe_run_retreat ? DecideMove(_legal_steps, _run_retreat) : ((null != _retreat) ? DecideMove(_retreat) : null));
        _caller = CallChain.NONE;
        if (null != tmpAction) {
          if (tmpAction is ActionMoveStep test) {
            // all setup should have been done in the run-retreat case
            if (!_safe_run_retreat) {
              m_Actor.IsRunning = RunIfAdvisable(test.dest);
              if (m_Actor.IsRunning && m_Actor.RunIsFreeMove) {
                   // * if attackable enemies, attack
                   // * else if not in damage field, rest (to reset AP) or try to improve tactical positioning/get away further
                   // * else do not contrain processing (null out)
                   // XXX \todo once enemies_in_FOV is location-based we can use different sequences safely for the engaged and not-engaged cases
                   var next = new ActionSequence(m_Actor,new int[] { (int)ZeroAryBehaviors.AttackWithoutMoving_ObjAI, (int)ZeroAryBehaviors.WaitIfSafe_ObjAI });
                   SetObjective(new Goal_NextCombatAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, next, next));
              }
            } else m_Actor.Run();
          }
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
      }

      if (null != _retreat) {
        if (   WillTireAfterAttack(m_Actor) // need stamina to melee: slow retreat ok
            || null != _slow_melee_threat) {    // have slow enemies nearby
          _caller = CallChain.ManageMeleeRisk;
	      tmpAction = DecideMove(_retreat);
          _caller = CallChain.NONE;
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
      }
      // end melee risk management check
#if USING_ESCAPE_MOVES
      if (null != _escape_moves) throw new InvalidOperationException("need to use escape moves");
#endif
      return null;
    }

    private void NavigateFilter(HashSet<Location> tainted)
    {
#if DEBUG
      if (null==tainted || 0 >=tainted.Count) throw new ArgumentNullException(nameof(tainted));
      if (tainted.Contains(m_Actor.Location)) throw new InvalidOperationException("tainted.Contains(m_Actor.Location.Position)");
#endif
      int min_dist = int.MaxValue;
      int max_dist = int.MinValue;
      var dist = new Dictionary<Location,int>();
      foreach(var loc in tainted) {
        int tmp = Rules.GridDistance(in loc, m_Actor.Location);
        dist.Add(loc, tmp);
        if (tmp > max_dist) max_dist = tmp;
        if (tmp < min_dist) min_dist = tmp;
      }
      if (min_dist >= max_dist-min_dist) return;
      int min_2x = 2*min_dist;
      foreach(var x in dist) {
        if (x.Value > min_2x) tainted.Remove(x.Key);
      }
    }

    private ActorAction Equip(ItemRangedWeapon rw) {
      if (!rw.IsEquipped /* && m_Actor.CanEquip(rw) */) rw.EquippedBy(m_Actor);
      if (0 >= rw.Ammo) {
        ItemAmmo ammo = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
        if (null != ammo) return UseAmmo(ammo, rw);
      }
      return null;
    }

    // want to be able to use this contrafactually
    private HashSet<Location> GetRangedAttackFromZone(List<Percept_<Actor>> enemies)   // XXX does not handle firing through exits
    {
      var ret = new HashSet<Location>();
      var danger = new HashSet<Location>();
      foreach(var en in enemies) {
        if (null!=(en.Percepted.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo()) continue;
        foreach(var pt in en.Location.Position.Adjacent()) {
          var test = new Location(en.Location.Map,pt);
          if (Map.Canonical(ref test)) danger.Add(test);
        }
      }
      var range = m_Actor.CurrentRangedAttack.Range;
      var optimal_FOV = LOS.OptimalFOV(range);
      foreach(var en in enemies) {
        foreach(var p in optimal_FOV) {
          var pt = p + en.Location.Position;
          var test = new Location(en.Location.Map,pt);
          if (!Map.Canonical(ref test)) continue;
          if (ret.Contains(test)) continue;
          if (danger.Contains(test)) continue;
          if (!test.Map.IsWalkableFor(test.Position,m_Actor)) continue;
          var LoF = new List<Point>();  // XXX micro-optimization?: create once, clear N rather than create N
          if (LOS.CanTraceHypotheticalFireLine(in test, en.Location, range, m_Actor, LoF)) {
            ret.Add(test);
            if (4 >= LoF.Count || 0 == p.X || 0 == p.Y || p.X == p.Y || p.X == -p.Y) {  // some conditions where adding the whole line of fire is safe
              int n = LoF.Count-1;
              while(1 < n--) {
                var transit = new Location(test.Map,LoF[n]);
                if (!Map.Canonical(ref transit)) throw new InvalidProgramException("line of fire contains impossible locations");
                if (!danger.Contains(transit)) ret.Add(transit);
              }
            }
          }
          // if "safe" attack possible init danger in different/earlier loop
        }
      }
#if DEBUG
      if (ret.Contains(m_Actor.Location)) throw new InvalidProgramException("trying to fire from a location without line of fire");
#else
      ret.Remove(m_Actor.Location);    // if we could fire from here we wouldn't have called this; prevents invariant crash later
#endif
      return ret;
    }

    // forked from BaseAI::BehaviorEquipWeapon
    protected ActorAction BehaviorEquipWeapon(List<ItemRangedWeapon> available_ranged_weapons)
    {
#if DEBUG
      if ((null == available_ranged_weapons) != (null == GetBestRangedWeaponWithAmmo())) throw new InvalidOperationException("(null == available_ranged_weapons) != (null == GetBestRangedWeaponWithAmmo())");
#endif

      var game = RogueGame.Game;

      // migrated from CivilianAI::SelectAction
      ActorAction tmpAction = null;
      if (null != _enemies) {
        tmpAction = ScanForMeleeSnipe();
        if (null != tmpAction) return tmpAction;
      }

      // if no ranged weapons, use BaseAI
      // OrderableAI::GetAvailableRangedWeapons knows about AI disabling of ranged weapons
      if (null == available_ranged_weapons) return base.BehaviorEquipWeapon();

      // if no enemies in sight, reload all ranged weapons and then equip longest-range weapon
      // XXX there may be more important objectives than this
      if (null == _enemies) {
        foreach(ItemRangedWeapon rw in available_ranged_weapons) {
          if (rw.Model.MaxAmmo <= rw.Ammo) continue;
          ItemAmmo am = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
          if (null == am) continue;
          if (0 == rw.Ammo || (rw.Model.MaxAmmo - rw.Ammo) >= am.Quantity) return Equip(rw) ?? new ActionUseItem(m_Actor, am);
        }
        return Equip(GetBestRangedWeaponWithAmmo());
      }
      // at this point, null != enemies, we have a ranged weapon available, and melee one-shot is not feasible
      // also, damage field should be non-null because enemies is non-null

      int best_range = available_ranged_weapons.Max(rw => rw.Model.Attack.Range);
      var en_in_range = FilterFireTargets(_enemies,best_range);

      // if no enemies in range, or just one available ranged weapon, use the best one
      if (null == en_in_range || 1==available_ranged_weapons.Count) {
        tmpAction = Equip(GetBestRangedWeaponWithAmmo());
        if (null != tmpAction) return tmpAction;
      }

      if (null == en_in_range && null != _legal_steps) {
        var percepts2 = FilterPossibleFireTargets(_enemies);
		if (null != percepts2) {
		  IEnumerable<Point> tmp = _legal_steps.Where(p=>AnyContrafactualFireTargets(percepts2,p));
		  if (tmp.Any()) {
	        tmpAction = DecideMove(tmp);
            if (null != tmpAction) {
              m_Actor.Activity = Activity.FIGHTING;
              if (tmpAction is ActionMoveStep test) {
                m_Actor.IsRunning = RunIfAdvisable(test.dest);
                if (m_Actor.IsRunning && m_Actor.RunIsFreeMove) {
                   // * if attackable enemies, attack
                   // * else if not in damage field, rest (to reset AP) or try to improve tactical positioning/get away further
                   // * else do not contrain processing (null out)
                   // XXX \todo once enemies_in_FOV is location-based we can use different sequences safely for the engaged and not-engaged cases
                   var next = new ActionSequence(m_Actor,new int[] { (int)ZeroAryBehaviors.AttackWithoutMoving_ObjAI, (int)ZeroAryBehaviors.WaitIfSafe_ObjAI });
                   SetObjective(new Goal_NextCombatAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, next, next));
                }
              }
              return tmpAction;
            }
		  }
        }

        // XXX need to use floodfill pathfinder
        var fire_from_here = GetRangedAttackFromZone(_enemies);
        if (2<=fire_from_here.Count) NavigateFilter(fire_from_here);
        _last_move = null;  // could backtrack legitimately
        tmpAction = BehaviorPathTo(fire_from_here);
        if (tmpAction is ActionShove shove) {
          if (null != _enemies) {
//          Dictionary<Point,int> risk = null;  // optimized out
            List<Location> norisk = null;
            // this might endanger the shove target...be more careful
            foreach(Direction d in Direction.COMPASS) {
              var dest = shove.Target.Location+d;
              if (!shove.Target.CanBeShovedTo(dest.Position)) continue;
              // _damage_field expected to be non-null as we have at least one enemy in sight
              int incoming = RiskAt(in dest);
              if (0 >= incoming) (norisk ??= new List<Location>()).Add(dest);
            }
            if (null != norisk) {
              var target_rw = (shove.Target.Controller as ObjectiveAI).GetBestRangedWeaponWithAmmo();
              if (null != target_rw) {
                Dictionary<Point,int> targets = null;
                var my_loc = shove.Target.Location.Map.Denormalize(m_Actor.Location);   // expected to be non-null since we could shove
                foreach(var en in _enemies) {
                  var loc = shove.Target.Location.Map.Denormalize(en.Location);
                  if (null == loc) continue;
                  foreach(var dest in norisk) {
                    var line = new List<Point>();
                    if (!LOS.CanTraceHypotheticalFireLine(in dest, loc.Value, target_rw.Model.Attack.Range, shove.Target, line)) continue;
                    if (!Rules.IsAdjacent(m_Actor.Location,dest) && line.Contains(shove.Target.Location.Position)) continue;
                    if (null == targets) targets = new Dictionary<Point, int> { [dest.Position] = 1 };
                    else if (!targets.ContainsKey(dest.Position)) targets[dest.Position] = 1;
                    else ++targets[dest.Position];
                  }
                }
                if (null == targets) tmpAction = null;  // shove is tactically contra-indicated
                else {
                  var choice = Rules.Get.DiceRoller.Choose(targets);
                  return new ActionShove(m_Actor,shove.Target,Direction.FromVector(choice.Key-shove.Target.Location.Position));
                }
              } else tmpAction = null;  // shove is tactically contra-indicated
            } else tmpAction = null;  // shove is tactically contra-indicated
          } else {
            // HMM....
          }
        }
        if (null != tmpAction) return tmpAction;
      }

      if (null == en_in_range) return null; // no enemies in range, no constructive action: do something else

      // filter immediate threat by being in range
      Zaimoni.Data.Stack<Actor> immediate_threat_in_range = default;
      if (null != _immediate_threat) immediate_threat_in_range = en_in_range.Select(p => p.Percepted).ToZStack(a => _immediate_threat.Contains(a));

      if (1 == available_ranged_weapons.Count) {
        if (1 == en_in_range.Count) {
          return BehaviorRangedAttack(en_in_range[0].Percepted);
        } else if (1 == immediate_threat_in_range.Count) {
          return BehaviorRangedAttack(immediate_threat_in_range[0]);
        }
      }

      // Get ETA stats
      var best_weapon_ETAs = new Dictionary<Actor,int>();
      var best_weapons = new Dictionary<Actor,ItemRangedWeapon>();
      if (1<available_ranged_weapons.Count) {
        foreach(var p in en_in_range) {
          Actor a = p.Percepted;
          int range = Rules.InteractionDistance(m_Actor.Location, p.Location);
          foreach(ItemRangedWeapon rw in available_ranged_weapons) {
            if (range > rw.Model.Attack.Range) continue;
            ETAToKill(a, range, rw,best_weapon_ETAs, best_weapons);
          }
        }
      } else {
        foreach(var p in en_in_range) {
          Actor a = p.Percepted;
          ETAToKill(a,Rules.InteractionDistance(m_Actor.Location,p.Location), available_ranged_weapons[0], best_weapon_ETAs);
        }
      }

      // cf above: we got here because there were multiple ranged weapons to choose from in these cases
      if (1 == en_in_range.Count) {
        Actor a = en_in_range[0].Percepted;
        return Equip(best_weapons[a]) ?? BehaviorRangedAttack(a);
      } else if (1 == immediate_threat_in_range.Count) {
        Actor a = immediate_threat_in_range[0];
        return Equip(best_weapons[a]) ?? BehaviorRangedAttack(a);
      }
      // at this point: there definitely is more than one enemy in range
      // if there are any immediate threat, there are at least two immediate threat
      if (2 <= immediate_threat_in_range.Count) {
        int ETA_min = immediate_threat_in_range.Min(a => best_weapon_ETAs[a]);
        immediate_threat_in_range.SelfFilter(a => best_weapon_ETAs[a] == ETA_min);
        if (2 <= immediate_threat_in_range.Count) {
          int HP_min = ((2 >= ETA_min) ? immediate_threat_in_range.Max(a => a.HitPoints) : immediate_threat_in_range.Min(a => a.HitPoints));
          immediate_threat_in_range.SelfFilter(a => a.HitPoints == HP_min);
          if (2 <= immediate_threat_in_range.Count) {
           int dist_min = immediate_threat_in_range.Min(a => Rules.InteractionDistance(m_Actor.Location,a.Location));
           immediate_threat_in_range.SelfFilter(a => Rules.InteractionDistance(m_Actor.Location, a.Location) == dist_min);
          }
        }
        Actor actor = immediate_threat_in_range[0];
        if (1 < available_ranged_weapons.Count) {
         tmpAction = Equip(best_weapons[actor]);
         if (null != tmpAction) return tmpAction;
        }
        return BehaviorRangedAttack(actor);
      }
      // at this point, no immediate threat in range
      {
        int ETA_min = en_in_range.Min(p => best_weapon_ETAs[p.Percepted]);
        if (2==ETA_min) {
          // snipe something
          en_in_range = en_in_range.Filter(a => ETA_min == best_weapon_ETAs[a]);
          if (2<=en_in_range.Count) {
            int HP_max = en_in_range.Max(p => p.Percepted.HitPoints);
            en_in_range = en_in_range.Filter(a => a.HitPoints == HP_max);
            if (2<=en_in_range.Count) {
             int dist_min = en_in_range.Min(p => Rules.InteractionDistance(m_Actor.Location,p.Location));
             en_in_range = en_in_range.Filter<Percept_<Actor>>(p => Rules.InteractionDistance(m_Actor.Location, p.Location) == dist_min);
            }
          }
          Actor actor = en_in_range[0].Percepted;
          if (1 < available_ranged_weapons.Count) {
            tmpAction = Equip(best_weapons[actor]);
            if (null != tmpAction) return tmpAction;
          }
          return BehaviorRangedAttack(actor);
        }
      }

      // just deal with something close
      {
        int dist_min = en_in_range.Min(p => Rules.InteractionDistance(m_Actor.Location,p.Location));
        en_in_range = en_in_range.Filter<Percept_<Actor>>(p => Rules.InteractionDistance(m_Actor.Location, p.Location) == dist_min);
        if (2<=en_in_range.Count) {
          int HP_min = en_in_range.Min(p => p.Percepted.HitPoints);
          en_in_range = en_in_range.Filter(a => a.HitPoints == HP_min);
        }
        Actor actor = en_in_range[0].Percepted;
        if (1 < available_ranged_weapons.Count) {
          tmpAction = Equip(best_weapons[actor]);
          if (null != tmpAction) return tmpAction;
        }
        return BehaviorRangedAttack(actor);
      }
    }

    // This is only called when the actor is hungry.  It doesn't need to do food value corrections
    protected ItemFood? GetBestEdibleItem()
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
    protected ItemFood? GetBestPerishableItem()
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

#nullable enable
    protected ActorAction? BehaviorEat()
    {
      var bestEdibleItem = GetBestEdibleItem();
      if (null == bestEdibleItem) return null;
      return (m_Actor.CanUse(bestEdibleItem) ? new ActionUseItem(m_Actor, bestEdibleItem) : null);
    }

    protected ActorAction? BehaviorEatProactively()
    {
      var bestEdibleItem = GetBestPerishableItem();
      if (null == bestEdibleItem) return null;
      return (m_Actor.CanUse(bestEdibleItem) ? new ActionUseItem(m_Actor, bestEdibleItem) : null);
    }
#nullable restore

    /// <returns>true if and only if light should be equipped</returns>
    protected bool BehaviorEquipLight()
    {
      var light = m_Actor.Inventory.GetBestMatching<ItemLight>(it => !it.IsUseless,(lhs,rhs) => {
          if (lhs.IsEquipped) return false;
          if (rhs.IsEquipped) return true;
          return lhs.IsLessUsableThan(rhs);
      });
      if (null == light) return false;
      if (Needs(light)) {
        light.EquippedBy(m_Actor);
        return true;
      }
      light.UnequippedBy(m_Actor);
      return false;
    }

    /// <returns>true if and only if a cell phone is required to be equipped</returns>
    protected bool BehaviorEquipCellPhone()
    {
      var phone = m_Actor.Inventory.GetBestMatching<ItemTracker>(it => it.CanTrackFollowersOrLeader && !it.IsUseless, (lhs, rhs) => {
          if (lhs.IsEquipped) return false;
          if (rhs.IsEquipped) return true;
          return lhs.Batteries>rhs.Batteries;
      });
      if (null == phone) return false;
      if (m_Actor.NeedActiveCellPhone) {    // VAPORWARE could dial 911, at least while that desk is manned
        phone.EquippedBy(m_Actor);
        return true;
      }
      phone.UnequippedBy(m_Actor);
      return false;
    }

    protected ActionThrowGrenade? BehaviorThrowGrenade()
    {
#if DEBUG
      if (null == _enemies) throw new ArgumentNullException(nameof(_enemies));
#endif
      if (3 > _enemies.Count) return null;
      ItemGrenade firstGrenade = m_Actor.Inventory.GetFirstMatching<ItemGrenade>();
      if (firstGrenade == null) return null;
      ItemGrenadeModel itemGrenadeModel = firstGrenade.Model;
      int maxRange = m_Actor.MaxThrowRange(itemGrenadeModel.MaxThrowDistance);
      int blast_radius = itemGrenadeModel.BlastAttack.Radius;
      var a_map = m_Actor.Location.Map;
      int my_dist;
      Point? bestSpot = null;
      int bestSpotScore = 0;
      foreach(var view in m_Actor.Controller.FOV) {
        if (blast_radius >= (my_dist = Rules.GridDistance(m_Actor.Location.Position, in view))) continue;
        if (maxRange < my_dist) continue;
        if (!a_map.GetTileModelAtExt(view).IsWalkable) continue;
        if (!LOS.CanTraceThrowLine(m_Actor.Location, in view, maxRange)) continue;
        if (_blast_field?.Contains(view) ?? false) continue;
        int score = 0;
        Rectangle blast_zone = new Rectangle(view - (Point)blast_radius, (Point)(2 * blast_radius + 1));
        // XXX \todo we want to evaluate the damage for where threat is *when the grenade explodes*
        if (   !blast_zone.Any(pt => {
                  var actorAt = a_map.GetActorAtExt(pt);
                  if (null == actorAt) return false;
#if DEBUG
                  if (actorAt == m_Actor) throw new InvalidProgramException("actorAt == m_Actor"); // integrity issue w/map
#endif
                  if (m_Actor.IsEnemyOf(actorAt)) {
                    score += (itemGrenadeModel.BlastAttack.DamageAt(Rules.GridDistance(new Location(a_map, view), actorAt.Location)) * actorAt.MaxHPs);
                    return false;
                  }
                  return true;
               })
            &&  score>bestSpotScore) {
            bestSpot = view;
            bestSpotScore = score;
          }
      }
      if (null == bestSpot) return null;
      if (!firstGrenade.IsEquipped) firstGrenade.EquippedBy(m_Actor);    // XXX required by the legality check
      var actorAction = new ActionThrowGrenade(m_Actor, bestSpot.Value);
#if DEBUG
      if (!actorAction.IsPerformable()) throw new InvalidProgramException("created illegal ActionThrowGrenade");  // invariant failure
#endif
      return actorAction;
    }

    private static string MakeCentricLocationDirection(Location from, Location to)
    {
      if (from.Map != to.Map) return string.Format("in {0}", to.Map.Name);
      Point v = to.Position - from.Position;
      return string.Format("{0} tiles to the {1}", (int) Rules.StdDistance(in v), Direction.ApproximateFromVector(v));
    }

    private bool IsItemWorthTellingAbout(Item it)
    {
      return it != null && !(it is ItemBarricadeMaterial) && (m_Actor.Inventory.IsEmpty || !m_Actor.Inventory.Contains(it));
    }

    protected string DescribePercept(Percept percept, Actor audience)
    {
      if (null == percept) return null;
      string str1 = MakeCentricLocationDirection(m_Actor.Location, percept.Location);
      string str2 = string.Format("{0} ago", WorldTime.MakeTimeDurationMessage(m_Actor.Location.Map.LocalTime.TurnCounter - percept.Turn));

      if (percept.Percepted is Actor old_a) return string.Format("I saw {0} {1} {2}.", old_a.Name, str1, str2);
      else if (percept.Percepted is Inventory inventory) {
        if (inventory.IsEmpty) return null;
        Item it = Rules.Get.DiceRoller.Choose(inventory.Items);
        if (!IsItemWorthTellingAbout(it)) return null;
        int num = audience.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
        if ((double) Rules.StdDistance(percept.Location, audience.Location) <= (double) (2 + num)) return null;
        return string.Format("I saw {0} {1} {2}.", it.AName, str1, str2);
      } else if (percept.Percepted is string str3) return string.Format("I heard {0} {1} {2}!", str3, str1, str2);
      else throw new InvalidOperationException("unhandled percept.Percepted type");
    }

    protected string DescribePercept(Percept_<Actor> percept, Actor audience)
    {
      if (null == percept) return null;
      string str1 = MakeCentricLocationDirection(m_Actor.Location, percept.Location);
      string str2 = string.Format("{0} ago", WorldTime.MakeTimeDurationMessage(m_Actor.Location.Map.LocalTime.TurnCounter - percept.Turn));
      return string.Format("I saw {0} {1} {2}.", percept.Percepted.Name, str1, str2);
    }

    protected ActionSay? BehaviorTellFriendAboutPercept(Percept percept, int chance)
    {
      var friends = AdjacentFriends();
      if (null == friends) return null;
      var rules = Rules.Get;
      if (!rules.RollChance(chance)) return null;
      Actor actorAt1 = rules.DiceRoller.Choose(friends);
      string text = DescribePercept(percept, actorAt1);
      return string.IsNullOrEmpty(text) ? null : new ActionSay(m_Actor, actorAt1, text, RogueGame.Sayflags.NONE);
    }

    protected ActionSay? BehaviorTellFriendAboutPercept(Percept_<Actor> percept, int chance)
    {
      var friends = AdjacentFriends();
      if (null == friends) return null;
      var rules = Rules.Get;
      if (!rules.RollChance(chance)) return null;
      Actor actorAt1 = rules.DiceRoller.Choose(friends);
      string text = DescribePercept(percept, actorAt1);
      return string.IsNullOrEmpty(text) ? null : new ActionSay(m_Actor, actorAt1, text, RogueGame.Sayflags.NONE);
    }

    private ActorAction BehaviorSecurePerimeter()
    {
      Map map = m_Actor.Location.Map;
      var want_to_resolve = new Dictionary<Point,int>();
      foreach (Point position in m_Actor.Controller.FOV) {
        if (!(map.GetMapObjectAt(position) is DoorWindow door)) continue;
        if (door.IsOpen && m_Actor.CanClose(door)) {
          if (Rules.IsAdjacent(in position, m_Actor.Location.Position)) {
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
          want_to_resolve[position] = Rules.GridDistance(in position, m_Actor.Location.Position);
        }
        if (door.IsWindow && !door.IsBarricaded && m_Actor.CanBarricade(door)) {
          if (Rules.IsAdjacent(in position, m_Actor.Location.Position))
            return new ActionBarricadeDoor(m_Actor, door);
          want_to_resolve[position] = Rules.GridDistance(in position, m_Actor.Location.Position);
        }
      }
      // we could floodfill this, of course -- but everything is in LoS so try something else
      // we want to head for a nearest objective in such a way that the distance to all of the other objectives is minimized
      return BehaviorEfficientlyHeadFor(want_to_resolve);
    }

    protected ActionShout? BehaviorWarnFriends(List<Percept_<Actor>> friends, Actor nearestEnemy)
    {
#if DEBUG
      if (null == nearestEnemy) throw new ArgumentNullException(nameof(nearestEnemy));
#endif
      if (Rules.IsAdjacent(m_Actor.Location, nearestEnemy.Location)) return null;
      bool need_waking(Actor a) {
        return a.IsSleeping && Rules.LOUD_NOISE_RADIUS >= Rules.GridDistance(a.Location, m_Actor.Location);
      }

      if (m_Actor.HasLeader && need_waking(m_Actor.Leader)) return new ActionShout(m_Actor);
      foreach (var friend in friends) {
        var actor = friend.Percepted;
        if (actor != m_Actor && need_waking(actor) && !m_Actor.IsEnemyOf(actor) && actor.IsEnemyOf(nearestEnemy)) {
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
      // next test game \todo (m_Actor.CountFollowers+1)/ 2 ,so a single follower isn't left behind
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

#nullable enable
    protected ActorAction? BehaviorLeadActor(Percept_<Actor> target)
    {
      Actor target1 = target.Percepted;
      if (!m_Actor.CanTakeLeadOf(target1)) return null;
      if (Rules.IsAdjacent(m_Actor.Location, target1.Location)) return new ActionTakeLead(m_Actor, target1);
      if (!m_Actor.WillActAgainBefore(target1)) {
        // ai only can lead ai (would need extra handling for dogs since they're not ObjectiveAI anyway)
        // need an after-action "hint" to the target on where/who to go to
        if (!(target1.Controller is OrderableAI targ_ai)) return null;
        targ_ai.CancelPathTo(m_Actor);  // do not count pathing to *me* as focused
        if (targ_ai.IsFocused) return null;
        int t0 = Session.Get.WorldTime.TurnCounter+m_Actor.HowManyTimesOtherActs(1,target1)-(m_Actor.IsBefore(target1) ? 1 : 0);
        targ_ai.SetObjective(new Goal_HintPathToActor(t0, target1, m_Actor));
      }
      return BehaviorIntelligentBumpToward(target1.Location, false, false);
    }
#nullable restore

    protected ActionUseItem? BehaviorUseMedecine(int factorHealing, int factorStamina, int factorSleep, int factorCure, int factorSan)
    {
      Inventory inventory = m_Actor.Inventory;
      if (inventory.IsEmpty) return null;

      // OrderableAI::BehaviorUseEntertainment has been upgraded to know about sanity medications.

      // \todo should be less finicky about SLP/Inf/SAN when enemies in sight
      bool needHP = m_Actor.HitPoints < m_Actor.MaxHPs;
      bool needSTA = m_Actor.IsTired;
      bool needSLP = m_Actor.WouldLikeToSleep;
      bool needCure = m_Actor.Infection > 0;
//    bool needSan = m_Actor.Model.Abilities.HasSanity && m_Actor.Sanity < 3*m_Actor.MaxSanity/4;   // Historical; retained in ObjectiveAI::WantRestoreSAN
      bool needSan = m_Actor.Model.Abilities.HasSanity && m_Actor.Sanity < m_Actor.MaxSanity/4;   // in immediate danger of losing control
      if (!needHP && !needSTA && (!needSLP && !needCure) && !needSan) return null;
      // XXX \todo following does not handle bandaids vs. medikit properly at low hp deficits
      var choiceEval = Choose(inventory.GetItemsByType<ItemMedicine>(), it =>
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

    protected override ActorAction BehaviorChargeEnemy(Percept_<Actor> target, bool canCheckBreak, bool canCheckPush)
    {
      Actor actor = target.Percepted;
      ActorAction? tmpAction = BehaviorMeleeAttack(actor);
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
          if (0 < m_Actor.Location.Map.TrapsUnavoidableMaxDamageAtFor(ptA, m_Actor)) return float.NaN; // just cancel the move
          return (float)Rules.StdDistance(in ptA, in ptB);
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
            (LoF ??= new List<List<Point>>()).Add(line);
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
        if (null != tmpAction) m_Actor.TargetedActivity(Activity.FIGHTING, actor);
      }
    }

    private HashSet<Point>? AlliesNeedLoFvs(Actor enemy)
    {
      var friends = friends_in_FOV;
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

    public ActorAction BehaviorWalkAwayFrom(IEnumerable<Location> goals, HashSet<Point> LoF_reserve)
    {
      var leader = m_Actor.LiveLeader;
      var ranged_target = null != leader ? (leader.Controller as ObjectiveAI)?.GetNearestTargetFor() : null;
      Actor actor = ranged_target?.Key;
      bool checkLeaderLof = null != actor;
      List<Point> leaderLoF = null;
      if (checkLeaderLof) {
        leaderLoF = new List<Point>(1);
        LOS.CanTraceFireLine(leader.Location, actor.Location, ranged_target.Value.Value.Model.Attack.Range, leaderLoF);
      }
      var choiceEval = Choose(Direction.COMPASS, dir => {
        Location location = m_Actor.Location + dir;
        var bump = Rules.IsBumpableFor(m_Actor, in location);
        if (!IsValidFleeingAction(bump)) return float.NaN;
#if DEBUG
        if (!bump.IsPerformable()) throw new InvalidOperationException("non-null non-performable bump");
#else
        if (!bump.IsPerformable()) return float.NaN;
#endif
        float num = SafetyFrom(in location, goals);
        if (LoF_reserve?.Contains(location.Position) ?? false) --num;
        if (null != leader) {
          num -= (float)Rules.StdDistance(in location, leader.Location);
          if (leaderLoF?.Contains(location.Position) ?? false) num -= (float)IN_LEADER_LOF_SAFETY_PENALTY;
        }
        return num;
      }, (a, b) => a > b);
      return ((choiceEval != null) ? new ActionBump(m_Actor, choiceEval.Choice) : null);
    }

    private ActorAction? BehaviorFlee(Actor enemy, HashSet<Point>? LoF_reserve, bool doRun, string[] emotes)
    {
      var game = RogueGame.Game;
      var rules = Rules.Get;
      ActorAction? tmpAction = null;
      if (rules.RollChance(EMOTE_FLEE_CHANCE))
        game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[0], enemy.Name));
        // All OrderableAI instances currently can both use map objects, and barricade
        // there is an inventory check requirement on barricading as well
        // due to preconditions it is mutually exclusive that a door be closable or barricadable
        // however, we do not want to obstruct line of fire of allies
        {
        bool could_barricade = m_Actor.CouldBarricade();
        // historically was Dictionary<Point,DoorWindow> but we don't actually use the Point
        List<DoorWindow>? close_doors = null;
        List<DoorWindow>? barricade_doors = null;
        foreach(var dir in Direction.COMPASS) {
          var pt = m_Actor.Location.Position + dir;
          if (LoF_reserve?.Contains(pt) ?? false) continue;
          if (!(m_Actor.Location.Map.GetMapObjectAt(pt) is DoorWindow door)) continue;
          if (m_Actor.Location.Map!=enemy.Location.Map || !IsBetween(m_Actor.Location.Position, in pt, enemy.Location.Position)) continue;
          // magic constant 4 is the maximum number of doors that may reasonably be adjacent to a point with our map generation
          if (m_Actor.CanClose(door)) {
            if (!Rules.IsAdjacent(in pt, enemy.Location.Position) || !enemy.CanClose(door)) (close_doors ??= new List<DoorWindow>(4)).Add(door);
          } else if (could_barricade && door.CanBarricade()) {
            (barricade_doors ??= new List<DoorWindow>(4)).Add(door);
          }
        }
        if (null != close_doors) {
          var dest = rules.DiceRoller.Choose(close_doors);
          SetObjective(new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, dest.Location));
          return new ActionCloseDoor(m_Actor, dest, m_Actor.Location == PrevLocation);
        } else if (null != barricade_doors) {
          var dest = rules.DiceRoller.Choose(barricade_doors);
          SetObjective(new Goal_BreakLineOfSight(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, dest.Location));
          return new ActionBarricadeDoor(m_Actor, dest);
        }
        }   // enable automatic GC
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && (Lighting.DARKNESS== m_Actor.Location.Map.Lighting || rules.RollChance(FLEE_THROUGH_EXIT_CHANCE))) {
          tmpAction = BehaviorUseExit(UseExitFlags.NONE);
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
        // XXX we should run for the exit here ...
        if (null == _damage_field || !_damage_field.ContainsKey(m_Actor.Location.Position)) {
          tmpAction = BehaviorUseMedecine(2, 2, 1, 0, 0);
          if (null != tmpAction) {
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        // XXX or run for the exit here
        {
        var enemies_FOV = enemies_in_FOV;
        if (null != enemies_FOV) {
          tmpAction = BehaviorWalkAwayFrom(enemies_FOV.Keys, LoF_reserve);
          if (null != tmpAction) {
            if (doRun) m_Actor.Run();
            m_Actor.Activity = Activity.FLEEING;
            return tmpAction;
          }
        }
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (rules.RollChance(EMOTE_FLEE_TRAPPED_CHANCE)) game.DoEmote(m_Actor, emotes[1], true);
          return BehaviorMeleeAttack(enemy);
        }
        return null;
    }

    private List<Engine._Action.MoveStep>? _closeIn(Location e_loc) {
        int current_distance = Rules.GridDistance(m_Actor.Location, in e_loc);
#if DEBUG
        if (2 != current_distance) throw new InvalidOperationException("use something more general");
#endif

        Predicate<Location> closing_in = loc => current_distance > Rules.GridDistance(in loc, in e_loc);

        var first = Engine.Op.MoveStep.outbound(m_Actor.Location, m_Actor, closing_in, false);
        if (null == first) return null;

        var last = e_loc.Adjacent(loc => m_Actor.CanEnter(loc));
        if (null == last) return null;

        bool want_to_run = !m_Actor.RunIsFreeMove;
        bool will_tire_after_run = m_Actor.WillTireAfter(Rules.STAMINA_COST_RUNNING + m_Actor.NightSTApenalty);

        int best_STA = int.MaxValue;
        Dictionary<Location, int> STA_costs = new();
        var STA_cost = STA_delta(0, 1, 0, 0); // one melee attack
        List<Engine._Action.MoveStep> routes = new();
        foreach(var move in first.Value.Key) {
          if (!last.Contains(move.dest)) continue;
          var test_STA = m_Actor.RunningStaminaCost(move.dest);
          if (best_STA < test_STA) continue;
          var act = move.Bind(m_Actor);
          if (!(act is Engine._Action.MoveStep step)) continue;
          if (best_STA > test_STA) {
            best_STA = test_STA;
            routes.Clear();
          }
          if (!will_tire_after_run) {
            var dest = move.dest;
            if (!STA_costs.TryGetValue(dest, out var dest_cost)) STA_costs.Add(dest, dest_cost = m_Actor.RunningStaminaCost(dest));
            if (!m_Actor.WillTireAfter(STA_cost + dest_cost)) {
              routes.Add(new Engine._Action.MoveStep(m_Actor.Location, move.dest, true, m_Actor));
              continue;
            }
          }
          routes.Add(step);
        }

        return 0 < routes.Count ? routes : null;
    }

    private List<KeyValuePair<List<Engine.Op.MoveStep>, ActorAction>>? _chargeTowards(Location e_loc)
    {
        // check if this is "too tiring" under optimal circumstances
        var STA_cost = STA_delta(0, 1, 0, 0); // one melee attack
        if (m_Actor.WillTireAfter(STA_cost + 3*(Rules.STAMINA_COST_RUNNING + m_Actor.NightSTApenalty) - Rules.STAMINA_REGEN_PER_TURN)) return null;

        int current_distance = Rules.GridDistance(m_Actor.Location, in e_loc);
#if DEBUG
        if (3 > current_distance) throw new InvalidOperationException("should be using other special cases for that");
        if (3 < current_distance) throw new InvalidOperationException("review implementation before attmepting this case");
#endif

        Predicate<Location> closing_in = loc => current_distance > Rules.GridDistance(in loc, in e_loc);

        var first = Engine.Op.MoveStep.outbound(m_Actor.Location, m_Actor, closing_in, true);
        if (null == first) return null;

        var last = e_loc.Adjacent(loc => m_Actor.CanEnter(loc));
        if (null == last) return null;

        --current_distance;
        Dictionary<Location, KeyValuePair<List<Engine.Op.MoveStep>, HashSet<Location>>> intermediate = new();
        foreach(var origin in first.Value.Value) {
          var test = Engine.Op.MoveStep.outbound(origin, m_Actor, closing_in, true);
          if (null != test) intermediate.Add(origin, test.Value);
        }
        if (0 >= intermediate.Count) return null;

        //m_Actor.RunningStaminaCost(step.dest)
        int best_STA = int.MaxValue;
        Dictionary<Location, int> STA_costs = new();
        Dictionary<Location, ActorAction> steps = new();
        List<KeyValuePair<List<Engine.Op.MoveStep>, ActorAction>> routes = new();
        foreach(var move in first.Value.Key) {
          var act = move.Bind(m_Actor);
          if (null == act) continue;
          var stage = move.dest;
          if (!intermediate.TryGetValue(stage, out var cache)) continue;
          var stage_STA = m_Actor.RunningStaminaCost(stage);
          foreach(var next_move in cache.Key) {
            var dest = next_move.dest;
            if (!STA_costs.TryGetValue(dest, out var dest_cost)) STA_costs.Add(dest, dest_cost = m_Actor.RunningStaminaCost(dest));
            var test_STA = STA_cost + stage_STA + dest_cost + (Rules.STAMINA_COST_RUNNING + m_Actor.NightSTApenalty) - Rules.STAMINA_REGEN_PER_TURN;
            if (best_STA < test_STA) continue;
            if (m_Actor.WillTireAfter(test_STA)) continue;
            // this assumes run-steps cannot be interrupted (true 2022-05-19)
            if (!dest.IsWalkableFor(m_Actor)) continue;
            var route = new List<Engine.Op.MoveStep>{move, next_move};
            if (best_STA > test_STA) {
              best_STA = test_STA;
              routes.Clear();
            }
            routes.Add(new(route, act));
          }
        }

        return 0<routes.Count ? routes : null;
    }

    private static int CannotMeleeForThisLong(Actor a) {
        if (Actor.STAMINA_MIN_FOR_ACTIVITY-4 < a.StaminaPoints) return 0;
        return (a.StaminaPoints - (Actor.STAMINA_MIN_FOR_ACTIVITY - 4))/4;
    }

    // sunk from BaseAI
    protected ActorAction? BehaviorFightOrFlee(RogueGame game, ActorCourage courage, string[] emotes, RouteFinder.SpecialActions allowedChargeActions)
    {
      if (_blast_field?.Contains(m_Actor.Location.Position) ?? false) {
        // oops.  Panic.  Wasn't able to flee explosives and will likely die, either immediately or by sudden vulnerability.
        return new ActionWait(m_Actor);
      }

      // it is possible to reach here with a ranged weapon.  (Char Office assault, for instance)  In this case, we don't have Line of Fire.
      if (null != GetBestRangedWeaponWithAmmo()) return null;

      List<Point> legal_steps = _legal_steps; // XXX working reference due to following postprocessing
      if (null != _blast_field && null != legal_steps) {
        IEnumerable<Point> test = legal_steps.Where(pt => !_blast_field.Contains(pt));
        legal_steps = (test.Any() ? test.ToList() : null);
      }

      // this needs a serious rethinking; dashing into an ally's line of fire is immersion-breaking.
      var target = FilterNearest(_enemies);  // may not be enemies[0] due to this using StdDistance rather than GridDistance
      Actor enemy = target.Percepted;
      Location e_loc = enemy.Location;

      bool doRun = false;	// only matters when fleeing
      bool decideToFlee = (null != legal_steps);
      if (decideToFlee) {
        if (enemy.HasEquipedRangedWeapon()) decideToFlee = false;
        else if (m_Actor.Model.Abilities.IsLawEnforcer && enemy.MurdersOnRecord(m_Actor) > 0)
          decideToFlee = false;
        else if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, in e_loc))
          decideToFlee = true;
        else if (m_Actor.Leader != null && ActorCourage.COURAGEOUS == courage) {
	      decideToFlee = false;
        } else if (ActorCourage.COWARD == courage) {
          decideToFlee = true;
          doRun = true;
        } else {
          decideToFlee = WantToEvadeMelee(m_Actor, courage, enemy);
          doRun = !HasSpeedAdvantage(m_Actor, enemy);
        }
        if (!decideToFlee && WillTireAfterAttack(m_Actor)) {
          if (   (null != _damage_field && _damage_field.ContainsKey(m_Actor.Location.Position))
              || Rules.IsAdjacent(m_Actor.Location, in e_loc))
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
           if (!CanReachSimple(in e_loc, allowedChargeActions)) {
             if (null == (enemy.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo()) return null;  // no ranged weapon, unreachable: harmless?
             decideToFlee = true;   // get out of here, now
           }
//      }
      }

      var LoF_reserve = AlliesNeedLoFvs(enemy);
      ActorAction? tmpAction = null;
      if (decideToFlee) {
        tmpAction = BehaviorFlee(enemy, LoF_reserve, doRun, emotes);
        if (null != tmpAction) return tmpAction;
      }

      var approachable_enemies = _enemies.FindAll(p => Rules.IsAdjacent(m_Actor.Location, p.Location));

      if (0 >= approachable_enemies.Count) {
        if (null != legal_steps) {
          // nearest enemy is not adjacent.  Filter by whether it's legal to approach.
          approachable_enemies = _enemies.FindAll(p => {
            int dist = Rules.GridDistance(m_Actor.Location,p.Location);
            return legal_steps.Any(pt => dist>Rules.GridDistance(new Location(m_Actor.Location.Map,pt),p.Location));
          });
          if (0 >= approachable_enemies.Count) approachable_enemies = null;
        }
      }

      // if enemy is not approachable then following checks are invalid
      if (!(approachable_enemies?.Contains(target) ?? false)) return new ActionWait(m_Actor);

      // redo the pause check
      var next_to = 2 == Rules.GridDistance(m_Actor.Location, in e_loc) ? _closeIn(e_loc) : null;
      if (null != next_to) {
        if (1 < next_to.Count && null != LoF_reserve) {
          List<Engine._Action.MoveStep> stage = new(next_to.Count);
          foreach (var move in next_to) {
            if (!LoF_reserve.Contains(move.dest.Position)) stage.Add(move);
          }
          if (0<stage.Count && stage.Count < next_to.Count) next_to = stage;
        }

        var no_melee_for = CannotMeleeForThisLong(enemy);
        if (2 <= no_melee_for) {
          var n = Rules.Get.DiceRoller.Roll(0, next_to.Count);
          var act = next_to[n];
          if (act.is_running) {
            Engine.Goal.NextAction goal = new(m_Actor, new ActionMeleeAttack(m_Actor, enemy));
            SetObjective(goal);
          }
          return act;
        };
        if (m_Actor.Speed > enemy.Speed && !enemy.CanRun()) {
          List<Engine._Action.MoveStep> stage = new(next_to.Count);
          foreach (var move in next_to) {
            if (move.is_running) stage.Add(move);
          }
          if (0<stage.Count) {
            if (stage.Count < next_to.Count) next_to = stage;

            var n = Rules.Get.DiceRoller.Roll(0, next_to.Count);
            var act = next_to[n];
            Engine.Goal.NextAction goal = new(m_Actor, new ActionMeleeAttack(m_Actor, enemy));
            SetObjective(goal);
            return act;
          } else
            return new ActionWait(m_Actor);
        }
        // otherwise, check for dash-attack
        {
          List<Engine._Action.MoveStep> stage = new(next_to.Count);
          foreach (var move in next_to) {
            if (move.is_running) stage.Add(move);
          }
          if (0<stage.Count) {
            if (stage.Count < next_to.Count) next_to = stage;

            var n = Rules.Get.DiceRoller.Roll(0, next_to.Count);
            var act = next_to[n];
            Engine.Goal.NextAction goal = new(m_Actor, new ActionMeleeAttack(m_Actor, enemy));
            SetObjective(goal);
            return act;
          }
        }
      }

      if (3 == Rules.GridDistance(m_Actor.Location, in e_loc) && m_Actor.RunIsFreeMove && enemy.CanRun()) {
        var options = _chargeTowards(e_loc);
        if (null != options) {
          var n = Rules.Get.DiceRoller.Roll(0,options.Count);
          Engine.Goal.NextAction goal = new(m_Actor, options[n].Key[1]);
          SetObjective(goal);
          return options[n].Value;
        }
      }

      // charge
      tmpAction = BehaviorChargeEnemy(target, true, true);
      if (null != tmpAction) {
        if (Rules.Get.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[2], enemy.Name), true);
        return tmpAction;
      }
      return null;
    }

#nullable enable
    public ActorAction? WouldUseAccessibleStack(in Location dest,bool is_real=false) {
      var stacks = Map.GetAccessibleInventorySources(dest);
      if (null != stacks) {
        ActorAction? act;
        foreach(var stack in stacks) {
          act = WouldGrabFromAccessibleStack(in stack, is_real);
          if (null != act) return act;
        }
      }
      return null;
    }

    public ActorAction? BehaviorUseAdjacentStack() { return WouldUseAccessibleStack(m_Actor.Location, true); }
#nullable restore
	protected ActorAction BehaviorPathTo(in Location dest) { return BehaviorPathTo(new HashSet<Location> { dest }); }

	protected ActorAction BehaviorPathToAdjacent(Location dest)
	{
      var seen = new HashSet<Location> { dest };
      var final_range = new Dictionary<Location,ActorAction>();

      var range = m_Actor.OnePathRange(in dest);
      if (null == range) return null;

      void reprocess_range(Dictionary<Location,ActorAction> src) {
        var reject = new HashSet<Location>();
        foreach(var x in src) {
          seen.Add(x.Key);
          var test = x.Value as ActorDest;
          if (null == test && !(x.Value is ActionOpenDoor)) continue;
          if (!VetoAction(x.Value)) {
            final_range.Add(x.Key,x.Value);
            continue;
          }
          reject.Add(x.Key);
        }
        foreach(var loc in reject) {
          var failover = m_Actor.OnePathRange(in loc);
          if (null == failover) continue;
          failover.OnlyIf(loc2 => !seen.Contains(loc2));
          if (0 >= failover.Count) continue;
          reprocess_range(failover);
        }
      }

      reprocess_range(range);
      if (0 >= final_range.Count) return null;
      range = final_range;

      var adjacent = m_Actor.OnePathRange(m_Actor.Location);
      if (null == adjacent) return null;
      adjacent.OnlyIf((Predicate<ActorAction>)(action => (action is ActorDest || action is ActionOpenDoor) && action.IsPerformable() && !VetoAction(action)));  // only allow actions that prefigure moving to destination quickly
      if (0 >= adjacent.Count) return null;
      adjacent.OnlyIf(loc => range.ContainsKey(loc));
      if (0 < adjacent.Count) return DecideMove(adjacent.CloneOnlyMinimal(act => Map.PathfinderMoveCosts(act) + Map.TrapMoveCostFor(act, m_Actor)));

#if OBSOLETE
      var init_costs = new Dictionary<Location,int>();
      foreach(var x in range) init_costs[x.Key] = 0;
      return BehaviorPathTo(PathfinderFor(init_costs,new HashSet<Map>()));
#endif
      return BehaviorPathTo(new HashSet<Location>(range.Keys));
	}

    protected ActorAction BehaviorHangAroundActor(Actor other, int minDist, int maxDist)
    {
      if (other?.IsDead ?? true) return null;

      var clan = m_Actor.ChainOfCommand;

      // Historically, this has been a repeated hot-spot for period-2 move loops, and also has had other unwanted behaviors.
#region 1) if we're out of range, get within range
      if (maxDist < Rules.GridDistance(m_Actor.Location,other.Location)) {
        int span = 2 * maxDist + 1;
        var rect = new Rectangle(other.Location.Position - (Point)maxDist, (Point)span);
        var goals = new HashSet<Location>();
        rect.DoForEach(pt => {
            var loc2 = new Location(other.Location.Map, pt);
            if (!Map.Canonical(ref loc2)) return;
            if (clan.Any(a => loc2 == a.Location)) return;
            if (minDist > Rules.GridDistance(in loc2, other.Location)) return; // no-op if minDist is 1
            if (!loc2.IsWalkableFor(m_Actor)) return;
            goals.Add(loc2);
        });
        if (0 < goals.Count) {
       	  var act = BehaviorPathTo(goals);
          if (null == act || !act.IsPerformable()) return null; // direct-returned from SelectAction only
          if (act is ActionMoveStep step) m_Actor.IsRunning = RunIfAdvisable(step.dest);
          return act;
        }
      }
#endregion

      // \todo replace this with some form of formation management
      var rules = Rules.Get;
      var range = new Rectangle((Point)minDist, (Point)(maxDist - minDist));

      Point otherPosition = other.Location.Position;
      int num = 0;
      Location loc;
      do {
        if (100 < ++num) return null;
        Point p = otherPosition + rules.DiceRoller.Choose(range) - rules.DiceRoller.Choose(range);
        loc = new Location(other.Location.Map,p);
        if (!Map.Canonical(ref loc)) continue;
        if (loc == m_Actor.Location) return new ActionWait(m_Actor);
        if (clan.Any(a => loc == a.Location)) continue;
      }
      while(!loc.IsWalkableFor(m_Actor) || Rules.GridDistance(in loc,other.Location) < minDist);
      _last_move = null;    // very random if we reach this point, expected to false-positive

	  ActorAction actorAction = BehaviorPathTo(in loc);
      if (!actorAction?.IsPerformable() ?? true) return null;   // direct-returned from SelectAction only
      if (actorAction is ActionMoveStep tmp) {
        if (Rules.GridDistance(m_Actor.Location, tmp.dest) > maxDist)   // no-op by construction; retaining this as placeholder for run logic refinement
           m_Actor.IsRunning = RunIfAdvisable(tmp.dest);
	  }
      return actorAction;
    }

    private bool InProximity(in Location src, in Location dest, int maxDist)
    {  // due to definitions, for maxDist=1 the other two tests are implied by the GridDistance test for m_Actor.Location
       return Rules.GridDistance(in src, in dest) <= maxDist
           && (1 >= maxDist || (CanSee(in dest) && null != m_Actor.MinStepPathTo(in src, in dest)));
    }

    protected override ActorAction BehaviorFollowActor(Actor other, int maxDist)
    {
      if (other?.IsDead ?? true) return null;
      if (InProximity(m_Actor.Location, other.Location, maxDist)) {
          RecordCloseToActor(other, maxDist);
          return new ActionWait(m_Actor);
      }
	  ActorAction actorAction = BehaviorPathToAdjacent(other.Location); // can null out from traffic jam
      if (!actorAction?.IsPerformable() ?? true) return null;
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
      // if either ActorUnsuspicousChance exceeds 100%, or ActorSpotMurdererChance is 0% or less, report
      // the murders counter value as zero.  (This should not immunize against actually witnessing the murder, or murder in progress.)
      // this needs to affect the popup as well.  A murderer should not do an action that enables detection.
      // \todo release block; next savegame; A murderer should not do an action that enables detection.
      foreach(var x in friends) {
        if (0 >= x.Value.MurdersOnRecord(m_Actor)) continue;
        (murderers ??= new()).Add(x.Key, x.Value);
      }
      if (null == murderers) return null;
      var rules = Rules.Get;
      if (!rules.RollChance(LAW_ENFORCE_CHANCE)) return null;  // \todo but should be 100% for hungry civilians attacking for food, that is in-progress
      friends = null;  // enable auto GC
      var game = RogueGame.Game;
      foreach(var x in murderers) {
        if (0 >= x.Value.MurdersInProgress && x.Value.IsUnsuspiciousFor(m_Actor)) game.DoEmote(x.Value, string.Format("moves unnoticed by {0}.", m_Actor.Name));
        else (friends ?? new()).Add(x.Key, x.Value);
      }
      if (null == friends) return null;
      // at this point, entries in friends are murderers that have elicited suspicion
      foreach(var suspect in friends.Values) {
        game.DoEmote(m_Actor, string.Format("takes a closer look at {0}.", suspect.Name));
        if (0 >= suspect.MurdersInProgress && !rules.RollChance(suspect.MurdererSpottedByChance(m_Actor))) continue;
        // XXX \todo V.0.10.0 this needs a rethinking (a well-armed murderer may be of more use killing z, a weak one should be assassinated)
        game.DoMakeAggression(m_Actor, suspect);
        m_Actor.TargetActor = suspect;
        // players are special: they get to react to this first
        return new ActionSay(m_Actor, suspect, string.Format("HEY! YOU ARE WANTED FOR {0}!", "murder".QtyDesc(suspect.MurdersOnRecord(m_Actor)).ToUpper()), (suspect.IsPlayer ? RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER : RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER | RogueGame.Sayflags.IS_FREE_ACTION));
      }
      return null;
    }

    protected ActorAction BehaviorBuildLargeFortification(int startLineChance)
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < m_Actor.BarricadingMaterialNeedForFortification(true)) return null;
      Map map = m_Actor.Location.Map;
      var choiceEval = Choose(Direction.COMPASS, dir =>
      {
        Point pt = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(pt) || !map.IsWalkable(pt) || map.IsOnMapBorder(pt) || map.HasActorAt(in pt) || map.HasExitAt(in pt) || map.IsInsideAt(pt))
          return false;
        var inv = map.GetItemsAt(pt);
        if (null != inv && !inv.IsEmpty && inv.Items.Any(it => !it.IsUseless)) return false;   // this should be more intentional
        int num1 = map.CountAdjacentTo(pt, ptAdj => !map.GetTileModelAt(ptAdj).IsWalkable); // allows IsInBounds above
        int num2 = map.CountAdjacent<Fortification>(pt, fortification => !fortification.IsTransparent);
        return (num1 == 3 && num2 == 0 && Rules.Get.RollChance(startLineChance)) || (num1 == 0 && num2 == 1);
      }, dir => Rules.Get.Roll(0, 666), (a, b) => a > b);
      if (choiceEval == null) return null;
      Point pt1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!m_Actor.CanBuildFortification(in pt1, true)) return null;
      return new ActionBuildFortification(m_Actor, in pt1, true);
    }

    protected static bool IsDoorwayOrCorridor(Map map, in Point pos)
    { // all of these can use IsInBounds
      if (!map.GetTileModelAt(pos).IsWalkable) return false;
      Point pt = pos + Direction.NE;
      bool flag_ne = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      pt = pos + Direction.NW;
      bool flag_nw = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      pt = pos + Direction.SE;
      bool flag_se = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      pt = pos + Direction.SW;
      bool flag_sw = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      bool no_corner = !flag_ne && !flag_se && !flag_nw && !flag_sw;
      if (!no_corner) return false;

      pt = pos + Direction.N;
      bool flag_n = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      pt = pos + Direction.S;
      bool flag_s = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      pt = pos + Direction.E;
      bool flag_e = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      pt = pos + Direction.W;
      bool flag_w = map.IsInBounds(pt) && !map.GetTileModelAt(pt).IsWalkable;
      return (flag_n && flag_s && !flag_e && !flag_w) || (flag_e && flag_w && !flag_n && !flag_s);
    }

    protected ActorAction BehaviorBuildSmallFortification()
    {
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) return null;
      if (m_Actor.CountItems<ItemBarricadeMaterial>() < m_Actor.BarricadingMaterialNeedForFortification(false)) return null;
      Map map = m_Actor.Location.Map;
      var choiceEval = Choose(Direction.COMPASS, dir =>
      {
        Point pt = m_Actor.Location.Position + dir;
        if (!map.IsInBounds(pt) || !map.IsWalkable(pt) || map.IsOnMapBorder(pt) || map.HasActorAt(in pt) || map.HasExitAt(in pt))
          return false;
        return IsDoorwayOrCorridor(map, in pt); // this allows using IsInBounds rather than IsValid
      }, dir => Rules.Get.Roll(0, 666), (a, b) => a > b);
      if (choiceEval == null) return null;
      Point pt1 = m_Actor.Location.Position + choiceEval.Choice;
      if (!m_Actor.CanBuildFortification(in pt1, false)) return null;
      return new ActionBuildFortification(m_Actor, in pt1, false);
    }

    /// <returns>0 for disallowed, 1 for allowed, 2+ for "better".</returns>
    protected int SleepLocationRating(Location loc)
    {
      // the legacy tests
      Map.Canonical(ref loc);
      var obj = loc.MapObject;
      if (obj is DoorWindow) return -1;  // contextual; need to be aware of doors
      if (loc.Map.AnyAdjacentExt<DoorWindow>(loc.Position)) return 0;
      if (loc.Map.HasExitAt(loc.Position)) return 0;    // both unsafe, and problematic for pathing in general
      var g_inv = loc.Items;
      if (null != g_inv) foreach(var it in g_inv.Items) { // sleeping on useful items is bad for others' pathing
        if (it.IsUseless) continue;
        if (it is ItemTrap trap) { // cf. ObjectiveAI::ItemIsUseless
          if (!trap.IsActivated) return 0;
          if (trap.IsSafeFor(m_Actor)) continue;
          if (0 < trap.Model.Damage) return 0;
        }
        return 0;
      }
      if (m_Actor.Location!=loc && loc.StrictHasActorAt) return 0;  // contextual
      if (obj?.IsCouch ?? false) return 1;  // jail cells are ok even though their geometry is bad

      bool wall_at(Point pt) {
        return !loc.Map.GetTileModelAtExt(pt)?.IsWalkable ?? true;
      } // invalid is impassable so acts like a wall

      // geometric code (walls, etc)
      if (!loc.Map.IsInsideAt(loc.Position)) return 0;
      if (!loc.TileModel.IsWalkable) return 0;
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
        int dist = Rules.GridDistance(m_Actor.Location.Position, in pt);
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
      if (it is BatteryPowered) it.UnequippedBy(m_Actor);
      return new ActionSleep(m_Actor);
    }

    private ActorAction BehaviorNavigateToSleep(Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> item_memory)
    {
#if DEBUG
        if (null == item_memory) throw new ArgumentNullException(nameof(item_memory));
#endif
        if (null == _legal_path) return null;
        var med_slp = item_memory.WhereIs(GameItems.IDs.MEDICINE_PILLS_SLP);    // \todo precalculate sleep-relevant medicines at game start
        bool known_bed(Location loc) {  // XXX depending on incoming events this may not be conservative enough
            if (null!=med_slp && med_slp.ContainsKey(loc)) return true;
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
      // \todo precalculate sleep-relevant medicines at game start
      // use SLP-relevant medicines from inventory (2019-07-29: caught by BehaviorUseMedecine, but we want to be more precise)
      var med_slp = m_Actor?.Inventory.GetBestDestackable(GameItems.PILLS_SLP);
      if (null != med_slp) return new ActionUseItem(m_Actor, med_slp);

      // taking SLP-relevant medicines from accessible stacks should be intercepted by general adjacent-stack handling

      // go to SLP-relevant medicines in inventory stacks that are in sight
      static bool has_SLP_relevant(Inventory inv) {
        return null!=inv.GetBestDestackable(GameItems.PILLS_SLP);
      }

      var tmpAction = BehaviorFindStack(has_SLP_relevant);
      if (null != tmpAction) return tmpAction;

      // try to resolve sleep-disruptive sanity without pathing
      if (3<=WantRestoreSAN) {  // intrinsic item rating code for sanity restore is need or higher (possible CPU hit from double-checking for want later)
        tmpAction = BehaviorUseEntertainment();
        if (null != tmpAction)  return tmpAction;
      }

      var item_memory = m_Actor.Controller.ItemMemory;
      if (!m_Actor.IsInside) {
        if (null != item_memory) return BehaviorNavigateToSleep(item_memory);
        // XXX this is stymied by closed, opaque doors which logically have inside squares near them; also ex-doorways
        // ignore barricaded doors on residences (they have lots of doors).  Do not respect those in shops, subways, or (vintage) the sewer maintenance.
        // \todo replace by more reasonable foreach loop
        var see_inside = FOVloc.Where(loc => loc.Map.IsInsideAt(loc.Position) && loc.IsWalkableFor(m_Actor));
        return BehaviorHeadFor(see_inside, false, false);
      }

      if (null != item_memory) {
        // reject if the smallest zone containing this location does not have a bed
        // problems with: CHAR Underground base (bypassed), subways (doors aren't walkable but they're inside)
        if (District.IsSubwayMap(m_Actor.Location.Map)) return BehaviorNavigateToSleep(item_memory);

        Rectangle scan_this = m_Actor.Location.Map.Rect;
        var z_list = m_Actor.Location.Map.GetZonesAt(m_Actor.Location.Position);    // non-null check in map generation
        // no zone? must not be acceptable
        if (null == z_list) return BehaviorNavigateToSleep(item_memory);
        foreach(var z in z_list) {
          if (scan_this.Width < z.Bounds.Width) continue;
          if (scan_this.Height < z.Bounds.Height) continue;
          if (scan_this.Width > z.Bounds.Width || scan_this.Height > z.Bounds.Height) scan_this = z.Bounds;
        }

        if (!scan_this.Any(pt => {
            Location loc = new Location(m_Actor.Location.Map, pt);
            if (!loc.Map.IsInsideAt(loc.Position)) return false;
            // be as buggy as the display, which shows objects in their current positions
            if (!(loc.MapObject?.IsCouch ?? false)) return false;
            return !(loc.Actor?.IsSleeping ?? false);   // cheat: bed should not have someone already sleeping in it
        })) return BehaviorNavigateToSleep(item_memory);
      }

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
          foreach(var pt in in_bounds.ToArray()) {
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

    protected ActorAction? BehaviorRestIfTired()
    {
      if (m_Actor.StaminaPoints >= Actor.STAMINA_MIN_FOR_ACTIVITY) return null;
      // ok to walk to a lower damage field
      if (null != _damage_field && _damage_field.ContainsKey(m_Actor.Location.Position)) {
        var steps = Engine.Op.MoveStep.outbound(m_Actor.Location, m_Actor, loc => !_damage_field.ContainsKey(loc.Position), false);
        if (null != steps) {
            while(0<steps.Value.Key.Count) {
              var n = Rules.Get.DiceRoller.Roll(0, steps.Value.Key.Count);
              var act = steps.Value.Key[n].Bind(m_Actor);
              if (null != act) return act;
              steps.Value.Key.RemoveAt(n);
            }
        }
      }

      return new ActionWait(m_Actor);
    }

    protected override ActorAction BehaviorExplore(ExplorationData exploration)
    {
      ActorCourage courage = Directives.Courage;
      Direction prevDirection = Direction.FromVector(m_Actor.Location.Position - PrevLocation.Position);
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
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
        const int AVOID_TRAPS = -1000; // alpha10 greatly increase penalty and x by potential damage; was -50
        const int EXPLORE_INOUT = 50;
        const int EXPLORE_DIRECTION = 25;
        const int EXPLORE_RANDOM = 10;

//      if (exploration.HasExplored(loc)) return float.NaN;
        Map map = loc.Map;
        Point position = loc.Position;
        int trap_max_damage = m_Actor.Model.Abilities.IsIntelligent ? map.TrapsUnavoidableMaxDamageAtFor(position,m_Actor) : 0;
        if (m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous && trap_max_damage >= m_Actor.HitPoints)
          return float.NaN;
        int num = 0;
        if (!exploration.HasExplored(map.GetZonesAt(position))) num += EXPLORE_ZONES;
        if (!exploration.HasExplored(in loc)) num += EXPLORE_LOCS;
        var mapObjectAt = map.GetMapObjectAt(position);
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
        return (float) (num + Rules.Get.Roll(0, EXPLORE_RANDOM));
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
      {
      var it = inventory.GetFirst<ItemEntertainment>(en => m_Actor.CanUse(en));
      if (null != it) return new ActionUseItem(m_Actor, it);
      }
      {
      var it = inventory.GetFirst<ItemMedicine>(med => 0<med.SanityCure);   // VAPORWARE side-effecting medicines
      if (null != it) return new ActionUseItem(m_Actor, it);
      }
      // pathing to chat would be CPU-intensive so don't do that here.  If policy is changed, we'd want to allow disabling it when pathing to sleep, etc.
      // VAPORWARE chat with other needy souls in sight
      return null;
    }

#region stench killer
    // stench killer support -- don't want to lock down to the only user, CivilianAI
    // actually, this particular heuristic is *bad* because it causes the z to lose tracking too close to shelter.
    // with the new scent-suppressor mechaniics, the cutpoints are somewhat reasonable but extra distance/LoS breaking is needed
    private bool IsGoodStenchKillerSpot(Location loc)
    {
      // 2. Spray in a good position:
      //    2.1 entering or leaving a building.
      if (PrevLocation.Map.IsInsideAt(PrevLocation.Position) != loc.Map.IsInsideAt(loc.Position)) return true;
      //    2.3 an exit.
      if (loc.Map.HasExitAt(loc.Position)) return true;
      //    2.2 a door/window.
      return loc.MapObject is DoorWindow;
    }

    protected ItemSprayScent? GetEquippedStenchKiller()
    {
      return m_Actor.Inventory.GetFirst<ItemSprayScent>(spray => spray.IsEquipped && Odor.SUPPRESSOR == spray.Model.Odor);
    }

    protected ActionSprayOdorSuppressor? BehaviorUseStenchKiller()
    {
      // alpha 10 redefined spray suppression to work on the odor source, not the odor

      // RS Revived: identify the least-charged stench killer.  Does not have to be equipped.
      var spray = m_Actor.Inventory.GetBestMatching<ItemSprayScent>(it => !it.IsUseless && Odor.SUPPRESSOR == it.Model.Odor,
                    (lhs,rhs) => lhs.SprayQuantity>rhs.SprayQuantity);
      if (null == spray) return null;

      const int USE_STENCH_KILLER_CHANCE = 75;
      if (Rules.Get.RollChance(USE_STENCH_KILLER_CHANCE)) {  // compromise between burning RNG and very high false negative rate
      // first check if want to use it on self, then check on adj leader/follower
      Actor sprayOn = null;

      bool WantsToSprayOn(Actor a)
      {
        // never spray on player, could mess with his tactics   \todo good use of a directive, if that wasn't obsolete
        if (a.IsPlayer) return false;

        // only if self or adjacent
        if (a != m_Actor && !Rules.IsAdjacent(m_Actor.Location, a.Location)) return false;

        // dont spray if already suppressed for 2h or more
        if (a.OdorSuppressorCounter >= 2 * WorldTime.TURNS_PER_HOUR) return false;

        // RS Revived: If already suppressed, maintain it
        // \todo be more flexible with the threshold once we have proper backward planning
        if (0 < a.OdorSuppressorCounter) return true;

        // spot must be interesting to spray for either us or the target.
        if (IsGoodStenchKillerSpot(m_Actor.Location)) return true;
        if (IsGoodStenchKillerSpot(a.Location)) return true;
        return false;
      }

      // self?...
      if (WantsToSprayOn(m_Actor)) sprayOn = m_Actor;
      else {
        // ...adj leader/mates/followers
        var leader = m_Actor.LiveLeader;
        if (null != leader) {
          if (WantsToSprayOn(leader)) sprayOn = leader;
          else sprayOn = leader.FirstFollower(WantsToSprayOn);
        }

        if (null == sprayOn) sprayOn = m_Actor.FirstFollower(WantsToSprayOn);
      }

      //  spray?
      if (sprayOn != null) {
        spray.EquippedBy(m_Actor);
        var sprayIt = new ActionSprayOdorSuppressor(m_Actor, spray, sprayOn);
        if (sprayIt.IsPerformable()) return sprayIt;  // should be tautological given above
      }
      }

      GetEquippedStenchKiller()?.UnequippedBy(m_Actor);
      return null;  // nope.
    }
#endregion

#nullable enable
    protected ActorAction? BehaviorGoReviveCorpse(List<Percept>? percepts)
    {
	  if (!Session.Get.HasCorpses) return null;
      if (m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) == 0) return null;
      if (!m_Actor.HasItemOfModel(GameItems.MEDIKIT)) return null;
      var corpsePercepts = percepts?.FilterT<List<Corpse>>()?.Filter(p =>
      {
        foreach (Corpse corpse in p.Percepted as List<Corpse>) {
          if (m_Actor.CanRevive(corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return true;
        }
        return false;
      });
      if (null == corpsePercepts) return null;
      var percept = FilterNearest(corpsePercepts);
	  if (m_Actor.Location.Position==percept.Location.Position) {
        foreach (Corpse corpse in (percept.Percepted as List<Corpse>)) {
          if (m_Actor.CanRevive(corpse) && !m_Actor.IsEnemyOf(corpse.DeadGuy))
            return new ActionReviveCorpse(m_Actor, corpse);
        }
	  }
      return BehaviorHeadFor(percept.Location,false,false);
    }
#nullable restore

#region ground inventory stacks
    private List<Item>? InterestingItems(IEnumerable<Item> Items)
    {
#if DEBUG
      if (null == Items) throw new ArgumentNullException(nameof(Items));
#endif
      var exclude = new HashSet<GameItems.IDs>(Objectives.Where(o=>o is Goal_DoNotPickup).Select(o=>(o as Goal_DoNotPickup).Avoid));
#if REPAIR_DO_NOT_PICKUP
      exclude.ExceptWith(WhatDoINeedNow());
      exclude.ExceptWith(WhatDoIWantNow());
#endif
      var tmp = Items.Where(it => !exclude.Contains(it.Model.ID) && IsInterestingItem(it));
      if (!tmp.Any()) return null;
      var tmp2 = tmp.Where(it => it is UsableItem use && use.FreeSlotByUse(m_Actor));
      if (tmp2.Any()) return tmp2.ToList();
      tmp2 = tmp.Where(it => it is UsableItem use && use.UseBeforeDrop(m_Actor));
      if (tmp2.Any()) return tmp2.ToList();
      return tmp.ToList();
    }

    private List<Item>? InterestingItems(Inventory inv)
    {
      return InterestingItems(inv.Items);
    }

    private Item MostInterestingItemInStack(Inventory stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      var interesting = InterestingItems(stack);
      if (null==interesting) return null;

      Item obj = null;
      foreach (Item it in interesting) {
        if (null == obj || RHSMoreInteresting(obj, it)) obj = it;
      }
      return obj;
    }

#nullable enable
    protected override ActorAction? BehaviorWouldGrabFromStack(in Location loc, Inventory? stack)
    {
      return BehaviorGrabFromStack(in loc,stack,false);
    }

    public bool WouldGrabFromStack(in Location loc, Inventory? stack)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.StackIsBlocked(in loc)) return false;
      return WouldGrabFromAccessibleStack(in loc,stack)?.IsLegal() ?? false;
    }
#nullable restore

    private ActorAction? _takeThis(in InventorySource<Item> stack, Item obj, ActorAction recover, bool is_real)
    {
#if DEBUG
      if (null == stack.obj_owner && null == stack.loc) throw new InvalidOperationException("do not try to take from actor inventory");
#endif
        // XXX \todo this has to be able to upgrade to swap in some cases (e.g. if armor is better than best armor)
        if (obj is ItemBodyArmor armor) {
          var best_armor = GetEquippedBodyArmor();
          if (null != best_armor && armor.Rating > best_armor.Rating) {
            // we actually want to wear this (second test redundant now, but not once stockpiling goes in)
            return ActionTradeWith.Cast(in stack, m_Actor, best_armor, obj);
          }
        }
#if DEBUG
        if (is_real && !m_Actor.MayTakeFrom(in stack)) throw new InvalidOperationException(m_Actor.Name + " attempted telekinetic take");
#endif
        var tmp = new ActionTakeItem(m_Actor, in stack, obj);
        if (tmp.IsLegal()) return tmp; // in case this is the biker/trap pickup crash [cairo123]
        if (m_Actor.Inventory.IsFull && null != recover && recover.IsLegal()) {
          if (recover is ActorGive drop) {
            if (obj.Model.ID == drop.Give.Model.ID) return null;
            if (is_real) Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, drop.Give.Model.ID));
          }
          return new ActionChain(recover, tmp);
        }
        return null;
    }

    private ActorAction? _takeThis(in Location loc, Item obj, ActorAction recover, bool is_real)
    {
        // XXX \todo this has to be able to upgrade to swap in some cases (e.g. if armor is better than best armor)
        if (obj is ItemBodyArmor armor) {
          var best_armor = GetEquippedBodyArmor();
          if (null != best_armor && armor.Rating > best_armor.Rating) {
            // we actually want to wear this (second test redundant now, but not once stockpiling goes in)
            return ActionTradeWith.Cast(loc, m_Actor, best_armor, obj);
          }
        }
#if DEBUG
        if (is_real && !m_Actor.MayTakeFromStackAt(in loc)) throw new InvalidOperationException(m_Actor.Name + " attempted telekinetic take from " + loc + " at " + m_Actor.Location);
#endif
        var tmp = new ActionTakeItem(m_Actor, in loc, obj);
        if (tmp.IsLegal()) return tmp; // in case this is the biker/trap pickup crash [cairo123]
        if (m_Actor.Inventory.IsFull && null != recover && recover.IsLegal()) {
          if (recover is ActorGive drop) {
            if (obj.Model.ID == drop.Give.Model.ID) return null;
            if (is_real) Objectives.Add(new Goal_DoNotPickup(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, drop.Give.Model.ID));
          }
          return new ActionChain(recover, tmp);
        }
        return null;
    }

    public ActorAction? WouldGrabFromAccessibleStack(in InventorySource<Item> stack, bool is_real=false)
    {
#if DEBUG
      if (null == stack.obj_owner && null == stack.loc) throw new InvalidOperationException("do not try to grab from actor inventory");
#endif
      Item obj = MostInterestingItemInStack(stack.inv);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = cant_get && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, in stack) : null);
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
      var tmp = _takeThis(in stack, obj, recover, is_real);
      if (null == tmp) return null;
      if (is_real && Rules.Get.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        RogueGame.Game.DoEmote(m_Actor, string.Format("{0}! Great!", obj.AName));
      return tmp;
    }

    public ActorAction? WouldGrabFromAccessibleStack(in Location loc, Inventory stack, bool is_real=false)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.Location.Map != loc.Map) return null;
      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = cant_get && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, loc) : null);
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
      var tmp = _takeThis(in loc, obj, recover, is_real);
      if (null == tmp) return null;
      if (is_real && Rules.Get.RollChance(EMOTE_GRAB_ITEM_CHANCE))
        RogueGame.Game.DoEmote(m_Actor, string.Format("{0}! Great!", obj.AName));
      return tmp;
    }

#nullable enable
    public ActorAction? BehaviorGrabFromAccessibleStack(Location loc, Inventory stack)
    {
      return WouldGrabFromAccessibleStack(in loc, stack, true);
    }
#nullable restore

    protected ActorAction? BehaviorGrabFromStack(in Location loc, Inventory? stack, bool is_real = true)
    {
#if DEBUG
      if (stack?.IsEmpty ?? true) throw new ArgumentNullException(nameof(stack));
#endif
      if (m_Actor.StackIsBlocked(in loc) || m_Actor.Location.Map != loc.Map) return null;

      Item obj = MostInterestingItemInStack(stack);
      if (obj == null) return null;

      // but if we cannot take it, ignore anyway
      bool cant_get = !m_Actor.CanGet(obj);
      bool need_recover = !m_Actor.CanGet(obj) && m_Actor.Inventory.IsFull;
      ActorAction recover = (need_recover ? BehaviorMakeRoomFor(obj, loc) : null);
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
      bool may_take = is_real ? m_Actor.MayTakeFromStackAt(in loc) : true;

      if (may_take) {
        tmp = _takeThis(in loc, obj, recover, is_real);
        if (null == tmp) return null;
        if (is_real && Rules.Get.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueGame.Game.DoEmote(m_Actor, string.Format("{0}! Great!", obj.AName));
        return tmp;
      }
      { // scoping brace
      if (null == _legal_steps) return null;
      var tmpAction = BehaviorPathTo(in loc);
      if (null != tmpAction) return tmpAction;

#if OBSOLETE
      // following prone to stepping into traps
      int current_distance = Rules.GridDistance(m_Actor.Location, in loc);
      Location? denorm = m_Actor.Location.Map.Denormalize(in loc);
      var costs = new Dictionary<Point,int>();
      var vis_costs = new Dictionary<Point,int>();
      if (_legal_steps.Contains(denorm.Value.Position)) {
        Point pt = denorm.Value.Position;
        Location test = new Location(m_Actor.Location.Map,pt);
        costs[pt] = 1;
        // this particular heuristic breaks badly if it loses sight of its target
        if (LOS.ComputeFOVFor(m_Actor, in test).Contains(denorm.Value.Position)) vis_costs[pt] = 1;
      } else {
        foreach(Point pt in _legal_steps) {
          Location test = new Location(m_Actor.Location.Map,pt);
          int dist = Rules.GridDistance(in test, in loc);
          if (dist >= current_distance) continue;
          costs[pt] = dist;
          // this particular heuristic breaks badly if it loses sight of its target
          if (!LOS.ComputeFOVFor(m_Actor, in test).Contains(denorm.Value.Position)) continue;
          vis_costs[pt] = dist;
        }
        // above fails if a direct diagonal path is blocked.
        if (0 >= costs.Count) {
          foreach(Point pt in _legal_steps) {
            Location test = new Location(m_Actor.Location.Map,pt);
            int dist = Rules.GridDistance(in test, in loc);
            if (dist == current_distance) continue;
            costs[pt] = dist;
            // this particular heuristic breaks badly if it loses sight of its target
            if (!LOS.ComputeFOVFor(m_Actor, in test).Contains(denorm.Value.Position)) continue;
            vis_costs[pt] = dist;
          }
        }
      }

      tmpAction = DecideMove(vis_costs.Keys);
      if (null != tmpAction) {
#if DEBUG
        throw new InvalidOperationException("test case?");
#endif
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
        m_Actor.Activity = Activity.IDLE;
        if (is_real && Rules.Get.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
        return tmpAction;
      }
      tmpAction = DecideMove(costs.Keys);
      if (null != tmpAction) {
#if DEBUG
        throw new InvalidOperationException("test case?");
#endif
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
        m_Actor.Activity = Activity.IDLE;
        if (is_real && Rules.Get.RollChance(EMOTE_GRAB_ITEM_CHANCE))
          RogueForm.Game.DoEmote(m_Actor, string.Format("{0}! Great!", (object) obj.AName));
        return tmpAction;
      }
#endif
      } // end scoping brace
      return null;
    }

    protected ActorAction BehaviorHeadForBestStack(Dictionary<Location,Inventory> stacks)
    {
#if DEBUG
        if (null == stacks) throw new ArgumentNullException(nameof(stacks));
#endif
        ActorAction tmpAction = null;
          {
          var get_item = new Dictionary<Location, ActorAction>();
          foreach(var x in stacks) {
            if (!m_Actor.MayTakeFromStackAt(x.Key)) continue;
            tmpAction = BehaviorGrabFromAccessibleStack(x.Key, x.Value);
            if (tmpAction?.IsPerformable() ?? false) get_item[x.Key] = tmpAction;
          }
          if (1<get_item.Count) {
            var considering = new List<Location>(get_item.Count);
            var dominated = new List<Location>(get_item.Count);
            foreach(var x in get_item) {
              if (0 >= considering.Count) {
                considering.Add(x.Key);
                continue;
              }
              int item_compare = 0;   // new item.CompareTo(any old item) i.e. new item <=> any old item
              // relying on more specific cases blocking less-specific cases in C# here
              // ActionTakeItem does implement ActorTake; we want ActorTake to handle the various trade-with-inventory classes
              switch(x.Value) {
              case ActionTakeItem new_take:
                item_compare = 1;
                foreach(var old_loc in considering) {
                  switch(get_item[old_loc]) {
                  case ActionTakeItem old_take:
                     if (new_take.Take.Model.ID==old_take.Take.Model.ID) { // \todo take from "endangered stack" if quantity-sensitive, otherwise not-endangered stack
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(new_take.Take,old_take.Take)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_take.Take, new_take.Take)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case ActorTake old_trade:
                     if (RHSMoreInteresting(new_take.Take,old_trade.Take)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_trade.Take, new_take.Take)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case Use<Item> old_use:
                    // generally better to take than use
                    if (old_use.Use.Model.ID!=new_take.Take.Model.ID) dominated.Add(old_loc);
                    else item_compare = 0;
                    break;
                  }
                  if (-1==item_compare) break;
                }
                break;
              case ActorTake new_trade:
                item_compare = 1;
                foreach(var old_loc in considering) {
                  switch(get_item[old_loc]) {
                  case ActionTakeItem old_take:
                     if (RHSMoreInteresting(new_trade.Take,old_take.Take)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_take.Take, new_trade.Take)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case ActorTake old_trade:
                     if (new_trade.Take.Model.ID == old_trade.Take.Model.ID) { // \todo take from "endangered stack" if quantity-sensitive, otherwise not-endangered stack
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(new_trade.Take, old_trade.Take)) {
                       item_compare = -1;
                       break;
                     }
                     if (RHSMoreInteresting(old_trade.Take, new_trade.Take)) dominated.Add(old_loc);
                     else item_compare = 0;
                    break;
                  case Use<Item> old_use:
                    // generally better to take than use
                    if (old_use.Use.Model.ID!= new_trade.Take.Model.ID) dominated.Add(old_loc);
                    else item_compare = 0;
                    break;
                  }
                  if (-1==item_compare) break;
                }
                break;
              case Use<Item> new_use:
                item_compare = 0;   // new item.CompareTo(any old item) i.e. new item <=> any old item
                foreach(var old_loc in considering) {
                  switch(get_item[old_loc]) {
                    case Use<Item> old_use:
                      if (old_use.Use.Model.ID==new_use.Use.Model.ID) { // duplicate
                        item_compare = -1;
                        break;
                      }
                      break;
                    case ActionTakeItem old_take:
                      if (old_take.Take.Model.ID!=new_use.Use.Model.ID) { // generally better to take than use
                        item_compare = -1;
                        break;
                      }
                      break;
                  }
                  if (-1==item_compare) break;
                }
                break;
              }
              // respond to item comparison
              if (1 == item_compare) {
                considering.Clear();
                dominated.Clear();
              } else if (0 < dominated.Count) {
                foreach(var reject in dominated) considering.Remove(reject);
                dominated.Clear();
              }
              if (-1 == item_compare) continue;
              considering.Add(x.Key);
            }
            get_item.OnlyIf(loc => considering.Contains(loc));
          }
#if FALSE_POSITIVE
          if (/* m_Actor.IsDebuggingTarget && */ 1<get_item.Count && !m_Actor.Inventory.IsEmpty) throw new InvalidOperationException(m_Actor.Name+", stack choosing: "+get_item.to_s());
#endif
          if (0<get_item.Count) {
            var take = get_item.FirstOrDefault();
            m_Actor.Activity = Activity.IDLE;
            return take.Value;
          }
          }

          // no accessible interesting stacks.  Memorize them just in case.
          {
          var track_inv = Goal<Goal_PathToStack>();
          foreach(var x in stacks) {
            if (null == track_inv) {
              track_inv = new Goal_PathToStack(m_Actor.Location.Map.LocalTime.TurnCounter,m_Actor,x.Key);
              Objectives.Add(track_inv);
            } else track_inv.newStack(x.Key);
          }
          }

          var percept = FilterNearest(stacks);
          while(null != percept.Value) {
            m_LastItemsSaw = new Percept(percept.Value,m_Actor.Location.Map.LocalTime.TurnCounter,percept.Key);
            tmpAction = BehaviorGrabFromStack(percept.Key, percept.Value);
            if (tmpAction?.IsPerformable() ?? false) {
#if TRACE_SELECTACTION
              if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "taking from stack");
#endif
              m_Actor.Activity = Activity.IDLE;
              return tmpAction;
            }
            // XXX the main valid way this could fail, is a stack behind a non-walkable, etc., object that isn't a container
            // could happen in normal play in the sewers
            // under is handled within the Behavior functions
#if TRACE_SELECTACTION
            Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+" has abandoned getting the items at "+ percept.Key);
#endif
            stacks.Remove(percept.Key);
            percept = FilterNearest(stacks);
          }
        return null;
    }

#nullable enable
    private ActorAction? BehaviorFindStack(Predicate<Inventory> want_now) {
        var stacks = GetInterestingInventoryStacks(want_now);
        if (null != stacks) return BehaviorHeadForBestStack(stacks);
        return null;
    }
#nullable restore

    private ActorAction BehaviorRequestCriticalFromGroup()
    {
        var clan = m_Actor.ChainOfCommand;
        if (null == clan) return null;
        var critical = WhatDoINeedNow();
        if (0 >= critical.Count) return null;
        critical.RemoveWhere(it => {
          if (GameItems.ammo.Contains(it)) {
            var rw = m_Actor.Inventory.GetCompatibleRangedWeapon(it);
            if (null == rw || rw.Ammo == rw.Model.MaxAmmo) return true;  // not needed
          }
          return false;
        });
        if (0 >= critical.Count) return null;
        var insurance = new Dictionary<Actor, GameItems.IDs>();   // trading CPU for lower GC might be empirically ok here (null with alloc on first use)
        var want = new Dictionary<Actor, GameItems.IDs>();
        foreach (var a in clan) {
            if (!CanSee(a.Location)) continue;  // don't want to mention this sort of thing over radio
            if (!InCommunicationWith(a)) continue;
            if (a.IsPlayer) continue; // \todo self-order for this
            var ai = a.Controller as OrderableAI;
            var have = ai.NonCriticalInInventory();
            if (null != have.Key) {
                foreach (var x in have.Key) {
                    if (!critical.Contains(x)) continue;
                    if (GameItems.ammo.Contains(x)) {
                      var rw2 = a.Inventory.GetCompatibleRangedWeapon(x);
                      if (null != rw2 && rw2.Ammo < rw2.Model.MaxAmmo) continue;    // allow defensive behavior
                    }
#if REDUNDANT
                    if (GameItems.restoreSAN.Contains(x)) {
                      if (3 <= ai.WantRestoreSAN && null!=ai.BehaviorUseEntertainment()) continue;  // allow defensive behavior
                    }
#endif
                    insurance[a] = x;
                    break;
                }
                if (insurance.ContainsKey(a)) continue; // possible MSIL compaction here (local bool vs. member function call); may not be a real micro-optimization
            }
            if (null != have.Value) {
                foreach (var x in have.Value) {
                    if (!critical.Contains(x)) continue;
                    if (GameItems.ammo.Contains(x)) {
                      var rw2 = a.Inventory.GetCompatibleRangedWeapon(x);
                      if (null != rw2 && rw2.Ammo < rw2.Model.MaxAmmo) continue;    // allow defensive behavior
                    }
#if REDUNDANT
                    if (GameItems.restoreSAN.Contains(x)) {
                      if (3 <= ai.WantRestoreSAN && null!=ai.BehaviorUseEntertainment()) continue;  // allow defensive behavior
                    }
#endif
                    want[a] = x;
                    break;
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
                if ((x.Key.Controller as ObjectiveAI).IsFocused) continue; // stacking this can backfire badly
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
                    (donor.Controller as OrderableAI).SetObjective(your_plan);
                    SetObjective(my_plan);
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
                if ((x.Key.Controller as ObjectiveAI).IsFocused) continue; // stacking this can backfire badly
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
                    (donor.Controller as OrderableAI).SetObjective(your_plan);
                    SetObjective(my_plan);
                    return tmpAction;
                }
                return null;
            }
        }
        return null;
    }

    private ActorAction BehaviorDefendFromRequestCriticalFromGroup()
    {
        var clan = m_Actor.ChainOfCommand;
        if (null == clan) return null;
        var have = NonCriticalInInventory();
        if (null == have.Key && null == have.Value) return null;
        List<GameItems.IDs> precious = null;
        foreach(var a in clan) {
          var critical = (a.Controller as ObjectiveAI)?.WhatDoINeedNow();   // yes, we also defend vs player leader
          if (null == critical || 0 >= critical.Count) continue;
          if (!CanSee(a.Location)) continue;  // don't want to mention this sort of thing over radio
          if (!InCommunicationWith(a)) continue;
          if (null != have.Key) {
            foreach (var x in have.Key) {
                if (GameItems.ammo.Contains(x)) {
                  var rw = a.Inventory.GetCompatibleRangedWeapon(x);
                  if (null == rw || rw.Ammo == rw.Model.MaxAmmo) continue;
                }
                if (critical.Contains(x)) (precious ??= new List<GameItems.IDs>()).Add(x);
            }
          }
          if (null != have.Value) {
            foreach (var x in have.Value) {
                if (GameItems.ammo.Contains(x)) {
                  var rw = a.Inventory.GetCompatibleRangedWeapon(x);
                  if (null == rw || rw.Ammo == rw.Model.MaxAmmo) continue;
                }
                if (critical.Contains(x)) (precious ??= new List<GameItems.IDs>()).Add(x);
            }
          }
          if (null != precious) foreach(var it in precious) {
            if (GameItems.ranged.Contains(it)) continue;    // handled at NonCriticalInInventory stage
            if (GameItems.food.Contains(it)) continue;    // ally is hungry...ok
            if (GameItems.ammo.Contains(it)) {  // reload ASAP
              var rw = m_Actor.Inventory.GetCompatibleRangedWeapon(it);
              if (null != rw && rw.Ammo < rw.Model.MaxAmmo) return UseAmmo(m_Actor.Inventory.GetCompatibleAmmoItem(rw), rw);
              continue;
            }
            // different medicines need different handling.  The immediate-use ones can be ceded immediately.
            if (GameItems.IDs.MEDICINE_MEDIKIT==it || GameItems.IDs.MEDICINE_BANDAGES==it || GameItems.IDs.MEDICINE_PILLS_ANTIVIRAL==it) continue;
            if (GameItems.restoreSAN.Contains(it)) {    // only have to defend if we ourselves are critical
#if REDUNDANT
              if (3 > WantRestoreSAN) continue; // only have to consider action loops
              var act = BehaviorUseEntertainment();
              if (null != act) return act;
#endif
              continue;
            }
#if DEBUG
            throw new InvalidOperationException("unhandled precious item: "+it+"; "+m_Actor.Name+" defending from "+a.Name);
#endif
          }
        }
        return null;
    }

    private ActorAction? BehaviorFindTrade()
    {
#if DEBUG
        if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException("must want to trade");   // \todo technically redundant now
#endif
        var percepts2 = GetTradingTargets(friends_in_FOV); // this should only return legal trading targets
        if (null == percepts2) return null;

        var near = FilterNearest(percepts2);

        // We are having CPU loading problems, so don't retest the legality of the trade
        if (Rules.IsAdjacent(m_Actor.Location, near.Key)) {
          MarkActorAsRecentTrade(near.Value);
          (near.Value.Controller as OrderableAI)?.MarkActorAsRecentTrade(m_Actor);   // try to reduce trading spam: one trade per pair, not two
          RogueGame.DoSay(m_Actor, near.Value, string.Format("Hey {0}, let's make a deal!", near.Value.Name), RogueGame.Sayflags.IS_FREE_ACTION);  // formerly paid AP cost here rather than in RogueGame::DoTrade
          return new ActionTrade(m_Actor, near.Value);
        }
        if (IsFocused) return null; // just in case
        var o_oai = (near.Value.Controller as OrderableAI)!;
        if (o_oai.IsFocused && !m_Actor.WillActAgainBefore(near.Value)) return null;
        var tmpAction = BehaviorIntelligentBumpToward(near.Key, false, false);
        if (null == tmpAction) return null;
        if (o_oai.IsFocused) return null;
        // alpha10 announce it to make it clear to the player whats happening but dont spend AP (free action)
        // might spam for a few turns, but its better than not understanding whats going on.
        RogueGame.DoSay(m_Actor, near.Value, string.Format("Hey {0}, let's make a deal!", near.Value.Name), RogueGame.Sayflags.IS_FREE_ACTION);
        m_Actor.TargetedActivity(Activity.FOLLOWING, near.Value);

        // install trading objectives -- hints to target where to go
        var my_trading = new Trade(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, near.Value);
        SetObjective(my_trading);
        var your_trading = o_oai.Goal<Trade>();
        if (null != your_trading) your_trading.Add(m_Actor);
        else {
          your_trading = new Trade(m_Actor.Location.Map.LocalTime.TurnCounter, near.Value, m_Actor);
          o_oai.SetObjective(your_trading);
        }
        return tmpAction;
    }

    protected ActorAction BehaviorTrading()
    {
#if DEBUG
        if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException("must want to trade");
#endif
        var tmpAction = BehaviorDefendFromRequestCriticalFromGroup();
        if (null != tmpAction) return tmpAction;
        tmpAction = BehaviorRequestCriticalFromGroup();
        if (null != tmpAction) return tmpAction;
        if (Directives.CanTrade) {
          tmpAction = BehaviorFindTrade();
          if (null != tmpAction) return tmpAction;
        }
        return null;
    }

    protected ActorAction BehaviorRangedInventory()
    {
      if (null != _enemies) return null;
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "checking for items to take");
#endif
      var tmp = BehaviorFindStack(TRUE);
      if (null != tmp) return tmp;
      tmp = Pathing<Goal_HintPathToActor>();    // leadership or trading requests
      if (null != tmp) return tmp;
      tmp = BehaviorTrading();

      foreach(var obj in Objectives) {
        if (obj is LatePathable path) {
          tmp = path.Pathing();
          if (null != tmp) return tmp;
        }
      }

      // heuristics that install late-pathing go here
      tmp = BehaviorHandleDeathTrap();
      if (null != tmp) return tmp;

      return null;
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

    public bool ProposeSwitchPlaces(Location dest)
    {
      if (null != WouldUseAccessibleStack(m_Actor.Location)) return false;
      if (null != WouldUseAccessibleStack(in dest)) return true;
      var track_inv = Goal<Goal_PathToStack>();
      if (null != track_inv) {
        var dests = track_inv.Destinations;
        if (dests.Any()) {
          if (dests.Min(loc => Rules.GridDistance(in loc,m_Actor.Location)) > dests.Min(loc => Rules.GridDistance(in loc, dest))) return false;
//        if (dests.Min(loc => Rules.GridDistance(in loc,m_Actor.Location)) < dests.(loc => Rules.GridDistance(in loc, dest))) return true;
        }
      }
      return true;
    }

    public bool RejectSwitchPlaces(Location dest)
    {
      if (null != WouldUseAccessibleStack(m_Actor.Location)) return true;
      if (null != WouldUseAccessibleStack(in dest)) return false;

      var want_here = WantToGoHere(m_Actor.Location);
      if (null != want_here) return !want_here.Contains(dest);

      var track_inv = Goal<Goal_PathToStack>();
      if (null != track_inv) {
        var dests = track_inv.Destinations;
        if (dests.Any()) {
          var old_dist = track_inv.Destinations.Min(loc => Rules.GridDistance(in loc, m_Actor.Location));
          var new_dist = track_inv.Destinations.Min(loc => Rules.GridDistance(in loc, dest));
          if (old_dist > new_dist) return false;
          if (old_dist < new_dist) return true;
        }
      }

      var already_near_actor = GetCloseToActor();
      if (   null!=already_near_actor.Key && 0<already_near_actor.Value // reject default-initialization
          && InProximity(in dest, already_near_actor.Key.Location, already_near_actor.Value))
        return false;

      return true;
    }

#nullable enable
/// <summary>
/// Enemies check should have completed before this.
/// </summary>
/// <returns>null, or a performable action</returns>
    protected ActorAction? NonCombatReflexMoves() {
      ActorAction? act = BehaviorUseMedecine(2, 1, 2, 4, 2);
      if (null != act) return act;
      act = BehaviorRestIfTired();
      if (null != act) return act;
      act = BehaviorEatProactively();
      if (null != act) return act;
      if (m_Actor.IsHungry) {
        act = BehaviorEat();
        if (null != act) return act;
        if (m_Actor.IsStarving || m_Actor.IsInsane) {
          act = BehaviorGoEatCorpse(FilterCurrent(_all));
          if (null != act) return act;
        }
      }

      act = TurnOnAdjacentGenerators();
      if (null != act) {
        SetObjective(new Goal_NonCombatComplete(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, new ActionSequence(m_Actor, new int[] { (int)ZeroAryBehaviors.TurnOnAdjacentGenerators_ObjAI })));
        return act;
      }
      act = RechargeWithAdjacentGenerator();
      if (null!= act) return act;

      // while groggy ai may not be up to ranged inventory management, items in reach should still be managed
      // XXX this should lose to same-map threat hunting at close ETA
      act = InventoryStackTactics();
      if (null != act) return act;
      act = BehaviorUseAdjacentStack();
      if (null != act && act.IsPerformable()) return act;

      act = BehaviorDropUselessItem();    // inventory normalization should normally be a no-op
      if (null != act) return act;

      // lifted here to break action loop 2020-10-30 zaimoni
      if (2<=WantRestoreSAN) {  // intrinsic item rating code for sanity restore is want or higher
        act = BehaviorUseEntertainment();
        if (null != act)  return act;
      }

      // XXX this should lose to same-map threat hunting at close ETA
      act = BehaviorRangedInventory();
      if (null != act) return act;

      return null;
    }
#nullable restore

    // taboos
    public void MarkActorAsRecentTrade(Actor other)
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
