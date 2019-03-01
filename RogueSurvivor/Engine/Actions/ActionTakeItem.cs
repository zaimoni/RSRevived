// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionTakeItem : ActorAction
  {
    private readonly Location m_Location;
    private readonly Item m_Item;

    public ActionTakeItem(Actor actor, Location loc, Item it)
      : base(actor)
    {
#if DEBUG
      if (null == it) throw new ArgumentNullException(nameof(it));
      if (!(actor.Controller as Gameplay.AI.ObjectiveAI).IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
#if TRACER
      if (actor.IsDebuggingTarget && Gameplay.GameItems.IDs.==it.Model.ID) throw new InvalidOperationException(actor.Name+": "+it.ToString());
#endif
      m_Location = loc;
      m_Item = it;
#if DEBUG
      Inventory itemsAt = loc.Map.GetItemsAtExt(loc.Position);
      if (null == itemsAt || !itemsAt.Contains(it)) throw new InvalidOperationException("tried to take "+it.ToString()+" from stack that didn't have it");
#endif
    }

    public Item Item { get { return m_Item; } }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      Inventory itemsAt = m_Location.Map.GetItemsAtExt(m_Location.Position);
      if (!itemsAt?.Contains(m_Item) ?? true) return false;
      return m_Actor.CanGet(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      if (m_Location.Map==m_Actor.Location.Map) RogueForm.Game.DoTakeItem(m_Actor, m_Location.Position, m_Item);    // would fail for cross-district containers
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
      if (null != m_Item && (m_Actor.Location.Map.GetItemsAtExt(m_pos.Value)?.Contains(m_Item) ?? false)) return;
      Dictionary<Point,Inventory> stacks = m_Actor.Location.Map.GetAccessibleInventories(m_Actor.Location.Position);
      if (0 > (stacks?.Count ?? 0)) return;

      ItemModel model = Models.Items[(int)m_ID];

      foreach(var x in stacks) {
        m_Item = x.Value.GetFirstByModel(model);
        if (null != m_Item) {
          m_pos = x.Key;
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

  [Serializable]
  internal class ActionGiveTo : ActorAction // next test game \todo wire into CivilianAI
  {
    private readonly Gameplay.GameItems.IDs m_ID;
    private Actor m_Target;

    public ActionGiveTo(Actor actor, Actor target, Gameplay.GameItems.IDs it)
      : base(actor)
    {
      m_ID = it;
      m_Target = target;
    }

    public Gameplay.GameItems.IDs ID { get { return m_ID; } }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      var gift = m_Actor.Inventory.GetBestDestackable(Models.Items[(int)m_ID]);
      if (null==gift) { m_FailReason = "not in inventory"; return false; }
      if (!m_Target.IsPlayer && m_Target.Inventory.IsFull) {
        var trade = RogueForm.Game.PickItemsToTrade(m_Actor, m_Target, gift);   // micro-optimize \todo just need boolean here, so could bypass List<> internal temporary
        if (null == trade) { m_FailReason = "target does not have room in inventory"; return false; }
      }

      return true;
    }

    public override void Perform()
    {
      RogueForm.Game.DoGiveItemTo(m_Actor, m_Target, m_ID);
    }

    public override string ToString()
    {
      return m_Actor.Name + " giving " + m_ID.ToString() + " to " + m_Target.Name;
    }
  } // ActionTake
}
