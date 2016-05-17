// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.LOSSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
  [Serializable]
  internal class LOSSensor : Sensor
  {
    private HashSet<Point> m_FOV;
    private LOSSensor.SensingFilter m_Filters;

    public HashSet<Point> FOV
    {
      get
      {
        return m_FOV;
      }
    }

    public LOSSensor.SensingFilter Filters
    {
      get
      {
        return m_Filters;
      }
      set
      {
                m_Filters = value;
      }
    }

    public LOSSensor(LOSSensor.SensingFilter filters)
    {
            m_Filters = filters;
    }

    public override List<Percept> Sense(RogueGame game, Actor actor)
    {
      m_FOV = LOS.ComputeFOVFor(actor, actor.Location.Map.LocalTime, game.Session.World.Weather);
      int num = actor.FOVrange(actor.Location.Map.LocalTime, game.Session.World.Weather);
      List<Percept> perceptList = new List<Percept>();
      if ((m_Filters & LOSSensor.SensingFilter.ACTORS) != LOSSensor.SensingFilter.NONE)
      {
        if (num * num < actor.Location.Map.CountActors)
        {
          foreach (Point point in m_FOV)
          {
            Actor actorAt = actor.Location.Map.GetActorAt(point.X, point.Y);
            if (actorAt != null && actorAt != actor)
              perceptList.Add(new Percept((object) actorAt, actor.Location.Map.LocalTime.TurnCounter, actorAt.Location));
          }
        }
        else
        {
          foreach (Actor actor1 in actor.Location.Map.Actors)
          {
            if (actor1 != actor && (double) Rules.LOSDistance(actor.Location.Position, actor1.Location.Position) <= (double) num && m_FOV.Contains(actor1.Location.Position))
              perceptList.Add(new Percept((object) actor1, actor.Location.Map.LocalTime.TurnCounter, actor1.Location));
          }
        }
      }
      if ((m_Filters & LOSSensor.SensingFilter.ITEMS) != LOSSensor.SensingFilter.NONE)
      {
        foreach (Point position in m_FOV)
        {
          Inventory itemsAt = actor.Location.Map.GetItemsAt(position);
          if (itemsAt != null && !itemsAt.IsEmpty)
            perceptList.Add(new Percept((object) itemsAt, actor.Location.Map.LocalTime.TurnCounter, new Location(actor.Location.Map, position)));
        }
      }
      if ((m_Filters & LOSSensor.SensingFilter.CORPSES) != LOSSensor.SensingFilter.NONE)
      {
        foreach (Point position in m_FOV)
        {
          List<Corpse> corpsesAt = actor.Location.Map.GetCorpsesAt(position.X, position.Y);
          if (corpsesAt != null)
            perceptList.Add(new Percept((object) corpsesAt, actor.Location.Map.LocalTime.TurnCounter, new Location(actor.Location.Map, position)));
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
