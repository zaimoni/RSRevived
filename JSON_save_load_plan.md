# JSON save/load plan

Target: a loadable, playable JSON save by the .NET 11 upgrade (early November 2026).
AI planning state is the tail that stays `Incomplete` past that date.

Recorded 2026-10-01.

## Where things stand

- Production save is still `BinaryFormatter`, through the `System.Runtime.Serialization.Formatters` package.
- JSON is `Session.SaveFormat.FORMAT_JSON`, the test format: `Session.Save`/`Session.Load` take it, and a JSON-loaded session goes live (phases 0-2 done, 2026-10-02). It is not offered from the game's menus; the command line reaches it (see "Test drive" below).
- A DEBUG build writes the test save beside every stable save (`Session.SaveGame`, phase 3 step 7, 2026-10-05). Under `BOOTSTRAP_TEST_SERIALIZATION` (`dotnet build -p:TestFormat=true`; off in a plain build), every save also runs the save-then-reload round trip self-check and audits the converters.
- The roles are named in code, the encodings only where an encoding is meant (phase 4 step 1, 2026-10-05): `Session.StableFormat` and `Session.TestFormat` (nullable: nothing under experiment) resolve to `SaveFormat` values per build; `Session.TestPath` pairs `save.test` with `save.dat`; `--start=load-test`. `SaveJson`/`LoadJson` and the `m_JsonLoaded*` staging fields keep their names: they are about JSON.
- A converter's namespace says whether it is finished: `Zaimoni.JsonConvert` (complete) or `Zaimoni.JsonConvertIncomplete`. The audit has been empty since 2026-10-05 (phase 3 step 6). `Map` completed 2026-10-02 with the `BgMusic` derivation.
- Two roles, two formats (see "Stable and test formats" below): the **stable** format is what the game plays on and backs up; the **test** format is derived from it at every save in a DEBUG build, and may break at any time. Today stable is binary and test is JSON; phase 4 makes JSON stable.
- Polymorphic roots so far: `Item` (16), `MapObject` (6), `TimedTask` (2), `ActorController` (11). Phase 3 adds `Objective` (~30), `ActorAction` / `WorldUpdate` (~33), and the observers, sensors and percepts.
- A custom `JsonConverter<Base>` bypasses the built-in `[JsonDerivedType]` handling, just as it bypasses `ReferenceHandler`. So `$type` is hand-rolled alongside the existing `$id`/`$ref`.

## Test drive (command line)

Options that drive the program rather than the game, held in `Engine.TestDrive`, parsed in `Program.Main`. They never enter `Session.CommandLineOptions` (so are never saved): they describe a test run, not the game being played.

- `--start=new`: skip the main menu into the new-game dialogs, which `--spawn` and its relatives fill in.
- `--start=load`: skip the menu and load the default save (`Saves/save.dat`, the stable format). Documented in the manual.
- `--start=load-test` (was `load-json` until phase 4 step 1): skip the menu and load the test-format save beside it (`Saves/save.test`; `Session.TestPath`). Not documented: the test format may break at any time. Cannot start when the build has no test format.
- `--save-and-exit`: at the first point a player would be asked to act (`RogueGame.HandlePlayerActor`), save to the default slot, then quit. Works after any of the three starts, and after a start from the menu. (Both formats: explicitly until phase 3 step 7, then because a DEBUG save always writes both.)

With `--save-and-exit` the run is unattended: `ErrorPopup` and `InfoPopup` go to the log instead of waiting for ESC, an unhandled exception is logged instead of shown in the bug-report dialog, and the process exit code is non-zero when the run failed (`TestDrive.ExitCode`). A `--start` that cannot start (no save to load) ends an unattended run as a failure, and shows the menu otherwise.

The end-to-end JSON test is two runs: `--start=new --spawn=… --save-and-exit`, then `--start=load-json --save-and-exit`. The second run's `save.json` should match the first's, up to what the load changes (controllers reload with amnesia until phase 3).

## Design

1. **`$type` dispatch.** One generic dispatcher converter per root. It writes `$id`, then `$type`, then fields. On read it handles `$ref`, otherwise reads the metadata and dispatches through a tag-to-converter registry.
2. **Tags.** Short registered strings, not CLR type names: they survive renames and cannot load arbitrary types. Closed generics (`Goal_LastAction<T>`, `Percept_<T>`) are registered individually.
3. **Completeness signal.** Each concrete type registers through a generic wrapper that exists in both namespaces, e.g. `Zaimoni.JsonConvertIncomplete.Via<Item, ItemFood>`. A root is complete when all its registered subtypes are in `Zaimoni.JsonConvert`.
4. **Inheritance on read.** The dispatcher parses each polymorphic object to a `JsonElement`; each constructor in the chain picks out its own fields. Streaming stays for the non-polymorphic bulk (tile arrays).
5. **Ownership and references.** Owned objects get context from their container, as `Map.s_LoadOwner` does now. Only true cross-links use `$ref`.
6. **Forward references.** A cross-link may point at an object not yet loaded (an Actor's leader in a later map). The resolver separates "id assigned" from "definition written"; loading keeps a fixup list. Fields set by fixup cannot be `readonly`.

## Decisions made

- **`JsonElement` staging** for polymorphic objects, not pure streaming (2026-10-01). It costs an allocation per object, but gives order-independence and lets `$id` register before children load.

- **Controller amnesia is acceptable as an intermediate state** (2026-10-01): AIs may reload with no memory until phase 3.

## Open decisions

See "Not yet decided" under Phase 2.

## Phases

### Phase 0: infrastructure (week 1)

- [x] **Resolver changes** (`Microsoft/PreserveReference.cs`, `Zaimoni/Data/JSON.cs`, `Engine/Session.cs`)
  - Write side: `Reference(value)` assigns an id without claiming the definition was written, so `SaveAsRef` can forward-reference. `GetReference(value, out alreadyExists)` reports whether the *definition* has been written.
  - Read side: `TryResolve`, and `Defer(id, fixup)`, which runs at once if the id is known and otherwise when `AddReference` sees it.
  - `PreserveReferenceHandler.Finish()` replaces the manual `Reset()` calls: it throws if an id was referenced but never defined (save) or a fixup is still waiting (load), and resets either way.
  - The deferral half has no consumer until Actors.
- [x] **Factor out the `$id` handling** marked `\todo` in `District` and `Map`
  - Read: `reader.ReadMetadata(out string? id, out string? type)` consumes `$id`/`$type` in either order and leaves the reader on the first real property.
  - Write: the converter calls `writer.WriteStartDefinition(id, type)`, then a fields-only `toJsonFields(writer, options)`, then `WriteEndObject`. (Not named `WriteStartObject`: `Utf8JsonWriter.WriteStartObject(string propertyName)` would win overload resolution.)
  - The constructor registers `this` immediately after reading the metadata, not at the end. A `Location` inside a map's own contents cannot resolve its map otherwise.
- [x] **Dispatcher**: `Zaimoni.JSON.Polymorphic<TBase> : JsonConverter<TBase>` (`Zaimoni/Data/JsonPolymorphic.cs`)
  - `CanConvert` accepts any type assignable to `TBase`, so fields declared as a derived type still dispatch.
  - Interface-typed fields (`Observer<Location[]>`, `UsableItem`) get their own instance listing the implementers.
- [x] **Registry and completeness wrapper**
  - Contract: `IJsonStaged<TSelf>` with `static abstract TSelf fromJson(JsonElement, JsonSerializerOptions)` and `void toJsonFields(Utf8JsonWriter, JsonSerializerOptions)`.
  - Entry: abstract `Subtype<TBase>` (tag, CLR type, `Load`, `Save`), implemented identically by `Zaimoni.JsonConvert.Via<TBase,T>` and `Zaimoni.JsonConvertIncomplete.Via<TBase,T>`.
  - DEBUG: `RequireAllSubtypes()` throws while building the options if any non-abstract subtype of a root is unregistered. Open generic definitions are skipped; their closed instantiations are registered one by one.
  - Type-side pattern: a `(JsonElement src, JsonSerializerOptions options)` constructor chained through the base class, which calls `src.RecordRef(this)` first; a `public virtual toJsonFields` that calls `base.toJsonFields` first; an explicit `static T IJsonStaged<T>.fromJson`.
  - Proved on `TimedTask`: `TaskRemoveDecoration` complete; `TaskEscapeNanny` incomplete (its escapee list needs Actor). `Map` does not write its timers yet (phase 1).
- [x] **Round-trip test**: save, load, save again, compare the two JSON texts. Runs from `Session.SaveGame` (`RoundTripTest`; was inside `SaveBin` until phase 3 step 7) under `BOOTSTRAP_JSON_SERIALIZATION`.
  - A mismatch throws, naming the first differing line; the second text is left beside the save as `<save>json2`.
  - It cannot notice a field that is not saved at all: both texts omit it.
  - `Session` keeps the JSON-loaded `World` (`m_JsonLoadedWorld`), since it is not taken live; re-saving a loaded session writes that one.
  - Writers must be deterministic given load order. First case: `Map_Links` is now loaded in save order, so each map's exits iterate as they were saved.
- [x] **Audit** listing every `Incomplete` converter and registry entry: `Zaimoni.JSON.Audit.IncompleteConverters(options)`. JSON becomes a real `SaveFormat` only when the list is empty.
  - Controlled by `BOOTSTRAP_JSON_SERIALIZATION`, not `DEBUG`: the list is logged and shown in the "JSON round trip ok" popup. (Until phase 3 step 7 ordinary DEBUG play never built `JSON_opts`; since the parallel save, `RequireAllSubtypes()` runs at the first DEBUG save, and the audit stays with the self-check.)
  - It only sees registered converters. A type with no converter at all (`Actor`, `Item`, ...) is not listed until something registers it.

### Phase 1: map contents without actors (weeks 2-3)

- [x] **`Item`**: 16 concrete types under `Polymorphic<Item>`; tags are the class names.
  - Base fields: `Model` (the `Item_IDs` name), `Qty` (omitted when it is 1 and the model is not stackable), `Equipped` (omitted when `NONE`).
  - Each subclass checks its model is the right kind on load.
  - `ItemTrap` is incomplete: owner and known-by are Actor references, reloaded empty.
  - `Engine/Items/ItemGrenadePrimed.cs` and `ItemPrimedExplosive.cs` are not in the csproj and were left alone.
- [x] **`Inventory`**: `Zaimoni.JsonConvert.Sealed<Inventory>` (`$id`, no `$type`); `MaxCapacity` and `Items`.
- [x] **Owned collections are bare JSON arrays** (`writer.WriteArray`), with no `$id`/`$values` wrapper.
- [x] **Wiring**: `Map` writes `GroundItems` as `[position, inventory]` pairs; `Session` writes `UniqueItems`.
- [x] **`MapObject`**: six concrete types under `Polymorphic<MapObject>`, all complete.
  - Base fields: `ID` (the `IDs` name). Implied when omitted: `HitPoints` (the model's maximum), `Break` (what the constructor chooses for that `ID`), `Fire` (`UNINFLAMMABLE`).
  - State is written by name by the subclass: `DoorWindow` `CLOSED`/`OPEN`/`BROKEN` plus `Barricade` (omitted when 0), with its door/window type implied by `ID`; `PowerGenerator` `ON`/`OFF`.
  - `Board` writes `Text`; `ShelfLike` writes its `Inventory` in full.
  - Location is not saved. `Map` writes `MapObjects` as `[position, object]` pairs and sets each location on load with `RepairLoad`, not `PlaceAt`.
- [x] **`TimedTask` wired into `Map`** as a bare `Timers` array; the tasks' own point collections are bare arrays too.
- [x] **Tile IDs**: `Map` writes `Tiles` row-major, one row of numbers per line, after `Extent`.
  - `Session` writes one `TileLegend` before `World`: the tile name for each number. On load the numbers are translated through it, so a save survives the tile IDs being renumbered or extended. A used tile whose legend name is unknown is rejected.
  - Deliberately not a Base64 blob: that would interfere with extending configuration files.
- [x] **Remaining exits**: not saved. The JSON `World` constructor regenerates them with `District.InterpolateExits` once all districts are loaded; that is the only source of exits leaving a map's bounds. The round-trip test cannot see them, so they are checked in the headless harness only.
- [x] **Decorations**: `Map` writes `Decorations` as `[position, [image IDs]]` pairs, one per line (`writer.WriteArrayOfLines`, shared with `Tiles`). Image IDs are written verbatim; whether an image exists is not checked on load.
  - Deferred as micro-optimizations: grouping by image, and stripping the common path prefix from image IDs.
- [x] **Scents**: `Map` writes `Scents` as `[position, {odor: strength}]` pairs, one per line. The object form relies on there being at most one scent of each odor at a position.
  - Rejected on load: an unknown or numeric odor, an odor repeated at a position, a position with no scents, a strength outside `MIN_STRENGTH`..`MAX_STRENGTH` (the `Strength` setter would silently clamp it).
  - None expected at turn zero: the first in-game exercise is a save a few turns in.

Phase 1 is complete. What `Map` still does not save is everything involving actors: the actor list and corpses. Three other `Map` fields are deliberately not saved:

- `Seed`: only used in map generation.
- `BgMusic`: not saved; derived from the name on load (`Map.DeriveBgMusic`). The generator code that chooses a map's name also chooses its music (every `new Map(…)` in `BaseTownGenerator` passes both), so a prefix table mirrors it: `Sewers@`, `Subway@`, `basement` (house and shop basements) → sewers; `Hospital - ` → hospital; `Police Station - ` → surface; `CHAR Underground Facility` → its own; anything else is a district entry map → surface. No `UniqueMaps` lookup was needed: the entry map is the only kind without a prefix, and the catalog loads after `World` anyway. The generator stays the source of truth; a DEBUG check in `Map.toJsonFields` throws, naming the map, when the table disagrees with what the generator passed, so a new or renamed map kind is caught the first time it is saved.
- `IsSecret`: do not wire in. It waits for a feature redesign and a big-bang integration. The original intent was to keep the CHAR Underground Base out of [police] pathfinding and to put the map in stasis; the stasis effect is to be eliminated in a storyline revision. The legacy `GetObjectData` does not write it either, so it is already lost on a legacy load.

Readability rule: the JSON is meant to be read, so no packing of the `Item_s` kind. Omitting a field whose value is implied is fine.

### Phase 2: actors (weeks 3-5)

Controllers are saved as `$type` only, so AIs reload with amnesia. This is the November milestone.

Design:

- **Owners define, cross-links only reference.** An actor is written in full where it lives: a map's actor list, a corpse (its dead actor), `UniqueActors` (not yet spawned), the refugee pool. Everything else writes a bare `$ref`.
- The actor writes its `Position`; the owning map supplies the map on load.
- Cross-link fixups run once, at the end of the load, in file order. Firing each when its target happens to load would reorder lists and dictionaries and fail the round-trip test.
- A controller's constructor takes its actor, so `ActorController` gets the `s_LoadOwner` pattern `Map` uses for its district.
- Recomputed on load, not saved: `m_Leader` (from the leader's followers), current attack and defence (from equipped items).
- Implied when omitted: hit points, stamina, food, sleep and sanity equal to the model's base value; each "previous" value equal to the current one.

Steps:

- [x] 1. Resolver: ordered end-of-load fixups; helpers for a single reference, a list of references, a dictionary keyed by reference.
  - `Defer` now always queues, even when the id is already loaded; `PreserveReferenceHandler.Finish()` runs the queue in the order deferred, then validates. This replaces the Phase 0 behaviour (run at once if known, else when the id loads).
  - Until `Finish()`, every cross-linked field, list and dictionary is empty. Nothing during the load may rely on them.
  - Helpers in `JSON_ext`: `WriteRef`/`ReadRef`, `WriteRefs`/`ReadRefs` (one `{"$ref":…}` per line), `WriteRefDictionary`/`ReadRefDictionary` (`[reference, value]` per line).
  - Ids are assigned on first mention, so a forward-referenced object has a lower id than objects defined before it. Ids are not in definition order.
- [x] 2. Actor core: `Sealed<Actor>` (model, faction, name, flags, spawn time, stats, skills, doll, inventory, inventory slots, controller type). `Map` writes `Actors` as a bare array in list order (turn order). `Session` writes the player as a `$ref`.
  - `Name` is always written: it is expected to differ from the model's name (non-human actors get their instance number suffixed).
  - `Controller` is omitted only when it is truly the default, i.e. what the model would create with no saved state. That test gets more specific as controller save/load is built out.
  - On load the controller field is assigned directly. `TakeControl`, the actors-by-position cache and `CommandLinePlayer` belong to the post-load stage (step 7).
  - A `PlayerController` can be constructed without the UI, so the headless harness can cover the player path. `RogueGame.HandlePlayerActor` cannot run headless.
  - `Actor` registers as incomplete (`JsonConvertIncomplete.Sealed<Actor>`) until its cross-links and `ActorScoring` are saved.
  - Eleven controller types under `Polymorphic<ActorController>`, all incomplete: `PlayerController` and the ten AIs. `ActorController.IsJsonDefault` is the "truly default" test; today it only compares the type with the model's default controller.
  - `Location` rejects a null map in DEBUG, so the actor cannot be built with a position alone. `Actor.s_LoadMap` holds the owning map while it loads its `Actors`; an actor saved with a `Position` but loaded outside a map is rejected.
  - Also implied when omitted: `ActionPoints` at the model's speed; `Infection`, `OdorSuppressorCounter`, `TrustInLeader` at zero; `Activity` when `IDLE`; flags that are false; `Slots` when all empty.
  - `Session` keeps the loaded player in `m_JsonLoadedPlayer`, so re-saving a loaded session writes that one.
- [x] 3. Actor cross-links (target, followers, trust, aggressor and self-defence lists) and `ActorScoring`.
  - **Pre-save pruning.** The legacy save runs `[OnSerializing]` hooks; the JSON save now calls the same ones from `toJsonFields`: `Actor` (dead target, dead followers/aggressors), `Map` (dead actors out of its list), `Inventory` (zero-quantity items), `ItemTrap` (dead owner and known-by). Saving therefore mutates live state, as the legacy save does. `ItemEntertainment`'s hook is compiled out (`#if PROTOTYPE`) and is not called.
  - `Target` is a single `$ref`; `Followers`, `AggressorOf`, `SelfDefenceFrom` are lists of `$ref`s; `Trust` is `[reference, value]` pairs. Each is omitted when empty, and the list stays null on load.
  - The leader is not written: the fixup that fills a leader's `Followers` sets each follower's leader.
  - Dead keys in the trust dictionary are skipped on write, without touching the live dictionary. Revisit once corpses are saved (step 4): a revivable corpse should keep its trust.
  - `Scoring` (the actor's `ActorScoring`) is written only when it has something in it: `Achievements`, `FirstKills`, `KillCounts`, `Sightings`, `Events` (`[turn, text]` per line), `VisitedMaps` (`$ref`s), `DeathReason`.
  - `Actor` stays incomplete until the dragged corpse is saved (step 4).
- [x] 4. Corpses: `Sealed<Corpse>`, `Corpses` on `Map`, the dragged-corpse link.
  - A corpse owns its dead actor: `DeadGuy` is written in full inside the corpse, with its `Position` and `"Dead": true`. A corpse has no location of its own. `Map` holds `Actor.s_LoadMap` while loading `Corpses`, as it does for `Actors`.
  - `Turn`, `HitPoints` and `Rotation` are always written. Implied when omitted: `MaxHitPoints` equal to the dead actor's current maximum, a `Scale` of 1.
  - Loaded through a constructor that does not add the corpse to its map; the corpses-by-position cache belongs to the post-load stage (step 7).
  - `DraggedCorpse` on the actor is a `$ref`.
  - Trust entries: written when the actor is alive, or dead with a corpse (`Map.HasCorpseOf`, by identity). A dead actor without a corpse is still skipped. The untested `World.CorpseFrom(ActorTag)`/`Map.CorpseFrom(ActorTag)` are not used here; they matter where only a tag is stored (step 6).
  - `Corpse` and `Actor` are now complete. The controllers are tracked separately and remain incomplete.
- [x] 5. `UniqueActors` and the refugee pool. `ItemTrap` and `TaskEscapeNanny` become complete.
  - No explicit ownership test. `Session` writes `RefugeePool` and then `UniqueActors` after `World`, and both write their actors the ordinary way: an actor a map or corpse already saved becomes a `$ref`; anything else is defined there.
  - `Actor.s_SaveMap` is the write-side mirror of `s_LoadMap`: an actor writes `Position` only while its own map is saving it. An actor saved by the catalog or the pool writes none, and loads with no map.
  - Accepted change in state: a dead unique whose corpse is gone loads with a null map. Dead actors leave their map's list at turn end/start; after that the only access is through the unique actor catalog.
  - `UniqueActors` is an object keyed by unique name; each entry has `Actor`, `Spawned`, and `WithRefugees`/`Music`/`Message` when set. A unique not generated yet is absent.
  - `RogueGame.RefugeePool` is a read-only accessor, so `RogueGame.cs` needs no JSON handling. The loaded pool is kept on the session (`m_JsonLoadedRefugeePool`).
  - `ItemTrap` writes `Owner` and `KnownBy`; `TaskEscapeNanny` writes `Escapees`, dropping the dead first as `Trigger` does.
  - A save or load that throws part-way must be abandoned with `PreserveReferenceHandler.Reset()`, not `Finish()`: `Finish` would throw again on the fixups whose targets never loaded. The bootstrap block in `Session.SaveBin` now does this.
- [x] 6. Police `RadioFaction`: item memory, `ThreatTracking`, the investigate set, event log, rankings. `Session` writes a `Police` object last; `FactionID` and `RadioID` are not saved, since whoever constructs the faction knows them.
  - [x] 6a. `Threats`: one entry per threat, `{"Actor": reference, "Where": [[map reference, [points]], …]}`. `Investigate`: `[map reference, [points]]` per line. Both are cross-links, filled when the load finishes.
    - **Event handlers.** `ThreatTracking`'s constructor subscribes to the static `Actor.Dies`/`Actor.Moving` events, and `Session.Police` is a field initializer, so every constructed `Session` subscribes one. The JSON `Session` constructor now calls `Police.Detach()` first. Re-attaching a loaded session's tracker belongs to the post-load stage (step 7). Before this, each bootstrap save left one more subscribed tracker behind.
    - Generic containers get enumerators, not read accessors: `RelationC` has a `GetEnumerator` over its private list.
  - [x] 6b. `ItemMemory` (`Ary2Dictionary<Location, Item_IDs, int>`): one `{"Map": reference, "Seen": […]}` per map; each line of `Seen` is `[x, y, turn]` where nothing was there, or `[x, y, turn, item kinds…]`. Item kinds by name. In `ItemMemoryJSON`, a helper not tied to `RadioFaction`: `PlayerController` holds the same type.
    - `Ary2Dictionary` gets two enumerators (what was seen; where nothing was seen). Its third dictionary, the by-item index, is derived: `Set` rebuilds it on load.
    - **Size.** The police are told about every tile of every entry map at world creation (`District.EntryMap`'s setter calls `ForcePoliceKnown`), so "nothing seen at turn 0" lines dominate the file: 44,097 of 53,215 lines in the headless test world. Deferred size optimizations: a legend for item kinds; a denser form for the nothing-seen entries (for example runs along a row).
  - [x] 6c. Event log and rankings, one entry per line. These hold `ActorTag`s, not references; only their locations' maps are cross-links.
    - `EventLog`: `{"Subject": tag, "Seen": [[x, y], map reference], "SeenTurn", "Verb", "Object": tag, "Turn"}`.
    - `Quick`: `{"Who": tag, "Rank": [t0, survival, kill, achievement]}`. `Dead`: likewise, then `"Died"`, `"Killer"` (omitted when there was none), `"Where"`.
    - Event verbs are written as words (`FactionKill`, `FactionAggression`, `Murder`, `Assault` for `v_code` 1..4): `SVOevent.v_code` is a raw number because its data design is not finished (context probably matters). The words exist only in the save format. A code with no word is written as its number.
    - `Ranking` and `Demise` gained field-wise constructors.
- [x] 7. Post-load stage for a JSON-loaded session: derived caches and `RepairLoad`. Then (pulled forward from after phase 3, for the command-line test drive) installing the loaded world and player, and JSON as a save format.
  - Two stages, because only one is safe on a session that is not live.
  - **`Session.JsonPostLoad()`**: derived data that depends only on the loaded session. Per map: actors and corpses by position, map geometry, the pathing cache clock. Call once, after `PreserveReferenceHandler.Finish()`. The bootstrap test runs it.
    - The map's lazy caches (`Players`, `Viewpoints`, `Police`, `PowerGenerators`, `destination_maps`) are `readonly`, so the JSON `Map` constructor assigns them.
    - `Map.ReconstructAuxiliaryFields` took a parameter so that this stage can skip its `PlayerController.InstallHandlers()` call.
  - **`Session.JsonAttach()` / `JsonDetach()`**: binding to the live game. `TakeControl` then `RepairLoad` for each actor on a map; re-attaching the police threat tracker.
    - Only actors on a map, as for the legacy `RepairLoad`. A controller cannot `TakeControl` of an actor on no map: `CivilianAI` reviews item ratings, and `Actor.HasEnoughFoodFor` reads the map's clock. In the live game such an actor took control at construction, with an empty inventory.
    - Not carried over from the legacy path: `MapObject.RepairLoad` (the JSON load already set the location; a second call throws in DEBUG) and `Map.AfterLoad(District)`.
  - **Bootstrap check of derived data** (`Session.JsonDerivedDataErrors()`), since the round-trip text cannot see it. By-position caches and the `Players`/`PowerGenerators` caches are checked against the loaded map's own lists, because the live map may still hold actors that died this turn. Geometry and destination maps are compared with the live map.
  - **Taking a loaded session live** (`Session.JsonTakeLive`, from `Session.Load(…, FORMAT_JSON)`): what the legacy load does from `Session.OnDeserialization` and `World.OnDeserialized`. Installs the session (`s_TheSession`), the world (`World.JsonTakeLive`), the player and refugee pool (`RogueGame.AfterLoad(Actor?, List<Actor>?)`), then `JsonAttach`. The staging fields (`m_JsonLoadedWorld` and friends) are then cleared, so the live game is what saves next. The counted commands and message log are reset: they are not in the save until phase 3.
  - `Session.Save(…, FORMAT_JSON)` / `Session.Load(…, FORMAT_JSON)`: `SaveJson`, `LoadJsonStaged` (through the post-load stage, not live: what the bootstrap round trip uses) and `LoadJson`. Each abandons the resolver (`PreserveReferenceHandler.Reset`) when it fails part-way, so a failed attempt cannot poison the next. `RogueGame.LoadGame` takes the format; the after-load repairs (zone exits, zone walk, sensors) run for both formats.

Not yet decided:

- Whether `Controller` is omitted when it is the model's default controller (implied while controllers carry no state).
- References to actors nobody owns (a dead actor with no corpse, still in a trust or aggressor list). The save-side check will throw and name the field; decide the rule from what a real save shows.

### Phase 3: AI state (started 2026-10-04)

Objectives, actions, sensors and percepts. The rule (2026-10-03): a field tagged `[NonSerialized]` is rebuilt after load; every other field is persisted. For actions, `[Serializable]` on the class is the existing marker for "can be stored in an objective", so it is the registry for `Polymorphic<ActorAction>`; an unregistered action reaching the save is an error naming the holding objective.

Steps (each round-tripped in the headless harness first):

- [x] 1. Value types: `Percept_<T>` (payload keyed by kind: `Actor`, `Inventory`, `Stack`, `Corpses`, `MapObject`, `Text`; no `$type`, the kinds are closed and unrelated), `Percept_s<AIScent>`, `ZoneLoc` (`[map, rectangle]`; its `Zone` is `[NonSerialized]`), `InvOrigin` (whichever of `Actor`/`Shelf`/`Location` is set), `ActorOrder`, `ActorDirective` (only what differs from the defaults; `{}` when nothing does), `Waypoint_s`, `Message`/`MessageManager` (colors: a known color by name, any other as `#AARRGGBB`; the text is stored as shown, since the live constructor prefixes the turn), `PathToTarget`. `LinearChokepoint` was suppressed as unwired before this step, so `SparseData.UsingChokepoint` has nothing to save.
  - These hold references inside structs and readonly fields, so they resolve **now** (`JSON_ext.RequireRef`, `RequireRefs`), not at `Finish`. The referent must already be loaded. Inside a map's actor list that is not guaranteed (an order or a path may name another map, or an actor on it), so **controllers must load after `World`**: step 5 stages each actor's controller JSON and constructs it once all maps are in. Cross-links that live in class fields keep using the deferred `ReadRef`.
- [x] 2. `Polymorphic<ActorAction>` (22 classes) and `Polymorphic<WorldUpdate>` (7): exactly the `[Serializable]` ones. `RequireAllSubtypes(t => t.IsSerializable)` checks both directions in DEBUG: every `[Serializable]` subclass registered, nothing else registered. Tags are bare class names; `Fork`, `Join`, `MoveStep`, `PushOnto` name one class under each root (namespaces `_Action` and `Op`), so there are 25 distinct tags.
  - `m_Actor` is implied: the actor whose controller is being saved (`ActorController.s_SaveOwner`, set by `Actor.toJsonFields`) or loaded (`s_LoadOwner`). An action for a different actor writes `"Actor": {$ref}`, which the loader prefers. `m_FailReason` is `[NonSerialized]` (2026-10-03), so each class writes only its own fields.
  - Each class has a private JSON constructor that assigns its fields, because the live constructors do more: reset the actor's `Activity` (`ActionWait`, `ActionDropItem`, `ActionMeleeAttack`, `ActionUseItem`), read the live clock and action points (`ActionSequence`: the saved `AP`/`Turn` are what count), or demand pathing legality and a controller kind that hold at save and are not for a load to re-check.
  - `ActionGiveTo.m_Target` is an `OrderableAI`; it is written as that controller's **actor**, since a controller with no state of its own is not in the save at all (`IsJsonDefault`). The loader takes `actor.Controller as OrderableAI`. For step 5 this means an actor's controller object must exist before any other controller's state loads: construct the controller shell when the actor loads, fill its fields after `World`.
  - Also for step 5: `ActionRangedAttack` rebuilds its `oai` from `m_Actor.Controller` at construction, so the owning controller must already be assigned to its actor when its objectives load.
  - Omitted when default: `Free` (close door), `Running` (move steps), `FireMode` when aimed, `Join`'s absent sequel. `LoF` is points on one line; `Fork`'s options are `[[cost, [actions]], …]`; `PushOnto`'s object code is a bit field, written as a number; behaviors and reflexes by enum name.
- [x] 3. Sensors: `LOSSensor`, `SmellSensor`, `MemorizedSensor<LOSSensor>`, as nested objects of the leaf controllers (an interface held only in typed fields: no root, no `$type`). The classes are `internal`, so their converters are too.
  - `LOSSensor`: `Filters` by name (`SensingFilter` is now `[Flags]` with no `NONE` member, so a combination renders as `"ACTORS, ITEMS"`); `Friends`/`Enemies` as `[location, $ref]` pairs filled at `Finish` (`JSON_ext.WriteRefValueDictionary`/`ReadRefValueDictionary`); `Items` as `[location, stack]` pairs. The viewpoint is the implied owner (`ActorController.WriteActorUnlessOwner`/`ActorOrOwner`, which the actions now share). The legacy `OptimizeBeforeSaving` runs from `toJson`: a dead viewpoint's caches are dropped, live and in the file.
  - `MemorizedSensor<T>`: `Persistance`, the nested `Sensor`, `Memory` (step 1's percepts, omitted when empty). `Forget()` runs from `toJson` as the legacy save did, so stale percepts never reach the file; note it replaces the list object.
  - `SmellSensor`: `Odor` by name, `Scents` when any.
  - For step 5: the leaf AIs initialise their smell sensors in field initialisers; the JSON constructors overwrite those `readonly` fields in the constructor body.
- [x] 4. `Polymorphic<Objective>`: the 30 `[Serializable]` classes (`SetTrap` is not in the project), `Goal_LastAction<ActionCloseDoor>` registered under that tag. The base writes `Turn`, `Expired` when true, and the actor only when not the implied owner; each class has a private `(Actor, JsonElement, options)` constructor chaining to `Objective(Actor, JsonElement)`, assigning the saved state as it was: the live constructors re-derive, filter and check legality, which is not for a load.
  - `SharedPlan` is one object shared by every cooperating actor's `Cooperate`: `Sealed<SharedPlan>` gives it an `$id`. `Condition<Actor>` has one implementor, so the target is keyed by its name (`"Target": {"CoverTrap": [locations]}`); a second implementor would make it a `$type` root.
  - `BlacklistExits`: exits have no ids, no origin field, and are compared by reference. One is written as `[from, to]`; the load takes `from.Map.GetExitAt(from.Position)` and refuses it, naming both, unless it still leads to `to`. The origin is found at save time with `Map.FindExitOrigin` over the actor's map, the maps of its district and the destination's, and the destination's own exits' maps (2026-10-04: an in-memory micro-optimization kept as is).
  - The `oai`/`ordai` back-pointers are recovered through the classes' existing `[OnDeserialized]` hooks, from `m_Actor.Controller`: the owning controller must be assigned to its actor before its objectives load (step 5). `ClearZone`'s `threats`/`tourism` stay with `RepairLoad` at the post-load stage.
  - `JSON_opts.Encoder` is `UnsafeRelaxedJsonEscaping`: a save file is not HTML, so `<`, `>` and non-ASCII are written as themselves (the generic tag, actor names). The bootstrap round trip is unaffected: both sides use the options.
  - Omitted when default: `Expired`, `Walking`, `Turns` at `int.MaxValue`, an absent intent slot (`Goal_NextCombatAction`, `NextAction`, `Goal_HintPathToActor`), `Alternates`, `Unverified`.
- [x] 5. Controller layers (2026-10-04). Flat in the one `$type`d object, base layers first, through virtual `toJsonFields`/`loadJsonFields` chains.
  - **Two-phase load.** An actor's load constructs its controller as the live game would (a shell: `JsonLoaded` registers it and stages the `JsonElement` on `ActorController.s_JsonPending`); `Session`'s JSON constructor calls `ActorController.LoadPending` as its last act, so every controller's fields are read once the whole `World` is in, in file order, with `s_LoadOwner` set. A failed load `AbandonPending`s. This is what lets objectives, actions and sensors resolve references now, and lets `ActionGiveTo`, `ActionRangedAttack` and the `oai` back-pointers find another actor's controller already constructed.
  - **`Session.Loading`.** The shell constructors run before the loaded session is the live one; `ObjectiveAI`'s faction watchers (`TryToClearZones`, `ImplicitRadio`, `LookingForCHARBase`) therefore come from `Session.Loading ?? Session.Get`. Without it a menu load would bind police AIs to a fresh default session (whose constructor also resets the `World`). Pre-existing and flagged, not fixed: `LookingForCHARBase.IsSecret` caches the CHAR base `Map` in a static across sessions.
  - `BaseAI`: `PrevLocation` when on a map. `ObjectiveAI`: `Objectives` (before `FOVevents`, which are `$ref`s to the objectives among the watchers; the faction watchers are re-added by the constructor), `PlannedMoves`, `Sparse` keyed by `SparseData` name with each key's own value type (`MinStepPath`'s two forms told apart by element shape), `STA_reserve`, `RatingOverrides`, `LastMove`, `Recoil`. **`ItemPriorities` is not written**: `ObjectiveAI.TakeControl` recomputes it (`ReviewItemRatings`, relocated from `CivilianAI` on 2026-10-04), so it is derived for a load; the binary save still carries it (`[NonSerialized]` failed big-bang testing there). `OrderableAI`: taboo trades, directive, order, the four last-seen percepts, patrol/report state, `DontFollowLeader`; the legacy pre-save hooks (the dead forget what they saw; dead taboo-trade partners dropped, now for every `OrderableAI` rather than `CivilianAI` alone, since the dead are not in the save) run from `toJsonFields`.
  - Leaves: `LOS`/`MemLOS`/`Smell`/`MasterSmell` (the fields lost `readonly` so the staged load can assign them), `Squad`, `Gang`, `SafeTurns`, `Emotes` when not the class default, `Exploration` (`ExplorationData`: locations, and zones as `[map, index into its zones]`; a zone does not know its map, so the save looks in the actor's map and the explored locations' maps).
  - `IsJsonDefault` is now `type == model default && !HasJsonState`, with `HasJsonState` a virtual each layer extends. Any AI that has acted has a `PrevLocation` and is written.
  - The audit lists `PlayerController` and `Session`. The headless harness (`ctest`) round-trips a whole session with controller state; the harness world needed `Direction_ext.Now()` before saving, as the game has each turn: the sensors' pre-save hooks compute FOV.
  - **Pre-save pruning, from a real turn-17 save** (2026-10-04; `dangle` harness walks each controller's object graph to name what refers to an unsaved object). 288 unsaved references were: expired `AcquireLineOfSight` objectives still watching FOV (19), emptied ground inventories in `m_LastItemsSaw` (60), dead undead with no corpse (209). Added, as `[OnSerializing]` hooks that the JSON `toJsonFields` also call: `ObjectiveAI` drops `IsExpired` objectives from `Objectives` and `FOVevents`, then drops those whose `Objective.UrgentAction_readonly` returns true with a null action; `OrderableAI` nulls `m_LastItemsSaw` when its inventory is empty; `LOSSensor` drops dead actors from a live viewpoint's `_friends`/`_enemies`.
    - `UrgentAction_readonly` is `UrgentAction` without side effects: a virtual defaulting to `UrgentAction`, overridden where that acts (18 of 30): pathing, speech, staging, binding a shared plan, `SetObjective`, per-turn decrements, or deliberation (`Resupply`, `Goal_DoNotPickup`, `Goal_RechargeAll` answer false, never pruned). `Goal_NextAction`, `Goal_NextCombatAction`, `NextAction` and `Goal_LastAction<T>` are one-shots that expire on every call, so they override too, or a save would consume them. `UrgentAction` must not be called at save time: it edits `Objectives` while they are being enumerated, and reaches into pathing.
    - `Goal_HintPathToActor` reads its deadline from the actor's map clock in both paths (was `Session.Get.WorldTime`, the last district's, which can be a tick behind).
    - At save time only `IsLegal()` decides (2026-10-05): `Goal_NextAction`, `Goal_NextCombatAction`, `StageAction` and `Goal_NonCombatComplete` (which got its own override for this) prune on an intent that is no longer legal, never on one that is merely not performable now; a save is not "now", and a stored intent is usually waiting out exactly that. Trap noted twice now: inside `namespace Zaimoni.JsonConvert`, a bare `Location` or `ActorTag` is the converter class, not the data type; alias the data type.
    - Two more from the first full round trip of a real save (turn 7, 31.5 MB, 1.1M lines: save, load, post-load, re-save identical). Under a reference handler the default collection path writes `{"$id", "$values": […]}`, so nested collections go through `JSON_ext.WriteArrayValue`/`WriteArrayOfArrays` (`MinStepPath`, `PathToTarget` steps, `Fork` options, `Goal_Terminate` targets, `CoverTrap`); the file has no `$values` left. And a staged load has not run `RepairLoad` (that is for a live session), so `ClearZone.UrgentAction_readonly` answers false while its `threats`/`tourism` are unbound: the bootstrap's re-save of the loaded session would otherwise throw.
    - `OrderableAI.m_LastEnemySaw` and `m_LastSoldierSaw` are `Percept_<ActorTag>` (2026-10-04, a binary savefile break): a last sighting names the actor by tag, so a long-dead one is no longer a reference to an unowned object. Written as `{"Turn", "Location", "ActorTag": …}` (`Zaimoni.JsonConvert.PerceptOfTag`). The one remaining reference of that kind in the turn-17 save, a live `Goal_NextCombatAction` whose engaged intent attacks a dead actor, is left as is.
- [x] 6. `PlayerController` and the player UI statics (2026-10-05). `Session` completes; the audit is empty.
  - The controller object, after the `ObjectiveAI` layers: `LOS` (the sensor), `ItemMemory` (`ItemMemoryJSON`, **omitted for a police-faction actor**: the constructor binds a police player to `Session.Police.ItemMemory` by reference, so it is implied and already under `Police`; the load mirrors the constructor's rule, and the constructor reads `Session.Loading ?? Session.Get`), `Waypoints` when any, `Messages` (the `MessageManager`; `RogueGame.setPlayer` adopts the player controller's as the live log, so a load needs nothing more), `CountedCommand: [count, "MOVE_N"]` when `RogueGame` holds one for this controller. A `PlayerController` is never its model's default, so it is always written. `m_LOSSensor` and `Messages` lost `readonly` for the staged load.
  - **Counted commands stay in `RogueGame`** (two `internal static` accessors: read one controller's, install one); the format already keeps each with its controller, so moving the static later changes no file. A load parks the command on the controller (`[NonSerialized]`), and `PlayerController.TakeControl` installs it when the loaded session goes live; `RogueGame.AfterLoad` still clears the live list first. A pending counted command pre-empts the keypress a save needs, so the key is almost always absent.
  - **`PlayerUI`**, a `Session`-level key written last: `{"BeforeAction": [...], "AfterAction": [...]}`, entries under `Polymorphic<EventUnconditional>` (`UIOnSighting`: `Actor`, `Text`, `Music`; `UITracking`: `Actor`). They are per-session state shared by every `PlayerController` (multi-PC), so they do not hang off one controller; `Reset()` builds the after list as a copy of the before list, so the shared entries are `$ref`s there, which is why the root has ids. Written last so that `UIOnSighting.who` (readonly) can `RequireRef` an actor `World` or `UniqueActors` defined. Staged on the session (`m_JsonLoadedBeforeAction`/`AfterAction`, like the refugee pool) and installed by `JsonTakeLive` through `PlayerController.AfterLoad`. Not rebuilt from `Reset()`: that would re-arm sightings that already fired, and `KnowsWhere` reads the tracking entries.
    - Pruned at save: an entry whose `Expire` would drop it without acting (`EventUnconditional.IsStale`: an expired sighting, a dead actor). `expired` is therefore implied false. The key is omitted when nothing is left.
  - The turn-7 save round-trips with the audit empty (31.6 MB, 1.1M lines, identical text); `ctest` covers a non-police player's log, waypoint, item memory, parked and then installed counted command, and the shared/stale UI entries.
- [x] 7. Parallel save (respecified and done 2026-10-05; was "JSON offered from the game's own save and load commands", which phase 4 now covers). In a DEBUG build, every write of the stable save is followed by a write of the test save beside it: a save, not a self-test. Stable first, so a test-format bug can never cost the game; a failed test write deletes its partial file and logs (DEBUG: a one-line popup), so a missing test file is the only staleness signal and `--start=load-test` needs no timestamp logic. The test file gets no turn-versioned backup: it is derived, and any `save.N` re-renders into it (`--start=load --save-and-exit`, or `dangle`). The round-trip self-check (save, load, re-save, compare, audit) moves out of the save path and becomes an explicit opt-in under the build property; the test drive and `dangle` are its normal homes. Release builds write and read only the stable format.
  - `Session.SaveGame(session, path)` is the game's one save entry (`RogueGame.DoSaveGame`; the test drive's save-and-exit goes through it and lost its own JSON line): the DEBUG inventory-damage pre-check, `Save(FORMAT_BIN)`, then `SaveTestFormat` (DEBUG) and `RoundTripTest` (build property). `Session.Save(session, path, format)` remains the single-format primitive. Permadeath's `Session.Delete` removes the test sibling too (DEBUG), or a dead game's test file would outlive it.
  - After `--start=load-json --save-and-exit`, the parallel save writes a binary save from a JSON-loaded session: the binary writer's first look at that object graph, and a way back from a test file to stable.

Data design notes that bear on the format:

- **`ActorController` vs. `BaseAI` is a historical accident.** `PlayerController` now derives from `ObjectiveAI`, so the whole `BaseAI` API could be lifted into `ActorController`. The format must not encode that layering: one `ActorController` root, tags are the concrete class names, and base-class fields are written flat in the one object (which the constructor chain already does). Merging the two classes then changes no file.
- **Counted commands belong with `PlayerController`.** They live in `RogueGame` as technical debt (`s_CountedCommands` is already keyed by `PlayerController`). Save them inside the owning player's controller object, not as a `Session`-level field, so paying the debt changes no file.
- The underlying problem: command translation is front-end dependent and has to be isolated from game code. `HandlePlayerActor` inlines what would be `PlayerController.GetAction` and `PlayerController.SelectAction`.
- Also here: the `PlayerController` event handlers and the message log.

### Phase 4: retire BinaryFormatter

1. [x] Rename the test-format plumbing after its role, not its encoding, since JSON is about to be the stable format (2026-10-05): `--start=load-json` → `--start=load-test`, `JsonBootstrap`/`BOOTSTRAP_JSON_SERIALIZATION` → `TestFormat`/`BOOTSTRAP_TEST_SERIALIZATION`, `Session.JsonPath` → `Session.TestPath` (the file is `save.test`: its encoding is whatever is under experiment, and `save.json` is about to mean the stable save), and `Session.StableFormat`/`Session.TestFormat` properties that the build resolves to concrete `SaveFormat` values (`TestFormat` nullable). `RogueGame.LoadGame` takes the format explicitly. Done first, so the commit that removes the binary format is only that.
2. [x] Move `GameOptions`, `Keybindings`, `HiScoreTable` and `GameHintsStatus` to attribute-driven JSON (2026-10-05): `FileExt.JsonSerialize`/`JsonDeserialize` with one options object (indented, enums by name, relaxed escaping), replacing `BinarySerialize`/`BinaryDeserialize` at those four sites. The files are `options.json`, `keys.json`, `hints.json`, `hiscores.json`; the old `.dat` files are left alone, and each loader's existing fallback (defaults; a regenerated table) covers their absence.
   - `GameOptions` is a struct of public properties, so it needs no attributes; a partial file sets what it names and leaves the struct default (not `ResetToDefaultValues`) for the rest.
   - `Keybindings`: `{"Bindings": {"SHOUT": "Y, Control", …}}` through a `[JsonConstructor]` that applies the file over the defaults, so a command the file predates keeps its default key; the reverse dictionary is derived. `HiScore`: fields became get-only properties with a `[JsonConstructor]`; `HiScoreTable`: `MaxEntries` and `Table`, `Count` ignored. `GameHintsStatus`: `{"Given": [hint names]}`; an unknown name or `_COUNT` fails the load, which resets.
   - Harness: `cfgtest` round-trips all four, and checks the partial-file and bad-name paths.
3. JSON becomes `StableFormat`: the menus' save and load, the turn-versioned backups, `--start=load`. Drop the `System.Runtime.Serialization.Formatters` package. `TestFormat` is then whatever encoding is under experiment, or nothing.

## Stable and test formats

Decided 2026-10-05. The question was whether one build should save exactly one format, or a development build both.

- Either way a development build must **load** the stable format: the real saves are the fixtures that found what the harness worlds never did. The question is only whether it keeps a working stable **writer**.
- For exclusive save: with two hand-rolled writers (after phase 4), every data-model change is two edits and two loaders, and the stable writer gets no coverage from play in the dev build. It is also honest about model changes: the old format cannot hold the new state, so the only correct move is "new build loads old version, saves new" — migration on load, needed for players regardless. And one file per slot: no stale sibling to mislead `--start=load-test`.
- For dual save: the developer plays dev builds. Under exclusive save the game in progress is in the unstable format and every unstable change costs it. Under dual save the progression lives in stable and the test file is regenerated from live state at every save, so the test format can break daily for free — which is how phase 3 went: the JSON changed shape a dozen times, the binary saves carried on, and the one stable break was deliberate.
- The distinction that decides it: **encoding changes** (an item-kind legend, run-length nothing-seen entries, the `$values` fix) touch no model and leave the stable writer alone; dual save costs nothing there. **Model changes** force a stable break under either option, and dual save degenerates to exclusive save by itself.
- So: dual save in DEBUG builds through phase 4 and the encoding optimizations, collapsing to one format afterward, when changes are model changes and each is a versioned break with loader migration. The divergence between stable and test is *which converters are registered*: two `JsonSerializerOptions` sharing everything but the converters under experiment, so the format switch is the options object passed, not a global.
- Consequences accepted: every DEBUG save pays the test write (31 MB at turn 7, growing with the item memory: the deferred size work moves up), through the one place the stable save is written, autosaves included; release builds write and read stable only, and `load-test` is simply not there.

## Ordering constraints in the JSON format

- In a `Session`, `RefugeePool` and `UniqueActors` must follow `World`: their actors may be `$ref`s to actors a map or corpse defined.
- In a `Session`, `TileLegend` must precede `World`; in a `Map`, `Extent` must precede `Tiles`. The reader throws otherwise.
- In a `District`, `WorldPos` must precede `Maps`: the `Map` constructor copies `m_District.WorldPosition` into `DistrictPos` and hashes it. The reader throws otherwise.

## A JSON-loaded world going live

- The JSON `Map` constructor does not build the derived caches (`Players`, `PowerGenerators`, and the rest). That is deliberate: they belong to a postprocessing stage after load (`JsonPostLoad`), as in the legacy format, and are not saved.
- Since phase 3 step 6 nothing is missing: AI and player controller state, the message log, counted commands, the player's item memory and the pending UI events all load. Still rebuilt rather than loaded, by design: `ObjectiveAI.ItemPriorities`, the faction watchers, every `[NonSerialized]` field.

## Parked

- Every door and fortification writes `"Fire": "BURNABLE"`. Left as is until it is decided what to do with the inherited, unexercised fire feature.

## Known costs

- `JsonElement` lookups silently ignore unknown field names, where the streaming `field_code` functions reject them. A DEBUG check against a per-type known-name set would restore that.
- Any field filled by a deferred fixup cannot be `readonly`.
