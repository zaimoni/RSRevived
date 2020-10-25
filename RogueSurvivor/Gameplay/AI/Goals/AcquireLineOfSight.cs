using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // only need to see one target to expire
    [Serializable]
    class AcquireLineOfSight : Objective, Pathable, Observer<Location[]>
    {
        readonly private HashSet<Location> _locs;
        [NonSerialized] ObjectiveAI oai;

        public AcquireLineOfSight(Actor who, in Location loc) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (!who.CanEnter(loc)) throw new InvalidOperationException("unpathable");
#endif
            _locs = new HashSet<Location> { loc };
            OnDeserialized(default);
        }

        public AcquireLineOfSight(Actor who, IEnumerable<Location> locs) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            _locs = new HashSet<Location>(locs);
            _locs.RemoveWhere(goal => !who.CanEnter(goal));
#if DEBUG
            if (0 >= _locs.Count) throw new ArgumentNullException(nameof(locs));
#endif
            OnDeserialized(default);
        }

        public AcquireLineOfSight(Actor who, HashSet<Location> locs) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            _locs = locs; // intentional by-reference assignment
            _locs.RemoveWhere(goal => !who.CanEnter(goal));
#if DEBUG
            if (0 >= _locs.Count) throw new ArgumentNullException(nameof(locs));
#endif
            OnDeserialized(default);
        }

        [OnDeserialized] private void OnDeserialized(StreamingContext context)
        {
            if (m_Actor.Controller is ObjectiveAI ai) oai = ai;
            else throw new ArgumentNullException(nameof(oai));
        }

#if PROTOTYPE
        public void NewTarget(Location target)
        {
            _locs.Add(target);
        }

        public void NewTarget(IEnumerable<Location> target)
        {
            _locs.UnionWith(target);
        }

        public void RemoveTarget(Location target)
        {
            _locs.Remove(target);
        }

        public void RemoveTarget(IEnumerable<Location> target)
        {
            _locs.ExceptWith(target);
        }
#endif

        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            if (IsExpired) return true;

            foreach (Actor friend in m_Actor.Allies) { // if any in-communication ally can see the location, clear it
                if (!oai.InCommunicationWith(friend)) continue;
                if (_locs.Any(loc => friend.Controller.CanSee(in loc))) {
                    _isExpired = true;
#if DEBUG
                    if (oai.ControlledActor.IsDebuggingTarget) throw new InvalidOperationException("tracing");
#endif
                    return true;
                }
            }
#if DEBUG
            if (0 < oai.InterruptLongActivity() && oai.ControlledActor.IsDebuggingTarget) throw new InvalidOperationException("tracing");
#endif
            // defer actual pathing action to allow other non-combat heuristics to cut in
            return false;
        }

        public ActorAction? Pathing() { return 0 < oai.InterruptLongActivity() ? null : oai.BehaviorPathTo(_locs); }

        public bool update(Location[] src) {
            if (_locs.Any(x => 0 <= Array.IndexOf(src, x))) {
                _isExpired = true;
#if DEBUG
                if (oai.ControlledActor.IsDebuggingTarget) throw new InvalidOperationException("tracing");
#endif
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Acquiring line of sight to any of " + _locs.to_s();
        }
    }
}
