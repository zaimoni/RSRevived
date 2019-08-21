using System;
using System.Collections.Generic;
using System.Linq;
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
            INVENTORY = 0,  // invalidated if required item is absent; this is a base from which offsets are calculated (downwards)
            VIEW,   // invalidated if neither tourism nor threat has this
            GENERATOR_OFF,  // these two both require generators at the location; different invalidation conditions
            RECHARGE
        }

        public readonly Dictionary<Location,int> reasons = new Dictionary<Location, int>();
        private List<List<Point>> pt_path = null;
        private List<List<Location>> loc_path = null;
        private readonly HashSet<Location> stage_view = new HashSet<Location>(); // these three can be non-serialized only if they are rebuilt on load
        private readonly HashSet<Location> stage_inventory = new HashSet<Location>();
        private readonly List<Engine.MapObjects.PowerGenerator> stage_generators = new List<Engine.MapObjects.PowerGenerator>();
        private bool _expired = false;

        public PathToTarget() { }

        public int steps {
            get {
                if (null != pt_path) return pt_path.Count;
                if (null != loc_path) return loc_path.Count;
                return 0;
            }
        }

#region Lambda pathing glue
        public void StageView(Map m, IEnumerable<Point> src) {
            foreach (var pt in src) stage_view.Add(new Location(m, pt));
        }
        public void UnStageView(Map m, Predicate<Point> reject)
        {
            stage_view.RemoveWhere(loc => {
                if (loc.Map != m) return false;
                return reject(loc.Position);
            });
        }

        public void StageInventory(Map m, IEnumerable<Point> src) {
            foreach (var pt in src) stage_inventory.Add(new Location(m, pt));
        }

        public void StageGenerators(IEnumerable<Engine.MapObjects.PowerGenerator> src) {
            foreach (var x in src) if (!stage_generators.Contains(x)) stage_generators.Add(x);
        }

        public void ForgetStaging() {
            stage_view.Clear();
            stage_inventory.Clear();
            stage_generators.Clear();
        }
#endregion

        private static List<List<Location>> _upgrade(Map m, List<List<Point>> src) {
            if (null == src) return null;
            var xform = new List<List<Location>>();
            foreach (var step in src) {
                if (0 >= step.Count) break;
                var new_step = new List<Location>(step.Count);
                foreach (var pt in step) new_step.Add(new Location(m, pt));
            }
            return 0 < xform.Count ? xform : null;
        }

        private Map HomeMap(ObjectiveAI ai) {
            if (null == pt_path) return null;
            if (0 < reasons.Count) return reasons.First().Key.Map;
            return ai.ControlledActor.Location.Map;
        }

        private void _upgrade(ObjectiveAI ai) {
            if (null == pt_path) return;
            loc_path = _upgrade(HomeMap(ai), pt_path);
            pt_path = null;
        }

        private void _merge_reasons(ObjectiveAI ai) {
            if (0 < stage_view.Count) {
                foreach (var x in stage_view) reasons[x] = (int)What.VIEW;
                stage_view.Clear();
            }
            if (0 < stage_inventory.Count) {
                var items = ai.ItemMemory;
                if (null != items) {
                    var need = ai.WhatDoINeedNow();
                    var want = ai.WhatDoIWantNow();

                    int code(Location loc) {
                      var think_is_there = items.WhatIsAt(loc);
                      if (null != think_is_there) {
                        // \todo duplicate cheating postfilters from ActorController::WhereIs
                        foreach (var it in need) {
                            if (think_is_there.Contains(it)) return (int)What.INVENTORY - (int)it;
                        }
                        foreach (var it in want) {
                            if (think_is_there.Contains(it)) return (int)What.INVENTORY - (int)it;
                        }
                      }
                      return 0;
                    }

                    int test;
                    foreach (var x in stage_inventory) {
                      if (0 != (test = code(x))) reasons[x] = test;
                    }
                }
                stage_inventory.Clear();
            }
            if (0 < stage_generators.Count) {
                foreach (var x in stage_generators) reasons[x.Location] = (int)What.GENERATOR_OFF;   // will downgrade to real reason on update
                stage_generators.Clear();
            }
        }

        public void Install(List<List<Location>> src, ObjectiveAI ai) {
            if (null == src || 0 >= src.Count) {
                ForgetStaging();
                return;
            }
            if (null != pt_path) _upgrade(ai);
            if (null == loc_path) loc_path = src;
            else {
                int merge_ub = loc_path.Count;
                int ub = src.Count;
                int i = -1;
                while (++i < ub) {
                    if (merge_ub <= i) loc_path.Add(src[i]);
                    else {
                        var step = loc_path[i];
                        foreach (var pt in src[i]) {
                            if (!step.Contains(pt)) step.Add(pt);
                        }
                    }
                }
            }

            _merge_reasons(ai);
            _expired = false;
        }

        public void Install(Map m, List<List<Point>> src, ObjectiveAI ai)
        {
            if (null == src || 0 >= src.Count) {
                ForgetStaging();
                return;
            }
            if (null != pt_path && m != HomeMap(ai)) _upgrade(ai);
            if (null != loc_path) { // merging into location path
                Install(_upgrade(m,src),ai);
                return;
            }
            if (null == pt_path) pt_path = src;
            else {
                int merge_ub = pt_path.Count;
                int ub = src.Count;
                int i = -1;
                while (++i < ub) {
                    if (merge_ub <= i) pt_path.Add(src[i]);
                    else {
                        var step = pt_path[i];
                        foreach (var pt in src[i]) {
                            if (!step.Contains(pt)) step.Add(pt);
                        }
                    }
                }
            }

            _merge_reasons(ai);
            _expired = false;
        }

#region path management
        // require that we be adjacent to the path.
        public static bool reject_path(List<List<Location>> min_path, ObjectiveAI ai)
        {
           if (null == min_path) return false;
           Location loc = ai.ControlledActor.Location;
           bool is_adjacent(Location pt) { return 1 != Engine.Rules.InteractionDistance(loc, pt); };

           int i = min_path.Count;
           while(0 < i--) {
             if (min_path[i].Any(pt => 1>=ai.FastestTrapKill(pt))) {
               var nonlethal = min_path[i].FindAll(pt => 1< ai.FastestTrapKill(pt));
               if (0 >= nonlethal.Count) return true;
               min_path[i] = nonlethal;
             }
             if (min_path[i].Any(is_adjacent)) {
               var adjacent = min_path[i].FindAll(is_adjacent);
               if (adjacent.Count < min_path[i].Count) min_path[i] = adjacent;
               if (0 < i) min_path.RemoveRange(0,i);
               return false;
             }
             if (min_path[i].Contains(loc)) {
               min_path.RemoveRange(0,i+1);
               break;
             }
          }
//        if (0 >= min_path.Count) return true;
          return true;   // \todo could try to reconnect if there was a path
        }

        public static bool reject_path(List<List<Point>> min_path, ObjectiveAI ai)
        {
           if (null == min_path) return true;
           Location loc = ai.ControlledActor.Location;
           Map map = loc.Map;
           Point pos = loc.Position;
           bool is_adjacent(Point pt) { return 1 == Engine.Rules.InteractionDistance(loc, new Location(map, pt)); };

           int i = min_path.Count;
           while(0 < i--) {
             if (min_path[i].Any(pt => 1>=ai.FastestTrapKill(new Location(map,pt)))) {
               var nonlethal = min_path[i].FindAll(pt => 1< ai.FastestTrapKill(new Location(map, pt)));
               if (0 >= nonlethal.Count) return true;
               min_path[i] = nonlethal;
             }
             if (min_path[i].Any(is_adjacent)) {
               var adjacent = min_path[i].FindAll(is_adjacent);
               if (adjacent.Count < min_path[i].Count) min_path[i] = adjacent;
               if (0 < i) min_path.RemoveRange(0,i);
               return false;
             }
             if (min_path[i].Contains(pos)) {
               min_path.RemoveRange(0,i+1);
               break;
             }
          }
//        if (0 >= min_path.Count) return true;
          return true;   // \todo could try to reconnect if there was a path
        }

#if PROTOTYPE
        // eliminate reasons not near the path
        private bool decay_reasons(List<List<Location>> min_path) {
            if (null == min_path) return false;
            var screen = new Dictionary<Location, int>(reasons);
            var ret = new Dictionary<Location, int>();
            int i = min_path.Count;
            while (0 < i--) {
                foreach (var pt in min_path[i]) {
                    if (screen.TryGetValue(pt,out var test)) {
                        ret.Add(pt, test);
                        screen.Remove(pt);
                        continue;
                    }
                }
                foreach (var anchor in min_path[i]) {
                    foreach (var pivot in anchor.Position.Adjacent()) {
                        var pt = new Location(anchor.Map, pivot);
                        if (!pt.ForceCanonical()) continue;
                        if (screen.TryGetValue(pt, out var test)) {
                            ret.Add(pt, test);
                            screen.Remove(pt);
                            continue;
                        }
                    }
                }
                if (0 >= screen.Count) break;
            }
            if (0 >= ret.Count) return true;
            reasons.Clear();
            foreach (var x in ret) reasons.Add(x.Key, x.Value);
            return false;
        }

        private bool decay_reasons(List<List<Point>> min_path) {
            if (null == min_path) return false;
            var map = reasons.First().Key.Map;
            var screen = new Dictionary<Location, int>(reasons);
            var ret = new Dictionary<Location, int>();
            int i = min_path.Count;
            while (0 < i--) {
                foreach (var pt2 in min_path[i]) {
                    var pt = new Location(map, pt2);
                    if (screen.TryGetValue(pt,out var test)) {
                        ret.Add(pt, test);
                        screen.Remove(pt);
                        continue;
                    }
                }
                foreach (var anchor in min_path[i]) {
                    foreach (var pivot in anchor.Adjacent()) {
                        var pt = new Location(map, pivot);
                        if (!pt.ForceCanonical()) continue;
                        if (screen.TryGetValue(pt, out var test)) {
                            ret.Add(pt, test);
                            screen.Remove(pt);
                            continue;
                        }
                    }
                }
                if (0 >= screen.Count) break;
            }
            if (0 >= ret.Count) return true;
            reasons.Clear();
            foreach (var x in ret) reasons.Add(x.Key, x.Value);
            return false;
        }

        // remove path that does not lead to a reason quickly
        private bool decay_path(List<List<Location>> min_path) {
            if (null == min_path) return false;
            var working = new List<List<Location>>();
            var reference = new List<List<Location>>();
            var triggers = new HashSet<Location>(reasons.Keys);

            void _transfer(Location pt, int i) {
                working[i].Add(pt);
                reference[i].Remove(pt);
                if (0 < i) {
                    var next = reference[i - 1].FindAll(pt2 => 1 == Engine.Rules.InteractionDistance(pt2, pt));
                    foreach (var pt2 in next) _transfer(pt2, i - 1);
                }
            }

            bool transfer(Location pt,int i) {
                if (reference[i].Contains(pt)) {
                    _transfer(pt, i);
                    return true;
                }
                return false;
            }

            foreach (var step in min_path) {
                reference.Add(new List<Location>(step)); // force value copy
                working.Add(new List<Location>());
            }

            var found = new HashSet<Location>();
            int ub = reference.Count;
            int n = -1;
            while (++n < ub) {
                foreach (var pt in triggers) {
                    if (transfer(pt, n)) found.Add(pt);
                    else if (working[n].Contains(pt)) found.Add(pt);
                }
                if (0 < found.Count) {
                    triggers.ExceptWith(found);
                    found.Clear();
                    ub = n + 1; // prepare for early exit
                }
                foreach (var anchor in triggers) {
                    foreach (var pivot in anchor.Position.Adjacent()) {
                        var pt = new Location(anchor.Map, pivot);
                        if (!pt.ForceCanonical()) continue;
                        if (transfer(pt, n)) {
                            found.Add(anchor);
                        } else if (working[n].Contains(pt)) {
                            found.Add(anchor);
                        }
                    }
                }
                if (0 < found.Count) {
                    triggers.ExceptWith(found);
                    found.Clear();
                    ub = n + 1; // prepare for early exit
                }
            }
            if (0 >= working.Count || 0>= working[0].Count) return true;
            min_path.Clear();
            foreach (var step in working) {
                if (0 >= step.Count) break;
                min_path.Add(step);
            }
            return false;
        }

        private bool decay_path(List<List<Point>> min_path) {
            if (null == min_path) return false;
            var working = new List<List<Point>>();
            var reference = new List<List<Point>>();
            var triggers = new HashSet<Location>(reasons.Keys);
            var map = reasons.First().Key.Map;

            void _transfer(Point pt, int i) {
                working[i].Add(pt);
                reference[i].Remove(pt);
                if (0 < i) {
                    var next = reference[i - 1].FindAll(pt2 => 1 == Engine.Rules.GridDistance(pt2, pt));
                    foreach (var pt2 in next) _transfer(pt2, i - 1);
                }
            }

            bool transfer(Point pt,int i) {
                if (reference[i].Contains(pt)) {
                    _transfer(pt, i);
                    return true;
                }
                return false;
            }

            foreach (var step in min_path) {
                reference.Add(new List<Point>(step)); // force value copy
                working.Add(new List<Point>());
            }

            var found = new HashSet<Location>();
            int ub = reference.Count;
            int n = -1;
            while (++n < ub) {
                foreach (var pt in triggers) {
                    if (transfer(pt.Position, n)) found.Add(pt);
                    else if (working[n].Contains(pt.Position)) found.Add(pt);
                }
                if (0 < found.Count) {
                    triggers.ExceptWith(found);
                    found.Clear();
                    ub = n + 1; // prepare for early exit
                }
                foreach (var anchor in triggers) {
                    foreach (var pivot in anchor.Position.Adjacent()) {
                        if (!map.IsInBounds(pivot)) continue;
                        if (transfer(pivot, n)) {
                            found.Add(anchor);
                        } else if (working[n].Contains(pivot)) {
                            found.Add(anchor);
                        }
                    }
                }
                if (0 < found.Count) {
                    triggers.ExceptWith(found);
                    found.Clear();
                    ub = n + 1; // prepare for early exit
                }
            }
            if (0 >= working.Count || 0>= working[0].Count) return true;
            min_path.Clear();
            foreach (var step in working) {
                if (0 >= step.Count) break;
                min_path.Add(step);
            }
            return false;
        }
#endif

        static private Func<Location,int> static_reasons(ObjectiveAI ai)
        {
            Func<Location, int> test = null;
            var items = ai.ItemMemory;
            if (null != items) {
                var need = ai.WhatDoINeedNow();
                var want = ai.WhatDoIWantNow();
                test = test.Or(loc => {
                   var think_is_there = items.WhatIsAt(loc);
                    if (null != think_is_there) {
                        // \todo duplicate cheating postfilters from ActorController::WhereIs
                        foreach (var it in need) {
                            if (think_is_there.Contains(it)) return (int)What.INVENTORY - (int)it;
                        }
                        foreach (var it in want) {
                            if (think_is_there.Contains(it)) return (int)What.INVENTORY - (int)it;
                        }
                    }
                    return 0;
                });
            }

            var threat = ai.ControlledActor.Threats;
            if (null != threat) test = test.Or(loc => {
                if (threat.AnyThreatAt(loc) && !ai.CanSee(loc)) return (int)What.VIEW;
                return 0;
            });

            var tourism = ai.ControlledActor.InterestingLocs;
            if (null != tourism) test = test.Or(loc => {
                if (tourism.Contains(loc)) return (int)What.VIEW;
                return 0;
            });
            return test;
        }

        private bool recode_reasons(ObjectiveAI ai) {
            var ret = new Dictionary<Location, int>();
            var retestable = static_reasons(ai);
            foreach (var x in reasons) {
                if ((int)What.GENERATOR_OFF == x.Value) {
                    if (x.Key.MapObject is Engine.MapObjects.PowerGenerator power && !power.IsOn) {
                        ret.Add(x.Key, x.Value);
                        continue;
                    }
                    if (ai.WantToRecharge()) ret.Add(x.Key, (int)What.RECHARGE);
                    continue;
                } else if ((int)What.RECHARGE == x.Value) {
                    if (ai.WantToRecharge()) ret.Add(x.Key, x.Value);
                    continue;
                } else if (null!=retestable) {
                    var code = retestable(x.Key);
                    if (0 != code) ret.Add(x.Key, code);
                }
            }
//          if (0 >= ret.Count) return true;    // would be incorrect if tracking actors
            if (ret.Count < reasons.Count) return true; // unclear how to safely update path so expire
            reasons.Clear();
            foreach (var x in ret) reasons.Add(x.Key, x.Value);
            return false;
        }
#endregion

        public bool Expire(ObjectiveAI ai) {
            if (_expired) return true;
            if (0 >= steps || recode_reasons(ai) || reject_path(pt_path, ai) || reject_path(loc_path, ai)) {
                _expired = true;
                return true;
            }
            // current initializers (in particular, escape from basements without any pathing targets)
            // may truncate the path which invalidates the decay heuristics
#if PROTOTYPE
            if (decay_reasons(pt_path) || decay_reasons(loc_path)) {
                _expired = true;
                return true;
            }
            if (decay_path(pt_path) || decay_path(loc_path)) {
                _expired = true;
                return true;
            }
#endif
            return false;
        }

        public ActorAction WalkPath(ObjectiveAI ai) {
            if (null != loc_path) return ai.UsePreexistingPath(loc_path);
            if (null != pt_path) return ai.UsePreexistingPath(pt_path);
            return null;
        }
    }
}
