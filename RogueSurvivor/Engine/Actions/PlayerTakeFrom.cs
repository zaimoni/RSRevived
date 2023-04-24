using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
    internal class PlayerTakeFrom : ActorAction
    {
        private readonly InvOrigin m_Src;

        // error checks in ...::create
        private PlayerTakeFrom(PlayerController pc, InvOrigin obj) : base(pc.ControlledActor)
        {
            m_Src = obj;
        }

        public override bool IsLegal() => CanGetFrom(in m_Src, out m_FailReason);

        public override bool IsPerformable()
        {
            var dist = Rules.GridDistance(m_Actor.Location, m_Src.Location);
            if (1 < dist) return false;
            if (1 == dist && null != m_Src.loc && !m_Actor.IsCrouching && !m_Actor.CanCrouch()) return false;
            // \todo make traps dangerous, again (guard clause 1 == dist && null != m_Src.loc + something about trap damage)
            return base.IsPerformable();
        }

        public override void Perform()
        {
            RogueGame.Game.HandlePlayerTakeItem(m_Actor.Controller as PlayerController, m_Src);
        }

        static private string ReasonCantGetFrom(in InvOrigin src)
        {
            if (src.inv.IsEmpty) return "nothing to take there";
            return "";
        }

        static private bool CanGetFrom(in InvOrigin src, out string reason)
        {
            reason = ReasonCantGetFrom(in src);
            return string.IsNullOrEmpty(reason);
        }

        static public PlayerTakeFrom? create(PlayerController pc, Location loc)
        {
            if (!Map.Canonical(ref loc)) return null;
            var inv = loc.Items;
            if (null == inv || inv.IsEmpty) return null;
            return new PlayerTakeFrom(pc, new InvOrigin(loc));
        }

        static public PlayerTakeFrom? create(PlayerController pc, ShelfLike obj)
        {
            if (null == obj || obj.Inventory.IsEmpty) return null;
            return new PlayerTakeFrom(pc, new InvOrigin(obj));
        }
    }
}
