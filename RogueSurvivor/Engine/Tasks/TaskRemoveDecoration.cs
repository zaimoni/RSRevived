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
    private int m_X;
    private int m_Y;
    private string m_imageID;

    public TaskRemoveDecoration(int turns, int x, int y, string imageID)
      : base(turns)
    {
      this.m_X = x;
      this.m_Y = y;
      this.m_imageID = imageID;
    }

    public override void Trigger(Map m)
    {
      m.GetTileAt(this.m_X, this.m_Y).RemoveDecoration(this.m_imageID);
    }
  }
}
