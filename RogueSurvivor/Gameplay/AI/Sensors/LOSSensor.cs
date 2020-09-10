// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.LOSSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public Actor Viewpoint { get { return m_Actor; } }
    public HashSet<Point> FOV { get { return LOS.ComputeFOVFor(m_Actor); } }
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
#nullable restore

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
        if (m_Actor.IsEnemyOf(actorAt)) (_enemies ?? (_enemies = new Dictionary<Location, Actor>())).Add(loc, actorAt);
        else (_friends ?? (_friends = new Dictionary<Location, Actor>())).Add(loc, actorAt);
      }
    }

    private void _seeActors(List<Percept> perceptList, Location[] normalized_FOV, ThreatTracking threats)
    {
        _enemies = null;
        _friends = null;
        HashSet<Point> has_threat = new HashSet<Point>();   // XXX Span<Point> will not convert to IEnumerable<Point>
        foreach (var loc in normalized_FOV) {
          var actorAt = loc.Actor;
          var test = m_Actor.Location.Map.Denormalize(in loc);
          if (   null==actorAt
              || actorAt== m_Actor
              || actorAt.IsDead) {
            if (null == test) threats.Cleared(loc.Map,new Point[1] { loc.Position });
            continue;
          }
          perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
          bool is_enemy = m_Actor.IsEnemyOf(actorAt);
          if (is_enemy) threats.Sighted(actorAt, actorAt.Location);
          if (null == test) {
            if (!is_enemy) threats.Cleared(loc.Map,new Point[1] { loc.Position });
            continue;
          }
          if (is_enemy) {
            (_enemies ?? (_enemies = new Dictionary<Location, Actor>())).Add(loc, actorAt);
            has_threat.Add(test.Value.Position);
          } else (_friends ?? (_friends = new Dictionary<Location, Actor>())).Add(loc, actorAt);
        }
        // ensure fact what is in sight is current, is recorded
		threats.Cleared(m_Actor.Location.Map,FOV.Except(has_threat));
    }

    private void _seeItems(List<Percept> perceptList, Location[] normalized_FOV)
    {
      _items = null;
      foreach (var loc in normalized_FOV) {
        var allItems = Map.AllItemsAt(loc, m_Actor);
        if (null == allItems) continue;
        foreach(var inv in allItems) {
          perceptList.Add(new Percept(inv, m_Actor.Location.Map.LocalTime.TurnCounter, in loc)); // \todo fix this
          (_items ?? (_items = new Dictionary<Location, Inventory>()))[loc] = inv; // \todo may have to retype this
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
          (_items ?? (_items = new Dictionary<Location, Inventory>()))[loc] = inv; // \todo may have to retype this
        }
        items.Set(loc, staging, loc.Map.LocalTime.TurnCounter);
      }
    }

    private void _seeCorpses(List<Percept> perceptList, Location[] normalized_FOV)
    {
      var turn = m_Actor.Location.Map.LocalTime.TurnCounter;
      foreach (var loc in normalized_FOV) {
        var corpsesAt = loc.Map.GetCorpsesAt(loc.Position);
        if (corpsesAt != null) perceptList.Add(new Percept(corpsesAt, turn, in loc));
      }
    }

#nullable enable
    private Action<List<Percept>, Location[]> HowToSense()
    {
      Action<List<Percept>, Location[]>? ret = null;
      if ((Filters & SensingFilter.ACTORS) != SensingFilter.NONE) {
        var threats = m_Actor.Threats;
        ret = (null != threats) ? threats.Bind<List<Percept>, Location[], ThreatTracking>(_seeActors)
                                : _seeActors;
      }
      if ((Filters & SensingFilter.ITEMS) != SensingFilter.NONE) {
        var items = m_Actor.Controller.ItemMemory;
        ret = ret.Compose(null != items ? items.Bind<List<Percept>, Location[], Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>>(_seeItems)
                                        : _seeItems);
      }
      if ((Filters & SensingFilter.CORPSES) != SensingFilter.NONE) ret = ret.Compose(_seeCorpses);
      if (null == ret) throw new ArgumentNullException(nameof(ret));
      return ret;
    }

    public List<Percept> Sense()
    {
      var actor = Viewpoint;
      var _view_map = actor.Location.Map;
      HashSet<Point> m_FOV = FOV;
      actor.InterestingLocs?.Seen(_view_map, m_FOV);    // will have seen everything; note this
      var e = actor.Location.Exit;
      var normalized_FOV = new Location[m_FOV.Count+(null == e ? 0 : 1)];
      {
      int i = 0;
      foreach(var pt in m_FOV) {
        var loc = new Location(_view_map, pt);
        if (!Map.Canonical(ref loc)) throw new InvalidOperationException("FOV coordinates should be denormalized-legal");
        normalized_FOV[i++] = loc;
      }
      if (null != e) {
        normalized_FOV[i] = e.Location; // chained value here isn't C++-legal so have to do this in two steps
        actor.InterestingLocs?.Seen(e.Location);
      }
      }
      List<Percept> perceptList = new List<Percept>();
      (_sense ?? (_sense = HowToSense()))(perceptList, normalized_FOV);
      return perceptList;
    }
#nullable restore

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
