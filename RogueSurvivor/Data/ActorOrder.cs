// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorOrder
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class ActorOrder
  {
    private ActorTasks m_Task;
    private Location m_Location;

    public ActorTasks Task
    {
      get
      {
        return this.m_Task;
      }
    }

    public Location Location
    {
      get
      {
        return this.m_Location;
      }
    }

    public ActorOrder(ActorTasks task, Location location)
    {
      this.m_Task = task;
      this.m_Location = location;
    }

    public override string ToString()
    {
      switch (this.m_Task)
      {
        case ActorTasks.BARRICADE_ONE:
          return string.Format("barricade one ({0},{1})", (object) this.m_Location.Position.X, (object) this.m_Location.Position.Y);
        case ActorTasks.BARRICADE_MAX:
          return string.Format("barricade max ({0},{1})", (object) this.m_Location.Position.X, (object) this.m_Location.Position.Y);
        case ActorTasks.GUARD:
          return string.Format("guard ({0},{1})", (object) this.m_Location.Position.X, (object) this.m_Location.Position.Y);
        case ActorTasks.PATROL:
          return string.Format("patrol ({0},{1})", (object) this.m_Location.Position.X, (object) this.m_Location.Position.Y);
        case ActorTasks.DROP_ALL_ITEMS:
          return "drop all items";
        case ActorTasks.BUILD_SMALL_FORTIFICATION:
          return string.Format("build small fortification ({0},{1})", (object) this.m_Location.Position.X, (object) this.m_Location.Position.Y);
        case ActorTasks.BUILD_LARGE_FORTIFICATION:
          return string.Format("build large fortification ({0},{1})", (object) this.m_Location.Position.X, (object) this.m_Location.Position.Y);
        case ActorTasks.REPORT_EVENTS:
          return "reporting events to leader";
        case ActorTasks.SLEEP_NOW:
          return "sleep there";
        case ActorTasks.FOLLOW_TOGGLE:
          return "stop/start following";
        case ActorTasks.WHERE_ARE_YOU:
          return "reporting position";
        default:
          throw new NotImplementedException("unhandled task");
      }
    }
  }
}
