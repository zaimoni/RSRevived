// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Session
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define BOOTSTRAP_Z_SERIALIZATION
// #define INTEGRATE_Z_SERIALIZATION

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Session : ISerializable,IDeserializationCallback
#if BOOTSTRAP_Z_SERIALIZATION
        , Zaimoni.Serialization.ISerialize
#endif
    {
    public static int COMMAND_LINE_SEED;
    public static readonly Dictionary<string, string> CommandLineOptions = new Dictionary<string, string>();
    private static Session s_TheSession;
    private static bool s_IsLoading = false;
    [NonSerialized] private static Map.ActorCode s_Player;

    [NonSerialized] private Scoring_fatality m_Scoring_fatality = null;
    private Scoring m_Scoring;
    private readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, string> m_CommandLineOptions;    // needs .NET 4.6 or higher
    public readonly RadioFaction Police = new RadioFaction(Data.GameFactions.IDs.ThePolice, Gameplay.GameItems.IDs.TRACKER_POLICE_RADIO);

    private static int s_seed = 0;  // We're a compiler-enforced singleton so this only looks weird
    static public int Seed { get { return s_seed; } }

    public World World { get; private set; }
    public UniqueActors UniqueActors { get; private set; }
    public UniqueItems UniqueItems { get; private set; }
    public UniqueMaps UniqueMaps { get; private set; }

    public GameMode GameMode;
    public int LastTurnPlayerActed;
    public bool PlayerKnows_CHARUndergroundFacilityLocation;
    public int ScriptStage_PoliceStationPrisoner;
    public int ScriptStage_PoliceCHARrelations;
    public int ScriptStage_HospitalPowerup;

    public static Session Get { get {
#if DEBUG
      if (s_IsLoading) throw new InvalidOperationException("unsafe game loading");
#endif
      return s_TheSession ??= new Session();
    } }

    // This has been historically problematic.  With the no-skew scheduler, it's simplest to say the world time is just the time
    // of the last district to simulate in a turn -- the bottom-right one.  Note that the entry map is "last" so it will execute last.

    // Groceries are highly demanding and will crash world generation without unusual measures here.
    public WorldTime WorldTime { get { return new WorldTime(World[World.Size-1,World.Size-1]?.EntryMap?.LocalTime ?? new WorldTime(0)); } }

    public Scoring Scoring { get { return m_Scoring; } }
    public Scoring_fatality Scoring_fatality { get { return m_Scoring_fatality; } }

    private Session()
    {
      m_CommandLineOptions = (0 >= (CommandLineOptions?.Count ?? 0) ? null : new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>(Session.CommandLineOptions)));
      Reset();
    }

#region Implement ISerializable
    // general idea is Plain Old Data before objects.
    protected Session(SerializationInfo info, StreamingContext context)
    {
      info.read(ref m_Scoring, "Scoring");
      GameMode = (GameMode) info.GetSByte("GameMode");
      ScriptStage_PoliceStationPrisoner = (int) info.GetSByte("ScriptStage_PoliceStationPrisoner");
      ScriptStage_PoliceCHARrelations = (int) info.GetSByte("ScriptStage_PoliceCHARrelations");
      ScriptStage_HospitalPowerup = (int) info.GetSByte("ScriptStage_HospitalPowerup");
      s_seed = info.GetInt32("Seed");
      LastTurnPlayerActed = info.GetInt32("LastTurnPlayerActed");
      PlayerKnows_CHARUndergroundFacilityLocation = info.GetBoolean("PlayerKnows_CHARUndergroundFacilityLocation");
      info.read_nullsafe(ref m_CommandLineOptions, "CommandLineOptions");
      // load other classes' static variables
      ActorModel.Load(info,context);
      Actor.Load(info,context);
      Rules.Get.Load(info,context);
      PlayerController.Load(info,context);
      // end load other classes' static variables
      World = (World) info.GetValue("World",typeof(World));
      RogueGame.Load(info, context);
      info.read_s(ref s_Player, "s_Player");
      UniqueActors = (UniqueActors) info.GetValue("UniqueActors",typeof(UniqueActors));
      UniqueItems = (UniqueItems) info.GetValue("UniqueItems",typeof(UniqueItems));
      UniqueMaps = (UniqueMaps) info.GetValue("UniqueMaps",typeof(UniqueMaps));
      info.read(ref Police, "Police");
#if DEBUG
      if (Police.FactionID != GameFactions.IDs.ThePolice) throw new InvalidOperationException("police faction id is not police");
#endif
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Scoring",m_Scoring,typeof(Scoring));

      info.AddValue("GameMode",(SByte)GameMode);
      info.AddValue("ScriptStage_PoliceStationPrisoner",(SByte)ScriptStage_PoliceStationPrisoner);
      info.AddValue("ScriptStage_PoliceCHARrelations", (SByte)ScriptStage_PoliceCHARrelations);
      info.AddValue("ScriptStage_HospitalPowerup", (SByte)ScriptStage_HospitalPowerup);
      info.AddValue("Seed",s_seed);
      info.AddValue("LastTurnPlayerActed",LastTurnPlayerActed);
      info.AddValue("PlayerKnows_CHARUndergroundFacilityLocation",PlayerKnows_CHARUndergroundFacilityLocation);
      info.AddValue("CommandLineOptions", m_CommandLineOptions,typeof(System.Collections.ObjectModel.ReadOnlyDictionary<string, string>));
      ActorModel.Save(info,context);
      Actor.Save(info, context);
      Rules.Get.Save(info,context);
      PlayerController.Save(info,context);
      info.AddValue("World",World,typeof(World));
      RogueGame.Save(info, context);
      info.AddValue("UniqueActors",UniqueActors,typeof(UniqueActors));
      info.AddValue("UniqueItems",UniqueItems,typeof(UniqueItems));
      info.AddValue("UniqueMaps",UniqueMaps,typeof(UniqueMaps));
      info.AddValue("Police", Police, typeof(RadioFaction));

      // non-serialized fields
      m_Scoring_fatality = null;
    }

    void IDeserializationCallback.OnDeserialization(object sender)
    {
      RogueGame.AfterLoad(s_Player);
    }

#if BOOTSTRAP_Z_SERIALIZATION
    protected Session(Stream src, Zaimoni.Serialization.DecodeObjects decode)
    {
            sbyte relay = 0;

            Zaimoni.Serialization.Formatter.Deserialize(src, ref relay);
            GameMode = (GameMode)(relay);
            Zaimoni.Serialization.Formatter.Deserialize(src, ref relay);
            ScriptStage_PoliceStationPrisoner = relay;
            Zaimoni.Serialization.Formatter.Deserialize(src, ref relay);
            ScriptStage_PoliceCHARrelations = relay;
            Zaimoni.Serialization.Formatter.Deserialize(src, ref relay);
            ScriptStage_HospitalPowerup = relay;
            Zaimoni.Serialization.Formatter.Deserialize(src, ref s_seed);
            Zaimoni.Serialization.Formatter.Deserialize(src, ref LastTurnPlayerActed);
            Zaimoni.Serialization.Formatter.Deserialize(src, ref relay);
            PlayerKnows_CHARUndergroundFacilityLocation = 0 != relay;

            Dictionary<string, string> opts = null;
            decode.LoadFrom(src, ref opts);
            m_CommandLineOptions = new(opts);

            m_Scoring = decode.LoadInline<Scoring>(src);

            // mockup to allow testing
            var city_size = RogueGame.Options.CitySize;
            World = new World(RogueGame.Options.CitySize);
            UniqueActors = new UniqueActors();
            UniqueItems = new UniqueItems();
            UniqueMaps = new UniqueMaps();
/*
            // load other classes' static variables
            ActorModel.Load(info, context);
            Actor.Load(info, context);
            Rules.Get.Load(info, context);
            PlayerController.Load(info, context);
            // end load other classes' static variables
            World = (World)info.GetValue("World", typeof(World));
            RogueGame.Load(info, context);
            info.read_s(ref s_Player, "s_Player");
            UniqueActors = (UniqueActors)info.GetValue("UniqueActors", typeof(UniqueActors));
            UniqueItems = (UniqueItems)info.GetValue("UniqueItems", typeof(UniqueItems));
            UniqueMaps = (UniqueMaps)info.GetValue("UniqueMaps", typeof(UniqueMaps));
            info.read(ref Police, "Police");
*/
#if DEBUG
      if (Police.FactionID != GameFactions.IDs.ThePolice) throw new InvalidOperationException("police faction id is not police");
#endif

    }

    void SaveLoadOk(Session test) {
        m_Scoring.SaveLoadOk(test.m_Scoring);

        var err = string.Empty;
        if (GameMode != test.GameMode) err += "GameMode != test.GameMode: "+ GameMode.ToString() + " "+ test.GameMode.ToString() + "\n";
        if (ScriptStage_PoliceStationPrisoner != test.ScriptStage_PoliceStationPrisoner) err += "ScriptStage_PoliceStationPrisoner != test.ScriptStage_PoliceStationPrisoner " + ScriptStage_PoliceStationPrisoner.ToString() + " " + test.ScriptStage_PoliceStationPrisoner.ToString() + "\n";
        if (ScriptStage_PoliceCHARrelations != test.ScriptStage_PoliceCHARrelations) err += "ScriptStage_PoliceCHARrelations != test.ScriptStage_PoliceCHARrelations\n";
        if (ScriptStage_HospitalPowerup != test.ScriptStage_HospitalPowerup) err += "ScriptStage_HospitalPowerup != test.ScriptStage_HospitalPowerup\n";
        if (LastTurnPlayerActed != test.LastTurnPlayerActed) err += "LastTurnPlayerActed != test.LastTurnPlayerActed: "+ LastTurnPlayerActed.ToString() + " "+ test.LastTurnPlayerActed.ToString() + "\n";
        if (PlayerKnows_CHARUndergroundFacilityLocation != test.PlayerKnows_CHARUndergroundFacilityLocation) err += "PlayerKnows_CHARUndergroundFacilityLocation != test.PlayerKnows_CHARUndergroundFacilityLocation\n";

        foreach(var x in m_CommandLineOptions) {
          if (test.m_CommandLineOptions.TryGetValue(x.Key, out var cache)) {
            if (x.Value != cache) err += "wrong KV pair, command line options:" + x.Key + ":" + x.Value;
          } else err += "missing key, command line options: " + x.Key;
        }
        foreach(var x in test.m_CommandLineOptions) {
          if (!test.m_CommandLineOptions.TryGetValue(x.Key, out var cache)) err += "extra key, command line options: " + x.Key + ":" + x.Value;
        }

        if (!string.IsNullOrEmpty(err)) throw new InvalidOperationException(err);
    }

    void Zaimoni.Serialization.ISerialize.save(Stream dest, Zaimoni.Serialization.EncodeObjects encode)
    {
            Zaimoni.Serialization.Formatter.Serialize(dest, (sbyte)GameMode);
            Zaimoni.Serialization.Formatter.Serialize(dest, (sbyte)ScriptStage_PoliceStationPrisoner);
            Zaimoni.Serialization.Formatter.Serialize(dest, (sbyte)ScriptStage_PoliceCHARrelations);
            Zaimoni.Serialization.Formatter.Serialize(dest, (sbyte)ScriptStage_HospitalPowerup);
            Zaimoni.Serialization.Formatter.Serialize(dest, s_seed);
            Zaimoni.Serialization.Formatter.Serialize(dest, LastTurnPlayerActed);
            Zaimoni.Serialization.Formatter.Serialize(dest, (sbyte)(PlayerKnows_CHARUndergroundFacilityLocation ? 1 : 0));
            encode.SaveTo(m_CommandLineOptions, dest);
//          encode.LinearSave(m_CommandLineOptions, dest);
            encode.SaveInline(dest, m_Scoring);
            // encode.format.....

/*
            ActorModel.Save(info, context);
            Actor.Save(info, context);
            Rules.Get.Save(info, context);
            PlayerController.Save(info, context);
            info.AddValue("World", World, typeof(World));
            RogueGame.Save(info, context);
            info.AddValue("UniqueActors", UniqueActors, typeof(UniqueActors));
            info.AddValue("UniqueItems", UniqueItems, typeof(UniqueItems));
            info.AddValue("UniqueMaps", UniqueMaps, typeof(UniqueMaps));
            info.AddValue("Police", Police, typeof(RadioFaction));
*/
            // non-serialized fields
            m_Scoring_fatality = null;
    }
#endif
#endregion

    public void Reset()
    {
      s_seed = (0 == COMMAND_LINE_SEED ? (int) DateTime.UtcNow.TimeOfDay.Ticks : COMMAND_LINE_SEED);
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "Seed: "+s_seed.ToString()); // this crashes if it tries to log during deserialization
#endif
      RogueGame.Reset();
      m_Scoring = new Scoring();
      var city_size = RogueGame.Options.CitySize;
      World = new World(city_size);
      LastTurnPlayerActed = 0;
      PlayerKnows_CHARUndergroundFacilityLocation = false;
      ScriptStage_PoliceStationPrisoner = 0;
      ScriptStage_PoliceCHARrelations = 0;
      ScriptStage_HospitalPowerup = 0;
      UniqueActors = new UniqueActors();
      UniqueItems = new UniqueItems();
      UniqueMaps = new UniqueMaps();
      Police.Clear();
    }

    public bool CMDoptionExists(string x) {
      if (null == m_CommandLineOptions) return false;
      return m_CommandLineOptions.ContainsKey(x);
    }

    public void ForcePoliceKnown(Location loc) {   // for world creation
      var allItems = Map.AllItemsAt(loc);
      if (null == allItems) {
        Police.ItemMemory.Set(loc, null, 0);
      } else {
        var seen_items = new HashSet<Gameplay.GameItems.IDs>();
        foreach(var inv in allItems) seen_items.UnionWith(inv.Items.Select(x => x.InventoryMemoryID));
        Police.ItemMemory.Set(loc, seen_items, 0);
      }
    }

#nullable enable
    // thin-wrappers; not suppressing these is technical debt.
    public void SetLastRaidTime(RaidType raid, Map map) => World.SetLastRaidTime(raid, map);
#nullable restore

    public void LatestKill(Actor killer, Actor victim, string death_loc)
    {
      m_Scoring_fatality = new Scoring_fatality(killer,victim,death_loc);
    }

    public static void Save(Session session, string filepath, SaveFormat format)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
#if DEBUG
      var errors = new List<string>();
      session.World._RejectInventoryDamage(errors);
      if (0 < errors.Count) throw new InvalidOperationException("inventory damage pre-save: " + string.Join("\n", errors));
#endif
      switch (format) {
        case SaveFormat.FORMAT_BIN:
          SaveBin(session, filepath);
          break;
      }
    }

    private void RepairLoad()
    {
      World.RepairLoad();
    }

    public static bool Load(string filepath, SaveFormat format)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      switch (format) {
        case SaveFormat.FORMAT_BIN: return LoadBin(filepath);
        default: return false;
      }
    }

    private static void SaveBin(Session session, string filepath)
    {
#if DEBUG
      if (null == session) throw new ArgumentNullException(nameof(session));
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
	  filepath.BinarySerialize(session);
#if BOOTSTRAP_Z_SERIALIZATION
	  Zaimoni.Serialization.Virtual.BinarySave(filepath+"test", session);
#if INTEGRATE_Z_SERIALIZATION
      // immediate integation test
      var compare = Zaimoni.Serialization.Virtual.BinaryLoad<Session>(filepath + "test");
      session.SaveLoadOk(compare);
#endif
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadBin(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");
#if DEBUG
#else
      try {
#endif
        s_IsLoading = true;
        s_TheSession = filepath.BinaryDeserialize<Session>();
        s_IsLoading = false;
        s_TheSession.RepairLoad();
#if DEBUG
#else
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        return false;
      }
#endif
#if DEBUG
      var errors = new List<string>();
      s_TheSession.World._RejectInventoryDamage(errors);
      if (0 < errors.Count) throw new InvalidOperationException("inventory damage on load: " + string.Join("\n", errors));
#endif
#if PRERELEASE_MOTHBALL
      s_TheSession.World.DoForAllMaps(m=>m.RegenerateChokepoints());
#endif
	  Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
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
        case GameMode.GM_STANDARD: return "STD - Standard Game";
        case GameMode.GM_CORPSES_INFECTION: return "C&I - Corpses & Infection";
        case GameMode.GM_VINTAGE: return "VTG - Vintage Zombies";
        case GameMode.GM_WORLD_WAR_Z: return "WWZ - World War Z";
        default: throw new ArgumentOutOfRangeException(nameof(mode),(int)mode,"unhandled game mode");
      }
    }

    public static string DescShortGameMode(GameMode mode)
    {
      switch (mode) {
        case GameMode.GM_STANDARD: return "STD";
        case GameMode.GM_CORPSES_INFECTION: return "C&I";
        case GameMode.GM_VINTAGE: return "VTG";
        case GameMode.GM_WORLD_WAR_Z: return "WWZ";
        default: throw new ArgumentOutOfRangeException(nameof(mode),(int)mode,"unhandled game mode");
      }
    }

    public bool HasImmediateZombification {
      get { return GameMode.GM_STANDARD == GameMode || GameMode.GM_WORLD_WAR_Z == GameMode; }
    }

    public bool HasInfection {
      get { return GameMode.GM_STANDARD != GameMode && GameMode.GM_WORLD_WAR_Z != GameMode; }
    }

    public bool HasCorpses {
      get { return GameMode.GM_STANDARD != GameMode; }
    }

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
      FORMAT_BIN,
      FORMAT_SOAP, // dropped from .NET Core
      FORMAT_XML,  // incompatible with access control
    }
  }
}
