﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.GameHintsStatus
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class GameHintsStatus
  {
    private readonly bool[] m_AdvisorHints = new bool[(int) AdvisorHint._COUNT];

    public void ResetAllHints()
    {
      for (int index = 0; index < (int)AdvisorHint._COUNT; ++index)
        m_AdvisorHints[index] = false;
    }

    public bool IsAdvisorHintGiven(AdvisorHint hint)
    {
      return m_AdvisorHints[(int) hint];
    }

    public void SetAdvisorHintAsGiven(AdvisorHint hint)
    {
      m_AdvisorHints[(int) hint] = true;
    }

    public int CountAdvisorHintsGiven()
    {
      int num = 0;
      for (int index = 0; index < (int)AdvisorHint._COUNT; ++index) {
        if (m_AdvisorHints[index]) ++num;
      }
      return num;
    }

    public bool HasAdvisorGivenAllHints()
    {
      return CountAdvisorHintsGiven() >= (int)AdvisorHint._COUNT;
    }

    public static void Save(GameHintsStatus hints, string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hints...");
      IFormatter formatter = GameHintsStatus.CreateFormatter();
      Stream stream = GameHintsStatus.CreateStream(filepath, true);
      formatter.Serialize(stream, (object) hints);
      stream.Flush();
      stream.Close();
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hints... done!");
    }

    public static GameHintsStatus Load(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hints...");
      GameHintsStatus gameHintsStatus;
      try {
        IFormatter formatter = GameHintsStatus.CreateFormatter();
        Stream stream = GameHintsStatus.CreateStream(filepath, false);
        gameHintsStatus = (GameHintsStatus) formatter.Deserialize(stream);
        stream.Close();
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load hints (first run?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "resetting.");
        gameHintsStatus = new GameHintsStatus();
        gameHintsStatus.ResetAllHints();
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading options... done!");
      return gameHintsStatus;
    }

    private static IFormatter CreateFormatter()
    {
      return new BinaryFormatter();
    }

    private static Stream CreateStream(string saveFileName, bool save)
    {
      return new FileStream(saveFileName, save ? FileMode.Create : FileMode.Open, save ? FileAccess.Write : FileAccess.Read, FileShare.None);
    }
  }
}
