using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using Rectangle = Zaimoni.Data.Box2D<short>;
using Size = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    internal class InferActor : Objective
    {
        private readonly List<Percept_<Actor>> tracked;

        public InferActor(Actor who, Percept_<Actor> target) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            tracked = new() { target };
        }

        // Influence on perception, so never returns an action
        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;

            // Try to expire.
            var ub = tracked.Count;
            while (0 <= --ub) {
                if (tracked[ub].Percepted.IsDead) {
                    tracked.RemoveAt(ub);
                    continue;
                }
                // This space-time scales.
                if (WorldTime.TURNS_PER_HOUR/6 < m_Actor.Location.Map.LocalTime.TurnCounter - tracked[ub].Turn) {
                    tracked.RemoveAt(ub);
                    continue;
                }
            }
            _isExpired = 0 >= tracked.Count;
            return _isExpired;
        }

        public void Track(Percept_<Actor> target) {
            var ub = tracked.Count;
            while (0 <= --ub) {
                if (tracked[ub].Percepted == target.Percepted && tracked[ub].Turn < target.Turn) {
                    tracked[ub] = target;
                    return;
                }
            }

            tracked.Add(target);
        }

        public void Infer(ref List<Percept> dest) {
            List<Percept_<Actor>> needed = new(tracked);

            if (null != dest) {
                var ub = dest.Count;
                while (0 <= --ub)
                {
                    var actor = dest[ub].Percepted as Actor;
                    if (null == actor) continue;

                    var found = needed.FirstOrDefault(x => x.Percepted == actor);
                    if (null == found) continue;
                    needed.Remove(found);
                    if (found.Turn < dest[ub].Turn)
                    { // live sighting
                        tracked.Remove(found);
                        tracked.Add(new(actor, dest[ub].Turn, dest[ub].Location));
                    }
                }
            } else dest = new();

            foreach (var p in needed) {
                if (m_Actor.Controller.CanSee(p.Location)) {
                    // oops, not here
                    ZoneLoc scan = new(p.Location.Map, new Rectangle(p.Location.Position + Direction.NW, (Size)3));
                    var where_is = scan.Listing.Where(loc => !m_Actor.Controller.CanSee(loc) && loc.IsWalkableFor(p.Percepted) && loc.IsWalkableFor(m_Actor));
                    // \todo: handle exits, when usable by both sides
                    tracked.Remove(p);
                    if (!where_is.Any()) continue; // lost track
                    Percept_<Actor> guess = new(p.Percepted, p.Turn, Rules.Get.DiceRoller.Choose(where_is.ToArray()));
                    tracked.Add(guess);
                    dest.Add(new(guess.Percepted, guess.Turn, guess.Location));
                    continue;
                }
                dest.Add(new(p.Percepted, p.Turn, p.Location));
            }
        }
    }
}
