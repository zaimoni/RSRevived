using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using Zaimoni.Data;
using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;

#nullable enable

// Case study in Many-Worlds interpretation of Quantum Mechanics.

// namespace Action conflicts with C# STL Action<>
namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    class Fork : ActorAction, RecursivePathfinderMoveCost
    {
      private readonly Dictionary<int, List<ActorAction>> m_Options = new Dictionary<int, List<ActorAction>>();

      public Fork(Actor actor, List<ActorAction> legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
        foreach (var act in legal_path) Add(act);
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

      public int PathfinderMoveCost() { return m_Options.Keys.Min(); }

      // assume the candidate indexing is mostly correct
      public int CumulativeMoveCost()
      {
retry:
        int act_cost = m_Options.Keys.Min();
        var cache = m_Options[act_cost];
        int ub = cache.Count;
        bool recalc = false;
        while(0 <= --ub) {
          var act = cache[ub];
          int test = (act is Actions.ActionChain chain) ? chain.CumulativeMoveCost() : Map.PathfinderMoveCosts(act) + Map.TrapMoveCostFor(act, m_Actor);
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
      }

      private void Add(ActorAction src)
      {
        int cost = (src is Actions.ActionChain chain) ? chain.CumulativeMoveCost() : Map.PathfinderMoveCosts(src) + Map.TrapMoveCostFor(src, m_Actor);
        if (m_Options.TryGetValue(cost, out var cache)) cache.Add(src);
        else m_Options.Add(cost, new List<ActorAction> { src });
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
            (args ??= new List<ActorAction>()).Add(act);
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
    class Fork : WorldUpdate, CanReduce<WorldUpdate>
    {
      private readonly List<WorldUpdate> m_Options = new List<WorldUpdate>();

      public Fork(List<WorldUpdate> legal_path)
      {
        foreach (var act in legal_path) Add(act);
#if DEBUG
        if (!IsLegal()) throw new InvalidProgramException("created illegal Join");
#endif
      }

      public Fork(WorldUpdate x1, WorldUpdate x2)
      {
        Add(x1);
        Add(x2);
#if DEBUG
        if (!IsLegal()) throw new InvalidProgramException("created illegal Join");
#endif
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
        var ub = m_Options.Count;
        while(0 <= --ub) if (m_Options[ub].IsRelevant()) return true;
        return false;
      }

      public override bool IsRelevant(Location loc) {
        var ub = m_Options.Count;
        while(0 <= --ub) if (m_Options[ub].IsRelevant(loc)) return true;
        return false;
      }

      public override bool IsSuppressed(Actor a)
      {
        var ub = m_Options.Count;
        while(0 <= --ub) if (!m_Options[ub].IsSuppressed(a)) return false;
        return true;
      }

      public WorldUpdate? Reduce()
      {
        var ub = m_Options.Count;
        while (0 <= --ub) {
          if (m_Options[ub] is CanReduce<WorldUpdate> reducing) {
            var now = reducing.Reduce();
            if (null == now) {
              m_Options.RemoveAt(ub);
              continue;
            }
            if (now is Fork fork) {
              m_Options.RemoveAt(ub);
              m_Options.AddRange(fork.m_Options);
              continue;
            } else
              m_Options[ub] = now;
          }
          var act = m_Options[ub];
          if (!act.IsLegal()) {
            m_Options.RemoveAt(ub);
            continue;
          }
          if (act is CanFinish ending) {
            if (ending.IsCompleted()) return act;
          }
        };

        switch (m_Options.Count)
        {
          case 1: return m_Options[0];
          case 0: return null;
        }

        return this;
      }

      public override ActorAction? Bind(Actor src) {
#if DEBUG
        if (!(src.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
#endif
        var actions = new List<ActorAction>();
        foreach(var x in m_Options) {
          if (x.IsRelevant(src.Location) && !x.IsSuppressed(src)) {
#if DEBUG
            if (x is Join) throw new InvalidOperationException("test case: Join");
#endif
            var act = x.Bind(src);
            if (null != act) actions.Add(act);
          }
        }
        var act_count = actions.Count;
        if (1 >= act_count) return (0 >= act_count) ? null : actions[0];
        return new _Action.Fork(src, actions);
      }


      public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
      {
        List<KeyValuePair<ActorAction, WorldUpdate?>> options = new();
        foreach(var x in m_Options) {
          var stage = x.BindReduce(src);
          if (null != stage) options.Add(stage.Value);
        }
        if (0 >= options.Count) return null;
        if (1 == options.Count) return options[0];
        // XXX would like something more reasonable here
        List<ActorAction> actions = new();
        List<WorldUpdate> next = new();
        foreach(var x in options) {
          actions.Add(x.Key);
          if (null != x.Value) next.Add(x.Value);
        }
        return new(new _Action.Fork(src, actions), 0 >= next.Count ? null : new Fork(next));
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

      public Join Append(WorldUpdate next)
      {
         return new Join(new List<WorldUpdate>(m_Options), next); // value-copy for safety
      }

      public bool ForceRelevant(Location loc, ref WorldUpdate dest) {
        var staging = new List<WorldUpdate>();
        foreach(var act in m_Options) if (act.IsRelevant(loc)) staging.Add(act);
        var staged = staging.Count;
        if (1 > staged) throw new InvalidOperationException("tried to force-relevant a not-relevant objective");
        // also of interest: maybe prefilter by whether actions are performable?
        if (2 <= staged) {
          var should_sift = staging.HaveItBothWays(act => act is Join);
          if (should_sift.Key) {
            // have Join in here.  non-Join would be faster; would be more useful to retype as Join
            if (should_sift.Value) {
              // also have non-Join
#if DEBUG
              throw new InvalidOperationException("test case");
#endif
            } else {
              // random-select a Join
              dest = Rules.Get.DiceRoller.Choose(staging);
              return true;
            }
          }
        }
        if (2 <= staged) {
          if (m_Options.Count > staged) dest = new Fork(staging);
          return false;
        }
        dest = staging[0];
        return true;
      }
    }
}
