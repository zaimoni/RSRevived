using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Zaimoni.Data;
using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class ThreatTracking
    {
        // The first iteration of this cost 39MB of savefile size for a full probability analysis.

		// The second iteration of this took 42MB when trying to be realistic at the start of the game, for taint checking.  Conjecture is that storing full locations is
		// expensive compared to map with points within the map.

        // As we actually have to iterate over the keys of _threats in a multi-threaded situation, just lock it when using.
        private readonly Dictionary<Actor, Dictionary<Map, HashSet<Point>>> _threats = new Dictionary<Actor, Dictionary<Map, HashSet<Point>>>();

        public ThreatTracking()
        {
          InstallHandlers(default);
        }

        [OnDeserialized] private void InstallHandlers(StreamingContext context) {
          Actor.Dies += HandleDie;  // XXX removal would be in destructor
          Actor.Moving += HandleMove;
          _ThreatWhere_cache = new Dictionary<Map, HashSet<Point>>();
        }

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

#nullable enable
        public List<Actor> ThreatAt(in Location loc)
		{
          var ret = new List<Actor>();
		  lock(_threats) {
            foreach(var x in _threats) {
              if (!x.Value.TryGetValue(loc.Map,out var cache)) continue;
              if (!cache.Contains(loc.Position)) continue;
              ret.Add(x.Key);
            }
		  }
          return ret;
		}

        private void threatAt(ZoneLoc zone, Dictionary<Actor, Dictionary<Map, Point[]>> catalog)
        {
          Func<Point,bool> ok = pt => zone.Rect.Contains(pt);
		  lock(_threats) {
            foreach(var x in _threats) {
              if (!x.Value.TryGetValue(zone.m,out var cache)) continue;
              if (!cache.Any(ok)) continue;
              if (catalog.TryGetValue(x.Key, out var cache2)) {
                cache2.Add(zone.m, cache.Where(ok).ToArray());
              } else {
                catalog.Add(x.Key, new Dictionary<Map, Point[]> {
                    [zone.m] = cache.Where(ok).ToArray()
                });
              }
            }
		  }
        }

        private void threatAt(ZoneLoc zone, Dictionary<Actor, Dictionary<Map, Point[]>> catalog, Func<Actor, bool> pred)
        {
          Func<Point,bool> ok = pt => zone.Rect.Contains(pt);
		  lock(_threats) {
            foreach(var x in _threats) {
              if (!pred(x.Key)) continue;
              if (!x.Value.TryGetValue(zone.m,out var cache)) continue;
              if (!cache.Any(ok)) continue;
              if (catalog.TryGetValue(x.Key, out var cache2)) {
                cache2.Add(zone.m, cache.Where(ok).ToArray());
              } else {
                catalog.Add(x.Key, new Dictionary<Map, Point[]> {
                    [zone.m] = cache.Where(ok).ToArray()
                });
              }
            }
		  }
        }

		public Dictionary<Actor, Dictionary<Map, Point[]>> ThreatAt(ZoneLoc zone)
		{
          var catalog = new Dictionary<Actor, Dictionary<Map, Point[]>>();
          var canon = zone.GetCanonical;
          if (null == canon) {
            threatAt(zone, catalog);
          } else {
            foreach(var z in canon) threatAt(z, catalog);
          }
          return catalog;
		}

		public Dictionary<Actor, Dictionary<Map, Point[]>> ThreatAt(ZoneLoc zone, Func<Actor, bool> pred)
		{
          var catalog = new Dictionary<Actor, Dictionary<Map, Point[]>>();
          var canon = zone.GetCanonical;
          if (null == canon) {
            threatAt(zone, catalog, pred);
          } else {
            foreach(var z in canon) threatAt(z, catalog, pred);
          }
          return catalog;
		}

        private void imageThreat(ZoneLoc zone, Dictionary<Map, Point[]> catalog, Func<Actor, bool> pred)
        {
          Func<Point,bool> ok = pt => zone.Rect.Contains(pt);
		  lock(_threats) {
            var staging = new HashSet<Point>();
            foreach(var x in _threats) {
              if (!pred(x.Key)) continue;
              if (!x.Value.TryGetValue(zone.m,out var cache)) continue;
              if (!cache.Any(ok)) continue;
              staging.UnionWith(cache.Where(ok));
            }
            catalog.Add(zone.m, staging.ToArray());
		  }
        }

        public List<KeyValuePair<Map, Point[]>>? ImageThreat(ZoneLoc zone, Func<Actor, bool> pred)
        {
          var catalog = new Dictionary<Map, Point[]>();
          var canon = zone.GetCanonical;
          if (null == canon) {
            imageThreat(zone, catalog, pred);
          } else {
            foreach(var z in canon) imageThreat(z, catalog, pred);
          }
          return 0 < catalog.Count ? catalog.ToList() : null;
        }
#nullable restore

        private HashSet<Point> _ThreatWhere(Map map)    // needs lock against _ThreatWhere_cache
        {
          var ret = new HashSet<Point>();
		  lock(_threats) {
            foreach (var x in _threats) {
              if (x.Value.TryGetValue(map, out var src)) ret.UnionWith(src);
            }
            _ThreatWhere_cache.Add(map, ret);
		  }
		  return ret;
		}

		public bool AnyThreatAt(in Location loc)
		{
          lock(_ThreatWhere_cache) {
            if (_ThreatWhere_cache.TryGetValue(loc.Map, out var cache2)) return cache2.Contains(loc.Position);
            return _ThreatWhere(loc.Map).Contains(loc.Position);
          }
        }

        public bool AnyThreatAt(ZoneLoc loc)
		{
          Func<Point,bool> ok = pt => loc.Rect.Contains(pt);
          lock(_ThreatWhere_cache) {
            if (_ThreatWhere_cache.TryGetValue(loc.m, out var cache2)) return cache2.Any(ok);
            return _ThreatWhere(loc.m).Any(ok);
          }
        }

        [NonSerialized] Dictionary<Map, HashSet<Point>> _ThreatWhere_cache = new Dictionary<Map, HashSet<Point>>();
        /// <remarks>Both callers only use the value in read-only ways so correctness-required value copy is omitted</remarks>
		public HashSet<Point> ThreatWhere(Map map)
		{
          lock(_ThreatWhere_cache) {
            if (_ThreatWhere_cache.TryGetValue(map, out var cache)) return cache;
            return _ThreatWhere(map);
          }
        }

        public HashSet<Point> ThreatWhere(Map map, Rectangle view)  // we exploit Rectangle being value-copied rather than reference-copied here
		{
          var ret = new HashSet<Point>();
          if (null == map) return ret;
          var crossdistrict_ok = District.UsesCrossDistrictView(map);
          if (0 < crossdistrict_ok) {
            Point pos = map.DistrictPos;   // only used in denormalized cases
            var world = Engine.Session.Get.World;
            District test = null;
            // subway may be null
            if (0 > view.Left) {
              if (null != (test = world.At(pos + Direction.W))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(map.Width + view.Left, view.Top, -view.Left, view.Height)))
                  ret.Add(new Point(pt.X-map.Width,pt.Y));
              }
              view.Width += view.Left;
              view.X = 0;
            }
            if (map.Width < view.Right) {
              var new_width = map.Width;
              new_width -= view.Left;
              if (null != (test = world.At(pos + Direction.E))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(0, view.Top, view.Width - new_width, view.Height)))
                  ret.Add(new Point(pt.X+map.Width,pt.Y));
              }
              view.Width = new_width;
            }
            if (0 > view.Top) {
              if (null != (test = world.At(pos + Direction.N))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, map.Height + view.Top, view.Width, -view.Top)))
                  ret.Add(new Point(pt.X,pt.Y-map.Height));
              }
              view.Height += view.Top;
              view.Y = 0;
            }
            if (map.Height < view.Bottom) {
              var new_height = map.Height;
              new_height -= view.Top;
              if (null != (test = world.At(pos + Direction.S))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, 0, view.Width, view.Height - new_height)))
                  ret.Add(new Point(pt.X,pt.Y+map.Height));
              }
              view.Height = new_height;
            }
	      }
          var tmp = new HashSet<Point>();
    	  lock(_threats) {
            foreach (var x in _threats) {
              if (!x.Value.TryGetValue(map, out var src)) continue;
              tmp.UnionWith(src);
            }
		  }
          if (!view.Contains(map.Rect)) tmp.RemoveWhere(pt => !view.Contains(pt));
          if (0 >= ret.Count) ret = tmp;
          else ret.UnionWith(tmp);
		  return ret;
		}

		public bool AnyThreatIn(Map map, Rectangle view)  // we exploit Rectangle being value-copied rather than reference-copied here
		{
          if (null == map) return false;
          var crossdistrict_ok = District.UsesCrossDistrictView(map);
          if (0 < crossdistrict_ok) {
            Point pos = map.DistrictPos;   // only used in denormalized cases
            var world = Engine.Session.Get.World;
            District? test = null;
            // subway may be null
            if (0 > view.Left) {
              if (null != (test = world.At(pos + Direction.W))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(map.Width + view.Left, view.Top, -view.Left, view.Height))) return true;
              }
              view.Width += view.Left;
              view.X = 0;
            }
            if (map.Width < view.Right) {
              var new_width = map.Width;
              new_width -= view.Left;
              if (null != (test = world.At(pos + Direction.E))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(0, view.Top, view.Width - new_width, view.Height))) return true;
              }
              view.Width = new_width;
            }
            if (0 > view.Top) {
              if (null != (test = world.At(pos + Direction.N))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, map.Height + view.Top, view.Width, -view.Top))) return true;
              }
              view.Height += view.Top;
              view.Y = 0;
            }
            if (map.Height < view.Bottom) {
              var new_height = map.Height;
              new_height -= view.Top;
              if (null != (test = world.At(pos + Direction.S))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, 0, view.Width, view.Height - new_height))) return true;
              }
              view.Height = new_height;
            }
	      }
          var tmp = new HashSet<Point>();
    	  lock(_threats) {
            foreach (var x in _threats) {
              if (!x.Value.TryGetValue(map, out var src)) continue;
              tmp.UnionWith(src);
            }
		  }
          if (!view.Contains(map.Rect)) tmp.RemoveWhere(pt => !view.Contains(pt));
          return 0<tmp.Count;
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
            if (!_threats.TryGetValue(a,out var cache)) _threats.Add(a, cache = new Dictionary<Map, HashSet<Point>>());
		    cache.Add(m, new HashSet<Point>(pts));
          }
        }

        public void RecordTaint(Actor a, Location loc)
        {
		  lock(_threats) {
            _ThreatWhere_cache.Remove(loc.Map); // XXX could be more selective
            if (!_threats.TryGetValue(a,out var cache)) _threats.Add(a, cache = new Dictionary<Map, HashSet<Point>>());
            if (cache.TryGetValue(loc.Map, out var cache2)) cache2.Add(loc.Position);
            else cache.Add(loc.Map, new HashSet<Point> { loc.Position });
		  }
        }

        public void RecordTaint(Actor a, Map m, Point p)
        {
		  lock(_threats) {
            _ThreatWhere_cache.Remove(m); // XXX could be more selective
            if (!_threats.TryGetValue(a,out var cache)) _threats.Add(a, cache = new Dictionary<Map, HashSet<Point>>());
            if (cache.TryGetValue(m, out var cache2)) cache2.Add(p);
            else cache.Add(m, new HashSet<Point> { p });
		  }
        }

        public void RecordTaint(Actor a, Map m, IEnumerable<Point> pts)
        {
          var local = new List<Point>(pts.Count());
          List<Point> other = null;
          foreach(Point pt in pts) {
            (m.IsInBounds(pt) ? local : (other ??= new List<Point>(pts.Count()))).Add(pt);
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
            if (_threats.TryGetValue(a, out var cache)) {
              foreach(var x in cache) _ThreatWhere_cache.Remove(x.Key);
            }
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
          Point pos = m.DistrictPos;   // only used in denormalized cases
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
                    invalid_xy.Add(pt + m.Extent);
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
                    invalid_xy.Add(pt - m.Extent);
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

        public void Cleared(IEnumerable<Location> locs)
        { // assume all values of locs are in canonical form
          var staging = new Dictionary<Map, HashSet<Point>>();
          Map? last_map = null;
          HashSet<Point>? last_pts = null;
          foreach(var loc in locs) {
            if (last_map == loc.Map) last_pts.Add(loc.Position);
            else {
              last_map = loc.Map;
              if (!staging.TryGetValue(last_map, out last_pts)) staging.Add(last_map, (last_pts = new HashSet<Point>()));
              last_pts.Add(loc.Position);
            }
          }
          lock(_threats) {
            foreach(var m_pts in staging) {
              _ThreatWhere_cache.Remove(m_pts.Key);   // XXX could be more selective
              var amnesia = new List<Actor>();
              foreach(var x in _threats) {
                if (!x.Value.TryGetValue(m_pts.Key, out var test)) continue;
                test.ExceptWith(m_pts.Value);
                if (0 >= test.Count) {
                  x.Value.Remove(m_pts.Key);
                  if (0 >= x.Value.Count) amnesia.Add(x.Key);
                }
              }
              foreach(Actor a in amnesia) _threats.Remove(a);
            }
          }
        }

        public void Cleared(Dictionary<Actor, Dictionary<Map, Point[]>> catalog) {
            lock (_threats) {
                foreach (var x in catalog) {
                    if (_threats.TryGetValue(x.Key, out var cache)) {
                        foreach (var y in x.Value) {
                            if (cache.TryGetValue(y.Key, out var cache2)) {
                                var former = cache2.Count;
                                cache2.ExceptWith(y.Value);
                                var now = cache2.Count;
                                if (former > now) {
                                    _ThreatWhere_cache.Remove(y.Key);
                                    if (0 >= now) cache.Remove(y.Key);
                                }
                            }
                        }
                        if (0 >= cache.Count) _threats.Remove(x.Key);
                    }
                }
            }
        }

        public void Cleared(Location loc)
        { // assume all values of locs are in canonical form
          lock(_threats) {
            _ThreatWhere_cache.Remove(loc.Map);   // XXX could be more selective
            Actor? amnesia = null;
            foreach(var x in _threats) {
                if (x.Value.TryGetValue(loc.Map, out var test)) {
                  test.Remove(loc.Position);
                  if (0 >= test.Count) {
                    x.Value.Remove(loc.Map);
                    if (0 >= x.Value.Count) amnesia = x.Key;
                  }
                }
            }
            if (null != amnesia) _threats.Remove(amnesia);
          }
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
            List<Actor>? discard = null;
            lock (_threats) {
                foreach (var x in _threats) {
                    if (ok(x.Key)) continue;
                    (discard ??= new List<Actor>(_threats.Count)).Add(x.Key);
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

      public void Clear() { lock(_locs) { _locs.Clear(); } }

      public bool Contains(in Location loc)
      {
        lock(_locs) {
		  return _locs.TryGetValue(loc.Map,out var test) && test.Contains(loc.Position);
		}
	  }

      public bool ContainsAny(ZoneLoc loc)
      {
        Func<Point,bool> ok = pt => loc.Rect.Contains(pt);
        lock(_locs) {
		  return _locs.TryGetValue(loc.m,out var cache) && cache.Any(ok);
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
          var crossdistrict_ok = District.UsesCrossDistrictView(map);
          Point pos = map.DistrictPos;   // only used in denormalized cases
          if (0> view.Left) {
            if (0<pos.X && 0<crossdistrict_ok) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos + Direction.W].CrossDistrictViewing(crossdistrict_ok),new Rectangle(map.Width+view.Left,view.Top,-view.Left,view.Height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X-map.Width,pt.Y));
            }
            view.Width += view.Left;
            view.X = 0;
          }
          if (map.Width < view.Right) {
            var new_width = map.Width;
            new_width -= view.Left;
            if (Engine.Session.Get.World.Size>pos.X+1 && 0<crossdistrict_ok) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos + Direction.E].CrossDistrictViewing(crossdistrict_ok),new Rectangle(0,view.Top,view.Width-new_width,view.Height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X+map.Width,pt.Y));
            }
            view.Width = new_width;
          }
          if (0 > view.Top) {
            if (0<pos.Y && 0<crossdistrict_ok && 3!= crossdistrict_ok) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos + Direction.N].CrossDistrictViewing(crossdistrict_ok),new Rectangle(view.Left,map.Height+view.Top,view.Width,-view.Top));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y-map.Height));
            }
            view.Height += view.Top;
            view.Y = 0;
          }
          if (map.Height < view.Bottom) {
            var new_height = map.Height;
            new_height -= view.Top;
            if (Engine.Session.Get.World.Size>pos.Y+1 && 0<crossdistrict_ok && 3 != crossdistrict_ok) {
              HashSet<Point> tmp = In(Engine.Session.Get.World[pos + Direction.S].CrossDistrictViewing(crossdistrict_ok),new Rectangle(view.Left,0,view.Width,view.Height-new_height));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X,pt.Y+map.Height));
            }
            view.Height = new_height;
          }
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
          if (!_locs.TryGetValue(m, out var cache)) _locs.Add(m, cache = new HashSet<Point>());
          cache.UnionWith(pts.Where(pt => m.GetTileModelAt(pt).IsWalkable));
        }
      }

      public void Record(Map m, in Point pt)
      {
        if (!m.GetTileModelAt(pt).IsWalkable) return; // reject unwalkable tiles
        lock(_locs) {
          if (!_locs.TryGetValue(m, out var cache)) _locs.Add(m,(cache = new HashSet<Point>()));
          cache.Add(pt);
        }
      }

      public void Record(in Location loc)
      {
        if (!loc.Map.GetTileModelAt(loc.Position).IsWalkable) return; // reject unwalkable tiles
		lock(_locs) {
          if (_locs.TryGetValue(loc.Map, out var cache)) cache.Add(loc.Position);
          else _locs.Add(loc.Map, new HashSet<Point> { loc.Position });
		}
      }

#if DEAD_FUNC
      public void Seen(Map m, Predicate<Point> fail) {
        lock(_locs) {
          if (_locs.TryGetValue(m, out var target) && 0 < target.RemoveWhere(fail) && 0 >= target.Count) _locs.Remove(m);
        }
      }
#endif

      public void Seen(Location[] locs) {
        // assume all of these are in canonical form
        lock(_locs) {
          foreach(var loc in locs) {
		    if (_locs.TryGetValue(loc.Map, out var target) && target.Remove(loc.Position) && 0 >= target.Count) _locs.Remove(loc.Map);
          }
        }
      }

      // duplicate signature rather than risk indirection through IEnumerable; the Location[] signature is on a profile-hot path 2020-09-27 zaimoni
      public void Seen(List<Location> locs) {
        // assume all of these are in canonical form
        lock(_locs) {
          foreach(var loc in locs) {
		    if (_locs.TryGetValue(loc.Map, out var target) && target.Remove(loc.Position) && 0 >= target.Count) _locs.Remove(loc.Map);
          }
        }
      }

      public void Seen(in Location loc)
      {
        // assume loc is in canonical form
		lock(_locs) {
		  if (_locs.TryGetValue(loc.Map, out var target) && target.Remove(loc.Position) && 0 >= target.Count) _locs.Remove(loc.Map);
		}
      }

#if DEAD_FUNC
      public void Seen(Map m, IEnumerable<Point> pts)
      {
        foreach(Point pt in pts) Seen(new Location(m,pt));
      }

      public void Seen(Map m, Point pt) { Seen(new Location(m,pt)); }
#endif
    }

    [Serializable]
    class LocationFunction<T>
    {
        private readonly Dictionary<Map, Dictionary<Point, T>> _locs = new Dictionary<Map, Dictionary<Point, T>>();
        [NonSerialized] private List<delete_from>? _handlers = null;
        [NonSerialized] private Action<Location>? _after_delete = null;

        private class delete_from {
            public static readonly delete_from BLANK = new delete_from();

            private readonly List<Location> triggers;
            private readonly List<Location> targets;
            private readonly List<LocationFunction<T>> hosts;

            private delete_from() { }

            public delete_from(Location trigger, Location target, LocationFunction<T> host) {
                triggers = new List<Location> { trigger };
                targets = new List<Location> { target };
                hosts = new List<LocationFunction<T>> { host };
            }

            public delete_from(IEnumerable<Location> trigger, IEnumerable<Location> target, LocationFunction<T> host)
            {
                triggers = new List<Location>(trigger);
                targets = new List<Location>(target);
                hosts = new List<LocationFunction<T>> { host };
            }

            /// <returns>true if and only if this has activated and should be deleted</returns>
            public bool Fire(in Location trigger) {
                if (null == triggers) return false; // BLANK is no-op
                if (targets.Remove(trigger) && 0 >= targets.Count) return true; // just became no-op
                if (!triggers.Remove(trigger)) return false; // did not match
                if (0 < triggers.Count) return false; // still have work to do
                foreach (var host in hosts) host.Remove(targets);
                return true;
            }

#if PROTOTYPE
            public bool Merge(delete_from src) {
                if (!triggers.ValueEqual(src.triggers)) return false;
                if (1 == targets.Count && targets.ValueEqual(src.targets)) {
                    foreach (var host in src.hosts) {
                        if (!hosts.Contains(host)) hosts.Add(host);
                        return true;
                    }
                }
                if (1 == hosts.Count && hosts.ValueEqual(src.hosts)) {
                    foreach (var target in src.targets) {
                        if (!targets.Contains(target)) targets.Add(target);
                        return true;
                    }
                }
                return false;
            }
#endif
        }

        public void Clear() { lock (_locs) { _locs.Clear(); } }
        public void ClearHandlers() { lock (_locs) { _handlers = null; } }
        public void InstallAfterDelete(Action<Location> src) {
#if DEBUG
            if (null != _after_delete) throw new InvalidOperationException("need to build out handler composition");
#endif
            _after_delete = src;
        }

        public bool Contains(in Location loc) {
            lock (_locs) {
                return _locs.TryGetValue(loc.Map, out var test) && test.ContainsKey(loc.Position);
            }
        }

        public bool TryGetValue(in Location loc, out T dest) {
            dest = default;
            lock (_locs) {
                if (_locs.TryGetValue(loc.Map, out var test) && test.TryGetValue(loc.Position, out var ret)) {
                    dest = ret;
                    return true;
                }
            }
            return false;
        }

        public T this[Location loc]
        {
          get {
            lock (_locs) {
                if (_locs.TryGetValue(loc.Map, out var test) && test.TryGetValue(loc.Position, out var ret)) return ret;
                throw new KeyNotFoundException(loc.ToString());
            }
          }

          set {
            lock (_locs) {
                if (_locs.TryGetValue(loc.Map, out var test)) test[loc.Position] = value;
                else _locs.Add(loc.Map, new Dictionary<Point, T> { [loc.Position] = value });
            }
          }
        }

        public T this[Map m, IEnumerable<Point> pts]
        {
            set {
                if (null == pts || !pts.Any()) return;
                lock (_locs) {
                    if (_locs.TryGetValue(m, out var cache)) {
                        foreach (var pt in pts) cache[pt] = value;
                    } else {
                        var staging = new Dictionary<Point, T>();
                        foreach (var pt in pts) staging.Add(pt, value);
                        _locs.Add(m, staging);
                    }
                }
            }
        }

#if PROTOTYPE
        // lock provided by callers
        private void merge(delete_from src) {
            if (null == _handlers) {
                _handlers = new List<delete_from> { src };
                return;
            }
            int ub = _handlers.Count;
            while (0 <= --ub) {
                if (src.Merge(_handlers[ub])) {
                    _handlers.RemoveAt(ub);
                    ub = _handlers.Count;
                }
            }
            _handlers.Add(src);
        }
#endif

        // lock provided by callers
        private void fire_delete(in Location loc) {
            if (null != _handlers) {
                // XXX linear search XXX
                int ub = _handlers.Count;
                while (0 <= --ub) {
                    var ev = delete_from.BLANK;
                    _handlers[ub] = Interlocked.Exchange(ref ev, _handlers[ub]);
                    if (ev.Fire(in loc)) {
                        _handlers.Remove(delete_from.BLANK);
                        if (ub > _handlers.Count) ub = _handlers.Count;
                    } else _handlers[ub] = ev;
                }
                if (0 >= _handlers.Count) _handlers = null;
            }
        }

        // lock provided by callers
        private bool remove(in Location loc) {
            if (_locs.TryGetValue(loc.Map, out var test) && test.Remove(loc.Position)) {
                fire_delete(in loc);
                if (0 >= test.Count) _locs.Remove(loc.Map);
                if (null != _after_delete) _after_delete(loc);
                return true;
            }
            return false;
        }

        public bool Remove(Location loc) { lock (_locs) { return remove(in loc); } }

        public int Remove(IEnumerable<Location> locs)
        {
            int ret = 0;
            lock (_locs) {
                foreach (var loc in locs) if (remove(in loc)) ret += 1;
            }
            return ret;
        }

        public int RemoveWhere(Map m, Func<Point, bool> fail) {
          lock(_locs) {
            int ret = 0;
            if (_locs.TryGetValue(m, out var cache)) {
              foreach(var pt in cache.Keys.Where(fail).ToArray()) if (cache.Remove(pt)) {
                fire_delete(new Location(m, pt));
                ret += 1;
              }
              if (0 >= cache.Count) _locs.Remove(m);
            }
            return ret;
          }
        }

        public List<Location>? RemoveWhere(Func<T, bool> fail) {
          lock(_locs) {
            var ret = new List<Location>();
            var ex_map = new List<Map>();
            foreach(var x in _locs) {
              var doomed = new List<Point>();
              foreach(var y in x.Value) {
                if (fail(y.Value)) doomed.Add(y.Key);
              }
              foreach(var pt in doomed) {
                if (x.Value.Remove(pt)) {
                  var loc = new Location(x.Key, pt);
                  fire_delete(in loc);
                  ret.Add(loc);
                }
              }
              if (0 >= x.Value.Count) ex_map.Add(x.Key);
            }
            foreach(var m in ex_map) _locs.Remove(m);
            return (0 < ret.Count) ? ret : null;
          }
        }

#region proxy setup
        public LocationFunction<T> ForwardingClone()
        {
          lock(_locs) {
            var ret = new LocationFunction<T>();
            foreach(var x in _locs) {
              ret._locs.Add(x.Key, new Dictionary<Point, T>(x.Value));
              foreach(var y in x.Value) {
                var loc = new Location(x.Key, y.Key);
                (ret._handlers ??= new List<delete_from>()).Add(new delete_from(loc, loc, this));
              }
            }
            return ret;
          }
        }

        public void ForwardingMerge(LocationFunction<T> src, Func<T,T,int> cmp)
        {
          lock(_locs) {
            lock(src._locs) {
              foreach(var x in src._locs) {
                if (_locs.TryGetValue(x.Key, out var cache)) {
                  foreach(var y in x.Value) {
                    if (cache.TryGetValue(y.Key, out var legacy)) {
                      int code = cmp(legacy, y.Value);
                      if (0 > code) continue; // new value "worse"
                      var loc = new Location(x.Key, y.Key);
                      if (0 < code) {
                        // new value "better"
                        fire_delete(loc);
                        cache[y.Key] = y.Value;
                        (_handlers ??= new List<delete_from>()).Add(new delete_from(loc, loc, src));
                        continue;
                      }
//                    merge(new delete_from(loc, loc, src));
                      (_handlers ??= new List<delete_from>()).Add(new delete_from(loc, loc, src));
                      continue;
                    } else {
                      cache.Add(y.Key, y.Value);
                      var loc = new Location(x.Key, y.Key);
                      (_handlers ??= new List<delete_from>()).Add(new delete_from(loc, loc, src));
                      continue;
                    }
                  }
                } else {
                  _locs.Add(x.Key, new Dictionary<Point, T>(x.Value));
                  foreach(var y in x.Value) {
                    var loc = new Location(x.Key, y.Key);
                    (_handlers ??= new List<delete_from>()).Add(new delete_from(loc, loc, src));
                  }
                }
              }
            }
          }
        }
#endregion

       public void Relink(IEnumerable<Location> triggers, IEnumerable<Location> targets) {
            (_handlers ??= new List<delete_from>()).Add(new delete_from(triggers, targets, this));
       }

        // single-threaded context
        public KeyValuePair<Dictionary<Location, ZoneLoc[]>, List<KVpair<ZoneLoc, ZoneLoc[]>>> GoalStats() {
            var goals = new Dictionary<Location, ZoneLoc[]>();
            var goal_zones = new List<ZoneLoc>();

            foreach (var x in _locs) {
                foreach (var y in x.Value) {
                    var loc = new Location(x.Key, y.Key);
                    var tmp_zones = loc.TrivialDistanceZones;
                    if (null == tmp_zones) continue;
                    foreach (var z in tmp_zones) if (!goal_zones.Contains(z)) goal_zones.Add(z);
                    goals.Add(loc, tmp_zones);
                }
            }

            var relay = new List<KVpair<ZoneLoc, ZoneLoc[]>>();
            foreach (var z in goal_zones) relay.Add(new KVpair<ZoneLoc, ZoneLoc[]>(z, z.ExitZones));

            return new KeyValuePair<Dictionary<Location, ZoneLoc[]>, List<KVpair<ZoneLoc, ZoneLoc[]>>>(goals, relay);
       }

       private void Within(ZoneLoc src, Dictionary<Location, T> dest)
       {
            var exits = src.Exits;
            lock (_locs) {
                foreach (var x in _locs) {
                    foreach (var y in x.Value) {
                        var loc = new Location(x.Key, y.Key);
                        if (src.Contains(loc)) dest.Add(loc, y.Value);
                        else if (0 <= Array.IndexOf(exits, loc)) dest.Add(loc, y.Value);
                    }
                }
            }
        }

       public Dictionary<Location, T> Within(IEnumerable<ZoneLoc> src)
       {
            var ret = new Dictionary<Location, T>();
            foreach (var z in src) Within(z, ret);
            return ret;
        }

       public Dictionary<Location, ZoneLoc[]> Goals() {
            var goals = new Dictionary<Location, ZoneLoc[]>();

            foreach (var x in _locs) {
                foreach (var y in x.Value) {
                    var loc = new Location(x.Key, y.Key);
                    var tmp_zones = loc.TrivialDistanceZones;
                    if (null != tmp_zones) goals.Add(loc, tmp_zones);
                }
            }

            return goals;
       }
    }
}
