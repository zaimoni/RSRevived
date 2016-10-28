using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class ThreatTracking
    {
        // an earlier iteration of this cost 39MB of savefile size.  Instead of attempting a full probability analysis,
        // we'll just do taint checking.
        private Dictionary<Actor, HashSet<Location>> _threats;  // simpler taint tracking

        public ThreatTracking()
        {
          _threats = new Dictionary<Data.Actor, HashSet<Data.Location>>();
          Actor.Dies += HandleDie;  // XXX removal would be in destructor
          Actor.Moving += HandleMove;
        }

        public void Clear()
        {
          _threats.Clear();
        }

        public bool IsThreat(Actor a)
        {
          return _threats.ContainsKey(a);
        }

        public void RecordSpawn(Actor a, IEnumerable<Location> locs)
        {
          _threats[a] = new HashSet<Location>(locs);
        }

        public void RecordTaint(Actor a, Location loc)
        {
          if (!_threats.ContainsKey(a))  _threats[a] = new HashSet<Location>();
          _threats[a].Add(loc);
        }

        public void Sighted(Actor a, Location loc)
        {
          _threats[a] = new HashSet<Location>();
          _threats[a].Add(loc);
        }

        public void Cleared(Location loc)
        { // some sort of race condition here ... a dead actor may be removed between _threats[a].Remove(loc)
          // and _threats[a].Count
          foreach (Actor a in _threats.Keys.ToList().Where(a=>!a.IsDead)) {
            if (_threats[a].Remove(loc) && 0 >= _threats[a].Count) _threats.Remove(a);
          }
        }

        public void Cleared(Actor a)
        {
          _threats.Remove(a);
        }

        // cheating die handler
        private void HandleDie(object sender, Actor.DieArgs e)
        {
          Contract.Requires(null!=(sender as Actor));
          _threats.Remove(sender as Actor);
        }

        // cheating move handler
        private void HandleMove(object sender, EventArgs e)
        {
          Contract.Requires(null != (sender as Actor));
          Actor moving = (sender as Actor);
          if (!_threats.ContainsKey(moving)) return;
          List<Point> tmp = moving.OneStepRange(moving.Location.Map, moving.Location.Position);
          foreach(Point pt in tmp) {
            _threats[moving].Add(new Location(moving.Location.Map,pt));
          }
        }
    }
}
