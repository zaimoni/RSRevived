﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Zone
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zaimoni.Data;
using static Zaimoni.Data.Compass;

using UpdateMoveDelta = djack.RogueSurvivor.Engine.Actions.UpdateMoveDelta;
using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
using Size = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Data
{
#nullable enable
  [Serializable]
  internal struct AVtable // attribute-value table
  {
    private Dictionary<string, object>? m_Attributes;

    public bool HasKey(string key) { return m_Attributes?.ContainsKey(key) ?? false; }

    public void Set<_T_>(string key, [DisallowNull] _T_ value)
    {
#if DEBUG
      if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
#endif
      (m_Attributes ??= new Dictionary<string, object>(1))[key] = value;
    }

    [return: MaybeNull]
    public _T_ Get<_T_>(string key)
    {
#if DEBUG
      if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
#endif
      if (m_Attributes == null) return default;
      if (!m_Attributes.TryGetValue(key, out var obj)) return default;
      if (obj is _T_ x) return x;
      throw new InvalidOperationException("game attribute is not of requested type");
    }

    public void Unset(string key)
    {
      if (null != m_Attributes) {
        m_Attributes.Remove(key);
        if (0 >= m_Attributes.Count) m_Attributes = null;
      }
    }
  }
#nullable restore

  // not meant to be self-contained
  [Serializable]
  internal class Zone
  {
    private readonly string m_Name = "unnamed zone";
    private Rectangle m_Bounds; // assumed to be fully in bounds of the underlying map
    // while zone attributes have great potential, RS Alpha 9 underwhelms in its use of them.
    public AVtable Attribute;
    [NonSerialized] public AVtable VolatileAttribute;

    public string Name { get { return m_Name; } }
    public Rectangle Bounds { get { return m_Bounds; } }

    public Zone(string name, Rectangle bounds)
    {
#if DEBUG
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
#endif
      m_Name = name;
      m_Bounds = bounds;
    }

    public void InstallExits(ZoneLoc host) {
#if DEBUG
        if (host.Rect != Bounds) throw new InvalidOperationException("untrusted setup");
#endif
        if (VolatileAttribute.HasKey("exit_zones")) return;
        var prior_exits = VolatileAttribute.Get<Location[]>("exits");
        if (null == prior_exits) {
          var locs = new HashSet<Location>();
          var walking = host.WalkOut();
          if (null != walking) locs.UnionWith(walking.Select(act => act.dest));
          var vertical = host.grep(loc => null != loc.Exit);
          if (null != vertical) locs.UnionWith(vertical.Select(loc => loc.Exit!.Location));
#if DEBUG
          if (0 >= locs.Count) throw new InvalidOperationException("zone w/o exits");
#endif
          prior_exits = locs.ToArray();
          VolatileAttribute.Set("exits", prior_exits);
        }

        var zones = new Dictionary<Map,HashSet<Zone>>(); // using default pointer-equality, so duplicate coordinates aren't deduplicated
        foreach(var loc in prior_exits) {
          var dest_zones = loc.Map.GetZonesAt(loc.Position);
          if (null != dest_zones) {
            if (!zones.TryGetValue(loc.Map, out var cache)) zones.Add(loc.Map,(cache = new HashSet<Zone>()));
            cache.UnionWith(dest_zones);
          }
        }
        var ordered_zones = new List<ZoneLoc>();
        var order_staging = new Dictionary<int, List<Zone>>();
        foreach(var mapzone in zones) {
          foreach(var x in mapzone.Value) {
            var area = (x.Bounds.Right-x.Bounds.Left)*(x.Bounds.Bottom-x.Bounds.Top);
            if (!order_staging.TryGetValue(area, out var cache)) order_staging.Add(area, (cache = new List<Zone>()));
            cache.Add(x);
          }
          while(0 < order_staging.Count) {
            int index = order_staging.Keys.Max();
            foreach(var x in order_staging[index]) ordered_zones.Add(new ZoneLoc(mapzone.Key, x));
            order_staging.Remove(index);
          }
        }
        VolatileAttribute.Set("exit_zones", ordered_zones.ToArray());
    }
  }

  [Serializable]    // just in case
  internal record ZoneLoc : IMap
  {
    public readonly Map m;
    public readonly Rectangle Rect; // doesn't have to be normalized
    [NonSerialized] private Zone? z;

    public ZoneLoc(Map _m, Rectangle _r)
    {
      m = _m;
      Rect = _r;
    }

    public ZoneLoc(Map _m, Zone _z)
    {
      m = _m;
      Rect = _z.Bounds;
      z = _z;
    }

#nullable enable
    public Location[]? Exits { get {
        if (null == Zone) return null;
        var ret = z.VolatileAttribute.Get<Location[]>("exits");
        if (null != ret) return ret;
        z.InstallExits(this);
        return z.VolatileAttribute.Get<Location[]>("exits");
    } }

    public ZoneLoc[]? Exit_zones { get {
        if (null == Zone) return null;
        var ret = z.VolatileAttribute.Get<ZoneLoc[]>("exit_zones");
        if (null != ret) return ret;
        z.InstallExits(this);
        ret = z.VolatileAttribute.Get<ZoneLoc[]>("exit_zones");
        foreach(var zone in ret) if (0 > Array.IndexOf(zone.Exit_zones, this)) zone.Zone.VolatileAttribute.Unset("exit_zones");
        return ret;
    } }

    public KeyValuePair<Location[]?, ZoneLoc[]?> ExitData { get {
        if (null == Zone) return default;
        var e_zones = Exit_zones; // need this to run first
        return new KeyValuePair<Location[]?, ZoneLoc[]?>(z.VolatileAttribute.Get<Location[]>("exits"), e_zones);
    } }
#nullable restore

#region IMap implementation
    public short Height { get { return Rect.Height; } }
    public short Width { get { return Rect.Width; } }
    public Point Origin { get { return Rect.Location; } }
    public string MapName { get {
      var center = Rect.Location + Rect.Size/2;
      var view = GetActorAt(center);
      if (null != view && view.IsPlayer) return "navigation: "+view.Name; // anticipated use case is DaimonMap
      return m.Name;
    } }
    public bool HasExitAt(in Point pt) { return m.HasExitAtExt(pt); }
    public Actor? GetActorAt(Point pt) { return m.GetActorAtExt(pt); }
    public Inventory? GetItemsAt(Point pt) { return m.GetItemsAtExt(pt); }
    public MapObject? GetMapObjectAt(Point pt) { return m.GetMapObjectAtExt(pt); }
    public TileModel? GetTileModelAt(Point pt) { return m.GetTileModelAtExt(pt); }

    // for now, bluff these two -- relies on canonicalization
    public int TurnOrderFor(Actor a) {
      var zones = GetCanonical;
      if (null == zones) return m.TurnOrderFor(a);
      return -1;
    }
    public Dictionary<Point, List<Corpse>>? FilterCorpses(Predicate<Corpse> ok) {
      var zones = GetCanonical;
      if (null != zones) return null; // temporary
      var home_catalog = m.FilterCorpses(ok);
      if (null != home_catalog) {
        home_catalog.OnlyIf(pt => Contains(new Location(m, pt)));
        if (0 >= home_catalog.Count) return null;
      }
//    if (null != zones) { ... };
      return home_catalog;
    }
#endregion

    public List<Location>? Contains(IEnumerable<Location> src) {
        var ret = new List<Location>();
        foreach(var loc in src) if (Contains(in loc)) ret.Add(loc);
        return (0 < ret.Count) ? ret : null;
    }
    public bool Contains(in Location loc) { return m == loc.Map && Rect.Contains(loc.Position); }
    public bool ContainsExt(in Location loc) {
      if (m == loc.Map) return Rect.Contains(loc.Position);
      var test = m.Denormalize(in loc);
      return null!=test && Rect.Contains(test.Value.Position);
    }

    public void DoForEach(Action<Location> doFn) {
      Point point = new Point();
      for (point.X = Rect.Left; point.X < Rect.Right; ++point.X) {
        for (point.Y = Rect.Top; point.Y < Rect.Bottom; ++point.Y) {
          var loc = new Location(m,point);
          if (Map.Canonical(ref loc)) doFn(loc);
        }
      }
    }

    public bool Any(Predicate<Location> ok) {
      var ret = new List<Location>();
      Point point = new Point();
      for (point.X = Rect.Left; point.X < Rect.Right; ++point.X) {
        for (point.Y = Rect.Top; point.Y < Rect.Bottom; ++point.Y) {
          var loc = new Location(m,point);
          if (Map.Canonical(ref loc) && ok(loc)) return true;
        }
      }
      return false;
    }

    public List<Location>? grep(Predicate<Location> ok) {
      var ret = new List<Location>();
      Point point = new Point();
      for (point.X = Rect.Left; point.X < Rect.Right; ++point.X) {
        for (point.Y = Rect.Top; point.Y < Rect.Bottom; ++point.Y) {
          var loc = new Location(m,point);
          if (Map.Canonical(ref loc) && ok(loc)) ret.Add(loc);
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public Location Center { get { return new Location(m, Rect.Location + Rect.Size / 2); } }

#nullable enable
    public List<ZoneLoc>? GetCanonical {
        get {
            var code = District.UsesCrossDistrictView(m);
            if (0 >= code) return null; // denormalized locations don't exist anyway

            Span<bool> overflow = stackalloc bool[(int)reference.XCOM_STRICT_UB / 2];

            // \todo if we aren't even intersecting, compensate

            overflow[(int)XCOMlike.N / 2] = 0 > Rect.Top;
            overflow[(int)XCOMlike.E / 2] = m.Width < Rect.Right;
            overflow[(int)XCOMlike.S / 2] = m.Height < Rect.Bottom;
            overflow[(int)XCOMlike.W / 2] = 0 > Rect.Left;

            int overflow_ew = (overflow[(int)XCOMlike.E / 2] ? 2 : 0)+ (overflow[(int)XCOMlike.W / 2] ? 8 : 0);
            int overflow_ns = (overflow[(int)XCOMlike.N / 2] ? 1 : 0)+ (overflow[(int)XCOMlike.S / 2] ? 4 : 0);

            if (0 == overflow_ew && 0 == overflow_ns) return null; // within bounds

            // At this point, we effectively know our map is whole district-sized (and thus all adjacent maps, *if* they exist, have the same size

            Map?[] maps = new Map?[(int)reference.XCOM_EXT_STRICT_UB];
            maps[(int)reference.NEUTRAL] = m;

            int i = (int)reference.XCOM_STRICT_UB;
            while(0 <= --i) {
                var where = m.DistrictPos + Direction.COMPASS[i];
                maps[i] = Engine.Session.Get.World.At(where)?.CrossDistrictViewing(code);
            }

            Span<short> width_cuts = stackalloc short[4];
            if (overflow[(int)XCOMlike.W / 2]) {
                width_cuts[0] = (short)(m.Width + Rect.Left);
                width_cuts[1] = 0;
                if (null == maps[(int)XCOMlike.W] && null == maps[(int)XCOMlike.NW] && null == maps[(int)XCOMlike.SW]) {
                    overflow[(int)XCOMlike.W / 2] = false;
                    overflow_ew -= 8;
                }
            } else width_cuts[1] = Rect.Left;

            if (overflow[(int)XCOMlike.E / 2]) {
                width_cuts[2] = Rect.Right;
                width_cuts[3] = (short)(Rect.Right - m.Width);
                if (null == maps[(int)XCOMlike.E] && null == maps[(int)XCOMlike.NE] && null == maps[(int)XCOMlike.SE]) {
                    overflow[(int)XCOMlike.E / 2] = false;
                    overflow_ew -= 2;
                }
            } else width_cuts[2] = Rect.Right;
            if (0 == overflow_ew && 0 == overflow_ns) return null; // overflow contained no renormalizable coordinates

            Span<short> height_cuts = stackalloc short[4];
            if (overflow[(int)XCOMlike.N / 2]) {
                height_cuts[0] = (short)(m.Height + Rect.Top);
                height_cuts[1] = 0;
                if (null == maps[(int)XCOMlike.N] && null == maps[(int)XCOMlike.NW] && null == maps[(int)XCOMlike.NE]) {
                    overflow[(int)XCOMlike.N / 2] = false;
                    overflow_ew -= 1;
                }
            } else height_cuts[1] = Rect.Top;

            if (overflow[(int)XCOMlike.S / 2]) {
                height_cuts[2] = Rect.Bottom;
                height_cuts[3] = (short)(Rect.Bottom - m.Height);
                if (null == maps[(int)XCOMlike.S] && null == maps[(int)XCOMlike.SW] && null == maps[(int)XCOMlike.SE]) {
                    overflow[(int)XCOMlike.S / 2] = false;
                    overflow_ew -= 4;
                }
            } else height_cuts[2] = Rect.Bottom;
            if (0 == overflow_ew && 0 == overflow_ns) return null; // overflow contained no renormalizable coordinates

            // install the repaired zone for our map
            var ret = new List<ZoneLoc>() { new ZoneLoc(m, Rectangle.FromLTRB(width_cuts[1], height_cuts[1], width_cuts[2], height_cuts[2])) };

            // we'd like this within a countdown loop
            ZoneLoc? zone = null;
            List<ZoneLoc>? canon = null;
            if (null != maps[(int)XCOMlike.N] && overflow[(int)XCOMlike.N / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.N], Rectangle.FromLTRB(width_cuts[1], height_cuts[0], width_cuts[2], m.Height));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.NE] && overflow[(int)XCOMlike.N / 2] && overflow[(int)XCOMlike.E / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.NE], Rectangle.FromLTRB(0, height_cuts[0], width_cuts[3], m.Height));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.E] && overflow[(int)XCOMlike.E / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.E], Rectangle.FromLTRB(0, height_cuts[1], width_cuts[3], height_cuts[2]));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.SE] && overflow[(int)XCOMlike.S / 2] && overflow[(int)XCOMlike.E / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.SE], Rectangle.FromLTRB(0, 0, width_cuts[3], height_cuts[3]));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.S] && overflow[(int)XCOMlike.S / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.S], Rectangle.FromLTRB(width_cuts[1], 0, width_cuts[2], height_cuts[3]));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.SW] && overflow[(int)XCOMlike.S / 2] && overflow[(int)XCOMlike.W / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.SW], Rectangle.FromLTRB(width_cuts[0], 0, m.Width, height_cuts[3]));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.W] && overflow[(int)XCOMlike.W / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.W], Rectangle.FromLTRB(width_cuts[0], height_cuts[1], m.Width, height_cuts[2]));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            if (null != maps[(int)XCOMlike.NW] && overflow[(int)XCOMlike.N / 2] && overflow[(int)XCOMlike.W / 2]) {
                zone = new ZoneLoc(maps[(int)XCOMlike.NW], Rectangle.FromLTRB(width_cuts[0], height_cuts[0], m.Width, m.Height));
                canon = zone.GetCanonical;
                if (null == canon) ret.Add(zone);
                else ret.AddRange(canon);
            }
            return ret;
        }
    }

    public Zone? Zone {
      get {
        return z ??= m.GetZonesAt(Rect.Location)?.Find(z => z.Bounds == Rect);
      }
    }

    public bool IsClearable { get {
        return m.IsClearableZone(this);
    } }

    public ZoneLoc[] ExitZones { get {
        var data = ExitData;
        var staging = new List<ZoneLoc>(Array.Exists(data.Value, zone => !zone.IsClearable) ? Array.FindAll(data.Value, zone => zone.IsClearable) : data.Value);
        foreach (var loc in data.Key) {
            var test = loc.ClearableZones;
            if (null != test) foreach (var z2 in test) if (this != z2 && !staging.Contains(z2)) staging.Add(z2);
        }
        return staging.ToArray();
    } }
#nullable restore

    public List<UpdateMoveDelta>? WalkOut() {
      var ret = new List<UpdateMoveDelta>();
      Rect.DoForEachOnEdge(pt => {
          var test = UpdateMoveDelta.fromOrigin(new Location(m, pt), loc => !Contains(loc));
          if (null != test) ret.AddRange(test);
      });
      return 0<ret.Count ? ret : null;
    }

    public Rectangle DistrictSpan { get {
      var ret = new Rectangle(m.DistrictPos, new Size(1, 1));
      if (0 < District.UsesCrossDistrictView(m)) {
        int test = Rect.Left;
        int delta = m.Width;
        while (0 > test) {
          test += delta;
          ret.X -= 1;
          ret.Width += 1;
        }
        test = Rect.Right;
        while (delta <= test) {
          test -= delta;
          ret.Width += 1;
        }
        test = Rect.Top;
        delta = m.Height;
        while (0 > test) {
          test += delta;
          ret.Y -= 1;
          ret.Height += 1;
        }
        test = Rect.Bottom;
        while (delta <= test) {
          test -= delta;
          ret.Height += 1;
        }
      }
      return ret;
    } }
  }

#if CTHORPE_BROKEN_GENERICS
  internal class ZoneSpan<T> where T:IEnumerable<Point>
  {
    public readonly Map m;
    public readonly T pts; // doesn't have to be normalized

    public ZoneSpan(Map _m, T _pts)
    {
      m = _m;
      pts = _pts;
    }

    // 2020-03-15: just because T is required to be IEnumerable<Point> doesn't make IEnumerable functions available
    public bool Contains(in Location loc) { return m == loc.Map && pts.Contains(loc.Position); }
    public bool ContainsExt(in Location loc) {
      if (m == loc.Map) return pts.Contains(loc.Position);
      var test = m.Denormalize(in loc);
      return null!=test && pts.Contains(test.Value.Position);
    }

    public void DoForEach(Action<Location> doFn) {
      foreach(var pt in pts) {
        var loc = new Location(m,pt);
        if (Map.Canonical(ref loc)) doFn(loc);
      }
    }
  }
#endif
}
