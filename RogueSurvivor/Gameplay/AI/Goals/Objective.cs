using System;
using System.Collections.Generic;
using djack.RogueSurvivor.Data;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    internal abstract class Objective
    {
      protected int turn;   // turn count of WorldTime .. will need a more complex representation at some point.
      protected readonly Actor m_Actor;   // owning actor is likely important
      protected bool _isExpired;

      public int TurnCounter { get { return turn; } }
      public bool IsExpired { get { return _isExpired; } }
      public Actor Actor { get { return m_Actor; } }

      protected Objective(int t0, Actor who)
      {
#if DEBUG
         if (null == who) throw new ArgumentNullException(nameof(who));
#endif
         turn = t0;
         m_Actor = who;
      }

      /// <param name="ret">null triggers deletion.  non-null ret.IsPerformable() must be true</param>
      /// <returns>true to take action</returns>
      public abstract bool UrgentAction(out ActorAction ret);

      public virtual List<Objective> Subobjectives() { return null; }
    }

    internal interface Pathable
    {
        ActorAction Pathing();
    }

    [Serializable]
    internal class PathToTarget {
        enum What {
            NONE = 0,
            VIEW,   // invalidated if neither tourism nor threat has this
            INVENTORY // invalidated if required item is absent; this is a base from which offsets are calculated
        }

        public readonly Dictionary<Location,int> reasons;
        public readonly List<List<Point>> pt_path = null;
        public readonly List<List<Location>> loc_path = null;
        private bool _expired = false;
        public bool Expired { get { return _expired; } }

        PathToTarget(Dictionary<Location, int> why, List<List<Point>> src)
        {
            reasons = why;
            pt_path = src;
        }
        PathToTarget(Dictionary<Location, int> why, List<List<Location>> src)
        {
            reasons = why;
            loc_path = src;
        }

        int steps {
            get {
                if (null != pt_path) return pt_path.Count;
                if (null != loc_path) return loc_path.Count;
                return 0;
            }
        }

        static Dictionary<Location, int> encode_reasons(IEnumerable<Location> goals, ObjectiveAI ai) {
            var ret = new Dictionary<Location, int>();
            Predicate<Location> test = null;
            var items = ai.ItemMemory;
            if (null != items) test = test.Or(loc => {
                var think_is_there = items.WhatIsAt(loc);
                if (null != think_is_there) {
                    foreach (var it in ai.WhatDoINeedNow()) {
                        if (think_is_there.Contains(it)) {
                            ret[loc] = (int)What.INVENTORY + (int)it;
                            return true;
                        }
                    }
                    foreach (var it in ai.WhatDoIWantNow()) {
                        if (think_is_there.Contains(it)) {
                            ret[loc] = (int)What.INVENTORY + (int)it;
                            return true;
                        }
                    }
                }
                return false;
            });

            var threat = ai.ControlledActor.Threats;
            if (null != threat) test = test.Or(loc => {
                if (threat.AnyThreatAt(loc)) {
                    ret[loc] = (int)What.VIEW;
                    return true;
                }
                return false;
            });

            var tourism = ai.ControlledActor.InterestingLocs;
            if (null != tourism) test = test.Or(loc => {
                if (tourism.Contains(loc)) {
                    ret[loc] = (int)What.VIEW;
                    return true;
                }
                return false;
            });

            foreach (var loc in goals) test(loc);
            return 0 < ret.Count ? ret : null;
        }

        static Dictionary<Location, int> encode_reasons(IEnumerable<Point> goals, ObjectiveAI ai) {
            var map = ai.ControlledActor.Location.Map;
            var xform = new List<Location>();
            foreach (var pt in goals) {
                var loc = new Location(map, pt);
                if (loc.ForceCanonical()) xform.Add(loc);
            }
            return encode_reasons(xform,ai);
        }

        bool Expire(ObjectiveAI ai) {
            if (_expired) return true;
            if (0 >= steps) {
                _expired = true;
                return true;
            }
            var refresh = encode_reasons(reasons.Keys, ai);
            if (null == refresh) {
                _expired = true;
                return true;
            }
            reasons.Clear();
            foreach (var x in refresh) reasons.Add(x.Key, x.Value);
            return false;
        }
    }
}
