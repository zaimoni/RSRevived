// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.LOSSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using Sensor = djack.RogueSurvivor.Engine.AI.Sensor;
using LOS = djack.RogueSurvivor.Engine.LOS;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
  [Serializable]
  internal class LOSSensor : Sensor
  {
#nullable enable
    private Actor m_Actor;
    private readonly SensingFilter Filters;
    // Actor caches for AI purposes
    private Dictionary<Location, Actor>? _friends;
    private Dictionary<Location, Actor>? _enemies;
    private Dictionary<Location,Inventory>? _items;
    [NonSerialized] private Action<List<Percept>, Location[]>? _sense;
    [NonSerialized] private Location[]? _normalized_FOV;

    public Actor Viewpoint { get { return m_Actor; } }
    public HashSet<Point> FOV { get { return LOS.ComputeFOVFor(m_Actor); } }
    public Location[] FOVloc { get {
      return _normalized_FOV ?? (_normalized_FOV = _buildNormalizedFOV());
    } }
    public Dictionary<Location,Actor>? friends { get { return _friends; } } // reference-return
    public Dictionary<Location, Actor>? enemies { get { return _enemies; } } // reference-return
    public Dictionary<Location, Inventory>? items { get { return _items; } } // reference-return

    public LOSSensor(SensingFilter filters, Actor actor)
    {
#if DEBUG
      if (filters == SensingFilter.NONE) throw new ArgumentNullException(nameof(filters));
#endif
      m_Actor = actor;
      Filters = filters;
    }

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      if (null != m_Actor && m_Actor.IsDead) {
        _friends = null;
        _enemies = null;
        _items = null;
      }
#if DEBUG
      if (null != _items) foreach(var x in _items) x.Value.RepairZeroQty();
#endif
    }

    private void _seeActors(List<Percept> perceptList, Location[] normalized_FOV)
    {
      _enemies = null;
      _friends = null;
      foreach (var loc in normalized_FOV) {
        var actorAt = loc.Actor;
        if (null==actorAt) continue;
        if (actorAt==m_Actor) continue;
        if (actorAt.IsDead) continue;
        perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
        if (m_Actor.IsEnemyOf(actorAt)) (_enemies ??= new Dictionary<Location, Actor>()).Add(loc, actorAt);
        else (_friends ??= new Dictionary<Location, Actor>()).Add(loc, actorAt);
      }
    }

#nullable restore

    private void _seeItems(List<Percept> perceptList, Location[] normalized_FOV)
    {
      _items = null;
      foreach (var loc in normalized_FOV) {
        var allItems = Map.AllItemsAt(loc, m_Actor);
        if (null == allItems) continue;
        foreach(var inv in allItems) {
          perceptList.Add(new Percept(inv, m_Actor.Location.Map.LocalTime.TurnCounter, in loc)); // \todo fix this
          (_items ??= new Dictionary<Location, Inventory>())[loc] = inv; // \todo may have to retype this
        }
      }
    }

    private void _seeItems(List<Percept> perceptList, Location[] normalized_FOV, Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items)
    {
      _items = null;
      foreach (var loc in normalized_FOV) {
        var allItems = Map.AllItemsAt(loc, m_Actor);
        if (null == allItems) {
          items.Set(loc,null,loc.Map.LocalTime.TurnCounter);
          continue;
        }
        HashSet<Gameplay.GameItems.IDs>? staging = new HashSet<Gameplay.GameItems.IDs>();
        foreach(var inv in allItems) {
          staging.UnionWith(inv.Items.Select(x => x.Model.ID));
          perceptList.Add(new Percept(inv, m_Actor.Location.Map.LocalTime.TurnCounter, in loc)); // \todo fix this
          (_items ??= new Dictionary<Location, Inventory>())[loc] = inv; // \todo may have to retype this
        }
        items.Set(loc, staging, loc.Map.LocalTime.TurnCounter);
      }
    }

    private void _seeCorpses(List<Percept> perceptList, Location[] normalized_FOV)
    {
      var turn = m_Actor.Location.Map.LocalTime.TurnCounter;
      foreach (var loc in normalized_FOV) {
        var corpsesAt = loc.Corpses;
        if (null != corpsesAt) perceptList.Add(new Percept(corpsesAt, turn, in loc));
      }
    }

#nullable enable
    private Action<List<Percept>, Location[]> HowToSense()
    {
      Action<List<Percept>, Location[]>? ret = null;
      if ((Filters & SensingFilter.ACTORS) != SensingFilter.NONE) ret = _seeActors;
      if ((Filters & SensingFilter.ITEMS) != SensingFilter.NONE) {
        var items = m_Actor.Controller.ItemMemory;
        ret = ret.Compose(null != items ? items.Bind<List<Percept>, Location[], Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>>(_seeItems)
                                        : _seeItems);
      }
      if ((Filters & SensingFilter.CORPSES) != SensingFilter.NONE) ret = ret.Compose(_seeCorpses);
      if (null == ret) throw new ArgumentNullException(nameof(ret));
      return ret;
    }

    private Location[] _buildNormalizedFOV()
    {
      var actor = Viewpoint;
      var _view_map = actor.Location.Map;
      HashSet<Point> m_FOV = FOV;
      var e = actor.Location.Exit;
      var normalized_FOV = new Location[m_FOV.Count+(null == e ? 0 : 1)];
      {
      int i = 0;
      foreach(var pt in m_FOV) {
        var loc = new Location(_view_map, pt);
        if (!Map.Canonical(ref loc)) throw new InvalidOperationException("FOV coordinates should be denormalized-legal");
        normalized_FOV[i++] = loc;
      }
      if (null != e) normalized_FOV[i] = e.Location;
      }
      return normalized_FOV;
    }

    public List<Percept> Sense()
    {
      _normalized_FOV = _buildNormalizedFOV(); // \todo stop thrashing GC (some sort of pooling)
      List<Percept> perceptList = new List<Percept>();
      (_sense ?? (_sense = HowToSense()))(perceptList, _normalized_FOV);
      Viewpoint.Controller.eventFOV(); // trigger additional vision processing; rely on z processing being so trivial that their CPU wastage is negligible
      return perceptList;
    }

    [System.Flags]
    public enum SensingFilter
    {
      NONE = 0,
      ACTORS = 1,
      ITEMS = 2,
      CORPSES = 4,
    }
  }
}
