using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
#else
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
#endif

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
          lock(_threats) {
            _threats.Clear();
            _ThreatWhere_cache.Clear();
          }
        }

        public bool IsThreat(Actor a)
        {
          lock(_threats) { return _threats.ContainsKey(a); }
        }

        public bool Any()
        {
          lock(_threats) { return 0<_threats.Count; }
        }


#if DEAD_FUNC
		public List<Actor> ThreatAt(Location loc)
		{
		  lock(_threats) {
			return _threats.Keys.Where(a=>_threats[a].Keys.Contains(loc.Map) && _threats[a][loc.Map].Contains(loc.Position)).ToList();
		  }
		}

		public bool AnyThreatAt(Location loc)
		{
		  lock(_threats) {
            for(var x in _threats) {
              if (x.Value.TryGetValue(loc.Map,out var cache) && cache.Contains(loc.Position)) return true;
            }
            return false;
		  }
		}
#endif

        [NonSerialized] Dictionary<Map, HashSet<Point>> _ThreatWhere_cache = new Dictionary<Map, HashSet<Point>>();
		public HashSet<Point> ThreatWhere(Map map)
		{
          if (_ThreatWhere_cache.TryGetValue(map, out var cache)) return new HashSet<Point>(cache);   // value copy for correctness
          var ret = new HashSet<Point>();
		  lock(_threats) {
            foreach (var x in _threats) {
              if (x.Value.TryGetValue(map, out var src)) ret.UnionWith(src);
            }
            _ThreatWhere_cache[map] = new HashSet<Point>(ret);
		  }
		  return ret;
		}

		public HashSet<Point> ThreatWhere(Map map, Rectangle view)  // we exploit Rectangle being value-copied rather than reference-copied here
		{
          var ret = new HashSet<Point>();
          if (null == map) return ret;
          var crossdistrict_ok = new Zaimoni.Data.Dataflow<Map,int>(map,District.UsesCrossDistrictView);
          if (0 < crossdistrict_ok.Get) {
            Point pos = map.District.WorldPosition;   // only used in denormalized cases
            var world = Engine.Session.Get.World;
            District test = null;
            // subway may be null
            if (0 > view.Left) {
              if (null != (test = world.At(pos + Direction.W))) {
                HashSet<Point> tmp = ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(map.Width+view.Left,view.Top,-view.Left,view.Height));
                foreach(Point pt in tmp) ret.Add(new Point(pt.X-map.Width,pt.Y));
              }
              view.Width += view.Left;
              view.X = 0;
            };
            if (map.Width < view.Right) {
              var new_width = map.Width;
              new_width -= view.Left;
              if (null != (test = world.At(pos + Direction.E))) {
                HashSet<Point> tmp = ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(0,view.Top,view.Width-new_width,view.Height));
                foreach(Point pt in tmp) ret.Add(new Point(pt.X+map.Width,pt.Y));
              }
              view.Width = new_width;
            };
            if (0 > view.Top) {
              if (null != (test = world.At(pos + Direction.N))) {
                HashSet<Point> tmp = ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,map.Height+view.Top,view.Width,-view.Top));
                foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y-map.Height));
              }
              view.Height += view.Top;
              view.Y = 0;
            };
            if (map.Height < view.Bottom) {
              var new_height = map.Height;
              new_height -= view.Top;
              if (null != (test = world.At(pos + Direction.S))) {
                HashSet<Point> tmp = ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,0,view.Width,view.Height-new_height));
                foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y+map.Height));
              }
              view.Height = new_height;
            };
	      }
    	  lock(_threats) {
            var tmp = new HashSet<Point>();
            foreach (var x in _threats) {
              if (!x.Value.TryGetValue(map, out var src)) continue;
              tmp.UnionWith(src);
            }
            if (!view.Contains(map.Rect)) tmp.RemoveWhere(pt => !view.Contains(pt));
            if (0 >= ret.Count) ret = tmp;
            else ret.UnionWith(tmp);
		  }
		  return ret;
		}

		public bool AnyThreatIn(Map map, Rectangle view)  // we exploit Rectangle being value-copied rather than reference-copied here
		{
          var ret = new HashSet<Point>();
          if (null == map) return false;
          var crossdistrict_ok = new Zaimoni.Data.Dataflow<Map,int>(map,District.UsesCrossDistrictView);
          if (0 < crossdistrict_ok.Get) {
            Point pos = map.District.WorldPosition;   // only used in denormalized cases
            var world = Engine.Session.Get.World;
            District test = null;
            // subway may be null
            if (0 > view.Left) {
              if (null != (test = world.At(pos + Direction.W))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok.Get), new Rectangle(map.Width + view.Left, view.Top, -view.Left, view.Height))) return true;
              }
              view.Width += view.Left;
              view.X = 0;
            };
            if (map.Width < view.Right) {
              var new_width = map.Width;
              new_width -= view.Left;
              if (null != (test = world.At(pos + Direction.E))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok.Get), new Rectangle(0, view.Top, view.Width - new_width, view.Height))) return true;
              }
              view.Width = new_width;
            };
            if (0 > view.Top) {
              if (null != (test = world.At(pos + Direction.N))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok.Get), new Rectangle(view.Left, map.Height + view.Top, view.Width, -view.Top))) return true;
              }
              view.Height += view.Top;
              view.Y = 0;
            };
            if (map.Height < view.Bottom) {
              var new_height = map.Height;
              new_height -= view.Top;
              if (null != (test = world.At(pos + Direction.S))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok.Get), new Rectangle(view.Left, 0, view.Width, view.Height - new_height))) return true;
              }
              view.Height = new_height;
            };
	      }
    	  lock(_threats) {
            var tmp = new HashSet<Point>();
            foreach (var x in _threats) {
              if (!x.Value.TryGetValue(map, out var src)) continue;
              tmp.UnionWith(src);
            }
            if (!view.Contains(map.Rect)) tmp.RemoveWhere(pt => !view.Contains(pt));
            return 0<tmp.Count;
		  }
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
            _ThreatWhere_cache.Remove(m);
            if (!_threats.TryGetValue(a,out var cache)) cache = _threats[a] = new Dictionary<Map, HashSet<Point>>();
		    cache[m] = new HashSet<Point>(pts);
          }
        }

        public void RecordTaint(Actor a, Location loc)
        {
		  lock(_threats) {
            _ThreatWhere_cache.Remove(loc.Map); // XXX could be more selective
            if (!_threats.TryGetValue(a,out var cache)) cache = _threats[a] = new Dictionary<Map, HashSet<Point>>();
            if (cache.TryGetValue(loc.Map, out var cache2)) cache2.Add(loc.Position);
            else cache[loc.Map] = new HashSet<Point> { loc.Position };
		  }
        }

        public void RecordTaint(Actor a, Map m, Point p)
        {
		  lock(_threats) {
            _ThreatWhere_cache.Remove(m); // XXX could be more selective
            if (!_threats.TryGetValue(a,out var cache)) cache = _threats[a] = new Dictionary<Map, HashSet<Point>>();
            if (cache.TryGetValue(m, out var cache2)) cache2.Add(p);
            else _threats[a][m] = new HashSet<Point> { p };
		  }
        }

        public void RecordTaint(Actor a, Map m, IEnumerable<Point> pts)
        {
          var local = new List<Point>(pts.Count());
          List<Point> other = null;
          foreach(Point pt in pts) {
            (m.IsInBounds(pt) ? local : (other ?? (other = new List<Point>(pts.Count())))).Add(pt);
          }
		  lock(_threats) {
            _ThreatWhere_cache.Remove(m); // XXX could be more selective
            if (!_threats.TryGetValue(a,out var map_cache)) _threats.Add(a,map_cache = new Dictionary<Map, HashSet<Point>>());
            if (!map_cache.TryGetValue(m,out var cache)) map_cache.Add(m,cache = new HashSet<Point>());
            cache.UnionWith(local);
		  }
          if (null == other) return;
          var remap = new Dictionary<Map,List<Point>>();
          foreach(Point pt in other) {
            Location? test = m.Normalize(pt);
            if (null == test) continue;
            if (!remap.TryGetValue(test.Value.Map,out var cache)) remap.Add(test.Value.Map, cache = new List<Point>(other.Count));
            cache.Add(test.Value.Position);
          }
          foreach(var x in remap) RecordTaint(a,x.Key,x.Value);
        }


        public void Sighted(Actor a, Location loc)
        {
          lock(_threats) {
            _ThreatWhere_cache.Remove(loc.Map);
            _threats[a] = new Dictionary<Map, HashSet<Point>>(1) {
              [loc.Map] = new HashSet<Point> { loc.Position }
            };
          }
        }

		public void Cleared(Map m, IEnumerable<Point> pts)
        {
          lock(_threats) {
            _ThreatWhere_cache.Remove(m);   // XXX could be more selective
            var amnesia = new List<Actor>();
            foreach(var x in _threats) {
              if (!x.Value.TryGetValue(m, out var test)) continue;
              test.ExceptWith(pts);
              if (0 >= test.Count) {
                x.Value.Remove(m);
                if (0 >= x.Value.Count) amnesia.Add(x.Key);
              }
            }
            foreach(Actor a in amnesia) _threats.Remove(a);
          }
          // FOV is small compared to district size so will not overflow both ways
          int crossdistrict_ok = District.UsesCrossDistrictView(m);
          if (0 >= crossdistrict_ok) return;
          Point pos = m.District.WorldPosition;   // only used in denormalized cases
          var invalid_xy = new List<Point>(pts.Count());
          var invalid_x = new List<Point>(pts.Count());
          var invalid_y = new List<Point>(pts.Count());
          int x_delta = 0;
          int y_delta = 0;
          foreach(Point pt in pts) {
                if (0 > pt.X) {
                  if (0 >= pos.X) continue;
                  x_delta = -1;
                  if (0 > pt.Y) {
                    if (0 >= pos.Y) continue;
                    y_delta = -1;
#if Z_VECTOR
                    invalid_xy.Add(pt + m.Extent);
#else
                    invalid_xy.Add(new Point(pt.X+m.Width,pt.Y+m.Height));
#endif
                  } else if (m.Height <= pt.Y) {
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
                    if (0 >= pos.Y) continue;
                    y_delta = -1;
                    invalid_xy.Add(new Point(pt.X-m.Width,pt.Y+m.Height));
                  } else if (m.Height <= pt.Y) {
                    if (Engine.Session.Get.World.Size <= pos.Y+1) continue;
                    y_delta = 1;
#if Z_VECTOR
                    invalid_xy.Add(pt - m.Extent);
#else
                    invalid_xy.Add(new Point(pt.X-m.Width,pt.Y-m.Height));
#endif
                  } else {
                    invalid_x.Add(new Point(pt.X-m.Width,pt.Y));
                  }
                } else if (0 > pt.Y) {
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
          lock(_threats) {
            if (_threats.TryGetValue(a, out var cache)) {
              foreach(var x in cache) _ThreatWhere_cache.Remove(x.Key);   // XXX could be more selective
              _threats.Remove(a);
            }
          }
        }

        public void Audit(Predicate<Actor> ok) {
            List<Actor> discard = null;
            lock (_threats) {
                foreach (var x in _threats) {
                    if (ok(x.Key)) continue;
                    (discard ?? (discard = new List<Actor>(_threats.Count))).Add(x.Key);
                }
            }
            if (null != discard) foreach (var a in discard) Cleared(a);
        }

        // huge cheat ... requires any tainted location to be assigned to its "nearest threat on-map"
        // mainly of use at game start
        public void Rebuild(Map m)
        {
            var who = new List<Actor>();
            var fulltaint = new HashSet<Point>();
            lock (_threats) {
                foreach (var x in _threats) {
                    if (x.Key.Location.Map != m) continue;  // ignore threats not on current map
                    if (x.Value.TryGetValue(m, out var src)) {
                        fulltaint.UnionWith(src);
                        who.Add(x.Key);
                    }
                }
            }

            if (0 >= who.Count) return; // no-op for completely inappropriate call

            var accounted_for = new HashSet<Point>();
            var boundary = new Dictionary<Point, HashSet<Actor>>();
            foreach (var a in who) {
                Sighted(a, a.Location);
                accounted_for.Add(a.Location.Position);
                foreach (var pt in a.Location.Position.Adjacent()) {
                    if (!fulltaint.Contains(pt)) continue;
                    if (accounted_for.Contains(pt)) continue;
                    if (boundary.TryGetValue(pt, out var cache)) cache.Add(a);
                    else boundary[pt] = new HashSet<Actor> { a };
                }
            }
            fulltaint.ExceptWith(accounted_for);
            while (0 < fulltaint.Count && 0 < boundary.Count) {
                accounted_for.Clear();
                var new_boundary = new Dictionary<Point, HashSet<Actor>>();
                foreach (var x in boundary) {
                    foreach (var a in x.Value) RecordTaint(a, m, x.Key);
                    accounted_for.Add(x.Key);
                    foreach (var pt in x.Key.Adjacent()) {
                        if (!fulltaint.Contains(pt)) continue;
                        if (accounted_for.Contains(pt)) continue;
                        if (new_boundary.TryGetValue(pt, out var cache)) cache.UnionWith(x.Value);
                        else new_boundary[pt] = new HashSet<Actor>(x.Value);
                    }
                }
                fulltaint.ExceptWith(accounted_for);
                boundary = new_boundary;
            }
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
            lock(_threats) {
              if (!_threats.ContainsKey(moving)) return;
              // we don't have the CPU to do this right (that is, expand out legal steps from all current candidate locations)
              List<Point> tmp = moving.LegalSteps;
              if (null == tmp) return;
			  tmp.Add(moving.Location.Position);

#if PROTOTYPE
              // logic puzzle here...automate it
              var pre_existing = ThreatWhere(moving.Location.Map, new Rectangle(moving.Location.Position.X-1,moving.Location.Position.Y-1,3,3));
              pre_existing.ExceptWith(tmp);
              if (0<pre_existing.Count) {
              }
#endif

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
        lock(_locs) { _locs.Clear(); }
      }

      public bool Contains(Location loc)
      {
        lock(_locs) {
		  return _locs.TryGetValue(loc.Map,out var test) && test.Contains(loc.Position);
		}
	  }

      public HashSet<Point> In(Map map)
	  {
		lock(_locs) {
          return _locs.TryGetValue(map, out var src) ? new HashSet<Point>(src) : new HashSet<Point>();
		}
	  }

      public HashSet<Point> In(Map map, Rectangle view)
	  {
          var ret = new HashSet<Point>();
          if (null == map) return ret;
          var crossdistrict_ok = new Zaimoni.Data.Dataflow<Map,int>(map,District.UsesCrossDistrictView);
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
            var new_width = map.Width;
            new_width -= view.Left;
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
            view.Y = 0;
          };
          if (map.Height < view.Bottom) {
            var new_height = map.Height;
            new_height -= view.Top;
            if (Engine.Session.Get.World.Size>pos.Y+1 && 0<crossdistrict_ok.Get && 3 != crossdistrict_ok.Get) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos.X,pos.Y+1].CrossDistrictViewing(crossdistrict_ok.Get),new Rectangle(view.Left,0,view.Width,view.Height-new_height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y+map.Height));
            }
            view.Height = new_height;
          };
		  lock(_locs) {
            if (!_locs.TryGetValue(map,out HashSet<Point> tmp2)) return ret;
            var tmp = new HashSet<Point>(tmp2); // want a value copy here
#if PROTOTYPE
            tmp.RemoveWhere(pt => !view.Contains(pt));
            if (tmp.Any(pt => !view.Contains(pt))) throw new InvalidOperationException("trace");
#endif

//            if (0<view.Left) tmp.RemoveWhere(pt => pt.X<view.Left);
//            if (0<view.Top) tmp.RemoveWhere(pt => pt.Y<view.Top);
//            if (map.Width>view.Right) tmp.RemoveWhere(pt => pt.X >= view.Right);
//            if (map.Height>view.Bottom) tmp.RemoveWhere(pt => pt.Y >= view.Bottom);
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
          if (!_locs.TryGetValue(m, out var cache)) _locs.Add(m,(cache = new HashSet<Point>()));
          cache.Add(pt);
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

      public void Seen(Location loc)
      {
        if (loc.ForceCanonical()) 
  		  lock(_locs) {
		    if (_locs.TryGetValue(loc.Map, out var target) && target.Remove(loc.Position) && 0 >= target.Count) _locs.Remove(loc.Map);
		  }
      }

      public void Seen(Map m, IEnumerable<Point> pts)
      {
        foreach(Point pt in pts) Seen(new Location(m,pt));
      }

      public void Seen(Map m, Point pt)
      {
        Seen(new Location(m,pt));
      }
    }
}
