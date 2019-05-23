// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionCloseDoor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionCloseDoor : ActorAction
  {
    private readonly DoorWindow m_Door;
    private readonly bool m_IsFreeAction;

    public ActionCloseDoor(Actor actor, DoorWindow door, bool free = false)
      : base(actor)
    {
#if DEBUG
      if (null == door) throw new ArgumentNullException(nameof(door));
#endif
      m_Door = door;
      m_IsFreeAction = free;
    }

    public DoorWindow Door { get { return m_Door; } }
    public bool IsFreeAction { get { return m_IsFreeAction; } }

    public override bool IsLegal()
    {
      return m_Actor.CanClose(m_Door, out m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
      return Rules.IsAdjacent(m_Actor.Location,m_Door.Location);
    }

    public override void Perform()
    {
      RogueForm.Game.DoCloseDoor(m_Actor, m_Door, m_IsFreeAction);
    }
  }
}
