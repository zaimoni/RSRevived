// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Tasks.TaskRemoveDecoration
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Tasks
{
  [Serializable]
  internal class TaskRemoveDecoration : TimedTask
  {
    private readonly int m_X;
    private readonly int m_Y;
    private readonly string m_imageID;

    public TaskRemoveDecoration(int turns, int x, int y, string imageID)
      : base(turns)
    {
      m_X = x;
      m_Y = y;
      m_imageID = imageID;
    }

    public override void Trigger(Map m)
    {
      m.RemoveDecorationAt(m_imageID, m_X, m_Y);
    }
  }
}
