// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine
{
  public class CSVTable
  {
    private readonly int m_nbFields;
    private readonly List<CSVLine> m_Lines;

    public CSVField this[int field, int line] {
      get {
#if DEBUG
        if (0 > field) throw new InvalidOperationException("0 > field");
        if (0 > line) throw new InvalidOperationException("0 > line");
        if (m_Lines.Count <= line) throw new InvalidOperationException("m_Lines.Count <= line");
        if (null == m_Lines[line][field]) throw new ArgumentNullException("m_Lines[line][field]");
#endif
        return m_Lines[line][field];
      }
    }

#if DEAD_FUNC
    public IEnumerable<CSVLine> Lines { get { return m_Lines; } }
    public int CountLines { get { return m_Lines.Count; } }
#endif

    public CSVTable(int nbFields)
    {
      m_nbFields = nbFields;
      m_Lines = new List<CSVLine>();
    }

    public void AddLine(CSVLine line)
    {
#if DEBUG
      if (null == line) throw new ArgumentNullException(nameof(line));
#endif
      if (line.FieldsCount != m_nbFields)
        throw new ArgumentException(string.Format("line fields count {0} does not match with table fields count {1}", line.FieldsCount, m_nbFields));
      m_Lines.Add(line);
    }

    public CSVLine FindLineFor<_T_>(_T_ modelID)
    {
      foreach (CSVLine line in m_Lines) {
        if (line[0].ParseText() == modelID.ToString()) return line;
      }
      throw new InvalidOperationException(string.Format("{0} {1} not found", typeof(_T_).ToString(), modelID.ToString()));
    }

    public _DATA_TYPE_ GetDataFor<_DATA_TYPE_,_T_>(Func<CSVLine, _DATA_TYPE_> fn, _T_ modelID)
    {
#if DEBUG
      if (null == fn) throw new ArgumentNullException(nameof(fn));
#endif
      CSVLine lineForModel = FindLineFor(modelID);
      try {
        return fn(lineForModel);
      } catch (Exception ex) {
        throw new InvalidOperationException(string.Format("invalid data format for {0} {1}; exception : {2}", typeof(_T_).ToString(), modelID.ToString(), ex.ToString()));
      }
    }
  }
}
