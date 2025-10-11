using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
    internal class PlayerTakeFrom : ActorAction
    {
        private readonly Data.Model.InvOrigin m_Src;

        // error checks in ...::create
        private PlayerTakeFrom(PlayerController pc, Data.Model.InvOrigin obj) : base(pc.ControlledActor)
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
            var game = RogueGame.Game;
            var it = game.Choose(m_Src.Inventory, "Taking...");
            if (null != it) game.Interpret(new _Action.TakeItem(m_Actor, in m_Src, it));
        }

        static private string ReasonCantGetFrom(in Data.Model.InvOrigin src)
        {
            if (src.Inventory.IsEmpty) return "nothing to take there";
            return "";
        }

        static private bool CanGetFrom(in Data.Model.InvOrigin src, out string reason)
        {
            reason = ReasonCantGetFrom(in src);
            return string.IsNullOrEmpty(reason);
        }

        static public PlayerTakeFrom? create(PlayerController pc, Location loc)
        {
            if (!Map.Canonical(ref loc)) return null;
            var inv = loc.Items;
            if (null == inv || inv.IsEmpty) return null;
            var obj = loc.MapObject;
            if (null != obj && obj.BlocksReachInto()) return null;
            var player = pc.ControlledActor;
            var p_inv = player.Inventory;
            if (null == p_inv) return null;
            if (p_inv.IsFull) {
                var ok = false;
                foreach (var it in inv) {
                    if (player.CanGet(it)) {
                        ok = true;
                        break;
                    }
                }
                if (!ok) return null;
            }
            return new PlayerTakeFrom(pc, new(loc));
        }

        static public PlayerTakeFrom? create(PlayerController pc, ShelfLike obj)
        {
            var inv = obj?.NonEmptyInventory;
            if (null == inv) return null;
            var player = pc.ControlledActor;
            var p_inv = player.Inventory;
            if (null == p_inv) return null;
            if (p_inv.IsFull) {
                var ok = false;
                foreach (var it in inv) {
                    if (player.CanGet(it)) {
                        ok = true;
                        break;
                    }
                }
                if (!ok) return null;
            }
            return new PlayerTakeFrom(pc, new(obj));
        }

        public override string ToString() => "take from: " + m_Src + "; "+m_Src.Inventory;
    }
}
