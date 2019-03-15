// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionPush
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionPush : ActorAction
  {
    private readonly MapObject m_Object;
    private readonly Direction m_Direction;
    private readonly Point m_To;

    public MapObject Target { get { return m_Object; } }
    public Point To { get { return m_To; } }

    public ActionPush(Actor actor, MapObject pushObj, Direction pushDir)
      : base(actor)
    {
#if DEBUG
      if (null == pushObj) throw new ArgumentNullException(nameof(pushObj));
      if (null == pushDir) throw new ArgumentNullException(nameof(pushDir));
#endif
      m_Object = pushObj;
      m_Direction = pushDir;
      m_To = pushObj.Location.Position + pushDir;
#if DEBUG
      // will be ok when tactical pushing is implemented
      if (m_Object.Location.Map.HasExitAt(m_To) && m_Object.Location.Map.IsInBounds(m_To) && !m_Object.IsJumpable) throw new InvalidOperationException("Blocking exit with push");
#endif
    }

    public override bool IsLegal()
    {
      if (m_Actor.CanPush(m_Object))
        return m_Object.CanPushTo(m_To, out m_FailReason);
      return false;
    }

    public override void Perform()
    {
      RogueForm.Game.DoPush(m_Actor, m_Object, m_To);
    }
  }
}
