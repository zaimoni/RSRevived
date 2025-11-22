using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Zaimoni.Data;
using Zaimoni.Lazy;
using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public sealed class ThreatTracking
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

#if DEAD_FUNC
        public void Clear()
        {
          lock(_threats) {
            _threats.Clear();
            _ThreatWhere_cache.Clear();
          }
        }
#endif

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

        private void threatAt(ZoneLoc zone, Dictionary<Actor, Dictionary<Map, Point[]>> catalog, Func<Actor, bool> pred)
        {
          Func<Point,bool> ok = pt => zone.Rect.Contains(pt);
		  lock(_threats) {
            foreach(var x in _threats) {
              if (!pred(x.Key)) continue;
              if (!x.Value.TryGetValue(zone.m,out var cache)) continue;
              if (!cache.Any(ok)) continue;
              if (!catalog.TryGetValue(x.Key, out var cache2)) catalog.Add(x.Key, cache2 = new());
              cache2.Add(zone.m, cache.Where(ok).ToArray());
            }
		  }
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

        private bool _anyThreatAt(in Location loc) {
            if (_ThreatWhere_cache.TryGetValue(loc.Map, out var cache2)) return cache2.Contains(loc.Position);
            return _ThreatWhere(loc.Map).Contains(loc.Position);
        }

        public bool AnyThreatAt(in Location loc)
		{
          lock(_ThreatWhere_cache) {
            return _anyThreatAt(in loc);
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
            var world = World.Get;
            District test = null;
            // subway may be null
            if (0 > view.Left) {
              if (null != (test = world.At(pos + Direction.W))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle((short)(map.Width + view.Left), view.Top, (short)(-view.Left), view.Height)))
                  ret.Add(new Point((short)(pt.X-map.Width),pt.Y));
              }
              view.Width += view.Left;
              view.X = 0;
            }
            if (map.Width < view.Right) {
              var new_width = map.Width;
              new_width -= view.Left;
              if (null != (test = world.At(pos + Direction.E))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(0, view.Top, (short)(view.Width - new_width), view.Height)))
                  ret.Add(new Point((short)(pt.X+map.Width),pt.Y));
              }
              view.Width = new_width;
            }
            if (0 > view.Top) {
              if (null != (test = world.At(pos + Direction.N))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, (short)(map.Height + view.Top), view.Width, (short)(-view.Top))))
                  ret.Add(new Point(pt.X,(short)(pt.Y - map.Height)));
              }
              view.Height += view.Top;
              view.Y = 0;
            }
            if (map.Height < view.Bottom) {
              var new_height = map.Height;
              new_height -= view.Top;
              if (null != (test = world.At(pos + Direction.S))) {
                foreach(Point pt in ThreatWhere(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, 0, view.Width, (short)(view.Height - new_height))))
                  ret.Add(new Point(pt.X,(short)(pt.Y + map.Height)));
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
            var world = World.Get;
            District? test = null;
            // subway may be null
            if (0 > view.Left) {
              if (null != (test = world.At(pos + Direction.W))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle((short)(map.Width + view.Left), view.Top, (short)(-view.Left), view.Height))) return true;
              }
              view.Width += view.Left;
              view.X = 0;
            }
            if (map.Width < view.Right) {
              var new_width = map.Width;
              new_width -= view.Left;
              if (null != (test = world.At(pos + Direction.E))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(0, view.Top, (short)(view.Width - new_width), view.Height))) return true;
              }
              view.Width = new_width;
            }
            if (0 > view.Top) {
              if (null != (test = world.At(pos + Direction.N))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, (short)(map.Height + view.Top), view.Width, (short)(-view.Top)))) return true;
              }
              view.Height += view.Top;
              view.Y = 0;
            }
            if (map.Height < view.Bottom) {
              var new_height = map.Height;
              new_height -= view.Top;
              if (null != (test = world.At(pos + Direction.S))) {
                if (AnyThreatIn(test.CrossDistrictViewing(crossdistrict_ok), new Rectangle(view.Left, 0, view.Width, (short)(view.Height - new_height)))) return true;
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
          lock (_ThreatWhere_cache) {
            _ThreatWhere_cache.Remove(m);
            lock(_threats) {
              if (!_threats.TryGetValue(a,out var cache)) _threats.Add(a, cache = new());
		      cache.Add(m, new(pts));
            }
          }
        }

        public void RecordTaint(Actor a, Location loc)
        {
          lock (_ThreatWhere_cache) {
            _ThreatWhere_cache.Remove(loc.Map); // XXX could be more selective
		    lock(_threats) {
              if (!_threats.TryGetValue(a,out var cache)) _threats.Add(a, cache = new());
              if (cache.TryGetValue(loc.Map, out var cache2)) cache2.Add(loc.Position);
              else cache.Add(loc.Map, new(){ loc.Position });
		    }
          }
        }

        public void RecordTaint(Actor a, Map m, Point p)
        {
          lock (_ThreatWhere_cache) {
            _ThreatWhere_cache.Remove(m); // XXX could be more selective
		    lock(_threats) {
              if (!_threats.TryGetValue(a,out var cache)) _threats.Add(a, cache = new Dictionary<Map, HashSet<Point>>());
              if (cache.TryGetValue(m, out var cache2)) cache2.Add(p);
              else cache.Add(m, new HashSet<Point> { p });
		    }
          }
        }

        public void RecordTaint(Actor a, Map m, IEnumerable<Point> pts)
        {
          List<Point> local = new(pts.Count());
          List<Point> other = null;
          foreach(Point pt in pts) {
            (m.IsInBounds(pt) ? local : (other ??= new(pts.Count()))).Add(pt);
          }
          lock (_ThreatWhere_cache) {
            _ThreatWhere_cache.Remove(m); // XXX could be more selective
		    lock(_threats) {
              if (!_threats.TryGetValue(a,out var map_cache)) _threats.Add(a,map_cache = new());
              if (!map_cache.TryGetValue(m,out var cache)) map_cache.Add(m,cache = new());
              cache.UnionWith(local);
		    }
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
          lock (_ThreatWhere_cache) {
            _ThreatWhere_cache.Remove(loc.Map);
            lock(_threats) {
              if (_threats.TryGetValue(a, out var cache)) {
                foreach(var x in cache) _ThreatWhere_cache.Remove(x.Key);
              }
              _threats[a] = new(1) {
                [loc.Map] = new(){ loc.Position }
              };
            }
          }
        }

        public void Cleared<T>(Dictionary<Map, T> staging) where T:IEnumerable<Point>
        {   // assume all values of locs are in canonical form
            lock (_ThreatWhere_cache) {
                lock (_threats) {
                    foreach (var m_pts in staging) {
                        _ThreatWhere_cache.Remove(m_pts.Key);   // XXX could be more selective
                        List<Actor> amnesia = new();
                        foreach (var x in _threats) {
                            if (!x.Value.TryGetValue(m_pts.Key, out var test)) continue;
                            test.ExceptWith(m_pts.Value);
                            if (0 >= test.Count) {
                                x.Value.Remove(m_pts.Key);
                                if (0 >= x.Value.Count) amnesia.Add(x.Key);
                            }
                        }
                        foreach (Actor a in amnesia) _threats.Remove(a);
                    }
                }
            }
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
          Cleared(staging);
        }

        public void Cleared(Dictionary<Actor, Dictionary<Map, Point[]>> catalog) {
            lock (_ThreatWhere_cache) {
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
        }

#if DEAD_FUNC
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
#endif

        public void Cleared(Actor a)
        {
            lock (_ThreatWhere_cache) {
                lock (_threats) {
                    if (_threats.TryGetValue(a, out var cache)) {
                        foreach (var x in cache) _ThreatWhere_cache.Remove(x.Key);   // XXX could be more selective
                        _threats.Remove(a);
                    }
                }
            }
        }

        public void Audit(Predicate<Actor> ok) {
            List<Actor>? discard = null;
            lock (_threats) {
                foreach (var x in _threats) {
                    if (ok(x.Key)) continue;
                    (discard ??= new(_threats.Count)).Add(x.Key);
                }
            }
            if (null != discard) foreach (var a in discard) Cleared(a);
        }

        // huge cheat ... requires any tainted location to be assigned to its "nearest threat on-map"
        // mainly of use at game start
        public void Rebuild(Map m)
        {
            List<Actor> who = new();
            HashSet<Point> fulltaint = new();
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

            HashSet<Point> accounted_for = new();
            Dictionary<Point, HashSet<Actor>> boundary = new();
            foreach (var a in who) {
                Sighted(a, a.Location);
                accounted_for.Add(a.Location.Position);
                foreach (var pt in a.Location.Position.Adjacent()) {
                    if (!fulltaint.Contains(pt)) continue;
                    if (accounted_for.Contains(pt)) continue;
                    if (boundary.TryGetValue(pt, out var cache)) cache.Add(a);
                    else boundary[pt] = new(){ a };
                }
            }
            fulltaint.ExceptWith(accounted_for);
            while (0 < fulltaint.Count && 0 < boundary.Count) {
                accounted_for.Clear();
                Dictionary<Point, HashSet<Actor>> new_boundary = new();
                foreach (var x in boundary) {
                    foreach (var a in x.Value) RecordTaint(a, m, x.Key);
                    accounted_for.Add(x.Key);
                    foreach (var pt in x.Key.Adjacent()) {
                        if (!fulltaint.Contains(pt)) continue;
                        if (accounted_for.Contains(pt)) continue;
                        if (new_boundary.TryGetValue(pt, out var cache)) cache.UnionWith(x.Value);
                        else new_boundary[pt] = new(x.Value);
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
            var tmp = moving.LegalSteps;
            if (null == tmp) return;
            tmp.Add(moving.Location.Position);

            lock (_ThreatWhere_cache) {
                lock (_threats) {
                    if (!_threats.ContainsKey(moving)) return;
                    // we don't have the CPU to do this right (that is, expand out legal steps from all current candidate locations)

                    bool new_mark = false;
                    foreach (var pt in tmp) {
                        Location loc = new(moving.Location.Map, pt);
                        if (!Map.Canonical(ref loc)) continue;
                        if (!_anyThreatAt(loc)) {
                            new_mark = true;
                            break;
                        }
                    }
                    if (new_mark) {
                        ZoneLoc near = new(moving.Location.Map, new Rectangle(moving.Location.Position + 2 * Direction.NW, new Point(5, 5)));
                        var lockdown = near.ParsedListing;
                        if (_threats.TryGetValue(moving, out var cache)) {
                            List<Map> doomed = new();
                            foreach (var x in cache) {
                                if (lockdown.TryGetValue(x.Key, out var cache2)) {
                                    x.Value.IntersectWith(cache2);
                                    if (0 < x.Value.Count) continue;
                                }
                                // HMM...inferred not to be here
                                doomed.Add(x.Key);
                            }
                            foreach (var m in doomed) {
                                cache.Remove(m);
                                _ThreatWhere_cache.Remove(m);
                            }
                        }
                    }
                }
            }
            RecordTaint(moving, moving.Location.Map, tmp);
        }
    }   // end ThreatTracking definition

    // stripped down version of above, just managing locations of interest across maps
    // due to locking, prefer to not reuse this above.
    [Serializable]
    public sealed class LocationSet
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
              HashSet<Point> tmp = In(World.Get[pos + Direction.W].CrossDistrictViewing(crossdistrict_ok),new Rectangle((short)(map.Width+view.Left),view.Top, (short)(-view.Left),view.Height));
              foreach(Point pt in tmp) ret.Add(new Point((short)(pt.X-map.Width),pt.Y));
            }
            view.Width += view.Left;
            view.X = 0;
          }
          if (map.Width < view.Right) {
            var new_width = map.Width;
            new_width -= view.Left;
            if (World.Get.Size>pos.X+1 && 0<crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.E].CrossDistrictViewing(crossdistrict_ok),new Rectangle(0,view.Top, (short)(view.Width-new_width),view.Height));
              foreach(Point pt in tmp) ret.Add(new Point((short)(pt.X+map.Width),pt.Y));
            }
            view.Width = new_width;
          }
          if (0 > view.Top) {
            if (0<pos.Y && 0<crossdistrict_ok && 3!= crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.N].CrossDistrictViewing(crossdistrict_ok),new Rectangle(view.Left, (short)(map.Height+view.Top),view.Width, (short)(-view.Top)));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X, (short)(pt.Y-map.Height)));
            }
            view.Height += view.Top;
            view.Y = 0;
          }
          if (map.Height < view.Bottom) {
            var new_height = map.Height;
            new_height -= view.Top;
            if (World.Get.Size>pos.Y+1 && 0<crossdistrict_ok && 3 != crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.S].CrossDistrictViewing(crossdistrict_ok),new Rectangle(view.Left,0,view.Width, (short)(view.Height-new_height)));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X, (short)(pt.Y+map.Height)));
            }
            view.Height = new_height;
          }
		  lock(_locs) {
            if (!_locs.TryGetValue(map,out HashSet<Point> tmp2)) return ret;
            var tmp = new HashSet<Point>(tmp2); // want a value copy here

            if (0 >= ret.Count) ret = tmp;
            else ret.UnionWith(tmp);
		  }
		  return ret;
      }

      public List<Location>? All() {
        List<Location> ret = new();
        lock(_locs) {
          foreach(var x in _locs) {
            foreach(var pt in x.Value) {
              ret.Add(new(x.Key, pt));
            }
          }
        }
        return 0 < ret.Count ? ret : null;
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
    }

    public sealed class LocationSet2 : Zaimoni.SetTheory.RelationC<Map, Point>
    {

      public bool Contains(in Location loc) => Contains(loc.Map, loc.Position);
      public bool ContainsAny(ZoneLoc loc)
      {
        Func<Point,bool> ok = pt => loc.Rect.Contains(pt);
        return Range(loc.m)?.Any(ok) ?? false;
	  }

//    public IEnumerable<Point>? In(Map map) => Range(map);
      public HashSet<Point> In(Map map) {
        var stage = Range(map);
        return null != stage ? new(stage) : new();
      }

      public HashSet<Point> In(Map map, Rectangle view)
	  {
          var ret = new HashSet<Point>();
          if (null == map) return ret;
          var crossdistrict_ok = District.UsesCrossDistrictView(map);
          Point pos = map.DistrictPos;   // only used in denormalized cases
          if (0> view.Left) {
            if (0<pos.X && 0<crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.W].CrossDistrictViewing(crossdistrict_ok),new Rectangle((short)(map.Width+view.Left),view.Top, (short)(-view.Left),view.Height));
              foreach(Point pt in tmp) ret.Add(new Point((short)(pt.X-map.Width),pt.Y));
            }
            view.Width += view.Left;
            view.X = 0;
          }
          if (map.Width < view.Right) {
            var new_width = map.Width;
            new_width -= view.Left;
            if (World.Get.Size>pos.X+1 && 0<crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.E].CrossDistrictViewing(crossdistrict_ok),new Rectangle(0,view.Top, (short)(view.Width-new_width),view.Height));
              foreach(Point pt in tmp) ret.Add(new Point((short)(pt.X+map.Width),pt.Y));
            }
            view.Width = new_width;
          }
          if (0 > view.Top) {
            if (0<pos.Y && 0<crossdistrict_ok && 3!= crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.N].CrossDistrictViewing(crossdistrict_ok),new Rectangle(view.Left, (short)(map.Height+view.Top),view.Width, (short)(-view.Top)));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X, (short)(pt.Y-map.Height)));
            }
            view.Height += view.Top;
            view.Y = 0;
          }
          if (map.Height < view.Bottom) {
            var new_height = map.Height;
            new_height -= view.Top;
            if (World.Get.Size>pos.Y+1 && 0<crossdistrict_ok && 3 != crossdistrict_ok) {
              HashSet<Point> tmp = In(World.Get[pos + Direction.S].CrossDistrictViewing(crossdistrict_ok),new Rectangle(view.Left,0,view.Width, (short)(view.Height-new_height)));
              foreach(Point pt in tmp) ret.Add(new Point(pt.X, (short)(pt.Y+map.Height)));
            }
            view.Height = new_height;
          }

          var tmp2 = Range(map);
          if (null == tmp2) return ret;
          HashSet<Point> stage = new(tmp2);
          if (0 >= ret.Count) ret = stage;
          else ret.UnionWith(stage);
		  return ret;
      }

      public List<Location>? All() {
        List<Location> ret = new();
        void encode(Map m, Point pt) {
            Location loc = new(m,pt);
            if (Map.Canonical(ref loc) && !ret.Contains(loc)) ret.Add(loc);
        };
        ForAll(encode);
        return 0 < ret.Count ? ret : null;
      }


      public void Record(Map m, in Point pt)
      {
        if (!m.GetTileModelAt(pt).IsWalkable) return; // reject unwalkable tiles
        Add(m, pt);
      }
      public void Record(in Location loc) => Record(loc.Map, loc.Position);

      // assume loc is in canonical form
      public void Seen(in Location loc) => Remove(loc.Map, loc.Position);

      public void Seen(Location[] locs) {
        // assume all of these are in canonical form
        foreach(var loc in locs) Remove(loc.Map, loc.Position);
      }

      // duplicate signature rather than risk indirection through IEnumerable; the Location[] signature is on a profile-hot path 2020-09-27 zaimoni
      public void Seen(List<Location> locs) {
        // assume all of these are in canonical form
        foreach(var loc in locs) Remove(loc.Map, loc.Position);
      }

    }

    [Serializable]
    public sealed class LocationFunction<T>
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

            public delete_from(IEnumerable<Location> trigger, Location target, LocationFunction<T> host)
            {
                triggers = new List<Location>(trigger);
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

       // \todo these two may need auto-merge capability (value-equal triggers/host allows merging targets)
       public void Relink(IEnumerable<Location> triggers, Location target) {
            (_handlers ??= new List<delete_from>()).Add(new delete_from(triggers, target, this));
       }

       public void Relink(IEnumerable<Location> triggers, IEnumerable<Location> targets) {
            (_handlers ??= new List<delete_from>()).Add(new delete_from(triggers, targets, this));
       }

#nullable enable
       public Dictionary<Location, T> Within(ZoneLoc src)
       {
            var ret = new Dictionary<Location, T>();
            var exits = src.Exits;
            lock (_locs) {
                foreach (var x in _locs) {
                    foreach (var y in x.Value) {
                        var loc = new Location(x.Key, y.Key);  // can't do merge here without unwanted constraints
                        if (src.Contains(loc)) ret.Add(loc, y.Value);
                        else if (0 <= Array.IndexOf(exits, loc)) {
                            ret.Add(loc, y.Value);
#if DEBUG
                            var cz = loc.ClearableZone;
//                          var ez = src.Exit_zones;
                            var ez = src.ExitZones;
                            if (null != cz && 0 > Array.IndexOf(ez, cz)) throw new InvalidOperationException("asymmetric accessibility of zones");
                            var za = loc.TrivialDistanceZones;
                            if (0 > Array.IndexOf(za, src)) throw new InvalidOperationException("asymmetric accessibility of zones");
#endif
                        }
                    }
                }
            }
            return ret;
       }

       public List<Dictionary<Location, T>> Within(IEnumerable<ZoneLoc> src)
       {
            var ret = new List<Dictionary<Location, T>>();
            foreach (var z in src) ret.Add(Within(z));
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
#nullable restore
    }
}
