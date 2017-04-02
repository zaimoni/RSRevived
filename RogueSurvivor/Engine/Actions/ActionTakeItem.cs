// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionTakeItem : ActorAction
  {
    private readonly Point m_Position;
    private readonly Item m_Item;

    public ActionTakeItem(Actor actor, Point position, Item it)
      : base(actor)
    {
      Contract.Requires(null != it);
      m_Position = position;
      m_Item = it;
#if DEBUG
      Inventory itemsAt = actor.Location.Map.GetItemsAt(position);
      if (null == itemsAt || !itemsAt.Contains(it)) throw new InvalidOperationException("tried to take "+it.ToString()+" from stack that didn't have it");
#endif
    }

    public Item Item {
      get {
        return m_Item;
      }
    }

    public override bool IsLegal()
    {
      return m_Actor.CanGet(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoTakeItem(m_Actor, m_Position, m_Item);
    }

    public override string ToString()
    {
      return m_Actor.Name + " takes " + m_Item;
    }
  }
}
