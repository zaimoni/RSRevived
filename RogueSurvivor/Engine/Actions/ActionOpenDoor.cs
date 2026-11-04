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
      m_FailReason = ReasonIllegal(m_Actor) ?? ReasonIllegal(m_Door);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!IsLegal()) return false;
      m_FailReason = ReasonCant(m_Actor, m_Door);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      bool was_opaque = !m_Door.IsTransparent;
      m_Door.SetState(DoorWindow.STATE_OPEN);
      m_Actor.SpendActionPoints();
      if (was_opaque) m_Actor.Controller.UpdateSensors();
      RogueGame.Game.UI_OpenDoor(m_Actor, m_Door);
    }

    public static string? ReasonIllegal(Actor actor)
    {
      if (!actor.Model.Abilities.CanUseMapObjects) return "no ability to open";
      return null;
    }

    private static string? ReasonIllegal(DoorWindow door)
    {
      if (door.BarricadePoints > 0) return "is barricaded";
      if (!door.IsClosed) return "not closed";
      return null;
    }

    private static string? ReasonCant(Actor actor, DoorWindow door)
    {
      if (1 != Rules.InteractionDistance(actor.Location, door.Location)) return "not adjacent to door/window";
      return null;
    }
  }
}
