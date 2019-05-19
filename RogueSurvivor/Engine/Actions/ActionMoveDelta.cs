using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using Zaimoni.Data;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionMoveDelta : ActorAction
  {
    private Location m_NewLocation;
    private Location m_Origin;
    [NonSerialized] private ActorAction _result;    // make this non-serialized if we need to serialize this

	public Location dest { get { return m_NewLocation; } }
	public Location origin { get { return m_Origin; } }

    public ActionMoveDelta(Actor actor, Location to)
      : base(actor)
    {
      m_NewLocation = to;
      m_Origin = m_Actor.Location;
#if DEBUG
      if (1!=Rules.InteractionDistance(m_NewLocation,m_Origin)) throw new InvalidOperationException("move delta must be adjacent");
#endif
    }

    public ActionMoveDelta(Actor actor, Location to, Location from)
      : base(actor)
    {
      m_NewLocation = to;
      m_Origin = from;
#if DEBUG
      if (1!=Rules.InteractionDistance(m_NewLocation,m_Origin)) throw new InvalidOperationException("move delta must be adjacent");
#endif
    }

    public override bool IsLegal()
    {
      return (_result ?? (_result = _resolve()))?.IsLegal() ?? false;
    }

    public override bool IsPerformable()
    {
      if (1!=Rules.InteractionDistance(m_Actor.Location,m_NewLocation)) return false;
      if (!base.IsPerformable()) return false;
      return (_result ?? (_result = _resolve()))?.IsPerformable() ?? false;
    }

    public override void Perform()
    {
        (_result ?? (_result = _resolve()))?.Perform();
        _result = null;
        if (1 == Rules.InteractionDistance(m_Actor.Location, m_NewLocation)) {
            // reschedule ourselves
            (m_Actor.Controller as djack.RogueSurvivor.Gameplay.AI.ObjectiveAI)?.SetObjective(new djack.RogueSurvivor.Gameplay.AI.Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, this));
        }
    }

    // works like pathing rather than bumping
    private ActorAction _resolve()
    {
      ActorAction working = null;
      if (m_NewLocation.Map.IsWalkableFor(m_NewLocation.Position, m_Actor.Model, out m_FailReason)) working = new ActionMoveStep(m_Actor, m_NewLocation);
      else {
         var obj = m_NewLocation.MapObject;
         var actorAt = m_NewLocation.Actor;
         if (null != actorAt) {
           if (m_Actor.IsEnemyOf(actorAt)) return null; // should be in combat processing
  		   // player as leader should be able to switch with player as follower
		   // NPCs shouldn't be leading players anyway
           if (m_Actor.IsPlayer || !actorAt.IsPlayer) {
             if (m_Actor.CanSwitchPlaceWith(actorAt, out m_FailReason)) return new ActionSwitchPlace(m_Actor, actorAt);
           }

          // check for mutual-advantage switching place between ais
          if (   ((m_Actor.Controller as Gameplay.AI.OrderableAI)?.ProposeSwitchPlaces(actorAt.Location) ?? false)
              && !((actorAt.Controller as Gameplay.AI.OrderableAI)?.RejectSwitchPlaces(m_Actor.Location) ?? true))
            return new ActionSwitchPlaceEmergency(m_Actor, actorAt);
          if (m_Actor.AbleToPush && m_Actor.CanShove(actorAt)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           // we are adjacent due to the early-escape above
           Dictionary<Point,Direction> push_dest = actorAt.ShoveDestinations;

           bool push_legal = 1<=push_dest.Count;
           if (push_legal) {
             Dictionary<Point, int> self_block = (m_Actor.Controller as Gameplay.AI.ObjectiveAI).MovePlanIf(actorAt.Location.Position);
             if (null != self_block) push_dest.OnlyIf(pt => !self_block.ContainsKey(pt));
             push_legal = 1<=push_dest.Count;
           }
           if (push_legal) {
             // function target
             List<KeyValuePair<Point, Direction>> candidates = null;
             IEnumerable<KeyValuePair<Point, Direction>> candidates_2 = push_dest.Where(pt => !Rules.IsAdjacent(m_Actor.Location.Position, pt.Key));
             IEnumerable<KeyValuePair<Point, Direction>> candidates_1 = push_dest.Where(pt => Rules.IsAdjacent(m_Actor.Location.Position, pt.Key));
             if (candidates_2.Any()) candidates = candidates_2.ToList();
             if (null == candidates && candidates_1.Any()) candidates = candidates_1.ToList();
             // end function target

             if (null != candidates) return new ActionShove(m_Actor,actorAt,RogueForm.Game.Rules.DiceRoller.Choose(candidates).Value);
           }
        }
         } else if (null != obj) {
           if (obj is DoorWindow door) {
             if (door.BarricadePoints > 0) {
               // pathfinding livings will break barricaded doors (they'll prefer to go around it)
               if (m_Actor.CanBash(door, out m_FailReason)) return new ActionBashDoor(m_Actor, door);
               if (m_Actor.Model.CanBreak(door, out m_FailReason)) {
                 if (m_Actor.IsTired) return new ActionWait(m_Actor);
                 else if (m_Actor.CanBreak(door, out m_FailReason)) return new ActionBreak(m_Actor, door);
               }
               m_FailReason = "cannot bash the barricade";
               return null;
             }
             if (door.IsClosed) {
               if (m_Actor.CanOpen(door, out m_FailReason)) return new ActionOpenDoor(m_Actor, door);
               if (m_Actor.CanBash(door, out m_FailReason)) return new ActionBashDoor(m_Actor, door);
               return null;
             }
           }
           // \todo: handle pushable objects
        // pushing is very bad for bumping, but ok for pathing
        if (m_Actor.AbleToPush && m_Actor.CanPush(obj)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           Dictionary<Point,Direction> push_dest = obj.Location.Map.ValidDirections(obj.Location.Position, (m, pt) => {
               // short-circuit language requirement on operator && failed here
               if (!obj.CanPushTo(pt)) return false;
               if (m.HasExitAt(pt) && m.IsInBounds(pt)) return false;   // pushing onto an exit is very disruptive; may be ok tactically, but not when pathing
               return !m.PushCreatesSokobanPuzzle(pt, m_Actor);
           });   // does not trivially create a Sokoban puzzle (can happen in police station)

           bool push_legal = (1 <= push_dest.Count); // always adjacent
           if (push_legal) {
               Dictionary<Point, int> self_block = (m_Actor.Controller as Gameplay.AI.ObjectiveAI)?.MovePlanIf(obj.Location.Position);
               if (null != self_block) push_dest.OnlyIf((Predicate<Point>)(pt => !self_block.ContainsKey(pt)));

               // function target
               List<KeyValuePair<Point, Direction>> candidates = null;
               IEnumerable<KeyValuePair<Point, Direction>> candidates_2 = push_dest.Where(pt => !Rules.IsAdjacent(m_Actor.Location.Position, pt.Key));
               IEnumerable<KeyValuePair<Point, Direction>> candidates_1 = push_dest.Where(pt => Rules.IsAdjacent(m_Actor.Location.Position, pt.Key));
               if (candidates_2.Any()) candidates = candidates_2.ToList();
               if (null == candidates && candidates_1.Any()) candidates = candidates_1.ToList();
               // end function target

               if (null != candidates) return new ActionPush(m_Actor,obj,RogueForm.Game.Rules.DiceRoller.Choose(candidates).Value);
           }
           // proceed with pull if we can't push safely
           var possible = obj.Location.Position.Adjacent();
           var pull_dests = possible.Where(pt => 1==Rules.GridDistance(m_Actor.Location,new Location(obj.Location.Map,pt)));
           if (pull_dests.Any()) {
             return new ActionPull(m_Actor,obj,RogueForm.Game.Rules.DiceRoller.Choose(pull_dests));
           }
         }
         return null;
        }
      }
      // \todo: handle pulling allies instead of just move-stepping, or pulling pushable objects
      return working;
    }
  }
}
