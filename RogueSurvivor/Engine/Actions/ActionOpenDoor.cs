// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionOpenDoor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionOpenDoor : ActorAction
  {
    private readonly DoorWindow m_Door;

    public DoorWindow door { get { return m_Door; } }

    public ActionOpenDoor(Actor actor, DoorWindow door) : base(actor)
    {
      m_Door = door;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanOpen(m_Door, out m_FailReason);
    }

    // would need IsPerformable if could be saved to hard drive

    public override void Perform()
    {
      RogueForm.Game.DoOpenDoor(m_Actor, m_Door);
    }
  }
}
