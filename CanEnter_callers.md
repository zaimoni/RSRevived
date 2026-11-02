# Callers of the CanEnter families

Seeded 2026-10-12 from master at 71cc05b3; refreshed 2026-10-15 at af2b5f1c. Companion to `IsLegal_vs_IsPerformable.md`.

Three predicates share the name, and a fourth chooses between two of them. Each root below lists its callers, one row per call site.

- **`Map.CanEnter(ref Location)`** (static): canonicalises, then asks only whether the tile is walkable. "It is possible to configure an actor that can enter." Geometry alone.
- **`ActorModel.CanEnter(Location)`**, `CanEnter(ref Location)`: the model's abilities, no skills, stamina or tiredness. Legality: an actor of this model could ever enter.
- **`Actor.CanEnter(Location)`**, `CanEnter(ref Location)`: this actor, with its skills and stamina. Performability: it can enter now. `Actor.StrictCanEnter(in Location)` is the same test without canonicalising: out of bounds is a no. Its callers are listed under `Actor.CanEnter`.
- **`Actor.MayEnter(Location)`**, `MayEnter(ref Location)`: the strictest answer the distance allows. The actor's own tile: yes. Adjacent (InteractionDistance one): `Actor.CanEnter`. Farther: `ActorModel.CanEnter`.

## How to read and maintain this file

- A section heads a function and lists its callers. Expanding the graph upward means adding a section for a caller that already appears in a table. A caller listed in a table but lacking a section is the frontier.
- `caller` is the enclosing member, qualified enough to be unique (the `Op` and `_Action` namespaces both have a `MoveStep` and a `PushOnto`). A local function or lambda is credited to the member that contains it.
- `site` is a repo-relative path under `RogueSurvivor/` and a line number. Lines drift; the caller name is what to match on. Refresh a site only when its row is touched.
- `status` is one of: `unreviewed` (added mechanically, not yet judged), `ok` (used correctly), `wrong-family` (should ask a different one of the three), `legacy` (function intentionally not maintained), `reimplement` (schedule re-implementation). A mechanical refresh never overwrites a status other than `unreviewed`.
- `note` is one clause. Longer reasoning belongs in a commit, which the note may cite by date or hash.
- Where one line calls the predicate twice, there is one row and the note says so.

## Map.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Location.ForEachAdjacent | Data/Location.cs:70 | ok | filters the neighbours handed to the callback |
| Op.PushOnto.schedule | Engine/Action/PushOnto.cs:184 | ok | origin |
| Op.PushOnto.schedule | Engine/Action/PushOnto.cs:185 | ok | destination |
| UpdateMoveDelta..ctor | Engine/Actions/ActionMoveDelta.cs:292 | ok | throws on the origin |
| UpdateMoveDelta..ctor | Engine/Actions/ActionMoveDelta.cs:293 | ok | throws on the destination |
| UpdateMoveDelta.toDest | Engine/Actions/ActionMoveDelta.cs:322 | ok | |
| UpdateMoveDelta.fromOrigin | Engine/Actions/ActionMoveDelta.cs:329 | ok | |
| PathToTarget.Unstage | Gameplay/AI/Goals/Objective.cs:217 | ok | |

## ActorModel.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Actor.MovesTo | Data/Actor.cs:1591 | ok | the destination itself |
| Actor.MayEnter(ref Location) | Data/Actor.cs:2309 | ok | beyond adjacency |
| ActorModel.CastToBumpableDestinations | Data/ActorModel.cs:255 | ok | |
| ActorModel.CastToBumpableDestinations | Data/ActorModel.cs:257 | ok | |
| ActorModel.CastToInventoryAccessibleDestinations(,) | Data/ActorModel.cs:274 | ok | excludes shelf-like objects separately |
| ActorModel.CastToInventoryAccessibleDestinations(,) | Data/ActorModel.cs:277 | ok | |
| ActorModel.CastToInventoryAccessibleDestinations() | Data/ActorModel.cs:291 | ok | a shelf: any neighbour |
| ActorModel.CastToInventoryAccessibleDestinations() | Data/ActorModel.cs:297 | ok | a ground stack: the tile itself |
| ActorModel.CastToInventoryAccessibleDestinations() | Data/ActorModel.cs:303 | ok | a ground stack: any neighbour, when reaching in is possible; row added 2026-10-14 |
| ActorModel.HasAnyInventoryAccessibleDestinations | Data/ActorModel.cs:315 | ok | a shelf: any neighbour |
| ActorModel.HasAnyInventoryAccessibleDestinations | Data/ActorModel.cs:321 | ok | a ground stack: the tile itself |
| ActorModel.HasAnyInventoryAccessibleDestinations | Data/ActorModel.cs:326 | ok | a ground stack: any neighbour, when reaching in is possible; row added 2026-10-14 |
| Map.WouldBlacklistFor | Data/Map.cs:1178 | ok | receiver is the actor parameter |
| Map.PathfindLocSteps | Data/Map.cs:1192 | ok | StrictCanEnter, in the flood-fill's lambda; receiver is actor.Model |
| Map.TrapWouldSealBuilding | Data/Map.cs:2166 | ok | asks GameActors.MaleCivilian on behalf of no one (2026-10-11) |
| Op.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:93 | ok | origin; was Actor.CanEnter until 81a9544a |
| Op.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:94 | ok | destination; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.schedule | Engine/Action/MoveStep.cs:173 | ok | destination; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.schedule | Engine/Action/MoveStep.cs:174 | ok | origin; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.RunFrom | Engine/Action/MoveStep.cs:205 | ok | origin |
| _Action.MoveStep.RunFrom | Engine/Action/MoveStep.cs:211 | ok | each candidate destination, by ref |
| _Action.PushOnto.schedule | Engine/Action/PushOnto.cs:280 | ok | origin |
| _Action.PushOnto.schedule | Engine/Action/PushOnto.cs:281 | ok | destination |
| _Action.SwitchPlace.IsPerformable | Engine/Action/SwitchPlace.cs:45 | ok |  |
| _Action.SwitchPlace.schedule(,,) | Engine/Action/SwitchPlace.cs:102 | ok |  |
| _Action.SwitchPlace.schedule(,,) | Engine/Action/SwitchPlace.cs:103 | ok |  |
| _Action.SwitchPlace.schedule(,) | Engine/Action/SwitchPlace.cs:111 | ok |  |
| ActionMoveDelta.schedule(Actor, Location, Location) | Engine/Actions/ActionMoveDelta.cs:230 | ok | destination |
| ActionMoveDelta.schedule(Actor, Location, Location) | Engine/Actions/ActionMoveDelta.cs:231 | ok | origin |
| ActionMoveDelta.schedule(Actor, Location) | Engine/Actions/ActionMoveDelta.cs:249 | ok | destination; the overload without an origin |
| ActionMoveStep.schedule(Actor, Location) | Engine/Actions/ActionMoveStep.cs:52 | ok | |
| ActionMoveStep.schedule(Actor, Point) | Engine/Actions/ActionMoveStep.cs:58 | ok | |
| RogueGame.HandleItemInfo | Engine/RogueGame.cs:4083 | ok | local function `details`; receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13350 | ok | receiver a follower, chosen by index |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13375 | ok | receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13383 | ok | receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13405 | ok | receiver Player |
| AcquireLineOfSight..ctor(Actor, in Location) | Gameplay/AI/Goals/AcquireLineOfSight.cs:44 | ok | throws via Spotlight: "unpathable"; was Actor.CanEnter until c94620ce |
| AcquireLineOfSight..ctor(Actor, IEnumerable) | Gameplay/AI/Goals/AcquireLineOfSight.cs:52 | ok | RemoveWhere lambda; was Actor.CanEnter until c94620ce |
| AcquireLineOfSight..ctor(Actor, HashSet) | Gameplay/AI/Goals/AcquireLineOfSight.cs:61 | ok | RemoveWhere lambda; was Actor.CanEnter until c94620ce |
| PathTo..ctor | Gameplay/AI/Goals/PathTo.cs:59 | ok | #if DEBUG |
| PathTo..ctor | Gameplay/AI/Goals/PathTo.cs:70 | ok | #if DEBUG |
| ObjectiveAI.DeathTrapsInSight | Gameplay/AI/ObjectiveAI.cs:4609 | ok | two calls: N and S of the trap |
| ObjectiveAI.DeathTrapsInSight | Gameplay/AI/ObjectiveAI.cs:4612 | ok | two calls: E and W of the trap |
| ObjectiveAI.CanDisarmDeathtrap | Gameplay/AI/ObjectiveAI.cs:4652 | ok | by ref |

## Actor.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Actor.MutuallyAdjacentFor | Data/Actor.cs:2321 | ok | by ref |
| _Action.MoveStep._Ok | Engine/Action/MoveStep.cs:118 | ok | throws on the destination |
| _Action.MoveStep._runFrom | Engine/Action/MoveStep.cs:192 | ok | each candidate destination, by ref |
| _Action.PushOnto.IsPerformable | Engine/Action/PushOnto.cs:249 | ok | |
| Rules.IsBumpableFor | Engine/Rules.cs:218 | ok | by ref |
| ObjectiveAI.UsePreexistingPath(List<List<Point>>, ...) | Gameplay/AI/ObjectiveAI.cs:925 | ok | comment: generators are pathable but not enterable |
| ObjectiveAI.UsePreexistingPath(List<List<Location>>, ...) | Gameplay/AI/ObjectiveAI.cs:982 | ok | comment: generators are pathable but not enterable |
| OrderableAI._closeIn | Gameplay/AI/OrderableAI.cs:2822 | ok | Adjacent lambda |
| OrderableAI._chargeTowards | Gameplay/AI/OrderableAI.cs:2870 | ok | Adjacent lambda |

## Actor.MayEnter

| caller | site | status | note |
|---|---|---|---|
| SharedPlan.Goals | Gameplay/AI/Goals/Cooperate.cs:102 | ok | RemoveWhere lambda; comment says it matters for burning cars |
| PathTo.Pathing | Gameplay/AI/Goals/PathTo.cs:94 | ok | MinStapPath precondition |
| ObjectiveAI.WantToGoHere | Gameplay/AI/ObjectiveAI.cs:1211 | ok | by ref |
| ObjectiveAI.PlanWalkAwayFrom | Gameplay/AI/ObjectiveAI.cs:3045 | ok | local function destination_melee_safe |
| ObjectiveAI.PlanWalkAwayFrom | Gameplay/AI/ObjectiveAI.cs:3100 | ok | local function install_moves; its row had gone missing |
| ObjectiveAI.BehaviorPathTo(Func, ...) | Gameplay/AI/ObjectiveAI.cs:4007 | ok | by ref |
| ObjectiveAI.BehaviorPathTo(HashSet<Location>) | Gameplay/AI/ObjectiveAI.cs:4040 | ok | #if DEBUG |
| ObjectiveAI.BehaviorPathTo(HashSet<Location>) | Gameplay/AI/ObjectiveAI.cs:4323 | ok | |
| MinStepPath..ctor | Gameplay/AI/Tools/MinStepPath.cs:154 | ok | throws via Spotlight on the origin |
| MinStepPath..ctor | Gameplay/AI/Tools/MinStepPath.cs:155 | ok | throws via Spotlight on the destination |
| MinStepPath.Find | Gameplay/AI/Tools/MinStepPath.cs:216 | ok | by ref |

# cleanup tasks

* VetoExit call site micro-optimization for CPU
* Law of Demeter: Actor -> ActorModel member functions