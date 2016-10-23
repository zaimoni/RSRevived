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

    public ActionSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
      : base(actor)
    {
      if (powGen == null) throw new ArgumentNullException("powGen");
      m_PowGen = powGen;
    }

    public override bool IsLegal()
    {
      return RogueForm.Game.Rules.IsSwitchableFor(m_Actor, m_PowGen, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoSwitchPowerGenerator(m_Actor, m_PowGen);
    }
  }
}
