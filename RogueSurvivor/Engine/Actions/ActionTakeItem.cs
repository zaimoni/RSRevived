﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Drawing;

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
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!(actor.Controller as Gameplay.AI.ObjectiveAI).IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
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

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      Inventory itemsAt = m_Actor.Location.Map.GetItemsAt(m_Position);
      if (!itemsAt?.Contains(m_Item) ?? true) return false;
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
  } // ActionTakeItem


  [Serializable]
  internal class ActionTake : ActorAction
  {
    private readonly Gameplay.GameItems.IDs m_ID;
    private Item m_Item;
    private Point? m_pos;

    public ActionTake(Actor actor, Gameplay.GameItems.IDs it)
      : base(actor)
    {
      m_ID = it;
    }

    public Gameplay.GameItems.IDs ID { get { return m_ID; } }

    private Item Item {
      get {
        init();
        return m_Item;
      }
    }

    private void init()
    {
      if (null != m_Item && (m_Actor.Location.Map.GetItemsAtExt(m_pos.Value.X, m_pos.Value.Y)?.Contains(m_Item) ?? false)) return;
      Inventory itemsAt = m_Actor.Location.Map.GetItemsAt(m_Actor.Location.Position);
      ItemModel model = Models.Items[(int)m_ID];
      m_Item = itemsAt?.GetFirstByModel(model);
      if (null != m_Item) {
        m_pos = m_Actor.Location.Position;
        return;
      }
      foreach(Direction dir in Direction.COMPASS) {
        Point pt = m_Actor.Location.Position+dir;
        itemsAt = m_Actor.Location.Map.GetItemsAtExt(pt.X,pt.Y);
        if (null == itemsAt) continue;
        MapObject obj = m_Actor.Location.Map.GetMapObjectAtExt(pt.X,pt.Y);
        if (!obj?.IsContainer ?? false) continue;
        m_Item = itemsAt.GetFirstByModel(model);
        if (null != m_Item) {
          m_pos = pt;
          return;
        }
      }
    }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      Item it = Item;
      if (null == it) {
        m_FailReason = "not in reach";
        return false;
      }
      return m_Actor.CanGet(it, out m_FailReason);
    }

    public override void Perform()
    {
      Item it = Item;
      RogueForm.Game.DoTakeItem(m_Actor, m_pos.Value, it);
    }

    public override string ToString()
    {
      return m_Actor.Name + " takes " + m_ID.ToString();
    }
  } // ActionTake
}
