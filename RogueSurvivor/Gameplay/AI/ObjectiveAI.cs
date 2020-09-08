#define INTEGRITY_CHECK_ITEM_RETURN_CODE
#define PATHFIND_IMPLEMENTATION_GAPS
// #define TRACE_GOALS

using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class Goal_RestRatherThanLoseturnWhenTired : Objective
  {
    public Goal_RestRatherThanLoseturnWhenTired(int t0, Actor who)
    : base(t0,who)
    {
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (!m_Actor.IsTired) {
        _isExpired = true;
        return true;
      }
      if (null != m_Actor.Controller.enemies_in_FOV) return false;
      if (m_Actor.CanActNextTurn) return false;
      ret = new ActionWait(m_Actor);
      return true;
    }
  }

  [Serializable]
  internal class Goal_RecoverSTA : Objective
  {
    public readonly int targetSTA;

    public Goal_RecoverSTA(int t0, Actor who, int target)
    : base(t0,who)
    {
      targetSTA = target;
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (null != m_Actor.Controller.enemies_in_FOV) {
        _isExpired = true;
        return true;
      }
      ret = (m_Actor.Controller as ObjectiveAI).DoctrineRecoverSTA(targetSTA);
      if (null == ret) _isExpired = true;
      return true;
    }
  }

  [Serializable]
  internal class Goal_MedicateSLP : Objective
  {
    public Goal_MedicateSLP(int t0, Actor who)
    : base(t0,who)
    {
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (null != m_Actor.Controller.enemies_in_FOV) {
        _isExpired = true;
        return true;
      }
      ret = (m_Actor.Controller as ObjectiveAI).DoctrineMedicateSLP();
      if (null == ret) _isExpired = true;
      return true;
    }
  }

  [Serializable]
  internal class Goal_RechargeAll : Objective
  {
    public Goal_RechargeAll(int t0, Actor who)
    : base(t0,who)
    {
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (null != m_Actor.Controller.enemies_in_FOV) {
        _isExpired = true;
        return true;
      }
      {
      var lights = m_Actor?.Inventory.GetItemsByType<ItemLight>();
      if (0 < (lights?.Count ?? 0)) {
        foreach(var x in lights) {
          ret = (m_Actor.Controller as ObjectiveAI)?.DoctrineRechargeToFull(x);
          if (null != ret) return true;
        }
      }
      }
      {
      var trackers = m_Actor?.Inventory.GetItemsByType<ItemTracker>(it => GameItems.IDs.TRACKER_POLICE_RADIO!=it.Model.ID);
      if (0 < (trackers?.Count ?? 0)) {
        foreach(var x in trackers) {
          ret = (m_Actor.Controller as ObjectiveAI)?.DoctrineRechargeToFull(x);
          if (null != ret) return true;
        }
      }
      }

      _isExpired = true;
      return true;
    }
  }

  [Serializable]
  internal class Goal_Butcher : Objective
  {
    private readonly Corpse _corpse;

    public Goal_Butcher(int t0, Actor who, Corpse target)
    : base(t0,who)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      _corpse = target;
    }

    public override bool UrgentAction(out ActorAction ret)
    {
      ret = null;
      if (null != m_Actor.Controller.enemies_in_FOV) {
        _isExpired = true;
        return true;
      }
      ret = (m_Actor.Controller as ObjectiveAI)?.DoctrineButcher(_corpse);
      if (null == ret) _isExpired = true;
      return true;
    }
  }

  [Serializable]
  internal abstract class ObjectiveAI : BaseAI
  {
    protected ObjectiveAI(Actor src) : base(src) {}

    public enum SparseData {
      LoF = 0,   // line of fire -- should be telegraphed and obvious to anyone looking at the ranged weapon user, at least the near part (5 degree precision?)
      CloseToActor,
      ClearingZone,
      UsingChokepoint,
      MinStepPath,   // may be either List<List<Point>> or List<List<Location>>
      PathingTo,
      EscapingTo,
      LambdaPath
    };

    public enum ZeroAryBehaviors {
      AttackWithoutMoving_ObjAI = 0,
      WaitIfSafe_ObjAI,
      TurnOnAdjacentGenerators_ObjAI
    };

    protected enum CallChain {
      NONE = 0,
      ManageMeleeRisk,   // OrderableAI::ManageMeleeRisk; this caller is retreating and needs additional postprocessing
      SelectAction_LambdaPath   // ...::SelectAction: path is from a lambda pathing block and should be recorded to the lambda path cache
    }

    readonly protected List<Objective> Objectives = new List<Objective>();
    readonly private Dictionary<Point,Dictionary<Point, int>> PlannedMoves = new Dictionary<Point, Dictionary<Point, int>>();
    readonly private sbyte[] ItemPriorities = new sbyte[(int)GameItems.IDs._COUNT]; // XXX probably should have some form of PC override
    readonly private UntypedCache<SparseData> _sparse = new UntypedCache<SparseData>();
    private int _STA_reserve;
    protected int STA_reserve { get { return _STA_reserve; } }

    // cache variables
    [NonSerialized] protected List<Point> _legal_steps = null;
    [NonSerialized] protected Dictionary<Location,ActorAction> _legal_path = null;
    [NonSerialized] protected Dictionary<Point, int> _damage_field = null;
    [NonSerialized] protected List<Actor> _slow_melee_threat = null;
    [NonSerialized] protected HashSet<Actor> _immediate_threat = null;
    [NonSerialized] protected HashSet<Point> _blast_field = null;
#nullable enable
    [NonSerialized] protected List<Point>? _retreat = null;
    [NonSerialized] protected List<Point>? _run_retreat = null;
#nullable restore
    [NonSerialized] protected bool _safe_retreat = false;
    [NonSerialized] protected bool _safe_run_retreat = false;
#nullable enable
    protected ActionMoveDelta? _last_move = null;   // for detecting period 2 move looping
#nullable restore
    [NonSerialized] protected bool _used_advanced_pathing = false;
    [NonSerialized] protected bool _rejected_backtrack = false;
    [NonSerialized] protected HashSet<Location> _current_goals = null;
    [NonSerialized] protected CallChain _caller = CallChain.NONE;
    [NonSerialized] protected ActorAction? _staged_action = null;   // should be a free action
#if USING_ESCAPE_MOVES
    [NonSerialized] protected Dictionary<Location,ActorAction> _escape_moves = null;
#endif

    public void ClearLastMove() { _last_move = null; }

    public virtual bool UsesExplosives { get { return true; } } // default to what PC does

    public T Goal<T>(Func<T,bool> test) where T:Objective { return Objectives.FirstOrDefault(o => o is T goal && test(goal)) as T;}
    public T Goal<T>() where T:Objective { return Objectives.FirstOrDefault(o => o is T) as T;}

    // thin wrapper for when the key logic is elsewhere; we still prefer central-logic specializations)
    public void SetObjective(Objective src) {
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      bool authorized = src.Actor == m_Actor;   // retain this as we may want more precision later (e.g, some things we may accept from leader but not from mates)
      if (!authorized)
#if DEBUG
        throw new InvalidOperationException(src.Actor.Name+" not allowed to give objectives to "+m_Actor.Name);
#else
        return;
#endif
      // for now, treat this as "early"
      Objectives.Insert(0,src);
    }

    public ActorAction Pathing<T>() where T:Objective,Pathable
    {
        var remote = Goal<T>();
        if (null != remote) {
          var ret = remote.Pathing();
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, remote.ToString()+": " +ret.to_s());
#endif
          if (null != ret) return ret;
        }
        return null;
    }

    public override bool IsEngaged { get {
      if (base.IsEngaged) return true;
      if (null != _enemies) return true;
      return null!=Goal<Goal_Terminate>();
    } }

    public override bool InCombat { get {
      if (IsEngaged) return true;   // inlining base.IsCombat
      var threats = m_Actor.Threats;
      if (null != threats) {
        if (threats.AnyThreatIn(m_Actor.Location.Map,m_Actor.Location.LocalView)) return true;
      }

      return false;
    } }

    public bool IsFocused { get {
      if (null!=Goal<Goal_NextAction>()) return true;
      if (null!=Goal<Goal_NextCombatAction>()) return true;
      if (null!=Goal<Goal_NonCombatComplete>()) return true;
      if (null!=Goal<Goal_BreakLineOfSight>()) return true;
      if (null!=Goal<Goal_HintPathToActor>()) return true;
      return false;
    } }

    public void CancelPathTo(Actor dest) {
      var goal = Goal<Goal_HintPathToActor>(o => o.Whom == dest);
      if (null != goal) Objectives.Remove(goal);    // \todo? optimize...could just find index and remove that index immediately
    }

    public override ActorAction? ExecAryZeroBehavior(int code)
    {
      switch(code) {
        case (int)ZeroAryBehaviors.AttackWithoutMoving_ObjAI: return AttackWithoutMoving();
        case (int)ZeroAryBehaviors.WaitIfSafe_ObjAI: return WaitIfSafe();
        case (int)ZeroAryBehaviors.TurnOnAdjacentGenerators_ObjAI: return TurnOnAdjacentGenerators();
        default: return base.ExecAryZeroBehavior(code);
      }
    }

    public void Stage(ActorAction act) { _staged_action = act; }

    protected override void ResetAICache()
    {
      base.ResetAICache();
      _legal_steps = null;
      _legal_path = null;
      _damage_field = null;
      _slow_melee_threat = null;
      _immediate_threat = null;
      _blast_field = null;
      _retreat = null;
      _run_retreat = null;
      _safe_retreat = false;
      _safe_run_retreat = false;
      _used_advanced_pathing = false;
      _rejected_backtrack = false;
      _current_goals = null;
#if USING_ESCAPE_MOVES
      _escape_moves = null;
#endif
    }

    public void OnRaid(RaidType raid, in Location loc)
    {
      if (!m_Actor.IsSleeping) _onRaid(raid, loc);
    }

    protected abstract void _onRaid(RaidType raid, in Location loc);

    /// <summary>
    /// should maintain any cache data that is location-based; incomplete implementation 2019-08-23
    /// </summary>
    public void OnMove() {
      if (PathToTarget.reject_path(GetMinStepPath<Point>(),this) || PathToTarget.reject_path(GetMinStepPath<Location>(),this)) {
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget) {
            var pt_path = GetMinStepPath<Point>();
            var loc_path = GetMinStepPath<Location>();
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "removing invalidated path "+(pt_path?.to_s() ?? loc_path.to_s()));
        }
#endif
        _sparse.Unset(SparseData.MinStepPath);
        _last_move = null;
      }
      if (GetLambdaPath()?.Expire(this) ?? false) {
        _sparse.Unset(SparseData.LambdaPath);
        _last_move = null;
      }
    }

    public void SparseReset()
    {
      _sparse.Unset(SparseData.LoF);
      _sparse.Unset(SparseData.CloseToActor);
      _sparse.Unset(SparseData.ClearingZone);
      _sparse.Unset(SparseData.EscapingTo);
      var choke = GetChokepoint();
      if (null != choke && 0>=choke.Contains(m_Actor.Location)) _sparse.Unset(SparseData.UsingChokepoint);

      OnMove();  // 2019-08-24: both calls required to pass regression test
    }

    private void UpdateRetreatDestinations()
    {
      // calculate retreat destinations if possibly needed
      if (null != _damage_field && null != _legal_steps && _damage_field.ContainsKey(m_Actor.Location.Position)) {
        _retreat = FindRetreat();
        if (null != _retreat) {
          AvoidBeingCornered();
          _safe_retreat = !_damage_field.ContainsKey(_retreat[0]);
        }
        if (m_Actor.RunIsFreeMove && m_Actor.CanRun() && !_safe_retreat) {
          _run_retreat = FindRunRetreat();
          if (null != _run_retreat) {
            AvoidBeingRunCornered();
            _safe_run_retreat = !_damage_field.ContainsKey(_run_retreat[0]);
          }
        }
      }
#if USING_ESCAPE_MOVES
      var enemies = enemies_in_FOV;
      if (null == enemies) return;

      // \todo augment above.  We actually want to:
      // * respond to all slow threat, not just immediate slow melee threat
      // * declare retreat destinations so allies/friends don't zugzwang us
      // * declare "good ambush positions" for melee/ranged combat based on predicted enemy positions
      // * internally record escape action sequences (door opening is 2-moves and if we lose a move after step/jump that needs accounting for as well
      int escape_cost(ActorAction escape) {
        if (escape is ActionOpenDoor) return 2;
        if (escape is ActionMoveStep && m_Actor.RunIsFreeMove && m_Actor.CanRun()) return 0;
        if (escape is ActorDest) return 1;  // backstop, need to account for fatigue and/or running
        return int.MaxValue;    // doesn't work in time
      }

      var escape_actions = m_Actor.OnePath(m_Actor.Location);
      var escape_costs = new Dictionary<Location,int>();
      var escape_damage = new Dictionary<Location,int>();
      escape_actions.OnlyIf(act => act.IsPerformable() && !VetoAction(act));
      {
      var secondary_escape_actions = new Dictionary<Location,ActorAction>();
      int damage_relay = 0;
      _damage_field?.TryGetValue(m_Actor.Location.Position, out damage_relay);
      foreach(var x in escape_actions) {
        int cost = escape_cost(x.Value);
        if (2 < cost) continue;    // not fast enough
        escape_costs[x.Key] = cost;
        if (0==cost) {
          var extra_actions = m_Actor.OnePath(x.Key);
          extra_actions.OnlyIf((Predicate<Location>)(loc => 2==Rules.InteractionDistance(m_Actor.Location,x.Key)));
          foreach(var y in extra_actions) {
            int alt_cost = escape_cost(y.Value);
            if (1<alt_cost) continue;
            if (0 == alt_cost) alt_cost = 1;
            escape_costs[y.Key] = alt_cost;
            secondary_escape_actions[y.Key] = y.Value;
          }
        }
      }
      foreach(var x in secondary_escape_actions) escape_actions[x.Key] = x.Value;
      {
      var remove = new List<Location>(escape_actions.Count);
      foreach(var x in escape_actions) if (!escape_costs.ContainsKey(x.Key)) remove.Add(x.Key);
      if (0<remove.Count) foreach(var loc in remove) escape_actions.Remove(loc);
      }

      var backup = new Dictionary<Location,ActorAction>(escape_actions);
      var ok = new Dictionary<Location, ActorAction>(escape_actions);
      foreach(var x in escape_actions) {
        int cost = escape_costs[x.Key];
        int damage = 0;
        if (x.Key.Map==m_Actor.Location.Map && (_damage_field?.TryGetValue(x.Key.Position,out damage) ?? false)) {
          if (2 <= cost) damage += damage_relay;    // <= in case of specification change
          escape_damage[x.Key] = damage;
        } else if (2 <= cost && 0 < damage_relay) escape_damage[x.Key] = damage_relay;
        backup[x.Key] = x.Value;    // works but may harm others
        if (VetoAction(x.Value)) continue;  // does harm others
        ok[x.Key] = x.Value;
      }
      escape_actions = (0 < ok.Count) ? ok : backup;
      }
      if (0 >= escape_actions.Count) return;    // might want to record this fact to trigger sheer panic messaging
      // \todo identify which escape actions are actually needed, based on where enemies are predicted to be
      var compromised = new Dictionary<Location,int>();
      foreach(var x in escape_actions) compromised[x.Key] = 0;
      compromised[m_Actor.Location] = 0;
      var double_compromised = new Dictionary<Location,int>(compromised); 

      foreach(var x in enemies) {
        var rws = m_Actor?.Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || null != m_Actor.Inventory.GetCompatibleAmmoItem(rw));
        if (null != rws) continue;  // bail for now on ranged weapons
        int e_moves = m_Actor.HowManyTimesOtherActs(1,x.Value);
        if (0 < e_moves) {
          var now = new HashSet<Location> { x.Key };
          var ever = new HashSet<Location>();
          var next = new HashSet<Location>();
          int n = e_moves;
          do {
            foreach(var src in now) {
              var danger = new ZoneLoc(src.Map,new Rectangle(src.Position.X-1,src.Position.Y-1,3,3));
              danger.DoForEach(loc => {
                if (ever.Contains(loc)) return;
                ever.Add(loc);
                if (!loc.Map.IsWalkableFor(loc.Position,x.Value)) return;
                compromised.TryGetValue(loc, out var targeted);
                compromised[loc] = targeted + 1;
                double_compromised[loc] = targeted + 1;
                next.Add(loc);
              });
            }
            var swap = now;
            next = now;
            swap.Clear();
            next = swap;
          } while (0 < --n);
          int e_moves2 = m_Actor.HowManyTimesOtherActs(2,x.Value);
          if (e_moves < e_moves2) {
            n = e_moves2 - e_moves;
            e_moves = e_moves2;
            do {
              foreach(var src in now) {
               var danger = new ZoneLoc(src.Map,new Rectangle(src.Position.X-1,src.Position.Y-1,3,3));
                 danger.DoForEach(loc => {
                  if (ever.Contains(loc)) return;
                  ever.Add(loc);
                  if (!loc.Map.IsWalkableFor(loc.Position,x.Value)) return;
                  double_compromised.TryGetValue(loc, out var targeted);
                  double_compromised[loc] = targeted + 1;
                  next.Add(loc);
                });
              }
              var swap = now;
              next = now;
              swap.Clear();
              next = swap;
            } while (0 < --n);
          }
          e_moves2 = (0==double_compromised[m_Actor.Location] ? m_Actor.HowManyTimesOtherActs(3,x.Value) : 0);
          if (e_moves < e_moves2) {
            n = e_moves2 - e_moves;
            e_moves = e_moves2;
            do {
              foreach(var src in now) {
               var danger = new ZoneLoc(src.Map,new Rectangle(src.Position.X-1,src.Position.Y-1,3,3));
                 danger.DoForEach(loc => {
                  if (ever.Contains(loc)) return;
                  ever.Add(loc);
                  if (!loc.Map.IsWalkableFor(loc.Position,x.Value)) return;
                  double_compromised.TryGetValue(loc, out var targeted);
                  double_compromised[loc] = targeted + 1;
                  next.Add(loc);
                });
              }
              var swap = now;
              next = now;
              swap.Clear();
              next = swap;
            } while (0 < --n);
          }
        }
      }

      if (0 >= double_compromised[m_Actor.Location]) return;    // may need to do a strategic retreat but not our immediate issue
      // prefer uncompromised escapes
      var threshold = escape_actions.Min(x => double_compromised[x.Key]);
      escape_actions.OnlyIf(loc => threshold==double_compromised[loc]);
      // if we have any escapes with a time cost of 1, ignore those with a time cost of 2
      bool have_fast_escape = escape_actions.Any(x => 1 == escape_costs[x.Key]);
      if (have_fast_escape) escape_actions.OnlyIf(loc => 1>=escape_costs[loc]);
      // prefer not being cornered
      {
      var remove = new List<Location>(escape_actions.Count);
      foreach(var x in escape_actions) {
        int dist = Rules.InteractionDistance(m_Actor.Location,x.Key);
        bool ok = false;
        foreach(var dir in Direction.COMPASS) {
          Location test = x.Key+dir;
          if (!test.ForceCanonical()) continue;
          if (dist > Rules.GridDistance(m_Actor.Location,test)) continue;  // yes, int.MaxValue for a different map is ok here
          if (!test.Map.IsWalkableFor(test.Position,m_Actor)) continue;
          ok = true;
          break;
        }
        if (!ok) remove.Add(x.Key);
      }
      if (0<remove.Count && escape_actions.Any(x => !remove.Contains(x.Key))) foreach(var loc in remove) escape_actions.Remove(loc);
      }

      // prefer closer escapes
      if (escape_actions.Any(x => 1==Rules.InteractionDistance(x.Key,m_Actor.Location))) escape_actions.OnlyIf(loc => 1==Rules.InteractionDistance(loc,m_Actor.Location));
      RecordWantToEscapeTo(escape_actions.Keys.ToList());   // \todo declare ambush points as well
      if (!have_fast_escape || 0< compromised[m_Actor.Location]) _escape_moves = escape_actions;
#endif
    }

    // morally a constructor-type function
    protected void InitAICache(List<Percept> now, List<Percept> all_time=null)
    {
      _initAICache();

      // sparse data reset is here (start of select action) so it persists during other actors' turns
      SparseReset();

      // AI cache fields
      _legal_steps = m_Actor.LegalSteps;
      _damage_field = new Dictionary<Point, int>();
      _slow_melee_threat = new List<Actor>();
      _immediate_threat = new HashSet<Actor>();
      if (null != enemies_in_FOV) VisibleMaximumDamage(_damage_field, _slow_melee_threat, _immediate_threat);
      AddTrapsToDamageField(_damage_field, now);
      if (UsesExplosives) {   // only civilians and soldiers respect explosives; CHAR and gang don't
        _blast_field = new HashSet<Point>();  // thrashes GC for GangAI/CHARGuardAI
        AddExplosivesToDamageField(all_time);
        if (0>= _blast_field.Count) _blast_field = null;
      }
      if (0>= _damage_field.Count) _damage_field = null;
      if (0>= _slow_melee_threat.Count) _slow_melee_threat = null;
      if (0>= _immediate_threat.Count) _immediate_threat = null;

      UpdateRetreatDestinations();

      _legal_path = m_Actor.OnePath(m_Actor.Location);
      _legal_path.OnlyIf(act => act.IsPerformable() && !VetoAction(act));
      if (0 >= _legal_path.Count) _legal_path = null;
      if (null!=_last_move && _last_move.dest!=m_Actor.Location) _last_move = null;
    }

    protected override void RecordLastAction(ActorAction act) {
      if (null == act || !act.PerformedBy(m_Actor)) return; // not ours, reject
      if (act is ActorDest dest && 1==Rules.InteractionDistance(m_Actor.Location,dest.dest)) {  // a movement-type action
        // the one type that actually knows the origin; the legacy actions don't.
        if (!(act is ActionMoveDelta record)) record = new ActionMoveDelta(m_Actor,dest.dest);
        _last_move = record;
#if TRACE_SELECTACTION
        if (m_Actor.IsDebuggingTarget)  Logger.WriteLine(Logger.Stage.RUN_MAIN, "recording "+record+" for "+act);
#endif
        return;
      }
      if (act is ActionTake || act is ActorTake || act is ActionTrade) {
        // these actions are inventory-altering and will invalidate inventory-based pathing
        _last_move = null;
        return;
      }
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget)  Logger.WriteLine(Logger.Stage.RUN_MAIN, "non-move "+act+" not recorded, leaving "+(_last_move?.ToString() ?? "null")+" in place");
#endif
    }

    public int RiskAt(in Location loc) {
      if (null != _damage_field) {
        var denorm = m_Actor.Location.Map.Denormalize(in loc);
        if (null != denorm) {
          _damage_field.TryGetValue(loc.Position,out var damage);
          return damage;
        }
      }
      int risk = 0;
      if (null != _enemies) {
        foreach(var en in _enemies) {
          var rw = ((en.Percepted as Actor).Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo();
          int range = null!=rw ? rw.Model.Attack.Range : 1; // Father Time's scythe is range 2
          if (range < Rules.InteractionDistance(in loc,en.Location)) continue;
          risk += null!=rw ? rw.Model.Attack.DamageValue : (en.Percepted as Actor).CurrentMeleeAttack.DamageValue;
        }
      }
      if (0 < risk) return risk;
      var en_FOV = enemies_in_FOV;
      if (null != en_FOV) {
        foreach(var en in en_FOV) {
          var rw = (en.Value.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo();
          int range = null!=rw ? rw.Model.Attack.Range : 1; // Father Time's scythe is range 2
          if (range < Rules.InteractionDistance(in loc,en.Key)) continue;
          risk += null!=rw ? rw.Model.Attack.DamageValue : en.Value.CurrentMeleeAttack.DamageValue;
        }
      }
      return risk;
    }

    public ActorAction UsePreexistingPath(List<List<Point>> path_pt, HashSet<Location> goals=null)
    {
        if (null == path_pt || 0 >= path_pt.Count) return null;
        if (2 <= path_pt.Count) {
              // relevant.  Assume that if there is a 2-step min-cost path on this that we want the first step
              var candidates = new List<List<ActionMoveDelta>>();
              var stage2 = new List<ActionMoveDelta>();

              { // scope brace for depth2
              var depth2 = path_pt[1];
              if (null != goals) {
                depth2 = path_pt[1].FindAll(pt => goals.Contains(new Location(m_Actor.Location.Map,pt)));
                if (0 >= depth2.Count) depth2 = path_pt[1].FindAll(pt => goals.Any(pt2 => 1==Rules.GridDistance(pt2.Position,in pt)));
                if (0 >= depth2.Count) depth2 = path_pt[1];
              }

              foreach(var pt in path_pt[0]) {
                var loc = new Location(m_Actor.Location.Map, pt);
                if (!_legal_path.ContainsKey(loc)) continue;  // something wrong here
                if (!m_Actor.CanEnter(loc)) continue;  // generators are pathable but not enterable
                if (1 >= FastestTrapKill(in loc)) continue;
                foreach(var pt2 in depth2) {
                  if (1!=Rules.GridDistance(in pt,in pt2)) continue;
                  var loc2 = new Location(m_Actor.Location.Map, pt2);
                  if (!m_Actor.CanEnter(loc2)) continue;
                  if (1 >= FastestTrapKill(in loc2)) continue;
                  var act = new ActionMoveDelta(m_Actor, in loc2, in loc);
                  if (act.IsLegal()) stage2.Add(act);
                }
              }
              } // end scope brace for depth2
              // check for min-cost two steps
              var costs = new Dictionary<Location,int>();
              foreach(var pt in path_pt[0]) {
                var loc = new Location(m_Actor.Location.Map, pt);
                if (!_legal_path.TryGetValue(loc,out var considering)) continue;
                if (1<Map.PathfinderMoveCosts(considering)) continue;
                foreach(var act in stage2) {
                  if (act.origin != loc || 1<Map.PathfinderMoveCosts(act)) continue;
                  costs[loc] = 1;
                  break;
                }
              }
              if (1==costs.Count) return _legal_path[costs.First().Key];
              if (0 < costs.Count) return DecideMove(costs);
        }
        // one step remaining
        {
              var costs = new Dictionary<Location,int>();
              foreach(var pt in path_pt[0]) {
                var loc = new Location(m_Actor.Location.Map, pt);
                if (!_legal_path.TryGetValue(loc,out var considering)) continue;
                costs[loc] = Map.PathfinderMoveCosts(considering);
              }
              if (1==costs.Count) return _legal_path[costs.First().Key];
              if (0 < costs.Count) return DecideMove(costs);
        }
        return null;
    }

    public ActorAction UsePreexistingPath(List<List<Location>> path_pt, HashSet<Location> goals=null)
    {
        if (null == path_pt || 0 >= path_pt.Count) return null;
        if (2 <= path_pt.Count) {
              // relevant.  Assume that if there is a 2-step min-cost path on this that we want the first step
              var candidates = new List<List<ActionMoveDelta>>();
              var stage2 = new List<ActionMoveDelta>();

              { // scope brace for depth2
              var depth2 = path_pt[1];
              if (null != goals) {
                depth2 = path_pt[1].FindAll(pt => goals.Contains(pt));
                if (0 >= depth2.Count) depth2 = path_pt[1].FindAll(pt => goals.Any(pt2 => 1==Rules.GridDistance(in pt2,pt)));
                if (0 >= depth2.Count) depth2 = path_pt[1];
              }

              foreach(var pt in path_pt[0]) {
                if (!_legal_path.ContainsKey(pt)) continue;  // something wrong here
                if (!m_Actor.CanEnter(pt)) continue;  // generators are pathable but not enterable
                if (1 >= FastestTrapKill(in pt)) continue;
                foreach(var pt2 in depth2) {
                  if (1!=Rules.GridDistance(in pt, in pt2)) continue;
                  if (!m_Actor.CanEnter(pt2)) continue;
                  if (1 >= FastestTrapKill(in pt2)) continue;
                  var act = new ActionMoveDelta(m_Actor, in pt2, in pt);
                  if (act.IsLegal()) stage2.Add(act);
                }
              }
              } // end scope brace for depth2
              // check for min-cost two steps
              var costs = new Dictionary<Location,int>();
              foreach(var loc in path_pt[0]) {
                if (!_legal_path.TryGetValue(loc,out var considering)) continue;
                if (1<Map.PathfinderMoveCosts(considering)) continue;
                foreach(var act in stage2) {
                  if (act.origin != loc || 1<Map.PathfinderMoveCosts(act)) continue;
                  costs[loc] = 1;
                  break;
                }
              }
              if (1==costs.Count) return _legal_path[costs.First().Key];
              if (0 < costs.Count) return DecideMove(costs);
        }
        // one step remaining
        {
              var costs = new Dictionary<Location,int>();
              foreach(var loc in path_pt[0]) {
                if (!_legal_path.TryGetValue(loc,out var considering)) continue;
                costs[loc] = Map.PathfinderMoveCosts(considering);
              }
              if (1==costs.Count) return _legal_path[costs.First().Key];
              if (0 < costs.Count) return DecideMove(costs);
        }
        return null;
    }

    protected ActorAction UsePreexistingPath(HashSet<Location> goals=null)
    {
        var ret = UsePreexistingPath(GetMinStepPath<Point>(), goals);
        if (null != ret) return ret;
        ret = UsePreexistingPath(GetMinStepPath<Location>(), goals);
        if (null != ret) return ret;
        return null;
    }

    protected ActorAction UsePreexistingLambdaPath() { return GetLambdaPath()?.WalkPath(this); }

    private ActorAction _pathNear(Location loc)
    {
        if (null != _legal_path) {
            var candidates = new List<ActorAction>(_legal_path.Count);
            foreach (var x in _legal_path) {
                if (x.Key == loc) continue;
                if (1 != Rules.InteractionDistance(x.Key, in loc)) continue;
                candidates.Add(x.Value);
            }
            if (0 < candidates.Count) return Rules.Get.DiceRoller.Choose(candidates);
        }
        return null;
    }

#region sparse data accessors
    // protected setters could be eliminated by downgrading _sparse to protected, but types have to be manually aligned between set/get anyway
    public void RecordLoF(List<Point> LoF)  // XXX access control weakness required by RogueGame
    {
      if (null == LoF || 1>=LoF.Count) return;
      _sparse.Set(SparseData.LoF,LoF);
    }

    public List<Point> GetLoF() { return _sparse.Get<List<Point>>(SparseData.LoF); }   // XXX reference-copy return 
    protected void RecordCloseToActor(Actor a,int maxDist) { _sparse.Set(SparseData.CloseToActor,new KeyValuePair<Actor,int>(a,maxDist)); }
    public KeyValuePair<Actor, int> GetCloseToActor() { return _sparse.Get<KeyValuePair<Actor, int>>(SparseData.CloseToActor); }

    protected void RecordChokepoint(LinearChokepoint src) {
      if (null == src || 0 >= src.Contains(m_Actor.Location)) return;
      _sparse.Set(SparseData.UsingChokepoint, src);
    }
    public LinearChokepoint GetChokepoint() { return _sparse.Get<LinearChokepoint>(SparseData.UsingChokepoint); }

    // caller does not have the goal-set that mandated calculating this
    // caller does have the action generated from the path
    protected ActorAction RecordMinStepPath(List<List<Point>> src, ActorAction act) {
      if (null == src) return act;
      if (act is ActorDest test && null != _last_move && test.dest == _last_move.origin) {
          var alt_act = _pathNear(test.dest);
          if (null != alt_act) {
            if (CallChain.SelectAction_LambdaPath == _caller) {
              var prior = GetLambdaPath();
              prior.Install(m_Actor.Location.Map, src,this);
              _sparse.Unset(SparseData.MinStepPath);
            } else {
             _sparse.Set(SparseData.MinStepPath,src);
            }
            return alt_act;
          }

          // no good ... should not be overwriting
          alt_act = UsePreexistingPath();
          if (null!=alt_act) {
             _rejected_backtrack = true; // signal that the path isn't really for the current goals
             return alt_act;
          }
          if (null != _current_goals && !_current_goals.ValueEqual(GetPreviousGoals())) _last_move = null;
#if DETECT_PERIOD_2_MOVE_LOOP
          else throw new InvalidOperationException(m_Actor.Name+" committed a period-2 move loop on turn "+m_Actor.Location.Map.LocalTime.TurnCounter+": "+_last_move+", "+act);
#endif
      }
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "most recent path: "+src.to_s());
#endif
      if (CallChain.SelectAction_LambdaPath == _caller) {
        var prior = GetLambdaPath();
        prior.Install(m_Actor.Location.Map,src,this);
        _sparse.Unset(SparseData.MinStepPath);
      } else {
        _sparse.Set(SparseData.MinStepPath,src);
      }
      return act;
    }

    protected ActorAction RecordMinStepPath(List<List<Location>> src, ActorAction act) {
      if (null == src) return act;
      if (act is ActorDest test && null != _last_move && test.dest == _last_move.origin) {
          var alt_act = _pathNear(test.dest);
          if (null != alt_act) {
            if (CallChain.SelectAction_LambdaPath == _caller) {
              var prior = GetLambdaPath();
              prior.Install(src,this);
              _sparse.Unset(SparseData.MinStepPath);
            } else {
              _sparse.Set(SparseData.MinStepPath,src);
            }
            return alt_act;
          }

          // no good ... should not be overwriting
          alt_act = UsePreexistingPath();
          if (null!=alt_act) {
             _rejected_backtrack = true; // signal that the path isn't really for the current goals
             return alt_act;
          }
          if (null == _current_goals || !_current_goals.ValueEqual(GetPreviousGoals())) _last_move = null;
#if DETECT_PERIOD_2_MOVE_LOOP
          else throw new InvalidOperationException(m_Actor.Name+" committed a period-2 move loop on turn "+m_Actor.Location.Map.LocalTime.TurnCounter+": "+_last_move+", "+act);
#endif
      }
#if TRACE_SELECTACTION
       if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "most recent path: " + src.to_s());
#endif
      if (CallChain.SelectAction_LambdaPath == _caller) {
        var prior = GetLambdaPath();
        prior.Install(src,this);
        _sparse.Unset(SparseData.MinStepPath);
      } else {
        _sparse.Set(SparseData.MinStepPath,src);
      }
      return act;
    }

    public HashSet<Location> GetPreviousGoals() { return _sparse.Get<HashSet<Location>>(SparseData.PathingTo); }

    protected void RecordGoals(HashSet<Location> src) {
      if (null == src) return;
      if (_rejected_backtrack) return;
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "most recent goals: "+src.to_s());
#endif
      _sparse.Set(SparseData.PathingTo,src);
      _used_advanced_pathing = true;
    }

    public List<List<T>> GetMinStepPath<T>() { return _sparse.Get<List<List<T>>>(SparseData.MinStepPath); }
    protected PathToTarget GetLambdaPath() { return _sparse.Get<PathToTarget>(SparseData.LambdaPath); }
    protected PathToTarget ForceLambdaPath() {
      var prior = _sparse.Get<PathToTarget>(SparseData.LambdaPath);
      if (null == prior) {
        prior = new PathToTarget();
        _sparse.Set(SparseData.LambdaPath, prior);
      }
      return prior;
    }

#if USING_ESCAPE_MOVES
    protected void RecordWantToEscapeTo(List<Location> src) { _sparse.Set(SparseData.EscapingTo, src); }
    public List<Location> WantToEscapeTo() { return _sparse.Get<List<Location>>(SparseData.EscapingTo); }
#endif

    public ZoneLoc ClearingThisZone() {
      var ret = _sparse.Get<ZoneLoc>(SparseData.ClearingZone);
      if (null != ret) return ret;
      if (m_Actor.Location.Map.IsInsideAt(m_Actor.Location.Position)) {
        Rectangle scan_this = m_Actor.Location.Map.Rect;
        var z_list = m_Actor.Location.Map.GetZonesAt(m_Actor.Location.Position);    // non-null check in map generation
        foreach(var z in z_list) {
          if (scan_this.Width < z.Bounds.Width) continue;
          if (scan_this.Height < z.Bounds.Height) continue;
          if (RogueGame.HALF_VIEW_WIDTH <= m_Actor.Location.Position.X-z.Bounds.Left) continue;
          if (RogueGame.HALF_VIEW_WIDTH < z.Bounds.Right - m_Actor.Location.Position.X) continue;
          if (RogueGame.HALF_VIEW_HEIGHT <= m_Actor.Location.Position.Y-z.Bounds.Top) continue;
          if (RogueGame.HALF_VIEW_HEIGHT < z.Bounds.Bottom - m_Actor.Location.Position.Y) continue;
          if (scan_this.Width > z.Bounds.Width || scan_this.Height > z.Bounds.Height) scan_this = z.Bounds;
        }
        ret = new ZoneLoc(m_Actor.Location.Map,scan_this);
        _sparse.Set(SparseData.ClearingZone,ret);
        return ret;
      };
      return null;
    }
#endregion

    public List<Location> WantToGoHere(Location loc) {
      List<Location> ret = PathToTarget.WantToGoHere(GetMinStepPath<Location>(), loc);
      if (null != ret) return ret;
      var path = GetLambdaPath();
      if (null != path) path.WantToGoHere(in loc,this);
      if (loc.Map == m_Actor.Location.Map) {
         ret = PathToTarget.WantToGoHere(GetMinStepPath<Point>(), loc);
         if (null != ret) return ret;
         if (PlannedMoves.TryGetValue(loc.Position, out var src) && null!=src && 0 < src.Count) {
            ret = new List<Location>(src.Count);
            foreach(var x in src) ret.Add(new Location(m_Actor.Location.Map,x.Key));
            return ret;
         }
      }
      if (null != _last_move) {
        // educated guess
        var domain = new Location?[8];
        var ok = new HashSet<Location>();
        Span<bool> can_enter = stackalloc bool[8];
        int origin = -1;
        foreach(var dir in Direction.COMPASS) {
          var test = _last_move.dest + dir;
          if (!m_Actor.CanEnter(ref test)) continue;
          can_enter[dir.Index] = true;
          if (test == _last_move.origin) origin = dir.Index;
          else domain[dir.Index]=test;
        }
        if (!can_enter[(int)Compass.XCOMlike.N] && !can_enter[(int)Compass.XCOMlike.S]) {
            // traveling EW or WE.  Use the C enumeration as a testing
            Location? test;
            if ((int)Compass.XCOMlike.S > origin) {
                // EW: west destinations likely not-bad
                if (null!=(test = domain[(int)Compass.XCOMlike.SW])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.W])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.NW])) ok.Add(test.Value);
            } else {
                // WE: east destinations likely not-bad
                if (null!=(test = domain[(int)Compass.XCOMlike.NE])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.E])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.SE])) ok.Add(test.Value);
            }
        }
        if (!can_enter[(int)Compass.XCOMlike.E] && !can_enter[(int)Compass.XCOMlike.W]) {
            // traveling NS or SN
            Location? test;
            if ((int)Compass.XCOMlike.E > origin || (int)Compass.XCOMlike.W < origin) {
                // NS: south destinations likely not-bad
                if (null!=(test = domain[(int)Compass.XCOMlike.SE])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.S])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.SW])) ok.Add(test.Value);
            } else {
                // SN: north destinations likely not-bad
                if (null!=(test = domain[(int)Compass.XCOMlike.NW])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.N])) ok.Add(test.Value);
                if (null!=(test = domain[(int)Compass.XCOMlike.NE])) ok.Add(test.Value);
            }
        }
        if (0 < ok.Count) return ok.ToList();
      }
      return null;
    }

    /// <remark>This executes before the main lambda pathing block, so its use of the lambda pathing cache does not conflict with that.</remark>
    protected ActorAction BehaviorResupply(HashSet<GameItems.IDs> critical)
    {
      var act = UsePreexistingLambdaPath();
      if (null != act) return act;
      var update_path = ForceLambdaPath();
      HashSet<Point> inv_dests(Map m) {
        if (Session.Get.HasZombiesInSewers && CombatUnready() && m == m.District.SewersMap) return null;
        var ret = WhereIs(critical, m);
        if (null == ret || 0 >= ret.Count) return null;
        update_path.StageInventory(m,ret);
        return ret;
      }
      _caller = CallChain.SelectAction_LambdaPath;
#if CPU_HOG
      var test = m_Actor.CastToInventoryAccessibleDestinations(m_Actor.Location.Map, inv_dests(m_Actor.Location.Map));
      if (null != test && test.Any(pt => pt == m_Actor.Location.Position)) {
        var accessible = m_Actor.Location.Map.GetAccessibleInventories(m_Actor.Location.Position);
        if (null == accessible) throw new InvalidOperationException("self-pathing inventory accessible destination, isn't");
        else {
            foreach(var inv in accessible) {
                if (null != (m_Actor.Controller as OrderableAI).WouldGrabFromAccessibleStack(new Location(m_Actor.Location.Map, inv.Key), inv.Value)) {
                     throw new InvalidOperationException("usable inventory ignored");
                }
            }
        }
        throw new InvalidOperationException("self-pathing?");
      }
#endif
      act = BehaviorPathTo(m => m_Actor.CastToInventoryAccessibleDestinations(m,inv_dests(m)));
      _caller = CallChain.NONE;
      return act;
    }

    [System.Flags]
    public enum ReactionCode : uint {
      NONE = 0,
      ENEMY = uint.MaxValue/2+1,
      ITEM = ENEMY/2,
      TRADE = ITEM/2
    };

    // XXX return-code so we know what kind of heuristics are dominating.  Should be an enumeration or bitflag return
    // Should not need to be an override of a reduced-functionality BaseAI version
    public ReactionCode InterruptLongActivity()
    {
        ReactionCode ret = ReactionCode.NONE;
        if (null != enemies_in_FOV) ret |= ReactionCode.ENEMY;
        // we should also interrupt if there is a useful item in sight (this can happen with an enemy in sight)
        // (requires items in view cache from LOSSensor, which is wasted RAM for Z; living-specific cache in savefile indicated)
        // note that due to critical ai issues, soldiers and CHAR guards do not trade or intentionally seek out ground inventories.
        // Active research into removing this debilitating effect of the no-eating conversion serum was being conducted when the z-apocalypse hit.
        if (m_Actor.Model.Abilities.CanTrade) {
          var items = items_in_FOV;
          if (null != items) {
            foreach(var x in items) {
             if (x.Value.IsEmpty) continue;
             if (m_Actor.StackIsBlocked(x.Key)) continue; // XXX ignore items under barricades or fortifications
             var inv = x.Key.Items;
             if (null!=inv && !inv.IsEmpty && (BehaviorWouldGrabFromStack(x.Key, inv)?.IsLegal() ?? false)) {    // items seen cache can be obsolete
               ret |= ReactionCode.ITEM;
               break;
             }
            }
          }
          // \todo we should also interrupt if there is a valid trading apportunity in sight (this is suppressed by an enemy in sight)
          if (HaveTradingTargets()) ret |= ReactionCode.TRADE;
        }
        return ret;
    }

    public bool IsDistracted(ReactionCode Priority) {
      switch(Priority)
      {
      case ReactionCode.NONE:   // would be noticed if paying full attention to environment
        if (ReactionCode.NONE != InterruptLongActivity()) return true;
        break;
      case ReactionCode.ENEMY:  // direct communication from leader, or other credible survival mutual advantage
        if (null != enemies_in_FOV) return true;
        break;
      default: throw new InvalidProgramException("Unsupported priority of event");  // need to specify what happens
      }
      // \todo check for objectives and/or legacy orders that override
      return false;
    }

    // rethinking aggression.  Would have to lift this to handle feral dogs barking back/calling for help
    /// <returns>message to say</returns>
    public virtual string AggressedBy(Actor aggressor)
    {
      return "BASTARD! TRAITOR!";
    }

#nullable enable
    protected ActorAction? BehaviorFleeExplosives()
    {
      if (!(_blast_field?.Contains(m_Actor.Location.Position) ?? false)) return null;
      ActorAction? ret = (_safe_run_retreat ? DecideMove(_legal_steps, _run_retreat) : ((null != _retreat) ? DecideMove(_retreat) : null));
      if (null != ret) {
        if (ret is ActionMoveStep) m_Actor.Run();
        m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
      }
      return ret;
    }

    private ActorAction? BehaviorMeleeSnipe(Actor en, Attack tmp_attack, bool one_on_one)
    {
      if (en.HitPoints>tmp_attack.DamageValue/2) return null;
      ActorAction? tmpAction;
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
#nullable restore

    protected void ETAToKill(Actor en, int dist, ItemRangedWeapon rw, Dictionary<Actor, int> best_weapon_ETAs, Dictionary<Actor, ItemRangedWeapon> best_weapons=null)
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

#nullable enable
    protected ActorAction? ScanForMeleeSnipe()
    {
#if DEBUG
      if (null == _enemies) throw new ArgumentNullException(nameof(_enemies));
#endif
      if (1 < Rules.InteractionDistance(_enemies[0].Location, m_Actor.Location)) return null;
      // something adjacent...check for one-shotting
      var tmp_melee = m_Actor.GetBestMeleeWeapon();
      if (null != tmp_melee) {
        foreach (var p in _enemies) {
          if (!Rules.IsAdjacent(p.Location, m_Actor.Location)) return null;
          Actor en = p.Percepted;
          var act = BehaviorMeleeSnipe(en, m_Actor.MeleeWeaponAttack(tmp_melee.Model, en), null == _immediate_threat || (1 == _immediate_threat.Count && _immediate_threat.Contains(en)));
          if (null != act) {
            if (!tmp_melee.IsEquipped) tmp_melee.EquippedBy(m_Actor);
            return act;
          }
        }
      } else { // also check for no-weapon one-shotting
        foreach (var p in _enemies) {
          if (!Rules.IsAdjacent(p.Location, m_Actor.Location)) return null;
          Actor en = p.Percepted;
          var act = BehaviorMeleeSnipe(en, m_Actor.UnarmedMeleeAttack(en), null == _immediate_threat || (1 == _immediate_threat.Count && _immediate_threat.Contains(en)));
          if (null != act) {
            if (0 < m_Actor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MARTIAL_ARTS)) m_Actor.GetEquippedWeapon()?.UnequippedBy(m_Actor);
            return act;
          }
        }
      }
      return null;
    }
#nullable restore

    // forked from OrderableAI::BehaviorEquipWeapon
    private ActorAction? AttackWithoutMoving()
    {
      if (null == _enemies) return null;    // XXX likely error condition

      // migrated from CivilianAI::SelectAction
      var tmpAction = ScanForMeleeSnipe();
      if (null != tmpAction) return tmpAction;

      if (   !(m_Actor.GetEquippedWeapon() is ItemRangedWeapon rw)  // XXX likely error condition
          ||   0 >= rw.Ammo)  // XXX likely error condition
        return null;

      // at this point, null != enemies, we have a ranged weapon available, and melee one-shot is not feasible
      // also, damage field should be non-null because enemies is non-null

      var en_in_range = FilterFireTargets(_enemies,rw.Model.Attack.Range);
      if (null == en_in_range) return null; // XXX likely error condition
      if (1 == en_in_range.Count) return BehaviorRangedAttack(en_in_range[0].Percepted);

      // filter immediate threat by being in range
      Zaimoni.Data.Stack<Actor> immediate_threat_in_range = default;
      if (null != _immediate_threat) {
        immediate_threat_in_range = en_in_range.Select(p => p.Percepted).ToZStack(a => _immediate_threat.Contains(a));
      }

      if (1 == immediate_threat_in_range.Count) return BehaviorRangedAttack(immediate_threat_in_range[0]);

      // Get ETA stats
      var best_weapon_ETAs = new Dictionary<Actor,int>();
      foreach(var p in en_in_range) {
        ETAToKill(p.Percepted, Rules.InteractionDistance(m_Actor.Location,p.Location), rw, best_weapon_ETAs);
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
           int dist_min = immediate_threat_in_range.Min(a => Rules.InteractionDistance(m_Actor.Location, a.Location));
           immediate_threat_in_range.SelfFilter(a => Rules.InteractionDistance(m_Actor.Location, a.Location) == dist_min);
          }
        }
        return BehaviorRangedAttack(immediate_threat_in_range[0]);
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
          return BehaviorRangedAttack(en_in_range.First().Percepted);
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
        return BehaviorRangedAttack(en_in_range.First().Percepted);
      }
    }

    private ActorAction? WaitIfSafe()
    {
      return (_damage_field?.ContainsKey(m_Actor.Location.Position) ?? false) ? null : new ActionWait(m_Actor);
    }

    public ActionSwitchPowerGenerator? TurnOnAdjacentGenerators()
    {
      var generators_off = GeneratorsToTurnOn(m_Actor.Location.Map);
      if (null != generators_off) {
        foreach(Engine.MapObjects.PowerGenerator gen in generators_off) {   // these are never on map edges
          // \todo release block: do not allow flashlights to fully discharge; recharge at batteries 8- instead if at least one generator is on
          if (Rules.IsAdjacent(m_Actor.Location.Position,gen.Location.Position)) {
            return new ActionSwitchPowerGenerator(m_Actor, gen);    // VAPORWARE non-CHAR generators might have material legality checks e.g. needing fuel
          }
        }
      }
      return null;
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

    public bool WantToRecharge() { return m_Actor.Inventory.Has<ItemLight>(it => WantToRecharge(it)); }

#nullable enable
    public ActionRechargeItemBattery? RechargeWithAdjacentGenerator()
    {
      var recharge_these = m_Actor.Inventory.GetItemsByType<ItemLight>(it => WantToRecharge(it));
      if (null == recharge_these) return null;
      foreach(var gen in m_Actor.Location.Map.PowerGenerators.Get) {
        // design decision to not turn on here
        if (gen.IsOn && Rules.IsAdjacent(m_Actor.Location, gen.Location)) {
          var recharge = recharge_these[0];
          recharge.EquippedBy(m_Actor);
          return new ActionRechargeItemBattery(m_Actor, recharge);
        }
      }
      return null;
    }
#nullable restore

    private void AvoidBeingCornered()
    {
#if DEBUG
      if (null == _retreat) throw new ArgumentNullException(nameof(_retreat));
#endif
      if (2 > _retreat.Count) return;

      var cornered = new HashSet<Point>(_retreat);
      foreach(Point pt in Enumerable.Range(0,16).Select(i=>m_Actor.Location.Position.RadarSweep(2,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(in pt,in pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count< _retreat.Count) _retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    private void AvoidBeingRunCornered()
    {
#if DEBUG
      if (null == _run_retreat) throw new ArgumentNullException(nameof(_run_retreat));
#endif
      if (2 > _run_retreat.Count) return;

      var cornered = new HashSet<Point>(_run_retreat);
      foreach(Point pt in Enumerable.Range(0,24).Select(i=>m_Actor.Location.Position.RadarSweep(3,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(in pt,in pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count< _run_retreat.Count) _run_retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    public bool RunIfAdvisable(Location dest)
    {
      if (!m_Actor.CanRun()) return false;
      // we don't want preparing to push a car to block running at full stamina
      if (m_Actor.MaxSTA > m_Actor.StaminaPoints) {
        if (m_Actor.WillTireAfter(STA_reserve + m_Actor.RunningStaminaCost(in dest))) return false;
        if (m_Actor.NextMoveLostWithoutRunOrWait) return true;
        int double_run_STA = 2 * m_Actor.RunningStaminaCost(in dest);
        if (m_Actor.WillTireAfter(STA_reserve + double_run_STA)) {
          if (m_Actor.WillTireAfter(STA_reserve + double_run_STA - Actor.STAMINA_REGEN_WAIT)) return false;
          return 1 >= m_Actor.MoveLost(1, 1, 1);
        }
      }
      return true;
    }

    protected void ReserveSTA(int jump, int melee, int push, int push_weight)   // currently jump and break have the same cost
    {
      int tmp = push_weight;
      tmp += jump*Rules.STAMINA_COST_JUMP;
      tmp += melee*(Rules.STAMINA_COST_MELEE_ATTACK+m_Actor.BestMeleeAttack().StaminaPenalty);

      _STA_reserve = tmp+m_Actor.NightSTApenalty*(jump+melee+push);
    }

    // these two return a value copy for correctness
    protected Dictionary<Point, int> PlanApproach(Zaimoni.Data.FloodfillPathfinder<Point> navigate)
    {
      PlannedMoves.Clear();
      Dictionary<Point, int> dest = navigate.Approach(m_Actor.Location.Position);
      if (null == dest) return dest;
      PlannedMoves[m_Actor.Location.Position] = dest;
      foreach(Point pt in dest.Keys) {
        if (0>navigate.Cost(pt)) continue;
        PlannedMoves[pt] = navigate.Approach(pt);
      }
      return new Dictionary<Point,int>(PlannedMoves[m_Actor.Location.Position]);
    }

    private static Dictionary<Point, int> DowngradeApproach(Dictionary<Location,int> src)
    {
      var ret = new Dictionary<Point,int>();
      foreach(var x in src) {
        Location? test = x.Key.Map.Denormalize(x.Key);
        if (null == test) continue;
        ret[test.Value.Position] = x.Value;
      }
      return ret;
    }

    protected Dictionary<Location, int> PlanApproach(Zaimoni.Data.FloodfillPathfinder<Location> navigate)
    {
      PlannedMoves.Clear();
      var dest = navigate.Approach(m_Actor.Location);
      if (0 < dest.Count) {
        var approach = DowngradeApproach(dest);
        if (0<approach.Count) PlannedMoves[m_Actor.Location.Position] = approach;
        foreach(var x in dest) {
          Location? test = x.Key.Map.Denormalize(x.Key);
          if (null == test) continue;
          var test2 = navigate.Approach(x.Key);
          if (null != test2) {
            approach = DowngradeApproach(test2);
            if (0<approach.Count) PlannedMoves[test.Value.Position] = approach;
          }
        }
      }
      return dest;
    }

    protected void ClearMovePlan() { PlannedMoves.Clear(); }

#nullable restore
    private List<Point>? FindRetreat()
    {
#if DEBUG
      if (null == _damage_field) throw new ArgumentNullException(nameof(_damage_field));
      if (null == _legal_steps) throw new ArgumentNullException(nameof(_legal_steps));
      if (!_damage_field.ContainsKey(m_Actor.Location.Position)) throw new InvalidOperationException("!damage_field.ContainsKey(m_Actor.Location.Position)");
#endif
      IEnumerable<Point> tmp_point = _legal_steps.Where(pt=>!_damage_field.ContainsKey(pt));
      if (tmp_point.Any()) return tmp_point.ToList();
      tmp_point = _legal_steps.Where(p=> _damage_field[p] < _damage_field[m_Actor.Location.Position]);
      return (tmp_point.Any() ? tmp_point.ToList() : null);
    }

    private List<Point>? FindRunRetreat()
    {
#if DEBUG
      if (null == _damage_field) throw new ArgumentNullException(nameof(_damage_field));
      if (null == _legal_steps) throw new ArgumentNullException(nameof(_legal_steps));
      if (!_damage_field.ContainsKey(m_Actor.Location.Position)) throw new InvalidOperationException("!damage_field.ContainsKey(m_Actor.Location.Position)");
#endif
      var ret = new HashSet<Point>(Enumerable.Range(0, 16).Select(i => m_Actor.Location.Position.RadarSweep(2, i)).Where(pt => m_Actor.Location.Map.IsWalkableFor(pt, m_Actor) && !m_Actor.Location.Map.HasActorAt(in pt)));
      ret.RemoveWhere(pt => !_legal_steps.Any(pt2 => Rules.IsAdjacent(in pt,in pt2)));

      IEnumerable<Point> tmp_point = ret.Where(pt=>!_damage_field.ContainsKey(pt));
      if (tmp_point.Any()) return tmp_point.ToList();
      tmp_point = ret.Where(pt=> _damage_field[pt] < _damage_field[m_Actor.Location.Position]);
      return (tmp_point.Any() ? tmp_point.ToList() : null);
    }
#nullable restore

#region DecideMove
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
        Location? test = m_Actor.Location.Map.Denormalize(in loc);
        if (null != test && avoid.Contains(test.Value.Position)) continue;  // null is expected for using a same-district exit
        ok.Add(loc);
      }
	  int new_dest = ok.Count;
      return ((0 < new_dest && new_dest < src.Count) ? ok : src);
    }

    private Dictionary<Location, T> DecideMove_Avoid<T>(Dictionary<Location,T> src, IEnumerable<Point> avoid)
    {
      if (null == avoid) return src;
      var ok = new Dictionary<Location, T>();
      foreach(var loc in src) {
        Location? test = m_Actor.Location.Map.Denormalize(loc.Key);
        if (null != test && avoid.Contains(test.Value.Position)) continue;  // null is expected for using a same-district exit
        ok.Add(loc.Key,loc.Value);
      }
	  int new_dest = ok.Count;
      return ((0 < new_dest && new_dest < src.Count) ? ok : src);
    }


    private List<Point> DecideMove_NoJump(List<Point> src)
    {
      IEnumerable<Point> no_jump = src.Where(pt=> {
        var tmp2 = m_Actor.Location.Map.GetMapObjectAt(pt);
          return !tmp2?.IsJumpable ?? true;
      });
	  int new_dest = no_jump.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_jump.ToList() : src);
    }

    private static List<Location> DecideMove_NoJump(List<Location> src)
    {
      IEnumerable<Location> no_jump = src.Where(Location.NoJump);
	  int new_dest = no_jump.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_jump.ToList() : src);
    }

    private static void DecideMove_NoJump<T>(Dictionary<Location,T> src)
    {
      if (src.NontrivialFilter(Location.NoJump)) src.OnlyIf(Location.NoJump);
    }

    private List<Point> DecideMove_LongPath(List<Point> src)
    {
      var plan = PlanWalkAwayFrom(enemies_in_FOV,src);
      if (null != plan) {
        var long_path = new List<Point>();
        foreach(var x in plan) {
          var loc = m_Actor.Location.Map.Denormalize(x.Key);
          if (null != loc && src.Contains(loc.Value.Position)) long_path.Add(loc.Value.Position);
        }
        if (0 < long_path.Count) return long_path;
      }
      return src;
    }

    private static List<T> DecideMove_NoShove<T>(List<T> src, Dictionary<T, ActorAction> legal_steps)
    {
      IEnumerable<T> no_shove = src.Where(loc=> !(legal_steps[loc] is ActionShove));
	  int new_dest = no_shove.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_shove.ToList() : src);
    }

    private static void DecideMove_NoShove<T>(Dictionary<T,ActorAction> src)
    {
      if (src.NontrivialFilter(ActorAction.IsNot<ActionShove,T>)) src.OnlyIf(ActorAction.IsNot<ActionShove>);
    }

    private static List<T> DecideMove_NoPush<T>(List<T> src, Dictionary<T, ActorAction> legal_steps)
    {
      IEnumerable<T> no_push = src.Where(loc => !(legal_steps[loc] is ActionPush));
	  int new_dest = no_push.Count();
      return ((0 < new_dest && new_dest < src.Count) ? no_push.ToList() : src);
    }

    private static void DecideMove_NoPush<T>(Dictionary<T, ActorAction> src)
    {
      if (src.NontrivialFilter(ActorAction.IsNot<ActionShove,T>)) src.OnlyIf(ActorAction.IsNot<ActionShove>);
    }

    static private List<Point> DecideMove_maximize_visibility(List<Point> dests, HashSet<Point> tainted, HashSet<Point> new_los, Dictionary<Point,HashSet<Point>> hypothetical_los) {
        tainted.IntersectWith(new_los);
        if (0>=tainted.Count) return dests;
        var taint_exposed = new Dictionary<Point,int>();
        foreach(Point pt in dests) {
          if (!hypothetical_los.TryGetValue(pt,out var src)) {
            taint_exposed[pt] = 0;
            continue;
          }
          HashSet<Point> tmp2 = new HashSet<Point>(src);
          tmp2.IntersectWith(tainted);
          taint_exposed[pt] = tmp2.Count;
        }
        taint_exposed.OnlyIfMaximal();
        return taint_exposed.Keys.ToList();
    }

    private List<Location> DecideMove_maximize_visibility(List<Location> dests, HashSet<Point> tainted, HashSet<Point> new_los, Dictionary<Point,HashSet<Point>> hypothetical_los) {
        tainted.IntersectWith(new_los);
        if (0>=tainted.Count) return dests;
        var taint_exposed = new Dictionary<Location,int>();
        foreach(Location loc in dests) {
          Location? test = m_Actor.Location.Map.Denormalize(in loc);
          if (null == test) {   // assume same-district exit use...don't really want to do this when other targets are close
            taint_exposed.Add(loc, 0);
            continue;
          }
          if (!hypothetical_los.TryGetValue(test.Value.Position,out var src)) {
            taint_exposed.Add(loc, 0);
            continue;
          }
          HashSet<Point> tmp2 = new HashSet<Point>(src);
          tmp2.IntersectWith(tainted);
          taint_exposed.Add(loc, tmp2.Count);
        }
        taint_exposed.OnlyIfMaximal();
        return taint_exposed.Keys.ToList();
    }

    private Dictionary<Location, T> DecideMove_maximize_visibility<T>(Dictionary<Location,T> dests, HashSet<Point> tainted, HashSet<Point> new_los, Dictionary<Point,HashSet<Point>> hypothetical_los) {
        tainted.IntersectWith(new_los);
        if (0>=tainted.Count) return dests;
        var taint_exposed = new Dictionary<Location,int>();
        foreach(var loc in dests) {
          var test = m_Actor.Location.Map.Denormalize(loc.Key);
          if (null == test) {   // assume same-district exit use...don't really want to do this when other targets are close
            taint_exposed[loc.Key] = 0;
            continue;
          }
          if (!hypothetical_los.TryGetValue(test.Value.Position,out var src)) {
            taint_exposed[loc.Key] = 0;
            continue;
          }
          HashSet<Point> tmp2 = new HashSet<Point>(src);
          tmp2.IntersectWith(tainted);
          taint_exposed[loc.Key] = tmp2.Count;
        }
        int max_taint_exposed = dests.Select(pt=>taint_exposed[pt.Key]).Max();
        dests.OnlyIf(loc=>taint_exposed[loc]==max_taint_exposed);
        return dests;
    }

    private ActorAction? _finalDecideMove(List<Point> tmp, Dictionary<Point,ActorAction> legal_steps)
    {
	  var secondary = new List<ActorAction>();
      bool prefer_cardinal(Point pt) {
        if (m_Actor.Location.Position.X == pt.X) return true;
        if (m_Actor.Location.Position.Y == pt.Y) return true;
        return false;
      }

      // since we are pathfinding to n destinations at once, we don't have the usual heuristics for the 1-destination case.
      // however, since the most natural-looking paths stay within the rhombus rather than the full min-step pathing rectangle,
      // we can prefer cardinal directions to diagonal directions safely at this point (all tactical considerations were supposed to have
      // been applied first)
      Actor? shove_target;
	  while(0<tmp.Count) {
		ActorAction ret = legal_steps[Rules.Get.DiceRoller.ChooseWithoutReplacement(tmp, prefer_cardinal)];
        if (ret is ActionShove shove && (shove_target = shove.Target).Controller is ObjectiveAI ai) {
           var ok_dests = ai.WantToGoHere(shove_target.Location);
           if (Rules.IsAdjacent(shove.a_dest, m_Actor.Location)) {
             // non-moving shove...would rather not spend the stamina if there is a better option
             if (null != ok_dests  && ok_dests.Contains(shove.a_dest)) secondary.Add(ret); // shove is to a wanted destination
             continue;
           }
           // discard action if the target is on an in-bounds exit (target is likely pathing through the chokepoint)
           // target should not be sleeping; check for that anyway
           if (null!= shove_target.Location.Exit && !shove_target.IsSleeping) continue;

           if (   null == ok_dests // shove is rude
               || !ok_dests.Contains(shove.a_dest)) // shove is not to a wanted destination
                {
                secondary.Add(ret);
                continue;
                }
        }
		return ret;
	  }
      return 0 < secondary.Count ? Rules.Get.DiceRoller.Choose(secondary) : null;
    }

	protected HashSet<Point> FriendsLoF()
	{
      var enemies = enemies_in_FOV;
	  if (null == enemies) return null;
      var friends = friends_in_FOV;
	  if (null == friends) return null;
	  var tmp = new HashSet<Point>();
      short range;
	  foreach(var f in friends) {
        if (!f.Value.HasEquipedRangedWeapon()) continue;
        range = f.Value.CurrentRangedAttack.Range;
        var f_loc = f.Value.Location;
	    foreach(var e in enemies) {
          var e_loc = e.Value.Location;
		  if (range < Rules.GridDistance(f_loc, e_loc)) continue;
		  List<Point> line = new List<Point>();
	      if (LOS.CanTraceViewLine(f_loc, e_loc, range, line)) tmp.UnionWith(line);
		}
	  }
	  return (0<tmp.Count ? tmp : null);
	}

    // this assumes conditions like "everything is in FOV" so that a floodfill pathfinding is not needed.
    // we also assume no enemies in sight.
    // XXX as a de-facto leaf function, we can get away with destructive modifications to goals
    public ActorAction BehaviorEfficientlyHeadFor(Dictionary<Point,int> goals)
    {
      if (0>=goals.Count) return null;
      if (null == _legal_steps) return null;

#if DEBUG
      var cardinal = new Dictionary<Compass.XCOMlike, HashSet<Point>>();
#if PROTOTYPE
      var staging = new Dictionary<Compass.XCOMlike, HashSet<Point>>();

      void install(Compass.XCOMlike dir, Point pt) {
        if (staging.TryGetValue(dir, out var cache)) cache.Add(pt);
        else staging.Add(dir, new HashSet<Point> {pt});
      }
#endif

      void primary_install(Compass.XCOMlike dir, Point pt) {
        if (cardinal.TryGetValue(dir, out var cache)) cache.Add(pt);
        else cardinal.Add(dir, new HashSet<Point> {pt});
#if PROTOTYPE
        install(dir, pt);
#endif
      }

      static Compass.XCOMlike routing_code(Point delta) {
        if (0 == delta.X) {
          return 0 < delta.Y ? Compass.XCOMlike.S : Compass.XCOMlike.N;
        } else if (0 == delta.Y) {
          return 0 < delta.X ? Compass.XCOMlike.E : Compass.XCOMlike.W;  // E, W
        } else if (0 < delta.X) {
          if (0 < delta.Y) {
            if (delta.X==delta.Y) return Compass.XCOMlike.SE;
            else return (delta.X < delta.Y) ? Compass.XCOMlike.S : Compass.XCOMlike.E;
          } else {
            if (delta.X==-delta.Y) return Compass.XCOMlike.SW;
            else return (delta.X < -delta.Y) ? Compass.XCOMlike.S : Compass.XCOMlike.W;
          }
        } else {
          if (0 < delta.Y) {
            if (delta.X==-delta.Y) return Compass.XCOMlike.NE;
            else return (-delta.X < delta.Y) ? Compass.XCOMlike.N : Compass.XCOMlike.E;
          } else {
            if (delta.X==delta.Y) return Compass.XCOMlike.NW;
            else return (delta.X < delta.Y) ? Compass.XCOMlike.W : Compass.XCOMlike.N;
          }
        }
      }

      // classify
      foreach(var x in goals) {
        var delta = x.Key - m_Actor.Location.Position;
        var code = routing_code(delta);
        primary_install(code, x.Key);
#if PROTOTYPE
        switch(routing_code(delta))
        {
        case Compass.XCOMlike.NE:
          install(Compass.XCOMlike.N, x.Key);
          install(Compass.XCOMlike.E, x.Key);
          break;
        case Compass.XCOMlike.NW:
          install(Compass.XCOMlike.N, x.Key);
          install(Compass.XCOMlike.W, x.Key);
          break;
        case Compass.XCOMlike.W:
          install(Compass.XCOMlike.NW, x.Key);
          install(Compass.XCOMlike.SW, x.Key);
          break;
        case Compass.XCOMlike.N:
          install(Compass.XCOMlike.NE, x.Key);
          install(Compass.XCOMlike.NW, x.Key);
          break;
        case Compass.XCOMlike.S:
          install(Compass.XCOMlike.SE, x.Key);
          install(Compass.XCOMlike.SW, x.Key);
          break;
        case Compass.XCOMlike.E:
          install(Compass.XCOMlike.NE, x.Key);
          install(Compass.XCOMlike.SE, x.Key);
          break;
        case Compass.XCOMlike.SE:
          install(Compass.XCOMlike.S, x.Key);
          install(Compass.XCOMlike.E, x.Key);
          break;
        case Compass.XCOMlike.SW:
          install(Compass.XCOMlike.S, x.Key);
          install(Compass.XCOMlike.W, x.Key);
          break;
        default: throw new InvalidProgramException("unexpected routing code");
        }
#endif
      }

#if PROTOTYPE
      // Veto
      var keep_dirs = new Zaimoni.Data.Stack<Compass.XCOMlike>(stackalloc Compass.XCOMlike[(int)Compass.reference.XCOM_STRICT_UB]);
      foreach(var pt in _legal_steps) {
        var dir = Direction.FromVector(pt-m_Actor.Location.Position);
        if (null != dir) keep_dirs.push((Compass.XCOMlike)(dir.Index));
      }
      var flush_dirs = new Zaimoni.Data.Stack<Compass.XCOMlike>(stackalloc Compass.XCOMlike[(int)Compass.reference.XCOM_STRICT_UB]);
      var ok_steps = new List<Point>();
      foreach(var x in staging) {
        if (!keep_dirs.Contains(x.Key)) flush_dirs.push(x.Key);
        else ok_steps.Add(m_Actor.Location.Position + Direction.COMPASS[(int)x.Key]);
      }
      if (0 >= ok_steps.Count) return null;
      if (1 == ok_steps.Count) {
  	    var act = DecideMove(ok_steps);
        if (null != act) {
          if (act is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
          m_Actor.Activity = Activity.IDLE;
          return act;
        }
        return null;
      } 

      int i = flush_dirs.Count;
      while(0 <= --i) staging.Remove(flush_dirs[i]);
      if (0 >= staging.Count) return null;

      // ignore goals we cannot approach
      var working = new Zaimoni.Data.Stack<Point>(stackalloc Point[goals.Count]);
      foreach(var x in goals) {
        bool discard = true;
        foreach(var y in staging) {
          if (y.Value.Contains(x.Key)) {
            discard = false;
            break;
          }
        }
        if (discard) working.push(x.Key);
      }
      i = working.Count;
      while(0 <= --i) goals.Remove(working[i]);
      if (0 >= goals.Count) return null;

      // must be approaching a nearest goal
      if (1 < ok_steps.Count && 1 < goals.Count) {
        int min_dist = int.MaxValue;
        working.Clear();
        foreach(var x in goals) {
          if (x.Value > min_dist) continue;
          if (x.Value < min_dist) {
            min_dist = x.Value;
            working.Clear();
          }
          working.push(x.Key);
        }
        flush_dirs.Clear();
        foreach(var x in staging) {
          bool discard = true;
          i = working.Count;
          while(0 <= --i) if (x.Value.Contains(working[i])) {
            discard = false;
            break;
          }
          if (discard) flush_dirs.push(x.Key);
        }
        i = flush_dirs.Count;
        while(0 <= --i) staging.Remove(flush_dirs[i]);
        if (0 >= staging.Count) return null;
        ok_steps.Clear();
        foreach(var x in staging) ok_steps.Add(m_Actor.Location.Position + Direction.COMPASS[(int)x.Key]);
        if (1 == ok_steps.Count) {
    	  var act = DecideMove(ok_steps);
          if (null != act) {
            if (act is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
            m_Actor.Activity = Activity.IDLE;
            return act;
          }
          return null;
        } 
      }

#if DEBUG
      if (ok_steps.Count < _legal_steps.Count) throw new InvalidOperationException("tracing");
#endif
#endif
#endif

      // Following is not safe to call from within item evaluation -- stack overflow
      List<Point> legal_steps = (2 <= _legal_steps.Count) ? DecideMove_WaryOfTraps(_legal_steps) : _legal_steps;    // need working copy here
      if (2 <= legal_steps.Count) {
        int min_dist = int.MaxValue;
        // this breaks down if 2+ goals equidistant.
        {
        var near = new Zaimoni.Data.Stack<Point>(stackalloc Point[goals.Count]);
        foreach(var x in goals) {
          if (x.Value > min_dist) continue;
          if (x.Value < min_dist) {
            min_dist = x.Value;
            near.Clear();
          }
          near.push(x.Key);
        }
        int ub = near.Count;
        if (1 < ub) {
          var ok = Rules.Get.Roll(0, ub);
          while (0 <= --ub) if (ok != ub) goals.Remove(near[ub]);
        }
        }
        // exactly one minimum-cost goal now
        int near_scale = goals.Count+1;
        var efficiency = new Dictionary<Point,int>();
        foreach(Point pt in legal_steps) {
          efficiency[pt] = 0;
          foreach(var pt_delta in goals) {
            // relies on FOV not being "too large"
            int delta = pt_delta.Value-Rules.GridDistance(in pt, pt_delta.Key);
            efficiency[pt] += (min_dist == pt_delta.Value ? near_scale * delta : delta);
          }
        }
        efficiency.OnlyIfMaximal();
        legal_steps = new List<Point>(efficiency.Keys);
      }

#if DEBUG
      IEnumerable<Point> only_cardinal = legal_steps.Where(pt => cardinal.ContainsKey((Compass.XCOMlike)(Direction.FromVector(pt - m_Actor.Location.Position).Index)));
      if (only_cardinal.Any() && only_cardinal.Count()<legal_steps.Count) legal_steps = only_cardinal.ToList();
#endif

	  var tmpAction = DecideMove(legal_steps);
      if (null != tmpAction) {
        if (tmpAction is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest);
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      return null;
    }

    protected ActorAction? DecideMove(IEnumerable<Point> src)
	{
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif

      var legal_steps = m_Actor.OnePathPt(m_Actor.Location); // other half
      legal_steps.OnlyIf(action => action.IsPerformable() && !VetoAction(action));

	  List<Point> tmp = src.Where(pt => legal_steps.ContainsKey(pt)).ToList();
      if (0 >= tmp.Count) return null;
      if (1 >= tmp.Count) return _finalDecideMove(tmp, legal_steps);

	  // do not get in the way of allies' line of fire
	  tmp = DecideMove_Avoid(tmp, FriendsLoF());
      if (1 >= tmp.Count) return _finalDecideMove(tmp, legal_steps);

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  ThreatTracking threats = m_Actor.Threats;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
      Dictionary<Point,HashSet<Point>>? hypothetical_los = null;
      HashSet<Point>? new_los = null;

      if (null != threats || null != sights_to_see) {
        hypothetical_los = new Dictionary<Point, HashSet<Point>>();
        new_los = new HashSet<Point>();
	    // only need points newly in FOV that aren't currently
	    foreach(Point pt in tmp) {
	      hypothetical_los[pt] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt)).Except(FOV));
          new_los.UnionWith(hypothetical_los[pt]);
	    }
        // only need to check if new locations seen
        if (0 >= new_los.Count) {
          threats = null;
          sights_to_see = null;
        }
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position - (Point)tmp_LOSrange, (Point)(2*tmp_LOSrange+1));

      if (null != threats) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
        if (1 >= tmp.Count) return _finalDecideMove(tmp, legal_steps);
	  }
	  if (null != sights_to_see) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
        if (1 >= tmp.Count) return _finalDecideMove(tmp, legal_steps);
	  }

      // weakly prefer not to shove
      tmp = DecideMove_NoShove(tmp, legal_steps);

      // weakly prefer not to push
      tmp = DecideMove_NoPush(tmp, legal_steps);

      // weakly prefer not to jump
      tmp = DecideMove_NoJump(tmp);

      if (CallChain.ManageMeleeRisk == _caller) tmp = DecideMove_LongPath(tmp);
      return _finalDecideMove(tmp, legal_steps);
	}

#nullable enable
    private ActorAction? _finalDecideMove(List<Location> tmp, Dictionary<Location, ActorAction> legal_steps)
    {
	  var secondary = new List<ActorAction>();
      bool prefer_cardinal(Location loc) {
        if (m_Actor.Location.Position.X == loc.Position.X) return true;
        if (m_Actor.Location.Position.Y == loc.Position.Y) return true;
        return false;
      }
      Actor? shove_target;
	  while(0<tmp.Count) {
        var dest = Rules.Get.DiceRoller.ChooseWithoutReplacement(tmp, prefer_cardinal);
        var ret = legal_steps[dest];    // sole caller guarantees exists and is legal
        if (ret is ActionUseExit use_exit && use_exit.IsNotBlocked) continue;
        if (ret is ActionShove shove && (shove_target = shove.Target).Controller is ObjectiveAI ai) {
           var ok_dests = ai.WantToGoHere(shove_target.Location);
           if (Rules.IsAdjacent(shove.a_dest, m_Actor.Location)) {
             // non-moving shove...would rather not spend the stamina if there is a better option
             if (null != ok_dests  && ok_dests.Contains(shove.a_dest)) secondary.Add(ret); // shove is to a wanted destination
             continue;
           }
           // discard action if the target is on an in-bounds exit (target is likely pathing through the chokepoint)
           // target should not be sleeping; check for that anyway
           if (null!= shove_target.Location.Exit && !shove_target.IsSleeping) continue;

           if (   null == ok_dests // shove is rude
               || !ok_dests.Contains(shove.a_dest)) // shove is not to a wanted destination
                {
                secondary.Add(ret);
                continue;
                }
        }
		return ret;
	  }
      return 0 < secondary.Count ? Rules.Get.DiceRoller.Choose(secondary) : null;
    }

    private ActorAction? _finalDecideMove(Dictionary<Location,ActorAction>? src)
    {
      if (null==src || 0==src.Count) return null;
      if (1==src.Count) return src.First().Value;   // intentionally allow non-moving shove through to simulate panic
      return _finalDecideMove(src.Keys.ToList(), src);
    }
#nullable restore

    // \todo timing test: handle Resolvable before or after ActorDest?
    public bool VetoAction(ActorAction x)
    {
      if (x is ActorDest a_dest) {
        if (a_dest.dest.ChokepointIsContested(m_Actor)) return true; // XXX telepathy; we don't want to enter a chokepoint with someone else in it that could be heading our way
        if (1>=FastestTrapKill(a_dest.dest)) return true;   // death-trapped
#if PROTOTYPE
        if (m_Actor.Model.Abilities.AI_CanUseAIExits) {
          if (x is ActionUseExit) {
            var leader = m_Actor.LiveLeader;
            if (null != leader && leader.ClanIsDisrupted(2).Key) return true;    // civilian AI value; others don't use exits
          } else {
            var e = a_dest.dest.Exit;
            if (null != e) {
              if (null != NeedsAir(e.Location, m_Actor)) return true;    // too crowded?
              var leader = m_Actor.LiveLeader;
              if (null != leader && leader.ClanIsDisrupted(2).Key) return true;    // civilian AI value; others don't use exits
            }
          }
        }
#endif
#if USING_ESCAPE_MOVES
        var friends = friends_in_FOV;
        if (null != friends_in_FOV) {
          foreach(var fr in friends_in_FOV) {
            var escaping_to = (fr.Value.Controller as ObjectiveAI)?.WantToEscapeTo();   // seems hard to trigger
            if (escaping_to?.Contains(a_dest.dest) ?? false) return true;
          }
        }
#endif
      }
      if (x is Resolvable res) return VetoAction(res.ConcreteAction);   // resolvable actions should use the actual action to execute
      if (x is ActionCloseDoor close) {
        var door = close.Door;
        foreach(var pt in door.Location.Position.Adjacent()) {
          var actor = door.Location.Map.GetActorAtExt(pt);
          if (null == actor || m_Actor.IsEnemyOf(actor)) continue;
          if (actor.Controller is ObjectiveAI ai) {
            var tmp = ai.WantToGoHere(actor.Location);
            if (tmp?.Contains(door.Location) ?? false) return true;
          }
        }
      }

      if (x is ActionShove shove) {
        if (_blast_field?.Contains(shove.To) ?? false) return true;   // exceptionally hostile to shove into an explosion
        if (_damage_field?.ContainsKey(shove.To) ?? false) return true;   // hostile to shove into a damage field

        var target = shove.Target;
        if (target.Controller is ObjectiveAI ai) {
          if (Rules.IsAdjacent(shove.a_dest, m_Actor.Location)) {
            // non-moving shove...would rather not spend the stamina if there is a better option
            var ok_dests = ai.WantToGoHere(target.Location);
            if (null != ok_dests) return !ok_dests.Contains(shove.a_dest); // shove is to a wanted destination
          }
          // discard action if the target is on an in-bounds exit (target is likely pathing through the chokepoint)
          // target should not be sleeping; check for that anyway
          if (null != target.Location.Exit && !target.IsSleeping) return true;
        }
        // cf OrderableAI::RejectSwitchPlaces
        if (!target.IsSleeping && target.Controller is OrderableAI oai) {
          var trace = oai.WouldUseAccessibleStack(target.Location);
          if (null != trace) return true;
        }
      }
      if (x is ActionUseExit exit && !exit.IsNotBlocked) return true;

      return false;
    }

#nullable enable
    static public bool ActorsNearby(Location loc, Predicate<Actor> test)
    {
      var e = loc.Exit;
      if (null != e) {
        var a = e.Location.Actor;
        if (null != a && test(a)) return true;
      }
      foreach(var pt in loc.Position.Adjacent()) {
        var loc2 = new Location(loc.Map, pt);
        if (!Map.Canonical(ref loc2)) continue;
        var a = loc2.Actor;
        if (null != a && test(a)) return true;
        // \todo set up range-2 processing
      }
      return false;
    }

    static public int Air(Location test, Actor viewpoint) {
      int ret = 0;
      foreach(var pt in test.Position.Adjacent()) {
        var loc = new Location(test.Map, pt);
        if (!Map.Canonical(ref loc)) continue;
        if (Map.Canonical(ref loc) && loc.IsWalkableFor(viewpoint)) ret++;
      }
      return ret;
    }

    static public Actor? NeedsAir(Location test, Actor viewpoint) {
      foreach(var pt in test.Position.Adjacent()) {
        var loc = new Location(test.Map, pt);
        if (!Map.Canonical(ref loc) || loc==viewpoint.Location) continue;
        var a = loc.Actor;
        if (null == a) continue;
        var air = Air(loc, a);    // for debugging convenience
        if (1 >= air) return a;
      }
      return null;
    }

    static public bool VetoExit(Actor a, Exit? e) {
      if (a.Model.Abilities.AI_CanUseAIExits && null != e) {
        if (null != Gameplay.AI.ObjectiveAI.NeedsAir(e.Location, a)) return true;    // too crowded?
#if PROTOTYPE
        var leader = actor.LiveLeader;
        if (null != leader && leader.ClanIsDisrupted(2).Key) return true;    // civilian AI value; others don't use exits
#endif
      }
      return false;
    }

    public static bool NoContestedExit(ActorDest a_dest) {
      var e = a_dest.dest.Exit;
      return null == e || !ActorsNearby(e.Location, a => !a.IsSleeping);
    }

    protected ActorAction? BehaviorMakeTime() {
      if (null == _legal_path) return null;
      var tolerable_moves = _legal_path.CloneCast<Location,ActorAction,ActorDest>(step => null == NeedsAir(step.dest, m_Actor));
      // if very crowded, relax standards
      if (0 >= tolerable_moves.Count) tolerable_moves = _legal_path.CloneCast<Location,ActorAction,ActorDest>();
      if (0 >= tolerable_moves.Count) return null;
      if (1 == tolerable_moves.Count) return (ActorAction)tolerable_moves.First().Value;
      var best_moves = tolerable_moves.CloneCast<Location, ActorDest, ActionMoveStep>(NoContestedExit);
      if (1 <= best_moves.Count) return Rules.Get.DiceRoller.Choose(best_moves).Value;
      var rude_moves = tolerable_moves.CloneOnly(NoContestedExit);
      if (1 <= rude_moves.Count) return (ActorAction)Rules.Get.DiceRoller.Choose(rude_moves).Value;
      best_moves = tolerable_moves.CloneCast<Location, ActorDest, ActionMoveStep>();
      if (1 <= best_moves.Count) return Rules.Get.DiceRoller.Choose(best_moves).Value;
      /* if (1 <= tolerable_moves.Count) */ return (ActorAction)Rules.Get.DiceRoller.Choose(tolerable_moves).Value;
    }

    public ActorAction? RewriteAction(ActorAction x)
    {
      if (x is CombatAction) return null;   // do not second-guess combat actions
      if (x is ActionTradeWith) return null;
      if (x is ActorGive) return null;
      if (x is ActorTake) return null;
      if (x is ActionTrade) return null;
      if (x is ActionPutInContainer) return null;
      if (x is ActionSequence) return null;
      if (x is ActionGiveTo) return null;
      if (x is ActionTake) return null;
      if (x is ActionUseItem) return null;
      if (x is ActionUse) return null;
      if (x is ActionSprayOdorSuppressor) return null;
      if (x is ActionTakeLead) return null;
      if (x is ActionBreak) return null;
      if (x is ActionSay) return null;
      if (x is ActionSwitchPlace) return null;
      if (x is ActionSwitchPlaceEmergency) return null;

      // exit-related processing.
      var e = m_Actor.Location.Exit;
      if (x is ActorDest a_dest) {
        if (null == e) {
          var dest_e = a_dest.dest.Exit;
            // crowd control
          var gasping = NeedsAir(a_dest.dest, m_Actor);
          if (null != gasping) {
            if (x is ActionOpenDoor) return null;
            var act = BehaviorMakeTime();
            if (null != act) return act;
          }
        }
        if (_staged_action is ActionCloseDoor close && a_dest.dest == close.Door.Location) _staged_action = null;   // 2020-03-29: do not self-block
      } else if (ActorsNearby(m_Actor.Location, a => !a.IsSleeping)) {
        if (null == e) {    // don't dawdle on the exit itself
          if (x is ActionOpenDoor) return null;
          var act = BehaviorMakeTime();
          if (null != act) return act;
        }
      }
      // clear staged actions here
      if (null != _staged_action) {
        if (_staged_action.IsPerformable() && !VetoAction(_staged_action)) {
          if (_staged_action is ActionCloseDoor close && x is ActionCloseDoor o_close && close.Door.Location==o_close.Door.Location) {
            // double close.  Use ours (it's free)
            _staged_action = null;
            return close;
          }
          _staged_action.Perform();
        }
        _staged_action = null;
      }
      return null;
    }
#nullable restore

    public void ScheduleFollowup(ActorAction x)
    {
      // recursions
      if (x is Resolvable resolve) { ScheduleFollowup(resolve.ConcreteAction); return; }
      // inline ClearGoals body here -- this is called at the right place
      if (x is ActorDest && !_used_advanced_pathing) _sparse.Unset(SparseData.PathingTo);
      // proper handling
      if (x is ActionMoveStep) {
        // Historically, CivilianAI has the behavior of closing doors behind them.  The other three OrderableAI classes don't do this
        // refine the historical behavior to not happen in-combat (bad for CHAR base assault, good for most other combat situations)
        if (m_Actor.Model.DefaultController==typeof(CivilianAI)) {
          if (m_Actor.Location.MapObject is DoorWindow door && door.IsOpen && !InCombat) {
            Objectives.Insert(0,new Goals.StageAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, new ActionCloseDoor(m_Actor, door, true)));
            return;
          }
        }
        return;
      }
    }

    protected void DecideMove_WaryOfTraps<T>(Dictionary<Location, T> src)
    {
	  var trap_damage_field = new Dictionary<Location,int>();
	  foreach (var x in src) trap_damage_field.Add(x.Key,x.Key.Map.TrapsUnavoidableMaxDamageAtFor(x.Key.Position,m_Actor));

	  var safe = src.Keys.Where(x => 0>=trap_damage_field[x]);
	  int new_dest = safe.Count();
      if (0 == new_dest) {
		safe = src.Keys.Where(x => m_Actor.HitPoints>trap_damage_field[x]);
		new_dest = safe.Count();
      }
      if (0 >= new_dest || new_dest >= src.Count) return;
      src.OnlyIf(x => safe.Contains(x));
    }

    protected ActorAction? DecideMove(Dictionary<Location,int> src)
	{
      if (null == src) return null; // does happen
      var legal_steps = m_Actor.OnePathRange(m_Actor.Location); // other half
      legal_steps.OnlyIf(action => action.IsPerformable() && !VetoAction(action));
      src.OnlyIf(loc => legal_steps.ContainsKey(loc));
      if (0 >= src.Count) return null;
      src.OnlyIfMinimal();

      DecideMove_WaryOfTraps(src);

      // XXX \todo if there are maps we do not want to path to, screen those here
      List<Location> tmp = src.Keys.ToList();
      if (1 >= tmp.Count) return _finalDecideMove(tmp,legal_steps);

	  // do not get in the way of allies' line of fire
	  tmp = DecideMove_Avoid(tmp, FriendsLoF());
      if (1 >= tmp.Count) return _finalDecideMove(tmp,legal_steps);

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  ThreatTracking threats = m_Actor.Threats;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  Dictionary<Point,HashSet<Point>>? hypothetical_los = null;
      HashSet<Point>? new_los = null;

      if (null != threats || null != sights_to_see) {
	    hypothetical_los = new Dictionary<Point,HashSet<Point>>();
        new_los = new HashSet<Point>();

	    // only need points newly in FOV that aren't currently
	    foreach(var x in tmp) {
          if (!legal_steps.ContainsKey(x)) continue;
          if (legal_steps[x] is ActionUseExit) continue;
          Location? test = m_Actor.Location.Map.Denormalize(in x);
          if (null == test) throw new ArgumentNullException(nameof(test));
	      hypothetical_los[test.Value.Position] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, test.Value).Except(FOV));
          new_los.UnionWith(hypothetical_los[test.Value.Position]);
	    }

        // only need to check if new locations seen
        if (0 >= new_los.Count) {
          threats = null;
          sights_to_see = null;
        }
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position - (Point)tmp_LOSrange, (Point)(2*tmp_LOSrange+1));

      if (null != threats) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
        if (1 >= tmp.Count) return _finalDecideMove(tmp,legal_steps);
	  }
	  if (null != sights_to_see) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) {
          tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
          if (1 >= tmp.Count) return _finalDecideMove(tmp,legal_steps);
        }
	  }

      // weakly prefer not to shove
      tmp = DecideMove_NoShove(tmp, legal_steps);

      // weakly prefer not to push
      tmp = DecideMove_NoPush(tmp, legal_steps);

      // weakly prefer not to jump
      tmp = DecideMove_NoJump(tmp);
      return _finalDecideMove(tmp,legal_steps);
	}

    protected ActorAction DecideMove(Dictionary<Location,ActorAction> src)
	{
      if (null == src) return null; // does happen
      src.OnlyIf(loc => 1==Rules.InteractionDistance(m_Actor.Location,in loc));
      if (0 >= src.Count) return null;

      DecideMove_WaryOfTraps(src);

      // XXX \todo if there are maps we do not want to path to, screen those here
      if (1 >= src.Count) return _finalDecideMove(src);

	  // do not get in the way of allies' line of fire
	  src = DecideMove_Avoid(src, FriendsLoF());
      if (1 >= src.Count) return _finalDecideMove(src);

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  var threats = m_Actor.Threats;
	  var sights_to_see = m_Actor.InterestingLocs;
	  Dictionary<Point,HashSet<Point>>? hypothetical_los = null;
      HashSet<Point>? new_los = null;

      if (null != threats || null != sights_to_see) {
	    hypothetical_los = new Dictionary<Point,HashSet<Point>>();
        new_los = new HashSet<Point>();

	    // only need points newly in FOV that aren't currently
	    foreach(var x in src) {
          if (x.Value is ActionUseExit) continue;
          Location? test = m_Actor.Location.Map.Denormalize(x.Key);
          if (null == test) throw new ArgumentNullException(nameof(test));
	      hypothetical_los[test.Value.Position] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, test.Value).Except(FOV));
          new_los.UnionWith(hypothetical_los[test.Value.Position]);
	    }

        // only need to check if new locations seen
        if (0 >= new_los.Count) {
          threats = null;
          sights_to_see = null;
        }
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position - (Point)tmp_LOSrange, (Point)(2*tmp_LOSrange+1));

      if (null != threats) {
        src = DecideMove_maximize_visibility(src, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
        if (1 >= src.Count) return _finalDecideMove(src);
	  }
	  if (null != sights_to_see) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) {
          src = DecideMove_maximize_visibility(src, inspect, new_los, hypothetical_los);
          if (1 >= src.Count) return _finalDecideMove(src);
        }
	  }

      // weakly prefer not to shove
      DecideMove_NoShove(src);
      if (1 >= src.Count) return _finalDecideMove(src);

      // weakly prefer not to push
      DecideMove_NoPush(src);
      if (1 >= src.Count) return _finalDecideMove(src);

      // weakly prefer not to jump
      DecideMove_NoJump(src);
      return _finalDecideMove(src);
	}

#nullable enable
    protected ActorAction? DecideMove(Dictionary<Location, KeyValuePair<ActorAction, int>> dests)
	{
      if (0 >= dests.Count) return null;
      dests.Minimize(x => x.Value.Value);
      if (1 == dests.Count) return dests.First().Value.Key;   // intentionally allow non-moving shove through to simulate panic

      var _dests = new Dictionary<Location, ActorAction>();
      foreach(var x in dests) _dests.Add(x.Key, x.Value.Key);

      DecideMove_WaryOfTraps(_dests);
      if (1 == _dests.Count) return _dests.First().Value;

	  // do not get in the way of allies' line of fire
	  _dests = DecideMove_Avoid(_dests, FriendsLoF());
      if (1 == _dests.Count) return _dests.First().Value;

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  var threats = m_Actor.Threats;
	  var sights_to_see = m_Actor.InterestingLocs;
	  Dictionary<Point,HashSet<Point>>? hypothetical_los = null;
      HashSet<Point>? new_los = null;

      if (null != threats || null != sights_to_see) {
	    hypothetical_los = new Dictionary<Point,HashSet<Point>>();
        new_los = new HashSet<Point>();

	    // only need points newly in FOV that aren't currently
	    foreach(var x in dests) {
          if (x.Value.Key is ActionUseExit) continue;
          Location? test = m_Actor.Location.Map.Denormalize(x.Key);
          if (null == test) throw new ArgumentNullException(nameof(test));
	      hypothetical_los[test.Value.Position] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, test.Value).Except(FOV));
          new_los.UnionWith(hypothetical_los[test.Value.Position]);
	    }

        // only need to check if new locations seen
        if (0 >= new_los.Count) {
          threats = null;
          sights_to_see = null;
        }
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 1;
      Rectangle view = new Rectangle(m_Actor.Location.Position - (Point)tmp_LOSrange, (Point)(2*tmp_LOSrange+1));

      if (null != threats) {
        _dests = DecideMove_maximize_visibility(_dests, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
        if (1 == _dests.Count) return _dests.First().Value;
	  }
	  if (null != sights_to_see) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) {
          _dests = DecideMove_maximize_visibility(_dests, inspect, new_los, hypothetical_los);
          if (1 == _dests.Count) return _dests.First().Value;
        }
	  }

      // weakly prefer not to shove
      DecideMove_NoShove(_dests);
      if (1 == _dests.Count) return _dests.First().Value;

      // weakly prefer not to push
      DecideMove_NoPush(_dests);
      if (1 == _dests.Count) return _dests.First().Value;

      // weakly prefer not to jump
      DecideMove_NoJump(_dests);
      return _finalDecideMove(_dests);
	}
#nullable restore

    private ActionMoveStep _finalDecideMove(IEnumerable<Point> src, List<Point> tmp2)
    {
      var range = new Dictionary<Point,IEnumerable<Point>>();
      foreach (var pt in src) {
        var test = tmp2.Where(pt2 => Rules.IsAdjacent(in pt,in pt2));
        if (!test.Any()) continue;
        range[pt] = test;
      }
      if (0 >= range.Count) return null;
      bool prefer_cardinal(Point pt) {
        if (m_Actor.Location.Position.X == pt.X) return true;
        if (m_Actor.Location.Position.Y == pt.Y) return true;
        return false;
      }

      var next = range.Keys.ToList();
      while(0 < next.Count) {
        var x = Rules.Get.DiceRoller.ChooseWithoutReplacement(next, prefer_cardinal);
        var act = Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, x));
        if (act is ActionMoveStep step && step.IsLegal()) {
          m_Actor.Run();
          if (!range.TryGetValue(x,out var final_dests)) throw new InvalidProgramException("tried to move where no safe location is nearby");
          var final_act = new Dictionary<Point,ActorAction>();
          foreach(var pt2 in final_dests) {
            var act2 = Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, pt2));   // \todo may need to be a pathability check to enable push/shove
            if (null == act2) continue;
            if (   act2 is ActionMoveStep
                || act2 is ActionOpenDoor) {    // \todo may need to whitelist other actions.  Opening doors needs to be scheduled slightly earlier; this is historical behavior
              final_act[pt2] = act2;
            }
          }
          if (0 >= final_act.Count) continue;
          // \todo: something that decides which escape destination to use, (i.e. actually can react to newly visible threat, etc.)
          var schedule = Rules.Get.DiceRoller.Choose(final_act);
          Objectives.Insert(0,new Goal_NextCombatAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, schedule.Value, schedule.Value));
          return step;
        }
      }
	  return null;
    }

    // src_r2 is the desired destination list
    // src are legal steps
    protected ActionMoveStep DecideMove(IEnumerable<Point> src, IEnumerable<Point> src_r2)
	{
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
      if (null == src_r2) throw new ArgumentNullException(nameof(src_r2));
#endif
	  List<Point> tmp2 = src_r2.ToList();
      if (1 >= tmp2.Count) return _finalDecideMove(src, tmp2);

	  // do not get in the way of allies' line of fire
	  tmp2 = DecideMove_Avoid(tmp2, FriendsLoF());
      if (1 >= tmp2.Count) return _finalDecideMove(src, tmp2);

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  ThreatTracking threats = m_Actor.Threats;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  Dictionary<Point,HashSet<Point>>? hypothetical_los = null;
      HashSet<Point>? new_los = null;

      if (null != threats || null != sights_to_see) {
  	    hypothetical_los = new Dictionary<Point,HashSet<Point>>();
        new_los = new HashSet<Point>();

	    // only need points newly in FOV that aren't currently
	    foreach(Point pt in tmp2) {
	      hypothetical_los[pt] = new HashSet<Point>(LOS.ComputeFOVFor(m_Actor, new Location(m_Actor.Location.Map,pt)).Except(FOV));
          new_los.UnionWith(hypothetical_los[pt]);
	    }
        // only need to check if new locations seen
        if (0 >= new_los.Count) {
          threats = null;
          sights_to_see = null;
        }
      }

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 2;
      Rectangle view = new Rectangle(m_Actor.Location.Position - (Point)tmp_LOSrange, (Point)(2*tmp_LOSrange+1));

	  if (null != threats) {
        tmp2 = DecideMove_maximize_visibility(tmp2, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
        if (1 >= tmp2.Count) return _finalDecideMove(src, tmp2);
	  }
	  if (null != sights_to_see && 2<=tmp2.Count) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) {
          tmp2 = DecideMove_maximize_visibility(tmp2, inspect, new_los, hypothetical_los);
          if (1 >= tmp2.Count) return _finalDecideMove(src, tmp2);
        }
	  }

      // weakly prefer not to jump
      tmp2 = DecideMove_NoJump(tmp2);

      return _finalDecideMove(src,tmp2);
	}

    protected List<Point> DecideMove_WaryOfTraps(List<Point> src)
    {
	  Dictionary<Point,int> trap_damage_field = new Dictionary<Point,int>();
   	  foreach (Point pt in src) {
		trap_damage_field[pt] = m_Actor.Location.Map.TrapsUnavoidableMaxDamageAtFor(pt,m_Actor);
	  }
	  IEnumerable<Point> safe = src.Where(pt => 0>=trap_damage_field[pt]);
	  int new_dest = safe.Count();
      if (0 == new_dest) {
		safe = src.Where(pt => m_Actor.HitPoints>trap_damage_field[pt]);
		new_dest = safe.Count();
      }
      return ((0 < new_dest && new_dest < src.Count) ? safe.ToList() : src);
    }
#endregion

    public Dictionary<Location, ActionMoveDelta> PlanWalkAwayFrom(Dictionary<Location,Actor> fear, IEnumerable<Location> range = null, IEnumerable<Location> range2 = null)
    {
      var move_plans = new Dictionary<Location,KeyValuePair<int,Dictionary<Location,ActionMoveDelta>>>();
      var inverted_move_plans = new Dictionary<Location, Dictionary<Location, ActionMoveDelta>>();
      var process = new HashSet<Location>();
      var working_fear = new Dictionary<Location,Actor>(fear);  // need a value-copy just in case
      var fire_lines = new Dictionary<Location,List<Location[]>>();

      // moral constructors
      {
      var friends = friends_in_FOV;
      if (null != friends) {
         ItemRangedWeapon rw;
         Location[] line;
         foreach(var x in friends) {
           if (null == (rw = (x.Value.Controller as ObjectiveAI)?.GetBestRangedWeaponWithAmmo())) continue;
           foreach(var y in fear) {
             if (null == (line = LOS.IdealFireLine(x.Value.Location, y.Value.Location, rw.Model.Attack.Range))) continue;
             foreach(var pt in line) {
               if (fire_lines.TryGetValue(pt,out var cache)) cache.Add(line);
               else fire_lines.Add(pt,new List<Location[]> { line });
             }
           }
         }
      }
      }

      var blacklist = new HashSet<Location>();
      var whitelist = new HashSet<Location>();
      if (null != range) {
        whitelist.UnionWith(range);
        foreach(var pt in m_Actor.Location.Position.Adjacent()) {
          var loc = new Location(m_Actor.Location.Map,pt);
          if (Map.Canonical(ref loc) && !whitelist.Contains(loc)) blacklist.Add(loc);
        }
      } else if (null != range2) {
        whitelist.UnionWith(range2);
        var map = m_Actor.Location.Map;
        foreach(Point pt in Enumerable.Range(0,16).Select(i=> m_Actor.Location.Position.RadarSweep(2,i))) {
          var loc = new Location(map, pt);
          if (Map.Canonical(ref loc) && !whitelist.Contains(loc)) blacklist.Add(loc);
        }
        // do not have to update whitelist here
        foreach(var pt in m_Actor.Location.Position.Adjacent()) {
          var loc = new Location(map, pt);
          if (Map.Canonical(ref loc) && !whitelist.Any(pt2 => 1==Rules.GridDistance(loc, in pt2))) blacklist.Add(loc);
        }
      }

      // local functions
      bool destination_melee_safe(in Location loc,int depth) {
        if (!m_Actor.CanEnter(loc)) return false;
        if (move_plans.ContainsKey(loc)) return false;
        if (working_fear.ContainsKey(loc)) return false;
        if (blacklist.Contains(loc)) return false;
        foreach(var x in working_fear) {    // not quite true at very high speeds
          if ((1<=depth || !m_Actor.WillActAgainBefore(x.Value))) {
            if (1 >= Rules.InteractionDistance(in loc,x.Key)) return false;
          }
        }
        return true;
      }

      void unlink(in Location dest)
      {
         if (!inverted_move_plans.TryGetValue(dest,out var cache)) return;
         List<Location> staging = null;
         foreach(var x in cache) {
           if (!move_plans.TryGetValue(x.Key,out var test)) continue;
           test.Value.Remove(dest);
           if (0 >= test.Value.Count) {
             (staging ?? (staging = new List<Location>())).Add(x.Key);
             move_plans[x.Key] = new KeyValuePair<int, Dictionary<Location, ActionMoveDelta>>(test.Key,null);
           }
         }
         inverted_move_plans.Remove(dest);
         if (null != staging) foreach(var x in staging) unlink(in x);
      }

#if PROTOTYPE
      int code(ActionMoveDelta x) {
        var real = x.ConcreteAction;
        if (real is ActionMoveStep || real is ActionUseExit) return 4;
        else if (real is ActionOpenDoor) return 3;
        else if (real is ActionPush || real is ActionPull || real is ActionShove) return 2;
        else if (real is ActionSwitchPlace) return 1;
        return 0;
      }
#endif

      Dictionary<Location, ActionMoveDelta> decide() {
        if (!move_plans.TryGetValue(m_Actor.Location,out var test)) return null;    // invalid
        if (null == test.Value || 0 >= test.Value.Count) return null;   // invalid
        return test.Value;
      }

      KeyValuePair<bool,Dictionary<Location,ActionMoveDelta>> trivialized()
      {
        if (move_plans.TryGetValue(m_Actor.Location,out var test)) {
          if (null == test.Value || 0 >= test.Value.Count) return new KeyValuePair<bool, Dictionary<Location, ActionMoveDelta>>(true,null);
          if  (1 == test.Value.Count) return new KeyValuePair<bool, Dictionary<Location, ActionMoveDelta>>(true, test.Value);
        }
        return new KeyValuePair<bool, Dictionary<Location, ActionMoveDelta>>(false,null);
      }

      void install_moves(in Location loc) {
        if (!m_Actor.CanEnter(loc)) return;
        int depth = 0;
        if (move_plans.TryGetValue(loc,out var cache)) {
          depth = cache.Key+1;
          if (null == cache.Value || 0<cache.Value.Count) return;   // already done
        } else move_plans[loc] = (cache = new KeyValuePair<int, Dictionary<Location, ActionMoveDelta>>(0,new Dictionary<Location, ActionMoveDelta>()));
        // generate logical moves
        foreach(var pt in loc.Position.Adjacent()) {
          var loc2 = new Location(loc.Map,pt);
          if (!Map.Canonical(ref loc2)) continue;
          if (!destination_melee_safe(in loc2, depth)) continue;
          var delta = new ActionMoveDelta(m_Actor, in loc2, in loc);
          if (delta.IsLegal()) {
            cache.Value.Add(loc2, delta);
            move_plans.Add(loc2, new KeyValuePair<int, Dictionary<Location, ActionMoveDelta>>(depth,new Dictionary<Location, ActionMoveDelta>()));
            if (inverted_move_plans.TryGetValue(loc2,out var origins)) origins.Add(loc, delta);
            else (inverted_move_plans[loc2] = new Dictionary<Location, ActionMoveDelta>()).Add(loc, delta);
          }
        }
        if (m_Actor.Model.Abilities.AI_CanUseAIExits) {
          var e = loc.Exit;
          if (null != e) {
            var loc2 = e.Location;
            if (destination_melee_safe(in loc2, depth)) {
              var delta = new ActionMoveDelta(m_Actor, in loc2, in loc);
              if (delta.IsLegal()) {
                cache.Value.Add(loc2, delta);
                move_plans.Add(loc2, new KeyValuePair<int, Dictionary<Location, ActionMoveDelta>>(depth,new Dictionary<Location, ActionMoveDelta>()));
                if (inverted_move_plans.TryGetValue(loc2,out var origins)) origins.Add(loc, delta);
                else (inverted_move_plans[loc2] = new Dictionary<Location, ActionMoveDelta>()).Add(loc, delta);
              }
            }
          }
        }
        if (0 >= cache.Value.Count) {
          move_plans[loc] = new KeyValuePair<int, Dictionary<Location, ActionMoveDelta>>(depth,null);
          return;
        }
        if (2 <= cache.Value.Count) {
          var blocked = new Dictionary<Location,int>();
          foreach (var x in cache.Value) {
            if (!fire_lines.TryGetValue(x.Key,out var failed)) continue;
            blocked.Add(x.Key,failed.Count);
          }
          if (0 < blocked.Count) {
            if (blocked.Count >= cache.Value.Count) {
              int min_blocked = blocked.Min(x => x.Value);
              cache.Value.OnlyIf((Predicate<Location>)(x => min_blocked == blocked[x]));
            } else {
              foreach(var x in blocked) cache.Value.Remove(x.Key);
            }
          }
        }
        process.UnionWith(cache.Value.Keys);
      }

      // set up
      int current_depth = 0;
      install_moves(m_Actor.Location);  // depth 0
      var trivial = trivialized();
      if (trivial.Key) return trivial.Value;

      // following would be a *very* inefficient Dijkstra if allowed to iterate indefinitely.
      while(3 > ++current_depth) {
        var working_process = process;
        process = new HashSet<Location>();
        foreach(var dest in working_process) install_moves(in dest);
        if (0 >= process.Count) return decide();

        // any deadends just attempted (working process) should have their inverse moves pruned
        bool have_pruned = false;
        foreach(var x in working_process) {
          if (move_plans.TryGetValue(x,out var cache) && null==cache.Value) {
            unlink(in x);
            have_pruned = true;
          }
        }
        if (have_pruned) {
          trivial = trivialized();
          if (trivial.Key) return trivial.Value;
        }

        // \todo the inverted move plans for depth 2 (in process here) are the first depth for which multiple routes are possible.
      }

      return decide();
    }

    public Dictionary<Location, ActionMoveDelta> PlanWalkAwayFrom(Dictionary<Location,Actor> fear, IEnumerable<Point> range = null, IEnumerable<Point> range2 = null)
    {
      List<Location> upgrade(IEnumerable<Point> src) {
        var ret = new List<Location>();
        var map = m_Actor.Location.Map;
        foreach(var pt in src) {
          var loc = new Location(map, pt);
          if (Map.Canonical(ref loc)) ret.Add(loc);
        }
        return (0 < ret.Count) ? ret : null;
      }

      return PlanWalkAwayFrom(fear, (null != range) ? upgrade(range) : null, (null != range2) ? upgrade(range2) : null);
    }

    public void ExecuteActionChain(List<ActorAction> actions)
    {
      var act = actions[0];
      if (act is Resolvable resolve) act = resolve.ConcreteAction;
      if (act is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest); // XXX should be more tactically aware
      act.Perform();
      if (!(actions[0] is ActorDest) || !actions[0].IsPerformable()) actions.RemoveAt(0);
      if (1<=actions.Count)
        Objectives.Insert(0,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, 1 < actions.Count ? new ActionChain(m_Actor, actions)
                                                                                                                       : actions[0]));
    }

    public void ExecuteActionFork(List<ActorAction> actions)
    {
      var act = Rules.Get.DiceRoller.Choose(actions);
#if DEBUG
      if (act is Engine._Action.Fork) throw new ArgumentOutOfRangeException(nameof(act),act,"fork of fork not reasonable");
      if (!act.IsPerformable()) throw new ArgumentOutOfRangeException(nameof(act),act,"not performable");
#endif
      act.Perform();
    }

    private HashSet<Location> Goals(Func<Map, HashSet<Point>> targets_at, Map dest, Predicate<Map> preblacklist)
    {
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": ObjectiveAI::Goals (depth 1) @ "+m_Actor.Location);
#endif
      var exit_veto = Goal<Goals.BlacklistExits>();
      var goals = new HashSet<Location>();
      var already_seen = new List<Map>();
      var scheduled = new List<Map>();
      var obtain_goals_cache = new Dictionary<Map,HashSet<Point>>();
      // branch-bound prefiltering support
      var min_dist = new Dictionary<Location,int>();
      var waypoint_for_dist = new Dictionary<Location,Location>();
      int lb = int.MaxValue;
      int ub = int.MaxValue;
      var waypoint_dist = new Dictionary<Location,Point>(); // X lower bound, Y upper bound

      void install_waypoint(Location loc, Point dists) {
        if (!waypoint_dist.ContainsKey(loc)) waypoint_dist[loc] = dists;
        var e = loc.Exit;
        if (null != e && !m_Actor.Model.Abilities.AI_CanUseAIExits && e.ToMap.District==loc.Map.District) e = null;
        if (null != e && !waypoint_dist.ContainsKey(e.Location) && (null == exit_veto || !exit_veto.Veto(e))) {
          if (VetoExit(m_Actor, e)) {
            if (1 < loc.Map.destination_maps.Get.Count) {
              if (null == exit_veto) {
                exit_veto = new Goals.BlacklistExits(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, e);
                Objectives.Add(exit_veto);
              } else exit_veto.Blacklist(e);
            }
          } else {
            var move = new ActionMoveDelta(m_Actor, e.Location, loc);
            int cost = Map.PathfinderMoveCosts(move);
            if (short.MaxValue-cost > dists.Y) waypoint_dist[e.Location] = dists + new Point(cost,cost);
          }
        }
      }

      install_waypoint(m_Actor.Location, new Point(0,0));
      int actor_mapcode = District.UsesCrossDistrictView(m_Actor.Location.Map);

      Point waypoint_bounds(in Location loc) {
        Point ret = Point.MaxValue;
        if (waypoint_dist.TryGetValue(loc, out var ret2)) return ret2;

        void set_distance_estimate(Location test, in Location loc, Point range)
        {
          int dist = Rules.InteractionDistance(test,in loc);
          if (short.MaxValue <= dist || short.MaxValue - dist <= range.X) return;
          int lb_dist = dist + range.X;
//        if (ub < lb_dist) continue;   // doesn't work in practice; pathfinder needs these long-range values as waypoint anchors
          if (ret.X < lb_dist) return;
          // no restrictions causes problems w/hospital ground floor 2020-09-07 zaimoni
          if (test != m_Actor.Location && 0 < actor_mapcode) {
            var e = test.Exit;
            if (null != e && !m_Actor.Model.Abilities.AI_CanUseAIExits && e.ToMap.District==loc.Map.District) e = null;
            if (null != e) {
              int loc_mapcode = District.UsesCrossDistrictView(loc.Map);
              if (0 < loc_mapcode) {
                int test_mapcode = District.UsesCrossDistrictView(test.Map);
                int dest_mapcode = District.UsesCrossDistrictView(e.ToMap);
                if (test_mapcode == loc_mapcode && 0 >= dest_mapcode) return;  // vertical exit away from cross-map view not a useful waypoint
              }
            }
          }

          if (ret.X > dist) {
            ret.X = (short)dist;
            waypoint_for_dist[loc] = test;
          }
          short ub_dist = short.MaxValue;
          if (ub_dist/2 >= dist && ub_dist - 2*dist > range.Y) ub_dist = (short)(2*dist + range.Y);
          if (ret.Y > ub_dist) {
            ret.Y = ub_dist;
            waypoint_for_dist[loc] = test;
          }
        }

        set_distance_estimate(m_Actor.Location, in loc, new Point(0,0));
        foreach(var x in waypoint_dist) set_distance_estimate(x.Key, in loc, x.Value);
#if DELEGATED_TO_CALLER
        if (int.MaxValue <= ret.X) throw new InvalidOperationException("no-data bounds: "+ret.to_s()+" for "+loc+"; "+waypoint_dist.to_s()+" "+ last_waypoint_ok.ToString());
#endif
        return ret;
      }

      HashSet<Point> obtain_goals(Map m) {  // return value is only checked for zero/no-zero count, but we already paid for a full construction
        if (obtain_goals_cache.TryGetValue(m,out var cache)) return cache;
#if TRACE_GOALS
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name + ": obtaining goals for " + m +"\nalready seen: "+already_seen.to_s()+"\nscheduled: "+ scheduled.to_s()+ "\nwaypoint_dist: "+waypoint_dist.to_s());
#endif
Restart:
        var dests = targets_at(m);
        if (0 < dests.Count) {
          try {
          foreach(var pt in dests) {
            var loc = new Location(m,pt);
            Point dist = waypoint_bounds(in loc);
            if (short.MaxValue <= dist.Y) continue;
            if (ub < dist.X) continue;
            if (ub > dist.Y) {
              ub = dist.Y;
              List<Location> remove = null;
              foreach(var x in min_dist) {
                if (ub < x.Value) {
                  (remove ?? (remove = new List<Location>(min_dist.Count))).Add(x.Key);
#if TRACE_GOALS
                  if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "removing " + x.Key + " in favor of "+loc+"; "+x.Value+", "+ dist.to_s());
#endif
                }
              }
              if (null != remove) foreach(var x in remove) {
                goals.Remove(x);
                min_dist.Remove(x);
                waypoint_for_dist.Remove(x);
              }
            }
            if (lb > dist.X) lb = dist.X;
#if TRACE_GOALS
            if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "min dist: "+loc.ToString()+": "+ dist.to_s());
#endif
            if (!min_dist.TryGetValue(loc,out var old_min) || old_min>dist.X) min_dist[loc] = dist.X;
            goals.Add(loc);
          }
          } catch (InvalidOperationException e) {
            if (e.Message.Contains("Collection was modified")) goto Restart;
            throw;
          }
        }
        already_seen.Add(m);
#if TRACE_GOALS
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name + ": min_dist " + min_dist.to_s()+"\nwaypoint_for_dist "+waypoint_for_dist.to_s());
#endif
        return obtain_goals_cache[m] = dests;
      }

      bool veto_map(Map m, Map src) {
        if (0 < obtain_goals(m).Count) return false;
        if (1 >= m.destination_maps.Get.Count) return true;
        if (!m_Actor.Model.Abilities.AI_CanUseAIExits) {
          int my_code = District.UsesCrossDistrictView(m_Actor.Location.Map);
          if (District.UsesCrossDistrictView(m) != my_code) return true;
        };
        bool is_surface = m == m.District.EntryMap;
        if (is_surface) return false;
        if (null != Session.Get.UniqueMaps.NavigateHospital(src)) return false;
        if (null != Session.Get.UniqueMaps.NavigatePoliceStation(src)) return false;
        foreach(var test in m.destination_maps.Get) {
          if (test == src) continue;
          if (0 < obtain_goals(test).Count) return false;
          if (test == test.District.EntryMap) return false;
          if (!is_surface && test.destination_maps.Get.Contains(m.District.EntryMap)) return false;
          if (null != Session.Get.UniqueMaps.NavigateHospital(test)) return false;
          if (null != Session.Get.UniqueMaps.NavigatePoliceStation(test)) return false;
        }
        return true;
      }

      var where_to_go = obtain_goals(dest);
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "goal iteration: " + goals.to_s()+" for "+dest);
#endif

      // upper/lower bounds; using X as lower, Y as upper bound
        
      // The SWAT team can have a fairly impressive pathing degeneration at game start (they want their heavy hammers, etc.)
      if (0==where_to_go.Count && 0>=District.UsesCrossDistrictView(dest)) {
        var maps = new HashSet<Map>(dest.destination_maps.Get);
        if (null != preblacklist) maps.RemoveWhere(preblacklist);
        if (1<maps.Count) {
          foreach(Map m in maps.ToList()) if (veto_map(m,dest)) maps.Remove(m);
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "goal iteration: " + goals.to_s() + " for exit short-circuit  of " + dest);
#endif
        }
        if (1==maps.Count && 0==goals.Count) {
          Dictionary<Point,Exit> exits = dest.GetExits(e => maps.Contains(e.ToMap));
          foreach(var pos_exit in exits) {
            Location loc = new Location(dest, pos_exit.Key);
            if (!Map.Canonical(ref loc)) continue;
            goals.Add(loc==m_Actor.Location ? pos_exit.Value.Location : loc);
          }
#if TRACE_GOALS
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name + ": short-circuit exit " + goals.to_s());
#endif
          return goals;
        }
        // if that isn't enough, we could also use the police and hospital geometries
      } 

      void schedule_maps(Map m2) {
#if TRACE_GOALS
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name + ": scheduling " + m2 +"\nalready seen: "+already_seen.to_s()+"\nscheduled: "+ scheduled.to_s());
#endif
        var ok_maps = new HashSet<Map>();
        m2.ForEachExit((pt,e) => {
          if (already_seen.Contains(e.ToMap)) return;
          if (scheduled.Contains(e.ToMap)) return;
          if (null!=preblacklist && preblacklist(e.ToMap)) return;
          if (veto_map(e.ToMap,m2)) return;
          if (null != exit_veto && exit_veto.Veto(e)) return;
          if (VetoExit(m_Actor, e)) {
            if (1 < m2.destination_maps.Get.Count) {
              if (null == exit_veto) {
                exit_veto = new Goals.BlacklistExits(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, e);
                Objectives.Add(exit_veto);
              } else exit_veto.Blacklist(e);
            }
            return;
          }
          Location dest = new Location(m2, pt);
          Point dist = waypoint_bounds(dest);
#if DEBUG
          if (short.MaxValue == dist.X) throw new InvalidOperationException("no distance estimate for "+(new Location(m2, pt)));
#else
          if (short.MaxValue == dist.X) return; // something haywire, discard
#endif
#if DEBUG
          if (0 > dist.X || 0 > dist.Y) throw new InvalidOperationException("negative distance bounds: "+dist.to_s());
#endif
          if (ub < dist.X) return;
          bool in_bounds = m2.IsInBounds(pt);
          if (in_bounds) install_waypoint(dest, dist);
          ok_maps.Add(e.ToMap);
        });
        scheduled.AddRange(ok_maps);
      }

      schedule_maps(dest);

      while(0 < scheduled.Count) {
        var m = scheduled[0];

        obtain_goals(m);
#if TRACE_GOALS
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "goal iteration: " + goals.to_s() + " for " + m);
#endif
        schedule_maps(m);

        scheduled.RemoveAt(0);
      }
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "final goals: " + goals.to_s() + "\nwaypoints: " + waypoint_for_dist.to_s());
#endif
      var replace_goals = new List<Location>();
      var replace_with = new HashSet<Location>();
      var backup_replace_with = new HashSet<Location>();
      foreach(var x in goals) {
        if (!waypoint_for_dist.TryGetValue(x,out var relay) || relay==m_Actor.Location) continue;
        replace_goals.Add(x);
        (PrevLocation != relay ? replace_with : backup_replace_with).Add(relay);
      }
      goals.ExceptWith(replace_goals);
      goals.UnionWith(replace_with);
      if (0 >= goals.Count()) goals.UnionWith(backup_replace_with);
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "returned goals: " + goals.to_s());
#endif
      return goals;
    }

#if DEAD_FUNC
    private Dictionary<Map, HashSet<Point>> RadixSortLocations(IEnumerable<Location> goals)
    {
      var map_goals = new Dictionary<Map,HashSet<Point>>();
      foreach(var goal in goals) {
        if (map_goals.TryGetValue(goal.Map,out var cache)) {
          cache.Add(goal.Position);
        } else map_goals[goal.Map] = new HashSet<Point> { goal.Position };
      }
      return map_goals;
    }
#endif

    private Dictionary<Map, Dictionary<Point,int>> RadixSortLocations(Dictionary<Location,int> goals)
    {
      var map_goals = new Dictionary<Map, Dictionary<Point, int>>();
      foreach(var goal in goals) {
        if (map_goals.TryGetValue(goal.Key.Map,out var cache)) {
          cache[goal.Key.Position] = goal.Value;
        } else {
           var tmp = new Dictionary<Point, int>();
           tmp[goal.Key.Position] = goal.Value;
           map_goals[goal.Key.Map] = tmp;
        }
      }
      return map_goals;
    }

    private Predicate<Location> BlacklistFunction(Dictionary<Location,int> goals, HashSet<Map> excluded)
    {
       Predicate<Location> ret = null;

      // map prefilter -- essentially a functional blacklist rather than an enumerated one
      var my_map = m_Actor.Location.Map;
      var required_0 = new HashSet<Map> { my_map };

      // any map not containing us but containing goals, will need its distance-to-exits set
      // index is Encode(this Rectangle rect, HashSet<Point> src)
      Rectangle district_span = my_map.NavigationScope;

      Map g_map;
      foreach(var goal in goals) {
        required_0.Add(g_map = goal.Key.Map);
        district_span = Rectangle.Union(district_span, g_map.NavigationScope);
      }

      var required = new HashSet<Map>(required_0);

      // early exit: unique map, not-sewer: should be handled at caller level (do not enforce until we no longer have a serious CPU problem)

      // \todo hospital and police station have unusual behavior (multi-level linear)
      var police_station = required_0.HaveItBothWays(m => null==Session.Get.UniqueMaps.NavigatePoliceStation(m));
      var p_offices = Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap;
      if (!police_station.Value) excluded.Add(p_offices);
      else if (!police_station.Key) excluded.Add(p_offices.District.EntryMap);
      else {
        required.Add(p_offices);
        required.Add(p_offices.District.EntryMap);
      }
      var hospital = required_0.HaveItBothWays(m => null==Session.Get.UniqueMaps.NavigateHospital(m));
      var h_admissions = Session.Get.UniqueMaps.Hospital_Admissions.TheMap;
      if (!hospital.Value) excluded.Add(h_admissions);
      else if (!hospital.Key) excluded.Add(h_admissions.District.EntryMap);
      else {
        required.Add(h_admissions);
        required.Add(h_admissions.District.EntryMap);
      }
      bool ignore_subway = !required_0.Any(District.IsSubwayMap);
      bool ignore_sewer = !required_0.Any(District.IsSewersMap);

      var now = new HashSet<Map>(required);
restart:
      var next = new HashSet<Map>();
      foreach(Map m in now) {
        var dests = m.destination_maps.Get;
        if (1==dests.Count) {
          Map test2 = dests.First();
#if DEBUG
          if (excluded.Contains(test2)) throw new InvalidProgramException("logic paradox: excluded map is also required");
#else
          if (excluded.Contains(test2)) continue;
#endif
          required.Add(test2);
          continue;
        }
        foreach(Map test in dests) {
          if (required.Contains(test) || excluded.Contains(test)) continue;
          // dead end that is not already required, is excluded
          if (1==test.destination_maps.Get.Count) {
            excluded.Add(test);
            continue;
          }
          // do not consider entering subway or sewers if no goals there
          if (ignore_subway && District.IsSubwayMap(test)) {
            excluded.Add(test);
            continue;
          }
          if (ignore_sewer && District.IsSewersMap(test)) {
            excluded.Add(test);
            continue;
          }
          if (!district_span.Contains(test.District.WorldPosition)) {
            excluded.Add(test);
            continue;
          }
          next.Add(test);   // assume that if it passes all pre-screens it may be needed
        }
      }
      { // 2019-01-12: unsure if this block actually helps
      var next2 = new HashSet<Map>();
      foreach(Map m in next) {
         var screen = new HashSet<Map>(m.destination_maps.Get);
         screen.RemoveWhere(x => excluded.Contains(m));
         ((1 >= screen.Count) ? excluded : next2).Add(m);
      }
      next = next2;
      }
      if (0 < next.Count)
            {
            required.UnionWith(next);
            now = next;
            goto restart;
            }

      // once excluded is known, we can construct a lambda function and use that as an additional blacklist.
      if (0<excluded.Count) ret = ret.Or(loc => excluded.Contains(loc.Map));

      return ret;
    }

    protected FloodfillPathfinder<Location> PathfinderFor(Dictionary<Location, int> goal_costs, HashSet<Map> excluded)
    {
#if DEBUG
      if (0 >= (goal_costs?.Count ?? 0)) throw new ArgumentNullException(nameof(goal_costs));
      if (null == excluded) throw new ArgumentNullException(nameof(excluded));
#endif
      var navigate = m_Actor.Location.Map.PathfindLocSteps(m_Actor);

      // \todo BehaviorResupply needs some pathfinding algebra here
      // 1) maps that do not contain the actor just need a cost map to the (relevant) exits i.e are cacheable in principle
      // 2) cache will be invalidated by just about any game-state change affecting pathability since we're "too low" to know where the goal list is coming from.
      // We should be fine using Zaimoni.Data.TimeCache here (at map level).
      // 2a) The key will have to be a unique equality-comparable representation of the goal points on the map (i.e. HashSet won't work)  It doesn't have to be reversible.
      // ** C# char is an unsigned short (2 bytes); this should allow a Map object to convert an in-bounds HashSet<Point> to a unique C# string reliably (lexical ordering)
      // 3) the factored pathfinders will be based on Point rather than location.  The interpolated _now will be based on their exits
      // 4) a map that does not contain a goal, does not contain the origin location, and is not in a "minimal" closed loop that is qualified may be blacklisted for pathing.
      // 4a) a chokepointed zone that does not contain a goal may be blacklisted for pathing

      var blacklist = BlacklistFunction(goal_costs,excluded);
      if (null != blacklist) navigate.InstallBlacklist(blacklist);

      navigate.GoalDistance(goal_costs, m_Actor.Location);
      return navigate;
    }

#nullable enable
    protected FloodfillPathfinder<Point> PathfinderFor(Dictionary<Point, int> goal_costs,Map m)
    {
#if DEBUG
      if (0 >= goal_costs.Count) throw new ArgumentNullException(nameof(goal_costs));
#endif
      var navigate = m.PathfindSteps(m_Actor);

      if (m_Actor.Location.Map == m) {
        navigate.GoalDistance(goal_costs, m_Actor.Location.Position);
      } else {
        navigate.GoalDistance(goal_costs, m.GetEdge());
      }
      return navigate;
    }

    protected FloodfillPathfinder<Point> PathfinderFor(IEnumerable<Point> goals)
    {
#if DEBUG
      if (!goals.Any()) throw new ArgumentNullException(nameof(goals));
#endif
      var navigate = m_Actor.Location.Map.PathfindSteps(m_Actor);

      navigate.GoalDistance(goals, m_Actor.Location.Position);
      return navigate;
    }

    protected ActorAction? BehaviorPathTo(FloodfillPathfinder<Point> navigate)
    {
      if (null == navigate) return null;
      if (!navigate.Domain.Contains(m_Actor.Location.Position)) return null;
      int current_cost = navigate.Cost(m_Actor.Location.Position);
      var approach = PlanApproach(navigate);
      Dictionary<Location, KeyValuePair<ActorAction,int> >? ok_path = null;
      ActionUseExit? _exit_map = null;
      foreach(var x in _legal_path) {
        if (x.Value is ActionUseExit use_exit && use_exit.dest != PrevLocation) {   // no-op unless m_Actor.Model.Abilities.AI_CanUseAIExits
          _exit_map = use_exit;
          continue;
        }
        var denorm = m_Actor.Location.Map.Denormalize(x.Key);
        if (null != denorm && null != approach && approach.TryGetValue(denorm.Value.Position, out var cost)) {
          (ok_path ?? (ok_path = new Dictionary<Location, KeyValuePair<ActorAction, int>>(_legal_path.Count))).Add(x.Key, new KeyValuePair<ActorAction, int>(x.Value, cost));
        }
      }
      // \todo if _exit_map is null, may want to get more clever re push/pull
      if (null == ok_path) return _exit_map;
      var ret = DecideMove(ok_path);
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest); // XXX should be more tactically aware
      var min_steps = navigate.MinStepPathTo(m_Actor.Location.Position);
      if (null != min_steps && 1<min_steps.Count) return RecordMinStepPath(min_steps,ret);
      return ret;
    }
#nullable restore

    protected ActorAction? BehaviorPathTo(FloodfillPathfinder<Location> navigate)
    {
      if (null == navigate) return null;
      if (!navigate.Domain.Contains(m_Actor.Location)) return null;
      Dictionary<Location,int> costs = null;
      var path = navigate.MinStepPathTo(m_Actor.Location,m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather));
#if TRACE_SELECTACTION
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "path: "+path.to_s());
#endif
      if (null != path) {
        var pathable = path[0].FindAll(loc => _legal_path.ContainsKey(loc));
        if (0 >= pathable.Count) return null;
        path[0] = pathable;
      }

      if (null != path) {
        void purge_non_adjacent(int i) {    // \todo non-local function target?
          while(0 < i) {
            var tmp = path[i - 1].FindAll(loc => path[i].Any(loc2 => Rules.IsAdjacent(in loc2, loc)));
            if (tmp.Count >= path[i - 1].Count || 0>=tmp.Count) return;
            path[--i] = tmp;
          }
        }

        if (1 < path[0].Count) {
          // work backwards.
          int i = path.Count;
          while(0 < --i) {
            if (1 < path[i].Count) {
              var no_jump = new List<Location>();
              var jump = new List<Location>();
              foreach(Location loc in path[i]) (Location.RequiresJump(in loc) ? jump : no_jump).Add(loc);
              if (0<jump.Count && 0<no_jump.Count) {
                path[i] = no_jump;
                purge_non_adjacent(i);
                if (1 == path[0].Count) break;
              }
            }
          }
        }
        if (1 == path[0].Count) {
#if DEBUG
          if (navigate.IsBlacklisted(path[0][0])) throw new InvalidOperationException("blacklisted path: "+path.to_s());
#endif
          ActorAction act = _legal_path[path[0][0]];
#if TRACE_SELECTACTION
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "action: "+(act?.ToString() ?? null)+"; "+ (act?.IsLegal() ?? false));
#endif
          if (act is ActionMoveStep tmp) m_Actor.IsRunning = RunIfAdvisable(tmp.dest); // XXX should be more tactically aware

          var min_steps = navigate.MinStepPathTo(m_Actor.Location);
          if (null != min_steps && 1<min_steps.Count) return RecordMinStepPath(min_steps,act);
          return act;
        }
        if (0 < path[0].Count) {
          costs = new Dictionary<Location,int>();
          foreach(var loc in path[0]) costs[loc] = navigate.Cost(loc);
        }
      } else {
        costs = navigate.Approach(m_Actor.Location);
      }

      ActorAction ret = DecideMove(costs);
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest); // XXX should be more tactically aware
      PlanApproach(navigate);
      {
      var min_steps = navigate.MinStepPathTo(m_Actor.Location);
      if (null != min_steps && 1<min_steps.Count) return RecordMinStepPath(min_steps,ret);
      }
      return ret;
    }

    private bool GoalRewrite(HashSet<Location> goals, Dictionary<Location, int> goal_costs, Dictionary<Map, Dictionary<Point, int>> map_goals,Map src,Map dest, HashSet<Map> excluded)
    {
        if (!map_goals.TryGetValue(src,out var test)) {
          excluded.Add(src);
          return false;
        }
        if (src.Exits.Any(e => e.Location==m_Actor.Location)) return false; // would be very bad to remap a goal w/cost onto the actor

        string index = src.Rect.Encode(test);
        if (!map_goals.TryGetValue(dest,out var cache)) {
          cache = new Dictionary<Point,int>();
          map_goals[dest] = cache;
        }

        if (src.pathing_exits_to_goals.TryGetValue(index,out var saved)) {
          foreach(var x in saved) {
            if (x.Key.Map!=dest) continue;
            int cost = x.Value;
            goal_costs[x.Key] = cost;
            cache[x.Key.Position] = cost;
            goals.Add(x.Key);
          }
        } else {
          var archive = new Dictionary<Location,int>();
          var navigate = PathfinderFor(test, src);

          int exit_cost(in Point pt) {
            if (src.IsInBounds(pt)) return navigate.Cost(pt) + 1;
            return pt.Adjacent().Where(pt2 => src.IsInBounds(pt2)).Select(pt2 => navigate.Cost(pt2) + 1).Min();
          }

          src.ForEachExit((pt,e)=> {
            int cost = exit_cost(in pt);
            archive[e.Location] = cost;
            if (e.Location.Map!=dest) return;
            goal_costs[e.Location] = cost;
            cache[e.Location.Position] = cost;
            goals.Add(e.Location);
          });

          src.pathing_exits_to_goals.Set(index,archive);
        }

        foreach(var x in test) {
          Location tmp = new Location(src, x.Key);
          goal_costs.Remove(tmp);
          goals.Remove(tmp);
        }
        map_goals.Remove(src);
        excluded.Add(src);

        bool ret = !District.IsSewersMap(m_Actor.Location.Map) && !goals.Any(loc => loc.Map != m_Actor.Location.Map);
        if (ret) _current_goals = goals;
        return ret;
    }

    private void PartialInvertLOS(HashSet<Location> tainted, short radius)
    {
      var ideal = LOS.OptimalFOV(radius);
      foreach(var loc in tainted.ToList()) {
        if (!loc.Map.WouldBlacklistFor(loc.Position,m_Actor)) continue;
        foreach(var offset in ideal) {
          var legal = new Location(loc.Map, loc.Position + offset); // may be denormalized
          if (!Map.Canonical(ref legal)) continue;
          if (tainted.Contains(legal)) continue;
          if (m_Actor.Location == legal) continue;
          if (legal.Map.WouldBlacklistFor(legal.Position,m_Actor)) continue;
          if (LOS.CanTraceViewLine(in legal, in loc)) tainted.Add(legal);
        }
        tainted.Remove(loc);
      }
    }

    protected ActorAction GreedyStep(Dictionary<Location, int> move_scores, Func<Location, double> minimize)
    {
#if DEBUG
      if (0 >= (move_scores?.Count ?? 0)) throw new ArgumentNullException(nameof(move_scores));
#endif
      var legal_steps = m_Actor.OnePathRange(m_Actor.Location); // mirror DecideMove so we don't error out
      legal_steps.OnlyIf(action => action.IsPerformable() && !VetoAction(action));
      if (0 >= legal_steps.Count) return null;
      move_scores.OnlyIf(loc => legal_steps.ContainsKey(loc));
      if (0 >= move_scores.Count) return null;
      move_scores.OnlyIfMaximal();
      legal_steps.OnlyIf(loc => move_scores.ContainsKey(loc));

      legal_steps = legal_steps.CloneOnlyMinimal(Map.PathfinderMoveCosts);

      ActorAction tmp = DecideMove(legal_steps.CloneOnlyMinimal(minimize));
#if FALSE_POSITIVE
      if (null == tmp) throw new ArgumentNullException(nameof(tmp));
#endif
      if (tmp is ActionMoveStep test) {
        ReserveSTA(0,1,0,0);    // for now, assume we must reserve one melee attack of stamina (which is at least as much as one push/jump, typically)
        m_Actor.IsRunning = RunIfAdvisable(test.dest); // XXX should be more tactically aware
        ReserveSTA(0,0,0,0);
      }
      return tmp;
    }

    // usage: put the actor's location first so we can early-exit if there are any goals in LOS
    private KeyValuePair<Dictionary<Location, int>,Dictionary<Location, int>> DestsinLoS(List<Location> src, HashSet<Location> inspect)
    {
        var inspect_view = new Dictionary<Map, HashSet<Point>>();
        var exposed = new Dictionary<Location,int>();
        var safe_exposed = new Dictionary<Location,int>();
        foreach (var x in src) {
          HashSet<Point> los = LOS.ComputeFOVFor(m_Actor, x);
          if (!inspect_view.TryGetValue(x.Map,out var cache)) {
            cache = new HashSet<Point>();
            foreach(var loc in inspect) {
              if (loc.Map==x.Map) cache.Add(loc.Position);
              else {
                var test = x.Map.Denormalize(in loc);
                if (null!=test) cache.Add(test.Value.Position);
              }
            }
            inspect_view[x.Map] = cache;
          }
          los.IntersectWith(cache);
          if (0 >= los.Count) continue;
          exposed[x] = los.Count;
          if (   m_Actor.RunIsFreeMove
              || !los.Any(pt2 => Rules.IsAdjacent(in pt2, x.Position))
              || (x.MapObject is DoorWindow door && door.IsClosed)) {
            safe_exposed[x] = los.Count;
          }
          if (x == m_Actor.Location) break;
        }
        return new KeyValuePair<Dictionary<Location, int>, Dictionary<Location, int>>(safe_exposed,exposed);
    }

    private ActorAction _recordPathfinding(ActorAction act, HashSet<Location> goals)
    {
      if (null != act) {
        if (goals.ValueEqual(GetPreviousGoals()) && act is ActorDest test && null != _last_move && test.dest == _last_move.origin) {
          bool altered = false;
          foreach(var target in goals) {
            var denorm = m_Actor.Location.Map.Denormalize(in target);
            if (null == denorm) continue;
            var stats = new Tools.MinStepPath(m_Actor, m_Actor.Location, in target);
            foreach(var dir in stats.stats.advancing) {
              var loc = m_Actor.Location+dir;
              if (!Map.Canonical(ref loc)) continue;
              if (loc == test.dest) continue;
              if (!_legal_path.TryGetValue(loc,out var alt_act)) continue;
              if (1 >= FastestTrapKill(in loc)) continue;
              act = alt_act;
              test = alt_act as ActorDest;
              altered = true;
              break;
            }
            if (altered) break;
          }
          if (!altered) {
            var alt_act = _pathNear(test.dest);
            if (null != alt_act) {
              act = alt_act;
              test = act as ActorDest;
              altered = true;
            }
          }
#if DETECT_PERIOD_2_MOVE_LOOP
          if (null != test && test.dest == _last_move.origin) {
            bool backtrack_ok = false;
            if (act is ActionUseExit) {
              // inverse of ActionUseExit is ActionUseExit.  If the origin map has no goals, it's ok
              backtrack_ok = (m_Actor.Location.Map == m_Actor.Location.Map.District.SewersMap || !goals.Any(loc => loc.Map == m_Actor.Location.Map));
            }
            // \todo second test should be its own function
            if (!backtrack_ok && !goals.Any(loc => Rules.InteractionDistance(loc, _last_move.dest) > Rules.InteractionDistance(loc, _last_move.origin))) {
              Session.Get.World.DaimonMap();
              throw new InvalidOperationException(m_Actor.Name + " committed a period-2 move loop on turn " + m_Actor.Location.Map.LocalTime.TurnCounter + ": " + _last_move + ", " + act);
            }
          }
#endif
        }
        RecordGoals(goals);
      }
      return act;
    }

    public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at, Predicate<Map> preblacklist = null, Predicate<Location> postblacklist = null)
    {
      var goals = Goals(targets_at, m_Actor.Location.Map, preblacklist);
      if (0 >= goals.Count) return null;
      PartialInvertLOS(goals, m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather));

      bool rude_goal(Location loc) {
        if (!CanSee(in loc)) return false;
        var actor = loc.Actor;
        if (null == actor) return false;
        return !m_Actor.IsEnemyOf(actor);
      }

      void force_polite(HashSet<Location> x) {
        var rude = new HashSet<Location>(x.Where(rude_goal));
        var ever_rude = new HashSet<Location>(rude);
        var next_rude = new HashSet<Location>();
restart:
        foreach(var fail in rude) {
          var working = fail;
          if (m_Actor.IsInGroupWith(working.Actor)) continue;
          var exit = m_Actor.Location.Exit;
          if (null != exit && exit.Location == working) working = m_Actor.Location;
          foreach(var pt in working.Position.Adjacent()) {
            var loc = new Location(working.Map,pt);
            if (!m_Actor.CanEnter(ref loc)) continue;
            if (null!=preblacklist && preblacklist(loc.Map)) continue;
            if (ever_rude.Contains(loc)) continue;
            if (rude_goal(loc)) {
              next_rude.Add(loc);
              continue;
            }
            // we should be ignoring non-pathable adjacent locations later
            x.Add(loc);
          }
          x.Remove(fail);
        }
        if (0<next_rude.Count) {
          ever_rude.UnionWith(next_rude);
          rude = next_rude;
          next_rude = new HashSet<Location>();
          goto restart;
        }
      }
      force_polite(goals);
      if (null != postblacklist) goals.RemoveWhere(postblacklist);
      return _recordPathfinding(BehaviorPathTo(goals),goals);
    }

    public ActorAction BehaviorPathTo(HashSet<Location> goals)
    {
      if (0 >= (goals?.Count ?? 0)) return null;
#if DEBUG
      if (goals.Contains(m_Actor.Location)) throw new InvalidOperationException(m_Actor.Name+" self-pathing? "+m_Actor.Location+"; "+goals.to_s());
#endif
      bool moveloop_risk = (null != _last_move && goals.Contains(_last_move.origin));

      {
      var moves = m_Actor.OnePath(m_Actor.Location);    // this usage needs to know about invalid moves
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: checking for moves "+(moves?.to_s() ?? "null")+" to adjacent goals "+goals.to_s());
#endif
      {
      bool null_return = false;
      ActorAction? movelooping = null;
      foreach(Location loc in goals) {  // \todo should only null-return if no legal adjacent goals at all
        if (moves.TryGetValue(loc,out var tmp)) {
          if (moveloop_risk && _last_move.origin==loc) {
            movelooping = tmp;
            continue;
          }
          if (tmp.IsPerformable() && !VetoAction(tmp)) return _recordPathfinding(tmp, goals);
          null_return = true;
        }
      }
      if (null != movelooping) {
        if (movelooping.IsPerformable() && !VetoAction(movelooping)) return _recordPathfinding(movelooping, goals);
        null_return = true;
      }
      if (null_return) return null;
      }
      moves.OnlyIf(action => action.IsPerformable() && !VetoAction(action));
      if (0 >= moves.Count) return null;    // possibly ActionWait instead

      // if we couldn't path to an adjacent goal, wait
      if (goals.Any(loc => Rules.IsAdjacent(m_Actor.Location, in loc))) {
        var e = m_Actor.Location.Exit;
        if (null!=e && goals.Contains(e.Location)) {
          // copied from BaseAI::BehaviorUseExit
          var mapObjectAt = e.Location.MapObject;
          if (mapObjectAt != null && m_Actor.CanBreak(mapObjectAt))
            return new ActionBreak(m_Actor, mapObjectAt);
          var actorAt = e.Location.Actor;
          if (null != actorAt && !m_Actor.IsEnemyOf(actorAt)) {
            var act = BehaviorMakeTime();
            if (null != act) return act;
#if DEBUG
            throw new InvalidProgramException("need to handle friend blocking exit: " + goals.Where(loc => Rules.IsAdjacent(m_Actor.Location, in loc)).ToList().to_s());
#endif
          }
          // needs implementation
#if DEBUG
          throw new InvalidProgramException("need to handle adjacent to blocked exit: " + goals.Where(loc => Rules.IsAdjacent(m_Actor.Location, in loc)).ToList().to_s());
#endif
        }
#if DEBUG
        throw new InvalidProgramException("non-pathing to adjacent location: "+goals.Where(loc => Rules.IsAdjacent(m_Actor.Location, in loc)).ToList().to_s());
#else
        return new ActionWait(m_Actor); // completely inappropriate for a z on the other side of an exit
#endif
      }

      // check for pre-existing relevant path (approaching dead code)
      {
      var path_pt = GetMinStepPath<Point>();
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: pre-existing point path: "+(path_pt?.to_s() ?? "null") );
#endif
      if (null != path_pt) {
        var this_map_goals = goals.Where(loc => loc.Map == m_Actor.Location.Map).Select(loc => loc.Position);
        int path_contains_our_goals_pt(List<List<Point>> path) {
          if (this_map_goals.Any()) {
            int n = -1;
            while(path.Count > ++n) if (1<=n && path[n].Any(pt => this_map_goals.Contains(pt))) return n;
          }
          return 0;
        }

        if (GetPreviousGoals().ValueEqual(goals) || 0< path_contains_our_goals_pt(path_pt)) {
              var alt_act = UsePreexistingPath(goals);
              if (null != alt_act) _recordPathfinding(alt_act, goals);
        }
      }
      }

      { // Adaptation of prior "almost in view" heuristic for threat hunting
      int fov = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather);
      double edge_of_maxrange = fov+1.5;

      var near_tainted = new HashSet<Location>();
      foreach(var loc in goals) {
        if (fov + 1 < Rules.GridDistance(in loc, m_Actor.Location)) continue;
        if (edge_of_maxrange > Rules.StdDistance(in loc,m_Actor.Location)) near_tainted.Add(loc);  // slight underestimate for diagonal steps
      }

      double dist_to_all_goals(Location test) {
        double ret = 0;
        foreach(var loc in goals) {
          int working = Rules.InteractionDistance(in loc,in test);
          if (int.MaxValue==working) continue;
          ret += working;
        }
        return ret;
      }

      if (0<near_tainted.Count) {
#if TRACE_GOALS
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: near goals: "+near_tainted.to_s());
#endif
        var candidates = new List<Location>(moves.Count + 1) { m_Actor.Location };
        candidates.AddRange(moves.Keys);
        var goals_in_sight = DestsinLoS(candidates, near_tainted);
        if (goals_in_sight.Value.ContainsKey(m_Actor.Location)) {   // already had goal in sight; not an error condition
        } else if (0<goals_in_sight.Key.Count) return _recordPathfinding(GreedyStep(goals_in_sight.Key, dist_to_all_goals),goals);   // expose goals safely
        else if (0<goals_in_sight.Value.Count) return _recordPathfinding(GreedyStep(goals_in_sight.Value, dist_to_all_goals),goals);   // expose goals unsafely
      }
      }
      }
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: past GreedyStep check");
#endif

       // remove a degenerate case from consideration
       if (   !District.IsSewersMap(m_Actor.Location.Map)
           && !goals.Any(loc => loc.Map!= m_Actor.Location.Map)) {
         _current_goals = goals;
         return _recordPathfinding(BehaviorPathTo(PathfinderFor(goals.Select(loc => loc.Position))),goals);
       }
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: past single map pathfinder reduction");
#endif

#if PROTOTYPE
      if (   1==m_Actor.Location.Map.destination_maps.Get.Count
          && !goals.Any(loc => loc.Map == m_Actor.Location.Map))
            {
            throw new InvalidProgramException("need to handle single-map escape case");
            }
#endif

      var goal_costs = new Dictionary<Location,int>();
      foreach(var goal in goals) goal_costs[goal] = 0;

      var map_goals = RadixSortLocations(goal_costs);

      var excluded = new HashSet<Map>();
#if PROTOTYPE
      var excluded_zones = new List<ZoneLoc>();
#endif

restart_single_exit:
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: iterating restart_single_exit");
#endif
      foreach(var x in map_goals) {
        if (m_Actor.Location.Map == x.Key) continue;    // do not try to goal-rewrite the map we are in
        var tmp = x.Key.destination_maps.Get;
        // 2019-01-13: triggers on subways (diagonal connectors not generated properly)
        if (1==tmp.Count && m_Actor.Location.Map!=tmp.First()) {
          if (GoalRewrite(goals, goal_costs, map_goals, x.Key, tmp.First(),excluded))
            return _recordPathfinding(BehaviorPathTo(PathfinderFor(goals.Select(loc => loc.Position))),goals);
          goto restart_single_exit;
        }
      }

      var my_police_map_code = Session.Get.UniqueMaps.PoliceStationDepth(m_Actor.Location.Map);
      if (0 == my_police_map_code) {
        var goals_map_code = goals.Max(loc => Session.Get.UniqueMaps.PoliceStationDepth(loc.Map));
        if (0 == goals_map_code) {
          excluded.Add(Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap);
          var zone = Session.Get.UniqueMaps.PoliceLanding();    // \todo cache this (profile-negative)
          if (!goals.Any(loc => zone.Contains(loc))) {
            if (zone.Contains(m_Actor.Location)) {
              var escape = zone.WalkOut();  // \todo cache this (profile-negative)
              if (null != escape) {
                var new_goals = new HashSet<Location>();
                foreach(var x in escape) new_goals.Add(x.origin);
                if (!new_goals.Any(loc => Rules.IsAdjacent(loc, m_Actor.Location))) {
                  goals = new_goals;
                  goal_costs.Clear();
                  foreach(var goal in goals) goal_costs[goal] = 0;
                  _current_goals = goals;
                  return _recordPathfinding(BehaviorPathTo(PathfinderFor(goals.Select(loc => loc.Position))),goals);
                }
              }
#if PROTOTYPE
            } else {
              excluded_zones.Add(zone);
#endif
            }
          }
        } else {
          do {
            if (GoalRewrite(goals, goal_costs, map_goals, Session.Get.UniqueMaps.PoliceStationMap(goals_map_code), Session.Get.UniqueMaps.PoliceStationMap(goals_map_code - 1), excluded))
              return _recordPathfinding(BehaviorPathTo(PathfinderFor(goals.Select(loc => loc.Position))),goals);
            var next_code = goals.Max(loc => Session.Get.UniqueMaps.PoliceStationDepth(loc.Map));
            if (goals_map_code <= next_code) break;
            goals_map_code = next_code;
          } while(0 < goals_map_code);
          var no_map = Session.Get.UniqueMaps.PoliceStationMap(goals_map_code + 1);
          if (null != no_map) excluded.Add(no_map);
        }
      }

      var my_hospital_map_code = Session.Get.UniqueMaps.HospitalDepth(m_Actor.Location.Map);
      if (0 == my_hospital_map_code) {
        var goals_map_code = goals.Max(loc => Session.Get.UniqueMaps.HospitalDepth(loc.Map));
        if (0 == goals_map_code) {
          excluded.Add(Session.Get.UniqueMaps.Hospital_Admissions.TheMap);
          var zone = Session.Get.UniqueMaps.HospitalLanding();   // \todo cache this (profile-negative)
          if (!goals.Any(loc => zone.Contains(loc))) {
            if (zone.Contains(m_Actor.Location)) {
              var escape = zone.WalkOut();  // \todo cache this (profile-negative)
              if (null != escape) {
                var new_goals = new HashSet<Location>();
                foreach(var x in escape) new_goals.Add(x.origin);
                if (!new_goals.Any(loc => Rules.IsAdjacent(loc, m_Actor.Location))) {
                  goals = new_goals;
                  goal_costs.Clear();
                  foreach(var goal in goals) goal_costs[goal] = 0;
                  _current_goals = goals;
                  return _recordPathfinding(BehaviorPathTo(PathfinderFor(goals.Select(loc => loc.Position))),goals);
                }
              }
#if PROTOTYPE
            } else {
              excluded_zones.Add(zone);
#endif
            }
          }
        } else {
          do {
            if (GoalRewrite(goals, goal_costs, map_goals, Session.Get.UniqueMaps.HospitalMap(goals_map_code), Session.Get.UniqueMaps.HospitalMap(goals_map_code - 1), excluded))
              return _recordPathfinding(BehaviorPathTo(PathfinderFor(goals.Select(loc => loc.Position))),goals);
            var next_code = goals.Max(loc => Session.Get.UniqueMaps.HospitalDepth(loc.Map));
            if (goals_map_code <= next_code) break;
            goals_map_code = next_code;
          } while(0 < goals_map_code);
          var no_map = Session.Get.UniqueMaps.HospitalMap(goals_map_code + 1);
          if (null != no_map) excluded.Add(no_map);
        }
      }

      int path_contains_our_goals_loc(List<List<Location>> path) {
        int n = -1;
        while(path.Count > ++n) if (1<=n && path[n].Any(loc => goals.Contains(loc))) return n;
        return 0;
      }

      {
      var path_pt = GetMinStepPath<Location>();
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: pre-existing location path: "+(path_pt?.to_s() ?? "null") );
#endif
      if (null != path_pt) {
        if (GetPreviousGoals().ValueEqual(goals) || 0< path_contains_our_goals_loc(path_pt)) {
              var alt_act = UsePreexistingPath(goals);
              if (null != alt_act) _recordPathfinding(alt_act, goals);
        }
#if TRACE_GOALS
        if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "BehaviorPathTo: pre-existing location path did not contain any goals: "+goals.to_s());
#endif
      }
      }

#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "main exit: "+goal_costs.to_s()+", "+excluded.to_s());
#endif
      _current_goals = goals;
      return _recordPathfinding(BehaviorPathTo(PathfinderFor(goal_costs,excluded)),goals);
    }

    public void GoalHeadFor(Map m, HashSet<Point> dest)
    {
      Objectives.Insert(0, new Goal_PathTo(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, dest.Select(pt => new Location(m, pt))));
    }

#region damage field
    private void VisibleMaximumDamage(Dictionary<Point, int> ret,List<Actor> slow_melee_threat, HashSet<Actor> immediate_threat)
    {
      if (null == m_Actor) return;
      if (null == m_Actor.Location.Map) return;    // Duckman
      var enemies = m_Actor.Controller.enemies_in_FOV;
      if (null == enemies) return;
      Map map = m_Actor.Location.Map;
      foreach(var where_enemy in enemies) {
        Actor a = where_enemy.Value;
        int a_turns = m_Actor.HowManyTimesOtherActs(1,a);
        if (a.CanRun() && a.RunIsFreeMove) a_turns++;
        int a_turns_bak = a_turns;
        if (0 >= a_turns) continue; // morally if (!a.CanActNextTurn) continue;
        if (0==a.CurrentRangedAttack.Range && Rules.IsAdjacent(m_Actor.Location, where_enemy.Key) && m_Actor.Speed>a.Speed) slow_melee_threat.Add(a);
        // calculate melee damage field now
        Dictionary<Point,int> melee_damage_field = new Dictionary<Point,int>();
        int a_max_dam;
        if (Actor.STAMINA_MIN_FOR_ACTIVITY <= a.StaminaPoints) {
          a_max_dam = a.MeleeAttack(m_Actor).DamageValue;
          foreach(var dir in Direction.COMPASS) {
            var pt = a.Location.Position + dir;
            if (map.GetTileModelAtExt(pt)?.IsWalkable ?? false) melee_damage_field.Add(pt, a_turns * a_max_dam);
          }
          while(1<a_turns) {
            HashSet<Point> sweep = new HashSet<Point>(melee_damage_field.Keys);
            a_turns--;
            foreach(Point pt2 in sweep) {
              foreach(var dir in Direction.COMPASS) {
                var pt = pt2 + dir;
                if (sweep.Contains(pt)) continue;
                if (map.GetTileModelAtExt(pt)?.IsWalkable ?? false) melee_damage_field[pt] = a_turns * a_max_dam;
              }
            }
          }
          if (melee_damage_field.ContainsKey(m_Actor.Location.Position)) immediate_threat.Add(a);
        }
        // we can do melee attack damage field without FOV
        // FOV doesn't matter without a ranged attack
        // XXX doesn't handle non-optimal ranged attacks
        Dictionary<int,Attack> ranged_attacks = (a.Controller as ObjectiveAI)?.GetBestRangedAttacks(m_Actor);

        if (null == ranged_attacks) {
          foreach(var pt_dam in melee_damage_field) {
            if (ret.ContainsKey(pt_dam.Key)) ret[pt_dam.Key] += pt_dam.Value;
            else ret[pt_dam.Key] = pt_dam.Value;
          }
          continue;
        }

        // just directly recalculate FOV if needed, to avoid problems with newly spawned actors
        HashSet<Point> aFOV = LOS.ComputeFOVFor(a);
        // maximum melee damage: a.MeleeAttack(m_Actor).DamageValue
        // maximum ranged damage: a.CurrentRangedAttack.DamageValue
        Dictionary<Point,int> ranged_damage_field = new Dictionary<Point,int>();
        a_turns = a_turns_bak;
        foreach(Point pt in aFOV) {
          if (pt == a.Location.Position) continue;
          int dist = Rules.GridDistance(in pt, a.Location.Position);
          if (ranged_attacks.TryGetValue(dist,out var att)) {
            a_max_dam = ranged_attacks[dist].DamageValue;
            ranged_damage_field[pt] = a_turns*a_max_dam;
          }
        }
        if (1<a_turns) {
          HashSet<Point> already = new HashSet<Point>();
          HashSet<Point> now = new HashSet<Point>{ a.Location.Position };
          do {
            a_turns--;
            var tmp2 = a.NextStepRange(a.Location.Map,already,now);
            if (null == tmp2) break;
            foreach(Point pt2 in tmp2) {
              aFOV = LOS.ComputeFOVFor(a,new Location(a.Location.Map,pt2));
              aFOV.ExceptWith(ranged_damage_field.Keys);
              foreach(Point pt in aFOV) {
                int dist = Rules.GridDistance(in pt, a.Location.Position);
                if (ranged_attacks.TryGetValue(dist,out var att)) {
                  a_max_dam = ranged_attacks[dist].DamageValue;
                  ranged_damage_field[pt] = a_turns*a_max_dam;
                }
              }
            }
            already.UnionWith(now);
            now = tmp2;
          } while(1<a_turns);
        }
        if (ranged_damage_field.ContainsKey(m_Actor.Location.Position)) immediate_threat.Add(a);
        // ranged damage field should be a strict superset of melee in typical cases (exception: basement without flashlight)
        foreach(var pt_dam in ranged_damage_field) {
          if (melee_damage_field.TryGetValue(pt_dam.Key,out int prior_dam)) {
            if (ret.ContainsKey(pt_dam.Key)) ret[pt_dam.Key] += Math.Max(pt_dam.Value, prior_dam);
            else ret[pt_dam.Key] = Math.Max(pt_dam.Value, prior_dam);
          } else {
            if (ret.ContainsKey(pt_dam.Key)) ret[pt_dam.Key] += pt_dam.Value;
            else ret[pt_dam.Key] = pt_dam.Value;
          }
        }
      }
    }

#nullable enable
    private void AddExplosivesToDamageField(List<Percept_<Inventory>>? goals)
    {
      if (null == goals) return;
      IEnumerable<Percept_<ItemPrimedExplosive>> explosives = goals.Select(p => new Percept_<ItemPrimedExplosive>((p.Percepted as Inventory).GetFirst<ItemPrimedExplosive>(), p.Turn, p.Location));
      foreach (Percept_<ItemPrimedExplosive> exp in explosives) {
        BlastAttack tmp_blast = exp.Percepted.Model.BlastAttack;
        Point pt = exp.Location.Position;
        if (_damage_field.ContainsKey(pt)) _damage_field[pt] += tmp_blast.Damage[0];
        else _damage_field[pt] = tmp_blast.Damage[0];
        _blast_field.Add(pt);
        // We would need a very different implementation for large blast radii.
        short r = 0;
        while (++r <= tmp_blast.Radius) {
          foreach (Point p in Enumerable.Range(0, 8 * r).Select(i => exp.Location.Position.RadarSweep(r, i))) {
            if (!exp.Location.Map.IsValid(p)) continue;
            if (!LOS.CanTraceFireLine(exp.Location, p, tmp_blast.Radius)) continue;
            _blast_field.Add(p);
            if (_damage_field.ContainsKey(p)) _damage_field[p] += tmp_blast.Damage[r];
            else _damage_field[p] = tmp_blast.Damage[r];
          }
        }
      }
    }

    private void AddExplosivesToDamageField(List<Percept>? percepts)
    {
      AddExplosivesToDamageField(percepts?.FilterCast<Inventory>(inv => inv.Has<ItemPrimedExplosive>()));
    }

    private void AddTrapsToDamageField(Dictionary<Point,int> damage_field, List<Percept>? percepts)
    {
      var goals = percepts?.FilterCast<Inventory>(inv => inv.Has<ItemTrap>());
      if (null == goals) return;
      foreach(var p in goals) {
        if (p.Location==m_Actor.Location) continue; // trap has already triggered, or not: safe
        if (p.Location.Map!=m_Actor.Location.Map) continue; // XXX will be misrecorded
        var tmp = p.Percepted.GetItemsByType<ItemTrap>();
#if DEBUG
        if (null == tmp) throw new InvalidProgramException("inventory thought to have trap, didn't");
#endif
        int damage = tmp.Sum(trap => ((trap.IsActivated && !trap.IsSafeFor(m_Actor)) ? trap.Model.Damage : 0));   // XXX wrong for barbed wire
        if (0 >= damage) continue;
        if (damage_field.ContainsKey(p.Location.Position)) damage_field[p.Location.Position] += damage;
        else damage_field[p.Location.Position] = damage;
      }
    }
#nullable restore
#endregion

    public void Terminate(Actor a)
    {
#if DEBUG
      if (CombatUnready()) throw new InvalidOperationException("cannot consider terminate order when combat unready");
      if (m_Actor.Inventory.IsEmpty) throw new ArgumentNullException(nameof(m_Actor));
#endif
      var test = Goal<Goal_Terminate>();
      if (null==test) {
        Objectives.Add(new Goal_Terminate(m_Actor.Location.Map.LocalTime.Tick,m_Actor,a));
      } else test.NewTarget(a);
    }

    public void Avoid(Exit e)
    {
        var veto_map = Goal<Goals.BlacklistExits>();
        if (null != veto_map) veto_map.Blacklist(e);
        else Objectives.Add(new Goals.BlacklistExits(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, e));
    }

    public bool InCommunicationWith(Actor a)
    {
      if (m_Actor==a) return true;
      if (!(a.Controller is OrderableAI) && !(a.Controller is PlayerController)) return false;
      if (a.IsSleeping) return false;
      if (a.Controller.CanSee(m_Actor.Location) && m_Actor.Controller.CanSee(a.Location)) return true;
      if (a.HasActivePoliceRadio && m_Actor.HasActivePoliceRadio) return RogueGame.POLICE_RADIO_RANGE >= Rules.GridDistance(Rules.PoliceRadioLocation(m_Actor.Location),Rules.PoliceRadioLocation(a.Location));
      if (a.HasActiveArmyRadio && m_Actor.HasActiveArmyRadio) return RogueGame.POLICE_RADIO_RANGE >= Rules.GridDistance(Rules.PoliceRadioLocation(m_Actor.Location), Rules.PoliceRadioLocation(a.Location));
      if (null!=a.GetEquippedCellPhone() && null!=m_Actor.GetEquippedCellPhone()) return true;
      return false;
    }

    public void RecruitHelp(Actor enemy) {
#if DEBUG
      if (enemy?.IsDead ?? true) throw new ArgumentNullException(nameof(enemy));
#endif
      // message leader and followers, but *not* mates (chain of command) in communication by any means
      // message friends that can see us
      bool is_available(Actor a, ReactionCode priority) {
        if (null == a) return false;
        if (a.IsDead) return false;
        if (!(a.Controller is ObjectiveAI test_ai)) return false;
        if (!InCommunicationWith(a)) return false;
        if (!a.IsEnemyOf(enemy)) return false;
        return !test_ai.IsDistracted(priority);
      }

      var responders = new List<Actor>();
      var test_actor = m_Actor.LiveLeader;
      if (is_available(test_actor, ReactionCode.ENEMY)) responders.Add(test_actor);
      if (0<m_Actor.CountFollowers) {
        foreach(Actor fo in m_Actor.Followers) {
          if (is_available(fo, ReactionCode.ENEMY)) responders.Add(fo);
        }
      }
      // XXX should be inverse-visibility
      if (null!= friends_in_FOV) {
        foreach(var x in friends_in_FOV) {
          if (!responders.Contains(x.Value) && is_available(x.Value, ReactionCode.ENEMY)) responders.Add(x.Value);
        }
      }
      // \todo recruit allies by radio if needed
      // \todo filter responders by tactical requirements (goes in before recruiting allies by radio)
      foreach(var a in responders) {
        if (!(a.Controller is ObjectiveAI test_ai)) continue;  // invariant failure
        if (!test_ai.CombatUnready()) {   // part of the planned filter testing (need to allow combat-unready ais to participate if relevant)
          test_ai.Terminate(enemy);
        }
      }
    }

    protected void AdviseFriendsOfSafety()
    {
#if DEBUG
      if (null != enemies_in_FOV) throw new InvalidOperationException("not really safe");
#endif
      var observers = new Dictionary<Actor, ThreatTracking>();
      var friends = friends_in_FOV;
      if (null != friends) {
        foreach(var pos_fr in friends) {
          Actor friend = pos_fr.Value;
          ThreatTracking ally_threat = friend.Threats;
          if (null == ally_threat || m_Actor.Threats == ally_threat) continue;
          if (!InCommunicationWith(friend)) continue;
          observers[friend] = ally_threat;
        }
      }
      HashSet<Actor> allies = m_Actor.Allies; // XXX thrashes garbage collector, possibly should be handled by LoS sensor for the leader only?
      if (null != allies) {
        foreach(Actor friend in allies) {
          ThreatTracking ally_threat = friend.Threats;
          if (null == ally_threat || m_Actor.Threats == ally_threat) continue;
          if (!InCommunicationWith(friend)) continue;
          observers[friend] = ally_threat;
        }
      }
      // but this won't trigger if any of our friends are mutual enemies
      if (0<observers.Count) {
        foreach(KeyValuePair<Actor,ThreatTracking> wary in observers) {
          if (!wary.Key.AnyEnemiesInFov(FOV)) wary.Value.Cleared(m_Actor.Location.Map,FOV);
        }
      }
    }

    protected void AdviseCellOfInventoryStacks(List<Percept> stacks)
    {
#if DEBUG
      if (0 >= (stacks?.Count ?? 0)) throw new ArgumentNullException(nameof(stacks));
#endif
      var cell = m_Actor.ChainOfCommand;  // Cf. Robert Heinlein, "The Moon is a Harsh Mistress"
      if (null == cell) return;
      foreach(Actor ally in cell) {
        if (!(ally.Controller is OrderableAI ai)) continue;
        if (!InCommunicationWith(ally)) continue;
        var ai_items = ai.ItemMemory;
        if (null != ai_items) {
          if (null!= ItemMemory && ItemMemory == ai_items) continue; // already updated
          foreach (Percept p in stacks) {
            ai_items.Set(p.Location, new HashSet<Gameplay.GameItems.IDs>((p.Percepted as Inventory).Items.Select(x => x.Model.ID)), p.Location.Map.LocalTime.TurnCounter);
          }
          continue; // followers with item memory can decide on their own what to do
        }
        var track_inv = ai.Goal<Goal_PathToStack>();
        foreach(Percept p in stacks) {
          if (m_Actor.Location != p.Location && ai.CanSee(p.Location)) continue;
          if (!ai.WouldGrabFromStack(p.Location, p.Percepted as Inventory)) continue;

          if (null == track_inv) {
            track_inv = new Goal_PathToStack(ally.Location.Map.LocalTime.TurnCounter,ally,p.Location);
            ai.Objectives.Add(track_inv);
          } else track_inv.newStack(p.Location);
        }
      }
    }

#nullable enable
    abstract protected ActorAction? BehaviorWouldGrabFromStack(in Location loc, Inventory? stack);

    public ActorAction? WouldGetFrom(MapObject? obj)
    {
      if (null == obj || !obj.IsContainer) return null;
      var stack = obj.Inventory;
      if (null == stack || stack.IsEmpty) return null;
      return BehaviorWouldGrabFromStack(obj.Location, stack);
    }
#nullable restore

    protected Dictionary<Location, Inventory> GetInterestingInventoryStacks(Predicate<Inventory> want_now)   // technically could be ActorController
    {
      var items = items_in_FOV;
      if (null == items) return null;

      // following needs to be more sophisticated.
      // 1) identify all stacks, period.
      // 2) categorize stacks by whether they are personally interesting or not.
      // 3) in-communication followers will be consulted regarding the not-interesting stacks
      Map map = m_Actor.Location.Map;
      var examineStacks = new Dictionary<Location,Inventory>(items.Count);
      { // scoping brace
      var boringStacks = new List<Percept>(items.Count);
      int t0 = map.LocalTime.TurnCounter;
      foreach(var x in items) {
        if (!want_now(x.Value)) continue;   // not immediately relevant
        if (m_Actor.StackIsBlocked(x.Key)) continue; // XXX ignore items under barricades or fortifications
        if (!m_Actor.CanEnter(x.Key)) continue;    // XXX ignore buggy stack placement
        if (!BehaviorWouldGrabFromStack(x.Key, x.Value)?.IsLegal() ?? true) {
          boringStacks.Add(new Percept(x.Value, t0, x.Key));
          continue;
        }
        examineStacks.Add(x.Key,x.Value);
      }
      if (0 < boringStacks.Count) AdviseCellOfInventoryStacks(boringStacks);    // XXX \todo PC leader should do the same
      } // end scoping brace
      if (0 >= examineStacks.Count) return null;

      bool imStarvingOrCourageous = m_Actor.IsStarving;
      if ((this is OrderableAI ai) && ActorCourage.COURAGEOUS == ai.Directives.Courage) imStarvingOrCourageous = true;
      var ret = new Dictionary<Location,Inventory>(examineStacks.Count);
      foreach(var x in examineStacks) {
        if (IsOccupiedByOther(x.Key)) continue; // blocked
        if (!m_Actor.MayTakeFromStackAt(x.Key)) {
            if (!imStarvingOrCourageous && 1>=m_Actor.Controller.FastestTrapKill(x.Key)) continue;  // destination deathtrapped
            // check for iron gates, etc in way
            var path = m_Actor.MinStepPathTo(m_Actor.Location, x.Key);
            if (null == path) continue;
            if (!path[0].Any(pt => null != Rules.IsBumpableFor(m_Actor, new Location(map, pt)))) continue;
        }
        ret.Add(x.Key,x.Value);
      }
      return 0<ret.Count ? ret : null;
    }

    // XXX to implement
    // core inventory should be (but is not)
    // armor: 1 slot (done)
    // flashlight: 1 slot (currently very low priority)
    // melee weapon: 1 slot (done)
    // ranged weapon w/ammo: 1 slot
    // ammo clips: 1 slot high priority, 1 slot moderate priority (tradeable)
    // without Hauler levels, that is 5 non-tradeable slots when fully kitted
    // Also, has enough food checks should be based on wakeup time

    // Gun bunnies would:
    // * have a slot budget of MaxCapacity-3 or -4 for ranged weapons and ammo combined
    // * use no more than half of that slot budget for ranged weapons, rounded up
    // * strongly prefer one clip for each of two ranged weapons over 2 clips for a single ranged weapon

    // close to the inverse of IsInterestingItem
    public bool IsTradeableItem(Item it)
    {
#if DEBUG
        if (null == it) throw new ArgumentNullException(nameof(it));
        if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException(m_Actor.Name+" cannot trade");
#endif
        if (it is ItemBodyArmor) return !it.IsEquipped; // XXX best body armor should be equipped
        if (it is ItemFood food)
            {
            if (!m_Actor.Model.Abilities.HasToEat) return true;
            if (m_Actor.IsHungry) return false;
            // only should trade away food that doesn't drop below threshold
            if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food))
              return food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter);
            return true;
            }
        if (it is ItemRangedWeapon rw)
            {
            if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return true;
            if (0 < rw.Ammo) return false;
            if (null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return false;
            return true;    // more work needed
            }
        if (it is ItemAmmo am)
            {
            if (m_Actor.Inventory.GetCompatibleRangedWeapon(am) == null) return true;
            return m_Actor.HasAtLeastFullStackOf(it, 2);
            }
        if (it is ItemMeleeWeapon melee)
            {
            if (m_Actor.MeleeWeaponAttack(melee.Model).Rating <= m_Actor.UnarmedMeleeAttack().Rating) return true;
            if (2<=m_Actor.Inventory.Count(it.Model)) return true;  // trading away a spare is ok
            // do not trade away the best melee weapon.  Others ok.
            return m_Actor.GetBestMeleeWeapon() != it;  // return value should not be null
            }
        if (it is ItemLight)
            {
            if (!m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 2)) return false;
            // XXX more work needed
            return true;
            }
        // player should be able to trade for blue pills
/*
        if (it is ItemMedicine)
            {
            return HasAtLeastFullStackOfItemTypeOrModel(it, 2);
            }
*/
        return true;    // default to ok to trade away
    }

#nullable enable
    public List<Item>? GetTradeableItems()
    {
      Inventory inv = m_Actor.Inventory;
      if (inv == null) return null;
      IEnumerable<Item> ret = inv.Items.Where(it => IsTradeableItem(it));
      return ret.Any() ? ret.ToList() : null;
    }
#nullable restore

    public Dictionary<Location, Actor> GetTradingTargets(Dictionary<Location,Actor> friends) // Waterfall Lifecycle: retain this parameter for contrafactual use
    {
        if (null == friends || 0 >= friends.Count) return null;
        if (!m_Actor.Model.Abilities.CanTrade) return null; // arguably an invariant but not all PCs are overriding appropriate base AIs
        var TradeableItems = GetTradeableItems();
        if (null == TradeableItems || 0>=TradeableItems.Count) return null;

        var ret = new Dictionary< Location, Actor >(friends.Count);
        foreach(var x in friends) {
          if (x.Value.IsPlayer) continue;
          if (this is OrderableAI ai && ai.IsActorTabooTrade(x.Value)) continue;
          if (!m_Actor.CanTradeWith(x.Value)) continue;
          if (null==m_Actor.MinStepPathTo(m_Actor.Location, x.Key)) continue;    // something wrong, e.g. iron gates in way.  Usual case is police visiting jail.
          if (1 == TradeableItems.Count) {
            var other_TradeableItems = (x.Value.Controller as OrderableAI).GetTradeableItems();
            if (null == other_TradeableItems) continue;
            if (1 == other_TradeableItems.Count && TradeableItems[0].Model.ID== other_TradeableItems[0].Model.ID) continue;
          }
          if (!(x.Value.Controller as OrderableAI).HasAnyInterestingItem(TradeableItems)) continue;
          if (!HaveTradeOptions(x.Value)) continue;
          ret.Add(x.Key,x.Value);
        }
        return 0<ret.Count ? ret : null;
    }

    public List<KeyValuePair<Item, Item>> TradeOptions(Actor target)
    {
      var wants = m_Actor.GetInterestingTradeableItems(target);  // charisma check involved for these
      if (null!=wants && 0 < wants.Count) return null;
      var offers = target.GetInterestingTradeableItems(m_Actor);
      if (null != offers && 0 < offers.Count) return null;
      var negotiate = new List<KeyValuePair<Item,Item>>(wants.Count*offers.Count);   // relies on "small" inventory to avoid arithmetic overflow
      foreach(var s_item in wants) {
        foreach(var b_item in offers) {
          if (TradeVeto(s_item,b_item)) continue;
          if (TradeVeto(b_item,s_item)) continue;
            // charisma can't do everything
            negotiate.Add(new KeyValuePair<Item,Item>(s_item,b_item));
          }
      }
      return 0<negotiate.Count ? negotiate : null;
    }

    public bool HaveTradeOptions(Actor target)
    {
      var wants = m_Actor.GetInterestingTradeableItems(target);  // charisma check involved for these
      if (null!=wants && 0 < wants.Count) return false;
      var offers = target.GetInterestingTradeableItems(m_Actor);
      if (null != offers && 0 < offers.Count) return false;
      foreach(var s_item in wants) {
        foreach(var b_item in offers) {
          if (TradeVeto(s_item,b_item)) continue;
          if (TradeVeto(b_item,s_item)) continue;
          return true;
        }
      }
      return false;
    }

    protected bool HaveTradingTargets()
    {
        if (!m_Actor.Model.Abilities.CanTrade) return false; // arguably an invariant but not all PCs are overriding appropriate base AIs
        if (null == friends_in_FOV) return false;
        var TradeableItems = GetTradeableItems();
        if (null == TradeableItems || 0 >= TradeableItems.Count) return false;

        foreach(var x in friends_in_FOV) {
          if (x.Value.IsDead) continue;
          if (x.Value.IsPlayer) continue;
          if (this is OrderableAI ai && ai.IsActorTabooTrade(x.Value)) continue;
          if (!m_Actor.CanTradeWith(x.Value)) continue;
          if (null==m_Actor.MinStepPathTo(m_Actor.Location, x.Value.Location)) continue;    // something wrong, e.g. iron gates in way.  Usual case is police visiting jail.
          if (1 == TradeableItems.Count) {
            var other_TradeableItems = (x.Value.Controller as OrderableAI).GetTradeableItems();
            if (null == other_TradeableItems) continue;
            if (1 == other_TradeableItems.Count && TradeableItems[0].Model.ID== other_TradeableItems[0].Model.ID) continue;
          }
          if (!(x.Value.Controller as OrderableAI).HasAnyInterestingItem(TradeableItems)) return false;    // other half of m_Actor.GetInterestingTradeableItems(...)
          return HaveTradeOptions(x.Value);
        }
        return false;
    }

    private ActorAction? _PrefilterDrop(Item it, bool use_ok=true)
    {
      if (use_ok) {
      // use stimulants before dropping them
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        if (m_Actor.Inventory.GetBestDestackable(it) is ItemMedicine stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = m_Actor.ScaleMedicineEffect(stim2.SleepBoost);
          if (num4 <= need &&  m_Actor.CanUse(stim2)) return new ActionUseItem(m_Actor, stim2);
        }
      }

      // reload weapons before dropping ammo
      { // scoping brace
      if (it is ItemAmmo ammo) {
        foreach(Item obj in m_Actor.Inventory.Items) {
          if (   obj is ItemRangedWeapon rw
              && rw.AmmoType==ammo.AmmoType
              && rw.Ammo < rw.Model.MaxAmmo) {
            rw.EquippedBy(m_Actor);
            return new ActionUseItem(m_Actor, ammo);
          }
        }
      }
// does not work: infinite recursion issue, too vague
#if PROTOTYPE
      if (!(it is ItemTrap)) {  // traps: try to use them explicitly
        var use_trap = new Gameplay.AI.Goals.SetTrap(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor);
        if (use_trap.UrgentAction(out var ret) && null!=ret) {
          Objectives.Insert(0, use_trap);
          return ret;
        }
      }
#endif
      } // end scoping brace
      } // if (use_ok)
      return null;
    }

    protected ActorAction? BehaviorDropItem(Item? it)
    {
      if (it == null) return null;
      var tmp = _PrefilterDrop(it);
      if (null != tmp) return tmp;

      var has_container = new Zaimoni.Data.Stack<MapObject>(new MapObject[8]);
      foreach(var dir in Direction.COMPASS) {
        var pos = m_Actor.Location.Position + dir;
        var container = Rules.CanActorPutItemIntoContainer(m_Actor, in pos);
        if (null == container) continue;
        var itemsAt = container.Inventory;
        if (null != itemsAt)
          {
          if (itemsAt.CountItems+1 >= itemsAt.MaxCapacity) continue; // practical consideration
#if DEBUG
          if (itemsAt.IsFull) throw new InvalidOperationException("illegal put into container attempted");
#endif
          }
        has_container.push(container);
      }
      if (0 < has_container.Count) return new ActionPutInContainer(m_Actor, it, Rules.Get.DiceRoller.Choose(has_container));

      return (m_Actor.CanDrop(it) ? new ActionDropItem(m_Actor, it) : null);
    }

    public bool ItemIsUseless(ItemModel it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
	  if (it.IsForbiddenToAI) return true;
	  if (it is ItemSprayPaintModel) return true;
      if (it is ItemGrenadePrimedModel) return true;    // XXX want a general primed explosive model test

      // only soldiers and civilians use grenades (CHAR guards are disallowed as a balance issue; unsure about why gangsters dont)
      if (GameItems.IDs.EXPLOSIVE_GRENADE == it.ID && !UsesExplosives) return true;

      // only civilians use stench killer
      if (GameItems.IDs.SCENT_SPRAY_STENCH_KILLER == it.ID && !(m_Actor.Controller is CivilianAI)) return true;

      // police have implicit police trackers
      if (GameItems.IDs.TRACKER_POLICE_RADIO == it.ID && !m_Actor.WantPoliceRadio) return true;
      if (GameItems.IDs.TRACKER_CELL_PHONE == it.ID && !m_Actor.WantCellPhone) return true;

      if (it is ItemFoodModel && !m_Actor.Model.Abilities.HasToEat) return true; // Soldiers and CHAR guards.  There might be a serum for this.
      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) {    // Bikers
        if (it is ItemRangedWeaponModel || it is ItemAmmoModel) return true;
      }

      return false;
    }

    // Would be the lowest priority level of an item, except that it conflates "useless to everyone" and "useless to me"
    public bool ItemIsUseless(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (ItemIsUseless(it.Model)) return true;

      if (it is ItemTrap trap && trap.IsActivated) return true;
      if (it.IsUseless || it is ItemPrimedExplosive) return true;
      if (it is ItemEntertainment ent && ent.IsBoringFor(m_Actor)) return true;
      Inventory inv;
      if (it is ItemRangedWeapon rw && 0==rw.Ammo && null == (inv = m_Actor.Inventory).GetCompatibleAmmoItem(rw) && inv.IsFull) return true;
      return false;
    }

    private int ItemRatingCode_generic(Item it)
    {
        Inventory inv = m_Actor.Inventory;
        if (!inv.Contains(it) && inv.HasAtLeastFullStackOf(it, 1)) return 0;
        return 1;
    }

    private int ItemRatingCode_generic(ItemModel it)
    {
        return m_Actor.HasAtLeastFullStackOf(it, 1) ? 0 : 1;
    }

    private int ItemRatingCode(ItemTracker it)
    {
      var ok_trackers = new Zaimoni.Data.Stack<GameItems.IDs>(stackalloc GameItems.IDs[2]);
      if (m_Actor.NeedActiveCellPhone) ok_trackers.push(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.push(GameItems.IDs.TRACKER_POLICE_RADIO);

      // AI does not yet use z-trackers or blackops trackers correctly; possible only threat-aware AIs use them
      Inventory inv;
      if ((inv = m_Actor.Inventory).Contains(it)) return (ok_trackers.Contains(it.Model.ID) && null!=m_Actor.LiveLeader) ? 2 : 1;
      if (inv.Items.Any(obj => !obj.IsUseless && obj.Model == it.Model)) return 0;
      return (ok_trackers.Contains(it.Model.ID) && null != m_Actor.LiveLeader) ? 2 : 1;
    }

    /// <returns>the item rating code for a generic sanity-restoring item</returns>
    public int WantRestoreSAN { get {   // arguably should be over at Actor
      if (!m_Actor.Model.Abilities.HasSanity) return 0;
      if (m_Actor.IsDisturbed) return 3;
      if (m_Actor.Sanity < 3 * m_Actor.MaxSanity / 4) return 2;   // gateway expression for using entertainment
      return 1;
    } }

    public int WantRestoreSLP { get {   // arguably should be over at Actor
      if (!m_Actor.Model.Abilities.HasToSleep) return 0;
      var hours_to_zzz = m_Actor.HoursUntilSleepy;
      if (3 >= hours_to_zzz) return 3;  // inlining Actor::IsAlmostSleepy
      if (6 >= hours_to_zzz) return 2;
      return 1;
    } }

    private int ItemRatingCode(ItemEntertainment it)
    {
      if (!m_Actor.Inventory.Contains(it) && m_Actor.HasAtLeastFullStackOf(it, 1)) return 0;
      return WantRestoreSAN;
    }

    private int ItemRatingCode(ItemLight it)
    {
      Inventory inv;
      if ((inv = m_Actor.Inventory).Contains(it)) return 2;
      if (inv.Has<ItemLight>(obj => !obj.IsUseless)) return 0; // XXX \todo fix; could want to swap out, for instance
      return 2;   // historically low priority but ideal kit has one
    }

    // XXX sleep and stamina pills have special uses for sufficiently good AI
    // XXX sanity pills should be treated like entertainment
    private int ItemRatingCode(ItemMedicine it)
    {
      // simulate historical usage (alpha 9)
      Inventory inv;
      if ((inv = m_Actor.Inventory).Contains(it)) return 1;
      if (inv.HasAtLeastFullStackOf(it, inv.IsFull ? 1 : 2)) return 0;
      if (0 < it.SanityCure) {  // we would need to account for side effects mainly for mods or "realism"
        var rating = WantRestoreSAN;
        if (1!=rating) return rating;
      }
      if (0 < it.SleepBoost) {
        var rating = WantRestoreSLP;
        if (1!=rating) return rating;
      }
      return 1;
    }

    private int ItemRatingCode(ItemBodyArmor armor)
    {
      var best = m_Actor.GetBestBodyArmor();
      if (null == best) return 3; // want 3, but historically RHSMoreInteresting  says 2
      if (best == armor) return 3;
      return best.Rating < armor.Rating ? 2 : 0; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
    }

    private int ItemRatingCode(ItemMeleeWeapon melee)
    {
      int rating = m_Actor.MeleeWeaponAttack(melee.Model).Rating;
      if (rating <= m_Actor.UnarmedMeleeAttack().Rating) return 0;
      var best = m_Actor.GetBestMeleeWeapon();
      if (null == best) return 2;  // martial arts invalidates starting baton for police
      int best_rating = m_Actor.MeleeWeaponAttack(best.Model).Rating;    // rely on OrderableAI doing the right thing
      if (best_rating < rating) return 2;
      if (best_rating > rating) return 1;
      if (melee == best) return 2;
      int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
      if (m_Actor.Inventory.Contains(melee)) return 1 == melee_count ? 2 : 1;
      if (2 <= melee_count) {
        var worst = m_Actor.GetWorstMeleeWeapon();
        return m_Actor.MeleeWeaponAttack(worst.Model).Rating < rating ? 1 : 0;
      }
      return 1;
    }

    private int ItemRatingCode(ItemFood food)
    {
//    if (!m_Actor.Model.Abilities.HasToEat) return 0;    // redundant; for documentation
      if (m_Actor.IsHungry) return 3;
      if (food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter)) return 0;
      // if the preemptive eat behavior would trigger, that is 3.
      // XXX \todo account for travel tiem
      if (food.IsPerishable && (m_Actor.CurrentNutritionOf(food) <= (m_Actor.MaxFood - m_Actor.FoodPoints))) return 3;
      if (m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food)) return 1;
      return 2;
    }

    private bool KnowRelevantInventory(ItemAmmo am)
    {
      // second opinion...if we know where a suitable rw, close by, then elevate priority
      var track_inv = Goal<Goal_PathToStack>();
      if (null != track_inv) {
        foreach(Inventory inv in track_inv.Inventories) {
          if (inv.IsEmpty) continue;
          if (null != inv.GetCompatibleRangedWeapon(am)) return true;
        }
      }
      return false;
    }

    private int ItemRatingCode(ItemAmmo am)
    {
      Inventory inv = m_Actor.Inventory;
      bool is_in_inventory = inv.Contains(am);

      var rw = inv.GetCompatibleRangedWeapon(am);
      if (null == rw) {
        int potential_importance = KnowRelevantInventory(am) ? 2 : 1;
        if (is_in_inventory) return potential_importance;
        if (0 < inv.Count(am.Model)) return 0;
        if (AmmoAtLimit) return int.MaxValue;  // BehaviorMakeRoomFor triggers recursion. real value 0 or potential_importance
        return potential_importance;
      }
      if (is_in_inventory) return 2;
      if (rw.Ammo < rw.Model.MaxAmmo) return 2;
      if (inv.HasAtLeastFullStackOf(am, 2)) return 0;
      if (null != inv.GetFirstByModel<ItemAmmo>(am.Model,am2=>am2.Quantity<am.Model.MaxQuantity)) return 2;
      if (AmmoAtLimit) return int.MaxValue;  // BehaviorMakeRoomFor triggers recursion. real value 0 or 2
      return 2;
    }

    private int ItemRatingCode(ItemRangedWeapon rw)
    { // similar to IsInterestingItem(rw)
      Inventory inv = m_Actor.Inventory;
      if (inv.Contains(rw)) return 0<rw.Ammo ? 3 : 1;
      int rws_w_ammo = inv.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo);
      if (0 < rws_w_ammo) {
        if (null != inv.GetFirstByModel<ItemRangedWeapon>(rw.Model, obj => 0 < obj.Ammo)) return 0;    // XXX
        if (null != inv.GetFirst<ItemRangedWeapon>(obj => obj.AmmoType==rw.AmmoType && 0 < obj.Ammo)) return 0; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
      }
      if (0 < rw.Ammo && null != inv.GetFirstByModel<ItemRangedWeapon>(rw.Model, obj => 0 == obj.Ammo)) return 3;  // this replacement is ok; implies not having ammo
      var compatible = inv.GetCompatibleAmmoItem(rw);
      if (null == compatible) {
        if (0 >= rw.Ammo) return 1;
        // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
        // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
        if (AmmoAtLimit) return 0;
      } else {
        if (0 >= rws_w_ammo) return 3;
      }
      if (0< rws_w_ammo) return 2;
      return 3;
    }

    private int ItemRatingCode(ItemGrenade grenade)
    {
      Inventory inv = m_Actor.Inventory;
      if (inv.Contains(grenade)) return inv.HasAtLeastFullStackOf(grenade, 1) ? 1 : 2;  // \todo fix not quite...stack of 6 w/no spares should be 2
      if (inv.IsFull) return 1;
      if (inv.HasAtLeastFullStackOf(grenade, 1)) return 1;
      return 2;
    }

    // XXX should be an enumeration
    // 0: useless
    // 1: insurance
    // 2: want
    // 3: need
    protected int ItemRatingCode(Item it, Location? loc=null)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (ItemIsUseless(it)) return 0;

      if (it is ItemTracker tracker) return ItemRatingCode(tracker);
      if (it is ItemEntertainment ent) return ItemRatingCode(ent);
      if (it is ItemLight light) return ItemRatingCode(light);
      if (it is ItemMedicine med) return ItemRatingCode(med);
      if (it is ItemBodyArmor armor) return ItemRatingCode(armor);
      if (it is ItemMeleeWeapon melee) return ItemRatingCode(melee);
      if (it is ItemFood food) return ItemRatingCode(food);
      if (it is ItemRangedWeapon rw) return ItemRatingCode(rw);
      if (it is ItemAmmo am) {
        int ret = ItemRatingCode(am);
        if (int.MaxValue == ret) {
          if (null == BehaviorMakeRoomFor(it)) return 0;  // BehaviorMakeRoomFor triggers recursion
          return null!=m_Actor.Inventory.GetCompatibleRangedWeapon(am) || KnowRelevantInventory(am) ? 2 : 1;
        }
        return ret;
      }
      if (it is ItemGrenade grenade) return ItemRatingCode(grenade);

      if (it is ItemBarricadeMaterial) return ItemRatingCode_generic(it);
      if (it is ItemTrap trap) {
        if (trap.IsActivated) return 0;
        return ItemRatingCode_generic(it);
      }
      if (it is ItemSprayScent) return ItemRatingCode_generic(it);

      return 1;
    }

    protected int ItemRatingCode_no_recursion(Item it, Location? loc=null)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (ItemIsUseless(it)) return 0;

      if (it is ItemTracker tracker) return ItemRatingCode(tracker);
      if (it is ItemEntertainment ent) return ItemRatingCode(ent);
      if (it is ItemLight light) return ItemRatingCode(light);
      if (it is ItemMedicine med) return ItemRatingCode(med);
      if (it is ItemBodyArmor armor) return ItemRatingCode(armor);
      if (it is ItemMeleeWeapon melee) return ItemRatingCode(melee);
      if (it is ItemFood food) return ItemRatingCode(food);
      if (it is ItemRangedWeapon rw) return ItemRatingCode(rw);
      if (it is ItemAmmo am) {
        int ret = ItemRatingCode(am);
        if (int.MaxValue == ret) return 0; // BehaviorMakeRoomFor triggers recursion
        return ret;
      }
      if (it is ItemGrenade grenade) return ItemRatingCode(grenade);

      if (it is ItemBarricadeMaterial) return ItemRatingCode_generic(it);
      if (it is ItemTrap) return ItemRatingCode_generic(it);
      if (it is ItemSprayScent) return ItemRatingCode_generic(it);

      return 1;
    }

    // this variant should only be used on targets not in inventory
    // evaluations based on item location knowledge shouldn't reach here (that is,
    // there are cases working off of item model ID that do not belong here)
    private int ItemRatingCode(ItemModel it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
#endif
      if (ItemIsUseless(it)) return 0;

      if (it is ItemBarricadeMaterialModel) return ItemRatingCode_generic(it);
      if (it is ItemTrapModel) return ItemRatingCode_generic(it);
      if (it is ItemSprayScentModel) return ItemRatingCode_generic(it);

      {
      if (it is ItemTrackerModel) {
        var ok_trackers = new Zaimoni.Data.Stack<GameItems.IDs>(stackalloc GameItems.IDs[2]);
        if (m_Actor.NeedActiveCellPhone) ok_trackers.push(GameItems.IDs.TRACKER_CELL_PHONE);
        if (m_Actor.NeedActivePoliceRadio) ok_trackers.push(GameItems.IDs.TRACKER_POLICE_RADIO);
        // AI does not yet use z-trackers or blackops trackers correctly; possible only threat-aware AIs use them
        if (m_Actor.Inventory.Items.Any(obj => !obj.IsUseless && obj.Model == it)) return 0;
        return (ok_trackers.Contains(it.ID) && null != m_Actor.LiveLeader) ? 2 : 1;
      }

      if (it is ItemEntertainmentModel) {
        if (!m_Actor.Model.Abilities.HasSanity) return 0;
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return 0;
        if (m_Actor.IsDisturbed) return 3;
        if (m_Actor.Sanity < 3 * m_Actor.MaxSanity / 4) return 2;   // gateway expression for using entertainment
        return 1;
      }
      {
      if (it is ItemLightModel) {
        if (m_Actor.Inventory.Items.Any(obj => !obj.IsUseless && obj is ItemLight)) return 0;
        return 2;   // historically low priority but ideal kit has one
      }
      }
      // XXX note that sleep and stamina have special uses for sufficiently good AI
      if (it is ItemMedicineModel) {
        int needHP = m_Actor.MaxHPs- m_Actor.HitPoints;
        if (GameItems.IDs.MEDICINE_MEDIKIT == it.ID || GameItems.IDs.MEDICINE_BANDAGES == it.ID) {
          if (needHP >= m_Actor.ScaleMedicineEffect(GameItems.MEDIKIT.Healing)) return 3;    // second aid
        }

        if (GameItems.IDs.MEDICINE_BANDAGES == it.ID && needHP >= m_Actor.ScaleMedicineEffect(GameItems.BANDAGE.Healing)) {
          return 2; // first aid
        }

        if (m_Actor.HasAtLeastFullStackOf(it, m_Actor.Inventory.IsFull ? 1 : 2)) return 0;
        return 1;
      }
      {
      if (it is ItemBodyArmorModel armor) {
        var best = m_Actor.GetBestBodyArmor();
        if (null == best) return 2; // want 3, but RHSMoreInteresting  says 2
        return best.Rating < armor.Rating ? 2 : 0; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
      }
      }
      {
      if (it is ItemMeleeWeaponModel melee) {
        int rating = m_Actor.MeleeWeaponAttack(melee).Rating;
        if (rating <= m_Actor.UnarmedMeleeAttack().Rating) return 0;
        int? best_rating = m_Actor.GetBestMeleeWeaponRating();    // rely on OrderableAI doing the right thing
        if (null == best_rating) return 2;  // martial arts invalidates starting baton for police
        if (best_rating < rating) return 2;
        if (best_rating > rating) return 1;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
        if (2 <= melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return m_Actor.MeleeWeaponAttack(worst.Model).Rating < rating ? 1 : 0;
        }
        return 1;
      }
      }
      {
      if (it is ItemFoodModel) {
//      if (!m_Actor.Model.Abilities.HasToEat) return false;    // redundant; for documentation
        if (m_Actor.IsHungry) return 3;
        // we don't do the pre-emptive eat test here due to lack of information re expiration date
        if (m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2)) return 1;
        return 2;
      }
      }

      { // similar to IsInterestingItem(ItemAmmo)
      if (it is ItemAmmoModel am) {
        ItemRangedWeapon rw = m_Actor.Inventory.GetCompatibleRangedWeapon(am);
        if (null == rw) return 0 < m_Actor.Inventory.Count(am) ? 0 : 1;

        if (null == m_Actor.Inventory.GetCompatibleAmmoItem(rw) && !AmmoAtLimit) return 3;

        if (rw.Ammo < rw.Model.MaxAmmo) return 2;
        if (m_Actor.HasAtLeastFullStackOf(am, 2)) return 0;
        if (null != m_Actor.Inventory.GetFirstByModel<ItemAmmo>(am,am2=>am2.Quantity<am.MaxQuantity)) return 2;
        if (AmmoAtLimit) return 0;  // doesn't completely work yet
        return 2;
      }
      }
      { // similar to IsInterestingItem(rw)
      if (it is ItemRangedWeaponModel rw) return ItemRatingCode(rw.Example);
      }
      {
      if (it is ItemGrenadeModel grenade) {
        if (m_Actor.Inventory.IsFull) return 1;
        if (m_Actor.HasAtLeastFullStackOf(grenade, 1)) return 1;
        return 2;
      }
      }

      return 1;
    }
    }

    private int ItemRatingCode(GameItems.IDs x)
    {
       // \todo location-based inferences
       return ItemRatingCode(Models.Items[(int)x]);
    }

    protected void ReviewItemRatings()
    {
      int i = (int)GameItems.IDs._COUNT;
      while(0 < i--) {
        ItemPriorities[i] = (sbyte)ItemRatingCode((GameItems.IDs)i);
      }
    }

    public int RatingCode(GameItems.IDs x)
    {
      return ItemPriorities[(int)x];
    }

    protected ActorAction? BehaviorDropUselessItem() // XXX would be convenient if this were fast-failing
    {
      Inventory inv = m_Actor.Inventory;
      if (inv.IsEmpty) return null;
      foreach (Item it in inv.Items) {
        if (ItemIsUseless(it)) return BehaviorDropItem(it); // allows recovering cleanly from bugs and charismatic trades
      }

      // strict domination checks
      var armor = m_Actor.GetWorstBodyArmor();
      if (null != armor && 2 <= m_Actor.CountQuantityOf<ItemBodyArmor>()) return BehaviorDropItem(armor);

      ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
      if (null != weapon) {
        if (m_Actor.MeleeWeaponAttack(weapon.Model).Rating <= m_Actor.UnarmedMeleeAttack().Rating) return BehaviorDropItem(weapon);
      }

      ItemRangedWeapon rw = inv.GetFirstMatching<ItemRangedWeapon>(it => 0==it.Ammo && 2<=m_Actor.Count(it.Model));
      if (null != rw) return BehaviorDropItem(rw);

#if FALSE_POSITIVE
      if (m_Actor.Inventory.MaxCapacity-5 <= m_Actor.Inventory.CountType<ItemAmmo>()) {
        if (0 < m_Actor.Inventory.CountType<ItemRangedWeapon>()) {
          ItemAmmo am = m_Actor.Inventory.GetFirstMatching<ItemAmmo>(it => null == m_Actor.Inventory.GetCompatibleRangedWeapon(it));
          if (null != am) return BehaviorDropItem(am);
        }
      }
#endif

      return null;
    }

#nullable enable
    private ActorAction? _BehaviorDropOrExchange(Item give, Item take, Point? position, bool use_ok=true)
    {
      if (give.Model.IsStackable) give = m_Actor.Inventory.GetBestDestackable(give);    // should be non-null
      var tmp = _PrefilterDrop(give, use_ok);
      if (null != tmp) return tmp;
      if (null != position) return ActionTradeWith.Cast(position.Value, m_Actor, give, take);
      return BehaviorDropItem(give);
    }
#nullable restore

    protected bool RHSMoreInteresting(Item lhs, Item rhs)
    {
#if DEBUG
      if (null == lhs) throw new ArgumentNullException(nameof(lhs));
      if (null == rhs) throw new ArgumentNullException(nameof(rhs));
      if (!m_Actor.Inventory.Contains(lhs) && !IsInterestingItem(lhs)) throw new InvalidOperationException(lhs.ToString()+" not interesting to "+m_Actor.Name);
      if (!m_Actor.Inventory.Contains(rhs) && !IsInterestingItem(rhs)) throw new InvalidOperationException(rhs.ToString()+" not interesting to "+m_Actor.Name);
#endif
      if (lhs.Model.ID == rhs.Model.ID) {
        if (lhs.Quantity < rhs.Quantity) return true;
        if (lhs.Quantity > rhs.Quantity) return false;
        if (lhs is BatteryPowered lhs_batt) return (lhs_batt.Batteries < (rhs as BatteryPowered).Batteries);
        else if (lhs is ItemFood lhs_food && lhs_food.IsPerishable)
          { // complicated
          int need = m_Actor.MaxFood - m_Actor.FoodPoints;
          int lhs_nutrition = lhs_food.NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter);
          int rhs_nutrition = (rhs as ItemFood).NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter);
          if (lhs_nutrition==rhs_nutrition) return false;
          if (need < lhs_nutrition && need >= rhs_nutrition) return true;
          if (need < rhs_nutrition && need >= lhs_nutrition) return false;
          return lhs_nutrition < rhs_nutrition;
          }
        else if (lhs is ItemRangedWeapon lhs_rw) return (lhs_rw.Ammo < (rhs as ItemRangedWeapon).Ammo);
        return false;
      }

      // Top-level prescreen.  Resolves following priority issues in smoke testing
      // * food/ranged weapons/ammo
      // * melee weapons/armor
      // * trackers, lights, and entertainment
      int lhs_code = ItemRatingCode(lhs);
      int rhs_code = ItemRatingCode(rhs);
      if (lhs_code>rhs_code) return false;
      if (lhs_code<rhs_code) return true;

      // if food is interesting, it will dominate non-food
      if (rhs is ItemFood) return !(lhs is ItemFood);
      else if (lhs is ItemFood) return false;

      // ranged weapons
      if (rhs is ItemRangedWeapon) return !(lhs is ItemRangedWeapon);
      else if (lhs is ItemRangedWeapon) return false;

      if (rhs is ItemAmmo) return !(lhs is ItemAmmo);
      else if (lhs is ItemAmmo) return false;

      if (rhs is ItemMeleeWeapon rhs_melee)
        {
        int rating = m_Actor.MeleeWeaponAttack(rhs_melee.Model).Rating;
        if (rating <= m_Actor.UnarmedMeleeAttack().Rating) return false;
        int? best_rating = m_Actor.GetBestMeleeWeaponRating();    // rely on OrderableAI doing the right thing
        if (null == best_rating) return true;
        if (best_rating.Value < rating) return true;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
        if (2<=melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return m_Actor.MeleeWeaponAttack(worst.Model).Rating < rating;
        }
        if (lhs is ItemMeleeWeapon lhs_melee) return m_Actor.MeleeWeaponAttack(lhs_melee.Model).Rating < rating;
        return false;
        }
      else if (lhs is ItemMeleeWeapon lhs_melee) {
        int rating = m_Actor.MeleeWeaponAttack(lhs_melee.Model).Rating;
        if (rating <= m_Actor.UnarmedMeleeAttack().Rating) return true;
        int? best_rating = m_Actor.GetBestMeleeWeaponRating();    // rely on OrderableAI doing the right thing
        if (null == best_rating) return false;
        if (best_rating.Value < rating) return false;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
        if (2<=melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return m_Actor.MeleeWeaponAttack(worst.Model).Rating >= rating;
        }
        return true;
      }

      {
      if (rhs is ItemBodyArmor rhs_armor)
        {
        if (lhs is ItemBodyArmor lhs_armor) return lhs_armor.Rating < rhs_armor.Rating;
        return false;
        }
      else if (lhs is ItemBodyArmor) return false;
      }

      if (rhs is ItemGrenade) return !(lhs is ItemGrenade);
      else if (lhs is ItemGrenade) return false;

      // light and entertainment have been revised to possibly higher priority (context-sensitive)
      // traps and barricade material are guaranteed insurance policy status
      // medicine historically was, but that's an AI flaw

      // note that ItemTrap and ItemBarricadeMaterial have maximum rating code 1

      // XXX note that sleep and stamina have special uses for sufficiently good AI
      bool lhs_low_priority = (lhs is ItemLight) || (lhs is ItemTrap) || (lhs is ItemMedicine) || (lhs is ItemEntertainment) || (lhs is ItemBarricadeMaterial);
      bool rhs_low_priority = (rhs is ItemLight) || (rhs is ItemTrap) || (rhs is ItemMedicine) || (rhs is ItemEntertainment) || (rhs is ItemBarricadeMaterial);
      if (rhs_low_priority) return !lhs_low_priority;
      else if (lhs_low_priority) return false;

      var ok_trackers = new Zaimoni.Data.Stack<GameItems.IDs>(stackalloc GameItems.IDs[2]);
      if (m_Actor.NeedActiveCellPhone) ok_trackers.push(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.push(GameItems.IDs.TRACKER_POLICE_RADIO);

      if (rhs is ItemTracker)
        {
        if (!(lhs is ItemTracker)) return false;
        if (ok_trackers.Contains(lhs.Model.ID)) return false;
        return ok_trackers.Contains(rhs.Model.ID);
        }
      else if (lhs is ItemTracker) return false;

      return false;
    }

    protected T GetWorst<T>(IEnumerable<T> src) where T:Item
    {
      if (!src?.Any() ?? true) return null;
      T worst = null;
      foreach(T test in src) {
        if (null == worst) worst = test;
        else if (RHSMoreInteresting(test,worst)) worst = test;
      }
      return worst;
    }

    public ActorAction BehaviorMakeRoomFor(Item it, Point? position=null, bool use_ok=true)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!m_Actor.Inventory.IsFull) throw new InvalidOperationException("already have room for "+it.ToString());
      if (m_Actor.CanGet(it)) throw new InvalidOperationException("already could get "+it.ToString());
      if (ItemIsUseless(it)) throw new InvalidOperationException(it.ToString()+" is useless and need not have room made for it");
      // also should require IsInterestingItem(it), but that's infinite recursion for reasonable use cases
#endif
      Inventory inv = m_Actor.Inventory;
      { // drop useless item doesn't always happen in a timely fashion
      var useless = inv.GetFirst<Item>(obj => ItemIsUseless(obj));
      if (null != useless) return _BehaviorDropOrExchange(useless, it, position, use_ok);
      }

      // not-best body armor can be dropped
      if (2<=m_Actor.CountQuantityOf<ItemBodyArmor>()) {
        var armor = m_Actor.GetWorstBodyArmor();
        if (null != armor) return _BehaviorDropOrExchange(armor,it,position, use_ok);
      }

      { // not-best melee weapon can be dropped
        List<ItemMeleeWeapon> melee = inv.GetItemsByType<ItemMeleeWeapon>();
        if (null != melee) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          if (2<=melee.Count) return _BehaviorDropOrExchange(weapon, it, position);
          if (it is ItemMeleeWeapon new_melee && m_Actor.MeleeWeaponAttack(weapon.Model).Rating < m_Actor.MeleeWeaponAttack(new_melee.Model).Rating) return _BehaviorDropOrExchange(weapon, it, position, use_ok);
        }
      }
      {
      if (it is ItemRangedWeapon rw) {
        var rws = inv.GetFirst<ItemRangedWeapon>(obj => 0==obj.Ammo && obj.AmmoType==rw.AmmoType);
        if (null != rws) return _BehaviorDropOrExchange(rws, it, position);
        rws = inv.GetFirst<ItemRangedWeapon>(obj => 0==obj.Ammo && null==inv.GetCompatibleAmmoItem(obj));
        if (null != rws) return _BehaviorDropOrExchange(rws, it, position, use_ok);
      }
      }

      // another behavior is responsible for pre-emptively eating perishable food
      // canned food is normally eaten at the last minute
      if (use_ok) {
      {
      if (GameItems.IDs.FOOD_CANNED_FOOD == it.Model.ID && inv.GetBestDestackable(it) is ItemFood food) {
        // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
        int need = m_Actor.MaxFood - m_Actor.FoodPoints;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4 <= need && m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
      }
      }
      { // it should be ok to devour stimulants in a glut
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID && inv.GetBestDestackable(it) is ItemMedicine stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = m_Actor.ScaleMedicineEffect(stim.SleepBoost);
        if (num4 <= need && m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
      }
      }
      } // if (use_ok)

      int it_rating = ItemRatingCode_no_recursion(it);
      if (1==it_rating && it is ItemMeleeWeapon) return null;   // break action loop here
      bool is_SAN_restore_item = (it is ItemEntertainment || (it is ItemMedicine med && 0 < med.SanityCure));
      if (1<it_rating) {
        // generally, find a less-critical item to drop
        // this is expected to correctly handle the food glut case (item rating 1)
        bool rating_kludge = false;
        // entertainment is problematic.  Its rating-2 (want) is still immediate-use (i.e. it acts like 3 (need))
        if (2 == it_rating) {
          if (is_SAN_restore_item) rating_kludge = true;
        }

        if (rating_kludge) ++it_rating;

        int i = 0;
        while(++i < it_rating) {
          Item worst = GetWorst(m_Actor.Inventory.Items.Where(obj => ItemRatingCode_no_recursion(obj) == i && !TradeVeto(obj,it) && !InventoryTradeVeto(it,obj)));
          if (null == worst) continue;
          return _BehaviorDropOrExchange(worst, it, position, use_ok);
        }
        if (rating_kludge) --it_rating;
      }

      if (it is ItemAmmo am) {
        ItemRangedWeapon rw = m_Actor.Inventory.GetCompatibleRangedWeapon(am);
        if (null != rw && rw.Ammo < rw.Model.MaxAmmo) {
          // we really do need to reload this.
          if (null!=position && use_ok) { // only do this when right on top of the inventory containing the ammo
            if (inv.GetBestDestackable(am) is ItemAmmo already) return new ActionUseItem(m_Actor, already);
          }

          // 1) we would re-pickup food.
          // 2) it would not be a big deal if something less important than ammo were not re-picked up.
          Item drop = inv.GetFirst<ItemFood>();
          if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
          if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.MEDIKIT);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.BANDAGE);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
          if (null == drop) drop = inv.GetFirst<ItemGrenade>();
          if (null == drop) drop = inv.GetFirst<ItemAmmo>();
          if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
          if (null != drop) {
            if (drop.Model.IsStackable) drop = m_Actor.Inventory.GetBestDestackable(drop);    // should be non-null
            List<ActorAction> recover = new List<ActorAction>(3);
            if (null != position) {
              ActorAction tmp = _PrefilterDrop(drop, use_ok);
              if (null != tmp) return tmp;

              // 3a) drop target without triggering the no-pickup schema
              recover.Add(ActionTradeWith.Cast(position.Value, m_Actor, drop, it));
            } else {
              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionDropItem(m_Actor,drop));
              // 3b) pick up ammo
              recover.Add(new ActionTake(m_Actor,it.Model.ID));
            }
            // 3c) use ammo just picked up : arguably ActionUseItem; use ActionUse(Actor actor, Gameplay.GameItems.IDs it)
            recover.Add(new ActionUse(m_Actor, it.Model.ID));
            return new ActionChain(m_Actor,recover);
          }
        }
      }

      {
      int needHP = m_Actor.MaxHPs- m_Actor.HitPoints;
      if (0 < needHP) {
        if (   (GameItems.MEDIKIT == it.Model && needHP >= m_Actor.ScaleMedicineEffect(GameItems.MEDIKIT.Healing))
            || (GameItems.BANDAGE == it.Model && needHP >= m_Actor.ScaleMedicineEffect(GameItems.BANDAGE.Healing)))
          { // same idea as reloading, only hp instead of ammo
          Item drop = inv.GetFirst<ItemFood>();
          if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
          if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
          if (null == drop) drop = inv.GetFirst<ItemGrenade>();
          if (null == drop) drop = inv.GetFirst<ItemAmmo>();
          if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
          if (null != drop) {
            if (drop.Model.IsStackable) drop = m_Actor.Inventory.GetBestDestackable(drop);    // should be non-null
            List<ActorAction> recover = new List<ActorAction>(3);
            if (null != position) {
              ActorAction tmp = _PrefilterDrop(drop, use_ok);
              if (null != tmp) return tmp;

              // 3a) drop target without triggering the no-pickup schema
              recover.Add(ActionTradeWith.Cast(position.Value, m_Actor, drop, it));
            } else {
              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionDropItem(m_Actor,drop));
              // 3b) pick up ammo
              recover.Add(new ActionTake(m_Actor,it.Model.ID));
            }
            // 3c) use ammo just picked up : arguably ActionUseItem; use ActionUse(Actor actor, Gameplay.GameItems.IDs it)
            recover.Add(new ActionUse(m_Actor, it.Model.ID));
            return new ActionChain(m_Actor,recover);
          }
          }
      }
      }

      {
      if (it is ItemFood) {
          Item drop = inv.GetFirst<ItemFood>();
          if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
          if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
          if (null == drop) drop = inv.GetFirst<ItemSprayScent>();
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
          if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
          if (m_Actor.IsHungry) {
            if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
            if (null == drop) drop = inv.GetFirst<ItemGrenade>();
            if (null == drop) drop = inv.GetFirst<ItemAmmo>();
            if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
          }
          if (null != drop) {
            if (drop.Model.IsStackable) drop = m_Actor.Inventory.GetBestDestackable(drop);    // should be non-null
            if (null != position) return _BehaviorDropOrExchange(drop,it,position.Value);
            List<ActorAction> recover = new List<ActorAction>(2);
            // 3a) drop target without triggering the no-pickup schema
            recover.Add(new ActionDropItem(m_Actor,drop));
            // 3b) pick up food
            recover.Add(new ActionTake(m_Actor,it.Model.ID));
            return new ActionChain(m_Actor,recover);
          }
      }
      }

      // priority classes of incoming items are:
      // food
      // ranged weapon
      // ammo for a ranged weapon in inventory
      // melee weapon
      // body armor
      // grenades (soldiers and civilians, screened at the interesting item check)
      // light, traps, barricading, medical/entertainment, stench killer (civilians, screened at the interesting item check)
      // trackers (mainly because AI can't use properly), but cell phones are trackers

      // dropping body armor to get a better one should be ok
      if (it is ItemBodyArmor old_armor) {
        var armor = m_Actor.GetBestBodyArmor();
        if (null != armor && armor.Rating < old_armor.Rating) return _BehaviorDropOrExchange(armor, it, position, use_ok);
      }

// does not work: infinite recursion
#if PROTOTYPE
      if (use_ok && !(it is ItemTrap)) {  // traps: try to use them explicitly
        var use_trap = new Gameplay.AI.Goals.SetTrap(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor);
        if (use_trap.UrgentAction(out var ret) && null!=ret) {
          Objectives.Insert(0, use_trap);
          return ret;
        }
      }
#endif

      // medicine glut ... drop it
      foreach(GameItems.IDs x in GameItems.medicine) {
        if (it.Model.ID == x) continue;
        ItemModel model = Models.Items[(int)x];
        if (2>m_Actor.Count(model)) continue;
        Item tmp = m_Actor.Inventory.GetBestDestackable(model);
        if (null != tmp) return _BehaviorDropOrExchange(tmp, it, position, use_ok);
      }

      // trackers (mainly because AI can't use properly), but cell phones are trackers
      // XXX this is triggering a coverage failure; we need to be more sophisticated about trackers
      List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
      if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);
      if (it is ItemTracker) {
        if (!ok_trackers.Contains(it.Model.ID)) return null;   // tracker normally not worth clearing a slot for
      }
      // ditch an unwanted tracker if possible
      ItemTracker tmpTracker = inv.GetFirstMatching<ItemTracker>(it2 => !ok_trackers.Contains(it2.Model.ID));
      if (null != tmpTracker) return _BehaviorDropOrExchange(tmpTracker, it, position, use_ok);

      // these lose to everything other than trackers.  Note that we should drop a light to get a more charged light -- if we're right on top of it.
      if (it is ItemSprayScent) return null;
      ItemSprayScent tmpSprayScent = inv.GetFirstMatching<ItemSprayScent>();
      if (null != tmpSprayScent) return _BehaviorDropOrExchange(tmpSprayScent, it, position, use_ok);

      if (it is ItemBarricadeMaterial) return null;
      ItemBarricadeMaterial tmpBarricade = inv.GetFirstMatching<ItemBarricadeMaterial>();
      if (null != tmpBarricade) return _BehaviorDropOrExchange(tmpBarricade, it, position, use_ok);

      if (it is ItemEntertainment) return null;
      ItemEntertainment tmpEntertainment = inv.GetFirstMatching<ItemEntertainment>();
      if (null != tmpEntertainment) return _BehaviorDropOrExchange(tmpEntertainment, it, position, use_ok);

      if (it is ItemTrap) return null;
      ItemTrap tmpTrap = inv.GetFirstMatching<ItemTrap>();
      if (null != tmpTrap) return _BehaviorDropOrExchange(tmpTrap, it, position, use_ok);

      if (it is ItemLight) {
        if (1 >= it_rating) return null;
        Item worst = GetWorst(m_Actor.Inventory.Items.Where(obj => 1 >= ItemRatingCode(obj)));
        if (null == worst) return null;
        return _BehaviorDropOrExchange(worst, it, position, use_ok);
      }

      if (it is ItemMedicine) return null;

      // ditch unimportant items
      ItemMedicine tmpMedicine = inv.GetFirstMatching<ItemMedicine>();
      if (null != tmpMedicine) return _BehaviorDropOrExchange(tmpMedicine, it, position, use_ok);

      // least charged flashlight goes
      List<ItemLight> lights = inv.GetItemsByType<ItemLight>();
      if (null != lights && 2<=lights.Count) {
        int min_batteries = lights.Select(obj => obj.Batteries).Min();
        ItemLight discard = lights.Find(obj => obj.Batteries==min_batteries);
        return BehaviorDropItem(discard);
      }

      // uninteresting ammo
      {
      ItemAmmo tmpAmmo;
      if (it is ItemRangedWeapon rw && rw.Ammo<rw.Model.MaxAmmo) {
        tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => null == m_Actor.Inventory.GetCompatibleRangedWeapon(ammo) && ammo.AmmoType!=rw.AmmoType);  // not quite the full check here.  Problematic if no ranged weapons at all.
        if (null != tmpAmmo) return _BehaviorDropOrExchange(tmpAmmo, it, position, use_ok);
        if (use_ok) {
          tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => null == m_Actor.Inventory.GetCompatibleRangedWeapon(ammo) && ammo.AmmoType==rw.AmmoType);
          if (null != tmpAmmo) {
            Item drop = inv.GetFirst<ItemFood>();
            if (null == drop) drop = inv.GetFirst<ItemEntertainment>();
            if (null == drop) drop = inv.GetFirst<ItemBarricadeMaterial>();
            if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SAN);
            if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_ANTIVIRAL);
            if (null == drop) drop = inv.GetFirstByModel(GameItems.MEDIKIT);
            if (null == drop) drop = inv.GetFirstByModel(GameItems.BANDAGE);
            if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_STA);
            if (null == drop) drop = inv.GetFirstByModel(GameItems.PILLS_SLP);
            if (null == drop) drop = inv.GetFirst<ItemGrenade>();
            if (null == drop) drop = inv.GetFirst<ItemAmmo>(ammo => ammo.AmmoType != rw.AmmoType);
            if (null == drop) drop = inv.GetFirst<Item>(obj => !(obj is ItemRangedWeapon) && !(obj is ItemAmmo));
            if (null != drop) {
              if (drop.Model.IsStackable) drop = m_Actor.Inventory.GetBestDestackable(drop);    // should be non-null
              if (null != position) return _BehaviorDropOrExchange(drop,it,position.Value);
              List<ActorAction> recover = new List<ActorAction>(2);
              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionDropItem(m_Actor,drop));
              // 3b) pick up food
              recover.Add(new ActionTake(m_Actor,it.Model.ID));
              return new ActionChain(m_Actor,recover);
            }
          }
        }
      } else {
        tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => null == m_Actor.Inventory.GetCompatibleRangedWeapon(ammo));  // not quite the full check here.  Problematic if no ranged weapons at all.
//      tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => !IsInterestingItem(ammo));  // full check, triggers infinite recursion
        if (null != tmpAmmo) return _BehaviorDropOrExchange(tmpAmmo, it, position, use_ok);
      }
      }

      // ranged weapon with zero ammo is ok to drop for something other than its own ammo
      ItemRangedWeapon tmpRw2 = inv.GetFirstMatching<ItemRangedWeapon>(rw => 0 >= rw.Ammo);
      if (null != tmpRw2) {
         bool reloadable = (it is ItemAmmo ? (it as ItemAmmo).AmmoType==tmpRw2.AmmoType : false);
         if (!reloadable) return _BehaviorDropOrExchange(tmpRw2, it, position, use_ok);
      }

      // if we have 2 clips of an ammo type, trading one for a melee weapon or food is ok
      if (it is ItemMeleeWeapon || it is ItemFood) {
        foreach(GameItems.IDs x in GameItems.ammo) {
          ItemModel model = Models.Items[(int)x];
          if (2<=m_Actor.Count(model)) {
            ItemAmmo ammo = inv.GetBestDestackable(model) as ItemAmmo;
            return _BehaviorDropOrExchange(ammo, it, position, use_ok);
          }
        }
        // if we have two clips of any type, trading the smaller one for a melee weapon or food is ok
        ItemAmmo test = null;
        foreach(GameItems.IDs x in GameItems.ammo) {
          if (inv.GetBestDestackable(Models.Items[(int)x]) is ItemAmmo ammo) {
             if (null == test || test.Quantity>ammo.Quantity) test = ammo;
          }
        }
        if (null != test) return _BehaviorDropOrExchange(test, it, position, use_ok);
      }

      // if inventory is full and the problem is ammo at this point, ignore if we already have a full clip
      if (it is ItemAmmo && 1<=m_Actor.Count(it.Model)) return null;
      if (it is ItemAmmo && AmmoAtLimit) return null;

      // if inventory is full and the problem is ranged weapon at this point, ignore if we already have one
      if (it is ItemRangedWeapon && 1<= inv.CountType<ItemRangedWeapon>()) return null;

      // grenades next
      if (it is ItemGrenade) return null;
      ItemGrenade tmpGrenade = inv.GetFirstMatching<ItemGrenade>();
      if (null != tmpGrenade) return _BehaviorDropOrExchange(tmpGrenade, it, position, use_ok);

      // important trackers go for ammo or food
      if (it is ItemAmmo || it is ItemFood) {
        ItemTracker discardTracker = inv.GetFirstMatching<ItemTracker>();
        if (null != discardTracker) return _BehaviorDropOrExchange(discardTracker, it, position, use_ok);
      }

      // ok to drop second food item for ammo
      if (it is ItemAmmo) {
        if (2<=m_Actor.Inventory.CountType<ItemFood>()) {
          var discard = inv.GetFirstMatching<ItemFood>();
          if (null != discard) return _BehaviorDropOrExchange(discard, it, position, use_ok);
        }
      }

#if DEBUG
      // do not pick up trackers if it means dropping body armor or higher priority
      if (it is ItemTracker) return null;

      // body armor
      if (it is ItemBodyArmor) return null;

      if (it is ItemMeleeWeapon) return null;

      throw new InvalidOperationException("coverage hole of types in BehaviorMakeRoomFor");
#else
      // give up
      return null;
#endif
    }

#nullable enable
    private bool _InterestingItemPostprocess(Item it)
    {
      if (!m_Actor.CanGet(it)) {
        if (m_Actor.Inventory.IsFull) return null != BehaviorMakeRoomFor(it);
        return false;
      }
      return true;
    }

    public bool IsInterestingItem(ItemRangedWeapon rw)
    {
      Inventory inv = m_Actor.Inventory;
      if (inv.Contains(rw)) {
        if (0 < rw.Ammo) return true;
        // should not have ammo in inventory at this point
      } else {
        if (0< inv.CountType<ItemRangedWeapon>(it => 0 < it.Ammo)) {
          if (null != inv.GetFirstByModel<ItemRangedWeapon>(rw.Model, it => 0 < it.Ammo)) return false;    // XXX
          if (null != inv.GetFirst<ItemRangedWeapon>(it => it.AmmoType==rw.AmmoType && 0 < it.Ammo)) return false; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
        } else {
          if (null != inv.GetCompatibleAmmoItem(rw)) return true;
        }
        if (0 < rw.Ammo && null != inv.GetFirstByModel<ItemRangedWeapon>(rw.Model, it => 0 == it.Ammo)) return true;  // this replacement is ok; implies not having ammo
      }
      // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
      // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
      if (null == inv.GetCompatibleAmmoItem(rw)) {
        if (AmmoAtLimit) return false;
        if (0 >= rw.Ammo) return false;
      }
      return _InterestingItemPostprocess(rw);
    }

    private bool AmmoAtLimit {
      get {
        // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
        // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
        Inventory inv = m_Actor.Inventory;
        int limit = inv.MaxCapacity;
        if (0< inv.CountType<ItemBodyArmor>()) limit--;
        if (0< inv.CountType<ItemLight>()) limit--;
        if (0< inv.CountType<ItemFood>()) limit--;
        if (0< inv.CountType<ItemExplosive>()) limit--;
        if (0< inv.CountType<ItemMeleeWeapon>()) limit--;

        if (limit <= inv.CountType<ItemAmmo>()) return true;
        if (limit <= inv.CountType<ItemRangedWeapon>(it => 0 < it.Ammo)+ inv.CountType<ItemAmmo>()) return true;
        return false;
      }
    }

    public bool IsInterestingItem(ItemAmmo am)
    {
      Inventory inv = m_Actor.Inventory;
      ItemRangedWeapon rw = inv.GetCompatibleRangedWeapon(am);
      if (null == rw) {
        if (0 < inv.Count(am.Model)) return false;    // only need one clip to prime AI to look for empty ranged weapons
        if (KnowRelevantInventory(am) && !AmmoAtLimit) return true;
        if (0 < inv.CountType<ItemRangedWeapon>()) return false;  // XXX
      } else {
        if (rw.Model.MaxAmmo>rw.Ammo) return true;
        if (inv.HasAtLeastFullStackOf(am, 2)) return false;
        if (null != inv.GetFirstByModel<ItemAmmo>(am.Model, it => it.Quantity < it.Model.MaxQuantity)) return true;   // topping off clip is ok
      }
      return _InterestingItemPostprocess(am);
    }
#nullable restore

    // so we can do post-condition testing cleanly
    private bool _IsInterestingItem(Item it)
    {
      // note that CHAR guards and soldiers don't need to eat like civilians, so they would not be interested in food
      if (it is ItemFood food) {
//      if (!m_Actor.Model.Abilities.HasToEat) return false;    // redundant; for documentation
        if (m_Actor.IsHungry) return true;
        if (food.IsSpoiledAt(m_Actor.Location.Map.LocalTime.TurnCounter)) return false;
        if (!m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food)) return true;
        // only interesting if pre-emptive eating would kick in
        return food.IsPerishable && m_Actor.CurrentNutritionOf(food)<= m_Actor.MaxFood - m_Actor.FoodPoints;
      }

      if (it is ItemRangedWeapon rw) {
//      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;    // redundant; for documentation
        return IsInterestingItem(rw);
      }
      if (it is ItemAmmo am) {
//      if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons) return false;    // redundant; for documentation
        bool ret = IsInterestingItem(am);
        if (ret) return true;
        // this post-condition is difficult to fully get, above
        int item_rating = ItemRatingCode(it);   // may need to be no-recursion form here?
        if (1 >= item_rating) return false;
        // check inventory for less-interesting item.  Force high visibility in debugger.
        foreach(Item obj in m_Actor.Inventory.Items) {
          int test_rating = ItemRatingCode(obj);   // may need to be no-recursion form here?
          if (test_rating < item_rating) return true;
        }
        return false;
      }
      if (it is ItemMeleeWeapon melee) {
        int rating = m_Actor.MeleeWeaponAttack(melee.Model).Rating;
        if (rating <= m_Actor.UnarmedMeleeAttack().Rating) return false;
        int? best_rating = m_Actor.GetBestMeleeWeaponRating();    // rely on OrderableAI doing the right thing
        if (null == best_rating) return true;
        if (best_rating.Value < rating) return true;
        int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
#if DEBUG
        if (0 >= melee_count) throw new InvalidOperationException("inconstent return values");
#endif
        if (2<= melee_count) {
          ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
          return m_Actor.MeleeWeaponAttack(worst.Model).Rating < rating;
        }
        return true;
      }
      if (it is ItemBodyArmor new_armor) {
        var armor = m_Actor.GetBestBodyArmor();
        return null == armor || armor.Rating < new_armor.Rating; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
      }

      // No specific heuristic.
      if (it is ItemTracker) {
        if (1<=m_Actor.Inventory.Count(it.Model)) return false;
      } else if (it is ItemLight) {
        if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1)) return false;
      } else if (it is ItemMedicine med) {
        // XXX easy to action-loop if inventory full
        if (m_Actor.HasAtLeastFullStackOf(it, m_Actor.Inventory.IsFull ? 1 : 2)) return false;
        if (0<med.SanityCure && 2<=WantRestoreSAN) return true;
      } else if (it is ItemTrap trap) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
        if (trap.IsActivated) return false;
#if DEBUG
      } else if (it is ItemEntertainment) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
        if (2<=WantRestoreSAN) return true;
      } else if (it is ItemBarricadeMaterial) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
      } else if (it is ItemSprayScent) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
      } else if (it is ItemGrenade) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
      } else {
        throw new InvalidOperationException("coverage hole");
#else
      } else {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
#endif
      }

      return _InterestingItemPostprocess(it);
    }

    public bool IsInterestingItem(Item it)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!m_Actor.Model.Abilities.HasInventory) throw new InvalidOperationException("inventory required");   // CHAR guards: wander action can get item from containers
      if (!m_Actor.Model.Abilities.CanUseMapObjects) throw new InvalidOperationException("using map objects required");
#endif
      if (ItemIsUseless(it)) return false;

#if DEBUG
#if INTEGRITY_CHECK_ITEM_RETURN_CODE
      bool ret = _IsInterestingItem(it);
      int item_rating = ItemRatingCode(it);
      if (ret && 1>item_rating) throw new InvalidOperationException("interesting item thought to have no use");
      if (!ret && 1<item_rating && !m_Actor.Inventory.Has(it.Model.ID)) {
        // check inventory for less-interesting item.  Force high visibility in debugger.
        foreach(Item obj in m_Actor.Inventory.Items) {
          if (it.Model == obj.Model) continue;
          int test_rating = ItemRatingCode(obj);
          if (test_rating < item_rating) throw new InvalidOperationException("uninteresting item thought to have a clear use");
        }
      }
      return ret;
#else
      return _IsInterestingItem(it);
#endif
#else
      return _IsInterestingItem(it);
#endif
    }

#nullable enable
    public virtual bool IsInterestingTradeItem(Actor speaker, Item offeredItem) // Cf. OrderableAI::IsRationalTradeItem
    {
#if DEBUG
      if (!speaker.Model.Abilities.CanTrade) throw new InvalidOperationException(nameof(speaker)+" must be able to trade");
      if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException(nameof(m_Actor)+" must be able to trade");
#endif
      if (Rules.Get.RollChance(speaker.CharismaticTradeChance)) return true;
      return IsInterestingItem(offeredItem);
    }
#nullable restore

    private static void _InterpretRangedWeapons(IEnumerable<ItemRangedWeapon>? rws, in Point pt, Dictionary<Point, ItemRangedWeapon[]> best_rw, Dictionary<Point, ItemRangedWeapon[]> reload_empty_rw, Dictionary<Point, ItemRangedWeapon[]> discard_empty_rw, Dictionary<Point, ItemRangedWeapon[]> reload_rw)
    {
        if (null == rws || !rws.Any()) return;

        best_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        reload_empty_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        discard_empty_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        reload_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        bool keep_empty = false;
        bool keep_reload = false;

        foreach(var rw in rws) {
          // note that "better" ranged weapons taking the same ammo have larger clips
          int am_type = (int)rw.AmmoType;
          ItemRangedWeapon rw_am_type;
          if (0==rw.Ammo) {
            if (   null == (rw_am_type = reload_empty_rw[pt][am_type])
                || rw_am_type.Model.MaxAmmo < rw.Model.MaxAmmo)
              reload_empty_rw[pt][am_type] = rw;
            if (   null == (rw_am_type = discard_empty_rw[pt][am_type])
                || rw_am_type.Model.MaxAmmo > rw.Model.MaxAmmo)
              discard_empty_rw[pt][am_type] = rw;
            keep_empty = true;
          }
          if (rw.Model.MaxAmmo > rw.Ammo) {
            int m_maxammo;
            if (    null == (rw_am_type = reload_rw[pt][am_type])
                || (m_maxammo = rw_am_type.Model.MaxAmmo) < rw.Model.MaxAmmo
                || (m_maxammo - rw_am_type.Ammo) < (rw.Model.MaxAmmo - rw.Ammo))
              reload_rw[pt][am_type] = rw;
            keep_reload = true;
          }
          if (   null == (rw_am_type = best_rw[pt][am_type])
              || rw_am_type.Ammo < rw.Ammo
              || rw_am_type.Model.MaxAmmo < rw.Model.MaxAmmo) {
            best_rw[pt][am_type] = rw;
            continue;
          }
        }
        if (!keep_empty) {
          reload_empty_rw.Remove(pt);
          discard_empty_rw.Remove(pt);
        }
        if (!keep_reload) {
          reload_rw.Remove(pt);
        }
    }

    // we are having some problems with breaking an action loop that requires reloading a weapon to make ammo gettable, when already at ammo limit
    // the logic is there but it's not being reached.
    // issue w/recovery logic and ammo
    protected ActorAction? InventoryStackTactics(Location loc)
    {
      Inventory inv = m_Actor.Inventory;
      if (inv.IsEmpty) return null;

      // The index case.
      var rws = inv.GetItemsByType<ItemRangedWeapon>();
      if (null != rws) {
        foreach(var rw in rws) {
          if (rw.Ammo < rw.Model.MaxAmmo) {
            // usually want to reload this even if we had to drop ammo as a recovery option
            var want_ammo = (GameItems.IDs)((int)(rw.AmmoType) + (int)(GameItems.IDs.AMMO_LIGHT_PISTOL));
            int i = Objectives.Count;
            while(0<i) if (Objectives[--i] is Goal_DoNotPickup dnp && dnp.Avoid == want_ammo) Objectives.RemoveAt(i);
          }
        }
      }

      var ground_inv = loc.Map.GetAccessibleInventories(loc.Position);
      if (0 >= ground_inv.Count) return null;

      // set up pattern-matching for ranged weapons
      Point viewpoint_inventory = Point.MaxValue; // intentionally chosen to be impossible, as a flag
      var best_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var reload_empty_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var discard_empty_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var reload_rw = new Dictionary<Point, ItemRangedWeapon[]>();

      _InterpretRangedWeapons(rws, in viewpoint_inventory, best_rw, reload_empty_rw, discard_empty_rw, reload_rw);

      if (reload_rw.ContainsKey(viewpoint_inventory)) {
        ItemRangedWeapon? local_rw;
        { // historically, we preferred handling this reload-get combination elsewhere
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == (local_rw = reload_rw[viewpoint_inventory][i])) continue;
            var local_ammo = inv.GetCompatibleAmmoItem(local_rw);
            if (null == local_ammo) continue;
            foreach(var x in ground_inv) {
             var remote_ammo = x.Value.GetCompatibleAmmoItem(local_rw);
             if (null == remote_ammo) continue;
             Objectives.Insert(0, new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter + 1, m_Actor, new ActionTake(m_Actor, (GameItems.IDs)(i + (int)GameItems.IDs.AMMO_LIGHT_PISTOL))));
             local_rw.EquippedBy(m_Actor);  // \todo evaluate sinking this into the ammo use handler
             return new ActionUseItem(m_Actor, local_ammo);
            }
          }
        }

        // prepare to analyze ranged weapon swaps.
        foreach(var x in ground_inv) {
          var ground_rws = x.Value.GetItemsByType<ItemRangedWeapon>();
          _InterpretRangedWeapons(ground_rws, x.Key, best_rw, reload_empty_rw, discard_empty_rw, reload_rw);
        }

        ItemRangedWeapon? alt_rw;
        if (discard_empty_rw.ContainsKey(viewpoint_inventory)) {
          // we should not have been able to reload this i.e. no ammo.
          Point? dest = null;
          ItemRangedWeapon? test = null;
          ItemRangedWeapon? src = null;
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == (local_rw = discard_empty_rw[viewpoint_inventory][i])) continue;
            foreach(var where_inv in best_rw) {
              if (where_inv.Key == viewpoint_inventory) continue;
              if (null == (alt_rw = where_inv.Value[i])) continue;
              if (0 >= alt_rw.Ammo) continue;
              if (    null == test
                  || (test.Ammo < alt_rw.Ammo && test.Model.MaxAmmo <= alt_rw.Model.MaxAmmo)) {
                dest = where_inv.Key;
                src = local_rw;
                test = alt_rw;
                continue;
              }
            }
          }
          if (null != test) return ActionTradeWith.Cast(dest.Value, m_Actor, src, test);
        }

        // optimization: swap for most-loaded ranged weapon taking same ammo
        {
          Point? dest = null;
          ItemRangedWeapon? test = null;
          ItemRangedWeapon? src = null;
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == (local_rw = reload_rw[viewpoint_inventory][i])) continue;
            foreach(var where_inv in best_rw) {
              if (where_inv.Key == viewpoint_inventory) continue;
              if (null == (alt_rw = where_inv.Value[i])) continue;
              if (local_rw.Ammo >= alt_rw.Ammo) continue;
              if (local_rw.Model.MaxAmmo > alt_rw.Model.MaxAmmo) continue;
              if (    null == test
                  || (test.Ammo < alt_rw.Ammo && test.Model.MaxAmmo <= alt_rw.Model.MaxAmmo)) {
                dest = where_inv.Key;
                src = local_rw;
                test = alt_rw;
                continue;
              }
            }
          }
          if (null != test) return ActionTradeWith.Cast(dest.Value, m_Actor, src, test);
        }
      }

      return null;
    }

#nullable enable
    protected ActorAction? InventoryStackTactics() { return InventoryStackTactics(m_Actor.Location); }

    /// <remark>Intentionally asymmetric.  Call this twice to get proper coverage.
    /// Will ultimately end up in ObjectiveAI when AI state needed.</remark>
    static public bool TradeVeto(Item mine, Item theirs)
    {
      var rw_model = mine.Model as ItemRangedWeaponModel;
      // reject identity trades for now.  This will change once AI state is involved.
      if (mine.Model == theirs.Model) {
        // ranged weapons: require ours to have strictly less ammo
        if (null != rw_model) return (mine as ItemRangedWeapon).Ammo >= (theirs as ItemRangedWeapon).Ammo;
        // battery-powered items: require strictly less charge (police radios not included as they are low-grade generators)
        if (mine is BatteryPowered test && mine.Model.ID!=GameItems.IDs.TRACKER_POLICE_RADIO) return test.Batteries >= (theirs as BatteryPowered).Batteries;
        // generally, if stackable we want to trade away the smaller stack (intercepting partial take from ground inventory is a higher order test)
        if (1<mine.Model.StackingLimit) return mine.Quantity >= theirs.Quantity;
        // default is to reject.   Expected to change once AI state is involved
        return true;
      }

      // do not trade away weapon for own ammo
      if (null != rw_model && (GameItems.IDs)((int)rw_model.AmmoType+(int)GameItems.IDs.AMMO_LIGHT_PISTOL) == theirs.Model.ID) return true;

      switch(mine.Model.ID)
      {
      // flashlights.  larger radius and longer duration are independently better...do not trade if both are worse
      case GameItems.IDs.LIGHT_BIG_FLASHLIGHT:
        if (GameItems.IDs.LIGHT_FLASHLIGHT==theirs.Model.ID && (theirs as BatteryPowered).Batteries<(mine as BatteryPowered).Batteries) return true;
        if (GameItems.IDs.LIGHT_BIG_FLASHLIGHT==theirs.Model.ID && (theirs as BatteryPowered).Batteries<(mine as BatteryPowered).Batteries) return true;
        break;
      case GameItems.IDs.LIGHT_FLASHLIGHT:
        if (GameItems.IDs.LIGHT_FLASHLIGHT==theirs.Model.ID && (theirs as BatteryPowered).Batteries<(mine as BatteryPowered).Batteries) return true;
        break;
      }
      return false;
    }

    /// <remark>Intentionally asymmetric.  Ground inventories can't object.</remark>
    public bool InventoryTradeVeto(Item mine, Item theirs)
    {
      var rw_model = mine.Model as ItemRangedWeaponModel;

      // do not trade away weapon for own ammo
      if (null != rw_model && (GameItems.IDs)((int)rw_model.AmmoType+(int)GameItems.IDs.AMMO_LIGHT_PISTOL) == theirs.Model.ID) return true;

      // if we have 2 clips of an ammo type, trading one for a melee weapon or food is ok (don't reverse this)
      // InventoryTradeVeto: reject sole melee for 2nd ammo [has no other uses so easier to manipulate]
      if (mine is ItemAmmo am) {
        var already_have = m_Actor.Count(am.Model);
        if (1 <= already_have) {
          if (theirs is ItemMeleeWeapon melee && melee==m_Actor.GetBestMeleeWeapon()) return true;
          // could need to test for food as well, but no test case (yet)
        }
      }

      return false;
    }

    // cf ActorController::IsTradeableItem
    // this must prevent CivilianAI from
    // 1) bashing barricades, etc. for food when hungry
    // 2) trying to search for z at low ammo when there is ammo available
    public HashSet<GameItems.IDs> WhatDoINeedNow()
    {
      HashSet<GameItems.IDs> ret = new HashSet<GameItems.IDs>();
      GameItems.IDs i = GameItems.IDs._COUNT;
      while(0 < i--) {
        if (3==ItemRatingCode(i)) ret.Add(i);
      }
      return ret;
    }

    // If an item would be IsInterestingItem(it), its ID should be in this set if not handled by WhatDoINeedNow().
    // items flagged here should "be more interesting" than what we have
    public HashSet<GameItems.IDs> WhatDoIWantNow()
    {
      HashSet<GameItems.IDs> ret = new HashSet<GameItems.IDs>();

      GameItems.IDs i = GameItems.IDs._COUNT;
      while(0 < i--) {
        if (2==ItemRatingCode(i)) ret.Add(i);
      }
      return ret;
    }

    public KeyValuePair<List<GameItems.IDs>, List<GameItems.IDs>> NonCriticalInInventory()
    {
      var insurance = new List<GameItems.IDs>((int)GameItems.IDs._COUNT);   // bloated, but it'll garbage-collect shortly anyway and this would be expected to prevent in-build reallocations
      var want = new List<GameItems.IDs>((int)GameItems.IDs._COUNT);
      GameItems.IDs i = GameItems.IDs._COUNT;
      Inventory inv = m_Actor.Inventory;
      ItemModel model;
      while(0 < i--) {
        if (null == inv.GetBestDestackable((model = Models.Items[(int)i]))) continue;   // not really in inventory
        var code = ItemRatingCode(i);
        if (3 <= code) continue;
        // ranged weapons are problematic
        if (GameItems.ranged.Contains(i)) {
          if (1 >= inv.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo)) continue;    // really critical
          if (null != inv.GetCompatibleAmmoItem(model as ItemRangedWeaponModel)) continue;
        }
        if (2 == code) want.Add(i);
        else insurance.Add(i);
      }
      return new KeyValuePair<List<GameItems.IDs>, List<GameItems.IDs>>((0<insurance.Count ? insurance : null), (0 < want.Count ? want : null));
    }

    // arguable whether these twp should be public in Map
    static protected IEnumerable<Engine.MapObjects.PowerGenerator>? GeneratorsToTurnOn(Map m)
    {
      if (Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap == m) return null; // plot consequences until Prisoner That Should Not Be is dead, does not light level.
      var gens_off = m.PowerGenerators.Get.Where(obj => !obj.IsOn);
      return gens_off.Any() ? gens_off : null;
    }

    static protected IEnumerable<Engine.MapObjects.PowerGenerator>? Generators(Map m)
    {
      if (Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap == m) return null; // plot consequences until Prisoner That Should Not Be is dead, does not light level.
      var gens = m.PowerGenerators.Get;
      return (0 >= gens.Count) ? null : gens;
    }

    public bool CombatUnready()
    {
      if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(rw => null!=m_Actor.Inventory.GetCompatibleAmmoItem(rw))) return false;
      // further one-on-one evaluation requires either an actor model, or an actor, as target
      return true;
    }

    protected static int ScoreRangedWeapon(ItemRangedWeapon w)
    {
      Attack rw_attack = w.Model.Attack;
      return 1000 * rw_attack.Range + rw_attack.DamageValue;
    }

    // conceptual difference between "doctrine" and "behavior" is that doctrine doesn't have contextual validity checks
    // that is, a null action return is defined to mean the doctrine is invalid
    public ActorAction? DoctrineRecoverSTA(int targetSTA)
    {
       int tmp;
       if ((tmp = m_Actor.MaxSTA) < targetSTA) targetSTA = tmp;
       if ((tmp = m_Actor.StaminaPoints) >= targetSTA) return null;
       if (tmp < targetSTA - 4 && m_Actor.CanActNextTurn) {
         var stim = m_Actor?.Inventory.GetBestDestackable(GameItems.PILLS_STA);
         if (null != stim) return new ActionUseItem(m_Actor,stim);
       }
       return new ActionWait(m_Actor);
    }

    public ActorAction? DoctrineMedicateSLP()
    {
       if (!(m_Actor?.Inventory.GetBestDestackable(GameItems.PILLS_SLP) is ItemMedicine stim)) return null;
       if (m_Actor.SleepPoints > (m_Actor.MaxSleep - m_Actor.ScaleMedicineEffect(stim.SleepBoost))) return null;
       if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
       return new ActionUseItem(m_Actor,stim);
    }

    public ActorAction? DoctrineRechargeToFull(Item it)
    {
      BatteryPowered obj = it as BatteryPowered;
#if DEBUG
      if (null == obj) throw new ArgumentNullException(nameof(obj));
#endif
      if (obj.MaxBatteries-1 <= obj.Batteries) return null;
      var generators = m_Actor.Location.Map.PowerGenerators.Get.Where(power => Rules.IsAdjacent(m_Actor.Location,power.Location)).ToList();
      if (0 >= generators.Count) return null;
      var generators_on = generators.FindAll(power => power.IsOn);
      if (0 >= generators_on.Count) return new ActionSwitchPowerGenerator(m_Actor,generators[0]);
      if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
      if (!it.IsEquipped) it.EquippedBy(m_Actor);
      return new ActionRechargeItemBattery(m_Actor,it);
    }

    public ActorAction? DoctrineButcher(Corpse c)
    {
      if (!m_Actor.CanButcher(c)) return null;
      if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);

      m_Actor.GetBestMeleeWeapon()?.EquippedBy(m_Actor);
      return new ActionButcher(m_Actor,c);
    }

    // XXX should also have concept of hoardable item (suitable for transporting to a safehouse)
    public ItemRangedWeapon? GetBestRangedWeaponWithAmmo()
    {
      var inv = m_Actor?.Inventory;  // PC zombies won't have inventory
      if (inv?.IsEmpty ?? true) return null;
      var rws = inv.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo || null != m_Actor.Inventory.GetItemsByType<ItemAmmo>(am => am.AmmoType == rw.AmmoType));
      return rws?.Maximize(w => ScoreRangedWeapon(w));
    }

    public KeyValuePair<Actor,ItemRangedWeapon>? GetNearestTargetFor()
    {
      var rw = GetBestRangedWeaponWithAmmo();
      if (null == rw) return null;
      var range = rw.Model.Attack.Range;
      short r = 0;
      while (++r <= range) {
        foreach (Point pt in Enumerable.Range(0, 8 * r).Select(i => m_Actor.Location.Position.RadarSweep(r, i))) {
            Location loc = new Location(m_Actor.Location.Map, pt);
            if (!Map.Canonical(ref loc)) continue;
            var actor2 = loc.Actor;
            if (null != actor2 && !actor2.IsDead && m_Actor != actor2 && m_Actor.IsEnemyOf(actor2)) {
              if (LOS.CanTraceViewLine(m_Actor.Location, actor2.Location)) return new KeyValuePair<Actor, ItemRangedWeapon>(actor2,rw);
            }
        }
      }
      return null;
    }
#nullable restore

    public Dictionary<int,Attack>? GetBestRangedAttacks(Actor target)
    {
      if (m_Actor?.Inventory.IsEmpty ?? true) return null;  // PC zombies won't have inventory
      var rws = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>(rw => 0 < rw.Ammo|| null != m_Actor.Inventory.GetItemsByType<ItemAmmo>(am => am.AmmoType == rw.AmmoType));
      if (null == rws) return null;
      var ret = new Dictionary<int, Attack>();
      foreach(var w in rws) {
        int r = w.Model.Attack.Range;
        do {
           var r_attack = m_Actor.HypotheticalRangedAttack(w.Model.Attack, r, target);
           // \todo proper comparison of attacks
           if (!ret.TryGetValue(r,out var att) || att.DamageValue<r_attack.DamageValue) ret[r] = r_attack;
           }
        while(0 < --r);
      }
      return ret;
    }

#nullable enable
    public void DeBarricade(Engine.MapObjects.DoorWindow doorWindow)
    {
      void install_break(ObjectiveAI ai) {
        // XXX \todo message this so it's clear what's going on
        if (null == ai.Goal<Goal_BreakBarricade>(o => o.Target == doorWindow)) {
          ai.Objectives.Insert(0, new Goal_BreakBarricade(ai.m_Actor, doorWindow));
        }
      }

      install_break(this);
      m_Actor.DoForAllFollowers(fo => {
          if (InCommunicationWith(fo) && fo.Controller is OrderableAI ai) install_break(ai);   // excludes players
      });
    }
#nullable restore
  }
}
