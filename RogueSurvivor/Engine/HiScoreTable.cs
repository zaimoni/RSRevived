// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.HiScoreTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class HiScoreTable
  {
    public const int DEFAULT_MAX_ENTRIES = 12;
    private readonly List<HiScore> m_Table;
    private readonly int m_MaxEntries;

    public int Count { get { return m_Table.Count; } }

    public HiScore this[int index]
    {
      get
      {
        return Get(index);
      }
    }

    public HiScoreTable(int maxEntries)
    {
	  Contract.Requires(0 < maxEntries);
      m_Table = new List<HiScore>(maxEntries);
      m_MaxEntries = maxEntries;
    }

    public void Clear()
    {
	  m_Table.Clear();
      for (int index = 0; index < m_MaxEntries; ++index)
        m_Table.Add(new HiScore()
        {
          Death = "no death",
          DifficultyPercent = 0,
          KillPoints = 0,
          Name = "no one",
          PlayingTime = TimeSpan.Zero,
          SurvivalPoints = 0,
          TotalPoints = 0,
          TurnSurvived = 0,
          SkillsDescription = "no skills"
        });
    }

    public bool Register(HiScore hi)
    {
      int index = 0;
      while (index < m_Table.Count && m_Table[index].TotalPoints >= hi.TotalPoints)
        ++index;
      if (index > m_MaxEntries) return false;
      m_Table.Insert(index, hi);
      while (m_Table.Count > m_MaxEntries)
        m_Table.RemoveAt(m_Table.Count - 1);
      return true;
    }

    public HiScore Get(int index)
    {
	  Contract.Requires(0 <= index && index < Count);
      return m_Table[index];
    }

    public static void Save(HiScoreTable table, string filepath)
    {
	  Contract.Requires(null != table);
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hiscore table...");
	  filepath.BinarySerialize(table);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hiscore table... done!");
    }

    public static HiScoreTable Load(string filepath)
    {
	  Contract.Requires(!string.IsNullOrEmpty(filepath));
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hiscore table...");
      HiScoreTable hiScoreTable;
      try {
	    hiScoreTable = filepath.BinaryDeserialize<HiScoreTable>();
      } catch (Exception ex) {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load hiscore table (no hiscores?).");
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("load exception : {0}.", (object) ex.ToString()));
        return null;
      }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hiscore table... done!");
      return hiScoreTable;
    }
  }
}
