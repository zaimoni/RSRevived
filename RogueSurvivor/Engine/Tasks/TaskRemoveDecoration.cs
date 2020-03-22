// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Tasks.TaskRemoveDecoration
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Engine.Tasks
{
  [Serializable]
  internal class TaskRemoveDecoration : TimedTask
  {
    private readonly List<Point> m_pt;  // or HashSet<Point>
    private readonly string m_imageID;

    public string WillRemove { get { return m_imageID; } }

    public TaskRemoveDecoration(int turns, in Point pt, string imageID)
      : base(turns)
    {
      m_pt = new List<Point> { pt };
      m_imageID = imageID;
    }

    public override void Trigger(Map m) {
      foreach(var pt in m_pt) m.RemoveDecorationAt(m_imageID, in pt);
    }

    public void Add(Point dest) { if (!m_pt.Contains(dest)) m_pt.Add(dest); }
    public bool Remove(Point dest) { m_pt.Remove(dest); return 0 >= m_pt.Count; }
  }
}
