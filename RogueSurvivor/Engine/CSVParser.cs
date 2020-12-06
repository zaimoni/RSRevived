﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVParser
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Collections.Generic;
using System.Text;

namespace djack.RogueSurvivor.Engine
{
  public class CSVParser
  {
    private readonly char m_Delimiter;

    public char Delimiter { get { return m_Delimiter; } }

    public CSVParser()
    {
      m_Delimiter = ',';
    }

    public string[] Parse(string line)
    {
      if (line == null) return new string[0];
      line = line.TrimEnd();
      List<string> stringList = new List<string>(line.Split(m_Delimiter));
      int index1 = 0;
      do {
        string str1 = stringList[index1];
        if ('"' == str1[0] && '"' != str1[^1]) {
          string str2 = str1;
          int index2 = index1 + 1;
          while (index2 < stringList.Count) {
            string str3 = stringList[index2];
            str2 = str2 + "," + str3;
            stringList.RemoveAt(index2);
            if ('"' == str3[^1]) break;
          }
          stringList[index1] = str2;
        } else
          ++index1;
      }
      while (index1 < stringList.Count - 1);
      return stringList.ToArray();
    }

    public List<string[]> Parse(string[] lines)
    {
      int ub = (null == lines) ? 0 : lines.Length;
      if (0 >= ub) return new List<string[]>();
      List<string[]> strArrayList = new List<string[]>(ub);
      foreach (string line in lines)
        strArrayList.Add(Parse(line));
      return strArrayList;
    }

    public CSVTable ParseToTable(string[] lines, int nbFields)
    {
      CSVTable csvTable = new CSVTable(nbFields);
      foreach (string[] strArray in Parse(lines)) {
        csvTable.AddLine(new CSVLine(strArray));
      }
      return csvTable;
    }

    public string Format(string[] fields)
    {
      if (fields == null) return string.Format("{0}", (object)m_Delimiter);
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string field in fields) {
        stringBuilder.Append(field);
        stringBuilder.Append(m_Delimiter);
      }
      return stringBuilder.ToString();
    }
  }
}
