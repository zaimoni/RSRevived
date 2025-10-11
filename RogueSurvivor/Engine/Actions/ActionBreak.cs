// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBreak
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBreak : ActorAction
  {
    private readonly MapObject m_Obj;
    public MapObject Target { get { return m_Obj; } }

    private ActionBreak(Actor actor, MapObject obj) : base(actor) { m_Obj = obj; }

    public override bool IsLegal()
    {
      m_FailReason = CannotBreak(m_Actor) ?? CannotBreak(m_Obj) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
      if (!Rules.IsAdjacent(m_Actor.Location, m_Obj.Location)) {
        m_FailReason = "not adjacent to";
        return false;
      }
      m_FailReason = CouldNotBreak(m_Actor) ?? CouldNotBreak(m_Obj) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      m_Actor.Activity = Activity.IDLE;
      RogueGame.Game.DoBreak(m_Actor, m_Obj);
    }

    // these four are modeled on Actor::ReasonCantBreak
    private static string? CannotBreak(Actor a) {
      if (!a.Model.Abilities.CanBreakObjects) return "cannot break objects";
      return null;
    }

    private static string? CannotBreak(MapObject mapObj) {
      if (MapObject.Break.BREAKABLE == mapObj.BreakState) return null;
      if (mapObj is DoorWindow dw && dw.IsBarricaded) return null;
      return "can't break this object";
    }

    private static string? CouldNotBreak(Actor a) {
      if (a.IsTired) return "tired";
      return null;
    }

    private static string? CouldNotBreak(MapObject mapObj) {
      if (mapObj.Location.StrictHasActorAt) return "someone is there";
      return null;
    }

    static public ActionBreak? create(Actor actor, MapObject? obj)
    {
      if (null == obj) return null;
      if (null != CannotBreak(actor)) return null;
      if (null != CannotBreak(obj)) return null;
      var stage = new ActionBreak(actor, obj);
      if (!stage.IsPerformable()) return null;
      return stage;
    }

    static public ActionBreak? schedule(Actor actor, MapObject? obj)
    { // inline the IsLegal test so we can avoid vacuous construction
      if (null == obj) return null;
      if (null != CannotBreak(actor)) return null;
      if (null != CannotBreak(obj)) return null;
      return new ActionBreak(actor, obj);
    }
  }
}
