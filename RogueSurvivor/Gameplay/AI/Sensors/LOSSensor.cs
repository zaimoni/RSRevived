// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.LOSSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using Sensor = djack.RogueSurvivor.Engine.AI.Sensor;
using LOS = djack.RogueSurvivor.Engine.LOS;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
  [Serializable]
  internal class LOSSensor : Sensor
  {
    private Actor m_Actor;
    private readonly SensingFilter Filters;
    // Actor caches for AI purposes
    private Dictionary<Location, Actor> _friends;
    private Dictionary<Location, Actor> _enemies;
    private Dictionary<Location,Inventory> _items;

    public HashSet<Point> FOV { get { return LOS.ComputeFOVFor(m_Actor); } }
    public Dictionary<Location,Actor> friends { get { return _friends; } } // reference-return
    public Dictionary<Location, Actor> enemies { get { return _enemies; } } // reference-return
    public Dictionary<Location, Inventory> items { get { return _items; } } // reference-return

    public LOSSensor(SensingFilter filters)
    {
      Filters = filters;
    }

    public void OwnedBy(Actor actor)
    {
      m_Actor = actor;
    }

    private void _seeActors(List<Percept> perceptList, Location[] normalized_FOV)
    {
      _enemies = null;
      _friends = null;
      foreach (var loc in normalized_FOV) {
        Actor actorAt = loc.Actor;
        if (null==actorAt) continue;
        if (actorAt==m_Actor) continue;
        if (actorAt.IsDead) continue;
        perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
        if (m_Actor.IsEnemyOf(actorAt)) (_enemies ?? (_enemies = new Dictionary<Location, Actor>()))[loc] = actorAt;
        else (_friends ?? (_friends = new Dictionary<Location, Actor>()))[loc] = actorAt;
      }
    }

    private void _seeActors(List<Percept> perceptList, Location[] normalized_FOV, ThreatTracking threats)
    {
        _enemies = null;
        _friends = null;
        HashSet<Point> has_threat = new HashSet<Point>();
        foreach (var loc in normalized_FOV) {
          Actor actorAt = loc.Actor;
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
            (_enemies ?? (_enemies = new Dictionary<Location, Actor>()))[loc] = actorAt;
            has_threat.Add(test.Value.Position);
          } else (_friends ?? (_friends = new Dictionary<Location, Actor>()))[loc] = actorAt;
        }
        // ensure fact what is in sight is current, is recorded
		threats.Cleared(m_Actor.Location.Map,FOV.Except(has_threat));
    }

    private void _seeItems(List<Percept> perceptList, Location[] normalized_FOV)
    {
      _items = null;
      foreach (var loc in normalized_FOV) {
        var itemsAt = loc.Items;
        if (null==itemsAt || itemsAt.IsEmpty) continue;
        perceptList.Add(new Percept(itemsAt, m_Actor.Location.Map.LocalTime.TurnCounter, in loc));
        (_items ?? (_items = new Dictionary<Location, Inventory>()))[loc] = itemsAt;
      }
    }

    private void _seeItems(List<Percept> perceptList, Location[] normalized_FOV, Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items)
    {
      _items = null;
      foreach (var loc in normalized_FOV) {
        var itemsAt = loc.Items;
        if (null== itemsAt || itemsAt.IsEmpty) {
          items.Set(loc,null,loc.Map.LocalTime.TurnCounter);
          continue;
        }
        perceptList.Add(new Percept(itemsAt, m_Actor.Location.Map.LocalTime.TurnCounter, in loc));
        items.Set(loc, new HashSet<Gameplay.GameItems.IDs>(itemsAt.Items.Select(x => x.Model.ID)), loc.Map.LocalTime.TurnCounter);
        (_items ?? (_items = new Dictionary<Location, Inventory>()))[loc] = itemsAt;
      }
    }

    public List<Percept> Sense(Actor actor)
    {
      m_Actor = actor;
      var _view_map = m_Actor.Location.Map;
      HashSet<Point> m_FOV = FOV;
      actor.InterestingLocs?.Seen(actor.Location.Map,m_FOV);    // will have seen everything; note this
      var e = m_Actor.Location.Exit;
      var normalized_FOV = new Location[m_FOV.Count+(null == e ? 0 : 1)];
      {
      int i = 0;
      foreach(var pt in m_FOV) {
        var loc = new Location(_view_map, pt);
        if (!Map.Canonical(ref loc)) throw new InvalidOperationException("FOV coordinates should be denormalized-legal");
        normalized_FOV[i++] = loc;
      }
      if (null != e) {
        normalized_FOV[i] = e.Location;
        actor.InterestingLocs?.Seen(e.Location);
      }
      }
      List<Percept> perceptList = new List<Percept>();
      if ((Filters & SensingFilter.ACTORS) != SensingFilter.NONE) {
        ThreatTracking threats = actor.Threats;
        if (null != threats) _seeActors(perceptList, normalized_FOV, threats);
        else _seeActors(perceptList, normalized_FOV);
      }
      if ((Filters & SensingFilter.ITEMS) != SensingFilter.NONE) {
        Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items = actor.Controller.ItemMemory;
        if (null != items) _seeItems(perceptList, normalized_FOV, items);
        else _seeItems(perceptList, normalized_FOV);
      }
      if ((Filters & SensingFilter.CORPSES) != SensingFilter.NONE) {
        foreach (var loc in normalized_FOV) {
          List<Corpse> corpsesAt = loc.Map.GetCorpsesAt(loc.Position);
          if (corpsesAt != null) perceptList.Add(new Percept(corpsesAt, actor.Location.Map.LocalTime.TurnCounter, in loc));
        }
      }
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
