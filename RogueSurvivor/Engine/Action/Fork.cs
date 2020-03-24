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
    class Fork : ActorAction,BackwardPlan
    {
      private readonly List<ActorAction> m_Options;

      // imports pre-existing legal path calculation as the "starting point"
      public Fork(Actor actor, Dictionary<Location, ActorAction>? legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (null == legal_path) throw new ArgumentNullException(nameof(legal_path));
#endif
        m_Options = new List<ActorAction>(legal_path.Count);
        foreach(var x in legal_path) {
          if (x.Value is Actions.ActorDest) m_Options.Add(new Engine.Actions.ActionMoveDelta(m_Actor, x.Key));
          else if (x.Value is Actions.Resolvable act) m_Options.Add(act.ConcreteAction);
          else m_Options.Add(x.Value);
        }
      }

      private Fork(Actor actor, List<ActorAction>? legal_path) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (null == legal_path) throw new ArgumentNullException(nameof(legal_path));
#endif
        m_Options = legal_path;
      }

      public Fork(Actor actor, ActorAction root) : base(actor)
      {
#if DEBUG
        if (!(actor.Controller is ObjectiveAI)) throw new InvalidOperationException("controller not smart enough to plan actions");
        if (!root.IsLegal()) throw new ArgumentOutOfRangeException(nameof(root), root, "must be legal");
        if (root is BackwardPlan) throw new ArgumentOutOfRangeException(nameof(root), root, "must be capable of backward planning");
        if (root is Fork) throw new InvalidOperationException("fork of fork is not reasonable");
#endif
        m_Options = new List<ActorAction> { root };
      }

      public override bool IsLegal()
      {
        int ub = m_Options.Count;
        bool ok = false;
        string? last_fail = null;
        while(0 <= --ub) {
          if (m_Options[ub].IsLegal()) ok = true;
          else {
            var fail = m_Options[ub].FailReason;
            m_Options.RemoveAt(ub);
            if (!ok && !string.IsNullOrEmpty(fail)) last_fail = fail;
          }
        }
        if (!ok) m_FailReason = last_fail;
        return ok;
      }

      // predicted to be CPU-expensive, so just assume components correctly implement performable => legal
      public override bool IsPerformable()
      {
        int ub = m_Options.Count;
        bool ok = false;
        string? last_fail = null;
        while(0 <= --ub) {
          if (m_Options[ub].IsPerformable()) ok = true;
          else {
            var fail = m_Options[ub].FailReason;
            m_Options.RemoveAt(ub);
            if (!ok && !string.IsNullOrEmpty(fail)) last_fail = fail;
          }
        }
        if (!ok) m_FailReason = last_fail;
        return ok;
      }

      public override void Perform()
      {
        (m_Actor.Controller as ObjectiveAI).ExecuteActionFork(m_Options);
      }

      public ActorAction? splice(Fork next)
      {
        List<Engine.Actions.ActionChain>? preview_ret = null;
        foreach(var start in m_Options) {
          if (start is Actions.ActorDest a_dest) {
            foreach (var end in next.m_Options) {
              if (end is Actions.ActorOrigin a_origin) {
                if (a_dest.dest == a_origin.origin) {
                  var test = new Engine.Actions.ActionChain(m_Actor, new List<ActorAction> { start, end });
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

      public List<ActorAction>? prequel() {
        List<Actions.ActionChain>? preview_ret = null;
        int ub = m_Options.Count;
        while(0 <= --ub) {
          if (m_Options[ub] is BackwardPlan act) {
            var setup = act.prequel();
            if (null != setup) foreach (var x in setup) {
              var test = new Actions.ActionChain(x, m_Options[ub]);
              if (!test.IsSemanticParadox()) (preview_ret ?? (preview_ret = new List<Actions.ActionChain>())).Add(test);
            }
          }
        }
        if (null == preview_ret) return null;
        if (1 == preview_ret.Count) return new List<ActorAction> { preview_ret[0] };
        int lb = 0;
        // following assumes ary-2 which is invalidated above
        while(lb < preview_ret.Count-1) {
          List<ActorAction>? fork_options = null;
          ub = preview_ret.Count;
          while(lb < --ub) {
            if (preview_ret[lb].ConcreteAction.AreEquivalent(preview_ret[ub].ConcreteAction)) {
              (fork_options ?? (fork_options = new List<ActorAction> { preview_ret[lb].Next })).Add(preview_ret[ub].Next);
              preview_ret.RemoveAt(ub);
            }
          }
          if (null != fork_options) preview_ret[lb] = new Actions.ActionChain(preview_ret[lb].ConcreteAction, new Fork(m_Actor, fork_options));
          lb++;
        }
        return new List<ActorAction>(preview_ret);
      }
    }
}
