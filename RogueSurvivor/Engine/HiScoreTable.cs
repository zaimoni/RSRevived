// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.HiScoreTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class HiScoreTable
  {
    private const int DEFAULT_MAX_ENTRIES = 12;
    private readonly List<HiScore> m_Table;
    private readonly int m_MaxEntries;

    public int Count { get { return m_Table.Count; } }

    public HiScore this[int index] {
      get {
        return Get(index);
      }
    }

    public HiScoreTable(int maxEntries = DEFAULT_MAX_ENTRIES)
    {
#if DEBUG
      if (0 >= maxEntries) throw new InvalidOperationException("0 >= maxEntries");
#endif
      m_Table = new List<HiScore>(maxEntries);
      m_MaxEntries = maxEntries;
    }

    public void Clear()
    {
	  m_Table.Clear();
      for (int index = 0; index < m_MaxEntries; ++index)
        m_Table.Add(new HiScore());
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
#if DEBUG
      if (0 > index || Count <= index) throw new InvalidOperationException("0 > index || Count <= index");
#endif
      return m_Table[index];
    }

    public static void Save(HiScoreTable table, string filepath)
    {
#if DEBUG
      if (null == table) throw new ArgumentNullException(nameof(table));
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hiscore table...");
	  filepath.BinarySerialize(table);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hiscore table... done!");
    }

    public static HiScoreTable Load(string filepath)
    {
#if DEBUG
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
#endif
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hiscore table...");
      HiScoreTable hiScoreTable;
      try {
#if LINUX
        filepath = filepath.Replace("\\", "/");
#endif
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
