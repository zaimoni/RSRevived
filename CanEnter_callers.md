# Callers of the CanEnter families

Seeded 2026-10-12 from master at 71cc05b3; refreshed 2026-10-12 at c94620ce. Companion to `IsLegal_vs_IsPerformable.md`.

Three predicates share the name. Each root below lists its callers, one row per call site.

- **`Map.CanEnter(ref Location)`** (static): canonicalises, then asks only whether the tile is walkable. "It is possible to configure an actor that can enter." Geometry alone.
- **`ActorModel.CanEnter(Location)`**, `CanEnter(ref Location)`: the model's abilities, no skills, stamina or tiredness. Legality: an actor of this model could ever enter.
- **`Actor.CanEnter(Location)`**, `CanEnter(ref Location)`: this actor, with its skills and stamina. Performability: it can enter now.

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
| Op.PushOnto._Ok | Engine/Action/PushOnto.cs:28 | unreviewed | throws on the origin |
| Op.PushOnto._Ok | Engine/Action/PushOnto.cs:29 | unreviewed | throws on the destination |
| Op.PushOnto._CanConstruct | Engine/Action/PushOnto.cs:38 | unreviewed | origin |
| Op.PushOnto._CanConstruct | Engine/Action/PushOnto.cs:39 | unreviewed | destination |
| UpdateMoveDelta..ctor | Engine/Actions/ActionMoveDelta.cs:273 | ok | throws on the origin |
| UpdateMoveDelta..ctor | Engine/Actions/ActionMoveDelta.cs:274 | ok | throws on the destination |
| UpdateMoveDelta.toDest | Engine/Actions/ActionMoveDelta.cs:308 | ok | |
| UpdateMoveDelta.fromOrigin | Engine/Actions/ActionMoveDelta.cs:315 | ok | |
| PathToTarget.Unstage | Gameplay/AI/Goals/Objective.cs:193 | ok | |

## ActorModel.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Map.TrapWouldSealBuilding | Data/Map.cs:2163 | ok | asks GameActors.MaleCivilian on behalf of no one (2026-10-11) |
| Op.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:93 | ok | origin; was Actor.CanEnter until 81a9544a |
| Op.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:94 | ok | destination; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:176 | ok | destination; was Actor.CanEnter until 81a9544a |
| _Action.MoveStep.ScheduleCast | Engine/Action/MoveStep.cs:177 | ok | origin; was Actor.CanEnter until 81a9544a |
| ActionMoveStep.schedule(Actor, Location) | Engine/Actions/ActionMoveStep.cs:53 | ok | |
| ActionMoveStep.schedule(Actor, Point) | Engine/Actions/ActionMoveStep.cs:58 | ok | |
| RogueGame.HandleItemInfo | Engine/RogueGame.cs:4196 | ok | local function `details`; receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13489 | ok | receiver a follower, chosen by index |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13514 | ok | receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13522 | ok | receiver Player |
| RogueGame.DoPlayerFarLook | Engine/RogueGame.cs:13544 | ok | receiver Player |
| AcquireLineOfSight..ctor(Actor, in Location) | Gameplay/AI/Goals/AcquireLineOfSight.cs:46 | ok | throws via Spotlight: "unpathable"; was Actor.CanEnter until c94620ce |
| AcquireLineOfSight..ctor(Actor, IEnumerable) | Gameplay/AI/Goals/AcquireLineOfSight.cs:55 | ok | RemoveWhere lambda; was Actor.CanEnter until c94620ce |
| AcquireLineOfSight..ctor(Actor, HashSet) | Gameplay/AI/Goals/AcquireLineOfSight.cs:65 | ok | RemoveWhere lambda; was Actor.CanEnter until c94620ce |
| ObjectiveAI.DeathTrapsInSight | Gameplay/AI/ObjectiveAI.cs:4795 | ok | two calls: N and S of the trap |
| ObjectiveAI.DeathTrapsInSight | Gameplay/AI/ObjectiveAI.cs:4798 | ok | two calls: E and W of the trap |
| ObjectiveAI.CanDisarmDeathtrap | Gameplay/AI/ObjectiveAI.cs:4838 | ok | by ref |

## Actor.CanEnter

| caller | site | status | note |
|---|---|---|---|
| Actor.MovesTo | Data/Actor.cs:2055 | unreviewed | the destination itself |
| Actor.MovesTo | Data/Actor.cs:2058 | unreviewed | each adjacent source, by ref |
| Actor.MovesTo | Data/Actor.cs:2062 | unreviewed | an exit's location, same district |
| Actor.MovesFrom | Data/Actor.cs:2074 | unreviewed | the source itself |
| Actor.MovesFrom | Data/Actor.cs:2077 | unreviewed | each adjacent destination, by ref |
| Actor.MovesFrom | Data/Actor.cs:2081 | unreviewed | an exit's location, same district |
| Actor.ReachableFrom | Data/Actor.cs:2092 | unreviewed | the source itself |
| Actor.ReachableFrom | Data/Actor.cs:2095 | unreviewed | each adjacent destination, by ref |
| Actor.ReachableFrom | Data/Actor.cs:2099 | unreviewed | an exit's location, same district |
| Actor.OnePath(in Location, ...) | Data/Actor.cs:2164 | unreviewed | fallback after Rules.IsPathableFor |
| Actor.OnePath(Map, in Point, ...) | Data/Actor.cs:2214 | unreviewed | fallback after Rules.IsPathableFor |
| Actor.CastToBumpableDestinations | Data/Actor.cs:2889 | unreviewed | |
| Actor.CastToBumpableDestinations | Data/Actor.cs:2891 | unreviewed | |
| Actor.CastToInventoryAccessibleDestinations | Data/Actor.cs:2907 | unreviewed | excludes shelf-like objects separately |
| Actor.CastToInventoryAccessibleDestinations | Data/Actor.cs:2912 | unreviewed | |
| Actor.MutuallyAdjacentFor | Data/Actor.cs:2928 | unreviewed | by ref |
| Map.WouldBlacklistFor | Data/Map.cs:1170 | unreviewed | receiver is the actor parameter |
| _Action.MoveStep._Ok | Engine/Action/MoveStep.cs:118 | unreviewed | throws on the destination |
| _Action.MoveStep..ctor | Engine/Action/MoveStep.cs:148 | unreviewed | throws on the origin |
| _Action.MoveStep.RunFrom | Engine/Action/MoveStep.cs:190 | unreviewed | origin |
| _Action.MoveStep.RunFrom | Engine/Action/MoveStep.cs:196 | unreviewed | each candidate destination, by ref |
| _Action.PushOnto._Ok | Engine/Action/PushOnto.cs:224 | unreviewed | throws on the origin |
| _Action.PushOnto._Ok | Engine/Action/PushOnto.cs:225 | unreviewed | throws on the destination |
| _Action.PushOnto._CanConstruct | Engine/Action/PushOnto.cs:236 | unreviewed | origin |
| _Action.PushOnto._CanConstruct | Engine/Action/PushOnto.cs:237 | unreviewed | destination |
| _Action.PushOnto.IsPerformable | Engine/Action/PushOnto.cs:287 | unreviewed | |
| UpdateMoveDelta.Bind | Engine/Actions/ActionMoveDelta.cs:291 | unreviewed | the bound actor, new location |
| UpdateMoveDelta.Bind | Engine/Actions/ActionMoveDelta.cs:292 | unreviewed | the bound actor, origin |
| Rules.IsBumpableFor | Engine/Rules.cs:220 | ok | by ref |
| ClearZone..ctor | Gameplay/AI/Goals/ClearZone.cs:84 | unreviewed | Where lambda over the zone; comment says "todo? eliminate" |
| SharedPlan.Goals | Gameplay/AI/Goals/Cooperate.cs:104 | unreviewed | RemoveWhere lambda; comment says it matters for burning cars |
| ObjectiveAI.UsePreexistingPath(List<List<Point>>, ...) | Gameplay/AI/ObjectiveAI.cs:1115 | ok | comment: generators are pathable but not enterable |
| ObjectiveAI.UsePreexistingPath(List<List<Point>>, ...) | Gameplay/AI/ObjectiveAI.cs:1120 | unreviewed | |
| ObjectiveAI.UsePreexistingPath(List<List<Location>>, ...) | Gameplay/AI/ObjectiveAI.cs:1174 | ok | comment: generators are pathable but not enterable |
| ObjectiveAI.UsePreexistingPath(List<List<Location>>, ...) | Gameplay/AI/ObjectiveAI.cs:1178 | unreviewed | |
| ObjectiveAI.WantToGoHere | Gameplay/AI/ObjectiveAI.cs:1412 | unreviewed | by ref |
| ObjectiveAI.destination_melee_safe | Gameplay/AI/ObjectiveAI.cs:3280 | unreviewed | |
| ObjectiveAI.install_moves | Gameplay/AI/ObjectiveAI.cs:3335 | unreviewed | |
| ObjectiveAI.BehaviorPathTo(Func, ...) | Gameplay/AI/ObjectiveAI.cs:4247 | unreviewed | by ref |
| ObjectiveAI.BehaviorPathTo(HashSet<Location>) | Gameplay/AI/ObjectiveAI.cs:4538 | unreviewed | |
| ObjectiveAI.GetInterestingInventoryStacks | Gameplay/AI/ObjectiveAI.cs:5309 | unreviewed | comment: ignore buggy stack placement |
| Goal_PathToStack._removeInvalidStacks | Gameplay/AI/OrderableAI.cs:823 | unreviewed | |
| OrderableAI._closeIn | Gameplay/AI/OrderableAI.cs:2827 | unreviewed | Adjacent lambda |
| OrderableAI._chargeTowards | Gameplay/AI/OrderableAI.cs:2875 | unreviewed | Adjacent lambda |
| MinStepPath..ctor | Gameplay/AI/Tools/MinStepPath.cs:71 | unreviewed | throws via Spotlight on the origin |
| MinStepPath..ctor | Gameplay/AI/Tools/MinStepPath.cs:72 | unreviewed | throws via Spotlight on the destination |
