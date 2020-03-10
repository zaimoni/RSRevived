// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorOrder
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal enum ActorTasks
  {
    BARRICADE_ONE,
    BARRICADE_MAX,
    GUARD,
    PATROL,
    DROP_ALL_ITEMS,
    BUILD_SMALL_FORTIFICATION,
    BUILD_LARGE_FORTIFICATION,
    REPORT_EVENTS,
    SLEEP_NOW,
    FOLLOW_TOGGLE,
    WHERE_ARE_YOU,
  }

  [Serializable]
  internal class ActorOrder
  {
    public readonly ActorTasks Task;
    public readonly Location Location;

    public ActorOrder(ActorTasks task, Location location)
    {
      Task = task;
      Location = location;
    }

    public override string ToString()
    {
      switch (Task)
      {
        case ActorTasks.BARRICADE_ONE:
          return string.Format("barricade one ({0},{1})", (object) Location.Position.X, (object) Location.Position.Y);
        case ActorTasks.BARRICADE_MAX:
          return string.Format("barricade max ({0},{1})", (object) Location.Position.X, (object) Location.Position.Y);
        case ActorTasks.GUARD:
          return string.Format("guard ({0},{1})", (object) Location.Position.X, (object) Location.Position.Y);
        case ActorTasks.PATROL:
          return string.Format("patrol ({0},{1})", (object) Location.Position.X, (object) Location.Position.Y);
        case ActorTasks.DROP_ALL_ITEMS:
          return "drop all items";
        case ActorTasks.BUILD_SMALL_FORTIFICATION:
          return string.Format("build small fortification ({0},{1})", (object) Location.Position.X, (object) Location.Position.Y);
        case ActorTasks.BUILD_LARGE_FORTIFICATION:
          return string.Format("build large fortification ({0},{1})", (object) Location.Position.X, (object) Location.Position.Y);
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
