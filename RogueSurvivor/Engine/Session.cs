// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Session
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define BOOTSTRAP_TEST_SERIALIZATION

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using Zaimoni.Data;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine
{
    [Serializable]
    public sealed class Session
    {
        [NonSerialized] public static int COMMAND_LINE_SEED = 0;
        [NonSerialized] public static readonly Dictionary<string, string> CommandLineOptions = new Dictionary<string, string>();
        private static Session s_TheSession;
        [NonSerialized] private static Session? s_Loading = null;

        /// <summary>The session being loaded from JSON, while it is: what a constructor run by the load must consult instead of Get,
        /// which is the live session, or a fresh default one when there is none (its constructor resets the World).</summary>
        public static Session? Loading { get => s_Loading; }

        [NonSerialized] private Scoring_fatality m_Scoring_fatality = null;
        private Scoring m_Scoring = new();
        private readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, string> m_CommandLineOptions;    // needs .NET 4.6 or higher
        public readonly RadioFaction Police = new RadioFaction(Data.GameFactions.IDs.ThePolice, Gameplay.Item_IDs.TRACKER_POLICE_RADIO);

        private static int s_seed = 0;  // We're a compiler-enforced singleton so this only looks weird
        static public int Seed { get { return s_seed; } }

        public readonly UniqueActors UniqueActors = new();
        public readonly UniqueItems UniqueItems = new();
        public readonly UniqueMaps UniqueMaps = new();

        public GameMode GameMode;
        public int LastTurnPlayerActed = 0;
        public bool PlayerKnows_CHARUndergroundFacilityLocation = false;
        public int ScriptStage_PoliceStationPrisoner = 0;
        public int ScriptStage_PoliceCHARrelations = 0;
        public int ScriptStage_HospitalPowerup = 0;

        public static Session Get => s_TheSession ??= new Session();

        // This has been historically problematic.  With the no-skew scheduler, it's simplest to say the world time is just the time
        // of the last district to simulate in a turn -- the bottom-right one.  Note that the entry map is "last" so it will execute last.

        // Groceries are highly demanding and will crash world generation without unusual measures here.
        public WorldTime WorldTime { get { return new WorldTime(World.Get.Last?.EntryMap?.LocalTime ?? new WorldTime(0)); } }

        public Scoring Scoring { get { return m_Scoring; } }
        public Scoring_fatality Scoring_fatality { get { return m_Scoring_fatality; } }

        public static RadioFaction? FromID(GameFactions.IDs id) {
            var police = Get.Police;
            if (police.FactionID == id) return police;
            return null;
        }

#if false
        private Session(GameMode mode = GameMode.GM_STANDARD)
#else
        private Session(GameMode mode = GameMode.GM_WORLD_WAR_Z)
#endif
        {
            m_CommandLineOptions = (0 >= (CommandLineOptions?.Count ?? 0) ? null : new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>(Session.CommandLineOptions)));
            s_seed = (0 == COMMAND_LINE_SEED ? (int)DateTime.UtcNow.TimeOfDay.Ticks : COMMAND_LINE_SEED);
            GameMode = mode;
#if DEBUG
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "Seed: " + s_seed.ToString()); // this crashes if it tries to log during deserialization
#endif
            RogueGame.Reset();
            World.Reset();
        }

    // As the session object is a singleton, we are responsible for save/load of static Session fields.
    static private int field_code(ref Utf8JsonReader reader) {
        if (reader.ValueTextEquals("Seed")) return 1;
        else if (reader.ValueTextEquals("LastTurnPlayerActed")) return 2;
        else if (reader.ValueTextEquals("ScriptStage_PoliceStationPrisoner")) return 3;
        else if (reader.ValueTextEquals("ScriptStage_PoliceCHARrelations")) return 4;
        else if (reader.ValueTextEquals("ScriptStage_HospitalPowerup")) return 5;
        else if (reader.ValueTextEquals("PlayerKnows_CHARUndergroundFacilityLocation")) return 6;
        else if (reader.ValueTextEquals("GameMode")) return 7;
        else if (reader.ValueTextEquals("CommandLineOptions")) return 8;
        else if (reader.ValueTextEquals("Scoring")) return 9;
        else if (reader.ValueTextEquals("Rules")) return 10;
        else if (reader.ValueTextEquals("CreatedCounts")) return 11;
        else if (reader.ValueTextEquals("World")) return 12;
        else if (reader.ValueTextEquals("UniqueMaps")) return 13;
        else if (reader.ValueTextEquals("UniqueItems")) return 14;
        else if (reader.ValueTextEquals("TileLegend")) return 15;
        else if (reader.ValueTextEquals("Player")) return 16;
        else if (reader.ValueTextEquals("UniqueActors")) return 17;
        else if (reader.ValueTextEquals("RefugeePool")) return 18;
        else if (reader.ValueTextEquals("Police")) return 19;
        else if (reader.ValueTextEquals("PlayerUI")) return 20;

        RogueGame.Game.ErrorPopup(reader.GetString());
        throw new JsonException();
    }

    private Session(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      // We are not the live session.  The field initializer for Police subscribed its threat tracking to live game events.
      Police.Detach();
      s_Loading = this;
      try {
      // body not indented, to leave its lines alone
      if (JsonTokenType.StartObject != reader.TokenType) throw new JsonException();
      int origin_depth = reader.CurrentDepth;
      reader.Read();

      Dictionary<string, string> relay_commandline = null;
      UniqueMaps? relay_uniqueMaps = null;
      World? relay_world = null;
      UniqueItems? relay_uniqueItems = null;
      UniqueActors? relay_uniqueActors = null;
      List<Actor>? relay_refugees = null;

      void read(ref Utf8JsonReader reader) {
          int code = field_code(ref reader);
          reader.Read();

          switch (code) {
          case 1:
              s_seed = reader.GetInt32();
              break;
          case 2:
              LastTurnPlayerActed = reader.GetInt32();
              break;
          case 3:
              ScriptStage_PoliceStationPrisoner = reader.GetInt32();
              break;
          case 4:
              ScriptStage_PoliceCHARrelations = reader.GetInt32();
              break;
          case 5:
              ScriptStage_HospitalPowerup = reader.GetInt32();
              break;
          case 6:
              PlayerKnows_CHARUndergroundFacilityLocation = reader.GetBoolean();
              break;
          case 7:
              {
              string stage = reader.GetString();
              if (Enum.TryParse(stage, out GameMode)) return;
              RogueGame.Game.ErrorPopup("unrecognized GameMode " + stage);
              }
              throw new JsonException();
          case 8:
              relay_commandline = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options) ?? throw new JsonException();
              break;
          case 9:
              m_Scoring = JsonSerializer.Deserialize<Scoring>(ref reader, options) ?? throw new JsonException();
              break;
          case 10:
              Rules.Load(ref reader);
              break;
          case 11:
              ActorModel.Load(ref reader);
              break;
          case 12:
              relay_world = World.Load(ref reader);
              break;
          case 15:
              Map.LoadTileLegend(JsonElement.ParseValue(ref reader));
              break;
          case 13:
              relay_uniqueMaps = JsonSerializer.Deserialize<UniqueMaps>(ref reader, options) ?? throw new JsonException();
              break;
          case 14:
              relay_uniqueItems = JsonSerializer.Deserialize<UniqueItems>(ref reader, options) ?? throw new JsonException();
              break;
          case 16:
              JsonElement.ParseValue(ref reader).ReadRef<Actor>(a => m_JsonLoadedPlayer = a);   // a cross-link: set when the load finishes
              break;
          case 17:
              relay_uniqueActors = JsonSerializer.Deserialize<UniqueActors>(ref reader, options) ?? throw new JsonException();
              break;
          case 19:
              Police.LoadJson(JsonElement.ParseValue(ref reader), options);
              break;
          case 20:
              PlayerController.fromJsonUI(JsonElement.ParseValue(ref reader), options, out m_JsonLoadedBeforeAction, out m_JsonLoadedAfterAction);
              break;
          case 18:
              {
              // these are on no map: no Actor.s_LoadMap
              relay_refugees = new();
              foreach (var x in JsonElement.ParseValue(ref reader).EnumerateArray()) relay_refugees.Add(x.Deserialize<Actor>(options) ?? throw new JsonException("null refugee"));
              }
              break;
          }
      }

      while (reader.CurrentDepth != origin_depth || JsonTokenType.EndObject != reader.TokenType) {
          if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

          read(ref reader);

          reader.Read();
      }

      if (null != relay_commandline) m_CommandLineOptions = new(relay_commandline);
      if (null != relay_uniqueMaps) UniqueMaps = relay_uniqueMaps;
      if (null != relay_uniqueItems) UniqueItems = relay_uniqueItems;
      if (null != relay_uniqueActors) UniqueActors = relay_uniqueActors;
      m_JsonLoadedRefugeePool = relay_refugees;
      m_JsonLoadedWorld = relay_world;
      Map.LoadTileLegend(null); // only meaningful for the save it came from

      if (JsonTokenType.EndObject != reader.TokenType) throw new JsonException();

      // Everything with an id is in: the controllers' state can now refer to any of it.
      ActorController.LoadPending(options);
      } finally {
        s_Loading = null;
      }
    }

    // A JSON load stages what it loads here, until JsonTakeLive installs it (or does not: the bootstrap round-trip test re-saves what was loaded).
    [NonSerialized] private World? m_JsonLoadedWorld = null;
    [NonSerialized] private Actor? m_JsonLoadedPlayer = null;   // a cross-link: set when the load finishes
    [NonSerialized] private List<Actor>? m_JsonLoadedRefugeePool = null;   // the live one is RogueGame's
    [NonSerialized] private List<EventUnconditional>? m_JsonLoadedBeforeAction = null;   // the live ones are PlayerController's
    [NonSerialized] private List<EventUnconditional>? m_JsonLoadedAfterAction = null;

    public static Session fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options) => new Session(ref reader, options);

#region JSON post-load, for a session that was loaded from JSON
    /// <summary>Rebuilds what is derived from our own content.  Call once, after PreserveReferenceHandler.Finish.
    /// Safe for a session that is not the live one.</summary>
    public void JsonPostLoad() { m_JsonLoadedWorld!.DoForAllMaps(m => m.JsonPostLoad()); }

    /// <summary>Binds us to the live game: static events, the live session.  Only for a session that is going live.</summary>
    // Only actors on a map, as for the legacy RepairLoad.  A controller cannot TakeControl of an actor that is on no map
    // (a refugee waiting to arrive, an unspawned unique): reviewing its item ratings needs the map's clock.
    public void JsonAttach()
    {
      (m_JsonLoadedWorld ?? World.Get).DoForAllMaps(m => m.JsonAttach());
      Police.Attach();
    }

    public void JsonDetach()
    {
      Police.Detach();
      (m_JsonLoadedWorld ?? World.Get).DoForAllMaps(m => m.JsonDetach());
    }

    /// <summary>Installs us as the live game.  Call once, after JsonPostLoad.  What the legacy load does from OnDeserialized and World.OnDeserialized.</summary>
    private void JsonTakeLive()
    {
      s_TheSession = this;
      m_JsonLoadedWorld!.JsonTakeLive();
      RogueGame.AfterLoad(m_JsonLoadedPlayer, m_JsonLoadedRefugeePool);
      PlayerController.AfterLoad(m_JsonLoadedBeforeAction, m_JsonLoadedAfterAction);
      JsonAttach();
      // from here on, we save the live game
      m_JsonLoadedWorld = null;
      m_JsonLoadedPlayer = null;
      m_JsonLoadedRefugeePool = null;
      m_JsonLoadedBeforeAction = null;
      m_JsonLoadedAfterAction = null;
    }

    /// <summary>For the bootstrap test: derived data is not in the save, so the round trip cannot see it.  Empty if and only if ok.</summary>
    public List<string> JsonDerivedDataErrors()
    {
      List<string> errors = new();
      var live = World.Get;
      var loaded = m_JsonLoadedWorld!;
      for (short x = 0; x < live.Size; x++) for (short y = 0; y < live.Size; y++) {
        var a = live[x, y].Maps.ToList();
        var b = loaded[x, y].Maps.ToList();
        if (a.Count != b.Count) { errors.Add("district " + x.ToString() + "," + y.ToString() + ": map count"); continue; }
        for (int i = 0; i < a.Count; i++) b[i].JsonDerivedDataErrors(a[i], errors);
      }
      return errors;
    }
#endregion

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options) {
      writer.WriteStartObject();
      writer.WriteNumber("Seed", s_seed);
      writer.WriteNumber("LastTurnPlayerActed", LastTurnPlayerActed);
      writer.WriteNumber("ScriptStage_PoliceStationPrisoner", ScriptStage_PoliceStationPrisoner);
      writer.WriteNumber("ScriptStage_PoliceCHARrelations", ScriptStage_PoliceCHARrelations);
      writer.WriteNumber("ScriptStage_HospitalPowerup", ScriptStage_HospitalPowerup);
      writer.WriteBoolean("PlayerKnows_CHARUndergroundFacilityLocation", PlayerKnows_CHARUndergroundFacilityLocation);
      writer.WriteString("GameMode", GameMode.ToString());
      if (null != m_CommandLineOptions) {
        writer.WritePropertyName("CommandLineOptions");
        JsonSerializer.Serialize(writer, m_CommandLineOptions, options);
      }
      writer.WritePropertyName("Scoring");
      JsonSerializer.Serialize(writer, m_Scoring, options);
      writer.WritePropertyName("Rules");
      JsonSerializer.Serialize(writer, Rules.Get, options);
      writer.WritePropertyName("CreatedCounts");
      ActorModel.Save(writer);
      writer.WritePropertyName("TileLegend");   // must precede World: the maps' tiles are numbers into this
      Map.SaveTileLegend(writer);
      writer.WritePropertyName("World");
      JsonSerializer.Serialize(writer, m_JsonLoadedWorld ?? World.Get, options);
      {
      var player = (null != m_JsonLoadedWorld) ? m_JsonLoadedPlayer : RogueGame.Player;
      if (null != player) writer.WriteRef("Player", player);
      // Refugees waiting to arrive are on no map, so nothing else saves them.  Must follow World, as must UniqueActors:
      // an actor that a map or corpse already saved is a reference here.
      var refugees = (null != m_JsonLoadedWorld) ? m_JsonLoadedRefugeePool : RogueGame.RefugeePool;
      if (null != refugees) writer.WriteArray("RefugeePool", refugees, options);
      }
      writer.WritePropertyName("UniqueMaps");
      JsonSerializer.Serialize(writer, UniqueMaps, options);
      writer.WritePropertyName("UniqueItems");
      JsonSerializer.Serialize(writer, UniqueItems, options);
      writer.WritePropertyName("UniqueActors");
      JsonSerializer.Serialize(writer, UniqueActors, options);
      writer.WritePropertyName("Police");
      Police.toJson(writer, options);
      // last: its entries name actors that World or UniqueActors defined
      if (null != m_JsonLoadedWorld) PlayerController.toJsonUI(writer, "PlayerUI", options, m_JsonLoadedBeforeAction, m_JsonLoadedAfterAction);
      else PlayerController.toJsonUI(writer, "PlayerUI", options);
      writer.WriteEndObject();
    }

        public static void Reset(GameMode mode) => s_TheSession = new Session(mode);

        public bool CMDoptionExists(string x) {
            if (null == m_CommandLineOptions) return false;
            return m_CommandLineOptions.ContainsKey(x);
        }

        public void ForcePoliceKnown(Location loc) {   // for world creation
            var allItems = Map.AllItemsAt(loc);
            if (null == allItems) {
                Police.ItemMemory.Set(loc, null, 0);
            } else {
                HashSet<Gameplay.Item_IDs> seen_items = new();
                foreach (var inv in allItems) seen_items.UnionWith(inv.Inventory.Select(x => x.InventoryMemoryID));
                Police.ItemMemory.Set(loc, seen_items, 0);
            }
        }

        public void LatestKill(Actor killer, Actor victim)
        {
            m_Scoring_fatality = new Scoring_fatality(killer, victim);
        }

        /// <summary>The game's save: the stable format, then in a DEBUG build the test format beside it (phase 3 step 7).
        /// Stable first, so a test-format bug can never cost the game.  The build property adds the round-trip self-check.</summary>
        public static void SaveGame(Session session, string filepath)
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
            var errors = new List<string>();
            World.Get._RejectInventoryDamage(errors);
            if (0 < errors.Count) throw new InvalidOperationException("inventory damage pre-save: " + string.Join("\n", errors));
#endif
            Save(session, filepath, StableFormat);
#if DEBUG
            SaveTestFormat(session, filepath);
#endif
#if BOOTSTRAP_TEST_SERIALIZATION
            RoundTripTest(filepath);
#endif
        }

#if DEBUG
        /// <summary>The test save is derived from the stable one, so a failure costs only the file: it is deleted, so that its absence is the signal.</summary>
        private static void SaveTestFormat(Session session, string filepath)
        {
            if (TestFormat is not { } format) return;   // nothing under experiment
            var path = TestPath(filepath);
            try {
                Save(session, path, format);
            } catch (Exception ex) {
                try { File.Delete(path); } catch (Exception) { }
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "test-format save failed: " + ex.ToString());
                RogueGame.Game.ErrorPopup("test-format save failed: " + ex.Message);
            }
        }
#endif

        public static void Save(Session session, string filepath, SaveFormat format)
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            switch (format) {
                case SaveFormat.FORMAT_JSON:
                    SaveJson(session, filepath);
                    break;
            }
        }

        private void RepairLoad()
        {
            World.Get.RepairLoad();
        }

        public static bool Load(string filepath, SaveFormat format)
        {
#if DEBUG
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
            switch (format) {
                case SaveFormat.FORMAT_JSON: return LoadJson(filepath);
                default: return false;
            }
        }

        /// <summary>The format the game plays on and backs up: what the menus and --start=load read.</summary>
        public static SaveFormat StableFormat { get => SaveFormat.FORMAT_JSON; }

        /// <summary>The format under experiment, written beside every stable save in a DEBUG build and read by --start=load-test;
        /// null when there is none.  May break at any time: the file is derived, and any stable save re-renders into it.</summary>
        public static SaveFormat? TestFormat { get => null; }

        /// <summary>the test-format save that goes with a stable-format save</summary>
        public static string TestPath(string filepath) => Path.ChangeExtension(filepath, ".test");

    private static JsonSerializerOptions? s_j_opts = null;
    public static JsonSerializerOptions JSON_opts {
      get {
        if (null == s_j_opts) {
          s_j_opts = new();
          s_j_opts.ReferenceHandler = new Zaimoni.JSON.PreserveReferenceHandler();
          s_j_opts.WriteIndented = true;
          s_j_opts.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;   // a save file is not HTML: < > and non-ASCII as themselves (the tag Goal_LastAction<ActionCloseDoor>)
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.DiceRoller());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.District());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Random());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Rules());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Scoring());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.UniqueMaps());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.World());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.WorldTime());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Vector2D_short());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Box2D_short());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Location());
          // AI state: value types (phase 3 step 1)
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Percept<object>());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Percept<Actor>());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.PerceptOfTag());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Scent());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.ZoneLoc());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.InvOrigin());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.ActorOrder());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.ActorDirective());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Waypoint());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Message());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.MessageManager());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.PathToTarget());
          // AI state: sensors (phase 3 step 3)
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.LOSSensor());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.SmellSensor());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.MemorizedSensor<Gameplay.AI.Sensors.LOSSensor>());

          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.ActorTag());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.UniqueItems());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.UniqueActors());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Sealed<Inventory>(true));

          var items = new Zaimoni.JSON.Polymorphic<Item>(true);
          items.Add(new Zaimoni.JsonConvert.Via<Item, Item>("Item"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemAmmo>("ItemAmmo"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemBarricadeMaterial>("ItemBarricadeMaterial"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemBodyArmor>("ItemBodyArmor"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemEntertainment>("ItemEntertainment"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemFood>("ItemFood"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemGrenade>("ItemGrenade"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemLight>("ItemLight"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemMedicine>("ItemMedicine"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemMeleeWeapon>("ItemMeleeWeapon"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemRangedWeapon>("ItemRangedWeapon"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemSprayPaint>("ItemSprayPaint"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemSprayScent>("ItemSprayScent"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemTracker>("ItemTracker"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Data._Item.PrimedExplosive>("PrimedExplosive"));
          items.Add(new Zaimoni.JsonConvert.Via<Item, Items.ItemTrap>("ItemTrap"));
          items.RequireAllSubtypes();
          s_j_opts.Converters.Add(items);

          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Sealed<Actor>());   // its controllers are tracked separately, below
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Sealed<Corpse>(true));

          var minds = new Zaimoni.JSON.Polymorphic<ActorController>();
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, PlayerController>("PlayerController"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.CHARGuardAI>("CHARGuardAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.CivilianAI>("CivilianAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.FeralDogAI>("FeralDogAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.GangAI>("GangAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.InsaneHumanAI>("InsaneHumanAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.RatAI>("RatAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.SewersThingAI>("SewersThingAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.SkeletonAI>("SkeletonAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.SoldierAI>("SoldierAI"));
          minds.Add(new Zaimoni.JsonConvert.Via<ActorController, Gameplay.AI.ZombieAI>("ZombieAI"));
          minds.RequireAllSubtypes();
          s_j_opts.Converters.Add(minds);

          // the player UI's pending events (Session's PlayerUI); the two lists share entries, so these have ids
          var ui_events = new Zaimoni.JSON.Polymorphic<EventUnconditional>(true);
          ui_events.Add(new Zaimoni.JsonConvert.Via<EventUnconditional, UIOnSighting>("UIOnSighting"));
          ui_events.Add(new Zaimoni.JsonConvert.Via<EventUnconditional, UITracking>("UITracking"));
          ui_events.RequireAllSubtypes();
          s_j_opts.Converters.Add(ui_events);

          // Actions: only those an objective may hold, which [Serializable] marks.  The rest are transient.
          var acts = new Zaimoni.JSON.Polymorphic<ActorAction>(true);
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionChain>("ActionChain"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionCloseDoor>("ActionCloseDoor"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionDropItem>("ActionDropItem"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionGiveTo>("ActionGiveTo"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionMeleeAttack>("ActionMeleeAttack"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionMoveDelta>("ActionMoveDelta"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionPull>("ActionPull"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionRangedAttack>("ActionRangedAttack"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionSequence>("ActionSequence"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionSleep>("ActionSleep"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionTake>("ActionTake"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionTrade>("ActionTrade"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionUse>("ActionUse"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionUseItem>("ActionUseItem"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine.Actions.ActionWait>("ActionWait"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.BehavioristABC>("BehavioristABC"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.Fork>("Fork"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.Join>("Join"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.MoveStep>("MoveStep"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.PushOnto>("PushOnto"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.TakeItem>("TakeItem"));
          acts.Add(new Zaimoni.JsonConvert.Via<ActorAction, Engine._Action.TradeItem>("TradeItem"));
          acts.RequireAllSubtypes(t => t.IsSerializable);
          s_j_opts.Converters.Add(acts);

          var plans = new Zaimoni.JSON.Polymorphic<WorldUpdate>(true);
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Actions.OpportunityTarget>("OpportunityTarget"));
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Actions.UpdateMoveDelta>("UpdateMoveDelta"));
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Op.Chain>("Chain"));
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Op.Fork>("Fork"));
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Op.Join>("Join"));
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Op.MoveStep>("MoveStep"));
          plans.Add(new Zaimoni.JsonConvert.Via<WorldUpdate, Engine.Op.PushOnto>("PushOnto"));
          plans.RequireAllSubtypes(t => t.IsSerializable);
          s_j_opts.Converters.Add(plans);

          // Objectives: likewise the [Serializable] ones.  A plan shared between cooperating actors has an id of its own.
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Sealed<Gameplay.AI.Goals.SharedPlan>());
          var goals = new Zaimoni.JSON.Polymorphic<Gameplay.AI.Objective>();
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_RestRatherThanLoseturnWhenTired>("Goal_RestRatherThanLoseturnWhenTired"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_RecoverSTA>("Goal_RecoverSTA"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_MedicateSLP>("Goal_MedicateSLP"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_MedicateHP>("Goal_MedicateHP"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_RechargeAll>("Goal_RechargeAll"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_Butcher>("Goal_Butcher"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_NextAction>("Goal_NextAction"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_NextCombatAction>("Goal_NextCombatAction"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_NonCombatComplete>("Goal_NonCombatComplete"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_LastAction<Engine.Actions.ActionCloseDoor>>("Goal_LastAction<ActionCloseDoor>"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_DoNotPickup>("Goal_DoNotPickup"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_BreakLineOfSight>("Goal_BreakLineOfSight"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_Terminate>("Goal_Terminate"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_PathTo>("Goal_PathTo"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_PathToStack>("Goal_PathToStack"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_HintPathToActor>("Goal_HintPathToActor"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goal_BreakBarricade>("Goal_BreakBarricade"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.AcquireLineOfSight>("AcquireLineOfSight"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.BlacklistExits>("BlacklistExits"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.ClearZone>("ClearZone"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.Cooperate>("Cooperate"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.FleeExplosive>("FleeExplosive"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.InferActor>("InferActor"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.PathTo>("PathTo"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.PathToStack>("PathToStack"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.Resupply>("Resupply"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.StageAction>("StageAction"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Gameplay.AI.Goals.Trade>("Trade"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Engine.Goal.DeathTrapped>("DeathTrapped"));
          goals.Add(new Zaimoni.JsonConvert.Via<Gameplay.AI.Objective, Engine.Goal.NextAction>("NextAction"));
          goals.RequireAllSubtypes(t => t.IsSerializable);
          s_j_opts.Converters.Add(goals);

          var objs = new Zaimoni.JSON.Polymorphic<MapObject>(true);
          objs.Add(new Zaimoni.JsonConvert.Via<MapObject, MapObject>("MapObject"));
          objs.Add(new Zaimoni.JsonConvert.Via<MapObject, ShelfLike>("ShelfLike"));
          objs.Add(new Zaimoni.JsonConvert.Via<MapObject, MapObjects.Board>("Board"));
          objs.Add(new Zaimoni.JsonConvert.Via<MapObject, MapObjects.DoorWindow>("DoorWindow"));
          objs.Add(new Zaimoni.JsonConvert.Via<MapObject, MapObjects.Fortification>("Fortification"));
          objs.Add(new Zaimoni.JsonConvert.Via<MapObject, MapObjects.PowerGenerator>("PowerGenerator"));
          objs.RequireAllSubtypes();
          s_j_opts.Converters.Add(objs);

          var tasks = new Zaimoni.JSON.Polymorphic<TimedTask>(true);
          tasks.Add(new Zaimoni.JsonConvert.Via<TimedTask, djack.RogueSurvivor.Engine.Tasks.TaskRemoveDecoration>("TaskRemoveDecoration"));
          tasks.Add(new Zaimoni.JsonConvert.Via<TimedTask, djack.RogueSurvivor.Engine.Tasks.TaskEscapeNanny>("TaskEscapeNanny"));
          tasks.RequireAllSubtypes();
          s_j_opts.Converters.Add(tasks);

          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Map());
          s_j_opts.Converters.Add(new Zaimoni.JsonConvert.Session());
        }
        return s_j_opts;
      }
    }

#if BOOTSTRAP_TEST_SERIALIZATION
    /// <summary>The self-check, after SaveGame wrote its formats: the test save (the stable one, when nothing is under experiment) must
    /// load, agree on derived data, and save again as the same text.  Does not notice what is not saved at all.  Then the converter audit.</summary>
    private static void RoundTripTest(string filepath)
    {
      var json_path = (null == TestFormat) ? filepath : TestPath(filepath);
      var json_path2 = json_path+"2";   // only survives a failed round-trip test
      RogueGame.Game.ErrorPopup("JSON save ok");
      var test2 = LoadJsonStaged(json_path);
      {
      var json_derived = test2.JsonDerivedDataErrors();
      if (0 < json_derived.Count) throw new InvalidOperationException("JSON load, derived data:\n" + string.Join("\n", json_derived));
      }
      RogueGame.Game.ErrorPopup("JSON load ok");
      SaveJson(test2, json_path2);
      var json_diff = Zaimoni.JSON.JSON_ext.FirstDifference(File.ReadAllLines(json_path), File.ReadAllLines(json_path2));
      if (null != json_diff) throw new InvalidOperationException("JSON round trip: "+ json_path + " vs. "+ json_path2 + "\n" + json_diff);
      File.Delete(json_path2);
      var json_incomplete = Zaimoni.JSON.Audit.IncompleteConverters(JSON_opts);
      var json_audit = 0 >= json_incomplete.Count ? "all JSON converters complete" : "incomplete JSON converters (" + json_incomplete.Count.ToString() + "): " + string.Join(", ", json_incomplete);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, json_audit);
      RogueGame.Game.ErrorPopup("JSON round trip ok; " + json_audit);
    }
#endif

    // JSON is the stable format since phase 4 (2026-10-05).  A save or load that fails part-way leaves ids and
    // fixups behind: abandon them, or they poison the next attempt.  (Finish would throw again, on the fixups whose targets never loaded.)
    private static void SaveJson(Session session, string filepath)
    {
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving JSON session...");
      try {
        // Two passes.  The first, to nowhere, learns which objects anything refers to; the second writes $id only for those, and an
        // object with no id is a value, which its converter may put on one line.  The pre-save pruning hooks run twice: they are idempotent.
        PreserveReferenceHandler.PrimeFromCountingPass(() => JsonSerializer.Serialize(Stream.Null, session, typeof(Session), JSON_opts));
        using (var stream = filepath.CreateStream(true)) {
          JsonSerializer.Serialize(stream, session, typeof(Session), JSON_opts);
          stream.Flush();
        }
        PreserveReferenceHandler.Finish();
      } catch {
        PreserveReferenceHandler.Reset();
        throw;
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving JSON session... done!");
    }

    /// <summary>Loads, through the post-load stage, but does not install: the result is not the live session.</summary>
    private static Session LoadJsonStaged(string filepath)
    {
      try {
        Session ret;
        using (var stream = filepath.CreateStream(false)) {
          ret = JsonSerializer.Deserialize<Session>(stream, JSON_opts) ?? throw new JsonException("no session");
        }
        PreserveReferenceHandler.Finish();
        ret.JsonPostLoad();
        return ret;
      } catch {
        PreserveReferenceHandler.Reset();
        ActorController.AbandonPending();
        throw;
      }
    }

    private static bool LoadJson(string filepath)
    {
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading JSON session...");
#if DEBUG
#else
      try {
#endif
        LoadJsonStaged(filepath).JsonTakeLive();
#if DEBUG
#else
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load JSON session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        return false;
      }
#endif
#if DEBUG
      var errors = new List<string>();
      World.Get._RejectInventoryDamage(errors);
      if (0 < errors.Count) throw new InvalidOperationException("inventory damage on load: " + string.Join("\n", errors));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading JSON session... done!");
      return true;
    }

    public static bool Delete(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "deleting saved game...");
      bool flag = false;
      try {
        File.Delete(filepath);
#if DEBUG
        File.Delete(TestPath(filepath));   // the derived test save goes with it
#endif
        flag = true;
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to delete saved game (no save?)");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "exception :");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, ex.ToString());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failing silently.");
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "deleting saved game... done!");
      return flag;
    }

    // game mode support
    public static string DescGameMode(GameMode mode)
    {
      switch (mode) {
#if false
        case GameMode.GM_STANDARD: return "STD - Standard Game";
#endif
        case GameMode.GM_CORPSES_INFECTION: return "C&I - Corpses & Infection";
        case GameMode.GM_VINTAGE: return "VTG - Vintage Zombies";
        case GameMode.GM_WORLD_WAR_Z: return "WWZ - World War Z";
        default: throw new ArgumentOutOfRangeException(nameof(mode),(int)mode,"unhandled game mode");
      }
    }

    public static string DescShortGameMode(GameMode mode)
    {
      switch (mode) {
#if false
        case GameMode.GM_STANDARD: return "STD";
#endif
        case GameMode.GM_CORPSES_INFECTION: return "C&I";
        case GameMode.GM_VINTAGE: return "VTG";
        case GameMode.GM_WORLD_WAR_Z: return "WWZ";
        default: throw new ArgumentOutOfRangeException(nameof(mode),(int)mode,"unhandled game mode");
      }
    }

#if false
    public bool HasImmediateZombification {
      get { return GameMode.GM_STANDARD == GameMode || GameMode.GM_WORLD_WAR_Z == GameMode; }
    }

    public bool HasInfection {
      get { return GameMode.GM_STANDARD != GameMode && GameMode.GM_WORLD_WAR_Z != GameMode; }
    }

    public bool HasCorpses {
      get { return GameMode.GM_STANDARD != GameMode; }
    }
#else
    public bool HasImmediateZombification { get => GameMode.GM_WORLD_WAR_Z == GameMode; }
    public bool HasInfection { get => GameMode.GM_WORLD_WAR_Z != GameMode; }
    public const bool HasCorpses = true;
#endif

    public bool HasEvolution {
      get { return GameMode.GM_VINTAGE != GameMode; }
    }

    public bool HasAllZombies {
      get { return GameMode.GM_VINTAGE != GameMode; }
    }

    public bool HasZombiesInBasements {
      get { return GameMode.GM_VINTAGE != GameMode; }
    }

    public bool HasZombiesInSewers {
      get { return GameMode.GM_VINTAGE != GameMode; }
    }

    public enum SaveFormat
    {
//    FORMAT_BIN_XXX,  // BinaryFormatter: retired 2026-10-05 (phase 4)
//    FORMAT_SOAP, // dropped from .NET Core
//    FORMAT_XML,  // incompatible with access control
      FORMAT_JSON  // the stable format since phase 4; see SaveGame
    }
  }
}

namespace Zaimoni.JsonConvert
{
    public class Session : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.Session>
    {
        public override djack.RogueSurvivor.Engine.Session Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            return djack.RogueSurvivor.Engine.Session.fromJson(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.Session src, JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
