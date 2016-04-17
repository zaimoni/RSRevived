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
    private string m_Text;

    public ActionShout(Actor actor, RogueGame game)
      : this(actor, game, (string) null)
    {
    }

    public ActionShout(Actor actor, RogueGame game, string text)
      : base(actor, game)
    {
      this.m_Text = text;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorShout(this.m_Actor, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoShout(this.m_Actor, this.m_Text);
    }
  }
}
