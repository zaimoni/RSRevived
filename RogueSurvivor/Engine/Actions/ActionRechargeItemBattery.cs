// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRechargeItemBattery
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionRechargeItemBattery : ActorAction, Use<Item>, NotSchedulable
    {
    private readonly Item m_Item;
    private readonly BatteryPowered _batt;

    private ActionRechargeItemBattery(Actor actor, Item it) : base(actor)
    {
      _batt = it as BatteryPowered ?? throw new ArgumentNullException(nameof(it));
      m_Item = it;
    }

    public Item Use { get { return m_Item; } }

    public override bool IsLegal() => true;

    public override void Perform()
    {
      m_Actor.SpendActionPoints();
      m_Item.EquippedBy(m_Actor);
      _batt.Recharge();
      RogueGame.Game.UI_RechargeItemBattery(m_Actor, m_Item);
    }

    public static Item? WantsToRecharge(Actor whom) {
      Item? it = whom.Inventory.GetFirstMatching<Items.ItemLight>(obj => obj.MaxBatteries - 1 > obj.Batteries);
      if (null != it) return it;
      return whom.Inventory.GetFirstMatching<Items.ItemTracker>(obj => Gameplay.Item_IDs.TRACKER_POLICE_RADIO != obj.ModelID && obj.MaxBatteries - 1 > obj.Batteries);
    }

    public static ActionRechargeItemBattery? Recharge(Actor whom, Item? it) {
      // inline Actor::CanRecharge
#if DEBUG
      if (!whom.Model.Abilities.CanUseItems) return null;
#endif
      if (it is not BatteryPowered obj) return null; // should imply equippable
      if (obj.MaxBatteries-1 <= obj.Batteries) return null; // no-op
      if (!whom.Inventory.Contains(it)) return null;
//    it.EquippedBy(whom); // called when pathfinding, so no side effects here
      return new ActionRechargeItemBattery(whom, it);
    }


  }
}
