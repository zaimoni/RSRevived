#define INTEGRITY_CHECK_ITEM_RETURN_CODE

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using ActionButcher = djack.RogueSurvivor.Engine.Actions.ActionButcher;
using ActionChain = djack.RogueSurvivor.Engine.Actions.ActionChain;
using ActionDropItem = djack.RogueSurvivor.Engine.Actions.ActionDropItem;
using ActionMoveStep = djack.RogueSurvivor.Engine.Actions.ActionMoveStep;
using ActionPutInContainer = djack.RogueSurvivor.Engine.Actions.ActionPutInContainer;
using ActionRechargeItemBattery = djack.RogueSurvivor.Engine.Actions.ActionRechargeItemBattery;
using ActionShove = djack.RogueSurvivor.Engine.Actions.ActionShove;
using ActionSwitchPowerGenerator = djack.RogueSurvivor.Engine.Actions.ActionSwitchPowerGenerator;
using ActionTake = djack.RogueSurvivor.Engine.Actions.ActionTake;
using ActionUseExit = djack.RogueSurvivor.Engine.Actions.ActionUseExit;
using ActionUseItem = djack.RogueSurvivor.Engine.Actions.ActionUseItem;
using ActionUse = djack.RogueSurvivor.Engine.Actions.ActionUse;
using ActionTradeWithContainer = djack.RogueSurvivor.Engine.Actions.ActionTradeWithContainer;
using ActionWait = djack.RogueSurvivor.Engine.Actions.ActionWait;

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
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) return false;
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
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
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
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
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
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
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
      if (0 < (m_Actor.Controller.enemies_in_FOV?.Count ?? 0)) {
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
    readonly protected List<Objective> Objectives = new List<Objective>();
    readonly private Dictionary<Point,Dictionary<Point, int>> PlannedMoves = new Dictionary<Point, Dictionary<Point, int>>();
    readonly private sbyte[] ItemPriorities = new sbyte[(int)GameItems.IDs._COUNT]; // XXX probably should have some form of PC override
    private int _STA_reserve;
    int STA_reserve { get { return _STA_reserve; } }

    // cache variables
    [NonSerialized] protected List<Point> _legal_steps = null;
    [NonSerialized] protected Dictionary<Point, int> _damage_field = null;
    [NonSerialized] protected List<Actor> _slow_melee_threat = null;
    [NonSerialized] protected HashSet<Actor> _immediate_threat = null;
    [NonSerialized] protected HashSet<Point> _blast_field = null;
    [NonSerialized] protected List<Point> _retreat = null;
    [NonSerialized] protected List<Point> _run_retreat = null;
    [NonSerialized] protected bool _safe_retreat = false;
    [NonSerialized] protected bool _safe_run_retreat = false;

    public virtual bool UsesExplosives { get { return true; } } // default to what PC does

    public T Goal<T>(Func<T,bool> test) where T:Objective { return Objectives.FirstOrDefault(o => o is T goal && test(goal)) as T;}
    public T Goal<T>() where T:Objective { return Objectives.FirstOrDefault(o => o is T) as T;}

    public void ResetAICache()
    {
      _legal_steps = null;
      _damage_field = null;
      _slow_melee_threat = null;
      _immediate_threat = null;
      _blast_field = null;
      _retreat = null;
      _run_retreat = null;
      _safe_retreat = false;
      _safe_run_retreat = false;
    }

    // morally a constructor-type function
    protected void InitAICache(List<Percept> now, List<Percept> all_time=null)
    {
      _legal_steps = m_Actor.LegalSteps;
      _damage_field = new Dictionary<Point, int>();
      _slow_melee_threat = new List<Actor>();
      _immediate_threat = new HashSet<Actor>();
      _blast_field = new HashSet<Point>();  // thrashes GC for GangAI/CHARGuardAI
      if (null != enemies_in_FOV) VisibleMaximumDamage(_damage_field, _slow_melee_threat, _immediate_threat);
      AddTrapsToDamageField(_damage_field, now);
      if (UsesExplosives) AddExplosivesToDamageField(all_time);  // only civilians and soldiers respect explosives; CHAR and gang don't
      if (0>= _damage_field.Count) _damage_field = null;
      if (0>= _slow_melee_threat.Count) _slow_melee_threat = null;
      if (0>= _immediate_threat.Count) _immediate_threat = null;
      if (0>= _blast_field.Count) _blast_field = null;

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
    }

    [System.Flags]
    public enum ReactionCode : uint {
      NONE = 0,
      ENEMY = uint.MaxValue/2+1
    };

    // XXX return-code so we know what kind of heuristics are dominating.  Should be an enumeration or bitflag return
    // Should not need to be an override of a reduced-functionality BaseAI version
    public ReactionCode InterruptLongActivity()
    {
        ReactionCode ret = ReactionCode.NONE;
        if (null != enemies_in_FOV) ret |= ReactionCode.ENEMY;
        // \todo we should also interrupt if there is a useful item in sight (this can happen with an enemy in sight)
        // (requires items in view cache from LOSSensor, which is wasted RAM for Z; living-specific cache in savefile indicated)
        // \todo we should also interrupt if there is a valid trading apportunity in sight (this is suppressed by an enemy in sight)
        return ret;
    }

    private void AvoidBeingCornered()
    {
#if DEBUG
      if (null == _retreat) throw new ArgumentNullException(nameof(_retreat));
#endif
      if (2 > _retreat.Count) return;

      var cornered = new HashSet<Point>(_retreat);
      foreach(Point pt in Enumerable.Range(0,16).Select(i=>m_Actor.Location.Position.RadarSweep(2,i)).Where(pt=>m_Actor.Location.Map.IsWalkableFor(pt,m_Actor))) {
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) return;
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
        if (0<cornered.RemoveWhere(pt2=>Rules.IsAdjacent(pt,pt2)) && 0>=cornered.Count) return;
      }

      if (cornered.Count< _run_retreat.Count) _run_retreat.RemoveAll(pt => cornered.Contains(pt));
    }

    protected bool RunIfAdvisable(Point dest)
    {
      if (!m_Actor.CanRun()) return false;
      // we don't want preparing to push a car to block running at full stamina
      if (m_Actor.MaxSTA > m_Actor.StaminaPoints) {
        if (m_Actor.RunIsFreeMove) {
          if (m_Actor.WillTireAfter(STA_reserve + m_Actor.RunningStaminaCost(dest))) return false;
        } else {
          if (m_Actor.WillTireAfter(STA_reserve + 2*m_Actor.RunningStaminaCost(dest)- m_Actor.NightSTApenalty)) return false;
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
      if (0 >= dest.Count) return dest;
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
          approach = DowngradeApproach(test2);
          if (0<approach.Count) PlannedMoves[test.Value.Position] = approach;
        }
      }
      return dest;
    }

    protected ActorAction PlanApproachFailover(Zaimoni.Data.FloodfillPathfinder<Point> navigate)
    {
      List<Point> legal_steps = m_Actor.OnePathRange(m_Actor.Location.Map,m_Actor.Location.Position);
      if (null != legal_steps) {
        var costs = new Dictionary<Point,int>();
        foreach(Point pt in legal_steps) {
          costs[pt] = navigate.Cost(pt);
        }
        int min_cost = costs.Values.Min();
        if (int.MaxValue == min_cost) return null;
        costs.OnlyIf(val => val <= min_cost);
        if (0<costs.Count) return Rules.IsPathableFor(m_Actor,new Location(m_Actor.Location.Map,RogueForm.Game.Rules.DiceRoller.Choose(costs.Keys.ToList())));
      }
      return null;
    }

    protected ActorAction PlanApproachFailover(Zaimoni.Data.FloodfillPathfinder<Location> navigate)
    {
      Dictionary<Location,ActorAction> legal_steps = m_Actor.OnePathRange(m_Actor.Location);
      if (null != legal_steps) {
        var costs = new Dictionary<Location,int>();
        foreach(var loc_action in legal_steps) {
          costs[loc_action.Key] = navigate.Cost(loc_action.Key);
        }
        int min_cost = costs.Values.Min();
        if (int.MaxValue == min_cost) return null;
        costs.OnlyIf(val => val <= min_cost);
        if (0<costs.Count) return legal_steps[RogueForm.Game.Rules.DiceRoller.Choose(costs.Keys.ToList())];
      }
      return null;
    }

    protected void ClearMovePlan()
    {
      PlannedMoves.Clear();
    }

    public Dictionary<Point, int> MovePlanIf(Point pt)
    {
      if (   !PlannedMoves.TryGetValue(pt, out var src)
          ||  null==src)  // XXX probably being used incorrectly
        return null;
      return new Dictionary<Point,int>(src);
    }

    private List<Point> FindRetreat()
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

    private List<Point> FindRunRetreat()
    {
#if DEBUG
      if (null == _damage_field) throw new ArgumentNullException(nameof(_damage_field));
      if (null == _legal_steps) throw new ArgumentNullException(nameof(_legal_steps));
      if (!_damage_field.ContainsKey(m_Actor.Location.Position)) throw new InvalidOperationException("!damage_field.ContainsKey(m_Actor.Location.Position)");
#endif
      var ret = new HashSet<Point>(Enumerable.Range(0, 16).Select(i => m_Actor.Location.Position.RadarSweep(2, i)).Where(pt => m_Actor.Location.Map.IsWalkableFor(pt, m_Actor)));
//    ret.RemoveWhere(pt => !_legal_steps.Select(pt2 => Rules.IsAdjacent(pt,pt2)).Any());    // predicted to work but fails due to MS C# compiler bug April 12 2018
      ret.RemoveWhere(pt => !_legal_steps.Any(pt2 => Rules.IsAdjacent(pt,pt2))); // this does work and would be more efficient than the broken version above

      IEnumerable<Point> tmp_point = ret.Where(pt=>!_damage_field.ContainsKey(pt));
      if (tmp_point.Any()) return tmp_point.ToList();
      tmp_point = ret.Where(pt=> _damage_field[pt] < _damage_field[m_Actor.Location.Position]);
      return (tmp_point.Any() ? tmp_point.ToList() : null);
    }

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

    private static List<Location> DecideMove_NoJump(List<Location> src)
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
          if (!hypothetical_los.TryGetValue(test.Value.Position,out var src)) {
            taint_exposed[loc] = 0;
            continue;
          }
          HashSet<Point> tmp2 = new HashSet<Point>(src);
          tmp2.IntersectWith(tainted);
          taint_exposed[loc] = tmp2.Count;
        }
        int max_taint_exposed = dests.Select(pt=>taint_exposed[pt]).Max();
        taint_exposed.OnlyIf(val=>max_taint_exposed==val);
        return taint_exposed.Keys.ToList();
    }

    private ActorAction _finalDecideMove(List<Point> tmp)
    {
	  var secondary = new List<ActorAction>();
	  while(0<tmp.Count) {
		ActorAction ret = Rules.IsPathableFor(m_Actor, new Location(m_Actor.Location.Map, RogueForm.Game.Rules.DiceRoller.ChooseWithoutReplacement(tmp)));
        if (!ret?.IsLegal() ?? true) continue;  // not really an option
        if (ret is ActionShove shove && shove.Target.Controller is ObjectiveAI ai) {
           Dictionary<Point, int> ok_dests = ai.MovePlanIf(shove.Target.Location.Position);
           if (Rules.IsAdjacent(shove.To,m_Actor.Location.Position)) {
             // non-moving shove...would rather not spend the stamina if there is a better option
             if (null != ok_dests  && ok_dests.ContainsKey(shove.To)) secondary.Add(ret); // shove is to a wanted destination
             continue;
           }
           // discard action if the target is on an in-bounds exit (target is likely pathing through the chokepoint)
           // target should not be sleeping; check for that anyway
           if (null!=shove.Target.Location.Exit && !shove.Target.IsSleeping) continue;

           if (   null == ok_dests // shove is rude
               || !ok_dests.ContainsKey(shove.To)) // shove is not to a wanted destination
                {
                secondary.Add(ret);
                continue;
                }
        }
		return ret;
	  }
      return 0 < secondary.Count ? RogueForm.Game.Rules.DiceRoller.Choose(secondary) : null;
    }

    protected ActorAction DecideMove(IEnumerable<Point> src)
	{
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
	  List<Point> tmp = src.ToList();
      if (1 >= tmp.Count) return _finalDecideMove(tmp);

	  // do not get in the way of allies' line of fire
	  tmp = DecideMove_Avoid(tmp, FriendsLoF());
      if (1 >= tmp.Count) return _finalDecideMove(tmp);

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  bool want_LOS_heuristics = false;
	  ThreatTracking threats = m_Actor.Threats;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  if (null != threats || null != sights_to_see) want_LOS_heuristics = true;

	  var hypothetical_los = (want_LOS_heuristics ? new Dictionary<Point,HashSet<Point>>() : null);
      var new_los = new HashSet<Point>();
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

      if (null != threats) {
        tmp = DecideMove_maximize_visibility(tmp, threats.ThreatWhere(m_Actor.Location.Map, view), new_los, hypothetical_los);
        if (1 >= tmp.Count) return _finalDecideMove(tmp);
	  }
	  if (null != sights_to_see) {
        HashSet<Point> inspect = sights_to_see.In(m_Actor.Location.Map, view);
        if (null!=inspect) tmp = DecideMove_maximize_visibility(tmp, inspect, new_los, hypothetical_los);
        if (1 >= tmp.Count) return _finalDecideMove(tmp);
	  }

      // weakly prefer not to jump
      tmp = DecideMove_NoJump(tmp);
      return _finalDecideMove(tmp);
	}

    private ActorAction _finalDecideMove(List<Location> tmp, Dictionary<Location, ActorAction> legal_steps)
    {
	  var secondary = new List<ActorAction>();
	  while(0<tmp.Count) {
        var dest = RogueForm.Game.Rules.DiceRoller.ChooseWithoutReplacement(tmp);
        if (!legal_steps.TryGetValue(dest, out var ret) || !ret.IsLegal()) continue; // not really an option
        if (ret is ActionUseExit use_exit && string.IsNullOrEmpty(use_exit.Exit.ReasonIsBlocked(m_Actor))) {
          continue;
        };
        if (ret is ActionShove shove && shove.Target.Controller is ObjectiveAI ai) {
           Dictionary<Point, int> ok_dests = ai.MovePlanIf(shove.Target.Location.Position);
           if (Rules.IsAdjacent(shove.To,m_Actor.Location.Position)) {
             // non-moving shove...would rather not spend the stamina if there is a better option
             if (null != ok_dests  && ok_dests.ContainsKey(shove.To)) secondary.Add(ret); // shove is to a wanted destination
             continue;
           }
           // discard action if the target is on an in-bounds exit (target is likely pathing through the chokepoint)
           // target should not be sleeping; check for that anyway
           if (null!=shove.Target.Location.Exit && !shove.Target.IsSleeping) continue;

           if (   null == ok_dests // shove is rude
               || !ok_dests.ContainsKey(shove.To)) // shove is not to a wanted destination
                {
                secondary.Add(ret);
                continue;
                }
        }
		return ret;
	  }
      return 0 < secondary.Count ? RogueForm.Game.Rules.DiceRoller.Choose(secondary) : null;
    }

    public bool VetoAction(ActorAction x)
    {
      if (x is ActionMoveStep step) {   // XXX telepathy
        Exit exitAt = m_Actor.Location.Map.GetExitAt(step.dest.Position);
        Actor actorAt = exitAt?.Location.Actor;
        if (null!=actorAt && !m_Actor.IsEnemyOf(actorAt)) return true;
      }
      if (x is ActionShove shove) {
        if (_blast_field?.Contains(shove.To) ?? false) return true;   // exceptionally hostile to shove into an explosion
        if (_damage_field?.ContainsKey(shove.To) ?? false) return true;   // hostile to shove into a damage field

        if (shove.Target.Controller is ObjectiveAI ai) {
          Dictionary<Point, int> ok_dests = ai.MovePlanIf(shove.Target.Location.Position);
          if (Rules.IsAdjacent(shove.To,m_Actor.Location.Position)) {
            // non-moving shove...would rather not spend the stamina if there is a better option
            if (null != ok_dests  && ok_dests.ContainsKey(shove.To)) return false; // shove is to a wanted destination
            return true;
          }
          // discard action if the target is on an in-bounds exit (target is likely pathing through the chokepoint)
          // target should not be sleeping; check for that anyway
          if (null!=shove.Target.Location.Exit && !shove.Target.IsSleeping) return true;
/*
           if (   null == ok_dests // shove is rude
               || !ok_dests.ContainsKey(shove.To)) // shove is not to a wanted destination
               return tmp;
*/
        }
      }
      if (x is ActionUseExit exit && exit.IsBlocked) return true;

      return false;
    }

    protected ActorAction DecideMove(Dictionary<Location,int> src)
	{
      if (null == src) return null; // does happen
      var legal_steps = m_Actor.OnePathRange(m_Actor.Location); // other half
      legal_steps.OnlyIf(action => !VetoAction(action));
      // XXX \todo if there are maps we do not want to path to, screen those here
	  List<Location> tmp = src.Keys.ToList();
      if (1 >= tmp.Count) return _finalDecideMove(tmp,legal_steps);

	  // do not get in the way of allies' line of fire
	  tmp = DecideMove_Avoid(tmp, FriendsLoF());
      if (1 >= tmp.Count) return _finalDecideMove(tmp,legal_steps);

      // XXX if we have priority-see locations, maximize that
      // XXX if we have threat tracking, maximize threat cleared
      // XXX if we have item memory, maximize "update"
	  bool want_LOS_heuristics = false;
	  ThreatTracking threats = m_Actor.Threats;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  if (null != threats || null != sights_to_see) want_LOS_heuristics = true;

	  var hypothetical_los = want_LOS_heuristics ? new Dictionary<Point,HashSet<Point>>() : null;
      var new_los = new HashSet<Point>();
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

      // weakly prefer not to jump
      tmp = DecideMove_NoJump(tmp);
      return _finalDecideMove(tmp,legal_steps);
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
      return DecideMove(dests.Keys);
	}

    private ActionMoveStep _finalDecideMove(IEnumerable<Point> src, List<Point> tmp2)
    {
      // filter down intermediate destinations
      IEnumerable<Point> tmp3 = src.Where(pt => tmp2.Any(pt2 => Rules.IsAdjacent(pt,pt2)));
      if (!tmp3.Any()) return null;
      var tmp = tmp3.ToList();

      while (0<tmp.Count) {
		ActorAction ret = Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, RogueForm.Game.Rules.DiceRoller.ChooseWithoutReplacement(tmp)));
        if (ret is ActionMoveStep step && step.IsLegal()) {
          RunIfPossible();
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
	  bool want_LOS_heuristics = false;
	  ThreatTracking threats = m_Actor.Threats;
	  LocationSet sights_to_see = m_Actor.InterestingLocs;
	  if (null != threats || null != sights_to_see) want_LOS_heuristics = true;

	  var hypothetical_los = want_LOS_heuristics ? new Dictionary<Point,HashSet<Point>>() : null;
      var new_los = new HashSet<Point>();
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

      int tmp_LOSrange = m_Actor.FOVrange(m_Actor.Location.Map.LocalTime, Session.Get.World.Weather) + 2;
      Rectangle view = new Rectangle(m_Actor.Location.Position.X - tmp_LOSrange, m_Actor.Location.Position.Y - tmp_LOSrange, 2*tmp_LOSrange+1,2*tmp_LOSrange+1);

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
		trap_damage_field[pt] = m_Actor.Location.Map.TrapsMaxDamageAtFor(pt,m_Actor);
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

    public void ExecuteActionChain(IEnumerable<ActorAction> actions)
    {
      int insertAt = -2;
      foreach(ActorAction action in actions) {
        insertAt++;
        if (0 > insertAt) {
          action.Perform();
          continue;
        }
        Objectives.Insert(insertAt,new Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, action));
       }
    }

    private List<Location> Goals(Func<Map, HashSet<Point>> targets_at, Map dest)
    {
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": OrderableAI::Goals (depth 1)");
#endif
      var goals = new List<Location>();
      HashSet<Point> where_to_go = targets_at(dest);
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": where_to_go " + where_to_go.to_s());
#endif
      if (0 < where_to_go.Count) {
        foreach(Point pt in where_to_go) goals.Add(new Location(dest,pt));
      }

      var already_seen = new List<Map>{ dest };

      // The SWAT team can have a fairly impressive pathing degeneration at game start (they want their heavy hammers, etc.)
      if (0==where_to_go.Count) {
        var maps = new HashSet<Map>(dest.destination_maps.Get);
        if (1<maps.Count) {
          foreach(Map m in maps.ToList()) {
            if (1<m.destination_maps.Get.Count) continue;
            HashSet<Point> go_here = targets_at(m);
            if (0<go_here.Count) {
              foreach(Point pt in go_here) goals.Add(new Location(m,pt));
              already_seen.Add(m);
              continue;
            }
            maps.Remove(m);
          }
        }
        if (1==maps.Count && 0==goals.Count) {
          Dictionary<Point,Exit> exits = dest.GetExits(e => maps.Contains(e.ToMap));
          foreach(var pos_exit in exits) {
            Location loc = new Location(dest, pos_exit.Key);
            goals.Add(loc==m_Actor.Location ? pos_exit.Value.Location : loc);
          }
#if TRACE_GOALS
          if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name + ": short-circuit exit " + goals.to_s());
#endif
          return goals;
        }
        // if that isn't enough, we could also use the police and hospital geometries
      } 

      foreach(Map m in dest.destination_maps.Get) {
        if (already_seen.Contains(m)) continue;
        Goals(targets_at,m,already_seen,goals);
      }
      return goals;
    }

    private List<Location> Goals(Func<Map, HashSet<Point>> targets_at, Map dest, List<Map> already_seen, List<Location> goals)
    {
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": OrderableAI::Goals (depth 2+)");
#endif
      HashSet<Point> where_to_go = targets_at(dest);
      if (0 < where_to_go.Count) {
        foreach(Point pt in where_to_go) goals.Add(new Location(dest,pt));
      }
#if TRACE_GOALS
      if (m_Actor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, m_Actor.Name+ ": where_to_go " + where_to_go.to_s());
#endif

      already_seen.Add(dest);

      foreach(Map m in dest.destination_maps.Get) {
        if (already_seen.Contains(m)) continue;
        Goals(targets_at,m,already_seen,goals);
      }
      return goals;
    }

    protected FloodfillPathfinder<Location> PathfinderFor(List<Location> goals, Map dest)
    {
#if DEBUG
      if (0 >= (goals?.Count ?? 0)) throw new ArgumentNullException(nameof(goals));
#endif
      var navigate = dest.PathfindLocSteps(m_Actor);

      navigate.GoalDistance(goals, m_Actor.Location);
      return navigate;
    }

    protected ActorAction BehaviorPathTo(FloodfillPathfinder<Location> navigate)
    {
      if (null == navigate) return null;
      if (!navigate.Domain.Contains(m_Actor.Location)) return null;
      ActorAction ret = DecideMove(navigate.Approach(m_Actor.Location));
      if (null == ret) ret = PlanApproachFailover(navigate);
      if (null == ret) return null;
      if (ret is ActionMoveStep test) m_Actor.IsRunning = RunIfAdvisable(test.dest.Position); // XXX should be more tactically aware
      return ret;
    }

    public ActorAction BehaviorPathTo(Func<Map,HashSet<Point>> targets_at)
    {
      List<Location> goals = Goals(targets_at, m_Actor.Location.Map);
      if (0 >= goals.Count) return null;
      {
      var already = new Dictionary<Location, ActorAction>();
      Dictionary<Location, ActorAction> moves = m_Actor.OnePath(m_Actor.Location, already);
      foreach(Location loc in goals) {
        if (moves.TryGetValue(loc,out var tmp)) {
          if (!tmp.IsLegal()) return null;
          if (VetoAction(tmp)) return null;
          return tmp;
        }
      }
      }

      return BehaviorPathTo(PathfinderFor(goals,m_Actor.Location.Map));
    }

     public void GoalHeadFor(Map m, HashSet<Point> dest)
     {
       var locs = new HashSet<Location>(dest.Select(pt => new Location(m,pt)));
       Objectives.Insert(0,new Goal_PathTo(m_Actor.Location.Map.LocalTime.TurnCounter,m_Actor,locs));
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
        int a_turns_bak = a_turns;
        if (0 >= a_turns) continue; // morally if (!a.CanActNextTurn) continue;
        if (0==a.CurrentRangedAttack.Range && 1 == Rules.GridDistance(m_Actor.Location.Position, where_enemy.Key) && m_Actor.Speed>a.Speed) slow_melee_threat.Add(a);
        // calculate melee damage field now
        Dictionary<Point,int> melee_damage_field = new Dictionary<Point,int>();
        int a_max_dam = a.MeleeAttack(m_Actor).DamageValue;
        foreach(Point pt in Direction.COMPASS.Select(dir=>a.Location.Position+dir).Where(pt=>map.IsValid(pt) && map.GetTileModelAtExt(pt).IsWalkable)) {
          melee_damage_field[pt] = a_turns*a_max_dam;
        }
        while(1<a_turns) {
          HashSet<Point> sweep = new HashSet<Point>(melee_damage_field.Keys);
          a_turns--;
          foreach(Point pt2 in sweep) {
            foreach(Point pt in Direction.COMPASS.Select(dir=>pt2+dir).Where(pt=>map.IsValid(pt) && map.GetTileModelAtExt(pt).IsWalkable && !sweep.Contains(pt))) {
              melee_damage_field[pt] = a_turns*a_max_dam;
            }
          }
        }
        if (melee_damage_field.ContainsKey(m_Actor.Location.Position)) {
          immediate_threat.Add(a);
        }
        // we can do melee attack damage field without FOV
        // FOV doesn't matter without a ranged attack
        // XXX doesn't handle non-optimal ranged attacks
        if (0 >= a.CurrentRangedAttack.Range) {
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
          int dist = Rules.GridDistance(pt, a.Location.Position);
          a_max_dam = a.RangedAttack(dist, m_Actor).DamageValue;
          if (dist <= a.CurrentRangedAttack.Range) {
            ranged_damage_field[pt] = a_turns*a_max_dam;
          }
        }
        if (1<a_turns) {
          HashSet<Point> already = new HashSet<Point>();
          HashSet<Point> now = new HashSet<Point>{ a.Location.Position };
          do {
            a_turns--;
            HashSet<Point> tmp2 = a.NextStepRange(a.Location.Map,already,now);
            if (null == tmp2) break;
            foreach(Point pt2 in tmp2) {
              aFOV = LOS.ComputeFOVFor(a,new Location(a.Location.Map,pt2));
              aFOV.ExceptWith(ranged_damage_field.Keys);
              foreach(Point pt in aFOV) {
                int dist = Rules.GridDistance(pt, a.Location.Position);
                a_max_dam = a.RangedAttack(dist, m_Actor).DamageValue;
                if (dist <= a.CurrentRangedAttack.Range) {
                  ranged_damage_field[pt] = a_turns*a_max_dam;
                }
              }
            }
            already.UnionWith(now);
            now = tmp2;
          } while(1<a_turns);
        }
        if (ranged_damage_field.ContainsKey(m_Actor.Location.Position)) {
          immediate_threat.Add(a);
        }
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

    private void AddExplosivesToDamageField(List<Percept_<Inventory>> goals)
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
        int r = 0;
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

    private void AddExplosivesToDamageField(List<Percept> percepts)
    {
      AddExplosivesToDamageField(percepts.FilterCast<Inventory>(inv => inv.Has<ItemPrimedExplosive>()));
    }

    private void AddTrapsToDamageField(Dictionary<Point,int> damage_field, List<Percept> percepts)
    {
      List<Percept> goals = percepts.FilterT<Inventory>(inv => inv.Has<ItemTrap>());
      if (null == goals) return;
      foreach(Percept p in goals) {
        if (p.Location==m_Actor.Location) continue; // trap has already triggered, or not: safe
        List<ItemTrap> tmp = (p.Percepted as Inventory).GetItemsByType<ItemTrap>();
        if (null == tmp) continue;
        int damage = tmp.Sum(trap => (trap.IsActivated ? trap.Model.Damage : 0));   // XXX wrong for barbed wire
        if (0 >= damage) continue;
        if (damage_field.ContainsKey(p.Location.Position)) damage_field[p.Location.Position] += damage;
        else damage_field[p.Location.Position] = damage;
      }
    }
#endregion

    public bool InCommunicationWith(Actor a)
    {
      if (m_Actor==a) return true;
      if (!(a.Controller is OrderableAI) && !(a.Controller is PlayerController)) return false;
      if (a.IsSleeping) return false;
      if (a.Location.Map == m_Actor.Location.Map && a.Controller.CanSee(m_Actor.Location) && m_Actor.Controller.CanSee(a.Location)) return true;
      if (a.HasActivePoliceRadio && m_Actor.HasActivePoliceRadio) return true;
      if (a.HasActiveArmyRadio && m_Actor.HasActiveArmyRadio) return true;
      if (null!=a.GetEquippedCellPhone() && null!=m_Actor.GetEquippedCellPhone()) return true;
      return false;
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

    abstract protected ActorAction BehaviorWouldGrabFromStack(Location loc, Inventory stack);

    protected List<Percept> GetInterestingInventoryStacks(IEnumerable<Percept> src)
    {
      if (!src?.Any() ?? true) return null;
      // following needs to be more sophisticated.
      // 1) identify all stacks, period.
      // 2) categorize stacks by whether they are personally interesting or not.
      // 3) in-communication followers will be consulted regarding the not-interesting stacks
      Map map = m_Actor.Location.Map;
      int t0 = map.LocalTime.TurnCounter;
      var examineStacks = new List<Percept>(src.Count());
      var boringStacks = new List<Percept>(src.Count());
      foreach(Percept p in src) {
        if (!(p.Percepted is Inventory inv)) continue;
        if (p.Turn != t0) continue;    // not in sight
        if (m_Actor.StackIsBlocked(p.Location, out MapObject mapObjectAt)) continue; // XXX ignore items under barricades or fortifications
        if (!BehaviorWouldGrabFromStack(p.Location, p.Percepted as Inventory)?.IsLegal() ?? true) {
          boringStacks.Add(p);
          continue;
        }
        examineStacks.Add(p);
      }
      if (0 < boringStacks.Count) AdviseCellOfInventoryStacks(boringStacks);    // XXX \todo PC leader should do the same
      if (0 >= examineStacks.Count) return null;

      bool imStarvingOrCourageous = m_Actor.IsStarving;
      if ((this is OrderableAI ai) && ActorCourage.COURAGEOUS == ai.Directives.Courage) imStarvingOrCourageous = true;
      return examineStacks.FilterT<Inventory>().FilterOut(p => {
          if (IsOccupiedByOther(p.Location)) return true; // blocked
          if (!m_Actor.MayTakeFromStackAt(p.Location)) {    // something wrong, e.g. iron gates in way
            if (!imStarvingOrCourageous && map.TrapsMaxDamageAtFor(p.Location.Position,m_Actor) >= m_Actor.HitPoints) return true;  // destination deathtrapped
            // check for iron gates, etc in way
            List<List<Point> > path = m_Actor.MinStepPathTo(m_Actor.Location, p.Location);
            if (null == path) return true;
            List<Point> test = path[0].Where(pt => null != Rules.IsBumpableFor(m_Actor, new Location(m_Actor.Location.Map, pt))).ToList();
            if (0 >= test.Count) return true;
            path[0] = test;
            if (!imStarvingOrCourageous && path[0].Any(pt=> map.TrapsMaxDamageAtFor(pt,m_Actor) >= m_Actor.HitPoints)) return true;
          }
          return false;
        });
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

    public List<Item> GetTradeableItems()
    {
      Inventory inv = m_Actor.Inventory;
      if (inv == null) return null;
      IEnumerable<Item> ret = inv.Items.Where(it => IsTradeableItem(it));
      return ret.Any() ? ret.ToList() : null;
    }

    protected List<Percept> GetTradingTargets(IEnumerable<Percept> friends)
    {
        if (!m_Actor.Model.Abilities.CanTrade) return null; // arguably an invariant but not all PCs are overriding appropriate base AIs
        var TradeableItems = GetTradeableItems();
        if (0>=(TradeableItems?.Count ?? 0)) return null;
        Map map = m_Actor.Location.Map;

        return friends.FilterOut(p => {
          if (p.Turn != map.LocalTime.TurnCounter) return true;
          Actor actor = p.Percepted as Actor;
          if (actor.IsPlayer) return true;
          if (this is OrderableAI ai && ai.IsActorTabooTrade(actor)) return true;
          if (!m_Actor.CanTradeWith(actor)) return true;
          if (null==m_Actor.MinStepPathTo(m_Actor.Location, p.Location)) return true;    // something wrong, e.g. iron gates in way.  Usual case is police visiting jail.
          if (1 == TradeableItems.Count) {
            List<Item> other_TradeableItems = (actor.Controller as OrderableAI).GetTradeableItems();
            if (null == other_TradeableItems) return true;
            if (1 == other_TradeableItems.Count && TradeableItems[0].Model.ID== other_TradeableItems[0].Model.ID) return true;
          }
          return !(actor.Controller as OrderableAI).HasAnyInterestingItem(TradeableItems);    // other half of m_Actor.GetInterestingTradeableItems(...)
        });
    }

    private ActorAction _PrefilterDrop(Item it)
    {
      // use stimulants before dropping them
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        if (m_Actor.Inventory.GetBestDestackable(it) is ItemMedicine stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
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
            RogueForm.Game.DoEquipItem(m_Actor,rw);
            return new ActionUseItem(m_Actor, ammo);
          }
        }
      }
      } // end scoping brace
      return null;
    }

    protected ActorAction BehaviorDropItem(Item it)
    {
      if (it == null) return null;
      ActorAction tmp = _PrefilterDrop(it);
      if (null != tmp) return tmp;

      // use stimulants before dropping them
      if (GameItems.IDs.MEDICINE_PILLS_SLP == it.Model.ID) {
        if (m_Actor.Inventory.GetBestDestackable(it) is ItemMedicine stim2) {
          int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
          int num4 = Rules.ActorMedicineEffect(m_Actor, stim2.SleepBoost);
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
            RogueForm.Game.DoEquipItem(m_Actor,rw);
            return new ActionUseItem(m_Actor, ammo);
          }
        }
      }
      } // end scoping brace


      if (m_Actor.CanUnequip(it)) RogueForm.Game.DoUnequipItem(m_Actor,it);

      List<Point> has_container = new List<Point>();
      foreach(Point pos in Direction.COMPASS.Select(dir => m_Actor.Location.Position+dir)) {
        MapObject container = m_Actor.Location.Map.GetMapObjectAtExt(pos);
        if (!container?.IsContainer ?? true) continue;
        Inventory itemsAt = m_Actor.Location.Map.GetItemsAt(pos);
        if (null != itemsAt)
          {
          if (itemsAt.CountItems+1 >= itemsAt.MaxCapacity) continue; // practical consideration
#if DEBUG
          if (itemsAt.IsFull) throw new InvalidOperationException("illegal put into container attempted");
#endif
          }
#if DEBUG
        if (!RogueForm.Game.Rules.CanActorPutItemIntoContainer(m_Actor, pos)) throw new InvalidOperationException("illegal put into container attempted");
#endif
        has_container.Add(pos);
      }
      if (0 < has_container.Count) return new ActionPutInContainer(m_Actor, it, RogueForm.Game.Rules.DiceRoller.Choose(has_container));

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

      return false;
    }

    private int ItemRatingCode_generic(Item it)
    {
        if (!m_Actor.Inventory.Contains(it) && m_Actor.HasAtLeastFullStackOf(it, 1)) return 0;
        return 1;
    }

    private int ItemRatingCode_generic(ItemModel it)
    {
        return m_Actor.HasAtLeastFullStackOf(it, 1) ? 0 : 1;
    }

    private int ItemRatingCode(ItemTracker it)
    {
      List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
      if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);

      // AI does not yet use z-trackers or blackops trackers correctly; possible only threat-aware AIs use them
      if (m_Actor.Inventory.Contains(it)) return (ok_trackers.Contains(it.Model.ID) && null!=m_Actor.LiveLeader) ? 2 : 1;
      if (m_Actor.Inventory.Items.Any(obj => !obj.IsUseless && obj.Model == it.Model)) return 0;
      return (ok_trackers.Contains(it.Model.ID) && null != m_Actor.LiveLeader) ? 2 : 1;
    }

    private int ItemRatingCode(ItemEntertainment it)
    {
      if (!m_Actor.Model.Abilities.HasSanity) return 0;
      if (!m_Actor.Inventory.Contains(it) && m_Actor.HasAtLeastFullStackOf(it, 1)) return 0;
      if (m_Actor.IsDisturbed) return 3;
      if (m_Actor.Sanity < 3 * m_Actor.MaxSanity / 4) return 2;   // gateway expression for using entertainment
      return 1;
    }

    private int ItemRatingCode(ItemLight it)
    {
      if (m_Actor.Inventory.Contains(it)) return 2;
      if (m_Actor.Inventory.Items.Any(obj => !obj.IsUseless && obj is ItemLight)) return 0;
      return 2;   // historically low priority but ideal kit has one
    }

    // XXX sleep and stamina pills have special uses for sufficiently good AI
    // XXX sanity pills should be treated like entertainment
    private int ItemRatingCode(ItemMedicine it)
    {
      if (m_Actor.Inventory.Contains(it)) return 1;
      if (m_Actor.HasAtLeastFullStackOf(it, m_Actor.Inventory.IsFull ? 1 : 2)) return 0;
      return 1;
    }

    private int ItemRatingCode(ItemBodyArmor armor)
    {
      ItemBodyArmor best = m_Actor.GetBestBodyArmor();
      if (null == best) return 2; // want 3, but RHSMoreInteresting  says 2
      if (best == armor) return 3;
      return best.Rating < armor.Rating ? 2 : 0; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
    }

    private int ItemRatingCode(ItemMeleeWeapon melee)
    {
      int rating = m_Actor.MeleeWeaponAttack(melee.Model).Rating;
      if (rating <= m_Actor.UnarmedMeleeAttack().Rating) return 0;
      int? best_rating = m_Actor.GetBestMeleeWeaponRating();    // rely on OrderableAI doing the right thing
      if (null == best_rating) return 2;  // martial arts invalidates starting baton for police
      if (best_rating < rating) return 2;
      if (best_rating > rating) return 1;
      int melee_count = m_Actor.CountQuantityOf<ItemMeleeWeapon>(); // XXX possibly obsolete
      if (m_Actor.Inventory.Contains(melee)) return 1 == melee_count ? 2 : 1;
      if (2 <= melee_count) {
        ItemMeleeWeapon worst = m_Actor.GetWorstMeleeWeapon();
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
      bool is_in_inventory = m_Actor.Inventory.Contains(am);

      ItemRangedWeapon rw = m_Actor.Inventory.GetCompatibleRangedWeapon(am);
      if (null == rw) {
        int potential_importance = KnowRelevantInventory(am) ? 2 : 1;
        if (is_in_inventory) return potential_importance;
        if (0 < m_Actor.Inventory.Count(am.Model)) return 0;
        if (AmmoAtLimit) return int.MaxValue;  // BehaviorMakeRoomFor triggers recursion. real value 0 or potential_importance
        return potential_importance;
      }
      if (is_in_inventory) return 2;
      if (rw.Ammo < rw.Model.MaxAmmo) return 2;
      if (m_Actor.HasAtLeastFullStackOf(am, 2)) return 0;
      if (null != m_Actor.Inventory.GetFirstByModel<ItemAmmo>(am.Model,am2=>am2.Quantity<am.Model.MaxQuantity)) return 2;
      if (AmmoAtLimit) return int.MaxValue;  // BehaviorMakeRoomFor triggers recursion. real value 0 or 2
      return 2;
    }

    private int ItemRatingCode(ItemRangedWeapon rw)
    { // similar to IsInterestingItem(rw)
      if (m_Actor.Inventory.Contains(rw)) return 0<rw.Ammo ? 3 : 1;
      int rws_w_ammo = m_Actor.Inventory.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo);
      if (0 < rws_w_ammo) {
        if (null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, obj => 0 < obj.Ammo)) return 0;    // XXX
        if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(obj => obj.AmmoType==rw.AmmoType && 0 < obj.Ammo)) return 0; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
      }
      if (0 < rw.Ammo && null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, obj => 0 == obj.Ammo)) return 3;  // this replacement is ok; implies not having ammo
      ItemAmmo compatible = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
      if (0 >= rw.Ammo && null == compatible) return 1;
      if (0 >= rws_w_ammo && null != compatible) return 3;
      // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
      // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
      if (AmmoAtLimit && null==compatible) return 0;
      if (0< rws_w_ammo) return 2;
      return 3;
    }

    private int ItemRatingCode(ItemGrenade grenade)
    {
      if (m_Actor.Inventory.Contains(grenade)) return 2;
      if (m_Actor.Inventory.IsFull) return 1;
      if (m_Actor.HasAtLeastFullStackOf(grenade, 1)) return 1;
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
        List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
        if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
        if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);
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
          if (needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.MEDIKIT.Healing)) return 3;    // second aid
        }

        if (GameItems.IDs.MEDICINE_BANDAGES == it.ID && needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.BANDAGE.Healing)) {
          return 2; // first aid
        }

        if (m_Actor.HasAtLeastFullStackOf(it, m_Actor.Inventory.IsFull ? 1 : 2)) return 0;
        return 1;
      }
      {
      if (it is ItemBodyArmorModel armor) {
        ItemBodyArmor best = m_Actor.GetBestBodyArmor();
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
      if (it is ItemRangedWeaponModel rw) {
        int rws_w_ammo = m_Actor.Inventory.CountType<ItemRangedWeapon>(obj => 0 < obj.Ammo);
        if (0 < rws_w_ammo) {
          if (null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw, obj => 0 < obj.Ammo)) return 0;    // XXX
          if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(obj => obj.AmmoType==rw.AmmoType && 0 < obj.Ammo)) return 0; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
        }
        ItemAmmo am = m_Actor.Inventory.GetCompatibleAmmoItem(rw);
        if (0 >= rws_w_ammo && null != am) return 3;
        if (!AmmoAtLimit && null != am) return 3;
        // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
        // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
        if (m_Actor.Inventory.MaxCapacity-5 <= rws_w_ammo) return 0;
        if (m_Actor.Inventory.MaxCapacity-4 <= rws_w_ammo + m_Actor.Inventory.CountType<ItemAmmo>()) return 0;
        if (/* 0 >= rw.Ammo && */ null == am) return 0;  // XXX assume no ammo because information not available at this level
        if (0< rws_w_ammo) return 2;
        return 3;
      }
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

    protected ActorAction BehaviorDropUselessItem() // XXX would be convenient if this were fast-failing
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      foreach (Item it in m_Actor.Inventory.Items) {
        if (ItemIsUseless(it)) return BehaviorDropItem(it); // allows recovering cleanly from bugs and charismatic trades
      }

      // strict domination checks
      ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
      if (null != armor && 2 <= m_Actor.CountQuantityOf<ItemBodyArmor>()) return BehaviorDropItem(armor);

      ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
      if (null != weapon) {
        if (m_Actor.MeleeWeaponAttack(weapon.Model).Rating <= m_Actor.UnarmedMeleeAttack().Rating) return BehaviorDropItem(weapon);
      }

      ItemRangedWeapon rw = m_Actor.Inventory.GetFirstMatching<ItemRangedWeapon>(it => 0==it.Ammo && 2<=m_Actor.Count(it.Model));
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

    private ActorAction _BehaviorDropOrExchange(Item give, Item take, Point? position)
    {
      ActorAction tmp = _PrefilterDrop(give);
      if (null != tmp) return tmp;
      if (null != position) return new ActionTradeWithContainer(m_Actor,give,take,position.Value);
      return BehaviorDropItem(give);
    }

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
        if (lhs is BatteryPowered)
          {
          return ((lhs as BatteryPowered).Batteries < (rhs as BatteryPowered).Batteries);
          }
        else if (lhs is ItemFood && (lhs as ItemFood).IsPerishable)
          { // complicated
          int need = m_Actor.MaxFood - m_Actor.FoodPoints;
          int lhs_nutrition = (lhs as ItemFood).NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter);
          int rhs_nutrition = (rhs as ItemFood).NutritionAt(m_Actor.Location.Map.LocalTime.TurnCounter);
          if (lhs_nutrition==rhs_nutrition) return false;
          if (need < lhs_nutrition && need >= rhs_nutrition) return true;
          if (need < rhs_nutrition && need >= lhs_nutrition) return false;
          return lhs_nutrition < rhs_nutrition;
          }
        else if (lhs is ItemRangedWeapon)
          {
          return ((lhs as ItemRangedWeapon).Ammo < (rhs as ItemRangedWeapon).Ammo);
          }
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
      // medicine currently is, but that's an AI flaw

      // XXX note that sleep and stamina have special uses for sufficiently good AI
      bool lhs_low_priority = (lhs is ItemLight) || (lhs is ItemTrap) || (lhs is ItemMedicine) || (lhs is ItemEntertainment) || (lhs is ItemBarricadeMaterial);
      if ((rhs is ItemLight) || (rhs is ItemTrap) || (rhs is ItemMedicine) || (rhs is ItemEntertainment) || (rhs is ItemBarricadeMaterial)) return !lhs_low_priority;
      else if (lhs_low_priority) return false;

      List<GameItems.IDs> ok_trackers = new List<GameItems.IDs>();
      if (m_Actor.NeedActiveCellPhone) ok_trackers.Add(GameItems.IDs.TRACKER_CELL_PHONE);
      if (m_Actor.NeedActivePoliceRadio) ok_trackers.Add(GameItems.IDs.TRACKER_POLICE_RADIO);

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
#if DEBUG
      if (!src?.Any() ?? true) return null;
#endif
      T worst = null;
      foreach(T test in src) {
        if (null == worst) worst = test;
        else if (RHSMoreInteresting(test,worst)) worst = test;
      }
      return worst;
    }

    protected ActorAction BehaviorMakeRoomFor(Item it, Point? position=null)
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
      var useless = inv.Items.Where(obj => ItemIsUseless(obj)).ToList();
      if (0<useless.Count) return _BehaviorDropOrExchange(useless[0], it, position);
      }

      // not-best body armor can be dropped
      if (2<=m_Actor.CountQuantityOf<ItemBodyArmor>()) {
        ItemBodyArmor armor = m_Actor.GetWorstBodyArmor();
        if (null != armor) return _BehaviorDropOrExchange(armor,it,position);
      }

      { // not-best melee weapon can be dropped
        List<ItemMeleeWeapon> melee = inv.GetItemsByType<ItemMeleeWeapon>();
        if (null != melee) {
          ItemMeleeWeapon weapon = m_Actor.GetWorstMeleeWeapon();
          if (2<=melee.Count) return _BehaviorDropOrExchange(weapon, it, position);
          if (it is ItemMeleeWeapon new_melee && m_Actor.MeleeWeaponAttack(weapon.Model).Rating < m_Actor.MeleeWeaponAttack(new_melee.Model).Rating) return _BehaviorDropOrExchange(weapon, it, position);
        }
      }

      // another behavior is responsible for pre-emptively eating perishable food
      // canned food is normally eaten at the last minute
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
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4 <= need && m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
      }
      }

      { // see if we can eat our way to a free slot
      if (inv.GetBestDestackable(GameItems.CANNED_FOOD) is ItemFood food) {
        // inline part of OrderableAI::GetBestPerishableItem, OrderableAI::BehaviorEat
        int need = m_Actor.MaxFood - m_Actor.FoodPoints;
        int num4 = m_Actor.CurrentNutritionOf(food);
        if (num4*food.Quantity <= need && m_Actor.CanUse(food)) return new ActionUseItem(m_Actor, food);
      }
      }

      { // finisbing off stimulants to get a free slot is ok
      if (inv.GetBestDestackable(GameItems.PILLS_SLP) is ItemMedicine stim) {
        int need = m_Actor.MaxSleep - m_Actor.SleepPoints;
        int num4 = Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost);
        if (num4*stim.Quantity <= need && m_Actor.CanUse(stim)) return new ActionUseItem(m_Actor, stim);
      }
      }

      int it_rating = ItemRatingCode_no_recursion(it);
      if (1==it_rating && it is ItemMeleeWeapon) return null;   // break action loop here
      if (1<it_rating) {
        // generally, find a less-critical item to drop
        // this is expected to correctly handle the food glut case (item rating 1)
        int i = 0;
        while(++i < it_rating) {
          Item worst = GetWorst(m_Actor.Inventory.Items.Where(obj => ItemRatingCode_no_recursion(obj) == i));
          if (null == worst) continue;
          return _BehaviorDropOrExchange(worst, it, position);
        }
      }

      if (it is ItemAmmo am) {
        ItemRangedWeapon rw = m_Actor.Inventory.GetCompatibleRangedWeapon(am);
        if (null != rw && rw.Ammo < rw.Model.MaxAmmo) {
          // we really do need to reload this.
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
            List<ActorAction> recover = new List<ActorAction>(3);
            if (null != position) {
              ActorAction tmp = _PrefilterDrop(drop);
              if (null != tmp) return tmp;

              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionTradeWithContainer(m_Actor,drop,it,position.Value));
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
        if (   (GameItems.MEDIKIT == it.Model && needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.MEDIKIT.Healing))
            || (GameItems.BANDAGE == it.Model && needHP >= Rules.ActorMedicineEffect(m_Actor, GameItems.BANDAGE.Healing)))
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
            List<ActorAction> recover = new List<ActorAction>(3);
            if (null != position) {
              ActorAction tmp = _PrefilterDrop(drop);
              if (null != tmp) return tmp;

              // 3a) drop target without triggering the no-pickup schema
              recover.Add(new ActionTradeWithContainer(m_Actor,drop,it,position.Value));
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
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null != armor && armor.Rating < (it as ItemBodyArmor).Rating) {
          return _BehaviorDropOrExchange(armor, it, position);
        }
      }

      // medicine glut ... drop it
      foreach(GameItems.IDs x in GameItems.medicine) {
        if (it.Model.ID == x) continue;
        ItemModel model = Models.Items[(int)x];
        if (2>m_Actor.Count(model)) continue;
        Item tmp = m_Actor.Inventory.GetBestDestackable(model);
        if (null != tmp) return _BehaviorDropOrExchange(tmp, it, position);

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
      if (null != tmpTracker) return _BehaviorDropOrExchange(tmpTracker, it, position);

      // these lose to everything other than trackers.  Note that we should drop a light to get a more charged light -- if we're right on top of it.
      if (it is ItemSprayScent) return null;
      ItemSprayScent tmpSprayScent = inv.GetFirstMatching<ItemSprayScent>();
      if (null != tmpSprayScent) return _BehaviorDropOrExchange(tmpSprayScent, it, position);

      if (it is ItemBarricadeMaterial) return null;
      ItemBarricadeMaterial tmpBarricade = inv.GetFirstMatching<ItemBarricadeMaterial>();
      if (null != tmpBarricade) return _BehaviorDropOrExchange(tmpBarricade, it, position);

      if (it is ItemEntertainment) return null;
      ItemEntertainment tmpEntertainment = inv.GetFirstMatching<ItemEntertainment>();
      if (null != tmpEntertainment) return _BehaviorDropOrExchange(tmpEntertainment, it, position);

      if (it is ItemTrap) return null;
      ItemTrap tmpTrap = inv.GetFirstMatching<ItemTrap>();
      if (null != tmpTrap) return _BehaviorDropOrExchange(tmpTrap, it, position);

      if (it is ItemLight) {
        if (1 >= it_rating) return null;
        Item worst = GetWorst(m_Actor.Inventory.Items.Where(obj => 1 >= ItemRatingCode(obj)));
        if (null == worst) return null;
        return _BehaviorDropOrExchange(worst, it, position);
      }

      if (it is ItemMedicine) return null;

      // ditch unimportant items
      ItemMedicine tmpMedicine = inv.GetFirstMatching<ItemMedicine>();
      if (null != tmpMedicine) return _BehaviorDropOrExchange(tmpMedicine, it, position);

      // least charged flashlight goes
      List<ItemLight> lights = inv.GetItemsByType<ItemLight>();
      if (null != lights && 2<=lights.Count) {
        int min_batteries = lights.Select(obj => obj.Batteries).Min();
        ItemLight discard = lights.Find(obj => obj.Batteries==min_batteries);
        return BehaviorDropItem(discard);
      }

      // uninteresting ammo
      ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => null == m_Actor.Inventory.GetCompatibleRangedWeapon(ammo));  // not quite the full check here.  Problematic if no ranged weapons at all.
//    ItemAmmo tmpAmmo = inv.GetFirstMatching<ItemAmmo>(ammo => !IsInterestingItem(ammo));  // full check, triggers infinite recursion
      if (null != tmpAmmo) return _BehaviorDropOrExchange(tmpAmmo, it, position);

      // ranged weapon with zero ammo is ok to drop for something other than its own ammo
      ItemRangedWeapon tmpRw2 = inv.GetFirstMatching<ItemRangedWeapon>(rw => 0 >= rw.Ammo);
      if (null != tmpRw2) {
         bool reloadable = (it is ItemAmmo ? (it as ItemAmmo).AmmoType==tmpRw2.AmmoType : false);
         if (!reloadable) return _BehaviorDropOrExchange(tmpRw2, it, position);
      }

      // if we have 2 clips of an ammo type, trading one for a melee weapon or food is ok
      if (it is ItemMeleeWeapon || it is ItemFood) {
        foreach(GameItems.IDs x in GameItems.ammo) {
          ItemModel model = Models.Items[(int)x];
          if (2<=m_Actor.Count(model)) {
            ItemAmmo ammo = inv.GetBestDestackable(model) as ItemAmmo;
            return _BehaviorDropOrExchange(ammo, it, position);
          }
        }
        // if we have two clips of any type, trading the smaller one for a melee weapon or food is ok
        ItemAmmo test = null;
        foreach(GameItems.IDs x in GameItems.ammo) {
          if (inv.GetBestDestackable(Models.Items[(int)x]) is ItemAmmo ammo) {
             if (null == test || test.Quantity>ammo.Quantity) test = ammo;
          }
        }
        return _BehaviorDropOrExchange(test, it, position);
      }

      // if inventory is full and the problem is ammo at this point, ignore if we already have a full clip
      if (it is ItemAmmo && 1<=m_Actor.Count(it.Model)) return null;
      if (it is ItemAmmo && AmmoAtLimit) return null;

      // if inventory is full and the problem is ranged weapon at this point, ignore if we already have one
      if (it is ItemRangedWeapon && 1<= inv.CountType<ItemRangedWeapon>()) return null;

      // grenades next
      if (it is ItemGrenade) return null;
      ItemGrenade tmpGrenade = inv.GetFirstMatching<ItemGrenade>();
      if (null != tmpGrenade) return _BehaviorDropOrExchange(tmpGrenade, it, position);

      // important trackers go for ammo
      if (it is ItemAmmo) {
        ItemTracker discardTracker = inv.GetFirstMatching<ItemTracker>();
        if (null != discardTracker) return _BehaviorDropOrExchange(discardTracker, it, position);
      }

#if DEBUG
      // do not pick up trackers if it means dropping body armor or higher priority
      if (it is ItemTracker) return null;

      // body armor
      if (it is ItemBodyArmor) return null;

      throw new InvalidOperationException("coverage hole of types in BehaviorMakeRoomFor");
#else
      // give up
      return null;
#endif
    }

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
      if (m_Actor.Inventory.Contains(rw)) {
        if (0 < rw.Ammo) return true;
        // should not have ammo in inventory at this point
      }
      int rws_w_ammo = m_Actor.Inventory.CountType<ItemRangedWeapon>(it => 0 < it.Ammo);
      if (!m_Actor.Inventory.Contains(rw)) {
        if (0< rws_w_ammo) {
          if (null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, it => 0 < it.Ammo)) return false;    // XXX
          if (null != m_Actor.Inventory.GetFirst<ItemRangedWeapon>(it => it.AmmoType==rw.AmmoType && 0 < it.Ammo)) return false; // XXX ... more detailed handling in order; blocks upgrading from sniper rifle to army rifle, etc.
        } else {
          if (null != m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return true;   
        }
        if (0 < rw.Ammo && null != m_Actor.Inventory.GetFirstByModel<ItemRangedWeapon>(rw.Model, it => 0 == it.Ammo)) return true;  // this replacement is ok; implies not having ammo
      }
      // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
      // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
      if (AmmoAtLimit && null== m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return false;
      if (0 >= rw.Ammo && null == m_Actor.Inventory.GetCompatibleAmmoItem(rw)) return false;
      return _InterestingItemPostprocess(rw);
    }

    private bool AmmoAtLimit {
      get {
        // ideal non-ranged slots: armor, flashlight, melee weapon, 1 other
        // of the ranged slots, must reserve one for a ranged weapon and one for ammo; the others are "wild, biased for ammo"
        int limit = m_Actor.Inventory.MaxCapacity;
        if (0< m_Actor.Inventory.CountType<ItemBodyArmor>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemLight>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemFood>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemExplosive>()) limit--;
        if (0< m_Actor.Inventory.CountType<ItemMeleeWeapon>()) limit--;

        if (limit <= m_Actor.Inventory.CountType<ItemAmmo>()) return true;
        if (limit <= m_Actor.Inventory.CountType<ItemRangedWeapon>(it => 0 < it.Ammo)+ m_Actor.Inventory.CountType<ItemAmmo>()) return true;
        return false;
      }
    }

    public bool IsInterestingItem(ItemAmmo am)
    {
      ItemRangedWeapon rw = m_Actor.Inventory.GetCompatibleRangedWeapon(am);
      if (null == rw) {
        if (0 < m_Actor.Inventory.Count(am.Model)) return false;    // only need one clip to prime AI to look for empty ranged weapons
        if (KnowRelevantInventory(am) && !AmmoAtLimit) return true;
        if (0 < m_Actor.Inventory.CountType<ItemRangedWeapon>()) return false;  // XXX
      } else {
        if (rw.Model.MaxAmmo>rw.Ammo) return true;
        if (m_Actor.HasAtLeastFullStackOf(am, 2)) return false;
        if (null != m_Actor.Inventory.GetFirstByModel<ItemAmmo>(am.Model, it => it.Quantity < it.Model.MaxQuantity)) return true;   // topping off clip is ok
      }
      return _InterestingItemPostprocess(am);
    }

    public bool IsInterestingItem(ItemFood food)
    {
      return !m_Actor.HasEnoughFoodFor(m_Actor.Sheet.BaseFoodPoints / 2, food);
    }

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
        return IsInterestingItem(am);
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
      if (it is ItemBodyArmor) {
        ItemBodyArmor armor = m_Actor.GetBestBodyArmor();
        if (null == armor) return true;
        return armor.Rating < (it as ItemBodyArmor).Rating; // dropping inferior armor specifically handled in BehaviorMakeRoomFor so don't have to postprocess here
      }

      // No specific heuristic.
      if (it is ItemTracker) {
        if (1<=m_Actor.Inventory.Count(it.Model)) return false;
      } else if (it is ItemLight) {
        if (m_Actor.HasAtLeastFullStackOfItemTypeOrModel(it, 1)) return false;
      } else if (it is ItemMedicine) {
        // XXX easy to action-loop if inventory full
        if (m_Actor.HasAtLeastFullStackOf(it, m_Actor.Inventory.IsFull ? 1 : 2)) return false;
      } else if (it is ItemTrap trap) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
        if (trap.IsActivated) return false;
#if DEBUG
      } else if (it is ItemEntertainment) {
        if (m_Actor.HasAtLeastFullStackOf(it, 1)) return false;
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
      if (!ret && 1<item_rating) {
        // check inventory for less-interesting item.  Force high visibility in debugger.
        foreach(Item obj in m_Actor.Inventory.Items) {
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

    public virtual bool IsInterestingTradeItem(Actor speaker, Item offeredItem) // Cf. OrderableAI::IsRationalTradeItem
    {
#if DEBUG
      if (null == speaker) throw new ArgumentNullException(nameof(speaker));
      if (!speaker.Model.Abilities.CanTrade) throw new InvalidOperationException(nameof(speaker)+" must be able to trade");
      if (!m_Actor.Model.Abilities.CanTrade) throw new InvalidOperationException(nameof(m_Actor)+" must be able to trade");
#endif
      if (RogueForm.Game.Rules.RollChance(Rules.ActorCharismaticTradeChance(speaker))) return true;
      return IsInterestingItem(offeredItem);
    }

    private static void _InterpretRangedWeapons(IEnumerable<ItemRangedWeapon> rws, Point pt, Dictionary<Point, ItemRangedWeapon[]> best_rw, Dictionary<Point, ItemRangedWeapon[]> reload_empty_rw, Dictionary<Point, ItemRangedWeapon[]> discard_empty_rw, Dictionary<Point, ItemRangedWeapon[]> reload_rw)
    {
        if (!rws?.Any() ?? true) return;

        best_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        reload_empty_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        discard_empty_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        reload_rw[pt] = new ItemRangedWeapon[(int)AmmoType._COUNT];
        bool keep_empty = false;
        bool keep_reload = false;

        foreach(var rw in rws) {
          // note that "better" ranged weapons taking the same ammo have larger clips
          if (0==rw.Ammo) {
            if (null == reload_empty_rw[pt][(int)rw.AmmoType]) reload_empty_rw[pt][(int)rw.AmmoType] = rw;
            else if (reload_empty_rw[pt][(int)rw.AmmoType].Model.MaxAmmo < rw.Model.MaxAmmo) reload_empty_rw[pt][(int)rw.AmmoType] = rw;
            if (null == discard_empty_rw[pt][(int)rw.AmmoType]) discard_empty_rw[pt][(int)rw.AmmoType] = rw;
            else if (discard_empty_rw[pt][(int)rw.AmmoType].Model.MaxAmmo > rw.Model.MaxAmmo) discard_empty_rw[pt][(int)rw.AmmoType] = rw;
            keep_empty = true;
          }
          if (rw.Model.MaxAmmo > rw.Ammo) {
            if (null == reload_rw[pt][(int)rw.AmmoType]) reload_rw[pt][(int)rw.AmmoType] = rw;
            else if (reload_rw[pt][(int)rw.AmmoType].Model.MaxAmmo < rw.Model.MaxAmmo) reload_rw[pt][(int)rw.AmmoType] = rw;
            else if ((reload_rw[pt][(int)rw.AmmoType].Model.MaxAmmo - reload_rw[pt][(int)rw.AmmoType].Ammo) < (rw.Model.MaxAmmo-rw.Ammo)) reload_rw[pt][(int)rw.AmmoType] = rw;
            keep_reload = true;
          }
          if (null == best_rw[pt][(int)rw.AmmoType]) {
            best_rw[pt][(int)rw.AmmoType] = rw;
            continue;
          }
          if (best_rw[pt][(int)rw.AmmoType].Ammo < rw.Ammo) {
            best_rw[pt][(int)rw.AmmoType] = rw;
            continue;
          }
          if (best_rw[pt][(int)rw.AmmoType].Model.MaxAmmo < rw.Model.MaxAmmo) {
            best_rw[pt][(int)rw.AmmoType] = rw;
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
    protected ActorAction InventoryStackTactics(Location loc)
    {
      if (m_Actor.Inventory.IsEmpty) return null;

      // The index case.
      var rws = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>();
      if (0 < (rws?.Count ?? 0)) {
        foreach(var rw in rws) {
          if (rw.Ammo < rw.Model.MaxAmmo) {
            // usually want to reload this even if we had to drop ammo as a recovery option
            int i = Objectives.Count;
            while(0<i) {
              if (Objectives[--i] is Goal_DoNotPickup dnp) {
                if (dnp.Avoid != (GameItems.IDs)((int)(rw.AmmoType)+(int)(GameItems.IDs.AMMO_LIGHT_PISTOL))) continue;
                Objectives.RemoveAt(i);
              }
            }
          }
        }
      }

      Dictionary<Point,Inventory> ground_inv = loc.Map.GetAccessibleInventories(loc.Position);
      if (0 >= ground_inv.Count) return null;

      // set up pattern-matching for ranged weapons
      Point viewpoint_inventory = new Point(int.MaxValue,int.MaxValue); // intentionally chosen to be impossible, as a flag
      var best_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var reload_empty_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var discard_empty_rw = new Dictionary<Point, ItemRangedWeapon[]>();
      var reload_rw = new Dictionary<Point, ItemRangedWeapon[]>();

      _InterpretRangedWeapons(rws, viewpoint_inventory, best_rw, reload_empty_rw, discard_empty_rw, reload_rw);

      if (reload_rw.ContainsKey(viewpoint_inventory)) {
        // prepare to analyze ranged weapon swaps.
        foreach(var x in ground_inv) {
          var ground_rws = x.Value.GetItemsByType<ItemRangedWeapon>();
          _InterpretRangedWeapons(ground_rws, x.Key, best_rw, reload_empty_rw, discard_empty_rw, reload_rw);
        }

        if (discard_empty_rw.ContainsKey(viewpoint_inventory)) {
          // we should not have been able to reload this i.e. no ammo.
          Point? dest = null;
          ItemRangedWeapon test = null;
          ItemRangedWeapon src = null;
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == discard_empty_rw[viewpoint_inventory][i]) continue;
            foreach(var where_inv in best_rw) {
              if (where_inv.Key == viewpoint_inventory) continue;
              if (null == where_inv.Value[i]) continue;
              if (0 >= where_inv.Value[i].Ammo) continue;
              if (null == test) {
                dest = where_inv.Key;
                src = discard_empty_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
              if (test.Ammo < where_inv.Value[i].Ammo && test.Model.MaxAmmo <= where_inv.Value[i].Model.MaxAmmo) {
                dest = where_inv.Key;
                src = discard_empty_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
            }
          }
          if (null != test) return new ActionTradeWithContainer(m_Actor,src,test,dest.Value);
        }

        // optimization
        {
          Point? dest = null;
          ItemRangedWeapon test = null;
          ItemRangedWeapon src = null;
          int i = (int)AmmoType._COUNT;
          while(0 <= --i) {
            if (null == reload_rw[viewpoint_inventory][i]) continue;
            foreach(var where_inv in best_rw) {
              if (where_inv.Key == viewpoint_inventory) continue;
              if (null == where_inv.Value[i]) continue;
              if (reload_rw[viewpoint_inventory][i].Ammo >= where_inv.Value[i].Ammo) continue;
              if (reload_rw[viewpoint_inventory][i].Model.MaxAmmo > where_inv.Value[i].Model.MaxAmmo) continue;
              if (null == test) {
                dest = where_inv.Key;
                src = reload_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
              if (test.Ammo < where_inv.Value[i].Ammo && test.Model.MaxAmmo <= where_inv.Value[i].Model.MaxAmmo) {
                dest = where_inv.Key;
                src = reload_rw[viewpoint_inventory][i];
                test = where_inv.Value[i];
                continue;
              }
            }
          }
          if (null != test) return new ActionTradeWithContainer(m_Actor,src,test,dest.Value);
        }
      }

      return null;
    }

    protected ActorAction InventoryStackTactics() { return InventoryStackTactics(m_Actor.Location); }

    /// <remark>Intentionally asymmetric.  Call this twice to get proper coverage.
    /// Will ultimately end up in ObjectiveAI when AI state needed.</remark>
    static public bool TradeVeto(Item mine, Item theirs)
    {
      // reject identity trades for now.  This will change once AI state is involved.
      if (mine.Model == theirs.Model) return true;

      switch(mine.Model.ID)
      {
      // two weapons for the ammo
      case GameItems.IDs.RANGED_PRECISION_RIFLE:
      case GameItems.IDs.RANGED_ARMY_RIFLE:
        if (GameItems.IDs.AMMO_HEAVY_RIFLE==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_PISTOL:
      case GameItems.IDs.RANGED_KOLT_REVOLVER:
        if (GameItems.IDs.AMMO_LIGHT_PISTOL==theirs.Model.ID) return true;
        break;
      // one weapon for the ammo
      case GameItems.IDs.RANGED_ARMY_PISTOL:
        if (GameItems.IDs.AMMO_HEAVY_PISTOL==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_HUNTING_CROSSBOW:
        if (GameItems.IDs.AMMO_BOLTS==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_HUNTING_RIFLE:
        if (GameItems.IDs.AMMO_LIGHT_RIFLE==theirs.Model.ID) return true;
        break;
      case GameItems.IDs.RANGED_SHOTGUN:
        if (GameItems.IDs.AMMO_SHOTGUN==theirs.Model.ID) return true;
        break;
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

    protected static int ScoreRangedWeapon(ItemRangedWeapon w)
    {
      Attack rw_attack = w.Model.Attack;
      return 1000 * rw_attack.Range + rw_attack.DamageValue;
    }

    // conceptual difference between "doctrine" and "behavior" is that doctrine doesn't have contextual validity checks
    // that is, a null action return is defined to mean the doctrine is invalid
    public ActorAction DoctrineRecoverSTA(int targetSTA)
    {
       if (m_Actor.MaxSTA < targetSTA) targetSTA = m_Actor.MaxSTA;
       if (m_Actor.StaminaPoints >= targetSTA) return null;
       if (   m_Actor.StaminaPoints < targetSTA - 4
           && m_Actor.CanActNextTurn) {
         Item stim = m_Actor?.Inventory.GetBestDestackable(Models.Items[(int)GameItems.IDs.MEDICINE_PILLS_STA]);
         if (null != stim) return new ActionUseItem(m_Actor,stim);
       }
       return new ActionWait(m_Actor);
    }

    public ActorAction DoctrineMedicateSLP()
    {
       ItemMedicine stim = (m_Actor?.Inventory.GetBestDestackable(Models.Items[(int)Gameplay.GameItems.IDs.MEDICINE_PILLS_SLP]) as ItemMedicine);
       if (null == stim) return null;
       int threshold = m_Actor.MaxSleep-(Rules.ActorMedicineEffect(m_Actor, stim.SleepBoost));
       if (m_Actor.SleepPoints > threshold) return null;
       if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
       return new ActionUseItem(m_Actor,stim);
    }

    public ActorAction DoctrineRechargeToFull(Item it)
    {
      BatteryPowered obj = it as BatteryPowered;
#if DEBUG
      if (null == obj) throw new ArgumentNullException(nameof(obj));
#endif
      if (obj.MaxBatteries-1 <= obj.Batteries) return null;
      var generators = m_Actor.Location.Map.PowerGenerators.Get.Where(power => Rules.IsAdjacent(m_Actor.Location,power.Location)).ToList();
      if (0 >= generators.Count) return null;
      var generators_on = generators.Where(power => power.IsOn).ToList();
      if (0 >= generators_on.Count) return new ActionSwitchPowerGenerator(m_Actor,generators[0]);
      if (!it.IsEquipped) RogueForm.Game.DoEquipItem(m_Actor,it);
      if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
      return new ActionRechargeItemBattery(m_Actor,it);
    }

    public ActorAction DoctrineButcher(Corpse c)
    {
      if (!m_Actor.CanButcher(c)) return null;
      
      {
      var best = m_Actor.GetBestMeleeWeapon();
      if (null!=best && !best.IsEquipped) RogueForm.Game.DoEquipItem(m_Actor,best);
      }
      if (!m_Actor.CanActNextTurn) return new ActionWait(m_Actor);
      return new ActionButcher(m_Actor,c);
    }

    // XXX should also have concept of hoardable item (suitable for transporting to a safehouse)
    public ItemRangedWeapon GetBestRangedWeaponWithAmmo()
    {
      if (m_Actor.Inventory.IsEmpty) return null;
      var rws = m_Actor.Inventory.GetItemsByType<ItemRangedWeapon>(rw => {
        if (0 < rw.Ammo) return true;
        var ammo = m_Actor.Inventory.GetItemsByType < ItemAmmo >(am => am.AmmoType==rw.AmmoType);
        return null != ammo;
      });
      if (null == rws) return null;
      if (1==rws.Count) return rws[0];
      ItemRangedWeapon obj1 = null;
      int num1 = 0;
      foreach (ItemRangedWeapon w in rws) {
        int num2 = ScoreRangedWeapon(w);
        if (num2 > num1) {
          obj1 = w;
          num1 = num2;
        }
      }
      return obj1;
    }

    public void DeBarricade(Engine.MapObjects.DoorWindow doorWindow)
    {
#if DEBUG
      if (null == doorWindow) throw new ArgumentNullException(nameof(doorWindow));
#endif
      if (null == Goal<Goal_BreakBarricade>(o => o.Target == doorWindow)) Objectives.Insert(0, new Goal_BreakBarricade(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, doorWindow));
      if (0 < m_Actor.CountFollowers) {
        foreach(Actor fo in m_Actor.Followers) {
          if (!InCommunicationWith(fo)) continue;
          if (fo.IsPlayer) continue;
          OrderableAI ai = fo.Controller as OrderableAI;
          // XXX \todo message this so it's clear what's going on
          if (null == ai.Goal<Goal_BreakBarricade>(o => o.Target == doorWindow)) ai.Objectives.Insert(0, new Goal_BreakBarricade(fo.Location.Map.LocalTime.TurnCounter, fo, doorWindow));
        }
      }
    }
  }
}
