using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    class Trade : Objective
    {
        readonly List<Actor> _whom = new List<Actor>(1);

        public Trade(int t0, Actor who, Actor target) : base(t0, who)
        {
            _whom.Add(target);
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            if (_isExpired) return true;
            if (m_Actor.Controller.InCombat) return false;  // suspend if in combat

            // prior legality checks
            int i = _whom.Count;
            while (0 < i) {
                if (_whom[--i].IsDead) {
                    _whom.RemoveAt(i);
                    continue;
                }
            }
            if (0 >= _whom.Count) {
                _isExpired = true;
                return true;
            }

            // closely related to OrderableAI::GetTradingTargets.  We'll use the pathfinder if minstep pathing fails.
            var oai = (m_Actor.Controller as ObjectiveAI)!;
            if (oai.IsFocused) return false;
            var TradeableItems = oai.GetTradeableItems();   // may need revision for player....
            if (null == TradeableItems || 0 >= TradeableItems.Count) {
                _isExpired = true;
                return true;
            }

            i = _whom.Count;
            while (0 < i) {
                var actor = _whom[--i];
                if (!m_Actor.CanTradeWith(actor)) { // XXX \todo CouldTradeWith (less sensitive to current activity) but can't, just continues
                    _whom.RemoveAt(i);
                    continue;
                }
                if (oai.CanSee(actor.Location) && null == m_Actor.MinStepPathTo(m_Actor.Location, actor.Location)) {
                    _whom.RemoveAt(i);
                    continue;
                }
                var a_oai = (actor.Controller as ObjectiveAI)!;
                if (1 == TradeableItems.Count) {
                    var other_TradeableItems = a_oai.GetTradeableItems();
                    if (null == other_TradeableItems) continue;
                    if (1 == other_TradeableItems.Count && TradeableItems[0].Model.ID == other_TradeableItems[0].Model.ID) {
                        _whom.RemoveAt(i);
                        continue;
                    }
                }
                if (a_oai is OrderableAI ai && !ai.HasAnyInterestingItem(TradeableItems)) {
                    _whom.RemoveAt(i);
                    continue;
                }
                if (!oai.HaveTradeOptions(actor)) {
                    _whom.RemoveAt(i);
                    continue;
                }
            }
            if (0 >= _whom.Count) {
                _isExpired = true;
                return true;
            }
            // \todo post-filter by (rest of) CanTradeWith once we have CouldTradeWith
            var adjacent = _whom.FindAll(act => Rules.IsAdjacent(m_Actor.Location, act.Location));
            if (0 < adjacent.Count) {
                var actor = Rules.Get.DiceRoller.Choose(adjacent);
                (oai as OrderableAI)?.MarkActorAsRecentTrade(actor);
                (actor.Controller as OrderableAI)?.MarkActorAsRecentTrade(m_Actor);   // try to reduce trading spam: one trade per pair, not two
                RogueGame.DoSay(m_Actor, actor, string.Format("Hey {0}, let's make a deal!", actor), RogueGame.Sayflags.IS_FREE_ACTION);  // formerly paid AP cost here rather than in RogueGame::DoTrade
                ret = new ActionTrade(m_Actor, actor);
                return true;
            }
            /*
             * Legacy code was doing:
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = near.Value;
             *  */

            var find_us = _whom.Select(act => act.Location);
            var act = oai.BehaviorHeadFor(find_us.Where(loc => oai.CanSee(loc)), false, false);
            if (null != act) {
                ret = act;
                return true;
            }
            act = oai.BehaviorPathTo(new HashSet<Location>(find_us));
            if (null != act) {
                ret = act;
                return true;
            }
            return false;
        }

        public void Add(Actor target) { if (!_whom.Contains(target)) _whom.Add(target); }
        public void Remove(Actor target) { _whom.Remove(target); }
    }
}
