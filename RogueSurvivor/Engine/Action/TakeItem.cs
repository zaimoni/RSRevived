using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    internal class TakeItem : ActorAction, ActorTake
    {
        private readonly Data.Model.InvOrigin m_src;
        private readonly Item m_Item;
        [NonSerialized] private bool m_Crouching = false;

        public TakeItem(Actor actor, in Location loc, Item it) : base(actor)
        {
#if DEBUG
            if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
            if (!m_Actor.CanGetItems()) throw new InvalidOperationException("!m_Actor.CanGetItems()");
            if (!ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
            m_src = new(loc);
            m_Item = it;
        }

        public TakeItem(Actor actor, ShelfLike shelf, Item it) : base(actor)
        {
#if DEBUG
            if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
            if (!m_Actor.CanGetItems()) throw new InvalidOperationException("!m_Actor.CanGetItems()");
            if (!ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
            m_src = new(shelf);
            m_Item = it;
        }

        public TakeItem(Actor actor, in Data.Model.InvOrigin stack, Item it) : base(actor)
        {
#if DEBUG
            if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
            if (!m_Actor.CanGetItems()) throw new InvalidOperationException("!m_Actor.CanGetItems()");
            if (!ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
            if (null == stack.loc && null == stack.obj_owner) throw new InvalidOperationException("trying to take from actor");
#endif
            m_src = stack;
            m_Item = it;
        }

        public Item Take { get => m_Item; }

        public override bool IsLegal()
        {
            var inv = m_src.Inventory;
            if (null == inv || !inv.Contains(m_Item)) return false;
            m_Actor.Inventory!.RejectContains(m_Item, "have already taken ");
            return m_Actor.CanGet(m_Item, out m_FailReason);
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            var dist = Rules.GridDistance(m_Actor.Location, m_src.Location);
            if (1 < dist) return false;
            m_Crouching = 1 == dist && m_src.IsGroundInventory;
            if (m_Crouching && !m_Actor.CanCrouch()) return false;
            return true;
        }

        public override void Perform()
        {
          m_Actor.SpendActionPoints();
          if (m_Crouching) m_Actor.Crouch();
          if (m_Item is Engine.Items.ItemTrap trap) trap.Desactivate(); // alpha10
          m_src.Inventory.Transfer(m_Item, m_Actor.Inventory);
          if (!m_Item.Model.DontAutoEquip && m_Actor.CanEquip(m_Item) && null == m_Actor.GetEquippedItem(m_Item.Model.EquipmentPart))
            m_Item.EquippedBy(m_Actor);

          RogueGame.Game.UI_TakeItem(m_Actor, m_src.Location, m_Item);
        }
    }
}
