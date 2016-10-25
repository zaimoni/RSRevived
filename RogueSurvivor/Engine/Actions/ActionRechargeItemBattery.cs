// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRechargeItemBattery
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionRechargeItemBattery : ActorAction
  {
    private Item m_Item;

    public ActionRechargeItemBattery(Actor actor, Item it)
      : base(actor)
    {
      if (null == (it as BatteryPowered)) throw new ArgumentNullException("it");
      m_Item = it;
    }

    public override bool IsLegal()
    {
      return RogueForm.Game.Rules.CanActorRechargeItemBattery(m_Actor, m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoRechargeItemBattery(m_Actor, m_Item);
    }
  }
}
