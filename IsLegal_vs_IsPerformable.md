# Legality vs. performability: the `ActionMeleeAttack` index case

Recorded 2026-10-08. The issues that one failing save exposed, and where each stands. Companion to `JSON_save_load_plan.md` (phase 3, "pre-save pruning").

## The contract

`ActorAction` has two predicates. **`IsLegal()`**: the action makes sense and may be scheduled for later. **`IsPerformable()`**: it can be done *now* (adjacency, stamina, line of fire, inventory room, mood). `IsPerformable()` implies `IsLegal()`; the split actions make that true by construction (`ReasonIllegal(...) ?? ReasonCant(...)`), and the legacy default `IsPerformable() => IsLegal()` makes it true trivially.

The save-time pruning of objectives (`Objective.UrgentAction_readonly`, 2026-10-04) must decide from stored state alone: a save is not "now", and a stored intent is usually waiting out exactly the condition that makes it unperformable. So at save **only `IsLegal()` decides** (2026-10-05). Post-conditions, documented in `fc021ef2`: a non-null `ret` from `UrgentAction_readonly` satisfies `ret.IsLegal()`; from `UrgentAction`, `ret.IsPerformable()`. Every one of the nineteen `UrgentAction_readonly` overrides answers `ret = null`, and the classes that inherit the default satisfy the stronger condition because `IsPerformable()` refines `IsLegal()`.

## The index case

A test game saved at turn 19 from a turn-17 start lost a `Goal_NextCombatAction`. Its engaged intent was an `ActionMeleeAttack` whose target had stepped out of reach; `ActionMeleeAttack.IsLegal()` was the legacy `Actor.CanMeleeAttack`, which tested adjacency and stamina along with the target being alive, so the save judged a legal, merely unperformable, intent dead and pruned the objective. The actor's plan was gone on load.

The root cause is older than the save format: RS Alpha 10 did not distinguish the two predicates, and the `Actor.Can*` / `ReasonCant*` family mixes them. The JSON save is the first client that asks the legality question on its own.

## Issues and status

### 1. Legacy actions whose `IsLegal` contains "now" constraints

Only the classes registered for the save matter here (the `[Serializable]` actions, the ones an objective may hold).

| Action | What its `IsLegal` mixed in | Status |
|---|---|---|
| `ActionMeleeAttack` | adjacency, stamina | **Split** (`f08fd53b`): legality is the target alive; `Actor.CanMeleeAttack` became `ActionMeleeAttack.Can`. |
| `ActionRangedAttack` | equipped ranged weapon, range, ammo, line of fire | **Split** (`8994e578`): likewise; `ActionRangedAttack.Can`. |
| `ActionTrade` | activity (fighting, fleeing), empty inventories, a mutually acceptable trade | **Split** (`a3cbab3a`, `f6f49cd3`, `6dee9430`); see issue 4. |
| `ActionTake` | reach (`init()` resolves the stack: "not in reach") | Open. Legality is the item still being where it was. |
| `ActionSleep` | `CanSleep`: hungry, not sleepy enough | Open. Legality is `HasToSleep`. |
| `ActionCloseDoor` | `ReasonCantClose`: "someone is there" | Open; it already has an `IsPerformable` (adjacency), so the clause only moves. |
| `TakeItem` | `CanGet`: inventory full | Open; has an `IsPerformable` (distance). |
| `ActionPull` | `ReasonCantPull`: blocking object, walkable destination | Open; has an `IsPerformable` (distance). |
| `ActionUseItem`, `ActionUse` | `ReasonCantUseItem` ends in the item's own `ReasonCantUse(actor)` (medicine when healthy, and the like) | Open. Legality is ability, item type, carrying. |
| `ActionSequence` | ends in the resolved action's `IsPerformable` | Benign today: its first clause (same turn, action points still there) holds at any save within the turn. |

Already clean: `ActionMoveDelta`, `ActionGiveTo`, `ActionDropItem`, `ActionWait`, `MoveStep`, `PushOnto`, `TakeItem`'s sibling `TradeItem`, `Fork`, `Join`, `Chain`, `BehavioristABC`; every `WorldUpdate`.

Any legacy action not in the registry (`ActionBump`, `ActionBashDoor`, `ActionUseExit`, …) can keep the mix without harming a save, since it never reaches one.

### 2. The save-time decision rule

- `UrgentAction` must not be called at save: it edits `Objectives` while they are enumerated and reaches into pathing (abandoned 2026-10-04 after a "collection was modified" and an untraced null dereference).
- `UrgentAction_readonly` (`85a504fe`): `UrgentAction` without side effects, virtual, defaulting to `UrgentAction`; overridden where that acts.
- The four intent-holding objectives (`Goal_NextAction`, `Goal_NextCombatAction`, `StageAction`, `Goal_NonCombatComplete`) decide on `IsLegal()`, never `IsPerformable()` (`b8f1d1f1`, 2026-10-05).
- `Goal.NextAction.UrgentAction_readonly` keeps the objective while any of its four slots is legal (`f08fd53b`); before that it never pruned.
- The post-conditions above are documented (`fc021ef2`). **Status: done.**

### 3. The three one-shot objectives are not materially parallel

`Goal.NextAction` (engaged/disengaged, action or plan) generalises `Goal_NextCombatAction` (engaged/disengaged actions), which generalises `Goal_NextAction` (one intent, deferred while enemies are in sight, vetoable). Their `UrgentAction` / `UrgentAction_readonly` pairs now agree on the save-time rule (any slot legal → keep; nothing legal → expire) and on "handing over nothing still expires". Remaining divergences:

| | Status |
|---|---|
| `Abort()` is consulted only by `Goal_NextAction`. `ActorAction` documents it as the hook to run after `IsLegal`/`IsPerformable` (how a `Fork` reports it emptied itself); the two general classes never ask, so an emptied `Fork` is kept at save and handed over live. | Open. Parallel form: alive iff `IsLegal() && !Abort()` at save, `IsPerformable() && !Abort()` live, in all three. |
| `Goal_NextAction` tests performability before "enemies in sight", so a legal-but-not-yet-performable intent expires even though the class would have deferred it; the save path keeps it. | Open; a behaviour change, so a design call. |
| `RunIfAdvisable` on a handed-over `MoveStep` only in `Goal_NextCombatAction`. | Open; either both or neither, or it belongs in the step's `Perform`. |
| `Goal_NextCombatAction` has more fundamental design issues (2026-10-06). | Not a cleanup item; out of scope here. |

### 4. `ActionTrade` is between two NPCs

`Perform` needs an `OrderableAI` on both sides; the static `Can` predicate still admits PCs for historical reasons (`RogueGame.DoChat` guards with `speaker.IsPlayer`).

- The live constructor's DEBUG check had the inverted polarity, and the inlined `Perform` passed the speaker's controller as the target's: both fixed (`6dee9430`).
- The JSON constructor refuses a save where either actor is run by anything else (`2dc0998a`). The controllers are shells by the time controller fields load (two-phase load), so the check is sound there. **Status: done.**

### 5. Coverage

- `goaltest` and `ctest` cover the pruning rules on synthetic objectives; `dangle` round-trips the real saves (turn 7, turn 14) with the audit empty.
- Not covered: a ranged intent whose target is out of line of fire at save (should be kept) against one whose target is dead (dropped); the negative case for issue 4 (an `ActionTrade` naming a zombie refused on load), which needs a crafted `$ref` resolved inside a live load. Both open and optional.

## Order of work, if continued

1. `ActionTake`, then the five `Can*` clauses in issue 1 (small, mechanical; each a legality/performability pair in the `ReasonIllegal ?? ReasonCant` shape the three split actions already use).
2. The `Abort()` parallelism in issue 3.
3. The two design calls in issue 3, if wanted.
