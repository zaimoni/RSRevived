// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSwitchPowerGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionSwitchPowerGenerator : ActorAction
  {
    private readonly PowerGenerator m_PowGen;

    public ActionSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
      : base(actor)
    {
      m_PowGen = powGen;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanSwitch(m_PowGen, out m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
      return 1==Rules.GridDistance(m_Actor.Location, m_PowGen.Location);
    }

    public override void Perform()
    {
      RogueGame.Game.DoSwitchPowerGenerator(m_Actor, m_PowGen);
    }
  }
}
