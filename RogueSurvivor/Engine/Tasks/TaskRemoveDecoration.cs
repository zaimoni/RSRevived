// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Tasks.TaskRemoveDecoration
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Tasks
{
  [Serializable]
  internal class TaskRemoveDecoration : TimedTask
  {
    private readonly Point m_pt;
    private readonly string m_imageID;

    public TaskRemoveDecoration(int turns, int x, int y, string imageID)
      : base(turns)
    {
      m_pt = new Point((short)x,(short)y);    // \todo Z_VECTOR : adjust constructor to take correct parameter
      m_imageID = imageID;
    }

    public override void Trigger(Map m)
    {
      m.RemoveDecorationAt(m_imageID, m_pt);
    }
  }
}
