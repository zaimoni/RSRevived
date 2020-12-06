// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionPush
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal interface ObjectDest
  {
    Location obj_dest { get; }
  }

  internal class ActionPush : ActorAction, ActorDest,ObjectDest
  {
    private readonly MapObject m_Object;
    private readonly Location m_To;

    public MapObject Target { get { return m_Object; } }
    public Location obj_dest { get { return m_To; } }
    public Location dest { get { return m_Object.Location; } }

    public ActionPush(Actor actor, MapObject pushObj, Direction pushDir) : base(actor)
    {
#if DEBUG
      if (null == pushDir) throw new ArgumentNullException(nameof(pushDir));
#endif
      m_Object = pushObj
#if DEBUG
        ?? throw new ArgumentNullException(nameof(pushObj))
#endif
      ;
      m_To = pushObj.Location + pushDir;
#if DEBUG
      // will be ok when tactical pushing is implemented
      if (actor.Controller is Gameplay.AI.OrderableAI && m_To.Map.HasExitAt(m_To.Position) && m_To.Map.IsInBounds(m_To.Position) && !m_Object.IsJumpable) throw new InvalidOperationException("Blocking exit with push");
#endif
      if (!Map.Canonical(ref m_To)) throw new InvalidOperationException("pushed off map");
    }

    public override bool IsLegal()
    {
      if (m_Actor.CanPush(m_Object))
        return m_Object.CanPushTo(m_To, out m_FailReason);
      return false;
    }

    public override void Perform()
    {
      RogueForm.Game.DoPush(m_Actor, m_Object, in m_To);
    }
  }
}
