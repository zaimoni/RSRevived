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
    private MapObject m_Obj;

    public ActionBreak(Actor actor, MapObject obj)
      : base(actor)
    {
      if (obj == null) throw new ArgumentNullException("obj");
      m_Obj = obj;
    }

    public override bool IsLegal()
    {
      m_FailReason = m_Actor.ReasonCantBreak(m_Obj);
      return ""==m_FailReason;
    }

    public override void Perform()
    {
      RogueForm.Game.DoBreak(m_Actor, m_Obj);
    }
  }
}
