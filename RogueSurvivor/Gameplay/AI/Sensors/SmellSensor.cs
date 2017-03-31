// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;
using System;
using System.Collections.Generic;
using System.Drawing;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
  [Serializable]
  internal class SmellSensor : Sensor
  {
    private readonly Odor m_OdorToSmell;
    private readonly List<Percept> m_List;

    public List<Percept> Scents {
      get {
        return m_List;
      }
    }

    public SmellSensor(Odor odorToSmell)
    {
      m_OdorToSmell = odorToSmell;
      m_List = new List<Percept>(9);
    }

    public List<Percept> Sense(Actor actor)
    {
      m_List.Clear();
      int num = actor.SmellThreshold;
      int x1 = actor.Location.Position.X - 1;
      int x2 = actor.Location.Position.X + 1;
      int y1 = actor.Location.Position.Y - 1;
      int y2 = actor.Location.Position.Y + 1;
      actor.Location.Map.TrimToBounds(ref x1, ref y1);
      actor.Location.Map.TrimToBounds(ref x2, ref y2);
      int turnCounter = actor.Location.Map.LocalTime.TurnCounter;
      Point position = new Point();
      for (int index1 = x1; index1 <= x2; ++index1) {
        position.X = index1;
        for (int index2 = y1; index2 <= y2; ++index2) {
          position.Y = index2;
          int scentByOdorAt = actor.Location.Map.GetScentByOdorAt(m_OdorToSmell, position);
          if (scentByOdorAt >= 0 && scentByOdorAt >= num)
            m_List.Add(new Percept((object) new SmellSensor.AIScent(m_OdorToSmell, scentByOdorAt), turnCounter, new Location(actor.Location.Map, position)));
        }
      }
      return m_List;
    }

    [Serializable]
    public class AIScent
    {
      public Odor Odor { get; private set; }

      public int Strength { get; private set; }

      public AIScent(Odor odor, int strength)
      {
                Odor = odor;
                Strength = strength;
      }
    }
  }
}
