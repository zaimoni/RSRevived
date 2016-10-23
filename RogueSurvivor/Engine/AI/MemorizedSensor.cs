// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.MemorizedSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine.AI
{
  [Serializable]
  internal class MemorizedSensor : Sensor
  {
    private List<Percept> m_Percepts = new List<Percept>();
    private Sensor m_Sensor;
    private int m_Persistance;

    public Sensor Sensor
    {
      get
      {
        return m_Sensor;
      }
    }

    public MemorizedSensor(Sensor noMemorySensor, int persistance)
    {
      if (noMemorySensor == null)
        throw new ArgumentNullException("decoratedSensor");
            m_Sensor = noMemorySensor;
            m_Persistance = persistance;
    }

    public void Clear()
    {
            m_Percepts.Clear();
    }

    public void Forget(Actor actor)
    {
      int i = m_Percepts.Count;
      while(0 < i--)
        {
        if (m_Percepts[i].GetAge(actor.Location.Map.LocalTime.TurnCounter) > m_Persistance)
          m_Percepts.RemoveAt(i);
        }
      i = m_Percepts.Count;
      while(0 < i--)
        {
        Actor actor1 = m_Percepts[i].Percepted as Actor;
        if (actor1 != null && (actor1.IsDead || actor1.Location.Map != actor.Location.Map))
          m_Percepts.RemoveAt(i);
        }
    }

    public override List<Percept> Sense(Actor actor)
    {
      Forget(actor);

      List<Percept> perceptList1 = m_Sensor.Sense(actor);
      List<Percept> perceptList2 = null;
      foreach (Percept percept in perceptList1)
      {
        bool flag = false;
        foreach (Percept mPercept in m_Percepts) {
          if (mPercept.Percepted == percept.Percepted) {
            mPercept.Location = percept.Location;
            mPercept.Turn = percept.Turn;
            flag = true;
            break;
          }
        }
        if (!flag) {
          if (perceptList2 == null)
            perceptList2 = new List<Percept>(perceptList1.Count);
          perceptList2.Add(percept);
        }
      }
      if (perceptList2 != null) {
        foreach (Percept percept in perceptList2)
          m_Percepts.Add(percept);
      }
      return m_Percepts;
    }
  }
}
