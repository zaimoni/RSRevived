﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionEquipItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionEquipItem : ActorAction
  {
    private readonly Item m_Item;

    public ActionEquipItem(Actor actor, Item it) : base(actor)
    {
      m_Item = it
#if DEBUG
        ?? throw new ArgumentNullException(nameof(it))
#endif
      ;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanEquip(m_Item, out m_FailReason);
    }

    public override void Perform() { m_Item.EquippedBy(m_Actor); }

    public override string ToString()
    {
      return m_Actor.Name+" equips "+m_Item;
    }
  }
}
