using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using Zaimoni.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
  // relevant interfaces
  [Serializable]
  public sealed class ActionMoveDelta : ActorAction,ActorDest,ActorOrigin,Resolvable, IJsonStaged<ActionMoveDelta>
  {
    private readonly Location m_NewLocation;
    private readonly Location m_Origin;
    [NonSerialized] private ActorAction? _result;    // make this non-serialized if we need to serialize this

    public Location dest { get { return m_NewLocation; } }  // of m_Actor
    public Location origin { get { return m_Origin; } }
    public ActorAction? ConcreteAction { get { return _result ?? _resolve(); } }

    #region JSON save/load
    private ActionMoveDelta(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
    {
      m_Origin = src.Require("From").Deserialize<Location>(options);
      m_NewLocation = src.Require("To").Deserialize<Location>(options);
    }

    static ActionMoveDelta IJsonStaged<ActionMoveDelta>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionMoveDelta(JsonActor(src), src, options));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WritePropertyName("From");
      JsonSerializer.Serialize(writer, m_Origin, options);
      writer.WritePropertyName("To");
      JsonSerializer.Serialize(writer, m_NewLocation, options);
    }
    #endregion

    private ActionMoveDelta(Actor actor, in Location to, in Location from) : base(actor)
    {
      m_NewLocation = to;
      m_Origin = from;
    }

    public override bool IsLegal()
    {
      if (1!=Rules.InteractionDistance(m_Actor.Location, in m_NewLocation)) return true;
      return (_result ??= _resolve())?.IsLegal() ?? false;
    }

    public override bool IsPerformable()
    {
      if (!IsLegal()) return false;
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
    private ActorAction? _resolve()
    {
      ActorAction? working = null;
      bool see_dest = m_Actor.Controller.CanSee(in m_NewLocation);
      var obj = see_dest ? m_NewLocation.MapObject : null;
      var actorAt = see_dest ? m_NewLocation.Actor : null;

      const bool tracing = false;

      { // deal with exits first; cf BaseAI::BehaviorUseExit
      var exit = m_Origin.Exit;
      if (exit is not null && exit.Location == m_NewLocation) {
        if (!see_dest || exit.IsNotBlocked(m_Actor)) return new ActionUseExit(m_Actor, in m_Origin);  // all failures of this test require sight information
        if (actorAt is not null) return null;   // should be in combat if enemy; don't have good options for allies
        return ActionBreak.schedule(m_Actor, obj);
      }
      }

      if (m_NewLocation.Map.IsWalkableFor(m_NewLocation.Position, m_Actor.Model, out m_FailReason)) working = ActionMoveStep.schedule(m_Actor, m_NewLocation);

      if (actorAt is not null) {
        if (m_Actor.IsEnemyOf(actorAt)) return null; // should be in combat processing
        var switch_place = _Action.SwitchPlace.create(m_Actor, actorAt);
        // Proposing a switch place always succeeds here, unlike pathfinding
        if (switch_place is not null && !switch_place.RequiresUI()) return switch_place;

        if (m_Actor.AbleToPush && m_Actor.CanShove(actorAt)) {
           // at least 2 destinations: ok (1 ok if adjacent)
           // better to push to non-adjacent when pathing
           // we are adjacent due to the early-escape above
           var push_dest = actorAt.ShoveDestinations;
           push_dest.OnlyIf(loc => !Gameplay.AI.ObjectiveAI.VetoExit(actorAt, loc.Exit));   // not onto an exit the shoved actor would be vetoed from

           bool push_legal = 1<=push_dest.Count;
           if (push_legal) {
             var self_block = (m_Actor.Controller as Gameplay.AI.ObjectiveAI)?.WantToGoHere(actorAt.Location);
             if (self_block is not null && 1==self_block.Count) {
               push_dest.OnlyIf(pt => !self_block.Contains(pt));
               push_legal = 1<=push_dest.Count;
             }
           }
           if (push_legal) {
             bool i_am_in_his_way = false;
             bool i_can_help = false;
             var help_him = (actorAt.Controller as Gameplay.AI.ObjectiveAI)?.WantToGoHere(actorAt.Location);
             if (help_him is not null) {
               i_am_in_his_way = help_him.Contains(m_Actor.Location);
               if (push_dest.NontrivialFilter(x => help_him.Contains(x.Key))) push_dest.OnlyIf(pt => help_him.Contains(pt));
               i_can_help = help_him.Contains(push_dest.First().Key);
             }

             var rules = Rules.Get;
             // function target
             var candidates_2 = push_dest.Where(pt => !Rules.IsAdjacent(m_Actor.Location, pt.Key));
             var candidates_1 = push_dest.Where(pt => Rules.IsAdjacent(m_Actor.Location, pt.Key));
             var candidates = (i_can_help && candidates_2.Any()) ? candidates_2.ToList() : null;
             if (candidates is null && !i_am_in_his_way && i_can_help && candidates_1.Any()) candidates = candidates_1.ToList();
             if (candidates is null && i_am_in_his_way) {
               // HMM...maybe step aside instead?
               var considering = m_Actor.MutuallyAdjacentFor(actorAt.Location);
               if (considering is not null) {
                 considering = considering.FindAll(pt => pt.IsWalkableFor(m_Actor));
                 if (0 < considering.Count) return ActionMoveStep.schedule(m_Actor, rules.DiceRoller.Choose(considering));
               }
             }

             // legacy initialization
             if (candidates is null && candidates_2.Any()) candidates = candidates_2.ToList();
             if (candidates is null && candidates_1.Any()) candidates = candidates_1.ToList();
             // end function target
             if (tracing) {
                if (candidates is not null) Logger.Trace("ActionShove: " + m_Actor.Name);
             }

             if (candidates is not null && switch_place is not null) {
                List<ActorAction> acts_failover = [switch_place];

                List<ActorAction> acts_fork = new();
                foreach(var x in candidates) acts_fork.Add(new ActionShove(m_Actor, actorAt, x.Value));
                acts_failover.Add(1 == acts_fork.Count ? acts_fork[0] : new _Action.Fork(m_Actor, acts_fork));

                return new _Action.Fallback(m_Actor, acts_failover);
             }

             if (candidates is not null) return new ActionShove(m_Actor,actorAt, rules.DiceRoller.Choose(candidates).Value);
           }
        }
      } else if (obj is not null) {
           if (obj is DoorWindow door) {
             if (door.BarricadePoints > 0) {
               // pathfinding livings will break barricaded doors (they'll prefer to go around it)
               var act_bash = ActionBashDoor.schedule(m_Actor, door);
               if (act_bash is not null) return act_bash;
               var act_break = ActionBreak.schedule(m_Actor, door);
               if (act_break is not null) {
                 if (act_break.IsPerformable()) return act_break;
                 if ("tired" == act_break.FailReason) return new ActionWait(m_Actor);
               }
               m_FailReason = "cannot bash the barricade";
               return null;
             }
             if (door.IsClosed) {
               if (m_Actor.CanOpen(door, out m_FailReason)) return new ActionOpenDoor(m_Actor, door);
               return ActionBashDoor.schedule(m_Actor, door);
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
               if (self_block is not null) push_dest.OnlyIf(pt => !self_block.Contains(pt));

               var candidates = Rules.PreferNonAdjacent(push_dest, m_Actor.Location);
               if (candidates is not null) {
                 Rules.MaximizeVisibility(ref candidates, m_Actor, obj);
                 return ActionPush.Random(m_Actor, obj);
               }
           }
           // proceed with pull if we can't push safely
           var pull_dests = obj.Location.Adjacent(loc => 1 == Rules.GridDistance(m_Actor.Location, loc));
           if (pull_dests is not null) return new ActionPull(m_Actor,obj, rules.DiceRoller.Choose(pull_dests));
         }
         return null;
        }
      // \todo: handle pulling allies instead of just move-stepping, or pulling pushable objects
      return working;
    }

    static public ActionMoveDelta? create(Actor actor, in Location to, in Location from)
    {
        var ret = schedule(actor, to, from);
        return (ret is not null && ret.IsPerformable()) ? ret : null;
    }

    static public ActionMoveDelta? create(Actor actor, in Location to)
    {
        var ret = schedule(actor, to);
        return (ret is not null && ret.IsPerformable()) ? ret : null;
    }

    static public ActionMoveDelta? schedule(Actor actor, Location to, Location from)
    {
      if (!actor.Model.CanEnter(ref to)) return null;
      if (!actor.Model.CanEnter(ref from)) return null;
      var exit = from.Exit;
      if (exit is not null && exit.Location == to) { // may want to cache this: non-null if needed
        if (!actor.Model.Abilities.AI_CanUseAIExits) return null;
      }
      if (1!=Rules.InteractionDistance(from, in to)) return null;

      ActionMoveDelta ret = new(actor, to, from);
      return ret.IsLegal() ? ret : null;
    }

    static public ActionMoveDelta? schedule(Actor actor, Location to)
    {
      var from = actor.Location;  // backward compatibility
      var exit = from.Exit;
      if (exit is not null && exit.Location == to) { // may want to cache this: non-null if needed
        if (!actor.Model.Abilities.AI_CanUseAIExits) return null;
      }
      if (!actor.Model.CanEnter(ref to)) return null;
      if (1!=Rules.InteractionDistance(from, in to)) return null;

      ActionMoveDelta ret = new(actor, to, from);
      return ret.IsLegal() ? ret : null;
    }

    public override string ToString()
    {
       return "moving: " + m_Origin + " to " + m_NewLocation;
    }
  }

  [Serializable]
  public sealed class UpdateMoveDelta : WorldUpdate, ActorDest, ActorOrigin, CanComplete<Actor>, IJsonStaged<UpdateMoveDelta>
  {
        private readonly Location m_NewLocation;
        private readonly Location m_Origin;

        public Location dest { get { return m_NewLocation; } }  // of m_Actor
        public Location origin { get { return m_Origin; } }

        #region JSON save/load
        private UpdateMoveDelta(JsonElement src, JsonSerializerOptions options)
        {
            m_Origin = src.Require("From").Deserialize<Location>(options);
            m_NewLocation = src.Require("To").Deserialize<Location>(options);
        }

        static UpdateMoveDelta IJsonStaged<UpdateMoveDelta>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new UpdateMoveDelta(src, options));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("From");
            JsonSerializer.Serialize(writer, m_Origin, options);
            writer.WritePropertyName("To");
            JsonSerializer.Serialize(writer, m_NewLocation, options);
        }
        #endregion

        private UpdateMoveDelta(Location from, Location to) {
#if DEBUG
            if (1 != Rules.InteractionDistance(in from, in to)) throw new InvalidOperationException("move delta must be adjacent");
            if (!Map.CanEnter(ref from)) throw new InvalidOperationException("must be able to exist at the origin");
            if (!Map.CanEnter(ref to)) throw new InvalidOperationException("must be able to exist at the destination");
#endif
            m_NewLocation = to;
            m_Origin = from;
        }

        public override bool IsLegal() { return true; }
        public override bool IsRelevant() { return false; }
        public override bool IsRelevant(Location loc) { return m_Origin == loc; }
        public override bool IsSuppressed(Actor a)
        {   // \todo more sophisticated response to enemies in sight (might need configuring)
            return a.Controller.enemies_in_FOV is not null;
        }

        public bool IsCompleted(Actor src) { return m_NewLocation == src.Location; }

        public override ActorAction? Bind(Actor src) => ActionMoveDelta.create(src, in m_NewLocation, in m_Origin);

        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src)
        {
            var act = Bind(src);
            if (act is null) return null;
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
