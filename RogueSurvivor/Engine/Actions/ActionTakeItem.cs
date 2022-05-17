// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionTakeItem : ActorAction,ActorTake
  {
    private readonly Location m_Location;
    private readonly Item m_Item;
    private readonly MapObject? m_Container = null;

    public ActionTakeItem(Actor actor, in Location loc, Item it) : base(actor)
    {
#if DEBUG
      if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
      if (!ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
      m_Location = loc;
      if (!Map.Canonical(ref m_Location)) throw new ArgumentNullException(nameof(loc));
      m_Item = it;
      var obj = loc.MapObject;
      var obj_inv = obj?.NonEmptyInventory;
      if (null != obj_inv && obj_inv.Contains(it)) m_Container = obj;

#if DEBUG
      else {
        var itemsAt = loc.Items;
        if (null == itemsAt || !itemsAt.Contains(it))
          throw new InvalidOperationException("tried to take "+it.ToString()+" from stack that didn't have it");
      }
#endif
    }

    public ActionTakeItem(Actor actor, in InventorySource<Item> stack, Item it) : base(actor)
    {
#if DEBUG
      if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
      if (!ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
      if (null == stack.loc && null == stack.obj_owner) throw new InvalidOperationException("trying to take from actor");
      if (null == stack.inv || !stack.inv.Contains(it)) throw new InvalidOperationException("tried to take "+it.ToString()+" from stack that didn't have it");
#endif
      m_Location = null == stack.loc ? stack.obj_owner.Location : stack.loc.Value;
      if (!Map.Canonical(ref m_Location)) throw new ArgumentNullException(nameof(m_Location));
      m_Item = it;
      m_Container = stack.obj_owner;
    }


    public Item Take { get { return m_Item; } }

    private Inventory? _inv { get { return null != m_Container ? m_Container.Inventory : m_Location.Map.GetItemsAtExt(m_Location.Position); } }
    private Location _loc { get { return null != m_Container ? m_Container.Location : m_Location; } }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      if (!_inv?.Contains(m_Item) ?? true) return false;
      if (m_Actor.Inventory.Contains(m_Item)) return false; // can happen when returning to task
      return m_Actor.CanGet(m_Item, out m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
      return m_Actor.MayTakeFromStackAt(_loc);
    }

    public override void Perform()
    {
#if DEBUG
      if (!m_Actor.MayTakeFromStackAt(in m_Location)) throw new InvalidOperationException(m_Actor.Name + " attempted telekinetic take from " + m_Location + " at " + m_Actor.Location);
#endif
      if (null != m_Container) RogueGame.Game.DoTakeItem(m_Actor, m_Container, m_Item);
      else if (m_Location.Map==m_Actor.Location.Map) RogueGame.Game.DoTakeItem(m_Actor, m_Location.Position, m_Item);    // would fail for cross-district containers
    }

    public override string ToString()
    {
      return m_Actor.Name + " takes " + m_Item;
    }
  } // ActionTakeItem

  [Serializable]
  internal class ActionTake : ActorAction,ActorTake,Target<MapObject?>
  {
    private readonly Gameplay.GameItems.IDs m_ID;
    [NonSerialized] private Item? m_Item;
    [NonSerialized] private InventorySource<Item>? m_InvSrc;

    public ActionTake(Actor actor, Gameplay.GameItems.IDs it) : base(actor)
    {
      m_ID = it;
    }

    public Gameplay.GameItems.IDs ID { get { return m_ID; } }

    public Item? Take { get {
      init();
      return m_Item;
    } }

    public MapObject? What { get {
      init();
      return m_InvSrc.Value.obj_owner;
    } }

    private void init()
    {
      if (null != m_Item && m_InvSrc.Value.inv.Contains(m_Item)) return;

      var stacks = Map.GetAccessibleInventorySources(m_Actor.Location);
      if (null == stacks) return;

      ItemModel model = Gameplay.GameItems.From(m_ID);

      foreach(var stack in stacks) {
        m_Item = stack.inv.GetFirstByModel(model);
        if (null != m_Item) {
          m_InvSrc = stack;
          return;
        }
      }
    }

    private Inventory? _inv { get { return m_InvSrc.Value.inv; } }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      var it = Take;
      if (null == it) {
        m_FailReason = "not in reach";
        return false;
      }
      return m_Actor.CanGet(it, out m_FailReason);
    }

    public override void Perform()
    {
      Item it = Take!;  // cf IsLegal(), above
      m_Actor.Inventory.RejectCrossLink(_inv!);
      RogueGame.Game.DoTakeItem(m_Actor, m_InvSrc.Value);
      _inv?.RejectCrossLink(m_Actor.Inventory);
    }

    public override string ToString()
    {
      return m_Actor.Name + " takes " + m_ID.ToString();
    }
  } // ActionTake

  [Serializable]
  internal class ActionGiveTo : ActorAction,ActorGive,TargetActor
  {
    private readonly Gameplay.GameItems.IDs m_ID;
    private readonly Actor m_Target;
    [NonSerialized] Item? gift;
    [NonSerialized] Item? received;
    [NonSerialized] ActorAction? m_ConcreteAction; // not meant to be Resolvable

    public ActionGiveTo(Actor actor, Actor target, Gameplay.GameItems.IDs it) : base(actor)
    {
      m_ID = it;
      m_Target = target;
    }

    public Actor Whom { get { return m_Target; } }

    public Gameplay.GameItems.IDs ID { get { return m_ID; } }

    public Item? Give { get {
      gift = m_Actor.Inventory.GetBestDestackable(Gameplay.GameItems.From(m_ID)); // force regeneration
      return gift;
    } }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      // can happen if double-executing
      if (null != received && !m_Target.Inventory.Contains(received)) { m_FailReason = "no longer had received"; return false; }
      if (null==Give) { m_FailReason = "not in inventory"; return false; }
      return true;
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
#if DEBUG
      if (!m_Actor.Inventory.Contains(gift)) throw new InvalidOperationException("no longer had gift");
#endif
      m_Target.Inventory.RepairContains(gift, "already had ");
      if (null != m_ConcreteAction) {
        if (m_ConcreteAction.IsPerformable()) return true;
        m_ConcreteAction = null;
      }
      if (!m_Target.IsPlayer && m_Target.Inventory.IsFull && !RogueGame.CanPickItemsToTrade(m_Actor, m_Target, gift)) {
        if (m_Target.CanGet(gift)) return true;
        var recover = (m_Target.Controller as Gameplay.AI.ObjectiveAI).BehaviorMakeRoomFor(gift,m_Actor.Location,false); // unsure if this works cross-map
        if (null == recover) return false;
        if (recover is ActionTradeWithActor trade && trade.Whom == m_Target) {
          if (trade.IsPerformable()) {
            m_ConcreteAction = trade;
            return true;
          }
          return false;
        }

        static Item? parse_recovery(ActorAction act) {
          if (act is Resolvable chain) return parse_recovery(chain.ConcreteAction); // historically ActionChain
          if (act is ActorGive trade) return trade.Give;
          return null;
        }

        if (null!=(received = parse_recovery(recover))) {
#if DEBUG
          if (!m_Target.Inventory.Contains(received)) throw new InvalidOperationException("no longer had recieved");
#endif
          m_Actor.Inventory.RepairContains(received, "already had recieved ");
          return true;
        }

        m_FailReason = "target does not have room in inventory";
#if DEBUG
        throw new InvalidOperationException("tracing");
#else
        return false;
#endif
      }
      return true;
    }

    public override void Perform()
    {
      if (null != m_ConcreteAction) m_ConcreteAction.Perform();
      else RogueGame.Game.DoGiveItemTo(m_Actor, m_Target, gift!, received!);
    }

    public override string ToString()
    {
      return m_Actor.Name + " giving " + m_ID.ToString() + " to " + m_Target.Name;
    }
  } // ActionTake
}
