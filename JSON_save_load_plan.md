# JSON save/load plan

Target: a loadable, playable JSON save by the .NET 11 upgrade (early November 2026).
AI planning state is the tail that stays `Incomplete` past that date.

Recorded 2026-10-01.

## Where things stand

- Production save is still `BinaryFormatter`, through the `System.Runtime.Serialization.Formatters` package.
- The JSON path only runs under `BOOTSTRAP_JSON_SERIALIZATION` (not defined in the csproj), as a save-then-reload smoke test inside `Session.SaveBin`.
- A converter's namespace says whether it is finished: `Zaimoni.JsonConvert` (complete) or `Zaimoni.JsonConvertIncomplete`.
- `Map` writes only Name, Lighting, LocalTime, Extent, NextActorIndex and Zones.
- `Session` omits UniqueItems, UniqueActors, Police, the player reference, and the `RogueGame`/`PlayerController`/`Actor` statics.
- No polymorphic root is started:

| Root | Concrete types |
|---|---|
| `Item` | ~16 |
| `MapObject` | 6 |
| `TimedTask` | 2 |
| `ActorController` | 10 |
| `Objective` | ~30 |
| `ActorAction` / `WorldUpdate` | ~33 |
| Observers, sensors, percepts | a handful, some generic |

- A custom `JsonConverter<Base>` bypasses the built-in `[JsonDerivedType]` handling, just as it bypasses `ReferenceHandler`. So `$type` is hand-rolled alongside the existing `$id`/`$ref`.

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
- [x] **Round-trip test**: save, load, save again, compare the two JSON texts. Runs in `Session.SaveBin` under `BOOTSTRAP_JSON_SERIALIZATION`.
  - A mismatch throws, naming the first differing line; the second text is left beside the save as `<save>json2`.
  - It cannot notice a field that is not saved at all: both texts omit it.
  - `Session` keeps the JSON-loaded `World` (`m_JsonLoadedWorld`), since it is not taken live; re-saving a loaded session writes that one.
  - Writers must be deterministic given load order. First case: `Map_Links` is now loaded in save order, so each map's exits iterate as they were saved.
- [x] **Audit** listing every `Incomplete` converter and registry entry: `Zaimoni.JSON.Audit.IncompleteConverters(options)`. JSON becomes a real `SaveFormat` only when the list is empty.
  - Controlled by `BOOTSTRAP_JSON_SERIALIZATION`, not `DEBUG`: the list is logged and shown in the "JSON round trip ok" popup. Ordinary DEBUG play never builds `JSON_opts`, so neither the audit nor `RequireAllSubtypes()` runs there.
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
- `BgMusic`: arguably recoverable from where the map is. Not saved; deriving it on load is still to do.
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

- [ ] 1. Resolver: ordered end-of-load fixups; helpers for a single reference, a list of references, a dictionary keyed by reference.
- [ ] 2. Actor core: `Sealed<Actor>` (model, faction, name, flags, spawn time, stats, skills, doll, inventory, inventory slots, controller type). `Map` writes `Actors` as a bare array in list order (turn order). `Session` writes the player as a `$ref`.
- [ ] 3. Actor cross-links (target, followers, trust, aggressor and self-defence lists) and `ActorScoring`.
- [ ] 4. Corpses: `Sealed<Corpse>`, `Corpses` on `Map`, the dragged-corpse link.
- [ ] 5. `UniqueActors` and the refugee pool. `ItemTrap` and `TaskEscapeNanny` become complete.
- [ ] 6. Police `RadioFaction`: item memory, `ThreatTracking`, the investigate set, event log, rankings.
- [ ] 7. Post-load stage for a JSON-loaded session: derived caches and `RepairLoad`. Installing the loaded world and player, and JSON as a real save format, wait until after phase 3.

Not yet decided:

- Whether `Controller` is omitted when it is the model's default controller (implied while controllers carry no state).
- References to actors nobody owns (a dead actor with no corpse, still in a trust or aggressor list). The save-side check will throw and name the field; decide the rule from what a real save shows.

### Phase 3: AI state (after November)

Objectives, actions, sensors and percepts. Each needs a decision: persist, or rebuild after load.

Data design notes that bear on the format:

- **`ActorController` vs. `BaseAI` is a historical accident.** `PlayerController` now derives from `ObjectiveAI`, so the whole `BaseAI` API could be lifted into `ActorController`. The format must not encode that layering: one `ActorController` root, tags are the concrete class names, and base-class fields are written flat in the one object (which the constructor chain already does). Merging the two classes then changes no file.
- **Counted commands belong with `PlayerController`.** They live in `RogueGame` as technical debt (`s_CountedCommands` is already keyed by `PlayerController`). Save them inside the owning player's controller object, not as a `Session`-level field, so paying the debt changes no file.
- The underlying problem: command translation is front-end dependent and has to be isolated from game code. `HandlePlayerActor` inlines what would be `PlayerController.GetAction` and `PlayerController.SelectAction`.
- Also here: the `PlayerController` event handlers and the message log.

After phase 3: install the loaded world and player (take a JSON-loaded session live), and offer JSON as a save format behind a test define.

### Phase 4: retire BinaryFormatter

Move `GameOptions`, `Keybindings`, `HiScoreTable` and `GameHintsStatus` to attribute-driven JSON, then drop the `System.Runtime.Serialization.Formatters` package.

## Ordering constraints in the JSON format

- In a `Session`, `TileLegend` must precede `World`; in a `Map`, `Extent` must precede `Tiles`. The reader throws otherwise.
- In a `District`, `WorldPos` must precede `Maps`: the `Map` constructor copies `m_District.WorldPosition` into `DistrictPos` and hashes it. The reader throws otherwise.

## Before a JSON-loaded world can go live

- The JSON `Map` constructor does not build the derived caches (`Players`, `PowerGenerators`, and the rest). That is deliberate: they belong to a postprocessing stage after load, as in the legacy format, and are not saved.

## Parked

- Every door and fortification writes `"Fire": "BURNABLE"`. Left as is until it is decided what to do with the inherited, unexercised fire feature.

## Known costs

- `JsonElement` lookups silently ignore unknown field names, where the streaming `field_code` functions reject them. A DEBUG check against a per-type known-name set would restore that.
- Any field filled by a deferred fixup cannot be `readonly`.
