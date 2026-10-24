// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUnequipItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionUnequipItem : ActorAction
  {
    private readonly Item m_Item;

    private new readonly LivingActor m_Actor;   // the same object as the base field

    public ActionUnequipItem(LivingActor actor, Item it) : base(actor)
    {
      m_Actor = actor;
      m_Item = it
#if DEBUG
        ?? throw new ArgumentNullException(nameof(it))
#endif
      ;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanUnequip(m_Item, out m_FailReason);
    }

    public override void Perform() { m_Item.UnequippedBy(m_Actor); }
  }
}
