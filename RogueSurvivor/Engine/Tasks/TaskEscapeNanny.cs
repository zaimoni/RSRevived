using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Tasks
{
    [Serializable]
    class TaskEscapeNanny : TimedTask
    {
        private readonly List<Actor> _escapees;
        private readonly HashSet<Point> _safe_zone;

        public TaskEscapeNanny(List<Actor>  escapees, HashSet<Point> safe_zone) : base(1)
        {
          _escapees = escapees;
          _safe_zone = safe_zone;
        }

        public override void Trigger(Map m)
        {
            _escapees.RemoveAll(a => a.IsDead);
            if (0 >= _escapees.Count) return;
            _escapees.RemoveAll(a => a.Location.Map==m && _safe_zone.Contains(a.Location.Position));
            if (0 >= _escapees.Count) return;
            foreach(var a in _escapees) {
              var ai = a.Controller as Gameplay.AI.ObjectiveAI;
              if (null != ai.Goal<Gameplay.AI.Goal_PathTo>()) continue;
              ai.GoalHeadFor(m,_safe_zone);
            }

            TurnsLeft = 1;  // regenerate -- do not thrash GC
        }
    }
}
