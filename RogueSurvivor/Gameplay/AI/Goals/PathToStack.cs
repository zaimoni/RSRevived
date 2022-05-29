using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // design ancestor: Goal_PathToStack
    [Serializable]
    internal class PathToStack : Objective, LatePathable, PreciseCountermand
    {
        private readonly List<KeyValuePair<InventorySource<Item>, GameItems.IDs>> _stacks = new(1);
        [NonSerialized] private ObjectiveAI oai;
        [NonSerialized] private List<KeyValuePair<InventorySource<Item>, ActorAction>>? _inventory_actions = null;

#if DEAD_FUNC
        public IEnumerable<Inventory> Inventories { get { return _stacks.Select(p => p.Key.inv); } }
        public IEnumerable<Location> Destinations { get { return _stacks.Select(p => p.Key.Location); } }
#endif

        public PathToStack(Actor who, in InventorySource<Item> src, GameItems.IDs take) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            if (!(who.Controller is ObjectiveAI ai)) throw new InvalidOperationException("need an ai with inventory");
            oai = ai;

            KeyValuePair<InventorySource<Item>, GameItems.IDs> stage = new(src, take);
            if (!_stacks.Contains(stage)) _stacks.Add(stage);
        }

        // We are not a primary owning authority for inventories, so don't do the auto-repair before save

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            oai = m_Actor.Controller as ObjectiveAI;
        }

        public void Add(in InventorySource<Item> src, GameItems.IDs take) {
            KeyValuePair<InventorySource<Item>, GameItems.IDs> stage = new(src, take);
            if (!_stacks.Contains(stage)) _stacks.Add(stage);
        }

        private static ActorAction? Take(Actor who, in InventorySource<Item> src, GameItems.IDs what) {
          var take = src.inv.GetWorstDestackable(GameItems.From(what));
          if (null == take) return null;
          bool src_is_inanimate = null != src.loc || null != src.obj_owner;
          if (!who.Inventory!.IsFull) {
            if (src_is_inanimate) return new ActionTakeItem(who, in src, take);
          }
          // will have to trade.
          if (src_is_inanimate) {
            var recover = (who.Controller as ObjectiveAI)?.BehaviorMakeRoomFor(take, in src, false);
            if (recover is ActorGive give) {
              return ActionTradeWith.Cast(src, who, give.Give, take);
            }
          }
          return null;
       }

        /// <returns>true if and only if no stacks remain</returns>
        private bool _removeInvalidStacks()
        {
            _inventory_actions = null;
            var leader = m_Actor.LiveLeader;
            int i = _stacks.Count;
            while (0 < i--) {
                // no longer exists
                var loc = _stacks[i].Key.Location;
                if (m_Actor.Controller.CanSee(in loc)) {
                    if (!_stacks[i].Key.Exists) {
                        leader?.Controller.ReportGone(_stacks[i].Key, m_Actor);
                        _stacks.RemoveAt(i);
                        oai.ClearLastMove(); // unsure if this is needed
                        continue;
                    }
                    // no longer has target
                    if (!_stacks[i].Key.inv.Has(_stacks[i].Value)) {
                        leader?.Controller.ReportNotThere(_stacks[i].Key, _stacks[i].Value, m_Actor);
                        _stacks.RemoveAt(i);
                        oai.ClearLastMove(); // unsure if this is needed
                        continue;
                    }
                    // blocked!
                    if (m_Actor.StackIsBlocked(in loc)) {
                        leader?.Controller.ReportBlocked(_stacks[i].Key, m_Actor);
                        _stacks.RemoveAt(i);
                        oai.ClearLastMove(); // unsure if this is needed
                        continue;
                    }
                }
                // need special handling for Actor on stack, but that may not rate amnesia

                if (!_stacks[i].Key.IsAccessible(m_Actor.Location)) continue;
                var need_override = oai.SetRatingOverride(_stacks[i].Value, 3, 2);
                var act = Take(m_Actor, _stacks[i].Key, _stacks[i].Value);
                if (null != act && act.IsPerformable()) {
                    (_inventory_actions ??= new()).Add(new(_stacks[i].Key, act));
                } else if (null != need_override) {
                    oai.UnsetRatingOverride(_stacks[i].Value, 2);
                    if (0 < need_override.Value.Value) oai.SetRatingOverride(_stacks[i].Value, need_override.Value.Key, need_override.Value.Value);
                }

            }
            return 0 >= _stacks.Count;
        }

        private static ActorTake? ExtractTake(ActorAction act) {
            var ret = act as ActorTake;
            if (null == ret)
            {
                if (act is ActionChain chain) ret = chain.LastAction as ActorTake;
            }
            return ret;
        }

        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;

            if (_removeInvalidStacks()) {
                _isExpired = true;
                return true;
            }

            if (m_Actor.Controller.IsEngaged) return false;

            if (null != _inventory_actions)
            {
                // prefilter
                if (2 <= _inventory_actions.Count)
                {
                    var ub = _inventory_actions.Count;
                    while (1 <= --ub)
                    {
                        var upper_take = ExtractTake(_inventory_actions[ub].Value);
                        if (null == upper_take) continue;
                        var i = ub;
                        while (0 <= --i)
                        {
                            var take = ExtractTake(_inventory_actions[i].Value);
                            if (null == take) continue;
                            if (oai.RHSMoreInteresting(take.Take, upper_take.Take)) {
                                _inventory_actions.RemoveAt(i);
                                break;
                            } else if (oai.RHSMoreInteresting(upper_take.Take, take.Take)) {
                                _inventory_actions.RemoveAt(ub);
                                break;
                            }
                        }
                    }
                }
                ret = _inventory_actions[0].Value;
                var ret_take = ExtractTake(ret);
                if (null != ret_take) {
                    var have = ret_take.Take;
                    m_Actor.LiveLeader?.Controller.ReportTaken(_inventory_actions[0].Key, have, m_Actor);
                }
                m_Actor.Activity = Activity.IDLE;
                _isExpired = true;  // we don't play well with action chains
                return true;
            }

            // let other AI processing kick in before final pathing
            return false;
        }

        public ActorAction Pathing()
        {
            if (_removeInvalidStacks()) return null;
            var _locs = _stacks.Select(p => p.Key.Location).Where(loc => !loc.StrictHasActorAt);
            if (!_locs.Any()) return null;

            var ret = oai.BehaviorPathTo(new HashSet<Location>(_locs));
            return (ret?.IsPerformable() ?? false) ? ret : null;
        }

        public bool HandlePlayerCountermand() { // stub
            return ObjectiveAI.DefaultPlayerCountermand(this);
        }

        public override string ToString()
        {
            string ret = "Pathing to:";
            foreach (var stack in _stacks) {
                ret += "\n"+stack.Key.ToString() + " for " + stack.Value.ToString();
            }
            return ret;
        }
    }
}
