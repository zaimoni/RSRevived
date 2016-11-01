﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionTakeItem : ActorAction
  {
    private Point m_Position;
    private Item m_Item;

    public ActionTakeItem(Actor actor, Point position, Item it)
      : base(actor)
    {
      Contract.Requires(null != it);
      m_Position = position;
      m_Item = it;
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
  }
}
