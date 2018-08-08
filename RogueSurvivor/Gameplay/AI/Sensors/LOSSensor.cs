// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.LOSSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

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
    // Actor caches for AI pruposes
    private Map _view_map;
    private Dictionary<Point,Actor> _friends;
    private Dictionary<Point,Actor> _enemies;


    public HashSet<Point> FOV { get { return LOS.ComputeFOVFor(m_Actor); } }
    public Dictionary<Point,Actor> friends { get { return _friends; } } // reference-return
    public Dictionary<Point, Actor> enemies { get { return _enemies; } } // reference-return

    public LOSSensor(SensingFilter filters)
    {
      Filters = filters;
    }

    public void OwnedBy(Actor actor)
    {
      m_Actor = actor;
    }

    private void _seeActors(List<Percept> perceptList)
    {
      _enemies = null;
      _friends = null;
      foreach (Point pt in FOV) {
        Actor actorAt = m_Actor.Location.Map.GetActorAtExt(pt); // XXX change target for cross-district vision
        if (null==actorAt) continue;
        if (actorAt==m_Actor) continue;
        if (actorAt.IsDead) continue;
        perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
        if (m_Actor.IsEnemyOf(actorAt)) (_enemies ?? (_enemies = new Dictionary<Point,Actor>()))[pt] = actorAt;
        else (_friends ?? (_friends = new Dictionary<Point,Actor>()))[pt] = actorAt;
      }
    }

    private void _seeActors(List<Percept> perceptList, ThreatTracking threats)
    {
        _enemies = null;
        _friends = null;
        HashSet<Point> has_threat = new HashSet<Point>();
        foreach (Point pt in FOV) {
          Actor actorAt = m_Actor.Location.Map.GetActorAtExt(pt); // XXX change target for cross-district vision
          if (null==actorAt) continue;
          if (actorAt== m_Actor) continue;
          if (actorAt.IsDead) continue;
          perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
          if (m_Actor.IsEnemyOf(actorAt)) {
            (_enemies ?? (_enemies = new Dictionary<Point,Actor>()))[pt] = actorAt;
            threats.Sighted(actorAt, actorAt.Location); // XXX change target for cross-district vision
            has_threat.Add(pt);
          } else (_friends ?? (_friends = new Dictionary<Point,Actor>()))[pt] = actorAt;
        }
        // ensure fact what is in sight is current, is recorded
		threats.Cleared(m_Actor.Location.Map,FOV.Except(has_threat));
    }

    private void _seeItems(List<Percept> perceptList)
    {
      foreach (Point pt in FOV) {
        Location tmp = new Location(m_Actor.Location.Map,pt);
        if (!tmp.Map.IsInBounds(pt)) {
          Location? test = m_Actor.Location.Map.Normalize(pt);
          if (null == test) continue;
          tmp = test.Value;
        }
        Inventory itemsAt = tmp.Map.GetItemsAt(tmp.Position);
        if (null==itemsAt) continue;
        perceptList.Add(new Percept(itemsAt, m_Actor.Location.Map.LocalTime.TurnCounter, tmp));
      }
    }

    private void _seeItems(List<Percept> perceptList, Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items)
    {
      foreach (Point pt in FOV) {
        Location tmp = new Location(m_Actor.Location.Map,pt);
        if (!tmp.Map.IsInBounds(pt)) {
          Location? test = m_Actor.Location.Map.Normalize(pt);
          if (null == test) continue;
          tmp = test.Value;
        }
        Inventory itemsAt = tmp.Map.GetItemsAt(tmp.Position);
        if (null==itemsAt) {
          items.Set(tmp,null,tmp.Map.LocalTime.TurnCounter);
          continue;
        }
        perceptList.Add(new Percept(itemsAt, m_Actor.Location.Map.LocalTime.TurnCounter, tmp));
        items.Set(tmp, new HashSet<Gameplay.GameItems.IDs>(itemsAt.Items.Select(x => x.Model.ID)), tmp.Map.LocalTime.TurnCounter);
      }
    }

    public List<Percept> Sense(Actor actor)
    {
      m_Actor = actor;
      _view_map = m_Actor.Location.Map;
      HashSet<Point> m_FOV = FOV;
      actor.InterestingLocs?.Seen(actor.Location.Map,m_FOV);
      List<Percept> perceptList = new List<Percept>();
      if ((Filters & SensingFilter.ACTORS) != SensingFilter.NONE) {
        ThreatTracking threats = actor.Threats;
        if (null != threats) _seeActors(perceptList,threats);
        else _seeActors(perceptList);
      }
      if ((Filters & SensingFilter.ITEMS) != SensingFilter.NONE) {
        Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items = actor.Controller.ItemMemory;
        if (null != items) _seeItems(perceptList, items);
        else _seeItems(perceptList);
      }
      if ((Filters & SensingFilter.CORPSES) != SensingFilter.NONE) {
        foreach (Point position in m_FOV) {
          List<Corpse> corpsesAt = actor.Location.Map.GetCorpsesAtExt(position.X, position.Y);
          if (corpsesAt != null)
            perceptList.Add(new Percept((object) corpsesAt, actor.Location.Map.LocalTime.TurnCounter, new Location(actor.Location.Map, position)));
        }
      }
      // now have seen everything; note this
      actor.InterestingLocs?.Seen(actor.Location.Map, m_FOV);
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
