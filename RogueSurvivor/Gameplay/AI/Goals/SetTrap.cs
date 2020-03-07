using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;

using ItemTrap = djack.RogueSurvivor.Engine.Items.ItemTrap;
using Point = Zaimoni.Data.Vector2D_short;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    class SetTrap : Objective
    {
        public SetTrap(int t0, Actor who)
        : base(t0, who)
        {
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            if (_isExpired) return true;
            if (m_Actor.Controller.InCombat) return false;  // suspend if in combat

            Inventory a_inv = m_Actor.Inventory!;
            ItemTrap? trap = null;
            if (!a_inv.IsFull || null == (trap = a_inv.GetFirst<ItemTrap>(tr => 0 < tr.Model.Damage))) { // resolved
                _isExpired = true;
                return true;
            }

            if (IsGoodTrapSpot(m_Actor.Location, out var msg)) {
                ret = BuildTrap(trap, msg);
                return true;
            }

            var candidates = new Dictionary<Point,int>();
            var fov = m_Actor.Controller.FOV;
            foreach (var pt in fov) {
                var loc = new Location(m_Actor.Location.Map, pt);
                if (!Map.Canonical(ref loc)) continue;
                if (IsGoodTrapSpot(loc,out var test)) candidates.Add(pt, Engine.Rules.GridDistance(m_Actor.Location.Position,pt));
            }
            if (0 < candidates.Count) {
                var oai = (m_Actor.Controller as ObjectiveAI)!;
                ret = oai.BehaviorEfficientlyHeadFor(candidates);
                return true;
            }
            _isExpired = true;
            return true;
        }

        // duplicated from BaseAI::IsGoodTrapSpot
        protected bool IsGoodTrapSpot(Location loc, out string? reason)
        {
            reason = null;
            if (loc.Map.IsTrapCoveringMapObjectAt(loc.Position)) return false; // if it won't trigger, waste of a trap.  Other two calls don't have a corresponding object re-fetch.
            // doorway/exit is always a candidate
            bool isInside = loc.Map.IsInsideAt(loc.Position);
            var obj = loc.MapObject;
            if (null != obj) {
                if (obj is Engine.MapObjects.DoorWindow) reason = "protecting the doorway with";   // currently a doorway
            }
            else if (null != loc.Exit) reason = "protecting the exit with";   // exit is ok
            else {
                foreach (var pt in loc.Position.Adjacent()) {
                    var test = new Location(loc.Map, pt);
                    if (!Map.Canonical(ref test)) continue;
                    if (test.Map.IsInsideAt(test.Position) != isInside) {
                        reason = "protecting the building with";  // transition between inside and outside is ok
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(reason)) return false;  // prescreen failed

            // do not feed the Z.  (VAPORWARE: This would not apply to a grenade boobytrap.)
            if (loc.Map.HasCorpsesAt(loc.Position)) return false;

            // trap overpopulation check
            var itemsAt = loc.Items;
            if (null == itemsAt) return true;
            return 3 >= itemsAt.Items.Count(it => it is ItemTrap itemTrap && itemTrap.IsActivated);
        }

        // duplicated from BaseAI::BehaviorBuildTrap
        protected ActorAction BuildTrap(ItemTrap trap, string? reason)
        {
            if (!trap.IsActivated && !trap.Model.ActivatesWhenDropped) return new ActionUseItem(m_Actor, trap);
            if (!string.IsNullOrEmpty(reason)) RogueForm.Game.DoEmote(m_Actor, string.Format("{0} {1}!", reason, trap.AName), true);
            return new ActionDropItem(m_Actor, trap);
        }
    }
}
