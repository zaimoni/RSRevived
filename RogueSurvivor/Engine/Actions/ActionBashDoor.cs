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

    private ActionBashDoor(Actor actor, DoorWindow door) : base(actor) { m_Door = door; }

    public override bool IsLegal()
    {
      m_FailReason = CannotBash(m_Actor) ?? CannotBash(m_Door) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
      if (!Rules.IsAdjacent(m_Actor.Location, m_Door.Location)) {
        m_FailReason = "not adjacent to";
        return false;
      }
      m_FailReason = CouldNotBash(m_Actor) ?? CouldNotBash(m_Door) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoBreak(m_Actor, m_Door);
    }

    // these four are modeled on Actor::ReasonCantBash
    private static string? CannotBash(Actor a)
    {
      if (!a.Model.Abilities.CanBashDoors) return "can't bash doors";
      return null;
    }

    private static string? CannotBash(DoorWindow dw)
    {
      if (MapObject.Break.BREAKABLE == dw.BreakState) return null;
      if (dw.IsBarricaded) return null;
      return "can't break this object";
    }

    private static string? CouldNotBash(Actor a)
    {
      if (a.IsTired) return "tired";
      return null;
    }

    private static string? CouldNotBash(MapObject mapObj)
    {
      if (mapObj.Location.StrictHasActorAt) return "someone is there";
      return null;
    }

    static public ActionBashDoor? create(Actor actor, DoorWindow? dw)
    {
      if (null == dw) return null;
      if (null != CannotBash(actor)) return null;
      if (null != CannotBash(dw)) return null;
      var stage = new ActionBashDoor(actor, dw);
      if (!stage.IsPerformable()) return null;
      return stage;
    }

    static public ActionBashDoor? schedule(Actor actor, DoorWindow? dw)
    { // inline the IsLegal test so we can avoid vacuous construction
      if (null == dw) return null;
      if (null != CannotBash(actor)) return null;
      if (null != CannotBash(dw)) return null;
      return new ActionBashDoor(actor, dw);
    }
  }
}
