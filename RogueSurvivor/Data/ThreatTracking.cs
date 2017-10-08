using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

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

#if DEAD_FUNC
		public List<Actor> ThreatAt(Location loc)
		{
		  lock(_threats) {
			return _threats.Keys.Where(a=>_threats[a].Keys.Contains(loc.Map) && _threats[a][loc.Map].Contains(loc.Position)).ToList();
		  }
		}
#endif

		public HashSet<Point> ThreatWhere(Map map)
		{
          HashSet<Point> ret = new HashSet<Point>();
		  lock(_threats) {
            foreach (var x in _threats) {
              if (!x.Value.ContainsKey(map)) continue;
              ret.UnionWith(x.Value[map]);
            }
		  }
		  return ret;
		}

		public HashSet<Point> ThreatWhere(Map map, Rectangle view)  // we exploit Rectangle being value-copied rather than reference-copied here
		{
          HashSet<Point> ret = new HashSet<Point>();
          Zaimoni.Data.Dataflow<Map,int> crossdistrict_ok = new Zaimoni.Data.Dataflow<Map,int>(map,Map.UsesCrossDistrictView);
          Point pos = map.District.WorldPosition;   // only used in denormalized cases
          if (0> view.Left) {
            if (0<pos.X && 0<crossdistrict_ok.Get) {
              HashSet<Point> tmp = ThreatWhere(Engine.Session.Get.World[pos.X-1,pos.Y].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(map.Width+view.Left,view.Top,-view.Left,view.Height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X-map.Width,pt.Y));
            }
            view.Width += view.Left;
            view.X = 0;
          };
          if (map.Width < view.Right) {
            int new_width = map.Width-view.Left;
            if (Engine.Session.Get.World.Size>pos.X+1 && 0<crossdistrict_ok.Get) {
              HashSet<Point> tmp = ThreatWhere(Engine.Session.Get.World[pos.X+1,pos.Y].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(0,view.Top,view.Width-new_width,view.Height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X+map.Width,pt.Y));
            }
            view.Width = new_width;
          };
          if (0 > view.Top) {
            if (0<pos.Y && 0<crossdistrict_ok.Get && 3!= crossdistrict_ok.Get) {
              HashSet<Point> tmp = ThreatWhere(Engine.Session.Get.World[pos.X,pos.Y-1].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,map.Height+view.Top,view.Width,-view.Top));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y-map.Height));
            }
            view.Height += view.Top;
            view.X = 0;
          };
          if (map.Height < view.Bottom) {
            int new_height = map.Height-view.Top;
            if (Engine.Session.Get.World.Size>pos.Y+1 && 0<crossdistrict_ok.Get && 3 != crossdistrict_ok.Get) {
              HashSet<Point> tmp = ThreatWhere(Engine.Session.Get.World[pos.X,pos.Y-1].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,0,view.Width,view.Height-new_height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y+map.Height));
            }
            view.Height = new_height;
          };
		  lock(_threats) {
            HashSet<Point> tmp = new HashSet<Point>();
            foreach (var x in _threats) {
              if (!x.Value.ContainsKey(map)) continue;
              tmp.UnionWith(x.Value[map]);
            }
            if (0<view.Left) tmp.RemoveWhere(pt => pt.X<view.Left);
            if (0<view.Top) tmp.RemoveWhere(pt => pt.Y<view.Top);
            if (map.Width>view.Right) tmp.RemoveWhere(pt => pt.X >= view.Right);
            if (map.Height>view.Bottom) tmp.RemoveWhere(pt => pt.Y >= view.Bottom);
            if (0 >= ret.Count) ret = tmp;
            else ret.UnionWith(tmp);
		  }
		  return ret;
		}

#if DEAD_FUNC
        public List<Actor> ThreatIn(Map map)
		{
		  lock(_threats) {
			return _threats.Keys.Where(a=>_threats[a].Keys.Contains(map)).ToList();
		  }
		}
#endif

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
            _threats[a] = new Dictionary<Map, HashSet<Point>>(1);
            _threats[a][loc.Map] = new HashSet<Point> { loc.Position };
          }
        }

		public void Cleared(Map m, IEnumerable<Point> pts)
        {
          lock(_threats) {
            foreach(var x in _threats) {
              if (!x.Value.ContainsKey(m)) continue;
              x.Value[m].ExceptWith(pts);
              if (0 >= x.Value[m].Count) {
                x.Value.Remove(m);
#if DEBUG
                if (0 >= x.Value.Count) throw new InvalidOperationException(x.Key.Name+" inferred to be nowhere");
#endif
              }
            }
          }
          // FOV is small compared to district size so will not overflow both ways
          int crossdistrict_ok = Map.UsesCrossDistrictView(m);
          if (0 >= crossdistrict_ok) return;
          Point pos = m.District.WorldPosition;   // only used in denormalized cases
          List<Point> invalid_xy = new List<Point>(pts.Count());
          List<Point> invalid_x = new List<Point>(pts.Count());
          List<Point> invalid_y = new List<Point>(pts.Count());
          int x_delta = 0;
          int y_delta = 0;
          foreach(Point pt in pts) {
                if (0 > pt.X) {
                  if (0 >= pos.X) continue;
                  x_delta = -1;
                  if (0 > pt.Y) {
                    if (3 == crossdistrict_ok) continue;
                    if (0 >= pos.Y) continue;
                    y_delta = -1;
                    invalid_xy.Add(new Point(pt.X+m.Width,pt.Y+m.Height));
                  } else if (m.Height <= pt.Y) {
                    if (3 == crossdistrict_ok) continue;
                    if (Engine.Session.Get.World.Size <= pos.Y+1) continue;
                    y_delta = 1;
                    invalid_xy.Add(new Point(pt.X+m.Width,pt.Y-m.Height));
                  } else {
                    invalid_x.Add(new Point(pt.X+m.Width,pt.Y));
                  }
                } else if (m.Width <= pt.X) {
                  if (Engine.Session.Get.World.Size <= pos.X+1) continue;
                  x_delta = 1;
                  if (0 > pt.Y) {
                    if (3 == crossdistrict_ok) continue;
                    if (0 >= pos.Y) continue;
                    y_delta = -1;
                    invalid_xy.Add(new Point(pt.X-m.Width,pt.Y+m.Height));
                  } else if (m.Height <= pt.Y) {
                    if (3 == crossdistrict_ok) continue;
                    if (Engine.Session.Get.World.Size <= pos.Y+1) continue;
                    y_delta = 1;
                    invalid_xy.Add(new Point(pt.X-m.Width,pt.Y-m.Height));
                  } else {
                    invalid_x.Add(new Point(pt.X-m.Width,pt.Y));
                  }
                } else if (3 == crossdistrict_ok) continue;
                else if (0 > pt.Y) {
                  if (0 >= pos.Y) continue;
                  y_delta = -1;
                  invalid_y.Add(new Point(pt.X,pt.Y+m.Height));
                } else if (m.Height <= pt.Y) {
                  if (Engine.Session.Get.World.Size <= pos.Y+1) continue;
                  y_delta = 1;
                  invalid_y.Add(new Point(pt.X,pt.Y-m.Height));
                }
          }
          if (0==x_delta && 0==y_delta) return;
          if (0<invalid_x.Count) Cleared(Engine.Session.Get.World[pos.X+x_delta,pos.Y].CrossDistrictViewing(crossdistrict_ok),invalid_x);
          if (0<invalid_y.Count) Cleared(Engine.Session.Get.World[pos.X,pos.Y+y_delta].CrossDistrictViewing(crossdistrict_ok),invalid_y);
          if (0<invalid_xy.Count) Cleared(Engine.Session.Get.World[pos.X+x_delta,pos.Y+y_delta].CrossDistrictViewing(crossdistrict_ok),invalid_xy);
        }

        public void Cleared(Actor a)
        {
          lock(_threats) {  _threats.Remove(a); }
        }

        // cheating die handler
        private void HandleDie(object sender, Actor.DieArgs e)
        {
          lock(_threats) { _threats.Remove(sender as Actor); }
        }

        // cheating move handler
        private void HandleMove(object sender, EventArgs e)
        {
          Actor moving = (sender as Actor);
          lock (_threats) {
            if (!_threats.ContainsKey(moving)) return;
            List<Point> tmp = moving.LegalSteps;
            if (null == tmp) return;
			tmp.Add(moving.Location.Position);
            RecordTaint(moving,moving.Location.Map, tmp);
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
          return _locs.ContainsKey(map) ? new HashSet<Point>(_locs[map]) : new HashSet<Point>();
		}
	  }

      public HashSet<Point> In(Map map, Rectangle view)
	  {
          HashSet<Point> ret = new HashSet<Point>();
          Zaimoni.Data.Dataflow<Map,int> crossdistrict_ok = new Zaimoni.Data.Dataflow<Map,int>(map,Map.UsesCrossDistrictView);
          Point pos = map.District.WorldPosition;   // only used in denormalized cases
          if (0> view.Left) {
            if (0<pos.X && 0<crossdistrict_ok.Get) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos.X-1,pos.Y].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(map.Width+view.Left,view.Top,-view.Left,view.Height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X-map.Width,pt.Y));
            }
            view.Width += view.Left;
            view.X = 0;
          };
          if (map.Width < view.Right) {
            int new_width = map.Width-view.Left;
            if (Engine.Session.Get.World.Size>pos.X+1 && 0<crossdistrict_ok.Get) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos.X+1,pos.Y].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(0,view.Top,view.Width-new_width,view.Height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X+map.Width,pt.Y));
            }
            view.Width = new_width;
          };
          if (0 > view.Top) {
            if (0<pos.Y && 0<crossdistrict_ok.Get && 3!= crossdistrict_ok.Get) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos.X,pos.Y-1].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,map.Height+view.Top,view.Width,-view.Top));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y-map.Height));
            }
            view.Height += view.Top;
            view.X = 0;
          };
          if (map.Height < view.Bottom) {
            int new_height = map.Height-view.Top;
            if (Engine.Session.Get.World.Size>pos.Y+1 && 0<crossdistrict_ok.Get && 3 != crossdistrict_ok.Get) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos.X,pos.Y-1].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,0,view.Width,view.Height-new_height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y+map.Height));
            }
            view.Height = new_height;
          };
		  lock(_locs) {
            HashSet<Point> tmp = new HashSet<Point>();
            if (!_locs.TryGetValue(map,out HashSet<Point> tmp2)) return ret;
            tmp.UnionWith(tmp2);    // want a value copy here

            if (0<view.Left) tmp.RemoveWhere(pt => pt.X<view.Left);
            if (0<view.Top) tmp.RemoveWhere(pt => pt.Y<view.Top);
            if (map.Width>view.Right) tmp.RemoveWhere(pt => pt.X >= view.Right);
            if (map.Height>view.Bottom) tmp.RemoveWhere(pt => pt.Y >= view.Bottom);
            if (0 >= ret.Count) ret = tmp;
            else ret.UnionWith(tmp);
		  }
		  return ret;
      }

      public void Record(Map m, IEnumerable<Point> pts)
      {
        lock(_locs) {
		  if (!_locs.ContainsKey(m)) _locs[m] = new HashSet<Point>();
          _locs[m].UnionWith(pts.Where(pt => m.GetTileModelAt(pt).IsWalkable));
        }
      }

      public void Record(Map m, Point pt)
      {
        lock(_locs) {
          if (!m.GetTileModelAt(pt).IsWalkable) return; // reject unwalkable tiles
		  if (!_locs.ContainsKey(m)) _locs[m] = new HashSet<Point>();
          _locs[m].Add(pt);
        }
      }

      public void Record(Location loc)
      {
		lock(_locs) {
          if (!loc.Map.GetTileModelAt(loc.Position).IsWalkable) return; // reject unwalkable tiles
		  if (!_locs.ContainsKey(loc.Map)) _locs[loc.Map] = new HashSet<Point>();
          _locs[loc.Map].Add(loc.Position);
		}
      }

      public void Seen(Map m, IEnumerable<Point> pts)
      {
        lock(_locs) {
		  if (!_locs.ContainsKey(m)) return;
          _locs[m].ExceptWith(pts);
          if (0 >= _locs[m].Count) _locs.Remove(m);
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
