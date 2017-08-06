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
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Session : ISerializable
  {
    public static int COMMAND_LINE_SEED = 0;
    public static Dictionary<string, string> CommandLineOptions = new Dictionary<string, string>();
    private static Session s_TheSession = null;

    private readonly WorldTime m_WorldTime = new WorldTime();
    private Scoring m_Scoring;
    private int[,,] m_Event_Raids;
    private readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, string> m_CommandLineOptions;    // needs .NET 4.6 or higher
    private readonly Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> m_PoliceItemMemory = new Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>();
    private readonly ThreatTracking m_PoliceThreatTracking = new ThreatTracking();
    private readonly LocationSet m_PoliceInvestigate = new LocationSet();

    public int Seed { get; private set; }
    public World World { get; private set; }
    public UniqueActors UniqueActors { get; private set; }
    public UniqueItems UniqueItems { get; private set; }
    public UniqueMaps UniqueMaps { get; private set; }

    public GameMode GameMode;
    public int LastTurnPlayerActed;
    public Map CurrentMap;
    public bool PlayerKnows_CHARUndergroundFacilityLocation;
    public bool PlayerKnows_TheSewersThingLocation;
    public bool CHARUndergroundFacility_Activated;
    public int ScriptStage_PoliceStationPrisoner;

    public static Session Get {
      get {
        Contract.Ensures(null!=Contract.Result<Session>());
        if (s_TheSession == null) s_TheSession = new Session();
        return s_TheSession;
      }
    }

    public WorldTime WorldTime { get { return m_WorldTime; } }
    public Scoring Scoring { get { return m_Scoring; } }
    public Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> PoliceItemMemory { get { return m_PoliceItemMemory; } }
    public ThreatTracking PoliceThreatTracking { get { return m_PoliceThreatTracking; } }
    public LocationSet PoliceInvestigate { get { return m_PoliceInvestigate; } }

    private Session()
    {
      m_CommandLineOptions = (null == Session.CommandLineOptions || 0 >= Session.CommandLineOptions.Count ? null : new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>(Session.CommandLineOptions)));
      Reset();
    }

#region Implement ISerializable
    // general idea is Plain Old Data before objects.
    protected Session(SerializationInfo info, StreamingContext context)
    {
      m_WorldTime = (WorldTime) info.GetValue("WorldTime",typeof(WorldTime));
      m_Scoring = (Scoring) info.GetValue("Scoring",typeof(Scoring));
      m_Event_Raids = (int[,,]) info.GetValue("Event_Raids",typeof(int[,,]));
      GameMode = (GameMode) info.GetSByte("GameMode");
      ScriptStage_PoliceStationPrisoner = (int) info.GetSByte("ScriptStage_PoliceStationPrisoner");
      Seed = info.GetInt32("Seed");
      LastTurnPlayerActed = info.GetInt32("LastTurnPlayerActed");
      PlayerKnows_CHARUndergroundFacilityLocation = info.GetBoolean("PlayerKnows_CHARUndergroundFacilityLocation");
      PlayerKnows_TheSewersThingLocation = info.GetBoolean("PlayerKnows_TheSewersThingLocation");
      CHARUndergroundFacility_Activated = info.GetBoolean("CHARUndergroundFacility_Activated");
      m_CommandLineOptions = (System.Collections.ObjectModel.ReadOnlyDictionary<string, string>) info.GetValue("CommandLineOptions", typeof(System.Collections.ObjectModel.ReadOnlyDictionary<string, string>));
      World = (World) info.GetValue("World",typeof(World));
      CurrentMap = (Map) info.GetValue("CurrentMap",typeof(Map));
      UniqueActors = (UniqueActors) info.GetValue("UniqueActors",typeof(UniqueActors));
      UniqueItems = (UniqueItems) info.GetValue("UniqueItems",typeof(UniqueItems));
      UniqueMaps = (UniqueMaps) info.GetValue("UniqueMaps",typeof(UniqueMaps));
      m_PoliceItemMemory = (Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>) info.GetValue("m_PoliceItemMemory", typeof(Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>));
      m_PoliceThreatTracking = (ThreatTracking) info.GetValue("m_PoliceThreatTracking", typeof(ThreatTracking));
      m_PoliceInvestigate = (LocationSet) info.GetValue("m_PoliceInvestigate", typeof(LocationSet));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("WorldTime",m_WorldTime,typeof(WorldTime));
      info.AddValue("Scoring",m_Scoring,typeof(Scoring));
      info.AddValue("Event_Raids",m_Event_Raids,typeof(int[,,]));

      info.AddValue("GameMode",(SByte)GameMode);
      info.AddValue("ScriptStage_PoliceStationPrisoner",(SByte)ScriptStage_PoliceStationPrisoner);
      info.AddValue("Seed",Seed);
      info.AddValue("LastTurnPlayerActed",LastTurnPlayerActed);
      info.AddValue("PlayerKnows_CHARUndergroundFacilityLocation",PlayerKnows_CHARUndergroundFacilityLocation);
      info.AddValue("PlayerKnows_TheSewersThingLocation",PlayerKnows_TheSewersThingLocation);
      info.AddValue("CHARUndergroundFacility_Activated",CHARUndergroundFacility_Activated);
      info.AddValue("CommandLineOptions", m_CommandLineOptions,typeof(System.Collections.ObjectModel.ReadOnlyDictionary<string, string>));
      info.AddValue("World",World,typeof(World));
      info.AddValue("CurrentMap",CurrentMap,typeof(Map));
      info.AddValue("UniqueActors",UniqueActors,typeof(UniqueActors));
      info.AddValue("UniqueItems",UniqueItems,typeof(UniqueItems));
      info.AddValue("UniqueMaps",UniqueMaps,typeof(UniqueMaps));
      info.AddValue("m_PoliceItemMemory", m_PoliceItemMemory, typeof(Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>));
      info.AddValue("m_PoliceThreatTracking", m_PoliceThreatTracking, typeof(ThreatTracking));
      info.AddValue("m_PoliceInvestigate", m_PoliceInvestigate, typeof(LocationSet));
    }
#endregion

    public void Reset()
    {
      Seed = (0 == COMMAND_LINE_SEED ? (int) DateTime.UtcNow.TimeOfDay.Ticks : COMMAND_LINE_SEED);
      CurrentMap = null;
      m_Scoring = new Scoring();
      World = new World(RogueGame.Options.CitySize);
      m_WorldTime.TurnCounter = 0;
      LastTurnPlayerActed = 0;
      m_Event_Raids = new int[(int) RaidType._COUNT, RogueGame.Options.CitySize, RogueGame.Options.CitySize];
      for (int index1 = 0; index1 < (int)RaidType._COUNT; ++index1) {
        for (int index2 = 0; index2 < RogueGame.Options.CitySize; ++index2) {
          for (int index3 = 0; index3 < RogueGame.Options.CitySize; ++index3)
            m_Event_Raids[index1, index2, index3] = -1;
        }
      }
      CHARUndergroundFacility_Activated = false;
      PlayerKnows_CHARUndergroundFacilityLocation = false;
      PlayerKnows_TheSewersThingLocation = false;
      ScriptStage_PoliceStationPrisoner = 0;
      UniqueActors = new UniqueActors();
      UniqueItems = new UniqueItems();
      UniqueMaps = new UniqueMaps();
      m_PoliceItemMemory.Clear();
      m_PoliceThreatTracking.Clear();
    }

    public bool CMDoptionExists(string x) {
      if (null == m_CommandLineOptions) return false;
      return m_CommandLineOptions.ContainsKey(x);
    }

    public void ForcePoliceKnown(Location loc) {   // for world creation
      Inventory tmp = loc.Items;
      HashSet<Gameplay.GameItems.IDs> seen_items = (null == tmp ? null : new HashSet<Gameplay.GameItems.IDs>(tmp.Items.Select(x => x.Model.ID)));
      m_PoliceItemMemory.Set(loc, seen_items, 0);
    }

    // to eventually be obsoleted by an event
    public void PoliceTrackingThroughExitSpawn(Actor a) {
      if (a.Faction.IsEnemyOf(Models.Factions[(int) Gameplay.GameFactions.IDs.ThePolice]) || m_PoliceThreatTracking.IsThreat(a)) {
        m_PoliceThreatTracking.RecordTaint(a,a.Location);
      }
    }

    public bool HasRaidHappened(RaidType raid, District district)
    {
      Contract.Requires(null != district);
      return m_Event_Raids[(int) raid, district.WorldPosition.X, district.WorldPosition.Y] > -1;
    }

    public int LastRaidTime(RaidType raid, District district)
    {
      Contract.Requires(null != district);
      return m_Event_Raids[(int) raid, district.WorldPosition.X, district.WorldPosition.Y];
    }

    public void SetLastRaidTime(RaidType raid, District district, int turnCounter)
    {
      Contract.Requires(null != district);
      m_Event_Raids[(int) raid, district.WorldPosition.X, district.WorldPosition.Y] = turnCounter;
    }

    public static void Save(Session session, string filepath, Session.SaveFormat format)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      session.World.OptimizeBeforeSaving();
      switch (format)
      {
        case Session.SaveFormat.FORMAT_BIN:
          Session.SaveBin(session, filepath);
          break;
        case Session.SaveFormat.FORMAT_SOAP:
          Session.SaveSoap(session, filepath);
          break;
        case Session.SaveFormat.FORMAT_XML:
          Session.SaveXml(session, filepath);
          break;
      }
    }

    public static bool Load(string filepath, Session.SaveFormat format)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      switch (format)
      {
        case Session.SaveFormat.FORMAT_BIN:
          return Session.LoadBin(filepath);
        case Session.SaveFormat.FORMAT_SOAP:
          return Session.LoadSoap(filepath);
        case Session.SaveFormat.FORMAT_XML:
          return Session.LoadXml(filepath);
        default:
          return false;
      }
    }

    private static void SaveBin(Session session, string filepath)
    {
      Contract.Requires(null != session);
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
	  filepath.BinarySerialize(session);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadBin(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
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
	  Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
      return true;
    }

    private static void SaveSoap(Session session, string filepath)
    {
      Contract.Requires(null != session);
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
	  filepath.BinarySerialize(session);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadSoap(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
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

    private static void SaveXml(Session session, string filepath)
    {
      Contract.Requires(null != session);
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
      using (Stream stream = filepath.CreateStream(true)) {
        new XmlSerializer(typeof (Session)).Serialize(stream, (object) session);
        stream.Flush();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadXml(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
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

    public static bool Delete(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
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

    private static IFormatter CreateSoapFormatter()
    {
      return (IFormatter) new SoapFormatter();
    }

    // game mode support
    public static string DescGameMode(GameMode mode)
    {
      switch (mode)
      {
        case GameMode.GM_STANDARD:
          return "STD - Standard Game";
        case GameMode.GM_CORPSES_INFECTION:
          return "C&I - Corpses & Infection";
        case GameMode.GM_VINTAGE:
          return "VTG - Vintage Zombies";
        default:
          throw new ArgumentOutOfRangeException("mode",(int)mode,"unhandled game mode");
      }
    }

    public static string DescShortGameMode(GameMode mode)
    {
      switch (mode)
      {
        case GameMode.GM_STANDARD:
          return "STD";
        case GameMode.GM_CORPSES_INFECTION:
          return "C&I";
        case GameMode.GM_VINTAGE:
          return "VTG";
        default:
          throw new ArgumentOutOfRangeException("mode",(int)mode,"unhandled game mode");
      }
    }

    public bool HasImmediateZombification {
      get { return GameMode.GM_STANDARD == GameMode; }
    }

    public bool HasInfection {
      get { return GameMode.GM_STANDARD != GameMode; }
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
