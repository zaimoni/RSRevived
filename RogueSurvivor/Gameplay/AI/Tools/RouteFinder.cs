using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;

using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Gameplay.AI.Tools
{
    // alpha10 new ai helper tool
    /// <summary>
    /// A very simple and restricted pathfinder that just checks if an actor ai can reach a tile.
    /// Only explore tiles that are closer to the goal (mimicking baseai move routine BehaviorBumpToward)
    /// and within a max distance from start.
    ///
    /// Each BaseAI can should use an instance and allocate on demand.
    ///
    /// Not serializable as it is not meant to be persistent!
    ///
    /// Not a static tool class as it would be a nightmare to manage due threads (ais living in the sim thread).
    /// </summary>
    /// <see cref="BaseAI.CanReachSimple(RogueGame, Point, RouteFinder.SpecialActions)"/>
    /// <see cref="BaseAI.BehaviorBumpToward(RogueGame, Point, Func{Point, Point, float})"/>
    public sealed class RouteFinder
    {
        #region Types
        private class Node<T>
        {
            public bool IsVisited;
            public T Pos;
            public int DistToGoal;
        }

        /// <summary>
        /// Allowed actions when blocked by a map object. Will ask game rules for validity.
        /// </summary>
        [Flags]
        public enum SpecialActions : int
        {
            /// <summary>
            /// Can use just regular walk movement.
            /// </summary>
            NONE = 0,

            /// <summary>
            /// Allowed to try to open doors.
            /// </summary>
            DOORS = (1 << 0),

            /// <summary>
            ///  Allowed to try jump on jumpables.
            /// </summary>
            JUMP = (1 << 1),

            /// <summary>
            ///  Allowed to try push pushable objects
            /// </summary>
            PUSH = (1 << 2),

            /// <summary>
            ///  Allowed to try to break objects eg: break blocking doors
            /// </summary>
            BREAK = (1 << 3),

            /// <summary>
            /// Any tile adj to dest is a goal, not only the dest itself.
            /// Doens't need to check can move in the destination.
            /// Eg of goals that dont need to move into the dest proper:
            /// - getting item from a container
            /// - attacking an actor in melee
            /// - operating a generator
            /// etc...
            /// </summary>
            ADJ_TO_DEST_IS_GOAL = (1 << 4)
        }
        #endregion

        #region Fields
        readonly BaseAI m_AI;
        #endregion

        #region Properties
        /// <summary>
        /// Actions the RouteFinder for this AI is allowed to try when checking route to a destination.
        /// Set this depending on:
        /// - the ai or the model, eg: civilians can try to jump and open doors.
        /// - the intent, eg: gangs can try to break stuff to get to items while civilians should not.
        /// </summary>
        /// <see cref="RouteFinder.CanReachSimple(in Location, int, Func{Location, Location, int})"/>
        public SpecialActions AllowedActions { get; set; }
        #endregion

        #region Init
        public RouteFinder(BaseAI ai)
        {
            m_AI = ai;
        }
        #endregion

        #region Checking reachability
        /// <summary>
        /// Can the BaseAI expect to reach this destination by using its simple movement logic BehaviorBumpToward.
        ///
        /// Will explore only tiles :
        /// - within maxDist distance of actor current position.
        /// - that are closer to the destination (mimicking what the ai behaviors do).
        ///
        /// Limiting search space this way ensures that the algorithm :
        /// - isnt too slow for usage by lots of actors
        /// - only search tiles that can realistically be reached by the simple movement behaviors of the AI.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="dest"></param>
        /// <param name="maxDist"></param>
        /// <param name="distanceFn"></param>
        /// <returns></returns>
        /// <see cref="BaseAI.BehaviorBumpToward(RogueGame, Point, Func{Point, Point, float})"/>
        public bool CanReachSimple(in Location dest, int maxDist, Func<Location,Location,int> distanceFn)
        {
            Actor a = m_AI.ControlledActor;
            Location start_loc = a.Location;

            // trivial case of starting adj to dest
            bool adjToDestIsGoal = (AllowedActions & SpecialActions.ADJ_TO_DEST_IS_GOAL) != 0;
            if (distanceFn(start_loc, dest) == 1) {
                if (adjToDestIsGoal) return true;
                return CanMoveIn(RogueGame.Game, a, dest);
            }

            // search similar to A*...
            // expect we don't care about accumulated path cost as we want only to check reachability
            var m_Nodes = new LinkedList<Node<Location>>();
            m_Nodes.AddFirst(new Node<Location>() { IsVisited = false, Pos = start_loc, DistToGoal = distanceFn(start_loc, dest) });
            for (;;) {
                // get most promising node, nodes are sorted by their distance to goal, similar to A*
                var iCurrent = m_Nodes.First;
                for(;;) {
                    if (iCurrent.Value.Pos == dest) return true;  // found goal

                    if (adjToDestIsGoal) {
                        if (distanceFn(iCurrent.Value.Pos, dest) == 1)  return true; // found adj to goal
                    }

                    if (!iCurrent.Value.IsVisited) break;  // visit this one

                    // node already visited, try next one.
                    if (iCurrent.Next == null) return false;  // out of nodes to explore
                    iCurrent = iCurrent.Next;
                }

                // explore this node by looking at adj tiles. 
                // only explore tiles that are not too far, closer to the goal and can be moved in.
                var current = iCurrent.Value;
                current.IsVisited = true;
                int currentDistToGoal = distanceFn(current.Pos, dest);
                foreach (Direction dir in Direction.COMPASS) {
                    var adj = current.Pos + dir;
                    if (!Map.Canonical(ref adj)) continue;
                    int adjDistToGoal = distanceFn(adj, dest);
                    if (adjDistToGoal >= currentDistToGoal || adjDistToGoal > maxDist) continue;

                    if (!CanMoveIn(RogueGame.Game, a, adj)) continue;

                    var adjNode = m_Nodes.FirstOrDefault(n => n.Pos == adj);
                    bool exploreIt = false;
                    if (adjNode == null) {
                        // new one to explore
                        adjNode = new Node<Location>() { IsVisited = false, Pos = adj, DistToGoal = adjDistToGoal };
                        exploreIt = true;
                    } else exploreIt = !adjNode.IsVisited; // explore only if not already explored

                    if (exploreIt) Insert(m_Nodes,adjNode);
                }
            }
        }

        /// <summary>
        /// Insert a new node and keep nodes list sorted on distance to goal.
        /// </summary>
        /// <param name="newNode"></param>
        private void Insert<T>(LinkedList<Node<T>> src, Node<T> newNode)
        {
            var insertBefore = src.First;

            while (insertBefore != null && insertBefore.Value.DistToGoal < newNode.DistToGoal)
                insertBefore = insertBefore.Next;

            if (insertBefore == null) src.AddLast(newNode);
            else src.AddBefore(insertBefore, newNode);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Check if actor can move in this tile.
        /// As per map.IsWalkable() check only tile and mapobject, ignore actors.
        /// "Move in" = walk in or handle the blocking mapobject to make way using one of the allowed actions
        /// Eg: open a door, jump on obj, push obj, break obj.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="a"></param>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// <see cref="Map.IsWalkable(Point)"/>
        /// <see cref="AllowedActions"/>
        bool CanMoveIn(RogueGame game, Actor a, Map map, Point pos)
        {
            // check tile & mapobj
            if (map.IsWalkable(pos)) return true;

            var mobj = map.GetMapObjectAt(pos);
            if (mobj == null) return false;   // blocked by a wall tile

            // blocked by a mapobj, can we operate it?

            //... a door?
            if ((AllowedActions & SpecialActions.DOORS) != 0) {
                if (mobj is DoorWindow door) {
                    // open?
                    if (a.CanOpen(door)) return true;
                    // break?
                    if (((AllowedActions & SpecialActions.BREAK) != 0) && a.CanBreak(door)) return true;
                    // nope, blocked by door
                    return false;
                }
            }

            //... a jumpable?
            if (((AllowedActions & SpecialActions.JUMP) != 0) && mobj.IsJumpable && a.CanJump) return true;

            //... a pushable?
            if (((AllowedActions & SpecialActions.PUSH) != 0) && a.CanPush(mobj)) return true;

            //... a breakable?
            if (((AllowedActions & SpecialActions.BREAK) != 0) && a.CanBreak(mobj)) return true;

            // blocked by a mapobject we can't handle.
            return false;
        }

        // location adapter for above
        bool CanMoveIn(RogueGame game, Actor a, Location loc)
        {
            return Map.Canonical(ref loc) && CanMoveIn(game, a, loc.Map, loc.Position);
        }
        #endregion
    }
}
