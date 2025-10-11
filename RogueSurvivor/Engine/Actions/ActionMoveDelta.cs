using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  // relevant interfaces
  public interface ActorDest
  {
    Location dest { get; }  // of m_Actor
  }

  internal interface ActorOrigin
  {
    Location origin { get; }  // of m_Actor
  }

  internal interface Resolvable
  {
    ActorAction ConcreteAction { get; }
  }

  [Serializable]
  public sealed class ActionMoveDelta : ActorAction,ActorDest,ActorOrigin,Resolvable
  {
    private readonly Location m_NewLocation;
    private readonly Location m_Origin;
    [NonSerialized] private ActorAction _result;    // make this non-serialized if we need to serialize this

	public Location dest { get { return m_NewLocation; } }  // of m_Actor
	public Location origin { get { return m_Origin; } }
    public ActorAction ConcreteAction { get { return _result ?? _resolve(); } }

    public ActionMoveDelta(Actor actor, Location to) : base(actor)
    {
      m_NewLocation = to;
      m_Origin = m_Actor.Location;
#if DEBUG
      if (1!=Rules.InteractionDistance(m_NewLocation,m_Origin)) throw new InvalidOperationException("move delta must be adjacent");
#endif
    }

    public ActionMoveDelta(Actor actor, in Location to, in Location from) : base(actor)
    {
      _Action.MoveStep._Ok(in from, in to, actor);
      m_NewLocation = to;
      m_Origin = from;
    }

    public override bool IsLegal()
    {
      var exit = m_Origin.Exit;
      if (null != exit && exit.Location == m_NewLocation) { // may want to cache this: non-null if needed
        if (!m_Actor.Model.Abilities.AI_CanUseAIExits) return false;
      }
      if (1!=Rules.InteractionDistance(m_Actor.Location, in m_NewLocation)) return true;
      return (_result ??= _resolve())?.IsLegal() ?? false;
    }

    public override bool IsPerformable()
    {
      if (1!=Rules.InteractionDistance(m_Actor.Location, in m_NewLocation)) return false;
      if (!base.IsPerformable()) return false;
      return (_result ??= _resolve())?.IsPerformable() ?? false;
    }

    public override void Perform()
    {
        (_result ??= _resolve())?.Perform();
        _result = null;
        if (1 == Rules.InteractionDistance(m_Actor.Location, in m_NewLocation)) {
            // reschedule ourselves
            (m_Actor.Controller as djack.RogueSurvivor.Gameplay.AI.ObjectiveAI)?.SetObjective(new djack.RogueSurvivor.Gameplay.AI.Goal_NextAction(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, this));
        }
    }

    public override bool AreEquivalent(ActorAction? src)
    {
      return src is ActionMoveDelta alt && m_Origin == alt.m_Origin && m_NewLocation == alt.m_NewLocation;
    }

    // works like pathing rather than bumping
    private ActorAction _resolve()
    {
      ActorAction working = null;
      bool see_dest = m_Actor.Controller.CanSee(in m_NewLocation);
      var obj = see_dest ? m_NewLocation.MapObject : null;
      var actorAt = see_dest ? m_NewLocation.Actor : null;

      { // deal with exits first; cf BaseAI::BehaviorUseExit
      var exit = m_Origin.Exit;
      if (null != exit && exit.Location == m_NewLocation) {
        if (!see_dest || exit.IsNotBlocked(m_Actor)) return new ActionUseExit(m_Actor, in m_Origin);  // all failures of this test require sight information
        if (null != actorAt) return null;   // should be in combat if enemy; don't have good options for allies
        return ActionBreak.schedule(m_Actor, obj);
      }
      }

      if (m_NewLocation.Map.IsWalkableFor(m_NewLocation.Position, m_Actor.Model, out m_FailReason)) working = new ActionMoveStep(m_Actor, in m_NewLocation);

      if (null != actorAt) {
        if (m_Actor.IsEnemyOf(actorAt)) return null; // should be in combat processing
		// player as leader should be able to switch with player as follower
		// NPCs shouldn't be leading players anyway
        if (m_Actor.IsPlayer || !actorAt.IsPlayer) {
          if (m_Actor.CanSwitchPlaceWith(actorAt, out m_FailReason)) return new ActionSwitchPlace(m_Actor, actorAt);
        }

        if (m_Actor.AbleToPush && m_Actor.CanShove(actorAt)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           // we are adjacent due to the early-escape above
           var push_dest = actorAt.ShoveDestinations;
           push_dest.OnlyIf(loc => Gameplay.AI.ObjectiveAI.VetoExit(actorAt, loc.Exit));

           bool push_legal = 1<=push_dest.Count;
           if (push_legal) {
             var self_block = (m_Actor.Controller as Gameplay.AI.ObjectiveAI)?.WantToGoHere(actorAt.Location);
             if (null != self_block && 1==self_block.Count) {
               push_dest.OnlyIf(pt => !self_block.Contains(pt));
               push_legal = 1<=push_dest.Count;
             }
           }
           if (push_legal) {
             bool i_am_in_his_way = false;
             bool i_can_help = false;
             var help_him = (actorAt.Controller as Gameplay.AI.ObjectiveAI)?.WantToGoHere(actorAt.Location);
             if (null != help_him) {
               i_am_in_his_way = help_him.Contains(m_Actor.Location);
               if (push_dest.NontrivialFilter(x => help_him.Contains(x.Key))) push_dest.OnlyIf(pt => help_him.Contains(pt));
               i_can_help = help_him.Contains(push_dest.First().Key);
             }

             var rules = Rules.Get;
             // function target
             var candidates_2 = push_dest.Where(pt => !Rules.IsAdjacent(m_Actor.Location, pt.Key));
             var candidates_1 = push_dest.Where(pt => Rules.IsAdjacent(m_Actor.Location, pt.Key));
             var candidates = (i_can_help && candidates_2.Any()) ? candidates_2.ToList() : null;
             if (null == candidates && !i_am_in_his_way && i_can_help && candidates_1.Any()) candidates = candidates_1.ToList();
             if (null == candidates && i_am_in_his_way) {
               // HMM...maybe step aside instead?
               var considering = m_Actor.MutuallyAdjacentFor(m_Actor.Location,actorAt.Location);
               if (null != considering) {
                 considering = considering.FindAll(pt => pt.IsWalkableFor(m_Actor));
                 if (0 < considering.Count) return new ActionMoveStep(m_Actor, rules.DiceRoller.Choose(considering));
               }
             }

             // legacy initialization
             if (null == candidates && candidates_2.Any()) candidates = candidates_2.ToList();
             if (null == candidates && candidates_1.Any()) candidates = candidates_1.ToList();
             // end function target

             if (null != candidates) return new ActionShove(m_Actor,actorAt, rules.DiceRoller.Choose(candidates).Value);
           }
        }
        // check for mutual-advantage switching place between ais.  Proposing always succeeds, here (unlike pathfinding)
        if (actorAt.Controller is Gameplay.AI.OrderableAI oai && !oai.RejectSwitchPlaces(m_Actor.Location)) {
          return new ActionSwitchPlaceEmergency(m_Actor,actorAt);    // this is an AI cheat so shouldn't be happening that much
        }
      } else if (null != obj) {
           if (obj is DoorWindow door) {
             if (door.BarricadePoints > 0) {
               // pathfinding livings will break barricaded doors (they'll prefer to go around it)
               if (m_Actor.CanBash(door, out m_FailReason)) return new ActionBashDoor(m_Actor, door);
               var act_break = ActionBreak.schedule(m_Actor, door);
               if (null != act_break) {
                 if (act_break.IsPerformable()) return act_break;
                 if ("tired" == act_break.FailReason) return new ActionWait(m_Actor);
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
        // pushing is very bad for bumping, but ok for pathing
        if (m_Actor.AbleToPush && m_Actor.CanPush(obj)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           var push_dest = Map.ValidDirections(obj.Location, loc => {
               // short-circuit language requirement on operator && failed here
               if (!obj.CanPushTo(in loc)) return false;
               if (loc.Map.HasExitAt(loc.Position)) return false;   // pushing onto an exit is very disruptive; may be ok tactically, but not when pathing
               return !loc.Map.PushCreatesSokobanPuzzle(loc.Position, m_Actor);
           });   // does not trivially create a Sokoban puzzle (can happen in police station)

           var rules = Rules.Get;
           bool push_legal = (1 <= push_dest.Count); // always adjacent
           if (push_legal) {
               var self_block = (m_Actor.Controller as Gameplay.AI.ObjectiveAI)?.WantToGoHere(m_Actor.Location);
               if (null != self_block) push_dest.OnlyIf(pt => !self_block.Contains(pt));

               var candidates = Rules.PreferNonAdjacent(push_dest, m_Actor.Location);
               if (null != candidates) {
                 Rules.MaximizeVisibility(ref candidates, m_Actor, obj);
                 return ActionPush.Random(m_Actor, obj);
               }
           }
           // proceed with pull if we can't push safely
           var pull_dests = obj.Location.Adjacent(loc => 1 == Rules.GridDistance(m_Actor.Location, loc));
           if (null != pull_dests) return new ActionPull(m_Actor,obj, rules.DiceRoller.Choose(pull_dests));
         }
         return null;
        }
      // \todo: handle pulling allies instead of just move-stepping, or pulling pushable objects
      return working;
    }

    public override string ToString()
    {
       return "moving: " + m_Origin + " to " + m_NewLocation;
    }
  }

  [Serializable]
  public sealed class UpdateMoveDelta : WorldUpdate, ActorDest, ActorOrigin, CanComplete<Actor>
  {
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;

        public Location dest { get { return m_NewLocation; } }  // of m_Actor
        public Location origin { get { return m_Origin; } }

        public UpdateMoveDelta(Location from, Location to) {
#if DEBUG
            if (1 != Rules.InteractionDistance(in from, in to)) throw new InvalidOperationException("move delta must be adjacent");
#endif
            if (!Map.CanEnter(ref from)) throw new InvalidOperationException("must be able to exist at the origin");
            if (!Map.CanEnter(ref to)) throw new InvalidOperationException("must be able to exist at the destination");
            m_NewLocation = to;
            m_Origin = from;
        }

        public override bool IsLegal() { return true; }
        public override bool IsRelevant() { return false; }
        public override bool IsRelevant(Location loc) { return m_Origin == loc; }
        public override bool IsSuppressed(Actor a)
        {   // \todo more sophisticated response to enemies in sight (might need configuring)
            return null != a.Controller.enemies_in_FOV;
        }

        public bool IsCompleted(Actor src) { return m_NewLocation == src.Location; }

        public override ActorAction? Bind(Actor src) {
            if (!src.CanEnter(m_NewLocation)) return null;
            if (!src.CanEnter(m_Origin)) return null;
            var act = new ActionMoveDelta(src, in m_NewLocation, in m_Origin);
            return act.IsPerformable() ? act : null;
        }

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = Bind(src);
            if (null == act) return null;
            return new(act, null);
        }

        public override void Blacklist(HashSet<Location> goals) {} // intentional no-op; might be incorrect
        public override void Goals(HashSet<Location> goals) { goals.Add(m_Origin); }

        static public List<UpdateMoveDelta>? toDest(Location _dest) {
            if (!Map.CanEnter(ref _dest)) return null;
            List<UpdateMoveDelta> ret = new();
            _dest.ForEachAdjacent(_origin => ret.Add(new UpdateMoveDelta(_origin, _dest)));
            return 0 < ret.Count ? ret : null;
        }

        static public List<UpdateMoveDelta>? fromOrigin(Location _origin, Func<Location, bool> ok) {
            if (!Map.CanEnter(ref _origin)) return null;
            List<UpdateMoveDelta> ret = new();
            _origin.ForEachAdjacent(_dest => {
                if (ok(_dest)) ret.Add(new UpdateMoveDelta(_origin, _dest));
            });
            return 0 < ret.Count ? ret : null;
        }
    }
}
