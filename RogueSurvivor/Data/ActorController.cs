// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;
using static Zaimoni.Data.Functor;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Data
{
  // The suffix names the class whose ExecAryZeroBehavior implements the behavior.  Was nested in ObjectiveAI.
  public enum ZeroAryBehaviors {
    AttackWithoutMoving_ObjAI = 0,
    WaitIfSafe_ObjAI,
    TurnOnAdjacentGenerators_ObjAI
  };

  [Serializable]
  public abstract class ActorController
  {
    protected readonly Actor m_Actor;

    protected ActorController(Actor src) { m_Actor = src; }

    protected const int FLEE_THROUGH_EXIT_CHANCE = 90;  // alpha10 increased from 50%
    protected const int EMOTE_FLEE_CHANCE = 30;
    protected const int EMOTE_FLEE_TRAPPED_CHANCE = 50;
    protected const int EMOTE_CHARGE_CHANCE = 30;
    private const float MOVE_DISTANCE_PENALTY = 0.42f;
    public const int MAX_EMOTES = 3;    // 0: flee; 1: last stand; 2:charge

    private Location m_prevLocation;
    [NonSerialized] protected RouteFinder m_RouteFinder;    // alpha10
    [NonSerialized] protected List<Percept>? _all;           // cache variables of use to all AI subclasses
    [NonSerialized] protected List<Percept_<Actor>>? _enemies;       // usage varies...Z don't care about "current", OrderableAI subclasses do
    [NonSerialized] private bool _processing;

#region JSON save/load
    // A controller is constructed for an actor.  While an actor loads its controllers, it is here; while it saves them, s_SaveOwner.
    [NonSerialized] public static Actor? s_LoadOwner = null;
    [NonSerialized] public static Actor? s_SaveOwner = null;

    /// <summary>for the subclasses' fromJson</summary>
    static protected Actor JsonOwner { get => s_LoadOwner ?? throw new JsonException("controller without an actor to control"); }

    // An action's or sensor's actor is the one whose controller is being saved or loaded.  Only a different one is written.
    static public void WriteActorUnlessOwner(Utf8JsonWriter writer, Actor src) { if (src != s_SaveOwner) writer.WriteRef("Actor", src); }
    static public Actor ActorOrOwner(JsonElement src) => src.TryGetProperty("Actor", out var a) ? a.RequireRef<Actor>() : (s_LoadOwner ?? throw new JsonException("no actor to own this"));

    /// <summary>for the log of a failed save or load: whose controller it was in, if any</summary>
    static public string? JsonOwnerContext()
    {
      if (s_SaveOwner is not null) return "saving the controller of " + s_SaveOwner.Name;
      if (s_LoadOwner is not null) return "loading the controller of " + s_LoadOwner.Name;
      return null;
    }

    // Two-phase load.  A controller's state refers to things (other maps, other actors' controllers) that may be later in the
    // file, and other controllers' state refers to this controller.  So the actor's load constructs the controller as the live
    // game would, a shell, and the fields are read from the staged JSON once the whole World is in: Session calls LoadPending last.
    [NonSerialized] private static List<KeyValuePair<ActorController, JsonElement>>? s_JsonPending = null;

    /// <summary>for the subclasses' fromJson: registers the shell, and stages its fields</summary>
    static protected T JsonLoaded<T>(JsonElement src, T ret) where T : ActorController
    {
      src.RecordRef(ret);
      (s_JsonPending ??= new()).Add(new(ret, src));
      return ret;
    }

    /// <summary>Reads the staged fields of every controller constructed since the last call, in file order.</summary>
    static public void LoadPending(JsonSerializerOptions options)
    {
      if (s_JsonPending is null) return;
      var pending = s_JsonPending;
      s_JsonPending = null;
      var prior = s_LoadOwner;
      try {
        foreach (var x in pending) {
          s_LoadOwner = x.Key.m_Actor;
          x.Key.loadJsonFields(x.Value, options);
        }
      } finally {
        s_LoadOwner = prior;
      }
    }

    /// <summary>a load that failed part-way: the shells are garbage</summary>
    static public void AbandonPending() => s_JsonPending = null;

    // The layers chain: base first, then their own fields.  The field names are flat in the one object, which is why
    // merging BaseAI into this class (2026-11-08) changed no file.
    public virtual void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      if (m_prevLocation.Map is not null) {
        writer.WritePropertyName("PrevLocation");
        JsonSerializer.Serialize(writer, m_prevLocation, options);
      }
    }

    protected virtual void loadJsonFields(JsonElement src, JsonSerializerOptions options)
    {
      if (src.TryGetProperty("PrevLocation", out var x)) m_prevLocation = x.Deserialize<Location>(options);
    }

    /// <summary>true if and only if a save would write a field of ours; each layer adds its own</summary>
    protected virtual bool HasJsonState => m_prevLocation.Map is not null;

    /// <summary>True if and only if we are indistinguishable, when saved, from what our actor's model would construct.
    /// Our actor then does not write us at all.</summary>
    public bool IsJsonDefault { get => !HasJsonState && GetType() == m_Actor.Model.DefaultController; }
#endregion

    public virtual void RepairLoad() { }

    public Actor ControlledActor { get { return m_Actor; } } // alpha10
    public virtual void TakeControl() {}
    public virtual void LeaveControl() {}

    public virtual Gameplay.GameGangs.IDs GangID { get { return Gameplay.GameGangs.IDs.NONE; } }

#region UI messages
    // forwarder system for to RogueGame::AddMessage
    public virtual void AddMessageForceRead(UI.Message msg, List<PlayerController> witnesses) {
      if (0 < witnesses.Count) {
        bool rendered = false;
        foreach(var witness in witnesses) if (witness.AddMessage(msg)) rendered = true;
        if (!rendered) RogueGame.Game.PanViewportTo(witnesses);
        return;
      }
    }

    public virtual void AddMessageForceReadClear(UI.Message msg, List<PlayerController> witnesses) {
      if (0 < witnesses.Count) {
        bool rendered = false;
        foreach(var witness in witnesses) if (witness.AddMessage(msg)) rendered = true;
        if (!rendered) RogueGame.Game.PanViewportTo(witnesses);
        return;
      }
    }

    // check-in with leader
    public virtual bool ReportBlocked(in Data.Model.InvOrigin src, Actor who) { return true; }
    public virtual bool ReportGone(in Data.Model.InvOrigin src, Actor who) { return true; }
    public virtual bool ReportNotThere(in Data.Model.InvOrigin src, Gameplay.Item_IDs what, Actor who) { return true; }
    public virtual bool ReportTaken(in Data.Model.InvOrigin src, Item it, Actor who) { return true; }
#endregion

    public virtual Zaimoni.Data.Ary2Dictionary<Location, Gameplay.Item_IDs, int>? ItemMemory {
       get {
         return (m_Actor.IsFaction(GameFactions.IDs.ThePolice)) ? Session.Get.Police.ItemMemory : null;
       }
    }

    private bool LastSeen(ref Location x, out int turn) {
      turn = 0;
      var memory = ItemMemory;
      return memory is not null && Map.Canonical(ref x) && memory.HaveEverSeen(x, out turn);
    }
    private bool LastSeen(Location x, out int turn) => LastSeen(ref x, out turn);
    public bool IsKnown(ref Location x) => LastSeen(ref x, out _);
    public bool IsKnown(Location x) => LastSeen(x, out _);

    public void ForceKnown(Point x) {   // for world creation
      var map = m_Actor.Location.Map;
      ItemMemory?.Set(new Location(map, x), null, map.LocalTime.TurnCounter);
    }

    public List<Gameplay.Item_IDs>? WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int>? WhereIs(Gameplay.Item_IDs x) { return ItemMemory?.WhereIs(x); }

    virtual public IEnumerable<Gameplay.Item_IDs>? RejectUnwanted(IEnumerable<Gameplay.Item_IDs>? src, Location loc) { return src; }

    public Dictionary<Location, int>? Filter(Dictionary<Location, int> src, Predicate<Inventory> ok) {
      var it_memory = ItemMemory;
      if (it_memory is null) return src;

      Dictionary<Location, int> ret = new();
      foreach(var x in src) {
        // Cf. LOSSensor::_seeItems
        var allItems = Map.AllItemsAt(x.Key, m_Actor);
        if (allItems is null) {
          it_memory.Set(x.Key, null, x.Key.Map.LocalTime.TurnCounter);   // Lost faith there was anything there
          continue;
        }
        var ub = allItems.Count;
        var loc_ok = false;
        while (0 < ub) {
          var itemsAt = allItems[--ub];
          if (ok(itemsAt.Inventory)) loc_ok = true;
        }
        if (loc_ok) ret.Add(x.Key, x.Value);
        else {
          HashSet<Gameplay.Item_IDs> staging = new(allItems[0].Inventory.Select(x => x.InventoryMemoryID));
          ub = allItems.Count;
          while (1 < ub) {
            var itemsAt = allItems[--ub];
            staging.UnionWith(itemsAt.Inventory.Select(x => x.InventoryMemoryID));
          }
          it_memory.Set(x.Key, staging, x.Key.Map.LocalTime.TurnCounter);   // extrasensory perception update
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public HashSet<Point>? WhereIs(IEnumerable<Gameplay.Item_IDs> src, Map map) {
      var it_memory = ItemMemory;
      if (it_memory is null) return null;
      var ret = new HashSet<Point>();
      bool IsInHere(Location loc) { return loc.Map == map; }
      foreach(var it in src) {
        var tmp = it_memory.WhereIs(it, IsInHere);
        if (tmp is null) continue;
        tmp.OnlyIf(loc => !m_Actor.StackIsBlocked(in loc));
        if (0 >= tmp.Count) continue;
        var it_model = (0 <= (int)it) ? Gameplay.GameItems.From(it) : null; // exclude synthetic item models
        // cheating post-filter: reject boring entertainment
        if (it_model is ItemEntertainmentModel ent) {
          tmp = Filter(tmp, inv => inv.GetFirstByModel<ItemEntertainment>(ent, e => !e.IsBoringFor(m_Actor)) is not null);
          if (tmp is null) continue;
        }
        // cheating post-filter: reject dead flashlights at full inventory (these look useless as items but the type may not be useless)
        if (m_Actor is LivingActor living && living.Inventory.IsFull) {
          if (it_model is ItemLightModel || it_model is ItemTrackerModel) {   // want to say "the item type this model is for, is BatteryPowered" without thrashing garbage collector
            tmp = Filter(tmp, inv => {
              var test = inv.GetFirstByModel(it_model);
              if (test is null) return false;
              if (!test.IsUseless) return true;
              return inv.GetFirstByModel<Item>(it_model, obj => !obj.IsUseless) is not null;
            });
            if (tmp is null) continue;
          }
        }
        ret.UnionWith(tmp.Keys.Select(Location.pos));
      }
      // XXX need to ask allies where hey are headed for (or are), to avoid traffic jams
      return ret;
    }

    public abstract List<Percept> UpdateSensors();

    // vision
    public abstract HashSet<Point> FOV { get; }
    public abstract Location[] FOVloc { get; }
    public virtual void eventFOV() { }

    public abstract Dictionary<Location, Actor>? friends_in_FOV { get; }
    public abstract Dictionary<Location, Actor>? enemies_in_FOV { get; }
    public virtual Dictionary<Location, Data.Model.InvOrigin>? items_in_FOV { get => null; }

    public List<KeyValuePair<double, List<Data.Model.CombatActor>>>? model_now { get {
      List<Data.Model.CombatActor> stage = new();

      var who = friends_in_FOV;
      if (who is not null) foreach(var a in who) stage.Add(new(a.Value));

      who = enemies_in_FOV;
      if (who is not null) foreach(var a in who) stage.Add(new(a.Value));

      if (0 >= stage.Count) return null;

      stage.Sort(Data.Model.CombatActor.CompareAP);
      stage.Insert(0, new(m_Actor));

      List<KeyValuePair<double, List<Data.Model.CombatActor>>> ret = new();
      ret.Add(new(1, stage));
      return ret;
    } }

    public virtual bool IsEngaged { get => enemies_in_FOV is not null; }
    public virtual bool InCombat { get => IsEngaged; }

    /// <returns>null, or an action x for which x.IsPerformable() is true</returns>
    public virtual ActorAction? ExecAryZeroBehavior(ZeroAryBehaviors code) { return null; }

    public bool CanSee(in Location x)  // correctness requires Location being value-copied
    {
      if (x.Map is null) return false;     // convince Duckman to not superheroically crash many games on turn 0
      return 0 <= Array.IndexOf(FOVloc, x);
    }

    // we would like to use the CanSee function name for these, but we probably don't need the overhead for sleeping special cases
    private bool _IsVisibleTo(Map map, Point position)
    {
      Location loc = new (map, position);
      if (!Map.Canonical(ref loc)) return false;
      return 0 <= Array.IndexOf(FOVloc, loc);
    }

    public bool IsVisibleTo(Map? map, in Point position)
    {
      return map is not null && _IsVisibleTo(map, position);    // convince Duckman to not superheroically crash many games on turn 0
    }

    public bool IsVisibleTo(in Location loc)
    {
      return loc.Map is not null && _IsVisibleTo(loc.Map, loc.Position);    // convince Duckman to not superheroically crash many games on turn 0
    }

    public bool IsVisibleTo(Actor actor) => actor == m_Actor || IsVisibleTo(actor.Location);
    public string VisibleIdentity<T>(T x) where T:ILocation,INoun => IsVisibleTo(x.Location) ? x.TheName : x.UnknownPronoun;

    // for _Action.Choice
    public virtual ActorAction Choose(List<ActorAction> src) => Rules.Get.DiceRoller.Choose(src);

    /// <returns>number of turns of trap activation it takes to kill, or int.MaxValue for no known problem</returns>
    public virtual int FastestTrapKill(in Location loc) { return int.MaxValue; }   // z are unaware of deathtraps.  \todo override for dogs

    public Location PrevLocation { get { return m_prevLocation; } }
    public void UpdatePrevLocation() { m_prevLocation = m_Actor.Location; } // for PlayerController

    protected void _initAICache()
    {
      _processing = true;
    }

    protected virtual void ResetAICache()
    {
      _all = null;
      _enemies = null;
      _processing = false;
    }

    public bool IsMyTurn() { return _processing; }

    protected virtual void RecordLastAction(ActorAction act) { }    // no-op at this level

    public virtual ActorAction? GetAction()
    {
      if (m_prevLocation.Map is null) m_prevLocation = m_Actor.Location;
      m_Actor.TargetActor = null;
      var actorAction = SelectAction();
      const bool tracing = false;
#if DEBUG
      if (actorAction is not null && !actorAction.IsPerformable()) throw m_Actor.Spotlight(new InvalidOperationException("illegal action returned from SelectAction"));
#endif
      if (tracing) {
        RogueGame.Game.PanViewportTo(m_Actor);
        RogueGame.Game.InfoPopup(m_Actor.Name+": "+actorAction.ToString());
      }
      if ((this is ObjectiveAI ai)) {
        if (ai.VetoAction(actorAction)) actorAction = new ActionWait(m_Actor);
#if DEBUG
        bool is_rewritable = actorAction is ActionMoveStep || actorAction is ActionMoveDelta || actorAction is ActionWait
                          || actorAction is ActionCloseDoor || actorAction is Resolvable || actorAction is ActionLeaveMap
                          || actorAction is ActionSleep || actorAction is ActionChat || actorAction is ActionPush || actorAction is ActionPull
                          || actorAction is ActionShove;
        var backup = actorAction;
#endif
        var alt = ai.RewriteAction(actorAction);
        if (alt is not null && alt.IsPerformable()) {
#if DEBUG
          if (!is_rewritable && (alt is ActionMoveStep || alt is ActionMoveDelta)) throw new InvalidOperationException("tracing");
#endif
          actorAction = alt;
        }
#if DEBUG
        if (!is_rewritable && (actorAction is ActionMoveStep || actorAction is ActionMoveDelta)) throw new InvalidOperationException("tracing");
#endif
        ai.ScheduleFollowup(actorAction);
      }
      ResetAICache();
      RecordLastAction(actorAction);
      if (!(actorAction is ActionCloseDoor)) m_prevLocation = m_Actor.Location;
      if (tracing) RogueGame.Game.InfoPopup("Enacting: " +actorAction.ToString());
      return actorAction ?? new ActionWait(m_Actor);
    }

    /// <returns>non-null, action x for which x.IsPerformable() is true</returns>
    protected abstract ActorAction? SelectAction();

/*
    NOTE: List<Percept>, as a list data structure, takes O(n) time/RAM to reset its capacity down
    to its real size.  Since C# is a garbage collected language, this would actually worsen
    the RAM loading until the next explicit GC.Collect() call (typically within a fraction of a second).
    The only realistic mitigation is to pro-rate the capacity request.
 */
    // April 22, 2016: testing indicates this does not need micro-optimization
    protected List<Percept_<_T_>>? FilterSameMap<_T_>(IEnumerable<Percept_<_T_>> percepts) where _T_:class
    {
      Map map = m_Actor.Location.Map;
      Func< Percept_ < _T_ >,bool> same_map = p => map.Denormalize(p.Location) is not null;
      var e = m_Actor.Location.Exit;
      if (e is not null) same_map = same_map.Or(p => e.Location==p.Location);
      return percepts.Filter(same_map);
    }

    protected List<Percept_<Actor>>? FilterEnemies(List<Percept>? percepts)
    {
      return percepts?.FilterCast<Actor>(target => target!=m_Actor && m_Actor.IsEnemyOf(target));
    }

    protected List<Percept_<Actor>>? FilterNonEnemies(List<Percept>? percepts)
    {
      return percepts?.FilterCast<Actor>(target => target!=m_Actor && !m_Actor.IsEnemyOf(target));
    }

    protected List<_T_>? FilterCurrent<_T_>(IEnumerable<_T_>? percepts) where _T_: WhereWhen
        {
      return percepts?.FilterCurrent(m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    protected List<_T_>? FilterOld<_T_>(IEnumerable<_T_> percepts) where _T_: WhereWhen
    {
      return percepts?.FilterOld(m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    // GangAI's mugging target selection triggered a race condition
    // that allowed a non-null non-empty percepts
    // to be seen as returning null from FilterNearest anyway, from
    // the outside (Contracts saw a non-null return)
    [return: NotNullIfNotNull("percepts")]
    protected _T_? FilterNearest<_T_>(IEnumerable<_T_>? percepts) where _T_: class,ILocation
    {
      return percepts?.Minimize(p=>Rules.InteractionStdDistance(m_Actor.Location, p.Location));
    }

    public KeyValuePair<Location, T> FilterNearest<T>(Dictionary<Location,T>? src)
    {
      return src?.Minimize(x => Rules.InteractionDistance(m_Actor.Location, x.Key)) ?? default;
    }

    // firearms use grid i.e. L-infinity distance
    [return: NotNullIfNotNull(nameof(percepts))] protected List<_T_>? SortByGridDistance<_T_>(List<_T_>? percepts) where _T_:ILocation
    {
      Location from = m_Actor.Location;
      return percepts?.SortIncreasing(p => Rules.InteractionDistance(p.Location, from));
    }

    // policy change for behaviors: unless the action from a behavior is being used to decide whether to commit to the behavior,
    // a behavior should handle all free actions itself and return only non-free actions.

    /// <param name="exploration">can be null for ais with no exploration</param>
    protected ActorAction? BehaviorWander(ExplorationData? exploration=null, Predicate<Location>? goodWanderLocFn=null)
    {
      int ranking(Location next) {
        int score = 0;

        // alpha10.1
        const int BACKTRACKING = -10000;
        const int AVOID_TRAPS = -1000;
#if OBSOLETE
        const int UNEXPLORED_LOC = 1000;  // should not happen, see below
#endif
        const int DOORWINDOWS = 100;
        const int EXITS = 50;
        const int INSIDE_WHEN_ALMOST_SLEEPY = 100;

        if (next == m_prevLocation) score += BACKTRACKING;
        if (m_Actor.Model.Abilities.IsIntelligent && 0 < next.Map.TrapsUnavoidableMaxDamageAtFor(next.Position,m_Actor)) score += AVOID_TRAPS;

#if OBSOLETE
        // alpha10.1 prefer unexplored/oldest
        // unexplored should not happen because exploration rule is tested before wander rule but just to be more robust...
        if (exploration is not null) {
          int locAge = exploration.GetExploredAge(in next);
          score += (0 == locAge) ? UNEXPLORED_LOC : locAge;
        }
#endif

        // alpha10.1 prefer wandering to doorwindows and exits. 
        // helps civs ai getting stuck in semi-infinite loop when running out of new exploration to do.
        // as a side effect, make ais with no exploration data (eg zombies) more eager to visit door/windows and exits.
        if (next.MapObject is DoorWindow) score += DOORWINDOWS;
        var e = next.Exit;
        if (e is not null) score += (e.Location==m_prevLocation ? BACKTRACKING : EXITS);    // Staying Alive: match fix to BehaviorExplore

        // alpha10.1 prefer inside when almost sleepy
        if (m_Actor.IsAlmostSleepy && next.IsInside) score += INSIDE_WHEN_ALMOST_SLEEPY;

        return score;
      }

      var opts = Direction.COMPASS.Candidates(dir => {
          var act = ActionBump.create(m_Actor, dir);
          if (act is null) return null;
          Location next = act.dest;
          if (goodWanderLocFn is not null && !goodWanderLocFn(next)) return null;
          if (!IsValidWanderAction(act.ConcreteAction)) return null;

          return act;
      });
      if (opts is null) return null;

      var best = opts.KeepMaximal(kv => ranking(kv.Value.dest));
      if (1 == best.Count) return best[0].Key.Value;

      return Rules.Get.DiceRoller.Choose(best).Key.Value;
    }

        // alpha10
        /// <summary>
        /// For intelligent npcs, additional cost to distance cost when chosing which adj tile to bump to.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <see cref="BehaviorBumpToward(RogueGame, Point, bool, bool, Func{Point, Point, float})"/>
        protected float EstimateBumpActionCost(in Location loc, ActorAction action)
        {
            float cost = 0;

            if (action is ActionBump bump) action = bump.ConcreteAction;

            // Consuming additional sta
            if (m_Actor.Model.Abilities.CanTire) {
                // jumping
                if (action is ActionMoveStep) {
                    if (Location.RequiresJump(in loc)) cost = MOVE_DISTANCE_PENALTY;
                }

                // actions that always consume sta or may take more than one turn
                if (action is ActionBreak || action is ActionPush) cost = MOVE_DISTANCE_PENALTY;
            }

            return cost;
        }

    protected ActorAction? BehaviorBumpToward(Point goal, bool canCheckBreak, bool canCheckPush, Func<Point, Point, float> distanceFn)
    {
#if DEBUG
      if (distanceFn is null) throw new ArgumentNullException(nameof(distanceFn));
#endif
      var choiceEval = ChooseExtended(Direction.COMPASS, dir => {
        Location next = m_Actor.Location + dir;
        ActorAction? a = Rules.IsBumpableFor(m_Actor, in next);
        if (a is null) {
          if (m_Actor.Model.Abilities.IsUndead && m_Actor.AbleToPush) {
            var mapObjectAt = next.MapObject;
            if (mapObjectAt is not null && m_Actor.CanPush(mapObjectAt)) {
              var push = ActionPush.Random(m_Actor, mapObjectAt);
              if (push is not null) return push;
            }
          }

          // alpha10 check special actions
          if (canCheckBreak) {
            var act_break = ActionBreak.create(m_Actor, m_Actor.Location.Map.GetMapObjectAt(next.Position));
            if (act_break is not null) return act_break;
          }
          if (canCheckPush) {
            var obj = m_Actor.Location.Map.GetMapObjectAt(next.Position);
            if (obj is not null && m_Actor.CanPush(obj)) {
                // push in a valid direction at random
                List<Direction> validPushes = new List<Direction>(8);
                foreach (Direction pushDir in Direction.COMPASS) {
                  var dest = obj.Location.Position + pushDir;
                  if (obj.Location.Map.IsInBounds(dest) && obj.Location.Map.HasExitAt(in dest)) continue;  // constructor crash
                  if (obj.CanPushTo(dest)) validPushes.Add(pushDir);
                }
                if (validPushes.Count > 0) return new ActionPush(m_Actor, obj, Rules.Get.DiceRoller.Choose(validPushes));
            }
          }

          return null;
        }
        return (next.Position == goal || IsValidMoveTowardGoalAction(a)) ? a : null;
      }, (dir, action) => {
        Location next = m_Actor.Location + dir;
        float cost = distanceFn(next.Position, goal);

        // alpha10 add action cost heuristic if npc is intelligent
        if (!float.IsNaN(cost) && m_Actor.Model.Abilities.IsIntelligent) cost += EstimateBumpActionCost(in next, action);

        return cost;
      }, (a, b) => a < b);
      if (choiceEval is not null) return choiceEval.Choice;
      return null;
    }

    protected ActorAction? BehaviorStupidBumpToward(Point goal, bool canCheckBreak, bool canCheckPush)
    {
      return BehaviorBumpToward(goal, canCheckBreak, canCheckPush, (ptA, ptB) => {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(in ptA, in ptB);
        if (!m_Actor.Location.Map.IsWalkableFor(ptA, m_Actor)) num += MOVE_DISTANCE_PENALTY;
        return num;
      });
    }

    protected ActorAction? BehaviorStupidBumpToward(in Location goal, bool canCheckBreak, bool canCheckPush)
    {
      var map = m_Actor.Location.Map;
      if (map == goal.Map) return BehaviorStupidBumpToward(goal.Position, canCheckBreak, canCheckPush);
      Location? test = map.Denormalize(in goal);
      if (test is null) return null;
      return BehaviorStupidBumpToward(test.Value.Position, canCheckBreak, canCheckPush);
    }

    protected ActorAction? BehaviorIntelligentBumpToward(Point goal, bool canCheckBreak, bool canCheckPush)
    {
      const float MOVE_INTO_TRAPS_PENALTY = 1;  // alpha10

      float currentDistance = (float)Rules.StdDistance(m_Actor.Location.Position, in goal);
      Func<Point,Point,float> close_in = (ptA,ptB) => {
        if (ptA == ptB) return 0.0f;
        float num = (float)Rules.StdDistance(in ptA, in ptB);
        return (num >= currentDistance) ? float.NaN : num;
      };

      ActorCourage courage = (this as OrderableAI)?.Directives_nocreate?.Courage ?? ActorDirective.Courage_default;
      bool imStarvingOrCourageous = m_Actor.IsStarving || ActorCourage.COURAGEOUS == courage;
      if (imStarvingOrCourageous) close_in = close_in.Postprocess((ptA,ptB,dist) => {
          var loc = new Location(m_Actor.Location.Map, ptA);
          if (!Map.Canonical(ref loc)) return float.NaN;
          var turns_to_fatality = m_Actor.Controller.FastestTrapKill(in loc);
          if (1 >= turns_to_fatality) return float.NaN;
          if (int.MaxValue > turns_to_fatality) return dist + MOVE_INTO_TRAPS_PENALTY;
          return dist;
      });

      return BehaviorBumpToward(goal, canCheckBreak, canCheckPush, close_in);
    }

    protected ActorAction? BehaviorIntelligentBumpToward(in Location goal, bool canCheckBreak, bool canCheckPush)
    {
      var map = m_Actor.Location.Map;
      if (map == goal.Map) return BehaviorIntelligentBumpToward(goal.Position, canCheckBreak, canCheckPush);
      Location? test = map.Denormalize(in goal);
      if (test is null) return null;
      return BehaviorIntelligentBumpToward(test.Value.Position, canCheckBreak, canCheckPush);
    }

    protected ActorAction? BehaviorHeadFor(in Location goal, bool canCheckBreak, bool canCheckPush)
    {
      if (m_Actor.Model.Abilities.IsIntelligent) return BehaviorIntelligentBumpToward(in goal, canCheckBreak, canCheckPush);
      return BehaviorStupidBumpToward(in goal, canCheckBreak, canCheckPush);
    }

    // A number of the melee enemy targeting sequences not only work on grid distance,
    // they need to return a coordinated action/target pair.
    // We assume the list is sorted in increasing order of grid distance
    protected ActorAction? TargetGridMelee(List<Percept_<Actor>>? perceptList)
    {
      if (perceptList is null) return null; // inefficient, but reduces lines of code elsewhere
      foreach (var percept in perceptList) {
        var tmp = BehaviorStupidBumpToward(percept.Location, true, true);
        if (tmp is not null) {
          m_Actor.TargetedActivity(Activity.CHASING, percept.Percepted);
          return tmp;
        }
      }
      return null;
    }

    protected ActionMeleeAttack? BehaviorMeleeAttack(Actor target)
    {
      return (ActionMeleeAttack.Can(m_Actor, target) ? new ActionMeleeAttack(m_Actor, target) : null);
    }

    /// <returns>null, or a non-free action</returns>
    protected ActorAction? BehaviorEquipWeapon()
    {
      // One of our callers is InsaneHumanAI::SelectAction.  As this AI is always insane, it does not trigger
      // random insane actions which could pick up ranged weapons.
      // Thus, no AI that calls this function has a usable firearm in inventory.

      var tmp_a = m_Actor;
      var equippedWeapon = tmp_a.GetEquippedWeapon();
      var bestMeleeWeapon = tmp_a.GetBestMeleeWeapon();   // rely on OrderableAI doing the right thing

      if (bestMeleeWeapon is null) {
        equippedWeapon?.UnequippedBy(tmp_a);    // unusable ranged weapon
        return null;
      }
      if (equippedWeapon != bestMeleeWeapon) bestMeleeWeapon.EquippedBy(tmp_a);
      return null;
    }

    protected virtual ActorAction? BehaviorFollowActor(Actor? other, int maxDist)
    {
      if (other is null || other.IsDead) return null;
      int num = Rules.GridDistance(m_Actor.Location, other.Location);
      if (CanSee(other.Location) && num <= maxDist) return new ActionWait(m_Actor);
      if (other.Location.Map != m_Actor.Location.Map) {
        var exitAt = m_Actor.Location.Exit;
        if (exitAt is not null && exitAt.ToMap == other.Location.Map && m_Actor.CanUseExit())
          return BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      var actorAction = BehaviorIntelligentBumpToward(other.Location, false, false);
      if (actorAction is null || !actorAction.IsPerformable()) return null;
      if (other.IsRunning) m_Actor.Run();
      return actorAction;
    }

    protected ActorAction? BehaviorTrackScent(List<Percept_s<AIScent>> scents)
    {
      if (0 >= scents.Count) return null;
      var percept = scents.Maximize(scent => scent.Percepted.Strength)!;
      if (m_Actor.Location != percept.Location) {
        var tmpAction = BehaviorIntelligentBumpToward(percept.Location, false, false);
        if (tmpAction is not null) return tmpAction;
        var dir = Direction.FromVector(percept.Location.Position-m_Actor.Location.Position);
        // Cf. Angband.
        if (dir is not null) {
          if (Rules.Get.DiceRoller.RollChance(50)) { // anti-clockwise bias
            tmpAction = BehaviorIntelligentBumpToward(percept.Location+dir.Left, false, false);
            if (tmpAction is not null) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location+dir.Right, false, false);
            if (tmpAction is not null) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Left.Left, false, false);
            if (tmpAction is not null) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Right.Right, false, false);
            if (tmpAction is not null) return tmpAction;
          } else {  // clockwise bias
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Right, false, false);
            if (tmpAction is not null) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Left, false, false);
            if (tmpAction is not null) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Right.Right, false, false);
            if (tmpAction is not null) return tmpAction;
            tmpAction = BehaviorIntelligentBumpToward(percept.Location + dir.Left.Left, false, false);
            if (tmpAction is not null) return tmpAction;
          }
        }
        return null;
      }
      if (m_Actor.Model.Abilities.AI_CanUseAIExits) {
          var e = m_Actor.Location.Exit;
          if (e is not null) return BehaviorUseExit(e, UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
      }
      return null;
    }

    protected virtual ActorAction? BehaviorChargeEnemy(Percept_<Actor> target, bool canCheckBreak, bool canCheckPush)
    {
      Actor actor = target.Percepted;
      ActorAction? tmpAction = BehaviorMeleeAttack(actor);
      // XXX there is some common post-processing we want done regardless of the exact path.  This abuse of try-catch-finally probably is a speed hit.
      try {
        if (tmpAction is not null) return tmpAction;
        if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, target.Location)) return new ActionWait(m_Actor);
        tmpAction = BehaviorHeadFor(target.Location, canCheckBreak, canCheckPush);
        if (tmpAction is null) return null;
        if (m_Actor.CurrentRangedAttack.Range < actor.CurrentRangedAttack.Range) m_Actor.Run();
        return tmpAction;
      } catch(Exception) {
        throw;
      } finally {
        if (tmpAction is not null) m_Actor.TargetedActivity(Activity.FIGHTING, actor);
      }
    }

    // the intelligent callers are using the override at OrderableAI
    protected virtual ActorAction? BehaviorExplore(ExplorationData exploration)
    {
      Direction? prevDirection = Direction.FromVector(m_Actor.Location.Position - m_prevLocation.Position);

      int ranking(Direction dir, Location loc) {
        const int EXPLORE_ZONES = 1000;
        const int EXPLORE_BARRICADES = ExplorationData.SCORE_BARRICADES;
        const int EXPLORE_INOUT = ExplorationData.SCORE_INOUT;
        const int EXPLORE_DIRECTION = 25;

//      if (exploration.HasExplored(loc)) return float.NaN;
        Map map = loc.Map;
        Point position = loc.Position;
        int num = 0;
        if (!exploration.HasExplored(map, map.GetZonesAt(position))) num += EXPLORE_ZONES;
        num += exploration.AgeScore(in loc);
        var mapObjectAt = map.GetMapObjectAt(position);
        // this is problematic when the door is the previous location.  Do not overwhelm in/out
        if (mapObjectAt is not null && (mapObjectAt.IsMovable || mapObjectAt is DoorWindow)) {
          num += (loc != PrevLocation ? EXPLORE_BARRICADES : -EXPLORE_DIRECTION);
        }
        if (map.IsInsideAtExt(position)) {
          if (map.LocalTime.IsNight) num += EXPLORE_INOUT;
        }
        else if (!map.LocalTime.IsNight) num += EXPLORE_INOUT;
        if (dir == prevDirection) num += EXPLORE_DIRECTION;
        return num;
      }

      var opts = Direction.COMPASS.Candidates(dir => {
          var act = ActionBump.create(m_Actor, dir);
          if (act is null) return null;
          if (!IsValidMoveTowardGoalAction(act.ConcreteAction)) return null;

          return act;
      });
      if (opts is null) return null;

      var best = opts.KeepMaximal(kv => ranking(kv.Key, kv.Value.dest));
      if (1 == best.Count) return best[0].Key.Value;

      // \todo --faust option effects
      if (false /* Engine.Session.Get.CMDoptionExists("faust") */) {
        var PCs = RogueGame.PCsNearby(m_Actor.Location, RogueGame.VIEW_RADIUS, TRUE);
        if (PCs is not null) {
          RogueGame.FaustClairvoyance(m_Actor, PCs.Select(pc => pc.ControlledActor.Location));
          var act = RogueGame.Game.AI_prayer(m_Actor, best.Keys().Values());
          if (act is not null) return act;
        }
      }

      return Rules.Get.DiceRoller.Choose(best).Key.Value;
    }

    private ActorAction? BehaviorUseExit(Exit exitAt, UseExitFlags useFlags)
    {
      if ((useFlags & UseExitFlags.DONT_BACKTRACK) != UseExitFlags.NONE && exitAt.Location == m_prevLocation) return null;
      if (exitAt.IsNotBlocked(out var actorAt, out var mapObjectAt, m_Actor)) return (m_Actor.CanUseExit() ? new ActionUseExit(m_Actor, m_Actor.Location) : null);
      if ((useFlags & UseExitFlags.ATTACK_BLOCKING_ENEMIES) != UseExitFlags.NONE) {
        if (actorAt is not null && m_Actor.IsEnemyOf(actorAt) && ActionMeleeAttack.Can(m_Actor, actorAt))
          return new ActionMeleeAttack(m_Actor, actorAt);
      }
      if ((useFlags & UseExitFlags.BREAK_BLOCKING_OBJECTS) != UseExitFlags.NONE) {
        var act_break = ActionBreak.create(m_Actor, mapObjectAt);
        if (act_break is not null) return act_break;
      }
      return null;
    }

    protected ActorAction? BehaviorUseExit(UseExitFlags useFlags) {
      var e = m_Actor.Location.Exit;
      if (e is null) return null;
      return BehaviorUseExit(e, useFlags);
    }

    protected ActorAction? BehaviorGoEatCorpse(List<Percept>? percepts)
    {
#if false
      if (!Session.Get.HasCorpses || !ActionEatCorpse.WantTo(m_Actor)) return null;
#else
      if (!Session.HasCorpses || !ActionEatCorpse.WantTo(m_Actor)) return null;
#endif
            var corpsesPercepts = percepts?.FilterCast<List<Corpse>>();
      if (corpsesPercepts is null) return null;
      m_Actor.Activity = Activity.IDLE;
      var percept = FilterNearest(corpsesPercepts);
      if (m_Actor.Location == percept.Location) return new ActionEatCorpse(m_Actor, percept.Percepted[0]);
      return BehaviorHeadFor(percept.Location,true,true);
    }

    /// <summary>
    /// Compute safety from a list of dangers at a given position.
    /// </summary>
    /// <param name="from">position to compute the safety</param>
    /// <param name="dangers">dangers to avoid</param>
    /// <returns>a heuristic value, the higher the better the safety from the dangers</returns>
    protected float SafetyFrom(Point from, IEnumerable<Point> dangers)
    {
      Map map = m_Actor.Location.Map;

      // Heuristics:
      // Primary: Get away from dangers.
      // Weighting factors:
      // 1 Avoid getting in corners.
      // 2 Prefer going outside/inside if majority of dangers are inside/outside.
      // 3 If can tire, prefer not jumping.
#region Primary: Get away from dangers.
      float avgDistance = (float) (dangers.Sum(pt => Rules.GridDistance(in from, in pt))) / (1 + dangers.Count());
#endregion
#region 1 Avoid getting in corners.
      int countFreeSquares = map.CountAdjacentTo(from,pt => pt == m_Actor.Location.Position || map.IsWalkableFor(pt, m_Actor));
      float avoidCornerBonus = countFreeSquares * 0.1f;
#endregion
#region 2 Prefer going outside/inside if majority of dangers are inside/outside.
      bool isFromInside = map.IsInsideAtExt(from);
      int majorityDangersInside = dangers.ExcessTrue(pt => map.IsInsideAtExt(pt));
      const float inOutFactor = 1.25f;
      float inOutBonus = 0.0f;
      if (isFromInside) {
        if (majorityDangersInside < 0) inOutBonus = inOutFactor;
      }
      else if (majorityDangersInside > 0) inOutBonus = inOutFactor;
#endregion
#region 3 If can tire, prefer not jumping.
      float jumpPenalty = 0.0f;
      if (m_Actor.Model.Abilities.CanTire && m_Actor.Model.Abilities.CanJump) {
        var mapObjectAt = map.GetMapObjectAtExt(from);
        if (mapObjectAt is not null && mapObjectAt.IsJumpable) jumpPenalty = 0.1f;
      }
#endregion
      float heuristicFactorBonus = 1f + avoidCornerBonus + inOutBonus - jumpPenalty;
      return avgDistance * heuristicFactorBonus;
    }

    static protected ChoiceEval<_T_>? Choose<_T_>(IEnumerable<_T_>? listOfChoices, Func<_T_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (listOfChoices is null || !listOfChoices.Any()) return null;

      float num = float.NaN;
      List<ChoiceEval<_T_>> candidates = new List<ChoiceEval<_T_>>();
      foreach(_T_ tmp in listOfChoices) {
        float f = evalChoiceFn(tmp);
        if (float.IsNaN(f)) continue;   // NaN is not valid
        if (float.IsNaN(num) || isBetterEvalThanFn(f, num)) {
          num = f;
          candidates.Clear();
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;
        candidates.Add(new ChoiceEval<_T_>(tmp, f));
      }
      if (0 >= candidates.Count) return null;
      return Rules.Get.DiceRoller.Choose(candidates);
    }

    // isBetterThanEvalFn will never see NaN
    static protected ChoiceEval<_DATA_>? ChooseExtended<_T_, _DATA_>(IEnumerable<_T_>? listOfChoices, Func<_T_, _DATA_> isChoiceValidFn, Func<_T_, _DATA_, float> evalChoiceFn, Func<float, float, bool> isBetterEvalThanFn)
    {
      if (listOfChoices is null || !listOfChoices.Any()) return null;

      Dictionary<float, List<ChoiceEval<_DATA_>>> choiceEvalDict = new Dictionary<float, List<ChoiceEval<_DATA_>>>();

      float num = float.NaN;
      foreach(_T_ tmp in listOfChoices) {
        _DATA_ choice = isChoiceValidFn(tmp);
        if (choice is null) continue;
        float f = evalChoiceFn(tmp, choice);
        if (float.IsNaN(f)) continue;
        if (float.IsNaN(num) || isBetterEvalThanFn(f, num)) {
          num = f;
          // XXX at our scale we shouldn't need to early-enable garbage collection here
        } else if (num != f) continue;

        ChoiceEval< _DATA_ > tmp2 = new ChoiceEval<_DATA_>(choice, f);
        if (choiceEvalDict.TryGetValue(f,out var dest))  dest.Add(tmp2);
        else choiceEvalDict.Add(f, new List<ChoiceEval<_DATA_>>{ tmp2 });
      }

      if (!choiceEvalDict.TryGetValue(num, out var ret_from)) return null;
      return Rules.Get.DiceRoller.Choose(ret_from);
    }

    protected bool IsValidFleeingAction([NotNullWhen(true)] ActorAction? a)
    {
      if (a is null) return false;
      if (a is Resolvable res) return IsValidFleeingAction(res.ConcreteAction);
      if (a is Engine._Action.SwitchPlace swap) return swap.LeaderSwitchPlace;
      return a is ActionMoveStep || a is ActionOpenDoor;
    }

    protected bool IsValidWanderAction(ActorAction? a)
    {
      if (a is null) return false;
      if (a is ActionMoveStep) return true;
      if (a is Engine._Action.SwitchPlace swap) return swap.LeaderSwitchPlace;
//    if (a is ActionPush) return true; // wasn't being generated in RS alpha 9...results do not look good
      if (a is ActionOpenDoor) return true;
      // historically, zombies were allowed to bash doors, but livings were not allowed to break barricades or doors as wandering actions
      if (a is ActionBreak act_break && act_break.What is DoorWindow && a.Actor.str_CannotBash is null) return true;
      if (a is ActionBarricadeDoor) return true;
      if (this is OrderableAI ordai) {
        if (a is ActionChat chat) {
          return (ordai.Directives_nocreate?.CanTrade ?? ActorDirective.CanTrade_default ) || chat.Target == (m_Actor as LivingActor)?.Leader;
        }
      }
      return false;
    }

    protected bool IsValidMoveTowardGoalAction(ActorAction? a)
    {
      if (a is null) return false;
      if (a is Resolvable res) return IsValidMoveTowardGoalAction(res.ConcreteAction);
      // blacklist actions inappropriate for bump movement
      if (a is ActionChat) return false;
      if (a is PlayerTakeFrom) return false;
      if (a is ActionRechargeItemBattery) return false;
      if (a is Engine._Action.SwitchPlace swap) return swap.LeaderSwitchPlace;
      if (a is ActionSwitchPowerGenerator) return false;
      return true;
    }

    protected bool WantToEvadeMelee(Actor actor, ActorCourage courage, Actor target)
    {
//    if (WillTireAfterAttack(actor)) return true;  // post-process this, handling this here is awful for rats
      if (actor.Speed > target.Speed) {
        if (actor.WillActAgainBefore(target)) return false; // caller must handle distance 2 correctly.
        if (Rules.IsAdjacent(actor.Location,target.Location) && Actor.STAMINA_MIN_FOR_ACTIVITY<=target.StaminaPoints) return true;  // back-and-smack indicated
//      if (target.TargetActor == actor) return true;
        return false;
      }
      var weakerInMelee = FindWeakerInMelee(m_Actor, target);
      return weakerInMelee != target && (weakerInMelee == m_Actor || courage != ActorCourage.COURAGEOUS);
    }

    private static Actor? FindWeakerInMelee(Actor a, Actor b)
    {
      int a_dam = a.MeleeAttack(b).DamageValue - b.CurrentDefence.Protection_Hit;
      int b_dam = b.MeleeAttack(a).DamageValue - a.CurrentDefence.Protection_Hit;
      if (Actor.STAMINA_MIN_FOR_ACTIVITY > a.StaminaPoints && Actor.STAMINA_MIN_FOR_ACTIVITY <= b.StaminaPoints) a_dam = 0;
      if (Actor.STAMINA_MIN_FOR_ACTIVITY > b.StaminaPoints && Actor.STAMINA_MIN_FOR_ACTIVITY <= a.StaminaPoints) b_dam = 0;
      if (0 >= a_dam) return a;
      if (0 >= b_dam) return b;
      int speed_factor = a.HowManyTimesOtherActs(1, b);
      if (1 < speed_factor) b_dam *= a.HowManyTimesOtherActs(1, b);
      else if (1 > speed_factor) a_dam *= 2;    // usually double-move
      if (a_dam/2 >= b.HitPoints) return b;	// one-shot if hit
      if (b_dam >= a.HitPoints) return a; // could die if hit
      int a_kill_b_in = ((8*b.HitPoints)/(5*a_dam));	// assume bad luck when attacking
      int b_kill_a_in = ((8*a.HitPoints)/(7*b_dam));	// assume bad luck when defending
      if (a_kill_b_in < b_kill_a_in) return b;
      if (a_kill_b_in > b_kill_a_in) return a;
//	  int num1 = a.HitPoints + a.MeleeAttack(b).DamageValue;
//      int num2 = b.HitPoints + b.MeleeAttack(a).DamageValue;
//      if (num1 < num2) return a;
//      if (num1 > num2) return b;
      return null;
    }

    // XXX these two break down cross-map
    protected bool CanReachSimple(in Location dest, RouteFinder.SpecialActions allowedActions)
    {
       (m_RouteFinder ??= new RouteFinder(this)).AllowedActions = allowedActions;
       return m_RouteFinder.CanReachSimple(in dest, Rules.GridDistanceFn);
    }

    protected void FilterOutUnreachable<_T_>(ref List<_T_> percepts, RouteFinder.SpecialActions allowedActions) where _T_:ILocation
    {
      percepts?.OnlyIf(p => CanReachSimple(p.Location, allowedActions));
    }

    protected class ChoiceEval<_T_>
    {
      public readonly _T_ Choice;
      public readonly float Value;

      public ChoiceEval(_T_ choice, float value)
      {
        Choice = choice;
        Value = value;
      }

      public override string ToString()
      {
        return string.Format("ChoiceEval({0}; {1:F})", Choice is null ? "NULL" : Choice.ToString(), Value);
      }
    }

    [System.Flags]
    protected enum UseExitFlags
    {
      NONE = 0,
      BREAK_BLOCKING_OBJECTS = 1,
      ATTACK_BLOCKING_ENEMIES = 2,
      DONT_BACKTRACK = 4,
    }
  }

  internal static partial class ext_RSRevived {
    public static AI RunBy<AI>(this Actor? a) where AI : ActorController {
      return (a?.Controller as AI) ?? throw new InvalidOperationException((a?.UnmodifiedName ?? "null") + " is not run by " + typeof(AI).Name);
    }

    public static Actor ControlledBy<AI>(this Actor a) where AI : ActorController {
      return (a is not null && a.Controller is AI) ? a : throw new InvalidOperationException((a?.UnmodifiedName ?? "null") + " is not run by " + typeof(AI).Name);
    }

    public static LivingActor ControlledBy<AI>(this LivingActor a) where AI : ActorController {
      return (a is not null && a.Controller is AI) ? a : throw new InvalidOperationException((a?.UnmodifiedName ?? "null") + " is not run by " + typeof(AI).Name);
    }

    [Conditional("DEBUG")]
    public static void ControlledBy<AI>(this IEnumerable<Actor> src) where AI : ActorController {
      if (src is null) return;
      foreach(var a in src) if (a.Controller is not AI) throw new InvalidOperationException((a?.UnmodifiedName ?? "null") + " is not run by " + typeof(AI).Name);
    }

    public static List<KeyValuePair<_T_, A>>? Candidates<_T_, A>(this IEnumerable<_T_> choices, Func<_T_,A?> interpret) where A:ActorAction
    {
      List<KeyValuePair<_T_, A>> ret = new();
      foreach(var choice in choices) {
        var test = interpret(choice);
        if (test is null) continue;
        if (!test.IsPerformable()) continue;
        ret.Add(new(choice, test));
      }
      return 0<ret.Count ? ret : null;
    }

    public static Func<Point, Point, float> Postprocess(this Func<Point, Point, float> src, Func<Point, Point, float, float> post)
    {
      if (post is null) {
        Func<Point, Point, float> l = src;  // local copies needed to get true lambda calculus
        return l;
      }
      float ret(Point ptA,Point ptB) {
        float x = src(ptA,ptB);
        if (!float.IsNaN(x)) x = post(ptA,ptB,x);
        return x;
      }
      return ret;
    }
  }
}
