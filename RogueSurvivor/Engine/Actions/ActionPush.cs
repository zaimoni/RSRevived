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
        return m_Direction;
      }
    }

    public Point To
    {
      get
      {
        return m_To;
      }
    }

    public ActionPush(Actor actor, RogueGame game, MapObject pushObj, Direction pushDir)
      : base(actor, game)
    {
      if (pushObj == null)
        throw new ArgumentNullException("pushObj");
            m_Object = pushObj;
            m_Direction = pushDir;
            m_To = pushObj.Location.Position + pushDir;
    }

    public override bool IsLegal()
    {
      if (m_Game.Rules.CanActorPush(m_Actor, m_Object))
        return m_Game.Rules.CanPushObjectTo(m_Object, m_To, out m_FailReason);
      return false;
    }

    public override void Perform()
    {
            m_Game.DoPush(m_Actor, m_Object, m_To);
    }
  }
}
