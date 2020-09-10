// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUseItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal interface Use<out T> where T:Item
  {
    public T Use { get; }
  }


  internal class ActionUseItem : ActorAction, Use<Item>
    {
    private readonly Item m_Item;

    public ActionUseItem(Actor actor, Item it)
      : base(actor)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));  // insane action items need not be usable
#endif
      m_Item = it;
      actor.Activity = Activity.IDLE;
    }

    public Item Use { get { return m_Item; } }

    public override bool IsLegal()
    {
      return m_Actor.CanUse(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoUseItem(m_Actor, m_Item);
    }
  } // ActionUseItem

  [Serializable]
  internal class ActionUse : ActorAction, Use<Item>
    {
    private readonly Gameplay.GameItems.IDs m_ID;
    [NonSerialized] private Item? m_Item = null;

    public ActionUse(Actor actor, Gameplay.GameItems.IDs it)
      : base(actor)
    {
#if DEBUG
      if (!RogueGame.IsUsable(Models.Items[(int)it])) throw new ArgumentNullException(nameof(it));
#endif
      m_ID = it;
    }

    public Gameplay.GameItems.IDs ID { get { return m_ID; } } // for completeness

    public Item? Use {
      get {
        if (null == m_Item) m_Item = m_Actor.Inventory.GetBestDestackable(Models.Items[(int) m_ID]);
        return m_Item;
      }
    }

    public override bool IsLegal()
    {
      var it = Use;
      if (null == it) {
        m_FailReason = "not in inventory";
        return false;
      }
      return m_Actor.CanUse(it, out m_FailReason);
    }

    public override void Perform()
    {
      m_Actor.Activity = Activity.IDLE;
      RogueForm.Game.DoUseItem(m_Actor, Use);
    }
  } // ActionUse
}
