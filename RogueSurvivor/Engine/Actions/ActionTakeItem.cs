// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionTake : ActorAction,ActorTake,Target<MapObject?>
  {
    private readonly Gameplay.Item_IDs m_ID;
    [NonSerialized] private _Action.TakeItem? _resolved = null;

    public ActionTake(Actor actor, Gameplay.Item_IDs it) : base(actor)
    {
      m_ID = it;
    }

    public Gameplay.Item_IDs ID { get { return m_ID; } }

    public Item? Take { get {
      if (null == _resolved) init();
      return _resolved?.Take;
    } }

    public MapObject? What { get {
      if (null == _resolved) init();
      return _resolved?.What;
    } }

    private void init()
    {
      if (null != _resolved) {
        if (!_resolved.IsLegal()) {
          m_FailReason = null;
          _resolved = null;
        }
      }

      var stacks = Map.GetAccessibleInventoryOrigins(m_Actor.Location);
      if (null == stacks) return;

      var model = Gameplay.GameItems.From(m_ID);

      foreach(var stack in stacks) {
        var obj = stack.Inventory!.GetFirstByModel(model);
        if (null == obj) continue;
        if (!m_Actor.IsPlayer && !(m_Actor.Controller as Gameplay.AI.ObjectiveAI).IsInterestingItem(obj)) continue; // XXX temporary, not valid once safehouses are landing
        var act = new _Action.TakeItem(m_Actor, in stack, obj);
        if (!act.IsPerformable()) continue;
        _resolved = act;
        return;
      }
    }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      if (null == _resolved) {
        if (!string.IsNullOrEmpty(m_FailReason)) return false;
        init();
        if (null == _resolved) {
          m_FailReason = "not in reach";
          return false;
        }
      }
      bool ret = _resolved!.IsLegal();
      if (!ret) m_FailReason = _resolved.FailReason;
      return ret;
    }

    public override bool IsPerformable()
    {
      if (null == _resolved) {
        if (!string.IsNullOrEmpty(m_FailReason)) return false;
        init();
        if (null == _resolved) {
          m_FailReason = "not in reach";
          return false;
        }
      }
      bool ret = _resolved!.IsPerformable();
      if (!ret) m_FailReason = _resolved.FailReason;
      return ret;
    }

    public override void Perform()
    {
      _resolved.Perform();
    }

    public override string ToString()
    {
      return m_Actor.Name + " takes " + m_ID.ToString();
    }
  } // ActionTake

  [Serializable]
  internal class ActionGiveTo : ActorAction,ActorGive,TargetActor
  {
    private readonly Gameplay.Item_IDs m_ID;
    private readonly Gameplay.AI.OrderableAI m_Target;
    [NonSerialized] Item? gift;
    [NonSerialized] Item? received;
    [NonSerialized] ActorAction? m_ConcreteAction; // not meant to be Resolvable

    public ActionGiveTo(Actor actor, Gameplay.AI.OrderableAI target, Gameplay.Item_IDs it) : base(actor)
    {
      m_ID = it;
      m_Target = target;
    }

    public Actor Whom { get => m_Target.ControlledActor; }

    public Gameplay.Item_IDs ID { get { return m_ID; } }

    public Item? Give { get {
      gift = m_Actor.Inventory.GetBestDestackable(Gameplay.GameItems.From(m_ID)); // force regeneration
      return gift;
    } }

    // just because it was ok at construction time doesn't mean it's ok now (also used for containers)
    public override bool IsLegal()
    {
      // can happen if double-executing
      if (null != received && !m_Target.ControlledActor.IsCarrying(received)) { m_FailReason = "no longer had received"; return false; }
      if (null==Give) { m_FailReason = "not in inventory"; return false; }
      if (m_Target.ItemIsUseless(gift)) return false;
      return true;
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
#if DEBUG
      if (!m_Actor.IsCarrying(gift)) throw new InvalidOperationException("no longer had gift");
#endif
      var t_actor = m_Target.ControlledActor;
      var t_inv = t_actor.Inventory;
      t_inv.RepairContains(gift, "already had ");
      if (null != m_ConcreteAction) {
        if (m_ConcreteAction.IsPerformable()) return true;
        m_ConcreteAction = null;
      }
      if (t_inv.IsFull && !RogueGame.CanPickItemsToTrade(m_Actor, t_actor, gift)) {
        if (t_actor.CanGet(gift)) return true;
        var recover = m_Target.BehaviorMakeRoomFor(gift,m_Actor.Location,false); // unsure if this works cross-map
        if (null == recover) return false;
        if (recover is ActionTradeWithActor trade && trade.Whom == t_actor) {
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
          if (!t_inv.Contains(received)) throw new InvalidOperationException("no longer had recieved");
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
      else RogueGame.Game.DoGiveItemTo(m_Actor.Controller as Gameplay.AI.OrderableAI, m_Target, gift!, received!);
    }

    public override string ToString()
    {
      return m_Actor.Name + " giving " + m_ID.ToString() + " to " + m_Target.ControlledActor.Name;
    }
  } // ActionTake
}
