// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.TextFile
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.IO;

namespace djack.RogueSurvivor.Engine
{
  internal class TextFile
  {
    private List<string> m_RawLines;
    private List<string> m_FormatedLines;

    public IEnumerable<string> RawLines
    {
      get
      {
        return (IEnumerable<string>)m_RawLines;
      }
    }

    public List<string> FormatedLines
    {
      get
      {
        return m_FormatedLines;
      }
    }

    public TextFile()
    {
            m_RawLines = new List<string>();
    }

    public bool Load(string fileName)
    {
      try
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("Loading text file {0}...", (object) fileName));
        StreamReader streamReader = File.OpenText(fileName);
                m_RawLines = new List<string>();
        while (!streamReader.EndOfStream)
                    m_RawLines.Add(streamReader.ReadLine());
        streamReader.Close();
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "done!");
        return true;
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("Loading exception: {0}", (object) ex.ToString()));
        return false;
      }
    }

    public bool Save(string fileName)
    {
      try
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("Saving text file {0}...", (object) fileName));
        File.WriteAllLines(fileName, m_RawLines.ToArray());
        Logger.WriteLine(Logger.Stage.RUN_MAIN, "done!");
        return true;
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_MAIN, string.Format("Saving exception: {0}", (object) ex.ToString()));
        return false;
      }
    }

    public void Append(string line)
    {
            m_RawLines.Add(line);
    }

    public void FormatLines(int charsPerLine)
    {
      if (m_RawLines == null || m_RawLines.Count == 0)
        return;
            m_FormatedLines = new List<string>(m_RawLines.Count);
      for (int index = 0; index < m_RawLines.Count; ++index)
      {
        string str1;
        string str2;
        for (str1 = m_RawLines[index]; str1.Length > charsPerLine; str1 = str2)
        {
          string str3 = str1.Substring(0, charsPerLine);
          str2 = str1.Remove(0, charsPerLine);
                    m_FormatedLines.Add(str3);
        }
                m_FormatedLines.Add(str1);
      }
    }
  }
}
