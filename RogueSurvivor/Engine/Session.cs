// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Session
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
#if OBSOLETE
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;
#endif
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Session : ISerializable
  {
    public static int COMMAND_LINE_SEED;
    public static readonly Dictionary<string, string> CommandLineOptions = new Dictionary<string, string>();
    private static Session s_TheSession;

    [NonSerialized] private Scoring_fatality m_Scoring_fatality = null;
    private Scoring m_Scoring;
    private int[,,] m_Event_Raids;
    private readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, string> m_CommandLineOptions;    // needs .NET 4.6 or higher
    public readonly RadioFaction Police = new RadioFaction(Gameplay.GameFactions.IDs.ThePolice, Gameplay.GameItems.IDs.TRACKER_POLICE_RADIO);

    [NonSerialized] private static int s_seed = 0;  // We're a compiler-enforced singleton so this only looks weird

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

    public static Session Get { get { return s_TheSession ??= new Session(); } }

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
      info.read(ref m_Event_Raids, "Event_Raids");
      GameMode = (GameMode) info.GetSByte("GameMode");
      ScriptStage_PoliceStationPrisoner = (int) info.GetSByte("ScriptStage_PoliceStationPrisoner");
      ScriptStage_PoliceCHARrelations = (int) info.GetSByte("ScriptStage_PoliceCHARrelations");
      ScriptStage_HospitalPowerup = (int) info.GetSByte("ScriptStage_HospitalPowerup");
      s_seed = info.GetInt32("Seed");
      LastTurnPlayerActed = info.GetInt32("LastTurnPlayerActed");
      PlayerKnows_CHARUndergroundFacilityLocation = info.GetBoolean("PlayerKnows_CHARUndergroundFacilityLocation");
      info.read_nullsafe(ref m_CommandLineOptions, "CommandLineOptions");
      ActorModel.Load(info,context);
      Actor.Load(info,context);
      Rules.Get.Load(info,context);
      PlayerController.Load(info,context);
      World = (World) info.GetValue("World",typeof(World));
      RogueGame.Load(info, context);
      UniqueActors = (UniqueActors) info.GetValue("UniqueActors",typeof(UniqueActors));
      UniqueItems = (UniqueItems) info.GetValue("UniqueItems",typeof(UniqueItems));
      UniqueMaps = (UniqueMaps) info.GetValue("UniqueMaps",typeof(UniqueMaps));
      info.read(ref Police, "Police");
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Scoring",m_Scoring,typeof(Scoring));
      info.AddValue("Event_Raids",m_Event_Raids,typeof(int[,,]));

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

    [OnDeserialized] private void OnDeserialized(StreamingContext context)
    {
      RogueGame.AfterLoad();
    }
#endregion

    public void Reset()
    {
      s_seed = (0 == COMMAND_LINE_SEED ? (int) DateTime.UtcNow.TimeOfDay.Ticks : COMMAND_LINE_SEED);
#if DEBUG
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "Seed: "+s_seed.ToString());
#endif
      RogueGame.Reset();
      m_Scoring = new Scoring();
      var city_size = RogueGame.Options.CitySize;
      World = new World(city_size);
      LastTurnPlayerActed = 0;
      m_Event_Raids = new int[(int) RaidType._COUNT, city_size, city_size];
      for (int index1 = 0; index1 < (int)RaidType._COUNT; ++index1) {
        for (int index2 = 0; index2 < city_size; ++index2) {
          for (int index3 = 0; index3 < city_size; ++index3)
            m_Event_Raids[index1, index2, index3] = -1;
        }
      }
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
        foreach(var inv in allItems) seen_items.UnionWith(inv.Items.Select(x => x.Model.ID));
        Police.ItemMemory.Set(loc, seen_items, 0);
      }
    }

    // we have conflicting implementation imperatives here.
    // access control wants m_Event_Raids to be a private static member of District.  However, it is not 
    // a natural singleton (one per savegame) so it probably belongs with the World object.
    // At that point, keeping it in Session eliminates a use of the Load/Save helper idiom.
#nullable enable
    public bool HasRaidHappened(RaidType raid, District district)
    {
      var w_pos = district.WorldPosition;
      return m_Event_Raids[(int) raid, w_pos.X, w_pos.Y] > -1;
    }

    public int LastRaidTime(RaidType raid, District district)
    {
      var w_pos = district.WorldPosition;
      return m_Event_Raids[(int) raid, w_pos.X, w_pos.Y];
    }

    public void SetLastRaidTime(RaidType raid, Map map)
    {
      var w_pos = map.District.WorldPosition;
      m_Event_Raids[(int) raid, w_pos.X, w_pos.Y] = map.LocalTime.TurnCounter;
    }
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
#if LINUX
      filename = filename.Replace("\\", "/");
#endif
      switch (format) {
        case SaveFormat.FORMAT_BIN:
          SaveBin(session, filepath);
          break;
#if NOT_NET_CORE
        case SaveFormat.FORMAT_SOAP:
          SaveSoap(session, filepath);
          break;
#endif
#if OBSOLETE
        case SaveFormat.FORMAT_XML:
          SaveXml(session, filepath);
          break;
#endif
      }
    }

    public static bool Load(string filepath, SaveFormat format)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
#if LINUX
      filepath = filepath.Replace("\\", "/");
#endif
      switch (format) {
        case SaveFormat.FORMAT_BIN: return LoadBin(filepath);
#if NOT_NET_CORE
        case SaveFormat.FORMAT_SOAP: return LoadSoap(filepath);
#endif
#if OBSOLETE
        case SaveFormat.FORMAT_XML: return LoadXml(filepath);
#endif
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
        s_TheSession = filepath.BinaryDeserialize<Session>();
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

#if NOT_NET_CORE
    private static void SaveSoap(Session session, string filepath)
    {
#if DEBUG
      if (null == session) throw new ArgumentNullException(nameof(session));
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
	  filepath.BinarySerialize(session);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadSoap(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");
      try {
        using (Stream stream = filepath.CreateStream(false)) {
          s_TheSession = (Session) Session.CreateSoapFormatter().Deserialize(stream);
        }
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        return false;
      }
     Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
     return true;
    }
#endif

#if OBSOLETE
    private static void SaveXml(Session session, string filepath)
    {
#if DEBUG
      if (null == session) throw new ArgumentNullException(nameof(session));
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
      using (Stream stream = filepath.CreateStream(true)) {
        new XmlSerializer(typeof (Session)).Serialize(stream, (object) session);
        stream.Flush();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadXml(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");
      try {
        using (Stream stream = filepath.CreateStream(false)) {
          s_TheSession = (Session) new XmlSerializer(typeof (Session)).Deserialize(stream);
          stream.Flush();
        }
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        return false;
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
      return true;
    }
#endif

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

#if NOT_NET_CORE
    private static IFormatter CreateSoapFormatter()
    {
      return new SoapFormatter();
    }
#endif

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
      FORMAT_SOAP,
      FORMAT_XML,
    }
  }
}
