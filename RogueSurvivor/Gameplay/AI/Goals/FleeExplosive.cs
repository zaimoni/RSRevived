﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    internal class FleeExplosive: Objective
    {
        private readonly ZoneLoc m_Fear;

        public FleeExplosive(int t0, Actor who, ZoneLoc avoid) : base(t0, who)
        {
            m_Fear = avoid;
        }

        // we are an influence on behaviors so we don't actually execute
        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            return false;
        }

        // rather than expire on our own cognizance, we let the game engine expire us
        public void ExpireNow() { _isExpired = true; }

        public List<Point>? FilterLegalSteps(Location origin, List<Point>? steps) {
            if (null == steps) return null;
            bool starting_in = m_Fear.ContainsExt(origin);
            Location center = new(m_Fear.m, m_Fear.Rect.Location + new Point(m_Fear.Rect.Width / 2, m_Fear.Rect.Height / 2));
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
            Location center = new(m_Fear.m, m_Fear.Rect.Location + new Point(m_Fear.Rect.Width / 2, m_Fear.Rect.Height / 2));
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
