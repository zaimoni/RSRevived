// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVLine
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  public class CSVLine
  {
    private readonly CSVField[] m_Fields;

    public CSVField this[int field] {
      get {
#if DEBUG
        if (0 > field) throw new InvalidOperationException("0 > field");
        if (null == m_Fields[field]) throw new InvalidOperationException("m_Fields[field]");
#endif
        return m_Fields[field];
      }
      set {
#if DEBUG
        if (0 > field) throw new InvalidOperationException("0 > field");
        if (null == value) throw new InvalidOperationException(nameof(value));
#endif
        m_Fields[field] = value;
      }
    }

    public int FieldsCount { get { return m_Fields.Length; } }

    public CSVLine(int nbFields)
    {
#if DEBUG
      if (1 > nbFields) throw new InvalidOperationException("1 > nbFields");
#endif
      m_Fields = new CSVField[nbFields];
    }
  }
}
