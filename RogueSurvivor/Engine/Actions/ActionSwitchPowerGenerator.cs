// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSwitchPowerGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionSwitchPowerGenerator : ActorAction
  {
    private PowerGenerator m_PowGen;

    public ActionSwitchPowerGenerator(Actor actor, RogueGame game, PowerGenerator powGen)
      : base(actor, game)
    {
      if (powGen == null)
        throw new ArgumentNullException("powGen");
      this.m_PowGen = powGen;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.IsSwitchableFor(this.m_Actor, this.m_PowGen, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoSwitchPowerGenerator(this.m_Actor, this.m_PowGen);
    }
  }
}
