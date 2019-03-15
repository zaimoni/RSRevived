// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.Sensors.SmellSensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.AI;
using System;
using System.Collections.Generic;
using Zaimoni.Data;

#if Z_VECTOR
using Rectangle = Zaimoni.Data.Box2D_int;
#else
using Rectangle = System.Drawing.Rectangle;
#endif


namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
  [Serializable]
  internal struct AIScent
  {
    public readonly int Strength;

    public AIScent(int strength)
    {
      Strength = strength;
    }
  }

  [Serializable]
  internal class SmellSensor
  {
    private readonly Odor m_OdorToSmell;
    private readonly List<Percept_<AIScent>> m_List = new List<Percept_<AIScent>>(9);

    public List<Percept_<AIScent>> Scents { get { return m_List; } }

    public SmellSensor(Odor odorToSmell)
    {
      m_OdorToSmell = odorToSmell;
    }

    public List<Percept_<AIScent>> Sense(Actor actor)
    {
#if DEBUG
      if (OdorScent.MIN_STRENGTH > actor.SmellThreshold) throw new ArgumentOutOfRangeException(nameof(actor), OdorScent.MIN_STRENGTH.ToString()+" > actor.SmellThreshold");
#endif
      m_List.Clear();
      int num = actor.SmellThreshold;  // floors at 1
      Rectangle survey = new Rectangle(actor.Location.Position.X - 1, actor.Location.Position.Y - 1, 3, 3);
      Map map = actor.Location.Map;
      int turnCounter = actor.Location.Map.LocalTime.TurnCounter;
      int scentByOdorAt = 0;
      survey.DoForEach(pt => { 
        m_List.Add(new Percept_<AIScent>(new AIScent(scentByOdorAt), turnCounter, new Location(map, pt)));
      },pt => { 
        scentByOdorAt = map.GetScentByOdorAt(m_OdorToSmell, pt); // XXX 0 is the no-scent value
        return scentByOdorAt >= num;
      });
      return m_List;
    }
  }
}
