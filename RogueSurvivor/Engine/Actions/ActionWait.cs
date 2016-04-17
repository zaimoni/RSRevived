// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionWait
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionWait : ActorAction
  {
    public ActionWait(Actor actor, RogueGame game)
      : base(actor, game)
    {
    }

    public override bool IsLegal()
    {
      return true;
    }

    public override void Perform()
    {
      this.m_Game.DoWait(this.m_Actor);
    }
  }
}
