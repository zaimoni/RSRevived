using System;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
#nullable enable

    // alpha10
    public sealed class ActionSprayOdorSuppressor : ActorAction, TargetActor, Use<ItemSprayScent>, NotSchedulable
    {
        private readonly ItemSprayScent m_Spray;
        private readonly Actor m_SprayOn;

        public ActionSprayOdorSuppressor(Actor actor, ItemSprayScent spray, Actor sprayOn) : base(actor)
        {
#if DEBUG
            if (!actor.Model.Abilities.CanUseItems) throw new InvalidOperationException("cannot use items");
            if (!actor.Inventory.Contains(spray)) throw new InvalidOperationException("spray not in inventory");
#endif
            m_Spray = spray;
            m_SprayOn = sprayOn
#if DEBUG
                ?? throw new ArgumentNullException(nameof(sprayOn))
#endif
            ;
        }

        public Actor Whom { get => m_SprayOn; }
        public ItemSprayScent Use { get => m_Spray; }

        public override bool IsLegal()
        {
            m_FailReason = ReasonCant(m_Spray) ?? ReasonCant(m_Actor, m_SprayOn);
            return string.IsNullOrEmpty(m_FailReason);
        }

        public override void Perform()
        {
            m_Spray.EquippedBy(m_Actor);
            m_Actor.SpendActionPoints();  // spend AP.
            --m_Spray.SprayQuantity;   // spend spray.
            m_SprayOn.OdorSuppressorCounter += m_Spray.Model.Strength; // add odor suppressor on spray target

            RogueGame.Game.UI_SprayOdorSuppressor(m_Actor, m_Spray, m_SprayOn);
        }

        // AI support
        public static string? ReasonCant(ItemSprayScent suppressor)
        {
            if (Odor.SUPPRESSOR != suppressor.Model.Odor) return "not an odor suppressor";
            if (suppressor.SprayQuantity <= 0) return "No spray left.";

            return null;  // all clear.
        }

        // technically about performability
        public static string? ReasonCant(Actor doer, Actor sprayOn)
        {
            if (sprayOn != doer && !Rules.IsAdjacent(doer.Location, sprayOn.Location)) return "not adjacent";
            return null;  // all clear.
        }
    }
}
