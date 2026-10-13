# Callers of the CanEnter families

Seeded 2026-10-12 from master at 71cc05b3; refreshed 2026-10-13 at 821c2f7f. Companion to `IsLegal_vs_IsPerformable.md`.

Three predicates share the name. Each root below lists its callers, one row per call site.

- **`Map.CanEnter(ref Location)`** (static): canonicalises, then asks only whether the tile is walkable. "It is possible to configure an actor that can enter." Geometry alone.
- **`ActorModel.CanEnter(Location)`**, `CanEnter(ref Location)`: the model's abilities, no skills, stamina or tiredness. Legality: an actor of this model could ever enter.
- **`Actor.CanEnter(Location)`**, `CanEnter(ref Location)`: this actor, with its skills and stamina. Performability: it can enter now. `Actor.StrictCanEnter(in Location)` is the same test without canonicalising: out of bounds is a no. Its callers are listed under `Actor.CanEnter`.

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
| Location.ForEachAdjacent | Data/Location.cs:76 | ok | filters the neighbours handed to the callback |
| Op.PushOnto.schedule | Engine/Action/PushOnto.cs:184 | ok | origin |
| Op.PushOnto.schedule | Engine/Action/PushOnto.cs:185 | ok | destination |
| UpdateMoveDelta..ctor | Engine/Actions/ActionMoveDelta.cs:299 | ok | throws on the origin |
| UpdateMoveDelta..ctor | Engine/Actions/ActionMoveDelta.cs:300 | ok | throws on the destination |
| UpdateMoveDelta.toDest | Engine/Actions/ActionMoveDelta.cs:329 | ok | |
| UpdateMoveDelta.fromOrigin | Engine/Actions/ActionMoveDelta.cs:336 | ok | |
| PathToTarget.Unstage | Gameplay/AI/Goals/Objective.cs:193 | ok | |

## ActorModel.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Actor.MovesTo | Data/Actor.cs:2055 | ok | the destination itself |
| Map.TrapWouldSealBuilding | Data/Map.cs:2163 | ok | asks GameActors.MaleCivilian on behalf of no one (2026-10-11) |
| Op.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:93 | ok | origin; was Actor.CanEnter until 81a9544a |
| Op.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:94 | ok | destination; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:176 | ok | destination; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:177 | ok | origin; was Actor.CanEnter until 81a9544a |
| _Action.PushOnto.schedule | Engine/Action/PushOnto.cs:280 | ok | origin |
| _Action.PushOnto.schedule | Engine/Action/PushOnto.cs:281 | ok | destination |
| ActionMoveDelta.schedule(Actor, Location, Location) | Engine/Actions/ActionMoveDelta.cs:237 | ok | destination |
| ActionMoveDelta.schedule(Actor, Location, Location) | Engine/Actions/ActionMoveDelta.cs:238 | ok | origin |
| ActionMoveDelta.schedule(Actor, Location) | Engine/Actions/ActionMoveDelta.cs:256 | ok | destination; the overload without an origin |
| ActionMoveStep.schedule(Actor, Location) | Engine/Actions/ActionMoveStep.cs:52 | ok | |
| ActionMoveStep.schedule(Actor, Point) | Engine/Actions/ActionMoveStep.cs:58 | ok | |
| RogueGame.HandleItemInfo | Engine/RogueGame.cs:4196 | ok | local function `details`; receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13489 | ok | receiver a follower, chosen by index |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13514 | ok | receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13522 | ok | receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13544 | ok | receiver Player |
| AcquireLineOfSight..ctor(Actor, in Location) | Gameplay/AI/Goals/AcquireLineOfSight.cs:46 | ok | throws via Spotlight: "unpathable"; was Actor.CanEnter until c94620ce |
| AcquireLineOfSight..ctor(Actor, IEnumerable) | Gameplay/AI/Goals/AcquireLineOfSight.cs:55 | ok | RemoveWhere lambda; was Actor.CanEnter until c94620ce |
| AcquireLineOfSight..ctor(Actor, HashSet) | Gameplay/AI/Goals/AcquireLineOfSight.cs:65 | ok | RemoveWhere lambda; was Actor.CanEnter until c94620ce |
| ObjectiveAI.DeathTrapsInSight | Gameplay/AI/ObjectiveAI.cs:4788 | ok | two calls: N and S of the trap |
| ObjectiveAI.DeathTrapsInSight | Gameplay/AI/ObjectiveAI.cs:4791 | ok | two calls: E and W of the trap |
| ObjectiveAI.CanDisarmDeathtrap | Gameplay/AI/ObjectiveAI.cs:4831 | ok | by ref |

## Actor.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Actor.CastToBumpableDestinations | Data/Actor.cs:2849 | unreviewed | |
| Actor.CastToBumpableDestinations | Data/Actor.cs:2851 | unreviewed | |
| Actor.CastToInventoryAccessibleDestinations | Data/Actor.cs:2867 | unreviewed | excludes shelf-like objects separately |
| Actor.CastToInventoryAccessibleDestinations | Data/Actor.cs:2872 | unreviewed | |
| Actor.MutuallyAdjacentFor | Data/Actor.cs:2888 | ok | by ref |
| Map.WouldBlacklistFor | Data/Map.cs:1170 | unreviewed | receiver is the actor parameter |
| Map.PathfindLocSteps | Data/Map.cs:1184 | unreviewed | StrictCanEnter, in the flood-fill's lambda; receiver is the actor parameter |
| _Action.MoveStep._Ok | Engine/Action/MoveStep.cs:118 | unreviewed | throws on the destination |
| _Action.MoveStep..ctor | Engine/Action/MoveStep.cs:148 | unreviewed | throws on the origin |
| _Action.MoveStep.RunFrom | Engine/Action/MoveStep.cs:190 | unreviewed | origin |
| _Action.MoveStep.RunFrom | Engine/Action/MoveStep.cs:196 | unreviewed | each candidate destination, by ref |
| _Action.PushOnto.IsPerformable | Engine/Action/PushOnto.cs:249 | ok | |
| Rules.IsBumpableFor | Engine/Rules.cs:220 | ok | by ref |
| ClearZone..ctor | Gameplay/AI/Goals/ClearZone.cs:84 | unreviewed | Where lambda over the zone; comment says "todo? eliminate" |
| SharedPlan.Goals | Gameplay/AI/Goals/Cooperate.cs:104 | unreviewed | RemoveWhere lambda; comment says it matters for burning cars |
| ObjectiveAI.UsePreexistingPath(List<List<Point>>, ...) | Gameplay/AI/ObjectiveAI.cs:1111 | ok | comment: generators are pathable but not enterable |
| ObjectiveAI.UsePreexistingPath(List<List<Location>>, ...) | Gameplay/AI/ObjectiveAI.cs:1168 | ok | comment: generators are pathable but not enterable |
| ObjectiveAI.WantToGoHere | Gameplay/AI/ObjectiveAI.cs:1404 | unreviewed | by ref |
| ObjectiveAI.PlanWalkAwayFrom | Gameplay/AI/ObjectiveAI.cs:3272 | unreviewed | local function destination_melee_safe |
| ObjectiveAI.PlanWalkAwayFrom | Gameplay/AI/ObjectiveAI.cs:3327 | unreviewed | local function install_moves; its row had gone missing |
| ObjectiveAI.BehaviorPathTo(Func, ...) | Gameplay/AI/ObjectiveAI.cs:4240 | unreviewed | by ref |
| ObjectiveAI.BehaviorPathTo(HashSet<Location>) | Gameplay/AI/ObjectiveAI.cs:4531 | unreviewed | |
| ObjectiveAI.GetInterestingInventoryStacks | Gameplay/AI/ObjectiveAI.cs:5302 | unreviewed | comment: ignore buggy stack placement |
| Goal_PathToStack._removeInvalidStacks | Gameplay/AI/OrderableAI.cs:823 | unreviewed | |
| OrderableAI._closeIn | Gameplay/AI/OrderableAI.cs:2827 | unreviewed | Adjacent lambda |
| OrderableAI._chargeTowards | Gameplay/AI/OrderableAI.cs:2875 | unreviewed | Adjacent lambda |
| MinStepPath..ctor | Gameplay/AI/Tools/MinStepPath.cs:71 | unreviewed | throws via Spotlight on the origin |
| MinStepPath..ctor | Gameplay/AI/Tools/MinStepPath.cs:72 | unreviewed | throws via Spotlight on the destination |
