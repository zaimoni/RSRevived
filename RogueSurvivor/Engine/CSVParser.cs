// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVParser
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine
{
  public class CSVParser
  {
    private readonly char m_Delimiter;

    public char Delimiter {
      get {
        return m_Delimiter;
      }
    }

    public CSVParser()
    {
      m_Delimiter = ',';
    }

    public string[] Parse(string line)
    {
      Contract.Ensures(null != Contract.Result<string[]>());
      if (line == null) return new string[0];
      line = line.TrimEnd();
      List<string> stringList = new List<string>((IEnumerable<string>) line.Split(m_Delimiter));
      int index1 = 0;
      do {
        string str1 = stringList[index1];
        if ((int) str1[0] == 34 && (int) str1[str1.Length - 1] != 34) {
          string str2 = str1;
          int index2 = index1 + 1;
          while (index2 < stringList.Count) {
            string str3 = stringList[index2];
            str2 = str2 + "," + str3;
            stringList.RemoveAt(index2);
            if ((int) str3[str3.Length - 1] == 34) break;
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
      Contract.Ensures(null != Contract.Result<List<string[]>>());
      List<string[]> strArrayList = new List<string[]>(1);
      if (lines == null) return strArrayList;
      foreach (string line in lines)
        strArrayList.Add(Parse(line));
      return strArrayList;
    }

    public CSVTable ParseToTable(string[] lines, int nbFields)
    {
      CSVTable csvTable = new CSVTable(nbFields);
      foreach (string[] strArray in Parse(lines)) {
        CSVLine line = new CSVLine(strArray.Length);
        for (int index = 0; index < line.FieldsCount; ++index)
          line[index] = new CSVField(strArray[index]);
        csvTable.AddLine(line);
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
