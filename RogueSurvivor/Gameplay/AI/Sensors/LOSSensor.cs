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
    private readonly SensingFilter m_Filters;

    public HashSet<Point> FOV { get { return LOS.ComputeFOVFor(m_Actor); } }
    public SensingFilter Filters { get { return m_Filters; } }

    public LOSSensor(SensingFilter filters)
    {
      m_Filters = filters;
    }

    public void OwnedBy(Actor actor)
    {
      m_Actor = actor;
    }

    private void _seeActors(List<Percept> perceptList)
    {
      foreach (Point pt in FOV) {
        Actor actorAt = m_Actor.Location.Map.GetActorAt(pt); // XXX change target for cross-district vision
        if (null==actorAt) continue;
        if (actorAt==m_Actor) continue;
        if (actorAt.IsDead) continue;
        perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
      }
    }

    private void _seeActors(List<Percept> perceptList, ThreatTracking threats)
    {
        HashSet<Point> has_threat = (null==threats ? null : new HashSet<Point>());
        foreach (Point pt in FOV) {
          Actor actorAt = m_Actor.Location.Map.GetActorAt(pt); // XXX change target for cross-district vision
          if (null==actorAt) continue;
          if (actorAt== m_Actor) continue;
          if (actorAt.IsDead) continue;
          perceptList.Add(new Percept(actorAt, m_Actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
          if (!m_Actor.IsEnemyOf(actorAt)) continue;
          threats.Sighted(actorAt, actorAt.Location); // XXX change target for cross-district vision
          has_threat.Add(pt);
        }
        // ensure fact what is in sight is current, is recorded
		threats.Cleared(m_Actor.Location.Map,FOV.Except(has_threat));
    }

    private void _seeItems(List<Percept> perceptList)
    {
      foreach (Point position in FOV) {
        Inventory itemsAt = m_Actor.Location.Map.GetItemsAt(position);
        if (null==itemsAt) continue;
        perceptList.Add(new Percept(itemsAt, m_Actor.Location.Map.LocalTime.TurnCounter, new Location(m_Actor.Location.Map, position)));
      }
    }

    private void _seeItems(List<Percept> perceptList, Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items)
    {
      Dictionary<Location,HashSet< Gameplay.GameItems.IDs >> seen_items = new Dictionary<Location, HashSet<Gameplay.GameItems.IDs>>();
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
        seen_items[tmp] = new HashSet<Gameplay.GameItems.IDs>(itemsAt.Items.Select(x => x.Model.ID));
      }
      foreach(Point pt2 in FOV) {
        Location tmp3 = new Location(m_Actor.Location.Map,pt2);
        if (!tmp3.Map.IsInBounds(pt2)) {
          Location? test = m_Actor.Location.Map.Normalize(pt2);
          if (null == test) continue;
          tmp3 = test.Value;
        }
        items.Set(tmp3, (seen_items.ContainsKey(tmp3) ? seen_items[tmp3] : null), tmp3.Map.LocalTime.TurnCounter);
      }
    }

    public List<Percept> Sense(Actor actor)
    {
      m_Actor = actor;
      HashSet<Point> m_FOV = FOV;
      actor.InterestingLocs?.Seen(actor.Location.Map,m_FOV);
      List<Percept> perceptList = new List<Percept>();
      if ((m_Filters & SensingFilter.ACTORS) != SensingFilter.NONE) {
        ThreatTracking threats = actor.Threats;
        if (null != threats) _seeActors(perceptList,threats);
        else _seeActors(perceptList);
      }
      if ((m_Filters & SensingFilter.ITEMS) != SensingFilter.NONE) {
        Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> items = actor.Controller.ItemMemory;
        if (null != items) _seeItems(perceptList, items);
        else _seeItems(perceptList);
      }
      if ((m_Filters & SensingFilter.CORPSES) != SensingFilter.NONE) {
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
