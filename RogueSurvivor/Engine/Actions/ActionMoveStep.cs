// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMoveStep
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionMoveStep : ActorAction
  {
    private Location m_NewLocation;

	public Location dest { get { return m_NewLocation; } }

    public ActionMoveStep(Actor actor, Direction direction)
      : base(actor)
    {
      m_NewLocation = actor.Location + direction;
    }

    public ActionMoveStep(Actor actor, Point to)
      : base(actor)
    {
      m_NewLocation = new Location(actor.Location.Map, to);
    }

    public ActionMoveStep(Actor actor, Location to)
      : base(actor)
    {
      m_NewLocation = to;
    }

    public override bool IsLegal()
    {
      return m_NewLocation.IsWalkableFor(m_Actor, out m_FailReason) && Rules.IsAdjacent(m_Actor.Location, m_NewLocation);
    }

    public override void Perform()
    {
      if (m_Actor.Location.Map==m_NewLocation.Map) RogueForm.Game.DoMoveActor(m_Actor, m_NewLocation);
      else if (m_Actor.Location.Map.District!=m_NewLocation.Map.District) {
        var test = m_Actor.Location.Map.Denormalize(m_NewLocation);
        RogueForm.Game.DoLeaveMap(m_Actor, test.Value.Position, true);
      } else RogueForm.Game.DoLeaveMap(m_Actor, m_Actor.Location.Position, true);
    }

    public override string ToString()
    {
      return "step: "+m_Actor.Location+" to "+m_NewLocation;
    }
  }

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
          // \todo: handle pushing actors
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
         }
      }
      // \todo: handle pulling allies instead of just move-stepping, or pulling pushable objects
      return working;
    }
  }
}
