using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // Ancestor class Goal_PathTo
    [Serializable]
    internal class PathTo : Objective, LatePathable
    {
      private readonly HashSet<Location> _locs;
      private readonly bool walking;
      private int turns;

      public PathTo(Actor who, in Location loc, bool walk=false,int n=int.MaxValue)
      : base(who.Location.Map.LocalTime.TurnCounter, who)
      {
        _locs = new HashSet<Location>{loc};
        walking = walk;
        turns = n;
      }

      public PathTo(Actor who, IEnumerable<Location> locs, bool walk = false, int n = int.MaxValue)
      : base(who.Location.Map.LocalTime.TurnCounter, who)
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
        if (null != m_Actor.Controller.enemies_in_FOV) {
          _isExpired = true;    // cancel: something urgent
          return true;
        }
        return false; // defer actual pathing until later
      }

      public ActorAction? Pathing()
      {
        var dests = _locs.Where(loc => !loc.StrictHasActorAt);
        if (!dests.Any()) return null;

        var oai = m_Actor.Controller as ObjectiveAI;
        var ret = oai.BehaviorPathTo(new HashSet<Location>(_locs));
        if (!(ret?.IsPerformable() ?? false)) {
          _isExpired = true;    // cancel: buggy
          return null;
        }

        while(ret is Resolvable res) ret = res.ConcreteAction;
        if (ret is ActionMoveStep step) {
          bool want_to_walk = true;
          if (m_Actor.CanRun()) {
            if (walking) {
              if (m_Actor.MaxSTA <= m_Actor.StaminaPoints) want_to_walk = false;
              else if (!m_Actor.RunIsFreeMove && !m_Actor.WillTireAfter(oai.STA_delta(0, 1, 0, 0) + m_Actor.RunningStaminaCost(step.dest)))
                want_to_walk = false;
            } else {
              if (!m_Actor.WillTireAfter(oai.STA_delta(0, 1, 0, 0) + m_Actor.RunningStaminaCost(step.dest)))
                want_to_walk = false;
            }
          }
          if (want_to_walk) m_Actor.Walk();
          else m_Actor.Run();
        } else m_Actor.Walk();

        return ret;
      }
    }
}
