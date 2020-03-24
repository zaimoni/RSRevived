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

      public Fork(Actor actor, List<ActorAction>? legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (null == legal_path) throw new ArgumentNullException(nameof(legal_path));
#endif
        foreach (var act in legal_path) Add(act);
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

      public ActorAction? Reduce()
      {
        if (1 != m_Candidates.Count) return null;
        var test = m_Candidates.First();
        return 1==test.Value.Count ? test.Value[0] : null;
      }

      public bool ContainsSuffix(List<ActorAction> src, int index)
      {
        if (index < src.Count) {
          var test = src[index];
          foreach(var x in m_Candidates) {
            foreach(var act in x.Value) {
              if (act is Actions.ActionChain chain) return chain.ContainsSuffix(src, index);
              else if (test.AreEquivalent(act)) return index+1==src.Count;
            }
          }
          return false;
        }
        return true;
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

      public void splice(Actions.ActionChain wrapped)
      {
        var test = FindFirst(wrapped.ConcreteAction);
        if (int.MaxValue == test.Key) {
          Add(wrapped);
          return;
        }

        var prefix_cache = m_Candidates[test.Key];
        var prefix_match = prefix_cache[test.Value];
        prefix_cache.RemoveAt(test.Value);
        if (0 >= prefix_cache.Count) m_Candidates.Remove(test.Key);
        if (!(prefix_match is Actions.ActionChain chain)) {
          Add(wrapped);
          return;
        }
        var replace = chain.splice(wrapped);
        if (null != replace) Add(replace);
      }

      private KeyValuePair<int,int> FindFirst(ActorAction src) {
        var ret = new KeyValuePair<int,int>(int.MaxValue, int.MaxValue);
        foreach(var x in m_Candidates) {
          int ub = x.Value.Count;
          while(0 <= --ub) {
            var test_act = x.Value[ub];
            if (test_act is Actions.ActionChain chain) {
              if (src.AreEquivalent(chain.ConcreteAction)) return new KeyValuePair<int,int>(x.Key, ub);
              continue;
            }
            if (src.AreEquivalent(test_act)) return new KeyValuePair<int,int>(x.Key, ub);
          }
        }
        return ret;
      }

      private KeyValuePair<int,int> FindFirst(Actions.ActionChain src) {
        var ret = new KeyValuePair<int,int>(int.MaxValue, int.MaxValue);
        foreach(var x in m_Candidates) {
          int ub = x.Value.Count;
          while(0 <= --ub) {
            var test_act = x.Value[ub];
            if (test_act is Actions.ActionChain chain) {
              if (src.ConcreteAction.AreEquivalent(chain.ConcreteAction)) return new KeyValuePair<int,int>(x.Key, ub);
              continue;
            }
            if (src.ConcreteAction.AreEquivalent(test_act)) return new KeyValuePair<int,int>(x.Key, ub);
          }
        }
        return ret;
      }

      bool backward_chain()
      {
        int act_cost = m_Candidates.Keys.Min();
        var cache = m_Candidates[act_cost];
        var working = new List<Actions.ActionChain>();
        var copyin = new List<Actions.ActionChain>();
        int ub = cache.Count;
        while(0 <= --ub) {
          if (!(cache[ub] is BackwardPlan act)) continue;
          var setup = act.prequel();
          if (null == setup) continue;
          foreach (var x in setup) {
            var test = new Actions.ActionChain(x, cache[ub]);
            if (!test.IsSemanticParadox()) working.Add(test);
          }
          if (0 >= working.Count) {
            cache.RemoveAt(ub);
            continue;
          }
          int in_scan = working.Count;
          while(0 <= --in_scan) {   // trivial vs fork processing
            var test_act = working[in_scan];
            var test = FindFirst(test_act);
            if (int.MaxValue == test.Key) {
              copyin.Add(test_act);
              working.RemoveAt(in_scan);
              continue;
            }
            var prefix_cache = m_Candidates[test.Key];
            var prefix_match = prefix_cache[test.Value];
            if (!(prefix_match is Actions.ActionChain chain)) {
              copyin.Add(test_act);
              working.RemoveAt(in_scan);
              prefix_cache.RemoveAt(test.Value);
              if (0 >= prefix_cache.Count) m_Candidates.Remove(test.Key);
              continue;
            }
            if (test_act.AreEquivalent(prefix_match)) {
              working.RemoveAt(in_scan);
              continue;
            }
          }
          cache.RemoveAt(ub);
          if (0 >= cache.Count) m_Candidates.Remove(act_cost);
          in_scan = working.Count;
          while(0 <= --in_scan) {   // fork processing
            var test_act = working[in_scan];
            var test = FindFirst(test_act);
#if DEBUG
            if (int.MaxValue == test.Key) throw new InvalidProgramException("should not have disappeared");
#endif
            var prefix_cache = m_Candidates[test.Key];
            var prefix_match = prefix_cache[test.Value];
            var chain = prefix_match as Actions.ActionChain;
#if DEBUG
            if (null == chain) throw new InvalidProgramException("no longer spliceable");
#endif
            var new_chain = test_act.splice(chain);
            if (null != new_chain) {
              prefix_cache.RemoveAt(test.Value);
              if (0 >= prefix_cache.Count) m_Candidates.Remove(test.Key);
              Add(new_chain);
            };
          }
          foreach(var chain in copyin) Add(chain);
          return true;
        }
        return false;
      }
    }
}
