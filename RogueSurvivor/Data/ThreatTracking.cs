using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
    class ThreatTracking
    {
        private Dictionary<Actor, Zaimoni.Data.DenormalizedProbability<Location>> _threats;
        private Dictionary<Map, HashSet<Actor>> _threats_on_map;    // cache field

        public ThreatTracking()
        {
          _threats = new Dictionary<Actor, Zaimoni.Data.DenormalizedProbability<Location>>();
          _threats_on_map = new Dictionary<Map, HashSet<Actor>>();
        }

        public void RecordSpawn(Actor a, IEnumerable<Location> locs)
        {
          Zaimoni.Data.DenormalizedProbability<Location> tmp = new Zaimoni.Data.DenormalizedProbability<Location>();
          foreach(Location loc in locs) {
            tmp[loc] = 1f;
            _ActorOnMap(a,loc.Map);
          }
          _threats[a] = tmp;
        }

        public void Sighted(Actor a, Location loc)
        {
          foreach(Actor tmp in new List<Actor>(_threats.Keys)) {
            if (tmp == a) continue;
#if DEBUG
            if (1 == _threats[tmp].Count && 0.0f < _threats[tmp][loc]) throw new InvalidOperationException("cannot infer "+tmp.Name+" is nowhere");
#endif
            _threats[tmp][loc] = 0.0f;
          }
          Zaimoni.Data.DenormalizedProbability<Location> tmp2 = new Zaimoni.Data.DenormalizedProbability<Location>();
          tmp2[loc] = 1f;
          _threats[a] = tmp2;
          _ActorOnMap(a,loc.Map);
          foreach(Map tmp in new List<Map>(_threats_on_map.Keys)) {
            if (tmp == loc.Map) continue;
            _ActorNotOnMap(a, tmp);
          }
        }

        public void Cleared(Location loc)
        {
          foreach(Actor tmp in new List<Actor>(_threats.Keys)) {
#if DEBUG
            if (1 == _threats[tmp].Count && 0.0f < _threats[tmp][loc]) throw new InvalidOperationException("cannot infer "+tmp.Name+" is nowhere");
#endif
            _threats[tmp][loc] = 0.0f;
          }
        }

        public void Cleared(Actor a)
        {
          _threats.Remove(a);
          foreach(Map tmp in new List<Map>(_threats_on_map.Keys)) {
            _ActorNotOnMap(a, tmp);
          }
        }

        public bool MapIsSafe(Map map)
        {
          return !_threats_on_map.ContainsKey(map);
        }

        public void MassNormalize(Map map)
        {
          if (!_threats_on_map.ContainsKey(map)) return;  // safe
          foreach(Actor tmp in _threats_on_map[map]) {
            _threats[tmp].Normalize();
          }
        }

        private void _ActorOnMap(Actor a, Map map)
        {
          if (!_threats_on_map.ContainsKey(map))  _threats_on_map[map] = new HashSet<Actor>();
          _threats_on_map[map].Add(a);
        }

        private void _ActorNotOnMap(Actor a, Map map)
        {
          if (!_threats_on_map.ContainsKey(map)) return;
          if (_threats_on_map[map].Remove(a) && 0 >= _threats_on_map[map].Count) _threats_on_map.Remove(map);
        }
    }
}
