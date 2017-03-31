// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.MemorizedSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

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
      Contract.Requires(null != noMemorySensor);
      m_Sensor = noMemorySensor;
      m_Persistance = persistance;
    }

    public void Clear()
    {
      m_Percepts.Clear();
    }

    public void Forget(Actor actor)
    {
      IEnumerable<Percept> tmp = m_Percepts.Where(p=>p.GetAge(actor.Location.Map.LocalTime.TurnCounter) <= m_Persistance).Where(p=>
        {
            Actor a = p.Percepted as Actor;
            return a == null || (!a.IsDead && a.Location.Map == actor.Location.Map);
        });
      m_Percepts = tmp.ToList();
    }

    public override List<Percept> Sense(Actor actor)
    {
      Forget(actor);

      HashSet<Percept> tmp = new HashSet<Percept>(m_Sensor.Sense(actor));
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
      m_Percepts.AddRange(tmp);
      return m_Percepts;
    }
  }
}
