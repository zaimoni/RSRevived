using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;

using BackwardPlan = Zaimoni.Data.BackwardPlan<djack.RogueSurvivor.Data.ActorAction>;
using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;

#nullable enable

// Case study in Many-Worlds interpretation of Quantum Mechanics.

// namespace Action conflicts with C# STL Action<>
namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class Fork : ActorAction
    {
      private readonly Dictionary<int, List<ActorAction>> m_Options = new Dictionary<int, List<ActorAction>>();

      // imports pre-existing legal path calculation as the "starting point"
      public Fork(Actor actor, Dictionary<Location, ActorAction> legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
        void record(KeyValuePair<Location, ActorAction> x) {
          if (x.Value is Actions.ActorDest) Add(x.Value is Actions.ActorOrigin ? x.Value : new Actions.ActionMoveDelta(m_Actor, x.Key));
          else if (x.Value is Actions.Resolvable act) record(new KeyValuePair<Location, ActorAction>(x.Key, act.ConcreteAction));
          else Add(x.Value);
        }

        foreach(var x in legal_path) record(x);
      }

      public Fork(Actor actor, List<ActorAction> legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
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
        var doomed = new Zaimoni.Data.Stack<int>(stackalloc int[m_Options.Count]);
        bool ok = false;
        string? last_fail = null;
        int ub;
        foreach(var x in m_Options) {
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
        while(0 <= --ub) m_Options.Remove(doomed[ub]);
        if (!ok) m_FailReason = last_fail;
        return ok;
      }

      // predicted to be CPU-expensive, so just assume components correctly implement performable => legal
      public override bool IsPerformable()
      {
        var doomed = new Zaimoni.Data.Stack<int>(stackalloc int[m_Options.Count]);
        bool ok = false;
        string? last_fail = null;
        int ub;
        foreach(var x in m_Options) {
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
        while(0 <= --ub) m_Options.Remove(doomed[ub]);
        if (!ok) m_FailReason = last_fail;
        return ok;
      }

      public override void Perform()
      {
        int act_cost = m_Options.Keys.Min();
        (m_Actor.Controller as ObjectiveAI).ExecuteActionFork(m_Options[act_cost]);
      }

      public override bool Abort()  // \todo want to block double-processing to conserve CPU
      {
        foreach(var x in m_Options) foreach(var act in x.Value) if (act.Abort()) return true;
        return false;
      }

      // assume the candidate indexing is mostly correct
      public int CumulativeMoveCost()
      {
#if TOO_FAST
        return m_Candidates.Keys.Min();
#else
retry:
        int act_cost = m_Options.Keys.Min();
        var cache = m_Options[act_cost];
        int ub = cache.Count;
        bool recalc = false;
        while(0 <= --ub) {
          var act = cache[ub];
          int test = (act is Actions.ActionChain chain) ? chain.CumulativeMoveCost() : Map.PathfinderMoveCosts(act);
          if (test != act_cost) {
            cache.RemoveAt(ub);
            Add(act);
            if (test < act_cost) recalc = true;
          }
        }
        if (0 >= cache.Count) {
          m_Options.Remove(act_cost);
          if (!recalc) goto retry;
        }
        if (recalc) return m_Options.Keys.Min();
        return act_cost;
#endif
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
        if (m_Options.TryGetValue(cost, out var cache)) cache.Add(src);
        else m_Options.Add(cost, new List<ActorAction> { src });
      }

      public ActorAction? Reduce()
      {
        if (1 != m_Options.Count) return null;
        var test = m_Options.First();
        return 1==test.Value.Count ? test.Value[0] : null;
      }

      public bool ContainsSuffix(List<ActorAction> src, int index)
      {
        if (index < src.Count) {
          var test = src[index];
          foreach(var x in m_Options) {
            foreach(var act in x.Value) {
              if (act is Actions.ActionChain chain) return chain.ContainsSuffix(src, index);
              else if (test.AreEquivalent(act)) return index+1==src.Count;
            }
          }
          return false;
        }
        return true;
      }

      public void splice(Actions.ActionChain wrapped)
      {
        var test = FindFirst(wrapped.ConcreteAction);
        if (int.MaxValue == test.Key) {
          Add(wrapped);
          return;
        }

        var prefix_cache = m_Options[test.Key];
        var prefix_match = prefix_cache[test.Value];
        prefix_cache.RemoveAt(test.Value);
        if (0 >= prefix_cache.Count) m_Options.Remove(test.Key);
        if (!(prefix_match is Actions.ActionChain chain)) {
          Add(wrapped);
          return;
        }
        var replace = chain.splice(wrapped);
        if (null != replace) Add(replace);
      }

      private KeyValuePair<int,int> FindFirst(ActorAction src) {
        var ret = new KeyValuePair<int,int>(int.MaxValue, int.MaxValue);
        foreach(var x in m_Options) {
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

      private static List<ActorAction>? CanBackwardPlan(ActorAction src)
      {
        var act = src as BackwardPlan;
        if (null == act && src is Actions.ActionChain chain) act = chain.ConcreteAction as BackwardPlan;
        if (null != act) return act.prequel();
        return null;
      }

      bool backward_chain()
      {
        int act_cost = m_Options.Keys.Min();
        var cache = m_Options[act_cost];
        var working = new List<Actions.ActionChain>();
        int ub = cache.Count;
        while(0 <= --ub) {
          var dest = cache[ub];
          var setup = CanBackwardPlan(dest);
          if (null == setup) throw new ArgumentNullException(nameof(setup));    // invariant failure
          foreach (var x in setup) {
            var test = new Actions.ActionChain(x, dest);
            if (!test.IsSemanticParadox()) working.Add(test);
          }
          cache.RemoveAt(ub);
          if (0 >= working.Count) continue;
          foreach(var chain in working) splice(chain);
          return true;
        }
        return false;
      }

      public ActorAction? RejectOrigin(Location origin)
      {
        List<ActorAction>? args = null;
        bool all_there = true;
        foreach(var x in m_Options) {
          foreach (var act in x.Value) {
            if (act is Actions.ActorDest a_dest) {
              if (a_dest.dest == origin) {
                all_there = false;
                continue;
              }
            } else if (act is Actions.ActionChain chain && chain.RejectOrigin(origin, 0)) { 
              all_there = false;
              continue;
            }
            (args ?? (args = new List<ActorAction>())).Add(act);
          }
        }
        if (all_there) return this;
        if (null == args) return null;
        if (1 == args.Count) return args[0];
        return new Fork(m_Actor, args);
      }
    }
}

namespace djack.RogueSurvivor.Engine.Op
{
    [Serializable]
    class Fork : WorldUpdate
    {
      private readonly List<WorldUpdate> m_Options = new List<WorldUpdate>();

      public Fork(List<WorldUpdate> legal_path)
      {
        foreach (var act in legal_path) Add(act);
      }

      public Fork(WorldUpdate x1, WorldUpdate x2)
      {
        Add(x1);
        Add(x2);
      }

      public override bool IsLegal()
      {
        var ub = m_Options.Count;
        while(0 <= --ub) {
          if (m_Options[ub].IsLegal()) return true;
          m_Options.RemoveAt(ub);
        }
        return false;
      }

      public override bool IsRelevant() {
        foreach(var x in m_Options) if (x.IsRelevant()) return true;
        return false;
      }

      public override bool IsRelevant(Location loc) {
        foreach(var x in m_Options) if (x.IsRelevant(loc)) return true;
        return false;
      }

      public override ActorAction? Bind(Actor src) {
#if DEBUG
        if (!(src.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
        var actions = new List<ActorAction>();
        foreach(var x in m_Options) {
          if (x.IsRelevant(src.Location)) {
            var act = x.Bind(src);
            if (null != act) actions.Add(act);
          }
        }
        var act_count = actions.Count;
        if (1 >= act_count) return (0 >= act_count) ? null : actions[0];
        return new _Action.Fork(src, actions);
      }

      public override void Blacklist(HashSet<Location> goals)
      {
        var ub = m_Options.Count;
        while(0 <= --ub) m_Options[ub].Blacklist(goals);
      }

      public override void Goals(HashSet<Location> goals)
      {
        var ub = m_Options.Count;
        while(0 <= --ub) m_Options[ub].Goals(goals);
      }

      public void Add(WorldUpdate src)
      {
        if (src is Fork fork) {
          foreach(var act in fork.m_Options) Add(act);
        } else if (src.IsLegal() && !m_Options.Contains(src)) {
          if (src is Join join) {
            var ub = m_Options.Count;
            while(0 <= --ub) {
              if (m_Options[ub] is Join prior_join && prior_join.ForkMerge(join)) return;
            }
          }
          m_Options.Add(src);
        }
      }

      public bool ForceRelevant(Location loc, ref WorldUpdate dest) {
        var staging = new List<WorldUpdate>();
        foreach(var act in m_Options) if (act.IsRelevant(loc)) staging.Add(act);
        var staged = staging.Count;
        if (1 > staged) throw new InvalidOperationException("tried to force-relevant a not-relevant objective");
        if (2 <= staged) {
          if (m_Options.Count > staged) dest = new Fork(staging);
          return false;
        }
        dest = staging[0];
        return true;
      }
    }
}
