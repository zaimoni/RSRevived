// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBashDoor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBashDoor : ActorAction
  {
    private readonly DoorWindow m_Door;
    public DoorWindow Target { get { return m_Door; } }

    public ActionBashDoor(Actor actor, DoorWindow door)
      : base(actor)
    {
#if DEBUG
      if (null == door) throw new ArgumentNullException(nameof(door));
#endif
      m_Door = door;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanBash(m_Door, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoBreak(m_Actor, (MapObject)m_Door);
    }
  }
}
