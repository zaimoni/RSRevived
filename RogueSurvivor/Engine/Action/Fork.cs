using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;

using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;

#nullable enable

// namespace Action conflicts with C# STL Action<>
namespace djack.RogueSurvivor.Engine._Action
{
    internal interface BackwardPlan
    {
      List<ActorAction>? prequel();
    }

    // Case study in Many-Worlds interpretation of Quantum Mechanics.
    [Serializable]
    class Fork : ActorAction
    {
      private readonly Dictionary<int, List<ActorAction>> m_Candidates = new Dictionary<int, List<ActorAction>>();

      // imports pre-existing legal path calculation as the "starting point"
      public Fork(Actor actor, Dictionary<Location, ActorAction>? legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (null == legal_path) throw new ArgumentNullException(nameof(legal_path));
#endif
        foreach(var x in legal_path) {
          if (x.Value is Actions.ActorDest) Add(new Actions.ActionMoveDelta(m_Actor, x.Key));
          else if (x.Value is Actions.Resolvable act) Add(act.ConcreteAction);
          else Add(x.Value);
        }
      }

      private Fork(Actor actor, List<ActorAction>? legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (null == legal_path) throw new ArgumentNullException(nameof(legal_path));
#endif
      }

      public Fork(Actor actor, ActorAction root) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (!root.IsLegal()) throw new ArgumentOutOfRangeException(nameof(root), root, "must be legal");
        if (root is BackwardPlan) throw new ArgumentOutOfRangeException(nameof(root), root, "must be capable of backward planning");
        if (root is Fork) throw new InvalidOperationException("fork of fork is not reasonable");
#endif
        Add(root);
      }

      public override bool IsLegal()
      {
        var doomed = new Zaimoni.Data.Stack<int>(stackalloc int[m_Candidates.Count]);
        bool ok = false;
        string? last_fail = null;
        int ub;
        foreach(var x in m_Candidates) {
          ub = x.Value.Count;
          while(0 <= --ub) {
            if (x.Value[ub].IsLegal()) ok = true;
            else {
              var fail = x.Value[ub].FailReason;
              x.Value.RemoveAt(ub);
              if (!ok && !string.IsNullOrEmpty(fail)) last_fail = fail;
            }
          }
          if (0 >= x.Value.Count) doomed.push(x.Key);
        }
        ub = doomed.Count;
        while(0 <= --ub) m_Candidates.Remove(doomed[ub]);
        if (!ok) m_FailReason = last_fail;
        return ok;
      }

      // predicted to be CPU-expensive, so just assume components correctly implement performable => legal
      public override bool IsPerformable()
      {
        var doomed = new Zaimoni.Data.Stack<int>(stackalloc int[m_Candidates.Count]);
        bool ok = false;
        string? last_fail = null;
        int ub;
        foreach(var x in m_Candidates) {
          ub = x.Value.Count;
          while(0 <= --ub) {
            if (x.Value[ub].IsPerformable()) ok = true;
            else {
              var fail = x.Value[ub].FailReason;
              x.Value.RemoveAt(ub);
              if (!ok && !string.IsNullOrEmpty(fail)) last_fail = fail;
            }
          }
          if (0 >= x.Value.Count) doomed.push(x.Key);
        }
        ub = doomed.Count;
        while(0 <= --ub) m_Candidates.Remove(doomed[ub]);
        if (!ok) m_FailReason = last_fail;
        return ok;
      }

      public override void Perform()
      {
        int act_cost = m_Candidates.Keys.Min();
        (m_Actor.Controller as ObjectiveAI).ExecuteActionFork(m_Candidates[act_cost]);
      }

      private static void _add(Dictionary<int, List<Actions.ActionChain>> dest, Actions.ActionChain src)
      {
        int cost = (src is Actions.ActionChain chain) ? chain.CumulativeMoveCost() : Map.PathfinderMoveCosts(src);
        if (dest.TryGetValue(cost, out var cache)) cache.Add(src);
        else dest.Add(cost, new List<Actions.ActionChain> { src });
      }

      private void Add(ActorAction src)
      {
        int cost = (src is Actions.ActionChain chain) ? chain.CumulativeMoveCost() : Map.PathfinderMoveCosts(src);
        if (m_Candidates.TryGetValue(cost, out var cache)) cache.Add(src);
        else m_Candidates.Add(cost, new List<ActorAction> { src });
      }

#if PROTOTYPE
      public ActorAction? splice(Fork next)
      {
        List<Engine.Actions.ActionChain>? preview_ret = null;
        foreach(var start in m_Options) {
          if (start is Actions.ActorDest a_dest) {
            foreach (var end in next.m_Options) {
              if (end is Actions.ActorOrigin a_origin) {
                if (a_dest.dest == a_origin.origin) {
                  var test = new Actions.ActionChain(start, end);
                  if (!test.IsSemanticParadox()) (preview_ret ?? (preview_ret = new List<Engine.Actions.ActionChain>())).Add(test);
                }
              }
            }
          }
        }
        if (null == preview_ret) return null;
        if (1 == preview_ret.Count) return preview_ret[0];
        return new Fork(m_Actor, new List<ActorAction>(preview_ret));
      }
#endif

      bool backward_chain()
      {
        int act_cost = m_Candidates.Keys.Min();
        var cache = m_Candidates[act_cost];
        var working = new Dictionary<int, List<Actions.ActionChain>>();
        int ub = cache.Count;
        while(0 <= --ub) {
          if (!(cache[ub] is BackwardPlan act)) continue;
          var setup = act.prequel();
          if (null == setup) continue;
          foreach (var x in setup) {
            var test = new Actions.ActionChain(x, cache[ub]);
            if (!test.IsSemanticParadox()) _add(working, test);
          }
          if (0 >= working.Count) {
            cache.RemoveAt(ub);
            continue;
          }
          // assuming increasing cost; reject duplicates
          var doomed = new Zaimoni.Data.Stack<int>(stackalloc int[working.Count]);
          int in_scan;
          foreach(var x in working) {
            if (!m_Candidates.TryGetValue(x.Key, out var examining)) continue;
            bool duplicate = false;
            in_scan = x.Value.Count;
            while(0 <= --in_scan) {
              var test_act = x.Value[in_scan];
              int scan = examining.Count;
              while(0 <= --scan) {
                if (test_act.AreEquivalent(examining[scan])) {
                  duplicate = true;
                  break;
                }
              }
              if (duplicate) {
                x.Value.RemoveAt(in_scan);
                continue;
              }
//            if (int.MaxValue > forkable) { /* ... */ }
            }
            if (0 >= x.Value.Count) doomed.push(x.Key);
          }
          in_scan = doomed.Count;
          while(0 <= in_scan) working.Remove(doomed[in_scan]);
          if (0 >= working.Count) {
            cache.RemoveAt(ub);
            continue;
          }
        }
        if (0 >= cache.Count) m_Candidates.Remove(act_cost);
        return 0 < m_Candidates.Count;
      }
    }
}
