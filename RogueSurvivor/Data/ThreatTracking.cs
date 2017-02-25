using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class ThreatTracking : ISerializable
    {
        // The first iteration of this cost 39MB of savefile size for a full probability analysis.

		// The second iteration of this took 42MB when trying to be realistic at the start of the game, for taint checking.  Conjecture is that storing full locations is
		// expensive compared to map with points within the map.

        // As we actually have to iterate over the keys of _threats in a multi-threaded situation, just lock it when using.
        private readonly Dictionary<Actor, Dictionary<Map, HashSet<Point>>> _threats = new Dictionary<Actor, Dictionary<Map, HashSet<Point>>>();

        public ThreatTracking()
        {
          Actor.Dies += HandleDie;  // XXX removal would be in destructor
          Actor.Moving += HandleMove;
        }

#region Implement ISerializable
        // general idea is Plain Old Data before objects.
        protected ThreatTracking(SerializationInfo info, StreamingContext context)
        {
          _threats = (Dictionary<Actor, Dictionary<Map, HashSet<Point>>>)info.GetValue("threats",typeof(Dictionary<Actor, Dictionary<Map, HashSet<Point>>>));
          Actor.Dies += HandleDie;  // XXX removal would be in destructor
          Actor.Moving += HandleMove;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
          info.AddValue("threats", _threats, typeof(Dictionary<Actor, HashSet<Location>>));
        }
#endregion

        public void Clear()
        {
          lock(_threats) { _threats.Clear(); }
        }

        public bool IsThreat(Actor a)
        {
          lock(_threats) { return _threats.ContainsKey(a); }
        }

		public List<Actor> ThreatAt(Location loc)
		{
		  lock(_threats) {
			return _threats.Keys.Where(a=>_threats[a].Keys.Contains(loc.Map) && _threats[a][loc.Map].Contains(loc.Position)).ToList();
		  }
		}

		public HashSet<Point> ThreatWhere(Map map)
		{
          HashSet<Point> ret = new HashSet<Point>();
		  lock(_threats) {
            foreach (Actor a in _threats.Keys.Where(a => _threats[a].Keys.Contains(map))) {
              ret.UnionWith(_threats[a][map]);
            }
		  }
		  return ret;
		}

		public List<Actor> ThreatIn(Map map)
		{
		  lock(_threats) {
			return _threats.Keys.Where(a=>_threats[a].Keys.Contains(map)).ToList();
		  }
		}

        public void RecordSpawn(Actor a, Map m, IEnumerable<Point> pts)
        {
          lock(_threats) {  
		    if (!_threats.ContainsKey(a)) _threats[a] = new Dictionary<Map, HashSet<Point>>();
		    _threats[a][m] = new HashSet<Point>(pts); }
        }

        public void RecordTaint(Actor a, Location loc)
        {
		  lock(_threats) {
		    if (!_threats.ContainsKey(a)) _threats[a] = new Dictionary<Map, HashSet<Point>>();
		    if (!_threats[a].ContainsKey(loc.Map)) _threats[a][loc.Map] = new HashSet<Point>();
            _threats[a][loc.Map].Add(loc.Position);
		  }
        }

        public void RecordTaint(Actor a, Map m, Point p)
        {
		  lock(_threats) {
		    if (!_threats.ContainsKey(a)) _threats[a] = new Dictionary<Map, HashSet<Point>>();
		    if (!_threats[a].ContainsKey(m)) _threats[a][m] = new HashSet<Point>();
            _threats[a][m].Add(p);
		  }
        }

        public void RecordTaint(Actor a, Map m, IEnumerable<Point> pts)
        {
		  lock(_threats) {
		    if (!_threats.ContainsKey(a)) _threats[a] = new Dictionary<Map, HashSet<Point>>();
		    if (!_threats[a].ContainsKey(m)) _threats[a][m] = new HashSet<Point>();
            _threats[a][m].UnionWith(pts);
		  }
        }


        public void Sighted(Actor a, Location loc)
        {
          lock(_threats) { 
		    if (!_threats.ContainsKey(a)) _threats[a] = new Dictionary<Map, HashSet<Point>>();
            _threats[a][loc.Map] = new HashSet<Point>();
            _threats[a][loc.Map].Add(loc.Position);
          }
        }

		public void Cleared(Map m, HashSet<Point> pts)
        {
          lock(_threats) { 
            foreach (Actor a in _threats.Keys.ToList().Where(a=>_threats[a].ContainsKey(m))) {
			  _threats[a][m] = new HashSet<Point>(_threats[a][m].Except(pts));
			  if (0 >= _threats[a][m].Count) _threats[a].Remove(m);
			  if (0 >= _threats[a].Count) _threats.Remove(a);	// should not happen
			}
          }
        }

        public void Cleared(Actor a)
        {
          lock(_threats) {  _threats.Remove(a); }
        }

        // cheating die handler
        private void HandleDie(object sender, Actor.DieArgs e)
        {
          Contract.Requires(null!=(sender as Actor));
          lock(_threats) { _threats.Remove(sender as Actor); }
        }

        // cheating move handler
        private void HandleMove(object sender, EventArgs e)
        {
          Contract.Requires(null != (sender as Actor));
          Actor moving = (sender as Actor);
          lock (_threats) {
            if (!_threats.ContainsKey(moving)) return;
            List<Point> tmp = moving.LegalSteps;
            if (null == tmp) return;
			tmp.Add(moving.Location.Position);
            foreach(Point pt in tmp) RecordTaint(moving,moving.Location.Map, tmp);
          }
        }
    }   // end ThreatTracking definition

    // stripped down version of above, just managing locations of interest across maps
    // due to locking, prefer to not reuse this above.
    [Serializable]
    class LocationSet
    {
      private readonly Dictionary<Map, HashSet<Point>> _locs = new Dictionary<Map, HashSet<Point>>();

      public LocationSet()
      {
      }

      public void Clear()
      {
        lock (_locs) { _locs.Clear(); }
      }

      public bool Contains(Location loc)
      {
        lock (_locs) {
		  return _locs.ContainsKey(loc.Map) && _locs[loc.Map].Contains(loc.Position);
		}
	  }

      public HashSet<Point> In(Map map)
	  {
		lock(_locs) {
          return _locs.ContainsKey(map) ? _locs[map] : new HashSet<Point>();
		}
	  }

      public void Record(Map m, IEnumerable<Point> pts)
      {
        lock(_locs) {  
		  if (!_locs.ContainsKey(m)) _locs[m] = new HashSet<Point>();
          _locs[m].UnionWith(pts);
        }
      }

      public void Record(Map m, Point pt)
      {
        lock(_locs) {  
		  if (!_locs.ContainsKey(m)) _locs[m] = new HashSet<Point>();
          _locs[m].Add(pt);
        }
      }

      public void Record(Location loc)
      {
		lock(_locs) {
		  if (!_locs.ContainsKey(loc.Map)) _locs[loc.Map] = new HashSet<Point>();
          _locs[loc.Map].Add(loc.Position);
		}
      }

      public void Seen(Map m, IEnumerable<Point> pts)
      {
        lock(_locs) {  
		  if (!_locs.ContainsKey(m)) return;
          IEnumerable<Point> tmp = _locs[m].Except(pts);
          if (tmp.Any()) _locs[m] = new HashSet<Point>(tmp);
          else _locs.Remove(m);
        }
      }

      public void Seen(Map m, Point pt)
      {
        lock(_locs) {  
		  if (!_locs.ContainsKey(m)) return;
          if (_locs[m].Remove(pt) && 0>=_locs[m].Count) _locs.Remove(m);
        }
      }

      public void Seen(Location loc)
      {
		lock(_locs) {
		  if (!_locs.ContainsKey(loc.Map)) return;
          if (_locs[loc.Map].Remove(loc.Position) && 0 >= _locs[loc.Map].Count) _locs.Remove(loc.Map);
		}
      }
    }
}
