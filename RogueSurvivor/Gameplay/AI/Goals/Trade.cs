using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    class Trade : Objective, IJsonStaged<Trade>
    {
        readonly List<LivingActor> _whom = new List<LivingActor>(1);
        private new readonly LivingActor m_Actor;   // the same object as the base field

        #region JSON save/load

        private Trade(LivingActor who, JsonElement src) : base(who, src)

        {

            m_Actor = who;
            _whom.AddRange(src.Require("Whom").RequireRefs<LivingActor>());
            // Both parties are NPCs run by OrderableAI, as ActionTrade requires.  The controllers are shells by now (two-phase load).
            if (who.Controller is not OrderableAI) throw new JsonException("Trade: " + who.UnmodifiedName + " is not run by an OrderableAI");
            foreach (var a in _whom) if (a.Controller is not OrderableAI) throw new JsonException("Trade: " + a.UnmodifiedName + " is not run by an OrderableAI");

        }


        static Trade IJsonStaged<Trade>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new Trade(JsonLivingActor(src), src));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            writer.WriteRefs("Whom", _whom);

        }

        #endregion


        public Trade(int t0, LivingActor who, LivingActor target) : base(t0, who)
        {
            m_Actor = who;
            _whom.Add(target);
#if DEBUG
            if (who.Controller is not OrderableAI) throw new InvalidOperationException(who.UnmodifiedName + " is not run by an OrderableAI");
            if (target.Controller is not OrderableAI) throw new InvalidOperationException(target.UnmodifiedName + " is not run by an OrderableAI");
#endif
        }

        /// <summary>Both parties of a trade are NPCs run by OrderableAI, as ActionTrade's constructor requires.</summary>
        static private OrderableAI TraderAI(LivingActor who) => who.Controller as OrderableAI ?? throw new InvalidOperationException(who.UnmodifiedName + " is not run by an OrderableAI");

        public override bool UrgentAction_readonly(out ActorAction? ret)

        {

          ret = null;

          if (_isExpired) return true;

          _whom.RemoveAll(Actor.IsDeceased);

          if (0 >= _whom.Count) _isExpired = true;

          return _isExpired;

        }


        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            if (_isExpired) return true;
            var ordai = OrderableController;
            if (ordai.InCombat) return false;  // suspend if in combat

            // prior legality checks
            int i = _whom.Count;
            while (0 < i) {
                if (_whom[--i].IsDead) {
                    _whom.RemoveAt(i);
                    continue;
                }
                if (ordai.IsActorTabooTrade(_whom[i])) {
                    _whom.RemoveAt(i);
                    continue;
                }
            }
            if (0 >= _whom.Count) {
                _isExpired = true;
                return true;
            }

            // closely related to OrderableAI::GetTradingTargets.  We'll use the pathfinder if minstep pathing fails.
            if (ordai.IsFocused) return false;
            var TradeableItems = ordai.GetTradeableItems();   // may need revision for player....
            if (TradeableItems is null || 0 >= TradeableItems.Count) {
                _isExpired = true;
                return true;
            }

            i = _whom.Count;
            while (0 < i) {
                var actor = _whom[--i];
                if (!ActionTrade.Can(m_Actor, actor)) { // XXX \todo CouldTradeWith (less sensitive to current activity) but can't, just continues
                    _whom.RemoveAt(i);
                    continue;
                }
                if (ordai.CanSee(actor.Location) && m_Actor.MinStepPathTo(m_Actor.Location, actor.Location) is null) {
                    _whom.RemoveAt(i);
                    continue;
                }
                var a_ordai = TraderAI(actor);
                if (1 == TradeableItems.Count) {
                    var other_TradeableItems = a_ordai.GetTradeableItems();
                    if (other_TradeableItems is null) continue;
                    if (1 == other_TradeableItems.Count && TradeableItems[0].ModelID == other_TradeableItems[0].ModelID) {
                        _whom.RemoveAt(i);
                        continue;
                    }
                }
                if (!a_ordai.HasAnyInterestingItem(TradeableItems)) {
                    _whom.RemoveAt(i);
                    continue;
                }
                if (!ordai.HaveTradeOptions(actor)) {
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
                ordai.MarkActorAsRecentTrade(actor);
                TraderAI(actor).MarkActorAsRecentTrade(m_Actor);   // try to reduce trading spam: one trade per pair, not two
                m_Actor.Say(actor, string.Format("Hey {0}, let's make a deal!", actor), Sayflags.IS_FREE_ACTION);  // formerly paid AP cost here rather than in RogueGame::DoTrade
                ret = new ActionTrade(m_Actor, actor);
                return true;
            }

            var find_us = _whom.Select(act => act.Location);
            var find_visible = find_us.Where(loc => ordai.CanSee(loc));
            var act = ordai.BehaviorHeadFor(find_visible, false, false);
            if (act is not null) {
                m_Actor.TargetedActivity(Activity.FOLLOWING, Rules.Get.DiceRoller.Choose(find_visible).Actor); // XXX possibly misleading
                ret = act;
                return true;
            }
            act = ordai.BehaviorPathTo(new HashSet<Location>(find_us));
            if (act is not null) {
                m_Actor.TargetedActivity(Activity.FOLLOWING, Rules.Get.DiceRoller.Choose(_whom)); // XXX possibly misleading
                ret = act;
                return true;
            }
            return false;
        }

        public void Add(LivingActor target) {
#if DEBUG
            if (target.Controller is not OrderableAI) throw new InvalidOperationException(target.UnmodifiedName + " is not run by an OrderableAI");
#endif
            if (!_whom.Contains(target)) _whom.Add(target);
        }
        public void Remove(LivingActor target) { _whom.Remove(target); }
    }
}
