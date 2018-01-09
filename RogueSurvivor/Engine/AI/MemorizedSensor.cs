// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.MemorizedSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Engine.AI
{
  [Serializable]
  internal class MemorizedSensor : Sensor
  {
    private List<Percept> m_Percepts = new List<Percept>();
    private readonly Sensor m_Sensor;
    private readonly int m_Persistance;

    public Sensor Sensor { get { return m_Sensor; } }

    public MemorizedSensor(Sensor noMemorySensor, int persistance)
    {
#if DEBUG
      if (null == noMemorySensor) throw new ArgumentNullException(nameof(noMemorySensor));
#endif
      m_Sensor = noMemorySensor;
      m_Persistance = persistance;
    }

    public void Clear()
    {
      m_Percepts.Clear();
    }

    public void Forget(Actor actor)
    {
      var tmp = new List<Percept>(m_Percepts.Count);
      foreach(var p in m_Percepts) {
        if (p.GetAge(actor.Location.Map.LocalTime.TurnCounter)>m_Persistance) continue;
        if (p.Percepted is Actor a) {
          if (a.IsDead) continue;
          if (a.Location.Map != actor.Location.Map) continue;   // XXX valid for RS Alpha 6, invalid for RS Revived; want to verify other changes first
        }
        if (p.Percepted is Inventory inv) {
          if (inv.IsEmpty) continue;
        }
      }
      m_Percepts = tmp.ToList();
    }

    public List<Percept> Sense(Actor actor)
    {
      Forget(actor);

      var tmp = new HashSet<Percept>(m_Sensor.Sense(actor));   // time is m_Actor.Location.Map.LocalTime.TurnCounter
      foreach (Percept percept in tmp.ToList()) { 
        foreach (Percept mPercept in m_Percepts) {
          if (mPercept.Percepted == percept.Percepted) {
            mPercept.Location = percept.Location;
            mPercept.Turn = percept.Turn;
            tmp.Remove(percept);
            break;
          }
        }
      }

      // huge automatic multipler isn't a good idea here
      int newSize = m_Percepts.Count + tmp.Count;
      if (newSize > m_Percepts.Capacity) m_Percepts.Capacity = newSize;

      m_Percepts.AddRange(tmp);
      return m_Percepts;
    }
  }
}
