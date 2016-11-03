// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSwitchPowerGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionSwitchPowerGenerator : ActorAction
  {
    private readonly PowerGenerator m_PowGen;

    public ActionSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
      : base(actor)
    {
      Contract.Requires(null != powGen);
      m_PowGen = powGen;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanSwitch(m_PowGen, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoSwitchPowerGenerator(m_Actor, m_PowGen);
    }
  }
}
