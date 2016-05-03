// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUseItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionUseItem : ActorAction
  {
    private Item m_Item;

    public ActionUseItem(Actor actor, RogueGame game, Item it)
      : base(actor, game)
    {
      if (it == null)
        throw new ArgumentNullException("item");
            m_Item = it;
    }

    public override bool IsLegal()
    {
      return m_Game.Rules.CanActorUseItem(m_Actor, m_Item, out m_FailReason);
    }

    public override void Perform()
    {
            m_Game.DoUseItem(m_Actor, m_Item);
    }
  }
}
