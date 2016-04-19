// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Session
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class Session
  {
    private GameMode m_GameMode;
    private WorldTime m_WorldTime;
    private World m_World;
    private Map m_CurrentMap;
    private Scoring m_Scoring;
    private int[,,] m_Event_Raids;
    [NonSerialized]
    private static Session s_TheSession;

    public static Session Get
    {
      get
      {
        if (Session.s_TheSession == null)
          Session.s_TheSession = new Session();
        return Session.s_TheSession;
      }
    }

    public GameMode GameMode
    {
      get
      {
        return this.m_GameMode;
      }
      set
      {
        this.m_GameMode = value;
      }
    }

    public int Seed { get; set; }

    public WorldTime WorldTime
    {
      get
      {
        return this.m_WorldTime;
      }
    }

    public int LastTurnPlayerActed { get; set; }

    public World World
    {
      get
      {
        return this.m_World;
      }
      set
      {
        this.m_World = value;
      }
    }

    public Map CurrentMap
    {
      get
      {
        return this.m_CurrentMap;
      }
      set
      {
        this.m_CurrentMap = value;
      }
    }

    public Scoring Scoring
    {
      get
      {
        return this.m_Scoring;
      }
    }

    public UniqueActors UniqueActors { get; set; }

    public UniqueItems UniqueItems { get; set; }

    public UniqueMaps UniqueMaps { get; set; }

    public bool PlayerKnows_CHARUndergroundFacilityLocation { get; set; }

    public bool PlayerKnows_TheSewersThingLocation { get; set; }

    public bool CHARUndergroundFacility_Activated { get; set; }

    public ScriptStage ScriptStage_PoliceStationPrisonner { get; set; }

    private Session()
    {
      this.Reset();
    }

    public void Reset()
    {
      this.Seed = (int) DateTime.UtcNow.TimeOfDay.Ticks;
      this.m_CurrentMap = (Map) null;
      this.m_Scoring = new Scoring();
      this.m_World = (World) null;
      this.m_WorldTime = new WorldTime();
      this.LastTurnPlayerActed = 0;
      m_Event_Raids = new int[(int) RaidType._COUNT, RogueGame.Options.CitySize, RogueGame.Options.CitySize];
      for (int index1 = 0; index1 < (int)RaidType._COUNT; ++index1)
      {
        for (int index2 = 0; index2 < RogueGame.Options.CitySize; ++index2)
        {
          for (int index3 = 0; index3 < RogueGame.Options.CitySize; ++index3)
            this.m_Event_Raids[index1, index2, index3] = -1;
        }
      }
      this.CHARUndergroundFacility_Activated = false;
      this.PlayerKnows_CHARUndergroundFacilityLocation = false;
      this.PlayerKnows_TheSewersThingLocation = false;
      this.ScriptStage_PoliceStationPrisonner = ScriptStage.STAGE_0;
      this.UniqueActors = new UniqueActors();
      this.UniqueItems = new UniqueItems();
      this.UniqueMaps = new UniqueMaps();
    }

    public bool HasRaidHappened(RaidType raid, District district)
    {
      if (district == null)
        throw new ArgumentNullException("district");
      return this.m_Event_Raids[(int) raid, district.WorldPosition.X, district.WorldPosition.Y] > -1;
    }

    public int LastRaidTime(RaidType raid, District district)
    {
      if (district == null)
        throw new ArgumentNullException("district");
      return this.m_Event_Raids[(int) raid, district.WorldPosition.X, district.WorldPosition.Y];
    }

    public void SetLastRaidTime(RaidType raid, District district, int turnCounter)
    {
      if (district == null)
        throw new ArgumentNullException("district");
      this.m_Event_Raids[(int) raid, district.WorldPosition.X, district.WorldPosition.Y] = turnCounter;
    }

    public static void Save(Session session, string filepath, Session.SaveFormat format)
    {
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
      if (session == null)
        throw new ArgumentNullException("session");
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
      using (Stream stream = Session.CreateStream(filepath, true))
      {
        Session.CreateFormatter().Serialize(stream, (object) session);
        stream.Flush();
        stream.Close();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadBin(string filepath)
    {
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");
      try
      {
        using (Stream stream = Session.CreateStream(filepath, false))
        {
          Session.s_TheSession = (Session) Session.CreateFormatter().Deserialize(stream);
          stream.Close();
        }
        Session.s_TheSession.ReconstructAuxiliaryFields();
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        Session.s_TheSession = (Session) null;
        return false;
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
      return true;
    }

    private static void SaveSoap(Session session, string filepath)
    {
      if (session == null)
        throw new ArgumentNullException("session");
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
      using (Stream stream = Session.CreateStream(filepath, true))
      {
        Session.CreateSoapFormatter().Serialize(stream, (object) session);
        stream.Flush();
        stream.Close();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadSoap(string filepath)
    {
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");
      try
      {
        using (Stream stream = Session.CreateStream(filepath, false))
        {
          Session.s_TheSession = (Session) Session.CreateSoapFormatter().Deserialize(stream);
          stream.Close();
        }
        Session.s_TheSession.ReconstructAuxiliaryFields();
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        Session.s_TheSession = (Session) null;
        return false;
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
      return true;
    }

    private static void SaveXml(Session session, string filepath)
    {
      if (session == null)
        throw new ArgumentNullException("session");
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");
      using (Stream stream = Session.CreateStream(filepath, true))
      {
        new XmlSerializer(typeof (Session)).Serialize(stream, (object) session);
        stream.Flush();
        stream.Close();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
    }

    private static bool LoadXml(string filepath)
    {
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");
      try
      {
        using (Stream stream = Session.CreateStream(filepath, false))
        {
          Session.s_TheSession = (Session) new XmlSerializer(typeof (Session)).Deserialize(stream);
          stream.Flush();
          stream.Close();
        }
        Session.s_TheSession.ReconstructAuxiliaryFields();
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        Session.s_TheSession = (Session) null;
        return false;
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
      return true;
    }

    public static bool Delete(string filepath)
    {
      if (filepath == null)
        throw new ArgumentNullException("filepath");
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "deleting saved game...");
      bool flag = false;
      try
      {
        File.Delete(filepath);
        flag = true;
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to delete saved game (no save?)");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "exception :");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, ex.ToString());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failing silently.");
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "deleting saved game... done!");
      return flag;
    }

    private static IFormatter CreateFormatter()
    {
      return (IFormatter) new BinaryFormatter();
    }

    private static IFormatter CreateSoapFormatter()
    {
      return (IFormatter) new SoapFormatter();
    }

    private static Stream CreateStream(string saveFileName, bool save)
    {
      return (Stream) new FileStream(saveFileName, save ? FileMode.Create : FileMode.Open, save ? FileAccess.Write : FileAccess.Read, FileShare.None);
    }

    private void ReconstructAuxiliaryFields()
    {
      for (int index1 = 0; index1 < this.m_World.Size; ++index1)
      {
        for (int index2 = 0; index2 < this.m_World.Size; ++index2)
        {
          foreach (Map map in this.m_World[index1, index2].Maps)
            map.ReconstructAuxiliaryFields();
        }
      }
    }

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
          throw new Exception("unhandled game mode");
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
          throw new Exception("unhandled game mode");
      }
    }

    public enum SaveFormat
    {
      FORMAT_BIN,
      FORMAT_SOAP,
      FORMAT_XML,
    }
  }
}
