// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUseItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionUseItem : ActorAction
  {
    private readonly Item m_Item;

    public ActionUseItem(Actor actor, Item it)
      : base(actor)
    {
      Contract.Requires(null != it);
      m_Item = it;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanUse(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoUseItem(m_Actor, m_Item);
    }
  }
}
