// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRechargeItemBattery
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionRechargeItemBattery : ActorAction,Use<Item>
  {
    private readonly Item m_Item;

    public ActionRechargeItemBattery(Actor actor, Item it) : base(actor)
    {
#if DEBUG
      if (!(it is BatteryPowered)) throw new ArgumentNullException(nameof(it));
#endif
      m_Item = it;
    }

    public Item Use { get { return m_Item; } }

    public override bool IsLegal()
    {
      return m_Actor.CanRecharge(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoRechargeItemBattery(m_Actor, m_Item);
    }
  }
}
