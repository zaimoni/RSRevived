// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionChat
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionChat : ActorAction
  {
    private readonly Actor m_Target;

    public Actor Target { get { return m_Target; } }

    public ActionChat(Actor actor, Actor target)
      : base(actor)
    {
      m_Target = target;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanChatWith(m_Target,out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoChat(m_Actor, m_Target);
    }
  }
}
