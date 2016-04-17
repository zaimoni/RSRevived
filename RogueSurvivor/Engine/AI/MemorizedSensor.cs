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
        return this.m_Sensor;
      }
    }

    public MemorizedSensor(Sensor noMemorySensor, int persistance)
    {
      if (noMemorySensor == null)
        throw new ArgumentNullException("decoratedSensor");
      this.m_Sensor = noMemorySensor;
      this.m_Persistance = persistance;
    }

    public void Clear()
    {
      this.m_Percepts.Clear();
    }

    public override List<Percept> Sense(RogueGame game, Actor actor)
    {
      int index1 = 0;
      while (index1 < this.m_Percepts.Count)
      {
        if (this.m_Percepts[index1].GetAge(actor.Location.Map.LocalTime.TurnCounter) > this.m_Persistance)
          this.m_Percepts.RemoveAt(index1);
        else
          ++index1;
      }
      int index2 = 0;
      while (index2 < this.m_Percepts.Count)
      {
        Actor actor1 = this.m_Percepts[index2].Percepted as Actor;
        if (actor1 != null && (actor1.IsDead || actor1.Location.Map != actor.Location.Map))
          this.m_Percepts.RemoveAt(index2);
        else
          ++index2;
      }
      List<Percept> perceptList1 = this.m_Sensor.Sense(game, actor);
      List<Percept> perceptList2 = (List<Percept>) null;
      foreach (Percept percept in perceptList1)
      {
        bool flag = false;
        foreach (Percept mPercept in this.m_Percepts)
        {
          if (mPercept.Percepted == percept.Percepted)
          {
            mPercept.Location = percept.Location;
            mPercept.Turn = percept.Turn;
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          if (perceptList2 == null)
            perceptList2 = new List<Percept>(perceptList1.Count);
          perceptList2.Add(percept);
        }
      }
      if (perceptList2 != null)
      {
        foreach (Percept percept in perceptList2)
          this.m_Percepts.Add(percept);
      }
      return this.m_Percepts;
    }
  }
}
