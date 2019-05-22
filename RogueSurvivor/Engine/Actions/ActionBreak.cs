// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBreak
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBreak : ActorAction
  {
    private readonly MapObject m_Obj;
    public MapObject Target { get { return m_Obj; } }

    public ActionBreak(Actor actor, MapObject obj)
      : base(actor)
    {
#if DEBUG
      if (null == obj) throw new ArgumentNullException(nameof(obj));
#endif
      m_Obj = obj;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanBreak(m_Obj, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoBreak(m_Actor, m_Obj);
    }
  }
}
