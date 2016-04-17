// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionPush
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionPush : ActorAction
  {
    private readonly MapObject m_Object;
    private readonly Direction m_Direction;
    private readonly Point m_To;

    public Direction Direction
    {
      get
      {
        return this.m_Direction;
      }
    }

    public Point To
    {
      get
      {
        return this.m_To;
      }
    }

    public ActionPush(Actor actor, RogueGame game, MapObject pushObj, Direction pushDir)
      : base(actor, game)
    {
      if (pushObj == null)
        throw new ArgumentNullException("pushObj");
      this.m_Object = pushObj;
      this.m_Direction = pushDir;
      this.m_To = pushObj.Location.Position + pushDir;
    }

    public override bool IsLegal()
    {
      if (this.m_Game.Rules.CanActorPush(this.m_Actor, this.m_Object))
        return this.m_Game.Rules.CanPushObjectTo(this.m_Object, this.m_To, out this.m_FailReason);
      return false;
    }

    public override void Perform()
    {
      this.m_Game.DoPush(this.m_Actor, this.m_Object, this.m_To);
    }
  }
}
