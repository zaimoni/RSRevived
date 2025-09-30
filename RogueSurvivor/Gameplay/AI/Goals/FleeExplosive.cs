using System;
using System.Collections.Generic;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;

using Point = Zaimoni.Data.Vector2D<short>;
using PrimedExplosive = djack.RogueSurvivor.Data._Item.PrimedExplosive;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    public sealed class FleeExplosive: Objective
    {
        private readonly ZoneLoc m_Fear;
        private PrimedExplosive m_Explosive;

        public FleeExplosive(Actor who, ZoneLoc avoid, PrimedExplosive obj) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
            m_Fear = avoid;
            m_Explosive = obj;
        }

        // we are an influence on behaviors so we don't actually execute
        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            return 0 >= m_Explosive.FuseTimeLeft; // has already exploded
        }

        public List<Point>? FilterLegalSteps(Location origin, List<Point>? steps) {
            if (null == steps) return null;
            bool starting_in = m_Fear.ContainsExt(origin);
            Location center = new(m_Fear.m, m_Fear.Rect.Location + m_Fear.Rect.Size / 2);
            var radius = Rules.GridDistance(center, new Location(m_Fear.m, m_Fear.Rect.Location));
            var start_radius = Rules.GridDistance(center, origin);
            if (radius + 2 <= start_radius) return steps;
            int ub = steps.Count;
            while (0 <= --ub) {
                var loc = new Location(origin.Map, steps[ub]);
                if (starting_in) {
                    if (!m_Fear.ContainsExt(loc)) continue;
                    var new_radius = Rules.GridDistance(center, loc);
                    if (start_radius < new_radius) continue;
                    if (start_radius > new_radius) {
                        steps.RemoveAt(ub);
                        continue;
                    }
                    // ObjectiveAI::BehaviorFleeExplosives uses this, so leave in steps within the explosion, at the edge, when starting
                    // on the edge
                    if (radius > new_radius) {
                        steps.RemoveAt(ub);
                        continue;
                    }
                } else {
                    if (m_Fear.ContainsExt(loc)) {
                        steps.RemoveAt(ub);
                        continue;
                    }
                }
            }
            return 0 <= steps.Count ? steps : null;
        }

        public Dictionary<Location, ActorAction>? FilterLegalPath(Location origin, Dictionary<Location, ActorAction>? path)
        {
            if (null == path) return null;
            bool starting_in = m_Fear.ContainsExt(origin);
            Location center = new(m_Fear.m, m_Fear.Rect.Location + m_Fear.Rect.Size/2);
            var radius = Rules.GridDistance(center, new Location(m_Fear.m, m_Fear.Rect.Location));
            var start_radius = Rules.GridDistance(center, origin);
            if (radius + 2 <= start_radius) return path;
            Dictionary<Location, ActorAction> ret = new();
            foreach (var act in path) {
                if (!m_Fear.ContainsExt(act.Key)) {
                    ret.Add(act.Key, act.Value);
                    continue;
                }
                if (!starting_in) continue;
                var new_radius = Rules.GridDistance(center, act.Key);
                if (radius > new_radius) continue;
                if (radius < new_radius) {
                    ret.Add(act.Key, act.Value);
                    continue;
                }
                // XXX technically a run-step at same radius is ok, but legacy code would catch that first
            }
            return 0 <= ret.Count ? ret : null;
        }
    }
}
