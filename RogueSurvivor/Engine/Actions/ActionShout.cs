// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionShout
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionShout : ActorAction
  {
    private readonly string m_Text;

    public ActionShout(Actor actor, string text=null)
      : base(actor)
    {
      m_Text = text;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanShout(out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoShout(m_Actor, m_Text);
    }
  }
}
