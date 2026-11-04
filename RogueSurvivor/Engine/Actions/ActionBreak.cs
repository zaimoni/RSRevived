// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBreak
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBreak : ActorAction
  {
    private readonly MapObject m_Obj;
    public MapObject Target { get { return m_Obj; } }

    private ActionBreak(Actor actor, MapObject obj) : base(actor) { m_Obj = obj; }

    public override bool IsLegal()
    {
      m_FailReason = m_Actor.CannotBreak ?? m_Obj.ReasonCannotBreak();
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!IsLegal()) return false;
      if (!Rules.IsAdjacent(m_Actor.Location, m_Obj.Location)) {
        m_FailReason = "not adjacent to";
        return false;
      }
      m_FailReason = m_Actor.ReasonCouldNotBreak() ?? m_Obj.ReasonCouldNotBreak();
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      m_Actor.Activity = Activity.IDLE;
      RogueGame.Game.DoBreak(m_Actor, m_Obj);
    }

    static public ActionBreak? schedule(Actor actor, MapObject? obj)
    { // inline the IsLegal test so we can avoid vacuous construction
      if (obj is null) return null;
      if (actor.CannotBreak is not null) return null;
      if (obj.ReasonCannotBreak() is not null) return null;
      return new ActionBreak(actor, obj);
    }

    static public ActionBreak? create(Actor actor, MapObject? obj)
    {
      var ret = schedule(actor, obj);
      if (ret is not null && !ret.IsPerformable()) return null;
      return ret;
    }
  }
}
