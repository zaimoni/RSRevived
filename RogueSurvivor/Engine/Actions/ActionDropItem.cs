// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionDropItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionDropItem : ActorAction,ActorGive
  {
    private readonly Item m_Item;

    public Item Give { get { return m_Item; } }

    public ActionDropItem(Actor actor, Item it) : base(actor)
    {
      m_Item = it
#if DEBUG
        ?? throw new ArgumentNullException(nameof(it))
#endif
      ;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanDrop(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoDropItem(m_Actor, m_Item);
    }
  }
}
