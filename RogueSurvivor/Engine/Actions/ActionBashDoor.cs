// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBashDoor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBashDoor : ActorAction
  {
    private DoorWindow m_Door;

    public ActionBashDoor(Actor actor, DoorWindow door)
      : base(actor)
    {
      Contract.Requires(null != door);
      m_Door = door;
    }

    public override bool IsLegal()
    {
      return RogueForm.Game.Rules.IsBashableFor(m_Actor, m_Door, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoBreak(m_Actor, (MapObject)m_Door);
    }
  }
}
