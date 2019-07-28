// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Tasks.TaskRemoveDecoration
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_short;
#else
using Point = System.Drawing.Point;
#endif


namespace djack.RogueSurvivor.Engine.Tasks
{
  [Serializable]
  internal class TaskRemoveDecoration : TimedTask
  {
#if Z_VECTOR
    private readonly Point m_pt;
#else
    private readonly int m_X;   // \todo savefile break: convert these two to Point
    private readonly int m_Y;
#endif
    private readonly string m_imageID;

    public TaskRemoveDecoration(int turns, int x, int y, string imageID)
      : base(turns)
    {
#if Z_VECTOR
      m_pt = new Point((short)x,(short)y);    // \todo Z_VECTOR : adjust constructor to take correct parameter
#else
      m_X = x;
      m_Y = y;
#endif
      m_imageID = imageID;
    }

    public override void Trigger(Map m)
    {
#if Z_VECTOR
      m.RemoveDecorationAt(m_imageID, m_pt);
#else
      m.RemoveDecorationAt(m_imageID, new Point(m_X, m_Y));
#endif
    }
  }
}
