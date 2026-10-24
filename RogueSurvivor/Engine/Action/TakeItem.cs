using System;
using djack.RogueSurvivor.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine._Action
{
    [Serializable]
    internal class TakeItem : ActorAction, ActorTake, Target<MapObject?>, IJsonStaged<TakeItem>
    {
        private new readonly LivingActor m_Actor;   // the same object as the base field
        private readonly Data.Model.InvOrigin m_src;
        private readonly Item m_Item;
        [NonSerialized] private bool m_Crouching = false;

        #region JSON save/load
        private TakeItem(LivingActor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
        {
            m_Actor = actor;
            m_src = src.Require("Source").Deserialize<Data.Model.InvOrigin>(options);
            m_Item = src.Require("Item").RequireRef<Item>();
        }

        static TakeItem IJsonStaged<TakeItem>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new TakeItem(JsonLivingActor(src), src, options));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            toJsonActor(writer);
            writer.WritePropertyName("Source");
            JsonSerializer.Serialize(writer, m_src, options);
            writer.WriteRef("Item", m_Item);
        }
        #endregion

        public TakeItem(LivingActor actor, in Location loc, Item it) : base(actor)
        {
            m_Actor = actor;
#if DEBUG
            if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
            if (!m_Actor.CanGetItems()) throw new InvalidOperationException("!m_Actor.CanGetItems()");
            if (!m_Actor.IsPlayer && !ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
            m_src = new(loc);
            m_Item = it;
        }

        public TakeItem(LivingActor actor, ShelfLike shelf, Item it) : base(actor)
        {
            m_Actor = actor;
#if DEBUG
            if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
            if (!m_Actor.CanGetItems()) throw new InvalidOperationException("!m_Actor.CanGetItems()");
            if (!ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
#endif
            m_src = new(shelf);
            m_Item = it;
        }

        public TakeItem(LivingActor actor, in Data.Model.InvOrigin stack, Item it) : base(actor)
        {
            m_Actor = actor;
#if DEBUG
            if (actor.Controller is not Gameplay.AI.ObjectiveAI ai) throw new ArgumentNullException(nameof(ai));  // not for a trained dog fetching something
            if (!m_Actor.CanGetItems()) throw new InvalidOperationException("!m_Actor.CanGetItems()");
            if (!actor.IsPlayer && !ai.IsInterestingItem(it)) throw new InvalidOperationException("trying to take not-interesting item"); // XXX temporary, not valid once safehouses are landing
            if (stack.loc is null && stack.obj_owner is null) throw new InvalidOperationException("trying to take from actor");
#endif
            m_src = stack;
            m_Item = it;
        }

        public Item Take { get => m_Item; }
        public MapObject? What { get => m_src.obj_owner; }

        public override bool IsLegal()
        {
            if (!m_src.IsCarrying(m_Item)) {
                m_FailReason = "does not contain";
                return false;
            }
            m_Actor.Inventory.RejectContains(m_Item, "have already taken ");
            return m_Actor.CanGet(m_Item, out m_FailReason);
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            var dist = Rules.GridDistance(m_Actor.Location, m_src.Location);
            if (1 < dist) return false;
            m_Crouching = 1 == dist && m_src.IsGroundInventory;
            if (m_Crouching) {
                if (!m_Actor.CanCrouch(out m_FailReason)) return false;
                var actor = m_src.Location.Actor;
                if (actor is not null) {
                    var allies = m_Actor.Allies;
                    if (allies is null) return false;
                    if (!allies.Contains(actor)) return false;
                }
            }
            return true;
        }

        public override void Perform()
        {
          m_Actor.SpendActionPoints();
          if (m_Crouching) {
            m_Actor.Crouch();
            var code = m_src.Location.OnReachInto(m_Actor);
            if (0 >= code) return; // we took a hit -- cancel taking item
          } else if (m_src.obj_owner is not null) {
            // need to stand to make this work
            m_Actor.StandUp();
          }
          if (m_Item is Engine.Items.ItemTrap trap) trap.Desactivate(); // alpha10
          m_src.Transfer(m_Item, new Data.Model.InvOrigin(m_Actor));
          if (!m_Item.Model.DontAutoEquip && m_Actor.CanEquip(m_Item) && m_Actor.GetEquippedItem(m_Item.Model.EquipmentPart) is null)
            m_Item.EquippedBy(m_Actor);

          RogueGame.Game.UI_TakeItem(m_Actor, m_src.Location, m_Item);
        }

        public override string? ToString() => m_Actor.ToString() +": taking "+ m_Item.ToString() +" from " + m_src.ToString();
    }
}
